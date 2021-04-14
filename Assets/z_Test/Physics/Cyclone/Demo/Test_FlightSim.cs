using UnityEngine;
using System.Diagnostics;

public class Test_FlightSim : MonoBehaviour 
{
    Cyclone.FlightSimDemo demo = null;
    Transform t_pos = null;

    private void Start()
    {
        demo = new Cyclone.FlightSimDemo();
        t_pos = GameObject.Find("t_pos").transform;
    }

    private void Update()
    {

        demo.key();
        demo.update();
    }

    private void OnDrawGizmos()
    {
        if (null == demo) return;
        demo.display(t_pos);
    }
}

namespace Cyclone
{
    /**
 * The main demo class definition.
 */
    public class FlightSimDemo
    {
        AeroControl left_wing;
        AeroControl right_wing;
        AeroControl rudder;
        Aero tail;
        RigidBody aircraft;
        ForceRegistry registry;

        Vector3 windspeed;

        float left_wing_control;
        float right_wing_control;
        float rudder_control;

        void resetPlane()
        {
            aircraft.setPosition(0, 0, 0);
            aircraft.setOrientation(1, 0, 0, 0);

            aircraft.setVelocity(0, 0, 0);
            aircraft.setRotation(0, 0, 0);
        }


        /** Creates a new demo object. */
        public FlightSimDemo()
        {
            windspeed = new Vector3(0, 0, 0);

            right_wing = new AeroControl(new Matrix3(0, 0, 0, -1, -0.5f, 0, 0, 0, 0),
           new Matrix3(0, 0, 0, -0.995f, -0.5f, 0, 0, 0, 0),
           new Matrix3(0, 0, 0, -1.005f, -0.5f, 0, 0, 0, 0),
           new Vector3(-1.0f, 0.0f, 2.0f), windspeed);

            left_wing = new AeroControl(new Matrix3(0, 0, 0, -1, -0.5f, 0, 0, 0, 0),
              new Matrix3(0, 0, 0, -0.995f, -0.5f, 0, 0, 0, 0),
              new Matrix3(0, 0, 0, -1.005f, -0.5f, 0, 0, 0, 0),
              new Vector3(-1.0f, 0.0f, -2.0f), windspeed);

            rudder = new AeroControl(new Matrix3(0, 0, 0, 0, 0, 0, 0, 0, 0),
           new Matrix3(0, 0, 0, 0, 0, 0, 0.01f, 0, 0),
           new Matrix3(0, 0, 0, 0, 0, 0, -0.01f, 0, 0),
           new Vector3(2.0f, 0.5f, 0), windspeed);

            tail = new Aero(new Matrix3(0, 0, 0, -1, -0.5f, 0, 0, 0, -0.1f),
                 new Vector3(2.0f, 0, 0), windspeed);

            left_wing_control = 0;
            right_wing_control = 0;
            rudder_control = 0;


            aircraft = new RigidBody();

            // Set up the aircraft rigid body.
            resetPlane();

            aircraft.setMass(2.5f);
            Matrix3 it = Matrix3.identityMatrix;
            it.setBlockInertiaTensor(new Vector3(2, 1, 1), 1);
            aircraft.setInertiaTensor(it);

            aircraft.setDamping(0.8f, 0.8f);

            aircraft.setAcceleration(Vector3.GRAVITY);
            aircraft.calculateDerivedData();

            aircraft.setAwake(true);
            aircraft.setCanSleep(false);

            registry = new ForceRegistry();
            registry.add(aircraft, left_wing);
            registry.add(aircraft, right_wing);
            registry.add(aircraft, rudder);
            registry.add(aircraft, tail);
        }

