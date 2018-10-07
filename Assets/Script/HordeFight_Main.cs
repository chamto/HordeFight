﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

using Utility;

namespace HordeFight
{
    public class HordeFight_Main : MonoBehaviour
    {
        
        // Use this for initialization
        void Start()
        {
            ResolutionController.CalcViewportRect(SingleO.canvasRoot, SingleO.mainCamera); //화면크기조정
            SingleO.resourceManager.Init(); //스프라이트 로드 

            gameObject.AddComponent<LineControl>();

            gameObject.AddComponent<PathFinder>();
            gameObject.AddComponent<GridManager>();
            gameObject.AddComponent<ObjectManager>();

            gameObject.AddComponent<UI_Main>();
            gameObject.AddComponent<DebugViewer>();

            gameObject.AddComponent<TouchProcess>();

            //===================

            SingleO.objectManager.Create_StageInfo();


        }

        // Update is called once per frame
        //void Update()
        void FixedUpdate()
        {

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

        public static WideCoroutine coroutine
        {
            get
            {
                return CSingleton<WideCoroutine>.Instance;
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
        private Vector3 _end = Vector3.zero;
        public void DrawLine(Vector3 start, Vector3 end)
        {
            _start = start;
            _end = end;
        }

		//private void Update()
        void FixedUpdate()
		{
            Debug.DrawLine(_start, _end);
		}
	}
}


//========================================================
//==================     그리드 관리기     ==================
//========================================================

namespace HordeFight
{
    //public struct CellIndex
    //{
    //    public int n1;
    //    public int n2;

    //    public CellIndex(int a1, int a2)
    //    {
    //        n1 = a1;
    //        n2 = a2;
    //    }

    //    public override string ToString()
    //    {
    //        return "(" + n1 + ", " + n2 + ")";
    //    }

    //    public static CellIndex operator +(CellIndex a, CellIndex b)
    //    {
    //        return new CellIndex(a.n1 + b.n1, a.n2 + b.n2);
    //    }

    //    public static bool operator ==(CellIndex a, CellIndex b)
    //    {
    //        if (a.n1 == b.n1 && a.n2 == b.n2)
    //            return true;

    //        return false;
    //    }

    //    public static bool operator !=(CellIndex a, CellIndex b)
    //    {
    //        if (a.n1 == b.n1 && a.n2 == b.n2)
    //            return false;

    //        return true;
    //    }
    //}



    public class CellInfo : LinkedList<Being>
    {
        
        public Vector3Int _index = default(Vector3Int);

    }



    public class GridManager : MonoBehaviour
    {

        private Grid _grid = null;
        private Tilemap _structTilemap = null;
        public Dictionary<Vector3Int,CellInfo> _cellList = new Dictionary<Vector3Int,CellInfo>();

        //중심이 (0,0)인 nxn 그리드 인덱스 값을 미리 구해놓는다
        public Dictionary<uint, Vector3Int[]> _indexesNxN = new Dictionary<uint, Vector3Int[]>();

        public const int NxN_MIN = 3;   //그리드 범위 최소크기
        public const int NxN_MAX = 11;  //그리드 범위 최대크기

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


		private void Start()
		{
            _grid = GameObject.Find("0_grid").GetComponent<Grid>();
            GameObject o = GameObject.Find("Tilemap_Structure");
            if(null != o)
            {
                _structTilemap = o.GetComponent<Tilemap>();    
            }

            _indexesNxN[3] = CreateIndexesNxN_SquareCenter_Tornado(3, Vector3.up);
            _indexesNxN[5] = CreateIndexesNxN_SquareCenter_Tornado(5, Vector3.up);
            _indexesNxN[7] = CreateIndexesNxN_SquareCenter_Tornado(7, Vector3.up);
            _indexesNxN[9] = CreateIndexesNxN_SquareCenter_Tornado(9, Vector3.up);
            _indexesNxN[11] = CreateIndexesNxN_SquareCenter_Tornado(11, Vector3.up); //화면 세로길이를 벗어나지 않는 그리드 최소값

		}

		//private void Update()
        void FixedUpdate()
		{
			
		}

        //원 반지름 길이를 포함하는 그리드범위 구하기
        public uint GetNxNIncluded_CircleRadius(float maxRadius)
        {
            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint NxN = (uint)((maxRadius * 2) / this.cellSize_x); //소수점 버림을 한다 
            if (0 == NxN % 2) NxN -= 1; //짝수라면 홀수로 변환한다
            if (NxN > GridManager.NxN_MAX) NxN = GridManager.NxN_MAX;
            if (NxN < GridManager.NxN_MIN) NxN = GridManager.NxN_MIN;

            return NxN;
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
                        index.x = i;
                        index.y = 0;
                        index.z = j;    
                    }
                        if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        index.x = i;
                        index.y = j;
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

                    if (Vector3.up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        dir = Quaternion.Euler(0, -90f, 0) * dir;
                        //DebugWide.LogBlue(dir + "  " + Tornado_Num); //chamto test
                    }
                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, -90f) * dir;
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
                    
                    if (Vector3.up == axis)
                    {
                        //y축 중심 : 3d용 좌표

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

                        dir = Quaternion.Euler(0, angle, 0) * dir;
                    }

                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Count/4)+1; //1 1 1 1 2 2 2 2 3 3 3 3 .... 이렇게 증가하는 수열값임 

                    Tornado_Count++;
                }

            }

