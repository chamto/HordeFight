﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UtilGS9;


namespace HordeFight
{


    public class Obstacle : Being
    {

        //private void Update()
        //{
        //          base.UpdateAll();

        //}

        //____________________________________________
        //                  충돌반응
        //____________________________________________

        public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        {
            Move_Push(dir, meterPerSecond);
        }


    }

    /// <summary>
    /// 구조물 : 건물 , 배
    /// </summary>
    public class Structure : Being
    {
        //private void Start()
        //{

        //}

        //private void Update()
        //{

        //}
    }

    //======================================================



    public class Being : MonoBehaviour, SphereModel.IUserData
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


        //객체이름에 '_' 가 들어가면 안된다. 전체 애니이름의 문자열을 분리할 때 '_' 기준으로 자르기 때문임 
        public enum eKind
        {
            None = 0,

            ______Champ_Humans______,
            peasant,
            footman,
            archer,
            knight,
            cleric,
            conjurer,
            medivh,
            lothar,

            ______Champ_Orcs______,
            peon,
            grunt,
            spearman,
            raider,
            necrolyte,
            warlock,
            garona,

            ______Champ_Etc______,
            slime,
            brigand,
            catapult,
            ogre,
            skeleton,
            skeletonW,
            scorpion,
            spider,
            daemon,
            waterElemental,
            fireElemental,

            ______Effect______,
            spear, //창
            arrow, //활
            fireBall,
            waterBolt,
            magicMissile,
            sackMissile, //자루

            ______Misc______,
            barrel, //숱통

        }

        //==================================================

        //public enum AniOverKey
        //{
        //    base_attack = 2,
        //    base_fallDown=1,
        //    base_idle=3,
        //    base_move=0,

        //}
        //public class AniClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
        //{
        //    public AniClipOverrides(int capacity) : base(capacity) { }
        //    //public void Init()
        //    //{
        //    //    DebugWide.LogBlue("-----------------"); //chamto test
        //    //    foreach(KeyValuePair<AnimationClip, AnimationClip> pair in this)
        //    //    {
        //    //        DebugWide.LogBlue(pair.Key.name + "  ");
        //    //    }
        //    //}
        //    public void SetOverAni(AniOverKey base_key, AnimationClip over_clip)
        //    {
        //        //KeyValuePair 구조체는 멤버변수를 수정 할 수 없기 떄문에, 다시 생성해서 넣어줘야 한다.
        //        this[(int)base_key] = new KeyValuePair<AnimationClip, AnimationClip>(this[(int)base_key].Key, over_clip);
        //    }
        //}

        //==================================================

        //상수
        public int ANI_STATE = Animator.StringToHash("state");
        public int ANI_STATE_IDLE = Animator.StringToHash("idle");
        public int ANI_STATE_MOVE = Animator.StringToHash("move");
        public int ANI_STATE_ATTACK = Animator.StringToHash("attack");
        public int ANI_STATE_FALLDOWN = Animator.StringToHash("fallDown");

        //==================================================

        //셀공간 링크 정보
        public Being _prev_sibling = null;
        public Being _next_sibling = null;
        public CellSpace _cur_cell = null;

        //==================================================

        //복사정보 - 속도를 위해 미리 구해 놓은 정보
        public GameObject _gameObject = null;
        public Transform _transform = null;
        protected Vector3 _getPos3D = ConstV.v3_zero;

        //public Index2       _getPos2D = ConstV.id2_zero;
        //public int          _getPos1D = -1;
        public Vector3 _getBounds_min = ConstV.v3_zero;
        public Vector3 _getBounds_max = ConstV.v3_zero;

        //==================================================

        //고유정보
        public uint _id;
        public eKind _kind = eKind.None;

        public int _hp_cur = 10;
        public int _hp_max = 10;


        //==================================================

        //진영정보
        public Camp _belongCamp = null; //소속 캠프
        public Camp.eKind _campKind = Camp.eKind.None;

        //==================================================

        public delegate void CallBack_State();

        public List<CallBack_State> _onStates_Start = new List<CallBack_State>();
        public List<CallBack_State> _onStates_End = new List<CallBack_State>();


