using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UtilGS9;




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
            GameObject o = null;
            o = GameObject.Find("FPS");
            if (null != o)
            {
                _fpsText = o.GetComponentInChildren<Text>();
            }

            o = GameObject.Find("leader");
            if (null != o)
            {
                _img_leader = o.GetComponentInChildren<Image>();
            }



        }

        //StringBuilder __sb = new StringBuilder(32);
        float __deltaTime = 0.0f;
        float __msec, __fps;
        private void Update()
        {
            __deltaTime += (Time.unscaledDeltaTime - __deltaTime) * 0.1f;

            __msec = __deltaTime * 1000.0f;
            __fps = 1.0f / __deltaTime;
            _fpsText.text = string.Format("{1:0.} fps ({0:0.0} ms)", __fps, __msec);


            //StringBuilder 를 사용해도 GC가 발생함. 
            //__sb.Length = 0; //clear
            //_fpsText.text = __sb.AppendFormat("{0:0.0} ms ({1:0.} fps)", __msec, __fps).ToString();
        }

        public void SelectLeader(string name)
        {
            if (null == _img_leader) return;

            Sprite spr = null;
            SingleO.resourceManager._sprIcons.TryGetValue(name.GetHashCode(), out spr);
            if (null == spr)
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

    //전술원
    public class TacticsSphere
    {
        //public uint _leaderId = 0; //연결된 리더ID
        //public Vector3 _local_initPosition = ConstV.v3_zero; //리더중심으로 부터의 초기 위치 
        //public Vector3 _local_calcPosition = ConstV.v3_zero; //리더중심으로 부터의 계산된 위치 (초기위치가 이동 할 수 없는 위치에 있는 경우, 이동 할 수 있는 위치로 계산한다)

        //public Vector3 _position = ConstV.v3_zero; //전술원의 월드 위치
        public float _min_radius = 0;
        public float _max_radius = 0;
        public Geo.Sphere _sphere = Geo.Sphere.Zero;

    }

    //뛰어난 동물 진영
    public class Camp
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

        //진영
        public enum eKind
        {
            None = 0,

            Hero,
            Blue,
            Red,
            White,
            Black,

            Obstacle, //구조물

            Max,
        }

        //진영간 관계
        public enum eRelation
        {
            Unknown = 0,    //알수없음
            SameSide,       //같은편
            Neutrality,     //중립
            Alliance,       //동맹
            Enemy,          //적대


        }

        //캠프 배치 정보
        public class Placement
        {
            public Being _champUnit = null;
            public Vector3 _localPos = ConstV.v3_zero; //캠프 위치로부터의 상대적 위치

            public Placement(Vector3 localPos)
            {
                _localPos = localPos;
            }

        }

        //====================================

        public int _campHashName = 0;
        public eKind _eCampKind = eKind.None;

        //====================================

        public uint _leaderId = 0;
        public Vector3 _campPos = ConstV.v3_zero; //캠프의 위치
        public TacticsSphere _tacticsSphere = new TacticsSphere(); //캠프 전술원 크기
        public List<Placement> _placements = new List<Placement>(); //배치 위치-객체 정보 

        private Camp() { }

        public Camp(int campHashName, eKind kind)
        {
            _campHashName = campHashName;
            _eCampKind = kind;
        }

        //public eKind campKind
        //{
        //    get { return _eCampKind; }
        //}

        public Vector3 GetPosition(int posNum)
        {
            if (posNum >= _placements.Count || 0 > posNum) return _campPos;

            return _placements[posNum]._localPos + _campPos;
        }

        public Vector3 RandPosition()
        {
            int rnd = Misc.rand.Next(0, _placements.Count);
            return GetPosition(rnd);
        }
    }

    //캠프(분대)를 소대로 묶음 : 키값 : 문자열 해쉬
    public class CampPlatoon : Dictionary<int, Camp>
    {
        public Camp GetCamp(int hashName)
        {
            if (true == this.ContainsKey(hashName))
            {
                return this[hashName];
            }

            return null;
        }
    }

    //캠프와 캠프간의 관계를 저장 
    public class CampRelation : List<Camp.eRelation>
    {

        //public CampRelation() : base(new EnumCampKindComparer()) {}
        public void SetRelation(Camp.eKind campKey, Camp.eRelation relat)
        {
            this[(int)campKey] = relat;

            //if (false == this.ContainsKey(camp))
            //{
            //    //없으면 추가 한다
            //    this.Add(camp, relat);
            //}
            //this[camp] = relat;

        }

        public Camp.eRelation GetRelation(Camp.eKind campKey)
        {
            return this[(int)campKey];

            //if (true == this.ContainsKey(campKey))
            //{
            //    return this[campKey];
            //}
            //return Camp.eRelation.Unknown;
        }
    }


    //public class EnumCampKindComparer : IEqualityComparer<Camp.eKind>
    //{
    //    public bool Equals(Camp.eKind a, Camp.eKind b)
    //    {
    //        if (a == b)
    //            return true;
    //        return false;
    //    }

    //    public int GetHashCode(Camp.eKind a)
    //    {
    //        return (int)a;
    //    }
    //}

    public class CampManager
    {
        //private Dictionary<Camp.eKind, CampRelation> _relations = new Dictionary<Camp.eKind, CampRelation>(new EnumCampKindComparer());
        //private Dictionary<Camp.eKind, CampPlatoon> _campDivision = new Dictionary<Camp.eKind, CampPlatoon>(new EnumCampKindComparer()); //전체소대를 포함하는 사단
        private List<CampRelation> _relations = new List<CampRelation>(); //제거대상

        private List<CampPlatoon> _campDivision = new List<CampPlatoon>(); //전체소대를 포함하는 사단
        private Camp.eRelation[][] _relations2 = new Camp.eRelation[(int)Camp.eKind.Max][];
        //캠프의 초기 배치정보 

        public CampManager()
        {
            Create_DefaultCamp();
        }

        public void Create_DefaultCamp()
        {
            foreach (Camp.eKind kind in Enum.GetValues(typeof(Camp.eKind)))
            {
                if (Camp.eKind.Max == kind) continue;


                CampPlatoon platoon = new CampPlatoon();
                _campDivision.Add(platoon);

                CampRelation campRelation = new CampRelation();
                _relations.Add(campRelation);
                //==========================================
                //각 분대별 관계를 미리 넣어놓는다  
                foreach (Camp.eKind kind2 in Enum.GetValues(typeof(Camp.eKind)))
                {
                    if (Camp.eKind.Max == kind2) continue;


                    campRelation.Add(Camp.eRelation.Unknown);

                    //배열형 관계정보 설정 
                    _relations2[(int)kind2] = new Camp.eRelation[(int)Camp.eKind.Max];
                }


                //==========================================
                _relations2[(int)kind][(int)kind] = Camp.eRelation.SameSide; //같은편 설정한다 

                Camp camp = new Camp((int)kind, kind); //열거형 값을 키로 사용한다 
                platoon.Add((int)kind, camp);
            }


        }


        //계층도에서 읽어들인다 
        public void Load_CampPlacement(Camp.eKind kind)
        {
            string campRoot = "0_main/0_placement/Camp/";
            string campPath = campRoot + kind.ToString();

            Transform campKind = SingleO.hierarchy.GetTransformA(campPath);

            Camp camp = null;
            foreach (Transform TcampName in campKind.GetComponentsInChildren<Transform>())
            {
                if (TcampName.parent == campKind)
                {
                    //DebugWide.LogBlue(TcampName.name);

                    //캠프 추가 
                    camp = CreateCamp(kind, TcampName.name.GetHashCode());
                    //camp._campHashName = TcampName.name.GetHashCode();
                    camp._campPos = TcampName.position;
                    //맴버위치 추가 
                    foreach (Transform Tmember in TcampName.GetComponentsInChildren<Transform>())
                    {
                        if (Tmember.parent == TcampName)
                        {
                            //DebugWide.LogBlue(Tmember.name);

                            //계층도 이름과 상관없이 단순히 게임오브젝트 순서대로 위치값을 얻는다 (순서가 중요)
                            camp._placements.Add(new Camp.Placement(Tmember.localPosition));
                        }
                    }
                }

            }

        }


        private CampRelation GetCampRelation(Camp.eKind camp)
        {
            //CampRelation camp_relat = null;
            //if (false == _relations.TryGetValue(camp, out camp_relat))
            //{
            //    //없으면 추가 한다
            //    camp_relat = new CampRelation();
            //    _relations.Add(camp, camp_relat);
            //}

            return _relations[(int)camp];
        }

        public void SetRelation(Camp.eRelation eRelation, Camp.eKind camp_1, Camp.eKind camp_2)
        {
            //같은 캠프에 관계를 설정 할 수 없다
            if (camp_1 == camp_2) return;

            GetCampRelation(camp_1).SetRelation(camp_2, eRelation);
            GetCampRelation(camp_2).SetRelation(camp_1, eRelation);

            _relations2[(int)camp_1][(int)camp_2] = eRelation;
            _relations2[(int)camp_2][(int)camp_1] = eRelation;
        }

        public Camp.eRelation GetRelation(Camp.eKind camp_1, Camp.eKind camp_2)
        {
            //DebugWide.LogBlue(camp_1.ToString() + "   " + camp_2.ToString()); //chamto test
            //return GetCampRelation(camp_1).GetRelation(camp_2);

            return _relations2[(int)camp_1][(int)camp_2];
        }

        public Camp GetDefaultCamp(Camp.eKind kind)
        {
            //CampPlatoon platoon = null;
            //if (true == _campDivision.TryGetValue(kind, out platoon))
            //{
            //    if (null != platoon)
            //    {
            //        return platoon.GetCamp((int)kind);
            //    }
            //}

            CampPlatoon platoon = _campDivision[(int)kind];
            if (null != platoon)
            {
                return platoon.GetCamp((int)kind);
            }
            return null;
        }

        public Camp GetCamp(Camp.eKind kind, int hashName)
        {
            //CampPlatoon platoon = null;
            //if (true == _campDivision.TryGetValue(kind, out platoon))
            //{
            //    if (null != platoon)
            //    {
            //        return platoon.GetCamp(hashName);
            //    }
            //}

            CampPlatoon platoon = _campDivision[(int)kind];
            if (null != platoon)
            {
                return platoon.GetCamp(hashName);
            }
            return null;
        }

        public CampPlatoon GetPlatoon(Camp.eKind kind)
        {
            //CampPlatoon platoon = null;
            //if (false == _campDivision.TryGetValue(kind, out platoon))
            //{
            //    return null;
            //}

            return _campDivision[(int)kind];
        }


        public Camp CreateCamp(Camp.eKind kind, int hashName)
        {
            //열거형에 있는 캠프분대를 미리 만들어 놓았기 때문에, 분대를 가져오지 못했다면 무언가 잘못된 열거형 값이 들어온 것이다 
            CampPlatoon platoon = GetPlatoon(kind);
            if (null == platoon) return null;

            Camp camp = GetCamp(kind, hashName);
            if (null == camp)
            {
                camp = new Camp(hashName, kind);
                platoon.Add(hashName, camp);
            }

            return camp;
        }

    }
}

