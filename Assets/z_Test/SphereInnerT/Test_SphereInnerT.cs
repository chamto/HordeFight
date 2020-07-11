using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using System;

public class Test_SphereInnerT : MonoBehaviour 
{

    public Transform _T_0 = null;
    public Transform _root_start = null;
    public Transform _root_end = null;
    public Transform _sub_start = null;
    public Transform _sub_end = null;

    public Transform _seg0_start = null;
    public Transform _seg0_end = null;

    public Transform _seg1_start = null;
    public Transform _seg1_end = null;

    private bool _init = false;

    private void OnDrawGizmos()
    {
        if (false == _init) return;

        float radius;
        radius = (_root_start.position - _seg0_end.position).magnitude;
        DebugWide.DrawCircle(_root_start.position, radius, Color.gray);

        DebugWide.DrawLine(_root_start.position, _sub_start.position, Color.gray);
        radius = Vector3.Magnitude(_root_start.position - _sub_start.position);
        DebugWide.DrawCircle(_root_start.position, radius, Color.gray);

        radius = (_seg0_start.position - _seg0_end.position).magnitude;
        DebugWide.DrawCircle(_seg0_start.position, radius, Color.gray);
        DebugWide.DrawCircle(_seg0_start.position, 0.1f, Color.gray);
        DebugWide.DrawLine(_seg0_start.position, _seg0_end.position, Color.yellow); 
        DebugWide.DrawLine(_seg1_start.position, _seg1_end.position, Color.green); 

        DebugWide.DrawLine(_root_start.position, _root_end.position, Color.magenta);
        DebugWide.DrawLine(_sub_start.position, _sub_end.position, Color.magenta);


    }

	// Use this for initialization
	void Start () 
    {
        _init = true;
        Transform root, sub, seg0 , seg1;
        _T_0 = Hierarchy.GetTransform(null, "T_0");	
        root = Hierarchy.GetTransform(_T_0, "root");
        sub = Hierarchy.GetTransform(root, "sub_0");
        seg0 = Hierarchy.GetTransform(null, "seg_0");
        seg1 = Hierarchy.GetTransform(null, "seg_1");

        _root_start = Hierarchy.GetTransform(root, "start");
        _root_end = Hierarchy.GetTransform(root, "end");

        _sub_start = Hierarchy.GetTransform(sub, "start");
        _sub_end = Hierarchy.GetTransform(sub, "end");

        _seg0_start = Hierarchy.GetTransform(seg0, "start");
        _seg0_end = Hierarchy.GetTransform(seg0, "end");
        _seg1_start = Hierarchy.GetTransform(seg1, "start");
        _seg1_end = Hierarchy.GetTransform(seg1, "end");
	}
	
	// Update is called once per frame
	void Update () 
    {
        //1# root_start , seg_start 사이의 거리 
        LineSegment3 ls_AB = new LineSegment3(_seg0_start.position, _root_start.position);
        float c = (_root_start.position - _seg0_start.position).magnitude;

        //2# root_start , sub 선분의 접촉점 사이의 거리 
        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(_sub_start.position, _sub_end.position);
        LineSegment3 ls_seg0 = new LineSegment3(_seg0_start.position, _seg0_end.position);
        LineSegment3.ClosestPoints(out pt_min, out pt_max,ls_sub , ls_seg0);

        float a = (_root_start.position - pt_min).magnitude;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        //코사인 제2법칙 이용
        LineSegment3 ls_seg1 = new LineSegment3(_seg1_start.position, _seg1_end.position);
        Vector3 dir_Seg1_AB = Vector3.Cross(ls_seg1.direction, ls_AB.direction);
        float cosA = UtilGS9.Geo.AngleSigned(ls_seg1.direction, ls_AB.direction, dir_Seg1_AB);
        cosA = (float)Math.Cos(cosA * Mathf.Deg2Rad);

        float dt1 = -2f * c * cosA;
        float dt2 = c * c - a * a;

        //이차방정식의 근의공식 이용
        float root = (float)Math.Sqrt(dt1 * dt1 - 4 * dt2);
        float b_plus = (-dt1 + root) / 2f;
        float b_minus = (-dt1 - root) / 2f;

        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b+ : " + b_plus + "  b- : " + b_minus);
	}
}
