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

        static public Vector3 ClosestPoint(Line3 l, Vector3 point)
        {
            Vector3 w = point - l.origin;
            float vsq = Vector3.Dot(l.direction, l.direction);
            float proj =  Vector3.Dot(w, l.direction);

            return l.origin + (proj/vsq) * l.direction;

        }

        static public Vector3 ClosestPoint(Vector3 origin, Vector3 direction, Vector3 point)
        {
            Vector3 w = point - origin;
            float vsq = Vector3.Dot(direction, direction);
            float proj = Vector3.Dot(w, direction);

            return origin + (proj / vsq) * direction;

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

        //길이가 있는 방향으로 선분을 이동시킨다
        static public LineSegment3 Move(LineSegment3 tar, Vector3 dir_len)
        {
            return new LineSegment3(tar.origin + dir_len, tar.last + dir_len);
        }

        //같은 직선상의 선분이며 방향이 같은 선분이라고 가정
        //방향이 다를 경우 target 선분의 origin 과 last 를 바꾸어 방향을 맞춰준다 
        static public LineSegment3 Merge(LineSegment3 a, LineSegment3 b)
        {
            Vector3 t_origin = b.origin, t_last = b.last;
            //서로 방향이 다른 선분일 경우  
            if (0 > Vector3.Dot(a.direction, b.direction))
            {
                t_origin = b.last;
                t_last = b.origin;
            }

            Vector3 dir_o_o = t_origin - a.origin;
            Vector3 start, end;
            if(0 < Vector3.Dot(a.direction, dir_o_o))
            {
                start = a.origin;
                end = t_last;
            }
            else
            {
                start = t_origin;
                end = a.last;
            }


            return new LineSegment3(start,end);
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

        //inRange : 선분 범위 안에서 최근접이 발생했는지 여부를 나타낸다  
        public Vector3 ClosestPoint(Vector3 point , out bool inRange)
        {
            inRange = false;
            Vector3 w = point - origin;
            Vector3 dir = last - origin;
            float proj = Vector3.Dot(w, dir);
            // endpoint 0 is closest point
            if (proj <= 0.0f)
                return origin;
            else
            {
                float vsq = Vector3.Dot(dir, dir);
                // endpoint 1 is closest point
                if (proj >= vsq)
                    return last;
                // else somewhere else in segment
                else
                {
                    //선분 위에 최근접 발생
                    inRange = true;
                    return origin + (proj / vsq) * dir;
                }

            }
        }

        static public Vector3 ClosestPoint(Vector3 origin, Vector3 last, Vector3 point)
        {
            Vector3 direction = last - origin;

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

        static public bool Intersection(LineSegment3 segment0, LineSegment3 segment1)
        {
            //선분이 방향값이 없는 점일 경우 계산을 못하는 문제가 발견됨 
            float s, c;
            float sq = DistanceSquared(segment0, segment1, out s, out c);

            if (true == Misc.IsZero(sq))
            {
                return true;
            }

            return false;
        }

        static public bool Intersection(LineSegment3 segment0, LineSegment3 segment1, out float dist0, out Vector3 point0)
        {

            float s, c;
            float sq = DistanceSquared(segment0, segment1, out s, out c);

            point0 = segment0.origin + s * segment0.direction;
            dist0 = segment0.Length() * s;

            if (true == Misc.IsZero(sq))
            {
                return true;
            }

            return false;
        }

        public LineSegment3 Rotate(Vector3 pos_ori, Quaternion rot)
        {
            //trs부모 * trs자식 * vertex
            //  2        1      대상정점   <=  정점에 적용되는 순서 , s크기 <= r회전 <= t이동 순으로 곱해진다  
            //t0_sub_start = t부모 * trs자식 * vertex
            //r부모 * t부모 * trs자식 * vertex : 현재 부모의 trs순서가 안맞아 문제가 생김 , r이 먼저 곱해져야 하는데 t가 먼저 곱해짐 
            //r부모 * t0_sub_start : 이상태임

            //trs 순서가 안맞아 문제가 있는 계산
            //ori = rota * (t0_sub_start.origin);
            //last = rota * (t0_sub_start.last);

            //t부모를 제거한후 r부모를 적용한다. 그다음 t부모를 다시 적용한다 
            Vector3 o, l;
            o = (rot * (this.origin - pos_ori)) + pos_ori;
            l = (rot * (this.last - pos_ori)) + pos_ori;

            return new LineSegment3(o, l);
        }

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
            DebugWide.DrawCircle(last, 0.05f, color);
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

        //원점에서 평면까지의 최소거리에 있는 점  
        public Vector3 GetPos()
        {
            return _normal * -_offset;
        }

        // !! 정상동작하는지 확인필요
        // translate : 추가되는 이동량 , 기존 오프셋에 더해지는 값임 
        public void Transform(ref Quaternion rotate, ref Vector3 translate )
        {
            // transform to get normal
            _normal = rotate * _normal;
            
            // transform to get offset
            Vector3 newTrans = rotate * translate;
            _offset = -1f * Vector3.Dot(newTrans , _normal ) + _offset;

        }

        // manipulators
        //스튜어트 미분적분학 685p 참고 
        //평면의 법선백터 : n(a,b,c) , 벡터가 정규화 안되어 있다고 생각해야 함 
        //평면의 방정식 : ax + by + cz + d  = 0  , ax + by + cz  -(ax0 + by0 + cz0) = 0 ,  d = -(ax0 + by0 + cz0)
        //내적으로 표현 : n.dot(x,y,z) + d , d = -n.dot(x0,y0,z0)
        // d 의 의미 : d = -(ax0 + by0 + cz0) , 식의 변형이며 다른 의미는 없다 
        //_offset : 원점에서 평면까지의 최소거리 , n이 정규화 되어 있다면 : offset = d = -n.dot(x0,y0,z0)
        //          n이 정규화 되어 있지 않다면 : offset = -n.dot(x0,y0,z0) / |n|
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

                // -n.dot(x0,y0,z0) / |n|  , n이 정규화 안되어 있다 가정하므로 나누는 처리 필요
                _offset = d * recip; //원점에서 얼마나 떨어져있는지 나타내는 값을 계산 : 
                                     // !! d값 안의 법선벡터가 정규화 되어있지 않기에 recip를 곱해 정규화 해준다. d = 평면법선.dot(평면위의 임의점) 
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

            //DebugWide.LogRed(lensq + "  " + p0 + "  " + p1 + "  " + p2 );

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

        //교점구하는 방법 : close_PT = mLine.origin + mLineParameter * mLine.direction;
        static public int Intersect(out float mLineParameter, Line3 mLine, Plane mPlane)
        {
            mLineParameter = 0;

            float DdN = Vector3.Dot(mLine.direction, mPlane._normal);
            float signedDistance = mPlane.Test(mLine.origin);
            if (Math.Abs(DdN) > float.Epsilon)
            {
                // The line is not parallel to the plane, so they must intersect.
                mLineParameter = -signedDistance / DdN;

                return 1;
            }

            // The Line and plane are parallel.  Determine if they are numerically
            // close enough to be coincident.
            if (Math.Abs(signedDistance) <= float.Epsilon)
            {
                // The line is coincident with the plane, so choose t = 0 for the
                // parameter.
                mLineParameter = 0;

                return 2;
            }


            return 0;
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

        public bool Intersect(ref Capsule other )
        {
            //캡슐과 선분 충돌검사로 변형해 검사한다 , 소스캡슐의 반지름에 대상캡슐의 반지름을 더해서 대상캡슐의 선분값으로 비교가능하게 만든다. 
            float radiusSum = mRadius + other.mRadius;

            // if colliding
            float s, t;
            float distancesq = LineSegment3.DistanceSquared(mSegment, other.mSegment, out s, out t); 

            return (distancesq <= radiusSum* radiusSum );
        }

        public bool Intersect(ref LineSegment3 segment )
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
        public float Classify(ref Plane plane )
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

        public void AddDrawQ(Color color)
        {
            Vector3 dirRight = Vector3.Cross(Vector3.up, mSegment.direction);
            dirRight = VOp.Normalize(dirRight);

            DebugWide.AddDrawQ_Circle(mSegment.origin, mRadius, color);
            DebugWide.AddDrawQ_Circle(mSegment.last, mRadius, color);

            DebugWide.AddDrawQ_Line(mSegment.origin + dirRight * mRadius, mSegment.last + dirRight * mRadius, color);
            DebugWide.AddDrawQ_Line(mSegment.origin - dirRight * mRadius, mSegment.last - dirRight * mRadius, color);
        }
    }



    //======================================================

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

        public LineSegment3 GetLine_Last()
        {
            return new LineSegment3(tri0.V[1], tri0.V[2]);
        }

        public LineSegment3 GetLine_Origin()
        {
            return new LineSegment3(tri1.V[0], tri1.V[2]);
        }

        public LineSegment3 GetLine_Seg0()
        {
            return new LineSegment3(tri0.V[0], tri0.V[1]);
        }

        public LineSegment3 GetLine_Seg1()
        {
            return new LineSegment3(tri1.V[2], tri1.V[1]);
        }

        //tri0
        //v1  - v2
        // |  /     
        //v0  

        //tri1
        //      v1
        //    /  |   
        //v0  - v2
        public void Set(LineSegment3 seg0, LineSegment3 seg1)
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