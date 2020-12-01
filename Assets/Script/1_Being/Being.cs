using System;
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
        //public int ANI_STATE = Animator.StringToHash("state");
        //public int ANI_STATE_IDLE = Animator.StringToHash("idle");
        //public int ANI_STATE_MOVE = Animator.StringToHash("move");
        //public int ANI_STATE_ATTACK = Animator.StringToHash("attack");
        //public int ANI_STATE_FALLDOWN = Animator.StringToHash("fallDown");

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
        public float _force = 1f; //밀거나 버티는 힘, 클수록 강한힘  

        //==================================================

        //진영정보
        public Camp _belongCamp = null; //소속 캠프
        public Camp.eKind _campKind = Camp.eKind.None;

        //==================================================


        //==================================================
        //애니
        //==================================================
        //public Animator _animator = null;
        //protected AnimatorOverrideController _overCtr = null;
        //protected AniClipOverrides _clipOverrides = null;
        //protected SpriteRenderer _sprRender = null;

        public AniControl _ani = new AniControl();

        //protected SpriteMask _sprMask = null;

        public Effect _effect = new Effect();

        //==================================================
        //상태정보
        //==================================================

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
        public SphereCollider _collider = null;
        public float _collider_radius = 0f;
        public float _collider_sqrRadius = 0f;
        //public Vector3 _prevLocalPos = ConstV.v3_zero;

        //==================================================
        // 구트리 모델 
        //==================================================
        public SphereModel _sphereModel = null;


        //==================================================

        public virtual void Init()
        {
            _gameObject = gameObject;
            _transform = transform;
            _getPos3D = _transform.position;
            //SingleO.cellPartition.ToPosition1D(_getPos3D, out _getPos2D, out _getPos1D);
            //Apply_Bounds();
            this.SetPos(_getPos3D);
            Apply_UnityPosition();
            //=====================================================

            //_sortingGroup = GetComponent<SortingGroup>();
            //_collider = GetComponent<SphereCollider>();
            //_collider_radius = _collider.radius;
            //_collider_sqrRadius = _collider_radius * _collider_radius;
            //_prevLocalPos = transform.localPosition;

            //_move = GetComponent<Movement>();
            //_ai = GetComponent<AI>();
            //if (null != _ai)
            //{
            //    _ai.Init();
            //}

            //_sprRender = GetComponentInChildren<SpriteRenderer>();
            //_animator = GetComponentInChildren<Animator>();
            //_sprMask = GetComponentInChildren<SpriteMask>();
            _ani.Init(transform, _id);
            // 전용 effect 설정 
            _effect.Init(transform);
            //=====================================================
            ////미리 생성된 오버라이드컨트롤러를 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
            ////오버라이트컨트롤러를 직접 생성해서 추가한다
            //if (null != _animator)
            //{
            //    //RuntimeAnimatorController new_baseController = RuntimeAnimatorController.Instantiate<RuntimeAnimatorController>(SingleO.resourceManager._base_Animator);
            //    _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            //    _overCtr.name = "divide_character_" + _id.ToString();
            //    _animator.runtimeAnimatorController = _overCtr;

            //    //ref : https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
            //    //_clipOverrides = new AniClipOverrides(_overCtr.overridesCount);
            //    //_overCtr.GetOverrides(_clipOverrides);
            //    //_clipOverrides.Init(); //chamto test
            //    //ApplyOverrides 이 함수는 내부적으로 값을 복사하는 것 같음. 프레임이 급격히 떨어짐. 이 방식 사용하지 말기 
            //    //_overCtr.ApplyOverrides(_clipOverrides);
            //}

            //=====================================================
            //셀정보 초기 위치값에 맞춰 초기화
            if (null != SingleO.cellPartition)
            {
                int _getPos1D = SingleO.cellPartition.ToPosition1D(_getPos3D);
                SingleO.cellPartition.AttachCellSpace(_getPos1D, this);
            }


            //=====================================================
            //초기 애니 설정 
            //_skill_idle.Idle();
            //this.Idle();
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
            if (null != (object)_ani._sprRender)
            {
                _ani._sprRender.enabled = onoff;
                //_sprRender.gameObject.SetActive(onoff);
            }
            if (null != (object)_ani._animator)
            {
                _ani._animator.enabled = onoff;
            }

        }

        public void SetColor(Color color)
        {
            if (null != (object)_ani._sprRender)
            {
                _ani._sprRender.color = color;
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

        //public Bounds old_GetBounds()
        //{
        //    float diameter = _collider_radius * 2f;
        //    return new Bounds(_getPos3D, new Vector3(diameter, 0, diameter));
        //}

        //public void Update_PositionAndBounds()
        //{
        //    //_getPos3D = _transform.position;
        //    //SingleO.cellPartition.ToPosition1D(_getPos3D, out _getPos2D, out _getPos1D);

        //    //_getBounds_min.x = _getPos3D.x - _collider_radius;
        //    //_getBounds_min.z = _getPos3D.z - _collider_radius;
        //    //_getBounds_max.x = _getPos3D.x + _collider_radius;
        //    //_getBounds_max.z = _getPos3D.z + _collider_radius;
        //}


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
            _ani._sprRender.sortingOrder = GetSortingOrder(add);
            //_sortingGroup.sortingOrder = GetSortingOrder(add);
        }


        //한 프레임에서 start 다음에 running 이 바로 시작되게 한다. 상태 타이밍 이벤트는 콜벡함수로 처리한다 
        public virtual bool UpdateAll()
        {
            _getPos3D = _transform.position; //chamto test

            _effect.Apply_BarRed((float)_hp_cur / (float)_hp_max);

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

        //기본 아이들 함수 
        public virtual void Idle()
        {
            _ani.Play(_kind, eAniBaseKind.idle, _move._eDir8);
        }

        public void Idle_LookAt(Vector3 lookAtDir)
        {
            lookAtDir.y = 0;
            _move.SetDirection(lookAtDir);

            _ani.Play(_kind, eAniBaseKind.idle, _move._eDir8);
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


        private float __elapsedTime_1 = 0f;
        private float __randTime = 0f;
        public void Idle_Random()
        {
            if(_ani.IsAniState(eAniBaseKind.idle))
            //if((int)eAniBaseKind.idle == _animator.GetInteger(ANI_STATE))
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

                    _ani.Play(_kind, eAniBaseKind.idle, _move._eDir8);
                    //Switch_Ani(_kind, eAniBaseKind.idle, _move._eDir8);
                    //_animator.SetInteger(ANI_STATE, (int)eAniBaseKind.idle);

                    __elapsedTime_1 = 0f;

                    //3~6초가 지났을 때 돌아감
                    __randTime = (float)Misc.rand.Next(3, 7); //3~6
                }

            }

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
            AnimatorStateInfo aniState = _ani._animator.GetCurrentAnimatorStateInfo(0);
            AnimatorTransitionInfo aniTrans = _ani._animator.GetAnimatorTransitionInfo(0);

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
                _curCount = (int)aniState.normalizedTime; //1초로 가정된 코드 
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

            _ani.Play(_kind, eAniBaseKind.fallDown, _move._eDir8);
            //Switch_Ani(_kind, eAniBaseKind.fallDown, _move._eDir8);
            //_animator.SetInteger(ANI_STATE, (int)eAniBaseKind.fallDown);

        }

        public void Move_Forward(Vector3 dir, float second)
        {
            dir.y = 0;
            _move.SetNextMoving(false);
            _move.Move_Forward(dir, 2f, second);

            _ani.Play(_kind, eAniBaseKind.move, _move._eDir8);

        }

        public void Move_LookAt(Vector3 moveDir, Vector3 lookAtDir, float second)
        {
            moveDir.y = 0;
            _move.SetNextMoving(false);
            _move.Move_LookAt(moveDir, lookAtDir, 2f, second);

            _ani.Play(_kind, eAniBaseKind.move, _move._eDir8);
        }

        public void Move_Push(Vector3 dir, float second)
        {
            dir.y = 0;
            _move.SetNextMoving(false);
            _move.Move_Push(dir, 2f, second);

            //아이들 상태에서 밀려 이동하는 걸 표현

            _ani.Play(_kind, eAniBaseKind.idle, _move._eDir8);

        }

        public void MoveToTarget(Vector3 targetPos, float speed)
        {
            targetPos.y = 0;
            _move.SetNextMoving(false);
            _move.MoveToTarget(targetPos, speed);

            _ani.Play(_kind, eAniBaseKind.move, _move._eDir8);

            //_animator.Play("idle"); //상태전이 없이 바로 적용
        }

    }

}


