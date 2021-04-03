using System;

namespace Cyclone
{
    /**
     * A particle is the simplest object that can be simulated in the
     * physics system.
     *
     * It has position data (no orientation data), along with
     * velocity. It can be integrated forward through time, and have
     * linear forces, and impulses applied to it. The particle manages
     * its state and allows access through a set of methods.
     */
    public class Particle
    {

        // ... Other Particle code as before ...


        /**
         * @name Characteristic Data and State
         *
         * This data holds the state of the particle. There are two
         * sets of data: characteristics and state.
         *
         * Characteristics are properties of the particle
         * independent of its current kinematic situation. This
         * includes mass, moment of inertia and damping
         * properties. Two identical particles will have the same
         * values for their characteristics.
         *
         * State includes all the characteristics and also includes
         * the kinematic situation of the particle in the current
         * simulation. By setting the whole state data, a particle's
         * exact game state can be replicated. Note that state does
         * not include any forces applied to the body. Two identical
         * rigid bodies in the same simulation will not share the same
         * state values.
         *
         * The state values make up the smallest set of independent
         * data for the particle. Other state data is calculated
         * from their current values. When state data is changed the
         * dependent values need to be updated: this can be achieved
         * either by integrating the simulation, or by calling the
         * calculateInternals function. This two stage process is used
         * because recalculating internals can be a costly process:
         * all state changes should be carried out at the same time,
         * allowing for a single call.
         *
         * @see calculateInternals
         */
        /*@{*/

        /**
         * Holds the inverse of the mass of the particle. It
         * is more useful to hold the inverse mass because
         * integration is simpler, and because in real time
         * simulation it is more useful to have objects with
         * infinite mass (immovable) than zero mass
         * (completely unstable in numerical simulation).
         */
        protected float inverseMass;

        /**
         * Holds the amount of damping applied to linear
         * motion. Damping is required to remove energy added
         * through numerical instability in the integrator.
         */
        protected float damping;

        /**
         * Holds the linear position of the particle in
         * world space.
         */
        protected Vector3 position;

        /**
         * Holds the linear velocity of the particle in
         * world space.
         */
        protected Vector3 velocity;

        /*@}*/

        /**
         * @name Force Accumulators
         *
         * These data members store the current force and
         * global linear acceleration of the particle.
         */

        /*@{*/

        /**
         * Holds the accumulated force to be applied at the next
         * simulation iteration only. This value is zeroed at each
         * integration step.
         */
        protected Vector3 forceAccum;

        /**
         * Holds the acceleration of the particle.  This value
         * can be used to set acceleration due to gravity (its primary
         * use), or any other constant acceleration.
         */
        protected Vector3 acceleration;

        /*@}*/

        
        /**
         * @name Constructor and Destructor
         *
         * There are no data members in the particle class that are
         * created on the heap. So all data storage is handled
         * automatically.
         */
        /*@{*/
        /*@}*/

        /**
         * @name Integration and Simulation Functions
         *
         * These functions are used to simulate the particle's
         * motion over time. A normal application sets up one or more
         * rigid bodies, applies permanent forces (i.e. gravity), then
         * adds transient forces each frame, and integrates, prior to
         * rendering.
         *
         * Currently the only integration function provided is the
         * first order Newton Euler method.
         */
        /*@{*/

        /**
         * Integrates the particle forward in time by the given amount.
         * This function uses a Newton-Euler integration method, which is a
         * linear approximation to the correct integral. For this reason it
         * may be inaccurate in some cases.
         */
        public void integrate(float duration)
        {
            // We don't integrate things with zero mass.
            if (inverseMass <= 0.0f) return;

            //assert(duration > 0.0);

            // Update linear position.
            position.addScaledVector(velocity, duration);

            // Work out the acceleration from the force
            Vector3 resultingAcc = acceleration;
            resultingAcc.addScaledVector(forceAccum, inverseMass);

            // Update linear velocity from the acceleration.
            velocity.addScaledVector(resultingAcc, duration);

            // Impose drag.
            velocity *= (float)Math.Pow(damping, duration);

            // Clear the forces.
            clearAccumulator();
        }

        /*@}*/


        /**
         * @name Accessor Functions for the Particle's State
         *
         * These functions provide access to the particle's
         * characteristics or state.
         */
        /*@{*/

        /**
         * Sets the mass of the particle.
         *
         * @param mass The new mass of the body. This may not be zero.
         * Small masses can produce unstable rigid bodies under
         * simulation.
         *
         * @warning This invalidates internal data for the particle.
         * Either an integration function, or the calculateInternals
         * function should be called before trying to get any settings
         * from the particle.
         */
        public void setMass(float mass)
        {
            //assert(mass != 0);
            this.inverseMass = (1.0f)/mass;
        }

        /**
         * Gets the mass of the particle.
         *
         * @return The current mass of the particle.
         */
        public float getMass()
        {
            if (inverseMass == 0) {
                return float.MaxValue;
            } else {
                return (1.0f)/inverseMass;
            }
        }

