using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


//실험노트 2021-3-2 에 정리 : 무게중심좌표와 교점구할때 공통적인 연립방정식을 쓴다는 것을 발견함 
// 행렬식값으로 uv 를 구하는데 스칼라삼중곱 형태로 나타낼 수 있었음 (행렬식값 == 스칼라삼중곱)
// uv 를 구하는 스칼라삼중곱 식을 분석해봄. 기하학적으로 내가 직접 쓸수 있지 않을까 했음 
// 연립방정식의 해를 구하기 위한 식으로 도출해낸 것임
// 기하학적인 의미를 분석해내지 못함. 
public class Test_BarycentricCoordinates : MonoBehaviour 
{

    Transform _t_p0 = null;
    Transform _t_p1 = null;
    Transform _t_p2 = null;
    Transform _t_point = null;
    Transform _t_ls0 = null;
    Transform _t_ls1 = null;

    Transform _t_l0_o = null;
    Transform _t_l0_l = null;
    Transform _t_l1_o = null;
    Transform _t_l1_l = null;

    // Use this for initialization
    void Start () 
    {
        _t_p0 = GameObject.Find("P0").transform;
        _t_p1 = GameObject.Find("P1").transform;
        _t_p2 = GameObject.Find("P2").transform;
        _t_point = GameObject.Find("Point").transform;
        _t_ls0 = GameObject.Find("LS0").transform;
        _t_ls1 = GameObject.Find("LS1").transform;

        _t_l0_o = GameObject.Find("L0_0").transform;
        _t_l0_l = GameObject.Find("L0_1").transform;
        _t_l1_o = GameObject.Find("L1_0").transform;
        _t_l1_l = GameObject.Find("L1_1").transform;
    }
	
	// Update is called once per frame
	//void Update () {
		
	//}

    private void OnDrawGizmos()
    {
        if (null == (object)_t_p0) return;

        if(false)
        {
            DebugWide.DrawLine(_t_p0.position, _t_p1.position, Color.blue);
            DebugWide.DrawLine(_t_p1.position, _t_p2.position, Color.blue);
            DebugWide.DrawLine(_t_p2.position, _t_p0.position, Color.blue);
            DebugWide.DrawCircle(_t_point.position, 3, Color.red);

            float r, s, t;
            BarycentricCoordinates(out r, out s, out t, _t_point.position, _t_p0.position, _t_p1.position, _t_p2.position);
        }

        //========================================

        if(true)
        {
            DebugWide.DrawLine(_t_p0.position, _t_p1.position, Color.blue);
            DebugWide.DrawLine(_t_p1.position, _t_p2.position, Color.blue);
            DebugWide.DrawLine(_t_p2.position, _t_p0.position, Color.blue);

            LineSegment3 ls = new LineSegment3(_t_ls0.position, _t_ls1.position);
            ls.Draw(Color.magenta);
            float t;
            //TriangleIntersect(out t, _t_p0.position, _t_p1.position, _t_p2.position, ls);
            TriangleIntersect2(out t, _t_p0.position, _t_p1.position, _t_p2.position, ls);
        }

        //========================================

        if (false)
        {
            DebugWide.DrawLine(_t_l0_o.position, _t_l0_l.position, Color.blue);
            DebugWide.DrawLine(_t_l1_o.position, _t_l1_l.position, Color.magenta);

            LineIntersection2D(_t_l0_o.position, _t_l0_l.position, _t_l1_o.position, _t_l1_l.position);
        }

    }

