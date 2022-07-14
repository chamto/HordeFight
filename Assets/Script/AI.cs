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

            float sqrDis = (_me.GetPos3D() -  _me._looking.GetPos3D()).sqrMagnitude;

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

            float sqrDis = (_me.GetPos3D() - _me._looking.GetPos3D()).sqrMagnitude;

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


        public int __findNum = 0;
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
                        int result = Situation_Is_InRange(0, 15f);
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
                            Vector3 lookDir = _me._looking.GetPos3D() - _me.GetPos3D();
                            if (_IN_RANGE == result)
                            {

                                _me._target = _me._looking; //보고 있는 상대를 목표로 설정 
                                _me.Attack(lookDir, null);
                                _me.Throw(_me._target.GetPos3D());
                                //_me._skill_attack.Play(lookDir, null); //테스트 
                                //_state = eState.Attack;
                                break;
                                //DebugWide.LogBlue("attack");
                            }


                            float second = 0.7f;

                            //상대와 너무 붙어 최소공격사거리 조건에 안맞는 경우 
                            if (_OUT_RANGE_MIN == result)
                            {
                                //뒷걸음질 
                                second = 3f;
                                _me.Move_LookAt(-lookDir, lookDir, second);
                            }else
                            {
                                
                                lookDir = VOp.Normalize(lookDir);
                                float[] ay_angle = new float[] {0,45f,-45f,90f,-90,135f,-135,180f};
                                Vector3 findDir = Quaternion.AngleAxis(ay_angle[__findNum], ConstV.v3_up) * lookDir;

                                Vector3 pos_1 = _me.GetPos3D() + lookDir * _me._collider_radius;
                                Vector3 pos_2 = _me.GetPos3D() + findDir * _me._collider_radius;
                                float SENSOR_RADIUS = _me._collider_radius; //센서의 크기는 자신의 충돌원크기가 되어야 한다. 더 작다면 이동 시도가 되어 동료를 밀게 된다 
                                //임시로 두번 호출한다 , 한번호출에 두결과값을 얻을 수 있게 수정하기 
                                Being find_1 = SingleO.objectManager.RangeTest(_me, Camp.eRelation.SameSide, pos_1, 0, SENSOR_RADIUS);
                                Being find_2 = SingleO.objectManager.RangeTest(_me, Camp.eRelation.SameSide, pos_2, 0, SENSOR_RADIUS);
                                if(null == find_1)
                                {
                                    _me.Move_Forward(lookDir, second);
                                    //__findNum = 0; //이동했으면 센서를 대상방향으로 다시 설정 
                                }else if (null == find_2)
                                {
                                    _me.Move_Forward(findDir, second);

                                }else
                                {
                                    //4가지 설정된 방향을 무작위로 설정 
                                    __findNum = Misc.RandInt(1, 4);
                                    _me.Idle_LookAt(lookDir); //이동하지 않고 대상을 바라보게 한다  
                                }
                                    

                                //셀로 검사하는 것은 자연스럽게 안보인다 
                                //Being sameSide = SingleO.cellPartition.RayCast_FirstReturn(_me, _me._looking.GetPos3D(), Camp.eRelation.SameSide, 0.1f);
                                //if (null == sameSide)
                                    //_me.Move_Forward(lookDir, second);
                            }

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
                        _me._looking = SingleO.objectManager.GetNearCharacter(_me, Camp.eRelation.Enemy, 0, 15f);
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

                        //1~2초마다 방향을 바꾼다
                        //if (1f <= _elapsedTime)
                        if (Misc.RandFloat(1f, 2f) <= _elapsedTime)
                        {
                            _ai_Dir = Misc.GetDir8_Random_AxisY();
                            _elapsedTime = 0f;
                        }

                        _me.Move_Forward(_ai_Dir, 3f);

                    }
                    break;
            }//end switch

            _elapsedTime += Time.deltaTime;

        }//end func

    }



}//end namespace

