using System;

namespace Cyclone
{
    /**
     * Represents a primitive to detect collisions against.
     */
    public class CollisionPrimitive
    {

        /**
         * This class exists to help the collision detector
         * and intersection routines, so they should have
         * access to its data.
         */
        //friend IntersectionTests;
        //friend CollisionDetector;

        /**
         * The rigid body that is represented by this primitive.
         */
        public RigidBody body;

        /**
         * The offset of this primitive from the given rigid body.
         */
        public Matrix4 offset;

        /**
         * Calculates the internals for the primitive.
         */
        public void calculateInternals()
        {
            transform = body.getTransform() * offset;
        }

        /**
         * This is a convenience function to allow access to the
         * axis vectors in the transform for this primitive.
         */
        public Vector3 getAxis(uint index)
        {
            return transform.getAxisVector(index);
        }

        /**
         * Returns the resultant transform of the primitive, calculated from
         * the combined offset of the primitive and the transform
         * (orientation + position) of the rigid body to which it is
         * attached.
         */
        public Matrix4 getTransform()
        {
            return transform;
        }


        /**
         * The resultant transform of the primitive. This is
         * calculated by combining the offset of the primitive
         * with the transform of the rigid body.
         */
        public Matrix4 transform;
    }

    /**
     * Represents a rigid body that can be treated as a sphere
     * for collision detection.
     */
    public class CollisionSphere : CollisionPrimitive
    {
    
        /**
         * The radius of the sphere.
         */
        public float radius;
    }

    /**
     * The plane is not a primitive: it doesn't represent another
     * rigid body. It is used for contacts with the immovable
     * world geometry.
     */
    public class CollisionPlane
    {

        /**
         * The plane normal
         */
        public Vector3 direction;

        /**
         * The distance of the plane from the origin.
         */
        public float offset;
    }

    /**
     * Represents a rigid body that can be treated as an aligned bounding
     * box for collision detection.
     */
    public class CollisionBox : CollisionPrimitive
    {

        /**
         * Holds the half-sizes of the box along each of its local axes.
         */
        public Vector3 halfSize;
    }

    /**
     * A wrapper class that holds fast intersection tests. These
     * can be used to drive the coarse collision detection system or
     * as an early out in the full collision tests below.
     */
    public class IntersectionTests
    {


        public static bool sphereAndHalfSpace(CollisionSphere sphere, CollisionPlane plane)
        {
            // Find the distance from the origin
            float ballDistance =
                plane.direction *
                sphere.getAxis(3) -
                sphere.radius;

            // Check for the intersection
            return ballDistance <= plane.offset;
        }

        public static bool sphereAndSphere(CollisionSphere one, CollisionSphere two)
        {
            // Find the vector between the objects
            Vector3 midline = one.getAxis(3) - two.getAxis(3);

            // See if it is large enough.
            return midline.squareMagnitude() <
                (one.radius + two.radius) * (one.radius + two.radius);
        }


        public static float transformToAxis(CollisionBox box, Vector3 axis)
        {
            return
                box.halfSize.x * Math.Abs(axis * box.getAxis(0)) +
                box.halfSize.y * Math.Abs(axis * box.getAxis(1)) +
                box.halfSize.z * Math.Abs(axis * box.getAxis(2));
        }

        /**
         * This function checks if the two boxes overlap
         * along the given axis. The final parameter toCentre
         * is used to pass in the vector between the boxes centre
         * points, to avoid having to recalculate it each time.
         */
        public static bool overlapOnAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCentre)
        {
            // Project the half-size of one onto axis
            float oneProject = transformToAxis(one, axis);
            float twoProject = transformToAxis(two, axis);

            // Project this onto the axis
            float distance = (float)Math.Abs(toCentre * axis); //연산자재정의* 내적 

            // Check for overlap
            return (distance < oneProject + twoProject);
        }

