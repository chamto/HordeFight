using System;
using System.Collections.Generic;
using UnityEngine;
using Buckland;
using UtilGS9;

namespace Test_SimpleSoccer
{
    public class SoccerTeam
    {
        public enum team_color { blue, red };

        //an instance of the state machine class
        StateMachine<SoccerTeam> m_pStateMachine;

        //the team must know its own color!
        team_color m_Color;

        //pointers to the team members
        List<PlayerBase> m_Players;

        //a pointer to the soccer pitch
        SoccerPitch m_pPitch;

        //pointers to the goals
        Goal m_pOpponentsGoal;
        Goal m_pHomeGoal;

        //a pointer to the opposing team
        SoccerTeam m_pOpponents;

        //pointers to 'key' players
        PlayerBase m_pControllingPlayer;
        PlayerBase m_pSupportingPlayer;
        PlayerBase m_pReceivingPlayer;
        PlayerBase m_pPlayerClosestToBall;

        //the squared distance the closest player is from the ball
        float m_dDistSqToBallOfClosestPlayer;

        //players use this to determine strategic positions on the playing field
        SupportSpotCalculator m_pSupportSpotCalc;

        //creates all the players for this team
        void CreatePlayers()
        {
            if (m_Color == team_color.blue)
            {
                //goalkeeper
                m_Players.Add(new GoalKeeper(this,
                                           1,
                                           TendGoal.instance,
                                           Vector3.forward,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale));

                //create the players
                m_Players.Add(new FieldPlayer(this,
                                           6,
                                           Wait.instance,
                                           Vector3.forward,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale,
                                           player_role.attacker));



                m_Players.Add(new FieldPlayer(this,
                                       8,
                                       Wait.instance,
                                       Vector3.forward,
                                       Vector3.zero,
                                       Prm.PlayerMass,
                                       Prm.PlayerMaxForce,
                                       Prm.PlayerMaxSpeedWithoutBall,
                                       Prm.PlayerMaxTurnRate,
                                       Prm.PlayerScale,
                                       player_role.attacker));





                m_Players.Add(new FieldPlayer(this,
                                       3,
                                       Wait.instance,
                                       Vector3.forward,
                                       Vector3.zero,
                                       Prm.PlayerMass,
                                       Prm.PlayerMaxForce,
                                       Prm.PlayerMaxSpeedWithoutBall,
                                       Prm.PlayerMaxTurnRate,
                                       Prm.PlayerScale,
                                       player_role.defender));


                m_Players.Add(new FieldPlayer(this,
                                       5,
                                       Wait.instance,
                                       Vector3.forward,
                                       Vector3.zero,
                                       Prm.PlayerMass,
                                       Prm.PlayerMaxForce,
                                       Prm.PlayerMaxSpeedWithoutBall,
                                       Prm.PlayerMaxTurnRate,
                                       Prm.PlayerScale,
                                      player_role.defender));

            }

            else
            {

                //goalkeeper
                m_Players.Add(new GoalKeeper(this,
                                           16,
                                           TendGoal.instance,
                                           Vector3.back,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale));


                //create the players
                m_Players.Add(new FieldPlayer(this,
                                           9,
                                           Wait.instance,
                                           Vector3.back,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale,
                                           player_role.attacker));

                m_Players.Add(new FieldPlayer(this,
                                           11,
                                           Wait.instance,
                                           Vector3.back,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale,
                                           player_role.attacker));



                m_Players.Add(new FieldPlayer(this,
                                           12,
                                           Wait.instance,
                                           Vector3.back,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale,
                                           player_role.defender));


                m_Players.Add(new FieldPlayer(this,
                                           14,
                                           Wait.instance,
                                           Vector3.back,
                                           Vector3.zero,
                                           Prm.PlayerMass,
                                           Prm.PlayerMaxForce,
                                           Prm.PlayerMaxSpeedWithoutBall,
                                           Prm.PlayerMaxTurnRate,
                                           Prm.PlayerScale,
                                           player_role.defender));

            }

            //register the players with the entity manager
            foreach(PlayerBase pb in m_Players)
            {
                EntityManager.Instance().RegisterEntity(pb);
            }
        }

