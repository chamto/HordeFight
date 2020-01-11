using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace HordeFight
{
    //가지 , 팔다리 
    public class Limbs : MonoBehaviour
    {

        public class Control
        {
        }

        public class PathCircle
        {
        }


        public enum ePart
        {
            None,
            OneHand,
            TwoHand,

            TwoHand_LeftO, //왼손기준이 되는 잡기(왼손이 고정)
            TwoHand_RightO, //오른손기준이 되는 잡기(오른손이 고정)

        }

        public enum eStance
        {
            Cut,    //베기
            Sting,  //찌르기
            Block,  //막기
            Etc,    //그외

            Max,
        }

        //------------------------------------------------------

        //todo : 그림자 설정은 나중에 따로 빼기 
        public Vector3 _light_dir; //방향성 빛
        public Vector3 _groundY; //땅표면 

        //------------------------------------------------------

        public Transform _tbody_dir = null;
        public Transform _shoulder_left = null;
        public Transform _shoulder_right = null;
        public Transform _hand_left = null;
        public Transform _hand_right = null;
        public Transform _object_left = null;
        public Transform _object_right = null;
        public Transform _odir_left = null; //한손으로 쥐고 있었을 떄의 물체의 방향.
        public Transform _odir_right = null;


        //핸들 
        public Transform _HANDLE_twoHand = null;
        public Transform _HANDLE_oneHand = null;
        public Transform _HANDLE_left = null;   //핸들
        public Transform _HANDLE_right = null;

        //찌르기용 핸들 
        public Transform _hs_objectDir = null;
        public Transform _hs_standard = null;

        //목표
        public Transform[] _target = new Transform[2]; //임시 최대2개 목표 


        public Transform _hand_left_spr = null;
        public Transform _hand_right_spr = null;

        private Transform _hand_left_obj = null;
        private Transform _hand_left_obj_up = null;
        private Transform _hand_left_obj_end = null;
        private Transform _hand_left_obj_shader = null;
        private SpriteRenderer _hand_left_obj_spr = null;
        private MeshRenderer _hand_left_obj_mesh = null;

        private Transform _hand_right_obj = null;
        private Transform _hand_right_obj_up = null;
        private Transform _hand_right_obj_end = null;
        private Transform _hand_right_obj_shader = null;
        private SpriteRenderer _hand_right_obj_spr = null;
        private MeshRenderer _hand_right_obj_mesh = null;

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

        public ePart _part_control = ePart.TwoHand; //조종부위 <한손 , 양손 , 한다리 , 꼬리 등등>
        public ePart _eHandOrigin = ePart.TwoHand_LeftO; //고정으로 잡는 손지정(부위지정)  
        public eStance _eStance = eStance.Cut; //자세 

        public bool _active_shadowObject = true;

        public bool _active_projectionSlope = true;
        public float _boxSlope_angle = 75f;

        private Geo.Model _Model_left_0 = new Geo.Model(); //a
        private Geo.Model _Model_left_1 = new Geo.Model(); //a
        private Geo.Model _Model_right_0 = new Geo.Model();//b 
        private Geo.Model _Model_right_1 = new Geo.Model();//b

        private MotionTrajectory _motion_oneHand_left_0 = null;
        private MotionTrajectory _motion_oneHand_left_1 = null;
        private MotionTrajectory _motion_oneHand_right_0 = null;
        private MotionTrajectory _motion_oneHand_right_1 = null;
        private MotionTrajectory _motion_twoHand_left = null;
        private MotionTrajectory _motion_twoHand_right = null;


        //======================================================

        //양손 조종용 경로원
        private Transform _pos_circle_left = null;
        private Transform _upDir_circle_left = null;
        private Transform _highest_circle_left = null;
        private Transform _tornado_unlace_left = null;

        private Transform _pos_circle_right = null;
        private Transform _upDir_circle_right = null;
        private Transform _highest_circle_right = null;
        private Transform _tornado_unlace_right = null;

        //왼손 조종용 경로원
        //[ 손목 ]
        private Transform _pos_circle_A0 = null;
        private Transform _upDir_circle_A0 = null;
        private Transform _highest_circle_A0 = null;
        private Transform _tornado_unlace_A0 = null;
        //[ 손잡이 ]
        private Transform _pos_circle_A1 = null;
        private Transform _upDir_circle_A1 = null;
        private Transform _highest_circle_A1 = null;
        private Transform _tornado_unlace_A1 = null;

        //오른손 조종용 경로 
        //[ 손목 ]
        private Transform _pos_circle_B0 = null;
        private Transform _upDir_circle_B0 = null;
        private Transform _highest_circle_B0 = null;
        private Transform _tornado_unlace_B0 = null;
        //[ 손잡이 ]
        private Transform _pos_circle_B1 = null;
        private Transform _upDir_circle_B1 = null;
        private Transform _highest_circle_B1 = null;
        private Transform _tornado_unlace_B1 = null;


        //======================================================

        static public GameObject CreatePrefab(string prefabPath, Transform parent, string name)
        {
            const string root = "Warcraft/Prefab/3_limbs/";
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(root + prefabPath)) as GameObject;
            obj.transform.SetParent(parent, false);
            obj.transform.name = name;


            return obj;
        }

        static public Limbs CreateLimbs_TwoHand(Transform parent)
        {
            
            GameObject limbPrefab = Limbs.CreatePrefab("limbs_twoHand", parent, "limbs_twoHand");
            Limbs limbs = limbPrefab.AddComponent<Limbs>();

            return limbs;
        }


        public void Init()
        {

            //--------------------------------------------------

            _Model_left_0.branch = Geo.Model.eBranch.arm_left_0;
            _Model_left_1.branch = Geo.Model.eBranch.arm_left_0;
            _Model_right_0.branch = Geo.Model.eBranch.arm_right_0;
            _Model_right_1.branch = Geo.Model.eBranch.arm_right_0;



            //==================================================
            //1차 자식 : root
            //==================================================
            Transform root = SingleO.hierarchy.GetTransform(transform, "root");
            //root->waist
            Transform waist = SingleO.hierarchy.GetTransform(root, "waist");
            //root->waist->body_dir
            _tbody_dir = SingleO.hierarchy.GetTransform(root, "body_dir");


            //-------------------------
            _shoulder_left = SingleO.hierarchy.GetTransform(waist, "shoulder_left");
            //root->waist->shoulder_left->hand_left
            _hand_left = SingleO.hierarchy.GetTransform(_shoulder_left, "hand_left");
            //root->waist->shoulder_left->hand_left->object_left
            _object_left = SingleO.hierarchy.GetTransform(_hand_left, "object_left");
            _odir_left = SingleO.hierarchy.GetTransform(_hand_left, "odir_left");
            _hand_left_spr = SingleO.hierarchy.GetTransform(_hand_left, "spr");
            _hand_left_obj_shader = SingleO.hierarchy.GetTransform(_hand_left, "shader");
            //-------------------------
            _hand_left_obj = SingleO.hierarchy.GetTransform(_object_left, "spear");
            _hand_left_obj_mesh = _hand_left_obj.GetComponent<MeshRenderer>();
            _hand_left_obj_up = SingleO.hierarchy.GetTransform(_object_left, "object_left_up");
            _hand_left_obj_end = SingleO.hierarchy.GetTransform(_hand_left_obj, "object_left_end");
            _hand_left_obj_spr = SingleO.hierarchy.GetTypeObject<SpriteRenderer>(_object_left, "spear");

            //-------------------------
            _shoulder_right = SingleO.hierarchy.GetTransform(waist, "shoulder_right");
            //root->waist->shoulder_right->hand_right
            _hand_right = SingleO.hierarchy.GetTransform(_shoulder_right, "hand_right");
            //root->waist->shoulder_right->hand_right->object_right
            _object_right = SingleO.hierarchy.GetTransform(_hand_right, "object_right");
            _odir_right = SingleO.hierarchy.GetTransform(_hand_right, "odir_right");
            _hand_right_spr = SingleO.hierarchy.GetTransform(_hand_right, "spr");
            //-------------------------



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
            //-------------------------
            //ctr->handle_oneHand
            _HANDLE_oneHand = SingleO.hierarchy.GetTransform(ctr, "handle_oneHand");
            _target[0] = SingleO.hierarchy.GetTransform(_HANDLE_oneHand, "target_1");
            _target[1] = SingleO.hierarchy.GetTransform(_HANDLE_oneHand, "target_2");
            //-------------------------
            _HANDLE_left = SingleO.hierarchy.GetTransform(_target[0], "handle_left"); //핸들 
            _HANDLE_right = SingleO.hierarchy.GetTransform(_target[1], "handle_right");
            //-------------------------



            //==================================================
            //1차 자식 : path_circle 조종항목
            //==================================================
            Transform path_circle = SingleO.hierarchy.GetTransform(transform, "path_circle");

            //-------------------------
            Transform two_cut = SingleO.hierarchy.GetTransform(path_circle, "two_cut");

            _pos_circle_left = SingleO.hierarchy.GetTransform(two_cut, "pos_circle_left");
            _motion_twoHand_left = _pos_circle_left.GetComponent<MotionTrajectory>();
            _upDir_circle_left = SingleO.hierarchy.GetTransform(_pos_circle_left, "upDir_circle_left");
            _highest_circle_left = SingleO.hierarchy.GetTransform(_pos_circle_left, "highest_circle_left");
            _tornado_unlace_left = SingleO.hierarchy.GetTransform(_highest_circle_left, "tornado_unlace_left");
            //-------------------------
            _pos_circle_right = SingleO.hierarchy.GetTransform(two_cut, "pos_circle_right");
            _motion_twoHand_right = _pos_circle_right.GetComponent<MotionTrajectory>();
            _upDir_circle_right = SingleO.hierarchy.GetTransform(_pos_circle_right, "upDir_circle_right");
            _highest_circle_right = SingleO.hierarchy.GetTransform(_pos_circle_right, "highest_circle_right");
            _tornado_unlace_right = SingleO.hierarchy.GetTransform(_highest_circle_right, "tornado_unlace_right");

            //----------------------------------------------

            Transform one_left = SingleO.hierarchy.GetTransform(path_circle, "one_left");

            _pos_circle_A0      = SingleO.hierarchy.GetTransform(one_left, "pos_circle_A0");
            _motion_oneHand_left_0 = _pos_circle_A0.GetComponent<MotionTrajectory>();
            _upDir_circle_A0    = SingleO.hierarchy.GetTransform(_pos_circle_A0, "upDir_circle_A0");
            _highest_circle_A0  = SingleO.hierarchy.GetTransform(_pos_circle_A0, "highest_circle_A0");
            _tornado_unlace_A0  = SingleO.hierarchy.GetTransform(_highest_circle_A0, "tornado_unlace_A0");
            //-------------------------
            _pos_circle_A1      = SingleO.hierarchy.GetTransform(_pos_circle_A0, "pos_circle_A1");
            _motion_oneHand_left_1 = _pos_circle_A1.GetComponent<MotionTrajectory>();
            _upDir_circle_A1    = SingleO.hierarchy.GetTransform(_pos_circle_A1, "upDir_circle_A1");
            _highest_circle_A1  = SingleO.hierarchy.GetTransform(_pos_circle_A1, "highest_circle_A1");
            _tornado_unlace_A1  = SingleO.hierarchy.GetTransform(_highest_circle_A1, "tornado_unlace_A1");

            //----------------------------------------------

            Transform one_right = SingleO.hierarchy.GetTransform(path_circle, "one_right");

            _pos_circle_B0      = SingleO.hierarchy.GetTransform(one_right, "pos_circle_B0");
            _motion_oneHand_right_0 = _pos_circle_B0.GetComponent<MotionTrajectory>();
            _upDir_circle_B0    = SingleO.hierarchy.GetTransform(_pos_circle_B0, "upDir_circle_B0");
            _highest_circle_B0  = SingleO.hierarchy.GetTransform(_pos_circle_B0, "highest_circle_B0");
            _tornado_unlace_B0  = SingleO.hierarchy.GetTransform(_highest_circle_B0, "tornado_unlace_B0");
            //-------------------------
            _pos_circle_B1      = SingleO.hierarchy.GetTransform(_pos_circle_B0, "pos_circle_B1");
            _motion_oneHand_right_1 = _pos_circle_B1.GetComponent<MotionTrajectory>();
            _upDir_circle_B1    = SingleO.hierarchy.GetTransform(_pos_circle_B1, "upDir_circle_B1");
            _highest_circle_B1  = SingleO.hierarchy.GetTransform(_pos_circle_B1, "highest_circle_B1");
            _tornado_unlace_B1  = SingleO.hierarchy.GetTransform(_highest_circle_B1, "tornado_unlace_B1");

            //-------------------------


            //==================================================

        }

        public void Update_All()
        {
            //몸 방향값 갱신 
            //_body_dir = (_tbody_dir.position - transform.position).normalized; //사용처가 없음 

            _light_dir = SingleO.lightDir.position;
            _groundY = SingleO.groundY.position;

            //==================================================

            if (null == (object)_shoulder_left || null == (object)_shoulder_right) return;

            Vector3 shLR = _shoulder_left.position - _shoulder_right.position;
            _shoulder_length = shLR.magnitude;

            //==================================================

            //손 움직임 만들기 
            Update_HandControl();

            //2d 게임에서의 높이표현 
            if(_active_projectionSlope)
            {
                _hand_right.position = Project_BoxSlope(_hand_right.position, (_hand_right.position - _groundY).y);
                _hand_left.position = Project_BoxSlope(_hand_left.position, (_hand_left.position - _groundY).y);
            }
            //==================================================
            //손에 칼 붙이기
            Update_AttachHand();

            //그림자 표현
            Update_Shadow();

            //물건 움직임에 따라 손 스프라이트 표현 
            Update_HandAni();
            //==================================================
        }

        //==================================================

        //2d 게임같은 높이값을 표현한다. 기울어진 투영상자의 빗면에 높이값을 투영한다.
        //그 길이를 대상위치에 z축 값을 더한다  
        public Vector3 Project_BoxSlope(Vector3 target, float heightY)
        {
            float angle = 90f - _boxSlope_angle;
            float b = Mathf.Tan(angle * Mathf.Deg2Rad) * Mathf.Abs(heightY);

            target.z += b;
            return target;
        }


        public void Rotate(Vector3 dir)
        {
            Vector3 temp = transform.localEulerAngles;
            temp.y = Geo.AngleSigned_AxisY(ConstV.v3_forward, dir);
            transform.localEulerAngles = temp;
        }


        public void Update_HandControl()
        {
            if (_part_control == ePart.OneHand)
            {
                if (eStance.Sting == _eStance)
                {   //찌르기

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
                else if (eStance.Cut == _eStance)
                {   //베기

                    Cut_OneHand();
                }

            }

            //==================================================

            if (_part_control == ePart.TwoHand)
            {
                if (eStance.Sting == _eStance)
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


                }
                //================================================================
                else if (eStance.Cut == _eStance)
                {   //베기 


                    //-----------------------

                    Cut_TwoHand(_HANDLE_twoHand.position, _eHandOrigin, _motion_twoHand_left._eModel, _motion_twoHand_right._eModel); //_eModelKind_Left_0, _eModelKind_Right_0);

                    //--------------------
                    //찌르기 모드로 연결하기 위한 핸들값 조정 
                    _hs_standard.position = _hand_left.position;
                    _hs_objectDir.position = _hand_right.position;
                }//else end
            }

        }

        public Vector3 CalcShadowPos(Vector3 lightDir, Vector3 ground, Vector3 objectPos)
        {

            //평면과 광원사이의 최소거리 
            Vector3 groundUp = ConstV.v3_up;
            Vector3 groundToObject = objectPos - ground;
            float len_groundToObject = Vector3.Dot(groundToObject, groundUp);

            //s = len / sin@
            float sinAngle = Geo.Angle360(-groundUp, lightDir, Vector3.Cross(-groundUp, lightDir));
            sinAngle = 90f - sinAngle;

            float s = len_groundToObject / Mathf.Sin(sinAngle * Mathf.Deg2Rad);

            Vector3 nDir = VOp.Normalize(lightDir);

            return objectPos + s * nDir;
        }



        public Vector3 CalcShadowPos(Vector3 lightDir, Vector3 objDir, Vector3 ground, Vector3 objectPos, out float len_groundToObject)
        {

            if (ground.y > objectPos.y)
                lightDir = objDir;

            //평면과 광원사이의 최소거리 
            Vector3 groundUp = ConstV.v3_up;
            Vector3 groundToObject = objectPos - ground;
            len_groundToObject = Vector3.Dot(groundToObject, groundUp);

            //s = len / sin@
            float sinAngle = Geo.Angle360(-groundUp, lightDir, Vector3.Cross(-groundUp, lightDir));
            sinAngle = 90f - sinAngle;

            //s : 삼각형의 빗면 
            float s = len_groundToObject / Mathf.Sin(sinAngle * Mathf.Deg2Rad);

            Vector3 nDir = VOp.Normalize(lightDir);

            return objectPos + s * nDir;
        }


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



        public void SetModel_OneHand(Geo.Model model_0, Geo.Model model_1)
        {
            Vector3 upDir;
            if (true == model_0.IsLeft())
            {
                upDir = _upDir_circle_A0.position - _pos_circle_A0.position;
                //upDir.Normalize();
                model_0.SetModel(upDir, _pos_circle_A0.position, _motion_oneHand_left_0._radius, _highest_circle_A0.position,
                                 _motion_oneHand_left_0._far_radius, _motion_oneHand_left_0._tornado_angle, _tornado_unlace_A0.localPosition);

                upDir = _upDir_circle_A1.position - _pos_circle_A1.position;
                //upDir.Normalize();
                model_1.SetModel(upDir, _pos_circle_A1.position, _motion_oneHand_left_1._radius, _highest_circle_A1.position,
                                 _motion_oneHand_left_1._far_radius, _motion_oneHand_left_1._tornado_angle, _tornado_unlace_A1.localPosition);
            }
            if (true == model_0.IsRight())
            {
                upDir = _upDir_circle_B0.position - _pos_circle_B0.position;
                //upDir.Normalize();
                model_0.SetModel(upDir, _pos_circle_B0.position, _motion_oneHand_right_0._radius, _highest_circle_B0.position,
                                 _motion_oneHand_right_0._far_radius, _motion_oneHand_right_0._tornado_angle, _tornado_unlace_B0.localPosition);

                upDir = _upDir_circle_B1.position - _pos_circle_B1.position;
                //upDir.Normalize();
                model_1.SetModel(upDir, _pos_circle_B1.position, _motion_oneHand_right_1._radius, _highest_circle_B1.position,
                                 _motion_oneHand_right_1._far_radius, _motion_oneHand_right_1._tornado_angle, _tornado_unlace_B1.localPosition);
            }

        }

        public void SetModel_TwoHand(Geo.Model model, Vector3 upDir)
        {
            if (true == model.IsLeft())
            {
                model.SetModel(upDir, _pos_circle_left.position, _motion_twoHand_left._radius, _highest_circle_left.position,
                               _motion_twoHand_left._far_radius, _motion_twoHand_left._tornado_angle, _tornado_unlace_left.localPosition);
            }
            if (true == model.IsRight())
            {
                model.SetModel(upDir, _pos_circle_right.position, _motion_twoHand_right._radius, _highest_circle_right.position,
                               _motion_twoHand_right._far_radius, _motion_twoHand_right._tornado_angle, _tornado_unlace_right.localPosition);
            }

        }

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

        public void Cut_OneHand()
        {
            if (ePart.OneHand == _part_control)
            {

                //주변원 반지름 갱신
                //_radius_circle_A0 = (_pos_circle_A0.position - _edge_circle_A0.position).magnitude;
                //_radius_circle_A1 = (_pos_circle_A1.position - _edge_circle_A1.position).magnitude;
                //_radius_circle_B0 = (_pos_circle_B0.position - _edge_circle_B0.position).magnitude;
                //_radius_circle_B1 = (_pos_circle_B1.position - _edge_circle_B1.position).magnitude;

                //====================

                Vector3 handle = _HANDLE_left.position;
                //Vector3 upDir = _left_axis_up.position - _left_axis_o.position;

                Vector3 newPos = Vector3.zero;
                float newLength = 0f;
                //------------------------------------------

                _Model_left_0.kind = _motion_oneHand_left_0._eModel; //_eModelKind_Left_0;
                _Model_left_1.kind = _motion_oneHand_left_1._eModel; //_eModelKind_Left_1;
                SetModel_OneHand(_Model_left_0, _Model_left_1);
                CalcHandPos_PlaneArea(_Model_left_0, handle,
                                      _shoulder_left.position, _arm_left_max_length, _arm_left_min_length,
                                      out newPos, out newLength);

                _arm_left_length = newLength;
                _hand_left.position = newPos;


                CalcHandPos_PlaneArea(_Model_left_1, handle,
                                      _shoulder_left.position, 1000, _arm_left_min_length,
                                      out newPos, out newLength);
                _odir_left.position = newPos;

                //------------------------------------------

                handle = _HANDLE_right.position;
                //upDir = _right_axis_up.position - _right_axis_o.position;

                _Model_right_0.kind = _motion_oneHand_right_0._eModel; //_eModelKind_Right_0;
                _Model_right_1.kind = _motion_oneHand_right_1._eModel; //_eModelKind_Right_1;
                SetModel_OneHand(_Model_right_0, _Model_right_1);
                CalcHandPos_PlaneArea(_Model_right_0, handle,
                                      _shoulder_right.position, _arm_right_max_length, _arm_right_min_length,
                                      out newPos, out newLength);

                _arm_right_length = newLength;
                _hand_right.position = newPos;


                CalcHandPos_PlaneArea(_Model_right_1, handle,
                                      _shoulder_right.position, 1000, _arm_right_min_length,
                                      out newPos, out newLength);

                _odir_right.position = newPos;

            }
        }

        //지정손 기준으로 지정길이 만큼의 반대손 위치 구하기 
        //handO : 기준이 되는 손 , handDir : 손과 다른손간의 방향 , twoLength : 손과 다른손의 사이길이 
        //handE : 위치를 구할려는 손 
        public void ApplyHandPos_TwoHandLength(Vector3 handLeft_pos, float handLeft_length, Vector3 handRight_pos, float handRight_length, ePart eHandO, float twoLength)
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
        public void Cut_TwoHand(Vector3 handle, ePart eHandOrigin, Geo.Model.eKind eModelLeft, Geo.Model.eKind eModelRight)
        {
            //Vector3 axis_up = _L2R_axis_up.position - _L2R_axis_o.position;
            Vector3 axis_up = _upDir_circle_left.position - _pos_circle_left.position; //임시로 왼쪽 upDir 사용 

            axis_up.Normalize();

            float new_leftLength = 0f;
            Vector3 new_leftPos;
            float new_rightLength = 0f;
            Vector3 new_rightPos;


            //-----------------------
            //모델원 위치 계산 

            _Model_left_0.kind = eModelLeft;
            this.SetModel_TwoHand(_Model_left_0, axis_up);
            this.CalcHandPos_PlaneArea(_Model_left_0, handle,
                                       _shoulder_left.position, _arm_left_max_length, _arm_left_min_length, out new_leftPos, out new_leftLength);



            _Model_right_0.kind = eModelRight;
            this.SetModel_TwoHand(_Model_right_0, axis_up);
            this.CalcHandPos_PlaneArea(_Model_right_0, handle,
                                       _shoulder_right.position, _arm_right_max_length, _arm_right_min_length, out new_rightPos, out new_rightLength);


            //----------------------------

            ApplyHandPos_TwoHandLength(new_leftPos, new_leftLength, new_rightPos, new_rightLength,
                                       eHandOrigin, _twoHand_length);



        }

        //==================================================

        public void Update_AttachHand()
        {
            if (ePart.OneHand == _part_control)
            {   //한손 칼 붙이기 

                //찌르기 
                if (eStance.Sting == _eStance)
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
                //베기 
                else if (eStance.Cut == _eStance)
                {
                    Vector3 handToTarget = _target[0].position - _hand_left.position;
                    Vector3 obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                    float angleW = Vector3.SignedAngle(Vector3.forward, handToTarget, obj_shaft);
                    _object_left.rotation = Quaternion.AngleAxis(angleW, obj_shaft);

                    handToTarget = _target[1].position - _hand_right.position;
                    obj_shaft = Vector3.Cross(Vector3.forward, handToTarget);
                    angleW = Vector3.SignedAngle(Vector3.forward, handToTarget, obj_shaft);
                    _object_right.rotation = Quaternion.AngleAxis(angleW, obj_shaft);
                }

            }
            if (ePart.TwoHand == _part_control)
            {   //양손 칼 붙이기
                Vector3 hLhR = _hand_right.position - _hand_left.position;
                _object_left.rotation = Quaternion.FromToRotation(Vector3.forward, hLhR);

                //2d칼을 좌/우로 90도 세웠을때 안보이는 문제를 피하기 위해 z축 롤값을 0으로 한다  
                Vector3 temp = _object_left.eulerAngles;
                //temp.z = 0; //<쿼터니언 회전시 무기뒤집어지는 문제 원인> 이 처리를 주석하면 수직베기시 정상처리가 된다 


                //칼의 뒷면 표현
                if (Vector3.Dot(ConstV.v3_up, _hand_left_obj_up.position - _hand_left_obj.position) > 0)
                //if (Vector3.Dot(_body_dir, hLhR) > 0)
                {
                    //앞면
                    //_hand_left_obj_spr.color = Color.white;
                    //_hand_left_obj_mesh.sharedMaterial.color = Color.white;
                    temp.z = 0;
                }
                else
                {
                    //뒷면 
                    //_hand_left_obj_spr.color = Color.gray;
                    //_hand_left_obj_mesh.sharedMaterial.color = Color.gray;
                    temp.z = 180;
                }
                _object_left.eulerAngles = temp;

            }
        }

        public void Update_HandAni()
        {
            //주먹 회전 (어깨에서 손까지)
            float angleY = Vector3.SignedAngle(Vector3.forward, (_hand_left.position - _shoulder_left.position), Vector3.up);
            _hand_left_spr.eulerAngles = new Vector3(90, angleY, 0);
            angleY = Vector3.SignedAngle(Vector3.forward, (_hand_right.position - _shoulder_right.position), Vector3.up);
            _hand_right_spr.eulerAngles = new Vector3(90, angleY, 0);
        }

        public void Update_Shadow()
        {
            if (false == _active_shadowObject) return;

            if (ePart.TwoHand == _part_control)
            {
                //-----------------------------------
                //평면과 광원사이의 최소거리 
                Vector3 hLhR = _hand_right.position - _hand_left.position;
                float len_groundToObj_start, len_groundToObj_end;
                //Vector3 start = this.CalcShaderPos(_light_dir.position,hLhR, _ground.position, _hand_left_obj.position, out len_groundToObj_start);
                Vector3 end = this.CalcShadowPos(_light_dir, hLhR, _groundY, _hand_left_obj_end.position, out len_groundToObj_end);
                Vector3 start = this.CalcShadowPos(_light_dir, _groundY, _hand_left_obj.position);
                //Vector3 end = this.CalcShaderPos(_light_dir.position, _ground.position, _hand_left_obj_end.position);

                Vector3 startToEnd = end - start;
                float len_startToEnd = startToEnd.magnitude;
                float rate = len_startToEnd / 2.4f; //창길이 하드코딩 


                float shader_angle = Geo.AngleSigned_AxisY(ConstV.v3_forward, startToEnd);
                //float shader_angle = Geo.Angle360(ConstV.v3_forward, startToEnd, Vector3.up);
                //DebugWide.LogBlue(shader_angle);
                _hand_left_obj_shader.rotation = Quaternion.AngleAxis(shader_angle, ConstV.v3_up); // 땅up벡터 축으로 회전 
                _hand_left_obj_shader.position = start;

                //높이에 따라 그림자 길이 조절 
                Vector3 scale = _hand_left_obj_shader.localScale;
                scale.z = rate;
                _hand_left_obj_shader.localScale = scale;
                //------
                //그림자 땅표면위에만 있게 하기위해 pitch 회전값 제거  
                Vector3 temp2 = _hand_left_obj_shader.eulerAngles;
                //temp2.x = 90f;
                if (Vector3.Dot(ConstV.v3_up, _hand_left_obj_up.position - _hand_left_obj.position) > 0)
                {
                    temp2.z = 0;
                }
                else
                {
                    temp2.z = 180f;
                }
                _hand_left_obj_shader.eulerAngles = temp2;
                //-----------------------------------  

                //DebugWide.LogBlue(len_startToEnd + "   " + (_hand_left_obj_end.position - _hand_left_obj.position).magnitude);

                //땅을 통과하는 창 자르기 
                SpriteMesh spriteMesh = _hand_left_obj.GetComponent<SpriteMesh>();
                if (_groundY.y > _hand_left_obj_end.position.y)
                {
                    float rate_viewLen = (_hand_left_obj_end.position - end).magnitude / 2.4f; //하드코딩
                    spriteMesh._cuttingRate.y = -rate_viewLen;
                    spriteMesh._update_perform = true;

                }
                else if (0 != spriteMesh._cuttingRate.y)
                {
                    //기존값이 남아있는 경우를 제거한다  
                    spriteMesh._cuttingRate.y = 0;
                    spriteMesh._update_perform = true;
                }
            }
        
        }

    }//end class


}//end namespace

