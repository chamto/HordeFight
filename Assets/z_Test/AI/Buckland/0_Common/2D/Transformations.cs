using System;
using System.Collections.Generic;
using UnityEngine;

namespace Buckland
{
    public class Transformations 
    {
        //--------------------------- WorldTransform -----------------------------
        //
        //  given a std::vector of 2D vectors, a position, orientation and scale,
        //  this function transforms the 2D vectors into the object's world space
        //------------------------------------------------------------------------
        public static List<Vector2> WorldTransform(List<Vector2> points,
                                                 Vector2 pos,
                                                 Vector2 forward,
                                                 Vector2 side,
                                                 Vector2 scale)
        {
            //copy the original vertices into the buffer about to be transformed
            List<Vector2> TranVector2Ds = new List<Vector2>(points);
            //DebugWide.LogBlue(pos + "  " + forward + "  " + side + "   " + scale);
            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //scale
            if ((scale.x != 1.0f) || (scale.y != 1.0f))
            {
                matTransform.Scale(scale.x, scale.y);
            }

            //rotate
            matTransform.RotateY(forward, side);

            //and translate
            matTransform.Translate(pos.x, pos.y);

            //now transform the object's vertices
            C2DMatrix.Transform(ref matTransform, ref TranVector2Ds);

            return TranVector2Ds;
        }

        //--------------------------- WorldTransform -----------------------------
        //
        //  given a std::vector of 2D vectors, a position and  orientation
        //  this function transforms the 2D vectors into the object's world space
        //------------------------------------------------------------------------
        public static List<Vector2> WorldTransform(List<Vector2> points,
                                  Vector2 pos,
                                  Vector2 forward,
                                  Vector2 side)
        {
            //copy the original vertices into the buffer about to be transformed
            List<Vector2> TranVector2Ds = new List<Vector2>(points);

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;
            //rotate
            matTransform.RotateY(forward, side);

            //and translate
            matTransform.Translate(pos.x, pos.y);

            //now transform the object's vertices
            C2DMatrix.Transform(ref matTransform, ref TranVector2Ds);

            return TranVector2Ds;
        }

        //--------------------- PointToWorldSpace --------------------------------
        //
        //  Transforms a point from the agent's local space into world space
        //------------------------------------------------------------------------
        public static Vector2 Point2DToWorldSpace(Vector2 point,
                                     Vector2 AgentHeading,
                                     Vector2 AgentSide,
                                     Vector2 AgentPosition)
        {
            //make a copy of the point
            Vector2 TransPoint = point;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //rotate
            matTransform.RotateY(AgentHeading, AgentSide);

            //and translate
            matTransform.Translate(AgentPosition.x, AgentPosition.y);

            //now transform the vertices
            C2DMatrix.Transform(ref matTransform, ref point);

            return point;
        }

        //실험노트 20210115 에 분선한 글이 있다. 참고하기 
        public static Vector2 Point2DToLocalSpace(Vector2 point,
                             Vector2 AgentHeading,
                             Vector2 AgentSide,
                             Vector2 AgentPosition)
        {

            //make a copy of the point
            Vector2 TransPoint = point;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //로컬좌표기준 이동량을 구한다 
            float Tx = -Vector2.Dot(AgentPosition, AgentHeading);
            float Ty = -Vector2.Dot(AgentPosition, AgentSide);

            //기저축 정보를 이용하여 바로 역행렬을 만든다 
            matTransform._11 = AgentHeading.x; matTransform._12 = AgentSide.x;
            matTransform._21 = AgentHeading.y; matTransform._22 = AgentSide.y;

            //이동량 적용
            matTransform._31 = Tx; matTransform._32 = Ty;

            //내부적으로 대상점에 회전행렬을 곱한 후 이동량이 더해진다 p' = (m*p)+t
            C2DMatrix.Transform(ref matTransform, ref TransPoint);

            return TransPoint;
        }

        public static Vector2 Vector2DToLocalSpace(Vector2 vec,
                              Vector2 AgentHeading,
                              Vector2 AgentSide)
        {

            //make a copy of the point
            Vector2 TransPoint = vec;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //create the transformation matrix
            matTransform._11 = AgentHeading.x; matTransform._12 = AgentSide.x;
            matTransform._21 = AgentHeading.y; matTransform._22 = AgentSide.y;

            //now transform the vertices
            C2DMatrix.Transform(ref matTransform, ref TransPoint);

            return TransPoint;
        }

