using UnityEngine;


namespace Raven
{
    public class Goal_TraverseEdge : Goal<Raven_Bot>
    {

        //the edge the bot will follow
        PathEdge m_Edge;

        //true if m_Edge is the last in the path.
        bool m_bLastEdgeInPath;

        //the estimated time the bot should take to traverse the edge
        float m_dTimeExpected;

        //this records the time this goal was activated
        float m_dStartTime;

        //returns true if the bot gets stuck
        bool isStuck()
        {
            float TimeTaken = Time.time - m_dStartTime;

            if (TimeTaken > m_dTimeExpected)
            {
                DebugWide.LogBlue("BOT " + m_pOwner.ID() + " IS STUCK!!");

                return true;
            }

            return false;
        }


        public Goal_TraverseEdge(Raven_Bot pBot,
                                     PathEdge edge,
                                     bool LastEdge) : base(pBot, (int)eGoal.traverse_edge)
        {
            m_Edge = edge;
            m_dTimeExpected = 0.0f;
            m_bLastEdgeInPath = LastEdge;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //the edge behavior flag may specify a type of movement that necessitates a 
            //change in the bot's max possible speed as it follows this edge
            switch (m_Edge.Behavior())
            {
                case (int)NavGraphEdge.eFlag.swim:
                    {
                        m_pOwner.SetMaxSpeed(Params.Bot_MaxSwimmingSpeed);
                    }

                    break;

                case (int)NavGraphEdge.eFlag.crawl:
                    {
                        m_pOwner.SetMaxSpeed(Params.Bot_MaxCrawlingSpeed);
                    }

                    break;
            }


            //record the time the bot starts this goal
            m_dStartTime = Time.time;

            //calculate the expected time required to reach the this waypoint. This value
            //is used to determine if the bot becomes stuck 
            m_dTimeExpected = m_pOwner.CalculateTimeToReachPosition(m_Edge.Destination());

            //factor in a margin of error for any reactive behavior
            const float MarginOfError = 2.0f;

            m_dTimeExpected += MarginOfError;


            //set the steering target
            m_pOwner.GetSteering().SetTarget(m_Edge.Destination());

            //Set the appropriate steering behavior. If this is the last edge in the path
            //the bot should arrive at the position it points to, else it should seek
            if (m_bLastEdgeInPath)
            {
                m_pOwner.GetSteering().ArriveOn();
            }

            else
            {
                m_pOwner.GetSteering().SeekOn();
            }
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //if the bot has become stuck return failure
            if (isStuck())
            {
                m_iStatus = (int)eStatus.failed;
            }

            //if the bot has reached the end of the edge return completed
            else
            {
                if (m_pOwner.isAtPosition(m_Edge.Destination()))
                {
                    m_iStatus = (int)eStatus.completed;
                }
            }

            return m_iStatus;
        }

        override public void Terminate()
        {
            //turn off steering behaviors.
            m_pOwner.GetSteering().SeekOff();
            m_pOwner.GetSteering().ArriveOff();

            //return max speed back to normal
            m_pOwner.SetMaxSpeed(Params.Bot_MaxSpeed);
        }

        override public void Render()
        {
            if (m_iStatus == (int)eStatus.active)
            {
                DebugWide.DrawLine(m_pOwner.Pos(), m_Edge.Destination(), Color.blue);

                DebugWide.DrawCircle(m_Edge.Destination(), 3, Color.black);
            }
        }
    }

}//end namespace

