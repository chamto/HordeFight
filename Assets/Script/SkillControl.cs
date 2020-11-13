using UnityEngine;
using System.Collections.Generic;

namespace HordeFight
{

    //자세 => 준비동작 => 공격 (3단계)

    //========================================================
    //==================     동작  정보     ==================
    //========================================================
    public partial class Behavior
    {

        //운동 모양
        public enum eMovementShape
        {
            None,
            Straight,   //직선

            Rotation,   //회전 
            Area,       //영역
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

        //===================================

        //운동 모양 
        public eMovementShape movementShape;

        //공격행동의 중심점 
        public Vector3 point_0; //행동 중심점 (상대위치임) : 0(시작) , 1(끝)
        public Vector3 point_1;


        public Vector3 object_startDir;     //무기 시작 방향
        public Vector3 behaDir;             //행동의 방향
        public float angle;             //범위 각도

        public float plus_range_0;      //더해지는 범위 최소 
        public float plus_range_1;      //더해지는 범위 최대


        //=== 직선 공격 모델 === Straight
        //무기를 휘둘러 무기가 최대공격점까지 이동한후 제자리로 돌아오는 것을 수치모델로 만든것임. 요요라 보면됨
        public float distance_travel;   //공격점까지 이동 거리 : 상대방까지의 직선거리 , 근사치 , 판단을 위한 값임 , 정확한 충돌검사용 값이 아님.
        public float distance_maxTime;  //최대거리가 되는 시간 , 공격점에 도달하는 시간
        public float velocity_before;   //공격점 전 속도 
        public float velocity_after;    //공격점 후 속도  


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

            movementShape = eMovementShape.None;
            plus_range_0 = 0f;
            plus_range_1 = 0f;
            angle = 45f;
            distance_travel = 0f;
            distance_maxTime = 0f;
            velocity_before = 0f;
            velocity_after = 0f;
        }

        public Behavior Clone()
        {
            return this.MemberwiseClone() as Behavior;
        }



    }

    public partial class Behavior
    {

        public float GetEventTime_Interval()
        {
            return this.eventTime_1 - this.eventTime_0;
        }

        public float GetOpenTime_Interval()
        {
            return this.openTime_1 - this.openTime_0;
        }

        //public bool Valid_EventTime(Being.ePhase phase, float timeDelta)
        //{

        //    if (Being.ePhase.Start == phase || Being.ePhase.Running == phase)
        //    {
        //        if (this.eventTime_0 <= timeDelta && timeDelta <= this.eventTime_1)
        //            return true;
        //    }

        //    return false;
        //}

        //public bool Valid_CloggedTime(Being.ePhase phase, float timeDelta)
        //{
        //    if (Being.ePhase.Start == phase || Being.ePhase.Running == phase)
        //    {
        //        if (this.cloggedTime_0 <= timeDelta && timeDelta <= this.cloggedTime_1)
        //            return true;
        //    }

        //    return false;
        //}

        //public bool Valid_OpenTime(Being.ePhase phase, float timeDelta)
        //{
        //    if (Being.ePhase.Running == phase)
        //    {
        //        if (this.openTime_0 <= timeDelta && timeDelta <= this.openTime_1)
        //            return true;
        //    }

        //    return false;
        //}


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

        //==================================================

        static public Behavior Details_Idle()
        {
            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1f;

            bhvo.eventTime_0 = 0f;
            bhvo.eventTime_1 = 0f;
            bhvo.openTime_0 = 0;
            bhvo.openTime_1 = 10f;

            return bhvo;
        }

        static public Behavior Details_Move()
        {
            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1f;

            bhvo.eventTime_0 = 0f;
            bhvo.eventTime_1 = 0f;
            bhvo.openTime_0 = 0;
            bhvo.openTime_1 = 10f;

            return bhvo;
        }


        static public Behavior Details_Attack_Strong()
        {

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1.0f;
            //1
            bhvo.cloggedTime_0 = 0.1f;
            bhvo.cloggedTime_1 = 1.0f;
            //2
            bhvo.eventTime_0 = 1.0f;
            bhvo.eventTime_1 = 1.2f;
            //3
            bhvo.openTime_0 = 1.5f;
            bhvo.openTime_1 = 1.8f;
            //4
            bhvo.rigidTime = 0.0f;

            bhvo.movementShape = Behavior.eMovementShape.Straight;
            //bhvo.attack_shape = eTraceShape.Vertical;
            bhvo.angle = 45f;
            bhvo.plus_range_0 = 2f;
            bhvo.plus_range_1 = 2f;
            bhvo.distance_travel = 1f;
            //bhvo.distance_maxTime = bhvo.eventTime_0; //유효범위 시작시간에 최대 거리가 되게 한다. : 떙겨치기 , [시간증가에 따라 유효거리 감소]
            bhvo.distance_maxTime = bhvo.eventTime_1; //유효범위 끝시간에 최대 거리가 되게 한다. : 일반치기 , [시간증가에 따라 유효거리 증가]

            bhvo.Calc_Velocity();

            return bhvo;
        }

    }

}//end namespace
//*/
//========================================================
//==================     스킬  정보     ==================
//========================================================


