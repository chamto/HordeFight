using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilGS9;


namespace Raven
{

    /*
    public class Raven_Game
    {
        LinkedList<Raven_Bot> m_Bots = new LinkedList<Raven_Bot>();
        Raven_Map m_pMap = new Raven_Map();
        PathManager<Raven_PathPlanner> m_pPathManager = new PathManager<Raven_PathPlanner>(0);

        public Raven_Map GetMap() { return m_pMap; }
        public PathManager<Raven_PathPlanner> GetPathManager() { return m_pPathManager; }
        public bool isLOSOkay(Vector3 a, Vector3 b) { return false; }
        public LinkedList<Raven_Bot> GetAllBots() { return m_Bots; }

        public void TagRaven_BotsWithinViewRange(BaseGameEntity pRaven_Bot, float range)
        { TagNeighbors(pRaven_Bot, m_Bots, range); }


        void TagNeighbors(BaseGameEntity entity, LinkedList<Raven_Bot> others, float radius)
        {

            foreach (Raven_Bot it in others)
            {
                //first clear any current tag
                (it).UnTag();

                //work in distance squared to avoid sqrts
                Vector3 to = (it).Pos() - entity.Pos();

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + (it).BRadius();

                //if entity within range, tag for further consideration
                if (((it) != entity) && (to.sqrMagnitude < range * range))
                {
                    (it).Tag();
                }

            }//next entity
        }

        public bool isPathObstructed(Vector3 a, Vector3 b, float f) { return false; }
    }
    //*/

    //*
    public class Raven_Game
    {

        //the current game map
        Raven_Map m_pMap;

        //a list of all the bots that are inhabiting the map
        LinkedList<Raven_Bot> m_Bots = new LinkedList<Raven_Bot>();

        //the user may select a bot to control manually. This is a pointer to that
        //bot
        Raven_Bot m_pSelectedBot;

        //this list contains any active projectiles (slugs, rockets,
        //shotgun pellets, etc)
        LinkedList<Raven_Projectile> m_Projectiles = new LinkedList<Raven_Projectile>();

        //this class manages all the path planning requests
        PathManager<Raven_PathPlanner> m_pPathManager;


        //if true the game will be paused
        bool m_bPaused;

        //if true a bot is removed from the game
        bool m_bRemoveABot;

        //when a bot is killed a "grave" is displayed for a few seconds. This
        //class manages the graves
        GraveMarkers m_pGraveMarkers;

        //this iterates through each trigger, testing each one against each bot
        //void UpdateTriggers();

        //deletes all entities, empties all containers and creates a new navgraph 
        void Clear()
        {

            DebugWide.LogBlue("\n------------------------------ Clearup -------------------------------");

            foreach (Raven_Bot it in m_Bots)
            {

                DebugWide.LogBlue("deleting entity id: " + it.ID() + " of type "
                          + (eObjType)((it).EntityType()) + "(" + (it).EntityType() + ")");


                //delete* it;
            }

            //delete any active projectiles
            foreach (Raven_Projectile curW in m_Projectiles)
            {

                DebugWide.LogBlue("deleting projectile id: " + (curW).ID());


                //delete* curW;
            }

            //clear the containers
            m_Projectiles.Clear();
            m_Bots.Clear();

            m_pSelectedBot = null;


        }

        //attempts to position a spawning bot at a free spawn point. returns false
        //if unsuccessful 
        bool AttemptToAddBot(Raven_Bot pBot)
        {
            //make sure there are some spawn points available
            if (m_pMap.GetSpawnPoints().Count <= 0)
            {
                DebugWide.LogError("Map has no spawn points!");
                return false;
            }

            //we'll make the same number of attempts to spawn a bot this update as
            //there are spawn points
            int attempts = m_pMap.GetSpawnPoints().Count;

            while (--attempts >= 0)
            {
                //select a random spawn point
                Vector3 pos = m_pMap.GetRandomSpawnPoint();

                //check to see if it's occupied
                bool bAvailable = true;
                foreach (Raven_Bot curBot in m_Bots)
                {
                    //if the spawn point is unoccupied spawn a bot
                    if ((pos - (curBot).Pos()).magnitude < (curBot).BRadius())
                    {
                        bAvailable = false;
                    }
                }

                if (bAvailable)
                {
                    pBot.Spawn(pos);

                    return true;
                }
            }

            return false;
        }

