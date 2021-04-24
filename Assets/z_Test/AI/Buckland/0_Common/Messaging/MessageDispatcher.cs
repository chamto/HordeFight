using System.Collections.Generic;
using UnityEngine;


namespace Buckland
{

    public class MessageDispatcher
    {

        //a std::set is used as the container for the delayed messages
        //because of the benefit of automatic sorting and avoidance
        //of duplicates. Messages are sorted by their dispatch time.
        private SortedSet<Telegram> PriorityQ = new SortedSet<Telegram>();


        private static MessageDispatcher _instance = null;
        public static MessageDispatcher Instance()
        {
            if (null == _instance)
                _instance = new MessageDispatcher();

            return _instance;
        }

        public static void Init()
        {
            _instance = new MessageDispatcher(); 
        }

        //this method is utilized by DispatchMsg or DispatchDelayedMessages.
        //This method calls the message handling member function of the receiving
        //entity, pReceiver, with the newly created telegram
        void Discharge(BaseGameEntity pReceiver, Telegram telegram)
        {
          if (!pReceiver.HandleMessage(telegram))
          {
                //telegram could not be handled
                //#ifdef SHOW_MESSAGING_INFO
                DebugWide.LogRed("Message not handled");
                //#endif
            }
        }



        //send a message to another agent. Receiving agent is referenced by ID.
        public void DispatchMsg(float delay,
                                    int sender,
                                    int receiver,
                                    int msg,
                                    object AdditionalInfo)
        {

            //get a pointer to the receiver
            BaseGameEntity pReceiver =  EntityManager.Instance().GetEntityFromID(receiver);

            //make sure the receiver is valid
            if (pReceiver == null)
            {
                DebugWide.LogRed("Warning! No Receiver with ID of " + receiver + " found");
                return;
            }

            //create the telegram
            Telegram telegram = new Telegram(0, sender, receiver, msg, AdditionalInfo);

            //if there is no delay, route telegram immediately                       
            if (delay <= 0.0f)
            {
                //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                //DebugWide.LogWhite("\nInstant telegram dispatched at time: " + Time.time +
                //" by " + sender + " for " + receiver + ". Msg is " + pReceiver.ToStringMessage(msg));
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

                DebugWide.LogWhite("Delayed telegram from " + sender + " recorded at time " + Time.time
                + " for " + receiver + ". Msg is " + pReceiver.ToStringMessage(msg));
            }
        }

        //send out any delayed messages. This method is called each time through   
        //the main game loop.
        public void DispatchDelayedMessages()
        {
            float CurrentTime = Time.time;

            //now peek at the queue to see if any telegrams need dispatching.
            //remove all telegrams from the front of the queue that have gone
            //past their sell by date
            while (0 != PriorityQ.Count && PriorityQ.Min.DispatchTime < CurrentTime && PriorityQ.Min.DispatchTime > 0)
            {
                //read the telegram from the front of the queue
                Telegram telegram = PriorityQ.Min;

                //find the recipient
                BaseGameEntity pReceiver = EntityManager.Instance().GetEntityFromID(telegram.Receiver);


                DebugWide.LogWhite("Queued telegram ready for dispatch: Sent to " + pReceiver.ID()
                    + ". Msg is " + telegram.Msg);
                //send the telegram to the recipient
                Discharge(pReceiver, telegram);

                //remove it from the queue
                PriorityQ.Remove(telegram);
            }
        }
    }

}

