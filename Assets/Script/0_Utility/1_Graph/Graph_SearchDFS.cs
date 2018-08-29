using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

//----------------------------- Graph_SearchDFS -------------------------------
//
//  class to implement a depth first search. 
//-----------------------------------------------------------------------------
public class Graph_SearchDFS
{
		
		//to aid legibility
	private enum Aid : int
	{
		visited = 0, 
		unvisited = 1, 
		no_parent_assigned = 2
	}
		
		//a reference to the graph to be searched
	private	 SparseGraph m_Graph = null;
	
	//this records the indexes of all the nodes that are visited as the
	//search progresses
	private List<int>  m_Visited = new List<int>();
	
	//this holds the route taken to the target. Given a node index, the value
	//at that index is the node's parent. ie if the path to the target is
	//3-8-27, then m_Route[8] will hold 3 and m_Route[27] will hold 8.
	private List<int>  m_Route = new List<int>();
	
	//As the search progresses, this will hold all the edges the algorithm has
	//examined. THIS IS NOT NECESSARY FOR THE SEARCH, IT IS HERE PURELY
	//TO PROVIDE THE USER WITH SOME VISUAL FEEDBACK
	private List<GraphEdge>  m_SpanningTree = new List<GraphEdge>();
	
	//the source and target node indices
	private int               m_iSource, m_iTarget;
	
	//true if a path to the target has been found
	private bool              m_bFound;
	
	//this method performs the DFS search
	private bool Search()
	{
		//create a std stack of edges
		Stack<GraphEdge> stack = new Stack<GraphEdge>();
		
		//create a dummy edge and put on the stack
		GraphEdge Dummy = new GraphEdge(m_iSource, m_iSource, 0);
		stack.Push(Dummy);
		
		//while there are edges in the stack keep searching
		while (0 != stack.Count)
		{
			//grab the next edge
			GraphEdge Next = stack.Peek();
			//Debug.Log(Next); //chamto test
			
			//remove the edge from the stack
			stack.Pop();
			
			//make a note of the parent of the node this edge points to
			m_Route[Next.To()] = Next.From();
			
			//put it on the tree. (making sure the dummy edge is not placed on the tree)
			if (Next != Dummy)
			{
				m_SpanningTree.Add(Next);
			}
			
			//and mark it visited
			m_Visited[Next.To()] = (int)Aid.visited;
			
			//if the target has been found the method can return success
			if (Next.To() == m_iTarget)
			{
				return true;
			}
			
			//push the edges leading from the node this edge points to onto
			//the stack (provided the edge does not point to a previously 
			//visited node)
			foreach(GraphEdge pE in m_Graph.GetEdges(Next.To()))
			{
				if (m_Visited[pE.To()] == (int)Aid.unvisited)
				{
					stack.Push(pE);
				}
			}
		}
		
		//no path to target
		return false;
	}

	public void Init( SparseGraph  graph,
		                int          source,
		                int          target )
	{  
		this.Clear ();

		m_Graph = graph;
		m_iSource = source;
		m_iTarget = target;
		m_bFound = false;

		m_Visited.Capacity = m_Graph.NumNodes ();
		m_Route.Capacity = m_Graph.NumNodes ();

		for (int i=0; i<m_Graph.NumNodes(); i++) 
		{
			//Debug.Log(m_Graph.NumNodes() + " : " + i); //chamto test
			m_Visited.Add((int)Aid.unvisited);
			m_Route.Add((int)Aid.no_parent_assigned);
			//m_Visited[i] = (int)Aid.unvisited;
			//m_Route[i] = (int)Aid.no_parent_assigned;
		}

		m_bFound = Search(); 
	}

	public void Clear()
	{
		m_Visited.Clear ();
		m_Route.Clear ();
		m_SpanningTree.Clear ();
	}
	
	//returns a vector containing pointers to all the edges the search has examined
	public List<GraphEdge> GetSearchTree() {return m_SpanningTree;}
	
	//returns true if the target node has been located
	public bool   Found() {return m_bFound;}
	
	//returns a vector of node indexes that comprise the shortest path
	//from the source to the target
	public List<int> GetPathToTarget()
	{
		List<int> path = new List<int>();
		
		//just return an empty path if no path to target found or if
		//no target has been specified
		if (!m_bFound || m_iTarget<0) return path;
		
		int nd = m_iTarget;

		path.Add(nd);
		
		while (nd != m_iSource)
		{
			nd = m_Route[nd];
			
			path.Add(nd);
		}
		
		return path;
	}
}
