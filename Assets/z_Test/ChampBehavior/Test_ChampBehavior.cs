using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace HordeFight
{
    public class Test_ChampBehavior : MonoBehaviour
    {

        private ChampUnit _testChamp = null;

        string tempGui = "Cut and Sting";
        private void OnGUI()
        {

            //if (GUI.Button(new Rect(100, 100, 200, 100), tempGui))
            //{
            //}
        }

        private void OnDrawGizmos()
        {


        }

        public void CreateTestChamp(Transform champTR)
        {
            GameObject obj = champTR.gameObject;
            ChampUnit cha = obj.AddComponent<ChampUnit>();

            Movement mov = obj.AddComponent<Movement>();
            mov._being = cha;
            obj.AddComponent<AI>();
            cha._id = 0;
            cha._kind = ChampUnit.eKind.lothar;
            cha._belongCamp = null;
            cha.transform.position = champTR.position;

            ////==============================================
            ////가지(촉수) 등록
            Limbs limbs_hand = obj.GetComponentInChildren<Limbs>();
            limbs_hand.Init();
            limbs_hand._ref_movement = mov;
            limbs_hand._ref_being = cha;
            cha._limbs = limbs_hand;
            ////======================

            cha.Init();
            _testChamp = cha;
        }

        private void Start()
        {
            Misc.Init();
            SingleO.Init_Tool(gameObject); //싱글톤 객체 생성 , 초기화 

            GameObject gobj = GameObject.Find("Lothar");
            CreateTestChamp(gobj.transform);
        }



        private void Update()
        {

            //==================================================

            _testChamp.UpdateAll();
            _testChamp.Apply_UnityPosition();

            //==================================================

        }



        //======================================================================================================================================
        //======================================================================================================================================
        //======================================================================================================================================




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

        //public void TwoHandControl2_Left()
        //{
        //    Vector3 axis_forward = _L2R_axis_forward.position - _L2R_axis_o.position;
        //    Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
        //    //Vector3 Os1 = _shoulder_right.position - _hand_left.position;
        //    //Vector3 Lf = Vector3.Cross(axis_up, Os1);
        //    //Vector3 Os2 = Vector3.Cross(Lf,axis_up);
        //    //Vector3 up2 = Vector3.Cross(Os2, -Os1);

        //    //==================================================


        //    //손 움직임 만들기 
        //    Vector3 shoulderToCrossHand = _hand_left.position - _shoulder_right.position;
        //    float shoulderToCrossHand_length = shoulderToCrossHand.magnitude;

        //    //==================================================
        //    //삼각형 구성 불능 검사
        //    //if (shoulderToCrossHand.magnitude + _twoHand_length < _arm_right_length)
        //        //DebugWide.LogBlue("삼각형 구성 불능 : "+ shoulderToCrossHand_length);

        //    //==================================================

        //    //손 움직임 만들기 
        //    //코사인 제2법칙 공식을 사용하는 방식 
        //    float a = shoulderToCrossHand_length;
        //    float b = _arm_right_length;
        //    float c = _twoHand_length;

        //    //Acos 에는 0~1 사이의 값만 들어가야 함. 검사 : a+b < c 일 경우 음수값이 나옴 
        //    //if (a + b - c < 0)
        //    //c = (a + b) * 0.8f; //c의 길이를 표현 최대값의 80%값으로 설정  
        //    //a = (c - b) * 1.01f;

        //    float cosC = (a * a + b * b - c * c) / (2 * a * b);
        //    //DebugWide.LogBlue(cosC + "   " + Mathf.Acos(cosC));

        //    cosC = Mathf.Clamp01(cosC); //0~1사이의 값만 사용

        //    float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;
        //    Vector3 newPos_hR = shoulderToCrossHand.normalized * b;

        //    //회전축 구하기 
        //    Vector3 shaft = Vector3.Cross(shoulderToCrossHand, (_hand_right.position - _shoulder_right.position));
        //    //shaft = Vector3.left; //chamto test 0-------
        //    //shaft = -shaft_t; //chamto test 2-------
        //    //shaft = up2;
        //    //shaft = Lf;

        //    //shoulderToCrossHand 를 기준으로 내적값이 오른손이 오른쪽에 있으면 양수 , 왼쪽에 있으면 음수가 된다 
        //    //위 내적값으로 shoulderToCrossHand 기준으로 양쪽으로 오른손이 회전을 할 수 있게 한다 
        //    if(Vector3.Dot(axis_up , shaft) >= 0)
        //        shaft = axis_up;
        //    else 
        //        shaft = -axis_up;

        //    //shaft = Quaternion.AngleAxis(-angleC, axis_forward) * shaft; //chamto test

        //    newPos_hR = _shoulder_right.position + Quaternion.AngleAxis(angleC, shaft) * newPos_hR;
        //    _hand_right.position = newPos_hR;
        //    _arm_right_length = (_shoulder_right.position - _hand_right.position).magnitude; //임시 

        //}



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



        public class Quat
        {
            public float _x, _y, _z, _w;

            public void Set(float w, float x, float y, float z)
            {
                _w = w; _x = x; _y = y; _z = z;
            }

            public void Identity()
            {
                _x = _y = _z = 0.0f;
                _w = 1.0f;
            }

            public void Normalize()
            {
                float lengthsq = _w * _w + _x * _x + _y * _y + _z * _z;

                if (Misc.IsZero(lengthsq))
                {
                    _x = _y = _z = _w = 0f;
                }
                else
                {
                    float factor = 1f / (float)System.Math.Sqrt(lengthsq);
                    _w *= factor;
                    _x *= factor;
                    _y *= factor;
                    _z *= factor;
                }

            }

            //angle_rd : 라디안 값 넣어야함 
            public Quaternion AngleAxis(float angle_rd, Vector3 axis)
            {
                // if axis of rotation is zero vector, just set to identity quat
                float sqrLength = axis.sqrMagnitude;
                if (Misc.IsZero(sqrLength))
                {
                    Identity();
                    return Quaternion.identity;
                }

                // take half-angle
                angle_rd *= 0.5f;

                float sintheta = 0, costheta = 0;
                sintheta = (float)System.Math.Sin(angle_rd);
                costheta = (float)System.Math.Cos(angle_rd);


                float scaleFactor = sintheta / (float)System.Math.Sqrt(sqrLength);

                _w = costheta;
                _x = scaleFactor * axis.x;
                _y = scaleFactor * axis.y;
                _z = scaleFactor * axis.z;


                return new Quaternion(_x, _y, _z, _w);
            }

            public Quaternion FromToRotation(Vector3 from, Vector3 to)
            {

                // get axis of rotation
                Vector3 axis = Vector3.Cross(from, to);


                // get scaled cos of angle between vectors and set initial quaternion
                Set(Vector3.Dot(from, to), axis.x, axis.y, axis.z);
                // quaternion at this point is ||from||*||to||*( cos(theta), r*sin(theta) )

                // normalize to remove ||from||*||to|| factor
                Normalize();
                // quaternion at this point is ( cos(theta), r*sin(theta) )
                // what we want is ( cos(theta/2), r*sin(theta/2) )

                // set up for half angle calculation
                _w += 1.0f;

                // now when we normalize, we'll be dividing by sqrt(2*(1+cos(theta))), which is 
                // what we want for r*sin(theta) to give us r*sin(theta/2)  (see pages 487-488)
                // 
                // w will become 
                //                 1+cos(theta)
                //            ----------------------
                //            sqrt(2*(1+cos(theta)))        
                // which simplifies to
                //                cos(theta/2)

                // before we normalize, check if vectors are opposing
                if (_w <= float.Epsilon)
                {
                    // rotate pi radians around orthogonal vector
                    // take cross product with x axis
                    if (from.z * from.z > from.x * from.x)
                        Set(0.0f, 0.0f, from.z, -from.y);
                    // or take cross product with z axis
                    else
                        Set(0.0f, from.y, -from.x, 0.0f);
                }

                // normalize again to get rotation quaternion
                Normalize();

                return new Quaternion(_x, _y, _z, _w);

            }
        }
    }


    //=========================================================


}
