using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test_LineCollision : MonoBehaviour 
{
    private bool _init = false;
    private Transform _line0_start = null;
    private Transform _line0_end = null;
    private Transform _ts0 = null;

    private Transform _line1_start = null;
    private Transform _line1_end = null;
    private Transform _ts1 = null;
    public float _line1_width = 0.001f;
    private Geo.Model _model_1 = new Geo.Model();

    private Transform _line2_start = null;
    private Transform _line2_end = null;
    private Transform _ts2 = null;

    //===========================================

    //public struct TrTri
    //{
    //    public Transform v0, v1, v2;
    //    public TriTri_Test1.triangle tri;

    //    public void Update()
    //    {
    //        tri.a = v0.position;
    //        tri.b = v1.position;
    //        tri.c = v2.position;
    //    }
    //}

    public struct Tri3
    {
        public Transform v0, v1, v2;
        public TriTri_Test2.Triangle3 tri0;

        public void Update()
        {
            tri0.V[0] = v0.position;
            tri0.V[1] = v1.position;
            tri0.V[2] = v2.position;
        }

        public void Draw(Color color)
        {
            DebugWide.DrawLine(tri0.V[0], tri0.V[1], color);
            DebugWide.DrawLine(tri0.V[0], tri0.V[2], color);
            DebugWide.DrawLine(tri0.V[1], tri0.V[2], color);
        }

    }

    public struct Tetra3
    {
        //v0    v1
        //
        //v2    v3
        //v0 - v1 - v2 , v2 - v1 - v3
        public Transform v0, v1, v2, v3;
        public TriTri_Test2.Triangle3 tri0;
        public TriTri_Test2.Triangle3 tri1;

        public void Update()
        {
            tri0.V[0] = v0.position;
            tri0.V[1] = v1.position;
            tri0.V[2] = v2.position;

            tri1.V[0] = v2.position;
            tri1.V[1] = v1.position;
            tri1.V[2] = v3.position;
        }

        public void Draw(Color color)
        {
            DebugWide.DrawLine(tri0.V[0], tri0.V[1], color);
            DebugWide.DrawLine(tri0.V[0], tri0.V[2], color);
            DebugWide.DrawLine(tri0.V[1], tri0.V[2], color);

            DebugWide.DrawLine(tri1.V[0], tri1.V[2], color);
            DebugWide.DrawLine(tri1.V[1], tri1.V[2], color);
        }
    }


    private Tri3 _tri0;
    private Tri3 _tri1;
    private TriTri_Test2.IntrTriangle3Triangle3 _intrTriTri;

    private Tetra3 _tetra_0_1; //삼각형 0,1 합친모양의 사각형
    private Tetra3 _tetra_2_3;
    private TriTri_Test2.IntrTriangle3Triangle3 _intr_0_2;
    private TriTri_Test2.IntrTriangle3Triangle3 _intr_0_3;
    private TriTri_Test2.IntrTriangle3Triangle3 _intr_1_2;
    private TriTri_Test2.IntrTriangle3Triangle3 _intr_1_3;

    //===========================================


    //===========================================

    private void OnDrawGizmos()
    {
        //if (null == (object)_line0_start)
        if(false == _init)
            return;

        if(false)
        {
            float len;
            DebugWide.DrawLine(_line0_start.position, _line0_end.position, Color.red);
            len = (_line0_start.position - _line0_end.position).magnitude;
            DebugWide.DrawCirclePlane(_line0_start.position,len,Vector3.up, Color.red);
            DebugWide.DrawCircle(_line0_start.position, 0.1f, Color.red);

            //DebugWide.DrawLine(_line1_start.position, _line1_end.position, Color.green);
            //len = (_line1_start.position - _line1_end.position).magnitude;
            //DebugWide.DrawCirclePlane(_line1_start.position,len,Vector3.up, Color.green);
            //DebugWide.DrawCircle(_line1_start.position, 0.1f, Color.green);


            DebugWide.DrawLine(_ts0.position, _ts1.position, Color.magenta);

            //===============

            _model_1.cylinder.Draw(Color.green);

            //===============

            UtilGS9.Plane plane = new UtilGS9.Plane(_line0_start.position, _line0_end.position, _line1_start.position);
            Vector3 contactPt;
            if(0 != UtilGS9.Plane.Intersect(out contactPt, new LineSegment3(_line2_start.position, _line2_end.position), plane))
            {
                DebugWide.DrawCircle(contactPt, 0.1f, Color.gray);   
            }
            plane.Draw(5,Color.gray);

            //===============
            DebugWide.DrawLine(_line2_start.position, _line2_end.position, Color.green);
            DebugWide.DrawLine(_line0_start.position, _line1_end.position, Color.green);
            DebugWide.DrawLine(_line0_start.position, _line1_start.position, Color.green);
            DebugWide.DrawLine(_line0_end.position, _line1_end.position, Color.green);
            DebugWide.DrawCircle(_ts2.position, 0.1f, Color.green);


        }

        //===============

        if (true)
        {
            //TriTri_Test1.Draw(_tri_0.tri, Color.blue);
            //TriTri_Test1.Draw(_tri_1.tri, Color.magenta);

            //if(_intrTriTri.mIntersectionType != TriTri_Test2.eIntersectionType.EMPTY)
            //{
            //    string temp = string.Empty;
            //    foreach(Vector3 p in _intrTriTri.mPoint)
            //    {
            //        temp += p + " ";
            //    }
            //    DebugWide.LogBlue(_intrTriTri.mQuantity + "  " + _intrTriTri.mIntersectionType + "   " + _intrTriTri.mReportCoplanarIntersections
            //                      + "   " + temp);
            //}
            //_tri0.Draw(Color.blue);
            //_tri1.Draw(Color.magenta);
            //_intrTriTri.Draw(Color.red);


            _tetra_0_1.Draw(Color.blue);
            _tetra_2_3.Draw(Color.magenta);
            _intr_0_2.Draw(Color.red);
            _intr_0_3.Draw(Color.red);
            _intr_1_2.Draw(Color.red);
            _intr_1_3.Draw(Color.red);
        }

    }

	// Use this for initialization
	void Start () 
    {
        _init = true;
        Transform obj = Hierarchy.GetTransform(transform.parent, "obj");
        //DebugWide.LogBlue(transform.parent + "  :  "+ _obj);
        Transform line_0 = Hierarchy.GetTransform(obj, "line_0");
        _line0_start = Hierarchy.GetTransform(line_0, "start");
        _line0_end = Hierarchy.GetTransform(line_0, "end");
        _ts0 = Hierarchy.GetTransform(line_0, "ts");

        Transform line_1 = Hierarchy.GetTransform(obj, "line_1");
        _line1_start = Hierarchy.GetTransform(line_1, "start");
        _line1_end = Hierarchy.GetTransform(line_1, "end");
        _ts1 = Hierarchy.GetTransform(line_1, "ts");

        Transform line_2 = Hierarchy.GetTransform(obj, "line_2");
        _line2_start = Hierarchy.GetTransform(line_2, "start");
        _line2_end = Hierarchy.GetTransform(line_2, "end");
        _ts2 = Hierarchy.GetTransform(line_2, "ts");

        //===========================================

        Transform tri_tri = Hierarchy.GetTransform(transform.parent, "tri_tri");
        Transform tri = Hierarchy.GetTransform(tri_tri, "tri_0");
        _tri0.v0 = Hierarchy.GetTransform(tri, "v0");
        _tri0.v1 = Hierarchy.GetTransform(tri, "v1");
        _tri0.v2 = Hierarchy.GetTransform(tri, "v2");

        tri = Hierarchy.GetTransform(tri_tri, "tri_1");
        _tri1.v0 = Hierarchy.GetTransform(tri, "v0");
        _tri1.v1 = Hierarchy.GetTransform(tri, "v1");
        _tri1.v2 = Hierarchy.GetTransform(tri, "v2");

        //===========================================

        Transform tetra_tetra = Hierarchy.GetTransform(transform.parent, "tetra_tetra");
        Transform tetra = Hierarchy.GetTransform(tetra_tetra, "tetra_0");
        _tetra_0_1.v0 = Hierarchy.GetTransform(tetra, "v0");
        _tetra_0_1.v1 = Hierarchy.GetTransform(tetra, "v1");
        _tetra_0_1.v2 = Hierarchy.GetTransform(tetra, "v2");
        _tetra_0_1.v3 = Hierarchy.GetTransform(tetra, "v3");

        tetra = Hierarchy.GetTransform(tetra_tetra, "tetra_1");
        _tetra_2_3.v0 = Hierarchy.GetTransform(tetra, "v0");
        _tetra_2_3.v1 = Hierarchy.GetTransform(tetra, "v1");
        _tetra_2_3.v2 = Hierarchy.GetTransform(tetra, "v2");
        _tetra_2_3.v3 = Hierarchy.GetTransform(tetra, "v3");

        //===========================================

        _tri0.tri0 = TriTri_Test2.Triangle3.Zero();
        _tri1.tri0 = TriTri_Test2.Triangle3.Zero();
        _intrTriTri = new TriTri_Test2.IntrTriangle3Triangle3(_tri0.tri0, _tri1.tri0);

        _tetra_0_1.tri0 = TriTri_Test2.Triangle3.Zero();
        _tetra_0_1.tri1 = TriTri_Test2.Triangle3.Zero();
        _tetra_2_3.tri0 = TriTri_Test2.Triangle3.Zero();
        _tetra_2_3.tri1 = TriTri_Test2.Triangle3.Zero();
        _intr_0_2 = new TriTri_Test2.IntrTriangle3Triangle3(_tetra_0_1.tri0, _tetra_2_3.tri0);
        _intr_0_3 = new TriTri_Test2.IntrTriangle3Triangle3(_tetra_0_1.tri0, _tetra_2_3.tri1);
        _intr_1_2 = new TriTri_Test2.IntrTriangle3Triangle3(_tetra_0_1.tri1, _tetra_2_3.tri0);
        _intr_1_3 = new TriTri_Test2.IntrTriangle3Triangle3(_tetra_0_1.tri1, _tetra_2_3.tri1);
        //===========================================

        //int[] a = new int[3] { 1, 2, 3 };
        //PrintArray(1, a);
        //int[] b = TestArray(ref a);
        //b[0] = 4;
        //PrintArray(2, a);
        //함수인자배열에 out 사용은, 함수안에서 배열 모든요소값을 모두 할당하겠다는 의미가 있음 
        //함수인자배열의 ref 사용은, 명시하지 않는것과 별다른 차이가 없음 
        //c#의 배열은 동작할당된다. c++처럼 정적할당이 안됨  

        //배열 정적할당하기
        //https://docs.microsoft.com/ko-kr/dotnet/csharp/language-reference/operators/stackalloc

        //===========================================

	}

    public int[] TestArray(ref int[] arr)
    {
        arr[0] = 8;

        return arr;
    }

    public void PrintArray(int num, int[] arr)
    {
        string temp = string.Empty;
        for (int i = 0; i < arr.Length;++i)
        {
            temp += arr[i] + " ";
        }
        DebugWide.LogBlue( num + "__ "+temp);
    }
	
	// Update is called once per frame
	void Update () 
    {


        //if(true == TriTri_Test1.Triangles_colliding(_tri_0.tri, _tri_1.tri))
        //{
        //    DebugWide.LogBlue("TriTri collision !!");
        //}

        //_tri0.Update();
        //_tri1.Update();
        //_intrTriTri.Find();

        _tetra_0_1.Update();
        _tetra_2_3.Update();
        _intr_0_2.Find();
        _intr_0_3.Find();
        _intr_1_2.Find();
        _intr_1_3.Find();


        //===========================================

        float s, t;
        LineSegment3 unit0 = new LineSegment3(_line0_start.position, _line0_end.position);
        LineSegment3 unit1 = new LineSegment3(_line1_start.position, _line1_end.position);

        float sqrdis = LineSegment3.DistanceSquared(unit0, unit1, out s, out t);
        _ts0.position = unit0.direction * s + _line0_start.position;
        _ts1.position = unit1.direction * t + _line1_start.position;

        float line1_width = 0.1f;
        if (sqrdis < line1_width*line1_width)
        {
            DebugWide.LogBlue("Collision!!" + "   s:" + s + "  t:" + t);

            float len = (_line0_start.position - _line0_end.position).magnitude;
            //Vector3 newDir = (_line1_end.position - _line0_start.position).normalized;
            Vector3 newDir = (_ts1.position - _line0_start.position).normalized;
            _line0_end.position = _line0_start.position + newDir * len;
        }

        _model_1.cylinder.Set(Vector3.up, _line1_start.position, line1_width, _line1_end.position, line1_width);

        //====================================

        float line_t = 0f;
        if(Tetragon.Intersect(out line_t, _line0_start.position, _line0_end.position, _line1_end.position , _line1_start.position,
                              new LineSegment3(_line2_start.position, _line2_end.position)))
        {
            _ts2.position = _line2_start.position + (_line2_end.position - _line2_start.position) * line_t;
        }


	}

}
