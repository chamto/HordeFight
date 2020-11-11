using System.Collections;
using UnityEngine;
//using UnityEngine.Assertions;
using UtilGS9;

namespace HordeFight
{
    //========================================================

    /// <summary>
    /// 뛰어난 존재 
    /// </summary>
    public class ChampUnit : Being
    {

        //직책 
        //public enum eJobPosition
        //{
        //    None = 0,
        //    Squad_Leader, //분대장 10
        //    Platoon_Leader, //소대장 20
        //    Company_Commander, //중대장 100
        //    Battalion_Commander, //대대장 300
        //    Division_Commander, //사단장 , 독립된 부대단위 전술을 펼칠수 있다 3000
        //}
        //public eJobPosition _jobPosition = eJobPosition.None;


        //전용 effect
        public enum eEffectKind
        {
            Aim,        //조준점
            Dir,        //방향
            Emotion,    //감정표현
            Circle,     //선택원
            Bar_Red,    //생명바
            Bar_Blue,   //체력바

            Max,
        }


        //==================================================

        public ushort _level = 1;

        //능력치1 
        public ushort _power = 1;
        public float _mt_range_min = 0.2f; //충돌원 바깥부터 시작되는 길이값
        public float _mt_range_max = 0.5f; //충돌원 바깥부터 시작되는 길이값

        //보조정보 
        //private Geo.Sphere _collider;
        //private Vector3 _direction = Vector3.forward;

        //==================================================

        //주시대상
        public Being _looking = null;

        //소유아이템
        public Inventory _inventory = null;

        //전용effect
        private Transform[] _effect = new Transform[(int)eEffectKind.Max];
        private SpriteRenderer _bar_red = null;
        private SpriteRenderer _bar_blue = null;


        //전용UI
        //public int _UIID_circle_collider = -1;
        //public int _UIID_hp = -1;
        public LineControl.Info _ui_circle;
        public LineControl.Info _ui_hp;

        //==================================================

        //진영정보
        //public Camp _belongCamp = null; //소속 캠프
        //public Camp.eKind _campKind = Camp.eKind.None;

        public Geo.Sphere _activeRange = Geo.Sphere.Zero;

        //==================================================

        //가지정보
        public Limbs _limbs = null;
        //==================================================

        public float attack_range_min
        {
            get { return this._collider_radius + _mt_range_min * GridManager.MeterToWorld; }
        }
        public float attack_range_max
        {
            get { return this._collider_radius + _mt_range_max * GridManager.MeterToWorld; }
        }

        //==================================================



        //private void Start()
        //{
        //          //DebugWide.LogBlue("ChampUnit");
        //          //this.Init();
        //}

        //private void Update()
        //{
        //    this.UpdateAll();
        //}

        //LineControl LINE_CONTROL = null;