        public static bool boxAndBox(CollisionBox one, CollisionBox two)
        {
            // Find the vector between the two centres
            Vector3 toCentre = two.getAxis(3) - one.getAxis(3);

            return (
                // Check on box one's axes first
                overlapOnAxis(one, two, one.getAxis(0), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(1), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(2), toCentre) &&

                // And on two's
                overlapOnAxis(one, two, two.getAxis(0), toCentre) &&
                overlapOnAxis(one, two, two.getAxis(1), toCentre) &&
                overlapOnAxis(one, two, two.getAxis(2), toCentre) &&

                // Now on the cross products
                overlapOnAxis(one, two, one.getAxis(0) % two.getAxis(0), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(0) % two.getAxis(1), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(0) % two.getAxis(2), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(1) % two.getAxis(0), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(1) % two.getAxis(1), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(1) % two.getAxis(2), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(2) % two.getAxis(0), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(2) % two.getAxis(1), toCentre) &&
                overlapOnAxis(one, two, one.getAxis(2) % two.getAxis(2), toCentre)
            );
        }

        /**
         * Does an intersection test on an arbitrarily aligned box and a
         * half-space.
         *
         * The box is given as a transform matrix, including
         * position, and a vector of half-sizes for the extend of the
         * box along each local axis.
         *
         * The half-space is given as a direction (i.e. unit) vector and the
         * offset of the limiting plane from the origin, along the given
         * direction.
         */
        public static bool boxAndHalfSpace(CollisionBox box, CollisionPlane plane)
        {
            // Work out the projected radius of the box onto the plane direction
            float projectedRadius = transformToAxis(box, plane.direction);

            // Work out how far the box is from the origin
            float boxDistance =
                plane.direction *
                box.getAxis(3) -
                projectedRadius;

            // Check for the intersection
            return boxDistance <= plane.offset;
        }
    }


    /**
     * A helper structure that contains information for the detector to use
     * in building its contact data.
     */
    public class CollisionData
    {
        /**
         * Holds the base of the collision data: the first contact
         * in the array. This is used so that the contact pointer (below)
         * can be incremented each time a contact is detected, while
         * this pointer points to the first contact found.
         */
        public Contact[] contactArray;

        /** Holds the contact array to write into. */
        public Contact contacts;

        /** Holds the maximum number of contacts the array can take. */
        public int contactsLeft;

        /** Holds the number of contacts found so far. */
        public uint contactCount;

        /** Holds the friction value to write into any collisions. */
        public float friction;

        /** Holds the restitution value to write into any collisions. */
        public float restitution;

        /**
         * Holds the collision tolerance, even uncolliding objects this
         * close should have collisions generated.
         */
        public float tolerance;

        /**
         * Checks if there are more contacts available in the contact
         * data.
         */
        public bool hasMoreContacts()
        {
            return contactsLeft > 0;
        }

        /**
         * Resets the data so that it has no used contacts recorded.
         */
        public void reset(uint maxContacts)
        {
            contactsLeft = (int)maxContacts;
            contactCount = 0;
            contacts = contactArray[0];
        }

        /**
         * Notifies the data that the given number of contacts have
         * been added.
         */
        public void addContacts(uint count)
        {
            // Reduce the number of contacts remaining, add number used
            contactsLeft -= (int)count;
            contactCount += count;

            // Move the array forward
            //contacts += count;
            contacts = contactArray[contactCount];
        }
    }

    /**
     * A wrapper class that holds the fine grained collision detection
     * routines.
     *
     * Each of the functions has the same format: it takes the details
     * of two objects, and a pointer to a contact array to fill. It
     * returns the number of contacts it wrote into the array.
     */
    public class CollisionDetector
    {

        public static uint sphereAndHalfSpace(CollisionSphere sphere, CollisionPlane plane, CollisionData data)
        {
            // Make sure we have contacts
            if (data.contactsLeft <= 0) return 0;

            // Cache the sphere position
            Vector3 position = sphere.getAxis(3);

                // Find the distance from the plane
            float ballDistance =
                    plane.direction * position -
                    sphere.radius - plane.offset;

            if (ballDistance >= 0) return 0;

            // Create the contact - it has a normal in the plane direction.
            Contact contact = data.contacts;
            contact.contactNormal = plane.direction;
            contact.penetration = -ballDistance;
            contact.contactPoint = position - plane.direction* (ballDistance + sphere.radius);
            contact.setBodyData(sphere.body, null, data.friction, data.restitution);

            data.addContacts(1);

            return 1;
        }

        public static uint sphereAndTruePlane(CollisionSphere sphere, CollisionPlane plane, CollisionData data)
        {
            // Make sure we have contacts
            if (data.contactsLeft <= 0) return 0;

            // Cache the sphere position
            Vector3 position = sphere.getAxis(3);

            // Find the distance from the plane
            float centreDistance = plane.direction * position - plane.offset;

            // Check if we're within radius
            if (centreDistance* centreDistance > sphere.radius* sphere.radius)
            {
                return 0;
            }

            // Check which side of the plane we're on
            Vector3 normal = plane.direction;
            float penetration = -centreDistance;
            if (centreDistance< 0)
            {
                normal *= -1;
                penetration = -penetration;
            }
            penetration += sphere.radius;

            // Create the contact - it has a normal in the plane direction.
            Contact contact = data.contacts;
            contact.contactNormal = normal;
            contact.penetration = penetration;
            contact.contactPoint = position - plane.direction* centreDistance;
            contact.setBodyData(sphere.body, null, data.friction, data.restitution);

            data.addContacts(1);

            return 1;
        }

        public static uint sphereAndSphere(CollisionSphere one, CollisionSphere two, CollisionData data)
        {
            // Make sure we have contacts
            if (data.contactsLeft <= 0) return 0;

            // Cache the sphere positions
            Vector3 positionOne = one.getAxis(3);
            Vector3 positionTwo = two.getAxis(3);

            // Find the vector between the objects
            Vector3 midline = positionOne - positionTwo;
            float size = midline.magnitude();

            // See if it is large enough.
            if (size <= 0.0f || size >= one.radius + two.radius)
            {
                return 0;
            }

            // We manually create the normal, because we have the
            // size to hand.
            Vector3 normal = midline * (1.0f / size);

            Contact contact = data.contacts;
            contact.contactNormal = normal;
            contact.contactPoint = positionOne + midline * 0.5f;
            contact.penetration = (one.radius + two.radius - size);
            contact.setBodyData(one.body, two.body, data.friction, data.restitution);

            data.addContacts(1);

            return 1;
        }

        static float[,] mults = new float[8,3]{{1,1,1},{-1,1,1},{1,-1,1},{-1,-1,1},
                                   {1,1,-1},{-1,1,-1},{1,-1,-1},{-1,-1,-1}};
        /**
         * Does a collision test on a collision box and a plane representing
         * a half-space (i.e. the normal of the plane
         * points out of the half-space).
         */
        public static uint boxAndHalfSpace(CollisionBox box, CollisionPlane plane, CollisionData data)
        {
            // Make sure we have contacts
            if (data.contactsLeft <= 0) return 0;

            // Check for intersection
            if (!IntersectionTests.boxAndHalfSpace(box, plane))
            {
                return 0;
            }

            // We have an intersection, so find the intersection points. We can make
            // do with only checking vertices. If the box is resting on a plane
            // or on an edge, it will be reported as four or two contact points.

            // Go through each combination of + and - for each half-size
            //static real mults[8][3] = {{1,1,1},{-1,1,1},{1,-1,1},{-1,-1,1},
            //{1,1,-1},{-1,1,-1},{1,-1,-1},{-1,-1,-1}};

            Contact contact = data.contacts;
            uint contactsUsed = 0;
            for (uint i = 0; i< 8; i++) 
            {

                // Calculate the position of each vertex
                Vector3 vertexPos = new Vector3(mults[i,0], mults[i,1], mults[i,2]);
                vertexPos.componentProductUpdate(box.halfSize);
                vertexPos = box.transform.transform(vertexPos);

                // Calculate the distance from the plane
                float vertexDistance = vertexPos * plane.direction;

                // Compare this to the plane's distance
                if (vertexDistance <= plane.offset)
                {
                    // Create the contact data.

                    // The contact point is halfway between the vertex and the
                    // plane - we multiply the direction by half the separation
                    // distance and add the vertex location.
                    contact.contactPoint = plane.direction;
                    contact.contactPoint *= (vertexDistance-plane.offset);
                    contact.contactPoint = vertexPos;
                    contact.contactNormal = plane.direction;
                    contact.penetration = plane.offset - vertexDistance;

                    // Write the appropriate data
                    contact.setBodyData(box.body, null, data.friction, data.restitution);

                    // Move onto the next contact
                    //contact++;
                    contactsUsed++;
                    contact = data.contactArray[data.contactCount + contactsUsed];
                    if (contactsUsed == data.contactsLeft) return contactsUsed;
                }
            }

            data.addContacts(contactsUsed);
            return contactsUsed;
        }

        public static float transformToAxis(CollisionBox box, Vector3 axis)
        {
            return
                box.halfSize.x * Math.Abs(axis * box.getAxis(0)) +
                box.halfSize.y * Math.Abs(axis * box.getAxis(1)) +
                box.halfSize.z * Math.Abs(axis * box.getAxis(2));
        }

        /*
         * This function checks if the two boxes overlap
         * along the given axis, returning the ammount of overlap.
         * The final parameter toCentre
         * is used to pass in the vector between the boxes centre
         * points, to avoid having to recalculate it each time.
         */
        static float penetrationOnAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCentre)
        {
            // Project the half-size of one onto axis
            float oneProject = transformToAxis(one, axis);
            float twoProject = transformToAxis(two, axis);

            // Project this onto the axis
            float distance = Math.Abs(toCentre * axis);

            // Return the overlap (i.e. positive indicates
            // overlap, negative indicates separation).
            return oneProject + twoProject - distance;
        }


        static bool tryAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCentre, uint index,
        // These values may be updated
        ref float smallestPenetration, ref uint smallestCase)
        {
            // Make sure we have a normalized axis, and don't check almost parallel axes
            if (axis.squareMagnitude() < 0.0001) return true;
            axis.normalise();

            float penetration = penetrationOnAxis(one, two, axis, toCentre);

            if (penetration < 0) return false;
            if (penetration < smallestPenetration)
            {
                smallestPenetration = penetration;
                smallestCase = index;
            }
            return true;
        }

        static void fillPointFaceBoxBox(CollisionBox one, CollisionBox two, Vector3 toCentre, CollisionData data, uint best, float pen)
        {
            // This method is called when we know that a vertex from
            // box two is in contact with box one.

            Contact contact = data.contacts;

            // We know which axis the collision is on (i.e. best),
            // but we need to work out which of the two faces on
            // this axis.
            Vector3 normal = one.getAxis(best);
            if (one.getAxis(best) * toCentre > 0)
            {
                normal = normal * -1.0f;
            }

            // Work out which vertex of box two we're colliding with.
            // Using toCentre doesn't work!
            Vector3 vertex = two.halfSize;
            if (two.getAxis(0) * normal < 0) vertex.x = -vertex.x;
            if (two.getAxis(1) * normal < 0) vertex.y = -vertex.y;
            if (two.getAxis(2) * normal < 0) vertex.z = -vertex.z;

            // Create the contact data
            contact.contactNormal = normal;
            contact.penetration = pen;
            contact.contactPoint = two.getTransform() * vertex;
            contact.setBodyData(one.body, two.body, data.friction, data.restitution);
        }

        static Vector3 contactPoint(Vector3 pOne, Vector3 dOne, float oneSize, Vector3 pTwo, Vector3 dTwo, float twoSize,
        // If this is true, and the contact point is outside
        // the edge (in the case of an edge-face contact) then
        // we use one's midpoint, otherwise we use two's.
        bool useOne)
        {
            Vector3 toSt, cOne, cTwo;
            float dpStaOne, dpStaTwo, dpOneTwo, smOne, smTwo;
            float denom, mua, mub;

            smOne = dOne.squareMagnitude();
            smTwo = dTwo.squareMagnitude();
            dpOneTwo = dTwo * dOne;

            toSt = pOne - pTwo;
            dpStaOne = dOne * toSt;
            dpStaTwo = dTwo * toSt;

            denom = smOne * smTwo - dpOneTwo * dpOneTwo;

            // Zero denominator indicates parrallel lines
            if (Math.Abs(denom) < 0.0001f)
            {
                return useOne ? pOne : pTwo;
            }

            mua = (dpOneTwo * dpStaTwo - smTwo * dpStaOne) / denom;
            mub = (smOne * dpStaTwo - dpOneTwo * dpStaOne) / denom;

            // If either of the edges has the nearest point out
            // of bounds, then the edges aren't crossed, we have
            // an edge-face contact. Our point is on the edge, which
            // we know from the useOne parameter.
            if (mua > oneSize ||
                mua < -oneSize ||
                mub > twoSize ||
                mub < -twoSize)
            {
                return useOne ? pOne : pTwo;
            }
            else
            {
                cOne = pOne + dOne * mua;
                cTwo = pTwo + dTwo * mub;

                return cOne * 0.5f + cTwo * 0.5f;
            }
        }


        public static uint boxAndBox(CollisionBox one, CollisionBox two, CollisionData data)
        {
            //if (!IntersectionTests::boxAndBox(one, two)) return 0;

            // Find the vector between the two centres
            Vector3 toCentre = two.getAxis(3) - one.getAxis(3);

            // We start assuming there is no contact
            float pen = float.MaxValue;
            uint best = 0xffffff;

            // Now we check each axes, returning if it gives us
            // a separating axis, and keeping track of the axis with
            // the smallest penetration otherwise.
            if (!tryAxis(one, two, one.getAxis(0), toCentre, 0, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(1), toCentre, 1, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(2), toCentre, 2, ref pen, ref best)) return 0;

            if (!tryAxis(one, two, one.getAxis(0), toCentre, 3, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(1), toCentre, 4, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(2), toCentre, 5, ref pen, ref best)) return 0;

            // Store the best axis-major, in case we run into almost
            // parallel edge collisions later
            uint bestSingleAxis = best;

            if (!tryAxis(one, two, (one.getAxis(0) % two.getAxis(0)), toCentre, 6, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(0) % two.getAxis(1)), toCentre, 7, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(0) % two.getAxis(2)), toCentre, 8, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(1) % two.getAxis(0)), toCentre, 9, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(1) % two.getAxis(1)), toCentre, 10, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(1) % two.getAxis(2)), toCentre, 11, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(2) % two.getAxis(0)), toCentre, 12, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(2) % two.getAxis(1)), toCentre, 13, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, (one.getAxis(2) % two.getAxis(2)), toCentre, 14, ref pen, ref best)) return 0;


            // Make sure we've got a result.
            //assert(best != 0xffffff);

            // We now know there's a collision, and we know which
            // of the axes gave the smallest penetration. We now
            // can deal with it in different ways depending on
            // the case.
            if (best < 3)
            {
                // We've got a vertex of box two on a face of box one.
                fillPointFaceBoxBox(one, two, toCentre, data, best, pen);
                data.addContacts(1);
                return 1;
            }
            else if (best < 6)
            {
                // We've got a vertex of box one on a face of box two.
                // We use the same algorithm as above, but swap around
                // one and two (and therefore also the vector between their
                // centres).
                fillPointFaceBoxBox(two, one, toCentre * -1.0f, data, best - 3, pen);
                data.addContacts(1);
                return 1;
            }
            else
            {
                // We've got an edge-edge contact. Find out which axes
                best -= 6;
                uint oneAxisIndex = best / 3;
                uint twoAxisIndex = best % 3;
                Vector3 oneAxis = one.getAxis(oneAxisIndex);
                Vector3 twoAxis = two.getAxis(twoAxisIndex);
                Vector3 axis = oneAxis % twoAxis;
                axis.normalise();

                // The axis should point from box one to box two.
                if (axis * toCentre > 0) axis = axis * -1.0f;

                // We have the axes, but not the edges: each axis has 4 edges parallel
                // to it, we need to find which of the 4 for each object. We do
                // that by finding the point in the centre of the edge. We know
                // its component in the direction of the box's collision axis is zero
                // (its a mid-point) and we determine which of the extremes in each
                // of the other axes is closest.
                Vector3 ptOnOneEdge = one.halfSize;
                Vector3 ptOnTwoEdge = two.halfSize;
                for (uint i = 0; i < 3; i++)
                {
                    if (i == oneAxisIndex) ptOnOneEdge[i] = 0;
                    else if (one.getAxis(i) * axis > 0) ptOnOneEdge[i] = -ptOnOneEdge[i];

                    if (i == twoAxisIndex) ptOnTwoEdge[i] = 0;
                    else if (two.getAxis(i) * axis < 0) ptOnTwoEdge[i] = -ptOnTwoEdge[i];
                }

                // Move them into world coordinates (they are already oriented
                // correctly, since they have been derived from the axes).
                ptOnOneEdge = one.transform * ptOnOneEdge;
                ptOnTwoEdge = two.transform * ptOnTwoEdge;

                // So we have a point and a direction for the colliding edges.
                // We need to find out point of closest approach of the two
                // line-segments.
                Vector3 vertex = contactPoint(
                    ptOnOneEdge, oneAxis, one.halfSize[oneAxisIndex],
                    ptOnTwoEdge, twoAxis, two.halfSize[twoAxisIndex],
                    bestSingleAxis > 2
                    );

                // We can fill the contact.
                Contact contact = data.contacts;

                contact.penetration = pen;
                contact.contactNormal = axis;
                contact.contactPoint = vertex;
                contact.setBodyData(one.body, two.body, data.friction, data.restitution);
                data.addContacts(1);
                return 1;
            }
            return 0;
        }

        public static uint boxAndPoint(CollisionBox box, Vector3 point, CollisionData data)
        {
            // Transform the point into box coordinates
            Vector3 relPt = box.transform.transformInverse(point);

            Vector3 normal;

            // Check each axis, looking for the axis on which the
            // penetration is least deep.
            float min_depth = box.halfSize.x - Math.Abs(relPt.x);
            if (min_depth < 0) return 0;
            normal = box.getAxis(0) * ((relPt.x < 0) ? -1 : 1);

            float depth = box.halfSize.y - Math.Abs(relPt.y);
            if (depth < 0) return 0;
            else if (depth < min_depth)
            {
                min_depth = depth;
                normal = box.getAxis(1) * ((relPt.y < 0) ? -1 : 1);
            }

            depth = box.halfSize.z - Math.Abs(relPt.z);
            if (depth < 0) return 0;
            else if (depth < min_depth)
            {
                min_depth = depth;
                normal = box.getAxis(2) * ((relPt.z < 0) ? -1 : 1);
            }

            // Compile the contact
            Contact contact = data.contacts;
            contact.contactNormal = normal;
            contact.contactPoint = point;
            contact.penetration = min_depth;

            // Note that we don't know what rigid body the point
            // belongs to, so we just use NULL. Where this is called
            // this value can be left, or filled in.
            contact.setBodyData(box.body, null, data.friction, data.restitution);

            data.addContacts(1);
            return 1;
        }

        public static uint boxAndSphere(CollisionBox box, CollisionSphere sphere, CollisionData data)
        {
            // Transform the centre of the sphere into box coordinates
            Vector3 centre = sphere.getAxis(3);
            Vector3 relCentre = box.transform.transformInverse(centre);

            // Early out check to see if we can exclude the contact
            if (Math.Abs(relCentre.x) - sphere.radius > box.halfSize.x ||
                Math.Abs(relCentre.y) - sphere.radius > box.halfSize.y ||
                Math.Abs(relCentre.z) - sphere.radius > box.halfSize.z)
            {
                return 0;
            }

            Vector3 closestPt = Vector3.ZERO;
            float dist;

            // Clamp each coordinate to the box.
            dist = relCentre.x;
            if (dist > box.halfSize.x) dist = box.halfSize.x;
            if (dist < -box.halfSize.x) dist = -box.halfSize.x;
            closestPt.x = dist;

            dist = relCentre.y;
            if (dist > box.halfSize.y) dist = box.halfSize.y;
            if (dist < -box.halfSize.y) dist = -box.halfSize.y;
            closestPt.y = dist;

            dist = relCentre.z;
            if (dist > box.halfSize.z) dist = box.halfSize.z;
            if (dist < -box.halfSize.z) dist = -box.halfSize.z;
            closestPt.z = dist;

            // Check we're in contact
            dist = (closestPt - relCentre).squareMagnitude();
            if (dist > sphere.radius * sphere.radius) return 0;

            // Compile the contact
            Vector3 closestPtWorld = box.transform.transform(closestPt);

            Contact contact = data.contacts;
            contact.contactNormal = (closestPtWorld - centre);
            contact.contactNormal.normalise();
            contact.contactPoint = closestPtWorld;
            contact.penetration = sphere.radius - (float)Math.Sqrt(dist);
            contact.setBodyData(box.body, sphere.body, data.friction, data.restitution);

            data.addContacts(1);
            return 1;
        }
    }
}
