using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using System.Runtime.CompilerServices;
using System;
using System.Diagnostics;

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

        //삼각함수 성능 테스트 2 - 잘못 측정한 성능 - 유니티 프로파일러로 정확한 성능을 측정할 수 없다
        //함수안에 함수가 있는경우 프로파일러에 의해 오버헤드가 크게 발생한다. 
        //예)Mathf.Atan2 와 Mathf.Cos 이 둘은 실제성능은 비슷하다. 프로파일러로 재면 의해 Atan2가 과하게 성능이 낮은 것으로 나온
        Vector3 va = _line_0.position;
        Vector3 vb = _line_1.position;
        Vector3 vc;
        //for (int i = 0; i < 10000; i++)
        //{
        //    // * 제곱근함수 < 삼각함수 < 초월함수 < 벡터노멀함수
        //    //예상외로 제곱근함수가 가장빠름
        //    //제곱근함수를 한번 사용하는 노멀함수가 가장느림
        //    //예상외로 초월함수가 가장느림a

        //    float a = Mathf.Atan2((float)i, 5f); //10000번  4.09ms
        //    a = (float)System.Math.Atan2((double)i, 5.0);
        //    a = ATan2((float)i, 5f);
        //    a = FastAtan2((float)i, 5f);
        //    float b = Mathf.Cos((float)i); //10000번  1.65ms
        //    float c = Mathf.Sqrt((float)i); //10000번  1.27ms
        //    float d = Vector3.Dot(_line_0.position , _line_1.position); //10000번  1.1ms

        //    // * My_Normalize < Misc.GetDir64_Normal3D < Vector3.Normalize < Vector3.normalized
        //    //직접 구현한 노멀함수가 가장빠름 
        //    Vector3 vd = _line_0.position;
        //    vd.Normalize(); //10000번  10.61ms
        //    Vector3 ve = vd.normalized; //10000번  12.74ms
        //    Misc.GetDir64_Normal3D(vd); //10000번  6.82ms
        //    My_Normalize(vd); //10000번  6.55ms

        //    //벡터 연산 테스트
            //vc = va * i;
            //vc = va / i;
            //vc = va + vb;
            //vc = va - vb;

            //vc = Operator_Mult(va , i);
            //vc = Operator_Division(va , i);
            //vc = Operator_Plus(va , vb , vc);
            //vc = Operator_Minus(va , vb);
        //}



        //

        //유니티 프로파일러로 정확한 성능을 측정할 수 없다
        //함수안에 함수가 있는경우 프로파일러에 의해 오버헤드가 크게 발생한다. 
        //예)Mathf.Atan2 와 Mathf.Cos 이 둘은 실제성능은 비슷하다. 프로파일러로 재면 의해 Atan2가 과하게 성능이 낮은 것으로 나온다

        //프로파일러를 사용하지 않은 상태에서의 성능측정 (이 정보를 참고하기)
        //정리 : 삼각함수는 유니티것을 쓰지 않느다. 제곱근 구하는 함수가 가장 빠르다 
        //단위벡터는 직접 구현한 함수를 사용한다. 각도구하는 함수는 직접 구현한 것을 사용한다. 벡터사칙연산은 직접 구현한 것을 사용한다. 

        //** 수행횟수 5만번 기준 **
        //1ms Math.Sqrt
        //1.4ms v_mult //벡터연산 곱 
        //1.4ms v_plus //벡터연산 합
        //1.4ms v_minu //벡터연산 차
        //2ms v_div //벡터연산 나눔
        //2ms Vector3.magnitude
        //2ms Math.Atan2 ~ Math.Cos ~ Math.Sin
        //3.5ms GetSignedAngle_Atan2_AxisY (월드축)
        //4ms My_Normalize
        //6ms Vector3.Dot
        //6ms GetSignedAngle_Normalize (월드축제한 없음 : 인수벡터 2개 정규화)
        //10ms GetSignedAngle_Normalize (월드축제한 없음 : 인수벡터 1개 정규화)
        //17ms GetSignedAngle_Normalize (월드축제한 없음 : 인수벡터 0개 정규화)





        //==============================================================================
        //atan2(1)  2.994ms atan2(2)  2.217ms atan2(3)  2.919ms atan2(4)  3.251ms atan2(5)  3.709ms atan2(6)  3.648ms

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = Mathf.Atan2((float)i, 5f); 
        }
        _timeTemp += "  atan2(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = (float)Math.Atan2((double)i, 5.0); // !!!!!!!!!!!!!! 가장빠름 
        }
        _timeTemp += "  atan2(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = FastAtan2((float)i, 5.0f);
        }
        _timeTemp += "  atan2(3)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = inline_ATan2((float)i, 5.0f);
        }
        _timeTemp += "  atan2(4)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = anony_ATan2((float)i, 5.0f);
        }
        _timeTemp += "  atan2(5)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = lamda_ATan2((float)i, 5.0f);
        }
        _timeTemp += "  atan2(6)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _timeTemp += "\n";
        //==============================================================================
        //sqrt  1.873ms sqrt(2)  1.167ms cos  2.751ms cos(2)  2.095ms sin  2.861ms sin(2)  2.062ms dot(1)  5.94ms dot(2)  5.883ms

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float c = Mathf.Sqrt((float)i); 
        }
        _timeTemp += "  sqrt  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float c = (float)Math.Sqrt((float)i);  //가장빠름  - 이것쓰!!!!!
        }
        _timeTemp += "  sqrt(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float b = Mathf.Cos((float)i);
        }
        _timeTemp += "  cos  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float b = (float)Math.Cos((float)i); //가장빠름  - 이것쓰!!!!!
        }
        _timeTemp += "  cos(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float b = Mathf.Sin((float)i);
        }
        _timeTemp += "  sin  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float b = (float)Math.Sin((float)i); //가장빠름  - 이것쓰!!!!!
        }
        _timeTemp += "  sin(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float d = Vector3.Dot(_line_0.position, _line_1.position);
        }
        _timeTemp += "  dot(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float d = Dot(_line_0.position, _line_1.position);
        }
        _timeTemp += "  dot(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _timeTemp += "\n";
        //==============================================================================
        //norm(1)  5.087ms norm(2)  5.887ms norm(3)  3.748ms norm(4)  3.956ms

        Vector3 vd = _line_0.position;
        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vd.Normalize();
        }
        _timeTemp += "  norm(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            Vector3 ve = vd.normalized;
        }
        _timeTemp += "  norm(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            Misc.GetDir64_Normal3D(vd); 
        }
        _timeTemp += "  norm(3)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            My_Normalize(vd); //두번째로 빠름 . 가장 빠른것보다 정확도가 높기 때문에 이것쓰기 !!!!!! 
        }
        _timeTemp += "  norm(4)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _timeTemp += "\n";
        //==============================================================================
        //v_mult  1.602ms v_mult(1)  1.285ms v_div  2.204ms v_div(1)  2.055ms v_plus  1.709ms v_plus(1)  1.418ms v_minu  1.748ms v_minu(1)  1.378ms
  
        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = va * i;
           
        }
        _timeTemp += "  v_mult  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = Operator_Mult(va, i);
        }
        _timeTemp += "  v_mult(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = va / i;
        }
        _timeTemp += "  v_div  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = Operator_Division(va, i);
        }
        _timeTemp += "  v_div(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = va + vb;
        }
        _timeTemp += "  v_plus  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = Operator_Plus(va, vb);
        }
        _timeTemp += "  v_plus(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = va - vb;

        }
        _timeTemp += "  v_minu  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            vc = Operator_Minus(va, vb);
        }
        _timeTemp += "  v_minu(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _timeTemp += "\n";
        //==============================================================================
        //v_length(1)  1.983ms v_length(2)  2.498ms
  
        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = va.magnitude; //기본 벡터길이 함수 사용하기 !!!!!!!

        }
        _timeTemp += "  v_length(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = GetV2Length_AxisY(va);

        }
        _timeTemp += "  v_length(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        _timeTemp += "\n";
        //==============================================================================
        //v_angle(1)  22.471ms v_angle(2)  17.347ms v_angle(2.1)  10.376ms v_angle(2.2)  6.143ms v_angle(3)  3.539ms v_angle(4)  3.398ms

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = Vector3.SignedAngle(va, vb, ConstV.v3_up);

        }
        _timeTemp += "  v_angle(1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = GetSignedAngle_Normalize(va, vb, ConstV.v3_up); //0개 정규화된 인수 

        }
        _timeTemp += "  v_angle(2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = GetSignedAngle_Normalize_B(va, vb, ConstV.v3_up); //1개 정규화된 인수

        }
        _timeTemp += "  v_angle(2.1)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = GetSignedAngle_Normalize_C(va, vb, ConstV.v3_up); //2개 정규화된 인수 

        }
        _timeTemp += "  v_angle(2.2)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";


        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = GetSignedAngle_Atan2_AxisY_A(va, vb); //가장빠름 - 이것 사용하기 !!!!!! 

        }
        _timeTemp += "  v_angle(3)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        _startDateTime = DateTime.Now;
        for (int i = 0; i < 50000; i++)
        {
            float a = GetSignedAngle_Atan2_AxisY_B(va, vb);

        }
        _timeTemp += "  v_angle(4)  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

        DebugWide.LogBlue(_timeTemp);
        _timeTemp = ConstV.STRING_EMPTY;



	}

    string _timeTemp = ConstV.STRING_EMPTY;
    Stopwatch _sw = new Stopwatch();
    DateTime _startDateTime;
    DateTime _endDateTime;



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


    public float Dot(Vector3 a, Vector3 b)
    {
        //float xx = a.x * b.x;
        //float yy = a.y * b.y;
        //float zz = a.z * b.z;
        //return xx + yy + zz;
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }


    //ref : http://blog.naver.com/PostView.nhn?blogId=neverabandon&logNo=221106512600
    float FastAtan2(float y, float x)
    {
        const float MY_FLT_EPSILON = 1.192092896e-07F;
        const float scale = 0.017453292f; // pi / 180 = 1도에 해당하는 라디안
        const float atan2_p1 = 57.283627f; // 180 / pi = 1라디안에 해당하는 도
        const float atan2_p3 = -18.667446f;
        const float atan2_p5 = 8.9140005f;
        const float atan2_p7 = -2.5397246f;

        float ax = (x > 0) ? x : -x;
        float ay = (y > 0) ? y : -y;
        float a, c, c2;

        if (ax >= ay)
        {
            c = ay / (ax + MY_FLT_EPSILON); //0으로 나누는 것을 방지하기 위해 앱실론을 더한다 
            c2 = c * c;
            a = (((atan2_p7 * c2 + atan2_p5) * c2 + atan2_p3) * c2 + atan2_p1) * c;
        }
        else
        {
            c = ax / (ay + MY_FLT_EPSILON);
            c2 = c * c;
            a = 90.0f - (((atan2_p7 * c2 + atan2_p5) * c2 + atan2_p3) * c2 + atan2_p1) * c;
        }

        if (x < 0)
        {
            a = 180.0f - a;
        }
        if (y < 0)
        {
            a = 360.0f - a;
        }
        return (float)(a * scale); //라디안으로 변환
    }


    //ref : https://stackoverflow.com/questions/473782/inline-functions-in-c
    //인라인 적용이 안됨. 강제 컴파일 키워드와 관련있는 것 같음 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float inline_ATan2(float y, float x) // y / x
    {
        return (float)System.Math.Atan2(y, x);
    }



    //ref : https://stackoverflow.com/questions/4900069/how-to-make-inline-functions-in-c-sharp
    //인라인화 되는지는 모르겠지만 성능이 Mathf.Atan2 보다 안좋다
    Func<float, float, float> anony_ATan2 = delegate (float x, float y)
    {
        return (float)System.Math.Atan2(x, y);
    };
    //무명 메소드와 마찮가지로 인라인화 되는지 모르겠고, 성능도 안좋다
    Func<float, float, float> lamda_ATan2 = (float x, float y) => (float)System.Math.Atan2(x, y);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Vector3 Operator_Plus(Vector3 va, Vector3 vb, Vector3 result = default(Vector3))
    {
        result.x = va.x + vb.x;
        result.y = va.y + vb.y;
        result.z = va.z + vb.z;
        return result;
        //return new Vector3(va.x + vb.x , va.y + vb.y , va.z + vb.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Vector3 Operator_Minus(Vector3 va, Vector3 vb, Vector3 result = default(Vector3))
    {
        result.x = va.x - vb.x;
        result.y = va.y - vb.y;
        result.z = va.z - vb.z;
        return result;
        //return new Vector3(va.x - vb.x, va.y - vb.y, va.z - vb.z);
    }

    Vector3 Operator_Mult(Vector3 va, float b, Vector3 result = default(Vector3))
    {
        result.x = va.x * b;
        result.y = va.y * b;
        result.z = va.z * b;
        return result;
        //return new Vector3(va.x * b, va.y * b, va.z * b);
    }

    Vector3 Operator_Division(Vector3 va, float b, Vector3 result = default(Vector3))
    {
        result.x = va.x / b;
        result.y = va.y / b;
        result.z = va.z / b;
        return result;
        //return new Vector3(va.x / b, va.y / b, va.z / b);
    }


    Vector3 My_Normalize(Vector3 vector3)
    {
        float len = vector3.magnitude;
        vector3.x /= len;
        vector3.y /= len;
        vector3.z /= len;

        return vector3; //Vector3.normalize 보다 빠르다

        //return vector3 / len; //Vector3.normalize 의 방식. 느리다
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
    public float GetSignedAngle_Normalize(Vector3 v0, Vector3 v1 , Vector3 axis)
    {
        //v0.Normalize();
        //v1.Normalize();
        v0 = My_Normalize(v0);
        v1 = My_Normalize(v1);
        float proj = Vector3.Dot(v0, v1);
        Vector3 vxw = Vector3.Cross(v0, v1);

        //스칼라삼중적을 이용하여 최단회전방향을 구한다 
        //float sign = 1f;
        if (Vector3.Dot(axis , vxw) < 0 ) 
            return Mathf.Acos(proj) * Mathf.Rad2Deg * -1f;

        return Mathf.Acos(proj) * Mathf.Rad2Deg;
    }

    public float GetSignedAngle_Normalize_B(Vector3 v0, Vector3 v1, Vector3 axis)
    {
        //v0.Normalize();
        //v1.Normalize();
        //v0 = My_Normalize(v0);
        v1 = My_Normalize(v1);
        float proj = Vector3.Dot(v0, v1);
        Vector3 vxw = Vector3.Cross(v0, v1);

        //스칼라삼중적을 이용하여 최단회전방향을 구한다 
        //float sign = 1f;
        if (Vector3.Dot(axis, vxw) < 0)
            return Mathf.Acos(proj) * Mathf.Rad2Deg * -1f;

        return Mathf.Acos(proj) * Mathf.Rad2Deg;
    }

    public float GetSignedAngle_Normalize_C(Vector3 v0, Vector3 v1, Vector3 axis)
    {
        //v0.Normalize();
        //v1.Normalize();
        //v0 = My_Normalize(v0);
        //v1 = My_Normalize(v1);
        float proj = Vector3.Dot(v0, v1);
        Vector3 vxw = Vector3.Cross(v0, v1);

        //스칼라삼중적을 이용하여 최단회전방향을 구한다 
        //float sign = 1f;
        if (Vector3.Dot(axis, vxw) < 0)
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


    public static float GetSignedAngle_Atan2_AxisY_A(Vector3 v0, Vector3 v1)
    {
        float at0 = (float)Math.Atan2(v0.z, v0.x);
        float at1 = (float)Math.Atan2(v1.z, v1.x);

        return (at0 - at1) * Mathf.Rad2Deg;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetSignedAngle_Atan2_AxisY_B(Vector3 v0, Vector3 v1)
    {
        float at0 = (float)Math.Atan2(v0.z, v0.x);
        float at1 = (float)Math.Atan2(v1.z, v1.x);

        return (at0 - at1) * Mathf.Rad2Deg;
    }
   
   
    public static float GetAngle_Atan2_AxisY(Vector3 v0, Vector3 v1)
    {
        //float at0 = Mathf.Atan2(v0.z, v0.x);
        //float at1 = Mathf.Atan2(v1.z, v1.x);
        float at0 = (float)Math.Atan2(v0.z, v0.x);
        float at1 = (float)Math.Atan2(v1.z, v1.x);
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
