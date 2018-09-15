using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Utility;

namespace HordeFight
{
    public class HordeFight_Main : MonoBehaviour
    {




        // Use this for initialization
        void Start()
        {
            ResolutionController.CalcViewportRect(Single.canvasRoot, Single.mainCamera); //화면크기조정
           
            gameObject.AddComponent<TouchProcess>();
            gameObject.AddComponent<LineControl>();
            gameObject.AddComponent<ObjectManager>();
            gameObject.AddComponent<GridManager>();
            gameObject.AddComponent<UI_Main>();

            Single.resourceManager.Init();

            //===================

            Single.objectManager.Create_StageInfo();


        }

        // Update is called once per frame
        void Update()
        {

        }


    }

}

//========================================================
//==================      전역  객체      ==================
//========================================================
namespace HordeFight
{
    public static class Single
    {
        
        public static TouchProcess touchProcess
        {
            get
            {
                return CSingletonMono<TouchProcess>.Instance;
            }
        }

        public static LineControl lineControl
        {
            get
            {
                return CSingletonMono<LineControl>.Instance;
            }
        }

        public static ObjectManager objectManager
        {
            get
            {
                return CSingletonMono<ObjectManager>.Instance;
            }
        }

        public static GridManager gridManager
        {
            get
            {
                return CSingletonMono<GridManager>.Instance;
            }
        }

        public static UI_Main uiMain
        {
            get
            {
                return CSingletonMono<UI_Main>.Instance;
            }
        }

        public static ResourceManager resourceManager
        {
            get
            {
                return CSingleton<ResourceManager>.Instance;
            }
        }

        private static Camera _mainCamera = null;
        public static Camera mainCamera
        {
            get
            {
                if (null == _mainCamera)
                {

                    GameObject obj = GameObject.Find("Main Camera");
                    if (null != obj)
                    {
                        _mainCamera = obj.GetComponent<Camera>();
                    }

                }
                return _mainCamera;
            }
        }


        private static Canvas _canvasRoot = null;
        public static Canvas canvasRoot
        {
            get
            {
                if (null == _canvasRoot)
                {
                    GameObject obj = GameObject.Find("Canvas");
                    if (null != obj)
                    {
                        _canvasRoot = obj.GetComponent<Canvas>();
                    }

                }
                return _canvasRoot;
            }
        }


        private static Transform _gridRoot = null;
        public static Transform gridRoot
        {
            get
            {
                if (null == _gridRoot)
                {
                    GameObject obj = GameObject.Find("0_grid");
                    if (null != obj)
                    {
                        _gridRoot = obj.GetComponent<Transform>();
                    }

                }
                return _gridRoot;
            }
        }

        private static Transform _unitRoot = null;
        public static Transform unitRoot
        {
            get
            {
                if (null == _unitRoot)
                {
                    GameObject obj = GameObject.Find("0_unit");
                    if (null != obj)
                    {
                        _unitRoot = obj.GetComponent<Transform>();
                    }

                }
                return _unitRoot;
            }
        }


    }

}//end namespace



//========================================================
//==================     리소스 관리기     ==================
//========================================================

namespace HordeFight
{
    public class ResourceManager
    {

        //키값 : 애니메이션 이름에 대한 해쉬코드 
        public Dictionary<int, AnimationClip> _aniClips = new Dictionary<int, AnimationClip>();
        public Dictionary<int, Sprite> _sprIcons = new Dictionary<int, Sprite>();


        //==================== Get / Set ====================


        //==================== <Method> ====================

        //public eSPRITE_NAME StringToEnum(string name)
        //{
        //    //20170813 chamto fixme - value 값이 없을 때의 예외 처리가 없음 
        //    //ref : https://stackoverflow.com/questions/2444033/get-dictionary-key-by-value
        //    return _spriteNames.FirstOrDefault(x => x.Value == name).Key;
        //}


        public void ClearAll()
        {

        }

        public void Init()
        {
            Load_Animation();
        }


        public void Load_Animation()
        {

            //=============================================
            //LOAD 
            //=============================================
            AnimationClip[] loaded = Resources.LoadAll<AnimationClip>("Warcraft/Animation");
            foreach(AnimationClip ac in loaded)
            {
                //ac.GetHashCode 값과 ac.name.GetHashCode 값은 다르다
                _aniClips.Add(ac.name.GetHashCode(), ac); 
            }

            Sprite[] spres = Resources.LoadAll<Sprite>("Warcraft/Textures/Icons");
            foreach(Sprite spr in spres)
            {
                _sprIcons.Add(spr.name.GetHashCode(), spr);
            }
        }



    }//end class


}


//========================================================
//==================      라인 관리기      ==================
//========================================================
namespace HordeFight
{
    public class LineControl : MonoBehaviour
    {

        private int _sequenceId = 0;
        private Dictionary<int, LineInfo> _list = new Dictionary<int, LineInfo>();


        public enum eKind
        {
            None,
            Line,   //hp 표현
            Circle, //캐릭터 선택 표현
            Square, //캐릭터 선택 표현
            Polygon,//여러 캐릭터 선택 표현
            Graph,  //경로 표현 
        }

        public struct LineInfo
        {
            public LineRenderer render;
            public eKind kind;
            public int id;

            public void Init()
            {
                render = null;
                kind = eKind.None;
                id = -1;
            }

            //public void Update_Circle()
            //{
            //    if (null == renderer) return;

            //    float deg = 360f / renderer.positionCount;
            //    float radius = renderer.transform.parent.GetComponent<CircleCollider2D>().radius;
            //    Vector3 pos = Vector3.right;
            //    for (int i = 0; i < renderer.positionCount; i++)
            //    {
            //        pos.x = Mathf.Cos(deg * i * Mathf.Deg2Rad) * radius;
            //        pos.y = Mathf.Sin(deg * i * Mathf.Deg2Rad) * radius;
            //        renderer.SetPosition(i, pos + renderer.transform.parent.position);
            //        //DebugWide.LogBlue(Mathf.Cos(deg * i * Mathf.Deg2Rad) + " _ " + deg*i);
            //    }
            //}
        }

