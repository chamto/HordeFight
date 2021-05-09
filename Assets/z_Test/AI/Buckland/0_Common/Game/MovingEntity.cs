using System;
using UnityEngine;
using UtilGS9;

namespace Buckland
{
    public class MovingEntity : BaseGameEntity
    {

        protected Vector3 m_vVelocity;

        //a normalized vector pointing in the direction the entity is heading. 
        protected Vector3 m_vHeading;

        //a vector perpendicular to the heading vector
        protected Vector3 m_vSide;

        protected float m_dMass;

        //the maximum speed this entity may travel at.
        protected float m_dMaxSpeed;

        //the maximum force this entity can produce to power itself 
        //(think rockets and thrust)
        protected float m_dMaxForce;

        //the maximum rate (radians per second)this vehicle can rotate         
        protected float m_dMaxTurnRate;


        public MovingEntity(Vector3 position,
                   float radius,
                   Vector3 velocity,
                   float max_speed,
                   Vector3 heading,
                   float mass,
                   Vector3 scale,
                   float turn_rate,
                   float max_force) : base(BaseGameEntity.GetNextValidID())
        {
            m_vHeading = heading;
            m_vVelocity = velocity;
            m_dMass = mass;
            m_vSide = Vector3.Cross(heading, ConstV.v3_up);
            m_dMaxSpeed = max_speed;
            m_dMaxTurnRate = turn_rate;
            m_dMaxForce = max_force;
            m_vPosition = position;
            m_dBoundingRadius = radius;
            m_vScale = scale;

            //DebugWide.LogBlue(m_dMaxSpeed +" ----------");
        }


        //accessors
        public Vector3 Velocity() { return m_vVelocity; }
        public void SetVelocity(Vector3 NewVel) { m_vVelocity = NewVel; }

        public float Mass() { return m_dMass; }

        public Vector3 Side() { return m_vSide; }

        public float MaxSpeed() { return m_dMaxSpeed; }
        public void SetMaxSpeed(float new_speed) { m_dMaxSpeed = new_speed; }

        public float MaxForce() { return m_dMaxForce; }
        public void SetMaxForce(float mf) { m_dMaxForce = mf; }

        public bool IsSpeedMaxedOut() { return m_dMaxSpeed * m_dMaxSpeed >= m_vVelocity.sqrMagnitude; }
        public float Speed() { return m_vVelocity.magnitude; }
        public float SpeedSq() { return m_vVelocity.sqrMagnitude; }

        public Vector3 Heading() { return m_vHeading; }

        //------------------------- SetHeading ----------------------------------------
        //
        //  first checks that the given heading is not a vector of zero length. If the
        //  new heading is valid this fumction sets the entity's heading and side 
        //  vectors accordingly
        //-----------------------------------------------------------------------------
        public void SetHeading(Vector3 new_heading)
        {
            //assert((new_heading.LengthSq() - 1.0) < 0.00001);

            m_vHeading = new_heading;

            //the side vector must always be perpendicular to the heading
            m_vSide = Vector3.Cross(m_vHeading, ConstV.v3_up);
        }

        //--------------------------- RotateHeadingToFacePosition ---------------------
        //
        //  given a target position, this method rotates the entity's heading and
        //  side vectors by an amount not greater than m_dMaxTurnRate until it
        //  directly faces the target.
        //
        //  returns true when the heading is facing in the desired direction
        //-----------------------------------------------------------------------------
        public bool RotateHeadingToFacePosition(Vector3 target)
        {
            Vector3 toTarget = VOp.Normalize(target - m_vPosition);

            float dot = Vector3.Dot(m_vHeading, toTarget);

            //some compilers lose acurracy so the value is clamped to ensure it
            //remains valid for the acos
            dot = Mathf.Clamp(dot, -1, 1);


            //first determine the angle between the heading vector and the target
            float angle = (float)Math.Acos(dot);

            //return true if the player is facing the target
            if (angle < 0.00001f) return true;

            //clamp the amount to turn to the max turn rate
            if (angle > m_dMaxTurnRate) angle = m_dMaxTurnRate;

            Vector3 up = Vector3.Cross(m_vHeading, toTarget);
            Quaternion rotQ = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, up);
            m_vHeading = rotQ * m_vHeading;
            m_vVelocity = rotQ * m_vVelocity;

            //finally recreate m_vSide
            m_vSide = Vector3.Cross(ConstV.v3_up, m_vHeading);

            return false;
        }

        public float MaxTurnRate() { return m_dMaxTurnRate; }
        public void SetMaxTurnRate(float val) { m_dMaxTurnRate = val; }

    }


}//end namespace

