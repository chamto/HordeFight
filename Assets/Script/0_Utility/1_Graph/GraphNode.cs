using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class GraphNode : System.ICloneable
{  
	//every node has an index. A valid index is >= 0
	protected 	int        m_iIndex;

	public const int INVALID_NODE_INDEX = -1;
		
	public GraphNode(){ m_iIndex = GraphNode.INVALID_NODE_INDEX;}
	public GraphNode(int idx){ m_iIndex = idx;}

	public int  Index() {return m_iIndex;}
	public void SetIndex(int NewIndex){m_iIndex = NewIndex;}

	public override string ToString()
	{
		return "" + m_iIndex;
	}

	public virtual object Clone()
	{
		GraphNode node = new GraphNode ();
		node.m_iIndex = this.m_iIndex;

		return (object)node;
	}

}


//-----------------------------------------------------------------------------
//
//  Graph node for use in creating a navigation graph.This node contains
//  the position of the node and a pointer to a BaseGameEntity... useful
//  if you want your nodes to represent health packs, gold mines and the like
//-----------------------------------------------------------------------------

public	class NavGraphNode : GraphNode
{
	
	//the node's position
	protected	Vector2     m_vPosition;
	
	//often you will require a navgraph node to contain additional information.
	//For example a node might represent a pickup such as armor in which
	//case m_ExtraInfo could be an enumerated value denoting the pickup type,
	//thereby enabling a search algorithm to search a graph for specific items.
	//Going one step further, m_ExtraInfo could be a pointer to the instance of
	//the item type the node is twinned with. This would allow a search algorithm
	//to test the status of the pickup during the search. 
	//protected object  m_ExtraInfo;
	
	
	public NavGraphNode(){}
	
	public NavGraphNode(int  idx, Vector2 pos):base(idx)
	{
		m_vPosition = pos;
	}
	
	public override string ToString ()
	{
		return "" + base.ToString () + " " + m_vPosition;
	}
	
	
	
	public Vector2   Pos() {return m_vPosition;}
	public void       SetPos(Vector2 NewPosition){m_vPosition = NewPosition;}
	
	//public object ExtraInfo() {return m_ExtraInfo;}
	//public void       SetExtraInfo(object info){m_ExtraInfo = info;}
	
	public  override object Clone()
	{
		NavGraphNode node = base.Clone () as NavGraphNode;
		node.m_vPosition = this.m_vPosition;
		
		return (object)node;
	}
	
}