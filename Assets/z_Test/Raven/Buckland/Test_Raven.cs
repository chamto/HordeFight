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

    public static class Feature
    {

        //returns a value between 0 and 1 based on the bot's health. The better
        //the health, the higher the rating
        public static float Health(Raven_Bot pBot)
        {
            return (float)pBot.Health() / (float)pBot.MaxHealth();

        }

        //returns a value between 0 and 1 based on the bot's closeness to the 
        //given item. the further the item, the higher the rating. If there is no
        //item of the given type present in the game world at the time this method
        //is called the value returned is 1
        public static float DistanceToItem(Raven_Bot pBot, int ItemType)
        {
            //determine the distance to the closest instance of the item type
            float DistanceToItem = pBot.GetPathPlanner().GetCostToClosestItem(ItemType);

            //if the previous method returns a negative value then there is no item of
            //the specified type present in the game world at this time.
            if (DistanceToItem < 0) return 1f;

            //these values represent cutoffs. Any distance over MaxDistance results in
            //a value of 0, and value below MinDistance results in a value of 1
            const float MaxDistance = 500.0f;
            const float MinDistance = 50.0f;

            DistanceToItem = Mathf.Clamp(DistanceToItem, MinDistance, MaxDistance);

            return DistanceToItem / MaxDistance;
        }

        //returns a value between 0 and 1 based on how much ammo the bot has for
        //the given weapon, and the maximum amount of ammo the bot can carry. The
        //closer the amount carried is to the max amount, the higher the score
        public static float IndividualWeaponStrength(Raven_Bot pBot,
                                               int WeaponType)
        {
            //grab a pointer to the gun (if the bot owns an instance)
            Raven_Weapon wp = pBot.GetWeaponSys().GetWeaponFromInventory(WeaponType);

            if (null != wp)
            {
                return wp.NumRoundsRemaining() / GetMaxRoundsBotCanCarryForWeapon(WeaponType);
            }

            else
            {
                return 0.0f;
            }
        }


        public static float GetMaxRoundsBotCanCarryForWeapon(int WeaponType)
        {
            switch (WeaponType)
            {
                case (int)eObjType.rail_gun:

                    return Params.RailGun_MaxRoundsCarried;

                case (int)eObjType.rocket_launcher:

                    return Params.RocketLauncher_MaxRoundsCarried;

                case (int)eObjType.shotgun:

                    return Params.ShotGun_MaxRoundsCarried;

                
                    //throw std::runtime_error("trying to calculate  of unknown weapon");

            }//end switch

            DebugWide.LogError("trying to calculate  of unknown weapon");
            return -1f;
        }

        //returns a value between 0 and 1 based on the total amount of ammo the
        //bot is carrying each of the weapons. Each of the three weapons a bot can
        //pick up can contribute a third to the score. In other words, if a bot
        //is carrying a RL and a RG and has max ammo for the RG but only half max
        //for the RL the rating will be 1/3 + 1/6 + 0 = 0.5
        public static float TotalWeaponStrength(Raven_Bot pBot)
        {
            float MaxRoundsForShotgun = GetMaxRoundsBotCanCarryForWeapon((int)eObjType.shotgun);
            float MaxRoundsForRailgun = GetMaxRoundsBotCanCarryForWeapon((int)eObjType.rail_gun);
            float MaxRoundsForRocketLauncher = GetMaxRoundsBotCanCarryForWeapon((int)eObjType.rocket_launcher);
            float TotalRoundsCarryable = MaxRoundsForShotgun + MaxRoundsForRailgun + MaxRoundsForRocketLauncher;

            float NumSlugs = (float)pBot.GetWeaponSys().GetAmmoRemainingForWeapon((int)eObjType.rail_gun);
            float NumCartridges = (float)pBot.GetWeaponSys().GetAmmoRemainingForWeapon((int)eObjType.shotgun);
            float NumRockets = (float)pBot.GetWeaponSys().GetAmmoRemainingForWeapon((int)eObjType.rocket_launcher);

            //the value of the tweaker (must be in the range 0-1) indicates how much
            //desirability value is returned even if a bot has not picked up any weapons.
            //(it basically adds in an amount for a bot's persistent weapon -- the blaster)
            const float Tweaker = 0.1f;

            return Tweaker + (1 - Tweaker) * (NumSlugs + NumCartridges + NumRockets) / (MaxRoundsForShotgun + MaxRoundsForRailgun + MaxRoundsForRocketLauncher);
        }
    }

    public class Goal_Evaluator
    {

        //when the desirability score for a goal has been evaluated it is multiplied 
        //by this value. It can be used to create bots with preferences based upon
        //their personality
        protected float m_dCharacterBias;


        public Goal_Evaluator(float CharacterBias)
        {
            m_dCharacterBias = CharacterBias;
        }


        //returns a score between 0 and 1 representing the desirability of the
        //strategy the concrete subclass represents
        public virtual double CalculateDesirability(Raven_Bot pBot) { return 0; }

        //adds the appropriate goal to the given bot's brain
        public virtual void SetGoal(Raven_Bot pBot) { }

        //used to provide debugging/tweaking support
        public virtual void RenderInfo(Vector3 Position, Raven_Bot pBot) { }
    }

    public class GetHealthGoal_Evaluator : Goal_Evaluator
    {

        public GetHealthGoal_Evaluator(float bias) : base(bias) { }

        public float CalculateDesirability(Raven_Bot pBot)
        {
            //first grab the distance to the closest instance of a health item
            float Distance = Feature.DistanceToItem(pBot, (int)eObjType.health);

            //if the distance feature is rated with a value of 1 it means that the
            //item is either not present on the map or too far away to be worth 
            //considering, therefore the desirability is zero
            if (Distance == 1f)
            {
                return 0;
            }
            else
            {
                //value used to tweak the desirability
                const float Tweaker = 0.2f;

                //the desirability of finding a health item is proportional to the amount
                //of health remaining and inversely proportional to the distance from the
                //nearest instance of a health item.
                float Desirability = Tweaker * (1 - Feature.Health(pBot)) /
                                    (Feature.DistanceToItem(pBot, (int)eObjType.health));

                //ensure the value is in the range 0 to 1
                Desirability = Mathf.Clamp(Desirability, 0, 1f);

                //bias the value according to the personality of the bot
                Desirability *= m_dCharacterBias;

                return Desirability;
            }
        }

        public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_GetItem((int)eObjType.health);
        }

        public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {

            DebugWide.PrintText(Position, Color.black, "H: " + CalculateDesirability(pBot));
            //return;

            //std::string s = ttos(1 - Raven_Feature::Health(pBot)) + ", " + ttos(Raven_Feature::DistanceToItem(pBot, type_health));
            //gdi->TextAtPos(Position + Vector2D(0, 15), s);
        }
    }


    public class AttackTargetGoal_Evaluator : Goal_Evaluator
    { 
    
        public AttackTargetGoal_Evaluator(float bias) :base(bias) { }

        public float CalculateDesirability(Raven_Bot pBot)
        {
            float Desirability = 0.0f;

            //only do the calculation if there is a target present
            if (pBot.GetTargetSys().isTargetPresent())
            {
                const float Tweaker = 1.0f;

                Desirability = Tweaker *
                               Feature.Health(pBot) *
                               Feature.TotalWeaponStrength(pBot);

                //bias the value according to the personality of the bot
                Desirability *= m_dCharacterBias;
            }

            return Desirability;
        }

        public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_AttackTarget();
        }

        public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            DebugWide.PrintText(Position, Color.black, "AT: " + CalculateDesirability(pBot));
            //return;

            //std::string s = ttos(Raven_Feature::Health(pBot)) + ", " + ttos(Raven_Feature::TotalWeaponStrength(pBot));
            //gdi->TextAtPos(Position + Vector2D(0, 12), s);
        }
    }

    public class ExploreGoal_Evaluator : Goal_Evaluator
    { 

        public ExploreGoal_Evaluator(float bias) :base(bias) { }

        public float CalculateDesirability(Raven_Bot pBot)
        {
            float Desirability = 0.05f;

            Desirability *= m_dCharacterBias;

            return Desirability;
        }

        public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_Explore();
        }

        public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            DebugWide.PrintText(Position, Color.black, "EX: " + CalculateDesirability(pBot));
        }
    }

    public class GetWeaponGoal_Evaluator : Goal_Evaluator
    {
        int m_iWeaponType;


        public GetWeaponGoal_Evaluator(float bias,
                              int WeaponType) : base(bias)
        {
            m_iWeaponType = WeaponType;
        }

        public float CalculateDesirability(Raven_Bot pBot)
        {
            //grab the distance to the closest instance of the weapon type
            float Distance = Feature.DistanceToItem(pBot, m_iWeaponType);

            //if the distance feature is rated with a value of 1 it means that the
            //item is either not present on the map or too far away to be worth 
            //considering, therefore the desirability is zero
            if (Distance == 1)
            {
                return 0;
            }
            else
            {
                //value used to tweak the desirability
                const float Tweaker = 0.15f;

                float Health, WeaponStrength;

                Health = Feature.Health(pBot);

                WeaponStrength = Feature.IndividualWeaponStrength(pBot,
                                                                         m_iWeaponType);

                float Desirability = (Tweaker * Health * (1 - WeaponStrength)) / Distance;

                //ensure the value is in the range 0 to 1
                Desirability = Mathf.Clamp(Desirability, 0, 1f);

                Desirability *= m_dCharacterBias;

                return Desirability;
            }
        }

        public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_GetItem(m_iWeaponType);
        }

        public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            string s = "";
            switch (m_iWeaponType)
            {
                case (int)eObjType.rail_gun:
                    s = "RG: "; break;
                case (int)eObjType.rocket_launcher:
                    s = "RL: "; break;
                case (int)eObjType.shotgun:
                    s = "SG: "; break;
            }

            DebugWide.PrintText(Position, Color.black, s + CalculateDesirability(pBot));

        }
    }

    //======================================================

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

