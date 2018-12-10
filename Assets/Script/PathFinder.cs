using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

using UtilGS9;


//========================================================
//==================      길찾기      ==================
//========================================================
namespace HordeFight
{
    public class PathFinder : MonoBehaviour
    {
        //*
        public SparseGraph _graph = new SparseGraph(false);
        public Graph_SearchDFS _searchDFS = new Graph_SearchDFS();
        public Graph_SearchAStar _searchAStar = new Graph_SearchAStar();

        public Transform _town = null;

        const int TILE_BLOCK_WIDTH = 38;
        const int TILE_BLOCK_HEIGHT = 24;
        //const int TILE_BLOCK_WIDTH = 15;
        //const int TILE_BLOCK_HEIGHT = 15;
        public Vector3Int TILE_BLOCK_SIZE =  new Vector3Int(TILE_BLOCK_WIDTH, TILE_BLOCK_HEIGHT, 1);

        // Use this for initialization
        void Start()
        {
            LoadNodes(TILE_BLOCK_SIZE);
            LoadEdges(TILE_BLOCK_SIZE);

            DebugWide.LogBlue("!!! Loaded Nodes,Edges !!!");


        }

      
        void Update()
        {
            

        }

        public void LoadNodes(Vector3Int tileBlock_size)
        {
            //if (null == tilemap) return;
            //int count = 0;
            int id_wh = 0;
            NavGraphNode node = new NavGraphNode(0, Vector3.zero);
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tileBlock_size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int posXY_2d in boundsInt.allPositionsWithin)
            {
                //chamto test
                //DebugWide.LogBlue(count++ + "   " + pos2d + "   " + SingleO.gridManager.ToPosition1D(pos2d , boundsInt.size.x) + "    " + SingleO.gridManager.ToPosition3D_Center(pos2d, Vector3.up));
                //id_wh++;
                //if (id_wh == 30) return;

                //TileBase baseTile = tilemap.GetTile(pos2d); 
                //if (null == baseTile) continue; //모든 노드가 순서대로 들어가야 한다. 타일이 없다고 건너뛰면 안된다

                id_wh = SingleO.gridManager.ToPosition1D(posXY_2d , boundsInt.size.x);

                node.SetPos(SingleO.gridManager.ToPosition3D_Center(posXY_2d));
                node.SetIndex(id_wh); //id 순서값과 루프문 순서와 일치해야 한다. 내부에서 리스트에 들어가는 순서값을 1차원 위치값으로 사용하기 때문임 
                _graph.AddNode(node.Clone() as NavGraphNode);

            }

        }

