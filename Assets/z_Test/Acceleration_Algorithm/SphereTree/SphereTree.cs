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

    public static int FIXED = 16;//16;
    public static int SW_ID = (SCREEN_WIDTH * FIXED); //1024 * 16 = 16384
    public static int SH_IT = (SCREEN_HEIGHT * FIXED); //768 * 16 = 12288

    public static int G_CENTER_X = SW_ID / 2; //16384 / 2 = 8192
    public static int G_CENTER_Y = SH_IT / 2; //12288 / 2 = 6144


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

