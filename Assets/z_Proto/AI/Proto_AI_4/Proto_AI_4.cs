using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_4
{
    public enum MessageType
    {
        Msg_ReceiveBall,
        Msg_PassToMe,
        Msg_SupportAttacker,
        Msg_GoHome,
        Msg_Wait
    }

    public class EntityMgr
    {

        public static readonly List<Unit> list = new List<Unit>();
        public static int Add(Unit v)
        {
            list.Add(v);
            return list.Count - 1;

        }
    }

    public class Proto_AI_4 : MonoBehaviour
    {

        public Stage _stage = null;

        public bool _Draw_EntityTile = false;
        public bool _Draw_UpTile = false;
        public bool _Draw_StructTile = false;
        public bool _Draw_BoundaryTile = false;
        public bool _Draw_ArcTile = false;

        public bool _Draw_SphereTree_Struct = false;
        public bool _Draw_SphereTree = false;
        public bool _SphereTree_NoneRecursive = false;
        public bool _SphereTree_Level_0 = false;
        public bool _SphereTree_Level_1 = false;
        public bool _SphereTree_Level_2 = false;
        public bool _SphereTree_Level_3 = false;
        public float _SphereTree_Gravy = 0.5f;


        public Transform _tr_test = null;
        public Transform _tr_line_a = null;
        public Transform _tr_line_b = null;

        private bool _init = false;

        void Awake()
        {
            //QualitySettings.vSyncCount = 0; //v싱크 끄기
            //Application.targetFrameRate = 60; //60 프레임 제한

            FrameTime.SetFixedFrame_FPS_30(); //30프레임 제한 설정

        }

        void Start()
        {
            _tr_test = GameObject.Find("Test").transform;
            _tr_line_a = GameObject.Find("line_a").transform;
            _tr_line_b = GameObject.Find("line_b").transform;


            _stage = new Stage();
            _stage.Init(); 
        }

        private void Update()
        {
            //프레임이 크게 떨어질시 Time.delta 값이 과하게 커지는 경우가 발생 , 이럴경우 벽통과등의 문제등이 생긴다. 
            //deltaTime 값을 작게 유지하여 프로그램 무결성을 유지시킨다. 속도가 느려지고 시간이 안맞는 것은 어쩔 수 없다 
            float deltaTime = FrameTime.DeltaTime();

            _stage.Update(deltaTime);
        }

        public void Debug_FuncTest_Include_Sphere_Rate()
        {
            float rate = Geo.Include_Sphere_Rate(Vector3.zero, 5, _tr_test.position, (_tr_test.position - _tr_line_a.position).magnitude);
            DebugWide.DrawCircle(Vector3.zero, 5, Color.white);
            DebugWide.PrintText(_tr_test.position, Color.red, rate + "");

            Draw_Include_Sphere_Rate(5, 4, Color.yellow);
            Draw_Include_Sphere_Rate(5, 3, Color.blue);
            Draw_Include_Sphere_Rate(5, 2, Color.red);

        }

        public delegate float Dele_Include_Sphere_Rate(Vector3 src_pos, float src_radius, Vector3 in_pos, float in_radius, bool reversal=false);
        public void Draw_Include_Sphere_Rate(float src_radius , float in_radius , Color color)
        {
            Dele_Include_Sphere_Rate FUNC = Geo.Include_Sphere_Rate;

            for (int i = 0; i < 80; i++)
            {
                Vector3 nextPos = new Vector3(0 + i * 0.1f, 0, 0);
                float rate = FUNC(Vector3.zero, src_radius, nextPos, in_radius);

                Vector3 nextPos2 = nextPos;
                nextPos2.z += rate;
                DebugWide.DrawLine(nextPos, nextPos2, color);
            }

            //0~2 접촉 , 0 가운데겹침 , 0~1 완전포함 , 1.5 중점걸림 , 2 외곽접함
            float rt0 = FUNC(Vector3.zero, src_radius, new Vector3(0, 0, 0), in_radius); //0 가운데겹침
            float rt1 = FUNC(Vector3.zero, src_radius, new Vector3(src_radius - in_radius, 0, 0), in_radius); //1 완전포함
            float rt2 = FUNC(Vector3.zero, src_radius, new Vector3(src_radius, 0, 0), in_radius); //1.5 중점걸림
            float rt3 = FUNC(Vector3.zero, src_radius, new Vector3(src_radius + in_radius, 0, 0), in_radius); //2 외곽접함
            //DebugWide.LogBlue(src_radius+" , " +in_radius + " -- " + rt0 + "  " + rt1 + "  " + rt2 + "  " + rt3);

        }

        private void OnDrawGizmos()
        {
            if (null == _stage) return;
            if (false == _stage._init) return;

            _stage.OnDrawGizmos();

            if (true == _Draw_EntityTile)
                GridManager.Inst.Draw_EntityTile();

            if (true == _Draw_UpTile)
                GridManager.Inst.Draw_UpTile();

            if (true == _Draw_StructTile)
                GridManager.Inst.Draw_StructTile();

            if (true == _Draw_BoundaryTile)
                GridManager.Inst.Draw_BoundaryTile();

            if (true == _Draw_ArcTile)
                GridManager.Inst.Draw_ArcTile();

            GridManager.Inst.Draw_StructTile_ArcInfo(_stage._tr_test.position);


            if (true == _Draw_SphereTree)
            {
                ObjectManager.Inst.Draw(_SphereTree_NoneRecursive, _SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3,
                    _tr_test.position, _tr_line_a.position, _tr_line_b.position);
            }
            if (true == _Draw_SphereTree_Struct)
            {
                ObjectManager.Inst.Draw_Struct(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }

            //Debug_FuncTest_Include_Sphere_Rate(); //chamto test

            //DebugWide.DrawQ_All_AfterTime(1);
            DebugWide.DrawQ_All_AfterClear();
        }
    }



}//end namespace



