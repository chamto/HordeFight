using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{
    /*
    public class Raven_Map
    {
        List<Wall2D> m_Walls;
        SparseGraph m_pNavGraph = new SparseGraph(false);
        CellSpacePartition<NavGraphNode> m_pSpacePartition;
        float m_dCellSpaceNeighborhoodRange;

        TriggerSystem m_TriggerSystem;

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
        public SparseGraph GetNavGraph() { return m_pNavGraph; }
        public float GetCellSpaceNeighborhoodRange() { return m_dCellSpaceNeighborhoodRange; }
        public CellSpacePartition<NavGraphNode> GetCellSpace() { return m_pSpacePartition; }
        public float CalculateCostToTravelBetweenNodes(int nd1, int nd2) { return 0f; }

        public List<Wall2D> GetWalls() { return m_Walls; }

        public Wall2D AddWall(Vector3 a, Vector3 b) { return new Wall2D(); }
    }
    //*/

    //*
    public class Raven_Map
    {


        //typedef CellSpacePartition<NavGraph::NodeType*>   CellSpace;

        //typedef Trigger<Raven_Bot>                        TriggerType;
        //typedef TriggerSystem<TriggerType>                TriggerSystem;


        //the walls that comprise the current map's architecture. 
        List<Wall2D> m_Walls = new List<Wall2D>();

        //trigger are objects that define a region of space. When a raven bot
        //enters that area, it 'triggers' an event. That event may be anything
        //from increasing a bot's health to opening a door or requesting a lift.
        TriggerSystem m_TriggerSystem = new TriggerSystem();

        //this holds a number of spawn positions. When a bot is instantiated
        //it will appear at a randomly selected point chosen from this vector
        List<Vector3> m_SpawnPoints = new List<Vector3>();

        //a map may contain a number of sliding doors.
        List<Raven_Door> m_Doors = new List<Raven_Door>();

        //this map's accompanying navigation graph
        SparseGraph m_pNavGraph;

        //the graph nodes will be partitioned enabling fast lookup
        CellSpacePartition<NavGraphNode> m_pSpacePartition;

        //the size of the search radius the cellspace partition uses when looking for 
        //neighbors 
        float m_dCellSpaceNeighborhoodRange;

        int m_iSizeX;
        int m_iSizeY;

        void PartitionNavGraph()
        {
            //if (null != m_pSpacePartition) delete m_pSpacePartition;

            m_pSpacePartition = new CellSpacePartition<NavGraphNode>(m_iSizeX,
                                                                            m_iSizeY,
                                                                            Params.NumCellsX,
                                                                            Params.NumCellsY,
                                                                            m_pNavGraph.NumNodes());

            //add the graph nodes to the space partition
            foreach (NavGraphNode pN in m_pNavGraph.GetListNodes())
            {
                m_pSpacePartition.AddEntity(pN);
            }
        }

        //this will hold a pre-calculated lookup table of the cost to travel from
        //one node to any other.
        List<List<float>> m_PathCosts;


        //stream constructors for loading from a file
        void AddWall(string line)
        {
            m_Walls.Add(new Wall2D(line));
        }

        public Wall2D AddWall(Vector3 from, Vector3 to)
        {
            Wall2D w = new Wall2D(from, to);

            m_Walls.Add(w);

            return w;
        }

        void AddSpawnPoint(string line)
        {

            string[] sp = HandyString.SplitBlank(line);

            float x = float.Parse(sp[1]);
            float z = float.Parse(sp[2]);


            m_SpawnPoints.Add(new Vector3(x, 0, z));
        }


        void AddHealth_Giver(string line)
        {
            //---------->
            string subLine;
            int id = HandyString.GetFirstLetter<int>(line, out subLine);
            //---------->

            Trigger_HealthGiver hg = new Trigger_HealthGiver(id, subLine);

            m_TriggerSystem.Register(hg);

            //let the corresponding navgraph node point to this object
            NavGraphNode node = m_pNavGraph.GetNode(hg.GraphNodeIndex());

            node.SetExtraInfo(hg);

            //register the entity 
            SingleO.entityMgr.RegisterEntity(hg);
        }

        void AddWeapon_Giver(int type_of_weapon, string line)
        {
            //---------->
            string subLine;
            int id = HandyString.GetFirstLetter<int>(line, out subLine);
            //---------->

            Trigger_WeaponGiver wg = new Trigger_WeaponGiver(id, subLine);

            wg.SetEntityType(type_of_weapon);

            //add it to the appropriate vectors
            m_TriggerSystem.Register(wg);

            //let the corresponding navgraph node point to this object
            NavGraphNode node = m_pNavGraph.GetNode(wg.GraphNodeIndex());

            node.SetExtraInfo(wg);

            //register the entity 
            SingleO.entityMgr.RegisterEntity(wg);
        }
        void AddDoor(string line)
        {
            //---------->
            string subLine;
            int id = HandyString.GetFirstLetter<int>(line, out subLine);
            //---------->

            Raven_Door pDoor = new Raven_Door(this, id, subLine);

            m_Doors.Add(pDoor);

            //register the entity 
            SingleO.entityMgr.RegisterEntity(pDoor);
        }
        void AddDoorTrigger(string line)
        {
            //---------->
            string subLine;
            int id = HandyString.GetFirstLetter<int>(line, out subLine);
            //---------->

            Trigger_OnButtonSendMsg<Raven_Bot> tr = new Trigger_OnButtonSendMsg<Raven_Bot>(id, subLine);

            m_TriggerSystem.Register(tr);

            //register the entity 
            SingleO.entityMgr.RegisterEntity(tr);

        }

        void Clear()
        {
            //delete the triggers
            m_TriggerSystem.Clear();

            //delete the doors
            //std::vector<Raven_Door*>::iterator curDoor = m_Doors.begin();
            //for (curDoor; curDoor != m_Doors.end(); ++curDoor)
            //{
            //    delete* curDoor;
            //}

            //m_Doors.clear();

            //std::vector<Wall2D*>::iterator curWall = m_Walls.begin();
            //for (curWall; curWall != m_Walls.end(); ++curWall)
            //{
            //    delete* curWall;
            //}

            m_Walls.Clear();
            m_SpawnPoints.Clear();

            //delete the navgraph
            //delete m_pNavGraph;

            //delete the partioning info
            //delete m_pSpacePartition;
        }


        public Raven_Map()
        {
            m_pNavGraph = null;
            m_pSpacePartition = null;
            m_iSizeY = 0;
            m_iSizeX = 0;
            m_dCellSpaceNeighborhoodRange = 0;
        }
        //~Raven_Map();

        public void Render()
        {
            //render the navgraph
            if (UserOptions.m_bShowGraph)
            {
                HandyGraph.GraphHelper_DrawUsingGDI(m_pNavGraph, Color.grey, UserOptions.m_bShowNodeIndices);
            }

            //render any doors
            foreach (Raven_Door curDoor in m_Doors)
            {
                (curDoor).Render();
            }

            //render all the triggers
            m_TriggerSystem.Render();

            //render all the walls
            foreach (Wall2D curWall in m_Walls)
            {
                //gdi->ThickBlackPen();
                (curWall).Render();
            }

            foreach (Vector3 curSp in m_SpawnPoints)
            {
                DebugWide.DrawCircle(curSp, 7, Color.grey);
            }
        }

        //loads an environment from a file
        public bool LoadMap(string filename)
        {

            StreamReader stream = FileToStream.FileLoading(filename); 

            Clear();

            BaseGameEntity.ResetNextValidID();

            //first of all read and create the navgraph. This must be done before
            //the entities are read from the map file because many of the entities
            //will be linked to a graph node (the graph node will own a pointer
            //to an instance of the entity)
            m_pNavGraph = new SparseGraph(false);

            m_pNavGraph.Load_Nav(stream);


            DebugWide.LogBlue("NavGraph for " + filename + " loaded okay");

            //determine the average distance between graph nodes so that we can
            //partition them efficiently
            m_dCellSpaceNeighborhoodRange = HandyGraph.CalculateAverageGraphEdgeLength(m_pNavGraph) + 1;


            DebugWide.LogBlue("Average edge length is " + HandyGraph.CalculateAverageGraphEdgeLength(m_pNavGraph));


            DebugWide.LogBlue("Neighborhood range set to " + m_dCellSpaceNeighborhoodRange);

            //load in the map size and adjust the client window accordingly
            string line = stream.ReadLine();
            string[] sp = HandyString.SplitBlank(line);
            m_iSizeX = int.Parse(sp[0]);
            m_iSizeY = int.Parse(sp[1]);

            DebugWide.LogBlue("Partitioning navgraph nodes...");

            //partition the graph nodes
            PartitionNavGraph();


            //get the handle to the game window and resize the client area to accommodate
            //the map
            //extern char* g_szApplicationName;
            //extern char* g_szWindowClassName;
            //HWND hwnd = FindWindow(g_szWindowClassName, g_szApplicationName);
            //const int ExtraHeightRqdToDisplayInfo = 50;
            //ResizeWindow(hwnd, m_iSizeX, m_iSizeY+ExtraHeightRqdToDisplayInfo);


            DebugWide.LogBlue("Loading map...");

            //now create the environment entities
            while ((line = stream.ReadLine()) != null)
            {
                //get type of next map object
                //---------->
                string subLine;
                int entityType = HandyString.GetFirstLetter<int>(line, out subLine);
                //---------->

                DebugWide.LogBlue("Creating a " + (eObjType)(entityType));


                //create the object
                switch (entityType)
                {
                    case (int)eObjType.wall:

                        AddWall(subLine); break;

                    case (int)eObjType.sliding_door:

                        AddDoor(subLine); break;

                    case (int)eObjType.door_trigger:

                        AddDoorTrigger(subLine); break;

                    case (int)eObjType.spawn_point:

                        AddSpawnPoint(subLine); break;

                    case (int)eObjType.health:

                        AddHealth_Giver(subLine); break;

                    case (int)eObjType.shotgun:

                        AddWeapon_Giver(entityType, subLine); break;

                    case (int)eObjType.rail_gun:

                        AddWeapon_Giver(entityType, subLine); break;

                    case (int)eObjType.rocket_launcher:

                        AddWeapon_Giver(entityType, subLine); break;

                    default:

                        DebugWide.LogError("<Map::Load>: Attempting to load undefined object");
                        return false;

                }//end switch
            }


            DebugWide.LogBlue(filename + " loaded okay");

            //calculate the cost lookup table
            m_PathCosts = HandyGraph.CreateAllPairsCostsTable(m_pNavGraph);

            stream.Close(); //파일닫기 

            return true;
        }


        //public void AddSoundTrigger(Raven_Bot pSoundSource, float range)
        //{
        //    m_TriggerSystem.Register(new Trigger_SoundNotify(pSoundSource, range));
        //}

        public float CalculateCostToTravelBetweenNodes(int nd1, int nd2)
        {
            //assert(nd1>=0 && nd1<m_pNavGraph->NumNodes() &&
            //nd2>=0 && nd2<m_pNavGraph->NumNodes() &&
            //"<Raven_Map::CostBetweenNodes>: invalid index");

            return m_PathCosts[nd1][nd2];
        }

        //returns the position of a graph node selected at random
        public Vector3 GetRandomNodeLocation()
        {
            int y = m_pNavGraph.NumActiveNodes() - 1;
            if (y < 0) y = 0;
            int RandIndex = Misc.RandInt(0, y);

            return m_pNavGraph.GetNode(RandIndex).Pos();
        }


        public void UpdateTriggerSystem(LinkedList<Raven_Bot> bots)
        {
            m_TriggerSystem.Update(bots);
        }

        public TriggerList GetTriggers() { return m_TriggerSystem.GetTriggers(); }
        public List<Wall2D> GetWalls() { return m_Walls; }
        public SparseGraph GetNavGraph() { return m_pNavGraph; }
        public List<Raven_Door> GetDoors() { return m_Doors; }
        public List<Vector3> GetSpawnPoints() { return m_SpawnPoints; }
        public CellSpacePartition<NavGraphNode> GetCellSpace() { return m_pSpacePartition; }
        public Vector3 GetRandomSpawnPoint() { return m_SpawnPoints[Misc.RandInt(0, m_SpawnPoints.Count - 1)]; }
        public int GetSizeX() { return m_iSizeX; }
        public int GetSizeY() { return m_iSizeY; }
        public int GetMaxDimension() { return Math.Max(m_iSizeX, m_iSizeY); }
        public float GetCellSpaceNeighborhoodRange() { return m_dCellSpaceNeighborhoodRange; }

    }
    //*/
}//end namespace

