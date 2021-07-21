using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_2
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

    public class Proto_AI_2 : MonoBehaviour
    {
        public Transform _tr_target = null;
        public Proto_AI.GridManager _gridMgr = new Proto_AI.GridManager();

        public float _mass = 1f;
        public float _maxSpeed = 10f;
        public float _maxForce = 40f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;
        public float _weight = 20;
        public bool _isNonpenetration = true;

        void Awake()
        {
            //QualitySettings.vSyncCount = 0; //v싱크 끄기
            //Application.targetFrameRate = 60; //60 프레임 제한

            FrameTime.SetFixedFrame_FPS_30(); //30프레임 제한 설정
        }

        // Use this for initialization
        void Start()
        {
            _gridMgr.Init();
            _tr_target = GameObject.Find("tr_target").transform;

            //0
            Vehicle v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._mode = SteeringBehavior.eType.arrive;

            //1
            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[0];
            v._offset = new Vector3(1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //2
            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[0];
            v._offset = new Vector3(-1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //-------------------

            //3
            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[0];
            v._offset = new Vector3(1f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            ////4
            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[3];
            v._offset = new Vector3(1f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            //5
            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[3];
            v._offset = new Vector3(2f, 0, 0);
            v._mode = SteeringBehavior.eType.offset_pursuit;

        }


        int ID = 0;
        void Update()
        {

            //프레임이 크게 떨어질시 Time.delta 값이 과하게 커지는 경우가 발생 , 이럴경우 벽통과등의 문제등이 생긴다. 
            //deltaTime 값을 작게 유지하여 프로그램 무결성을 유지시킨다. 속도가 느려지고 시간이 안맞는 것은 어쩔 수 없다 
            float deltaTime = FrameTime.DeltaTime();

            //========================
            Vehicle vh = EntityMgr.list[ID];
            if (null != _tr_target)
                vh._target = _tr_target.position;
            vh.KeyInput();

            foreach (Vehicle v in EntityMgr.list)
            {
                v._mass = _mass;
                v._maxSpeed = _maxSpeed;
                v._maxForce = _maxForce;
                v._Friction = _Friction;
                v._anglePerSecond = _anglePerSecond;
                v._weight = _weight;
                v._isNonpenetration = _isNonpenetration;
                v.Update(deltaTime);
            }

            foreach (Vehicle v in EntityMgr.list)
            {

                //==========================================
                //동굴벽과 캐릭터 충돌처리 

                float maxR = Mathf.Clamp(v._radius, 0, 1); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 
                //동굴벽과 캐릭터 경계원 충돌처리 
                v._pos = _gridMgr.Collision_StructLine(v._pos, maxR);

                //==========================================

            }

        }

        public bool _Draw_BoundaryTile = false;
        private void OnDrawGizmos()
        {
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
        }
    }


    public class Vehicle
    {
        public int _id = -1;

        public Vector3 _pos = Vector3.zero;

        public Vector3 _velocity = new Vector3(0, 0, 0);

        public Vector3 _heading = Vector3.forward;


        public float _mass = 1f;
        public float _maxSpeed = 10f;
        public float _maxForce = 40f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;
        public float _weight = 20;
        public bool _isNonpenetration = true; //비침투 

        public float _speed = 0;

        public Vector3 _size =  new Vector3(0.5f, 0, 0.5f);

        public bool _tag = false;
        public float _radius = 0.5f;

        public Quaternion _rotation = Quaternion.identity;

        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vehicle _leader = null;
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();

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

        public void Init()
        {
            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;
        }


        public void Update(float deltaTime)
        {

            //_speed = 0;

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


            _pos += _velocity * deltaTime;
            //_pos = WrapAroundXZ(_pos, 100, 100);


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


            }
            //else
            //{
            //    _speed = 0;
            //    _velocity = ConstV.v3_zero;

            //}

            if(_isNonpenetration)
                EnforceNonPenetrationConstraint(this, EntityMgr.list);

        }

        public Vector3 WrapAroundXZ(Vector3 pos, int MaxX, int MaxY)
        {
            if (pos.x > MaxX) { pos.x = 0.0f; }

            if (pos.x < 0) { pos.x = (float)MaxX; }

            if (pos.z < 0) { pos.z = (float)MaxY; }

            if (pos.z > MaxY) { pos.z = 0.0f; }

            return pos;
        }

        public void EnforceNonPenetrationConstraint(Vehicle entity, List<Vehicle> ContainerOfEntities)
        {
            Vehicle curEntity;
            //iterate through all entities checking for any overlap of bounding radii
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //make sure we don't check against the individual
                if (curEntity == entity) continue;

                //calculate the distance between the positions of the entities
                Vector3 ToEntity = entity._pos - (curEntity)._pos;

                float DistFromEachOther = ToEntity.magnitude;

                //if this distance is smaller than the sum of their radii then this
                //entity must be moved away in the direction parallel to the
                //ToEntity vector   
                float AmountOfOverLap = (curEntity)._radius + entity._radius -
                                         DistFromEachOther;

                if (AmountOfOverLap >= 0)
                {
                    //ToEntity 가 0벡터일 경우  DistFromEachOther 로 나누어 노멀벡터를 구하려하면 nan에러가 발생한다 
                    //move the entity a distance away equivalent to the amount of overlap.
                    entity._pos = (entity._pos + VOp.Normalize(ToEntity) *
                                   AmountOfOverLap);

                    //DebugWide.LogRed(entity._id + "  " + AmountOfOverLap + "  " + entity._pos);
                }
            }//next entity
        }

        public void Draw(Color color)
        {


            Vector3 vb0, vb1, vb2;
            vb0 = _rotation * _array_VB[0] * _size.z;
            vb1 = _rotation * _array_VB[1] * _size.z;
            vb2 = _rotation * _array_VB[2] * _size.z;

            //에이젼트 출력 
            DebugWide.DrawLine(_pos + vb0, _pos + vb1, color);
            DebugWide.DrawLine(_pos + vb1, _pos + vb2, color);
            DebugWide.DrawLine(_pos + vb2, _pos + vb0, color);
            //DebugWide.DrawCircle(_pos, _radius, color); 

            //if (SteeringBehavior.eType.wander == _mode)
            //{
            //    _steeringBehavior.DrawWander();
            //}
        }

        public void KeyInput()
        {

            //마찰력
            if (Input.GetKey(KeyCode.R))
            {
                _Friction += 0.01f; //마찰감소
                if (_Friction > 1)
                    _Friction = 1;
            }
            if (Input.GetKey(KeyCode.F))
            {
                _Friction -= 0.01f; //마찰증가
                if (_Friction < 0)
                    _Friction = 0;
            }
            if (Input.GetKey(KeyCode.T))
            {
                _anglePerSecond += 15;
            }
            if (Input.GetKey(KeyCode.G))
            {
                _anglePerSecond -= 15;
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

            if (dist > 0)
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
                Vector3 DesiredVelocity = ToTarget * speed / dist;

                return (DesiredVelocity - _vehicle._velocity);
            }

            return Vector3.zero;
        }

        public Vector3 OffsetPursuit(Vehicle leader, Vector3 offset)
        {
            //calculate the offset's position in world space
            Vector3 WorldOffsetPos = (leader._rotation * offset) + leader._pos; //PointToWorldSpace

            Vector3 ToOffset = WorldOffsetPos - _vehicle._pos;

            //최소거리 이하로 계산됐다면 처리하지 않는다 - 이상회전 문제 때문에 예외처리함 
            const float MinLen = 0.1f;
            const float SqrMinLen = MinLen * MinLen;
            if (ToOffset.sqrMagnitude < SqrMinLen)
                return ConstV.v3_zero;

            //DebugWide.AddDrawQ_Circle(WorldOffsetPos, 0.1f, Color.red);

            //the lookahead time is propotional to the distance between the leader
            //and the pursuer; and is inversely proportional to the sum of both
            //agent's velocities
            float LookAheadTime = ToOffset.magnitude /
                                  (_vehicle._maxSpeed + leader._speed);

            //now Arrive at the predicted future position of the offset
            return Arrive(WorldOffsetPos + leader._velocity * LookAheadTime, Deceleration.fast);
            //return Arrive(WorldOffsetPos, Deceleration.fast);
        }
    }

}//end namespace



