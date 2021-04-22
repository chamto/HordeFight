using System.Collections.Generic;
using UnityEngine;
using Buckland;

namespace Test_SimpleSoccer
{

    public class SoccerPitch
    {
        const int NumRegionsHorizontal = 6;
        const int NumRegionsVertical = 3;

        public SoccerBall m_pBall;

        public SoccerTeam m_pRedTeam;
        public SoccerTeam m_pBlueTeam;

        public Goal m_pRedGoal;
        public Goal m_pBlueGoal;

        //container for the boundary walls
        public List<Wall2D> m_vecWalls = new List<Wall2D>();

        //defines the dimensions of the playing area
        public Region m_pPlayingArea;

        //the playing field is broken up into regions that the team
        //can make use of to implement strategies.
        public List<Region> m_Regions;

        //true if a goal keeper has possession
        public bool m_bGoalKeeperHasBall;

        //true if the game is in play. Set to false whenever the players
        //are getting ready for kickoff
        public bool m_bGameOn;

        //set true to pause the motion
        public bool m_bPaused;

        //local copy of client window dimensions
        public int m_cxClient, m_cyClient;

        //this instantiates the regions the players utilize to  position
        //themselves
        public void CreateRegions(float width, float height)
        {
            //index into the vector
            int idx = m_Regions.Count - 1;

            for (int col = 0; col < NumRegionsHorizontal; ++col)
            {
                for (int row = 0; row < NumRegionsVertical; ++row)
                {
                    m_Regions[idx--] = new Region(PlayingArea().Left() + col * width,
                                                 PlayingArea().Top() + row * height,
                                                 PlayingArea().Left() + (col + 1) * width,
                                                 PlayingArea().Top() + (row + 1) * height,
                                                 idx);
                }
            }
        }



        public SoccerPitch(int cx, int cy)
        {
            m_cxClient = cx;
            m_cyClient = cy;
            m_bPaused = false;
            m_bGoalKeeperHasBall = false;
            m_bGameOn = true;
            m_Regions = new List<Region>(NumRegionsHorizontal * NumRegionsVertical);
            for(int i=0;i< NumRegionsHorizontal * NumRegionsVertical;i++)
            {
                m_Regions.Add(new Region());
            }

            //define the playing area
            m_pPlayingArea = new Region(20, 20, cx - 20, cy - 20);

            //create the regions  
            CreateRegions(PlayingArea().Width() / (float)NumRegionsHorizontal,
                          PlayingArea().Height() / (float)NumRegionsVertical);

            //create the goals
            m_pRedGoal = new Goal(new Vector3(m_pPlayingArea.Left(),0, (cy - Prm.GoalWidth) / 2),
                                   new Vector3(m_pPlayingArea.Left(),0, cy - (cy - Prm.GoalWidth) / 2),
                                   new Vector3(1, 0,0));



            m_pBlueGoal = new Goal(new Vector3(m_pPlayingArea.Right(),0, (cy - Prm.GoalWidth) / 2),
                                    new Vector3(m_pPlayingArea.Right(),0, cy - (cy - Prm.GoalWidth) / 2),
                                    new Vector3(-1, 0,0));


            //create the soccer ball
            m_pBall = new SoccerBall(new Vector3((float)m_cxClient / 2.0f, (float)m_cyClient / 2.0f),
                                     Prm.BallSize,
                                     Prm.BallMass,
                                     m_vecWalls);


            //create the teams 
            m_pRedTeam = new SoccerTeam(m_pRedGoal, m_pBlueGoal, this, SoccerTeam.team_color.red);
            m_pBlueTeam = new SoccerTeam(m_pBlueGoal, m_pRedGoal, this, SoccerTeam.team_color.blue);

            //make sure each team knows who their opponents are
            m_pRedTeam.SetOpponents(m_pBlueTeam);
            m_pBlueTeam.SetOpponents(m_pRedTeam);

            //create the walls
            Vector3 TopLeft = new Vector3(m_pPlayingArea.Left(), 0,m_pPlayingArea.Top());
            Vector3 TopRight = new Vector3(m_pPlayingArea.Right(),0, m_pPlayingArea.Top());
            Vector3 BottomRight = new Vector3(m_pPlayingArea.Right(),0, m_pPlayingArea.Bottom());
            Vector3 BottomLeft = new Vector3(m_pPlayingArea.Left(),0, m_pPlayingArea.Bottom());

            m_vecWalls.Add(new Wall2D(BottomLeft, m_pRedGoal.RightPost()));
            m_vecWalls.Add(new Wall2D(m_pRedGoal.LeftPost(), TopLeft));
            m_vecWalls.Add(new Wall2D(TopLeft, TopRight));
            m_vecWalls.Add(new Wall2D(TopRight, m_pBlueGoal.LeftPost()));
            m_vecWalls.Add(new Wall2D(m_pBlueGoal.RightPost(), BottomRight));
            m_vecWalls.Add(new Wall2D(BottomRight, BottomLeft));

            //ParamLoader* p = ParamLoader::Instance();
        }


        public void Update()
        {
            if (m_bPaused) return;

            //static int tick = 0;

            //update the balls
            m_pBall.Update();

            //update the teams
            m_pRedTeam.Update();
            m_pBlueTeam.Update();

            //if a goal has been detected reset the pitch ready for kickoff
            if (m_pBlueGoal.Scored(m_pBall) || m_pRedGoal.Scored(m_pBall))
            {
                m_bGameOn = false;

                //reset the ball                                                      
                m_pBall.PlaceAtPosition(new Vector3((float)m_cxClient / 2.0f, 0, (float)m_cyClient / 2.0f));

                //get the teams ready for kickoff
                m_pRedTeam.GetFSM().ChangeState(PrepareForKickOff.instance);
                m_pBlueTeam.GetFSM().ChangeState(PrepareForKickOff.instance);
            }
        }

