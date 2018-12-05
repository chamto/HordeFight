using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
//using UnityEngine.Assertions;

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

            //SingleO.mainCamera.gameObject.AddComponent<CameraWalk>();
            gameObject.AddComponent<CameraWalk>();

            gameObject.AddComponent<TouchEvent>();
            gameObject.AddComponent<TouchControl>();

            SingleO.hierarchy.GetTransform("z_debug").gameObject.AddComponent<DebugViewer>();
            SingleO.debugViewer._origin = SingleO.hierarchy.GetTransform("z_debug/origin");
            SingleO.debugViewer._target = SingleO.hierarchy.GetTransform("z_debug/target");
            //===================

            //SingleO.objectManager.Create_Characters(); //여러 캐릭터들 테스트용
            //SingleO.objectManager.Create_ChampCamp();


        }

        // Update is called once per frame
        //void Update()
        void FixedUpdate()
        {

        }

        void OnGUI()
        {
            //if (GUI.Button(new Rect(10, 10, 200, 100), new GUIContent("Refresh Timemap Fog of War")))
            //{
            //    //RuleExtraTile ruleTile =  SingleO.gridManager.GetTileMap_Struct().GetTile<RuleExtraTile>(new Vector3Int(0, 0, 0));

            //    SingleO.gridManager.GetTileMap_FogOfWar().RefreshAllTiles();
            //    //DebugWide.LogBlue("TileMap_Struct RefreshAllTiles");
            //}
        }


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

        public static TouchControl touchControl
        {
            get
            {
                return CSingletonMono<TouchControl>.Instance;
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

        public static CameraWalk cameraWalk
        {
            get
            {
                return CSingletonMono<CameraWalk>.Instance;
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

        private static Transform _shotRoot = null;
        public static Transform shotRoot
        {
            get
            {
                if (null == _shotRoot)
                {
                    GameObject obj = GameObject.Find("0_shot");
                    if (null != obj)
                    {
                        _shotRoot = obj.GetComponent<Transform>();
                    }

                }
                return _shotRoot;
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

        public RuntimeAnimatorController _base_Animator = null;

        //키값 : 애니메이션 이름에 대한 해쉬코드 
        public Dictionary<int, AnimationClip> _aniClips = new Dictionary<int, AnimationClip>();
        public Dictionary<int, Sprite> _sprIcons = new Dictionary<int, Sprite>();
        public Dictionary<int, TileBase> _tileScripts = new Dictionary<int, TileBase>();


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
            _base_Animator = Resources.Load<RuntimeAnimatorController>("Warcraft/Animation/base_Animator");


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

            TileBase[] tiles = Resources.LoadAll<TileBase>("Warcraft/Palette/ScriptTile");
            foreach(TileBase r in tiles)
            {
                _tileScripts.Add(r.name.GetHashCode(), r);
                //DebugWide.LogBlue(r.name);
            }
        }


        public TileBase GetTileScript(int nameToHash)
        {
            if(true == _tileScripts.ContainsKey(nameToHash))
            {
                return _tileScripts[nameToHash];
            }

            return null;
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
            //render.sortingOrder = -10; //나중에 그려지게 한다.
            render.sortingLayerName = "Effect";
            render.positionCount = 2;
            render.transform.localPosition = Vector3.zero;


            render.startWidth = 0.12f;
            render.endWidth = 0.12f;
            render.startColor = Color.red;
            render.endColor = Color.red;

            _list.Add(_sequenceId, info); //추가

            Vector3 pos = Vector3.zero;
            pos.x = -0.5f; pos.z = -0.8f;
            render.SetPosition(0, pos);
            pos.x += 0.8f;
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
            //render.sortingOrder = -10; //먼저그려지게 한다.
            render.sortingLayerName = "Effect";
            render.positionCount = 20;
            render.loop = true; //처음과 끝을 연결한다 .
            render.transform.localPosition = Vector3.zero;

            color.a = 0.4f; //흐리게 한다
            render.startWidth = 0.1f;
            render.endWidth = 0.1f;
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
        public float _length_interval = 0.1f;
        public Transform _origin = null;
        public Transform _target = null;

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
#if UNITY_EDITOR
            Vector3Int prev = Vector3Int.zero;
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(7, Vector3.up))
            foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter_Tornado(11,11, Vector3.up))
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter(7, Vector3.up))
            {
                //DebugWide.LogBlue(v);
                Debug.DrawLine(cur - Vector3.one * 0.3f , cur + Vector3.one * 0.3f, Color.red);
                Debug.DrawLine(prev, cur);

                UnityEditor.Handles.Label(cur, "( "+cur.x + " , " + cur.z + " )");

                prev = cur;

            }
#endif
        }

        public void UpdateDraw_FogOfWar_DivisionNum()
        {
#if UNITY_EDITOR
            BoundsInt bounds = SingleO.gridManager.GetTileMap_FogOfWar().cellBounds;
            RuleExtraTile rule = null;
            byte divisionNum = 0;
            Vector3 pos3d = Vector3.zero;
            foreach (Vector3Int xy in bounds.allPositionsWithin)
            {
                rule = SingleO.gridManager.GetTileMap_FogOfWar().GetTile<RuleExtraTile>(xy);
                if (null != rule)
                {
                    pos3d = SingleO.gridManager.ToPosition3D(xy);
                    pos3d.x += 0.08f;
                    pos3d.z += 0.16f;
                    divisionNum = rule._tileDataMap.Get_DivisionNum(xy);
                    if (0 < divisionNum)
                        UnityEditor.Handles.color = Color.red;
                    else
                        UnityEditor.Handles.color = Color.white;

                    UnityEditor.Handles.Label(pos3d, "" + divisionNum);
                }


            }
#endif
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
                if(StructTile.Specifier_DiagonalFixing == t._specifier)
                {
                    ccc = Color.red;
                }

                Vector3 end = t._center_3d + Misc.GetDir8_Normal3D_AxisY(t._dir) * 0.12f;
                Debug.DrawLine(t._center_3d, end, ccc);


                //UnityEditor.Handles.Label(t._center_3d, "( " + cur.x + " , " + cur.z + " )");

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
            Vector3Int to_XY_2d;
            Vector3 from_3d, to_3d;
            NavGraphNode from_nav, to_nav;
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tileBlock_size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int from_XY_2d in boundsInt.allPositionsWithin)
            {
                from_1d = grid.ToPosition1D(from_XY_2d, boundsInt.size.x);
                from_3d = grid.ToPosition3D(from_XY_2d);

                SparseGraph.EdgeList list = finder._graph.GetEdges(from_1d);
                foreach (GraphEdge e in list)
                {
                    to_XY_2d = grid.ToPosition2D(e.To(), boundsInt.size.x);
                    to_3d = grid.ToPosition3D(to_XY_2d);

                    from_nav = finder._graph.GetNode(e.From()) as NavGraphNode;
                    to_nav = finder._graph.GetNode(e.To()) as NavGraphNode;
                    Debug.DrawLine(from_nav.Pos(), to_nav.Pos(), Color.green);

                    //chamto test
                    if (true == debugLog)
                    {
                        DebugWide.LogBlue(e + "  " + from_XY_2d + "   " + to_XY_2d);
                    }

                }
            }
        }


        public void Draw_Grid()
        {
            float size = SingleO.gridManager.cellSize_x;
            Vector3 start = Vector3.zero;
            Vector3 end = Vector3.zero;
            Vector3 xz_length = new Vector3(6.5f, 0, 4f);

            end.z = xz_length.z;
            for (int x = 0; size * x < xz_length.x; x++)
            {
                start.x = size * x;
                end.x = size * x;

                Debug.DrawLine(start, end, Color.white);
            }

            start = end = Vector3.zero;
            end.x = xz_length.x;
            for (int z = 0; size * z < xz_length.z; z++)
            {
                start.z = size * z;
                end.z = size * z;

                Debug.DrawLine(start, end, Color.white);
            }

        }


        public void Update_IsVisible_SightOfView(Vector3 origin, Vector3 target, float length_interval)
        {
#if UNITY_EDITOR

            //값이 너무 작으면 바로 종료 
            if(0.01f >= length_interval)
            {
                return;
            }

            target = SingleO.gridManager.ToCenter3D_FromPosition3D(target);

            Vector3Int orgin_2d = SingleO.gridManager.ToPosition2D(origin);
            Vector3 origin_center = SingleO.gridManager.ToPosition3D_Center(orgin_2d);
            Vector3 line = target - origin_center;
            Vector3 n = line.normalized;

            StructTile structTile = null;
            Vector3 next = Vector3.zero;
            string keyword = "";
            for (int i = 0; line.sqrMagnitude > next.sqrMagnitude; i++)
            {
                //========================================================================
                Vector3 cr = Vector3.Cross(n, Vector3.up);
                cr.Normalize();
                Debug.DrawLine(origin_center + next - cr * 0.01f, origin_center + next + cr * 0.01f, Color.red);
                //========================================================================


                next = n * length_interval * i;

                keyword = "";

                if (orgin_2d == SingleO.gridManager.ToPosition2D(origin_center + next))
                {
                    keyword += "Eq";
                    UnityEditor.Handles.Label(origin_center + next, keyword + ":" + i);
                    continue;
                }


                if (true == SingleO.gridManager.HasStructTile(origin_center + next, out structTile))
                {
                    keyword += "St";

                    if (true == structTile._isUpTile)
                    {
                        //덮개타일인 경우
                        keyword += "Up";
                    }

                    switch (SingleO.gridManager.IsIncluded_InDiagonalArea(origin_center + next))
                    {
                        case GridManager._ReturnMessage_NotIncluded_InDiagonalArea:
                            keyword += "AntiDg";
                            break;
                        case GridManager._ReturnMessage_Included_InDiagonalArea:
                            //대각타일인 경우
                            keyword += "Dg";
                            break;
                    }


                }
                UnityEditor.Handles.Label(origin_center + next, keyword + ":" + i);

            }

            Debug.DrawLine(origin_center, target, Color.green);

#endif
        }


        public void IsVisibleTile(Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {
#if UNITY_EDITOR
            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval)
            {
                return;
            }

            target_3d = SingleO.gridManager.ToCenter3D_FromPosition3D(target_3d);

            //대상타일이 구조덮개타일이면 볼수없다. 바로 끝 
            //if (true == this.HasStructUpTile(target)) return false;

            Vector3Int origin_2d = SingleO.gridManager.ToPosition2D(origin_3d);
            Vector3 origin_3d_center = SingleO.gridManager.ToPosition3D_Center(origin_2d);

            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            StructTile structTile = null;
            if (SingleO.gridManager.HasStructTile(origin_3d, out structTile))
            {
                switch (structTile._dir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._dir);
                            origin_3d_center.x += dir.x * SingleO.gridManager.cellSize_x * 0.5f;
                            origin_3d_center.z += dir.y * SingleO.gridManager.cellSize_z * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            origin_3d_center = SingleO.gridManager.ToPosition3D_Center(origin_2d + Misc.GetDir8_Normal2D(structTile._dir));
                        }
                        break;
                }

                UnityEditor.Handles.Label(origin_3d_center, "__before:" + SingleO.gridManager.ToPosition3D_Center(origin_2d) + " after:" + origin_3d_center);
            }


            Vector3 line = target_3d - origin_3d_center;
            Vector3 n = line.normalized;

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            Vector3 next = n * length_interval * count;
            while (line.sqrMagnitude > next.sqrMagnitude)
            {
                UnityEditor.Handles.Label(origin_3d_center + next, "__" + count);

                if (SingleO.gridManager.HasStructTile(origin_3d_center + next, out structTile))
                //if (this.HasStructUpTile(origin_center + next))
                {
                    if (true == structTile._isUpTile)
                    {

                        //return false;

                        if (GridManager._ReturnMessage_NotIncluded_InDiagonalArea != SingleO.gridManager.IsIncluded_InDiagonalArea(origin_3d_center + next))
                        {
                            //return false;
                            break;
                        }
                    }

                }

                count++;
                next = n * length_interval * count;

            }

            UnityEditor.Handles.Label(origin_3d_center + next,  "_________end :" + count);

            Debug.DrawLine(origin_3d_center, target_3d, Color.green);
            //return true;
