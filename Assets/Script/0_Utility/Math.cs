using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;


namespace UtilGS9
{

    /// <summary>
    /// 직선 , 매개변수가 곱해진 방향값(direction) 을 사용한다
    /// </summary>
    public struct Line3
    {
        public Vector3 direction;
        public Vector3 origin;

        public Line3(Vector3 pa_dir , Vector3 pa_ori )
        {
            direction = pa_dir;
            origin = pa_ori;
        }

        //두 직선의 교점을 구한다. 두 직선이 평행한 경우, 선과 선사이의 최소거리를 반환한다
        static public void ClosestPoints(out Vector3 point0, out Vector3 point1,
                                  Line3 line0,
                                  Line3 line1)
        {
            // compute intermediate parameters
            Vector3 w0 = line0.origin - line1.origin;

            float a = Vector3.Dot(line0.direction, line0.direction);
            float b = Vector3.Dot(line0.direction, line1.direction);
            float c = Vector3.Dot(line1.direction, line1.direction);
            float d = Vector3.Dot(line0.direction, w0);
            float e = Vector3.Dot(line1.direction, w0);

            float denom = a * c - b * b;


            //if (ML.Util.IsZero(denom))
            if (Math.Abs(denom) < float.Epsilon)
            {
                point0 = line0.origin;
                point1 = line1.origin + (e / c) * line1.direction;
            }
            else
            {
                point0 = line0.origin + ((b * e - c * d) / denom) * line0.direction;
                point1 = line1.origin + ((a * e - b * d) / denom) * line1.direction;
            }

        }

    }

    public struct Ray3
    {
        public Vector3 direction;
        public Vector3 origin;

        public Ray3(Vector3 pa_dir, Vector3 pa_ori)
        {
            direction = pa_dir;
            origin = pa_ori;
        }

        //두 반직선의 교점을 구한다. 두 반직선이 평행한 경우, 선과 선사이의 최소거리를 반환한다
        static public void ClosestPoints(out Vector3 point0, out Vector3 point1,
                                  Ray3 ray0, Ray3 ray1)
        {
            // compute intermediate parameters
            Vector3 w0 = ray0.origin - ray1.origin;

            float a = Vector3.Dot(ray0.direction, ray0.direction);
            float b = Vector3.Dot(ray0.direction, ray1.direction);
            float c = Vector3.Dot(ray1.direction, ray1.direction);
            float d = Vector3.Dot(ray0.direction, w0);
            float e = Vector3.Dot(ray1.direction, w0);

            float denom = a * c - b * b;

            // parameters to compute s_c, t_c
            float s_c, t_c;
            float sn, sd, tn, td;

            //if (ML.Util.IsZero(denom))
            if (Math.Abs(denom) < float.Epsilon)
            {
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // start by clamping s_c
                sd = td = denom;
                sn = b * e - c * d;
                tn = a * e - b * d;

                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }

            }

            // clamp t_c within [0,+inf]
            // clamp t_c to 0
            if (tn < 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if (-d < 0.0f)
                {
                    s_c = 0.0f;
                }
                else
                {
                    s_c = -d / a;
                }
            }
            else
            {
                t_c = tn / td;
                s_c = sn / sd;
            }

