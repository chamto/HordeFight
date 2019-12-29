using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TestCombineRotate : MonoBehaviour 
{
	public Transform _testTarget = null;
	public Transform _testAxis = null;
	public string _multiOrder = "s(x y z) rz0 ry0 rx0 t(x y z)";

	public bool _repeat = false;
	public bool _apply = false;

	private TRSParser _parser = new TRSParser();



	void Start () 
	{
		Matrix4x4 mrx = Matrix4x4.identity;
		Matrix4x4 mry = Matrix4x4.identity;
		Matrix4x4 mrz = Matrix4x4.identity;
		Matrix4x4 mt = Matrix4x4.identity;
		Matrix4x4 r = Matrix4x4.zero;

		mrx = TRSHelper.GetRotateX (45f);
		mry = TRSHelper.GetRotateY (45f);
		mrz = TRSHelper.GetRotateY (0f);
		mt = TRSHelper.GetTranslate(new Vector3(0,0,0));

		//r = mrx * mry * mrz * mt;
		r = mt* mrx * mry * mrz;
		//r = mrx * mry * mrz;



		IvQuat quat = new IvQuat ();
		quat.Set (0f, 45f, 45f);
		r = quat.GetMatrix ();
		//this.ApplyMatrixToGroupPosition (_testAxis, r);
		//this.ApplyMatrixToGroupPosition (_testAxis, r);


		//_testTarget.position = r.MultiplyPoint (_testTarget.position);
		//_testTarget.position = mt.MultiplyPoint (_testTarget.position);

		//------------------------------------------------------------
		//  float 비트 배열 분석 
		//------------------------------------------------------------
//		bool bigIndian = true;
//		DebugWide.LogBlue(ML.Util.ToBit(10.25f) + ": 10.25f");
//		DebugWide.LogBlue(ML.Util.ToBit(1f,bigIndian) + ": 1f ");
//		DebugWide.LogBlue(ML.Util.ToBit(-1f,bigIndian) + ": -1f ");
//		DebugWide.LogBlue(ML.Util.ToBit(2f,bigIndian) + ": 2f ");
//		DebugWide.LogBlue(ML.Util.ToBit(-2f,bigIndian) + ": -2f ");
//		DebugWide.LogBlue(ML.Util.ToBit(3f,bigIndian) + ": 3f ");
//		DebugWide.LogBlue(ML.Util.ToBit(-3f,bigIndian) + ": -3f ");
//		DebugWide.LogBlue(ML.Util.ToBit(4f,bigIndian) + ": 4f ");
//		DebugWide.LogBlue(ML.Util.ToBit(-4f,bigIndian) + ": -4f ");
//		DebugWide.LogBlue(ML.Util.ToBit(5f,bigIndian) + ": 5f ");
//		DebugWide.LogBlue(ML.Util.ToBit(-5f,bigIndian) + ": -5f ");
//		DebugWide.LogBlue(ML.Util.ToBit(6f,bigIndian) + ": 6f ");
//		DebugWide.LogBlue(ML.Util.ToBit(-6f,bigIndian) + ": -6f ");
//		DebugWide.LogBlue(ML.Util.ToBit(float.NaN) + ": Nan");
//		DebugWide.LogBlue(ML.Util.ToBit(float.Epsilon) + ": Epsilon " + float.Epsilon);
//		DebugWide.LogBlue(ML.Util.ToBit(float.PositiveInfinity) + ": PositiveInfinity");
//		DebugWide.LogBlue(ML.Util.ToBit(float.NegativeInfinity) + ": NegativeInfinity");
//
//
//		float a = 1.00000011920929f; //1 + 1.0 * 1/(2^23)
//		float b = 1.0f - a;
//		DebugWide.LogBlue (Mathf.Log (8388608, 2)); //Log2^8388608 = 23
//
//		DebugWide.LogBlue(ML.Util.ToBit(a) + ": a1 : " + a + " sqrt : " + Mathf.Sqrt(b));
//		//a = 1.00000011111f; 
//		//a = 1.0000001f;
//		//a = 1.000001f;
//
//		if(float.Epsilon < Mathf.Abs(b))
//			DebugWide.LogBlue(ML.Util.ToBit(b) + ": a2 : " + a);
//
//		DebugWide.LogBlue(ML.Util.ToBit(b) + ": b3 : " + b);





		this.SavePosition (_testAxis);
	}


	void Update () 
	{
		if (true == _apply) 
		{
			
			Matrix4x4 trs = TRSHelper.ParsingMatrix(_parser , _multiOrder);

			//-----------
			//1. 비행기모양으로 배치된 객체에 적용 : _testTarget
			prev_angles = dest_angles;
			//1. 우주선모양으로 배치된 객체에 적용 : _testTarget
			//this.ApplyMatrixToTransform(_testTarget, trs);
			_testTarget.localRotation = TRSHelper.GetQuaternion(_parser); //chamto test - Iv쿼터니언 바로 적용 

			dest_angles = _testTarget.eulerAngles;
			DebugWide.LogBlue ("2: unity angles : "+_testTarget.eulerAngles + "\n"); //chamto test

			//1. 좌표계모양으로 배치된 객체에 적용 : _testAxis
			this.ApplyMatrixToGroupPosition (_testAxis, trs);
			//-----------

			elapsedTime = 0;
			_apply = false;
		}

		elapsedTime += Time.deltaTime;

		this.Repeat ();

	}

	float elapsedTime = 0;
	Vector3 prev_angles = Vector3.zero;
	Vector3 dest_angles = Vector3.zero;
	public void Repeat()
	{
		//interpolation
		if (true == _repeat)
		{
			elapsedTime = Mathf.Repeat (elapsedTime, 2f);

			//0~1
			if(0 <= elapsedTime && elapsedTime <= 1f)
				_testTarget.eulerAngles = Vector3.Lerp (dest_angles, prev_angles, elapsedTime);
			//1~2
			else if(1f < elapsedTime && elapsedTime <= 2f)
				_testTarget.eulerAngles = Vector3.Lerp (prev_angles, dest_angles, elapsedTime -1f);
		}
		else
			_testTarget.eulerAngles = Vector3.Lerp (prev_angles, dest_angles, elapsedTime);
		
	}






	//행렬을 unityTransform에 적용한다
	//ref : http://answers.unity3d.com/questions/1134216/how-to-set-transformation-matrices-of-transform.html
	public void ApplyMatrixToTransform(Transform tr, Matrix4x4 mat)
	{
		tr.localPosition = mat.GetColumn( 3 );
		tr.localScale = new Vector3( mat.GetColumn( 0 ).magnitude, mat.GetColumn( 1 ).magnitude, mat.GetColumn( 2 ).magnitude );

		float w = Mathf.Sqrt( 1.0f + mat.m00 + mat.m11 + mat.m22 ) / 2.0f;
		tr.localRotation = new Quaternion( ( mat.m21 - mat.m12 ) / ( 4.0f * w ), ( mat.m02 - mat.m20 ) / ( 4.0f * w ), ( mat.m10 - mat.m01 ) / ( 4.0f * w ), w );
	}

	//그룹 전체 tr에 행렬을 곱한다. 
	public void ApplyMatrixToGroupPosition(Transform groupTr, Matrix4x4 mat)
	{
		if (null == groupTr)
			return;

		this.LoadPosition (groupTr);
		foreach (Transform tr in groupTr.GetComponentsInChildren<Transform> ()) 
		{
			tr.position = mat.MultiplyPoint (tr.position); //열우선 행렬 곱
			//tr.position = this.MultiplyFirstRow(tr.position, mat); //행우선 행렬 곱
		}
	}

	//행우선 행렬곱 : 행우선 행렬곱을 테스트하기 위해 만듬
	//=> 유니티는 열우선 행렬곱을 한다. 
	public Vector4 MultiplyFirstRow(Vector4 p_left ,Matrix4x4 m_right )
	{
		Vector4 result;
		result.x = Vector4.Dot (m_right.GetColumn (0), p_left);
		result.y = Vector4.Dot (m_right.GetColumn (1), p_left);
		result.z = Vector4.Dot (m_right.GetColumn (2), p_left);
		result.w = Vector4.Dot (m_right.GetColumn (3), p_left);

		return result;
	}


	//그룹 tr의 초기 월드포지션을 저장한다. 
	Dictionary<Transform ,Vector3> savePositions = new Dictionary<Transform ,Vector3> ();
	public void SavePosition(Transform groupTr)
	{
		savePositions.Clear ();
		foreach (Transform tr in groupTr.GetComponentsInChildren<Transform> ()) 
		{
			savePositions.Add (tr,tr.position);
		}
	}

	//변경된 그룹 tr의 월드포지션을 초기 월드포지션으로 바꾼다.
	public void LoadPosition(Transform groupTr)
	{
		foreach (KeyValuePair<Transform, Vector3> p in savePositions) 
		{
			p.Key.position = p.Value;
		}
	}


	//----------------------






}