		private void Start()
		{
			
		}

		private void Update()
		{
            //foreach(LineInfo info in _list.Values)
            //{
            //    if(eKind.Circle == info.kind)
            //    {
                    
            //    }
            //}
		}

        public int Create_LineHP_AxisY(Transform dst)
        {
            GameObject obj = new GameObject();
            LineRenderer render = obj.AddComponent<LineRenderer>();
            LineInfo info = new LineInfo();
            info.Init();

            _sequenceId++;

            info.id = _sequenceId;
            info.render = render;
            info.kind = eKind.Line;

            render.name = info.kind.ToString() + "_" + _sequenceId.ToString("000");
            render.material = new Material(Shader.Find("Sprites/Default"));
            render.useWorldSpace = false; //로컬좌표로 설정하면 부모객체 이동시 영향을 받는다. (변경정보에 따른 재갱싱 필요없음)
            render.transform.parent = dst;
            render.sortingOrder = -10; //나중에 그려지게 한다.
            render.positionCount = 2;

            render.SetWidth(0.02f, 0.02f);
            render.SetColors(Color.red, Color.red);


            _list.Add(_sequenceId, info); //추가

            Vector3 pos = Vector3.zero;
            pos.x = -0.05f; pos.z = -0.15f;
            render.SetPosition(0, pos);
            pos.x += 0.1f;
            render.SetPosition(1, pos);

            return _sequenceId;
        }

        public int Create_Circle_AxisY(Transform dst)
        {
            GameObject obj = new GameObject();
            LineRenderer render = obj.AddComponent<LineRenderer>();
            LineInfo info = new LineInfo();
            info.Init();

            _sequenceId++;

            info.id = _sequenceId;
            info.render = render;
            info.kind = eKind.Circle;

            render.name = info.kind.ToString() + "_" + _sequenceId.ToString("000");
            render.material = new Material(Shader.Find("Sprites/Default"));
            render.useWorldSpace = false; //로컬좌표로 설정하면 부모객체 이동시 영향을 받는다. (변경정보에 따른 재갱싱 필요없음)
            render.transform.parent = dst;//부모객체 지정
            render.sortingOrder = -10; //먼저그려지게 한다.
            render.positionCount = 20;
            render.loop = true; //처음과 끝을 연결한다 .

            render.SetWidth(0.01f, 0.01f);
            render.SetColors(Color.green, Color.green);


            _list.Add(_sequenceId, info); //추가

            //info.Update_Circle(); //값설정
            float deg = 360f / render.positionCount;
            float radius = render.transform.parent.GetComponent<SphereCollider>().radius;
            Vector3 pos = Vector3.right;
            for (int i = 0; i < render.positionCount; i++)
            {
                pos.x = Mathf.Cos(deg * i * Mathf.Deg2Rad) * radius;
                pos.z = Mathf.Sin(deg * i * Mathf.Deg2Rad) * radius;

                render.SetPosition(i, pos );
                //DebugWide.LogBlue(Mathf.Cos(deg * i * Mathf.Deg2Rad) + " _ " + deg*i);
            }

            return _sequenceId;

        }

        public void Create_Square(Transform dst)
        { }

        public void Create_Polygon(Transform dst)
        { }

        public void SetActive(int id, bool onOff)
        {
            //todo : 예외처리 추가하기 
            _list[id].render.gameObject.SetActive(onOff);
        }

        //rate : 0~1
        public void SetLineHP(int id, float rate)
        {
            if (0 > rate) rate = 0;
            if (1f < rate) rate = 1f;

            LineRenderer render = _list[id].render;
            Vector3 pos = Vector3.zero;
            pos.x = -0.05f; pos.z = -0.15f;
            render.SetPosition(0, pos);
            pos.x += (0.1f * rate) ;
            render.SetPosition(1, pos);
        }

	}
}


//========================================================
//==================     그리드 관리기     ==================
//========================================================

namespace HordeFight
{

    //키값 : being.id
    public class CellInfo : LinkedList<Being>
    {
        public struct Index
        {
            public int n1;
            public int n2;


            public override string ToString()
            {
                return n1 + ", " + n2;
            }

            public static bool operator ==(Index a, Index b)
            {
                if (a.n1 == b.n1 && a.n2 == b.n2)
                    return true;

                return false;
            }

            public static bool operator !=(Index a, Index b)
            {
                if (a.n1 == b.n1 && a.n2 == b.n2)
                    return false;

                return true;
            }
        }

        public Index _index = default(Index);
    }



    public class GridManager : MonoBehaviour
    {

        private Grid _grid = null;
        public Dictionary<CellInfo.Index,CellInfo> _cellList = new Dictionary<CellInfo.Index,CellInfo>();

		private void Start()
		{
            _grid = GameObject.Find("0_grid").GetComponent<Grid>();
		}

		private void Update()
		{
			
		}

        public CellInfo GetCellInfo(CellInfo.Index cellIndex)
        {
            CellInfo cell = null;
            _cellList.TryGetValue(cellIndex, out cell);

            return cell;
        }


        public CellInfo GetCellInfo_NxN(CellInfo.Index center , ushort NCount_odd)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == NCount_odd % 2) return null;

            CellInfo dst = null;
            CellInfo cellList = new CellInfo();
            cellList._index = center;


            DebugWide.LogBlue("================" + NCount_odd + " ================ ");
            string temp1 = "";
            int startIdx = (NCount_odd - 1) / 2; //중심좌표를 (0,0)으로 만들기 위함
            for (int i = -startIdx; i < NCount_odd - startIdx; i++)
            {
                temp1 = "";
                for (int j = -startIdx; j < NCount_odd - startIdx; j++)
                {
                    temp1 += "(" + i + ", "+ j +")  "; 

                }
                DebugWide.LogBlue(temp1);
            }




