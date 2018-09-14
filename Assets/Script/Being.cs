using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Utility;



//========================================================
//==================      진영 관리기      ==================
//========================================================
namespace HordeFight
{
    //뛰어난 동물 진영
    public class CampChamp
    {
        //* 리더1이 부상시 2,3순위가 리더가 된다. 리더가 없을 경우 진영이 흩어진다. 
        //* 진영에 포함된 캐릭터는 진영컨트롤로 한꺼번에 조종 할 수 있다.
        //* 개인행동(뗄감,채집,사냥,정찰 등) 명령을 내리면 진영에서 이탈하게 된다. 
        //진영 리더 1순위
        //진영 리더 2순위
        //진영 리더 3순위

        //* 진영별로 목표점을 조절하여 진영의 모양에 변화를 줄 수 있다.
        //진영 종류 : 원형 , 종형 , 횡형

        //진영에 있는 챔프목록
        //개인행동 하는 챔프목록 

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


    public class Movement : MonoBehaviour
    {
        public eDirection8 _eDir8 = eDirection8.down;

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






    public class Behavior
    {

        public enum eKind
        {
            None = 0,

            Idle = 10,
            Idle_Random = 11,
            Idle_LookAt = 12,
            Idle_Max = 19,

            Move = 20,
            Move_Max = 29,

            Attack = 30,
            Attack_Max = 39,

            FallDown = 40,
            FallDown_Max = 49,

        }


        //===================================

        public float runningTime;   //동작 전체 시간 


        //아래는 동작의 전체 시간안에 있는 시간이다 

        //상대가 막을 수 있는 시간
        public float cloggedTime_0; //막히는 시간 : 0(시작) , 1(끝)  
        public float cloggedTime_1;

        //동작 이벤트가 성공하는 시간
        public float eventTime_0;   //동작 유효 범위 : 0(시작) , 1(끝)  
        public float eventTime_1;

        //콤보 연결을 위한 입력가능 시간
        public float openTime_0;    //다음 동작 연결시간 : 0(시작) , 1(끝)  
        public float openTime_1;

        //하나의 동작이 완료후 경직되는 시간. 마지막 모션상태로 경직 상태동안 있는다
        public float rigidTime;     //동작 완료후 경직 시간


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

        }

        public Behavior Clone()
        {
            return this.MemberwiseClone() as Behavior;
        }



    }

    public class AttackBehavior : Behavior
    {
        //공격의 모양
        public enum eTraceShape
        {
            None,
            Horizon,  //수평
            Vertical, //수직
            Straight, //직선
        }

        //공격 모델 종류 
        public eTraceShape attack_shape;

        //=== 범위 공격 모델 === Horizon , Vertical
        public float plus_range_0;      //더해지는 범위 최소 
        public float plus_range_1;      //더해지는 범위 최대
        public float angle;             //범위 각도

        //=== 직선 공격 모델 === Straight
        //무기를 휘둘러 무기가 최대공격점까지 이동한후 제자리로 돌아오는 것을 수치모델로 만든것임. 요요라 보면됨
        public float distance_travel;   //공격점까지 이동 거리 : 상대방까지의 직선거리 , 근사치 , 판단을 위한 값임 , 정확한 충돌검사용 값이 아님.
        public float distance_maxTime;  //최대거리가 되는 시간 , 공격점에 도달하는 시간
        public float velocity_before;   //공격점 전 속도 
        public float velocity_after;    //공격점 후 속도  

