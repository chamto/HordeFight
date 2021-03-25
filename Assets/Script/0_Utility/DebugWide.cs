﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

public class DebugWide 
{

	private static string _strCommon = "";

    public enum eDebugLogMode
    {
        _None 		= 0,
        _Waring 	= 1,
        _Error 		= 2,
        _Exception 	= 3,
    }


    public static void _BaseLog(object message , Object context , System.Exception exception ,eDebugLogMode eMode)
    {
		//_strCommon = "[Thr:"+System.AppDomain.GetCurrentThreadId().ToString("00000") + "] "; // window only? 
		//_strCommon = CommonNet.CUnitUtil.TimeStamp();
		_strCommon = _strCommon + message;

		if (true == UnityEngine.Debug.isDebugBuild)
        {
            switch (eMode)
            {
                case eDebugLogMode._None:
                    {
                        Debug.Log(message, context); 
                    }
                    break;
                case eDebugLogMode._Waring:
                    {
                        Debug.LogWarning(message, context);
                    }
                    break;
                case eDebugLogMode._Error:
                    {
                        Debug.LogError(message, context);
                    }
                    break;
                case eDebugLogMode._Exception:
                    {
                        Debug.LogException(exception, context);    
                    }
                    break;
            }
            
        }

    }

	const string start_color_White 	= "<color=white>";
	const string start_color_Red 	= "<color=red>";
	const string start_color_Yellow = "<color=yellow>";
	const string start_color_Green 	= "<color=green>";
	const string start_color_Blue 	= "<color=blue>";
	const string end_color 			= "</color>";
	static StringBuilder _strBD = new StringBuilder ("", 1000);

	public static void LogBuilder(string message)
	{
		_strBD.Append ("[Thr:");
		_strBD.Append (System.AppDomain.GetCurrentThreadId ().ToString ("00000"));
		_strBD.Append ("] ");

		_strBD.Append (start_color_White);
		_strBD.Append (message);
		_strBD.Append (end_color);
		_strBD.AppendLine ();

		Debug.Log(_strBD); 

	}

	public static void LogBool(bool boolColor, object message)
	{
		if (true == boolColor) 
		{
			_BaseLog("<color=white>"+message+"</color>",null,null, eDebugLogMode._None);
		} 
		else 
		{
			_BaseLog("<color=red>"+message+"</color>",null,null, eDebugLogMode._None);
		}
	}


	public static void LogWhite(object message)
	{
		_BaseLog("<color=white>"+message+"</color>",null,null, eDebugLogMode._None);
	}
	public static void LogYellow(object message)
	{
		_BaseLog("<color=yellow>"+message+"</color>",null,null, eDebugLogMode._None);
	}
	public static void LogGreen(object message)
	{
		_BaseLog("<color=green>"+message+"</color>",null,null, eDebugLogMode._None);
	}
	public static void LogRed(object message)
	{
		_BaseLog("<color=red>"+message+"</color>",null,null, eDebugLogMode._None);
	}
	public static void LogBlue(object message)
	{
		_BaseLog("<color=blue>"+message+"</color>",null,null, eDebugLogMode._None);
	}

	public static void Log(object message)
    {
        
        _BaseLog(message,null,null, eDebugLogMode._None);
    }

    public static void Log(object message, Object context)
    {
        _BaseLog(message, context,null, eDebugLogMode._None);
    }

    public static void LogError(object message)
    {
        _BaseLog(message, null,null, eDebugLogMode._Error);
    }

    public static void LogError(object message, Object context)
    {
        _BaseLog(message, context,null, eDebugLogMode._Error);
    }

    public static void LogException(System.Exception exception)
    {
       _BaseLog(null, null,exception, eDebugLogMode._Exception);
    }

    public static void LogException(System.Exception exception, Object context)
    {
        _BaseLog(null, context,exception, eDebugLogMode._Exception);
    }

    public static void LogWarning(object message)
    {
        _BaseLog(message, null,null, eDebugLogMode._Waring);
    }

    public static void LogWarning(object message, Object context)
    {
        _BaseLog(message, context,null, eDebugLogMode._Waring);
    }


    //==================================================================
    //==================================================================
    //==================================================================
    //==================================================================


    static public void DrawLine(Vector3 start, Vector3 end, Color cc)
    {
#if UNITY_EDITOR
        Gizmos.color = cc;
        Gizmos.DrawLine(start, end);
#endif
    }

    static public void DrawCircle(Vector3 pos, float radius, Color cc)
    {
#if UNITY_EDITOR
        Gizmos.color = cc;
        Gizmos.DrawWireSphere(pos, radius);
#endif
    }

