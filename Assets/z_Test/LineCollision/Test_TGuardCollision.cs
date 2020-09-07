using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


public class Test_TGuardCollision : MonoBehaviour 
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
                                                                 _frame_sword_A._cur_seg[i].origin,
                                                                 _frame_sword_B._cur_seg[j].origin,
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