using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;

using Utility;

namespace HordeFight
{
    public class HordeFight_Main : MonoBehaviour
    {
        
        // Use this for initialization
        void Start()
        {
            ResolutionController.CalcViewportRect(SingleO.canvasRoot, SingleO.mainCamera); //화면크기조정
            SingleO.hierarchy.Init(); //계층도 읽어들이기 
            SingleO.resourceManager.Init(); //스프라이트 로드 

            gameObject.AddComponent<LineControl>();

            gameObject.AddComponent<GridManager>();
            gameObject.AddComponent<CampManager>();
            gameObject.AddComponent<ObjectManager>();
            gameObject.AddComponent<PathFinder>();

            gameObject.AddComponent<UI_Main>();
            gameObject.AddComponent<DebugViewer>();

            gameObject.AddComponent<TouchEvent>();
            gameObject.AddComponent<TouchControl>();

            //===================

            SingleO.campManager.Load_CampPlacement(Camp.eKind.Blue);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.White);
            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Blue, Camp.eKind.White);
            //SingleO.objectManager.Create_Characters(); //여러 캐릭터들 테스트용
            SingleO.objectManager.Create_ChampCamp();


        }

        // Update is called once per frame
        //void Update()
        void FixedUpdate()
        {

        }

        //void OnGUI()
        //{
        //    if (GUI.Button(new Rect(10, 10, 200, 100), new GUIContent("Refresh Timemap Struct")))
        //    {
        //        RuleTile ruleTile =  SingleO.gridManager.GetTileMap_Struct().GetTile<RuleTile>(new Vector3Int(0, 0, 0));

        //        SingleO.gridManager.GetTileMap_Struct().RefreshAllTiles();
        //        DebugWide.LogBlue("TileMap_Struct RefreshAllTiles");
        //    }
        //}


    }

}

//========================================================
//==================      전역  객체      ==================
//========================================================
namespace HordeFight
{
    public static class SingleO
    {
        
        public static TouchEvent touchEvent
        {
            get
            {
                return CSingletonMono<TouchEvent>.Instance;
            }
        }

        public static LineControl lineControl
        {
            get
            {
                return CSingletonMono<LineControl>.Instance;
            }
        }

        public static CampManager campManager
        {
            get
            {
                return CSingletonMono<CampManager>.Instance;
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

        public static PathFinder pathFinder
        {
            get
            {
                return CSingletonMono<PathFinder>.Instance;
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

        public static HierarchyPreLoader hierarchy
        {
            get
            {
                return CSingleton<HierarchyPreLoader>.Instance;
            }
        }

        public static WideCoroutine coroutine
        {
            get
            {
                return CSingleton<WideCoroutine>.Instance;
            }
        }

        public static HashToStringMap hashMap
        {
            get
            {
                return CSingleton<HashToStringMap>.Instance;
            }
        }

        public static DebugViewer debugViewer
        {
            get
            {
                return CSingletonMono<DebugViewer>.Instance;
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
        public RuntimeAnimatorController _base_Animator = null;


        //==================== Get / Set ====================


        //==================== <Method> ====================

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

            _base_Animator = Resources.Load<RuntimeAnimatorController>("Warcraft/Animation/base_Animator");
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

		//private void Update()
        void FixedUpdate()
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
            render.transform.localPosition = Vector3.zero;


            render.startWidth = 0.02f;
            render.endWidth = 0.02f;
            render.startColor = Color.red;
            render.endColor = Color.red;

            _list.Add(_sequenceId, info); //추가

            Vector3 pos = Vector3.zero;
            pos.x = -0.05f; pos.z = -0.15f;
            render.SetPosition(0, pos);
            pos.x += 0.1f;
            render.SetPosition(1, pos);

            return _sequenceId;
        }

        public int Create_Circle_AxisY(Transform parent, float radius, Color color)
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
            render.transform.parent = parent;//부모객체 지정
            render.sortingOrder = -10; //먼저그려지게 한다.
            render.positionCount = 20;
            render.loop = true; //처음과 끝을 연결한다 .
            render.transform.localPosition = Vector3.zero;

            color.a = 0.4f; //흐리게 한다
            render.startWidth = 0.01f;
            render.endWidth = 0.01f;
            render.startColor = color;//Color.green;
            render.endColor = color;//Color.green;

            _list.Add(_sequenceId, info); //추가

            //info.Update_Circle(); //값설정
            float deg = 360f / render.positionCount;
            //float radius = render.transform.parent.GetComponent<SphereCollider>().radius;
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

        public void SetScale(int id, float scale)
        {
            //Vector3 s = _list[id].render.transform.localScale;
            _list[id].render.transform.localScale = Vector3.one * scale; 
        }

        public void SetCircle_Radius(int id, float radius)
        {
            
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
//==================     디버그      ==================
//========================================================

namespace HordeFight
{
    public class DebugViewer : MonoBehaviour
    {
        private Tilemap _tilemap = null;
        private Dictionary<Vector3Int, Color> _colorMap = new Dictionary<Vector3Int, Color>();

		private void Start()
		{
            GameObject game = GameObject.Find("Tilemap_Debug");
            if(null != game)
            {
                _tilemap = game.GetComponent<Tilemap>();    
            }

		}

		public void ResetColor()
		{
            foreach(Vector3Int i3 in _colorMap.Keys)
            {
                _tilemap.SetColor(i3, Color.white);
            }
		}
        public void SetColor(Vector3 pos , Color c)
        {
            Vector3Int i3 = _tilemap.WorldToCell(pos);
            _tilemap.SetTileFlags(i3, UnityEngine.Tilemaps.TileFlags.None);
            _tilemap.SetColor(i3, c);

            if(!_colorMap.Keys.Contains(i3))
                _colorMap.Add(i3, c);
        }


        private Vector3 _start = Vector3.zero;
        private Vector3 _end = Vector3.right;
        public void DrawLine(Vector3 start, Vector3 end)
        {
            _start = start;
            _end = end;
        }

        public void UpdateDraw_IndexesNxN()
        {
            Vector3Int prev = Vector3Int.zero;
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(7, Vector3.up))
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(7, Vector3.back))

            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter_Tornado(7, Vector3.up))
            foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter_Tornado(7, Vector3.back))

            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter(7, Vector3.up))
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter(7, Vector3.back))
            {
                //DebugWide.LogBlue(v);
                Debug.DrawLine(prev, cur);
                prev = cur;
            }
        }

        public void UpdateDraw_StructTileDir()
        {
            Color ccc = Color.white;
            foreach (StructTile t in SingleO.gridManager._structTileList.Values)
            {
                if (eDirection8.none == t._dir) continue;

                //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
                //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z

                ccc = Color.white;
                if(StructTile.DiagonalFixing == t._id)
                {
                    ccc = Color.red;
                }

                Vector3 end = t._v3Center + Misc.GetDir8Normal_AxisY(t._dir) * 0.12f;

                Debug.DrawLine(t._v3Center, end, ccc);

                //Vector3 crossL = t._v3Center;
                //crossL.x += -0.08f;
                //crossL.z += 0.08f;
                //Vector3 crossR = t._v3Center;
                //crossR.x += 0.08f;
                //crossR.z += -0.08f;
                //Debug.DrawLine(crossL, crossR, ccc);
            }
        }

        public void Update_DrawEdges( bool debugLog)
        {
            PathFinder finder = SingleO.pathFinder;
            GridManager grid = SingleO.gridManager;
            Vector3Int tileBlock_size = finder.TILE_BLOCK_SIZE;

            int from_1d = 0;
            Vector3Int to_2d;
            Vector3 from_3d, to_3d;
            NavGraphNode from_nav, to_nav;
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tileBlock_size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int from_2d in boundsInt.allPositionsWithin)
            {
                from_1d = grid.ToPosition1D(from_2d, boundsInt.size.x);
                from_3d = grid.ToPosition3D(from_2d, Vector3.up);

                SparseGraph.EdgeList list = finder._graph.GetEdges(from_1d);
                foreach (GraphEdge e in list)
                {
                    to_2d = grid.ToPosition2D(e.To(), boundsInt.size.x);
                    to_3d = grid.ToPosition3D(to_2d, Vector3.up);

                    from_nav = finder._graph.GetNode(e.From()) as NavGraphNode;
                    to_nav = finder._graph.GetNode(e.To()) as NavGraphNode;
                    Debug.DrawLine(from_nav.Pos(), to_nav.Pos(), Color.green);

                    //chamto test
                    if (true == debugLog)
                    {
                        DebugWide.LogBlue(e + "  " + from_2d + "   " + to_2d);
                    }

                }
            }
        }


        void FixedUpdate()
		{
            //Debug.DrawLine(_start, _end);

            //UpdateDraw_StructTileDir();

            //UpdateDraw_IndexesNxN();

            //Update_DrawEdges(false);

		}
	}
}


//========================================================
//==================     그리드 관리기     ==================
//========================================================

namespace HordeFight
{

