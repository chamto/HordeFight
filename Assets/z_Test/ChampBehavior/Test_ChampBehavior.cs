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
        Hand_Left,
        Hand_Right,

        Max,
    }

    public Transform _tbody_dir = null;
    public Transform _shoulder_left = null;
    public Transform _shoulder_right = null;
    public Transform _hand_left = null;
    public Transform _hand_right = null;
    public Transform _object_sword = null;


    public float _shoulder_length = 0f;
    public float _arm_left_length = 1f;
    public float _arm_left_min_length = 0.5f;
    public float _arm_left_max_length = 1.0f;
    public float _arm_right_length = 1.5f;
    public float _arm_right_min_length = 0.8f;
    public float _arm_right_max_length = 1.6f;
    public float _twoHand_length = 0.7f;

    public Vector3 _body_dir = UtilGS9.ConstV.v3_zero;
    public ePart _part_control = ePart.Hand_Left;

    public bool _active_armLength_max_min = false;

    public string ___SHOULDER_AUTO_ROTATE___ = "";
    public bool  _active_shoulder_autoRotate = false;
    public float _angle_shoulderLeft_autoRotate = -10f; //왼쪽 어깨 자동회전 각도량
    public float _angle_shoulderRight_autoRotate = -10f; //오른쪽 어깨 자동회전 각도량

    public string ___BODY_AROUND_ROTATE___ = "";
    public bool   _active_body_aroundRotate = false;
    public float _radius_handLeft_aroundRotate = 0.5f;
    public float _radius_handRight_aroundRotate = 1f;
    public Transform _pos_handLeft_aroundRotate = null;
    public Transform _pos_handRight_aroundRotate = null;

    public string ___Trajectory_Circle___ = "";
    public bool _active_trajectoryCircle = false;
    public Transform _tc_center = null;
    public Transform _tc_anchorA = null;
    public Transform _tc_anchorB = null;
    public Transform _tc_highest = null;
    public float _tc_radius = 0.5f;

    public string ___Hand_Control_1___ = "";
    public bool _active_hand_control_1 = false;
    public Transform _hc1_axis_o = null; //control => _hc1_axis_o
    public Transform _hc1_object_dir = null; 
    public Transform _hc1_target = null;
    public Transform _hc1_standard = null;


    public string ___Hand_Control_2___ = "";
    public bool _active_hand_control_2 = false;
    public Transform _hc2_L_axis_o = null;
    public Transform _hc2_L_axis_up = null;
    public Transform _hc2_L_axis_right = null;
    public Transform _hc2_L_axis_forward = null;


	private void Start()
	{
        _tbody_dir = GameObject.Find("body_dir").transform;
        _shoulder_left = GameObject.Find("shoulder_left").transform;
        _shoulder_right = GameObject.Find("shoulder_right").transform;
        _hand_left = GameObject.Find("hand_left").transform;
        _hand_right = GameObject.Find("hand_right").transform;
        _object_sword = GameObject.Find("object_sword").transform;


        //궤적원
        _tc_center = GameObject.Find("trajectoryCircle").transform;
        _tc_anchorA = GameObject.Find("anchorA").transform;
        _tc_anchorB = GameObject.Find("anchorB").transform;
        _tc_highest = GameObject.Find("highest").transform;


        //=======
        //조종항목
        _pos_handLeft_aroundRotate = GameObject.Find("pos_handLeft_aroundRotate").transform;
        _pos_handRight_aroundRotate = GameObject.Find("pos_handRight_aroundRotate").transform;



        //=======
        //손조종 1
        _hc1_object_dir = GameObject.Find("hc1_object_dir").transform;
        _hc1_target = GameObject.Find("hc1_target").transform;
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

        //손길이 제약 
        Vector3 hLsL = (_hand_left.position - _shoulder_left.position);
        Vector3 hRsR = (_hand_right.position - _shoulder_right.position);
        Vector3 n_hLsL = hLsL.normalized;
        Vector3 n_hRsR = hRsR.normalized;


        if(false == _active_armLength_max_min)
        {
            //길이 고정 
            //*
            _hand_right.position = _shoulder_right.position + n_hRsR * _arm_right_length;
            _hand_left.position = _shoulder_left.position + n_hLsL * _arm_left_length;
            //*/    
        }
        else
        {
            //최대 최소 길이 안에서 가변적 
            //*
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
            //*/
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
        if(true == _active_shoulder_autoRotate)
        {
            Vector3 axis = Vector3.right; //chamto test
            _shoulder_left.Rotate(axis, _angle_shoulderLeft_autoRotate, Space.World);
            _shoulder_right.Rotate(axis, _angle_shoulderRight_autoRotate, Space.World);

            //_control.Rotate(axis, _angle_shoulderLeft_autoRotate, Space.World);
        }

        //==================================================
        //임의의 원 중심으로 오른손/왼손 길이 계산 하기  
        if (true == _active_body_aroundRotate)
        {
            //오른손 길이 계산
            Vector3 rightCircleCenter = _pos_handRight_aroundRotate.position;
            Vector3 n_circleToHandRight = (_hand_right.position - rightCircleCenter).normalized;
            _hand_right.position = rightCircleCenter + n_circleToHandRight * _radius_handRight_aroundRotate;
            _arm_right_length = (_hand_right.position - _shoulder_right.position).magnitude;

            //왼손 길이 계산 
            Vector3 leftCircleCenter = _pos_handLeft_aroundRotate.position;
            Vector3 n_circleToHandLeft = (_hand_left.position - leftCircleCenter).normalized;
            _hand_left.position = leftCircleCenter + n_circleToHandLeft * _radius_handLeft_aroundRotate;
            _arm_left_length = (_hand_left.position - _shoulder_left.position).magnitude;

            _twoHand_length = (_hand_right.position - _hand_left.position).magnitude; //양손 사이길이 다시 셈함
        }

        //궤적원에 따른 왼손/오른손 움직임 표현 
        if(true == _active_trajectoryCircle)
        {
            //__cur_tc_angle = 0;
            //__cur_tc_angle += 5f;
            //__cur_tc_angle %= 360f;

            if (ePart.Hand_Left == _part_control)
                _hand_left.position = Geo.DeformationSpherePoint(_hand_left.position, _tc_center.position, _tc_radius, _tc_anchorA.position, _tc_anchorB.position, _tc_highest.position, 1);
            if (ePart.Hand_Right == _part_control)
                _hand_right.position = Geo.DeformationSpherePoint(_hand_right.position, _tc_center.position, _tc_radius, _tc_anchorA.position, _tc_anchorB.position, _tc_highest.position, 1);
            
        }

        //==================================================
        //손 움직임 만들기 
        if (true == _active_hand_control_1)
        {
            //조종축으로 손위치 계산 
            if (ePart.Hand_Left == _part_control)
                HandControl1_Left();
        }
        if(true == _active_hand_control_2)
        {
            //코사인 제2법칙 공식을 사용하는 방식 
            if (ePart.Hand_Left == _part_control)
                HandControl2_Left();
            if (ePart.Hand_Right == _part_control)
                HandControl2_Right();    
        }

        
        //DebugWide.LogBlue(shaft + "    angle: " +angleC +"  a:" +a+ "   b:" +b+ "   c:" + c);
        //DebugWide.LogBlue(newPos_hR.x + "  " + newPos_hR.z + "    :" + angleC + "   p:" + (a+b-c));
        //DebugWide.LogBlue(angleC + " a : " + Quaternion.FromToRotation(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position).eulerAngles.y);
        //DebugWide.LogBlue(angleC + " b : " + Vector3.SignedAngle(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position, Vector3.up));


        //==================================================



        //==================================================
        //손에 칼 붙이기 3d
        Vector3 hLhR = _hand_right.position - _hand_left.position;
        Vector3 obj_shaft = Vector3.Cross(Vector3.forward, hLhR);
        //angleC의 각도가 0이 나올 경우 외적값이 0이 된다. 각도가 0일때 물건을 손에 붙이는 계산이 안되는 문제가 발생함
        //물건 기준으로 외적값을 구해 사용하면 문제가 해결됨 

        float angleW = Vector3.SignedAngle(Vector3.forward, hLhR, obj_shaft);
        _object_sword.rotation = Quaternion.AngleAxis(angleW, obj_shaft);

	}


    public void HandControl1_Left()
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

    public void HandControl2_Left()
    {
        
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

        newPos_hR = _shoulder_right.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _hand_right.position = newPos_hR;
    }


    public void HandControl2_Right()
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


	private void OnDrawGizmos()
	{
        
        //if(true == _active_shoulder_autoRotate)
        {
            DebugWide.DrawCircle(_shoulder_left.position, _arm_left_length, Color.gray);
            DebugWide.DrawCircle(_shoulder_right.position, _arm_right_length, Color.gray);    
        }
        if(true == _active_body_aroundRotate)
        {
            DebugWide.DrawCircle(_pos_handLeft_aroundRotate.position, _radius_handLeft_aroundRotate, Color.yellow);
            DebugWide.DrawCircle(_pos_handRight_aroundRotate.position, _radius_handRight_aroundRotate, Color.yellow);    
        }

        //==================================================

        if(ePart.Hand_Left == _part_control)
        {
            DebugWide.DrawLine(_shoulder_right.position, _hand_left.position, Color.red);

            Vector3 shaft = Vector3.Cross(_hand_left.position - _shoulder_right.position, _hand_right.position - _shoulder_right.position);
            //shaft.Normalize();
            //DebugWide.PrintText(_shoulder_right.position + Vector3.right, Color.red, Vector3.SignedAngle(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position, shaft) + "");    
            //DebugWide.DrawLine(_shoulder_right.position, _shoulder_right.position + shaft, Color.white);

        }
        if (ePart.Hand_Right == _part_control)
        {
            DebugWide.DrawLine(_shoulder_left.position, _hand_right.position, Color.red);

            Vector3 shaft = Vector3.Cross(_hand_right.position - _shoulder_left.position, _hand_left.position - _shoulder_left.position);
            DebugWide.PrintText(_shoulder_left.position + Vector3.left, Color.red, Vector3.SignedAngle(_hand_left.position - _shoulder_left.position, _hand_right.position - _shoulder_left.position, shaft) + "");
        }


        //궤적원에 따른 왼손 움직임 표현 
        if (true == _active_trajectoryCircle)
        {
            Geo.DeformationSpherePoint_Gizimo(Vector3.zero, _tc_center.position, _tc_radius, _tc_anchorA.position, _tc_anchorB.position, _tc_highest.position, 1);
        }

        //손조종 1
        if (true == _active_hand_control_1)
        {
            Vector3 objectDir = _hc1_object_dir.position - _hc1_standard.position;
            Vector3 targetDir = _hc1_target.position - _hc1_standard.position;
            Vector3 shaft_t = Vector3.Cross(objectDir, targetDir);
            DebugWide.DrawLine(_hc1_standard.position, _hc1_target.position, Color.black);
            DebugWide.DrawLine(_hc1_standard.position, _hc1_object_dir.position, Color.black);
            DebugWide.DrawLine(_hc1_standard.position, _hc1_standard.position + shaft_t, Color.white);

            float rsq = _arm_left_length * _arm_left_length;
            float wsq = (_hc1_standard.position - _shoulder_left.position).sqrMagnitude;
            float inArea = 1;
            if (rsq < wsq) inArea = -1f;
            DebugWide.DrawLine(_hc1_standard.position, _hc1_standard.position + _body_dir * inArea * 3, Color.yellow);
        }

        //손조종 2
        if (true == _active_hand_control_2)
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

            this.DrawCircleCone(_shoulder_right.position, _arm_right_length, axis_up, _hand_right.position - _shoulder_right.position, Color.cyan);
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
}