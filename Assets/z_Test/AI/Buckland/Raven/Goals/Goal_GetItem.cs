using Buckland;

namespace Raven
{
    public class Goal_GetItem : Goal_Composite<Raven_Bot>
    {

        int m_iItemToGet;

        Trigger<Raven_Bot> m_pGiverTrigger;

        //true if a path to the item has been formulated
        bool m_bFollowingPath;

        //returns true if the bot sees that the item it is heading for has been
        //picked up by an opponent
        bool hasItemBeenStolen()
        {
            if (null != m_pGiverTrigger &&
                !m_pGiverTrigger.isActive() &&
                m_pOwner.hasLOSto(m_pGiverTrigger.Pos()))
            {
                return true;
            }

            return false;
        }

        static int ItemTypeToGoalType(int gt)
        {
            switch (gt)
            {
                case (int)eObjType.health:

                    return (int)eGoal.get_health;

                case (int)eObjType.shotgun:

                    return (int)eGoal.get_shotgun;

                case (int)eObjType.rail_gun:

                    return (int)eGoal.get_railgun;

                case (int)eObjType.rocket_launcher:

                    return (int)eGoal.get_rocket_launcher;

                    //default: throw std::runtime_error("Goal_GetItem cannot determine item type");

            }//end switch

            DebugWide.LogRed("Goal_GetItem cannot determine item type");
            return -1;
        }

        public Goal_GetItem(Raven_Bot pBot, int item) : base(pBot, ItemTypeToGoalType(item))
        {
            m_iItemToGet = item;
            m_pGiverTrigger = null;
            m_bFollowingPath = false;
        }


        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            m_pGiverTrigger = null;

            //request a path to the item
            m_pOwner.GetPathPlanner().RequestPathToItem(m_iItemToGet);

            //the bot may have to wait a few update cycles before a path is calculated
            //so for appearances sake it just wanders
            AddSubgoal(new Goal_Wander(m_pOwner));

        }

        override public int Process()
        {
            ActivateIfInactive();

            if (hasItemBeenStolen())
            {
                Terminate();
            }

            else
            {
                //process the subgoals
                m_iStatus = ProcessSubgoals();
            }

            return m_iStatus;
        }

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

                        //get the pointer to the item
                        m_pGiverTrigger = (Trigger<Raven_Bot>)(msg.ExtraInfo);

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

        override public void Terminate() { m_iStatus = (int)eStatus.completed; }
    }


}//end namespace

