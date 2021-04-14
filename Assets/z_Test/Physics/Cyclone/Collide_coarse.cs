using System;

namespace Cyclone
{
    /**
     * Represents a bounding sphere that can be tested for overlap.
     */
    public struct BoundingSphere
    {
        public Vector3 centre;
        public float radius;


        /**
         * Creates a new bounding sphere at the given centre and radius.
         */
        public BoundingSphere(Vector3 centre, float radius)
        {
            this.centre = centre;
            this.radius = radius;
        }

        /**
         * Creates a bounding sphere to enclose the two given bounding
         * spheres.
         */
         //두 원을 포함하는 큰원을 만든다 - 20210413 실험노트 참조 
        public BoundingSphere(BoundingSphere one, BoundingSphere two)
        {
            Vector3 centreOffset = two.centre - one.centre;
            float distance = centreOffset.squareMagnitude();
            float radiusDiff = two.radius - one.radius;

            // Check if the larger sphere encloses the small one
            //한원이 다른원에 완전히 포함된 경우 
            if (radiusDiff * radiusDiff >= distance)
            {
                if (one.radius > two.radius)
                {
                    centre = one.centre;
                    radius = one.radius;
                }
                else
                {
                    centre = two.centre;
                    radius = two.radius;
                }
            }

            // Otherwise we need to work with partially
            // overlapping spheres
            //두 원이 겹쳐있거나 떨어져 있는 경우 
            else
            {
                distance = (float)Math.Sqrt(distance);
                radius = (distance + one.radius + two.radius) * (0.5f);

                // The new centre is based on one's centre, moved towards
                // two's centre by an ammount proportional to the spheres'
                // radii.
                centre = one.centre;
                if (distance > 0)
                {
                    centre += centreOffset * ((radius - one.radius) / distance);
                }
            }

        }

        /**
         * Checks if the bounding sphere overlaps with the other given
         * bounding sphere.
         */
        public bool overlaps(BoundingSphere other)
        {
            float distanceSquared = (centre - other.centre).squareMagnitude();
            return distanceSquared<(radius+other.radius)*(radius+other.radius);
        }

        /**
         * Reports how much this bounding sphere would have to grow
         * by to incorporate the given bounding sphere. Note that this
         * calculation returns a value not in any particular units (i.e.
         * its not a volume growth). In fact the best implementation
         * takes into account the growth in surface area (after the
         * Goldsmith-Salmon algorithm for tree construction).
         */
        public float getGrowth(BoundingSphere other)
        {
            BoundingSphere newSphere = new BoundingSphere (this, other);

            // We return a value proportional to the change in surface
            // area of the sphere.
            return newSphere.radius* newSphere.radius - radius* radius;
        }

        /**
         * Returns the volume of this bounding volume. This is used
         * to calculate how to recurse into the bounding volume tree.
         * For a bounding sphere it is a simple calculation.
         */
        public float getSize()
        {
            return (1.333333f) * (float)Math.PI* radius * radius* radius;
        }
    }

    /**
     * Stores a potential contact to check later.
     */
    public struct PotentialContact
    {
        /**
         * Holds the bodies that might be in contact.
         */
        //public RigidBody* body[2];
        public RigidBody[] body;

        public void Init()
        {
            body = new RigidBody[2];
        }
        public void Reset()
        {
            body[0] = null;
            body[1] = null;
        }
    }

    /**
     * A base class for nodes in a bounding volume hierarchy.
     *
     * This class uses a binary tree to store the bounding
     * volumes.
     */

    public class BVHNode
    {
    
        /**
         * Holds the child nodes of this node.
         */
        public BVHNode[] children = new Cyclone.BVHNode[2];

        /**
         * Holds a single bounding volume encompassing all the
         * descendents of this node.
         */
        public BoundingSphere volume;

        /**
         * Holds the rigid body at this node of the hierarchy.
         * Only leaf nodes can have a rigid body defined (see isLeaf).
         * Note that it is possible to rewrite the algorithms in this
         * class to handle objects at all levels of the hierarchy,
         * but the code provided ignores this vector unless firstChild
         * is NULL.
         */
        public RigidBody body;

        // ... other BVHNode code as before ...

        /**
         * Holds the node immediately above us in the tree.
         */
        public BVHNode parent;

