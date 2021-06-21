using UnityEngine;

public class Test_ParticleJoint : MonoBehaviour 
{
    Cyclone.ParticleJointDemo demo = null;

    Transform mousePT = null;

    private void Start()
    {
        demo = new Cyclone.ParticleJointDemo();
        mousePT = GameObject.Find("mousePt").transform;
    }

    private void Update()
    {
        demo.InputPos(mousePT.position.x, mousePT.position.y, mousePT.position.z);
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
    public class ParticleJointDemo : MassAggregateApplication
    {
        const int CABLE_COUNT = 3;
        const int ROD_COUNT = 3;

        const int BASE_MASS = 1;
        const int EXTRA_MASS = 10;

        const int SUPPORT_COUNT = 1;
        ParticleRodConstraint[] supports = null;
        ParticleCable[] cables = null;
        ParticleRod[] rods = null;

        const int PARTICLE_COUNT = 10;


        public ParticleJointDemo()
        {
            Init(PARTICLE_COUNT);


            // Create the masses and connections.
            for (uint i = 0; i < PARTICLE_COUNT; i++)
            {
                uint x = (i % PARTICLE_COUNT) / 2;
                particleArray[i].setPosition(
                    i * 5,
                    6,
                    0
                    );
                particleArray[i].setMass(BASE_MASS);
                particleArray[i].setVelocity(0, 0, 0);
                particleArray[i].setDamping(0.9f);
                particleArray[i].setAcceleration(Vector3.GRAVITY);
                particleArray[i].clearAccumulator();
            }

            //Cable 테스트 , 떨림이 비정상적으로 발생함. 엔진에 원래 결함이 있는것 같음 
            //particleArray[0].setAcceleration(Vector3.ZERO);
            //particleArray[3].setMass(10); 

            supports = new ParticleRodConstraint[SUPPORT_COUNT];
            for (uint i = 0; i < SUPPORT_COUNT; i++)
            {
                supports[i] = new ParticleRodConstraint();
                supports[i].particle = particleArray[0]; //0 , 4
                supports[i].anchor = new Vector3(
                    0,
                    6,
                    0
                    );
                supports[i].length = 1f;
                //supports[i].maxLength = 3f;
                //supports[i].restitution = 0.5f;
                world.getContactGenerators().Add(supports[i]);
            }

            //0 ~ 3
            // Add the links
            cables = new ParticleCable[CABLE_COUNT];
            for (uint i = 0; i < CABLE_COUNT; i++)
            {
                cables[i] = new ParticleCable();
                cables[i].particle[0] = particleArray[i];
                cables[i].particle[1] = particleArray[i + 1];
                cables[i].maxLength = 2f;
                cables[i].restitution = 0.5f;
                world.getContactGenerators().Add(cables[i]);
            }

            //4 ~ 7
            int next = CABLE_COUNT+1;
            rods = new ParticleRod[ROD_COUNT];
            for (uint i = 0; i < ROD_COUNT; i++)
            {
                rods[i] = new ParticleRod();
                rods[i].particle[0] = particleArray[i + next];
                rods[i].particle[1] = particleArray[i + 1 + next];
                rods[i].length = 2;
                world.getContactGenerators().Add(rods[i]);
            }



        }

        public void display()
        {
            base.display();

            //glBegin(GL_LINES);
            //glColor3f(0, 0, 1);
            Color cc = new Color(0, 0, 1);
            for (uint i = 0; i < ROD_COUNT; i++)
            {
                Particle[] particles = rods[i].particle;
                Vector3 p0 = particles[0].getPosition();
                Vector3 p1 = particles[1].getPosition();
                DebugWide.DrawLine(p0.ToUnity(), p1.ToUnity(), cc);
                //glVertex3f(p0.x, p0.y, p0.z);
                //glVertex3f(p1.x, p1.y, p1.z);
            }

            cc = new Color(0, 1, 0);
            for (uint i = 0; i < CABLE_COUNT; i++)
            {
                Particle[] particles = cables[i].particle;
                Vector3 p0 = particles[0].getPosition();
                Vector3 p1 = particles[1].getPosition();
                DebugWide.DrawLine(p0.ToUnity(), p1.ToUnity(), cc);
                //glVertex3f(p0.x, p0.y, p0.z);
                //glVertex3f(p1.x, p1.y, p1.z);
            }

            cc = new Color(0.7f, 0.7f, 0.7f);
            for (uint i = 0; i < SUPPORT_COUNT; i++)
            {
                Vector3 p0 = supports[i].particle.getPosition();
                Vector3 p1 = supports[i].anchor;
                DebugWide.DrawLine(p0.ToUnity(), p1.ToUnity(), cc);
                //glVertex3f(p0.x, p0.y, p0.z);
                //glVertex3f(p1.x, p1.y, p1.z);
            }

        }

        /** Update the particle positions. */
        public override void update()
        {
            base.update();
        }

        UnityEngine.Vector3 _u_dir = new UnityEngine.Vector3(0,1,0);
        public void key()
        {
            if (Input.GetKey(KeyCode.A))
            {
                _u_dir = UnityEngine.Quaternion.AngleAxis(15, new UnityEngine.Vector3(0,0,1)) * _u_dir;

                Vector3 dir = new Vector3(_u_dir.x, _u_dir.y, _u_dir.z );
                particleArray[4].setPosition(supports[0].anchor + dir * 5f);

            }

        }

        public void InputPos(float x,float y,float z)
        {

            supports[0].anchor = new Vector3(x, y, z);
            //particleArray[0].setPosition(x, y, z);

            this._viewVelocity = true;
            //particleArray[4].setAcceleration(Vector3.ZERO);
            //particleArray[4].setPosition(x, y, z);
        }
    }
}