namespace HordeFight
{
    //========================================================
    //==================   캐릭터/구조물 정보   ==================
    //========================================================

    public class Movement : MonoBehaviour
    {
        //public const float ONE_METER = 0.16f; //타일 한개의 가로길이 
        //public const float SQR_ONE_METER = ONE_METER * ONE_METER;
        //public const float WorldToMeter = 1f / ONE_METER;
        //public const float MeterToWorld = ONE_METER;

        public Being _being = null;
        public GameObject _gameObject = null;
        public Transform _transform = null;

        public eDirection8 _eDir8 = eDirection8.down;
        //public float _dir_angleY = 0f;
        public Vector3 _direction = Vector3.back; //_eDir8과 같은 방향으로 설정한다  

        private float _speed_meterPerSecond = 1f;

        private bool _isNextMoving = false;
        private float _elapsedTime = 0f;
        private float _prevInterpolationTime = 0;

        private Vector3 _startPos = ConstV.v3_zero;
        private Vector3 _lastTargetPos = ConstV.v3_zero;
        private Vector3 _nextTargetPos = ConstV.v3_zero;
        private Queue<Vector3> _targetPath = null;

        private float _oneMeter_movingTime = 0.8f; //임시처리
        private float _elapsed_movingTime = 0f;

        private void Start()
        {
            _gameObject = gameObject;
            _transform = transform;
            _direction = Misc.GetDir8_Normal3D_AxisY(_eDir8);
        }

        //private void Update()
        //{
        //    //UpdateNextPath();
        //}


        public void UpdateNextPath()
        {
            if (false == _isNextMoving) return;

            if (null == (object)_targetPath || 0 == _targetPath.Count)
            {
                //이동종료 
                _elapsed_movingTime = 0;
                _isNextMoving = false;
                return;
            }

            //1meter 가는데 걸리는 시간을 넘었다면 다시 경로를 찾는다
            if (_oneMeter_movingTime < _elapsed_movingTime)
            {
                _elapsed_movingTime = 0;

                MoveToTarget(_lastTargetPos, _speed_meterPerSecond); //test
            }
            //1meter 의 20% 길이에 해당하는 거리가 남았다면 도착 
            else if ((_nextTargetPos - _being.GetPos3D()).sqrMagnitude <= GridManager.SQR_ONE_METER * 0.2f)
            {
                _elapsed_movingTime = 0;

                _nextTargetPos = _targetPath.Dequeue();
                _nextTargetPos.y = 0f; //목표위치의 y축 값이 있으면, 위치값이 잘못계산됨 

                //회전쿼터니언을 추출해 방향값에 곱한다 => 새로운 방향 
                _direction = Quaternion.FromToRotation(_direction, VOp.Minus(_nextTargetPos, _transform.position)) * _direction;
                _eDir8 = Misc.GetDir8_AxisY(_direction);
                //_dir_angleY = Geo.AngleSigned_AxisY(ConstV.v3_forward, _direction);
            }
            _elapsed_movingTime += Time.deltaTime;



            //1초후 초기화 
            if (_speed_meterPerSecond < _elapsedTime)
            {
                _elapsedTime = 0;
                _prevInterpolationTime = 0;
            }
            _elapsedTime += Time.deltaTime;


            Move_Interpolation(_direction, 2f, _speed_meterPerSecond); //2 미터를 1초에 간다

            //this.transform.position = Vector3.Lerp(this.transform.position, _targetPos, _elapsedTime / (_onePath_movingTime * _speed));

            DebugVeiw_DrawPath_MoveToTarget(); //chamto test
        }

