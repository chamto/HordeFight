using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;


public class Test_Sailboat : MonoBehaviour 
{
    Cyclone.SailboatDemo demo = null;
    Transform t_pos = null;

    private void Start()
    {
        demo = new Cyclone.SailboatDemo();
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
    public class SailboatDemo 
    {
        Buoyancy buoyancy;

        Aero sail;
        //AeroControl sail;
        RigidBody sailboat;
        ForceRegistry registry;

        Random r;
        Vector3 windspeed;

        float sail_control;

        
        /** Creates a new demo object. */
        public SailboatDemo()
        {
            r = Random.GetI();
            windspeed = new Vector3(1, 0, 1);

            sail = new Aero(new Matrix3(0, 0, 0, 0, 0, 0, 0, 0, -1.0f), new Vector3(2.0f, 0, 0), windspeed);
           // sail = new AeroControl(new Matrix3(0, 0, 0, 0, 0, 0, 0, 0, -1.0f),
           // new Matrix3(0, 0, 0, -0.995f, -0.5f, 0, 0, 0, 0),
           //new Matrix3(0, 0, 0, -1.005f, -0.5f, 0, 0, 0, 0),
           //new Vector3(2.0f, 0, 0), windspeed);

            buoyancy = new Buoyancy(new Vector3(0.0f, 0.5f, 0.0f), 1.0f, 3.0f, 1.6f);

            sail_control = 0;

            sailboat = new RigidBody(); 
            // Set up the boat's rigid body.
            sailboat.setPosition(0, 1.6f, 0);
            sailboat.setOrientation(1, 0, 0, 0);

            sailboat.setVelocity(0, 0, 0);
            sailboat.setRotation(0, 0, 0);

            sailboat.setMass(200.0f);
            Matrix3 it = Matrix3.identityMatrix;
            it.setBlockInertiaTensor(new Vector3(2, 1, 1), 100.0f);
            sailboat.setInertiaTensor(it);

            sailboat.setDamping(0.8f, 0.8f);

            sailboat.setAcceleration(Vector3.GRAVITY);
            sailboat.calculateDerivedData();

            sailboat.setAwake(true);
            sailboat.setCanSleep(false);

            registry = new ForceRegistry();
            registry.add(sailboat, sail);
            registry.add(sailboat, buoyancy);
        }

        void drawBoat(Vector3 pos, Quaternion rot, Color cc)
        {
            Vector3 p;
            Vector3 size;

            // Left Hull
            //glPushMatrix();
            //glTranslatef(0, 0, -1.0f);
            //glScalef(2.0f, 0.4f, 0.4f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(0, 0, -1.0f) + pos;
            size = new Vector3(2.0f, 0.4f, 0.4f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Right Hull
            //glPushMatrix();
            //glTranslatef(0, 0, 1.0f);
            //glScalef(2.0f, 0.4f, 0.4f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(0, 0, 1.0f) + pos;
            size = new Vector3(2.0f, 0.4f, 0.4f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Deck
            //glPushMatrix();
            //glTranslatef(0, 0.3f, 0);
            //glScalef(1.0f, 0.1f, 2.0f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(0, 0.3f, 0) + pos;
            size = new Vector3(1.0f, 0.1f, 2.0f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Mast
            //glPushMatrix();
            //glTranslatef(0, 1.8f, 0);
            //glScalef(0.1f, 3.0f, 0.1f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(0, 1.8f, 0) + pos;
            size = new Vector3(0.1f, 3.0f, 0.1f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

        }

        /** Display the particles. */
        public  void display()
        {
            // Clear the view port and set the camera direction
            //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            //glLoadIdentity();

            Vector3 pos = sailboat.getPosition();
            Vector3 offset = new Vector3(4.0f, 0, 0);
            offset = sailboat.getTransform().transformDirection(offset);
            //gluLookAt(pos.x + offset.x, pos.y + 5.0f, pos.z + offset.z,
                      //pos.x, pos.y, pos.z,
                      //0.0, 1.0, 0.0);

            //glColor3f(0.6f, 0.6f, 0.6f);
            //int bx = int(pos.x);
            //int bz = int(pos.z);
            //glBegin(GL_QUADS);
            //for (int x = -20; x <= 20; x++) for (int z = -20; z <= 20; z++)
            //    {
            //        glVertex3f(bx + x - 0.1f, 0, bz + z - 0.1f);
            //        glVertex3f(bx + x - 0.1f, 0, bz + z + 0.1f);
            //        glVertex3f(bx + x + 0.1f, 0, bz + z + 0.1f);
            //        glVertex3f(bx + x + 0.1f, 0, bz + z - 0.1f);
            //    }
            //glEnd();

            // Set the transform matrix for the aircraft
            //cyclone::Matrix4 transform = sailboat.getTransform();
            //GLfloat gl_transform[16];
            //transform.fillGLArray(gl_transform);
            //glPushMatrix();
            //glMultMatrixf(gl_transform);

            // Draw the boat
            //glColor3f(0, 0, 0);
            drawBoat(sailboat.getPosition(), sailboat.getOrientation(), Color.black);
            //glPopMatrix();

            //char buffer[256];
            //sprintf(
            //    buffer,
            //    "Speed %.1f",
            //    sailboat.getVelocity().magnitude()
            //    );
            //glColor3f(0, 0, 0);
            //renderText(10.0f, 24.0f, buffer);
            DebugWide.LogBlue("Speed: " + sailboat.getVelocity().magnitude());

            //sprintf(
            //    buffer,
            //    "Sail Control: %.1f",
            //    sail_control
            //    );
            //renderText(10.0f, 10.0f, buffer);
            DebugWide.LogBlue("Sail Control: " + sail_control);
        }

        Stopwatch stopWatch = Stopwatch.StartNew();
        long __prevMs = 0;
        long __timeStepMs = 0;
        /** Update the particle positions. */
        public void update()
        {
            __timeStepMs = (stopWatch.ElapsedMilliseconds - __prevMs);
            __prevMs = stopWatch.ElapsedMilliseconds;

            float duration = (float)__timeStepMs * 0.001f;
            if (duration <= 0.0f) return;

            // Start with no forces or acceleration.
            sailboat.clearAccumulators();

            // Add the forces acting on the boat.
            registry.updateForces(duration);

            // Update the boat's physics.
            sailboat.integrate(duration);

            // Change the wind speed.
            windspeed = windspeed * 0.9f + r.randomXZVector(1.0f);
            //windspeed += r.randomXZVector(1.0f);
            sail.windspeed = windspeed;

        }

        /** Handle a key press. */
        public void key()
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                sail_control -= 0.1f;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                sail_control += 0.1f;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                sail_control = 0.0f;
            }


            // Make sure the controls are in range
            if (sail_control < -1.0f) sail_control = -1.0f;
            else if (sail_control > 1.0f) sail_control = 1.0f;

            // Update the control surfaces
            //sail.setControl(sail_control);
        }
    }
}
