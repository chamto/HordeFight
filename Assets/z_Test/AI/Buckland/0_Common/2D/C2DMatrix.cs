using System;
using System.Collections.Generic;
using UnityEngine;


namespace Buckland
{
    public struct C2DMatrix
    {
        public float _11, _12, _13;
        public float _21, _22, _23;
        public float _31, _32, _33;

        //multiplies m_Matrix with mIn
        public static C2DMatrix operator *(C2DMatrix a, C2DMatrix b)
        {
            C2DMatrix matrix = new C2DMatrix();

            //first row
            matrix._11 = (a._11 * b._11) + (a._12 * b._21) + (a._13 * b._31);
            matrix._12 = (a._11 * b._12) + (a._12 * b._22) + (a._13 * b._32);
            matrix._13 = (a._11 * b._13) + (a._12 * b._23) + (a._13 * b._33);

            //second
            matrix._21 = (a._21 * b._11) + (a._22 * b._21) + (a._23 * b._31);
            matrix._22 = (a._21 * b._12) + (a._22 * b._22) + (a._23 * b._32);
            matrix._23 = (a._21 * b._13) + (a._22 * b._23) + (a._23 * b._33);

            //third
            matrix._31 = (a._31 * b._11) + (a._32 * b._21) + (a._33 * b._31);
            matrix._32 = (a._31 * b._12) + (a._32 * b._22) + (a._33 * b._32);
            matrix._33 = (a._31 * b._13) + (a._32 * b._23) + (a._33 * b._33);

            return matrix;
        }

        public override string ToString()
        {
            return _11 + " " + _12 + " " + _13 + "\n" +
                _21 + " " + _22 + " " + _23 + "\n" +
                _31 + " " + _32 + " " + _33;
        }

        public void InitZero()
        {
            _11 = 0.0f; _12 = 0.0f; _13 = 0.0f;
            _21 = 0.0f; _22 = 0.0f; _23 = 0.0f;
            _31 = 0.0f; _32 = 0.0f; _33 = 0.0f;
        }

        //create an identity matrix
        public void Identity()
        {
            _11 = 1; _12 = 0; _13 = 0;

            _21 = 0; _22 = 1; _23 = 0;

            _31 = 0; _32 = 0; _33 = 1;
        }

        static public C2DMatrix identity
        {
            get
            {
                C2DMatrix m = new C2DMatrix();
                m.Identity();
                return m;
            }
        }

        //create a transformation matrix
        public void Translate(float x, float y)
        {
            C2DMatrix mat = new C2DMatrix();

            mat._11 = 1; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = 1; mat._23 = 0;

            mat._31 = x; mat._32 = y; mat._33 = 1;

            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        //create a scale matrix
        public void Scale(float xScale, float yScale)
        {
            C2DMatrix mat = new C2DMatrix();

            mat._11 = xScale; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = yScale; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        //create a rotation matrix
        public void RotateY(float rot)
        {
            C2DMatrix mat;

            float Sin = (float)Math.Sin(rot);
            float Cos = (float)Math.Cos(rot);

            mat._11 = Cos; mat._12 = Sin; mat._13 = 0;

            mat._21 = -Sin; mat._22 = Cos; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        // | cos@  sin@|   | head |   | @      |
        // |-sin@  cos@| = | side | = | @ + 90 |
        // side = perp(head)
        // 회전행렬을 가로축으로 배치된 벡터로 보았을때 첫번째행 백터와 두번째행 벡터는 90도가 차이가 난다 
        // 이 특징을 이용하여 정규화된 특정 방향백터 하나의 값으로 회전행렬을 구할 수 있다  
        //create a rotation matrix from a fwd and side 2D vector
        public void RotateY(Vector2 fwd, Vector2 side)
        {
            C2DMatrix mat = new C2DMatrix();

            mat._11 = fwd.x; mat._12 = fwd.y; mat._13 = 0;

            mat._21 = side.x; mat._22 = side.y; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //DebugWide.LogBlue(this + " \n " + mat);
            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        //applys a transformation matrix to a std::vector of points
        public static void Transform(ref C2DMatrix matrix, ref List<Vector2> vPoints)
        {

            for (int i = 0; i < vPoints.Count; ++i)
            {
                Vector2 temp = new Vector2();
                temp.x = (matrix._11 * vPoints[i].x) + (matrix._21 * vPoints[i].y) + (matrix._31);

                temp.y = (matrix._12 * vPoints[i].x) + (matrix._22 * vPoints[i].y) + (matrix._32);

                vPoints[i] = temp;
            }
        }

        //applys a transformation matrix to a point
        public static void Transform(ref C2DMatrix matrix, ref Vector2 vPoint)
        {
            Vector2 temp = new Vector2();
            temp.x = (matrix._11 * vPoint.x) + (matrix._21 * vPoint.y) + (matrix._31);

            temp.y = (matrix._12 * vPoint.x) + (matrix._22 * vPoint.y) + (matrix._32);

            vPoint = temp;
        }
    }

}//end namespace