        public void SetNextMoving(bool isNext)
        {
            _isNextMoving = isNext;
        }

        public bool IsMoving()
        {
            return _isNextMoving;
        }

        public void MoveToTarget(Vector3 targetPos, float speed_meterPerSecond)
        {
            _targetPath = null; //기존 설정 경로를 비운다
            targetPos.y = 0;

            _isNextMoving = true;
            _startPos = _being.GetPos3D();//_transform.position;
            _lastTargetPos = targetPos;
            _nextTargetPos = _being.GetPos3D();//_transform.position; //현재위치를 넣어 바로 다음 경로를 꺼내게 한다 
            _speed_meterPerSecond = 1f / speed_meterPerSecond; //역수를 넣어준다. 숫자가 커질수록 빠르게 설정하기 위함이다 

            //연속이동요청시에 이동처리를 할수 있게 주석처리함
            _elapsedTime = 0;
            _prevInterpolationTime = 0;

            _targetPath = SingleO.pathFinder.Search(_being.GetPos3D(), targetPos);

            //초기방향을 구한다
            _eDir8 = Misc.GetDir8_AxisY(_targetPath.First() - _startPos);
            _direction = Misc.GetDir8_Normal3D_AxisY(_eDir8);
        }


        private void Move_Interpolation(Vector3 dir, float meter, float perSecond)
        {
            //float interpolationTime = Interpolation.easeInBack(0f, 1f, (_elapsedTime) / perSecond); //슬라임
            //float interpolationTime = Interpolation.easeInOutSine(0f, 1f, (_elapsedTime) / perSecond); //말탄애들 
            //float interpolationTime = Interpolation.punch( 1f, (_elapsedTime) / perSecond);

            float interpolationTime = Interpolation.linear(0f, 1f, (_elapsedTime) / perSecond);


            //보간이 들어갔을때 : Tile.deltaTime 와 같은 간격을 구하기 위해, 현재보간시간에서 전보간시간을 빼준다  
            //_transform.Translate(dir * (GridManager.ONE_METER * meter) * (interpolationTime - _prevInterpolationTime));
            Vector3 newPos = _being.GetPos3D() + dir * (GridManager.ONE_METER * meter) * (interpolationTime - _prevInterpolationTime);
            _being.SetPos(newPos);

            //보간 없는 기본형
            //this.transform.Translate(dir * (ONE_METER * meter) * (Time.deltaTime * perSecond));

            _prevInterpolationTime = interpolationTime;
        }

        public void Move_Forward(Vector3 dir, float meter, float perSecond)
        {

            //dir.Normalize();
            _isNextMoving = true;
            _direction = Quaternion.FromToRotation(_direction, dir) * _direction;
            _eDir8 = Misc.GetDir8_AxisY(dir);
            //if(eDirection8.max == _eDir8)
            //{
            //    DebugWide.LogBlue(_direction + "    : " + dir + "    : " + _being.name); //chamto test
            //}


            perSecond = 1f / perSecond;
            //보간 없는 기본형
            //this.transform.Translate(_direction * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond));
            Vector3 newPos = _being.GetPos3D() + _direction * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond);
            Vector3 newPosBounds = _being.GetPos3D() + _direction * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond) * _being._collider_radius;
            //================================
            //!!!! 셀에 원의 중점 기준으로만 등록되어 있어서 가져온 셀정보 외에서도 충돌가능원이 있을 가능성이 매우 높다. 즉 제대로 처리 할 수가 없다  
            //새로운 위치로 이동이 가능한지 검사 
            //CellSpace cell = SingleO.cellPartition.GetCellSpace(newPos); //원의 중점인 자기위치의 타일정보만 가져오는 결과가 됨
            CellSpace cell = SingleO.cellPartition.GetCellSpace(newPosBounds); //객체 반지름의 경계면의 타일정보를 가져온다  
            if (null != cell)
            {
                //DebugWide.LogBlue(cell._childCount);
                bool moveable = true;
                Being cur = cell._children;
                for (int i = 0; i < cell._childCount; i++)
                {
                    if ((object)_being == (object)cur) continue;
                    Vector3 between = cur.GetPos3D() - newPos;
                    float sumRadius = cur._collider_radius + _being._collider_radius;
                    if (between.sqrMagnitude <= sumRadius * sumRadius)
                    {
                        moveable = false;
                        break;
                    }


                    cur = cell._children._next_sibling;
                }

                if (true == moveable)//&& 3 > cell._childCount)
                    _being.SetPos(newPos);

            }

