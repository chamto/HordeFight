using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Test_Steering_OffsetPursuit
{
    [System.Serializable]
    public class Test_OffsetPursuit : MonoBehaviour
    {
        public Transform _tr_target = null;

        public Param _prm = new Param();
        List<Vehicle> _list_vehicle = new List<Vehicle>();

        // Use this for initialization
        void Start()
        {
            SingleO.prm = _prm;

            for (int i = 0; i < _prm.NumAgents; i++)
            {
                Vehicle v = new Vehicle();
                v.Init();
                _list_vehicle.Add(v);
            }

            _list_vehicle[1]._mode = 1;
            _list_vehicle[1]._leader = _list_vehicle[0];
            _list_vehicle[1]._offset = new Vector3(1f, 0, -1f);

            _list_vehicle[2]._mode = 1;
            _list_vehicle[2]._leader = _list_vehicle[0];
            _list_vehicle[2]._offset = new Vector3(-1f, 0, -1f);

        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        private void OnDrawGizmos()
        {
            foreach (Vehicle v in _list_vehicle)
            {
                if(null != (object)_tr_target)
                    v._target = _tr_target.position;

                v.Update();
                v.Draw(Color.black);
            }
        }
    }

    //======================================================

    public static class SingleO
    {
        public static Param prm = null;
    }

    [System.Serializable]
    public class Param
    {
        [Space]
        public int NumAgents = 10;
    }

    public class Vehicle
    {
        public Vector3 _pos = ConstV.v3_zero;

        public Vector3 _velocity;

        public Vector3 _heading;

        //public Vector3 _side;

        public float _mass = 1f;

        public float _speed;

        public float _maxSpeed = 150f;

        public float _maxForce;

        public float _maxTurnRate;

        public Quaternion _rotatioin = Quaternion.identity;

        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vehicle _leader = null;
        public int _mode = 0;

        Vector3[] _array_VB = new Vector3[3];

        SteeringBehavior _steeringBehavior = new SteeringBehavior();

        public void Init()
        {
            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;
        }

        public void Update()
        {

            //Vector3 SteeringForce = m_pSteering.Calculate();
            Vector3 SteeringForce = ConstV.v3_zero;
            if(0 == _mode)
                SteeringForce = _steeringBehavior.Arrive(_target, SteeringBehavior.Deceleration.normal);
            else if(1 == _mode)
                SteeringForce = _steeringBehavior.OffsetPursuit(_leader, _offset);

            //Acceleration = Force/Mass
            Vector3 acceleration = SteeringForce / _mass;

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
                _rotatioin = Quaternion.FromToRotation(ConstV.v3_forward, _heading);

            }

        }
        public void Draw(Color color)
        {
            Vector3 vb0, vb1, vb2;
            vb0 = _rotatioin * _array_VB[0];
            vb1 = _rotatioin * _array_VB[1];
            vb2 = _rotatioin * _array_VB[2];

            DebugWide.DrawLine(_pos + vb0, _pos + vb1, color);
            DebugWide.DrawLine(_pos + vb1, _pos + vb2, color);
            DebugWide.DrawLine(_pos + vb2, _pos + vb0, color);
            DebugWide.DrawCircle(_pos, 1f, color); //test
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
}

