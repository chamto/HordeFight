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
                

        private bool _init = false;

        void Awake()
        {
            //QualitySettings.vSyncCount = 0; //v싱크 끄기
            //Application.targetFrameRate = 60; //60 프레임 제한

            FrameTime.SetFixedFrame_FPS_30(); //30프레임 제한 설정

        }

        void Start()
        {
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
                ObjectManager.Inst.Draw(_SphereTree_NoneRecursive, _SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }
            if (true == _Draw_SphereTree_Struct)
            {
                ObjectManager.Inst.Draw_Struct(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }


            //DebugWide.DrawQ_All_AfterTime(1);
            DebugWide.DrawQ_All_AfterClear();
        }
    }



}//end namespace



