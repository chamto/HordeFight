using UnityEngine;
using System;
using System.Diagnostics;

public class Test_Ballistic : MonoBehaviour
{
    Cyclone.BallisticDemo demo = null;

    private void Start()
    {
        demo = new Cyclone.BallisticDemo();
    }

    private void OnDrawGizmos()
    {
        //DebugWide.DrawSphere(Vector3.zero, 1f, Color.white);

        if (null == demo) return;



        demo.key();
        demo.update();
        demo.display();
    }

}

namespace Cyclone
{

    public class BallisticDemo //: Application
    {
        public enum ShotType
        {
            UNUSED = 0,
            PISTOL,
            ARTILLERY,
            FIREBALL,
            LASER
        }

        /**
         * Holds a single ammunition round record.
         */
        public struct AmmoRound
        {
            public Particle particle;
            public ShotType type;
            public uint startTime;

            /** Draws the round. */
            public void render()
            {
                Vector3 position;
                particle.getPosition(out position);


                //glColor3f(0, 0, 0);
                //glPushMatrix();
                //glTranslatef(position.x, position.y, position.z);
                //glutSolidSphere(0.3f, 5, 4);
                //glPopMatrix();

                Color cc; //= Color.black;
                cc = new Color(0, 0, 0);
                UnityEngine.Vector3 u_pos = new UnityEngine.Vector3(position.x, position.y, position.z);
                DebugWide.DrawSphere(u_pos, 0.3f, cc);
                //DebugWide.DrawCircle(u_pos, 1f, cc);

                //DebugWide.LogBlue(position);
                //glColor3f(0.75, 0.75, 0.75);
                //glPushMatrix();
                //glTranslatef(position.x, 0, position.z);
                //glScalef(1.0f, 0.1f, 1.0f);
                //glutSolidSphere(0.6f, 5, 4);
                //glPopMatrix();

                cc = new Color(0.75f, 0.75f, 0.75f);
                u_pos.y = 0;
                DebugWide.DrawSphere(u_pos, 0.6f, cc);
            }
        }

        /**
         * Holds the maximum number of  rounds that can be
         * fired.
         */
        const uint ammoRounds = 16;

        /** Holds the particle data. */
        AmmoRound[] ammo = new AmmoRound[ammoRounds];

        /** Holds the current shot type. */
        ShotType currentShotType;

        Stopwatch stopWatch = Stopwatch.StartNew(); //new Stopwatch();

        /** Dispatches a round. */
        void fire()
        {
            // Find the first available round.
            AmmoRound shot = ammo[0];
            //for (shot = ammo; shot < ammo + ammoRounds; shot++)
            int count = 0;
            for (; count < ammoRounds; count++)
            {
                //DebugWide.LogBlue(count + " --1-- " + ammo[count].type);
                shot = ammo[count];
                //DebugWide.LogBlue(count +" --2-- "+ shot.type);
                if (shot.type == ShotType.UNUSED) break;
            }

            DebugWide.LogRed(count);

            // If we didn't find a round, then exit - we can't fire.
            //if (shot >= ammo + ammoRounds) return;
            if (count >= ammoRounds) return;

            // Set the properties of the particle
            switch (currentShotType)
            {
                case ShotType.PISTOL:
                    shot.particle.setMass(2.0f); // 2.0kg
                    shot.particle.setVelocity(0.0f, 0.0f, 35.0f); // 35m/s
                    shot.particle.setAcceleration(0.0f, -1.0f, 0.0f);
                    shot.particle.setDamping(0.99f);
                    break;

                case ShotType.ARTILLERY:
                    shot.particle.setMass(200.0f); // 200.0kg
                    shot.particle.setVelocity(0.0f, 30.0f, 40.0f); // 50m/s
                    shot.particle.setAcceleration(0.0f, -20.0f, 0.0f);
                    shot.particle.setDamping(0.99f);
                    break;

                case ShotType.FIREBALL:
                    shot.particle.setMass(1.0f); // 1.0kg - mostly blast damage
                    shot.particle.setVelocity(0.0f, 0.0f, 10.0f); // 5m/s
                    shot.particle.setAcceleration(0.0f, 0.6f, 0.0f); // Floats up
                    shot.particle.setDamping(0.9f);
                    break;

                case ShotType.LASER:
                    // Note that this is the kind of laser bolt seen in films,
                    // not a realistic laser beam!
                    shot.particle.setMass(0.1f); // 0.1kg - almost no weight
                    shot.particle.setVelocity(0.0f, 0.0f, 100.0f); // 100m/s
                    shot.particle.setAcceleration(0.0f, 0.0f, 0.0f); // No gravity
                    shot.particle.setDamping(0.99f);
                    break;
            }

            // Set the data common to all particle types
            shot.particle.setPosition(0.0f, 1.5f, 0.0f);
            //shot.startTime = TimingData::get().lastFrameTimestamp;
            shot.startTime = (uint)stopWatch.ElapsedMilliseconds;
            shot.type = currentShotType;

            // Clear the force accumulators
            shot.particle.clearAccumulator();
            ammo[count] = shot;
            DebugWide.LogGreen(count + "  ___   " + ammo[count].particle.getPosition() + "  __ " + ammo[count].type);
        }

