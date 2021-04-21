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

        private void Start()
        {
            _tr_1 = GameObject.Find("tr_1").transform;
            _tr_2 = GameObject.Find("tr_2").transform;
            _tr_3 = GameObject.Find("tr_3").transform;
            _tr_up = GameObject.Find("tr_up").transform;

            //Test_Type();
        }

        private void OnDrawGizmos()
        {
            Test_TangentPoints();
        }


        private void Test_TangentPoints()
        {
            if (null == (object)_tr_1) return;

            DebugWide.DrawLine(_tr_1.position, _tr_2.position, Color.black);
            DebugWide.DrawLine(_tr_2.position, _tr_3.position, Color.gray);

            float r = (_tr_2.position - _tr_3.position).magnitude;
            DebugWide.DrawCircle(_tr_2.position, r, Color.gray);

            Vector3 p1, p2;
            Geometry.GetTangentPointsXZ(_tr_2.position, r, _tr_1.position, out p1, out p2);
            //Geo.GetTangentPoints(_tr_2.position, r, _tr_up.position.normalized,  _tr_1.position, out p1, out p2);
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

    public static class AutoList<T>
    {
        static LinkedList<T> _allMember = new LinkedList<T>();

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

