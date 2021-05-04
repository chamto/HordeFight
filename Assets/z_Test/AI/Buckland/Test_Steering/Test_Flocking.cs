using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_Steering_Flocking
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
    public class Test_Flocking : MonoBehaviour
    {
        //public Transform _tr_target = null;

        public const int Num_Agents = 20;

        // Use this for initialization
        void Start()
        {

            for (int i = 0; i < Num_Agents; i++)
            {
                Vehicle v = new Vehicle();
                v.Init();
                //EntityMgr.list.Add(v);
                EntityMgr.Add(v);
            }
            //DebugWide.LogBlue(EntityMgr.list.Count);

            //EntityMgr.list[0]._mode = SteeringBehavior.eType.evade;
            //EntityMgr.list[0]._v_target = EntityMgr.list[1];

            //EntityMgr.list[1]._mode = SteeringBehavior.eType.pursuit; //추격 
            //EntityMgr.list[1]._v_target = EntityMgr.list[0];

        }

        // Update is called once per frame
        void Update()
        {
            SteeringBehavior sb0 = EntityMgr.list[0]._steeringBehavior;
            float sep = sb0._WeightSeparation;
            float ali = sb0._WeightAlignment;
            float coh = sb0._WeightCohesion;
            float wan = sb0._WanderWeight;
            string mode = sb0._calcMode.ToString() + "  isNp[Z]: " + EntityMgr.list[0]._isNonpenetration;
            DebugWide.LogBlue("0_ sep[QA]:  "+sep + " ali[WS]: " + ali + "  coh[ED]: " + coh + "  wan[RF]: " + wan + "  [TGB] " + mode);

            //분리
            if (Input.GetKey(KeyCode.Q))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WeightSeparation += 10f;
                }
            }
            if (Input.GetKey(KeyCode.A))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WeightSeparation -= 10f;
                    if (0 > v._steeringBehavior._WeightSeparation) v._steeringBehavior._WeightSeparation = 0;
                }
            }
            //정렬
            if (Input.GetKey(KeyCode.W))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WeightAlignment += 10f;
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WeightAlignment -= 10f;
                    if (0 > v._steeringBehavior._WeightAlignment) v._steeringBehavior._WeightAlignment = 0;
                }
            }
            //응집 
            if (Input.GetKey(KeyCode.E))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WeightCohesion += 10f;
                }
            }
            if (Input.GetKey(KeyCode.D))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WeightCohesion -= 10f;
                    if (0 > v._steeringBehavior._WeightCohesion) v._steeringBehavior._WeightCohesion = 0;
                }
            }
            //방황
            if (Input.GetKey(KeyCode.R))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WanderWeight += 10f;
                }
            }
            if (Input.GetKey(KeyCode.F))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._WanderWeight -= 10f;
                    if (0 > v._steeringBehavior._WanderWeight) v._steeringBehavior._WanderWeight = 0;
                }
            }
            //계산모드 
            if (Input.GetKeyDown(KeyCode.T))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._calcMode = SteeringBehavior.eCalcMode.CalculateWeightedSum;
                }
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._calcMode = SteeringBehavior.eCalcMode.CalculatePrioritized;
                }
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                foreach (Vehicle v in EntityMgr.list)
                {
                    v._steeringBehavior._calcMode = SteeringBehavior.eCalcMode.CalculateDithered;
                }
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {   
                foreach(Vehicle v in EntityMgr.list)
                {
                    v._isNonpenetration = !v._isNonpenetration;
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Vehicle v in EntityMgr.list)
            {
                //if (null != _tr_target)
                    //v._target = _tr_target.position;

                v.Update();
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
        public SteeringBehavior.eType _mode = SteeringBehavior.eType.none;

        Vector3[] _array_VB = new Vector3[3];

        public SteeringBehavior _steeringBehavior = new SteeringBehavior();

        public void Init()
        {
            _array_VB[0] = new Vector3(0.0f, 0, 1f);
            _array_VB[1] = new Vector3(0.6f, 0, -1f);
            _array_VB[2] = new Vector3(-0.6f, 0, -1f);

            _steeringBehavior._vehicle = this;
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

            if(_isNonpenetration)
                EnforceNonPenetrationConstraint(this, EntityMgr.list);

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
            Vector3 vb0, vb1, vb2;
            vb0 = _rotatioin * _array_VB[0] * _size.z;
            vb1 = _rotatioin * _array_VB[1] * _size.z;
            vb2 = _rotatioin * _array_VB[2] * _size.z;

            //에이젼트 출력 
            DebugWide.DrawLine(_pos + vb0, _pos + vb1, color);
            DebugWide.DrawLine(_pos + vb1, _pos + vb2, color);
            DebugWide.DrawLine(_pos + vb2, _pos + vb0, color);
            //DebugWide.DrawCircle(_pos, 1f * _size.z, color); //test

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
            pursuit = 0x00800, //이동위치 예측 추격
            evade = 0x01000, //추격위치 예측 회피
            interpose = 0x02000,
            hide = 0x04000,
            flock = 0x08000,
            offset_pursuit = 0x10000,
        }

        public enum Deceleration { slow = 3, normal = 2, fast = 1 };

        public Vehicle _vehicle;

        public Vector3 _steeringForce = Vector3.zero;

        //public Vector3 _target;

        //==================================================

        public Vector3 Seek(Vector3 TargetPos)
        {
            Vector3 DesiredVelocity = (TargetPos - _vehicle._pos).normalized
                            * _vehicle._maxSpeed;


            return (DesiredVelocity - _vehicle._velocity);
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

        public float _maxForce = 2 * 200;
        public float _ViewDistance = 30;

        public float _WeightSeparation = 1 * 200;
        public float _WeightAlignment = 1 * 200;
        public float _WeightCohesion = 2 * 200;

        public float _WanderWeight = 1 * 200f;
        public float _PursuitWeight = 1 * 200f;
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

            if(0 == _vehicle._id)
                DrawTagNeighbors(_vehicle);

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

            _steeringForce += Separation(EntityMgr.list) * _WeightSeparation;
            //DebugWide.LogBlue(_vehicle._id + " Separation : " + force);
            _steeringForce += Alignment(EntityMgr.list) * _WeightAlignment;
            _steeringForce += Cohesion(EntityMgr.list) * _WeightCohesion;
            _steeringForce += Wander() * _WanderWeight;
            //DebugWide.LogBlue(_vehicle._id+ " wander : " +force);

            _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

            return _steeringForce;
        }

        public Vector3 CalculatePrioritized()
        {
            _steeringForce = Vector3.zero;

            Vector3 force = Vector3.zero;

            force = Separation(EntityMgr.list) * _WeightSeparation;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;

            force = Alignment(EntityMgr.list) * _WeightAlignment;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;

            force = Cohesion(EntityMgr.list) * _WeightCohesion;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;

            force = Wander() * _WanderWeight;
            if (!AccumulateForce(ref _steeringForce, force)) return _steeringForce;
            //DebugWide.LogBlue(_vehicle._id+ " wander : " +force);

            return _steeringForce;
        }


        public float prSeparation = 0.2f;
        public float prAlignment = 0.3f;
        public float prCohesion = 0.6f;
        public float prWander = 0.8f;
        public Vector3 CalculateDithered()
        {
            //reset the steering force
            _steeringForce = Vector3.zero;

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
            if (Misc.RandFloat() < prWander)
            {
                _steeringForce += Wander() * _WanderWeight / prWander;

                if (!Misc.IsZero(_steeringForce))
                {
                    _steeringForce = VOp.Truncate(_steeringForce, _maxForce);

                    return _steeringForce;
                }
            }

            return _steeringForce;
        }

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
                if(curEntity._tag)
                {
                    DebugWide.DrawLine(entity._pos, curEntity._pos, Color.green);
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

                AverageHeading -= _vehicle._heading;
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

            //the magnitude of cohesion is usually much larger than separation or
            //allignment so it usually helps to normalize it.
            return SteeringForce.normalized;
        }
    }
}

