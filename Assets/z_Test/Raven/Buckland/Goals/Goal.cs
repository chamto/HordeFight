using UnityEngine;


namespace Raven
{
    public class Goal<entity_type>
    {


        public enum eStatus { active = 0, inactive = 1, completed = 2, failed = 3 };


        //an enumerated type specifying the type of goal
        protected int m_iType;

        //a pointer to the entity that owns this goal
        protected entity_type m_pOwner;

        //an enumerated value indicating the goal's status (active, inactive,
        //completed, failed)
        protected int m_iStatus;


        // the following methods were created to factor out some of the commonality
        // in the implementations of the Process method()

        //if m_iStatus = inactive this method sets it to active and calls Activate()
        protected void ActivateIfInactive()
        {
            if (isInactive())
            {
                Activate();
            }
        }

        //if m_iStatus is failed this method sets it to inactive so that the goal
        //will be reactivated (and therefore re-planned) on the next update-step.
        protected void ReactivateIfFailed()
        {
            if (hasFailed())
            {
                m_iStatus = (int)eStatus.inactive;
            }
        }



        //note how goals start off in the inactive state
        public Goal(entity_type pE, int type)
        {
            m_iType = type;
            m_pOwner = pE;
            m_iStatus = (int)eStatus.inactive;
        }


        //logic to run when the goal is activated.
        public virtual void Activate() { }

        //logic to run each update-step
        public virtual int Process() { return 0; }

        //logic to run when the goal is satisfied. (typically used to switch
        //off any active steering behaviors)
        public virtual void Terminate() { }

        //goals can handle messages. Many don't though, so this defines a default
        //behavior
        public virtual bool HandleMessage(Telegram msg) { return false; }


        //a Goal is atomic and cannot aggregate subgoals yet we must implement
        //this method to provide the uniform interface required for the goal
        //hierarchy.
        public virtual void AddSubgoal(Goal<entity_type> g)
        {
            //throw std::runtime_error("Cannot add goals to atomic goals"); 
        }


        public bool isComplete() { return m_iStatus == (int)eStatus.completed; }
        public bool isActive() { return m_iStatus == (int)eStatus.active; }
        public bool isInactive() { return m_iStatus == (int)eStatus.inactive; }
        public bool hasFailed() { return m_iStatus == (int)eStatus.failed; }
        public int GetType() { return m_iType; }



        //this is used to draw the name of the goal at the specific position
        //used for debugging
        public virtual void RenderAtPos(Vector3 pos)
        {
            //pos.z -= 10;
            Color color = Color.green;
            if (isComplete()) color = Color.green;
            if (isInactive()) color = Color.black;
            if (hasFailed()) color = Color.red;
            if (isActive()) color = Color.blue;

            DebugWide.PrintText(pos, color, (eGoal)GetType() + "  _" + (eStatus)m_iStatus);
            //DebugWide.LogBlue(((eGoal)GetType()).ToString());

        }

        //used to render any goal specific information
        public virtual void Render() { }

    }


}//end namespace