namespace HordeFight
{
    //스킬 : 행동의 집합체 
    public partial class Skill : List<Behavior>
    {

        public enum eKind
        {
            None,
            Move,
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

            Move_0,

            Hit_Body,
            Hit_Weapon,

            Attack_Strong_1,

            Block_1,

            Max
        }


        //========================================

        private int _index_current = 0;


        //========================================

        public eKind _kind;
        public eName _name;

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
            skinfo._kind = eKind.None;
            skinfo._name = eName.Idle;

            skinfo.Add(Behavior.Details_Idle());

            return skinfo;
        }

        static public Skill Details_Move()
        {
            Skill skinfo = new Skill();
            skinfo._kind = eKind.Move;
            skinfo._name = eName.Move_0;

            skinfo.Add(Behavior.Details_Move());

            return skinfo;
        }


        static public Skill Details_Attack_Strong()
        {
            Skill skinfo = new Skill();
            skinfo._kind = eKind.Attack_Strong;
            skinfo._name = eName.Attack_Strong_1;

            skinfo.Add(Behavior.Details_Attack_Strong());

            return skinfo;
        }

    }

    //======================================================

    public partial class Skill
    {
        public class BaseInfo
        {
            public SkillControl skillControl = null;
            public Skill skill = null;
            public Being being = null;


            public void Init(SkillControl skillControl, Being be, Skill.eName name)
            {
                this.skillControl = skillControl;
                skill = SingleO.skillBook.Refer(name);
                being = be;
            }

            public virtual void On_Start() { }
            public virtual void On_Running() { }
            public virtual void On_Wait() { }
            public virtual void On_End() { }
        }
    }

    /// <summary>
    /// Skill book.
    /// </summary>
    public class SkillBook
    {
        private delegate Skill Details_Skill();
        private Dictionary<Skill.eName, Skill> _referDict = new Dictionary<Skill.eName, Skill>();   //미리 만들어진 정보로 빠르게 사용
        //private Dictionary<Skill.eName, Details_Skill> _createDict = new Dictionary<Skill.eName, Details_Skill>(); //새로운 스킬인스턴스를 만들때 사용 

        public SkillBook()
        {
            this.Add(Skill.eName.Idle, Skill.Details_Idle);

            this.Add(Skill.eName.Move_0, Skill.Details_Move);

            this.Add(Skill.eName.Attack_Strong_1, Skill.Details_Attack_Strong);

        }

        private void Add(Skill.eName name, Details_Skill skillPtr)
        {
            _referDict.Add(name, skillPtr());
            //_createDict.Add(name, skillPtr);
        }

        //만들어진 객체를 참조한다 
        public Skill Refer(Skill.eName name)
        {
            return _referDict[name];
        }

        ////요청객체를 생성한다
        //public Skill Create(Skill.eName name)
        //{
        //    return _createDict[name]();
        //}
    }
}

namespace HordeFight
{
    public class SkillControl
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

        public enum eSubState
        {
            None,

            Start,
            Running,
            End,

            Max
        }


        //public enum ePoint
        //{
        //    Start,  //시작
        //    End,    //끝지

        //    Cur,    //현재

        //    Max,
        //}

        //public class Part
        //{
        //    //신체부위
        //    public enum eKind
        //    {
        //        None = 0,

        //        //==== 인간형태 ==== 
        //        Head = 0,
        //        Hand_Left,  //손
        //        Hand_Right,
        //        Foot_Left,  //발
        //        Foot_Right,

        //        //Other_1,    //또다른 손발등
        //        //Other_2,

        //        Max,

        //    }


        //    public Vector3 _pos_standard;    //신체 부위의 기준점 
        //    public float _range_max;         //부위 기준점으로 부터 최대 범위
        //    public Vector3[] _pos;           //부위 위치 (로컬값)
        //    public Vector3[] _dir;           //부위 방향 (로컬값)

        //    public Vector3 _target;          //목표점 (월드값)

        //    public void Init()
        //    {
        //        _pos_standard = ConstV.v3_zero;
        //        _range_max = 1f;
        //        _pos = new Vector3[(int)ePoint.Max];
        //        _dir = new Vector3[(int)ePoint.Max];

        //        _target = new Vector3(0, 0, 2f); //z축을 보게 한다 
        //    }

        //}


        //====================================
        //public Part[] _parts = null;    //부위 정보 


        //동작정보
        public Behavior _behavior_cur = null;
        //public Skill _skill_cur = null;

