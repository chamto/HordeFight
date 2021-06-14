using System.Collections.Generic;


namespace Cyclone
{
    /**
     * Keeps track of a set of particles, and provides the means to
     * update them all.
     */

    public class Particles : List<Particle> { }
    public class ContactGenerators : List<ParticleContactGenerator> { }

    public class ParticleWorld
    {

        //typedef std::vector<Particle*> Particles;
        //typedef std::vector<ParticleContactGenerator*> ContactGenerators;

        
        /**
         * Holds the particles
         */
        protected Particles particles = new Particles();

        /**
         * True if the world should calculate the number of iterations
         * to give the contact resolver at each frame.
         */
        protected bool calculateIterations;

        /**
         * Holds the force generators for the particles in this world.
         */
        protected ParticleForceRegistry registry = new ParticleForceRegistry();

        /**
         * Holds the resolver for contacts.
         */
        protected ParticleContactResolver resolver;

        /**
         * Contact generators.
         */
        protected ContactGenerators contactGenerators = new ContactGenerators();

        /**
         * Holds the list of contacts.
         */
        protected ParticleContact[] contacts;

        /**
         * Holds the maximum number of contacts allowed (i.e. the
         * size of the contacts array).
         */
        protected uint maxContacts;


        /**
         * Creates a new particle simulator that can handle up to the
         * given number of contacts per frame. You can also optionally
         * give a number of contact-resolution iterations to use. If you
         * don't give a number of iterations, then twice the number of
         * contacts will be used.
         */
        public ParticleWorld(uint maxContacts, uint iterations = 0 )
        {
            this.resolver = new ParticleContactResolver(iterations);
            this.maxContacts = maxContacts;
            contacts = new ParticleContact[maxContacts];
            for(int i=0;i< maxContacts;i++)
            {
                contacts[i] = new ParticleContact();
            }
            calculateIterations = (iterations == 0);

        }


        /**
         * Calls each of the registered contact generators to report
         * their contacts. Returns the number of generated contacts.
         */
        //public uint generateContacts()
        //{
        //    uint limit = maxContacts;
        //    //ParticleContact* nextContact = contacts;
        //    ParticleContact nextContact = contacts[0];
        //    uint count = 0;
        //    foreach (ParticleContactGenerator g in contactGenerators)
        //    {
        //        uint used = g.addContact(nextContact, limit);
        //        limit -= used;
        //        //nextContact += used;
        //        nextContact = contacts[count + used];
        //        count += used;

        //        // We've run out of contacts to fill. This means we're missing
        //        // contacts.
        //        if (limit <= 0) break;
        //    }


        //    // Return the number of contacts used.
        //    return maxContacts - limit;
        //}

        public uint generateContacts()
        {
            uint limit = maxContacts;
            //ParticleContact* nextContact = contacts;
            ParticleContact nextContact = contacts[0];
            uint count = 0;
            uint used = 0;
            foreach (ParticleContactGenerator g in contactGenerators)
            {
                if(true == g._isContactList)
                    used = g.addContactList(contacts, (int)(count + used), limit);
                else
                    used = g.addContact(nextContact, limit);
                limit -= used;
                //nextContact += used;
                nextContact = contacts[count + used];
                count += used;

                // We've run out of contacts to fill. This means we're missing
                // contacts.
                if (limit <= 0) break;
            }


            // Return the number of contacts used.
            return maxContacts - limit;
        }

        /**
         * Integrates all the particles in this world forward in time
         * by the given duration.
         */
        public void integrate(float duration)
        {
            foreach(Particle p in particles)
            {
                // Remove all forces from the accumulator
                p.integrate(duration);
            }

            //for (Particles::iterator p = particles.begin();
            //    p != particles.end();
            //    p++)
            //{
            //    // Remove all forces from the accumulator
            //    (*p)->integrate(duration);
            //}
        }

        /**
         * Processes all the physics for the particle world.
         */
        public void runPhysics(float duration)
        {
            // First apply the force generators
            registry.updateForces(duration);

            // Then integrate the objects
            integrate(duration);

            // Generate contacts
            uint usedContacts = generateContacts();

            // And process them
            if (0 != usedContacts)
            {
                if (calculateIterations) resolver.setIterations(usedContacts * 2);
                resolver.resolveContacts(contacts, usedContacts, duration);
            }
        }

        //public void runPhysics_MassAggregate(float duration)
        //{
        //    // First apply the force generators
        //    registry.updateForces(duration);

        //    // Then integrate the objects
        //    integrate(duration);

        //    // Generate contacts
        //    uint usedContacts = generateContacts_MassAggregate();

        //    // And process them
        //    if (0 != usedContacts)
        //    {
        //        if (calculateIterations) resolver.setIterations(usedContacts * 2);
        //        resolver.resolveContacts(contacts, usedContacts, duration);
        //    }
        //}

        /**
         * Initializes the world for a simulation frame. This clears
         * the force accumulators for particles in the world. After
         * calling this, the particles can have their forces for this
         * frame added.
         */
        public void startFrame()
        {
            foreach (Particle p in particles)
            {
                // Remove all forces from the accumulator
                p.clearAccumulator();
            }

            //for (Particles::iterator p = particles.begin();
            //    p != particles.end();
            //    p++)
            //{
            //    // Remove all forces from the accumulator
            //    (*p)->clearAccumulator();
            //}
        }

        /**
         *  Returns the list of particles.
         */
        public Particles getParticles()
        {
            return particles;
        }

        /**
         * Returns the list of contact generators.
         */
        public ContactGenerators getContactGenerators()
        {
            return contactGenerators;
        }

        /**
         * Returns the force registry.
         */
        public ParticleForceRegistry getForceRegistry()
        {
            return registry;
        }
    }

    /**
      * A contact generator that takes an STL vector of particle pointers and
     * collides them against the ground.
     */
    public class GroundContacts : ParticleContactGenerator
    {
        Particles particles = new Particles();


        public void init(Particles particles)
        {
            _isContactList = true; //addContactList 를 재정의 한것을 구별하기 위해 추가  - chamto 
            this.particles = particles;
        }

        public override uint addContactList(ParticleContact[] contactList, int cIdx,
                                    uint limit)
        {
            uint count = 0;
            ParticleContact contact = contactList[cIdx];
            foreach (Particle p in particles)
            {

                float y = p.getPosition().y;
                if (y < 0.0f)
                {
                    //DebugWide.LogBlue(cIdx + "  " + count + "  " + limit);
                    contact.contactNormal = Vector3.UP;
                    contact.particle[0] = p;
                    contact.particle[1] = null;
                    contact.penetration = -y;
                    contact.restitution = 0.2f;
                    //contact.restitution = 0.9f;

                    //contact++;
                    count++;

                    contact = contactList[cIdx + count];
                }

                if (count >= limit) return count;
            }

            //for (cyclone::ParticleWorld::Particles::iterator p = particles->begin();
            //    p != particles->end();
            //    p++)
            //{
            //    cyclone::real y = (*p)->getPosition().y;
            //    if (y< 0.0f)
            //    {
            //        contact->contactNormal = cyclone::Vector3::UP;
            //        contact->particle[0] = *p;
            //        contact->particle[1] = NULL;
            //        contact->penetration = -y;
            //        contact->restitution = 0.2f;
            //        contact++;
            //        count++;
            //    }

            //    if (count >= limit) return count;
            //}
            return count;
        }
    }

}
