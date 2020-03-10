using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Assertions;
using System;


namespace UtilGS9
{
    
    //======================================================
    //Essential Math 책 버젼 [tri vs line,segment] 교차검사
    public struct Tri3_Seg3_Test1
    {
    
        public struct Triangle
        {

            //-------------------------------------------------------------------------------
            // @ ::IsPointInTriangle()
            //-------------------------------------------------------------------------------
            // Returns true if point on triangle plane lies inside triangle (3D version)
            // Assumes triangle is not degenerate
            //-------------------------------------------------------------------------------
            static public bool IsPointInTriangle(Vector3 point, Vector3 P0, Vector3 P1, Vector3 P2)
            {
                Vector3 v0 = P1 - P0;
                Vector3 v1 = P2 - P1;
                Vector3 n = Vector3.Cross(v0, v1);

                //DebugWide.DrawLine(P0, P0 + n, Color.green);
                float F_Epsilon = 0.0000001f; //교점이 선분위에 있을때를 판별하기 위한 적당히 작은값 
                                              //교점이 선분위에 있으면 외적의 값이 0에 가까워 진다 (같은 방향의 벡터 외적 , sin0 = 0) 
                                              //외적값이 0에 가까운지로 교점이 선분위에 있는것을 검사한다 
                                              //교점이 선분위에 있으면 내적테스트와 상관없이 통과시킨다 (wTest.sqrMagnitude <= F_Epsilon)

                //외적의 180도 기점으로 방향이 바뀌는 점을 이용 , 벡터v0이 나누는 공간에서 어느공간에 점이 있는지 테스트하게 된다  
                Vector3 wTest = Vector3.Cross(v0, (point - P0));
                //DebugWide.DrawLine(P0, P0 + wTest, Color.yellow);
                //DebugWide.LogBlue("0 -> "+Vector3.Dot(wTest, n));
                if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
                {
                    return false;
                }

                wTest = Vector3.Cross(v1, (point - P1));
                //DebugWide.DrawLine(P1, P1 + wTest, Color.white);
                //DebugWide.LogBlue("1 -> " + Vector3.Dot(wTest, n) + "    wT:" + wTest + "  " + wTest.sqrMagnitude);
                if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
                {
                    return false;
                }

                Vector3 v2 = P0 - P2;
                wTest = Vector3.Cross(v2, (point - P2));
                //DebugWide.DrawLine(P2, P2 + wTest, Color.cyan);
                //DebugWide.LogBlue("2 -> " + Vector3.Dot(wTest, n));
                if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
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
            static public bool TriangleIntersect(out float t, Vector3 P0, Vector3 P1, Vector3 P2, Ray3 ray)
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
                if (u < 0.0f || u > 1.0f)
                    return false;

                Vector3 q = Vector3.Cross(s, e1);
                float v = f * Vector3.Dot(ray.direction, q);

                // ray falls outside triangle
                if (v < 0.0f || u + v > 1.0f)
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
            public float TriangleClassify(Vector3 P0, Vector3 P1, Vector3 P2, Plane plane)
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
                if (false == result)
                    result = Triangle.TriangleIntersect(out t, P0, P2, P3, line);

                return result;
            }
        }
    }

