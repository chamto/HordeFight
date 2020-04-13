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
        public Vector3 origin;
        public Vector3 direction;

        public Line3(Vector3 ori, Vector3 dir )
        {
            direction = dir;
            origin = ori;
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
            //DebugWide.LogBlue(denom + "  " + a + "  " + b + "  " + c + "   " + line1.direction);

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
        public Vector3 origin;
        public Vector3 direction;

        public Ray3(Vector3 ori, Vector3 dir)
        {
            direction = dir;
            origin = ori;
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
        //public Vector3 direction;
        public Vector3 origin;
        public Vector3 last;

        public LineSegment3(Vector3 in_origin, Vector3 in_last)
        {
            origin = in_origin;
            //direction = in_last - in_origin;
            last = in_last;
        }

        public Vector3 direction
        {
            get
            {
                return last - origin;
            }
        }
        //public Vector3 last
        //{
        //    get
        //    {
        //        return origin + direction;
        //    }

        //    //set
        //    //{
        //    //    direction = value - origin;
        //    //}
        //}
        //public float last_x
        //{
        //    set
        //    {
        //        direction.x = value - origin.x;
        //    }
        //}
        //public float last_y
        //{
        //    set
        //    {
        //        direction.y = value - origin.y;
        //    }
        //}
        //public float last_z
        //{
        //    set
        //    {
        //        direction.z = value - origin.z;
        //    }
        //}

        public static LineSegment3 zero
        {
            get
            {
                LineSegment3 l = new LineSegment3();
                l.last = ConstV.v3_zero;
                l.origin = ConstV.v3_zero;
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

        public void Draw(Color color)
        {
            DebugWide.DrawLine(origin, last, color);
            DebugWide.DrawCircle(origin, 0.05f, color);
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
            {   //평면의 방향을 계산 할 수 없을때 
                //_normal = Vector3.up;
                _normal = ConstV.v3_zero; //에러만 안나게 0값을 설정
                _offset = 0.0f;
            }
            else
            {
                //float recip = 1.0f / lensq; //이건 아닌것 같음 
                //float recip = 1f / (float)Math.Sqrt(lensq); lensq 가 0이 될때 NaN 에러가 발생하는 것으로 추정 
                //DebugWide.LogRed(recip + "  " + lensq + "  ");
                //_normal.Set(w.x * recip, w.y * recip, w.z * recip);
                _normal = VOp.Normalize(w); //위 코드는 NaN 에러가 간혹발생 할수 있음. 예외처리가 들어간 코드로 변경 

                _offset = -1f * Vector3.Dot(_normal,p0);
            }
        }

        //점에서 평면까지의 최소거리에 있는 평면위의 점 
        // closest point
        public Vector3 ClosestPoint(Vector3 point )
        {
            //point 위치에서 평면상 수직인 점으로 이동(내림)
            return point - Test(point)*_normal; 
        }

        //점에서 평면까지의 최소거리 
        // distance
        public float Distance(Vector3 point)
        {
            return Math.Abs(Test(point));
        }

        //점에서 평면까지의 부호가 있는 최소거리 
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



    //움직이는 선분의 교차요소 구하기
    public class MovingSegement3
    {
        private LineSegment3 _prev_seg_A;
        private LineSegment3 _prev_seg_B;
        public LineSegment3 _cur_seg_A;
        public LineSegment3 _cur_seg_B;

        //삼각형 0,1 합친모양의 사각형
        private Tetragon3 _tetr01; 
        private Tetragon3 _tetr23;

        private IntrTriangle3Triangle3 _intr_0_2;
        private IntrTriangle3Triangle3 _intr_0_3;
        private IntrTriangle3Triangle3 _intr_1_2;
        private IntrTriangle3Triangle3 _intr_1_3;
        private Vector3 _minV, _maxV;

        public MovingSegement3()
        {
            _tetr01 = new Tetragon3();
            _tetr23 = new Tetragon3();
            _tetr01.tri0 = Triangle3.Zero();
            _tetr01.tri1 = Triangle3.Zero();
            _tetr23.tri0 = Triangle3.Zero();
            _tetr23.tri1 = Triangle3.Zero();
            _intr_0_2 = new IntrTriangle3Triangle3(_tetr01.tri0, _tetr23.tri0);
            _intr_0_3 = new IntrTriangle3Triangle3(_tetr01.tri0, _tetr23.tri1);
            _intr_1_2 = new IntrTriangle3Triangle3(_tetr01.tri1, _tetr23.tri0);
            _intr_1_3 = new IntrTriangle3Triangle3(_tetr01.tri1, _tetr23.tri1);
        }

        //CalcSegment_PushPoint 함수 호출전 초기값 prev 와 cur 을 같게 만들어 줘야 한다  
        //public void InitSegAB(LineSegment3 segA, LineSegment3 segB)
        //{
        //    _cur_seg_A = segA;
        //    _prev_seg_A = segA;

        //    _cur_seg_B = segB;
        //    _prev_seg_B = segB;
        //}


        public void Draw()
        {
            if(__result_meet)
            {
                //min, max
                DebugWide.DrawCircle(_minV, 0.02f, Color.white);
                DebugWide.DrawCircle(_maxV, 0.04f, Color.white);

                //meetPt
                DebugWide.DrawCircle(__meetPt, 0.5f, Color.red); //chamto test
            }
            
            if (__isSeg_A && __isSeg_B)
            {   //선분과 선분

                _prev_seg_A.Draw(Color.blue);
                _prev_seg_B.Draw(Color.magenta);
                if(true == __intr_seg_seg)
                {
                    DebugWide.DrawCircle(__cpPt0, 0.05f, Color.red);
                    //DebugWide.DrawCircle(__cpPt1, 0.05f, Color.red);
                    //DebugWide.DrawLine(__cpPt0, __cpPt1, Color.red);    
                }


            }
            else
            {   //삼각형과 삼각형 

                _tetr01.Draw(Color.blue);
                _tetr23.Draw(Color.magenta);
                _intr_0_2.Draw(Color.red);
                _intr_0_3.Draw(Color.red);
                _intr_1_2.Draw(Color.red);
                _intr_1_3.Draw(Color.red);

            }

        }

        //ctp : 접촉점 
        //stand : 정렬하기 위한 기준점 
        public void SortMinMax(Vector3 ctP, Vector3 stand, 
                                         ref Vector3 minV , ref Vector3 maxV , ref float minD , ref float maxD)
        {
            float a = (stand - ctP).sqrMagnitude;
            if (a <= minD)
            {
                minV = ctP;
                minD = a;
            }
            if (a > maxD)
            {
                maxV = ctP;
                maxD = a;
            }
        }

        //count : 찾은 접촉점(최대6개)에 대하여 검색할 최대 개수 지정
        // 예) 찾은 접촉점이 점과 선분에 해당한다면 개수를 2로 지정 , 3이상은 평면
        public bool GetMinMax_ContactPt(Vector3 comparisonSt, out Vector3 minV, out Vector3 maxV , int count)
        {
            bool result = false;
            minV = ConstV.v3_zero;
            maxV = ConstV.v3_zero;
            float minD = 1000000, maxD = 0;
            //int count = 2;
            for (int i = 0; i < count;i++)
            {
                //교점이 점 , 선분일때 처리 
                if(i < _intr_0_2.mQuantity)
                {
                    SortMinMax(_intr_0_2.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i02:"+_intr_0_2.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_0_3.mQuantity)
                {
                    SortMinMax(_intr_0_3.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i03:" + _intr_0_3.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_1_2.mQuantity)
                {
                    SortMinMax(_intr_1_2.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i12:" + _intr_1_2.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_1_3.mQuantity)
                {
                    SortMinMax(_intr_1_3.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i13:" + _intr_1_3.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
            }
            return result;
        }

        // ** meetPt 를 지나는 적당한(근사값) 선분 구하기 ** 
        // ** start 와 end 의 길이가 같다는 전제로 작성된 알고리즘 **
        //선분이 start => end 로 이동시 사다리꼴 모양의 평면이 만들어진다 
        //사다리꼴의 위쪽을 v_up , 아래쪽을 v_down 하자
        //v_up 과 v_down 의 길이가 작은쪽을 기준으로 새로운 선분의 시작점을(origin) 구한다
        //새로운 선분은 origin 에서 meetPt 를 지난다 
        private void CalcSegment(Vector3 meetPt, LineSegment3 start, LineSegment3 end, out LineSegment3 newSeg)
        {

            Vector3 v_up = end.last - start.last;
            Vector3 v_down = end.origin - start.origin;
            float len_up = v_up.sqrMagnitude;
            float len_down = v_down.sqrMagnitude;

            Vector3 n_left = VOp.Normalize(start.direction);
            Vector3 n_right = VOp.Normalize(end.direction);
            float len_proj_left = Vector3.Dot(n_left, (meetPt - start.origin));
            float len_proj_right = Vector3.Dot(n_right, (meetPt - end.origin));
            float len_perp_left = ((n_left * len_proj_left + start.origin) - meetPt).magnitude;
            float len_perp_right = ((n_right * len_proj_right + end.origin) - meetPt).magnitude;
            float rate = len_perp_left / (len_perp_left + len_perp_right);

            Vector3 origin, last;
            float len_start = start.direction.magnitude;
            //작은쪽을 선택 
            if(len_up > len_down)
            {
                origin = v_down * rate + start.origin;

                last = VOp.Normalize(meetPt - origin) * len_start + origin;

            }else
            {
                last = v_up * rate + start.last;

                origin = VOp.Normalize(meetPt - last) * len_start + last;
            }
            newSeg = new LineSegment3(origin, last);

        }

        //fixedOriginPt : 고정된 새로운 선분의 출발점 
        private void CalcSegment(bool allowFixed, Vector3 fixedOriginPt, Vector3 meetPt, LineSegment3 start, LineSegment3 end, out LineSegment3 newSeg)
        {
            Vector3 origin, last;
            float len_start = start.direction.magnitude;

            if(allowFixed)
            {
                origin = fixedOriginPt;
                last = VOp.Normalize(meetPt - origin) * len_start + origin;    
            }else
            {
                Vector3 v_up = end.last - start.last;
                Vector3 v_down = end.origin - start.origin;
                float len_up = v_up.sqrMagnitude;
                float len_down = v_down.sqrMagnitude;

                Vector3 n_left = VOp.Normalize(start.direction);
                Vector3 n_right = VOp.Normalize(end.direction);
                float len_proj_left = Vector3.Dot(n_left, (meetPt - start.origin));
                float len_proj_right = Vector3.Dot(n_right, (meetPt - end.origin));
                float len_perp_left = ((n_left * len_proj_left + start.origin) - meetPt).magnitude;
                float len_perp_right = ((n_right * len_proj_right + end.origin) - meetPt).magnitude;
                float rate = len_perp_left / (len_perp_left + len_perp_right);

                 
                //NaN 예외처리 추가 
                if(Misc.IsZero(len_perp_left + len_perp_right))
                {
                    //DebugWide.LogYellow("prev: " + start + " cur: " + end + "  left: " + len_perp_left +"   right: "+ len_perp_right);
                    rate = 0;
                }
                    

                //작은쪽을 선택 
                if (len_up > len_down)
                {
                    origin = v_down * rate + start.origin;

                    last = VOp.Normalize(meetPt - origin) * len_start + origin;

                }
                else
                {
                    last = v_up * rate + start.last;

                    origin = VOp.Normalize(meetPt - last) * len_start + last;
                }
            }


            newSeg = new LineSegment3(origin, last);
        }

        //minSeg 선분에서 maxPt 를 지나는 새로운 선분을 구한다 
        private void CalcSegment(bool allowFixed, Vector3 fixedOriginPt, Vector3 minPt, Vector3 maxPt, LineSegment3 minSeg , out LineSegment3 maxSeg)
        {
            Vector3 origin, last;
            float len_curSeg = minSeg.direction.magnitude;

            if (allowFixed)
            {
                origin = fixedOriginPt;
                last = VOp.Normalize(maxPt - origin) * len_curSeg + origin;
            }
            else
            {
                Vector3 dir = maxPt - minPt;
                origin = minSeg.origin + dir;
                last = minSeg.last + dir;
            }


            maxSeg = new LineSegment3(origin, last);
        }

        //최종 접촉점을 지나는 선분을 구함
        public void CalcSegment_FromContactPt()
        {
            
            Vector3 meetPt;
            if(true == GetMeetPoint(out meetPt))
            {
                //DebugWide.LogRed(meetPt); //chamto test
                if(true == __isSeg_A && false == __isSeg_B )
                {
                    CalcSegment(meetPt, _prev_seg_B, _cur_seg_B, out _cur_seg_B);
                }
                else if (false == __isSeg_A && true == __isSeg_B)
                {
                    CalcSegment(meetPt, _prev_seg_A, _cur_seg_A, out _cur_seg_A);
                }
                else if (false == __isSeg_A && false == __isSeg_B)
                {
                    CalcSegment(meetPt, _prev_seg_A, _cur_seg_A, out _cur_seg_A);
                    CalcSegment(meetPt, _prev_seg_B, _cur_seg_B, out _cur_seg_B);    
                }

                Dropping();
            }

            _prev_seg_A = _cur_seg_A;
            _prev_seg_B = _cur_seg_B;
        }

        public void CalcSegment_FromContactPt(bool allowFixed_a, bool allowFixed_b, Vector3 fixedOriginPt_a, Vector3 fixedOriginPt_b)
        {

            Vector3 meetPt;
            if (true == GetMeetPoint(out meetPt))
            {
                //DebugWide.LogRed("meetPt: " + meetPt); //chamto test
                if (true == __isSeg_A && false == __isSeg_B)
                {
                    CalcSegment(allowFixed_b, fixedOriginPt_b, meetPt, _prev_seg_B, _cur_seg_B, out _cur_seg_B);
                }
                else if (false == __isSeg_A && true == __isSeg_B)
                {
                    CalcSegment(allowFixed_a, fixedOriginPt_a, meetPt, _prev_seg_A, _cur_seg_A, out _cur_seg_A);
                }
                else if (false == __isSeg_A && false == __isSeg_B)
                {
                    CalcSegment(allowFixed_a, fixedOriginPt_a, meetPt, _prev_seg_A, _cur_seg_A, out _cur_seg_A);
                    CalcSegment(allowFixed_b, fixedOriginPt_b, meetPt, _prev_seg_B, _cur_seg_B, out _cur_seg_B);
                }

                Dropping(allowFixed_a, allowFixed_b, meetPt, fixedOriginPt_a, fixedOriginPt_b);
            }

            _prev_seg_A = _cur_seg_A;
            _prev_seg_B = _cur_seg_B;
        }


        //붙어있는 두선분을 떨어뜨리기
        public void Dropping()
        {
            __dir_A = (_cur_seg_A.origin - _prev_seg_A.origin) + (_cur_seg_A.last - _prev_seg_A.last);
            __dir_B = (_cur_seg_B.origin - _prev_seg_B.origin) + (_cur_seg_B.last - _prev_seg_B.last);

            float RADIUS = 0.01f;
            bool zero_a, zero_b;
            float sensibility = float.Epsilon; //이 값이 크면 zero로 판단하는 영역이 커진다 
            //float sensibility = 0.0000000001f;
            zero_a = Misc.IsZero(__dir_A, sensibility);
            zero_b = Misc.IsZero(__dir_B, sensibility);
            Vector3 n_dir;
            //방향성 정보가 없는 경우
            if (zero_a && zero_b)
            {
                //DebugWide.LogGreen("zero_a,b : " + __dir_A + "   " + __dir_B);
                n_dir = Vector3.Cross(_cur_seg_A.direction, _cur_seg_B.direction);
                n_dir = VOp.Normalize(n_dir);
                _cur_seg_A.origin = _cur_seg_A.origin + n_dir * RADIUS;
                _cur_seg_A.last = _cur_seg_A.last + n_dir * RADIUS;
                _cur_seg_B.origin = _cur_seg_B.origin + n_dir * -RADIUS;
                _cur_seg_B.last = _cur_seg_B.last + n_dir * -RADIUS;
            }
            //방향성 정보가 있는 경우 
            else
            {
                if (false == zero_a)
                {
                    //DebugWide.LogGreen("___non_zero_a" + __dir_A);
                    n_dir = VOp.Normalize(__dir_A);
                    _cur_seg_A.origin = _cur_seg_A.origin + n_dir * -RADIUS;
                    _cur_seg_A.last = _cur_seg_A.last + n_dir * -RADIUS;
                }
                if (false == zero_b)
                {
                    //DebugWide.LogGreen("___non_zero_b" + __dir_B);
                    n_dir = VOp.Normalize(__dir_B);
                    _cur_seg_B.origin = _cur_seg_B.origin + n_dir * -RADIUS;
                    _cur_seg_B.last = _cur_seg_B.last + n_dir * -RADIUS;
                }
            }

        }


        public void Dropping(bool allowFixed_a, bool allowFixed_b, Vector3 meetPt, Vector3 fixedOriginPt_a, Vector3 fixedOriginPt_b)
        {
            //DebugWide.LogBlue("dropping -----");
            __dir_A = (_cur_seg_A.origin - _prev_seg_A.origin) + (_cur_seg_A.last - _prev_seg_A.last);
            __dir_B = (_cur_seg_B.origin - _prev_seg_B.origin) + (_cur_seg_B.last - _prev_seg_B.last);

            float RADIUS = 0.001f;
            float ANGLE = 0.1f;
            bool zero_a, zero_b;
            float sensibility = float.Epsilon; //이 값이 크면 zero로 판단하는 영역이 커진다 
            //float sensibility = 0.00000001f;
            //__dir_A = VOp.Normalize(__dir_A); //방향값 증폭 
            //__dir_B = VOp.Normalize(__dir_B);
            zero_a = Misc.IsZero(__dir_A, sensibility);
            zero_b = Misc.IsZero(__dir_B, sensibility);
            Vector3 n_dir;

            //-----
            //꼬임이 바뀌었는지 검사
            //Plane plane = new Plane(_cur_seg_A.origin, _cur_seg_A.last, _cur_seg_B.origin);
            //float test = plane.Test(_cur_seg_B.last);
            ////DebugWide.LogBlue("test:  " + test);
            float sign = 1;
            //if (test >= 0)
            //{   //a위에b
            //    sign = 1;
            //}
            //else if (test < 0)
                //sign = -1f;
            //-----

            //방향성 정보가 없는 경우
            if (zero_a && zero_b)
            {
                //DebugWide.LogGreen("zero_a,b : "+ VOp.ToString(__dir_A) + "   " + VOp.ToString(__dir_B));
                //return; //방향성 정보가 없는 경우 처리하지 않는다. 고정되어야 하는 상황에서 조금씩 이동되는 현상이 발생하는 원인임 

                n_dir = Vector3.Cross(_cur_seg_A.direction, _cur_seg_B.direction);
                n_dir = VOp.Normalize(n_dir);

                if (allowFixed_a)
                {
                    float len = _cur_seg_A.Length();
                    _cur_seg_A.origin = fixedOriginPt_a;
                    _cur_seg_A.last = fixedOriginPt_a + VOp.Normalize(meetPt - fixedOriginPt_a) * len;
                    Vector3 axis = Vector3.Cross(_cur_seg_A.direction, n_dir);
                    _cur_seg_A.last = Quaternion.AngleAxis(ANGLE, axis) * _cur_seg_A.direction + fixedOriginPt_a;
                }
                else
                {
                    _cur_seg_A.origin = _cur_seg_A.origin + n_dir * RADIUS * sign;
                    _cur_seg_A.last = _cur_seg_A.last + n_dir * RADIUS * sign;
                }

                //====
                if (allowFixed_b)
                {
                    float len = _cur_seg_B.Length();
                    _cur_seg_B.origin = fixedOriginPt_b;
                    _cur_seg_B.last = fixedOriginPt_b + VOp.Normalize(meetPt - fixedOriginPt_b) * len;
                    Vector3 axis = Vector3.Cross(_cur_seg_B.direction, -n_dir);
                    _cur_seg_B.last = Quaternion.AngleAxis(ANGLE, axis) * _cur_seg_B.direction + fixedOriginPt_b;
                }
                else
                {
                    _cur_seg_B.origin = _cur_seg_B.origin + n_dir * -RADIUS * sign;
                    _cur_seg_B.last = _cur_seg_B.last + n_dir * -RADIUS * sign;
                }
            }
            //방향성 정보가 있는 경우 
            else
            {
                if (false == zero_a)
                {
                    //DebugWide.LogGreen("___non_zero_a" + VOp.ToString(__dir_A));
                    n_dir = VOp.Normalize(__dir_A);
                    if(allowFixed_a)
                    {
                        float len = _cur_seg_A.Length();
                        _cur_seg_A.origin = fixedOriginPt_a;
                        _cur_seg_A.last = fixedOriginPt_a + VOp.Normalize(meetPt - fixedOriginPt_a) * len;
                        Vector3 axis = Vector3.Cross(_cur_seg_A.direction, -n_dir);
                        _cur_seg_A.last = Quaternion.AngleAxis(ANGLE, axis) * _cur_seg_A.direction + fixedOriginPt_a;
                    }
                    else
                    {
                        _cur_seg_A.origin = _cur_seg_A.origin + n_dir * -RADIUS;
                        _cur_seg_A.last = _cur_seg_A.last + n_dir * -RADIUS;
                    }

                }
                if (false == zero_b)
                {
                    //DebugWide.LogGreen("___non_zero_b" + VOp.ToString(__dir_B));
                    n_dir = VOp.Normalize(__dir_B);
                    if (allowFixed_b)
                    {
                        float len = _cur_seg_B.Length();
                        _cur_seg_B.origin = fixedOriginPt_b;
                        _cur_seg_B.last = fixedOriginPt_b + VOp.Normalize(meetPt - fixedOriginPt_b) * len;
                        Vector3 axis = Vector3.Cross(_cur_seg_B.direction, -n_dir);
                        _cur_seg_B.last = Quaternion.AngleAxis(ANGLE, axis) * _cur_seg_B.direction + fixedOriginPt_b;
                    }
                    else
                    {
                        _cur_seg_B.origin = _cur_seg_B.origin + n_dir * -RADIUS;
                        _cur_seg_B.last = _cur_seg_B.last + n_dir * -RADIUS;
                    }
                }
            }
        }

        Vector3 __meetPt;
        bool __result_meet = false;
        //두 움직이는 선분이 만나는 하나의 교점. 교차객체에서 교점정보를 분석해 하나의 만나는 점을 찾음 
        public bool GetMeetPoint(out Vector3 meetPt)
        {
            bool result = false;
            meetPt = ConstV.v3_zero;


            //선분과 선분이 만난 경우 
            if (__isSeg_A && __isSeg_B)
            {
                if(true == __intr_seg_seg)
                {
                    //DebugWide.LogBlue("!! 선분 vs 선분  ");
                    meetPt = __cpPt0;
                    result = true;
                }
            }
            else
            {
                //if (0 != _intr_0_2.mQuantity ||
                //    0 != _intr_0_3.mQuantity ||
                //    0 != _intr_1_2.mQuantity ||
                //    0 != _intr_1_3.mQuantity)
                //{
                //    DebugWide.LogBlue("----- intr -----  " + __isSeg_A + "  " + __isSeg_B);
                //    DebugWide.LogBlue("intr_0_2  " + _intr_0_2.ToString());
                //    DebugWide.LogBlue("intr_0_3  " + _intr_0_3.ToString());
                //    DebugWide.LogBlue("intr_1_2  " + _intr_1_2.ToString());
                //    DebugWide.LogBlue("intr_1_3  " + _intr_1_3.ToString());
                //}


                //사각꼴이 서로 같은 평면에서 만난경우
                if(eIntersectionType.PLANE == _intr_0_2.mIntersectionType  ||
                    eIntersectionType.PLANE == _intr_0_3.mIntersectionType  ||
                    eIntersectionType.PLANE == _intr_1_2.mIntersectionType  ||
                    eIntersectionType.PLANE == _intr_1_3.mIntersectionType  )
                {
                    //DebugWide.LogBlue("!! 사각꼴(선분) vs 사각꼴(선분)  ");
                    //선분과 사각꼴이 같은 평면에서 만난경우
                    if (__isSeg_A || __isSeg_B)
                    {
                        
                        if(true == __isSeg_A)
                        {
                            result = GetMinMax_ContactPt(_prev_seg_B.origin, out _minV, out _maxV, 4);
                            if(0 < Vector3.Dot(__dir_B, (_minV-_maxV)))
                            {
                                meetPt = _maxV;
                            }else
                            {
                                meetPt = _minV;
                            }
                        }else
                        {
                            result = GetMinMax_ContactPt(_prev_seg_A.origin, out _minV, out _maxV, 4);
                            if (0 < Vector3.Dot(__dir_A, (_minV - _maxV)))
                            {
                                meetPt = _maxV;
                            }
                            else
                            {
                                meetPt = _minV;
                            }
                        }

                    }
                    //사각꼴과 사각꼴이 같은 평면에서 만난경우 
                    //(false == __b_A && false == __b_B)
                    else
                    {
                        //DebugWide.LogRed("!! 사각꼴과 사각꼴이 같은 평면에서 만난경우 ");
                        //todo : 두 평면의 겹치는 영역에서 한점을 선택해야 한다. 추후 필요하면 구현하기 
                    }
                }
                //사각꼴(선분)이 서로 엇갈려 만난경우
                else
                {   
                    //DebugWide.LogBlue("!! 사각꼴(선분)이 서로 엇갈려 만난경우 ");
                    result = GetMinMax_ContactPt(_cur_seg_A.origin, out _minV, out _maxV, 2);
                    meetPt = _minV + (_maxV - _minV) * 0.5f; //중간지점을 만나는 점으로 삼는다 
                }
                

            }

            __meetPt = meetPt;
            __result_meet = result;
            return result;
        }



        //rate 0.5 : a,b 선분의 최소최대 교차영역의 중간지점을 접촉점으로 삼는다
        //rate 0.0 : b 선분 최대
        //rate 1.0 : a 선분 최대 
        public bool CalcSegment_PushPoint(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Vector3 fixedOriginPt_a, Vector3 fixedOriginPt_b)
        {
            bool result = false;
            bool result_2 = false;
            Vector3 meetPt = ConstV.v3_zero;

            //선분과 선분이 만난 경우 
            if (__isSeg_A && __isSeg_B)
            {
                if (true == __intr_seg_seg)
                {
                    //DebugWide.LogBlue("!! 선분 vs 선분  ");    
                    meetPt = __cpPt0;
                    result = true;
                }
            }
            else
            {
                //DebugWide.LogBlue("!! 사각꼴(선분) vs 사각꼴(선분)  ");
                //사각꼴이 서로 같은 평면에서 만난경우
                if (eIntersectionType.PLANE == _intr_0_2.mIntersectionType ||
                    eIntersectionType.PLANE == _intr_0_3.mIntersectionType ||
                    eIntersectionType.PLANE == _intr_1_2.mIntersectionType ||
                    eIntersectionType.PLANE == _intr_1_3.mIntersectionType)
                {
                    
                    //선분과 사각꼴이 같은 평면에서 만난경우
                    if (true == __isSeg_A && false == __isSeg_B)
                    {
                        //DebugWide.LogBlue("!! 선분 vs 사각꼴 ");
                        result = GetMinMax_ContactPt(_prev_seg_B.origin, out _minV, out _maxV, 4);
                    }
                    else if (false == __isSeg_A && true == __isSeg_B)
                    {
                        //DebugWide.LogBlue("!! 사각꼴 vs 선분 ");
                        result = GetMinMax_ContactPt(_prev_seg_A.origin, out _minV, out _maxV, 4);
                    }
                    //사각꼴과 사각꼴이 같은 평면에서 만난경우 
                    else if (false == __isSeg_A && false == __isSeg_B)
                    {
                        //!!!! 초기에 prev 와 cur 을 같게 안만들어주면 여기로 처리가 들어오게 된다
                        //  잘못된 정보에 의해 접촉한것으로 계산하게 되며 초기값에 의해 방향성을 가지게 되어 
                        //  dropping 에서 잘못된 방향으로 무한히 떨어뜨리게 된다.
                        //DebugWide.LogRed("!! 사각꼴과 사각꼴이 같은 평면에서 만난경우 ");
                        result = GetMinMax_ContactPt(_cur_seg_A.origin, out _minV, out _maxV, 6);

                    }
                    meetPt = _minV + (_maxV - _minV) * rateAtoB;

                    //if(result)
                        //DebugWide.LogBlue("!! 사각꼴(선분)이 같은 평면에서 만난 경우 ");

                }
                //사각꼴(선분)이 서로 엇갈려 만난경우
                else
                {
                    result = GetMinMax_ContactPt(_cur_seg_A.origin, out _minV, out _maxV, 2);
                    result_2 = true;

                    //사각꼴과 선분이 만난 경우 : 교점이 하나만 나오므로 max를 따로 구해야 한다 
                    if (true == __isSeg_A)
                    {
                        //min 구하기 
                        Line3.ClosestPoints(out _minV, out _minV, new Line3(_maxV, __dir_B), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                        //Line3.ClosestPoints(out _maxV, out _maxV, new Line3(_minV, __dir_B), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                        //DebugWide.LogRed("segA max : " + _maxV + "   " + _minV + "   " + __dir_B); //chamto test
                    }
                    else if (true == __isSeg_B)
                    {
                        //max 구하기
                        Line3.ClosestPoints(out _maxV, out _maxV, new Line3(_minV, __dir_A), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));
                        //DebugWide.LogRed("segB max : " + _maxV + "   " + _minV + "   " + __dir_A + "   " + _cur_seg_B.direction); //chamto test
                    }

                    meetPt = _minV + (_maxV - _minV) * rateAtoB;

                    //if(result)
                        //DebugWide.LogBlue("!! 사각꼴(선분)이 서로 엇갈려 만난 경우 ");
                }
            }

            if(result)
            {
                
                //** 사각꼴(선분)이 서로 엇갈려 만난경우 : rateAtoB 값에 따라 밀어내기 처리를 한다 
                //사각꼴에서 meetPt를 지나는 새로운 선분 구한다
                if (false == __isSeg_A)
                {
                    CalcSegment(allowFixed_a, fixedOriginPt_a, meetPt, _prev_seg_A, _cur_seg_A, out _cur_seg_A);
                    //DebugWide.LogBlue("1 +++ : " +  _prev_seg_A + "  |||  " + _cur_seg_A); //chamto test
                }
                if (false == __isSeg_B)
                {
                    CalcSegment(allowFixed_b, fixedOriginPt_b, meetPt, _prev_seg_B, _cur_seg_B, out _cur_seg_B);
                    //DebugWide.LogBlue("2 +++ : " + _prev_seg_B + "  |||  " + _cur_seg_B); //chamto test
                } 

                //기존 선분에서 meetPt를 지나는 새로운 선분 구한다 , prev 선분값을 cur 로 갱신한다  
                if(true == result_2)
                {
                    if (true == __isSeg_A)
                    {
                        CalcSegment(allowFixed_a, fixedOriginPt_a, _maxV ,meetPt , _cur_seg_A, out _cur_seg_A);
                        //CalcSegment(allowFixed_a, fixedOriginPt_a, _minV, meetPt, _cur_seg_A, out _cur_seg_A);
                        _prev_seg_A = _cur_seg_A;
                    }
                    else if (true == __isSeg_B)
                    {
                        CalcSegment(allowFixed_b, fixedOriginPt_b, _minV, meetPt, _cur_seg_B, out _cur_seg_B);
                        _prev_seg_B = _cur_seg_B;
                    }
                }

                //** 사각꼴(선분)이 같은 평면에서 만난 경우 : 떨어뜨리기 처리를 한다 
                //DebugWide.LogRed("meetPt: " + meetPt  + "   " + _cur_seg_A + "  |||  " + _cur_seg_B); //chamto test
                Dropping(allowFixed_a, allowFixed_b, meetPt, fixedOriginPt_a, fixedOriginPt_b);

            }

            _prev_seg_A = _cur_seg_A;
            _prev_seg_B = _cur_seg_B;

            __meetPt = meetPt;
            __result_meet = result;

            return result;
        }


        public void Update_Tetra(Vector3 a0_s, Vector3 a0_e, Vector3 a1_s, Vector3 a1_e, 
                                 Vector3 b0_s, Vector3 b0_e, Vector3 b1_s, Vector3 b1_e)
        {
            _cur_seg_A = new LineSegment3(a1_s , a1_e);
            _cur_seg_B = new LineSegment3(b1_s , b1_e);

            //선분의 이동방향
            __dir_A = (a1_s - a0_s) + (a1_e - a0_e);
            __dir_B = (b1_s - b0_s) + (b1_e - b0_e);
            //선분상태인지 사각꼴 상태인지 검사 
            //__isSeg_A = Misc.IsZero(a0_s - a1_s) && Misc.IsZero(a0_e - a1_e);
            //__isSeg_B = Misc.IsZero(b0_s - b1_s) && Misc.IsZero(b0_e - b1_e);
            __isSeg_A = Misc.IsZero(__dir_A);
            __isSeg_B = Misc.IsZero(__dir_B);

            _tetr01.Set(a0_s, a0_e, a1_s, a1_e);
            _tetr23.Set(b0_s, b0_e, b1_s, b1_e);
         

            {
                _intr_0_2.Find_Twice();
                _intr_0_3.Find_Twice();
                _intr_1_2.Find_Twice();
                _intr_1_3.Find_Twice();    
            }

            _prev_seg_A = new LineSegment3(a0_s, a0_e);
            _prev_seg_B = new LineSegment3(b0_s, b0_e);
        }



        bool __isSeg_A, __isSeg_B;
        bool __intr_seg_seg = false;
        float __cpS, __cpT;
        Vector3 __cpPt0;
        public Vector3 __dir_A = ConstV.v3_zero;
        public Vector3 __dir_B = ConstV.v3_zero;
        public void Find(LineSegment3 prev_segA, LineSegment3 prev_segB, LineSegment3 cur_segA, LineSegment3 cur_segB)
        {

            _cur_seg_A = cur_segA;
            _cur_seg_B = cur_segB;
            _prev_seg_A = prev_segA;
            _prev_seg_B = prev_segB;
            //--------------------

            //선분의 이동방향
            __dir_A = (_cur_seg_A.origin - _prev_seg_A.origin) + (_cur_seg_A.last - _prev_seg_A.last);
            __dir_B = (_cur_seg_B.origin - _prev_seg_B.origin) + (_cur_seg_B.last - _prev_seg_B.last);

            //방향값이 너무 작은 경우 zero 로 인식되는 문제가 있어, 정규화로 크기를 키운다 
            //__dir_A = VOp.Normalize(__dir_A); 
            //__dir_B = VOp.Normalize(__dir_B);
            //선분상태인지 사각꼴 상태인지 검사 
            //__isSeg_A = Misc.IsZero(_prev_seg_A.origin - segA.origin) && Misc.IsZero(_prev_seg_A.last - segA.last);
            //__isSeg_B = Misc.IsZero(_prev_seg_B.origin - segB.origin) && Misc.IsZero(_prev_seg_B.last - segB.last);
            __isSeg_A = Misc.IsZero(__dir_A);
            __isSeg_B = Misc.IsZero(__dir_B);

            _tetr01.Set(_prev_seg_A, _cur_seg_A);
            _tetr23.Set(_prev_seg_B, _cur_seg_B);

            //DebugWide.LogBlue(VOp.ToString(__dir_A) + "   " + VOp.ToString(__dir_B));
            if (__isSeg_A && __isSeg_B)
            {   //선분과 선분

                __intr_seg_seg = false;

                //Dropping 으로 밀은후에도 값이 커 선분이 교차한것으로 나옴  
                //Dropping zero 상태에 의해 조금씩 미는 효과가 생김 , 의도치 않은 것이므로 제거함 
                //if(0.00001f > LineSegment3.DistanceSquared(segA, segB, out __cpS, out __cpT)) 

                if (float.Epsilon > LineSegment3.DistanceSquared(_cur_seg_A, _cur_seg_B, out __cpS, out __cpT))
                //if(0.0000001f > LineSegment3.DistanceSquared(_cur_seg_A, _cur_seg_B, out __cpS, out __cpT)) 
                {
                    //DebugWide.LogRed("-----find: DistanceSquared"); //chamto test
                    __cpPt0 = _cur_seg_A.origin + _cur_seg_A.direction * __cpS;
                    __intr_seg_seg = true;
                }

            }
            //else
            {   //삼각형과 삼각형 
                _intr_0_2.Find_Twice();
                _intr_0_3.Find_Twice();
                _intr_1_2.Find_Twice();
                _intr_1_3.Find_Twice();
            }

        }

    }


    public struct Tetragon3
    {
        //v1  - v2
        // |  /  |   
        //v0  - v3
        //v0 - v1 - v2 , v0 - v2 - v3

        //a0 - a1 - a2 , b0 - b1 - b2
        //seg0 : a0 - a1 , seg1 : b2 - b1
        public Triangle3 tri0;
        public Triangle3 tri1;

        public Vector3 corner0
        {
            get { return tri0.V[1]; }
        }
        public Vector3 corner1
        {
            get { return tri1.V[2]; }
        }

        public void Set(LineSegment3 seg0 , LineSegment3 seg1)
        {
            tri0.V[0] = seg0.origin;
            tri0.V[1] = seg0.last; //모서리
            tri0.V[2] = seg1.last;

            tri1.V[0] = seg0.origin;
            tri1.V[1] = seg1.last;
            tri1.V[2] = seg1.origin; //모서리 
        }

        public void Set(Vector3 seg0_s, Vector3 seg0_e, Vector3 seg1_s, Vector3 seg1_e)
        {
            tri0.V[0] = seg0_s;
            tri0.V[1] = seg0_e;
            tri0.V[2] = seg1_e;

            tri1.V[0] = seg0_s;
            tri1.V[1] = seg1_e;
            tri1.V[2] = seg1_s;
        }

        public void Draw(Color color)
        {
            //v1  - v2
            // |  /  |   
            //v0  - v3
            Color cc = Color.gray;
            //DebugWide.DrawLine(tri0.V[0], tri0.V[1], cc);
            DebugWide.DrawLine(tri0.V[1], tri0.V[2], cc);
            DebugWide.DrawLine(tri0.V[0], tri0.V[2], cc);

            //DebugWide.DrawLine(tri1.V[2], tri1.V[1], color);
            DebugWide.DrawLine(tri1.V[0], tri1.V[2], cc);

            //선분출력
            cc = color;
            DebugWide.DrawLine(tri0.V[0], tri0.V[1], cc);
            DebugWide.DrawLine(tri1.V[2], tri1.V[1], cc);
            DebugWide.DrawCircle(tri0.V[0], 0.05f, cc); //선분 시작점
            DebugWide.DrawCircle(tri1.V[2], 0.05f, cc); //선분 시작점

        }
    }

}