    public void BarycentricCoordinates(out float w, out float u, out float v,
                                         Vector3 point, Vector3 P0, Vector3 P1, Vector3 P2)
    {
        // get difference vectors
        Vector3 e1 = P1 - P0;
        Vector3 e2 = P2 - P0;
        Vector3 pp = point - P0;

        // compute cross product to get area of parallelograms
        //Vector3 a = Vector3.Cross(e1, pp);
        //Vector3 b = Vector3.Cross(pp, e2);
        //Vector3 c = Vector3.Cross(e1, e2);

        //외적의 길이값으로 계산하면 부호가 사라지므로 점이 삼각형 밖에 있는지 판별을 할 수 없다 
        // compute barycentric coordinates as ratios of areas
        //float denom = 1.0f / c.magnitude;
        //u = b.magnitude * denom; //u
        //v = a.magnitude * denom; //v
        //w = 1.0f - s - t; //w

        //스칼라 삼중곱 : 면적을(또는 길이) 쌓는다 
        //스칼라 삼중곱을 하기위해 외적 순서를 맞춘다 , 3차원에서의 행렬식값 구하기
        //외적의 감기는 순서를 같게 만들어 줘야 함 

        //denom : (e1 x e2) . (e1 x e2)
        // u : (pp x e2) . (e1 x e2) 
        // v : (e1 x pp) . (e1 x e2)


        Vector3 b = Vector3.Cross(pp, e2);
        Vector3 a = Vector3.Cross(e1, pp);
        Vector3 c = Vector3.Cross(e1, e2);

        float denom = 1.0f / Vector3.Dot(c,c);
        u = denom * Vector3.Dot(c, b); //denom : e1 , pp/e1
        v = denom * Vector3.Dot(c, a); //denom : e2 , pp/e2
        w = 1.0f - u - v; //w
        //무게중심좌표 : t(u,v) = (1 - u - v)P0 + uP1 + vP2

        //test 1 - 행렬식값의 비율이 아닌 e1,e2 길이의 비율이므로 정상계산 못함 
        //u = Vector3.Dot(pp, e1) / Vector3.Dot(e1, e1);
        //v = Vector3.Dot(pp, e2) / Vector3.Dot(e2, e2);
        //w = 1.0f - u - v; //w

        //test 2 - cos90 =0 이 되어 nan에러가 발생 
        //u = Vector3.Dot(pp, c) / Vector3.Dot(e1, c);
        //v = Vector3.Dot(pp, c) / Vector3.Dot(e2, c);
        //w = 1.0f - u - v; //w

        //test 3 - cos90 =0 이 안되는 임의의 노멀에 성립하는지 시험 , 정상동작 !!!! 
        // 점이 삼각형 평면에 있을 때만 성립되는 특수한 경우였음 , 방향선분의 끝부분이 삼각형의 평면에 있을 때도 성립 
        //Vector3 nm = c.normalized;
        //Vector3 nm = new Vector3(5, -3, 4);
        //nm.Normalize();
        //denom = 1.0f / Vector3.Dot(nm, c);
        //u = denom * Vector3.Dot(nm, b); //denom : e1 , pp/e1
        //v = denom * Vector3.Dot(nm, a); //denom : e2 , pp/e2
        //w = 1.0f - u - v; //w

        //test 4 - 임의의 축에 동작하는지 시험 , 정상계산 못함  
        //Vector3 nm = Vector3.forward;
        //u = Vector3.Dot(pp, nm) / Vector3.Dot(e1, nm);
        //v = Vector3.Dot(pp, nm) / Vector3.Dot(e2, nm);
        //w = 1.0f - u - v; //w

        //==================
        DebugWide.DrawLine(P0, P0 + e1 * u, Color.green);
        DebugWide.DrawLine(P0, P0 + e2 * v, Color.green);

        DebugWide.DrawLine(P0, P0 + (e1 * u)+(e2 * v), Color.green); //아래와 같은 표현 
        //DebugWide.DrawLine(P0, w*P0 + u*P1 + v*P2, Color.green);

        DebugWide.LogBlue("w: " + w + " u:" + u + " v:" + v + " u+v:" + (u+v));
    }

    //수학책 525참고 
    public bool TriangleIntersect(out float t, Vector3 P0, Vector3 P1, Vector3 P2, LineSegment3 line)
    {
        t = 0f;
        Vector3 e1 = P1 - P0;
        Vector3 e2 = P2 - P0;
        Vector3 pp = line.origin - P0;

        Vector3 p = Vector3.Cross(line.direction, e2);
        Vector3 q = Vector3.Cross(pp, e1);
        float a = Vector3.Dot(e1, p);

        //스칼라삼중곱의 성질 :  a ⋅ (b x c) = b ⋅ (c x a) = c ⋅ (a x b)
        //denom 의 외적부분을 날리기 위해 각 uvw 에 맞게 변환한다.  
        //denom : e1 . (l_dir x e2) -> e2 . (e1 x l_dir)  -> l_dir . (e2 x e1)
        //u -> pp . (l_dir x e2) -> pp . (l_dir x e2)
        //v -> l_dir . (pp x e1) -> pp . (e1 x l_dir)
        //t -> e2 . (pp x e1) -> pp . (e1 x e2) -> -pp . (e2 x e1)

        //삼각형과 선분의 무한교차 판별못함 
        // if result zero, no intersection or infinite intersections
        if (Math.Abs(a) < float.Epsilon)
        {
            return false;
        }


        // compute denominator
        float denom = 1.0f / a;

        // compute barycentric coordinates
        float u = denom * Vector3.Dot(pp, p); //denom : e1 ,  pp/e1
        float v = denom * Vector3.Dot(line.direction, q); //denom : e2 , pp/e2
        t = denom * Vector3.Dot(e2, q); //denom : l_dir , -pp/l_dir (-pp는 line_origin 에서 시작함, l_dir과 시작위치 동일) 
        float w = 1 - u - v;

        DebugWide.DrawLine(P0, P0 + u * e1, Color.green);
        DebugWide.DrawLine(P0, P0 + v * e2, Color.green);
        //DebugWide.DrawLine(P0, P0 + (u * e1)+(v * e2), Color.green);
        DebugWide.DrawLine(P0, w * P0 + u * P1 + v * P2, Color.green);
        DebugWide.DrawCircle(line.origin+line.direction*t ,3, Color.green);
        DebugWide.LogBlue(" u:" + u + " v:" + v + " u+v:" + (u + v) + "  t:" + t);

        return (t >= 0.0f && t <= 1f);
    }

