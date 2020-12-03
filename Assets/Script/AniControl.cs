using UnityEngine;
using UtilGS9;


namespace HordeFight
{
    public class AniControl
    {
        public readonly int ANI_STATE = Animator.StringToHash("state");
        public readonly int ANI_STATE_IDLE = Animator.StringToHash("idle");
        public readonly int ANI_STATE_MOVE = Animator.StringToHash("move");
        public readonly int ANI_STATE_ATTACK = Animator.StringToHash("attack");
        public readonly int ANI_STATE_FALLDOWN = Animator.StringToHash("fallDown");
        public int[] HASH_STATE = new int[(int)eAniBaseKind.MAX];

        public Animator _animator = null;
        public AnimatorOverrideController _overCtr = null;
        public SpriteRenderer _sprRender = null;


        public void Init(Transform parent, uint id)
        {
            HASH_STATE[(int)eAniBaseKind.idle] = ANI_STATE_IDLE;
            HASH_STATE[(int)eAniBaseKind.move] = ANI_STATE_MOVE;
            HASH_STATE[(int)eAniBaseKind.attack] = ANI_STATE_ATTACK;
            HASH_STATE[(int)eAniBaseKind.fallDown] = ANI_STATE_FALLDOWN;

            _sprRender = parent.GetComponentInChildren<SpriteRenderer>();
            _animator = parent.GetComponentInChildren<Animator>();

            //미리 생성된 오버라이드컨트롤러를 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
            //오버라이트컨트롤러를 직접 생성해서 추가한다
            if (null != _animator)
            {
                _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                _overCtr.name = "divide_character_" + id.ToString();
                _animator.runtimeAnimatorController = _overCtr;
            }
        }

        public bool IsActive_Animator()
        {
            if (null != (object)_animator && true == _animator.gameObject.activeInHierarchy)
                return true;

            return false;
        }

        public bool IsAniState(eAniBaseKind ani_kind)
        {
            if (null == (object)_animator) return false;

            if ((int)ani_kind == _animator.GetInteger(ANI_STATE))
                return true;

            return false;
        }

        public void Play(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir)
        {
            if (false == IsActive_Animator()) return;

            //_move._eDir8 = Misc.GetDir8_AxisY(dir);
            _animator.SetInteger(ANI_STATE, (int)ani_kind);
            Switch_Ani(being_kind, ani_kind, dir);

            //_animator.Play(ANI_STATE_ATTACK, 0, 0.0f); //애니의 노멀타임을 설정한다  
            //_animator.speed = 0.5f; //속도를 설정한다 
        }

        //에니메이터의 전이과정 없이 즉시재생시킨다 
        public void PlayNow(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir)
        {
            if (false == IsActive_Animator()) return;

            _animator.SetInteger(ANI_STATE, (int)ani_kind);
            Switch_Ani(being_kind, ani_kind, dir);
            _animator.Play(HASH_STATE[(int)ani_kind]);

        }

        public void PlayNow(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir, float normalTime)
        {
            if (false == IsActive_Animator()) return;

            _animator.SetInteger(ANI_STATE, (int)ani_kind);
            Switch_Ani(being_kind, ani_kind, dir);

            _animator.Play(HASH_STATE[(int)ani_kind], 0, normalTime);
        }

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


            AnimationClip base_clip = SingleO.resourceManager.GetBaseAniClip(ani_kind);
            _overCtr[base_clip] = SingleO.resourceManager.GetClip(being_kind, ani_kind, dir); //부하가 조금 있다. 중복되는 요청을 걸러내야 한다 
            __cache_cur_aniMultiKey[(int)ani_kind] = next_aniMultiKey;

        }

        //애니메이터 상태별 상세값이 어떻게 변화되는지 보기 위해 작성함
        //ChampStateMachine.OnStateEnter 에서 전이중일 때, "상태 시작함수"의 현재상태값이 "다음 상태"로 나오는 반면, 아래함수로 직접 출력해 보면 "현재 상태"로 나온다
        public void Print_AnimatorState()
        {
            AnimatorStateInfo aniState = _animator.GetCurrentAnimatorStateInfo(0);
            AnimatorTransitionInfo aniTrans = _animator.GetAnimatorTransitionInfo(0);
            //DebugWide.LogBlue(_selected._animator.speed);

            float normalTime = aniState.normalizedTime - (int)aniState.normalizedTime;
            float playTime = aniState.length;
            string stateName = SingleO.hashMap.GetString(aniState.shortNameHash);
            string transName = SingleO.hashMap.GetString(aniTrans.nameHash);
            int hash = Animator.StringToHash("attack");
            if (hash == aniState.shortNameHash)
            {

                DebugWide.LogBlue(aniTrans.nameHash + "  plt: " + playTime + "   tr: " + transName + "    du: " + aniTrans.duration + "   trNt: " + aniTrans.normalizedTime +
                                  "  :::   st: " + stateName + "   ct: " + (int)aniState.normalizedTime + "  stNt:" + normalTime);
            }
            else
            {
                DebugWide.LogRed(aniTrans.nameHash + "  plt: " + playTime + "   tr: " + transName + "    du: " + aniTrans.duration + "   trNt: " + aniTrans.normalizedTime +
                                 "  :::   st: " + stateName + "   ct: " + (int)aniState.normalizedTime + "  stNt:" + normalTime);
            }
        }

        //--------------------------------------------------

    }//end

}


