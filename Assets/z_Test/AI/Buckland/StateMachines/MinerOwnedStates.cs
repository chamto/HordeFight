using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WestWorld
{
    //------------------------------------------------------------------------
    //
    //  In this state the miner will walk to a goldmine and pick up a nugget
    //  of gold. If the miner already has a nugget of gold he'll change state
    //  to VisitBankAndDepositGold. If he gets thirsty he'll change state
    //  to QuenchThirst
    //------------------------------------------------------------------------
    public class EnterMineAndDigForNugget : State<Miner>
    {

        public static EnterMineAndDigForNugget Instance = new EnterMineAndDigForNugget();

        public override void Enter(Miner pMiner)
        {
            //if the miner is not already located at the goldmine, he must
            //change location to the gold mine
            if (pMiner.Location() != location_type.goldmine)
            {

                DebugWide.LogBlue("Miner Bob: " + "Walkin' to the goldmine");

                pMiner.ChangeLocation(location_type.goldmine);
            }
        }


        public override void Execute(Miner pMiner)
        {
            //Now the miner is at the goldmine he digs for gold until he
            //is carrying in excess of MaxNuggets. If he gets thirsty during
            //his digging he packs up work for a while and changes state to
            //gp to the saloon for a whiskey.
            pMiner.AddToGoldCarried(1);

            pMiner.IncreaseFatigue();

            DebugWide.LogBlue("Miner Bob: " + "Pickin' up a nugget");

            //if enough gold mined, go and put it in the bank
            if (pMiner.PocketsFull())
            {
                pMiner.GetFSM().ChangeState(VisitBankAndDepositGold.Instance);
            }

            if (pMiner.Thirsty())
            {
                pMiner.GetFSM().ChangeState(QuenchThirst.Instance);
            }
        }


        public override void Exit(Miner pMiner)
        {

            DebugWide.LogBlue("Miner Bob: " + "Ah'm leavin' the goldmine with mah pockets full o' sweet gold");
        }


        public override bool OnMessage(Miner pMiner, Telegram msg)
        {
            //send msg to global message handler
            return false;
        }
    }

    //------------------------------------------------------------------------
    //
    //  Entity will go to a bank and deposit any nuggets he is carrying. If the 
    //  miner is subsequently wealthy enough he'll walk home, otherwise he'll
    //  keep going to get more gold
    //------------------------------------------------------------------------
    public class VisitBankAndDepositGold : State<Miner>
    {

        public static VisitBankAndDepositGold Instance = new VisitBankAndDepositGold();

        public override void Enter(Miner pMiner)
        {
            //on entry the miner makes sure he is located at the bank
            if (pMiner.Location() != location_type.bank)
            {

                DebugWide.LogBlue("Miner Bob: " + "Goin' to the bank. Yes siree");

                pMiner.ChangeLocation(location_type.bank);
            }
        }


        public override void Execute(Miner pMiner)
        {
            //deposit the gold
            pMiner.AddToWealth(pMiner.GoldCarried());

            pMiner.SetGoldCarried(0);

            DebugWide.LogBlue("Miner Bob: " + "Depositing gold. Total savings now: " + pMiner.Wealth());

            //wealthy enough to have a well earned rest?
            if (pMiner.Wealth() >= Const.ComfortLevel)
            {

                DebugWide.LogBlue("Miner Bob: " + "WooHoo! Rich enough for now. Back home to mah li'lle lady");

                pMiner.GetFSM().ChangeState(GoHomeAndSleepTilRested.Instance);
            }

            //otherwise get more gold
            else
            {
                pMiner.GetFSM().ChangeState(EnterMineAndDigForNugget.Instance);
            }
        }


        public override void Exit(Miner pMiner)
        {
            DebugWide.LogBlue("Miner Bob: " + "Leavin' the bank");
        }


        public override bool OnMessage(Miner pMiner, Telegram msg)
        {
            //send msg to global message handler
            return false;
        }
    }


    //------------------------------------------------------------------------
    //
    //  miner will go home and sleep until his fatigue is decreased
    //  sufficiently
    //------------------------------------------------------------------------
    public class GoHomeAndSleepTilRested : State<Miner>
    {

        public static GoHomeAndSleepTilRested Instance = new GoHomeAndSleepTilRested();

        public override void Enter(Miner pMiner)
        {
            if (pMiner.Location() != location_type.shack)
            {

                DebugWide.LogBlue("Miner Bob: " + "Walkin' home");

                pMiner.ChangeLocation(location_type.shack);

                //let the wife know I'm home
                SingleO.dispatcher.DispatchMessage(Telegram.SEND_MSG_IMMEDIATELY, pMiner.ID(), (int)entity_id.Elsa,
                    (int)msg_type.HiHoneyImHome, null);

            }
        }

        public override void Execute(Miner pMiner)
        {
            //if miner is not fatigued start to dig for nuggets again.
            if (!pMiner.Fatigued())
            {

                DebugWide.LogBlue("Miner Bob: " + "All mah fatigue has drained away. Time to find more gold!");

                pMiner.GetFSM().ChangeState(EnterMineAndDigForNugget.Instance);
            }

            else
            {
                //sleep
                pMiner.DecreaseFatigue();

                DebugWide.LogBlue("Miner Bob: " + "ZZZZ... ");
            }
        }

        public override void Exit(Miner pMiner)
        {
        }


        public override bool OnMessage(Miner pMiner, Telegram msg)
        {

            switch (msg.Msg)
            {
                case (int)msg_type.StewReady:
                    {
                        DebugWide.LogRed("Message handled by: Miner Bob: " + " at time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        DebugWide.LogBlue("Miner Bob: " + "Okay Hun, ahm a comin'!");

                        pMiner.GetFSM().ChangeState(EatStew.Instance);
                    }
                    return true;

            }//end switch

            return false; //send message to global message handler

        }
    }


    //------------------------------------------------------------------------
    //
    //  miner changes location to the saloon and keeps buying Whiskey until
    //  his thirst is quenched. When satisfied he returns to the goldmine
    //  and resumes his quest for nuggets.
    //------------------------------------------------------------------------
    public class QuenchThirst : State<Miner>
    {
        public static QuenchThirst Instance = new QuenchThirst();

        public override void Enter(Miner pMiner)
        {
            if (pMiner.Location() != location_type.saloon)
            {
                pMiner.ChangeLocation(location_type.saloon);

                DebugWide.LogBlue("Miner Bob: " + "Boy, ah sure is thusty! Walking to the saloon");
            }
        }

        public override void Execute(Miner pMiner)
        {
            pMiner.BuyAndDrinkAWhiskey();

            DebugWide.LogBlue("Miner Bob: " + "That's mighty fine sippin' liquer");

            pMiner.GetFSM().ChangeState(EnterMineAndDigForNugget.Instance);
        }


        public override void Exit(Miner pMiner)
        {
            DebugWide.LogBlue("Miner Bob: " + "Leaving the saloon, feelin' good");
        }


        public override bool OnMessage(Miner pMiner, Telegram msg)
        {
            //send msg to global message handler
            return false;
        }
    }


    //------------------------------------------------------------------------
    //
    //  this is implemented as a state blip. The miner eats the stew, gives
    //  Elsa some compliments and then returns to his previous state
    //------------------------------------------------------------------------
    public class EatStew : State<Miner>
    {
        public static EatStew Instance = new EatStew();

        public override void Enter(Miner pMiner)
        {
            DebugWide.LogBlue("Miner Bob: " + "Smells Reaaal goood Elsa!");
        }

        public override void Execute(Miner pMiner)
        {
            DebugWide.LogBlue("Miner Bob: " + "Tastes real good too!");

            pMiner.GetFSM().RevertToPreviousState();
        }

        public override void Exit(Miner pMiner)
        {
            DebugWide.LogBlue("Miner Bob: " + "Thankya li'lle lady. Ah better get back to whatever ah wuz doin'");
        }


        public override bool OnMessage(Miner pMiner, Telegram msg)
        {
            //send msg to global message handler
            return false;
        }
    }
}//end namespace