//======================================================

    public class Goal_Think : Goal_Composite<Raven_Bot>
    {
        public class GoalEvaluators : List<Goal_Evaluator> { }

        GoalEvaluators m_Evaluators = new GoalEvaluators();


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

    public class Raven_PathPlanner
    {
        public Raven_PathPlanner GetPath() { return null; }
        public float GetCostToClosestItem(int GiverType) { return 0;  }
        public bool RequestPathToPosition(Vector3 TargetPos) { return false; }
    }

    public class Raven_TargetingSystem
    {
        //the owner of this system
        Raven_Bot m_pOwner;

        //the current target (this will be null if there is no target assigned)
        Raven_Bot m_pCurrentTarget;

        public bool isTargetPresent() {return m_pCurrentTarget != null;}
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

    public class Raven_Bot //: BaseGameEntity
    {
    
        Goal_Think m_pBrain;
        //the bot uses this to plan paths
        Raven_PathPlanner m_pPathPlanner;

        Raven_TargetingSystem m_pTargSys;
        Raven_WeaponSystem m_pWeaponSys;

        int m_iHealth;
        int m_iMaxHealth;

        //set to true when a human player takes over control of the bot
        bool m_bPossessed;


        public int Health() {return m_iHealth;}
        public int MaxHealth() {return m_iMaxHealth;}

        public bool isPossessed() {return m_bPossessed;}

        public Raven_TargetingSystem GetTargetSys() {return m_pTargSys;}
        public Raven_WeaponSystem GetWeaponSys() {return m_pWeaponSys;}
        public Goal_Think  GetBrain(){return m_pBrain;}
        public Raven_PathPlanner  GetPathPlanner(){return m_pPathPlanner;}
    }


}//end namespace

