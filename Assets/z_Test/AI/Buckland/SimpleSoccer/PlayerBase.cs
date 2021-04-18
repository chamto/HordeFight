using System;
using System.Collections.Generic;
using UnityEngine;
using Buckland;

namespace Test_SimpleSoccer
{
    public class PlayerBase : MovingEntity
    {

        //this player's role in the team
        protected player_role m_PlayerRole;

        //a pointer to this player's team
        protected SoccerTeam m_pTeam;

        //the steering behaviors
        protected SteeringBehaviors m_pSteering;

        //the region that this player is assigned to.
        protected int m_iHomeRegion;

        //the region this player moves to before kickoff
        protected int m_iDefaultRegion;

        //the distance to the ball (in squared-space). This value is queried 
        //a lot so it's calculated once each time-step and stored here.
        protected float m_dDistSqToBall;


        //the vertex buffer
        protected List<Vector3> m_vecPlayerVB;
        //the buffer for the transformed vertices
        protected List<Vector3> m_vecPlayerVBTrans;


        public PlayerBase(SoccerTeam home_team,
                       int home_region,
                       Vector3 heading,
                       Vector3 velocity,
                       float mass,
                       float max_force,
                       float max_speed,
                       float max_turn_rate,
                       float scale,
                       player_role role) :

                base(home_team.Pitch().GetRegionFromIndex(home_region).Center(),
                 scale * 10.0f,
                 velocity,
                 max_speed,
                 heading,
                 mass,
                 new Vector3(scale, scale, scale),
                 max_turn_rate,
                 max_force)
        {
            AutoList<PlayerBase>.Add(this);

            m_pTeam = home_team;
            m_dDistSqToBall = float.MaxValue;
            m_iHomeRegion = home_region;
            m_iDefaultRegion = home_region;
            m_PlayerRole = role;

            //setup the vertex buffers and calculate the bounding radius
            const int NumPlayerVerts = 4;
            Vector3[] player = new Vector3[NumPlayerVerts]{new Vector3(-3, 8),
                                            new Vector3(3,10),
                                            new Vector3(3,-10),
                                            new Vector3(-3,-8)};

            for (int vtx = 0; vtx < NumPlayerVerts; ++vtx)
            {
                m_vecPlayerVB.Add(player[vtx]);

                //set the bounding radius to the length of the 
                //greatest extent
                if (Math.Abs(player[vtx].x) > m_dBoundingRadius)
                {
                    m_dBoundingRadius = Math.Abs(player[vtx].x);
                }

                if (Math.Abs(player[vtx].y) > m_dBoundingRadius)
                {
                    m_dBoundingRadius = Math.Abs(player[vtx].y);
                }
            }

            //set up the steering behavior class
            m_pSteering = new SteeringBehaviors(this,
                                                m_pTeam.Pitch(),
                                                Ball());

            //a player's start target is its start position (because it's just waiting)
            m_pSteering.SetTarget(home_team.Pitch().GetRegionFromIndex(home_region).Center());
        }
        //virtual ~PlayerBase();

        //------------------------------------------------------------------------
        //
        //binary predicates for std::sort (see CanPassForward/Backward)
        //------------------------------------------------------------------------
        bool SortByDistanceToOpponentsGoal(PlayerBase p1, PlayerBase p2)
        {
            return (p1.DistToOppGoal() < p2.DistToOppGoal());
        }

        bool SortByReversedDistanceToOpponentsGoal(PlayerBase p1, PlayerBase p2)
        {
            return (p1.DistToOppGoal() > p2.DistToOppGoal());
        }

        //returns true if there is an opponent within this player's 
        //comfort zone
        public bool isThreatened()
        {
            //check against all opponents to make sure non are within this
            //player's comfort zone
            //std::vector<PlayerBase*>::const_iterator curOpp;
            //curOpp = Team()->Opponents()->Members().begin();
            //for (curOpp; curOpp != Team()->Opponents()->Members().end(); ++curOpp)
            foreach (PlayerBase curOpp in Team().Opponents().Members())
            {
                //calculate distance to the player. if dist is less than our
                //comfort zone, and the opponent is infront of the player, return true
                if (PositionInFrontOfPlayer((curOpp).Pos()) &&
                   ((Pos() - (curOpp).Pos()).sqrMagnitude < Prm.PlayerComfortZoneSq))
                {
                    return true;
                }

            }// next opp

            return false;
        }

        //rotates the player to face the ball or the player's current target
        public void TrackBall()
        {
            RotateHeadingToFacePosition(Ball().Pos());
        }
        public void TrackTarget()
        {
            SetHeading((Steering().Target() - Pos()).normalized);
        }

        //this messages the player that is closest to the supporting spot to
        //change state to support the attacking player
        public void FindSupport()
        {
            PlayerBase BestSupportPly = null;
            //if there is no support we need to find a suitable player.
            if (Team().SupportingPlayer() == null)
            {
                BestSupportPly = Team().DetermineBestSupportingAttacker();

                Team().SetSupportingPlayer(BestSupportPly);

                MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                        ID(),
                                        Team().SupportingPlayer().ID(),
                                        (int)MessageType.Msg_SupportAttacker,
                                        null);
            }

