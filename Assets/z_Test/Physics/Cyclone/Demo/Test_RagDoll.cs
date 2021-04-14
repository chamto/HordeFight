using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_RagDoll : MonoBehaviour 
{

    Cyclone.RagdollDemo demo = null;

    Transform mousePT = null;

    // Use this for initialization
    void Start () 
    {
        mousePT = GameObject.Find("mousePt").transform;
        demo = new Cyclone.RagdollDemo();
    }


    private void Update()
    {

        Vector3 pos = mousePT.transform.position;
        //demo.InputMousePt(pos.x, pos.y, pos.z);

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

    public class RagdollDemo : RigidBodyApplication
    {
        const int NUM_BONES = 12;
        const int NUM_JOINTS = 11;

        //Random random = new Random();

        /** Holds the bone bodies. */
        Bone[] bones = new Bone[NUM_BONES];

        /** Holds the joints. */
        Joint[] joints = new Joint[NUM_JOINTS];

        /** Processes the contact generation code. */
        protected override void generateContacts()
        {
            // Create the ground plane data
            CollisionPlane plane = new CollisionPlane();
            plane.direction = new Cyclone.Vector3(0, 1, 0);
            plane.offset = 0;

            // Set up the collision data structure
            cData.reset(maxContacts);
            cData.friction = 0.9f;
            cData.restitution = 0.6f;
            cData.tolerance = 0.1f;

            // Perform exhaustive collision detection on the ground plane
            Matrix4 transform, otherTransform;
            Vector3 position, otherPosition;
            //for (Bone* bone = bones; bone < bones + NUM_BONES; bone++)
            for (int i = 0; i < NUM_BONES; i++)
            {
                Bone bone = bones[i];

                // Check for collisions with the ground plane
                if (!cData.hasMoreContacts()) return;
                CollisionDetector.boxAndHalfSpace(bone, plane, cData);

                CollisionSphere boneSphere = bone.getCollisionSphere();

                // Check for collisions with each other box
                //for (Bone* other = bone + 1; other < bones + NUM_BONES; other++)
                for (int j = i+1; j < NUM_BONES; j++)
                {
                    Bone other = bones[j];

                    if (!cData.hasMoreContacts()) return;

                    CollisionSphere otherSphere = other.getCollisionSphere();

                    CollisionDetector.sphereAndSphere(
                        boneSphere,
                        otherSphere,
                        cData
                        );
                }
            }

            // Check for joint violation
            //for (Joint* joint = joints; joint < joints + NUM_JOINTS; joint++)
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                Joint joint = joints[i];

                if (!cData.hasMoreContacts()) return;
                uint added = joint.addContact(cData.contacts, (uint)cData.contactsLeft);
                cData.addContacts(added);
            }
        }

        /** Processes the objects in the simulation forward in time. */
        protected override void updateObjects(float duration)
        {
            //for (Bone* bone = bones; bone < bones + NUM_BONES; bone++)
            for (int i = 0; i < NUM_BONES; i++)
            {
                Bone bone = bones[i];

                bone.body.integrate(duration);
                bone.calculateInternals();
            }
        }

        /** Resets the position of all the bones. */
        public void reset()
        {
            bones[0].setState(
                new Vector3(0, 0.993f, -0.5f),
                new Vector3(0.301f, 1.0f, 0.234f)); //다리 
            bones[1].setState(
                new Vector3(0, 3.159f, -0.56f),
                new Vector3(0.301f, 1.0f, 0.234f));
            bones[2].setState(
                new Vector3(0, 0.993f, 0.5f),
                new Vector3(0.301f, 1.0f, 0.234f)); //다리 
            bones[3].setState(
                new Vector3(0, 3.15f, 0.56f),
                new Vector3(0.301f, 1.0f, 0.234f));

            bones[4].setState(
                new Vector3(-0.054f, 4.683f, 0.013f),
                new Vector3(0.415f, 0.392f, 0.690f));
            bones[5].setState(
                new Vector3(0.043f, 5.603f, 0.013f),
                new Vector3(0.301f, 0.367f, 0.693f)); //몸통 
            bones[6].setState(
                new Vector3(0, 6.485f, 0.013f),
                new Vector3(0.435f, 0.367f, 0.786f));

            bones[7].setState(
                new Vector3(0, 7.759f, 0.013f),
                new Vector3(0.45f, 0.598f, 0.421f)); //머리

            bones[8].setState(
                new Vector3(0, 5.946f, -1.066f),
                new Vector3(0.267f, 0.888f, 0.207f));
            bones[9].setState(
                new Vector3(0, 4.024f, -1.066f),
                new Vector3(0.267f, 0.888f, 0.207f)); //팔
            bones[10].setState(
                new Vector3(0, 5.946f, 1.066f),
                new Vector3(0.267f, 0.888f, 0.207f));
            bones[11].setState(
                new Vector3(0, 4.024f, 1.066f),
                new Vector3(0.267f, 0.888f, 0.207f)); //팔

            float strength = (float)-random.randomReal(100.0f, 400.0f);
            for (uint i = 0; i < NUM_BONES; i++)
            {
                bones[i].body.addForceAtBodyPoint(
                    new Vector3(strength, 0, 0), new Vector3()
                    );
            }
            bones[6].body.addForceAtBodyPoint(
                new Vector3(strength, 0, (float)random.randomBinomial(1000.0f)),
                new Vector3((float)random.randomBinomial(4.0f), (float)random.randomBinomial(3.0f), 0)
                );

            // Reset the contacts
            cData.contactCount = 0;
        }


        /** Creates a new demo object. */
        public RagdollDemo()
        {
            // Set up the bone hierarchy.
            for (int i = 0; i < NUM_BONES; i++)
            {
                bones[i] = new Bone();
            }

            for (int i=0;i< NUM_JOINTS;i++)
            {
                joints[i] = new Joint();
            }

            // Right Knee
            joints[0].set(
                bones[0].body, new Vector3(0, 1.07f, 0),
                bones[1].body, new Vector3(0, -1.07f, 0),
                0.15f
                );

            // Left Knee
            joints[1].set(
                bones[2].body, new Vector3(0, 1.07f, 0),
                bones[3].body, new Vector3(0, -1.07f, 0),
                0.15f
                );

            // Right elbow
            joints[2].set(
                bones[9].body, new Vector3(0, 0.96f, 0),
                bones[8].body, new Vector3(0, -0.96f, 0),
                0.15f
                );

            // Left elbow
            joints[3].set(
                bones[11].body, new Vector3(0, 0.96f, 0),
                bones[10].body, new Vector3(0, -0.96f, 0),
                0.15f
                );

            // Stomach to Waist
            joints[4].set(
                bones[4].body, new Vector3(0.054f, 0.50f, 0),
                bones[5].body, new Vector3(-0.043f, -0.45f, 0),
                0.15f
                );

            joints[5].set(
                bones[5].body, new Vector3(-0.043f, 0.411f, 0),
                bones[6].body, new Vector3(0, -0.411f, 0),
                0.15f
                );

            joints[6].set(
                bones[6].body, new Vector3(0, 0.521f, 0),
                bones[7].body, new Vector3(0, -0.752f, 0),
                0.15f
                );

            // Right hip
            joints[7].set(
                bones[1].body, new Vector3(0, 1.066f, 0),
                bones[4].body, new Vector3(0, -0.458f, -0.5f),
                0.15f
                );

            // Left Hip
            joints[8].set(
                bones[3].body, new Vector3(0, 1.066f, 0),
                bones[4].body, new Vector3(0, -0.458f, 0.5f),
                0.105f
                );

            // Right shoulder
            joints[9].set(
                bones[6].body, new Vector3(0, 0.367f, -0.8f),
                bones[8].body, new Vector3(0, 0.888f, 0.32f),
                0.15f
                );

            // Left shoulder
            joints[10].set(
                bones[6].body,  new Vector3(0, 0.367f, 0.8f),
                bones[10].body, new Vector3(0, 0.888f, -0.32f),
                0.15f
                );

            // Set up the initial positions
            reset();
        }

        public void InputMousePt(float x, float y, float z)
        {

            bones[7].body.setPosition(x, y, z);
            bones[7].body.calculateDerivedData();

        }

        /** Display the particle positions. */
        public void display()
        {
            //const static GLfloat lightPosition[] = { 0.7f, -1, 0.4f, 0 };
            //const static GLfloat lightPositionMirror[] = { 0.7f, 1, 0.4f, 0 };

            //RigidBodyApplication::display();

            // Render the bones
            //glEnable(GL_DEPTH_TEST);
            //glEnable(GL_LIGHTING);
            //glEnable(GL_LIGHT0);

            //glLightfv(GL_LIGHT0, GL_POSITION, lightPosition);
            //glColorMaterial(GL_FRONT_AND_BACK, GL_DIFFUSE);
            //glEnable(GL_COLOR_MATERIAL);

            //glEnable(GL_NORMALIZE);
            //glColor3f(1, 0, 0);
            for (uint i = 0; i < NUM_BONES; i++)
            {
                bones[i].render();
            }
            //glDisable(GL_NORMALIZE);

            //glDisable(GL_LIGHTING);
            //glDisable(GL_COLOR_MATERIAL);

            //glDisable(GL_DEPTH_TEST);
            //glBegin(GL_LINES);
            for (uint i = 0; i < NUM_JOINTS; i++)
            {
                Joint joint = joints[i];
                Vector3 a_pos = joint.body[0].getPointInWorldSpace(joint.position[0]);
                Vector3 b_pos = joint.body[1].getPointInWorldSpace(joint.position[1]);
                float length = (b_pos - a_pos).magnitude();

                Color cc;
                if (length > joint.error) cc = new Color(1, 0, 0);
                else cc = new Color(0, 1, 0);

                //glVertex3f(a_pos.x, a_pos.y, a_pos.z);
                //glVertex3f(b_pos.x, b_pos.y, b_pos.z);

                UnityEngine.Vector3 start = new UnityEngine.Vector3(a_pos.x, a_pos.y, a_pos.z);
                UnityEngine.Vector3 end = new UnityEngine.Vector3(b_pos.x, b_pos.y, b_pos.z);
                DebugWide.DrawLine(start, end, cc);
            }
            //glEnd();
            //glEnable(GL_DEPTH_TEST);

            UnityEngine.Vector3 prev = new UnityEngine.Vector3();
            UnityEngine.Vector3 cur = new UnityEngine.Vector3();

            // Draw some scale circles
            //glColor3f(0.75, 0.75, 0.75);
            for (uint i = 1; i < 20; i++)
            {

                //glBegin(GL_LINE_LOOP);
                for (uint j = 0; j < 32; j++)
                {
                    float theta = 3.1415926f * j / 16.0f;
                    //glVertex3f(i * cosf(theta), 0.0f, i * sinf(theta));

                    cur.x = i * (float)Math.Cos(theta);
                    cur.z = i * (float)Math.Sin(theta);
                    //DebugWide.DrawLine(prev, cur, Color.white);
                    prev = cur;
                }
                //glEnd();
            }
            //glBegin(GL_LINES);
            //glVertex3f(-20, 0, 0);
            //glVertex3f(20, 0, 0);
            //glVertex3f(0, 0, -20);
            //glVertex3f(0, 0, 20);
            //glEnd();

            base.drawDebug();
        }
    }
}


