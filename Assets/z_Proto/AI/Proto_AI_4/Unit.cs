using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_4
{

    public class BaseEntity : SphereModel.IUserData
    {
        public int _id = -1;

        public float _mass;
        public Vector3 _oldPos = Vector3.zero;
        public Vector3 _pos; 
        public Vector3 _velocity;
        public Vector3 _linearAcceleration;
        public Vector3 _forces;
        //public Vector3 _steeringForce; //_steeringBehavior._steeringForce 이용하기 

        public float _elasticity = 1; //탄성력  
        public float _damping = 1; //제동

        public float _endurance_max = 100; //최대 지구력
        public float _endurance = 30; //현재 지구력 
        public float _endurance_recovery = 100; //지구력 회복량
        public float _rest_start; //휴식시작 지구력 , 휴식상태가 활성
        public float _rest_end; //휴식끝 지구력 , 범위를 넘으면 휴식상태 비활성 

        public bool _isRest = false; //휴식상태 , 지구력이 감소하지 않고 힘이 적용 안된다  
        public bool _isImpluse = true; //단발성 충격힘 적용
        public bool _isStatic = false; //충돌에 반응하지 않게 설정(상대 객체는 충돌반응함) 
        public bool _isCollision = true; //충돌처리를 안하게 설정(상대 객체도 충돌반응 안함) 


        public float _radius_geo = 0.5f; //지형과의 충돌원크기
        public float _radius_body = 1f; //다른객체와의 충돌원크기  

        public float _speed = 10f;
        public float _withstand = 1f; //버티기 - 
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
            _collision._bounds_min.x = newPos.x - _radius_body;
            _collision._bounds_min.z = newPos.z - _radius_body;
            _collision._bounds_max.x = newPos.x + _radius_body;
            _collision._bounds_max.z = newPos.z + _radius_body;
            //==============================================

            GridManager.Inst.AttachCellSpace(_pos, this);

        }
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

        public Vector3 _targetPos = ConstV.v3_zero;

        public Vector3 _before_targetPos = Vector3.zero;
        public bool _changeTarget = false;

        virtual public void Update(float deltaTime)
        {
            _changeTarget = false;
            if (false == Misc.IsZero(_before_targetPos - _targetPos))
            {
                _changeTarget = true;
            }
            _before_targetPos = _targetPos;


            //도착시 종료 
            if ((_targetPos - _pos).sqrMagnitude < 1)
            {
                //특정거리 안에서 이동없이 방향전환 
                _rotation = Quaternion.FromToRotation(Vector3.forward, _targetPos - _pos);
                _velocity = Vector3.zero; //초기화를 안해주면 키이동시 경로가 이상해지는 문제가 발생함
                return;
            }

            //DebugWide.LogBlue("sfsdf: " + _pos );
            Vector3 n = VOp.Normalize(_targetPos - _pos);
            _velocity = n * _speed;
            _pos += _velocity * deltaTime;

            _rotation = Quaternion.FromToRotation(Vector3.forward, _velocity);
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(_targetPos, 0.1f, color);
            DebugWide.DrawLine(_pos, _targetPos, color);
        }

    }

    public struct Flocking
    {
        public float follow_distance; //따라가기 거리 
        public float separation_distance; //분리 거리 
    }

    //시야 
    public struct Sight
    {
        public Unit eye;
        public Vector3 localPos_arc_in;
        public Vector3 localPos_sph_in;
        public Vector3 localPos_sph_notIn;
        public Vector3 localPos_sph_in_around;

        //view
        public Geo.Arc arc_in;
        public Geo.Sphere sph_in;
        public Geo.Sphere sph_notIn;

        public List<Unit> list_view; //시야목록 
        public Unit near_unit; //시야목록중 worldPos_parent 와 가장가까운 객체
        public Unit far_unit;

        //around
        public Geo.Sphere sph_in_around;
        public List<Unit> list_around; //주변객체목룍 

        public void Init(Unit eyeUnit, float arc_angle, float sph_rad_in, float sph_rad_notIn, float sph_around)
        {
            eye = eyeUnit;
            localPos_arc_in = ConstV.v3_zero;
            localPos_sph_in = ConstV.v3_zero;
            localPos_sph_notIn = ConstV.v3_zero;
            localPos_sph_in_around = ConstV.v3_zero;

            arc_in = new Geo.Arc();
            arc_in.Init(ConstV.v3_zero, arc_angle, ConstV.v3_forward);
            sph_notIn = new Geo.Sphere(ConstV.v3_zero, sph_rad_notIn);
            sph_in = new Geo.Sphere(ConstV.v3_zero, sph_rad_in);
            sph_in_around = new Geo.Sphere(ConstV.v3_zero, sph_around);
            list_view = new List<Unit>();
            list_around = new List<Unit>();
            near_unit = null;
            far_unit = null;
        }

        public void Calc_LocalToWorldPos()
        {
            //arc_in.origin = (rot * localPos_arc_in) + pos_parent;
            //sph_in.origin = (rot * localPos_sph_in) + pos_parent;
            //sph_notIn.origin = (rot * localPos_sph_notIn) + pos_parent;

            arc_in.origin = (eye._rotation * localPos_arc_in) + eye._pos;
            sph_in.origin = (eye._rotation * localPos_sph_in) + eye._pos;
            sph_notIn.origin = (eye._rotation * localPos_sph_notIn) + eye._pos;

            sph_in_around.origin = (eye._rotation * localPos_sph_in_around) + eye._pos;
        }

        public void Draw(Color color)
        {
            arc_in.Draw(color, sph_in.radius);
            arc_in.Draw(color, sph_notIn.radius);

            sph_in_around.Draw(Color.gray);

            foreach (Unit u in list_view )
            {
                DebugWide.DrawCircle(u._pos, 0.1f, Color.green);
            }

            if (null != near_unit && null != eye)
            {
                DebugWide.DrawLine(near_unit._pos, eye._pos, Color.green);
                DebugWide.DrawCircle(near_unit._pos, 0.3f, Color.green);
            }

        }
    }

    public class Unit : BaseEntity
    {

        //public Vector3 _velocity = new Vector3(0, 0, 0); //실제 향하고 있는 방향
        public Vector3 _heading = Vector3.forward; //객체의 방향
        public Vector3 _facing = Vector3.forward; //얼굴의 방향


        public float _maxSpeed = 10f;
        public float _maxForce = 40f;

        public float _anglePerSecond = 180;

        public bool _isNonpenetration = true; //비침투 

        public bool _tag = false; //*** 제거대상 

        public Vector3 _targetPos = ConstV.v3_zero;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();


        //--------------------------------------------------

        public Disposition _disposition = new Disposition(); //배치정보 

        public StateMachine<Unit> _stateMachine = null;

        //--------------------------------------------------

        public Sight _sight = new Sight(); //시야 
        public Flocking _flocking = new Flocking(); //무리짓기 정보 

        //--------------------------------------------------

        public void Reset()
        {
            _velocity = new Vector3(0, 0, 0);
            _heading = Vector3.forward;
            _facing = Vector3.forward;
            _rotation = Quaternion.identity;

            _steeringBehavior.AllOff();
        }

        public void Init(int id, float radius_body, Vector3 pos)
        {
            _id = id;

            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;
            _radius_body = radius_body;

            //==============================================
            //_collision._list_idx = _id;
            //_collision._radius = radius;

            //==============================================
            ////구트리 등록 
            ObjectManager.Inst.AddSphereTree(this);
            //==============================================

            SetPos(pos);
            _oldPos = pos;
            _targetPos = pos;

            //==============================================

            _stateMachine = new StateMachine<Unit>(this);
            _stateMachine.Init(State_Move_Unit.inst, StateGlobal_Unit.inst);

            //==============================================

            _sight.Init(this, 90, 5, 0, 4);
        }

        //public void SetPos(Vector3 newPos)
        //{
        //    _pos = newPos;

        //    //==============================================
        //    //!!!!! 구트리 위치 갱신 
        //    if (null != _sphereModel)
        //    {
        //        _sphereModel.NewPos(_pos);
        //    }

        //    //==============================================

        //    //!!!!! 경계상자 위치 갱신
        //    _collision._bounds_min.x = newPos.x - _radius_body;
        //    _collision._bounds_min.z = newPos.z - _radius_body;
        //    _collision._bounds_max.x = newPos.x + _radius_body;
        //    _collision._bounds_max.z = newPos.z + _radius_body;
        //    //==============================================

        //    GridManager.Inst.AttachCellSpace(_pos, this);

        //}

        public void SetRadius(float new_radius_body)
        {
            _radius_body = new_radius_body;

            //==============================================
            //!!!!! 구트리 갱신 
            if (null != _sphereModel)
            {

                _sphereModel.NewRadius(new_radius_body);
                //_sphereModel.NewPosRadius(_pos, new_radius_body);
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
        //public void EnforceNonPenetrationConstraint(Unit src, List<Unit> ContainerOfEntities, float src_withstand, float dst_withstand)
        //{

        //    Unit dst = null;
        //    for (int i = 0; i < ContainerOfEntities.Count; i++)
        //    {
        //        dst = ContainerOfEntities[i];
        //        if (src == dst) continue;

        //        Vector3 dir_dstTOsrc = src._pos - dst._pos;
        //        Vector3 n = ConstV.v3_zero;
        //        float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;
        //        float r_sum = (src._radius_body + dst._radius_body);
        //        float sqr_r_sum = r_sum * r_sum;

        //        //1.두 캐릭터가 겹친상태 
        //        if (sqr_dstTOsrc < sqr_r_sum)
        //        {

        //            //==========================================
        //            float rate_src, rate_dst;
        //            float f_sum = src_withstand + dst_withstand;
        //            if (Misc.IsZero(f_sum)) rate_src = rate_dst = 0.5f;
        //            else
        //            {
        //                rate_src = 1f - (src_withstand / f_sum);
        //                rate_dst = 1f - rate_src;
        //            }

        //            n = VOp.Normalize(dir_dstTOsrc);

        //            float len_dstTOsrc = (float)Math.Sqrt(sqr_dstTOsrc);
        //            float len_bitween = (r_sum - len_dstTOsrc);
        //            float len_bt_src = len_bitween * rate_src;
        //            float len_bt_dst = len_bitween * rate_dst;

        //            //DebugWide.LogBlue(len_dstTOsrc + "  " + n + "  " + _id + "  " );
                    

        //            //2.완전겹친상태 
        //            if (float.Epsilon >= len_dstTOsrc)
        //            {
        //                n = Misc.GetDir8_Random_AxisY();
        //                len_dstTOsrc = 1f;
        //                len_bt_src = r_sum * 0.5f;
        //                len_bt_dst = r_sum * 0.5f;
        //            }

        //            //src._oldPos = src._pos;
        //            //dst._oldPos = dst._pos;

        //            src._pos += n * len_bt_src;
        //            dst._pos += -n * len_bt_dst;

        //            //test
        //            //bool a;
        //            //src.SetPos(GridManager.Inst.Collision_StructLine_Test3(src._oldPos, src._pos, src._radius, out a));
        //            //dst.SetPos(GridManager.Inst.Collision_StructLine_Test3(dst._oldPos, dst._pos, dst._radius, out a));
        //        }
        //    }

        //}

        Vector3 _before_targetPos = Vector3.zero;
        public void Update(float deltaTime)
        {

            _stateMachine.Update();

            //----------------------------------------------

            _oldPos = _pos;
            //_targetPos = _steeringBehavior._targetPos; //Arrive2 에서의 목표위치 
            if (null != _disposition._belong_formation)
            {
                //OffsetPursuit 에서의 목표위치 
                //_targetPos = (_disposition._belong_formation._rotation * _disposition._offset) + _disposition._belong_formation._pos; //PointToWorldSpace 
                if (_steeringBehavior.IsOffsetPursuitOn())
                {
                    _targetPos = _disposition._belong_formation._targetPos;
                }

                //---------
                if (_disposition._belong_formation._changeTarget)
                {
                    _before_targetPos = _pos;
                }
                //DebugWide.AddDrawQ_Circle(_before_targetPos, 0.5f, Color.green);
                //---------
            }

            //DebugWide.LogGreen(_targetPos + "  " + _squard._pos + "  " + _steeringBehavior._targetPos);

            Update_NormalMovement(deltaTime);
            //Update_RotateMovement(deltaTime);

            //초기 이동 갱신이후 [src 1 , dst 0] 일때 선형적으로 밀려 배치되는 문제 발생 
            //접촉정보를 한꺼번에 모아 처리하지 않으면 생기는 문제이다. 해당 알고리즘으로는 해결이 안된다  
            //if(_isNonpenetration)
            //EnforceNonPenetrationConstraint(this, EntityMgr.list, 0, 1); //겹침이 적게 일어나는 방식 

            //bool a;
            //SetPos(GridManager.Inst.Collision_StructLine_Test3(_oldPos, _pos, _radius, out a));

            //시야 임시적용 , 유닛방향을 시야방향으로 삼는다 
            //_sight.arc.SetRotateY(_rotation);  
            _sight.arc_in.SetDir(_heading);

            //새로운 시야정보로 갱신한다. 
            //fixme 0.3초 간격으로 갱신하도록 변경하기 
            _sight.Calc_LocalToWorldPos();
            ObjectManager.Inst._sphereTree.SightTest(ref _sight);

        }


        //구트리를 이용한 멈추기 및 돌아가기
        int __findNum = 0;
        public float Action_SphereTree()
        {
            float curSpeed = _maxSpeed;
            //if(false)
            {
                //float[] ay_angle = new float[] { 0, 45f, -45f, 90f, -90, 135f, -135, 180f };
                float[] ay_angle = new float[] { 0, 45f, -45f, 60f, -60, 135f, -135, 180f };
                Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * _velocity.normalized;
                //float sum_r = _radius + _radius;
                Vector3 pos_1 = _pos + _velocity.normalized * (_radius_body);
                Vector3 pos_2 = _pos + findDir * (_radius_body);

                //--------------------
                //* 구트리로 객체이동 조절
                Vector3 find_pos = _pos + _velocity.normalized * (_radius_body + 0.3f);
                Vector3 ori_velo = _velocity;
                BaseEntity findEnty_1 = ObjectManager.Inst.RangeTest(this, pos_1, 0, _radius_body);
                BaseEntity findEnty_2 = ObjectManager.Inst.RangeTest(this, pos_2, 0, _radius_body);

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
                    find_pos = _pos + findDir * (_radius_body + 0.3f);

                }
                else
                {
                    //DebugWide.AddDrawQ_Circle(pos_2, 0.1f, Color.red);
                    //DebugWide.AddDrawQ_Circle(pos_1, 0.1f, Color.cyan);
                    __findNum = Misc.RandInt(1, 4);
                    //if(_withstand <= findEnty_2._withstand)
                    curSpeed = 0;
                }


                //if (5 == _id)
                //{
                //    curSpeed = _maxSpeed;
                //    _velocity = ori_velo;
                //    //_withstand = 1.2f;
                //}


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

        //공간분할을 이용한 멈추기 및 돌아가기 
        public void Action_CellSpace()
        {
            float curSpeed = _maxSpeed;
            //if(false)
            {
                //float[] ay_angle = new float[] { 0, 45f, -45f, 90f, -90, 135f, -135, 180f };
                float[] ay_angle = new float[] { 0, 45f, -45f, 60f, -60, 135f, -135, 180f };
                Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * _velocity.normalized;
                float sum_r = _radius_body + _radius_body;
                Vector3 pos_1 = _pos + _velocity.normalized * (_radius_body);
                Vector3 pos_2 = _pos + findDir * (_radius_body);

                //-----------------
                //* 셀공간으로 객체이동 조절
                CellSpace findCell_1 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + _velocity.normalized * sum_r, 5);
                CellSpace findCell_2 = GridManager.Inst.Find_FirstEntityTile(this, _pos, _pos + findDir * sum_r, 5);
                Vector3 find_pos = _pos + _velocity.normalized * (_radius_body + 0.3f);
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
                    find_pos = _pos + findDir * (_radius_body + 0.3f);

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

        public void Intergrate(float deltaTime)
        {
            _endurance += _endurance_recovery * deltaTime; //지구력 초당 회복
            _endurance = (_endurance_max > _endurance) ? _endurance : _endurance_max;

            //if(0 == _id)
            //{
            //    DebugWide.LogBlue("1 >> "+endurance + " recps: " + (endurance_recovery * changeInTime)); 
            //}

            Vector3 steeringForce = _steeringBehavior.Calculate(); //조종힘 계산 
            //if(0 == _id)
                    //DebugWide.LogRed("!!!!! - " + steeringForce);
            Vector3 allForce = _forces + steeringForce;
            allForce = VOp.Truncate(allForce, _maxForce);

            //if(0 == _id)
            //{
            //    DebugWide.LogBlue("1 >> "+ _forces + " sf: " + steeringForce + "  " + allForce.magnitude + "  " + _forces.magnitude); 
            //}

            //-------------------------------------
            float force_scala = allForce.magnitude;
            //float force_scala = _forces.magnitude;
            float force_perSecond = force_scala * deltaTime;
            //-------------------------------------


            if (_endurance <= _rest_start || _endurance < force_perSecond)
            {
                _isRest = true; //휴식활성 
            }
            else if (_rest_end <= _endurance)
            {
                _isRest = false; //휴식해제 
            }

            //_isRest = false; //test
            //if (0 == id)
            //{
            //    DebugWide.LogBlue("2 >> " + endurance + " fcps: " + force_perSecond);
            //}
            //-------------------------------------

            if (true == _isRest)
            {
                _velocity *= (float)Math.Pow(_damping, deltaTime); //초당 damping 비율 만큼 감소시킨다.

            }
            else
            {
                _endurance -= force_perSecond; //질량과 상관없는 힘만 감소시킨다  , 지구력 초당 감소 
                _endurance = (0 < _endurance) ? _endurance : 0;

                // a = F/m
                _linearAcceleration = allForce / _mass;

                //maxSpeed < maxForce 일 경우 *기본계산 처럼 작동 
                //maxSpeed > maxForce 일 경우 회전효과가 생긴다 
                //관성으로 미끄러지는 효과가 생긴다 
                //일정하게 가속하도록 한다. 질량이 1일때 1초당 f = a = v = s 가 된다. f 를 s 로 보고 초당 가속하는 거리를 예측할 수 있다 
                _velocity += _linearAcceleration * deltaTime;

                _velocity *= (float)Math.Pow(_damping, deltaTime); //초당 damping 비율 만큼 감소시킨다.
                                                                   //linearVelocity *= damping; //!! 이렇게 사용하면 프레임에 따라 값의 변화가 일정하지 않게됨 

                //if (0 == _id)
                //{
                //    DebugWide.LogBlue(_velocity.magnitude + "  " + addForce + "  " + _linearAcceleration + "  " + deltaTime);
                //}

            }

            _velocity = VOp.Truncate(_velocity, _maxSpeed);
            //if (0 == _id)
                //DebugWide.LogRed("!!_velocity - " + _velocity);
            //-------------------------------------

            if (_isImpluse)
            {
                _forces = ConstV.v3_zero; //힘 적용후 바로 초기화 - impluse
            }
        }

        //자유이동 , 방향에만 제한을 두는 방식 
        public void Update_NormalMovement(float deltaTime)
        {

            //Vector3 steeringForce = _steeringBehavior.Calculate(); //조종힘 계산 

            Intergrate(deltaTime);

            //=============================================================
            //if(0 == _id)
            //{
            //    DebugWide.LogRed(_targetPos);
            //}


            Vector3 ToOffset = _targetPos - _pos;
            //if(null != _sight.closest)
            //{
            //    ToOffset = _sight.closest._pos - _pos;
            //}

            //if (ToOffset.sqrMagnitude > 0.001f) //잘못된처리 제거 : 속도값이 있음에도 목표위치에 가까우면 처리안하는 것은 잘못이다. - 도착관련 예전처리 제거 

            //최적화를 위해서는 목표와의 거리가 아닌 현재 속도값이 작은지 검사 한다  
            //if (_velocity.sqrMagnitude > 0.0001f) //속도가 없을때도 방향전환 가능해야 하기에 처리제거 
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
                //if (0 == Action_SphereTree()) return;

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

            Vector3 SteeringForce = _steeringBehavior.Calculate();

            Vector3 acceleration = SteeringForce / _mass;

            //Intergrate(deltaTime); //todo - 완성하기 
            //----------------------------------------------------

            Vector3 ToOffset = _targetPos - _pos;
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

                _velocity *= (float)Math.Pow(_damping, deltaTime); //마찰력 적용 

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
            vb0 = _rotation * _array_VB[0] * _radius_body;
            vb1 = _rotation * _array_VB[1] * _radius_body;
            vb2 = _rotation * _array_VB[2] * _radius_body;

            //에이젼트 출력 
            DebugWide.DrawLine(_pos + vb0, _pos + vb1, color);
            DebugWide.DrawLine(_pos + vb1, _pos + vb2, color);
            DebugWide.DrawLine(_pos + vb2, _pos + vb0, color);
            DebugWide.DrawCircle(_pos, _radius_body, color);
            DebugWide.DrawLine(_pos, _pos + _heading, Color.white);

            //지구력 출력 
            //DebugWide.PrintText(_pos, Color.white, _endurance.ToString(".0")); 

            //id출력
            //DebugWide.PrintText(_pos, Color.white, _id.ToString());

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



