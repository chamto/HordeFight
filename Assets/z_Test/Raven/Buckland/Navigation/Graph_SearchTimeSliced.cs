using System.Collections.Generic;


namespace Raven
{

    public class EdgeType { }

    //these enums are used as return values from each search update method
    public enum eReturn { target_found, target_not_found, search_incomplete };

    //------------------------ Graph_SearchTimeSliced -----------------------------
    //
    // base class to define a common interface for graph search algorithms
    //-----------------------------------------------------------------------------
    public class Graph_SearchTimeSliced<T_Edge>
    {

        public enum eSearchType { AStar, Dijkstra };

        eSearchType m_SearchType;


        public Graph_SearchTimeSliced(eSearchType type)
        {
            m_SearchType = type;
        }

        //When called, this method runs the algorithm through one search cycle. The
        //method returns an enumerated value (target_found, target_not_found,
        //search_incomplete) indicating the status of the search
        public virtual int CycleOnce() { return -1; }

        //returns the vector of edges that the algorithm has examined
        public virtual List<T_Edge> GetSPT() { return null; }


        //returns the total cost to the target
        public virtual float GetCostToTarget() { return 0f; }

        //returns a list of node indexes that comprise the shortest path
        //from the source to the target
        public virtual LinkedList<int> GetPathToTarget() { return null; }

        //returns the path as a list of PathEdges
        public virtual Path GetPathAsPathEdges() { return null; }

        public eSearchType GetType() { return m_SearchType; }
    }

    //-------------------------- Graph_SearchAStar_TS -----------------------------
    //
    //  a A* class that enables a search to be completed over multiple update-steps
    //-----------------------------------------------------------------------------

    public class Graph_SearchAStar_TS<T_Node, T_Edge> : Graph_SearchTimeSliced<T_Edge> 
        where T_Node : NavGraphNode where T_Edge : NavGraphEdge
    {

        //create typedefs for the node and edge types used by the graph
        //typedef typename graph_type::EdgeType Edge;
        //typedef typename graph_type::NodeType Node;

        SparseGraph<T_Node, T_Edge> m_Graph;

        //indexed into my node. Contains the 'real' accumulative cost to that node
        List<float> m_GCosts;

        //indexed into by node. Contains the cost from adding m_GCosts[n] to
        //the heuristic cost from n to the target node. This is the vector the
        //iPQ indexes into.
        List<float> m_FCosts;

        List<T_Edge> m_ShortestPathTree;
        List<T_Edge> m_SearchFrontier;

        int m_iSource;
        int m_iTarget;

        //create an indexed priority queue of nodes. The nodes with the
        //lowest overall F cost (G+H) are positioned at the front.
        IndexedPriorityQLow<float> m_pPQ;

        public IHeuristic<T_Node, T_Edge> iHeuristic = null;

        public Graph_SearchAStar_TS(SparseGraph<T_Node, T_Edge> G,
                      int source, int target) :base(eSearchType.AStar)
  
        {
            m_Graph = G;
            m_ShortestPathTree = new List<T_Edge>(G.NumNodes());
            m_SearchFrontier = new List<T_Edge>(G.NumNodes());
            m_GCosts = new List<float>(G.NumNodes()); 
            m_FCosts = new List<float>(G.NumNodes());
            m_iSource = source;
            m_iTarget = target;

            for(int i=0;i< G.NumNodes();i++)
            {
                m_ShortestPathTree.Add(null);
                m_SearchFrontier.Add(null);
                m_GCosts.Add(0f);
                m_FCosts.Add(0f);
            }
            //create the PQ   
            m_pPQ = new IndexedPriorityQLow<float>(m_FCosts, m_Graph.NumNodes());

            //put the source node on the queue
            m_pPQ.insert(m_iSource);
        }

    
        //When called, this method pops the next node off the PQ and examines all
        //its edges. The method returns an enumerated value (target_found,
        //target_not_found, search_incomplete) indicating the status of the search
        override public int CycleOnce()
        {
            //if the PQ is empty the target has not been found
            if (m_pPQ.empty())
            {
                return (int)eReturn.target_not_found;
            }

            //get lowest cost node from the queue
            int NextClosestNode = m_pPQ.Pop();

            //put the node on the SPT
            m_ShortestPathTree[NextClosestNode] = m_SearchFrontier[NextClosestNode];

            //if the target has been found exit
            if (NextClosestNode == m_iTarget)
            {
                return (int)eReturn.target_found;
            }

            //now to test all the edges attached to this node
            foreach (T_Edge pE in m_Graph.GetEdges(NextClosestNode))
            {
                //calculate the heuristic cost from this node to the target (H)                       
                float HCost = iHeuristic.Calculate(m_Graph, m_iTarget, pE.To());

                //calculate the 'real' cost to this node from the source (G)
                float GCost = m_GCosts[NextClosestNode] + pE.Cost();

                //if the node has not been added to the frontier, add it and update
                //the G and F costs
                if (m_SearchFrontier[pE.To()] == null)
                {
                    m_FCosts[pE.To()] = GCost + HCost;
                    m_GCosts[pE.To()] = GCost;

                    m_pPQ.insert(pE.To());

                    m_SearchFrontier[pE.To()] = pE;
                }

                //if this node is already on the frontier but the cost to get here
                //is cheaper than has been found previously, update the node
                //costs and frontier accordingly.
                else if ((GCost < m_GCosts[pE.To()]) && (m_ShortestPathTree[pE.To()] == null))
                {
                    m_FCosts[pE.To()] = GCost + HCost;
                    m_GCosts[pE.To()] = GCost;

                    m_pPQ.ChangePriority(pE.To());

                    m_SearchFrontier[pE.To()] = pE;
                }
            }

            //there are still nodes to explore
            return (int)eReturn.search_incomplete;
        }

        //returns the vector of edges that the algorithm has examined
        override public List<T_Edge> GetSPT() {return m_ShortestPathTree;}

        //returns a vector of node indexes that comprise the shortest path
        //from the source to the target
        override public LinkedList<int> GetPathToTarget()
        {
            LinkedList<int> path = new LinkedList<int>();

            //just return an empty path if no target or no path found
            if (m_iTarget< 0)  return path;    

            int nd = m_iTarget;
            path.AddLast(nd);
                
            
            while ((nd != m_iSource) && (m_ShortestPathTree[nd] != null))
            {
                nd = m_ShortestPathTree[nd].From();
                path.AddFirst(nd);
          }

          return path;
        }

        //returns the path as a list of PathEdges
        override public Path GetPathAsPathEdges()
        {
            Path path = new Path();

            //just return an empty path if no target or no path found
            if (m_iTarget< 0)  return path;    

            int nd = m_iTarget;
            
            while ((nd != m_iSource) && (m_ShortestPathTree[nd] != null))
            {

                path.AddFirst(new PathEdge(m_Graph.GetNode(m_ShortestPathTree[nd].From()).Pos(),
                                     m_Graph.GetNode(m_ShortestPathTree[nd].To()).Pos(),
                                     m_ShortestPathTree[nd].Flags(),
                                     m_ShortestPathTree[nd].IDofIntersectingEntity()));

                nd = m_ShortestPathTree[nd].From();
            }

            return path;
        }

        //returns the total cost to the target
        override public float GetCostToTarget() {return m_GCosts[m_iTarget];}
    }

