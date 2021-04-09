using UnityEngine;

public class Test_Fracture : MonoBehaviour 
{
    Cyclone.FractureDemo demo = null;

    private void Start()
    {
        demo = new Cyclone.FractureDemo();
    }

    private void Update()
    {
        demo.key();
        demo.update();
    }

    private void OnDrawGizmos()
    {
        if (null == demo) return;
        demo.display();
    }
}


namespace Cyclone
{

    public class FractureDemo : RigidBodyApplication
    {
        /** Tracks if a block has been hit. */
        bool hit;
        bool ball_active;
        uint fracture_contact;

        /** Handle random numbers. */
        //cyclone::Random random;

        const int MAX_BLOCKS = 9;

        /** Holds the bodies. */
        Block[] blocks = new Block[MAX_BLOCKS];

        /** Holds the projectile. */
        CollisionSphere ball = new CollisionSphere();

        /** Processes the contact generation code. */
        protected override void generateContacts()
        {
            hit = false;

            // Create the ground plane data
            CollisionPlane plane = new CollisionPlane();
            plane.direction = new Vector3(0, 1, 0);
            plane.offset = 0;

            // Set up the collision data structure
            cData.reset(maxContacts);
            cData.friction = 0.9f;
            cData.restitution = 0.2f;
            cData.tolerance = 0.1f;

            // Perform collision detection
            Matrix4 transform, otherTransform;
            Vector3 position, otherPosition;
            for(int i=0;i< MAX_BLOCKS;i++)
            //for (Block* block = blocks; block < blocks + MAX_BLOCKS; block++)
            {
                Block block = blocks[i];
                if (!block.exists) continue;

                // Check for collisions with the ground plane
                if (!cData.hasMoreContacts()) return;
                CollisionDetector.boxAndHalfSpace(block, plane, cData);

                if (ball_active)
                {
                    // And with the sphere
                    if (!cData.hasMoreContacts()) return;
                    if (0 != CollisionDetector.boxAndSphere(block, ball, cData))
                    {
                        hit = true;
                        fracture_contact = cData.contactCount - 1;
                    }
                }

                // Check for collisions with each other box
                for (int j = i+1; j < MAX_BLOCKS; j++)
                //for (Block* other = block + 1; other < blocks + MAX_BLOCKS; other++)
                {
                    Block other = blocks[j];
                    if (!other.exists) continue;

                    if (!cData.hasMoreContacts()) return;
                    CollisionDetector.boxAndBox(block, other, cData);
                }
            }

            // Check for sphere ground collisions
            if (ball_active)
            {
                if (!cData.hasMoreContacts()) return;
                CollisionDetector.sphereAndHalfSpace(ball, plane, cData);
            }
        }

        /** Processes the objects in the simulation forward in time. */
        protected override void updateObjects(float duration)
        {
            for (int i = 0; i < MAX_BLOCKS; i++)
            //for (Block* block = blocks; block < blocks + MAX_BLOCKS; block++)
            {
                Block block = blocks[i];

                if (block.exists)
                {
                    block.body.integrate(duration);
                    block.calculateInternals();
                }
            }

            if (ball_active)
            {
                ball.body.integrate(duration);
                ball.calculateInternals();
            }
        }

        /** Resets the position of all the blocks. */
        void reset()
        {
            // Only the first block exists
            blocks[0].exists = true;
            for (int j = 1; j < MAX_BLOCKS; j++)
            //for (Block* block = blocks + 1; block < blocks + MAX_BLOCKS; block++)
            {
                Block block = blocks[j];
                block.exists = false;
            }

            // Set the first block
            blocks[0].halfSize = new Vector3(4, 4, 4);
            blocks[0].body.setPosition(0, 7, 0);
            blocks[0].body.setOrientation(1, 0, 0, 0);
            blocks[0].body.setVelocity(0, 0, 0);
            blocks[0].body.setRotation(0, 0, 0);
            blocks[0].body.setMass(100.0f);
            Matrix3 it = Matrix3.identityMatrix;
            it.setBlockInertiaTensor(blocks[0].halfSize, 100.0f);
            blocks[0].body.setInertiaTensor(it);
            blocks[0].body.setDamping(0.9f, 0.9f);
            blocks[0].body.calculateDerivedData();
            blocks[0].calculateInternals();

            blocks[0].body.setAcceleration(Vector3.GRAVITY);
            blocks[0].body.setAwake(true);
            blocks[0].body.setCanSleep(true);


            ball_active = true;

            // Set up the ball
            ball.body.setPosition(0, 5.0f, 20.0f);
            ball.body.setOrientation(1, 0, 0, 0);
            ball.body.setVelocity(
                (float)random.randomBinomial(4.0f),
                (float)random.randomReal(1.0f, 6.0f),
                -20.0f
                );
            ball.body.setRotation(0, 0, 0);
            ball.body.calculateDerivedData();
            ball.body.setAwake(true);
            ball.calculateInternals();

            hit = false;

            // Reset the contacts
            cData.contactCount = 0;
        }