            return cellList;
        }


        public void AddCellInfo_Being(CellInfo.Index cellIndex , Being being)
        {
            CellInfo cell = null;
            if(false == _cellList.TryGetValue(cellIndex, out cell))
            {
                cell = new CellInfo();
                cell._index = cellIndex;
                _cellList.Add(cellIndex, cell);
            }
            _cellList[cellIndex].AddLast(being);
        }

        public void RemoveCellInfo_Being(CellInfo.Index cellIndex, Being being)
        {
            if (null == being) return;

            CellInfo cell = null;
            if (true == _cellList.TryGetValue(cellIndex, out cell))
            {
                cell.Remove(being);
            }

        }


        public CellInfo.Index ToCellIndex(Vector3 pos , Vector3 axis)
        {

            CellInfo.Index cellIndex = new CellInfo.Index();
            cellIndex = default(CellInfo.Index);
            Vector3 cellSize = Vector3.zero;

            if(Vector3.up == axis)
            {
                //그리드 자체에 스케일을 적용시킨 경우가 있으므로 스케일값을 적용한다. 
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x);
                cellSize.z = (_grid.cellSize.y * _grid.transform.localScale.z);


                if(0 <= pos.x)
                {
                    //양수일때는 소수점을 버린다. 
                    cellIndex.n1 = (int)(pos.x / cellSize.x);
                }
                else
                {
                    //음수일때는 올림을 한다. 
                    cellIndex.n1 = (int)((pos.x / cellSize.x) - 0.9f);
                }


                if (0 <= pos.z)
                { 
                    cellIndex.n2 = (int)(pos.z / cellSize.z);
                }
                else
                { 
                    cellIndex.n2 = (int)((pos.z / cellSize.z) - 0.9f);
                }

            }

            return cellIndex;
        }

        public Vector3 ToPosition(CellInfo.Index ci , Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            if (Vector3.up == axis)
            {
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x);
                cellSize.z = (_grid.cellSize.y * _grid.transform.localScale.z);

                pos.x = (float)ci.n1 * cellSize.x;
                pos.z = (float)ci.n2 * cellSize.z;

                //셀의 중간에 위치하도록 한다
                pos.x += cellSize.x * 0.5f;
                pos.z += cellSize.z * 0.5f;
            }

            return pos;
        }
	}
}


//========================================================
//==================      객체 관리기      ==================
//========================================================

namespace HordeFight
{

    public class ObjectManager : MonoBehaviour
    {
        public Dictionary<uint, Being> _beings = new Dictionary<uint, Being>();
        public List<Being> _listTest = new List<Being>();

        private void Start()
        {

        }


        //객체간의 충돌검사 최적화를 위한 충돌가능객체군 미리 조직하기 
        private void Update()
        {
            //UpdateCollision();

            //UpdateCollision_UseDictElementAt(); //obj100 , fps10
            //UpdateCollision_UseDictForeach(); //obj100 , fps60
            //UpdateCollision_UseList(); //obj100 , fps80 : obj200 , fps45 , nonUpdateFps100
        }

        //** 그리드 조직 하기



        //챔프를 중심으로 3x3그리드 영역의 정보를 가지고 충돌검사한다
        public void UpdateCollision_UseGrid3x3() //3x3 => 5x5 => 7x7 ... 홀수로 그리드 범위를 늘려 테스트 해볼 수 있다
        {
            //1. 3x3그리드 정보를 가져온다
            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            foreach(Being a in _listTest)
            {

                //3x3그리드 정보
                //Single.gridManager.GetCellInfo()
                foreach(Being b in a._cellInfo)
                {
                    if (a == b) continue;

                    //충돌검사    
                }

            }

        }

        public void UpdateCollision_UseDictForeach()
        {

            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;



            foreach(Being src in _beings.Values)
            {
                foreach (Being dst in _beings.Values)
                {
                    if (src == dst) continue;

                    sqr_dis = src.transform.localPosition - dst.transform.localPosition;

                    r_sum = src.GetCollider_Radius() + dst.GetCollider_Radius();

                    //1.두 캐릭터가 겹친상태 
                    if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
                    {
                        //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                        //todo : 최적화 필요 

                        Vector3 n = sqr_dis.normalized;
                        //Vector3 n = sqr_dis;
                        float div_dis = 0.1f;

                        //2.반지름 이상으로 겹쳐있는 경우
                        if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
                        {
                            //3.완전 겹쳐있는 경우
                            if (n == Vector3.zero)
                            {
                                //방향값이 없기 때문에 임의로 지정해 준다. 
                                n = Misc.RandomDir8_AxisY();
                            }

                            div_dis = 0.3f;
                        }

                        src._move.Move_Forward(n, div_dis, 1);
                        dst._move.Move_Forward(-n, div_dis, 1);


                    }
                }
            }


        }


        //딕셔너리 보다 인덱싱 속도가 빠르다. 안정적 객체수 : 500
        public void UpdateCollision_UseList()
        {

            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;
            Being src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < _beings.Count - 1; i++)
            {
                for (int j = i + 1; j < _beings.Count; j++)
                {
                    src = _listTest[i];
                    dst = _listTest[j];

                    sqr_dis = src.transform.localPosition - dst.transform.localPosition;

                    r_sum = src.GetCollider_Radius() + dst.GetCollider_Radius();

                    //1.두 캐릭터가 겹친상태 
                    if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
                    {
                        //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                        //todo : 최적화 필요 

                        Vector3 n = sqr_dis.normalized;
                        //Vector3 n = sqr_dis;
                        float div_dis = 0.1f;

                        //2.반지름 이상으로 겹쳐있는 경우
                        if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
                        {
                            //3.완전 겹쳐있는 경우
                            if (n == Vector3.zero)
                            {
                                //방향값이 없기 때문에 임의로 지정해 준다. 
                                n = Misc.RandomDir8_AxisY();
                            }

                            div_dis = 0.3f;
                        }

                        src._move.Move_Forward(n, div_dis, 1);
                        dst._move.Move_Forward(-n, div_dis, 1);

                        //_characters[i].Idle_View(n, false);
                        //_characters[j].Idle_View(-n, false);
                    }


                }
            }
        }

