using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace HordeFight
{
    //가지 , 팔다리 
    public class Limbs : MonoBehaviour
    {

        //public class Control
        //{
        //}

        //public class PathCircle
        //{
        //}


        public enum ePart
        {
            None,
            OneHand,
            TwoHand,
        }

        public enum eStandard
        {
            TwoHand_LeftO, //왼손기준이 되는 잡기(왼손이 고정)
            TwoHand_RightO, //오른손기준이 되는 잡기(오른손이 고정)
        }

        public enum eStance
        {
            None = 0,

            Cut,    //베기
            Sting,  //찌르기
            Block,  //막기
            Etc,    //그외

            Max,
        }

        public enum eState
        {
            None = 0,

            Start,
            Running,
            Waiting,
            End,

            Max,
        }


        //------------------------------------------------------

        public Being _ref_being = null;
        public Movement _ref_movement = null;

        //------------------------------------------------------

        //todo : 그림자 설정은 나중에 따로 빼기 
        public Vector3 _light_dir; //방향성 빛
        public Vector3 _groundY; //땅표면 

        //------------------------------------------------------

        public Vector3 _sight_dir = UtilGS9.ConstV.v3_zero;
        public Vector3 _upperBody_dir = UtilGS9.ConstV.v3_zero;
        public Vector3 _foot_dir = UtilGS9.ConstV.v3_zero;

        private Vector3 _hand_left_dirPos = ConstV.v3_zero; //한손으로 쥐고 있었을 때의 물체의 방향.
        private Vector3 _hand_right_dirPos = ConstV.v3_zero;
        //------------------------------------------------------

        private Transform _tr_sight_dir = null;
        private Transform _tr_upperBody_dir = null;
        private Transform _tr_foot_dir = null;

        private Transform _tr_root = null;
        private Transform _tr_head = null;
        private Transform _tr_waist = null; //허리

        private Transform _tr_shoulder_left = null;
        public Transform _tr_hand_left = null;
        private Transform _tr_hand_left_wrist = null; //손목
        private Transform _tr_arm_left_up = null;
        //private Transform _tr_arm_left_dir = null; //한손으로 쥐고 있었을 떄의 물체의 방향.

        //private Transform _arm_left = null;
        //private Transform _arm_left_start = null;
        //private Transform _arm_left_end = null;
        //private Transform _arm_left_shader = null;
        //private SpriteMesh _arm_left_spr = null;


        private Transform _tr_shoulder_right = null;
        public Transform _tr_hand_right = null;
        private Transform _tr_hand_right_wrist = null;
        private Transform _tr_arm_right_up = null;
        //private Transform _tr_arm_right_dir = null;

        //private Transform _arm_right = null;
        //private Transform _arm_right_start = null;
        //private Transform _arm_right_end = null;
        //private Transform _arm_right_shader = null;
        //private SpriteMesh _arm_right_spr = null;


        //핸들 
        private Transform _HANDLE_twoHand = null;
        private Transform _HANDLE_oneHand = null;
        private Transform _HANDLE_left = null;   //핸들
        private Transform _HANDLE_right = null;

        //찌르기용 핸들 
        public Transform _hs_objectDir = null;
        public Transform _hs_standard = null;
        public Transform _hs_ani_start = null;

        //목표
        private Transform[] _target = new Transform[2]; //임시 최대2개 목표 
        //private Vector3 _view_target_0 = ConstV.v3_zero;
        //private Vector3 _view_target_1 = ConstV.v3_zero;

        public float _shoulder_length = 0f;
        public float _arm_left_length = 0.5f;
        public float _arm_left_min_length = 0.2f;
        public float _arm_left_max_length = 1f;
        public float _arm_right_length = 0.7f;
        public float _arm_right_min_length = 0.2f;
        public float _arm_right_max_length = 1f;
        //public float _twoHand_basic_length = 0.2f; //양손간격 기본길이 , 길이 복원시에 사용 
        public float _twoHand_cut_length = 0.2f;
        public float _twoHand_min_length = 0.1f;
        public float _twoHand_max_length = 0.5f;


        public ePart _part_control = ePart.TwoHand; //조종부위 <한손 , 양손 , 한다리 , 꼬리 등등>
        private ePart _part_control_prev = ePart.TwoHand;
        public eStandard _eHandStandard = eStandard.TwoHand_LeftO; //고정으로 잡는 손지정(부위지정)  
        private eStandard _eHandStandard_prev = eStandard.TwoHand_LeftO;
        public eStance _eStance_hand_left = eStance.Sting; //자세
        public eStance _eStance_hand_right = eStance.Sting; //자세

        //무기정보
        private List<ArmedInfo> _list_armedInfo = new List<ArmedInfo>();
        public ArmedInfo.eIdx _equipment_handLeft = ArmedInfo.eIdx.Federschwert;
        public ArmedInfo.eIdx _equipment_handRight = ArmedInfo.eIdx.None;
        public ArmedInfo _armed_left = null;
        public ArmedInfo _armed_right = null;
        public bool _isUpdateEq_handLeft = false;
        public bool _isUpdateEq_handRight = false;

        public bool _active_shadow = true;

        public bool _active_projectionSlope = true;
        public float _boxSlope_angle = 75f;


        //======================================================



        //왼손 조종용 경로원
        private ControlModel _ctrm_one_A0 = new ControlModel();//왼손
        private ControlModel _ctrm_one_A1 = new ControlModel();//왼손 손잡이
        //오른손 조종용 경로 
        private ControlModel _ctrm_one_B0 = new ControlModel();//오른손
        private ControlModel _ctrm_one_B1 = new ControlModel();//오른손 손잡이

        //양손 조종용 경로원
        private ControlModel _ctrm_two_A0 = new ControlModel();//양손 - 첫번째 기준
        private ControlModel _ctrm_two_B0 = new ControlModel();//양손 - 두번쨰 기준 


        //======================================================
        //ani 정보 

        private Transform _shoul_left_start;
        private Transform _shoul_left_end;
        private Transform _shoul_right_start;
        private Transform _shoul_right_end;
        private Transform _upperBody_start;
        private Transform _upperBody_end;
        private Transform _foot_start;
        private Transform _foot_end;
        private Transform _foot_movePos;


        //======================================================

        [Header("____GIZMO____")]
        [Space]
        public bool _show_gizmos = true;
        public bool _show_bagic = false;
        public bool _show_armed = false;
        public bool _show_control = false;
        public bool _show_shadow = false;

        //======================================================
        [Space]

        //ref : https://mentum.tistory.com/223
        [Header("____ANI____")]
        [Space]
        public Ani _ani_hand_left = new Ani();
        [Space]
        public Ani _ani_hand_right = new Ani();
        [Space]
        public Ani _ani_upperBody = new Ani();
        [Space]
        public Ani _ani_foot_move = new Ani();
        [Space]
        public Ani _ani_foot_rotate = new Ani();
        //======================================================

        public class ControlModel
        {
            public Transform tr = null;
            public Transform tr_up = null;
            public Transform tr_highest = null;
            public Transform tr_tornado_unlace = null;
            public MotionTrajectory motion = null;

            public Geo.Model model = new Geo.Model();

            public void SetModel()
            {
                SetModel(tr_up.position - tr.position);
            }
            public void SetModel(Vector3 upDir)
            {
                model.kind = motion._eModel;

                model.SetModel(upDir, tr.position, motion._radius, tr_highest.position,
                                     motion._far_radius, motion._tornado_angle, tr_tornado_unlace.localPosition);

            }

            public void Load(Transform parent, string ctr_name)
            {
                tr = SingleO.hierarchy.GetTransform(parent, ctr_name);
                motion = tr.GetComponent<MotionTrajectory>();
                tr_up = SingleO.hierarchy.GetTransform(tr, "up");
                tr_highest = SingleO.hierarchy.GetTransform(tr, "highest");
                tr_tornado_unlace = SingleO.hierarchy.GetTransform(tr_highest, "tornado_unlace");
            }

            public void Draw(Color color, Vector3 handle_pos)
            {
                model.Draw(color);

                DebugWide.DrawLine(tr.position, handle_pos, color);

            }

        }


        [System.Serializable]
        public class Ani
        {
            public delegate void CallFunc(Ani ani);
            public CallFunc _Call_Func = null;

            private float _elapsedTime = 0f;
            private float _aniProgDir = 1f;
            private float _entire_aniTime = 0f;

            public eState _state_current = eState.Start;
            public bool _active_ani = false; //재생 활성
            public bool _active_backAni = false; //역재생 활성 
            public bool _active_loopAni = false;

            public float _aniTime_SE = 1f; //스탠스 앞으로 재생시간 1초
            public float _aniTime_ES = 0.7f; //스탠스 뒤로 재생시간
            public float _stiffTime_E = 0.1f; //end 도달후 경직시간
            public int _rotate_count = 0;
            public float _amplitude_punch = 1f; //펀치 애니 진폭

            public Interpolation.eKind _ani_interpolation = Interpolation.eKind.easeInOutSine;

            public float _cur_inpol = 0f;
            public float _prev_inpol = 0f;

            public void Update()
            {
                if (false == _active_ani) 
                {
                    //_state_current = eState.Start;
                    return;   
                }

                switch (this._state_current)
                {
                    case eState.Start:
                        {
                            //기본애니정보 초기화 
                            _aniProgDir = 1f;
                            _elapsedTime = 0f;
                            _entire_aniTime = _aniTime_SE;

                            _cur_inpol = 0f;
                            _prev_inpol = 0f;

                            //다음 상태로 변경
                            _state_current = eState.Running;
                        }
                        break;
                    case eState.Running:
                        {
                            Running_Ani();
                        }
                        break;
                    case eState.End:
                        {
                            
                            if (_active_loopAni)
                                _state_current = eState.Start;
                        }
                        break;
                }
            }

            public void Running_Ani()
            {

                _elapsedTime += Time.deltaTime * _aniProgDir;
                float curTime = _elapsedTime;
                //DebugWide.LogBlue(__aniProgDir + "  " + curTime);

                //정방향 재생 , 동작시간 경과
                if (0 < _aniProgDir && _elapsedTime > _aniTime_SE)
                {
                    curTime = _aniTime_SE;

                    //경직시간 경과  
                    if (_elapsedTime > _aniTime_SE + _stiffTime_E)
                    {

                        if (true == _active_backAni)
                        {
                            //재생방향 변경 
                            _aniProgDir = -1f;
                            _elapsedTime = _aniTime_ES;
                            curTime = _aniTime_ES;
                            _entire_aniTime = _aniTime_ES;
                        }
                        else
                        {
                            _state_current = eState.End;
                        }
                    }
                }
                //역방향 재생 , 동작시간 경과 
                if (0 > _aniProgDir && _elapsedTime < 0)
                {
                    _state_current = eState.End;
                }


                //if (_active_ani)
                {

                    float t = 0f;
                    if (float.Epsilon > _entire_aniTime)
                        t = 0f;
                    else
                        t = curTime / _entire_aniTime;

                    if (Interpolation.eKind.punch == _ani_interpolation)
                    {
                        _cur_inpol = Interpolation.Calc(_ani_interpolation, 0, _amplitude_punch, t);
                        _active_backAni = false;
                    }
                    else
                    {
                        _cur_inpol = Interpolation.Calc(_ani_interpolation, 0, 1f, t);
                    }

                    if(null != _Call_Func)
                        _Call_Func(this);
                }

                _prev_inpol = _cur_inpol;

            }//end func
        }


        public void Call_UpperBody_Rotate(Ani ani)
        {
            //상체회전 애니 
            //if (ani._active_ani)
            {
                UpperBody_RotateAni(ani._cur_inpol);
            }
            //else
            {
                //상체회전 비활성시 기본값으로 초기화 한다 
                //Vector3 temp = _tr_waist.localEulerAngles;
                //temp.y = 0;
                //_tr_waist.localEulerAngles = temp;
            }

        }

        public void Call_Foot_Move(Ani ani)
        {
            //다리이동 애니 

            if (0 > ani._cur_inpol) ani._cur_inpol = 0;
            else if (ani._cur_inpol > 1f) ani._cur_inpol = 1f;

            float tt_delta = ani._cur_inpol - ani._prev_inpol; //0~1 값을 프레임차이값으로 변환한다 


            Vector3 move_dir = _foot_movePos.position - transform.position;
            _ref_being.SetPos(_ref_being.GetPos3D() + move_dir * tt_delta);

        }

        public void Call_Foot_Rotate(Ani ani)
        {
            if (0 > ani._cur_inpol) ani._cur_inpol = 0;
            else if (ani._cur_inpol > 1f) ani._cur_inpol = 1f;

            float tt_delta = ani._cur_inpol - ani._prev_inpol; //0~1 값을 프레임차이값으로 변환한다 

            //감기는 방향 
            Vector3 winding = Vector3.Cross(_foot_start.position - transform.position, _foot_dir);

            //전체각도 
            float entireAngle = Geo.Angle360_AxisRotate_Normal_Axis(_foot_start.position - transform.position, _foot_end.position - transform.position, winding);
            entireAngle = entireAngle + (360f * ani._rotate_count);

            Vector3 dir = Quaternion.AngleAxis(entireAngle * tt_delta, winding) * _foot_dir;
            _ref_movement.SetDirection(dir);
            _ref_being.UpdateIdle();

        }

        public void Call_Handle_TwoHand_LeftO_Cut(Ani ani)
        {
            //if (_part_control == ePart.TwoHand)
            {
                
                {
                    //구면 보간
                    Vector3 arcUp = Vector3.Cross(_shoul_left_start.position - transform.position, _foot_dir);
                    _HANDLE_twoHand.position = InterpolationTornado(transform.position, _shoul_left_start.position, _shoul_left_end.position, arcUp, ani._rotate_count, ani._cur_inpol);
                }
            }
        }

        public void Call_Handle_TwoHand_Sting(Ani ani)
        {
            //if (_part_control == ePart.TwoHand)
            {
                
                {
                    Vector3 dir_target = _hs_objectDir.position - _hs_ani_start.position;
                    Vector3 n_dir_target = VOp.Normalize(dir_target);
                    Vector3 start = _hs_ani_start.position;
                    Vector3 end = _hs_objectDir.position - n_dir_target * _twoHand_min_length;
                    _hs_standard.position = start + (end-start) * ani._cur_inpol;
                }
            }
        }

        public void Call_Handle_TwoHand_RightO_Cut(Ani ani)
        {
            //if (_part_control == ePart.TwoHand)
            {

                {
                    //구면 보간
                    Vector3 arcUp = Vector3.Cross(_shoul_right_start.position - transform.position, _foot_dir);
                    _HANDLE_twoHand.position = InterpolationTornado(transform.position, _shoul_right_start.position, _shoul_right_end.position, arcUp, ani._rotate_count, ani._cur_inpol);
                }
            }
        }


        public void Call_Handle_RightHand_Sting(Ani ani)
        {
            //if (_part_control == ePart.OneHand)
            {
                
                {
                    //if (0 > ani._cur_inpol) ani._cur_inpol = 0;
                    //else if (ani._cur_inpol > 1f) ani._cur_inpol = 1f;

                    Vector3 dir_target = _target[1].position - _tr_shoulder_right.position;
                    dir_target = VOp.Normalize(dir_target);
                    Vector3 start = _tr_shoulder_right.position + dir_target * _arm_right_min_length;
                    Vector3 end = start + dir_target * (_arm_right_max_length-_arm_right_min_length) * ani._cur_inpol;
                    _HANDLE_right.position = end;

                }
            }
        }

        public void Call_Handle_LeftHand_Sting(Ani ani)
        {
            //if (_part_control == ePart.OneHand)
            {
                
                {
                    //if (0 > ani._cur_inpol) ani._cur_inpol = 0;
                    //else if (ani._cur_inpol > 1f) ani._cur_inpol = 1f;

                    Vector3 dir_target = _target[0].position - _tr_shoulder_left.position;
                    dir_target = VOp.Normalize(dir_target);
                    Vector3 start = _tr_shoulder_left.position + dir_target * _arm_left_min_length;
                    Vector3 end = start + dir_target * (_arm_left_max_length - _arm_left_min_length) * ani._cur_inpol;
                    _HANDLE_left.position = end;

                }
            }
        }

        public void Call_Handle_LeftHand_Cut(Ani ani)
        {
            //if (_part_control == ePart.OneHand)
            {
                {
                    if (0 > ani._cur_inpol) ani._cur_inpol = 0;
                    else if (ani._cur_inpol > 1f) ani._cur_inpol = 1f;

                    Vector3 arcUp = Vector3.Cross(_shoul_left_start.position - transform.position, _foot_dir);
                    _HANDLE_left.position = InterpolationTornado(transform.position, _shoul_left_start.position, _shoul_left_end.position, arcUp, ani._rotate_count, ani._cur_inpol);
                }
            }
        }

        public void Call_Handle_RightHand_Cut(Ani ani)
        {
            //if (_part_control == ePart.OneHand)
            {
                {
                    if (0 > ani._cur_inpol) ani._cur_inpol = 0;
                    else if (ani._cur_inpol > 1f) ani._cur_inpol = 1f;

                    Vector3 arcUp = Vector3.Cross(_shoul_right_start.position - transform.position, _foot_dir);
                    _HANDLE_right.position = InterpolationTornado(transform.position, _shoul_right_start.position, _shoul_right_end.position, arcUp, ani._rotate_count, ani._cur_inpol);
                }
            }
        }

        //==================================================

        private void OnDrawGizmos()
        {
            if (false == _show_gizmos) return;
            if (null == _ref_being) return;
            //------------------------------------------------

            //_armed_left.Draw(Color.blue);
            //_armed_right.Draw(Color.magenta);

            //기본 손정보  출력 
            if(_show_bagic)
            {
                Vector3 sLsR = _tr_shoulder_right.position - _tr_shoulder_left.position;
                Vector3 hLsL = _tr_hand_left.position - _tr_shoulder_left.position;
                Vector3 hRsR = _tr_hand_right.position - _tr_shoulder_right.position;
                Vector3 hLhR = _tr_hand_left.position - _tr_hand_right.position;

                DebugWide.PrintText(_tr_shoulder_left.position + hLsL * 0.5f, Color.white, "armL " + _arm_left_length.ToString("00.00"));
                DebugWide.PrintText(_tr_shoulder_right.position + hRsR * 0.5f, Color.white, "armR " + _arm_right_length.ToString("00.00"));
                DebugWide.PrintText(_tr_shoulder_left.position + sLsR * 0.5f, Color.white, "shoulder " + _shoulder_length.ToString("00.00"));
                DebugWide.PrintText(_tr_hand_right.position + hLhR * 0.5f, Color.white, "twoH " + hLhR.magnitude.ToString("00.00"));


                DebugWide.DrawLine(_tr_shoulder_left.position, _tr_hand_left.position, Color.green);
                DebugWide.DrawCircle(_tr_hand_left.position, 0.05f, Color.green);
                DebugWide.DrawLine(_tr_shoulder_right.position, _tr_hand_right.position, Color.green);
                DebugWide.DrawCircle(_tr_hand_right.position, 0.05f, Color.green);
                DebugWide.DrawLine(_tr_hand_right.position, _tr_hand_left.position, Color.black);

                DebugWide.DrawCircle(_tr_shoulder_left.position, _arm_left_min_length, Color.gray);
                DebugWide.DrawCircle(_tr_shoulder_left.position, _arm_left_length, Color.gray);
                DebugWide.DrawCircle(_tr_shoulder_right.position, _arm_right_min_length, Color.gray);
                DebugWide.DrawCircle(_tr_shoulder_right.position, _arm_right_length, Color.gray);

            }

            if (_show_armed)
            //무기정보
            {
                _armed_left.Draw(Color.blue);
                _armed_right.Draw(Color.magenta);

                DebugWide.DrawLine(_tr_hand_left.position, _tr_arm_left_up.position, Color.blue);
                DebugWide.DrawLine(_tr_hand_right.position, _tr_arm_right_up.position, Color.magenta);

                //무기 뒷면 
                if (false)
                {
                    //DebugWide.DrawLine(_hand_left_obj.position, _arm_left_end.position, Color.cyan);
                    //DebugWide.DrawLine(_hand_left_obj.position, _arm_left_up.position, Color.cyan);
                    //DebugWide.DrawCircle(_arm_left_up.position, 0.1f, Color.cyan);

                    //Vector3 hLhR = _hand_right.position - _hand_left.position;
                    //Vector3 obj_shaft = Vector3.Cross(Vector3.forward, hLhR);
                    //float angleW = Vector3.SignedAngle(Vector3.forward, hLhR, obj_shaft);
                    //DebugWide.DrawLine(_arm_left.position, _arm_left.position + obj_shaft * 3f, Color.red);
                    //DebugWide.PrintText(_arm_left.position + obj_shaft * 3f, Color.red, angleW + "");
                }
            }



            //조종정보
            //if(false)
            {
                if (_ani_hand_left._active_ani)
                {
                    DebugWide.DrawCircle(_shoul_left_end.position, 0.5f, Color.black);
                    DebugWide.DrawLine(_shoul_left_start.position, _shoul_left_end.position, Color.black);

                    Vector3 up = Vector3.Cross(_shoul_left_start.position - transform.position, _foot_dir);
                    DebugWide.DrawArc(transform.position, _shoul_left_start.position, _shoul_left_end.position, up, Color.red);    

                }

                if (_ani_hand_right._active_ani)
                {
                    DebugWide.DrawCircle(_shoul_right_end.position, 0.5f, Color.black);
                    DebugWide.DrawLine(_shoul_right_start.position, _shoul_right_end.position, Color.black);

                    Vector3 up = Vector3.Cross(_shoul_right_start.position - transform.position, _foot_dir);
                    DebugWide.DrawArc(transform.position, _shoul_right_start.position, _shoul_right_end.position, up, Color.red);

                }

                if(_ani_upperBody._active_ani)
                {
                    DebugWide.DrawArc(transform.position, _upperBody_start.position, _upperBody_end.position, ConstV.v3_up, 4f, Color.blue, "upperBody");    
                }

                if (_ani_foot_move._active_ani)
                {
                    //다리이동
                    DebugWide.DrawLine(transform.position, _foot_movePos.position, Color.red);
                    DebugWide.DrawCircle(_foot_movePos.position, 0.1f, Color.red);
                }
                if (_ani_foot_rotate._active_ani)
                {
                    //다리회전
                    Vector3 winding = Vector3.Cross( _foot_start.position - transform.position, _foot_dir);
                    DebugWide.DrawArc(transform.position, _foot_start.position, _foot_end.position, winding, 3f, Color.red, "foot");

                }

            }

            if (true == _show_shadow)
            {
                //무기 그림자 표현 
                DebugWide.DrawLine(Vector3.zero, _light_dir, Color.black);
                DebugWide.DrawCircle(_light_dir, 0.1f, Color.black);
                DebugWide.DrawLine(_groundY + Vector3.forward * 5, _groundY, Color.black);


                if(ePart.TwoHand == _part_control)
                {
                    ArmedInfo armed_ori = null;
                    Vector3 objDir = ConstV.v3_zero;
                    Vector3 hand_ori_pos = ConstV.v3_zero;

                    if (eStandard.TwoHand_LeftO == _eHandStandard)
                    {
                        armed_ori = _armed_left;
                        objDir = _tr_hand_right.position - _tr_hand_left.position;
                        hand_ori_pos = _tr_hand_left.position;
                    }    
                    else if (eStandard.TwoHand_RightO == _eHandStandard)
                    {
                        armed_ori = _armed_right;
                        objDir = _tr_hand_left.position - _tr_hand_right.position;
                        hand_ori_pos = _tr_hand_right.position;
                    }    

                    Vector3 arm_ori_end = armed_ori._tr_frame_end.position;

                    float len_groundToObj_start, len_groundToObj_end;
                    Vector3 shaderStart = this.CalcShadowPos(_light_dir, objDir, _groundY, hand_ori_pos, out len_groundToObj_start);
                    Vector3 shaderEnd = this.CalcShadowPos(_light_dir, objDir, _groundY, arm_ori_end, out len_groundToObj_end);
                    //--

                    DebugWide.DrawLine(hand_ori_pos, hand_ori_pos + len_groundToObj_start * Vector3.down, Color.black);
                    DebugWide.DrawLine(arm_ori_end, arm_ori_end + len_groundToObj_end * Vector3.down, Color.black);
                    DebugWide.DrawLine(arm_ori_end, shaderStart, Color.black);
                    DebugWide.DrawLine(arm_ori_end, shaderEnd, Color.black);
                    DebugWide.DrawLine(shaderEnd, shaderStart, Color.red); //그림자 놓여질 위치 표현 
                }
                    
            }


            //손방향 조종
            if(_show_control)
            {
                if (_part_control == ePart.OneHand)
                {
                    //찌르기 
                    if (eStance.Sting == _eStance_hand_left)
                    {
                        DebugWide.DrawLine(_HANDLE_left.position, _target[0].position, Color.red);
                        DebugWide.DrawCircle(_HANDLE_left.position, 0.05f, Color.green);
                    }
                    //찌르기 
                    if (eStance.Sting == _eStance_hand_right)
                    {
                        DebugWide.DrawLine(_HANDLE_right.position, _target[1].position, Color.red);
                        DebugWide.DrawCircle(_HANDLE_right.position, 0.05f, Color.green);
                    }

                    //DebugWide.DrawLine(_HANDLE_oneHand.position, _target[0].position, Color.magenta);
                    //DebugWide.DrawLine(_HANDLE_oneHand.position, _target[1].position, Color.magenta);

                    if (eStance.Cut == _eStance_hand_left)
                    {
                        
                        _ctrm_one_A0.Draw(Color.yellow, _HANDLE_left.position);
                        _ctrm_one_A1.Draw(Color.yellow, _HANDLE_left.position);

                        DebugWide.DrawCircle(_hand_left_dirPos, 0.05f, Color.yellow);
                    }

                    if (eStance.Cut == _eStance_hand_right)
                    {
                        _ctrm_one_B0.Draw(Color.blue, _HANDLE_right.position);
                        _ctrm_one_B1.Draw(Color.blue, _HANDLE_right.position);

                        DebugWide.DrawCircle(_hand_right_dirPos, 0.05f, Color.blue);
                    }

                }

                if (_part_control == ePart.TwoHand)
                {
                    if(eStance.Sting == _eStance_hand_left || eStance.Sting == _eStance_hand_right)
                    {
                        DebugWide.DrawLine(_hs_standard.position, _hs_objectDir.position, Color.white);
                        DebugWide.DrawCircle(_hs_objectDir.position, 0.05f, Color.white);    
                    }

                    if (eStance.Cut == _eStance_hand_left || eStance.Cut == _eStance_hand_right)
                    {
                        
                        //설정된 모델 그리기 
                        _ctrm_two_A0.Draw(Color.yellow, _HANDLE_twoHand.position);
                        _ctrm_two_B0.Draw(Color.blue, _HANDLE_twoHand.position);
                    }

                }

            }


        }//end func



        static public GameObject CreatePrefab(string prefabPath, Transform parent, string name)
        {
            const string root = "Warcraft/Prefab/3_limbs/";
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(root + prefabPath)) as GameObject;
            obj.transform.SetParent(parent, false);
            obj.transform.name = name;


            return obj;
        }

        static public Limbs CreateLimbs(Transform parent)
        {
            
            GameObject limbPrefab = Limbs.CreatePrefab("limbs", parent, "limbs");
            //Limbs limbs = limbPrefab.AddComponent<Limbs>();
            Limbs limbs = limbPrefab.GetComponent<Limbs>();

            return limbs;
        }


        public void Load_RootBone()
        {

            //==================================================
            //1차 자식 : root
            //==================================================
            _tr_root = SingleO.hierarchy.GetTransform(transform.parent, "root");
            //root->foot_dir
            _tr_foot_dir = SingleO.hierarchy.GetTransform(_tr_root, "foot_dir");
            _tr_head = SingleO.hierarchy.GetTransform(_tr_root, "head");
            _tr_sight_dir = SingleO.hierarchy.GetTransform(_tr_head, "sight_dir");
            //root->waist
            _tr_waist = SingleO.hierarchy.GetTransform(_tr_root, "waist");
            _tr_upperBody_dir = SingleO.hierarchy.GetTransform(_tr_waist, "upperBody_dir");


            //-------------------------
            _tr_shoulder_left = SingleO.hierarchy.GetTransform(_tr_waist, "shoulder_left");
            _tr_hand_left = SingleO.hierarchy.GetTransform(_tr_shoulder_left, "hand_left");
            _tr_hand_left_wrist = SingleO.hierarchy.GetTransform(_tr_hand_left, "wrist");
            _tr_arm_left_up = SingleO.hierarchy.GetTransform(_tr_hand_left, "arm_up");
            //_tr_arm_left_dir = SingleO.hierarchy.GetTransform(_tr_hand_left, "arm_dir");


            //_arm_left = SingleO.hierarchy.GetTransform(_hand_left, "arm");
            //_arm_left_start = SingleO.hierarchy.GetTransform(_arm_left, "start");
            //_arm_left_end = SingleO.hierarchy.GetTransform(_arm_left, "end");
            //_arm_left_shader = SingleO.hierarchy.GetTransform(_arm_left, "shader");
            //if (null != (object)_arm_left)
            //{
            //    _arm_left_spr = _arm_left.GetComponentInChildren<SpriteMesh>();
            //}


            //-------------------------

            _tr_shoulder_right = SingleO.hierarchy.GetTransform(_tr_waist, "shoulder_right");
            _tr_hand_right = SingleO.hierarchy.GetTransform(_tr_shoulder_right, "hand_right");
            _tr_hand_right_wrist = SingleO.hierarchy.GetTransform(_tr_hand_right, "wrist");
            _tr_arm_right_up = SingleO.hierarchy.GetTransform(_tr_hand_right, "arm_up");
            //_tr_arm_right_dir = SingleO.hierarchy.GetTransform(_tr_hand_right, "arm_dir");

            //_arm_right = SingleO.hierarchy.GetTransform(_hand_right, "arm");
            //_arm_right_start = SingleO.hierarchy.GetTransform(_arm_right, "start");
            //_arm_right_end = SingleO.hierarchy.GetTransform(_arm_right, "end");
            //_arm_right_shader = SingleO.hierarchy.GetTransform(_arm_right, "shader");
            //if(null != (object)_arm_right)
            //{
            //    _arm_right_spr = _arm_right.GetComponentInChildren<SpriteMesh>();    
            //}


            //-------------------------

        }

        public void Init()
        {

            Load_RootBone();

            //--------------------------------------------------

            //_Model_A0.branch = Geo.Model.eBranch.arm_left_0;
            //_Model_A1.branch = Geo.Model.eBranch.arm_left_0;
            //_Model_B0.branch = Geo.Model.eBranch.arm_right_0;
            //_Model_B1.branch = Geo.Model.eBranch.arm_right_0;


            //==================================================
            //1차 자식 : arms
            //==================================================
            Transform arms = SingleO.hierarchy.GetTransform(transform, "arms");

            foreach(ArmedInfo arin in arms.GetComponentsInChildren<ArmedInfo>(true))
            {
                arin.Init(arin.transform);
                _list_armedInfo.Add(arin);    
            }


            //==================================================
            //1차 자식 : ctr
            //==================================================
            Transform ctr = SingleO.hierarchy.GetTransform(transform, "ctr");
            //ctr->handle_twoHand
            _HANDLE_twoHand = SingleO.hierarchy.GetTransform(ctr, "handle_twoHand");
            //-------------------------
            //손조종 찌르기 
            _hs_objectDir = SingleO.hierarchy.GetTransform(_HANDLE_twoHand, "hs_objectDir");
            _hs_standard = SingleO.hierarchy.GetTransform(_HANDLE_twoHand, "hs_standard");
            _hs_ani_start = SingleO.hierarchy.GetTransform(_HANDLE_twoHand, "hs_ani_start");
            //-------------------------
            //ctr->handle_oneHand
            _HANDLE_oneHand = SingleO.hierarchy.GetTransform(ctr, "handle_oneHand");
            _target[0] = SingleO.hierarchy.GetTransform(_HANDLE_oneHand, "target_1");
            _target[1] = SingleO.hierarchy.GetTransform(_HANDLE_oneHand, "target_2");
            //-------------------------
            _HANDLE_left = SingleO.hierarchy.GetTransform(_HANDLE_oneHand, "handle_left"); //핸들 
            _HANDLE_right = SingleO.hierarchy.GetTransform(_HANDLE_oneHand, "handle_right");
            //-------------------------

            //ctr->sh_left
            Transform sh_left = SingleO.hierarchy.GetTransform(ctr, "sh_left");
            _shoul_left_start = SingleO.hierarchy.GetTransform(sh_left, "dir_start");
            _shoul_left_end = SingleO.hierarchy.GetTransform(sh_left, "dir_end");
            //ctr->sh_right
            Transform sh_right = SingleO.hierarchy.GetTransform(ctr, "sh_right");
            _shoul_right_start = SingleO.hierarchy.GetTransform(sh_right, "dir_start");
            _shoul_right_end = SingleO.hierarchy.GetTransform(sh_right, "dir_end");
            //ctr->upperBody
            Transform upperBody = SingleO.hierarchy.GetTransform(ctr, "upperBody");
            _upperBody_start = SingleO.hierarchy.GetTransform(upperBody, "dir_start");
            _upperBody_end = SingleO.hierarchy.GetTransform(upperBody, "dir_end");
            //ctr->foot
            Transform foot = SingleO.hierarchy.GetTransform(ctr, "foot");
            _foot_start = SingleO.hierarchy.GetTransform(foot, "dir_start");
            _foot_end = SingleO.hierarchy.GetTransform(foot, "dir_end");
            _foot_movePos = SingleO.hierarchy.GetTransform(foot, "move_pos");

            //==================================================
            //1차 자식 : path_circle 조종항목
            //==================================================
            Transform path_circle = SingleO.hierarchy.GetTransform(transform, "path_circle");

            //-------------------------
            Transform two_cut = SingleO.hierarchy.GetTransform(path_circle, "two_cut");

            _ctrm_two_A0.Load(two_cut, "pos_circle_left");
            //_ctr_two_A0 = SingleO.hierarchy.GetTransform(two_cut, "pos_circle_left");
            //_motion_twoHand_A0 = _ctr_two_A0.GetComponent<MotionTrajectory>();
            //_ctr_two_A0_up = SingleO.hierarchy.GetTransform(_ctr_two_A0, "upDir_circle_left");
            //_ctr_two_A0_highest = SingleO.hierarchy.GetTransform(_ctr_two_A0, "highest_circle_left");
            //_ctr_two_A0_tornado_unlace = SingleO.hierarchy.GetTransform(_ctr_two_A0_highest, "tornado_unlace_left");
            //-------------------------

            _ctrm_two_B0.Load(two_cut, "pos_circle_right");
            //_ctr_two_B0 = SingleO.hierarchy.GetTransform(two_cut, "pos_circle_right");
            //_motion_twoHand_B0 = _ctr_two_B0.GetComponent<MotionTrajectory>();
            //_ctr_two_B0_up = SingleO.hierarchy.GetTransform(_ctr_two_B0, "upDir_circle_right");
            //_ctr_two_B0_highest = SingleO.hierarchy.GetTransform(_ctr_two_B0, "highest_circle_right");
            //_ctr_two_B0_tornado_unlace = SingleO.hierarchy.GetTransform(_ctr_two_B0_highest, "tornado_unlace_right");

            //----------------------------------------------

            Transform one_left = SingleO.hierarchy.GetTransform(path_circle, "one_left");

            _ctrm_one_A0.Load(one_left, "pos_circle_A0");

            //_ctr_one_A0      = SingleO.hierarchy.GetTransform(one_left, "pos_circle_A0");
            //_motion_oneHand_A0 = _ctr_one_A0.GetComponent<MotionTrajectory>();
            //_ctr_one_A0_up    = SingleO.hierarchy.GetTransform(_ctr_one_A0, "upDir_circle_A0");
            //_ctr_one_A0_highest  = SingleO.hierarchy.GetTransform(_ctr_one_A0, "highest_circle_A0");
            //_ctr_one_A0_tornado_unlace  = SingleO.hierarchy.GetTransform(_ctr_one_A0_highest, "tornado_unlace_A0");
            //-------------------------

            _ctrm_one_A1.Load(_ctrm_one_A0.tr, "pos_circle_A1");

            //_ctr_one_A1      = SingleO.hierarchy.GetTransform(_ctr_one_A0, "pos_circle_A1");
            //_motion_oneHand_A1 = _ctr_one_A1.GetComponent<MotionTrajectory>();
            //_ctr_one_A1_up    = SingleO.hierarchy.GetTransform(_ctr_one_A1, "upDir_circle_A1");
            //_ctr_one_A1_highest  = SingleO.hierarchy.GetTransform(_ctr_one_A1, "highest_circle_A1");
            //_ctr_one_A1_tornado_unlace  = SingleO.hierarchy.GetTransform(_ctr_one_A1_highest, "tornado_unlace_A1");

            //----------------------------------------------

            Transform one_right = SingleO.hierarchy.GetTransform(path_circle, "one_right");

            _ctrm_one_B0.Load(one_right, "pos_circle_B0");

            //_ctr_one_B0      = SingleO.hierarchy.GetTransform(one_right, "pos_circle_B0");
            //_motion_oneHand_B0 = _ctr_one_B0.GetComponent<MotionTrajectory>();
            //_ctr_one_B0_up    = SingleO.hierarchy.GetTransform(_ctr_one_B0, "upDir_circle_B0");
            //_ctr_one_B0_highest  = SingleO.hierarchy.GetTransform(_ctr_one_B0, "highest_circle_B0");
            //_ctr_one_B0_tornado_unlace  = SingleO.hierarchy.GetTransform(_ctr_one_B0_highest, "tornado_unlace_B0");
            //-------------------------

            _ctrm_one_B1.Load(_ctrm_one_B0.tr, "pos_circle_B1");

            //_ctr_one_B1      = SingleO.hierarchy.GetTransform(_ctr_one_B0, "pos_circle_B1");
            //_motion_oneHand_B1 = _ctr_one_B1.GetComponent<MotionTrajectory>();
            //_ctr_one_B1_up    = SingleO.hierarchy.GetTransform(_ctr_one_B1, "upDir_circle_B1");
            //_ctr_one_B1_highest  = SingleO.hierarchy.GetTransform(_ctr_one_B1, "highest_circle_B1");
            //_ctr_one_B1_tornado_unlace  = SingleO.hierarchy.GetTransform(_ctr_one_B1_highest, "tornado_unlace_B1");

            //-------------------------

            //__entire_aniTime = _stance_aniTime_SE;

            //==================================================

            Init_Equipment(); //장비 설정값 채우기 

        }


        //public delegate void CallFunc();
        //public CallFunc _Call_MovingModel = null;
        public void UpdateAll()
        {
            //방향값 갱신
            _foot_dir = VOp.Normalize(_tr_foot_dir.position - transform.position);
            _upperBody_dir = VOp.Normalize(_tr_upperBody_dir.position - transform.position);
            _sight_dir = VOp.Normalize (_tr_sight_dir.position - transform.position);


            _light_dir = SingleO.lightDir.position;
            _groundY = SingleO.groundY.position;


            //==================================================

            if (null == (object)_tr_shoulder_left || null == (object)_tr_shoulder_right) return;

            Vector3 shLR = _tr_shoulder_left.position - _tr_shoulder_right.position;
            _shoulder_length = shLR.magnitude;

            //==================================================

            if(ePart.TwoHand == _part_control)
            {
                //양손모드에서는 오른손 자세값을 사용하지 않느다 
                _eStance_hand_right = eStance.None;
                _equipment_handRight = ArmedInfo.eIdx.None;
            }

            //Rotate(_ref_movement._direction);

            //stance ani 재생 만들기 ( 현재 cut handle 값만 계산함 ) 
            //stance 값으로 handle을 계산 , Update_HandControl 보다 먼저 계산되어야 한다 
            Update_Ani();


            //handle에 대한 손 움직임 만들기 
            Update_Position_Handle(_tr_hand_left, _tr_hand_right);
            Update_Rotation_Dir(0);
            //BillBoard_Hand(_tr_hand_left, _tr_hand_right); //원본값을 변형할 필요 없음 
            Update_Equipment(); //tr_hand 값이 다 구해진 다음에 수행되어야 함 , 장비 장착
            //==================================================

            //MovingModel 처리가 와야 한다 
            // - 새로 구한 회전값 적용 
            // - 새로 구한 회전값에서 손의 위치 다시 계산 

            //==================================================


        }

        public void UpdateAll_Late()
        {
            //2d 게임에서의 높이표현 
            Update_Position_ProjectionSlope(); //view 위치값 갱신 , 이후 코드에서 view 회전량을 구한다 
            Update_Rotation_Dir(1);

            //칼이 화면을 바라보게 함  
            BillBoard_Hand(_armed_left, _armed_right);

            //그림자 표현
            Update_Shadow();

            //물건 움직임에 따라 손 스프라이트 표현 
            Update_Ani_Wrist();
            //==================================================

            Rotate(_ref_movement._direction);
        }
		//==================================================


        public void ActiveAll_Arms(bool value)
        {
            foreach (ArmedInfo arin in _list_armedInfo)
            {
                arin.SetActive(value);
            }
        }

        public void ActiveAll_Arms(bool value , ArmedInfo.eIdx except)
        {
            foreach(ArmedInfo arin in _list_armedInfo)
            {
                if (except == arin._eIdx) continue;

                arin.SetActive(value);
            }
        }

        public void Update_Position_ProjectionSlope()
        {
            //Transform view_left = _armed_left._tr_view;
            //Transform view_right = _armed_right._tr_view;

            //Update_HandControl 로 계산이 끝난 손정보를 2d카메라 상자에 투영한다   
            if (_active_projectionSlope)
            {
                _armed_left._tr_view.position = Project_BoxSlope(_tr_hand_left.position, (_tr_hand_left.position - _groundY).y);
                _armed_right._tr_view.position = Project_BoxSlope(_tr_hand_right.position, (_tr_hand_right.position - _groundY).y);

                _armed_left._view_arms_start = Project_BoxSlope(_armed_left._tr_frame_start.position);
                _armed_left._view_arms_end = Project_BoxSlope(_armed_left._tr_frame_end.position);

                _armed_right._view_arms_start = Project_BoxSlope(_armed_right._tr_frame_start.position);
                _armed_right._view_arms_end = Project_BoxSlope(_armed_right._tr_frame_end.position);

                //_view_target_0 = Project_BoxSlope(_target[0].position);
                //_view_target_1 = Project_BoxSlope(_target[1].position);
            }
            else
            {
                _armed_left._tr_view.position = _tr_hand_left.position;
                _armed_right._tr_view.position = _tr_hand_right.position;

                //그림자 계산을 할 무기의 시작위치와 끝위치를 설정한다 
                _armed_left._view_arms_start = _armed_left._tr_frame_start.position;
                _armed_left._view_arms_end = _armed_left._tr_frame_end.position;

                _armed_right._view_arms_start = _armed_right._tr_frame_start.position;
                _armed_right._view_arms_end = _armed_right._tr_frame_end.position;

                //_view_target_0 = _target[0].position;
                //_view_target_1 = _target[1].position;
            }
        }



        public void Init_Equipment()
        {
            if (ePart.TwoHand == _part_control)
            {
                _equipment_handRight = ArmedInfo.eIdx.None;
            }

            ActiveAll_Arms(false);

            _armed_left = _list_armedInfo[(int)_equipment_handLeft];
            _armed_left.SetActive(true);

            _armed_right = _list_armedInfo[(int)_equipment_handRight];
            _armed_right.SetActive(true);

            _armed_left.Update_Frame(_tr_hand_left);
            _armed_right.Update_Frame(_tr_hand_right);
        }

		public void Update_Equipment()
        {
            //ArmedInfo.eIdx arin_left_idx = _equipment_handLeft;
            //ArmedInfo.eIdx arin_right_idx = _equipment_handRight;
            //if (null != (object)_armed_left)
            //    arin_left_idx = _armed_left._eIdx;
            //if (null != (object)_armed_right)
                //arin_right_idx = _armed_right._eIdx;



            _isUpdateEq_handLeft = false;
            _isUpdateEq_handRight = false;

            //왼손 무기 장착
            if(ePart.OneHand == _part_control)
            {
                //if (null == (object)_armed_left || _armed_left._eIdx != _equipment_handLeft)
                if((_part_control != _part_control_prev) ||
                   (_armed_left._eIdx != _equipment_handLeft))
                {
                    ActiveAll_Arms(false, _equipment_handRight);
                    _armed_left = _list_armedInfo[(int)_equipment_handLeft];
                    _armed_left.SetActive(true);

                    _isUpdateEq_handLeft = true;
                }   

                //오른손 무기 장착
                //기존값과 비교 , 다른때만 갱신 
                //if (null == (object)_armed_right || _armed_right._eIdx != _equipment_handRight)
                if((_part_control != _part_control_prev) ||
                   (_armed_right._eIdx != _equipment_handRight))
                {
                    ActiveAll_Arms(false, _equipment_handLeft);
                    _armed_right = _list_armedInfo[(int)_equipment_handRight];
                    _armed_right.SetActive(true);

                    _isUpdateEq_handRight = true;
                }
            }
            else if (ePart.TwoHand == _part_control)
            {
                
                {
                    if ((_part_control != _part_control_prev) ||
                        (_armed_left._eIdx != _equipment_handLeft &&
                         eStandard.TwoHand_LeftO == _eHandStandard) ||
                        (eStandard.TwoHand_RightO == _eHandStandard_prev && 
                         eStandard.TwoHand_LeftO == _eHandStandard))
                    {
                        ActiveAll_Arms(false);
                        _armed_left = _list_armedInfo[(int)_equipment_handLeft];
                        _armed_right = _list_armedInfo[(int)ArmedInfo.eIdx.None];
                        _armed_left.SetActive(true); 
                        //DebugWide.LogBlue("1");
                        _isUpdateEq_handLeft = true;
                    }


                }    

                //왼손기준으로 설정되게 한다 , 붙이는 것만 오른손이다 
                {
                    if ((_part_control != _part_control_prev) ||
                        (_armed_right._eIdx != _equipment_handLeft &&
                         eStandard.TwoHand_RightO == _eHandStandard) || 
                        (eStandard.TwoHand_LeftO == _eHandStandard_prev &&
                         eStandard.TwoHand_RightO == _eHandStandard))
                    {
                        ActiveAll_Arms(false);
                        _armed_right = _list_armedInfo[(int)_equipment_handLeft];
                        _armed_left = _list_armedInfo[(int)ArmedInfo.eIdx.None]; //left 와 right 가 같은 무기면 위치값을 공유하는 문제있음 , 같은 무기가 안되게 함 
                        _armed_right.SetActive(true);

                        //DebugWide.LogBlue("2");
                        _isUpdateEq_handLeft = true;
                    }

                }    

            }

            //=============================================
            //DebugWide.LogBlue("sdfs");
            _armed_left.Update_Frame(_tr_hand_left);
            _armed_right.Update_Frame(_tr_hand_right);

            _eHandStandard_prev = _eHandStandard;
            _part_control_prev = _part_control;
		}


		public void Update_Ani()
        {

            switch(_part_control)
            {
                case ePart.OneHand:
                    {
                        
                        if (eStance.Sting == _eStance_hand_left)
                        {
                            _ani_hand_left._Call_Func = Call_Handle_LeftHand_Sting;
                        }

                        if (eStance.Cut == _eStance_hand_left)
                        {
                            _ani_hand_left._Call_Func = Call_Handle_LeftHand_Cut;
                        }

                        if (eStance.Sting == _eStance_hand_right)
                        {
                            _ani_hand_right._Call_Func = Call_Handle_RightHand_Sting;
                        }

                        if (eStance.Cut == _eStance_hand_right)
                        {
                            _ani_hand_right._Call_Func = Call_Handle_RightHand_Cut;
                        }
                    }
                    break;
                case ePart.TwoHand:
                    {
                        if(eStandard.TwoHand_LeftO == _eHandStandard)
                        {
                            if (eStance.Sting == _eStance_hand_left)
                            {
                                _ani_hand_left._Call_Func = Call_Handle_TwoHand_Sting;
                            }

                            if (eStance.Cut == _eStance_hand_left)
                            {
                                _ani_hand_left._Call_Func = Call_Handle_TwoHand_LeftO_Cut;
                            }    
                        }


                        if (eStandard.TwoHand_RightO == _eHandStandard)
                        {
                            if (eStance.Sting == _eStance_hand_right)
                            {
                                _ani_hand_left._Call_Func = Call_Handle_TwoHand_Sting;
                            }

                            if (eStance.Cut == _eStance_hand_right)
                            {
                                _ani_hand_left._Call_Func = Call_Handle_TwoHand_RightO_Cut;
                            }
                        }

                    }
                    break;
            }
            _ani_upperBody._Call_Func = Call_UpperBody_Rotate;
            _ani_foot_move._Call_Func = Call_Foot_Move;
            _ani_foot_rotate._Call_Func = Call_Foot_Rotate;



            _ani_hand_left.Update();
            _ani_hand_right.Update();
            _ani_upperBody.Update();
            _ani_foot_move.Update();
            _ani_foot_rotate.Update();


        }


        private void UpperBody_RotateAni(float t)
        {
            if (0 > t) t = 0;
            else if (t > 1f) t = 1f;

            //시작각도 
            float startAngle = Geo.AngleSigned_AxisY(_foot_dir, _upperBody_start.position - transform.position);
            //전체각도 
            //float entireAngle = Geo.AngleSigned_AxisY(_upperBody_start.position - transform.position, _upperBody_end.position - transform.position);
            float entireAngle = Geo.Angle360_AxisRotate_Normal_Axis(_upperBody_start.position - transform.position, _upperBody_end.position - transform.position, ConstV.v3_up);

            Vector3 temp = _tr_waist.localEulerAngles;
            temp.y = startAngle + entireAngle * t;
            _tr_waist.localEulerAngles = temp;
        }


        public void Rotate(Vector3 dir)
        {
            
            Vector3 temp = transform.localEulerAngles;
            temp.y = Geo.AngleSigned_AxisY(ConstV.v3_forward, dir);
            transform.localEulerAngles = temp;

            _tr_root.localEulerAngles = temp; //루트본에도 적용 
        }


        //회오리를 이용한 구면 보간. 360도 이상 표현 가능 
        //rotateCount : pos1 -> pos2 까지 몇번 회전 하는지 나타냄
        //rotateCount = 0 : pos1 -> pos2 
        //rotateCount = 1 : (pos1 -> pos2 -> pos1) 까지 회전
        //rotateCount = 2 : (pos1 -> pos2 -> pos1) -> (pos1 -> pos2 -> pos1) 까지 회전
        public Vector3 InterpolationTornado(Vector3 origin, Vector3 pos1, Vector3 pos2, Vector3 upDir, float rotateCount,  float t)
        {
            Vector3 startO = pos1 - origin;
            float angle = UtilGS9.Geo.Angle360_AxisRotate(pos1 - origin, pos2 - origin, upDir);

            return origin + Quaternion.AngleAxis(t * (angle + (360f * rotateCount)), upDir) * startO;
        }

        //호를 이용한 구면 보간. 360도 까지 표현 가능
        public Vector3 InterpolationArc(Vector3 origin, Vector3 pos1, Vector3 pos2, Vector3 upDir, float t)
        {

            Vector3 startO = pos1 - origin;
            float angle = UtilGS9.Geo.Angle360_AxisRotate(pos1 - origin, pos2 - origin, upDir);

            return origin + Quaternion.AngleAxis( t * angle, upDir) * startO;
        }


        //2d 게임같은 높이값을 표현한다. 기울어진 투영상자의 빗면에 높이값을 투영한다.
        //그 길이를 대상위치에 z축 값을 더한다  
        public Vector3 Project_BoxSlope(Vector3 target, float heightY)
        {
            float angle = 90f - _boxSlope_angle;
            float b = Mathf.Tan(angle * Mathf.Deg2Rad) * Mathf.Abs(heightY);

            target.z += b;
            return target;
        }

        public Vector3 Project_BoxSlope(Vector3 target)
        {
            float heightY = (target - _groundY).y;
            return Project_BoxSlope(target, heightY);
        }


        public void Update_Position_Handle(Transform tr_left, Transform tr_right)
        {
            Vector3 hand_left = tr_left.position;
            Vector3 hand_right = tr_right.position;

            if (_part_control == ePart.OneHand)
            {
                Vector3 newRightPos;
                Vector3 newLeftPos;
                float newRightLength;
                float newLeftLength;

                //찌르기
                if (eStance.Sting == _eStance_hand_left)
                {
                    
                    this.CalcHandPos(_HANDLE_left.position, _tr_shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out newLeftPos, out newLeftLength);
                    //_tr_hand_left.position = newLeftPos;
                    hand_left = newLeftPos;
                    _arm_left_length = newLeftLength;
                }
                if (eStance.Sting == _eStance_hand_right)
                { 
                    this.CalcHandPos(_HANDLE_right.position, _tr_shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out newRightPos, out newRightLength);
                    //_tr_hand_right.position = newRightPos;
                    hand_right = newRightPos;
                    _arm_right_length = newRightLength;
                }


                //Cut_OneHand(out hand_left, out hand_right);

                if (eStance.Cut == _eStance_hand_left)
                {   //베기
                    
                    Vector3 handle = _HANDLE_left.position;
                    //Vector3 upDir = _left_axis_up.position - _left_axis_o.position;

                    Vector3 newPos = Vector3.zero;
                    float newLength = 0f;
                    //------------------------------------------


                    _ctrm_one_A0.SetModel();
                    _ctrm_one_A1.SetModel();
                    CalcHandPos_PlaneArea(_ctrm_one_A0.model, handle,
                                          _tr_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                          out newPos, out newLength);

                    _arm_left_length = newLength;
                    hand_left = newPos;


                    CalcHandPos_PlaneArea(_ctrm_one_A1.model, handle,
                                          _tr_shoulder_left.position, 1000, _arm_left_min_length,
                                          out newPos, out newLength);
                    //_tr_arm_left_dir.position = newPos;
                    _hand_left_dirPos = newPos;
                }

                if (eStance.Cut == _eStance_hand_right)
                {
                    
                    Vector3 handle = _HANDLE_right.position;
                    //upDir = _right_axis_up.position - _right_axis_o.position;

                    Vector3 newPos = Vector3.zero;
                    float newLength = 0f;

                    //_Model_B0.kind = _motion_oneHand_B0._eModel; //_eModelKind_Right_0;
                    //_Model_B1.kind = _motion_oneHand_B1._eModel; //_eModelKind_Right_1;
                    //SetModel_OneHand(_Model_B0, _Model_B1);
                    _ctrm_one_B0.SetModel();
                    _ctrm_one_B1.SetModel();
                    CalcHandPos_PlaneArea(_ctrm_one_B0.model, handle,
                                          _tr_shoulder_right.position, _arm_right_max_length, _arm_right_min_length,
                                          out newPos, out newLength);

                    _arm_right_length = newLength;
                    //_tr_hand_right.position = newPos;
                    hand_right = newPos;


                    CalcHandPos_PlaneArea(_ctrm_one_B1.model, handle,
                                          _tr_shoulder_right.position, 1000, _arm_right_min_length,
                                          out newPos, out newLength);

                    //_tr_arm_right_dir.position = newPos;
                    _hand_right_dirPos = newPos;

                
                }

            }

            //==================================================

            if (_part_control == ePart.TwoHand)
            {
                //if(eStandard.TwoHand_LeftO == _eHandStandard)
                {
                    if (eStance.Sting == _eStance_hand_left)
                    {   //찌르기 , 치기 


                        //=======================================
                        //test
                        //hand_left = _tr_hand_left.position;
                        //_arm_left_length = (_tr_shoulder_left.position - hand_left).magnitude;
                        //Vector3 dir = VOp.Normalize(_tr_hand_right.position - _tr_hand_left.position);
                        //hand_right = hand_left + dir * _twoHand_length;
                        //_arm_right_length = (_tr_shoulder_right.position - hand_right).magnitude;

                        //=======================================
                        //test
                        //Vector3 objectDir = _hs_objectDir.position - _hs_standard.position;
                        //objectDir = VOp.Normalize(objectDir);
                        //_hand_left = _hs_standard.position;
                        //_arm_left_length = (_tr_shoulder_left.position - _hand_left).magnitude;
                        //_hand_right = _hand_left + objectDir * _twoHand_length;
                        //_arm_right_length = (_tr_shoulder_right.position - _hand_right).magnitude;
                        //=======================================

                        //return; //임시로 막아놓은 코드임 --- chamto test

                        //조종축 회전 테스트 코드 
                        //_hc1_object_dir.position = _HANDLE_staff.position + (_HANDLE_staff.position - _hc1_standard.position);
                        //_hc1_standard.position = _HANDLE_staff.position + (_HANDLE_staff.position - _hc1_object_dir.position);

                        Vector3 hand_ori, hand_end;
                        float len_ori, len_end;
                        Sting_TwoHand(out hand_ori,out len_ori, out hand_end, out len_end);
                        if (_eHandStandard == eStandard.TwoHand_LeftO)
                        {
                            hand_left = hand_ori;
                            _arm_left_length = len_ori;
                            hand_right = hand_end;
                            _arm_right_length = len_end;
                        }
                        else if (_eHandStandard == eStandard.TwoHand_RightO)
                        {
                            hand_left = hand_end;
                            _arm_left_length = len_end;
                            hand_right = hand_ori;
                            _arm_right_length = len_ori;
                        }
                        //-----------------------

                    }
                    //================================================================
                    else if (eStance.Cut == _eStance_hand_left)
                    {   //베기 


                        //-----------------------

                        Cut_TwoHand(_HANDLE_twoHand.position, _eHandStandard,
                                    out hand_left, out hand_right); //_eModelKind_Left_0, _eModelKind_Right_0);

                        //--------------------
                        //찌르기 모드로 연결하기 위한 핸들값 조정 
                        //_hs_standard.position = hand_left;
                        //_hs_objectDir.position = hand_right;

                    }//else end
                }

            }

            tr_left.position = hand_left;
            tr_right.position = hand_right;
        }

        public Vector3 CalcShadowPos(Vector3 lightDir, Vector3 ground, Vector3 objectPos)
        {

            //평면과 광원사이의 최소거리 
            Vector3 groundUp = ConstV.v3_up;
            Vector3 groundToObject = objectPos - ground;
            float b = Vector3.Dot(groundToObject, groundUp);

            //s = len / sin@
            float theta0 = Geo.Angle360(-groundUp, lightDir, Vector3.Cross(-groundUp, lightDir));
            float theta1 = 90f - theta0;

            float s = b / Mathf.Sin(theta1 * Mathf.Deg2Rad);

            Vector3 nDir = VOp.Normalize(lightDir);

            return objectPos + s * nDir;
        }



        public Vector3 CalcShadowPos(Vector3 lightDir, Vector3 objDir, Vector3 ground, Vector3 objectPos, out float proj_groundToObject)
        {
            //물체가 땅바닥보다 아래에 있으면 
            if (ground.y > objectPos.y)
                lightDir = objDir;

            //평면과 광원사이의 최소거리 
            Vector3 groundUp = ConstV.v3_up;
            Vector3 groundToObject = objectPos - ground;
            proj_groundToObject = Vector3.Dot(groundToObject, groundUp);

            //s = len / sin@
            float sinAngle = Geo.Angle360(-groundUp, lightDir, Vector3.Cross(-groundUp, lightDir));
            sinAngle = 90f - sinAngle;

            //s : 삼각형의 빗면 
            float s = proj_groundToObject / Mathf.Sin(sinAngle * Mathf.Deg2Rad);

            Vector3 nDir = VOp.Normalize(lightDir);

            return objectPos + s * nDir;
        }

        public bool CalcHandPos_LineSegment2(Vector3 line_start, Vector3 line_end, 
                                             float line_min_length, float line_max_length,
                                            Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                             out Vector3 newHand_end, out float newArm_length_end)

        {
            newHand_end = line_start;
            //newHand_start = line_start;

            Vector3 cpt_first, cpt_second;
            Vector3 dir_line_SE = line_end - line_start;
            Vector3 n_dir_line_SE = VOp.Normalize(dir_line_SE);
            float len_line_SE = dir_line_SE.magnitude;

            Vector3 dir_s_sh = shoulder_pos - line_start;
            float a = dir_s_sh.magnitude;
            Vector3 up = Vector3.Cross(dir_s_sh, dir_line_SE);
            //----------------------------------------------

            bool result = UtilGS9.Geo.IntersectRay2(shoulder_pos, arm_max_length, line_start, n_dir_line_SE, out cpt_first);
            float p = (line_start - cpt_first).magnitude;


            //----------------------------------------------

            //원과 접촉가능성이 있는 경우 
            if (true == result)
            {   
                //원안에 시작점이 있는 경우 
                if ((arm_max_length * arm_max_length >= (shoulder_pos - line_start).sqrMagnitude))
                {
                    
                    float L = len_line_SE;
                    if (line_min_length > len_line_SE) L = line_min_length;
                    if (line_max_length < len_line_SE) L = line_max_length;
                    if (L > p) L = p;

                    newHand_end = line_start + n_dir_line_SE * L;

                    //DebugWide.LogBlue("1 ");
                }
                else
                {  
                    //라인이 원밖에 있다 또는 원밖에 생긴다 
                    if(p > len_line_SE || p > line_max_length)
                    {
                        //DebugWide.LogBlue("2 ");
                        result = false; 
                    }
                    else
                    {
                        
                        float L = len_line_SE;
                        float L2 = len_line_SE * 2; //두번째 접촉점이 없을 떄의 경우도 조건식을 성립시키기 위하여 임의로 증가시킴 
                        //원안에 끝점이 있는지 검사한다 
                        if (arm_max_length * arm_max_length < (shoulder_pos - line_end).sqrMagnitude)
                        {
                            //두번째 접촉점을 구한다
                            UtilGS9.Geo.IntersectRay2(shoulder_pos, arm_max_length, cpt_first + n_dir_line_SE * 0.0001f, n_dir_line_SE, out cpt_second);    
                            L2 = (line_start - cpt_second).magnitude;
                        }

                        if (line_min_length > len_line_SE) L = line_min_length;
                        if (line_max_length < len_line_SE) L = line_max_length;
                        if (L > L2) L = L2;

                        newHand_end = line_start + n_dir_line_SE * L;

                        //DebugWide.LogBlue("3 : ");
                    }

                }

            }

            //라인이 원밖에 있는 경우 , 코사인 제2법칙 이용 
            if (false == result )
            {

                //DebugWide.LogBlue("라인이 원밖에 있음 ");

                float p_min = a - arm_max_length;
                float p_max = a + arm_max_length;
                float L = len_line_SE; //공책식에는 p로 표현 - 20201020
                if (line_min_length > len_line_SE) L = line_min_length;
                if (line_max_length < len_line_SE) L = line_max_length;
                if (p_min > L) L = p_min;
                if (p_max < L) L = p_max;


                float angle4 = 0;
                float LL = Mathf.Sqrt(a * a - arm_max_length * arm_max_length); //피타고라스 
                if(LL < L)
                {
                    //DebugWide.LogBlue("4 ");
                    angle4 = Mathf.Atan2(arm_max_length, LL) * Mathf.Rad2Deg;

                    if (line_min_length > LL) LL = line_min_length;
                    L = LL;
                }
                else
                {
                    //DebugWide.LogBlue("5 ");
                    float aL2 = (a * L * 2);
                    float cos4 = (a * a + L * L - arm_max_length * arm_max_length) / aL2; //코사인 제2법칙 
                    cos4 = Mathf.Clamp(cos4, -1f, 1f);
                    angle4 = Mathf.Acos(cos4) * Mathf.Rad2Deg;    
                }

                Vector3 cpt = Quaternion.AngleAxis(angle4, up) * dir_s_sh;
                newHand_end = line_start + VOp.Normalize(cpt) * L;

                //-----------------------
            }

            newArm_length_end = (newHand_end - shoulder_pos).magnitude;


            return result;

        }

        //어깨범위와 선분의 교차위치를 구한다. 어깨범위의 최소범위는 적용안됨 
        public bool CalcHandPos_LineSegment(Vector3 line_origin, Vector3 line_dir, float line_length,
                                            Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                            out Vector3 newHand_pos, out float newArm_length)
        {

            Vector3 n_line_dir = VOp.Normalize(line_dir);
            Vector3 posOnMaxCircle;
            float sqr_arm_max_length = arm_max_length * arm_max_length;
            bool result = UtilGS9.Geo.IntersectRay2(shoulder_pos, arm_max_length, line_origin, n_line_dir, out posOnMaxCircle);

            //----------------------------------------------

            if (true == result)
            {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 있다  

                if ((sqr_arm_max_length - (shoulder_pos - line_origin).sqrMagnitude) > 0)
                {   //왼손이 오른손 최대 범위 안에 있는 경우

                    if ((line_origin - posOnMaxCircle).sqrMagnitude > line_length * line_length)
                    {
                        newHand_pos = line_origin + n_line_dir * line_length;
                    }
                    else
                    {
                        newHand_pos = posOnMaxCircle;
                    }
                }
                else
                {   //왼손이 오른손 최대 범위 밖에 있는 경우 

                    newHand_pos = line_origin + n_line_dir * line_length;
                    if ((newHand_pos - shoulder_pos).sqrMagnitude > sqr_arm_max_length)
                    {
                        newHand_pos = posOnMaxCircle;
                    }

                }

            }
            else
            {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 없다   :  목표와 왼손 사이의 직선경로가 오른손 최대범위에 닿지 않는 경우

                //Vector3 dir_o_sh = (shoulder_pos - line_origin);
                //Vector3 n_dir_o_sh = VOp.Normalize(dir_o_sh);
                //float len_dir_o_sh = dir_o_sh.magnitude;
                //float length_contactPt = dir_o_sh.sqrMagnitude - sqr_arm_max_length;
                //length_contactPt = (float)System.Math.Sqrt(length_contactPt);
                //float proj_cos = length_contactPt / dir_o_sh.magnitude;
                //float len_contactPt = len_dir_o_sh - arm_max_length;
                //float proj_cos = len_contactPt / len_dir_o_sh;
                //float angleC = Mathf.Acos(proj_cos) * Mathf.Rad2Deg;
                //Vector3 shaft_l = Vector3.Cross(line_origin, shoulder_pos);
                //newHand_pos = line_origin + Quaternion.AngleAxis(-angleC, shaft_l) * n_dir_o_sh * len_contactPt;
                //-----------------------

                //Vector3 line_end = line_origin + n_line_dir * line_length;
                Vector3 line_end = line_origin + line_dir;
                Vector3 dir_sh_e = (line_end - shoulder_pos);
                Vector3 n_dir_sh_e = VOp.Normalize(dir_sh_e);
                newHand_pos = shoulder_pos + n_dir_sh_e * arm_max_length;

                //-----------------------
            }

            newArm_length = (newHand_pos - shoulder_pos).magnitude;
            //if (newArm_length > arm_max_length)
                //newArm_length = arm_max_length;


            return result;

        }


        //handle 이 지정범위에 포함되면 참 , 그렇지 않으면 거짓을 반환 
        public bool CalcHandPos(Vector3 handle,
                                            Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                            out Vector3 newHand_pos, out float newArm_length)
        {
            bool inCircle = true;
            Vector3 sdToHandle = (handle - shoulder_pos);
            float length_sdToHandle = sdToHandle.magnitude;
            if (float.Epsilon > length_sdToHandle)
                length_sdToHandle = 1f; //NaN 예외처리 추가 
            Vector3 n_shToHandle = sdToHandle / length_sdToHandle;
            newArm_length = length_sdToHandle;
            newHand_pos = handle;


            if (length_sdToHandle < arm_min_length)
            {
                //DebugWide.LogBlue("2 dsdd  "  + length_sdToHandle + "  " + arm_min_length); //test
                newArm_length = arm_min_length;
                newHand_pos = shoulder_pos + n_shToHandle * newArm_length;
                inCircle = false;
                //DebugWide.LogBlue("1 dsdd  " + (newHand_pos - shoulder_pos).magnitude); //test
            }
            else if (length_sdToHandle > arm_max_length)
            {
                newArm_length = arm_max_length;
                newHand_pos = shoulder_pos + n_shToHandle * newArm_length;
                inCircle = false;
            }

            return inCircle;
        }

        public void CalcHandPos_AroundCircle(Vector3 handle, Vector3 circle_up, Vector3 circle_pos, float circle_radius,
                                            Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                            out Vector3 newHand_pos, out float newArm_length)
        {

            Vector3 handleToCenter = circle_pos - handle;
            Vector3 proj_handle = circle_up * Vector3.Dot(handleToCenter, circle_up) / circle_up.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 
                                                                                                               //axis_up 이 정규화 되었을 때 : = Dot(handleToCenter, n_axis_up) : n_axis_up 에 handleToCenter  를 투영한 길이를 반환한다  
            Vector3 proj_handlePos = handle + proj_handle;
            Vector3 n_circleToHand = (proj_handlePos - circle_pos).normalized;


            //===== 1차 계산
            Vector3 aroundCalcPos = circle_pos + n_circleToHand * circle_radius;
            Vector3 n_sdToAround = (aroundCalcPos - shoulder_pos).normalized;
            Vector3 handleCalcPos = aroundCalcPos;

            float sqrLength_sdToAround = (aroundCalcPos - shoulder_pos).sqrMagnitude;
            float sqrLength_sdToHandle = (proj_handlePos - shoulder_pos).sqrMagnitude;

            float length_cur = Mathf.Sqrt(sqrLength_sdToHandle);
            //_arm_left_length = length_curLeft;

            //최대길이를 벗어나는 핸들 최대길이로 변경
            if (length_cur > arm_max_length)
            {
                length_cur = arm_max_length;
                sqrLength_sdToHandle = arm_max_length * arm_max_length;
            }

            //최소원 , 최대원 , 현재원(핸들위치기준) , 주변원
            //===== 2차 계산
            if (arm_min_length >= length_cur)
            {   //현재원이 최소원안에 있을 경우 : 왼손길이 최소값으로 조절 
                //DebugWide.LogBlue("0"); //test
                length_cur = arm_min_length;
                n_sdToAround = (proj_handlePos - shoulder_pos).normalized;
                handleCalcPos = shoulder_pos + n_sdToAround * length_cur;
            }
            else
            {

                if (sqrLength_sdToAround <= arm_min_length * arm_min_length)
                {   //주변원 위의 점이 최소거리 이내인 경우
                    //DebugWide.LogBlue("1"); //test
                    length_cur = arm_min_length;
                    handleCalcPos = shoulder_pos + n_sdToAround * length_cur;
                }
                else if (sqrLength_sdToAround >= sqrLength_sdToHandle)
                {   //왼손범위에 벗어나는 주변원상 위의 점인 경우  
                    //DebugWide.LogBlue("2"); //test
                    handleCalcPos = shoulder_pos + n_sdToAround * length_cur;
                }

            }

            newArm_length = (handleCalcPos - shoulder_pos).magnitude;
            newHand_pos = handleCalcPos;
        }



        //public void SetModel_OneHand(Geo.Model model_0, Geo.Model model_1)
        //{
        //    Vector3 upDir;
        //    if (true == model_0.IsLeft())
        //    {
        //        upDir = _ctr_one_A0_up.position - _ctr_one_A0.position;
        //        //upDir.Normalize();
        //        model_0.SetModel(upDir, _ctr_one_A0.position, _motion_oneHand_A0._radius, _ctr_one_A0_highest.position,
        //                         _motion_oneHand_A0._far_radius, _motion_oneHand_A0._tornado_angle, _ctr_one_A0_tornado_unlace.localPosition);

        //        upDir = _ctr_one_A1_up.position - _ctr_one_A1.position;
        //        //upDir.Normalize();
        //        model_1.SetModel(upDir, _ctr_one_A1.position, _motion_oneHand_A1._radius, _ctr_one_A1_highest.position,
        //                         _motion_oneHand_A1._far_radius, _motion_oneHand_A1._tornado_angle, _ctr_one_A1_tornado_unlace.localPosition);
        //    }
        //    if (true == model_0.IsRight())
        //    {
        //        upDir = _ctr_one_B0_up.position - _ctr_one_B0.position;
        //        //upDir.Normalize();
        //        model_0.SetModel(upDir, _ctr_one_B0.position, _motion_oneHand_B0._radius, _ctr_one_B0_highest.position,
        //                         _motion_oneHand_B0._far_radius, _motion_oneHand_B0._tornado_angle, _ctr_one_B0_tornado_unlace.localPosition);

        //        upDir = _ctr_one_B1_up.position - _ctr_one_B1.position;
        //        //upDir.Normalize();
        //        model_1.SetModel(upDir, _ctr_one_B1.position, _motion_oneHand_B1._radius, _ctr_one_B1_highest.position,
        //                         _motion_oneHand_B1._far_radius, _motion_oneHand_B1._tornado_angle, _ctr_one_B1_tornado_unlace.localPosition);
        //    }

        //}

        //public void SetModel_TwoHand(Geo.Model model, Vector3 upDir)
        //{
        //    if (true == model.IsLeft())
        //    {
        //        model.SetModel(upDir, _ctr_two_A0.position, _motion_twoHand_A0._radius, _ctr_two_A0_highest.position,
        //                       _motion_twoHand_A0._far_radius, _motion_twoHand_A0._tornado_angle, _ctr_two_A0_tornado_unlace.localPosition);
        //    }
        //    if (true == model.IsRight())
        //    {
        //        model.SetModel(upDir, _ctr_two_B0.position, _motion_twoHand_B0._radius, _ctr_two_B0_highest.position,
        //                       _motion_twoHand_B0._far_radius, _motion_twoHand_B0._tornado_angle, _ctr_two_B0_tornado_unlace.localPosition);
        //    }

        //}

        //대상 도형모델의 평면공간에 투영한 결과를 반환한다.
        //평면공간에 투영이 불가능한 경우에는 어깨와 평면공간의 최소거리의 위치를 반환한다
        public void CalcHandPos_PlaneArea(Geo.Model model, Vector3 handle,
                                            Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                            out Vector3 newHand_pos, out float newArm_length)
        {

            Vector3 md_origin = model.origin;

            //===== 1차 계산
            Vector3 upDir2;
            Vector3 aroundCalcPos = model.CollisionPos(handle, out upDir2);


            //===== 어깨원 투영
            Vector3 proj_sdToOrigin = upDir2 * Vector3.Dot((md_origin - shoulder_pos), upDir2) / upDir2.sqrMagnitude;
            Vector3 proj_sdToOringPos = shoulder_pos + proj_sdToOrigin; //어깨원의 중심점을 주변원공간에 투영 
            float sqrLength_d = (aroundCalcPos - shoulder_pos).sqrMagnitude;

            if (sqrLength_d > arm_max_length * arm_max_length)
            {   //주변원과 어꺠최대원이 접촉이 없는 상태. [최대값 조절 필요]

                Vector3 interPos;
                if (false == UtilGS9.Geo.IntersectRay2(shoulder_pos, arm_max_length, aroundCalcPos, (proj_sdToOringPos - aroundCalcPos).normalized, out interPos))
                {
                    //todo : 최적화 필요 , 노멀 안구하는 다른 방법 찾기 
                    interPos = shoulder_pos + (interPos - shoulder_pos).normalized * arm_max_length;
                }

                aroundCalcPos = interPos;

            }
            else if (sqrLength_d < arm_min_length * arm_min_length)
            {   //주변원과 어깨최소원이 접촉한 상태

                Vector3 interPos;
                UtilGS9.Geo.IntersectRay2(shoulder_pos, arm_min_length, md_origin, (aroundCalcPos - md_origin).normalized, out interPos);
                aroundCalcPos = interPos;

            }

            newArm_length = (aroundCalcPos - shoulder_pos).magnitude;
            newHand_pos = aroundCalcPos;
        }

        //public void Cut_OneHand(out Vector3 hand_left, out Vector3 hand_right)
        //{
        //    hand_left = ConstV.v3_zero;
        //    hand_right = ConstV.v3_zero;

        //    if (ePart.OneHand == _part_control)
        //    {

        //        //주변원 반지름 갱신
        //        //_radius_circle_A0 = (_pos_circle_A0.position - _edge_circle_A0.position).magnitude;
        //        //_radius_circle_A1 = (_pos_circle_A1.position - _edge_circle_A1.position).magnitude;
        //        //_radius_circle_B0 = (_pos_circle_B0.position - _edge_circle_B0.position).magnitude;
        //        //_radius_circle_B1 = (_pos_circle_B1.position - _edge_circle_B1.position).magnitude;

        //        //====================

        //        Vector3 handle = _HANDLE_left.position;
        //        //Vector3 upDir = _left_axis_up.position - _left_axis_o.position;

        //        Vector3 newPos = Vector3.zero;
        //        float newLength = 0f;
        //        //------------------------------------------

        //        _Model_A0.kind = _motion_oneHand_A0._eModel; //_eModelKind_Left_0;
        //        _Model_A1.kind = _motion_oneHand_A1._eModel; //_eModelKind_Left_1;
        //        SetModel_OneHand(_Model_A0, _Model_A1);
        //        CalcHandPos_PlaneArea(_Model_A0, handle,
        //                              _tr_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
        //                              out newPos, out newLength);

        //        _arm_left_length = newLength;
        //        //_tr_hand_left.position = newPos;
        //        hand_left = newPos;


        //        CalcHandPos_PlaneArea(_Model_A1, handle,
        //                              _tr_shoulder_left.position, 1000, _arm_left_min_length,
        //                              out newPos, out newLength);
        //        _tr_arm_left_dir.position = newPos;

        //        //------------------------------------------

        //        handle = _HANDLE_right.position;
        //        //upDir = _right_axis_up.position - _right_axis_o.position;

        //        _Model_B0.kind = _motion_oneHand_B0._eModel; //_eModelKind_Right_0;
        //        _Model_B1.kind = _motion_oneHand_B1._eModel; //_eModelKind_Right_1;
        //        SetModel_OneHand(_Model_B0, _Model_B1);
        //        CalcHandPos_PlaneArea(_Model_B0, handle,
        //                              _tr_shoulder_right.position, _arm_right_max_length, _arm_right_min_length,
        //                              out newPos, out newLength);

        //        _arm_right_length = newLength;
        //        //_tr_hand_right.position = newPos;
        //        hand_right = newPos;


        //        CalcHandPos_PlaneArea(_Model_B1, handle,
        //                              _tr_shoulder_right.position, 1000, _arm_right_min_length,
        //                              out newPos, out newLength);

        //        _tr_arm_right_dir.position = newPos;

        //    }
        //}

        //지정손 기준으로 지정길이 만큼의 반대손 위치 구하기 
        //handO : 기준이 되는 손 , handDir : 손과 다른손간의 방향 , twoLength : 손과 다른손의 사이길이 
        //handE : 위치를 구할려는 손 
        public void ApplyHandPos_TwoHandLength(Vector3 handLeft_pos, float handLeft_length, Vector3 handRight_pos, float handRight_length, 
                                               eStandard eHandS, float twoLength,
                                               out Vector3 hand_left, out Vector3 hand_right)
        {
            hand_left = ConstV.v3_zero;
            hand_right = ConstV.v3_zero;

            Vector3 handO; //Origin
            Vector3 handE; //End
            Vector3 shO;
            Vector3 shE;
            Vector3 handOE_dir;
            Vector3 handOE_nDir;
            Vector3 new_handE_pos;
            float new_handE_length;

            float shE_min;
            float shE_max;

            if (eHandS == eStandard.TwoHand_LeftO)
            {
                handO = handLeft_pos;
                handE = handRight_pos;
                shO = _tr_shoulder_left.position;
                shE = _tr_shoulder_right.position;
                shE_min = _arm_right_min_length;
                shE_max = _arm_right_max_length;
            }
            else if (eHandS == eStandard.TwoHand_RightO)
            {
                handO = handRight_pos;
                handE = handLeft_pos;
                shO = _tr_shoulder_right.position;
                shE = _tr_shoulder_left.position;
                shE_min = _arm_left_min_length;
                shE_max = _arm_left_max_length;
            }
            else
            {
                DebugWide.LogBlue("예외상황 : CalcHandPos_TwoHandLength : 처리 못하는 eHandO 값");
                return;
            }

            handOE_dir = (handE - handO);
            handOE_nDir = handOE_dir.normalized;

            //====================

            //왼손으로부터 오른손의 지정된 거리에 맞게 위치 계산
            new_handE_pos = handO + handOE_nDir * twoLength;
            Vector3 sdToHand = (new_handE_pos - shE);
            float length_sdToHand = sdToHand.magnitude;
            Vector3 n_sdToHand = sdToHand / length_sdToHand;
            new_handE_length = length_sdToHand;

            if (length_sdToHand > shE_max)
            {   //오른손 위치가 오른손의 최대범위를 벗어난 경우 
                new_handE_length = shE_max;
                new_handE_pos = shE + n_sdToHand * new_handE_length;
            }
            else if (length_sdToHand < shE_min)
            {   //오른손 위치가 오른손의 최소범위를 벗어난 경우 
                new_handE_length = shE_min;
                new_handE_pos = shE + n_sdToHand * new_handE_length;
            }

            //====================

            if (eHandS == eStandard.TwoHand_LeftO)
            {
                _arm_left_length = handLeft_length;
                //_tr_hand_left.position = handO;
                hand_left = handO;

                _arm_right_length = new_handE_length;
                //_tr_hand_right.position = new_handE_pos;
                hand_right = new_handE_pos;
            }
            if (eHandS == eStandard.TwoHand_RightO)
            {
                _arm_right_length = handRight_length;
                //_tr_hand_right.position = handO;
                hand_right = handO;

                _arm_left_length = new_handE_length;
                //_tr_hand_left.position = new_handE_pos;
                hand_left = new_handE_pos;
            }

        }


        public void Sting_TwoHand(out Vector3 hand_ori , out float len_ori, out Vector3 hand_end, out float len_end)
        {
            //Vector3 hand_ori; //Origin
            //Vector3 hand_end; //End
            Vector3 sh_ori = ConstV.v3_zero;
            Vector3 sh_end = ConstV.v3_zero;;

            //float len_ori = 0;
            //float len_end = 0;
            float sh_ori_min = 0;
            float sh_ori_max = 0;
            float sh_end_min = 0;
            float sh_end_max = 0;

            if (_eHandStandard == eStandard.TwoHand_LeftO)
            {
                
                sh_ori = _tr_shoulder_left.position;
                sh_end = _tr_shoulder_right.position;
                sh_ori_min = _arm_left_min_length;
                sh_ori_max = _arm_left_max_length;
                sh_end_min = _arm_right_min_length;
                sh_end_max = _arm_right_max_length;
            }
            else if (_eHandStandard == eStandard.TwoHand_RightO)
            {
                
                sh_ori = _tr_shoulder_right.position;
                sh_end = _tr_shoulder_left.position;
                sh_ori_min = _arm_right_min_length;
                sh_ori_max = _arm_right_max_length;
                sh_end_min = _arm_left_min_length;
                sh_end_max = _arm_left_max_length;
            }

            //================================

            //Vector3 objectDir = _hs_objectDir.position - _hs_standard.position;
            //Vector3 newPos_left, newPos_right;
            //float newLength;
            this.CalcHandPos(_hs_standard.position, sh_ori, sh_ori_max, sh_ori_min, out hand_ori, out len_ori);

            //hand_left = newPos_left;
            //len_arm_left = newLength;


            //this.CalcHandPos_LineSegment(newPos, objectDir, _twoHand_length,
            //_tr_shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out newPos, out newLength);
            this.CalcHandPos_LineSegment2(hand_ori, _hs_objectDir.position,
                                          _twoHand_min_length, _twoHand_max_length,
                                          sh_end, sh_end_max, sh_end_min,
                                          out hand_end, out len_end);

            //hand_right = newPos_right;
            //len_arm_right = newLength;

            //오른손위치가 오른손어깨의 최소거리안에 있를때 오른손위치를 다시 계산한다 
            if(sh_end_min > len_end)
            {
                this.CalcHandPos(hand_end, sh_end, sh_end_max, sh_end_min, out hand_end, out len_end);
                //hand_right = newPos_right;
                //len_arm_right = newLength;    
            }

            //-----------------------
            //손과손의 거리가 손과손최소거리 보다 작을시 왼손위치를 최소거리에 있게 한다 
            if ((hand_ori - hand_end).sqrMagnitude < _twoHand_min_length * _twoHand_min_length)
            {
                hand_ori = hand_end + VOp.Normalize(hand_ori - hand_end) * _twoHand_min_length;
                this.CalcHandPos(hand_ori, sh_ori, sh_ori_max, sh_ori_min, out hand_ori, out len_ori);

                //hand_left = newPos_left;
                //len_arm_left = newLength;
            }


        }


		//handle 
		//eHandOrigin : 고정손
		//eModelLeft : 궤적모형 
		public void Cut_TwoHand(Vector3 handle, eStandard eHandStandard,
                                out Vector3 hand_left, out Vector3 hand_right)
        {
            
            //Vector3 axis_up = _ctr_two_A0_up.position - _ctr_two_A0.position; //임시로 왼쪽 upDir 사용 
            //axis_up = VOp.Normalize(axis_up);

            float new_left_length = 0f;
            Vector3 new_leftPos;
            float new_right_length = 0f;
            Vector3 new_rightPos;


            //-----------------------
            //모델원 위치 계산 
            ControlModel first = null, second = null;
            if(eStandard.TwoHand_LeftO == _eHandStandard)
            {
                first = _ctrm_two_A0;
                second = _ctrm_two_B0;

            }
            else if(eStandard.TwoHand_RightO == _eHandStandard)
            {
                first = _ctrm_two_B0;
                second = _ctrm_two_A0;
            }


            first.SetModel();
            second.SetModel(first.tr_up.position - first.tr.position); //첫번째 조종모형의 up값으로 설정 

            this.CalcHandPos_PlaneArea(first.model, handle,
                                       _tr_shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out new_leftPos, out new_left_length);

            this.CalcHandPos_PlaneArea(second.model, handle,
                                   _tr_shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out new_rightPos, out new_right_length);


            //----------------------------
            //hand_left = new_leftPos;
            //hand_right = new_rightPos;
            //_arm_left_length = new_left_length;
            //_arm_right_length = new_right_length;
            ApplyHandPos_TwoHandLength(new_leftPos, new_left_length, new_rightPos, new_right_length,
                                       eHandStandard, _twoHand_cut_length, out hand_left, out hand_right);



        }

        //==================================================

        public void Update_Rotation_Dir(int calcMode)
        {
            Transform tr_left = null;
            Transform tr_right = null;
            Vector3 left_ori = ConstV.v3_zero;
            Vector3 right_ori = ConstV.v3_zero;
            Vector3 cut_left_end = ConstV.v3_zero;
            Vector3 cut_right_end = ConstV.v3_zero;
            Vector3 sting_left_end = ConstV.v3_zero;
            Vector3 sting_right_end = ConstV.v3_zero;


            //경사도 적용 안함
            if(0 == calcMode)
            {
                tr_left = _tr_hand_left;
                tr_right = _tr_hand_right;
                left_ori = _tr_hand_left.position;
                right_ori = _tr_hand_right.position;

                cut_left_end = _hand_left_dirPos;
                cut_right_end = _hand_right_dirPos;
                sting_left_end = _target[0].position;
                sting_right_end = _target[1].position;
            }
            //경사도 적용함 (2d z축표현) , 변환이 끝난 뷰값을 사용함 
            else if(1 == calcMode)
            {
                tr_left = _armed_left._tr_view;
                tr_right = _armed_right._tr_view;
                left_ori = _armed_left._tr_view.position;
                right_ori = _armed_right._tr_view.position;

                cut_left_end = _armed_left._view_arms_end;
                cut_right_end = _armed_right._view_arms_end;
                sting_left_end = _armed_left._view_arms_end;
                sting_right_end = _armed_right._view_arms_end;
            }

            //DebugWide.LogBlue(_armed_left + "  --  " + _armed_right);

            if (ePart.OneHand == _part_control)
            {   //한손 칼 붙이기 

                //베기
                if (eStance.Cut == _eStance_hand_left)
                {
                    Vector3 handToTarget = cut_left_end - left_ori;
                    //tr_left.rotation = Quaternion.FromToRotation(ConstV.v3_forward, handToTarget);
                    tr_left.rotation = Quaternion.FromToRotation(_foot_dir, handToTarget) * _tr_root.rotation;
                }

                if (eStance.Cut == _eStance_hand_right)
                {
                    Vector3 handToTarget = cut_right_end - right_ori;
                    //tr_right.rotation = Quaternion.FromToRotation(ConstV.v3_forward, handToTarget);
                    tr_right.rotation = Quaternion.FromToRotation(_foot_dir, handToTarget) * _tr_root.rotation;
                }
                //찌르기 

                if (eStance.Sting == _eStance_hand_left)
                {

                    //Vector3 handToTarget = sting_left_end - left_ori;
                    //Vector3 obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                    //float angleW = Geo.AngleSigned(Vector3.forward, handToTarget, obj_shaft);
                    ////float angleW = Geo.Angle360_AxisRotate(Vector3.forward, handToTarget, obj_shaft);
                    //tr_left.rotation = Quaternion.AngleAxis(angleW, obj_shaft);

                    Vector3 handToTarget = sting_left_end - left_ori;
                    //tr_left.localRotation = Quaternion.FromToRotation(_foot_dir, handToTarget );
                    //* 위의 로컬회전에 값을 넣는 코드가 원하는 결과가 안나오는 이유 : 회전결합 , 회전순서 문제 때문임
                    //  캐릭터가 180도 회전후 손의 로컬에 회전을 넣는다는 생각으로 작성하였지만 실제로는 
                    //  자식인 손의 로컬 회전량이 먼저 적용되고 부모인 캐릭터가 180도 회전하게 된다 
                    //  자식의 회전량이 먼저 적용되어서 x축 회전시 반대방향으로 회전하는 것으로 보이게 된다 
                    tr_left.rotation =  Quaternion.FromToRotation(_foot_dir, handToTarget) * _tr_root.rotation;

                }

                if (eStance.Sting == _eStance_hand_right)
                {
                    Vector3 handToTarget = sting_right_end - right_ori;
                    //tr_right.rotation = Quaternion.FromToRotation(ConstV.v3_forward, handToTarget);
                    tr_right.rotation = Quaternion.FromToRotation(_foot_dir, handToTarget) * _tr_root.rotation;
                }

            }
            if (ePart.TwoHand == _part_control)
            {   //양손 칼 붙이기
                //Vector3 hLhR = _tr_hand_right.position - _tr_hand_left.position;
                //_tr_hand_left.rotation = Quaternion.FromToRotation(Vector3.forward, hLhR);

                if(_eHandStandard == eStandard.TwoHand_LeftO )
                {
                    Vector3 hLhR = right_ori - left_ori;
                    tr_left.rotation = Quaternion.FromToRotation(_foot_dir, hLhR) * _tr_root.rotation;    
                }
                else if (_eHandStandard == eStandard.TwoHand_RightO)
                {
                    Vector3 hLhR = left_ori - right_ori;
                    tr_right.rotation = Quaternion.FromToRotation(_foot_dir, hLhR) * _tr_root.rotation;
                }

            }
        }

        public void BillBoard_Hand(ArmedInfo left , ArmedInfo right)
        {
            //Transform tr_left = _armed_left._tr_view;

            //if (ePart.TwoHand == _part_control)
            {

                //2d칼을 좌/우로 90도 세웠을때 안보이는 문제를 피하기 위해 z축 롤값을 0으로 한다  
                Vector3 temp = left._tr_view.eulerAngles;

                temp.z = 0; //<쿼터니언 회전시 무기뒤집어지는 문제 원인> 이 처리를 주석하면 수직베기시 정상처리가 된다

                //칼의 뒷면 표현
                if (Vector3.Dot(ConstV.v3_up, _tr_arm_left_up.position - _tr_hand_left.position) > 0)
                {
                    //윗면
                    left._view_spr.color = Color.white;
                    //temp.z = 0;
                }
                else
                {
                    //뒷면 
                    left._view_spr.color = Color.gray;
                    //temp.z = 180;
                }

                left._tr_view.eulerAngles = temp;

                //===============================

                temp = right._tr_view.eulerAngles;
                temp.z = 0;
                if (Vector3.Dot(ConstV.v3_up, _tr_arm_right_up.position - _tr_hand_right.position) > 0)
                {
                    //윗면
                    right._view_spr.color = Color.white;
                    //temp.z = 0;
                }
                else
                {
                    //뒷면 
                    right._view_spr.color = Color.gray;
                    //temp.z = 180;
                }

                right._tr_view.eulerAngles = temp;

            }
        }

        //손,손목 애니 
        public void Update_Ani_Wrist()
        {
            //주먹 회전 (어깨에서 손까지)
            //float angleY = Geo.Angle360(ConstV.v3_forward, (_hand_left.position - _shoulder_left.position), ConstV.v3_up);
            //_hand_left_wrist.eulerAngles = new Vector3(0, angleY, 0);
            //angleY = Geo.Angle360(ConstV.v3_forward, (_hand_right.position - _shoulder_right.position), ConstV.v3_up);
            //_hand_right_wrist.eulerAngles = new Vector3(0, angleY, 0);

            Transform view_left = _armed_left._tr_view;
            Transform view_right = _armed_right._tr_view;

            _tr_hand_left_wrist.rotation = Quaternion.FromToRotation(ConstV.v3_forward, (view_left.position - _tr_shoulder_left.position));
            Vector3 temp = _tr_hand_left_wrist.eulerAngles;
            temp.z = 0; //롤링효과 제거한다 
            _tr_hand_left_wrist.eulerAngles = temp;
            _tr_hand_left_wrist.position = view_left.position;

            _tr_hand_right_wrist.rotation = Quaternion.FromToRotation(ConstV.v3_forward, (view_right.position - _tr_shoulder_right.position));
            temp = _tr_hand_right_wrist.eulerAngles;
            temp.z = 0;
            _tr_hand_right_wrist.eulerAngles = temp;
            _tr_hand_right_wrist.position = view_right.position;
        }

        public void Draw_Shadow(ArmedInfo armed_ori)
        {
            Transform view_ori = armed_ori._tr_view;
            Vector3 arm_ori_end = armed_ori._view_arms_end;
            Vector3 arm_ori_start = armed_ori._view_arms_start;
            //Vector3 arm_ori_end = _view_target_1;
            //Vector3 arm_ori_end = view_end_pos;
            Transform arm_ori_shadow = armed_ori._tr_view_shadow;

            if (false == _active_shadow)
            {
                arm_ori_shadow.gameObject.SetActive(false);
                return;
            }
            arm_ori_shadow.gameObject.SetActive(true);

            //-----------------------------------

            //평면과 광원사이의 최소거리 
            //Vector3 object_dir = view_end - view_ori.position;
            Vector3 arm_dir = arm_ori_end - view_ori.position;
            float len_groundToObj_start, proj_groundToObj_end;

            //Vector3 end = this.CalcShadowPos(_light_dir, object_dir, _groundY, arm_ori_end, out proj_groundToObj_end);
            //Vector3 start = this.CalcShadowPos(_light_dir, _groundY, view_ori.position);


            Vector3 end = this.CalcShadowPos(_light_dir, arm_dir, _groundY, arm_ori_end, out proj_groundToObj_end);
            Vector3 start = this.CalcShadowPos(_light_dir, _groundY, arm_ori_start);
            Vector3 pos = this.CalcShadowPos(_light_dir, _groundY, view_ori.position);

            Vector3 startToEnd = end - start;
            float len_startToEnd = startToEnd.magnitude;
            //float rate = len_startToEnd / 2.4f; //창길이 하드코딩 
            float rate = len_startToEnd / armed_ori._length; //창길이 하드코딩 
            //DebugWide.LogBlue(armed_ori._length + " " + rate);

            float shader_angle = Geo.AngleSigned_AxisY(ConstV.v3_forward, startToEnd);
            //float shader_angle = Geo.Angle360(ConstV.v3_forward, startToEnd, Vector3.up);
            //DebugWide.LogBlue(shader_angle);
            arm_ori_shadow.rotation = Quaternion.AngleAxis(shader_angle, ConstV.v3_up); // 땅up벡터 축으로 회전 
            //arm_ori_shadow.position = start;
            arm_ori_shadow.position = pos;

            //높이에 따라 그림자 길이 조절 
            Vector3 scale = arm_ori_shadow.localScale;
            scale.z = rate;
            arm_ori_shadow.localScale = scale;
            //------
            //그림자 땅표면위에만 있게 하기위해 pitch 회전값 제거  
            //Vector3 temp2 = arm_ori_shadow.eulerAngles;
            //if (Vector3.Dot(ConstV.v3_up, up_ori) > 0)
            //{
            //    temp2.z = 0;
            //}
            //else
            //{
            //    temp2.z = 180f;
            //}
            //arm_ori_shadow.eulerAngles = temp2;
            //-----------------------------------  

            //DebugWide.LogBlue(len_startToEnd + "   " + (_hand_left_obj_end.position - _hand_left_obj.position).magnitude);

            //나중에 그림자 자르기 처리와 같이 사용하기 , 보기에 크게 차이가 안난다 
            //땅을 통과하는 창 자르기 
            //SpriteMesh spriteMesh = armed_ori._view_spr;
            //if (_groundY.y > arm_ori_end.y)
            //{
            //    float rate_viewLen = (arm_ori_end - end).magnitude / armed_ori._length;
            //    spriteMesh._cuttingRate.y = -rate_viewLen;
            //    spriteMesh._update_perform = true;

            //}
            //else if (0 != spriteMesh._cuttingRate.y)
            //{
                ////기존값이 남아있는 경우를 제거한다  
                //spriteMesh._cuttingRate.y = 0;
                //spriteMesh._update_perform = true;
            //}
        }

        public void Update_Shadow()
        {
            
            ArmedInfo armed_ori = null;

            //Vector3 up_ori = ConstV.v3_zero;

            if (ePart.OneHand == _part_control)
            {
                //왼쪽손
                armed_ori = _armed_left;

                //up_ori = _tr_arm_left_up.position - _tr_hand_left.position;
                Draw_Shadow(armed_ori);

                //오른쪽손
                armed_ori = _armed_right;

                //up_ori = _tr_arm_right_up.position - _tr_hand_right.position;;
                Draw_Shadow(armed_ori);
            }
            else if (ePart.TwoHand == _part_control)
            {
                if (eStandard.TwoHand_LeftO == _eHandStandard)
                {
                    armed_ori = _armed_left;

                    //up_ori = _tr_arm_left_up.position - _tr_hand_left.position;
                }
                else if (eStandard.TwoHand_RightO == _eHandStandard)
                {
                    armed_ori = _armed_right;

                    //up_ori = _tr_arm_right_up.position - _tr_hand_right.position;;
                }

                Draw_Shadow(armed_ori);
            }

        }

    }//end class


}//end namespace