    //한셀에 몇명의 캐릭터가 있는지 나타낸다 
    public class CellInfo : LinkedList<Being>
    {
        
        public Vector3Int _index = default(Vector3Int);
    }

    //장애물 정보 
    public class StructTile
    {
        public const int DiagonalFixing = 7; //대각고정 예약어

        public int _id = 0;
        public eDirection8 _dir = eDirection8.none;
        public Vector3 _v3Center = Vector3.zero;    //타일의 중앙 월드위치
    }


    public class GridManager : MonoBehaviour
    {
        public const int NxN_MIN = 3;   //그리드 범위 최소크기
        public const int NxN_MAX = 11;  //그리드 범위 최대크기
        public const float GridCell_Size = 0.16f;
        public const float GridCell_HalfSize = GridCell_Size * 0.5f;


        private Grid _grid = null;
        private Tilemap _tilemap_struct = null;
        public Dictionary<Vector3Int,CellInfo> _cellList = new Dictionary<Vector3Int,CellInfo>();
        public Dictionary<Vector3Int, StructTile> _structTileList = new Dictionary<Vector3Int, StructTile>();

        //중심이 (0,0)인 nxn 그리드 인덱스 값을 미리 구해놓는다
        public Dictionary<uint, Vector3Int[]> _indexesNxN = new Dictionary<uint, Vector3Int[]>();


        public Grid grid
        {
            get { return _grid; }
        }
        public float cellSize_x
        {
            get
            {
                return (_grid.cellSize.x * _grid.transform.localScale.x);
            }
        }
        public float cellSize_z
        {
            get
            {
                return (_grid.cellSize.y * _grid.transform.localScale.z);
            }
        }

        public Tilemap GetTileMap_Struct()
        {
            return _tilemap_struct;
        }

		private void Start()
		{
            _grid = GameObject.Find("0_grid").GetComponent<Grid>();
            GameObject o = GameObject.Find("Tilemap_Structure");
            if(null != o)
            {
                _tilemap_struct = o.GetComponent<Tilemap>();    
            }

            //타일맵 좌표계 x-y에 해당하는 축값 back로 구한다 
            _indexesNxN[3] = CreateIndexesNxN_SquareCenter_Tornado(3, Vector3.back);
            _indexesNxN[5] = CreateIndexesNxN_SquareCenter_Tornado(5, Vector3.back);
            _indexesNxN[7] = CreateIndexesNxN_SquareCenter_Tornado(7, Vector3.back);
            _indexesNxN[9] = CreateIndexesNxN_SquareCenter_Tornado(9, Vector3.back);
            _indexesNxN[11] = CreateIndexesNxN_SquareCenter_Tornado(11, Vector3.back); //화면 세로길이를 벗어나지 않는 그리드 최소값

            this.LoadTilemap_Struct();

		}

		//private void Update()
        void FixedUpdate()
		{
			
		}

        //ref : https://gamedev.stackexchange.com/questions/150917/how-to-get-all-tiles-from-a-tilemap
        public void LoadTilemap_Struct()
        {
            if (null == _tilemap_struct) return;

            SingleO.gridManager.GetTileMap_Struct().RefreshAllTiles();
            StructTile structTile = null;
            RuleTile.TilingRule ruleInfo = null;
            int intId = 0;
            foreach (Vector3Int vint in _tilemap_struct.cellBounds.allPositionsWithin)
            {
                RuleTile ruleTile = _tilemap_struct.GetTile(vint) as RuleTile; //룰타일 종류와 상관없이 다 가져온다. 
                if (null == ruleTile) continue;

                ruleInfo = ruleTile._tileDataMap.GetTilingRule(vint);
                if (null == ruleInfo || false == int.TryParse(ruleInfo.m_ID, out intId)) 
                    intId = 0;

                structTile = new StructTile();
                structTile._id = intId;
                structTile._v3Center = _tilemap_struct.CellToWorld(vint);
                structTile._v3Center.x += GridCell_HalfSize;
                structTile._v3Center.z += GridCell_HalfSize;
                //structTile._v3Center = this.ToPosition_Center(vint, Vector3.up); //grid 함수와 호환안됨 
                structTile._dir = ruleTile._tileDataMap.GetDirection8(vint);
                _structTileList.Add(vint, structTile);

            }

            DebugWide.LogBlue("LoadTile : " + _structTileList.Count + "  -  TileMap_Struct RefreshAllTiles");
        }