            // compute closest points
            point0 = ray0.origin + s_c * ray0.direction;
            point1 = ray1.origin + t_c * ray1.direction;

        }

    }

    /// <summary>
    /// 선분 , 매개변수가 곱해진 방향값(direction) 을 사용한다.
    /// </summary>
    public struct LineSegment3 
    {

        /// <summary>
        /// direction : 선분의 길이* 길이가 1인 방향
        /// origin : 선분의 시작점 
        /// last : 선분의 끝점
        /// </summary>
        /// 
        public Vector3 direction;
        public Vector3 origin;

        public LineSegment3(Vector3 in_origin, Vector3 in_last)
        {
            origin = in_origin;
            direction = in_last - in_origin;
        }

        public Vector3 last
        {
            get
            {
                return origin + direction;
            }

            set
            {
                direction = value - origin;
            }
        }
        public float last_x
        {
            set
            {
                direction.x = value - origin.x;
            }
        }
        public float last_y
        {
            set
            {
                direction.y = value - origin.y;
            }
        }
        public float last_z
        {
            set
            {
                direction.z = value - origin.z;
            }
        }

        public static LineSegment3 zero
        {
            get
            {
                LineSegment3 l = new LineSegment3();
                l.direction = Vector3.zero;
                l.origin = Vector3.zero;
                return l;
            }
        }

        public Vector3 GetEndpoint0() { return origin; }
        public Vector3 GetEndpoint1() { return origin + direction; }

        // ---------------------------------------------------------------------------
        // Returns the distance between two endpoints
        //-----------------------------------------------------------------------------
        public float Length()
        {
            return direction.magnitude;
        }



        // ---------------------------------------------------------------------------
        // Returns the squared distance between two endpoints
        //-----------------------------------------------------------------------------
        public float LengthSquared()
        {
            return direction.sqrMagnitude;
        }


        // ---------------------------------------------------------------------------
        // Returns the closest point on line segment to point
        //-----------------------------------------------------------------------------
        public Vector3 ClosestPoint(Vector3 point)
        {
            Vector3 w = point - origin;
            float proj = Vector3.Dot(w, direction);
            // endpoint 0 is closest point
            if (proj <= 0.0f)
                return origin;
            else
            {
                float vsq = Vector3.Dot(direction, direction);
                // endpoint 1 is closest point
                if (proj >= vsq)
                    return origin + direction;
                // else somewhere else in segment
                else
                    return origin + (proj / vsq) * direction;
            }
        }

        // ---------------------------------------------------------------------------
        // Returns the closest points between two line segments.
        //-----------------------------------------------------------------------------
        static public void ClosestPoints(out Vector3 point0, out Vector3 point1,
                           LineSegment3 segment0,
                           LineSegment3 segment1)
        {
            // compute intermediate parameters
            Vector3 w0 = segment0.origin - segment1.origin;

            float a = Vector3.Dot(segment0.direction, segment0.direction); //u . u
            float b = Vector3.Dot(segment0.direction, segment1.direction); //u . v
            float c = Vector3.Dot(segment1.direction, segment1.direction); //v . v
            float d = Vector3.Dot(segment0.direction, w0); //u . w0
            float e = Vector3.Dot(segment1.direction, w0); //v . w0

            float denom = a * c - b * b;
            // parameters to compute s_c, t_c
            float s_c, t_c;
            float sn, sd, tn, td; //  sn/sd , tn/td

            // if denom is zero, try finding closest point on segment1 to origin0
            //if (ML.Util.IsZero(denom))
            if(Math.Abs(denom) < float.Epsilon)
            {
                // clamp s_c to 0
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // clamp s_c within [0,1]
                sd = td = denom;
                sn = b * e - c * d;
                tn = a * e - b * d;

                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }
                // clamp s_c to 1
                else if (sn > sd)
                {
                    sn = sd;
                    tn = e + b;
                    td = c;
                }
            }

            // clamp t_c within [0,1]
            // clamp t_c to 0
            if (tn < 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if (-d < 0.0f)
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if (-d > a)
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = -d / a;
                }
            }
            // clamp t_c to 1
            else if (tn > td)
            {
                t_c = 1.0f;
                // clamp s_c to 0
                if ((-d + b) < 0.0f)
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ((-d + b) > a)
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = (-d + b) / a;
                }
            }
            else
            {
                t_c = tn / td;
                s_c = sn / sd;
            }

            // compute closest points
            point0 = segment0.origin + s_c * segment0.direction;
            point1 = segment1.origin + t_c * segment1.direction;

        }

        // ---------------------------------------------------------------------------
        // Returns the minimum distance squared between line segment and point
        // Returns 판별식 t_c = (w⋅v)/(v⋅v)   [0~1 사이의 값] 487p 참고
        //-----------------------------------------------------------------------------
        public float DistanceSquared(Vector3 point, out float t_c)
        {
            Vector3 w = point - origin;
            float proj = Vector3.Dot(w, direction);
            // endpoint 0 is closest point
            if (proj <= 0)
            {
                t_c = 0.0f;
                return Vector3.Dot(w, w);
            }
            else
            {
                float vsq = Vector3.Dot(direction, direction);
                // endpoint 1 is closest point
                if (proj >= vsq)
                {
                    t_c = 1.0f;
                    return Vector3.Dot(w, w) - 2.0f * proj + vsq;
                }
                // otherwise somewhere else in segment
                else
                {
                    t_c = proj / vsq;
                    //CDefine.DebugLog("w.Dot(w) : " + w.Dot(w) + " proj : " + proj + " t_c * proj : " + (t_c * proj) );
                    //return w.Dot(w) - t_c * proj;

                    //20140911 chamto - bug fix, ref : TestFuncMinDist Scene 
                    //실수값 빼기에서 해가 0에 근접할수록 오차값이 생겨 값이 이상해짐
                    //테스트를 통해 판별값 0.0001 보다 작은수면 0으로 처리함 , 판별값이 이보다 작아지면 여전히 오차가 발생했음
                    //디바이스에서 테스트시 결과가 다를수 있다.
                    vsq = Vector3.Dot(w, w) - t_c * proj;
                    if (0.0001f > vsq) return 0;
                    else return vsq;
                }
            }

        }

        //----------------------------------------------------------------------------
        // @ ::DistanceSquared()
        // ---------------------------------------------------------------------------
        // Returns the distance squared between two line segments.
        // Based on article and code by Dan Sunday at www.geometryalgorithms.com
        //-----------------------------------------------------------------------------
        static public float DistanceSquared(LineSegment3 segment0, LineSegment3 segment1, 
                 out float s_c, out float t_c )
        {
            // compute intermediate parameters
            Vector3 w0 = segment0.origin - segment1.origin;
            float a = Vector3.Dot(segment0.direction, segment0.direction); 
            float b = Vector3.Dot(segment0.direction, segment1.direction);
            float c = Vector3.Dot(segment1.direction, segment1.direction);
            float d = Vector3.Dot(segment0.direction, w0);
            float e = Vector3.Dot(segment1.direction, w0);

            float denom = a * c - b * b;
            // parameters to compute s_c, t_c
            float sn, sd, tn, td;
            //s_c = sn/sd , t_c = tn/td

            // if denom is zero, try finding closest point on segment1 to origin0
            //if ( ::IsZero(denom) )
            if (Math.Abs(denom) < float.Epsilon)
            {
                // clamp s_c to 0
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // clamp s_c within [0,1]
                sd = td = denom;
                sn = b* e - c* d;
                tn = a* e - b* d;
          
                // clamp s_c to 0
                if (sn< 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }
                // clamp s_c to 1
                else if (sn > sd)
                {
                    sn = sd;
                    tn = e + b;
                    td = c;
                }
            }

            // clamp t_c within [0,1]
            // clamp t_c to 0
            if (tn< 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if ( -d< 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( -d > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = -d/a;
                }
            }
            // clamp t_c to 1
            else if (tn > td)
            {
                t_c = 1.0f;
                // clamp s_c to 0
                if ((-d+b) < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ((-d+b) > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = (-d+b)/a;
                }
            }
            else
            {
                t_c = tn/td;
                s_c = sn/sd;
            }

            // compute difference vector and distance squared
            Vector3 wc = w0 + s_c * segment0.direction - t_c * segment1.direction;
            return Vector3.Dot(wc, wc);

        }   // End of ::DistanceSquared()


        public override string ToString()
        {
            return string.Format("origin: {0}, last: {1}, direction: {2}", new object[]
                                  {
                this.origin,
                this.last,
                this.direction
            });
        }


    }//End Class


    public struct Plane
    {
        public Vector3 _normal;
        public float _offset; //원점에서 그은 수선의 발이 평면과 수직인 거리 , 평면과 원점의 최소거리 


        //public Plane()
        //{
        //    _normal = new Vector3(0.0f, 1.0f, 0.0f);
        //    _offset = 0.0f;
        //}
        public Plane(float a, float b, float c, float d)
        {
            _normal = new Vector3(0.0f, 1.0f, 0.0f);
            _offset = 0.0f;

            Set(a, b, c, d);
        }
        public Plane(Vector3 p0, Vector3 p1, Vector3 p2 )
        {
            _normal = new Vector3(0.0f, 1.0f, 0.0f);
            _offset = 0.0f;

            Set(p0, p1, p2);
        }
    
        public Vector3 GetPos()
        {
            return _normal * -_offset;
        }

        public void Transform(ref Quaternion rotate, ref Vector3 translate )
        {
            // transform to get normal
            _normal = rotate * _normal;
            
            // transform to get offset
            Vector3 newTrans = rotate * translate;
            _offset = -1f * Vector3.Dot(newTrans , _normal ) + _offset;

        }

        // manipulators
        //평면의 법선백터 : n(a,b,c)
        //평면의 방정식 : ax + by + cz + d  = 0  , (d = -(ax0 + by0 + cz0))
        //내적으로 표현 : n.dot(x,y,z) + d , d = -n.dot(x0,y0,z0)
        public void Set(float a, float b, float c, float d)
        {
            // normalize for cheap distance checks
            float lensq = a * a + b * b + c * c; //내적표현으로 보면 평면의 법선백터 제곱길이가 됨 
                                                 // length of normal had better not be zero


            // recover gracefully
            //if ( ::IsZero(lensq))
            if (lensq < float.Epsilon)
            {
                _normal = Vector3.up;
                _offset = 0.0f;
            }
            else
            {
                float recip = 1f / (float)Math.Sqrt(lensq); //  [ 1f / sqrt(lensq) ] , 나눗셈 연산을 줄이기 위한 장치
                _normal.Set(a * recip, b * recip, c * recip); //정규화되지 않은 법선벡터를 길이로 나누어 정규화함
                _offset = d * recip; //원점에서 얼마나 떨어져있는지 나타내는 값을 계산 : 
                                     // d값 안의 법선벡터가 정규화 되어있지 않기에 recip를 곱해 정규화 해준다. d = 평면법선.dot(평면위의 임의점) 
            }
        }

        public void Set(Vector3 p0, Vector3 p1,Vector3 p2 )
        {
            // get plane vectors
            Vector3 u = p1 - p0;
            Vector3 v = p2 - p0;
            Vector3 w = Vector3.Cross(u,v);

            // normalize for cheap distance checks
            float lensq = w.x * w.x + w.y * w.y + w.z * w.z;

            // recover gracefully
            ///if ( ::IsZero(lensq))
            if (lensq < float.Epsilon)
            {
                _normal = Vector3.up;
                _offset = 0.0f;
            }
            else
            {
                //float recip = 1.0f / lensq; //이건 아닌것 같음 
                float recip = 1f / (float)Math.Sqrt(lensq);
                _normal.Set(w.x * recip, w.y * recip, w.z * recip);
                _offset = -1f * Vector3.Dot(_normal,p0);
            }
        }


        // closest point
        public Vector3 ClosestPoint(ref Vector3 point )
        {
            //point 위치에서 평면상 수직인 점으로 이동 
            return point - Test(point)*_normal; 
        }

        // distance
        public float Distance(Vector3 point)
        {
            return Math.Abs(Test(point));
        }

        // result of plane test
        public float Test(Vector3 point )
        {
            //법선벡터 방향으로 원점에서 point 까지의 거리 - 법선벡터 방향으로 원점에서 평면 까지의 거리 
            // = 법선벡터 방향으로 평면에서 point 까지의 거리 
            return Vector3.Dot(_normal, point) + _offset;
        }

        //ref : http://geomalgorithms.com/a05-_intersect-1.html
        //    Return: 0 = disjoint (no intersection)
        //            1 =  intersection in the unique point *I0
        //            2 = the  segment lies in the plane
        static public int Intersect(out Vector3 I, LineSegment3 S, Plane Pn)
        {
            I = S.origin;
            Vector3 u = S.direction;
            Vector3 w = S.origin - Pn.GetPos();

            float D = Vector3.Dot(Pn._normal, u);
            float N = -Vector3.Dot(Pn._normal, w);

            if (Mathf.Abs(D) < float.Epsilon)
            {   // segment is parallel to plane
                if (N == 0)                      // segment lies in plane
                    return 2;
                else
                    return 0;                    // no intersection
            }
            // they are not parallel
            // compute intersect param
            float sI = N / D;
            if (sI < 0 || sI > 1f)
                return 0;                        // no intersection

            I = S.origin + sI * u;               // compute segment intersect point
            return 1;
        }
                
        public void Draw(float length, Color cc)
        {
            Vector3 ori = GetPos();
            DebugWide.DrawCircle(ori, 0.05f, cc);
            DebugWide.DrawLine(ori, ori + _normal, cc);

            //float length = 5f;
            Vector3 leftUp, rightUp, leftDown, rightDown;
            Vector3 left = Vector3.Cross(Vector3.forward, _normal);
            Vector3 fwd = Vector3.Cross(left, _normal);
            fwd.Normalize(); left.Normalize();

            leftUp = fwd * length + left * length;
            rightUp = fwd * length + -left * length;
            leftDown = -fwd * length + left * length;
            rightDown = -fwd * length + -left * length;

            DebugWide.DrawLine(ori + leftUp, ori + rightUp, cc);
            DebugWide.DrawLine(ori + leftUp, ori + leftDown, cc);
            DebugWide.DrawLine(ori + leftDown, ori + rightDown, cc);
            DebugWide.DrawLine(ori + rightUp, ori + rightDown, cc);
        }

    }//end class


    //======================================================

    public struct Capsule
    {
        public LineSegment3 mSegment;
        public float mRadius;

        bool Intersect(ref Capsule other )
        {
            //캡슐과 선분 충돌검사로 변형해 검사한다 , 소스캡슐의 반지름에 대상캡슐의 반지름을 더해서 대상캡슐의 선분값으로 비교가능하게 만든다. 
            float radiusSum = mRadius + other.mRadius;

            // if colliding
            float s, t;
            float distancesq = LineSegment3.DistanceSquared(mSegment, other.mSegment, out s, out t); 

            return (distancesq <= radiusSum* radiusSum );
        }

        bool Intersect(ref LineSegment3 segment )
        {
            //선분과 선분의 최소거리는 서로의 직각인 선분이다 , 직각이 되는 선분의 길이를 캡슐의 반지름과 비교한다 
            // test distance between segment and segment vs. radius
            float s_c, t_c;
            return ( LineSegment3.DistanceSquared(mSegment, segment, out s_c, out t_c ) <= mRadius* mRadius );

        }

        //----------------------------------------------------------------------------
        // @ IvCapsule::Classify()
        // ---------------------------------------------------------------------------
        // Return signed distance between capsule and plane
        //-----------------------------------------------------------------------------
        float Classify(ref Plane plane )
        {
            float s0 = plane.Test(mSegment.GetEndpoint0());
            float s1 = plane.Test(mSegment.GetEndpoint1());

            //평면 위쪽에 점이 있으면 그거리는 양수 , 평면 아랫쪽에 점이 있으면 그거리는 음수
            //선분이 평면을 관통한다면 s0 * s1 값은 음수가 나와야 한다 
            // points on opposite sides or intersecting plane
            if (s0* s1 <= 0.0f )
                return 0.0f;

            //캡슐뚜껑이 닿은 경우 
            // intersect if either endpoint is within radius distance of plane
            if( Math.Abs(s0) <= mRadius || Math.Abs(s1) <= mRadius )
                return 0.0f;

            // return signed distance
            return (Math.Abs(s0) < Mathf.Abs(s1) ? s0 : s1 ); //!! 캡슐뚜껑의 반지름 길이를 더하지 않고 반환하고 있다 
        }
    }



    //======================================================

    public struct Triangle
    {

        //-------------------------------------------------------------------------------
        // @ ::IsPointInTriangle()
        //-------------------------------------------------------------------------------
        // Returns true if point on triangle plane lies inside triangle (3D version)
        // Assumes triangle is not degenerate
        //-------------------------------------------------------------------------------
        public bool IsPointInTriangle(Vector3 point, Vector3 P0, Vector3 P1, Vector3 P2)
        {
            Vector3 v0 = P1 - P0;
            Vector3 v1 = P2 - P1;
            Vector3 n = Vector3.Cross(v0, v1);

            //외적의 180도 기점으로 방향이 바뀌는 점을 이용 , 벡터v0이 나누는 공간에서 어느공간에 점이 있는지 테스트하게 된다  
            Vector3 wTest = Vector3.Cross(v0, (point - P0));
            if (Vector3.Dot(wTest, n) < 0.0f)
            {
                return false;
            }

            wTest = Vector3.Cross(v1, (point - P1));
            if (Vector3.Dot(wTest, n) < 0.0f)
            {
                return false;
            }

            Vector3 v2 = P0 - P2;
            wTest = Vector3.Cross(v2, (point - P2));
            if (Vector3.Dot(wTest, n) < 0.0f)
            {
                return false;
            }

            return true;
        }


        //-------------------------------------------------------------------------------
        // @ ::BarycentricCoordinates()
        //-------------------------------------------------------------------------------
        // Returns barycentric coordinates for point inside triangle (3D version)
        // Assumes triangle is not degenerate
        //
        // 무게중심좌표 : t(u,v) = (1 - u - v)P0 + uP1 + vP2
        //-------------------------------------------------------------------------------
        public void BarycentricCoordinates(ref float r, ref float s, ref float t,
                                     Vector3 point, Vector3 P0, Vector3 P1, Vector3 P2)
        {
            // get difference vectors
            Vector3 u = P1 - P0;
            Vector3 v = P2 - P0;
            Vector3 w = point - P0;

            // compute cross product to get area of parallelograms
            Vector3 a = Vector3.Cross(u, w);
            Vector3 b = Vector3.Cross(v, w);
            Vector3 c = Vector3.Cross(u, v);

            //외적의 값(평행사변형의 면적)을 이용하여 0~1 값의 비율로 만든다   
            // compute barycentric coordinates as ratios of areas
            float denom = 1.0f / c.magnitude;
            s = b.magnitude * denom; //v
            t = a.magnitude * denom; //u
            r = 1.0f - s - t; //w
        }


        //-------------------------------------------------------------------------------
        // @ ::TriangleIntersect()
        //-------------------------------------------------------------------------------
        // Returns true if ray intersects triangle
        //
        // < 매개변수방정식 = 무게중심좌표 >
        // o + t * d = (1-u-v) * p0 + u * p1 + v * p2
        // o - p0 = -td + u(p1-p0) + v(p2-p0)
        // < 행렬로 표현 >
        // o - p0 = [-d, (p1-p0), (p2-p0)] [t, u, v] 
        // s = o - p0 , e1 = (p1-p0), e2 = (p2-p0)
        // s = [-d, e1, e2] [t, u, v]
        // < 크래머의 법칙으로 연립방정식의 해 구하기 >
        // [t, u, v] = 1/adj[-d, e1, e2] [ adj[s, e1, e2], adj[-d, s, e2], adj[-d, e1, s] ]
        //  < 행렬식를 기하학적 표현으로 바꾸기 , 스칼라삼중곱 >
        // p = d x e2 , q = s x e1
        // a = adj[-d, e1, e2] = (e2 x -d) ⋅ e1 = (d x e2) ⋅ e1 = p ⋅ e1
        // a1 = adj[s, e1, e2] = (s x e1) ⋅ e2 = q ⋅ e2
        // a2 = adj[-d, s, e2] = (e2 x -d) ⋅ s = (d x e2) ⋅ s = p ⋅ s
        // a3 = adj[-d, e1, s] = -d ⋅ (e1 x s) = -d ⋅ -(s x e1) = d ⋅ (s x e1) = q ⋅ d
        // t = a1 / a = q ⋅ e2 / a
        // u = a2 / a = p ⋅ s / a
        // v = a3 / a = q ⋅ d / a 
        //
        //-------------------------------------------------------------------------------
        static public bool TriangleIntersect(out float t, Vector3 P0, Vector3 P1, Vector3 P2, Ray3 ray )
        {
            // test ray direction against triangle
            t = 0f;
            Vector3 e1 = P1 - P0;
            Vector3 e2 = P2 - P0;
            Vector3 p = Vector3.Cross(ray.direction, e2);
            float a = Vector3.Dot(e1, p);

            // if result zero, no intersection or infinite intersections
            // (ray parallel to triangle plane)
            //if ( ::IsZero(a) )
            if (Math.Abs(a) < float.Epsilon)
                return false;

            // compute denominator
            float f = 1.0f / a;

            // compute barycentric coordinates
            Vector3 s = ray.origin - P0;
            float u = f * Vector3.Dot(s, p);

            // ray falls outside triangle
            if (u< 0.0f || u> 1.0f) 
                return false;

            Vector3 q = Vector3.Cross(s, e1);
            float v = f * Vector3.Dot(ray.direction, q);

            // ray falls outside triangle
            if (v< 0.0f || u+v> 1.0f) 
                return false;

            // compute line parameter
            t = f * Vector3.Dot(e2, q);

            return (t >= 0.0f);
        }

        static public bool TriangleIntersect(out float t, Vector3 P0, Vector3 P1, Vector3 P2, LineSegment3 line)
        {
            t = 0f;
            Vector3 e1 = P1 - P0;
            Vector3 e2 = P2 - P0;
            Vector3 p = Vector3.Cross(line.direction, e2);
            float a = Vector3.Dot(e1, p);

            //삼각형과 선분의 무한교차 판별못함 
            // if result zero, no intersection or infinite intersections
            if (Math.Abs(a) < float.Epsilon)
            {
                //Vector3 oTo = line.origin - P0;
                //float b = Vector3.Dot(oTo, p);
                //if (Math.Abs(b) < float.Epsilon)
                //{   //무한교차 가능 상태 
                //    //DebugWide.LogBlue("infinite intersections");
                //    //return true;
                //}
                    
                return false;
            }
                

            // compute denominator
            float f = 1.0f / a;

            // compute barycentric coordinates
            Vector3 s = line.origin - P0;
            float u = f * Vector3.Dot(s, p);
            //DebugWide.LogBlue(f + "  " + u);
            // ray falls outside triangle
            if (u < 0.0f || u > 1.0f)
                return false;

            Vector3 q = Vector3.Cross(s, e1);
            float v = f * Vector3.Dot(line.direction, q);

            // ray falls outside triangle
            if (v < 0.0f || u + v > 1.0f)
                return false;

            // compute line parameter
            t = f * Vector3.Dot(e2, q);

            return (t >= 0.0f && t <= 1f);
        }

        //-------------------------------------------------------------------------------
        // @ ::TriangleClassify()
        //-------------------------------------------------------------------------------
        // Returns signed distance between plane and triangle
        //-------------------------------------------------------------------------------
        public float TriangleClassify( Vector3 P0, Vector3 P1,Vector3 P2, Plane plane )
        {
            float test0 = plane.Test(P0);
            float test1 = plane.Test(P1);

            // if two points lie on opposite sides of plane, intersect
            if (test0 * test1 < 0.0f)
                return 0.0f;

            float test2 = plane.Test(P2);

            // if two points lie on opposite sides of plane, intersect
            if (test0 * test2 < 0.0f)
                return 0.0f;
            if (test1 * test2 < 0.0f)
                return 0.0f;

            // no intersection, return signed distance
            if (test0 < 0.0f)
            {
                if (test0 < test1)
                {
                    if (test1 < test2)
                        return test2;
                    else
                        return test1;
                }
                else if (test0 < test2)
                {
                    return test2;
                }
                else
                {
                    return test0;
                }
            }
            else
            {
                if (test0 > test1)
                {
                    if (test1 > test2)
                        return test2;
                    else
                        return test1;
                }
                else if (test0 > test2)
                {
                    return test2;
                }
                else
                {
                    return test0;
                }
            }

        }

    }//end class


    //사각꼴 , 테트리곤
    public struct Tetragon
    {
        //-------------------------------------------------------------------------------
        // @ 사각꼴과 선분 교차 검사
        //-------------------------------------------------------------------------------
        static public bool Intersect(out float t, Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, LineSegment3 line)
        {
            //교차함수 분석결과 두번호출해야 한다. 삼각형의 무게중심좌표 방정식이 달라지기 때문에 어쩔 수 없다  
            bool result = Triangle.TriangleIntersect(out t, P0, P1, P2, line);
            if(false == result)
                result = Triangle.TriangleIntersect(out t, P0, P2, P3, line);

            return result;
        }
    }


    //움직이는 선분의 교차요소 구하기
    public struct MovingLineSegement3
    {
        public LineSegment3 start; //선분의 움직인 시작위치
        public LineSegment3 end; //선분의 움직인 끝위치


        static public bool Intersect(out Vector3 p0, out Vector3 p1, LineSegment3 src0, 
                                     MovingLineSegement3 line0, MovingLineSegement3 line1)
        {
            p0 = p1 = Vector3.zero;
            return false;
        }
    }


    /* ************************************************************************
    * Author: Kevin Kaiser  
    * Copyright (C) 1999,2000 Kevin Kaiser 
    * For use in 'Game Programming Gems'  
    * ************************************************************************ */
    public struct TriTri_1
    {
        public const int UP = 0;
        public const int DOWN = 1;

        public struct triangle
        {
            public Vector3 a, b, c;

            public float pA, pB, pC, pD; // Plane normals
            //public float radius;
            //public Vector3 center;
        }

        static public void Draw(triangle tri, Color color)
        {
            DebugWide.DrawLine(tri.a, tri.b, color);
            DebugWide.DrawLine(tri.a, tri.c, color);
            DebugWide.DrawLine(tri.b, tri.c, color);
        }

        /********************************************************
        * FUNCTION: line_plane_collision()
        *  PURPOSE: Use parametrics to see where on the plane of
        * tri1 the line made by a->b intersect
        ******************************************************** */
        static Vector3 Line_plane_collision(Vector3 a, Vector3 b, triangle tri1)
        {
            float final_x, final_y, final_z, final_t;
            float t, i;
            Vector3 temp;

            t = (float)0; i = (float)0;
            i += (float)(tri1.pA * b.x) + (tri1.pB * b.y) + (tri1.pC * b.z) + (tri1.pD);
            t += (float)(tri1.pA * (b.x * -1f)) + (tri1.pB * (b.y * -1f)) + (tri1.pC * (b.z * -1f));
            t += (float)(tri1.pA * a.x) + (tri1.pB * a.y) + (tri1.pC * a.z);

            // Be wary of possible divide-by-zeros here (i.e. if t==0)
            final_t = (-i) / t;

            // Vertical Line Segment
            if ((a.x == b.x) && (a.z == b.z))
            { // vertical line segment

                temp.x = a.x;
                temp.y = (-((tri1.pA * a.x) + (tri1.pC * a.z) + (tri1.pD))) / (tri1.pB);
                temp.z = a.z;

                return (temp);
            }

            final_x = (((a.x) * (final_t)) + ((b.x) * (1 - final_t)));
            final_y = (((a.y) * (final_t)) + ((b.y) * (1 - final_t)));
            final_z = (((a.z) * (final_t)) + ((b.z) * (1 - final_t)));

            temp.x = final_x;
            temp.y = final_y;
            temp.z = final_z;

            return (temp);
        }


        /********************************************************   
        * FUNCTION: point_inbetween_vertices()
        *  PURPOSE: Using parametrics, it is easy to determine   
        *           whether a point lies between two vertices:
        *           as long as t lies between 0 and 1, the point
        *           lies between the vertices.
        ******************************************************** */
        static bool Point_inbetween_vertices(Vector3 a, Vector3 b, triangle tri1)
        {
            float t, i, final_t;

            t = (float)0; i = (float)0;
            i += (float)(tri1.pA * b.x) + (tri1.pB * b.y) + (tri1.pC * b.z) + (tri1.pD);
            t += (float)(tri1.pA * (b.x * -1f)) + (tri1.pB * (b.y * -1f)) + (tri1.pC * (b.z * -1f));
            t += (float)(tri1.pA * a.x) + (tri1.pB * a.y) + (tri1.pC * a.z);

            // Be wary of possible divide-by-zeros here (i.e. if t==0)
            final_t = (-i) / t;

            if ((final_t >= 0) && (final_t <= 1))
                return true;
            else
                return false;
        }

        /*
        *********************************************************
        * FUNCTION: point_inside_triangle()
        *  PURPOSE: Determine whether the given point 'vert' lies
        *           inside the triangle 'tri' when both are
        *           "flattened" into 2D
        ********************************************************* */
        static bool Point_inside_triangle(triangle tri, Vector3 vert, bool x, bool y, bool z)
        {
            float a1 = 0f , b1 = 0f, a2 = 0f, b2 = 0f, a3 = 0f, b3 = 0f, a4 = 0f, b4 = 0f;
            float center_x = 0f , center_y = 0f;
            float m1 = 0f , m2 = 0f , m3 = 0f;
            float bb1 = 0f , bb2 = 0f , bb3 = 0f;
            int inside = 0, direction;
            bool AB_vert, BC_vert, CA_vert;

            // First make sure only one axis was "dropped"
            if (((x == false) && (y == false)) || ((x == false) && (z == false))
                || ((y == false) && (z == false)) || ((x == false) && (y == false) && (z == false)))
            {

                //printf("point_inside_triangle():more than one axis dropped, exiting\n");
                return false;
            }

            if (x == false)
            { // dropping x coordinate
                a1 = tri.a.y;
                b1 = tri.a.z;
                a2 = tri.b.y;
                b2 = tri.b.z;
                a3 = tri.c.y;
                b3 = tri.c.z;
                a4 = vert.y;
                b4 = vert.z;
                inside = 0;
            }
            else if (y == false)
            { // dropping y coordinate
                a1 = tri.a.x;
                b1 = tri.a.z;
                a2 = tri.b.x;
                b2 = tri.b.z;
                a3 = tri.c.x;
                b3 = tri.c.z;
                a4 = vert.x;
                b4 = vert.z;
                inside = 0;
            }
            else if (z == false)
            { // dropping z coordinate
                a1 = tri.a.x;
                b1 = tri.a.y;
                a2 = tri.b.x;
                b2 = tri.b.y;
                a3 = tri.c.x;
                b3 = tri.c.y;
                a4 = vert.x;
                b4 = vert.y;
                inside = 0;
            }

//# ifdef DEBUG
//            if (x == false)
//            {
//                drawline(0, a1, b1, 0, a2, b2);
//                drawline(0, a2, b2, 0, a3, b3);
//                drawline(0, a3, b3, 0, a1, b1);
//            }
//            else if (y == false)
//            {
//                drawline(a1, 0, b1, a2, 0, b2);
//                drawline(a2, 0, b2, a3, 0, b3);
//                drawline(a3, 0, b3, a1, 0, b1);
//            }
//            else if (z == false)
//            {
//                drawline(a1, b1, 0, a2, b2, 0);
//                drawline(a2, b2, 0, a3, b3, 0);
//                drawline(a3, b3, 0, a1, b1, 0);
//            }
//#endif

            AB_vert = BC_vert = CA_vert = false;

            // y=mx+b for outer 3 lines
            if ((a2 - a1) != 0)
            {
                m1 = (b2 - b1) / (a2 - a1); // a->b
                bb1 = (b1) - (m1 * a1);    // y/(mx) using vertex a
            }
            else if ((a2 - a1) == 0)
            {
                AB_vert = true;
            }

            if ((a3 - a2) != 0)
            {
                m2 = (b3 - b2) / (a3 - a2); // b->c
                bb2 = (b2) - (m2 * a2);    // y/(mx) using vertex b
            }
            else if ((a3 - a2) == 0)
            {
                BC_vert = true;
            }

            if ((a1 - a3) != 0)
            {
                m3 = (b1 - b3) / (a1 - a3); // c->a
                bb3 = (b3) - (m3 * a3);    // y/(mx) using vertex c
            }
            else if ((a1 - a3) == 0)
            {
                CA_vert = true;
            }

            // find average point of triangle (point is guaranteed
            center_x = (a1 + a2 + a3) / 3f;        // to lie inside the triangle)
            center_y = (b1 + b2 + b3) / 3f;

//# ifdef DEBUG
//            if (x == false)
//            {
//                drawcross(0, center_x, center_y, 2);
//                drawcross(0, vert->y, vert->z, 1);
//            }
//            else if (y == FALSE)
//            {
//                drawcross(center_x, 0, center_y, 2);
//                drawcross(vert->x, 0, vert->z, 1);
//            }
//            else if (z == FALSE)
//            {
//                drawcross(center_x, center_y, 0, 2);
//                drawcross(vert->x, vert->y, 0, 1);
//            }
//            drawcross(vert->x, vert->y, vert->z, 1);
//#endif


            // See whether (center_x,center_y) is above or below the line,
            // then set direction to UP if the point is above or DOWN if the
            // point is below the line

            // a->b
            if (((m1 * center_x) + bb1) >= center_y)
                direction = UP;
            else
                direction = DOWN;
            if (AB_vert == true)
            {
                if ((a1 < a4) && (a1 < center_x)) // vert projected line
                    inside++;
                else if ((a1 > a4) && (a1 > center_x)) // vert projected line
                    inside++;
            }
            else
            {
                if (direction == UP)
                {
                    if (b4 <= ((m1 * a4) + bb1)) // b4 less than y to be inside
                        inside++;              // (line is above point)
                }
                else if (direction == DOWN)
                {
                    if (b4 >= ((m1 * a4) + bb1)) // b4 greater than y to be inside
                        inside++;              // (line is below point)
                }
            }

            // b->c
            if (((m2 * center_x) + bb2) >= center_y)
                direction = UP;
            else
                direction = DOWN;
            if (BC_vert == true)
            {
                if ((a2 < a4) && (a2 < center_x)) // vert projected line
                    inside++;
                else if ((a2 > a4) && (a2 > center_x)) // vert projected line
                    inside++;
            }
            else
            {
                if (direction == UP)
                {
                    if (b4 <= ((m2 * a4) + bb2)) // b4 less than y to be inside 
                        inside++;              // (line is above point)
                }
                else if (direction == DOWN)
                {
                    if (b4 >= ((m2 * a4) + bb2)) // b4 greater than y to be inside
                        inside++;              // (line is below point)
                }
            }
            // c->a
            if (((m3 * center_x) + bb3) >= center_y)
                direction = UP;
            else
                direction = DOWN;
            if (CA_vert == true)
            {
                if ((a3 < a4) && (a3 < center_x)) // vert projected line
                    inside++;
                else if ((a3 > a4) && (a3 > center_x)) // vert projected line
                    inside++;
            }
            else
            {
                if (direction == UP)
                {
                    if (b4 <= ((m3 * a4) + bb3)) // b4 less than y to be inside 
                        inside++;              // (line is above point)
                }
                else if (direction == DOWN)
                {
                    if (b4 >= ((m3 * a4) + bb3)) // b4 greater than y to be inside 
                        inside++;              // (line is below point)
                }
            }
            if (inside == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }//end func


        //================== Test ====================
        static public bool Triangles_colliding(triangle tri1, triangle tri2)
        {
            bool temp = false; // default assignment
            Vector3 v1, v2, cross_v1xv2, p;

            //v1.x = tri1.b.x - tri1.a.x;
            //v1.y = tri1.b.y - tri1.a.y;
            //v1.z = tri1.b.z - tri1.a.z;
            v1 = (tri1.b - tri1.a);

            //v2.x = tri1.c.x - tri1.a.x;
            //v2.y = tri1.c.y - tri1.a.y;
            //v2.z = tri1.c.z - tri1.a.z;
            v2 = (tri1.c - tri1.a);


            // i  j  k
            //1x 1y 1z
            //2x 2y 2z
            //i(1y2z - 1z2y) - j(1x2z - 1z2x) + k(1x2y - 1y2x)

            // i  j  k
            //2x 2y 2z
            //1x 1y 1z
            //i(2y1z - 2z1y) - j(2x1z - 2z1x) + k(2x1y - 2y1x)

            //cross_v1xv2.x = (v2.y * v1.z - v2.z * v1.y);
            //cross_v1xv2.y = -(v2.x * v1.z - v2.z * v1.x);
            //cross_v1xv2.z = (v2.x * v1.y - v2.y * v1.x);
            ////cross_v1xv2 = Vector3.Cross(v1, v2);
            cross_v1xv2 = Vector3.Cross(v2, v1);

            tri1.pA = cross_v1xv2.x;
            tri1.pB = cross_v1xv2.y;
            tri1.pC = cross_v1xv2.z;
            //tri1.pD += (-(tri1.a.x)) * (cross_v1xv2.x);
            //tri1.pD += (-(tri1.a.y)) * (cross_v1xv2.y);
            //tri1.pD += (-(tri1.a.z)) * (cross_v1xv2.z);
            tri1.pD = -Vector3.Dot(tri1.a, cross_v1xv2);

            // Scroll thru 3 line segments of the other triangle
            // First iteration  (a,b)
            p = Line_plane_collision(tri2.a, tri2.b, tri1);

            // Determine which axis to project to
            // X is greatest
            if ((Mathf.Abs(tri1.pA) >= Mathf.Abs(tri1.pB)) && (Mathf.Abs(tri1.pA) >= Mathf.Abs(tri1.pC)))
                temp = Point_inside_triangle(tri1, p, false, true, true);
            // Y is greatest
            else if ((Mathf.Abs(tri1.pB) >= Mathf.Abs(tri1.pA)) && (Mathf.Abs(tri1.pB) >= Mathf.Abs(tri1.pC)))
                temp = Point_inside_triangle(tri1, p, true, false, true);
            // Z is greatest
            else if ((Mathf.Abs(tri1.pC) >= Mathf.Abs(tri1.pA)) && (Mathf.Abs(tri1.pC) >= Mathf.Abs(tri1.pB)))
                temp = Point_inside_triangle(tri1, p, true, true, false);

            if (temp == true)
            {
                // Point needs to be checked to see if it lies between the two vertices
                // First check for the special case of vertical line segments
                if ((tri2.a.x == tri2.b.x) && (tri2.a.z == tri2.b.z))
                {
                    if (((tri2.a.y <= p.y) && (p.y <= tri2.b.y)) ||
                        ((tri2.b.y <= p.y) && (p.y <= tri2.a.y)))
                        return true;
                }
                // End vertical line segment check

                // Now check for point on line segment
                if (Point_inbetween_vertices(tri2.a, tri2.b, tri1) == true)
                    return true;
                else
                    return false;
            }

            // Second iteration (b,c)
            p = Line_plane_collision(tri2.b, tri2.c, tri1);

            // Determine which axis to project to
            // X is greatest
            if ((Mathf.Abs(tri1.pA) >= Mathf.Abs(tri1.pB)) && (Mathf.Abs(tri1.pA) >= Mathf.Abs(tri1.pC)))
                temp = Point_inside_triangle(tri1, p, false, true, true);
            // Y is greatest
            else if ((Mathf.Abs(tri1.pB) >= Mathf.Abs(tri1.pA)) && (Mathf.Abs(tri1.pB) >= Mathf.Abs(tri1.pC)))
                temp = Point_inside_triangle(tri1, p, true, false, true);
            // Z is greatest
            else if ((Mathf.Abs(tri1.pC) >= Mathf.Abs(tri1.pA)) && (Mathf.Abs(tri1.pC) >= Mathf.Abs(tri1.pB)))
                temp = Point_inside_triangle(tri1, p, true, true, false);

            if (temp == true)
            {
                // Point needs to be checked to see if it lies between the two vertices
                // First check for the special case of vertical line segments
                if ((tri2.b.x == tri2.c.x) && (tri2.b.z == tri2.c.z))
                {
                    if (((tri2.b.y <= p.y) && (p.y <= tri2.c.y)) ||
                        ((tri2.c.y <= p.y) && (p.y <= tri2.b.y)))
                        return true;
                }

                // Now check for point on line segment
                if (Point_inbetween_vertices(tri2.b, tri2.c, tri1) == true)
                    return true;
                else
                    return false;
            }

            // Third iteration  (c,a)
            p = Line_plane_collision(tri2.c, tri2.a, tri1);

            // Determine which axis to project to
            // X is greatest
            if ((Mathf.Abs(tri1.pA) >= Mathf.Abs(tri1.pB)) && (Mathf.Abs(tri1.pA) >= Mathf.Abs(tri1.pC)))
                temp = Point_inside_triangle(tri1, p, false, true, true);
            // Y is greatest
            else if ((Mathf.Abs(tri1.pB) >= Mathf.Abs(tri1.pA)) && (Mathf.Abs(tri1.pB) >= Mathf.Abs(tri1.pC)))
                temp = Point_inside_triangle(tri1, p, true, false, true);
            // Z is greatest
            else if ((Mathf.Abs(tri1.pC) >= Mathf.Abs(tri1.pA)) && (Mathf.Abs(tri1.pC) >= Mathf.Abs(tri1.pB)))
                temp = Point_inside_triangle(tri1, p, true, true, false);

            if (temp == true)
            {
                // Point needs to be checked to see if it lies between the two vertices
                // First check for the special case of vertical line segments
                if ((tri2.c.x == tri2.a.x) && (tri2.c.z == tri2.a.z))
                {
                    if (((tri2.c.y <= p.y) && (p.y <= tri2.a.y)) ||
                        ((tri2.a.y <= p.y) && (p.y <= tri2.c.y)))
                        return true;
                }

                // Now check for point on line segment
                if (Point_inbetween_vertices(tri2.c, tri2.a, tri1) == true)
                    return true; // Intersection point is inside the triangle and on
                else           // the line segment
                    return false;
            }
            return false; // Default value/no collision
        }


    }//end struct

}