using UnityEngine;


namespace Raven
{
    public class Goal_Explore : Goal_Composite<Raven_Bot>
    {

        Vector3 m_CurrentDestination;

        //set to true when the destination for the exploration has been established
        bool m_bDestinationIsSet;


        public Goal_Explore(Raven_Bot pOwner) : base(pOwner, (int)eGoal.explore)
        {
            m_bDestinationIsSet = false;
        }


        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //if this goal is reactivated then there may be some existing subgoals that
            //must be removed
            RemoveAllSubgoals();

            if (!m_bDestinationIsSet)
            {
                //grab a random location
                m_CurrentDestination = m_pOwner.GetWorld().GetMap().GetRandomNodeLocation();

                m_bDestinationIsSet = true;
            }

            //and request a path to that position
            m_pOwner.GetPathPlanner().RequestPathToPosition(m_CurrentDestination);

            //the bot may have to wait a few update cycles before a path is calculated
            //so for appearances sake it simple ARRIVES at the destination until a path
            //has been found
            AddSubgoal(new Goal_SeekToPosition(m_pOwner, m_CurrentDestination));
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //process the subgoals
            m_iStatus = ProcessSubgoals();

            return m_iStatus;
        }

        override public void Terminate() { }

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

                        //clear any existing goals
                        RemoveAllSubgoals();

                        AddSubgoal(new Goal_FollowPath(m_pOwner,
                                                       m_pOwner.GetPathPlanner().GetPath()));

                        return true; //msg handled


                    case (int)eMsg.NoPathAvailable:

                        m_iStatus = (int)eStatus.failed;

                        return true; //msg handled

                    default: return false;
                }
            }

            //handled by subgoals
            return true;
        }
    }


}//end namespace

