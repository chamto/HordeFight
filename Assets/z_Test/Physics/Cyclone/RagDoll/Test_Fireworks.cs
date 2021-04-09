using System;
using System.Diagnostics;
using UnityEngine;

public class Test_Fireworks : MonoBehaviour
{
    Cyclone.FireworksDemo demo = null;

    private void Start()
    {
        demo = new Cyclone.FireworksDemo();
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

    public class FireworksDemo
    {
        /**
         * Holds the maximum number of fireworks that can be in use.
         */
        const uint maxFireworks = 1024;

        /** Holds the firework data. */
        Firework[] fireworks = new Firework[maxFireworks];

        /** Holds the index of the next firework slot to use. */
        uint nextFirework;

        /** And the number of rules. */
        const uint ruleCount = 9;

        /** Holds the set of rules. */
        FireworkRule[] rules = new FireworkRule[ruleCount];

        /** Dispatches a firework from the origin. */
        void create(uint type, uint number, Firework parent)
        {
            for (uint i = 0; i<number; i++)
            {
                create(type, parent);
            }
        }

        /** Dispatches the given number of fireworks from the given parent. */
        void create(uint type, Firework parent)
        {
            // Get the rule needed to create this firework
            //FireworkRule rule = rules + (type - 1);
            FireworkRule rule = rules[type - 1];

            // Create the firework
            rule.create(fireworks[nextFirework], parent);

            // Increment the index for the next firework
            nextFirework = (nextFirework + 1) % maxFireworks;
        }

        /** Creates the rules. */
        void initFireworkRules()
        {
            // Go through the firework types and create their rules.
            rules[0].init(2);
            rules[0].setParameters(
                1, // type
                0.5f, 1.4f, // age range
                new Vector3(-5, 25, -5), // min velocity
                new Vector3(5, 28, 5), // max velocity
                0.1f // damping
                );
            rules[0].payloads[0].set(3, 5);
            rules[0].payloads[1].set(5, 5);

            rules[1].init(1);
            rules[1].setParameters(
                2, // type
                0.5f, 1.0f, // age range
                new Vector3(-5, 10, -5), // min velocity
                new Vector3(5, 20, 5), // max velocity
                0.8f // damping
                );
            rules[1].payloads[0].set(4, 2);

            rules[2].init(0);
            rules[2].setParameters(
                3, // type
                0.5f, 1.5f, // age range
                new Vector3(-5, -5, -5), // min velocity
                new Vector3(5, 5, 5), // max velocity
                0.1f // damping
                );

            rules[3].init(0);
            rules[3].setParameters(
                4, // type
                0.25f, 0.5f, // age range
                new Vector3(-20, 5, -5), // min velocity
                new Vector3(20, 5, 5), // max velocity
                0.2f // damping
                );

            rules[4].init(1);
            rules[4].setParameters(
                5, // type
                0.5f, 1.0f, // age range
                new Vector3(-20, 2, -5), // min velocity
                new Vector3(20, 18, 5), // max velocity
                0.01f // damping
                );
            rules[4].payloads[0].set(3, 5);

            rules[5].init(0);
            rules[5].setParameters(
                6, // type
                3, 5, // age range
                new Vector3(-5, 5, -5), // min velocity
                new Vector3(5, 10, 5), // max velocity
                0.95f // damping
                );

            rules[6].init(1);
            rules[6].setParameters(
                7, // type
                4, 5, // age range
                new Vector3(-5, 50, -5), // min velocity
                new Vector3(5, 60, 5), // max velocity
                0.01f // damping
                );
            rules[6].payloads[0].set(8, 10);

            rules[7].init(0);
            rules[7].setParameters(
                8, // type
                0.25f, 0.5f, // age range
                new Vector3(-1, -1, -1), // min velocity
                new Vector3(1, 1, 1), // max velocity
                0.01f // damping
                );

            rules[8].init(0);
            rules[8].setParameters(
                9, // type
                3, 5, // age range
                new Vector3(-15, 10, -5), // min velocity
                new Vector3(15, 15, 5), // max velocity
                0.95f // damping
                );
            // ... and so on for other firework types ...
        }


        /** Creates a new demo object. */
        public FireworksDemo()
        {
            nextFirework = 0;

            // Make all shots unused
            for(int i=0;i< maxFireworks;i++)
            {
                fireworks[i] = new Firework();
                fireworks[i].type = 0;
            }
            for (int i = 0; i < ruleCount; i++)
            {
                rules[i] = new FireworkRule();
            }


            // Create the firework types
            initFireworkRules();
        }

        Stopwatch stopWatch = Stopwatch.StartNew();
        long __prevMs = 0;
        long __timeStepMs = 0;
        /** Update the particle positions. */
        public void update()
        {
            // Find the duration of the last frame in seconds
            __timeStepMs = (stopWatch.ElapsedMilliseconds - __prevMs);
            __prevMs = stopWatch.ElapsedMilliseconds;

            float duration = (float)__timeStepMs * 0.001f;
            if (duration <= 0.0f) return;

            //for (Firework* firework = fireworks;
            //firework < fireworks + maxFireworks;
            //firework++)
            for (int i = 0; i < maxFireworks; i++)
            {
                Firework firework = fireworks[i];
                // Check if we need to process this firework.
                if (firework.type > 0)
                {
                    // Does it need removing?
                    if (firework.update(duration))
                    {
                        // Find the appropriate rule
                        //FireworkRule rule = rules + (firework.type - 1);
                        FireworkRule rule = rules[firework.type - 1];

                        // Delete the current firework (this doesn't affect its
                        // position and velocity for passing to the create function,
                        // just whether or not it is processed for rendering or
                        // physics.
                        firework.type = 0;

                        // Add the payload
                        for (uint j = 0; j < rule.payloadCount; j++)
                        {
                            //FireworkRule.Payload payload = rule.payloads + j;
                            FireworkRule.Payload payload = rule.payloads[j];
                            create(payload.type, payload.count, firework);
                        }
                    }
                }
            }


        }

        /** Display the particle positions. */
        public void display()
        {
            const float size = 0.1f;

            // Clear the viewport and set the camera direction
            //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            //glLoadIdentity();
            //gluLookAt(0.0, 4.0, 10.0, 0.0, 4.0, 0.0, 0.0, 1.0, 0.0);

            // Render each firework in turn
            //glBegin(GL_QUADS);
            Color cc = Color.black;
            //for (Firework* firework = fireworks;
            //firework < fireworks + maxFireworks;
            //firework++)
            for (int i = 0; i < maxFireworks; i++)
            {
                Firework firework = fireworks[i];
                // Check if we need to process this firework.
                if (firework.type > 0)
                {
                    switch (firework.type)
                    {
                        case 1: cc = new Color(1, 0, 0); break;
                        case 2: cc = new Color(1, 0.5f, 0); break;
                        case 3: cc = new Color(1, 1, 0); break;
                        case 4: cc = new Color(0, 1, 0); break;
                        case 5: cc = new Color(0, 1, 1); break;
                        case 6: cc = new Color(0.4f, 0.4f, 1); break;
                        case 7: cc = new Color(1, 0, 1); break;
                        case 8: cc = new Color(1, 1, 1); break;
                        case 9: cc = new Color(1, 0.5f, 0.5f); break;
                    };

                    Vector3 pos = firework.getPosition();
                    Vector3 p0, p1, p2, p3;
                    p0 = new Vector3(pos.x - size, pos.y - size, pos.z);
                    p1 = new Vector3(pos.x + size, pos.y - size, pos.z);
                    p2 = new Vector3(pos.x + size, pos.y + size, pos.z);
                    p3 = new Vector3(pos.x - size, pos.y + size, pos.z);
                    DebugWide.DrawSolidQuads(p0.ToUnity(), p1.ToUnity(), p2.ToUnity(), p3.ToUnity(), cc);


                    p0 = new Vector3(pos.x - size, -pos.y - size, pos.z);
                    p1 = new Vector3(pos.x + size, -pos.y - size, pos.z);
                    p2 = new Vector3(pos.x + size, -pos.y + size, pos.z);
                    p3 = new Vector3(pos.x - size, -pos.y + size, pos.z);
                    DebugWide.DrawSolidQuads(p0.ToUnity(), p1.ToUnity(), p2.ToUnity(), p3.ToUnity(), cc);

                    //glVertex3f(pos.x - size, pos.y - size, pos.z);
                    //glVertex3f(pos.x + size, pos.y - size, pos.z);
                    //glVertex3f(pos.x + size, pos.y + size, pos.z);
                    //glVertex3f(pos.x - size, pos.y + size, pos.z);


                    // Render the firework's reflection
                    //glVertex3f(pos.x - size, -pos.y - size, pos.z);
                    //glVertex3f(pos.x + size, -pos.y - size, pos.z);
                    //glVertex3f(pos.x + size, -pos.y + size, pos.z);
                    //glVertex3f(pos.x - size, -pos.y + size, pos.z);
                }
            }
            //glEnd();
        }

        /** Handle a keypress. */
        public void key()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                create(1, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                create(2, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                create(3, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                create(4, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                create(5, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                create(6, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                create(7, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                create(8, 1, null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                create(9, 1, null);
            }


            //switch (key)
            //{
            //    case '1': create(1, 1, NULL); break;
            //    case '2': create(2, 1, NULL); break;
            //    case '3': create(3, 1, NULL); break;
            //    case '4': create(4, 1, NULL); break;
            //    case '5': create(5, 1, NULL); break;
            //    case '6': create(6, 1, NULL); break;
            //    case '7': create(7, 1, NULL); break;
            //    case '8': create(8, 1, NULL); break;
            //    case '9': create(9, 1, NULL); break;
            //}
        }
    }

/**
* Fireworks are particles, with additional data for rendering and
* evolution.
*/
public class Firework : Particle
    {
    
        /** Fireworks have an integer type, used for firework rules. */
        public uint type;

        /**
         * The age of a firework determines when it detonates. Age gradually
         * decreases, when it passes zero the firework delivers its payload.
         * Think of age as fuse-left.
         */
        public float age;

        /**
         * Updates the firework by the given duration of time. Returns true
         * if the firework has reached the end of its life and needs to be
         * removed.
         */
        public bool update(float duration)
        {
            // Update our physical state
            integrate(duration);

            // We work backwards from our age to zero.
            age -= duration;
            return (age < 0) || (position.y < 0);
        }
    }

    public struct FireworkRule
    {
        /** The type of firework that is managed by this rule. */
        public uint type;

        /** The minimum length of the fuse. */
        public float minAge;

        /** The maximum legnth of the fuse. */
        public float maxAge;

        /** The minimum relative velocity of this firework. */
        public Vector3 minVelocity;

        /** The maximum relative velocity of this firework. */
        public Vector3 maxVelocity;

        /** The damping of this firework type. */
        public float damping;

        /**
         * The payload is the new firework type to create when this
         * firework's fuse is over.
         */
        public struct Payload
        {
            /** The type of the new particle to create. */
            public uint type;

            /** The number of particles in this payload. */
            public uint count;

            /** Sets the payload properties in one go. */
            public void set(uint type, uint count)
            {
                this.type = type;
                this.count = count;
            }
        }

        /** The number of payloads for this firework type. */
        public uint payloadCount;

        /** The set of payloads. */
        public Payload[] payloads;

        //public FireworkRule()
        //{
        //    payloadCount = 0;
        //    payloads = null;
        //}

        public void init(uint payloadCount)
        {
            this.payloadCount = payloadCount;
            payloads = new Payload[payloadCount];
            for(int i=0;i< payloadCount;i++)
            {
                payloads[i] = new Payload();
            }
        }


        /**
         * Set all the rule parameters in one go.
         */
        public void setParameters(uint type, float minAge, float maxAge, Vector3 minVelocity, Vector3 maxVelocity, float damping)
        {
            this.type = type;
            this.minAge = minAge;
            this.maxAge = maxAge;
            this.minVelocity = minVelocity;
            this.maxVelocity = maxVelocity;
            this.damping = damping;
        }

        /**
         * Creates a new firework of this type and writes it into the given
         * instance. The optional parent firework is used to base position
         * and velocity on.
         */
        public void create(Firework firework, Firework parent = null)
        {
            firework.type = type;
            firework.age = (float)Random.GetI().randomReal(minAge, maxAge);

            Vector3 vel = Vector3.ZERO;
            if (null != parent) 
            {
                // The position and velocity are based on the parent.
                firework.setPosition(parent.getPosition());
                vel += parent.getVelocity();
            }
            else
            {
                Vector3 start = Vector3.ZERO;
                int x = (int)Random.GetI().randomInt(3) - 1;
                start.x = 5.0f * (float)x;
                firework.setPosition(start);
            }

            vel += Random.GetI().randomVector(minVelocity, maxVelocity);
            firework.setVelocity(vel);

            // We use a mass of one in all cases (no point having fireworks
            // with different masses, since they are only under the influence
            // of gravity).
            firework.setMass(1);

            firework.setDamping(damping);

            firework.setAcceleration(Vector3.GRAVITY);

            firework.clearAccumulator();
        }
    }
}