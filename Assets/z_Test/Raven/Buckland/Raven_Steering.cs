using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

/*
namespace Raven
{


    public class Wall2D
    {

        protected Vector3 m_vA,
                        m_vB,
                        m_vN;


        protected void CalculateNormal()
        {
            Vector3 temp = VOp.Normalize(m_vB - m_vA);

            //m_vN.x = -temp.y;
            //m_vN.y = temp.x;
            m_vN = Vector3.Cross(temp, ConstV.v3_up);
        }


        public Wall2D() { }

        public Wall2D(Vector3 A, Vector3 B) 
        {
            m_vA = A;
            m_vB = B;
            CalculateNormal();
        }

        public Wall2D(Vector3 A, Vector3 B, Vector3 N) 
        {
            m_vA = A;
            m_vB = B;
            m_vN = N;
        }

        //Wall2D(std::ifstream& in) { Read(in); }

        public virtual void Render(bool RenderNormals = false)
        {
            DebugWide.DrawLine(m_vA, m_vB, Color.white);

            //render the normals if rqd
            if (RenderNormals)
            {
                int MidX = (int)((m_vA.x + m_vB.x) / 2);
                int MidY = (int)((m_vA.y + m_vB.y) / 2);

                DebugWide.DrawLine(
                gdi->Line(MidX, MidY, (int)(MidX+(m_vN.x* 5)), (int) (MidY+(m_vN.y* 5)));
            }
        }

Vector2D From()const  {return m_vA;}
  void SetFrom(Vector2D v) { m_vA = v; CalculateNormal(); }

Vector2D To()const    {return m_vB;}
  void SetTo(Vector2D v) { m_vB = v; CalculateNormal(); }

Vector2D Normal()const{return m_vN;}
  void SetNormal(Vector2D n) { m_vN = n; }

Vector2D Center()const{return (m_vA+m_vB)/2.0;}

  std::ostream& Wall2D::Write(std::ostream& os)const
  {
    os << std::endl;
    os << From() << ",";
    os << To() << ",";
    os << Normal();
    return os;
  }

 void Read(std::ifstream& in)
{
    double x, y;

    in >> x >> y;
    SetFrom(Vector2D(x, y));

    in >> x >> y;
    SetTo(Vector2D(x, y));

     in >> x >> y;
    SetNormal(Vector2D(x, y));
}
  
}

    //======================================================

    public class Raven_Steering
    {

        public enum eSumming_method { weighted_average, prioritized, dithered };


        public enum eBehavior
        {
            none = 0x00000,
            seek = 0x00002,
            arrive = 0x00008,
            wander = 0x00010,
            separation = 0x00040,
            wall_avoidance = 0x00200,
        }

        //Arrive makes use of these to determine how quickly a Raven_Bot
        //should decelerate to its target
        public enum eDeceleration { slow = 3, normal = 2, fast = 1 };

        //a pointer to the owner of this instance
        Raven_Bot m_pRaven_Bot;

        //pointer to the world data
        Raven_Game m_pWorld;

        //the steering force created by the combined effect of all
        //the selected behaviors
        Vector3 m_vSteeringForce;

        //these can be used to keep track of friends, pursuers, or prey
        Raven_Bot m_pTargetAgent1;
        Raven_Bot m_pTargetAgent2;

        //the current target
        Vector3 m_vTarget;


        //a vertex buffer to contain the feelers rqd for wall avoidance  
        List<Vector3> m_Feelers;

        //the length of the 'feeler/s' used in wall detection
        float m_dWallDetectionFeelerLength;


        //the current position on the wander circle the agent is
        //attempting to steer towards
        Vector3 m_vWanderTarget;

        //explained above
        float m_dWanderJitter;
        float m_dWanderRadius;
        float m_dWanderDistance;


        //multipliers. These can be adjusted to effect strength of the  
        //appropriate behavior.
        float m_dWeightSeparation;
        float m_dWeightWander;
        float m_dWeightWallAvoidance;
        float m_dWeightSeek;
        float m_dWeightArrive;


        //how far the agent can 'see'
        float m_dViewDistance;

        //binary flags to indicate whether or not a behavior should be active
        int m_iFlags;


        //default
        eDeceleration m_Deceleration;

        //is cell space partitioning to be used or not?
        bool m_bCellSpaceOn;

        //what type of method is used to sum any active behavior
        eSumming_method m_SummingMethod;


        //this function tests if a specific bit of m_iFlags is set
        bool On(eBehavior bt) { return (m_iFlags & (int)bt) == (int)bt; }

        bool AccumulateForce(ref Vector3 RunningTot, Vector3 ForceToAdd)
        {
            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = RunningTot.magnitude;

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = m_pRaven_Bot.MaxForce() - MagnitudeSoFar;

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
                MagnitudeToAdd = MagnitudeRemaining;

                //add it to the steering force
                RunningTot += (VOp.Normalize(ForceToAdd) * MagnitudeToAdd);
            }

            return true;
        }

        //creates the antenna utilized by the wall avoidance behavior
        void CreateFeelers()
        {
            //feeler pointing straight in front
            m_Feelers[0] = m_pRaven_Bot.Pos() + m_dWallDetectionFeelerLength *
                           m_pRaven_Bot.Heading() * m_pRaven_Bot.Speed();

            //feeler to left
            Vector3 temp = m_pRaven_Bot.Heading();
            Transformation.Vec2DRotateAroundOrigin(ref temp, ConstV.HalfPi * 3.5f);
            m_Feelers[1] = m_pRaven_Bot.Pos() + m_dWallDetectionFeelerLength / 2.0f * temp;

            //feeler to right
            temp = m_pRaven_Bot.Heading();
            Transformation.Vec2DRotateAroundOrigin(ref temp, ConstV.HalfPi * 0.5f);
            m_Feelers[2] = m_pRaven_Bot.Pos() + m_dWallDetectionFeelerLength / 2.0f * temp;
        }



        // .......................................................

        //               BEGIN BEHAVIOR DECLARATIONS

        // .......................................................


        //this behavior moves the agent towards a target position
        Vector3 Seek(Vector3 target)
        {
         
          Vector3 DesiredVelocity = VOp.Normalize(target - m_pRaven_Bot.Pos())
                                    * m_pRaven_Bot.MaxSpeed();

          return (DesiredVelocity - m_pRaven_Bot.Velocity());
        }

        //this behavior is similar to seek but it attempts to arrive 
        //at the target with a zero velocity
        Vector3 Arrive(Vector3 target, eDeceleration deceleration)
        {
            Vector3 ToTarget = target - m_pRaven_Bot.Pos();

            //calculate the distance to the target
            float dist = ToTarget.magnitude;

            if (dist > 0)
            {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration..
                float DecelerationTweaker = 0.3f;

                //calculate the speed required to reach the target given the desired
                //deceleration
                float speed = dist / ((float)deceleration * DecelerationTweaker);

                //make sure the velocity does not exceed the max
                speed = Math.Min(speed, m_pRaven_Bot.MaxSpeed());

                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                Vector3 DesiredVelocity = ToTarget * speed / dist;

                return (DesiredVelocity - m_pRaven_Bot.Velocity());
            }

            return ConstV.v3_zero;
        }

        //this behavior makes the agent wander about randomly
        Vector3 Wander()
        {
            //first, add a small random vector to the target's position
            m_vWanderTarget += new Vector3(Misc.RandomClamped() * m_dWanderJitter, 0,
                                        Misc.RandomClamped() * m_dWanderJitter);

            //reproject this new vector back on to a unit circle
            m_vWanderTarget.Normalize();

            //increase the length of the vector to the same as the radius
            //of the wander circle
            m_vWanderTarget *= m_dWanderRadius;

            //move the target into a position WanderDist in front of the agent
            Vector3 target = m_vWanderTarget + new Vector3(m_dWanderDistance, 0, 0);

            //project the target into world space
            Vector3 Target = Transformation.PointToWorldSpace(target,
                                                 m_pRaven_Bot.Heading(),
                                                 m_pRaven_Bot.Side(),
                                                 m_pRaven_Bot.Pos());

            //and steer towards it
            return Target - m_pRaven_Bot.Pos();
        }

        //this returns a steering force which will keep the agent away from any
        //walls it may encounter
        Vector3 WallAvoidance(const vector<Wall2D*> &walls)
        {
            //the feelers are contained in a std::vector, m_Feelers
            CreateFeelers();

            double DistToThisIP = 0.0;
            double DistToClosestIP = MaxDouble;

            //this will hold an index into the vector of walls
            int ClosestWall = -1;

            Vector2D SteeringForce,
                      point,         //used for storing temporary info
                      ClosestPoint;  //holds the closest intersection point

            //examine each feeler in turn
            for (unsigned int flr = 0; flr < m_Feelers.size(); ++flr)
            {
                //run through each wall checking for any intersection points
                for (unsigned int w = 0; w < walls.size(); ++w)
                {
                    if (LineIntersection2D(m_pRaven_Bot->Pos(),
                                           m_Feelers[flr],
                                           walls[w]->From(),
                                           walls[w]->To(),
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
                    Vector2D OverShoot = m_Feelers[flr] - ClosestPoint;

                    //create a force in the direction of the wall normal, with a 
                    //magnitude of the overshoot
                    SteeringForce = walls[ClosestWall]->Normal() * OverShoot.Length();
                }

            }//next feeler

            return SteeringForce;
        }


        Vector3 Separation(const std::list<Raven_Bot*>& neighbors)
        {
            //iterate through all the neighbors and calculate the vector from the
            Vector2D SteeringForce;

            std::list<Raven_Bot*>::const_iterator it = neighbors.begin();
            for (it; it != neighbors.end(); ++it)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough. ***also make sure it doesn't
                //include the evade target ***
                if ((*it != m_pRaven_Bot) && (*it)->IsTagged() &&
                  (*it != m_pTargetAgent1))
                {
                    Vector2D ToAgent = m_pRaven_Bot->Pos() - (*it)->Pos();

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    SteeringForce += Vec2DNormalize(ToAgent) / ToAgent.Length();
                }
            }

            return SteeringForce;
        }


    // .......................................................

    //                 END BEHAVIOR DECLARATIONS

    // .......................................................

    //calculates and sums the steering forces from any active behaviors
    Vector3 CalculatePrioritized()
        {
            Vector2D force;

            if (On(wall_avoidance))
            {
                force = WallAvoidance(m_pWorld->GetMap()->GetWalls()) *
                        m_dWeightWallAvoidance;

                if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
            }


            //these next three can be combined for flocking behavior (wander is
            //also a good behavior to add into this mix)

            if (On(separation))
            {
                force = Separation(m_pWorld->GetAllBots()) * m_dWeightSeparation;

                if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(seek))
            {
                force = Seek(m_vTarget) * m_dWeightSeek;

                if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
            }


            if (On(arrive))
            {
                force = Arrive(m_vTarget, m_Deceleration) * m_dWeightArrive;

                if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(wander))
            {
                force = Wander() * m_dWeightWander;

                if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
            }


            return m_vSteeringForce;
        }



        public Raven_Steering(Raven_Game world, Raven_Bot agent)
        {

            m_pWorld = world;
            m_pRaven_Bot = agent;
            m_iFlags = 0;
            m_dWeightSeparation = Params.SeparationWeight;
            m_dWeightWander = Params.WanderWeight;
            m_dWeightWallAvoidance = Params.WallAvoidanceWeight;
            m_dViewDistance = Params.ViewDistance;
            m_dWallDetectionFeelerLength = Params.WallDetectionFeelerLength;
            m_Feelers = new List<Vector3>(3);
            m_Feelers.Add(ConstV.v3_zero);
            m_Feelers.Add(ConstV.v3_zero);
            m_Feelers.Add(ConstV.v3_zero);
            m_Deceleration = Deceleration.normal;
            m_pTargetAgent1 = null;
            m_pTargetAgent2 = null;
            m_dWanderDistance = Const.WanderDist;
            m_dWanderJitter = Const.WanderJitterPerSec;
            m_dWanderRadius = Const.WanderRad;
             m_dWeightSeek = Params.SeekWeight;
            m_dWeightArrive = Params.ArriveWeight;
            m_bCellSpaceOn = false;
            m_SummingMethod = summing_method.prioritized;

            //stuff for the wander behavior
            float theta = Misc.RandFloat() * ConstV.TwoPi;

            //create a vector to a target position on the wander circle
            m_vWanderTarget = new Vector3(m_dWanderRadius * (float)Math.Cos(theta), 0,
                                        m_dWanderRadius * (float)Math.Sin(theta));

        }


        //calculates and sums the steering forces from any active behaviors
        public Vector3 Calculate()
        {
            //reset the steering force
            m_vSteeringForce.Zero();

            //tag neighbors if any of the following 3 group behaviors are switched on
            if (On(separation))
            {
                m_pWorld->TagRaven_BotsWithinViewRange(m_pRaven_Bot, m_dViewDistance);
            }

            m_vSteeringForce = CalculatePrioritized();

            return m_vSteeringForce;
        }

        //calculates the component of the steering force that is parallel
        //with the Raven_Bot heading
        public float ForwardComponent()
        {
            return m_pRaven_Bot->Heading().Dot(m_vSteeringForce);
        }

        //calculates the component of the steering force that is perpendicuar
        //with the Raven_Bot heading
        public float SideComponent()
        {
            return m_pRaven_Bot->Side().Dot(m_vSteeringForce);
        }


        public void SetTarget(Vector3 t) { m_vTarget = t; }
        public Vector3 Target() { return m_vTarget; }

        public void SetTargetAgent1(Raven_Bot Agent) { m_pTargetAgent1 = Agent; }
        public void SetTargetAgent2(Raven_Bot Agent) { m_pTargetAgent2 = Agent; }


        public Vector3 Force() { return m_vSteeringForce; }

        public void SetSummingMethod(summing_method sm) { m_SummingMethod = sm; }


        public void SeekOn() { m_iFlags |= (int)eBehavior.seek; }
        public void ArriveOn() { m_iFlags |= (int)eBehavior.arrive; }
        public void WanderOn() { m_iFlags |= (int)eBehavior.wander; }
        public void SeparationOn() { m_iFlags |= (int)eBehavior.separation; }
        public void WallAvoidanceOn() { m_iFlags |= (int)eBehavior.wall_avoidance; }

        public void SeekOff() { if (On(eBehavior.seek)) m_iFlags ^= (int)eBehavior.seek; }
        public void ArriveOff() { if (On(eBehavior.arrive)) m_iFlags ^= (int)eBehavior.arrive; }
        public void WanderOff() { if (On(eBehavior.wander)) m_iFlags ^= (int)eBehavior.wander; }
        public void SeparationOff() { if (On(eBehavior.separation)) m_iFlags ^= (int)eBehavior.separation; }
        public void WallAvoidanceOff() { if (On(eBehavior.wall_avoidance)) m_iFlags ^= (int)eBehavior.wall_avoidance; }

        public bool SeekIsOn() { return On(eBehavior.seek); }
        public bool ArriveIsOn() { return On(eBehavior.arrive); }
        public bool WanderIsOn() { return On(eBehavior.wander); }
        public bool SeparationIsOn() { return On(eBehavior.separation); }
        public bool WallAvoidanceIsOn() { return On(eBehavior.wall_avoidance); }

        public List<Vector3> GetFeelers() { return m_Feelers; }

        public float WanderJitter() { return m_dWanderJitter; }
        public float WanderDistance() { return m_dWanderDistance; }
        public float WanderRadius() { return m_dWanderRadius; }

        public float SeparationWeight() { return m_dWeightSeparation; }

    }


}//end namespace
*/
