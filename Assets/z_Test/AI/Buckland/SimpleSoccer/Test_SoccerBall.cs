using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace Test_SoccerBall
{
    public class Test_SoccerBall : MonoBehaviour
    {
        public Transform _target = null;
        public float _friction = -0.015f;
        public float _playerKickingAccuracy = 0.9f;
        public float _force = 0.3f;
        public bool _apply = false;

        SoccerBall ball = null;

        // Use this for initialization
        void Start()
        {
            ball = new SoccerBall(Vector2.zero, 1, 1, null);
        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        private void OnDrawGizmos()
        {
            if (null == ball) return;

            if(true == _apply)
            {
                _apply = false;
                //Vector3 dir = _target.position - ball._pos;

                Vector3 noiseTarget = ball.AddNoiseToKick(ball._pos, _target.position);
                Vector3 dir = noiseTarget - ball._pos;
                ball.Kick(dir, _force);
            }

            DebugWide.DrawLine(ball._pos, _target.position, Color.green);

            Prm.Friction = _friction;
            Prm.PlayerKickingAccuracy = _playerKickingAccuracy;
            ball.Update();
            ball.Render();
        }
    }

    //======================================================
    public static class Prm
    {
        static public float Friction = -0.015f;
        static public float PlayerKickingAccuracy = 0.99f;
    }


    public class SoccerBall 
    {
        public Vector3 _pos;

        //protected Vector2 m_vScale;

        //the length of this object's bounding radius
        float m_dBoundingRadius;

        protected Vector3 m_vVelocity;

        //a normalized vector pointing in the direction the entity is heading. 
        protected Vector3 m_vHeading;

        //a vector perpendicular to the heading vector
        protected Vector3 m_vSide;

        protected float m_dMass;

        //the maximum speed this entity may travel at.
        //protected float m_dMaxSpeed;

        //the maximum force this entity can produce to power itself 
        //(think rockets and thrust)
        //protected float m_dMaxForce;

        //the maximum rate (radians per second)this vehicle can rotate         
        //protected float m_dMaxTurnRate;

        //keeps a record of the ball's position at the last update
        Vector2 m_vOldPos;

        //a local reference to the Walls that make up the pitch boundary
        List<Wall2D> m_PitchBoundary = null;

        public float BRadius() { return m_dBoundingRadius; }

        public SoccerBall(Vector2 pos, float BallSize, float mass, List<Wall2D> PitchBoundary)
        {
            _pos = pos;
            m_dBoundingRadius = BallSize;
            m_dMass = mass;
            m_PitchBoundary = PitchBoundary;

            m_vVelocity = Vector2.zero;
            m_vHeading = new Vector2(0, 1f);

            //m_dMaxSpeed = -1f; //max speed - unused
            //m_vScale = Vector2.one; //scale     - unused
            //m_dMaxTurnRate = 0f; //turn rate - unused
            //m_dMaxForce = 0f; //max force - unused
        }

        //tests to see if the ball has collided with a ball and reflects 
        //the ball's velocity accordingly
        //public void TestCollisionWithWalls(List<Wall2D> walls)
        //{  
        //    //test ball against each wall, find out which is closest
        //    int idxClosest = -1;

        //    Vector3 VelNormal = m_vVelocity.normalized;

        //    Vector3 IntersectionPoint, CollisionPoint;

        //    double DistToIntersection = float.MaxValue;

        //    //iterate through each wall and calculate if the ball intersects.
        //    //If it does then store the index into the closest intersecting wall
        //    for (int w = 0; w<walls.Count; ++w)
        //    {
        //        //assuming a collision if the ball continued on its current heading 
        //        //calculate the point on the ball that would hit the wall. This is 
        //        //simply the wall's normal(inversed) multiplied by the ball's radius
        //        //and added to the balls center (its position)
        //        Vector3 ThisCollisionPoint = _pos - (walls[w].Normal() * BRadius());

        //        //fixme => 3d
        //        //calculate exactly where the collision point will hit the plane    
        //        if (Util.WhereIsPoint(ThisCollisionPoint,
        //                         walls[w].From(),
        //                         walls[w].Normal()) == Util.span_type.plane_backside)
        //        {
        //            float DistToWall = Util.DistanceToRayPlaneIntersection(ThisCollisionPoint,
        //                                                         walls[w].Normal(),
        //                                                         walls[w].From(),
        //                                                         walls[w].Normal());

        //            IntersectionPoint = ThisCollisionPoint + (DistToWall* walls[w].Normal());
              
        //        }
        //        else
        //        {
        //            float DistToWall = Util.DistanceToRayPlaneIntersection(ThisCollisionPoint,
        //                                                         VelNormal,
        //                                                         walls[w].From(),
        //                                                         walls[w].Normal());

        //            IntersectionPoint = ThisCollisionPoint + (DistToWall* VelNormal);
        //        }

        //        //check to make sure the intersection point is actually on the line
        //        //segment
        //        bool OnLineSegment = false;

        //        //fixme => 3d
        //        if (Util.LineIntersection2D(walls[w].From(), 
        //                               walls[w].To(),
        //                               ThisCollisionPoint - walls[w].Normal()*20.0f,
        //                               ThisCollisionPoint + walls[w].Normal()*20.0f))
        //        {

        //            OnLineSegment = true;                                               
        //        }

                                                                            
        //        //Note, there is no test for collision with the end of a line segment
            
        //        //now check to see if the collision point is within range of the
        //        //velocity vector. [work in distance squared to avoid sqrt] and if it
        //        //is the closest hit found so far. 
        //        //If it is that means the ball will collide with the wall sometime
        //        //between this time step and the next one.
        //        float distSq =(ThisCollisionPoint - IntersectionPoint).sqrMagnitude;

        //        if ((distSq <= m_vVelocity.sqrMagnitude) && (distSq<DistToIntersection) && OnLineSegment)            
        //        {        
        //          DistToIntersection = distSq;
        //          idxClosest = w;
        //          CollisionPoint = IntersectionPoint;
        //        }     
        //    }//next wall

            
        //    //to prevent having to calculate the exact time of collision we
        //    //can just check if the velocity is opposite to the wall normal
        //    //before reflecting it. This prevents the case where there is overshoot
        //    //and the ball gets reflected back over the line before it has completely
        //    //reentered the playing area.
        //    if ((idxClosest >= 0 ) && Vector3.Dot(VelNormal, walls[idxClosest].Normal()) < 0)
        //    {
        //        m_vVelocity = Util.Reflect(m_vVelocity, walls[idxClosest].Normal());   
        //    }
        //}


        //----------------------------- Update -----------------------------------
        //
        //  updates the ball physics, tests for any collisions and adjusts
        //  the ball's velocity accordingly
        //------------------------------------------------------------------------
        public void Update()
        {

            //keep a record of the old position so the goal::scored method
            //can utilize it for goal testing
            m_vOldPos = _pos;

            //Test for collisions
            //if(null != m_PitchBoundary)
                //TestCollisionWithWalls(m_PitchBoundary);

            //Simulate Prm.Friction. Make sure the speed is positive 
            //first though
            if (m_vVelocity.sqrMagnitude > Prm.Friction * Prm.Friction)
            {
                m_vVelocity += m_vVelocity.normalized * Prm.Friction;

                _pos += m_vVelocity;



                //update heading
                m_vHeading = m_vVelocity.normalized;
            }
        }

        //implement base class Render
        public void Render()
        {

            DebugWide.DrawCircle(_pos, m_dBoundingRadius, Color.black);

        }

        //a soccer ball doesn't need to handle messages
        //public bool HandleMessage(Telegram msg) { return false; }

        //-------------------------- Kick ----------------------------------------
        //                                                                        
        //  applys a force to the ball in the direction of heading. Truncates
        //  the new velocity to make sure it doesn't exceed the max allowable.
        //------------------------------------------------------------------------
        public void Kick(Vector3 direction, float force)
        {
            //ensure direction is normalized
            direction.Normalize();

            //calculate the acceleration
            Vector3 acceleration = (direction * force) / m_dMass;

            //update the velocity
            m_vVelocity = acceleration;
        }

        //---------------------- TimeToCoverDistance -----------------------------
        //
        //  Given a force and a distance to cover given by two vectors, this
        //  method calculates how long it will take the ball to travel between
        //  the two points
        //------------------------------------------------------------------------
        public float TimeToCoverDistance(Vector2 A, Vector2 B, float force)
        {
            //this will be the velocity of the ball in the next time step *if*
            //the player was to make the pass. 
            float speed = force / m_dMass;

            //calculate the velocity at B using the equation
            //
            //  v^2 = u^2 + 2as
            //

            //first calculate s (the distance between the two positions)
            float DistanceToCover = (A - B).magnitude;

            float term = speed * speed + 2.0f * DistanceToCover * Prm.Friction;

            //if  (u^2 + 2as) is negative it means the ball cannot reach point B.
            if (term <= 0.0) return -1.0f;

            float v = (float)Math.Sqrt(term);

            //it IS possible for the ball to reach B and we know its speed when it
            //gets there, so now it's easy to calculate the time using the equation
            //
            //    t = v-u
            //        ---
            //         a
            //
            return (v-speed)/Prm.Friction;
        }

        //--------------------- FuturePosition -----------------------------------
        //
        //  given a time this method returns the ball position at that time in the
        //  future
        //------------------------------------------------------------------------
        public Vector3 FuturePosition(float time)
        {
            //using the equation s = ut + 1/2at^2, where s = distance, a = friction
            //u=start velocity

            //calculate the ut term, which is a vector
            Vector3 ut = m_vVelocity * time;

            //calculate the 1/2at^2 term, which is scalar
            float half_a_t_squared = 0.5f * Prm.Friction * time * time;

            //turn the scalar quantity into a vector by multiplying the value with
            //the normalized velocity vector (because that gives the direction)
            Vector3 ScalarToVector = half_a_t_squared * m_vVelocity.normalized;

            //the predicted position is the balls position plus these two terms
            return _pos + ut + ScalarToVector;
        }

        //this is used by players and goalkeepers to 'trap' a ball -- to stop
        //it dead. That player is then assumed to be in possession of the ball
        //and m_pOwner is adjusted accordingly
        public void Trap() { m_vVelocity = Vector3.zero; }

        public Vector3 OldPos() {return m_vOldPos;}

        //----------------------- PlaceAtLocation -------------------------------------
        //
        //  positions the ball at the desired location and sets the ball's velocity to
        //  zero
        //-----------------------------------------------------------------------------
        public void PlaceAtPosition(Vector3 NewPos)
        {
            _pos = NewPos;

            m_vOldPos = _pos;

            m_vVelocity = Vector3.zero;
        }

        //----------------------------- AddNoiseToKick --------------------------------
        //
        //  this can be used to vary the accuracy of a player's kick. Just call it 
        //  prior to kicking the ball using the ball's position and the ball target as
        //  parameters.
        //-----------------------------------------------------------------------------
        public Vector3 AddNoiseToKick(Vector3 BallPos, Vector3 BallTarget)
        {

            float displacement = (ConstV.Pi - ConstV.Pi * Prm.PlayerKickingAccuracy) * Misc.RandomClamped();

            Vector3 toTarget = BallTarget - BallPos;

            //toTarget = Util.Vec2DRotateAroundOrigin(toTarget, displacement); //2d
            toTarget = Quaternion.AngleAxis(displacement, ConstV.v3_up) * toTarget; //3d

            return toTarget + BallPos;
        }

    }//end class 


}//end namespace

