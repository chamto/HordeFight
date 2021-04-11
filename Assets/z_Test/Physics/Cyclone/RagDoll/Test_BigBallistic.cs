using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;


public class Test_BigBallistic : MonoBehaviour 
{
    Cyclone.BigBallisticDemo demo = null;
    Transform t_pos = null;

    private void Start()
    {
        demo = new Cyclone.BigBallisticDemo();
        //t_pos = GameObject.Find("t_pos").transform;
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
    public enum ShotType
    {
        UNUSED = 0,
        PISTOL,
        ARTILLERY,
        FIREBALL,
        LASER
    }

    public class BigBallisticDemo : RigidBodyApplication
    {
        /**
         * Holds the maximum number of  rounds that can be
         * fired.
         */
        const uint ammoRounds = 256;

        /** Holds the particle data. */
        AmmoRound[] ammo = new AmmoRound[ammoRounds];

        /**
        * Holds the number of boxes in the simulation.
        */
        const uint boxes = 2;

        /** Holds the box data. */
        BBox[] boxData = new BBox[boxes];

        /** Holds the current shot type. */
        ShotType currentShotType;

        /** Resets the position of all the boxes and primes the explosion. */
         void reset()
        {
            for(int i=0;i< ammoRounds;i++)
            {
                AmmoRound shot = ammo[i];
                shot.type = ShotType.UNUSED;
            }
            float z = 20.0f;
            for (int i = 0; i < boxes; i++)
            {
                BBox box = boxData[i];
                box.setState(z);
                z += 90.0f;
            }
            // Make all shots unused
            //for (AmmoRound* shot = ammo; shot < ammo + ammoRounds; shot++)
            //{
            //    shot->type = UNUSED;
            //}

            // Initialise the box
            //float z = 20.0f;
            //for (Box* box = boxData; box < boxData + boxes; box++)
            //{
            //    box->setState(z);
            //    z += 90.0f;
            //}
        }

        /** Build the contacts for the current situation. */
        protected override void generateContacts()
        {
            // Create the ground plane data
            CollisionPlane plane = new CollisionPlane(); ;
            plane.direction = new Vector3(0, 1, 0);
            plane.offset = 0;

            // Set up the collision data structure
            cData.reset(maxContacts);
            cData.friction = 0.9f;
            cData.restitution = 0.1f;
            cData.tolerance = 0.1f;

            for (int i = 0; i < boxes; i++)
            {
                BBox box = boxData[i];
                if (!cData.hasMoreContacts()) return;
                CollisionDetector.boxAndHalfSpace(box, plane, cData);

                for (int j = 0; j < ammoRounds; j++)
                {
                    AmmoRound shot = ammo[j];
                    if (shot.type != ShotType.UNUSED)
                    {
                        if (!cData.hasMoreContacts()) return;

                        // When we get a collision, remove the shot
                        if (0 != CollisionDetector.boxAndSphere(box, shot, cData))
                        {
                            shot.type = ShotType.UNUSED;
                        }
                    }
                }
            }

            // NB We aren't checking box-box collisions.
        }

        /** Processes the objects in the simulation forward in time. */
        protected override void updateObjects(float duration)
        {
            // Update the physics of each particle in turn
            //for (AmmoRound* shot = ammo; shot < ammo + ammoRounds; shot++)
            for (int j = 0; j < ammoRounds; j++)
            {
                AmmoRound shot = ammo[j];
                if (shot.type != ShotType.UNUSED)
                {
                    // Run the physics
                    shot.body.integrate(duration);
                    shot.calculateInternals();

                    // Check if the particle is now invalid
                    if (shot.body.getPosition().y < 0.0f ||
                        shot.startTime + 5000 < stopWatch.ElapsedMilliseconds ||
                        shot.body.getPosition().z > 200.0f)
                    {
                        // We simply set the shot type to be unused, so the
                        // memory it occupies can be reused by another shot.
                        shot.type = ShotType.UNUSED;
                    }
                }
            }

            // Update the boxes
            for (int i = 0; i < boxes; i++)
            {
                BBox box = boxData[i];
                // Run the physics
                box.body.integrate(duration);
                box.calculateInternals();
            }
        }

        /** Dispatches a round. */
        void fire()
        {
            // Find the first available round.
            AmmoRound shot = ammo[0];
            for (int j = 0; j < ammoRounds; j++)
            {
                shot = ammo[j];
                if (shot.type == ShotType.UNUSED) break;
            }

            // If we didn't find a round, then exit - we can't fire.
            //if (shot >= ammo + ammoRounds) return;

            // Set the shot
            shot.setState(currentShotType, (uint)stopWatch.ElapsedMilliseconds);

        }


        /** Creates a new demo object. */
        public BigBallisticDemo():base()
        {
            for (int i = 0; i < boxes; i++)
            {
                boxData[i] = new BBox();
            }

            for (int j = 0; j < ammoRounds; j++)
            {
                ammo[j] = new AmmoRound();
            }
            currentShotType = ShotType.LASER;
            pauseSimulation = false;
            reset();
        }


        /** Display world. */
        public void display()
        {
            //const static GLfloat lightPosition[] = { -1, 1, 0, 0 };

            // Clear the viewport and set the camera direction
            //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            //glLoadIdentity();
            //gluLookAt(-25.0, 8.0, 5.0, 0.0, 5.0, 22.0, 0.0, 1.0, 0.0);

            // Draw a sphere at the firing point, and add a shadow projected
            // onto the ground plane.
            //glColor3f(0.0f, 0.0f, 0.0f);
            //glPushMatrix();
            //glTranslatef(0.0f, 1.5f, 0.0f);
            //glutSolidSphere(0.1f, 5, 5);
            //glTranslatef(0.0f, -1.5f, 0.0f);
            //glColor3f(0.75f, 0.75f, 0.75f);
            //glScalef(1.0f, 0.1f, 1.0f);
            //glutSolidSphere(0.1f, 5, 5);
            //glPopMatrix();

            // Draw some scale lines
            Color cc = new Color(0.75f, 0.75f, 0.75f);
            //glBegin(GL_LINES);
            for (uint i = 0; i < 200; i += 10)
            {
                //glVertex3f(-5.0f, 0.0f, i);
                //glVertex3f(5.0f, 0.0f, i);

                Vector3 p = new Vector3(-5.0f, 0.0f, i);
                Vector3 p1 = new Vector3(5.0f, 0.0f, i);
                DebugWide.DrawLine(p.ToUnity(), p1.ToUnity(), cc);
            }
            //glEnd();

            // Render each particle in turn
            cc = new Color(1, 0, 0);
            for (int j = 0; j < ammoRounds; j++)
            {
                AmmoRound shot = ammo[j];
                if (shot.type != ShotType.UNUSED)
                {
                    shot.render(cc);
                }
            }

            // Render the box
            //glEnable(GL_DEPTH_TEST);
            //glEnable(GL_LIGHTING);
            //glLightfv(GL_LIGHT0, GL_POSITION, lightPosition);
            //glColorMaterial(GL_FRONT_AND_BACK, GL_DIFFUSE);
            //glEnable(GL_COLOR_MATERIAL);
            cc = new Color(1, 0, 0);
            for (int i = 0; i < boxes; i++)
            {
                BBox box = boxData[i];
                box.render(cc);
            }
            //glDisable(GL_COLOR_MATERIAL);
            //glDisable(GL_LIGHTING);
            //glDisable(GL_DEPTH_TEST);

            // Render the description
            //glColor3f(0.0f, 0.0f, 0.0f);
            //renderText(10.0f, 34.0f, "Click: Fire\n1-4: Select Ammo");

            // Render the name of the current shot type
            //switch (currentShotType)
            //{
            //    case PISTOL: renderText(10.0f, 10.0f, "Current Ammo: Pistol"); break;
            //    case ARTILLERY: renderText(10.0f, 10.0f, "Current Ammo: Artillery"); break;
            //    case FIREBALL: renderText(10.0f, 10.0f, "Current Ammo: Fireball"); break;
            //    case LASER: renderText(10.0f, 10.0f, "Current Ammo: Laser"); break;
            //}
        }


        /** Handle a keypress. */
        public void key()
        {

            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentShotType = ShotType.PISTOL;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentShotType = ShotType.ARTILLERY;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentShotType = ShotType.FIREBALL;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                currentShotType = ShotType.LASER;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                reset(); 
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                fire(); 
            }

        }
    }

