using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
//using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.U2D;

using UtilGS9;

namespace HordeFight
{
    public class HordeFight_Main : MonoBehaviour
    {
        
        // Use this for initialization
        void Start()
        {
            Misc.Init();

            SingleO.Init(gameObject); //싱글톤 객체 생성 , 초기화 


            SingleO.debugViewer._origin = SingleO.hierarchy.GetTransformA("z_debug/origin");
            SingleO.debugViewer._target = SingleO.hierarchy.GetTransformA("z_debug/target");
            //===================

            //SingleO.objectManager.Create_Characters(); //여러 캐릭터들 테스트용
            //SingleO.objectManager.Create_ChampCamp();

        }


        // Update is called once per frame
        //void Update()
        //{
        //}


        //void OnGUI()
        //{
        //    //if (GUI.Button(new Rect(10, 10, 200, 100), new GUIContent("Refresh Timemap Fog of War")))
        //    //{
        //    //    //RuleExtraTile ruleTile =  SingleO.gridManager.GetTileMap_Struct().GetTile<RuleExtraTile>(new Vector3Int(0, 0, 0));
        //    //    SingleO.gridManager.GetTileMap_FogOfWar().RefreshAllTiles();
        //    //    //DebugWide.LogBlue("TileMap_Struct RefreshAllTiles");
        //    //}
        //}


    }

}

//========================================================
//==================      전역  객체      ==================
//========================================================
namespace HordeFight
{
    
    //==================================================================
    //S<CellSpacePartition>.Create();
    //S<ResourceManager>.Create();
    //S<HierarchyPreLoader>.Create();
    //S<WideCoroutine>.Create();
    //S<HashToStringMap>.Create();

    //S<GridManager>.inst     = gameObject.AddComponent<GridManager>();
    //S<CampManager>.inst     = gameObject.AddComponent<CampManager>();
    //S<ObjectManager>.inst   = gameObject.AddComponent<ObjectManager>();
    //S<PathFinder>.inst      = gameObject.AddComponent<PathFinder>();
    //S<UI_Main>.inst         = gameObject.AddComponent<UI_Main>();
    //S<DebugViewer>.inst     = gameObject.AddComponent<DebugViewer>();
    //S<LineControl>.inst     = gameObject.AddComponent<LineControl>();
    //S<CameraWalk>.inst      = gameObject.AddComponent<CameraWalk>();
    //S<TouchEvent>.inst      = gameObject.AddComponent<TouchEvent>();
    //S<TouchControl>.inst    = gameObject.AddComponent<TouchControl>();

    //S<GameObject>.mainCamera    = S<GameObject>.FindHierarchy<Camera>("Main Camera");
    //S<GameObject>.canvasRoot    = S<GameObject>.FindHierarchy<Canvas>("Canvas");
    //S<GameObject>.gridRoot      = S<GameObject>.FindHierarchy<Transform>("0_grid");
    //S<GameObject>.unitRoot      = S<GameObject>.FindHierarchy<Transform>("0_unit");
    //S<GameObject>.shotRoot      = S<GameObject>.FindHierarchy<Transform>("0_shot");

    //S<HierarchyPreLoader>.inst.GetTransform("z_debug").gameObject.AddComponent<DebugViewer>();
    //S<DebugViewer>.inst._origin = S<HierarchyPreLoader>.inst.GetTransform("z_debug/origin");

    //S<HierarchyPreLoader>.inst.Init();
    //S<ResourceManager>.inst.Init();
    //==================================================================
    //public static class S<T> where T : class, new()
    //{
    //    static public T inst = null;

    //    //ex:  S<CellSpacePartition>.i.Create();
    //    static public void Create()
    //    {
    //        S<T>.inst = new T();
    //    }

    //    //ex:  S<TouchEvent>.i = S<TouchEvent>.FindMono<TouchEvent>();
    //    static public MONO FindMono<MONO>() where MONO : MonoBehaviour
    //    {
    //        return (MONO)MonoBehaviour.FindObjectOfType(typeof(MONO));
    //    }

    //    //ex:  S<GameObject>.mainCamera = S<GameObject>.FindHierarchy<Camera>("Main Camera");
    //    static public  GT FindHierarchy<GT>(string name) where GT : class
    //    {
    //        if (typeof(T) != typeof(GameObject)) 
    //            return null;

    //        GameObject obj = GameObject.Find(name);
    //        if (null != obj)
    //        {
    //            return obj.GetComponent<GT>();
    //        }
    //        return null;

    //    } 

    //    static public Camera mainCamera = null; //"Main Camera"  
    //    static public Canvas canvasRoot = null; //"Canvas"
    //    static public Transform gridRoot = null; //"0_grid
    //    static public Transform unitRoot = null; //"0_unit"
    //    static public Transform shotRoot = null; //"0_shot
    //}

    public static class SingleO
    {
        
        static public  GT FindHierarchy<GT>(string name) where GT : class
        {
            
            GameObject obj = GameObject.Find(name);
            if (null != obj)
            {
                return obj.GetComponent<GT>();
            }
            return null;
        } 

        public static void Init(GameObject parent)
        {

            DateTime _startDateTime;
            string _timeTemp = "";

            //==============================================

            mainCamera = FindHierarchy<Camera>("Main Camera");
            canvasRoot = FindHierarchy<Canvas>("Canvas");
            gridRoot = FindHierarchy<Transform>("0_grid");
            unitRoot = FindHierarchy<Transform>("0_unit");
            shotRoot = FindHierarchy<Transform>("0_shot");
            debugRoot = FindHierarchy<Transform>("z_debug");
            lightDir = FindHierarchy<Transform>("light_dir");
            groundY = FindHierarchy<Transform>("groundY");

            //==============================================

            _startDateTime = DateTime.Now;
            SingleO.hierarchy.Init(); //계층도 읽어들이기 
            _timeTemp += "  hierarchy.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            _startDateTime = DateTime.Now;
            SingleO.resourceManager.Init(); //스프라이트 로드 
            ResolutionController.CalcViewportRect(SingleO.canvasRoot, SingleO.mainCamera); //화면크기조정
            _timeTemp += "  resourceManager. CalcViewportRect  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================

            cameraWalk = parent.AddComponent<CameraWalk>();
            touchEvent = parent.AddComponent<TouchEvent>();
            touchControl = parent.AddComponent<TouchControl>();
            uiMain = parent.AddComponent<UI_Main>();
            lineControl = parent.AddComponent<LineControl>();
            debugViewer = debugRoot.gameObject.AddComponent<DebugViewer>();

            //==============================================

            _startDateTime = DateTime.Now;
            gridManager = parent.AddComponent<GridManager>();
            gridManager.Init();
            _timeTemp += "  GridManager.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            _startDateTime = DateTime.Now;
            objectManager = parent.AddComponent<ObjectManager>();
            objectManager.Init();
            _timeTemp += "  ObjectManager.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


            _startDateTime = DateTime.Now;
            pathFinder = parent.AddComponent<PathFinder>();
            pathFinder.Init();
            _timeTemp += "  PathFinder.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            DebugWide.LogBlue(_timeTemp);
            //==============================================

        }


        //유니티 객체 
        static public Camera mainCamera = null; //"Main Camera"  
        static public Canvas canvasRoot = null; //"Canvas"
        static public Transform gridRoot = null; //"0_grid
        static public Transform unitRoot = null; //"0_unit"
        static public Transform shotRoot = null; //"0_shot
        static public Transform debugRoot = null; //"z_debug
        static public Transform lightDir = null; //"light_dir"
        static public Transform groundY = null; //"groundY"

        //모노 객체 
        public static ObjectManager objectManager = null;
        public static GridManager gridManager = null;
        public static PathFinder pathFinder = null;
        public static CameraWalk cameraWalk = null;
        public static TouchEvent touchEvent = null;
        public static TouchControl touchControl = null;
        public static UI_Main uiMain = null;
        public static LineControl lineControl = null;
        public static DebugViewer debugViewer = null;


