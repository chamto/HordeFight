using UnityEngine;


namespace Raven
{
    public class Goal_NegotiateDoor : Goal_Composite<Raven_Bot>
    {

        PathEdge m_PathEdge;

        bool m_bLastEdgeInPath;


        public Goal_NegotiateDoor(Raven_Bot pBot,
                   PathEdge edge,
                   bool LastEdge) : base(pBot, (int)eGoal.negotiate_door)
        {
            m_PathEdge = edge;
            m_bLastEdgeInPath = LastEdge;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //if this goal is reactivated then there may be some existing subgoals that
            //must be removed
            RemoveAllSubgoals();

            //get the position of the closest navigable switch
            Vector3 posSw = m_pOwner.GetWorld().GetPosOfClosestSwitch(m_pOwner.Pos(),
                                                                    m_PathEdge.DoorID());

            //because goals are *pushed* onto the front of the subgoal list they must
            //be added in reverse order.

            //first the goal to traverse the edge that passes through the door
            AddSubgoal(new Goal_TraverseEdge(m_pOwner, m_PathEdge, m_bLastEdgeInPath));

            //next, the goal that will move the bot to the beginning of the edge that
            //passes through the door
            AddSubgoal(new Goal_MoveToPosition(m_pOwner, m_PathEdge.Source()));

            //finally, the Goal that will direct the bot to the location of the switch
            AddSubgoal(new Goal_MoveToPosition(m_pOwner, posSw));
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
    }

}//end namespace

