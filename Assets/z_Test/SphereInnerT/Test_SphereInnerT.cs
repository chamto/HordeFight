using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test_SphereInnerT : MonoBehaviour 
{

    public Transform _T_0 = null;
    public Transform _root_start = null;
    public Transform _root_end = null;
    public Transform _sub_start = null;
    public Transform _sub_end = null;

    public Transform _seg_start = null;
    public Transform _seg_end = null;

    private bool _init = false;

    private void OnDrawGizmos()
    {
        if (false == _init) return;

        float radius;
        radius = (_root_start.position - _seg_end.position).magnitude;
        DebugWide.DrawCircle(_root_start.position, radius, Color.gray);

        DebugWide.DrawLine(_root_start.position, _sub_start.position, Color.gray);
        radius = Vector3.Magnitude(_root_start.position - _sub_start.position);
        DebugWide.DrawCircle(_root_start.position, radius, Color.gray);

        DebugWide.DrawLine(_root_start.position, _root_end.position, Color.magenta); 
        DebugWide.DrawLine(_sub_start.position, _sub_end.position, Color.magenta);

        radius = (_seg_start.position - _seg_end.position).magnitude;
        DebugWide.DrawCircle(_seg_start.position, radius, Color.gray);
        DebugWide.DrawCircle(_seg_start.position, 0.1f, Color.gray);
        DebugWide.DrawLine(_seg_start.position, _seg_end.position, Color.yellow); 


    }

	// Use this for initialization
	void Start () 
    {
        _init = true;
        Transform root, sub, seg;
        _T_0 = Hierarchy.GetTransform(null, "T_0");	
        root = Hierarchy.GetTransform(_T_0, "root");
        sub = Hierarchy.GetTransform(_T_0, "sub_0");
        seg = Hierarchy.GetTransform(null, "seg");

        _root_start = Hierarchy.GetTransform(root, "start");
        _root_end = Hierarchy.GetTransform(root, "end");

        _sub_start = Hierarchy.GetTransform(sub, "start");
        _sub_end = Hierarchy.GetTransform(sub, "end");

        _seg_start = Hierarchy.GetTransform(seg, "start");
        _seg_end = Hierarchy.GetTransform(seg, "end");
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}
}
