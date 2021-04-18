using System;
using System.Collections.Generic;
using UnityEngine;
using Buckland;
using UtilGS9;

namespace Test_SimpleSoccer
{
    public class SoccerBall : MovingEntity
    {
        //keeps a record of the ball's position at the last update
        Vector3 m_vOldPos;

        //a local reference to the Walls that make up the pitch boundary
        List<Wall2D> m_PitchBoundary;


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
            toTarget = Transformations.Vec3RotateYAroundOrigin(toTarget, displacement);
            
            return toTarget + BallPos;
        }

        //tests to see if the ball has collided with a ball and reflects 
        //the ball's velocity accordingly
        public void TestCollisionWithWalls(List<Wall2D> walls)
        {
            //test ball against each wall, find out which is closest
            int idxClosest = -1;

            Vector3 VelNormal = m_vVelocity.normalized;

            Vector3 IntersectionPoint, CollisionPoint;

            float DistToIntersection = float.MaxValue;

            //iterate through each wall and calculate if the ball intersects.
            //If it does then store the index into the closest intersecting wall
            for (int w = 0; w < walls.Count; ++w)
            {
                //assuming a collision if the ball continued on its current heading 
                //calculate the point on the ball that would hit the wall. This is 
                //simply the wall's normal(inversed) multiplied by the ball's radius
                //and added to the balls center (its position)
                Vector3 ThisCollisionPoint = Pos() - (walls[w].Normal() * BRadius());

                //calculate exactly where the collision point will hit the plane    
                if (Geometry.WhereIsPoint(ThisCollisionPoint,
                                 walls[w].From(),
                                 walls[w].Normal()) == span_type.plane_backside)
                {
                  float DistToWall = Geometry.DistanceToRayPlaneIntersection(ThisCollisionPoint,
                                                                     walls[w].Normal(),
                                                                     walls[w].From(),
                                                                     walls[w].Normal());

                    IntersectionPoint = ThisCollisionPoint + (DistToWall* walls[w].Normal());
                  
                }
                else
                {
                  float DistToWall = Geometry.DistanceToRayPlaneIntersection(ThisCollisionPoint,
                                                                     VelNormal,
                                                                     walls[w].From(),
                                                                     walls[w].Normal());

                IntersectionPoint = ThisCollisionPoint + (DistToWall* VelNormal);
                }

                //check to make sure the intersection point is actually on the line
                //segment
                bool OnLineSegment = false;

                if (Geometry.LineIntersection2D(walls[w].From(), 
                                       walls[w].To(),
                                       ThisCollisionPoint - walls[w].Normal()*20.0f,
                                       ThisCollisionPoint + walls[w].Normal()*20.0f))
                {

                  OnLineSegment = true;                                               
                }


                //Note, there is no test for collision with the end of a line segment

                //now check to see if the collision point is within range of the
                //velocity vector. [work in distance squared to avoid sqrt] and if it
                //is the closest hit found so far. 
                //If it is that means the ball will collide with the wall sometime
                //between this time step and the next one.
                float distSq = (ThisCollisionPoint - IntersectionPoint).sqrMagnitude;

                if ((distSq <= m_vVelocity.sqrMagnitude) && (distSq<DistToIntersection) && OnLineSegment)            
                {        
                  DistToIntersection = distSq;
                  idxClosest = w;
                  CollisionPoint = IntersectionPoint;
                }     
              }//next wall


            //to prevent having to calculate the exact time of collision we
            //can just check if the velocity is opposite to the wall normal
            //before reflecting it. This prevents the case where there is overshoot
            //and the ball gets reflected back over the line before it has completely
            //reentered the playing area.
            if ((idxClosest >= 0) && Vector3.Dot(VelNormal, walls[idxClosest].Normal()) < 0)
            {
                m_vVelocity = VOp.Reflect(m_vVelocity, walls[idxClosest].Normal());
                //m_vVelocity.Reflect(walls[idxClosest].Normal());   
            }
        }

        public SoccerBall(Vector3 pos,
               float BallSize,
               float mass,
               List<Wall2D> PitchBoundary) :

            //set up the base class
            base(pos,
                  BallSize,
                  Vector3.zero,
                  -1.0f,                //max speed - unused
                  Vector3.forward,
                  mass,
                  new Vector3(1,1,1),  //scale     - unused
                  0,                   //turn rate - unused
                  0)                  //max force - unused
        {
                m_PitchBoundary = PitchBoundary;
        }

        //implement base class Update
        public override void Update()
        {
            //keep a record of the old position so the goal::scored method
            //can utilize it for goal testing
            m_vOldPos = m_vPosition;

            //Test for collisions
            TestCollisionWithWalls(m_PitchBoundary);

            //Simulate Prm.Friction. Make sure the speed is positive 
            //first though
            if (m_vVelocity.sqrMagnitude > Prm.Friction * Prm.Friction)
            {
                m_vVelocity += m_vVelocity.normalized * Prm.Friction;

                m_vPosition += m_vVelocity;



                //update heading
                m_vHeading = (m_vVelocity.normalized);
            }
        }

        //implement base class Render
        public override void Render()
        {
            //gdi->BlackBrush();
            //gdi->Circle(m_vPosition, m_dBoundingRadius);

            DebugWide.DrawCircle(m_vPosition, m_dBoundingRadius, Color.black);
        }

        //a soccer ball doesn't need to handle messages
        public override bool HandleMessage(Telegram msg) { return false; }

        //this method applies a directional force to the ball (kicks it!)
        public void Kick(Vector3 direction, float force)
        {
            //ensure direction is normalized
            direction.Normalize();

            //calculate the acceleration
            Vector3 acceleration = (direction * force) / m_dMass;

            //update the velocity
            m_vVelocity = acceleration;
        }

        //given a kicking force and a distance to traverse defined by start
        //and finish points, this method calculates how long it will take the
        //ball to cover the distance.
        public float TimeToCoverDistance(Vector3 A, Vector3 B, float force)
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
            return (v - speed) / Prm.Friction;
        }

        //this method calculates where the ball will in 'time' seconds
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
            Vector3 ScalarToVector = half_a_t_squared * (m_vVelocity.normalized);

          //the predicted position is the balls position plus these two terms
          return Pos() + ut + ScalarToVector;
        }

        //this is used by players and goalkeepers to 'trap' a ball -- to stop
        //it dead. That player is then assumed to be in possession of the ball
        //and m_pOwner is adjusted accordingly
        public void Trap() { m_vVelocity = Vector3.zero; }

        public Vector3 OldPos() {return m_vOldPos;}

        //this places the ball at the desired location and sets its velocity to zero
        public void PlaceAtPosition(Vector3 NewPos)
        {
            m_vPosition = NewPos;

            m_vOldPos = m_vPosition;

            m_vVelocity = Vector3.zero;
        }
    }
}

