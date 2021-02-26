using UnityEngine;
using UtilGS9;


namespace Raven
{
    public class Geometry
    {
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

        //ref : https://bowbowbow.tistory.com/17
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

            //수직내적 , 행렬식값 , 부호가 있는 외적길이  
            float rTop = VOp.PerpDot_XZ(Blo, ABo); //v, w => v.x*w.z - v.z*w.x 
            float sTop = VOp.PerpDot_XZ(Alo, ABo);
            float Bot = VOp.PerpDot_XZ(Blo, Alo);
            //float Bot = VOp.PerpDot_XZ(Alo, Blo);


            if (Bot == 0)//parallel
            {
                return false;
            }

            //무게중심좌표 구하는 공식과 관련있는 듯  
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
    }



}//end namespace

