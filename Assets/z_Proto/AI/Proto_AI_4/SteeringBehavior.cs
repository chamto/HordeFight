using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UtilGS9;

namespace Proto_AI_4
{
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
        //Deceleration m_Deceleration;

        public Unit _vehicle;

        public Vector3 _steeringForce;

        //these can be used to keep track of friends, pursuers, or prey
        public Unit _pTargetAgent1;
        public Unit _pTargetAgent2; //인터포즈(둘 사이에 끼기)에서 사용 

        //the current target
        public Vector3 _target;

        //length of the 'detection box' utilized in obstacle avoidance
        public float _detectBoxLength;

        public float _decelerationTime = 0.3f; //Arrive2 알고리즘에서 사용 , 남은거리를 몇초로 이동할지 설정 

        //a vertex buffer to contain the feelers rqd for wall avoidance  
        //List<Vector3> m_Feelers;

        //the length of the 'feeler/s' used in wall detection
        //float _wallDetectionFeelerLength;



        //the current position on the wander circle the agent is
        //attempting to steer towards
        public Vector3 _wanderTarget;

        //explained above
        public float _wanderJitter;
        public float _wanderRadius;
        public float _wanderDistance;


        //multipliers. These can be adjusted to effect strength of the  
        //appropriate behavior. Useful to get flocking the way you require
        //for example.
        public float _weightSeparation;
        public float _weightCohesion;
        public float _weightAlignment;
        public float _weightWander;
        public float _weightObstacleAvoidance;
        public float _weightWallAvoidance;
        public float _weightSeek;
        public float _weightFlee;
        public float _weightArrive;
        public float _weightPursuit;
        public float _weightOffsetPursuit;
        public float _weightInterpose;
        public float _weightHide;
        public float _weightEvade;
        public float _weightFollowPath;

        //how far the agent can 'see'
        public float _viewDistance;

        //pointer to any current path
        //Path m_pPath;

        //the distance (squared) a vehicle has to be from a path waypoint before
        //it starts seeking to the next waypoint
        public float _waypointSeekDistSq;


        //any offset used for formations or offset pursuit
        public Vector3 _offset;

        //binary flags to indicate whether or not a behavior should be active
        private int _flags;

        public SummingMethod _summingMethod;

        //==================================================

        public void SetSummingMethod(SummingMethod sm) { _summingMethod = sm; }


        public void FleeOn() { _flags |= (int)eType.flee; }
        public void SeekOn() { _flags |= (int)eType.seek; }
        public void ArriveOn() { _flags |= (int)eType.arrive; }
        public void WanderOn() { _flags |= (int)eType.wander; }
        public void PursuitOn(Unit v) { _flags |= (int)eType.pursuit; _pTargetAgent1 = v; }
        public void EvadeOn(Unit v) { _flags |= (int)eType.evade; _pTargetAgent1 = v; }
        public void CohesionOn() { _flags |= (int)eType.cohesion; }
        public void SeparationOn() { _flags |= (int)eType.separation; }
        public void AlignmentOn() { _flags |= (int)eType.allignment; }
        public void ObstacleAvoidanceOn() { _flags |= (int)eType.obstacle_avoidance; }
        public void WallAvoidanceOn() { _flags |= (int)eType.wall_avoidance; }
        public void FollowPathOn() { _flags |= (int)eType.follow_path; }
        public void InterposeOn(Unit v1, Unit v2) { _flags |= (int)eType.interpose; _pTargetAgent1 = v1; _pTargetAgent2 = v2; }
        public void HideOn(Unit v) { _flags |= (int)eType.hide; _pTargetAgent1 = v; }
        public void OffsetPursuitOn(Unit v1, Vector2 offset) { _flags |= (int)eType.offset_pursuit; _offset = offset; _pTargetAgent1 = v1; }
        public void FlockingOn() { CohesionOn(); AlignmentOn(); SeparationOn(); WanderOn(); }

