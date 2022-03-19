namespace Proto_AI_4
{

    public class StateGlobal_Unit : State<Unit>
    {
        private StateGlobal_Unit() { }


        //this is a singleton
        public static readonly StateGlobal_Unit inst = new StateGlobal_Unit();

        //public override void Enter(FieldPlayer* player) { }

        public override void Execute(Unit player)
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

        public override bool OnMessage(Unit player, Telegram telegram)
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

    public class State_Move_Unit : State<Unit>
    {
            private State_Move_Unit() { }

            //this is a singleton
            public static readonly State_Move_Unit inst = new State_Move_Unit();

            public override void Enter(Unit player)
            {
                
            }

            public override void Execute(Unit player)
            {
                
            }

            public override void Exit(Unit player)
            {
                
            }

            public override bool OnMessage(Unit f, Telegram t) { return false; }
    }

   
}//end namespace



