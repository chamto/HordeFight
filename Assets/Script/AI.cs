using UnityEngine;

using UtilGS9;


//========================================================
//==================       인공 지능      ==================
//========================================================
namespace HordeFight
{
    public class AI : MonoBehaviour
    {

        public enum eState
        {
            Detect, //탐지
            Chase,  //추격
            Attack,  //공격
            Escape, //도망
            Roaming, //배회하기
        }
        private eState _state = eState.Roaming;
        private ChampUnit _me = null;
        //private Being _target = null;
        private Vector3 _ai_Dir = ConstV.v3_zero;
        private float _elapsedTime = 0f;


        public bool _ai_running = false;

        public void Init()
        {
            _me = GetComponent<ChampUnit>();
            _ai_Dir = Misc.GetDir8_Random_AxisY(); //초기 임의의 방향 설정
        }



        public void UpdateAI()
        {
            //_me.Attack(_me._move._direction); //chamto test

            if (false == _ai_running) return;

            //if (true == _me.isDeath()) return;


            this.StateUpdate();
        }


        public bool Situation_Is_Enemy()
        {
            ChampUnit champTarget = _me._looking as ChampUnit;
            if (null == (object)champTarget) return false;

            //불확실한 대상
            if (null == (object)_me._looking || null == (object)champTarget._belongCamp || null == (object)_me._belongCamp) return false;

            Camp.eRelation relation = SingleO.campManager.GetRelation(_me._belongCamp._eCampKind, champTarget._belongCamp._eCampKind);

            if (Camp.eRelation.Enemy == relation) return true;

            return false;
        }


        private const int _FAILURE = -1;
        private const int _OUT_RANGE_MIN = 0;
        private const int _OUT_RANGE_MAX = 1;
        private const int _IN_RANGE = 2;
        public int Situation_Is_InRange(float meter_rangeMin, float meter_rangeMax)
        {
            if (null == (object)_me._looking) return _FAILURE;

            float sqrDis = VOp.Minus(_me.GetPos3D(), _me._looking.GetPos3D()).sqrMagnitude;

            float sqrRangeMax = (meter_rangeMax * GridManager.ONE_METER) * (meter_rangeMax * GridManager.ONE_METER);
            float sqrRangeMin = (meter_rangeMin * GridManager.ONE_METER) * (meter_rangeMin * GridManager.ONE_METER);

            if (sqrRangeMin <= sqrDis)
            {
                if (sqrDis <= sqrRangeMax)
                    return _IN_RANGE;
                else
                    return _OUT_RANGE_MAX;
            }


            return _OUT_RANGE_MIN;
        }

        public int Situation_Is_AttackInRange()
        {
            if (null == (object)_me._looking) return _FAILURE;

            float sqrDis = VOp.Minus(_me.GetPos3D(), _me._looking.GetPos3D()).sqrMagnitude;

            float sqrRangeMax = (_me.attack_range_max + _me._looking._collider_radius) * (_me.attack_range_max + _me._looking._collider_radius);
            float sqrRangeMin = (_me.attack_range_min + _me._looking._collider_radius) * (_me.attack_range_min + _me._looking._collider_radius);

            if (sqrRangeMin <= sqrDis)
            {
                if (sqrDis <= sqrRangeMax)
                    return _IN_RANGE;
                else
                    return _OUT_RANGE_MAX;
            }


            return _OUT_RANGE_MIN;
        }