        //딕션너리의 ElementAt 을 사용하여 객체를 가져오는것은 부하가 심하다. 안정적 객체수 : 100이하 
        public void UpdateCollision_UseDictElementAt()
        {
            
            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;
            Being src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < _beings.Count - 1; i++)
            {
                for (int j = i + 1; j < _beings.Count; j++)
                {
                    src = _beings.Values.ElementAt(i);
                    dst = _beings.Values.ElementAt(j);

                    sqr_dis = src.transform.localPosition - dst.transform.localPosition;

                    r_sum = src.GetCollider_Radius() + dst.GetCollider_Radius();

                    //1.두 캐릭터가 겹친상태 
                    if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
                    {
                        //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                        //todo : 최적화 필요 

                        Vector3 n = sqr_dis.normalized;
                        float div_dis = 0.1f;

                        //2.반지름 이상으로 겹쳐있는 경우
                        if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
                        {
                            //3.완전 겹쳐있는 경우
                            if (n == Vector3.zero)
                            {
                                //방향값이 없기 때문에 임의로 지정해 준다. 
                                n = Misc.RandomDir8_AxisY();
                            }

                            div_dis = 0.5f;
                        }

                        src._move.Move_Forward(n, div_dis, 1);
                        dst._move.Move_Forward(-n, div_dis, 1);

                        //_characters[i].Idle_View(n, false);
                        //_characters[j].Idle_View(-n, false);
                    }


                }
            }
        }


        public void ClearAll()
        {
            
            foreach (Being b in _beings.Values)
            {
                GameObject.Destroy(b.gameObject);
            }

            _beings.Clear();

        }

        public Being GetCharacter(uint id)
        {
            Being being = null;
            if(true == _beings.TryGetValue(id, out being))
            {
                return being;
            }


            return null;
        }


        /// <summary>
        /// 최소 반경보다 크고 최대 반경보다 작은 범위 안에서 가장 가까운 객체를 반환한다
        /// </summary>
        public Being GetNearCharacter(Being exceptChar, float minRadius, float maxRadius)
        {
            return null;

            float sqr_minRadius = Mathf.Pow(minRadius, 2);
            float sqr_maxRadius = Mathf.Pow(maxRadius, 2);
            float min_value = sqr_maxRadius * 2f; //최대 반경보다 큰 최대값 지정
            float sqr_dis = 0f;
            Being target = null;
            foreach (Being t in _beings.Values)
            {
                if (t == exceptChar) continue;

                sqr_dis = (exceptChar.transform.position - t.transform.position).sqrMagnitude;

                //최대 반경 이내일 경우
                if (sqr_minRadius <= sqr_dis && sqr_dis <= sqr_maxRadius)
                {

                    //DebugWide.LogBlue(min_value + "__" + sqr_dis +"__"+  t.name); //chamto test

                    //기존 객체보다 더 가까운 경우
                    if (min_value > sqr_dis)
                    {
                        min_value = sqr_dis;
                        target = t;
                    }
                }

            }

            //if(null != target)
            //DebugWide.LogBlue(min_value + "__" + sqr_dis + "__" + target.name); //chamto test

            return target;
        }


        /// <summary>
        /// 전체 캐릭터가 특정캐릭터를 쳐다보게 설정한다
        /// </summary>
        /// <param name="target">Target.</param>
        public void LookAtTarget(Being target)
        {
            return;

            Vector3 dir = Vector3.zero;
            foreach (Being t in _beings.Values)
            {
                if (t == target) continue;


                if ((int)Behavior.eKind.Idle <= (int)t._behaviorKind && (int)t._behaviorKind <= (int)Behavior.eKind.Idle_Max)
                {
                    dir = target.transform.position - t.transform.position;
                    t.Idle_View(dir, true);

                    t._behaviorKind = Behavior.eKind.Idle_LookAt;
                }

            }

        }

        public void SetAll_Behavior(Behavior.eKind kind)
        {
            foreach (Being t in _beings.Values)
            {

                t._behaviorKind = kind;

            }
        }




        //____________________________________________
        //                  객체 생성 
        //____________________________________________

        public GameObject CreatePrefab(string prefabPath, Transform parent, string name)
        {
            const string root = "Warcraft/Prefab/";
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(root + prefabPath)) as GameObject;
            obj.transform.SetParent(parent, false);
            obj.transform.name = name;


            return obj;
        }



        public Being Create_Character(Transform parent, Being.eKind eKind, uint id, Vector3 pos)
        {

            GameObject obj = CreatePrefab(eKind.ToString(), parent, id.ToString("000") + "_" + eKind.ToString());
            Being cha = obj.AddComponent<Being>();
            obj.AddComponent<Movement>();
            obj.AddComponent<AI>();
            cha._id = id;
            cha._kind = eKind;
            cha.transform.localPosition = pos;
            //cha.Init_Create();

            _beings.Add(id,cha);
            _listTest.Add(cha); //chamto test

            if (Being.eKind.spearman == eKind)
            {
                Create_ShotSpear(obj.transform, 0);
                //Create_ShotSpear(obj.transform, 1);
                //Create_ShotSpear(obj.transform, 2);
            }

            return cha;
        }

        public GameObject Create_ShotSpear(Transform parent, int id)
        {
            GameObject obj = CreatePrefab("shot/spear", parent, id.ToString("000") + "_spear");
            obj.transform.parent = parent;
            obj.transform.localPosition = new Vector3(-0.15f, 0, 0.15f);

            return obj;
        }

        public void Create_StageInfo()
        {

            if (null == Single.unitRoot) return;

            uint id_sequence = 0;
            Vector3 pos = Vector3.zero;
            Create_Character(Single.unitRoot, Being.eKind.lothar, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.garona, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.footman, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.spearman, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.brigand, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.ogre, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.conjurer, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.slime, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.raider, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.grunt, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.knight, id_sequence++, pos);//.SetAIRunning(false);


            for (int i = 0; i < 1;i++)
            {
                Create_Character(Single.unitRoot, Being.eKind.skeleton, id_sequence++, pos);
            }

            Create_Character(Single.unitRoot, Being.eKind.daemon, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.waterElemental, id_sequence++, pos);
            Create_Character(Single.unitRoot, Being.eKind.fireElemental, id_sequence++, pos);

        }

    }
}


