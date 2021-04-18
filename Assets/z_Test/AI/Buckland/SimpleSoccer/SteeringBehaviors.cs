using System;
using System.Collections.Generic;
using UnityEngine;
using Buckland;
using UtilGS9;

namespace Test_SimpleSoccer
{
    public enum behavior_type
    {
        none = 0x0000,
        seek = 0x0001,
        arrive = 0x0002,
        separation = 0x0004,
        pursuit = 0x0008,
        interpose = 0x0010
    }

    public class SteeringBehaviors
    {

        PlayerBase m_pPlayer;

        SoccerBall m_pBall;

        //the steering force created by the combined effect of all
        //the selected behaviors
        Vector3 m_vSteeringForce;

        //the current target (usually the ball or predicted ball position)
        Vector3 m_vTarget;

        //the distance the player tries to interpose from the target
        float m_dInterposeDist;

        //multipliers. 
        float m_dMultSeparation;

        //how far it can 'see'
        float m_dViewDistance;


        //binary flags to indicate whether or not a behavior should be active
        int m_iFlags;


        //used by group behaviors to tag neighbours
        bool m_bTagged;

        //Arrive makes use of these to determine how quickly a vehicle
        //should decelerate to its target
        public enum Deceleration { slow = 3, normal = 2, fast = 1 };


        //this behavior moves the agent towards a target position
        Vector3 Seek(Vector3 target)
        {

            Vector3 DesiredVelocity = (target - m_pPlayer.Pos()).normalized * m_pPlayer.MaxSpeed();

            return (DesiredVelocity - m_pPlayer.Velocity());
        }

        //this behavior is similar to seek but it attempts to arrive 
        //at the target with a zero velocity
        Vector3 Arrive(Vector3 target, Deceleration deceleration)
        {
            Vector3 ToTarget = target - m_pPlayer.Pos();

            //calculate the distance to the target
            float dist = ToTarget.magnitude;

            if (dist > 0)
            {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration..
                const float DecelerationTweaker = 0.3f;

                //calculate the speed required to reach the target given the desired
                //deceleration
                float speed = dist / ((float)deceleration * DecelerationTweaker);

                //make sure the velocity does not exceed the max
                speed = Math.Min(speed, m_pPlayer.MaxSpeed());

                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                Vector3 DesiredVelocity = ToTarget * speed / dist;

                return (DesiredVelocity - m_pPlayer.Velocity());
            }

            return Vector3.zero;
        }

        //This behavior predicts where its prey will be and seeks
        //to that location
        Vector3 Pursuit(SoccerBall ball)
        {
            Vector3 ToBall = ball.Pos() - m_pPlayer.Pos();

            //the lookahead time is proportional to the distance between the ball
            //and the pursuer; 
            float LookAheadTime = 0.0f;

            if (ball.Speed() != 0.0)
            {
                LookAheadTime = ToBall.magnitude / ball.Speed();
            }

            //calculate where the ball will be at this time in the future
            m_vTarget = ball.FuturePosition(LookAheadTime);

          //now seek to the predicted future position of the ball
          return Arrive(m_vTarget, Deceleration.fast);
        }

        Vector3 Separation()
        {
            //iterate through all the neighbors and calculate the vector from the
            Vector3 SteeringForce = Vector3.zero;

            LinkedList<PlayerBase> AllPlayers = AutoList< PlayerBase >.GetAllMembers();
            //std::list<PlayerBase*>::iterator curPlyr;
            //for (curPlyr = AllPlayers.begin(); curPlyr != AllPlayers.end(); ++curPlyr)
            foreach(PlayerBase curPlyr in AllPlayers)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent is close enough
                if ((curPlyr != m_pPlayer) && (curPlyr).Steering().Tagged())
                {
                    Vector3 ToAgent = m_pPlayer.Pos() - (curPlyr).Pos();

                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    SteeringForce += (ToAgent.normalized) / ToAgent.magnitude;
                }
            }

            return SteeringForce;
        }

        //this attempts to steer the agent to a position between the opponent
        //and the object
        Vector3 Interpose(SoccerBall ball,
                                              Vector3 target,
                                              float DistFromTarget)
        {
            return Arrive(target + (ball.Pos() - target).normalized *
                          DistFromTarget, Deceleration.normal);
        }


        //finds any neighbours within the view radius
        void FindNeighbours()
        {
            LinkedList<PlayerBase> AllPlayers = AutoList < PlayerBase >.GetAllMembers();
            //std::list<PlayerBase*>::iterator curPlyr;
            //for (curPlyr = AllPlayers.begin(); curPlyr != AllPlayers.end(); ++curPlyr)
            foreach(PlayerBase curPlyr in AllPlayers)
            {
                //first clear any current tag
                (curPlyr).Steering().UnTag();

                //work in distance squared to avoid sqrts
                Vector3 to = (curPlyr).Pos() - m_pPlayer.Pos();

                if (to.sqrMagnitude < (m_dViewDistance * m_dViewDistance))
                {
                    (curPlyr).Steering().Tag();
                }
            }//next
        }


        //this function tests if a specific bit of m_iFlags is set
        bool On(behavior_type bt) { return (m_iFlags & (int)bt) == (int)bt; }

