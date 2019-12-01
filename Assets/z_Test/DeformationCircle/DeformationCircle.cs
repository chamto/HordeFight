using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UtilGS9;

public class DeformationCircle : MonoBehaviour
{

    public bool _isTornado = true;
    public bool _isInter = false;

    public float _radius = 5f;
    public float _maxAngle = 360f * 2f;
    public Transform _sphereCenter = null;
    public Transform _highestPoint = null;
    public Transform _anchorPointA = null;
    public Transform _anchorPointB = null;

    public Transform _tornadoCenter = null;
    public Transform _tc_upDir_endPos = null;
    public Transform _tc_unlaceDir_endPos = null;
    public Transform _tc_handle = null;
    public Transform _tc_highest = null;

    public Transform _arcCenter         = null;
    public Transform _ac_upDir_endPos   = null;
    public Transform _ac_handle         = null;
    public Transform _ac_far_endPos     = null;
    public Transform _ac_near_endPos    = null;
    public Transform _ac_degree         = null;
    public Transform _ac_near_included = null;


    public Transform _cylinderCenter    = null;
    public Transform _cc_far_o   = null;
    public Transform _cc_handle         = null;
    public Transform _cc_far_endPos     = null;
    public Transform _cc_near_endPos    = null;


    private Vector3 _initialDir = Vector3.forward;

    // Use this for initialization
    void Start()
    {
        _sphereCenter = GameObject.Find("sphereCenter").transform;
        _highestPoint = GameObject.Find("highestPoint").transform;
        _anchorPointA = GameObject.Find("anchorPointA").transform;
        _anchorPointB = GameObject.Find("anchorPointB").transform;

        _tornadoCenter = GameObject.Find("tornadoCenter").transform;
        _tc_upDir_endPos = GameObject.Find("tc_upDir_endPos").transform;
        _tc_unlaceDir_endPos = GameObject.Find("tc_unlaceDir_endPos").transform;
        _tc_handle = GameObject.Find("tc_handle").transform;
        _tc_highest = GameObject.Find("tc_highest").transform;


        _arcCenter = GameObject.Find("arcCenter").transform;
        _ac_upDir_endPos = GameObject.Find("ac_upDir_endPos").transform;
        _ac_handle       = GameObject.Find("ac_handle").transform;
        _ac_far_endPos   = GameObject.Find("ac_far_endPos").transform;
        _ac_near_endPos  = GameObject.Find("ac_near_endPos").transform;
        _ac_degree       = GameObject.Find("ac_degree").transform;
        _ac_near_included = GameObject.Find("ac_near_included").transform;


        _cylinderCenter     = GameObject.Find("cylinderCenter").transform;
        _cc_far_o    = GameObject.Find("cc_far_o").transform;
        _cc_handle          = GameObject.Find("cc_handle").transform;
        _cc_far_endPos      = GameObject.Find("cc_far_endPos").transform;
        _cc_near_endPos     = GameObject.Find("cc_near_endPos").transform;


    }

    // Update is called once per frame
    void Update()
    {

    }

    //chamto 2019-08-31 제작 
    public Vector3 DeformationSpherePoint(float rotateAngle, Vector3 sphereCenter, float sphereRadius, Vector3 anchorA, Vector3 anchorB, Vector3 highestPoint, int interpolationNumber)
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

        if (minAngle <= rotateAngle && rotateAngle <= maxAngle)
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
            switch (interpolationNumber)
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

