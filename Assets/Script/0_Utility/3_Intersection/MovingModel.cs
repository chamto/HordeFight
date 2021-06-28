using UnityEngine;
using UtilGS9;

namespace UtilGS9
{
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
                for (int i = 0; i < MAX_SEGMENT_NUMBER; i++)
                {
                    if (0 == i)
                    {
                        seg = Hierarchy.GetTransform(tr_frame, "root");
                        root = seg;
                    }
                    else
                        seg = Hierarchy.GetTransform(root, "sub_" + (i - 1));

                    if (null != (object)seg)
                    {
                        _seg_count++;
                        _info[i].start = Hierarchy.GetTransform(seg, "start");
                        _info[i].end = Hierarchy.GetTransform(seg, "end");
                        _info[i].radius = 0.05f; //임시로 값 넣어둠 

                        _info[i].cur_seg = new LineSegment3();
                        _info[i].cur_seg.origin = _info[i].start.position;
                        _info[i].cur_seg.last = _info[i].end.position;
                        _info[i].prev_seg = _info[i].cur_seg;

                    }
                }

            }//end init

            public void Cur_Update()
            {
                for (int i = 0; i < _seg_count; i++)
                {
                    _info[i].cur_seg.origin = _info[i].start.position;
                    _info[i].cur_seg.last = _info[i].end.position;
                }
            }

            public void Prev_Update()
            {
                for (int i = 0; i < _seg_count; i++)
                {
                    _info[i].prev_seg.origin = _info[i].start.position;
                    _info[i].prev_seg.last = _info[i].end.position;
                    //_info[i].prev_seg = _info[i].cur_seg;
                }
            }

        }//end class

        //public MovingSegement3 _movingSegment = new MovingSegement3();
        public SegementCCD _movingSegment = new SegementCCD();
        public Frame _frame_A = new Frame();
        public Frame _frame_B = new Frame();

        public float _rateAtoB = 0.5f;
        public bool _update = false;
        public bool _allowFixed_a = true;
        public bool _allowFixed_b = true;

        public void Draw()
        {
            if (false == __init) return;

            _frame_A.Draw(Color.blue);
            _frame_B.Draw(Color.magenta);

            DebugWide.DrawCircle(_movingSegment._meetPt_A, _movingSegment._radius_A, Color.gray);
            DebugWide.DrawCircle(_movingSegment._meetPt_B, _movingSegment._radius_B, Color.gray);

            _movingSegment.Draw();

            __find_seg_A.Draw(Color.white);
            __find_seg_B.Draw(Color.black);

        }

        public void SetFrame(bool allowFixed_a, bool allowFixed_b,  Frame frame_A, Frame frame_B)
        {
            __init = true;
            _frame_A = frame_A;
            _frame_B = frame_B;
            //Init_Prev_AB_Order();

            _allowFixed_a = allowFixed_a;
            _allowFixed_b = allowFixed_b;
        }

        public bool __init = false;
        Vector3[,] __prev_A_B_order = null;
        public void Init(Transform frame_A, Transform frame_B)
        {
            __init = true;

            //_frame_A = new Frame();
            //_frame_B = new Frame();

            _frame_A.Init(frame_A);
            _frame_B.Init(frame_B);


            //Init_Prev_AB_Order();

        }

        //public void Init_Prev_AB_Order()
        //{
        //    Vector3 pt_start, pt_end;
        //    __prev_A_B_order = new Vector3[_frame_A._seg_count, _frame_B._seg_count];
        //    for (int a = 0; a < _frame_A._seg_count; a++)
        //    {
        //        for (int b = 0; b < _frame_B._seg_count; b++)
        //        {
        //            LineSegment3.ClosestPoints(out pt_start, out pt_end,
        //                                       _frame_A._info[a].ToSegment(), _frame_B._info[b].ToSegment());
        //            _movingSegment.__prev_A_B_order = pt_end - pt_start;
        //            __prev_A_B_order[a, b] = _movingSegment.__prev_A_B_order;
        //        }
        //    }
        //}

        //private Quaternion __prev_A_rot = Quaternion.identity;
        //private Quaternion __prev_B_rot = Quaternion.identity;
        private Quaternion __min_A_rot = Quaternion.identity;
        private Quaternion __min_B_rot = Quaternion.identity;
        public void Update_2()
        {
            if (false == __init) return;

            _frame_A.Cur_Update();
            _frame_B.Cur_Update();
            //=================================================

            LineSegment3 prev_A, cur_A;
            LineSegment3 prev_B, cur_B;

            int idx = 0;

            prev_A = _frame_A._info[idx].prev_seg;
            cur_A = _frame_A._info[idx].cur_seg;

            prev_B = _frame_B._info[idx].prev_seg;
            cur_B = _frame_B._info[idx].cur_seg;

            _movingSegment._radius_A = _frame_A._info[idx].radius;
            _movingSegment._radius_B = _frame_B._info[idx].radius;
            _movingSegment.Input_TGuard(prev_A, prev_B, cur_A, cur_B);

            bool contact = _movingSegment.Find_TGuard_vs_TGuard(_rateAtoB, _allowFixed_a, _allowFixed_b, _frame_A._tr_frame, _frame_B._tr_frame);
            if (true == contact)
            {
                _frame_A._tr_frame.rotation = _movingSegment._localRota_A * _frame_A._tr_frame.rotation; //실제적용 
                _frame_B._tr_frame.rotation = _movingSegment._localRota_B * _frame_B._tr_frame.rotation;

                //_frame_sword_A._tr_frame.rotation = _movingSegment.__localRota_A * __prev_A_rot; //실제적용 
                //_frame_sword_B._tr_frame.rotation = _movingSegment.__localRota_B * __prev_B_rot;
            }

            //=================================================
            //__prev_A_rot = _frame_sword_A._tr_frame.rotation;
            //__prev_B_rot = _frame_sword_B._tr_frame.rotation;

            _frame_A._info[idx].prev_seg = _movingSegment._prev_seg_A;
            _frame_B._info[idx].prev_seg = _movingSegment._prev_seg_B;
            //__prev_A_B_order[idx, idx] = _movingSegment.__prev_A_B_order;
            //_frame_sword_A.Prev_Update();
            //_frame_sword_B.Prev_Update();
        }

        public LineSegment3 __find_seg_A = new LineSegment3();
        public LineSegment3 __find_seg_B = new LineSegment3();
        public Vector3 __dir_move_A = ConstV.v3_zero;
        public Vector3 __dir_move_B = ConstV.v3_zero;
        public bool Update()
        {

            if (false == __init) return false;

            _frame_A.Cur_Update();
            _frame_B.Cur_Update();
            //=================================================

            LineSegment3 prev_A, cur_A;
            LineSegment3 prev_B, cur_B;
            //Vector3 stand = _frame_sword_A._prev_seg[ROOT0].origin;
            bool recalc = false;
            _update = false;
            float min_len = 1000000f;
            float max_len = 0f;
            __min_A_rot = Quaternion.identity;
            __min_B_rot = Quaternion.identity;
            int find_a=-1, find_b=-1;

            //DebugWide.LogBlue(_frame_sword_A._seg_count);
            for (int a = 0; a < _frame_A._seg_count; a++)
            {
                for (int b = 0; b < _frame_B._seg_count; b++)
                {
                    
                    prev_A = _frame_A._info[a].prev_seg;
                    cur_A = _frame_A._info[a].cur_seg;

                    prev_B = _frame_B._info[b].prev_seg;
                    cur_B = _frame_B._info[b].cur_seg;

                    //_movingSegment.__prev_A_B_order = __prev_A_B_order[a, b]; //*** 이전순서 복원 ***

                    _movingSegment._radius_A = _frame_A._info[a].radius;
                    _movingSegment._radius_B = _frame_B._info[b].radius;
                    _movingSegment.Input_TGuard(prev_A, prev_B, cur_A, cur_B);


                    //=============

                    recalc = _movingSegment.Find_TGuard_vs_TGuard(_rateAtoB, _allowFixed_a, _allowFixed_b,
                                                                  _frame_A._tr_frame, _frame_B._tr_frame);

                    //if(0 < _movingSegment.__test_value)
                    //{
                    //    DebugWide.LogBlue("  a: " + a + "  b: " + b + " ------- ");
                    //}

                    //_frame_A._info[a].prev_seg = _movingSegment._prev_seg_A;
                    //_frame_B._info[b].prev_seg = _movingSegment._prev_seg_B;
                    //__prev_A_B_order[a, b] = _movingSegment.__prev_A_B_order;
                    //=============

                    if (recalc)
                    {
                        

                        _update = true;


                        //하나의 프레임에서 하나의 유형만 발생한다.
                        float new_len = _movingSegment.__cur_A_B_order.sqrMagnitude;
                        if (true == _movingSegment._intr_A_B_inside)
                        {
                            //반지름의 합 내에서 발생
                            //선분 vs 선분  :  최소거리 찾기 
                            //선분 vs 사각꼴   :  최소거리 찾기 
                            //사각꼴 vs 사각꼴  :  최소거리 찾기 
                            if (min_len > new_len)
                            {
                                min_len = new_len;

                                __min_A_rot = _movingSegment._localRota_A;
                                __min_B_rot = _movingSegment._localRota_B;
                                __dir_move_A = _movingSegment._dir_move_A;
                                __dir_move_B = _movingSegment._dir_move_B;

                                __find_seg_A = _movingSegment._cur_seg_A;
                                __find_seg_B = _movingSegment._cur_seg_B;
                                find_a = a;
                                find_b = b;
                            }
                        }
                        else
                        {
                            //선분 vs 사각꼴   :  최대거리 찾기 
                            //사각꼴 vs 사각꼴  :  최대거리 찾기 
                            if (max_len < new_len)
                            {
                                max_len = new_len;

                                __min_A_rot = _movingSegment._localRota_A;
                                __min_B_rot = _movingSegment._localRota_B;
                                __dir_move_A = _movingSegment._dir_move_A;
                                __dir_move_B = _movingSegment._dir_move_B;

                                __find_seg_A = _movingSegment._cur_seg_A;
                                __find_seg_B = _movingSegment._cur_seg_B;
                                find_a = a;
                                find_b = b;
                            }
                        }

                        //DebugWide.DrawCircle(_movingSegment._meetPt, 0.05f, Color.red);
                    }
                }
            }//end for


            //=================================================
            //적용 
            if (_update)
            {
                //DebugWide.LogGreen("  find_a: " + find_a + "  find_b: " + find_b + " ------- ");
                if(true == _allowFixed_a)
                {
                    _frame_A._tr_frame.rotation = __min_A_rot * _frame_A._tr_frame.rotation; //실제적용     
                }
                else
                {
                    _frame_A._tr_frame.position += __dir_move_A;
                    //DebugWide.DrawLine(__dir_move_A + _frame_A._tr_frame.position, _frame_A._tr_frame.position, Color.white);
                    //DebugWide.LogBlue("aa  " + __dir_move_A);

                }

                if (true == _allowFixed_b)
                {
                    _frame_B._tr_frame.rotation = __min_B_rot * _frame_B._tr_frame.rotation;    
                }
                else
                {
                    _frame_B._tr_frame.position += __dir_move_B;
                    //DebugWide.DrawLine(__dir_move_B + _frame_B._tr_frame.position, _frame_B._tr_frame.position, Color.white);
                    //DebugWide.LogBlue("bb  " + __dir_move_B);
                }

            }

            _frame_A.Prev_Update();
            _frame_B.Prev_Update();

            return _update;
        }
    }
}