        /** Processes the physics. */
        public void update()
        {
            base.update();

            // Handle fractures.
            if (hit)
            {
                blocks[0].divideBlock(
                    cData.contactArray[fracture_contact],
                    blocks[0],
                    blocks
                    );
                ball_active = false;
            }
        }


        /** Creates a new demo object. */
        public FractureDemo()
        {
            for(int i=0;i< MAX_BLOCKS;i++)
            {
                blocks[i] = new Block();
            }

            // Create the ball.
            ball.body = new RigidBody();
            ball.radius = 0.25f;
            ball.body.setMass(5.0f);
            ball.body.setDamping(0.9f, 0.9f);
            Matrix3 it = Matrix3.identityMatrix;
            it.setDiagonal(5.0f, 5.0f, 5.0f);
            ball.body.setInertiaTensor(it);
            ball.body.setAcceleration(Vector3.GRAVITY);

            ball.body.setCanSleep(false);
            ball.body.setAwake(true);

            // Set up the initial block
            reset();
        }


        /** Display the particle positions. */
        public void display()
        {
            //const static GLfloat lightPosition[] = { 0.7f, 1, 0.4f, 0 };

            //RigidBodyApplication::display();

            //glEnable(GL_DEPTH_TEST);
            //glEnable(GL_LIGHTING);
            //glEnable(GL_LIGHT0);

            //glLightfv(GL_LIGHT0, GL_POSITION, lightPosition);
            //glColorMaterial(GL_FRONT_AND_BACK, GL_DIFFUSE);
            //glEnable(GL_COLOR_MATERIAL);

            //glEnable(GL_NORMALIZE);
            for (int i = 0; i < MAX_BLOCKS; i++)
            //for (Block* block = blocks; block < blocks + MAX_BLOCKS; block++)
            {
                Block block = blocks[i];
                if (block.exists) block.render();
            }
            //glDisable(GL_NORMALIZE);

            if (ball_active)
            {
                //glColor3f(0.4f, 0.7f, 0.4f);
                //glPushMatrix();
                //cyclone::Vector3 pos = ball.body->getPosition();
                //glTranslatef(pos.x, pos.y, pos.z);
                //glutSolidSphere(0.25f, 16, 8);
                //glPopMatrix();

                DebugWide.DrawSolidCircle(ball.body.getPosition().ToUnity(), 0.25f, new Color(0.4f, 0.7f, 0.4f));
            }

            //glDisable(GL_LIGHTING);
            //glDisable(GL_COLOR_MATERIAL);

            // Draw some scale circles
            //glColor3f(0.75, 0.75, 0.75);
            //for (unsigned i = 1; i < 20; i++)
            //{
            //    glBegin(GL_LINE_LOOP);
            //    for (unsigned j = 0; j < 32; j++)
            //    {
            //        float theta = 3.1415926f * j / 16.0f;
            //        glVertex3f(i * cosf(theta), 0.0f, i * sinf(theta));
            //    }
            //    glEnd();
            //}
            //glBegin(GL_LINES);
            //glVertex3f(-20, 0, 0);
            //glVertex3f(20, 0, 0);
            //glVertex3f(0, 0, -20);
            //glVertex3f(0, 0, 20);
            //glEnd();

            base.drawDebug();
        }
    }

    public class Block : CollisionBox
    {

        public bool exists;

        public Block()
        {
            exists = false;
            body = new RigidBody();
        }


        /** Draws the block. */
        public void render()
        {
            // Get the OpenGL transformation
            //GLfloat mat[16];
            //body->getGLTransform(mat);

            Color cc;
            if (body.getAwake()) cc = new Color(1.0f, 0.7f, 0.7f);
            else cc = new Color(0.7f, 0.7f, 1.0f);

            //glPushMatrix();
            //glMultMatrixf(mat);
            //glScalef(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);
            //glutSolidCube(1.0f);
            //glPopMatrix();

            Vector3 pos = body.getPosition();
            Quaternion rot = body.getOrientation();
            Vector3 size = new Vector3(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);

            DebugWide.DrawCube(pos.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);
        }

        /** Sets the block to a specific location. */
        public void setState(Vector3 position,
                  Quaternion orientation,
                  Vector3 extents,
                  Vector3 velocity)
        {
            body.setPosition(position);
            body.setOrientation(orientation);
            body.setVelocity(velocity);
            body.setRotation(new Vector3(0, 0, 0));
            halfSize = extents;

            float mass = halfSize.x * halfSize.y * halfSize.z * 8.0f;
            body.setMass(mass);

            Matrix3 tensor = Matrix3.identityMatrix;
            tensor.setBlockInertiaTensor(halfSize, mass);
            body.setInertiaTensor(tensor);

            body.setLinearDamping(0.95f);
            body.setAngularDamping(0.8f);
            body.clearAccumulators();
            body.setAcceleration(0, -10.0f, 0);

            //body->setCanSleep(false);
            body.setAwake(true);

            body.calculateDerivedData();
        }

