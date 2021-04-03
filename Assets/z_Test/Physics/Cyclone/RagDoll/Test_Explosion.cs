using System;
using UnityEngine;


public class Test_Explosion : MonoBehaviour
{

    Cyclone.ExplosionDemo demo = null;

    Transform mousePT = null;

    private void Start()
    {
        mousePT = GameObject.Find("mousePt").transform;
        demo = new Cyclone.ExplosionDemo();
    }

    private void OnDrawGizmos()
    {
        if (null == demo) return;

        Vector3 pos = mousePT.transform.position;
        //demo.InputMousePt(pos.x, pos.y, pos.z);
        demo.mouseDrag(pos.x, pos.y, pos.z);

        if (Input.GetKey(KeyCode.F))
        {
            demo.fire(); 
        }
        if (Input.GetKey(KeyCode.P))
        {
            demo.Input_PauseSimul();
        }
        if(Input.GetKey(KeyCode.Space))
        {
            demo.Input_AutoPause();
        }

        demo.update();
        demo.display();
    }
}


namespace Cyclone
{
    public class Ball : CollisionSphere
    {

        public Ball()
        {
            body = new RigidBody();
        }


        /** Draws the box, excluding its shadow. */
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
            //glutSolidSphere(radius, 20, 20);
            //glPopMatrix();

            Vector3 pos = body.getPosition();
            UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            DebugWide.DrawCircle(u_pos, radius, cc);
        }



        /** Sets the box to a specific location. */
        public void setState(Vector3 position,
                  Quaternion orientation,
                  float radius,
                  Vector3 velocity)
        {
            body.setPosition(position);
            body.setOrientation(orientation);
            body.setVelocity(velocity);
            body.setRotation(new Vector3(0, 0, 0));
            this.radius = radius;

            float mass = 4.0f * 0.3333f * 3.1415f * radius * radius * radius;
            body.setMass(mass);

            Matrix3 tensor = Matrix3.identityMatrix;
            float coeff = 0.4f * mass * radius * radius;
            tensor.setInertiaTensorCoeffs(coeff, coeff, coeff);
            body.setInertiaTensor(tensor);

            body.setLinearDamping(0.95f);
            body.setAngularDamping(0.8f);
            body.clearAccumulators();
            body.setAcceleration(0, -10.0f, 0);

            //body->setCanSleep(false);
            body.setAwake(true);

            body.calculateDerivedData();
        }