            //================================

        }

        public void Move_Push(Vector3 dir, float meter, float perSecond)
        {
            _isNextMoving = true;
            perSecond = 1f / perSecond;
            //보간 없는 기본형
            //this.transform.Translate(dir * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond));
            Vector3 newPos = _being.GetPos3D() + dir * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond);
            _being.SetPos(newPos);
        }

        public void DebugVeiw_DrawPath_MoveToTarget()
        {
            Vector3 prev = _targetPath.FirstOrDefault();

            foreach (Vector3 pos3d in _targetPath)
            {
                DebugWide.DrawLine(prev, pos3d, Color.green);
                prev = pos3d;
            }
        }

    }
    //========================================================



    //========================================================


    //작업하다 말았음 - 나중에 작업 다시 시작하기 
    public class EffectAni
    {
        public enum eEffectKind
        {
            Aim,        //조준점
            Dir,        //방향
            Emotion,    //감정표현
            Hand_Left,  //행동 왼손
            Hand_Right, //행동 오른손

            Max,
        }

        public Being _owner = null; 
        public Being _target = null;
        public bool _on_theWay = false; //발사되어 날아가는 중

        //전용effect
        private Transform[] _effect = new Transform[(int)eEffectKind.Max];


        public void Init()
        {
           
            // 전용 effect 설정 
            _effect[(int)eEffectKind.Aim] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/aim");
            _effect[(int)eEffectKind.Dir] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/dir");
            _effect[(int)eEffectKind.Emotion] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/emotion");
            _effect[(int)eEffectKind.Hand_Left] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/hand_left");
            _effect[(int)eEffectKind.Hand_Right] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/hand_right");

        }


        //public void End()
        //{
        //    if (null != (object)_owner)
        //    {
        //        ChampUnit champ = _owner as ChampUnit;
        //        if (null != (object)champ)
        //        {
        //            champ._shot = null;
        //        }
        //    }
        //    _owner = null;
        //    _on_theWay = false;

        //    //spr 위치를 현재위치로 적용
        //    _transform.position = _sprParent.position;
        //    _sprParent.localPosition = ConstV.v3_zero;

        //    //캐릭터 아래에 깔리도록 설정
        //    base.Update_SortingOrder(-500);
        //    //_sortingGroup.sortingOrder = base.GetSortingOrder(0);

        //}

        //____________________________________________
        //                  충돌반응
        //____________________________________________

        //public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        //{
        //    //무언가와 충돌했으면 투사체를 해제한다 (챔프 , 숥통 등)

        //    if ((object)dst != (object)_owner)
        //        End();
        //}


        //____________________________________________
        //                  갱신 처리 
        //____________________________________________

        //Vector3 _launchPos = ConstV.v3_zero; //시작위치
        //Vector3 _targetPos = ConstV.v3_zero;
        //Vector3 _maxHeight_pos = ConstV.v3_zero; //곡선의 최고점이 되는 위치 
        //Vector3 _perpNormal = ConstV.v3_zero; //target - launch 벡터의 수직노멀 
        //Vector3 _prev_pos = ConstV.v3_zero;
        //float _shotMoveTime = 1f; //샷 이동 시간 

        //float _elapsedTime = 0f;
        //public float _maxHeight_length = 2f; //곡선의 최고점 높이 길이
        //public float _maxHeight_posRate = 0.5f; //곡선의 최고점이 되는 위치 비율
        //public void ThrowThings(Being owner, Vector3 launchPos, Vector3 targetPos)
        //{

        //    if (null == (object)_sprRender)
        //        return; //Start 함수 수행전에 호출된 경우 


        //    if (false == _on_theWay && null != owner)
        //    {
        //        base.Update_SortingOrder(1000); //지형위로 날라다니게 설정 

        //        _transform.position = launchPos;
        //        _on_theWay = true;
        //        float shake = (float)Misc.RandInRange(0f, 0.2f); //흔들림 정도
        //        _launchPos = launchPos;
        //        _targetPos = targetPos + Misc.GetDir8_Random_AxisY() * shake;
        //        Vector3 toTarget = VOp.Minus(_targetPos, _launchPos);

        //        //기준값 : 7미터 당 1초
        //        _shotMoveTime = toTarget.magnitude / (GridManager.MeterToWorld * 7f);
        //        //DebugWide.LogBlue((_targetPos - _launchPos).magnitude + "  " + _shotMoveTime);

        //        //_perpNormal = Vector3.Cross(_targetPos - _launchPos, Vector3.up).normalized;
        //        _perpNormal = Misc.GetDir8_Normal3D(Vector3.Cross(toTarget, ConstV.v3_up));
        //        _elapsedTime = 0f;
        //        _owner = owner;
        //        _prev_pos = _launchPos; //!
        //        _gameObject.SetActive(true);


        //        //초기 방향 설정 
        //        Vector3 angle = ConstV.v3_zero;
        //        //angle.y = Vector3.SignedAngle(ConstV.v3_forward, toTarget, ConstV.v3_up);
        //        angle.y = Geo.AngleSigned_AxisY(ConstV.v3_forward, toTarget);
        //        _transform.localEulerAngles = angle;


        //        float posRateVert = 0f;
        //        Vector3 posHori = ConstV.v3_zero;
        //        if (eDirection8.up == owner._move._eDir8)
        //        {
        //            //그림자를 참 뒤에 보이게 함
        //            posRateVert = 0.8f;
        //        }
        //        else if (eDirection8.down == owner._move._eDir8)
        //        {
        //            //그림자를 창 앞에 보이게 함 
        //            posRateVert = 0.3f;
        //        }
        //        else
        //        {
        //            //* 캐릭터 방향이 위,아래가 아니면 포물선을 나타내기 위해 중앙노멀값을 적용한다
        //            if (owner._move._direction.x < 0)
        //            {
        //                //왼쪽방향을 보고 있을 때, 중앙노멀값의 방향을 바꿔준다 
        //                _perpNormal *= -1f;
        //            }
        //            posRateVert = _maxHeight_posRate + shake;
        //            posHori = VOp.Multiply(_perpNormal, _maxHeight_length);
        //        }
        //        _maxHeight_pos = VOp.Multiply(toTarget, posRateVert);
        //        _maxHeight_pos = VOp.Plus(_maxHeight_pos, _launchPos);
        //        _maxHeight_pos = VOp.Plus(_maxHeight_pos, posHori);


        //        //TweenStart();

        //    }
        //}

        //public Vector3 ClosestPoint_ToBezPos(Vector3 bezPos)
        //{
        //    Vector3 direction = _targetPos - _launchPos;
        //    Vector3 w = bezPos - _launchPos;
        //    float proj = Vector3.Dot(w, direction);
        //    // endpoint 0 is closest point
        //    if (proj <= 0.0f)
        //        return _launchPos;
        //    else
        //    {
        //        float vsq = Vector3.Dot(direction, direction);
        //        // endpoint 1 is closest point
        //        if (proj >= vsq)
        //            return _launchPos + direction;
        //        // else somewhere else in segment
        //        else
        //            return bezPos - (_launchPos + (proj / vsq) * direction);
        //    }
        //}

        //Vector3 _ori_scale = Vector3.one;
        //public void Update_Shot()
        //{

        //    if (true == this._on_theWay)
        //    {
        //        base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
        //        base.Update_SortingOrder(1000); //지형위로 날라다니게 한다 

        //        _elapsedTime += Time.deltaTime;

        //        //Rotate_Towards_FrontGap(this.transform);
        //        //float angle = Vector3.SignedAngle(Vector3.forward, __targetPos - __launchPos, Vector3.up);
        //        //Vector3 euler = __shot.transform.localEulerAngles;
        //        //euler.y = angle;
        //        //__shot.transform.localEulerAngles = euler;

        //        float zeroOneRate = _elapsedTime / _shotMoveTime;

        //        //* 화살 중간비율값 위치에 최대크기 비율값 계산
        //        //중간비율값 위치에 최고비율로 값변형 : [ 0 ~ 0.7 ~ 1 ] => [ 0 ~ 1 ~ 0 ]
        //        float scaleMaxRate = 0f;
        //        if (zeroOneRate < _maxHeight_posRate)
        //        {
        //            scaleMaxRate = zeroOneRate / _maxHeight_posRate;
        //        }
        //        else
        //        {
        //            scaleMaxRate = 1f - (zeroOneRate - _maxHeight_posRate) / (1f - _maxHeight_posRate);
        //        }
        //        _sprParent.localScale = _ori_scale + (ConstV.v3_one * scaleMaxRate * 0.5f); //최고점에서 1.5배
        //        _shader.transform.localScale = (_ori_scale * 0.7f) + (ConstV.v3_one * scaleMaxRate * 0.5f); //시작과 끝위치점에서 0.7배. 최고점에서 1.2배

        //        //* 이동 , 회전 
        //        this.transform.position = Vector3.Lerp(_launchPos, _targetPos, zeroOneRate); //그림자 위치 설정 
        //        _sprParent.position = Misc.BezierCurve(_launchPos, _maxHeight_pos, _targetPos, zeroOneRate);
        //        Rotate_Towards_FrontGap(_sprParent);


        //        if (_shotMoveTime < _elapsedTime)
        //        {
        //            //base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
        //            Update_CellSpace();//마지막 위치 등록 - 등록된 객체만 충돌처리대상이 된다 
        //            this.End();
        //        }


        //        //================================================
        //        //Update_CellInfo(); //매프레임 마다 충돌검사의 대상이 된다

        //        //덮개 타일과 충돌하면 동작을 종료한다 
        //        int result = SingleO.gridManager.IsIncluded_InDiagonalArea(this.transform.position);
        //        if (GridManager._ReturnMessage_Included_InStructTile == result ||
        //           GridManager._ReturnMessage_Included_InDiagonalArea == result)
        //        {

        //            End();
        //        }
        //        //================================================
        //        //debug test
        //        //Debug.DrawLine(_maxHeight_pos, _maxHeight_pos - _perpNormal * _maxHeight_length);
        //        //Debug.DrawLine(_launchPos, _targetPos);
        //    }
        //}
    }

    public class Shot : Being
    {
        public Being _owner = null; //발사체를 소유한 객체
        public bool _on_theWay = false; //발사되어 날아가는 중
        private Transform _sprParent = null;
        private Transform _shader = null;



        //private void Start()
        //{
        //    //base.Init();

        //}

        public override void Init()
        {
            base.Init();

            //_sprParent = SingleO.hierarchy.GetTransformA(SingleO.hierarchy.GetFullPath(transform) + "/pos");
            //_shader = SingleO.hierarchy.GetTransformA(SingleO.hierarchy.GetFullPath(transform) + "/shader");

            //_sprParent = SingleO.hierarchy.GetTransformA(transform, "/pos");
            //_shader = SingleO.hierarchy.GetTransformA(transform, "/shader");
            //this.gameObject.SetActive(false); //start 함수 호출하는 메시지가 비활성객체에는 전달이 안된다


            _sprParent = SingleO.hierarchy.GetTransform(transform, "pos");
            _shader = SingleO.hierarchy.GetTransform(transform, "shader");


        }

        //private void Update()
        //{

        //    Update_Shot();
        //    //Update_CellInfo(); //매프레임 마다 충돌검사의 대상이 된다

        //    //덮개 타일과 충돌하면 동작을 종료한다 
        //    int result = SingleO.gridManager.IsIncluded_InDiagonalArea(this.transform.position);
        //    if(GridManager._ReturnMessage_Included_InStructTile == result ||
        //       GridManager._ReturnMessage_Included_InDiagonalArea == result)
        //    {

        //        End();
        //    }

        //}

        //샷의 움직임을 종료한다 
        public void End()
        {
            if (null != (object)_owner)
            {
                ChampUnit champ = _owner as ChampUnit;
                if (null != (object)champ)
                {
                    champ._shot = null;
                }
            }
            _owner = null;
            _on_theWay = false;

            //spr 위치를 현재위치로 적용
            _transform.position = _sprParent.position;
            _sprParent.localPosition = ConstV.v3_zero;

            //캐릭터 아래에 깔리도록 설정
            base.Update_SortingOrder(-500);
            //_sortingGroup.sortingOrder = base.GetSortingOrder(0);


        }

        //____________________________________________
        //                  충돌반응
        //____________________________________________

        public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        {
            //무언가와 충돌했으면 투사체를 해제한다 (챔프 , 숥통 등)

            if ((object)dst != (object)_owner)
                End();
        }


        //____________________________________________
        //                  갱신 처리 
        //____________________________________________

        Vector3 _launchPos = ConstV.v3_zero; //시작위치
        Vector3 _targetPos = ConstV.v3_zero;
        Vector3 _maxHeight_pos = ConstV.v3_zero; //곡선의 최고점이 되는 위치 
        Vector3 _perpNormal = ConstV.v3_zero; //target - launch 벡터의 수직노멀 
        Vector3 _prev_pos = ConstV.v3_zero;
        float _shotMoveTime = 1f; //샷 이동 시간 

        float _elapsedTime = 0f;
        public float _maxHeight_length = 2f; //곡선의 최고점 높이 길이
        public float _maxHeight_posRate = 0.5f; //곡선의 최고점이 되는 위치 비율
        public void ThrowThings(Being owner, Vector3 launchPos, Vector3 targetPos)
        {

            if (null == (object)_sprRender)
                return; //Start 함수 수행전에 호출된 경우 


            if (false == _on_theWay && null != owner)
            {
                base.Update_SortingOrder(1000); //지형위로 날라다니게 설정 

                _transform.position = launchPos;
                _on_theWay = true;
                float shake = (float)Misc.RandInRange(0f, 0.2f); //흔들림 정도
                _launchPos = launchPos;
                _targetPos = targetPos + Misc.GetDir8_Random_AxisY() * shake;
                Vector3 toTarget = VOp.Minus(_targetPos, _launchPos);

                //기준값 : 7미터 당 1초
                _shotMoveTime = toTarget.magnitude / (GridManager.MeterToWorld * 7f);
                //DebugWide.LogBlue((_targetPos - _launchPos).magnitude + "  " + _shotMoveTime);

                //_perpNormal = Vector3.Cross(_targetPos - _launchPos, Vector3.up).normalized;
                _perpNormal = Misc.GetDir8_Normal3D(Vector3.Cross(toTarget, ConstV.v3_up));
                _elapsedTime = 0f;
                _owner = owner;
                _prev_pos = _launchPos; //!
                _gameObject.SetActive(true);


                //초기 방향 설정 
                Vector3 angle = ConstV.v3_zero;
                //angle.y = Vector3.SignedAngle(ConstV.v3_forward, toTarget, ConstV.v3_up);
                angle.y = Geo.AngleSigned_AxisY(ConstV.v3_forward, toTarget);
                _transform.localEulerAngles = angle;


                float posRateVert = 0f;
                Vector3 posHori = ConstV.v3_zero;
                if (eDirection8.up == owner._move._eDir8)
                {
                    //그림자를 참 뒤에 보이게 함
                    posRateVert = 0.8f;
                }
                else if (eDirection8.down == owner._move._eDir8)
                {
                    //그림자를 창 앞에 보이게 함 
                    posRateVert = 0.3f;
                }
                else
                {
                    //* 캐릭터 방향이 위,아래가 아니면 포물선을 나타내기 위해 중앙노멀값을 적용한다
                    if (owner._move._direction.x < 0)
                    {
                        //왼쪽방향을 보고 있을 때, 중앙노멀값의 방향을 바꿔준다 
                        _perpNormal *= -1f;
                    }
                    posRateVert = _maxHeight_posRate + shake;
                    posHori = VOp.Multiply(_perpNormal, _maxHeight_length);
                }
                _maxHeight_pos = VOp.Multiply(toTarget, posRateVert);
                _maxHeight_pos = VOp.Plus(_maxHeight_pos, _launchPos);
                _maxHeight_pos = VOp.Plus(_maxHeight_pos, posHori);


                //TweenStart();

            }
        }

        //public Vector3 ClosestPoint_ToBezPos(Vector3 bezPos)
        //{
        //    Vector3 direction = _targetPos - _launchPos;
        //    Vector3 w = bezPos - _launchPos;
        //    float proj = Vector3.Dot(w, direction);
        //    // endpoint 0 is closest point
        //    if (proj <= 0.0f)
        //        return _launchPos;
        //    else
        //    {
        //        float vsq = Vector3.Dot(direction, direction);
        //        // endpoint 1 is closest point
        //        if (proj >= vsq)
        //            return _launchPos + direction;
        //        // else somewhere else in segment
        //        else
        //            return bezPos - (_launchPos + (proj / vsq) * direction);
        //    }
        //}

        Vector3 _ori_scale = Vector3.one;
        public void Update_Shot()
        {

            if (true == this._on_theWay)
            {
                base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
                base.Update_SortingOrder(1000); //지형위로 날라다니게 한다 

                _elapsedTime += Time.deltaTime;

                //Rotate_Towards_FrontGap(this.transform);
                //float angle = Vector3.SignedAngle(Vector3.forward, __targetPos - __launchPos, Vector3.up);
                //Vector3 euler = __shot.transform.localEulerAngles;
                //euler.y = angle;
                //__shot.transform.localEulerAngles = euler;

                float zeroOneRate = _elapsedTime / _shotMoveTime;

                //* 화살 중간비율값 위치에 최대크기 비율값 계산
                //중간비율값 위치에 최고비율로 값변형 : [ 0 ~ 0.7 ~ 1 ] => [ 0 ~ 1 ~ 0 ]
                float scaleMaxRate = 0f;
                if (zeroOneRate < _maxHeight_posRate)
                {
                    scaleMaxRate = zeroOneRate / _maxHeight_posRate;
                }
                else
                {
                    scaleMaxRate = 1f - (zeroOneRate - _maxHeight_posRate) / (1f - _maxHeight_posRate);
                }
                _sprParent.localScale = _ori_scale + (ConstV.v3_one * scaleMaxRate * 0.5f); //최고점에서 1.5배
                _shader.transform.localScale = (_ori_scale * 0.7f) + (ConstV.v3_one * scaleMaxRate * 0.5f); //시작과 끝위치점에서 0.7배. 최고점에서 1.2배

                //* 이동 , 회전 
                this.transform.position = Vector3.Lerp(_launchPos, _targetPos, zeroOneRate); //그림자 위치 설정 
                _sprParent.position = Misc.BezierCurve(_launchPos, _maxHeight_pos, _targetPos, zeroOneRate);
                Rotate_Towards_FrontGap(_sprParent);


                if (_shotMoveTime < _elapsedTime)
                {
                    //base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
                    Update_CellSpace();//마지막 위치 등록 - 등록된 객체만 충돌처리대상이 된다 
                    this.End();
                }


                //================================================
                //Update_CellInfo(); //매프레임 마다 충돌검사의 대상이 된다

                //덮개 타일과 충돌하면 동작을 종료한다 
                int result = SingleO.gridManager.IsIncluded_InDiagonalArea(this.transform.position);
                if (GridManager._ReturnMessage_Included_InStructTile == result ||
                   GridManager._ReturnMessage_Included_InDiagonalArea == result)
                {

                    End();
                }
                //================================================
                //debug test
                //Debug.DrawLine(_maxHeight_pos, _maxHeight_pos - _perpNormal * _maxHeight_length);
                //Debug.DrawLine(_launchPos, _targetPos);
            }
        }


        public void Rotate_Towards_FrontGap(Transform tr)
        {
            Vector3 dir = VOp.Minus(tr.position, _prev_pos);

            //목표지점의 반대방향일 경우는 처리하지 않는다 
            if (0 > Vector3.Dot(VOp.Minus(_targetPos, _launchPos), dir))
                return;

            const float TILE_LENGTH_SQR = 0.16f * 0.16f;
            if (dir.sqrMagnitude <= TILE_LENGTH_SQR) //타일 하나의 길이만큼 거리가 있어야 함a
                return;


            Vector3 euler = tr.eulerAngles;
            float angle = (float)Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            //아래 3가지 방법도 같은 표현이다
            //float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            //tr.localRotation = Quaternion.LookRotation(dir, Vector3.up);
            //tr.localRotation = Quaternion.FromToRotation(Vector3.forward, dir);

            //euler.x = 90f; euler.z = -30f;//euler.z = -30f;
            euler.y = angle;
            tr.eulerAngles = euler;

            _prev_pos = tr.position;
        }



        public void TweenStart()
        {
            Vector3[] list = { _launchPos, _maxHeight_pos, _targetPos };
            iTween.MoveTo(_gameObject, iTween.Hash(
                "time", 0.8f
                , "easetype", "easeOutBack"
                , "path", list
                //,"orienttopath",true
                //,"axis","z"
                , "islocal", true //로컬위치값을 사용하겠다는 옵션. 대상객체의 로컬위치값이 (0,0,0)이 되는 문제 있음. 직접 대상객체 로컬위치값을 더해주어야 한다.
                , "movetopath", false //현재객체에서 첫번째 노드까지 자동으로 경로를 만들겠냐는 옵션. 경로 생성하는데 문제가 있음. 비활성으로 사용해야함
                                      //"looktarget",new Vector3(5,-5,7)
                , "onupdate", "Rotate_Towards_FrontGap"
                , "onupdatetarget", _gameObject
                , "onupdateparams", _transform
            ));

            //DebugWide.LogBlue("--------------TweenStart"); //chamto test
        }

    }

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
            Hand_Left,  //행동 왼손
            Hand_Right, //행동 오른손

            Max,