        public void AllOff() { _flags = (int)eType.none; }
        public void FleeOff() { if (On(eType.flee)) _flags ^= (int)eType.flee; }
        public void SeekOff() { if (On(eType.seek)) _flags ^= (int)eType.seek; }
        public void ArriveOff() { if (On(eType.arrive)) _flags ^= (int)eType.arrive; }
        public void WanderOff() { if (On(eType.wander)) _flags ^= (int)eType.wander; }
        public void PursuitOff() { if (On(eType.pursuit)) _flags ^= (int)eType.pursuit; }
        public void EvadeOff() { if (On(eType.evade)) _flags ^= (int)eType.evade; }
        public void CohesionOff() { if (On(eType.cohesion)) _flags ^= (int)eType.cohesion; }
        public void SeparationOff() { if (On(eType.separation)) _flags ^= (int)eType.separation; }
        public void AlignmentOff() { if (On(eType.allignment)) _flags ^= (int)eType.allignment; }
        public void ObstacleAvoidanceOff() { if (On(eType.obstacle_avoidance)) _flags ^= (int)eType.obstacle_avoidance; }
        public void WallAvoidanceOff() { if (On(eType.wall_avoidance)) _flags ^= (int)eType.wall_avoidance; }
        public void FollowPathOff() { if (On(eType.follow_path)) _flags ^= (int)eType.follow_path; }
        public void InterposeOff() { if (On(eType.interpose)) _flags ^= (int)eType.interpose; }
        public void HideOff() { if (On(eType.hide)) _flags ^= (int)eType.hide; }
        public void OffsetPursuitOff() { if (On(eType.offset_pursuit)) _flags ^= (int)eType.offset_pursuit; }
        public void FlockingOff() { CohesionOff(); AlignmentOff(); SeparationOff(); WanderOff(); }

        bool On(eType bt) { return (_flags & (int)bt) == (int)bt; }

