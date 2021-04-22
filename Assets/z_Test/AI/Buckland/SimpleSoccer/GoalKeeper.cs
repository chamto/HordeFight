using UnityEngine;
using Buckland;
using UtilGS9;

namespace Test_SimpleSoccer
{

    public class GoalKeeper : PlayerBase
    {

        //an instance of the state machine class
        StateMachine<GoalKeeper> m_pStateMachine;

        //this vector is updated to point towards the ball and is used when
        //rendering the goalkeeper (instead of the underlaying vehicle's heading)
        //to ensure he always appears to be watching the ball
        Vector3 m_vLookAt;

        public GoalKeeper(SoccerTeam home_team,
                         int home_region,
                         State<GoalKeeper> start_state,
                         Vector3 heading,
                         Vector3 velocity,
                         float mass,
                         float max_force,
                         float max_speed,
                         float max_turn_rate,
                         float scale) :
                base(home_team, home_region, heading, velocity, mass, max_force, max_speed, max_turn_rate, scale, player_role.goal_keeper)

        {
            m_pStateMachine = new StateMachine<GoalKeeper>(this);

            m_pStateMachine.SetCurrentState(start_state);
            m_pStateMachine.SetPreviousState(start_state);
            m_pStateMachine.SetGlobalState(GlobalKeeperState.instance);

            m_pStateMachine.CurrentState().Enter(this);
        }

        //these must be implemented
        public override void Update()
        {
            //run the logic for the current state
            m_pStateMachine.Update();

            //calculate the combined force from each steering behavior 
            Vector3 SteeringForce = m_pSteering.Calculate();



            //Acceleration = Force/Mass
            Vector3 Acceleration = SteeringForce / m_dMass;

            //update velocity
            m_vVelocity += Acceleration;

            //make sure player does not exceed maximum velocity
            m_vVelocity = VOp.Truncate(m_vVelocity, m_dMaxSpeed);

            //update the position
            m_vPosition += m_vVelocity;


            //enforce a non-penetration constraint if desired
            if (0 != Prm.bNonPenetrationConstraint)
            {
                EntityFunctionTemplates.EnforceNonPenetrationContraint(this, AutoList < PlayerBase >.GetAllMembers());
            }

            //update the heading if the player has a non zero velocity
            if (! Misc.IsZero(m_vVelocity))
            {
                m_vHeading = (m_vVelocity).normalized;

                m_vSide = Vector3.Cross(Vector3.up, m_vHeading);
            }

            //look-at vector always points toward the ball
            if (!Pitch().GoalKeeperHasBall())
            {
                m_vLookAt = (Ball().Pos() - Pos()).normalized;
            }
        }
        public override void Render()
        {
            Color cc;
            if (Team().Color() == SoccerTeam.team_color.blue)
                cc = Color.blue;
            else
                cc = Color.red;

            Vector3 perp = Vector3.Cross(Vector3.up, m_vLookAt);
            Transformations.Draw_WorldTransform(m_vecPlayerVB,
                                                 Pos(),
                                                 m_vLookAt,
                                                 perp,
                                                 Scale(), cc);

            //gdi->ClosedShape(m_vecPlayerVBTrans);

            //draw the head
            //gdi->BrownBrush();
            cc = new Color(165, 42, 42);
            //gdi->Circle(Pos(), 6);
            DebugWide.DrawCircle(Pos(), 6, cc);

            //draw the ID
            if (0 != Prm.ViewIDs)
            {
                //gdi->TextColor(0, 170, 0); ;
                //gdi->TextAtPos(Pos().x - 20, Pos().y - 20, ttos(ID()));

                cc = new Color(0, 170, 0);
                DebugWide.PrintText(new Vector3(Pos().x - 20, 0, Pos().z - 20), cc, ID() + "");
            }

            //draw the state
            if (0 != Prm.ViewStates)
            {
                //gdi->TextColor(0, 170, 0);
                //gdi->TransparentText();
                //gdi->TextAtPos(m_vPosition.x, m_vPosition.y - 20, std::string(m_pStateMachine->GetNameOfCurrentState()));

                cc = new Color(0, 170, 0);
                DebugWide.PrintText(new Vector3(m_vPosition.x, 0, m_vPosition.z - 20), cc, m_pStateMachine.GetNameOfCurrentState());
            }
        }
        public override bool HandleMessage( Telegram msg)
        {
            return m_pStateMachine.HandleMessage(msg);
        }


        //returns true if the ball comes close enough for the keeper to 
        //consider intercepting
        public bool BallWithinRangeForIntercept()
        {
            return ((Team().HomeGoal().Center() - Ball().Pos()).sqrMagnitude <=
                Prm.GoalKeeperInterceptRangeSq);
        }

        //returns true if the keeper has ventured too far away from the goalmouth
        public bool TooFarFromGoalMouth()
        {
          return ((Pos() - GetRearInterposeTarget()).sqrMagnitude >
                  Prm.GoalKeeperInterceptRangeSq);
        }

        //this method is called by the Intercept state to determine the spot
        //along the goalmouth which will act as one of the interpose targets
        //(the other is the ball).
        //the specific point at the goal line that the keeper is trying to cover
        //is flexible and can move depending on where the ball is on the field.
        //To achieve this we just scale the ball's y value by the ratio of the
        //goal width to playingfield width
        public Vector3 GetRearInterposeTarget()
        {
            float xPosTarget = Team().HomeGoal().Center().x;

            float zPosTarget = Pitch().PlayingArea().Center().z -
                           Prm.GoalWidth * 0.5f + (Ball().Pos().z * Prm.GoalWidth) /
                           Pitch().PlayingArea().Height();

            return new Vector3(xPosTarget, 0 ,zPosTarget); 
        }

        public StateMachine<GoalKeeper> GetFSM() {return m_pStateMachine;}


        public Vector3 LookAt() {return m_vLookAt;}
        public void SetLookAt(Vector3 v) { m_vLookAt = v; }
    }
}