        public bool Render()
        {
            //draw the grass
            //gdi->DarkGreenPen();
            //gdi->DarkGreenBrush();
            //gdi->Rect(0, 0, m_cxClient, m_cyClient);

            Vector3 pos = Vector3.zero;
            Vector3 size = new Vector3(m_cxClient, 0, m_cyClient);
            Color cc = Color.green;
            DebugWide.DrawCube(pos, size, cc);

            //render regions
            if (Prm.ViewRegions)
            {
                for (int r = 0; r < m_Regions.Count; ++r)
                {
                    m_Regions[r].Render(true);
                }
            }

            //render the goals
            //gdi->HollowBrush();
            //gdi->RedPen();
            //gdi->Rect(m_pPlayingArea.Left(), (m_cyClient - Prm.GoalWidth) / 2, m_pPlayingArea.Left() + 40, m_cyClient - (m_cyClient - Prm.GoalWidth) / 2);
            pos = new Vector3(m_pPlayingArea.Left(), 0, (m_cyClient - Prm.GoalWidth) / 2);
            size = new Vector3(m_pPlayingArea.Left() + 40, 0, m_cyClient - (m_cyClient - Prm.GoalWidth) / 2);
            cc = Color.red;
            DebugWide.DrawCube(pos, size, cc);

            //gdi->BluePen();
            //gdi->Rect(m_pPlayingArea.Right(), (m_cyClient - Prm.GoalWidth) / 2, m_pPlayingArea.Right() - 40, m_cyClient - (m_cyClient - Prm.GoalWidth) / 2);

            pos = new Vector3(m_pPlayingArea.Right(), 0,(m_cyClient - Prm.GoalWidth) / 2);
            size = new Vector3(m_pPlayingArea.Right() - 40, 0,m_cyClient - (m_cyClient - Prm.GoalWidth) / 2);
            cc = Color.blue;
            DebugWide.DrawCube(pos, size, cc);


            //render the pitch markings
            //gdi->WhitePen();
            //gdi->Circle(m_pPlayingArea.Center(), m_pPlayingArea.Width() * 0.125);
            //gdi->Line(m_pPlayingArea.Center().x, m_pPlayingArea.Top(), m_pPlayingArea.Center().x, m_pPlayingArea.Bottom());
            ////gdi->WhiteBrush();
            //gdi->Circle(m_pPlayingArea.Center(), 2.0);

            cc = Color.white;
            DebugWide.DrawCircle(m_pPlayingArea.Center(), m_pPlayingArea.Width() * 0.125f, cc);
            DebugWide.DrawLine(new Vector3(m_pPlayingArea.Center().x, 0, m_pPlayingArea.Top()), new Vector3(m_pPlayingArea.Center().x,0, m_pPlayingArea.Bottom()), cc);
            DebugWide.DrawCircle(m_pPlayingArea.Center(), 2f, cc);

            //the ball
            //gdi->WhitePen();
            //gdi->WhiteBrush();
            m_pBall.Render();

            //Render the teams
            m_pRedTeam.Render();
            m_pBlueTeam.Render();

            //render the walls
            //gdi->WhitePen();
            for (int w = 0; w < m_vecWalls.Count; ++w)
            {
                m_vecWalls[w].Render();
            }

            //show the score
            //gdi->TextColor(Cgdi::red);
            //gdi->TextAtPos((m_cxClient / 2) - 50, m_cyClient - 18, "Red: " + ttos(m_pBlueGoal->NumGoalsScored()));

            //gdi->TextColor(Cgdi::blue);
            //gdi->TextAtPos((m_cxClient / 2) + 10, m_cyClient - 18, "Blue: " + ttos(m_pRedGoal->NumGoalsScored()));

            pos = new Vector3((m_cxClient / 2) - 50, 0, m_cyClient - 18);
            cc = Color.red;
            DebugWide.PrintText(pos, cc, m_pBlueGoal.NumGoalsScored() + "");

            pos = new Vector3((m_cxClient / 2) + 10, 0, m_cyClient - 18);
            cc = Color.blue;
            DebugWide.PrintText(pos, cc, m_pRedGoal.NumGoalsScored() + "");

            return true;
        }

        public void TogglePause() { m_bPaused = !m_bPaused; }
        public bool Paused() {return m_bPaused;}

        public int cxClient() {return m_cxClient;}
        public int cyClient() {return m_cyClient;}

        public bool GoalKeeperHasBall() {return m_bGoalKeeperHasBall;}
        public void SetGoalKeeperHasBall(bool b) { m_bGoalKeeperHasBall = b; }

        public Region  PlayingArea() {return m_pPlayingArea;}
        public List<Wall2D> Walls() { return m_vecWalls; }
        public SoccerBall Ball() {return m_pBall;}

        public Region  GetRegionFromIndex(int idx)                                
        {
            //assert((idx >= 0) && (idx<(int)m_Regions.size()) );

            return m_Regions[idx];
        }

        public bool GameOn() {return m_bGameOn;}
        public void SetGameOn() { m_bGameOn = true; }
        public void SetGameOff() { m_bGameOn = false; }

    }
}

