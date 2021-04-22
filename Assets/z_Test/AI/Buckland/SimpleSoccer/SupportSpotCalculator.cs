using System;
using System.Collections.Generic;
using UnityEngine;
using Buckland;

namespace Test_SimpleSoccer
{
    public class SupportSpotCalculator
    {

  //a data structure to hold the values and positions of each spot
        public class SupportSpot
        {

            public Vector3 m_vPos;

            public float m_dScore;

            public SupportSpot(Vector3 pos, float value) 
            {
                m_vPos = pos;
                m_dScore = value;
            }
        }

        SoccerTeam m_pTeam;

        List<SupportSpot> m_Spots = new List<SupportSpot>();

        //a pointer to the highest valued spot from the last update
        SupportSpot m_pBestSupportingSpot;

        //this will regulate how often the spots are calculated (default is
        //one update per second)
        Regulator m_pRegulator;


        public SupportSpotCalculator(int numX, int numY, SoccerTeam team) 
        {
            m_pBestSupportingSpot = null;
            m_pTeam = team;

            Region PlayingField = team.Pitch().PlayingArea();

            //calculate the positions of each sweet spot, create them and 
            //store them in m_Spots
            float HeightOfSSRegion = PlayingField.Height() * 0.8f;
            float WidthOfSSRegion = PlayingField.Width() * 0.9f;
            float SliceX = WidthOfSSRegion / numX;
            float SliceY = HeightOfSSRegion / numY;

            float left = PlayingField.Left() + (PlayingField.Width() - WidthOfSSRegion) / 2.0f + SliceX / 2.0f;
            float right = PlayingField.Right() - (PlayingField.Width() - WidthOfSSRegion) / 2.0f - SliceX / 2.0f;
            float top = PlayingField.Top() + (PlayingField.Height() - HeightOfSSRegion) / 2.0f + SliceY / 2.0f;

            for (int x = 0; x < (numX / 2) - 1; ++x)
            {
                for (int y = 0; y < numY; ++y)
                {
                    if (m_pTeam.Color() == SoccerTeam.team_color.blue)
                    {
                        m_Spots.Add(new SupportSpot(new Vector3(left + x * SliceX,0, top + y * SliceY), 0.0f));
                    }

                    else
                    {
                        m_Spots.Add(new SupportSpot(new Vector3(right - x * SliceX,0, top + y * SliceY), 0.0f));
                    }
                }
            }

            //create the regulator
            m_pRegulator = new Regulator(Prm.SupportSpotUpdateFreq);
        }


        //draws the spots to the screen as a hollow circles. The higher the 
        //score, the bigger the circle. The best supporting spot is drawn in
        //bright green.
        public void Render()
        {
            //gdi->HollowBrush();
                //gdi->GreyPen();

            Color cc = Color.gray;
            for (int spt = 0; spt<m_Spots.Count; ++spt)
            {
                //gdi->Circle(m_Spots[spt].m_vPos, m_Spots[spt].m_dScore);
                DebugWide.DrawCircle(m_Spots[spt].m_vPos, m_Spots[spt].m_dScore, cc);
            }

            if (null != m_pBestSupportingSpot)
            {
            //  gdi->GreenPen();
            //gdi->Circle(m_pBestSupportingSpot.m_vPos, m_pBestSupportingSpot.m_dScore);

                cc = Color.green;
                DebugWide.DrawCircle(m_pBestSupportingSpot.m_vPos, m_pBestSupportingSpot.m_dScore, cc);
            }
        }

        //this method iterates through each possible spot and calculates its
        //score.
        public Vector3 DetermineBestSupportingPosition()
        {
            //only update the spots every few frames                              
            if (!m_pRegulator.isReady() && null != m_pBestSupportingSpot)
            {
                return m_pBestSupportingSpot.m_vPos;
            }

            //reset the best supporting spot
            m_pBestSupportingSpot = null;

            float BestScoreSoFar = 0.0f;

            SupportSpot curSpot = null;
            for (int i = 0; i < m_Spots.Count; i++)
            {
                curSpot = m_Spots[i];
                //first remove any previous score. (the score is set to one so that
                //the viewer can see the positions of all the spots if he has the 
                //aids turned on)
                curSpot.m_dScore = 1.0f;

                //Test 1. is it possible to make a safe pass from the ball's position 
                //to this position?
                if (m_pTeam.isPassSafeFromAllOpponents(m_pTeam.ControllingPlayer().Pos(),
                                                       curSpot.m_vPos,
                                                       null,
                                                       Prm.MaxPassingForce))
                {
                    curSpot.m_dScore += Prm.Spot_CanPassScore;
                }

                Vector3 vtemp;
                //Test 2. Determine if a goal can be scored from this position.  
                if (m_pTeam.CanShoot(curSpot.m_vPos,
                                      Prm.MaxShootingForce, out vtemp))
                {
                    curSpot.m_dScore += Prm.Spot_CanScoreFromPositionScore;
                }


                //Test 3. calculate how far this spot is away from the controlling
                //player. The further away, the higher the score. Any distances further
                //away than OptimalDistance pixels do not receive a score.
                if (null != m_pTeam.SupportingPlayer())
                {
                    const float OptimalDistance = 200.0f;

                    float dist = (m_pTeam.ControllingPlayer().Pos() - curSpot.m_vPos).magnitude;

                    float temp = (float)Math.Abs(OptimalDistance - dist);

                    if (temp < OptimalDistance)
                    {

                        //normalize the distance and add it to the score
                        curSpot.m_dScore += Prm.Spot_DistFromControllingPlayerScore *
                                             (OptimalDistance - temp) / OptimalDistance;
                    }
                }

                //check to see if this spot has the highest score so far
                if (curSpot.m_dScore > BestScoreSoFar)
                {
                    BestScoreSoFar = curSpot.m_dScore;

                    m_pBestSupportingSpot = curSpot;
                }

            }

            return m_pBestSupportingSpot.m_vPos;
        }

        //returns the best supporting spot if there is one. If one hasn't been
        //calculated yet, this method calls DetermineBestSupportingPosition and
        //returns the result.
        public Vector3 GetBestSupportingSpot()
        {
            if (null != m_pBestSupportingSpot)
            {
                return m_pBestSupportingSpot.m_vPos;
            }

            else
            {
                return DetermineBestSupportingPosition();
            }
        }
    }
}