        //원 반지름 길이를 포함하는 그리드범위 구하기
        public uint GetNxNIncluded_CircleRadius(float maxRadius)
        {
            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint nxn = (uint)((maxRadius * 2) / this.cellSize_x); //소수점 버림을 한다 
            if (0 == nxn % 2) nxn -= 1; //짝수라면 홀수로 변환한다
            if (nxn > GridManager.NxN_MAX) nxn = GridManager.NxN_MAX;
            if (nxn < GridManager.NxN_MIN) nxn = GridManager.NxN_MIN;

            return nxn;
        }

        /// <summary>
        /// 정사각형 모양의 중심이 0인 인덱스 배열을 만든다
        /// </summary>
        /// <param name="widthCenter">홀수만 가능 , 가운데 가로길이a</param>
        /// <param name="axis">Y축 , -Z축 만 가능</param>
        public Vector3Int[] CreateIndexesNxN_SquareCenter(ushort widthCenter , Vector3 axis)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == widthCenter % 2) return null;

            Vector3Int[] indexes = new Vector3Int[widthCenter * widthCenter];

            int count = 0;           
            Vector3Int index = Vector3Int.zero;
            int startIdx = (widthCenter - 1) / 2; //중심좌표를 (0,0)으로 만들기 위함
            for (int i = -startIdx; i < widthCenter - startIdx; i++)
            {
                //temp1 = "";
                for (int j = -startIdx; j < widthCenter - startIdx; j++)
                {
                    //temp1 += "(" + i + ", "+ j +")  "; 

                    if (Vector3.up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        index.x = j;
                        index.y = 0;
                        index.z = i;    
                    }

                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        index.x = j;
                        index.y = i;
                        index.z = 0;    
                    }

                    indexes[count++] = index;
                }
                //DebugWide.LogBlue(temp1);
            }
            return indexes;

        }

        //중심좌표로부터 가까운 순으로 배열이 배치된다
        public Vector3Int[] CreateIndexesNxN_SquareCenter_Tornado(ushort widthCenter, Vector3 axis)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == widthCenter % 2) return null;

            int Tornado_Num = 1; 
            int Tornado_Count = 0;
            Vector3 dir = Vector3.right;
            Vector3 prevMax = Vector3.zero;
            Vector3Int cur = Vector3Int.zero;
            Vector3Int[] indexes = new Vector3Int[widthCenter * widthCenter];
            for (int cnt = 0; cnt < indexes.Length; cnt++)
            {
                indexes[cnt] = cur;

                //해당방향의 최대값에 도달하면 90도 회전 
                if(prevMax + dir * Tornado_Num == cur)
                {
                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Num + Tornado_Count%2); //1 1 2 2 3 3 4 4 5 5 .... 이렇게 증가하는 수열값임 
                    Tornado_Count++;

                    //반시계방향으로 회전하도록 한다 
                    if (Vector3.up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        dir = Quaternion.Euler(0, -90f, 0) * dir;
                        //DebugWide.LogBlue(dir + "  " + Tornado_Num); //chamto test
                    }
                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, 90f) * dir;
                    }
                }

                //지정된 방향값으로 한칸씩 증가한다
                cur.x += (int)Mathf.Round(dir.x); //반올림 한다 : 대각선 방향 때문 
                cur.y += (int)Mathf.Round(dir.y);
                cur.z += (int)Mathf.Round(dir.z);
            }

            return indexes;

        }

        //마름모꼴
        public Vector3Int[] CreateIndexesNxN_RhombusCenter(ushort widthCenter, Vector3 axis)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == widthCenter % 2) return null;

            //마름모꼴의 전체 셀 개수구하기
            //마름모 반지름의 크기에 따라 1 4 8 12 16 ... 으로 증가한다 
            int Length = 1;
            for (int i = 1; i <= widthCenter / 2; i++)
            {
                Length += i * 4;
            }

            int Tornado_Num = 1;
            int Tornado_Count = 0;
            float angle = 0f;
            Vector3 dir = Vector3.zero;
            Vector3 prevMax = Vector3.zero;
            Vector3Int curMax = Vector3Int.zero;
            Vector3Int cur = Vector3Int.zero;
            Vector3Int[] indexes = new Vector3Int[Length];
            for (int cnt = 0; cnt < indexes.Length; cnt++)
            {
                indexes[cnt] = cur;


                //지정된 방향값으로 한칸씩 증가한다
                cur.x += (int)Mathf.Round(dir.x); //반올림 한다 : 대각선 방향 때문 
                cur.y += (int)Mathf.Round(dir.y);
                cur.z += (int)Mathf.Round(dir.z);

                //DebugWide.LogBlue("cur:  " + cur + "   perv" + prevMax + "   if" + (prevMax + dir * Tornado_Num));
                //해당방향의 최대값에 도달하면 90도 회전 
                curMax.x = (int)(prevMax.x + Mathf.Round(dir.x) * Tornado_Num);
                curMax.y = (int)(prevMax.y + Mathf.Round(dir.y) * Tornado_Num);
                curMax.z = (int)(prevMax.z + Mathf.Round(dir.z) * Tornado_Num);

                if (curMax == cur)
                {
                    //DebugWide.LogBlue(curMax + "  " + Tornado_Count % 4 + "  :" + Tornado_Count);
                    if (0 == Tornado_Count % 4)
                    {
                        //좌상단으로 방향 전환
                        angle = -135f;

                        //오른쪽으로 한칸더 간다
                        dir = Vector3.right;
                        cur.x += (int)Mathf.Round(dir.x);
                        cur.y += (int)Mathf.Round(dir.y);
                        cur.z += (int)Mathf.Round(dir.z);
                    }
                    else
                    {
                        angle = -90f;
                    }

                    if (Vector3.up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        dir = Quaternion.Euler(0, angle, 0) * dir;
                    }
                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, -angle) * dir;
                    }

                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Count/4)+1; //1 1 1 1 2 2 2 2 3 3 3 3 .... 이렇게 증가하는 수열값임 

                    Tornado_Count++;
                }

            }

            return indexes;

        }


        public bool HasStructTile_InPostion2D(Vector3Int tileInt)
        {
            StructTile structTile = null;
            if (true == _structTileList.TryGetValue(tileInt, out structTile))
            {
                return true;
            }

            return false;
        }

        public bool HasStructTile(Vector3 tilePos)
        {
            Vector3Int tileInt = _tilemap_struct.WorldToCell(tilePos);

            StructTile structTile = null;
            if (true == _structTileList.TryGetValue(tileInt, out structTile))
            {
                return true;
            }

            return false;
        }
        public bool HasStructTile(Vector3 tilePos, out StructTile structTile)
        {
            structTile = null;

            if (null == _tilemap_struct) return false;

            //x-z 평면상에 타일맵을 놓아도 내부적으로는 x-y 평면으로 처리된다
            Vector3Int tileInt = _tilemap_struct.WorldToCell(tilePos);
            if(true == _structTileList.TryGetValue(tileInt, out structTile))
            {
                return true;
            }

            return false;
        }

        public CellInfo GetCellInfo(Vector3Int cellIndex)
        {
            CellInfo cell = null;
            _cellList.TryGetValue(cellIndex, out cell);

            return cell;
        }



        //todo : 자료구조를 복사하는데 부하가 있기때문에, 자료구조를 직접 순회하면서 처리하는 방향으로 해봐야겠다
        public CellInfo GetCellInfo_NxN(Vector3Int center , ushort NCount_odd)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == NCount_odd % 2) return null;

            CellInfo cellList = new CellInfo();
            cellList._index = center;

            //DebugWide.LogBlue("================" + NCount_odd + " ================ ");
            //string temp1 = "";

            CellInfo dst = null;
            Vector3Int index = Vector3Int.zero;
            int startIdx = (NCount_odd - 1) / 2; //중심좌표를 (0,0)으로 만들기 위함
            for (int i = -startIdx; i < NCount_odd - startIdx; i++)
            {
                //temp1 = "";
                for (int j = -startIdx; j < NCount_odd - startIdx; j++)
                {
                    //temp1 += "(" + i + ", "+ j +")  "; 
                    index.x = i + center.x; 
                    index.y = j + center.y;
                    dst = this.GetCellInfo(index);

                    if(null != dst && 0 != dst.Count)
                    {
                        //DebugWide.LogBlue(dst.Count + "  " + i + "," + j);

                        foreach(var v in dst)
                        {
                           cellList.AddLast(v);    
                        }

                    }
                }
                //DebugWide.LogBlue(temp1);
            }

            return cellList;
        }


        public void AddCellInfo_Being(Vector3Int cellIndex, Being being)
        {
            CellInfo cell = null;
            if (false == _cellList.TryGetValue(cellIndex, out cell))
            {
                cell = new CellInfo();
                cell._index = cellIndex;
                _cellList.Add(cellIndex, cell);
            }
            _cellList[cellIndex].AddLast(being);
        }

        public void RemoveCellInfo_Being(Vector3Int cellIndex, Being being)
        {
            if (null == being) return;

            CellInfo cell = null;
            if (true == _cellList.TryGetValue(cellIndex, out cell))
            {
                cell.Remove(being);
            }

        }


        public Vector3Int ToPosition2D(Vector3 pos3d , Vector3 axis)
        {

            Vector3Int cellIndex = Vector3Int.zero;
            Vector3 cellSize = Vector3.zero;

            if(Vector3.up == axis)
            {
                //그리드 자체에 스케일을 적용시킨 경우가 있으므로 스케일값을 적용한다. 
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x) ;
                cellSize.y = (_grid.cellSize.y * _grid.transform.localScale.z) ;


                if(0 <= pos3d.x)
                {
                    //양수일때는 소수점을 버린다. 
                    cellIndex.x = (int)(pos3d.x / cellSize.x);
                }
                else
                {
                    //음수일때는 올림을 한다. 
                    cellIndex.x = (int)((pos3d.x / cellSize.x) - 0.9f);
                }


                if (0 <= pos3d.z)
                { 
                    cellIndex.y = (int)(pos3d.z / cellSize.y);
                }
                else
                { 
                    cellIndex.y = (int)((pos3d.z / cellSize.y) - 0.9f);
                }

            }

            return cellIndex;
        }

        public Vector3 ToCenter3D_FromPosition3D(Vector3 pos3d, Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            Vector3Int cellPos = this.ToPosition2D(pos3d, axis);

            return this.ToPosition3D_Center(cellPos, axis);
        }

        //grid 와 호환 안되는 함수 
        public Vector3 ToPosition3D_Center(Vector3Int pos2d , Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            if (Vector3.up == axis)
            {
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x);
                cellSize.y = (_grid.cellSize.y * _grid.transform.localScale.z);

                pos.x = (float)pos2d.x * cellSize.x;
                pos.z = (float)pos2d.y * cellSize.y;

                //셀의 중간에 위치하도록 한다
                pos.x += cellSize.x * 0.5f;
                pos.z += cellSize.y * 0.5f;
            }

            return pos;
        }

        public Vector3 ToPosition3D(Vector3Int pos2d, Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            if (Vector3.up == axis)
            {
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x);
                cellSize.y = (_grid.cellSize.y * _grid.transform.localScale.z);

                pos.x = (float)pos2d.x * cellSize.x;
                pos.z = (float)pos2d.y * cellSize.y;

            }

            return pos;
        }


        public int ToPosition1D(Vector3Int pos2d , int tileBlock_width_size)
        {
            Assert.IsFalse(0 > pos2d.x || 0 > pos2d.y, "음수좌표값은 1차원값으로 변환 할 수 없다");
            if (0 > pos2d.x || 0 > pos2d.y) return -1;
            
            return (pos2d.x + pos2d.y * tileBlock_width_size); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Vector3Int ToPosition2D(int pos1d , int tileBlock_width_size)
        {
            Assert.IsFalse(0 > pos1d, "음수좌표값은 2차원값으로 변환 할 수 없다");
            if (0 > pos1d) return Vector3Int.zero;

            Vector3Int v3int = Vector3Int.zero;

            v3int.x = pos1d % tileBlock_width_size;
            v3int.y = pos1d / tileBlock_width_size;

            return v3int;
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

        private uint _id_sequence = 0;

        public Dictionary<uint, Being> _beings = new Dictionary<uint, Being>();
        public List<Being> _linearSearch_list = new List<Being>(); //충돌처리시 선형검색 속도를 높이기 위해 사전정보와 동일한 객체를 리스트에 넣음 

        private void Start()
        {
            //===============
            //해쉬와 문자열 설정
            //===============

            SingleO.hashMap.Add(Animator.StringToHash("idle"),"idle");
            SingleO.hashMap.Add(Animator.StringToHash("move"),"move");
            SingleO.hashMap.Add(Animator.StringToHash("block"),"block");
            SingleO.hashMap.Add(Animator.StringToHash("attack"),"attack");
            SingleO.hashMap.Add(Animator.StringToHash("fallDown"),"fallDown");
            SingleO.hashMap.Add(Animator.StringToHash("idle -> attack"),"idle -> attack");
            SingleO.hashMap.Add(Animator.StringToHash("attack -> idle"),"attack -> idle");

        }


        //객체간의 충돌검사 최적화를 위한 충돌가능객체군 미리 조직하기 
        //private void Update()
        private void FixedUpdate()
        {
            //UpdateCollision();

            //UpdateCollision_UseDictElementAt(); //obj100 : fps10
            //UpdateCollision_UseDictForeach(); //obj100 : fps60

            //UpdateCollision_UseList(); //obj100 : fps80 , obj200 : fps40 , obj400 : fps15
            //UpdateCollision_UseGrid3x3(); //obj100 : fps65 , obj200 : fps40
            UpdateCollision_UseDirectGrid3x3(); //obj100 : fps70 , obj200 : fps45 , obj400 : fps20
        }

        //____________________________________________
        //              선분을 이용한 CCD   
        //____________________________________________
        public Vector3[] LineSegmentTest(Vector3 origin, Vector3 last)
        {
            LineSegment3 lineSeg = LineSegment3.zero;
            lineSeg.origin = origin;
            //lineSeg.direction = dir;
            lineSeg.last = last;

            LinkedList<Vector3> cellList = new LinkedList<Vector3>();
            float CELL_HARF_SIZE = SingleO.gridManager.cellSize_x * 0.5f;
            float CELL_SQUARED_RADIUS = Mathf.Pow(CELL_HARF_SIZE, 2f);
            float sqrDis = 0f;
            float t_c = 0;

            //기준셀값을 더해준다. 기준셀은 그리드값 변환된 값이이어야 한다 
            Vector3Int originToGridInt = SingleO.gridManager.ToPosition2D(origin, Vector3.up);
            Vector3 originToPos = SingleO.gridManager.ToPosition3D(originToGridInt, Vector3.up);
            Vector3 worldCellCenterPos = Vector3.zero;
            foreach (Vector3Int cellLBPos in SingleO.gridManager._indexesNxN[7])
            {
                //셀의 중심좌표로 변환 
                worldCellCenterPos = SingleO.gridManager.ToPosition3D_Center(cellLBPos, Vector3.up);
                worldCellCenterPos += originToPos;


                //시작위치셀을 포함하거나 뺄때가 있다. 사용하지 않느다 
                //선분방향과 반대방향인 셀들을 걸러낸다 , (0,0)원점 즉 출발점의 셀은 제외한다 
                if(0 == cellLBPos.sqrMagnitude || 0 >= Vector3.Dot(lineSeg.direction, worldCellCenterPos - origin))
                {
                    continue;
                }

                sqrDis = lineSeg.MinimumDistanceSquared(worldCellCenterPos, out t_c);

                //선분에 멀리있는 셀들을 걸러낸다
                if(CELL_SQUARED_RADIUS < sqrDis)
                {
                    continue;
                }

                cellList.AddLast(worldCellCenterPos);
            }


            Vector3[] result = (from v3 in cellList
                                orderby (v3-origin).sqrMagnitude ascending
                                select v3).ToArray();

            return result;
        }

        //____________________________________________
        //                  충돌 검사   
        //____________________________________________
       

        public void CollisionPush(Being src , Being dst)
        {
            
            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;

            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            sqr_dis = src.transform.localPosition - dst.transform.localPosition;
            r_sum = src.GetCollider_Radius() + dst.GetCollider_Radius();

            //1.두 캐릭터가 겹친상태 
            if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
            {
                //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                //todo : 최적화 필요 

                Vector3 n = sqr_dis.normalized;
                //Vector3 n = sqr_dis;
                float meterPersecond = 2f;

                //2.반지름 이상으로 겹쳐있는 경우
                if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
                {
                    //3.완전 겹쳐있는 경우
                    if (n == Vector3.zero)
                    {
                        //방향값이 없기 때문에 임의로 지정해 준다. 
                        n = Misc.RandomDir8_AxisY();
                    }

                    meterPersecond = 0.5f;
                }

                //밀리는 처리 
                //if(Being.eKind.skeleton !=  src._kind || Being.eKind.skeleton == dst._kind)
                src.Move_Push(n, meterPersecond);

                //if (Being.eKind.skeleton != dst._kind  || Being.eKind.skeleton == src._kind)
                dst.Move_Push(-n, meterPersecond);

            }
        }

        //챔프를 중심으로 3x3그리드 영역의 정보를 가지고 충돌검사한다
        public void UpdateCollision_UseGrid3x3() //3x3 => 5x5 => 7x7 ... 홀수로 그리드 범위를 늘려 테스트 해볼 수 있다
        {
            CellInfo cellInfo = null;
            foreach(Being src in _linearSearch_list)
            {

                //1. 3x3그리드 정보를 가져온다
                cellInfo = SingleO.gridManager.GetCellInfo_NxN(src._cellInfo._index, 3);

                foreach (Being dst in cellInfo)
                {
                    if (src == dst) continue;
                    CollisionPush(src, dst);
                }
            }
        }


        public void CollisionPush_Rigid(Being src, StructTile structTile)
        {

            //이상진동 : 방향의 평균내기 방식
            //Vector3 smoothDir = Misc.GetDir8Normal_AxisY(structTile._dir);
            //smoothDir += src._move._direction.normalized;
            //smoothDir /= 2f;
            //src._move.Move_Forward(smoothDir, 2f, 0.5f);
            //return;

            const float Tile_Radius = 0.08f;
            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            Vector3 sqr_dis = src.transform.localPosition - structTile._v3Center;
            float r_sum = src.GetCollider_Radius() + Tile_Radius;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
            {
                //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                //todo : 최적화 필요 

                Vector3 n = sqr_dis.normalized;
                //n = Vector3.back;
                //Vector3 n = sqr_dis;
                float div_dis = 0.5f;

                //2.반지름 이상으로 겹쳐있는 경우
                if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
                {
                    //3.완전 겹쳐있는 경우
                    if (n == Vector3.zero)
                    {
                        //방향값이 없기 때문에 임의로 지정해 준다. 
                        n = Misc.RandomDir8_AxisY();
                    }

                    div_dis = 0.2f;
                }

                //src.transform.position = collisionCellPos_center + n * 0.16f;
                //src._move.Move_Forward(n, 2f, div_dis);
                src.Move_Forward(n, div_dis, true);
                //DebugWide.LogBlue(SingleO.gridManager.ToCellIndex(src.transform.position, Vector3.up) + "   " + src.transform.position);
                

            }
        }

        //고정된 물체와 충돌 검사 : 동굴벽 등 
        public void CollisionPush_StructTile(Being src, StructTile structTile)
        {
            if (null == structTile) return;

            Vector3 srcPos = src.transform.position;
            Vector3 centerToSrc_dir = srcPos - structTile._v3Center;
            Vector3 push_dir = Misc.GetDir8Normal_AxisY(structTile._dir);

            float size = GridManager.GridCell_HalfSize;
            Vector3 center = Vector3.zero;
            LineSegment3 line3 = new LineSegment3();
            //8방향별 축값 고정  
            switch (structTile._dir)
            {
                case eDirection8.up:
                    {
                        srcPos.z = structTile._v3Center.z + size;
                    }
                    break;
                case eDirection8.down:
                    {
                        srcPos.z = structTile._v3Center.z - size;
                    }
                    break;
                case eDirection8.left:
                    {
                        srcPos.x = structTile._v3Center.x - size;
                    }
                    break;
                case eDirection8.right:
                    {
                        srcPos.x = structTile._v3Center.x + size;
                    }
                    break;
                case eDirection8.leftUp:
                    {
                        //down , right
                        if(StructTile.DiagonalFixing == structTile._id)
                        {
                            srcPos.x = structTile._v3Center.x - size;
                            srcPos.z = structTile._v3Center.z + size;
                            break;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._v3Center;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._v3Center;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        //down , left
                        if (StructTile.DiagonalFixing == structTile._id)
                        {
                            srcPos.x = structTile._v3Center.x + size;
                            srcPos.z = structTile._v3Center.z + size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._v3Center;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._v3Center;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        //up , right
                        if (StructTile.DiagonalFixing == structTile._id)
                        {
                            srcPos.x = structTile._v3Center.x - size;
                            srcPos.z = structTile._v3Center.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;

                        center = structTile._v3Center;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._v3Center;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.rightDown:
                    {
                        //up , left
                        if (StructTile.DiagonalFixing == structTile._id)
                        {
                            srcPos.x = structTile._v3Center.x + size;
                            srcPos.z = structTile._v3Center.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._v3Center;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._v3Center;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;

            }

            src.transform.position = srcPos;

        }

        public void UpdateCollision_UseDirectGrid3x3()
        {
            //int count = 0;
            Vector3 collisionCellPos_center = Vector3.zero;
            CellInfo cellInfo = null;
            StructTile structTile = null;
            foreach (Being src in _linearSearch_list)
            {
                if (null == src._cellInfo) continue;
                if (true == src.isDeath()) continue;

                //1. 3x3그리드 정보를 가져온다
                foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[3])
                {
                    
                    //count++;
                    cellInfo = SingleO.gridManager.GetCellInfo(ix + src._cellInfo._index);
                    if (null == cellInfo) continue;

                    foreach (Being dst in cellInfo)
                    {
                        //count++;
                        if (src == dst) continue;
                        if (true == dst.isDeath()) continue;

                        CollisionPush(src, dst);
                    }
                }


                //동굴벽과 캐릭터 충돌처리 
                if (SingleO.gridManager.HasStructTile(src.transform.position, out structTile))
                {
                    CollisionPush_StructTile(src, structTile);
                    //CollisionPush_Rigid(src, structTile);
                }

            }

            //DebugWide.LogRed(_listTest.Count + "  총회전:" + count); //114 , 1988
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
        /// 가까운 대상 객체의 충돌원이 지정된 최소/최대 원에 포함되는지 검사한다 
        /// 조건에 포함하는 가장 가까운 객체를 반환한다
        /// 대상 객체의 충돌원 크기와 상관없이, 최대 원 크기의 그리드를 가져와 그리드 안에있는 객체들로만 검사한다   
        /// </summary>
        public Being GetNearCharacter(Being src, Camp.eRelation vsRelation, float minRadius, float maxRadius)
        {
            float sqr_minRadius = 0;
            float sqr_maxRadius = 0;
            float min_value = maxRadius * maxRadius * 1000f; //최대 반경보다 큰 최대값 지정
            float sqr_dis = 0f;

            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint NxN = SingleO.gridManager.GetNxNIncluded_CircleRadius(maxRadius);

            //int count = 0;
            CellInfo cellInfo = null;
            Being target = null;
            foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[ NxN ])
            {
                cellInfo = SingleO.gridManager.GetCellInfo(ix + src._cellInfo._index);

                if (null == cellInfo) continue;

                foreach (Being dst in cellInfo)
                {
                    if (src == dst) continue;
                    if (true == dst.isDeath()) continue; //쓰러진 객체는 처리하지 않는다 

                    if(vsRelation != Camp.eRelation.Unknown && null != src._belongCamp && null != dst._belongCamp)
                    {
                        Camp.eRelation getRelation = SingleO.campManager.GetRelation(src._belongCamp.campKind, dst._belongCamp.campKind);

                        //요청 관계가 아니면 처리하지 않는다 
                        if (vsRelation != getRelation)
                            continue;
                    }

                    //count++;
                    //==========================================================
                    sqr_minRadius = Mathf.Pow(minRadius + dst.GetCollider_Radius(), 2);
                    sqr_maxRadius = Mathf.Pow(maxRadius + dst.GetCollider_Radius(), 2);
                    sqr_dis = (src.transform.position - dst.transform.position).sqrMagnitude;

                    //최대 반경 이내일 경우
                    if (sqr_minRadius <= sqr_dis && sqr_dis <= sqr_maxRadius)
                    {

                        //DebugWide.LogBlue(min_value + "__" + sqr_dis +"__"+  dst.name); //chamto test

                        //기존 객체보다 더 가까운 경우
                        if (min_value > sqr_dis)
                        {
                            min_value = sqr_dis;
                            target = dst;
                        }
                    }
                    //==========================================================v
                }
            }

            //DebugWide.LogRed(count);
            return target;
        }



        /// <summary>
        /// 지정된 범위의 캐릭터가 특정캐릭터를 쳐다보게 설정한다
        /// </summary>
        /// <param name="target">Target.</param>
        public void LookAtTarget(Being src , uint gridRange_NxN)
        {
            
            Vector3 dir = Vector3.zero;
            CellInfo cellInfo = null;
            foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[gridRange_NxN])
            {
                cellInfo = SingleO.gridManager.GetCellInfo(ix + src._cellInfo._index);
                if (null == cellInfo) continue;

                foreach (Being dst in cellInfo)
                {
                    if (src == dst) continue;

                    if ((int)Behavior.eKind.Idle <= (int)dst._behaviorKind && (int)dst._behaviorKind <= (int)Behavior.eKind.Idle_Max)
                    {
                        dir = src.transform.position - dst.transform.position;

                        //그리드범위에 딱들어가는 원을 설정, 그 원 밖에 있으면 처리하지 않는다 
                        //==============================
                        float sqr_radius = (float)(gridRange_NxN) / 2f; //반지름으로 변환
                        sqr_radius *= SingleO.gridManager.cellSize_x; //셀크기로 변환
                        sqr_radius *= sqr_radius; //제곱으로 변환
                        if(sqr_radius < dir.sqrMagnitude)
                            continue;
                        //==============================

                        //dst.Idle_View(dir, true); //todo 나중에 수정된 함수 호출하기 
                        dst._behaviorKind = Behavior.eKind.Idle_LookAt;
                    }
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



        public Being Create_Character(Transform parent, Being.eKind eKind, Camp belongCamp, Vector3 pos)
        {
            _id_sequence++;

            GameObject obj = CreatePrefab(eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            Being cha = obj.AddComponent<Being>();
            obj.AddComponent<Movement>();
            obj.AddComponent<AI>();
            cha._id = _id_sequence;
            cha._kind = eKind;
            cha._belongCamp = belongCamp;
            cha.transform.localPosition = pos;
            //cha.Init_Create();

            _beings.Add(_id_sequence,cha);
            _linearSearch_list.Add(cha); //속도향상을 위해 중복된 데이터 추가

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

        private int __TestSkelCount = 9;
        public void Create_Characters()
        {

            if (null == SingleO.unitRoot) return;

            //챔프 설정

            Vector3 pos = new Vector3(3.2f,0,1.12f);
            Being being = null;
            being = Create_Character(SingleO.unitRoot, Being.eKind.lothar, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.garona, null, pos);
            being = Create_Character(SingleO.unitRoot, Being.eKind.footman, null, pos);
            being = Create_Character(SingleO.unitRoot, Being.eKind.spearman, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.brigand, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.ogre, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.conjurer, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.slime, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.raider, null, pos);
            being = Create_Character(SingleO.unitRoot, Being.eKind.grunt, null, pos);
            being = Create_Character(SingleO.unitRoot, Being.eKind.knight, null, pos);


            for (int i = 0; i < __TestSkelCount;i++)
            {
                //pos.x = (float)Misc.rand.NextDouble() * Mathf.Pow(-1f, i);
                //pos.z = (float)Misc.rand.NextDouble() * Mathf.Pow(-1f, i);
                being = Create_Character(SingleO.unitRoot, Being.eKind.skeleton, null, pos);
            }

            //being = Create_Character(SingleO.unitRoot, Being.eKind.daemon, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.waterElemental, null, pos);
            //being = Create_Character(SingleO.unitRoot, Being.eKind.fireElemental, null, pos);

        }

        public void Create_ChampCamp()
        {
            if (null == SingleO.unitRoot) return;

            string Blue_CampName = "Blue_Champ";
            string White_CampName = "White_Skel";
            int camp_position = 0;

            Camp camp_BLUE = SingleO.campManager.GetCamp(Camp.eKind.Blue, Blue_CampName.GetHashCode());
            Camp camp_WHITE = SingleO.campManager.GetCamp(Camp.eKind.White, White_CampName.GetHashCode());
            Being being = null;

            // -- 블루 진형 --
            being = Create_Character(SingleO.unitRoot, Being.eKind.lothar, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            camp_position++;
            being = Create_Character(SingleO.unitRoot, Being.eKind.footman, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            camp_position++;
            being = Create_Character(SingleO.unitRoot, Being.eKind.spearman, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            camp_position++;
            being = Create_Character(SingleO.unitRoot, Being.eKind.grunt, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            camp_position++;
            being = Create_Character(SingleO.unitRoot, Being.eKind.knight, camp_BLUE, camp_BLUE.GetPosition(camp_position));


            // -- 휜색 진형 --
            camp_position = 0;
            being = Create_Character(SingleO.unitRoot, Being.eKind.daemon, camp_WHITE, camp_WHITE.GetPosition(camp_position));
            camp_position++;
            //for (int i = 0; i < 6; i++)
            //{ 
            //    being = Create_Character(SingleO.unitRoot, Being.eKind.skeleton, camp_WHITE, camp_WHITE.GetPosition(camp_position));
            //    being.GetComponent<AI>()._ai_running = true;
            //    camp_position++;
            //}

            for (int i = 0; i < 10; i++)
            {
                being = Create_Character(SingleO.unitRoot, Being.eKind.brigand, camp_WHITE, camp_WHITE.GetPosition(camp_position));
                being.GetComponent<AI>()._ai_running = true;
                camp_position++;
            }

        }

    }
}


namespace HordeFight
{
   
    //public class Movable : MonoBehaviour
    //{

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
    //}

}


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
        private Being _me = null;
        private Being _target = null;
        private Vector3 _ai_Dir = Vector3.zero;
        private float _elapsedTime = 0f;


        public bool _ai_running = false;

        private void Start()
        {
            _me = GetComponent<Being>();
        }



        private void FixedUpdate()
        {
            if (false == _ai_running) return;

            if (true == _me.isDeath()) return;

            this.StateUpdate();
        }


        public bool Situation_Is_Enemy()
        {
            //불확실한 대상
            if (null == _target || null == _target._belongCamp || null == _me._belongCamp) return false;

            Camp.eRelation relation = SingleO.campManager.GetRelation(_me._belongCamp.campKind, _target._belongCamp.campKind);

            if (Camp.eRelation.Enemy == relation) return true;

            return false;
        }


        public bool Situation_Is_InRange(float range)
        {
            if (null == _target) return false;

            float sqrDis = (_me.transform.position - _target.transform.position).sqrMagnitude;

            float sqrRange = Mathf.Pow(range, 2);

            if (sqrRange >= sqrDis) return true;

            return false;
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
                        if (false == Situation_Is_InRange(Movement.ONE_METER * 6f))
                        {
                            //거리가 멀리 떨어져 있으면 다시 배회한다.
                            _state = eState.Roaming;

                        }else
                        {

                            //공격사거리 안에 들어오면 공격한다 
                            if (true == Situation_Is_InRange(Movement.ONE_METER * 1.2f))
                            {
                                _me.Attack(_target.transform.position - _me.transform.position);
                                //_state = eState.Attack;
                                break;
                                //DebugWide.LogBlue("attack");
                            }


                            _me.Move_Forward(_target.transform.position - _me.transform.position, 0.4f, true);
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
                        _target = SingleO.objectManager.GetNearCharacter(_me, Camp.eRelation.Enemy, 0, Movement.ONE_METER * 5f);
                        //DebugWide.LogBlue("Roaming: " + _target);
                        if (true == Situation_Is_Enemy())
                        {
                            _state = eState.Chase;
                            //DebugWide.LogBlue("Chase");
                            break;
                        }

                        //1초마다 방향을 바꾼다
                        if(1f <= _elapsedTime)
                        {
                            _ai_Dir = Misc.RandomDir8_AxisY();
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


//가속도계 참고할것  :  https://docs.unity3d.com/kr/530/Manual/MobileInput.html
//마우스 시뮬레이션  :  https://docs.unity3d.com/kr/530/ScriptReference/Input.html   마우스 => 터치로 변환
//========================================================
//==================      터치  처리      ==================
//========================================================
namespace HordeFight
{

    public class TouchControl : MonoBehaviour
    {
        public Being _selected = null;
        
		private void Start()
		{
            SingleO.touchEvent.Attach_SendObject(this.gameObject);
		}

		private void Update()
		{
            if (null == _selected) return;


		}

		private Vector3 __startPos = Vector3.zero;
		private void TouchBegan() 
        {
            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            __startPos = hit.point;
            __startPos.y = 0f;


            Being getBeing = hit.transform.GetComponent<Being>();
            if(null != getBeing)
            {
                //쓰러진 객체는 처리하지 않는다 
                if (true == getBeing.isDeath()) return;
                
                //전 객체 선택 해제 
                if (null != _selected)
                {
                    SingleO.lineControl.SetActive(_selected._UIID_circle_collider, false);
                }

                //새로운 객체 선택
                _selected = getBeing;
                SingleO.lineControl.SetActive(_selected._UIID_circle_collider, true);
            }
            //else
            //{
            //    if (null != _selected)
            //    {
            //        SingleO.lineControl.SetActive(_selected._UIID_circle_collider, false);
            //    }
            //    _selected = null;
            //}


            //DebugWide.LogBlue(__startPos + "  " + _selected); //chamto test

            //===============================================

            if (null == _selected) return;

            //챔프를 선택한 경우, 추가 처리 하지 않는다
            if (getBeing == _selected) return;

            //_selected.MoveToTarget(hit.point, 1f);
           
           
        }
        private void TouchMoved() 
        {
            RaycastHit hit = SingleO.touchEvent.GetHit3D();

            if (null == _selected) return;

            //_selected.Attack(hit.point - _selected.transform.position);
            //_selected.Block_Forward(hit.point - _selected.transform.position);
            _selected.Move_Forward(hit.point - _selected.transform.position, 1f, true);  

            Being target = SingleO.objectManager.GetNearCharacter(_selected, Camp.eRelation.Unknown, 0, Movement.ONE_METER * 1.2f);
            if (null != target)
            {
                //_selected.Move_Forward(hit.point - _selected.transform.position, 3f, true); 
                _selected.Attack(target.transform.position - _selected.transform.position , target);
                //target.AddHP(-1);

            }

            //View_AnimatorState();
        }
        private void TouchEnded() 
        {
            if (null == _selected) return;

            _selected.Idle();

        }

        //애니메이터 상태별 상세값이 어떻게 변화되는지 보기 위해 작성함
        //ChampStateMachine.OnStateEnter 에서 전이중일 때, "상태 시작함수"의 현재상태값이 "다음 상태"로 나오는 반면, 아래함수로 직접 출력해 보면 "현재 상태"로 나온다
        public void View_AnimatorState()
        {
            AnimatorStateInfo aniState = _selected._animator.GetCurrentAnimatorStateInfo(0);
            AnimatorTransitionInfo aniTrans = _selected._animator.GetAnimatorTransitionInfo(0);
            float normalTime = aniState.normalizedTime - (int)aniState.normalizedTime;
            float playTime = aniState.length;
            string stateName = SingleO.hashMap.GetString(aniState.shortNameHash);
            string transName = SingleO.hashMap.GetString(aniTrans.nameHash);
            int hash = Animator.StringToHash("attack");
            if (hash == aniState.shortNameHash)
            {

                DebugWide.LogBlue(aniTrans.nameHash +  "   tr: " + transName  + "    du: " + aniTrans.duration + "   trNt: " + aniTrans.normalizedTime + 
                                  "  :::   st: " + stateName + "   ct: " + (int)aniState.normalizedTime + "  stNt:" + normalTime);
            }
            else
            {
                DebugWide.LogRed(aniTrans.nameHash +  "   tr: " + transName + "    du: " + aniTrans.duration + "   trNt: " + aniTrans.normalizedTime + 
                                 "  :::   st: " + stateName + "   ct: " + (int)aniState.normalizedTime + "  stNt:" + normalTime);
            }
        }
    }



    //==================      기본 터치 이벤트 처리      ==================

    public class TouchEvent : MonoBehaviour
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
                //SendTouchEvent_Device_Target();
                SendTouchEvent_Device_NonTarget();
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                //SendMouseEvent_Editor_Target();
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

        //==========================================

        //private void SendTouchEvent_Device_Target()
        //{
        //    if (Input.touchCount > 0)
        //    {
        //        if (Input.GetTouch(0).phase == TouchPhase.Began)
        //        {
        //            //DebugWide.LogError("Update : TouchPhase.Began"); //chamto test
        //            _prevTouchMovedPos = this.GetTouchPos();
        //            _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.GetTouch(0).position);
        //        }
        //        else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
        //        {
        //            //DebugWide.LogError("Update : TouchPhase.Moved"); //chamto test

        //            if (null != _TouchedObject)
        //                _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

        //            _prevTouchMovedPos = this.GetTouchPos();

        //        }
        //        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        //        {
        //            //DebugWide.LogError("Update : TouchPhase.Ended"); //chamto test
        //            if (null != _TouchedObject)
        //                _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
        //            _TouchedObject = null;
        //        }
        //        else
        //        {
        //            DebugWide.LogError("Update : Exception Input Event : " + Input.GetTouch(0).phase);
        //        }
        //    }
        //}


        //private bool f_isEditorDraging = false;
        //private void SendMouseEvent_Editor_Target()
        //{

        //    //=================================
        //    //    mouse Down
        //    //=================================
        //    //Debug.Log("mousedown:" +Input.GetMouseButtonDown(0)+ "  mouseup:" + Input.GetMouseButtonUp(0) + " state:" +Input.GetMouseButton(0)); //chamto test
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        //Debug.Log ("______________ MouseBottonDown ______________" + m_TouchedObject); //chamto test
        //        if (false == f_isEditorDraging)
        //        {

        //            _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.mousePosition);
        //            if (null != _TouchedObject)
        //                f_isEditorDraging = true;
        //        }

        //    }

        //    //=================================
        //    //    mouse Up
        //    //=================================
        //    if (Input.GetMouseButtonUp(0))
        //    {

        //        //Debug.Log ("______________ MouseButtonUp ______________" + m_TouchedObject); //chamto test
        //        f_isEditorDraging = false;

        //        if (null != _TouchedObject)
        //        {
        //            _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
        //        }

        //        _TouchedObject = null;

        //    }


        //    //=================================
        //    //    mouse Move
        //    //=================================
        //    if (this.GetMouseButtonMove(0))
        //    {

        //        //=================================
        //        //     mouse Drag 
        //        //=================================
        //        if (f_isEditorDraging)
        //        {
        //            //Debug.Log ("______________ MouseMoved ______________" + m_TouchedObject); //chamto test

        //            if (null != _TouchedObject)
        //                _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);


        //        }//if
        //    }//if
        //}

        //==========================================

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
