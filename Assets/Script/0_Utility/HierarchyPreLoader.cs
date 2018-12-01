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

    public Transform GetTransform(string full_path)
    {
        int hash_key = full_path.GetHashCode();
        Transform getTr = GetTransform(hash_key);

        //해쉬맵에는 없고 계층도에는 있는 경우 , 경로 추가처리 한다 
        getTr = this.Find<Transform>(full_path);
        if(null != getTr)
        {
            this.Add(getTr);
        }

        return getTr;
    }

    public GameObject GetGameObject(string full_path)
    {
        Transform tr = this.GetTransform(full_path);
        if (null != tr)
        {
            return tr.gameObject;
        }

        return null;
    }


    public TaaT GetTypeObject<TaaT>(string fullPath) where TaaT : class
    {
        Transform f = this.GetTransform(fullPath);
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
			DebugWide.LogRed (full_path + "  이미 탐색한 경로입니다");
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
		Transform f = Resources.FindObjectsOfTypeAll<Transform>().Where(tr => this.GetFullPath (tr) == fullPath).First();

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
		Transform f =  Resources.FindObjectsOfTypeAll<Transform> ().Where (
			tr => this.GetFullPath (tr) == fullPath && tr.hideFlags != HideFlags.HideInHierarchy).First ();

		//GameObject 를 컴포넌트로 검색하면 에러남. GameObject 는 컴포넌트가 아니다
		if (typeof(TaaT) == typeof(GameObject)) 
		{
			return f.gameObject as TaaT;
		}

		return f.GetComponentInChildren <TaaT>(true);
	}
}

