using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Parabola : MonoBehaviour
{

    public Transform _start = null;
    public Transform _end = null;

	private void Start()
	{
        if (null == _start || null == _end) return;
        StartPos = _start.position;
        DestPos = _end.position;

        PreCalculate();
	}

	private void Update()
	{
        if (null == _start || null == _end) return;
        transform.Translate(Move());
	}

    //public void Create_Path(Transform parent, Vector3 start, Vector3 end)
    //{
    //    GameObject obj = new GameObject();
    //    LineRenderer render = obj.AddComponent<LineRenderer>();

    //    render.name = "Parabola_Path";
    //    render.material = new Material(Shader.Find("Sprites/Default"));
    //    render.useWorldSpace = true;
    //    render.transform.parent = parent;//부모객체 지정


    //    render.positionCount = 20;
    //    render.transform.localPosition = Vector3.zero;
    //    render.startWidth = 0.01f;
    //    render.endWidth = 0.01f;


    //    float deg = 360f / render.positionCount;

    //    Vector3 pos = Vector3.right;
    //    for (int i = 0; i < render.positionCount; i++)
    //    {
    //        pos.x = Mathf.Cos(deg * i * Mathf.Deg2Rad) * radius;
    //        pos.z = Mathf.Sin(deg * i * Mathf.Deg2Rad) * radius;

    //        render.SetPosition(i, pos);
    //        //DebugWide.LogBlue(Mathf.Cos(deg * i * Mathf.Deg2Rad) + " _ " + deg*i);
    //    }

    //}


    //ref : https://www.slideshare.net/semin204/ss-16331290

    Vector3 DestPos; //도착점
    Vector3 StartPos; //시작점

    float vx; //x축 방향의 속도
    float vy; //y축 방향의 속도
    float g; //y축 방향의 중력가속도

    float dat; //도착점 도달 시간
    float mh; //최고점 - 시작점(높이) 
    float dh; //도착점 높이
    float t = 0f; //진행시간
    float MAX_Y = 70f; //최고점 높이 
    float mht = 1.6f; //최고점 도달 시간

    void PreCalculate()
    {
        dh = DestPos.y - StartPos.y;
        mh = MAX_Y - StartPos.y;

        g = 2 * mh / (mht * mht);

        vy = Mathf.Sqrt(2 * g * mh);

        float a = g;
        float b = -2 * vy;
        float c = 2 * dh;

        dat = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        vx = -(StartPos.x - DestPos.x) / dat;

    }

    Vector3 Move()
    {
        t += Time.deltaTime;

        if (t > dat) return Vector3.zero;

        float x = StartPos.x + vx * t;
        float y = StartPos.y + vy * t - 0.5f * g * t * t;

        return new Vector3(x, y, 0);
    }
}