namespace HordeFight
{
    //========================================================
    //==================     캐릭터 정보(임시)     ==================
    //========================================================

    public partial class  Character : MonoBehaviour
    {

        //애니
        private Animator                    _animator = null;
        private AnimatorOverrideController  _overCtr = null;
        private SpriteRenderer              _sprRender = null;

        //이동
        private Movable _move     = null;
        private eDirection8 _eDir8 = eDirection8.down;

        //상태
        public eState   _eState = eState.None;

        //UI
        public int      _UIID_circle = -1;
        public int      _UIID_hp = -1;

        //고유정보
        public int _id = -1;
        public eKind _eKind = eKind.None;

        public float    _disPerSecond = 1f; //초당 이동거리 

        public ushort   _power = 1;
        public ushort   _hp_cur = 10;
        public ushort   _hp_max = 10;
        public float    _range_min = 0.15f;
        public float    _range_max = 0.15f;
        public bool     _death = false;
       

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

        public enum eState
        {
            None        = 0,

            Idle        = 10,
            Idle_Random = 11,
            Idle_LookAt = 12,
            Idle_Max    = 19,

            Move        = 20,
            Move_Max    = 29,

            Attack      = 30,
            Attack_Max  = 39,

            FallDown        = 40,
            FallDown_Max    = 49,

        }


        public void Init_Create()
        {
            _animator = GetComponentInChildren<Animator>();
            _sprRender = GetComponentInChildren<SpriteRenderer>();

            _move = GetComponent<Movable>();
            //Single.touchProcess.Attach_SendObject(this.gameObject);

            //오버라이드컨트롤러를 생성해서 추가하지 않고, 미리 생성된 것을 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
            _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _overCtr.name = "divide_character_"+ _id.ToString();
            _animator.runtimeAnimatorController = _overCtr;

        }

        //ref : https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
        private void Start()
        {
            _eState = eState.Idle_Random;
            //_animator.SetInteger("state", (int)eState.Idle);

            _UIID_circle = Single.lineControl.Create_Circle_AxisY(this.transform);
            Single.lineControl.SetActive(_UIID_circle, false);

            _UIID_hp = Single.lineControl.Create_LineHP_AxisY(this.transform);

        }



        private void Update()
        {
            if (true == _death) return;

            if (9 > _hp_cur)
            {
                //DebugWide.LogBlue("death  " + _eState);
                _death = true;
                _eState = eState.FallDown;
                FallDown();
            }

            Update_Shot();


            if (eState.Idle == _eState)
            {
                _animator.SetInteger("state", (int)eState.Idle);
            }
            else if (eState.Idle_Random == _eState)
            {
                _animator.SetInteger("state", (int)eState.Idle);
                Idle_Random();

            }
            else if (eState.Move == _eState)
            {
                _animator.SetInteger("state", (int)eState.Move);
            }
            else if (eState.Attack == _eState)
            {
                _animator.SetInteger("state", (int)eState.Attack);
            }
            else if (eState.FallDown == _eState)
            {
                _animator.SetInteger("state", (int)eState.FallDown);
            }



            //y축값이 작을수록 먼저 그려지게 한다. 캐릭터간의 실수값이 너무 작아서 100배 한후 소수점을 버린값을 사용함
            _sprRender.sortingOrder = -(int)(transform.position.z * 100f);
        }

        public float GetCollider_Radius()
        {

            return GetComponent<SphereCollider>().radius;
        }

        public void SetAIRunning(bool run)
        {
            this.GetComponent<AI>()._ai_running = run;
        }


        //____________________________________________
        //                  애니메이션  
        //____________________________________________

        //todo optimization : 애니메이션 찾는 방식 최적화 필요. 해쉬로 변경하기 
        public AnimationClip GetClip(string name)
        {
            AnimationClip animationClip = null;
            Single.resourceManager._aniClips.TryGetValue(name.GetHashCode(), out animationClip);


            return animationClip;
        }


