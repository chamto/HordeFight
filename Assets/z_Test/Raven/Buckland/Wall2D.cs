using UnityEngine;
using UtilGS9;

//*
namespace Raven
{
    public class Wall2D
    {

        protected Vector3 m_vA,
                        m_vB,
                        m_vN;


        protected void CalculateNormal()
        {
            Vector3 temp = VOp.Normalize(m_vB - m_vA);

            //m_vN.x = -temp.y;
            //m_vN.y = temp.x;
            m_vN = Vector3.Cross(temp, ConstV.v3_up);
        }


        public Wall2D() { }

        public Wall2D(Vector3 A, Vector3 B)
        {
            m_vA = A;
            m_vB = B;
            CalculateNormal();
        }

        public Wall2D(Vector3 A, Vector3 B, Vector3 N)
        {
            m_vA = A;
            m_vB = B;
            m_vN = N;
        }

        //Wall2D(std::ifstream& in) { Read(in); }

        public virtual void Render(bool RenderNormals = false)
        {
            DebugWide.DrawLine(m_vA, m_vB, Color.white);

            //render the normals if rqd
            if (RenderNormals)
            {
                Vector3 mid = (m_vA + m_vB) / 2f; //벡터중간점 구하는 특수공식

                DebugWide.DrawLine(mid, mid + m_vN * 5f, Color.white);

            }
        }

        public Vector3 From() { return m_vA; }
        public void SetFrom(Vector3 v) { m_vA = v; CalculateNormal(); }

        public Vector3 To() { return m_vB; }
        public void SetTo(Vector3 v) { m_vB = v; CalculateNormal(); }

        public Vector3 Normal() { return m_vN; }
        public void SetNormal(Vector3 n) { m_vN = n; }

        public Vector3 Center() { return (m_vA + m_vB) / 2.0f; }



        public void Read(string line)
        {
            float x = 0f, y = 0f, z = 0f;

            string[] sp = line.Split(' ');
            int idx = 0;

            x = float.Parse(sp[idx++]);
            z = float.Parse(sp[idx++]);
            SetFrom(new Vector3(x, y, z));

            x = float.Parse(sp[idx++]);
            z = float.Parse(sp[idx++]);
            SetTo(new Vector3(x, y, z));

            x = float.Parse(sp[idx++]);
            z = float.Parse(sp[idx++]);
            SetNormal(new Vector3(x, y, z));
        }

    }


}//end namespace
//*/
