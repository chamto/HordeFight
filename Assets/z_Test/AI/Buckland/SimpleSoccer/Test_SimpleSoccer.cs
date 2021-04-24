using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;


namespace Test_SimpleSoccer
{

    public class Test_SimpleSoccer : MonoBehaviour
    {
        Transform _tr_1 = null;
        Transform _tr_2 = null;
        Transform _tr_3 = null;
        Transform _tr_up = null;

        Main_SimpleSoccer _main = new Main_SimpleSoccer();

        private void Start()
        {
            _tr_1 = GameObject.Find("tr_1").transform;
            _tr_2 = GameObject.Find("tr_2").transform;
            _tr_3 = GameObject.Find("tr_3").transform;
            _tr_up = GameObject.Find("tr_up").transform;

            _main.Init();

            //Test_Type();
        }

        private void Update()
        {
            _main.Key();
            _main.Update();
        }

        private void OnGUI()
        {
            if (null == (object)_tr_1) return;
            _main.OnGUI();
        }

        private void OnDrawGizmos()
        {
            if (null == (object)_tr_1) return;
            _main.Render();
            //Test_TangentPoints();
        }


        private void Test_TangentPoints()
        {
            if (null == (object)_tr_1) return;

            DebugWide.DrawLine(_tr_1.position, _tr_2.position, Color.black);
            DebugWide.DrawLine(_tr_2.position, _tr_3.position, Color.gray);

            float r = (_tr_2.position - _tr_3.position).magnitude;
            DebugWide.DrawCircle(_tr_2.position, r, Color.gray);

            Vector3 p1, p2;
            //Geometry.GetTangentPointsXZ(_tr_2.position, r, _tr_1.position, out p1, out p2);
            Geo.GetTangentPoints(_tr_2.position, r, _tr_up.position.normalized, _tr_1.position, out p1, out p2);
            DebugWide.DrawCircle(p1, 2.5f, Color.red);
            DebugWide.DrawCircle(p2, 2.5f, Color.red);

            DebugWide.DrawLine(_tr_2.position, p1, Color.green);
            DebugWide.DrawLine(_tr_1.position, p1, Color.green);
        }

        public void Test_Type()
        {
            State<FieldPlayer> a = GlobalPlayerState.instance;
            State<FieldPlayer> b = ChaseBall.instance;
            //State<PlayerBase> c = (State<PlayerBase>)TendGoal.instance; //변환안됨 

            DebugWide.LogBlue(a.GetType() + "  " + b.GetType());
            DebugWide.LogBlue(a.GetType() == b.GetType());
        }
    }

    public class Main_SimpleSoccer
    {
        SoccerPitch g_SoccerPitch = null;

        public void Init()
        {
            g_SoccerPitch = new SoccerPitch(700, 400);
        }

        public void Key()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                g_SoccerPitch = new SoccerPitch(700, 400);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                g_SoccerPitch.TogglePause();
            }
        }

        public void Update()
        {
            g_SoccerPitch.Update();
        }
        public void Render()
        {
            g_SoccerPitch.Render();
        }

        public void OnGUI()
        {
            int count = 0;
            int x = 120, y = 20;
            if (GUI.Button(new Rect(x * count, 0, x, y), new GUIContent("1 AIDS_NOAIDS")))
            {
                Prm.ViewStates = false;
                Prm.ViewRegions = false;
                Prm.ViewIDs = false;
                Prm.ViewSupportSpots = false;
                Prm.bViewTargets = false;

            }
            count++;
            if (GUI.Button(new Rect(x * count, 0, x, y), new GUIContent("2 SHOW_REGIONS")))
            {
                Prm.ViewRegions = !Prm.ViewRegions;
            }
            count++;
            if (GUI.Button(new Rect(x * count, 0, x, y), new GUIContent("3 SHOW_STATES")))
            {
                Prm.ViewStates = !Prm.ViewStates;

            }
            count = 0;
            if (GUI.Button(new Rect(x * count, y, x, y), new GUIContent("4 SHOW_IDS")))
            {
                Prm.ViewIDs = !Prm.ViewIDs;
            }
            count++;
            if (GUI.Button(new Rect(x * count, y, x, y), new GUIContent("5 AIDS_SUPPORTSPOTS")))
            {
                Prm.ViewSupportSpots = !Prm.ViewSupportSpots;
            }
            count++;
            if (GUI.Button(new Rect(x * count, y, x, y), new GUIContent("6 AIDS_SHOWTARGETS")))
            {
                Prm.bViewTargets = !Prm.bViewTargets;
            }
            count = 0;
            if (GUI.Button(new Rect(x * count, y * 2, x, y), new GUIContent("7 AIDS_HIGHLITE")))
            {
                Prm.bHighlightIfThreatened = !Prm.bHighlightIfThreatened;
            }
            count++;
            if (GUI.Button(new Rect(x * count, y * 2, x, y), new GUIContent("8 bNonPenetrationConstraint")))
            {
                Prm.bNonPenetrationConstraint = !Prm.bNonPenetrationConstraint;
            }
            count++;
            if (GUI.Button(new Rect(x * count, y * 2, x, y), new GUIContent("9 SHOW_TEAM_STATE")))
            {
                Prm.SHOW_TEAM_STATE = !Prm.SHOW_TEAM_STATE;
            }
            count = 0;
            if (GUI.Button(new Rect(x * count, y * 3, x, y), new GUIContent("10 SHOW_SUPPORTING_PLAYERS_TARGET")))
            {
                Prm.SHOW_SUPPORTING_PLAYERS_TARGET = !Prm.SHOW_SUPPORTING_PLAYERS_TARGET;
            }
            count++;

        }
    }

    public static class AutoList<T>
    {
        static LinkedList<T> _allMember = new LinkedList<T>();

        public static void Reset()
        {
            _allMember.Clear();
        }

        public static void Add(T data)
        {
            _allMember.AddLast(data);
        }

        public static LinkedList<T> GetAllMembers()
        {
            return _allMember;
        }
    }

    public class Const
    {
        //--------------------------- Constants ----------------------------------
        public const float Pi = 3.14159f;
        public const float TwoPi = Pi * 2f;
        public const float HalfPi = Pi / 2f;
        public const float QuarterPi = Pi / 4f;
        //------------------------------------------------------------------------ 

        public const int TeamSize = 5;
    }

    public enum player_role { goal_keeper, attacker, defender };

    public enum MessageType
    {
        Msg_ReceiveBall,
        Msg_PassToMe,
        Msg_SupportAttacker,
        Msg_GoHome,
        Msg_Wait
    }


}//end namespace

