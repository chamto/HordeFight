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


            _bot_0 = new Raven_Bot(SingleO.raven, 0);
        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        private void OnDrawGizmos()
        {
            if (null == _bot_0) return;
            _bot_0.Update();
            _bot_0.GetBrain().Render(); //위치출력
            _bot_0.GetBrain().RenderAtPos(ConstV.v3_zero); //복합목표 출력
            _bot_0.GetBrain().RenderEvaluations(new Vector3(0,0,15)); //생각을 위한 평가값들 출력

        }
    }

    //======================================================

    public static class SingleO
    {
        public static EntityManager entityMgr = null;
        public static MessageDispatcher dispatcher = null;
        public static Raven_Game raven = null;

        public static void Init()
        {
            entityMgr = new EntityManager();
            dispatcher = new MessageDispatcher();
            raven = new Raven_Game();
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

    public class Const
    {
        public const float SEND_MSG_IMMEDIATELY = 0.0f;
        public const int NO_ADDITIONAL_INFO = 0;
        public const int SENDER_ID_IRRELEVANT = -1;
    }

    //======================================================

    public static class UserOptions
    {

        public static bool m_bShowGraph = false;

        public static bool m_bShowNodeIndices = false;

        public static bool m_bShowPathOfSelectedBot = true;

        public static bool m_bShowTargetOfSelectedBot = false;

        public static bool m_bShowOpponentsSensedBySelectedBot = true;

        public static bool m_bOnlyShowBotsInTargetsFOV = false;

        public static bool m_bShowGoalsOfSelectedBot = true;

        public static bool m_bShowGoalAppraisals = true;

        public static bool m_bShowWeaponAppraisals = false;

        public static bool m_bSmoothPathsQuick = false;
        public static bool m_bSmoothPathsPrecise = false;

        public static bool m_bShowBotIDs = false;

        public static bool m_bShowBotHealth = true;

        public static bool m_bShowScore = false;
    }
    //======================================================

    public class Params
    {
        public const int FrameRate = 60;

        public const float RocketLauncher_MaxRoundsCarried = 50f;
        public const float RailGun_MaxRoundsCarried = 50f;
        public const float ShotGun_MaxRoundsCarried = 50f;

        public const int Bot_MaxHealth = 100;
        public const float Bot_MaxSpeed = 1f;
        public const float Bot_Mass = 1f;
        public const float Bot_MaxForce = 1.0f;
        public const float Bot_MaxHeadTurnRate = 0.2f;
        public const float Bot_Scale       = 0.8f;
    }

    //======================================================
    public class Goal_SeekToPosition : Goal<Raven_Bot>
    {

        //the position the bot is moving to
        Vector3 m_vPosition;

        //the approximate time the bot should take to travel the target location
        float m_dTimeToReachPos;

        //this records the time this goal was activated
        float m_dStartTime;

        //returns true if a bot gets stuck
        bool isStuck()
        {  

          float TimeTaken = Time.time - m_dStartTime;

          if (TimeTaken > m_dTimeToReachPos)
          {
                //debug_con << "BOT " << m_pOwner->ID() << " IS STUCK!!" << "";
                DebugWide.LogBlue("BOT " + m_pOwner.ID() + " IS STUCK!!");

                return true;
          }

          return false;
        }


        public Goal_SeekToPosition(Raven_Bot pBot, Vector3 target):base(pBot, (int)eGoal.seek_to_position) 
        {
            m_vPosition = target;
            m_dTimeToReachPos = 0f;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //record the time the bot starts this goal
            m_dStartTime = Time.time;

            //This value is used to determine if the bot becomes stuck 
            m_dTimeToReachPos = m_pOwner.CalculateTimeToReachPosition(m_vPosition);

            //factor in a margin of error for any reactive behavior
            const float MarginOfError = 1.0f;

            m_dTimeToReachPos += MarginOfError;


            //m_pOwner.GetSteering().SetTarget(m_vPosition);
            //m_pOwner.GetSteering().SeekOn();
        }
        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //test to see if the bot has become stuck
            if (isStuck())
            {
                m_iStatus = (int)eStatus.failed;
            }

            //test to see if the bot has reached the waypoint. If so terminate the goal
            else
            {
                if (m_pOwner.isAtPosition(m_vPosition))
                {
                    m_iStatus = (int)eStatus.completed;
                }
            }

            return m_iStatus;
        }
        override public void Terminate()
        {
            //m_pOwner.GetSteering().SeekOff();
            //m_pOwner.GetSteering().ArriveOff();

            m_iStatus = (int)eStatus.completed;
        }

        override public void Render()
        {
            //DebugWide.LogBlue((eStatus)m_iStatus);
            if (m_iStatus == (int)eStatus.active)
            {
                DebugWide.DrawCircle(m_vPosition, 3, Color.green);
            }

            else if (m_iStatus == (int)eStatus.inactive)
            {
                DebugWide.DrawCircle(m_vPosition, 3, Color.red);
            }
        }
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

    

    //======================================================

    public class Trigger<entity_type> : BaseGameEntity
    {
        public Trigger(int id) : base(id) { }
        public Vector3 Pos() { return ConstV.v3_zero; }
        public bool isActive() { return false; }
        public int GraphNodeIndex() { return -1; }
    }

    public class TriggerList : LinkedList<Trigger<Raven_Bot>> 
    {
    }

    public class TriggerSystem<trigger_type>
    {
        public TriggerList GetTriggers(){ return null; }
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

    public class PathManager<path_planner>
    {
        public void Register(path_planner pPathPlanner)
        { }
        public void UnRegister(path_planner pPathPlanner)
        {
            //m_SearchRequests.remove(pPathPlanner);

        }
    }

    public class Path : LinkedList<PathEdge> { }

    public class NavGraph : SparseGraph<NavGraphNode, NavGraphEdge>
    {
        public NavGraph(bool digraph) : base(digraph)
        {
        }
    }


    public class Raven_Map
    {

        NavGraph m_pNavGraph = new NavGraph(false);
        CellSpacePartition<NavGraphNode> m_pSpacePartition;
        float m_dCellSpaceNeighborhoodRange;

        TriggerSystem<Trigger<Raven_Bot>> m_TriggerSystem;

        public Raven_Map()
        {
            m_pSpacePartition = new CellSpacePartition<NavGraphNode>(100,
                                                                  100,
                                                                  10,
                                                                  10,
                                                                  m_pNavGraph.NumNodes());


        }

        public TriggerList GetTriggers() {return m_TriggerSystem.GetTriggers();}
        public Vector3 GetRandomNodeLocation() 
        {
            //return ConstV.v3_zero;
            return new Vector3(30,0,30); //임시로 값 부여 
        } 
        public NavGraph GetNavGraph() {return m_pNavGraph;}
        public float GetCellSpaceNeighborhoodRange() {return m_dCellSpaceNeighborhoodRange;}
        public CellSpacePartition<NavGraphNode> GetCellSpace() {return m_pSpacePartition;}
        public float CalculateCostToTravelBetweenNodes(int nd1, int nd2) { return 0f; }
    }

    public class Raven_Game
    {
        Raven_Map m_pMap = new Raven_Map();
        PathManager<Raven_PathPlanner> m_pPathManager = new PathManager<Raven_PathPlanner>();

        public Raven_Map GetMap() { return m_pMap; }
        public PathManager<Raven_PathPlanner> GetPathManager() { return m_pPathManager; }
    }


}//end namespace

