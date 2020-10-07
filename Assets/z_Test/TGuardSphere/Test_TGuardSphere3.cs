using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using System;


public class Test_TGuardSphere3 : MonoBehaviour 
{
    
    public class TGS_Info
    {
        public bool _init = false;

        public Transform _T0 = null;
        public Transform _T0_root = null;
        public Transform _T0_root_start = null;
        public Transform _T0_root_end = null;
        public Transform _T0_sub_start = null;
        public Transform _T0_sub_end = null;

        public Transform _T1 = null;
        public Transform _T1_root = null;
        public Transform _T1_root_start = null;
        public Transform _T1_root_end = null;
        public Transform _T1_sub_start = null;
        public Transform _T1_sub_end = null;

        public Transform _Tctl = null;
        public Transform _Tctl_root = null;
        public Transform _Tctl_root_start = null;
        public Transform _Tctl_root_end = null;
        public Transform _Tctl_sub_start = null;
        public Transform _Tctl_sub_end = null;


        public Vector3 _mt_min = ConstV.v3_zero;
        public Vector3 _mt_rate = ConstV.v3_zero;
        public Vector3 _mt_max = ConstV.v3_zero;

        public LineSegment3 ToRoot_T0()
        {
            return new LineSegment3(_T0_root_start.position, _T0_root_end.position);
        }
        public LineSegment3 ToRoot_Tctl()
        {
            return new LineSegment3(_Tctl_root_start.position, _Tctl_root_end.position);
        }


        public LineSegment3 ToSeg_T0()
        {
            return new LineSegment3(_T0_sub_start.position, _T0_sub_end.position);
        }
        public LineSegment3 ToSeg_Tctl()
        {
            return new LineSegment3(_Tctl_sub_start.position, _Tctl_sub_end.position);
        }


        public void Draw()
        {
            if (false == _init) return;

            float radius;

            DebugWide.DrawLine(_T0_root_start.position, _T0_sub_start.position, Color.gray);
            radius = Vector3.Magnitude(_T0_root_start.position - _T0_sub_start.position);
            //DebugWide.DrawCircle(_T0_root_start.position, radius, Color.gray);

            //-----------------

            DebugWide.DrawLine(_Tctl_root_start.position, _Tctl_root_end.position, Color.gray);
            DebugWide.DrawLine(_Tctl_sub_start.position, _Tctl_sub_end.position, Color.gray);
            //DebugWide.DrawCircle(_Tctl_root_end.position, 0.05f, Color.gray);

            //-----------------

            DebugWide.DrawLine(_T0_root_start.position, _T0_root_end.position, Color.magenta);
            DebugWide.DrawLine(_T0_sub_start.position, _T0_sub_end.position, Color.magenta);

            //-----------------

            DebugWide.DrawLine(_T1_root_start.position, _T1_root_end.position, Color.blue);
            DebugWide.DrawLine(_T1_sub_start.position, _T1_sub_end.position, Color.blue);

            //-----------------

            //DebugWide.DrawCircle(_mt_min, 0.02f, Color.red);
            //DebugWide.DrawCircle(_mt_rate, 0.04f, Color.red);
            //DebugWide.DrawCircle(_mt_max, 0.06f, Color.red);

            //DebugWide.DrawLine(_seg1_start.position, _mt1, Color.red);
            //DebugWide.DrawLine(_T0_root_start.position, _mt_max, Color.red);
            //DebugWide.DrawLine(_T0_root_start.position, _seg0_start.position, Color.red);

        }