        public void LoadEdges(Vector3Int tileBlock_size)
        {
            Tilemap tilemap = SingleO.gridManager.GetTileMap_Struct();

            //if (null == tilemap) return;

            int from_1d = 0, to_1d = 0;
            Vector3Int to_2d = Vector3Int.zero;
            GraphEdge edge = new GraphEdge(0, 1);
            Vector3Int[] grid3x3 = SingleO.gridManager._indexesNxN[3];
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tileBlock_size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int from_2d in boundsInt.allPositionsWithin)
            {
                //구조물이 있는 셀에는 엣지를 연결하지 않는다 - 임시처리 : 엣지를 어떻게 연결할지 좀더 생각해 봐야 함
                if (true == SingleO.gridManager.HasStructTile_InPostion2D(from_2d)) continue;


                from_1d = SingleO.gridManager.ToPosition1D(from_2d , boundsInt.size.x);

                foreach (Vector3Int dst_2d in grid3x3)
                {
                    //미리구한 그리드범위에 중심위치값 더하기
                    to_2d = from_2d + dst_2d;

                    if (true == SingleO.gridManager.HasStructTile_InPostion2D(to_2d)) continue;

                    //설정된 노드 범위를 벗어나는 노드값은 추가하면 안된다 
                    if (to_2d.x < 0 || TILE_BLOCK_WIDTH <= to_2d.x) continue;
                    if (to_2d.y < 0 || TILE_BLOCK_HEIGHT <= to_2d.y) continue;


                    //1차원으로 변환
                    to_1d = SingleO.gridManager.ToPosition1D(to_2d , boundsInt.size.x);

                    if (false == _graph.isNodePresent(to_1d)) continue;

                    //자기자신을 연결하는 엣지는 추가하면 안된다 
                    if (from_1d == to_1d) continue;

                    //DebugWide.LogBlue(from + "  " + to); //chamto test
                    edge.SetFrom(from_1d);
                    edge.SetTo(to_1d);
                    edge.SetCost(1.0f);
                    _graph.AddEdge(edge.Clone() as GraphEdge);
                }   
            }

        }

        //Physics.RaycastNonAlloc 함수 테스트 해보기. 결과값을 저장할 객체를 외부에서 생성하는 방식 
        public bool Possible_LinearMove(Vector3 srcPos, Vector3 destPos, int layerMask)
        {
            return Physics.Linecast(srcPos, destPos, layerMask);
        }

        public bool Possible_LinearMove_TileMap(Vector3 srcPos, Vector3 destPos)
        {
            LineSegment3 lineSeg = LineSegment3.zero;
            lineSeg.origin = srcPos;
            lineSeg.last = destPos;

            float CELL_HARF_SIZE = SingleO.gridManager._cellSize_x * 0.5f;
            float CELL_SQUARED_RADIUS = Mathf.Pow(CELL_HARF_SIZE, 2f);
            float sqrDis = 0f;
            float t_c = 0;

            //기준셀값을 더해준다. 기준셀은 그리드값 변환된 값이이어야 한다 
            Vector3Int originToGrid_XY_2d = SingleO.gridManager.ToPosition2D(srcPos);
            Vector3 originToPos_3d = SingleO.gridManager.ToPosition3D(originToGrid_XY_2d);
            Vector3 worldCellCenterPos = Vector3.zero;
            foreach (Vector3Int cell_posXY_2d in SingleO.gridManager._indexesNxN[7])
            {
                //셀의 중심좌표로 변환 
                worldCellCenterPos = SingleO.gridManager.ToPosition3D_Center(cell_posXY_2d);
                worldCellCenterPos += originToPos_3d;


                //시작위치셀을 포함하거나 뺄때가 있다. 사용하지 않느다 
                //선분방향과 반대방향인 셀들을 걸러낸다 , (0,0)원점 즉 출발점의 셀은 제외한다 
                if (0 == cell_posXY_2d.sqrMagnitude || 0 >= Vector3.Dot(lineSeg.direction, worldCellCenterPos - srcPos))
                {
                    continue;
                }

                sqrDis = lineSeg.MinimumDistanceSquared(worldCellCenterPos, out t_c);

                //선분에 멀리있는 셀들을 걸러낸다
                if (CELL_SQUARED_RADIUS < sqrDis)
                {
                    continue;
                }

                if (true == SingleO.gridManager.HasStructTile(worldCellCenterPos))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Searchs the non alloc.
        /// </summary>
        /// <returns> pathPos.Count.</returns>
        public int SearchNonAlloc(Vector3 srcPos, Vector3 destPos, ref Queue<Vector3> pathPos)
        {
            NavGraphNode destNode = _graph.FindNearNode(destPos);
            NavGraphNode srcNode = _graph.FindNearNode(srcPos);
            NavGraphNode tempNode = null;

            if (null == pathPos)
                return 0;

            //직선이동이 가능하면 길찾기 안하고 종료한다
            //if (true == this.Possible_LinearMove(srcPos, destPos, 0))
            if (true == this.Possible_LinearMove_TileMap(srcPos, destPos))
            {
                pathPos.Clear();
                pathPos.Enqueue(destPos);

                return 1;
            }

            _searchDFS.Init(_graph, srcNode.Index(), destNode.Index());
            LinkedList<int> pathList = _searchDFS.GetPathToTarget();

            //-------- chamto test --------
            //      string nodeChaine = "nodeChaine : ";
            //      foreach (int node in pathList) 
            //      {
            //          nodeChaine += node + "->";
            //      }
            //      Debug.Log (nodeChaine); 
            //-------- ------------ --------


            pathPos.Clear();
            foreach (int node in pathList)
            {
                tempNode = _graph.GetNode(node) as NavGraphNode;
                pathPos.Enqueue(tempNode.Pos());
            }
            pathPos.Enqueue(destPos);

            return pathPos.Count;
        }

        public Queue<Vector3> Search(Vector3 srcPos, Vector3 destPos)
        {
            NavGraphNode destNode = _graph.FindNearNode(destPos);
            NavGraphNode srcNode = _graph.FindNearNode(srcPos);
            NavGraphNode tempNode = null;
            Queue<Vector3> pathPos = new Queue<Vector3>();

            //if (true == this.Possible_LinearMove(srcPos, destPos, 0))
            if (true == this.Possible_LinearMove_TileMap(srcPos, destPos))
            {
                //DebugWide.LogBlue("Possible_LinearMove_TileMap !!! ");

                pathPos.Enqueue(destPos);
                return pathPos;
            }

            //DebugWide.LogBlue(srcNode + "   " + destNode); //chamto test

            _searchDFS.Init(_graph, srcNode.Index(), destNode.Index());
            _searchAStar.Init(_graph, srcNode.Index(), destNode.Index());
            //LinkedList<int> pathList = _searchDFS.GetPathToTarget();
            LinkedList<int> pathList = _searchAStar.GetPathToTarget();

            //-------- chamto test --------
            string nodeChaine = "nodeChaine : ";
            Vector3Int pos2d = Vector3Int.zero;
            foreach (int node in pathList)
            {
                pos2d = SingleO.gridManager.ToPosition2D(node , TILE_BLOCK_WIDTH);
                nodeChaine += pos2d + "->";
            }
            Debug.Log(nodeChaine);
            //-------- ------------ --------

            foreach (int node in pathList)
            {
                tempNode = _graph.GetNode(node) as NavGraphNode;
                pathPos.Enqueue(tempNode.Pos());
            }
            pathPos.Enqueue(destPos);

            return pathPos;

        }



    }
}



