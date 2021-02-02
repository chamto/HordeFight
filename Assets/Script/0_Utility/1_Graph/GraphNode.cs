using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class GraphNode : System.ICloneable
{  
	//every node has an index. A valid index is >= 0
	protected 	int        m_iIndex;

	public const int INVALID_NODE_INDEX = -1;
		
	public GraphNode()
    { 
        m_iIndex = GraphNode.INVALID_NODE_INDEX;
    }
	public GraphNode(int idx)
    { 
        m_iIndex = idx;
    }

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