        //called each frame. Sets m_pClosestPlayerToBall to point to the player
        //closest to the ball. 
        void CalculateClosestPlayerToBall()
        {
            float ClosestSoFar = float.MaxValue;

            foreach (PlayerBase it in m_Players)
            {
                //calculate the dist. Use the squared value to avoid sqrt
                float dist = ((it).Pos() - Pitch().Ball().Pos()).sqrMagnitude;

                //keep a record of this value for each player
                (it).SetDistSqToBall(dist);

                if (dist < ClosestSoFar)
                {
                    ClosestSoFar = dist;

                    m_pPlayerClosestToBall = it;
                }
            }

            m_dDistSqToBallOfClosestPlayer = ClosestSoFar;
        }



        public SoccerTeam(Goal home_goal,
                       Goal opponents_goal,
                       SoccerPitch pitch,
                       team_color color)
        {
            m_pOpponentsGoal = opponents_goal;
            m_pHomeGoal = home_goal;
            m_pOpponents = null;
            m_pPitch = pitch;
            m_Color = color;
            m_dDistSqToBallOfClosestPlayer = 0.0f;
            m_pSupportingPlayer = null;
            m_pReceivingPlayer = null;
            m_pControllingPlayer = null;
            m_pPlayerClosestToBall = null;

            //setup the state machine
            m_pStateMachine = new StateMachine<SoccerTeam>(this);

            m_pStateMachine.SetCurrentState(Defending.instance);
            m_pStateMachine.SetPreviousState(Defending.instance);
            m_pStateMachine.SetGlobalState(null);

            //create the players and goalkeeper
            CreatePlayers();

            //set default steering behaviors
            foreach (PlayerBase it in m_Players)
            {
                (it).Steering().SeparationOn();
            }

            //create the sweet spot calculator
            m_pSupportSpotCalc = new SupportSpotCalculator(Prm.NumSweetSpotsX,
                                                           Prm.NumSweetSpotsY,
                                                           this);
        }

        //~SoccerTeam();

        //the usual suspects
        public void Render()
        {
            foreach (PlayerBase it in m_Players)
            {
                (it).Render();
            }

            Color cc = UnityEngine.Color.white;

            //show the controlling team and player at the top of the display
            if (1 == Prm.bShowControllingTeam)
            {
                //gdi->TextColor(Cgdi::white);

                if ((Color() == team_color.blue) && InControl())
                {
                    //gdi->TextAtPos(20,3,"Blue in Control");
                    DebugWide.PrintText(new Vector3(20, 0, 3), cc, "Blue in Control");
                }
                else if ((Color() == team_color.red) && InControl())
                {
                    //gdi->TextAtPos(20,3,"Red in Control");
                    DebugWide.PrintText(new Vector3(20, 0, 3), cc, "Red in Control");
                }
            
                if (m_pControllingPlayer != null)
                {
                    //gdi->TextAtPos(Pitch()->cxClient()-150, 3, "Controlling Player: " + ttos(m_pControllingPlayer->ID()));
                    DebugWide.PrintText(new Vector3(Pitch().cxClient() - 150, 0, 3), cc, "Controlling Player: "+ m_pControllingPlayer.ID());
                }
            }

            //render the sweet spots
            if (1 == Prm.ViewSupportSpots && InControl())
            {
                m_pSupportSpotCalc.Render();
            }

            //#define SHOW_TEAM_STATE
            //#ifdef SHOW_TEAM_STATE
        //    if (Color() == team_color.red)
        //    {
        //        //gdi->TextColor(Cgdi::white);
        //        cc = Color.white;

        //        if (CurrentState() == Attacking::Instance())
        //    {
        //      gdi->TextAtPos(160, 20, "Attacking");
        //    }
        //    if (CurrentState() == Defending::Instance())
        //    {
        //      gdi->TextAtPos(160, 20, "Defending");
        //    }
        //    if (CurrentState() == PrepareForKickOff::Instance())
        //    {
        //      gdi->TextAtPos(160, 20, "Kickoff");
        //    }
        //  }
        //  else
        //  {
        //    if (CurrentState() == Attacking::Instance())
        //    {
        //      gdi->TextAtPos(160, Pitch()->cyClient()-40, "Attacking");
        //    }
        //    if (CurrentState() == Defending::Instance())
        //    {
        //      gdi->TextAtPos(160, Pitch()->cyClient()-40, "Defending");
        //    }
        //    if (CurrentState() == PrepareForKickOff::Instance())
        //    {
        //      gdi->TextAtPos(160, Pitch()->cyClient()-40, "Kickoff");
        //    }
        //  }
        ////#endif

        ////#define SHOW_SUPPORTING_PLAYERS_TARGET
        ////#ifdef SHOW_SUPPORTING_PLAYERS_TARGET
        //  if (m_pSupportingPlayer)
        //  {
        //    gdi->BlueBrush();
        //gdi->RedPen();
        //gdi->Circle(m_pSupportingPlayer->Steering()->Target(), 4);

          //}
        //#endif

        }

