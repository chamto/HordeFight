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

    public class Const
    {
        public const float SEND_MSG_IMMEDIATELY = 0.0f;
        public const int NO_ADDITIONAL_INFO = 0;
        public const int SENDER_ID_IRRELEVANT = -1;
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

    public class Path : LinkedList<PathEdge> { }

    public class NavGraph : SparseGraph<NavGraphNode, NavGraphEdge>
    {
        public NavGraph(bool digraph) : base(digraph)
        {
        }
    }

    public class CellSpacePartition<entity>
    {
        public List<entity> m_Neighbors;
        public void CalculateNeighbors(Vector3 TargetPos, float QueryRadius) { }
    }
    public class CellSpace : CellSpacePartition<NavGraphNode>
    { }

    public class Raven_PathPlanner
    {

        //for legibility
        const int no_closest_node_found = -1;


        //A pointer to the owner of this class
        Raven_Bot m_pOwner;

        //a reference to the navgraph
        NavGraph m_NavGraph;

        //a pointer to an instance of the current graph search algorithm.
        Graph_SearchTimeSliced<NavGraphEdge> m_pCurrentSearch;

        //this is the position the bot wishes to plan a path to reach
        Vector3 m_vDestinationPos;


        //returns the index of the closest visible and unobstructed graph node to
        //the given position
        int GetClosestNodeToPosition(Vector3 pos)
        {
            float ClosestSoFar = float.MaxValue;
            int ClosestNode = no_closest_node_found;

            //when the cell space is queried this the the range searched for neighboring
            //graph nodes. This value is inversely proportional to the density of a 
            //navigation graph (less dense = bigger values)
            float range = m_pOwner.GetWorld().GetMap().GetCellSpaceNeighborhoodRange();

            //calculate the graph nodes that are neighboring this position
            m_pOwner.GetWorld().GetMap().GetCellSpace().CalculateNeighbors(pos, range);


            foreach(NavGraphNode pN in m_pOwner.GetWorld().GetMap().GetCellSpace().m_Neighbors)
            {
                if (m_pOwner.canWalkBetween(pos, pN.Pos()))
                {
                    float dist = (pos - pN.Pos()).sqrMagnitude;

                    //keep a record of the closest so far
                    if (dist < ClosestSoFar)
                    {
                        ClosestSoFar = dist;
                        ClosestNode = pN.Index();
                    }
                }

            }

          return ClosestNode;
        }

        //smooths a path by removing extraneous edges. (may not remove all
        //extraneous edges)
        void SmoothPathEdgesQuick(Path path)
        {

            LinkedListNode<PathEdge> e1, e2 , temp;
            e1 = path.First;
            e2 = path.First.Next;

            //while e2 is not the last edge in the path, step through the edges checking
            //to see if the agent can move without obstruction from the source node of
            //e1 to the destination node of e2. If the agent can move between those 
            //positions then the two edges are replaced with a single edge.
            while (e1 != path.Last)
            {
                //check for obstruction, adjust and remove the edges accordingly
                if ((e2.Value.Behavior() == (int)NavGraphEdge.eFlag.normal) &&
                      m_pOwner.canWalkBetween(e1.Value.Source(), e2.Value.Destination()))
                {
                    e1.Value.SetDestination(e2.Value.Destination());

                    //e2 = path.erase(e2);
                    temp = e2.Next;
                    path.Remove(e2);
                    e2 = temp;
                }

                else
                {
                    e1 = e2;
                    e2 = e2.Next;
                }
            }

        }

        //smooths a path by removing extraneous edges. (removes *all* extraneous
        //edges)
        void SmoothPathEdgesPrecise(Path path)
        {

            LinkedListNode<PathEdge> e1, e2, temp;
            e1 = path.First;

            while (e1 != path.Last)
            {
                //point e2 to the edge immediately following e1
                e2 = e1.Next;

                //while e2 is not the last edge in the path, step through the edges
                //checking to see if the agent can move without obstruction from the 
                //source node of e1 to the destination node of e2. If the agent can move
                //between those positions then the any edges between e1 and e2 are
                //replaced with a single edge.
                while (e2 != path.Last)
                {
                    //check for obstruction, adjust and remove the edges accordingly
                    if ((e2.Value.Behavior() == (int)NavGraphEdge.eFlag.normal) &&
                          m_pOwner.canWalkBetween(e1.Value.Source(), e2.Value.Destination()))
                    {
                        e1.Value.SetDestination(e2.Value.Destination());

                        //e2 = path.erase(++e1, ++e2); //e1 ~ e2 전 것 까지만 지운다 
                        //e1 = e2;
                        //--e1;

                        e1 = e1.Next;
                        e2 = e2.Next;
                        while(e1 != e2)
                        {
                            temp = e1.Next;
                            path.Remove(e1);
                            e1 = temp;
                        }
                        e1 = e1.Previous;

                    }

                    else
                    {
                        //++e2;
                        e2 = e2.Next;
                    }
                }

                //++e1;
                e1 = e1.Next;
            }

        }

        //called at the commencement of a new search request. It clears up the 
        //appropriate lists and memory in preparation for a new search request
        void GetReadyForNewSearch()
        {
            //unregister any existing search with the path manager
            m_pOwner.GetWorld().GetPathManager().UnRegister(this);

            //clean up memory used by any existing search
            //delete m_pCurrentSearch;
            m_pCurrentSearch = null;
        }



        public Raven_PathPlanner(Raven_Bot owner)
        {
            m_pOwner = owner;
            m_NavGraph = m_pOwner.GetWorld().GetMap().GetNavGraph();
            m_pCurrentSearch = null;
        }

        //creates an instance of the A* time-sliced search and registers it with
        //the path manager
        public bool RequestPathToItem(int ItemType)
        {
            //clear the waypoint list and delete any active search
            GetReadyForNewSearch();

            //find the closest visible node to the bots position
            int ClosestNodeToBot = GetClosestNodeToPosition(m_pOwner.Pos());

            //remove the destination node from the list and return false if no visible
            //node found. This will occur if the navgraph is badly designed or if the bot
            //has managed to get itself *inside* the geometry (surrounded by walls),
            //or an obstacle
            if (ClosestNodeToBot == no_closest_node_found)
            {
        //# ifdef SHOW_NAVINFO
        //        debug_con << "No closest node to bot found!" << "";
        //#endif

                return false;
            }

            Graph_SearchDijkstras_TS < NavGraphNode, NavGraphEdge > graph = new Graph_SearchDijkstras_TS<NavGraphNode, NavGraphEdge>(m_NavGraph,
                                             ClosestNodeToBot,
                                             ItemType);
            graph.iCondition = new FindActiveTrigger<NavGraphNode, NavGraphEdge>();
            m_pCurrentSearch = graph;


            //register the search with the path manager
            m_pOwner.GetWorld().GetPathManager().Register(this);

            return true;
        }

        //creates an instance of the Dijkstra's time-sliced search and registers 
        //it with the path manager
        public bool RequestPathToPosition(Vector3 TargetPos)
        {
        //# ifdef SHOW_NAVINFO
        //    debug_con << "------------------------------------------------" << "";
        //#endif
            GetReadyForNewSearch();

            //make a note of the target position.
            m_vDestinationPos = TargetPos;

            //if the target is walkable from the bot's position a path does not need to
            //be calculated, the bot can go straight to the position by ARRIVING at
            //the current waypoint
            if (m_pOwner.canWalkTo(TargetPos))
            {
                return true;
            }

            //find the closest visible node to the bots position
            int ClosestNodeToBot = GetClosestNodeToPosition(m_pOwner.Pos());

            //remove the destination node from the list and return false if no visible
            //node found. This will occur if the navgraph is badly designed or if the bot
            //has managed to get itself *inside* the geometry (surrounded by walls),
            //or an obstacle.
            if (ClosestNodeToBot == no_closest_node_found)
            {
        //# ifdef SHOW_NAVINFO
        //        debug_con << "No closest node to bot found!" << "";
        //#endif

                return false;
            }

        //# ifdef SHOW_NAVINFO
        //    debug_con << "Closest node to bot is " << ClosestNodeToBot << "";
        //#endif

            //find the closest visible node to the target position
            int ClosestNodeToTarget = GetClosestNodeToPosition(TargetPos);

            //return false if there is a problem locating a visible node from the target.
            //This sort of thing occurs much more frequently than the above. For
            //example, if the user clicks inside an area bounded by walls or inside an
            //object.
            if (ClosestNodeToTarget == no_closest_node_found)
            {
        //# ifdef SHOW_NAVINFO
        //        debug_con << "No closest node to target (" << ClosestNodeToTarget << ") found!" << "";
        //#endif

                return false;
            }

            //# ifdef SHOW_NAVINFO
            //    debug_con << "Closest node to target is " << ClosestNodeToTarget << "";
            //#endif


            //create an instance of a the distributed A* search class
            Graph_SearchAStar_TS<NavGraphNode, NavGraphEdge> graph = new Graph_SearchAStar_TS<NavGraphNode, NavGraphEdge>(m_NavGraph,
                                             ClosestNodeToBot,
                                         ClosestNodeToTarget);
            graph.iHeuristic = new Heuristic_Euclid<NavGraphNode, NavGraphEdge>();
            m_pCurrentSearch = graph;


            //and register the search with the path manager
            m_pOwner.GetWorld().GetPathManager().Register(this);

            return true;
        }

        //called by an agent after it has been notified that a search has terminated
        //successfully. The method extracts the path from m_pCurrentSearch, adds
        //additional edges appropriate to the search type and returns it as a list of
        //PathEdges.
        public Path GetPath()
        {
            //assert(m_pCurrentSearch &&
                    //"<Raven_PathPlanner::GetPathAsNodes>: no current search");

            Path path = m_pCurrentSearch.GetPathAsPathEdges();

            int closest = GetClosestNodeToPosition(m_pOwner.Pos());

            path.AddFirst(new PathEdge(m_pOwner.Pos(),
                                      GetNodePosition(closest),
                                      (int)NavGraphEdge.eFlag.normal));


            //if the bot requested a path to a location then an edge leading to the
            //destination must be added
            if (m_pCurrentSearch.GetType() == Graph_SearchTimeSliced < NavGraphEdge >.eSearchType.AStar)
            {
                path.AddLast(new PathEdge(path.Last.Value.Destination(),
                                        m_vDestinationPos,
                                        (int)NavGraphEdge.eFlag.normal));
            }

            //smooth paths if required
            if (UserOptions.m_bSmoothPathsQuick)
            {
                SmoothPathEdgesQuick(path);
            }

            if (UserOptions.m_bSmoothPathsPrecise)
            {
                SmoothPathEdgesPrecise(path);
            }

            return path;
        }

        //returns the cost to travel from the bot's current position to a specific 
        //graph node. This method makes use of the pre-calculated lookup table
        //created by Raven_Game
        public double GetCostToNode(int NodeIdx)
        {
            //find the closest visible node to the bots position
            int nd = GetClosestNodeToPosition(m_pOwner.Pos());

            //add the cost to this node
            float cost = (m_pOwner.Pos() - m_NavGraph.GetNode(nd).Pos()).magnitude;

            //add the cost to the target node and return
            return cost + m_pOwner.GetWorld().GetMap().CalculateCostToTravelBetweenNodes(nd, NodeIdx);
        }

        //returns the cost to the closest instance of the GiverType. This method
        //also makes use of the pre-calculated lookup table. Returns -1 if no active
        //trigger found
        public float GetCostToClosestItem(int GiverType)
        {
          //find the closest visible node to the bots position
          int nd = GetClosestNodeToPosition(m_pOwner.Pos());

          //if no closest node found return failure
          if (nd == GraphNode.INVALID_NODE_INDEX) return -1;

          float ClosestSoFar = float.MaxValue;

            //iterate through all the triggers to find the closest *active* trigger of 
            //type GiverType
            TriggerList triggers = m_pOwner.GetWorld().GetMap().GetTriggers();

            foreach(Trigger<Raven_Bot> it in triggers)
            {
                if (((it).EntityType() == GiverType) && (it).isActive())
                {
                    float cost =
                    m_pOwner.GetWorld().GetMap().CalculateCostToTravelBetweenNodes(nd,
                                                                    (it).GraphNodeIndex());

                    if (cost < ClosestSoFar)
                    {
                        ClosestSoFar = cost;
                    }
                }
            }

          //return a negative value if no active trigger of the type found
          if (Misc.IsEqual(ClosestSoFar, float.MaxValue))
          {
            return -1;
          }

          return ClosestSoFar;
        }

        //the path manager calls this to iterate once though the search cycle
        //of the currently assigned search algorithm. When a search is terminated
        //the method messages the owner with either the msg_NoPathAvailable or
        //msg_PathReady messages
        public int CycleOnce()
        {
          //assert(m_pCurrentSearch && "<Raven_PathPlanner::CycleOnce>: No search object instantiated");

            int result = m_pCurrentSearch.CycleOnce();

          //let the bot know of the failure to find a path
          if (result == (int)eReturn.target_not_found)
          {

                SingleO.dispatcher.DispatchMessage(Const.SEND_MSG_IMMEDIATELY,
                                     Const.SENDER_ID_IRRELEVANT,
                                     m_pOwner.ID(),
                                     (int)eMsg.NoPathAvailable,
                                     null);
                                     
          }

          //let the bot know a path has been found
          else if (result == (int)eReturn.target_found)
          {
                //if the search was for an item type then the final node in the path will
                //represent a giver trigger. Consequently, it's worth passing the pointer
                //to the trigger in the extra info field of the message. (The pointer
                //will just be NULL if no trigger)
                var pTrigger =
                m_NavGraph.GetNode(m_pCurrentSearch.GetPathToTarget().Last.Value).ExtraInfo<Trigger<Raven_Bot>>();


                SingleO.dispatcher.DispatchMessage(Const.SEND_MSG_IMMEDIATELY,
                                Const.SENDER_ID_IRRELEVANT,
                                m_pOwner.ID(),
                                (int)eMsg.PathReady,
                                pTrigger);
          }

          return result;
        }

        public Vector3 GetDestination() {return m_vDestinationPos;}
        public void SetDestination(Vector3 NewPos) { m_vDestinationPos = NewPos; }

        //used to retrieve the position of a graph node from its index. (takes
        //into account the enumerations 'non_graph_source_node' and 
        //'non_graph_target_node'
        Vector3 GetNodePosition(int idx)
        {
          return m_NavGraph.GetNode(idx).Pos();
        }
    }

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


    public class Raven_Game
    {
        PathManager<Raven_PathPlanner> m_pPathManager;

        public Raven_Map GetMap() { return null; }
        public PathManager<Raven_PathPlanner> GetPathManager(){return m_pPathManager;}
    }

    public class Raven_Map
    {

        NavGraph m_pNavGraph;
        CellSpace m_pSpacePartition;
        float m_dCellSpaceNeighborhoodRange;

        TriggerSystem<Trigger<Raven_Bot>> m_TriggerSystem;

        public TriggerList GetTriggers() {return m_TriggerSystem.GetTriggers();}
    public Vector3 GetRandomNodeLocation() { return ConstV.v3_zero; }
        public NavGraph GetNavGraph() {return m_pNavGraph;}
        public float GetCellSpaceNeighborhoodRange() {return m_dCellSpaceNeighborhoodRange;}
        public CellSpace  GetCellSpace() {return m_pSpacePartition;}
        public float CalculateCostToTravelBetweenNodes(int nd1, int nd2) { return 0f; }
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
        public bool canWalkBetween(Vector3 from, Vector3 to) { return false; }
        public bool canWalkTo(Vector3 pos) { return false; }
    }


}//end namespace

