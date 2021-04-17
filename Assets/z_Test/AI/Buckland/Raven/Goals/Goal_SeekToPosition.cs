using UnityEngine;


namespace Raven
{
    //======================================================
    public class Goal_SeekToPosition : Goal<Raven_Bot>
    {

        //the position the bot is moving to
        Vector3 m_vPosition;

        //the approximate time the bot should take to travel the target location
        float m_dTimeToReachPos;

        //this records the time this goal was activated
        float m_dStartTime;

        //returns true if a bot gets stuck
        bool isStuck()
        {

            float TimeTaken = Time.time - m_dStartTime;

            if (TimeTaken > m_dTimeToReachPos)
            {
                //debug_con << "BOT " << m_pOwner->ID() << " IS STUCK!!" << "";
                DebugWide.LogBlue("BOT " + m_pOwner.ID() + " IS STUCK!!");

                return true;
            }

            return false;
        }


        public Goal_SeekToPosition(Raven_Bot pBot, Vector3 target) : base(pBot, (int)eGoal.seek_to_position)
        {
            m_vPosition = target;
            m_dTimeToReachPos = 0f;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //record the time the bot starts this goal
            m_dStartTime = Time.time;

            //This value is used to determine if the bot becomes stuck 
            m_dTimeToReachPos = m_pOwner.CalculateTimeToReachPosition(m_vPosition);

            //factor in a margin of error for any reactive behavior
            const float MarginOfError = 1.0f;

            m_dTimeToReachPos += MarginOfError;


            m_pOwner.GetSteering().SetTarget(m_vPosition);
            m_pOwner.GetSteering().SeekOn();
        }
        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //test to see if the bot has become stuck
            if (isStuck())
            {
                m_iStatus = (int)eStatus.failed;
            }

            //test to see if the bot has reached the waypoint. If so terminate the goal
            else
            {
                if (m_pOwner.isAtPosition(m_vPosition))
                {
                    m_iStatus = (int)eStatus.completed;
                }
            }

            return m_iStatus;
        }
        override public void Terminate()
        {
            m_pOwner.GetSteering().SeekOff();
            m_pOwner.GetSteering().ArriveOff();

            m_iStatus = (int)eStatus.completed;
        }

        override public void Render()
        {
            //DebugWide.LogBlue((eStatus)m_iStatus);
            if (m_iStatus == (int)eStatus.active)
            {
                DebugWide.DrawCircle(m_vPosition, 3, Color.green);
            }

            else if (m_iStatus == (int)eStatus.inactive)
            {
                DebugWide.DrawCircle(m_vPosition, 3, Color.red);
            }
        }
    }

    //public class NavGraph : SparseGraph
    //{
    //    public NavGraph(bool digraph) : base(digraph) {}
    //}

}//end namespace

