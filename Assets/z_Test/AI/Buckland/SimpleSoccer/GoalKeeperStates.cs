using UnityEngine;
using Buckland;

namespace Test_SimpleSoccer
{

    public class GlobalKeeperState : State<GoalKeeper>
    {
        private GlobalKeeperState() { }


        //this is a singleton
        public static readonly GlobalKeeperState instance = new GlobalKeeperState();

        //public override void Enter(GoalKeeper* keeper) { }

        //public override void Execute(GoalKeeper* keeper) { }

        //public override void Exit(GoalKeeper* keeper) { }

        public override bool OnMessage(GoalKeeper keeper, Telegram telegram)
        {
          switch((MessageType)telegram.Msg)
          {
            case MessageType.Msg_GoHome:
            {
              keeper.SetDefaultHomeRegion();

                keeper.GetFSM().ChangeState(ReturnHome.instance);
            }
            break;

            case MessageType.Msg_ReceiveBall:
            {
                keeper.GetFSM().ChangeState(InterceptBall.instance);
            }
            break;

          }//end switch

          return false;
        }
    }

    //-----------------------------------------------------------------------------

    public class TendGoal : State<GoalKeeper>
    {
        private TendGoal() { }


        //this is a singleton
        public static readonly TendGoal instance = new TendGoal();

        public override void Enter(GoalKeeper keeper)
        {
            //turn interpose on
            keeper.Steering().InterposeOn(Prm.GoalKeeperTendingDistance);

            //interpose will position the agent between the ball position and a target
            //position situated along the goal mouth. This call sets the target
            keeper.Steering().SetTarget(keeper.GetRearInterposeTarget());
        }

        public override void Execute(GoalKeeper keeper)
        {
            //the rear interpose target will change as the ball's position changes
            //so it must be updated each update-step 
            keeper.Steering().SetTarget(keeper.GetRearInterposeTarget());

            //if the ball comes in range the keeper traps it and then changes state
            //to put the ball back in play
            if (keeper.BallWithinKeeperRange())
            {
                keeper.Ball().Trap();

                keeper.Pitch().SetGoalKeeperHasBall(true);

                keeper.GetFSM().ChangeState(PutBallBackInPlay.instance);

                return;
            }

            //if ball is within a predefined distance, the keeper moves out from
            //position to try and intercept it.
            if (keeper.BallWithinRangeForIntercept() && !keeper.Team().InControl())
            {
                keeper.GetFSM().ChangeState(InterceptBall.instance);
            }

            //if the keeper has ventured too far away from the goal-line and there
            //is no threat from the opponents he should move back towards it
            if (keeper.TooFarFromGoalMouth() && keeper.Team().InControl())
            {
                keeper.GetFSM().ChangeState(ReturnHome.instance);

                return;
            }
        }

        public override void Exit(GoalKeeper keeper)
        {
            keeper.Steering().InterposeOff();
        }

        public override bool OnMessage(GoalKeeper g, Telegram t) { return false; }
    }

//------------------------------------------------------------------------
    public class InterceptBall : State<GoalKeeper>
    {
        private InterceptBall() { }

        //this is a singleton
        public static readonly InterceptBall instance = new InterceptBall();

        public override void Enter(GoalKeeper keeper)
        {
            keeper.Steering().PursuitOn();

    //# ifdef GOALY_STATE_INFO_ON
    //        debug_con << "Goaly " << keeper->ID() << " enters InterceptBall" << "";
    //#endif
        }

        public override void Execute(GoalKeeper keeper)
        {
            //if the goalkeeper moves to far away from the goal he should return to his
            //home region UNLESS he is the closest player to the ball, in which case,
            //he should keep trying to intercept it.
            if (keeper.TooFarFromGoalMouth() && !keeper.isClosestPlayerOnPitchToBall())
            {
                keeper.GetFSM().ChangeState(ReturnHome.instance);

                return;
            }

            //if the ball becomes in range of the goalkeeper's hands he traps the 
            //ball and puts it back in play
            if (keeper.BallWithinKeeperRange())
            {
                keeper.Ball().Trap();

                keeper.Pitch().SetGoalKeeperHasBall(true);

                keeper.GetFSM().ChangeState(PutBallBackInPlay.instance);

                return;
            }
        }

        public override void Exit(GoalKeeper keeper)
        {
            keeper.Steering().PursuitOff();
        }

        public override bool OnMessage(GoalKeeper g, Telegram t) { return false; }
    }

    //------------------------------------------------------------------------
    public class ReturnHome : State<GoalKeeper>
    {
        private ReturnHome() { }

        //this is a singleton
        public static readonly ReturnHome instance = new ReturnHome();

        public override void Enter(GoalKeeper keeper)
        {
            keeper.Steering().ArriveOn();
        }

        public override void Execute(GoalKeeper keeper)
        {
            keeper.Steering().SetTarget(keeper.HomeRegion().Center());

            //if close enough to home or the opponents get control over the ball,
            //change state to tend goal
            if (keeper.InHomeRegion() || !keeper.Team().InControl())
            {
                keeper.GetFSM().ChangeState(TendGoal.instance);
            }
        }

        public override void Exit(GoalKeeper keeper)
        {
            keeper.Steering().ArriveOff();
        }

        public override bool OnMessage(GoalKeeper g, Telegram t) { return false; }
    }

//------------------------------------------------------------------------
    public class PutBallBackInPlay : State<GoalKeeper>
    {
        private PutBallBackInPlay() { }

        //this is a singleton
        public static readonly PutBallBackInPlay instance = new PutBallBackInPlay();

        public override void Enter(GoalKeeper keeper)
        {
            //let the team know that the keeper is in control
            keeper.Team().SetControllingPlayer(keeper);

            //send all the players home
            keeper.Team().Opponents().ReturnAllFieldPlayersToHome();
            keeper.Team().ReturnAllFieldPlayersToHome();
        }

        public override void Execute(GoalKeeper keeper)
        {
            PlayerBase receiver = null;
            Vector3 BallTarget;

            //test if there are players further forward on the field we might
            //be able to pass to. If so, make a pass.
            if (keeper.Team().FindPass(keeper,
                                        out receiver,
                                        out BallTarget,
                                        Prm.MaxPassingForce,
                                        Prm.GoalkeeperMinPassDistance))
            {
                //make the pass   
                keeper.Ball().Kick((BallTarget - keeper.Ball().Pos()).normalized,
                                     Prm.MaxPassingForce);

                //goalkeeper no longer has ball 
                keeper.Pitch().SetGoalKeeperHasBall(false);

                //let the receiving player know the ball's comin' at him
                MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                      keeper.ID(),
                                      receiver.ID(),
                                      (int)MessageType.Msg_ReceiveBall,
                                      BallTarget);

                //go back to tending the goal   
                keeper.GetFSM().ChangeState(TendGoal.instance);

                return;
            }

            keeper.SetVelocity(Vector3.zero);
        }

    //public override void Exit(GoalKeeper* keeper) { }

        public override bool OnMessage(GoalKeeper g, Telegram t) { return false; }
    }


}//end namespace

