using System;
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
            //_main.Update();
        }

        private void OnGUI()
        {
            if (null == (object)_tr_1) return;
            _main.OnGUI();
        }

        private void OnDrawGizmos()
        {
            if (null == (object)_tr_1) return;

            _main.Update();
            _main.Render();

            //Test_TangentPoints();

            //Test_isPassSafeFromOpponent();
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


        public void Test_isPassSafeFromOpponent()
        {
            //Vector3 ToBall = player.Ball().Pos() - player.Pos();
            //float dot = Vector3.Dot(player.Heading(), ToBall.normalized);
            float dot = 1f;
            float power = Prm.MaxShootingForce * dot;

            bool result = isPassSafeFromOpponent(_tr_1.position, _tr_2.position, Vector3.zero, _tr_3.position, power, false);
            DebugWide.LogBlue(result);

            DebugWide.DrawLine(_tr_1.position, _tr_2.position, Color.black);
            DebugWide.DrawLine(_tr_2.position, _tr_3.position, Color.gray);
        }

        public bool isPassSafeFromOpponent(Vector3 from,
                                        Vector3 target,
                                        Vector3 receiver,
                                        Vector3 opp,
                                        float PassingForce,
                                        bool test_receiver)
        {
            //move the opponent into local space.
            Vector3 ToTarget = target - from;
            Vector3 ToTargetNormalized = (ToTarget).normalized;

            Vector3 perp = Vector3.Cross(Vector3.up, ToTargetNormalized);
            //Vector3 perp = Vector3.Cross(ToTargetNormalized, Vector3.up);

            Vector3 LocalPosOpp = Misc.PointToLocalSpace_3(opp,
                                       ToTargetNormalized,
                                       perp,
                                       from);
            DebugWide.LogBlue(LocalPosOpp);
            //if opponent is behind the kicker then pass is considered okay(this is 
            //based on the assumption that the ball is going to be kicked with a 
            //velocity greater than the opponent's max velocity)
            if (LocalPosOpp.z < 0)
            {
                return true;
            }

            //if the opponent is further away than the target we need to consider if
            //the opponent can reach the position before the receiver.
            if ((from - target).sqrMagnitude < (opp - from).sqrMagnitude)
            {
                if (true == test_receiver)
                {
                    if ((target - opp).sqrMagnitude >
                         (target - receiver).sqrMagnitude)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                }
            }

            //calculate how long it takes the ball to cover the distance to the 
            //position orthogonal to the opponents position
            float TimeForBall =
            TimeToCoverDistance(Vector3.zero,
                                                 new Vector3(LocalPosOpp.z, 0, 0),
                                                 PassingForce);
            float opp_maxSpeed = 1.6f;
            float ballSize = 5f;
            float opp_radius = 10f;
            //now calculate how far the opponent can run in this time
            float reach = opp_maxSpeed * TimeForBall +
                          ballSize +
                          opp_radius;

            DebugWide.LogBlue(reach);
            DebugWide.DrawCircle(opp, reach, Color.red);

            //if the distance to the opponent's y position is less than his running
            //range plus the radius of the ball and the opponents radius then the
            //ball can be intercepted
            if (Math.Abs(LocalPosOpp.x) < reach)
            {
                return false;
            }

            return true;
        }

        public float TimeToCoverDistance(Vector3 A, Vector3 B, float force)
        {
            float m_dMass = 1f;
            //this will be the velocity of the ball in the next time step *if*
            //the player was to make the pass. 
            float speed = force / m_dMass;

            //calculate the velocity at B using the equation
            //
            //  v^2 = u^2 + 2as
            //

            //first calculate s (the distance between the two positions)
            float DistanceToCover = (A - B).magnitude;

            float term = speed * speed + 2.0f * DistanceToCover * Prm.Friction;

            //if  (u^2 + 2as) is negative it means the ball cannot reach point B.
            if (term <= 0.0) return -1.0f;

            float v = (float)Math.Sqrt(term);

            //it IS possible for the ball to reach B and we know its speed when it
            //gets there, so now it's easy to calculate the time using the equation
            //
            //    t = v-u
            //        ---
            //         a
            //
            return (v - speed) / Prm.Friction;
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

