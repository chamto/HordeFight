using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test_ChampBehavior : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        GameObject champ = GameObject.Find("Champ");
        if(null != (object)champ)
        {
            champ.AddComponent<TwoHandControl>();
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}
}


public class TwoHandControl : MonoBehaviour
{
    public enum ePart
    {
        None,
        OneHand,
        TwoHand,

        TwoHand_LeftO, //왼손기준이 되는 잡기(왼손이 고정)
        TwoHand_RightO, //오른손기준이 되는 잡기(오른손이 고정)

    }

    //------------------------------------------------------
    //debug
    public bool _A_debug_mode = false;
    public LineRenderer _debugLine = null;


    //------------------------------------------------------

    public Transform _tbody_dir = null;
    public Transform _shoulder_left = null;
    public Transform _shoulder_right = null;
    public Transform _hand_left = null;
    public Transform _hand_right = null;
    public Transform _object_left = null;
    public Transform _object_right = null;
    public Transform _odir_left = null; //한손으로 쥐고 있는 물체의 방향.
    public Transform _odir_right = null;


    //핸들 
    public Transform _HANDLE_staff = null;
    public Transform _HANDLE_left = null;   //핸들
    public Transform _HANDLE_right = null;
    public Transform _HANDLE_twoHand = null;
    public Transform _HANDLE_oneHand = null;

    //목표
    public Transform _target_1 = null;
    public Transform _target_2 = null;

    public SpriteRenderer _spr_object_left = null;
    public SpriteRenderer _spr_object_right = null;

    public Transform _hand_left_spr = null;
    public Transform _hand_right_spr = null;

    public float _shoulder_length = 0f;
    public float _arm_left_length = 0.5f;
    public float _arm_left_min_length = 0.2f;
    public float _arm_left_max_length = 1f;
    public float _arm_right_length = 0.7f;
    public float _arm_right_min_length = 0.2f;
    public float _arm_right_max_length = 1f;
    //public float _twoHand_length = 0.15f;
    public float _twoHand_length = 0.5f;

    public Vector3 _body_dir = UtilGS9.ConstV.v3_zero;
    //public ePart _part_control = ePart.TwoHand;
    public ePart _part_control = ePart.OneHand;
    public ePart _eHandOrigin = ePart.TwoHand_LeftO; //고정으로 잡는 손지정 


    //경로모델
    public Geo.Model.eKind _eModelKind_Left = Geo.Model.eKind.Cylinder;
    public Geo.Model.eKind _eModelKind_Right = Geo.Model.eKind.Cylinder;

    private Geo.Model _Model_left = new Geo.Model();
    private Geo.Model _Model_right = new Geo.Model();

    //----------------------------------------------------
    public bool _A_armLength_max_min = false;

    public bool _A_action_cut = false;
    public bool _A_action_sting = true; //찌르기 동작 
    //----------------------------------------------------


    public string _1____________________ = "";
    public bool  _A_shoulder_autoRotate = false;
    public float _angle_shoulderLeft_autoRotate = -10f; //왼쪽 어깨 자동회전 각도량
    public float _angle_shoulderRight_autoRotate = -10f; //오른쪽 어깨 자동회전 각도량

    //======================================================
    public string _2_1__________________ = "";
    public bool _A_body_aroundRotate2 = false;

    //양손모드 조종원 크기 
    public float _radius_circle_left = 0.5f;
    public float _radius_circle_right = 0.8f;

    //왼손모드 조종원 크기
    public float _radius_circle_A0 = 0f;
    public float _radius_circle_A1 = 0f;

    //오른손모드 조종원 크기 
    public float _radius_circle_B0 = 0f;
    public float _radius_circle_B1 = 0f;

    //======================================================

    //양손 좌표축
    private Transform _L2R_axis_o = null;
    private Transform _L2R_axis_up = null;
    private Transform _L2R_axis_right = null;
    private Transform _L2R_axis_forward = null;

    //왼손 좌표축
    private Transform _left_axis_o = null;
    private Transform _left_axis_up = null;
    private Transform _left_axis_right = null;
    private Transform _left_axis_forward = null;

    //오른손 좌표축 
    private Transform _right_axis_o = null;
    private Transform _right_axis_up = null;
    private Transform _right_axis_right = null;
    private Transform _right_axis_forward = null;

    //======================================================

    //양손 조종용 경로원
    private Transform _pos_circle_left = null;
    private Transform _pos_circle_right = null;
    private Transform _edge_circle_left = null;
    private Transform _edge_circle_right = null;
    private Transform _highest_circle_left = null;
    private Transform _highest_circle_right = null;
    private Transform _far_edge_circle_left = null;
    private Transform _far_edge_circle_right = null;
    private Transform _tornado_angle_left = null;
    private Transform _tornado_angle_right = null;
    private Transform _tornado_unlace_left = null;
    private Transform _tornado_unlace_right = null;

    //왼손 조종용 경로원
    private Transform _pos_circle_A0 = null;
    private Transform _pos_circle_A1 = null;
    private Transform _edge_circle_A0 = null;
    private Transform _edge_circle_A1 = null;
    private Transform _highest_circle_A0 = null;
    private Transform _highest_circle_A1 = null;

    //오른손 조종용 경로 
    private Transform _pos_circle_B0 = null;
    private Transform _pos_circle_B1 = null;
    private Transform _edge_circle_B0 = null;
    private Transform _edge_circle_B1 = null;


    //양손조종. 왼손 회오리 경로원
    private Transform _TL2R_pos_circle_left = null;
    private Transform _TL2R_edge_circle_left = null;
    private Transform _TL2R_highest_circle_left = null;
    private Transform _TL2R_angle_circle_left = null;
    private Transform _TL2R_unlace_circle_left = null;


    //======================================================


    public string _5_1___________________ = "";
    public bool _A_handleStaff_control = false;
    public bool _switch_cutAndSting = true; //cut : true , sting : false
    public Transform _hs_objectDir = null; 
    public Transform _hs_standard = null;


    public string _5_2___________________ = "";
    public bool _A_hand_control_2 = false;



    //public string _6_1___________________ = "";
    //public bool _A_oneHand_control_1 = false;



