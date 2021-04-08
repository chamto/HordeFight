using System;
using System.Collections.Generic;

namespace Cyclone
{
    public class ParticleForceGenerator
    {

        /**
         * Overload this in implementations of the interface to calculate
         * and update the force applied to the given particle.
         */
        public virtual void updateForce(Particle particle, float duration) { }
    }

    /**
     * A force generator that applies a gravitational force. One instance
     * can be used for multiple particles.
     */
    public class ParticleGravity : ParticleForceGenerator
    {
        /** Holds the acceleration due to gravity. */
        Vector3 gravity;

    
        /** Creates the generator with the given acceleration. */
        public ParticleGravity(Vector3 gravity)
        {
            this.gravity = gravity;
        }

        /** Applies the gravitational force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Check that we do not have infinite mass
            if (!particle.hasFiniteMass()) return;

            // Apply the mass-scaled force to the particle
            particle.addForce(gravity * particle.getMass());
        }
    }

    /**
     * A force generator that applies a drag force. One instance
     * can be used for multiple particles.
     */
    public class ParticleDrag : ParticleForceGenerator
    {
        /** Holds the velocity drag coeffificent. */
        float k1;

        /** Holds the velocity squared drag coeffificent. */
        float k2;

        /** Creates the generator with the given coefficients. */
        public ParticleDrag(float k1, float k2)
        {
            this.k1 = k1;
            this.k2 = k2;
        }

        /** Applies the drag force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            Vector3 force;
            particle.getVelocity(out force);

            // Calculate the total drag coefficient
            float dragCoeff = force.magnitude();
            dragCoeff = k1 * dragCoeff + k2 * dragCoeff * dragCoeff;

            // Calculate the final force and apply it
            force.normalise();
            force *= -dragCoeff;
            particle.addForce(force);
        }
    }

    /**
     * A force generator that applies a Spring force, where
     * one end is attached to a fixed point in space.
     */
    public class ParticleAnchoredSpring : ParticleForceGenerator
    {
    
        /** The location of the anchored end of the spring. */
        protected Vector3 anchor;

        /** Holds the sprint constant. */
        protected float springConstant;

        /** Holds the rest length of the spring. */
        protected float restLength;


        public ParticleAnchoredSpring()
        { }

        /** Creates a new spring with the given parameters. */
        public ParticleAnchoredSpring(Vector3 anchor,
                                               float sc, float rl)
        {
            this.anchor = anchor;
            this.springConstant = sc;
            this.restLength = rl;
        }

        /** Retrieve the anchor point. */
        public Vector3 getAnchor() { return anchor; }

        /** Set the spring's properties. */
        public void init(Vector3 anchor, float springConstant,
                                  float restLength)
        {
            this.anchor = anchor;
            this.springConstant = springConstant;
            this.restLength = restLength;
        }

        /** Applies the spring force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Calculate the vector of the spring
            Vector3 force;
            particle.getPosition(out force);
            force -= anchor;

            // Calculate the magnitude of the force
            float magnitude = force.magnitude();
            magnitude = (restLength - magnitude) * springConstant;

            // Calculate the final force and apply it
            force.normalise();
            force *= magnitude;
            particle.addForce(force);
        }
    }

    /**
    * A force generator that applies a bungee force, where
    * one end is attached to a fixed point in space.
    */
    public class ParticleAnchoredBungee : ParticleAnchoredSpring
    {

        /** Applies the spring force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Calculate the vector of the spring
            Vector3 force;
            particle.getPosition(out force);
            force -= anchor;

            // Calculate the magnitude of the force
            float magnitude = force.magnitude();
            if (magnitude < restLength) return;

            magnitude = magnitude - restLength;
            magnitude *= springConstant;

            // Calculate the final force and apply it
            force.normalise();
            force *= -magnitude;
            particle.addForce(force);
        }
    }

    /**
     * A force generator that fakes a stiff spring force, and where
     * one end is attached to a fixed point in space.
     */
    public class ParticleFakeSpring : ParticleForceGenerator
    {
        /** The location of the anchored end of the spring. */
        Vector3 anchor;

        /** Holds the sprint constant. */
        float springConstant;

        /** Holds the damping on the oscillation of the spring. */
        float damping;

        /** Creates a new spring with the given parameters. */
        public ParticleFakeSpring(Vector3 anchor, float sc, float d)
        {
            this.anchor = anchor;
            this.springConstant = sc;
            this.damping = d;
        }

        /** Applies the spring force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Check that we do not have infinite mass
            if (!particle.hasFiniteMass()) return;

            // Calculate the relative position of the particle to the anchor
            Vector3 position;
            particle.getPosition(out position);
            position -= anchor;

            // Calculate the constants and check they are in bounds.
            float gamma = 0.5f * (float)Math.Sqrt(4 * springConstant - damping * damping);
            if (gamma == 0.0f) return;
            Vector3 c = position * (damping / (2.0f * gamma)) +
                particle.getVelocity() * (1.0f / gamma);

            // Calculate the target position
            Vector3 target = position * (float)Math.Cos(gamma * duration) +
                c * (float)Math.Sin(gamma * duration);
            target *= (float)Math.Exp(-0.5f * duration * damping);

            // Calculate the resulting acceleration and therefore the force
            Vector3 accel = (target - position) * (1.0f / (duration * duration)) -
                particle.getVelocity() * (1.0f / duration);
            particle.addForce(accel * particle.getMass());
        }
    }

    /**
     * A force generator that applies a Spring force.
     */
    public class ParticleSpring : ParticleForceGenerator
    {
        /** The particle at the other end of the spring. */
        Particle other;

