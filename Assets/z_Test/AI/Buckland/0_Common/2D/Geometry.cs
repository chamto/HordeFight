using UnityEngine;
using UtilGS9;
using System;

namespace Buckland
{
    public enum span_type { plane_backside, plane_front, on_plane };

    public class Geometry
    {
        //given a plane and a ray this function determins how far along the ray 
        //an interestion occurs. Returns negative if the ray is parallel
        static public float DistanceToRayPlaneIntersection(Vector3 RayOrigin,
                                                     Vector3 RayHeading,
                                                     Vector3 PlanePoint,  //any point on the plane
                                                     Vector3 PlaneNormal)
        {

            float d = Vector3.Dot(-PlaneNormal, PlanePoint);
            float numer = Vector3.Dot(PlaneNormal, RayOrigin) + d;
            float denom = Vector3.Dot(PlaneNormal, RayHeading);

            // normal is parallel to vector
            if ((denom < 0.000001f) && (denom > -0.000001f))
            {
                return (-1.0f);
            }

            return -(numer / denom);
        }

        static public span_type WhereIsPoint(Vector3 point,
                                      Vector3 PointOnPlane, //any point on the plane
                                      Vector3 PlaneNormal)
        {
            Vector3 dir = PointOnPlane - point;

            float d = Vector3.Dot(dir, PlaneNormal);

            if (d < -0.000001f)
            {
                return span_type.plane_front;
            }

            else if (d > 0.000001f)
            {
                return span_type.plane_backside;
            }

            return span_type.on_plane;
        }

        static public float DistToLineSegment(Vector3 A, Vector3 B, Vector3 P)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be A
            //float dotA = (P.x - A.x) * (B.x - A.x) + (P.y - A.y) * (B.y - A.y);
            Vector3 PA = P - A;
            Vector3 BA = B - A;
            Vector3 PB = P - B;
            float dotA = Vector3.Dot(PA, BA);

            if (dotA <= 0) return (A - P).magnitude;

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be B
            //float dotB = (P.x - B.x) * (A.x - B.x) + (P.y - B.y) * (A.y - B.y);
            float dotB = Vector3.Dot(PB, -BA);

            if (dotB <= 0) return (B - P).magnitude;

            //calculate the point along AB that is the closest to P
            Vector3 Point = A + ((BA) * dotA) / (dotA + dotB);

            //calculate the distance P-Point
            return (P - Point).magnitude;
        }

        static public float DistToLineSegmentSq(Vector3 A, Vector3 B, Vector3 P)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be A
            //float dotA = (P.x - A.x) * (B.x - A.x) + (P.y - A.y) * (B.y - A.y);
            Vector3 PA = P - A;
            Vector3 BA = B - A;
            Vector3 PB = P - B;
            float dotA = Vector3.Dot(PA, BA);

            if (dotA <= 0) return (A - P).sqrMagnitude;

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be B
            //float dotB = (P.x - B.x) * (A.x - B.x) + (P.y - B.y) * (A.y - B.y);
            float dotB = Vector3.Dot(PB, -BA);

            if (dotB <= 0) return (B - P).sqrMagnitude;

            //calculate the point along AB that is the closest to P
            Vector3 Point = A + ((BA) * dotA) / (dotA + dotB);

