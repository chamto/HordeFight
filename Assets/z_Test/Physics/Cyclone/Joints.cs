using System;

namespace Cyclone
{
    public class Joint : ContactGenerator
    {
    
        /**
         * Holds the two rigid bodies that are connected by this joint.
         */
        //RigidBody* body[2];
        public RigidBody[] body = new RigidBody[2];

        /**
         * Holds the relative location of the connection for each
         * body, given in local coordinates.
         */
        //Vector3 position[2];
        public Vector3[] position = new Vector3[2];

        /**
         * Holds the maximum displacement at the joint before the
         * joint is considered to be violated. This is normally a
         * small, epsilon value.  It can be larger, however, in which
         * case the joint will behave as if an inelastic cable joined
         * the bodies at their joint locations.
         */
        public float error;

        /**
         * Configures the joint in one go.
         */
        public void set(RigidBody a, Vector3 a_pos, RigidBody b, Vector3 b_pos, float error)
        {
            body[0] = a;
            body[1] = b;

            position[0] = a_pos;
            position[1] = b_pos;

            this.error = error;
        }

        /**
         * Generates the contacts required to restore the joint if it
         * has been violated.
         */
        override public uint addContact(Contact contact, uint limit)
        {
            // Calculate the position of each connection point in world coordinates
            Vector3 a_pos_world = body[0].getPointInWorldSpace(position[0]);
            Vector3 b_pos_world = body[1].getPointInWorldSpace(position[1]);

            // Calculate the length of the joint
            Vector3 a_to_b = b_pos_world - a_pos_world;
            Vector3 normal = a_to_b;
            normal.normalise();
            float length = a_to_b.magnitude();

            // Check if it is violated
            if (Math.Abs(length) > error)
            {
                contact.body[0] = body[0];
                contact.body[1] = body[1];
                contact.contactNormal = normal;
                contact.contactPoint = (a_pos_world + b_pos_world) * 0.5f;
                contact.penetration = length-error;
                contact.friction = 1.0f;
                contact.restitution = 0;
                return 1;
            }

            return 0;
        }
    }
}
