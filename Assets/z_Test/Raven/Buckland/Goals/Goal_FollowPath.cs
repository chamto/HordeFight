using UnityEngine;


namespace Raven
{
    public class Goal_FollowPath : Goal_Composite<Raven_Bot>
    {
        //a local copy of the path returned by the path planner
        Path m_Path;


        public Goal_FollowPath(Raven_Bot pBot, Path path) : base(pBot, (int)eGoal.seek_to_position)
        {
            m_Path = path;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //get a reference to the next edge
            PathEdge edge = m_Path.First.Value;

            //remove the edge from the path
            m_Path.RemoveFirst();


            //some edges specify that the bot should use a specific behavior when
            //following them. This switch statement queries the edge behavior flag and
            //adds the appropriate goals/s to the subgoal list.
            switch (edge.Behavior())
            {
                case (int)NavGraphEdge.eFlag.normal:
                    {
                        AddSubgoal(new Goal_TraverseEdge(m_pOwner, edge, 0 == m_Path.Count));
                    }

                    break;

                case (int)NavGraphEdge.eFlag.goes_through_door:
                    {

                        //also add a goal that is able to handle opening the door
                        AddSubgoal(new Goal_NegotiateDoor(m_pOwner, edge, 0 == m_Path.Count));
                    }

                    break;

                case (int)NavGraphEdge.eFlag.jump:
                    {
                        //add subgoal to jump along the edge
                    }

                    break;

                case (int)NavGraphEdge.eFlag.grapple:
                    {
                        //add subgoal to grapple along the edge
                    }

                    break;

                default:

                    //throw std::runtime_error("<Goal_FollowPath::Activate>: Unrecognized edge type");
                    DebugWide.LogError("<Goal_FollowPath::Activate>: Unrecognized edge type");
                    break;
            }
        }
        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            m_iStatus = ProcessSubgoals();

            //if there are no subgoals present check to see if the path still has edges.
            //remaining. If it does then call activate to grab the next edge.
            if (m_iStatus == (int)eStatus.completed && 0 != m_Path.Count)
            {
                Activate();
            }

            return m_iStatus;
        }
        override public void Render()
        {
            //render all the path waypoints remaining on the path list
            foreach (PathEdge it in m_Path)
            {

                DebugWide.DrawLine(it.Source(), it.Destination(), Color.black);

                DebugWide.DrawCircle(it.Destination(), 3, Color.black);
            }

            //forward the request to the subgoals
            base.Render();
        }
        override public void Terminate() { }
    }

}//end namespace

