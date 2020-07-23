using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using System;

public class Test_SphereInnerT : MonoBehaviour 
{

    public Transform _T0 = null;
    public Transform _T0_root = null;
    public Transform _T0_root_start = null;
    public Transform _T0_root_end = null;
    public Transform _T0_sub_start = null;
    public Transform _T0_sub_end = null;

    public Transform _T1 = null;
    public Transform _T1_root = null;
    public Transform _T1_root_start = null;
    public Transform _T1_root_end = null;
    public Transform _T1_sub_start = null;
    public Transform _T1_sub_end = null;

    public Transform _seg0_start = null;
    public Transform _seg0_end = null;

    public Transform _seg1_start = null;
    public Transform _seg1_end = null;

    private bool _init = false;

    private void OnDrawGizmos()
    {
        if (false == _init) return;

        float radius;
        radius = (_T0_root_start.position - _seg0_end.position).magnitude;
        DebugWide.DrawCircle(_T0_root_start.position, radius, Color.gray);

        DebugWide.DrawLine(_T0_root_start.position, _T0_sub_start.position, Color.gray);
        radius = Vector3.Magnitude(_T0_root_start.position - _T0_sub_start.position);
        DebugWide.DrawCircle(_T0_root_start.position, radius, Color.gray);

        radius = (_seg0_start.position - _seg0_end.position).magnitude;
        DebugWide.DrawCircle(_seg0_start.position, radius, Color.gray);
        DebugWide.DrawCircle(_seg0_start.position, 0.1f, Color.gray);
        DebugWide.DrawLine(_seg0_start.position, _seg0_end.position, Color.yellow); 
        DebugWide.DrawLine(_seg1_start.position, _seg1_end.position, Color.green); 


        //-----------------

        DebugWide.DrawLine(_T0_root_start.position, _T0_root_end.position, Color.magenta);
        DebugWide.DrawLine(_T0_sub_start.position, _T0_sub_end.position, Color.magenta);

        //-----------------

        DebugWide.DrawLine(_T1_root_start.position, _T1_root_end.position, Color.blue);
        DebugWide.DrawLine(_T1_sub_start.position, _T1_sub_end.position, Color.blue);

        //-----------------

        Vector3 n_seg1 = (_seg1_end.position - _seg1_start.position).normalized;
        Vector3 b_pos = __mt_1;
        //Vector3 b_pos = _seg1_start.position + n_seg1 * __b_plus;
        DebugWide.DrawLine(_seg1_start.position, b_pos, Color.red);
        DebugWide.DrawLine(_T0_root_start.position, b_pos, Color.red);
        DebugWide.DrawLine(_T0_root_start.position, _seg0_start.position, Color.red);

        //----------------



    }

	// Use this for initialization
	void Start () 
    {
        _init = true;
        Transform t0sub, t1sub, seg0 , seg1;
        _T0 = Hierarchy.GetTransform(null, "T_0");	
        _T0_root = Hierarchy.GetTransform(_T0, "root");
        t0sub = Hierarchy.GetTransform(_T0_root, "sub_0");
        _T0_root_start = Hierarchy.GetTransform(_T0_root, "start");
        _T0_root_end = Hierarchy.GetTransform(_T0_root, "end");
        _T0_sub_start = Hierarchy.GetTransform(t0sub, "start");
        _T0_sub_end = Hierarchy.GetTransform(t0sub, "end");

        _T1 = Hierarchy.GetTransform(null, "T_1");
        _T1_root = Hierarchy.GetTransform(_T1, "root");
        t1sub = Hierarchy.GetTransform(_T1_root, "sub_0");
        _T1_root_start = Hierarchy.GetTransform(_T1_root, "start");
        _T1_root_end = Hierarchy.GetTransform(_T1_root, "end");
        _T1_sub_start = Hierarchy.GetTransform(t1sub, "start");
        _T1_sub_end = Hierarchy.GetTransform(t1sub, "end");

        //------------

        seg0 = Hierarchy.GetTransform(null, "seg_0");
        seg1 = Hierarchy.GetTransform(null, "seg_1");

        _seg0_start = Hierarchy.GetTransform(seg0, "start");
        _seg0_end = Hierarchy.GetTransform(seg0, "end");
        _seg1_start = Hierarchy.GetTransform(seg1, "start");
        _seg1_end = Hierarchy.GetTransform(seg1, "end");
	}
	