        public void Init(Transform _TGS_root_)
        {
            _init = true;

            Transform temp;
            _T0 = Hierarchy.GetTransform(_TGS_root_, "T_0");
            _T0_root = Hierarchy.GetTransform(_T0, "root");
            temp = Hierarchy.GetTransform(_T0_root, "sub_0");
            _T0_root_start = Hierarchy.GetTransform(_T0_root, "start");
            _T0_root_end = Hierarchy.GetTransform(_T0_root, "end");
            _T0_sub_start = Hierarchy.GetTransform(temp, "start");
            _T0_sub_end = Hierarchy.GetTransform(temp, "end");

            _T1 = Hierarchy.GetTransform(_TGS_root_, "T_1");
            _T1_root = Hierarchy.GetTransform(_T1, "root");
            temp = Hierarchy.GetTransform(_T1_root, "sub_0");
            _T1_root_start = Hierarchy.GetTransform(_T1_root, "start");
            _T1_root_end = Hierarchy.GetTransform(_T1_root, "end");
            _T1_sub_start = Hierarchy.GetTransform(temp, "start");
            _T1_sub_end = Hierarchy.GetTransform(temp, "end");

            _Tctl = Hierarchy.GetTransform(_TGS_root_, "Tctl");
            _Tctl_root = Hierarchy.GetTransform(_Tctl, "root");
            temp = Hierarchy.GetTransform(_Tctl_root, "sub_0");
            _Tctl_root_start = Hierarchy.GetTransform(_Tctl_root, "start");
            _Tctl_root_end = Hierarchy.GetTransform(_Tctl_root, "end");
            _Tctl_sub_start = Hierarchy.GetTransform(temp, "start");
            _Tctl_sub_end = Hierarchy.GetTransform(temp, "end");

            //------------

        }


    }//end class

    TGS_Info _tgs_A = new TGS_Info();
    TGS_Info _tgs_B = new TGS_Info();
    public MovingSegement3 _movTgs = new MovingSegement3();

	// Use this for initialization
	void Start () 
    {
        Transform tgs_root = null;
        tgs_root = Hierarchy.GetTransform(null, "TGS_0");
        _tgs_A.Init(tgs_root);
        tgs_root = Hierarchy.GetTransform(null, "TGS_1");
        _tgs_B.Init(tgs_root);


        //realTime , frameStep init
        __seg_prev_A = _tgs_A.ToSeg_T0();
        __seg_prev_B = _tgs_B.ToSeg_T0();

        //__prev_A_rot = _tgs_A._T0_root.rotation;
        //__prev_B_rot = _tgs_B._T0_root.rotation;

        Vector3 pt_start, pt_end;
        LineSegment3.ClosestPoints(out pt_start, out pt_end, __seg_prev_A, __seg_prev_B);
        _movTgs.__prev_A_B_order = pt_end - pt_start;

        _movTgs._radius_A = __radius_A;
        _movTgs._radius_B = __radius_B;
	}


    //비율값에 따라 angle_1 을 변환한다 
    public float __rate = 1f;
    void Update()
    {
        

    }

    public enum eModeKind
    {
        StopTime,
        RealTime_1,
        RealTime_2,
        FrameStep,
    }


    public float __radius_A = 0.1f;
    public float __radius_B = 0.1f;
    public bool _allowFixed_a = true;
    public bool _allowFixed_b = true;


    public eModeKind __updateMode_stopTime = eModeKind.StopTime;
    private void OnDrawGizmos()
    {
        if (false == _tgs_A._init) return;

        _movTgs._radius_A = __radius_A;
        _movTgs._radius_B = __radius_B;

        //-----------------------------------

        switch (__updateMode_stopTime)
        {
            case eModeKind.StopTime:
                Update_StopTime();
                break;
            case eModeKind.RealTime_1:
                Update_RealTime_1();
                break;
            case eModeKind.RealTime_2:
                Update_RealTime_2();
                break;
            case eModeKind.FrameStep:
                Update_FrameStep();
                break;
        }

        //-----------------------------------

        _tgs_A.Draw();
        _tgs_B.Draw();

        DebugWide.DrawCircle(_movTgs._minV, 0.02f, Color.red);
        DebugWide.DrawCircle(_movTgs._meetPt, 0.04f, Color.red);
        DebugWide.DrawCircle(_movTgs._maxV, 0.06f, Color.red);

        DebugWide.DrawCircle(_movTgs._meetPt_A, _movTgs._radius_A, Color.gray);
        DebugWide.DrawCircle(_movTgs._meetPt_B, _movTgs._radius_B, Color.gray);

        DebugWide.DrawLine(_movTgs._cur_seg_A.origin, _movTgs._cur_seg_A.last, Color.white);
        DebugWide.DrawLine(_movTgs._cur_seg_B.origin, _movTgs._cur_seg_B.last, Color.white);
    }

