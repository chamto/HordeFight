using UnityEngine;
using Buckland;
using UtilGS9;

namespace Test_SimpleSoccer
{

    public class GlobalPlayerState : State<FieldPlayer>
    {
        private GlobalPlayerState() { }


        //this is a singleton
        public static readonly GlobalPlayerState instance = new GlobalPlayerState();

        //public override void Enter(FieldPlayer* player) { }

        public override void Execute(FieldPlayer player)
        {
            //if a player is in possession and close to the ball reduce his max speed
            if ((player.BallWithinReceivingRange()) && (player.isControllingPlayer()))
            {
                player.SetMaxSpeed(Prm.PlayerMaxSpeedWithBall);
            }

            else
            {
                player.SetMaxSpeed(Prm.PlayerMaxSpeedWithoutBall);
            }

        }

        //public override void Exit(FieldPlayer* player) { }

        public override bool OnMessage(FieldPlayer player, Telegram telegram)
        {
            switch ((MessageType)telegram.Msg)
            {
                case MessageType.Msg_ReceiveBall:
                {
                        //set the target
                        player.Steering().SetTarget((Vector3)(telegram.ExtraInfo));

                        //change state 
                        player.GetFSM().ChangeState(ReceiveBall.instance);

                        return true;
                }
                case MessageType.Msg_SupportAttacker:
                {
                        //if already supporting just return
                        if (player.GetFSM().isInState(SupportAttacker.instance))
                        {
                            return true;
                        }

                        //set the target to be the best supporting position
                        player.Steering().SetTarget(player.Team().GetSupportSpot());

                        //change the state
                        player.GetFSM().ChangeState(SupportAttacker.instance);

                        return true;
                }
                case MessageType.Msg_Wait:
                {
                        //change the state
                        player.GetFSM().ChangeState(Wait.instance);

                        return true;
                }
                case MessageType.Msg_GoHome:
                {
                        player.SetDefaultHomeRegion();

                        player.GetFSM().ChangeState(ReturnToHomeRegion.instance);

                        return true;
                }
                case MessageType.Msg_PassToMe:
                {

                        //get the position of the player requesting the pass 
                        FieldPlayer receiver = (FieldPlayer)(telegram.ExtraInfo);

                        //# ifdef PLAYER_STATE_INFO_ON
                        //debug_con << "Player " << player->ID() << " received request from " <<
                        //receiver->ID() << " to make pass" << "";
                        //#endif

                        //if the ball is not within kicking range or their is already a 
                        //receiving player, this player cannot pass the ball to the player
                        //making the request.
                        if (player.Team().Receiver() != null ||
                           !player.BallWithinKickingRange())
                        {
                            //#ifdef PLAYER_STATE_INFO_ON
                            //debug_con << "Player " << player->ID() << " cannot make requested pass <cannot kick ball>" << "";
                            //#endif

                            return true;
                        }

                        //make the pass   
                        player.Ball().Kick(receiver.Pos() - player.Ball().Pos(),
                                             Prm.MaxPassingForce);


                        //# ifdef PLAYER_STATE_INFO_ON
                        //debug_con << "Player " << player->ID() << " Passed ball to requesting player" << "";
                        //#endif

                        //let the receiver know a pass is coming 

                        MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                      player.ID(),
                                      receiver.ID(),
                                      (int)MessageType.Msg_ReceiveBall,
                                      receiver.Pos());



                        //change state   
                        player.GetFSM().ChangeState(Wait.instance);

                        player.FindSupport();

                        return true;
                }
          }//end switch

