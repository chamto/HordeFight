using System.Collections.Generic;
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

        public Wall2D(string line)
        {
            Read(line); 
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

            string[] sp = HandyString.SplitBlank(line);

            x = float.Parse(sp[0]);
            z = float.Parse(sp[1]);
            SetFrom(new Vector3(x, y, z));

            x = float.Parse(sp[2]);
            z = float.Parse(sp[3]);
            SetTo(new Vector3(x, y, z));

            x = float.Parse(sp[4]);
            z = float.Parse(sp[5]);
            SetNormal(new Vector3(x, y, z));
        }

        //==================================================

        
        //----------------------- doWallsObstructLineSegment --------------------------
        //
        //  given a line segment defined by the points from and to, iterate through all
        //  the map objects and walls and test for any intersection. This method
        //  returns true if an intersection occurs.
        //-----------------------------------------------------------------------------
        static public bool doWallsObstructLineSegment(Vector3 from, Vector3 to, List<Wall2D> walls)
        {
            LineSegment3 AB = new LineSegment3(from, to);

            //test against the walls
            int count = 0;
            foreach (Wall2D curWall in walls)
            {
                //DebugWide.LogGreen(AB + "  _" + curWall.From() + "  " + curWall.To());
                //do a line segment intersection test
                //if (LineSegment3.Intersection(AB, new LineSegment3(curWall.From(), curWall.To())) ) //선분이 점인 경우 계산에 문제가 발생 
                if(Geometry.LineIntersection2D(from,to, curWall.From(), curWall.To()))
                {
                    //DebugWide.LogGreen(count + "  __ " + AB + "  _" + curWall.From() + "  " + curWall.To());
                    return true;
                }
                count++;
          }
                                                                                   
          return false;
        }


        //----------------------- doWallsObstructCylinderSides -------------------------
        //
        //  similar to above except this version checks to see if the sides described
        //  by the cylinder of length |AB| with the given radius intersect any walls.
        //  (this enables the trace to take into account any the bounding radii of
        //  entity objects)
        //-----------------------------------------------------------------------------
        static public bool doWallsObstructCylinderSides(Vector3 A, Vector3 B,
                                                 float BoundingRadius,
                                                 List<Wall2D> walls)
        {
            //the line segments that make up the sides of the cylinder must be created
            Vector3 toB = VOp.Normalize(B - A);

            //A1B1 will be one side of the cylinder, A2B2 the other.
            Vector3 A1, B1, A2, B2;

            Vector3 radialEdge = Vector3.Cross(toB, ConstV.v3_up) * BoundingRadius;

            //create the two sides of the cylinder
            A1 = A + radialEdge;
            B1 = B + radialEdge;

            A2 = A - radialEdge;
            B2 = B - radialEdge;

            //now test against them
            if (!doWallsObstructLineSegment(A1, B1, walls))
            {
                return doWallsObstructLineSegment(A2, B2, walls);
            }

            return true;
        }

        //------------------ FindClosestPointOfIntersectionWithWalls ------------------
        //
        //  tests a line segment against the container of walls  to calculate
        //  the closest intersection point, which is stored in the reference 'ip'. The
        //  distance to the point is assigned to the reference 'distance'
        //
        //  returns false if no intersection point found
        //-----------------------------------------------------------------------------
        static public bool FindClosestPointOfIntersectionWithWalls(Vector3 A, Vector3 B,
                                                            out float          distance,
                                                            out Vector3       ip,
                                                            List<Wall2D> walls)
        {
            distance = float.MaxValue;
            ip = ConstV.v3_zero;

            LineSegment3 AB = new LineSegment3(A, B);
            foreach (Wall2D curWall in walls)
            {
                float dist = 0.0f;
                Vector3 point;


                //if (LineSegment3.Intersection(AB, new LineSegment3(curWall.From(), curWall.To()), out dist, out point))
                if (Geometry.LineIntersection2D(A, B, curWall.From(), curWall.To(), out dist, out point))
                {
                    if (dist < distance)
                    {
                        distance = dist;
                        ip = point;
                    }
                }
            }

            if (distance < float.MaxValue) return true;

            return false;
        }

        //------------------------ doWallsIntersectCircle -----------------------------
        //
        //  returns true if any walls intersect the circle of radius at point p
        //-----------------------------------------------------------------------------
        static public bool doWallsIntersectCircle(List<Wall2D> walls, Vector3 p, float r)
        {
            Vector3 intr;
            //test against the walls
            foreach(Wall2D curWall in walls)
            {

                //do a line segment intersection test
                //if (Geo.IntersectLineSegment(p, r, new LineSegment3(curWall.From(), curWall.To()), out intr))
                if(Geometry.LineSegmentCircleIntersection(curWall.From(), curWall.To(),p,r))

                {
                    return true;
                }
            }

            return false;
        }

    }


}//end namespace
//*/
