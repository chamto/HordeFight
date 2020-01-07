using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_RayAndBoxIntersection : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        //DebugWide.DrawCube(new Vector3(-1, 0, -1), new Vector3(2,0,2), Color.black);
        //DebugWide.Dr
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