        /**
         * Calculates and sets the mass and inertia tensor of this block,
         * assuming it has the given constant density.
         */
        public void calculateMassProperties(float invDensity)
        {
            // Check for infinite mass
            if (invDensity <= 0)
            {
                // Just set zeros for both mass and inertia tensor
                body.setInverseMass(0);
                body.setInverseInertiaTensor(Matrix3.identityMatrix);
            }
            else
            {
                // Otherwise we need to calculate the mass
                float volume = halfSize.magnitude() * 2.0f;
                float mass = volume / invDensity;

                body.setMass(mass);

                // And calculate the inertia tensor from the mass and size
                mass *= 0.333f;
                Matrix3 tensor = Matrix3.identityMatrix;
                tensor.setInertiaTensorCoeffs(
                    mass * halfSize.y * halfSize.y + halfSize.z * halfSize.z,
                    mass * halfSize.y * halfSize.x + halfSize.z * halfSize.z,
                    mass * halfSize.y * halfSize.x + halfSize.z * halfSize.y
                    );
                body.setInertiaTensor(tensor);
            }

        }

        /**
         * Performs the division of the given block into four, writing the
         * eight new blocks into the given blocks array. The blocks array can be
         * a pointer to the same location as the target pointer: since the
         * original block is always deleted, this effectively reuses its storage.
         * The algorithm is structured to allow this reuse.
         */
        public void divideBlock(Contact contact, Block target, Block[] blocks)
        {
            // Find out if we're block one or two in the contact structure, and
            // therefore what the contact normal is.
            Vector3 normal = contact.contactNormal;
            RigidBody body_ = contact.body[0];
            if (body_ != target.body)
            {
                normal.invert();
                body_ = contact.body[1];
            }

            // Work out where on the body (in body coordinates) the contact is
            // and its direction.
            Vector3 point = body_.getPointInLocalSpace(contact.contactPoint);
            normal = body_.getDirectionInLocalSpace(normal);

            // Work out the centre of the split: this is the point coordinates
            // for each of the axes perpendicular to the normal, and 0 for the
            // axis along the normal.
            point = point - normal * (point * normal);

            // Take a copy of the half size, so we can create the new blocks.
            Vector3 size = target.halfSize;

            // Take a copy also of the body's other data.
            RigidBody tempBody = new RigidBody();
            tempBody.setPosition(body_.getPosition());
            tempBody.setOrientation(body_.getOrientation());
            tempBody.setVelocity(body_.getVelocity());
            tempBody.setRotation(body_.getRotation());
            tempBody.setLinearDamping(body_.getLinearDamping());
            tempBody.setAngularDamping(body_.getAngularDamping());
            tempBody.setInverseInertiaTensor(body_.getInverseInertiaTensor());
            tempBody.calculateDerivedData();

            // Remove the old block
            target.exists = false;

            // Work out the inverse density of the old block
            float invDensity =
                halfSize.magnitude() * 8 * body_.getInverseMass();

            // Now split the block into eight.
            for (uint i = 0; i < 8; i++)
            {
                // Find the minimum and maximum extents of the new block
                // in old-block coordinates
                Vector3 min = Vector3.ZERO, max = Vector3.ZERO;
                if ((i & 1) == 0)
                {
                    min.x = -size.x;
                    max.x = point.x;
                }
                else
                {
                    min.x = point.x;
                    max.x = size.x;
                }
                if ((i & 2) == 0)
                {
                    min.y = -size.y;
                    max.y = point.y;
                }
                else
                {
                    min.y = point.y;
                    max.y = size.y;
                }
                if ((i & 4) == 0)
                {
                    min.z = -size.z;
                    max.z = point.z;
                }
                else
                {
                    min.z = point.z;
                    max.z = size.z;
                }

                // Get the origin and half size of the block, in old-body
                // local coordinates.
                Vector3 halfSize_ = (max - min) * 0.5f;
                Vector3 newPos = halfSize_ + min;

                // Convert the origin to world coordinates.
                newPos = tempBody.getPointInWorldSpace(newPos);

                // Work out the direction to the impact.
                Vector3 direction = newPos - contact.contactPoint;
                direction.normalise();

                // Set the body's properties (we assume the block has a body
                // already that we're going to overwrite).
                blocks[i].body.setPosition(newPos);
                blocks[i].body.setVelocity(tempBody.getVelocity() + direction * 10.0f);
                blocks[i].body.setOrientation(tempBody.getOrientation());
                blocks[i].body.setRotation(tempBody.getRotation());
                blocks[i].body.setLinearDamping(tempBody.getLinearDamping());
                blocks[i].body.setAngularDamping(tempBody.getAngularDamping());
                blocks[i].body.setAwake(true);
                blocks[i].body.setAcceleration(Vector3.GRAVITY);
                blocks[i].body.clearAccumulators();
                blocks[i].body.calculateDerivedData();
                blocks[i].offset = Matrix4.identityMatrix;
                blocks[i].exists = true;
                blocks[i].halfSize = halfSize_;

                // Finally calculate the mass and inertia tensor of the new block
                blocks[i].calculateMassProperties(invDensity);
            }
        }
    }
}