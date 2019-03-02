using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test_GetAngle : MonoBehaviour 
{

    public Transform _line_0 = null;
    public Transform _line_1 = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        float angle = 0;
        //for (int i = 0; i < 10000; i++)
        //{
        //    //angle = GetSignedAngle_Normalize(_line_0.position, _line_1.position, Vector3.back); //10000번  45.57ms
        //    //angle += GetSignedAngle_Atan2_AxisZ(_line_0.position, _line_1.position); //10000번 19.97ms
        //    //angle += GetSignedAngle_Vector3(_line_0.position, _line_1.position, Vector3.back); //10000번 55.71ms
        //}

        temp = "" + angle;
	}

    string temp = ConstV.STRING_EMPTY;
    Vector3 tempV3 = ConstV.v3_zero;
    private void OnDrawGizmos()
    {
        DebugWide.DrawCircle(ConstV.v3_zero, 10f, Color.red);

        if (null == _line_0 || null == _line_1) return;

        tempV3 = _line_0.position;
        temp = ConstV.STRING_EMPTY +  tempV3.magnitude;
        DebugWide.DrawLine(ConstV.v3_zero, _line_0.position, Color.blue);
        //DebugWide.PrintText(_line_0.position, Color.blue, temp);


        tempV3 = _line_1.position;
        temp = ConstV.STRING_EMPTY + tempV3.magnitude;
        DebugWide.DrawLine(ConstV.v3_zero, _line_1.position, Color.blue);
        DebugWide.PrintText(_line_1.position, Color.blue, temp);

        temp = ConstV.STRING_EMPTY + GetSignedAngle_Normalize(_line_0.position, _line_1.position , Vector3.back);
        DebugWide.PrintText(ConstV.v3_zero, Color.blue, temp);



    }

    //** 정규화 Normalize 한번 보다 초월함수 atan2 두번 호출하는 것이 빠르다 
    //Vector3.SignedAngle 와 내부 알고리즘 동일. 속도가 조금더 빠르다 
    public static float GetSignedAngle_Normalize(Vector3 v0, Vector3 v1 , Vector3 axis)
    {
        v0.Normalize();
        v1.Normalize();
        float proj = Vector3.Dot(v0, v1);
        Vector3 vxw = Vector3.Cross(v0, v1);

        //스칼라삼중적을 이용하여 최단회전방향을 구한다 
        //float sign = 1f;
        if (Vector3.Dot(axis , vxw) < 0 ) 
            return Mathf.Acos(proj) * Mathf.Rad2Deg * -1f;

        return Mathf.Acos(proj) * Mathf.Rad2Deg;
    }

 
    //속도가 가장 빠름. 월드축에서만 사용 할 수 있다 
    public static float GetSignedAngle_Atan2_AxisZ(Vector3 v0, Vector3 v1)
    {
        float at0 = Mathf.Atan2(v0.y, v0.x);
        float at1 = Mathf.Atan2(v1.y, v1.x);

        return (at0 - at1) * Mathf.Rad2Deg;
    }


    public static float GetSignedAngle_Vector3(Vector3 v0, Vector3 v1 , Vector3 axis)
    {
        return Vector3.SignedAngle(v0, v1, axis);
    }

}
