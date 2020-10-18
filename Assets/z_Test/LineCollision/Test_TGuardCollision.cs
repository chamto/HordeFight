﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


public class Test_TGuardCollision : MonoBehaviour 
{
    private MovingModel _movingModel = new MovingModel();
    public float __RateAtoB = 0.5f;
    public bool _allowFixed_a = true;
    public bool _allowFixed_b = true;
    public float __radius_A = 0.1f;
    public float __radius_B = 0.1f;
    public float __angle = 5f;
    public int __c2 = 10;
    int __count = 0;
    float __sign = 1f;
	private void OnDrawGizmos()
	{
        if (false == _movingModel.__init) return;

        __count++;


        if(0 == __count%__c2)
        {
            __sign *= -1f;
        }
        _movingModel._frame_A._tr_frame.rotation *= Quaternion.AngleAxis(__sign * __angle, ConstV.v3_up);

        _movingModel._rateAtoB = __RateAtoB;
        _movingModel._allowFixed_a = _allowFixed_a;
        _movingModel._allowFixed_b = _allowFixed_b;
        _movingModel._frame_A._info[0].radius = __radius_A;
        _movingModel._frame_B._info[0].radius = __radius_B;
        _movingModel.Update();

        _movingModel.Draw();
	}

	private void Start()
	{

        Transform model = Hierarchy.GetTransform(transform.parent, "model");
        Transform frame_A , frame_B;
        frame_A = Hierarchy.GetTransform(model, "frame_0");
        frame_B = Hierarchy.GetTransform(model, "frame_1");

        _movingModel.Init(frame_A, frame_B);
	}

	private void Update()
	{
        //_movingModel.__RateAtoB = __RateAtoB;
        //_movingModel.Update();
	}
}
