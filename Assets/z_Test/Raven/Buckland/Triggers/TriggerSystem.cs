using System.Collections.Generic;

namespace Raven
{
    public class TriggerList : LinkedList<Trigger<Raven_Bot>> { }

    public class TriggerSystem
    {

        TriggerList m_Triggers = new TriggerList();


        //this method iterates through all the triggers present in the system and
        //calls their Update method in order that their internal state can be
        //updated if necessary. It also removes any triggers from the system that
        //have their m_bRemoveFromGame field set to true.
        void UpdateTriggers()
        {
            foreach (Trigger<Raven_Bot> curTrg in m_Triggers)
            {
                //remove trigger if dead
                if ((curTrg).isToBeRemoved())
                {
                    //delete* curTrg;

                    m_Triggers.Remove(curTrg);
                }
                else
                {
                    //update this trigger
                    (curTrg).Update();

                }
            }
        }

        //this method iterates through the container of entities passed as a
        //parameter and passes each one to the Try method of each trigger *provided*
        //the entity is alive and provided the entity is ready for a trigger update.
        void TryTriggers(LinkedList<Raven_Bot> entities)
        {
            //test each entity against the triggers
            foreach (Raven_Bot curEnt in entities)
            {
                //an entity must be ready for its next trigger update and it must be 
                //alive before it is tested against each trigger.
                if ((curEnt).isReadyForTriggerUpdate() && (curEnt).isAlive())
                {
                    foreach (Trigger<Raven_Bot> curTrg in m_Triggers)
                    {
                        (curTrg).Try(curEnt);
                    }
                }
            }
        }


        //this deletes any current triggers and empties the trigger list
        public void Clear()
        {
            foreach (Trigger<Raven_Bot> curTrg in m_Triggers)
            {
                //delete* curTrg;
            }

            m_Triggers.Clear();
        }

        //This method should be called each update-step of the game. It will first
        //update the internal state odf the triggers and then try each entity against
        //each active trigger to test if any should be triggered.

        public void Update(LinkedList<Raven_Bot> entities)
        {
            UpdateTriggers();
            TryTriggers(entities);
        }

        //this is used to register triggers with the TriggerSystem (the TriggerSystem
        //will take care of tidying up memory used by a trigger)
        public void Register(Trigger<Raven_Bot> trigger)
        {
            m_Triggers.AddLast(trigger);
        }

        //some triggers are required to be rendered (like giver-triggers for example)
        public void Render()
        {
            foreach (Trigger<Raven_Bot> curTrg in m_Triggers)
            {
                (curTrg).Render();
            }
        }

        public TriggerList GetTriggers() { return m_Triggers; }

    }
}