        //==================================================
        //애니
        //==================================================
        public Animator _animator = null;
        protected AnimatorOverrideController _overCtr = null;
        //protected AniClipOverrides _clipOverrides = null;

        protected SpriteRenderer _sprRender = null;
        protected SphereCollider _collider = null;
        protected SpriteMask _sprMask = null;

        //==================================================
        //동작정보
        //==================================================
        public Behavior.eKind _behaviorKind = Behavior.eKind.None;
        public Behavior _behavior = null;
        public float _timeDelta = 0f;  //시간변화량
        public BodyControl _bodyControl = new BodyControl();

        //==================================================
        //상태정보
        //==================================================
        public ePhase _phase = ePhase.None;
        //private bool _death = false;

        //==================================================
        //이동 (방향정보)
        //==================================================
        public Movement _move = null;
        public CellInfo _cellInfo = null;

        //public SortingGroup _sortingGroup = null; 
        //drawcall 증가문제로 제거 , 2가지 아틀라스의 sprite 를 사용하고, 이를 sortingGroup로 묶으면 drawcall 증가가 된다 
        //ref : http://www.devkorea.co.kr/bbs/board.php?bo_table=m03_qna&wr_id=42809

        //==================================================
        //ai
        //==================================================
        public AI _ai = null;


        //==================================================

        //속도차이 때문에 직접 호출해 사용한다. 프로퍼티나 함수로 한번 감싸서 사용하면, 충돌처리에서 5프레임 정도 성능이 떨어진다 
        //_collider.radius 를 바로 호출하면 직접 호출보다 살짝 떨어진다 
        //성능순서 : _collider_radius > _collider.radius > GetCollider_Radius()
        public float _collider_radius = 0f;
        public float _collider_sqrRadius = 0f;
        //public Vector3 _prevLocalPos = ConstV.v3_zero;

        //==================================================
        // 구트리 모델 
        //==================================================
        public SphereModel _sphereModel = null;

        //==================================================
        // 작용힘 - 임시 
        //==================================================
        public Vector3 _force_dir = ConstV.v3_zero;
        public float _force = 0f;


        //==================================================

        public virtual void Init()
        {
            _gameObject = gameObject;
            _transform = transform;
            _getPos3D = _transform.position;
            //SingleO.cellPartition.ToPosition1D(_getPos3D, out _getPos2D, out _getPos1D);
            //Apply_Bounds();
            this.SetPos(_getPos3D);
            //=====================================================

            //_sortingGroup = GetComponent<SortingGroup>();
            _collider = GetComponent<SphereCollider>();
            _collider_radius = _collider.radius;
            _collider_sqrRadius = _collider_radius * _collider_radius;
            //_prevLocalPos = transform.localPosition;

            _move = GetComponent<Movement>();
            _ai = GetComponent<AI>();
            if (null != _ai)
            {
                _ai.Init();
            }

            _sprRender = GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
            _sprMask = GetComponentInChildren<SpriteMask>();

            //=====================================================
            //미리 생성된 오버라이드컨트롤러를 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
            //오버라이트컨트롤러를 직접 생성해서 추가한다
            if (null != _animator)
            {
                //RuntimeAnimatorController new_baseController = RuntimeAnimatorController.Instantiate<RuntimeAnimatorController>(SingleO.resourceManager._base_Animator);
                _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                _overCtr.name = "divide_character_" + _id.ToString();
                _animator.runtimeAnimatorController = _overCtr;

                //ref : https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
                //_clipOverrides = new AniClipOverrides(_overCtr.overridesCount);
                //_overCtr.GetOverrides(_clipOverrides);
                //_clipOverrides.Init(); //chamto test
                //ApplyOverrides 이 함수는 내부적으로 값을 복사하는 것 같음. 프레임이 급격히 떨어짐. 이 방식 사용하지 말기 
                //_overCtr.ApplyOverrides(_clipOverrides);
            }

            //=====================================================
            //셀정보 초기 위치값에 맞춰 초기화
            if (null != SingleO.cellPartition)
            {
                int _getPos1D = SingleO.cellPartition.ToPosition1D(_getPos3D);
                SingleO.cellPartition.AttachCellSpace(_getPos1D, this);
            }


            //=====================================================
            //초기 애니 설정 
            this.Idle();
        }

