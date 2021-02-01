using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{
    public class Test_Raven : MonoBehaviour
    {

        Raven_Bot _bot_0 = null;

        // Use this for initialization
        void Start()
        {
            SingleO.Init();

            _bot_0 = new Raven_Bot(0);
        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        private void OnDrawGizmos()
        {
            if (null == _bot_0) return;
            _bot_0.Update();
            //_bot_0.GetBrain().Render();
            _bot_0.GetBrain().RenderAtPos(ConstV.v3_zero);
            //_bot_0.GetBrain().RenderEvaluations(5, 5);

        }
    }

    //======================================================

    public static class SingleO
    {
        public static EntityManager entityMgr = null;
        public static MessageDispatcher dispatcher = null;

        public static void Init()
        {
            entityMgr = new EntityManager();
            dispatcher = new MessageDispatcher();
        }
    }

    public enum eObjType
    {
        wall,
        bot,
        unused,
        waypoint,
        health,
        spawn_point,
        rail_gun,
        rocket_launcher,
        shotgun,
        blaster,
        obstacle,
        sliding_door,
        door_trigger
    }

    public enum eGoal
    {
        think,
        explore,
        arrive_at_position,
        seek_to_position,
        follow_path,
        traverse_edge,
        move_to_position,
        get_health,
        get_shotgun,
        get_rocket_launcher,
        get_railgun,
        wander,
        negotiate_door,
        attack_target,
        hunt_target,
        strafe,
        adjust_range,
        say_phrase

    }

    public enum eMsg
    {
        Blank,
        PathReady,
        NoPathAvailable,
        TakeThatMF,
        YouGotMeYouSOB,
        GoalQueueEmpty,
        OpenSesame,
        GunshotSound,
        UserHasRemovedBot
    }

    //======================================================


    //======================================================

    public class Params
    {
        public const float RocketLauncher_MaxRoundsCarried = 50f;
        public const float RailGun_MaxRoundsCarried = 50f;
        public const float ShotGun_MaxRoundsCarried = 50f;
    }

    //======================================================
    public class Goal_SeekToPosition : Goal<Raven_Bot>
    {

        //the position the bot is moving to
        Vector3 m_vPosition;

        //the approximate time the bot should take to travel the target location
        double m_dTimeToReachPos;

        //this records the time this goal was activated
        double m_dStartTime;

        //returns true if a bot gets stuck
        //bool isStuck() { return false; }


        public Goal_SeekToPosition(Raven_Bot pBot, Vector3 target):base(pBot, (int)eGoal.seek_to_position) { }

        //the usual suspects
        //override public void Activate() { }
        //override public int Process() { return 0; }
        //override public void Terminate() { }

        //override public void Render() { }
    }

    public class Goal_FollowPath : Goal_Composite<Raven_Bot>
    {
        //a local copy of the path returned by the path planner
        Path m_Path;


        public Goal_FollowPath(Raven_Bot pBot, Path path) : base(pBot, (int)eGoal.seek_to_position) {}

        //the usual suspects
        //override public void Activate() { }
        //override public int Process() { return 0; }
        //override public void Render() { }
        //override public void Terminate() { }
    }

    public class Goal_Wander : Goal<Raven_Bot>
    {
    
        public Goal_Wander(Raven_Bot pBot) :base(pBot, (int)eGoal.wander) {}

        //override public void Activate() { }
        //override public int Process() { return 0; }
        //override public void Terminate() { }
    }

    public class Goal_HuntTarget : Goal_Composite<Raven_Bot>
    {
    
        //this value is set to true if the last visible position of the target
        //bot has been searched without success
         bool m_bLVPTried;


        public Goal_HuntTarget(Raven_Bot pBot) :base(pBot, (int)eGoal.hunt_target)
        {
            m_bLVPTried = false;
        }

        //the usual suspects
        //void Activate();
        //int Process();
        //void Terminate() { }
        //void Render();
    }

    public class Goal_DodgeSideToSide : Goal<Raven_Bot>
    {
    
        Vector3 m_vStrafeTarget;

        bool m_bClockwise;

        //Vector3 GetStrafeTarget();


        public Goal_DodgeSideToSide(Raven_Bot pBot) :base(pBot, (int)eGoal.strafe)
        {
            m_bClockwise = Misc.RandBool();
        }


        //void Activate();
        //int Process();
        //void Render();
        //void Terminate();

    }
    //======================================================

    public class Trigger<Raven_Bot>
    {
        public Vector3 Pos() { return ConstV.v3_zero; }
        public bool isActive() { return false; }
    }

    public class PathEdge { }
    public class Path : LinkedList<PathEdge> { }

    public class Raven_PathPlanner
    {
        public Path GetPath() { return null; }
        public float GetCostToClosestItem(int GiverType) { return 0;  }
        public bool RequestPathToPosition(Vector3 TargetPos) { return false; }
        public bool RequestPathToItem(int ItemType) { return false; }
    }

    public class Raven_TargetingSystem
    {
        //the owner of this system
        Raven_Bot m_pOwner;

        //the current target (this will be null if there is no target assigned)
        Raven_Bot m_pCurrentTarget;

        public bool isTargetPresent() {return m_pCurrentTarget != null;}
        public bool isTargetShootable() { return false; }
        public Raven_Bot GetTarget() {return m_pCurrentTarget;}
}

    public class Raven_Weapon
    {
        int m_iNumRoundsLeft;
        public int NumRoundsRemaining() {return m_iNumRoundsLeft;}
    }

    public class Raven_WeaponSystem
    {
        public Raven_Weapon GetWeaponFromInventory(int weapon_type)
        {

            //return m_WeaponMap[weapon_type];
            return null;
        }

        public int GetAmmoRemainingForWeapon(int weapon_type)
        {
            //if (m_WeaponMap[weapon_type])
            //{
            //    return m_WeaponMap[weapon_type]->NumRoundsRemaining();
            //}

            return 0;
        }
    }

    public class Raven_Game
    {
        public Raven_Map GetMap() { return null; }
    }

    public class Raven_Map
    {
        public Vector3 GetRandomNodeLocation() { return ConstV.v3_zero; }
    }

    public class Raven_Bot : BaseGameEntity
    {
    
        Goal_Think m_pBrain;
        //the bot uses this to plan paths
        Raven_PathPlanner m_pPathPlanner;

        Raven_TargetingSystem m_pTargSys;
        Raven_WeaponSystem m_pWeaponSys;
        Raven_Game m_pWorld;

        int m_iHealth;
        int m_iMaxHealth;

        //set to true when a human player takes over control of the bot
        bool m_bPossessed;

        public Raven_Bot(int id) : base(id) 
        {
            m_pBrain = new Goal_Think(this);
        }


        override public void Update()
        {
            m_pBrain.Process();
        }

        public Vector3 Pos() { return ConstV.v3_zero; }

        public int Health() {return m_iHealth;}
        public int MaxHealth() {return m_iMaxHealth;}

        public bool isPossessed() {return m_bPossessed;}
        public bool hasLOSto(Vector3 pos) { return false; }

        public Raven_Game  GetWorld(){return m_pWorld;}
        public Raven_TargetingSystem GetTargetSys() {return m_pTargSys;}
        public Raven_WeaponSystem GetWeaponSys() {return m_pWeaponSys;}
        public Goal_Think  GetBrain(){return m_pBrain;}
        public Raven_PathPlanner  GetPathPlanner(){return m_pPathPlanner;}
        public Raven_Bot GetTargetBot() { return m_pTargSys.GetTarget(); }
        public bool canStepLeft(Vector3 PositionOfStep) { return false; }
        public bool canStepRight(Vector3 PositionOfStep) { return false; }

    }


}//end namespace

