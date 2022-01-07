using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_4
{

    public class BaseEntity : SphereModel.IUserData
    {
        public int _id = -1;

        public float _radius = 0.5f;
        public float _radius_damage = 0.5f; //타격크기 
        public Vector3 _oldPos = Vector3.zero;
        public Vector3 _pos = Vector3.zero;
        public Vector3 _velocity = Vector3.zero;
        public float _speed = 10f;
        public float _withstand = 1f; //버티기
        public Quaternion _rotation = Quaternion.identity;

        //==================================================
        public CellSpace _cur_cell = null;
        public BaseEntity _prev_sibling = null;
        public BaseEntity _next_sibling = null;

        //==================================================
        // 충돌 모델 
        //==================================================
        public SweepPrune.CollisionObject _collision = new SweepPrune.CollisionObject();

        public SphereModel _sphereModel = null; //구트리
    }

    public class OrderPoint : BaseEntity
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

        public Vector3 _target = ConstV.v3_zero;

        public Vector3 _before_target = Vector3.zero;
        public bool _changeTarget = false;

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

    public class Unit : BaseEntity
    {
        //명령형식 , 숫자가 클수록 명령우선순위가 높다  
        //public enum eOrder
        //{
        //    None = 0,
        //    Solo = 1, //혼자 
        //    Squard = 2, //분대
        //    Platoon = 3, //소대
        //};

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

        public bool _tag = false; //*** 제거대상 

        public Vector3 _worldOffsetPos = ConstV.v3_zero;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();


        //--------------------------------------------------
        //public eOrder _eOrder = eOrder.None;
        public Formation _formation = new Formation();
        public Disposition _disposition = new Disposition();
        public Platoon _platoon = null; //소속소대정보 
        public Squard _squard = null; //소속분대정보  

        public StateMachine<Unit> _stateMachine = null;

        //--------------------------------------------------

        public void Reset()
        {
            _velocity = new Vector3(0, 0, 0);
            _heading = Vector3.forward;
            _facing = Vector3.forward;
            _rotation = Quaternion.identity;

            _steeringBehavior.AllOff();
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

            //==============================================

            _stateMachine = new StateMachine<Unit>(this);
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


        //public List<Vector3> _feelers = new List<Vector3>();
        //public void CreateFeelers()
        //{
        //    const float Feeler_Length = 1;
        //    Vector3 fpos = Vector3.forward * (_radius + Feeler_Length);
        //    _feelers.Add(fpos);

        //    fpos = Quaternion.AngleAxis(45, Vector3.up) * Vector3.forward * (_radius + Feeler_Length);
        //    _feelers.Add(fpos);

        //    fpos = Quaternion.AngleAxis(-45, Vector3.up) * Vector3.forward * (_radius + Feeler_Length);
        //    _feelers.Add(fpos);
        //}


        //객체하나에 대한 전체객체 접촉정보를 처리하는 방식, 중복된 접촉정보 있음, 계산후 겹치지 않음 
        public void EnforceNonPenetrationConstraint(Unit src, List<Unit> ContainerOfEntities, float src_withstand, float dst_withstand)
        {
            Unit dst = null;
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
            //_worldOffsetPos = _target;
            //if (null != _leader)
            //{
            //    _worldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace 

            //    //---------
            //    if (_leader._changeTarget)
            //    {
            //        _before_worldOffsetPos = _pos;
            //    }
            //    //DebugWide.AddDrawQ_Circle(_before_worldOffsetPos, 0.5f, Color.green);
            //    //---------
            //}


            Update_NormalMovement(deltaTime);
            //Update_RotateMovement(deltaTime);

            EnforceNonPenetrationConstraint(this, EntityMgr.list, 1, 1); //겹침이 적게 일어나는 방식 

            //bool a;
            //SetPos(GridManager.Inst.Collision_StructLine_Test3(_oldPos, _pos, _radius, out a));
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

            Vector3 SteeringForce = _steeringBehavior._steeringForce;

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

            Vector3 SteeringForce = _steeringBehavior._steeringForce;

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


            //if (null != _leader)
            //{
            //    Vector3 WorldOffsetPos = (_leader._rotation * _offset) + _leader._pos; //PointToWorldSpace
            //    DebugWide.DrawCircle(WorldOffsetPos, 0.1f, Color.green);
            //}

        }


    }//end class

}//end namespace



