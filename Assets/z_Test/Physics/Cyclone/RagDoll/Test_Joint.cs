using UnityEngine;

public class Test_Joint : MonoBehaviour
{
    Cyclone.JointDemo test = null;

    Transform mousePT = null;

    private void Start()
    {
        mousePT = GameObject.Find("mousePt").transform;
        test = new Cyclone.JointDemo();


        //Vector3[] aa = new Vector3[2];
        //DebugWide.LogRed(aa[0] + "  =====");
        //TestRef(aa);
        //DebugWide.LogRed(aa[1]);
    }

    //void TestRef(Vector3[] par)
    //{
    //    par[0].y = 10f;
    //    par[1] = new Vector3(3, 4, 5);
    //}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            test.Input_PauseSimul();
            DebugWide.LogBlue("Input_PauseSimul");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            test.Fire();

        }

        test.update();
    }

    private void OnDrawGizmos()
    {
        if (null == test) return;

        Vector3 pos = mousePT.transform.position;
        test.InputMousePt(pos.x, pos.y, pos.z);


        test.display();
    }
}


namespace Cyclone
{

    public class JointDemo : RigidBodyApplication
    {

        Cube bone_0 = new Cube();
        Cube bone_1 = new Cube();
        Sphere bone_2 = new Sphere();
        Sphere bone_3 = new Sphere();

        const int NUM_JOINTS = 3;
        Joint[] joints = new Joint[NUM_JOINTS];


        public JointDemo()
        {

            for(int i=0;i<NUM_JOINTS;i++)
            {
                joints[i] = new Joint();
            }

            bone_0.setState(new Vector3(), Quaternion.identity, new Vector3(1, 1, 1), new Vector3());
            bone_1.setState(new Vector3(0,0,2), Quaternion.identity, new Vector3(1, 1, 1), new Vector3());
            bone_2.setState(new Vector3(0,0,4), Quaternion.identity, 1, new Vector3());
            bone_3.setState(new Vector3(0,0,7), Quaternion.identity, 2, new Vector3());

            joints[0].set(
                bone_0.body, new Vector3(0, 0f, 1.0f),
                bone_1.body, new Vector3(0, 0f, -1.0f),
                1.0f
                );
            joints[1].set(
                bone_1.body, new Vector3(0, 0f, 1.0f),
                bone_2.body, new Vector3(0, 0f, -1.0f),
                1.0f
                );
            joints[2].set(
                bone_2.body, new Vector3(0, 0f, 1.0f),
                bone_3.body, new Vector3(0, 0f, -1.0f),
                1.0f
                );


            //bone_0.body.addForceAtBodyPoint(
            //new Vector3(1000f, 0, 1000f),
            //new Vector3(4, 0, 0));
        }

        public void Fire()
        {
            DebugWide.LogBlue("Fire");


            bone_0.body.addForceAtBodyPoint(
            new Vector3(0, 0, -1000f),
            new Vector3(0, 0, 0));
        }

        public void InputMousePt(float x, float y, float z)
        {

            bone_0.body.setPosition(x, y, z);
            bone_0.body.calculateDerivedData();

        }

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


            if (!cData.hasMoreContacts()) return;
            CollisionDetector.boxAndHalfSpace(bone_0, plane, cData);
            CollisionDetector.boxAndHalfSpace(bone_1, plane, cData);
            CollisionDetector.sphereAndHalfSpace(bone_2, plane, cData);
            CollisionDetector.sphereAndHalfSpace(bone_3, plane, cData);

            //if (!cData.hasMoreContacts()) return;
            //CollisionSphere boneSphere_0 = bone_0.getCollisionSphere();
            //CollisionSphere boneSphere_1 = bone_1.getCollisionSphere();
            //CollisionDetector.sphereAndSphere(
                        //boneSphere_0,
                        //boneSphere_1,
                        //cData
                        //);

            if (!cData.hasMoreContacts()) return;
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                Joint joint = joints[i];