        /** Holds the sprint constant. */
        float springConstant;

        /** Holds the rest length of the spring. */
        float restLength;

        /** Creates a new spring with the given parameters. */
        public ParticleSpring(Particle other, float sc, float rl)
        {
            this.other = other;
            this.springConstant = sc;
            this.restLength = rl;
        }

        /** Applies the spring force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Calculate the vector of the spring
            Vector3 force;
            particle.getPosition(out force);
            force -= other.getPosition();

            // Calculate the magnitude of the force
            float magnitude = force.magnitude();
            magnitude = Math.Abs(magnitude - restLength);
            magnitude *= springConstant;

            // Calculate the final force and apply it
            force.normalise();
            force *= -magnitude;
            particle.addForce(force);
        }
    }

    /**
     * A force generator that applies a spring force only
     * when extended.
     */
    public class ParticleBungee : ParticleForceGenerator
    {
        /** The particle at the other end of the spring. */
        Particle other;

        /** Holds the sprint constant. */
        float springConstant;

        /**
         * Holds the length of the bungee at the point it begins to
         * generator a force.
         */
        float restLength;

        /** Creates a new bungee with the given parameters. */
        public ParticleBungee(Particle other, float sc, float rl)
        {
            this.other = other;
            this.springConstant = sc;
            this.restLength = rl;
        }

        /** Applies the spring force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Calculate the vector of the spring
            Vector3 force;
            particle.getPosition(out force);
            force -= other.getPosition();

            // Check if the bungee is compressed
            float magnitude = force.magnitude();
            if (magnitude <= restLength) return;

            // Calculate the magnitude of the force
            magnitude = springConstant * (restLength - magnitude);

            // Calculate the final force and apply it
            force.normalise();
            force *= -magnitude;
            particle.addForce(force);
        }
    }

    /**
     * A force generator that applies a buoyancy force for a plane of
     * liquid parrallel to XZ plane.
     */
    public class ParticleBuoyancy : ParticleForceGenerator
    {
        /**
         * The maximum submersion depth of the object before
         * it generates its maximum boyancy force.
         */
        float maxDepth;

        /**
         * The volume of the object.
         */
        float volume;

        /**
         * The height of the water plane above y=0. The plane will be
         * parrallel to the XZ plane.
         */
        float waterHeight;

        /**
         * The density of the liquid. Pure water has a density of
         * 1000kg per cubic meter.
         */
        float liquidDensity;

        /** Creates a new buoyancy force with the given parameters. */
        public ParticleBuoyancy(float maxDepth,
                                 float volume,
                                 float waterHeight,
                                 float liquidDensity = 1000f)
        {
            this.maxDepth = maxDepth;
            this.volume = volume;
            this.waterHeight = waterHeight;
            this.liquidDensity = liquidDensity;
        }

        /** Applies the buoyancy force to the given particle. */
        public override void updateForce(Particle particle, float duration)
        {
            // Calculate the submersion depth
            float depth = particle.getPosition().y;

            // Check if we're out of the water
            if (depth >= waterHeight + maxDepth) return;
            Vector3 force = new Vector3(0,0,0);

            // Check if we're at maximum depth
            if (depth <= waterHeight - maxDepth)
            {
                force.y = liquidDensity * volume;
                particle.addForce(force);
                return;
            }

            // Otherwise we are partly submerged
            force.y = liquidDensity * volume *
                (depth - maxDepth - waterHeight) / (2 * maxDepth);
            particle.addForce(force);
        }
    }

    /**
     * Holds all the force generators and the particles they apply to.
     */
    public class ParticleForceRegistry
    {
    
        /**
         * Keeps track of one force generator and the particle it
         * applies to.
         */
        protected struct ParticleForceRegistration
        {
            public Particle particle;
            public ParticleForceGenerator fg;
        }

        /**
         * Holds the list of registrations.
         */
        //typedef std::vector<ParticleForceRegistration> Registry;
        //Registry registrations;
        protected List<ParticleForceRegistration> registrations = new List<ParticleForceRegistration>();

    
        /**
         * Registers the given force generator to apply to the
         * given particle.
         */
        public void add(Particle particle, ParticleForceGenerator fg)
        {
            ParticleForceRegistration registration = new ParticleForceRegistration();
            registration.particle = particle;
            registration.fg = fg;
            registrations.Add(registration);
        }

        /**
         * Removes the given registered pair from the registry.
         * If the pair is not registered, this method will have
         * no effect.
         */
        //public void remove(Particle* particle, ParticleForceGenerator* fg);

        /**
         * Clears all registrations from the registry. This will
         * not delete the particles or the force generators
         * themselves, just the records of their connection.
         */
        //public void clear();

        /**
         * Calls all the force generators to update the forces of
         * their corresponding particles.
         */
        public void updateForces(float duration)
        {
            foreach(ParticleForceRegistration i in registrations)
            {
                i.fg.updateForce(i.particle, duration);
            }

            //Registry::iterator i = registrations.begin();
            //for (; i != registrations.end(); i++)
            //{
            //    i->fg->updateForce(i->particle, duration);
            //}
        }
    }
}