        /** Creates a new demo object. */
        public BallisticDemo()
        {

            //stopWatch.Start();

        
            currentShotType = ShotType.LASER;

            // Make all shots unused
            //for (AmmoRound* shot = ammo; shot < ammo + ammoRounds; shot++)
            for(int i=0;i< ammoRounds;i++)
            {
                ammo[i].particle = new Particle();
                ammo[i].type = ShotType.UNUSED;
                //shot->type = UNUSED;
            }
        }

        long __prevMs = 0;
        long __prevTick = 0;
        long __timeStepMs = 0;
        //Random random = new Random();
        /** Update the particle positions. */
        public void update()
        {

            //float strength = (float)-random.randomReal(500.0f, 1000.0f);
            //DebugWide.LogBlue(strength);

            // Find the duration of the last frame in seconds
            //float duration = (float)TimingData::get().lastFrameDuration * 0.001f;

            //DebugWide.LogBlue(Stopwatch.Frequency +" __ "+ (stopWatch.ElapsedMilliseconds - __prevMs) + "   ___  " + (stopWatch.ElapsedTicks - __prevTick)/10000f);
            __timeStepMs = (stopWatch.ElapsedMilliseconds - __prevMs);
            __prevTick = stopWatch.ElapsedTicks;
            __prevMs = stopWatch.ElapsedMilliseconds;


            float duration = (float)__timeStepMs * 0.001f;
            if (duration <= 0.0f) return;


            // Update the physics of each particle in turn
            //for (AmmoRound* shot = ammo; shot < ammo + ammoRounds; shot++)
            AmmoRound shot;
            for (int i = 0; i < ammoRounds; i++)
            {
                shot = ammo[i];

                if (shot.type != ShotType.UNUSED)
                {
                    // Run the physics
                    shot.particle.integrate(duration);
                    //DebugWide.LogBlue(i + "  ___   " + shot.particle.getPosition() + "  __ " + shot.type);

                    // Check if the particle is now invalid
                    if (shot.particle.getPosition().y < 0.0f ||
                        shot.startTime + 5000 < stopWatch.ElapsedMilliseconds ||
                        shot.particle.getPosition().z > 200.0f)
                    {
                        // We simply set the shot type to be unused, so the
                        // memory it occupies can be reused by another shot.
                        shot.type = ShotType.UNUSED;
                        ammo[i] = shot;
                        //ammo[i].type = ShotType.UNUSED;
                        //DebugWide.LogBlue(i + " ____ " + shot.particle.getPosition());
                    }

                }
            }


        }

        /** Display the particle positions. */
        public void display()
        {
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

            //// Draw some scale lines
            //glColor3f(0.75f, 0.75f, 0.75f);
            //glBegin(GL_LINES);
            //for (unsigned i = 0; i < 200; i += 10)
            //{
            //    glVertex3f(-5.0f, 0.0f, i);
            //    glVertex3f(5.0f, 0.0f, i);
            //}
            //glEnd();

            // Render each particle in turn
            //for (AmmoRound* shot = ammo; shot < ammo + ammoRounds; shot++)
            for (int i = 0; i < ammoRounds; i++)
            {
                if (ammo[i].type != ShotType.UNUSED)
                {
                    ammo[i].render();
                }
            }

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

        /** Handle a mouse click. */
        //public virtual void mouse(int button, int state, int x, int y);

        /** Handle a keypress. */
        public void key()
        {

            if(Input.GetKey(KeyCode.Alpha1))
            {
                DebugWide.LogBlue("PISTOL");
                currentShotType = ShotType.PISTOL;
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                DebugWide.LogBlue("ARTILLERY");
                currentShotType = ShotType.ARTILLERY;
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                DebugWide.LogBlue("FIREBALL");
                currentShotType = ShotType.FIREBALL;
            }
            if (Input.GetKey(KeyCode.Alpha4))
            {
                DebugWide.LogBlue("LASER");
                currentShotType = ShotType.LASER;
            }
            if (Input.GetKey(KeyCode.F))
            {
                DebugWide.LogRed("fire!!");
                fire();
            }

        }
    }
}