        public override void Init()
        {
            base.Init();

            _activeRange.radius = GridManager.ONE_METER * 1f;

            //=====================================================
            // 전용 effect 설정 


            Transform effectTr = Hierarchy.GetTransform(transform, "effectIcon");
            _effect[(int)eEffectKind.Aim] = Hierarchy.GetTransform(effectTr, "aim");
            _effect[(int)eEffectKind.Dir] = Hierarchy.GetTransform(effectTr, "dir");
            _effect[(int)eEffectKind.Emotion] = Hierarchy.GetTransform(effectTr, "emotion");
            _effect[(int)eEffectKind.Circle] = Hierarchy.GetTransform(effectTr, "circle");
            _effect[(int)eEffectKind.Bar_Red] = Hierarchy.GetTransform(effectTr, "bar_red");
            _effect[(int)eEffectKind.Bar_Blue] = Hierarchy.GetTransform(effectTr, "bar_blue");

            _bar_red = Hierarchy.GetTypeObject<SpriteRenderer>(_effect[(int)eEffectKind.Bar_Red], "spr");
            _bar_blue = Hierarchy.GetTypeObject<SpriteRenderer>(_effect[(int)eEffectKind.Bar_Blue], "spr");

            //아틀라스에서 가져온 sprite로 변경하여 시험 
            //_effect[(int)eEffectKind.Aim].sprite = SingleO.resourceManager.GetSprite_Effect("aim_1");
            //_effect[(int)eEffectKind.Dir].sprite = SingleO.resourceManager.GetSprite_Effect("effect_dir");
            //_effect[(int)eEffectKind.Emotion].sprite = SingleO.resourceManager.GetSprite_Effect("effect_surprise");
            //_effect[(int)eEffectKind.Hand_Left].sprite = SingleO.resourceManager.GetSprite_Effect("effect_sheld_0");
            //_effect[(int)eEffectKind.Hand_Right].sprite = SingleO.resourceManager.GetSprite_Effect("effect_attack");

            //DebugWide.LogBlue(_effect[(int)eEffectKind.Dir].sprite.name); //chamto test

            //_effect[(int)eEffectKind.Aim] = SingleO.hierarchy.GetTransformA(transform, "effect/aim");
            //_effect[(int)eEffectKind.Dir] = SingleO.hierarchy.GetTransformA(transform, "effect/dir");
            //_effect[(int)eEffectKind.Emotion] = SingleO.hierarchy.GetTransformA(transform, "effect/emotion");
            //_effect[(int)eEffectKind.Hand_Left] = SingleO.hierarchy.GetTransformA(transform, "effect/hand_left");
            //_effect[(int)eEffectKind.Hand_Right] = SingleO.hierarchy.GetTransformA(transform, "effect/hand_right");



            //=====================================================
            // 전용 ui 설정 

            //todo : 성능이 무척 안좋은 처리 , 스프라이트HP바로 바꾸기 
            //_ui_circle = SingleO.lineControl.Create_Circle_AxisY(this.transform, _activeRange.radius, Color.green);
            //_ui_hp = SingleO.lineControl.Create_LineHP_AxisY(this.transform);
            //_ui_circle.gameObject.SetActive(false);
            //_ui_hp.gameObject.SetActive(false);
            ////SingleO.lineControl.SetScale(_UIID_circle_collider, 2f);
        }


        public override bool UpdateAll()
        {
            bool result = base.UpdateAll();
            if (true == result)
            {
                ApplyUI_HPBar();

                if (null != (object)_limbs)
                {
                    //_limbs.UpdateAll(); //가지들 갱신 
                    //_limbs.Rotate(_move._direction); //move 에서 오일러각을 따로 구한것을 사용하도록 코드 수정하기     
                }

            }

            return result;
        }

        public void Attack(Vector3 dir)
        {
            this.Attack(dir, null);

        }


        public Shot _shot = null;
        Vector3 _appointmentDir = ConstV.v3_zero;
        public Being _target = null;
        public void Attack(Vector3 dir, Being target)
        {
            
            _move._eDir8 = Misc.GetDir8_AxisY(dir);

            _ani.Play(_kind, eAniBaseKind.attack, _move._eDir8);
            //Switch_Ani(_kind, eAniBaseKind.attack, _move._eDir8);
            //_animator.SetInteger(ANI_STATE, (int)eAniBaseKind.attack);

            //_animator.Play(ANI_STATE_ATTACK, 0, 0.0f); //애니의 노멀타임을 설정한다  
            //_animator.speed = 0.5f; //속도를 설정한다 
            //_target = target;

            //1회 공격중 방향변경 안되게 하기. 1회 공격시간의 80% 경과시 콜백호출 하기.
            _appointmentDir = dir;
            //Update_AnimatorState(ANI_STATE_ATTACK, 0.8f);

            //임시코드 
            if (eKind.spearman == _kind || eKind.archer == _kind || eKind.catapult == _kind || eKind.cleric == _kind || eKind.conjurer == _kind)
            {

                if (null == (object)_shot || false == _shot._on_theWay)
                {
                    _shot = SingleO.objectManager.GetNextShot();
                    if (null != (object)_shot)
                    {
                        Vector3 targetPos = ConstV.v3_zero;
                        if (null != (object)target)
                        {
                            //targetPos = target.transform.position;
                            targetPos = target.GetPos3D();
                        }
                        else
                        {
                            _appointmentDir.Normalize();
                            //targetPos = this.transform.position + _appointmentDir * attack_range_max;
                            targetPos = _getPos3D + _appointmentDir * attack_range_max;
                        }
                        //_shot.ThrowThings(this, this.transform.position, targetPos);
                        _shot.ThrowThings(this, _getPos3D, targetPos);
                    }
                }

            }
        }

