using System;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace SteeringBehavior
{
    public class MovingEntity : BaseGameEntity
    {

        protected Vector2 m_vVelocity;

        //a normalized vector pointing in the direction the entity is heading. 
        protected Vector2 m_vHeading;

        //a vector perpendicular to the heading vector
        protected Vector2 m_vSide;

        protected float m_dMass;

        //the maximum speed this entity may travel at.
        protected float m_dMaxSpeed;

        //the maximum force this entity can produce to power itself 
        //(think rockets and thrust)
        protected float m_dMaxForce;

        //the maximum rate (radians per second)this vehicle can rotate         
        protected float m_dMaxTurnRate;


        public MovingEntity(Vector2 position,
                               float radius,
                               Vector2 velocity,
                               float max_speed,
                               Vector2 heading,
                               float mass,
                               Vector2 scale,
                               float turn_rate,
                               float max_force) : base(0, position, radius)
        {
            m_vHeading = heading;
            m_vVelocity = velocity;
            m_dMass = mass;
            m_vSide = VOp.Perp(m_vHeading);
            m_dMaxSpeed = max_speed;
            m_dMaxTurnRate = turn_rate;
            m_dMaxForce = max_force;
            m_vScale = scale;
        }


        //accessors
        public Vector2 Velocity() { return m_vVelocity; }
        public void SetVelocity(Vector2 NewVel) { m_vVelocity = NewVel; }

        public float Mass() { return m_dMass; }

        public Vector2 Side() { return m_vSide; }

        public float MaxSpeed() { return m_dMaxSpeed; }
        public void SetMaxSpeed(float new_speed) { m_dMaxSpeed = new_speed; }

        public float MaxForce() { return m_dMaxForce; }
        public void SetMaxForce(float mf) { m_dMaxForce = mf; }

        public bool IsSpeedMaxedOut() { return m_dMaxSpeed * m_dMaxSpeed >= m_vVelocity.sqrMagnitude; }
        public float Speed() { return m_vVelocity.magnitude; }
        public float SpeedSq() { return m_vVelocity.sqrMagnitude; }

        public float MaxTurnRate() { return m_dMaxTurnRate; }
        public void SetMaxTurnRate(float val) { m_dMaxTurnRate = val; }

        public Vector2 Heading() { return m_vHeading; }

        //------------------------- SetHeading ----------------------------------------
        //
        //  first checks that the given heading is not a vector of zero length. If the
        //  new heading is valid this fumction sets the entity's heading and side 
        //  vectors accordingly
        //-----------------------------------------------------------------------------
        public void SetHeading(Vector2 new_heading)
        {
            //assert((new_heading.sqrMagnitude - 1.0f) < 0.00001f);

            m_vHeading = new_heading;

            //the side vector must always be perpendicular to the heading
            m_vSide = VOp.Perp(m_vHeading);
        }

        //--------------------------- RotateHeadingToFacePosition ---------------------
        //
        //  given a target position, this method rotates the entity's heading and
        //  side vectors by an amount not greater than m_dMaxTurnRate until it
        //  directly faces the target.
        //
        //  returns true when the heading is facing in the desired direction
        //-----------------------------------------------------------------------------
        public bool RotateHeadingToFacePosition(Vector2 target)
        {
            Vector2 toTarget = (target - m_vPos).normalized;

            //first determine the angle between the heading vector and the target
            float angle = (float)Math.Acos(Vector2.Dot(m_vHeading, toTarget));

            //return true if the player is facing the target
            if (angle < 0.00001f) return true;

            //clamp the amount to turn to the max turn rate
            if (angle > m_dMaxTurnRate) angle = m_dMaxTurnRate;

            //The next few lines use a rotation matrix to rotate the player's heading
            //vector accordingly

            C2DMatrix RotationMatrix = C2DMatrix.identity;
            //notice how the direction of rotation has to be determined when creating
            //the rotation matrix
            RotationMatrix.RotateY(angle * Util.Sign(m_vHeading, toTarget));
            C2DMatrix.Transform(ref RotationMatrix, ref m_vHeading);
            C2DMatrix.Transform(ref RotationMatrix, ref m_vVelocity);

            //finally recreate m_vSide
            m_vSide = VOp.Perp(m_vHeading);

            return false;
        }

    }
}

