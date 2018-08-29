using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class GraphEdge : System.ICloneable
{
	//An edge connects two nodes. Valid node indices are always positive.
	protected	int     m_iFrom;
	protected	int     m_iTo;
	
	//the cost of traversing the edge
	protected 	double  m_dCost;
		
		//ctors
	public	GraphEdge(int from, int to, double cost)
	{
		m_dCost = cost;
		m_iFrom = from;
		m_iTo = to;
	}
	
	public GraphEdge(int from, int  to)
	{
		m_dCost = 1.0;
		m_iFrom = from;
		m_iTo = to;
	}
	
	public GraphEdge()
	{
		m_dCost = 1.0;
		m_iFrom = GraphNode.INVALID_NODE_INDEX;
		m_iTo = GraphNode.INVALID_NODE_INDEX;
	}
	
	public override string ToString()
	{
		return "from:" + m_iFrom + " to:" + m_iTo + " cost:" + m_dCost;
	}

	public int   From() {return m_iFrom;}
	public void  SetFrom(int NewIndex){m_iFrom = NewIndex;}
	
	public int   To() {return m_iTo;}
	public void  SetTo(int NewIndex){m_iTo = NewIndex;}
	
	public double Cost() {return m_dCost;}
	public void  SetCost(double NewCost){m_dCost = NewCost;}
	
	//these two operators are required
	public static bool operator ==(GraphEdge rhsA, GraphEdge rhsB)
	{
		return rhsA.m_iFrom == rhsB.m_iFrom &&
			rhsA.m_iTo   == rhsB.m_iTo   &&
				rhsA.m_dCost == rhsB.m_dCost;
	}
	
	public static bool operator !=(GraphEdge rhsA, GraphEdge rhsB)
	{
		return !(rhsA == rhsB);
	}
	
	public override bool Equals(System.Object obj)
	{
		// If parameter is null return false.
		if (obj == null)
		{
			return false;
		}
		
		// If parameter cannot be cast to Point return false.
		GraphEdge p = obj as GraphEdge;
		if ((System.Object)p == null)
		{
			return false;
		}
		
		// Return true if the fields match:
		return (m_iFrom == p.m_iFrom) && (m_iTo == p.m_iTo);
	}
	
	public bool Equals(GraphEdge p)
	{
		// If parameter is null return false:
		if ((object)p == null)
		{
			return false;
		}
		
		// Return true if the fields match:
		return (m_iFrom == p.m_iFrom) && (m_iTo == p.m_iTo);
	}
	
	public override int GetHashCode()
	{
		return m_iFrom ^ m_iTo;
	}

	public object Clone()
	{
		GraphEdge edge = this.MemberwiseClone () as GraphEdge;
		return (object)edge;
	}

}

