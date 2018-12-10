using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//------------------------------- Graph_SearchAStar --------------------------
//
//  this searchs a graph using the distance between the target node and the 
//  currently considered node as a heuristic.
//
//  This search is more commonly known as A* (pronounced Ay-Star)
//-----------------------------------------------------------------------------
public class Graph_SearchAStar
{
 
    private SparseGraph              m_Graph;

    //indexed into my node. Contains the 'real' accumulative cost to that node
    private List<float> m_GCosts = new List<float>();

    //indexed into by node. Contains the cost from adding m_GCosts[n] to
    //the heuristic cost from n to the target node. This is the vector the
    //iPQ indexes into.
    private List<float> m_FCosts = new List<float>();

    private List<GraphEdge>       m_ShortestPathTree = new List<GraphEdge>();
    private List<GraphEdge>       m_SearchFrontier = new List<GraphEdge>();

    private int m_iSource;
    private int m_iTarget;

    private IHeuristic iHeuristic = null;

    //the A* search algorithm
    private void Search()
    {
        //create an indexed priority queue of nodes. The nodes with the
        //lowest overall F cost (G+H) are positioned at the front.
        IndexedPriorityQLow<float> pq = new IndexedPriorityQLow<float>(m_FCosts, m_Graph.NumNodes());

        //put the source node on the queue
        pq.insert(m_iSource);

        //while the queue is not empty
        while (!pq.empty())
        {
            //get lowest cost node from the queue
            int NextClosestNode = pq.Pop();

            //move this node from the frontier to the spanning tree
            m_ShortestPathTree[NextClosestNode] = m_SearchFrontier[NextClosestNode];

            //if the target has been found exit
            if (NextClosestNode == m_iTarget) return;

            //now to test all the edges attached to this node
            foreach(GraphEdge pE in m_Graph.GetEdges(NextClosestNode))
            {
                if (null == pE) continue;

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

                    pq.insert(pE.To());

                    m_SearchFrontier[pE.To()] = pE;
                }

                //if this node is already on the frontier but the cost to get here
                //is cheaper than has been found previously, update the node
                //costs and frontier accordingly.
                else if ((GCost < m_GCosts[pE.To()]) && (m_ShortestPathTree[pE.To()] == null))
                {
                    m_FCosts[pE.To()] = GCost + HCost;
                    m_GCosts[pE.To()] = GCost;

                    pq.ChangePriority(pE.To());

                    m_SearchFrontier[pE.To()] = pE;
                }
            }
        }
    }



    public void Init(SparseGraph graph, int source, int target)
    {

        this.Clear();

        m_Graph = graph;
        m_iSource = source;
        m_iTarget = target;
        iHeuristic = new Heuristic_Euclid();

        m_ShortestPathTree.Capacity = graph.NumNodes();
        m_SearchFrontier.Capacity = graph.NumNodes();
        m_GCosts.Capacity = graph.NumNodes(); //init value 0.0
        m_FCosts.Capacity = graph.NumNodes(); //init value 0.0

        for (int i = 0; i < graph.NumNodes();i++)
        {
            m_ShortestPathTree.Add(null);
            m_SearchFrontier.Add(null);
            m_GCosts.Add(0.0f);
            m_FCosts.Add(0.0f);
        }



        Search();
    }

    public void Clear()
    {
        m_ShortestPathTree.Clear();
        m_SearchFrontier.Clear();
        m_GCosts.Clear();
        m_FCosts.Clear();
    }

    //returns the vector of edges that the algorithm has examined
    public List<GraphEdge> GetSPT() 
    {
        return m_ShortestPathTree;
    }

    //returns a vector of node indexes that comprise the shortest path
    //from the source to the target
    public LinkedList<int> GetPathToTarget()
    {
        //just return an empty path if no target or no path found
        if (m_iTarget< 0)  return null;

        LinkedList<int> path = new LinkedList<int>();

        int nd = m_iTarget;

        //path.push_front(nd);
        path.AddFirst(nd);
            
        while ((nd != m_iSource) && (m_ShortestPathTree[nd] != null))
        {
            nd = m_ShortestPathTree[nd].From();

            //path.push_front(nd);
            path.AddFirst(nd);
        }

        return path;
    }

    //returns the total cost to the target
    public float GetCostToTarget()
    {
        return m_GCosts[m_iTarget];
    }
}


//-----------------------------------------------------------------------------
//
//  Name:   AStarHeuristicPolicies.h
//
//  Author: Mat Buckland (www.ai-junkie.com)
//
//  Desc:   class templates defining a heuristic policy for use with the A*
//          search algorithm
//-----------------------------------------------------------------------------




public interface IHeuristic 
{
    float Calculate(SparseGraph G, int nd1, int nd2);
}

//-----------------------------------------------------------------------------
//the euclidian heuristic (straight-line distance)
//-----------------------------------------------------------------------------
public class Heuristic_Euclid : IHeuristic
{
    
    //calculate the straight line distance from node nd1 to node nd2
    public float Calculate(SparseGraph G, int nd1, int nd2)
    {
        NavGraphNode node1 = G.GetNode(nd1) as NavGraphNode;
        NavGraphNode node2 = G.GetNode(nd2) as NavGraphNode;

        return Vector3.Distance(node1.Pos(), node2.Pos());
    }
}

//-----------------------------------------------------------------------------
//this uses the euclidian distance but adds in an amount of noise to the 
//result. You can use this heuristic to provide imperfect paths. This can
//be handy if you find that you frequently have lots of agents all following
//each other in single file to get from one place to another
//-----------------------------------------------------------------------------
public class Heuristic_Noisy_Euclidian : IHeuristic
{
  
    //calculate the straight line distance from node nd1 to node nd2
    public float Calculate(SparseGraph G, int nd1, int nd2)
    {
        NavGraphNode node1 = G.GetNode(nd1) as NavGraphNode;
        NavGraphNode node2 = G.GetNode(nd2) as NavGraphNode;

        //0.9~1.09 사이의 랜덤값을 node2에 곱합. 랜덤함수에서 최대값 1.1에 도달하지 못한다 
        return Vector3.Distance(node1.Pos(), node2.Pos() * (float)UtilGS9.Misc.RandInRange(0.9f, 1.1f));

        
    }
}

//-----------------------------------------------------------------------------
//you can use this class to turn the A* algorithm into Dijkstra's search.
//this is because Dijkstra's is equivalent to an A* search using a heuristic
//value that is always equal to zero.
//-----------------------------------------------------------------------------
class Heuristic_Dijkstra : IHeuristic
{
    public float Calculate(SparseGraph G, int nd1, int nd2)
    {
        return 0;
    }
}