	private void Start()
	{
        //if(true == _A_debug_mode)
        {
            GameObject dg = GameObject.Find("debug");
            if(null != dg)
            {
                _debugLine = dg.GetComponentInChildren<LineRenderer>(true);
                _debugLine.gameObject.SetActive(true);    
            }
            //_debugLine = GameObject.Find("debugLine").GetComponent<LineRenderer>();

        }

        _Model_left.branch = Geo.Model.eBranch.arm_left_0;
        _Model_right.branch = Geo.Model.eBranch.arm_right_0;

        //--------------------------------------------------

        _tbody_dir = GameObject.Find("body_dir").transform;
        _shoulder_left = GameObject.Find("shoulder_left").transform;
        _shoulder_right = GameObject.Find("shoulder_right").transform;
        _hand_left = GameObject.Find("hand_left").transform;
        _hand_right = GameObject.Find("hand_right").transform;
        _object_left = GameObject.Find("object_left").transform;
        _object_right = GameObject.Find("object_right").transform;
        _odir_left = GameObject.Find("odir_left").transform;
        _odir_right = GameObject.Find("odir_right").transform;

        //핸들
        _HANDLE_staff = GameObject.Find("handle_staff").transform; //핸들 
        _HANDLE_left = GameObject.Find("handle_left").transform; //핸들 
        _HANDLE_right = GameObject.Find("handle_right").transform;
        _HANDLE_twoHand = GameObject.Find("handle_twoHand").transform;
        _HANDLE_oneHand = GameObject.Find("handle_oneHand").transform;

        //목표
        _target_1 = GameObject.Find("target_1").transform;
        _target_2 = GameObject.Find("target_2").transform;

        //_spr_object_left = _object_left.GetComponentInChildren<SpriteRenderer>();
        //_spr_object_right = _object_right.GetComponentInChildren<SpriteRenderer>();

        _hand_left_spr = _hand_left.GetComponentInChildren<SpriteRenderer>().transform;
        _hand_right_spr = _hand_right.GetComponentInChildren<SpriteRenderer>().transform;


        //==================================================
        //조종항목 - AroundCircle
        _pos_circle_left = GameObject.Find("pos_circle_left").transform;
        _pos_circle_right= GameObject.Find("pos_circle_right").transform;
        _edge_circle_left = GameObject.Find("edge_circle_left").transform;
        _edge_circle_right = GameObject.Find("edge_circle_right").transform;
        _highest_circle_left = GameObject.Find("highest_circle_left").transform;
        _highest_circle_right = GameObject.Find("highest_circle_right").transform;
        _far_edge_circle_left = GameObject.Find("far_edge_circle_left").transform;
        _far_edge_circle_right = GameObject.Find("far_edge_circle_right").transform;
        _tornado_angle_left = GameObject.Find("tornado_angle_left").transform;
        _tornado_angle_right = GameObject.Find("tornado_angle_right").transform;
        _tornado_unlace_left = GameObject.Find("tornado_unlace_left").transform;
        _tornado_unlace_right = GameObject.Find("tornado_unlace_right").transform;

        _pos_circle_A0 = GameObject.Find("pos_circle_A0").transform;
        _pos_circle_A1 = GameObject.Find("pos_circle_A1").transform;
        _edge_circle_A0 = GameObject.Find("edge_circle_A0").transform;
        _edge_circle_A1 = GameObject.Find("edge_circle_A1").transform;
        _highest_circle_A0 = GameObject.Find("highest_circle_A0").transform;
        _highest_circle_A1 = GameObject.Find("highest_circle_A1").transform;


        _pos_circle_B0 = GameObject.Find("pos_circle_B0").transform;
        _pos_circle_B1 = GameObject.Find("pos_circle_B1").transform;
        _edge_circle_B0 = GameObject.Find("edge_circle_B0").transform;
        _edge_circle_B1 = GameObject.Find("edge_circle_B1").transform;

        //==================================================

        //=======
        //손조종 1
        _hs_objectDir = GameObject.Find("hs_objectDir").transform;
        _hs_standard = GameObject.Find("hs_standard").transform;
        //_hc1_axis_o = GameObject.Find("hc1_axis_o").transform;

        //=======
        //양손 좌표축
        _L2R_axis_o = GameObject.Find("L2R_axis_o").transform;
        _L2R_axis_up = GameObject.Find("L2R_axis_up").transform;
        _L2R_axis_right = GameObject.Find("L2R_axis_right").transform;
        _L2R_axis_forward = GameObject.Find("L2R_axis_forward").transform;

        //왼손 좌표축
        _left_axis_o = GameObject.Find("left_axis_o").transform;
        _left_axis_up = GameObject.Find("left_axis_up").transform;
        _left_axis_right = GameObject.Find("left_axis_right").transform;
        _left_axis_forward = GameObject.Find("left_axis_forward").transform;

        //오른손 좌표축
        _right_axis_o = GameObject.Find("right_axis_o").transform;
        _right_axis_up = GameObject.Find("right_axis_up").transform;
        _right_axis_right = GameObject.Find("right_axis_right").transform;
        _right_axis_forward = GameObject.Find("right_axis_forward").transform;

        //=======
        //양손조종. 왼손 회오리 경로
        _TL2R_pos_circle_left = GameObject.Find("TL2R_pos_circle_left").transform;
        _TL2R_edge_circle_left = GameObject.Find("TL2R_edge_circle_left").transform;
        _TL2R_highest_circle_left = GameObject.Find("TL2R_highest_circle_left").transform;
        _TL2R_angle_circle_left = GameObject.Find("TL2R_angle_circle_left").transform;
        _TL2R_unlace_circle_left = GameObject.Find("TL2R_unlace_circle_left").transform;

	}


    public Vector3 __prev_handL_Pos = UtilGS9.ConstV.v3_zero;
    public Vector3 __prev_hLsL = UtilGS9.ConstV.v3_zero;
	private void Update()
	{
        //몸 방향값 갱신 
        _body_dir = (_tbody_dir.position - transform.position).normalized;
        //==================================================

        if (null == (object)_shoulder_left || null == (object)_shoulder_right) return;

        Vector3 shLR = _shoulder_left.position - _shoulder_right.position;
        _shoulder_length = shLR.magnitude;

        //==================================================

        if (null == (object)_hand_left || null == (object)_hand_right) return;

        //==================================================


        //손길이 제약 
        //Vector3 hLsL = (_hand_left.position - _shoulder_left.position);
        //Vector3 hRsR = (_hand_right.position - _shoulder_right.position);
        //Vector3 n_hLsL = hLsL.normalized;
        //Vector3 n_hRsR = hRsR.normalized;

        //==================================================
        if (true == _A_action_cut)
        {
            //한손모드
            if (ePart.OneHand == _part_control)
            { 

                //주변원 반지름 갱신
                _radius_circle_A0 = (_pos_circle_A0.position - _edge_circle_A0.position).magnitude;
                _radius_circle_A1 = (_pos_circle_A1.position - _edge_circle_A1.position).magnitude;
                _radius_circle_B0 = (_pos_circle_B0.position - _edge_circle_B0.position).magnitude;
                _radius_circle_B1 = (_pos_circle_B1.position - _edge_circle_B1.position).magnitude;

                //====================

                Vector3 handle = _HANDLE_left.position;
                Vector3 axis_up = _left_axis_up.position - _left_axis_o.position;

                Vector3 newPos = Vector3.zero;
                float newLength = 0f;
                //------------------------------------------

                //회오리원 위치 계산 
                //axis_up = Geo.Trans_UnlaceDir(_TL2R_unlace_circle_left.position - _TL2R_pos_circle_left.position, axis_up, _TL2R_highest_circle_left.position - _TL2R_pos_circle_left.position);
                //float tonadoRadius = (_TL2R_edge_circle_left.position - _TL2R_pos_circle_left.position).magnitude;
                //this.CalcHandPos_TornadoCircle(handle, axis_up, _TL2R_pos_circle_left.position, tonadoRadius, _TL2R_highest_circle_left.position,
                                         //_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                         //out newPos, out newLength);

                //변형원 위치 계산 
                //this.CalcHandPos_DeformationCircle(handle, axis_up, _pos_circle_A0.position, _radius_circle_A0, _highest_circle_A0.position,
                                             //_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                             //out newPos, out newLength);

                //주변원 위치 계산 
                //this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_A0.position, _radius_circle_A0,
                                             //_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                             //out newPos, out newLength);
                _arm_left_length = newLength;
                _hand_left.position = newPos;

                this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_A1.position, _radius_circle_A1,
                                              _shoulder_left.position, 10000, _arm_left_min_length,
                                             out newPos, out newLength);
                
                _odir_left.position = newPos;
                //------------------------------------------
                handle = _HANDLE_right.position;
                axis_up = _right_axis_up.position - _right_axis_o.position;
                this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_B0.position, _radius_circle_B0,
                                             _shoulder_right.position, _arm_right_max_length, _arm_right_min_length,
                                             out newPos, out newLength);
                _arm_right_length = newLength;
                _hand_right.position = newPos;

                this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_B1.position, _radius_circle_B1,
                                              _shoulder_right.position, 10000, _arm_right_min_length,
                                             out newPos, out newLength);

