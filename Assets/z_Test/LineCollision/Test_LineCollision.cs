using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test_LineCollision : MonoBehaviour 
{

    private Transform _line0_start = null;
    private Transform _line0_end = null;
    private Transform _ts0 = null;

    private Transform _line1_start = null;
    private Transform _line1_end = null;
    private Transform _ts1 = null;

    private void OnDrawGizmos()
    {
        if (null == (object)_line0_start)
            return;

        {
            DebugWide.DrawLine(_line0_start.position, _line0_end.position, Color.red);

            DebugWide.DrawLine(_line1_start.position, _line1_end.position, Color.green);

            DebugWide.DrawLine(_ts0.position, _ts1.position, Color.magenta);
        }

    }

	// Use this for initialization
	void Start () 
    {
        HierarchyPreLoader hierarchy = CSingleton<HierarchyPreLoader>.Instance;
        hierarchy.Init(); //계층도 읽어들이기 	

        Transform obj = hierarchy.GetTransform(transform.parent, "obj");
        //DebugWide.LogBlue(transform.parent + "  :  "+ _obj);
        Transform line_0 = hierarchy.GetTransform(obj, "line_0");
        _line0_start = hierarchy.GetTransform(line_0, "start");
        _line0_end = hierarchy.GetTransform(line_0, "end");
        _ts0 = hierarchy.GetTransform(line_0, "ts");

        Transform line_1 = hierarchy.GetTransform(obj, "line_1");
        _line1_start = hierarchy.GetTransform(line_1, "start");
        _line1_end = hierarchy.GetTransform(line_1, "end");
        _ts1 = hierarchy.GetTransform(line_1, "ts");

	}
	
	// Update is called once per frame
	void Update () 
    {
        float s, t;
        LineSegment3 unit0 = new LineSegment3(_line0_start.position, _line0_end.position);
        LineSegment3 unit1 = new LineSegment3(_line1_start.position, _line1_end.position);

        float sqrdis = LineSegment3.DistanceSquared(unit0, unit1, out s, out t);
        _ts0.position = unit0.direction * s + _line0_start.position;
        _ts1.position = unit1.direction * t + _line1_start.position;

        if (sqrdis < 0.01f)
        {
            DebugWide.LogBlue("Collision!!" + "   s:" + s + "  t:" + t);
        }
	}

}
