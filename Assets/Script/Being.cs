using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;
using Utility;




//========================================================
//==================   User Inteface   ==================
//========================================================
namespace HordeFight
{
    public class UI_Main : MonoBehaviour
    {
        public Text _fpsText = null;
        public Image _img_leader = null;


		private void Start()
		{
            _fpsText = GameObject.Find("FPS").GetComponentInChildren<Text>();
            _img_leader = GameObject.Find("leader").GetComponent<Image>();

		}

        float __deltaTime = 0.0f;
        float __msec, __fps;
        //private void Update()
        void FixedUpdate()
        {
            __deltaTime += (Time.unscaledDeltaTime - __deltaTime) * 0.1f;

            __msec = __deltaTime * 1000.0f;
            __fps = 1.0f / __deltaTime;
            _fpsText.text = string.Format("{0:0.0} ms ({1:0.} fps)", __msec, __fps);
        }

        public void SelectLeader(string name)
        {
            Sprite spr = null;
            SingleO.resourceManager._sprIcons.TryGetValue(name.GetHashCode(), out spr);
            if(null == spr)
            {
                name = "None";
                spr = SingleO.resourceManager._sprIcons[name.GetHashCode()];
            }
            _img_leader.sprite = spr;

            //========================================

            foreach (RectTransform rt in _img_leader.GetComponentsInChildren<RectTransform>(true))
            {
                if ("back" == rt.name)
                {
                    rt.GetComponent<Image>().sprite = spr;
                }
                else if ("Text" == rt.name)
                {
                    rt.GetComponent<Text>().text = name;
                }

            }
        }

	}
}


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
        public const float ONE_METER = 0.16f; //타일 한개의 가로길이 
        public const float SQR_ONE_METER = ONE_METER * ONE_METER;
        public const float WorldToMeter = 1f / ONE_METER;
        public const float MeterToWorld = ONE_METER;

        public eDirection8 _eDir8 = eDirection8.down;
        private float _speed_meterPerSecond = 1f;

        private bool _isNextMoving = false;
        private float _elapsedTime = 0f;
        private float _prevInterpolationTime = 0;
        public Vector3 _direction = Vector3.zero;
        private Vector3 _startPos = Vector3.zero; 
        private Vector3 _lastTargetPos = Vector3.zero;
        private Vector3 _nextTargetPos = Vector3.zero;
        private Queue<Vector3> _targetPath = new Queue<Vector3>();


        private void Start()
        {
        }

        //private void Update()
        void FixedUpdate()
        {
            UpdateNextPath();
        }


        public void UpdateNextPath()
        {
            if (false == _isNextMoving) return;

            if ((_nextTargetPos - this.transform.position).sqrMagnitude <= SQR_ONE_METER)
            {
                if (0 == _targetPath.Count)
                {
                    //이동종료 
                    _isNextMoving = false;
                    return;
                }

                _nextTargetPos = _targetPath.Dequeue();
                _nextTargetPos.y = 0f; //목표위치의 y축 값이 있으면, 위치값이 잘못계산됨 

                _direction = Quaternion.FromToRotation(_direction, _nextTargetPos - this.transform.position) * _direction;
                _eDir8 = Misc.TransDirection8_AxisY(_direction);

            }



            //1초후 초기화 
            if(_speed_meterPerSecond < _elapsedTime)
            {
                _elapsedTime = 0;
                _prevInterpolationTime = 0;
            }
            _elapsedTime += Time.deltaTime;
             
            Move_Interpolation(_direction, 2f, _speed_meterPerSecond); //2 미터를 1초에 간다

            //this.transform.position = Vector3.Lerp(this.transform.position, _targetPos, _elapsedTime / (_onePath_movingTime * _speed));
        }

        public void SetNextMoving(bool isNext)
        {
            _isNextMoving = isNext;
        }

        public void MoveToTarget(Vector3 targetPos, float speed_meterPerSecond)
        {
            _targetPath.Clear(); //기존 설정 경로를 비운다

            _isNextMoving = true;
            _startPos = this.transform.position;
            _lastTargetPos = targetPos;
            _nextTargetPos = this.transform.position; //현재위치를 넣어 바로 다음 경로를 꺼내게 한다 
            _speed_meterPerSecond = 1f / speed_meterPerSecond; //역수를 넣어준다. 숫자가 커질수록 빠르게 설정하기 위함이다 
            _direction = Misc.GetDir8Normal_AxisY(_eDir8);

            //연속이동요청시에 이동처리를 할수 있게 주석처리함
            //_elapsedTime = 0; 
            //_prevInterpolationTime = 0;

            //_target_path =  //todo : 목표까지의 길찾기 경로 넣기
            _targetPath.Enqueue(_lastTargetPos);
        }


        private void Move_Interpolation(Vector3 dir, float meter, float perSecond)
        {
            //float interpolationTime = Interpolation.easeInBack(0f, 1f, (_elapsedTime) / perSecond); //슬라임
            //float interpolationTime = Interpolation.easeInOutSine(0f, 1f, (_elapsedTime) / perSecond); //말탄애들 
            //float interpolationTime = Interpolation.punch( 1f, (_elapsedTime) / perSecond);

            float interpolationTime = Interpolation.linear(0f, 1f, (_elapsedTime) / perSecond);


            //보간이 들어갔을때 : Tile.deltaTime 와 같은 간격을 구하기 위해, 현재보간시간에서 전보간시간을 빼준다  
            this.transform.Translate(dir * (ONE_METER * meter) * (interpolationTime -_prevInterpolationTime));

            //보간 없는 기본형
            //this.transform.Translate(dir * (ONE_METER * meter) * (Time.deltaTime * perSecond));

            _prevInterpolationTime = interpolationTime;
        }

        public void Move_Forward(Vector3 dir, float meter, float perSecond)
        {
            _isNextMoving = false;
            _eDir8 = Misc.TransDirection8_AxisY(dir);
            //DebugWide.LogBlue(_eDir8 + "  " + dir); //chamto test

            perSecond = 1f / perSecond;
            //보간 없는 기본형
            this.transform.Translate(dir * (ONE_METER * meter) * (Time.deltaTime * perSecond));
        }

        //public void Move_Forward2(Vector3 dir, float distance, float speed)
        //{
        //    Vector3[] lineTest = SingleO.objectManager.LineSegmentTest(transform.position, dir);
        //    foreach (Vector3 centerPos in lineTest)
        //    {
        //        if (true == SingleO.gridManager.HasStructTile(centerPos))
        //        {
        //            return;
        //        }

        //        break; //첫번째 것만 검사한다 
        //    }
        //    Move_Forward(dir, distance, speed);
        //}
    }
    //========================================================



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
        //private Vector3 _direction = Vector3.forward;


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
        public Movement _move = null;
        public CellInfo _cellInfo = null;
        public Vector3 _lastCellPos_withoutCollision = Vector3Int.zero; //충돌하지 않은 마지막 타일의 월드위치값
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

            //셀정보 초기 위치값에 맞춰 초기화
            Vector3Int cellIdx = SingleO.gridManager.ToCellIndex(transform.position, Vector3.up);
            SingleO.gridManager.AddCellInfo_Being(cellIdx, this);
            _cellInfo = SingleO.gridManager.GetCellInfo(cellIdx);
		}


    

		public float GetCollider_Radius()
        {
            Assert.IsTrue(null != _collider, this.name + " 충돌체가 Null이다");
                  
            //if (null == _collider)
                //DebugWide.LogRed(this.name);

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


        /// <summary>
        /// 그리드상 셀값이 변경되면 셀정보값을 갱신한다 
        /// </summary>
        public void Update_CellInfo()
        {
            Vector3Int curIdx = SingleO.gridManager.ToCellIndex(transform.position, Vector3.up);

            //충돌없는 마지막 타일 위치를 갱신한다 
            Vector3 centerPos = Vector3.zero;
            if(false == SingleO.gridManager.HasStructTile(transform.position, out centerPos))
            {
                _lastCellPos_withoutCollision = centerPos;//SingleO.gridManager.ToPosition_Center(curIdx,Vector3.up);
                //DebugWide.LogBlue(curIdx + "   " + this.transform.position);
            }
                

            UnityEngine.Assertions.Assert.IsTrue(null != _cellInfo, "CellInfo 가 Null 이다");
            if(_cellInfo._index != curIdx)
            {
                SingleO.gridManager.RemoveCellInfo_Being(_cellInfo._index, this);
                SingleO.gridManager.AddCellInfo_Being(curIdx, this);

                _cellInfo = SingleO.gridManager.GetCellInfo(curIdx);

                //chamto test
                //string temp = "count:"+_cellInfo.Count + "  (" + curIdx + ")  ";
                //foreach(Being b in _cellInfo)
                //{
                //    temp += " " + b.name;
                //}
                //DebugWide.LogBlue(temp); 
            }
        }


        //한 프레임에서 start 다음에 running 이 바로 시작되게 한다. 상태 타이밍 이벤트는 콜벡함수로 처리한다 
        //private void Update()
        void FixedUpdate()
        {

            //if (true == _death) return;
            Update_CellInfo();

            Update_Shot();

            UpdateNextPath(); //chamto test

            if (Behavior.eKind.Idle == _behaviorKind)
            {
                _animator.SetInteger("state", (int)Behavior.eKind.Idle);
            }
            else if (Behavior.eKind.Idle_Random == _behaviorKind)
            {
                _animator.SetInteger("state", (int)Behavior.eKind.Idle);
                Idle_Random();

            }
            else if (Behavior.eKind.Move == _behaviorKind)
            {
                Switch_Ani("base_move", _kind.ToString() + "_move_", _move._eDir8);
                _animator.SetInteger("state", (int)Behavior.eKind.Move);
            }
            else if (Behavior.eKind.Attack == _behaviorKind)
            {
                _animator.SetInteger("state", (int)Behavior.eKind.Attack);
            }
            else if (Behavior.eKind.FallDown == _behaviorKind)
            {
                _animator.SetInteger("state", (int)Behavior.eKind.FallDown);
            }


            //  1/0.16 = 6.25 : 곱해지는 값이 최소 6.25보다는 커야 한다
            //y축값이 작을수록 먼저 그려지게 한다. 캐릭터간의 실수값이 너무 작아서 20배 한후 소수점을 버린값을 사용함
            _sprRender.sortingOrder = -(int)(transform.position.z * 20f);

            //========================================

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
        //                  충돌반응
        //____________________________________________

        //존재간에 부딪힌 경우
        public void OnCollision_Beings(Being[] dsts)
        {


        }

        //때렸을때
        public void OnCollision_WhenHit(Being[] dsts)
        {
        }

        //맞았을때
        public void OnCollision_WhenBeHit(Being[] dsts)
        {
        }

        

        //____________________________________________
        //                  애니메이션  
        //____________________________________________

        //todo optimization : 애니메이션 찾는 방식 최적화 필요. 해쉬로 변경하기 
        public AnimationClip GetClip(int nameToHash)
        {
            AnimationClip animationClip = null;
            SingleO.resourceManager._aniClips.TryGetValue(nameToHash, out animationClip);

            //DebugWide.LogRed(animationClip + "   " + Single.resourceManager._aniClips.Count); //chamto test


            return animationClip;
        }


        public void Switch_Ani(string aniKind, string aniName, eDirection8 dir)
        {

            _sprRender.flipX = false;
            string aniNameSum = "";
            switch (dir)
            {
                //case eDirection8.none:
                    //{
                    //    DebugWide.LogRed("Switch_Ani : "+dir  + "값은 처리 할 수 없다 ");
                    //}
                    //break;
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

            //DebugWide.LogBlue(aniNameSum + "  " + dir); //chamto test


            _overCtr[aniKind] = GetClip(aniNameSum.GetHashCode());
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
                    if (1 > num) num = 8;
                    if (8 < num) num = 1;
                    _move._eDir8 = (eDirection8)num;

                    Switch_Ani("base_idle", _kind.ToString() + "_idle_", _move._eDir8);

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
            Switch_Ani("base_idle", _kind.ToString() + "_idle_", _move._eDir8);

            //if(true == setState)
            //_animator.SetInteger("state", (int)eState.Idle);
        }

        public void Move(Vector3 dir, float second, bool forward)//, bool setState)
        {
            //Vector3 dir = target - this.transform.position;
            //dir.Normalize();
            _move.Move_Forward(dir, 2f, second);

            //전진이 아니라면 애니를 반대방향으로 바꾼다 (뒷걸음질 효과)
            if (false == forward)
                dir *= -1f;

            //_move._eDir8 = Misc.TransDirection8_AxisY(dir);
            Switch_Ani("base_move", _kind.ToString() + "_move_", _move._eDir8);

        }

        public void Attack(Vector3 dir)
        {
            _move._eDir8 = Misc.TransDirection8_AxisY(dir);
            Switch_Ani("base_attack", _kind.ToString() + "_attack_", _move._eDir8);
        }


        bool __launch = false;
        GameObject __things = null;
        Vector3 __targetPos = Vector3.zero;
        Vector3 __launchPos = Vector3.zero;
        public void ThrowThings(Vector3 target)
        {
            //Vector3 dir = target - this.transform.position;
            //Attack(dir);


            if (null == __things)
            {
                __things = GameObject.Find("000_spear");
            }


            if (null != __things && false == __launch)
            {
                __targetPos = target;

                Vector3 angle = new Vector3(90f, 0, -120f);
                angle.y += (float)_move._eDir8 * -45f;
                __things.transform.localEulerAngles = angle;
                __things.transform.localPosition = Vector3.zero;
                __launch = true;
                __launchPos = __things.transform.position;
                __elapsedTime_2 = 0f;
                __things.SetActive(true);

            }
        }
        float __elapsedTime_2 = 0f;
        public void Update_Shot()
        {
            if (null != __things && true == __launch)
            {
                __elapsedTime_2 += Time.deltaTime;
                //Vector3 dir = __targetPos - __things.transform.position;
                __things.transform.position = Vector3.Lerp(__launchPos, __targetPos, __elapsedTime_2);

                if (1f < __elapsedTime_2)
                {
                    __launch = false;
                    __things.SetActive(false);
                }
            }
        }


        public void FallDown()
        {
            switch (_move._eDir8)
            {
                case eDirection8.up:
                    { }
                    break;
                case eDirection8.down:
                    { }
                    break;
                default:
                    {
                        _move._eDir8 = eDirection8.up; //기본상태 지정 
                    }
                    break;
            }

            Switch_Ani("base_fallDown",  _kind.ToString() + "_fallDown_", _move._eDir8);
        }


        //____________________________________________
        //               지정된 경로 이동   
        //____________________________________________

        Queue<Vector3> __path_find = null;
        Vector3 __dstPos = Vector3.zero;
        float __elapsedTime_3 = 10f;
        public void UpdateNextPath()
        {
            if (null == __path_find || 0 == __path_find.Count) return;


            if (1f < __elapsedTime_3)
            {
                __elapsedTime_3 = 0f;
                __dstPos = __path_find.Dequeue();
            }else
            {
                __elapsedTime_3 += Time.deltaTime;
                this.transform.position = Vector3.Lerp(this.transform.position, __dstPos, __elapsedTime_3);    
            }
        }


        //____________________________________________
        //                  터치 이벤트   
        //____________________________________________

        private Vector3 __startPos = Vector3.zero;
        private LineSegment3 __lineSeg = LineSegment3.zero;
        private void TouchBegan()
        {
            RaycastHit hit = SingleO.touchProcess.GetHit3D();
            __startPos = hit.point;
            __startPos.y = 0f;


            if (8 > _hp_cur)
            {
                //다시 살리기
                _animator.Play("idle 10");
                _death = false;
                _hp_cur = 10;
                _behaviorKind = Behavior.eKind.Idle;
            }

            SingleO.uiMain.SelectLeader(_kind.ToString());

            //chamto test
            //CellInfo.Index cidx = Single.gridManager.ToCellIndex(hit.point, Vector3.up);
            //Vector3 cidxToV3 = Single.gridManager.ToPosition(cidx, Vector3.up);
            //DebugWide.LogBlue(hit.point +"  "+cidx + "  " + cidxToV3); 
            //this.transform.position = cidxToV3;

            //CellInfo cinfo = Single.gridManager.GetCellInfo_NxN(_cellInfo._index, 3);
            //string temp = "count:"+cinfo.Count + "  (" + cinfo._index + ")  ";
            //foreach(Being b in cinfo)
            //{
            //    temp += " " + b.name;
            //}
            //DebugWide.LogBlue(temp);


            //int count = 1;
            //string temp = "";
            //foreach (CellInfo.Index b in Single.gridManager._indexesNxN[3])
            //{
            //    temp += " " + b;
            //    if (0 == count % 3) temp += "\n";
            //    count++;
            //}
            //DebugWide.LogBlue(temp);

            //int allCount = 0;
            //string temp = "";
            //CellInfo cellInfo = null;
            //foreach (CellInfo.Index ix in Single.gridManager._indexesNxN[3])
            //{

            //    cellInfo = Single.gridManager.GetCellInfo(ix + this._cellInfo._index);
            //    if (null == cellInfo) continue;

            //    temp += "   [" + "  cnt:" + cellInfo.Count + " "+(ix + this._cellInfo._index);

            //    foreach (Being dst in cellInfo)
            //    {
            //        temp += ", " + dst.name;
            //        allCount++;
            //    }
            //    temp += "] ";
            //}
            //DebugWide.LogBlue("allCnt:"+allCount + "  " +temp);

            //길찾기 시험 
            //__path_find =  SingleO.pathFinder.Search(this.transform.position, Vector3.zero); //chamto test



            //__path_find = new Queue<Vector3>();
            //foreach (Vector3Int v in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(5, Vector3.up))
            //{
            //    Vector3 vv = v;
            //    __path_find.Enqueue( vv * 0.16f);

            //}
            //__path_find.Enqueue(Vector3.zero);


        }


        private void TouchMoved()
        {
            //DebugWide.LogBlue("TouchMoved " + Single.touchProcess.GetTouchPos());

            RaycastHit hit = SingleO.touchProcess.GetHit3D();

            Vector3 dir = hit.point - this.transform.position;
            dir.y = 0;
            //DebugWide.LogBlue("TouchMoved " + dir);

            if (eKind.spearman == _kind)
            {
                Being target = SingleO.objectManager.GetNearCharacter(this, 0.5f, 2f);

                if(null != target)
                {
                    ThrowThings(target.transform.position);   
                    Vector3 things_dir = target.transform.position - this.transform.position;

                    _behaviorKind = Behavior.eKind.Attack;
                    Attack(things_dir);
                }




                _move.Move_Forward(dir, 2f, 1f); //chamto test
                //DebugWide.LogRed(target.name); //chamto test
            }
            else
            {

                //Vector3[] lineTest = SingleO.objectManager.LineSegmentTest(transform.position, dir);
                //foreach (Vector3 centerPos in lineTest)
                //{
                //    if(true == SingleO.gridManager.HasStructTile(centerPos))
                //        return;

                //    break; //첫번째 것만 검사한다 
                //}

                //dir.Normalize();
                //float dis = 0.16f * 1f;

                Being target = SingleO.objectManager.GetNearCharacter(this, 0, 0.2f);
                if (null != target)
                {
                    _behaviorKind = Behavior.eKind.Attack;
                    Attack(dir);

                    _move.Move_Forward(dir, 2f, 1f); //chamto test
                }
                else
                {
                    _behaviorKind = Behavior.eKind.Move;
                    Move(dir, 0.4f, true);
                    //_move.MoveToTarget(hit.point, 1f);

                    //DebugWide.LogBlue(dir + "  " + Misc.TransDirection8_AxisY(dir));
                }
            }

            SingleO.objectManager.LookAtTarget(this, GridManager.NxN_MIN);

        }

        private void TouchEnded()
        {
            //DebugWide.LogBlue("TouchEnded " + Single.touchProcess.GetTouchPos());
            //_move.MoveToTarget(transform.position, 1f); //이동종료
            _move.SetNextMoving(false);

            Switch_Ani("base_idle", _kind.ToString() + "_idle_", _move._eDir8);
            //_animator.SetInteger("state", (int)eState.Idle);
            _animator.Play("idle 10");

            _behaviorKind = Behavior.eKind.Idle_Random;
            SingleO.objectManager.SetAll_Behavior(Behavior.eKind.Idle_Random);

         
            //==========

            //SingleO.debugViewer.ResetColor(); //타일 색정보 초기화  
            //RaycastHit hit = SingleO.touchProcess.GetHit3D();
            //Vector3 last = hit.point; last.y = 0;
            //Color color = Color.black;
            //foreach(Vector3 v3 in SingleO.objectManager.LineSegmentTest(__startPos, last))
            //{
            //    Vector3Int iv = SingleO.gridManager.grid.WorldToCell(v3);
            //    float sqrL = (v3 - __startPos).sqrMagnitude;

            //    color = Color.blue;
            //    color.a = 0.2f;
            //    SingleO.debugViewer.SetColor(v3, color);
            //    //DebugWide.LogBlue(v3);

            //    //DebugWide.LogBlue( iv + "  " + sqrL);
            //}
            ////DebugWide.LogBlue("++++++++++++++++++++++++++++++");
            //SingleO.debugViewer.DrawLine(__startPos, last);

        }

    }

}


namespace HordeFight
{

    //========================================================
    //==================     동작  정보     ==================
    //========================================================
    public partial class Behavior
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

        //공격의 모양
        public enum eTraceShape
        {
            None,
            Horizon,  //수평
            Vertical, //수직
            Straight, //직선
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

            attack_shape = eTraceShape.None;
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

        public bool Valid_EventTime(Being.ePhase phase, float timeDelta)
        {

            if (Being.ePhase.Start == phase || Being.ePhase.Running == phase)
            {
                if (this.eventTime_0 <= timeDelta && timeDelta <= this.eventTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_CloggedTime(Being.ePhase phase, float timeDelta)
        {
            if (Being.ePhase.Start == phase || Being.ePhase.Running == phase)
            {
                if (this.cloggedTime_0 <= timeDelta && timeDelta <= this.cloggedTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_OpenTime(Being.ePhase phase, float timeDelta )
        {
            if (Being.ePhase.Running == phase)
            {
                if (this.openTime_0 <= timeDelta && timeDelta <= this.openTime_1)
                    return true;
            }

            return false;
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


    //========================================================
    //==================     스킬  정보     ==================
    //========================================================


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