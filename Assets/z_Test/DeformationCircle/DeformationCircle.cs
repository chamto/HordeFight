using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformationCircle : MonoBehaviour 
{

    public Transform highestPoint = null;
    public Transform anchorPointA = null;
    public Transform anchorPointB = null;

	// Use this for initialization
	void Start () 
    {
        highestPoint = GameObject.Find("highestPoint").transform;
        anchorPointA = GameObject.Find("anchorPointA").transform;
        anchorPointB = GameObject.Find("anchorPointB").transform;
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    //**** 1사분면에서만 정상 동작한다 ****
	private void OnDrawGizmos()
	{
        if (null == highestPoint) return;

        float radius = 10f;

        DebugWide.DrawCircle(Vector3.zero, radius, Color.black);
        DebugWide.DrawLine(Vector3.zero, Vector3.right * radius, Color.gray);
        DebugWide.DrawLine(Vector3.zero, Vector3.forward * radius, Color.gray);

        DebugWide.DrawLine(anchorPointA.position, highestPoint.position, Color.green);
        DebugWide.DrawLine(anchorPointB.position, highestPoint.position, Color.green);


        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        float t = highestPoint.position.magnitude - radius; 

        float angleA = Vector3.SignedAngle(Vector3.forward,anchorPointA.position,  Vector3.up);
        float angleB = Vector3.SignedAngle(Vector3.forward,anchorPointB.position,   Vector3.up);
        float angleH = Vector3.SignedAngle(Vector3.forward,highestPoint.position,   Vector3.up);
        DebugWide.PrintText(anchorPointA.position, Color.black, "A : " + angleA);
        DebugWide.PrintText(anchorPointB.position, Color.black, "B : " + angleB);
        DebugWide.PrintText(highestPoint.position, Color.black, "H : " + angleH + "  t : " + t);
        //angleH += 30f;

        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        float maxAngle = angleA > angleB ? angleA : angleB;
        float maxTd = (maxAngle * t) / angleH;

        float angleD = 0f;
        bool isTornado = false;
        float count = 300;
        Vector3 prevPos = Vector3.zero;
        for (int i = 0; i < count;i++)
        {
            if(false == isTornado)
            {
                angleD = Mathf.LerpAngle(angleA, angleB, i / (float)count);
                //angleD = Mathf.Lerp(angleA, angleB, i / (float)count);
            }
            else
            {
                angleD = i * 5f; //계속 증가하는 각도 .. 파도나치 수열의 소용돌이 모양이 나옴 
            }


            Vector3 tdPos = Quaternion.AngleAxis(angleD, Vector3.up) * Vector3.forward;

            float td = (angleD * t) / angleH;

            if (false == isTornado && td > t) 
            {
                //최고점을 기준으로 대칭형을 만들어 준다    
                td = td - maxTd;
                td *= -1f;
                td /= (maxTd - t); //1~0로 변환
                td *= t; //t~0 범위값으로 바꾸어 준다 
            }


            tdPos = tdPos * (radius + td);

            //DebugWide.DrawLine(Vector3.zero, tdPos, Color.red);
            //DebugWide.PrintText(tdPos, Color.black, " " + td);
            DebugWide.DrawLine(prevPos, tdPos, Color.blue);

            prevPos = tdPos;
        }
	}
}
