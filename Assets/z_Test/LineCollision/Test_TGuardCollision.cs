using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


public class Test_TGuardCollision : MonoBehaviour 
{
    private MovingModel _movingModel = new MovingModel();
    public float __RateAtoB = 0.5f;
    //public float __radius_A = 0.1f;
    //public float __radius_B = 0.1f;

	private void OnDrawGizmos()
	{
        _movingModel.__RateAtoB = __RateAtoB;
        //_movingModel.__radius_A = __radius_A;
        //_movingModel.__radius_B = __radius_B;
        _movingModel.Update();

        _movingModel.Draw();
	}

	private void Start()
	{

        Transform model = Hierarchy.GetTransform(transform.parent, "model");
        Transform frame_A , frame_B;
        frame_A = Hierarchy.GetTransform(model, "frame_0");
        frame_B = Hierarchy.GetTransform(model, "frame_1");

        _movingModel.Init(frame_A, frame_B);
	}

	private void Update()
	{
        //_movingModel.__RateAtoB = __RateAtoB;
        //_movingModel.Update();
	}
}

//움직이는 모형의 교차검사 
public class MovingModel
{
    public struct TGuard_Info
    {
        public Transform start;
        public Transform end;
        public float radius;

        public LineSegment3 prev_seg;
        public LineSegment3 cur_seg;

        public LineSegment3 ToSegment()
        {
            return new LineSegment3(start.position, end.position);
        }
    }
    public class Frame
    {
        public const int MAX_SEGMENT_NUMBER = 5;
        public int _seg_count = 0;

        public Transform _tr_frame = null;

        //public LineSegment3[] _prev_seg = null;
        //public LineSegment3[] _cur_seg = null;
        public TGuard_Info[] _info = null; 

        public void Draw(Color color)
        {
            if (null == _info) return;

            for (int i = 0; i < _seg_count; i++)
            {
                DebugWide.DrawLine(_info[i].prev_seg.origin, _info[i].prev_seg.last, Color.gray);
            }

            for (int i = 0; i < _seg_count; i++)
            {
                DebugWide.DrawLine(_info[i].start.position, _info[i].end.position, color);
                DebugWide.DrawCircle(_info[i].start.position, _info[i].radius, color);
                DebugWide.DrawCircle(_info[i].end.position, _info[i].radius, color);
            }
        }


        public void Init(Transform tr_frame)
        {
            //_prev_seg = new LineSegment3[MAX_SEGMENT_NUMBER];
            //_cur_seg = new LineSegment3[MAX_SEGMENT_NUMBER];

            _info = new TGuard_Info[MAX_SEGMENT_NUMBER];

            _tr_frame = tr_frame;
            Transform root = null;
            Transform seg = null;
            for (int i = 0; i < MAX_SEGMENT_NUMBER;i++)
            {
                if (0 == i)
                {
                    seg = Hierarchy.GetTransform(tr_frame, "root");
                    root = seg;
                }
                else
                    seg = Hierarchy.GetTransform(root, "sub_"+(i-1));
                
                if(null != (object)seg)
                {
                    _seg_count++;
                    _info[i].start = Hierarchy.GetTransform(seg, "start");
                    _info[i].end = Hierarchy.GetTransform(seg, "end");
                    _info[i].radius = 0.02f; //임시로 값 넣어둠 

                    _info[i].cur_seg.origin = _info[i].start.position;
                    _info[i].cur_seg.last = _info[i].end.position;
                    _info[i].prev_seg = _info[i].cur_seg;

                }
            }

        }//end init

        public void Cur_Update()
        {
            for (int i = 0; i < _seg_count;i++)
            {
                _info[i].cur_seg.origin = _info[i].start.position;
                _info[i].cur_seg.last = _info[i].end.position;
            }
        }

        //public void Prev_Update()
        //{
        //    for (int i = 0; i < _seg_count; i++)
        //    {
        //        _info[i].prev_seg = _info[i].cur_seg;
        //    }
        //}

    }//end class

    public MovingSegement3 _movingSegment = new MovingSegement3();
    public Frame _frame_sword_A = new Frame();
    public Frame _frame_sword_B = new Frame();

    public void Draw()
    {
        if (false == __init) return;

        _frame_sword_A.Draw(Color.blue);
        _frame_sword_B.Draw(Color.magenta);

        DebugWide.DrawCircle(_movingSegment._meetPt_A, _movingSegment._radius_A, Color.gray);
        DebugWide.DrawCircle(_movingSegment._meetPt_B, _movingSegment._radius_B, Color.gray);


    }


