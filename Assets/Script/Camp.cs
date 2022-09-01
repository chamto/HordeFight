using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Assertions;
using UtilGS9;


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
        //public Geo.Sphere _sphere = Geo.Sphere.Zero;
        public Geo.Sphere _sphere = new Geo.Sphere();

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

            //같은 캠프면 같은편이라고 반환해준다 
            if (camp_1 == camp_2)
                return Camp.eRelation.SameSide;

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

