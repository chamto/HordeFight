using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_BarycentricCoordinates : MonoBehaviour 
{

    Transform _t_p0 = null;
    Transform _t_p1 = null;
    Transform _t_p2 = null;
    Transform _t_point = null;
    // Use this for initialization
    void Start () 
    {
        _t_p0 = GameObject.Find("P0").transform;
        _t_p1 = GameObject.Find("P1").transform;
        _t_p2 = GameObject.Find("P2").transform;
        _t_point = GameObject.Find("Point").transform;

    }
	
	// Update is called once per frame
	//void Update () {
		
	//}

    private void OnDrawGizmos()
    {
        if (null == (object)_t_p0) return;

        DebugWide.DrawLine(_t_p0.position, _t_p1.position, Color.blue);
        DebugWide.DrawLine(_t_p1.position, _t_p2.position, Color.blue);
        DebugWide.DrawLine(_t_p2.position, _t_p0.position, Color.blue);
        DebugWide.DrawCircle(_t_point.position, 3, Color.red);

        float r, s, t;
        BarycentricCoordinates(out r, out s, out t, _t_point.position, _t_p0.position, _t_p1.position, _t_p2.position);
    }

    public void BarycentricCoordinates(out float r, out float s, out float t,
                                         Vector3 point, Vector3 P0, Vector3 P1, Vector3 P2)
    {
        // get difference vectors
        Vector3 u = P1 - P0;
        Vector3 v = P2 - P0;
        Vector3 w = point - P0;

        // compute cross product to get area of parallelograms
        //Vector3 a = Vector3.Cross(u, w);
        //Vector3 b = Vector3.Cross(v, w);
        //Vector3 c = Vector3.Cross(u, v);

        //외적의 길이값으로 계산하면 부호가 사라지므로 점이 삼각형 밖에 있는지 판별을 할 수 없다 
        // compute barycentric coordinates as ratios of areas
        //float denom = 1.0f / c.magnitude;
        //s = b.magnitude * denom; //u
        //t = a.magnitude * denom; //v
        //r = 1.0f - s - t; //w

        //스칼라 삼중곱을 하기위해 외적 순서를 맞춘다 , 3차원에서의 행렬식값 구하기
        Vector3 a = Vector3.Cross(u, w);
        Vector3 b = Vector3.Cross(w, v);
        Vector3 c = Vector3.Cross(u, v);

        float denom = 1.0f / Vector3.Dot(c,c);
        s = Vector3.Dot(c, b) * denom; //u
        t = Vector3.Dot(c, a) * denom; //v
        r = 1.0f - s - t; //w
        //무게중심좌표 : t(u,v) = (1 - u - v)P0 + uP1 + vP2

        //==================
        DebugWide.DrawLine(P0, P0 + u * s, Color.green);
        DebugWide.DrawLine(P0, P0 + v * t, Color.green);

        //DebugWide.DrawLine(P0, P0 + (u * s)+(v * t), Color.green); //아래와 같은 표현 
        DebugWide.DrawLine(P0, r*P0 + s*P1 + t*P2, Color.green);

        //DebugWide.DrawLine(P0, P0 + w * r, Color.red); //의미없음 
        DebugWide.LogBlue("w: " + r + " u:" + s + " v:" + t);
    }
}