        public void SetForce(Vector3 nDir, float force)
        {
            _force_dir = nDir * force;
            _force = force;
        }

        public void ReactionForce(Being dst, float deltaTime)
        {
            if (null != (object)dst)
            {
                _force_dir += dst._force_dir; //충돌후 방향을 구한다 
                //_force -= dst._force;
                //if (0 > _force) _force = 0; 
            }

            _getPos3D += _force_dir;

        }

        public Vector3 GetPos3D()
        {
            return _getPos3D;
        }

        public void SetPos(Vector3 newPos)
        {
            _getPos3D = newPos;

            //==============================================
            //!!!!! 구트리 위치 갱신 
            if (null != _sphereModel)
                _sphereModel.SetPos(_getPos3D);
            //==============================================

            //!!!!! 경계상자 위치 갱신
            _getBounds_min.x = _getPos3D.x - _collider_radius;
            _getBounds_min.z = _getPos3D.z - _collider_radius;
            _getBounds_max.x = _getPos3D.x + _collider_radius;
            _getBounds_max.z = _getPos3D.z + _collider_radius;
            //==============================================
        }

        public bool Intersects(Being dst)
        {
            //기본조건 : 두 선분 A , B 에 대하여 
            //A.max >= B.min && A.min <= B.max

            if (_getBounds_max.x >= dst._getBounds_min.x && _getBounds_min.x <= dst._getBounds_max.x
                && _getBounds_max.z >= dst._getBounds_min.z && _getBounds_min.z <= dst._getBounds_max.z)
            {
                return true;
            }

            return false;
        }

        public Bounds old_GetBounds()
        {
            float diameter = _collider_radius * 2f;
            return new Bounds(_getPos3D, new Vector3(diameter, 0, diameter));
        }

        //public float GetCollider_Radius()
        //{
        //    //Assert를 쓰면 심각할정도로 프레임 드랍현상이 발생함. 앞으로 절대 쓰지 말기 - chamto 20181205
        //    //Assert.IsTrue(null != _collider, this.name + " 충돌체가 Null이다");

        //    //if (null == _collider)
        //    //DebugWide.LogRed(this.name);

        //    return _collider.radius;
        //}


        public void SetVisible(bool onoff)
        {
            if (null != (object)_sprRender)
            {
                _sprRender.enabled = onoff;
                //_sprRender.gameObject.SetActive(onoff);
            }
            if (null != (object)_animator)
            {
                _animator.enabled = onoff;
            }
            if (null != (object)_sprMask)
            {
                //_sprMask.enabled = onoff;
                _sprMask.gameObject.SetActive(onoff);
            }

        }

        public void SetColor(Color color)
        {
            if (null != (object)_sprRender)
            {
                _sprRender.color = color;
            }
        }


        public bool isDeath()
        {
            if (0 == _hp_cur) return true;

            return false;
        }

        public void AddHP(int amount)
        {
            _hp_cur += amount;

            if (0 > _hp_cur)
                _hp_cur = 0;

            if (_hp_max < _hp_cur)
                _hp_cur = _hp_max;

        }

        public void Apply_UnityPosition()
        {
            _transform.position = _getPos3D;
        }

        //public void Apply_Bounds()
        //{
        //    _getBounds_min.x = _getPos3D.x - _collider_radius;
        //    _getBounds_min.z = _getPos3D.z - _collider_radius;
        //    _getBounds_max.x = _getPos3D.x + _collider_radius;
        //    _getBounds_max.z = _getPos3D.z + _collider_radius;
        //}

        public void Update_PositionAndBounds()
        {
            //_getPos3D = _transform.position;
            //SingleO.cellPartition.ToPosition1D(_getPos3D, out _getPos2D, out _getPos1D);

            //_getBounds_min.x = _getPos3D.x - _collider_radius;
            //_getBounds_min.z = _getPos3D.z - _collider_radius;
            //_getBounds_max.x = _getPos3D.x + _collider_radius;
            //_getBounds_max.z = _getPos3D.z + _collider_radius;
        }


