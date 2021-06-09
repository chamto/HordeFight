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

        Reset();
	}

	private void Update()
	{
        if (null == _start || null == _end) return;

        if(Input.GetKeyDown(KeyCode.R))
        {
            Reset(); 
        }

        Vector3 move = Move();

        DebugWide.AddDrawQ_Line(transform.position,  move, Color.white);

        transform.position = move;

    }

    private void OnDrawGizmos()
    {
        DebugWide.DrawQ_All();
    }

    public void Reset()
    {
        DebugWide.ClearDrawQ();

        transform.position = _start.position;
        StartPos = _start.position;
        DestPos = _end.position;
        t = 0;

        PreCalculate();
    }

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
    public float MAX_Y = 50f; //최고점 높이 
    public float mht = 0.2f; //최고점 도달 시간

    //등가속도 상황에서의 운동방정식 5가지
    //ref : https://en.wikipedia.org/wiki/Equations_of_motion
    void PreCalculate()
    {
        dh = DestPos.y - StartPos.y;
        mh = MAX_Y - StartPos.y;

        g = 2 * mh / (mht * mht); //운동방정식2번 이용 , 초기속도 0 인것을 이용하여 식을 정리한다 

        vy = Mathf.Sqrt(2 * g * mh); //운동방정식4번 이용 , 초기속도 0 

        //2차 방정식을 어떻게 세운것인지 모르겠다 ??
        float a = g;
        float b = -2 * vy;
        float c = 2 * dh;

        dat = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a); //근의 공식으로 전체 시간길이를 구한다  
        vx = -(StartPos.x - DestPos.x) / dat; //v = s/t

    }

    Vector3 Move()
    {
        //t += Time.deltaTime;
        t += 0.02f;

        if (t > dat) return DestPos;

        //운동방정식2번 이용 
        float x = StartPos.x + vx * t; //등속도 운동 
        float y = StartPos.y + vy * t - 0.5f * g * t * t; //g만큼 등가속도 운동 

        return new Vector3(x, y, 0);
    }
}