#endif
        }

        void OnDrawGizmos()
        {
            //UnityEditor.Handles.Label(Vector3.zero, "0,0");
            //UpdateDraw_IndexesNxN();

            //Debug.DrawLine(_start, _end);

            //UpdateDraw_StructTileDir();

            //UpdateDraw_FogOfWar_DivisionNum();

            //Update_DrawEdges(false);


            //if(null != _origin && null != _target)
            //{
            //    Draw_Grid();
            //    Update_IsVisible_SightOfView(_origin.position, _target.position , _length_interval);
            //    IsVisibleTile(_origin.position, _target.position, _length_interval);
            //}
        }

  //      void FixedUpdate()
		//{
		//}
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
        public const int Specifier_DiagonalFixing = 7; //대각고정 예약어

        public int _specifier = 0;
        public eDirection8 _dir = eDirection8.none;
        public Vector3 _center_3d = Vector3.zero;    //타일의 중앙 월드위치
        public bool _isUpTile = false; //챔프보다 위에 있는 타일 (TileMap_StructUp)
    }


    public class GridManager : MonoBehaviour
    {
        public const int NxN_MIN = 3;   //그리드 범위 최소크기
        public const int NxN_MAX = 11;  //그리드 범위 최대크기
        //public const float GridCell_Size = 0.16f;
        //public const float GridCell_HalfSize = GridCell_Size * 0.5f;

        public const float ONE_METER = 1f; //타일 한개의 가로길이 
        public const float SQR_ONE_METER = ONE_METER * ONE_METER;
        public const float WorldToMeter = 1f / ONE_METER;
        public const float MeterToWorld = ONE_METER;


        private Grid _grid = null;
        private Tilemap _tilemap_struct = null;
        private Tilemap _tilemap_structUp = null;
        private Tilemap _tilemap_fogOfWar = null;
        public Dictionary<Vector3Int,CellInfo> _cellInfoList = new Dictionary<Vector3Int,CellInfo>();
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

        public Tilemap GetTileMap_StructUp()
        {
            return _tilemap_structUp;
        }

        public Tilemap GetTileMap_FogOfWar()
        {
            return _tilemap_fogOfWar;
        }

		private void Start()
		{
            _grid = GameObject.Find("0_grid").GetComponent<Grid>();
            GameObject o = GameObject.Find("Tilemap_Structure");
            if(null != o)
            {
                _tilemap_struct = o.GetComponent<Tilemap>();    
            }
            o = GameObject.Find("Tilemap_StructureUp");
            if (null != o)
            {
                _tilemap_structUp = o.GetComponent<Tilemap>();
            }
            o = GameObject.Find("Tilemap_FogOfWar");
            if (null != o)
            {
                _tilemap_fogOfWar = o.GetComponent<Tilemap>();
            }

            //타일맵 좌표계 x-y에 해당하는 축값 back로 구한다 
            _indexesNxN[3] = CreateIndexesNxN_SquareCenter_Tornado(3,3, Vector3.back);
            _indexesNxN[5] = CreateIndexesNxN_SquareCenter_Tornado(5,5, Vector3.back);
            _indexesNxN[7] = CreateIndexesNxN_SquareCenter_Tornado(7,7, Vector3.back);
            _indexesNxN[9] = CreateIndexesNxN_SquareCenter_Tornado(9,9, Vector3.back);
            _indexesNxN[11] = CreateIndexesNxN_SquareCenter_Tornado(11,11, Vector3.back); //화면 세로길이를 벗어나지 않는 그리드 최소값
            _indexesNxN[29] = CreateIndexesNxN_SquareCenter_Tornado(29,19, Vector3.back);

            this.LoadTilemap_Struct();

		}


        //ref : https://gamedev.stackexchange.com/questions/150917/how-to-get-all-tiles-from-a-tilemap
        public void LoadTilemap_Struct()
        {
            if (null == _tilemap_struct) return;

            SingleO.gridManager.GetTileMap_Struct().RefreshAllTiles();
            StructTile structTile = null;
            RuleExtraTile.TilingRule ruleInfo = null;
            int specifier = 0;
            foreach (Vector3Int XY_2d in _tilemap_struct.cellBounds.allPositionsWithin)
            {
                RuleExtraTile ruleTile = _tilemap_struct.GetTile(XY_2d) as RuleExtraTile; //룰타일 종류와 상관없이 다 가져온다. 
                if (null == ruleTile) continue;

                ruleInfo = ruleTile._tileDataMap.GetTilingRule(XY_2d);
                if (null == ruleInfo || false == int.TryParse(ruleInfo.m_specifier, out specifier)) 
                    specifier = 0;


                structTile = new StructTile();
                structTile._specifier = specifier;
                structTile._center_3d = this.ToPosition3D_Center(XY_2d); 
                structTile._dir = ruleTile._tileDataMap.GetDirection8(XY_2d);
                structTile._isUpTile = ruleTile._tileDataMap.Get_IsUpTile(XY_2d);
                _structTileList.Add(XY_2d, structTile);

            }

            DebugWide.LogBlue("LoadTile : " + _structTileList.Count + "  -  TileMap_Struct RefreshAllTiles");

            //덮개 타일 생성
            TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            //TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            foreach (KeyValuePair<Vector3Int,StructTile> t in _structTileList)
            { 
                if(true == t.Value._isUpTile)
                {
                    _tilemap_structUp.SetTile(t.Key, tileScript);
                }
            }

            DebugWide.LogBlue("덮개타일 생성 완료 : " + tileScript.name);

        }


        //private void Update()
        void FixedUpdate()
        {

        }


        public bool IsVisibleTile(Vector3 origin_3d, Vector3 target_3d , float length_interval)
        {
            //interval 값이 너무 작으면 바로 종료 한다 
            if(0.01f >= length_interval)
            {
                return false;
            }

            //if(ToPosition2D(origin_3d) == new Vector3Int(22,13,0) && 
            //   ToPosition2D(target_3d) == new Vector3Int(22,9,0))
            //{
            //    DebugWide.LogBlue("오동작");

            //}
            //Vector3Int test = Vector3Int.zero;
            //test = _tilemap_struct.WorldToCell(origin_3d);
            //test = ToPosition2D(origin_3d);

            //대상타일이 구조덮개타일이면 볼수없다. 바로 끝 
            //if (true == this.HasStructUpTile(target)) return false;

            Vector3Int origin_2d = ToPosition2D(origin_3d);
            Vector3 origin_3d_center = ToPosition3D_Center(origin_2d);


            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            StructTile structTile = null;
            if (this.HasStructTile(origin_3d , out structTile))
            {
                switch(structTile._dir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._dir);
                            origin_3d_center.x += dir.x * cellSize_x * 0.5f;
                            origin_3d_center.z += dir.y * cellSize_z * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            origin_3d_center = ToPosition3D_Center(origin_2d + Misc.GetDir8_Normal2D(structTile._dir));
                        }
                        break;
                }

            }
                
            Vector3 line = target_3d - origin_3d_center;
            Vector3 n = line.normalized;

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            Vector3 next = n * length_interval * count;
            while (line.sqrMagnitude > next.sqrMagnitude)
            {
                
                if(this.HasStructTile(origin_3d_center + next, out structTile))
                //if (this.HasStructUpTile(origin_center + next))
                {
                    if(true == structTile._isUpTile)
                    {

                        //return false;

                        if (GridManager._ReturnMessage_NotIncluded_InDiagonalArea != this.IsIncluded_InDiagonalArea(origin_3d_center + next))
                        {
                            return false;
                        }
                    }

                }

                count++;
                next = n * length_interval * count;

            }

            return true;
        }

        public void SetTile(Tilemap tilemap, Vector3Int xy_2d , TileBase setTile)
        {
            TileBase getTile = tilemap.GetTile(xy_2d);
            if (getTile != setTile)
            {
                tilemap.SetTile(xy_2d, setTile);        
            }
        }


        public void Update_FogOfWar(Vector3 standard_3d, Vector3 lookAt_dir)
        {
            if (null == _tilemap_fogOfWar) return;

            //====================================================
            StartCoroutine(DivideUpdate_FogOfWar(standard_3d, lookAt_dir));
            return;
            //====================================================
        
            Color baseColor = Color.white;
            Color fogColor = new Color(1, 1, 1, 0.7f);
            TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_RuleExtraTile".GetHashCode());
            TileBase tileScript2 = SingleO.resourceManager.GetTileScript("ocean".GetHashCode());
            TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            TileBase tileScript4 = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            Vector3Int posXY_2d = this.ToPosition2D(standard_3d);
            Vector3 tile_3d_center = Vector3.zero;
            //Vector3 standard_3d_center = this.ToCenter3D_FromPosition3D(standard_3d);

            BoundsInt bounds = new BoundsInt(posXY_2d - new Vector3Int(14, 11, 0), new Vector3Int(28, 21, 1));

            foreach (Vector3Int tile_2d in bounds.allPositionsWithin)
            {
                //_tilemap_fogOfWar.SetTileFlags(xy, TileFlags.None);
                tile_3d_center = this.ToPosition3D_Center(tile_2d);

                RuleExtraTile ruleTile = _tilemap_fogOfWar.GetTile(tile_2d) as RuleExtraTile;
                float sqrDis = (tile_3d_center - standard_3d).sqrMagnitude;
                if (sqrDis <= Mathf.Pow(GridManager.MeterToWorld * 6.2f, 2))
                {
                    if (true == IsVisibleTile(standard_3d, tile_3d_center, 0.1f))
                    {

                        //대상과 정반대 방향이 아닐때 처리 
                        Vector3 tileDir = tile_3d_center - standard_3d;
                        tileDir.Normalize(); lookAt_dir.Normalize();
                        //tileDir = Misc.GetDir8_Normal3D(tileDir);
                        //target_dir = Misc.GetDir8_Normal3D(target_dir);


                        if (true == this.HasStructUpTile(tile_3d_center))
                        {
                            //구조덮개타일인 경우 
                            SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                        }
                        else 
                            if (Mathf.Cos(Mathf.Deg2Rad * 40f) < Vector3.Dot(tileDir, lookAt_dir))
                        {
                            //원거리 시야 표현
                            SetTile(_tilemap_fogOfWar, tile_2d, null);
                        }
                            else if (sqrDis <= Mathf.Pow(GridManager.MeterToWorld * 1.2f, 2))
                        {
                            //주변 시야 표현
                            SetTile(_tilemap_fogOfWar, tile_2d, null);
                        }
                        else
                        {
                            SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                        }

                        //SetTile(_tilemap_fogOfWar, xy, null);

                    }
                    else
                    {
                        SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                    }
                }
                else
                {
                    //안보이는 곳
                    SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                }

            }

        }

        private bool __is_dividing = false;
        public IEnumerator DivideUpdate_FogOfWar(Vector3 standard_3d, Vector3 lookAt_dir)
        {

            if (true == __is_dividing) yield break;
            __is_dividing = true;

            //TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            Color baseColor = Color.white;
            Color fogColor = new Color(1, 1, 1, 0.7f);
            Vector3Int posXY_2d = this.ToPosition2D(standard_3d);
            Vector3 tile_3d_center = Vector3.zero;
            Vector3Int tile_2d = Vector3Int.zero;
            int count = -1;
            foreach(Vector3Int xy in _indexesNxN[29])
            {
                tile_2d = xy + posXY_2d;

                //=====================================================

                tile_3d_center = this.ToPosition3D_Center(tile_2d);

                RuleExtraTile ruleTile = _tilemap_fogOfWar.GetTile(tile_2d) as RuleExtraTile;
                float sqrDis = (tile_3d_center - standard_3d).sqrMagnitude;
                if (sqrDis <= Mathf.Pow(GridManager.MeterToWorld * 6.2f, 2))
                {
                    if (true == IsVisibleTile(standard_3d, tile_3d_center, 0.1f))
                    {

                        //대상과 정반대 방향이 아닐때 처리 
                        Vector3 tileDir = tile_3d_center - standard_3d;
                        tileDir.Normalize(); lookAt_dir.Normalize();
                        //tileDir = Misc.GetDir8_Normal3D(tileDir);
                        //target_dir = Misc.GetDir8_Normal3D(target_dir);


                        if (true == this.HasStructUpTile(tile_3d_center))
                        {
                            //구조덮개타일인 경우 
                            SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                        }
                        else
                            if (Mathf.Cos(Mathf.Deg2Rad * 40f) < Vector3.Dot(tileDir, lookAt_dir))
                        {
                            //원거리 시야 표현
                            SetTile(_tilemap_fogOfWar, tile_2d, null);
                        }
                            else if (sqrDis <= Mathf.Pow(GridManager.MeterToWorld * 1.2f, 2))
                        {
                            //주변 시야 표현
                            SetTile(_tilemap_fogOfWar, tile_2d, null);
                        }
                        else
                        {
                            SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                        }

                        //SetTile(_tilemap_fogOfWar, xy, null);

                    }
                    else
                    {
                        SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                    }
                }
                else
                {
                    //안보이는 곳
                    SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                }

                //_tilemap_fogOfWar.SetTileFlags(dst, TileFlags.None);
                //if(_tilemap_fogOfWar.GetColor(dst).Equals(baseColor))
                //{
                //    _tilemap_fogOfWar.SetColor(dst, fogColor);    
                //}else
                //{
                //    _tilemap_fogOfWar.SetColor(dst, baseColor);    
                //}


                //=====================================================
                count++;


                if(0 == count % 300)
                    yield return new WaitForSeconds(0.001f);
                
                
            }
            __is_dividing = false;

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
        public Vector3Int[] CreateIndexesNxN_SquareCenter_Tornado(ushort width,ushort height, Vector3 axis)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == width % 2 || 0 == height % 2) 
            {
                DebugWide.LogRed("CreateIndexesNxN_SquareCenter_Tornado : width , height : 홀수값을 넣어야 한다 ");
                return null;   
            }
            //DebugWide.LogBlue("_____________________" + widthCenter);
            //string debugStr = "";
            //int debugSum = 0;

            int Tornado_Num = 1; 
            int Tornado_Count = 0;
            Vector3 dir = Vector3.right;
            Vector3Int prevMax = Vector3Int.zero;
            Vector3Int prediction = Vector3Int.zero;
            Vector3Int cur = Vector3Int.zero;
            Vector3Int[] indexes = new Vector3Int[width * height];
            int cnt = 0;
            int max_cnt = width > height ? width : height; //큰값이 들어가게 한다 
            max_cnt *= max_cnt;
            //for (int cnt = 0; cnt < indexes.Length; cnt++)
            for (int i = 0; i < max_cnt; i++)
            {
                if(Mathf.Abs(cur.x) < Mathf.Abs(width * 0.5f) &&
                   Mathf.Abs(cur.y) < Mathf.Abs(height * 0.5f))
                {
                    //DebugWide.LogBlue(cnt + "  " + cur);
                    indexes[cnt] = cur;
                    cnt++;
                }


                prediction.x = (int)(dir.x * Tornado_Num); //소수점 버림
                prediction.y = (int)(dir.y * Tornado_Num); //소수점 버림
                prediction.z = (int)(dir.z * Tornado_Num); //소수점 버림

                //DebugWide.LogBlue(" cnt: " + cnt + "  tnum : " + Tornado_Num + "  " +(prevMax + dir * Tornado_Num) + "  cur : " + cur);
                //해당방향의 최대값에 도달하면 90도 회전 
                if(prevMax + prediction == cur)
                {
                    //DebugWide.LogBlue("___" + cur);
                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Num + Tornado_Count%2); //1 1 2 2 3 3 4 4 5 5 .... 이렇게 증가하는 수열값임 
                    Tornado_Count++;

                    //debugStr += " " + Tornado_Num; //chamto test
                    //debugSum += Tornado_Num; 

                    //반시계방향으로 회전하도록 한다 
                    if (Vector3.up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        dir = Quaternion.Euler(0, -90f, 0) * dir;
                    }
                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, 90f) * dir;
                    }
                    dir.Normalize(); //부동소수점 문제 때문에 정규화 해준다 
                }

                //지정된 방향값으로 한칸씩 증가한다
                cur.x += (int)Mathf.Round(dir.x); //반올림 한다 : 대각선 방향 때문 
                cur.y += (int)Mathf.Round(dir.y);
                cur.z += (int)Mathf.Round(dir.z);
            }

            //DebugWide.LogBlue(debugStr + "   : sum : " + debugSum);

            return indexes;

        }

        //원형
        public Vector3Int[] CreateIndexesNxN_Circle(ushort radius, Vector3 axis)
        {
            Vector3Int[] indexes = new Vector3Int[5];



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
                    dir.Normalize(); //부동소수점 문제 때문에 정규화 해준다 

                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Count/4)+1; //1 1 1 1 2 2 2 2 3 3 3 3 .... 이렇게 증가하는 수열값임 

                    Tornado_Count++;
                }

            }

            return indexes;

        }



        /// <summary>
        /// 구조타일 영역에 미포함
        /// </summary>
        public const int _ReturnMessage_NotIncluded_InStructTile    = 0; 

        /// <summary>
        /// 구조타일 영역에 포함. 구조대각타일이 아니다
        /// </summary>
        public const int _ReturnMessage_Included_InStructTile       = 1; 

        /// <summary>
        /// 구조대각타일이며 , 대각타일 영역에 미포함
        /// </summary>
        public const int _ReturnMessage_NotIncluded_InDiagonalArea  = 2; 

        /// <summary>
        /// 구조대각타일이며 , 대각타일 영역에 포함
        /// </summary>
        public const int _ReturnMessage_Included_InDiagonalArea     = 3; 


        //구조타일의 대각타일 영역에 포함하는지 검사 
        public int IsIncluded_InDiagonalArea(Vector3 xz_3d)
        {
            Vector3Int xy_2d = _tilemap_struct.WorldToCell(xz_3d);

            StructTile structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                switch(structTile._dir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //특수고정대각 타일은 일반구조 타일처럼 처리한다 
                            if(StructTile.Specifier_DiagonalFixing == structTile._specifier)
                                return _ReturnMessage_Included_InStructTile;

                            //타일 중앙점 o , 검사할 점 p 가 있을 때
                            //대각타일 영역에서 점 p를 포함하는지 내적을 이용해서 검사 
                            Vector3 oDir = Misc.GetDir8_Normal3D_AxisY(structTile._dir);
                            //Vector3 pDir = xz_3d - structTile._center_3d; //실수값 부정확성 때문에 같은 위치에서도 다른 결과가 나옴.
                            Vector3 pDir = Misc.GetDir8_Normal3D(xz_3d - structTile._center_3d); //정규화된8 방향값으로 변환해서 부정확성을 최소로 줄인다.
                            if (0 > Vector3.Dot(oDir, pDir))
                                return _ReturnMessage_Included_InDiagonalArea; //대각타일 영역에 포함
                            else
                                return _ReturnMessage_NotIncluded_InDiagonalArea; //구조타일 영역에 포함 , 대각타일 영역에 미포함
                        }

                    default:
                        {
                            return _ReturnMessage_Included_InStructTile; //구조타일 영역에 포함
                        }
                        
                }

            }

            return _ReturnMessage_NotIncluded_InStructTile; //구조타일 영역에 미포함
        }

        public bool HasStructUpTile(Vector3 xz_3d)
        {
            StructTile structTile = null;
            return HasStructUpTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructUpTile(Vector3 xz_3d, out StructTile structTile)
        {
            //return HasStructUpTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
            return HasStructUpTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructUpTile_InPostion2D(Vector3Int xy_2d , out StructTile structTile)
        {
            structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                return structTile._isUpTile;
            }

            return false;
        }

        public bool HasStructTile(Vector3 xz_3d)
        {
            StructTile structTile = null;
            return HasStructTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
        }
        public bool HasStructTile(Vector3 xz_3d, out StructTile structTile)
        {
            //return HasStructTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
            return HasStructTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructTile_InPostion2D(Vector3Int xy_2d)
        {
            StructTile structTile = null;
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile_InPostion2D(Vector3Int xy_2d , out StructTile structTile)
        {
            structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                return true;
            }

            return false;
        }




        public CellInfo GetCellInfo(Vector3Int cellIndex)
        {
            CellInfo cell = null;
            _cellInfoList.TryGetValue(cellIndex, out cell);

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
            if (false == _cellInfoList.TryGetValue(cellIndex, out cell))
            {
                cell = new CellInfo();
                cell._index = cellIndex;
                _cellInfoList.Add(cellIndex, cell);
            }
            _cellInfoList[cellIndex].AddLast(being);
        }

        public void RemoveCellInfo_Being(Vector3Int cellIndex, Being being)
        {
            if (null == being) return;

            CellInfo cell = null;
            if (true == _cellInfoList.TryGetValue(cellIndex, out cell))
            {
                cell.Remove(being);
            }

        }



        //2d 좌표는 x,y만 사용한다. x,z좌표로의 변환은 허용 안한다 
        public Vector3Int ToPosition2D(Vector3 pos3d)
        {

            Vector3Int posXY_2d = Vector3Int.zero;

            //if (0 <= pos3d.x)
            //{
            //    //양수일때는 소수점을 버린다. 
            //    posXY_2d.x = (int)(pos3d.x / cellSize_x);
            //}
            //else
            //{
            //    //음수일때는 올림을 한다. 
            //    posXY_2d.x = (int)((pos3d.x / cellSize_x) - 0.9f);
            //}


            //if (0 <= pos3d.z)
            //{

            //    //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
            //    //posXY_2d.y = (int)(pos3d.z / cellSize_z); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다

            //    posXY_2d.y = Mathf.FloorToInt(pos3d.z / cellSize_z);

            //}
            //else
            //{
            //    posXY_2d.y = (int)((pos3d.z / cellSize_z) - 0.9f);
            //}

            //부동소수점 처리 문제로 직접계산하지 않는다 
            posXY_2d.x = Mathf.FloorToInt(pos3d.x / cellSize_x);
            posXY_2d.y = Mathf.FloorToInt(pos3d.z / cellSize_z);

            //posXY_2d = _tilemap_struct.WorldToCell(pos3d); //버림함수를 사용하여 계산하는 것과 결과가 달리 나온다 

            return posXY_2d;
        }

        public Vector3 ToCenter3D_FromPosition3D(Vector3 pos3d)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            Vector3Int cellPos = this.ToPosition2D(pos3d);

            return this.ToPosition3D_Center(cellPos);
        }

        //pos2d 는 항시 x,y공간만 사용한다. 다른 공간은 변환 허용안함.
        //grid 와 호환 안되는 함수 
        public Vector3 ToPosition3D_Center(Vector3Int posXY_2d)
        {
            Vector3 pos3d = Vector3.zero;

            {
                pos3d.x = (float)posXY_2d.x * cellSize_x;
                pos3d.z = (float)posXY_2d.y * cellSize_z;

                //셀의 중간에 위치하도록 한다
                pos3d.x += cellSize_x * 0.5f;
                pos3d.z += cellSize_z * 0.5f;
            }

            return pos3d;
        }

        public Vector3 ToPosition3D(Vector3Int posXY_2d)
        {
            Vector3 pos3d = Vector3.zero;

            {
                pos3d.x = (float)posXY_2d.x * cellSize_x;
                pos3d.z = (float)posXY_2d.y * cellSize_z;
            }

            return pos3d;
        }


        public int ToPosition1D(Vector3Int posXY_2d , int tileBlock_width_size)
        {
            //Assert.IsFalse(0 > posXY_2d.x || 0 > posXY_2d.y, "음수좌표값은 1차원값으로 변환 할 수 없다");
            if (0 > posXY_2d.x || 0 > posXY_2d.y) return -1;
            
            return (posXY_2d.x + posXY_2d.y * tileBlock_width_size); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Vector3Int ToPosition2D(int pos1d , int tileBlock_width_size)
        {
            //Assert.IsFalse(0 > pos1d, "음수좌표값은 2차원값으로 변환 할 수 없다");
            if (0 > pos1d) return Vector3Int.zero;

            Vector3Int posXY_2d = Vector3Int.zero;

            posXY_2d.x = pos1d % tileBlock_width_size;
            posXY_2d.y = pos1d / tileBlock_width_size;

            return posXY_2d;
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
        private uint _id_shot_sequence = 0; //발사체 아이디 생성기

        public Dictionary<uint, Being> _beings = new Dictionary<uint, Being>();
        public List<Being> _linearSearch_list = new List<Being>(); //충돌처리시 선형검색 속도를 높이기 위해 _beings 사전과 동일한 객체를 리스트에 넣음 

        //public Dictionary<uint, Shot> _shots = new Dictionary<uint, Shot>(); //발사체는 따로 관리한다
        public List<Shot> _shots = new List<Shot>();

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

            Create_ChampCamp(); //임시로 여기서 호출한다. 추후 스테이지 생성기로 옮겨야 한다 
            DebugWide.LogBlue("Start_ObjectManager !! ");

        }


        //객체간의 충돌검사 최적화를 위한 충돌가능객체군 미리 조직하기 
        //private void Update()
        private void FixedUpdate()
        {
            //UpdateCollision();

            //UpdateCollision_UseDictElementAt(); //obj100 : fps10
            //UpdateCollision_UseDictForeach(); //obj100 : fps60

            UpdateCollision_UseList(); //obj100 : fps80 , obj200 : fps40 , obj400 : fps15
            //UpdateCollision_UseGrid3x3(); //obj100 : fps65 , obj200 : fps40
            //UpdateCollision_UseDirectGrid3x3(); //obj100 : fps70 , obj200 : fps45 , obj400 : fps20

            if(null != SingleO.touchControl._selected)
            {
                Being selected = SingleO.touchControl._selected;
                SingleO.gridManager.Update_FogOfWar(selected.transform.position, selected._move._direction);
                selected.SetVisible(true);

                //챔프 시야에 없으면 안보이게 처리함 - 임시처리
                foreach (Being dst in _linearSearch_list)
                {
                    if (Being.eKind.barrel == dst._kind) continue; //술통은 항상 보이게 한다 -  임시 처리
                    if (dst == selected) continue;

                    dst.SetVisible(false);

                    if(true == IsVisibleArea(selected, dst.transform.position))
                    {
                        dst.SetVisible(true);
                    }

                }//end foreach
            }

        }

        public void UpdateCollision_UseDirectGrid3x3()
        {
            //return; //chamto test

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
                    //cellInfo = SingleO.gridManager.GetCellInfo(src._cellInfo._index);
                    if (null == cellInfo) continue;

                    foreach (Being dst in cellInfo)
                    {
                        //count++;
                        if (src == dst) continue;
                        if (null == dst || true == dst.isDeath()) continue;

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

        //챔프를 중심으로 3x3그리드 영역의 정보를 가지고 충돌검사한다
        public void UpdateCollision_UseGrid3x3() //3x3 => 5x5 => 7x7 ... 홀수로 그리드 범위를 늘려 테스트 해볼 수 있다
        {
            CellInfo cellInfo = null;
            foreach (Being src in _linearSearch_list)
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

        //딕셔너리 보다 인덱싱 속도가 빠르다. 안정적 객체수 : 500
        public void UpdateCollision_UseList()
        {

            Being src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < _linearSearch_list.Count - 1; i++)
            {
                for (int j = i + 1; j < _linearSearch_list.Count; j++)
                {
                    //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test
                    src = _linearSearch_list[i];
                    dst = _linearSearch_list[j];
                    CollisionPush(src, dst);
                    //CollisionPush(src.transform, dst.transform);
                }
            }

            //for (int i = 0; i < _transformList.Count - 1; i++)
            //{
            //    for (int j = i + 1; j < _transformList.Count; j++)
            //    {
                    
            //        CollisionPush(_transformList[i], _transformList[j]);
            //    }
            //}

        }

        public void CollisionPush(Transform src, Transform dst)
        {
            if (null == src || null == dst) return;

            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;
            float max_radius = Mathf.Max(0.5f, 0.5f);

            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            sqr_dis = src.position - dst.position;
            r_sum = 0.5f + 0.5f;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
            {
                //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                //todo : 최적화 필요 

                Vector3 n = sqr_dis.normalized;
                //Vector3 n = sqr_dis;
                float meterPerSecond = 2f;

                //2.큰 충돌원의 반지름 이상으로 겹쳐있는 경우
                if (sqr_dis.sqrMagnitude < Mathf.Pow(max_radius, 2))
                {
                    //3.완전 겹쳐있는 경우 , 방향값을 설정할 수 없는 경우
                    if (n == Vector3.zero)
                    {
                        //방향값이 없기 때문에 임의로 지정해 준다. 
                        n = Misc.GetDir8_Random_AxisY();
                    }

                    meterPerSecond = 0.5f;

                    //if(Being.eKind.spear != dst._kind)
                    //DebugWide.LogBlue(src.name + " " + dst.name + " : " + sqr_dis.magnitude + "  " + max_radius);
                }


                src.Translate(n * (GridManager.ONE_METER * 2f) * (Time.deltaTime * (1f/meterPerSecond)));
                dst.Translate(-n * (GridManager.ONE_METER * 2f) * (Time.deltaTime * (1f / meterPerSecond)));

            }
        }

        //가시영역 검사 
        public bool IsVisibleArea(Being src, Vector3 dstPos)
        {
            Vector3 dirToDst = dstPos - src.transform.position;
            float sqrDis = dirToDst.sqrMagnitude;
            if (sqrDis < Mathf.Pow(GridManager.MeterToWorld * 7f, 2))
            {

                //대상과 정반대 방향이 아닐때 처리 
                dirToDst.Normalize();
                if (Mathf.Cos(Mathf.Deg2Rad * 45f) < Vector3.Dot(src._move._direction, dirToDst) || 
                    sqrDis < Mathf.Pow(GridManager.MeterToWorld * 2f, 2))
                {
                    //보이는 위치의 타일인지 검사한다 
                    if(true == SingleO.gridManager.IsVisibleTile(src.transform.position,dstPos, 0.1f))
                        return true;
                }

            }

            return false;
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
            Vector3Int originToGridInt = SingleO.gridManager.ToPosition2D(origin);
            Vector3 originToPos = SingleO.gridManager.ToPosition3D(originToGridInt);
            Vector3 worldCellCenterPos = Vector3.zero;
            foreach (Vector3Int cellLBPos in SingleO.gridManager._indexesNxN[7])
            {
                //셀의 중심좌표로 변환 
                worldCellCenterPos = SingleO.gridManager.ToPosition3D_Center(cellLBPos);
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
            if (null == src || null == dst) return;

            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;
            //float max_radius = Mathf.Max(src.GetCollider_Radius(), dst.GetCollider_Radius());
            //float max_radius =  src._collider.radius > dst._collider.radius ? src._collider.radius : dst._collider.radius;
            //float max_radius = Mathf.Max(src._collider.radius, dst._collider.radius);
            //float max_radius = Mathf.Max(src.colliderRadius, dst.colliderRadius);
            float max_radius = Mathf.Max(src._colliderRadius, dst._colliderRadius);
            //float max_radius = Mathf.Max(0.5f, 0.3f);

            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            sqr_dis = src.transform.localPosition - dst.transform.localPosition;
            //r_sum = src.GetCollider_Radius() + dst.GetCollider_Radius();
            r_sum = src._colliderRadius + dst._colliderRadius;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
            {
                //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

                //todo : 최적화 필요 

                Vector3 n = sqr_dis.normalized;
                //Vector3 n = sqr_dis;
                float meterPerSecond = 2f;

                //2.큰 충돌원의 반지름 이상으로 겹쳐있는 경우
                if (sqr_dis.sqrMagnitude < Mathf.Pow(max_radius, 2))
                {
                    //3.완전 겹쳐있는 경우 , 방향값을 설정할 수 없는 경우
                    if (n == Vector3.zero)
                    {
                        //방향값이 없기 때문에 임의로 지정해 준다. 
                        n = Misc.GetDir8_Random_AxisY();
                    }

                    meterPerSecond = 0.5f;

                    //if(Being.eKind.spear != dst._kind)
                        //DebugWide.LogBlue(src.name + " " + dst.name + " : " + sqr_dis.magnitude + "  " + max_radius);
                }

                //밀리는 처리 
                //if(Being.eKind.barrel !=  src._kind )
                //  src.Move_Push(n, meterPersecond);

                //if (Being.eKind.barrel != dst._kind)
                    //dst.Move_Push(-n, meterPersecond);

                src.OnCollision_MovePush(dst, n, meterPerSecond);
                dst.OnCollision_MovePush(src, -n, meterPerSecond);

                //src.transform.Translate(n * (GridManager.ONE_METER * 2f) * (Time.deltaTime * (1f / meterPerSecond)));
                //dst.transform.Translate(-n * (GridManager.ONE_METER * 2f) * (Time.deltaTime * (1f / meterPerSecond)));

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
            Vector3 sqr_dis = src.transform.localPosition - structTile._center_3d;
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
                        n = Misc.GetDir8_Random_AxisY();
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
            Vector3 centerToSrc_dir = srcPos - structTile._center_3d;
            Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(structTile._dir);

            float size = SingleO.gridManager.cellSize_x * 0.5f;
            Vector3 center = Vector3.zero;
            LineSegment3 line3 = new LineSegment3();
            //8방향별 축값 고정  
            switch (structTile._dir)
            {
                case eDirection8.up:
                    {
                        srcPos.z = structTile._center_3d.z + size;
                    }
                    break;
                case eDirection8.down:
                    {
                        srcPos.z = structTile._center_3d.z - size;
                    }
                    break;
                case eDirection8.left:
                    {
                        srcPos.x = structTile._center_3d.x - size;
                    }
                    break;
                case eDirection8.right:
                    {
                        srcPos.x = structTile._center_3d.x + size;
                    }
                    break;
                case eDirection8.leftUp:
                    {
                        //down , right
                        if(StructTile.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._center_3d.x - size;
                            srcPos.z = structTile._center_3d.z + size;
                            break;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._center_3d;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._center_3d;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        //down , left
                        if (StructTile.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._center_3d.x + size;
                            srcPos.z = structTile._center_3d.z + size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._center_3d;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._center_3d;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        //up , right
                        if (StructTile.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._center_3d.x - size;
                            srcPos.z = structTile._center_3d.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;

                        center = structTile._center_3d;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._center_3d;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.rightDown:
                    {
                        //up , left
                        if (StructTile.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._center_3d.x + size;
                            srcPos.z = structTile._center_3d.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._center_3d;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._center_3d;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;

            }

            src.transform.position = srcPos;

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
        public ChampUnit GetNearCharacter(ChampUnit src, Camp.eRelation vsRelation, float meter_minRadius, float meter_maxRadius)
        {
            if (null == src) return null;

            float wrd_minRad = meter_minRadius * GridManager.ONE_METER;
            float wrd_maxRad = meter_maxRadius * GridManager.ONE_METER;
            float sqr_minRadius = 0;
            float sqr_maxRadius = 0;
            float min_value = wrd_maxRad * wrd_maxRad * 1000f; //최대 반경보다 큰 최대값 지정
            float sqr_dis = 0f;


            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint NxN = SingleO.gridManager.GetNxNIncluded_CircleRadius(wrd_maxRad);

            //int count = 0;
            CellInfo cellInfo = null;
            ChampUnit target = null;
            foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[ NxN ])
            {
                cellInfo = SingleO.gridManager.GetCellInfo(ix + src._cellInfo._index);

                if (null == cellInfo) continue;

                foreach (Being dst in cellInfo)
                {
                    ChampUnit champDst = dst as ChampUnit;
                    if (null == champDst) continue;
                    if (src == dst) continue;
                    if (true == dst.isDeath()) continue; //쓰러진 객체는 처리하지 않는다 


                    if(vsRelation != Camp.eRelation.Unknown && null != src._belongCamp && null != champDst._belongCamp)
                    {
                        Camp.eRelation getRelation = SingleO.campManager.GetRelation(src._belongCamp.campKind, champDst._belongCamp.campKind);

                        //요청 관계가 아니면 처리하지 않는다 
                        if (vsRelation != getRelation)
                            continue;
                    }

                    //count++;
                    //==========================================================
                    sqr_minRadius = Mathf.Pow(wrd_minRad + dst.GetCollider_Radius(), 2);
                    sqr_maxRadius = Mathf.Pow(wrd_maxRad + dst.GetCollider_Radius(), 2);
                    sqr_dis = (src.transform.position - dst.transform.position).sqrMagnitude;

                    //최대 반경 이내일 경우
                    if (sqr_minRadius <= sqr_dis && sqr_dis <= sqr_maxRadius)
                    {

                        //DebugWide.LogBlue(min_value + "__" + sqr_dis +"__"+  dst.name); //chamto test

                        //기존 객체보다 더 가까운 경우
                        if (min_value > sqr_dis)
                        {
                            min_value = sqr_dis;
                            target = champDst;
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

        int __shotNextCount = -1;
        public Shot GetNextShot()
        {

            //최대 샷 개수만큼만 사용안하는 샷을 찾는다
            for (int i = 0; i < _shots.Count; i++)
            {
                __shotNextCount++;
                //__shotNextCount = 0; //chamto test
                __shotNextCount %= _shots.Count;

                if (false == _shots[__shotNextCount]._on_theWay)
                {
                    //사용안하면 바로 반환한다 
                    return _shots[__shotNextCount];
                }
            }

            return null;



            //foreach(Shot shot in _shots.Values)
            //{
            //    if(false == shot._on_theWay)    
            //    {
            //        return shot;
            //    }
            //}
            //return null;
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



        public ChampUnit Create_Character(Transform parent, Being.eKind eKind, Camp belongCamp, Vector3 pos)
        {
            _id_sequence++;

            GameObject obj = CreatePrefab(eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            ChampUnit cha = obj.AddComponent<ChampUnit>();
            obj.AddComponent<Movement>();
            obj.AddComponent<AI>();
            cha._id = _id_sequence;
            cha._kind = eKind;
            cha._belongCamp = belongCamp;
            cha.transform.localPosition = pos;
            cha.Init();

            _beings.Add(_id_sequence,cha);
            _linearSearch_list.Add(cha); //속도향상을 위해 중복된 데이터 추가

            return cha;
        }

        public Shot Create_Shot(Transform parent, Being.eKind eKind,  Vector3 pos)
        {
            _id_shot_sequence++;

            GameObject obj = CreatePrefab("Shot/" + eKind.ToString(), parent, _id_shot_sequence.ToString("000") + "_" + eKind.ToString());
            Shot shot = obj.AddComponent<Shot>();
            obj.AddComponent<Movement>();
            shot._id = _id_shot_sequence;
            shot._kind = eKind;
            shot.transform.localPosition = pos;
            shot.Init();

            //_shots.Add(_id_shot_sequence, shot);
            _shots.Add(shot);

            return shot;
        }

        public Obstacle Create_Obstacle(Transform parent, Being.eKind eKind, Vector3 pos)
        {
            _id_sequence++;

            GameObject obj = CreatePrefab("Obstacle/" + eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            Obstacle obst = obj.AddComponent<Obstacle>();
            obj.AddComponent<Movement>();
            obst._id = _id_sequence;
            obst._kind = eKind;
            obst.transform.localPosition = pos;
            obst.Init();

            _beings.Add(_id_sequence, obst);
            _linearSearch_list.Add(obst);

            return obst;
        }


        List<Transform> _transformList = new List<Transform>();
        public Transform Create_Test(Transform parent, Vector3 pos , bool manager)
        {
            
            GameObject obj = CreatePrefab("test", parent,"test");
            obj.transform.localPosition = pos;

            if(true == manager)
                _transformList.Add(obj.transform);

            return obj.transform;
        }
        public Transform Create_Test2(Transform parent, Vector3 pos)
        {

            GameObject obj = CreatePrefab("test", parent, "test");
            Being being = obj.AddComponent<Being>();
            obj.AddComponent<Movement>();
            being.transform.localPosition = pos;
            being._id = _id_sequence;
            being.Init();


            _linearSearch_list.Add(being);

            return obj.transform;
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

            string Blue_CampName = "Champ_Sky";
            string White_CampName = "Skel_Gray";
            string Obstacle_CampName = "ExitGoGe";
            int camp_position = 0;

            SingleO.campManager.Load_CampPlacement(Camp.eKind.Blue);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.White);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.Obstacle);
            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Blue, Camp.eKind.White);

            Camp camp_BLUE = SingleO.campManager.GetCamp(Camp.eKind.Blue, Blue_CampName.GetHashCode());
            Camp camp_WHITE = SingleO.campManager.GetCamp(Camp.eKind.White, White_CampName.GetHashCode());
            Camp camp_Obstacle = SingleO.campManager.GetCamp(Camp.eKind.Obstacle, Obstacle_CampName.GetHashCode());
            Being being = null;
            ChampUnit champ = null;

            // -- 블루 진형 --
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.lothar, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.footman, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.spearman, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ._mt_range_min = 1f;
            //champ._mt_range_max = 8f;
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.conjurer, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.knight, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //for (int i = 0; i < 10; i++)
            //{
            //    champ = Create_Character(SingleO.unitRoot, Being.eKind.spearman, camp_BLUE, camp_BLUE.RandPosition());
            //    champ._mt_range_min = 2f;
            //    champ._mt_range_max = 6f;
            //    champ.GetComponent<AI>()._ai_running = true;
            //    camp_position++;
            //}

            //===================================================

            // -- 휜색 진형 --
            camp_position = 0;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.raider, camp_WHITE, camp_WHITE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            for (int i = 0; i < 0; i++)
            { 
                champ = Create_Character(SingleO.unitRoot, Being.eKind.spearman, camp_WHITE, camp_WHITE.RandPosition());
                champ._mt_range_min = 2f;
                champ._mt_range_max = 6f;
                champ.GetComponent<AI>()._ai_running = true;
                camp_position++;
                //champ.SetColor(Color.black);
            }

            for (int i = 0; i < 0; i++)
            {
                champ = Create_Character(SingleO.unitRoot, Being.eKind.ogre, camp_WHITE, camp_WHITE.RandPosition());
                champ.GetComponent<AI>()._ai_running = true;
                camp_position++;
            }

            //===================================================

            // -- 장애물 진형 --
            for (int i = 0; i < 0;i++)
            {
                Create_Obstacle(SingleO.unitRoot, Being.eKind.barrel, camp_Obstacle.RandPosition());
            }

            //===================================================

            // -- 발사체 미리 생성 --

            for (int i = 0; i < 0;i++)
            {
                being = Create_Shot(SingleO.shotRoot, Being.eKind.spear, Vector3.zero);
            }

            //===================================================
            // -- 테스트 객체 생성 --
            for (int i = 0; i < 100;i++)
            {
                Create_Test2(SingleO.unitRoot, camp_Obstacle.RandPosition());
                //Create_Test(SingleO.shotRoot, camp_Obstacle.RandPosition(), false);
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
        private ChampUnit _me = null;
        //private Being _target = null;
        private Vector3 _ai_Dir = Vector3.zero;
        private float _elapsedTime = 0f;


        public bool _ai_running = false;

        private void Start()
        {
            _me = GetComponent<ChampUnit>();
            _ai_Dir = Misc.GetDir8_Random_AxisY(); //초기 임의의 방향 설정
        }



        private void FixedUpdate()
        {
            //_me.Attack(_me._move._direction); //chamto test

            if (false == _ai_running) return;

            if (true == _me.isDeath()) return;

            this.StateUpdate();
        }


        public bool Situation_Is_Enemy()
        {
            ChampUnit champTarget = _me._looking as ChampUnit;
            if (null == champTarget ) return false;

            //불확실한 대상
            if (null == _me._looking || null == champTarget._belongCamp || null == _me._belongCamp) return false;

            Camp.eRelation relation = SingleO.campManager.GetRelation(_me._belongCamp.campKind, champTarget._belongCamp.campKind);

            if (Camp.eRelation.Enemy == relation) return true;

            return false;
        }


        private const int _FAILURE = -1;
        private const int _OUT_RANGE_MIN = 0;
        private const int _OUT_RANGE_MAX = 1;
        private const int _IN_RANGE = 2;
        public int Situation_Is_InRange(float meter_rangeMin, float meter_rangeMax)
        {
            if (null == _me._looking) return _FAILURE;

            float sqrDis = (_me.transform.position - _me._looking.transform.position).sqrMagnitude;

            float sqrRangeMax = Mathf.Pow(meter_rangeMax * GridManager.ONE_METER , 2);
            float sqrRangeMin = Mathf.Pow(meter_rangeMin * GridManager.ONE_METER , 2);

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
            if (null == _me._looking) return _FAILURE;

            float sqrDis = (_me.transform.position - _me._looking.transform.position).sqrMagnitude;

            float sqrRangeMax = Mathf.Pow(_me.attack_range_max + _me._looking.GetCollider_Radius() , 2);
            float sqrRangeMin = Mathf.Pow(_me.attack_range_min + _me._looking.GetCollider_Radius() , 2);

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

                        }else
                        {
                            //대상이 보이는 위치에 있는지 검사한다 
                            if (false == SingleO.objectManager.IsVisibleArea(_me, _me._looking.transform.position))
                            {
                                //대상이 안보이면 다시 배회하기 
                                _state = eState.Roaming;
                                break;
                            }


                            //공격사거리 안에 들어오면 공격한다 
                            result = Situation_Is_AttackInRange();
                            if (_IN_RANGE == result)
                            {
                                _me.Attack(_me._looking.transform.position - _me.transform.position);
                                //_state = eState.Attack;
                                break;
                                //DebugWide.LogBlue("attack");
                            }

                            Vector3 moveDir = _me._looking.transform.position - _me.transform.position;
                            float second = 0.7f;
                            bool foward = true;
                            //상대와 너무 붙어 최소공격사거리 조건에 안맞는 경우 
                            if(_OUT_RANGE_MIN == result)
                            {
                                moveDir *= -1f; //반대방향으로 바꾼다
                                second = 2f;
                                foward = false; //뒷걸음질 
                            }


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
                        //DebugWide.LogBlue("Roaming: " + _target);

                        //대상이 보이는 위치에 있는지 검사한다 
                        if(null != _me._looking)
                        {
                            if (false == SingleO.objectManager.IsVisibleArea(_me, _me._looking.transform.position))
                            {
                                //안보이는 위치면 대상을 해제한다 
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
                        if(1f <= _elapsedTime)
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
            ChampUnit champ = null;
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
                    champ = _selected as ChampUnit;
                    if(null != champ)
                    {
                        champ.GetComponent<AI>()._ai_running = true;
                        SingleO.lineControl.SetActive(champ._UIID_circle_collider, false);
                    }
                        

                }

                //새로운 객체 선택
                _selected = getBeing;

                champ = _selected as ChampUnit;
                if (null != champ)
                {
                    _selected.GetComponent<AI>()._ai_running = false;
                    SingleO.lineControl.SetActive(champ._UIID_circle_collider, true);
                }

                SingleO.cameraWalk.SetTarget(_selected.transform);
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
            if (null == _selected) return;

            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            Vector3 touchDir = hit.point - _selected.transform.position;

            //_selected.Attack(hit.point - _selected.transform.position);
            //_selected.Block_Forward(hit.point - _selected.transform.position);
            _selected.Move_Forward(touchDir, 1f, true);

            ChampUnit champSelected = _selected as ChampUnit;
            if (null != champSelected )
            {
                //임시처리
                Being target = SingleO.objectManager.GetNearCharacter(champSelected, Camp.eRelation.Unknown, 
                                                                      champSelected.attack_range_min, champSelected.attack_range_max);
                if(null != target)
                {
                    if (true == SingleO.objectManager.IsVisibleArea(champSelected, target.transform.position))
                    {
                        champSelected.Attack(target.transform.position - _selected.transform.position, target);
                    }

                    //_selected.Move_Forward(hit.point - _selected.transform.position, 3f, true); 
                        
                }

                //.Attack(champSelected._move._direction); //chamto test

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

        //private GameObject _TouchedObject = null;
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
