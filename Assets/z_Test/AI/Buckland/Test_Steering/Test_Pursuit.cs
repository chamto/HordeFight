using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_Steering_Pursuit
{
    public class Test_Pursuit : MonoBehaviour
    {
        public Transform _tr_target = null;

        List<Vehicle> _list_vehicle = new List<Vehicle>();

        public const int Num_Agents = 2;

        // Use this for initialization
        void Start()
        {

            for (int i = 0; i < Num_Agents; i++)
            {
                Vehicle v = new Vehicle();
                v.Init();
                _list_vehicle.Add(v);
            }

            _list_vehicle[0]._mode = SteeringBehavior.eType.wander;

            _list_vehicle[1]._mode = SteeringBehavior.eType.pursuit;
            _list_vehicle[1]._v_target = _list_vehicle[0];

        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        private void OnDrawGizmos()
        {
            foreach (Vehicle v in _list_vehicle)
            {
                if (null != _tr_target)
                    v._target = _tr_target.position;

                v.Update();
                v.Draw(Color.black);
            }
        }
    }

    public class Vehicle
    {
        public Vector3 _pos = Vector3.zero;

        public Vector3 _velocity = new Vector3(0,0,0);

        public Vector3 _heading = Vector3.forward;

        //public Vector3 _side;

        public float _mass = 1f;

        public float _speed;

        public float _maxSpeed = 50f;

        public float _maxForce = 400f;

        //public float _maxTurnRate;

        public Vector3 _size = new Vector3(3, 0, 3);

        public Quaternion _rotatioin = Quaternion.identity;

        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vehicle _v_target = null;
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        SteeringBehavior _steeringBehavior = new SteeringBehavior();

        public void Init()
        {
            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;
        }

        float __PursuitWeight = 200f;
        float __WanderWeight = 200f;
        public void Update()
        {

            //Vector3 SteeringForce = m_pSteering.Calculate();
            Vector3 SteeringForce = ConstV.v3_zero;
            if (SteeringBehavior.eType.arrive == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.normal);
            else if (SteeringBehavior.eType.offset_pursuit == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_v_target, _offset);
            else if (SteeringBehavior.eType.wander == _mode)
                SteeringForce = _steeringBehavior.Wander() * __WanderWeight;
            else if (SteeringBehavior.eType.pursuit == _mode)
                SteeringForce = _steeringBehavior.Pursuit(_v_target) * __PursuitWeight;

            SteeringForce = VOp.Truncate(SteeringForce, _maxForce);

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
            DebugWide.DrawCircle(_pos, 1f * _size.z, color); //test

            if (SteeringBehavior.eType.wander == _mode)
            {
                _steeringBehavior.DrawWander();
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

            //------------------------
            Vector3 DesiredVelocity, prPos;
            DesiredVelocity = ((evader._pos + evader._velocity * LookAheadTime) - _vehicle._pos);

            prPos = _vehicle._pos + (DesiredVelocity);
            DebugWide.DrawCircle(prPos, 2, Color.red);
            //------------------------

            //now seek to the predicted future position of the evader
            return Seek(evader._pos + evader._velocity * LookAheadTime);
        }

        public float TurnaroundTime(Vehicle agent , Vector3 targetPos)
        {
            Vector3 toTarget = (targetPos - agent._pos).normalized;

            float dot = Vector3.Dot(agent._heading, toTarget);

            const float coefficient = 0.5f;

            return (dot - 1f) * -coefficient;
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
            _vWanderTarget += new Vector3(Misc.RandomClamped() * JitterThisTimeSlice,0,
                                          Misc.RandomClamped() * JitterThisTimeSlice);

            //reproject this new vector back on to a unit circle
            _vWanderTarget.Normalize();

            //increase the length of the vector to the same as the radius
            //of the wander circle
            _vWanderTarget *= _wanderRadius;

            //move the target into a position WanderDist in front of the agent
            Vector3 targetLocal = _vWanderTarget + new Vector3(0,0, _wanderDistance);

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
    }
}