        public AttackBehavior()
        {
            
            attack_shape = eTraceShape.None;
            plus_range_0 = 0f;
            plus_range_1 = 0f;
            angle = 45f;
            distance_travel = 0f;
            distance_maxTime = 0f;
            velocity_before = 0f;
            velocity_after = 0f;

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

            //DebugWide.LogBlue("velocity_before : " + this.velocity_before + "   <-- 충돌점 -->   velocity_after : " + this.velocity_after + "  [distance_travel:" + distance_travel + "]"); //chamto test
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
        //단계
        public enum ePhase
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
        public Behavior.eKind _behaviorKind = Behavior.eKind.None;
        public Behavior _behavior = null;
        public float _timeDelta = 0f;  //시간변화량

        //상태정보
        public ePhase _phase = ePhase.None;
        public bool _death = false;

        //주시대상
        public Being _looking = null;

        //소유아이템
        public Inventory _inventory = null;

        //====================================

        public delegate void CallBack_State();

        public List<CallBack_State> _onStates_Start = new List<CallBack_State>();
        public List<CallBack_State> _onStates_End = new List<CallBack_State>();

        //====================================

        //애니
        private Animator _animator = null;
        private AnimatorOverrideController _overCtr = null;
        private SpriteRenderer _sprRender = null;
        private SphereCollider _collider = null;

        //이동 
        private Movement _move = null;
        //====================================

		private void Start()
		{
            _behaviorKind = Behavior.eKind.Idle_Random;

            //Single.touchProcess.Attach_SendObject(this.gameObject);

            _animator = GetComponentInChildren<Animator>();
            //오버라이드컨트롤러를 생성해서 추가하지 않고, 미리 생성된 것을 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
            _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _overCtr.name = "divide_character_" + _id.ToString();
            _animator.runtimeAnimatorController = _overCtr;
            _sprRender = GetComponentInChildren<SpriteRenderer>();
            _collider = GetComponent<SphereCollider>();

            _move = GetComponent<Movement>();
		}


    

		public float GetCollider_Radius()
        {

            return _collider.radius;
        }


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

            if (ePhase.Start == _phase || ePhase.Running == _phase)
            {
                if (_behavior.eventTime_0 <= _timeDelta && _timeDelta <= _behavior.eventTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_CloggedTime()
        {
            if (ePhase.Start == _phase || ePhase.Running == _phase)
            {
                if (_behavior.cloggedTime_0 <= _timeDelta && _timeDelta <= _behavior.cloggedTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_OpenTime()
        {
            if (ePhase.Running == _phase)
            {
                if (_behavior.openTime_0 <= _timeDelta && _timeDelta <= _behavior.openTime_1)
                    return true;
            }

            return false;
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




        //한 프레임에서 start 다음에 running 이 바로 시작되게 한다. 상태 타이밍 이벤트는 콜벡함수로 처리한다 
        public void Update()
        {
            switch (this._phase)
            {
                case ePhase.None:
                    {
                        this._timeDelta = 0f;
                    }
                    break;
                case ePhase.Start:
                    {
                        this._timeDelta = 0f;

                        //////////////////////
                        foreach(CallBack_State callback in _onStates_Start)
                        {
                            callback();
                        }
                        //////////////////////

                        _phase = ePhase.Running;


                    }
                    break;
            }

            switch (this._phase)
            {
                
                case ePhase.Running:
                    {

                        this._timeDelta += Time.deltaTime;
                        //this._timeDelta += FrameTime.DeltaTime(); //todo test : 프레임드랍 상황에서 테스트 필요 

                        if (this._timeDelta >= _behavior.runningTime)
                        {
                            //동작완료
                            _phase = ePhase.End;
                        }
                    }
                    break;
            }

            switch (this._phase)
            {
                case ePhase.Waiting:
                    {
                        
                    }
                    break;
                case ePhase.End:
                    {

                        //////////////////////
                        foreach (CallBack_State callback in _onStates_End)
                        {
                            callback();
                        }
                        //////////////////////

                        _phase = ePhase.Start;
                        _timeDelta = 0f;

                    }
                    break;
            }


        }//end func



        //____________________________________________
        //                  애니메이션  
        //____________________________________________

        //todo optimization : 애니메이션 찾는 방식 최적화 필요. 해쉬로 변경하기 
        public AnimationClip GetClip(string name)
        {
            foreach (AnimationClip ani in Single.resourceManager._aniClips)
            {
                if (ani.name.Equals(name))
                {
                    return ani;
                }
            }

            return null;
        }


        public void Switch_Ani(string aniKind, string aniName, eDirection8 dir)
        {

            _sprRender.flipX = false;
            string aniNameSum = "";
            switch (dir)
            {

                case eDirection8.leftUp:
                    {
                        aniNameSum = aniName + eDirection8.rightUp.ToString();
                        _sprRender.flipX = true;
                    }
                    break;
                case eDirection8.left:
                    {
                        aniNameSum = aniName + eDirection8.right.ToString();
                        _sprRender.flipX = true;
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        aniNameSum = aniName + eDirection8.rightDown.ToString();
                        _sprRender.flipX = true;
                    }
                    break;
                default:
                    {
                        aniNameSum = aniName + dir.ToString();
                        _sprRender.flipX = false;
                    }
                    break;

            }

            _overCtr[aniKind] = GetClip(aniNameSum);
        }


        private float __elapsedTime_1 = 0f;
        private float __randTime = 0f;
        public void Idle_Random()
        {
            if ((int)Behavior.eKind.Idle == _animator.GetInteger("state"))
            {
                __elapsedTime_1 += Time.deltaTime;


                if (__randTime < __elapsedTime_1)
                {

                    //_eDir8 = (eDirection)Single.rand.Next(0, 8); //0~7

                    //근접 방향으로 랜덤하게 회전하게 한다 
                    int num = Misc.rand.Next(-1, 2); //-1 ~ 1
                    num += (int)_move._eDir8;
                    if (0 > num) num = 7;
                    if (7 < num) num = 0;
                    _move._eDir8 = (eDirection8)num;

                    Switch_Ani("base_idle", _behaviorKind.ToString() + "_idle_", _move._eDir8);

                    __elapsedTime_1 = 0f;

                    //3~6초가 지났을 때 돌아감
                    __randTime = (float)Misc.rand.Next(3, 7); //3~6
                }

            }

        }

        public void Idle_View(Vector3 dir, bool forward)//, bool setState)
        {

            if (false == forward)
                dir *= -1f;

            _move._eDir8 = Misc.TransDirection8_AxisY(dir);
            //Switch_AniMove("base_move",_eKind.ToString()+"_attack_",_eDir8);
            Switch_Ani("base_idle", _behaviorKind.ToString() + "_idle_", _move._eDir8);

            //if(true == setState)
            //_animator.SetInteger("state", (int)eState.Idle);
        }

    }

}


namespace HordeFight
{
    //========================================================
    //==================     스킬  정보     ==================
    //========================================================


    public class SkillManager
    {

    }


    public class Skill : List<Behavior>
    {

        public enum eKind
        {
            None,
            Attack_Strong,
            Attack_Weak,
            Attack_Counter,
            Withstand,
            Block,
            Hit,
            Max
        }

        public enum eName
        {
            None,
            Idle,
            Hit_Body,
            Hit_Weapon,

            Attack_Strong_1,
            Attack_Weak_1,
            Attack_Counter_1,

            Attack_3Combo,

            Withstand_1,
            Block_1,

            Max
        }


        //========================================

        private int _index_current = 0;


        //========================================

        public eKind kind { get; set; }
        public eName name { get; set; }

        //========================================

        public Behavior FirstBehavior()
        {
            _index_current = 0; //index 초기화

            if (this.Count == 0)
                return null;

            return this[_index_current];
        }

        public Behavior NextBehavior()
        {
            if (this.Count > _index_current)
            {
                //마지막 인덱스임
                if (this.Count == _index_current + 1)
                    return null;

                _index_current++;
                return this[_index_current];
            }

            return null;
        }

        //다음 행동이 있나 질의한다
        public bool IsNextBehavior()
        {
            if (this.Count > _index_current)
            {
                //마지막 인덱스임
                if (this.Count == _index_current + 1)
                    return false;


                return true;
            }

            return false;
        }



        //========================================

        //스킬 명세서
        static public Skill Details_Idle()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.None;
            //skinfo.name = eName.Idle;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1f;

            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            //bhvo.openTime_0 = Behavior.MIN_OPEN_TIME;
            //bhvo.openTime_1 = Behavior.MAX_OPEN_TIME;
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_HitBody()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Hit;
            //skinfo.name = eName.Hit_Body;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1f;
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            //bhvo.openTime_0 = Behavior.MIN_OPEN_TIME;
            //bhvo.openTime_1 = Behavior.MAX_OPEN_TIME;
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_HitWeapon()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Hit;
            //skinfo.name = eName.Hit_Weapon;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1.5f;
            ////1
            //bhvo.cloggedTime_0 = 0f;
            //bhvo.cloggedTime_1 = 0f;
            ////2
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            ////3
            //bhvo.openTime_0 = -1f; //연결 동작을 못 넣게 막는다. 0으로 설정시 연속입력을 허용하게 된다
            //bhvo.openTime_1 = -1f;
            ////4
            //bhvo.rigidTime = 0f;


            ////bhvo.attack_shape = eTraceShape.Straight;
            //bhvo.angle = 0f;
            //bhvo.plus_range_0 = 0f;
            //bhvo.plus_range_1 = 0f;
            //bhvo.distance_travel = 0f;
            //bhvo.distance_maxTime = 0f;
            ////bhvo.Calc_Velocity ();
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Withstand_1()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Withstand;
            //skinfo.name = eName.Withstand_1;

            //Behavior bhvo = new Behavior();
            ////bhvo.runningTime = 10.0f; //임시값
            //bhvo.runningTime = 3.0f;
            ////1
            //bhvo.cloggedTime_0 = 0f;
            //bhvo.cloggedTime_1 = 0f;
            ////2
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            ////3
            //bhvo.openTime_0 = -1f; //연결 동작을 못 넣게 막는다. 0으로 설정시 연속입력을 허용하게 된다
            //bhvo.openTime_1 = -1f;
            ////4
            //bhvo.rigidTime = 0f;


            ////bhvo.attack_shape = eTraceShape.Straight;
            //bhvo.angle = 0f;
            //bhvo.plus_range_0 = 0f;
            //bhvo.plus_range_1 = 0f;
            //bhvo.distance_travel = 0f;
            //bhvo.distance_maxTime = 0f;
            ////bhvo.Calc_Velocity ();
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Attack_Weak()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Attack_Weak;
            //skinfo.name = eName.Attack_Weak_1;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1.5f;
            ////1
            //bhvo.cloggedTime_0 = 0.0f;
            //bhvo.cloggedTime_1 = 1.3f;
            ////2
            //bhvo.eventTime_0 = 0.7f;
            //bhvo.eventTime_1 = 1f;
            ////3
            //bhvo.openTime_0 = 1f;
            //bhvo.openTime_1 = 1.3f;
            ////4
            //bhvo.rigidTime = 0f;


            ////bhvo.attack_shape = eTraceShape.Straight;
            ////bhvo.attack_shape = eTraceShape.Vertical;
            //bhvo.angle = 45f;
            //bhvo.plus_range_0 = 0f;
            //bhvo.plus_range_1 = -4f;
            //bhvo.distance_travel = Behavior.DEFAULT_DISTANCE - 4f;
            ////bhvo.distance_maxTime = bhvo.eventTime_0; //유효범위 시작시간에 최대 거리가 되게 한다. : 떙겨치기 , [시간증가에 따라 유효거리 감소]
            //bhvo.distance_maxTime = bhvo.eventTime_1; //유효범위 끝시간에 최대 거리가 되게 한다. : 일반치기 , [시간증가에 따라 유효거리 증가]

            //bhvo.Calc_Velocity();
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Attack_Counter()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Attack_Counter;
            //skinfo.name = eName.Attack_Counter_1;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1.2f;
            ////1
            //bhvo.cloggedTime_0 = 0.0f;
            //bhvo.cloggedTime_1 = 0.7f;
            ////2
            //bhvo.eventTime_0 = 0.8f;
            //bhvo.eventTime_1 = 1.0f;
            ////3
            //bhvo.openTime_0 = -1f;
            //bhvo.openTime_1 = -1f;
            ////4
            //bhvo.rigidTime = 0.2f;


            ////bhvo.attack_shape = eTraceShape.Straight;
            //bhvo.distance_travel = Behavior.DEFAULT_DISTANCE;
            ////bhvo.distance_maxTime = bhvo.eventTime_0; //유효범위 시작시간에 최대 거리가 되게 한다. : 떙겨치기 , [시간증가에 따라 유효거리 감소]
            //bhvo.distance_maxTime = bhvo.eventTime_1; //유효범위 끝시간에 최대 거리가 되게 한다. : 일반치기 , [시간증가에 따라 유효거리 증가]
            //bhvo.Calc_Velocity();
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Attack_Strong()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Attack_Strong;
            //skinfo.name = eName.Attack_Strong_1;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 2.0f;
            ////1
            //bhvo.cloggedTime_0 = 0.1f;
            //bhvo.cloggedTime_1 = 1.0f;
            ////2
            //bhvo.eventTime_0 = 1.0f;
            //bhvo.eventTime_1 = 1.2f;
            ////3
            //bhvo.openTime_0 = 1.5f;
            //bhvo.openTime_1 = 1.8f;
            ////4
            //bhvo.rigidTime = 0.5f;

            ////bhvo.attack_shape = eTraceShape.Straight;
            ////bhvo.attack_shape = eTraceShape.Vertical;
            //bhvo.angle = 45f;
            //bhvo.plus_range_0 = 2f;
            //bhvo.plus_range_1 = 2f;
            //bhvo.distance_travel = Behavior.DEFAULT_DISTANCE;
            ////bhvo.distance_maxTime = bhvo.eventTime_0; //유효범위 시작시간에 최대 거리가 되게 한다. : 떙겨치기 , [시간증가에 따라 유효거리 감소]
            //bhvo.distance_maxTime = bhvo.eventTime_1; //유효범위 끝시간에 최대 거리가 되게 한다. : 일반치기 , [시간증가에 따라 유효거리 증가]

            //bhvo.Calc_Velocity();
            //skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Attack_3Combo()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Attack_Strong;
            //skinfo.name = eName.Attack_3Combo;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1f;
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            //bhvo.openTime_0 = Behavior.MIN_OPEN_TIME;
            //bhvo.openTime_1 = Behavior.MAX_OPEN_TIME;
            //skinfo.Add(bhvo);

            //bhvo = new Behavior();
            //bhvo.runningTime = 2f;
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            //bhvo.openTime_0 = Behavior.MIN_OPEN_TIME;
            //bhvo.openTime_1 = Behavior.MAX_OPEN_TIME;
            //skinfo.Add(bhvo);

            //bhvo = new Behavior();
            //bhvo.runningTime = 3f;
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 0f;
            //bhvo.openTime_0 = Behavior.MIN_OPEN_TIME;
            //bhvo.openTime_1 = Behavior.MAX_OPEN_TIME;
            //skinfo.Add(bhvo);

            return skinfo;
        }



        static public Skill Details_Block_1()
        {
            Skill skinfo = new Skill();

            //skinfo.kind = eKind.Block;
            //skinfo.name = eName.Block_1;

            //Behavior bhvo = new Behavior();
            //bhvo.runningTime = 1f;
            //bhvo.eventTime_0 = 0f;
            //bhvo.eventTime_1 = 1f;
            //bhvo.rigidTime = 0.1f;
            //bhvo.openTime_0 = Behavior.MIN_OPEN_TIME;
            //bhvo.openTime_1 = Behavior.MAX_OPEN_TIME;
            //skinfo.Add(bhvo);


            return skinfo;
        }

    }

    /// <summary>
    /// Skill book.
    /// </summary>
    public class SkillBook //: Dictionary<Skill.eName, Skill>
    {
        private delegate Skill Details_Skill();
        private Dictionary<Skill.eName, Skill> _referDict = new Dictionary<Skill.eName, Skill>();   //미리 만들어진 정보로 빠르게 사용
        private Dictionary<Skill.eName, Details_Skill> _createDict = new Dictionary<Skill.eName, Details_Skill>(); //새로운 스킬인스턴스를 만들때 사용 

        public SkillBook()
        {
            this.Add(Skill.eName.Idle, Skill.Details_Idle);
            this.Add(Skill.eName.Hit_Body, Skill.Details_HitBody);
            this.Add(Skill.eName.Hit_Weapon, Skill.Details_HitWeapon);

            this.Add(Skill.eName.Withstand_1, Skill.Details_Withstand_1);
            this.Add(Skill.eName.Block_1, Skill.Details_Block_1);

            this.Add(Skill.eName.Attack_Strong_1, Skill.Details_Attack_Strong);
            this.Add(Skill.eName.Attack_Weak_1, Skill.Details_Attack_Weak);
            this.Add(Skill.eName.Attack_Counter_1, Skill.Details_Attack_Counter);

            this.Add(Skill.eName.Attack_3Combo, Skill.Details_Attack_3Combo);

        }

        private void Add(Skill.eName name, Details_Skill skillPtr)
        {
            _referDict.Add(name, skillPtr());
            _createDict.Add(name, skillPtr);
        }

        //만들어진 객체를 참조한다 
        public Skill Refer(Skill.eName name)
        {
            return _referDict[name];
        }

        //요청객체를 생성한다
        public Skill Create(Skill.eName name)
        {
            return _createDict[name]();
        }
    }

}


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