                _odir_right.position = newPos;


            }
        }

        //찌르기 
        if(true == _A_action_sting)
        {
            //한손모드
            if (ePart.OneHand == _part_control)
            {
                Vector3 newRightPos;
                Vector3 newLeftPos;
                float newRightLength;
                float newLeftLength;
                this.CalcHandPos(_HANDLE_left.position, _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out newLeftPos, out newLeftLength);
                _hand_left.position = newLeftPos;
                _arm_left_length = newLeftLength;


                this.CalcHandPos(_HANDLE_right.position, _shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out newRightPos, out newRightLength);
                _hand_right.position = newRightPos;
                _arm_right_length = newRightLength;
            }

            //==============================================

            //양손모드
            if (ePart.TwoHand == _part_control)
            {

                //왼손을 핸들로 조종하기 
                Vector3 newRightPos;
                Vector3 newLeftPos;
                float newLeftLength;
                this.CalcHandPos(_HANDLE_twoHand.position, _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out newLeftPos, out newLeftLength);
                _hand_left.position = newLeftPos;
                _arm_left_length = newLeftLength;

                //<방식1> target_1 에 따라 오른손 위치 결정하기 - target_1 에 봉이 도달하지 못하는 경우가 있음 (오른손위치계산 => 오른손제약범위 적용)
                _hand_right.position = newLeftPos + (_target_1.position - newLeftPos).normalized * _twoHand_length;

                Vector3 handToTarget = (_target_1.position - newLeftPos);
                Vector3 n_handToTarget = handToTarget.normalized;
                Vector3 posOnMaxCircle;
                float newlength_twoHand = _arm_right_max_length - (_shoulder_right.position - newLeftPos).magnitude;

                //----------------------------------------------

                if (true == UtilGS9.Geo.IntersectRay2(_shoulder_right.position, _arm_right_max_length, newLeftPos, n_handToTarget, out posOnMaxCircle))
                {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 있다  

                    if (newlength_twoHand > 0)
                    {   //왼손이 오른손 최대 범위 안에 있는 경우

                        if ((newLeftPos - posOnMaxCircle).magnitude > _twoHand_length)
                        {
                            //DebugWide.LogBlue("111");
                            newRightPos = newLeftPos + n_handToTarget * _twoHand_length;
                        }
                        else
                        {
                            //DebugWide.LogBlue("222");
                            newRightPos = posOnMaxCircle;
                        }
                    }
                    else
                    {   //왼손이 오른손 최대 범위 밖에 있는 경우 

                        newRightPos = newLeftPos + n_handToTarget * _twoHand_length;
                        //newPos = newLeftPos + (posOnMaxCircle - newLeftPos).normalized * _twoHand_length;
                        if ((newRightPos - _shoulder_right.position).sqrMagnitude > _arm_right_max_length * _arm_right_max_length)
                        {
                            //DebugWide.LogBlue("333");
                            newRightPos = posOnMaxCircle;
                        }
                        else
                        {
                            //DebugWide.LogBlue("444");
                        }

                    }


                    //chamto debug test
                    //_debugLine.SetPosition(0, _hand_left.position);
                    //_debugLine.SetPosition(1, posOnMaxCircle);

                }
                else
                {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 없다   :  목표와 왼손 사이의 직선경로가 오른손 최대범위에 닿지 않는 경우

                    Vector3 targetToRSd = (_shoulder_right.position - newLeftPos);
                    Vector3 n_targetToRSd = targetToRSd.normalized;
                    float length_contactPt = targetToRSd.sqrMagnitude - _arm_right_max_length * _arm_right_max_length;
                    length_contactPt = (float)System.Math.Sqrt(length_contactPt);
                    float proj_cos = length_contactPt / targetToRSd.magnitude;

                    //-----------------------

                    //proj_cos = Mathf.Clamp01(proj_cos); //0~1사이의 값만 사용
                    float angleC = Mathf.Acos(proj_cos) * Mathf.Rad2Deg;
                    Vector3 shaft_l = Vector3.Cross(newLeftPos, _shoulder_right.position);
                    newRightPos = newLeftPos + Quaternion.AngleAxis(-angleC, shaft_l) * n_targetToRSd * length_contactPt;

                    //-----------------------


                }

                //-----------------------
                Vector3 leftToRight = newRightPos - newLeftPos;
                Vector3 shaft_rot = Vector3.Cross(newLeftPos, _shoulder_right.position);
                Vector3 rotateDir = Quaternion.AngleAxis(-90f, shaft_rot) * leftToRight.normalized;
                float length_min_twoHand = 0.1f;
                if (leftToRight.magnitude < length_min_twoHand)
                {   //양손 최소거리 일떄 자연스런 회전 효과를 준다 (미완성) 

                    newLeftPos = newLeftPos + rotateDir * 0.08f;
                    //_handle_leftToRight.position = newLeftPos;
                }
                //-----------------------

                _hand_left.position = newLeftPos;
                _hand_right.position = newRightPos;
                _arm_right_length = (_hand_right.position - _shoulder_right.position).magnitude;
                if (_arm_right_length > _arm_right_max_length)
                    _arm_right_length = _arm_right_max_length;


            }
        }


        //==================================================
        //왼손,오른손 자동 회전 테스트
        if(true == _A_shoulder_autoRotate)
        {
            
            Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
            Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
            Vector3 slTohl = _hand_left.position - _shoulder_left.position;
            Vector3 srTohr = _hand_right.position - _shoulder_right.position;
            slTohl.Normalize();
            srTohr.Normalize();

            //axis_forward = Quaternion.AngleAxis(45f, axis_up) * axis_forward;
            //기준축방향 좌우90도 도달시 회전 방향을 바꾸어 준다. 
            //if (Vector3.Dot(axis_forward, slTohl) < 0.1f)
            //{
            //    _angle_shoulderLeft_autoRotate *= -1f;
            //}
            //if (Vector3.Dot(axis_forward, srTohr) < 0.1f)
            //{
            //    _angle_shoulderRight_autoRotate *= -1f;
            //}

            _shoulder_right.Rotate(axis_up, _angle_shoulderRight_autoRotate, Space.World);
            _shoulder_left.Rotate(axis_up, _angle_shoulderLeft_autoRotate, Space.World);



        }


        //==================================================

        //주변원 모드2 
        if (true == _A_body_aroundRotate2)
        {

            //주변원 크기 갱신 
            _radius_circle_left = (_pos_circle_left.position - _edge_circle_left.position).magnitude;
            _radius_circle_right = (_pos_circle_right.position - _edge_circle_right.position).magnitude;

            //=====================

            Vector3 handle = _HANDLE_twoHand.position;
            Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
            Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;


            Vector3 newPos = Vector3.zero;
            float newLength = 0f;
            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_left.position, _radius_circle_left,
                                         _shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                         out newPos, out newLength);
            _arm_left_length = newLength;
            _hand_left.position = newPos;

            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_right.position, _radius_circle_right,
                                         _shoulder_right.position, _arm_right_max_length, _arm_right_min_length,
                                         out newPos, out newLength);
            


            _arm_right_length = newLength;
            _hand_right.position = newPos;


            //----------------------------

            Vector3 twoHand = ( _hand_right.position - _hand_left.position);
            Vector3 n_twoHand = twoHand.normalized;

            //왼손으로부터 오른손의 지정된 거리에 맞게 위치 계산
            newPos = _hand_left.position + n_twoHand * _twoHand_length;
            Vector3 sdToHand = (newPos - _shoulder_right.position);
            float length_sdToHand = sdToHand.magnitude;
            Vector3 n_sdToHand = sdToHand / length_sdToHand;
            newLength = length_sdToHand;
            if (length_sdToHand > _arm_right_max_length)
            {   //오른손 위치가 오른손의 최대범위를 벗어난 경우 
                newLength = _arm_right_max_length;
                newPos = _shoulder_right.position + n_sdToHand * newLength;
            }else if(length_sdToHand < _arm_right_min_length)
            {   //오른손 위치가 오른손의 최소범위를 벗어난 경우 
                newLength = _arm_right_min_length;
                newPos = _shoulder_right.position + n_sdToHand * newLength;
            }
                
            _arm_right_length = newLength;
            _hand_right.position = newPos;
        }

        //==================================================


        //손 움직임 만들기 
        if (true == _A_handleStaff_control)
        {
            HandDirControl_CutAndSting();
        }
        //if(true == _A_hand_control_2)
        //{
        //    //코사인 제2법칙 공식을 사용하는 방식 
        //    if (ePart.TwoHand_LeftO == _part_control)
        //        TwoHandControl2_Left();
        //    if (ePart.TwoHand_LeftO == _part_control)
        //        TwoHandControl2_Right();    
        //}
        //if(true == _A_oneHand_control_1)
        //{
        //    if(ePart.OneHand == _part_control)
        //    {
        //        this.OneHandleControl1_Left();
        //        this.OneHandleControl1_Right();
        //    }
        //}


        //DebugWide.LogBlue(shaft + "    angle: " +angleC +"  a:" +a+ "   b:" +b+ "   c:" + c);
        //DebugWide.LogBlue(newPos_hR.x + "  " + newPos_hR.z + "    :" + angleC + "   p:" + (a+b-c));
        //DebugWide.LogBlue(angleC + " a : " + Quaternion.FromToRotation(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position).eulerAngles.y);
        //DebugWide.LogBlue(angleC + " b : " + Vector3.SignedAngle(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position, Vector3.up));


        //==================================================
        //손에 칼 붙이기 3d

        if (ePart.OneHand == _part_control)
        {   //한손 칼 붙이기 

            if (true == _A_action_cut)
            {
                Vector3 handToTarget = _odir_left.position - _hand_left.position;
                Vector3 obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                float angleW = Vector3.SignedAngle(Vector3.forward, handToTarget, obj_shaft);
                _object_left.rotation = Quaternion.AngleAxis(angleW, obj_shaft);    

                //======

                handToTarget = _odir_right.position - _hand_right.position;
                obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                angleW = Vector3.SignedAngle(Vector3.forward, handToTarget, obj_shaft);
                _object_right.rotation = Quaternion.AngleAxis(angleW, obj_shaft);    
            }

            if(true == _A_action_sting)
            {
                Vector3 handToTarget = _target_1.position - _hand_left.position;
                Vector3 obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                float angleW = Vector3.SignedAngle(Vector3.forward, handToTarget, obj_shaft);
                _object_left.rotation = Quaternion.AngleAxis(angleW, obj_shaft);    

                handToTarget = _target_2.position - _hand_right.position;
                obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                angleW = Vector3.SignedAngle(Vector3.forward, handToTarget, obj_shaft);
                _object_right.rotation = Quaternion.AngleAxis(angleW, obj_shaft);        
            }

        }else
        {   //양손 칼 붙이기
            Vector3 hLhR = _hand_right.position - _hand_left.position;
            Vector3 obj_shaft = Vector3.Cross(Vector3.forward, hLhR);
            //angleC의 각도가 0이 나올 경우 외적값이 0이 된다. 각도가 0일때 물건을 손에 붙이는 계산이 안되는 문제가 발생함
            //물건 기준으로 외적값을 구해 사용하면 문제가 해결됨 
            //Vector3 obj_up = Vector3.Cross(obj_shaft, hLhR);

            float angleW = Vector3.SignedAngle(Vector3.forward, hLhR, obj_shaft);
            _object_left.rotation = Quaternion.AngleAxis(angleW, obj_shaft);

            //2d칼을 좌/우로 90도 세웠을때 안보이는 문제를 피하기 위해 z축 롤값을 0으로 한다  
            Vector3 temp = _object_left.eulerAngles;
            temp.z = 0;
            _object_left.eulerAngles = temp;

            //칼의 뒷면 표현 - 연구필요 
            //if(Vector3.Dot(Vector3.up, obj_up) < 0)
            //    _spr_object_left.color = Color.gray;
            //else
                //_spr_object_left.color = Color.white;

        }


        //주먹 회전 (어깨에서 손까지)
        float angleY = Vector3.SignedAngle(Vector3.forward, (_hand_left.position - _shoulder_left.position), Vector3.up);
        _hand_left_spr.eulerAngles = new Vector3(90,angleY,0);
        angleY = Vector3.SignedAngle(Vector3.forward, (_hand_right.position - _shoulder_right.position), Vector3.up);
        _hand_right_spr.eulerAngles = new Vector3(90, angleY, 0);

        //==================================================
        //스프라이트 칼 뒷면느낌 나게 색 설정 

        //if (_object_left.rotation.eulerAngles.x < -90f)
        //    _spr_object_left.color = Color.black;
        //else
            //_spr_object_left.color = Color.white;

        //==================================================
	}

    //==============================================================
    //==============================================================
    //==============================================================


    //어깨범위와 선분의 교차위치를 구한다. 어깨범위의 최소범위는 적용안됨 
    public bool CalcHandPos_LineSegment(Vector3 line_origin, Vector3 line_dir, float line_length,
                                        Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                        out Vector3 newHand_pos, out float newArm_length)
    {

        Vector3 n_line_dir = line_dir.normalized;
        Vector3 posOnMaxCircle;
        float sqr_arm_max_length = arm_max_length * arm_max_length;
        bool result = UtilGS9.Geo.IntersectRay2(shoulder_pos, arm_max_length, line_origin, n_line_dir, out posOnMaxCircle);

        //----------------------------------------------

        if (true == result)
        {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 있다  

            if((sqr_arm_max_length - (shoulder_pos - line_origin).sqrMagnitude) > 0)
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

            Vector3 targetToRSd = (shoulder_pos - line_origin);
            Vector3 n_targetToRSd = targetToRSd.normalized;
            float length_contactPt = targetToRSd.sqrMagnitude - sqr_arm_max_length;
            length_contactPt = (float)System.Math.Sqrt(length_contactPt);
            float proj_cos = length_contactPt / targetToRSd.magnitude;

            //-----------------------

            //proj_cos = Mathf.Clamp01(proj_cos); //0~1사이의 값만 사용
            float angleC = Mathf.Acos(proj_cos) * Mathf.Rad2Deg;
            Vector3 shaft_l = Vector3.Cross(line_origin, shoulder_pos);
            newHand_pos = line_origin + Quaternion.AngleAxis(-angleC, shaft_l) * n_targetToRSd * length_contactPt;

            //-----------------------


        }


        newArm_length = (newHand_pos - shoulder_pos).magnitude;
        if (newArm_length > arm_max_length)
            newArm_length = arm_max_length;


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
        Vector3 n_shToHandle = sdToHandle / length_sdToHandle;
        newArm_length = length_sdToHandle;
        newHand_pos = handle;


        if(length_sdToHandle < arm_min_length)
        {
            //DebugWide.LogBlue("2 dsdd  "  + length_sdToHandle + "  " + arm_min_length); //test
            newArm_length = arm_min_length;
            newHand_pos = shoulder_pos + n_shToHandle * newArm_length;
            inCircle = false;
            //DebugWide.LogBlue("1 dsdd  " + (newHand_pos - shoulder_pos).magnitude); //test
        }
        else if(length_sdToHandle > arm_max_length)
        {
            newArm_length = arm_max_length;
            newHand_pos = shoulder_pos + n_shToHandle * newArm_length;
            inCircle = false;
        }

        return inCircle;
    }

    public void CalcHandPos_AroundCircle(Vector3 handle , Vector3 circle_up , Vector3 circle_pos , float circle_radius , 
                                        Vector3 shoulder_pos , float arm_max_length , float arm_min_length , 
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


    public void SetModel_CurValue(Geo.Model model, Vector3 upDir)
    {
        
        switch (model.kind)
        {
            case Geo.Model.eKind.FreePlane:
                {
                    if (true == model.IsLeft())
                    {
                        model.freePlane.Set(_pos_circle_left.position);
                    }
                    if (true == model.IsRight())
                    {
                        model.freePlane.Set(_pos_circle_right.position);
                    }
                }
                break;
            case Geo.Model.eKind.Circle:
                {
                    if (true == model.IsLeft())
                    {
                        model.circle.Set(_pos_circle_left.position, _radius_circle_left);
                    }
                    if (true == model.IsRight())
                    {
                        model.circle.Set(_pos_circle_right.position, _radius_circle_right);
                    }
                }
                break;
            case Geo.Model.eKind.DeformationCircle:
                {
                    if (true == model.IsLeft())
                    {
                        model.deformationCircle.Set(upDir, _pos_circle_left.position, _radius_circle_left, _highest_circle_left.position, 1);
                    }
                    if (true == model.IsRight())
                    {
                        model.deformationCircle.Set(upDir, _pos_circle_right.position, _radius_circle_right, _highest_circle_right.position, 1);
                    }
                }
                break;
            case Geo.Model.eKind.Tornado:
                {
                    if (true == model.IsLeft())
                    {
                        model.tornado.Set(upDir, _pos_circle_left.position, _radius_circle_left, _highest_circle_left.position, _tornado_unlace_left.position, _tornado_angle_left.localPosition.x);
                    }
                    if (true == model.IsRight())
                    {
                        model.tornado.Set(upDir, _pos_circle_right.position, _radius_circle_right, _highest_circle_right.position, _tornado_unlace_right.position, _tornado_angle_right.localPosition.x);
                    }
                }
                break;
            case Geo.Model.eKind.Cylinder:
                {
                    
                    if(true == model.IsLeft())
                    {
                        float radius_far = (_highest_circle_left.position - _far_edge_circle_left.position).magnitude;
                        model.cylinder.Set(_pos_circle_left.position, _radius_circle_left, _highest_circle_left.position, radius_far);    
                    }
                    if (true == model.IsRight())
                    {
                        float radius_far = (_highest_circle_right.position - _far_edge_circle_right.position).magnitude;
                        model.cylinder.Set(_pos_circle_right.position, _radius_circle_right, _highest_circle_right.position, radius_far);
                    }

                }
                break;
        }
    }

    //대상 도형모델의 평면공간에 투영한 결과를 반환한다.
    //평면공간에 투영이 불가능한 경우에는 어깨와 평면공간의 최소거리의 위치를 반환한다
    public void CalcHandPos_PlaneArea(Geo.Model model, Vector3 handle, Vector3 upDir,
                                        Vector3 shoulder_pos, float arm_max_length, float arm_min_length,
                                        out Vector3 newHand_pos, out float newArm_length)
    {
        
        Vector3 md_origin = model.origin;

        Vector3 aroundCalcPos = UtilGS9.ConstV.v3_zero;

        //===== 1차 계산
        switch(model.kind)
        {
            case Geo.Model.eKind.FreePlane:
                {
                    aroundCalcPos = model.freePlane.CollisionPos(handle, upDir);
                }
                break;
            case Geo.Model.eKind.Circle:
                {
                    aroundCalcPos = model.circle.CollisionPos(handle, upDir);
                }
                break;
            case Geo.Model.eKind.DeformationCircle:
                {
                    aroundCalcPos = model.deformationCircle.CollisionPos_Fast(handle, upDir);
                }
                break;
            case Geo.Model.eKind.Tornado:
                {
                    //todo 적용하기 : 회오리원 감기는 방향 변환  
                    //upDir = Geo.Tornado.Trans_UnlaceDir(_tornado_angle_left.position - _pos_circle_left.position, upDir);

                    aroundCalcPos = model.tornado.CollisionPos(handle, upDir);
                }
                break;
            case Geo.Model.eKind.Cylinder:
                {
                    Vector3 upDir2;
                    aroundCalcPos = model.cylinder.CollisionPos(handle, upDir, out upDir2);
                    upDir = upDir2;
                }
                break;
        }


        //===== 어깨원 투영
        Vector3 proj_sdToOrigin = upDir * Vector3.Dot((md_origin - shoulder_pos), upDir) / upDir.sqrMagnitude;
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

    public void Cut_OneHand()
    {
        if (ePart.OneHand == _part_control)
        {

            //주변원 반지름 갱신
            _radius_circle_A0 = (_pos_circle_A0.position - _edge_circle_A0.position).magnitude;
            _radius_circle_A1 = (_pos_circle_A1.position - _edge_circle_A1.position).magnitude;
            _radius_circle_B0 = (_pos_circle_B0.position - _edge_circle_B0.position).magnitude;
            _radius_circle_B1 = (_pos_circle_B1.position - _edge_circle_B1.position).magnitude;

            //====================

            Vector3 handle = _HANDLE_left.position;
            Vector3 axis_up = _left_axis_up.position - _left_axis_o.position;

            Vector3 newPos = Vector3.zero;
            float newLength = 0f;
            //------------------------------------------

            //회오리원 위치 계산 
            //axis_up = Geo.Trans_UnlaceDir(_TL2R_unlace_circle_left.position - _TL2R_pos_circle_left.position, axis_up, _TL2R_highest_circle_left.position - _TL2R_pos_circle_left.position);
            //float tonadoRadius = (_TL2R_edge_circle_left.position - _TL2R_pos_circle_left.position).magnitude;
            //this.CalcHandPos_TornadoCircle(handle, axis_up, _TL2R_pos_circle_left.position, tonadoRadius, _TL2R_highest_circle_left.position,
            //_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
            //out newPos, out newLength);

            //변형원 위치 계산 
            //this.CalcHandPos_DeformationCircle(handle, axis_up, _pos_circle_A0.position, _radius_circle_A0, _highest_circle_A0.position,
            //_shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
            //out newPos, out newLength);

            //주변원 위치 계산 
            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_A0.position, _radius_circle_A0,
            _shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
            out newPos, out newLength);
            _arm_left_length = newLength;
            _hand_left.position = newPos;

            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_A1.position, _radius_circle_A1,
                                          _shoulder_left.position, 10000, _arm_left_min_length,
                                         out newPos, out newLength);

            _odir_left.position = newPos;
            //------------------------------------------
            handle = _HANDLE_right.position;
            axis_up = _right_axis_up.position - _right_axis_o.position;
            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_B0.position, _radius_circle_B0,
                                         _shoulder_right.position, _arm_right_max_length, _arm_right_min_length,
                                         out newPos, out newLength);
            _arm_right_length = newLength;
            _hand_right.position = newPos;

            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_circle_B1.position, _radius_circle_B1,
                                          _shoulder_right.position, 10000, _arm_right_min_length,
                                         out newPos, out newLength);

            _odir_right.position = newPos;


            //===============

            //float new_leftLength = 0f;
            //Vector3 new_leftPos;
            //float new_rightLength = 0f;
            //Vector3 new_rightPos;

            ////-----------------------


            ////_Model_left.kind = eModelLeft;
            //this.SetModel_CurValue(_Model_left, axis_up);
            //this.CalcHandPos_PlaneArea(_Model_left, handle, axis_up,
            //                           _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out new_leftPos, out new_leftLength);



            ////_Model_right.kind = eModelRight;
            //this.SetModel_CurValue(_Model_right, axis_up);
            //this.CalcHandPos_PlaneArea(_Model_right, handle, axis_up,
                               //        _shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out new_rightPos, out new_rightLength);


        }
    }

    private float __backup_radius_circle_left = -1f;
    private float __backup_radius_circle_right = -1f;
    public void HandDirControl_CutAndSting()
    {
        if (_part_control == ePart.OneHand)
        {

            if (false == _switch_cutAndSting)
            {   //찌르기
            }else
            {   //베기

                Cut_OneHand();
            }

        }

        //==================================================

        if (_part_control == ePart.TwoHand)
        {
            if (false == _switch_cutAndSting)
            {   //찌르기 , 치기 

                //조종축 회전 테스트 코드 
                //_hc1_object_dir.position = _HANDLE_staff.position + (_HANDLE_staff.position - _hc1_standard.position);
                //_hc1_standard.position = _HANDLE_staff.position + (_HANDLE_staff.position - _hc1_object_dir.position);

                Vector3 objectDir = _hs_objectDir.position - _hs_standard.position;
                Vector3 newPos;
                float newLength;
                this.CalcHandPos(_hs_standard.position, _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out newPos, out newLength);
                _hand_left.position = newPos;
                _arm_left_length = newLength;


                this.CalcHandPos_LineSegment(newPos, objectDir, _twoHand_length,
                                             _shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out newPos, out newLength);


                //-----------------------
                Vector3 leftToRight = newPos - _hand_left.position;
                Vector3 shaft_rot = Vector3.Cross(_hand_left.position, _shoulder_right.position);
                Vector3 rotateDir = Quaternion.AngleAxis(-90f, shaft_rot) * leftToRight.normalized;
                float length_min_twoHand = 0.1f;
                if (leftToRight.magnitude < length_min_twoHand)
                {   //양손 최소거리 일떄 자연스런 회전 효과를 준다 (미완성) 

                    _hand_left.position = _hand_left.position + rotateDir * 0.08f;
                    //_handle_leftToRight.position = newLeftPos;
                }
                //-----------------------

                _hand_right.position = newPos;
                _arm_right_length = newLength;

                //-----------------------
                //베기 모드로 연결하기 위한 핸들값 조정 

                //주변원의 중점과 손을 지나는 두개의 직선 교점  
                //Vector3 clopt0, clopt1;
                //UtilGS9.Ray3 ray0 = new Ray3((_hand_left.position-_pos_circle_left.position).normalized, _pos_circle_left.position), 
                //    ray1 = new Ray3((_hand_right.position - _pos_circle_right.position).normalized, _pos_circle_right.position);
                //Ray3.ClosestPoints(out clopt0, out clopt1, ray0, ray1);
                //_HANDLE_leftToRight.position = clopt0; 

                //_HANDLE_leftToRight.position =_pos_circle_left.position + (_hand_left.position - _pos_circle_left.position).normalized * 2f; 

                //_HANDLE_leftToRight.position = _hand_left.position;

                _HANDLE_twoHand.position = _hand_left.position + (_hand_right.position - _hand_left.position).normalized * 2f;

                _edge_circle_left.position = _hand_left.position; //현재 위치로 주변원을 맞춘다  
                _edge_circle_right.position = _hand_right.position;
                __backup_radius_circle_left = _radius_circle_left; //현재 설정값을 저장한다 
                __backup_radius_circle_right = _radius_circle_right;
                //DebugWide.LogBlue(_radius_circle_left + "   " + _radius_circle_right);
            }
            //================================================================
            else
            {   //베기 

                //-----------------------
                //주변원 크기를 설정된 값으로 천천히 되돌린다 


                if (__backup_radius_circle_left > 0f)
                {
                    //_radius_circle_left = Mathf.Lerp(_radius_circle_left, __backup_radius_circle_left, 1);
                    _radius_circle_left = __backup_radius_circle_left;
                    //DebugWide.LogBlue(_radius_circle_left + "   " + __backup_radius_circle_left + " - left ");
                    _edge_circle_left.position = _pos_circle_left.position + Vector3.forward * _radius_circle_left;
                    //__backup_radius_circle_left = -1f;
                }

                if (__backup_radius_circle_right > 0f)
                {
                    //_radius_circle_right = Mathf.Lerp(_radius_circle_right, __backup_radius_circle_right, 1);
                    _radius_circle_right = __backup_radius_circle_right;
                    //DebugWide.LogBlue(_radius_circle_right + "   " + __backup_radius_circle_right + " - right ");
                    _edge_circle_right.position = _pos_circle_right.position + Vector3.forward * _radius_circle_right;
                    //__backup_radius_circle_right = -1f;
                }


                //-----------------------
                //주변원 크기 갱신 
                _radius_circle_left = (_pos_circle_left.position - _edge_circle_left.position).magnitude;
                _radius_circle_right = (_pos_circle_right.position - _edge_circle_right.position).magnitude;

                //-----------------------

                Cut_HandOriginToHandEnd(_HANDLE_twoHand.position, _eHandOrigin, _eModelKind_Left, _eModelKind_Right);

                //--------------------
                //찌르기 모드로 연결하기 위한 핸들값 조정 
                _hs_standard.position = _hand_left.position;
                _hs_objectDir.position = _hand_right.position;
            }//else end
        }


    }


    //지정손 기준으로 지정길이 만큼의 반대손 위치 구하기 
    //handO : 기준이 되는 손 , handDir : 손과 다른손간의 방향 , twoLength : 손과 다른손의 사이길이 
    //handE : 위치를 구할려는 손 
    public void ApplyHandPos_TwoHandLength(Vector3 handLeft_pos, float handLeft_length, Vector3 handRight_pos ,float handRight_length, ePart eHandO, float twoLength)
    {

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

        if (eHandO == ePart.TwoHand_LeftO)
        {
            handO = handLeft_pos;
            handE = handRight_pos;
            shO = _shoulder_left.position;
            shE = _shoulder_right.position;
            shE_min = _arm_right_min_length;
            shE_max = _arm_right_max_length;
        }
        else if (eHandO == ePart.TwoHand_RightO)
        {
            handO = handRight_pos;
            handE = handLeft_pos;
            shO = _shoulder_right.position;
            shE = _shoulder_left.position;
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

        if (eHandO == ePart.TwoHand_LeftO)
        {
            _arm_left_length = handLeft_length;
            _hand_left.position = handO;

            _arm_right_length = new_handE_length;
            _hand_right.position = new_handE_pos;
        }
        if (eHandO == ePart.TwoHand_RightO)
        {
            _arm_right_length = handRight_length;
            _hand_right.position = handO;

            _arm_left_length = new_handE_length;
            _hand_left.position = new_handE_pos;
        }

    }

    //handle 
    //eHandOrigin : 고정손
    //eModelLeft : 궤적모형 
    public void Cut_HandOriginToHandEnd(Vector3 handle, ePart eHandOrigin, Geo.Model.eKind eModelLeft, Geo.Model.eKind eModelRight)
    {
        Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
        axis_up.Normalize();

        float new_leftLength = 0f;
        Vector3 new_leftPos;
        float new_rightLength = 0f;
        Vector3 new_rightPos;


        //-----------------------
        //모델원 위치 계산 

        _Model_left.kind = eModelLeft;
        this.SetModel_CurValue(_Model_left, axis_up);
        this.CalcHandPos_PlaneArea(_Model_left, handle, axis_up,
                                   _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out new_leftPos, out new_leftLength);



        _Model_right.kind = eModelRight;
        this.SetModel_CurValue(_Model_right, axis_up);
        this.CalcHandPos_PlaneArea(_Model_right, handle, axis_up,
                                   _shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out new_rightPos, out new_rightLength);


        //----------------------------

        ApplyHandPos_TwoHandLength(new_leftPos,new_leftLength, new_rightPos, new_rightLength, 
                                   eHandOrigin, _twoHand_length);

        

    }

    //public void HandDirControl_LeftToRight_old()
    //{
    //    Vector3 sdToHand = (_hand_left.position - _shoulder_left.position);
    //    Vector3 n_sdToHand = sdToHand.normalized;
    //    Vector3 objectDir = _hs_objectDir.position - _hs_standard.position;
    //    //조종축에 맞게 위치 계산 (코사인제2법칙으로 구현한 것과는 다른 방식)

    //    //- 기준점이 어깨범위를 벗어났는지 검사
    //    //*
    //    //1. 기준점이 왼손범위 안에 있는지 바깥에 있는지 검사
    //    float wsq = (_hs_standard.position - _shoulder_left.position).sqrMagnitude;
    //    float rsq = _arm_left_length * _arm_left_length;
    //    Vector3 inter_pos = UtilGS9.ConstV.v3_zero;
    //    bool testInter = false;
    //    float frontDir = 1f;
    //    float stand_dir = Vector3.Dot(_body_dir, _hs_standard.position - transform.position);

    //    //기준점이 왼손범위 바깥에 있다 - 몸방향 값을 받대로 바꿔서 계산 
    //    if (wsq > rsq)
    //        frontDir = -1f;

    //    //기준점이 몸방향과 반대방향에 있다 - 몸방향 값을 받대로 바꿔서 계산 
    //    if (0 > stand_dir)
    //        frontDir *= -1f;

    //    testInter = UtilGS9.Geo.IntersectRay2(_shoulder_left.position, _arm_left_length, _hs_standard.position, frontDir * _body_dir, out inter_pos);

    //    if (true == testInter)
    //    {
    //        _hand_left.position = inter_pos;
    //    }
    //    else
    //    {   //기준점에서 몸방향이 왼손범위에 닿지 않는 경우 
    //        sdToHand = inter_pos - _shoulder_left.position;
    //        n_sdToHand = sdToHand.normalized;
    //        _hand_left.position = _shoulder_left.position + n_sdToHand * _arm_left_length;
    //    }

    //    _hand_right.position = _hand_left.position + objectDir.normalized * _twoHand_length;
    //    //*/   


    //    //chamto test 2 - 고정위치로 회전면을 구해 오른손 위치값 결정하기 
    //    //Vector3 targetDir = _target_1.position - _hc1_standard.position;
    //    //Vector3 shaft_t = Vector3.Cross(objectDir, targetDir);
    //}

    public void TwoHandControl2_Left()
    {
        Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
        Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
        //Vector3 Os1 = _shoulder_right.position - _hand_left.position;
        //Vector3 Lf = Vector3.Cross(axis_up, Os1);
        //Vector3 Os2 = Vector3.Cross(Lf,axis_up);
        //Vector3 up2 = Vector3.Cross(Os2, -Os1);

        //==================================================


        //손 움직임 만들기 
        Vector3 shoulderToCrossHand = _hand_left.position - _shoulder_right.position;
        float shoulderToCrossHand_length = shoulderToCrossHand.magnitude;

        //==================================================
        //삼각형 구성 불능 검사
        //if (shoulderToCrossHand.magnitude + _twoHand_length < _arm_right_length)
            //DebugWide.LogBlue("삼각형 구성 불능 : "+ shoulderToCrossHand_length);
            
        //==================================================

        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        float a = shoulderToCrossHand_length;
        float b = _arm_right_length;
        float c = _twoHand_length;

        //Acos 에는 0~1 사이의 값만 들어가야 함. 검사 : a+b < c 일 경우 음수값이 나옴 
        //if (a + b - c < 0)
        //c = (a + b) * 0.8f; //c의 길이를 표현 최대값의 80%값으로 설정  
        //a = (c - b) * 1.01f;

        float cosC = (a * a + b * b - c * c) / (2 * a * b);
        //DebugWide.LogBlue(cosC + "   " + Mathf.Acos(cosC));

        cosC = Mathf.Clamp01(cosC); //0~1사이의 값만 사용

        float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;
        Vector3 newPos_hR = shoulderToCrossHand.normalized * b;

        //회전축 구하기 
        Vector3 shaft = Vector3.Cross(shoulderToCrossHand, (_hand_right.position - _shoulder_right.position));
        //shaft = Vector3.left; //chamto test 0-------
        //shaft = -shaft_t; //chamto test 2-------
        //shaft = up2;
        //shaft = Lf;

        //shoulderToCrossHand 를 기준으로 내적값이 오른손이 오른쪽에 있으면 양수 , 왼쪽에 있으면 음수가 된다 
        //위 내적값으로 shoulderToCrossHand 기준으로 양쪽으로 오른손이 회전을 할 수 있게 한다 
        if(Vector3.Dot(axis_up , shaft) >= 0)
            shaft = axis_up;
        else 
            shaft = -axis_up;

        //shaft = Quaternion.AngleAxis(-angleC, axis_forward) * shaft; //chamto test

        newPos_hR = _shoulder_right.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _hand_right.position = newPos_hR;
        _arm_right_length = (_shoulder_right.position - _hand_right.position).magnitude; //임시 

    }


    public void TwoHandControl2_Right()
    {
        //손 움직임 만들기 
        Vector3 shoulderToCrossHand = _hand_right.position - _shoulder_left.position;

        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        float a = shoulderToCrossHand.magnitude;
        float b = _arm_left_length;
        float c = _twoHand_length;

        //Acos 에는 0~1 사이의 값만 들어가야 함. 검사 : a+b < c 일 경우 음수값이 나옴 
        //if (a + b - c < 0)
        //c = (a + b) * 0.8f; //c의 길이를 표현 최대값의 80%값으로 설정  
        //a = (c - b) * 1.01f;

        float cosC = (a * a + b * b - c * c) / (2 * a * b);
        cosC = Mathf.Clamp01(cosC); //0~1사이의 값만 사용

        float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;
        Vector3 newPos_hR = shoulderToCrossHand.normalized * b;

        //회전축 구하기 
        Vector3 shaft = Vector3.Cross(shoulderToCrossHand, (_hand_left.position - _shoulder_left.position));

        newPos_hR = _shoulder_left.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _hand_left.position = newPos_hR;
    }

    public void OneHandleControl1_Left()
    {
        Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
        Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;

        //==================================================

        //손 움직임 만들기 
        Vector3 shoulderToCrossHandle = _HANDLE_left.position - _shoulder_left.position;
        float shoulderToCrossHandle_length = shoulderToCrossHandle.magnitude;

        Vector3 shoulderToHand = _hand_left.position - _shoulder_left.position;
        //==================================================

        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        float a = shoulderToCrossHandle_length;
        float b = _arm_left_length;
        float c = 0.3f; //임시값 , 칼 자루의 길이 

        float cosC = (a * a + b * b - c * c) / (2 * a * b);

        cosC = Mathf.Clamp01(cosC); //0~1사이의 값만 사용

        float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;
        Vector3 newPos_hR = shoulderToHand.normalized * a;

        //회전축 구하기 
        Vector3 shaft = Vector3.Cross(shoulderToHand, (_HANDLE_left.position - _hand_left.position));

        //shoulderToCrossHand 를 기준으로 내적값이 오른손이 오른쪽에 있으면 양수 , 왼쪽에 있으면 음수가 된다 
        //위 내적값으로 shoulderToCrossHand 기준으로 양쪽으로 오른손이 회전을 할 수 있게 한다 
        if (Vector3.Dot(axis_up, shaft) >= 0)
            shaft = axis_up;
        else
        shaft = -axis_up;

        //Vector3 shaft = -axis_up; //임시 

        newPos_hR = _shoulder_left.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _HANDLE_left.position = newPos_hR;
    }

    public void OneHandleControl1_Right()
    {
        Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
        Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;

        //==================================================

        //손 움직임 만들기 
        Vector3 shoulderToCrossHandle = _HANDLE_right.position - _shoulder_right.position;
        float shoulderToCrossHandle_length = shoulderToCrossHandle.magnitude;

        Vector3 shoulderToHand = _hand_right.position - _shoulder_right.position;
        //==================================================

        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        float a = shoulderToCrossHandle_length;
        float b = _arm_right_length;
        float c = 0.3f; //임시값 , 칼 자루의 길이 

        float cosC = (a * a + b * b - c * c) / (2 * a * b);

        cosC = Mathf.Clamp01(cosC); //0~1사이의 값만 사용

        float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;
        Vector3 newPos_hR = shoulderToHand.normalized * a;

        //회전축 구하기 
        Vector3 shaft = Vector3.Cross(shoulderToHand, (_HANDLE_right.position - _hand_right.position));

        //shoulderToCrossHand 를 기준으로 내적값이 오른손이 오른쪽에 있으면 양수 , 왼쪽에 있으면 음수가 된다 
        //위 내적값으로 shoulderToCrossHand 기준으로 양쪽으로 오른손이 회전을 할 수 있게 한다 
        if (Vector3.Dot(axis_up, shaft) >= 0)
            shaft = axis_up;
        else
        shaft = -axis_up;

        //Vector3 shaft = axis_up;

        newPos_hR = _shoulder_right.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _HANDLE_right.position = newPos_hR;
    }


    string tempGui = "Cut and Sting";
    private void OnGUI()
    {
        
        if (GUI.Button(new Rect(100, 100, 200, 100), tempGui))
        {
            _switch_cutAndSting = !_switch_cutAndSting;

            if (true == _switch_cutAndSting)
                tempGui = "Cut Mode !";
            else
                tempGui = "Sting Mode !!";
        }
    }

	private void OnDrawGizmos()
	{
        
        //if(true == _active_shoulder_autoRotate)
        {
            DebugWide.DrawCircle(_shoulder_left.position, _arm_left_min_length, Color.gray);
            DebugWide.DrawCircle(_shoulder_left.position, _arm_left_length, Color.gray);
            DebugWide.DrawCircle(_shoulder_right.position, _arm_right_min_length, Color.gray);    
            DebugWide.DrawCircle(_shoulder_right.position, _arm_right_length, Color.gray);    
        }

        if(true == _A_action_sting)
        {
            DebugWide.DrawLine(_HANDLE_left.position, _target_1.position, Color.red);
            DebugWide.DrawLine(_HANDLE_right.position, _target_2.position, Color.red);

            DebugWide.DrawLine(_HANDLE_oneHand.position, _target_1.position, Color.magenta);
            DebugWide.DrawLine(_HANDLE_oneHand.position, _target_2.position, Color.magenta);
        }

        if(true == _A_action_cut)
        {
            
            Vector3 axis_forward = _left_axis_forward.position - _left_axis_o.position;
            Vector3 axis_up = _left_axis_up.position - _left_axis_o.position;
            DebugWide.DrawCirclePlane(_pos_circle_A0.position, _radius_circle_A0, axis_up, Color.yellow);
            DebugWide.DrawCirclePlane(_pos_circle_A1.position, _radius_circle_A1, axis_up, Color.yellow);

            axis_forward = _right_axis_forward.position - _right_axis_o.position;
            axis_up = _right_axis_up.position - _right_axis_o.position;
            DebugWide.DrawCirclePlane(_pos_circle_B0.position, _radius_circle_B0, axis_up, Color.white);
            DebugWide.DrawCirclePlane(_pos_circle_B1.position, _radius_circle_B1, axis_up, Color.white);

            //주변원 그리기 
            //Geo.DeformationSpherePoint_Fast_Gizimo(Vector3.zero, _pos_circle_A0.position, _radius_circle_A0, axis_up, _highest_circle_A0.position, 1);

            //회전원 그리기
            //axis_up = Geo.Trans_UnlaceDir(_TL2R_unlace_circle_left.position - _TL2R_pos_circle_left.position, axis_up, _TL2R_highest_circle_left.position - _TL2R_pos_circle_left.position);
            //float tonado_radius = (_TL2R_edge_circle_left.position - _TL2R_pos_circle_left.position).magnitude;
            //Geo.DeformationCirclePos_Tornado3D_Gizimo(Vector3.zero, _TL2R_pos_circle_left.position, tonado_radius, axis_up, _TL2R_highest_circle_left.position, _TL2R_angle_circle_left.position.x);
        }

        //if(true == _A_body_aroundRotate)
        //{
        //    DebugWide.DrawCircle(_pos_circle_left.position, _radius_circle_left, Color.yellow);
        //    DebugWide.DrawCircle(_pos_circle_right.position, _radius_circle_right, Color.yellow);    
        //}
        if (true == _A_body_aroundRotate2)
        {
            //DebugWide.DrawCircle(_pos_handLeft_aroundRotate.position, _radius_handLeft_aroundRotate, Color.yellow);
            //DebugWide.DrawLine(_pos_handLeft_aroundRotate.position, _hand_left.position, Color.yellow);


            //DebugWide.DrawCircle(_pos_handRight_aroundRotate.position, _radius_handRight_aroundRotate, Color.yellow);    

            //===========
            Vector3 handle = _HANDLE_twoHand.position;
            Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
            Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;

            // ****  왼손만 계산(임시) *****
            Vector3 handleToCenter = _pos_circle_left.position - handle;
            Vector3 proj_handle = axis_up * Vector3.Dot(handleToCenter, axis_up) / axis_up.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 
            Vector3 proj_handlePos = handle + proj_handle;

            //왼손 길이 계산 
            Vector3 leftCircleCenter = _pos_circle_left.position;
            Vector3 n_circleToHandLeft = (proj_handlePos - leftCircleCenter).normalized;

            Vector3 leftPos = leftCircleCenter + n_circleToHandLeft * _radius_circle_left;
            Vector3 n_sdToLeftPos = (leftPos - _shoulder_left.position).normalized;

            DebugWide.DrawLine(leftCircleCenter, leftPos, Color.white);
            DebugWide.DrawLine(_shoulder_left.position, leftPos, Color.white);

            DebugWide.DrawCirclePlane(_pos_circle_left.position, _radius_circle_left, axis_up, Color.yellow);
            DebugWide.DrawCirclePlane(_pos_circle_right.position, _radius_circle_right, axis_up, Color.yellow);

        }

        //==================================================

        if(ePart.TwoHand == _part_control)
        {
            DebugWide.DrawLine(_shoulder_right.position, _hand_left.position, Color.red);

            Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
            Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
            float shoulderToCrossHand_length = (_hand_left.position - _shoulder_right.position).magnitude;
            float angleC = this.CalcJoint_AngleC(shoulderToCrossHand_length, _arm_right_length, _twoHand_length);
            //Vector3 shaft = Quaternion.AngleAxis(angleC, axis_forward) * axis_up;
            Vector3 shaft = Vector3.Cross((_hand_left.position - _shoulder_right.position), (_hand_right.position - _shoulder_right.position));
            shaft.Normalize();
            if (Vector3.Dot(axis_up, shaft) >= 0)
                shaft = axis_up;
            else
                shaft = -axis_up;
            DebugWide.PrintText(_shoulder_right.position + Vector3.right, Color.red, angleC + "");    
            DebugWide.DrawLine(_shoulder_right.position, _shoulder_right.position + shaft, Color.red);

        }
        //if (ePart.TwoHand_Right == _part_control)
        //{
        //    DebugWide.DrawLine(_shoulder_left.position, _hand_right.position, Color.red);

        //    Vector3 shaft = Vector3.Cross(_hand_right.position - _shoulder_left.position, _hand_left.position - _shoulder_left.position);
        //    DebugWide.PrintText(_shoulder_left.position + Vector3.left, Color.red, Vector3.SignedAngle(_hand_left.position - _shoulder_left.position, _hand_right.position - _shoulder_left.position, shaft) + "");
        //}


        //손방향 조종
        if (true == _A_handleStaff_control)
        {
            //Vector3 objectDir = _hc1_object_dir.position - _hc1_standard.position;
            //Vector3 targetDir = _target_1.position - _hc1_standard.position;
            //Vector3 shaft_t = Vector3.Cross(objectDir, targetDir);
            //DebugWide.DrawLine(_hc1_standard.position, _target_1.position, Color.black);
            DebugWide.DrawLine(_hs_standard.position, _hs_objectDir.position, Color.white);
            DebugWide.DrawCircle(_hs_objectDir.position, 0.05f, Color.white);
            //DebugWide.DrawLine(_hc1_standard.position, _hc1_standard.position + shaft_t, Color.white);

            //----
            //float rsq = _arm_left_length * _arm_left_length;
            //float wsq = (_hs_standard.position - _shoulder_left.position).sqrMagnitude;
            //float inArea = 1;
            //if (rsq < wsq) inArea = -1f;
            //DebugWide.DrawLine(_hs_standard.position, _hs_standard.position + _body_dir * inArea * 3, Color.yellow);
            //----

            if(true == _switch_cutAndSting)
            {
                
                //Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
                Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;

                //주변원의 중심에서 핸들까지 
                DebugWide.DrawLine(_pos_circle_left.position, _HANDLE_twoHand.position, Color.red);
                DebugWide.DrawLine(_pos_circle_right.position, _HANDLE_twoHand.position, Color.red);

                //설정된 모델 그리기 
                _Model_left.Draw(axis_up, Color.yellow);
                _Model_right.Draw(axis_up, Color.blue);


            }
        }

        //손조종 
        if (true == _A_hand_control_2)
        {
            //왼손기준 
            DebugWide.DrawLine(_hand_left.position, _hand_left.position + _L2R_axis_up.position - _L2R_axis_o.position, Color.red);
            //DebugWide.DrawLine(handPos,handPos + _hc2_L_axis_right.position - _hc2_L_axis_o.position, Color.red);
            //DebugWide.DrawLine(handPos,handPos + _hc2_L_axis_forward.position - _hc2_L_axis_o.position, Color.red);
            DebugWide.DrawCirclePlane(_hand_left.position, _twoHand_length, _L2R_axis_up.position - _L2R_axis_o.position, Color.magenta);

            Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
            //Vector3 Os1 = _shoulder_right.position - _hand_left.position;
            //Vector3 Lf = Vector3.Cross(axis_up, Os1);
            //Vector3 Os2 = Vector3.Cross(Lf, axis_up);
            //Vector3 up2 = Vector3.Cross(Os2, -Os1);
            //DebugWide.LogBlue(up2);
            //DebugWide.DrawLine(_shoulder_right.position, _shoulder_right.position + Lf, Color.magenta);

            //this.DrawCircleCone(_shoulder_right.position, _arm_right_length, axis_up, _hand_right.position - _shoulder_right.position, Color.cyan);
        }


        //기본 손정보  출력 
        {
            Vector3 sLsR = _shoulder_right.position - _shoulder_left.position;
            Vector3 hLsL = _hand_left.position - _shoulder_left.position;
            Vector3 hRsR = _hand_right.position - _shoulder_right.position;
            Vector3 hLhR = _hand_left.position - _hand_right.position;

            DebugWide.PrintText(_shoulder_left.position + hLsL * 0.5f, Color.white, "armL " + _arm_left_length.ToString("00.00"));
            DebugWide.PrintText(_shoulder_right.position + hRsR * 0.5f, Color.white, "armR " + _arm_right_length.ToString("00.00"));
            DebugWide.PrintText(_shoulder_left.position + sLsR * 0.5f, Color.white, "shoulder " + _shoulder_length.ToString("00.00"));
            DebugWide.PrintText(_hand_right.position + hLhR * 0.5f, Color.white, "twoH " + hLhR.magnitude.ToString("00.00"));


            DebugWide.DrawLine(_shoulder_left.position, _hand_left.position, Color.green);
            DebugWide.DrawCircle(_hand_left.position, 0.05f, Color.green);
            DebugWide.DrawLine(_shoulder_right.position, _hand_right.position, Color.green);
            DebugWide.DrawCircle(_hand_right.position, 0.05f, Color.green);
            DebugWide.DrawLine(_hand_right.position, _hand_left.position, Color.black);

            DebugWide.DrawCircle(_HANDLE_left.position, 0.05f, Color.green);
            DebugWide.DrawCircle(_HANDLE_right.position, 0.05f, Color.green);
        }
	}

    public float CalcJoint_AngleC(float a_length, float b_length, float c_length)
    {
        
        //==================================================
        //삼각형 구성 불능 검사
        //if (shoulderToCrossHand.magnitude + _twoHand_length < _arm_right_length)
        //DebugWide.LogBlue("삼각형 구성 불능 : "+ shoulderToCrossHand_length);

        //==================================================

        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        float a = a_length;
        float b = b_length;
        float c = c_length;

        float cosC = (a * a + b * b - c * c) / (2 * a * b);


        cosC = Mathf.Clamp01(cosC); //0~1사이의 값만 사용

        float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;

        return angleC;
    }
}