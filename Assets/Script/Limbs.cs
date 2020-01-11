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
        //public Transform _light_dir = null; //방향성 빛
        //public Transform _ground = null; //땅표면 

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
        private MeshRenderer _hand_left_spear = null;

        private Transform _hand_right_obj = null;
        private Transform _hand_right_obj_up = null;
        private Transform _hand_right_obj_end = null;
        private Transform _hand_right_obj_shader = null;
        private SpriteRenderer _hand_right_obj_spr = null;
        private MeshRenderer _hand_right_spear = null;

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
            Limbs limbs = parent.gameObject.AddComponent<Limbs>();

            GameObject limbPrefab = Limbs.CreatePrefab("limbs_twoHand", parent, "limbs_twoHand");

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
            _tbody_dir = SingleO.hierarchy.GetTransform(waist, "body_dir");


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
            _hand_left_spear = _hand_left_obj.GetComponent<MeshRenderer>();
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

        // Update is called once per frame
        //void Update () 
        //   {
        //}
    }
}

