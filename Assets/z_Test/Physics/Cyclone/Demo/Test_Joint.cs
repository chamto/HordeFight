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

        //Sphere bone_0 = new Sphere();
        //Sphere bone_1 = new Sphere();
        //Sphere bone_2 = new Sphere();
        //Sphere bone_3 = new Sphere();

        const int SPHERE_COUNT = 4;
        Sphere[] spheres = new Sphere[SPHERE_COUNT];
        const int SPHERE_JOINT_COUNT = 3;
        Joint[] sphere_joints = new Joint[SPHERE_JOINT_COUNT];

        const int CUBE_COUNT = 3;
        Cube[] cubes = new Cube[CUBE_COUNT];
        const int CUBE_JOINT_COUNT = 2;
        Joint[] cube_joints = new Joint[CUBE_JOINT_COUNT];


        public void InitSphere()
        {
            for (int i = 0; i < SPHERE_COUNT; i++)
            {
                spheres[i] = new Sphere();
                spheres[i].setState(new Vector3(), Quaternion.identity, 1, Vector3.ZERO);
            }
            for (int i = 0; i < SPHERE_JOINT_COUNT; i++)
            {
                sphere_joints[i] = new Joint();
                sphere_joints[i].set(spheres[i].body, new Vector3(0, 0f, 3.0f), spheres[i + 1].body, new Vector3(0, 0f, -2.0f), 0.01f);
            }
        }

        public void InitCube()
        {
            for(int i =0;i< CUBE_COUNT;i++)
            {
                cubes[i] = new Cube();
                cubes[i].setState(new Vector3(), Quaternion.identity, new Vector3(1, 1, 2), Vector3.ZERO);
            }
            for (int i = 0; i < CUBE_JOINT_COUNT; i++)
            {
                cube_joints[i] = new Joint();
                cube_joints[i].set(cubes[i].body, new Vector3(0, 0f, 3.0f), cubes[i + 1].body, new Vector3(0, 0f, -2.0f), 0.01f);
            }


        }

        public JointDemo()
        {

            InitSphere();
            InitCube();
        }

        public void Fire()
        {
            DebugWide.LogBlue("Fire");


            cubes[0].body.addForceAtBodyPoint(
            new Vector3(0, 0, -1000f),
            new Vector3(0, 0, 0));
        }

        public void InputMousePt(float x, float y, float z)
        {
            //RigidBody target = cubes[0].body;
            RigidBody target = spheres[3].body;
            //target.setAwake(false);
            target.setPosition(x, y, z);
            target.setOrientation(1, 0, 0, 0);
            target.setVelocity(0, 0, 0);
            target.setRotation(0, 0, 0);
            target.setAcceleration(0, 0, 0);
            target.calculateDerivedData();

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


            //for(int i=0;i<SPHERE_COUNT;i++)
            //{
            //    CollisionDetector.sphereAndHalfSpace(spheres[i], plane, cData);
            //}
            //for(int i=0;i<CUBE_COUNT;i++)
            //{
            //    CollisionDetector.boxAndHalfSpace(cubes[i], plane, cData);
            //}

            if (!cData.hasMoreContacts()) return;
            for (int i = 0; i < SPHERE_JOINT_COUNT; i++)
            {
                Joint joint = sphere_joints[i];

                if (!cData.hasMoreContacts()) return;
                uint added = joint.addContact(cData.contacts, (uint)cData.contactsLeft);
                cData.addContacts(added);
            }


            for (int i = 0; i < CUBE_JOINT_COUNT; i++)
            {
                Joint joint = cube_joints[i];
                if (!cData.hasMoreContacts()) return;
                uint added = joint.addContact(cData.contacts, (uint)cData.contactsLeft);
                cData.addContacts(added);
            }
        }

        protected override void updateObjects(float duration)
        {

            for (int i = 0; i < SPHERE_COUNT; i++)
            {
                spheres[i].body.integrate(duration);
                spheres[i].calculateInternals();
            }

            for (int i = 0; i < CUBE_COUNT; i++)
            {
                cubes[i].body.integrate(duration);
                cubes[i].calculateInternals();
            }
        }

        public void SphereRender()
        {
            for (int i = 0; i < SPHERE_COUNT; i++)
            {
                spheres[i].render();
            }

            for (uint i = 0; i < SPHERE_JOINT_COUNT; i++)
            {
                Joint joint = sphere_joints[i];
                Vector3 a_pos = joint.body[0].getPointInWorldSpace(joint.position[0]);
                Vector3 b_pos = joint.body[1].getPointInWorldSpace(joint.position[1]);
                float length = (b_pos - a_pos).magnitude();

                Color cc;
                if (length > joint.error) cc = new Color(1, 0, 0);
                else cc = new Color(0, 1, 0);


                DebugWide.DrawLine(a_pos.ToUnity(), b_pos.ToUnity(), cc);

                //DebugWide.DrawLine(joint.body[0].getPosition().ToUnity(), joint.body[1].getPosition().ToUnity(), Color.white);
            }
        }

        public void CubeRender()
        {
            for (int i = 0; i < CUBE_COUNT; i++)
            {
                cubes[i].render();
            }

            for (uint i = 0; i < CUBE_JOINT_COUNT; i++)
            {
                Joint joint = cube_joints[i];
                Vector3 a_pos = joint.body[0].getPointInWorldSpace(joint.position[0]);
                Vector3 b_pos = joint.body[1].getPointInWorldSpace(joint.position[1]);
                float length = (b_pos - a_pos).magnitude();

                Color cc;
                if (length > joint.error) cc = new Color(1, 0, 0);
                else cc = new Color(0, 1, 0);


                DebugWide.DrawLine(a_pos.ToUnity(), b_pos.ToUnity(), cc);

                //DebugWide.DrawLine(joint.body[0].getPosition().ToUnity(), joint.body[1].getPosition().ToUnity(), Color.white);
            }
        }

        public void display()
        {
            SphereRender();

            CubeRender();

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

            //body.setCanSleep(false);
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