x        }


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
        private SpriteRenderer[] _effect = new SpriteRenderer[(int)eEffectKind.Max];
        //public Transform _effect_aim = null; //조준
        //public Transform _effect_dir = null; //방향 
        //public Transform _effect_emotion = null; //감정 표현 
        //public Transform _effect_hand_left = null; //왼손 
        //public Transform _effect_hand_right = null; //오른손

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

            Transform effectTr = SingleO.hierarchy.GetTransform(transform, "effect");
            _effect[(int)eEffectKind.Aim] = SingleO.hierarchy.GetTypeObject<SpriteRenderer>(effectTr, "aim");
            _effect[(int)eEffectKind.Dir] = SingleO.hierarchy.GetTypeObject<SpriteRenderer>(effectTr, "dir");
            _effect[(int)eEffectKind.Emotion] = SingleO.hierarchy.GetTypeObject<SpriteRenderer>(effectTr, "emotion");
            _effect[(int)eEffectKind.Hand_Left] = SingleO.hierarchy.GetTypeObject<SpriteRenderer>(effectTr, "hand_left");
            _effect[(int)eEffectKind.Hand_Right] = SingleO.hierarchy.GetTypeObject<SpriteRenderer>(effectTr, "hand_right");

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
            _ui_circle = SingleO.lineControl.Create_Circle_AxisY(this.transform, _activeRange.radius, Color.green);
            _ui_hp = SingleO.lineControl.Create_LineHP_AxisY(this.transform);
            _ui_circle.gameObject.SetActive(false);
            _ui_hp.gameObject.SetActive(false);
            ////SingleO.lineControl.SetScale(_UIID_circle_collider, 2f);
        }


        public override bool UpdateAll()
        {
            bool result = base.UpdateAll();
            if(true == result)
            {
                
                _limbs.Update_All(); //가지들 갱신 
                _limbs.Rotate(_move._direction); //move 에서 오일러각을 따로 구한것을 사용하도록 코드 수정하기 
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
            _animator.SetInteger(ANI_STATE, (int)Behavior.eKind.Attack);
            _behaviorKind = Behavior.eKind.Attack;
            _bodyControl.Attack_Strong_1();

            //_move._eDir8 = Misc.TransDirection8_AxisY(dir);
            //Switch_Ani("base_attack", _kind.ToString() + "_attack_", _move._eDir8);
            _appointmentDir = dir;
            //_target = target;

            //1회 공격중 방향변경 안되게 하기. 1회 공격시간의 80% 경과시 콜백호출 하기.
            Update_AnimatorState(ANI_STATE_ATTACK, 0.8f);

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

            if (hash_state == ANI_STATE_ATTACK)
            {
                //예약된 방향값 설정
                _move._eDir8 = Misc.GetDir8_AxisY(_appointmentDir);
                _move._direction = Misc.GetDir8_Normal3D_AxisY(_move._eDir8);
                Switch_Ani(_kind, eAniBaseKind.attack, _move._eDir8);

                //this.SetActiveEffect(eEffectKind.Hand_Right, true);
            }
        }

        //임시처리
        public override void OnAniState_End(int hash_state)
        {
            //DebugWide.LogYellow("OnAniState_End :" + hash_state + "  " + _hash_attack + "   "+ _target); //chamto test

            if (hash_state == ANI_STATE_ATTACK)
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
                        target_champ.ApplyUI_HPBar();
                    }

                }

            }
        }

        public void ApplyUI_HPBar()
        {
            //HP갱신 
            _ui_hp.SetLineHP((float)_hp_cur / (float)_hp_max);
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
            _sprRender.color = Color.red;

            if (true == __in_corutin_Damage)
                yield break;

            __in_corutin_Damage = true;
            yield return new WaitForSeconds(0.5f);

            this.SetActiveEffect(eEffectKind.Emotion, false);
            _sprRender.color = Color.white;
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
            if(true == _GIZMOS_BEHAVIOR)
                Debug_DrawGizmos_Behavior();
        }

        Vector3 _debug_dir = Vector3.zero;
        Quaternion _debug_q = Quaternion.identity;
        Vector3 _debug_line = Vector3.zero;
        public void Debug_DrawGizmos_Behavior()
        {
            BodyControl.Part HL = _bodyControl._parts[(int)BodyControl.Part.eKind.Hand_Left];
            BodyControl.Part HR = _bodyControl._parts[(int)BodyControl.Part.eKind.Hand_Right];

            Vector3 posBody = this.GetPos3D();
            Quaternion quater_r = Quaternion.FromToRotation(UtilGS9.ConstV.v3_forward, _move._direction);
            Vector3 posHL = quater_r * HL._pos_standard + posBody;
            Vector3 posHR = quater_r * HR._pos_standard + posBody;

            float weaponArc_degree = 45f;
            float weaponArc_radius_far = 3f;
            Vector3 weaponArc_dir = _move._direction;
            //*
            //어깨 기준점 
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(posHL, HL._range_max);
            Gizmos.DrawWireSphere(posHR, HR._range_max);

            //skill 진행 상태 출력 
            DebugWide.PrintText(posHR,Color.white,_bodyControl._skill_current._kind.ToString() + "  st: " + _bodyControl._state_current.ToString() + "   t: " + _bodyControl._timeDelta.ToString("00.00"));

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
            Vector3 weapon_curPos = posBody + _bodyControl.CurrentDistance() * weaponArc_dir;
            _debug_line.y = -0.5f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(posBody, weapon_curPos);
            Gizmos.DrawWireSphere(weapon_curPos, 0.1f);
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
            int _getPos1D = SingleO.cellPartition.ToPosition1D(_getPos3D);
            SingleO.cellPartition.AttachCellSpace(_getPos1D, this);


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


namespace HordeFight
{

    //자세 => 준비동작 => 공격 (3단계)

    //========================================================
    //==================     동작  정보     ==================
    //========================================================
    public partial class Behavior
    {
        //제거대상 
        public enum eKind
        {
            None = 0,

            Idle = 10,
            Idle_Random = 11,
            Idle_LookAt = 12,
            Idle_Max = 19,

            Move = 20,
            Move_Max = 29,

            Block = 30,
            Block_Max = 39,

            Attack = 40,
            Attack_Max = 49,

            FallDown = 50,
            FallDown_Max = 59,

        }

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

        public bool Valid_OpenTime(Being.ePhase phase, float timeDelta)
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

}

//========================================================
//==================     스킬  정보     ==================
//========================================================

namespace HordeFight
{
    public class Skill : List<Behavior>
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

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1f;

            bhvo.eventTime_0 = 0f;
            bhvo.eventTime_1 = 0f;
            bhvo.openTime_0 = 0;
            bhvo.openTime_1 = 10f;
            skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Move()
        {
            Skill skinfo = new Skill();

            skinfo._kind = eKind.Move;
            skinfo._name = eName.Move_0;

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1f;

            bhvo.eventTime_0 = 0f;
            bhvo.eventTime_1 = 0f;
            bhvo.openTime_0 = 0;
            bhvo.openTime_1 = 10f;
            skinfo.Add(bhvo);

            return skinfo;
        }


        static public Skill Details_Attack_Strong()
        {
            Skill skinfo = new Skill();

            skinfo._kind = eKind.Attack_Strong;
            skinfo._name = eName.Attack_Strong_1;

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 2.0f;
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
            bhvo.rigidTime = 0.5f;

            bhvo.movementShape = Behavior.eMovementShape.Straight;
            //bhvo.attack_shape = eTraceShape.Vertical;
            bhvo.angle = 45f;
            bhvo.plus_range_0 = 2f;
            bhvo.plus_range_1 = 2f;
            bhvo.distance_travel = 1f; 
            //bhvo.distance_maxTime = bhvo.eventTime_0; //유효범위 시작시간에 최대 거리가 되게 한다. : 떙겨치기 , [시간증가에 따라 유효거리 감소]
            bhvo.distance_maxTime = bhvo.eventTime_1; //유효범위 끝시간에 최대 거리가 되게 한다. : 일반치기 , [시간증가에 따라 유효거리 증가]

            bhvo.Calc_Velocity();
            skinfo.Add(bhvo);

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

            this.Add(Skill.eName.Move_0, Skill.Details_Move);

            this.Add(Skill.eName.Attack_Strong_1, Skill.Details_Attack_Strong);

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
    public class BodyControl
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


        public enum ePoint
        {
            Start,  //시작
            End,    //끝지

            Cur,    //현재

            Max,
        }

        public class Part
        {
            //신체부위
            public enum eKind
            {
                None = 0,

                //==== 인간형태 ==== 
                Head = 0,
                Hand_Left,  //손
                Hand_Right,
                Foot_Left,  //발
                Foot_Right,

                //Other_1,    //또다른 손발등
                //Other_2,

                Max,

            }


            public Vector3 _pos_standard;    //신체 부위의 기준점 
            public float _range_max;         //부위 기준점으로 부터 최대 범위
            public Vector3[] _pos;           //부위 위치 (로컬값)
            public Vector3[] _dir;           //부위 방향 (로컬값)

            public Vector3 _target;          //목표점 (월드값)

            public void Init()
            {
                _pos_standard = UtilGS9.ConstV.v3Int_zero;
                _range_max = 1f;
                _pos = new Vector3[(int)ePoint.Max];
                _dir = new Vector3[(int)ePoint.Max];

                _target = new Vector3(0,0,2f); //z축을 보게 한다 
            }

        }

        public class Weapon
        {
            public float length; //무기 길이 
            //public 
        }


        //====================================
        public Part[] _parts = null;    //부위 정보 


        //동작정보
        public Behavior _behavior = null;
        public Skill _skill_current = null;
        private Skill _skill_next = null;
        public float _timeDelta = 0f;  //시간변화량

        //상태정보
        public eState _state_current = eState.None;
        private eSubState _eventState_current = eSubState.None;     //유효상태
                                                                    
        //판정
        //private Judgment _judgment = new Judgment();



        //====================================

        public BodyControl()
        {
            _parts = new Part[(int)Part.eKind.Max];
            for (int i = 0; i < (int)Part.eKind.Max;i++)
            {
                _parts[i] = new Part();
                _parts[i].Init();
            }

            Setting_Head_2Hand_2Foot();
        }

        public void Setting_2Hand()
        {
            //x-z축 공간에 캐릭터가 놓여 있고, z축을 바라보고 있다 가정 < Forward 방향 >
            Part HL = _parts[(int)Part.eKind.Hand_Left];
            Part HR = _parts[(int)Part.eKind.Hand_Right];

            HL._pos_standard = new Vector3(-0.5f, 0.5f,0);
            HR._pos_standard = new Vector3(0.5f, 0.5f, 0);
            HR._range_max = 1.1f; //오른쪽 사정거리를 약간 더 늘린다 
        }

        public void Setting_Head_2Hand_2Foot()
        {
            //x-z축 공간에 캐릭터가 놓여 있고, z축을 바라보고 있다 가정 < Forward 방향 >
            Setting_2Hand();
            _parts[(int)Part.eKind.Head]._pos_standard = new Vector3(0, 1f, 0);
            _parts[(int)Part.eKind.Foot_Left]._pos_standard = new Vector3(-0.5f, 0, 0);
            _parts[(int)Part.eKind.Foot_Right]._pos_standard = new Vector3(0.5f, 0, 0);
        }



        public float CurrentDistance()
        {
            return _behavior.CurrentDistance(_timeDelta);
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

        public void SetState(eState setState)
        {
            _state_current = setState;
        }

        public void SetEventState(eSubState setSubState)
        {
            _eventState_current = setSubState;
        }


        public SkillBook ref_skillBook { get { return CSingleton<SkillBook>.Instance; } }

        public void SetSkill_Start(Skill.eName name)
        {
            this.SetSkill_Start(ref_skillBook.Refer(name));
        }

        public void SetSkill_Start(Skill skill)
        {
            _skill_current = skill;
            _behavior = _skill_current.FirstBehavior();
            SetState(eState.Start);
            this._timeDelta = 0f;
        }

        public void SetSkill_AfterInterruption(Skill.eName name)
        {
            this.SetSkill_AfterInterruption(ref_skillBook.Refer(name));
        }

        //현재 스킬 중단후 다음스킬 시작 (현재 스킬을 end 상태로 바로 전환한다)  
        public void SetSkill_AfterInterruption(Skill skill)
        {
            //현재 스킬이 지정되어 있지 않으면 바로 요청 스킬로 지정한다
            //현재 상태가 end라면 스킬을 바로 지정한다
            if (null == _skill_current || eState.End == this._state_current)
            {
                this.SetSkill_Start(skill);
                return;
            }

            _skill_next = skill;

            //SetState(eState.End); //계속 함수를 호출하면 End 상태를 못 벗어나 주석처리함 
        }

        public void Attack_Strong_1()
        {
            SetSkill_AfterInterruption(Skill.eName.Attack_Strong_1);
        }

        public void Move_0()
        {
            SetSkill_AfterInterruption(Skill.eName.Move_0);
        }

        public void Idle()
        {
            SetSkill_AfterInterruption(Skill.eName.Idle);
        }

        public void Update()
        {
            if (null == _behavior) return;

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
                        SetState(eState.Running);
                        SetEventState(eSubState.None);

                        //DebugWide.LogRed ("[0: "+this._state_current);//chamto test
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
                                if (_behavior.eventTime_0 <= _timeDelta && _timeDelta <= _behavior.eventTime_1)
                                {
                                    this.SetEventState(eSubState.Start);
                                }
                                break;
                            case eSubState.Start:
                                this.SetEventState(eSubState.Running);
                                break;
                            case eSubState.Running:
                                if (!(_behavior.eventTime_0 <= _timeDelta && _timeDelta < _behavior.eventTime_1))
                                {
                                    this.SetEventState(eSubState.End);
                                }

                                break;
                            case eSubState.End:
                                this.SetEventState(eSubState.None);
                                break;

                        }


                        if (_behavior.runningTime <= this._timeDelta)
                        {
                            //동작완료
                            this.SetState(eState.Waiting);
                        }
                    }
                    break;
                case eState.Waiting:
                    {
                        //DebugWide.LogBlue (_behavior.rigidTime + "   " + (this._timeDelta - _behavior.allTime));
                        if (_behavior.rigidTime <= (this._timeDelta - _behavior.runningTime))
                        {
                            this.SetState(eState.End);
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
                            _behavior = _skill_current.NextBehavior();
                            if (null == _behavior)
                            //if(false == _skill_current.IsNextBehavior())
                            {
                                //스킬 동작을 모두 꺼냈으면 아이들상태로 들어간다
                                Idle();
                            }
                            else
                            {
                                //_behavior = _skill_current.NextBehavior ().Clone();

                                //다음 스킬 동작으로 넘어간다
                                SetState(eState.Start);
                                _timeDelta = 0f;

                                DebugWide.LogBlue("next combo !!");
                            }
                        }

                    }
                    break;


            }

            //============================================================================

            //this.GetJudgment().Update(); //판정 후처리 갱신

            //if(1 == this.GetID())
            //  DebugWide.LogBlue ("[3:_receiveState_current : " + _receiveState_current);
            //============================================================================

        }//end func

    }
}