    public bool __prevFrame = false;
    public bool __nextFrame = false;
    public float __angle = 0.05f;

    //private Quaternion __prev_A_rot = Quaternion.identity;
    //private Quaternion __prev_B_rot = Quaternion.identity;
    //프레임 한단계씩 계산
	public void Update_FrameStep()
	{
        
        if (true == _tgs_A._init)
        {
            __rate = Mathf.Clamp(__rate, 0, 1f);
            _tgs_A._T1_root.rotation = _tgs_A._T0_root.rotation;
            _tgs_B._T1_root.rotation = _tgs_B._T0_root.rotation;
            //_tgs_A._Tctl_root.rotation = _tgs_A._T0_root.rotation;
            //_tgs_B._Tctl_root.rotation = _tgs_B._T0_root.rotation;

            _tgs_A._T1_root.position = _tgs_A._T0_root.position;
            _tgs_B._T1_root.position = _tgs_B._T0_root.position;
            //_tgs_A._Tctl_root.position = _tgs_A._T0_root.position;
            //_tgs_B._Tctl_root.position = _tgs_B._T0_root.position;
            //------------

            //각도후진 시험 
            if (true == __prevFrame)
            {
                __prevFrame = false;
                Vector3 ori = _tgs_A._T0_root.position;
                Vector3 start = _tgs_A._T0_root_end.position;
                Vector3 end = _tgs_A._Tctl_root_end.position;
                Vector3 up_angle = Vector3.Cross(start - ori, end - ori);
                //_tgs_A._T0_root.rotation *= Quaternion.AngleAxis(__angle, -up_angle);
                _tgs_A._T0_root.rotation *= Quaternion.AngleAxis(__angle, -ConstV.v3_right);
            }

            ////각도전진 시험
            if (true == __nextFrame)
            {
                __nextFrame = false;
                Vector3 ori = _tgs_A._T0_root.position;
                Vector3 start = _tgs_A._T0_root_end.position;
                Vector3 end = _tgs_A._Tctl_root_end.position;
                Vector3 up_angle = Vector3.Cross(start - ori, end - ori);
                //_tgs_A._T0_root.rotation *= Quaternion.AngleAxis(__angle, up_angle);
                _tgs_A._T0_root.rotation *= Quaternion.AngleAxis(__angle, ConstV.v3_right);
            }

            //------------

            LineSegment3 prev_A, cur_A;
            LineSegment3 prev_B, cur_B;

            cur_A = _tgs_A.ToSeg_T0();
            cur_B = _tgs_B.ToSeg_T0();
            prev_A = __seg_prev_A;
            prev_B = __seg_prev_B;
            //DebugWide.DrawLine(__seg_prev_A.origin, __seg_prev_A.last, Color.black);
            //DebugWide.DrawLine(__seg_prev_B.origin, __seg_prev_B.last, Color.black);

            _movTgs.Input_TGuard(prev_A, prev_B, cur_A, cur_B);

            //if (true == __nextFrame)
            {
                //__nextFrame = false;


                bool contact = _movTgs.Find_TGuard_vs_TGuard(__rate, _allowFixed_a, _allowFixed_b,
                                                             _tgs_A._T0_root, _tgs_B._T0_root);
                if (true == contact)
                {

                    //_tgs_A._T1_root.rotation = _movTgs.__localRota_A * __prev_A_rot; //실제적용 
                    //_tgs_B._T1_root.rotation = _movTgs.__localRota_B * __prev_B_rot; //실제적용 

                    _tgs_A._T0_root.rotation = _movTgs.__localRota_A * _tgs_A._T0_root.rotation; //실제적용 
                    _tgs_B._T0_root.rotation = _movTgs.__localRota_B * _tgs_B._T0_root.rotation; //실제적용 

                    //이동값 선분에 적용
                    _tgs_A._T0_root.position += _movTgs.__dir_move_A;
                    _tgs_B._T0_root.position += _movTgs.__dir_move_B;

                }

                //__seg_prev_A = _tgs_A.ToSeg_T0();
                //__seg_prev_B = _tgs_B.ToSeg_T0();
                __seg_prev_A = _movTgs._prev_seg_A;
                __seg_prev_B = _movTgs._prev_seg_B;

                //__prev_A_rot = _tgs_A._T0_root.rotation;
                //__prev_B_rot = _tgs_B._T0_root.rotation;

            }


        }
	}


