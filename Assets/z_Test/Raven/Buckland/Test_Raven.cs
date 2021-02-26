using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{
    public class Test_Raven : MonoBehaviour
    {
        Transform _mousePoint = null;
        public bool _input_movePos = false;
        public static Raven_Game _game = null;

        Raven_Bot _bot_0 = null;

        // Use this for initialization
        void Start()
        {
            //Vector3 a = new Vector3(3, 7, 2);
            //Vector3 b = new Vector3(3, 1, 7);
            //Vector3 c = Vector3.Cross(a, b);
            ////float det = c.x + c.y + c.z;
            //float det = Vector3.Dot(c, c.normalized);
            //DebugWide.LogBlue(c.magnitude + "  " + det);


            _mousePoint = GameObject.Find("MousePoint").transform;
            SingleO.Init();
            _game = new Raven_Game();

            var first = _game.GetAllBots().First;
            _game.SelectMoveBot(first.Value, Vector3.zero);

            //var second = first.Next;
            //second.Value._pause = true;

            //_bot_0 = new Raven_Bot(_game, ConstV.v3_zero);
            //_bot_0.m_Status = Raven_Bot.eStatus.alive;

        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        private void OnDrawGizmos()
        {
            if (null == _game) return;

            if (true == _input_movePos)
            {
                _input_movePos = false;
                var first = _game.GetAllBots().First;
                _game.SelectMoveBot(first.Value, _mousePoint.position);

            }

            _game.Update();
            _game.Render();


            if (null == _bot_0) return;
            //_bot_0.Update();
            //_bot_0.GetBrain().Render(); //위치출력
            //_bot_0.GetBrain().RenderAtPos(ConstV.v3_zero); //복합목표 출력
            //_bot_0.GetBrain().RenderEvaluations(new Vector3(0,0,15)); //생각을 위한 평가값들 출력
            //_bot_0.Render();
            //_game.GetMap().Render();

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


        public static bool m_bShowTargetOfSelectedBot = true;

        public static bool m_bShowOpponentsSensedBySelectedBot = false;

        public static bool m_bOnlyShowBotsInTargetsFOV = false;

        //think 정보 출력 
        public static bool m_bShowPathOfSelectedBot = true;
        public static bool m_bShowGoalsOfSelectedBot = true;
        public static bool m_bShowGoalAppraisals = true;
        //======

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


            Vector3 tr, cur, prev = ConstV.v3_zero;

            Vector3 v = points[points.Count - 1];
            tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
            prev = m.MultiplyPoint(tr) + pos;
            //prev = rotQ * v + pos;
            for (int i=0;i<points.Count;i++)
            {
                v = points[i];
                tr = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
                cur = m.MultiplyPoint(tr) + pos;
                //cur = rotQ * points[i] + pos;

                DebugWide.DrawLine(prev, cur, color);

                prev = cur;
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

        static public void Vec2DRotateAroundOrigin(ref Vector3 v, float radian)
        {
            Quaternion rotQ = Quaternion.AngleAxis(radian * Mathf.Rad2Deg, ConstV.v3_up);
            v = rotQ * v;
        }

    }   

    public class HandyGraph
    {
            
        static public void GraphHelper_DrawUsingGDI(SparseGraph graph, Color color, bool DrawNodeIDs = false)
        {   

            //just return if the graph has no nodes
            if (graph.NumNodes() == 0) return;
            
            //draw the nodes 
            foreach (NavGraphNode pN in graph.GetListNodes())
            {
                DebugWide.DrawCircle(pN.Pos(), 2, color);
                

                if (DrawNodeIDs)
                {
                        Color c = new Color(200, 200, 200);
                        DebugWide.PrintText(pN.Pos() + new Vector3(5, 0, -5), c, "" + pN.Index());
                }

                foreach(NavGraphEdge edge in graph.GetListEdges()[pN.Index()])
                {
                        DebugWide.DrawLine(pN.Pos(), graph.GetNode(edge.To()).Pos(), color);
                  
                }
            }
        }


        static public  List<List<float>> CreateAllPairsCostsTable( SparseGraph G)
        {
            //create a two dimensional vector
            List<float> row = new List<float>(G.NumNodes());
            for(int i=0;i< G.NumNodes();i++)
            {
                row.Add(0); 
            }

            List<List<float>> PathCosts = new List<List<float>>(G.NumNodes());
            for (int i = 0; i < G.NumNodes(); i++)
            {
                PathCosts.Add(new List<float>(row));
            }

            for (int source=0; source<G.NumNodes(); ++source)
            {
                //do the search
                Graph_SearchDijkstra search = new Graph_SearchDijkstra(G, source);

                //iterate through every node in the graph and grab the cost to travel to
                //that node
                for (int target = 0; target<G.NumNodes(); ++target)
                {
                    if (source != target)
                    {
                        //DebugWide.LogBlue(source + "  ______ " + target + " ___" + PathCosts[source].Count + "  g:"+ G.NumNodes());
                        PathCosts[source][target]= search.GetCostToNode(target);
                    }
                }//next target node
            
            }//next source node

          return PathCosts;
        }

        //---------------------- CalculateAverageGraphEdgeLength ----------------------
        //
        //  determines the average length of the edges in a navgraph (using the 
        //  distance between the source & target node positions (not the cost of the 
        //  edge as represented in the graph, which may account for all sorts of 
        //  other factors such as terrain type, gradients etc)
        //------------------------------------------------------------------------------
        static public float CalculateAverageGraphEdgeLength(SparseGraph G)
        {
            float TotalLength = 0;
            int NumEdgesCounted = 0;

            foreach(NavGraphNode pN in G.GetListNodes())
            {
                foreach(NavGraphEdge pE in G.GetEdges(pN.Index()))
                {
                  //increment edge counter
                  ++NumEdgesCounted;

                  //add length of edge to total length
                  TotalLength += (G.GetNode(pE.From()).Pos() -  G.GetNode(pE.To()).Pos()).magnitude;
                }
            }

          return TotalLength / (float) NumEdgesCounted;
        }
    }

    //=======================================================


    public class Goal_Wander : Goal<Raven_Bot>
    {
    
        public Goal_Wander(Raven_Bot pBot) :base(pBot, (int)eGoal.wander) {}

        //override public void Activate() { }
        //override public int Process() { return 0; }
        //override public void Terminate() { }
    }

    //=======================================================


    public class GraveMarkers
    {
        public GraveMarkers(float v) { }

        public void Update() { }
        public void Render() { }

        public void AddGrave(Vector3 bot) { }
    }



}//end namespace