            //calculate the distance P-Point
            return (P - Point).sqrMagnitude;
        }

        //실험노트 2021-2-27 참고  
        //선분AB 와 CD 가 만나는지 검사 
        static public bool LineIntersection2D(Vector3 Ao, Vector3 Al,
                           Vector3 Bo, Vector3 Bl)
        {
            //수직내적을 풀어놓은 식 
            //float rTop = (Ao.z - Bo.z) * (Bl.x - Bo.x) - (Ao.x - Bo.x) * (Bl.z - Bo.z); 
            //float sTop = (Ao.z - Bo.z) * (Al.x - Ao.x) - (Ao.x - Bo.x) * (Al.z - Ao.z);
            //float Bot = (Al.x - Ao.x) * (Bl.z - Bo.z) - (Al.z - Ao.z) * (Bl.x - Bo.x);

            Vector3 Alo = Al - Ao;
            Vector3 Blo = Bl - Bo;
            Vector3 ABo = Ao - Bo;
            Vector3 BAo = Bo - Ao;

            //수직내적 , 행렬식값 , 부호가 있는 외적길이  
            //float rTop = VOp.PerpDot_XZ(Blo, ABo); //v, w => v.x*w.z - v.z*w.x 
            //float sTop = VOp.PerpDot_XZ(Alo, ABo);


            float rTop = VOp.PerpDot_XZ_RightHand(BAo , Blo); //v, w => v.x*w.z - v.z*w.x 
            float sTop = VOp.PerpDot_XZ_RightHand(BAo, Alo);
            float Bot = VOp.PerpDot_XZ_RightHand(Alo, Blo);


            if (Bot == 0)//parallel
            {
                return false;
            }

            //연립방정식 이용 
            //ref : https://bowbowbow.tistory.com/17  이 링크의 연립방정식을 외적으로 바꾸는 부분은 잘못된 것임. 
            //외적은 스칼라값이 아님. 외적이 아닌 행렬식값 또는 수직내적 으로 바꿔야 한다 
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

        static public bool LineIntersection2D(Vector3 Ao, Vector3 Al,
                               Vector3 Bo, Vector3 Bl,
                               out float dist, out Vector3 point)
        {
            dist = 0;
            point = ConstV.v3_zero;

            float rTop = (Ao.z - Bo.z) * (Bl.x - Bo.x) - (Ao.x - Bo.x) * (Bl.z - Bo.z);
            float rBot = (Al.x - Ao.x) * (Bl.z - Bo.z) - (Al.z - Ao.z) * (Bl.x - Bo.x);

            float sTop = (Ao.z - Bo.z) * (Al.x - Ao.x) - (Ao.x - Bo.x) * (Al.z - Ao.z);
            float sBot = (Al.x - Ao.x) * (Bl.z - Bo.z) - (Al.z - Ao.z) * (Bl.x - Bo.x);

            if ((rBot == 0) || (sBot == 0))
            {
                //lines are parallel
                return false;
            }

            float r = rTop / rBot;
            float s = sTop / sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                dist = (Ao - Al).magnitude * r;

                point = Ao + r * (Al - Ao);

                return true;
            }

            else
            {
                //dist = 0;

                return false;
            }
        }

        static public bool LineSegmentCircleIntersection(Vector3 A, Vector3 B, Vector3 P,
                                            float radius)
        {
            //first determine the distance from the center of the circle to
            //the line segment (working in distance squared space)
            float DistToLineSq = DistToLineSegmentSq(A, B, P);

            if (DistToLineSq < radius * radius)
            {
                return true;
            }

            else
            {
                return false;
            }

        }

        static public bool GetLineSegmentCircleClosestIntersectionPoint(Vector3 A,
                                                         Vector3 B,
                                                         Vector3 pos,
                                                         float radius,
                                                         ref Vector3 IntersectionPoint)
        {
            //IntersectionPoint = ConstV.v3_zero;
            Vector3 toBNorm = VOp.Normalize(B - A);

            //move the circle into the local space defined by the vector B-A with origin
            //at A
            Vector3 perp = Vector3.Cross(toBNorm, ConstV.v3_up);
            Vector3 LocalPos = Misc.PointToLocalSpace(pos, toBNorm, perp, A);

            bool ipFound = false;

            //if the local position + the radius is negative then the circle lays behind
            //point A so there is no intersection possible. If the local x pos minus the 
            //radius is greater than length A-B then the circle cannot intersect the 
            //line segment
            if ((LocalPos.x + radius >= 0) &&
               ((LocalPos.x - radius) * (LocalPos.x - radius) <= (B - A).sqrMagnitude))
            {
                //if the distance from the x axis to the object's position is less
                //than its radius then there is a potential intersection.
                if (Mathf.Abs(LocalPos.y) < radius)
                {
                    //now to do a line/circle intersection test. The center of the 
                    //circle is represented by A, B. The intersection points are 
                    //given by the formulae x = A +/-sqrt(r^2-B^2), y=0. We only 
                    //need to look at the smallest positive value of x.
                    float a = LocalPos.x;
                    float b = LocalPos.y;

                    float ip = a - (float)Math.Sqrt(radius * radius - b * b);

                    if (ip <= 0)
                    {
                        ip = a + (float)Math.Sqrt(radius * radius - b * b);
                    }

                    ipFound = true;

                    IntersectionPoint = A + toBNorm * ip;
                }
            }

            return ipFound;
        }


        //------------------------------------------------------------------------
        //  Given a point P and a circle of radius R centered at C this function
        //  determines the two points on the circle that intersect with the 
        //  tangents from P to the circle. Returns false if P is within the circle.
        //
        //  thanks to Dave Eberly for this one.
        //------------------------------------------------------------------------
        //ref : https://en.wikipedia.org/wiki/Tangent_lines_to_circles
        //ref : https://j1w2k3.tistory.com/860
        //원의 방정식과 접점의 방정식을 이용하여 식을 세운다 
        //20210422 - 실험노트 참고 
        static public bool GetTangentPointsXZ(Vector3 C, float R, Vector3 P, out Vector3 T1, out Vector3 T2)
        {
            T1 = T2 = Vector3.zero;

            Vector3 PmC = P - C;
            float SqrLen = PmC.sqrMagnitude;
            float RSqr = R * R;
            if (SqrLen <= RSqr)
            {
                // P is inside or on the circle
                return false;
            }

            float InvSqrLen = 1 / SqrLen;
            float Root = (float)Math.Sqrt(Math.Abs(SqrLen - RSqr));

            T1.x = C.x + R * (R * PmC.x - PmC.z * Root) * InvSqrLen;
            T1.z = C.z + R * (R * PmC.z + PmC.x * Root) * InvSqrLen;
            T2.x = C.x + R * (R * PmC.x + PmC.z * Root) * InvSqrLen;
            T2.z = C.z + R * (R * PmC.z - PmC.x * Root) * InvSqrLen;

            return true;
        }


    }//end class

}//end namespace