        //임시처리
        public override void OnAniState_Start(int hash_state)
        {
            //DebugWide.LogBlue("OnAniState_Start :" + hash_state); //chamto test

            if (hash_state == _ani.ANI_STATE_ATTACK)
            {
                //예약된 방향값 설정
                _move._eDir8 = Misc.GetDir8_AxisY(_appointmentDir);
                _move._direction = Misc.GetDir8_Normal3D_AxisY(_move._eDir8);

                _ani.Play(_kind, eAniBaseKind.attack, _move._eDir8);
                //Switch_Ani(_kind, eAniBaseKind.attack, _move._eDir8);

                //this.SetActiveEffect(eEffectKind.Hand_Right, true);
            }
        }

        //임시처리
        public override void OnAniState_End(int hash_state)
        {
            //DebugWide.LogYellow("OnAniState_End :" + hash_state + "  " + _hash_attack + "   "+ _target); //chamto test

            if (hash_state == _ani.ANI_STATE_ATTACK)
            {

                //목표에 피해를 준다
                if (null != _target)
                {
                    //DebugWide.LogYellow("OnAniState_End :" + hash_state); //chamto test

                    //this.SetActiveEffect(eEffectKind.Hand_Right, false);

                    _target.AddHP(-1);
                    ChampUnit target_champ = _target as ChampUnit;
                    if (null != target_champ)
                    {
                        StartCoroutine(target_champ.Damage());
                        //target_champ.ApplyUI_HPBar();
                    }

                }

            }
        }

        public void ApplyUI_HPBar()
        {
            //HP갱신 
            //_ui_hp.SetLineHP((float)_hp_cur / (float)_hp_max);

            //Vector2 temp = _effect[(int)eEffectKind.Bar_Red].size;
            //temp.x = (float)_hp_cur / (float)_hp_max;
            //_effect[(int)eEffectKind.Bar_Red].size = temp;

            Vector2 temp = _bar_red.size;
            temp.x = (float)_hp_cur / (float)_hp_max;
            _bar_red.size = temp;

        }

        bool __in_corutin_Damage = false;
        public IEnumerator Damage()
        {
            //같은 코루틴을 요청하면 빨강색으로 변경후 종료한다  
            //if (true == __in_corutin_Damage)
            //{
            //    _sprRender.color = Color.red;
            //    yield break;
            //}

            //for (int i = 0; i < 10; i++)
            //{
            //    __in_corutin_Damage = true;

            //    _sprRender.color = Color.Lerp(Color.red, Color.white, i / 10f);
            //    //_sprRender.color = Color.red;
            //    yield return new WaitForSeconds(0.05f);
            //}
            this.SetActiveEffect(eEffectKind.Emotion, true);
            _ani._sprRender.color = Color.red;

            if (true == __in_corutin_Damage)
                yield break;

            __in_corutin_Damage = true;
            yield return new WaitForSeconds(0.5f);

            this.SetActiveEffect(eEffectKind.Emotion, false);
            _ani._sprRender.color = Color.white;
            __in_corutin_Damage = false;

            yield break;
        }


        //____________________________________________
        //                  충돌반응
        //____________________________________________

        public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        {
            Move_Push(dir, meterPerSecond);
        }


        //____________________________________________
        //                  전용 effect 처리
        //____________________________________________
        public void SetActiveEffect(eEffectKind kind, bool value)
        {
            if (null == _effect[(int)kind]) return;

            _effect[(int)kind].gameObject.SetActive(value);
        }


