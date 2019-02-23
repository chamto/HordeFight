using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_003
{
    public class Test_003_CircleFactory : MonoBehaviour
    {

        public CircleFactory _circleFactory = null;
        public bool _pause = false;
        public float _sphereRadius = 10;
        public eRender_Tree _render_tree = eRender_Tree.ALL1;

        // Use this for initialization
        void Start()
        {
            _circleFactory = new CircleFactory(20);
            _circleFactory.SetState(Circle.State.SHOW_ALL); //CS_SHOW_RAYTRACE , CS_SHOW_FRUSTUM , CS_SHOW_RANGE_TEST

        }

        // Update is called once per frame
        void Update()
        {

            switch (Input.inputString)
            {
                case "a":
                case "A":
                    _circleFactory.SetState(Circle.State.SHOW_ALL);
                    break;
                case "t":
                case "T":
                    _circleFactory.SetState(Circle.State.SHOW_RAYTRACE);
                    break;
                case "f":
                case "F":
                    _circleFactory.SetState(Circle.State.SHOW_FRUSTUM);
                    break;
                case "r":
                case "R":
                    _circleFactory.SetState(Circle.State.SHOW_RANGE_TEST);
                    break;
                case "p":
                case "P":
                    _pause = !_pause;
                    break;
                case "1":
                    _render_tree = _render_tree ^ eRender_Tree.ROOT;
                    break;
                case "2":
                    _render_tree ^= eRender_Tree.LEAF;
                    break;
                case "3":
                    _render_tree ^= eRender_Tree.TEXT;
                    break;
                case "4":
                    _render_tree = eRender_Tree.ALL1;
                    break;
            }

            if (null == _circleFactory) return;

            _circleFactory.SetRenderMode(_render_tree);
            if (false == _pause)
                _circleFactory.Process();
        }

        private void OnDrawGizmos()
        {

            DefineO.DrawCircle(0, 0, _sphereRadius, 0x00ffffff);
            //return;

            if (null == _circleFactory) return;

            //if(false == _pause)
            //_circleFactory.Process();

            _circleFactory.Render();
        }

    }


    //=============================================================

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
            //UnityEditor.Handles.color = Misc.Color32_ToColor(Misc.Hex_ToColor32(color));

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Misc.Color32_ToColor(Misc.Hex_ToColor32(color));

            UnityEditor.Handles.BeginGUI();
            UnityEditor.Handles.Label(new Vector3(x, y, 0), text, style);
            UnityEditor.Handles.EndGUI();
        }
    }
}

