using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{
    public class Test_Raven : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            SingleO.Init();
        }

        // Update is called once per frame
        //void Update()
        //{
        //}
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

    public class Goal_MoveToPosition :  Goal_Composite<Raven_Bot>
    {
    
        //the position the bot wants to reach
        Vector3 m_vDestination;

    
        public Goal_MoveToPosition(Raven_Bot pBot,
                      Vector3 pos) : base(pBot, (int)eGoal.move_to_position)
        {
            m_vDestination = pos;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            //requests a path to the target position from the path planner. Because, for
            //demonstration purposes, the Raven path planner uses time-slicing when 
            //processing the path requests the bot may have to wait a few update cycles
            //before a path is calculated. Consequently, for appearances sake, it just
            //seeks directly to the target position whilst it's awaiting notification
            //that the path planning request has succeeded/failed
            if (m_pOwner.GetPathPlanner().RequestPathToPosition(m_vDestination))
            {
                AddSubgoal(new Goal_SeekToPosition(m_pOwner, m_vDestination));
            }
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //process the subgoals
            m_iStatus = ProcessSubgoals();

            //if any of the subgoals have failed then this goal re-plans
            ReactivateIfFailed();

            return m_iStatus;
        }

        override public void Terminate() { }

        //this goal is able to accept messages
        override public bool HandleMessage(Telegram msg)
        {
            //first, pass the message down the goal hierarchy
            bool bHandled = ForwardMessageToFrontMostSubgoal(msg);

            //if the msg was not handled, test to see if this goal can handle it
            if (bHandled == false)
            {
                switch(msg.Msg)
                {
                    case (int)eMsg.PathReady:
                    {
                        //clear any existing goals
                        RemoveAllSubgoals();

                        AddSubgoal(new Goal_FollowPath(m_pOwner, m_pOwner.GetPathPlanner().GetPath()));

                        return true; //msg handled
                    }
                    case (int)eMsg.NoPathAvailable:
                    {
                        m_iStatus = (int)eStatus.failed;

                        return true; //msg handled
                    }
                    default:
                        return false;
                }
            }

            //handled by subgoals
            return true;
        }

        public void Render()
        {
            //forward the request to the subgoals
            base.Render();

            //draw a bullseye
            DebugWide.DrawCircle(m_vDestination, 6, Color.black);

            DebugWide.DrawCircle(m_vDestination, 4, Color.red);

            DebugWide.DrawCircle(m_vDestination, 2, Color.yellow);
        }
    }


    public class Goal_GetItem : Goal_Composite<Raven_Bot>
    {
    
        int m_iItemToGet;

        Trigger<Raven_Bot> m_pGiverTrigger;

        //true if a path to the item has been formulated
        bool m_bFollowingPath;

        //returns true if the bot sees that the item it is heading for has been
        //picked up by an opponent
        bool hasItemBeenStolen()
        {
              if (null != m_pGiverTrigger &&
                  !m_pGiverTrigger.isActive() &&
                  m_pOwner.hasLOSto(m_pGiverTrigger.Pos()) )
              {
                return true;
              }

              return false;
        }

        static int ItemTypeToGoalType(int gt)
        {
            switch (gt)
            {
                case (int)eObjType.health:

                    return (int)eGoal.get_health;

                case (int)eObjType.shotgun:

                    return (int)eGoal.get_shotgun;

                case (int)eObjType.rail_gun:

                    return (int)eGoal.get_railgun;

                case (int)eObjType.rocket_launcher:

                    return (int)eGoal.get_rocket_launcher;

                //default: throw std::runtime_error("Goal_GetItem cannot determine item type");

            }//end switch

            DebugWide.LogRed("Goal_GetItem cannot determine item type");
            return -1;
        }

        public Goal_GetItem(Raven_Bot pBot, int item) : base(pBot, ItemTypeToGoalType(item))
        {
            m_iItemToGet = item;
            m_pGiverTrigger = null;
            m_bFollowingPath = false;
        }


        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            m_pGiverTrigger = null;

            //request a path to the item
            m_pOwner.GetPathPlanner().RequestPathToItem(m_iItemToGet);

            //the bot may have to wait a few update cycles before a path is calculated
            //so for appearances sake it just wanders
            AddSubgoal(new Goal_Wander(m_pOwner));

        }

        override public int Process()
        {
            ActivateIfInactive();

            if (hasItemBeenStolen())
            {
                Terminate();
            }

            else
            {
                //process the subgoals
                m_iStatus = ProcessSubgoals();
            }

            return m_iStatus;
        }

        override public bool HandleMessage( Telegram msg)
        {
          //first, pass the message down the goal hierarchy
          bool bHandled = ForwardMessageToFrontMostSubgoal(msg);

          //if the msg was not handled, test to see if this goal can handle it
          if (bHandled == false)
          {
            switch(msg.Msg)
            {
            case (int)eMsg.PathReady:

              //clear any existing goals
              RemoveAllSubgoals();
                AddSubgoal(new Goal_FollowPath(m_pOwner,
                                               m_pOwner.GetPathPlanner().GetPath()));

              //get the pointer to the item
              m_pGiverTrigger = (Trigger<Raven_Bot>)(msg.ExtraInfo);

              return true; //msg handled


            case (int)eMsg.NoPathAvailable:

              m_iStatus = (int)eStatus.failed;

              return true; //msg handled

            default: return false;
            }
        }

          //handled by subgoals
          return true;
        }

        override public void Terminate() { m_iStatus = (int)eStatus.completed; }
    }

    public class Goal_Explore : Goal_Composite<Raven_Bot>
    {
  
        Vector3 m_CurrentDestination;

        //set to true when the destination for the exploration has been established
        bool m_bDestinationIsSet;

    
        public Goal_Explore(Raven_Bot pOwner) : base(pOwner, (int)eGoal.explore)
        {
            m_bDestinationIsSet = false;
        }


        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //if this goal is reactivated then there may be some existing subgoals that
            //must be removed
            RemoveAllSubgoals();

            if (!m_bDestinationIsSet)
            {
                //grab a random location
                m_CurrentDestination = m_pOwner.GetWorld().GetMap().GetRandomNodeLocation();

                m_bDestinationIsSet = true;
            }

            //and request a path to that position
            m_pOwner.GetPathPlanner().RequestPathToPosition(m_CurrentDestination);

            //the bot may have to wait a few update cycles before a path is calculated
            //so for appearances sake it simple ARRIVES at the destination until a path
            //has been found
            AddSubgoal(new Goal_SeekToPosition(m_pOwner, m_CurrentDestination));
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //process the subgoals
            m_iStatus = ProcessSubgoals();

            return m_iStatus;
        }

        override public void Terminate() { }

        override public bool HandleMessage(Telegram msg)
        {
          //first, pass the message down the goal hierarchy
          bool bHandled = ForwardMessageToFrontMostSubgoal(msg);

          //if the msg was not handled, test to see if this goal can handle it
          if (bHandled == false)
          {
                switch(msg.Msg)
                {
                case (int)eMsg.PathReady:

                  //clear any existing goals
                  RemoveAllSubgoals();

                    AddSubgoal(new Goal_FollowPath(m_pOwner,
                                                   m_pOwner.GetPathPlanner().GetPath()));

                  return true; //msg handled


                case (int)eMsg.NoPathAvailable:

                  m_iStatus = (int)eStatus.failed;

                  return true; //msg handled

                default: return false;
                }
            }

          //handled by subgoals
          return true;
        }
    }

    public class Goal_AttackTarget : Goal_Composite<Raven_Bot>
    {

        public Goal_AttackTarget(Raven_Bot pOwner) : base(pOwner, (int)eGoal.attack_target)
        {}

        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //if this goal is reactivated then there may be some existing subgoals that
            //must be removed
            RemoveAllSubgoals();

            //it is possible for a bot's target to die whilst this goal is active so we
            //must test to make sure the bot always has an active target
            if (!m_pOwner.GetTargetSys().isTargetPresent())
            {
                m_iStatus = (int)eStatus.completed;

                return;
            }

            //if the bot is able to shoot the target (there is LOS between bot and
            //target), then select a tactic to follow while shooting
            if (m_pOwner.GetTargetSys().isTargetShootable())
            {
                //if the bot has space to strafe then do so
                Vector3 dummy = ConstV.v3_zero;
                if (m_pOwner.canStepLeft(dummy) || m_pOwner.canStepRight(dummy))
                {
                    AddSubgoal(new Goal_DodgeSideToSide(m_pOwner));
                }

                //if not able to strafe, head directly at the target's position 
                else
                {
                    AddSubgoal(new Goal_SeekToPosition(m_pOwner, m_pOwner.GetTargetBot().Pos()));
                }
            }

            //if the target is not visible, go hunt it.
            else
            {
                AddSubgoal(new Goal_HuntTarget(m_pOwner));
            }
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //process the subgoals
            m_iStatus = ProcessSubgoals();

            ReactivateIfFailed();

            return m_iStatus;
        }

        override public void Terminate() { m_iStatus = (int)eStatus.completed; }

    }
