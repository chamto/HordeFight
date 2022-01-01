namespace Proto_AI_4
{
    public class State_Charactor : State<Character>
    { }


    public class GlobalState_Charactor : State<Character>
    {
        private GlobalState_Charactor() { }


        //this is a singleton
        public static readonly GlobalState_Charactor inst = new GlobalState_Charactor();

        //public override void Enter(FieldPlayer* player) { }

        public override void Execute(Character player)
        {
            //if a player is in possession and close to the ball reduce his max speed
            //if ((player.BallWithinReceivingRange()) && (player.isControllingPlayer()))
            //{
            //    player.SetMaxSpeed(Prm.PlayerMaxSpeedWithBall);
            //}

            //else
            //{
            //    player.SetMaxSpeed(Prm.PlayerMaxSpeedWithoutBall);
            //}

        }

        //public override void Exit(Character player) { }

        public override bool OnMessage(Character player, Telegram telegram)
        {
            //switch ((MessageType)telegram.Msg)
            //{
            //    case MessageType.Msg_ReceiveBall:
            //        {
            //            //set the target
            //            player.Steering().SetTarget((Vector3)(telegram.ExtraInfo));

            //            //change state 
            //            player.GetFSM().ChangeState(ReceiveBall.instance);

            //            return true;
            //        }
            //    case MessageType.Msg_SupportAttacker:
            //        {
            //            //if already supporting just return
            //            if (player.GetFSM().isInState(SupportAttacker.instance))
            //            {
            //                return true;
            //            }

            //            //set the target to be the best supporting position
            //            player.Steering().SetTarget(player.Team().GetSupportSpot());

            //            //change the state
            //            player.GetFSM().ChangeState(SupportAttacker.instance);

            //            return true;
            //        }
            //    case MessageType.Msg_Wait:
            //        {
            //            //change the state
            //            player.GetFSM().ChangeState(Wait.instance);

            //            return true;
            //        }
            //    case MessageType.Msg_GoHome:
            //        {
            //            player.SetDefaultHomeRegion();

            //            player.GetFSM().ChangeState(ReturnToHomeRegion.instance);

            //            return true;
            //        }
            //    case MessageType.Msg_PassToMe:
            //        {

            //            //get the position of the player requesting the pass 
            //            FieldPlayer receiver = (FieldPlayer)(telegram.ExtraInfo);

            //            //# ifdef PLAYER_STATE_INFO_ON
            //            //debug_con << "Player " << player->ID() << " received request from " <<
            //            //receiver->ID() << " to make pass" << "";
            //            //#endif

            //            //if the ball is not within kicking range or their is already a 
            //            //receiving player, this player cannot pass the ball to the player
            //            //making the request.
            //            if (player.Team().Receiver() != null ||
            //               !player.BallWithinKickingRange())
            //            {
            //                //#ifdef PLAYER_STATE_INFO_ON
            //                //debug_con << "Player " << player->ID() << " cannot make requested pass <cannot kick ball>" << "";
            //                //#endif

            //                return true;
            //            }

            //            //make the pass   
            //            player.Ball().Kick(receiver.Pos() - player.Ball().Pos(),
            //                                 Prm.MaxPassingForce);


            //            //# ifdef PLAYER_STATE_INFO_ON
            //            //debug_con << "Player " << player->ID() << " Passed ball to requesting player" << "";
            //            //#endif

            //            //let the receiver know a pass is coming 

            //            MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
            //                          player.ID(),
            //                          receiver.ID(),
            //                          (int)MessageType.Msg_ReceiveBall,
            //                          receiver.Pos());



            //            //change state   
            //            player.GetFSM().ChangeState(Wait.instance);

            //            player.FindSupport();

            //            return true;
            //        }
            //}//end switch

            return false;
        }
    }

    //------------------------------------------------------------------------
    //public class ChaseBall : State<FieldPlayer>
    //{

    //    private ChaseBall() { }

    //    //this is a singleton
    //    public static readonly ChaseBall instance = new ChaseBall();

    //    public override void Enter(FieldPlayer player)
    //    {
    //        player.Steering().SeekOn();

    //        //# ifdef PLAYER_STATE_INFO_ON
    //        //            debug_con << "Player " << player->ID() << " enters chase state" << "";
    //        //#endif
    //    }

    //    public override void Execute(FieldPlayer player)
    //    {
    //        //if the ball is within kicking range the player changes state to KickBall.
    //        if (player.BallWithinKickingRange())
    //        {
    //            player.GetFSM().ChangeState(KickBall.instance);

    //            return;
    //        }

    //        //if the player is the closest player to the ball then he should keep
    //        //chasing it
    //        if (player.isClosestTeamMemberToBall())
    //        {
    //            player.Steering().SetTarget(player.Ball().Pos());

    //            return;
    //        }

    //        //if the player is not closest to the ball anymore, he should return back
    //        //to his home region and wait for another opportunity
    //        player.GetFSM().ChangeState(ReturnToHomeRegion.instance);
    //    }

    //    public override void Exit(FieldPlayer player)
    //    {
    //        player.Steering().SeekOff();
    //    }

    //    public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    //}
}//end namespace