        bool AccumulateForce(ref Vector3 sf, Vector3 ForceToAdd)
        {
            //first calculate how much steering force we have left to use
            float MagnitudeSoFar = sf.magnitude;

            float magnitudeRemaining = m_pPlayer.MaxForce() - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (magnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = ForceToAdd.magnitude;

            //now calculate how much of the force we can really add  
            if (MagnitudeToAdd > magnitudeRemaining)
            {
                MagnitudeToAdd = magnitudeRemaining;
            }

            //add it to the steering force
            sf += ((ForceToAdd).normalized * MagnitudeToAdd);

            return true;
        }

        Vector3 SumForces()
        {
            Vector3 force = Vector3.zero;

            //the soccer players must always tag their neighbors
            FindNeighbours();

            if (On(behavior_type.separation))
            {
                force += Separation() * m_dMultSeparation;

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(behavior_type.seek))
            {
                force += Seek(m_vTarget);

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(behavior_type.arrive))
            {
                force += Arrive(m_vTarget, Deceleration.fast);

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(behavior_type.pursuit))
            {
                force += Pursuit(m_pBall);

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            if (On(behavior_type.interpose))
            {
                force += Interpose(m_pBall, m_vTarget, m_dInterposeDist);

                if (!AccumulateForce(ref m_vSteeringForce, force)) return m_vSteeringForce;
            }

            return m_vSteeringForce;
        }

        //a vertex buffer to contain the feelers rqd for dribbling
        List<Vector3> m_Antenna;



        public SteeringBehaviors(PlayerBase agent,
                                     SoccerPitch world,
                                     SoccerBall ball)

        {
            m_pPlayer = agent;
            m_iFlags = 0;
            m_dMultSeparation = Prm.SeparationCoefficient;
            m_bTagged = false;
            m_dViewDistance = Prm.ViewDistance;
            m_pBall = ball;
            m_dInterposeDist = 0f;
            m_Antenna = new List<Vector3>();
            for (int i = 0; i < 5; i++)
            {
                m_Antenna.Add(Vector3.zero);
            }
        }

        //virtual ~SteeringBehaviors() { }


        public Vector3 Calculate()
        {
            //reset the force
            m_vSteeringForce = Vector3.zero;

            //this will hold the value of each individual steering force
            m_vSteeringForce = SumForces();

            //make sure the force doesn't exceed the vehicles maximum allowable
            m_vSteeringForce = VOp.Truncate(m_vSteeringForce, m_pPlayer.MaxForce());
            //m_vSteeringForce.Truncate(m_pPlayer->MaxForce());

            return m_vSteeringForce;
        }

        //calculates the component of the steering force that is parallel
        //with the vehicle heading
        public float ForwardComponent()
        {
            return Vector3.Dot( m_pPlayer.Heading(),m_vSteeringForce);
        }

        //calculates the component of the steering force that is perpendicuar
        //with the vehicle heading
        public float SideComponent()
        {
            return Vector3.Dot( m_pPlayer.Side(), m_vSteeringForce) * m_pPlayer.MaxTurnRate();
        }

        public Vector3 Force() {return m_vSteeringForce;}

        //renders visual aids and info for seeing how each behavior is
        //calculated
        //public void RenderInfo();
        public void RenderAids()
        {
            //render the steering force
            //gdi->RedPen();
            //gdi->Line(m_pPlayer->Pos(), m_pPlayer->Pos() + m_vSteeringForce * 20);

            DebugWide.DrawLine(m_pPlayer.Pos(), m_pPlayer.Pos() + m_vSteeringForce * 20, Color.red);
        }

        public Vector3 Target() {return m_vTarget;}
        public void SetTarget(Vector3 t) { m_vTarget = t; }

        public float InterposeDistance() {return m_dInterposeDist;}
        public void SetInterposeDistance(float d) { m_dInterposeDist = d; }

        public bool Tagged() {return m_bTagged;}
        public void Tag() { m_bTagged = true; }
        public void UnTag() { m_bTagged = false; }


        public void SeekOn() { m_iFlags |= (int)behavior_type.seek; }
        public void ArriveOn() { m_iFlags |= (int)behavior_type.arrive; }
        public void PursuitOn() { m_iFlags |= (int)behavior_type.pursuit; }
        public void SeparationOn() { m_iFlags |= (int)behavior_type.separation; }
        public void InterposeOn(float d) { m_iFlags |= (int)behavior_type.interpose; m_dInterposeDist = d; }


        public void SeekOff() { if (On(behavior_type.seek)) m_iFlags ^= (int)behavior_type.seek; }
        public void ArriveOff() { if (On(behavior_type.arrive)) m_iFlags ^= (int)behavior_type.arrive; }
        public void PursuitOff() { if (On(behavior_type.pursuit)) m_iFlags ^= (int)behavior_type.pursuit; }
        public void SeparationOff() { if (On(behavior_type.separation)) m_iFlags ^= (int)behavior_type.separation; }
        public void InterposeOff() { if (On(behavior_type.interpose)) m_iFlags ^= (int)behavior_type.interpose; }


        public bool SeekIsOn() { return On(behavior_type.seek); }
        public bool ArriveIsOn() { return On(behavior_type.arrive); }
        public bool PursuitIsOn() { return On(behavior_type.pursuit); }
        public bool SeparationIsOn() { return On(behavior_type.separation); }
        public bool InterposeIsOn() { return On(behavior_type.interpose); }

    }
}//end namespace