        /**
         * Creates a new node in the hierarchy with the given parameters.
         */
        public BVHNode(BVHNode parent, BoundingSphere volume,
            RigidBody body = null)

        {
            this.parent = parent;
            this.volume = volume;
            this.body = body;
            children[0] = children[1] = null;
        }

        /**
         * Checks if this node is at the bottom of the hierarchy.
         */
        public bool isLeaf()
        {
            return (body != null);
        }

        /**
         * Checks the potential contacts from this node downwards in
         * the hierarchy, writing them to the given array (up to the
         * given limit). Returns the number of potential contacts it
         * found.
         */
         //한노드에 대해서만 처리함 , 전체 노드를 재귀로 처리한다고 오해하고 사용할 수 있어 외부에서 사용못하게 막음 
        protected uint getPotentialContacts(ref PotentialContact[] contacts, uint cIdx ,uint limit) 
        {
            // Early out if we don't have the room for contacts, or
            // if we're a leaf node.
            if (isLeaf() || limit == 0) return 0;

            // Get the potential contacts of one of our children with
            // the other
            return children[0].getPotentialContactsWith(
                children[1], contacts,cIdx, limit
                );
        }

        //모든 노드를 순회하여 접촉가능성 정보를 모은다 
        //ref : https://blog.naver.com/ndb796/221233560789
        //트리의 전위순회를 통해 구현 
        static public uint GetPotentialContacts(BVHNode node, ref PotentialContact[] contacts, uint cIdx , uint limit)
        {
            if (null == node) return 0;

            uint count = node.getPotentialContacts(ref contacts, cIdx, limit);

            uint n_idx = cIdx + count;
            uint n_limit = limit - count;
            count = GetPotentialContacts(node.children[0], ref contacts, n_idx, n_limit);

            n_idx = n_idx + count;
            n_limit = n_limit - count;
            count = GetPotentialContacts(node.children[1], ref contacts, n_idx, n_limit);

            return count;
        }


        /**
         * Inserts the given rigid body, with the given bounding volume,
         * into the hierarchy. This may involve the creation of
         * further bounding volume nodes.
         */
        //20210413 실험노트  - 트리성장 모습 분석
        public void insert(RigidBody newBody, BoundingSphere newVolume)
        {
            // If we are a leaf, then the only option is to spawn two
            // new children and place the new body in one.
            if (isLeaf())
            {
                // Child one is a copy of us.
                children[0] = new BVHNode(
                    this, volume, body
                    );

                // Child two holds the new body
                children [1] = new BVHNode(
         
                     this, newVolume, newBody
                     );

                // And we now loose the body (we're no longer a leaf)
                this.body = null;

                // We need to recalculate our bounding volume
                recalculateBoundingVolume();
            }

            // Otherwise we need to work out which child gets to keep
            // the inserted body. We give it to whoever would grow the
            // least to incorporate it.
            else
            {
                if (children[0].volume.getGrowth(newVolume) < children[1].volume.getGrowth(newVolume))
                {
                    children[0].insert(newBody, newVolume);
                }
                else
                {
                    //성장도가 같다면 오른쪽으로 트리 성장시킴 
                    children[1].insert(newBody, newVolume);
                }
            }
        }

        /**
         * Deltes this node, removing it first from the hierarchy, along
         * with its associated
         * rigid body and child nodes. This method deletes the node
         * and all its children (but obviously not the rigid bodies). This
         * also has the effect of deleting the sibling of this node, and
         * changing the parent node so that it contains the data currently
         * in that sibling. Finally it forces the hierarchy above the
         * current node to reconsider its bounding volume.
         */
        public void Detach()
        {
            // If we don't have a parent, then we ignore the sibling
            // processing
            if (null != parent)
            {
                // Find our sibling
                BVHNode sibling = null;
                if (parent.children[0] == this) sibling = parent.children[1];
                else sibling = parent.children[0];

                // Write its data to our parent
                parent.volume = sibling.volume;
                parent.body = sibling.body;
                parent.children[0] = sibling.children[0];
                parent.children[1] = sibling.children[1];

                // Delete the sibling (we blank its parent and
                // children to avoid processing/deleting them)
                sibling.parent = null;
                sibling.body = null;
                sibling.children[0] = null;
                sibling.children[1] = null;
                //delete sibling;

                // Recalculate the parent's bounding volume
                parent.recalculateBoundingVolume();
            }

            // Delete our children (again we remove their
            // parent data so we don't try to process their siblings
            // as they are deleted).
            if (null != children[0])
            {
                children[0].parent = null;
                children[0] = null;
                //delete children[0];

            }
            if (null != children[1])
            {
                children[1].parent = null;
                children[1] = null;
                //delete children[1];

            }
        }



