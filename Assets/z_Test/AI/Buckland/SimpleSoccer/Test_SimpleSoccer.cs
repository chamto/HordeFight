using System.Collections.Generic;
using UnityEngine;

namespace Test_SimpleSoccer
{
    public class Test_SimpleSoccer : MonoBehaviour 
    {
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