    public void Gizimo_DeformationSpherePoint(Vector3 dPos, Vector3 sphereCenter, float sphereRadius, Vector3 anchorA, Vector3 anchorB, Vector3 highestPoint, int interpolationNumber)
    {
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        int count = 36;
        for (int i = 0; i < count; i++)
        {
            cur = DeformationSpherePoint(i * 10, sphereCenter, sphereRadius, anchorA, anchorB, highestPoint, interpolationNumber);

            if (0 != i)
                DebugWide.DrawLine(prev, dPos + cur, Color.cyan);

            prev = dPos + cur;
        }

        //=============================
        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (highestPoint - sphereCenter);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - sphereRadius;

        Vector3 upDir = Vector3.Cross(anchorA - sphereCenter, anchorB - sphereCenter);
        upDir.Normalize();

        //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
        Vector3 initialDir = Quaternion.AngleAxis(-90f, upDir) * centerToHighestPoint;
        initialDir.Normalize();
        //----------- debug print -----------
        Vector3 angle_M45 = initialDir;
        Vector3 angle_P45 = Quaternion.AngleAxis(180f, upDir) * initialDir;
        DebugWide.DrawLine(dPos + sphereCenter, dPos + sphereCenter + angle_M45 * sphereRadius, Color.red);
        DebugWide.DrawLine(dPos + sphereCenter, dPos + sphereCenter + angle_P45 * sphereRadius, Color.red);
        //----------- debug print -----------
        //DebugWide.DrawCircle(dPos + sphereCenter, sphereRadius, Color.black);
        DebugWide.DrawLine(dPos + sphereCenter, dPos + anchorA, Color.gray);
        DebugWide.DrawLine(dPos + sphereCenter, dPos + anchorB, Color.gray);

        DebugWide.DrawLine(dPos + anchorA, dPos + highestPoint, Color.green);
        DebugWide.DrawLine(dPos + anchorB, dPos + highestPoint, Color.green);
        DebugWide.DrawLine(dPos + sphereCenter, dPos + highestPoint, Color.red);
        //----------- debug print -----------

    }