        //when a bot is removed from the game by a user all remaining bots
        //must be notified so that they can remove any references to that bot from
        //their memory
        void NotifyAllBotsOfRemoval(Raven_Bot pRemovedBot)
        {
            foreach (Raven_Bot curBot in m_Bots)
            {

                SingleO.dispatcher.DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                      Const.SENDER_ID_IRRELEVANT,
                                      (curBot).ID(),
                                      (int)eMsg.UserHasRemovedBot,
                                      pRemovedBot);

            }
        }


        public Raven_Game()
        {
            m_pSelectedBot = null;
            m_bPaused = false;
            m_bRemoveABot = false;
            m_pMap = null;
            m_pPathManager = null;
            m_pGraveMarkers = null;

            //load in the default map
            LoadMap(Params.StartMap);
        }
        //~Raven_Game();

        //the usual suspects
        public void Render()
        {

            m_pGraveMarkers.Render();

            //render the map
            m_pMap.Render();

            //render all the bots unless the user has selected the option to only 
            //render those bots that are in the fov of the selected bot
            if (null != m_pSelectedBot && UserOptions.m_bOnlyShowBotsInTargetsFOV)
            {
                List<Raven_Bot> VisibleBots = GetAllBotsInFOV(m_pSelectedBot);

                foreach (Raven_Bot it in VisibleBots)
                {
                    (it).Render();
                }


                if (null != m_pSelectedBot) m_pSelectedBot.Render();
            }

            else
            {

                //render all the entities
                foreach (Raven_Bot curBot in m_Bots)
                {

                    if ((curBot).isAlive())
                    {
                        (curBot).Render();
                    }
                }
            }

            //render any projectiles
            foreach (Raven_Projectile curW in m_Projectiles)
            {
                (curW).Render();
            }

            // gdi->TextAtPos(300, WindowHeight - 70, "Num Current Searches: " + ttos(m_pPathManager->GetNumActiveSearches()));

            //render a red circle around the selected bot (blue if possessed)
            if (null != m_pSelectedBot)
            {
                if (m_pSelectedBot.isPossessed())
                {
                    DebugWide.DrawCircle(m_pSelectedBot.Pos(), m_pSelectedBot.BRadius() + 1, Color.blue);
                }
                else
                {
                    DebugWide.DrawCircle(m_pSelectedBot.Pos(), m_pSelectedBot.BRadius() + 1, Color.red);
                }


                if (UserOptions.m_bShowOpponentsSensedBySelectedBot)
                {
                    m_pSelectedBot.GetSensoryMem().RenderBoxesAroundRecentlySensed();
                }

                //render a square around the bot's target
                if (UserOptions.m_bShowTargetOfSelectedBot && null != m_pSelectedBot.GetTargetBot())
                {

                    Vector3 p = m_pSelectedBot.GetTargetBot().Pos();
                    float b = m_pSelectedBot.GetTargetBot().BRadius();

                    DebugWide.DrawLine(new Vector3(p.x - b, p.y, p.z - b), new Vector3(p.x + b, p.y, p.z - b), Color.red);
                    DebugWide.DrawLine(new Vector3(p.x + b, p.y, p.z - b), new Vector3(p.x + b, p.y, p.z + b), Color.red);
                    DebugWide.DrawLine(new Vector3(p.x + b, p.y, p.z + b), new Vector3(p.x - b, p.y, p.z + b), Color.red);
                    DebugWide.DrawLine(new Vector3(p.x - b, p.y, p.z + b), new Vector3(p.x - b, p.y, p.z - b), Color.red);
                    //gdi->Line(p.x - b, p.y - b, p.x + b, p.y - b);
                    //gdi->Line(p.x + b, p.y - b, p.x + b, p.y + b);
                    //gdi->Line(p.x + b, p.y + b, p.x - b, p.y + b);
                    //gdi->Line(p.x - b, p.y + b, p.x - b, p.y - b);
                }


                //render the path of the bot
                if (UserOptions.m_bShowPathOfSelectedBot)
                {
                    m_pSelectedBot.GetBrain().Render();
                }

                //display the bot's goal stack
                if (UserOptions.m_bShowGoalsOfSelectedBot)
                {
                    Vector3 p = m_pSelectedBot.Pos();
                    p.x -= 50;

                    m_pSelectedBot.GetBrain().RenderAtPos(p);
                }

                if (UserOptions.m_bShowGoalAppraisals)
                {
                    m_pSelectedBot.GetBrain().RenderEvaluations(new Vector3(0, 0, 15));
                }

                if (UserOptions.m_bShowWeaponAppraisals)
                {
                    m_pSelectedBot.GetWeaponSys().RenderDesirabilities();
                }


                if (Input.GetKey(KeyCode.Q) && m_pSelectedBot.isPossessed())
                {
                    Vector3 wpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    DebugWide.PrintText(wpos, Color.red, "Queuing");
                }
            }
        }