        public void Update()
        {
            //this information is used frequently so it's more efficient to 
            //calculate it just once each frame
            CalculateClosestPlayerToBall();

            //the team state machine switches between attack/defense behavior. It
            //also handles the 'kick off' state where a team must return to their
            //kick off positions before the whistle is blown
            m_pStateMachine.Update();

            //now update each player
            foreach (PlayerBase it in m_Players)
            {
                (it).Update();
            }

        }

        //calling this changes the state of all field players to that of 
        //ReturnToHomeRegion. Mainly used when a goal keeper has
        //possession
        public void ReturnAllFieldPlayersToHome()
        {
            foreach (PlayerBase it in m_Players)
            {
                if ((it).Role() != player_role.goal_keeper)
                {

                    MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                        1,
                                        (it).ID(),
                                        (int)MessageType.Msg_GoHome,
                                        null);
                }
            }
        }

        //returns true if player has a clean shot at the goal and sets ShotTarget
        //to a normalized vector pointing in the direction the shot should be
        //made. Else returns false and sets heading to a zero vector
        public bool CanShoot(Vector3 BallPos, float power, ref Vector3 ShotTarget)
        {
          //the number of randomly created shot targets this method will test 
          int NumAttempts = Prm.NumAttemptsToFindValidStrike;

          while (0 != NumAttempts)
          {
                NumAttempts--;
                //choose a random position along the opponent's goal mouth. (making
                //sure the ball's radius is taken into account)
                ShotTarget = OpponentsGoal().Center();

                //the y value of the shot position should lay somewhere between two
                //goalposts (taking into consideration the ball diameter)
                int MinYVal = (int)(OpponentsGoal().LeftPost().z + Pitch().Ball().BRadius());
                int MaxYVal = (int)(OpponentsGoal().RightPost().z - Pitch().Ball().BRadius());

                ShotTarget.z = (float) Misc.RandInt(MinYVal, MaxYVal);

                //make sure striking the ball with the given power is enough to drive
                //the ball over the goal line.
                float time = Pitch().Ball().TimeToCoverDistance(BallPos,
                                                                  ShotTarget,
                                                                  power);

                //if it is, this shot is then tested to see if any of the opponents
                //can intercept it.
                if (time >= 0)
                {
                    if (isPassSafeFromAllOpponents(BallPos, ShotTarget, null, power))
                    {
                        return true;
                    }
                }
            }
          
          return false;
        }