    public class AmmoRound : CollisionSphere
    {

        public ShotType type;
        public uint startTime;

        public AmmoRound()
        {
            body = new RigidBody();
        }



        /** Draws the box, excluding its shadow. */
        public void render(Color cc)
        {
            // Get the OpenGL transformation
            //GLfloat mat[16];
            //body->getGLTransform(mat);

            //glPushMatrix();
            //glMultMatrixf(mat);
            //glutSolidSphere(radius, 20, 20);
            //glPopMatrix();

            DebugWide.DrawCircle(body.getPosition().ToUnity(), radius, cc);
        }

        /** Sets the box to a specific location. */
        public void setState(ShotType shotType , uint startTime)
        {
            type = shotType;

            // Set the properties of the particle
            switch (type)
            {
                case ShotType.PISTOL:
                    body.setMass(1.5f);
                    body.setVelocity(0.0f, 0.0f, 20.0f);
                    body.setAcceleration(0.0f, -0.5f, 0.0f);
                    body.setDamping(0.99f, 0.8f);
                    radius = 0.2f;
                    break;

                case ShotType.ARTILLERY:
                    body.setMass(200.0f); // 200.0kg
                    body.setVelocity(0.0f, 30.0f, 40.0f); // 50m/s
                    body.setAcceleration(0.0f, -21.0f, 0.0f);
                    body.setDamping(0.99f, 0.8f);
                    radius = 0.4f;
                    break;

                case ShotType.FIREBALL:
                    body.setMass(4.0f); // 4.0kg - mostly blast damage
                    body.setVelocity(0.0f, -0.5f, 10.0f); // 10m/s
                    body.setAcceleration(0.0f, 0.3f, 0.0f); // Floats up
                    body.setDamping(0.9f, 0.8f);
                    radius = 0.6f;
                    break;

                case ShotType.LASER:
                    // Note that this is the kind of laser bolt seen in films,
                    // not a realistic laser beam!
                    body.setMass(0.1f); // 0.1kg - almost no weight
                    body.setVelocity(0.0f, 0.0f, 100.0f); // 100m/s
                    body.setAcceleration(0.0f, 0.0f, 0.0f); // No gravity
                    body.setDamping(0.99f, 0.8f);
                    radius = 0.2f;
                    break;
            }

            body.setCanSleep(false);
            body.setAwake(true);

            Matrix3 tensor = Matrix3.identityMatrix;
            float coeff = 0.4f * body.getMass() * radius * radius;
            tensor.setInertiaTensorCoeffs(coeff, coeff, coeff);
            body.setInertiaTensor(tensor);

            // Set the data common to all particle types
            body.setPosition(0.0f, 1.5f, 0.0f);
            //startTime = (uint)stopWatch.ElapsedMilliseconds;
            this.startTime = startTime;

            // Clear the force accumulators
            body.calculateDerivedData();
            calculateInternals();
        }
    }


    public class BBox : CollisionBox
    {

        public BBox()
        {
            body = new RigidBody();
        }

        /** Draws the box, excluding its shadow. */
        public void render(Color cc)
        {
            // Get the OpenGL transformation
            //GLfloat mat[16];
            //body->getGLTransform(mat);

            //glPushMatrix();
            //glMultMatrixf(mat);
            //glScalef(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);
            //glutSolidCube(1.0f);
            //glPopMatrix();

            Vector3 size = new Vector3(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);

            DebugWide.DrawCube(body.getPosition().ToUnity(), body.getOrientation().ToUnit(), size.ToUnity(), cc);
        }

        /** Sets the box to a specific location. */
        public void setState(float z)
        {
            body.setPosition(0, 3, z);
            body.setOrientation(1, 0, 0, 0);
            body.setVelocity(0, 0, 0);
            body.setRotation(new Vector3(0, 0, 0));
            halfSize = new Vector3(1, 1, 1);

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
            calculateInternals();
        }
    }
}
