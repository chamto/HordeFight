﻿using System.Collections.Generic;
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


            _bot_0 = new Raven_Bot(SingleO.raven, ConstV.v3_zero);
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

            _bot_0.Render();

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
        wall = 0 ,
        bot = 1,
        unused = 2,
        waypoint = 3,
        health = 4,
        spawn_point = 5,
        rail_gun = 6,
        rocket_launcher = 7,
        shotgun = 8,
        blaster = 9,
        obstacle = 10,
        sliding_door = 11,
        door_trigger = 12
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
        public const int FrameRate = 60;

        public const float SEND_MSG_IMMEDIATELY = 0.0f;
        public const int NO_ADDITIONAL_INFO = 0;
        public const int SENDER_ID_IRRELEVANT = -1;

        //the radius of the constraining circle for the wander behavior
        public const float WanderRad = 1.2f;
        //distance the wander circle is projected in front of the agent
        public const float WanderDist = 2.0f;
        //the maximum amount of displacement along the circle each frame
        public const float WanderJitterPerSec = 40.0f;
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

    public class Transformation
    {

        static public void Draw_WorldTransform(List<Vector3> points,
                                  Vector3 pos,
                                  Vector3 forward,
                                  Vector3 side,
                                  Vector3 scale,
                                  Color color)
        {
            Vector3 up = Vector3.Cross(forward, side);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, side); //열값 삽입
            m.SetColumn(1, up);
            m.SetColumn(2, forward);

            //Quaternion rotQ = Quaternion.FromToRotation(ConstV.v3_forward, forward);

            int count = 0;
            Vector3 tr, cur, prev = ConstV.v3_zero;
            foreach (Vector3 v in points)
            {
                tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
                //cur = (rotQ * tr) + pos;
                cur = m.MultiplyPoint(tr) + pos;

                if (count > 1)
                {
                    DebugWide.DrawLine(prev, cur, color);
                }

                prev = cur;

                count++;
            }
        }

        static public List<Vector3> WorldTransform(List<Vector3> points,
                                  Vector3   pos,
                                  Vector3   forward,
                                  Vector3   side,
                                  Vector3   scale)
        {
            Vector3 up = Vector3.Cross(forward, side);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, side); //열값 삽입
            m.SetColumn(1, up);
            m.SetColumn(2, forward);

            //Quaternion rotQ = Quaternion.FromToRotation(ConstV.v3_forward, forward);

            List<Vector3> list = new List<Vector3>();

            Vector3 tr;
            foreach (Vector3 v in points)
            {
                tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
                //list.Add((rotQ * tr) + pos);
                list.Add((m.MultiplyPoint(tr)) + pos);
            }
            return list;

        }

        static public Vector3 PointToWorldSpace(Vector3 point,
                                    Vector3 AgentHeading,
                                    Vector3 AgentSide,
                                    Vector3 AgentPosition)
        {
            Vector3 AgentUp = Vector3.Cross(AgentHeading, AgentSide);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, AgentSide); //열값 삽입
            m.SetColumn(1, AgentUp);
            m.SetColumn(2, AgentHeading);

            //Quaternion rotQ = Quaternion.FromToRotation(ConstV.v3_forward, AgentHeading);

            return m.MultiplyPoint(point) + AgentPosition;
        }

        static public void Vec2DRotateAroundOrigin(ref Vector3 v, float ang)
        {
            Quaternion rotQ = Quaternion.AngleAxis(ang, ConstV.v3_up);
            v = rotQ * v;
        }

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


            m_pOwner.GetSteering().SetTarget(m_vPosition);
            m_pOwner.GetSteering().SeekOn();
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
            m_pOwner.GetSteering().SeekOff();
            m_pOwner.GetSteering().ArriveOff();

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

        public Raven_TargetingSystem(Raven_Bot bot) { }

        public void Update() { }

        public bool isTargetPresent() {return m_pCurrentTarget != null;}
        public bool isTargetShootable() { return false; }
        public Raven_Bot GetTarget() {return m_pCurrentTarget;}
        public void ClearTarget() { }
    }

    public class Raven_Weapon
    {
        int m_iNumRoundsLeft;
        public int NumRoundsRemaining() {return m_iNumRoundsLeft;}
    }

    public class Raven_WeaponSystem
    {
        public Raven_WeaponSystem(Raven_Bot bot , float a , float b , float c) { }

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

        public void RenderDesirabilities() { }
        public void RenderCurrentWeapon() { }
        public void SelectWeapon() { }
        public void TakeAimAndShoot() { }

        public void ShootAt(Vector3 p) { }
        public void ChangeWeapon(int i) { }
        public void Initialize() { }
    }

    //======================================================

    

    public class GraveMarkers 
    {
        public GraveMarkers(float v) { }

        public void Update() { }
        public void Render() { }

        public void AddGrave(Vector3 bot) { }
    }

    public class Raven_Door
    {
        public int ID() { return 0; }
        public List<int> GetSwitchIDs() { return null; }
        public void Update() { }
    }

    public class Raven_Projectile : MovingEntity
    {
        public Raven_Projectile(Vector3 target,   //the target's position
                   Raven_Game world,  //a pointer to the world data
                   int ShooterID, //the ID of the bot that fired this shot
                   Vector3 origin,  //the start position of the projectile
                   Vector3 heading,   //the heading of the projectile
                   int damage,    //how much damage it inflicts
                   float scale,
                   float MaxSpeed,
                   float mass,
                   float MaxForce) :  base(origin,
                                                   scale,
                                                   ConstV.v3_zero,
                                                     MaxSpeed,
                                                     heading,
                                                     mass,
                                                     new Vector3(scale, scale, scale),
                                                     0, //max turn rate irrelevant here, all shots go straight
                                                     MaxForce)

                                        
            {
                                        //m_vTarget(target),
                                        //m_bDead(false),
                                        //m_bImpacted(false),
                                        //m_pWorld(world),
                                        //m_iDamageInflicted(damage),
                                        //m_vOrigin(origin),
                                        //m_iShooterID(ShooterID) 
            }

            public bool isDead() { return false; }
    }
    public class Rocket : Raven_Projectile
    { 
        public Rocket(Raven_Bot shooter, Vector3 target)
                         :base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         1,1,1,1,1)
        { } 
    }
    public class Slug : Raven_Projectile
    { 
        public Slug(Raven_Bot shooter, Vector3 target)
        : base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         1, 1, 1, 1, 1)
        { } 
    }
    public class Pellet : Raven_Projectile
    { 
        public Pellet(Raven_Bot shooter, Vector3 target)
            : base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         1, 1, 1, 1, 1)
        { } 
    }
    public class Bolt : Raven_Projectile
    { 
        public Bolt(Raven_Bot shooter, Vector3 target)
            : base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         1, 1, 1, 1, 1)
        { }
    }

    //======================================================

    public class PathManager<path_planner>
    {
        public PathManager(float a) { }

        public void Register(path_planner pPathPlanner)
        { }
        public void UnRegister(path_planner pPathPlanner)
        {
            //m_SearchRequests.remove(pPathPlanner);

        }
        public void UpdateSearches() { }
    }

    public class Path : LinkedList<PathEdge> { }

    public class NavGraph : SparseGraph<NavGraphNode, NavGraphEdge>
    {
        public NavGraph(bool digraph) : base(digraph)
        {
        }
    }

    //======================================================

    //*
    public class Raven_Map
    {
        List<Wall2D> m_Walls;
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

        public TriggerList GetTriggers() { return m_TriggerSystem.GetTriggers(); }
        public Vector3 GetRandomNodeLocation()
        {
            //return ConstV.v3_zero;

            return new Vector3(Misc.RandFloat() * 30, 0, Misc.RandFloat() * 30); //임시로 값 부여 
        }
        public NavGraph GetNavGraph() { return m_pNavGraph; }
        public float GetCellSpaceNeighborhoodRange() { return m_dCellSpaceNeighborhoodRange; }
        public CellSpacePartition<NavGraphNode> GetCellSpace() { return m_pSpacePartition; }
        public float CalculateCostToTravelBetweenNodes(int nd1, int nd2) { return 0f; }

        public List<Wall2D> GetWalls() { return m_Walls; }
    }
    //*/

    //======================================================

}//end namespace