        //The best pass is considered to be the pass that cannot be intercepted 
        //by an opponent and that is as far forward of the receiver as possible  
        //If a pass is found, the receiver's address is returned in the 
        //reference, 'receiver' and the position the pass will be made to is 
        //returned in the  reference 'PassTarget'
        public bool FindPass( PlayerBase passer,
                         out PlayerBase           receiver,
                         out Vector3              PassTarget,
                         float power,
                         float MinPassingDistance)
        {

            receiver = null;
            PassTarget = Vector3.zero;

            float ClosestToGoalSoFar = float.MaxValue;
            Vector3 Target;

            //iterate through all this player's team members and calculate which
            //one is in a position to be passed the ball 
            foreach (PlayerBase curPlyr in Members())
            {   
                //make sure the potential receiver being examined is not this player
                //and that it is further away than the minimum pass distance
                if ((curPlyr != passer) &&            
                    ((passer.Pos() - (curPlyr).Pos()).sqrMagnitude > 
                     MinPassingDistance* MinPassingDistance))                  
                {           
                    if (GetBestPassToReceiver(passer, curPlyr, out Target, power))
                    {
                        //if the pass target is the closest to the opponent's goal line found
                        // so far, keep a record of it
                        float Dist2Goal = Math.Abs(Target.x - OpponentsGoal().Center().x);

                        if (Dist2Goal<ClosestToGoalSoFar)
                        {
                            ClosestToGoalSoFar = Dist2Goal;
                  
                            //keep a record of this player
                            receiver = curPlyr;

                            //and the target
                            PassTarget = Target;
                        }
                    }
                }
            }//next team member

            if (null != receiver) return true;
            else return false;
        }

        //Three potential passes are calculated. One directly toward the receiver's
        //current position and two that are the tangents from the ball position
        //to the circle of radius 'range' from the receiver.
        //These passes are then tested to see if they can be intercepted by an
        //opponent and to make sure they terminate within the playing area. If
        //all the passes are invalidated the function returns false. Otherwise
        //the function returns the pass that takes the ball closest to the 
        //opponent's goal area.
        public bool GetBestPassToReceiver(PlayerBase passer,
                                        PlayerBase receiver,
                                       out Vector3               PassTarget,
                                       float power)
        {
            PassTarget = Vector3.zero;

            //first, calculate how much time it will take for the ball to reach 
            //this receiver, if the receiver was to remain motionless 
            float time = Pitch().Ball().TimeToCoverDistance(Pitch().Ball().Pos(),
                                                      receiver.Pos(),
                                                      power);

            //return false if ball cannot reach the receiver after having been
            //kicked with the given power
            if (time < 0) return false;

            //the maximum distance the receiver can cover in this time
            float InterceptRange = time * receiver.MaxSpeed();

            //Scale the intercept range
            const float ScalingFactor = 0.3f;
            InterceptRange *= ScalingFactor;

            //now calculate the pass targets which are positioned at the intercepts
            //of the tangents from the ball to the receiver's range circle.
            Vector3 ip1, ip2;

            Geometry.GetTangentPoints(receiver.Pos(),
                             InterceptRange,
                             Pitch().Ball().Pos(),
                             out ip1,
                             out ip2);

            const int NumPassesToTry = 3;
            Vector3[] Passes = new Vector3[NumPassesToTry]{ ip1, receiver.Pos(), ip2 };


            // this pass is the best found so far if it is:
            //
            //  1. Further upfield than the closest valid pass for this receiver
            //     found so far
            //  2. Within the playing area
            //  3. Cannot be intercepted by any opponents

            float ClosestSoFar = float.MaxValue;
            bool bResult = false;

            for (int pass = 0; pass < NumPassesToTry; ++pass)
            {    
                float dist = Math.Abs(Passes[pass].x - OpponentsGoal().Center().x);

                if ((dist<ClosestSoFar) &&
                    Pitch().PlayingArea().Inside(Passes[pass]) &&
                    isPassSafeFromAllOpponents(Pitch().Ball().Pos(),
                                               Passes[pass],
                                               receiver,
                                               power))
                
                {
                  ClosestSoFar = dist;
                  PassTarget   = Passes[pass];
                  bResult      = true;
                }
            }

            return bResult;
        }

