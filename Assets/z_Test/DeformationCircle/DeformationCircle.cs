using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformationCircle : MonoBehaviour 
{

    public bool _isTornado = true;
    public bool _isInter = false;

    public float _radius = 10f;
    public Transform _sphereCenter = null;
    public Transform _highestPoint = null;
    public Transform _anchorPointA = null;
    public Transform _anchorPointB = null;

    private Vector3 _initialDir = Vector3.forward;

	// Use this for initialization
	void Start () 
    {
        _sphereCenter = GameObject.Find("sphereCenter").transform;   
        _highestPoint = GameObject.Find("highestPoint").transform;
        _anchorPointA = GameObject.Find("anchorPointA").transform;
        _anchorPointB = GameObject.Find("anchorPointB").transform;
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    //chamto 2019-08-31 제작 
    public Vector3 DeformationSpherePoint(float rotateAngle, Vector3 sphereCenter, float sphereRadius, Vector3 anchorA , Vector3 anchorB , Vector3 highestPoint , int interpolationNumber)
    {

        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (highestPoint - sphereCenter);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - sphereRadius;

        Vector3 upDir = Vector3.Cross(anchorA - sphereCenter, anchorB - sphereCenter);
        upDir.Normalize();

        //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
        Vector3 initialDir = Quaternion.AngleAxis(-90f, upDir) * centerToHighestPoint;
        initialDir.Normalize();

        float angleA = Vector3.SignedAngle(initialDir, anchorA - sphereCenter, upDir);
        float angleB = Vector3.SignedAngle(initialDir, anchorB - sphereCenter, upDir);
        float angleH = 90f;

        //-1~-179 각도표현을 1~179 로 변환한다
        //각도가 음수영역으로 들어가면 양수영역 각도로 변환한다 (각도가 음수영역으로 들어가면 궤적이 올바르게 표현이 안됨)  
        if (0 > angleA)
            angleA *= -1;
        if (0 > angleB)
            angleB *= -1;


        if (angleH > angleA && angleH > angleB)
        {   //최고점 위영역에 앵커 두개가 있을 때의 예외처리 

            //최고점과 가까운 각도 찾기 
            if (angleA > angleB)
            {
                angleA = 91f;
            }
            else
            {
                angleB = 91f;
            }
        }
        if (angleH < angleA && angleH < angleB)
        {   //최고점 아래영역에 앵커 두개가 있을 떄의 예외처리 

            if (angleA < angleB)
            {
                angleA = 89f;
            }
            else
            {
                angleB = 89f;
            }
        }


        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        float maxAngle = angleA > angleB ? angleA : angleB;
        float minAngle = angleA < angleB ? angleA : angleB;
        float maxTd = (maxAngle * t) / angleH;
        float minTd = (minAngle * t) / angleH;


        Vector3 tdDir = Quaternion.AngleAxis(rotateAngle, upDir) * initialDir;
        float td = 0f;

        if(minAngle <= rotateAngle && rotateAngle <= maxAngle)
        {
            
            td = (rotateAngle * t) / angleH;

            //최고점이 중심원의 외부에 위치한 경우
            bool outside_highestPoint = td < t;

            if (highestPointLength < sphereRadius)
            {   //최고점이 중심원의 내부에 위치한 경우의 예외처리 
                outside_highestPoint = !outside_highestPoint;
            }

            //회오리 값의 지정구간 비율값을 0~1 , 1~0 으로 변환시킨다
            if (outside_highestPoint)
            {
                td = td - minTd; //minTd ~ t => 0 ~ (t - minTd)
                td /= (t - minTd); //0~1로 변환
            }
            else
            {
                //최고점을 기준으로 대칭형을 만들어 준다    
                td = maxTd - td; //t ~ maxTd => (maxTd - t) ~ 0
                td /= (maxTd - t); //1~0로 변환
            }

            //0 또는 범위외값 : 보간없음
            //1~4 : 번호가 높을 수록 표현이 날카로워 진다 
            switch(interpolationNumber)
            {
                
                case 1:
                    td = UtilGS9.Interpolation.easeInSine(0, 1f, td); //살짝 둥근 표현 
                    break;
                case 2:
                    td = UtilGS9.Interpolation.easeInCirc(0, 1f, td); //직선에 가까운 표현 가능 *
                    break;
                case 3:
                    td = UtilGS9.Interpolation.easeInQuart(0, 1f, td); //직선에 가까운 표현 가능 **
                    break;
                case 4:
                    td = UtilGS9.Interpolation.easeInBack(0, 1f, td); //직선에 가까운 표현 가능 ***
                    break;
                
                
            }

            td *= t; //0~t 범위로 변환 
        }

        return sphereCenter + tdDir * (sphereRadius + td);
    }


    private void PrintDemo(Vector3 pos, int interpolationNumber)
    {
        if (null == _highestPoint) return;

        DebugWide.PrintText(pos, Color.black, "interpolationN : " + interpolationNumber);

        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        int count = 36;
        for (int i = 0; i < count;i++)
        {
            cur = DeformationSpherePoint(i * 10, _sphereCenter.position , _radius, _anchorPointA.position, _anchorPointB.position, _highestPoint.position, interpolationNumber);
            //cur += Vector3.right * 35;
            cur += pos;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, Color.cyan);

            prev = cur;
        }

    }

	private void OnDrawGizmos()
	{
        //=======
        Vector3 demoPos = Vector3.right * 70 + Vector3.forward * 70;
        PrintDemo(demoPos - Vector3.forward * 35 * 0, 0);
        PrintDemo(demoPos - Vector3.forward * 35 * 1, 1);
        PrintDemo(demoPos - Vector3.forward * 35 * 2, 2);
        PrintDemo(demoPos - Vector3.forward * 35 * 3, 3);
        PrintDemo(demoPos - Vector3.forward * 35 * 4, 4);
        //=======

        if (null == _highestPoint) return;

        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (_highestPoint.position - _sphereCenter.position);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - _radius;

        Vector3 upDir = Vector3.Cross(_anchorPointA.position - _sphereCenter.position, _anchorPointB.position - _sphereCenter.position);
        upDir.Normalize();

        //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
        _initialDir = Quaternion.AngleAxis(-90f, upDir) * centerToHighestPoint;
        _initialDir.Normalize();

        float angleA = Vector3.SignedAngle(_initialDir, _anchorPointA.position - _sphereCenter.position,  upDir);
        float angleB = Vector3.SignedAngle(_initialDir, _anchorPointB.position - _sphereCenter.position,   upDir);
        //float angleH = Vector3.SignedAngle(_initialDir, centerToHighestPoint,   upDir);
        float angleH = 90f;

        //-1~-179 각도표현을 1~179 로 변환한다
        //각도가 음수영역으로 들어가면 양수영역 각도로 변환한다 (각도가 음수영역으로 들어가면 궤적이 올바르게 표현이 안됨)  
        if (0 > angleA)
            angleA *= -1;
        if (0 > angleB)
            angleB *= -1;


        if(angleH > angleA && angleH > angleB)
        {   //최고점 위영역에 앵커 두개가 있을 때의 예외처리 

            //최고점과 가까운 각도 찾기 
            if(angleA > angleB)
            {
                angleA = 91f;
            }else
            {
                angleB = 91f;
            }
        }
        if(angleH < angleA && angleH < angleB)
        {   //최고점 아래영역에 앵커 두개가 있을 떄의 예외처리 

            if (angleA < angleB)
            {
                angleA = 89f;
            }
            else
            {
                angleB = 89f;
            }
        }

        //-1~-179 각도표현을 359~181 로 변환한다 
        //if (0 > angleH)
            //angleH += 360f;

        //----------- debug print -----------
        Vector3 angle_M45 = _initialDir;
        Vector3 angle_P45 = Quaternion.AngleAxis(180f, upDir) * _initialDir;
        DebugWide.DrawLine(_sphereCenter.position, _sphereCenter.position + angle_M45 * _radius, Color.red);
        DebugWide.DrawLine(_sphereCenter.position, _sphereCenter.position + angle_P45 * _radius, Color.red);
        //----------- debug print -----------
        DebugWide.DrawCircle(_sphereCenter.position, _radius, Color.black);
        DebugWide.DrawLine(_sphereCenter.position, _anchorPointA.position, Color.gray);
        DebugWide.DrawLine(_sphereCenter.position, _anchorPointB.position, Color.gray);

        DebugWide.DrawLine(_anchorPointA.position, _highestPoint.position, Color.green);
        DebugWide.DrawLine(_anchorPointB.position, _highestPoint.position, Color.green);
        DebugWide.DrawLine(_sphereCenter.position, _highestPoint.position, Color.red);
        //----------- debug print -----------
        DebugWide.PrintText(_anchorPointA.position, Color.black, "A : " + angleA);
        DebugWide.PrintText(_anchorPointB.position, Color.black, "B : " + angleB);
        DebugWide.PrintText(_highestPoint.position, Color.black, "H : " + angleH + "  t : " + t);
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
        Vector3 prevPos = _sphereCenter.position;
        Vector3 tdPos = _sphereCenter.position;

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


            tdPos = _sphereCenter.position + tdPos * (_radius + td);

            //----------- debug print -----------
            if (0 != i)
                DebugWide.DrawLine(prevPos, tdPos, Color.gray);
            //----------- debug print -----------

            prevPos = tdPos;

        }



        //회오리 값의 지정구간 비율값을 0~1 , 1~0 으로 변환시킨다 
        count = 30;
        bool outside_highestPoint = true; 
        for (int i = 0; i < count+1;i++)
        {
            
            //angleD = Mathf.LerpAngle(angleA, angleB, i / (float)count);
            angleD = Mathf.LerpAngle(minAngle, maxAngle, i / (float)count);
            //angleD = Mathf.Lerp(angleA, angleB, i / (float)count);

            tdPos = Quaternion.AngleAxis(angleD, upDir) * _initialDir;


            float td = (angleD * t) / angleH;
            //DebugWide.PrintText(tdPos * _radius, Color.black, " " + td + "  " + angleD);

            if(false == _isTornado)
            {
                //최고점이 중심원의 외부에 위치한 경우
                outside_highestPoint = td < t;

                if (highestPointLength < _radius)
                {   //최고점이 중심원의 내부에 위치한 경우의 예외처리 
                    outside_highestPoint = !outside_highestPoint;
                }

                //if (td < t)
                if(outside_highestPoint)
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

                if(true == _isInter)
                {
                    td = UtilGS9.Interpolation.easeInQuart(0, 1f, td); //직선에 가까운 표현 가능 *
                    //td = UtilGS9.Interpolation.easeInBack(0, 1f, td); //직선에 가까운 표현 가능 **
                    //td = UtilGS9.Interpolation.easeInCirc(0, 1f, td); //직선에 가까운 표현 가능 ***
                    //td = UtilGS9.Interpolation.easeInSine(0, 1f, td);     
                }



                td *= t; //0~t 범위로 변환 
            }



            tdPos = _sphereCenter.position +  tdPos * (_radius + td);

            //----------- debug print -----------
            //DebugWide.DrawLine(sphereCenter.position, tdPos, Color.red);
            if(0 != i) 
                DebugWide.DrawLine(prevPos, tdPos, Color.blue);
            //----------- debug print -----------

            prevPos = tdPos;

        }


	}
}
