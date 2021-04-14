using System;
using UnityEngine;

public class Test_Bridge : MonoBehaviour 
{

    Cyclone.BridgeDemo demo = null; 

    private void Start()
    {
        demo = new Cyclone.BridgeDemo();
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
    public class BridgeDemo : MassAggregateApplication
    {
        const int ROD_COUNT  = 6;
        const int CABLE_COUNT = 10;
        const int SUPPORT_COUNT = 12;

        const int BASE_MASS = 1;
        const int EXTRA_MASS = 10;

        ParticleCableConstraint[] supports = null;
        ParticleCable[] cables = null;
        ParticleRod[] rods = null;

        Vector3 massPos;
        Vector3 massDisplayPos;

        const int PARTICLE_COUNT = 12;

        /**
         * Updates particle masses to take into account the mass
         * that's crossing the bridge.
         */
        //ref : https://m.blog.naver.com/PostView.nhn?blogId=herbbread&logNo=90191656950&proxyReferer=https:%2F%2Fwww.google.com%2F
        //c# 에서의 나머지 연산자는 float , double 에도 적용된다
        public void updateAdditionalMass()
        {
            for (uint i = 0; i < PARTICLE_COUNT; i++)
            {
                particleArray[i].setMass(BASE_MASS);
            }

            // Find the coordinates of the mass as an index and proportion
            int x = (int)massPos.x;
            //float xp = real_fmod(massPos.x, cyclone::real(1.0f));
            float xp = massPos.x % 1f;
            if (x < 0)
            {
                x = 0;
                xp = 0;
            }
            if (x >= 5)
            {
                x = 5;
                xp = 0;
            }

            int z = (int)massPos.z;
            //float zp = real_fmod(massPos.z, cyclone::real(1.0f));
            float zp = massPos.z % 1f;
            if (z < 0)
            {
                z = 0;
                zp = 0;
            }
            if (z >= 1)
            {
                z = 1;
                zp = 0;
            }

            // Calculate where to draw the mass
            massDisplayPos.clear();

            // Add the proportion to the correct masses
            particleArray[x * 2 + z].setMass(BASE_MASS + EXTRA_MASS * (1 - xp) * (1 - zp));
            massDisplayPos.addScaledVector(
                particleArray[x * 2 + z].getPosition(), (1 - xp) * (1 - zp)
                );

            if (xp > 0)
            {
                particleArray[x * 2 + z + 2].setMass(BASE_MASS + EXTRA_MASS * xp * (1 - zp));
                massDisplayPos.addScaledVector(
                    particleArray[x * 2 + z + 2].getPosition(), xp * (1 - zp)
                    );

                if (zp > 0)
                {
                    particleArray[x * 2 + z + 3].setMass(BASE_MASS + EXTRA_MASS * xp * zp);
                    massDisplayPos.addScaledVector(
                        particleArray[x * 2 + z + 3].getPosition(), xp * zp
                        );
                }
            }
            if (zp > 0)
            {
                particleArray[x * 2 + z + 1].setMass(BASE_MASS + EXTRA_MASS * (1 - xp) * zp);
                massDisplayPos.addScaledVector(
                    particleArray[x * 2 + z + 1].getPosition(), (1 - xp) * zp
                    );
            }
        }


        /** Creates a new demo object. */
        public BridgeDemo() //: base(12)
        {
            Init(PARTICLE_COUNT);

            massPos = new Vector3(0, 0, 0.5f);

            // Create the masses and connections.
            for (uint i = 0; i < PARTICLE_COUNT; i++)
            {
                uint x = (i % PARTICLE_COUNT) / 2;
                particleArray[i].setPosition(
                    (float)(i / 2) * 2.0f - 5.0f,
                    4,
                    (float)(i % 2) * 2.0f - 1.0f
                    );
                particleArray[i].setVelocity(0, 0, 0);
                particleArray[i].setDamping(0.9f);
                particleArray[i].setAcceleration(Vector3.GRAVITY);
                particleArray[i].clearAccumulator();
            }

            // Add the links
            cables = new ParticleCable[CABLE_COUNT];
            for (uint i = 0; i < CABLE_COUNT; i++)
            {
                cables[i] = new ParticleCable();
                cables[i].particle[0] = particleArray[i];
                cables[i].particle[1] = particleArray[i + 2];
                cables[i].maxLength = 1.9f;
                cables[i].restitution = 0.3f;
                world.getContactGenerators().Add(cables[i]);
            }

            supports = new ParticleCableConstraint[SUPPORT_COUNT];
            for (uint i = 0; i < SUPPORT_COUNT; i++)
            {
                supports[i] = new ParticleCableConstraint();
                supports[i].particle = particleArray[i];
                supports[i].anchor = new Vector3(
                    (i / 2) * 2.2f - 5.5f,
                    6,
                    (i % 2) * 1.6f - 0.8f
                    );
                if (i < 6) supports[i].maxLength = (i / 2) * 0.5f + 3.0f;
                else supports[i].maxLength = 5.5f - (i / 2) * 0.5f;
                supports[i].restitution = 0.5f;
                world.getContactGenerators().Add(supports[i]);
            }

            rods = new ParticleRod[ROD_COUNT];
            for (uint i = 0; i < ROD_COUNT; i++)
            {
                rods[i] = new ParticleRod();
                rods[i].particle[0] = particleArray[i * 2];
                rods[i].particle[1] = particleArray[i * 2 + 1];
                rods[i].length = 2;
                world.getContactGenerators().Add(rods[i]);
            }

            updateAdditionalMass();
        }



        /** Display the particles. */
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
            //glEnd();

            cc = new Color(1, 0, 0);
            //glPushMatrix();
            //glTranslatef(massDisplayPos.x, massDisplayPos.y + 0.25f, massDisplayPos.z);
            //glutSolidSphere(0.25f, 20, 10);
            DebugWide.DrawSolidCircle(massDisplayPos.ToUnity(), 0.25f, cc);
            //glPopMatrix();
        }

        /** Update the particle positions. */
        public override void update()
        {
            base.update();

            updateAdditionalMass();
        }

        /** Handle a key press. */
        public void key()
        {
            if(Input.GetKey(KeyCode.S))
            {
                massPos.z += 0.1f;
                if (massPos.z > 1.0f) massPos.z = 1.0f;
            }
            if(Input.GetKey(KeyCode.W))
            {
                massPos.z -= 0.1f;
                if (massPos.z < 0.0f) massPos.z = 0.0f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                massPos.x -= 0.1f;
                if (massPos.x < 0.0f) massPos.x = 0.0f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                massPos.x += 0.1f;
                if (massPos.x > 5.0f) massPos.x = 5.0f;
            }

        }
    }
}
