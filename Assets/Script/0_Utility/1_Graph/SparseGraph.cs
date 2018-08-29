using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


//성긴그래프
//public class SparseGraph<T_Node, T_Edge> where T_Node : GraphNode  where T_Edge : GraphEdge
public class SparseGraph
{
	//a couple more typedefs to save my fingers and to help with the formatting
	//of the code on the printed page
	public class NodeVector : List<GraphNode> {}
	public class EdgeList : LinkedList<GraphEdge> {}
	public class EdgeListVector : List<EdgeList> {}

	//public LinkedList<string> a = new LinkedList<string> ();
		
	//the nodes that comprise this graph
	private NodeVector      m_Nodes = new NodeVector();

	//벡터의 인접엣지리스트
	//a vector of adjacency edge lists. (each node index keys into the 
	//list of edges associated with that node)
	private EdgeListVector  m_Edges = new EdgeListVector();

	//다이그래프로 설정할 것인가 지정
	//=> 다이그래프란? 엣지의 방향성이 있는 그래프이다.
	//is this a directed graph?
	private bool            m_bDigraph;
	
	//the index of the next node to be added
	private int             m_iNextNodeIndex;

	//returns true if an edge is not already present in the graph. Used
	//when adding edges to make sure no duplicates are created.
	private bool  UniqueEdge(int from, int to)
	{

		foreach(GraphEdge iter in m_Edges[from])
		{
			if(iter.To() == to)
			{
				return false;
			}
		}

		return true;
	}
	
	//iterates through all the edges in the graph and removes any that point
	//to an invalidated node
	private void  CullInvalidEdges()
	{
		foreach (EdgeList list in m_Edges) 
		{
			foreach (GraphEdge curEdge in list) 
			{
				if (m_Nodes[curEdge.To()].Index() == GraphNode.INVALID_NODE_INDEX || 
				    m_Nodes[curEdge.From()].Index() == GraphNode.INVALID_NODE_INDEX)
				{
					//chamto watching me : 리스트 순회중 리스트요소를 지우는게 가능한지 확인하기
					list.Remove(curEdge);
				}
			}
		}

	}
	
	
	public SparseGraph(bool digraph)
	{
		m_iNextNodeIndex = 0; 
		m_bDigraph = digraph;
	}

	//다른 노드와 연결이 없는 닫혀있는 노드인지 확인한다
	public bool ClosedNode(int node)
	{
		EdgeList list = this.GetEdges (node);
		if (null != list && 0 != list.Count) 
		{
			return false;
		}
		return true;
	}

	public NavGraphNode FindNearNode(Vector2 pos)
	{
		Dictionary<int , float> magList = new Dictionary<int , float> ();
		List<Vector2> sort = new List<Vector2> ();
		int index = 0;
		float outValue;
		NavGraphNode findNode = null;

		foreach (NavGraphNode node in m_Nodes) 
		{
			if(null != node)
			{
				magList.Add(index, (pos - node.Pos()).sqrMagnitude);
			}
			index++;
		}

		magList = magList.OrderBy (x=> x.Value).ToDictionary(x=>x.Key, x=>x.Value);
		foreach (int nodeNum in magList.Keys) 
		{
			if (false == this.ClosedNode(nodeNum)) 
			{
				findNode = m_Nodes.ElementAt(nodeNum) as NavGraphNode;
				break;
			}
		}


		return findNode;
	}

	//returns the node at the given index
	public  GraphNode  GetNode(int idx)
	{
		Assert.IsTrue( (idx < m_Nodes.Count) &&
		       (idx >=0),             
		       "<SparseGraph::GetNode>: invalid index");
		
		return m_Nodes[idx];
	}
	
