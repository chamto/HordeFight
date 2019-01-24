using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Test_AABBCulling : MonoBehaviour 
{
    List<Bounds> mRectangles = null;
    RectangleManager mManager = null;

    const float WIDTH_MAX = 20f;
    readonly Vector3 V3_ZERO = Vector3.zero;

	// Use this for initialization
	void Start () 
    {
        Init();
	}
	
	// Update is called once per frame
	//void Update () 
    private void OnDrawGizmos()
    {
        if (null == mManager) return;

        ModifyRectangles();
        mManager.Update();
        DrawRectangles();
	}


    public float rnd
    {
        get
        {
            return Random.Range(0.125f * WIDTH_MAX, 0.875f * WIDTH_MAX);
        }
    }
    public float intrrnd
    {
        get
        {
            return Random.Range(1.0f, 3f);
        }
    }
    public float mPerturb
    {
        get
        {
            return Random.Range(-2.0f, 2.0f);
        }
    }

    public void Init()
    {

        int OBJECT_COUNT = 300;

        mRectangles = new List<Bounds>();
        for (int i = 0; i < OBJECT_COUNT; ++i)
        {
            Vector3 min = new Vector3(rnd, rnd, 0);
            Vector3 max = new Vector3(min.x + intrrnd, min.y + intrrnd , 0);

            Bounds bounds = new Bounds();
            bounds.min = min;
            bounds.max = max;
            //DebugWide.LogBlue(bounds + "   size: " + bounds.size + "   min: "  + bounds.min + "   max: " + bounds.max); //chamto test
            mRectangles.Add(bounds);
        }

        mManager = new RectangleManager(mRectangles);

    }


    public void ModifyRectangles()
    {
        
        for (int i = 0; i < mRectangles.Count;i++)
        {
            Vector3 min = V3_ZERO, max = V3_ZERO;
            Bounds rectangle = mRectangles[i];

            //DebugWide.LogBlue( i + ":a:   " + mRectangles[i] + "   size: " + mRectangles[i].size + "   min: " + mRectangles[i].min + "   max: " + mRectangles[i].max);
            float dx = mPerturb;
            if (0.0f <= rectangle.min.x + dx && rectangle.max.x + dx < WIDTH_MAX)
            {
                min.x += dx;
                max.x += dx;
            }

            float dy = mPerturb;
            if (0.0f <= rectangle.min.y + dy && rectangle.max.y + dy < WIDTH_MAX)
            {
                min.y += dy;
                max.y += dy;
            }

            rectangle.min += min;
            rectangle.max += max;
            mRectangles[i] = rectangle;
            //DebugWide.LogBlue(i + ":b:   " + mRectangles[i] + "   size: " + mRectangles[i].size + "   min: " + mRectangles[i].min + "   max: " + mRectangles[i].max);

            mManager.SetRectangle(i, rectangle);

        }


    }


    public void DrawRectangle(Bounds box, uint color, bool isSolid)
    {
        Color cc = Misc.Color32_ToColor(Misc.Hex_ToColor32(color));
        //DebugWide.LogBlue(cc + "    " + DefineO.HexToColor(color) + "    " + color);
        //cc.a = 1f;

        Gizmos.color = cc;

        if(true == isSolid)
            Gizmos.DrawCube(box.center, box.size);
        else
            Gizmos.DrawWireCube(box.center, box.size);

    }


    public bool QueryIntersects(Bounds box0, Bounds box1, out Bounds overlapArea)
    {
        overlapArea = new Bounds();
        for (int i = 0; i < 2; i++)
        {
            if (box0.max[i] < box1.min[i] || box0.min[i] > box1.max[i])
            {

                return false;
            }
        }

        Vector3 min = V3_ZERO, max = V3_ZERO;
        for (int i = 0; i < 2; i++)
        {
            if (box0.max[i] <= box1.max[i])
            {
                max[i] = box0.max[i];
            }
            else
            {
                max[i] = box1.max[i];
            }

            if (box0.min[i] <= box1.min[i])
            {
                min[i] = box1.min[i];
            }
            else
            {
                min[i] = box0.min[i];
            }
            overlapArea.max = max;
            overlapArea.min = min;
        }

        return true;
    }
   

    public void DrawRectangles()
    {
        uint gray = (uint)0xFFC0C0C0, black = 0, red = (uint)0xFF0000FF;

        for (int i = 0; i < mRectangles.Count;i++)
        {
            DrawRectangle(mRectangles[i], gray, true);
            DrawRectangle(mRectangles[i], black, false);
        }

        //DebugWide.LogBlue(" Overlap count: "+mManager.GetOverlap().Count); //chamto test

        //int overlap_count = mManager.GetOverlap().Count;

        foreach (RectangleManager.UnOrderedEdgeKey overlap in mManager.GetOverlap())
        {
            int i0 = overlap._V0, i1 = overlap._V1;
            //if(mRectangles[i0].Intersects(mRectangles[i1]))
            Bounds overlapArea;
            if(QueryIntersects(mRectangles[i0] , mRectangles[i1] , out overlapArea))
            {
                
                DrawRectangle(overlapArea, red, true); //채워진 사각형
                DrawRectangle(overlapArea, black, false); //빈 사각형
            }
        }
    }

}






