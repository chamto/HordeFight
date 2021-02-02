using UnityEngine;
//-----------------------------------------------------------------------------
//
//  Graph node for use in creating a navigation graph.This node contains
//  the position of the node and a pointer to a BaseGameEntity... useful
//  if you want your nodes to represent health packs, gold mines and the like
//-----------------------------------------------------------------------------

//public class NavGraphNode<extra_info> : GraphNode where extra_info : class
public class NavGraphNode : GraphNode
{

    //the node's position
    protected Vector3 m_vPosition;

    //often you will require a navgraph node to contain additional information.
    //For example a node might represent a pickup such as armor in which
    //case m_ExtraInfo could be an enumerated value denoting the pickup type,
    //thereby enabling a search algorithm to search a graph for specific items.
    //Going one step further, m_ExtraInfo could be a pointer to the instance of
    //the item type the node is twinned with. This would allow a search algorithm
    //to test the status of the pickup during the search. 
    protected object  m_ExtraInfo;
    //protected extra_info m_ExtraInfo;

    public NavGraphNode() { }

    public NavGraphNode(int idx, Vector3 pos) : base(idx)
    {
        m_vPosition = pos;
        m_ExtraInfo = null;
    }

    public override string ToString()
    {
        return "" + base.ToString() + " " + m_vPosition;
    }



    public Vector3 Pos() { return m_vPosition; }
    public void SetPos(Vector3 NewPosition) { m_vPosition = NewPosition; }

    public extra_info ExtraInfo<extra_info>() {return (extra_info)m_ExtraInfo;}
    public void       SetExtraInfo(object info){m_ExtraInfo = info;}

    public override object Clone()
    {
        NavGraphNode node = this.MemberwiseClone() as NavGraphNode;

        return node;
    }

}