    /* ************************************************************************
    * Author: Kevin Kaiser  
    * Copyright (C) 1999,2000 Kevin Kaiser 
    * For use in 'Game Programming Gems'  
    * ************************************************************************ */
    public struct Tri3_Tri3_Test1
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
            float a1 = 0f, b1 = 0f, a2 = 0f, b2 = 0f, a3 = 0f, b3 = 0f, a4 = 0f, b4 = 0f;
            float center_x = 0f, center_y = 0f;
            float m1 = 0f, m2 = 0f, m3 = 0f;
            float bb1 = 0f, bb2 = 0f, bb3 = 0f;
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


    }//end tritri_1




    //* ************************************************************************
    // WildMagic5 engine
    // Geometric Tools, eberly
    //* ************************************************************************
    public struct Tri3_Tri3_Test2
    {
        public enum eIntersectionType
        {
            EMPTY,
            POINT,
            SEGMENT,
            RAY,
            LINE,
            POLYGON,
            PLANE,
            POLYHEDRON,
            OTHER
        }

        public struct Query2
        {
            public int mNumVertices;
            public Vector2[] mVertices;

            public Query2(int numVertices, Vector2[] vertices)
            {
                mNumVertices = numVertices;
                mVertices = vertices;
            }

            //행렬식값
            float Det2(float x0, float y0, float x1, float y1)
            {
                return x0 * y1 - x1 * y0;
            }

            bool Sort(ref int v0, ref int v1)
            {
                //int j0, j1;
                //bool positive;

                //if (v0 < v1)
                //{
                //    j0 = 0; j1 = 1; positive = true;
                //}
                //else
                //{
                //    j0 = 1; j1 = 0; positive = false;
                //}

                //int[] value = new int[2] { v0, v1 };
                //v0 = value[j0];
                //v1 = value[j1];
                //return positive;


                if (v0 > v1)
                {
                    int temp = v0;
                    v0 = v1;
                    v1 = temp;

                    return false;
                }

                return true;
            }

            public int ToLine(Vector2 test, int v0, int v1)
            {
                bool positive = Sort(ref v0, ref v1);

                Vector2 vec0 = mVertices[v0];
                Vector2 vec1 = mVertices[v1];

                float x0 = test.x - vec0.x;
                float y0 = test.y - vec0.y;
                float x1 = vec1.x - vec0.x;
                float y1 = vec1.y - vec0.y;

                float det = Det2(x0, y0, x1, y1);
                //if (!positive)
                if (false == positive)
                {
                    det = -det;
                }

                return (det > 0f ? +1 : (det < 0f ? -1 : 0));
            }

            public int ToTriangle(Vector2 test, int v0, int v1, int v2)
            {
                int sign0 = ToLine(test, v1, v2);
                if (sign0 > 0)
                {
                    return +1;
                }

                int sign1 = ToLine(test, v0, v2);
                if (sign1 < 0)
                {
                    return +1;
                }

                int sign2 = ToLine(test, v0, v1);
                if (sign2 > 0)
                {
                    return +1;
                }

                //return ((sign0 && sign1 && sign2) ? -1 : 0);
                return ((sign0 != 0 && sign1 != 0 && sign2 != 0) ? -1 : 0);
            }
        }//end query2

        public struct Plane3
        {
            public Vector3 Normal;
            public float Constant; //원점으로 부터 평면이 떨어져 있는 거리 

            public Plane3(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                Vector3 edge1 = p1 - p0;
                Vector3 edge2 = p2 - p0;
                Normal = Vector3.Cross(edge1, edge2);
                Normal = UtilGS9.VOp.Normalize(Normal);

                Constant = Vector3.Dot(Normal, p0);
            }

            //점이 평면에 직각인 거리 
            public float DistanceTo(Vector3 p)
            {
                return Vector3.Dot(Normal, p) - Constant;
            }

            public override string ToString()
            {
                return " N:" + Normal + "   C:" + Constant;
            }
        }

        public struct Triangle2
        {
            public Vector2[] V; //[3]

            //static public Triangle2 zero = new Triangle2(); //배열할당을 안하고 사용하는 문제 생김

            static public Triangle2 Zero()
            {
                Triangle2 triangle2 = new Triangle2(ConstV.v2_zero, ConstV.v2_zero, ConstV.v2_zero);

                return triangle2;
            }

            public Triangle2(Vector2 v0, Vector2 v1, Vector2 v2)
            {
                V = new Vector2[3];
                V[0] = v0; V[1] = v1; V[2] = v2;
            }

        }

        public struct Triangle3
        {
            public struct V012
            {
                public Vector3 V0, V1, V2;
                public Vector3 this[int index]
                {
                    get
                    {
                        switch (index)
                        {
                            case 0: return V0;
                            case 1: return V1;
                            case 2: return V2;
                        }

                        DebugWide.LogError("get 범위를 벗어나는 인덱스 " + index);
                        return ConstV.v3_zero;
                    }
                    set
                    {
                        switch (index)
                        {
                            case 0: V0 = value; return;
                            case 1: V1 = value; return;
                            case 2: V2 = value; return;
                        }
                        DebugWide.LogError("set 범위를 벗어나는 인덱스 " + index);
                    }
                }
            }
            //public V012 V;
            public Vector3[] V; //[3] //구조체의 배열 복사시 얇은복사를 하여 주소가 공유되는 문제 생김

            //static public Triangle3 zero = new Triangle3(); //배열할당을 안하고 사용하는 문제 생김

            static public Triangle3 Zero()
            {
                Triangle3 triangle3 = new Triangle3(ConstV.v3_zero, ConstV.v3_zero, ConstV.v3_zero);

                return triangle3;
            }

            public Triangle3(Vector3 v0, Vector3 v1, Vector3 v2)
            {
                V = new Vector3[3];
                V[0] = v0; V[1] = v1; V[2] = v2;

                //V = new V012();
                //V[0] = v0; V[1] = v1; V[2] = v2;
            }

            override public string ToString()
            {
                return V[0] + "   " + V[1] + "   " + V[2];
            }
        }

        public struct Segment2
        {
            public Vector2 P0, P1;

            // Center-direction-extent representation.
            public Vector2 Center;
            public Vector2 Direction;
            public float Extent; //크기 : 선분길이의 반 

            public Segment2(Vector2 p0, Vector2 p1)
            {
                P0 = p0;
                P1 = p1;

                //ComputeCenterDirectionExtent();
                Center = 0.5f * (P0 + P1);
                Direction = P1 - P0;
                Extent = 0.5f * Direction.magnitude;
                Direction = VOp.Normalize(Direction);
            }

            void ComputeCenterDirectionExtent()
            {
                Center = 0.5f * (P0 + P1);
                Direction = P1 - P0;
                Extent = 0.5f * Direction.magnitude;
                Direction = VOp.Normalize(Direction);
            }
        }

        public struct Intersector1
        {
            // The intervals to intersect.
            public float[] mU; //[2]
            public float[] mV; //[2]

            // Information about the intersection set.
            public float mFirstTime, mLastTime;
            public int mNumIntersections;
            public float[] mIntersections; //[2]

            public Intersector1(float u0, float u1, float v0, float v1)
            {
                //assertion(u0 <= u1 && v0 <= v1, "Malformed interval\n");
                mU = new float[2];
                mV = new float[2];

                mU[0] = u0;
                mU[1] = u1;
                mV[0] = v0;
                mV[1] = v1;
                mFirstTime = 0f;
                mLastTime = 0f;
                mNumIntersections = 0;

                mIntersections = new float[2];
            }

            public bool Find()
            {
                if (mU[1] < mV[0] || mU[0] > mV[1])
                {
                    mNumIntersections = 0;
                }
                else if (mU[1] > mV[0])
                {
                    if (mU[0] < mV[1])
                    {
                        mNumIntersections = 2;
                        mIntersections[0] = (mU[0] < mV[0] ? mV[0] : mU[0]);
                        mIntersections[1] = (mU[1] > mV[1] ? mV[1] : mU[1]);
                        if (mIntersections[0] == mIntersections[1])
                        {
                            mNumIntersections = 1;
                        }
                    }
                    else  // mU[0] == mV[1]
                    {
                        mNumIntersections = 1;
                        mIntersections[0] = mU[0];
                    }
                }
                else  // mU[1] == mV[0]
                {
                    mNumIntersections = 1;
                    mIntersections[0] = mU[1];
                }

                return mNumIntersections > 0;
            }

        }


        public struct IntrSegment2Triangle2
        {
            public eIntersectionType mIntersectionType;

            // The objects to intersect.
            public Segment2 mSegment;
            public Triangle2 mTriangle;

            // Information about the intersection set.
            public int mQuantity;
            public Vector2[] mPoint; //[2]

            public IntrSegment2Triangle2(Segment2 segment, Triangle2 tri)
            {
                mSegment = segment;
                mTriangle = tri;

                mQuantity = 0;
                mPoint = new Vector2[2]
                { ConstV.v2_zero, ConstV.v2_zero };

                mIntersectionType = eIntersectionType.EMPTY;
            }

            public bool Find()
            {
                float[] dist = new float[3];
                int[] sign = new int[3];
                int positive, negative, zero;
                TriangleLineRelations(mSegment.Center, mSegment.Direction, mTriangle,
                                      ref dist, ref sign, out positive, out negative, out zero);
                //DebugWide.LogBlue(zero); //chamto test
                if (positive == 3 || negative == 3)
                {
                    // No intersections.
                    mQuantity = 0;
                    mIntersectionType = eIntersectionType.EMPTY;
                }
                else
                {
                    float[] param = new float[2];
                    GetInterval(mSegment.Center, mSegment.Direction, mTriangle, dist, sign, ref param);

                    Intersector1 intr = new Intersector1(
                        param[0], param[1], -mSegment.Extent, +mSegment.Extent);

                    intr.Find();

                    mQuantity = intr.mNumIntersections;
                    if (mQuantity == 2)
                    {
                        // Segment intersection.
                        mIntersectionType = eIntersectionType.SEGMENT;
                        mPoint[0] = mSegment.Center +
                            intr.mIntersections[0] * mSegment.Direction;
                        mPoint[1] = mSegment.Center +
                            intr.mIntersections[1] * mSegment.Direction;
                    }
                    else if (mQuantity == 1)
                    {
                        // Point intersection.
                        mIntersectionType = eIntersectionType.POINT;
                        mPoint[0] = mSegment.Center +
                            intr.mIntersections[0] * mSegment.Direction;
                    }
                    else
                    {
                        // No intersections.
                        mIntersectionType = eIntersectionType.EMPTY;
                    }
                }

                return mIntersectionType != eIntersectionType.EMPTY;
            }//end func

            //dist[3] , sign[3]
            void TriangleLineRelations(Vector2 origin, Vector2 direction, Triangle2 triangle,
                                       ref float[] dist, ref int[] sign,
                                       out int positive, out int negative, out int zero)
            {
                positive = 0;
                negative = 0;
                zero = 0;
                for (int i = 0; i < 3; ++i)
                {
                    //!!! direction 가 0벡터인 경우의 예외처리가 없음. 즉 직선이 아닌 점으로 들어온 경우임 
                    //!!! 0벡터로 인해 GetInterval 함수에서 에러가 발생한다 
                    Vector2 diff = triangle.V[i] - origin;
                    dist[i] = VOp.PerpDot(diff, direction); //수직내적 : |a||b|sin@ , a와b벡터가 방향이 같으면 0 
                    //DebugWide.LogGreen(dist[i] + "   " + diff + "   " + direction); //chamto test
                    //직선의 방향으로 두개의 공간으로 나눈다
                    // 윗공간은 sin@ 양수공간 , 아랫공간은 sin@ 음수공간
                    // 직선이 삼각형의 변을 지나간다면 양수공간과 음수공간을 나누어서 지나가게 된다 
                    if (dist[i] > float.Epsilon)
                    {   //직선의 윗방향
                        sign[i] = 1;
                        ++positive;
                    }
                    else if (dist[i] < -float.Epsilon)
                    {   //직선의 아랫방향
                        sign[i] = -1;
                        ++negative;
                    }
                    else
                    {   //직선이 삼각형의 변과 겹쳐 지남
                        dist[i] = 0f;
                        sign[i] = 0;
                        ++zero;
                    }
                }
            }//end func

            //dist[3] , sign[3] , param[2]
            void GetInterval(Vector2 origin, Vector2 direction, Triangle2 triangle,
                float[] dist, int[] sign, ref float[] param)
            {

                //DebugWide.LogGreen("seg-   ori:"+origin + "   dir:" + direction); //chamto test

                // Project triangle onto line.
                float[] proj = new float[3];
                int i;
                for (i = 0; i < 3; ++i)
                {
                    Vector2 diff = triangle.V[i] - origin;
                    proj[i] = Vector2.Dot(direction, diff);
                }

                // Compute transverse intersections of triangle edges with line.
                float numer, denom;
                int i0, i1, i2;
                int quantity = 0;
                for (i0 = 2, i1 = 0; i1 < 3; i0 = i1++)
                {
                    //DebugWide.LogBlue(i0 + "   " + i1); //chamto test
                    //2  0
                    //0  1
                    //1  2
                    //두 부호의 곱이 음수라면 직선이 선분을 지난다 
                    if (sign[i0] * sign[i1] < 0)
                    {
                        //assertion(quantity< 2, "Too many intersections\n");
                        //분석인 안되는 부분 .. - chamto  
                        numer = dist[i0] * proj[i1] - dist[i1] * proj[i0];
                        denom = dist[i0] - dist[i1];
                        //DebugWide.LogBlue(" !!!   d0:"+ dist[i0] + "   p0:" + proj[i0] + "    d1:" + dist[i1] + "   p1:" + proj[i1]); //chamto test
                        //DebugWide.LogBlue("  n:"+numer + "   d:" + denom + "   n/d:" + numer / denom);
                        param[quantity++] = numer / denom;
                    }
                }
                // - 1단계 : 두가지 상황을 가정
                //  1.세개의 sign 값이 같은 부호를 가짐 : 직선이 삼각형을 지나지 않음 q = 0
                //  2.두개의 sign 이 같은 부호이고 다른 한개의 sign 은 다른 부호를 가짐 : 직선이 두개의 선분을 지남 q = 2


                //직선이 0개 또는 한개의 선분을 지나는 경우 
                // - 2단계 : 세가지 상황을 가정 
                //  1.두개의 sign 0값을 가지고, 하나의 부호있는 sign 값을 가짐 : 직선이 삼각형의 선분과 완전 겹침 q = 2
                //  2.한개의 sign 0값을 가지고, 두개의 서로다른 부호의 sign 값을 가짐 : 직선이 한개의 선분과 정점을 통과함 q = 2
                //  3.한개의 sign 0값을 가지고, 두개의 같은 부호의 sign 값을 가짐 : 직선이 한개의 정점만 지남 q = 1
                // Check for grazing contact.
                if (quantity < 2)
                {
                    //for (i0 = 1, i1 = 2, i2 = 0; i2< 3; i0 = i1, i1 = i2++)
                    for (i2 = 0; i2 < 3; i2++)
                    {
                        if (sign[i2] == 0)
                        {   //삼각형의 정점과 직선이 겹치는 경우  
                            if (2 == quantity)
                                DebugWide.LogWarning("  quantity:" + quantity + "   삼각형 형태가 아님!! ");
                            //DebugWide.LogBlue(quantity);; //chamto test
                            //assertion(quantity< 2, "Too many intersections\n");
                            param[quantity++] = proj[i2];
                        }
                    }
                }

                //구한값이 2개인 경우 정렬을 한다
                // Sort.
                //assertion(quantity >= 1, "Need at least one intersection\n");
                if (quantity == 2)
                {
                    if (param[0] > param[1])
                    {
                        float save = param[0];
                        param[0] = param[1];
                        param[1] = save;
                    }
                }
                else
                {   //같은 값으로 채움 
                    param[1] = param[0];
                }
            }//end func

        }

        public struct IntrTriangle2Triangle2
        {

            public Triangle2 mTriangle0;
            public Triangle2 mTriangle1;


            public int mQuantity;
            public Vector2[] mPoint; //[6]

            public IntrTriangle2Triangle2(Triangle2 t0, Triangle2 t1)
            {
                mTriangle0 = t0;
                mTriangle1 = t1;

                mQuantity = 0;
                mPoint = new Vector2[6];

                for (int i = 0; i < 6; i++)
                {
                    mPoint[i] = UtilGS9.ConstV.v3_zero;
                }
            }

            public bool Find()
            {
                // The potential intersection is initialized to triangle1.  The set of
                // vertices is refined based on clipping against each edge of triangle0.
                mQuantity = 3;
                for (int i = 0; i < 3; ++i)
                {
                    mPoint[i] = mTriangle1.V[i];
                }

                for (int i1 = 2, i0 = 0; i0 < 3; i1 = i0++)
                {
                    // Clip against edge <V0[i1],V0[i0]>.
                    Vector2 N = new Vector2(
                        mTriangle0.V[i1].y - mTriangle0.V[i0].y,
                        mTriangle0.V[i0].x - mTriangle0.V[i1].x);
                    float c = Vector2.Dot(N, mTriangle0.V[i1]);
                    ClipConvexPolygonAgainstLine(N, c, ref mQuantity, ref mPoint);
                    if (mQuantity == 0)
                    {
                        // Triangle completely clipped, no intersection occurs.
                        return false;
                    }
                }

                return true;
            }//end func

            void ClipConvexPolygonAgainstLine(Vector2 N, float c, ref int quantity, ref Vector2[] V) //[6]
            {
                // The input vertices are assumed to be in counterclockwise order.  The
                // ordering is an invariant of this function.

                // Test on which side of line the vertices are.
                int positive = 0, negative = 0, pIndex = -1;
                float[] test = new float[6];
                int i;
                for (i = 0; i < quantity; ++i)
                {
                    test[i] = Vector2.Dot(N, V[i]) - c;
                    if (test[i] > 0f)
                    {
                        positive++;
                        if (pIndex < 0)
                        {
                            pIndex = i;
                        }
                    }
                    else if (test[i] < 0f)
                    {
                        negative++;
                    }
                }

                if (positive > 0)
                {
                    if (negative > 0)
                    {
                        // Line transversely intersects polygon.
                        Vector2[] CV = new Vector2[6];
                        int cQuantity = 0, cur, prv;
                        float t;

                        if (pIndex > 0)
                        {
                            // First clip vertex on line.
                            cur = pIndex;
                            prv = cur - 1;
                            t = test[cur] / (test[cur] - test[prv]);
                            CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);

                            // Vertices on positive side of line.
                            while (cur < quantity && test[cur] > 0f)
                            {
                                CV[cQuantity++] = V[cur++];
                            }

                            // Last clip vertex on line.
                            if (cur < quantity)
                            {
                                prv = cur - 1;
                            }
                            else
                            {
                                cur = 0;
                                prv = quantity - 1;
                            }
                            t = test[cur] / (test[cur] - test[prv]);
                            CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);
                        }
                        else  // pIndex is 0
                        {
                            // Vertices on positive side of line.
                            cur = 0;
                            while (cur < quantity && test[cur] > 0f)
                            {
                                CV[cQuantity++] = V[cur++];
                            }

                            // Last clip vertex on line.
                            prv = cur - 1;
                            t = test[cur] / (test[cur] - test[prv]);
                            CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);

                            // Skip vertices on negative side.
                            while (cur < quantity && test[cur] <= 0f)
                            {
                                ++cur;
                            }

                            // First clip vertex on line.
                            if (cur < quantity)
                            {
                                prv = cur - 1;
                                t = test[cur] / (test[cur] - test[prv]);
                                CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);

                                // Vertices on positive side of line.
                                while (cur < quantity && test[cur] > 0f)
                                {
                                    CV[cQuantity++] = V[cur++];
                                }
                            }
                            else
                            {
                                // cur = 0
                                prv = quantity - 1;
                                t = test[0] / (test[0] - test[prv]);
                                CV[cQuantity++] = V[0] + t * (V[prv] - V[0]);
                            }
                        }

                        quantity = cQuantity;
                        //memcpy(V, CV, cQuantity*sizeof(Vector2<Real>));
                        CV.CopyTo(V, 0); //CV를 V에 복사 , V의 0인덱스 위치부터 복사한다 
                    }
                    // else polygon fully on positive side of line, nothing to do.
                }
                else
                {
                    // Polygon does not intersect positive side of line, clip all.
                    quantity = 0;
                }
            }//end func
        }//end IntrTri2_Tri2


        public struct IntrTriangle3Triangle3
        {

            public Triangle3 mTriangle0;
            public Triangle3 mTriangle1;

            // Information about the intersection set.
            public int mQuantity;
            public Vector3[] mPoint; //[6]

            public bool mReportCoplanarIntersections;  // default 'true'

            public eIntersectionType mIntersectionType;

            //--------------------------------------------------

            public IntrTriangle3Triangle3(Triangle3 t0, Triangle3 t1)
            {
                mTriangle0 = t0;
                mTriangle1 = t1;

                mQuantity = 0;
                mPoint = new Vector3[6];

                for (int i = 0; i < 6; i++)
                {
                    mPoint[i] = UtilGS9.ConstV.v3_zero;
                }

                mReportCoplanarIntersections = true;
                mIntersectionType = eIntersectionType.EMPTY;
            }

            // Input W must be a unit-length vector.  The output vectors {U,V} are
            // unit length and mutually perpendicular, and {U,V,W} is an orthonormal
            // basis.
            void GenerateComplementBasis(out Vector3 u, out Vector3 v, Vector3 w)
            {
                float invLength;

                if (Mathf.Abs(w.x) >= Mathf.Abs(w.y))
                {
                    // W.x or W.z is the largest magnitude component, swap them
                    invLength = 1f / (float)Math.Sqrt(w.x * w.x + w.y * w.y);
                    u.x = -w.z * invLength;
                    u.y = 0f;
                    u.z = +w.x * invLength;

                    v.x = w.y * u.z;
                    v.y = w.z * u.x - w.x * u.z;
                    v.z = -w.y * u.x;
                }
                else
                {
                    // W.y or W.z is the largest magnitude component, swap them
                    invLength = 1f / (float)Math.Sqrt(w.y * w.y + w.z * w.z);
                    u.x = 0f;
                    u.y = +w.z * invLength;
                    u.z = -w.y * invLength;

                    v.x = w.y * u.z - w.z * u.y;
                    v.y = -w.x * u.z;
                    v.z = w.x * u.y;
                }
            }



            //=======================================
            public void Draw(Color color)
            {
                switch (mIntersectionType)
                {
                    case eIntersectionType.POINT:
                        {
                            DebugWide.DrawCircle(mPoint[0], 0.05f, color);
                        }
                        break;
                    case eIntersectionType.SEGMENT:
                        {
                            DebugWide.DrawCircle(mPoint[0], 0.05f, color);
                            DebugWide.DrawCircle(mPoint[1], 0.05f, color);
                            DebugWide.DrawLine(mPoint[0], mPoint[1], color);

                        }
                        break;
                    case eIntersectionType.PLANE:
                        {
                            for (int i = 0; i < mQuantity; i++)
                            {
                                int i2 = i + 1;
                                i2 = (i2 == mQuantity ? 0 : i2);
                                DebugWide.DrawLine(mPoint[i], mPoint[i2], color);
                            }
                        }
                        break;
                }
            }

            //교차 검사를 두번하여 정확한 값을 찾는다 
            public bool Find_Twice()
            {
                bool result = Find(mTriangle0, mTriangle1);
                if (false == result)
                    return Find(mTriangle1, mTriangle0);

                return result;
            }

            //삼각형 형태가 아닌 것(선분,점)은 정상처리가 안된다
            //삼각형 형태만 처리 할 수 있다 
            public bool Find(Triangle3 tri0, Triangle3 tri1)
            {
                //----------
                //검사전 초기화 
                mQuantity = 0;
                mIntersectionType = eIntersectionType.EMPTY;
                //----------

                int i, iM, iP;

                // Get the plane of triangle0.
                Plane3 plane0 = new Plane3(tri0.V[0], tri0.V[1], tri0.V[2]);

                // Compute the signed distances of triangle1 vertices to plane0.  Use
                // an epsilon-thick plane test.
                int pos1, neg1, zero1;
                int[] sign1 = new int[3];
                float[] dist1 = new float[3];

                TrianglePlaneRelations(tri1, plane0, ref dist1, ref sign1, out pos1, out neg1, out zero1);

                //Plane3 plane1 = new Plane3(tri1.V[0], tri1.V[1], tri1.V[2]);
                //int pos2, neg2, zero2;
                //int[] sign2 = new int[3];
                //float[] dist2 = new float[3];
                //TrianglePlaneRelations(tri0, plane1, ref dist2, ref sign2, out pos2, out neg2, out zero2);
                //DebugWide.LogGreen("  po1:" + pos1 + "  ne:" + neg1 + "   ze:" + zero1 + "  s0:" + sign1[0] + "  s1:" + sign1[1] + "  s2:" + sign1[2] ); //chamto test
                //DebugWide.LogBlue("  po2:" + pos2 + "  ne:" + neg2 + "   ze:" + zero2 + "  s0:" + sign2[0] + "  s1:" + sign2[1] + "  s2:" + sign2[2] ); //chamto test

                // 삼각형의 정점이 모두 평면위쪽에 있거나 평면아래쪽에 있는 경우 
                if (pos1 == 3 || neg1 == 3)
                {
                    // Triangle1 is fully on one side of plane0.
                    return false;
                }

                // 삼각형의 정점이 모두 평면에 있는 경우 
                if (zero1 == 3)
                {
                    // Triangle1 is contained by plane0.
                    if (mReportCoplanarIntersections)
                    {
                        //DebugWide.LogBlue("0_0---  "); //chamto test

                        //[seg0 vs point1] 로 검사시 교차하지 않는데도  point1 위치를 교차점으로 결과가 나오는 문제있음 
                        //비삼각형 모양중 점은 아주 특수하므로 잘못된 결과를 무시한다 
                        if (Misc.IsZero(tri1.V[0] - tri1.V[1]) && Misc.IsZero(tri1.V[0] - tri1.V[2]))
                            return ContainsPoint2(tri0, tri1.V[0]);

                        return GetCoplanarIntersection(plane0, tri0, tri1);
                    }
                    return false;
                }


                //ㄱ. 삼각형0 의 평면을 구하고, 그 평면에 삼각형1의 교점이 있는지 검사하는 것만으로 충분하지 않음
                //반대로 ㄴ. 삼각형1 의 평면을 구하고, 그 평면에 삼각형0의 교점이 있는지도 검사해야 한다 
                //ㄱ 만 했을때 명백히 교점이 있는데도 없다고 나올 수 있다 
                //if (pos2 == 0 || neg2 == 0)
                //{
                //    if(zero2 == 1)
                //    {
                //        for (i = 0; i < 3; ++i)
                //        {
                //            if (sign2[i] == 0)
                //            {
                //                DebugWide.LogBlue("1_0---  "); //chamto test
                //                return ContainsPoint2(mTriangle1, mTriangle0.V[i]);
                //            }
                //        }
                //    }
                //}

                // 평면이 가르는 한쪽영역에 삼각형이 위치하고 그 정점중 하나나 둘이 평면에 있는 모양 
                // Check for grazing contact between triangle1 and plane0.
                if (pos1 == 0 || neg1 == 0)
                {
                    if (zero1 == 2)
                    {   //정점 두개가 평면에 있음
                        // An edge of triangle1 is in plane0.
                        for (i = 0; i < 3; ++i)
                        {
                            if (sign1[i] != 0)
                            {
                                //0
                                //1 2
                                //i(0) => mp(2,1)
                                //i(1) => mp(0,2)
                                //i(2) => mp(1,0)
                                iM = (i + 2) % 3;
                                iP = (i + 1) % 3;

                                //DebugWide.LogBlue("2---"); //chamto test

                                if (Misc.IsZero(tri1.V[iM] - tri1.V[iP]))
                                    return ContainsPoint2(tri0, tri1.V[iM]);

                                return IntersectsSegment(plane0, tri0, tri1.V[iM], tri1.V[iP]);
                            }
                        }
                    }
                    else if (zero1 == 1)// zero1 == 1
                    {   //정점 한개가 평면에 있음 
                        // A vertex of triangle1 is in plane0.
                        for (i = 0; i < 3; ++i)
                        {
                            if (sign1[i] == 0)
                            {
                                //DebugWide.LogGreen("1_1---  tr0:" + tri0 +  "  tr1:" + tri1 + "  pl0:" + plane0); //chamto test

                                return ContainsPoint2(tri0, tri1.V[i]);
                            }
                        }
                    }
                }

                // At this point, triangle1 tranversely intersects plane 0.  Compute the
                // line segment of intersection.  Then test for intersection between this
                // segment and triangle 0.
                float t;
                Vector3 intr0, intr1;
                // 삼각형의 정점이 평면상에 하나도 없는 경우 
                if (zero1 == 0)
                {
                    //삼각형이 평면에 수직으로 겹쳐진 모양을 가정
                    // 두가지의 모양이 있다고 가정
                    // 삼각형의 점정이 평면위쪽에 한개 나온 모양[+1] , 평면아래쪽에 한개 나온 모양[-1]
                    int iSign = (pos1 == 1 ? +1 : -1);
                    for (i = 0; i < 3; ++i)
                    {
                        if (sign1[i] == iSign)
                        {
                            //i(0) => mp(2,1)
                            //i(1) => mp(0,2)
                            //i(2) => mp(1,0)
                            iM = (i + 2) % 3;
                            iP = (i + 1) % 3;

                            //    di ---- vi
                            //       | -
                            //       |-
                            // ------------- plan
                            //      -|
                            //     - |
                            //viM ---- dim
                            //삼각형의 닮음을 이용하여 t를 구함 
                            t = dist1[i] / (dist1[i] - dist1[iM]); //d[im] 은 평면의 아래쪽이므로 음수다. 이는 d[i]와의 합을 의미 
                            intr0 = tri1.V[i] + t * (tri1.V[iM] - tri1.V[i]);
                            t = dist1[i] / (dist1[i] - dist1[iP]);
                            intr1 = tri1.V[i] + t * (tri1.V[iP] - tri1.V[i]);

                            //DebugWide.DrawCircle(intr0,0.05f,Color.green);
                            //DebugWide.LogBlue("3---  di:" + dist1[i] + "   dm:" + dist1[iM] +  "   dp:" + dist1[iP] + "   intr0:" + intr0 + "    intr1:" + intr1); //chamto test

                            //intr0 과 intr1 이 같다면 점과 교차하는 처리를 한다 
                            if (Misc.IsZero(intr0 - intr1))
                                return ContainsPoint2(tri0, intr0);

                            return IntersectsSegment(plane0, tri0, intr0, intr1);
                        }
                    }
                    return false;
                }

                // 삼각형의 정점이 평면상에 하나 있는 경우 
                if (zero1 == 1)
                {
                    for (i = 0; i < 3; ++i)
                    {
                        //삼각형의 정점중 하나가 평면에 접해있고 
                        // 평면의 위쪽에 하나, 평면의 아래쪽에 하나의 정점이 있는 모양 
                        if (sign1[i] == 0)
                        {   //평면과 접점이라면 

                            iM = (i + 2) % 3;
                            iP = (i + 1) % 3;
                            t = dist1[iM] / (dist1[iM] - dist1[iP]);
                            intr0 = tri1.V[iM] + t * (tri1.V[iP] - tri1.V[iM]);

                            //DebugWide.LogBlue("4---"); //chamto test

                            return IntersectsSegment(plane0, tri0, tri1.V[i], intr0);
                        }
                    }
                }

                //삼각형0 의 모양이 선분모양인 경우, 여기까지 올 수 있다 
                return false;

            }//end func

            //distance[3] , sign[3]
            void TrianglePlaneRelations(Triangle3 triangle, Plane3 plane,
                                        ref float[] distance, ref int[] sign, out int positive, out int negative, out int zero)
            {
                // Compute the signed distances of triangle vertices to the plane.  Use
                // an epsilon-thick plane test.
                positive = 0;
                negative = 0;
                zero = 0;
                for (int i = 0; i < 3; ++i)
                {
                    distance[i] = plane.DistanceTo(triangle.V[i]); //평면에서 점까지의 최소거리 (평면과 직각)
                    //DebugWide.LogBlue("  n:" + plane.Normal + "  tri1:" + triangle.V[i] + "   d:" + distance[i] + "  i:" + i); //chamto test

                    if (distance[i] > float.Epsilon)
                    {   //평면위쪽
                        sign[i] = 1;
                        positive++;
                    }
                    else if (distance[i] < -float.Epsilon)
                    {   //평면아래쪽
                        sign[i] = -1;
                        negative++;
                    }
                    else
                    {   //평면에 위치 
                        distance[i] = 0f;
                        sign[i] = 0;
                        zero++;
                    }
                }
            }//end func


            bool GetCoplanarIntersection(Plane3 plane, Triangle3 tri0, Triangle3 tri1)
            {
                //*
                // Project triangles onto coordinate plane most aligned with plane
                // normal.
                int maxNormal = 0;
                float fmax = Mathf.Abs(plane.Normal.x);
                float absMax = Mathf.Abs(plane.Normal.y);
                if (absMax > fmax)
                {
                    maxNormal = 1;
                    fmax = absMax;
                }
                absMax = Mathf.Abs(plane.Normal.z);
                if (absMax > fmax)
                {
                    maxNormal = 2;
                }

                Triangle2 projTri0 = Triangle2.Zero(), projTri1 = Triangle2.Zero();

                int i;

                if (maxNormal == 0)
                {
                    // Project onto yz-plane.
                    for (i = 0; i < 3; ++i)
                    {
                        projTri0.V[i].x = tri0.V[i].y;
                        projTri0.V[i].y = tri0.V[i].z;
                        projTri1.V[i].x = tri1.V[i].y;
                        projTri1.V[i].y = tri1.V[i].z;
                    }
                }
                else if (maxNormal == 1)
                {
                    // Project onto xz-plane.
                    for (i = 0; i < 3; ++i)
                    {
                        projTri0.V[i].x = tri0.V[i].x;
                        projTri0.V[i].y = tri0.V[i].z;
                        projTri1.V[i].x = tri1.V[i].x;
                        projTri1.V[i].y = tri1.V[i].z;
                    }
                }
                else
                {
                    // Project onto xy-plane.
                    for (i = 0; i < 3; ++i)
                    {
                        projTri0.V[i].x = tri0.V[i].x;
                        projTri0.V[i].y = tri0.V[i].y;
                        projTri1.V[i].x = tri1.V[i].x;
                        projTri1.V[i].y = tri1.V[i].y;
                    }
                }

                // 2D triangle intersection routines require counterclockwise ordering.
                Vector2 save;
                Vector2 edge0 = projTri0.V[1] - projTri0.V[0];
                Vector2 edge1 = projTri0.V[2] - projTri0.V[0];
                //if (edge0.DotPerp(edge1) < (Real)0)
                if (VOp.PerpDot(edge0, edge1) < 0f)
                {
                    // Triangle is clockwise, reorder it.
                    save = projTri0.V[1];
                    projTri0.V[1] = projTri0.V[2];
                    projTri0.V[2] = save;
                }

                edge0 = projTri1.V[1] - projTri1.V[0];
                edge1 = projTri1.V[2] - projTri1.V[0];
                //if (edge0.DotPerp(edge1) < (Real)0)
                if (VOp.PerpDot(edge0, edge1) < 0f)
                {
                    // Triangle is clockwise, reorder it.
                    save = projTri1.V[1];
                    projTri1.V[1] = projTri1.V[2];
                    projTri1.V[2] = save;
                }

                IntrTriangle2Triangle2 intr = new IntrTriangle2Triangle2(projTri0, projTri1);
                if (!intr.Find())
                {
                    return false;
                }

                // Map 2D intersections back to the 3D triangle space.
                mQuantity = intr.mQuantity;
                if (maxNormal == 0)
                {
                    float invNX = 1f / plane.Normal.x;
                    for (i = 0; i < mQuantity; i++)
                    {
                        mPoint[i].y = intr.mPoint[i].x;
                        mPoint[i].z = intr.mPoint[i].y;
                        mPoint[i].x = invNX * (plane.Constant -
                                        plane.Normal.y * mPoint[i].y -
                                               plane.Normal.z * mPoint[i].z);
                    }
                }
                else if (maxNormal == 1)
                {
                    float invNY = 1f / plane.Normal.y;
                    for (i = 0; i < mQuantity; i++)
                    {
                        mPoint[i].x = intr.mPoint[i].x;
                        mPoint[i].z = intr.mPoint[i].y;
                        mPoint[i].y = invNY * (plane.Constant -
                                        plane.Normal.x * mPoint[i].x -
                                               plane.Normal.z * mPoint[i].z);
                    }
                }
                else
                {
                    float invNZ = 1f / plane.Normal.z;
                    for (i = 0; i < mQuantity; i++)
                    {
                        mPoint[i].x = intr.mPoint[i].x;
                        mPoint[i].y = intr.mPoint[i].y;
                        mPoint[i].z = invNZ * (plane.Constant -
                                        plane.Normal.x * mPoint[i].x -
                                               plane.Normal.y * mPoint[i].y);
                    }
                }

                mIntersectionType = eIntersectionType.PLANE;
                //*/
                return true;
            }//end func


            bool IntersectsSegment(Plane3 plane, Triangle3 triangle, Vector3 end0, Vector3 end1)
            {
                // Compute the 2D representations of the triangle vertices and the
                // segment endpoints relative to the plane of the triangle.  Then
                // compute the intersection in the 2D space.

                // Project the triangle and segment onto the coordinate plane most
                // aligned with the plane normal.
                int maxNormal = 0; //노멀방향 x축
                float fmax = Mathf.Abs(plane.Normal.x);
                float absMax = Mathf.Abs(plane.Normal.y);
                if (absMax > fmax)
                {
                    maxNormal = 1; //노멀방향 y축 
                    fmax = absMax;
                }
                absMax = Mathf.Abs(plane.Normal.z);
                if (absMax > fmax)
                {
                    maxNormal = 2; //노멀방향 z축
                }

                Triangle2 projTri = Triangle2.Zero();
                Vector2 projEnd0 = ConstV.v2_zero, projEnd1 = ConstV.v2_zero;
                int i;

                if (maxNormal == 0)
                {
                    // Project onto yz-plane.
                    for (i = 0; i < 3; ++i)
                    {
                        projTri.V[i].x = triangle.V[i].y;
                        projTri.V[i].y = triangle.V[i].z;
                        projEnd0.x = end0.y;
                        projEnd0.y = end0.z;
                        projEnd1.x = end1.y;
                        projEnd1.y = end1.z;
                    }
                }
                else if (maxNormal == 1)
                {
                    // Project onto xz-plane.
                    for (i = 0; i < 3; ++i)
                    {
                        projTri.V[i].x = triangle.V[i].x;
                        projTri.V[i].y = triangle.V[i].z;
                        projEnd0.x = end0.x;
                        projEnd0.y = end0.z;
                        projEnd1.x = end1.x;
                        projEnd1.y = end1.z;
                    }
                }
                else
                {
                    // Project onto xy-plane.
                    for (i = 0; i < 3; ++i)
                    {
                        projTri.V[i].x = triangle.V[i].x;
                        projTri.V[i].y = triangle.V[i].y;
                        projEnd0.x = end0.x;
                        projEnd0.y = end0.y;
                        projEnd1.x = end1.x;
                        projEnd1.y = end1.y;
                    }
                }
                //DebugWide.LogBlue(end0 + "  " + end1); //chamto test

                Segment2 projSeg = new Segment2(projEnd0, projEnd1);
                //DebugWide.LogBlue(projEnd0 + "  " + projEnd1); //chamto test
                IntrSegment2Triangle2 calc = new IntrSegment2Triangle2(projSeg, projTri);
                if (!calc.Find())
                {
                    return false;
                }

                Vector2[] intr = new Vector2[2];
                if (calc.mIntersectionType == eIntersectionType.SEGMENT)
                {
                    mIntersectionType = eIntersectionType.SEGMENT;
                    mQuantity = 2;
                    intr[0] = calc.mPoint[0];
                    intr[1] = calc.mPoint[1];
                }
                else
                {
                    //assertion(calc.GetIntersectionType() == IT_POINT, "Intersection must be a point\n");
                    mIntersectionType = eIntersectionType.POINT;
                    mQuantity = 1;
                    intr[0] = calc.mPoint[0];
                }

                // Unproject the segment of intersection.
                if (maxNormal == 0)
                {
                    float invNX = 1f / plane.Normal.x;
                    for (i = 0; i < mQuantity; ++i)
                    {
                        mPoint[i].y = intr[i].x;
                        mPoint[i].z = intr[i].y;
                        mPoint[i].x = invNX * (plane.Constant -
                            plane.Normal.y * mPoint[i].y -
                            plane.Normal.z * mPoint[i].z);
                    }
                }
                else if (maxNormal == 1)
                {
                    float invNY = 1f / plane.Normal.y;
                    for (i = 0; i < mQuantity; ++i)
                    {
                        mPoint[i].x = intr[i].x;
                        mPoint[i].z = intr[i].y;
                        mPoint[i].y = invNY * (plane.Constant -
                            plane.Normal.x * mPoint[i].x -
                            plane.Normal.z * mPoint[i].z);
                    }
                }
                else
                {
                    float invNZ = 1f / plane.Normal.z;
                    for (i = 0; i < mQuantity; ++i)
                    {
                        mPoint[i].x = intr[i].x;
                        mPoint[i].y = intr[i].y;
                        mPoint[i].z = invNZ * (plane.Constant -
                            plane.Normal.x * mPoint[i].x -
                            plane.Normal.y * mPoint[i].y);
                    }
                }

                return true;
            }//end func

            public bool IsPointInTriangle(Vector3 point, Vector3 P0, Vector3 P1, Vector3 P2)
            {
                Vector3 v0 = P1 - P0;
                Vector3 v1 = P2 - P1;
                Vector3 n = Vector3.Cross(v0, v1);

                //DebugWide.DrawLine(P0, P0 + n, Color.green);
                float F_Epsilon = 0.0000001f; //교점이 선분위에 있을때를 판별하기 위한 적당히 작은값 
                                              //교점이 선분위에 있으면 외적의 값이 0에 가까워 진다 (같은 방향의 벡터 외적 , sin0 = 0) 
                                              //외적값이 0에 가까운지로 교점이 선분위에 있는것을 검사한다 
                                              //교점이 선분위에 있으면 내적테스트와 상관없이 통과시킨다 (wTest.sqrMagnitude <= F_Epsilon)

                //외적의 180도 기점으로 방향이 바뀌는 점을 이용 , 벡터v0이 나누는 공간에서 어느공간에 점이 있는지 테스트하게 된다  
                Vector3 wTest = Vector3.Cross(v0, (point - P0));
                //DebugWide.DrawLine(P0, P0 + wTest, Color.yellow);
                //DebugWide.LogBlue("0 -> "+Vector3.Dot(wTest, n));
                if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
                {
                    return false;
                }

                wTest = Vector3.Cross(v1, (point - P1));
                //DebugWide.DrawLine(P1, P1 + wTest, Color.white);
                //DebugWide.LogBlue("1 -> " + Vector3.Dot(wTest, n) + "    wT:" + wTest + "  " + wTest.sqrMagnitude);
                if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
                {
                    return false;
                }

                Vector3 v2 = P0 - P2;
                wTest = Vector3.Cross(v2, (point - P2));
                //DebugWide.DrawLine(P2, P2 + wTest, Color.cyan);
                //DebugWide.LogBlue("2 -> " + Vector3.Dot(wTest, n));
                if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
                {
                    return false;
                }

                return true;
            }

            bool ContainsPoint2(Triangle3 triangle, Vector3 point)
            {
                if (IsPointInTriangle(point, triangle.V[0], triangle.V[1], triangle.V[2]))
                {
                    // Report the point of intersection to the caller.
                    mIntersectionType = eIntersectionType.POINT;
                    mQuantity = 1;
                    mPoint[0] = point;
                    return true;
                }

                return false;
            }

            //함수에 버그 있음. tri0 으로 만든 평면영역에 tri1 의 꼭지점 하나만 속해있을때 
            //  명백히 꼭지점이 tri0에 속해 있지 않은데 충돌된것으로 처리됨. 
            //   아래 함수의 알고리즘으로 만들어지는 2차원 삼각형을 출력 하였을때 삼각형의 모양이 직선이었음
            //   삼각형 모양으로 나오지 않는 것으로 보아 , 알고리즘에 문제가 있는 것으로 추정
            //   다른 알고리즘으로 (IsPointInTriangle) 처리하면 정상적으로 처리된다 
            bool ContainsPoint(Triangle3 triangle, Plane3 plane, Vector3 point)
            {
                // Generate a coordinate system for the plane.  The incoming triangle has
                // vertices <V0,V1,V2>.  The incoming plane has unit-length normal N.
                // The incoming point is P.  V0 is chosen as the origin for the plane. The
                // coordinate axis directions are two unit-length vectors, U0 and U1,
                // constructed so that {U0,U1,N} is an orthonormal set.  Any point Q
                // in the plane may be written as Q = V0 + x0*U0 + x1*U1.  The coordinates
                // are computed as x0 = Dot(U0,Q-V0) and x1 = Dot(U1,Q-V0).
                Vector3 U0, U1;
                GenerateComplementBasis(out U0, out U1, plane.Normal);

                // Compute the planar coordinates for the points P, V1, and V2.  To
                // simplify matters, the origin is subtracted from the points, in which
                // case the planar coordinates are for P-V0, V1-V0, and V2-V0.
                Vector3 PmV0 = point - triangle.V[0];
                Vector3 V1mV0 = triangle.V[1] - triangle.V[0];
                Vector3 V2mV0 = triangle.V[2] - triangle.V[0];

                // The planar representation of P-V0.
                Vector2 ProjP = new Vector2(Vector2.Dot(U0, PmV0), Vector3.Dot(U1, PmV0));

                // The planar representation of the triangle <V0-V0,V1-V0,V2-V0>.
                Vector2[] ProjV = new Vector2[3]
                    {
                        ConstV.v2_zero,
                        new Vector2(Vector2.Dot(U0,V1mV0), Vector2.Dot(U1,V1mV0)),
                        new Vector2(Vector2.Dot(U0,V2mV0), Vector2.Dot(U1,V2mV0))
                    };

                //----------
                //chamto test
                //DebugWide.LogBlue("  U0,1: " + U0 + "  " + U1 + "   N:" + plane.Normal);
                //DebugWide.LogBlue("  projv: "+ProjV[1] + "  " + ProjV[2]);
                //DebugWide.DrawCircle(ProjP, 0.05f, Color.black);
                //DebugWide.DrawLine(ProjV[0], ProjV[1], Color.black);
                //DebugWide.DrawLine(ProjV[1], ProjV[2], Color.black);
                //DebugWide.DrawLine(ProjV[2], ProjV[0], Color.black);
                //----------

                // Test whether P-V0 is in the triangle <0,V1-V0,V2-V0>.
                if (new Query2(3, ProjV).ToTriangle(ProjP, 0, 1, 2) <= 0)
                //if(Triangle.IsPointInTriangle(point,triangle.V[0],triangle.V[1],triangle.V[2]))
                {
                    // Report the point of intersection to the caller.
                    mIntersectionType = eIntersectionType.POINT;
                    mQuantity = 1;
                    mPoint[0] = point;
                    return true;
                }

                return false;
            }//end func

        }//end IntrTri3_Tri3

    }//end Tri3_Tri3_Test2


}