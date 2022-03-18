using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class VectorAverage : MonoBehaviour 
{
    int COUNT = 30;
    Vector3[] _poslist = null;

	// Use this for initialization
	void Start () 
    {

        _poslist = new Vector3[COUNT];
        for(int i=0;i<COUNT;i++)
        {
            //float x = Misc.RandFloat(-2f, 2f);
            //float z = Misc.RandFloat(-2f, 2f);

            //float len = Misc.RandFloat(2f, 2.5f);
            float len = 1;
            float angle = Misc.RandFloat(0, 300f);
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * len;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * len;

            _poslist[i] = new Vector3(x,0,z); 
        }
    }
	
	// Update is called once per frame
	void Update () 
    {

    }

    private void OnDrawGizmos()
    {
        if (null == _poslist) return;

        Vector3 sum = Vector3.zero , average = Vector3.zero;
        for (int i = 0; i < COUNT; i++)
        {
            DebugWide.DrawCircle(_poslist[i], 0.2f, Color.white);

            sum += _poslist[i];
        }

        average = sum / COUNT;

        //삼각형의 무게중심 :  https://jwmath.tistory.com/489
        //https://blog.naver.com/PostView.nhn?isHttpsRedirect=true&blogId=junhyuk7272&logNo=221420965314&parentCategoryNo=&categoryNo=84&viewDate=&isShowPopularPosts=true&from=search
        //원점에서 시작하는 벡터들의 합의 평균 => 무게중심 좌표 
        //벡터평균의 의미에 대해 분석 ,  벡터들의 합 : 평균방향 , 벡터평균 : 무게중심좌표이며 평균방향도 나타낸다 
        DebugWide.DrawCircle(average, 0.2f, Color.red);
        DebugWide.DrawCircle(sum, 0.3f, Color.blue);
        DebugWide.DrawLine(average, sum, Color.blue);
    }
}
