using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test2_SphereTree : MonoBehaviour 
{
    private SphereTree _sphereTree = null;

    public int _count = 100;
    public Transform _sphere = null;
    public Transform _lineStart = null;
    public Transform _lineEnd = null;


	// Use this for initialization
	void Start () 
    {
        
        _sphereTree = new SphereTree(_count, 256, 64, 8);

        for (int i = 0; i < _count;i++)
        {
            Vector3 pos = new Vector3(Misc.rand.Next() % 1000, Misc.rand.Next() % 600, 0);
            float radius = (Misc.rand.Next() % 4) + 1;
            SphereModel model = _sphereTree.AddSphere(pos, radius, SphereModel.Flag.LEAF_TREE);    
        }

	}
	
	// Update is called once per frame
	void Update () 
    {
        _sphereTree.Process();

	}

    private void OnDrawGizmos()
    {
        if (null == _sphereTree) return;
        _sphereTree.Render_Debug();

        Vector3 pointIts = Vector3.zero;
        Vector3 dir = _lineEnd.position - _lineStart.position;
        dir.Normalize();
        DefineI.DrawCircle(_sphere.position, 10, Color.red);
        DefineI.DrawLine(_lineStart.position, _lineEnd.position, Color.red);
        //DefineI.RayIntersection(_sphere.position, 10, _lineStart.position, dir, out pointIts);
        DefineI.RayIntersectionInFront(_sphere.position, 10, _lineStart.position, dir, out pointIts);

        DefineI.DrawCircle(pointIts, 1, Color.blue);
    }
}

//=======================================================

public class DefineI
{
    static public void DrawLine(Vector3 start, Vector3 end, Color cc)
    {
        
        Gizmos.color = cc;
        Gizmos.DrawLine(start, end);
    }

    static public void DrawCircle(Vector3 pos, float radius, Color cc)
    {
        Gizmos.color = cc;
        Gizmos.DrawWireSphere(pos, radius);
    }

    //intersect : 반직선이 원과 충돌한 첫번째 위치 
    static public bool RayIntersection(Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersect_firstPoint)
    {

        Vector3 w = sphereCenter - rayOrigin;
        Vector3 v = rayDirection; //rayDirection 이 정규화 되어 있어야 올바르게 계산할 수 있다 
        float rsq = sphereRadius * sphereRadius;
        float wsq = Vector3.Dot(w, w); //w.x * w.x + w.y * w.y + w.z * w.z;

        // Bug Fix For Gem, if origin is *inside* the sphere, invert the
        // direction vector so that we get a valid intersection location.
        if (wsq < rsq) v *= -1; //반직선의 시작점이 원안에 있는 경우 - 방법1 

        float proj = Vector3.Dot(w, v);
        float ssq = (wsq - proj * proj);
        float dsq = rsq - ssq; 

        intersect_firstPoint = Vector3.zero;
        if (dsq > 0.0f)
        {
            float d = Mathf.Sqrt(dsq);

            //반직선의 시작점이 원안에 있는 경우 - 방법2
            //float length = proj - d; //선분 시작점이 원 밖에 있는 경우
            //if(wsq < rsq) length = proj + d; //선분 시작점이 원 안에 있는 경우
            //intersect_firstPoint = rayOrigin + v * length;

            intersect_firstPoint = rayOrigin + v * (proj - d);

            return true;
        }
        return false;
    }


   
    static public bool RayIntersectionInFront(Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersect)
    {
        Vector3 intersect_firstPoint;
        bool hit = RayIntersection(sphereCenter, sphereRadius, rayOrigin, rayDirection, out intersect_firstPoint);

        intersect = Vector3.zero;
        if (hit)
        {
            Vector3 dir = intersect_firstPoint - rayOrigin;

            float dot = Vector3.Dot(dir, rayDirection);

            if (dot >= 0) // then it's in front!
            {
                intersect = intersect_firstPoint;
                return true;
            }
        }
        return false;
    }
}