        public void Update_SpriteMask()
        {
            if (null == (object)_sprMask) return;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _sprMask.sprite = _sprRender.sprite;

        }

        /// <summary>
        /// 그리드상 셀값이 변경되면 셀정보값을 갱신한다 
        /// </summary>
        public void Update_CellSpace()
        {
            if (null == SingleO.cellPartition) return;

            int _getPos1D = SingleO.cellPartition.ToPosition1D(_getPos3D);
            if (null == _cur_cell || _cur_cell._pos1d != _getPos1D)
            {
                SingleO.cellPartition.AttachCellSpace(_getPos1D, this);
            }
        }


        //느린방식 , 순차적으로 제거해야함
        //public void Update_CellInfo()
        //{
        //    Vector3Int cur_posXY_2d = SingleO.gridManager.ToPosition2D(transform.position);

        //    if (null != _cellInfo && _cellInfo._index != cur_posXY_2d)

        //    {
        //        SingleO.gridManager.RemoveCellInfo_Being(_cellInfo._index, this); //이전 위치의 정보 제거
        //        SingleO.gridManager.AddCellInfo_Being(cur_posXY_2d, this); //새로운 위치 정보 추가
        //        _cellInfo = SingleO.gridManager.GetCellInfo(cur_posXY_2d);


        //        //chamto test
        //        //string temp = "count:"+_cellInfo.Count + "  (" + curIdx + ")  ";
        //        //foreach(Being b in _cellInfo)
        //        //{
        //        //    temp += " " + b.name;
        //        //}
        //        //DebugWide.LogBlue(temp); 
        //    }
        //}

        public int GetSortingOrder(int add)
        {
            //  1/0.16 = 6.25 : 곱해지는 값이 최소 6.25보다는 커야 한다
            //y축값이 작을수록 먼저 그려지게 한다. 캐릭터간의 실수값이 너무 작아서 20배 한후 소수점을 버린값을 사용함
            return -(int)(_getPos3D.z * 20f) + add;
        }

        public void Update_SortingOrder(int add)
        {
            _sprRender.sortingOrder = GetSortingOrder(add);
            //_sortingGroup.sortingOrder = GetSortingOrder(add);
        }

        //public void Update_Collision()
        //{
        //    CellInfo cellInfo = null;
        //    StructTile structTile = null;

        //    //1. 3x3그리드 정보를 가져온다
        //    foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[3])
        //    {

        //        //count++;
        //        cellInfo = SingleO.gridManager.GetCellInfo(ix + this._cellInfo._index);
        //        //cellInfo = SingleO.gridManager.GetCellInfo(src._cellInfo._index);
        //        if (null == cellInfo) continue;

        //        foreach (Being dst in cellInfo)
        //        {
        //            //count++;
        //            if (this == dst) continue;
        //            if (null == dst || true == dst.isDeath()) continue;

        //            SingleO.objectManager.CollisionPush(this, dst);
        //        }
        //    }


        //    //동굴벽과 캐릭터 충돌처리 
        //    if (SingleO.gridManager.HasStructTile(this.transform.position, out structTile))
        //    {
        //        SingleO.objectManager.CollisionPush_StructTile(this, structTile);
        //        //CollisionPush_Rigid(src, structTile);
        //    }
        //}