    //-----------------------------------------------------------------------------

    //-------------------------- Graph_SearchDijkstras_TS -------------------------
    //
    //  Dijkstra's algorithm class modified to spread a search over multiple
    //  update-steps
    //-----------------------------------------------------------------------------

    public class Graph_SearchDijkstras_TS<T_Node, T_Edge> : Graph_SearchTimeSliced<T_Edge>
            where T_Node : NavGraphNode where T_Edge : NavGraphEdge
    {

        //create typedefs for the node and edge types used by the graph
        //typedef typename graph_type::EdgeType Edge;
        //typedef typename graph_type::NodeType Node;

        SparseGraph<T_Node, T_Edge> m_Graph;

        //indexed into my node. Contains the accumulative cost to that node
        List<float> m_CostToThisNode;

        List<T_Edge> m_ShortestPathTree;
        List<T_Edge> m_SearchFrontier;

        int m_iSource;
        int m_iTarget;

        //create an indexed priority queue of nodes. The nodes with the
        //lowest overall F cost (G+H) are positioned at the front.
        IndexedPriorityQLow<float> m_pPQ;

        public Termination_Condition<T_Node, T_Edge> iCondition = null;

        public Graph_SearchDijkstras_TS(SparseGraph<T_Node, T_Edge> G,
                              int source, int target ) :base(eSearchType.Dijkstra)
      
        {
            m_Graph = G;
            m_ShortestPathTree = new List<T_Edge>(G.NumNodes());                              
            m_SearchFrontier = new List<T_Edge>(G.NumNodes());
            m_CostToThisNode = new List<float>(G.NumNodes());

            for (int i = 0; i < G.NumNodes(); i++)
            {
                m_ShortestPathTree.Add(null);
                m_SearchFrontier.Add(null);
                m_CostToThisNode.Add(0f);
            }

            m_iSource = source;
            m_iTarget = target;

            //create the PQ         ,
            m_pPQ = new IndexedPriorityQLow<float>(m_CostToThisNode, m_Graph.NumNodes());

            //put the source node on the queue
            m_pPQ.insert(m_iSource);
        }

    

        //When called, this method pops the next node off the PQ and examines all
        //its edges. The method returns an enumerated value (target_found,
        //target_not_found, search_incomplete) indicating the status of the search
        override public int CycleOnce()
        {
            //if the PQ is empty the target has not been found
            if (m_pPQ.empty())
            {
                return (int)eReturn.target_not_found;
            }

            //get lowest cost node from the queue
            int NextClosestNode = m_pPQ.Pop();

            //move this node from the frontier to the spanning tree
            m_ShortestPathTree[NextClosestNode] = m_SearchFrontier[NextClosestNode];

            //if the target has been found exit
            if (iCondition.isSatisfied(m_Graph, m_iTarget, NextClosestNode))
            {
                //make a note of the node index that has satisfied the condition. This
                //is so we can work backwards from the index to extract the path from
                //the shortest path tree.
                m_iTarget = NextClosestNode;

                return (int)eReturn.target_found;
            }

            //now to test all the edges attached to this node
            foreach (T_Edge pE in m_Graph.GetEdges(NextClosestNode))
            {
                //the total cost to the node this edge points to is the cost to the
                //current node plus the cost of the edge connecting them.
                float NewCost = m_CostToThisNode[NextClosestNode] + pE.Cost();

                //if this edge has never been on the frontier make a note of the cost
                //to get to the node it points to, then add the edge to the frontier
                //and the destination node to the PQ.
                if (m_SearchFrontier[pE.To()] == null)
                {
                    m_CostToThisNode[pE.To()] = NewCost;

                    m_pPQ.insert(pE.To());

                    m_SearchFrontier[pE.To()] = pE;
                }

                //else test to see if the cost to reach the destination node via the
                //current node is cheaper than the cheapest cost found so far. If
                //this path is cheaper, we assign the new cost to the destination
                //node, update its entry in the PQ to reflect the change and add the
                //edge to the frontier
                else if ((NewCost < m_CostToThisNode[pE.To()]) &&
                          (m_ShortestPathTree[pE.To()] == null))
                {
                    m_CostToThisNode[pE.To()] = NewCost;

                    //because the cost is less than it was previously, the PQ must be
                    //re-sorted to account for this.
                    m_pPQ.ChangePriority(pE.To());

                    m_SearchFrontier[pE.To()] = pE;
                }
            }

            //there are still nodes to explore
            return (int)eReturn.search_incomplete;
        }

        //returns the vector of edges that the algorithm has examined
        override public List<T_Edge> GetSPT() {return m_ShortestPathTree;}

        //returns a vector of node indexes that comprise the shortest path
        //from the source to the target
        override public LinkedList<int> GetPathToTarget()
        {
          
            LinkedList<int> path = new LinkedList<int>();

            //just return an empty path if no target or no path found
            if (m_iTarget< 0)  return path;    

            int nd = m_iTarget;

            path.AddLast(nd);
            
            while ((nd != m_iSource) && (m_ShortestPathTree[nd] != null))
            {
                nd = m_ShortestPathTree[nd].From();

                path.AddFirst(nd);
            }

            return path;
        }

        //returns the path as a list of PathEdges
        override public Path GetPathAsPathEdges()
        {

            Path path = new Path();

            //just return an empty path if no target or no path found
            if (m_iTarget< 0)  return path;    

            int nd = m_iTarget;
            
            while ((nd != m_iSource) && (m_ShortestPathTree[nd] != null))
            {
                path.AddFirst(new PathEdge(m_Graph.GetNode(m_ShortestPathTree[nd].From()).Pos(),
                                     m_Graph.GetNode(m_ShortestPathTree[nd].To()).Pos(),
                                     m_ShortestPathTree[nd].Flags(),
                                     m_ShortestPathTree[nd].IDofIntersectingEntity()));
            
                nd = m_ShortestPathTree[nd].From();
            }

            return path;
        }

        //returns the total cost to the target
        override public float GetCostToTarget() {return m_CostToThisNode[m_iTarget];}
    }


