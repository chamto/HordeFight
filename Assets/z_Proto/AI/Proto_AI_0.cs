using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Proto_AI_0
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

    public class Proto_AI_0 : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            Vehicle v = new Vehicle();
            v.Init();
            EntityMgr.Add(v);

        }

        int ID = 0;
        void Update()
        {

            
            Vehicle vh = EntityMgr.list[ID];
            vh.KeyInput();
            vh.Update();
        }

        private void OnDrawGizmos()
        {
            foreach (Vehicle v in EntityMgr.list)
            {
                v.Draw(Color.black);
            }
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

        public float _maxSpeed = 50f;

        //public float _maxForce = 400f;

        //public float _maxTurnRate;

        public Vector3 _size = new Vector3(3, 0, 3);

        public bool _tag = false;
        public float _radius = 3;

        public Quaternion _rotatioin = Quaternion.identity;

        public Vector3 _target = ConstV.v3_zero;
        public Vector3 _offset = ConstV.v3_zero;
        public Vehicle _v_target = null;
        //public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        //public SteeringBehavior _steeringBehavior = new SteeringBehavior();

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
            //_mode = SteeringBehavior.eType.none;
        }

        public void Init()
        {
            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            //_steeringBehavior._vehicle = this;
        }

        public bool _isNonpenetration = false;
        public void Update2()
        {

            //Vector3 SteeringForce = _steeringBehavior.Calculate();
            Vector3 SteeringForce = new Vector3(0,0,1);

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
        public void Update()
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
                    //move the entity a distance away equivalent to the amount of overlap.
                    entity._pos = (entity._pos + (ToEntity / DistFromEachOther) *
                                   AmountOfOverLap);
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
            //DebugWide.DrawCircle(_pos, 1f * _size.z, color); //test

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
            if(false == isRot)
            {
                if(rot > 0)
                {
                    rot -= RotatePerSecond; 
                }else if( rot < 0)
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

                if(false == Misc.IsZero(_velocity))
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

        }
    }
}



