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
            seek = 1 << 0,
            flee = 1 << 1,
            arrive = 1 << 2,
            wander = 1 << 3,
            cohesion = 1 << 4,
            separation = 1 << 5,
            allignment = 1 << 6,
            obstacle_avoidance = 1 << 7,
            wall_avoidance = 1 << 8,
            follow_path = 1 << 9,
            pursuit = 1 << 10,
            evade = 1 << 11,
            interpose = 1 << 12,
            hide = 1 << 13,
            flock = 1 << 14,
            offset_pursuit = 1 << 15,
            follow = 1 << 16,
        }

        public enum Deceleration { slow = 3, normal = 2, fast = 1 };
        //Deceleration m_Deceleration;

        public Unit _vehicle;

        public Vector3 _steeringForce;

        //these can be used to keep track of friends, pursuers, or prey
        public Unit _pTargetAgent;

        //public BaseEntity _pOrderPoint;

        //인터포즈(둘 사이에 끼기)에서 사용 
        public Unit _pTargetInterpose1;
        public Unit _pTargetInterpose2;

        //the current target
        //public Vector3 _targetPos;

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


        public float _steeringForceTweaker = 1;

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
        public float _weightFollow;

        //how far the agent can 'see'
        //public float _viewDistance;

        //pointer to any current path
        //Path m_pPath;

        //the distance (squared) a vehicle has to be from a path waypoint before
        //it starts seeking to the next waypoint
        public float _waypointSeekDistSq;


        //any offset used for formations or offset pursuit
        //public Vector3 _offset;

        //binary flags to indicate whether or not a behavior should be active
        private int _flags;

        public SummingMethod _summingMethod;

        //==================================================

        public void SetSummingMethod(SummingMethod sm) { _summingMethod = sm; }


        public void FleeOn() { _flags |= (int)eType.flee; }
        public void SeekOn() { _flags |= (int)eType.seek; }
        public void ArriveOn() { _flags |= (int)eType.arrive; }
        public void WanderOn() { _flags |= (int)eType.wander; }
        public void PursuitOn(Unit v) { _flags |= (int)eType.pursuit; _pTargetAgent = v; }
        public void EvadeOn(Unit v) { _flags |= (int)eType.evade; _pTargetAgent = v; }
        public void CohesionOn() { _flags |= (int)eType.cohesion; }
        public void SeparationOn() { _flags |= (int)eType.separation; }
        public void AlignmentOn() { _flags |= (int)eType.allignment; }
        public void ObstacleAvoidanceOn() { _flags |= (int)eType.obstacle_avoidance; }
        public void WallAvoidanceOn() { _flags |= (int)eType.wall_avoidance; }
        public void FollowPathOn() { _flags |= (int)eType.follow_path; }
        public void InterposeOn(Unit v1, Unit v2) { _flags |= (int)eType.interpose; _pTargetInterpose1 = v1; _pTargetInterpose2 = v2; }
        public void HideOn(Unit v) { _flags |= (int)eType.hide; _pTargetAgent = v; }
        //public void OffsetPursuitOn(BaseEntity orderPoint, Vector3 offset) { _flags |= (int)eType.offset_pursuit; _offset = offset; _pOrderPoint = orderPoint; }
        public void OffsetPursuitOn() { _flags |= (int)eType.offset_pursuit; }
        public void FlockingOn() { CohesionOn(); AlignmentOn(); SeparationOn(); WanderOn(); }
        public void FollowOn() { _flags |= (int)eType.follow; }

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
        public void FollowOff() { if (On(eType.follow)) _flags ^= (int)eType.follow; }

        public bool IsFleeOn() { return On(eType.flee); }
        public bool IsSeekOn() { return On(eType.seek); }
        public bool IsArriveOn() { return On(eType.arrive); }
        public bool IsWanderOn() { return On(eType.wander); }
        public bool IsPursuitOn() { return On(eType.pursuit); }
        public bool IsEvadeOn() { return On(eType.evade); }
        public bool IsCohesionOn() { return On(eType.cohesion); }
        public bool IsSeparationOn() { return On(eType.separation); }
        public bool IsAlignmentOn() { return On(eType.allignment); }
        public bool IsObstacleAvoidanceOn() { return On(eType.obstacle_avoidance); }
        public bool IsWallAvoidanceOn() { return On(eType.wall_avoidance); }
        public bool IsFollowPathOn() { return On(eType.follow_path); }
        public bool IsInterposeOn() { return On(eType.interpose); }
        public bool IsHideOn() { return On(eType.hide); }
        public bool IsOffsetPursuitOn() { return On(eType.offset_pursuit); }
        public bool IsFlockingOn() { return On(eType.cohesion | eType.separation | eType.allignment | eType.wander); }
        public bool IsFollowOn() { return On(eType.follow); }

        private bool On(eType bt) { return (_flags & (int)bt) == (int)bt; }

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
                _steeringForce += ObstacleAvoidance(EntityMgr.list) * _weightObstacleAvoidance;
            }

            if (On(eType.evade))
            {
                //m_vSteeringForce += Evade(m_pTargetAgent1) * m_dWeightEvade;
            }


            if (On(eType.separation))
            {
                _steeringForce += Separation2(_vehicle._sight.list_around, _vehicle._flocking.separation_distance) * _weightSeparation;
                //_steeringForce += Separation(_vehicle._sight.list_around) * _weightSeparation;
            }

            if (On(eType.allignment))
            {
                //_steeringForce += Alignment(EntityMgr.list) * _weightAlignment;
                _steeringForce += Alignment(_vehicle._sight.list_around) * _weightAlignment;
            }

            if (On(eType.cohesion))
            {
                //_steeringForce += Cohesion(EntityMgr.list) * _weightCohesion ;
                _steeringForce += Cohesion(_vehicle._sight.list_around) * _weightCohesion;
            }

            if (On(eType.follow))
            {
                if (null != _vehicle._sight.near_unit)
                //if (null != _vehicle._sight.far_unit)
                {
                    _steeringForce += Follow2(_vehicle._sight.near_unit._pos, _vehicle._disposition._offset) * _weightFollow;
                    //_steeringForce += Follow(_vehicle._sight.far_unit._pos, _vehicle._flocking.follow_distance) * _weightFollow;

                    //_steeringForce += OffsetPursuit(_vehicle._sight.near_unit, _vehicle._disposition._offset) * _weightFollow;
                }

                //chamto test
                //if (null != _vehicle._sight.near_unit)
                //{
                //    if( (_vehicle._sight.near_unit._pos - _vehicle._pos).sqrMagnitude > _vehicle._flocking.follow_distance * _vehicle._flocking.follow_distance)
                //    {
                //        _steeringForce += Stop();
                //    }
                //}
            }

            if (On(eType.wander))
            {
                //_steeringForce += Wander() * m_dWeightWander;
            }

            if (On(eType.seek))
            {
                _steeringForce += Seek(_vehicle._targetPos) * _weightSeek;
                //DebugWide.LogBlue(_steeringForce.magnitude + "  " + _vehicle._targetPos);
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
                _steeringForce += Arrive2(_vehicle._targetPos) * _weightArrive;
            }

            if (On(eType.pursuit))
            {
                //_steeringForce += Pursuit(m_pTargetAgent1) * m_dWeightPursuit;
            }

            if (On(eType.offset_pursuit))
            {
                _steeringForce += OffsetPursuit(_vehicle._disposition._belong_formation, _vehicle._disposition._offset) * _weightOffsetPursuit;
            }

            if (On(eType.interpose))
            {
                //_steeringForce += Interpose(_pTargetInterpose1, _pTargetInterpose1) * _weightInterpose;
            }

            if (On(eType.hide))
            {
                //_steeringForce += Hide(m_pTargetAgent1, m_pVehicle.World().Obstacles()) * m_dWeightHide;
            }

            if (On(eType.follow_path))
            {
                //_steeringForce += FollowPath() * m_dWeightFollowPath;
            }

            return _steeringForce;
        }

        //---------------------- CalculatePrioritized ----------------------------
        //
        //  this method calls each active steering behavior in order of priority
        //  and acumulates their forces until the max steering force magnitude
        //  is reached, at which time the function returns the steering force 
        //  accumulated to that  point
        //------------------------------------------------------------------------
        Vector3 CalculatePrioritized()
        {
            Vector3 force;

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
                //force = OffsetPursuit(_pOrderPoint, m_vOffset);
                //if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.interpose))
            {
                //force = Interpose(_pTargetInterpose1, _pTargetInterpose1) * m_dWeightInterpose;
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
            _steeringForce = ConstV.v3_zero;

            //if (On(eType.separation) || On(eType.allignment) || On(eType.cohesion))
            //{
            //    TagNeighbors(_vehicle,EntityMgr.list , _viewDistance);
            //}

            //DebugWide.LogBlue(m_SummingMethod + "  " + m_pVehicle.Speed());
            switch (_summingMethod)
            {
                case SummingMethod.weighted_average:

                    _steeringForce = CalculateWeightedSum(); break;

                case SummingMethod.prioritized:

                    _steeringForce = CalculatePrioritized(); break;

                case SummingMethod.dithered:

                    //_steeringForce = CalculateDithered(); break;

                default: break;

            }//end switch

            return _steeringForce * _steeringForceTweaker;
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

                //_decelerationTime 이 작을수록 속도가 크게 계산된다 
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

        public Vector3 Stop()
        {
            return ( - _vehicle._velocity);
        }

        //최소속도보다 작다면 참을 반환 
        public bool IsItVelo_LessThan(float min_velo)
        {
            return _vehicle._velocity.sqrMagnitude < min_velo * min_velo;
        }

        private Vector3 OffsetPursuit(BaseEntity leader, Vector3 offset)
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

            //* 두 객체가 만날때의 시간 구하기  
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

            //Vector3 offsetPos = WorldOffsetPos + leader._velocity * LookAheadTime;
            ////계산된 오프셋위치가 운반기의 뒤에 있는 경우 
            //if (0 > Vector3.Dot(_vehicle._heading, (offsetPos - _vehicle._pos)))
            //{
            //    //return Stop(); 
            //    return ConstV.v3_zero;
            //}

            //WorldOffsetPos : 현재시간에서의 오프셋리더위치
            //목표가 움직이는 경우 , WorldOffsetPos 만으로는 목표위치에 도달 할 수 없는 문제가 있음 
            //두 객체가 움직일때 동시에 만나는 시점의 리더위치 : 현재시간에서의 오프셋리더위치 + 동시에 만나는 시점의 거리  
            //now Arrive at the predicted future position of the offset
            //return Arrive(WorldOffsetPos + leader._velocity * LookAheadTime, Deceleration.fast); //s = v * t 
            return Arrive2(WorldOffsetPos + leader._velocity * LookAheadTime); //s = v * t 

        }

        //제거대상 , 월드오프셋위치를 타겟포스로 사용함 
        //private Vector3 OffsetPursuit2(BaseEntity leader, Vector3 offset)
        //{
        //    //calculate the offset's position in world space
        //    //Vector3 WorldOffsetPos = (leader._rotation * offset) + leader._pos; //PointToWorldSpace
        //    Vector3 WorldOffsetPos = _targetPos;

        //    Vector3 ToOffset = WorldOffsetPos - _vehicle._pos;


        //    //속도공식 : v = s / t
        //    //A------> <------B 두 점이 직선상 한점에서 만나는 시간구하기 : t = s / v
        //    //1차원 직선상에 A와 B 두 점이 있다고 할시, A와 B의 두 사이거리는 S 가 된다 
        //    //A의 속도 1 , B의 속도 1  , S 가 9 일 경우 , t = 9 / (1+1) = 4.5 
        //    //A의 속도 2 , B의 속도 1  , S 가 9 일 경우 , t = 9 / (2+1) = 3
        //    // * 검산 : s = v * t
        //    //(1 * 4.5) + (1 * 4.5) = 4.5 + 4.5 = 9 : A가 4.5거리 만큼 이동후 B와 만나게 된다 
        //    //(2 * 3) + (1 * 3) = 6 + 3 = 9 : A가 6거리 만큼 이동후 B와 만나게 된다 
        //    float LookAheadTime = ToOffset.magnitude /
        //                          (_vehicle._maxSpeed + leader._speed);


        //    //now Arrive at the predicted future position of the offset
        //    //return Arrive(WorldOffsetPos + leader._velocity * LookAheadTime, Deceleration.fast); //s = v * t 
        //    return Arrive2(WorldOffsetPos + leader._velocity * LookAheadTime); //s = v * t 

        //}

        public float TurnAroundTime(Unit agent, Vector3 targetPos, float turnSecond)
        {
            Vector3 toTarget = (targetPos - agent._pos).normalized;

            float dot = Vector3.Dot(agent._heading, toTarget);

            float coefficient = 0.5f * turnSecond; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 1초
            //const float coefficient = 0.5f * 5; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 5초

            return (dot - 1f) * -coefficient; //[-2 ~ 0] * -coefficient
        }

        //offset.x 값을 지정하면 고정된 위치지정이 안된다. 기준위치에서 x이동하여 기준위치가 계속 이동된다
        //정지상태에서는 고정된 위치지정 안됨 , 이동상태에서는 어느정도 고정된 위치가 잡힌다   
        private Vector3 Follow2(Vector3 targetPos, Vector3 offset)
        {

            Vector3 ndir_z = VOp.Normalize(targetPos - _vehicle._pos);
            Vector3 ndir_x = Quaternion.AngleAxis(90, ConstV.v3_up) * ndir_z;

            Vector3 offsetPos = targetPos + ndir_z * offset.z;
            offsetPos += ndir_x * offset.x;

            //계산된 오프셋위치가 운반기의 뒤에 있는 경우 
            if (0 > Vector3.Dot(_vehicle._heading, (offsetPos - _vehicle._pos)))
            {
                //return Stop(); 
                return ConstV.v3_zero; 
            }

            return Arrive2(offsetPos);
        }

        private Vector3 Follow(Vector3 targetPos, float dis)
        {

            Vector3 ndir = VOp.Normalize(_vehicle._pos - targetPos);

            Vector3 offsetPos = targetPos + ndir * dis;

            //계산된 오프셋위치가 운반기의 뒤에 있는 경우 
            if (0 > Vector3.Dot(_vehicle._heading, (offsetPos - _vehicle._pos)))
            {
                //return Stop(); 
                return ConstV.v3_zero;
            }

            return Arrive2(offsetPos);
        }

        //거리지정이 있는 분리알고리즘
        Vector3 Separation2(List<Unit> neighbors , float max_dis)
        {

            int testCount = 0;
            Vector3 steeringForce = ConstV.v3_zero;

            for (int a = 0; a < neighbors.Count; ++a)
            {

                if ((neighbors[a] != _vehicle) && (neighbors[a] != _pTargetAgent))
                {
                    Vector3 ToAgent = _vehicle._pos - neighbors[a]._pos; //주변객체로 부터 떨어지는 방향 

                    if (ToAgent.sqrMagnitude > max_dis * max_dis) continue; 


                    //toAgent 가 0이 되면 Nan 값이 되어 , Nan과 연산한 다른 변수도 Nan이 되어버리는 문제가 있다 
                    if (false == Misc.IsZero(ToAgent))
                    {
                        //if (0 == _vehicle._id)
                        //DebugWide.LogBlue(a + " - " + SteeringForce);
                        steeringForce += ToAgent.normalized / ToAgent.magnitude;
                        // toAgent 의 길이가 1 일때 기준 조종힘 1이 된다 
                        // 길이 < 1 : 조종힘이 커진다 
                        // 1 < 길이 : 조종힘이 작아진다. 거리가 멀수록 힘이 작아진다  

                        testCount++;
                    }

                }
            }


            if (0 == _vehicle._id)
            {
                DebugWide.AddDrawQ_Line(_vehicle._pos, _vehicle._pos + steeringForce, Color.green);
                DebugWide.AddDrawQ_Circle(_vehicle._pos , max_dis, Color.green);
                DebugWide.AddDrawQ_Circle(_vehicle._pos + steeringForce.normalized * max_dis, 0.2f, Color.green);
            }

            //if(0 == testCount)
            //{
            //    if(false == IsItVelo_LessThan(0.1f))
            //    {
            //        return Stop(); //분리를 멈춘다          
            //    }
            //}

                
            return steeringForce;
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
                //if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag && (neighbors[a] != _pTargetAgent))
                if ((neighbors[a] != _vehicle) && (neighbors[a] != _pTargetAgent))
                {
                    Vector3 ToAgent = _vehicle._pos - neighbors[a]._pos; //주변객체로 부터 떨어지는 방향 

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    //toAgent 가 0이 되면 Nan 값이 되어 , Nan과 연산한 다른 변수도 Nan이 되어버리는 문제가 있다 
                    if (false == Misc.IsZero(ToAgent))
                    {
                        //if (0 == _vehicle._id)
                            //DebugWide.LogBlue(a + " - " + SteeringForce);
                        SteeringForce += ToAgent.normalized / ToAgent.magnitude;
                        // toAgent 의 길이가 1 일때 기준 조종힘 1이 된다 
                        // 길이 < 1 : 조종힘이 커진다 
                        // 1 < 길이 : 조종힘이 작아진다. 거리가 멀수록 힘이 작아진다  
                    }

                }
            }


            //if (0 > Vector3.Dot(_vehicle._heading, SteeringForce))
            //{
            //    SteeringForce = Stop(); //chamto test , 반향이 반대인 경우 정지힘을 준다 
            //}

            if (0 == _vehicle._id)
            {
                DebugWide.AddDrawQ_Line(_vehicle._pos, _vehicle._pos + SteeringForce, Color.green);
                DebugWide.AddDrawQ_Circle(_vehicle._pos + SteeringForce, 0.2f, Color.green);
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
                //if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag && (neighbors[a] != _pTargetAgent))
                if ((neighbors[a] != _vehicle) && (neighbors[a] != _pTargetAgent))
                {
                    AverageHeading += neighbors[a]._heading; //heading 은 길이가 1인 벡터 , 벡터의 합은 평균방향을 가리킨다 

                    ++NeighborCount; 
                }
            }

            //if the neighborhood contained one or more vehicles, average their
            //heading vectors.
            if (NeighborCount > 0)
            {
                AverageHeading /= (float)NeighborCount; //무게중심좌표를 구한다. 방향이 균일하다면 0좌표가 나올수도 있다

                AverageHeading -= _vehicle._heading; //seek 방향힘 구하는 방식
                //DesiredVelocity - _vehicle._heading 
            }

            //chamto test
            if (0 == _vehicle._id)
            {

                DebugWide.AddDrawQ_Line(_vehicle._pos, _vehicle._pos + AverageHeading, Color.blue);
                DebugWide.AddDrawQ_Circle(_vehicle._pos + AverageHeading, 0.2f, Color.blue);
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
                //if ((neighbors[a] != _vehicle) && true == neighbors[a]._tag && (neighbors[a] != _pTargetAgent))
                if ((neighbors[a] != _vehicle) && (neighbors[a] != _pTargetAgent))
                {
                    CenterOfMass += neighbors[a]._pos;

                    ++NeighborCount;
                }
            }

            if (NeighborCount > 0)
            {
                //the center of mass is the average of the sum of positions
                CenterOfMass /= (float)NeighborCount; //무게중심좌표를 구한다.

                //now seek towards that position
                SteeringForce = Seek(CenterOfMass);
            }

            if (0 == _vehicle._id && NeighborCount > 0)
            {
                DebugWide.AddDrawQ_Line(_vehicle._pos, _vehicle._pos + SteeringForce.normalized, Color.green);
                DebugWide.AddDrawQ_Circle(_vehicle._pos + SteeringForce.normalized, 0.2f, Color.green);
                DebugWide.AddDrawQ_Circle(CenterOfMass, 0.5f, Color.red);
            }

            //chamt test
            //if (0 == _vehicle._id)
            //{
            //    return ConstV.v3_zero; 
            //}

            //the magnitude of cohesion is usually much larger than separation or
            //allignment so it usually helps to normalize it.
            return SteeringForce.normalized;
        }


        public void TagNeighbors(Unit entity, List<Unit> ContainerOfEntities, float radius)
        {
            //iterate through all entities checking for range
            Unit curEntity;
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //first clear any current tag
                curEntity._tag = false;

                Vector3 to = curEntity._pos - entity._pos;

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + curEntity._radius_body;

                //if entity within range, tag for further consideration. (working in
                //distance-squared space to avoid sqrts)
                if ((curEntity != entity) && (to.sqrMagnitude < range * range))
                {
                    curEntity._tag = true;
                }

            }//next entity
        }

        float _DBoxLength;
        float _MinDetectionBoxLength = 5f;
        Vector3 ObstacleAvoidance(List<Unit> obstacles)
        {
            //the detection box length is proportional to the agent's velocity
            _DBoxLength = _MinDetectionBoxLength +
                            (_vehicle._speed / _vehicle._maxSpeed) * _MinDetectionBoxLength;

            //tag all obstacles within range of the box for processing
            TagNeighbors(_vehicle, obstacles, _DBoxLength);

            //this will keep track of the closest intersecting obstacle (CIB)
            Unit ClosestIntersectingObstacle = null;

            //this will be used to track the distance to the CIB
            float DistToClosestIP = float.MaxValue;

            //this will record the transformed local coordinates of the CIB
            Vector3 LocalPosOfClosestObstacle = Vector3.zero;

            Vector3 perp = Vector3.Cross(Vector3.up, _vehicle._heading);
            foreach (Unit curOb in obstacles)
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
                        float ExpandedRadius = curOb._radius_body + _vehicle._radius_body;

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
                            //DebugWide.DrawCircle(_vehicle._pos + _vehicle._heading * ip, 1f, Color.white);
                            //DebugWide.DrawCircle(curOb._pos, ExpandedRadius, Color.white);
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
                SteeringForce.x = (ClosestIntersectingObstacle._radius_body -
                                   LocalPosOfClosestObstacle.x) * multiplier;

                //apply a braking force proportional to the obstacles distance from
                //the vehicle. 
                const float BrakingWeight = 0.2f;

                SteeringForce.z = (ClosestIntersectingObstacle._radius_body -
                                   LocalPosOfClosestObstacle.z) *
                                   BrakingWeight;
            }

            //finally, convert the steering vector from local to world space
            Vector3 worldPos = Misc.PointToWorldSpaceZX(SteeringForce,
                                      _vehicle._heading,
                                      perp);

            //-------------------------------------------------
            Quaternion rot_box = Quaternion.FromToRotation(Vector3.forward, _vehicle._heading);
            Vector3 box_0 = _vehicle._pos + rot_box * new Vector3(_vehicle._radius_body, 0, 0);
            Vector3 box_1 = _vehicle._pos + rot_box * new Vector3(_vehicle._radius_body, 0, _DBoxLength);
            Vector3 box_2 = _vehicle._pos + rot_box * new Vector3(-_vehicle._radius_body, 0, _DBoxLength);
            Vector3 box_3 = _vehicle._pos + rot_box * new Vector3(-_vehicle._radius_body, 0, 0);
            //DebugWide.AddDrawQ_Line(box_0, box_1, Color.gray);
            //DebugWide.AddDrawQ_Line(box_1, box_2, Color.gray);
            //DebugWide.AddDrawQ_Line(box_2, box_3, Color.gray);
            //DebugWide.AddDrawQ_Line(box_3, box_0, Color.gray);
            //DebugWide.AddDrawQ_Line(_vehicle._pos, _vehicle._pos + worldPos, Color.black);
            //DebugWide.AddDrawQ_Circle(_vehicle._pos + worldPos, 0.5f, Color.black);
            //-------------------------------------------------


            return worldPos;
        }
    }

}//end namespace