namespace Cyclone
{

    public class Bone : CollisionBox
    {

        public Bone()
        {
            body = new RigidBody();
        }

        //public ~Bone()
        //{
        //    delete body;
        //}

        /**
         * We use a sphere to collide bone on bone to allow some limited
         * interpenetration.
         */
        public CollisionSphere getCollisionSphere()
        {
            CollisionSphere sphere = new CollisionSphere();
            sphere.body = body;
            sphere.radius = halfSize.x;
            sphere.offset = Matrix4.identityMatrix;
            if (halfSize.y<sphere.radius) sphere.radius = halfSize.y;
            if (halfSize.z<sphere.radius) sphere.radius = halfSize.z;
            sphere.calculateInternals();
            return sphere;
        }

        /** Draws the bone. */
        public void render()
        {
            // Get the OpenGL transformation
            //GLfloat mat[16];
            //body.getGLTransform(mat);

            //if (body.getAwake()) glColor3f(0.5f, 0.3f, 0.3f);
            //else glColor3f(0.3f, 0.3f, 0.5f);

            //glPushMatrix();
            //glMultMatrixf(mat);
            //glScalef(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);
            //glutSolidCube(1.0f);
            //glPopMatrix();

            Vector3 pos = body.getPosition();
            Quaternion rot = body.getOrientation();
            Vector3 size = new Vector3(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);

            UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            UnityEngine.Quaternion u_rot = new UnityEngine.Quaternion(rot.i, rot.j, rot.k, rot.r);
            UnityEngine.Vector3 u_size = new UnityEngine.Vector3(size.x, size.y, size.z);

            DebugWide.DrawCube(u_pos, u_rot, u_size, Color.blue);
        }

        /** Sets the bone to a specific location. */
        public void setState(Vector3 position, Vector3 extents)
        {
            body.setPosition(position);
            body.setOrientation(Quaternion.identity);
            body.setVelocity(Vector3.ZERO);
            body.setRotation(Vector3.ZERO);
            halfSize = extents;

            float mass = halfSize.x * halfSize.y * halfSize.z * 8.0f;
            body.setMass(mass);

            Matrix3 tensor = Matrix3.identityMatrix;
            tensor.setBlockInertiaTensor(halfSize, mass);
            body.setInertiaTensor(tensor);

            body.setLinearDamping(0.95f);
            body.setAngularDamping(0.8f);
            body.clearAccumulators();
            body.setAcceleration(Vector3.GRAVITY);

            body.setCanSleep(false);
            body.setAwake(true);

            body.calculateDerivedData();
            calculateInternals();
        }

    }
}
