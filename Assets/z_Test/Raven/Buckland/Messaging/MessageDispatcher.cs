using System.Collections.Generic;
using UnityEngine;


namespace Raven
{
    public class MessageDispatcher
    {

        //public static MessageDispatcher Instance = null;

        //a std::set is used as the container for the delayed messages
        //because of the benefit of automatic sorting and avoidance
        //of duplicates. Messages are sorted by their dispatch time.
        //std::set<Telegram> PriorityQ;
        private SortedSet<Telegram> PriorityQ = new SortedSet<Telegram>();

        //this method is utilized by DispatchMessage or DispatchDelayedMessages.
        //This method calls the message handling member function of the receiving
        //entity, pReceiver, with the newly created telegram
        private void Discharge(BaseGameEntity pReceiver, Telegram telegram)
        {
            if (!pReceiver.HandleMessage(telegram))
            {
                //telegram could not be handled
                DebugWide.LogRed("Message not handled");
            }
        }



        //---------------------------- DispatchMessage ---------------------------
        //
        //  given a message, a receiver, a sender and any time delay , this function
        //  routes the message to the correct agent (if no delay) or stores
        //  in the message queue to be dispatched at the correct time
        //------------------------------------------------------------------------
        public void DispatchMessage(float delay,
                                        int sender,
                                        int receiver,
                                        int msg,
                                        object ExtraInfo)
        {


            //get pointers to the sender and receiver
            BaseGameEntity pSender = SingleO.entityMgr.GetEntityFromID(sender);
            BaseGameEntity pReceiver = SingleO.entityMgr.GetEntityFromID(receiver);

            //make sure the receiver is valid
            if (null == pReceiver)
            {
                DebugWide.LogRed("Warning! No Receiver with ID of " + receiver + " found");

                return;
            }

            //create the telegram
            Telegram telegram = new Telegram(0, sender, receiver, msg, ExtraInfo);

            //if there is no delay, route telegram immediately                       
            if (delay <= 0.0f)
            {
                //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                DebugWide.LogWhite("\nInstant telegram dispatched at time: " + Time.time +
                " by " + pSender.ID() + " for " + pReceiver.ID() + ". Msg is " + ((eMsg)msg).ToString());
                //send the telegram to the recipient
                Discharge(pReceiver, telegram);
            }

            //else calculate the time when the telegram should be dispatched
            else
            {

                float CurrentTime = Time.time;

                telegram.DispatchTime = CurrentTime + delay;

                //and put it in the queue
                PriorityQ.Add(telegram);

                DebugWide.LogWhite("Delayed telegram from " + pSender.ID() + " recorded at time " + Time.time
                + " for " + pReceiver.ID() + ". Msg is " + ((eMsg)msg).ToString());
            }
        }

        //---------------------- DispatchDelayedMessages -------------------------
        //
        //  This function dispatches any telegrams with a timestamp that has
        //  expired. Any dispatched telegrams are removed from the queue
        //------------------------------------------------------------------------
        public void DispatchDelayedMessages()
        {

            //get current time

            float CurrentTime = Time.time;

            //now peek at the queue to see if any telegrams need dispatching.
            //remove all telegrams from the front of the queue that have gone
            //past their sell by date
            while (0 != PriorityQ.Count && PriorityQ.Min.DispatchTime < CurrentTime && PriorityQ.Min.DispatchTime > 0)
            {
                //read the telegram from the front of the queue
                Telegram telegram = PriorityQ.Min;

                //find the recipient
                BaseGameEntity pReceiver = SingleO.entityMgr.GetEntityFromID(telegram.Receiver);


                DebugWide.LogWhite("Queued telegram ready for dispatch: Sent to " + pReceiver.ID()
                    + ". Msg is " + ((eMsg)telegram.Msg).ToString());
                //send the telegram to the recipient
                Discharge(pReceiver, telegram);

                //remove it from the queue
                PriorityQ.Remove(telegram);
            }
        }//end class
    }
}

