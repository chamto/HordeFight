using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace SteeringBehavior
{

    public static class SingleO
    {
        public static Param prm = null;
    }

    [System.Serializable]
    public class Test_SteeringBehavior : MonoBehaviour
    {
        public Param _prm = new Param();

        //a container of all the moving entities
        List<Vehicle> m_Vehicles = new List<Vehicle>();

        //any obstacles
        List<BaseGameEntity> m_Obstacles = new List<BaseGameEntity>();

        //container containing any walls in the environment
        List<Wall2D> m_Walls = new List<Wall2D>();

        CellSpacePartition<Vehicle> m_pCellSpace;

        //any path we may create for the vehicles to follow
        Path m_pPath;

        //set true to pause the motion
        bool m_bPaused;

        //local copy of client window dimensions
        int m_cxClient,
                                      m_cyClient;
        //the position of the crosshair
        Vector2 m_vCrosshair;

        //keeps track of the average FPS
        float m_dAvFrameTime;


        //flags to turn aids and obstacles etc on/off
        bool m_bShowWalls;
        bool m_bShowObstacles;
        bool m_bShowPath;
        bool m_bShowDetectionBox;
        bool m_bShowWanderCircle;
        bool m_bShowFeelers;
        bool m_bShowSteeringForce;
        bool m_bShowFPS;
        bool m_bRenderNeighbors;
        bool m_bViewKeys;
        bool m_bShowCellSpaceInfo;

        //void Awake()
        //{
        //    QualitySettings.vSyncCount = 0;
        //    Application.targetFrameRate = 60;
        //}

        void CreateObstacles() 
        {
            //create a number of randomly sized tiddlywinks
            for (int o = 0; o < SingleO.prm.NumObstacles; ++o)
            {
                bool bOverlapped = true;

                //keep creating tiddlywinks until we find one that doesn't overlap
                //any others.Sometimes this can get into an endless loop because the
                //obstacle has nowhere to fit. We test for this case and exit accordingly

                int NumTrys = 0; int NumAllowableTrys = 2000;

                while (bOverlapped)
                {
                    NumTrys++;

                    if (NumTrys > NumAllowableTrys) return;

                    int radius = Misc.RandInt((int)SingleO.prm.MinObstacleRadius, (int)SingleO.prm.MaxObstacleRadius);

                    const int border = 10;
                    const int MinGapBetweenObstacles = 20;

                    Obstacle ob = new Obstacle(Misc.RandInt(radius + border, m_cxClient - radius - border),
                                                Misc.RandInt(radius + border, m_cyClient - radius - 30 - border),
                                                radius);

                    if (!Util.Overlapped(ob, m_Obstacles, MinGapBetweenObstacles))
                    {
                        //its not overlapped so we can add it
                        m_Obstacles.Add(ob);

                        bOverlapped = false;
                    }

                }
            }
        }

        void CreateWalls()
        {
            //create the walls  
            float bordersize = 20.0f;
            float CornerSize = 0.2f;
            float vDist = m_cyClient - 2 * bordersize;
            float hDist = m_cxClient - 2 * bordersize;

            const int NumWallVerts = 8;

            Vector2[] walls = new Vector2[NumWallVerts]
                {
                                   new Vector2(hDist*CornerSize+bordersize, bordersize),
                                   new Vector2(m_cxClient-bordersize-hDist*CornerSize, bordersize),
                                   new Vector2(m_cxClient-bordersize, bordersize+vDist*CornerSize),
                                   new Vector2(m_cxClient-bordersize, m_cyClient-bordersize-vDist*CornerSize),
                                   new Vector2(m_cxClient-bordersize-hDist*CornerSize, m_cyClient-bordersize),
                                   new Vector2(hDist*CornerSize+bordersize, m_cyClient-bordersize),
                                   new Vector2(bordersize, m_cyClient-bordersize-vDist*CornerSize),
                                   new Vector2(bordersize, bordersize+vDist*CornerSize)
                };

            for (int w = 0; w < NumWallVerts - 1; ++w)
            {
                m_Walls.Add(new Wall2D(walls[w], walls[w + 1]));
            }

            m_Walls.Add(new Wall2D(walls[NumWallVerts - 1], walls[0]));
        }


        public void Init_GameWorld()
        {
            m_cxClient = SingleO.prm.cxClient;
            m_cyClient = SingleO.prm.cyClient;
            int cx = m_cxClient, cy = m_cyClient;
            m_bPaused = false;
            m_vCrosshair = new Vector2(cxClient() / 2.0f, cxClient() / 2.0f);
            m_bShowWalls = false;
            m_bShowObstacles = false;
            m_bShowPath = false;
            m_bShowWanderCircle = false;
            m_bShowSteeringForce = false;
            m_bShowFeelers = false;
            m_bShowDetectionBox = false;
            m_bShowFPS = true;
            m_dAvFrameTime = 0;
            m_pPath = null;
            m_bRenderNeighbors = false;
            m_bViewKeys = false;
            m_bShowCellSpaceInfo = false;

            //setup the spatial subdivision class
            m_pCellSpace = new CellSpacePartition<Vehicle>((float)cx, (float)cy, SingleO.prm.NumCellsX, SingleO.prm.NumCellsY, SingleO.prm.NumAgents);

            float border = 30;
            m_pPath = new Path(5, border, border, cx - border, cy - border, true);


            //setup the agents
            for (int a = 0; a < SingleO.prm.NumAgents; ++a)
            {

                //determine a random starting position
                Vector2 SpawnPos = new Vector2(cx / 2.0f + Misc.RandomClamped() * cx / 2.0f,
                                             cy / 2.0f + Misc.RandomClamped() * cy / 2.0f);


                Vehicle pVehicle = new Vehicle(this,
                                                SpawnPos,                 //initial position
                                                Misc.RandFloat() * Const.TwoPi,        //start rotation
                                               Vector2.zero,            //velocity
                                               SingleO.prm.VehicleMass,          //mass
                                               SingleO.prm.MaxSteeringForce_2,     //max force
                                               SingleO.prm.MaxSpeed,             //max velocity
                                               SingleO.prm.MaxTurnRatePerSecond, //max turn rate
                                               SingleO.prm.VehicleScale);        //scale

                pVehicle.Steering().FlockingOn();
                //pVehicle.Steering().WanderOn();

                m_Vehicles.Add(pVehicle);
                //DebugWide.LogBlue(pVehicle.Pos() + " ---- ");
                //add it to the cell subdivision
                m_pCellSpace.AddEntity(pVehicle);
            }


//#define SHOAL
//# ifdef SHOAL

//#endif

            //create any obstacles or walls
            //CreateObstacles();
            //CreateWalls();
        }

        bool __isShoal = false;
        public void SwitchShoal()
        {
            __isShoal = !__isShoal;

            if (__isShoal)
            {
                m_Vehicles[SingleO.prm.NumAgents - 1].Steering().FlockingOff();
                m_Vehicles[SingleO.prm.NumAgents - 1].SetScale(new Vector2(10, 10));
                m_Vehicles[SingleO.prm.NumAgents - 1].Steering().WanderOn();
                m_Vehicles[SingleO.prm.NumAgents - 1].SetMaxSpeed(70);


                for (int i = 0; i < SingleO.prm.NumAgents - 1; ++i)
                {
                    m_Vehicles[i].Steering().EvadeOn(m_Vehicles[SingleO.prm.NumAgents - 1]);

                }
            }
            else
            {
                m_Vehicles[SingleO.prm.NumAgents - 1].Steering().FlockingOn();
                m_Vehicles[SingleO.prm.NumAgents - 1].SetScale(SingleO.prm.VehicleScale);
                m_Vehicles[SingleO.prm.NumAgents - 1].SetMaxSpeed(SingleO.prm.MaxSpeed);


                for (int i = 0; i < SingleO.prm.NumAgents - 1; ++i)
                {
                    m_Vehicles[i].Steering().EvadeOff();

                }
            }

        }

        public void Start()
		{
            SingleO.prm = _prm;
            SingleO.prm.Update(); //가장먼저 갱신되어야 함 

            const int SampleRate = 10;
            _FrameRateSmoother = new Buckland.Smoother<float, Buckland.FloatCalc>(SampleRate, 0.0f);

            Init_GameWorld();
		}


        Buckland.Smoother<float, Buckland.FloatCalc> _FrameRateSmoother;
        public void Update()
        {
            
            SingleO.prm.Update(); //인스펙터 파라메타값 갱신 

            HandleInputPresses();

            if (m_bPaused) return;

            //create a smoother to smooth the framerate
            m_dAvFrameTime = _FrameRateSmoother.Update(Time.deltaTime);


            //update the vehicles
            for (int a = 0; a < m_Vehicles.Count; ++a)
            {
                m_Vehicles[a].Update(Time.deltaTime);
            }

        }

		public void OnDrawGizmos()
		{
            Render();
		}
		public void Render()
        {

            //if (null == m_Walls) return;
            //render any walls
            //gdi->BlackPen();
            for (int w = 0; w < m_Walls.Count; ++w)
            {
                m_Walls[w].Render(true);  //true flag shows normals
            }

            //render any obstacles
            //gdi->BlackPen();
            for (int ob = 0; ob < m_Obstacles.Count; ++ob)
            {
                //gdi->Circle(m_Obstacles[ob].Pos(), m_Obstacles[ob].BRadius());
                DebugWide.DrawCircle(m_Obstacles[ob].Pos(), m_Obstacles[ob].BRadius(), Color.black);
            }

            //render the agents
            for ( int a = 0; a < m_Vehicles.Count; ++a)
            {
                    m_Vehicles[a].Render();
                //DebugWide.LogBlue(a + " ----OO ");
                    //render cell partitioning stuff
                    if (m_bShowCellSpaceInfo && a == 0)
                    {
                        //gdi->HollowBrush();
                        float viewDis = SingleO.prm.ViewDistance;
                        InvertedAABBox2D box = new InvertedAABBox2D(m_Vehicles[a].Pos() - new Vector2(viewDis, viewDis),
                                                        m_Vehicles[a].Pos() + new Vector2(viewDis, viewDis));
                        box.Render();

                        //gdi->RedPen();
                        CellSpace().CalculateNeighbors(m_Vehicles[a].Pos(), viewDis);
                        foreach(BaseGameEntity pV in CellSpace().GetNeighbors())
                        {
                            DebugWide.DrawCircle(pV.Pos(), pV.BRadius(), Color.red);
                        }
                    
                        DebugWide.DrawCircle(m_Vehicles[a].Pos(), viewDis, Color.green);
                    }
            }

                //#define CROSSHAIR
            //# ifdef CROSSHAIR
                //and finally the crosshair
            Color color = Color.red;
            Vector2 start = Vector2.zero, end = Vector2.zero;
            DebugWide.DrawCircle(m_vCrosshair, 4, color);
            start.x = m_vCrosshair.x - 8; start.y = m_vCrosshair.y;
            end.x = m_vCrosshair.x + 8; end.y = m_vCrosshair.y;
            DebugWide.DrawLine(start, end, color);
            start.x = m_vCrosshair.x; start.y = m_vCrosshair.y - 8;
            end.x = m_vCrosshair.x; end.y = m_vCrosshair.y + 8;
            DebugWide.DrawLine(start, end, color);
            DebugWide.PrintText(new Vector3(0, -7), color, "Click to move crosshair");
            //#endif


            //gdi->TextAtPos(cxClient() -120, cyClient() - 20, "Press R to reset");

            //gdi->TextColor(Cgdi::grey);
            color = Color.gray;
            if (RenderPath())
            {
                DebugWide.PrintText(new Vector3(0, -14), color, "Press 'U' for random path");
                m_pPath.Render();
            }

            if (RenderFPS())
            {
                DebugWide.PrintText(new Vector3(0, 0), color, ""+(1.0f / m_dAvFrameTime).ToString("N1"));
            } 

            if (m_bShowCellSpaceInfo)
            {
                m_pCellSpace.RenderCells();
            }
        }


        public void NonPenetrationContraint(Vehicle v) { Util.EnforceNonPenetrationConstraint(v, m_Vehicles); }

        public void TagVehiclesWithinViewRange(BaseGameEntity pVehicle, float range)
        {
            Util.TagNeighbors(pVehicle, m_Vehicles, range);
        }

        public void TagObstaclesWithinViewRange(BaseGameEntity pVehicle, float range)
        {
            Util.TagNeighbors(pVehicle, m_Obstacles, range);
        }

        public List<Wall2D>          Walls() { return m_Walls; }
        public CellSpacePartition<Vehicle> CellSpace() { return m_pCellSpace; }
        public List<BaseGameEntity> Obstacles() {return m_Obstacles;}
        public List<Vehicle>       Agents() { return m_Vehicles; }


        //handle WM_COMMAND messages
        public void HandleInputPresses()
        {

            SetCrosshair(Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.U))
            {
                //delete m_pPath;
                float border = 60;
                m_pPath = new Path(Misc.RandInt(3, 7), border, border, cxClient() - border, cyClient() - border, true);
                m_bShowPath = true;
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].Steering().SetPath(m_pPath.GetPath());
                }
            }

            //DebugWide.LogBlue(m_bPaused);
            if (Input.GetKeyDown(KeyCode.P))
                TogglePause(); 

            if (Input.GetKeyDown(KeyCode.O))
                ToggleRenderNeighbors(); 

            if (Input.GetKeyDown(KeyCode.I))
            {
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].ToggleSmoothing();
                }

            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                m_bShowObstacles = !m_bShowObstacles;

                if (!m_bShowObstacles)
                {
                    m_Obstacles.Clear();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_Vehicles[i].Steering().ObstacleAvoidanceOff();
                    }
                }
                else
                {
                    CreateObstacles();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_Vehicles[i].Steering().ObstacleAvoidanceOn();
                    }
                }
            }

        }

        void OnGUI()
        {
            int count = 0;
            int x = 120 , y = 20;
            if (GUI.Button(new Rect(x * count, 0, x, y), new GUIContent("1 OB_OBSTACLES")))
            {
                m_bShowObstacles = !m_bShowObstacles;

                if (!m_bShowObstacles)
                {
                    m_Obstacles.Clear();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_Vehicles[i].Steering().ObstacleAvoidanceOff();
                    }

                }
                else
                {
                    CreateObstacles();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_Vehicles[i].Steering().ObstacleAvoidanceOn();
                    }

                }

            }
            count++;
            if (GUI.Button(new Rect(x * count, 0, x, y), new GUIContent("2 OB_WALLS")))
            {
                m_bShowWalls = !m_bShowWalls;

                if (m_bShowWalls)
                {
                    CreateWalls();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_Vehicles[i].Steering().WallAvoidanceOn();
                    }

                }

                else
                {
                    m_Walls.Clear();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_Vehicles[i].Steering().WallAvoidanceOff();
                    }

                }
            }
            count++;
            if (GUI.Button(new Rect(x * count, 0, x, y), new GUIContent("3 PARTITIONING")))
            {
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].Steering().ToggleSpacePartitioningOnOff();
                }

                //if toggled on, empty the cell space and then re-add all the 
                //vehicles
                if (m_Vehicles[0].Steering().isSpacePartitioningOn())
                {
                    m_pCellSpace.EmptyCells();

                    for (int i = 0; i < m_Vehicles.Count; ++i)
                    {
                        m_pCellSpace.AddEntity(m_Vehicles[i]);
                    }

                }
                else
                {
                    m_bShowCellSpaceInfo = false;
                }
            }
            count=0;
            if (GUI.Button(new Rect(x * count, y, x, y), new GUIContent("4 PARTITION_VIEW_NEIGHBORS")))
            {
                m_bShowCellSpaceInfo = !m_bShowCellSpaceInfo;

                if (m_bShowCellSpaceInfo)
                {
                    if (!m_Vehicles[0].Steering().isSpacePartitioningOn())
                    {
                        //SendMessage(hwnd, WM_COMMAND, IDR_PARTITIONING, NULL);
                    }
                }
            }
            count++;
            if (GUI.Button(new Rect(x * count, y, x, y), new GUIContent("5 WEIGHTED_SUM")))
            {
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].Steering().SetSummingMethod(SteeringBehavior.SummingMethod.weighted_average);
                }
            }
            count++;
            if (GUI.Button(new Rect(x * count, y, x, y), new GUIContent("6 PRIORITIZED")))
            {
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].Steering().SetSummingMethod(SteeringBehavior.SummingMethod.prioritized);
                }
            }
            count=0;
            if (GUI.Button(new Rect(x * count, y*2, x, y), new GUIContent("7 DITHERED")))
            {
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].Steering().SetSummingMethod(SteeringBehavior.SummingMethod.dithered);
                }
            }
            count++;
            if (GUI.Button(new Rect(x * count, y*2, x, y), new GUIContent("8 VIEW_KEYS")))
            {
                ToggleViewKeys();
            }
            count++;
            if (GUI.Button(new Rect(x * count, y*2, x, y), new GUIContent("9 VIEW_FPS")))
            {
                ToggleShowFPS();
            }
            count=0;
            if (GUI.Button(new Rect(x * count, y*3, x, y), new GUIContent("10 SMOOTHING")))
            {
                for (int i = 0; i < m_Vehicles.Count; ++i)
                {
                    m_Vehicles[i].ToggleSmoothing();
                }
            }
            count++;
            if (GUI.Button(new Rect(x * count, y * 3, x, y), new GUIContent("11 SwitchShoal")))
            {
                SwitchShoal();
            }


        }


        public void TogglePause() { m_bPaused = !m_bPaused; }
        public bool Paused() {return m_bPaused;}

        public Vector2 Crosshair() {return m_vCrosshair;}

        public void SetCrosshair(Vector2 v) { m_vCrosshair = v; }

        public int cxClient() {return m_cxClient;}
        public int cyClient() {return m_cyClient;}
 
        public bool RenderWalls() {return m_bShowWalls;}
        public bool RenderObstacles() {return m_bShowObstacles;}
        public bool RenderPath() {return m_bShowPath;}
        public bool RenderDetectionBox() {return m_bShowDetectionBox;}
        public bool RenderWanderCircle() {return m_bShowWanderCircle;}
        public bool RenderFeelers() {return m_bShowFeelers;}
        public bool RenderSteeringForce() {return m_bShowSteeringForce;}

        public bool RenderFPS() {return m_bShowFPS;}
        public void ToggleShowFPS() { m_bShowFPS = !m_bShowFPS; }

        public void ToggleRenderNeighbors() { m_bRenderNeighbors = !m_bRenderNeighbors; }
        public bool RenderNeighbors() {return m_bRenderNeighbors;}
  
        public void ToggleViewKeys() { m_bViewKeys = !m_bViewKeys; }
        public bool ViewKeys() {return m_bViewKeys;}
    }//end class 
}