        //다음 스킬 정보 
        //private Skill _skill_next = null;
        public Skill.BaseInfo _skillInfo_cur = null;
        public Skill.BaseInfo _skillInfo_next = null;

        public float _timeDelta = 0f;  //시간변화량

        //상태정보
        public eState _state_current = eState.None;
        private eSubState _eventState_current = eSubState.None;     //유효상태

        //판정
        //private Judgment _judgment = new Judgment();



        public Being _ref_being = null;

        //====================================

        //public SkillControl(Being ref_being)
        //{
        //    _ref_being = ref_being;
        //}


        //public SkillControl()
        //{
        //    _parts = new Part[(int)Part.eKind.Max];
        //    for (int i = 0; i < (int)Part.eKind.Max; i++)
        //    {
        //        _parts[i] = new Part();
        //        _parts[i].Init();
        //    }

        //    Setting_Head_2Hand_2Foot();
        //}

        //public void Setting_2Hand()
        //{
        //    //x-z축 공간에 캐릭터가 놓여 있고, z축을 바라보고 있다 가정 < Forward 방향 >
        //    Part HL = _parts[(int)Part.eKind.Hand_Left];
        //    Part HR = _parts[(int)Part.eKind.Hand_Right];

        //    HL._pos_standard = new Vector3(-0.5f, 0.5f, 0);
        //    HR._pos_standard = new Vector3(0.5f, 0.5f, 0);
        //    HR._range_max = 1.1f; //오른쪽 사정거리를 약간 더 늘린다 
        //}

        //public void Setting_Head_2Hand_2Foot()
        //{
        //    //x-z축 공간에 캐릭터가 놓여 있고, z축을 바라보고 있다 가정 < Forward 방향 >
        //    Setting_2Hand();
        //    _parts[(int)Part.eKind.Head]._pos_standard = new Vector3(0, 1f, 0);
        //    _parts[(int)Part.eKind.Foot_Left]._pos_standard = new Vector3(-0.5f, 0, 0);
        //    _parts[(int)Part.eKind.Foot_Right]._pos_standard = new Vector3(0.5f, 0, 0);
        //}



        public float CurrentDistance()
        {
            return _behavior_cur.CurrentDistance(_timeDelta);
        }

        //public float GetWeaponDistance()
        //{
        //    return _behavior.CurrentDistance(_timeDelta);
        //}

        //public Vector3 GetWeaponPosition(float time)
        //{
        //    //DebugWide.LogBlue (_behavior.CurrentDistance (time) * _direction); //chamto test
        //    return _position + (_behavior.CurrentDistance(time) * _direction);
        //}

        //public Vector3 GetWeaponPosition()
        //{
        //    return this.GetWeaponPosition(_timeDelta);

        //}

        public void Init(Being being, Skill.BaseInfo info)
        {
            _ref_being = being;

            _skillInfo_cur = info;
            _behavior_cur = _skillInfo_cur.skill.FirstBehavior();
            SetState(eState.Start);
            this._timeDelta = 0f;
        }

        public void SetState(eState setState)
        {
            _state_current = setState;
        }

        public void SetEventState(eSubState setSubState)
        {
            _eventState_current = setSubState;
        }


        //public SkillBook ref_skillBook { get { return CSingleton<SkillBook>.Instance; } }

        //public void PlayNow(Skill.eName name, Skill.AddInfo addInfo)
        //{
        //    this.PlayNow(ref_skillBook.Refer(name) , addInfo);
        //}

        //바로 요청스킬로 변경 
        //public void PlayNow(Skill skill, Skill.AddInfo addInfo)
        public void PlayNow(Skill.BaseInfo info)
        {
            //현재 설정된 스킬과 같은 스킬이 들어오면 처리하지 않느다 
            if (eState.End != this._state_current
                && _skillInfo_cur.skill._name == info.skill._name)
                return;

            //다음스킬정보 초기화 
            _skillInfo_next = null;

            _skillInfo_cur = info;
            //_skill_cur = _addInfo_cur.skill;
            _behavior_cur = _skillInfo_cur.skill.FirstBehavior();
            SetState(eState.Start);
            this._timeDelta = 0f;
        }

        //public void Play(Skill.eName name ,Skill.AddInfo addInfo)
        //{
        //    this.Play(ref_skillBook.Refer(name), addInfo);
        //}