	//const method for obtaining a reference to an edge
	public  GraphEdge GetEdge(int from, int to)
	{
		Assert.IsTrue( (from < m_Nodes.Count) &&
		       (from >=0)              &&
		       m_Nodes[from].Index() != GraphNode.INVALID_NODE_INDEX ,
		       "<SparseGraph::GetEdge>: invalid 'from' index");
		
		Assert.IsTrue( (to < m_Nodes.Count) &&
		       (to >=0)              &&
		       m_Nodes[to].Index() != GraphNode.INVALID_NODE_INDEX ,
		       "<SparseGraph::GetEdge>: invalid 'to' index");

		foreach(GraphEdge curEdge in m_Edges[from])
		{
			if (curEdge.To() == to) return curEdge;
		}

		Assert.IsTrue (false , "<SparseGraph::GetEdge>: edge does not exist");

		return null;
	}
	
	public EdgeList GetEdges(int node)
	{
		Assert.IsTrue( (node < m_Nodes.Count) &&
		              (node >=0)              &&
		              m_Nodes[node].Index() != GraphNode.INVALID_NODE_INDEX ,
		              "<SparseGraph::GetEdges>: invalid 'node' index");

		return m_Edges [node];
	}
	
	
	//retrieves the next free node index
	public int   GetNextFreeNodeIndex() {return m_iNextNodeIndex;}
	
	//adds a node to the graph and returns its index
	public int   AddNode(GraphNode node)
	{
		if (node.Index() < (int)m_Nodes.Count)
		{
			//make sure the client is not trying to add a node with the same ID as
			//a currently active node
			Assert.IsTrue (m_Nodes[node.Index()].Index() == GraphNode.INVALID_NODE_INDEX ,
			        "<SparseGraph::AddNode>: Attempting to add a node with a duplicate ID");
			
			m_Nodes[node.Index()] = node;
			
			return m_iNextNodeIndex;
		}
		
		else
		{
			//make sure the new node has been indexed correctly
			Assert.IsTrue (node.Index() == m_iNextNodeIndex , "<SparseGraph::AddNode>:invalid index");

			m_Nodes.Add(node);
			m_Edges.Add(new EdgeList());
			
			return m_iNextNodeIndex++;
		}
	}
	
	//removes a node by setting its index to invalid_node_index
	public void  RemoveNode(int node)
	{
		Assert.IsTrue(node < (int)m_Nodes.Count , "<SparseGraph::RemoveNode>: invalid node index");
		
		//set this node's index to invalid_node_index
		m_Nodes[node].SetIndex(GraphNode.INVALID_NODE_INDEX);
		
		//if the graph is not directed remove all edges leading to this node and then
		//clear the edges leading from the node
		if (!m_bDigraph)
		{    
			//visit each neighbour and erase any edges leading to this node
			foreach (GraphEdge curEdge in m_Edges[node])
			{
				foreach (GraphEdge curE in m_Edges[curEdge.To()])
				{
					if (curE.To() == node)
					{
						m_Edges[curEdge.To()].Remove(curE);

						break;
					}
				}
			}
			
			//finally, clear this node's edges
			m_Edges[node].Clear();
		}
		
		//if a digraph remove the edges the slow way
		else
		{
			CullInvalidEdges();
		}
	}
	
	//Use this to add an edge to the graph. The method will ensure that the
	//edge passed as a parameter is valid before adding it to the graph. If the
	//graph is a digraph then a similar edge connecting the nodes in the opposite
	//direction will be automatically added.
	public void  AddEdge(GraphEdge edge)
	{
		//first make sure the from and to nodes exist within the graph 
		Assert.IsTrue( (edge.From() < m_iNextNodeIndex) && (edge.To() < m_iNextNodeIndex) ,
		              "<SparseGraph::AddEdge>: invalid node index" + " edge:" + edge.From() + "->" + edge.To() + " NextNode : " + m_iNextNodeIndex);
		
		//make sure both nodes are active before adding the edge
		if ( (m_Nodes[edge.To()].Index() != GraphNode.INVALID_NODE_INDEX) && 
		    (m_Nodes[edge.From()].Index() != GraphNode.INVALID_NODE_INDEX))
		{
			//add the edge, first making sure it is unique
			if (UniqueEdge(edge.From(), edge.To()))
			{
				m_Edges[edge.From()].AddLast(edge);
			}
			
			//if the graph is undirected we must add another connection in the opposite
			//direction
			if (!m_bDigraph)
			{
				//check to make sure the edge is unique before adding
				if (UniqueEdge(edge.To(), edge.From()))
				{
					GraphEdge NewEdge = edge.Clone() as GraphEdge;
					
					NewEdge.SetTo(edge.From());
					NewEdge.SetFrom(edge.To());
					
					m_Edges[edge.To()].AddLast(NewEdge);
				}
			}
		}
	}
	
