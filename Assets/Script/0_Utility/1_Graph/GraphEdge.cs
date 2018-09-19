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
	protected 	float  m_dCost;
		
		//ctors
	public	GraphEdge(int from, int to, float cost)
	{
		m_dCost = cost;
		m_iFrom = from;
		m_iTo = to;
	}
	
	public GraphEdge(int from, int  to)
	{
		m_dCost = 1.0f;
		m_iFrom = from;
		m_iTo = to;
	}
	
	public GraphEdge()
	{
		m_dCost = 1.0f;
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
	
	public float Cost() {return m_dCost;}
	public void  SetCost(float NewCost){m_dCost = NewCost;}
	
	//these two operators are required
	public static bool operator ==(GraphEdge rhsA, GraphEdge rhsB)
	{
        //https://code.i-harness.com/ko/q/11ff1
        //무한 재귀없이 '=='연산자 오버로드에서 null을 확인

        //object로 형변환 안하면 무한재귀로 강제종료된다. 
        if (null == (object)rhsA)
            return null == (object)rhsB;

        return rhsA.Equals(rhsB);
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
        return (m_iFrom == p.m_iFrom) && (m_iTo == p.m_iTo) && m_dCost.Equals(p.m_dCost);
	}
	
	public bool Equals(GraphEdge p)
	{
		// If parameter is null return false:
		if ((object)p == null)
		{
			return false;
		}
		
		// Return true if the fields match:
        return (m_iFrom == p.m_iFrom) && (m_iTo == p.m_iTo) && m_dCost.Equals(p.m_dCost);
	}
	
	public override int GetHashCode()
	{
		return m_iFrom ^ m_iTo;
	}

	public object Clone()
	{
		GraphEdge edge = this.MemberwiseClone () as GraphEdge;

		return edge;
	}

}

