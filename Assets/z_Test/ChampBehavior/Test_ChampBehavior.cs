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
        TwoHand_Left,
        TwoHand_Right,
        OneHand,

        Max,
    }

    //------------------------------------------------------
    //debug
    public LineRenderer _debugLine = null;


    //------------------------------------------------------

    public Transform _tbody_dir = null;
    public Transform _shoulder_left = null;
    public Transform _shoulder_right = null;
    public Transform _hand_left = null;
    public Transform _hand_right = null;
    public Transform _object_left = null;
    public Transform _object_right = null;

    //핸들 
    public Transform _handle_left = null;   //핸들
    public Transform _handle_right = null;
    public Transform _handle_leftToRight = null;

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
    public ePart _part_control = ePart.TwoHand_Left;
    //public ePart _part_control = ePart.OneHand;

    public bool _A_armLength_max_min = false;

    public string _1____________________ = "";
    public bool  _A_shoulder_autoRotate = false;
    public float _angle_shoulderLeft_autoRotate = -10f; //왼쪽 어깨 자동회전 각도량
    public float _angle_shoulderRight_autoRotate = -10f; //오른쪽 어깨 자동회전 각도량

    public string _2_1__________________ = "";
    public bool   _A_body_aroundRotate = false;
    public float _radius_leftAround = 0.5f;
    public float _radius_rightAround = 0.8f;
    public Transform _pos_leftAround = null;
    public Transform _pos_rightAround = null;

    public string _2_2__________________ = "";
    public bool _A_body_aroundRotate2 = false;

    public string _4____________________ = "";
    public bool _A_trajectoryCircle = false;
    public Transform _tc_center = null;
    public Transform _tc_anchorA = null;
    public Transform _tc_anchorB = null;
    public Transform _tc_highest = null;
    public float _tc_radius = 0.5f;

    public string _5_1___________________ = "";
    public bool _A_hand_control_1 = false;
    public Transform _hc1_axis_o = null; //control => _hc1_axis_o
    public Transform _hc1_object_dir = null; 
    public Transform _hc1_standard = null;


    public string _5_2___________________ = "";
    public bool _A_hand_control_2 = false;
    public Transform _hc2_L_axis_o = null;
    public Transform _hc2_L_axis_up = null;
    public Transform _hc2_L_axis_right = null;
    public Transform _hc2_L_axis_forward = null;


    public string _6_1___________________ = "";
    public bool _A_oneHand_control_1 = false;



	private void Start()
	{

        _debugLine = GameObject.Find("line").GetComponent<LineRenderer>();

        //--------------------------------------------------

        _tbody_dir = GameObject.Find("body_dir").transform;
        _shoulder_left = GameObject.Find("shoulder_left").transform;
        _shoulder_right = GameObject.Find("shoulder_right").transform;
        _hand_left = GameObject.Find("hand_left").transform;
        _hand_right = GameObject.Find("hand_right").transform;
        _object_left = GameObject.Find("object_left").transform;
        _object_right = GameObject.Find("object_right").transform;

        //핸들
        _handle_left = GameObject.Find("handle_left").transform; //핸들 
        _handle_right = GameObject.Find("handle_right").transform;
        _handle_leftToRight = GameObject.Find("handle_leftToRight").transform;

        //목표
        _target_1 = GameObject.Find("target_1").transform;
        _target_2 = GameObject.Find("target_2").transform;

        //_spr_object_left = _object_left.GetComponentInChildren<SpriteRenderer>();
        //_spr_object_right = _object_right.GetComponentInChildren<SpriteRenderer>();

        _hand_left_spr = _hand_left.GetComponentInChildren<SpriteRenderer>().transform;
        _hand_right_spr = _hand_right.GetComponentInChildren<SpriteRenderer>().transform;

        //궤적원
        _tc_center = GameObject.Find("trajectoryCircle").transform;
        _tc_anchorA = GameObject.Find("anchorA").transform;
        _tc_anchorB = GameObject.Find("anchorB").transform;
        _tc_highest = GameObject.Find("highest").transform;


        //=======
        //조종항목
        _pos_leftAround = GameObject.Find("pos_leftAround").transform;
        _pos_rightAround = GameObject.Find("pos_rightAround").transform;



        //=======
        //손조종 1
        _hc1_object_dir = GameObject.Find("hc1_object_dir").transform;
        _hc1_standard = GameObject.Find("hc1_standard").transform;
        _hc1_axis_o = GameObject.Find("hc1_axis_o").transform;

        //=======
        //손조종 2
        _hc2_L_axis_o = GameObject.Find("hc2_L_axis_o").transform;
        _hc2_L_axis_up = GameObject.Find("hc2_L_axis_up").transform;
        _hc2_L_axis_right = GameObject.Find("hc2_L_axis_right").transform;
        _hc2_L_axis_forward = GameObject.Find("hc2_L_axis_forward").transform;

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

        //궤적원에 따른 왼손/오른손 움직임 표현 
        if (true == _A_trajectoryCircle)
        {

            if (ePart.TwoHand_Left == _part_control)
            {
                Vector3 tempPos = Geo.DeformationSpherePoint(_hand_left.position, _tc_center.position, _tc_radius, _tc_anchorA.position, _tc_anchorB.position, _tc_highest.position, 1);
                float sqrTempLength = (_shoulder_left.position - tempPos).sqrMagnitude;
                if (_arm_left_min_length <= sqrTempLength && sqrTempLength <= _arm_left_max_length)
                {
                    _hand_left.position = tempPos;
                }

            }

            if (ePart.TwoHand_Right == _part_control)
            {
                Vector3 tempPos = Geo.DeformationSpherePoint(_hand_right.position, _tc_center.position, _tc_radius, _tc_anchorA.position, _tc_anchorB.position, _tc_highest.position, 1);
                float sqrTempLength = (_shoulder_right.position - tempPos).sqrMagnitude;
                if (_arm_right_min_length <= sqrTempLength && sqrTempLength <= _arm_right_max_length)
                {
                    _hand_right.position = tempPos;
                }
            }


        }


        //==================================================

        //손길이 제약 
        Vector3 hLsL = (_hand_left.position - _shoulder_left.position);
        Vector3 hRsR = (_hand_right.position - _shoulder_right.position);
        Vector3 n_hLsL = hLsL.normalized;
        Vector3 n_hRsR = hRsR.normalized;

        if(false)
        {
            if (false == _A_armLength_max_min)
            {
                //길이 고정 
                _hand_right.position = _shoulder_right.position + n_hRsR * _arm_right_length;
                _hand_left.position = _shoulder_left.position + n_hLsL * _arm_left_length;

            }
            else
            {
                //최대 최소 길이 안에서 가변적 
                _arm_left_length = hLsL.magnitude;
                _arm_right_length = hRsR.magnitude;

                //오른손길이 최소최대 제약 
                if (_arm_right_min_length > _arm_right_length)
                {
                    _arm_right_length = _arm_right_min_length;
                    _hand_right.position = _shoulder_right.position + n_hRsR * _arm_right_length;
                }
                else if (_arm_right_length > _arm_right_max_length)
                {
                    _arm_right_length = _arm_right_max_length;
                    _hand_right.position = _shoulder_right.position + n_hRsR * _arm_right_length;
                }

                //왼손길이 최소최대 제약
                if (_arm_left_min_length > _arm_left_length)
                {
                    _arm_left_length = _arm_left_min_length;
                    _hand_left.position = _shoulder_left.position + n_hLsL * _arm_left_length;
                }
                else if (_arm_left_length > _arm_left_max_length)
                {
                    _arm_left_length = _arm_left_max_length;
                    _hand_left.position = _shoulder_left.position + n_hLsL * _arm_left_length;
                }


                //양손거리 고정
                float sqrTwoHand_length = _twoHand_length * _twoHand_length;
                Vector3 twoHandDir = _hand_left.position - _hand_right.position;
                //DebugWide.LogBlue(Mathf.Abs(sqrTwoHand_length - twoHandDir.sqrMagnitude));
                if (Mathf.Abs(sqrTwoHand_length - twoHandDir.sqrMagnitude) > 0.001f)
                {
                    twoHandDir.Normalize();
                    if (ePart.TwoHand_Left == _part_control)
                    {
                        twoHandDir *= -1f;
                        _hand_right.position = _hand_left.position + twoHandDir * _twoHand_length;
                    }
                    if (ePart.TwoHand_Right == _part_control)
                    {
                        _hand_left.position = _hand_right.position + twoHandDir * _twoHand_length;
                    }

                    if (ePart.OneHand == _part_control)
                    {
                        //_hand_left.position = _hand_right.position + twoHandDir * _twoHand_length;

                    }
                }

            }
        }



        //==================================================

        //찌르기 

        //양손모드
        if (ePart.OneHand == _part_control)
        {}

        if (ePart.TwoHand_Left == _part_control)
        {
            
            //왼손을 핸들로 조종하기 
            Vector3 newPos;
            float newLength;
            this.CalcHandPos(_handle_leftToRight.position, _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out newPos, out newLength);
            _hand_left.position = newPos;
            _arm_left_length = newLength;

            //<방식1> target_1 에 따라 오른손 위치 결정하기 - target_1 에 봉이 도달하지 못하는 경우가 있음 (오른손위치계산 => 오른손제약범위 적용)
            _hand_right.position = _hand_left.position + (_target_1.position - _hand_left.position).normalized * _twoHand_length;

            Vector3 handToTarget = (_target_1.position - _hand_left.position);
            Vector3 n_handToTarget = handToTarget.normalized;
            Vector3 posOnMaxCircle;
            float newlength_twoHand = _arm_right_max_length - (_shoulder_right.position - _hand_left.position).magnitude;

            //----------------------------------------------

            if(true == UtilGS9.Geo.IntersectRay2(_shoulder_right.position, _arm_right_max_length, _hand_left.position, n_handToTarget, out posOnMaxCircle))
            {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 있다  

                if (newlength_twoHand > 0)
                {   //왼손이 오른손 최대 범위 안에 있는 경우
                    
                    if ((_hand_left.position - posOnMaxCircle).magnitude > _twoHand_length)
                    {
                        //DebugWide.LogBlue("111");
                        newPos = _hand_left.position + n_handToTarget * _twoHand_length;
                    }
                    else
                    {
                        //DebugWide.LogBlue("222");
                        newPos = posOnMaxCircle;
                    }    
                }else
                {   //왼손이 오른손 최대 범위 밖에 있는 경우 

                    newPos = _hand_left.position + n_handToTarget * _twoHand_length;
                    //newPos = _hand_left.position + (posOnMaxCircle - _hand_left.position).normalized * _twoHand_length;
                    if((newPos - _shoulder_right.position).sqrMagnitude > _arm_right_max_length * _arm_right_max_length) 
                    {
                        //DebugWide.LogBlue("333");
                        newPos = posOnMaxCircle;
                    }else
                    {
                        //DebugWide.LogBlue("444");
                    }
                        
                    
                }

                //chamto debug test
                //_debugLine.SetPosition(0, _hand_left.position);
                //_debugLine.SetPosition(1, posOnMaxCircle);

            }else
            {   //목표와 왼손 사이의 직선경로 위에서 오른손 위치를 구할 수 없다   :  목표와 왼손 사이의 직선경로가 오른손 최대범위에 닿지 않는 경우
                
                Vector3 targetToRSd = (_shoulder_right.position - _hand_left.position);
                Vector3 n_targetToRSd = targetToRSd.normalized;
                float length_contactPt = targetToRSd.sqrMagnitude - _arm_right_max_length * _arm_right_max_length;
                length_contactPt = (float)System.Math.Sqrt(length_contactPt);
                float proj_cos = length_contactPt / targetToRSd.magnitude;

                //-----------------------

                //proj_cos = Mathf.Clamp01(proj_cos); //0~1사이의 값만 사용
                float angleC = Mathf.Acos(proj_cos) * Mathf.Rad2Deg;
                newPos = _hand_left.position + Quaternion.AngleAxis(-angleC, Vector3.up) * n_targetToRSd * length_contactPt; //fixme : up_vector


            }


            _hand_right.position = newPos;
            _arm_right_length = (_hand_right.position - _shoulder_right.position).magnitude;
            if (_arm_right_length > _arm_right_max_length)
                _arm_right_length = _arm_right_max_length;


        }



        //==================================================

        //변경된 각도만큼 증가시키는 방식 - 왼손과 오른손 사이의 거리가 동일하게 유지가 안된다 
        //float angle = Vector3.SignedAngle(__prev_hLsL, hLsL, UtilGS9.ConstV.v3_up);
        //Vector3 newHRsR =  Quaternion.AngleAxis(angle, UtilGS9.ConstV.v3_up) * hRsR;
        //newHRsR += _shoulder_right.position;
        //_hand_right.position = newHRsR;

        //__prev_hLsL = hLsL;


        //==================================================
        //왼손,오른손 자동 회전 테스트
        if(true == _A_shoulder_autoRotate)
        {
            
            Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
            Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;
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

        //임의의 원 중심으로 오른손/왼손 길이 계산 하기  
        if (true == _A_body_aroundRotate)
        {
            //오른손 길이 계산
            Vector3 rightCircleCenter = _pos_rightAround.position;
            Vector3 n_circleToHandRight = (_hand_right.position - rightCircleCenter).normalized;
            _hand_right.position = rightCircleCenter + n_circleToHandRight * _radius_rightAround;
            _arm_right_length = (_hand_right.position - _shoulder_right.position).magnitude;

            //왼손 길이 계산 
            Vector3 leftCircleCenter = _pos_leftAround.position;
            Vector3 n_circleToHandLeft = (_hand_left.position - leftCircleCenter).normalized;
            _hand_left.position = leftCircleCenter + n_circleToHandLeft * _radius_leftAround;
            _arm_left_length = (_hand_left.position - _shoulder_left.position).magnitude;

            //_twoHand_length = (_hand_right.position - _hand_left.position).magnitude; //양손 사이길이 다시 셈함
        }

        //==================================================

        //주변원 모드2 
        if (true == _A_body_aroundRotate2)
        {
            Vector3 handle = _handle_leftToRight.position;
            Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
            Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;


            Vector3 newPos = Vector3.zero;
            float newLength = 0f;
            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_leftAround.position, _radius_leftAround,
                                         _shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                         out newPos, out newLength);
            _arm_left_length = newLength;
            _hand_left.position = newPos;

            this.CalcHandPos_AroundCircle(handle, axis_up, _pos_rightAround.position, _radius_rightAround,
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

        //float oo = 199208737f; //약2억 원금
        //float rr = 0.02f; //이율 0.2프로
        //float aa = oo * (1f + rr) * (12 / 12);
        //float ee = aa - oo; //1년치 증가한 이자
        //DebugWide.LogBlue("" + ee);

        //float minus = 830000 * 12f;
        //for (int i = 0; i < 30;i++)
        //{
        //    oo = oo - minus;
        //    aa = oo * (1f + rr) * (12 / 12);
        //    ee = aa - oo;
        //    oo = oo + ee;
        //    DebugWide.LogBlue(i + "  = " + oo);
        //}
        //DebugWide.LogBlue("----------------------");

        //==================================================
        //손 움직임 만들기 
        if (true == _A_hand_control_1)
        {
            //조종축으로 손위치 계산 
            if (ePart.TwoHand_Left == _part_control)
                TwoHandControl1_Left();
        }
        if(true == _A_hand_control_2)
        {
            //코사인 제2법칙 공식을 사용하는 방식 
            if (ePart.TwoHand_Left == _part_control)
                TwoHandControl2_Left();
            if (ePart.TwoHand_Right == _part_control)
                TwoHandControl2_Right();    
        }
        if(true == _A_oneHand_control_1)
        {
            if(ePart.OneHand == _part_control)
            {
                this.OneHandleControl1_Left();
                this.OneHandleControl1_Right();
            }
        }


        //DebugWide.LogBlue(shaft + "    angle: " +angleC +"  a:" +a+ "   b:" +b+ "   c:" + c);
        //DebugWide.LogBlue(newPos_hR.x + "  " + newPos_hR.z + "    :" + angleC + "   p:" + (a+b-c));
        //DebugWide.LogBlue(angleC + " a : " + Quaternion.FromToRotation(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position).eulerAngles.y);
        //DebugWide.LogBlue(angleC + " b : " + Vector3.SignedAngle(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position, Vector3.up));


        //==================================================
        //손에 칼 붙이기 3d

        if (ePart.OneHand == _part_control)
        {   //한손 칼 붙이기 

            Vector3 handToHandle = _handle_left.position - _hand_left.position;
            Vector3 obj_shaft = Vector3.Cross(Vector3.forward, handToHandle);
            float angleW = Vector3.SignedAngle(Vector3.forward, handToHandle, obj_shaft);
            _object_left.rotation = Quaternion.AngleAxis(angleW, obj_shaft);    

            handToHandle = _handle_right.position - _hand_right.position;
            obj_shaft = Vector3.Cross(Vector3.forward, handToHandle);
            angleW = Vector3.SignedAngle(Vector3.forward, handToHandle, obj_shaft);
            _object_right.rotation = Quaternion.AngleAxis(angleW, obj_shaft);    

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

    public void TwoHandControl1_Left()
    {
        Vector3 hLsL = (_hand_left.position - _shoulder_left.position);
        Vector3 n_hLsL = hLsL.normalized;
        //조종축에 맞게 위치 계산 (코사인제2법칙으로 구현한 것과는 다른 방식)

        //- 기준점이 어깨범위를 벗어났는지 검사
        //*
        //1. 기준점이 왼손범위 안에 있는지 바깥에 있는지 검사
        float wsq = (_hc1_standard.position - _shoulder_left.position).sqrMagnitude;
        float rsq = _arm_left_length * _arm_left_length;
        Vector3 inter_pos = UtilGS9.ConstV.v3_zero;
        bool testInter = false;
        float frontDir = 1f;
        float stand_dir = Vector3.Dot(_body_dir, _hc1_standard.position - transform.position);

        //기준점이 왼손범위 바깥에 있다 - 몸방향 값을 받대로 바꿔서 계산 
        if (wsq > rsq)
            frontDir = -1f;

        //기준점이 몸방향과 반대방향에 있다 - 몸방향 값을 받대로 바꿔서 계산 
        if (0 > stand_dir)
            frontDir *= -1f;

        testInter = UtilGS9.Geo.IntersectRay(_shoulder_left.position, _arm_left_length, _hc1_standard.position, frontDir * _body_dir, out inter_pos);

        if (true == testInter)
        {
            _hand_left.position = inter_pos;
        }
        else
        {   //기준점에서 몸방향이 왼손범위에 닿지 않는 경우 
            hLsL = inter_pos - _shoulder_left.position;
            n_hLsL = hLsL.normalized;
            _hand_left.position = _shoulder_left.position + n_hLsL * _arm_left_length;
        }

        _hand_right.position = _hand_left.position + (_hc1_object_dir.position - _hc1_standard.position).normalized * _twoHand_length;
        //*/   

        //chamto test 1 - 고정위치(object_dir)에서 오른손 위치값 구하기 
        //Vector3 objectDir = _hc1_object_dir.position - _hand_left.position;
        //objectDir.Normalize();
        //_hand_right.position = _hand_left.position + objectDir * _twoHand_length;
        //return;

        //chamto test 2 - 고정위치로 회전면을 구해 오른손 위치값 결정하기 
        //Vector3 objectDir = _object_dir.position - _standard.position;
        //Vector3 targetDir = _target.position - _standard.position;
        //Vector3 shaft_t = Vector3.Cross(objectDir, targetDir);
    }

    public void TwoHandControl2_Left()
    {
        Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
        Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;
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
        Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
        Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;

        //==================================================

        //손 움직임 만들기 
        Vector3 shoulderToCrossHandle = _handle_left.position - _shoulder_left.position;
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
        Vector3 shaft = Vector3.Cross(shoulderToHand, (_handle_left.position - _hand_left.position));

        //shoulderToCrossHand 를 기준으로 내적값이 오른손이 오른쪽에 있으면 양수 , 왼쪽에 있으면 음수가 된다 
        //위 내적값으로 shoulderToCrossHand 기준으로 양쪽으로 오른손이 회전을 할 수 있게 한다 
        if (Vector3.Dot(axis_up, shaft) >= 0)
            shaft = axis_up;
        else
        shaft = -axis_up;

        //Vector3 shaft = -axis_up; //임시 

        newPos_hR = _shoulder_left.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _handle_left.position = newPos_hR;
    }

    public void OneHandleControl1_Right()
    {
        Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
        Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;

        //==================================================

        //손 움직임 만들기 
        Vector3 shoulderToCrossHandle = _handle_right.position - _shoulder_right.position;
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
        Vector3 shaft = Vector3.Cross(shoulderToHand, (_handle_right.position - _hand_right.position));

        //shoulderToCrossHand 를 기준으로 내적값이 오른손이 오른쪽에 있으면 양수 , 왼쪽에 있으면 음수가 된다 
        //위 내적값으로 shoulderToCrossHand 기준으로 양쪽으로 오른손이 회전을 할 수 있게 한다 
        if (Vector3.Dot(axis_up, shaft) >= 0)
            shaft = axis_up;
        else
        shaft = -axis_up;

        //Vector3 shaft = axis_up;

        newPos_hR = _shoulder_right.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _handle_right.position = newPos_hR;
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
        if(true == _A_body_aroundRotate)
        {
            DebugWide.DrawCircle(_pos_leftAround.position, _radius_leftAround, Color.yellow);
            DebugWide.DrawCircle(_pos_rightAround.position, _radius_rightAround, Color.yellow);    
        }
        if (true == _A_body_aroundRotate2)
        {
            //DebugWide.DrawCircle(_pos_handLeft_aroundRotate.position, _radius_handLeft_aroundRotate, Color.yellow);
            //DebugWide.DrawLine(_pos_handLeft_aroundRotate.position, _hand_left.position, Color.yellow);


            //DebugWide.DrawCircle(_pos_handRight_aroundRotate.position, _radius_handRight_aroundRotate, Color.yellow);    

            //===========
            Vector3 handle = _handle_leftToRight.position;
            Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
            Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;

            // ****  왼손만 계산(임시) *****
            Vector3 handleToCenter = _pos_leftAround.position - handle;
            Vector3 proj_handle = axis_up * Vector3.Dot(handleToCenter, axis_up) / axis_up.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 
            Vector3 proj_handlePos = handle + proj_handle;

            //왼손 길이 계산 
            Vector3 leftCircleCenter = _pos_leftAround.position;
            Vector3 n_circleToHandLeft = (proj_handlePos - leftCircleCenter).normalized;

            Vector3 leftPos = leftCircleCenter + n_circleToHandLeft * _radius_leftAround;
            Vector3 n_sdToLeftPos = (leftPos - _shoulder_left.position).normalized;

            DebugWide.DrawLine(leftCircleCenter, leftPos, Color.white);
            DebugWide.DrawLine(_shoulder_left.position, leftPos, Color.white);

            this.DrawCirclePlate(_pos_leftAround.position, _radius_leftAround, axis_up, axis_forward, Color.yellow);
            this.DrawCirclePlate(_pos_rightAround.position, _radius_rightAround, axis_up, axis_forward, Color.yellow);

        }

        //==================================================

        if(ePart.TwoHand_Left == _part_control)
        {
            DebugWide.DrawLine(_shoulder_right.position, _hand_left.position, Color.red);

            Vector3 axis_forward = _hc2_L_axis_forward.position - _hc2_L_axis_o.position;
            Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;
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
        if (ePart.TwoHand_Right == _part_control)
        {
            DebugWide.DrawLine(_shoulder_left.position, _hand_right.position, Color.red);

            Vector3 shaft = Vector3.Cross(_hand_right.position - _shoulder_left.position, _hand_left.position - _shoulder_left.position);
            DebugWide.PrintText(_shoulder_left.position + Vector3.left, Color.red, Vector3.SignedAngle(_hand_left.position - _shoulder_left.position, _hand_right.position - _shoulder_left.position, shaft) + "");
        }


        //궤적원에 따른 왼손 움직임 표현 
        if (true == _A_trajectoryCircle)
        {
            Geo.DeformationSpherePoint_Gizimo(Vector3.zero, _tc_center.position, _tc_radius, _tc_anchorA.position, _tc_anchorB.position, _tc_highest.position, 1);
        }

        //손조종 1
        if (true == _A_hand_control_1)
        {
            Vector3 objectDir = _hc1_object_dir.position - _hc1_standard.position;
            Vector3 targetDir = _target_1.position - _hc1_standard.position;
            Vector3 shaft_t = Vector3.Cross(objectDir, targetDir);
            DebugWide.DrawLine(_hc1_standard.position, _target_1.position, Color.black);
            DebugWide.DrawLine(_hc1_standard.position, _hc1_object_dir.position, Color.black);
            DebugWide.DrawLine(_hc1_standard.position, _hc1_standard.position + shaft_t, Color.white);

            float rsq = _arm_left_length * _arm_left_length;
            float wsq = (_hc1_standard.position - _shoulder_left.position).sqrMagnitude;
            float inArea = 1;
            if (rsq < wsq) inArea = -1f;
            DebugWide.DrawLine(_hc1_standard.position, _hc1_standard.position + _body_dir * inArea * 3, Color.yellow);
        }

        //손조종 2
        if (true == _A_hand_control_2)
        {
            //왼손기준 
            DebugWide.DrawLine(_hand_left.position, _hand_left.position + _hc2_L_axis_up.position - _hc2_L_axis_o.position, Color.red);
            //DebugWide.DrawLine(handPos,handPos + _hc2_L_axis_right.position - _hc2_L_axis_o.position, Color.red);
            //DebugWide.DrawLine(handPos,handPos + _hc2_L_axis_forward.position - _hc2_L_axis_o.position, Color.red);
            this.DrawCirclePlate(_hand_left.position, _twoHand_length, _hc2_L_axis_up.position - _hc2_L_axis_o.position, _hc2_L_axis_forward.position - _hc2_L_axis_o.position, Color.magenta);

            Vector3 axis_up = _hc2_L_axis_up.position - _hc2_L_axis_o.position;
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

            DebugWide.DrawCircle(_handle_left.position, 0.05f, Color.green);
            DebugWide.DrawCircle(_handle_right.position, 0.05f, Color.green);
        }
	}

    public void DrawCirclePlate(Vector3 pos , float radius, Vector3 up , Vector3 forward, Color cc)
    {
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;

        int count = 36;
        for (int i = 0; i < count; i++)
        {
            Vector3 tdDir = Quaternion.AngleAxis(i * 10, up) * forward;

            cur = pos + tdDir * radius;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, cc);

            //if (0 == i%5)
                //DebugWide.DrawLine(pos, cur, cc);

            prev = cur;
        }
    }

    public void DrawCircleCone(Vector3 pos, float radius, Vector3 up, Vector3 forward, Color cc)
    {
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        forward.Normalize();

        int count = 36;
        for (int i = 0; i < count; i++)
        {
            Vector3 tdDir = Quaternion.AngleAxis(i * 10, up) * forward;

            cur = pos + tdDir * radius;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, cc);

            //if (0 == i%2)
                DebugWide.DrawLine(pos, cur, cc);

            prev = cur;
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