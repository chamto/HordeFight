using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class MomoryPoolTest : MonoBehaviour 
{
    MemoryPool<TData> _memoryPool = new MemoryPool<TData>();
    List<TData> _datas = new List<TData>();

	void Start () 
    {
        _memoryPool.Init(100, 20, "Test_Pool");

        for(int i=0;i<20;i++)
        {
            TData data = _memoryPool.Alloc();
            data.length = i;
            _datas.Add(data);
        }
        DebugWide.LogBlue(_memoryPool.ToString());


        for (int i = 0; i < 10; i++)
        {
            _datas[i].Reset();
            _memoryPool.Free(_datas[i]);

        }
        DebugWide.LogBlue(_memoryPool.ToString());

        for (int i = 0; i < 100; i++)
        {
            TData data = _memoryPool.Alloc();
            data.length = i;
            _datas.Add(data);
        }
        DebugWide.LogBlue(_memoryPool.ToString());
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

public class TData
{
    public int length = -1; 

    public void Reset()
    {
        length = -1; 
    }
}