        //한 프레임에서 start 다음에 running 이 바로 시작되게 한다. 상태 타이밍 이벤트는 콜벡함수로 처리한다 
        public virtual bool UpdateAll()
        {

            if (true == isDeath())
            {
                FallDown();

                Update_SortingOrder(-400);
                //_sprRender.sortingOrder = -800; //바닥타일과 동굴벽 보다 위에 있게 하고 다른 챔프들 보다 아래에 있게 한다 
                //if(false == _death)
                //{
                //    FallDown();
                //    _death = true;
                //}

                return false;
            }

            //==============================================
            //캠프값에 따라 기본 캠프 설정
            if (null != _belongCamp)
            {
                if (Camp.eKind.None == _campKind)
                {
                    _campKind = _belongCamp._eCampKind;
                }
                if (_campKind != _belongCamp._eCampKind)
                {
                    _belongCamp = SingleO.campManager.GetDefaultCamp(_campKind);
                }
            }


            //==============================================
            //위치 갱신
            //Update_PositionAndBounds();
            //==============================================

            //Update_CellInfo();
            Update_CellSpace();


            Update_SpriteMask();

            //Update_Collision(); //성능테스트 : objectManager 에서 일괄적으로 전체 객체의 충돌처리 하는게 약간 더 빠르다 

            if (false == _move.IsMoving())
            {
                //_behaviorKind = Behavior.eKind.Idle;
            }

            //int hash = Animator.StringToHash("idle");
            //AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            //if(hash == stateInfo.shortNameHash)
            //{
            //    //Idle();
            //}

            if (Behavior.eKind.Idle_Random == _behaviorKind)
            {
                //_animator.SetInteger("state", (int)Behavior.eKind.Idle);
                Idle_Random();

            }

            //========================================
            _bodyControl.Update(); //행동 진행 갱신 
            //========================================

            //아래 행동 관련 사용 안되는 코드임 - 분석필요 
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
                        foreach (CallBack_State callback in _onStates_Start)
                        {
                            //DebugWide.LogBlue("ssss"); //chamto test
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

            //========================================

            //이동정보에 따라 위치 갱신
            if (null != (object)_move)
                _move.UpdateNextPath();

            //인공지능 갱신
            if (null != (object)_ai)
                _ai.UpdateAI();

            Update_SortingOrder(0);
            //==============================================

            //_overCtr.ApplyOverrides(_clipOverrides);

            //==============================================

            return true;
        }

        public virtual void UpdateAll_Late()
        {
            
        }

        //____________________________________________
        //                  충돌반응
        //____________________________________________

        public virtual void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        {

        }


        //존재간에 부딪힌 경우
        //public void OnCollision_Beings(Being dst)
        //{
        //}

        ////때렸을때
        //public void OnCollision_WhenHit(Being dst)
        //{
        //}

        ////맞았을때
        //public void OnCollision_WhenBeHit(Being dst)
        //{
        //}



        //____________________________________________
        //                  애니메이션  
        //____________________________________________

        uint[] __cache_cur_aniMultiKey = new uint[(int)eAniBaseKind.MAX]; //기본애니 종류 별로 현재애니 정보를 저장한다. 
        public void Switch_Ani(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir)
        {
            if (null == (object)_overCtr) return;

            _sprRender.flipX = false;

            switch (dir)
            {

                case eDirection8.leftUp:
                    {
                        dir = eDirection8.rightUp;
                        _sprRender.flipX = true;
                    }
                    break;
                case eDirection8.left:
                    {
                        dir = eDirection8.right;
                        _sprRender.flipX = true;
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        dir = eDirection8.rightDown;
                        _sprRender.flipX = true;
                    }
                    break;

            }

            //현재상태와 같은 요청이 들어오면 갱신하지 않는다 
            uint next_aniMultiKey = SingleO.resourceManager.ComputeAniMultiKey(being_kind, ani_kind, dir);
            if (next_aniMultiKey == __cache_cur_aniMultiKey[(int)ani_kind]) return;

            //_clipOverrides.SetOverAni(AniOverKey.base_move, SingleO.resourceManager.GetClip(being_kind, ani_kind, dir)); //느려서 못씀 
            //_overCtr[ConstV.GetAniBaseKind(ani_kind)] = SingleO.resourceManager.GetClip(being_kind, ani_kind, dir); 

            AnimationClip clip = SingleO.resourceManager.GetBaseAniClip(ani_kind);
            _overCtr[clip] = SingleO.resourceManager.GetClip(being_kind, ani_kind, dir); //부하가 조금 있다. 중복되는 요청을 걸러내야 한다 
            __cache_cur_aniMultiKey[(int)ani_kind] = next_aniMultiKey;

        }

        //public void non_Switch_Ani(string aniKind, string aniName, eDirection8 dir)
        //{
        //    if (null == _overCtr) return;

        //    _sprRender.flipX = false;
        //    string aniNameSum = "";
        //    switch (dir)
        //    {
        //        //case eDirection8.none:
        //        //{
        //        //    DebugWide.LogRed("Switch_Ani : "+dir  + "값은 처리 할 수 없다 ");
        //        //}
        //        //break;
        //        case eDirection8.leftUp:
        //            {
        //                aniNameSum = aniName + eDirection8.rightUp.ToString();
        //                _sprRender.flipX = true;
        //            }
        //            break;
        //        case eDirection8.left:
        //            {
        //                aniNameSum = aniName + eDirection8.right.ToString();
        //                _sprRender.flipX = true;
        //            }
        //            break;
        //        case eDirection8.leftDown:
        //            {
        //                aniNameSum = aniName + eDirection8.rightDown.ToString();
        //                _sprRender.flipX = true;
        //            }
        //            break;
        //        default:
        //            {
        //                aniNameSum = aniName + dir.ToString();
        //                _sprRender.flipX = false;
        //            }
        //            break;

        //    }

        //    //DebugWide.LogBlue(aniNameSum + "  " + dir); //chamto test

        //    _overCtr[aniKind] = SingleO.resourceManager.GetClip(aniNameSum.GetHashCode()); //chamto test
        //}


        private float __elapsedTime_1 = 0f;
        private float __randTime = 0f;
        public void Idle_Random()
        {
            if ((int)Behavior.eKind.Idle == _animator.GetInteger(ANI_STATE))
            {
                __elapsedTime_1 += Time.deltaTime;


                if (__randTime < __elapsedTime_1)
                {

                    //_eDir8 = (eDirection)Single.rand.Next(0, 8); //0~7

                    //근접 방향으로 랜덤하게 회전하게 한다 
                    int num = Misc.rand.Next(-1, 2); //-1 ~ 1
                    num += (int)_move._eDir8;
                    if (1 > num) num = 8;
                    if (8 < num) num = 1;
                    _move._eDir8 = (eDirection8)num;

                    Switch_Ani(_kind, eAniBaseKind.idle, _move._eDir8);
                    _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Idle);

                    __elapsedTime_1 = 0f;

                    //3~6초가 지났을 때 돌아감
                    __randTime = (float)Misc.rand.Next(3, 7); //3~6
                }

            }

        }

