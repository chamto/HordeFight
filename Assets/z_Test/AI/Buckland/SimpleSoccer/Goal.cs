using UnityEngine;
using Buckland;

namespace Test_SimpleSoccer
{
    public class Goal
    {


        Vector3 m_vLeftPost;
        Vector3 m_vRightPost;

        //a vector representing the facing direction of the goal
        Vector3 m_vFacing;

        //the position of the center of the goal line
        Vector3 m_vCenter;

        //each time Scored() detects a goal this is incremented
        int m_iNumGoalsScored;


        public Goal(Vector3 left, Vector3 right, Vector3 facing)
        {
            m_vLeftPost = left;
            m_vRightPost = right;
            m_vCenter = (left + right) / 2.0f;
            m_iNumGoalsScored = 0;
            m_vFacing = facing;
        }

        //Given the current ball position and the previous ball position,
        //this method returns true if the ball has crossed the goal line 
        //and increments m_iNumGoalsScored
        public bool Scored( SoccerBall ball)
        {
            if (Geometry.LineIntersection2D(ball.Pos(), ball.OldPos(), m_vLeftPost, m_vRightPost))
            {
                ++m_iNumGoalsScored;

                return true;
            }

            return false;
        }

//-----------------------------------------------------accessor methods
        public Vector3 Center() {return m_vCenter;}
        public Vector3 Facing() {return m_vFacing;}
        public Vector3 LeftPost() {return m_vLeftPost;}
        public Vector3 RightPost() {return m_vRightPost;}

        public int NumGoalsScored() {return m_iNumGoalsScored;}
        public void ResetGoalsScored() { m_iNumGoalsScored = 0; }
    }


}//end namespace

