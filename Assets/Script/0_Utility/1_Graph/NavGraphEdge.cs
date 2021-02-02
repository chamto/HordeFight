
//==========================================================

public class NavGraphEdge : GraphEdge
{
    //examples of typical flags
    public enum eFlag
    {
        normal = 0,
        swim = 1 << 0,
        crawl = 1 << 1,
        creep = 1 << 3,
        jump = 1 << 3,
        fly = 1 << 4,
        grapple = 1 << 5,
        goes_through_door = 1 << 6
    }


    protected int m_iFlags;

    //if this edge intersects with an object (such as a door or lift), then
    //this is that object's ID. 
    protected int m_iIDofIntersectingEntity;


    public NavGraphEdge(int from,
               int to,
               float cost,
               int flags = 0,
               int id = -1) : base(from, to, cost)

    {
        m_iFlags = flags;
        m_iIDofIntersectingEntity = id;
    }

    public NavGraphEdge()
    { }

    public void Init(int from,
               int to,
               float cost,
               int flags = 0,
               int id = -1) 

    {
        m_iFrom = from;
        m_iTo = to;
        m_dCost = cost;

        m_iFlags = flags;
        m_iIDofIntersectingEntity = id;
    }


    public int Flags() { return m_iFlags; }
    public void SetFlags(int flags) { m_iFlags = flags; }

    public int IDofIntersectingEntity() { return m_iIDofIntersectingEntity; }
    public void SetIDofIntersectingEntity(int id) { m_iIDofIntersectingEntity = id; }

    public override string ToString()
    {
        return base.ToString() + " " + " flags :" + ((eFlag)m_iFlags).ToString() + " ID:" + m_iIDofIntersectingEntity;
    }


}
