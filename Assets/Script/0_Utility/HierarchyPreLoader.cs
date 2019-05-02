using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HierarchyPreLoader
{
	private bool _isInit = false;

	private Dictionary<int,Transform> _hashMap = new Dictionary<int, Transform> ();


    public void Init()
    {
        _hashMap.Clear();
        _isInit = true;

        //여러 최상위 루트마다 순회
        foreach (Transform oneOfManyRoots in UnityEngine.Object.FindObjectsOfType<Transform>())
        {
            if (oneOfManyRoots.parent == null)
            {

                //DebugWide.LogRed(oneOfManyRoots.name);
                this.PreOrderTraversal(oneOfManyRoots);
            }
        }

        //TestPrint(); //chamto test
    }


    //해쉬값으로 tr값을 얻는다
    public Transform GetTransform(int hash_key)
    {
        Transform getTr = null;
        if (true == _hashMap.TryGetValue(hash_key, out getTr))
        {
            return this._hashMap[hash_key];    
        }

        return null;
    }

    public Transform GetTransformA(string full_path)
    {
        int hash_key = full_path.GetHashCode();
        Transform getTr = GetTransform(hash_key); //1. 해쉬 목록 검사 
        if(null == getTr)
        {
            //2. 실제로 존재하는지 검사, 있으면 추가 
            getTr = this.Find<Transform>(full_path); 
            if (null != getTr)
            {
                //해쉬맵에는 없고 계층도에는 있는 경우 , 경로 추가처리 한다
                this.Add(getTr);
            }
        }

        return getTr;
    }

    // child_path 경로가 " " , "/" 인 경우 parent 를 그대로 반환한다 
    public Transform GetTransformA(Transform parent, string child_path)
    {
        int length = child_path.Length;
        if (0 == length) return parent;
        if ('/' == child_path[0]) 
        {
            if (1 == length) return parent;
        }
        else
        {
            child_path = "/" + child_path; // 경로구분자가 없으면 추가해준다 
        }
            

        string full_path =  HierarchyPreLoader.FullPath(parent) + child_path;
        //DebugWide.LogBlue(full_path); //chamto test

        return GetTransformA(full_path);

    }

    public GameObject GetGameObject(string full_path)
    {
        Transform tr = this.GetTransformA(full_path);
        if (null != tr)
        {
            return tr.gameObject;
        }

        return null;
    }


    public TaaT GetTypeObject<TaaT>(string fullPath) where TaaT : class
    {
        Transform f = this.GetTransformA(fullPath);
        if (null == f)
            return null;

        //GameObject 를 컴포넌트로 검색하면 에러남. GameObject 는 컴포넌트가 아니다
        if (typeof(TaaT) == typeof(GameObject))
        {
            return f.gameObject as TaaT;
        }

        return f.GetComponentInChildren<TaaT>(true);
    }

	//ref : http://answers.unity3d.com/questions/8500/how-can-i-get-the-full-path-to-a-gameobject.html
	static public string FullPath(Transform tr)
	{
		string path = tr.name;
		while (tr.parent != null)
		{
			tr = tr.parent;
			path = tr.name + "/" + path;
		}
		return path;

		//  aaa/aaa/~
	}

	public string GetFullPath(Transform tr)
	{
		return HierarchyPreLoader.FullPath (tr);
	}

	public void Add(Transform tr)
	{
		//tr값에서 경로정보를 얻는다. 얻은 경로정보를 해쉬값으로 변경한다 
		int hash_key = HierarchyPreLoader.FullPath (tr).GetHashCode ();

		Transform getTr = null;
		if (true == _hashMap.TryGetValue (hash_key, out getTr)) 
		{
			//해쉬값이 존재할 경우 새로운 값으로 변경한다
			this._hashMap[hash_key] = tr;
			return;
		}

		this._hashMap.Add(hash_key, tr);
	}

	
	public void PreOrderTraversal(Transform data)
	{
		if (false == _isInit) 
		{
			DebugWide.LogError ("Func : PreOrderTraversal : Initial function not called");
			return;
		}


		string full_path = HierarchyPreLoader.FullPath (data);
		int hash_key = full_path.GetHashCode ();


		//1. visit
		//DebugWide.LogRed (path +"    "+ data.name); //chamto test
		Transform getTr;
		if (true == _hashMap.TryGetValue (hash_key, out getTr)) 
		{
			
			//이미 있는 경로일 경우 자식쪽은 탐색을 중지 한다
            DebugWide.LogRed (full_path + "  이미 탐색한 경로입니다  " +  hash_key);
			return;
		}

		//2. 값등록
		this._hashMap.Add(hash_key, data);

		
		//3. traversal
		Transform[] tfoList = data.GetComponentsInChildren<Transform> (true);
		foreach(Transform child in tfoList)
		{
			if(child != data && child.parent == data) 
			{
				this.PreOrderTraversal(child);
			}
		}
	}


	public void TestPrint()
	{
		

		Debug.Log ("---------- HierarchyLoader : TestPrint ----------");
		foreach(KeyValuePair<int, Transform> keyValue in _hashMap)
		{
			Debug.Log("<color=blue>" + keyValue.Key + " : </color> <color=green>" + this.GetFullPath( keyValue.Value) + "</color> \n");
		}
	}



	//=================================================
	//        유니티 함수를 다시 렙핑한 것임 . 속도 장점없음
	//=================================================
    private TaaT Find<TaaT>(string fullPath) where TaaT : class
	{
        //ref : http://answers.unity3d.com/questions/8500/how-can-i-get-the-full-path-to-a-gameobject.html
        //Transform f = Resources.FindObjectsOfTypeAll<Transform>().Where(tr => this.GetFullPath (tr) == fullPath).First();

        //조건에 맞는 경로가 없을 경우의 예외처리 추가
        Transform f = null;
        IEnumerable<Transform> ee  = Resources.FindObjectsOfTypeAll<Transform>().Where(tr => this.GetFullPath(tr) == fullPath);
        if (0 != ee.Count<Transform>())
            f = ee.First();
        else
            return null;

		//return f.GetComponentInChildren (typeof(TaaT), true) as TaaT;
		//DebugWide.LogBlue(f.name); //chamto test

		//GameObject 를 컴포넌트로 검색하면 에러남. GameObject 는 컴포넌트가 아니다
		if (typeof(TaaT) == typeof(GameObject)) 
		{
			return f.gameObject as TaaT;
		}

		return f.GetComponentInChildren <TaaT>(true);
	}

	//=================================================
	//        유니티 함수를 다시 렙핑한 것임 . 속도 장없음
	//=================================================
    private TaaT FindOnlyActive<TaaT>(string fullPath) where TaaT : class
	{
		//ref : http://answers.unity3d.com/questions/8500/how-can-i-get-the-full-path-to-a-gameobject.html
		//Transform f =  Resources.FindObjectsOfTypeAll<Transform> ().Where (
			//tr => this.GetFullPath (tr) == fullPath && tr.hideFlags != HideFlags.HideInHierarchy).First ();

        //조건에 맞는 경로가 없을 경우의 예외처리 추가
        Transform f = null;
        IEnumerable<Transform> ee = Resources.FindObjectsOfTypeAll<Transform>().Where(
            tr => this.GetFullPath(tr) == fullPath && tr.hideFlags != HideFlags.HideInHierarchy);
        if (0 != ee.Count<Transform>())
            f = ee.First();
        else
            return null;

		//GameObject 를 컴포넌트로 검색하면 에러남. GameObject 는 컴포넌트가 아니다
		if (typeof(TaaT) == typeof(GameObject)) 
		{
			return f.gameObject as TaaT;
		}

		return f.GetComponentInChildren <TaaT>(true);
	}
}

