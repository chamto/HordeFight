using UnityEngine;


namespace Raven
{
    public class Goal_MoveToPosition : Goal_Composite<Raven_Bot>
    {

        //the position the bot wants to reach
        Vector3 m_vDestination;


        public Goal_MoveToPosition(Raven_Bot pBot,
                      Vector3 pos) : base(pBot, (int)eGoal.move_to_position)
        {
            m_vDestination = pos;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            //requests a path to the target position from the path planner. Because, for
            //demonstration purposes, the Raven path planner uses time-slicing when 
            //processing the path requests the bot may have to wait a few update cycles
            //before a path is calculated. Consequently, for appearances sake, it just
            //seeks directly to the target position whilst it's awaiting notification
            //that the path planning request has succeeded/failed
            if (m_pOwner.GetPathPlanner().RequestPathToPosition(m_vDestination))
            {
                AddSubgoal(new Goal_SeekToPosition(m_pOwner, m_vDestination));
            }
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //process the subgoals
            m_iStatus = ProcessSubgoals();

            //if any of the subgoals have failed then this goal re-plans
            ReactivateIfFailed();

            return m_iStatus;
        }

        override public void Terminate() { }

        //this goal is able to accept messages
        override public bool HandleMessage(Telegram msg)
        {
            //first, pass the message down the goal hierarchy
            bool bHandled = ForwardMessageToFrontMostSubgoal(msg);

            //if the msg was not handled, test to see if this goal can handle it
            if (bHandled == false)
            {
                switch (msg.Msg)
                {
                    case (int)eMsg.PathReady:
                        {
                            //clear any existing goals
                            RemoveAllSubgoals();

                            AddSubgoal(new Goal_FollowPath(m_pOwner, m_pOwner.GetPathPlanner().GetPath()));

                            return true; //msg handled
                        }
                    case (int)eMsg.NoPathAvailable:
                        {
                            m_iStatus = (int)eStatus.failed;

                            return true; //msg handled
                        }
                    default:
                        return false;
                }
            }

            //handled by subgoals
            return true;
        }

        public void Render()
        {
            //forward the request to the subgoals
            base.Render();

            //draw a bullseye
            DebugWide.DrawCircle(m_vDestination, 6, Color.black);

            DebugWide.DrawCircle(m_vDestination, 4, Color.red);

            DebugWide.DrawCircle(m_vDestination, 2, Color.yellow);
        }
    }


}//end namespace