    public bool __init = false;
    Vector3[,] __prev_A_B_order = null;
    public void Init(Transform frame_A, Transform frame_B)
    {
        __init = true;

        //Transform model = Hierarchy.GetTransform(root, "model");
        //Transform frame = null;
        //frame = Hierarchy.GetTransform(model, "frame_0");
        //_frame_sword_A.Init(frame);
        //frame = Hierarchy.GetTransform(model, "frame_1");
        //_frame_sword_B.Init(frame);

        _frame_sword_A.Init(frame_A);
        _frame_sword_B.Init(frame_B);


        Vector3 pt_start, pt_end;
        __prev_A_B_order = new Vector3[_frame_sword_A._seg_count, _frame_sword_B._seg_count];
        for (int a = 0; a < _frame_sword_A._seg_count; a++)
        {
            for (int b = 0; b < _frame_sword_B._seg_count; b++)
            {
                LineSegment3.ClosestPoints(out pt_start, out pt_end, 
                                           _frame_sword_A._info[a].ToSegment(), _frame_sword_B._info[b].ToSegment());
                _movingSegment.__prev_A_B_order = pt_end - pt_start;
                __prev_A_B_order[a, b] = _movingSegment.__prev_A_B_order;
            }
        }

    }

    //private Quaternion __prev_A_rot = Quaternion.identity;
    //private Quaternion __prev_B_rot = Quaternion.identity;
    private Quaternion __min_A_rot = Quaternion.identity;
    private Quaternion __min_B_rot = Quaternion.identity;
    public void Update_2()
    {
        if (false == __init) return;

        _frame_sword_A.Cur_Update();
        _frame_sword_B.Cur_Update();
        //=================================================

        LineSegment3 prev_A, cur_A;
        LineSegment3 prev_B, cur_B;

        int idx = 0;

        prev_A = _frame_sword_A._info[idx].prev_seg;
        cur_A = _frame_sword_A._info[idx].cur_seg;

        prev_B = _frame_sword_B._info[idx].prev_seg;
        cur_B = _frame_sword_B._info[idx].cur_seg;

        _movingSegment._radius_A = _frame_sword_A._info[idx].radius;
        _movingSegment._radius_B = _frame_sword_B._info[idx].radius;
        _movingSegment.Input_TGuard(prev_A, prev_B, cur_A, cur_B);

        bool contact = _movingSegment.Find_TGuard_vs_TGuard(__RateAtoB, _frame_sword_A._tr_frame, _frame_sword_B._tr_frame);
        if (true == contact)
        {
            _frame_sword_A._tr_frame.rotation = _movingSegment.__localRota_A * _frame_sword_A._tr_frame.rotation; //실제적용 
            _frame_sword_B._tr_frame.rotation = _movingSegment.__localRota_B * _frame_sword_B._tr_frame.rotation;

            //_frame_sword_A._tr_frame.rotation = _movingSegment.__localRota_A * __prev_A_rot; //실제적용 
            //_frame_sword_B._tr_frame.rotation = _movingSegment.__localRota_B * __prev_B_rot;
        }

        //=================================================
        //__prev_A_rot = _frame_sword_A._tr_frame.rotation;
        //__prev_B_rot = _frame_sword_B._tr_frame.rotation;

        _frame_sword_A._info[idx].prev_seg = _movingSegment._prev_seg_A;
        _frame_sword_B._info[idx].prev_seg = _movingSegment._prev_seg_B;
        __prev_A_B_order[idx, idx] = _movingSegment.__prev_A_B_order;
        //_frame_sword_A.Prev_Update();
        //_frame_sword_B.Prev_Update();
    }

