using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

//선분 vs 선분  충돌 버젼
public class Test_LineCollision_0 : MonoBehaviour 
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

    private Transform _ts_seg0_s = null;
    private Transform _ts_seg0_e = null;
    private Transform _ts_seg1_s = null;
    private Transform _ts_seg1_e = null;

    private Transform _ts_seg2_s = null;
    private Transform _ts_seg2_e = null;
    private Transform _ts_seg3_s = null;
    private Transform _ts_seg3_e = null;
    //===========================================

    public struct Tri3
    {
        public Transform v0, v1, v2;
        public Triangle3 tri0;

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


    private Tri3 _tri0;
    private Tri3 _tri1;
    private IntrTriangle3Triangle3 _intrTriTri;

    private MovingSegement3 _moveSegment = new MovingSegement3();
    private LineSegment3 _cur_seg_A;
    private LineSegment3 _cur_seg_B;
    private LineSegment3 _prev_seg_A;
    private LineSegment3 _prev_seg_B;
    //===========================================


    //===========================================

    private void OnDrawGizmos()
    {
        //if (null == (object)_line0_start)
        if(false == _init)
            return;
        
        //===============

        if (false)
        {
            _tri0.Update();
            _tri1.Update();
            _intrTriTri.Find_Twice();    

            if (_intrTriTri.mIntersectionType != eIntersectionType.EMPTY)
            {
                string temp = string.Empty;
                foreach (Vector3 p in _intrTriTri.mPoint)
                {
                    temp += p + " ";
                }
                DebugWide.LogBlue(" *** "+ _intrTriTri.mQuantity + "  " + _intrTriTri.mIntersectionType + "   " + _intrTriTri.mReportCoplanarIntersections
                                  + "   " + temp);
            }
            _tri0.Draw(Color.blue);
            _tri1.Draw(Color.magenta);
            _intrTriTri.Draw(Color.red);

        }

        //if(false)
        {
            DebugWide.DrawLine(_line0_start.position, _line0_end.position, Color.green);
            DebugWide.DrawLine(_line1_start.position, _line1_end.position, Color.green);
            _moveSegment.Draw();
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

        //v1  - v2
        // |  /  |   
        //v0  - v3
        _ts_seg0_s = Hierarchy.GetTransform(tetra, "s0_s");
        _ts_seg0_e = Hierarchy.GetTransform(tetra, "s0_e");
        _ts_seg1_s = Hierarchy.GetTransform(tetra, "s1_s");
        _ts_seg1_e = Hierarchy.GetTransform(tetra, "s1_e");

        tetra = Hierarchy.GetTransform(tetra_tetra, "tetra_1");
        _ts_seg2_s = Hierarchy.GetTransform(tetra, "s0_s");
        _ts_seg2_e = Hierarchy.GetTransform(tetra, "s0_e");
        _ts_seg3_s = Hierarchy.GetTransform(tetra, "s1_s");
        _ts_seg3_e = Hierarchy.GetTransform(tetra, "s1_e");

        //===========================================

        _tri0.tri0 = Triangle3.Zero();
        _tri1.tri0 = Triangle3.Zero();
        _intrTriTri = new IntrTriangle3Triangle3(_tri0.tri0, _tri1.tri0);

        //===========================================

        //CalcSegment_PushPoint 함수 호출전 초기값 prev 와 cur 을 같게 만들어 줘야 한다  
        //_moveSegment.InitSegAB(new LineSegment3(_line0_start.position, _line0_end.position),
        //new LineSegment3(_line1_start.position, _line1_end.position));
        _cur_seg_A = new LineSegment3(_line0_start.position, _line0_end.position);
        _cur_seg_B = new LineSegment3(_line1_start.position, _line1_end.position);
        _prev_seg_A = _cur_seg_A;
        _prev_seg_B = _cur_seg_B;


	}


    public float __RateAtoB = 0.5f;
    public bool __AllowFixed_A = false;
    public bool __AllowFixed_B = false;
	void Update () 
    {

        //_moveSegment.Update_Tetra(_ts_seg0_s.position, _ts_seg0_e.position, _ts_seg1_s.position, _ts_seg1_e.position,
        //_ts_seg2_s.position, _ts_seg2_e.position, _ts_seg3_s.position, _ts_seg3_e.position);

        _cur_seg_A = new LineSegment3(_line0_start.position, _line0_end.position);
        _cur_seg_B = new LineSegment3(_line1_start.position, _line1_end.position);

        _moveSegment.Find(_prev_seg_A, _prev_seg_B, _cur_seg_A, _cur_seg_B);
        
        //_moveSegment.CalcSegment_FromContactPt();
        //_moveSegment.CalcSegment_FromContactPt(false, false, _moveSegment._cur_seg_A.origin, _moveSegment._cur_seg_B.origin);
        _moveSegment.CalcSegment_PushPoint(__RateAtoB,__AllowFixed_A, __AllowFixed_B, _moveSegment._cur_seg_A.origin, _moveSegment._cur_seg_B.origin);


        //최종 계산된 선분 적용
        _line0_start.position = _moveSegment._cur_seg_A.origin;
        _line0_end.position = _moveSegment._cur_seg_A.last;
        _line1_start.position = _moveSegment._cur_seg_B.origin;
        _line1_end.position = _moveSegment._cur_seg_B.last;

        //이전 선분위치 갱신 
        _prev_seg_A = _moveSegment._cur_seg_A;
        _prev_seg_B = _moveSegment._cur_seg_B;
        //===========================================

        //float s, t;
        //LineSegment3 unit0 = new LineSegment3(_line0_start.position, _line0_end.position);
        //LineSegment3 unit1 = new LineSegment3(_line1_start.position, _line1_end.position);

        //float sqrdis = LineSegment3.DistanceSquared(unit0, unit1, out s, out t);
        //_ts0.position = unit0.direction * s + _line0_start.position;
        //_ts1.position = unit1.direction * t + _line1_start.position;

        //float line1_width = 0.1f;
        //if (sqrdis < line1_width*line1_width)
        //{
        //    DebugWide.LogBlue("Collision!!" + "   s:" + s + "  t:" + t);

        //    float len = (_line0_start.position - _line0_end.position).magnitude;
        //    //Vector3 newDir = (_line1_end.position - _line0_start.position).normalized;
        //    Vector3 newDir = (_ts1.position - _line0_start.position).normalized;
        //    _line0_end.position = _line0_start.position + newDir * len;
        //}

        //_model_1.cylinder.Set(Vector3.up, _line1_start.position, line1_width, _line1_end.position, line1_width);

        ////====================================

        //float line_t = 0f;
        //if(Tetragon.Intersect(out line_t, _line0_start.position, _line0_end.position, _line1_end.position , _line1_start.position,
        //                      new LineSegment3(_line2_start.position, _line2_end.position)))
        //{
        //    _ts2.position = _line2_start.position + (_line2_end.position - _line2_start.position) * line_t;
        //}


	}



    //======================================================
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

    public int[] TestArray(ref int[] arr)
    {
        arr[0] = 8;

        return arr;
    }
    public void PrintArray(int num, int[] arr)
    {
        string temp = string.Empty;
        for (int i = 0; i < arr.Length; ++i)
        {
            temp += arr[i] + " ";
        }
        DebugWide.LogBlue(num + "__ " + temp);
    }
}


public class Test_LineCollision : MonoBehaviour 
{
    private MovingModel _movingModel = new MovingModel();
    public float __RateAtoB = 0.5f;

	private void OnDrawGizmos()
	{
        _movingModel.Draw();
	}

	private void Start()
	{
        //gameObject.AddComponent<MovingModel>();
        _movingModel.Init(transform.parent);
	}

	private void Update()
	{
        _movingModel.__RateAtoB = __RateAtoB;
        _movingModel.Update();
	}
}

//움직이는 모형의 교차검사 
public class MovingModel
{
    public struct TR_Segment
    {
        public Transform start;
        public Transform end;
    }
    public class Frame
    {
        public const int MAX_SEGMENT_NUMBER = 5;
        public int _seg_count = 0;

        public Transform _tr_frame = null;

        public LineSegment3[] _prev_seg = null;
        public LineSegment3[] _cur_seg = null;
        public TR_Segment[] _tr_seg = null; 

        public void Draw(Color color)
        {
            if (null == _tr_seg) return;

            for (int i = 0; i < _seg_count; i++)
            {
                DebugWide.DrawLine(_prev_seg[i].origin, _prev_seg[i].last, Color.gray);
            }

            for (int i = 0; i < _seg_count;i++)
            {
                DebugWide.DrawLine(_tr_seg[i].start.position, _tr_seg[i].end.position, color);
            }
        }


        public void Init(Transform tr_frame)
        {
            _prev_seg = new LineSegment3[MAX_SEGMENT_NUMBER];
            _cur_seg = new LineSegment3[MAX_SEGMENT_NUMBER];

            _tr_seg = new TR_Segment[MAX_SEGMENT_NUMBER];

            _tr_frame = tr_frame;
            Transform seg = null;
            for (int i = 0; i < MAX_SEGMENT_NUMBER;i++)
            {
                if(0 == i)
                    seg = Hierarchy.GetTransform(tr_frame, "root");
                else
                    seg = Hierarchy.GetTransform(tr_frame, "sub_"+(i-1));
                
                if(null != (object)seg)
                {
                    _seg_count++;
                    _tr_seg[i].start = Hierarchy.GetTransform(seg, "start");
                    _tr_seg[i].end = Hierarchy.GetTransform(seg, "end");

                    _cur_seg[i].origin = _tr_seg[i].start.position;
                    _cur_seg[i].last = _tr_seg[i].end.position;
                    _prev_seg[i] = _cur_seg[i];
                }
            }

        }//end init

        public void Update()
        {
            for (int i = 0; i < _seg_count;i++)
            {
                _cur_seg[i].origin = _tr_seg[i].start.position;
                _cur_seg[i].last = _tr_seg[i].end.position;
            }
        }

        public void After_Update()
        {
            for (int i = 0; i < _seg_count; i++)
            {
                _prev_seg[i] = _cur_seg[i];
            }
        }

    }//end class

    public MovingSegement3 _movingSegment = new MovingSegement3();
    public Frame _frame_sword_A = new Frame();
    public Frame _frame_sword_B = new Frame();

    public void Draw()
    {
        _frame_sword_A.Draw(Color.blue);
        _frame_sword_B.Draw(Color.magenta);

        if(__update)
        {
            DebugWide.DrawLine(__calc_rootSeg_a.origin, __calc_rootSeg_a.last, Color.red);
            DebugWide.DrawLine(__calc_rootSeg_b.origin, __calc_rootSeg_b.last, Color.red);    
        }

    }

    public void Init(Transform root)
    {
        Transform model = Hierarchy.GetTransform(root, "model");
        Transform frame = null;
        frame = Hierarchy.GetTransform(model, "frame_0");
        _frame_sword_A.Init(frame);
        frame = Hierarchy.GetTransform(model, "frame_1");
        _frame_sword_B.Init(frame);
    }

    public void GetRootSegment_AroundRotate(out LineSegment3 cur_root, LineSegment3 cur_subSeg, LineSegment3 prev_root, LineSegment3 prev_subSeg)
    {
        cur_root.origin = prev_root.origin;
        Vector3 dir_o = prev_root.last - prev_subSeg.origin;
        cur_root.last = cur_subSeg.origin + dir_o;
    }

    public float __RateAtoB = 0.5f;
    //public bool __AllowFixed_A = false;
    //public bool __AllowFixed_B = false;
    bool __update = false;
    LineSegment3 __calc_rootSeg_a, __calc_rootSeg_b;
    public void Update()
    {
        _frame_sword_A.Update();
        _frame_sword_B.Update();
        //=================================================

        MovingSegement3.eCalcMethod eCalc_a;
        MovingSegement3.eCalcMethod eCalc_b;
        LineSegment3 prev_A, cur_A;
        LineSegment3 prev_B, cur_B;
        Vector3 fixed_A = _frame_sword_A._cur_seg[0].origin;
        Vector3 fixed_B = _frame_sword_B._cur_seg[0].origin;
        Vector3 stand = _frame_sword_A._prev_seg[0].origin;
        bool recalc = false;
        __update = false;
        float min_len = 1000000f;


        //for (int i = 0; i < Frame.MAX_LEAF_NUMBER - 1; i++)
        //{
            //for (int j = i + 1; j < Frame.MAX_LEAF_NUMBER; j++)
        for (int i = 0; i < _frame_sword_A._seg_count ; i++)
        {
            for (int j = 0; j < _frame_sword_B._seg_count; j++)
            {
                prev_A = _frame_sword_A._prev_seg[i];
                cur_A = _frame_sword_A._cur_seg[i];    

                prev_B = _frame_sword_B._prev_seg[j];
                cur_B = _frame_sword_B._cur_seg[j];

                _movingSegment.Find(prev_A, prev_B, cur_A, cur_B);

                //recalc = _movingSegment.CalcSegment_PushPoint(__RateAtoB, __AllowFixed_A, __AllowFixed_B,
                //fixed_A, fixed_B);

                //=============
                if (0 == i)
                    eCalc_a = MovingSegement3.eCalcMethod.Rotate_Root;
                else 
                    eCalc_a = MovingSegement3.eCalcMethod.Rotate_Sub;

                if (0 == j)
                    eCalc_b = MovingSegement3.eCalcMethod.Rotate_Root;
                else
                    eCalc_b = MovingSegement3.eCalcMethod.Rotate_Sub;
                //=============

                recalc = _movingSegment.CalcSubSegment_PushPoint(__RateAtoB, eCalc_a, eCalc_b,
                                                                 _frame_sword_A._prev_seg[0], _frame_sword_B._prev_seg[0]);

                if(recalc)
                {
                    __update = true;

                    //결과들중 최소거리의 선분 하나 선택
                    float new_len = (_movingSegment._minV - stand).sqrMagnitude; 
                    if(min_len > new_len)
                    {
                        min_len = new_len;

                        __calc_rootSeg_a = _movingSegment._cur_seg_A;
                        __calc_rootSeg_b = _movingSegment._cur_seg_B;
                        if(0 != i )
                        {
                            this.GetRootSegment_AroundRotate(out __calc_rootSeg_a, _movingSegment._cur_seg_A,
                                                             _frame_sword_A._prev_seg[0], _frame_sword_A._prev_seg[i]);    
                        }
                        if (0 != j)
                        {
                            this.GetRootSegment_AroundRotate(out __calc_rootSeg_b, _movingSegment._cur_seg_B,
                                                             _frame_sword_B._prev_seg[0], _frame_sword_B._prev_seg[j]);
                        }

                    }
                }
            }
        }//end for

        //=================================================
        //적용 
        if(__update)
        {
            _frame_sword_A._tr_frame.rotation = Quaternion.FromToRotation(Vector3.forward, __calc_rootSeg_a.direction);
            _frame_sword_A._tr_frame.position = __calc_rootSeg_a.origin;

            _frame_sword_B._tr_frame.rotation = Quaternion.FromToRotation(Vector3.forward, __calc_rootSeg_b.direction);
            _frame_sword_B._tr_frame.position = __calc_rootSeg_b.origin;    
        }


        //=================================================
        _frame_sword_A.After_Update();
        _frame_sword_B.After_Update();

    }
}