            return indexes;

        }

        public Tilemap GetStructTileMap()
        {
            return _structTilemap;
        }

        public bool HasStructTile(Vector3 tilePos)
        {
            Vector3Int tileInt = _structTilemap.WorldToCell(tilePos);

            RuleTile rTile = _structTilemap.GetTile<RuleTile>(tileInt);
            if (null != rTile)
            {
                return true;
            }

            return false;
        }
        public bool HasStructTile(Vector3 tilePos, out Vector3 tileCenterToWorld)
        {
            tileCenterToWorld = Vector3.zero;
            if (null == _structTilemap) return false;

            //x-z 평면상에 타일맵을 놓아도 내부적으로는 x-y 평면으로 처리된다
            Vector3Int tileInt = _structTilemap.WorldToCell(tilePos);

            //x-z 평면을 가정하고 처리
            //CellToWorld 함수의 값과 cellSize 값이 부정확하여 직접 계산한다 
            //Vector3 의 ToString 함수내부에서 원래값을 반올림하여 출력한다는 것을 알게 되었다. 값은 올바로 가지고 있는 것이었음 
            tileCenterToWorld.x = (float)tileInt.x * cellSize_x + (cellSize_x * 0.5f);
            tileCenterToWorld.y = 0;
            tileCenterToWorld.z = (float)tileInt.y * cellSize_z + (cellSize_z * 0.5f);

            //DebugWide.LogBlue(tileInt.x + "  " + cellSize_x + "   " + tileInt.x * cellSize_x + "   " + tileCenterToWorld.x + "   "  + _structTilemap.cellSize.x); //chamto test

            RuleTile rTile = _structTilemap.GetTile<RuleTile>(tileInt);
            if (null != rTile)
            {
                //DebugWide.LogBlue(tileInt + "  " + tilePos + "   " + tileCenterToWorld.x + ", " + tileCenterToWorld.z ); //chamto test
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
                    index.z = j + center.z;
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


        public Vector3Int ToCellIndex(Vector3 pos , Vector3 axis)
        {

            Vector3Int cellIndex = Vector3Int.zero;
            Vector3 cellSize = Vector3.zero;

            if(Vector3.up == axis)
            {
                //그리드 자체에 스케일을 적용시킨 경우가 있으므로 스케일값을 적용한다. 
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x) ;
                cellSize.z = (_grid.cellSize.y * _grid.transform.localScale.z) ;


                if(0 <= pos.x)
                {
                    //양수일때는 소수점을 버린다. 
                    cellIndex.x = (int)(pos.x / cellSize.x);
                }
                else
                {
                    //음수일때는 올림을 한다. 
                    cellIndex.x = (int)((pos.x / cellSize.x) - 0.9f);
                }


                if (0 <= pos.z)
                { 
                    cellIndex.z = (int)(pos.z / cellSize.z);
                }
                else
                { 
                    cellIndex.z = (int)((pos.z / cellSize.z) - 0.9f);
                }

            }

            return cellIndex;
        }

        public Vector3 ToCenterPosition(Vector3 worldPos, Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            Vector3Int cellPos = this.ToCellIndex(worldPos, axis);

            return this.ToPosition_Center(cellPos, axis);
        }

        public Vector3 ToPosition_Center(Vector3Int ci , Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            if (Vector3.up == axis)
            {
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x);
                cellSize.z = (_grid.cellSize.y * _grid.transform.localScale.z);

                pos.x = (float)ci.x * cellSize.x;
                pos.z = (float)ci.z * cellSize.z;

                //셀의 중간에 위치하도록 한다
                pos.x += cellSize.x * 0.5f;
                pos.z += cellSize.z * 0.5f;
            }

            return pos;
        }

        public Vector3 ToPosition(Vector3Int ci, Vector3 axis)
        {
            Vector3 pos = Vector3.zero;
            Vector3 cellSize = Vector3.zero;

            if (Vector3.up == axis)
            {
                cellSize.x = (_grid.cellSize.x * _grid.transform.localScale.x);
                cellSize.z = (_grid.cellSize.y * _grid.transform.localScale.z);

                pos.x = (float)ci.x * cellSize.x;
                pos.z = (float)ci.z * cellSize.z;

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
        public List<Being> _being_list = new List<Being>(); //충돌처리 속도를 높이기 위해 사전정보와 동일한 객체를 리스트에 넣음 

        private int __TestSkelCount = 3;

        private void Start()
        {

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
        public Vector3[] LineSegmentTest(Vector3 origin, Vector3 dir)
        {
            LineSegment3 lineSeg = LineSegment3.zero;
            lineSeg.origin = origin;
            lineSeg.direction = dir;

            LinkedList<Vector3> cellList = new LinkedList<Vector3>();
            float CELL_HARF_SIZE = SingleO.gridManager.cellSize_x * 0.5f;
            float CELL_SQUARED_RADIUS = Mathf.Pow(CELL_HARF_SIZE, 2f);
            float sqrDis = 0f;
            float t_c = 0;

            //기준셀값을 더해준다. 기준셀은 그리드값 변환된 값이이어야 한다 
            Vector3Int originToGridInt = SingleO.gridManager.ToCellIndex(origin, Vector3.up);
            Vector3 originToPos = SingleO.gridManager.ToPosition(originToGridInt, Vector3.up);
            //DebugWide.LogBlue(toGridInt);
            foreach (Vector3Int cellLBPos in SingleO.gridManager._indexesNxN[7])
            {
                //셀의 중심좌표로 변환 
                Vector3 worldCellCenterPos = SingleO.gridManager.ToPosition_Center(cellLBPos, Vector3.up);
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

                //밀리는 처리 
                //if(Being.eKind.skeleton !=  src._kind || Being.eKind.skeleton == dst._kind)
                src._move.Move_Forward(n, 2f, div_dis);
                //if (Being.eKind.skeleton != dst._kind  || Being.eKind.skeleton == src._kind)
                dst._move.Move_Forward(-n, 2f, div_dis);

            }
        }

        //챔프를 중심으로 3x3그리드 영역의 정보를 가지고 충돌검사한다
        public void UpdateCollision_UseGrid3x3() //3x3 => 5x5 => 7x7 ... 홀수로 그리드 범위를 늘려 테스트 해볼 수 있다
        {
            CellInfo cellInfo = null;
            foreach(Being src in _being_list)
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

        //고정된 물체와 충돌 검사 : 동굴벽 등 
        public void CollisionPush_Rigid(Being src, Vector3 collisionCellPos_center, float rigRadius)
        {

            Vector3 sqr_dis = collisionCellPos_center - src._lastCellPos_withoutCollision;

            Vector3 pos = src.transform.position;

            eDirection8 eDirection = Misc.TransDirection8_AxisY(sqr_dis);
            //DebugWide.LogBlue(eDirection + "   "  + collisionCellPos_center + "   " + SingleO.gridManager.ToCellIndex(src._lastCellPos_withoutCollision, Vector3.up) );



            switch (eDirection)
            {
                case eDirection8.up:
                case eDirection8.down:
                    {
                        pos.z = collisionCellPos_center.z - 0.08f;
                    }
                    break;
                case eDirection8.left:
                case eDirection8.right:
                    {
                        pos.x = collisionCellPos_center.x - 0.08f;
                    }
                    break;
                default:
                    {
                        pos.z = collisionCellPos_center.z;
                        //pos = src._lastCellPos_withoutCollision;
                    }
                    break;
            }

            src.transform.position = pos;

        }

        public void CollisionPush_Rigid2(Being src, Vector3 collisionCellPos_center, float rigRadius)
        {

            Vector3 sqr_dis = Vector3.zero;
            float r_sum = 0f;

            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            sqr_dis = src.transform.localPosition - collisionCellPos_center;
            r_sum = src.GetCollider_Radius() + rigRadius;

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

                    div_dis = 0.3f;
                }



                //src._move.Move_Forward(n, div_dis, 1);
                //DebugWide.LogBlue(SingleO.gridManager.ToCellIndex(src.transform.position, Vector3.up) + "   " + src.transform.position);
                

            }
        }

        public void UpdateCollision_UseDirectGrid3x3()
        {
            //int count = 0;
            Vector3 collisionCellPos_center = Vector3.zero;
            CellInfo cellInfo = null;
            foreach (Being src in _being_list)
            {
                if (null == src._cellInfo) continue;

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
                        CollisionPush(src, dst);
                    }
                }



                //동굴벽과 캐릭터 충돌처리 
                if (SingleO.gridManager.HasStructTile(src.transform.position, out collisionCellPos_center))
                {
                    CollisionPush_Rigid2(src, collisionCellPos_center, 0.2f);
                }



            }

            //DebugWide.LogRed(_listTest.Count + "  총회전:" + count); //114 , 1988
        }

        //딕셔너리 보다 인덱싱 속도가 빠르다. 안정적 객체수 : 500
        public void UpdateCollision_UseList()
        {
            //int count = 0;
            Being src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < _beings.Count - 1; i++)
            {
                for (int j = i + 1; j < _beings.Count; j++)
                {
                    src = _being_list[i];
                    dst = _being_list[j];

                    CollisionPush(src, dst);

                    //count++;
                }
            }

            //DebugWide.LogRed(_beings.Count + "  총회전:" + count); //114 , 6441
        }

        public void UpdateCollision_UseDictForeach()
        {
            
            foreach(Being src in _beings.Values)
            {
                foreach (Being dst in _beings.Values)
                {
                    if (src == dst) continue;
                    CollisionPush(src, dst);
                }
            }


        }


        //딕션너리의 ElementAt 을 사용하여 객체를 가져오는것은 부하가 심하다. 안정적 객체수 : 100이하 
        public void UpdateCollision_UseDictElementAt()
        {
            
            Being src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < _beings.Count - 1; i++)
            {
                for (int j = i + 1; j < _beings.Count; j++)
                {
                    src = _beings.Values.ElementAt(i);
                    dst = _beings.Values.ElementAt(j);

                    CollisionPush(src, dst);

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



        public Being GetNearCharacter(Being src, float minRadius, float maxRadius)
        {
            float sqr_minRadius = Mathf.Pow(minRadius, 2);
            float sqr_maxRadius = Mathf.Pow(maxRadius, 2);
            float min_value = sqr_maxRadius * 2f; //최대 반경보다 큰 최대값 지정
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

                    //count++;
                    //==========================================================v
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
        /// 최소 반경보다 크고 최대 반경보다 작은 범위 안에서 가장 가까운 객체를 반환한다
        /// </summary>
        public Being GetNearCharacter_old(Being exceptChar, float minRadius, float maxRadius)
        {
            
            float sqr_minRadius = Mathf.Pow(minRadius, 2);
            float sqr_maxRadius = Mathf.Pow(maxRadius, 2);
            float min_value = sqr_maxRadius * 2f; //최대 반경보다 큰 최대값 지정
            float sqr_dis = 0f;
            Being target = null;
            //foreach (Being t in _beings.Values)
            foreach (Being t in _being_list)
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

                        dst.Idle_View(dir, true);
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
            _being_list.Add(cha); //chamto test

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

            if (null == SingleO.unitRoot) return;

            uint id_sequence = 0;
            Vector3 pos = Vector3.zero;
            //Create_Character(SingleO.unitRoot, Being.eKind.lothar, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.garona, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.footman, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.spearman, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.brigand, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.ogre, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.conjurer, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.slime, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.raider, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.grunt, id_sequence++, pos);
            Create_Character(SingleO.unitRoot, Being.eKind.knight, id_sequence++, pos);//.SetAIRunning(false);


            for (int i = 0; i < __TestSkelCount;i++)
            {
                //pos.x = (float)Misc.rand.NextDouble() * Mathf.Pow(-1f, i);
                //pos.z = (float)Misc.rand.NextDouble() * Mathf.Pow(-1f, i);
                Create_Character(SingleO.unitRoot, Being.eKind.skeleton, id_sequence++, pos);
            }

            //Create_Character(SingleO.unitRoot, Being.eKind.daemon, id_sequence++, pos);
            //Create_Character(SingleO.unitRoot, Being.eKind.waterElemental, id_sequence++, pos);
            Create_Character(SingleO.unitRoot, Being.eKind.fireElemental, id_sequence++, pos);

        }

    }
}


namespace HordeFight
{
    //========================================================
    //==================     캐릭터 정보(임시)     ==================
    //========================================================

  //  public partial class  Character : MonoBehaviour
  //  {

  //      //애니
  //      private Animator                    _animator = null;
  //      private AnimatorOverrideController  _overCtr = null;
  //      private SpriteRenderer              _sprRender = null;

  //      //이동
  //      private Movable _move     = null;
  //      private eDirection8 _eDir8 = eDirection8.down;

  //      //상태
  //      public eState   _eState = eState.None;

  //      //UI
  //      public int      _UIID_circle = -1;
  //      public int      _UIID_hp = -1;

  //      //고유정보
  //      public int _id = -1;
  //      public eKind _eKind = eKind.None;

  //      public float    _disPerSecond = 1f; //초당 이동거리 

  //      public ushort   _power = 1;
  //      public ushort   _hp_cur = 10;
  //      public ushort   _hp_max = 10;
  //      public float    _range_min = 0.15f;
  //      public float    _range_max = 0.15f;
  //      public bool     _death = false;
       

  //      public enum eKind
  //      {
  //          None = 0,
  //          footman,
  //          lothar,
  //          skeleton,
  //          garona,
  //          conjurer,
  //          raider,
  //          slime,
  //          spearman,
  //          grunt,
  //          brigand,
  //          knight,
  //          ogre,
  //          daemon,
  //          waterElemental,
  //          fireElemental,

  //      }

  //      public enum eState
  //      {
  //          None        = 0,

  //          Idle        = 10,
  //          Idle_Random = 11,
  //          Idle_LookAt = 12,
  //          Idle_Max    = 19,

  //          Move        = 20,
  //          Move_Max    = 29,

  //          Attack      = 30,
  //          Attack_Max  = 39,

  //          FallDown        = 40,
  //          FallDown_Max    = 49,

  //      }


  //      public void Init_Create()
  //      {
  //          _animator = GetComponentInChildren<Animator>();
  //          _sprRender = GetComponentInChildren<SpriteRenderer>();

  //          _move = GetComponent<Movable>();
  //          //Single.touchProcess.Attach_SendObject(this.gameObject);

  //          //오버라이드컨트롤러를 생성해서 추가하지 않고, 미리 생성된 것을 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
  //          _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
  //          _overCtr.name = "divide_character_"+ _id.ToString();
  //          _animator.runtimeAnimatorController = _overCtr;

  //      }

  //      //ref : https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
  //      private void Start()
  //      {
  //          _eState = eState.Idle_Random;
  //          //_animator.SetInteger("state", (int)eState.Idle);

  //          _UIID_circle = Single.lineControl.Create_Circle_AxisY(this.transform);
  //          Single.lineControl.SetActive(_UIID_circle, false);

  //          _UIID_hp = Single.lineControl.Create_LineHP_AxisY(this.transform);

  //      }



  //      private void Update()
  //      {
  //          if (true == _death) return;

  //          if (9 > _hp_cur)
  //          {
  //              //DebugWide.LogBlue("death  " + _eState);
  //              _death = true;
  //              _eState = eState.FallDown;
  //              FallDown();
  //          }

  //          Update_Shot();


  //          if (eState.Idle == _eState)
  //          {
  //              _animator.SetInteger("state", (int)eState.Idle);
  //          }
  //          else if (eState.Idle_Random == _eState)
  //          {
  //              _animator.SetInteger("state", (int)eState.Idle);
  //              Idle_Random();

  //          }
  //          else if (eState.Move == _eState)
  //          {
  //              _animator.SetInteger("state", (int)eState.Move);
  //          }
  //          else if (eState.Attack == _eState)
  //          {
  //              _animator.SetInteger("state", (int)eState.Attack);
  //          }
  //          else if (eState.FallDown == _eState)
  //          {
  //              _animator.SetInteger("state", (int)eState.FallDown);
  //          }



  //          //y축값이 작을수록 먼저 그려지게 한다. 캐릭터간의 실수값이 너무 작아서 100배 한후 소수점을 버린값을 사용함
  //          _sprRender.sortingOrder = -(int)(transform.position.z * 100f);
  //      }

  //      public float GetCollider_Radius()
  //      {

  //          return GetComponent<SphereCollider>().radius;
  //      }

  //      public void SetAIRunning(bool run)
  //      {
  //          this.GetComponent<AI>()._ai_running = run;
  //      }


  //      //____________________________________________
  //      //                  애니메이션  
  //      //____________________________________________

  //      //todo optimization : 애니메이션 찾는 방식 최적화 필요. 해쉬로 변경하기 
  //      public AnimationClip GetClip(string name)
  //      {
  //          AnimationClip animationClip = null;
  //          Single.resourceManager._aniClips.TryGetValue(name.GetHashCode(), out animationClip);


  //          return animationClip;
  //      }


  //      public void Switch_Ani(string aniKind, string aniName , eDirection8 dir)
  //      {
            
  //          _sprRender.flipX = false;
  //          string aniNameSum = "";
  //          switch(dir)
  //          {
                
  //              case eDirection8.leftUp:
  //                  {
  //                      aniNameSum = aniName + eDirection8.rightUp.ToString();
  //                      _sprRender.flipX = true;
  //                  }
  //                  break;
  //              case eDirection8.left:
  //                  {
  //                      aniNameSum = aniName + eDirection8.right.ToString();
  //                      _sprRender.flipX = true;
  //                  }
  //                  break;
  //              case eDirection8.leftDown:
  //                  {
  //                      aniNameSum = aniName + eDirection8.rightDown.ToString();
  //                      _sprRender.flipX = true;
  //                  }
  //                  break;
  //              default:
  //                  {
  //                      aniNameSum = aniName + dir.ToString();
  //                      _sprRender.flipX = false;
  //                  }
  //                  break;
                
  //          }

  //          _overCtr[aniKind] = GetClip(aniNameSum);
  //      }


  //      private float __elapsedTime_1 = 0f;
  //      private float __randTime = 0f;
  //      public void Idle_Random()
  //      {
  //          if ((int)eState.Idle == _animator.GetInteger("state"))
  //          {
  //              __elapsedTime_1 += Time.deltaTime;


  //              if (__randTime < __elapsedTime_1)
  //              {
                    
  //                  //_eDir8 = (eDirection)Single.rand.Next(0, 8); //0~7

  //                  //근접 방향으로 랜덤하게 회전하게 한다 
  //                  int num = Misc.rand.Next(-1, 2); //-1 ~ 1
  //                  num += (int)_eDir8;
  //                  if (0 > num) num = 7;
  //                  if (7 < num) num = 0;
  //                  _eDir8 = (eDirection8)num;

  //                  Switch_Ani("base_idle", _eKind.ToString() + "_idle_", _eDir8);

  //                  __elapsedTime_1 = 0f;

  //                  //3~6초가 지났을 때 돌아감
  //                  __randTime = (float)Misc.rand.Next(3, 7); //3~6
  //              }

  //          }

  //      }

  //      public void Idle_View(Vector3 dir , bool forward )//, bool setState)
  //      {
            
  //          if (false == forward)
  //              dir *= -1f;

  //          _eDir8 = Misc.TransDirection8_AxisY(dir);
  //          //Switch_AniMove("base_move",_eKind.ToString()+"_attack_",_eDir8);
  //          Switch_Ani("base_idle", _eKind.ToString() + "_idle_", _eDir8);

  //          //if(true == setState)
  //              //_animator.SetInteger("state", (int)eState.Idle);
  //      }

  //      public void Move(Vector3 dir ,  float distance ,bool forward )//, bool setState)
		//{
  //          //Vector3 dir = target - this.transform.position;
  //          //dir.Normalize();
  //          _move.Move_Forward(dir, distance, 1f);

  //          //전진이 아니라면 애니를 반대방향으로 바꾼다 (뒷걸음질 효과)
  //          if (false == forward)
  //              dir *= -1f;
            
  //          _eDir8 = Misc.TransDirection8_AxisY(dir);
  //          Switch_Ani("base_move", _eKind.ToString() + "_move_", _eDir8);

		//}

  //      public void Attack(Vector3 dir)
  //      {
  //          _eDir8 = Misc.TransDirection8_AxisY(dir);
  //          Switch_Ani("base_attack",_eKind.ToString()+"_attack_",_eDir8);
  //      }


  //      bool __launch = false;
  //      GameObject __things = null;
  //      Vector3 __targetPos = Vector3.zero;
  //      Vector3 __launchPos = Vector3.zero;
  //      public void ThrowThings(Vector3 target)
  //      {
  //          //Vector3 dir = target - this.transform.position;
  //          //Attack(dir);


  //          if(null == __things)
  //          {
  //              __things = GameObject.Find("000_spear");
  //          }


  //          if (null != __things && false == __launch)
  //          {
  //              __targetPos = target;

  //              Vector3 angle = new Vector3(90f, 0, -120f);
  //              angle.y += (float)_eDir8 * -45f;
  //              __things.transform.localEulerAngles = angle;
  //              __things.transform.localPosition = Vector3.zero;
  //              __launch = true;
  //              __launchPos = __things.transform.position;
  //              __elapsedTime_2 = 0f;
  //              __things.SetActive(true);

  //          }
  //      }
  //      float __elapsedTime_2 = 0f;
  //      public void Update_Shot()
  //      {
  //          if (null != __things && true == __launch)
  //          {
  //              __elapsedTime_2 += Time.deltaTime;
  //              //Vector3 dir = __targetPos - __things.transform.position;
  //              __things.transform.position = Vector3.Lerp(__launchPos, __targetPos, __elapsedTime_2);
                    
  //              if(1f < __elapsedTime_2)
  //              {
  //                  __launch = false;
  //                  __things.SetActive(false);
  //              }
  //          }
  //      }


  //      public void FallDown()
  //      {
  //          switch (_eDir8)
  //          {
  //              case eDirection8.up:
  //                  { }
  //                  break;
  //              case eDirection8.down:
  //                  { }
  //                  break;
  //              default:
  //                  {
  //                      _eDir8 = eDirection8.up; //기본상태 지정 
  //                  }
  //                  break;
  //          }

  //          Switch_Ani("base_fallDown", _eKind.ToString() + "_fallDown_", _eDir8);
  //      }


  //      //____________________________________________
  //      //                  터치 이벤트  
  //      //____________________________________________

		//private Vector3 _startPos = Vector3.zero;
    //    private void TouchBegan() 
    //    {
    //        //DebugWide.LogBlue("TouchBegan " + Single.touchProcess.GetTouchPos());

    //        _animator.speed = 1f;


    //        RaycastHit hit = Single.touchProcess.GetHit3D();
    //        _startPos = hit.point;
    //        _startPos.y = 0f;


    //        Single.lineControl.SetActive(_UIID_circle, true);

    //        //_hp_cur--;
    //        Single.lineControl.SetLineHP(_UIID_hp, (float)_hp_cur/(float)_hp_max);


    //        if(8 > _hp_cur)
    //        {
    //            //다시 살리기
    //            _animator.Play("idle 10");
    //            _death = false;
    //            _hp_cur = 10;
    //            _eState = eState.Idle;    
    //        }


    //    }

    //    private void TouchMoved()
    //    {
    //        //DebugWide.LogBlue("TouchMoved " + Single.touchProcess.GetTouchPos());

    //        RaycastHit hit = Single.touchProcess.GetHit3D();

    //        Vector3 dir = hit.point - this.transform.position;
    //        dir.y = 0;
    //        //DebugWide.LogBlue("TouchMoved " + dir);

    //        //if (eKind.spearman == _eKind)
    //        //{
    //        //    Character target = Single.objectManager.GetNearCharacter(this, 0.5f, 2f);

    //        //    ThrowThings(target.transform.position);

    //        //    Vector3 things_dir = target.transform.position - this.transform.position;

    //        //    _eState = eState.Attack;
    //        //    Attack(things_dir);

    //        //    _move.Move_Forward(dir, 1f, 1f); //chamto test
    //        //    //DebugWide.LogRed(target.name); //chamto test
    //        //}
    //        //else
    //        //{
    //        //    Character target = Single.objectManager.GetNearCharacter(this, 0, 0.2f);
    //        //    if (null != target)
    //        //    {
    //        //        _eState = eState.Attack;
    //        //        Attack(dir);

    //        //        _move.Move_Forward(dir, 1f, 1f); //chamto test
    //        //    }
    //        //    else
    //        //    {
    //        //        _eState = eState.Move;
    //        //        Move(dir, _disPerSecond, true);
    //        //    }
    //        //}
                
    //        //Single.objectManager.LookAtTarget(this);



    //    }

    //    private void TouchEnded() 
    //    {
    //        //DebugWide.LogBlue("TouchEnded " + Single.touchProcess.GetTouchPos());

    //        Switch_Ani("base_idle", _eKind.ToString()+"_idle_", _eDir8);
    //        //_animator.SetInteger("state", (int)eState.Idle);
    //        _animator.Play("idle 10");

    //        //_eState = eState.Idle_Random;
    //        //Single.objectManager.SetAll_Behavior(eState.Idle_Random);

    //        //Single.lineControl.SetActive(_UIID_circle, false);
    //    }
    //}



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

        //private Character _target = null;


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
                        //if(null == _target)
                        //{
                        //    //_target = Single.objectManager.GetNearCharacter(this.GetComponent<Character>(),MIN_DIS, MAX_DIS);
                        //}
                        //else
                        //{
                            
                        //    Vector3 dir = _target.transform.position - this.transform.position;

                        //    if (dir.sqrMagnitude > Mathf.Pow(MAX_DIS, 2))
                        //    {
                        //        _target = null;
                        //    }
                        //    else
                        //    {
                        //        //this.GetComponent<Movable>().Move_Forward(dir, 1f, 1f);
                        //        this.GetComponent<Character>().Move(dir, 1f,true);
                        //        this.GetComponent<Character>()._eState = Character.eState.Move;
                        //    }
                        //}


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

