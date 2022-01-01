﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_Steering_Avoidance
{
    public class EntityMgr
    {

        public static readonly List<Vehicle> list = new List<Vehicle>();
        public static void Add(Vehicle v)
        {
            list.Add(v);
            v._id = list.Count - 1;
        }
    }
    public class Test_Avoidance : MonoBehaviour
    {
        public Transform _tr_target = null;

        public const int Num_Agents_X = 5;
        public const int Num_Agents_Y = 5;

        public float _maxForce = 200;
        public float _maxSpeed = 20;

        // Use this for initialization
        void Start()
        {

            _tr_target = GameObject.Find("tr_target").transform;

            for (int i = 0; i < Num_Agents_Y; i++)
            {
                for (int j = 0; j < Num_Agents_X; j++)
                {
                    Vehicle v = new Vehicle();
                    v.Init();
                    v._steeringBehavior._WeightObstacleAvoidance = 0;
                    v._steeringBehavior._WeightSeparation = 0;
                    v._steeringBehavior._WeightAlignment = 0;
                    v._steeringBehavior._WeightCohesion = 0;
                    v._steeringBehavior._WeightWander = 0;
                    v._pos = new Vector3(j*6 + 30, 0, i * 6 + 30);
                    EntityMgr.Add(v);
                }

            }

            Vehicle vh = EntityMgr.list[ID_X];
            vh._v_target = vh;
            vh._steeringBehavior._WeightObstacleAvoidance = 2000;
            vh._steeringBehavior._WeightWander = 200; //arrive 가 됨 
            vh._steeringBehavior.ObstacleAvoidanceOn();

        }

        public void ResetAll(float avoid, float sep , float ali , float coh, float wander , bool pos_reset)
        {
            for (int i = 0; i < Num_Agents_Y; i++)
            {
                for (int j = 0; j < Num_Agents_X; j++)
                {
                    //00 01 02 03 04 
                    //10 11 12 13 14
                    int idx = i * Num_Agents_X + j;
                    Vehicle v = EntityMgr.list[idx];
                    v.Reset();
                    v._steeringBehavior._WeightObstacleAvoidance = avoid;
                    v._steeringBehavior._WeightSeparation = sep;
                    v._steeringBehavior._WeightAlignment = ali;
                    v._steeringBehavior._WeightCohesion = coh;
                    v._steeringBehavior._WeightWander = wander;
                    if(pos_reset)
                        v._pos = new Vector3(j * 6 + 30, 0, i * 6 + 30);

                }

            }
        }

        public void TargetOnAll()
        {
            for (int i = 0; i < Num_Agents_Y; i++)
            {
                for (int j = 0; j < Num_Agents_X; j++)
                {
                    //00 01 02 03 04 
                    //10 11 12 13 14
                    int idx = i * Num_Agents_X + j;
                    Vehicle v = EntityMgr.list[idx];
                    v._v_target = v; //임시로 자기자신을 설정 
                    v._target = _tr_target.position; 
                }
            }
        }


        public int ID_X = 0;
        bool _one_ctr = true;
        void Update()
        {

            Vehicle vh = EntityMgr.list[ID_X];
            SteeringBehavior sb = EntityMgr.list[ID_X]._steeringBehavior;
            float sep = sb._WeightSeparation;
            float ali = sb._WeightAlignment;
            float coh = sb._WeightCohesion;
            float wan = sb._WeightWander;
            string mode = sb._calcMode.ToString() + "  isNp[Z]: " + vh._isNonpenetration;
            DebugWide.LogBlue(ID_X+"_ sep[QA]:  "+sep + " ali[WS]: " + ali + "  coh[ED]: " + coh + "  wan[RF]: " + wan + "  [TGB] " + mode + "  one_ctr[XC]: " + _one_ctr);

            vh._v_target = vh;
            vh._target = _tr_target.position; //id_0 목표위치 갱신 

            //-------------------------
            foreach (Vehicle v in EntityMgr.list)
            {
                v._maxSpeed = _maxSpeed;
                v._steeringBehavior._maxForce = _maxForce;
            }
            //-------------------------

            if (Input.GetKeyDown(KeyCode.X))
            {
                _one_ctr = true;
                ResetAll(0, 0, 0, 0, 0, false);
                vh._steeringBehavior._WeightObstacleAvoidance = 2000;
                vh._steeringBehavior._WeightWander = 200; //arrive 가 됨 
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                _one_ctr = false;
                ResetAll(0, 200, 200, 400, 200, true);
            }

            TargetOnAll(); //test

            if (_one_ctr)
            {
                vh.KeyInput();
            }else
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v.KeyInput();
                }
            }

        }


        private void OnDrawGizmos()
        {
            if (null == _tr_target) return;

            Vehicle vh = EntityMgr.list[ID_X];
            SteeringBehavior sb = EntityMgr.list[ID_X]._steeringBehavior;

            foreach (Vehicle v in EntityMgr.list)
            {
                v.Update();
                v.Draw(Color.black);
            }

            //sb.DrawTagNeighbors(vh);
        }
    }

    public class Smoother_Vector3
    {

        //this holds the history
        List<Vector3> m_History;

        int m_iNextUpdateSlot;


        public Smoother_Vector3(int SampleSize)
        {

            m_History = new List<Vector3>(SampleSize);
            for (int i = 0; i < SampleSize; i++)
            {
                m_History.Add(Vector3.zero);
            }

        
            m_iNextUpdateSlot = 0;
        }

        //each time you want to get a new average, feed it the most recent value
        //and this method will return an average over the last SampleSize updates
        public Vector3 Update(Vector3 MostRecentValue)
        {
            if (0 == m_History.Count) return Vector3.zero;

            m_iNextUpdateSlot++;
            m_iNextUpdateSlot %= m_History.Count;

            //overwrite the oldest value with the newest
            m_History[m_iNextUpdateSlot] = MostRecentValue;

            //make sure m_iNextUpdateSlot wraps around. 
            if (m_iNextUpdateSlot >= m_History.Count) m_iNextUpdateSlot = 0;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < m_History.Count; i++)
            {
            
                sum = sum + m_History[i];


            }

            return sum / (float)m_History.Count;

        }
    }


    public class Vehicle
    {
        public int _id = -1;

        public Vector3 _pos = Vector3.zero;

        public Vector3 _velocity = new Vector3(0, 0, 0);

        public Vector3 _heading = Vector3.forward;

        public Smoother_Vector3 _headingSmoother;
        public Vector3 _smoothedHeading;

        //public Vector3 _side;

        public float _mass = 1f;

        public float _speed;

        public float _maxSpeed = 20f;

        //public float _maxForce = 400f;

        //public float _maxTurnRate;

        public Vector3 _size = new Vector3(3, 0, 3);

        public bool _tag = false;
        public float _radius = 3;

        public Quaternion _rotatioin = Quaternion.identity;

        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vehicle _v_target = null;
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();

        public void Reset()
        {
            _velocity = new Vector3(0, 0, 0);
            _heading = Vector3.forward;
            _speed = 0;

            _tag = false;

            _rotatioin = Quaternion.identity;

            _target = ConstV.v3_zero;
            _offset = ConstV.v3_zero;
            _v_target = null;
            _mode = SteeringBehavior.eType.none;
        }

        public void Init()
        {
            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;

            _headingSmoother = new Smoother_Vector3(10);
        }

        public bool _isNonpenetration = false;
        public void Update()
        {

            Vector3 SteeringForce = _steeringBehavior.Calculate();

            //Acceleration = Force/Mass
            Vector3 acceleration = SteeringForce / _mass;
            //DebugWide.LogBlue(acceleration.magnitude);

            //update velocity
            _velocity += acceleration * Time.deltaTime;

            _velocity = VOp.Truncate(_velocity, _maxSpeed);

            Vector3 ToOffset = _target - _pos;
            //if (_velocity.sqrMagnitude > 0.001f)
            if (ToOffset.sqrMagnitude > 0.001f && _velocity.sqrMagnitude > 0.001f)
            {
                _heading = VOp.Normalize(_velocity);
                //_heading = VOp.Normalize(ToOffset);
                _speed = _velocity.magnitude;
                //DebugWide.LogBlue(_speed); 
                //_rotatioin = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

                //튀는 움직임 최소화 - 출력에만 사용한다 
                if (true)
                {
                    _smoothedHeading = _headingSmoother.Update(_heading);
                    _smoothedHeading.Normalize();
                    _rotatioin = Quaternion.FromToRotation(ConstV.v3_forward, _smoothedHeading);
                }

                _pos += _velocity * Time.deltaTime;
            }

            if (_isNonpenetration)
            {
                EnforceNonPenetrationConstraint(this, EntityMgr.list, 0 , 1);
            }


            _pos = WrapAroundXZ(_pos, 100, 100);

        }


        public Vector3 WrapAroundXZ(Vector3 pos, int MaxX, int MaxY)
        {
            if (pos.x > MaxX) { pos.x = 0.0f; }

            if (pos.x < 0) { pos.x = (float)MaxX; }

            if (pos.z < 0) { pos.z = (float)MaxY; }

            if (pos.z > MaxY) { pos.z = 0.0f; }

            return pos;
        }


        //객체하나에 대한 전체객체 접촉정보를 처리하는 방식, 중복된 접촉정보 있음, 계산후 겹치지 않음 
        public void EnforceNonPenetrationConstraint(Vehicle src, List<Vehicle> ContainerOfEntities, float src_withstand, float dst_withstand)
        {
            Vehicle dst = null;
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

                    src._pos += n * len_bt_src;
                    dst._pos += -n * len_bt_dst;

                }
            }

        }

        public void Draw(Color color)
        {
            Vector3 vb0, vb1, vb2;
            vb0 = _rotatioin * _array_VB[0] * _size.z;
            vb1 = _rotatioin * _array_VB[1] * _size.z;
            vb2 = _rotatioin * _array_VB[2] * _size.z;

            //에이젼트 출력 
            DebugWide.DrawLine(_pos + vb0, _pos + vb1, color);
            DebugWide.DrawLine(_pos + vb1, _pos + vb2, color);
            DebugWide.DrawLine(_pos + vb2, _pos + vb0, color);
            //DebugWide.DrawCircle(_pos, 1f * _size.z, color); //test

            //if (SteeringBehavior.eType.wander == _mode)
            //{
            //    _steeringBehavior.DrawWander();
            //}
        }

        public void KeyInput()
        {
            //분리
            if (Input.GetKey(KeyCode.Q))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightSeparation += 10f;
                }
            }
            if (Input.GetKey(KeyCode.A))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightSeparation -= 10f;
                    if (0 > _steeringBehavior._WeightSeparation) _steeringBehavior._WeightSeparation = 0;
                }
            }
            //정렬
            if (Input.GetKey(KeyCode.W))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightAlignment += 10f;
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightAlignment -= 10f;
                    if (0 > _steeringBehavior._WeightAlignment) _steeringBehavior._WeightAlignment = 0;
                }
            }
            //응집 
            if (Input.GetKey(KeyCode.E))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightCohesion += 10f;
                }
            }
            if (Input.GetKey(KeyCode.D))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightCohesion -= 10f;
                    if (0 > _steeringBehavior._WeightCohesion) _steeringBehavior._WeightCohesion = 0;
                }
            }
            //방황 , 찾기
            if (Input.GetKey(KeyCode.R))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightWander += 10f;
                }
            }
            if (Input.GetKey(KeyCode.F))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._WeightWander -= 10f;
                    if (0 > _steeringBehavior._WeightWander) _steeringBehavior._WeightWander = 0;
                }
            }
            //계산모드 
            if (Input.GetKeyDown(KeyCode.T))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._calcMode = SteeringBehavior.eCalcMode.CalculateWeightedSum;
                }
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._calcMode = SteeringBehavior.eCalcMode.CalculatePrioritized;
                }
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _steeringBehavior._calcMode = SteeringBehavior.eCalcMode.CalculateDithered;
                }
            }

            //겹침상태 
            if (Input.GetKeyDown(KeyCode.Z))
            {
                //foreach (Vehicle v in EntityMgr.list)
                {
                    _isNonpenetration = !_isNonpenetration;
                }
            }
        }
    }

    public class SteeringBehavior
    {
        public enum SummingMethod
        {
            weighted_average,
            prioritized,
            dithered
        }


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
            pursuit = 0x00800, //이동위치 예측 추격
            evade = 0x01000, //추격위치 예측 회피
            interpose = 0x02000,
            hide = 0x04000,
            flock = 0x08000,
            offset_pursuit = 0x10000,
        }

        public enum Deceleration { slow = 3, normal = 2, fast = 1 };

        public int _flags;

        public Vehicle _vehicle;

        public Vector3 _steeringForce = Vector3.zero;

        //public Vector3 _target;

        //==================================================
        public void ObstacleAvoidanceOn() { _flags |= (int)eType.obstacle_avoidance; }
        public void ObstacleAvoidanceOff() { if (On(eType.obstacle_avoidance)) _flags ^= (int)eType.obstacle_avoidance; }

        public bool On(eType bt) { return (_flags & (int)bt) == (int)bt; }

        public bool AccumulateForce(ref Vector3 RunningTot, Vector3 ForceToAdd)
        {
            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = RunningTot.magnitude;

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = _maxForce - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (MagnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = ForceToAdd.magnitude;

            //if the magnitude of the sum of ForceToAdd and the running total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector is
            //possible without going over the max.
            if (MagnitudeToAdd < MagnitudeRemaining)
            {
                RunningTot += ForceToAdd;
            }

            else
            {
                //add it to the steering force
                RunningTot += (ForceToAdd.normalized * MagnitudeRemaining);
            }

            return true;

        }

        public void DrawTagNeighbors(Vehicle entity)
        {
            DebugWide.DrawCircle(entity._pos, _ViewDistance, Color.gray);
            Vehicle curEntity;
            for (int i = 0; i < EntityMgr.list.Count; i++)
            {
                curEntity = EntityMgr.list[i];

                Vector3 to = curEntity._pos - entity._pos;

                float range = _ViewDistance + curEntity._radius;

                if ((curEntity != entity) && (to.sqrMagnitude < range * range))
                {
                    DebugWide.DrawLine(entity._pos, curEntity._pos, Color.gray);
                }
            }
        }

        public void TagNeighbors(Vehicle entity, List<Vehicle> ContainerOfEntities, float radius)
        {
            //iterate through all entities checking for range
            Vehicle curEntity;
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //first clear any current tag
                curEntity._tag = false;

                Vector3 to = curEntity._pos - entity._pos;

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + curEntity._radius;

                //if entity within range, tag for further consideration. (working in
                //distance-squared space to avoid sqrts)
                if ((curEntity != entity) && (to.sqrMagnitude < range * range))
                {
                    curEntity._tag = true;
                }

            }//next entity
        }

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

            if (dist > 0) //0으로 나누는 것에 대한 예외처리 , 기존 최소값 지정으로 인해 떠는 문제 있었음 
            {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration..
                const float DecelerationTweaker = 0.3f;

                //speed = dist / 1 
                //1초에 움직인 거리의 속도라고 볼때 speed = dist 이다. 즉 속도가 거리이다
                // _maxSpeed 가 거리라고 생각하면 _velocity 도 거리로 볼 수 있게 된다
                //전체거리 : ----------> 10  
                //이동거리 : -----> 5
                //*-* _velocity 최대속도 5에 도달하는 경우
                //전체거리가 10 , 최대속도 5 , t=1   감속시작되는 거리 : 5
                //전체거리가 10 , 최대속도 5 , t=0.5 감속시작되는 거리 : 2.5
                //전체거리가 10 , 최대속도 5 , t=0.2 감속시작되는 거리 : 1
                //*-* _velocity 최대속도 5에 도달하지 못하는 경우
                //_velocity < 이동거리 : 가속
                //_velocity > 이동거리 : 감속 

                //deceleration 가 작을수록 속도가 크게 계산된다 
                float speed = dist / ((float)deceleration * DecelerationTweaker); //v = s / t
                //speed = dist * 10; //decelerationTime = 10

                //make sure the velocity does not exceed the max
                speed = Math.Min(speed, _vehicle._maxSpeed);

                //speed = dist / 1 일떄 , speed = dist 가 된다 
                //dist >= maxSpeed 일때 speed 가 최대값이 된다 

                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                Vector3 DesiredVelocity = (ToTarget * speed) / dist; //toTarget / dist = 정규화벡터 , == speed * (toTarget / dist)
                //DebugWide.LogBlue(dist + "  " + speed + "  " + DesiredVelocity.magnitude);

                //DesiredVelocity.magnitude == speed 
                //_vehicle._velocity 가 DesiredVelocity 에 근접해지는 알고리즘이다 
                //DesiredVelocity > _vehicle._velocity 일때는 가속
                //------------> : DesiredVelocity
                //<-----        : _velocity
                //      ------> : 가속
                //DesiredVelocity < _vehicle._velocity 일때는 감속 
                //------------>      : DesiredVelocity
                //<----------------- : _velocity
                //            <----- : 감속
                //DesiredVelocity == _vehicle._velocity 일때는 등속도 
                //------------> : DesiredVelocity
                //<------------ : _velocity
                //            0 : 등속도
                return (DesiredVelocity - _vehicle._velocity);
            }

            return Vector3.zero;
        }

        public Vector3 Pursuit(Vehicle evader)
        {
            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector3 ToEvader = evader._pos - _vehicle._pos;

            float RelativeHeading = Vector2.Dot(_vehicle._heading, evader._heading);


            if ((Vector3.Dot(ToEvader, _vehicle._heading) > 0f) &&
                 (RelativeHeading < -0.95))  //acos(0.95)=18 degs
            {

                //DebugWide.DrawCircle(evader._pos, 2, Color.red);

                return Seek(evader._pos);
            }

            //Not considered ahead so we predict where the evader will be.

            //the lookahead time is propotional to the distance between the evader
            //and the pursuer; and is inversely proportional to the sum of the
            //agent's velocities
            float LookAheadTime = ToEvader.magnitude /
                                  (_vehicle._maxSpeed + evader._speed);

            //LookAheadTime += TurnaroundTime(_vehicle, evader._pos , 1); //이렇게 턴시간 늘리는 것은 아닌것 같음 

            //------------------------
            Vector3 prPos = evader._pos + evader._velocity * LookAheadTime;
            DebugWide.DrawCircle(prPos, 2, Color.red);
            //DebugWide.LogBlue(LookAheadTime);
            //------------------------

            //now seek to the predicted future position of the evader
            return Seek(evader._pos + evader._velocity * LookAheadTime);
        }

        public float TurnaroundTime(Vehicle agent, Vector3 targetPos, float turnSecond)
        {
            Vector3 toTarget = (targetPos - agent._pos).normalized;

            float dot = Vector3.Dot(agent._heading, toTarget);

            float coefficient = 0.5f * turnSecond; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 1초
            //const float coefficient = 0.5f * 5; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 5초

            return (dot - 1f) * -coefficient; //[-2 ~ 0] * -coefficient
        }


        //explained above
        float _wanderJitter = 80f;
        float _wanderRadius = 1.2f;
        float _wanderDistance = 2.0f;
        Vector3 _vWanderTarget = Vector3.zero;
        public Vector3 Wander()
        {
            //this behavior is dependent on the update rate, so this line must
            //be included when using time independent framerate.
            float JitterThisTimeSlice = _wanderJitter * Time.deltaTime;

            //first, add a small random vector to the target's position
            _vWanderTarget += new Vector3(Misc.RandomClamped() * JitterThisTimeSlice, 0,
                                          Misc.RandomClamped() * JitterThisTimeSlice);

            //reproject this new vector back on to a unit circle
            _vWanderTarget.Normalize();

            //increase the length of the vector to the same as the radius
            //of the wander circle
            _vWanderTarget *= _wanderRadius;

            //move the target into a position WanderDist in front of the agent
            Vector3 targetLocal = _vWanderTarget + new Vector3(0, 0, _wanderDistance);

            //project the target into world space
            Vector3 targetWorld = (_vehicle._rotatioin * targetLocal) + _vehicle._pos; //PointToWorldSpace

            //DebugWide.DrawCircle(_vehicle._pos + _vehicle._heading * _wanderDistance, _wanderRadius, Color.green);
            //DebugWide.DrawLine(_vehicle._pos, targetWorld, Color.green);
            //and steer towards it
            return targetWorld - _vehicle._pos;

        }

        public void DrawWander()
        {
            Vector3 targetLocal = _vWanderTarget + new Vector3(0, 0, _wanderDistance);
            Vector3 targetWorld = (_vehicle._rotatioin * targetLocal) + _vehicle._pos; //PointToWorldSpace
            DebugWide.DrawCircle(_vehicle._pos + _vehicle._heading * _wanderDistance, _wanderRadius, Color.green);
            DebugWide.DrawLine(_vehicle._pos, targetWorld, Color.green);
        }


        public Vector3 Flee(Vector3 TargetPos)
        {
            //only flee if the target is within 'panic distance'. Work in distance
            //squared space.
            //const float PanicDistanceSq = 100.0f * 100.0f;
            //if ((m_pVehicle.Pos() - TargetPos).sqrMagnitude > PanicDistanceSq)
            //{
            //    return Vector2.zero;
            //}


            Vector3 DesiredVelocity = (_vehicle._pos - TargetPos).normalized
                                      * _vehicle._maxSpeed;

            return (DesiredVelocity - _vehicle._velocity);
        }

        public Vector3 Evade(Vehicle pursuer)
        {
            /* Not necessary to include the check for facing direction this time */

            Vector3 ToPursuer = pursuer._pos - _vehicle._pos;

            //uncomment the following two lines to have Evade only consider pursuers 
            //within a 'threat range'
            const float ThreatRange = 30.0f;
            DebugWide.DrawCircle(_vehicle._pos, ThreatRange, Color.gray);
            if (ToPursuer.sqrMagnitude > ThreatRange * ThreatRange) return Vector3.zero;

            //the lookahead time is propotional to the distance between the pursuer
            //and the pursuer; and is inversely proportional to the sum of the
            //agents' velocities
            float LookAheadTime = ToPursuer.magnitude /
                                   (_vehicle._maxSpeed + pursuer._speed);

            //LookAheadTime += TurnaroundTime(_vehicle, pursuer._pos , -1); //이렇게 턴시간 늘리는 것은 아닌것 같음 

            //------------------------
            Vector3 prPos = pursuer._pos + pursuer._velocity * LookAheadTime;
            DebugWide.DrawCircle(prPos, 2, Color.blue);

            //------------------------

            //now flee away from predicted future position of the pursuer
            return Flee(pursuer._pos + pursuer._velocity * LookAheadTime);
        }

        public float _SteeringForceTweaker = 200;

        public float _maxForce = 200;
        public float _ViewDistance = 30;

        public float _WeightObstacleAvoidance = 10 * 200;

        public float _WeightSeparation = 1 * 200;
        public float _WeightAlignment = 1 * 200;
        public float _WeightCohesion = 2 * 200;

        public float _WeightWander = 1 * 200;
        public float _WeightPursuit = 1 * 200f;
        public float _WeightEvade = 1 * 200f;


        public enum eCalcMode
        {
            CalculateWeightedSum = 0,
            CalculatePrioritized,
            CalculateDithered
        }
        public eCalcMode _calcMode = eCalcMode.CalculateWeightedSum;
        public Vector3 Calculate()
        {
            TagNeighbors(_vehicle, EntityMgr.list, _ViewDistance);

            //if(0 == _vehicle._id)
                //DrawTagNeighbors(_vehicle);

            if(eCalcMode.CalculateWeightedSum == _calcMode)
                _steeringForce = CalculateWeightedSum();
            else if(eCalcMode.CalculatePrioritized == _calcMode)
                _steeringForce = CalculatePrioritized();
            else if (eCalcMode.CalculateDithered == _calcMode)
                _steeringForce = CalculateDithered();

            return _steeringForce;

        }

        public Vector3 CalculateWeightedSum()
        {
            _steeringForce = Vector3.zero;

            if (On(eType.obstacle_avoidance))
            {
                _steeringForce += ObstacleAvoidance(EntityMgr.list) * _WeightObstacleAvoidance;
            }

            _steeringForce += Separation(EntityMgr.list) * _WeightSeparation;
            //DebugWide.LogBlue(_vehicle._id + " Separation : " + force);
            _steeringForce += Alignment(EntityMgr.list) * _WeightAlignment;
            _steeringForce += Cohesion(EntityMgr.list) * _WeightCohesion;

            if (null != _vehicle._v_target)
            {
                //_steeringForce += Seek(_vehicle._target) * _WeightWander;
                _steeringForce += Arrive(_vehicle._target , Deceleration.fast) * _WeightWander;
                //_steeringForce += ArriveObstacleStop(_vehicle._target, EntityMgr.list) * _WeightWander;
            }
            else
            {
                _steeringForce += Wander() * _WeightWander;
                //DebugWide.LogBlue(_vehicle._id+ " wander : " +force); 
            }


            _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

            return _steeringForce;
        }

        public Vector3 CalculatePrioritized()
        {
            _steeringForce = Vector3.zero;

            Vector3 force = Vector3.zero;

            if (On(eType.obstacle_avoidance))
            {
                force = ObstacleAvoidance(EntityMgr.list) * _WeightObstacleAvoidance;
                if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;
            }

            force = Separation(EntityMgr.list) * _WeightSeparation;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;

            force = Alignment(EntityMgr.list) * _WeightAlignment;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;

            force = Cohesion(EntityMgr.list) * _WeightCohesion;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;

            if (null != _vehicle._v_target)
            {
                //force = Seek(_vehicle._target) * _WeightWander;
                force = Arrive(_vehicle._target, Deceleration.fast) * _WeightWander;

                if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;
            }
            else
            {
                force = Wander() * _WeightWander;
                if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;
                //DebugWide.LogBlue(_vehicle._id+ " wander : " +force); 
            }


            return _steeringForce;
        }

        public float prObstacleAvoidance = 0.5f;
        public float prSeparation = 0.2f;
        public float prAlignment = 0.3f;
        public float prCohesion = 0.6f;
        public float prWander = 0.8f;
        public Vector3 CalculateDithered()
        {
            //reset the steering force
            _steeringForce = Vector3.zero;


            if (On(eType.obstacle_avoidance) &&  Misc.RandFloat() < prObstacleAvoidance)
            {
                _steeringForce += ObstacleAvoidance(EntityMgr.list) * _WeightObstacleAvoidance / prObstacleAvoidance;

                if (!Misc.IsZero(_steeringForce))
                {
                    _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                    return _steeringForce;
                }
            }
            if (Misc.RandFloat() < prSeparation)
            {
                _steeringForce += Separation(EntityMgr.list) * _WeightSeparation / prSeparation;

                if (!Misc.IsZero(_steeringForce))
                {
                    _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                    return _steeringForce;
                }
            }
            if (Misc.RandFloat() < prAlignment)
            {
                _steeringForce += Alignment(EntityMgr.list) * _WeightAlignment / prAlignment;

                if (!Misc.IsZero(_steeringForce))
                {
                    _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                    return _steeringForce;
                }
            }
            if (Misc.RandFloat() < prCohesion)
            {
                _steeringForce += Cohesion(EntityMgr.list) * _WeightCohesion / prCohesion;

                if (!Misc.IsZero(_steeringForce))
                {
                    _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                    return _steeringForce;
                }
            }

            if (null != _vehicle._v_target)
            {
                //_steeringForce += Seek(_vehicle._target) * _WeightWander;
                _steeringForce += Arrive(_vehicle._target, Deceleration.fast) * _WeightWander;

                if (!Misc.IsZero(_steeringForce))
                {
                    _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                    return _steeringForce;
                }
            }else
            {
                if (Misc.RandFloat() < prWander)
                {
                    _steeringForce += Wander() * _WeightWander / prWander;

                    if (!Misc.IsZero(_steeringForce))
                    {
                        _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                        return _steeringForce;
                    }
                }
            }


            return _steeringForce;
        }


        //---------------------------- Separation --------------------------------
        //
        // this calculates a force repelling from the other neighbors
        //------------------------------------------------------------------------
        Vector3 Separation(List<Vehicle> neighbors)
        {
            Vector3 SteeringForce = Vector3.zero;

            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough. ***also make sure it doesn't
                //include the evade target ***
                if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag &&
                  (neighbors[a] != _vehicle._v_target))
                {
                    Vector3 ToAgent = _vehicle._pos - neighbors[a]._pos;

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    //toAgent 가 0이 되면 Nan 값이 되어 , Nan과 연산한 다른 변수도 Nan이 되어버리는 문제가 있다 
                    if(false == Misc.IsZero(ToAgent))
                    {
                        SteeringForce += ToAgent.normalized / ToAgent.magnitude;
                    }

                }
            }

            //if (0 == _vehicle._id)
            {
                //DebugWide.DrawLine(_vehicle._pos, _vehicle._pos + SteeringForce, Color.green);
                //DebugWide.DrawCircle(_vehicle._pos + SteeringForce, 0.2f, Color.green);
            }

            return SteeringForce;
        }

        //---------------------------- Alignment ---------------------------------
        //
        //  returns a force that attempts to align this agents heading with that
        //  of its neighbors
        //------------------------------------------------------------------------
        Vector3 Alignment(List<Vehicle> neighbors)
        {
            //used to record the average heading of the neighbors
            Vector3 AverageHeading = Vector3.zero;

            //used to count the number of vehicles in the neighborhood
            int NeighborCount = 0;

            //iterate through all the tagged vehicles and sum their heading vectors  
            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure *this* agent isn't included in the calculations and that
                //the agent being examined  is close enough ***also make sure it doesn't
                //include any evade target ***
                if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag &&
                  (neighbors[a] != _vehicle._v_target))
                {
                    AverageHeading += neighbors[a]._heading;

                    ++NeighborCount;
                }
            }

            //if the neighborhood contained one or more vehicles, average their
            //heading vectors.
            if (NeighborCount > 0)
            {
                AverageHeading /= (float)NeighborCount;

                AverageHeading -= _vehicle._heading; //seek 방향힘 구하는 방식 추정 
            }

            //if (0 == _vehicle._id)
            {
                //DebugWide.DrawLine(_vehicle._pos, _vehicle._pos + AverageHeading, Color.blue);
                //DebugWide.DrawCircle(_vehicle._pos + AverageHeading, 0.2f, Color.blue);
            }


            return AverageHeading;
        }

        Vector3 Cohesion(List<Vehicle> neighbors)
        {
            //first find the center of mass of all the agents
            Vector3 CenterOfMass = Vector3.zero, SteeringForce = Vector3.zero;

            int NeighborCount = 0;

            //iterate through the neighbors and sum up all the position vectors
            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure *this* agent isn't included in the calculations and that
                //the agent being examined is close enough ***also make sure it doesn't
                //include the evade target ***
                if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag &&
                  (neighbors[a] != _vehicle._v_target))
                {
                    CenterOfMass += neighbors[a]._pos;

                    ++NeighborCount;
                }
            }

            if (NeighborCount > 0)
            {
                //the center of mass is the average of the sum of positions
                CenterOfMass /= (float)NeighborCount;

                //now seek towards that position
                SteeringForce = Seek(CenterOfMass);
            }

            //if (0 == _vehicle._id)
            {
                //DebugWide.DrawLine(_vehicle._pos, _vehicle._pos + SteeringForce.normalized, Color.red);
                //DebugWide.DrawCircle(_vehicle._pos + SteeringForce.normalized, 0.2f, Color.red);
                //DebugWide.DrawCircle(CenterOfMass, 0.5f, Color.red);
            }

            //the magnitude of cohesion is usually much larger than separation or
            //allignment so it usually helps to normalize it.
            return SteeringForce.normalized;
        }

        float _DBoxLength;
        float _MinDetectionBoxLength = 15f;
        Vector3 ObstacleAvoidance(List<Vehicle> obstacles)
        {
            //the detection box length is proportional to the agent's velocity
            _DBoxLength = _MinDetectionBoxLength +
                            (_vehicle._speed / _vehicle._maxSpeed) * _MinDetectionBoxLength;

            //tag all obstacles within range of the box for processing
            TagNeighbors(_vehicle, obstacles, _DBoxLength); 

            //this will keep track of the closest intersecting obstacle (CIB)
            Vehicle ClosestIntersectingObstacle = null;

            //this will be used to track the distance to the CIB
            float DistToClosestIP = float.MaxValue;

            //this will record the transformed local coordinates of the CIB
            Vector3 LocalPosOfClosestObstacle = Vector3.zero;

            Vector3 perp = Vector3.Cross(Vector3.up, _vehicle._heading);
            foreach (Vehicle curOb in obstacles)
            {
                //if the obstacle has been tagged within range proceed
                if (true == curOb._tag)
                {
                    //calculate this obstacle's position in local space
                    Vector3 LocalPos = Misc.PointToLocalSpace_3(curOb._pos,
                                                           _vehicle._heading,
                                                           perp,
                                                           _vehicle._pos);

                    //if the local position has a negative x value then it must lay
                    //behind the agent. (in which case it can be ignored)
                    if (LocalPos.z >= 0)
                    {
                        //if the distance from the x axis to the object's position is less
                        //than its radius + half the width of the detection box then there
                        //is a potential intersection.
                        float ExpandedRadius = curOb._radius + _vehicle._radius;

                        if (Math.Abs(LocalPos.x) < ExpandedRadius)
                        {
                            //now to do a line/circle intersection test. The center of the 
                            //circle is represented by (cX, cY). The intersection points are 
                            //given by the formula x = cX +/-sqrt(r^2-cY^2) for y=0. 
                            //We only need to look at the smallest positive value of x because
                            //that will be the closest point of intersection.
                            float cZ = LocalPos.z;
                            float cX = LocalPos.x;

                            //we only need to calculate the sqrt part of the above equation once
                            float SqrtPart = (float)Math.Sqrt(ExpandedRadius * ExpandedRadius - cX * cX);

                            float ip = cZ - SqrtPart;

                            if (ip <= 0.0)
                            {
                                ip = cZ + SqrtPart;
                            }

                            //-----------------
                            DebugWide.DrawCircle(_vehicle._pos + _vehicle._heading * ip, 1f, Color.white);
                            DebugWide.DrawCircle(curOb._pos , ExpandedRadius, Color.white);
                            //-----------------

                            //test to see if this is the closest so far. If it is keep a
                            //record of the obstacle and its local coordinates
                            if (ip < DistToClosestIP)
                            {
                                DistToClosestIP = ip;

                                ClosestIntersectingObstacle = curOb;

                                LocalPosOfClosestObstacle = LocalPos;
                            }
                        }
                    }
                }

            }

            //if we have found an intersecting obstacle, calculate a steering 
            //force away from it
            Vector3 SteeringForce = Vector3.zero;

            if (null != ClosestIntersectingObstacle)
            {
                //the closer the agent is to an object, the stronger the 
                //steering force should be
                float multiplier = 1.0f + (_DBoxLength - LocalPosOfClosestObstacle.z) /
                                    _DBoxLength;

                //calculate the lateral force
                SteeringForce.x = (ClosestIntersectingObstacle._radius -
                                   LocalPosOfClosestObstacle.x) * multiplier;

                //apply a braking force proportional to the obstacles distance from
                //the vehicle. 
                const float BrakingWeight = 0.2f;

                SteeringForce.z = (ClosestIntersectingObstacle._radius -
                                   LocalPosOfClosestObstacle.z) *
                                   BrakingWeight;
            }

            //finally, convert the steering vector from local to world space
            Vector3 worldPos = Misc.PointToWorldSpaceZX(SteeringForce,
                                      _vehicle._heading,
                                      perp);

            //-------------------------------------------------
            Quaternion rot_box = Quaternion.FromToRotation(Vector3.forward, _vehicle._heading);
            Vector3 box_0 = _vehicle._pos + rot_box * new Vector3(_vehicle._radius,0,0);
            Vector3 box_1 = _vehicle._pos + rot_box * new Vector3(_vehicle._radius, 0, _DBoxLength);
            Vector3 box_2 = _vehicle._pos + rot_box * new Vector3(-_vehicle._radius, 0, _DBoxLength);
            Vector3 box_3 = _vehicle._pos + rot_box * new Vector3(-_vehicle._radius, 0,0);
            DebugWide.DrawLine(box_0, box_1, Color.gray);
            DebugWide.DrawLine(box_1, box_2, Color.gray);
            DebugWide.DrawLine(box_2, box_3, Color.gray);
            DebugWide.DrawLine(box_3, box_0, Color.gray);
            DebugWide.DrawLine(_vehicle._pos, _vehicle._pos + worldPos, Color.black);
            DebugWide.DrawCircle(_vehicle._pos + worldPos, 0.5f, Color.black);
            //-------------------------------------------------


            return worldPos;
        }

        Vector3 ArriveObstacleStop(Vector3 targetPos, List<Vehicle> obstacles)
        {
            //the detection box length is proportional to the agent's velocity
            _DBoxLength = _MinDetectionBoxLength +
                            (_vehicle._speed / _vehicle._maxSpeed) * _MinDetectionBoxLength;

            //tag all obstacles within range of the box for processing
            TagNeighbors(_vehicle, obstacles, _DBoxLength);

            //this will keep track of the closest intersecting obstacle (CIB)
            Vehicle ClosestIntersectingObstacle = null;

            //this will be used to track the distance to the CIB
            float DistToClosestIP = float.MaxValue;

            //this will record the transformed local coordinates of the CIB
            Vector3 LocalPosOfClosestObstacle = Vector3.zero;

            //Vector3 dirNormal = targetPos - _vehicle._pos;
            Vector3 dirNormal = _vehicle._heading;
            Vector3 perp = Vector3.Cross(Vector3.up, dirNormal);
            foreach (Vehicle curOb in obstacles)
            {
                //if the obstacle has been tagged within range proceed
                if (true == curOb._tag)
                {
                    //calculate this obstacle's position in local space
                    Vector3 LocalPos = Misc.PointToLocalSpace_3(curOb._pos,
                                                           dirNormal,
                                                           perp,
                                                           _vehicle._pos);

                    //if the local position has a negative x value then it must lay
                    //behind the agent. (in which case it can be ignored)
                    if (LocalPos.z >= 0)
                    {
                        //if the distance from the x axis to the object's position is less
                        //than its radius + half the width of the detection box then there
                        //is a potential intersection.
                        float ExpandedRadius = curOb._radius + _vehicle._radius;

                        if (Math.Abs(LocalPos.x) < ExpandedRadius)
                        {
                            //now to do a line/circle intersection test. The center of the 
                            //circle is represented by (cX, cY). The intersection points are 
                            //given by the formula x = cX +/-sqrt(r^2-cY^2) for y=0. 
                            //We only need to look at the smallest positive value of x because
                            //that will be the closest point of intersection.
                            float cZ = LocalPos.z;
                            float cX = LocalPos.x;

                            //we only need to calculate the sqrt part of the above equation once
                            float SqrtPart = (float)Math.Sqrt(ExpandedRadius * ExpandedRadius - cX * cX);

                            float ip = cZ - SqrtPart;

                            if (ip <= 0.0)
                            {
                                ip = cZ + SqrtPart;
                            }

                            //-----------------
                            //if(0 == _vehicle._id)
                            //{
                            //    DebugWide.DrawCircle(_vehicle._pos + dirNormal * ip, 1f, Color.white);
                            //    DebugWide.DrawCircle(curOb._pos, ExpandedRadius, Color.white);
                            //}

                            //-----------------

                            //test to see if this is the closest so far. If it is keep a
                            //record of the obstacle and its local coordinates
                            if (ip < DistToClosestIP)
                            {
                                DistToClosestIP = ip;

                                ClosestIntersectingObstacle = curOb;

                                LocalPosOfClosestObstacle = LocalPos;
                            }
                        }
                    }
                }

            }

            //if we have found an intersecting obstacle, calculate a steering 
            //force away from it
            Vector3 SteeringForce = Vector3.zero;
            Vector3 worldPos = Vector3.zero;
            if (null != ClosestIntersectingObstacle)
            {
                //worldPos = _vehicle._pos + dirNormal * DistToClosestIP;
                worldPos = ClosestIntersectingObstacle._pos + (_vehicle._pos - ClosestIntersectingObstacle._pos).normalized * (_vehicle._radius+ ClosestIntersectingObstacle._radius);
                SteeringForce = Arrive(worldPos, Deceleration.fast);

                if (0 == _vehicle._id)
                {
                    DebugWide.DrawCircle(worldPos, 0.5f, Color.red);
                    DebugWide.DrawCircle(ClosestIntersectingObstacle._pos, ClosestIntersectingObstacle._radius, Color.white);
                    DebugWide.DrawCircle(_vehicle._pos, _vehicle._radius, Color.white);
                }

            }
            else
            {
                SteeringForce = Arrive(targetPos, Deceleration.fast);
            }


            //-------------------------------------------------
            if (0 == _vehicle._id)
            {
                Quaternion rot_box = Quaternion.FromToRotation(Vector3.forward, _vehicle._heading);
                Vector3 box_0 = _vehicle._pos + rot_box * new Vector3(_vehicle._radius, 0, 0);
                Vector3 box_1 = _vehicle._pos + rot_box * new Vector3(_vehicle._radius, 0, _DBoxLength);
                Vector3 box_2 = _vehicle._pos + rot_box * new Vector3(-_vehicle._radius, 0, _DBoxLength);
                Vector3 box_3 = _vehicle._pos + rot_box * new Vector3(-_vehicle._radius, 0, 0);
                DebugWide.DrawLine(box_0, box_1, Color.gray);
                DebugWide.DrawLine(box_1, box_2, Color.gray);
                DebugWide.DrawLine(box_2, box_3, Color.gray);
                DebugWide.DrawLine(box_3, box_0, Color.gray);

            }

            //-------------------------------------------------


            return SteeringForce;
        }

    }
}