            BestSupportPly = Team().DetermineBestSupportingAttacker();

            //if the best player available to support the attacker changes, update
            //the pointers and send messages to the relevant players to update their
            //states
            if (null != BestSupportPly && (BestSupportPly != Team().SupportingPlayer()))
            {

                if (null != Team().SupportingPlayer())
                {
                    MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                            ID(),
                                            Team().SupportingPlayer().ID(),
                                            (int)MessageType.Msg_GoHome,
                                            null);
                }



                Team().SetSupportingPlayer(BestSupportPly);

                MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                        ID(),
                                        Team().SupportingPlayer().ID(),
                                        (int)MessageType.Msg_SupportAttacker,
                                        null);
            }
        }

        //returns true if the ball can be grabbed by the goalkeeper
        public bool BallWithinKeeperRange()
        {
            return ((Pos() - Ball().Pos()).sqrMagnitude < Prm.KeeperInBallRangeSq);
        }

        //returns true if the ball is within kicking range
        public bool BallWithinKickingRange()
        {
            return ((Ball().Pos() - Pos()).sqrMagnitude < Prm.PlayerKickingDistanceSq);
        }

        //returns true if a ball comes within range of a receiver
        public bool BallWithinReceivingRange()
        {
            return ((Pos() - Ball().Pos()).sqrMagnitude < Prm.BallWithinReceivingRangeSq);
        }

        //returns true if the player is located within the boundaries 
        //of his home region
        public bool InHomeRegion()
        {
            if (m_PlayerRole == player_role.goal_keeper)
            {
                return Pitch().GetRegionFromIndex(m_iHomeRegion).Inside(Pos(), region_modifier.normal);
            }
            else
            {
                return Pitch().GetRegionFromIndex(m_iHomeRegion).Inside(Pos(), region_modifier.halfsize);
            }
        }

        //returns true if this player is ahead of the attacker
        public bool isAheadOfAttacker()
        {
            return Math.Abs(Pos().x - Team().OpponentsGoal().Center().x) <
                   Mathf.Abs(Team().ControllingPlayer().Pos().x - Team().OpponentsGoal().Center().x);
        }

        //returns true if a player is located at the designated support spot
        //public bool AtSupportSpot();

        //returns true if the player is located at his steering target
        public bool AtTarget()
        {
            return ((Pos() - Steering().Target()).sqrMagnitude < Prm.PlayerInTargetRangeSq);
        }

        //returns true if the player is the closest player in his team to
        //the ball
        public bool isClosestTeamMemberToBall()
        {
            return Team().PlayerClosestToBall() == this;
        }

        //returns true if the point specified by 'position' is located in
        //front of the player
        public bool PositionInFrontOfPlayer(Vector3 position)
        {
            Vector3 ToSubject = position - Pos();

            if (Vector3.Dot(ToSubject, Heading()) > 0)

                return true;

            else

                return false;
        }

        //returns true if the player is the closest player on the pitch to the ball
        public bool isClosestPlayerOnPitchToBall()
        {
            return isClosestTeamMemberToBall() &&
                   (DistSqToBall() < Team().Opponents().ClosestDistToBallSq());
        }

        //returns true if this player is the controlling player
        public bool isControllingPlayer()
        {
            return Team().ControllingPlayer() == this;
        }

        //returns true if the player is located in the designated 'hot region' --
        //the area close to the opponent's goal
        public bool InHotRegion()
        {
            return Math.Abs(Pos().y - Team().OpponentsGoal().Center().y) <
                   Pitch().PlayingArea().magnitude / 3.0f;
        }

        public player_role Role() { return m_PlayerRole; }

        public float DistSqToBall() { return m_dDistSqToBall; }
        public void SetDistSqToBall(float val) { m_dDistSqToBall = val; }

        //calculate distance to opponent's/home goal. Used frequently by the passing
        //methods
        public float DistToOppGoal()
        {
            return Math.Abs(Pos().x - Team().OpponentsGoal().Center().x);
        }
        public float DistToHomeGoal()
        {
            return Math.Abs(Pos().x - Team().HomeGoal().Center().x);
        }

        public void SetDefaultHomeRegion() { m_iHomeRegion = m_iDefaultRegion; }

        public SoccerBall Ball()
        {
            return Team().Pitch().Ball();
        }
        public SoccerPitch Pitch()
        {
            return Team().Pitch();
        }
        public SteeringBehaviors Steering() { return m_pSteering; }
        public Region HomeRegion()
        {
            return Pitch().GetRegionFromIndex(m_iHomeRegion);
        }
        public void SetHomeRegion(int NewRegion) { m_iHomeRegion = NewRegion; }
        public SoccerTeam Team() { return m_pTeam; }

    }
}//end namespace