        //____________________________________________
        //                  디버그 정보 출력
        //____________________________________________

        public bool _GIZMOS_BEHAVIOR = false;
        void OnDrawGizmos()
        {
            if (true == _GIZMOS_BEHAVIOR)
                Debug_DrawGizmos_Behavior();
        }

        Vector3 _debug_dir = Vector3.zero;
        Quaternion _debug_q = Quaternion.identity;
        Vector3 _debug_line = Vector3.zero;
        public void Debug_DrawGizmos_Behavior()
        {
            //BodyControl.Part HL = _bodyControl._parts[(int)BodyControl.Part.eKind.Hand_Left];
            //BodyControl.Part HR = _bodyControl._parts[(int)BodyControl.Part.eKind.Hand_Right];

            Vector3 posBody = this.GetPos3D();
            Quaternion quater_r = Quaternion.FromToRotation(UtilGS9.ConstV.v3_forward, _move._direction);
            //Vector3 posHL = quater_r * HL._pos_standard + posBody;
            //Vector3 posHR = quater_r * HR._pos_standard + posBody;

            float weaponArc_degree = 45f;
            float weaponArc_radius_far = 3f;
            Vector3 weaponArc_dir = _move._direction;
            //*
            //어깨 기준점 
            //Gizmos.color = Color.gray;
            //Gizmos.DrawWireSphere(posHL, HL._range_max);
            //Gizmos.DrawWireSphere(posHR, HR._range_max);

            ////skill 진행 상태 출력 
            //DebugWide.PrintText(posHR, Color.white, _bodyControl._skill_current._kind.ToString() + "  st: " + _bodyControl._state_current.ToString() + "   t: " + _bodyControl._timeDelta.ToString("00.00"));

            //공격 범위 - 호/수직 : Vector3.forward
            //eTraceShape tr = eTraceShape.None;
            //_data.GetBehavior().attack_shape

            if (0 != weaponArc_degree)
            {
                Gizmos.color = Color.yellow;
                _debug_q = Quaternion.AngleAxis(weaponArc_degree * 0.5f, Vector3.up);
                _debug_dir = _debug_q * weaponArc_dir;
                Gizmos.DrawLine(posBody, posBody + _debug_dir * weaponArc_radius_far);
                _debug_q = Quaternion.AngleAxis(weaponArc_degree * -0.5f, Vector3.up);
                _debug_dir = _debug_q * weaponArc_dir;
                Gizmos.DrawLine(posBody, posBody + _debug_dir * weaponArc_radius_far);
            }

            //공격 범위 - 호/수평 : Vector3.up

            //캐릭터카드 충돌원
            //Gizmos.color = Color.black;
            //Gizmos.DrawWireSphere(posSt, _data.GetCollider_Sphere().radius);

            //캐릭터 방향 
            Gizmos.color = Color.black;
            Gizmos.DrawLine(posBody, posBody + _move._direction * 4);
            //Gizmos.DrawSphere(posBody + _move._direction * 4, 0.2f);

            //공격 무기이동 경로
            //Vector3 weapon_curPos = posBody + _bodyControl.CurrentDistance() * weaponArc_dir;
            //_debug_line.y = -0.5f;
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(posBody, weapon_curPos);
            //Gizmos.DrawWireSphere(weapon_curPos, 0.1f);
            //*/

            //칼죽이기 가능 범위
            //_debug_line.y = -1f;
            //Gizmos.color = Color.green;
            //Gizmos.DrawLine(_data.GetWeaponPosition(_data.GetBehavior().cloggedTime_0) + _debug_line, _data.GetWeaponPosition(_data.GetBehavior().cloggedTime_1) + _debug_line);

            //공격점 범위 
            //_debug_line.y = -1.5f;
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(_data.GetWeaponPosition(_data.GetBehavior().eventTime_0) + _debug_line, _data.GetWeaponPosition(_data.GetBehavior().eventTime_1) + _debug_line);
        }
    }

}