        //test if a pass from positions 'from' to 'target' kicked with force 
        //'PassingForce'can be intercepted by an opposing player
        public bool isPassSafeFromOpponent(Vector3 from,
                                        Vector3 target,
                                        PlayerBase receiver,
                                        PlayerBase opp,
                                        float PassingForce)
        {
            //move the opponent into local space.
            Vector3 ToTarget = target - from;
            Vector3 ToTargetNormalized = (ToTarget).normalized;

            Vector3 perp = Vector3.Cross(Vector3.up, ToTargetNormalized);
            Vector3 LocalPosOpp = Transformations.PointToLocalSpace(opp.Pos(),
                                       ToTargetNormalized,
                                       perp,
                                       from);

            //if opponent is behind the kicker then pass is considered okay(this is 
            //based on the assumption that the ball is going to be kicked with a 
            //velocity greater than the opponent's max velocity)
            if (LocalPosOpp.x < 0)
            {     
                return true;
            }

            //if the opponent is further away than the target we need to consider if
            //the opponent can reach the position before the receiver.
            if ((from - target).sqrMagnitude < (opp.Pos() - from).sqrMagnitude)
            {
                if (null != receiver)
                {
                    if ((target - opp.Pos()).sqrMagnitude >
                         (target - receiver.Pos()).sqrMagnitude)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                } 
            }

            //calculate how long it takes the ball to cover the distance to the 
            //position orthogonal to the opponents position
            float TimeForBall =
            Pitch().Ball().TimeToCoverDistance(Vector3.zero,
                                                 new Vector3(LocalPosOpp.x, 0, 0),
                                                 PassingForce);

            //now calculate how far the opponent can run in this time
            float reach = opp.MaxSpeed() * TimeForBall +
                          Pitch().Ball().BRadius() +
                          opp.BRadius();

            //if the distance to the opponent's y position is less than his running
            //range plus the radius of the ball and the opponents radius then the
            //ball can be intercepted
            if (Math.Abs(LocalPosOpp.y) < reach)
            {
                return false;
            }

            return true;
        }

        //tests a pass from position 'from' to position 'target' against each member
        //of the opposing team. Returns true if the pass can be made without
        //getting intercepted
        public bool isPassSafeFromAllOpponents(Vector3 from,
                                            Vector3 target,
                                            PlayerBase receiver,
                                            float PassingForce)
        {

            foreach (PlayerBase opp in Opponents().Members())
            {
                if (!isPassSafeFromOpponent(from, target, receiver, opp, PassingForce))
                {
                    //debug_on
        
                    return false;
                }
            }

            return true;
        }

        //returns true if there is an opponent within radius of position
        public bool isOpponentWithinRadius(Vector3 pos, float rad)
        {
            foreach (PlayerBase it in Opponents().Members())
            {
                if ((pos - (it).Pos()).sqrMagnitude < rad * rad)
                {
                    return true;
                }
            }

            return false;
        }

