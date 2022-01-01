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

        public static readonly List<Character> list = new List<Character>();
        public static int Add(Character v)
        {
            list.Add(v);
            return list.Count - 1;

        }
    }

    public class Proto_AI_4 : MonoBehaviour
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
        public float _radius_damage = 0.5f;
        public float _mass = 1f;
        public float _weight = 20;
        public float _maxForce = 40f;
        public float _maxSpeed = 10f;
        public float _withstand = 1f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;

        public bool _isObjNonpenetration = true;
        public bool _isStrNonpenetration = true;

        public float _minRange = 0;
        public float _maxRange = 0.5f;

        public FormationPoint _formationPoint = new FormationPoint();

        public bool _Draw_EntityTile = false;
        public bool _Draw_UpTile = false;
        public bool _Draw_StructTile = false;
        public bool _Draw_BoundaryTile = false;
        public bool _Draw_ArcTile = false;

        public bool _Draw_SphereTree_Struct = false;
        public bool _Draw_SphereTree = false;
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

            ObjectManager.Inst.Init();

            _formationPoint._target = _tr_target.position;
            _formationPoint._pos = _tr_target.position;

            //------

            EntityMgr.list.Clear();

            //0
            Character v = new Character();
            int id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._mode = SteeringBehavior.eType.arrive;
            v._leader = _formationPoint;
            v._offset = new Vector3(0, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;
            //v._maxSpeed = 14;

            //1
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //2
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(-1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //-------------------

            //3
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(1f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            ////4
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            v._leader = _formationPoint;
            v._offset = new Vector3(2f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            ////5
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(3f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;
            v._mode = SteeringBehavior.eType.arrive;


            for (int i=0;i<30;i++)
            {
                v = new Character();
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


        int ID_0 = 0;
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
            Character vh = EntityMgr.list[ID_0];

            _formationPoint._speed = _formation_speed;
            _formationPoint._target = _tr_target.position;
            _formationPoint.Update(deltaTime);
            KeyInput();
            //vh._target = _formationPoint._pos; //0번째 객체에만 특별히 부여 , 도착시험하기 위함 

            //float kmPerHour = (3600f / 1000f) * _maxSpeed;
            //DebugWide.LogBlue(ID + "  시간당 속도: " + kmPerHour + "  초당 속도: " + _maxSpeed + "  초당 거리: " + _maxSpeed * deltaTime);
            //운반기의 반지름이 0.5 이며 타일한개의 길이가 1인 경우 : _maxSpeed * deltaTime 의 값이 1.5 를 넘으면 지형을 통과하게 된다 
            //운반기의 반지름이 0과 가까운 아주 작은 값일 경우 : _maxSpeed * deltaTime 의 값이 1 을 넘으면 지형을 통과하게 된다 
            //현재의 타일기반 지형충돌 알고리즘으로는 _maxSpeed 가 30 (시속108) 까지만 충돌처리가 가능하다 

            //==============================================

            ObjectManager.Inst._sphereTree._gravy_supersphere = _SphereTree_Gravy;

            foreach (Character v in EntityMgr.list)
            {
                if(5 == v._id)
                {
                    v._withstand = _withstand;
                    v._target = _tr_test.position;
                    v.SetRadius(_radius);
                    v._radius_damage = _radius_damage;
                }
                if (6 <= v._id) 
                {
                    v._target = _tr_test2_s.position;
                }

                //v._radius = _radius;
                //v.SetRadius(_radius);
                v._mass = _mass;
                v._maxSpeed = _maxSpeed;
                v._maxForce = _maxForce;
                v._Friction = _Friction;
                v._anglePerSecond = _anglePerSecond;
                v._weight = _weight;
                v._isNonpenetration = _isObjNonpenetration;
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
                Character src = EntityMgr.list[key._V0];
                Character dst = EntityMgr.list[key._V1];

                if (src == dst) continue;

                if (_isObjNonpenetration)
                    CollisionPush(src, dst);
            }

            //==============================================

            foreach (Character v in EntityMgr.list)
            {
                //==========================================
                //동굴벽과 캐릭터 충돌처리 

                //객체의 반지름이 <0.1 ~ 0.99> 범위에 있어야 한다.
                //float maxR = Mathf.Clamp(v._radius, 0.1, 0.99); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 

                //DebugWide.AddDrawQ_Line(v._pos, _formationPoint._pos, Color.magenta);
                //DebugWide.AddDrawQ_Line(v._pos, v._oldPos, Color.red);

                if (_isStrNonpenetration)
                    v.SetPos(_gridMgr.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius, out v._stop));

                
                //DebugWide.AddDrawQ_Line(v._pos, _formationPoint._pos, Color.gray);

                //==========================================
            }

            ObjectManager.Inst.Update(deltaTime);
        }

        public void KeyInput()
        {
            const float MOVE_LENGTH = 1f;
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = VOp.Normalize(n);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }
            if (Input.GetKey(KeyCode.S))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = -VOp.Normalize(n);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = -VOp.PerpN(n, Vector3.up);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }
            if (Input.GetKey(KeyCode.D))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = VOp.PerpN(n, Vector3.up);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }

        }

        public void CollisionPush(Character src, Character dst)
        {
            if (null == src || null == dst) return;


            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            //Vector3 dir_dstTOsrc = VOp.Minus(src._pos, dst._pos);
            Vector3 dir_dstTOsrc = src._pos - dst._pos;
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

            //ObjectManager.Inst._sphereTree_struct.Debug_RayTrace(_tr_test.position, _tr_line_a.position);
            //ObjectManager.Inst._sphereTree_struct.Debug_RangeTest(_tr_test.position, _maxRange);
            //ObjectManager.Inst._sphereTree.Debug_RangeTest(_tr_test.position, _maxRange);

            //if(GridManager.Inst.IsVisibleTile(_tr_test.position, _tr_line_a.position, 10))
            //{
            //    DebugWide.AddDrawQ_Line(_tr_test.position, _tr_line_a.position, Color.red); 
            //}


            _formationPoint.Draw(Color.white);

            Color color = Color.black;
            foreach (Character v in EntityMgr.list)
            {
                color = Color.black;
                if (0 == v._id)
                    color = Color.red;
                if (5 == v._id)
                    color = Color.yellow;

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


            if (true == _Draw_SphereTree)
            {
                ObjectManager.Inst.Draw(_SphereTree_Level_0, _SphereTree_Level_1, _SphereTree_Level_2, _SphereTree_Level_3);
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



