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

    public struct TrTri
    {
        public Transform v0, v1, v2;
        public TriTri_Test2.Triangle3 tri;

        public void Update()
        {
            tri.V[0] = v0.position;
            tri.V[1] = v1.position;
            tri.V[2] = v2.position;
        }

        public void Draw(Color color)
        {
            DebugWide.DrawLine(tri.V[0], tri.V[1], color);
            DebugWide.DrawLine(tri.V[0], tri.V[2], color);
            DebugWide.DrawLine(tri.V[1], tri.V[2], color);
        }
    }

    private TrTri _tri_0;
    private TrTri _tri_1;
    private TriTri_Test2.IntrTriangle3Triangle3 _intr;

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
            _tri_0.Draw(Color.blue);
            _tri_1.Draw(Color.magenta);

            if(_intr.mIntersectionType != TriTri_Test2.eIntersectionType.EMPTY)
            {
                
                string temp = string.Empty;
                foreach(Vector3 p in _intr.mPoint)
                {
                    temp += p + " ";
                }

                DebugWide.LogBlue(_intr.mQuantity + "  " + _intr.mIntersectionType + "   " + _intr.mReportCoplanarIntersections
                                  + "   " + temp);

            }
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
        _tri_0.v0 = Hierarchy.GetTransform(tri, "v0");
        _tri_0.v1 = Hierarchy.GetTransform(tri, "v1");
        _tri_0.v2 = Hierarchy.GetTransform(tri, "v2");

        tri = Hierarchy.GetTransform(tri_tri, "tri_1");
        _tri_1.v0 = Hierarchy.GetTransform(tri, "v0");
        _tri_1.v1 = Hierarchy.GetTransform(tri, "v1");
        _tri_1.v2 = Hierarchy.GetTransform(tri, "v2");

        //===========================================

        _tri_0.tri = TriTri_Test2.Triangle3.Zero();
        _tri_1.tri = TriTri_Test2.Triangle3.Zero();
        _intr = new TriTri_Test2.IntrTriangle3Triangle3(_tri_0.tri, _tri_1.tri);
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

        _tri_0.Update();
        _tri_1.Update();
        //if(true == TriTri_Test1.Triangles_colliding(_tri_0.tri, _tri_1.tri))
        if (true == _intr.Find())
        {
            DebugWide.LogBlue("TriTri collision !!");
        }

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