        //--------------------- VectorToWorldSpace --------------------------------
        //
        //  Transforms a vector from the agent's local space into world space
        //------------------------------------------------------------------------
        public static Vector2 Vector2DToWorldSpace(Vector2 vec,
                                      Vector2 AgentHeading,
                                      Vector2 AgentSide)
        {
            //make a copy of the point
            Vector2 TransVec = vec;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //rotate
            matTransform.RotateY(AgentHeading, AgentSide);

            //now transform the vertices
            C2DMatrix.Transform(ref matTransform, ref TransVec);

            return TransVec;
        }

        //-------------------------- Vec2DRotateAroundOrigin --------------------------
        //
        //  rotates a vector ang rads around the origin
        //-----------------------------------------------------------------------------
        public static Vector2 Vec2DRotateAroundOrigin(Vector2 v, float ang)
        {
            //create a transformation matrix
            C2DMatrix mat = C2DMatrix.identity;

            //rotate
            mat.RotateY(ang);

            //now transform the object's vertices
            C2DMatrix.Transform(ref mat, ref v);

            return v;
        }

        //-------------------------- Vec2DRotateAroundOrigin --------------------------
        //
        //  rotates a vector ang rads around the origin
        //-----------------------------------------------------------------------------
        public static void Vec2DRotateAroundOrigin(ref Vector2 v, float ang)
        {
            //create a transformation matrix
            C2DMatrix mat = C2DMatrix.identity;

            //rotate
            mat.RotateY(ang);

            //now transform the object's vertices
            C2DMatrix.Transform(ref mat, ref v);
        }


        //================================================== 3d 확장함수 

        static public void Draw_WorldTransform(List<Vector3> points,
                                  Vector3 pos,
                                  Vector3 forward,
                                  Vector3 side,
                                  Vector3 scale,
                                  Color color)
        {
            Vector3 up = Vector3.Cross(forward, side);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, side); //열값 삽입
            m.SetColumn(1, up);
            m.SetColumn(2, forward);

            //Quaternion rotQ = Quaternion.FromToRotation(ConstV.v3_forward, forward);


            Vector3 tr, cur, prev = Vector3.zero;

            Vector3 v = points[points.Count - 1];
            tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
            prev = m.MultiplyPoint(tr) + pos;
            //prev = rotQ * v + pos;
            for (int i = 0; i < points.Count; i++)
            {
                v = points[i];
                tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
                cur = m.MultiplyPoint(tr) + pos;
                //cur = rotQ * points[i] + pos;

                DebugWide.DrawLine(prev, cur, color);

                prev = cur;
            }

        }

        static public List<Vector3> WorldTransform(List<Vector3> points,
                                  Vector3 pos,
                                  Vector3 forward,
                                  Vector3 side,
                                  Vector3 scale)
        {
            Vector3 up = Vector3.Cross(forward, side);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, side); //열값 삽입
            m.SetColumn(1, up);
            m.SetColumn(2, forward);

            //Quaternion rotQ = Quaternion.FromToRotation(ConstV.v3_forward, forward);

            List<Vector3> list = new List<Vector3>();

            Vector3 tr;
            foreach (Vector3 v in points)
            {
                tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
                //list.Add((rotQ * tr) + pos);
                list.Add((m.MultiplyPoint(tr)) + pos);
            }
            return list;

        }

        static public Vector3 PointToWorldSpace(Vector3 point,
                                    Vector3 AgentHeading,
                                    Vector3 AgentSide,
                                    Vector3 AgentPosition)
        {
            Vector3 AgentUp = Vector3.Cross(AgentHeading, AgentSide);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, AgentSide); //열값 삽입
            m.SetColumn(1, AgentUp);
            m.SetColumn(2, AgentHeading);

            //Quaternion rotQ = Quaternion.FromToRotation(ConstV.v3_forward, AgentHeading);

            return m.MultiplyPoint(point) + AgentPosition;
        }

        static public Vector3 Vec3RotateYAroundOrigin(Vector3 v, float radian)
        {
            Quaternion rotQ = Quaternion.AngleAxis(radian * Mathf.Rad2Deg, Vector3.up);
            return rotQ * v;
        }

        static public void Vec3RotateYAroundOrigin(ref Vector3 v, float radian)
        {
            Quaternion rotQ = Quaternion.AngleAxis(radian * Mathf.Rad2Deg, Vector3.up);
            v = rotQ * v;
        }
    }

}//end namespace