        public void Switch_Ani(string aniKind, string aniName , eDirection8 dir)
        {
            
            _sprRender.flipX = false;
            string aniNameSum = "";
            switch(dir)
            {
                
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

            _overCtr[aniKind] = GetClip(aniNameSum);
        }


        private float __elapsedTime_1 = 0f;
        private float __randTime = 0f;
        public void Idle_Random()
        {
            if ((int)eState.Idle == _animator.GetInteger("state"))
            {
                __elapsedTime_1 += Time.deltaTime;


                if (__randTime < __elapsedTime_1)
                {
                    
                    //_eDir8 = (eDirection)Single.rand.Next(0, 8); //0~7

                    //근접 방향으로 랜덤하게 회전하게 한다 
                    int num = Misc.rand.Next(-1, 2); //-1 ~ 1
                    num += (int)_eDir8;
                    if (0 > num) num = 7;
                    if (7 < num) num = 0;
                    _eDir8 = (eDirection8)num;

                    Switch_Ani("base_idle", _eKind.ToString() + "_idle_", _eDir8);

                    __elapsedTime_1 = 0f;

                    //3~6초가 지났을 때 돌아감
                    __randTime = (float)Misc.rand.Next(3, 7); //3~6
                }

            }

        }

        public void Idle_View(Vector3 dir , bool forward )//, bool setState)
        {
            
            if (false == forward)
                dir *= -1f;

            _eDir8 = Misc.TransDirection8_AxisY(dir);
            //Switch_AniMove("base_move",_eKind.ToString()+"_attack_",_eDir8);
            Switch_Ani("base_idle", _eKind.ToString() + "_idle_", _eDir8);

            //if(true == setState)
                //_animator.SetInteger("state", (int)eState.Idle);
        }

        public void Move(Vector3 dir ,  float distance ,bool forward )//, bool setState)
		{
            //Vector3 dir = target - this.transform.position;
            //dir.Normalize();
            _move.Move_Forward(dir, distance, 1f);

            //전진이 아니라면 애니를 반대방향으로 바꾼다 (뒷걸음질 효과)
            if (false == forward)
                dir *= -1f;
            
            _eDir8 = Misc.TransDirection8_AxisY(dir);
            Switch_Ani("base_move", _eKind.ToString() + "_move_", _eDir8);

		}

        public void Attack(Vector3 dir)
        {
            _eDir8 = Misc.TransDirection8_AxisY(dir);
            Switch_Ani("base_attack",_eKind.ToString()+"_attack_",_eDir8);
        }


        bool __launch = false;
        GameObject __things = null;
        Vector3 __targetPos = Vector3.zero;
        Vector3 __launchPos = Vector3.zero;
        public void ThrowThings(Vector3 target)
        {
            //Vector3 dir = target - this.transform.position;
            //Attack(dir);


            if(null == __things)
            {
                __things = GameObject.Find("000_spear");
            }


            if (null != __things && false == __launch)
            {
                __targetPos = target;

                Vector3 angle = new Vector3(90f, 0, -120f);
                angle.y += (float)_eDir8 * -45f;
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
                    
                if(1f < __elapsedTime_2)
                {
                    __launch = false;
                    __things.SetActive(false);
                }
            }
        }


        public void FallDown()
        {
            switch (_eDir8)
            {
                case eDirection8.up:
                    { }
                    break;
                case eDirection8.down:
                    { }
                    break;
                default:
                    {
                        _eDir8 = eDirection8.up; //기본상태 지정 
                    }
                    break;
            }

            Switch_Ani("base_fallDown", _eKind.ToString() + "_fallDown_", _eDir8);
        }


        //____________________________________________
        //                  터치 이벤트  
        //____________________________________________

		private Vector3 _startPos = Vector3.zero;
        private void TouchBegan() 
        {
            //DebugWide.LogBlue("TouchBegan " + Single.touchProcess.GetTouchPos());

            _animator.speed = 1f;


            RaycastHit hit = Single.touchProcess.GetHit3D();
            _startPos = hit.point;
            _startPos.y = 0f;


            Single.lineControl.SetActive(_UIID_circle, true);

            //_hp_cur--;
            Single.lineControl.SetLineHP(_UIID_hp, (float)_hp_cur/(float)_hp_max);


            if(8 > _hp_cur)
            {
                //다시 살리기
                _animator.Play("idle 10");
                _death = false;
                _hp_cur = 10;
                _eState = eState.Idle;    
            }


        }

        private void TouchMoved()
        {
            //DebugWide.LogBlue("TouchMoved " + Single.touchProcess.GetTouchPos());

            RaycastHit hit = Single.touchProcess.GetHit3D();

            Vector3 dir = hit.point - this.transform.position;
            dir.y = 0;
            //DebugWide.LogBlue("TouchMoved " + dir);

            //if (eKind.spearman == _eKind)
            //{
            //    Character target = Single.objectManager.GetNearCharacter(this, 0.5f, 2f);

            //    ThrowThings(target.transform.position);

            //    Vector3 things_dir = target.transform.position - this.transform.position;

            //    _eState = eState.Attack;
            //    Attack(things_dir);

            //    _move.Move_Forward(dir, 1f, 1f); //chamto test
            //    //DebugWide.LogRed(target.name); //chamto test
            //}
            //else
            //{
            //    Character target = Single.objectManager.GetNearCharacter(this, 0, 0.2f);
            //    if (null != target)
            //    {
            //        _eState = eState.Attack;
            //        Attack(dir);

            //        _move.Move_Forward(dir, 1f, 1f); //chamto test
            //    }
            //    else
            //    {
            //        _eState = eState.Move;
            //        Move(dir, _disPerSecond, true);
            //    }
            //}
                
            //Single.objectManager.LookAtTarget(this);



        }

        private void TouchEnded() 
        {
            //DebugWide.LogBlue("TouchEnded " + Single.touchProcess.GetTouchPos());

            Switch_Ani("base_idle", _eKind.ToString()+"_idle_", _eDir8);
            //_animator.SetInteger("state", (int)eState.Idle);
            _animator.Play("idle 10");

            //_eState = eState.Idle_Random;
            //Single.objectManager.SetAll_Behavior(eState.Idle_Random);

            //Single.lineControl.SetActive(_UIID_circle, false);
        }
    }



    public class Movable : MonoBehaviour
    {

		private void Start()
		{
		}

		private void Update()
		{
		}


		public void Move_Forward(Vector3 dir , float distance , float speed)
        {
            //보간, 이동 처리
            //float delta = Interpolation.easeInOutBack(0f, 0.2f, accumulate / MAX_SECOND);
            this.transform.Translate(dir * Time.deltaTime * speed * distance);
        }

        //public Vector3 GetDirect(Vector3 dstPos)
        //{
        //    Vector3 dir = dstPos - this.transform.position;
        //    dir.Normalize();
        //    return dir;
        //}

        //객체의 전진 방향을 반환한다.
        //public Vector3 GetForwardDirect()
        //{
        //    return Quaternion.Euler(this.transform.localEulerAngles) * Vector3.forward;
        //}