                if (!cData.hasMoreContacts()) return;
                uint added = joint.addContact(cData.contacts, (uint)cData.contactsLeft);
                cData.addContacts(added);
            }


        }

        protected override void updateObjects(float duration)
        {

            bone_0.body.integrate(duration);
            bone_0.calculateInternals();
            bone_1.body.integrate(duration);
            bone_1.calculateInternals();
            bone_2.body.integrate(duration);
            bone_2.calculateInternals();
            bone_3.body.integrate(duration);
            bone_3.calculateInternals();

        }

        public void display()
        {
            bone_0.render();
            bone_1.render();
            bone_2.render();
            bone_3.render();

            for (uint i = 0; i < NUM_JOINTS; i++)
            {
                Joint joint = joints[i];
                Vector3 a_pos = joint.body[0].getPointInWorldSpace(joint.position[0]);
                Vector3 b_pos = joint.body[1].getPointInWorldSpace(joint.position[1]);
                float length = (b_pos - a_pos).magnitude();

                Color cc;
                if (length > joint.error) cc = new Color(1, 0, 0);
                else cc = new Color(0, 1, 0);


                UnityEngine.Vector3 start = new UnityEngine.Vector3(a_pos.x, a_pos.y, a_pos.z);
                UnityEngine.Vector3 end = new UnityEngine.Vector3(b_pos.x, b_pos.y, b_pos.z);
                DebugWide.DrawLine(start, end, cc);
            }
        }


    }

    public class Sphere : CollisionSphere
    {

        public Sphere()
        {
            body = new RigidBody();
        }


        /** Draws the box, excluding its shadow. */
        public void render()
        {
            Color cc;

            if (body.getAwake()) cc = new Color(1.0f, 0.7f, 0.7f);
            else cc = new Color(0.7f, 0.7f, 1.0f);


            Vector3 pos = body.getPosition();
            UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            DebugWide.DrawCircle(u_pos, radius, cc);
            //DebugWide.DrawSolidCircle(u_pos, radius, cc);
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


    public class Cube : CollisionBox
    {

        public bool isOverlapping;

        public Cube()
        {
            body = new RigidBody();
        }


        /** Draws the box, excluding its shadow. */
        public void render()
        {
            Color cc;

            if (isOverlapping) cc = new Color(0.7f, 1.0f, 0.7f);
            else if (body.getAwake()) cc = new Color(1.0f, 0.7f, 0.7f);
            else cc = new Color(0.7f, 0.7f, 1.0f);


            //Matrix4 tr = body.getTransform();
            //UnityEngine.Matrix4x4 u_tr = Matrix4x4.identity;
            //u_tr.m00 = tr.m00; u_tr.m01 = tr.m01; u_tr.m02 = tr.m02; u_tr.m03 = tr.m03;
            //u_tr.m10 = tr.m10; u_tr.m11 = tr.m11; u_tr.m12 = tr.m12; u_tr.m13 = tr.m13;
            //u_tr.m20 = tr.m20; u_tr.m21 = tr.m21; u_tr.m22 = tr.m22; u_tr.m23 = tr.m23;
            //UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(tr.m03, tr.m13, tr.m23);
            //UnityEngine.Quaternion u_rot = u_tr.rotation;


            Vector3 pos = body.getPosition();
            Quaternion rot = body.getOrientation();
            Vector3 size = new Vector3(halfSize.x * 2, halfSize.y * 2, halfSize.z * 2);


            UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            UnityEngine.Quaternion u_rot = new UnityEngine.Quaternion(rot.i, rot.j, rot.k, rot.r);
            UnityEngine.Vector3 u_size = new UnityEngine.Vector3(size.x, size.y, size.z);

            //DebugWide.DrawSolidCube(u_pos, u_rot, u_size, cc);
            DebugWide.DrawCube(u_pos, u_rot, u_size, cc);
            //DebugWide.DrawSphere(u_pos, halfSize.x, cc);
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
}