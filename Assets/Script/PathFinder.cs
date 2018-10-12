using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
//using System.String;


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



        // Use this for initialization
        void Start()
        {
            LoadNodes_Tilemap();
            LoadEdges_Tilemap();

            DebugWide.LogBlue("!!! Loaded Nodes,Edges !!!");
        }


        //void Update()
        void FixedUpdate()
        {


        }

        public void LoadNodes_Tilemap()
        {
            Tilemap tilemap = SingleO.gridManager.GetTileMap_Struct();

            if (null == tilemap) return;

            int id_wh = 0;
            NavGraphNode node = new NavGraphNode(0, Vector3.zero);
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tilemap.size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int pos2d in boundsInt.allPositionsWithin)
            {
                //chamto test
                //DebugWide.LogBlue(pos2d + "   " + SingleO.gridManager.ToPosition1D(pos2d));
                //id_wh++;
                //if (id_wh == 30) return;

                TileBase baseTile = tilemap.GetTile(pos2d); 
                //if (null == baseTile) continue; //모든 노드가 순서대로 들어가야 한다. 타일이 없다고 건너뛰면 안된다

                id_wh = SingleO.gridManager.ToPosition1D(pos2d);

                node.SetPos(pos2d);
                node.SetIndex(id_wh); //id 순서값과 루프문 순서와 일치해야 한다. 내부에서 리스트에 들어가는 순서값을 1차원 위치값으로 사용하기 때문임 
                _graph.AddNode(node.Clone() as NavGraphNode);

            }

        }

        public void LoadEdges_Tilemap()
        {
            Tilemap tilemap = SingleO.gridManager.GetTileMap_Struct();

            if (null == tilemap) return;

            int from = 0, to = 0;
            Vector3Int toPos = Vector3Int.zero;
            GraphEdge edge = new GraphEdge(0, 1);
            Vector3Int[] grid3x3 = SingleO.gridManager._indexesNxN[3];
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tilemap.size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int pos2d in boundsInt.allPositionsWithin)
            {
                TileBase baseTile = tilemap.GetTile(pos2d);
                if (null == baseTile) continue;

                from = SingleO.gridManager.ToPosition1D(pos2d);

                foreach (Vector3Int cidx in grid3x3)
                {
                    //미리구한 그리드범위에 중심위치값 더하기
                    toPos = pos2d + cidx;

                    //설정된 노드 범위를 벗어나는 노드값은 추가하면 안된다 
                    if (toPos.x < 0 || tilemap.size.x <= toPos.x) continue;
                    if (toPos.y < 0 || tilemap.size.y <= toPos.y) continue;


                    //1차원으로 변환
                    to = SingleO.gridManager.ToPosition1D(toPos);

                    if (false == _graph.isNodePresent(to)) continue;

                    //자기자신을 연결하는 엣지는 추가하면 안된다 
                    if (from == to) continue;

                    //DebugWide.LogBlue(from + "  " + to); //chamto test
                    edge.SetFrom(from);
                    edge.SetTo(to);
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
            if (true == this.Possible_LinearMove(srcPos, destPos, 0))
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

            if (true == this.Possible_LinearMove(srcPos, destPos, 0))
            {
                pathPos.Enqueue(destPos);
                return pathPos;
            }

            _searchDFS.Init(_graph, srcNode.Index(), destNode.Index());
            _searchAStar.Init(_graph, srcNode.Index(), destNode.Index());
            //LinkedList<int> pathList = _searchDFS.GetPathToTarget();
            LinkedList<int> pathList = _searchAStar.GetPathToTarget();

            //-------- chamto test --------
            string nodeChaine = "nodeChaine : ";
            foreach (int node in pathList)
            {
                nodeChaine += node + "->";
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



