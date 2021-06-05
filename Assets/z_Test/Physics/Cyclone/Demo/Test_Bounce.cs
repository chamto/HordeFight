using UnityEngine;

public class Test_Bounce : MonoBehaviour
{
    Cyclone.BounceDemo demo = null;
    Transform t_pos = null;

    public float Friction = 0.9f;
    public float Restitution = 0.8f;
    public float Tolerance = 0.1f;

    private void Start()
    {
        demo = new Cyclone.BounceDemo();
        //t_pos = GameObject.Find("t_pos").transform;
    }

    private void Update()
    {
        demo.Friction = Friction;
        demo.Restitution = Restitution;
        demo.Tolerance = Tolerance;

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
    public class BounceDemo : RigidBodyApplication
    {
        CollisionSphere sphere = null;

        public BounceDemo() : base()
        {
            Init();
        }

        public void Init()
        {
            sphere = new CollisionSphere();
            sphere.body = new RigidBody();
            sphere.body.setMass(200.0f); // 200.0kg
            sphere.body.setVelocity(0.0f, 8.0f, 5.0f);
            sphere.body.setAcceleration(Vector3.GRAVITY);
            //sphere.body.setDamping(0.99f, 0.8f);
            sphere.body.setDamping(0.9f, 0.8f); //감쇠 
            sphere.radius = 0.4f;
            sphere.body.setCanSleep(false);
            sphere.body.setAwake(true);

            Matrix3 tensor = Matrix3.identityMatrix;
            float coeff = 0.4f * sphere.body.getMass() * sphere.radius * sphere.radius;
            tensor.setInertiaTensorCoeffs(coeff, coeff, coeff);
            sphere.body.setInertiaTensor(tensor);
            sphere.body.setPosition(0.0f, 1.5f, 0.0f);

            // Clear the force accumulators
            sphere.body.calculateDerivedData();
            sphere.calculateInternals();
        }

        protected override void updateObjects(float duration)
        {
            sphere.body.integrate(duration);
            sphere.calculateInternals();
        }

        public float Friction = 0.9f;
        public float Restitution = 0.8f;
        public float Tolerance = 0.1f;
        protected override void generateContacts()
        {
            // Create the ground plane data
            CollisionPlane plane = new CollisionPlane(); ;
            plane.direction = new Vector3(0, 1, 0);
            plane.offset = 0;

            // Set up the collision data structure
            cData.reset(maxContacts);
            cData.friction = Friction; //마찰력
            cData.restitution = Restitution; //반반력 , 탄성계수? 
            cData.tolerance = Tolerance;

            CollisionDetector.sphereAndHalfSpace(sphere, plane, cData);
        }

        public void display()
        {
            DebugWide.DrawCircle(sphere.body.getPosition().ToUnity(), sphere.radius, Color.blue);
        }

        public void key()
        {
            base.key();

            if(Input.GetKeyDown(KeyCode.R))
            {
                Init();
            }
        }
    }
}