using System;
using UnityEngine;


namespace Proto_AI_4
{
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

        public Character _vehicle;

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
                float speed = dist / _vehicle._decelerationTime; //v = s / t

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

        public float TurnAroundTime(Character agent, Vector3 targetPos, float turnSecond)
        {
            Vector3 toTarget = (targetPos - agent._pos).normalized;

            float dot = Vector3.Dot(agent._heading, toTarget);

            float coefficient = 0.5f * turnSecond; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 1초
            //const float coefficient = 0.5f * 5; //운반기가 목표지점과 정반대로 향하고 있다면 방향을 바꾸는데 5초

            return (dot - 1f) * -coefficient; //[-2 ~ 0] * -coefficient
        }
    }

}//end namespace



