using UnityEngine;
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


        public Transform _town = null;



        // Use this for initialization
        void Start()
        {
            //타일맵크기 임시지정
            const int TILEMAP_W = 15; //홀수여야 정중앙이 있음 
            const int TILEMAP_H = 15;
            int id_wh = 0;
            //int id_center = (TILEMAP_W * TILEMAP_H -1) / 2; //31x31 그리드의 중간셀 1차원위치값을 구한다

            float cellSize = Single.gridManager.cellSize_x;
            NavGraphNode node = new NavGraphNode(0, Vector3.zero);
            Vector3 pos = Vector3.zero;


            for (int h = 0; h < TILEMAP_H; h++) //세로
            {
                for (int w = 0; w < TILEMAP_W; w++) //가로
                {
                    pos.x = (float)w * cellSize;
                    pos.z = (float)h * cellSize;
                    id_wh = (w + h * TILEMAP_W); //2차원 인덱스를 1차원 인덱스로 변환
                    //id_ij =  id_ij - id_center; //중앙값을 0 이 된다

                    //DebugWide.LogBlue(id_wh);

                    node.SetPos(pos);
                    node.SetIndex( id_wh );
                    _graph.AddNode(node.Clone() as NavGraphNode);

                    //todo fixme : 그래프 노드 수정할점 - chamto 20180917 
                    //그래프노드의 멤버 인덱스변수는 List의 순서인덱스 값임. =>  순서인덱스값과 상관없이 그래프노드의 인덱스변수값은 독립적이어야 한다 
                    //순서인덱스에는 음수값을 넣을 수 없음. => 음수값도 넣을 수 있게 바꿔야 한다

                }
            }


            CellIndex[] grid3x3 =  Single.gridManager._indexesNxN[3];
            int from , to;
            GraphEdge edge = new GraphEdge(0, 1);
            for (int h = 0; h < TILEMAP_H; h++) //세로
            {
                for (int w = 0; w < TILEMAP_W; w++) //가로
                {
                    pos.x = (float)w * cellSize;
                    pos.z = (float)h * cellSize;
                    from = (w + h * TILEMAP_W);

                    foreach(CellIndex cidx in grid3x3)
                    {
                        to = cidx.n1 + cidx.n2 * TILEMAP_W;

                        if (false == _graph.isNodePresent(to)) continue;

                        edge.SetFrom(from);
                        edge.SetTo(to);
                        _graph.AddEdge(edge.Clone() as GraphEdge);    
                    }

                }
            }

        }


        void Update()
        {


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
        public int SearchNonAlloc(Vector3 srcPos, Vector3 destPos, ref Stack<Vector3> pathPos)
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
                pathPos.Push(destPos);

                return 1;
            }

            _searchDFS.Init(_graph, srcNode.Index(), destNode.Index());
            List<int> pathList = _searchDFS.GetPathToTarget();

            //-------- chamto test --------
            //      string nodeChaine = "nodeChaine : ";
            //      foreach (int node in pathList) 
            //      {
            //          nodeChaine += node + "<-";
            //      }
            //      Debug.Log (nodeChaine); 
            //-------- ------------ --------


            pathPos.Clear();
            pathPos.Push(destPos);
            foreach (int node in pathList)
            {
                tempNode = _graph.GetNode(node) as NavGraphNode;
                pathPos.Push(tempNode.Pos());
            }
            //pathPos.Push (srcPos);

            return pathPos.Count;
        }

        public Stack<Vector3> Search(Vector3 srcPos, Vector3 destPos)
        {
            NavGraphNode destNode = _graph.FindNearNode(destPos);
            NavGraphNode srcNode = _graph.FindNearNode(srcPos);
            NavGraphNode tempNode = null;
            Stack<Vector3> pathPos = new Stack<Vector3>();

            if (true == this.Possible_LinearMove(srcPos, destPos, 0))
            {
                pathPos.Push(destPos);
                return pathPos;
            }

            _searchDFS.Init(_graph, srcNode.Index(), destNode.Index());
            List<int> pathList = _searchDFS.GetPathToTarget();

            //-------- chamto test --------
            string nodeChaine = "nodeChaine : ";
            foreach (int node in pathList)
            {
                nodeChaine += node + "<-";
            }
            Debug.Log(nodeChaine);
            //-------- ------------ --------


            pathPos.Push(destPos);
            foreach (int node in pathList)
            {
                tempNode = _graph.GetNode(node) as NavGraphNode;
                pathPos.Push(tempNode.Pos());
            }
            //pathPos.Push (srcPos);

            return pathPos;

        }



    }
}



