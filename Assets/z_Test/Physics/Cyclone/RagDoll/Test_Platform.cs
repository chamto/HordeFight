using UnityEngine;

public class Test_Platform : MonoBehaviour 
{
    Cyclone.PlatformDemo demo = null;

    private void Start()
    {
        demo = new Cyclone.PlatformDemo();
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
    public class PlatformDemo : MassAggregateApplication
    {
        const int ROD_COUNT = 15;

        const int BASE_MASS = 1;
        const int EXTRA_MASS = 10;

        ParticleRod[] rods = null;

        Vector3 massPos;
        Vector3 massDisplayPos;

        /**
         * Updates particle masses to take into account the mass
         * that's on the platform.
         */
        void updateAdditionalMass()
        {
            for (uint i = 2; i < 6; i++)
            {
                particleArray[i].setMass(BASE_MASS);
            }

            // Find the coordinates of the mass as an index and proportion
            float xp = massPos.x;
            if (xp < 0) xp = 0;
            if (xp > 1) xp = 1;

            float zp = massPos.z;
            if (zp < 0) zp = 0;
            if (zp > 1) zp = 1;

            // Calculate where to draw the mass
            massDisplayPos.clear();

            // Add the proportion to the correct masses
            particleArray[2].setMass(BASE_MASS + EXTRA_MASS * (1 - xp) * (1 - zp));
            massDisplayPos.addScaledVector(
                particleArray[2].getPosition(), (1 - xp) * (1 - zp)
                );

            if (xp > 0)
            {
                particleArray[4].setMass(BASE_MASS + EXTRA_MASS * xp * (1 - zp));
                massDisplayPos.addScaledVector(
                    particleArray[4].getPosition(), xp * (1 - zp)
                    );

                if (zp > 0)
                {
                    particleArray[5].setMass(BASE_MASS + EXTRA_MASS * xp * zp);
                    massDisplayPos.addScaledVector(
                        particleArray[5].getPosition(), xp * zp
                        );
                }
            }
            if (zp > 0)
            {
                particleArray[3].setMass(BASE_MASS + EXTRA_MASS * (1 - xp) * zp);
                massDisplayPos.addScaledVector(
                    particleArray[3].getPosition(), (1 - xp) * zp
                    );
            }
        }


        /** Creates a new demo object. */
        public PlatformDemo()
        {
            Init(6);
            massPos = new Vector3(0, 0, 0.5f);
            // Create the masses and connections.
            particleArray[0].setPosition(0, 0, 1);
            particleArray[1].setPosition(0, 0, -1);
            particleArray[2].setPosition(-3, 2, 1);
            particleArray[3].setPosition(-3, 2, -1);
            particleArray[4].setPosition(4, 2, 1);
            particleArray[5].setPosition(4, 2, -1);
            for (uint i = 0; i < 6; i++)
            {
                particleArray[i].setMass(BASE_MASS);
                particleArray[i].setVelocity(0, 0, 0);
                particleArray[i].setDamping(0.9f);
                particleArray[i].setAcceleration(Vector3.GRAVITY);
                particleArray[i].clearAccumulator();
            }

            rods = new ParticleRod[ROD_COUNT];
            for(int i=0;i< ROD_COUNT;i++ )
            {
                rods[i] = new ParticleRod(); 
            }

            rods[0].particle[0] = particleArray[0];
            rods[0].particle[1] = particleArray[1];
            rods[0].length = 2;
            rods[1].particle[0] = particleArray[2];
            rods[1].particle[1] = particleArray[3];
            rods[1].length = 2;
            rods[2].particle[0] = particleArray[4];
            rods[2].particle[1] = particleArray[5];
            rods[2].length = 2;

            rods[3].particle[0] = particleArray[2];
            rods[3].particle[1] = particleArray[4];
            rods[3].length = 7;
            rods[4].particle[0] = particleArray[3];
            rods[4].particle[1] = particleArray[5];
            rods[4].length = 7;

            rods[5].particle[0] = particleArray[0];
            rods[5].particle[1] = particleArray[2];
            rods[5].length = 3.606f;
            rods[6].particle[0] = particleArray[1];
            rods[6].particle[1] = particleArray[3];
            rods[6].length = 3.606f;

            rods[7].particle[0] = particleArray[0];
            rods[7].particle[1] = particleArray[4];
            rods[7].length = 4.472f;
            rods[8].particle[0] = particleArray[1];
            rods[8].particle[1] = particleArray[5];
            rods[8].length = 4.472f;

            rods[9].particle[0] = particleArray[0];
            rods[9].particle[1] = particleArray[3];
            rods[9].length = 4.123f;
            rods[10].particle[0] = particleArray[2];
            rods[10].particle[1] = particleArray[5];
            rods[10].length = 7.28f;
            rods[11].particle[0] = particleArray[4];
            rods[11].particle[1] = particleArray[1];
            rods[11].length = 4.899f;
            rods[12].particle[0] = particleArray[1];
            rods[12].particle[1] = particleArray[2];
            rods[12].length = 4.123f;
            rods[13].particle[0] = particleArray[3];
            rods[13].particle[1] = particleArray[4];
            rods[13].length = 7.28f;
            rods[14].particle[0] = particleArray[5];
            rods[14].particle[1] = particleArray[0];
            rods[14].length = 4.899f;

            for (uint i = 0; i < ROD_COUNT; i++)
            {
                world.getContactGenerators().Add(rods[i]);
            }

            updateAdditionalMass();
        }


        /** Display the particles. */
        public void display()
        {
            base.display();

            //glBegin(GL_LINES);
            Color cc = new Color(0, 0, 1);
            for (uint i = 0; i < ROD_COUNT; i++)
            {
                Particle[] particles = rods[i].particle;
                Vector3 p0 = particles[0].getPosition();
                Vector3 p1 = particles[1].getPosition();
                //glVertex3f(p0.x, p0.y, p0.z);
                //glVertex3f(p1.x, p1.y, p1.z);
                DebugWide.DrawLine(p0.ToUnity(), p1.ToUnity(), cc);
            }
            //glEnd();

            cc = new Color(1, 0, 0);
            //glPushMatrix();
            //glTranslatef(massDisplayPos.x, massDisplayPos.y + 0.25f, massDisplayPos.z);
            //glutSolidSphere(0.25f, 20, 10);
            //glPopMatrix();
            DebugWide.DrawSolidCircle(new UnityEngine.Vector3(massDisplayPos.x, massDisplayPos.y + 0.25f, massDisplayPos.z), 0.25f, cc);

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
            if (Input.GetKey(KeyCode.UpArrow))
            {
                massPos.z += 0.1f;
                if (massPos.z > 1.0f) massPos.z = 1.0f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                massPos.z -= 0.1f;
                if (massPos.z < 0.0f) massPos.z = 0.0f;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                massPos.x -= 0.1f;
                if (massPos.x < 0.0f) massPos.x = 0.0f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                massPos.x += 0.1f;
                if (massPos.x > 1.0f) massPos.x = 1.0f;
            }


        }
    }
}