	//removes the edge connecting from and to from the graph (if present). If
	//a digraph then the edge connecting the nodes in the opposite direction 
	//will also be removed.
	public void  RemoveEdge(int from, int to)
	{
		Assert.IsTrue ( (from < (int)m_Nodes.Count) && (to < (int)m_Nodes.Count) ,
		        "<SparseGraph::RemoveEdge>:invalid node index");

		if (!m_bDigraph)
		{
			foreach(GraphEdge curEdge in m_Edges[to])
			{
				if (curEdge.To() == from){m_Edges[to].Remove(curEdge);break;}
			}
		}
		
		foreach(GraphEdge curEdge in m_Edges[from])
		{
			if (curEdge.To() == to){m_Edges[from].Remove(curEdge);break;}
		}
	}
	
	//sets the cost of an edge
	public void  SetEdgeCost(int from, int to, double cost)
	{
		//make sure the nodes given are valid
		Assert.IsTrue( (from < m_Nodes.Count) && (to < m_Nodes.Count) ,
		       "<SparseGraph::SetEdgeCost>: invalid index");
		
		//visit each neighbour and erase any edges leading to this node
		foreach(GraphEdge curEdge in m_Edges[from])
		{
			if (curEdge.To() == to)
			{
				curEdge.SetCost(cost);
				break;
			}
		}
	}
	
	//returns the number of active + inactive nodes present in the graph
	public int   NumNodes() {return m_Nodes.Count;}
	
	//returns the number of active nodes present in the graph (this method's
	//performance can be improved greatly by caching the value)
	public int   NumActiveNodes()
	{
		int count = 0;
		
		for (int n=0; n<m_Nodes.Count; ++n) if (m_Nodes[n].Index() != GraphNode.INVALID_NODE_INDEX) ++count;
		
		return count;
	}
	
	//returns the total number of edges present in the graph
	public int   NumEdges()
	{
		int tot = 0;

		foreach(EdgeList edgeList in m_Edges)
		{
			tot += edgeList.Count;
		}
		
		return tot;
	}
	
	//returns true if the graph is directed
	public bool  isDigraph(){return m_bDigraph;}
	
	//returns true if the graph contains no nodes
	public bool	isEmpty()
	{
		return (0 == m_Nodes.Count);
	}
	
	//returns true if a node with the given index is present in the graph
	public bool isNodePresent(int nd)
	{
		if ((nd >= (int)m_Nodes.Count || (m_Nodes[nd].Index() == GraphNode.INVALID_NODE_INDEX)))
		{
			return false;
		}
		else return true;
	}
	
	//returns true if an edge connecting the nodes 'to' and 'from'
	//is present in the graph
	public bool isEdgePresent(int from, int to)
	{
		if (isNodePresent(from) && isNodePresent(from))
		{
			foreach(GraphEdge curEdge in m_Edges[from])
			{
				if (curEdge.To() == to) return true;
			}
			
			return false;
		}
		else return false;
	}
	

	//clears the graph ready for new node insertions
	public void Clear(){m_iNextNodeIndex = 0; m_Nodes.Clear(); this.RemoveEdges (); m_Edges.Clear();}
	
	public void RemoveEdges()
	{
		foreach(EdgeList edgeList in m_Edges)
		{
			edgeList.Clear();
		}
	}

}