    private const int ROOT0 = 0;
    public float __RateAtoB = 0.5f;
    //public float __radius_A = 0.1f;
    //public float __radius_B = 0.1f;
    bool __update = false;
    //LineSegment3 __calc_rootSeg_a, __calc_rootSeg_b;
    public void Update()
    {

        if (false == __init) return;

        _frame_sword_A.Cur_Update();
        _frame_sword_B.Cur_Update();
        //=================================================

        LineSegment3 prev_A, cur_A;
        LineSegment3 prev_B, cur_B;
        //Vector3 stand = _frame_sword_A._prev_seg[ROOT0].origin;
        bool recalc = false;
        __update = false;
        float min_len = 1000000f;
        float max_len = 0f;
        __min_A_rot = Quaternion.identity;
        __min_B_rot = Quaternion.identity;


        //DebugWide.LogBlue(_frame_sword_A._seg_count);
        for (int a = 0; a < _frame_sword_A._seg_count ; a++)
        {
            for (int b = 0; b < _frame_sword_B._seg_count; b++)
            {
                prev_A = _frame_sword_A._info[a].prev_seg;
                cur_A = _frame_sword_A._info[a].cur_seg;    

                prev_B = _frame_sword_B._info[b].prev_seg;
                cur_B = _frame_sword_B._info[b].cur_seg;

                _movingSegment.__prev_A_B_order = __prev_A_B_order[a, b]; //*** 이전순서 복원 ***

                _movingSegment._radius_A = _frame_sword_A._info[a].radius;
                _movingSegment._radius_B = _frame_sword_B._info[b].radius;
                _movingSegment.Input_TGuard(prev_A, prev_B, cur_A, cur_B);


                //=============

                recalc = _movingSegment.Find_TGuard_vs_TGuard(__RateAtoB,
                                                              _frame_sword_A._tr_frame, _frame_sword_B._tr_frame);


                _frame_sword_A._info[a].prev_seg = _movingSegment._prev_seg_A;
                _frame_sword_B._info[b].prev_seg = _movingSegment._prev_seg_B;
                __prev_A_B_order[a, b] = _movingSegment.__prev_A_B_order;
                //=============

                if(recalc)
                {
                    __update = true;

                    //Vector3 n_left = VOp.Normalize(prev_A.direction);
                    //float len_proj_left = Vector3.Dot(n_left, (_movingSegment._meetPt - prev_A.origin));
                    //Vector3 pt_proj_A = len_proj_left * n_left + prev_A.origin;

                    //n_left = VOp.Normalize(prev_B.direction);
                    //len_proj_left = Vector3.Dot(n_left, (_movingSegment._meetPt - prev_B.origin));
                    //Vector3 pt_proj_B = len_proj_left * n_left + prev_B.origin;

                    ////meet으로부터 더 먼 점을 선택 , 기준이 되는 공통의 선분위의 점을 찾는다 
                    //Vector3 stand = pt_proj_B;
                    //if((pt_proj_A - _movingSegment._meetPt).sqrMagnitude > (pt_proj_B - _movingSegment._meetPt).sqrMagnitude)
                    //{
                    //    stand = pt_proj_A;
                    //}

                    //결과들중 최소거리의 선분 하나 선택
                    //float new_len = (_movingSegment._meetPt - stand).sqrMagnitude; 

                    //하나의 프레임에서 하나의 유형만 발생한다.
                    float new_len = _movingSegment.__cur_A_B_order.sqrMagnitude;
                    if(true == _movingSegment.__intr_rad_A_B)
                    {
                        //반지름의 합 내에서 발생
                        //선분 vs 선분  :  최소거리 찾기 
                        //선분 vs 사각꼴   :  최소거리 찾기 
                        //사각꼴 vs 사각꼴  :  최소거리 찾기 
                        if (min_len > new_len)
                        {
                            min_len = new_len;

                            __min_A_rot = _movingSegment.__localRota_A;
                            __min_B_rot = _movingSegment.__localRota_B;
                        }
                    }
                    else
                    {
                        //선분 vs 사각꼴   :  최대거리 찾기 
                        //사각꼴 vs 사각꼴  :  최대거리 찾기 
                        if (max_len < new_len)
                        {
                            max_len = new_len;

                            __min_A_rot = _movingSegment.__localRota_A;
                            __min_B_rot = _movingSegment.__localRota_B;
                        }
                    }



                    //DebugWide.DrawCircle(_movingSegment._meetPt, 0.05f, Color.red);
                }
            }
        }//end for

        //=================================================
        //적용 
        if(__update)
        {
            
            _frame_sword_A._tr_frame.rotation = __min_A_rot * _frame_sword_A._tr_frame.rotation; //실제적용 
            _frame_sword_B._tr_frame.rotation = __min_B_rot * _frame_sword_B._tr_frame.rotation;

        }

        //__prev_A_rot = _frame_sword_A._tr_frame.rotation;
        //__prev_B_rot = _frame_sword_B._tr_frame.rotation;

        //=================================================
        //_frame_sword_A.Prev_Update();
        //_frame_sword_B.Prev_Update();

    }
}