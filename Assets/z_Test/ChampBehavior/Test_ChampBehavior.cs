using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Transform _shoulder_left = null;
    public Transform _shoulder_right = null;
    public Transform _hand_left = null;
    public Transform _hand_right = null;
    public Transform _object_sword = null;

    public float _shoulder_length = 0f;
    public float _arm_left_length = 1f;
    public float _arm_right_length = 1.5f;
    public float _twoHand_length = 0.7f;

    public ePart _part_control = ePart.Hand_Left;

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

	private void Start()
	{
        _shoulder_left = GameObject.Find("shoulder_left").transform;
        _shoulder_right = GameObject.Find("shoulder_right").transform;
        _hand_left = GameObject.Find("hand_left").transform;
        _hand_right = GameObject.Find("hand_right").transform;
        _object_sword = GameObject.Find("object_sword").transform;

        //=======
        //조종항목
        _pos_handLeft_aroundRotate = GameObject.Find("pos_handLeft_aroundRotate").transform;
        _pos_handRight_aroundRotate = GameObject.Find("pos_handRight_aroundRotate").transform;

	}


    public Vector3 __prev_handL_Pos = UtilGS9.ConstV.v3_zero;
    public Vector3 __prev_hLsL = UtilGS9.ConstV.v3_zero;
	private void Update()
	{

        //==================================================

        if (null == (object)_shoulder_left || null == (object)_shoulder_right) return;

        Vector3 shLR = _shoulder_left.position - _shoulder_right.position;
        _shoulder_length = shLR.magnitude;

        //==================================================

        if (null == (object)_hand_left || null == (object)_hand_right) return;

        Vector3 hLsL = (_hand_left.position - _shoulder_left.position);
        Vector3 hRsR = (_hand_right.position - _shoulder_right.position);
        Vector3 n_hLsL = hLsL.normalized;
        Vector3 n_hRsR = hRsR.normalized;


        //==================================================
        //위치 제약 
        //if( 0.5f < _arm_left_length)
        //{
        //    _hand_left.position = __prev_handL_Pos;
        //}
        //__prev_handL_Pos = _hand_left.position;

        _hand_left.position = _shoulder_left.position + n_hLsL * _arm_left_length;
        _hand_right.position = _shoulder_right.position + n_hRsR * _arm_right_length;

        //다시 갱신 
        hLsL = _hand_left.position - _shoulder_left.position;
        hRsR = _hand_right.position - _shoulder_right.position;


        //==================================================

        //변경된 각도만큼 증가시키는 방식 - 왼손과 오른손 사이의 거리가 동일하게 유지가 안된다 
        //float angle = Vector3.SignedAngle(__prev_hLsL, hLsL, UtilGS9.ConstV.v3_up);
        //Vector3 newHRsR =  Quaternion.AngleAxis(angle, UtilGS9.ConstV.v3_up) * hRsR;
        //newHRsR += _shoulder_right.position;
        //_hand_right.position = newHRsR;

        //__prev_hLsL = hLsL;

        //==================================================
        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        if (ePart.Hand_Left == _part_control)
            HandLeft_Control();
        if (ePart.Hand_Right == _part_control)
            HandRight_Control();
        

        //DebugWide.LogBlue(shaft + "    angle: " +angleC +"  a:" +a+ "   b:" +b+ "   c:" + c);
        //DebugWide.LogBlue(newPos_hR.x + "  " + newPos_hR.z + "    :" + angleC + "   p:" + (a+b-c));
        //DebugWide.LogBlue(angleC + " a : " + Quaternion.FromToRotation(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position).eulerAngles.y);
        //DebugWide.LogBlue(angleC + " b : " + Vector3.SignedAngle(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position, Vector3.up));


        //==================================================
        //왼손,오른손 자동 회전 테스트
        if(true == _active_shoulder_autoRotate)
        {
            Vector3 axis = Vector3.right; //chamto test
            _shoulder_left.Rotate(axis, _angle_shoulderLeft_autoRotate, Space.World);
            _shoulder_right.Rotate(axis, _angle_shoulderRight_autoRotate, Space.World);
        }
            

        //==================================================
        //몸통 중심으로 오른손 회전하기 
        if(true == _active_body_aroundRotate)
        {

            Vector3 bodyCenter = _pos_handRight_aroundRotate.position;
            Vector3 n_bodyToHandRight = (_hand_right.position - bodyCenter).normalized;
            _hand_right.position = bodyCenter + n_bodyToHandRight * _radius_handRight_aroundRotate;
            _arm_right_length = (_hand_right.position - _shoulder_right.position).magnitude;

            bodyCenter = _pos_handLeft_aroundRotate.position;
            Vector3 n_bodyToHandLeft = (_hand_left.position - bodyCenter).normalized;
            _hand_left.position = bodyCenter + n_bodyToHandLeft * _radius_handLeft_aroundRotate;
            _arm_left_length = (_hand_left.position - _shoulder_left.position).magnitude;
        }

        //==================================================
        //손에 칼 붙이기 3d
        Vector3 hLhR = _hand_right.position - _hand_left.position;
        Vector3 obj_shaft = Vector3.Cross(Vector3.forward, hLhR);
        //angleC의 각도가 0이 나올 경우 외적값이 0이 된다. 각도가 0일때 물건을 손에 붙이는 계산이 안되는 문제가 발생함
        //물건 기준으로 외적값을 구해 사용하면 문제가 해결됨 

        float angleW = Vector3.SignedAngle(Vector3.forward, hLhR, obj_shaft);
        _object_sword.rotation = Quaternion.AngleAxis(angleW, obj_shaft);

	}


    public void HandRight_Control()
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
    public void HandLeft_Control()
    {
        //손 움직임 만들기 
        Vector3 shoulderToCrossHand = _hand_left.position - _shoulder_right.position;

        //손 움직임 만들기 
        //코사인 제2법칙 공식을 사용하는 방식 
        float a = shoulderToCrossHand.magnitude;
        float b = _arm_right_length;
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
        Vector3 shaft = Vector3.Cross(shoulderToCrossHand, (_hand_right.position - _shoulder_right.position));
        shaft = Vector3.left; //chamto test -------

        newPos_hR = _shoulder_right.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        _hand_right.position = newPos_hR;
    }

	private void OnDrawGizmos()
	{
        Vector3 sLsR = _shoulder_right.position - _shoulder_left.position;
        Vector3 hLsL = _hand_left.position - _shoulder_left.position;
        Vector3 hRsR = _hand_right.position - _shoulder_right.position;
        Vector3 hLhR = _hand_left.position - _hand_right.position;

        DebugWide.PrintText(_shoulder_left.position + hLsL * 0.5f, Color.white, "armL "+_arm_left_length.ToString("00.00"));
        DebugWide.PrintText(_shoulder_right.position + hRsR * 0.5f, Color.white, "armR " + _arm_right_length.ToString("00.00"));
        DebugWide.PrintText(_shoulder_left.position + sLsR * 0.5f, Color.white, "shoulder " + _shoulder_length.ToString("00.00"));
        DebugWide.PrintText(_hand_right.position + hLhR * 0.5f, Color.white, "twoH " + _twoHand_length.ToString("00.00"));

        DebugWide.DrawLine(_shoulder_left.position, _hand_left.position, Color.green);
        DebugWide.DrawLine(_shoulder_right.position, _hand_right.position, Color.green);
        DebugWide.DrawLine(_hand_right.position, _hand_left.position, Color.gray);

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
            Vector3 shaft = Vector3.Cross(_hand_left.position - _shoulder_right.position, _hand_right.position - _shoulder_right.position);
            DebugWide.DrawLine(_shoulder_right.position, _hand_left.position, Color.red);
            DebugWide.PrintText(_shoulder_right.position + Vector3.right, Color.red, Vector3.SignedAngle(_hand_right.position - _shoulder_right.position, _hand_left.position - _shoulder_right.position, shaft) + "");    
        }
        if (ePart.Hand_Right == _part_control)
        {
            Vector3 shaft = Vector3.Cross(_hand_right.position - _shoulder_left.position, _hand_left.position - _shoulder_left.position);
            DebugWide.DrawLine(_shoulder_left.position, _hand_right.position, Color.red);
            DebugWide.PrintText(_shoulder_left.position + Vector3.right, Color.red, Vector3.SignedAngle(_hand_left.position - _shoulder_left.position, _hand_right.position - _shoulder_left.position, shaft) + "");
        }
	}
}