    public interface Termination_Condition<T_Node, T_Edge> where T_Node : NavGraphNode where T_Edge : NavGraphEdge
    {
        //bool isSatisfied(SparseGraph<NavGraphNode, NavGraphEdge> G, int target, int CurrentNodeIdx);
        bool isSatisfied(SparseGraph<T_Node, T_Edge> G, int target, int CurrentNodeIdx);
    }

    //--------------------------- FindNodeIndex -----------------------------------

    //the search will terminate when the currently examined graph node
    //is the same as the target node.
    public class FindNodeIndex<T_Node, T_Edge> : Termination_Condition<T_Node, T_Edge>
        where T_Node : NavGraphNode where T_Edge : NavGraphEdge
    {

        public bool isSatisfied(SparseGraph<T_Node, T_Edge> G, int target, int CurrentNodeIdx)
        {
            return CurrentNodeIdx == target;
        }
    }

    //--------------------------- FindActiveTrigger ------------------------------

    //the search will terminate when the currently examined graph node
    //is the same as the target node.

    public class FindActiveTrigger<T_Node, T_Edge> : Termination_Condition<T_Node, T_Edge>
        where T_Node : NavGraphNode where T_Edge : NavGraphEdge
    {
    
  
        public bool isSatisfied(SparseGraph<T_Node, T_Edge> G, int target, int CurrentNodeIdx)
        {
            bool bSatisfied = false;

            //get a reference to the node at the given node index
            NavGraphNode node = G.GetNode(CurrentNodeIdx);

            Trigger<Raven_Bot> val = node.ExtraInfo<Trigger<Raven_Bot>>();
            //if the extrainfo field is pointing to a giver-trigger, test to make sure 
            //it is active and that it is of the correct type.
            if ((val != null) && 
                 val.isActive() && 
                (val.EntityType() == target) )
            {    
              bSatisfied = true;
            }

            return bSatisfied;
        }
    }

}//end namespace

