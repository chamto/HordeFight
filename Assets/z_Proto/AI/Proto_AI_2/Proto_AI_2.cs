﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_2
{

    public class EntityMgr
    {

        public static readonly List<Vehicle> list = new List<Vehicle>();
        public static int Add(Vehicle v)
        {
            list.Add(v);
            return list.Count - 1;

        }
    }

    public class SphereTree_0
    {
        public static readonly SphereTree_0 Inst = new SphereTree_0();
        public ST_Test_003.PT_SphereTree _sphereTree = new ST_Test_003.PT_SphereTree(500, 16, 10, 0.5f);

        private SphereTree_0()
        { }

        public void AddSphereTree(BaseEntity entity)
        {
            ST_Test_003.PT_SphereModel model = _sphereTree.AddSphere(entity._pos, entity._radius, ST_Test_003.PT_SphereModel.Flag.TREE_LEVEL_2);
            _sphereTree.AddIntegrateQ(model);

            entity._sphereModel_0 = model;
        }

        public void Update(float deltaTime)
        {
            int a, b;
            _sphereTree.ResetFlag();
            _sphereTree.Process(out a, out b);
        }

        public void Draw(bool level_0, bool level_1, bool level_2, bool level_3)
        {

            if (level_1)
                _sphereTree.Render_Debug(2, true);
            if (level_0)
                _sphereTree.Render_Debug(1, true);

        }
    }

    public class SphereTree_1
    {
        public static readonly SphereTree_1 Inst = new SphereTree_1();
        public ST_Test_004.SphereTree _sphereTree = new ST_Test_004.SphereTree(500, new float[] { 16, 10}, 0.5f);

        private SphereTree_1()
        { }

        public void AddSphereTree(BaseEntity entity)
        {
            ST_Test_004.SphereModel model = _sphereTree.AddSphere(entity._pos, entity._radius, ST_Test_004.SphereModel.Flag.CREATE_LEVEL_LAST);
            _sphereTree.AddIntegrateQ(model);
            //model.SetLink_UserData<BaseEntity>(entity);

            entity._sphereModel_1 = model;
        }

        public void Update(float deltaTime)
        {
            _sphereTree.Process();
        }

        public void Draw(bool level_0, bool level_1, bool level_2, bool level_3)
        {
            if (level_3)
                _sphereTree.Render_Debug(3, true);
            if (level_2)
                _sphereTree.Render_Debug(2, true);
            if (level_1)
                _sphereTree.Render_Debug(1, true);
            if (level_0)
                _sphereTree.Render_Debug(0, true);

        }
    }

    public class SphereTree_2
    {
        public static readonly SphereTree_2 Inst = new SphereTree_2();
        public ST_Test_005.SphereTree _sphereTree = new ST_Test_005.SphereTree(500, new float[] { 16, 10 }, 0.5f);

        private SphereTree_2()
        { }

        public void AddSphereTree(BaseEntity entity)
        {
            ST_Test_005.SphereModel model = _sphereTree.AddSphere(entity._pos, entity._radius, ST_Test_005.SphereModel.Flag.CREATE_LEVEL_LAST);
            _sphereTree.AddIntegrateQ(model);
            //model.SetLink_UserData<BaseEntity>(entity);

            entity._sphereModel_2 = model;
        }

        public void Update(float deltaTime)
        {
            _sphereTree.Process();
        }

        public void Draw(bool level_0, bool level_1, bool level_2, bool level_3)
        {
            if (level_3)
                _sphereTree.Render_Debug(3, true);
            if (level_2)
                _sphereTree.Render_Debug(2, true);
            if (level_1)
                _sphereTree.Render_Debug(1, true);
            if (level_0)
                _sphereTree.Render_Debug(0, true);
        
        }
            
    }

    public class SphereTree_3
    {
        public static readonly SphereTree_3 Inst = new SphereTree_3();
        //public ST_Test_006.SphereTree _sphereTree = new ST_Test_006.SphereTree(500, new float[] { 16, 10 }, 0.5f);
        public ST_Test_006.SphereTree _sphereTree = new ST_Test_006.SphereTree(500, new float[] { 32, 16 , 8 , 3}, 0.5f);
        public ST_Test_006.SphereTree _sphereTree_struct = new ST_Test_006.SphereTree(2000, new float[] { 32, 16, 8 , 2 }, 0.5f);

        private SphereTree_3()
        { }

        public void Init()
        {
            foreach (KeyValuePair<Vector3Int, CellSpace> t in GridManager.Inst._structTileList)
            {
                //if (true == t.Value._isUpTile)
                {
                    ST_Test_006.SphereModel model = _sphereTree_struct.AddSphere(t.Value._pos3d_center, 0.6f, ST_Test_006.SphereTree.CREATE_LEVEL_LAST);
                    _sphereTree_struct.AddIntegrateQ(model);
                }
            }

            _sphereTree_struct.Process();
        }

        public void AddSphereTree(BaseEntity entity)
        {
            ST_Test_006.SphereModel model = _sphereTree.AddSphere(entity._pos, entity._radius, ST_Test_006.SphereTree.CREATE_LEVEL_LAST);
            _sphereTree.AddIntegrateQ(model);
            //model.SetLink_UserData<BaseEntity>(entity);

            entity._sphereModel_3 = model;
        }

        public void Update(float deltaTime)
        {
            _sphereTree.Process();

        }

        public void Draw(bool level_0, bool level_1, bool level_2, bool level_3)
        {
            if (level_3)
            { 
                _sphereTree.Render_Debug(3, true);
            }
            if (level_2)
            { 
                _sphereTree.Render_Debug(2, true);
            }

            if (level_1)
            { 
                _sphereTree.Render_Debug(1, true);
            }

            if (level_0)
            { 
                _sphereTree.Render_Debug(0, true);
            }
        }

        public void Draw_Struct(bool level_0, bool level_1, bool level_2, bool level_3)
        {
            if (level_3)
            {
                _sphereTree_struct.Render_Debug(3, true);
            }
            if (level_2)
            {
                _sphereTree_struct.Render_Debug(2, true);
            }

            if (level_1)
            {
                _sphereTree_struct.Render_Debug(1, true);
            }

            if (level_0)
            {
                _sphereTree_struct.Render_Debug(0, true);
            }
        }

    }

    public class ObjectManager
    {
        public static readonly ObjectManager Inst = new ObjectManager();

        //public SphereTree _sphereTree_entity = new SphereTree(500, new float[] { 16, 10, 5, 3 }, 0.5f);

        private ObjectManager()
        { }



        //public BaseEntity RangeTest(BaseEntity src, Vector3 pos, float meter_minRadius, float meter_maxRadius)
        //{
        //    Param_RangeTest param = new Param_RangeTest(src, pos, meter_minRadius, meter_maxRadius);
        //    _sphereTree_entity.RangeTest_MinDisReturn(ref param); 


        //    if (null != param.find)
        //    {

        //        return param.find.GetLink_UserData() as BaseEntity;
        //    }

        //    return null;
        //}

        //public void AddSphereTree(BaseEntity entity)
        //{
        //    SphereModel model = _sphereTree_entity.AddSphere(entity._pos, entity._radius, SphereModel.Flag.CREATE_LEVEL_LAST);
        //    _sphereTree_entity.AddIntegrateQ(model);
        //    model.SetLink_UserData<BaseEntity>(entity);

        //    entity._sphereModel = model;
        //}

        //public void Update(float deltaTime)
        //{
        //    _sphereTree_entity.Process(); 
        //}
    }


    public class Proto_AI_2 : MonoBehaviour
    {
        public Transform _tr_target = null;
        public GridManager _gridMgr = null;
        public SweepPrune _sweepPrune = new SweepPrune();

        public Transform _tr_test = null;
        public Transform _tr_line_a = null;
        public Transform _tr_line_b = null;

        public Transform _tr_test2_s = null;
        public Transform _tr_test2_e = null;

        public float _formation_speed = 10;
        public float _radius = 0.5f;
        public float _mass = 1f;
        public float _weight = 20;
        public float _maxForce = 40f;
        public float _maxSpeed = 10f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;

        public bool _isNonpenetration = true;

        public float _minRange = 0;
        public float _maxRange = 0.5f;

        public FormationPoint _formationPoint = new FormationPoint();

        public bool _Draw_EntityTile = false;
        public bool _Draw_UpTile = false;
        public bool _Draw_StructTile = false;
        public bool _Draw_BoundaryTile = false;
        public bool _Draw_ArcTile = false;

        public bool _Draw_SphereTree_Struct = false;
        public bool _Draw_SphereTree_0 = false;
        public bool _Draw_SphereTree_1 = false;
        public bool _Draw_SphereTree_2 = false;
        public bool _Draw_SphereTree_3 = false;
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
            Init(); 
        }

        void Init()
        {
            _init = true;

            _tr_target = GameObject.Find("tr_target").transform;

            _tr_test = GameObject.Find("Test").transform;
            _tr_line_a = GameObject.Find("line_a").transform;
            _tr_line_b = GameObject.Find("line_b").transform;
            _tr_test2_s = GameObject.Find("Test2_s").transform;
            _tr_test2_e = GameObject.Find("Test2_e").transform;

            _gridMgr = GridManager.Inst;

            SphereTree_3.Inst.Init();

            _formationPoint._end = _tr_target.position;
            _formationPoint._pos = _tr_target.position;

            //------

            EntityMgr.list.Clear();

            //0
            Vehicle v = new Vehicle();
            int id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._mode = SteeringBehavior.eType.arrive;
            v._leader = _formationPoint;
            v._offset = new Vector3(0, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;
            //v._maxSpeed = 14;

            //1
            v = new Vehicle();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //2
            v = new Vehicle();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(-1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //-------------------

            //3
            v = new Vehicle();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(1f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            ////4
            v = new Vehicle();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(2f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //5
            v = new Vehicle();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(3f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;
            v._mode = SteeringBehavior.eType.arrive;


            for (int i=0;i<30;i++)
            {
                v = new Vehicle();
                id = EntityMgr.Add(v);
                v.Init(id, 0.5f, new Vector3(17, 0, 12));
                v._mode = SteeringBehavior.eType.arrive;
                v._target = new Vector3(17, 0, 12);

            }

            //==============================

            //충돌검출기 초기화 
            List<SweepPrune.CollisionObject> collObj = new List<SweepPrune.CollisionObject>();
            for(int i=0;i<EntityMgr.list.Count;i++)
            {
                collObj.Add(EntityMgr.list[i]._collision); 
            }
            _sweepPrune.Initialize(collObj);


            //----------

        }


        int ID = 0;
        void Update()
        {


            //Vector3 line_a = _tr_line_a.position - _tr_test.position;
            //Vector3 line_b = _tr_line_b.position - _tr_test.position;
            //float max_angle = Geo.AngleSigned_AxisY(line_a, line_b);
            //max_angle = Geo.AngleSigned(line_a, line_b, Vector3.up);
            //DebugWide.LogBlue(max_angle);


            //프레임이 크게 떨어질시 Time.delta 값이 과하게 커지는 경우가 발생 , 이럴경우 벽통과등의 문제등이 생긴다. 
            //deltaTime 값을 작게 유지하여 프로그램 무결성을 유지시킨다. 속도가 느려지고 시간이 안맞는 것은 어쩔 수 없다 
            float deltaTime = FrameTime.DeltaTime();

            //========================
            Vehicle vh = EntityMgr.list[ID];

            _formationPoint._speed = _formation_speed;
            _formationPoint._end = _tr_target.position;
            _formationPoint.Update(deltaTime);
            KeyInput();
            //vh._target = _formationPoint._pos; //0번째 객체에만 특별히 부여 , 도착시험하기 위함 

            vh = EntityMgr.list[5];
            vh._target = _tr_test.position;
            //float kmPerHour = (3600f / 1000f) * _maxSpeed;
            //DebugWide.LogBlue(ID + "  시간당 속도: " + kmPerHour + "  초당 속도: " + _maxSpeed + "  초당 거리: " + _maxSpeed * deltaTime);
            //운반기의 반지름이 0.5 이며 타일한개의 길이가 1인 경우 : _maxSpeed * deltaTime 의 값이 1.5 를 넘으면 지형을 통과하게 된다 
            //운반기의 반지름이 0과 가까운 아주 작은 값일 경우 : _maxSpeed * deltaTime 의 값이 1 을 넘으면 지형을 통과하게 된다 
            //현재의 타일기반 지형충돌 알고리즘으로는 _maxSpeed 가 30 (시속108) 까지만 충돌처리가 가능하다 

            //==============================================

            //ObjectManager.Inst._sphereTree_entity._gravy_supersphere = _SphereTree_Gravy;
            //ObjectManager.Inst._sphereTree_entity._maxRadius_supersphere = _SphereTree_maxRadius;
            SphereTree_0.Inst._sphereTree._gravy_supersphere = _SphereTree_Gravy;
            SphereTree_1.Inst._sphereTree._gravy_supersphere = _SphereTree_Gravy;
            SphereTree_2.Inst._sphereTree._gravy_supersphere = _SphereTree_Gravy;
            SphereTree_3.Inst._sphereTree._gravy_supersphere = _SphereTree_Gravy;

            foreach (Vehicle v in EntityMgr.list)
            {
                if (6 <= v._id) 
                {
                    // v._withstand = 100; //임시 시험 
                    v._target = _tr_test2_s.position;
                }

                //v._radius = _radius;
                v.SetRadius(_radius);
                v._mass = _mass;
                v._maxSpeed = _maxSpeed;
                v._maxForce = _maxForce;
                v._Friction = _Friction;
                v._anglePerSecond = _anglePerSecond;
                v._weight = _weight;
                v._isNonpenetration = _isNonpenetration;
                v.Update(deltaTime);
                //v._stop = false;

            }

            //==============================================
            //sweepPrune 삽입정렬 및 충돌처리
            //==============================================
            for (int i = 0; i < EntityMgr.list.Count; i++)
            {
                _sweepPrune.SetEndPoint(i, EntityMgr.list[i]._collision); //경계상자 위치 갱신
            }

            _sweepPrune.UpdateXZ();

            foreach (SweepPrune.UnOrderedEdgeKey key in _sweepPrune.GetOverlap()) 
            {
                Vehicle src = EntityMgr.list[key._V0];
                Vehicle dst = EntityMgr.list[key._V1];

                if (src == dst) continue;

                if (_isNonpenetration)
                    CollisionPush(src, dst);
            }

            //==============================================

            foreach (Vehicle v in EntityMgr.list)
            {
                //==========================================
                //동굴벽과 캐릭터 충돌처리 

                //객체의 반지름이 <0.1 ~ 0.99> 범위에 있어야 한다.
                //float maxR = Mathf.Clamp(v._radius, 0.1, 0.99); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 

                //DebugWide.AddDrawQ_Line(v._pos, _formationPoint._pos, Color.magenta);
                //DebugWide.AddDrawQ_Line(v._pos, v._oldPos, Color.red);


                //v.SetPos(_gridMgr.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius, out v._stop));

                
                //DebugWide.AddDrawQ_Line(v._pos, _formationPoint._pos, Color.gray);

                //==========================================
            }

            //ObjectManager.Inst.Update(deltaTime);
            SphereTree_0.Inst.Update(deltaTime);
            SphereTree_1.Inst.Update(deltaTime);
            SphereTree_2.Inst.Update(deltaTime);
            SphereTree_3.Inst.Update(deltaTime);
        }

        public void KeyInput()
        {
            const float MOVE_LENGTH = 1f;
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 n = _formationPoint._end - _formationPoint._pos;
                n = VOp.Normalize(n);
                _formationPoint._end += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._end;
            }
            if (Input.GetKey(KeyCode.S))
            {
                Vector3 n = _formationPoint._end - _formationPoint._pos;
                n = -VOp.Normalize(n);
                _formationPoint._end += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._end;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Vector3 n = _formationPoint._end - _formationPoint._pos;
                n = -VOp.PerpN(n, Vector3.up);
                _formationPoint._end += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._end;
            }
            if (Input.GetKey(KeyCode.D))
            {
                Vector3 n = _formationPoint._end - _formationPoint._pos;
                n = VOp.PerpN(n, Vector3.up);
                _formationPoint._end += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._end;
            }

        }

        public void CollisionPush(Vehicle src, Vehicle dst)
        {
            if (null == src || null == dst) return;


            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            Vector3 dir_dstTOsrc = VOp.Minus(src._pos, dst._pos);
            Vector3 n = ConstV.v3_zero;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;
            //float r_sum = (src._collision._radius + dst._collision._radius);
            float r_sum = (src._radius + dst._radius);
            float sqr_r_sum = r_sum * r_sum;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dstTOsrc < sqr_r_sum)
            {

                //==========================================
                float rate_src, rate_dst;
                float f_sum = src._withstand + dst._withstand;
                if (Misc.IsZero(f_sum)) rate_src = rate_dst = 0.5f;
                else
                {
                    rate_src = 1f - (src._withstand / f_sum);
                    rate_dst = 1f - rate_src;
                }


                //n = Misc.GetDir8_Normal3D(dir_dstTOsrc); //8방향으로만 밀리게 한다 
                n = VOp.Normalize(dir_dstTOsrc);

                float len_dstTOsrc = (float)Math.Sqrt(sqr_dstTOsrc);
                float len_bitween = (r_sum - len_dstTOsrc);
                float len_bt_src = len_bitween * rate_src;
                float len_bt_dst = len_bitween * rate_dst;

                //2.완전겹친상태 
                if (float.Epsilon >= len_dstTOsrc)
                {
                    n = Misc.GetDir8_Random_AxisY();
                    len_dstTOsrc = 1f;
                    len_bt_src = r_sum * 0.5f;
                    len_bt_dst = r_sum * 0.5f;
                }

                src.SetPos(src._pos + n * len_bt_src);
                dst.SetPos(dst._pos - n * len_bt_dst);

                //src._stop = true;
                //dst._stop = true;
            }
        }


        private void OnDrawGizmos()
        {
            if (false == _init) return;
            if (null == _tr_target) return;

            //Vehicle vh = EntityMgr.list[ID];

            DebugWide.DrawLine(_tr_test.position, _tr_line_a.position, Color.white);
            DebugWide.DrawLine(_tr_test.position, _tr_line_b.position, Color.white);

            //DebugWide.DrawLine(vh._oldPos, vh._pos, Color.cyan);

            //_gridMgr.Draw_Find_FirstStructTile(_tr_test.position, _tr_line_a.position , 10);

            //_gridMgr.Find_FirstStructTile(vh._oldPos, vh._oldPos + (vh._pos - vh._oldPos) * 100 , 5);

            //_gridMgr.Draw_line_equation3(_tr_test.position.x, _tr_test.position.z, _tr_line_a.position.x, _tr_line_a.position.z);


            //CellSpace cell = _gridMgr.GetStructTile(_tr_test2_s.position);
            //if(null != cell)
            //{
            //    cell.line.Draw(Color.green);
            //    _gridMgr.LineInterPos(_tr_test.position, _tr_line_a.position, cell.line.origin , cell.line.last);
            //}

            //Vector3 ot0, ot1;
            //_gridMgr.CalcArcFullyPos( _tr_test.position, _tr_line_a.position - _tr_test.position, vh._radius , out ot0, out ot1);

            //Vector3 test = _tr_line_a.position - _tr_test.position;
            //Vector3 dir4n = Misc.GetDir4_Normal3D_AxisY(test);
            //DebugWide.DrawLine(_tr_test.position, _tr_test.position + dir4n, Color.red);
            //DebugWide.DrawCircle(_tr_test.position + dir4n, 0.1f, Color.red);

            //DebugWide.DrawCircle(_tr_target.position, 0.1f, Color.white);
            //DebugWide.DrawLine(EntityMgr.list[0]._pos, _tr_target.position, Color.white);

            //BaseEntity findEnty_1 = ObjectManager.Inst.RangeTest(null, _tr_test.position, _minRange, _maxRange);
            //DebugWide.AddDrawQ_Circle(_tr_test.position, _minRange, Color.blue);
            //DebugWide.AddDrawQ_Circle(_tr_test.position, _maxRange, Color.blue);
            //if (null != findEnty_1)
            //{
            //    DebugWide.AddDrawQ_Circle(findEnty_1._pos, 0.1f, Color.red);
            //}

            if(GridManager.Inst.IsVisibleTile(_tr_test.position, _tr_line_a.position, 10))
            {
                DebugWide.AddDrawQ_Line(_tr_test.position, _tr_line_a.position, Color.red); 
            }


            _formationPoint.Draw(Color.white);

            Color color = Color.black;
            foreach (Vehicle v in EntityMgr.list)
            {
                color = Color.black;
                if (0 == v._id)
                    color = Color.red;

                v.Draw(color);
            }

            if (true == _Draw_EntityTile)
                _gridMgr.Draw_EntityTile();

            if (true == _Draw_UpTile)
                _gridMgr.Draw_UpTile();

            if (true == _Draw_StructTile)
                _gridMgr.Draw_StructTile();

            if (true == _Draw_BoundaryTile)
                _gridMgr.Draw_BoundaryTile();

            if (true == _Draw_ArcTile)
                _gridMgr.Draw_ArcTile();

            _gridMgr.Draw_StructTile_ArcInfo(_tr_test.position);

            if(true == _Draw_SphereTree_0)
            {
                SphereTree_0.Inst.Draw(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }
            if (true == _Draw_SphereTree_1)
            {
                SphereTree_1.Inst.Draw(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }
            if (true == _Draw_SphereTree_2)
            {
                SphereTree_2.Inst.Draw(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
            }
            if (true == _Draw_SphereTree_3)
            {
                SphereTree_3.Inst.Draw(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);

                if(true == _Draw_SphereTree_Struct)
                {
                    SphereTree_3.Inst.Draw_Struct(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
                }
            }


            //DebugWide.DrawQ_All_AfterTime(1);
            DebugWide.DrawQ_All_AfterClear();
        }
    }

    public class BaseEntity //: SphereModel.IUserData
    {
        public float _radius = 0.5f;
        public Vector3 _oldPos = Vector3.zero;
        public Vector3 _pos = Vector3.zero;
        public Vector3 _velocity = Vector3.zero;
        public float _speed = 10f;
        public Quaternion _rotation = Quaternion.identity;

        public CellSpace _cur_cell = null;
        public BaseEntity _prev_sibling = null;
        public BaseEntity _next_sibling = null;

        //==================================================
        // 충돌 모델 
        //==================================================
        public SweepPrune.CollisionObject _collision = new SweepPrune.CollisionObject();
        //public SphereModel _sphereModel = null; //구트리

        public ST_Test_003.PT_SphereModel _sphereModel_0 = null; //구트리
        public ST_Test_004.SphereModel _sphereModel_1 = null; //구트리
        public ST_Test_005.SphereModel _sphereModel_2 = null; //구트리
        public ST_Test_006.SphereModel _sphereModel_3 = null; //구트리
    }

    public class FormationPoint : BaseEntity
    {
        //public Vector3 _start = Vector3.zero;
        public Vector3 _end = Vector3.zero;


        //private Vector3 _n_dir = Vector3.zero;
        //public void Init(Vector3 start, Vector3 end)
        //{
        //    _start = start;
        //    _end = end;
        //    _pos = _start;

        //    _n_dir = VOp.Normalize(_end - _start);
        //}

        public void Update(float deltaTime)
        {

            //도착시 종료 
            if ((_end - _pos).sqrMagnitude < 1)
            {
                //특정거리 안에서 이동없이 방향전환 
                _rotation = Quaternion.FromToRotation(Vector3.forward, _end - _pos);
                _velocity = Vector3.zero; //초기화를 안해주면 키이동시 경로가 이상해지는 문제가 발생함
                return;
            }

            //DebugWide.LogBlue("sfsdf: " + _pos );
            Vector3 n = VOp.Normalize(_end - _pos);
            _velocity = n * _speed;
            _pos += _velocity * deltaTime;

            _rotation = Quaternion.FromToRotation(Vector3.forward, _velocity);
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(_end, 0.1f, color);
            DebugWide.DrawLine(_pos, _end, color);
        }

    }

    public class Vehicle : BaseEntity
    {
        public int _id = -1;

        //public Vector3 _pos = Vector3.zero;
        //public Vector3 _velocity = new Vector3(0, 0, 0);
        //public float _speed = 0;
        //public Quaternion _rotation = Quaternion.identity;

        public Vector3 _heading = Vector3.forward;


        public float _mass = 1f;
        public float _maxSpeed = 10f;
        public float _maxForce = 40f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;
        public float _weight = 20;
        public float _withstand = 1f; //버티기

        public bool _isNonpenetration = true; //비침투 

        public bool _stop = false;

        //public Vector3 _size =  new Vector3(0.5f, 0, 0.5f);

        public bool _tag = false;

        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vector3 _worldOffsetPos = ConstV.v3_zero;
        public BaseEntity _leader = null;
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();


        public List<Vector3> _feelers = new List<Vector3>();

        public void Reset()
        {
            _velocity = new Vector3(0, 0, 0);
            _heading = Vector3.forward;

            _tag = false;

            _rotation = Quaternion.identity;

            _target = ConstV.v3_zero;
            _offset = ConstV.v3_zero;
            _leader = null;
            _mode = SteeringBehavior.eType.none;
        }

        public void Init(int id , float radius , Vector3 pos)
        {
            _id = id;

            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;
            _radius = radius;

            //==============================================
            _collision._id = _id;
            //_collision._radius = radius;

            //==============================================
            ////구트리 등록 
            //ObjectManager.Inst.AddSphereTree(this);

            SphereTree_0.Inst.AddSphereTree(this);
            SphereTree_1.Inst.AddSphereTree(this);
            SphereTree_2.Inst.AddSphereTree(this);
            SphereTree_3.Inst.AddSphereTree(this);
            //==============================================

            SetPos(pos);
            _oldPos = pos;

            CreateFeelers();
        }

        public void SetPos(Vector3 newPos)
        {
            _pos = newPos;

            //==============================================
            //!!!!! 구트리 위치 갱신 
            //if (null != _sphereModel)
            //{
            //    _sphereModel.NewPos(_pos);
            //}

            _sphereModel_0.SetPos(_pos);
            _sphereModel_1.SetPos(_pos);
            _sphereModel_2.NewPos(_pos);
            _sphereModel_3.NewPos(_pos);
            //==============================================

            //!!!!! 경계상자 위치 갱신
            _collision._bounds_min.x = newPos.x - _radius;
            _collision._bounds_min.z = newPos.z - _radius;
            _collision._bounds_max.x = newPos.x + _radius;
            _collision._bounds_max.z = newPos.z + _radius;
            //==============================================

            GridManager.Inst.AttachCellSpace(_pos, this);

        }

        public void SetRadius(float radius)
        {
            _radius = radius;
            //==============================================
            //!!!!! 구트리 갱신 
            //if (null != _sphereModel)
            //{
            //    _sphereModel.NewPosRadius(_pos, _radius); //반지름도 함께 갱신되도록 한다 
            //}
        }


        public void CreateFeelers()
        {
            const float Feeler_Length = 1;
            Vector3 fpos = Vector3.forward * (_radius + Feeler_Length);
            _feelers.Add(fpos);

            fpos = Quaternion.AngleAxis(45, Vector3.up) * Vector3.forward  * (_radius + Feeler_Length);
            _feelers.Add(fpos);

            fpos = Quaternion.AngleAxis(-45, Vector3.up) * Vector3.forward * (_radius + Feeler_Length);
            _feelers.Add(fpos);
        }

        int __findNum = 0;
        public void Update(float deltaTime)
        {
            _oldPos = _pos;
            _worldOffsetPos = _target;
            if (null != _leader)
            {
                _worldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace 
            }


            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.fast) * _weight;
                //SteeringForce = _steeringBehavior.Seek(_target) * _weight;
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset) * _weight;

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);


            Vector3 acceleration = SteeringForce / _mass;

            _velocity *= _Friction; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 
                            
            _velocity +=  acceleration * deltaTime;

            _velocity = VOp.Truncate(_velocity, _maxSpeed);

            //_pos += _velocity * deltaTime;
            //_pos = WrapAroundXZ(_pos, 100, 100);
            //SetPos(_pos + _velocity * deltaTime);

            //SetPos(_pos + VOp.Normalize(_velocity) * _maxSpeed * deltaTime); //등속도 시험 

            //Vector3 n = Misc.GetDir8_Normal3D(_velocity); //8방향으로만 밀리게 한다 
            //SetPos(_pos + n * _velocity.magnitude * deltaTime);


            if (_velocity.sqrMagnitude > 0.001f)
            {

                //Vector3 perp = Vector3.Cross(_heading, _velocity);
                //float def = 1;
                //if (0 > Vector3.Dot(perp, Vector3.up)) def = -1f;

                //float def = VOp.PerpDot_ZX(_heading, _velocity); //행렬식값 , sin@ 값 
                //def = Mathf.Clamp(def, -1, 1);

                float def = VOp.Sign_ZX(_heading , _velocity);

                float max_angle = Geo.AngleSigned_AxisY(_heading, _velocity);

                float angle = _anglePerSecond * def * deltaTime;

                //DebugWide.LogRed(angle + "   " + max_angle);

                //최대회전량을 벗어나는 양이 계산되는 것을 막는다 
                if (Math.Abs(angle) > Math.Abs(max_angle))
                {
                    angle = max_angle;
                
                }


                _heading = Quaternion.AngleAxis(angle, ConstV.v3_up) * _heading;
                _heading = VOp.Normalize(_heading);
                _speed = _velocity.magnitude;
                _rotation = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

                //if(0 == _id)
                //{
                //    //DebugWide.AddDrawQ_Line(_pos, _pos + _heading, Color.grey);
                //    DebugWide.AddDrawQ_Line(_pos, _pos + VOp.Normalize(acceleration), Color.grey);
                //    DebugWide.AddDrawQ_Line(_pos, _pos + VOp.Normalize(_velocity), Color.green);
                //    DebugWide.LogBlue("  accel: " + VOp.ToString(acceleration) + "  " + acceleration.magnitude + "   vel: " + VOp.ToString(_velocity) + " " + _velocity.magnitude + " " + _velocity.sqrMagnitude);
                //}

                //SetPos(_pos + _heading * _speed * deltaTime);

                //-----------

                //최대각차이가 지정값 보다 작을 때만 이동시킨다 
                //if (Math.Abs(max_angle) < 1f)
                //{
                //    SetPos(_pos + _velocity * deltaTime);
                //}

                //{
                //    if (_stop)
                //        _velocity = VOp.Truncate(_velocity, 1);

                //    Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
                //    Vector3 ToOffset = WorldOffsetPos - _pos;
                //    Vector3 pos_future = _pos + _velocity * deltaTime;
                //    Vector3 ToFuture = pos_future - WorldOffsetPos;

                //    //fixme : 속도가 빠른 경우 떠는 문제 있음 
                //    //if (ToOffset.sqrMagnitude <= ToFuture.sqrMagnitude )//&& Vector3.Dot(ToOffset, _velocity) >= 0)
                //    if (ToOffset.sqrMagnitude < 0.001f)
                //    {
                //        SetPos(WorldOffsetPos);
                //    }
                //    else
                //        SetPos(pos_future);

                //}


                //-----------
                float curSpeed = _maxSpeed;
                //if(false)
                {
                    //float[] ay_angle = new float[] { 0, 45f, -45f, 90f, -90, 135f, -135, 180f };
                    float[] ay_angle = new float[] { 0, 45f, -45f, 60f, -60, 135f, -135, 180f };
                    Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * _velocity.normalized;
                    float sum_r = _radius + _radius;
                    Vector3 pos_1 = _pos + _velocity.normalized * _radius;
                    Vector3 pos_2 = _pos + findDir * _radius;

                    //CellSpace findCell_1 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + _velocity.normalized * sum_r, 5);
                    //CellSpace findCell_2 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + findDir * sum_r, 5);

                    //if (null == findCell_1 ||
                    //(null != findCell_1 && (findCell_1._head._pos - pos_1).sqrMagnitude > sum_r * sum_r))
                    //{
                    //    curSpeed = _maxSpeed;
                    //}
                    //else if (null == findCell_2 ||
                    //    (null != findCell_2 && (findCell_2._head._pos - pos_2).sqrMagnitude > sum_r * sum_r))
                    //{
                    //    curSpeed = _maxSpeed;
                    //    _velocity = findDir;
                    //    //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                    //}
                    //else
                    //{
                    //    __findNum = Misc.RandInt(1, 4);
                    //    curSpeed = 0;
                    //}

                    //BaseEntity findEnty_1 = ObjectManager.Inst.RangeTest(this, pos_1, 0, _radius);
                    //BaseEntity findEnty_2 = ObjectManager.Inst.RangeTest(this, pos_2, 0, _radius);
                    //if (null == findEnty_1)
                    //{
                    //    curSpeed = _maxSpeed;
                    //}
                    //else if (null == findEnty_2)
                    //{
                    //    curSpeed = _maxSpeed;
                    //    _velocity = findDir;
                    //    //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                    //}
                    //else
                    //{
                    //    __findNum = Misc.RandInt(1, 4);
                    //    curSpeed = 0;
                    //}

                    //if(false == GridManager.Inst.IsVisibleTile(_pos, pos_1, 10))
                    //{
                    //    curSpeed = 0.1f; 
                    //}

                }

                if (false)
                {
                    //Vector3 feeler_pos = _pos + _rotation * _feelers[0];
                    //CellSpace findCell = GridManager.Inst.Find_FirstEntityTile(this, _pos, feeler_pos, 5);
                    //if (null != findCell)
                    //{
                    //    feeler_pos = _pos + (_rotation * _feelers[0]).normalized * _radius;
                    //    DebugWide.AddDrawQ_Circle(feeler_pos, 0.1f, Color.red);
                    //    float sum_r = findCell._head._radius + _radius;
                    //    //float sum_r = findCell._head._radius + 0.3f;
                    //    if ((findCell._head._pos - feeler_pos).sqrMagnitude <= sum_r * sum_r)
                    //    {
                    //        curSpeed = 0;
                    //    }
                    //}
                    //feeler_pos = _pos + _rotation * _feelers[1];
                    //findCell = GridManager.Inst.Find_FirstEntityTile(this, _pos, feeler_pos, 5);
                    //if (null != findCell)
                    //{
                    //    feeler_pos = _pos + (_rotation * _feelers[1]).normalized * _radius;
                    //    DebugWide.AddDrawQ_Circle(feeler_pos, 0.1f, Color.red);
                    //    float sum_r = findCell._head._radius + _radius;
                    //    //float sum_r = findCell._head._radius + 0.3f;
                    //    if ((findCell._head._pos - feeler_pos).sqrMagnitude <= sum_r * sum_r)
                    //    {
                    //        curSpeed = 0;
                    //    }
                    //}
                    //feeler_pos = _pos + _rotation * _feelers[2];
                    //findCell = GridManager.Inst.Find_FirstEntityTile(this, _pos, feeler_pos, 5);
                    //if (null != findCell)
                    //{
                    //    feeler_pos = _pos + (_rotation * _feelers[2]).normalized * _radius;
                    //    DebugWide.AddDrawQ_Circle(feeler_pos, 0.1f, Color.red);
                    //    float sum_r = findCell._head._radius + _radius;
                    //    //float sum_r = findCell._head._radius + 0.3f;
                    //    if ((findCell._head._pos - feeler_pos).sqrMagnitude <= sum_r * sum_r)
                    //    {
                    //        curSpeed = 0;
                    //    }
                    //}
                }

                //-----------
                //if (_stop) 
                //{
                //    curSpeed = 0.1f; //속도변화에 따른 떨림의 원인이 된다
                //}

                //최대속도가 높을수록 진형을 잘 유지한다 
                //설정된 최대속도로 등속도 운동하게 한다.
                //_worldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
                Vector3 ToOffset = _worldOffsetPos - _pos;
                Vector3 pos_future = _pos + _velocity.normalized * curSpeed * deltaTime; //미래위치 계산 
                Vector3 ToFuture = _worldOffsetPos - pos_future;

                //DebugWide.AddDrawQ_Line(_pos, pos_future, Color.white);
                //DebugWide.AddDrawQ_Circle(pos_future, 0.1f,Color.white);
                //pos_future = GridManager.Inst.Collision_StructLine_Test3(_pos, pos_future, _radius, out _stop);
                ////DebugWide.LogBlue(_stop);
                //if (_stop)
                //{
                //    //_withstand = 100f;

                //    //지형충돌 상태고, 진형위치가 원의 범위 밖에 있을 때만 정지시킨다 - 떨림방지처리 
                //    if((_worldOffsetPos - _pos).sqrMagnitude > _radius*_radius)
                //    {
                //        SetPos(pos_future);
                //    }
                //}
                //else
                //{
                //    //_withstand = 0f;

                //    if (ToOffset.sqrMagnitude >= ToFuture.sqrMagnitude)
                //        SetPos(pos_future);
                //    else
                //    {
                //        float distSpeed = (_worldOffsetPos - _pos).magnitude;
                //        //SetPos(WorldOffsetPos); //목표오프셋 위치로 설정  - 순간이동 버그가 있어 제거 
                //        SetPos(_pos + _velocity.normalized * distSpeed * deltaTime); //거리를 속도로 사용  

                //    }
                //}


                //---------------------

                if (curSpeed > 0)
                //if ((_worldOffsetPos - ToFuture).sqrMagnitude > _radius * _radius)
                {

                    if (ToOffset.sqrMagnitude >= ToFuture.sqrMagnitude)
                        SetPos(pos_future);
                    else
                    {
                        float distSpeed = (_worldOffsetPos - _pos).magnitude;
                        //SetPos(WorldOffsetPos); //목표오프셋 위치로 설정  - 순간이동 버그가 있어 제거 
                        SetPos(_pos + _velocity.normalized * distSpeed * deltaTime); //거리를 속도로 사용  

                    }
                }

                //-------------

            }
            //else
            //{
            //    _speed = 0;
            //    _velocity = ConstV.v3_zero;

            //}


        }

        public void Update2(float deltaTime)
        {

            _oldPos = _pos;


            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.fast) * _weight;
                //SteeringForce = _steeringBehavior.Seek(_target) * _weight;
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset) * _weight;

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);


            Vector3 acceleration = SteeringForce / _mass;

            _velocity *= _Friction; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 

            Vector3 desiredVelocity = _velocity + acceleration;
            if (desiredVelocity.sqrMagnitude > 0.001f)
            {

                float def = VOp.Sign_ZX(_heading, _velocity + acceleration);
                float max_angle = Geo.AngleSigned_AxisY(_heading, _velocity + acceleration);
                float angle = _anglePerSecond * def * deltaTime;

                //최대회전량을 벗어나는 양이 계산되는 것을 막는다 
                if (Math.Abs(angle) > Math.Abs(max_angle))
                {
                    _velocity += acceleration * deltaTime; //안미끄러짐 
                    _heading = VOp.Normalize(_velocity);


                    //DebugWide.LogRed("---------------------------------------------");
                    //DebugWide.LogRed(max_angle + "   " + angle + "   " + def + "  " + _heading + "   " + _velocity + "  " + acceleration);

                }
                else
                {
                    //_anglePerSecond 값이 1000 이 넘어가면 빙빙도는것이 튀는것으로 보인다. 값이 적당히 커서 튀는 것으로 보이는것이다
                    //값을 10000 이상으로 설정하면 max_angle 값을 넘어가 튀는 현상을 피할수 있다  
                    _heading = Quaternion.AngleAxis(angle, ConstV.v3_up) * _heading;
                    _heading = VOp.Normalize(_heading);
                    _velocity += _heading * (_velocity + acceleration).magnitude * deltaTime; //실제 미끄러지게 하는 처리 

                    //DebugWide.LogBlue(max_angle + "   " + angle + "   " + def + "  " + _heading + "   " + _velocity + "  " + acceleration);
                }


                _velocity = VOp.Truncate(_velocity, _maxSpeed);
                _speed = _velocity.magnitude;
                _rotation = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

                //----------

                //Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
                ////DebugWide.AddDrawQ_Circle(WorldOffsetPos, 0.1f, Color.green);
                //Vector3 ToOffset = WorldOffsetPos - _pos;
                //_velocity = ToOffset;
                //SetPos(_pos + _velocity);

                //-----------

                //가속도 이동 
                SetPos(_pos + _velocity * deltaTime);

                //등속도 이동
                //SetPos(_pos + _heading * _maxSpeed * deltaTime); 

                //8방향으로만 이동 
                //Vector3 n = Misc.GetDir8_Normal3D(_heading); 
                //SetPos(_pos + n * _speed * deltaTime);
            }


        }

        public Vector3 WrapAroundXZ(Vector3 pos, int MaxX, int MaxY)
        {
            if (pos.x > MaxX) { pos.x = 0.0f; }

            if (pos.x < 0) { pos.x = (float)MaxX; }

            if (pos.z < 0) { pos.z = (float)MaxY; }

            if (pos.z > MaxY) { pos.z = 0.0f; }

            return pos;
        }


        public void Draw(Color color)
        {


            Vector3 vb0, vb1, vb2;
            vb0 = _rotation * _array_VB[0] * _radius;
            vb1 = _rotation * _array_VB[1] * _radius;
            vb2 = _rotation * _array_VB[2] * _radius;

            //에이젼트 출력 
            DebugWide.DrawLine(_pos + vb0, _pos + vb1, color);
            DebugWide.DrawLine(_pos + vb1, _pos + vb2, color);
            DebugWide.DrawLine(_pos + vb2, _pos + vb0, color);
            DebugWide.DrawCircle(_pos, _radius, color); 

            //if (SteeringBehavior.eType.wander == _mode)
            //{
            //    _steeringBehavior.DrawWander();
            //}

            if(null != _leader)
            {
                Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
                DebugWide.DrawCircle(WorldOffsetPos, 0.1f, Color.green);
            }

        }


    }//end class


    public class SteeringBehavior
    {
        //public enum SummingMethod
        //{
        //    weighted_average,
        //    prioritized,
        //    dithered
        //}


        public enum eType
        {
            none = 0x00000,
            seek = 0x00002,
            flee = 0x00004,
            arrive = 0x00008,
            wander = 0x00010,
            cohesion = 0x00020,
            separation = 0x00040,
            allignment = 0x00080,
            obstacle_avoidance = 0x00100,
            wall_avoidance = 0x00200,
            follow_path = 0x00400,
            pursuit = 0x00800,
            evade = 0x01000,
            interpose = 0x02000,
            hide = 0x04000,
            flock = 0x08000,
            offset_pursuit = 0x10000,
        }

        public enum Deceleration { slow = 3, normal = 2, fast = 1 };

        public Vehicle _vehicle;

        //public Vector3 _steeringForce;

        //public Vector3 _target;

        //==================================================

        public Vector3 Seek(Vector3 TargetPos)
        {
            Vector3 DesiredVelocity = (TargetPos - _vehicle._pos).normalized
                            * _vehicle._maxSpeed;


            return (DesiredVelocity - _vehicle._velocity);
        }

        public Vector3 Arrive(Vector3 TargetPos,
                        Deceleration deceleration)
        {
            Vector3 ToTarget = TargetPos - _vehicle._pos;

            //calculate the distance to the target
            float dist = ToTarget.magnitude;

            if (dist > 0.1f) //최소거리값을 적용한다 
            {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration..
                const float DecelerationTweaker = 0.3f;

                //calculate the speed required to reach the target given the desired
                //deceleration
                float speed = dist / ((float)deceleration * DecelerationTweaker);

                //make sure the velocity does not exceed the max
                speed = Math.Min(speed, _vehicle._maxSpeed);

                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                Vector3 DesiredVelocity = ToTarget * speed / dist; //toTarget / dist = 정규화벡터 

                return (DesiredVelocity - _vehicle._velocity);
            }

            return Vector3.zero;
        }

        public Vector3 OffsetPursuit(BaseEntity leader, Vector3 offset)
        {
            //calculate the offset's position in world space
            Vector3 WorldOffsetPos = (leader._rotation * offset) + leader._pos; //PointToWorldSpace

            Vector3 ToOffset = WorldOffsetPos - _vehicle._pos;

            //최소거리 이하로 계산됐다면 처리하지 않는다 - 이상회전 문제 때문에 예외처리함 
            //const float MinLen = 0.1f;
            //const float SqrMinLen = MinLen * MinLen;
            //if (ToOffset.sqrMagnitude < SqrMinLen)
            //return ConstV.v3_zero;

            //DebugWide.AddDrawQ_Circle(WorldOffsetPos, 0.1f, Color.red);


            //the lookahead time is propotional to the distance between the leader
            //and the pursuer; and is inversely proportional to the sum of both
            //agent's velocities
            float LookAheadTime = ToOffset.magnitude /
                                  (_vehicle._maxSpeed + leader._speed);

            //LookAheadTime += TurnAroundTime(_vehicle, WorldOffsetPos, 1); //이렇게 턴시간 늘리는 것은 아닌것 같음 

            //------------------------
            //Vector3 prPos = WorldOffsetPos + leader._velocity * LookAheadTime;
            //DebugWide.AddDrawQ_Circle(prPos, 0.1f, Color.green);
            //DebugWide.LogBlue(LookAheadTime);
            //------------------------

            //now Arrive at the predicted future position of the offset
            return Arrive(WorldOffsetPos + leader._velocity * LookAheadTime, Deceleration.fast);

        }

        public float TurnAroundTime(Vehicle agent, Vector3 targetPos, float turnSecond)
        {
            Vector3 toTarget = (targetPos - agent._pos).normalized;

            float dot = Vector3.Dot(agent._heading, toTarget);

            float coefficient = 0.5f * turnSecond; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 1초
            //const float coefficient = 0.5f * 5; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 5초

            return (dot - 1f) * -coefficient; //[-2 ~ 0] * -coefficient
        }
    }

}//end namespace



