using UnityEngine;


namespace Raven
{
    //======================================================

    public class TriggerRegion
    {
        //returns true if an entity of the given size and position is intersecting
        //the trigger region.
        public virtual bool isTouching(Vector3 EntityPos, float EntityRadius) { return false; }
    }

    //--------------------------- TriggerRegion_Circle ----------------------------
    //
    //  class to define a circular region of influence
    //-----------------------------------------------------------------------------
    public class TriggerRegion_Circle : TriggerRegion
    {

        //the center of the region
        Vector3 m_vPos;

        //the radius of the region
        float m_dRadius;


        public TriggerRegion_Circle(Vector3 pos, float radius)
        {
            m_dRadius = radius;
            m_vPos = pos;
        }

        public override bool isTouching(Vector3 pos, float EntityRadius)
        {
            return (m_vPos - pos).sqrMagnitude < (EntityRadius + m_dRadius) * (EntityRadius + m_dRadius);
        }
    }


    //--------------------------- TriggerRegion_Rectangle -------------------------
    //
    //  class to define a circular region of influence
    //-----------------------------------------------------------------------------
    public class TriggerRegion_Rectangle : TriggerRegion
    {

        InvertedAABBox2D m_pTrigger;


        public TriggerRegion_Rectangle(Vector3 TopLeft, Vector3 BottomRight)
        {
            m_pTrigger = new InvertedAABBox2D(TopLeft, BottomRight);
        }


        //there's no need to do an accurate (and expensive) circle v
        //rectangle intersection test. Instead we'll just test the bounding box of
        //the given circle with the rectangle.
        public override bool isTouching(Vector3 pos, float EntityRadius)
        {
            InvertedAABBox2D Box = new InvertedAABBox2D(new Vector3(pos.x - EntityRadius, pos.z - EntityRadius),
                                 new Vector3(pos.x + EntityRadius, pos.z + EntityRadius));

            return Box.isOverlappedWith(m_pTrigger);
        }
    }
}

