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


        Vector3 hitPoint;
        bool result = HitBoundingBox(boxMin, boxMax, _ray_start.position, _ray_end.position - _ray_start.position, out hitPoint);
        if(result)
        {
            DebugWide.DrawCircle(hitPoint, 0.5f, Color.red);
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
    //======================================================
    public bool HitBoundingBox(Vector3 minB, Vector3 maxB, Vector3 origin, Vector3 dir, out Vector3 coord)
    {
        const int NUMDIM = 3;
        const int RIGHT = 0;
        const int LEFT = 1;
        const int MIDDLE = 2;

        bool inside = true;
        int[] quadrant = new int[NUMDIM];
        int whichPlane;
        Vector3 maxT = UtilGS9.ConstV.v3_zero;
        Vector3 candidatePlane = UtilGS9.ConstV.v3_zero;

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
            }
            else
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


        /* Calculate T distances to candidate planes */
        for (int i = 0; i<NUMDIM; i++)
        {
            if (quadrant[i] != MIDDLE && dir[i] != 0f)
                maxT[i] = (candidatePlane[i] - origin[i]) / dir[i];
            else
                maxT[i] = -1f;
        }
            

        /* Get largest of the maxT's for final choice of intersection */
        whichPlane = 0;
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
                if (coord[i] < minB[i] || coord[i] > maxB[i])
                    return false;
            }
            else
            {
                coord[i] = candidatePlane[i];
            }
        }
            
        return true;              /* ray hits box */
    }  
}