        //일반 객체
        public static CampManager campManager = CSingleton<CampManager>.Instance;
        public static CellSpacePartition cellPartition = CSingleton<CellSpacePartition>.Instance;
        public static ResourceManager resourceManager = CSingleton<ResourceManager>.Instance;
        public static HierarchyPreLoader hierarchy = CSingleton<HierarchyPreLoader>.Instance;
        public static WideCoroutine coroutine = CSingleton<WideCoroutine>.Instance;
        public static HashToStringMap hashMap = CSingleton<HashToStringMap>.Instance;

    }

}//end namespace


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
            if (null != game)
            {
                _tilemap = game.GetComponent<Tilemap>();
            }

        }

        public void ResetColor()
        {
            foreach (Vector3Int i3 in _colorMap.Keys)
            {
                _tilemap.SetColor(i3, Color.white);
            }
        }
        public void SetColor(Vector3 pos, Color c)
        {
            Vector3Int i3 = _tilemap.WorldToCell(pos);
            _tilemap.SetTileFlags(i3, UnityEngine.Tilemaps.TileFlags.None);
            _tilemap.SetColor(i3, c);

            if (!_colorMap.Keys.Contains(i3))
                _colorMap.Add(i3, c);
        }

        public void SetPos(Vector3 origin, Vector3 target)
        {
            _origin.position = origin;
            _target.position = target;
        }


        private Vector3 _start = ConstV.v3_zero;
        private Vector3 _end = ConstV.v3_right;
        public void DrawLine(Vector3 start, Vector3 end)
        {
            _start = start;
            _end = end;
        }

        public void UpdateDraw_IndexesNxN()
        {
#if UNITY_EDITOR
            Index2 prev = ConstV.id2_zero;
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(7, ConstV.v3_up))
            foreach (Index2 cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter_Tornado(11, 11))
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter(7, ConstV.v3_up))
            {
                //DebugWide.LogBlue(v);
                //Debug.DrawLine(cur - Vector3.one * 0.3f, cur + Vector3.one * 0.3f, Color.red);
                //Debug.DrawLine(prev, cur);

                //UnityEditor.Handles.Label(cur, "( " + cur.x + " , " + cur.z + " )");

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
            Vector3 pos3d = ConstV.v3_zero;
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
            foreach (CellSpace t in SingleO.gridManager._structTileList.Values)
            {
                if (eDirection8.none == t._eDir) continue;

                //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
                //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z

                ccc = Color.white;
                if (CellSpace.Specifier_DiagonalFixing == t._specifier)
                {
                    ccc = Color.red;
                }

                Vector3 end = t._pos3d_center + Misc.GetDir8_Normal3D_AxisY(t._eDir) * 0.12f;
                Debug.DrawLine(t._pos3d_center, end, ccc);


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

        public void Update_DrawEdges(bool debugLog)
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
            float size = SingleO.gridManager._cellSize_x;
            Vector3 start = ConstV.v3_zero;
            Vector3 end = ConstV.v3_zero;
            Vector3 xz_length = new Vector3(6.5f, 0, 4f);

            end.z = xz_length.z;
            for (int x = 0; size * x < xz_length.x; x++)
            {
                start.x = size * x;
                end.x = size * x;

                Debug.DrawLine(start, end, Color.white);
            }

            start = end = ConstV.v3_zero;
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
            if (0.01f >= length_interval)
            {
                return;
            }

            target = SingleO.gridManager.ToCenter3D_FromPosition3D(target);

            Vector3Int orgin_2d = SingleO.gridManager.ToPosition2D(origin);
            Vector3 origin_center = SingleO.gridManager.ToPosition3D_Center(orgin_2d);
            Vector3 line = target - origin_center;
            Vector3 n = line.normalized;

            CellSpace structTile = null;
            Vector3 next = ConstV.v3_zero;
            string keyword = "";
            for (int i = 0; line.sqrMagnitude > next.sqrMagnitude; i++)
            {
                //========================================================================
                Vector3 cr = Vector3.Cross(n, ConstV.v3_up);
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
            CellSpace structTile = null;
            if (SingleO.gridManager.HasStructTile(origin_3d, out structTile))
            {
                switch (structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_3d_center.x += dir.x * SingleO.gridManager._cellSize_x * 0.5f;
                            origin_3d_center.z += dir.y * SingleO.gridManager._cellSize_z * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            origin_3d_center = SingleO.gridManager.ToPosition3D_Center(origin_2d + Misc.GetDir8_Normal2D(structTile._eDir));
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

            UnityEditor.Handles.Label(origin_3d_center + next, "_________end :" + count);

            Debug.DrawLine(origin_3d_center, target_3d, Color.green);
            //return true;
#endif
        }


        public void Draw_LookAtChamp()
        {
            foreach (Being bbb in SingleO.objectManager._linearSearch_list)
            {
                ChampUnit unit = bbb as ChampUnit;
                if (null != unit)
                {
                    if (unit.isDeath()) continue;

                    Color cc = Color.red;
                    if (Camp.eKind.Blue == unit._campKind) cc = Color.blue;
                    if (Camp.eKind.White == unit._campKind) cc = Color.white;

                    //chamto test - 
                    if (1 == unit._id)
                    {
                        unit._hp_max = 10000;
                        unit._hp_cur = 10000;
                        DebugWide.DrawCircle(unit.transform.position, 5f, cc); //탐지사정거리 
                    }


                    if (null != unit._looking)
                    {
                        if (unit._looking.isDeath()) continue;
                        Debug.DrawLine(unit.transform.position, unit._looking.transform.position, cc);
                        DebugWide.DrawCircle(unit.transform.position, 0.3f, cc);
                    }

                }
            }
        }

        void OnDrawGizmos()
        {
            //Misc.DrawDirN();
            //UnityEditor.Handles.Label(ConstV.v3_zero, "0,0");
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


            //==============================================
            //구트리 테스트 
            //SingleO.objectManager.GetSphereTree_Being().Render_Debug(3, false); 
            //SingleO.objectManager.GetSphereTree_Being().Render_RayTrace(_origin.position, _target.position);
            //SingleO.objectManager.GetSphereTree_Struct().Render_RayTrace(_origin.position, _target.position);
            //float radius = (_origin.position - _target.position).magnitude;
            //SingleO.objectManager.GetSphereTree().Render_RangeTest(_origin.position,radius);
            //==============================================

            //Draw_LookAtChamp();

           // SingleO.cellPartition.DebugPrint();
        }


    }
}


//========================================================
//==================     리소스 관리기     ==================
//========================================================

namespace HordeFight
{
    
    public class ResourceManager
    {
        
        public RuntimeAnimatorController _base_Animator = null;

        //해쉬맵 : 애니메이션 이름으로 해쉬키를 생성
        private Dictionary<int, AnimationClip> _hashKeyClips = new Dictionary<int, AnimationClip>();

        //Being.eKind, eAniBaseKind, eDirection8 : 3가지 값으로 키를 생성
        private Dictionary<uint, AnimationClip> _multiKeyClips = new Dictionary<uint, AnimationClip>();

        //기본 동작 AniClip 목록 : base_idle , base_move , base_attack , base_fallDown
        private AnimationClip[] _baseAniClips = null;

        public Dictionary<int, Sprite> _sprEffect = new Dictionary<int, Sprite>();
        public Dictionary<int, Sprite> _sprIcons = new Dictionary<int, Sprite>();
        public Dictionary<int, TileBase> _tileScripts = new Dictionary<int, TileBase>();

        public SpriteAtlas _atlas_etc = null;

        //==================== Get / Set ====================

        public AnimationClip GetBaseAniClip(eAniBaseKind baseKind)
        {
            if (eAniBaseKind.MAX <= baseKind) return null;

            return _baseAniClips[(int)baseKind];
        }

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
                _hashKeyClips.Add(ac.name.GetHashCode(), ac);

                uint multiKey = ComputeAniMultiKey(ac.name);
                if(0 != multiKey) //0 멀티키는 키생성에 문제가 있다는 것이다  
                {
                    _multiKeyClips.Add(multiKey, ac);
                }
                //else
                //{
                //    DebugWide.LogBlue(ac.name); //chamto test
                //}

            }
            _baseAniClips = ConstV.FindAniBaseClips(loaded);


            //DebugWide.LogBlue(spriteAtlas.spriteCount);
            //spriteAtlas.GetSprite()

            _atlas_etc = Resources.Load<SpriteAtlas>("Warcraft/Textures/Atlas/etc");

            //Sprite[] spres = Resources.LoadAll<Sprite>("Warcraft/Textures/ETC/effect");
            //foreach (Sprite spr in spres)
            //{
            //    _sprEffect.Add(spr.name.GetHashCode(), spr);
            //}

            //Sprite[] spres = Resources.LoadAll<Sprite>("Warcraft/Textures/ETC/Icons");
            //foreach(Sprite spr in spres)
            //{
            //    _sprIcons.Add(spr.name.GetHashCode(), spr);
            //}


            TileBase[] tiles = Resources.LoadAll<TileBase>("Warcraft/Palette/ScriptTile");
            foreach(TileBase r in tiles)
            {
                _tileScripts.Add(r.name.GetHashCode(), r);
                //DebugWide.LogBlue(r.name);
            }
        }

        public Sprite GetSprite_Effect(string spr_name)
        {
            int hash = spr_name.GetHashCode();
            if(false == _sprEffect.Keys.Contains(hash))
            {
                Sprite sprite = _atlas_etc.GetSprite(spr_name);
                if(null != sprite)
                    _sprEffect.Add(hash, sprite);    
            }

            return _sprEffect[hash];
        }

        public Sprite GetSprite_Icons(string spr_name)
        {
            int hash = spr_name.GetHashCode();
            if (false == _sprIcons.Keys.Contains(hash))
            {
                Sprite sprite = _atlas_etc.GetSprite(spr_name);
                if (null != sprite)
                    _sprIcons.Add(hash, sprite);
            }

            return _sprIcons[hash];
        }

        public TileBase GetTileScript(int nameToHash)
        {
            if(true == _tileScripts.ContainsKey(nameToHash))
            {
                return _tileScripts[nameToHash];
            }

            return null;
        }

        public AnimationClip GetClip(int nameToHash)
        {
            AnimationClip animationClip = null;
            _hashKeyClips.TryGetValue(nameToHash, out animationClip);

            //DebugWide.LogRed(animationClip + "   " + Single.resourceManager._aniClips.Count); //chamto test


            return animationClip;
        }

        public AnimationClip GetClip(Being.eKind being_kind,  eAniBaseKind ani_kind, eDirection8 dir)
        {
            uint multiKey = ComputeAniMultiKey(being_kind, ani_kind, dir);
            AnimationClip animationClip = null;
            _multiKeyClips.TryGetValue(multiKey, out animationClip);

            return animationClip;
        }

        public Being.eKind StringTo_BeingKind(string str)
        {
            return (Being.eKind)Enum.Parse(typeof(Being.eKind), str, true);
        }

        public eAniBaseKind StringTo_AniBaseKind(string str)
        {
            return (eAniBaseKind)Enum.Parse(typeof(eAniBaseKind), str, true); 

        }

        public eDirection8 StringTo_Direction8(string str)
        {
            return (eDirection8)Enum.Parse(typeof(eDirection8), str, true);
        }

        public uint ComputeAniMultiKey(string aniName)
        {
            //DebugWide.LogBlue(aniName); //chamto test

            string[] temps = aniName.Split('_');

            if (3 != temps.Length) 
            {
                return 0;
            }

            try
            {
                Being.eKind being_kind = StringTo_BeingKind(temps[0]);
                eAniBaseKind ani_kind = StringTo_AniBaseKind(temps[1]);
                eDirection8 dir = StringTo_Direction8(temps[2]);

                //DebugWide.LogBlue(aniName + " : " + being_kind.ToString() + "  " + ani_kind.ToString() + "  " + dir.ToString() + " : mkey : " + ComputeAniMultiKey(being_kind, ani_kind, dir)); //chamto test

                return ComputeAniMultiKey(being_kind, ani_kind, dir);
            }
            catch (ArgumentException e)
            {
                //Enum.Parse 수행시 들어온 문자열 값에 해당하는 열거형 값이 없을 경우, 이 예외가 발생한다 
                //DebugWide.LogException(e);
                return 0;
            }

        }

        public uint ComputeAniMultiKey(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir)
        {
            //being_kind 천의 자리 : 최대 99999개
            //ani_kind 십의 자리 : 최대 99개
            //dir 일의 자리 : 최대 9개

            return (uint)being_kind * 1000 + (uint)ani_kind * 10 + (uint)dir;
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
        private Dictionary<int, Info> _list = new Dictionary<int, Info>();


        public enum eKind
        {
            None,
            Line,   //hp 표현
            Circle, //캐릭터 선택 표현
            Square, //캐릭터 선택 표현
            Polygon,//여러 캐릭터 선택 표현
            Graph,  //경로 표현 
        }

        public struct Info
        {
            public LineRenderer render;
            public GameObject gameObject;
            public Transform transform;
            public eKind kind;
            public int id;

            public Vector3 hpPos_0;

            public void Init()
            {
                render = null;
                gameObject = null;
                transform = null;
                kind = eKind.None;
                id = -1;

                hpPos_0 = ConstV.v3_zero;
            }

            public void SetScale(float scale)
            {
                //Vector3 s = _list[id].render.transform.localScale;
                transform.localScale = Vector3.one * scale;
            }

            public void SetLineHP(float rate)
            {
                if (false == gameObject.activeSelf) return; //비활성시에는 처리하지 않는다 
                if (eKind.Line != kind) return;
                
                if (0 > rate) rate = 0;
                if (1f < rate) rate = 1f;

                Vector3 pos = hpPos_0;
                pos.x += HP_BAR_LENGTH * rate;
                render.SetPosition(1, pos);

            }

            //public void Update_Circle()
            //{
            //    if (null == renderer) return;

            //    float deg = 360f / renderer.positionCount;
            //    float radius = renderer.transform.parent.GetComponent<CircleCollider2D>().radius;
            //    Vector3 pos = ConstV.v3_right;
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

		

        const float HP_BAR_LENGTH = 0.8f;
        public Info Create_LineHP_AxisY(Transform dst)
        {
            GameObject obj = new GameObject();
            LineRenderer render = obj.AddComponent<LineRenderer>();
            Info info = new Info();
            info.Init();

            _sequenceId++;

            info.id = _sequenceId;
            info.render = render;
            info.gameObject = render.gameObject;
            info.transform = render.transform;
            info.kind = eKind.Line;

            render.name = info.kind.ToString() + "_" + _sequenceId.ToString("000");
            render.material = new Material(Shader.Find("Sprites/Default"));
            render.useWorldSpace = false; //로컬좌표로 설정하면 부모객체 이동시 영향을 받는다. (변경정보에 따른 재갱싱 필요없음)
            render.transform.parent = dst;
            //render.sortingOrder = -10; //나중에 그려지게 한다.
            render.sortingLayerName = "Effect";
            render.positionCount = 2;
            render.transform.localPosition = ConstV.v3_zero;


            render.startWidth = 0.12f;
            render.endWidth = 0.12f;
            render.startColor = Color.red;
            render.endColor = Color.red;

            _list.Add(_sequenceId, info); //추가

            Vector3 pos = ConstV.v3_zero;
            pos.x = -0.5f; pos.z = -0.8f;
            render.SetPosition(0, pos);
            info.hpPos_0 = pos; //초기위치 저장해 놓음
            pos.x += HP_BAR_LENGTH;
            render.SetPosition(1, pos);

            //return _sequenceId;
            return info;
        }

        public Info Create_Circle_AxisY(Transform parent, float radius, Color color)
        {
            GameObject obj = new GameObject();
            LineRenderer render = obj.AddComponent<LineRenderer>();
            Info info = new Info();
            info.Init();

            _sequenceId++;

            info.id = _sequenceId;
            info.render = render;
            info.gameObject = render.gameObject;
            info.transform = render.transform;
            info.kind = eKind.Circle;

            render.name = info.kind.ToString() + "_" + _sequenceId.ToString("000");
            render.material = new Material(Shader.Find("Sprites/Default"));
            render.useWorldSpace = false; //로컬좌표로 설정하면 부모객체 이동시 영향을 받는다. (변경정보에 따른 재갱싱 필요없음)
            render.transform.parent = parent;//부모객체 지정
            //render.sortingOrder = -10; //먼저그려지게 한다.
            render.sortingLayerName = "Effect";
            render.positionCount = 20;
            render.loop = true; //처음과 끝을 연결한다 .
            render.transform.localPosition = ConstV.v3_zero;

            color.a = 0.4f; //흐리게 한다
            render.startWidth = 0.1f;
            render.endWidth = 0.1f;
            render.startColor = color;//Color.green;
            render.endColor = color;//Color.green;

            _list.Add(_sequenceId, info); //추가

            //info.Update_Circle(); //값설정
            float deg = 360f / render.positionCount;
            //float radius = render.transform.parent.GetComponent<SphereCollider>().radius;
            Vector3 pos = ConstV.v3_right;
            for (int i = 0; i < render.positionCount; i++)
            {
                pos.x = Mathf.Cos(deg * i * Mathf.Deg2Rad) * radius;
                pos.z = Mathf.Sin(deg * i * Mathf.Deg2Rad) * radius;

                render.SetPosition(i, pos );
                //DebugWide.LogBlue(Mathf.Cos(deg * i * Mathf.Deg2Rad) + " _ " + deg*i);
            }

            //return _sequenceId;
            return info;

        }

        //public void Create_Square(Transform dst)
        //{ }

        //public void Create_Polygon(Transform dst)
        //{ }

        public bool IsActive(int id)
        {
            return _list[id].gameObject.activeSelf;
        }

        public void SetActive(int id, bool onOff)
        {
            //todo : 예외처리 추가하기 
            _list[id].gameObject.SetActive(onOff);
        }

        public void SetScale(int id, float scale)
        {
            //Vector3 s = _list[id].render.transform.localScale;
            _list[id].render.transform.localScale = Vector3.one * scale; 
        }

        //public void SetCircle_Radius(int id, float radius)
        //{
            
        //}

        //rate : 0~1
        public void SetLineHP(int id, float rate)
        {
            if (0 > rate) rate = 0;
            if (1f < rate) rate = 1f;

            LineRenderer render = _list[id].render;

            if (false == render.gameObject.activeSelf) return; //비활성시에는 처리하지 않는다 

            Vector3 pos = render.GetPosition(0);
            pos.x += HP_BAR_LENGTH * rate;
            render.SetPosition(1, pos);
            //pos.x = -0.05f; pos.z = -0.15f;
            //render.SetPosition(0, pos);
            //pos.x += (0.1f * rate) ;
            //render.SetPosition(1, pos);

        }

	}
}


//========================================================
//==================     그리드 관리기     ==================
//========================================================

namespace HordeFight
{

    public class Vector3IntComparer : IEqualityComparer<Vector3Int>
    {
        public bool Equals(Vector3Int a, Vector3Int b)
        {
            if (a.x == b.x && a.y == b.y && a.z == b.z)
                return true;
            return false;
        }

        public int GetHashCode(Vector3Int a)
        {
            return a.GetHashCode();
        }
    }

    //한셀에 몇명의 캐릭터가 있는지 나타낸다 
    public class CellInfo : LinkedList<Being>
    {

        public Vector3Int _index = default(Vector3Int);
    }


    //장애물 정보 
    public class CellSpace
    {
        public const int Specifier_DiagonalFixing = 7; //대각고정 예약어

        public int          _specifier = 0;
        public eDirection8  _eDir = eDirection8.none;
        //public Vector3      _v3Dir = ConstV.v3_zero;
        //public Index2       _i2Dir = ConstV.id2_zero;
        public bool         _isUpTile = false; //챔프보다 위에 있는 타일 (TileMap_StructUp)
        public bool         _isStructTile = false; //구조물 타일인지 나타냄

        public Vector3      _pos3d_center = ConstV.v3_zero;    //타일의 중앙 월드위치
        //public Vector2Int   _pos2d = ConstV.v2Int_zero;
        public Index2 _pos2d = ConstV.id2_zero;
        public int          _pos1d = -1; //타일의 1차원 위치값 

        //==================================================
        //타일에 속해있는 객체의 링크
        public Being _children = null; 
        public int _childCount = 0;


        public Being MatchRelation(Camp.eRelation relation , Being target)
        {
            Being getB = null;
            Being next = _children;
            while(null != (object)next)
            {
                getB = next;
                next = next._next_sibling;

                if ((object)getB == (object)target) continue;
                    
                Camp.eRelation getR = SingleO.campManager.GetRelation(target._campKind, getB._campKind);
                //DebugWide.LogBlue(getR.ToString()); //chamto test
                if (relation == getR)
                    return getB; //찾았다 !!


            }

            return null;
        }


        //새로운 객체가 머리가 된다 
        public void AttachChild(Being newHead)
        {
            Being cur_child = _children;
            _children = newHead; // new head of list

            newHead._prev_sibling = null; 
            newHead._next_sibling = cur_child; 
            newHead._cur_cell = this;

            if (null != cur_child) cur_child._prev_sibling = newHead; // previous now this..

            _childCount++;

        }

        public void DetachChild(Being dst)
        {
            if (null == dst._cur_cell || null == dst._cur_cell._children || 0 == dst._cur_cell._childCount) return;

            Being prev = dst._prev_sibling;
            Being next = dst._next_sibling;
            if (null != prev)
            {
                prev._next_sibling = next; 
                if (null != next) next._prev_sibling = prev;
            }
            else
            {
                //dst가 head 인 경우, 새로운 head 설정한다
                //_children = next;
                dst._cur_cell._children = next;

                if (null != next) next._prev_sibling = null;
            }

            dst._cur_cell._childCount--;
            dst._prev_sibling = null;
            dst._next_sibling = null;
            dst._cur_cell = null;
        }
    }


    public class CellSpacePartition
    {
        public const float ONE_METER = 1f; //타일 한개의 가로길이 , 월드 크기 

        private float _tileSize_w = ONE_METER * 1; //월드 크기 
        private float _tileSize_h = ONE_METER * 1; //월드 크기
        public int _tileMapSize_x = 64;
        private int _tileMapSize_y = 64;
        private int _max_tileMapSize = 64 * 64;
        //private Vector2Int _tileMapSize = new Vector2Int(64, 64); //64*64 타일 갯수까지 사용한다 
        private CellSpace[] _cellInfo2Map = null;

        string __debugTemp = ConstV.STRING_EMPTY;
        public void DebugPrint()
        {
            foreach(CellSpace cell in _cellInfo2Map)
            {
                if(0 != cell._childCount)
                {
                    __debugTemp = cell._childCount + "  "; //+ DebugPrint_Children(cell._children);
                    DebugWide.PrintText(cell._pos3d_center, Color.red, __debugTemp);
                    DebugWide.DrawCube(cell._pos3d_center,Vector3.one , Color.red);

                    //DebugWide.LogBlue(cell._pos2d + "  " + cell._childCount + "  " + DebugPrint_Children(cell._children));
                }
            }
        }
        public string DebugPrint_Children(Being be)
        {
            if (null == be) return ConstV.STRING_EMPTY;
            return be.name + " : " + DebugPrint_Children(be._next_sibling);

        }


        //public void Init(Vector2Int tileMapSize)
        public void Init(Index2 tileMapSize)
        {
            _tileMapSize_x = tileMapSize.x;
            _tileMapSize_y = tileMapSize.y;
            _max_tileMapSize = _tileMapSize_x * _tileMapSize_y;
            _cellInfo2Map = new CellSpace[_tileMapSize_x * _tileMapSize_y];

            CreateCellIfoMap_FromStructUpTile();
        }


        private void CreateCellIfoMap_FromStructUpTile()
        {
            int count = 0;
            for (int y = 0; y < _tileMapSize_y;y++)
            {
                for (int x = 0; x < _tileMapSize_x; x++)
                {
                    CellSpace structTile = null;
                    if(false == SingleO.gridManager.HasStructTile_InPostion2D(new Vector3Int(x, y, 0), out structTile))
                    {
                        //구조타일이 없는 공간설정 
                        structTile = new CellSpace();
                        structTile._pos3d_center = ToPosition3D_Center(count);
                        structTile._pos2d = new Index2(x, y);
                        structTile._pos1d = count;

                    }
                    _cellInfo2Map[count] = structTile;

                    count++;
                }   
            }

        }

        //테스트 필요 정상동적 안함 
        public Being RayCast_FirstReturn(Being origin , Vector3 target_3d, Camp.eRelation relation ,float length_interval)
        {
            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval || null == (object)origin)
            {
                return null;
            }

            //Index2 origin_2d = origin._getPos2D;
            int origin_1d = -1;
            Index2 origin_2d = ConstV.id2_zero;
            ToPosition1D(origin.GetPos3D(), out origin_2d, out origin_1d);

            Vector3 origin_3d_center = origin._cur_cell._pos3d_center;


            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            //CellSpace structTile = GetCellSpace(origin._getPos1D);
            CellSpace structTile = GetCellSpace(origin_1d);
            if (null != structTile && structTile._isStructTile)
            {
                switch (structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_3d_center.x += dir.x * _tileSize_w * 0.5f;
                            origin_3d_center.z += dir.y * _tileSize_h * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            Vector3Int vd = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_2d.x += vd.x;
                            origin_2d.y += vd.y;
                            origin_3d_center = ToPosition3D_Center(origin_2d);
                        }
                        break;
                }

            }


            Vector3 line = VOp.Minus(target_3d, origin_3d_center);
            Vector3 n = VOp.Normalize(line);
            n = VOp.Multiply(n, length_interval); //미리 곱해 놓는다 

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            //Vector3 next = n * count;
            Vector3 next = VOp.Multiply(n, count);
            float lineSqr = line.sqrMagnitude;
            while (lineSqr > next.sqrMagnitude)
            {
                //최대 50회까지만 탐색한다 
                if (50 <= count)
                {
                    //DebugWide.LogBlue(n); //chamto test
                    return null;
                }

                next = VOp.Plus(origin_3d_center, next);
                structTile = GetCellSpace(next);
                if (null != structTile)
                {
                    if (true == structTile._isUpTile)
                    {

                        if (_ReturnMessage_NotIncluded_InDiagonalArea != this.IsIncluded_InDiagonalArea(next))
                        {
                            return null;
                        }
                    }
                    else if(null != (object)structTile._children)
                    {
                        //==================================
                        if(Camp.eRelation.Unknown != relation)
                        {
                            //요청 관계에 해당하는 객체를 찾는다 
                            Being matchBeing = structTile.MatchRelation(relation, origin);
                            if (null != (object)matchBeing) return matchBeing;
                        }else
                        {
                            return structTile._children; //첫번째로 발견한 객체를 반환한다 (적군 , 아군 구별없다)    
                        }
                        //==================================

                    }

                }

                count++;
                next = VOp.Multiply(n, count);

            }

            return null;
        }

        public bool IsVisibleTile(Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {
            return IsVisibleTile(null, origin_3d, target_3d, length_interval);
        }

        public bool IsVisibleTile(Being origin, Vector3 origin_3d ,Vector3 target_3d, float length_interval)
        {
            
            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval)
            {
                return false;
            }

            Index2 origin_2d;
            Vector3 origin_3d_center;
            if(null == (object)origin)
            {
                origin_2d = ToPosition2D(origin_3d);
                origin_3d_center = ToPosition3D_Center(origin_2d);
            }else
            {
                //origin_2d = origin._getPos2D;
                origin_2d = ToPosition2D(origin_3d);
                origin_3d_center = origin._cur_cell._pos3d_center;    
            }



            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            CellSpace structTile = GetCellSpace(origin_3d);
            if (null != structTile && structTile._isStructTile)
            {
                switch (structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_3d_center.x += dir.x * _tileSize_w * 0.5f;
                            origin_3d_center.z += dir.y * _tileSize_h * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            Vector3Int vd = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_2d.x += vd.x; 
                            origin_2d.y += vd.y;
                            origin_3d_center = ToPosition3D_Center(origin_2d);
                        }
                        break;
                }

            }

            //Vector3 line = target_3d - origin_3d_center;
            Vector3 line = VOp.Minus(target_3d , origin_3d_center);
            Vector3 n = VOp.Normalize(line);
            //Vector3 n = Misc.GetDir360_Normal3D(line); //근사치 노멀값을 사용하면 목표에 도달이 안되는 무한루프에 
            //Vector3 n = line.normalized;
            //n *= length_interval; //미리 곱해 놓는다 
            n = VOp.Multiply(n, length_interval); //미리 곱해 놓는다 

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            //Vector3 next = n * count;
            Vector3 next =VOp.Multiply(n , count);
            float lineSqr = line.sqrMagnitude;
            while (lineSqr > next.sqrMagnitude)
            {
                //최대 50회까지만 탐색한다 
                if(50 <= count) 
                {
                    //DebugWide.LogBlue(n); //chamto test
                    return false;
                }
                //next = origin_3d_center + next;
                next = VOp.Plus(origin_3d_center , next);
                structTile = GetCellSpace(next);
                if (null != structTile)
                {
                    if (true == structTile._isUpTile)
                    {
                        
                        if (_ReturnMessage_NotIncluded_InDiagonalArea != this.IsIncluded_InDiagonalArea(next))
                        {
                            return false;
                        }
                    }

                }

                count++;
                //next = n * count;
                next = VOp.Multiply(n, count);

            }

            return true;
        }


        /// <summary>
        /// 구조타일 영역에 미포함
        /// </summary>
        public const int _ReturnMessage_NotIncluded_InStructTile = 0;

        /// <summary>
        /// 구조타일 영역에 포함. 구조대각타일이 아니다
        /// </summary>
        public const int _ReturnMessage_Included_InStructTile = 1;

        /// <summary>
        /// 구조대각타일이며 , 대각타일 영역에 미포함
        /// </summary>
        public const int _ReturnMessage_NotIncluded_InDiagonalArea = 2;

        /// <summary>
        /// 구조대각타일이며 , 대각타일 영역에 포함
        /// </summary>
        public const int _ReturnMessage_Included_InDiagonalArea = 3;


        //구조타일의 대각타일 영역에 포함하는지 검사 
        public int IsIncluded_InDiagonalArea(Vector3 xz_3d)
        {
            
            CellSpace structTile = GetCellSpace(xz_3d);
            if (null != structTile)
            {
                switch (structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //특수고정대각 타일은 일반구조 타일처럼 처리한다 
                            if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                                return _ReturnMessage_Included_InStructTile;

                            //타일 중앙점 o , 검사할 점 p 가 있을 때
                            //대각타일 영역에서 점 p를 포함하는지 내적을 이용해서 검사 
                            Vector3 oDir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);
                            //Vector3 pDir = xz_3d - structTile._center_3d; //실수값 부정확성 때문에 같은 위치에서도 다른 결과가 나옴.
                            Vector3 pDir = Misc.GetDir8_Normal3D(xz_3d - structTile._pos3d_center); //정규화된8 방향값으로 변환해서 부정확성을 최소로 줄인다.
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



        public Index2 ToPosition2D(Vector3 pos3d , Index2 result = default(Index2))
        {
            
            //부동소수점 처리 문제로 직접계산하지 않는다 
            //int pos2d_x = Mathf.FloorToInt(pos3d.x / _tileSize_w);
            //int pos2d_y = Mathf.FloorToInt(pos3d.z / _tileSize_h);


            //직접계산
            //==============================================
            int pos2d_x = 0;
            int pos2d_y = 0;
            if (0 <= pos3d.x)
            {
                //양수일때는 소수점을 버린다. 
                pos2d_x = (int)(pos3d.x / _tileSize_w);
            }
            else
            {
                //음수일때는 올림을 한다. 
                pos2d_x = (int)((pos3d.x / _tileSize_w) - 0.9f);
            }

            if (0 <= pos3d.z)
            {
                //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
                pos2d_y = (int)(pos3d.z / _tileSize_h); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다
            }
            else
            {
                pos2d_y = (int)((pos3d.z / _tileSize_h) - 0.9f);
            }
            //==============================================

            result.x = pos2d_x;
            result.y = pos2d_y;
            return result;
        }

        public Vector3 ToCenter3D_FromPosition3D(Vector3 pos3d)
        {
            
            return this.ToPosition3D_Center(ToPosition2D(pos3d));
        }


        public Vector3 ToPosition3D_Center(Index2 posXY_2d , Vector3 result = default(Vector3))
        {
            
            //2d => 3d
            float pos3d_x = (float)posXY_2d.x * _tileSize_w;
            float pos3d_z = (float)posXY_2d.y * _tileSize_h;

            //셀의 중간에 위치하도록 한다
            pos3d_x += _tileSize_w * 0.5f;
            pos3d_z += _tileSize_h * 0.5f;

            result.x = pos3d_x;
            result.y = 0;
            result.z = pos3d_z;
            return result;
        }

        public Vector3 ToPosition3D(Index2 posXY_2d , Vector3 result = default(Vector3))
        {
            result.x = (float)posXY_2d.x * _tileSize_w;
            result.y = 0;
            result.z = (float)posXY_2d.y * _tileSize_h;
            return result;
        }

        public int ToPosition1D(Index2 posXY_2d)
        {
            //Assert.IsFalse(0 > posXY_2d.x || 0 > posXY_2d.y, "음수좌표값은 1차원값으로 변환 할 수 없다");
            if (0 > posXY_2d.x || 0 > posXY_2d.y || _tileMapSize_x <= posXY_2d.x || _tileMapSize_y <= posXY_2d.y) return -1;

            return (posXY_2d.x + posXY_2d.y * _tileMapSize_x); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Index2 ToPosition2D(int pos1d , Index2 result = default(Index2))
        {
            //Assert.IsFalse(0 > pos1d, "음수좌표값은 2차원값으로 변환 할 수 없다");
            if (0 > pos1d) return ConstV.id2_zero;

            result.x = pos1d % _tileMapSize_x;
            result.y = pos1d / _tileMapSize_x;
            return result;
        }

        public void ToPosition1D(Vector3 pos3d , out Index2 out_pos2d, out int out_pos1d)
        {
            int pos2d_x = 0;
            int pos2d_y = 0;
            if (0 <= pos3d.x)
            {
                //양수일때는 소수점을 버린다. 
                pos2d_x = (int)(pos3d.x / _tileSize_w);
            }
            else
            {
                //음수일때는 올림을 한다. 
                pos2d_x = (int)((pos3d.x / _tileSize_w) - 0.9f);
            }

            if (0 <= pos3d.z)
            {
                //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
                pos2d_y = (int)(pos3d.z / _tileSize_h); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다
            }
            else
            {
                pos2d_y = (int)((pos3d.z / _tileSize_h) - 0.9f);
            }
            //==============================================

            out_pos2d = new Index2(pos2d_x, pos2d_y);
            //out_pos2d.x = pos2d_x; out_pos2d.y = pos2d_y;
            out_pos1d = -1;
            if (0 > pos2d_x || 0 > pos2d_y || _tileMapSize_x <= pos2d_x || _tileMapSize_y <= pos2d_y) return;

            //2d => 1d
            out_pos1d = (pos2d_x + pos2d_y * _tileMapSize_x); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 
        }

        public int ToPosition1D(Vector3 pos3d)
        {
            //3d => 2d
            //부동소수점 처리 문제로 직접계산하지 않는다 
            //int pos2d_x = Mathf.FloorToInt(pos3d.x / _tileSize_w);
            //int pos2d_y = Mathf.FloorToInt(pos3d.z / _tileSize_h);

            //직접계산
            //==============================================
            int pos2d_x = 0;
            int pos2d_y = 0;
            if (0 <= pos3d.x)
            {
                //양수일때는 소수점을 버린다. 
                pos2d_x = (int)(pos3d.x / _tileSize_w);
            }
            else
            {
                //음수일때는 올림을 한다. 
                pos2d_x = (int)((pos3d.x / _tileSize_w) - 0.9f);
            }

            if (0 <= pos3d.z)
            {
                //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
                pos2d_y = (int)(pos3d.z / _tileSize_h); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다
            }
            else
            {
                pos2d_y = (int)((pos3d.z / _tileSize_h) - 0.9f);
            }
            //==============================================

            if (0 > pos2d_x || 0 > pos2d_y || _tileMapSize_x <= pos2d_x || _tileMapSize_y <= pos2d_y) return -1;

            //2d => 1d
            return (pos2d_x + pos2d_y * _tileMapSize_x); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Vector3 ToPosition3D_Center(int pos1d , Vector3 result = default(Vector3))
        {
            if (0 > pos1d || _tileMapSize_x * _tileMapSize_y <= pos1d) return ConstV.v3_zero;

            //1d => 2d
            int pos2d_x = pos1d % _tileMapSize_x;
            int pos2d_y = pos1d / _tileMapSize_x;

            //2d => 3d
            float pos3d_x = (float)pos2d_x * _tileSize_w;
            float pos3d_z = (float)pos2d_y * _tileSize_h;

            //셀의 중간에 위치하도록 한다
            pos3d_x += _tileSize_w * 0.5f;
            pos3d_z += _tileSize_h * 0.5f;

            result.x = pos3d_x;
            result.y = 0;
            result.z = pos3d_z;
            return result;
        }


        //==================================================

        public bool HasStructUpTile(int p1d)
        {
            if (0 > p1d || p1d >= _max_tileMapSize)
            {
                return false;
            }

            CellSpace structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isUpTile && structTile._isStructTile;
            }

            return false;
        }
        public bool HasStructUpTile(Index2 p2d)
        {
            int p1d = this.ToPosition1D(p2d);
            if (-1 == p1d)
            {
                return false;
            }

            CellSpace structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {

                return structTile._isUpTile && structTile._isStructTile;
            }


            return false;
        }
        public bool HasStructUpTile(Vector3 p3d)
        {
            int p1d = this.ToPosition1D(p3d);
            if (-1 == p1d)
            {
                return false;
            }

            CellSpace structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isUpTile && structTile._isStructTile;
            }

            return false;
        }

        public bool HasStructTile(int p1d, out CellSpace structTile)
        {
            if(0 > p1d  || p1d >= _max_tileMapSize)
            {
                structTile = null;
                return false;
            }

            structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isStructTile;
            }

            return false;
        }
        public bool HasStructTile(Index2 p2d, out CellSpace structTile)
        {
            int p1d = this.ToPosition1D(p2d);
            if (-1 == p1d) 
            {
                structTile = null;
                return false;   
            }

            structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isStructTile;
            }


            return false;
        }
        public bool HasStructTile(Vector3 p3d, out CellSpace structTile)
        {
            int p1d = this.ToPosition1D(p3d);
            if (-1 == p1d)
            {
                structTile = null;
                return false;
            }

            structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isStructTile;
            }

            return false;
        }

        public CellSpace GetCellSpace(int p1d)
        {
            if (0 > p1d || p1d >= _max_tileMapSize)
            {
                return null;
            }

            return _cellInfo2Map[p1d];
        }

        public CellSpace GetCellSpace(Index2 pos2d)
        {
            //int pos1d = pos2d.x + pos2d.y * _tileMapSize_x;
                    
            int pos1d = ToPosition1D(pos2d);
            if (0 > pos1d) return null; //타일맵을 벗어나는 범위 

            return _cellInfo2Map[pos1d];
        }


        public CellSpace GetCellSpace(Vector3 pos3d)
        {
            int pos1d = ToPosition1D(pos3d);
            if (0 > pos1d) return null; //타일맵을 벗어나는 범위 

            return _cellInfo2Map[pos1d];

        }

        public void AttachCellSpace(int pos1d, Being dst)
        {
            CellSpace tile = null;
            if(false == HasStructTile(pos1d,out tile))
            {
                //뗀후 새로운 곳에 붙인다 
                tile.DetachChild(dst);
                tile.AttachChild(dst);
            }
        }

        public void DetachCellSpace(int pos1d, Being dst)
        {
            CellSpace tile = null;
            if (false == HasStructTile(pos1d, out tile))
            {
                tile.DetachChild(dst);
            }
        }

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
        public Dictionary<Vector3Int, CellInfo> _cellInfoList = new Dictionary<Vector3Int,CellInfo>(new Vector3IntComparer());
        public Dictionary<Vector3Int, CellSpace> _structTileList = new Dictionary<Vector3Int, CellSpace>(new Vector3IntComparer());

        //public CellSpacePartition _cellSpacePartition = new CellSpacePartition();

        //중심이 (0,0)인 nxn 그리드 인덱스 값을 미리 구해놓는다
        public Dictionary<uint, Index2[]> _indexesNxN = new Dictionary<uint, Index2[]>();


        public Grid grid
        {
            get { return _grid; }
        }
        public float _cellSize_x = 1f;
        //{
        //    get
        //    {
        //        return (_grid.cellSize.x * _grid.transform.localScale.x);
        //    }
        //}
        public float _cellSize_z = 1f;
        //{
        //    get
        //    {
        //        return (_grid.cellSize.y * _grid.transform.localScale.z);
        //    }
        //}

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

		//private void Start()
        public void Init()
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

            _cellSize_x = _grid.cellSize.x * _grid.transform.localScale.x;
            _cellSize_z = _grid.cellSize.y * _grid.transform.localScale.z;

            //타일맵 좌표계 x-y에 해당하는 축값 back로 구한다 
            _indexesNxN[3] = CreateIndexesNxN_SquareCenter_Tornado(3,3);
            _indexesNxN[5] = CreateIndexesNxN_SquareCenter_Tornado(5,5);
            _indexesNxN[7] = CreateIndexesNxN_SquareCenter_Tornado(7,7);
            _indexesNxN[9] = CreateIndexesNxN_SquareCenter_Tornado(9,9);
            _indexesNxN[11] = CreateIndexesNxN_SquareCenter_Tornado(11,11); //화면 세로길이를 벗어나지 않는 그리드 최소값
            _indexesNxN[29] = CreateIndexesNxN_SquareCenter_Tornado(29,19);

            this.LoadTilemap_Struct();

            SingleO.cellPartition.Init(new Index2(64, 64));
            //_cellSpacePartition.Init(new Vector2Int(64, 64)); //chamto test
		}


        //ref : https://gamedev.stackexchange.com/questions/150917/how-to-get-all-tiles-from-a-tilemap
        public void LoadTilemap_Struct()
        {
            if (null == _tilemap_struct) return;

            SingleO.gridManager.GetTileMap_Struct().RefreshAllTiles();
            CellSpace structTile = null;
            RuleExtraTile.TilingRule ruleInfo = null;
            int specifier = 0;
            foreach (Vector3Int XY_2d in _tilemap_struct.cellBounds.allPositionsWithin)
            {
                RuleExtraTile ruleTile = _tilemap_struct.GetTile(XY_2d) as RuleExtraTile; //룰타일 종류와 상관없이 다 가져온다. 
                if (null == ruleTile) continue;

                ruleInfo = ruleTile._tileDataMap.GetTilingRule(XY_2d);
                if (null == ruleInfo || false == int.TryParse(ruleInfo.m_specifier, out specifier)) 
                    specifier = 0;


                structTile = new CellSpace();
                structTile._specifier = specifier;
                structTile._pos3d_center = this.ToPosition3D_Center(XY_2d);
                structTile._pos2d = new Index2(XY_2d.x, XY_2d.y);
                structTile._pos1d = this.ToPosition1D(XY_2d, 64); //임시코드
                structTile._eDir = ruleTile._tileDataMap.GetDirection8(XY_2d);
                //structTile._v3Dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir); //방향값을 미리 구해 놓는다 
                //structTile._i2Dir = Misc.GetDir8_Normal2D(structTile._eDir);
                structTile._isUpTile = ruleTile._tileDataMap.Get_IsUpTile(XY_2d);
                structTile._isStructTile = true;
                _structTileList.Add(XY_2d, structTile);

            }

            DebugWide.LogBlue("LoadTile : " + _structTileList.Count + "  -  TileMap_Struct RefreshAllTiles");

            //덮개 타일 생성
            TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            //TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            foreach (KeyValuePair<Vector3Int,CellSpace> t in _structTileList)
            { 
                if(true == t.Value._isUpTile)
                {
                    _tilemap_structUp.SetTile(t.Key, tileScript);
                }
            }

            DebugWide.LogBlue("덮개타일 생성 완료 : " + tileScript.name);

        }


        //private void Update()
        //{

        //}


        public bool old_IsVisibleTile(Vector3 origin_3d, Vector3 target_3d , float length_interval)
        {
            return true; //test

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
            CellSpace structTile = null;
            if (this.HasStructTile(origin_3d , out structTile))
            {
                switch(structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_3d_center.x += dir.x * _cellSize_x * 0.5f;
                            origin_3d_center.z += dir.y * _cellSize_z * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            origin_3d_center = ToPosition3D_Center(origin_2d + Misc.GetDir8_Normal2D(structTile._eDir));
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
        
            //Color baseColor = Color.white;
            //Color fogColor = new Color(1, 1, 1, 0.7f);
            TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_RuleExtraTile".GetHashCode());
            TileBase tileScript2 = SingleO.resourceManager.GetTileScript("ocean".GetHashCode());
            TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            TileBase tileScript4 = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            Vector3Int posXY_2d = this.ToPosition2D(standard_3d);
            Vector3 tile_3d_center = ConstV.v3_zero;
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
                    if (true == old_IsVisibleTile(standard_3d, tile_3d_center, 0.1f))
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
            //Color baseColor = Color.white;
            //Color fogColor = new Color(1, 1, 1, 0.7f);
            Vector3Int v2IntTo3 = ConstV.v3Int_zero;
            Index2 posXY_2d = SingleO.cellPartition.ToPosition2D(standard_3d); //this.ToPosition2D(standard_3d);
            Vector3 tile_3d_center = ConstV.v3_zero;
            Index2 tile_2d = ConstV.id2_zero;
            int count = -1;
            foreach(Index2 xy in _indexesNxN[29])
            {
                tile_2d = xy + posXY_2d;

                //=====================================================

                tile_3d_center = SingleO.cellPartition.ToPosition3D_Center(tile_2d);//this.ToPosition3D_Center(tile_2d);

                v2IntTo3.x = tile_2d.x;
                v2IntTo3.y = tile_2d.y;
                v2IntTo3.z = 0;
                RuleExtraTile ruleTile = _tilemap_fogOfWar.GetTile(v2IntTo3) as RuleExtraTile;
                float sqrDis = VOp.Minus(tile_3d_center , standard_3d).sqrMagnitude;
                float sqrStd = GridManager.MeterToWorld * 6.2f; sqrStd *= sqrStd;
                if (sqrDis <= sqrStd)
                {
                    if (true == SingleO.cellPartition.IsVisibleTile(standard_3d, tile_3d_center, 0.1f))
                    //SphereModel model = SingleO.objectManager.GetSphereTree_Struct().RayTrace_FirstReturn(standard_3d, tile_3d_center, null);
                    //if(null == model)
                    {

                        //대상과 정반대 방향이 아닐때 처리 
                        //Vector3 tileDir = tile_3d_center - standard_3d;
                        Vector3 tileDir = VOp.Minus(tile_3d_center , standard_3d);
                        //tileDir.Normalize(); lookAt_dir.Normalize();
                        //tileDir = Misc.GetDir64_Normal3D(tileDir); lookAt_dir = Misc.GetDir64_Normal3D(lookAt_dir); //근사치노멀을 사용하면 값이 이상하게 나옴a
                        sqrStd = GridManager.MeterToWorld * 1.2f; sqrStd *= sqrStd;

                        if (true ==  SingleO.cellPartition.HasStructUpTile(tile_3d_center))//this.HasStructUpTile(tile_3d_center))
                        {
                            //구조덮개타일인 경우 
                            SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                        }
                        //else if (Mathf.Cos(Mathf.Deg2Rad * 40f) < Vector3.Dot(tileDir, lookAt_dir))
                        else if (40f >= Geo.Angle_AxisY(tileDir, lookAt_dir))
                        {
                            //원거리 시야 표현
                            SetTile(_tilemap_fogOfWar, v2IntTo3, null);
                        }
                        else if (sqrDis <= sqrStd)
                        {
                            //주변 시야 표현
                            SetTile(_tilemap_fogOfWar, v2IntTo3, null);
                        }
                        else
                        {
                            SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                        }

                        //SetTile(_tilemap_fogOfWar, xy, null);

                    }
                    else
                    {
                        SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                    }
                }
                else
                {
                    //안보이는 곳
                    SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
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

                //타일 300개 처리했으면 0.001초 뒤에 다시 처리한다
                if(0 == count % 300)
                    yield return new WaitForSeconds(0.001f);
                
                
            }
            __is_dividing = false;

        }

        //원 반지름 길이를 포함하는 그리드범위 구하기
        public uint GetNxNIncluded_CircleRadius(float maxRadius)
        {
            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint nxn = (uint)((maxRadius * 2) / this._cellSize_x); //소수점 버림을 한다 
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

                    if (ConstV.v3_up == axis)
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
        public Index2[] CreateIndexesNxN_SquareCenter_Tornado(ushort width,ushort height)
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
            Vector3 dir = ConstV.v3_right;
            Index2 prevMax = ConstV.id2_zero;
            Index2 prediction = ConstV.id2_zero;
            Index2 cur =ConstV.id2_zero;
            Index2[] indexes = new Index2[width * height];
            int cnt = 0;
            int max_cnt = width > height ? width : height; //큰값이 들어가게 한다 
            max_cnt *= max_cnt;
            //for (int cnt = 0; cnt < indexes.Length; cnt++)
            for (int i = 0; i < max_cnt; i++)
            {
                if(Math.Abs(cur.x) < Math.Abs(width * 0.5f) &&
                   Math.Abs(cur.y) < Math.Abs(height * 0.5f))
                {
                    //DebugWide.LogBlue(cnt + "  " + cur);
                    indexes[cnt] = cur;
                    cnt++;
                }


                prediction.x = (int)(dir.x * Tornado_Num); //소수점 버림
                prediction.y = (int)(dir.y * Tornado_Num); //소수점 버림
                //prediction.z = (int)(dir.z * Tornado_Num); //소수점 버림

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
                    //if (ConstV.v3_up == axis)
                    //{
                    //    //y축 중심 : 3d용 좌표
                    //    dir = Quaternion.Euler(0, -90f, 0) * dir;
                    //}
                    //if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, 90f) * dir;
                    }
                    dir.Normalize(); //부동소수점 문제 때문에 정규화 해준다 
                }

                //지정된 방향값으로 한칸씩 증가한다
                cur.x += (int)Mathf.Round(dir.x); //반올림 한다 : 대각선 방향 때문 
                cur.y += (int)Mathf.Round(dir.y);
                //cur.z += (int)Mathf.Round(dir.z);
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
            Vector3 dir = ConstV.v3_zero;
            Vector3 prevMax = ConstV.v3_zero;
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
                        dir = ConstV.v3_right;
                        cur.x += (int)Mathf.Round(dir.x);
                        cur.y += (int)Mathf.Round(dir.y);
                        cur.z += (int)Mathf.Round(dir.z);
                    }
                    else
                    {
                        angle = -90f;
                    }

                    if (ConstV.v3_up == axis)
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

            CellSpace structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                switch(structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //특수고정대각 타일은 일반구조 타일처럼 처리한다 
                            if(CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                                return _ReturnMessage_Included_InStructTile;

                            //타일 중앙점 o , 검사할 점 p 가 있을 때
                            //대각타일 영역에서 점 p를 포함하는지 내적을 이용해서 검사 
                            Vector3 oDir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);
                            //Vector3 pDir = xz_3d - structTile._center_3d; //실수값 부정확성 때문에 같은 위치에서도 다른 결과가 나옴.
                            Vector3 pDir = Misc.GetDir8_Normal3D(xz_3d - structTile._pos3d_center); //정규화된8 방향값으로 변환해서 부정확성을 최소로 줄인다.
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
            CellSpace structTile = null;
            return HasStructUpTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructUpTile(Vector3 xz_3d, out CellSpace structTile)
        {
            //return HasStructUpTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
            return HasStructUpTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructUpTile_InPostion2D(Vector3Int xy_2d , out CellSpace structTile)
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
            CellSpace structTile = null;
            Vector3Int xy_2d = _tilemap_struct.WorldToCell(xz_3d);
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile(Vector3 xz_3d, out CellSpace structTile)
        {
            //return HasStructTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
            Vector3Int xy_2d = ToPosition2D(xz_3d);
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile_InPostion2D(Vector3Int xy_2d)
        {
            CellSpace structTile = null;
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile_InPostion2D(Vector3Int xy_2d , out CellSpace structTile)
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
            posXY_2d.x = Mathf.FloorToInt(pos3d.x / _cellSize_x);
            posXY_2d.y = Mathf.FloorToInt(pos3d.z / _cellSize_z);

            //posXY_2d = _tilemap_struct.WorldToCell(pos3d); //버림함수를 사용하여 계산하는 것과 결과가 달리 나온다 

            return posXY_2d;
        }

        public Vector3 ToCenter3D_FromPosition3D(Vector3 pos3d)
        {
            Vector3 pos = ConstV.v3_zero;
            Vector3 cellSize = ConstV.v3_zero;

            Vector3Int cellPos = this.ToPosition2D(pos3d);

            return this.ToPosition3D_Center(cellPos);
        }

        //pos2d 는 항시 x,y공간만 사용한다. 다른 공간은 변환 허용안함.
        //grid 와 호환 안되는 함수 
        public Vector3 ToPosition3D_Center(Vector3Int posXY_2d)
        {
            Vector3 pos3d = ConstV.v3_zero;

            {
                pos3d.x = (float)posXY_2d.x * _cellSize_x;
                pos3d.z = (float)posXY_2d.y * _cellSize_z;

                //셀의 중간에 위치하도록 한다
                pos3d.x += _cellSize_x * 0.5f;
                pos3d.z += _cellSize_z * 0.5f;
            }

            return pos3d;
        }

        public Vector3 ToPosition3D(Vector3Int posXY_2d)
        {
            Vector3 pos3d = ConstV.v3_zero;

            {
                pos3d.x = (float)posXY_2d.x * _cellSize_x;
                pos3d.z = (float)posXY_2d.y * _cellSize_z;
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

        private AABBCulling _aabbCulling = new AABBCulling();
        private SphereTree _sphereTree_being = new SphereTree(2000, new float[]{ 16, 10 ,5, 2 }, 0.5f);
        private SphereTree _sphereTree_struct = new SphereTree(2000, new float[] { 16, 10, 4 }, 1f);


        //private void Start()
        public void Init()
        {
            DateTime _startDateTime;
            string _timeTemp = "";

            //===============
            //해쉬와 문자열 설정
            //===============

            //==============================================
            _startDateTime = DateTime.Now;
            SingleO.hashMap.Add(Animator.StringToHash("idle"),"idle");
            SingleO.hashMap.Add(Animator.StringToHash("move"),"move");
            SingleO.hashMap.Add(Animator.StringToHash("block"),"block");
            SingleO.hashMap.Add(Animator.StringToHash("attack"),"attack");
            SingleO.hashMap.Add(Animator.StringToHash("fallDown"),"fallDown");
            SingleO.hashMap.Add(Animator.StringToHash("idle -> attack"),"idle -> attack");
            SingleO.hashMap.Add(Animator.StringToHash("attack -> idle"),"attack -> idle");
            _timeTemp += "  ObjectManager.hashMap.Add  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================
            _startDateTime = DateTime.Now;
            this.Create_ChampCamp(); //임시로 여기서 호출한다. 추후 스테이지 생성기로 옮겨야 한다 
            _timeTemp += "  ObjectManager.Create_ChampCamp  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================
            _startDateTime = DateTime.Now;
            _aabbCulling.Initialize(_linearSearch_list); //aabb 컬링 초기화 
            DebugWide.LogBlue("Start_ObjectManager !! ");
            _timeTemp += "  ObjectManager.aabbCulling.init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================
            _startDateTime = DateTime.Now;
            //임시로 여기서 처리
            //구조타일 정보로 구트리정보를 만든다 
            foreach (KeyValuePair<Vector3Int, CellSpace> t in SingleO.gridManager._structTileList)
            {
                //if (true == t.Value._isUpTile)
                {
                    SphereModel model = _sphereTree_struct.AddSphere(t.Value._pos3d_center, 0.6f, SphereModel.Flag.TREE_LEVEL_LAST);
                    _sphereTree_struct.AddIntegrateQ(model);
                }
            }
            _timeTemp += "  ObjectManager.sphereTree.init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            DebugWide.LogBlue(_timeTemp);
            //==============================================

        }

        public SphereTree GetSphereTree_Being() { return _sphereTree_being; }
        public SphereTree GetSphereTree_Struct() { return _sphereTree_struct; }

		public Bounds GetBounds_CameraView()
		{
            float dis = 0f;
            float height_half = 0f;
            float width_half = 0f;
            Vector3 landPos = SingleO.mainCamera.transform.position;
            landPos.y = 0;
            Bounds bounds = new Bounds();
            bounds.center = landPos;
            if (true == SingleO.mainCamera.orthographic)
            {
                //직교투영
                height_half = SingleO.mainCamera.orthographicSize;
                width_half = SingleO.mainCamera.aspect * height_half;
                bounds.size = new Vector3(width_half * 2f, 0, height_half * 2f);

            }else
            {
                //원근투영
                //땅바닥이 항상 y축 0이라 가정한다
                dis = SingleO.mainCamera.transform.position.y - 0f;
                height_half = dis * Mathf.Tan(SingleO.mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                width_half = SingleO.mainCamera.aspect * height_half;
                bounds.size = new Vector3(width_half * 2f, 0, height_half * 2f);
            }

            return bounds;
		}

		//객체간의 충돌검사 최적화를 위한 충돌가능객체군 미리 조직하기 
		private void Update()
        //private void LateUpdate()
        {
            
            //UpdateCollision();

            //UpdateCollision_UseDictElementAt(); //obj100 : fps10
            //UpdateCollision_UseDictForeach(); //obj100 : fps60

            //UpdateCollision_UseList(); //obj100 : fps80 , obj200 : fps40 , obj400 : fps15
            //UpdateCollision_UseGrid3x3(); //obj100 : fps65 , obj200 : fps40
            //UpdateCollision_UseDirectGrid3x3(); //obj100 : fps100 , obj200 : fps75 , obj400 : fps55

            UpdateCollision_AABBCulling(); //obj110 : fps100 , obj200 : fps77 , obj400 : fps58


            _sphereTree_being.Process();
            _sphereTree_struct.Process();
        }


        //중복되는 영역을 제외한 검사영역 : 기존 9개의 영역에서 5개의 영역으로 줄였다
        Vector3Int[] __cellIndexes_5 = new Vector3Int[] {
            new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0), 
            new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 0),
        };


        //충돌원이 셀을 벗어나는 상황별 검사영역 
        //가정 : 셀보다 충돌원이 작아야 한다. 
        //List<Vector3Int[]> __cellIndexes_Max4 = new List<Vector3Int[]> {
        Vector3Int[][] __cellIndexes_Max4 = new Vector3Int[][] {
            new Vector3Int[] { new Vector3Int(0, 0, 0), }, //center = 0,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), }, //right = 1,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0),}, //rightUp = 2
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0),}, //up = 3,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, 0),}, //leftUp = 4,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0),}, //left = 5,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0),}, //leftDown = 6,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, -1, 0),}, //down = 7,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, -1, 0), new Vector3Int(0, -1, 0),}, //rightDown = 8
        };

        //충돌원이 셀에 겹친 영역을 구함
        public int GetOverlapCellSpace(Being being)
        {

            //Vector3 center = SingleO.gridManager.ToPosition3D_Center(being._cellInfo._index);
            Vector3 center = being._cur_cell._pos3d_center;
            Vector3 rate = being.GetPos3D() - center;
            float cellHalfSize = SingleO.gridManager._cellSize_x * 0.5f;

            //return 8;

            if(Mathf.Abs(rate.x) + being._collider_radius <= cellHalfSize && Mathf.Abs(rate.z) + being._collider_radius <= cellHalfSize)
            {
                return 0;
            }

            return (int)Misc.GetDir8_AxisY(rate);
        }


        //private void OnDrawGizmos()
        //{
        //          Bounds bounds = GetBounds_CameraView();
        //          Gizmos.DrawWireCube(bounds.center, bounds.size);
        //}

        //테스트용으로 임시 사용 
		//public void OnDrawGizmos()
		//{
  //          //          int src_count = _linearSearch_list.Count;
  //          //          //int cell_count = 0;
  //          //          for (int sc = 0; sc < src_count; sc++)
  //          //          {
  //          //              Bounds bb = _linearSearch_list[sc].GetBounds();
  //          //              Gizmos.DrawWireCube(bb.center, bb.size); 
  //          //          }

		//}

		public void UpdateCollision_AABBCulling()
		{
            
            //====================================
            // 발사체 갱신 
            //====================================
            int shot_count = _shots.Count;
            for (int si = 0; si < shot_count; si++)
            {
                _shots[si].Update_Shot();
            }
            //====================================

            Bounds cameraViewBounds = GetBounds_CameraView();
            Being selected = SingleO.touchControl._selected;
            if (null != (object)selected)
            {
                //SingleO.gridManager.Update_FogOfWar(selected.GetPos3D(), selected._move._direction);
                selected.SetVisible(true);
            }


            //Vector3 collisionCellPos_center = ConstV.v3_zero;
            CellSpace structTile = null;
            Being src = null, dst = null;
            //Vector3Int ix = ConstV.v3Int_zero;
            int src_count = _linearSearch_list.Count;
            for (int key = 0; key < src_count; key++)
            {
                src = _linearSearch_list[key];

                //============================
                src.UpdateAll(); //객체 갱신
                //============================

                if (true == src.isDeath()) continue;
                //if (false == src.gameObject.activeInHierarchy) continue; //<- 코드제거 : 죽지 않았으면 안보이는 상태라 해도 처리해야 한다 


                //===============================================================================


                //객체 컬링 처리
                if (null != (object)selected)
                {
                    //Culling_SightOfView(selected, src);
                }
                else
                {
                    //Culling_ViewFrustum(cameraViewBounds, src); //테스트를 위해 주석 
                }

                //============================
                //src.Apply_Bounds(); //경계상자 위치갱신
                _aabbCulling.SetEndPoint(key, src); //aabb 갱신 
                //============================
            }


            //==============================================
            //AABB 삽입정렬 및 충돌처리
            //==============================================
            _aabbCulling.UpdateXZ();

            //int overlapCount = _aabbCulling.GetOverlap().Count;
            //AABBCulling.UnOrderedEdgeKey[] toArray = _aabbCulling.GetOverlap().ToArray();
            //for (int i = 0; i < overlapCount;i++)
            //{
            //    src = _linearSearch_list[ toArray[i]._V0 ];
            //    dst = _linearSearch_list[ toArray[i]._V1 ];

            //    if (null == (object)src || true == src.isDeath()) continue;
            //    if (null == (object)dst || true == dst.isDeath()) continue;
            //    if ((object)src == (object)dst) continue;

            //    CollisionPush(src, dst);
            //}

            foreach (AABBCulling.UnOrderedEdgeKey key in _aabbCulling.GetOverlap()) //fixme : 조회속도가 빠른 자료구조로 바꾸기 
            {
                src = _linearSearch_list[key._V0];
                dst = _linearSearch_list[key._V1];

                //DebugWide.LogBlue(_aabbCulling.GetOverlap().Count + "   " + key._V0 + "  " + key._V1 + "   " + src + "  " + dst); //chamto test

                if (null == (object)src || true == src.isDeath()) continue;
                if (null == (object)dst || true == dst.isDeath()) continue;
                //if (false == src.gameObject.activeInHierarchy) continue; //<- 코드제거 : 죽지 않았으면 안보이는 상태라 해도 처리해야 한다 
                //if (false == dst.gameObject.activeInHierarchy) continue;
                if ((object)src == (object)dst) continue;

                CollisionPush(src, dst);
                //CollisionForce_Test(src, dst); //chamto test
            }
            //==============================================

            for (int key = 0; key < src_count; key++)
            {
                src = _linearSearch_list[key];

                //==========================================
                //동굴벽과 캐릭터 충돌처리 
                //if (SingleO.gridManager.HasStructTile(src.transform.position, out structTile))
                if (SingleO.cellPartition.HasStructTile(src.GetPos3D(), out structTile))
                {
                    CollisionPush_StructTile(src, structTile);
                    //CollisionPush_Rigid(src, structTile);
                }
                //==========================================

                src.Apply_UnityPosition();
            }
		}

		public void UpdateCollision_UseDirectGrid3x3()
        {
            //return; //chamto test

            //====================================
            // 발사체 갱신 
            //====================================
            int shot_count = _shots.Count;
            for (int si = 0; si < shot_count;si++)
            {
                _shots[si].Update_Shot();  
            }
            //====================================


            Bounds cameraViewBounds = GetBounds_CameraView();
            Being selected = SingleO.touchControl._selected;
            if (null != selected)
            {
                SingleO.gridManager.Update_FogOfWar(selected.transform.position, selected._move._direction);
                selected.SetVisible(true);
            }


            Vector3 collisionCellPos_center = ConstV.v3_zero;
            //CellInfo cellInfo = null;
            CellSpace cellSpace = null;

            //foreach (Being src in _linearSearch_list)
            Being src = null, dst = null;
            Vector3Int[] cellIndexes = null;
            Vector3Int ix = ConstV.v3Int_zero;
            int src_count = _linearSearch_list.Count;
            int cell_count = 0;
            for (int sc = 0; sc < src_count;sc++)
            {
                src = _linearSearch_list[sc];

                //============================
                src.UpdateAll(); //객체 갱신
                //============================

                //if (false == src.gameObject.activeInHierarchy) continue;
                //if (null == src._cellInfo) continue;
                if (true == src.isDeath()) continue;

                //src._move._direction = Misc.GetDir64_Normal3D(src._move._direction);
                //src._move._direction = src._move._direction.normalized; //성능상의 차이가 없음 

                //1. 3x3그리드 정보를 가져온다
                //foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[3]) //9개의 영역 : 객체200 fps60
                //foreach (Vector3Int ix in __cellIndexes_5) //5개의 영역 (중복되는 4개의 영역 제거) : 객체200 fps80
                //foreach(Vector3Int ix in __cellIndexes_Max4[this.GetOverlapCellSpace(src)]) //충돌원이 셀에 겹친 영역만 가져옴 (최대 4개 영역) : 객체 200 fps90
                cellIndexes = __cellIndexes_Max4[this.GetOverlapCellSpace(src)];
                cell_count = cellIndexes.Length;
                for (int cc = 0; cc < cell_count;cc++ )
                {
                    ix = cellIndexes[cc];

                    cellSpace = SingleO.cellPartition.GetCellSpace(ix + src._cellInfo._index);
                    //cellInfo = SingleO.gridManager.GetCellInfo(src._cellInfo._index);
                    //if (null == cellInfo) continue;

                    //foreach (Being dst in cellInfo)
                    //for (LinkedListNode<Being> curNode = cellInfo.First; null != curNode; curNode = curNode.Next)
                    Being next = cellSpace._children;
                    while(null != (object)next )
                    {
                        dst = next;
                        next = next._next_sibling;
                        //dst = curNode.Value;

                        //if (false == dst.gameObject.activeInHierarchy) continue;
                        if ((object)src == (object)dst) continue;
                        if (null == (object)dst || true == dst.isDeath()) continue;

                        CollisionPush(src, dst);
                    }
                }


                //동굴벽과 캐릭터 충돌처리 
                if (SingleO.cellPartition.HasStructTile(src.GetPos3D(), out cellSpace))
                {
                    CollisionPush_StructTile(src, cellSpace);
                    //CollisionPush_Rigid(src, structTile);
                }

                //객체 컬링 처리
                if(null != selected)
                {
                    //Culling_SightOfView(selected, src);
                }else
                {
                    //Culling_ViewFrustum(cameraViewBounds, src);
                }

            }

            //DebugWide.LogRed(_listTest.Count + "  총회전:" + count); //114 , 1988
        }



        public void Culling_SightOfView(Being selected , Being target)
        {
            
            bool onOff = false;
            //챔프 시야에 없으면 안보이게 처리함
            if (true == IsVisibleArea(selected, target.GetPos3D()))
            {
                onOff = true;
            }

            //술통은 항상 보이게 한다 -  임시 처리
            if ((object)target == (object)selected || Being.eKind.barrel == target._kind)  
            {
                onOff = true;
            }
            target.SetVisible(onOff);

        }


        public void Culling_ViewFrustum(Bounds viewBounds, Being target)
        {
            
            if (true == viewBounds.Contains(target.GetPos3D()))
            {
                target.SetVisible(true);
            }
            else
            {
                target.SetVisible(false);
            }

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

        }



        //가시영역 검사 
        public bool IsVisibleArea(Being src, Vector3 dstPos)
        {
            //return true; //chamto test
            
            //Vector3 dirToDst = dstPos - src.transform.position;
            Vector3 dirToDst = VOp.Minus(dstPos , src.transform.position);
            float dirToDstsq = dirToDst.sqrMagnitude;
            float DIS = GridManager.MeterToWorld * 7f;
            if (dirToDstsq < DIS*DIS) //목표와의 거리가 7미터 안
            {

                //대상과 정반대 방향이 아닐때 처리 
                //dirToDst.Normalize();
                //dirToDst = Misc.GetDir360_Normal3D(dirToDst); 
                dirToDst = VOp.Normalize(dirToDst); 
                DIS = GridManager.MeterToWorld * 2f;
                //if (Math.Cos(Mathf.Deg2Rad * 45f) < Vector3.Dot(src._move._direction, dirToDst) || dirToDstsq < DIS*DIS)
                if (45f > Geo.Angle_AxisY(src._move._direction, dirToDst) || dirToDstsq < DIS * DIS)
                {

                    //여기까지 오면 캐릭터 검사는 통과된것이다

                    //구조물 검사 
                    //SphereModel model = _sphereTree_struct.RayTrace_FirstReturn(src.transform.position, dstPos, null); 
                    //if (null == model) return true;

                    //보이는 위치의 타일인지 검사한다 
                    //if(true == SingleO.gridManager.IsVisibleTile(src._getPos3D, dstPos, 0.1f))
                    if (true == SingleO.cellPartition.IsVisibleTile(src, src.GetPos3D(), dstPos, 0.1f))
                        return true;
                }

            }

            return false;
        }

        //____________________________________________
        //              선분을 이용한 CCD   
        //____________________________________________
        //public Vector3[] LineSegmentTest(Vector3 origin, Vector3 last)
        //{
        //    LineSegment3 lineSeg = LineSegment3.zero;
        //    lineSeg.origin = origin;
        //    //lineSeg.direction = dir;
        //    lineSeg.last = last;

        //    LinkedList<Vector3> cellList = new LinkedList<Vector3>();
        //    float CELL_HARF_SIZE = SingleO.gridManager._cellSize_x * 0.5f;
        //    float CELL_SQUARED_RADIUS = Mathf.Pow(CELL_HARF_SIZE, 2f);
        //    float sqrDis = 0f;
        //    float t_c = 0;

        //    //기준셀값을 더해준다. 기준셀은 그리드값 변환된 값이이어야 한다 
        //    Vector3Int originToGridInt = SingleO.gridManager.ToPosition2D(origin);
        //    Vector3 originToPos = SingleO.gridManager.ToPosition3D(originToGridInt);
        //    Vector3 worldCellCenterPos = ConstV.v3_zero;
        //    foreach (Vector3Int cellLBPos in SingleO.gridManager._indexesNxN[7])
        //    {
        //        //셀의 중심좌표로 변환 
        //        worldCellCenterPos = SingleO.gridManager.ToPosition3D_Center(cellLBPos);
        //        worldCellCenterPos += originToPos;


        //        //시작위치셀을 포함하거나 뺄때가 있다. 사용하지 않느다 
        //        //선분방향과 반대방향인 셀들을 걸러낸다 , (0,0)원점 즉 출발점의 셀은 제외한다 
        //        if(0 == cellLBPos.sqrMagnitude || 0 >= Vector3.Dot(lineSeg.direction, worldCellCenterPos - origin))
        //        {
        //            continue;
        //        }

        //        sqrDis = lineSeg.MinimumDistanceSquared(worldCellCenterPos, out t_c);

        //        //선분에 멀리있는 셀들을 걸러낸다
        //        if(CELL_SQUARED_RADIUS < sqrDis)
        //        {
        //            continue;
        //        }

        //        cellList.AddLast(worldCellCenterPos);
        //    }


        //    Vector3[] result = (from v3 in cellList
        //                        orderby (v3-origin).sqrMagnitude ascending
        //                        select v3).ToArray();

        //    return result;
        //}

        //____________________________________________
        //                  충돌 검사   
        //____________________________________________
       
        //public void CollisionPush(Transform src, Transform dst)
        //{
        //    if (null == src || null == dst) return;

        //    Vector3 sqr_dis = ConstV.v3_zero;
        //    float r_sum = 0f;
        //    float max_radius = Mathf.Max(0.5f, 0.5f);

        //    //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
        //    sqr_dis = src.position - dst.position;
        //    r_sum = 0.5f + 0.5f;

        //    //1.두 캐릭터가 겹친상태 
        //    if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
        //    {
        //        //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

        //        //todo : 최적화 필요 

        //        Vector3 n = sqr_dis.normalized;
        //        //Vector3 n = sqr_dis;
        //        float meterPerSecond = 2f;

        //        //2.큰 충돌원의 반지름 이상으로 겹쳐있는 경우
        //        if (sqr_dis.sqrMagnitude < Mathf.Pow(max_radius, 2))
        //        {
        //            //3.완전 겹쳐있는 경우 , 방향값을 설정할 수 없는 경우
        //            if (n == ConstV.v3_zero)
        //            {
        //                //방향값이 없기 때문에 임의로 지정해 준다. 
        //                n = Misc.GetDir8_Random_AxisY();
        //            }

        //            meterPerSecond = 0.5f;

        //            //if(Being.eKind.spear != dst._kind)
        //            //DebugWide.LogBlue(src.name + " " + dst.name + " : " + sqr_dis.magnitude + "  " + max_radius);
        //        }


        //        src.Translate(n * (GridManager.ONE_METER * 2f) * (Time.deltaTime * (1f/meterPerSecond)));
        //        dst.Translate(-n * (GridManager.ONE_METER * 2f) * (Time.deltaTime * (1f / meterPerSecond)));

        //    }
        //}

        public void CollisionForce_Test(Being src, Being dst)
        {
            if (null == (object)src || null == (object)dst) return;

            float max_sqrRadius = dst._collider_sqrRadius;
            if (src._collider_sqrRadius > dst._collider_sqrRadius)
                max_sqrRadius = src._collider_sqrRadius;



            Vector3 dis = VOp.Minus(src.GetPos3D(), dst.GetPos3D());
            Vector3 n = ConstV.v3_zero;
            float dis_sqr = dis.sqrMagnitude;
            float r_sum = (src._collider_radius + dst._collider_radius);
            float r_sumsqr = r_sum * r_sum;
            //1.두 캐릭터가 겹친상태 
            if (dis_sqr < r_sumsqr)
            {
                //==========================================

                float length = (float)Math.Sqrt(dis_sqr);
                float btLength = (r_sum - length) * 0.5f;
                n = Misc.GetDir8_Normal3D(dis); //8방향으로만 밀리게 한다 

                src.SetForce(n, btLength);
                dst.SetForce(-n, btLength);

                if (float.Epsilon + 0.01f >= length)
                {
                    n = Misc.GetDir8_Random_AxisY();
                    length = 1f;
                    src.SetForce(n, 1f);
                    dst.SetForce(-n, 1f);

                }

                src.ReactionForce(dst, 1);
                dst.ReactionForce(src,1 );

            }
        }

        public void CollisionPush(Being src , Being dst)
        {
            if (null == (object)src || null == (object)dst) return;

            //float max_sqrRadius = Mathf.Max(src._collider_sqrRadius, dst._collider_sqrRadius);
            float max_sqrRadius = dst._collider_sqrRadius;
            if(src._collider_sqrRadius > dst._collider_sqrRadius)
                max_sqrRadius = src._collider_sqrRadius;


            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            //Vector3 dis = src.transform.localPosition - dst.transform.localPosition;
            //Vector3 dis = src._getPos3D - dst._getPos3D;
            Vector3 dis = VOp.Minus(src.GetPos3D() , dst.GetPos3D());
            Vector3 n = ConstV.v3_zero;
            float dis_sqr = dis.sqrMagnitude;
            //Vector3 dis = src._prevLocalPos - dst._prevLocalPos;
            float r_sum = (src._collider_radius + dst._collider_radius);
            float r_sumsqr = r_sum * r_sum;
            //1.두 캐릭터가 겹친상태 
            if (dis_sqr < r_sumsqr)
            {
                //==========================================
                float rate_src = 0.5f;
                float rate_dst = 0.5f;
                //if(Being.eKind.lothar == src._kind)
                //{
                //    rate_src = 0f;
                //    rate_dst = 1f;
                //}
                //if (Being.eKind.lothar == dst._kind)
                //{
                //    rate_src = 1f;
                //    rate_dst = 0f;
                //}

                n = Misc.GetDir8_Normal3D(dis); //8방향으로만 밀리게 한다 
                //n = dis;
                float length = (float)Math.Sqrt(dis_sqr);
                float btLength = (r_sum - length);
                float btLength_src = btLength * rate_src;
                float btLength_dst = btLength * rate_dst;
                if(0 == length)
                {
                    n = Misc.GetDir8_Random_AxisY();
                    length = 1f;
                    btLength_src = r_sum * 0.5f;
                    btLength_dst = r_sum * 0.5f;
                }
                //n.x /= length;
                //n.z /= length;

                src.SetPos(src.GetPos3D() + n * btLength_src);
                dst.SetPos(dst.GetPos3D() - n * btLength_dst);
                return;
                //DebugWide.LogBlue(n + "  " + btLength + "   " + length);
                //==========================================

                n = VOp.Normalize(dis);
                //Vector3 n = Misc.GetDir360_Normal3D(dis);
                //Vector3 n = dis.normalized;



                //DebugWide.LogBlue(dis + "  => " + n + "  compare : " + dis.normalized); //chamto test
                float meterPerSecond = 2f;

                //2.큰 충돌원의 반지름 이상으로 겹쳐있는 경우
                if (dis_sqr < max_sqrRadius)
                {
                    //3.완전 겹쳐있는 경우 , 방향값을 설정할 수 없는 경우
                    //if (n == ConstV.v3_zero)
                    if(Misc.IsZero(n))
                    {
                        //DebugWide.LogBlue(n); //chamto test
                        //방향값이 없기 때문에 임의로 지정해 준다. 
                        n = Misc.GetDir8_Random_AxisY();
                    }

                    meterPerSecond = 1.0f;

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

            }
        }


        //public void CollisionPush_Rigid(Being src, CellSpace structTile)
        //{
        //    //이상진동 : 방향의 평균내기 방식
        //    //Vector3 smoothDir = Misc.GetDir8Normal_AxisY(structTile._dir);
        //    //smoothDir += src._move._direction.normalized;
        //    //smoothDir /= 2f;
        //    //src._move.Move_Forward(smoothDir, 2f, 0.5f);
        //    //return;

        //    const float Tile_Radius = 0.08f;
        //    //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
        //    Vector3 sqr_dis = src.transform.localPosition - structTile._pos3d_center;
        //    float r_sum = src._collider_radius + Tile_Radius;

        //    //1.두 캐릭터가 겹친상태 
        //    if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
        //    {
        //        //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

        //        //todo : 최적화 필요 

        //        Vector3 n = sqr_dis.normalized;
        //        //n = Vector3.back;
        //        //Vector3 n = sqr_dis;
        //        float div_dis = 0.5f;

        //        //2.반지름 이상으로 겹쳐있는 경우
        //        if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
        //        {
        //            //3.완전 겹쳐있는 경우
        //            if (n == ConstV.v3_zero)
        //            {
        //                //방향값이 없기 때문에 임의로 지정해 준다. 
        //                n = Misc.GetDir8_Random_AxisY();
        //            }

        //            div_dis = 0.2f;
        //        }

        //        //src.transform.position = collisionCellPos_center + n * 0.16f;
        //        //src._move.Move_Forward(n, 2f, div_dis);
        //        src.Move_Forward(n, div_dis, true);
        //        //DebugWide.LogBlue(SingleO.gridManager.ToCellIndex(src.transform.position, ConstV.v3_up) + "   " + src.transform.position);
        //    }
        //}

        //고정된 물체와 충돌 검사 : 동굴벽 등 
        public void CollisionPush_StructTile(Being src, CellSpace structTile)
        {
            if (null == structTile) return;

            //Vector3 srcPos = src._transform.position;
            Vector3 srcPos = src.GetPos3D();
            //Vector3 centerToSrc_dir = srcPos - structTile._pos3d_center;
            Vector3 centerToSrc_dir = VOp.Minus(srcPos , structTile._pos3d_center);
            Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);

            float size = SingleO.gridManager._cellSize_x * 0.5f;
            Vector3 center = ConstV.v3_zero;
            LineSegment3 line3 = new LineSegment3();
            //8방향별 축값 고정  
            switch (structTile._eDir)
            {
                case eDirection8.up:
                    {
                        srcPos.z = structTile._pos3d_center.z + size;
                    }
                    break;
                case eDirection8.down:
                    {
                        srcPos.z = structTile._pos3d_center.z - size;
                    }
                    break;
                case eDirection8.left:
                    {
                        srcPos.x = structTile._pos3d_center.x - size;
                    }
                    break;
                case eDirection8.right:
                    {
                        srcPos.x = structTile._pos3d_center.x + size;
                    }
                    break;
                case eDirection8.leftUp:
                    {
                        //down , right
                        if(CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x - size;
                            srcPos.z = structTile._pos3d_center.z + size;
                            break;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        //down , left
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x + size;
                            srcPos.z = structTile._pos3d_center.z + size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        //up , right
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x - size;
                            srcPos.z = structTile._pos3d_center.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;

                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.rightDown:
                    {
                        //up , left
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x + size;
                            srcPos.z = structTile._pos3d_center.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;

            }
            //src._transform.position = srcPos;
            src.SetPos(srcPos);

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






        public struct Param_RangeTest
        {
            //==============================================
            public Camp.eRelation vsRelation;
            public ChampUnit srcUnit;
            public Camp.eKind src_campKind;
            public Vector3 src_pos;

            public float minRadius;
            public float maxRadius;
            public float maxRadiusSqr;

            public delegate bool Proto_ConditionCheck(ref Param_RangeTest param, SphereModel dstModel);
            public Proto_ConditionCheck callback;
            //==============================================

            public Param_RangeTest(ChampUnit in_srcUnit, Camp.eRelation in_vsRelation, float meter_minRadius, float meter_maxRadius)
            {
                vsRelation = in_vsRelation;
                srcUnit = in_srcUnit;
                src_campKind = in_srcUnit._campKind;
                src_pos = in_srcUnit.transform.position;
                minRadius = meter_minRadius * GridManager.ONE_METER;
                maxRadius = meter_maxRadius * GridManager.ONE_METER;
                maxRadiusSqr = maxRadius * maxRadius;

                callback = Param_RangeTest.Func_ConditionCheck;
            }

            //==============================================

            static public bool Func_ConditionCheck(ref Param_RangeTest param, SphereModel dstModel)
            {
                //return true;

                //기준객체는 검사대상에서 제외한다 
                if (param.srcUnit._sphereModel == dstModel) return false;

                ChampUnit dstUnit = dstModel.GetLink_UserData<ChampUnit>();

                if (null != (object)dstUnit && param.vsRelation != Camp.eRelation.Unknown)
                {
                    if (dstUnit.isDeath()) return false; //죽어있는 대상은 탐지하지 않는다 

                    Camp.eRelation getRelation = SingleO.campManager.GetRelation(param.src_campKind, dstUnit._belongCamp._eCampKind);

                    //요청 관계인지 검사한다 
                    if (param.vsRelation == getRelation)
                    {
                        //가시거리 검사 
                        return SingleO.cellPartition.IsVisibleTile(param.srcUnit, param.src_pos, dstModel.GetPos(), 0.1f);
                    }
                }

                return false;
            }
        }
        public ChampUnit tree_GetNearCharacter(ChampUnit src, Camp.eRelation vsRelation, float meter_minRadius, float meter_maxRadius)
        {
            //Old_GetNearCharacter(src, vsRelation, meter_minRadius, meter_maxRadius); //chamto test

            Param_RangeTest param = new Param_RangeTest(src, vsRelation, meter_minRadius, meter_maxRadius);
            SphereModel sphereModel = _sphereTree_being.RangeTest_MinDisReturn(ref param);


            if (null != sphereModel)
            {
                //DebugWide.LogBlue(sphereModel.GetLink_UserData<ChampUnit>()); //chamto test
                return sphereModel.GetLink_UserData<ChampUnit>();
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
            if (null == (object)src) return null;

            float wrd_minRad = meter_minRadius * GridManager.ONE_METER;
            float wrd_maxRad = meter_maxRadius * GridManager.ONE_METER;
            float sqr_minRadius = 0;
            float sqr_maxRadius = 0;
            float min_value = wrd_maxRad * wrd_maxRad * 1000f; //최대 반경보다 큰 최대값 지정
            float sqr_dis = 0f;


            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint NxN = SingleO.gridManager.GetNxNIncluded_CircleRadius(wrd_maxRad);
            int TILE_MAP_SIZE = SingleO.cellPartition._tileMapSize_x;

            //int count = 0;
            Index2 tempv2;
            CellSpace cell = null;
            ChampUnit target = null;
            Index2[] array = SingleO.gridManager._indexesNxN[NxN];
            int count = array.Length;
            for (int i = 0; i < count;i++)
            {
                //int pos1d = pos2d.x + pos2d.y * _tileMapSize_x; 1차원 값으로 변환 
                tempv2 = array[i]; 
                tempv2.x += src._cur_cell._pos2d.x;
                tempv2.y += src._cur_cell._pos2d.y;
                cell = SingleO.cellPartition.GetCellSpace(tempv2.y * TILE_MAP_SIZE + tempv2.x); 

                if (null == cell) continue;

                //foreach (Being dst in cell)
                Being dst = null;
                Being nextBeing = cell._children;
                while (null != (object)nextBeing)
                {
                    dst = nextBeing;
                    nextBeing = nextBeing._next_sibling;

                    ChampUnit champDst = dst as ChampUnit;
                    if (null == (object)champDst) continue;
                    if ((object)src == (object)dst) continue;
                    if (true == dst.isDeath()) continue; //쓰러진 객체는 처리하지 않는다 


                    if(vsRelation != Camp.eRelation.Unknown )//&& null != src._belongCamp && null != champDst._belongCamp)
                    {
                        Camp.eRelation getRelation = SingleO.campManager.GetRelation(src._belongCamp._eCampKind, champDst._belongCamp._eCampKind);

                        //요청 관계가 아니면 처리하지 않는다 
                        if (vsRelation != getRelation)
                            continue;
                    }

                    //count++;
                    //==========================================================
                    sqr_minRadius = (wrd_minRad + dst._collider_radius) * (wrd_minRad + dst._collider_radius);
                    sqr_maxRadius = (wrd_maxRad + dst._collider_radius) * (wrd_maxRad + dst._collider_radius);
                    sqr_dis = VOp.Minus(src.GetPos3D() , dst.GetPos3D()).sqrMagnitude;

                    //최대 반경 이내일 경우
                    if (sqr_minRadius <= sqr_dis && sqr_dis <= sqr_maxRadius)
                    {

                        //DebugWide.LogBlue(min_value + "__" + sqr_dis +"__"+  dst.name); //chamto test

                        //기존 객체보다 더 가까운 경우
                        if (min_value > sqr_dis)
                        {
                            min_value = sqr_dis;
                            target = champDst;

                            //첫번째 발견하면 바로 탈출한다 - 임시 
                            return target;
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
            
            Vector3 dir = ConstV.v3_zero;
            CellSpace cell = null;

            Index2[] array = SingleO.gridManager._indexesNxN[gridRange_NxN];
            int count = array.Length;
            for (int i = 0; i < count;i++)
            {
                
                cell = SingleO.cellPartition.GetCellSpace(array[i] + src._cur_cell._pos2d); //SingleO.gridManager.GetCellInfo(ix + src._cellInfo._index);
                if (null == cell) continue;

                Being dst = null;
                Being getBeing = cell._children;
                while(null != (object)getBeing)
                {
                    dst = getBeing;
                    getBeing = getBeing._next_sibling;

                    if ((object)src == (object)dst) continue;

                    if ((int)Behavior.eKind.Idle <= (int)dst._behaviorKind && (int)dst._behaviorKind <= (int)Behavior.eKind.Idle_Max)
                    {
                        dir = VOp.Minus(src.GetPos3D() , dst.GetPos3D());

                        //그리드범위에 딱들어가는 원을 설정, 그 원 밖에 있으면 처리하지 않는다 
                        //==============================
                        float sqr_radius = (float)(gridRange_NxN) * 0.5f; //반지름으로 변환
                        sqr_radius *= SingleO.gridManager._cellSize_x; //셀크기로 변환
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

            GameObject obj = CreatePrefab("0_champ/" +eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            ChampUnit cha = obj.AddComponent<ChampUnit>();
            ////obj.AddComponent<SortingGroup>(); //drawcall(batches) 증가 문제로 주석  
            Movement mov = obj.AddComponent<Movement>();
            mov._being = cha;
            obj.AddComponent<AI>();
            cha._id = _id_sequence;
            cha._kind = eKind;
            cha._belongCamp = belongCamp;
            cha.transform.position = pos;

            ////==============================================
            ////가지(촉수) 등록
            Limbs limbs_hand = Limbs.CreateLimbs_TwoHand(obj.transform);
            limbs_hand.Init();
            cha._limbs = limbs_hand;
            ////==============================================
            ////구트리 등록 
            SphereModel model = _sphereTree_being.AddSphere(pos, cha._collider_radius, SphereModel.Flag.TREE_LEVEL_LAST);
            _sphereTree_being.AddIntegrateQ(model);
            model.SetLink_UserData<ChampUnit>(cha);
            ////==============================================

            cha._sphereModel = model;
            cha.Init();

            ////==============================================

            _beings.Add(_id_sequence, cha);
            _linearSearch_list.Add(cha); //속도향상을 위해 중복된 데이터 추가

            return cha;
        }

        public Shot Create_Shot(Transform parent, Being.eKind eKind,  Vector3 pos)
        {
            _id_shot_sequence++;

            GameObject obj = CreatePrefab("1_effect/" + eKind.ToString(), parent, _id_shot_sequence.ToString("000") + "_" + eKind.ToString());
            Shot shot = obj.AddComponent<Shot>();
            obj.AddComponent<SortingGroup>();
            Movement mov = obj.AddComponent<Movement>();
            mov._being = shot;
            shot._id = _id_shot_sequence;
            shot._kind = eKind;
            shot.transform.position = pos;

            //==============================================
            //구트리 등록 
            SphereModel model = _sphereTree_being.AddSphere(pos, shot._collider_radius, SphereModel.Flag.TREE_LEVEL_LAST);
            _sphereTree_being.AddIntegrateQ(model);
            //==============================================

            shot._sphereModel = model;
            shot.Init();

            //==============================================

            ///////_shots.Add(_id_shot_sequence, shot);

            _shots.Add(shot);

            return shot;
        }

        public Obstacle Create_Obstacle(Transform parent, Being.eKind eKind, Vector3 pos)
        {
            _id_sequence++;

            GameObject obj = CreatePrefab("2_misc/" + eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            Obstacle obst = obj.AddComponent<Obstacle>();
            obj.AddComponent<SortingGroup>();
            Movement mov = obj.AddComponent<Movement>();
            mov._being = obst;
            obst._id = _id_sequence;
            obst._kind = eKind;
            obst.transform.position = pos;

            //==============================================
            //구트리 등록 
            SphereModel model = _sphereTree_being.AddSphere(pos, obst._collider_radius, SphereModel.Flag.TREE_LEVEL_LAST);
            _sphereTree_being.AddIntegrateQ(model);
            //==============================================

            obst._sphereModel = model;
            obst.Init();

            //==============================================

            _beings.Add(_id_sequence, obst);
            _linearSearch_list.Add(obst);

            return obst;
        }


        //List<Transform> _transformList = new List<Transform>();
        //public Transform Create_Test(Transform parent, Vector3 pos , bool manager)
        //{
            
        //    GameObject obj = CreatePrefab("test", parent,"test");
        //    obj.transform.localPosition = pos;

        //    if(true == manager)
        //        _transformList.Add(obj.transform);

        //    return obj.transform;
        //}
        //public Transform Create_Test2(Transform parent, Vector3 pos)
        //{

        //    GameObject obj = CreatePrefab("test", parent, "test");
        //    Being being = obj.AddComponent<Being>();
        //    obj.AddComponent<Movement>();
        //    being.transform.localPosition = pos;
        //    being._id = _id_sequence;
        //    being.Init();


        //    _linearSearch_list.Add(being);

        //    return obj.transform;
        //}


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
            DateTime _startDateTime;
            string _timeTemp = "";

            if (null == SingleO.unitRoot) return;

            //==============================================
            _startDateTime = DateTime.Now;
            string Blue_CampName = "Champ_Sky";
            string White_CampName = "Skel_Gray";
            string Obstacle_CampName = "ExitGoGe";
            int camp_position = 0;

            SingleO.campManager.Load_CampPlacement(Camp.eKind.Blue);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.White);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.Obstacle);

            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Blue, Camp.eKind.White);
            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Hero, Camp.eKind.Blue);
            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Hero, Camp.eKind.White);

            Camp camp_HERO = SingleO.campManager.GetDefaultCamp(Camp.eKind.Hero);
            Camp camp_BLUE = SingleO.campManager.GetCamp(Camp.eKind.Blue, Blue_CampName.GetHashCode());
            Camp camp_WHITE = SingleO.campManager.GetCamp(Camp.eKind.White, White_CampName.GetHashCode());
            Camp camp_Obstacle = SingleO.campManager.GetCamp(Camp.eKind.Obstacle, Obstacle_CampName.GetHashCode());
            _timeTemp += "  ObjectManager.Create_ChampCamp.CampInfo  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            //==============================================
            _startDateTime = DateTime.Now;

            Being being = null;
            ChampUnit champ = null;
            int numMax_create = 0;
            // -- 블루 진형 --
            champ = Create_Character(SingleO.unitRoot, Being.eKind.lothar, camp_HERO, camp_Obstacle.GetPosition(camp_position));
            champ._hp_max = 10000;
            champ._hp_cur = 10000;
            //champ.GetComponent<AI>()._ai_running = true;
            camp_position++;
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

            _timeTemp += "  ObjectManager.Create_ChampCamp.Create_Character  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            numMax_create = 1;
            for (int i = 0; i < numMax_create; i++)
            {
                champ = Create_Character(SingleO.unitRoot, Being.eKind.peasant, camp_BLUE, camp_BLUE.RandPosition());
                champ._hp_max = 30;
                champ._hp_cur = 30;
                //champ._mt_range_min = 0.3f;
                //champ._mt_range_max = 0.5f;
                //champ.GetComponent<AI>()._ai_running = true;
                camp_position++;
            }

            //===================================================

            // -- 휜색 진형 --
            camp_position = 0;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.raider, camp_WHITE, camp_WHITE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            numMax_create = 0;
            for (int i = 0; i < numMax_create; i++)
            { 
                champ = Create_Character(SingleO.unitRoot, Being.eKind.cleric, camp_WHITE, camp_WHITE.RandPosition());
                champ._mt_range_min = 1f;
                champ._mt_range_max = 4f;
                champ.GetComponent<AI>()._ai_running = true;
                camp_position++;
                //champ.SetColor(Color.black);
            }

            numMax_create = 0;
            for (int i = 0; i < numMax_create; i++)
            {
                champ = Create_Character(SingleO.unitRoot, Being.eKind.footman, camp_WHITE, camp_WHITE.RandPosition());
                champ.GetComponent<AI>()._ai_running = true;
                camp_position++;
            }

            //===================================================

            // -- 장애물 진형 --
            numMax_create = 0;
            for (int i = 0; i < numMax_create ;i++)
            {
                Create_Obstacle(SingleO.unitRoot, Being.eKind.barrel, camp_Obstacle.RandPosition());
            }

            //===================================================

            _startDateTime = DateTime.Now;

            // -- 발사체 미리 생성 --
            numMax_create = 300;
            for (int i = 0; i < numMax_create;i++)
            {
                //being = Create_Shot(SingleO.shotRoot, Being.eKind.spear, ConstV.v3_zero);
                being = Create_Shot(SingleO.shotRoot, Being.eKind.waterBolt, ConstV.v3_zero);
            }
            _timeTemp += "  ObjectManager.Create_ChampCamp.Create_Shot  : " +numMax_create+ " :  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //===================================================

            DebugWide.LogBlue(_timeTemp);
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
        //    return Vector3.SignedAngle(GetForwardDirect(), targetDir, ConstV.v3_up);

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
            if (null == (object)champTarget ) return false;

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

            float sqrDis = VOp.Minus(_me.GetPos3D() , _me._looking.GetPos3D()).sqrMagnitude;

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

            float sqrDis = VOp.Minus(_me.GetPos3D() , _me._looking.GetPos3D()).sqrMagnitude;

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

                        }else
                        {
                            if(_me._looking.isDeath())
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
                                _me.Attack(VOp.Minus(_me._looking.GetPos3D() , _me.GetPos3D()));
                                //_state = eState.Attack;
                                break;
                                //DebugWide.LogBlue("attack");
                            }
                           

                            Vector3 moveDir = VOp.Minus( _me._looking.GetPos3D() , _me.GetPos3D());
                            float second = 0.7f;
                            bool foward = true;

                            //상대와 너무 붙어 최소공격사거리 조건에 안맞는 경우 
                            if(_OUT_RANGE_MIN == result)
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


                        if(null != (object)_me._looking)
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
            if(_selected.isDeath()) 
            {
                _selected = null;
            }

		}

		private Vector3 __startPos = ConstV.v3_zero;
		private void TouchBegan() 
        {
            ChampUnit champ = null;
            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            __startPos = hit.point;
            __startPos.y = 0f;


            Being getBeing = hit.transform.GetComponent<Being>();
            if(null != (object)getBeing)
            {
                //쓰러진 객체는 처리하지 않는다 
                if (true == getBeing.isDeath()) return;
                
                //전 객체 선택 해제 
                if (null != (object)_selected)
                {
                    champ = _selected as ChampUnit;
                    if(null != champ)
                    {
                        champ.GetComponent<AI>()._ai_running = true;
                        //SingleO.lineControl.SetActive(champ._UIID_circle_collider, false);
                        champ._ui_circle.gameObject.SetActive(false);
                        champ._ui_hp.gameObject.SetActive(false);
                    }
                        

                }

                //새로운 객체 선택
                _selected = getBeing;

                champ = _selected as ChampUnit;
                if (null != (object)champ)
                {
                    _selected.GetComponent<AI>()._ai_running = false;
                    //SingleO.lineControl.SetActive(champ._UIID_circle_collider, true);
                    champ._ui_circle.gameObject.SetActive(true);
                    champ._ui_hp.gameObject.SetActive(true);
                }

                SingleO.cameraWalk.SetTarget(_selected._transform);
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

            if (null == (object)_selected) return;

            //챔프를 선택한 경우, 추가 처리 하지 않는다
            if ((object)getBeing == (object)_selected) return;

            //_selected.MoveToTarget(hit.point, 1f);
           
           
        }
        private void TouchMoved() 
        {
            if (null == (object)_selected) return;

            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            Vector3 touchDir = VOp.Minus(hit.point , _selected.GetPos3D());

            //_selected.Attack(hit.point - _selected.transform.position);
            //_selected.Block_Forward(hit.point - _selected.transform.position);
            _selected.Move_Forward(touchDir, 1f, true);

            ChampUnit champSelected = _selected as ChampUnit;
            if (null != (object)champSelected )
            {
                //임시처리 
                //최적화를 위해 주석처리 
                Being target = SingleO.objectManager.GetNearCharacter(champSelected, Camp.eRelation.Enemy, 
                                                                      champSelected.attack_range_min, champSelected.attack_range_max);
                if(null != target)
                {
                    if (true == SingleO.objectManager.IsVisibleArea(champSelected, target.transform.position))
                    {
                        champSelected.Attack(target.GetPos3D() - _selected.GetPos3D(), target);
                    }

                    //_selected.Move_Forward(hit.point - _selected._getPos3D, 3f, true); 
                        
                }

                //champSelected.Attack(champSelected._move._direction); //chamto test

            }



            //View_AnimatorState();
        }
        private void TouchEnded() 
        {
            if (null == (object)_selected) return;

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

        private Vector2 _prevTouchMovedPos = ConstV.v3_zero;
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


        void Update()
        {
            //화면상 ui를 터치했을 경우 터치이벤트를 보내지 않는다 
            if (null != (object)EventSystem.current && null != (object)EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //SendTouchEvent_Device_Target();
                SendTouchEvent_Device_NonTarget();
            }
            else //if (Application.platform == RuntimePlatform.OSXEditor )
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
            else //if (Application.platform == RuntimePlatform.OSXEditor)
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
            //foreach (GameObject o in _sendList)
            for (int i = 0; i < _sendList.Count;i++)
            {
                GameObject o = _sendList[i];
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
            //foreach (GameObject o in _sendList)
            for (int i = 0; i < _sendList.Count; i++)
            {
                GameObject o = _sendList[i];
            
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
