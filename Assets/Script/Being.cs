using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Utility;


namespace HordeFight
{
    //========================================================
    //==================   아이템 정보   ==================
    //========================================================

    public class Inventory : Dictionary<uint, Item>
    {
    }

    public class Item
    {
        public enum eCategory_1 : uint
        {
            None,
            Weapone,
            Armor,
            Potion,
            Max,
        }

        public enum eCategory_2 : uint
        {
            None,

            Weapone_Start,
            Sword,  //칼 
            Spear,  //창
            Weapone_End,

            Armor_Start,
            Armor_End,

            Potion_Start,
            Potion_End,

            Max,
        }

        public class BaseInfo { }

        public class WeaponeInfo : BaseInfo
        {
            public ushort _power = 1;
            public float _range_min = 0f;  //최소 사거리
            public float _range_max = 1f;  //최대 사거리 
        }

        public uint _id;
        public eCategory_1 _eCat01 = eCategory_1.None;
        public eCategory_2 _eCat02 = eCategory_2.None;
        public BaseInfo _info = null;
        public ushort _position = 0; //아이템 위치(놓인 위치,장착 위치)


    }

}

namespace HordeFight
{
    //========================================================
    //==================   캐릭터/구조물 정보   ==================
    //========================================================


    /// <summary>
    /// 존재
    /// </summary>
    //public class Being : MonoBehaviour
    //{
    //    //** 결합 기능들 **
    //    //      이동 , 타기가능 , 탈것가능 , 던지기 , 마법 , 스킬   
    //}

    /// <summary>
    /// 구조물 : 건물 , 배
    /// </summary>
    public class Structure : Being
    {

    }

    /// <summary>
    /// 뛰어난 존재 
    /// </summary>
    public class Champ : Being
    {

    }


    public class Movable_2 : MonoBehaviour
    {

        private void Start()
        {
        }

        private void Update()
        {
        }



        public void Move(Vector3 dir, float distance, float speed)
        {
            //보간, 이동 처리
            //float delta = Interpolation.easeInOutBack(0f, 0.2f, accumulate / MAX_SECOND);
            this.transform.Translate(dir * Time.deltaTime * speed * distance);
        }

        public void MoveToPoint(Vector3 target, float speed)
        {
            //todo 
            //Vector3 v = target - this.transform.position;
            //Vector3.Lerp(this.transform.position)
        }
    }
    //========================================================




    //자취의 모양
    //public enum eTraceShape
    //{
    //None,
    //Horizon,  //수평
    //Vertical, //수직
    //Straight, //직선
    //}

    public class Behavior
    {
        //--------<<============>>----------
        //    openTime_0 ~ openTime_1
        // 시간범위 안에 있어야 콤보가 된다
        public const float MAX_OPEN_TIME = 10f;
        public const float MIN_OPEN_TIME = 0f;

        public const float DEFAULT_DISTANCE = 14f;

        //===================================

        public float runningTime;   //동작 전체 시간 
                                    //1
        public float cloggedTime_0; //막히는 시간 : 0(시작) , 1(끝)  
        public float cloggedTime_1;
        //2
        public float eventTime_0;   //동작 유효 범위 : 0(시작) , 1(끝)  
        public float eventTime_1;
        //3
        public float openTime_0;    //다음 동작 연결시간 : 0(시작) , 1(끝)  
        public float openTime_1;
        //4
        public float rigidTime;     //동작 완료후 경직 시간



        //무기 움직임 정보 
        //public eTraceShape attack_shape;        //공격모양 : 종 , 횡 , 찌르기 , 던지기
        //  === 범위형 움직임 === : 종,횡,찌르기,던지기 (무기 위치가 기준이 되는 범위이다)
        public float plus_range_0;      //더해지는 범위 최소 
        public float plus_range_1;      //더해지는 범위 최대
        public float angle;             //범위 각도
                                        //  === 이동형 움직임 === : 찌르기,던지기
        public float distance_travel;   //공격점까지 이동 거리 : 상대방까지의 직선거리 , 근사치 , 판단을 위한 값임 , 정확한 충돌검사용 값이 아님.
        public float distance_maxTime;      //최대거리가 되는 시간 : 공격점에 도달하는 시간
        public float velocity_before;   //공격점 전 속도 
        public float velocity_after;        //공격점 후 속도  


        public Behavior()
        {

            runningTime = 0f;
            eventTime_0 = 0f;
            eventTime_1 = 0f;
            rigidTime = 0f;
            openTime_0 = 0f;
            openTime_1 = 0f;
            cloggedTime_0 = 0f;
            cloggedTime_1 = 0f;
            plus_range_0 = 0f;
            plus_range_1 = 0f;
            angle = 45f; //chamto test
                         //angle = 0f;
                         //attack_shape = eTraceShape.None;
                         //distance_travel = DEFAULT_DISTANCE;
            distance_travel = 0f;
            distance_maxTime = 0f;
            velocity_before = 0f;
            velocity_after = 0f;

        }

