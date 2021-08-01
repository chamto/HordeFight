using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Proto_AI;

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

    public class Proto_AI_2 : MonoBehaviour
    {
        public Transform _tr_target = null;
        public GridManager _gridMgr = new GridManager();
        public SweepPrune _sweepPrune = new SweepPrune();

        public Transform _tr_test = null;
        public Transform _tr_line_a = null;
        public Transform _tr_line_b = null;

        public float _formation_speed = 10;
        public float _radius = 0.5f;
        public float _mass = 1f;
        public float _maxSpeed = 10f;
        public float _maxForce = 40f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;
        public float _weight = 20;
        public bool _isNonpenetration = true;

        public FormationPoint _formationPoint = new FormationPoint();

        void Awake()
        {
            //QualitySettings.vSyncCount = 0; //v싱크 끄기
            //Application.targetFrameRate = 60; //60 프레임 제한

            FrameTime.SetFixedFrame_FPS_30(); //30프레임 제한 설정
        }

        // Use this for initialization
        void Start()
        {
            _tr_target = GameObject.Find("tr_target").transform;

            _tr_test = GameObject.Find("Test").transform;
            _tr_line_a = GameObject.Find("line_a").transform;
            _tr_line_b = GameObject.Find("line_b").transform;

            _gridMgr.Init();

            _formationPoint._end = _tr_target.position;
            _formationPoint._pos = _tr_target.position;

            //------

            //0
            Vehicle v = new Vehicle();
            int id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._mode = SteeringBehavior.eType.arrive;
            v._leader = _formationPoint;
            v._offset = new Vector3(0, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //1
            //v = new Vehicle();
            //id = EntityMgr.Add(v);
            //v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(1f, 0, -1f);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            ////2
            //v = new Vehicle();
            //id = EntityMgr.Add(v);
            //v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(-1f, 0, -1f);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            ////-------------------

            ////3
            //v = new Vehicle();
            //id = EntityMgr.Add(v);
            //v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(1f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            //////4
            //v = new Vehicle();
            //id = EntityMgr.Add(v);
            //v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(2f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            ////5
            //v = new Vehicle();
            //id = EntityMgr.Add(v);
            //v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(3f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

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
            vh._target = _formationPoint._pos; //0번째 객체에만 특별히 부여 , 도착시험하기 위함 

            //float kmPerHour = (3600f / 1000f) * vh._maxSpeed;
            //DebugWide.LogBlue(ID + "  시간당 속도: " + kmPerHour + "  초당 속도: " + vh._maxSpeed + "  " + vh._maxSpeed * deltaTime);
            //운반기의 반지름이 0.5 이며 타일한개의 길이가 1인 경우 : _maxSpeed * deltaTime 의 값이 1.5 를 넘으면 지형을 통과하게 된다 
            //운반기의 반지름이 0과 가까운 아주 작은 값일 경우 : _maxSpeed * deltaTime 의 값이 1 을 넘으면 지형을 통과하게 된다 
            //현재의 타일기반 지형충돌 알고리즘으로는 _maxSpeed 가 30 (시속108) 까지만 충돌처리가 가능하다 

            foreach (Vehicle v in EntityMgr.list)
            {
                //if (0 == v._id) v._withstand = 100; //임시 시험 

                v._radius = _radius;
                v._mass = _mass;
                v._maxSpeed = _maxSpeed;
                v._maxForce = _maxForce;
                v._Friction = _Friction;
                v._anglePerSecond = _anglePerSecond;
                v._weight = _weight;
                v._isNonpenetration = _isNonpenetration;
                v.Update(deltaTime);
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

                float maxR = Mathf.Clamp(v._radius, 0, 1); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 
                //동굴벽과 캐릭터 경계원 충돌처리 
                //v._pos = _gridMgr.Collision_StructLine_Test1(v._pos, maxR);
                v._pos = _gridMgr.Collision_StructLine_Test2(v._oldPos, v._pos, v._radius );

                //==========================================

            }
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
            float r_sum = (src._collision._radius + dst._collision._radius);
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
            }
        }

        public bool _Draw_BoundaryTile = false;
        private void OnDrawGizmos()
        {
            if (null == _tr_target) return;

            DebugWide.DrawLine(_tr_test.position, _tr_line_a.position, Color.white);
            DebugWide.DrawLine(_tr_test.position, _tr_line_b.position, Color.white);


            //_gridMgr.Find_FirstStructTile(_tr_test.position, _tr_line_a.position , 20);

            //Vehicle vh = EntityMgr.list[ID];
            //_gridMgr.Find_FirstStructTile(vh._oldPos, vh._oldPos + (vh._pos - vh._oldPos) * 100 , 5);


            //_gridMgr.Draw_line_equation3(_tr_test.position.x, _tr_test.position.z, _tr_line_a.position.x, _tr_line_a.position.z);

            //Vector3 test = _tr_line_a.position - _tr_test.position;
            //Vector3 dir4n = Misc.GetDir4_Normal3D_Y(test);
            //DebugWide.DrawLine(_tr_test.position, _tr_test.position + dir4n, Color.red);

            //DebugWide.DrawCircle(_tr_target.position, 0.1f, Color.white);
            //DebugWide.DrawLine(EntityMgr.list[0]._pos, _tr_target.position, Color.white);
            _formationPoint.Draw(Color.white);

            Color color = Color.black;
            foreach (Vehicle v in EntityMgr.list)
            {
                color = Color.black;
                if (0 == v._id)
                    color = Color.red;

                v.Draw(color);
            }

            if(true == _Draw_BoundaryTile)
                _gridMgr.Draw_BoundaryTile();

            DebugWide.DrawQ_All_AfterTime(1);
            //DebugWide.DrawQ_Dequeue();
        }
    }

    public class BaseEntity
    {
        public Vector3 _oldPos = Vector3.zero;
        public Vector3 _pos = Vector3.zero;
        public Vector3 _velocity = Vector3.zero;
        public float _speed = 10f;
        public Quaternion _rotation = Quaternion.identity;
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
            if ((_end - _pos).sqrMagnitude < 5)
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

        //public Vector3 _size =  new Vector3(0.5f, 0, 0.5f);

        public bool _tag = false;
        public float _radius = 0.5f;


        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public BaseEntity _leader = null;
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();

        public SweepPrune.CollisionObject _collision = new SweepPrune.CollisionObject();

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

            _collision._id = _id;
            _collision._radius = radius;
            SetPos(pos);
            _oldPos = pos;
        }

        public void SetPos(Vector3 newPos)
        {
            _pos = newPos;

            //!!!!! 경계상자 위치 갱신
            _collision._bounds_min.x = newPos.x - _radius;
            _collision._bounds_min.z = newPos.z - _radius;
            _collision._bounds_max.x = newPos.x + _radius;
            _collision._bounds_max.z = newPos.z + _radius;
            //==============================================
        }

        public void Update(float deltaTime)
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

                //-----------

                //최대속도가 높을수록 진형을 잘 유지한다 
                //설정된 최대속도로 등속도 운동하게 한다.
                Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
                Vector3 ToOffset = WorldOffsetPos - _pos;
                Vector3 pos_future = _pos + _velocity.normalized * _maxSpeed * deltaTime;
                Vector3 ToFuture = pos_future - WorldOffsetPos;
                if (ToOffset.sqrMagnitude > ToFuture.sqrMagnitude)
                    SetPos(pos_future);
                else
                    SetPos(WorldOffsetPos); //목표오프셋 위치로 설정 

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

            Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
            DebugWide.DrawCircle(WorldOffsetPos, 0.1f, Color.green);
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



