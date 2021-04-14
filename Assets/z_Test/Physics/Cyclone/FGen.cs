using System;
using System.Collections.Generic;

namespace Cyclone
{
    public class ForceGenerator
    {

        /**
         * Overload this in implementations of the interface to calculate
         * and update the force applied to the given rigid body.
         */
        public virtual void updateForce(RigidBody body, float duration) { }
    }

    /**
     * A force generator that applies a gravitational force. One instance
     * can be used for multiple rigid bodies.
     */
    public class Gravity : ForceGenerator
    {
        /** Holds the acceleration due to gravity. */
        Vector3 gravity;

    
        /** Creates the generator with the given acceleration. */
        public Gravity(Vector3 gravity)
        {
            this.gravity = gravity;
        }

        /** Applies the gravitational force to the given rigid body. */
        public override void updateForce(RigidBody body, float duration)
        {
            // Check that we do not have infinite mass
            if (!body.hasFiniteMass()) return;

            // Apply the mass-scaled force to the body
            body.addForce(gravity * body.getMass());
        }
    }

    /**
     * A force generator that applies a Spring force.
     */
    public class Spring : ForceGenerator
    {
        /**
         * The point of connection of the spring, in local
         * coordinates.
         */
        Vector3 connectionPoint;

        /**
         * The point of connection of the spring to the other object,
         * in that object's local coordinates.
         */
        Vector3 otherConnectionPoint;

        /** The particle at the other end of the spring. */
        RigidBody other;

        /** Holds the sprint constant. */
        float springConstant;

        /** Holds the rest length of the spring. */
        float restLength;

        /** Creates a new spring with the given parameters. */
        public Spring(Vector3 localConnectionPt,
               RigidBody other,
               Vector3 otherConnectionPt,
               float springConstant,
               float restLength)
        {
            connectionPoint = localConnectionPt;
            otherConnectionPoint = otherConnectionPt;
            this.other = other;
            this.springConstant = springConstant;
            this.restLength = restLength;
        }

        /** Applies the spring force to the given rigid body. */
        public override void updateForce(RigidBody body, float duration)
        {
            // Calculate the two ends in world space
            Vector3 lws = body.getPointInWorldSpace(connectionPoint);
            Vector3 ows = other.getPointInWorldSpace(otherConnectionPoint);

            // Calculate the vector of the spring
            Vector3 force = lws - ows;

            // Calculate the magnitude of the force
            float magnitude = force.magnitude();
            magnitude = Math.Abs(magnitude - restLength);
            magnitude *= springConstant;

            // Calculate the final force and apply it
            force.normalise();
            force *= -magnitude;
            body.addForceAtPoint(force, lws);
        }
    }

    /**
     * A force generator showing a three component explosion effect.
     * This force generator is intended to represent a single
     * explosion effect for multiple rigid bodies. The force generator
     * can also act as a particle force generator.
     */
    public class Explosion : ForceGenerator//, ParticleForceGenerator
    {
        /**
         * Tracks how long the explosion has been in operation, used
         * for time-sensitive effects.
         */
        float timePassed;


        // Properties of the explosion, these are public because
        // there are so many and providing a suitable constructor
        // would be cumbersome:

        /**
         * The location of the detonation of the weapon.
         */
        public Vector3 detonation;

        // ... Other Explosion code as before ...


        /**
         * The radius up to which objects implode in the first stage
         * of the explosion.
         */
        public float implosionMaxRadius;

        /**
         * The radius within which objects don't feel the implosion
         * force. Objects near to the detonation aren't sucked in by
         * the air implosion.
         */
        public float implosionMinRadius;

        /**
         * The length of time that objects spend imploding before the
         * concussion phase kicks in.
         */
        public float implosionDuration;

        /**
         * The maximal force that the implosion can apply. This should
         * be relatively small to avoid the implosion pulling objects
         * through the detonation point and out the other side before
         * the concussion wave kicks in.
         */
        public float implosionForce;

        /**
         * The speed that the shock wave is traveling, this is related
         * to the thickness below in the relationship:
         *
         * thickness >= speed * minimum frame duration
         */
        public float shockwaveSpeed;

        /**
         * The shock wave applies its force over a range of distances,
         * this controls how thick. Faster waves require larger
         * thicknesses.
         */
        public float shockwaveThickness;

        /**
         * This is the force that is applied at the very centre of the
         * concussion wave on an object that is stationary. Objects
         * that are in front or behind of the wavefront, or that are
         * already moving outwards, get proportionally less
         * force. Objects moving in towards the centre get
         * proportionally more force.
         */
        public float peakConcussionForce;

        /**
         * The length of time that the concussion wave is active.
         * As the wave nears this, the forces it applies reduces.
         */
        public float concussionDuration;

