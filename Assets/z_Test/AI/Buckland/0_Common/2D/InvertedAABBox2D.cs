using UnityEngine;

namespace Buckland
{
    public struct InvertedAABBox2D
    {

        public Vector3 m_vTopLeft;
        public Vector3 m_vBottomRight;
        public Vector3 m_vCenter;


        public InvertedAABBox2D(Vector3 tl, Vector3 br)
        {
            m_vTopLeft = tl;
            m_vBottomRight = br;
            m_vCenter = (tl + br) / 2.0f;
        }

        //returns true if the bbox described by other intersects with this one
        public bool isOverlappedWith(InvertedAABBox2D other)
        {
            return !((other.Top() > this.Bottom()) ||
                   (other.Bottom() < this.Top()) ||
                 (other.Left() > this.Right()) ||
                (other.Right() < this.Left()));
        }


        public Vector2 TopLeft() { return m_vTopLeft; }
        public Vector2 BottomRight() { return m_vBottomRight; }

        public float Top() { return m_vTopLeft.z; }
        public float Left() { return m_vTopLeft.x; }
        public float Bottom() { return m_vBottomRight.z; }
        public float Right() { return m_vBottomRight.x; }
        public Vector2 Center() { return m_vCenter; }

        public void Render(bool RenderCenter = false)
        {
            float size_x = Mathf.Abs(m_vTopLeft.x - m_vBottomRight.x);
            float size_z = Mathf.Abs(m_vTopLeft.z - m_vBottomRight.z);
            DebugWide.DrawCube(m_vCenter, new Vector3(size_x, 0, size_z), Color.white);

            if (RenderCenter)
            {
                DebugWide.DrawCircle(m_vCenter, 5, Color.white);
            }
        }

    }
}

