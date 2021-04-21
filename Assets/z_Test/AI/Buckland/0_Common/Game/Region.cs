using System;
using UnityEngine;
using UtilGS9;

namespace Buckland
{
    public enum region_modifier { halfsize, normal };
    public class Region
    {

        protected float m_dTop;
        protected float m_dLeft;
        protected float m_dRight;
        protected float m_dBottom;

        protected float m_dWidth;
        protected float m_dHeight;

        protected Vector3 m_vCenter;

        protected int m_iID;


        public Region()
        {
            m_dTop = 0;
            m_dBottom = 0;
            m_dLeft = 0;
            m_dRight = 0;
        }


        public Region(float left,
               float top,
               float right,
               float bottom,
               int id = -1)
        {
            m_dTop = top;
            m_dRight = right;
            m_dLeft = left;
            m_dBottom = bottom;
            m_iID = id;
            //calculate center of region
            m_vCenter = new Vector3((left + right) * 0.5f, 0, (top + bottom) * 0.5f);

            m_dWidth = Math.Abs(right - left);
            m_dHeight = Math.Abs(bottom - top);
        }

        //virtual ~Region() { }

        public virtual  void Render(bool ShowID = false)
        {
            //gdi->HollowBrush();
            //gdi->GreenPen();
            //gdi->Rect(m_dLeft, m_dTop, m_dRight, m_dBottom);

            DebugWide.DrawCube(m_vCenter, new Vector3(m_dWidth * 0.5f, 0, m_dHeight * 0.5f), Color.green);

          //if (ShowID)
          //{ 
          //      gdi->TextColor(Cgdi::green);
          //      gdi->TextAtPos(Center(), ttos(ID()));
          //}
        }

        //returns true if the given position lays inside the region. The
        //region modifier can be used to contract the region bounderies
        public bool Inside(Vector3 pos, region_modifier r = region_modifier.normal)
        {
          if (r == region_modifier.normal)
          {
            return ((pos.x > m_dLeft) && (pos.x<m_dRight) &&
                 (pos.y > m_dTop) && (pos.y<m_dBottom));
          }
          else
          {
            float marginX = m_dWidth * 0.25f;
            float marginY = m_dHeight * 0.25f;

            return ((pos.x > (m_dLeft+marginX)) && (pos.x<(m_dRight-marginX)) &&
                 (pos.y > (m_dTop+marginY)) && (pos.y<(m_dBottom-marginY)));
          }

        }

        //returns a vector representing a random location
        //within the region
        public Vector3 GetRandomPosition()
        {
            return new Vector3(Misc.RandFloat(m_dLeft, m_dRight), 0,
                       Misc.RandFloat(m_dTop, m_dBottom));
        }

        //-------------------------------
        public float Top() {return m_dTop;}
        public float Bottom() {return m_dBottom;}
        public float Left() {return m_dLeft;}
        public float Right() {return m_dRight;}
        public float Width() {return Math.Abs(m_dRight - m_dLeft);}
        public float Height() {return Math.Abs(m_dTop - m_dBottom);}
        public float Length() {return Math.Max(Width(), Height());}
        public float Breadth() {return Math.Min(Width(), Height());}

        public Vector3 Center() {return m_vCenter;}
        public int ID() {return m_iID;}

    }

}//end namespace