    static public void DrawCube(Vector3 pos, Vector3 size, Color cc)
    {
#if UNITY_EDITOR
        Gizmos.color = cc;
        Gizmos.DrawWireCube(pos, size);
        Mesh mesh = new Mesh();
#endif
    }

    //ref : http://ilkinulas.github.io/development/unity/2016/04/30/cube-mesh-in-unity3d.html
    static public Mesh CreateCubeMesh()
    {
        Vector3[] vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
        };

        int[] triangles = {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            0, 6, 7, //face bottom
            0, 1, 6
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }
    static Mesh __cubeMesh = null;

    static public void DrawCube(Vector3 pos, Quaternion rotation, Vector3 size, Color cc)
    {
#if UNITY_EDITOR
        Gizmos.color = cc;

        if (null == __cubeMesh) __cubeMesh = CreateCubeMesh();
        Gizmos.DrawWireMesh(__cubeMesh, pos, rotation, size);
#endif
    }

    static public void PrintText(Vector3 pos, Color cc, string text)
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = cc;

        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.Label(pos, text, style);
        UnityEditor.Handles.EndGUI();
#endif
    }

    static public void DrawCirclePlane(Vector3 pos, float radius, Vector3 up, Color cc)
    {
#if UNITY_EDITOR
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        Vector3 perp = Vector3.Cross(up, Vector3.forward);
        perp.Normalize();

        int count = 36;
        for (int i = 0; i < count; i++)
        {
            Vector3 tdDir = Quaternion.AngleAxis(i * 10, up) * perp;

            cur = pos + tdDir * radius;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, cc);

            //if (0 == i%5)
            //DebugWide.DrawLine(pos, cur, cc);

            prev = cur;
        }
        DebugWide.DrawLine(pos, pos + up, cc);

#endif
    }

    static public void DrawCircleCone(Vector3 pos, float radius, Vector3 up, Color cc)
    {
#if UNITY_EDITOR
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        Vector3 perp = Vector3.Cross(up, Vector3.forward);
        perp.Normalize();

        int count = 36;
        for (int i = 0; i < count; i++)
        {
            Vector3 tdDir = Quaternion.AngleAxis(i * 10, up) * perp;

            cur = pos + tdDir * radius;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, cc);

            //if (0 == i%2)
            DebugWide.DrawLine(pos, cur, cc);

            prev = cur;
        }
#endif
    }


    static public void DrawArc(Vector3 origin, Vector3 pos1, Vector3 pos2, Vector3 upDir ,Color cc)
    {
#if UNITY_EDITOR
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        Vector3 startO = pos1 - origin;
        //float len = startO.magnitude;
        //startO /= len; //노멀 구할 필요 없음 

        float angle = UtilGS9.Geo.Angle360_AxisRotate(pos1-origin, pos2-origin, upDir);

        int COUNT = 20;
        float angleDiv = angle / COUNT;
        for (int i = 0; i < COUNT; i++)
        {
            Vector3 tdDir = Quaternion.AngleAxis(angleDiv * i, upDir) * startO;

            //cur = origin + tdDir * len;
            cur = origin + tdDir;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, cc);

            //if (0 == i%5)
            //DebugWide.DrawLine(pos, cur, cc);

            prev = cur;
        }
        DebugWide.DrawLine(origin, origin + upDir, cc);
        DebugWide.DrawLine(origin, pos1, cc);
        DebugWide.DrawLine(origin, pos2, cc);

#endif
    }

    static public void DrawArc(Vector3 origin, Vector3 pos1, Vector3 pos2, Vector3 upDir, float length, Color cc , string text)
    {
#if UNITY_EDITOR
        Vector3 prev = Vector3.zero;
        Vector3 cur = Vector3.zero;
        Vector3 startO = pos1 - origin;
        startO = UtilGS9.VOp.Normalize(startO);

        float angle = UtilGS9.Geo.Angle360_AxisRotate(pos1 - origin, pos2 - origin, upDir);

        int COUNT = 20;
        float angleDiv = angle / COUNT;
        for (int i = 0; i <= COUNT; i++)
        {
            Vector3 tdDir = Quaternion.AngleAxis(angleDiv * i, upDir) * startO;

            cur = origin + tdDir * length;

            if (0 != i)
                DebugWide.DrawLine(prev, cur, cc);

            if (0 == i)
            {
                DebugWide.DrawLine(origin, cur, cc);
                DebugWide.PrintText(cur, cc, text);
            }
                
            if(COUNT == i)
                DebugWide.DrawLine(origin, cur, cc);
            //if (0 == i%5)
            //DebugWide.DrawLine(pos, cur, cc);

            prev = cur;
        }
        DebugWide.DrawLine(origin, origin + upDir, cc);

#endif
    }

}