        //내방향을 기준으로 목표위치가 어디쪽에 있는지 반환한다.  
        //public eDirection DirectionalInspection(Vector3 targetPos)
        //{

        //    Vector3 mainDir = GetForwardDirect();

        //    Vector3 targetTo = targetPos - this.transform.localPosition;

        //    //mainDir.Normalize();
        //    //targetTo.Normalize();

        //    Vector3 dir = Vector3.Cross(mainDir, targetTo);
        //    //dir.Normalize();
        //    //DebugWide.LogBlue("mainDir:" + mainDir + "  targetTo:" + targetTo + "   cross:" + dir.y);

        //    float angle = Vector3.Angle(mainDir, targetTo);
        //    angle = Mathf.Abs(angle);

        //    if (angle < 3f) return eDirection.CENTER; //사이각이 3도 보다 작다면 중앙으로 여긴다 

        //    //외적의 y축값이 음수는 왼쪽방향 , 양수는 오른쪽방향 
        //    if (dir.y < 0)
        //        return eDirection.LEFT;
        //    else if (dir.y > 0)
        //        return eDirection.RIGHT;

        //    return eDirection.CENTER;
        //}


        //회전할 각도 구하기
        //public float CalcRotationAngle(Vector3 targetDir)
        //{
        //    //atan2로 각도 구하는 것과 같음. -180 ~ 180 사이의 값을 반환
        //    return Vector3.SignedAngle(GetForwardDirect(), targetDir, Vector3.up);

        //}
    }
}


//========================================================
//==================       인공 지능      ==================
//========================================================
namespace HordeFight
{
    public class AI : MonoBehaviour
    {

        public bool _ai_running = false;

        private Character _target = null;


        public enum eState
        {
            Detect, //탐지
            Chase,  //추격
            Attack,  //공격
            Escape, //도망
            Roaming, //배회하기
        }
        private eState _state = eState.Roaming;

        private void Start()
        {
           
        }



        private void FixedUpdate()
        {
            if (false == _ai_running) return;

            this.StateUpdate();
        }


        public bool Situation_Is_AttackTarget()
        {
            return false;
        }

        public bool Situation_Is_AttackRange()
        {
            return false;
        }

        public void StateUpdate()
        {
            switch (_state)
            {
                case eState.Detect:
                    {
                        //공격대상이 맞으면 추격한다.
                        if (true == Situation_Is_AttackTarget())
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
                        //공격사정거리까지 이동했으면 공격한다. 
                        if (true == Situation_Is_AttackRange())
                        {
                            _state = eState.Attack;
                        }
                        //거리가 멀리 떨어져 있으면 다시 배회한다.
                        {
                            _state = eState.Roaming;
                        }

                    }
                    break;
                case eState.Attack:
                    {
                        //못이길것 같으면 도망간다.
                        {
                            _state = eState.Escape;
                        }

                        //적을 잡았으면 다시 배회한다.
                        {
                            _state = eState.Roaming;
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
                        //일정 거리 안에 적이 있으면 탐지한다.
                        //if (false)
                        //{
                        //    _state = eState.Detect;
                        //}


                        float MIN_DIS = 0f;
                        float MAX_DIS = 1f;
                        if(null == _target)
                        {
                            //_target = Single.objectManager.GetNearCharacter(this.GetComponent<Character>(),MIN_DIS, MAX_DIS);
                        }
                        else
                        {
                            
                            Vector3 dir = _target.transform.position - this.transform.position;

                            if (dir.sqrMagnitude > Mathf.Pow(MAX_DIS, 2))
                            {
                                _target = null;
                            }
                            else
                            {
                                //this.GetComponent<Movable>().Move_Forward(dir, 1f, 1f);
                                this.GetComponent<Character>().Move(dir, 1f,true);
                                this.GetComponent<Character>()._eState = Character.eState.Move;
                            }


                        }


                    }
                    break;
            }
        }

    }



}//end namespace


//가속도계 참고할것  :  https://docs.unity3d.com/kr/530/Manual/MobileInput.html
//마우스 시뮬레이션  :  https://docs.unity3d.com/kr/530/ScriptReference/Input.html   마우스 => 터치로 변환
//========================================================
//==================      터치  처리      ==================
//========================================================
namespace HordeFight
{

    public class TouchProcess : MonoBehaviour
    {

        private GameObject _TouchedObject = null;
        private List<GameObject> _sendList = new List<GameObject>();

        private Vector2 _prevTouchMovedPos = Vector3.zero;
        public Vector2 prevTouchMovedPos
        {
            get
            {
                return _prevTouchMovedPos;
            }
        }



        void Awake()
        {

            Input.simulateMouseWithTouches = false;
            Input.multiTouchEnabled = false;

        }

        // Use this for initialization
        void Start()
        {


        }


        //void Update()
        void FixedUpdate()
        {
            //화면상 ui를 터치했을 경우 터치이벤트를 보내지 않는다 
            if (null != EventSystem.current && null != EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SendTouchEvent_Device_Target();
                SendTouchEvent_Device_NonTarget();
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                SendMouseEvent_Editor_Target();
                SendMouseEvent_Editor_NonTarget();
            }
        }

        //==========================================
        //                 보조  함수
        //==========================================

        public void Attach_SendObject(GameObject obj)
        {
            _sendList.Add(obj);
        }

        public void Detach_SendObject(GameObject obj)
        {
            _sendList.Remove(obj);
        }

        public void DetachAll()
        {
            _sendList.Clear();
        }


        public Vector2 GetTouchPos()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Input.GetTouch(0).position;
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                return Input.mousePosition;
            }

            return Vector2.zero;
        }

        public bool GetMouseButtonMove(int button)
        {
            if (Input.GetMouseButton(button) && Input.GetMouseButtonDown(button) == false)
            {
                return true;
            }

            return false;
        }

