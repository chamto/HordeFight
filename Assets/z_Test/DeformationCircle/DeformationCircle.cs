using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformationCircle : MonoBehaviour 
{

    public bool isTornado = true;
    public bool isInter = false;

    public float _radius = 10f;
    public Transform sphereCenter = null;
    public Transform highestPoint = null;
    public Transform anchorPointA = null;
    public Transform anchorPointB = null;

    private Vector3 _initialDir = Vector3.forward;

	// Use this for initialization
	void Start () 
    {
        sphereCenter = GameObject.Find("sphereCenter").transform;   
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

        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        float centerToHighestPoint = (highestPoint.position - sphereCenter.position).magnitude;
        float t = centerToHighestPoint - _radius;

        Vector3 upDir = Vector3.Cross(anchorPointA.position - sphereCenter.position, anchorPointB.position - sphereCenter.position);
        upDir.Normalize();

        //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
        _initialDir = Quaternion.AngleAxis(-90f, upDir) * (highestPoint.position - sphereCenter.position);
        _initialDir.Normalize();

        float angleA = Vector3.SignedAngle(_initialDir, anchorPointA.position - sphereCenter.position,  upDir);
        float angleB = Vector3.SignedAngle(_initialDir, anchorPointB.position - sphereCenter.position,   upDir);
        float angleH = Vector3.SignedAngle(_initialDir, highestPoint.position - sphereCenter.position,   upDir);

        //-1~-179 각도표현을 1~179 로 변환한다
        //각도가 음수영역으로 들어가면 양수영역 각도로 변환한다 (각도가 음수영역으로 들어가면 궤적이 올바르게 표현이 안됨)  
        if (0 > angleA)
            angleA *= -1;
            //angleA += 360f;
        if (0 > angleB)
            angleB *= -1;
            //angleB += 360f;

        //-1~-179 각도표현을 359~181 로 변환한다 
        if (0 > angleH)
            angleH += 360f;

        //----------- debug print -----------
        Vector3 angle_M45 = _initialDir;
        Vector3 angle_P45 = Quaternion.AngleAxis(180f, upDir) * _initialDir;
        DebugWide.DrawLine(sphereCenter.position, sphereCenter.position + angle_M45 * _radius, Color.red);
        DebugWide.DrawLine(sphereCenter.position, sphereCenter.position + angle_P45 * _radius, Color.red);
        //----------- debug print -----------
        DebugWide.DrawCircle(sphereCenter.position, _radius, Color.black);
        DebugWide.DrawLine(sphereCenter.position, anchorPointA.position, Color.gray);
        DebugWide.DrawLine(sphereCenter.position, anchorPointB.position, Color.gray);

        DebugWide.DrawLine(anchorPointA.position, highestPoint.position, Color.green);
        DebugWide.DrawLine(anchorPointB.position, highestPoint.position, Color.green);
        DebugWide.DrawLine(sphereCenter.position, highestPoint.position, Color.red);
        //----------- debug print -----------
        DebugWide.PrintText(anchorPointA.position, Color.black, "A : " + angleA);
        DebugWide.PrintText(anchorPointB.position, Color.black, "B : " + angleB);
        DebugWide.PrintText(highestPoint.position, Color.black, "H : " + angleH + "  t : " + t);
        //----------- debug print -----------


        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        float maxAngle = angleA > angleB ? angleA : angleB;
        float minAngle = angleA < angleB ? angleA : angleB;
        float maxTd = (maxAngle * t) / angleH;
        float minTd = (minAngle * t) / angleH;

        float angleD = 0f;
        float count = 5;
        Vector3 prevPos = sphereCenter.position;
        Vector3 tdPos = sphereCenter.position;

        //최고점 기준 -90도 로 설정된 회오리를 그린다 
        count = 300;
        for (int i = 0; i < count; i++)
        {

            //5도 간격으로 각도를 늘린다 
            angleD = i * 5f; //계속 증가하는 각도 .. 파도나치 수열의 소용돌이 모양이 나옴 

            //각도변환 : 181~359 => -179~-1 
            //angleD %= 360f;
            //if(angleD > 180f)
                //angleD -= 360f;

            tdPos = Quaternion.AngleAxis(angleD, upDir) * _initialDir;


            float td = (angleD * t) / angleH;
            //float td = (angleD * 4f) / 45f;


            tdPos = sphereCenter.position + tdPos * (_radius + td);

            //----------- debug print -----------
            if (0 != i)
                DebugWide.DrawLine(prevPos, tdPos, Color.gray);
            //----------- debug print -----------

            prevPos = tdPos;

        }

        //회오리 값의 지정구간 비율값을 0~1 , 1~0 으로 변환시킨다 
        count = 30;
        for (int i = 0; i < count+1;i++)
        {
            
            //angleD = Mathf.LerpAngle(angleA, angleB, i / (float)count);
            angleD = Mathf.LerpAngle(minAngle, maxAngle, i / (float)count);
            //angleD = Mathf.Lerp(angleA, angleB, i / (float)count);

            tdPos = Quaternion.AngleAxis(angleD, upDir) * _initialDir;


            float td = (angleD * t) / angleH;
            //DebugWide.PrintText(tdPos * _radius, Color.black, " " + td + "  " + angleD);

            if(false == isTornado)
            {
                if (td < t)
                {
                    //td /= t; //0~1로 변환
                    //td *= t; //0~t 범위로 변환 

                    td = td - minTd; //minTd ~ t => 0 ~ (t - minTd)
                    td /= (t - minTd); //0~1로 변환

                }
                else //td >= t 
                {
                    //최고점을 기준으로 대칭형을 만들어 준다    
                    td = maxTd - td; //t ~ maxTd => (maxTd - t) ~ 0
                    td /= (maxTd - t); //1~0로 변환
                                       //td *= t; //t~0 범위값으로 바꾸어 준다 
                }

                if(true == isInter)
                {
                    td = UtilGS9.Interpolation.easeInQuart(0, 1f, td); //직선에 가까운 표현 가능 *
                    //td = UtilGS9.Interpolation.easeInBack(0, 1f, td); //직선에 가까운 표현 가능 **
                    //td = UtilGS9.Interpolation.easeInCirc(0, 1f, td); //직선에 가까운 표현 가능 ***
                    //td = UtilGS9.Interpolation.easeInSine(0, 1f, td);     
                }



                td *= t; //0~t 범위로 변환 
            }



            tdPos = sphereCenter.position +  tdPos * (_radius + td);

            //----------- debug print -----------
            //DebugWide.DrawLine(sphereCenter.position, tdPos, Color.red);
            if(0 != i) 
                DebugWide.DrawLine(prevPos, tdPos, Color.blue);
            //----------- debug print -----------

            prevPos = tdPos;

        }


	}
}
