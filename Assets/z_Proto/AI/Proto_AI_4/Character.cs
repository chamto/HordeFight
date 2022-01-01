using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_4
{

    public class BaseEntity : SphereModel.IUserData
    {
        public float _radius = 0.5f;
        public float _radius_damage = 0.5f; //타격크기 
        public Vector3 _oldPos = Vector3.zero;
        public Vector3 _pos = Vector3.zero;
        public Vector3 _velocity = Vector3.zero;
        public float _speed = 10f;
        public float _withstand = 1f; //버티기
        public Quaternion _rotation = Quaternion.identity;

        public CellSpace _cur_cell = null;
        public BaseEntity _prev_sibling = null;
        public BaseEntity _next_sibling = null;

        public Vector3 _target = ConstV.v3_zero;
        public bool _changeTarget = false;
        //==================================================
        // 충돌 모델 
        //==================================================
        public SweepPrune.CollisionObject _collision = new SweepPrune.CollisionObject();

        public SphereModel _sphereModel = null; //구트리
    }

    public class FormationPoint : BaseEntity
    {
        //public Vector3 _start = Vector3.zero;
        //public Vector3 _end = Vector3.zero;


        //private Vector3 _n_dir = Vector3.zero;
        //public void Init(Vector3 start, Vector3 end)
        //{
        //    _start = start;
        //    _end = end;
        //    _pos = _start;

        //    _n_dir = VOp.Normalize(_end - _start);
        //}

        public Vector3 _before_target = Vector3.zero;


        public void Update(float deltaTime)
        {
            _changeTarget = false;
            if (false == Misc.IsZero(_before_target - _target))
            {
                _changeTarget = true;
            }
            _before_target = _target;


            //도착시 종료 
            if ((_target - _pos).sqrMagnitude < 1)
            {
                //특정거리 안에서 이동없이 방향전환 
                _rotation = Quaternion.FromToRotation(Vector3.forward, _target - _pos);
                _velocity = Vector3.zero; //초기화를 안해주면 키이동시 경로가 이상해지는 문제가 발생함
                return;
            }

            //DebugWide.LogBlue("sfsdf: " + _pos );
            Vector3 n = VOp.Normalize(_target - _pos);
            _velocity = n * _speed;
            _pos += _velocity * deltaTime;

            _rotation = Quaternion.FromToRotation(Vector3.forward, _velocity);
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(_target, 0.1f, color);
            DebugWide.DrawLine(_pos, _target, color);
        }

    }

    public class Character : BaseEntity
    {
        public int _id = -1;

        //public Vector3 _velocity = new Vector3(0, 0, 0); //실제 향하고 있는 방향
        public Vector3 _heading = Vector3.forward; //객체의 방향
        public Vector3 _facing = Vector3.forward; //얼굴의 방향

        public float _mass = 1f;
        public float _maxSpeed = 10f;
        public float _maxForce = 40f;
        public float _Friction = 0.85f; //마찰력
        public float _elasticity = 1; //탄성력 
        public float _anglePerSecond = 180;
        public float _weight = 20;

        public bool _isNonpenetration = true; //비침투 

        public bool _stop = false;

        //public Vector3 _size =  new Vector3(0.5f, 0, 0.5f);

        public bool _tag = false;

        //public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vector3 _worldOffsetPos = ConstV.v3_zero;
        public BaseEntity _leader = null;
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();
        public float _decelerationTime = 0.3f; //Arrive2 알고리즘에서 사용 , 남은거리를 몇초로 이동할지 설정 

        public List<Vector3> _feelers = new List<Vector3>();

        //--------------------------------------------------
        public StateMachine<Character> _stateMachine = null;

        //--------------------------------------------------

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

        public void Init(int id, float radius, Vector3 pos)
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
            ObjectManager.Inst.AddSphereTree(this);
            //==============================================

            SetPos(pos);
            _oldPos = pos;

            CreateFeelers();

            //==============================================

            _stateMachine = new StateMachine<Character>(this);
            _stateMachine.Init(State_Move_Character.inst, StateGlobal_Charactor.inst);

        }

        public void SetPos(Vector3 newPos)
        {
            _pos = newPos;

            //==============================================
            //!!!!! 구트리 위치 갱신 
            if (null != _sphereModel)
            {
                _sphereModel.NewPos(_pos);
            }

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
            if (null != _sphereModel)
            {
                _sphereModel.NewPosRadius(_pos, _radius); //반지름도 함께 갱신되도록 한다 
            }
        }


        public void CreateFeelers()
        {
            const float Feeler_Length = 1;
            Vector3 fpos = Vector3.forward * (_radius + Feeler_Length);
            _feelers.Add(fpos);

            fpos = Quaternion.AngleAxis(45, Vector3.up) * Vector3.forward * (_radius + Feeler_Length);
            _feelers.Add(fpos);

            fpos = Quaternion.AngleAxis(-45, Vector3.up) * Vector3.forward * (_radius + Feeler_Length);
            _feelers.Add(fpos);
        }


        //객체하나에 대한 전체객체 접촉정보를 처리하는 방식, 중복된 접촉정보 있음, 계산후 겹치지 않음 
        public void EnforceNonPenetrationConstraint(Character src, List<Character> ContainerOfEntities, float src_withstand, float dst_withstand)
        {
            Character dst = null;
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                dst = ContainerOfEntities[i];
                if (src == dst) continue;

                Vector3 dir_dstTOsrc = src._pos - dst._pos;
                Vector3 n = ConstV.v3_zero;
                float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;
                float r_sum = (src._radius + dst._radius);
                float sqr_r_sum = r_sum * r_sum;

                //1.두 캐릭터가 겹친상태 
                if (sqr_dstTOsrc < sqr_r_sum)
                {

                    //==========================================
                    float rate_src, rate_dst;
                    float f_sum = src_withstand + dst_withstand;
                    if (Misc.IsZero(f_sum)) rate_src = rate_dst = 0.5f;
                    else
                    {
                        rate_src = 1f - (src_withstand / f_sum);
                        rate_dst = 1f - rate_src;
                    }

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

                    //src._oldPos = src._pos;
                    //dst._oldPos = dst._pos;

                    src._pos += n * len_bt_src;
                    dst._pos += -n * len_bt_dst;

                    //test
                    //bool a;
                    //src.SetPos(GridManager.Inst.Collision_StructLine_Test3(src._oldPos, src._pos, src._radius, out a));
                    //dst.SetPos(GridManager.Inst.Collision_StructLine_Test3(dst._oldPos, dst._pos, dst._radius, out a));
                }
            }

        }

        Vector3 _before_worldOffsetPos = Vector3.zero;
        public void Update(float deltaTime)
        {

            _oldPos = _pos;
            _worldOffsetPos = _target;
            if (null != _leader)
            {
                _worldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace 

                //---------
                if (_leader._changeTarget)
                {
                    _before_worldOffsetPos = _pos;
                }
                //DebugWide.AddDrawQ_Circle(_before_worldOffsetPos, 0.5f, Color.green);
                //---------
            }


            Update_NormalMovement(deltaTime);
            //Update_RotateMovement(deltaTime);

            EnforceNonPenetrationConstraint(this, EntityMgr.list, 1, 1); //겹침이 적게 일어나는 방식 

            //bool a;
            //SetPos(GridManager.Inst.Collision_StructLine_Test3(_oldPos, _pos, _radius, out a));
        }

        public Vector3 CalcSteeringForce()
        {
            //** 등속도이동 설정
            //_decelerationTime = 0.09f; //0.09초동안 감속 - 감속되는 것을 보여주지 않기 위해 짧게 설정 
            //_weight = 50;
            //_maxForce = 100;

            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.fast) * _weight;
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset) * _weight;
            //SteeringForce = _steeringBehavior.Seek(_worldOffsetPos) * _weight;


            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);

            return SteeringForce;
        }

        //멈추기 및 돌아가기 
        int __findNum = 0;
        public float AAA()
        {
            float curSpeed = _maxSpeed;
            //if(false)
            {
                //float[] ay_angle = new float[] { 0, 45f, -45f, 90f, -90, 135f, -135, 180f };
                float[] ay_angle = new float[] { 0, 45f, -45f, 60f, -60, 135f, -135, 180f };
                Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * _velocity.normalized;
                //float sum_r = _radius + _radius;
                Vector3 pos_1 = _pos + _velocity.normalized * (_radius);
                Vector3 pos_2 = _pos + findDir * (_radius);

                //--------------------
                //* 구트리로 객체이동 조절
                Vector3 find_pos = _pos + _velocity.normalized * (_radius + 0.3f);
                Vector3 ori_velo = _velocity;
                BaseEntity findEnty_1 = ObjectManager.Inst.RangeTest(this, pos_1, 0, _radius);
                BaseEntity findEnty_2 = ObjectManager.Inst.RangeTest(this, pos_2, 0, _radius);

                //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.white);
                if (null == findEnty_1)
                {
                    curSpeed = _maxSpeed;

                }
                else if (null == findEnty_2)
                {
                    curSpeed = _maxSpeed;
                    _velocity = findDir;
                    //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                    //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                    find_pos = _pos + findDir * (_radius + 0.3f);

                }
                else
                {
                    //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                    //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.cyan);
                    __findNum = Misc.RandInt(1, 4);
                    //if(_withstand <= findEnty_2._withstand)
                    curSpeed = 0;
                }


                if (5 == _id)
                {
                    curSpeed = _maxSpeed;
                    _velocity = ori_velo;
                    //_withstand = 1.2f;
                }


                if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                {
                    //DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                    __findNum = Misc.RandInt(1, 4);
                    curSpeed = 0f;
                    _velocity = ori_velo;
                }

            }

            return curSpeed;
        }

        public void BBB()
        {
            float curSpeed = _maxSpeed;
            //if(false)
            {
                //float[] ay_angle = new float[] { 0, 45f, -45f, 90f, -90, 135f, -135, 180f };
                float[] ay_angle = new float[] { 0, 45f, -45f, 60f, -60, 135f, -135, 180f };
                Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * _velocity.normalized;
                float sum_r = _radius + _radius;
                Vector3 pos_1 = _pos + _velocity.normalized * (_radius);
                Vector3 pos_2 = _pos + findDir * (_radius);

                //-----------------
                //* 셀공간으로 객체이동 조절
                CellSpace findCell_1 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + _velocity.normalized * sum_r, 5);
                CellSpace findCell_2 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + findDir * sum_r, 5);
                Vector3 find_pos = _pos + _velocity.normalized * (_radius + 0.3f);
                Vector3 ori_velo = _velocity;
                if (null == findCell_1)
                //|| (null != findCell_1 && (findCell_1._head._pos - pos_1).sqrMagnitude > sum_r * sum_r))
                {
                    curSpeed = _maxSpeed;
                }
                else if (null == findCell_2)
                //|| (null != findCell_2 && (findCell_2._head._pos - pos_2).sqrMagnitude > sum_r * sum_r))
                {
                    curSpeed = _maxSpeed;
                    _velocity = findDir;
                    //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                    find_pos = _pos + findDir * (_radius + 0.3f);

                    if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                    {
                        DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                        __findNum = Misc.RandInt(1, 4);
                        curSpeed = 0f;
                        _velocity = ori_velo;
                    }
                }
                else
                {
                    __findNum = Misc.RandInt(1, 4);
                    curSpeed = 0;
                }

                if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                {
                    DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                    __findNum = Misc.RandInt(1, 4);
                    curSpeed = 0f;
                    _velocity = ori_velo;
                }

                //--------------------

                Vector3 calcPos;
                if (null != GridManager.Inst.Find_FirstStructTile4(_pos, find_pos, 0.1f, out calcPos))
                {
                    DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                    __findNum = Misc.RandInt(1, 4);
                    curSpeed = 0f;
                    _velocity = ori_velo;
                }


            }
        }

        public void Update_NormalMovement(float deltaTime)
        {

            Vector3 SteeringForce = CalcSteeringForce();

            //*기본계산
            Vector3 acceleration = SteeringForce / _mass;


            //maxSpeed < maxForce 일 경우 *기본계산 처럼 작동 
            //maxSpeed > maxForce 일 경우 회전효과가 생긴다 
            //관성으로 미끄러지는 효과가 생긴다 
            //일정하게 가속하도록 한다. 질량이 1일때 1초당 f = a = v = s 가 된다. f 를 s 로 보고 초당 가속하는 거리를 예측할 수 있다 
            //Vector3 acceleration = SteeringForce.normalized * (_maxForce / _mass);
            _velocity += acceleration * deltaTime;

            _velocity *= (float)Math.Pow(_Friction, deltaTime);

            _velocity = VOp.Truncate(_velocity, _maxSpeed);

            //DebugWide.LogBlue("stf: "+SteeringForce.magnitude + "  v: " + _velocity.magnitude + "  a: " + acceleration.magnitude + "  a/s: " + acceleration.magnitude * deltaTime);

            Vector3 ToOffset = _worldOffsetPos - _pos;
            if (ToOffset.sqrMagnitude > 0.001f)
            {
                float def = VOp.Sign_ZX(_heading, ToOffset);
                float max_angle = Geo.AngleSigned_AxisY(_heading, ToOffset);
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


                //-----------
                //if (0 == AAA()) return;

                //Vector3 pos_future = _pos + _velocity.normalized * curSpeed * deltaTime; //미래위치 계산 - 등속도
                Vector3 pos_future = _pos + _velocity * deltaTime; //미래위치 계산 - 가속도 


                //최대각차이가 지정값 보다 작을 때만 이동시킨다 
                // if (Math.Abs(max_angle) < 1f)
                {
                    SetPos(pos_future);
                }

                //-------------

            }

        }

        public void Update_RotateMovement(float deltaTime)
        {

            Vector3 SteeringForce = CalcSteeringForce();

            Vector3 acceleration = SteeringForce / _mass;

            //----------------------------------------------------

            Vector3 ToOffset = _worldOffsetPos - _pos;
            if (ToOffset.sqrMagnitude > 0.001f)
            {
                float def = VOp.Sign_ZX(_heading, ToOffset);
                float max_angle = Geo.AngleSigned_AxisY(_heading, ToOffset);
                float angle = _anglePerSecond * def * deltaTime;


                //_anglePerSecond 값이 너무 크면 빙빙돌면서 튀는것 처럼 보인다. 최대 회전각을 벗어나지 않도록 예외처리 한다 
                if (Math.Abs(angle) > Math.Abs(max_angle))
                {
                    angle = max_angle;
                }


                _heading = Quaternion.AngleAxis(angle, ConstV.v3_up) * _heading;
                _heading = VOp.Normalize(_heading);
                _velocity += _heading * (acceleration).magnitude * deltaTime; //실제 미끄러지게 하는 처리

                _velocity *= (float)Math.Pow(_Friction, deltaTime); //마찰력 적용 

                _velocity = VOp.Truncate(_velocity, _maxSpeed);
                _speed = _velocity.magnitude;
                _rotation = Quaternion.FromToRotation(ConstV.v3_forward, _heading);


                //-----------
                //DebugWide.LogBlue("stf: " + SteeringForce.magnitude + "  v: " + _velocity.magnitude + "  a: " + acceleration.magnitude + "  a/s: " + acceleration.magnitude * deltaTime);
                //-----------

                //if (0 == AAA()) return;

                Vector3 pos_future = _pos + _velocity * deltaTime; //미래위치 계산 

                SetPos(pos_future);
            }

        }



        //int __findNum = 0;
        public void Update0(float deltaTime)
        {

            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.fast) * _weight;
            //SteeringForce = _steeringBehavior.Seek(_target) * _weight;
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset) * _weight;

            //-----------
            //_weight 값을 작은값(1) 으로 설정하면 회전효과가 생긴다 
            //_velocity = _velocity.normalized * _maxSpeed; //최대값 설정 
            //Vector3 DesiredVelocity = (_worldOffsetPos - _pos).normalized * _maxSpeed;
            //SteeringForce = (DesiredVelocity - _velocity) * _weight;
            //-----------

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);


            Vector3 acceleration = SteeringForce / _mass;


            _velocity *= _Friction; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 

            _velocity += acceleration * deltaTime; //*기본계산

            //----------
            //f = m * a , a = v / t , f = m * (v / t) , f = (m * v) / t , f * t = (m * v)
            //t = (m * v) / f
            //a * t = v , t = v / a

            //v = s / t , a * t = s / t , a * t2 = s , t2 = s / a 
            //s = v * t

            //float t1_a = (_mass * _maxSpeed) / SteeringForce.magnitude;
            //float t2_a = _maxSpeed / acceleration.magnitude;
            //float t_v = t1_a * t1_a;
            //DebugWide.LogBlue("time : "+t1_a + "  " + t2_a + "  " + t_v);
            //----------


            //DebugWide.LogBlue(SteeringForce.magnitude);
            //if ((_worldOffsetPos - _pos).magnitude > 0.3f )
            {
                //maxSpeed < maxForce 일 경우 *기본계산 처럼 작동 
                //maxSpeed > maxForce 일 경우 회전효과가 생긴다 
                //관성으로 미끄러지는 효과가 생긴다 
                //일정하게 가속하도록 한다. 질량이 1일때 1초당 f = a = v = s 가 된다. f 를 s 로 보고 초당 가속하는 거리를 예측할 수 있다 
                //acceleration = SteeringForce.normalized * (_maxForce / _mass);
                //_velocity += acceleration * deltaTime;
                //DebugWide.LogBlue("--------------------- ");
            }
            //else
            {
                //_velocity = Vector3.zero; 
            }

            //-----------
            //DebugWide.AddDrawQ_Line(_pos, _pos + _velocity, Color.yellow); //가속이 적용되기전의 속도 출력 
            //DebugWide.AddDrawQ_Line(_pos, _pos + SteeringForce, Color.black);
            //DebugWide.LogBlue("f: "+SteeringForce.magnitude + "  v: " + _velocity.magnitude + "  a: " + acceleration.magnitude + "  " + acceleration.magnitude * deltaTime);
            //-----------

            //_velocity += acceleration * deltaTime;


            //-----------
            //_velocity *= 1.5f; //최대속도로 빨리 올라가게 한다 
            //_velocity = _velocity.normalized * _maxSpeed;
            //DebugWide.AddDrawQ_Line(_pos, _pos + _velocity, Color.red);
            //DebugWide.AddDrawQ_Circle(_pos + _velocity, 0.1f, Color.white);
            //if (Misc.IsZero(_velocity.magnitude - _maxSpeed, 0.1f))
            //{
            //    DebugWide.AddDrawQ_Circle(_pos + _velocity, 0.1f, Color.red);
            //}
            //-----------

            _velocity = VOp.Truncate(_velocity, _maxSpeed);


            if (_velocity.sqrMagnitude > 0.001f)
            {

                //float def = VOp.Sign_ZX(_heading, _velocity); //weight 값에 의해 방향이 부정확하여 떠는 문제 발생
                //float max_angle = Geo.AngleSigned_AxisY(_heading, _velocity);
                float def = VOp.Sign_ZX(_heading, _worldOffsetPos - _pos);
                float max_angle = Geo.AngleSigned_AxisY(_heading, _worldOffsetPos - _pos);

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


                //-----------

                //최대각차이가 지정값 보다 작을 때만 이동시킨다 
                //if (Math.Abs(max_angle) < 1f)
                //{
                //    SetPos(_pos + _velocity * deltaTime);
                //}


                //-----------
                float curSpeed = _maxSpeed;
                //if(false)
                {
                    //float[] ay_angle = new float[] { 0, 45f, -45f, 90f, -90, 135f, -135, 180f };
                    float[] ay_angle = new float[] { 0, 45f, -45f, 60f, -60, 135f, -135, 180f };
                    Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * _velocity.normalized;
                    //float sum_r = _radius + _radius;
                    Vector3 pos_1 = _pos + _velocity.normalized * (_radius);
                    Vector3 pos_2 = _pos + findDir * (_radius);


                    //--------------------
                    //* 구트리로 객체이동 조절
                    Vector3 find_pos = _pos + _velocity.normalized * (_radius + 0.3f);
                    Vector3 ori_velo = _velocity;
                    BaseEntity findEnty_1 = ObjectManager.Inst.RangeTest(this, pos_1, 0, _radius);
                    BaseEntity findEnty_2 = ObjectManager.Inst.RangeTest(this, pos_2, 0, _radius);

                    //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.white);
                    if (null == findEnty_1)
                    {
                        curSpeed = _maxSpeed;

                    }
                    else if (null == findEnty_2)
                    {
                        curSpeed = _maxSpeed;
                        _velocity = findDir;
                        //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                        //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                        find_pos = _pos + findDir * (_radius + 0.3f);

                    }
                    else
                    {
                        //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                        //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.cyan);
                        __findNum = Misc.RandInt(1, 4);
                        //if(_withstand <= findEnty_2._withstand)
                        curSpeed = 0;
                    }


                    if (5 == _id)
                    {
                        curSpeed = _maxSpeed;
                        _velocity = ori_velo;
                        //_withstand = 1.2f;
                    }


                    if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                    {
                        //DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                        __findNum = Misc.RandInt(1, 4);
                        curSpeed = 0f;
                        _velocity = ori_velo;
                    }

                }

                //Vector3 find_pos2 = _pos + _heading * (_radius + 0.3f);
                //if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos2, 10))
                //{
                //    _velocity *= 0.3f;
                //}

                //_velocity = _velocity.normalized * curSpeed; //속도를 직접 조작하면 비정상 결과가 나온다 

                //-----------

                Vector3 nDir = VOp.Normalize(_worldOffsetPos - _pos);

                //최대속도가 높을수록 진형을 잘 유지한다 
                //설정된 최대속도로 등속도 운동하게 한다.
                Vector3 ToOffset = _worldOffsetPos - _pos;
                //Vector3 pos_future = _pos + nDir * curSpeed * deltaTime; //미래위치 계산 - 등속도
                Vector3 pos_future = _pos + _velocity.normalized * curSpeed * deltaTime; //미래위치 계산 - 등속도
                //Vector3 pos_future = _pos + _velocity * deltaTime; //미래위치 계산 - 가속도 
                Vector3 ToFuture = _worldOffsetPos - pos_future;


                //---------------------

                //최대각차이가 지정값 보다 작을 때만 이동시킨다 
                //if (Math.Abs(max_angle) < 1f)
                {
                    SetPos(pos_future);
                }

                //if (curSpeed > 0)
                //if ((_worldOffsetPos - ToFuture).sqrMagnitude > _radius * _radius)
                //{

                //    if (ToOffset.sqrMagnitude >= ToFuture.sqrMagnitude)
                //        SetPos(pos_future);
                //    else
                //    {
                //        float distSpeed = (_worldOffsetPos - _pos).magnitude;
                //        //SetPos(WorldOffsetPos); //목표오프셋 위치로 설정  - 순간이동 버그가 있어 제거 
                //        SetPos(_pos + _velocity.normalized * distSpeed * deltaTime); //거리를 속도로 사용  

                //    }
                //}

                //-------------

            }


        }

        public void Update1(float deltaTime)
        {
            //_oldPos = _pos;
            //_worldOffsetPos = _target;
            //if (null != _leader)
            //{
            //    _worldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace 
            //}


            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.fast) * _weight;
            //SteeringForce = _steeringBehavior.Seek(_target) * _weight;
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset) * _weight;

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);


            Vector3 acceleration = SteeringForce / _mass;

            _velocity *= _Friction; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 

            _velocity += acceleration * deltaTime;

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

                float def = VOp.Sign_ZX(_heading, _velocity);

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
                    //float sum_r = _radius + _radius;
                    Vector3 pos_1 = _pos + _velocity.normalized * (_radius);
                    Vector3 pos_2 = _pos + findDir * (_radius);

                    //-----------------
                    //* 셀공간으로 객체이동 조절
                    //CellSpace findCell_1 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + _velocity.normalized * sum_r, 5);
                    //CellSpace findCell_2 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + findDir * sum_r, 5);
                    //Vector3 find_pos = _pos + _velocity.normalized * (_radius+0.3f);
                    //Vector3 ori_velo = _velocity;
                    //if (null == findCell_1 )
                    ////|| (null != findCell_1 && (findCell_1._head._pos - pos_1).sqrMagnitude > sum_r * sum_r))
                    //{
                    //    curSpeed = _maxSpeed;
                    //}
                    //else if (null == findCell_2 )
                    ////|| (null != findCell_2 && (findCell_2._head._pos - pos_2).sqrMagnitude > sum_r * sum_r))
                    //{
                    //    curSpeed = _maxSpeed;
                    //    _velocity = findDir;
                    //    //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                    //    find_pos = _pos + findDir * (_radius + 0.3f);

                    //    if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                    //    {
                    //        DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                    //        __findNum = Misc.RandInt(1, 4);
                    //        curSpeed = 0f;
                    //        _velocity = ori_velo;
                    //    }
                    //}
                    //else
                    //{
                    //    __findNum = Misc.RandInt(1, 4);
                    //    curSpeed = 0;
                    //}

                    //if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                    //{
                    //    DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                    //    __findNum = Misc.RandInt(1, 4);
                    //    curSpeed = 0f;
                    //    _velocity = ori_velo;
                    //}

                    //--------------------
                    //* 구트리로 객체이동 조절
                    Vector3 find_pos = _pos + _velocity.normalized * (_radius + 0.3f);
                    Vector3 ori_velo = _velocity;
                    BaseEntity findEnty_1 = ObjectManager.Inst.RangeTest(this, pos_1, 0, _radius);
                    BaseEntity findEnty_2 = ObjectManager.Inst.RangeTest(this, pos_2, 0, _radius);

                    //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.white);
                    if (null == findEnty_1)
                    {
                        curSpeed = _maxSpeed;

                    }
                    else if (null == findEnty_2)
                    {
                        curSpeed = _maxSpeed;
                        _velocity = findDir;
                        //_rotation = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                        //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                        find_pos = _pos + findDir * (_radius + 0.3f);

                    }
                    else
                    {
                        //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                        //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.cyan);
                        __findNum = Misc.RandInt(1, 4);
                        //if(_withstand <= findEnty_2._withstand)
                        curSpeed = 0;
                    }


                    if (5 == _id)
                    {
                        curSpeed = _maxSpeed;
                        _velocity = ori_velo;
                        //_withstand = 1.2f;
                    }


                    //Vector3 calcPos;
                    //if (null != GridManager.Inst.Find_FirstStructTile4(_pos, find_pos, 0.1f, out calcPos))
                    //
                    //        DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                    //        __findNum = Misc.RandInt(1, 4);
                    //        curSpeed = 0f;
                    //        _velocity = ori_velo;
                    //}
                    if (false == GridManager.Inst.IsVisibleTile(_pos, find_pos, 10))
                    {
                        //DebugWide.AddDrawQ_Circle(find_pos, 0.2f, Color.gray);
                        __findNum = Misc.RandInt(1, 4);
                        curSpeed = 0f;
                        _velocity = ori_velo;
                    }

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
                //Vector3 pos_future = _pos + _velocity * deltaTime; //미래위치 계산 
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

            //-----------
            //_weight 값을 작은값(1) 으로 설정하면 회전효과가 생긴다 
            //_velocity = _velocity.normalized * _maxSpeed; //최대값 설정 
            //Vector3 DesiredVelocity = (_worldOffsetPos - _pos).normalized * _maxSpeed;
            //SteeringForce = (DesiredVelocity - _velocity) * _weight;
            //-----------

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);

            Vector3 acceleration = SteeringForce / _mass;


            //----------
            //f = m * a , a = v / t , f = m * (v / t) , f = (m * v) / t , f * t = (m * v)
            //t = (m * v) / f
            //a * t = v , t = v / a

            //v = s / t , a * t = s / t , a * t2 = s , t2 = s / a 

            //float t1_a = (_mass * _maxSpeed) / SteeringForce.magnitude;
            //float t2_a = _maxSpeed / acceleration.magnitude;
            //float t_v = t1_a * t1_a;
            //DebugWide.LogBlue("time : "+t1_a + "  " + t2_a + "  " + t_v);
            //----------


            _velocity *= _Friction; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 

            //-----------
            //DebugWide.AddDrawQ_Line(_pos, _pos + _velocity, Color.yellow); //가속이 적용되기전의 속도 출력 
            //DebugWide.AddDrawQ_Line(_pos, _pos + SteeringForce, Color.black);
            //DebugWide.AddDrawQ_Line(_pos, _pos + (_velocity + acceleration), Color.blue);
            DebugWide.LogBlue(_velocity.magnitude + "  " + acceleration.magnitude + "  " + acceleration.magnitude * deltaTime);
            //-----------


            //Vector3 desiredVelocity = _velocity + acceleration;
            //if (desiredVelocity.sqrMagnitude > 0.001f)
            {

                //SteeringForce는 조종힘의 의도로 사용되어야 한다. Arrive 는 Seek 과 달리 거리(최대속도)에 따라 힘의 길이가 보다 짧게 반환된다
                //_velocity + acceleration(SteeringForce) 는 조종힘이 정확히 목표방향을 가리키지 않는데 오해하고 가리킨다고 착각하고 사용하고 있었음
                //_worldOffsetPos - _pos 를 사용하여 정확하게 각도를 구해야 떠는 문제가 해결된다 
                //float def = VOp.Sign_ZX(_heading, _velocity + acceleration);
                //float max_angle = Geo.AngleSigned_AxisY(_heading, _velocity + acceleration);
                float def = VOp.Sign_ZX(_heading, _worldOffsetPos - _pos);
                float max_angle = Geo.AngleSigned_AxisY(_heading, _worldOffsetPos - _pos);
                float angle = _anglePerSecond * def * deltaTime;



                //떠는 문제 수정
                //_anglePerSecond 값이 너무 크면 빙빙돌면서 튀는것 처럼 보인다. 최대 회전각을 벗어나지 않도록 예외처리 한다 
                if (Math.Abs(angle) > Math.Abs(max_angle))
                //if (3 > Math.Abs(max_angle))
                {
                    //DebugWide.LogRed(max_angle + "  " + angle);
                    angle = max_angle;
                    //DebugWide.LogRed("1---------------------------------------------");
                }

                //끊어지는듯 떨리는 문제가 있음
                //최대회전량을 벗어나는 양이 계산되는 것을 막는다 
                //if (Math.Abs(angle) > Math.Abs(max_angle))
                if (2 > Math.Abs(max_angle))
                //if(true)
                //if((_worldOffsetPos - _pos).sqrMagnitude < 1)
                //float rangle = Geo.AngleSigned_AxisY(_before_worldOffsetPos - _worldOffsetPos, _pos - _worldOffsetPos);
                //DebugWide.LogBlue(rangle);
                //if ( Math.Abs(rangle) > 90+30)
                //if (Vector3.Dot(_before_worldOffsetPos- _worldOffsetPos, _pos - _worldOffsetPos) < 0)
                {
                    _velocity += acceleration * deltaTime; //안미끄러짐 
                    //_heading = VOp.Normalize(_velocity); //떠는 문제 발생
                    _heading = VOp.Normalize(_worldOffsetPos - _pos);
                    //_heading = Quaternion.AngleAxis(max_angle, ConstV.v3_up) * _heading;
                    //_heading = VOp.Normalize(_heading);

                    DebugWide.LogRed("2---------------------------------------------");
                    //DebugWide.LogRed(max_angle + "   " + angle + "   " + def + "  " + _heading + "   " + _velocity + "  " + acceleration);

                }
                else
                {

                    _heading = Quaternion.AngleAxis(angle, ConstV.v3_up) * _heading;
                    _heading = VOp.Normalize(_heading);
                    //_velocity += _heading * (_velocity + acceleration).magnitude * deltaTime; //실제 미끄러지게 하는 처리 
                    _velocity += _heading * (acceleration).magnitude * deltaTime; //실제 미끄러지게 하는 처리 , _velocity 를 잘못더한것 같음 


                    //DebugWide.LogBlue(max_angle + "   " + angle + "   " + def + "  " + _heading + "   " + _velocity + "  " + acceleration);
                }


                _velocity = VOp.Truncate(_velocity, _maxSpeed);
                _speed = _velocity.magnitude;
                _rotation = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

                //----------------
                Vector3 ToOffset = _worldOffsetPos - _pos;
                //Vector3 pos_future = _pos + _heading * _maxSpeed * deltaTime; //미래위치 계산 
                Vector3 pos_future = _pos + _velocity * deltaTime; //미래위치 계산 
                Vector3 ToFuture = _worldOffsetPos - pos_future;

                //_velocity = _velocity.normalized * _maxSpeed; //속도를 직접 조작하면 비정상 결과가 나온다 

                //if (ToOffset.sqrMagnitude >= ToFuture.sqrMagnitude)
                SetPos(pos_future);
                //else
                //{
                //    DebugWide.LogRed("---------------------------------------------");
                //    float distSpeed = (_worldOffsetPos - _pos).magnitude;
                //    //SetPos(_worldOffsetPos); //목표오프셋 위치로 설정  - 순간이동 버그가 있어 제거 
                //    SetPos(_pos + _velocity.normalized * distSpeed * deltaTime); //거리를 속도로 사용  
                //    //SetPos(_pos + ToOffset.normalized * distSpeed * deltaTime); //거리를 속도로 사용  
                //    //_velocity = Vector3.zero;
                //    //DebugWide.AddDrawQ_Line(_pos, _pos + ToOffset, Color.blue);
                //}
                //----------------

                //가속도 이동 
                //SetPos(_pos + _velocity * deltaTime);


                //8방향으로만 이동 
                //Vector3 n = Misc.GetDir8_Normal3D(_heading); 
                //SetPos(_pos + n * _speed * deltaTime);

                //----------
                //_velocity *= _Friction; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 
                //-----------

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
            DebugWide.DrawLine(_pos, _pos + _heading, Color.white);

            //if (SteeringBehavior.eType.wander == _mode)
            //{
            //    _steeringBehavior.DrawWander();
            //}


            if (null != _leader)
            {
                Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
                DebugWide.DrawCircle(WorldOffsetPos, 0.1f, Color.green);
            }

        }


    }//end class

}//end namespace