//======================================================

    public class Goal_Think : Goal_Composite<Raven_Bot>
    {
        public class GoalEvaluators : List<Goal_Evaluator> { }

        GoalEvaluators m_Evaluators = new GoalEvaluators();

        public int ItemTypeToGoalType(int gt)
        {
            switch (gt)
            {
                case (int)eObjType.health:

                    return (int)eGoal.get_health;

                case (int)eObjType.shotgun:

                    return (int)eGoal.get_shotgun;

                case (int)eObjType.rail_gun:

                    return (int)eGoal.get_railgun;

                case (int)eObjType.rocket_launcher:

                    return (int)eGoal.get_rocket_launcher;

                    //default: throw std::runtime_error("Goal_GetItem cannot determine item type");

            }//end switch

            DebugWide.LogRed("Goal_GetItem cannot determine item type");
            return -1;
        }

        public Goal_Think(Raven_Bot pBot) : base(pBot, (int)eGoal.think)
        {

            //these biases could be loaded in from a script on a per bot basis
            //but for now we'll just give them some random values
            const float LowRangeOfBias = 0.5f;
            const float HighRangeOfBias = 1.5f;

            float HealthBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float ShotgunBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float RocketLauncherBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float RailgunBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float ExploreBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float AttackBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);

            //create the evaluator objects
            m_Evaluators.Add(new GetHealthGoal_Evaluator(HealthBias));
            m_Evaluators.Add(new ExploreGoal_Evaluator(ExploreBias));
            m_Evaluators.Add(new AttackTargetGoal_Evaluator(AttackBias));
            m_Evaluators.Add(new GetWeaponGoal_Evaluator(ShotgunBias,
                                                     (int)eObjType.shotgun));
            m_Evaluators.Add(new GetWeaponGoal_Evaluator(RailgunBias,
                                                     (int)eObjType.rail_gun));
            m_Evaluators.Add(new GetWeaponGoal_Evaluator(RocketLauncherBias,
                                                     (int)eObjType.rocket_launcher));
        }

        //this method iterates through each goal evaluator and selects the one
        //that has the highest score as the current goal
        public void Arbitrate()
        {
            double best = 0;
            Goal_Evaluator MostDesirable = null;

            //iterate through all the evaluators to see which produces the highest score
            foreach (Goal_Evaluator curDes in m_Evaluators)
            {
                double desirabilty = curDes.CalculateDesirability(m_pOwner);

                if (desirabilty >= best)
                {
                    best = desirabilty;
                    MostDesirable = curDes;
                }
            }

            //assert(MostDesirable && "<Goal_Think::Arbitrate>: no evaluator selected");

            MostDesirable.SetGoal(m_pOwner);
        }

        //returns true if the given goal is not at the front of the subgoal list
        public bool notPresent(int GoalType)
        {
            if (0 != m_SubGoals.Count)
            {

                return m_SubGoals.First.Value.GetType() != GoalType;
            }

            return true;
        }

        //the usual suspects
        override public int Process()
        {
            ActivateIfInactive();

            int SubgoalStatus = ProcessSubgoals();

            if (SubgoalStatus == (int)eStatus.completed || SubgoalStatus == (int)eStatus.failed)
            {
                if (!m_pOwner.isPossessed())
                {
                    m_iStatus = (int)eStatus.inactive;
                }
            }

            return m_iStatus;
        }

        override public void Activate()
        {
            if (!m_pOwner.isPossessed())
            {
                Arbitrate();
            }

            m_iStatus = (int)eStatus.active;
        }
        override public void Terminate() { }


        //top level goal types
        public void AddGoal_MoveToPosition(Vector3 pos)
        {
            AddSubgoal(new Goal_MoveToPosition(m_pOwner, pos));
        }

        public void AddGoal_GetItem(int ItemType)
        {
            if (notPresent(ItemTypeToGoalType(ItemType)))
            {
                RemoveAllSubgoals();
                AddSubgoal(new Goal_GetItem(m_pOwner, ItemType));
            }
        }

        public void AddGoal_Explore()
        {
            if (notPresent((int)eGoal.explore))
            {
                RemoveAllSubgoals();
                AddSubgoal(new Goal_Explore(m_pOwner));
            }
        }
        public void AddGoal_AttackTarget()
        {
            if (notPresent((int)eGoal.attack_target))
            {
                RemoveAllSubgoals();
                AddSubgoal(new Goal_AttackTarget(m_pOwner));
            }
        }

        //this adds the MoveToPosition goal to the *back* of the subgoal list.
        public void QueueGoal_MoveToPosition(Vector3 pos)
        {
            m_SubGoals.AddLast(new Goal_MoveToPosition(m_pOwner, pos));
        }

        //this renders the evaluations (goal scores) at the specified location
        public void RenderEvaluations(int left, int top)
        {
            //gdi->TextColor(Cgdi::black);

            foreach (Goal_Evaluator cur in m_Evaluators)
            {
                cur.RenderInfo(new Vector3(left, 0, top), m_pOwner);
                //left += 75;
            }
        }

        override public void Render()
        {
            foreach (Goal<Raven_Bot> cur in m_SubGoals)
            {
                cur.Render();
            }

        }


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

        public Raven_Bot(int id) : base(id) { }

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

