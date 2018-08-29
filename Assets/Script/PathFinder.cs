using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
//using System.String;


public class PathFinder : MonoBehaviour 
{
    /*
	public SparseGraph 	_graph = new SparseGraph (true);
	public Graph_SearchDFS _searchDFS = new Graph_SearchDFS();

	
	public Transform _town = null;

	public bool _saveXML = false;
	public bool _loadXML = false;
	public bool _updateNode = false;

	// Use this for initialization
	void Start () 
	{

		//게임씬으로 시작시 리소스로딩이 안되어 있기 때문에, 
		//   작업편의성을 위해 직접 동기로 불러오는 처리를 넣는다.
		if (false == Single.resource.IsCompleteLoad ()) 
		{
			Single.resource.Load_Sync();
		}


		Table.File_NodeInfo table = Single.resource._nodeInfo;
		foreach (Table.NodeInfo nodeFrom in table._data) 
		{
			_graph.AddNode (new NavGraphNode (nodeFrom.nodeNum, _town.position + nodeFrom.nodePos));
		}

		foreach (Table.NodeInfo nodeFrom in table._data) 
		{
			foreach(int edgeTo in nodeFrom.edgeList)
			{
				_graph.AddEdge (new GraphEdge (nodeFrom.nodeNum, edgeTo));
			}
		}

		//Debug.DrawLine(Vector3.zero, new Vector3(1, 1, 0), Color.red);
		//Debug.Assert (false, "sdfsdfsdfsdf assert");

	}
	

	void Update () 
	{

		if (true == _saveXML) 
		{
			Table.File_NodeInfo table = Single.resource._nodeInfo;
			List<Table.NodeInfo> saveList = new List<Table.NodeInfo>();
			NodeInfo_MonoBehaviour[] monoList =  _town.GetComponentsInChildren <NodeInfo_MonoBehaviour>(false);
			foreach (NodeInfo_MonoBehaviour mono in monoList) 
			{
				saveList.Add(new Table.NodeInfo(mono._nodeNumber, mono.transform.localPosition, mono._adjacencyEdgeList));
			}
			table._data = saveList;
			table.SaveXML ("Assets/StreamingAssets/"+"townNode.xml", table._data);

			//---------------
			_saveXML = false;
		}

		if (true == _loadXML) 
		{
			Table.File_NodeInfo table = Single.resource._nodeInfo;
			NodeInfo_MonoBehaviour[] monoList =  _town.GetComponentsInChildren <NodeInfo_MonoBehaviour>(false);
			foreach(NodeInfo_MonoBehaviour mono in monoList)
			{
				GameObject.Destroy(mono.gameObject);
			}
			foreach(Table.NodeInfo info in table._data)
			{
				this.AddNodePrefab(info);
			}


			//---------------
			_loadXML = false;

		}

		if (true == _updateNode) 
		{

			NodeInfo_MonoBehaviour[] monoList =  _town.GetComponentsInChildren <NodeInfo_MonoBehaviour>(false);
			foreach(NodeInfo_MonoBehaviour mono in monoList)
			{
				//mono.UpdateEdgeList();
				mono._isUpdateValue = true;
			}
			_updateNode = false;
		}
	}

//	public void fix_LoadGraphNode()
//	{
//		NodeInfo_MonoBehaviour[] monoList =  _town.GetComponentsInChildren <NodeInfo_MonoBehaviour>(true);
//		foreach (NodeInfo_MonoBehaviour mono in monoList) 
//		{
//			Debug.Log("<color=red>Load Number:</color>" + mono._nodeNumber);
//		}
//
//	}

	/// <summary>
	/// Possibles the linear move.
	/// </summary>
	/// <returns><c>true</c>, if linear move was possibled, <c>false</c> otherwise.</returns>
	/// <param name="srcPos">Source position.</param>
	/// <param name="destPos">Destination position.</param>
	/// <param name="layerMask">Layer mask.</param>
	public bool PossibleLinearMove(Vector3 srcPos, Vector3 destPos, int layerMask)
	{
		if (0 == Physics2D.LinecastNonAlloc (srcPos, destPos, _preGenerated_raycasthit_PossibleLinearMove, layerMask)) 
		{
			return true;
		}
		
		return false;
	}
	// ---  PossibleLinearMove 함수전용 멤버변수 : 메모리재할당을 피하기 위해 미리생성된것을 사용하도록 한다.
	private RaycastHit2D[] _preGenerated_raycasthit_PossibleLinearMove = new RaycastHit2D[1];
	/// </summary>



	public int SearchNonAlloc(Vector3 srcPos, Vector3 destPos, ref Stack<Vector3> pathPos)
	{
		NavGraphNode destNode = _graph.FindNearNode (destPos);
		NavGraphNode srcNode = _graph.FindNearNode (srcPos);
		NavGraphNode tempNode = null;

		if (null == pathPos)
			return 0;

		if (true == this.PossibleLinearMove (srcPos, destPos, GlobalConstants.Layer.Mask.building)) 
		{
			if(null != pathPos) 
			{
				pathPos.Clear();
				pathPos.Push(destPos);
			}
			return 1;
		}
		
		_searchDFS.Init (_graph, srcNode.Index(), destNode.Index());
		List<int> pathList = _searchDFS.GetPathToTarget ();
		
		//-------- chamto test --------
//		string nodeChaine = "nodeChaine : ";
//		foreach (int node in pathList) 
//		{
//			nodeChaine += node + "<-";
//		}
//		Debug.Log (nodeChaine); 
		//-------- ------------ --------
		

		pathPos.Clear ();
		pathPos.Push (destPos);
		foreach(int node in pathList) 
		{
			tempNode = _graph.GetNode(node) as NavGraphNode;
			pathPos.Push(tempNode.Pos());
		}
		//pathPos.Push (srcPos);
		
		return pathPos.Count;
	}

	public Stack<Vector3> Search(Vector3 srcPos, Vector3 destPos)
	{
		NavGraphNode destNode = _graph.FindNearNode (destPos);
		NavGraphNode srcNode = _graph.FindNearNode (srcPos);
		NavGraphNode tempNode = null;
		Stack<Vector3> pathPos = new Stack<Vector3>();

		if (true == this.PossibleLinearMove (srcPos, destPos, GlobalConstants.Layer.Mask.building)) 
		{
			pathPos.Push(destPos);
			return pathPos;
		}

		_searchDFS.Init (_graph, srcNode.Index(), destNode.Index());
		List<int> pathList = _searchDFS.GetPathToTarget ();

		//-------- chamto test --------
		string nodeChaine = "nodeChaine : ";
		foreach (int node in pathList) 
		{
			nodeChaine += node + "<-";
		}
		Debug.Log (nodeChaine); 
		//-------- ------------ --------


		pathPos.Push (destPos);
		foreach(int node in pathList) 
		{
			tempNode = _graph.GetNode(node) as NavGraphNode;
			pathPos.Push(tempNode.Pos());
		}
		//pathPos.Push (srcPos);

		return pathPos;

	}


	public void AddNodePrefab(Table.NodeInfo info)
	{
		GameObject obj = this.CreatePrefab ("node (-1)");
		obj.transform.parent = _town;
		//obj.transform.position = info.nodePos;
		obj.transform.localPosition = info.nodePos;

		NodeInfo_MonoBehaviour mono = obj.GetComponent<NodeInfo_MonoBehaviour> ();
		mono._nodeNumber = info.nodeNum;
		mono._adjacencyEdgeList = info.edgeList;

		mono._isUpdateValue = true;

	}

	public GameObject CreatePrefab(string path)
	{
		const string root = "Prefab/";
		return MonoBehaviour.Instantiate(Resources.Load(root + path)) as GameObject;
	}

    //*/
}