        /**
         * This is the peak force for stationary objects in
         * the centre of the convection chimney. Force calculations
         * for this value are the same as for peakConcussionForce.
         */
        public float peakConvectionForce;

        /**
         * The radius of the chimney cylinder in the xz plane.
         */
        public float chimneyRadius;

        /**
         * The maximum height of the chimney.
         */
        public float chimneyHeight;

        /**
         * The length of time the convection chimney is active. Typically
         * this is the longest effect to be in operation, as the heat
         * from the explosion outlives the shock wave and implosion
         * itself.
         */
        public float convectionDuration;


        /**
         * Creates a new explosion with sensible default values.
         */
        //public Explosion();

        /**
         * Calculates and applies the force that the explosion
         * has on the given rigid body.
         */
        public override void updateForce(RigidBody body, float duration)
        {
        }

        /**
         * Calculates and applies the force that the explosion has
         * on the given particle.
         */
        //public virtual void updateForce(Particle* particle, real duration) = 0;

    }

    /**
     * A force generator that applies an aerodynamic force.
     */
    public class Aero : ForceGenerator
    {
    
        /**
         * Holds the aerodynamic tensor for the surface in body
         * space.
         */
        protected Matrix3 tensor;

        /**
         * Holds the relative position of the aerodynamic surface in
         * body coordinates.
         */
        protected Vector3 position;

        /**
         * Holds a pointer to a vector containing the windspeed of the
         * environment. This is easier than managing a separate
         * windspeed vector per generator and having to update it
         * manually as the wind changes.
         */
        public Vector3 windspeed;


        /**
         * Creates a new aerodynamic force generator with the
         * given properties.
         */
        public Aero(Matrix3 tensor, Vector3 position, Vector3 windspeed)
        {
            this.tensor = tensor;
            this.position = position;
            this.windspeed = windspeed;
        }

        /**
         * Applies the force to the given rigid body.
         */
        public override void updateForce(RigidBody body, float duration)
        {
            updateForceFromTensor(body, duration, tensor);
        }


        /**
         * Uses an explicit tensor matrix to update the force on
         * the given rigid body. This is exactly the same as for updateForce
         * only it takes an explicit tensor.
         */
        protected void updateForceFromTensor(RigidBody body, float duration,
                                  Matrix3 tensor)
        {
            // Calculate total velocity (windspeed and body's velocity).
            Vector3 velocity = body.getVelocity();
            velocity += windspeed;

            // Calculate the velocity in body coordinates
            Vector3 bodyVel = body.getTransform().transformInverseDirection(velocity);

            // Calculate the force in body coordinates
            Vector3 bodyForce = tensor.transform(bodyVel);
            Vector3 force = body.getTransform().transformDirection(bodyForce);

            // Apply the force
            body.addForceAtBodyPoint(force, position);
        }
    }

    /**
    * A force generator with a control aerodynamic surface. This
    * requires three inertia tensors, for the two extremes and
    * 'resting' position of the control surface.  The latter tensor is
    * the one inherited from the base class, the two extremes are
    * defined in this class.
    */
    public class AeroControl : Aero
    {

        /**
         * The aerodynamic tensor for the surface, when the control is at
         * its maximum value.
         */
        protected Matrix3 maxTensor;

        /**
         * The aerodynamic tensor for the surface, when the control is at
         * its minimum value.
         */
        protected Matrix3 minTensor;

        /**
        * The current position of the control for this surface. This
        * should range between -1 (in which case the minTensor value
        * is used), through 0 (where the base-class tensor value is
        * used) to +1 (where the maxTensor value is used).
*/
        protected float controlSetting;


        /**
         * Calculates the final aerodynamic tensor for the current
         * control setting.
         */
        Matrix3 getTensor()
        {
            if (controlSetting <= -1.0f) return minTensor;
            else if (controlSetting >= 1.0f) return maxTensor;
            else if (controlSetting < 0)
            {
                return Matrix3.linearInterpolate(minTensor, tensor, controlSetting + 1.0f);
            }
            else if (controlSetting > 0)
            {
                return Matrix3.linearInterpolate(tensor, maxTensor, controlSetting);
            }
            else return tensor;
        }


        /**
         * Creates a new aerodynamic control surface with the given
         * properties.
         */
        public AeroControl(Matrix3 base_, Matrix3 min, Matrix3 max,
                              Vector3 position, Vector3 windspeed)
            : base(base_, position, windspeed)
        {

            this.minTensor = min;
            this.maxTensor = max;
            controlSetting = 0.0f;
        }

        /**
         * Sets the control position of this control. This * should
        range between -1 (in which case the minTensor value is *
        used), through 0 (where the base-class tensor value is used) *
        to +1 (where the maxTensor value is used). Values outside that
                * range give undefined results.
        */
        public void setControl(float value)
        {
            controlSetting = value;
        }