        public Behavior Clone()
        {
            return this.MemberwiseClone() as Behavior;
        }

        public void Calc_Velocity()
        {
            //t * s = d
            //s = d/t
            if (0f == distance_maxTime)
                this.velocity_before = 0f;
            else
                this.velocity_before = distance_travel / distance_maxTime;

            this.velocity_after = distance_travel / (runningTime - distance_maxTime);

            DebugWide.LogBlue("velocity_before : " + this.velocity_before + "   <-- 충돌점 -->   velocity_after : " + this.velocity_after + "  [distance_travel:" + distance_travel + "]"); //chamto test
        }

        public float CurrentDistance(float currentTime)
        {
            //* 러닝타임 보다 더 큰 값이 들어오면 사용오류임
            if (runningTime < currentTime)
                return 0f;

            //* 최대거리에 도달하는 시간이 0이면 최대거리를 반환한다.
            if (0f == distance_maxTime)
            {
                return distance_travel;
            }

            //1. 전진
            if (currentTime <= distance_maxTime)
            {
                return this.velocity_before * currentTime;
            }

            //2. 후진
            //if(distance_maxTime < currentTime)
            return this.velocity_after * (runningTime - currentTime);
        }

    }



    public class Being : MonoBehaviour
    {
        public enum eState
        {
            None = 0,

            Start,
            Running,
            Waiting,
            End,

            Max,
        }

        public enum eKind
        {
            None = 0,
            footman,
            lothar,
            skeleton,
            garona,
            conjurer,
            raider,
            slime,
            spearman,
            grunt,
            brigand,
            knight,
            ogre,
            daemon,
            waterElemental,
            fireElemental,

        }


        //====================================

        //고유정보
        public uint _id;
        public eKind _kind = eKind.None;

        //능력치1 
        public ushort _power = 1;
        public ushort _hp_cur = 10;
        public ushort _hp_max = 10;
        public float _range_min = 0.15f;
        public float _range_max = 0.15f;

        //보조정보 
        //private Geo.Sphere _collider;
        private Vector3 _direction = Vector3.forward;


        //동작정보
        public Behavior _behavior = null;
        private Skill _skill_cur = null;
        private Skill _skill_next = null;
        private float _timeDelta = 0f;  //시간변화량

        //상태정보
        private eState _state_cur = eState.None;
        private eState _state_sub_cur = eState.None;   
        public bool _death = false;

        //주시대상
        public Being _looking = null;

        //소유아이템
        public Inventory _inventory = null;
        //====================================





        public void AddHP(ushort amount)
        {
            _hp_cur += amount;

            if (0 > _hp_cur)
                _hp_cur = 0;

            if (_hp_max < _hp_cur)
                _hp_cur = _hp_max;

        }


       

        public float GetEventTime_Interval()
        {
            return _behavior.eventTime_1 - _behavior.eventTime_0;
        }

        public float GetOpenTime_Interval()
        {
            return _behavior.openTime_1 - _behavior.openTime_0;
        }