        /**
         * Sets the inverse mass of the particle.
         *
         * @param inverseMass The new inverse mass of the body. This
         * may be zero, for a body with infinite mass
         * (i.e. unmovable).
         *
         * @warning This invalidates internal data for the particle.
         * Either an integration function, or the calculateInternals
         * function should be called before trying to get any settings
         * from the particle.
         */
        public void setInverseMass(float inverseMass)
        {
            this.inverseMass = inverseMass;
        }

        /**
         * Gets the inverse mass of the particle.
         *
         * @return The current inverse mass of the particle.
         */
        public float getInverseMass()
        {
            return inverseMass;
        }

        /**
         * Returns true if the mass of the particle is not-infinite.
         */
        public bool hasFiniteMass()
        {
            return inverseMass >= 0.0f;
        }

        /**
         * Sets both the damping of the particle.
         */
        public void setDamping(float damping)
        {
            this.damping = damping;
        }

        /**
         * Gets the current damping value.
         */
        public float getDamping()
        {
            return damping;
        }

        /**
         * Sets the position of the particle.
         *
         * @param position The new position of the particle.
         */
        public void setPosition(Vector3 position)
        {
            this.position = position;
        }

        /**
         * Sets the position of the particle by component.
         *
         * @param x The x coordinate of the new position of the rigid
         * body.
         *
         * @param y The y coordinate of the new position of the rigid
         * body.
         *
         * @param z The z coordinate of the new position of the rigid
         * body.
         */
        public void setPosition(float x, float y, float z)
        {
            position.x = x;
            position.y = y;
            position.z = z;
        }

        /**
         * Fills the given vector with the position of the particle.
         *
         * @param position A pointer to a vector into which to write
         * the position.
         */
        public void getPosition(out Vector3 position)
        {
            position = this.position;
        }

        /**
         * Gets the position of the particle.
         *
         * @return The position of the particle.
         */
        public Vector3 getPosition()
        {
            return position;
        }

        /**
         * Sets the velocity of the particle.
         *
         * @param velocity The new velocity of the particle.
         */
        public void setVelocity(Vector3 velocity)
        {
            this.velocity = velocity;
        }

        /**
         * Sets the velocity of the particle by component.
         *
         * @param x The x coordinate of the new velocity of the rigid
         * body.
         *
         * @param y The y coordinate of the new velocity of the rigid
         * body.
         *
         * @param z The z coordinate of the new velocity of the rigid
         * body.
         */
        public void setVelocity(float x, float y, float z)
        {
            velocity.x = x;
            velocity.y = y;
            velocity.z = z;
        }

        /**
         * Fills the given vector with the velocity of the particle.
         *
         * @param velocity A pointer to a vector into which to write
         * the velocity. The velocity is given in world local space.
         */
        public void getVelocity(out Vector3 velocity)
        {
            velocity = this.velocity;
        }

        /**
         * Gets the velocity of the particle.
         *
         * @return The velocity of the particle. The velocity is
         * given in world local space.
         */
        public Vector3 getVelocity()
        {
            return velocity;
        }

        /**
         * Sets the constant acceleration of the particle.
         *
         * @param acceleration The new acceleration of the particle.
         */
        public void setAcceleration(Vector3 acceleration)
        {
            this.acceleration = acceleration;
        }

        /**
         * Sets the constant acceleration of the particle by component.
         *
         * @param x The x coordinate of the new acceleration of the rigid
         * body.
         *
         * @param y The y coordinate of the new acceleration of the rigid
         * body.
         *
         * @param z The z coordinate of the new acceleration of the rigid
         * body.
         */
        public void setAcceleration(float x, float y, float z)
        {
            acceleration.x = x;
            acceleration.y = y;
            acceleration.z = z;
        }

        /**
         * Fills the given vector with the acceleration of the particle.
         *
         * @param acceleration A pointer to a vector into which to write
         * the acceleration. The acceleration is given in world local space.
         */
        public void getAcceleration(out Vector3 acceleration)
        {
            acceleration = this.acceleration;
        }

        /**
         * Gets the acceleration of the particle.
         *
         * @return The acceleration of the particle. The acceleration is
         * given in world local space.
         */
        public Vector3 getAcceleration()
        {
            return acceleration;
        }

        /*@}*/

        /**
         * @name Force Set-up Functions
         *
         * These functions set up forces to apply to the
         * particle.
         */
        /*@{*/

        /**
         * Clears the forces applied to the particle. This will be
         * called automatically after each integration step.
         */
        public void clearAccumulator()
        {
            forceAccum.clear();
        }

        /**
         * Adds the given force to the particle, to be applied at the
         * next iteration only.
         *
         * @param force The force to apply.
         */
        public void addForce(Vector3 force)
        {
            forceAccum += force;
        }
    }
}