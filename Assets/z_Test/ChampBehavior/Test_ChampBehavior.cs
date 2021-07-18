using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace HordeFight
{
    public partial class Test_ChampBehavior : MonoBehaviour
    {

        public ChampUnit _champ_0 = null;
        public ChampUnit _champ_1 = null;

        public Transform _effect = null;

        //private MovingSegement3 _movingSegment = new MovingSegement3();
        private MovingModel _movingModel = new MovingModel();
        public MovingModel.Frame _frame_ch_0 = new MovingModel.Frame();
        public MovingModel.Frame _frame_ch_1 = new MovingModel.Frame();

        //string tempGui = "Cut and Sting";
        private void OnGUI()
        {

            //if (GUI.Button(new Rect(100, 100, 200, 100), tempGui))
            //{
            //}
        }

        private void OnDrawGizmos()
        {
            
            _movingModel.Draw();
            //_frame_ch_0.Draw(Color.red);
        }

        public ChampUnit CreateTestChamp(Transform champTR, ChampUnit.eKind eKind)
        {
            GameObject obj = champTR.gameObject;
            ChampUnit cha = obj.AddComponent<ChampUnit>();

            cha._move = obj.AddComponent<Movement>();
            cha._move._being = cha;

            cha.SetPos(champTR.position);

            //mov._eDir8 = eDirection8.up;
            cha._ai = obj.AddComponent<AI>();
            cha._ai.Init();
            cha._id = 0;
            cha._kind = eKind;
            cha._belongCamp = null;
            cha._collider = obj.GetComponent<SphereCollider>();
            cha._collider_radius = cha._collider.radius;
            cha._collider_sqrRadius = cha._collider_radius * cha._collider_radius;

            ////==============================================
            ////가지(촉수) 등록
            cha._bone.Load(champTR);
            cha._limbs = obj.GetComponentInChildren<Limbs>();
            cha._limbs.Init(cha, cha._move, cha._bone);

            ////======================

            cha.Init();

            return cha;
        }

        private void Start()
        {
            Misc.Init();
            SingleO.Init_Tool(gameObject); //싱글톤 객체 생성 , 초기화 
            gameObject.AddComponent<TouchControlTool>();

            GameObject gobj = GameObject.Find("lothar");
            _champ_0 = CreateTestChamp(gobj.transform, ChampUnit.eKind.lothar);
            _frame_ch_0.Init(_champ_0.transform, "bone");
            _frame_ch_0._info[0].radius = _champ_0._collider_radius;

            gobj = GameObject.Find("footman");
            _champ_1 = CreateTestChamp(gobj.transform, ChampUnit.eKind.footman);
            _frame_ch_1.Init(_champ_1.transform , "bone");
            _frame_ch_1._info[0].radius = _champ_1._collider_radius;

            _effect = Hierarchy.GetTransform(gobj.transform, "emotion");
            //DebugWide.LogBlue(_champ_0.transform.name);

            _champ_0.UpdateAll();
            _champ_1.UpdateAll();


            //_movingModel.Init(_champ_0._limbs._armed_left._tr_frame, _champ_1._limbs._armed_left._tr_frame);

            //_movingModel.SetFrame(false, false, _frame_ch_0, _frame_ch_1);
            _movingModel.SetFrame(false, false, _champ_0._limbs._armed_left._frame, _frame_ch_1);
            //_movingModel.SetFrame(true, true, _champ_0._limbs._armed_left._frame, _champ_1._limbs._armed_left._frame);

        }




        public Interpolation.eKind __interKind = Interpolation.eKind.spring;
        private bool __reverse = false;
        private float __time_elapsed = 0;
        public float __RateAtoB = 1f;

		private void Update()
		{

            if(null != _effect)
            {
                //_effect.Rotate(ConstV.v3_up, -3f);

                Transform champ0_ani = Hierarchy.GetTransform(_champ_0.transform, "ani_spr");
                if (1 < __time_elapsed) 
                {
                    __reverse = !__reverse;
                    __time_elapsed = 0f;
                }
                //Interpolation.CalcShakePosition(champ0_ani.transform,_champ_0.transform.position , new Vector3(0, 0, 0.1f), __time_elapsed);
                //Interpolation.CalcShakeRotation(champ0_ani.transform, ConstV.v3_zero , new Vector3(0f, 0, 90), __time_elapsed);
                //Interpolation.CalcShakeScale(champ0_ani.transform, ConstV.v3_one,  new Vector3(0f, 0, 0.2f), __time_elapsed);

                //Interpolation.CalcPosition(champ0_ani.transform, _champ_0.transform.position, _champ_0.transform.position + new Vector3(0f, 0, 0.4f), __time_elapsed, __interKind, __reverse);
                ////Interpolation.CalcRotation(champ0_ani.transform, ConstV.v3_zero, new Vector3(0, 0, 360f), __time_elapsed, __interKind, __reverse);
                //Interpolation.CalcScale(champ0_ani.transform, ConstV.v3_one, new Vector3(1f, 1, 1.4f), __time_elapsed, __interKind, __reverse);


                //Interpolation.CalcShakePosition(_effect, _champ_1.transform.position, new Vector3(0.1f, 0, 0.2f), __time_elapsed);
                //Interpolation.CalcShakeRotation(_effect, _champ_1.transform.eulerAngles, new Vector3(0, 45f, 0), __time_elapsed);
                //Interpolation.CalcShakeScale(_effect, _champ_1.transform.localScale, new Vector3(0.2f, 0, 0.2f), __time_elapsed);

                ////Interpolation.CalcRotation(_effect, ConstV.v3_zero, new Vector3(0, -360f, 0), __time_elapsed, __interKind, false);

                __time_elapsed += Time.deltaTime;


            }


            //==================================================

            _champ_0.UpdateAll();
            //_champ_0._limbs.Update_Frame();
            _champ_1.UpdateAll();
            //_champ_1._limbs.Update_Frame();

            //==================================================

            if (_champ_0._limbs._isUpdateEq_handLeft || _champ_0._limbs._isUpdateEq_handRight ||
               _champ_1._limbs._isUpdateEq_handLeft || _champ_1._limbs._isUpdateEq_handRight)
            {
                //무기장착 정보가 변경되었다면 변경된 값으로 갱신시켜준다 
                //_movingModel._frame_A = _champ_0._limbs._armed_left._frame;
                //_movingModel._frame_B = _champ_1._limbs._armed_left._frame;
                //_movingModel.Init_Prev_AB_Order();
                _movingModel.SetFrame(true, true,_champ_0._limbs._armed_left._frame, _champ_1._limbs._armed_left._frame);

                DebugWide.LogBlue("무기정보 갱신 !!! ");
            }
            _movingModel._rateAtoB = __RateAtoB;
            if (true == _movingModel.Update())
            {
                //계산된 회전값을 sting , cut 에 적용하기

                //임시처리 
                _champ_0._limbs._bone._hand_left.rotation = _movingModel._frame_A._tr_frame.rotation;
                _champ_0._limbs._bone._hand_left.position = _movingModel._frame_A._tr_frame.position;
                Vector3 pos_o = _champ_0._limbs._bone._hand_left.position;
                Vector3 dir_two = _movingModel._frame_A._tr_frame.forward;
                float len_two = _champ_0._limbs._twoHand_cut_length;
                _champ_0._limbs._bone._hand_right.position = pos_o + dir_two.normalized * len_two;

                _champ_1._limbs._bone._hand_left.rotation = _movingModel._frame_B._tr_frame.rotation;
                _champ_1._limbs._bone._hand_left.position = _movingModel._frame_B._tr_frame.position;
                pos_o = _champ_1._limbs._bone._hand_left.position;
                dir_two = _movingModel._frame_B._tr_frame.forward;
                len_two = _champ_1._limbs._twoHand_cut_length;
                _champ_1._limbs._bone._hand_right.position = pos_o + dir_two.normalized * len_two;

                _movingModel._sum_dir_move_A.y = 0;
                _movingModel._sum_dir_move_B.y = 0;
                _champ_0.SetPos(_champ_0.GetPos3D() + _movingModel._sum_dir_move_A);
                _champ_1.SetPos(_champ_1.GetPos3D() + _movingModel._sum_dir_move_B);
            }

            //==================================================

            //_champ_0._limbs.Update_View();
            //_champ_1._limbs.Update_View();

            _champ_0.Apply_UnityPosition();
            _champ_1.Apply_UnityPosition();

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
    }


    //=========================================================


    public class TouchControlTool : MonoBehaviour
    {
        public Being _selected = null;
        private Test_ChampBehavior _test_ChampBehavior = null;

        private void Start()
        {
            SingleO.touchEvent.Attach_SendObject(this.gameObject);
            _test_ChampBehavior = gameObject.GetComponent<Test_ChampBehavior>();
        }

        private void Update()
        {
            if (null == _selected) return;
            if (_selected.isDeath())
            {
                _selected = null;
            }

        }

        private Vector3 __startPos = ConstV.v3_zero;
        private void TouchBegan()
        {
            ChampUnit champ = null;
            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            __startPos = hit.point;
            __startPos.y = 0f;

            //DebugWide.LogBlue(hit + "  " + hit.transform.name);
            Being getBeing = hit.transform.GetComponent<Being>();
            if (null != (object)getBeing)
            {
                //쓰러진 객체는 처리하지 않는다 
                if (true == getBeing.isDeath()) return;

                //------------------------------------------
                //전 객체 선택 해제 
                if (null != (object)_selected)
                {
                    champ = _selected as ChampUnit;
                    if (null != champ)
                    {
                        //champ.GetComponent<AI>()._ai_running = true;

                    }
                }
                //------------------------------------------

                //새로운 객체 선택
                _selected = getBeing;

                champ = _selected as ChampUnit;
                if (null != (object)champ)
                {
                    //_selected.GetComponent<AI>()._ai_running = false;

                }
                //------------------------------------------

                SingleO.cameraWalk.SetTarget(_selected._transform);
            }

            //===============================================

            if (null == (object)_selected) return;

            //챔프를 선택한 경우, 추가 처리 하지 않는다
            if ((object)getBeing == (object)_selected) return;

            //_selected.MoveToTarget(hit.point, 1f);


        }
        private void TouchMoved()
        {
            if (null == (object)_selected) return;

            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            Vector3 touchDir = VOp.Minus(hit.point, _selected.GetPos3D());


            ChampUnit champTarget = null;
            ChampUnit champSelected = _selected as ChampUnit;
            if (null != (object)champSelected)
            {
                if ((object)champSelected == (object)_test_ChampBehavior._champ_0)
                {
                    champTarget = _test_ChampBehavior._champ_1;
                }
                else
                {
                    champTarget = _test_ChampBehavior._champ_0;
                }
            }


            _selected.Move_LookAt(touchDir, champTarget.GetPos3D() - champSelected.GetPos3D(), 1f);

        }
        private void TouchEnded()
        {
            if (null == (object)_selected) return;

            _selected.Idle();

        }


    }//end class

}