        public void Idle()
        {

            _behaviorKind = Behavior.eKind.Idle;
            _bodyControl.Idle();

            if (true == IsActive_Animator())
            {
                Switch_Ani(_kind, eAniBaseKind.idle, _move._eDir8);
                _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Idle);
                _animator.Play(ANI_STATE_IDLE); //상태전이 없이 바로 적용되게 한다    
            }

        }

        public bool IsActive_Animator()
        {
            if (null != (object)_animator && true == _animator.gameObject.activeInHierarchy)
                return true;

            return false;
        }

        //public void Idle_LookAt()
        //{
        //    //todo..
        //}


        public virtual void OnAniState_Start(int hash_state)
        {

        }


        public virtual void OnAniState_End(int hash_state)
        {

        }



        bool _trans_start = false;
        int _prevCount = -1;
        int _curCount = 0;
        int _nextCount = 0; //동작카운트
        public void Update_AnimatorState(int hash_state, float progress)
        {
            AnimatorStateInfo aniState = _animator.GetCurrentAnimatorStateInfo(0);
            AnimatorTransitionInfo aniTrans = _animator.GetAnimatorTransitionInfo(0);

            float normalTime = 0;

            //* 상태전이 없고, 요청 상태값이 아닌 경우
            //if (true == IsActive_Animator()) 
            //{
            //    //요청 전 동작일 때 값을 초기화 해준다 
            //    _prevCount = -1;
            //    _curCount = 0;
            //    _nextCount = 0;
            //    _trans_start = false;

            //    return;
            //}

            //====================================================================
            //동작 전환전에 상태전이가 있다 
            if (0 != aniTrans.nameHash)
            {
                //상태전이시작
                if (false == _trans_start)
                {
                    _trans_start = true;
                    //DebugWide.LogGreen("상태전이 시작");
                }

                //상태전이 진행비율값이 progress 보다 큰경우 하나 이상의 공격동작이 진행됨
                //if(progress < aniTrans.duration)
                {
                    _curCount = (int)aniTrans.normalizedTime;
                    normalTime = aniTrans.normalizedTime - _curCount;
                }

            }
            //동작으로 전환되었다 
            else if (hash_state == aniState.shortNameHash)
            {
                _trans_start = false; //상태전이에서 동작으로 전환되면, 상태전이 시작값을 해제해 준다
                _curCount = (int)aniState.normalizedTime;
                normalTime = aniState.normalizedTime - _curCount;
            }
            //상태전이 없고, 요청 상태값이 아닌 경우
            else
            {
                //DebugWide.LogRed("상태전이 없고, 요청 상태값이 아닌 경우");
                //요청 전 동작일 때 값을 초기화 해준다 
                _prevCount = -1;
                _curCount = 0;
                _nextCount = 0;
                _trans_start = false;
                return;
            }
            //====================================================================


            if (_curCount != _prevCount)
            {
                _prevCount = _curCount;
                //DebugWide.LogGreen("애니동작 시작" + normalTime + "   cur: " + _curCount + "   next: " + _nextCount);
                this.OnAniState_Start(hash_state);
            }

            //DebugWide.LogGreen("애니동작 진행중 " + normalTime + "   cur: " + _curCount + "   next: " + _nextCount);

            //* 1회 동작이 80% 진행되었다면 동작이 완료되었다고 간주한다. 한동작에서 한번만 수행되게 한다
            if (progress < normalTime && _nextCount == _curCount)
            {
                _nextCount = _curCount + 1; //동작카운트의 소수점을 올림한다
                //DebugWide.LogRed("애니동작 완료 " + normalTime +  "   cur: " + _curCount + "   next: " + _nextCount);
                this.OnAniState_End(hash_state);
            }
        }


        public void FallDown()
        {
            switch (_move._eDir8)
            {
                case eDirection8.left:
                case eDirection8.leftUp:
                case eDirection8.rightUp:
                case eDirection8.up:
                    { _move._eDir8 = eDirection8.up; }
                    break;
                case eDirection8.right:
                case eDirection8.leftDown:
                case eDirection8.rightDown:
                case eDirection8.down:
                    { _move._eDir8 = eDirection8.down; }
                    break;

            }

            _behaviorKind = Behavior.eKind.FallDown;
            Switch_Ani(_kind, eAniBaseKind.fallDown, _move._eDir8);
            _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.FallDown);
            //int hash = Animator.StringToHash("fallDown");
            //_animator.SetTrigger(hash);
        }

        public void Block_Forward(Vector3 dir)
        {
            dir.y = 0;

            _move._eDir8 = Misc.GetDir8_AxisY(dir);

            _behaviorKind = Behavior.eKind.Block;
            Switch_Ani(_kind, eAniBaseKind.move, _move._eDir8);
            _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Block);

        }

        public void UpdateIdle()
        {

            if (true == IsActive_Animator())
            {
                Switch_Ani(_kind, eAniBaseKind.idle, _move._eDir8);

                _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Idle);
            }
        }

        public void Move_Forward(Vector3 dir, float second, bool forward)//, bool setState)
        {

            dir.y = 0;
            _move.SetNextMoving(false);
            _move.Move_Forward(dir, 2f, second);
            eDirection8 eDirection = _move._eDir8;

            //전진이 아니라면 애니를 반대방향으로 바꾼다 (뒷걸음질 효과)
            if (false == forward)
                eDirection = Misc.GetDir8_Reverse_AxisY(eDirection);

            _behaviorKind = Behavior.eKind.Move;
            _bodyControl.Move_0();

            if (true == IsActive_Animator())
            {
                Switch_Ani(_kind, eAniBaseKind.move, _move._eDir8);
                //int hash = Animator.StringToHash("move");
                _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Move);
            }

            //==============================================
            //!!!!! 구트리 위치 갱신 
            //_sphereModel.SetPos(_transform.position);
            //_sphereModel.SetPos(_getPos3D);
            //==============================================

        }

        public void Move_LookAt(Vector3 moveDir, Vector3 lookAtDir, float second)
        {
            moveDir.y = 0;
            _move.SetNextMoving(false);
            _move.Move_LookAt(moveDir, lookAtDir, 2f, second);

            //아이들 상태에서 밀려 이동하는 걸 표현
            _behaviorKind = Behavior.eKind.Move;

            if (true == IsActive_Animator())
            {
                Switch_Ani(_kind, eAniBaseKind.move, _move._eDir8);

                //int hash = Animator.StringToHash("move");
                _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Move);
            }

            //==============================================
            //!!!!! 구트리 위치 갱신 
            //_sphereModel.SetPos(_transform.position);
            //_sphereModel.SetPos(_getPos3D);
            //==============================================
        }

        public void Move_Push(Vector3 dir, float second)
        {
            dir.y = 0;
            _move.SetNextMoving(false);
            _move.Move_Push(dir, 2f, second);

            //아이들 상태에서 밀려 이동하는 걸 표현
            _behaviorKind = Behavior.eKind.Idle;

            if (true == IsActive_Animator())
            {
                Switch_Ani(_kind, eAniBaseKind.idle, _move._eDir8);

                //int hash = Animator.StringToHash("move");
                _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Idle);
            }

            //==============================================
            //!!!!! 구트리 위치 갱신 
            //_sphereModel.SetPos(_transform.position);
            //_sphereModel.SetPos(_getPos3D);
            //==============================================
        }

        public void MoveToTarget(Vector3 targetPos, float speed)
        {
            targetPos.y = 0;
            _move.SetNextMoving(false);
            _move.MoveToTarget(targetPos, speed);

            _behaviorKind = Behavior.eKind.Move;
            Switch_Ani(_kind, eAniBaseKind.move, _move._eDir8);
            _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Move);
            //int hash = Animator.StringToHash("move");
            //_animator.SetTrigger(hash);

            //_animator.Play("idle"); //상태전이 없이 바로 적용
        }


        //____________________________________________
        //                  터치 이벤트   
        //____________________________________________

        //private Vector3 __startPos = ConstV.v3_zero;
        //private LineSegment3 __lineSeg = LineSegment3.zero;
        //private void TouchBegan()
        //{
        //    RaycastHit hit = SingleO.touchEvent.GetHit3D();
        //    __startPos = hit.point;
        //    __startPos.y = 0f;


        //    if (8 > _hp_cur)
        //    {
        //        //다시 살리기
        //        _animator.Play(ANI_STATE_IDLE);
        //        //_death = false;
        //        _hp_cur = 10;
        //        _behaviorKind = Behavior.eKind.Idle;
        //    }

        //    SingleO.uiMain.SelectLeader(_kind.ToString());

        //}



        //private void TouchMoved()
        //{
        //    //DebugWide.LogBlue("TouchMoved " + Single.touchProcess.GetTouchPos());

        //    RaycastHit hit = SingleO.touchEvent.GetHit3D();

        //    Vector3 dir = hit.point - this.transform.position;
        //    dir.y = 0;
        //    //DebugWide.LogBlue("TouchMoved " + dir);

        //    //SingleO.objectManager.LookAtTarget(this, GridManager.NxN_MIN);

        //}

        //private void TouchEnded()
        //{
        //    RaycastHit hit = SingleO.touchEvent.GetHit3D();


        //    //DebugWide.LogBlue("TouchEnded " + Single.touchProcess.GetTouchPos());
        //    //_move.MoveToTarget(transform.position, 1f); //이동종료
        //    _move.SetNextMoving(false);

        //    Switch_Ani(_kind, eAniBaseKind.idle, _move._eDir8);
        //    //_animator.SetInteger("state", (int)eState.Idle);
        //    _animator.Play(ANI_STATE_IDLE);


        //    _behaviorKind = Behavior.eKind.Idle_Random;
        //    SingleO.objectManager.SetAll_Behavior(Behavior.eKind.Idle_Random);


        //    _behaviorKind = Behavior.eKind.Move;

        //    _move.MoveToTarget(hit.point, 1f);

        //}

    }

}