        /**
         * Checks for overlapping between nodes in the hierarchy. Note
         * that any bounding volume should have an overlaps method implemented
         * that checks for overlapping with another object of its own type.
         */
        protected bool overlaps(BVHNode other)
        {
            return volume.overlaps(other.volume);
        }

        /**
         * Checks the potential contacts between this node and the given
         * other node, writing them to the given array (up to the
         * given limit). Returns the number of potential contacts it
         * found.
         */
        protected uint getPotentialContactsWith(BVHNode other, PotentialContact[] contacts, uint ctIdx,
        uint limit)
        {
            // Early out if we don't overlap or if we have no room
            // to report contacts
            if (!overlaps(other) || limit == 0) return 0;

            // If we're both at leaf nodes, then we have a potential contact
            if (isLeaf() && other.isLeaf())
            {
                DebugWide.LogBlue("___^__0 :" + ctIdx);
                contacts[ctIdx].body[0] = body;
                contacts[ctIdx].body[1] = other.body;
                return 1;
            }

            // Determine which node to descend into. If either is
            // a leaf, then we descend the other. If both are branches,
            // then we use the one with the largest size.
            if (other.isLeaf() ||
                (!isLeaf() && volume.getSize() >= other.volume.getSize()))
            {
                DebugWide.LogBlue("_____1 :" + ctIdx);
                // Recurse into ourself
                uint count = children[0].getPotentialContactsWith(other, contacts, ctIdx, limit);

                // Check we have enough slots to do the other side too
                if (limit > count) {
                    return count + children[1].getPotentialContactsWith(
                        other, contacts, ctIdx + count, limit-count
                        );
                } else {
                    return count;
                }
            }
            else
            {
                DebugWide.LogBlue("_____2 :" + ctIdx);
                // Recurse into the other node
                uint count = getPotentialContactsWith(
                    other.children[0], contacts, ctIdx, limit
                    );

                // Check we have enough slots to do the other side too
                if (limit > count) 
                {
                    return count + getPotentialContactsWith(
                        other.children[1], contacts, ctIdx + count, limit-count
                        );
                } else 
                {
                    return count;
                }
            }
        }

        /**
         * For non-leaf nodes, this method recalculates the bounding volume
         * based on the bounding volumes of its children.
         */
        protected void recalculateBoundingVolume(bool recurse = true)
        {
            if (isLeaf()) return;

            // Use the bounding volume combining constructor.
            volume = new BoundingSphere(
                children[0].volume,
                children[1].volume
                );

            // Recurse up the tree
            if (null != parent) parent.recalculateBoundingVolume(true);
        }

        static public void DrawTree(BVHNode node)
        {
            if (null == node) return;

            //DebugWide.DrawCircle(node.volume.centre.ToUnity(), node.volume.radius, UnityEngine.Color.white);

            if (null == node.parent)
                DebugWide.DrawCircle(node.volume.centre.ToUnity(), node.volume.radius, UnityEngine.Color.black);
            else if(false == node.isLeaf())
            {
                DebugWide.DrawCircle(node.volume.centre.ToUnity(), node.volume.radius, UnityEngine.Color.gray);
                DebugWide.DrawCircle(node.volume.centre.ToUnity(), 0.5f, UnityEngine.Color.white);
            }



            if (null != node.children[0])
                DebugWide.DrawLine(node.volume.centre.ToUnity(), node.children[0].volume.centre.ToUnity(), UnityEngine.Color.green);

            if (null != node.children[1])
                DebugWide.DrawLine(node.volume.centre.ToUnity(), node.children[1].volume.centre.ToUnity(), UnityEngine.Color.magenta);

            DrawTree(node.children[0]);
            DrawTree(node.children[1]);

        }

        static public BVHNode GetRoot(BVHNode node)
        {
            if (null != node.parent) return GetRoot(node.parent);
            else
            {
                //DebugWide.LogBlue(node.parent);
                return node;
            }

        }
    }

}
