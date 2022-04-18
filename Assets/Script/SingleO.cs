using System;
using UnityEngine;
using System.Collections;

using HordeFight;
using UtilGS9;

//========================================================
//==================      전역  객체      ==================
//========================================================
//namespace HordeFight
//{
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

    static public GT FindHierarchy<GT>(string name) where GT : class
    {

        GameObject obj = GameObject.Find(name);
        if (null != obj)
        {
            return obj.GetComponent<GT>();
        }
        return null;
    }

    public static void Init_Tool(GameObject parent)
    {
        mainCamera = FindHierarchy<Camera>("Main Camera");
        lightDir = FindHierarchy<Transform>("light_dir");
        groundY = FindHierarchy<Transform>("groundY");

        hierarchy = CSingleton<HierarchyPreLoader>.Instance;
        SingleO.hierarchy.Init(); //계층도 읽어들이기 
        resourceManager = CSingleton<ResourceManager>.Instance;
        SingleO.resourceManager.Init(); //스프라이트 로드 

        skillBook = CSingleton<SkillBook>.Instance;

        cameraWalk = parent.AddComponent<CameraWalk>();
        touchEvent = parent.AddComponent<TouchEvent>();
        //touchControl = parent.AddComponent<TouchControl>();
    }

    public static void Init(GameObject parent)
    {

        DateTime _startDateTime;
        string _timeTemp = "";

        //==============================================
        uiMain = parent.AddComponent<UI_Main>();
        uiMain.Init();
        uiMain._gameText.text = "loading....";

        mainCamera = FindHierarchy<Camera>("Main Camera");
        canvasRoot = FindHierarchy<Canvas>("Canvas");
        gridRoot = FindHierarchy<Transform>("0_grid");
        unitRoot = FindHierarchy<Transform>("0_unit");
        shotRoot = FindHierarchy<Transform>("0_shot");
        debugRoot = FindHierarchy<Transform>("z_debug");
        lightDir = FindHierarchy<Transform>("light_dir");
        groundY = FindHierarchy<Transform>("groundY");

        campManager = CSingleton<CampManager>.Instance;
        cellPartition = CSingleton<CellSpacePartition>.Instance;
        coroutine = CSingleton<WideCoroutine>.Instance;
        //hashMap = CSingleton<HashToStringMap>.Instance;
        skillBook = CSingleton<SkillBook>.Instance;
        //==============================================

        SingleO.debugViewer._origin = SingleO.hierarchy.GetTransformA("z_debug/origin");
        SingleO.debugViewer._target = SingleO.hierarchy.GetTransformA("z_debug/target");

        _startDateTime = DateTime.Now;
        hierarchy = CSingleton<HierarchyPreLoader>.Instance;
        SingleO.hierarchy.Init(); //계층도 읽어들이기 
        _timeTemp += "  hierarchy.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        

        _startDateTime = DateTime.Now;
        resourceManager = CSingleton<ResourceManager>.Instance;
        SingleO.resourceManager.Init(); //스프라이트 로드 
        ResolutionController.CalcViewportRect(SingleO.canvasRoot, SingleO.mainCamera); //화면크기조정
        _timeTemp += "  resourceManager. CalcViewportRect  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        
    //==============================================

        cameraWalk = parent.AddComponent<CameraWalk>();
        touchEvent = parent.AddComponent<TouchEvent>();
        touchControl = parent.AddComponent<TouchControl>();
        
        lineControl = parent.AddComponent<LineControl>();

        if (null != (object)debugRoot)
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

    static public IEnumerator InitCoroutine(GameObject parent)
    {
        DateTime _startDateTime;
        string _timeAll = "";
        string tempStr = "";

        //==============================================
        uiMain = parent.AddComponent<UI_Main>();
        uiMain.Init();
        uiMain._gameText.text = "loading....";

        mainCamera = FindHierarchy<Camera>("Main Camera");
        canvasRoot = FindHierarchy<Canvas>("Canvas");
        gridRoot = FindHierarchy<Transform>("0_grid");
        unitRoot = FindHierarchy<Transform>("0_unit");
        shotRoot = FindHierarchy<Transform>("0_shot");
        debugRoot = FindHierarchy<Transform>("z_debug");
        lightDir = FindHierarchy<Transform>("light_dir");
        groundY = FindHierarchy<Transform>("groundY");

        campManager = CSingleton<CampManager>.Instance;
        cellPartition = CSingleton<CellSpacePartition>.Instance;
        coroutine = CSingleton<WideCoroutine>.Instance;
        //hashMap = CSingleton<HashToStringMap>.Instance;
        skillBook = CSingleton<SkillBook>.Instance;
        //==============================================


        yield return 0;

        _startDateTime = DateTime.Now;
        hierarchy = CSingleton<HierarchyPreLoader>.Instance;
        SingleO.hierarchy.Init(); //계층도 읽어들이기
        tempStr = "  hierarchy.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        _timeAll += tempStr;

        uiMain._gameText.text = tempStr;
        yield return 0;

        _startDateTime = DateTime.Now;
        resourceManager = CSingleton<ResourceManager>.Instance;
        SingleO.resourceManager.Init(); //스프라이트 로드 
        ResolutionController.CalcViewportRect(SingleO.canvasRoot, SingleO.mainCamera); //화면크기조정
        tempStr = "  resourceManager. CalcViewportRect  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        _timeAll += tempStr;

        uiMain._gameText.text = tempStr;
        yield return 0;
        //==============================================

        cameraWalk = parent.AddComponent<CameraWalk>();
        touchEvent = parent.AddComponent<TouchEvent>();
        touchControl = parent.AddComponent<TouchControl>();

        lineControl = parent.AddComponent<LineControl>();

        if (null != (object)debugRoot)
        {
            debugViewer = debugRoot.gameObject.AddComponent<DebugViewer>();
            debugViewer._origin = SingleO.hierarchy.GetTransformA("z_debug/origin");
            debugViewer._target = SingleO.hierarchy.GetTransformA("z_debug/target");
        }


        //==============================================

        _startDateTime = DateTime.Now;
        gridManager = parent.AddComponent<GridManager>();
        gridManager.Init();
        tempStr = "  GridManager.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        _timeAll += tempStr;

        uiMain._gameText.text = tempStr;
        yield return 0;

        _startDateTime = DateTime.Now;
        objectManager = parent.AddComponent<ObjectManager>();
        objectManager.Init();
        tempStr = "  ObjectManager.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        _timeAll += tempStr;

        uiMain._gameText.text = tempStr;
        yield return 0;

        _startDateTime = DateTime.Now;
        pathFinder = parent.AddComponent<PathFinder>();
        pathFinder.Init();
        tempStr = "  PathFinder.Init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
        _timeAll += tempStr;


        DebugWide.LogBlue(_timeAll);
        //==============================================

        uiMain._gameText.text = tempStr;
        yield break;
        //yield return new WaitForSeconds(0.001f);
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
    public static CampManager campManager = null;
    public static CellSpacePartition cellPartition = null;
    public static ResourceManager resourceManager = null;
    public static HierarchyPreLoader hierarchy = null;
    public static WideCoroutine coroutine = null;
    //public static HashToStringMap hashMap = null;
    public static SkillBook skillBook = null;

}

//}//end namespace