        /**
         * Applies the force to the given rigid body.
         */
        public override void updateForce(RigidBody body, float duration)
        {
            Matrix3 tensor_ = getTensor();
            updateForceFromTensor(body, duration, tensor_);
        }
    }

    /**
     * A force generator with an aerodynamic surface that can be
     * re-oriented relative to its rigid body. This derives the
     */
    public class AngledAero : Aero
    {
        /**
         * Holds the orientation of the aerodynamic surface relative
         * to the rigid body to which it is attached.
         */
        Quaternion orientation;


        /**
         * Creates a new aerodynamic surface with the given properties.
         */
        public AngledAero(Matrix3 tensor, Vector3 position, Vector3 windspeed)
            :base(tensor, position, windspeed)
        {}

        /**
         * Sets the relative orientation of the aerodynamic surface,
         * relative to the rigid body it is attached to. Note that
         * this doesn't affect the point of connection of the surface
         * to the body.
         */
        //public void setOrientation(const Quaternion &quat);

        /**
         * Applies the force to the given rigid body.
         */
        //public override void updateForce(RigidBody* body, real duration);
    }

    /**
     * A force generator to apply a buoyant force to a rigid body.
     */
    public class Buoyancy : ForceGenerator
    {
        /**
         * The maximum submersion depth of the object before
         * it generates its maximum buoyancy force.
         */
        float maxDepth;

        /**
         * The volume of the object.
         */
        float volume;

        /**
         * The height of the water plane above y=0. The plane will be
         * parallel to the XZ plane.
         */
        float waterHeight;

        /**
         * The density of the liquid. Pure water has a density of
         * 1000kg per cubic meter.
         */
        float liquidDensity;

        /**
         * The centre of buoyancy of the rigid body, in body coordinates.
         */
        Vector3 centreOfBuoyancy;


        /** Creates a new buoyancy force with the given parameters. */
        public Buoyancy(Vector3 cOfB, float maxDepth, float volume,
                   float waterHeight, float liquidDensity = 1000.0f )
        {
            centreOfBuoyancy = cOfB;
            this.liquidDensity = liquidDensity;
            this.maxDepth = maxDepth;
            this.volume = volume;
            this.waterHeight = waterHeight;
        }

        /**
         * Applies the force to the given rigid body.
         */
        public override void updateForce(RigidBody body, float duration)
        {
            // Calculate the submersion depth
            Vector3 pointInWorld = body.getPointInWorldSpace(centreOfBuoyancy);
            float depth = pointInWorld.y;

            // Check if we're out of the water
            if (depth >= waterHeight + maxDepth) return;
            Vector3 force = new Vector3(0,0,0);

            // Check if we're at maximum depth
            if (depth <= waterHeight - maxDepth)
            {
                force.y = liquidDensity * volume;
                body.addForceAtBodyPoint(force, centreOfBuoyancy);
                return;
            }

            // Otherwise we are partly submerged
            force.y = liquidDensity * volume *
                (depth - maxDepth - waterHeight) / 2 * maxDepth;
            body.addForceAtBodyPoint(force, centreOfBuoyancy);
        }
    }

    /**
    * Holds all the force generators and the bodies they apply to.
    */
    public class ForceRegistry
    {
    
        /**
        * Keeps track of one force generator and the body it
        * applies to.
        */
        public struct ForceRegistration
        {
            public RigidBody body;
            public ForceGenerator fg;
        }

        /**
        * Holds the list of registrations.
        */
        //typedef std::vector<ForceRegistration> Registry;
        public class Registry : List<ForceRegistration> { }
        Registry registrations = new Registry();

    
        /**
        * Registers the given force generator to apply to the
        * given body.
        */
        public void add(RigidBody body, ForceGenerator fg)
        {
            ForceRegistration registration = new ForceRegistration();
            registration.body = body;
            registration.fg = fg;
            registrations.Add(registration);
        }

        /**
        * Removes the given registered pair from the registry.
        * If the pair is not registered, this method will have
        * no effect.
        */
        //public void remove(RigidBody* body, ForceGenerator* fg);

        /**
        * Clears all registrations from the registry. This will
        * not delete the bodies or the force generators
        * themselves, just the records of their connection.
        */
        //public void clear();

        /**
        * Calls all the force generators to update the forces of
        * their corresponding bodies.
        */
        public void updateForces(float duration)
        {
            foreach(ForceRegistration i in registrations)
            {
                i.fg.updateForce(i.body, duration);
            }

            //Registry::iterator i = registrations.begin();
            //for (; i != registrations.end(); i++)
            //{
            //    i->fg->updateForce(i->body, duration);
            //}
        }
    }
}