        public void Update()
        {

            //don't update if the user has paused the game
            if (m_bPaused) return;

            m_pGraveMarkers.Update();

            //get any player keyboard input
            GetPlayerInput();

            //update all the queued searches in the path manager
            m_pPathManager.UpdateSearches();

            //update any doors
            foreach (Raven_Door curDoor in m_pMap.GetDoors())
            {
                (curDoor).Update();
            }

            //update any current projectiles
            foreach (Raven_Projectile curW in m_Projectiles)
            {
                //test for any dead projectiles and remove them if necessary
                if (!(curW).isDead())
                {
                    (curW).Update();

                    //++curW;
                }
                else
                {
                    //delete* curW;

                    //curW = m_Projectiles.erase(curW);
                    m_Projectiles.Remove(curW);
                }
            }

            //update the bots
            bool bSpawnPossible = true;

            foreach (Raven_Bot curBot in m_Bots)
            {
                //DebugWide.LogBlue(curBot.m_Status + "  " + curBot.ID() + "  " + curBot.Pos() +  "  " + curBot.Health() );

                //if this bot's status is 'respawning' attempt to resurrect it from
                //an unoccupied spawn point
                if ((curBot).isSpawning() && bSpawnPossible)
                {
                    bSpawnPossible = AttemptToAddBot(curBot);
                }

                //if this bot's status is 'dead' add a grave at its current location 
                //then change its status to 'respawning'
                else if ((curBot).isDead())
                {
                    //create a grave
                    m_pGraveMarkers.AddGrave((curBot).Pos());

                    //change its status to spawning
                    (curBot).SetSpawning();
                }

                //if this bot is alive update it.
                else if ((curBot).isAlive())
                {
                    (curBot).Update();
                }
            }

            //update the triggers
            m_pMap.UpdateTriggerSystem(m_Bots);

            //if the user has requested that the number of bots be decreased, remove
            //one
            if (m_bRemoveABot)
            {
                if (0 != m_Bots.Count)
                {
                    Raven_Bot pBot = m_Bots.Last.Value;
                    if (pBot == m_pSelectedBot) m_pSelectedBot = null;
                    NotifyAllBotsOfRemoval(pBot);
                    //delete m_Bots.back();
                    m_Bots.Remove(pBot);

                    pBot = null;
                }

                m_bRemoveABot = false;
            }
        }

        //loads an environment from a file
        public bool LoadMap(string filename)
        {
            //clear any current bots and projectiles
            Clear();

            //out with the old
            //delete m_pMap;
            //delete m_pGraveMarkers;
            //delete m_pPathManager;

            //in with the new
            m_pGraveMarkers = new GraveMarkers(Params.GraveLifetime);
            m_pPathManager = new PathManager<Raven_PathPlanner>(Params.MaxSearchCyclesPerUpdateStep);
            m_pMap = new Raven_Map();

            //make sure the entity manager is reset
            SingleO.entityMgr.Reset();


            //load the new map data
            if (m_pMap.LoadMap(filename))
            {
                AddBots(Params.NumBots);

                return true;
            }

            return false;
        }

