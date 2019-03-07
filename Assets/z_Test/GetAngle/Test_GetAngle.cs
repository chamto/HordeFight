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
        Misc.Init();

        //PrintErrorRate_V2Length();

        Create_LookUpTable_TrigonometricFunction();
	}
	
	// Update is called once per frame
	void Update () 
    {

        //각도 구하는 함수 성능 테스트 
        //float angle = 0;
        //for (int i = 0; i < 10000; i++)
        //{
        //    //angle = GetSignedAngle_Normalize(_line_0.position, _line_1.position, Vector3.back); //10000번  45.57ms
        //    //angle += GetSignedAngle_Atan2_AxisZ(_line_0.position, _line_1.position); //10000번 19.97ms
        //    //angle += GetSignedAngle_Vector3(_line_0.position, _line_1.position, Vector3.back); //10000번 55.71ms
        //}


        //벡터 길이 구하는 함수 성능 테스트 
        //Vector3 pos = _line_1.position;
        //for (int i = 0; i < 10000; i++)
        //{
        //    float d0 = GetV2Length_AxisY_WithError(pos); //10000번 1.15ms
        //    float d1 = GetV2Length_AxisY(pos); //10000번 1.46ms
        //    float d2 = pos.magnitude; //10000번 2.99ms
        //}


        //단위벡터 구하는 함수 성능 테스트 
        //Vector3 pos = _line_0.position;
        //for (int i = 0; i < 10000; i++)
        //{
        //    //_line_1.position = Misc.TestGetDir_Normal3D_AxisY(pos); //10000번  11.84ms
        //   Misc.GetDir64_Normal3D(pos); //10000번  5.14ms
        //    Vector3 n = pos.normalized; //10000번  11.13ms

        //    //DebugWide.LogBlue(i*5 + "도   :" + Mathf.Tan(Mathf.Deg2Rad * i * 5));
        //}


        //삼각함수 성능 테스트
        //for (int i = 0; i < 10000; i++)
        //{
        //    int angle = i % 360;
        //    //Mathf.Cos(i); 
        //    Mathf.Sin(angle); 
        //    //Cos(i);
        //    Sin(angle); //느리다 ..
        //}

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

        temp = ConstV.STRING_EMPTY + GetAngle_Atan2_AxisY(_line_0.position, _line_1.position );
        DebugWide.PrintText(ConstV.v3_zero, Color.blue, temp);

        //벡터 길이 근사값 테스트 
        temp = ConstV.STRING_EMPTY + "  근사치:" +GetV2Length_AxisY(_line_0.position) + "   정확한값:" + _line_0.position.magnitude;
        DebugWide.PrintText(_line_0.position, Color.blue, temp);

    }

    //ref : https://libsora.so/posts/angle-and-sine-doom-version/
    private float[] _lookUpTable_sin = null;
    const int _lookUpTable_precision = 10;
    public void Create_LookUpTable_TrigonometricFunction()
    {
        //int precision = 1000; //소수점 셋째 자리까지 표현  
        int size_tableSin = _lookUpTable_precision * (360);
        _lookUpTable_sin = new float[size_tableSin];
        for (int i = 0; i < size_tableSin; i++)
        {
            float toAngle = (float)i / _lookUpTable_precision; //인덱스를 각도로 변환 
            _lookUpTable_sin[i] = Mathf.Sin(Mathf.Deg2Rad * toAngle);
        }
    }


    //Mathf.Sin 보다 느리다.. 쓰지 말자 
    //ref : http://bbs.nicklib.com/algorithm/2005
	float Sin(float degree)
	{
        while((degree) < ((int)0))   (degree) += (int)360;
        while((degree) >= ((int)360))  (degree) -= (int)360; 

        return _lookUpTable_sin[ (int)(degree * _lookUpTable_precision) ];
	}

    float Cos(float degree)
    {
        return Sin(90f + degree);
    }

	//======================================================

	//벡터의 길이 근사치 구하는 함수의 오차율을 출력한다 
	//!! x 와 z축의 길이가 같을때 최대 오차율이 나온다 
	//!! 항상 근사치가 정확한값 보다 오차율 만큼 크다 
	//!! 근사치에서 오차량이 차지하는 비율은  항상 5.179096 퍼센트이다 (1차 오차비율) 
	//!! 1차 오차비율값을 적용하면 2차 비율이 나온다. 항상 0.5694969 퍼센트이다 (2차 오차비율) 
	//!! 2차 오차비율값을 적용하면 3차 오차 비율이 0이 나온다. 
	public void PrintErrorRate_V2Length()
    {
        Vector3 dst = ConstV.v3_zero;
        string tt = "";

        for (int i = 0; i < 30;i++)
        {
            dst.x = i * 10;
            dst.z = i * 10;
            float approximate = GetV2Length_AxisY_WithError(dst);
            float accurate = dst.magnitude;
            float errorAmount = approximate - accurate;
            float percent = errorAmount / approximate; // 오차량/근사치 = 오차비율
            float approximate2 = approximate - (approximate * percent);
            tt = "  근사치:" + approximate + "   정확한값:" + accurate + "   오차량:" + errorAmount + "  오차량/근사치:" + percent * 100f + "%   오차비율값 적용:" + approximate2;
            DebugWide.LogBlue(tt);
        }
    }


    //ref : https://github.com/id-Software/DOOM/blob/77735c3ff0772609e9c8d29e3ce2ab42ff54d20b/linuxdoom-1.10/p_maputl.c#L43-L58
    //ref : https://libsora.so/posts/vector-length-and-normalize-doom-version/
    //월드축에서만 사용할 수 있는 방법. 근사치를 구한다 
    //Vector3.magnitude 함수 보다 빠르다. 정확도 5%정도 떨어진다 
    static  float GetV2Length_AxisZ_WithError(Vector3 v0)
    {
        float x = v0.x >= 0 ? v0.x : v0.x * -1f;
        float y = v0.y >= 0 ? v0.y : v0.y * -1f;

        if (x > y)
            return x + y * 0.5f;

        return y + x * 0.5f;
    }


    static public float GetV2Length_AxisY_WithError(Vector3 v0)
    {
        float x = v0.x >= 0 ? v0.x : v0.x * -1f;
        float z = v0.z >= 0 ? v0.z : v0.z * -1f;

        if (x > z)
            return x + z * 0.5f;

        return z + x * 0.5f;
    }

    //Vector3.magnitude 함수 보다 빠르다. magnitude 와 동일한 값을 계산한다 
    static public float GetV2Length_AxisY(Vector3 v0)
    {
        const float ERROR_RATE_1STEP = 0.05179096f; //오차량 / 근사치 = 1차 오차비율
        const float ERROR_RATE_2STEP = 0.005694969f; //2차 오차비율 

        float x = v0.x >= 0 ? v0.x : v0.x * -1f;
        float z = v0.z >= 0 ? v0.z : v0.z * -1f;
        float approximate = 0f;

        if (x > z)
            approximate = x + z * 0.5f;
        else
            approximate = z + x * 0.5f;

        approximate -= approximate * ERROR_RATE_1STEP;
        approximate -= approximate * ERROR_RATE_2STEP;

        return approximate;
    }

    static public float GetV2Length_AxisZ(Vector3 v0)
    {
        const float ERROR_RATE_1STEP = 0.05179096f; //오차량 / 근사치 = 1차 오차비율
        const float ERROR_RATE_2STEP = 0.005694969f; //2차 오차비율 

        float x = v0.x >= 0 ? v0.x : v0.x * -1f;
        float y = v0.y >= 0 ? v0.y : v0.y * -1f;
        float approximate = 0f;

        if (x > y)
            approximate = x + y * 0.5f;
        else
            approximate = y + x * 0.5f;

        approximate -= approximate * ERROR_RATE_1STEP;
        approximate -= approximate * ERROR_RATE_2STEP;

        return approximate;
    }

    //======================================================

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

    public static float GetSignedAngle_Atan2_AxisY(Vector3 v0, Vector3 v1)
    {
        float at0 = Mathf.Atan2(v0.z, v0.x);
        float at1 = Mathf.Atan2(v1.z, v1.x);

        return (at0 - at1) * Mathf.Rad2Deg;
    }

    public static float GetAngle_Atan2_AxisY(Vector3 v0, Vector3 v1)
    {
        float at0 = Mathf.Atan2(v0.z, v0.x);
        float at1 = Mathf.Atan2(v1.z, v1.x);
        float rad = at0 - at1;

        if (rad < 0) rad *= -1; //부호를 없앤다 

        return rad * Mathf.Rad2Deg;
    }


    public static float GetSignedAngle_Vector3(Vector3 v0, Vector3 v1 , Vector3 axis)
    {
        return Vector3.SignedAngle(v0, v1, axis);
    }

    //======================================================
}
