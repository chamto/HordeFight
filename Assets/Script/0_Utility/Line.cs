using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UtilGS9
{
    public class Line
    {
        static private Dictionary<string, LineRenderer> _list = new Dictionary<string, LineRenderer>();

        static public LineRenderer GetLine(string keyName)
        {
            return _list[keyName];
        }


        static public LineRenderer AddDebugLine(Transform parent, string keyName)
        //static public LineRenderer AddDebugLine(Transform parent, string keyName, Vector3 pos1, Vector3 pos2)
        {

            if (true == _list.ContainsKey(keyName))
            {
                return null;
            }

            GameObject obj = new GameObject();
            LineRenderer line = obj.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.transform.parent = parent;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.SetWidth(0.05f, 0.1f);
            //line.SetColors (new Color(1,1,1,1), new Color(0,0,1,1));


            line.name = keyName;
            //line.SetPosition (0, pos1); //from
            //line.SetPosition (1, pos2); //to

            _list.Add(keyName, line);


            return line;

        }

        static public void UpdateDebugLineScale(string keyName, Vector3 scale)
        {
            if (true == _list.ContainsKey(keyName))
            {
                _list[keyName].transform.localScale = scale;
            }
        }
        static public void UpdateDebugLine(Transform parent, string keyName, Vector3 pos1, Vector3 pos2)
        {
            Vector3[] array = { pos1, pos2 };
            UpdateDebugLine(parent, keyName, array, Color.white, Color.blue);
        }
        static public void UpdateDebugLine(Transform parent, string keyName, Vector3 pos1, Vector3 pos2, Color col)
        {
            Vector3[] array = { pos1, pos2 };
            UpdateDebugLine(parent, keyName, array, Color.white, col);
        }

        static public void UpdateDebugLine(Transform parent, string keyName, Vector3[] array, Color startCol, Color endCol)
        //static public void UpdateDebugLine(string keyName, Vector3[] array)
        {
            if (false == _list.ContainsKey(keyName))
            {
                AddDebugLine(parent, keyName);
            }

            //_list[keyName].SetColors (new Color(0,1,0,1), new Color(0,0,0,1));
            _list[keyName].SetColors(startCol, endCol);

            _list[keyName].SetVertexCount(array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                _list[keyName].SetPosition(i, array[i]);
            }

        }

        //      static public void UpdateDebugLine(LineRenderer line, Vector3 pos1, Vector3 pos2)
        //      {
        //          line.SetPosition (0, pos1); //from
        //          line.SetPosition (1, pos2); //to
        //      }
    }
}