    //T가드의 최대이동 제약이 있는 알고리즘 
    void Update()
    {
        //1# root_start , seg_start 사이의 거리 
        LineSegment3 ls_AB = new LineSegment3(_seg0_start.position, _T0_root_start.position);
        float c = (_T0_root_start.position - _seg0_start.position).magnitude;

        //2# root_start , sub 선분의 접촉점 사이의 거리 
        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(_T0_sub_start.position, _T0_sub_end.position);
        LineSegment3 ls_seg0 = new LineSegment3(_seg0_start.position, _seg0_end.position);
        LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_sub, ls_seg0);

        float a = (_T0_root_start.position - pt_min).magnitude;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        //코사인 제2법칙 이용
        LineSegment3 ls_seg1 = new LineSegment3(_seg1_start.position, _seg1_end.position);
        Vector3 up_seg1_AB = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        float cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_seg1_AB);
        cosA = (float)Math.Cos(cosA * Mathf.Deg2Rad);

        float dt1 = -2f * c * cosA;
        //float dt2 = c * c - a * a;

        //이차방정식의 근의공식 이용
        float root = 0;
        float b_1 = (-dt1 + root) / 2f;

        //코사인 제2법칙으로 cosA를 다시 구함 
        cosA = (-(a * a) + (b_1 * b_1) + (c * c)) / (2 * b_1 * c);
        float new_angle = (float)Math.Acos(cosA);
        new_angle = new_angle * Mathf.Rad2Deg;
        Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(new_angle, up_seg1_AB) * ls_AB.direction;


        __mt_0 = pt_min;
        //__mt_1 = new_dir_ls_seg1; //길이가 b_1 이 아니기 때문에 다른 결과가 나온다 
        __mt_1 = ls_seg1.origin + new_dir_ls_seg1.normalized * b_1; 


        //=======================

        Vector3 dir_root = _T0_root_end.position - _T0_root_start.position;
        Vector3 up_t = Vector3.Cross(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position);
        float angle_t = Geo.AngleSigned(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position, up_t);

        _T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * _T0_root.rotation;

        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b_one : " + b_1 + "  new_angle : " + new_angle + "  cosA :" + cosA);

    }

    //T가드의 최대이동 제약이 없는 알고리즘 
	void Update_0 () 
    {
        //1# root_start , seg_start 사이의 거리 
        LineSegment3 ls_AB = new LineSegment3(_seg0_start.position, _T0_root_start.position);
        float c = (_T0_root_start.position - _seg0_start.position).magnitude;

        //2# root_start , sub 선분의 접촉점 사이의 거리 
        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(_T0_sub_start.position, _T0_sub_end.position);
        LineSegment3 ls_seg0 = new LineSegment3(_seg0_start.position, _seg0_end.position);
        LineSegment3.ClosestPoints(out pt_min, out pt_max,ls_sub , ls_seg0);

        float a = (_T0_root_start.position - pt_min).magnitude;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        //코사인 제2법칙 이용
        LineSegment3 ls_seg1 = new LineSegment3(_seg1_start.position, _seg1_end.position);
        Vector3 up_seg1_AB = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        float cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_seg1_AB);
        cosA = (float)Math.Cos(cosA * Mathf.Deg2Rad);

        float dt1 = -2f * c * cosA;
        float dt2 = c * c - a * a;

        //이차방정식의 근의공식 이용
        float root = (float)Math.Sqrt(dt1 * dt1 - 4 * dt2);
        float b_plus = (-dt1 + root) / 2f;
        float b_minus = (-dt1 - root) / 2f;

        //__b_plus = b_plus;
        //__b_minus = b_minus;

        __mt_0 = pt_min;
        __mt_1 = ls_seg1.origin + ls_seg1.direction.normalized * b_minus; //임시 , 플러스인 경우도 있음 

        //=======================

        Vector3 dir_root = _T0_root_end.position - _T0_root_start.position;
        Vector3 up_t = Vector3.Cross(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position);
        float angle_t = Geo.AngleSigned(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position, up_t);
        //Vector3 new_dir_root = Quaternion.AngleAxis(angle_t,up_t) * dir_root;
        //_T1_root.rotation = _T0_root.rotation * Quaternion.AngleAxis(angle_t, up_t); //부정확한 값이 나옴  
        _T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * _T0_root.rotation;  //정확한 값이 나옴 
        //부모회전행렬을 오른쪽에 놓는것이 고정된 식인지 "행/열우선 행렬"에 따라 곱하는 순서가 변화되어야 하는지 모르겠음. 다음에 분석해보기 - chamto 20200720
        //ref : https://m.blog.naver.com/PostView.nhn?blogId=sipack7297&logNo=220428447625&proxyReferer=https:%2F%2Fwww.google.com%2F


        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b+ : " + b_plus + "  b- : " + b_minus + "  angle_t : " + angle_t);

	}
    //float __b_plus = 0, __b_minus = 0;
    Vector3 __mt_0, __mt_1;
}