//===========================


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
    //==================   카메라 이동 정보   ==================
    //========================================================

    public class CameraWalk : MonoBehaviour
    {
        private Camera _camera = null;
        public Transform _target = null;

        public bool _enable_PixelPerfect = true;
        public float _PIXEL_PER_UNIT = 16f; //ppu
        public float _PIXEL_LENGTH = 0.0625f; //게임에서 사용하는 픽셀하나의 크기 

        private void Start()
        {
            _PIXEL_LENGTH = 1f / _PIXEL_PER_UNIT;
            _camera = SingleO.mainCamera;

        }

        private void Update()
        {
            if (null == _target || null == _camera) return;

            //_camera.cullingMask = 0;

            Vector3 targetPos = _target.position;
            targetPos.y = _camera.transform.position.y;

            if (true == _enable_PixelPerfect)
            {
                //픽셀퍼팩트 처리 : PIXEL_LENGTH 단위로 이동하게, 버림 처리한다 
                _camera.transform.position = ToPixelPerfect(targetPos, _PIXEL_PER_UNIT);
            }
            else
            {
                _camera.transform.position = targetPos;
            }

        }


        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public Vector3 ToPixelPerfect(Vector3 pos3d, float pixelPerUnit)
        {
            float pixel_length = 1f / pixelPerUnit;
            Vector3Int posXY_2d = new Vector3Int((int)pos3d.x, (int)pos3d.y, (int)pos3d.z);
            Vector3 decimalPoint = pos3d - posXY_2d;

            //예) 0.2341 => 0.0341
            Vector3 remainder = new Vector3(decimalPoint.x % pixel_length,
                                            decimalPoint.y % pixel_length,
                                            decimalPoint.z % pixel_length);


            return pos3d - remainder;
        }


    }

}