          return false;
        }
    }

    //------------------------------------------------------------------------
    public class ChaseBall : State<FieldPlayer>
    {
  
        private ChaseBall() { }

        //this is a singleton
        public static readonly ChaseBall instance = new ChaseBall();

        public override void Enter(FieldPlayer player)
        {
            player.Steering().SeekOn();

//# ifdef PLAYER_STATE_INFO_ON
//            debug_con << "Player " << player->ID() << " enters chase state" << "";
//#endif
        }

        public override void Execute(FieldPlayer player)
        {
            //if the ball is within kicking range the player changes state to KickBall.
            if (player.BallWithinKickingRange())
            {
                player.GetFSM().ChangeState(KickBall.instance);

                return;
            }

            //if the player is the closest player to the ball then he should keep
            //chasing it
            if (player.isClosestTeamMemberToBall())
            {
                player.Steering().SetTarget(player.Ball().Pos());

                return;
            }

            //if the player is not closest to the ball anymore, he should return back
            //to his home region and wait for another opportunity
            player.GetFSM().ChangeState(ReturnToHomeRegion.instance);
        }

        public override void Exit(FieldPlayer player)
        {
            player.Steering().SeekOff();
        }

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }

    //------------------------------------------------------------------------
    public class Dribble : State<FieldPlayer>
    {
  
        private Dribble() { }

        //this is a singleton
        public static readonly Dribble instance = new Dribble();

        public override void Enter(FieldPlayer player)
        {
            //let the team know this player is controlling
            player.Team().SetControllingPlayer(player);

//# ifdef PLAYER_STATE_INFO_ON
//            debug_con << "Player " << player->ID() << " enters dribble state" << "";
//#endif
        }

        public override void Execute(FieldPlayer player)
        {
            float dot = Vector3.Dot(player.Team().HomeGoal().Facing(), player.Heading());

            //if the ball is between the player and the home goal, it needs to swivel
            // the ball around by doing multiple small kicks and turns until the player 
            //is facing in the correct direction
            if (dot < 0)
            {
                //the player's heading is going to be rotated by a small amount (Pi/4) 
                //and then the ball will be kicked in that direction
                Vector3 direction = player.Heading();

                //calculate the sign (+/-) of the angle between the player heading and the 
                //facing direction of the goal so that the player rotates around in the 
                //correct direction
                int sign = VOp.Sign_XZ(player.Team().HomeGoal().Facing(), player.Heading());
                float angle = Const.QuarterPi * -1 * sign;

                Transformations.Vec3RotateYAroundOrigin(ref direction, angle);

                //this value works well whjen the player is attempting to control the
                //ball and turn at the same time
                const float KickingForce = 0.8f;

                player.Ball().Kick(direction, KickingForce);
            }

            //kick the ball down the field
            else
            {
                player.Ball().Kick(player.Team().HomeGoal().Facing(),
                                     Prm.MaxDribbleForce);
            }

            //the player has kicked the ball so he must now change state to follow it
            player.GetFSM().ChangeState(ChaseBall.instance);

            return;
        }

        //public override void Exit(FieldPlayer* player) { }

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }


    //------------------------------------------------------------------------
    public class ReturnToHomeRegion : State<FieldPlayer>
    {
  
        private ReturnToHomeRegion() { }


        //this is a singleton
        public static readonly ReturnToHomeRegion instance = new ReturnToHomeRegion();

        public override void Enter(FieldPlayer player)
        {
            player.Steering().ArriveOn();

            if (!player.HomeRegion().Inside(player.Steering().Target(), region_modifier.halfsize))
            {
                player.Steering().SetTarget(player.HomeRegion().Center());
            }

//# ifdef PLAYER_STATE_INFO_ON
            //debug_con << "Player " << player->ID() << " enters ReturnToHome state" << "";
//#endif
        }

        public override void Execute(FieldPlayer player)
        {
            if (player.Pitch().GameOn())
            {
                //if the ball is nearer this player than any other team member  &&
                //there is not an assigned receiver && the goalkeeper does not gave
                //the ball, go chase it
                if (player.isClosestTeamMemberToBall() &&
                     (player.Team().Receiver() == null) &&
                     !player.Pitch().GoalKeeperHasBall())
                {
                    player.GetFSM().ChangeState(ChaseBall.instance);

                    return;
                }
            }

            //if game is on and close enough to home, change state to wait and set the 
            //player target to his current position.(so that if he gets jostled out of 
            //position he can move back to it)
            if (player.Pitch().GameOn() && player.HomeRegion().Inside(player.Pos(),
                                                                        region_modifier.halfsize))
            {
                player.Steering().SetTarget(player.Pos());
                player.GetFSM().ChangeState(Wait.instance);
            }
            //if game is not on the player must return much closer to the center of his
            //home region
            else if (!player.Pitch().GameOn() && player.AtTarget())
            {
                player.GetFSM().ChangeState(Wait.instance);
            }
        }

        public override void Exit(FieldPlayer player)
        {
            player.Steering().ArriveOff();
        }

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }

    //------------------------------------------------------------------------
    public class Wait : State<FieldPlayer>
    {
        private Wait() { }

        //this is a singleton
        public static readonly Wait instance = new Wait();

        public override void Enter(FieldPlayer player)
        {
//# ifdef PLAYER_STATE_INFO_ON
//            debug_con << "Player " << player->ID() << " enters wait state" << "";
//#endif

            //if the game is not on make sure the target is the center of the player's
            //home region. This is ensure all the players are in the correct positions
            //ready for kick off
            if (!player.Pitch().GameOn())
            {
                player.Steering().SetTarget(player.HomeRegion().Center());
            }
        }

        public override void Execute(FieldPlayer player)
        {
            //if the player has been jostled out of position, get back in position  
            if (!player.AtTarget())
            {
                player.Steering().ArriveOn();

                return;
            }

            else
            {
                player.Steering().ArriveOff();

                player.SetVelocity(Vector3.zero);

                //the player should keep his eyes on the ball!
                player.TrackBall();
            }

            //if this player's team is controlling AND this player is not the attacker
            //AND is further up the field than the attacker he should request a pass.
            if (player.Team().InControl() &&
               (!player.isControllingPlayer()) &&
                 player.isAheadOfAttacker())
            {
                player.Team().RequestPass(player);

                return;
            }

            if (player.Pitch().GameOn())
            {
                //if the ball is nearer this player than any other team member  AND
                //there is not an assigned receiver AND neither goalkeeper has
                //the ball, go chase it
                if (player.isClosestTeamMemberToBall() &&
                    player.Team().Receiver() == null &&
                    !player.Pitch().GoalKeeperHasBall())
                {
                    player.GetFSM().ChangeState(ChaseBall.instance);

                    return;
                }
            }
        }

        //public override void Exit(FieldPlayer* player);

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }

    //------------------------------------------------------------------------
    public class KickBall : State<FieldPlayer>
    {
        private KickBall() { }

        //this is a singleton
        public static readonly KickBall instance = new KickBall();

        public override void Enter(FieldPlayer player)
        {
            //let the team know this player is controlling
            player.Team().SetControllingPlayer(player);

            //the player can only make so many kick attempts per second.
            if (!player.isReadyForNextKick())
            {
                player.GetFSM().ChangeState(ChaseBall.instance);
            }


//# ifdef PLAYER_STATE_INFO_ON
//            debug_con << "Player " << player->ID() << " enters kick state" << "";
//#endif
        }

        public override void Execute(FieldPlayer player)
        {
            //calculate the dot product of the vector pointing to the ball
            //and the player's heading
            Vector3 ToBall = player.Ball().Pos() - player.Pos();
            float dot = Vector3.Dot(player.Heading(), ToBall.normalized);

            //cannot kick the ball if the goalkeeper is in possession or if it is 
            //behind the player or if there is already an assigned receiver. So just
            //continue chasing the ball
            if (player.Team().Receiver() != null ||
                player.Pitch().GoalKeeperHasBall() ||
                (dot < 0))
            {
//# ifdef PLAYER_STATE_INFO_ON
//                debug_con << "Goaly has ball / ball behind player" << "";
//#endif

                player.GetFSM().ChangeState(ChaseBall.instance);

                return;
            }

            /* Attempt a shot at the goal */

            //if a shot is possible, this vector will hold the position along the 
            //opponent's goal line the player should aim for.
            Vector3 BallTarget = Vector3.zero;

            //the dot product is used to adjust the shooting force. The more
            //directly the ball is ahead, the more forceful the kick
            float power = Prm.MaxShootingForce * dot;

            //if it is determined that the player could score a goal from this position
            //OR if he should just kick the ball anyway, the player will attempt
            //to make the shot
            if (player.Team().CanShoot(player.Ball().Pos(),
                                         power,
                                         ref BallTarget) ||
               (Misc.RandFloat() < Prm.ChancePlayerAttemptsPotShot))
            {
//# ifdef PLAYER_STATE_INFO_ON
//                debug_con << "Player " << player->ID() << " attempts a shot at " << BallTarget << "";
//#endif

                //add some noise to the kick. We don't want players who are 
                //too accurate! The amount of noise can be adjusted by altering
                //Prm.PlayerKickingAccuracy
                BallTarget = SoccerBall.AddNoiseToKick(player.Ball().Pos(), BallTarget);

                //this is the direction the ball will be kicked in
                Vector3 KickDirection = BallTarget - player.Ball().Pos();

                player.Ball().Kick(KickDirection, power);

                //change state   
                player.GetFSM().ChangeState(Wait.instance);

                player.FindSupport();

                return;
            }


            /* Attempt a pass to a player */

            //if a receiver is found this will point to it
            PlayerBase receiver = null;

            power = Prm.MaxPassingForce * dot;

            //test if there are any potential candidates available to receive a pass
            if (player.isThreatened() &&
                player.Team().FindPass(player,
                                        out receiver,
                                        out BallTarget,
                                        power,
                                        Prm.MinPassDistance))
            {
                //add some noise to the kick
                BallTarget = SoccerBall.AddNoiseToKick(player.Ball().Pos(), BallTarget);

                Vector3 KickDirection = BallTarget - player.Ball().Pos();

                player.Ball().Kick(KickDirection, power);

                //# ifdef PLAYER_STATE_INFO_ON
                //                debug_con << "Player " << player->ID() << " passes the ball with force " << power << "  to player "
                //                          << receiver->ID() << "  Target is " << BallTarget << "";
                //#endif


                //let the receiver know a pass is coming 
                MessageDispatcher.Instance().DispatchMsg(Telegram.SEND_MSG_IMMEDIATELY,
                                        player.ID(),
                                        receiver.ID(),
                                        (int)MessageType.Msg_ReceiveBall,
                                        BallTarget);


                //the player should wait at his current position unless instruced
                //otherwise  
                player.GetFSM().ChangeState(Wait.instance);

                player.FindSupport();

                return;
            }

            //cannot shoot or pass, so dribble the ball upfield
            else
            {
                player.FindSupport();

                player.GetFSM().ChangeState(Dribble.instance);
            }
        }

        //public override void Exit(FieldPlayer* player) { }

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }

    //------------------------------------------------------------------------
    public class ReceiveBall : State<FieldPlayer>
    {
        private ReceiveBall() { }

        //this is a singleton
        public static readonly ReceiveBall instance = new ReceiveBall();

        public override void Enter(FieldPlayer player)
        {
            //let the team know this player is receiving the ball
            player.Team().SetReceiver(player);

            //this player is also now the controlling player
            player.Team().SetControllingPlayer(player);

            //there are two types of receive behavior. One uses arrive to direct
            //the receiver to the position sent by the passer in its telegram. The
            //other uses the pursuit behavior to pursue the ball. 
            //This statement selects between them dependent on the probability
            //ChanceOfUsingArriveTypeReceiveBehavior, whether or not an opposing
            //player is close to the receiving player, and whether or not the receiving
            //player is in the opponents 'hot region' (the third of the pitch closest
            //to the opponent's goal
            const float PassThreatRadius = 70.0f;

            if ((player.InHotRegion() ||
                  Misc.RandFloat() < Prm.ChanceOfUsingArriveTypeReceiveBehavior) &&
               !player.Team().isOpponentWithinRadius(player.Pos(), PassThreatRadius))
            {
                player.Steering().ArriveOn();

//# ifdef PLAYER_STATE_INFO_ON
//                debug_con << "Player " << player->ID() << " enters receive state (Using Arrive)" << "";
//#endif
            }
            else
            {
                player.Steering().PursuitOn();

//# ifdef PLAYER_STATE_INFO_ON
//                debug_con << "Player " << player->ID() << " enters receive state (Using Pursuit)" << "";
//#endif
            }
        }

        public override void Execute(FieldPlayer player)
        {
            //if the ball comes close enough to the player or if his team lose control
            //he should change state to chase the ball
            if (player.BallWithinReceivingRange() || !player.Team().InControl())
            {
                player.GetFSM().ChangeState(ChaseBall.instance);

                return;
            }

            if (player.Steering().PursuitIsOn())
            {
                player.Steering().SetTarget(player.Ball().Pos());
            }

            //if the player has 'arrived' at the steering target he should wait and
            //turn to face the ball
            if (player.AtTarget())
            {
                player.Steering().ArriveOff();
                player.Steering().PursuitOff();
                player.TrackBall();
                player.SetVelocity(Vector3.zero);
            }
        }

        public override void Exit(FieldPlayer player)
        {
            player.Steering().ArriveOff();
            player.Steering().PursuitOff();

            player.Team().SetReceiver(null);
        }

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }


    //------------------------------------------------------------------------
    public class SupportAttacker : State<FieldPlayer>
    {
        private SupportAttacker() { }

        //this is a singleton
        public static readonly SupportAttacker instance = new SupportAttacker();

        public override void Enter(FieldPlayer player)
        {
            player.Steering().ArriveOn();

            player.Steering().SetTarget(player.Team().GetSupportSpot());

//# ifdef PLAYER_STATE_INFO_ON
//            debug_con << "Player " << player->ID() << " enters support state" << "";
//#endif
        }

        public override void Execute(FieldPlayer player)
        {
            //if his team loses control go back home
            if (!player.Team().InControl())
            {
                player.GetFSM().ChangeState(ReturnToHomeRegion.instance); return;
            }


            //if the best supporting spot changes, change the steering target
            if (player.Team().GetSupportSpot() != player.Steering().Target())
            {
                player.Steering().SetTarget(player.Team().GetSupportSpot());

                player.Steering().ArriveOn();
            }

            //if this player has a shot at the goal AND the attacker can pass
            //the ball to him the attacker should pass the ball to this player
            Vector3 temp = Vector3.zero;
            if (player.Team().CanShoot(player.Pos(),
                                         Prm.MaxShootingForce, ref temp))
            {
                player.Team().RequestPass(player);
            }


            //if this player is located at the support spot and his team still have
            //possession, he should remain still and turn to face the ball
            if (player.AtTarget())
            {
                player.Steering().ArriveOff();

                //the player should keep his eyes on the ball!
                player.TrackBall();

                player.SetVelocity(Vector3.zero);

                //if not threatened by another player request a pass
                if (!player.isThreatened())
                {
                    player.Team().RequestPass(player);
                }
            }
        }

        public override void Exit(FieldPlayer player)
        {
            //set supporting player to null so that the team knows it has to 
            //determine a new one.
            player.Team().SetSupportingPlayer(null);

            player.Steering().ArriveOff();
        }

        public override bool OnMessage(FieldPlayer f, Telegram t) { return false; }
    }

}//end namespace