    private LineSegment3 __seg_prev_A = new LineSegment3();
    private LineSegment3 __seg_prev_B = new LineSegment3();

	//실시간 계산 
	private void Update_RealTime_1()
    {

        if (true == _tgs_A._init)
        {
            __rate = Mathf.Clamp(__rate, 0, 1f);
            _tgs_A._T1_root.rotation = _tgs_A._T0_root.rotation;
            _tgs_B._T1_root.rotation = _tgs_B._T0_root.rotation;
            _tgs_A._Tctl_root.rotation = _tgs_A._T0_root.rotation;
            _tgs_B._Tctl_root.rotation = _tgs_B._T0_root.rotation;

            _tgs_A._T1_root.position = _tgs_A._T0_root.position;
            _tgs_B._T1_root.position = _tgs_B._T0_root.position;
            _tgs_A._Tctl_root.position = _tgs_A._T0_root.position;
            _tgs_B._Tctl_root.position = _tgs_B._T0_root.position;
            //------------

            LineSegment3 prev_A, cur_A;
            LineSegment3 prev_B, cur_B;


            cur_A = _tgs_A.ToSeg_T0();
            cur_B = _tgs_B.ToSeg_T0();
            prev_A = __seg_prev_A;
            prev_B = __seg_prev_B;
            //DebugWide.DrawLine(__seg_prev_A.origin, __seg_prev_A.last, Color.black);
            //DebugWide.DrawLine(__seg_prev_B.origin, __seg_prev_B.last, Color.black);

            _movTgs.Input_TGuard(prev_A, prev_B, cur_A, cur_B);

            bool contact = _movTgs.Find_TGuard_vs_TGuard(__rate, _allowFixed_a, _allowFixed_b,
                                                         _tgs_A._T0_root, _tgs_B._T0_root);
            if (true == contact)
            {

                //cur 선분에 적용 
                _tgs_A._T0_root.rotation = _movTgs.__localRota_A * _tgs_A._T0_root.rotation; //실제적용 
                _tgs_B._T0_root.rotation = _movTgs.__localRota_B * _tgs_B._T0_root.rotation; //실제적용 

                //prev 선분에 적용 
                //_tgs_A._T0_root.rotation = _movTgs.__localRota_A * __prev_A_rot; //실제적용 
                //_tgs_B._T0_root.rotation = _movTgs.__localRota_B * __prev_B_rot; //실제적용 

                //이동값 선분에 적용
                _tgs_A._T0_root.position += _movTgs.__dir_move_A;
                _tgs_B._T0_root.position += _movTgs.__dir_move_B;

            }


            //__seg_prev_A = _tgs_A.ToSeg_T0();
            //__seg_prev_B = _tgs_B.ToSeg_T0();
            __seg_prev_A = _movTgs._prev_seg_A;
            __seg_prev_B = _movTgs._prev_seg_B;

            //__prev_A_rot = _tgs_A._T0_root.rotation;
            //__prev_B_rot = _tgs_B._T0_root.rotation;

            //------------

        }

    }

