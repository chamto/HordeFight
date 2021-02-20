using System.Collections.Generic;


namespace Raven
{

    //======================================================

    public class Path : LinkedList<PathEdge> { }



    public class PathManager
    {

        //a container of all the active search requests
        LinkedList<Raven_PathPlanner> m_SearchRequests = new LinkedList<Raven_PathPlanner>();

        //this is the total number of search cycles allocated to the manager. 
        //Each update-step these are divided equally amongst all registered path
        //requests
        int m_iNumSearchCyclesPerUpdate;


        public PathManager(int NumCyclesPerUpdate)
        {
            m_iNumSearchCyclesPerUpdate = NumCyclesPerUpdate;
        }

        //every time this is called the total amount of search cycles available will
        //be shared out equally between all the active path requests. If a search
        //completes successfully or fails the method will notify the relevant bot
        public void UpdateSearches()
        {
            int NumCyclesRemaining = m_iNumSearchCyclesPerUpdate;

            //iterate through the search requests until either all requests have been
            //fulfilled or there are no search cycles remaining for this update-step.

            LinkedListNode<Raven_PathPlanner> curPath = m_SearchRequests.First;
            while (0 != NumCyclesRemaining && 0 != m_SearchRequests.Count)
            {
                NumCyclesRemaining--;
                //DebugWide.LogBlue(curPath.Value + "  " + m_SearchRequests.Count + "  " +  NumCyclesRemaining);
                //make one search cycle of this path request
                int result = (curPath).Value.CycleOnce();

                //if the search has terminated remove from the list
                if ((result == (int)eReturn.target_found) || (result == (int)eReturn.target_not_found))
                {
                    //remove this path from the path list
                    LinkedListNode<Raven_PathPlanner> next = curPath.Next;
                    m_SearchRequests.Remove(curPath);
                    curPath = next;
                    //DebugWide.LogGreen(curPath + "__" + m_SearchRequests.First + "__" + m_SearchRequests.Last + "__" + m_SearchRequests.Count);
                }
                //move on to the next
                else
                {
                    curPath = curPath.Next;
                    //DebugWide.LogBlue(curPath + "__" + m_SearchRequests.First + "__" + m_SearchRequests.Last + "__" + m_SearchRequests.Count);
                }

                //the iterator may now be pointing to the end of the list. If this is so,
                // it must be reset to the beginning.
                if (curPath == null)
                {
                    curPath = m_SearchRequests.First;
                }

            }//end while
        }

        //a path planner should call this method to register a search with the 
        //manager. (The method checks to ensure the path planner is only registered
        //once)
        public void Register(Raven_PathPlanner pPathPlanner)
        {
            //make sure the bot does not already have a current search in the queue
            if (false == m_SearchRequests.Contains(pPathPlanner))
            {
                //add to the list
                m_SearchRequests.AddLast(pPathPlanner);
            }
        }

        public void UnRegister(Raven_PathPlanner pPathPlanner)
        {
            m_SearchRequests.Remove(pPathPlanner);

        }

        //returns the amount of path requests currently active.
        public int GetNumActiveSearches() { return m_SearchRequests.Count; }
    }


}//end namespace