        public bool Valid_EventTime()
        {

            if (eState.Start == _state_cur || eState.Running == _state_cur)
            {
                if (_behavior.eventTime_0 <= _timeDelta && _timeDelta <= _behavior.eventTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_CloggedTime()
        {
            if (eState.Start == _state_cur || eState.Running == _state_cur)
            {
                if (_behavior.cloggedTime_0 <= _timeDelta && _timeDelta <= _behavior.cloggedTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_OpenTime()
        {
            if (eState.Running == _state_cur)
            {
                if (_behavior.openTime_0 <= _timeDelta && _timeDelta <= _behavior.openTime_1)
                    return true;
            }

            return false;
        }




        public void SetSkill_Start(Skill skill)
        {
            _skill_cur = skill;
            _behavior = _skill_cur.FirstBehavior();

            _state_cur = eState.Start;
            this._timeDelta = 0f;
        }





        //공격이 상대방에 맞았나?
        //* 내무기 범위 또는 위치로 상대방 위치로 판단한다.
        //!!! 무기 범위가 방향성이 없다.  뒤나 앞이나 판정이 같다
        public bool Collision_Weaphon_Attack_VS(Being dst)
        {
            
            //***** 내무기 범위 vs 상대방 위치 *****

            //return Geo.Collision_Arc_VS_Sphere(this.GetArc_Weapon(), dst.GetCollider_Sphere());

            //{   //***** 내무기 위치 vs 상대방 위치 *****

            //    //fixme : 원과 반직선 충돌 처리로 변경하는게 더 낫다. 현재 처리로는 부족하다.
            //    //정면 5도안에 상대가 있을 경우만 공격가능
            //    //=======================================================================
            //    const float ANGLE_SCOPE = 10f;
            //    //각도를 2로 나누는 이유 : 1,4사분면 부호가 같기 때문에 둘을 구별 할 수 없다. 의도와 다르게 2배 영역이 된다.
            //    float angle = Mathf.Cos(ANGLE_SCOPE * 0.5f * Mathf.Deg2Rad);
            //    Vector3 toDst = dst.GetPosition() - this.GetPosition();
            //    toDst.Normalize();
            //    float cos = Vector3.Dot(this.GetDirection(), toDst);
            //    if (2 == Geo.Compare_CosAngle(angle, cos)) //angle 보다 cos이 작아야 함
            //    {
            //        DebugWide.LogBlue("std angle: " + angle + "   dst angle: " + Mathf.Acos(cos) * Mathf.Rad2Deg); //chamto test
            //        return false;
            //    }
            //    //=======================================================================

            //    if (true == Geo.Collision_Sphere(new Geo.Sphere(this.GetWeaponPosition(), this.weapon.collider_sphere_radius),
            //        dst.GetCollider_Sphere(), Geo.eSphere_Include_Status.Focus))
            //    {
            //        return true;
            //    }

            //}

            return false;
        }




        public void Update()
        {
            this._timeDelta += Time.deltaTime;
            //this._timeDelta += FrameControl.DeltaTime();

            switch (this._state_cur)
            {
                case eState.None:
                    {
                        //===== 처리철차 ===== 
                        //입력 -> ui갱신 -> (갱신 -> 판정)
                        //공격키입력 -> 행동상태none 에서 start 로 변경 -> start 상태 검출
                        //* 공격키입력으로 시작되는 상태는 None 이 되어야 한다. (바로 Start 상태가 되면 판정에서 Start상태인지 모른다)
                        //* 상태변이에 의해 시작되는 상태는 Start 여야 한다. (None 으로 시작되면 한프레임을 더 수행하는게 되므로 Start로 시작하게 한다)
                        this._timeDelta = 0f;
                        _state_cur = eState.Start;
                    }
                    break;
                case eState.Start:
                    {
                        this._timeDelta = 0f;
                        _state_cur = eState.Running;
                        _state_sub_cur = eState.None;

                        //DebugWide.LogRed ("[0: "+this._state_current);//chamto test
                    }
                    break;
                case eState.Running:
                    {

                        //====================================================
                        // update sub_state 
                        //====================================================



                        switch (_state_sub_cur)
                        {
                            case eState.None:
                                if (_behavior.eventTime_0 <= _timeDelta && _timeDelta <= _behavior.eventTime_1)
                                {
                                    _state_sub_cur = eState.Start;
                                }
                                break;
                            case eState.Start:
                                _state_sub_cur = eState.Running;
                                break;
                            case eState.Running:
                                if (!(_behavior.eventTime_0 <= _timeDelta && _timeDelta < _behavior.eventTime_1))
                                {
                                    _state_sub_cur = eState.End;
                                }

                                break;
                            case eState.End:
                                _state_sub_cur = eState.None;
                                break;

                        }


                        if (_behavior.runningTime <= this._timeDelta)
                        {
                            //동작완료
                            _state_cur = eState.Waiting;
                        }
                    }
                    break;
                case eState.Waiting:
                    {
                        //DebugWide.LogBlue (_behavior.rigidTime + "   " + (this._timeDelta - _behavior.allTime));
                        if (_behavior.rigidTime <= (this._timeDelta - _behavior.runningTime))
                        {
                            _state_cur = eState.End;
                        }

                    }
                    break;
                case eState.End:
                    {
                        //* 다음 스킬입력 처리  
                        if (null != _skill_next)
                        {
                            //DebugWide.LogBlue ("next : " + _skill_next.name);
                            SetSkill_Start(_skill_next);
                            _skill_next = null;
                        }
                        else
                        {
                            //** 콤보 스킬 처리
                            _behavior = _skill_cur.NextBehavior();
                            if (null == _behavior)
                            //if(false == _skill_current.IsNextBehavior())
                            {
                                //스킬 동작을 모두 꺼냈으면 아이들상태로 들어간다
                                //Idle();
                            }
                            else
                            {
                                //_behavior = _skill_current.NextBehavior ().Clone();

                                //다음 스킬 동작으로 넘어간다
                                _state_cur = eState.Start;
                                _timeDelta = 0f;

                                DebugWide.LogBlue("next combo !!");
                            }
                        }

                    }
                    break;


            }

            //============================================================================

           
            //============================================================================

        }//end func


    }

}