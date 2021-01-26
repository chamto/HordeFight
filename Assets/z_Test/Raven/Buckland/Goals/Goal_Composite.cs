using System.Collections.Generic;
using UnityEngine;


namespace Raven
{
    //<<<< 확장메소드 >>>> 
    //https://stackoverflow.com/questions/10231457/what-does-this-refer-to-in-a-c-sharp-method-signature-and-is-there-a-vb-net-eq
    //https://stackoverflow.com/questions/1211608/possible-to-iterate-backwards-through-a-foreach
    //https://stackoverflow.com/questions/8193806/how-to-traverse-c-sharp-linkedlist-in-reverse-order
    public static class Inner
    {
        //"this" signature : this 표시로 확장메서드를 만든다. 확장메서드는 상속,변환 없이도 객체에 메소드를 추가할 수 있다 
        public static IEnumerable<Goal<entity_type>> Reverse<entity_type>(this LinkedList<Goal<entity_type>> list)
        {
            var el = list.Last;
            while (el != null)
            {
                yield return el.Value;
                el = el.Previous;
            }
        }

        //사용예 
        //foreach (Goal<entity_type> g in m_SubGoals.Reverse<entity_type>()) { }
    }


    public class Goal_Composite<entity_type> : Goal<entity_type>
    {

        public class SubgoalList : LinkedList<Goal<entity_type>> { }

        //composite goals may have any number of subgoals
        protected SubgoalList m_SubGoals = new SubgoalList();


        //-------------------------- ProcessSubGoals ----------------------------------
        //
        //  this method first removes any completed goals from the front of the
        //  subgoal list. It then processes the next goal in the list (if there is one)
        //-----------------------------------------------------------------------------
        protected int ProcessSubgoals()
        {
            //remove all completed and failed goals from the front of the subgoal list
            while (0 != m_SubGoals.Count && (m_SubGoals.First.Value.isComplete() || m_SubGoals.First.Value.hasFailed()))
            {

                m_SubGoals.First.Value.Terminate();
                m_SubGoals.RemoveFirst();

                //m_SubGoals.front()->Terminate();
                //delete m_SubGoals.front();
                //m_SubGoals.pop_front();
            }

            //if any subgoals remain, process the one at the front of the list
            if (0 != m_SubGoals.Count)
            {
                //grab the status of the front-most subgoal
                int StatusOfSubGoals = m_SubGoals.First.Value.Process();

                //we have to test for the special case where the front-most subgoal
                //reports 'completed' *and* the subgoal list contains additional goals.When
                //this is the case, to ensure the parent keeps processing its subgoal list
                //we must return the 'active' status.
                if (StatusOfSubGoals == (int)eStatus.completed && m_SubGoals.Count > 1)
                {
                    return (int)eStatus.active;
                }

                return StatusOfSubGoals;
            }

            //no more subgoals to process - return 'completed'
            else
            {
                return (int)eStatus.completed;
            }
        }

        //passes the message to the front-most subgoal
        protected bool ForwardMessageToFrontMostSubgoal(Telegram msg)
        {
            if (0 != m_SubGoals.Count)
            {
                return m_SubGoals.First.Value.HandleMessage(msg);
            }

            //return false if the message has not been handled
            return false;
        }


        public Goal_Composite(entity_type pE, int type) : base(pE, type) { }

        //when this object is destroyed make sure any subgoals are terminated
        //and destroyed.
        //virtual ~Goal_Composite() { RemoveAllSubgoals(); }

        //logic to run when the goal is activated.
        public virtual void Activate() { }

        //logic to run each update-step.
        public virtual int Process() { return 0; }

        //logic to run prior to the goal's destruction
        public virtual void Terminate() { }

        //if a child class of Goal_Composite does not define a message handler
        //the default behavior is to forward the message to the front-most
        //subgoal
        public virtual bool HandleMessage(Telegram msg)
        { return ForwardMessageToFrontMostSubgoal(msg); }

        //adds a subgoal to the front of the subgoal list
        public void AddSubgoal(Goal<entity_type> g)
        {
            //add the new goal to the front of the list
            m_SubGoals.AddFirst(g);
            //m_SubGoals.push_front(g);
        }

        //this method iterates through the subgoals and calls each one's Terminate
        //method before deleting the subgoal and removing it from the subgoal list
        public void RemoveAllSubgoals()
        {
            foreach (Goal<entity_type> g in m_SubGoals)
            {
                g.Terminate();
            }
            m_SubGoals.Clear();

        }


        public virtual void RenderAtPos(Vector3 pos)
        {
            base.RenderAtPos(pos);

            //pos.x += 10;

            //확장메소드를 이용하여 뒤집은 경우 
            //foreach (Goal<entity_type> el in m_SubGoals.Reverse<entity_type>())
            //{
            //    el.RenderAtPos(pos);
            //}

            //그냥 뒤집은 경우 
            var el = m_SubGoals.Last;
            while (el != null)
            {
                // use el.Value
                el.Value.RenderAtPos(pos);
                el = el.Previous;
            }


            //pos.x -= 10;
        }

        //this is only used to render information for debugging purposes
        public virtual void Render()
        {
            if (0 != m_SubGoals.Count)
            {
                m_SubGoals.First.Value.Render();
            }
        }
    }


}//end namespace

