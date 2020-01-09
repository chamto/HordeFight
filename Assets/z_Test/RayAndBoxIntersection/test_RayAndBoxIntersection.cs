using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class test_RayAndBoxIntersection : MonoBehaviour 
{

    private Transform _box_ori = null;
    private Transform _box_size = null;
    private Transform _ray_start = null;
    private Transform _ray_end = null;


	// Use this for initialization
	void Start () 
    {
        _box_ori = GameObject.Find("box_ori").transform;
        _box_size = GameObject.Find("box_size").transform;
        _ray_start = GameObject.Find("ray_start").transform;
        _ray_end = GameObject.Find("ray_end").transform;
	}
	
	// Update is called once per frame
	//void Update () {}

    private void OnDrawGizmos()
    {
        if (null == _box_ori) return;

        Vector3 sizeHalf = _box_size.position * 0.5f;
        Vector3 boxMin = _box_ori.position - sizeHalf;
        Vector3 boxMax = _box_ori.position + sizeHalf;

        DebugWide.DrawCube(_box_ori.position, _box_size.position, Color.black);
        DebugWide.DrawLine(_ray_start.position, _ray_end.position, Color.green);


        Vector3 hitPoint, hitPoint2;
        bool result = HitBoundingBox(boxMin, boxMax, _ray_start.position, _ray_end.position - _ray_start.position, out hitPoint);
        result = HitBoundingBox(boxMin, boxMax, _ray_end.position, _ray_start.position - _ray_end.position, out hitPoint2);
        if(result)
        {
            DebugWide.DrawCircle(hitPoint, 0.5f, Color.red);
            DebugWide.DrawCircle(hitPoint2, 0.5f, Color.blue);
        }
    }


    //======================================================
    // 
    //Fast Ray-Box Intersection
    //by Andrew Woo
    //from "Graphics Gems", Academic Press, 1990
    //
    //리얼타임 렌더링 2판 630쪽 , Woo의 방법 , 광선과 AABB간의 교차점을 
    //찾는 최적화 방법
    //
    //minB[NUMDIM], [NUMDIM];           /*box */
    //origin[NUMDIM], dir[NUMDIM];     /*ray */
    //double coord[NUMDIM];            /* hit point */
    //
    //반직선이 나오는 면의 hitPoint 를 계산하기 위해 서는 나오는 면에 대한 처리를 똑같이 한번더 해야함
    //같은 처리이기 때문에 인수를 바꾸어 나오는 면에 대한 hitPoint 를 사용하기로 함  
    //======================================================
    public bool HitBoundingBox(Vector3 minB, Vector3 maxB, Vector3 origin, Vector3 dir, out Vector3 coord)
    {
        
        const int NUMDIM = 3;
        const int RIGHT = 0;
        const int LEFT = 1;
        const int MIDDLE = 2;

        bool inside = true;
        int[] quadrant = new int[NUMDIM]; //사분면 
        int whichPlane; //어느평면 
        Vector3 maxT = UtilGS9.ConstV.v3_zero;
        Vector3 candidatePlane = UtilGS9.ConstV.v3_zero; //후보 평면 

        coord = UtilGS9.ConstV.v3_zero;

        /* Find candidate planes; this loop can be avoided if
        rays cast all from the eye(assume perpsective view) */
        for (int i=0; i<NUMDIM; i++)
        {
            if (origin[i] < minB[i])
            {
                quadrant[i] = LEFT;
                candidatePlane[i] = minB[i];
                inside = false;

            }
            else if (origin[i] > maxB[i])
            {
                quadrant[i] = RIGHT;
                candidatePlane[i] = maxB[i];
                inside = false;

            }else
            {
                quadrant[i] = MIDDLE;
            }

        }

        /* Ray origin inside bounding box */
        if(true == inside)  
        {
            coord = origin;
            return true;
        }

        //반직선이 상자를 통과헤 지나간다면, 반직선이 상자에 들어가는 면 1개 , 나오는 면 1개가 존재하게 된다
        //후보면3개중 1개만 통과하며, 나머지 면은 후보면값(min 또는 max)을 그대로 가지고 있게 된다 

        //후보면의 t값 계산 : 각각의 후보면에서 (후보면 - ray시작점) 값이 가장 큰 것을 t로 삼는다 
        //이는 ray시작점 --- 후보면 --- ray방향길이 일때의 t값만 써야하기 때문이다 
        //반직선 시작점이 후보면 값보다 작을때, 상자를 통과한다. 이는 (후보면 - ray시작점) 값이 다른축의 값들 보다 크다는 뜻이다 

        /* Calculate T distances to candidate planes */
        for (int i = 0; i<NUMDIM; i++)
        {
            if (quadrant[i] != MIDDLE && dir[i] != 0f)
            {
                //직선을 벡터로 표현한 수식을 t에 대하여 정리한 것 
                //p = ori + t * dir  -->  t = (p - ori) / dir
                maxT[i] = (candidatePlane[i] - origin[i]) / dir[i]; 
            }
            else
                maxT[i] = -1f;
        }

        //DebugWide.LogBlue(maxT);

        /* Get largest of the maxT's for final choice of intersection */
        //총2번 루프 : [0] < [1](y평면)  , [1] < [2](z평면) , [0] < [2](z평면)
        //maxT중 가장 큰값을 가진 평면을 찾는다. 
        whichPlane = 0; //(x평면)
        for (int i = 1; i<NUMDIM; i++)
        {
            if (maxT[whichPlane] < maxT[i])
                whichPlane = i;
        }
            

        /* Check final candidate actually inside box */
        if (maxT[whichPlane] < 0f) return false;

        for (int i = 0; i<NUMDIM; i++)
        {
            if (whichPlane != i)
            {
                coord[i] = origin[i] + maxT[whichPlane] * dir[i];
                if (coord[i] < minB[i] || coord[i] > maxB[i]) //계산된 점이 평면 외부에 있는 경우 검사 
                    return false;
            }
            else
            {
                coord[i] = candidatePlane[i];
            }
        }
           
        //DebugWide.LogBlue(coord + "  cand: " + candidatePlane);

        return true;              /* ray hits box */
    }  
}