        public void AddBots(int NumBotsToAdd)
        {
            while (0 != NumBotsToAdd--)
            {
                //create a bot. (its position is irrelevant at this point because it will
                //not be rendered until it is spawned)
                Raven_Bot rb = new Raven_Bot(this, ConstV.v3_zero);

                //switch the default steering behaviors on
                rb.GetSteering().WallAvoidanceOn();
                rb.GetSteering().SeparationOn();

                m_Bots.AddLast(rb);

                //register the bot with the entity manager
                SingleO.entityMgr.RegisterEntity(rb);

                DebugWide.LogBlue("Adding bot with ID " + rb.ID());

            }
        }


        public void AddRocket(Raven_Bot shooter, Vector3 target)
        {
            Raven_Projectile rp = new Rocket(shooter, target);

            m_Projectiles.AddLast(rp);

            DebugWide.LogBlue("Adding a rocket " + rp.ID() + " at pos " + rp.Pos());
        }

        public void AddRailGunSlug(Raven_Bot shooter, Vector3 target)
        {
            Raven_Projectile rp = new Slug(shooter, target);

            m_Projectiles.AddLast(rp);


            DebugWide.LogBlue("Adding a rail gun slug" + rp.ID() + " at pos " + rp.Pos());

        }

        public void AddShotGunPellet(Raven_Bot shooter, Vector3 target)
        {
            Raven_Projectile rp = new Pellet(shooter, target);

            m_Projectiles.AddLast(rp);


            DebugWide.LogBlue("Adding a shotgun shell " + rp.ID() + " at pos " + rp.Pos());

        }

        public void AddBolt(Raven_Bot shooter, Vector3 target)
        {
            Raven_Projectile rp = new Bolt(shooter, target);

            m_Projectiles.AddLast(rp);


            DebugWide.LogBlue("Adding a bolt " + rp.ID() + " at pos " + rp.Pos());

        }

        //removes the last bot to be added
        public void RemoveBot()
        {
            m_bRemoveABot = true;
        }

        //returns true if a bot of size BoundingRadius cannot move from A to B
        //without bumping into world geometry
        public bool isPathObstructed(Vector3 A, Vector3 B, float BoundingRadius)
        {
            Vector3 ToB = VOp.Normalize(B - A);

            Vector3 curPos = A;

            //int count = 0;
            //string str = "";
            while ((curPos - B).sqrMagnitude > BoundingRadius * BoundingRadius)
            {

                //advance curPos one step
                curPos += ToB * 0.5f * BoundingRadius;

                //test all walls against the new position
                if (Wall2D.doWallsIntersectCircle(m_pMap.GetWalls(), curPos, BoundingRadius))
                {
                    return true;
                }

                
                //str += " " + count + " " + (curPos - B).magnitude + "  " + curPos;
                //count++;
                //if(100 < count)
                //{
                //    Vector3 pr = ToB * 0.5f * BoundingRadius;
                //    DebugWide.LogGreen((B - A) + " "  + VOp.Normalize(B - A) + "  " + (B-A).normalized);
                //    DebugWide.LogGreen(pr + "  " + (A+pr));
                //    DebugWide.LogGreen(BoundingRadius + "  " + A + "  " + B + "  " + curPos + "  " + (A-B).magnitude);
                //    DebugWide.LogBlue(str);
                //    return false; 
                //}

            }

            return false;
        }

        //returns a vector of pointers to bots in the FOV of the given bot
        public List<Raven_Bot> GetAllBotsInFOV(Raven_Bot pBot)
        {
            List<Raven_Bot> VisibleBots = new List<Raven_Bot>();

            foreach (Raven_Bot curBot in m_Bots)
            {
                //make sure time is not wasted checking against the same bot or against a
                // bot that is dead or re-spawning
                if (curBot == pBot || !(curBot).isAlive()) continue;

                //first of all test to see if this bot is within the FOV
                if (isSecondInFOVOfFirst(pBot.Pos(),
                                         pBot.Facing(),
                                         (curBot).Pos(),
                                         pBot.FieldOfView()))
                {
                    //cast a ray from between the bots to test visibility. If the bot is
                    //visible add it to the vector
                    if (false == Wall2D.doWallsObstructLineSegment(pBot.Pos(),
                                            (curBot).Pos(),
                                            m_pMap.GetWalls()))
                    {
                        VisibleBots.Add(curBot);
                    }
                }
            }

            return VisibleBots;
        }