    private void Update_RealTime_2()
    {

        if (true == _tgs_A._init)
        {
            __rate = Mathf.Clamp(__rate, 0, 1f);
            _tgs_A._T1_root.rotation = _tgs_A._T0_root.rotation;
            _tgs_B._T1_root.rotation = _tgs_B._T0_root.rotation;

            _tgs_A._T1_root.position = _tgs_A._T0_root.position;
            _tgs_B._T1_root.position = _tgs_B._T0_root.position;
            //------------

            LineSegment3 prev_A, cur_A;
            LineSegment3 prev_B, cur_B;


            prev_A = _tgs_A.ToSeg_T0();
            prev_B = _tgs_B.ToSeg_T0();
            cur_A = _tgs_A.ToSeg_Tctl();
            cur_B = _tgs_B.ToSeg_Tctl();


            _movTgs.Input_TGuard(prev_A, prev_B, cur_A, cur_B);

            bool contact = _movTgs.Find_TGuard_vs_TGuard(__rate, _allowFixed_a, _allowFixed_b,
                                                         _tgs_A._T0_root, _tgs_B._T0_root);
            if (true == contact)
            {

                _tgs_A._T0_root.rotation = _movTgs.__localRota_A * _tgs_A._Tctl_root.rotation; //실제적용 
                _tgs_B._T0_root.rotation = _movTgs.__localRota_B * _tgs_B._Tctl_root.rotation; //실제적용 

                _tgs_A._Tctl_root.rotation = _tgs_A._T0_root.rotation;
                _tgs_B._Tctl_root.rotation = _tgs_B._T0_root.rotation;

                //이동값 선분에 적용
                _tgs_A._T0_root.position += _movTgs.__dir_move_A;
                _tgs_B._T0_root.position += _movTgs.__dir_move_B;
                _tgs_A._Tctl_root.position = _tgs_A._T0_root.position;
                _tgs_B._Tctl_root.position = _tgs_B._T0_root.position;
            }


            //------------

            //_tgs_A.Draw();
            //_tgs_B.Draw();

        }

    }


    //정지 계산 
    private void Update_StopTime()
    {
        
        if (true == _tgs_A._init)
        {
            __rate = Mathf.Clamp(__rate, 0, 1f);
            _tgs_A._T1_root.rotation = _tgs_A._T0_root.rotation;
            _tgs_B._T1_root.rotation = _tgs_B._T0_root.rotation;

            _tgs_A._T1_root.position = _tgs_A._T0_root.position;
            _tgs_B._T1_root.position = _tgs_B._T0_root.position;
            //------------

            LineSegment3 prev_A, cur_A;
            LineSegment3 prev_B, cur_B;

            //prev_A = _tgs0.ToRoot_Prev();
            //cur_A = _tgs0.ToRoot_Cur();
            //prev_B = _tgs1.ToRoot_Prev();
            //cur_B = _tgs1.ToRoot_Cur();
            //Vector3 dirMeet = prev_A.direction +  prev_B.direction + cur_A.direction + cur_B.direction;


            prev_A = _tgs_A.ToSeg_T0();
            cur_A = _tgs_A.ToSeg_Tctl();
            prev_B = _tgs_B.ToSeg_T0();
            cur_B = _tgs_B.ToSeg_Tctl();

            _movTgs.Input_TGuard(prev_A, prev_B, cur_A, cur_B);


            bool contact = _movTgs.Find_TGuard_vs_TGuard(__rate, _allowFixed_a, _allowFixed_b,
                                                         _tgs_A._T0_root, _tgs_B._T0_root);
            if(true == contact)
            {
                
                //_tgs_A._T1_root.rotation = _movTgs.__localRota_A * _tgs_A._T0_root.rotation; //실제적용 
                //_tgs_B._T1_root.rotation = _movTgs.__localRota_B * _tgs_B._T0_root.rotation; //실제적용 

                _tgs_A._T1_root.rotation = _movTgs.__localRota_A * _tgs_A._Tctl_root.rotation; //실제적용 
                _tgs_B._T1_root.rotation = _movTgs.__localRota_B * _tgs_B._Tctl_root.rotation; //실제적용 

                //이동값 선분에 적용
                _tgs_A._T1_root.position += _movTgs.__dir_move_A;
                _tgs_B._T1_root.position += _movTgs.__dir_move_B;

            }


        }

    }

    //==========


}
