using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_1
{
    public static class DebugViewer
    {
        private static Queue<DrawInfo> _drawQ = new Queue<DrawInfo>();

        public static void AddDrawInfo(DrawInfo info)
        {
            _drawQ.Enqueue(info);
        }

        public static void AddDraw_Circle(Vector3 origin, float radius, Color color)
        {

            DrawInfo drawInfo = new DrawInfo();
            drawInfo.kind = DrawInfo.eKind.Circle;
            drawInfo.origin = origin;
            drawInfo.radius = radius;
            drawInfo.color = color;

            AddDrawInfo(drawInfo);
        }

        public static void AddDraw_Line(Vector3 origin, Vector3 last, Color color)
        {
            DrawInfo drawInfo = new DrawInfo();
            drawInfo.kind = DrawInfo.eKind.Line;
            drawInfo.origin = origin;
            drawInfo.last = last;
            drawInfo.color = color;

            AddDrawInfo(drawInfo);
        }

        public static void Clear()
        {
            _drawQ.Clear();
        }

        static float _elapsedTime = 0;
        public static void ClearAfterTime(float second)
        {
            _elapsedTime += Time.deltaTime;

            if(second < _elapsedTime)
            {
                _drawQ.Clear();
                _elapsedTime = 0; 
            }
        }

        public static void OnDrawGizmos()
        {
            foreach (DrawInfo info in _drawQ)
            {
                info.Draw();
            }

        }


    }

    public class EntityMgr
    {

        public static readonly List<Vehicle> list = new List<Vehicle>();
        public static void Add(Vehicle v)
        {
            list.Add(v);
            v._id = list.Count - 1;
        }
    }

    public class Proto_AI_1 : MonoBehaviour
    {
        public Transform _tr_target = null;
        public GridManager _gridMgr = new GridManager();

        //v싱크 끄기 ,  60 프레임 제한
        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        // Use this for initialization
        void Start()
        {
            _gridMgr.Init();
            _tr_target = GameObject.Find("tr_target").transform;

            Vehicle v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._mode = SteeringBehavior.eType.arrive;

            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[0];
            v._offset = new Vector3(1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;

            v = new Vehicle();
            v.Init();
            v._pos = new Vector3(17, 0, 12);
            EntityMgr.Add(v);
            v._leader = EntityMgr.list[0];
            v._offset = new Vector3(-1f, 0, -1f);
            v._mode = SteeringBehavior.eType.offset_pursuit;
        }


        float _frame_count = 0;
        float _elapsedTime = 0;
        int ID = 0;
        void Update()
        {
            if (1 < _elapsedTime)
            {
                DebugWide.LogBlue(_frame_count + "  " + Time.deltaTime);
                _frame_count = 0;
                _elapsedTime = 0;
            }
            _frame_count++;
            _elapsedTime += Time.deltaTime;


            //========================
            Vehicle vh = EntityMgr.list[ID];
            if (null != _tr_target)
                vh._target = _tr_target.position;
            vh.KeyInput();

            foreach (Vehicle v in EntityMgr.list)
            {
                v.Update();
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
            foreach (Vehicle v in EntityMgr.list)
            {
                v.Draw(Color.black);
            }

            if(true == _Draw_BoundaryTile)
                _gridMgr.Draw_BoundaryTile();

            DebugViewer.ClearAfterTime(1);
            DebugViewer.OnDrawGizmos();
        }
    }


    public class Vehicle
    {
        public int _id = -1;

        public Vector3 _pos = Vector3.zero;

        public Vector3 _velocity = new Vector3(0, 0, 0);

        public Vector3 _heading = Vector3.forward;

        //public Vector3 _side;

        public float _mass = 1f;

        public float _speed;

        public float _maxSpeed = 10f;

        public float _maxForce = 40f;

        //public float _maxTurnRate;

        public Vector3 _size =  new Vector3(0.5f, 0, 0.5f);

        public bool _tag = false;
        public float _radius = 0.5f;

        public Quaternion _rotatioin = Quaternion.identity;

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
            _speed = 0;

            _tag = false;

            _rotatioin = Quaternion.identity;

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

        public bool _isNonpenetration = false;
        public void Update2()
        {

            //Vector3 SteeringForce = _steeringBehavior.Calculate();
            Vector3 SteeringForce = new Vector3(0, 0, 1);

            //Acceleration = Force/Mass
            Vector3 acceleration = SteeringForce / _mass;
            //DebugWide.LogBlue(acceleration.magnitude);

            //update velocity
            _velocity += acceleration * Time.deltaTime;

            _velocity = VOp.Truncate(_velocity, _maxSpeed);

            //update the position
            _pos += _velocity * Time.deltaTime;

            //update the heading if the vehicle has a non zero velocity
            if (_velocity.sqrMagnitude > 0.00000001f)
            {
                _heading = VOp.Normalize(_velocity);
                _speed = _velocity.magnitude;
                //DebugWide.LogBlue(_speed); 
                _rotatioin = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

            }

            //if (_isNonpenetration)
            //EnforceNonPenetrationConstraint(this, EntityMgr.list);

            _pos = WrapAroundXZ(_pos, 100, 100);

        }

        //게임잼4 332p 참고 
        float F = 0.85f; //마찰력 
        float A = 0; //가속도 
        float rot = 0;
        public void Update1()
        {

            _rotatioin = Quaternion.AngleAxis(rot * Time.deltaTime, Vector3.up);

            _velocity *= F; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 
            DebugWide.LogGreen(VOp.ToString(_velocity));
            _heading = _rotatioin * _heading;
            _velocity = (_velocity) + (_heading * A) * Time.deltaTime;

            //_maxSpeed = 3f;
            //_velocity = VOp.Truncate(_velocity, _maxSpeed);
            //_velocity *= F;

            _pos += _velocity * Time.deltaTime;

            DebugWide.LogBlue("F: " + F + " A: " + A.ToString("00.00") + " rot: " + rot + " vel: " + VOp.ToString(_velocity));
            _pos = WrapAroundXZ(_pos, 100, 100);
        }

        float _rot2 = 180;
        float _weight = 20;
        public void Update()
        {
            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.fast) * _weight;
                //SteeringForce = _steeringBehavior.Seek(_target) * _weight;
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset) * _weight;

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);

            //F = 0;
            Vector3 acceleration = SteeringForce / _mass;
            A = acceleration.magnitude;
            _velocity *= F; //마찰계수가 1에 가깝거나 클수록 미끄러지는 효과가 커진다 
                            
                            //_velocity = (_velocity) + (_heading * A) * Time.deltaTime;
            _velocity +=  acceleration * Time.deltaTime;

            //_maxSpeed = 10f;
            _velocity = VOp.Truncate(_velocity, _maxSpeed);


            _pos += _velocity * Time.deltaTime;


            if (_velocity.sqrMagnitude > 0.00001f)
            {

                float def = VOp.PerpDot_XZ(_heading, _velocity);
                //if (Math.Abs(def) > 0.000001f)
                {
                    float max_angle = Geo.AngleSigned_AxisY(_heading, _velocity);
                    def = Mathf.Clamp(def, -1, 1);
                    float angle = _rot2 * -def * Time.deltaTime;
                    //if (_rot2 > max_angle)
                    //_rot2 = max_angle;
                    _rotatioin = Quaternion.AngleAxis(angle, Vector3.up);
                    _heading = _rotatioin * _heading;
                    _heading = VOp.Normalize(_heading);

                    //_rotatioin = Quaternion.FromToRotation(ConstV.v3_forward, _velocity);
                    //_heading = VOp.Normalize(_velocity);

                    //DebugViewer.AddDraw_Line(_pos, _pos + _heading, Color.grey);
                }

            }

            //if (_isNonpenetration)
                EnforceNonPenetrationConstraint(this, EntityMgr.list);

            //if (0 == _id)
                //DebugWide.LogBlue("F: " + F + " A: " + A.ToString("00.00") + " rot2: " + _rot2 + " vel: " + VOp.ToString(_velocity) + " " + _velocity.magnitude + "  he: " + _heading);
            
                //_pos = WrapAroundXZ(_pos, 100, 100);
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
            _rotatioin = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

            Vector3 vb0, vb1, vb2;
            vb0 = _rotatioin * _array_VB[0] * _size.z;
            vb1 = _rotatioin * _array_VB[1] * _size.z;
            vb2 = _rotatioin * _array_VB[2] * _size.z;

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
            bool isRot = false;
            float RotatePerSecond = 15;

            //왼쪽회전
            if (Input.GetKey(KeyCode.Q))
            {
                float Angle = -180;
                rot -= RotatePerSecond;
                if (rot < Angle)
                    rot = Angle;

                isRot = true;
            }
            //오른쪽회전
            if (Input.GetKey(KeyCode.E))
            {
                float Angle = 180;
                rot += RotatePerSecond;
                if (rot > Angle)
                    rot = Angle;

                isRot = true;
            }
            if (false == isRot)
            {
                if (rot > 0)
                {
                    rot -= RotatePerSecond;
                }
                else if (rot < 0)
                {
                    rot += RotatePerSecond;
                }
            }

            //가속
            if (Input.GetKey(KeyCode.W))
            {

                A += 10f;
                //if (A > 1)
                //A = 1;
            }
            //브레이크
            if (Input.GetKey(KeyCode.S))
            {
                if (0 > Vector3.Dot(_heading, _velocity))
                {
                    A = 0;
                    _velocity = Vector3.zero;
                }

                if (false == Misc.IsZero(_velocity))
                {
                    A -= 10f;
                }

            }

            //마찰력
            if (Input.GetKey(KeyCode.R))
            {
                F += 0.01f; //마찰감소
                if (F > 1)
                    F = 1;
            }
            if (Input.GetKey(KeyCode.F))
            {
                F -= 0.01f; //마찰증가
                if (F < 0)
                    F = 0;
            }
            if (Input.GetKey(KeyCode.T))
            {
                _rot2 += 15;
            }
            if (Input.GetKey(KeyCode.G))
            {
                _rot2 -= 15;
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
            Vector3 WorldOffsetPos = (leader._rotatioin * offset) + leader._pos; //PointToWorldSpace

            Vector3 ToOffset = WorldOffsetPos - _vehicle._pos;

            //the lookahead time is propotional to the distance between the leader
            //and the pursuer; and is inversely proportional to the sum of both
            //agent's velocities
            float LookAheadTime = ToOffset.magnitude /
                                  (_vehicle._maxSpeed + leader._speed);

            //now Arrive at the predicted future position of the offset
            return Arrive(WorldOffsetPos + leader._velocity * LookAheadTime, Deceleration.fast);
        }
    }

}//end namespace