        //returns true if the second bot is unobstructed by walls and in the field
        //of view of the first.
        public bool isSecondVisibleToFirst(Raven_Bot pFirst, Raven_Bot pSecond)
        {
            //if the two bots are equal or if one of them is not alive return false
            if (!(pFirst == pSecond) && pSecond.isAlive())
            {
                //first of all test to see if this bot is within the FOV
                if (isSecondInFOVOfFirst(pFirst.Pos(),
                                         pFirst.Facing(),
                                         pSecond.Pos(),
                                         pFirst.FieldOfView()))
                {
                    //test the line segment connecting the bot's positions against the walls.
                    //If the bot is visible add it to the vector
                    if (false == Wall2D.doWallsObstructLineSegment(pFirst.Pos(),
                                                    pSecond.Pos(),
                                                    m_pMap.GetWalls()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //returns true if the ray between A and B is unobstructed.
        public bool isLOSOkay(Vector3 A, Vector3 B)
        {
            return false == Wall2D.doWallsObstructLineSegment(A, B, m_pMap.GetWalls());
        }

        //starting from the given origin and moving in the direction Heading this
        //method returns the distance to the closest wall
        //public float GetDistanceToClosestWall(Vector2D Origin, Vector2D Heading);


        //returns the position of the closest visible switch that triggers the
        //door of the specified ID
        public Vector3 GetPosOfClosestSwitch(Vector3 botPos, int doorID)
        {
            List<int> SwitchIDs = null;

            //first we need to get the ids of the switches attached to this door
            foreach (Raven_Door curDoor in m_pMap.GetDoors())
            {
                if ((curDoor).ID() == doorID)
                {
                    SwitchIDs = (curDoor).GetSwitchIDs(); break;
                }
            }

            Vector3 closest = ConstV.v3_zero;
            float ClosestDist = float.MaxValue;

            //now test to see which one is closest and visible
            foreach (int it in SwitchIDs)
            {
                BaseGameEntity trig = SingleO.entityMgr.GetEntityFromID(it);

                if (isLOSOkay(botPos, trig.Pos()))
                {
                    float dist = (botPos - trig.Pos()).sqrMagnitude;

                    if (dist < ClosestDist)
                    {
                        ClosestDist = dist;
                        closest = trig.Pos();
                    }
                }
            }

            return closest;
        }

        //given a position on the map this method returns the bot found with its
        //bounding radius of that position.If there is no bot at the position the
        //method returns NULL
        public Raven_Bot GetBotAtPosition(Vector3 CursorPos)
        {
            foreach (Raven_Bot curBot in m_Bots)
            {
                if ((curBot.Pos() - CursorPos).magnitude < (curBot).BRadius())
                {
                    if (curBot.isAlive())
                    {
                        return curBot;
                    }
                }
            }

            return null;
        }


        public void TogglePause() { m_bPaused = !m_bPaused; }

        //this method is called when the user clicks the right mouse button.
        //The method checks to see if a bot is beneath the cursor. If so, the bot
        //is recorded as selected.If the cursor is not over a bot then any selected
        // bot/s will attempt to move to that position.
        public void ClickRightMouseButton(Vector3 p)
        {
            Vector3 wpos = Camera.main.ScreenToWorldPoint(p);
            wpos.y = 0;
            Raven_Bot pBot = GetBotAtPosition(wpos);

            //if there is no selected bot just return;
            if (null == pBot && m_pSelectedBot == null) return;

            //if the cursor is over a different bot to the existing selection,
            //change selection
            if (null != pBot && pBot != m_pSelectedBot)
            {
                if (null != m_pSelectedBot) m_pSelectedBot.Exorcise();
                m_pSelectedBot = pBot;

                return;
            }

            //if the user clicks on a selected bot twice it becomes possessed(under
            //the player's control)
            if (null != pBot && pBot == m_pSelectedBot)
            {
                m_pSelectedBot.TakePossession();

                //clear any current goals
                m_pSelectedBot.GetBrain().RemoveAllSubgoals();
            }

            //if the bot is possessed then a right click moves the bot to the cursor
            //position
            if (m_pSelectedBot.isPossessed())
            {
                //if the shift key is pressed down at the same time as clicking then the
                //movement command will be queued
                if (Input.GetKey(KeyCode.Q))
                {
                    m_pSelectedBot.GetBrain().QueueGoal_MoveToPosition(wpos);
                }
                else
                {
                    //clear any current goals
                    m_pSelectedBot.GetBrain().RemoveAllSubgoals();

                    m_pSelectedBot.GetBrain().AddGoal_MoveToPosition(wpos);
                }
            }
        }

        //this method is called when the user clicks the left mouse button. If there
        //is a possessed bot, this fires the weapon, else does nothing
        public void ClickLeftMouseButton(Vector3 p)
        {
            if (null != m_pSelectedBot && m_pSelectedBot.isPossessed())
            {
                Vector3 wpos = Camera.main.ScreenToWorldPoint(p);
                wpos.y = 0;
                m_pSelectedBot.FireWeapon(wpos);
            }
        }

        //when called will release any possessed bot from user control
        public void ExorciseAnyPossessedBot()
        {
            if (null != m_pSelectedBot) m_pSelectedBot.Exorcise();
        }

        //if a bot is possessed the keyboard is polled for user input and any 
        //relevant bot methods are called appropriately
        public void GetPlayerInput()
        {
            if (null != m_pSelectedBot && m_pSelectedBot.isPossessed())
            {
                Vector3 wpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_pSelectedBot.RotateFacingTowardPosition(wpos);
            }
        }

        public Raven_Bot PossessedBot() { return m_pSelectedBot; }

        public void ChangeWeaponOfPossessedBot(int weapon)
        {
            //ensure one of the bots has been possessed
            if (null != m_pSelectedBot)
            {
                switch (weapon)
                {
                    case (int)eObjType.blaster:

                        PossessedBot().ChangeWeapon((int)eObjType.blaster); return;

                    case (int)eObjType.shotgun:

                        PossessedBot().ChangeWeapon((int)eObjType.shotgun); return;

                    case (int)eObjType.rocket_launcher:

                        PossessedBot().ChangeWeapon((int)eObjType.rocket_launcher); return;

                    case (int)eObjType.rail_gun:

                        PossessedBot().ChangeWeapon((int)eObjType.rail_gun); return;

                }
            }
        }


        public Raven_Map GetMap() { return m_pMap; }
        public LinkedList<Raven_Bot> GetAllBots() { return m_Bots; }
        public PathManager<Raven_PathPlanner> GetPathManager() { return m_pPathManager; }
        public int GetNumBots() { return m_Bots.Count; }


        public void TagRaven_BotsWithinViewRange(BaseGameEntity pRaven_Bot, float range)
        { TagNeighbors(pRaven_Bot, m_Bots, range); }


        void TagNeighbors(BaseGameEntity entity, LinkedList<Raven_Bot> others, float radius)
        {

            foreach (Raven_Bot it in others)
            {
                //first clear any current tag
                (it).UnTag();

                //work in distance squared to avoid sqrts
                Vector3 to = (it).Pos() - entity.Pos();

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + (it).BRadius();

                //if entity within range, tag for further consideration
                if (((it) != entity) && (to.sqrMagnitude < range * range))
                {
                    (it).Tag();
                }

            }//next entity
        }

        public bool isSecondInFOVOfFirst(Vector3 posFirst,
                                         Vector3 facingFirst,
                                         Vector3 posSecond,
                                         float fov)
        {
            Vector3 toTarget = VOp.Normalize(posSecond - posFirst);

            return Vector3.Dot(facingFirst, toTarget) >= Math.Cos(fov / 2.0f); // 범위 : -0.5 <= 1 <= 0.5 , cos가 1일때 0도 0.5일때 45도 0일떄 90도 
        }

    }
    //*/
}//end namespace