        /** Positions the box at a random location. */
        public void random(Random random)
        {
            Vector3 minPos = new Vector3(-5, 5, -5);
            Vector3 maxPos = new Vector3(5, 10, 5);
            //Random r;
            setState(
                random.randomVector(minPos, maxPos),
                random.randomQuaternion(),
                (float)random.randomReal(0.5f, 1.5f),
                new Vector3()
                );
        }
    }


    public class Box : CollisionBox
    {

        public bool isOverlapping;

        public Box()
        {
            body = new RigidBody();
        }


        /** Draws the box, excluding its shadow. */
        public void render()
        {
            // Get the OpenGL transformation
            //GLfloat mat[16];
            //body->getGLTransform(mat);

            Color cc;

            if (isOverlapping) cc = new Color(0.7f, 1.0f, 0.7f);
            else if (body.getAwake()) cc = new Color(1.0f, 0.7f, 0.7f);
            else cc = new Color(0.7f, 0.7f, 1.0f);

            //glPushMatrix();
            //glMultMatrixf(mat);
            //glScalef(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);
            //glutSolidCube(1.0f);
            //glPopMatrix();

            Vector3 pos = body.getPosition();
            Quaternion rot = body.getOrientation();
            Vector3 size = new Vector3(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);
            //Vector3 size = new Vector3(halfSize.x * 1, halfSize.y * 1, halfSize.z * 1);

            UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            UnityEngine.Quaternion u_rot = new UnityEngine.Quaternion(rot.i, rot.j, rot.k, rot.r);
            UnityEngine.Vector3 u_size = new UnityEngine.Vector3(size.x, size.y, size.z);

            DebugWide.DrawCube(u_pos, u_rot, u_size, cc);
        }


        /** Sets the box to a specific location. */
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

            body.setCanSleep(false);
            body.setAwake(true);

            body.calculateDerivedData();

        }

        /** Positions the box at a random location. */
        public void random(Random random)
        {
            Vector3 minPos = new Vector3(-5, 5, -5);
            Vector3 maxPos = new Vector3(5, 10, 5);
            Vector3 minSize = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 maxSize = new Vector3(4.5f, 1.5f, 1.5f);

            setState(
                random.randomVector(minPos, maxPos),
                random.randomQuaternion(),
                random.randomVector(minSize, maxSize),
                new Vector3()
                );
        }
    }

    public class ExplosionDemo : RigidBodyApplication
    {
        bool editMode, upMode;

        /**
         * Holds the number of boxes in the simulation.
         */
        const uint boxes = 2;//OBJECTS;

        /** Holds the box data. */
        Box[] boxData = new Box[boxes];

        /**
         * Holds the number of balls in the simulation.
         */
        const uint balls = 0;//OBJECTS;

        /** Holds the ball data. */
        Ball[] ballData = new Ball[balls];


        /** Creates a new demo object. */
        public ExplosionDemo() : base()
        {
            editMode = false;
            upMode = false;

            for (int i = 0; i < boxes; i++)
            {
                boxData[i] = new Box();
            }

            for (int i = 0; i < balls; i++)
            {
                ballData[i] = new Ball();
            }


            reset();
        }

        /** Resets the position of all the boxes and primes the explosion. */
        public void reset()
        {
            //Box* box = boxData;

            boxData[0].setState(new Vector3(0, 1, 5),
                            Quaternion.identity,
                            new Vector3(1, 1, 1),
                            new Vector3(0, 0, 0));

            boxData[1].setState(new Vector3(0, 1, 0),
            Quaternion.identity,
            new Vector3(1, 1, 1),
            new Vector3(0, 0, 0));

            //ballData[0].setState(new Vector3(0, 2, 0),
            //                Quaternion.identity,
            //                1f,
            //                new Vector3(0, 0, 0));

            //ballData[1].setState(new Vector3(0, 1, 5),
                            //Quaternion.identity,
                            //1f,
                            //new Vector3(0, 0, 0));

            // Create the random objects
            Random random = new Random();

            //DebugWide.LogBlue(random.randomReal() + " __**__ " + random.randomReal_2());
            //DebugWide.LogBlue(random.randomReal() + " __**__ " + random.randomReal_2());
            //DebugWide.LogBlue(random.randomReal() + " __**__ " + random.randomReal_2());
            for (int i=0 ; i < boxes; i++)
            {
                //boxData[i].random(random); //문제 생김 
            }

            for (int i = 0; i < balls; i++)
            {
                //ballData[i].random(random);
            }

            // Reset the contacts
            cData.contactCount = 0;
        }

        /** Processes the contact generation code. */
        protected override void generateContacts()
        {

            // Note that this method makes a lot of use of early returns to avoid
            // processing lots of potential contacts that it hasn't got room to
            // store.

            // Create the ground plane data
            CollisionPlane plane = new CollisionPlane();
            plane.direction = new Vector3(0, 1, 0);
            plane.offset = 0;

            // Set up the collision data structure
            cData.reset(maxContacts);
            cData.friction = 0.9f;
            cData.restitution = 0.6f;
            cData.tolerance = 0.1f;

            // Perform exhaustive collision detection
            //Matrix4 transform, otherTransform;
            //Vector3 position, otherPosition;
            //for (Box* box = boxData; box < boxData + boxes; box++)
            for (int i = 0; i < boxes; i++)
            {
                // Check for collisions with the ground plane
                if (!cData.hasMoreContacts()) return;
                CollisionDetector.boxAndHalfSpace(boxData[i], plane, cData);

                // Check for collisions with each other box
                //for (Box* other = box + 1; other < boxData + boxes; other++)
                for (int j = i+1; j < boxes; j++)
                {
                    if (!cData.hasMoreContacts()) return;
                    CollisionDetector.boxAndBox(boxData[i], boxData[j], cData);

                    if (IntersectionTests.boxAndBox(boxData[i], boxData[j]))
                    {
                        boxData[i].isOverlapping = boxData[j].isOverlapping = true;
                    }
                }

                // Check for collisions with each ball
                //for (Ball* other = ballData; other < ballData + balls; other++)
                for (int k = 0; k < balls; k++)
                {
                    if (!cData.hasMoreContacts()) return;
                    CollisionDetector.boxAndSphere(boxData[i], ballData[k], cData);
                }
            }


            //for (Ball* ball = ballData; ball < ballData + balls; ball++)
            for (int i = 0; i < balls; i++)
            {
                // Check for collisions with the ground plane
                if (!cData.hasMoreContacts()) return;
                CollisionDetector.sphereAndHalfSpace(ballData[i], plane, cData);


                //for (Ball* other = ball + 1; other < ballData + balls; other++)
                for (int k = i+1; k < balls; k++)
                {
                    // Check for collisions with the ground plane
                    if (!cData.hasMoreContacts()) return;
                    CollisionDetector.sphereAndSphere(ballData[i], ballData[k], cData);
                }
            }

            DebugWide.LogBlue(cData.contactCount);
            //for(int i=0;i< cData.contactCount;i++)
            //{
            //    Contact contact = cData.contactArray[i];
            //    DebugWide.LogBlue("" + i + "  __ " + contact);
            //}
        }

        /** Processes the objects in the simulation forward in time. */
        protected override void updateObjects(float duration)
        {
            // Update the physics of each box in turn
            //for (Box* box = boxData; box < boxData + boxes; box++)
            for (int i = 0; i < boxes; i++)
            {
                // Run the physics
                boxData[i].body.integrate(duration);
                boxData[i].calculateInternals();
                boxData[i].isOverlapping = false;
            }

            // Update the physics of each ball in turn
            //for (Ball* ball = ballData; ball < ballData + balls; ball++)
            for (int i = 0; i < balls; i++)
            {
                // Run the physics
                ballData[i].body.integrate(duration);
                ballData[i].calculateInternals();
            }
        }


        /** Sets up the rendering. */
        //virtual void initGraphics();

        /** Returns the window title for the demo. */
        //virtual const char* getTitle();

        /** Display the particle positions. */
        public void display()
        {

            // Update the physics of each box in turn
            //for (Box* box = boxData; box < boxData + boxes; box++)
            for (int i = 0; i < boxes; i++)
            {
                // Run the physics
                boxData[i].calculateInternals();
                boxData[i].isOverlapping = false;
            }

            // Update the physics of each ball in turn
            //for (Ball* ball = ballData; ball < ballData + balls; ball++)
            for (int i = 0; i < balls; i++)
            {
                // Run the physics
                ballData[i].calculateInternals();
            }


            //for (Box* box = boxData; box < boxData + boxes; box++)
            for (int i = 0; i < boxes; i++)
            {
                boxData[i].render();
            }
            //for (Ball* ball = ballData; ball < ballData + balls; ball++)
            for (int i = 0; i < balls; i++)
            {
                ballData[i].render();
            }


            // Finish the frame, rendering any additional information
            drawDebug();
        }


        /** Handles a key press. */
        //virtual void key(unsigned char key);

        /** Handle a mouse drag */
        public void InputMousePt(float x, float y, float z)
        {
            //boxData[0].body.setAcceleration(0, 0, 0);
             
            //boxData[0].body.setPosition(x, y, z);
            //boxData[0].body.calculateDerivedData();
        }

        /** Detonates the explosion. */
        public void fire()
        {
            //Vector3 pos = ballData[0].body.getPosition();
            //pos.normalise();
            //ballData[0].body.addForce(pos * -1000.0f);
            //ballData[0].body.addForce(Vector3.Y * -1000.0f);

            //boxData[0].body.addForceAtBodyPoint(
            //new Vector3(0, 500f, 0), new Vector3(0,0,0));

            boxData[0].body.addForce(new Vector3(0, 500f, 0));
        }

        public void mouseDrag(float x, float y, float z)
        {
            float gamdo = 2.2f;
            boxData[0].body.setPosition(boxData[0].body.getPosition() +
                    new Vector3(
                        (x - last_x) * gamdo,
                        (y - last_y) * gamdo,
                        (z - last_z) * gamdo
                        )
                    );
            boxData[0].body.calculateDerivedData();

            //ballData[0].body.setPosition(ballData[0].body.getPosition() +
            //        new Vector3(
            //            (x - last_x) * gamdo,
            //            (y - last_y) * gamdo,
            //            (z - last_z) * gamdo
            //            )
            //        );
            //ballData[0].body.calculateDerivedData();

            // Remember the position
            last_x = x;
            last_y = y;
            last_z = z;
        }
    }
}