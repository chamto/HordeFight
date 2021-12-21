using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace SteeringBehavior
{
    public class SteeringBehavior
    {
        public enum SummingMethod
        {
            weighted_average,
            prioritized,
            dithered
        }


        public enum eType
        {
            none = 0x00000,
            seek = 0x00002,
            flee = 0x00004,
            arrive = 0x00008,
            wander = 0x00010,
            cohesion = 0x00020,
            separation = 0x00040,
            allignment = 0x00080,
            obstacle_avoidance = 0x00100,
            wall_avoidance = 0x00200,
            follow_path = 0x00400,
            pursuit = 0x00800,
            evade = 0x01000,
            interpose = 0x02000,
            hide = 0x04000,
            flock = 0x08000,
            offset_pursuit = 0x10000,
        }


        //a pointer to the owner of this instance
        Vehicle m_pVehicle;

        //the steering force created by the combined effect of all
        //the selected behaviors
        Vector2 m_vSteeringForce;

        //these can be used to keep track of friends, pursuers, or prey
        Vehicle m_pTargetAgent1;
        Vehicle m_pTargetAgent2;

        //the current target
        Vector2 m_vTarget;

        //length of the 'detection box' utilized in obstacle avoidance
        float m_dDBoxLength;


        //a vertex buffer to contain the feelers rqd for wall avoidance  
        List<Vector2> m_Feelers;

        //the length of the 'feeler/s' used in wall detection
        float m_dWallDetectionFeelerLength;



        //the current position on the wander circle the agent is
        //attempting to steer towards
        Vector2 m_vWanderTarget;

        //explained above
        float m_dWanderJitter;
        float m_dWanderRadius;
        float m_dWanderDistance;


        //multipliers. These can be adjusted to effect strength of the  
        //appropriate behavior. Useful to get flocking the way you require
        //for example.
        float m_dWeightSeparation;
        float m_dWeightCohesion;
        float m_dWeightAlignment;
        float m_dWeightWander;
        float m_dWeightObstacleAvoidance;
        float m_dWeightWallAvoidance;
        float m_dWeightSeek;
        float m_dWeightFlee;
        float m_dWeightArrive;
        float m_dWeightPursuit;
        float m_dWeightOffsetPursuit;
        float m_dWeightInterpose;
        float m_dWeightHide;
        float m_dWeightEvade;
        float m_dWeightFollowPath;

        //how far the agent can 'see'
        float m_dViewDistance;

        //pointer to any current path
        Path m_pPath;

        //the distance (squared) a vehicle has to be from a path waypoint before
        //it starts seeking to the next waypoint
        float m_dWaypointSeekDistSq;


        //any offset used for formations or offset pursuit
        Vector2 m_vOffset;



        //binary flags to indicate whether or not a behavior should be active
        int m_iFlags;


        //Arrive makes use of these to determine how quickly a vehicle
        //should decelerate to its target
        enum Deceleration { slow = 3, normal = 2, fast = 1 };

        //default
        Deceleration m_Deceleration;

        //is cell space partitioning to be used or not?
        bool m_bCellSpaceOn;

        //what type of method is used to sum any active behavior
        SummingMethod m_SummingMethod;


        //this function tests if a specific bit of m_iFlags is set
        bool On(eType bt) { return (m_iFlags & (int)bt) == (int)bt; }

        bool AccumulateForce(ref Vector2 RunningTot, Vector2 ForceToAdd)
        {
            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = RunningTot.magnitude;

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = m_pVehicle.MaxForce() - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (MagnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = ForceToAdd.magnitude;

            //if the magnitude of the sum of ForceToAdd and the running total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector is
            //possible without going over the max.
            if (MagnitudeToAdd < MagnitudeRemaining)
            {
                RunningTot += ForceToAdd;
            }

            else
            {
                //add it to the steering force
                RunningTot += (ForceToAdd.normalized * MagnitudeRemaining);
            }

            return true;

        }


        //------------------------------- CreateFeelers --------------------------
        //  더듬이 
        //  Creates the antenna utilized by WallAvoidance
        //------------------------------------------------------------------------
        //creates the antenna utilized by the wall avoidance behavior
        void CreateFeelers()
        {
            //feeler pointing straight in front
            m_Feelers[0] = m_pVehicle.Pos() + m_dWallDetectionFeelerLength * m_pVehicle.Heading();

            //feeler to left
            Vector2 temp = m_pVehicle.Heading();
            temp = Transformations.Vec2DRotateAroundOrigin(temp, Const.HalfPi * 3.5f);
            m_Feelers[1] = m_pVehicle.Pos() + m_dWallDetectionFeelerLength / 2.0f * temp;

            //feeler to right
            temp = m_pVehicle.Heading();
            temp = Transformations.Vec2DRotateAroundOrigin(temp, Const.HalfPi * 0.5f);
            m_Feelers[2] = m_pVehicle.Pos() + m_dWallDetectionFeelerLength / 2.0f * temp;
        }


        /* .......................................................

                         BEGIN BEHAVIOR DECLARATIONS

           .......................................................*/

        //------------------------------- Seek -----------------------------------
        //
        //  Given a target, this behavior returns a steering force which will
        //  direct the agent towards the target
        //------------------------------------------------------------------------
        //this behavior moves the agent towards a target position
        Vector2 Seek(Vector2 TargetPos)
        {
            Vector2 DesiredVelocity = (TargetPos - m_pVehicle.Pos()).normalized
                            * m_pVehicle.MaxSpeed();

            return (DesiredVelocity - m_pVehicle.Velocity());
        }

        //----------------------------- Flee -------------------------------------
        //
        //  Does the opposite of Seek
        //------------------------------------------------------------------------
        //this behavior returns a vector that moves the agent away
        //from a target position
        Vector2 Flee(Vector2 TargetPos)
        {
            //only flee if the target is within 'panic distance'. Work in distance
            //squared space.
            //const float PanicDistanceSq = 100.0f * 100.0f;
            //if ((m_pVehicle.Pos() - TargetPos).sqrMagnitude > PanicDistanceSq)
            //{
            //    return Vector2.zero;
            //}
             

            Vector2 DesiredVelocity = (m_pVehicle.Pos() - TargetPos).normalized
                                      * m_pVehicle.MaxSpeed();

            return (DesiredVelocity - m_pVehicle.Velocity());
        }

        //--------------------------- Arrive -------------------------------------
        //
        //  This behavior is similar to seek but it attempts to arrive at the
        //  target with a zero velocity
        //------------------------------------------------------------------------
        //this behavior is similar to seek but it attempts to arrive 
        //at the target position with a zero velocity
        Vector2 Arrive(Vector2 TargetPos,
                        Deceleration deceleration)
        {
            Vector2 ToTarget = TargetPos - m_pVehicle.Pos();

            //calculate the distance to the target
            float dist = ToTarget.magnitude;

            if (dist > 0) //0으로 나누는 것을 막는 처리 
            {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration..
                const float DecelerationTweaker = 0.3f;

                //calculate the speed required to reach the target given the desired
                //deceleration
                float speed = dist / ((float)deceleration * DecelerationTweaker);

                //make sure the velocity does not exceed the max
                speed = Util.MinOf(speed, m_pVehicle.MaxSpeed());

                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                Vector2 DesiredVelocity = ToTarget * speed / dist;

                return (DesiredVelocity - m_pVehicle.Velocity());
            }

            return Vector2.zero;
        }

        //------------------------------ Pursuit ---------------------------------
        //
        //  this behavior creates a force that steers the agent towards the 
        //  evader
        //------------------------------------------------------------------------
        //this behavior predicts where an agent will be in time T and seeks
        //towards that point to intercept it.
        Vector2 Pursuit(Vehicle evader)
        {
            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector2 ToEvader = evader.Pos() - m_pVehicle.Pos();

            float RelativeHeading = Vector2.Dot(m_pVehicle.Heading(), evader.Heading());

            if ((Vector2.Dot(ToEvader, m_pVehicle.Heading()) > 0f) &&
                 (RelativeHeading < -0.95))  //acos(0.95)=18 degs
            {
                return Seek(evader.Pos());
            }

            //Not considered ahead so we predict where the evader will be.

            //the lookahead time is propotional to the distance between the evader
            //and the pursuer; and is inversely proportional to the sum of the
            //agent's velocities
            float LookAheadTime = ToEvader.magnitude /
                                  (m_pVehicle.MaxSpeed() + evader.Speed());

            //now seek to the predicted future position of the evader
            return Seek(evader.Pos() + evader.Velocity() * LookAheadTime);
        }

        //------------------------- Offset Pursuit -------------------------------
        //  추구,추적  
        //  Produces a steering force that keeps a vehicle at a specified offset
        //  from a leader vehicle
        //------------------------------------------------------------------------
        //this behavior maintains a position, in the direction of offset
        //from the target vehicle
        Vector2 OffsetPursuit(Vehicle leader, Vector2 offset)
        {
            //calculate the offset's position in world space
            Vector2 WorldOffsetPos = Transformations.PointToWorldSpace(offset,
                                                            leader.Heading(),
                                                            leader.Side(),
                                                            leader.Pos());

            Vector2 ToOffset = WorldOffsetPos - m_pVehicle.Pos();

            //the lookahead time is propotional to the distance between the leader
            //and the pursuer; and is inversely proportional to the sum of both
            //agent's velocities
            float LookAheadTime = ToOffset.magnitude /
                                  (m_pVehicle.MaxSpeed() + leader.Speed());

            //now Arrive at the predicted future position of the offset
            return Arrive(WorldOffsetPos + leader.Velocity() * LookAheadTime, Deceleration.fast);
        }

        //----------------------------- Evade ------------------------------------
        //
        //  similar to pursuit except the agent Flees from the estimated future
        //  position of the pursuer
        //------------------------------------------------------------------------
        //this behavior attempts to evade a pursuer
        Vector2 Evade(Vehicle pursuer)
        {
            /* Not necessary to include the check for facing direction this time */

            Vector2 ToPursuer = pursuer.Pos() - m_pVehicle.Pos();

            //uncomment the following two lines to have Evade only consider pursuers 
            //within a 'threat range'
            const float ThreatRange = 100.0f;
            if (ToPursuer.sqrMagnitude > ThreatRange * ThreatRange) return Vector2.zero;

            //the lookahead time is propotional to the distance between the pursuer
            //and the pursuer; and is inversely proportional to the sum of the
            //agents' velocities
            float LookAheadTime = ToPursuer.magnitude /
                                   (m_pVehicle.MaxSpeed() + pursuer.Speed());

            //now flee away from predicted future position of the pursuer
            return Flee(pursuer.Pos() + pursuer.Velocity() * LookAheadTime);
        }

        //--------------------------- Wander -------------------------------------
        //
        //  This behavior makes the agent wander about randomly
        //------------------------------------------------------------------------
        //this behavior makes the agent wander about randomly
        Vector2 Wander()
        {
            //this behavior is dependent on the update rate, so this line must
            //be included when using time independent framerate.
            float JitterThisTimeSlice = m_dWanderJitter * m_pVehicle.TimeElapsed();

            //first, add a small random vector to the target's position
            m_vWanderTarget += new Vector2(Misc.RandomClamped() * JitterThisTimeSlice,
                                          Misc.RandomClamped() * JitterThisTimeSlice);

            //reproject this new vector back on to a unit circle
            m_vWanderTarget.Normalize();

            //increase the length of the vector to the same as the radius
            //of the wander circle
            m_vWanderTarget *= m_dWanderRadius;

            //move the target into a position WanderDist in front of the agent
            Vector2 target = m_vWanderTarget + new Vector2(m_dWanderDistance, 0);

            //project the target into world space
            Vector2 Target = Transformations.PointToWorldSpace(target,
                                                   m_pVehicle.Heading(),
                                                   m_pVehicle.Side(), 
                                                   m_pVehicle.Pos());

            //and steer towards it
            return Target - m_pVehicle.Pos(); 
        }

        //---------------------- ObstacleAvoidance -------------------------------
        //
        //  Given a vector of CObstacles, this method returns a steering force
        //  that will prevent the agent colliding with the closest obstacle
        //------------------------------------------------------------------------
        //this returns a steering force which will attempt to keep the agent 
        //away from any obstacles it may encounter
        Vector2 ObstacleAvoidance(List<BaseGameEntity> obstacles)
        {
            //the detection box length is proportional to the agent's velocity
            m_dDBoxLength = SingleO.prm.MinDetectionBoxLength +
                            (m_pVehicle.Speed() / m_pVehicle.MaxSpeed()) *
                            SingleO.prm.MinDetectionBoxLength;

            //tag all obstacles within range of the box for processing
            m_pVehicle.World().TagObstaclesWithinViewRange(m_pVehicle, m_dDBoxLength);

            //this will keep track of the closest intersecting obstacle (CIB)
            BaseGameEntity ClosestIntersectingObstacle = null;

            //this will be used to track the distance to the CIB
            float DistToClosestIP = float.MaxValue;

            //this will record the transformed local coordinates of the CIB
            Vector2 LocalPosOfClosestObstacle = Vector2.zero;


            foreach(BaseGameEntity curOb in obstacles)
            {
                //if the obstacle has been tagged within range proceed
                if (curOb.IsTagged())
                {
                    //calculate this obstacle's position in local space
                    Vector2 LocalPos = Transformations.Point2DToLocalSpace(curOb.Pos(),
                                                           m_pVehicle.Heading(),
                                                           m_pVehicle.Side(),
                                                           m_pVehicle.Pos());

                    //if the local position has a negative x value then it must lay
                    //behind the agent. (in which case it can be ignored)
                    if (LocalPos.x >= 0)
                    {
                        //if the distance from the x axis to the object's position is less
                        //than its radius + half the width of the detection box then there
                        //is a potential intersection.
                        float ExpandedRadius = curOb.BRadius() + m_pVehicle.BRadius();

                        if (Math.Abs(LocalPos.y) < ExpandedRadius)
                        {
                            //now to do a line/circle intersection test. The center of the 
                            //circle is represented by (cX, cY). The intersection points are 
                            //given by the formula x = cX +/-sqrt(r^2-cY^2) for y=0. 
                            //We only need to look at the smallest positive value of x because
                            //that will be the closest point of intersection.
                            float cX = LocalPos.x;
                            float cY = LocalPos.y;

                            //we only need to calculate the sqrt part of the above equation once
                            float SqrtPart = (float)Math.Sqrt(ExpandedRadius * ExpandedRadius - cY * cY);

                            float ip = cX - SqrtPart;

                            if (ip <= 0.0)
                            {
                                ip = cX + SqrtPart;
                            }

                            //test to see if this is the closest so far. If it is keep a
                            //record of the obstacle and its local coordinates
                            if (ip < DistToClosestIP)
                            {
                                DistToClosestIP = ip;

                                ClosestIntersectingObstacle = curOb;

                                LocalPosOfClosestObstacle = LocalPos;
                            }
                        }
                    }
                }

            }

            //if we have found an intersecting obstacle, calculate a steering 
            //force away from it
            Vector2 SteeringForce = Vector2.zero;

            if (null != ClosestIntersectingObstacle)
            {
                //the closer the agent is to an object, the stronger the 
                //steering force should be
                float multiplier = 1.0f + (m_dDBoxLength - LocalPosOfClosestObstacle.x) /
                                    m_dDBoxLength;

                //calculate the lateral force
                SteeringForce.y = (ClosestIntersectingObstacle.BRadius() -
                                   LocalPosOfClosestObstacle.y) * multiplier;

                //apply a braking force proportional to the obstacles distance from
                //the vehicle. 
                const float BrakingWeight = 0.2f;

                SteeringForce.x = (ClosestIntersectingObstacle.BRadius() -
                                   LocalPosOfClosestObstacle.x) *
                                   BrakingWeight;
            }

            //finally, convert the steering vector from local to world space
            return Transformations.Vector2DToWorldSpace(SteeringForce,
                                      m_pVehicle.Heading(),
                                      m_pVehicle.Side());
        }

        //--------------------------- WallAvoidance --------------------------------
        //  기피
        //  This returns a steering force that will keep the agent away from any
        //  walls it may encounter
        //------------------------------------------------------------------------
        //this returns a steering force which will keep the agent away from any
        //walls it may encounter
        Vector2 WallAvoidance(List<Wall2D> walls)
        {
            //the feelers are contained in a std::vector, m_Feelers
            CreateFeelers();

            float DistToThisIP = 0.0f;
            float DistToClosestIP = float.MaxValue;

            //this will hold an index into the vector of walls
            int ClosestWall = -1;

            Vector2 SteeringForce = Vector2.zero,
            point = Vector2.zero,         //used for storing temporary info
            ClosestPoint = Vector2.zero;  //holds the closest intersection point

            //examine each feeler in turn
            for (int flr = 0; flr < m_Feelers.Count; ++flr)
            {
                //run through each wall checking for any intersection points
                for (int w = 0; w < walls.Count; ++w)
                {
                    if (Util.LineIntersection2D(m_pVehicle.Pos(),
                                           m_Feelers[flr],
                                           walls[w].From(),
                                           walls[w].To(),
                                           DistToThisIP,
                                           point))
                    {
                        //is this the closest found so far? If so keep a record
                        if (DistToThisIP < DistToClosestIP)
                        {
                            DistToClosestIP = DistToThisIP;

                            ClosestWall = w;

                            ClosestPoint = point;
                        }
                    }
                }//next wall


                //if an intersection point has been detected, calculate a force  
                //that will direct the agent away
                if (ClosestWall >= 0)
                {
                    //calculate by what distance the projected position of the agent
                    //will overshoot the wall
                    Vector2 OverShoot = m_Feelers[flr] - ClosestPoint;

                    //create a force in the direction of the wall normal, with a 
                    //magnitude of the overshoot
                    SteeringForce = walls[ClosestWall].Normal() * OverShoot.magnitude;
                }

            }//next feeler

            return SteeringForce;
        }

  
        //------------------------------- FollowPath -----------------------------
        //
        //  Given a series of Vector2Ds, this method produces a force that will
        //  move the agent along the waypoints in order. The agent uses the
        // 'Seek' behavior to move to the next waypoint - unless it is the last
        //  waypoint, in which case it 'Arrives'
        //------------------------------------------------------------------------
        //given a series of Vector2Ds, this method produces a force that will
        //move the agent along the waypoints in order
        Vector2 FollowPath()
        {
            //move to next target if close enough to current target (working in
            //distance squared space)
            if ((m_pPath.CurrentWaypoint() - m_pVehicle.Pos()).sqrMagnitude <
               m_dWaypointSeekDistSq)
            {
                m_pPath.SetNextWaypoint();
            }

            if (!m_pPath.Finished())
            {
                return Seek(m_pPath.CurrentWaypoint());
            }

            else
            {
                return Arrive(m_pPath.CurrentWaypoint(), Deceleration.normal);
            }
        }

        //--------------------------- Interpose ----------------------------------
        //  개입 
        //  Given two agents, this method returns a force that attempts to 
        //  position the vehicle between them
        //------------------------------------------------------------------------
        //this results in a steering force that attempts to steer the vehicle
        //to the center of the vector connecting two moving agents.
        Vector2 Interpose(Vehicle AgentA, Vehicle AgentB)
        {
            //first we need to figure out where the two agents are going to be at 
            //time T in the future. This is approximated by determining the time
            //taken to reach the mid way point at the current time at at max speed.
            Vector2 MidPoint = (AgentA.Pos() + AgentB.Pos()) / 2.0f;

            float TimeToReachMidPoint = (m_pVehicle.Pos() - MidPoint).magnitude /
                                         m_pVehicle.MaxSpeed();

            //now we have T, we assume that agent A and agent B will continue on a
            //straight trajectory and extrapolate to get their future positions
            Vector2 APos = AgentA.Pos() + AgentA.Velocity() * TimeToReachMidPoint;
            Vector2 BPos = AgentB.Pos() + AgentB.Velocity() * TimeToReachMidPoint;

            //calculate the mid point of these predicted positions
            MidPoint = (APos + BPos) / 2.0f;

            //then steer to Arrive at it
            return Arrive(MidPoint, Deceleration.fast);
        }

        //given another agent position to hide from and a list of BaseGameEntitys this
        //method attempts to put an obstacle between itself and its opponent
        Vector2 Hide(Vehicle hunter, List<BaseGameEntity> obstacles)
        {
            float DistToClosest = float.MaxValue;
            Vector2 BestHidingSpot = Vector2.zero;

            //std::vector<BaseGameEntity*>::const_iterator closest;

            foreach(BaseGameEntity curOb in obstacles)
            {
                //calculate the position of the hiding spot for this obstacle
                Vector2 HidingSpot = GetHidingPosition(curOb.Pos(),
                                                         curOb.BRadius(),
                                                          hunter.Pos());

                //work in distance-squared space to find the closest hiding
                //spot to the agent
                float distSqr = (HidingSpot - m_pVehicle.Pos()).sqrMagnitude;

                if (distSqr < DistToClosest)
                {
                    DistToClosest = distSqr;

                    BestHidingSpot = HidingSpot;

                    //closest = curOb;
                }

            }//end while

            //if no suitable obstacles found then Evade the hunter
            if (DistToClosest == float.MaxValue)
            {
                return Evade(hunter);
            }

            //else use Arrive on the hiding spot
            return Arrive(BestHidingSpot, Deceleration.fast);
        }


        // -- Group Behaviors -- //

        //-------------------------------- Cohesion ------------------------------
        //  응집력 
        //  returns a steering force that attempts to move the agent towards the
        //  center of mass of the agents in its immediate area
        //------------------------------------------------------------------------
        Vector2 Cohesion(List<Vehicle> neighbors)
        {
            //first find the center of mass of all the agents
            Vector2 CenterOfMass = Vector2.zero, SteeringForce = Vector2.zero;

            int NeighborCount = 0;

            //iterate through the neighbors and sum up all the position vectors
            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure *this* agent isn't included in the calculations and that
                //the agent being examined is close enough ***also make sure it doesn't
                //include the evade target ***
                if ((neighbors[a] != m_pVehicle) && neighbors[a].IsTagged() &&
                  (neighbors[a] != m_pTargetAgent1))
                {
                    CenterOfMass += neighbors[a].Pos();

                    ++NeighborCount;
                }
            }

            if (NeighborCount > 0)
            {
                //the center of mass is the average of the sum of positions
                CenterOfMass /= (float)NeighborCount;

                //now seek towards that position
                SteeringForce = Seek(CenterOfMass);
            }

            //the magnitude of cohesion is usually much larger than separation or
            //allignment so it usually helps to normalize it.
            return SteeringForce.normalized;
        }
  
        //---------------------------- Separation --------------------------------
        //
        // this calculates a force repelling from the other neighbors
        //------------------------------------------------------------------------
        Vector2 Separation(List<Vehicle> neighbors)
        {
            Vector2 SteeringForce = Vector2.zero;

            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough. ***also make sure it doesn't
                //include the evade target ***
                if ((neighbors[a] != m_pVehicle) && neighbors[a].IsTagged() &&
                  (neighbors[a] != m_pTargetAgent1))
                {
                    Vector2 ToAgent = m_pVehicle.Pos() - neighbors[a].Pos();

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    SteeringForce += ToAgent.normalized / ToAgent.magnitude;
                }
            }

            return SteeringForce;
        }

        //---------------------------- Alignment ---------------------------------
        //
        //  returns a force that attempts to align this agents heading with that
        //  of its neighbors
        //------------------------------------------------------------------------
        Vector2 Alignment(List<Vehicle> neighbors)
        {
            //used to record the average heading of the neighbors
            Vector2 AverageHeading = Vector2.zero;

            //used to count the number of vehicles in the neighborhood
            int NeighborCount = 0;

            //iterate through all the tagged vehicles and sum their heading vectors  
            for (int a = 0; a < neighbors.Count; ++a)
            {
                //make sure *this* agent isn't included in the calculations and that
                //the agent being examined  is close enough ***also make sure it doesn't
                //include any evade target ***
                if ((neighbors[a] != m_pVehicle) && neighbors[a].IsTagged() &&
                  (neighbors[a] != m_pTargetAgent1))
                {
                    AverageHeading += neighbors[a].Heading();

                    ++NeighborCount;
                }
            }

            //if the neighborhood contained one or more vehicles, average their
            //heading vectors.
            if (NeighborCount > 0)
            {
                AverageHeading /= (float)NeighborCount;

                AverageHeading -= m_pVehicle.Heading();
            }

            return AverageHeading;
        }

        //-------------------------------- Cohesion ------------------------------
        //
        //  returns a steering force that attempts to move the agent towards the
        //  center of mass of the agents in its immediate area
        //
        //  USES SPACIAL PARTITIONING
        //------------------------------------------------------------------------
        //the following three are the same as above but they use cell-space
        //partitioning to find the neighbors
        Vector2 CohesionPlus(List<Vehicle> neighbors)
        {
            //first find the center of mass of all the agents
            Vector2 CenterOfMass = Vector2.zero, SteeringForce = Vector2.zero;

            int NeighborCount = 0;

            //iterate through the neighbors and sum up all the position vectors
            foreach (BaseGameEntity pV in m_pVehicle.World().CellSpace().GetNeighbors())
            {
                //make sure *this* agent isn't included in the calculations and that
                //the agent being examined is close enough
                if (pV != m_pVehicle)
                {
                    CenterOfMass += pV.Pos();

                    ++NeighborCount;
                }
            }

            if (NeighborCount > 0)
            {
                //the center of mass is the average of the sum of positions
                CenterOfMass /= (float)NeighborCount;

                //now seek towards that position
                SteeringForce = Seek(CenterOfMass);
            }

            //the magnitude of cohesion is usually much larger than separation or
            //allignment so it usually helps to normalize it.
            return SteeringForce.normalized;
        }

        //---------------------------- Separation --------------------------------
        //
        // this calculates a force repelling from the other neighbors
        //
        //  USES SPACIAL PARTITIONING
        //------------------------------------------------------------------------
        Vector2 SeparationPlus(List<Vehicle> neighbors)
        {
            Vector2 SteeringForce = Vector2.zero;

            //iterate through the neighbors and sum up all the position vectors
            foreach(BaseGameEntity pV in m_pVehicle.World().CellSpace().GetNeighbors())
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough
                if (pV != m_pVehicle)
                {
                    Vector2 ToAgent = m_pVehicle.Pos() - pV.Pos();

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    SteeringForce += ToAgent.normalized / ToAgent.magnitude;
                }

            }

            return SteeringForce;
        }

        //---------------------------- Alignment ---------------------------------
        //
        //  returns a force that attempts to align this agents heading with that
        //  of its neighbors
        //
        //  USES SPACIAL PARTITIONING
        //------------------------------------------------------------------------
        Vector2 AlignmentPlus(List<Vehicle> neighbors)
        {
            //This will record the average heading of the neighbors
            Vector2 AverageHeading = Vector2.zero;

            //This count the number of vehicles in the neighborhood
            float NeighborCount = 0.0f;

            //iterate through the neighbors and sum up all the position vectors
            foreach (MovingEntity pV in m_pVehicle.World().CellSpace().GetNeighbors())
            {
                //make sure *this* agent isn't included in the calculations and that
                //the agent being examined  is close enough
                if (pV != m_pVehicle)
                {
                    AverageHeading += pV.Heading();

                    ++NeighborCount;
                }

            }

            //if the neighborhood contained one or more vehicles, average their
            //heading vectors.
            if (NeighborCount > 0.0)
            {
                AverageHeading /= NeighborCount;

                AverageHeading -= m_pVehicle.Heading();
            }

            return AverageHeading;
        }

        /* .......................................................

                       END BEHAVIOR DECLARATIONS

        .......................................................*/

        //---------------------- CalculateWeightedSum ----------------------------
        //
        //  this simply sums up all the active behaviors X their weights and 
        //  truncates the result to the max available steering force before 
        //  returning
        //------------------------------------------------------------------------
        //calculates and sums the steering forces from any active behaviors
        Vector2 CalculateWeightedSum()
        {
            if (On(eType.wall_avoidance))
            {
                m_vSteeringForce += WallAvoidance(m_pVehicle.World().Walls()) *
                                     m_dWeightWallAvoidance;
            }

            if (On(eType.obstacle_avoidance))
            {
                m_vSteeringForce += ObstacleAvoidance(m_pVehicle.World().Obstacles()) *
                        m_dWeightObstacleAvoidance;
            }

            if (On(eType.evade))
            {
                //assert(m_pTargetAgent1 && "Evade target not assigned");

                m_vSteeringForce += Evade(m_pTargetAgent1) * m_dWeightEvade;
            }


            //these next three can be combined for flocking behavior (wander is
            //also a good behavior to add into this mix)
            if (!isSpacePartitioningOn())
            {
                if (On(eType.separation))
                {
                    m_vSteeringForce += Separation(m_pVehicle.World().Agents()) * m_dWeightSeparation;
                }

                if (On(eType.allignment))
                {
                    m_vSteeringForce += Alignment(m_pVehicle.World().Agents()) * m_dWeightAlignment;
                }

                if (On(eType.cohesion))
                {
                    m_vSteeringForce += Cohesion(m_pVehicle.World().Agents()) * m_dWeightCohesion;
                }
            }
            else
            {
                if (On(eType.separation))
                {
                    m_vSteeringForce += SeparationPlus(m_pVehicle.World().Agents()) * m_dWeightSeparation;
                }

                if (On(eType.allignment))
                {
                    m_vSteeringForce += AlignmentPlus(m_pVehicle.World().Agents()) * m_dWeightAlignment;
                }

                if (On(eType.cohesion))
                {
                    m_vSteeringForce += CohesionPlus(m_pVehicle.World().Agents()) * m_dWeightCohesion;
                }
            }


            if (On(eType.wander))
            {
                m_vSteeringForce += Wander() * m_dWeightWander;
            }

            if (On(eType.seek))
            {
                m_vSteeringForce += Seek(m_pVehicle.World().Crosshair()) * m_dWeightSeek;
            }

            if (On(eType.flee))
            {
                m_vSteeringForce += Flee(m_pVehicle.World().Crosshair()) * m_dWeightFlee;
            }

            if (On(eType.arrive))
            {
                m_vSteeringForce += Arrive(m_pVehicle.World().Crosshair(), m_Deceleration) * m_dWeightArrive;
            }

            if (On(eType.pursuit))
            {
                //assert(m_pTargetAgent1 && "pursuit target not assigned");

                m_vSteeringForce += Pursuit(m_pTargetAgent1) * m_dWeightPursuit;
            }

            if (On(eType.offset_pursuit))
            {
                //assert(m_pTargetAgent1 && "pursuit target not assigned");
                //assert(!m_vOffset.isZero() && "No offset assigned");

                m_vSteeringForce += OffsetPursuit(m_pTargetAgent1, m_vOffset) * m_dWeightOffsetPursuit;
            }

            if (On(eType.interpose))
            {
                //assert(m_pTargetAgent1 && m_pTargetAgent2 && "Interpose agents not assigned");

                m_vSteeringForce += Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_dWeightInterpose;
            }

            if (On(eType.hide))
            {
                //assert(m_pTargetAgent1 && "Hide target not assigned");

                m_vSteeringForce += Hide(m_pTargetAgent1, m_pVehicle.World().Obstacles()) * m_dWeightHide;
            }

            if (On(eType.follow_path))
            {
                m_vSteeringForce += FollowPath() * m_dWeightFollowPath;
            }

            m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

            return m_vSteeringForce;
        }

        //---------------------- CalculatePrioritized ----------------------------
        //
        //  this method calls each active steering behavior in order of priority
        //  and acumulates their forces until the max steering force magnitude
        //  is reached, at which time the function returns the steering force 
        //  accumulated to that  point
        //------------------------------------------------------------------------
        Vector2 CalculatePrioritized()
        {
            Vector2 force;

            if (On(eType.wall_avoidance))
            {
                force = WallAvoidance(m_pVehicle.World().Walls()) *
                        m_dWeightWallAvoidance;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.obstacle_avoidance))
            {
                force = ObstacleAvoidance(m_pVehicle.World().Obstacles()) *
                        m_dWeightObstacleAvoidance;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.evade))
            {
                //assert(m_pTargetAgent1 && "Evade target not assigned");

                force = Evade(m_pTargetAgent1) * m_dWeightEvade;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(eType.flee))
            {
                force = Flee(m_pVehicle.World().Crosshair()) * m_dWeightFlee;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }



            //these next three can be combined for flocking behavior (wander is
            //also a good behavior to add into this mix)
            if (!isSpacePartitioningOn())
            {
                if (On(eType.separation))
                {
                    force = Separation(m_pVehicle.World().Agents()) * m_dWeightSeparation;

                    if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
                }

                if (On(eType.allignment))
                {
                    force = Alignment(m_pVehicle.World().Agents()) * m_dWeightAlignment;

                    if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
                }

                if (On(eType.cohesion))
                {
                    force = Cohesion(m_pVehicle.World().Agents()) * m_dWeightCohesion;

                    if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
                }
            }

            else
            {

                if (On(eType.separation))
                {
                    force = SeparationPlus(m_pVehicle.World().Agents()) * m_dWeightSeparation;

                    if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
                }

                if (On(eType.allignment))
                {
                    force = AlignmentPlus(m_pVehicle.World().Agents()) * m_dWeightAlignment;

                    if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
                }

                if (On(eType.cohesion))
                {
                    force = CohesionPlus(m_pVehicle.World().Agents()) * m_dWeightCohesion;

                    if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
                }
            }

            if (On(eType.seek))
            {
                force = Seek(m_pVehicle.World().Crosshair()) * m_dWeightSeek;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(eType.arrive))
            {
                force = Arrive(m_pVehicle.World().Crosshair(), m_Deceleration) * m_dWeightArrive;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.wander))
            {
                force = Wander() * m_dWeightWander;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;

                //DebugWide.LogBlue(m_vSteeringForce.magnitude + "  wander: " + force.magnitude + "  " + m_dWeightWander);
            }

            if (On(eType.pursuit))
            {
                //assert(m_pTargetAgent1 && "pursuit target not assigned");

                force = Pursuit(m_pTargetAgent1) * m_dWeightPursuit;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.offset_pursuit))
            {
                //assert(m_pTargetAgent1 && "pursuit target not assigned");
                //assert(!m_vOffset.isZero() && "No offset assigned");

                force = OffsetPursuit(m_pTargetAgent1, m_vOffset);

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.interpose))
            {
                //assert(m_pTargetAgent1 && m_pTargetAgent2 && "Interpose agents not assigned");

                force = Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_dWeightInterpose;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(eType.hide))
            {
                //assert(m_pTargetAgent1 && "Hide target not assigned");

                force = Hide(m_pTargetAgent1, m_pVehicle.World().Obstacles()) * m_dWeightHide;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(eType.follow_path))
            {
                force = FollowPath() * m_dWeightFollowPath;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }


            return m_vSteeringForce;
        }

        //---------------------- CalculateDithered ----------------------------
        //  떨림 
        //  this method sums up the active behaviors by assigning a probabilty
        //  of being calculated to each behavior. It then tests the first priority
        //  to see if it should be calcukated this simulation-step. If so, it
        //  calculates the steering force resulting from this behavior. If it is
        //  more than zero it returns the force. If zero, or if the behavior is
        //  skipped it continues onto the next priority, and so on.
        //
        //  NOTE: Not all of the behaviors have been implemented in this method,
        //        just a few, so you get the general idea
        //------------------------------------------------------------------------
        Vector2 CalculateDithered()
        {
            //reset the steering force
            m_vSteeringForce = Vector2.zero;

            if (On(eType.wall_avoidance) && Misc.RandFloat() < SingleO.prm.prWallAvoidance)
            {
                m_vSteeringForce = WallAvoidance(m_pVehicle.World().Walls()) *
                    m_dWeightWallAvoidance / SingleO.prm.prWallAvoidance;

                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }

            if (On(eType.obstacle_avoidance) && Misc.RandFloat() < SingleO.prm.prObstacleAvoidance)
            {
                m_vSteeringForce += ObstacleAvoidance(m_pVehicle.World().Obstacles()) *
                    m_dWeightObstacleAvoidance / SingleO.prm.prObstacleAvoidance;

                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }

            if (!isSpacePartitioningOn())
            {
                if (On(eType.separation) && Misc.RandFloat() < SingleO.prm.prSeparation)
                {
                    m_vSteeringForce += Separation(m_pVehicle.World().Agents()) *
                        m_dWeightSeparation / SingleO.prm.prSeparation;

                    if (!Misc.IsZero(m_vSteeringForce))
                    {
                        m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                        return m_vSteeringForce;
                    }
                }
            }

            else
            {
                if (On(eType.separation) && Misc.RandFloat() < SingleO.prm.prSeparation)
                {
                    m_vSteeringForce += SeparationPlus(m_pVehicle.World().Agents()) *
                        m_dWeightSeparation / SingleO.prm.prSeparation;

                    if (!Misc.IsZero(m_vSteeringForce))
                    {
                        m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                        return m_vSteeringForce;
                    }
                }
            }


            if (On(eType.flee) && Misc.RandFloat() < SingleO.prm.prFlee)
            {
                m_vSteeringForce += Flee(m_pVehicle.World().Crosshair()) * m_dWeightFlee / SingleO.prm.prFlee;

                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }

            if (On(eType.evade) && Misc.RandFloat() < SingleO.prm.prEvade)
            {
                //assert(m_pTargetAgent1 && "Evade target not assigned");

                m_vSteeringForce += Evade(m_pTargetAgent1) * m_dWeightEvade / SingleO.prm.prEvade;

                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }


            if (!isSpacePartitioningOn())
            {
                if (On(eType.allignment) && Misc.RandFloat() < SingleO.prm.prAlignment)
                {
                    m_vSteeringForce += Alignment(m_pVehicle.World().Agents()) *
                        m_dWeightAlignment / SingleO.prm.prAlignment;

                    if (!Misc.IsZero(m_vSteeringForce))
                    {
                        m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                        return m_vSteeringForce;
                    }
                }

                if (On(eType.cohesion) && Misc.RandFloat() < SingleO.prm.prCohesion)
                {
                    m_vSteeringForce += Cohesion(m_pVehicle.World().Agents()) *
                        m_dWeightCohesion / SingleO.prm.prCohesion;

                    if (!Misc.IsZero(m_vSteeringForce))
                    {
                        m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                        return m_vSteeringForce;
                    }
                }
            }
            else
            {
                if (On(eType.allignment) && Misc.RandFloat() < SingleO.prm.prAlignment)
                {
                    m_vSteeringForce += AlignmentPlus(m_pVehicle.World().Agents()) *
                        m_dWeightAlignment / SingleO.prm.prAlignment;

                    if (!Misc.IsZero(m_vSteeringForce))
                    {
                        m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                        return m_vSteeringForce;
                    }
                }

                if (On(eType.cohesion) && Misc.RandFloat() < SingleO.prm.prCohesion)
                {
                    m_vSteeringForce += CohesionPlus(m_pVehicle.World().Agents()) *
                        m_dWeightCohesion / SingleO.prm.prCohesion;

                    if (!Misc.IsZero(m_vSteeringForce))
                    {
                        m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                        return m_vSteeringForce;
                    }
                }
            }

            if (On(eType.wander) && Misc.RandFloat() < SingleO.prm.prWander)
            {
                m_vSteeringForce += Wander() * m_dWeightWander / SingleO.prm.prWander;

                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }

            if (On(eType.seek) && Misc.RandFloat() < SingleO.prm.prSeek)
            {
                m_vSteeringForce += Seek(m_pVehicle.World().Crosshair()) * m_dWeightSeek / SingleO.prm.prSeek;

                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }

            if (On(eType.arrive) && Misc.RandFloat() < SingleO.prm.prArrive)
            {
                m_vSteeringForce += Arrive(m_pVehicle.World().Crosshair(), m_Deceleration) *
                    m_dWeightArrive / SingleO.prm.prArrive;


                if (!Misc.IsZero(m_vSteeringForce))
                {
                    m_vSteeringForce = Util.Truncate(m_vSteeringForce, m_pVehicle.MaxForce());

                    return m_vSteeringForce;
                }
            }

            return m_vSteeringForce;
        }

        //------------------------- GetHidingPosition ----------------------------
        //
        //  Given the position of a hunter, and the position and radius of
        //  an obstacle, this method calculates a position DistanceFromBoundary 
        //  away from its bounding radius and directly opposite the hunter
        //------------------------------------------------------------------------
        //helper method for Hide. Returns a position located on the other
        //side of an obstacle to the pursuer
        Vector2 GetHidingPosition(Vector2 posOb,
                              float radiusOb,
                              Vector2 posHunter)
        {
            //calculate how far away the agent is to be from the chosen obstacle's
            //bounding radius
            const float DistanceFromBoundary = 30.0f;
            float DistAway = radiusOb + DistanceFromBoundary;

            //calculate the heading toward the object from the hunter
            Vector2 ToOb = (posOb - posHunter).normalized;

            //scale it to size and add to the obstacles position to get
            //the hiding spot.
            return (ToOb * DistAway) + posOb;
        }



        public SteeringBehavior(Vehicle agent)
        {
            m_pVehicle = agent;
            m_iFlags = 0;
            m_dDBoxLength = SingleO.prm.MinDetectionBoxLength;
            m_dWeightCohesion = SingleO.prm.CohesionWeight_2;
            m_dWeightAlignment = SingleO.prm.AlignmentWeight_2;
            m_dWeightSeparation = SingleO.prm.SeparationWeight_2;
            m_dWeightObstacleAvoidance = SingleO.prm.ObstacleAvoidanceWeight_2;
            m_dWeightWander = SingleO.prm.WanderWeight_2;
            m_dWeightWallAvoidance = SingleO.prm.WallAvoidanceWeight_2;
            m_dViewDistance = SingleO.prm.ViewDistance;
            m_dWallDetectionFeelerLength = SingleO.prm.WallDetectionFeelerLength;
            m_Deceleration = Deceleration.normal;
            m_pTargetAgent1 = null;
            m_pTargetAgent2 = null;
            m_dWanderDistance = SingleO.prm.WanderDist;
            m_dWanderJitter = SingleO.prm.WanderJitterPerSec;
            m_dWanderRadius = SingleO.prm.WanderRad;
            m_dWaypointSeekDistSq = SingleO.prm.WaypointSeekDist * SingleO.prm.WaypointSeekDist;
            m_dWeightSeek = SingleO.prm.SeekWeight_2;
            m_dWeightFlee = SingleO.prm.FleeWeight_2;
            m_dWeightArrive = SingleO.prm.ArriveWeight_2;
            m_dWeightPursuit = SingleO.prm.PursuitWeight_2;
            m_dWeightOffsetPursuit = SingleO.prm.OffsetPursuitWeight_2;
            m_dWeightInterpose = SingleO.prm.InterposeWeight_2;
            m_dWeightHide = SingleO.prm.HideWeight_2;
            m_dWeightEvade = SingleO.prm.EvadeWeight_2;
            m_dWeightFollowPath = SingleO.prm.FollowPathWeight_2;
            m_bCellSpaceOn = false;
            m_SummingMethod = SummingMethod.prioritized;
            int feelCount = 3;
            m_Feelers = new List<Vector2>(feelCount);
            for (int i = 0 ; i < feelCount ; i++)
            {
                m_Feelers.Add(Vector2.zero);
            }
            //stuff for the wander behavior
            float theta =  Misc.RandFloat() * Const.TwoPi;

            //create a vector to a target position on the wander circle
            m_vWanderTarget = new Vector2(m_dWanderRadius * (float)Math.Cos(theta),
                                        m_dWanderRadius * (float)Math.Sin(theta));

            //create a Path
            m_pPath = new Path();
            m_pPath.LoopOn();
        }

        //calculates and sums the steering forces from any active behaviors
        public Vector2 Calculate()
        {
            //reset the steering force
            m_vSteeringForce = Vector2.zero;

            //use space partitioning to calculate the neighbours of this vehicle
            //if switched on. If not, use the standard tagging system
            if (!isSpacePartitioningOn())
            {
                //tag neighbors if any of the following 3 group behaviors are switched on
                if (On(eType.separation) || On(eType.allignment) || On(eType.cohesion))
                {
                    m_pVehicle.World().TagVehiclesWithinViewRange(m_pVehicle, m_dViewDistance);
                }
            }
            else
            {
                //calculate neighbours in cell-space if any of the following 3 group
                //behaviors are switched on
                if (On(eType.separation) || On(eType.allignment) || On(eType.cohesion))
                {
                    m_pVehicle.World().CellSpace().CalculateNeighbors(m_pVehicle.Pos(), m_dViewDistance);
                }
            }

            //DebugWide.LogBlue(m_SummingMethod + "  " + m_pVehicle.Speed());
            switch (m_SummingMethod)
            {
                case SummingMethod.weighted_average:

                    m_vSteeringForce = CalculateWeightedSum(); break;

                case SummingMethod.prioritized:

                    m_vSteeringForce = CalculatePrioritized(); break;

                case SummingMethod.dithered:

                    m_vSteeringForce = CalculateDithered(); break;

                default: m_vSteeringForce = Vector2.zero; break;

            }//end switch

            return m_vSteeringForce;
        }

        //calculates the component of the steering force that is parallel
        //with the vehicle heading
        public float ForwardComponent()
        {
            return Vector2.Dot(m_pVehicle.Heading(), m_vSteeringForce);
        }

        //calculates the component of the steering force that is perpendicuar
        //with the vehicle heading
        public float SideComponent()
        {
            return Vector2.Dot(m_pVehicle.Side(), m_vSteeringForce);
        }

        public void SetTarget(Vector2 t){m_vTarget = t;}

        public void SetTargetAgent1(Vehicle Agent) { m_pTargetAgent1 = Agent; }
        public void SetTargetAgent2(Vehicle Agent) { m_pTargetAgent2 = Agent; }

        public void SetOffset(Vector2 offset) { m_vOffset = offset; }
        public Vector2 GetOffset() {return m_vOffset;}

        public void SetPath(LinkedList<Vector2> new_path) { m_pPath.Set(new_path); }
        public void CreateRandomPath(int num_waypoints, int mx, int my, int cx, int cy)
        {
            m_pPath.CreateRandomPath(num_waypoints, mx, my, cx, cy);
        }

        public Vector2 Force() {return m_vSteeringForce;}

        public void ToggleSpacePartitioningOnOff() { m_bCellSpaceOn = !m_bCellSpaceOn; }
        public bool isSpacePartitioningOn() {return m_bCellSpaceOn;}

        public void SetSummingMethod(SummingMethod sm) { m_SummingMethod = sm; }


        public void FleeOn() { m_iFlags |= (int)eType.flee; }
        public void SeekOn() { m_iFlags |= (int)eType.seek; }
        public void ArriveOn() { m_iFlags |= (int)eType.arrive; }
        public void WanderOn() { m_iFlags |= (int)eType.wander; }
        public void PursuitOn(Vehicle v) { m_iFlags |= (int)eType.pursuit; m_pTargetAgent1 = v; }
        public void EvadeOn(Vehicle v) { m_iFlags |= (int)eType.evade; m_pTargetAgent1 = v; }
        public void CohesionOn() { m_iFlags |= (int)eType.cohesion; }
        public void SeparationOn() { m_iFlags |= (int)eType.separation; }
        public void AlignmentOn() { m_iFlags |= (int)eType.allignment; }
        public void ObstacleAvoidanceOn() { m_iFlags |= (int)eType.obstacle_avoidance; }
        public void WallAvoidanceOn() { m_iFlags |= (int)eType.wall_avoidance; }
        public void FollowPathOn() { m_iFlags |= (int)eType.follow_path; }
        public void InterposeOn(Vehicle v1, Vehicle v2) { m_iFlags |= (int)eType.interpose; m_pTargetAgent1 = v1; m_pTargetAgent2 = v2; }
        public void HideOn(Vehicle v) { m_iFlags |= (int)eType.hide; m_pTargetAgent1 = v; }
        public void OffsetPursuitOn(Vehicle v1,  Vector2 offset) { m_iFlags |= (int)eType.offset_pursuit; m_vOffset = offset; m_pTargetAgent1 = v1; }
        public void FlockingOn() { CohesionOn(); AlignmentOn(); SeparationOn(); WanderOn(); }

        public void FleeOff() { if (On(eType.flee)) m_iFlags ^= (int)eType.flee; }
        public void SeekOff() { if (On(eType.seek)) m_iFlags ^= (int)eType.seek; }
        public void ArriveOff() { if (On(eType.arrive)) m_iFlags ^= (int)eType.arrive; }
        public void WanderOff() { if (On(eType.wander)) m_iFlags ^= (int)eType.wander; }
        public void PursuitOff() { if (On(eType.pursuit)) m_iFlags ^= (int)eType.pursuit; }
        public void EvadeOff() { if (On(eType.evade)) m_iFlags ^= (int)eType.evade; }
        public void CohesionOff() { if (On(eType.cohesion)) m_iFlags ^= (int)eType.cohesion; }
        public void SeparationOff() { if (On(eType.separation)) m_iFlags ^= (int)eType.separation; }
        public void AlignmentOff() { if (On(eType.allignment)) m_iFlags ^= (int)eType.allignment; }
        public void ObstacleAvoidanceOff() { if (On(eType.obstacle_avoidance)) m_iFlags ^= (int)eType.obstacle_avoidance; }
        public void WallAvoidanceOff() { if (On(eType.wall_avoidance)) m_iFlags ^= (int)eType.wall_avoidance; }
        public void FollowPathOff() { if (On(eType.follow_path)) m_iFlags ^= (int)eType.follow_path; }
        public void InterposeOff() { if (On(eType.interpose)) m_iFlags ^= (int)eType.interpose; }
        public void HideOff() { if (On(eType.hide)) m_iFlags ^= (int)eType.hide; }
        public void OffsetPursuitOff() { if (On(eType.offset_pursuit)) m_iFlags ^= (int)eType.offset_pursuit; }
        public void FlockingOff() { CohesionOff(); AlignmentOff(); SeparationOff(); WanderOff(); }

        public bool isFleeOn() { return On(eType.flee); }
        public bool isSeekOn() { return On(eType.seek); }
        public bool isArriveOn() { return On(eType.arrive); }
        public bool isWanderOn() { return On(eType.wander); }
        public bool isPursuitOn() { return On(eType.pursuit); }
        public bool isEvadeOn() { return On(eType.evade); }
        public bool isCohesionOn() { return On(eType.cohesion); }
        public bool isSeparationOn() { return On(eType.separation); }
        public bool isAlignmentOn() { return On(eType.allignment); }
        public bool isObstacleAvoidanceOn() { return On(eType.obstacle_avoidance); }
        public bool isWallAvoidanceOn() { return On(eType.wall_avoidance); }
        public bool isFollowPathOn() { return On(eType.follow_path); }
        public bool isInterposeOn() { return On(eType.interpose); }
        public bool isHideOn() { return On(eType.hide); }
        public bool isOffsetPursuitOn() { return On(eType.offset_pursuit); }

        public float DBoxLength() {return m_dDBoxLength;}
        public List<Vector2> GetFeelers() {return m_Feelers;}
  
        public float WanderJitter() {return m_dWanderJitter;}
        public float WanderDistance() {return m_dWanderDistance;}
        public float WanderRadius() {return m_dWanderRadius;}

        public float SeparationWeight() {return m_dWeightSeparation;}
        public float AlignmentWeight() {return m_dWeightAlignment;}
        public float CohesionWeight() {return m_dWeightCohesion;}

        //renders visual aids and info for seeing how each behavior is
        //calculated
        public void RenderAids()
        {
            //gdi->TransparentText();
            //gdi->TextColor(Cgdi::grey);

            int NextSlot = 0; int SlotSize = 20;

            if (Input.GetKey(KeyCode.Q)) {m_pVehicle.SetMaxForce(m_pVehicle.MaxForce() + 1000.0f*m_pVehicle.TimeElapsed());} 
            if (Input.GetKey(KeyCode.W)) {if (m_pVehicle.MaxForce() > 0.2f) m_pVehicle.SetMaxForce(m_pVehicle.MaxForce() - 1000.0f*m_pVehicle.TimeElapsed());}
            if (Input.GetKey(KeyCode.E)) {m_pVehicle.SetMaxSpeed(m_pVehicle.MaxSpeed() + 50.0f*m_pVehicle.TimeElapsed());}
            if (Input.GetKey(KeyCode.R)) {if (m_pVehicle.MaxSpeed() > 0.2f) m_pVehicle.SetMaxSpeed(m_pVehicle.MaxSpeed() - 50.0f*m_pVehicle.TimeElapsed());}

            if (m_pVehicle.MaxForce() < 0) m_pVehicle.SetMaxForce(0.0f);
            if (m_pVehicle.MaxSpeed() < 0) m_pVehicle.SetMaxSpeed(0.0f);


            if (m_pVehicle.ID() == 0)
            { 
                DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "MaxForce(Q/W):");
                DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_pVehicle.MaxForce() / SingleO.prm.SteeringForceTweaker));
                NextSlot+=SlotSize;
            }
            if (m_pVehicle.ID() == 0)
            { 
                DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "MaxSpeed(E/R):");
                DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_pVehicle.MaxSpeed()));
                NextSlot+=SlotSize;
            }

            //render the steering force
            if (m_pVehicle.World().RenderSteeringForce())
            {  
                //gdi->RedPen();
                Vector2 F = (m_vSteeringForce / SingleO.prm.SteeringForceTweaker) * SingleO.prm.VehicleScale ;
                DebugWide.DrawLine(m_pVehicle.Pos(), m_pVehicle.Pos() + F, Color.red);
            }

              //render wander stuff if relevant
            if (On(eType.wander) && m_pVehicle.World().RenderWanderCircle())
            {    
                
                if (Input.GetKey(KeyCode.F)){m_dWanderJitter+=1.0f*m_pVehicle.TimeElapsed(); m_dWanderJitter = Mathf.Clamp(m_dWanderJitter, 0.0f, 100.0f);}
                if (Input.GetKey(KeyCode.V)){m_dWanderJitter-=1.0f*m_pVehicle.TimeElapsed(); m_dWanderJitter = Mathf.Clamp(m_dWanderJitter, 0.0f, 100.0f );}
                if (Input.GetKey(KeyCode.G)){m_dWanderDistance+=2.0f*m_pVehicle.TimeElapsed(); m_dWanderDistance = Mathf.Clamp(m_dWanderDistance, 0.0f, 50.0f);}
                if (Input.GetKey(KeyCode.B)){m_dWanderDistance-=2.0f*m_pVehicle.TimeElapsed(); m_dWanderDistance = Mathf.Clamp(m_dWanderDistance, 0.0f, 50.0f);}
                if (Input.GetKey(KeyCode.H)){m_dWanderRadius+=2.0f*m_pVehicle.TimeElapsed(); m_dWanderRadius = Mathf.Clamp(m_dWanderRadius, 0.0f, 100.0f);}
                if (Input.GetKey(KeyCode.N)){m_dWanderRadius-=2.0f*m_pVehicle.TimeElapsed(); m_dWanderRadius = Mathf.Clamp(m_dWanderRadius, 0.0f, 100.0f);}

             
                if (m_pVehicle.ID() == 0)
                { 
                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "Jitter(F/V): ");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_dWanderJitter));
                    NextSlot+=SlotSize;
                }
                if (m_pVehicle.ID() == 0) 
                {
                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "Distance(G/B): ");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_dWanderDistance));
                    NextSlot+=SlotSize;
                }
                if (m_pVehicle.ID() == 0) 
                {
                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "Radius(H/N): ");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_dWanderRadius));
                    NextSlot+=SlotSize;
                }

                
                //calculate the center of the wander circle
                Vector2 m_vTCC = Transformations.PointToWorldSpace(new Vector2(m_dWanderDistance*m_pVehicle.BRadius(), 0),
                                                     m_pVehicle.Heading(),
                                                     m_pVehicle.Side(),
                                                     m_pVehicle.Pos());
                //draw the wander circle
                DebugWide.DrawCircle(m_vTCC, m_dWanderRadius * m_pVehicle.BRadius(), Color.green);

                //draw the wander target
                Vector2 p_temp = Transformations.PointToWorldSpace((m_vWanderTarget + new Vector2(m_dWanderDistance, 0)) * m_pVehicle.BRadius(),
                                              m_pVehicle.Heading(),
                                              m_pVehicle.Side(),
                                                      m_pVehicle.Pos());
                DebugWide.DrawCircle(p_temp, 3, Color.red);

              }

              //render the detection box if relevant
              if (m_pVehicle.World().RenderDetectionBox())
              {
                //gdi->GreyPen();

                //a vertex buffer rqd for drawing the detection box
                List<Vector2> box = new List<Vector2>(4);

                float length = SingleO.prm.MinDetectionBoxLength + 
                              (m_pVehicle.Speed()/m_pVehicle.MaxSpeed()) *
                              SingleO.prm.MinDetectionBoxLength;

                //verts for the detection box buffer
                box[0] = new Vector2(0,m_pVehicle.BRadius());
                box[1] = new Vector2(length, m_pVehicle.BRadius());
                box[2] = new Vector2(length, -m_pVehicle.BRadius());
                box[3] = new Vector2(0, -m_pVehicle.BRadius());
             
              
                if (!m_pVehicle.isSmoothingOn())
                {
                    box = Transformations.WorldTransform(box,m_pVehicle.Pos(),m_pVehicle.Heading(),m_pVehicle.Side());
                  //gdi->ClosedShape(box);
                }
                else
                {
                    box = Transformations.WorldTransform(box,m_pVehicle.Pos(),m_pVehicle.SmoothedHeading(), VOp.Perp(m_pVehicle.SmoothedHeading()));
                  //gdi->ClosedShape(box);
                } 
                DebugWide.DrawLine(box[0], box[1], Color.gray);
                DebugWide.DrawLine(box[1], box[2], Color.gray);
                DebugWide.DrawLine(box[2], box[3], Color.gray);
                DebugWide.DrawLine(box[3], box[0], Color.gray);


                //////////////////////////////////////////////////////////////////////////
                //the detection box length is proportional to the agent's velocity
                m_dDBoxLength = SingleO.prm.MinDetectionBoxLength + 
                              (m_pVehicle.Speed()/m_pVehicle.MaxSpeed()) *
                              SingleO.prm.MinDetectionBoxLength;

                //tag all obstacles within range of the box for processing
                m_pVehicle.World().TagObstaclesWithinViewRange(m_pVehicle, m_dDBoxLength);

                //this will keep track of the closest intersecting obstacle (CIB)
                BaseGameEntity ClosestIntersectingObstacle = null;
             
                //this will be used to track the distance to the CIB
                float DistToClosestIP = float.MaxValue;

                //this will record the transformed local coordinates of the CIB
                Vector2 LocalPosOfClosestObstacle;

                foreach(BaseGameEntity curOb in m_pVehicle.World().Obstacles())
                {
                    //if the obstacle has been tagged within range proceed
                    if (curOb.IsTagged())
                    {
                        //calculate this obstacle's position in local space
                        Vector2 LocalPos = Transformations.Point2DToLocalSpace(curOb.Pos(),
                                                             m_pVehicle.Heading(),
                                                             m_pVehicle.Side(),
                                                             m_pVehicle.Pos());

                        //if the local position has a negative x value then it must lay
                        //behind the agent. (in which case it can be ignored)
                        if (LocalPos.x >= 0)
                        {
                        //if the distance from the x axis to the object's position is less
                        //than its radius + half the width of the detection box then there
                        //is a potential intersection.
                        if (Mathf.Abs(LocalPos.y) < (curOb.BRadius() + m_pVehicle.BRadius()))
                        {
                          //gdi->ThickRedPen();
                          //gdi->ClosedShape(box); 
                                DebugWide.DrawLine(box[0], box[1], Color.black);
                                DebugWide.DrawLine(box[1], box[2], Color.black);
                                DebugWide.DrawLine(box[2], box[3], Color.black);
                                DebugWide.DrawLine(box[3], box[0], Color.black);
                        }
                      }
                    }

                }


            /////////////////////////////////////////////////////
              }

              //render the wall avoidnace feelers
              if (On(eType.wall_avoidance) && m_pVehicle.World().RenderFeelers())
              {
                //gdi->OrangePen();

                for (int flr=0; flr<m_Feelers.Count; ++flr)
                {
                    //gdi->Line(m_pVehicle->Pos(), m_Feelers[flr]);
                    DebugWide.DrawLine(m_pVehicle.Pos(), m_Feelers[flr], Color.yellow);
                }            
              }  

              //render path info
              if (On(eType.follow_path) && m_pVehicle.World().RenderPath())
              {
                    m_pPath.Render();
              }

              
              if (On(eType.separation))
              {
                if (m_pVehicle.ID() == 0)
                { 
                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "Separation(S/X):");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_dWeightSeparation / SingleO.prm.SteeringForceTweaker));
                    NextSlot+=SlotSize;
                }
                if (Input.GetKey(KeyCode.S)) { m_dWeightSeparation += 200 * m_pVehicle.TimeElapsed(); m_dWeightSeparation = Mathf.Clamp(m_dWeightSeparation, 0.0f, 50.0f * SingleO.prm.SteeringForceTweaker); }
                if (Input.GetKey(KeyCode.X)) { m_dWeightSeparation -= 200 * m_pVehicle.TimeElapsed(); m_dWeightSeparation = Mathf.Clamp(m_dWeightSeparation, 0.0f, 50.0f * SingleO.prm.SteeringForceTweaker); }

              }

              if (On(eType.allignment))
              {
                if (m_pVehicle.ID() == 0) 
                {
                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "Alignment(A/Z):");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_dWeightAlignment / SingleO.prm.SteeringForceTweaker));
                    NextSlot+=SlotSize;
                }
                if (Input.GetKey(KeyCode.A)) { m_dWeightAlignment += 200 * m_pVehicle.TimeElapsed(); m_dWeightAlignment = Mathf.Clamp(m_dWeightAlignment, 0.0f, 50.0f * SingleO.prm.SteeringForceTweaker); }
                if (Input.GetKey(KeyCode.Z)) { m_dWeightAlignment -= 200 * m_pVehicle.TimeElapsed(); m_dWeightAlignment = Mathf.Clamp(m_dWeightAlignment, 0.0f, 50.0f * SingleO.prm.SteeringForceTweaker); }

              }

              if (On(eType.cohesion))
              {
                if (m_pVehicle.ID() == 0) 
                {
                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "Cohesion(D/C):");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (m_dWeightCohesion / SingleO.prm.SteeringForceTweaker));
                    NextSlot+=SlotSize;

                }
                if (Input.GetKey(KeyCode.D)) { m_dWeightCohesion += 200 * m_pVehicle.TimeElapsed(); m_dWeightCohesion = Mathf.Clamp(m_dWeightCohesion, 0.0f, 50.0f * SingleO.prm.SteeringForceTweaker); }
                if (Input.GetKey(KeyCode.C)) { m_dWeightCohesion -= 200 * m_pVehicle.TimeElapsed(); m_dWeightCohesion = Mathf.Clamp(m_dWeightCohesion, 0.0f, 50.0f * SingleO.prm.SteeringForceTweaker); }


              }

              if (On(eType.follow_path))
              { 
                
                if (m_pVehicle.ID() == 0)
                {
                    float sd = (float)Math.Sqrt(m_dWaypointSeekDistSq);

                    DebugWide.PrintText(new Vector3(5, NextSlot, 0), Color.gray, "SeekDistance(D/C):");
                    DebugWide.PrintText(new Vector3(160, NextSlot, 0), Color.gray, "" + (sd));
                    NextSlot+=SlotSize;

                }
                if (Input.GetKey(KeyCode.D)) { m_dWaypointSeekDistSq += 1.0f; }
                if (Input.GetKey(KeyCode.C)) { m_dWaypointSeekDistSq -= 1.0f; m_dWaypointSeekDistSq = Mathf.Clamp(m_dWaypointSeekDistSq, 0.0f, 400.0f); }

              }  

        }

    }
}