    private void PrintDemo(Vector3 pos, int interpolationNumber)
    {
        if (null == _highestPoint) return;

        DebugWide.PrintText(pos, Color.black, "interpolationN : " + interpolationNumber);

        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        int count = 36;
        for (int i = 0; i < count; i++)
        {
            cur = DeformationSpherePoint(i * 10, _sphereCenter.position, _radius, _anchorPointA.position, _anchorPointB.position, _highestPoint.position, interpolationNumber);
            //cur += Vector3.right * 35;
            cur += pos;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, Color.cyan);

            prev = cur;
        }

    }

    public void DrawTornano_T2AndAngle2(Vector3 target_pos, Vector3 circle_pos, float circle_radius, Vector3 upDir, bool endure_upDir, Vector3 circle_highest, float circle_maxAngle)
    {
        Vector3 centerToHighestPoint = (circle_highest - circle_pos);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - circle_radius;

        float t_td = (target_pos - circle_pos).magnitude - circle_radius; //target_pos에 대한 td를 바로 구한다 
        float t_angleD = (t_td * circle_maxAngle) / t;

        Vector3 initialDir = Quaternion.AngleAxis(360f - circle_maxAngle, upDir) * centerToHighestPoint;
        initialDir.Normalize();

       
        Vector3 tdPos = Quaternion.AngleAxis(t_angleD, upDir) * initialDir;
        tdPos = circle_pos + tdPos * (circle_radius + t_td);

        DebugWide.DrawLine(circle_pos, tdPos, Color.black);    

    }


    //회오리 자체의 방향을 바꾸는 것이 아님. 회오리 풀어지는 방향만 특정 방향으로 바꾸는 것임 
    //unlace_dir 정규화 되어 있지 않아도 됨 
    public Vector3 Trans_UnlaceDir(Vector3 unlace_dir, Vector3 upDir, Vector3 forward)
    {

        Vector3 cur_dir = Vector3.Cross(forward, upDir);

        if (Vector3.Dot(cur_dir, unlace_dir) < 0)
            upDir *= -1f;
        
        return upDir;
    }


    //2차원 회오리상의 목표위치만 구하는 함수 
    public Vector3 DeformationCirclePos_Tornado2D(Vector3 target_pos, Vector3 circle_pos, float circle_radius, Vector3 n_upDir, Vector3 circle_highest, float circle_maxAngle)
    {
        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (circle_highest - circle_pos);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - circle_radius;


        //==================================================
        Vector3 centerToTarget = target_pos - circle_pos;
        float t_target = centerToTarget.magnitude - circle_radius; //target_pos에 대한 td를 바로 구한다 
        float t_angleD = (t_target * circle_maxAngle) / t;

        if (t_angleD > circle_maxAngle) t_angleD = circle_maxAngle; //최대각도 이상 계산을 막는다 
        else if (t_angleD < 0) t_angleD = 0;

        //==================================================


        Vector3 initialDir = Quaternion.AngleAxis(360f - circle_maxAngle, n_upDir) * centerToHighestPoint;
        initialDir.Normalize();


        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        //angleD = (td * angleH) / t 

        float angleD = Geo.Angle360(initialDir, centerToTarget, n_upDir); //upDir벡터 기준으로 두벡터의 최소각 반환 
        int weight = (int)((t_angleD - angleD) / 360f); //회오리 두께구하기 , angleD(첫번째 회오리 두께의 각도)를 빼지 않으면 회오리가 아닌 원이 된다 


        angleD += weight * 360f; //회오리 두꼐에 따라 각도를 더한다 
        if (angleD > circle_maxAngle) angleD -= 360f; //더한 각도가 최대범위를 벗어나면 한두께 아래 회오리를 선택한다 


        Vector3 tdPos = circle_pos;
        tdPos = Quaternion.AngleAxis(angleD, n_upDir) * initialDir;
        float td = (angleD * t) / circle_maxAngle;
        tdPos = circle_pos + tdPos * (circle_radius + td);

        return tdPos;
    }

    //circle_pos 가 0이 아닐때 결과값이 부정확해지는 문제발생 - chamto 20191123
    //n_upDir 은 정규화된 값이 들어와야 한다 
    public Vector3 DeformationCirclePos_Tornado3D(Vector3 target_pos, Vector3 circle_pos, float circle_radius, Vector3 n_upDir ,  Vector3 circle_highest, float circle_maxAngle)
    {
        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (circle_highest - circle_pos);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - circle_radius;


        //==================================================
        //t, t_target 모두 upDir 기준으로 투영평면에 투영한 값을 사용해야 highest 의 높이 변화시에도 올바른 위치를 계산할 수 있다 
        Vector3 proj_centerToHighest = centerToHighestPoint - n_upDir * Vector3.Dot(n_upDir, centerToHighestPoint);
        float proj_highestPointLength = proj_centerToHighest.magnitude;
        float proj_t = proj_highestPointLength - circle_radius; //proj_t 가 음수가 되는 경우 : 반지름 보다 작은 최고점길이 일때 예외처리가 현재 없다 


        Vector3 centerToTarget = target_pos - circle_pos;
        Vector3 proj_target = centerToTarget - n_upDir * Vector3.Dot(n_upDir, centerToTarget);
        //float t_target = (proj_target - circle_pos).magnitude - circle_radius; 
        float t_target = proj_target.magnitude - circle_radius; //target_pos에 대한 td를 바로 구한다.  proj_target 는 이미 벡터값이므로 다시 원점에서 출발하는 점으로 계산하면 안된다 
        float t_angleD = (t_target * circle_maxAngle) / proj_t;

        //DebugWide.LogBlue(t_angleD + "  = " + t_target + "  *  " +  circle_maxAngle + "  / " + proj_t + "   " + (proj_target).magnitude);

        if (t_angleD > circle_maxAngle) t_angleD = circle_maxAngle; //최대각도 이상 계산을 막는다 
        else if (t_angleD < 0) t_angleD = 0;

        //==================================================


        Vector3 initialDir = Quaternion.AngleAxis(360f - circle_maxAngle, n_upDir) * centerToHighestPoint;
        initialDir.Normalize();


        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        //angleD = (td * angleH) / t 
        //float angleH = circle_maxAngle; //이 각도 값이 클수록 회오리가 작아진다.


        //float angleD = Geo.Angle360_AxisRotate(initialDir, target_pos, n_upDir); //음수표현없이 양수로 반환  
        float angleD = Geo.Angle360_AxisRotate_Normal_Axis(initialDir, centerToTarget, n_upDir); 
        int weight = (int)((t_angleD - angleD) / 360f); //회오리 두께구하기 , angleD(첫번째 회오리 두께의 각도)를 빼지 않으면 회오리가 아닌 원이 된다 

        //DebugWide.LogBlue(angleD + "  " + t_angleD + "  " + weight + "  " + initialDir + "   " + n_upDir); //test

        angleD += weight * 360f; //회오리 두꼐에 따라 각도를 더한다 
        if (angleD > circle_maxAngle) angleD -= 360f; //더한 각도가 최대범위를 벗어나면 한두께 아래 회오리를 선택한다 


        Vector3 tdPos = Quaternion.AngleAxis(angleD, n_upDir) * initialDir;
        float td = (angleD * t) / circle_maxAngle;
        tdPos = circle_pos + tdPos * (circle_radius + td);

        return tdPos;
    }

    //회오리 3d 로 테스트 필요 
    public Vector3 DeformationCirclePos_Tornado3D(float src_angle, Vector3 circle_pos, float circle_radius, Vector3 upDir, Vector3 circle_highest, float circle_maxAngle)
    {
        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (circle_highest - circle_pos);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - circle_radius;

        //Vector3 initialDir = centerToHighestPoint / highestPointLength;
        Vector3 initialDir = Quaternion.AngleAxis(360f - circle_maxAngle, upDir) * centerToHighestPoint;
        initialDir.Normalize();

        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        float angleH = circle_maxAngle; //이 각도 값이 클수록 회오리가 작아진다. 
        float angleD = src_angle;

        Vector3 tdPos = circle_pos;
        tdPos = Quaternion.AngleAxis(angleD, upDir) * initialDir;
        float td = (angleD * t) / angleH;
        tdPos = circle_pos + tdPos * (circle_radius + td);

        return tdPos;
    }

    public void DeformationCirclePos_Tornado3D_Gizimo(Vector3 plus_pos, Vector3 circle_pos , float circle_radius , Vector3 upDir, Vector3 circle_highest, float circle_maxAngle)
    {
        
        //=================================

        //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
        Vector3 centerToHighestPoint = (circle_highest - circle_pos);
        float highestPointLength = centerToHighestPoint.magnitude;
        float t = highestPointLength - circle_radius;

        //Vector3 initialDir = centerToHighestPoint / highestPointLength;
        Vector3 initialDir = Quaternion.AngleAxis(360f - circle_maxAngle , upDir) * centerToHighestPoint;
        initialDir.Normalize();

        //==================================================

        //비례식을 이용하여 td 구하기 
        //angleD : td  = angleH : t
        //td * angleH = angleD * t
        //td = (angleD * t) / angleH
        float minAngle = 0;
        float maxAngle = circle_maxAngle;
        float angleH = circle_maxAngle; //이 각도 값이 클수록 회오리가 작아진다. 
        float angleD = 0f;
        float count = 300; //5
        Vector3 prevPos = circle_pos;
        Vector3 tdPos = circle_pos;

        //최고점 기준 -90도 로 설정된 회오리를 그린다 
        for (int i = 0; i < count; i++)
        {

            //5도 간격으로 각도를 늘린다 
            angleD = i * 5f; //계속 증가하는 각도 .. 파도나치 수열의 소용돌이 모양이 나옴 

            tdPos = Quaternion.AngleAxis(angleD, upDir) * initialDir;

            float td = (angleD * t) / angleH;

            tdPos = circle_pos + tdPos * (circle_radius + td);
            //tdPos = this.DeformationCirclePos_Tornado(angleD, circle_pos, circle_radius, upDir, circle_highest, circle_maxAngle);
            //tdPos = this.DeformationCirclePos_Tornado(tdPos, circle_pos, circle_radius, upDir, circle_highest, circle_maxAngle);

            //----------- debug print -----------
            if (0 != i)
                DebugWide.DrawLine(plus_pos+ prevPos, plus_pos + tdPos, Color.gray);
            //----------- debug print -----------

            prevPos = tdPos;

        }
        //==================================================

        count = 30;
        for (int i = 0; i < count + 1; i++)
        {
            
            //angleD = Mathf.LerpAngle(minAngle, maxAngle, i / (float)count); //180도 이상 계산못함 
            angleD = Mathf.Lerp(minAngle, maxAngle, i / (float)count);
            //DebugWide.LogBlue(i + " : " + angleD);
            tdPos = Quaternion.AngleAxis(angleD, upDir) * initialDir;


            float td = (angleD * t) / angleH;
            //DebugWide.PrintText(tdPos * _radius, Color.black, " " + td + "  " + angleD);


            tdPos = circle_pos + tdPos * (circle_radius + td);

            //----------- debug print -----------
            //DebugWide.DrawLine(target_pos + circle_pos, target_pos + tdPos, Color.red);
            if (0 != i)
                DebugWide.DrawLine(plus_pos + prevPos, plus_pos + tdPos, Color.blue);
            //----------- debug print -----------

            prevPos = tdPos;

        }


        //----------- debug print -----------
        DebugWide.DrawCircle(plus_pos + circle_pos, circle_radius, Color.black);
        DebugWide.DrawLine(plus_pos + circle_pos, plus_pos + circle_highest, Color.red);
    }


	private void OnDrawGizmos()
	{

        if (null == _highestPoint) return;

        //==================================================
        //실린더 출력 시험
        Vector3 n_cldFw = (_cc_far_o.position - _cylinderCenter.position).normalized;
        float cld_length = (_cc_far_o.position - _cylinderCenter.position).magnitude;
        Vector3 farPos = _cylinderCenter.position + n_cldFw * cld_length;
        Cylinder cld = new Cylinder();
        cld.pos = _cylinderCenter.position;
        cld.dir = n_cldFw;
        cld.length = cld_length;
        cld.radius_near = (_cc_near_endPos.position - _cylinderCenter.position).magnitude;
        cld.radius_far = (_cc_far_endPos.position - farPos).magnitude;

        Cylinder.DrawCylinder(cld);

        Vector3 cld_colPos = cld.CollisionPos(_cc_handle.position);
        DebugWide.DrawCircle(cld_colPos, 0.5f, Color.magenta);
        DebugWide.DrawLine(cld.pos, _cc_handle.position, Color.green);

        //==================================================
        //호 출력 시험
        Vector3 arcUp = (_ac_upDir_endPos.position - _arcCenter.position).normalized;
        Arc arc = new Arc();
        arc.pos = _arcCenter.position;
        arc.degree = _ac_degree.localPosition.x;
        arc.dir = (_ac_far_endPos.position - _arcCenter.position).normalized;
        arc.radius_near = (_ac_near_endPos.position - _arcCenter.position).magnitude;
        arc.radius_far = (_ac_far_endPos.position - _arcCenter.position).magnitude;
        //arc.radius_collider_standard = arc.radius_near;
        arc.ratio_nearSphere_included = _ac_near_included.localPosition.x;
        Arc.DrawArc(arc, arcUp);


        //==================================================

        Vector3 tornado_pos = _tornadoCenter.position;

        Vector3 unlaceDir = _tc_unlaceDir_endPos.localPosition;//Vector3.up; //위쪽으로 설정 (위에서 아래로 공격한다 가정) 
        Vector3 upDir1 = _tc_upDir_endPos.localPosition; //upDir_endPos 가 항상 0위치에서 출발한다 가정 
        upDir1 = this.Trans_UnlaceDir(unlaceDir, upDir1, _tc_highest.position - tornado_pos); //풀어지는 방향에 맞게 upDir 재설정  
        upDir1.Normalize();

        //=========================

        this.DeformationCirclePos_Tornado3D_Gizimo(Vector3.zero, tornado_pos, _radius, upDir1, _tc_highest.position, _maxAngle); //chamto test
        Vector3 torPos = this.DeformationCirclePos_Tornado3D(_tc_handle.position, tornado_pos, _radius, upDir1, _tc_highest.position, _maxAngle);
        DebugWide.DrawCircle(torPos, 2f, Color.magenta);
        DebugWide.DrawLine(tornado_pos, _tc_handle.position, Color.magenta);
        this.DrawTornano_T2AndAngle2(_tc_handle.position, tornado_pos, _radius, upDir1, true, _tc_highest.position, _maxAngle);

        //==================================================


        Vector3 demoPos = Vector3.right * 70 + Vector3.forward * 70;

        PrintDemo(demoPos - Vector3.forward * 35 * 0, 0);
        PrintDemo(demoPos - Vector3.forward * 35 * 1, 1);
        PrintDemo(demoPos - Vector3.forward * 35 * 2, 2);
        PrintDemo(demoPos - Vector3.forward * 35 * 3, 3);
        PrintDemo(demoPos - Vector3.forward * 35 * 4, 4);

        Gizimo_DeformationSpherePoint(Vector3.forward * 35 * 1, _sphereCenter.position, _radius, _anchorPointA.position, _anchorPointB.position, _highestPoint.position, 0);

        //=======

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

//==========================================================
//==========================================================
//==========================================================

public struct Sphere
{
    public Vector3 pos;
    public float radius;

    public Sphere(Vector3 p, float r)
    {
        pos = p;
        radius = r;
    }

    static public Sphere Zero
    {
        get
        {
            Sphere sphere = new Sphere();
            sphere.pos = Vector3.zero;
            sphere.radius = 0f;

            return sphere;
        }
    }

    public override string ToString()
    {
        return "pos: " + pos + "  radius: " + radius;
    }
}

public struct Arc
{
    public Vector3 pos;             //호의 시작점  
    public Vector3 dir;             //정규화 되어야 한다
    public float degree;            //각도 
    public float radius_near;       //시작점에서 가까운 원의 반지름 
    public float radius_far;        //시작점에서 먼 원의 반지름
                                    
    //ratio : [-1 ~ 1]
    //호에 원이 완전히 포함 [1]
    //호에 원의 중점까지 포함 [0]
    //호에 원의 경계까지 포함 [-1 : 포함 범위 가장 넒음] 
    public const float Fully_Included = -1f;
    public const float Focus_Included = 0f;
    //public const float Boundary_Included = 1f; //경계에 붙이기 위한 값은 가까운원의 반지름값임  
    public float ratio_nearSphere_included;

    public float GetFactor()
    {
        if (0f == ratio_nearSphere_included)//Focus_Included
            return 0f;
        if (0 < ratio_nearSphere_included)  //Boundary_Included
            return radius_near;
        //if(0 > ratio_nearSphere_included) //Fully_Included  
        return -radius_near / (float)Math.Sin(Mathf.Deg2Rad * degree * 0.5f);
    }

    //public float factor
    //{
    //    get
    //    {   //f = radius / sin
    //        //return radius_collider_standard / (float)Math.Sin(Mathf.Deg2Rad * degree * 0.5f);
    //        return radius_near / (float)Math.Sin(Mathf.Deg2Rad * degree * 0.5f);
    //    }
    //}


    public Vector3 GetPosition_Factor()
    {
        return pos + dir * GetFactor();
    }

    public Sphere sphere_near
    {
        get
        {
            Sphere sph;
            sph.pos = this.pos;
            sph.radius = this.radius_near;
            return sph;
        }

    }

    public Sphere sphere_far
    {
        get
        {
            Sphere sph;
            sph.pos = this.pos;
            sph.radius = this.radius_far;
            return sph;
        }

    }

    public static void DrawArc(Arc arc, Vector3 upDir)
    {
        Vector3 factorPos = arc.GetPosition_Factor();
        Vector3 interPos;

        Vector3 far = Quaternion.AngleAxis(-arc.degree * 0.5f, upDir) * arc.dir;
        UtilGS9.Geo.IntersectRay2(arc.pos, arc.radius_far, factorPos, far, out interPos);
        DebugWide.DrawLine(factorPos, interPos, Color.green);

        far = Quaternion.AngleAxis(arc.degree * 0.5f, upDir) * arc.dir;
        UtilGS9.Geo.IntersectRay2(arc.pos, arc.radius_far, factorPos, far, out interPos);
        DebugWide.DrawLine(factorPos, interPos, Color.green);

        DebugWide.DrawLine(arc.pos, arc.pos + arc.dir * arc.radius_far, Color.green);
        DebugWide.DrawCircle(arc.pos, arc.radius_far, Color.green);
        DebugWide.DrawCircle(arc.pos, arc.radius_near, Color.green);
    }

    public override string ToString()
    {

        return "pos: " + pos + "  dir: " + dir + "  degree: " + degree
            + "  radius_near: " + radius_near + "  radius_far: " + radius_far +  "  factor: " + GetFactor();
    }
}


//실린더는 선분의 특징을 가지고 있다 
public struct Cylinder
{
    public Vector3 pos;             //호의 시작점  
    public Vector3 dir;             //정규화 되어야 한다
    public float length;            //길이 
    public float radius_near;       //시작점에서 가까운 원의 반지름 
    public float radius_far;        //시작점에서 먼 원의 반지름
                                    

    public Vector3 CollisionPos(Vector3 nearToPos)
    {
        Vector3 colPos = this.pos;
        Vector3 farPos = this.pos + this.dir * this.length;
        Vector3 dirRight = Vector3.Cross(Vector3.up, this.dir);
        dirRight.Normalize();

        Vector3 toHandlePos = nearToPos - this.pos;
        //가까운 반구 충돌위치 찾기
        float test = Vector3.Dot(this.dir, toHandlePos);
        if(test < 0)
        {
            toHandlePos.Normalize();
            colPos = this.pos + toHandlePos * radius_near;
        }else
        {
            UtilGS9.LineSegment3 line1 = new LineSegment3(this.pos, nearToPos);
            UtilGS9.LineSegment3 line2 = new LineSegment3(this.pos + dirRight * radius_near, farPos + dirRight * radius_far);
            Vector3 pt0, pt1;
            UtilGS9.LineSegment3.ClosestPoints(out pt0, out pt1, line1, line2);

            colPos = pt0;    

            //먼 원안에 pt0이 포함되는지 검사한다 
            if((farPos - pt0).sqrMagnitude <= radius_far*radius_far)
            {
                //toHandlePos.Normalize();
                //colPos = farPos + toHandlePos * radius_far;
            }
        }


        //먼 반구 충돌위치 찾기 
        //test = Vector3.Dot(-this.dir, nearToPos - farPos);
        //if (test > 0)
        //{
        //}



        return colPos;
    }


    public static void DrawCylinder(Cylinder cld)
    {
        Vector3 farPos = cld.pos + cld.dir * cld.length;
        Vector3 dirRight = Vector3.Cross(Vector3.up, cld.dir);
        dirRight.Normalize();

        DebugWide.DrawLine(cld.pos, farPos, Color.green);
        DebugWide.DrawLine(cld.pos + dirRight * cld.radius_near, farPos + dirRight * cld.radius_far, Color.green);
        DebugWide.DrawLine(cld.pos + -dirRight * cld.radius_near, farPos + -dirRight * cld.radius_far, Color.green);
        DebugWide.DrawCircle(cld.pos, cld.radius_near, Color.green);
        DebugWide.DrawCircle(farPos, cld.radius_far, Color.green);
    }

    public override string ToString()
    {

        return "pos: " + pos + "  dir: " + dir + "  length: " + length
        + "  radius_near: " + radius_near + "  radius_far: " + radius_far;
    }
}