        //this tests to see if a pass is possible between the requester and
        //the controlling player. If it is possible a message is sent to the
        //controlling player to pass the ball asap.
        public void RequestPass(FieldPlayer requester)
        {
            //maybe put a restriction here
            if (Misc.RandFloat() > 0.1f) return;

            if (isPassSafeFromAllOpponents(ControllingPlayer().Pos(),
                                 requester.Pos(),
                                 requester,
                                 Prm.MaxPassingForce))
            {

                //tell the player to make the pass
                //let the receiver know a pass is coming 
                MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                  requester.ID(),
                                  ControllingPlayer().ID(),
                                  (int)MessageType.Msg_PassToMe,
                                  requester); 

            }
        }

        //calculates the best supporting position and finds the most appropriate
        //attacker to travel to the spot
        public PlayerBase DetermineBestSupportingAttacker()
        {
            float ClosestSoFar = float.MaxValue;

            PlayerBase BestPlayer = null;

            foreach(PlayerBase it in m_Players)
            {
                //only attackers utilize the BestSupportingSpot
                if (((it).Role() == player_role.attacker) && ((it) != m_pControllingPlayer))
                {
                    //calculate the dist. Use the squared value to avoid sqrt
                    float dist = ((it).Pos() - m_pSupportSpotCalc.GetBestSupportingSpot()).sqrMagnitude;

                    //if the distance is the closest so far and the player is not a
                    //goalkeeper and the player is not the one currently controlling
                    //the ball, keep a record of this player
                    if ((dist < ClosestSoFar))
                    {
                        ClosestSoFar = dist;

                        BestPlayer = (it);
                    }
                }
            }

            return BestPlayer;
        }


        public List<PlayerBase> Members() {return m_Players;}

        public StateMachine<SoccerTeam> GetFSM() {return m_pStateMachine;}

        public Goal  HomeGoal() {return m_pHomeGoal;}
        public Goal  OpponentsGoal() {return m_pOpponentsGoal;}

        public SoccerPitch Pitch() {return m_pPitch;}

        public SoccerTeam Opponents() {return m_pOpponents;}
        public void SetOpponents(SoccerTeam opps) { m_pOpponents = opps; }

        public team_color Color() {return m_Color;}

        public void SetPlayerClosestToBall(PlayerBase plyr) { m_pPlayerClosestToBall = plyr; }
        public PlayerBase PlayerClosestToBall() {return m_pPlayerClosestToBall;}

        public float ClosestDistToBallSq() {return m_dDistSqToBallOfClosestPlayer;}

        public Vector3 GetSupportSpot() {return m_pSupportSpotCalc.GetBestSupportingSpot();}

        public PlayerBase SupportingPlayer() {return m_pSupportingPlayer;}
        public void SetSupportingPlayer(PlayerBase plyr) { m_pSupportingPlayer = plyr; }

        public PlayerBase Receiver() {return m_pReceivingPlayer;}
        public void SetReceiver(PlayerBase plyr) { m_pReceivingPlayer = plyr; }

        public PlayerBase ControllingPlayer() {return m_pControllingPlayer;}
        public void SetControllingPlayer(PlayerBase plyr)
        {
            m_pControllingPlayer = plyr;

            //rub it in the opponents faces!
            Opponents().LostControl();
        }


        public bool InControl() 
        {
            if(null != m_pControllingPlayer)
                return true; 
            else 
                return false;
        }
        public void LostControl() { m_pControllingPlayer = null; }

        public PlayerBase GetPlayerFromID(int id)
        {
            foreach (PlayerBase it in m_Players)
            {
                if ((it).ID() == id) return it;
            }

            return null;
        }


        public void SetPlayerHomeRegion(int plyr, int region)
        {
          //assert((plyr>=0) && (plyr<(int)m_Players.size()) );

          m_Players[plyr].SetHomeRegion(region);
        }

        public void DetermineBestSupportingPosition() {m_pSupportSpotCalc.DetermineBestSupportingPosition();}

        public void UpdateTargetsOfWaitingPlayers()
        {
            foreach (PlayerBase it in m_Players)
            {  
                if ((it).Role() !=  player_role.goal_keeper )
                {
                    //cast to a field player
                    FieldPlayer plyr = (FieldPlayer)(it);
      
                  if (plyr.GetFSM().isInState(Wait.instance) ||
                       plyr.GetFSM().isInState(ReturnToHomeRegion.instance) )
                  {
                    plyr.Steering().SetTarget(plyr.HomeRegion().Center());
                  }
                }
            }
        }

        //returns false if any of the team are not located within their home region
        public bool AllPlayersAtHome()
        {
            foreach (PlayerBase it in m_Players)
            {
                if ((it).InHomeRegion() == false)
                {
                  return false;
                }
            }

          return true;
        }

        public string Name() { return m_Color.ToString(); }

    }
}//end namespace

