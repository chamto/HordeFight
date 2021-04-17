using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace WestWorld
{
    public class WifesGlobalState : State<MinersWife>
    {  
    
        public static WifesGlobalState Instance = new WifesGlobalState();

        public override void Enter(MinersWife wife) { }

        public override void Execute(MinersWife wife)
        {
            //1 in 10 chance of needing the bathroom (provided she is not already
            //in the bathroom)
            if ((Misc.RandFloat() < 0.1f) &&
                 !wife.GetFSM().isInState(VisitBathroom.Instance))
            {
                wife.GetFSM().ChangeState(VisitBathroom.Instance);
            }
        }

        public override void Exit(MinersWife wife) { }

        public override bool OnMessage(MinersWife wife, Telegram msg)
        {
          
          switch(msg.Msg)
          {

                case (int)msg_type.HiHoneyImHome:
                {
                   
                    DebugWide.LogRed("Message handled by wife" + " at time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    DebugWide.LogGreen("wife: Hi honey. Let me make you some of mah fine country stew");
                    
                    wife.GetFSM().ChangeState(CookStew.Instance);
                }
                return true;

          }//end switch

          return false;
        }
    }


    //------------------------------------------------------------------------
    //

    //------------------------------------------------------------------------
    public class DoHouseWork : State<MinersWife>
    {
    
        public static DoHouseWork Instance = new DoHouseWork();

        public override void Enter(MinersWife wife)
        {
            DebugWide.LogGreen("wife: Time to do some more housework!");
        }

        public override void Execute(MinersWife wife)
        {
            switch (Misc.RandInt(0, 2))
            {
                case 0:
                
                    DebugWide.LogGreen("wife: Moppin' the floor");

                    break;

                case 1:
                
                    DebugWide.LogGreen("wife: Washin' the dishes");

                    break;

                case 2:

                    DebugWide.LogGreen("wife: Makin' the bed");

                    break;
            }
        }

        public override void Exit(MinersWife wife)
        {
        }

        public override bool OnMessage(MinersWife wife, Telegram msg)
        {
          return false;
        }

    }



    //------------------------------------------------------------------------
    //

    //------------------------------------------------------------------------
    class VisitBathroom : State<MinersWife>
    {
    
        public static VisitBathroom Instance = new VisitBathroom();

        public override void Enter(MinersWife wife)
        {
            DebugWide.LogGreen("wife: Walkin' to the can. Need to powda mah pretty li'lle nose");
        }


        public override void Execute(MinersWife wife)
        {
            DebugWide.LogGreen("wife: Ahhhhhh! Sweet relief!");

            wife.GetFSM().RevertToPreviousState();
        }

        public override void Exit(MinersWife wife)
        {
            DebugWide.LogGreen("wife: Leavin' the Jon");
        }


        public override bool OnMessage(MinersWife wife, Telegram msg)
        {
          return false;
        }

    }


    //------------------------------------------------------------------------
    //

    //------------------------------------------------------------------------
    public class CookStew : State<MinersWife>
    {

        public static CookStew Instance = new CookStew();

        public override void Enter(MinersWife wife)
        {
            //if not already cooking put the stew in the oven
            if (!wife.Cooking())
            {
                DebugWide.LogGreen("wife: Putting the stew in the oven");

                //send a delayed message myself so that I know when to take the stew
                //out of the oven
                SingleO.dispatcher.DispatchMessage(1.5f,                  //time delay
                                          wife.ID(),           //sender ID
                                          wife.ID(),           //receiver ID
                                          (int)msg_type.StewReady,        //msg
                                          null);

                wife.SetCooking(true);
            }
        }


        public override void Execute(MinersWife wife)
        {
            DebugWide.LogGreen("wife: Fussin' over food");
        }

        public override void Exit(MinersWife wife)
        {
            DebugWide.LogGreen("wife: Puttin' the stew on the table");
        }


        public override bool OnMessage(MinersWife wife, Telegram msg)
        {

            switch (msg.Msg)
            {
                case (int)msg_type.StewReady:
                    {


                        DebugWide.LogRed("Message received by wife:" + " at time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


                        DebugWide.LogGreen("wife: StewReady! Lets eat");

                        //let hubby know the stew is ready
                        SingleO.dispatcher.DispatchMessage(Telegram.SEND_MSG_IMMEDIATELY,
                                                  wife.ID(),
                                                  (int)entity_id.Miner_Bob,
                                                  (int)msg_type.StewReady,
                                                  null);

                        wife.SetCooking(false);

                        wife.GetFSM().ChangeState(DoHouseWork.Instance);
                    }
                    return true;

            }//end switch

            return false;
        }
    }//end class
}//end namespace
