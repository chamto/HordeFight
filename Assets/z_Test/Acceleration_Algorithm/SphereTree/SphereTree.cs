using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class SphereTree : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}


public class DefineO
{

    public static int SCREEN_WIDTH = 1024;
    public static int SCREEN_HEIGHT = 768;
    public static int MAX_ATTRACTORS = 16;

    public static int JUMP_TIME = 128;
    public static int FIXED = 16;
    public static int SWID = (SCREEN_WIDTH * FIXED);
    public static int SHIT = (SCREEN_HEIGHT * FIXED);

    public static int gCenterX = SWID / 2;
    public static int gCenterY = SHIT / 2;



    public enum SpherePackFlag
    {

        SPF_SUPERSPHERE = (1 << 0), // this is a supersphere, allocated and deleted by us
        SPF_ROOT_TREE = (1 << 1), // member of the root tree
        SPF_LEAF_TREE = (1 << 2), // member of the leaf node tree
        SPF_ROOTNODE = (1 << 3), // this is the root node
        SPF_RECOMPUTE = (1 << 4), // needs recomputed bounding sphere
        SPF_INTEGRATE = (1 << 5), // needs to be reintegrated into tree
                                  // Frame-to-frame view frustum status.  Only does callbacks when a
                                  // state change occurs.
        SPF_HIDDEN = (1 << 6), // outside of view frustum
        SPF_PARTIAL = (1 << 7), // partially inside view frustum
        SPF_INSIDE = (1 << 8)  // completely inside view frustum
    }

    public enum ViewState
    {
        VS_INSIDE,   // completely inside the frustum.
        VS_PARTIAL,  // partially inside and partially outside the frustum.
        VS_OUTSIDE   // completely outside the frustum
    }


    public enum CohenSutherland
    {
        CS_LEFT = (1 << 0),
        CS_RIGHT = (1 << 1),
        CS_TOP = (1 << 2),
        CS_BOTTOM = (1 << 3)
    }


    static public void DrawLine(float x1, float y1, float x2, float y2, uint color)
    {
        Color cc = Misc.Color32_ToColor(Misc.Hex_ToColor32(color));
        //DebugWide.LogBlue(cc);
        //cc.a = 1f;


        Gizmos.color = cc;
        Gizmos.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
    }

    static public void DrawCircle(float locx, float locy, float radius, uint color)
    {
        Color cc = Misc.Color32_ToColor(Misc.Hex_ToColor32(color));
        //DebugWide.LogBlue(cc + "    " + DefineO.HexToColor(color) + "    " + color);
        //cc.a = 1f;

        //UnityEditor.Handles.color = Color.red;
        Gizmos.color = cc;
        Gizmos.DrawWireSphere(new Vector3(locx, locy, 0), radius);
    }

    static public void PrintText(float x, float y, uint color, string text)
    {
        UnityEditor.Handles.color = Misc.Color32_ToColor(Misc.Hex_ToColor32(color));

        UnityEditor.Handles.Label(new Vector3(x, y, 0), text);
    }
}
//===================================================