    public bool TriangleIntersect2(out float t, Vector3 P0, Vector3 P1, Vector3 P2, LineSegment3 line)
    {
        t = 0f;
        Vector3 e1 = P1 - P0;
        Vector3 e2 = P2 - P0;
        Vector3 pp = line.origin - P0;

        Vector3 p = Vector3.Cross(line.direction, e2);
        //Vector3 q = Vector3.Cross(pp, e1);
        float a = Vector3.Dot(e1, p);

        Vector3 v_cr = Vector3.Cross(e1, line.direction);
        Vector3 t_cr = Vector3.Cross(e2, e1);


        //스칼라삼중곱의 성질 :  a ⋅ (b x c) = b ⋅ (c x a) = c ⋅ (a x b)
        //denom 의 외적부분을 날리기 위해 각 uvw 에 맞게 변환한다.  
        //denom : e1 . (l_dir x e2) -> e2 . (e1 x l_dir)  -> l_dir . (e2 x e1)
        //u -> pp . (l_dir x e2) -> pp . (l_dir x e2)
        //v -> l_dir . (pp x e1) -> pp . (e1 x l_dir)
        //t -> e2 . (pp x e1) -> pp . (e1 x e2) -> -pp . (e2 x e1)

        // compute denominator
        float denom = 1.0f / a;

        // compute barycentric coordinates
        float u = denom * Vector3.Dot(pp, p); //denom : e1 ,  pp/e1
        float v = denom * Vector3.Dot(pp, v_cr); //denom : e2 , pp/e2
        t = denom * Vector3.Dot(-pp, t_cr); //denom : l_dir , -pp/l_dir (-pp는 line_origin 에서 시작함, l_dir과 시작위치 동일) 
        float w = 1 - u - v;

        DebugWide.DrawLine(P0, P0 + u * e1, Color.green);
        DebugWide.DrawLine(P0, P0 + v * e2, Color.green);
        DebugWide.DrawLine(P0, w * P0 + u * P1 + v * P2, Color.green);
        DebugWide.DrawCircle(line.origin + line.direction * t, 3, Color.green);
        DebugWide.LogBlue(" u:" + u + " v:" + v + " u+v:" + (u + v) + "  t:" + t);

        return (t >= 0.0f && t <= 1f);
    }


    public bool LineIntersection2D(Vector3 Ao, Vector3 Al,
                           Vector3 Bo, Vector3 Bl)
    {

        Vector3 A_dir = Al - Ao;
        Vector3 B_dir = Bl - Bo;
        //Vector3 ABo = Ao - Bo;
        Vector3 BAo = Bo - Ao;



        //2차원 행렬식값 , 수직내적
        //float rTop = VOp.PerpDot_XZ(BAo, B_dir); //v, w => v.x*w.z - v.z*w.x 
        //float sTop = VOp.PerpDot_XZ(BAo, A_dir);
        //float Bot = VOp.PerpDot_XZ(A_dir, B_dir);


        //3차원 행렬식값 , 스칼라삼중적
        //denom : (a_dir x b_dir) . (a_dir x b_dir)
        // u : (bao x b_dir) . (a_dir x b_dir)
        // v : (bao x a_dir) . (a_dir x b_dir)

        Vector3 a = Vector3.Cross(BAo, B_dir);
        Vector3 b = Vector3.Cross(BAo, A_dir);
        Vector3 c = Vector3.Cross(A_dir, B_dir);

        float rTop = Vector3.Dot(c, a);
        float sTop = Vector3.Dot(c, b);
        float Bot = Vector3.Dot(c, c);


        if (Bot == 0)//parallel
        {
            return false;
        }

        //연립방정식 이용 

        float denom = 1.0f / Bot;
        float u = rTop * denom; //denom : a_dir , bao/a_dir
        float v = sTop * denom; //denom : b_dir , bao/b_dir

        DebugWide.DrawCircle(Ao + A_dir * u, 3, Color.red);
        DebugWide.LogBlue(" u:" + u + " v:" + v + " u+v:" + (u + v));

        if ((u > 0) && (u < 1) && (v > 0) && (v < 1))
        {
            //lines intersect
            return true;
        }

        //lines do not intersect
        return false;
    }
}
