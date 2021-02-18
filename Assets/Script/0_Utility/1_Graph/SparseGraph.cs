using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UtilGS9;

//성긴그래프

public class SparseGraph
{
	//a couple more typedefs to save my fingers and to help with the formatting
	//of the code on the printed page
	public class NodeVector : List<NavGraphNode> {}
	public class EdgeList : LinkedList<NavGraphEdge> {}
	public class EdgeListVector : List<EdgeList> {}

	//public LinkedList<string> a = new LinkedList<string> ();
		
	//the nodes that comprise this graph
	protected NodeVector      m_Nodes = new NodeVector();

    //벡터의 인접엣지리스트
    //a vector of adjacency edge lists. (each node index keys into the 
    //list of edges associated with that node)
    protected EdgeListVector  m_Edges = new EdgeListVector();


    //다이그래프로 설정할 것인가 지정
    //=> 다이그래프란? 엣지의 방향성이 있는 그래프이다.
    //is this a directed graph?
    protected bool            m_bDigraph;

    //the index of the next node to be added
    protected int             m_iNextNodeIndex;


    public List<NavGraphNode> GetListNodes() { return m_Nodes; }
    public EdgeListVector GetListEdges() { return m_Edges; }


    //returns true if an edge is not already present in the graph. Used
    //when adding edges to make sure no duplicates are created.
    protected bool  UniqueEdge(int from, int to)
	{

		foreach(NavGraphEdge iter in m_Edges[from])
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
    protected void  CullInvalidEdges()
	{
		foreach (EdgeList list in m_Edges) 
		{
			foreach (NavGraphEdge curEdge in list) 
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

	public NavGraphNode FindNearNode(Vector3 pos)
	{
        Dictionary<int , float> distance_List = new Dictionary<int , float> ();
        NavGraphNode findNode = null;

        int index = 0;
		foreach (NavGraphNode node in m_Nodes) 
		{
			if(null != node)
			{
				distance_List.Add(index, (pos - node.Pos()).sqrMagnitude);
			}
			index++;
		}

		distance_List = distance_List.OrderBy (x=> x.Value).ToDictionary(x=>x.Key, x=>x.Value);
		foreach (int nodeNum in distance_List.Keys) 
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
	public NavGraphNode GetNode(int idx)
	{
		//Assert.IsTrue( (idx < m_Nodes.Count) &&
		       //(idx >=0),             
		       //"<SparseGraph::GetNode>: invalid index");
		
		return m_Nodes[idx];
	}
	
	//const method for obtaining a reference to an edge
	public NavGraphEdge GetEdge(int from, int to)
	{
		//Assert.IsTrue( (from < m_Nodes.Count) &&
		       //(from >=0)              &&
		       //m_Nodes[from].Index() != GraphNode.INVALID_NODE_INDEX ,
		       //"<SparseGraph::GetEdge>: invalid 'from' index");
		
		//Assert.IsTrue( (to < m_Nodes.Count) &&
		       //(to >=0)              &&
		       //m_Nodes[to].Index() != GraphNode.INVALID_NODE_INDEX ,
		       //"<SparseGraph::GetEdge>: invalid 'to' index");

		foreach(NavGraphEdge curEdge in m_Edges[from])
		{
			if (curEdge.To() == to) return curEdge;
		}

		//Assert.IsTrue (false , "<SparseGraph::GetEdge>: edge does not exist");

		return null;
	}
	
	public EdgeList GetEdges(int node)
	{
		//Assert.IsTrue( (node < m_Nodes.Count) &&
		              //(node >=0)              &&
		              //m_Nodes[node].Index() != GraphNode.INVALID_NODE_INDEX ,
		              //"<SparseGraph::GetEdges>: invalid 'node' index");

		return m_Edges [node];
	}
	
	
	//retrieves the next free node index
	public int   GetNextFreeNodeIndex() {return m_iNextNodeIndex;}
	
	//adds a node to the graph and returns its index
	public int   AddNode(NavGraphNode node)
	{
		if (node.Index() < (int)m_Nodes.Count)
		{
			//make sure the client is not trying to add a node with the same ID as
			//a currently active node
			//Assert.IsTrue (m_Nodes[node.Index()].Index() == GraphNode.INVALID_NODE_INDEX ,
			        //"<SparseGraph::AddNode>: Attempting to add a node with a duplicate ID");
			
			m_Nodes[node.Index()] = node;
			
			return m_iNextNodeIndex;
		}
		
		else
		{
			//make sure the new node has been indexed correctly
            //Assert.IsTrue (node.Index() == m_iNextNodeIndex , "<SparseGraph::AddNode>:invalid index " + node.Index() + " != "+ m_iNextNodeIndex);

			m_Nodes.Add(node);
			m_Edges.Add(new EdgeList());
			
			return m_iNextNodeIndex++;
		}
	}
	
	//removes a node by setting its index to invalid_node_index
	public void  RemoveNode(int node)
	{
		//Assert.IsTrue(node < (int)m_Nodes.Count , "<SparseGraph::RemoveNode>: invalid node index");
		
		//set this node's index to invalid_node_index
		m_Nodes[node].SetIndex(GraphNode.INVALID_NODE_INDEX);
		
		//if the graph is not directed remove all edges leading to this node and then
		//clear the edges leading from the node
		if (!m_bDigraph)
		{    
			//visit each neighbour and erase any edges leading to this node
			foreach (NavGraphEdge curEdge in m_Edges[node])
			{
				foreach (NavGraphEdge curE in m_Edges[curEdge.To()])
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
	public void  AddEdge(NavGraphEdge edge)
	{
		//first make sure the from and to nodes exist within the graph 
		//Assert.IsTrue( (edge.From() < m_iNextNodeIndex) && (edge.To() < m_iNextNodeIndex) ,
		              //"<SparseGraph::AddEdge>: invalid node index" + " edge:" + edge.From() + "->" + edge.To() + " NextNode : " + m_iNextNodeIndex);
		
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
                    NavGraphEdge NewEdge = edge.Clone() as NavGraphEdge;
					
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
		//Assert.IsTrue ( (from < (int)m_Nodes.Count) && (to < (int)m_Nodes.Count) ,
		        //"<SparseGraph::RemoveEdge>:invalid node index");

		if (!m_bDigraph)
		{
			foreach(NavGraphEdge curEdge in m_Edges[to])
			{
				if (curEdge.To() == from){m_Edges[to].Remove(curEdge);break;}
			}
		}
		
		foreach(NavGraphEdge curEdge in m_Edges[from])
		{
			if (curEdge.To() == to){m_Edges[from].Remove(curEdge);break;}
		}
	}
	
	//sets the cost of an edge
	public void  SetEdgeCost(int from, int to, float cost)
	{
		//make sure the nodes given are valid
		//Assert.IsTrue( (from < m_Nodes.Count) && (to < m_Nodes.Count) ,
		       //"<SparseGraph::SetEdgeCost>: invalid index");
		
		//visit each neighbour and erase any edges leading to this node
		foreach(NavGraphEdge curEdge in m_Edges[from])
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
		if ((nd >= (int)m_Nodes.Count || nd < 0 || (m_Nodes[nd].Index() == GraphNode.INVALID_NODE_INDEX)))
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
			foreach(NavGraphEdge curEdge in m_Edges[from])
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

    public bool Load_Nav(StreamReader stream)
    {
        Clear();

        string line;
        line = stream.ReadLine();
        string[] sp = HandyString.SplitBlank(line);
        int numNodes = int.Parse(sp[0]);
        //get the number of nodes and read them in
        for (int n = 0; n < numNodes; ++n)
        {
            line = stream.ReadLine();
            NavGraphNode newNode = new NavGraphNode(line);

            //when editing graphs it's possible to end up with a situation where some
            //of the nodes have been invalidated (their id's set to invalid_node_index). Therefore
            //when a node of index invalid_node_index is encountered, it must still be added.
            if (newNode.Index() != GraphNode.INVALID_NODE_INDEX)
            {
                AddNode(newNode);
            }
            else
            {
                m_Nodes.Add(newNode);

                //make sure an edgelist is added for each node
                m_Edges.Add(new EdgeList());

                ++m_iNextNodeIndex;
            }
        }

        line = stream.ReadLine();
        sp = HandyString.SplitBlank(line);
        int numEdges = int.Parse(sp[0]);
        //now add the edges
        for (int e = 0; e < numEdges; ++e)
        {
            line = stream.ReadLine();
            NavGraphEdge NextEdge = new NavGraphEdge(line);

            AddEdge(NextEdge);
        }

        return true;
    }

}

////<일반화 인수의 대입에러에 대하여 >
//public class BBB
//{ }
//public class DDD : BBB
//{ }

//public class TTT<type> where type : DDD
//{
//    public void Add(type t) { }
//    public void Main()
//    {
//        Add(new DDD()); //일반화 형이 정해지지 않은 상태에서의 인수대입은 에러가 발생
//    }
//}

//public class MMM
//{
//    public void dddd()
//    {
//        TTT<DDD> t = new TTT<DDD>();
//        t.Add(new DDD()); //일반화 형이 정해진 후에는 인수대입이 가능 
//    }
//}