        bool AccumulateForce(ref Vector3 RunningTot, Vector3 ForceToAdd)
        {
            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = RunningTot.magnitude;

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = _vehicle._maxForce - MagnitudeSoFar;

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

        //==================================================

        Vector3 CalculateWeightedSum()
        {
            if (On(eType.wall_avoidance))
            {
                //m_vSteeringForce += WallAvoidance(m_pVehicle.World().Walls()) * m_dWeightWallAvoidance;
            }

            if (On(eType.obstacle_avoidance))
            {
                //m_vSteeringForce += ObstacleAvoidance(m_pVehicle.World().Obstacles()) * m_dWeightObstacleAvoidance;
            }

            if (On(eType.evade))
            {
                //m_vSteeringForce += Evade(m_pTargetAgent1) * m_dWeightEvade;
            }


            if (On(eType.separation))
            {
                //_steeringForce += Separation(m_pVehicle.World().Agents()) * m_dWeightSeparation;
            }

            if (On(eType.allignment))
            {
                //_steeringForce += Alignment(m_pVehicle.World().Agents()) * m_dWeightAlignment;
            }

            if (On(eType.cohesion))
            {
                //_steeringForce += Cohesion(m_pVehicle.World().Agents()) * m_dWeightCohesion;
            }



            if (On(eType.wander))
            {
                //_steeringForce += Wander() * m_dWeightWander;
            }

            if (On(eType.seek))
            {
                //_steeringForce += Seek(m_pVehicle.World().Crosshair()) * m_dWeightSeek;
            }

            if (On(eType.flee))
            {
                //_steeringForce += Flee(m_pVehicle.World().Crosshair()) * m_dWeightFlee;
            }

            if (On(eType.arrive))
            {
                //** 등속도이동 설정
                //_decelerationTime = 0.09f; //0.09초동안 감속 - 감속되는 것을 보여주지 않기 위해 짧게 설정 
                //_weight = 50;
                //_maxForce = 100;
                //_steeringForce += Arrive(m_pVehicle.World().Crosshair(), m_Deceleration) * m_dWeightArrive;
            }

            if (On(eType.pursuit))
            {
                //_steeringForce += Pursuit(m_pTargetAgent1) * m_dWeightPursuit;
            }

            if (On(eType.offset_pursuit))
            {
                _steeringForce += OffsetPursuit(_pTargetAgent1, _offset) * _weightOffsetPursuit;
            }

            if (On(eType.interpose))
            {
                //_steeringForce += Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_dWeightInterpose;
            }

            if (On(eType.hide))
            {
                //_steeringForce += Hide(m_pTargetAgent1, m_pVehicle.World().Obstacles()) * m_dWeightHide;
            }

            if (On(eType.follow_path))
            {
                //_steeringForce += FollowPath() * m_dWeightFollowPath;
            }

            _steeringForce = VOp.Truncate(_steeringForce, _vehicle._maxForce);

            return _steeringForce;
        }

        //---------------------- CalculatePrioritized ----------------------------
        //
        //  this method calls each active steering behavior in order of priority
        //  and acumulates their forces until the max steering force magnitude
        //  is reached, at which time the function returns the steering force 
        //  accumulated to that  point
        //------------------------------------------------------------------------
        Vector2 CalculatePrioritized()
        {
            Vector2 force;

            if (On(eType.wall_avoidance))
            {
                //force = WallAvoidance(m_pVehicle.World().Walls()) * m_dWeightWallAvoidance;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.obstacle_avoidance))
            {
                //force = ObstacleAvoidance(m_pVehicle.World().Obstacles()) * m_dWeightObstacleAvoidance;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.evade))
            {
                //force = Evade(m_pTargetAgent1) * m_dWeightEvade;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(eType.flee))
            {
                //force = Flee(m_pVehicle.World().Crosshair()) * m_dWeightFlee;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }



            if (On(eType.separation))
            {
                //force = Separation(m_pVehicle.World().Agents()) * m_dWeightSeparation;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.allignment))
            {
                //force = Alignment(m_pVehicle.World().Agents()) * m_dWeightAlignment;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.cohesion))
            {
                //force = Cohesion(m_pVehicle.World().Agents()) * m_dWeightCohesion;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.seek))
            {
                //force = Seek(m_pVehicle.World().Crosshair()) * m_dWeightSeek;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(eType.arrive))
            {
                //force = Arrive(m_pVehicle.World().Crosshair(), m_Deceleration) * m_dWeightArrive;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.wander))
            {
                //force = Wander() * m_dWeightWander;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.pursuit))
            {
                //force = Pursuit(m_pTargetAgent1) * m_dWeightPursuit;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.offset_pursuit))
            {
                //force = OffsetPursuit(m_pTargetAgent1, m_vOffset);
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.interpose))
            {
                //force = Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_dWeightInterpose;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.hide))
            {
                //force = Hide(m_pTargetAgent1, m_pVehicle.World().Obstacles()) * m_dWeightHide;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(eType.follow_path))
            {
                //force = FollowPath() * m_dWeightFollowPath;
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            return _steeringForce;
        }

        public Vector3 Calculate()
        {
            //reset the steering force
            _steeringForce = Vector3.zero;

            if (On(eType.separation) || On(eType.allignment) || On(eType.cohesion))
            {
                //m_pVehicle.World().TagVehiclesWithinViewRange(m_pVehicle, m_dViewDistance);
            }

            //DebugWide.LogBlue(m_SummingMethod + "  " + m_pVehicle.Speed());
            switch (_summingMethod)
            {
                case SummingMethod.weighted_average:

                    _steeringForce = CalculateWeightedSum(); break;

                case SummingMethod.prioritized:

                    _steeringForce = CalculatePrioritized(); break;

                case SummingMethod.dithered:

                    //_steeringForce = CalculateDithered(); break;

                default: _steeringForce = Vector3.zero; break;

            }//end switch

            return _steeringForce;
        }

        //calculates the component of the steering force that is parallel
        //with the vehicle heading
        //public float ForwardComponent()
        //{
        //    return Vector3.Dot(_vehicle._heading, _steeringForce);
        //}

        //calculates the component of the steering force that is perpendicuar
        //with the vehicle heading
        //public float SideComponent()
        //{
        //    return Vector3.Dot(_vehicle.Side(), m_vSteeringForce);
        //}

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

        public Vector3 Arrive2(Vector3 TargetPos)
        {
            Vector3 ToTarget = TargetPos - _vehicle._pos;

            //calculate the distance to the target
            float dist = ToTarget.magnitude;

            if (dist > 0) //0으로 나누는 것에 대한 예외처리 , 기존 최소값 지정으로 인해 떠는 문제 있었음 
            {

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
                float speed = dist / _decelerationTime; //v = s / t

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

        public Vector3 OffsetPursuit(BaseEntity leader, Vector3 offset)
        {
            //calculate the offset's position in world space
            Vector3 WorldOffsetPos = (leader._rotation * offset) + leader._pos; //PointToWorldSpace

            Vector3 ToOffset = WorldOffsetPos - _vehicle._pos;

            //최소거리 이하로 계산됐다면 처리하지 않는다 - 이상회전 문제 때문에 예외처리함 
            //const float MinLen = 0.1f;
            //const float SqrMinLen = MinLen * MinLen;
            //if (ToOffset.sqrMagnitude < SqrMinLen)
            //return ConstV.v3_zero;

            //DebugWide.AddDrawQ_Circle(WorldOffsetPos, 0.1f, Color.red);


            //속도공식 : v = s / t
            //A------> <------B 두 점이 직선상 한점에서 만나는 시간구하기 : t = s / v
            //1차원 직선상에 A와 B 두 점이 있다고 할시, A와 B의 두 사이거리는 S 가 된다 
            //A의 속도 1 , B의 속도 1  , S 가 9 일 경우 , t = 9 / (1+1) = 4.5 
            //A의 속도 2 , B의 속도 1  , S 가 9 일 경우 , t = 9 / (2+1) = 3
            // * 검산 : s = v * t
            //(1 * 4.5) + (1 * 4.5) = 4.5 + 4.5 = 9 : A가 4.5거리 만큼 이동후 B와 만나게 된다 
            //(2 * 3) + (1 * 3) = 6 + 3 = 9 : A가 6거리 만큼 이동후 B와 만나게 된다 
            float LookAheadTime = ToOffset.magnitude /
                                  (_vehicle._maxSpeed + leader._speed);

            //LookAheadTime += TurnAroundTime(_vehicle, WorldOffsetPos, 1); //이렇게 턴시간 늘리는 것은 아닌것 같음 

            //------------------------
            //Vector3 prPos = WorldOffsetPos + leader._velocity * LookAheadTime;
            //DebugWide.AddDrawQ_Circle(prPos, 0.1f, Color.green);
            //DebugWide.LogBlue(LookAheadTime);
            //------------------------

            //now Arrive at the predicted future position of the offset
            //return Arrive(WorldOffsetPos + leader._velocity * LookAheadTime, Deceleration.fast); //s = v * t 
            return Arrive2(WorldOffsetPos + leader._velocity * LookAheadTime); //s = v * t 

        }

        public float TurnAroundTime(Unit agent, Vector3 targetPos, float turnSecond)
        {
            Vector3 toTarget = (targetPos - agent._pos).normalized;

            float dot = Vector3.Dot(agent._heading, toTarget);

            float coefficient = 0.5f * turnSecond; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 1초
            //const float coefficient = 0.5f * 5; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 5초

            return (dot - 1f) * -coefficient; //[-2 ~ 0] * -coefficient
        }

        //---------------------------- Separation --------------------------------
        //
        // this calculates a force repelling from the other neighbors
        //------------------------------------------------------------------------
        Vector3 Separation(List<Unit> neighbors)
        {
            Vector3 SteeringForce = Vector3.zero;

            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough. ***also make sure it doesn't
                //include the evade target ***
                if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag &&
                  (neighbors[a] != _pTargetAgent1))
                {
                    Vector3 ToAgent = _vehicle._pos - neighbors[a]._pos;

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    //toAgent 가 0이 되면 Nan 값이 되어 , Nan과 연산한 다른 변수도 Nan이 되어버리는 문제가 있다 
                    if (false == Misc.IsZero(ToAgent))
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
        Vector3 Alignment(List<Unit> neighbors)
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
                  (neighbors[a] != _pTargetAgent1))
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

        Vector3 Cohesion(List<Unit> neighbors)
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
                  (neighbors[a] != _pTargetAgent1))
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
    }

}//end namespace



