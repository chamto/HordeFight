using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


public class Test_TGuardCollision : MonoBehaviour 
{
    private MovingModel _movingModel = new MovingModel();
    public float __RateAtoB = 0.5f;
    //public float __radius_A = 0.1f;
    //public float __radius_B = 0.1f;

	private void OnDrawGizmos()
	{
        _movingModel.__RateAtoB = __RateAtoB;
        //_movingModel.__radius_A = __radius_A;
        //_movingModel.__radius_B = __radius_B;
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
