namespace Cyclone
{
    /**
     * Links connect two particles together, generating a contact if
     * they violate the constraints of their link. It is used as a
     * base class for cables and rods, and could be used as a base
     * class for springs with a limit to their extension..
     */
    public class ParticleLink : ParticleContactGenerator
    {

        /**
         * Holds the pair of particles that are connected by this link.
         */
        //Particle* particle[2];
        public Particle[] particle = new Particle[2];


        /**
         * Returns the current length of the link.
         */
        protected float currentLength()
        {
            Vector3 relativePos = particle[0].getPosition() -
                                  particle[1].getPosition();
            return relativePos.magnitude();
        }


        /**
         * Geneates the contacts to keep this link from being
         * violated. This class can only ever generate a single
         * contact, so the pointer can be a pointer to a single
         * element, the limit parameter is assumed to be at least one
         * (zero isn't valid) and the return value is either 0, if the
         * cable wasn't over-extended, or one if a contact was needed.
         *
         * NB: This method is declared in the same way (as pure
         * virtual) in the parent class, but is replicated here for
         * documentation purposes.
         */
        //public override uint addContact(ParticleContact contact, uint limit) {}
    }


    /**
     * Cables link a pair of particles, generating a contact if they
     * stray too far apart.
     */
    public class ParticleCable : ParticleLink
    {
    
        /**
         * Holds the maximum length of the cable.
         */
        public float maxLength;

        /**
         * Holds the restitution (bounciness) of the cable.
         */
        public float restitution;


        /**
         * Fills the given contact structure with the contact needed
         * to keep the cable from over-extending.
         */
        public override uint addContact(ParticleContact contact,
                                    uint limit)
        {
            // Find the length of the cable
            float length = currentLength();

            // Check if we're over-extended
            if (length < maxLength)
            {
                return 0;
            }

            // Otherwise return the contact
            contact.particle[0] = particle[0];
            contact.particle[1] = particle[1];

            // Calculate the normal
            Vector3 normal = particle[1].getPosition() - particle[0].getPosition();
            normal.normalise();
            contact.contactNormal = normal;

            contact.penetration = length - maxLength;
            contact.restitution = restitution;

            return 1;
        }
    }


    /**
     * Rods link a pair of particles, generating a contact if they
     * stray too far apart or too close.
     */
    public class ParticleRod : ParticleLink
    {

        /**
         * Holds the length of the rod.
         */
        public float length;


        /**
         * Fills the given contact structure with the contact needed
         * to keep the rod from extending or compressing.
         */
        public override uint addContact(ParticleContact contact,
                                  uint limit)
        {
            // Find the length of the rod
            float currentLen = currentLength();

            // Check if we're over-extended
            if (currentLen == length)
            {
                return 0;
            }

            // Otherwise return the contact
            contact.particle[0] = particle[0];
            contact.particle[1] = particle[1];

            // Calculate the normal
            Vector3 normal = particle[1].getPosition() - particle[0].getPosition();
                normal.normalise();

            // The contact normal depends on whether we're extending or compressing
            if (currentLen > length) {
                contact.contactNormal = normal;
                contact.penetration = currentLen - length;
            } else {
                contact.contactNormal = normal* -1;
                contact.penetration = length - currentLen;
            }

            // Always use zero restitution (no bounciness)
            contact.restitution = 0;

            return 1;
        }
    }

    /**
    * Constraints are just like links, except they connect a particle to
    * an immovable anchor point.
    */
    public class ParticleConstraint : ParticleContactGenerator
    {

        /**
        * Holds the particles connected by this constraint.
        */
        public Particle particle;

        /**
         * The point to which the particle is anchored.
         */
        public Vector3 anchor;


        /**
        * Returns the current length of the link.
        */
        protected float currentLength()
        {
            Vector3 relativePos = particle.getPosition() - anchor;
            return relativePos.magnitude();
        }


        /**
        * Geneates the contacts to keep this link from being
        * violated. This class can only ever generate a single
        * contact, so the pointer can be a pointer to a single
        * element, the limit parameter is assumed to be at least one
        * (zero isn't valid) and the return value is either 0, if the
        * cable wasn't over-extended, or one if a contact was needed.
        *
        * NB: This method is declared in the same way (as pure
        * virtual) in the parent class, but is replicated here for
        * documentation purposes.
        */
        //public override uint addContact(ParticleContact contact, uint limit) const = 0;
    }

    /**
    * Cables link a particle to an anchor point, generating a contact if they
    * stray too far apart.
    */
    public class ParticleCableConstraint : ParticleConstraint
    {

        /**
        * Holds the maximum length of the cable.
        */
        public float maxLength;

        /**
        * Holds the restitution (bounciness) of the cable.
        */
        public float restitution;


        /**
        * Fills the given contact structure with the contact needed
        * to keep the cable from over-extending.
        */
        public override uint addContact(ParticleContact contact,
                                   uint limit)
        {
            // Find the length of the cable
            float length = currentLength();

            // Check if we're over-extended
            if (length<maxLength)
            {
                return 0;
            }

            // Otherwise return the contact
            contact.particle[0] = particle;
            contact.particle[1] = null;

            // Calculate the normal
            Vector3 normal = anchor - particle.getPosition();
            normal.normalise();
            contact.contactNormal = normal;

            contact.penetration = length-maxLength;
            contact.restitution = restitution;

            return 1;
        }
    }

    /**
    * Rods link a particle to an anchor point, generating a contact if they
    * stray too far apart or too close.
    */
    public class ParticleRodConstraint : ParticleConstraint
    {

        /**
        * Holds the length of the rod.
        */
        public float length;


        /**
        * Fills the given contact structure with the contact needed
        * to keep the rod from extending or compressing.
        */
        public override uint addContact(ParticleContact contact,
                                 uint limit)
        {
            // Find the length of the rod
            float currentLen = currentLength();

            // Check if we're over-extended
            if (currentLen == length)
            {
                return 0;
            }

            // Otherwise return the contact
            contact.particle[0] = particle;
            contact.particle[1] = null;

            // Calculate the normal
            Vector3 normal = anchor - particle.getPosition();
            normal.normalise();

            // The contact normal depends on whether we're extending or compressing
            if (currentLen > length) {
                contact.contactNormal = normal;
                contact.penetration = currentLen - length;
            } else {
                contact.contactNormal = normal* -1;
                contact.penetration = length - currentLen;
            }

            // Always use zero restitution (no bounciness)
            contact.restitution = 0;

            return 1;
        }
    }
}
