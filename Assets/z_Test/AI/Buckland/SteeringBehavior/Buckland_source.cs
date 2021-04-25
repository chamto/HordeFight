using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehavior
{
    public class Const
    {
        //--------------------------- Constants ----------------------------------
        public const float Pi = 3.14159f;
        public const float TwoPi = Pi * 2f;
        public const float HalfPi = Pi / 2f;
        public const float QuarterPi = Pi / 4f;
        //------------------------------------------------------------------------
    }

    public class Util
    {

        public static T MaxOf<T>(T a, T b) where T : IComparable
        {
            if (a.CompareTo(b) > 0) return a;
            return b;
        }

        public static T MinOf<T>(T a, T b) where T : IComparable
        {
            if (a.CompareTo(b) > 0) return b;
            return a;
        }

        //public static Vector2 Perp(Vector2 v2)
        //{
        //    return new Vector2(-v2.y, v2.x);
        //}

        //treats a window as a toroid
        public static Vector2 WrapAround(Vector2 pos, int MaxX, int MaxY)
        {
            if (pos.x > MaxX) { pos.x = 0.0f; }

            if (pos.x < 0) { pos.x = (float)MaxX; }

            if (pos.y < 0) { pos.y = (float)MaxY; }

            if (pos.y > MaxY) { pos.y = 0.0f; }

            return pos;
        }

        //----------------------------- Truncate ---------------------------------
        //
        //  truncates a vector so that its length does not exceed max
        //------------------------------------------------------------------------
        public static Vector2 Truncate(Vector2 v2, float max)
        {
            if (v2.sqrMagnitude > max * max)
            {
                v2.Normalize();

                return v2 *= max;
            }

            return v2;
        }

        //--------------------------- Reflect ------------------------------------
        //
        //  given a normalized vector this method reflects the vector it
        //  is operating upon. (like the path of a ball bouncing off a wall)
        //------------------------------------------------------------------------
        public static Vector2 Reflect(Vector2 v2, Vector2 norm)
        {
            return v2 + 2f * Vector2.Dot(v2, norm) * (-norm);
        }

    
        //----------------------------- TwoCirclesOverlapped ---------------------
        //
        //  Returns true if the two circles overlap
        //------------------------------------------------------------------------
        public static bool TwoCirclesOverlapped(Vector2 c1, float r1,
                                  Vector2 c2, float r2)
        {
            float DistBetweenCenters = (float)Math.Sqrt((c1.x - c2.x) * (c1.x - c2.x) +
                                              (c1.y - c2.y) * (c1.y - c2.y));

            if ((DistBetweenCenters < (r1 + r2)) || (DistBetweenCenters < Math.Abs(r1 - r2)))
            {
                return true;
            }

            return false;
        }


        //------------------------ Sign ------------------------------------------
        //
        //  returns positive if v2 is clockwise of this vector,
        //  minus if anticlockwise (Y axis pointing down, X axis to right)
        //------------------------------------------------------------------------
        public const int clockwise = 1;
        public const int anticlockwise = -1;
        public static int Sign(Vector2 a, Vector2 b)
        {
            if (a.y * b.x > a.x * b.y)
            {
                return anticlockwise;
            }
            else
            {
                return clockwise;
            }
        }



        //------------------------- Overlapped -----------------------------------
        //
        //  tests to see if an entity is overlapping any of a number of entities
        //  stored in a std container
        //------------------------------------------------------------------------
        public static bool Overlapped<T, conT>(T ob, List<conT> conOb, float MinDistBetweenObstacles = 40.0f)
            where T : BaseGameEntity
            where conT : BaseGameEntity
        {

            foreach (BaseGameEntity it in conOb)
            {
                if (TwoCirclesOverlapped(ob.Pos(),
                                     ob.BRadius() + MinDistBetweenObstacles,
                                     it.Pos(),
                                     it.BRadius()))
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------- TagNeighbors ----------------------------------
        //
        //  tags any entities contained in a std container that are within the
        //  radius of the single entity parameter
        //------------------------------------------------------------------------

        public static void TagNeighbors<T, conT>(T entity, List<conT> ContainerOfEntities, float radius)
            where T : BaseGameEntity
            where conT : BaseGameEntity
        {
            //iterate through all entities checking for range
            conT curEntity;
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //first clear any current tag
                curEntity.UnTag();

                Vector2 to = curEntity.Pos() - entity.Pos();

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + curEntity.BRadius();

                //if entity within range, tag for further consideration. (working in
                //distance-squared space to avoid sqrts)
                if ((curEntity != entity) && (to.sqrMagnitude < range * range))
                {
                    curEntity.Tag();
                }

            }//next entity
        }


        //------------------- EnforceNonPenetrationConstraint ---------------------
        //
        //  Given a pointer to an entity and a std container of pointers to nearby
        //  entities, this function checks to see if there is an overlap between
        //  entities. If there is, then the entities are moved away from each
        //  other
        //------------------------------------------------------------------------

        public static void EnforceNonPenetrationConstraint<T>(T entity, List<T> ContainerOfEntities) where T : BaseGameEntity
        {
            T curEntity;
            //iterate through all entities checking for any overlap of bounding radii
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //make sure we don't check against the individual
                if (curEntity == entity) continue;

                //calculate the distance between the positions of the entities
                Vector2 ToEntity = entity.Pos() - (curEntity).Pos();

                float DistFromEachOther = ToEntity.magnitude;

                //if this distance is smaller than the sum of their radii then this
                //entity must be moved away in the direction parallel to the
                //ToEntity vector   
                float AmountOfOverLap = (curEntity).BRadius() + entity.BRadius() -
                                         DistFromEachOther;

                if (AmountOfOverLap >= 0)
                {
                    //move the entity a distance away equivalent to the amount of overlap.
                    entity.SetPos(entity.Pos() + (ToEntity / DistFromEachOther) *
                                   AmountOfOverLap);
                }
            }//next entity
        }

        public static float Vec2DDistanceSq(Vector2 v1, Vector2 v2)
        {
            float ySeparation = v2.y - v1.y;
            float xSeparation = v2.x - v1.x;

            return ySeparation * ySeparation + xSeparation * xSeparation;
        }

        //==================================================
        //                    geometry
        //==================================================

        //given a plane and a ray this function determins how far along the ray 
        //an interestion occurs. Returns negative if the ray is parallel
        public static float DistanceToRayPlaneIntersection(Vector2 RayOrigin,
                                                     Vector2 RayHeading,
                                                     Vector2 PlanePoint,  //any point on the plane
                                                     Vector2 PlaneNormal)
        {

            float d = -Vector2.Dot(PlaneNormal, PlanePoint);
            float numer = Vector2.Dot(PlaneNormal, RayOrigin) + d;
            float denom = Vector2.Dot(PlaneNormal, RayHeading);

            // normal is parallel to vector
            if ((denom < 0.000001f) && (denom > -0.000001f))
            {
                return (-1.0f);
            }

            return -(numer / denom);
        }


        public enum span_type { plane_backside, plane_front, on_plane };
        public static span_type WhereIsPoint(Vector2 point,
                                      Vector2 PointOnPlane, //any point on the plane
                                      Vector2 PlaneNormal)
        {
            Vector2 dir = PointOnPlane - point;

            double d = Vector2.Dot(dir, PlaneNormal);

            if (d < -0.000001)
            {
                return span_type.plane_front;
            }

            else if (d > 0.000001)
            {
                return span_type.plane_backside;
            }

            return span_type.on_plane;
        }

        //--------------------LineIntersection2D-------------------------
        //
        //  Given 2 lines in 2D space AB, CD this returns true if an 
        //  intersection occurs.
        //
        //----------------------------------------------------------------- 

        public static bool LineIntersection2D(Vector2 A,
                                       Vector2 B,
                                       Vector2 C,
                                       Vector2 D)
        {
            float rTop = (A.y - C.y) * (D.x - C.x) - (A.x - C.x) * (D.y - C.y);
            float sTop = (A.y - C.y) * (B.x - A.x) - (A.x - C.x) * (B.y - A.y);

            float Bot = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);

            if (Bot == 0)//parallel
            {
                return false;
            }

            float invBot = 1.0f / Bot;
            float r = rTop * invBot;
            float s = sTop * invBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                //lines intersect
                return true;
            }

            //lines do not intersect
            return false;
        }

        //-------------------- LineIntersection2D-------------------------
        //
        //  Given 2 lines in 2D space AB, CD this returns true if an 
        //  intersection occurs and sets dist to the distance the intersection
        //  occurs along AB. Also sets the 2d vector point to the point of
        //  intersection
        //----------------------------------------------------------------- 
        public static bool LineIntersection2D(Vector2 A,
                                       Vector2 B,
                                       Vector2 C,
                                       Vector2 D,
                                       float dist,
                                       Vector2 point)
        {

            float rTop = (A.y - C.y) * (D.x - C.x) - (A.x - C.x) * (D.y - C.y);
            float rBot = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);

            float sTop = (A.y - C.y) * (B.x - A.x) - (A.x - C.x) * (B.y - A.y);
            float sBot = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);

            if ((rBot == 0) || (sBot == 0))
            {
                //lines are parallel
                return false;
            }

            float r = rTop / rBot;
            float s = sTop / sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                dist = (A - B).magnitude * r;

                point = A + r * (B - A);

                return true;
            }

            else
            {
                dist = 0;

                return false;
            }
        }

    }


    public class Obstacle : BaseGameEntity
    {

        public Obstacle(float x,
               float y,
               float r) : base(0, new Vector2(x, y), r)
        { }

        public Obstacle(Vector2 pos, float radius) : base(0, pos, radius)
        { }


        //this is defined as a pure virtual function in BasegameEntity so
        //it must be implemented
        public void Update(float time_elapsed) { }

        public void Render()
        {
            DebugWide.DrawCircle(Pos(), BRadius(), Color.black);
        }


    }

    public class Wall2D
    {

        Vector2 m_vA, m_vB, m_vN;

        void CalculateNormal()
        {
            Vector2 temp = (m_vB - m_vA).normalized;

            m_vN.x = -temp.y;
            m_vN.y = temp.x;
        }


        public Wall2D() { }

        public Wall2D(Vector2 A, Vector2 B)
        {
            m_vA = A;
            m_vB = B;
            CalculateNormal();
        }

        public Wall2D(Vector2 A, Vector2 B, Vector2 N)
        {
            m_vA = A;
            m_vB = B;
            m_vN = N;
        }


        public virtual void Render(bool RenderNormals = false)
        {
            //gdi->Line(m_vA, m_vB);
            DebugWide.DrawLine(m_vA, m_vB, Color.white);

            ////render the normals if rqd
            if (RenderNormals)
            {
                int MidX = (int)((m_vA.x + m_vB.x) / 2);
                int MidY = (int)((m_vA.y + m_vB.y) / 2);
                DebugWide.DrawLine(new Vector2(MidX, MidY), new Vector2((int)(MidX + (m_vN.x * 5)), (int)(MidY + (m_vN.y * 5))), Color.black);
                //gdi->Line(MidX, MidY, (int)(MidX+(m_vN.x* 5)), (int) (MidY+(m_vN.y* 5)));
            }
        }

        public Vector2 From() { return m_vA; }
        public void SetFrom(Vector2 v) { m_vA = v; CalculateNormal(); }

        public Vector2 To() { return m_vB; }
        public void SetTo(Vector2 v) { m_vB = v; CalculateNormal(); }

        public Vector2 Normal() { return m_vN; }
        public void SetNormal(Vector2 n) { m_vN = n; }

        public Vector2 Center() { return (m_vA + m_vB) / 2.0f; }

    }
}