        //충돌체가 2d 면 Physics2D 함수로만 찾아낼 수 있다.  2d객체에 3d충돌체를 넣으면 Raycast(3D) 함수로 찾아낼 수 있다. 
        public RaycastHit2D GetHit2D()
        {
            Ray ray = Camera.main.ScreenPointToRay(this.GetTouchPos());

            return Physics2D.Raycast(ray.origin, ray.direction);
        }

        public RaycastHit GetHit3D()
        {
            Ray ray = Camera.main.ScreenPointToRay(this.GetTouchPos());

            RaycastHit hit3D = default(RaycastHit);
            Physics.Raycast(ray, out hit3D, Mathf.Infinity);

            return hit3D;
        }

        //==========================================
        //                 이벤트  함수
        //==========================================

        private void SendTouchEvent_Device_NonTarget()
        {
            foreach (GameObject o in _sendList)
            {
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {

                        o.SendMessage("TouchBegan", 0, SendMessageOptions.DontRequireReceiver);

                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                    {

                        o.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {

                        o.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);

                    }
                    else
                    {
                        DebugWide.LogError("Update : Exception Input Event : " + Input.GetTouch(0).phase);
                    }
                }
            }

        }

        private bool __touchBegan = false;
        private void SendMouseEvent_Editor_NonTarget()
        {
            foreach (GameObject o in _sendList)
            {
                //=================================
                //    mouse Down
                //=================================
                if (Input.GetMouseButtonDown(0))
                {
                    //DebugWide.LogBlue("SendMouseEvent_Editor_NonTarget : TouchPhase.Began"); //chamto test
                    if (false == __touchBegan)
                    {
                        o.SendMessage("TouchBegan", 0, SendMessageOptions.DontRequireReceiver);

                        __touchBegan = true;

                    }
                }

                //=================================
                //    mouse Up
                //=================================
                if (Input.GetMouseButtonUp(0))
                {

                    if (true == __touchBegan)
                    {
                        __touchBegan = false;

                        o.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
                    }

                }


                //=================================
                //    mouse Move
                //=================================
                if (this.GetMouseButtonMove(0))
                {

                    //=================================
                    //     mouse Drag 
                    //=================================
                    if (true == __touchBegan)
                    {

                        o.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

                    }//if
                }//if
            }

        }

        private void SendTouchEvent_Device_Target()
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    //DebugWide.LogError("Update : TouchPhase.Began"); //chamto test
                    _prevTouchMovedPos = this.GetTouchPos();
                    _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.GetTouch(0).position);
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    //DebugWide.LogError("Update : TouchPhase.Moved"); //chamto test

                    if (null != _TouchedObject)
                        _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

                    _prevTouchMovedPos = this.GetTouchPos();

                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    //DebugWide.LogError("Update : TouchPhase.Ended"); //chamto test
                    if (null != _TouchedObject)
                        _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
                    _TouchedObject = null;
                }
                else
                {
                    DebugWide.LogError("Update : Exception Input Event : " + Input.GetTouch(0).phase);
                }
            }
        }


        private bool f_isEditorDraging = false;
        private void SendMouseEvent_Editor_Target()
        {

            //=================================
            //    mouse Down
            //=================================
            //Debug.Log("mousedown:" +Input.GetMouseButtonDown(0)+ "  mouseup:" + Input.GetMouseButtonUp(0) + " state:" +Input.GetMouseButton(0)); //chamto test
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log ("______________ MouseBottonDown ______________" + m_TouchedObject); //chamto test
                if (false == f_isEditorDraging)
                {

                    _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.mousePosition);
                    if (null != _TouchedObject)
                        f_isEditorDraging = true;
                }

            }

            //=================================
            //    mouse Up
            //=================================
            if (Input.GetMouseButtonUp(0))
            {

                //Debug.Log ("______________ MouseButtonUp ______________" + m_TouchedObject); //chamto test
                f_isEditorDraging = false;

                if (null != _TouchedObject)
                {
                    _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
                }

                _TouchedObject = null;

            }


            //=================================
            //    mouse Move
            //=================================
            if (this.GetMouseButtonMove(0))
            {

                //=================================
                //     mouse Drag 
                //=================================
                if (f_isEditorDraging)
                {
                    //Debug.Log ("______________ MouseMoved ______________" + m_TouchedObject); //chamto test

                    if (null != _TouchedObject)
                        _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);


                }//if
            }//if
        }


        private GameObject SendMessage_TouchObject(string callbackMethod, Vector3 touchPos)
        {

            Ray ray = Camera.main.ScreenPointToRay(touchPos);

            //Debug.Log ("  -- currentSelectedGameObject : " + EventSystem.current.currentSelectedGameObject); //chamto test

            //2. game input event test
            RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);
            if (null != hit2D.collider)
            {
                //DebugWide.Log(hit2D.transform.gameObject.name); //chamto test
                hit2D.transform.gameObject.SendMessage(callbackMethod, 0, SendMessageOptions.DontRequireReceiver);

                return hit2D.transform.gameObject;
            }



            RaycastHit hit3D = default(RaycastHit);
            if (true == Physics.Raycast(ray, out hit3D, Mathf.Infinity))
            {
                //DebugWide.LogBlue("SendMessage_TouchObject 2");
                hit3D.transform.gameObject.SendMessage(callbackMethod, 0, SendMessageOptions.DontRequireReceiver);

                return hit3D.transform.gameObject;
            }


            return null;
        }

        ////콜백함수 원형 : 지정 객체 아래로 터치이벤트를 보낸다 
        //private void TouchBegan() { }
        //private void TouchMoved() { }
        //private void TouchEnded() { }

        ////콜백함수 원형 : 지정 객체에 터치이벤트를 보낸다 
        //private void TouchBegan() { }
        //private void TouchMoved() { }
        //private void TouchEnded() { }


    }

}//end namespace

