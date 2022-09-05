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
        public float _includeRate = Geo.INCLUDE_MAX;
        //====================================================
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


        public Transform _tr0_test = null;
        public Transform _tr0_line_a = null;
        public Transform _tr0_line_b = null;

        public Transform _tr1_test = null;
        public Transform _tr1_line_a = null;
        public Transform _tr1_line_b = null;

        private bool _init = false;

        public Geo.Arc _arc = new Geo.Arc();

        void Awake()
        {
            //QualitySettings.vSyncCount = 0; //v싱크 끄기
            //Application.targetFrameRate = 60; //60 프레임 제한

            FrameTime.SetFixedFrame_FPS_30(); //30프레임 제한 설정

        }

        void Start()
        {
            _tr0_test = GameObject.Find("Test_0").transform;
            _tr0_line_a = GameObject.Find("line_0_a").transform;
            _tr0_line_b = GameObject.Find("line_0_b").transform;

            _tr1_test = GameObject.Find("Test_1").transform;
            _tr1_line_a = GameObject.Find("line_1_a").transform;
            _tr1_line_b = GameObject.Find("line_1_b").transform;

            _stage = new Stage();
            _stage.Init();

            _arc.Init(Vector3.zero, 90 , Vector3.forward);
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
            Vector3 pos = ConstV.v3_zero;
            Vector3 tr0_pos = _tr0_test.position;
            float rate = Geo.Sphere.Rate_DistanceZero(ref pos, 5, ref tr0_pos, (_tr0_test.position - _tr0_line_a.position).magnitude);
            DebugWide.DrawCircle(Vector3.zero, 5, Color.white);
            DebugWide.PrintText(_tr0_test.position, Color.red, rate + "");

            Draw_Include_Sphere_Rate(5, 4, Color.yellow);
            Draw_Include_Sphere_Rate(5, 3, Color.blue);
            Draw_Include_Sphere_Rate(5, 2, Color.red);

        }

        public delegate float Dele_Include_Sphere_Rate(ref Vector3 src_pos, float src_radius, ref Vector3 in_pos, float in_radius, bool reversal = false);
        public void Draw_Include_Sphere_Rate(float src_radius, float in_radius, Color color)
        {
            Dele_Include_Sphere_Rate FUNC = Geo.Sphere.Rate_DistanceZero;
            Vector3 zero = ConstV.v3_zero;
            for (int i = 0; i < 80; i++)
            {
                Vector3 nextPos = new Vector3(0 + i * 0.1f, 0, 0);
                float rate = FUNC(ref zero, src_radius, ref nextPos, in_radius);

                Vector3 nextPos2 = nextPos;
                nextPos2.z += rate;
                DebugWide.DrawLine(nextPos, nextPos2, color);
            }

            Vector3 cen = new Vector3(0, 0, 0);
            Vector3 min = new Vector3(src_radius - in_radius, 0, 0);
            Vector3 mid = new Vector3(src_radius, 0, 0);
            Vector3 max = new Vector3(src_radius + in_radius, 0, 0);

            //0~2 접촉 , 0 가운데겹침 , 0~1 완전포함 , 1.5 중점걸림 , 2 외곽접함
            float rt0 = FUNC(ref zero, src_radius, ref cen, in_radius); //0 가운데겹침
            float rt1 = FUNC(ref zero, src_radius, ref min, in_radius); //1 완전포함
            float rt2 = FUNC(ref zero, src_radius, ref mid, in_radius); //1.5 중점걸림
            float rt3 = FUNC(ref zero, src_radius, ref max, in_radius); //2 외곽접함
            //DebugWide.LogBlue(src_radius+" , " +in_radius + " -- " + rt0 + "  " + rt1 + "  " + rt2 + "  " + rt3);

        }


        public void Debug_FuncTest_Arc()
        {
            const int COUNT = 15;
            float rad0_include = (_tr0_test.position - _tr0_line_b.position).magnitude;
            float rad0_notInclude = (_tr0_test.position - _tr0_line_a.position).magnitude;
            float rad1 = (_tr1_test.position - _tr1_line_a.position).magnitude;
            Vector3 ndir0 = (_tr0_line_b.position - _tr0_test.position).normalized;
            Vector3 pos_tr0_include = _tr0_test.position;
            Vector3 pos_tr1_target = _tr1_test.position;

            Draw_Ruler(_tr0_test.position + ndir0 * rad0_include, ndir0, rad1, COUNT);
            Draw_Sphere(_tr1_test.position, _tr1_line_a.position, _tr1_line_b.position,"", Color.gray);

            Draw_Arc_SetAngle(_tr0_test.position, _tr0_line_a.position, _tr0_line_b.position);

            //------------------
            //_arc.Init(_tr0_test.position, angle, rad0_near, rad0_far, ndir0);

            //------------------
            //string temp = "" , temp2= "";
            //for(int i=0;i< COUNT; i++)
            //{
            //    Vector3 next = _tr1_test.position + Vector3.back * rad1 * i;
            //    Vector3 next2 = _tr1_test.position + Vector3.right * rad1 * i;
            //    float r = _arc.Include_Rate_Arc_Sphere(next, rad1);
            //    temp += "  " + r;
            //    r = Geo.Include_Rate_SphereZero(_tr0_test.position, rad0_far, next2, rad1, false);
            //    temp2 += "  " + r;
            //}
            //DebugWide.LogBlue("arc : "+temp);
            //DebugWide.LogBlue("sph : "+ temp2);
            //------------------

            Geo.Sphere sph_target = new Geo.Sphere(_tr1_test.position, rad1, _includeRate);
            Geo.Sphere sph_include = new Geo.Sphere(_tr0_test.position, rad0_include, _includeRate);
            Geo.Sphere sph_notInclude = new Geo.Sphere(_tr0_test.position, rad0_notInclude, _includeRate);

            float rate = 0;
            bool isIn = false;

            //rate = Geo.Area.Rate_Sphere(ref sph_target, ref sph_include, ref sph_notInclude, ref _arc);
            //isIn = Geo.Area.Include_Sphere(ref sph_target, ref sph_include, ref sph_notInclude, ref _arc);
            //rate = Geo.Area.Rate_Sphere(ref sph_target, ref sph_include, ref sph_notInclude);
            //isIn = Geo.Area.Include_Sphere(ref sph_target, ref sph_include, ref sph_notInclude);

            //float rate = _arc.Rate_Sphere(ref sph_target);
            //bool isIn = _arc.Include_Deg360(ref sph_target);
            //bool isIn = _arc.Include_Deg180(ref sph_target);

            //float rate = sph_include.Rate_DistanceZero(ref sph_target);
            //bool isIn = sph_include.Include_SqrDistance(ref sph_target);

            rate = Geo.Sphere.Rate_DistanceZero(ref pos_tr0_include, rad0_include, ref pos_tr1_target, rad1);
            isIn = Geo.Sphere.Include_SqrDistance(ref pos_tr0_include, rad0_include, ref pos_tr1_target, rad1, _includeRate);


            if (isIn) Draw_Sphere(_tr1_test.position, _tr1_line_a.position, _tr1_line_b.position,rate.ToString("F3"), Color.yellow);
            DebugWide.LogBlue(rate + "  " + isIn);
        }

        public void Draw_Ruler(Vector3 ori, Vector3 dir, float interval, uint count)
        {
            const int MAX_COUNT = 20;

            dir = dir.normalized;
            Vector3 next = ori;
            for(int i=0;i<count;i++)
            {
                if (i >= MAX_COUNT) break;

                DebugWide.DrawLine(next, next + dir * interval, Color.gray);
                DebugWide.PrintText(next, Color.white, i + "");
                next = next + dir * interval;
            }

        }

        public void Draw_Arc_SetAngle(Vector3 ori, Vector3 pos_near, Vector3 pos_far)
        {
            Vector3 dir_notInclude = pos_near - ori;
            Vector3 dir_include = pos_far - ori;
            float angle = Geo.Angle_AxisY(dir_notInclude, dir_include); //180도 까지 구할 수 있음 
            angle *= 2f;

            //DebugWide.LogGreen(angle);
            _arc.origin = ori;
            _arc.includeRate = _includeRate;
            _arc.SetDir(dir_include.normalized);
            _arc.SetAngle(angle);
            Geo.Area.Draw(Color.blue, dir_include.magnitude, dir_notInclude.magnitude, ref _arc);
        }

        public void Draw_Sphere(Vector3 ori, Vector3 pos_near, Vector3 pos_far, string text, Color color)
        {
            DebugWide.DrawLine(ori, pos_near, Color.white);
            DebugWide.DrawLine(ori, pos_far, Color.white);
            DebugWide.DrawCircle(ori, (ori - pos_near).magnitude, color);
            DebugWide.DrawCircle(ori, (ori - pos_far).magnitude, color);
            DebugWide.PrintText(ori, Color.white, text);
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

            GridManager.Inst.Draw_StructTile_ArcInfo(_stage._tr0_test.position);


            if (true == _Draw_SphereTree)
            {
                ObjectManager.Inst.Draw(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);

                ObjectManager.Inst.Draw_Sight(_SphereTree_NoneRecursive, _tr0_test.position, _tr0_line_a.position, _tr0_line_b.position,
                    _tr1_test.position, _tr1_line_a.position, _tr1_line_b.position, _includeRate);
            }
            if (true == _Draw_SphereTree_Struct)
            {
                ObjectManager.Inst.Draw_Struct(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }

            //Draw_Sphere(_tr0_test.position, _tr0_line_a.position, _tr0_line_b.position);
            //Draw_Sphere(_tr1_test.position, _tr1_line_a.position, _tr1_line_b.position);

            //Debug_FuncTest_Include_Sphere_Rate(); //chamto test

            //Debug_FuncTest_Arc();

            //DebugWide.DrawQ_All_AfterTime(1);
            DebugWide.DrawQ_All_AfterClear();
        }
    }



}//end namespace