        //현재 스킬의 행동 end 까지 진행후 다음스킬 시작 (현재 스킬을 end 상태로 바로 전환한다)  
        //public void Play(Skill skill, Skill.AddInfo addInfo)
        public void PlayNext(Skill.BaseInfo info)
        {
            
            //현재 스킬이 지정되어 있지 않으면 바로 요청 스킬로 지정한다
            //현재 상태가 end라면 스킬을 바로 지정한다
            if (eState.End == this._state_current)
            {
                this.PlayNow(info);
                return;
            }

            //설정할려는 스킬과 현재스킬이 같다면 설정하지 않는다 
            if (info.skill._name == _skillInfo_cur.skill._name)
                return;

            _skillInfo_next = info;
            //_skill_next = skill;

            //SetState(eState.End); //계속 함수를 호출하면 End 상태를 못 벗어나 주석처리함 
        }

        //public void Attack_Strong_1()
        //{
        //    Play(Skill.eName.Attack_Strong_1);
        //}

        //public void Move_0()
        //{
        //    Play(Skill.eName.Move_0);
        //}

        //public void Idle()
        //{
        //    Play(Skill.eName.Idle);
        //}

        public void Update()
        {
            if (null == _behavior_cur) return;

            this._timeDelta += Time.deltaTime;


            switch (this._state_current)
            {
                case eState.None:
                    {
                        //===== 처리철차 ===== 
                        //입력 -> ui갱신 -> (갱신 -> 판정)
                        //공격키입력 -> 행동상태none 에서 start 로 변경 -> start 상태 검출
                        //* 공격키입력으로 시작되는 상태는 None 이 되어야 한다. (바로 Start 상태가 되면 판정에서 Start상태인지 모른다)
                        //* 상태변이에 의해 시작되는 상태는 Start 여야 한다. (None 으로 시작되면 한프레임을 더 수행하는게 되므로 Start로 시작하게 한다)
                        this._timeDelta = 0f;
                        SetState(eState.Start);
                    }
                    break;
                case eState.Start:
                    {

                        this._timeDelta = 0f;


                        _skillInfo_cur.On_Start();

                        //DebugWide.LogBlue("[0: " + this._state_current + "  " + _skillInfo_cur.skill._name + "  " + _timeDelta + "  : ");//chamto test


                        SetState(eState.Running);
                        SetEventState(eSubState.None);

                    }
                    break;
                case eState.Running:
                    {

                        //====================================================
                        // update sub_state 
                        //====================================================



                        switch (_eventState_current)
                        {
                            case eSubState.None:
                                if (_behavior_cur.eventTime_0 <= _timeDelta && _timeDelta <= _behavior_cur.eventTime_1)
                                {
                                    this.SetEventState(eSubState.Start);
                                }
                                break;
                            case eSubState.Start:
                                this.SetEventState(eSubState.Running);
                                break;
                            case eSubState.Running:
                                if (!(_behavior_cur.eventTime_0 <= _timeDelta && _timeDelta < _behavior_cur.eventTime_1))
                                {
                                    this.SetEventState(eSubState.End);
                                }

                                break;
                            case eSubState.End:
                                this.SetEventState(eSubState.None);
                                break;

                        }


                        if (_behavior_cur.runningTime <= this._timeDelta)
                        {

                            _skillInfo_cur.On_End();

                            //동작완료
                            this.SetState(eState.Waiting);

                            break;
                        }


                        _skillInfo_cur.On_Running();

                        //DebugWide.LogBlue("[0: " + this._state_current + "  " + _skill_cur._name + "  " + _timeDelta);//chamto test
                    }
                    break;
                case eState.Waiting:
                    {
                        DebugWide.LogBlue("[0: " + this._state_current + "  " + _skillInfo_cur.skill._name + "  " + _timeDelta + "  : ");//chamto test
                        //DebugWide.LogBlue (_behavior.rigidTime + "   " + (this._timeDelta - _behavior.allTime));
                        if (_behavior_cur.rigidTime <= (this._timeDelta - _behavior_cur.runningTime))
                        {
                            this.SetState(eState.End);
                        }

                    }
                    break;
                case eState.End:
                    {

                        //* 다음 스킬입력 처리  
                        if (null != _skillInfo_next)
                        {
                            //DebugWide.LogBlue ("next : " + _skill_next.name);
                            PlayNow(_skillInfo_next);
                            //_skill_next = null;

                        }
                        else
                        {
                            //** 콤보 스킬 처리
                            _behavior_cur = _skillInfo_cur.skill.NextBehavior();
                            if (null == _behavior_cur)
                            //if(false == _skill_current.IsNextBehavior())
                            {
                                //스킬 동작을 모두 꺼냈으면 아이들상태로 들어간다
                                //Idle();
                                _ref_being.Idle();
                                //_ref_being._skill_idle.Play();
                            }
                            else
                            {
                                //_behavior = _skill_current.NextBehavior ().Clone();

                                //다음 스킬 동작으로 넘어간다
                                SetState(eState.Start);

                                DebugWide.LogBlue("next combo !!");
                            }
                        }
                        _timeDelta = 0f;

                    }
                    break;


            }


            //============================================================================

        }//end func

    }
}