        void drawAircraft(Vector3 pos, Quaternion rot, Color cc)
        {
            Vector3 p;
            Vector3 size;
            // Fuselage
            //glPushMatrix();
            //glTranslatef(-0.5f, 0, 0);
            //glScalef(2.0f, 0.8f, 1.0f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(-0.5f, 0, 0) + pos;
            size = new Vector3(2.0f, 0.8f, 1.0f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Rear Fuselage
            //glPushMatrix();
            //glTranslatef(1.0f, 0.15f, 0);
            //glScalef(2.75f, 0.5f, 0.5f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(1.0f, 0.15f, 0) + pos;
            size = new Vector3(2.75f, 0.5f, 0.5f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Wings
            //glPushMatrix();
            //glTranslatef(0, 0.3f, 0);
            //glScalef(0.8f, 0.1f, 6.0f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(0, 0.3f, 0) + pos;
            size = new Vector3(0.8f, 0.1f, 6.0f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Rudder
            //glPushMatrix();
            //glTranslatef(2.0f, 0.775f, 0);
            //glScalef(0.75f, 1.15f, 0.1f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(2.0f, 0.775f, 0) + pos;
            size = new Vector3(0.75f, 1.15f, 0.1f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);

            // Tail-plane
            //glPushMatrix();
            //glTranslatef(1.9f, 0, 0);
            //glScalef(0.85f, 0.1f, 2.0f);
            //glutSolidCube(1.0f);
            //glPopMatrix();
            p = new Vector3(1.9f, 0, 0) + pos;
            size = new Vector3(0.85f, 0.1f, 2.0f);
            DebugWide.DrawCube(p.ToUnity(), rot.ToUnit(), size.ToUnity(), cc);
        }

        /** Display the particles. */
        public void display(Transform t_pos)
        {
            // Clear the view port and set the camera direction
            //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            //glLoadIdentity();

            Vector3 pos = aircraft.getPosition();
            Vector3 offset = new Vector3(4.0f + aircraft.getVelocity().magnitude(), 0, 0);
            offset = aircraft.getTransform().transformDirection(offset);
            t_pos.position = pos.ToUnity();
            //gluLookAt(pos.x + offset.x, pos.y + 5.0f, pos.z + offset.z,
                      //pos.x, pos.y, pos.z,
                      //0.0, 1.0, 0.0);

            Color cc = new Color(0.6f, 0.6f, 0.6f);
            int bx = (int)(pos.x);
            int bz = (int)(pos.z);
            //glBegin(GL_QUADS);
            for (int x = -20; x <= 20; x++)
            {
                for (int z = -20; z <= 20; z++)
                {
                    //glVertex3f(bx + x - 0.1f, 0, bz + z - 0.1f);
                    //glVertex3f(bx + x - 0.1f, 0, bz + z + 0.1f);
                    //glVertex3f(bx + x + 0.1f, 0, bz + z + 0.1f);
                    //glVertex3f(bx + x + 0.1f, 0, bz + z - 0.1f);

                    Vector3 p0, p1, p2, p3;
                    p0 = new Vector3(bx + x - 0.1f, 0, bz + z - 0.1f);
                    p1 = new Vector3(bx + x - 0.1f, 0, bz + z + 0.1f);
                    p2 = new Vector3(bx + x + 0.1f, 0, bz + z + 0.1f);
                    p3 = new Vector3(bx + x + 0.1f, 0, bz + z - 0.1f);

                    //이 처리 수행하면 안됨. 컴퓨터가 뻗어버린다 
                    //DebugWide.DrawSolidQuads(p0.ToUnity(), p1.ToUnity(), p2.ToUnity(), p3.ToUnity(), cc); 
                }
            }

            //glEnd();

            // Set the transform matrix for the aircraft
            Matrix4 transform = aircraft.getTransform();
            //GLfloat gl_transform[16];
            //transform.fillGLArray(gl_transform);
            //glPushMatrix();
            //glMultMatrixf(gl_transform);

            // Draw the aircraft
            cc = new Color(0, 0, 0);
            drawAircraft(aircraft.getPosition(), aircraft.getOrientation(), cc);
            //glPopMatrix();

            //glColor3f(0.8f, 0.8f, 0.8f);
            //glPushMatrix();
            //glTranslatef(0, -1.0f - pos.y, 0);
            //glScalef(1.0f, 0.001f, 1.0f);
            //glMultMatrixf(gl_transform);
            //drawAircraft();
            //glPopMatrix();

            //char buffer[256];
            //sprintf(
            //    buffer,
            //    "Altitude: %.1f | Speed %.1f",
            //    aircraft.getPosition().y,
            //    aircraft.getVelocity().magnitude()
            //    );
            //glColor3f(0, 0, 0);
            //renderText(10.0f, 24.0f, buffer);
            DebugWide.LogBlue("Altitude: " + aircraft.getPosition().y + " Speed: " + 
                aircraft.getVelocity().magnitude());

            //sprintf(
            //    buffer,
            //    "Left Wing: %.1f | Right Wing: %.1f | Rudder %.1f",
            //    left_wing_control, right_wing_control, rudder_control
            //    );
            //renderText(10.0f, 10.0f, buffer);
            DebugWide.LogBlue("Left Wing: " + left_wing_control + " Right Wing: " + right_wing_control + " Rudder: " + rudder_control);
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
            aircraft.clearAccumulators();

            // Add the propeller force
            Vector3 propulsion = new Vector3(-10.0f, 0, 0);
            propulsion = aircraft.getTransform().transformDirection(propulsion);
            aircraft.addForce(propulsion);

            // Add the forces acting on the aircraft.
            registry.updateForces(duration);

            // Update the aircraft's physics.
            aircraft.integrate(duration);

            // Do a very basic collision detection and response with the ground.
            Vector3 pos = aircraft.getPosition();
            if (pos.y < 0.0f)
            {
                pos.y = 0.0f;
                aircraft.setPosition(pos);

                if (aircraft.getVelocity().y < -10.0f)
                {
                    //resetPlane(); //임시로 주석 
                }
            }

        }

        /** Handle a key press. */
        public void key()
        {
            if(Input.GetKey(KeyCode.Q))
            {
                rudder_control += 0.1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                rudder_control -= 0.1f;
            }
            if (Input.GetKey(KeyCode.W))
            {
                left_wing_control -= 0.1f;
                right_wing_control -= 0.1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                left_wing_control += 0.1f;
                right_wing_control += 0.1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                left_wing_control -= 0.1f;
                right_wing_control += 0.1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                left_wing_control += 0.1f;
                right_wing_control -= 0.1f;
            }
            if (Input.GetKey(KeyCode.X))
            {
                left_wing_control = 0.0f;
                right_wing_control = 0.0f;
                rudder_control = 0.0f;
            }
            if (Input.GetKey(KeyCode.R))
            {
                resetPlane();
            }


            // Make sure the controls are in range
            if (left_wing_control < -1.0f) left_wing_control = -1.0f;
            else if (left_wing_control > 1.0f) left_wing_control = 1.0f;
            if (right_wing_control < -1.0f) right_wing_control = -1.0f;
            else if (right_wing_control > 1.0f) right_wing_control = 1.0f;
            if (rudder_control < -1.0f) rudder_control = -1.0f;
            else if (rudder_control > 1.0f) rudder_control = 1.0f;

            // Update the control surfaces
            left_wing.setControl(left_wing_control);
            right_wing.setControl(right_wing_control);
            rudder.setControl(rudder_control);
        }
    }
}
