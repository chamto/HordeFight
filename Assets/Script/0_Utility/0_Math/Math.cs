﻿using System.Collections;
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

        public void Draw(Color color)
        {
            DebugWide.DrawLine(origin, last, color);
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
                _normal = Vector3.up;
                _offset = 0.0f;
            }
            else
            {
                //float recip = 1.0f / lensq; //이건 아닌것 같음 
                float recip = 1f / (float)Math.Sqrt(lensq);
                _normal.Set(w.x * recip, w.y * recip, w.z * recip);
                //_normal = w * recip;
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



    //움직이는 선분의 교차요소 구하기
    public class MovingSegement3
    {
        private LineSegment3 _prev_seg0;
        private LineSegment3 _prev_seg1;


        //삼각형 0,1 합친모양의 사각형
        private Tetragon3 _tetr01; 
        private Tetragon3 _tetr23;

        private IntrTriangle3Triangle3 _intr_0_2;
        private IntrTriangle3Triangle3 _intr_0_3;
        private IntrTriangle3Triangle3 _intr_1_2;
        private IntrTriangle3Triangle3 _intr_1_3;

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


        public void Draw()
        {
            if (__s0 && __s1)
            {   //선분과 선분

                _prev_seg0.Draw(Color.blue);
                _prev_seg1.Draw(Color.magenta);
                DebugWide.DrawCircle(__cpPt0, 0.05f, Color.red);
                DebugWide.DrawCircle(__cpPt1, 0.05f, Color.red);
                DebugWide.DrawLine(__cpPt0, __cpPt1,Color.red);

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

        public void Update_Tetra(Vector3 aV0, Vector3 aV1, Vector3 aV2, Vector3 aV3, 
                                 Vector3 bV0, Vector3 bV1, Vector3 bV2, Vector3 bV3)
        {
            __s0 = __s1 = false;

            _tetr01.Set(aV0, aV1, aV2, aV3);
            _tetr23.Set(bV0, bV1, bV2, bV3);
         
            _intr_0_2.Find_Twice();
            _intr_0_3.Find_Twice();
            _intr_1_2.Find_Twice();
            _intr_1_3.Find_Twice();
        }

        bool __s0, __s1;
        Vector3 __cpPt0, __cpPt1;
        public void Update_Move(LineSegment3 seg0, LineSegment3 seg1)
        {

            __s0 = Misc.IsZero(_prev_seg0.origin - seg0.origin) && Misc.IsZero(_prev_seg0.last - seg0.last);
            __s1 = Misc.IsZero(_prev_seg1.origin - seg1.origin) && Misc.IsZero(_prev_seg1.last - seg1.last);

            _tetr01.Set(_prev_seg0, seg0);
            _tetr23.Set(_prev_seg1, seg1);


            if (__s0 && __s1)
            {   //선분과 선분
                
                LineSegment3.ClosestPoints(out __cpPt0, out __cpPt1, seg0, seg1);

            }
            else
            {   //삼각형과 삼각형 
                _intr_0_2.Find_Twice();
                _intr_0_3.Find_Twice();
                _intr_1_2.Find_Twice();
                _intr_1_3.Find_Twice();
            }

            _prev_seg0 = seg0;
            _prev_seg1 = seg1;
        }
    }


    public struct Tetragon3
    {
        //v0    v1
        //
        //v2    v3
        //v0 - v1 - v2 , v2 - v1 - v3
        public Triangle3 tri0;
        public Triangle3 tri1;

        public void Set(LineSegment3 seg0 , LineSegment3 seg1)
        {
            tri0.V[0] = seg0.last;
            tri0.V[1] = seg0.origin;
            tri0.V[2] = seg1.origin;

            tri1.V[0] = seg0.last;
            tri1.V[1] = seg1.origin;
            tri1.V[2] = seg1.last;
        }

        public void Set(Vector3 seg0_s, Vector3 seg0_e, Vector3 seg1_s, Vector3 seg1_e)
        {
            tri0.V[0] = seg0_e;
            tri0.V[1] = seg0_s;
            tri0.V[2] = seg1_s;

            tri1.V[0] = seg0_e;
            tri1.V[1] = seg1_s;
            tri1.V[2] = seg1_e;
        }

        public void Draw(Color color)
        {
            DebugWide.DrawLine(tri0.V[0], tri0.V[1], color);
            DebugWide.DrawLine(tri0.V[0], tri0.V[2], color);
            DebugWide.DrawLine(tri0.V[1], tri0.V[2], color);

            DebugWide.DrawLine(tri1.V[0], tri1.V[2], color);
            DebugWide.DrawLine(tri1.V[1], tri1.V[2], color);

            //선분출력
            //DebugWide.DrawLine(tri0.V[1], tri0.V[0], Color.red);
            //DebugWide.DrawLine(tri1.V[1], tri1.V[2], Color.red);
        }
    }

}