        public void StateUpdate()
        {
            switch (_state)
            {
                case eState.Detect:
                    {
                        //공격대상이 맞으면 추격한다.
                        if (true == Situation_Is_Enemy())
                        {
                            _state = eState.Chase;
                        }
                        //공격대상이 아니면 다시 배회한다.
                        else
                        {
                            _state = eState.Roaming;
                        }

                    }
                    break;

                case eState.Chase:
                    {
                        //DebugWide.LogBlue("Chase");
                        int result = Situation_Is_InRange(0, 6f);
                        if (_IN_RANGE != result)
                        {
                            //거리가 멀리 떨어져 있으면 다시 배회한다.
                            _state = eState.Roaming;

                        }
                        else
                        {
                            if (_me._looking.isDeath())
                            {
                                _state = eState.Roaming;
                                break;
                            }
                            //대상이 보이는 위치에 있는지 검사한다 
                            if (false == SingleO.objectManager.IsVisibleArea(_me, _me.GetPos3D()))
                            {
                                //대상이 안보이면 다시 배회하기 
                                _state = eState.Roaming;
                                break;
                            }


                            //공격사거리 안에 들어오면 공격한다 
                            result = Situation_Is_AttackInRange();
                            if (_IN_RANGE == result)
                            {

                                _me._target = _me._looking; //보고 있는 상대를 목표로 설정 
                                _me.Attack(VOp.Minus(_me._looking.GetPos3D(), _me.GetPos3D()));
                                //_state = eState.Attack;
                                break;
                                //DebugWide.LogBlue("attack");
                            }


                            Vector3 moveDir = VOp.Minus(_me._looking.GetPos3D(), _me.GetPos3D());
                            float second = 0.7f;
                            bool foward = true;

                            //상대와 너무 붙어 최소공격사거리 조건에 안맞는 경우 
                            if (_OUT_RANGE_MIN == result)
                            {
                                moveDir *= -1f; //반대방향으로 바꾼다
                                second = 2f;
                                foward = false; //뒷걸음질 
                            }

                            //이동 방향에 동료가 있으면 밀지 않늗다 
                            //Being sameSide = SingleO.cellPartition.RayCast_FirstReturn(_me, moveDir, Camp.eRelation.SameSide, 0.1f);
                            //Being sameSide = _me._cur_cell.MatchRelation(Camp.eRelation.SameSide, _me);
                            //if (null != (object)sameSide && true == foward)
                            //////if(1 < _me._cur_cell._childCount)
                            //{

                            //    DebugWide.LogBlue(_me.name + "  ->  " + sameSide.name); //chamto test
                            //    break;
                            //}


                            _me.Move_Forward(moveDir, second, foward);
                        }

                    }
                    break;
                case eState.Attack:
                    {

                        //못이길것 같으면 도망간다.
                        {
                            //_state = eState.Escape;
                        }

                        //적을 잡았으면 다시 배회한다.
                        {
                            //_state = eState.Roaming;
                        }

                    }
                    break;
                case eState.Escape:
                    {
                        //일정 거리 안에 적이 있으면 탐지한다.
                        {
                            _state = eState.Detect;
                        }

                        //다시 배회한다.
                        {
                            _state = eState.Roaming;
                        }
                    }
                    break;
                case eState.Roaming:
                    {
                        //일정 거리 안에 적이 있으면 추격한다
                        _me._looking = SingleO.objectManager.GetNearCharacter(_me, Camp.eRelation.Enemy, 0, 5f);
                        _me._target = null; //공격할 대상 없음 
                        //DebugWide.LogBlue("Roaming: " + _target);


                        if (null != (object)_me._looking)
                        {
                            //죽은 객체면 대상을 해제한다 , //안보이는 위치면 대상을 해제한다
                            if (_me._looking.isDeath()
                                || false == SingleO.objectManager.IsVisibleArea(_me, _me._looking.GetPos3D()))
                            {
                                _me._looking = null;
                            }

                        }


                        if (true == Situation_Is_Enemy())
                        {
                            _state = eState.Chase;
                            //DebugWide.LogBlue("Chase");
                            break;
                        }

                        //1초마다 방향을 바꾼다
                        if (1f <= _elapsedTime)
                        {
                            _ai_Dir = Misc.GetDir8_Random_AxisY();
                            _elapsedTime = 0f;
                        }

                        _me.Move_Forward(_ai_Dir, 3f, true);

                    }
                    break;
            }//end switch

            _elapsedTime += Time.deltaTime;

        }//end func

    }



}//end namespace

