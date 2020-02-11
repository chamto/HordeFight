﻿using System.Collections;
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
    public float _line1_width = 0.001f;
    private Geo.Model _model_1 = new Geo.Model();

    private void OnDrawGizmos()
    {
        if (null == (object)_line0_start)
            return;

        {
            float len;
            DebugWide.DrawLine(_line0_start.position, _line0_end.position, Color.red);
            len = (_line0_start.position - _line0_end.position).magnitude;
            DebugWide.DrawCirclePlane(_line0_start.position,len,Vector3.up, Color.red);
            DebugWide.DrawCircle(_line0_start.position, 0.1f, Color.red);

            //DebugWide.DrawLine(_line1_start.position, _line1_end.position, Color.green);
            //len = (_line1_start.position - _line1_end.position).magnitude;
            //DebugWide.DrawCirclePlane(_line1_start.position,len,Vector3.up, Color.green);
            //DebugWide.DrawCircle(_line1_start.position, 0.1f, Color.green);


            DebugWide.DrawLine(_ts0.position, _ts1.position, Color.magenta);

            //===============

            _model_1.cylinder.Draw(Color.green);

            //===============

            UtilGS9.Plane plane = new UtilGS9.Plane(_line0_start.position, _line0_end.position, _line1_start.position);
            plane.Draw(5,Color.gray);
        }

    }

	// Use this for initialization
	void Start () 
    {
        Transform obj = Hierarchy.GetTransform(transform.parent, "obj");
        //DebugWide.LogBlue(transform.parent + "  :  "+ _obj);
        Transform line_0 = Hierarchy.GetTransform(obj, "line_0");
        _line0_start = Hierarchy.GetTransform(line_0, "start");
        _line0_end = Hierarchy.GetTransform(line_0, "end");
        _ts0 = Hierarchy.GetTransform(line_0, "ts");

        Transform line_1 = Hierarchy.GetTransform(obj, "line_1");
        _line1_start = Hierarchy.GetTransform(line_1, "start");
        _line1_end = Hierarchy.GetTransform(line_1, "end");
        _ts1 = Hierarchy.GetTransform(line_1, "ts");

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

        float line1_width = 0.1f;
        if (sqrdis < line1_width*line1_width)
        {
            DebugWide.LogBlue("Collision!!" + "   s:" + s + "  t:" + t);

            float len = (_line0_start.position - _line0_end.position).magnitude;
            //Vector3 newDir = (_line1_end.position - _line0_start.position).normalized;
            Vector3 newDir = (_ts1.position - _line0_start.position).normalized;
            _line0_end.position = _line0_start.position + newDir * len;
        }

        _model_1.cylinder.Set(Vector3.up, _line1_start.position, line1_width, _line1_end.position, line1_width);
	}

}
