using System;
using UnityEngine;
using System.Diagnostics;

public class Test_Blob : MonoBehaviour 
{
    Cyclone.BlobDemo demo = null;
    Transform t_pos = null;

    private void Start()
    {
        demo = new Cyclone.BlobDemo();
        //t_pos = GameObject.Find("t_pos").transform;
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
    public class BConst
    {
        public const int BLOB_COUNT = 5;
        public const float BLOB_RADIUS = 0.4f;
        public const int PLATFORM_COUNT = 10;
    }

    /**
     * Platforms are two dimensional: lines on which the
     * particles can rest. Platforms are also contact generators for the physics.
     */
    public class Platform : ParticleContactGenerator
    {


        public Vector3 start;
        public Vector3 end;

        /**
         * Holds a pointer to the particles we're checking for collisions with.
         */
        public Particle[] particles = new Particle[BConst.BLOB_COUNT];

        public void init()
        {
            _isContactList = true; //addContactList 를 재정의 한것을 구별하기 위해 추가  - chamto 

            for(int i=0;i< BConst.BLOB_COUNT;i++)
            {
                particles[i] = new Particle();
            }
        }

        public override uint addContactList(ParticleContact[] contactList, int cIdx,
                                    uint limit)
        {
            const float restitution = 0.0f;

            ParticleContact contact = contactList[cIdx];
            uint used = 0;
            for (uint i = 0; i< BConst.BLOB_COUNT; i++)
            {
                if (used >= limit) break;

                // Check for penetration
                Vector3 toParticle = particles[i].getPosition() - start;
                Vector3 lineDirection = end - start;
                float projected = toParticle * lineDirection;
                float platformSqLength = lineDirection.squareMagnitude();
                if (projected <= 0)
                {
                    // The blob is nearest to the start point
                    if (toParticle.squareMagnitude() < BConst.BLOB_RADIUS * BConst.BLOB_RADIUS)
                    {
                        // We have a collision
                        contact.contactNormal = toParticle.unit();
                        contact.contactNormal.z = 0;
                        contact.restitution = restitution;
                        contact.particle[0] = particles[i];
                        contact.particle[1] = null;
                        contact.penetration = BConst.BLOB_RADIUS - toParticle.magnitude();
                        used ++;
                        //contact ++;
                        contact = contactList[cIdx + used];
                    }

                }
                else if (projected >= platformSqLength)
                {
                    // The blob is nearest to the end point
                    toParticle = particles[i].getPosition() - end;
                    if (toParticle.squareMagnitude() < BConst.BLOB_RADIUS * BConst.BLOB_RADIUS)
                    {
                        // We have a collision
                        contact.contactNormal = toParticle.unit();
                        contact.contactNormal.z = 0;
                        contact.restitution = restitution;
                        contact.particle[0] = particles[i];
                        contact.particle[1] = null;
                        contact.penetration = BConst.BLOB_RADIUS - toParticle.magnitude();
                        used ++;
                        //contact ++;
                        contact = contactList[cIdx + used];
                    }
                }
                else
                {
                    // the blob is nearest to the middle.
                    float distanceToPlatform =
                        toParticle.squareMagnitude() -
                        projected * projected / platformSqLength;
                    if (distanceToPlatform < BConst.BLOB_RADIUS * BConst.BLOB_RADIUS)
                    {
                        // We have a collision
                        Vector3 closestPoint =
                            start + lineDirection * (projected / platformSqLength);

                        contact.contactNormal = (particles[i].getPosition()-closestPoint).unit();
                        contact.contactNormal.z = 0;
                        contact.restitution = restitution;
                        contact.particle[0] = particles[i];
                        contact.particle[1] = null;
                        contact.penetration = BConst.BLOB_RADIUS - (float)Math.Sqrt(distanceToPlatform);
                        used ++;
                        //contact ++;
                        contact = contactList[cIdx + used];
                    }
                }
            }
            return used;
        }
    }

    /**
     * A force generator for proximal attraction.
     */
    public class BlobForceGenerator : ParticleForceGenerator
    {
    
        /**
        * Holds a pointer to the particles we might be attracting.
        */
        public Particle[] particles;

        /**
         * The maximum force used to push the particles apart.
         */
        public float maxReplusion;

        /**
         * The maximum force used to pull particles together.
         */
        public float maxAttraction;

        /**
         * The separation between particles where there is no force.
         */
        public float minNaturalDistance, maxNaturalDistance;

        /**
         * The force with which to float the head particle, if it is
         * joined to others.
         */
        public float floatHead;

        /**
         * The maximum number of particles in the blob before the head
         * is floated at maximum force.
         */
        public uint maxFloat;

        /**
         * The separation between particles after which they 'break' apart and
         * there is no force.
         */
        public float maxDistance;

        public override void updateForce(Particle particle, float duration)
        {
            uint joinCount = 0;
            for (uint i = 0; i < BConst.BLOB_COUNT; i++)
            {
                // Don't attract yourself
                if (particles[i] == particle) continue;

                // Work out the separation distance
                Vector3 separation =
                    particles[i].getPosition() - particle.getPosition();
                separation.z = 0.0f;
                float distance = separation.magnitude();

                if (distance < minNaturalDistance)
                {
                    // Use a repulsion force.
                    distance = 1.0f - distance / minNaturalDistance;
                    particle.addForce(
                        separation.unit() * (1.0f - distance) * maxReplusion * -1.0f
                        );
                    joinCount++;
                }
                else if (distance > maxNaturalDistance && distance < maxDistance)
                {
                    // Use an attraction force.
                    distance =
                        (distance - maxNaturalDistance) /
                        (maxDistance - maxNaturalDistance);
                    particle.addForce(
                        separation.unit() * distance * maxAttraction
                        );
                    joinCount++;
                }
            }

            // If the particle is the head, and we've got a join count, then float it.
            if (particle == particles[0] && joinCount > 0 && maxFloat > 0)
            {
                float force = (float)(joinCount / maxFloat) * floatHead;
                if (force > floatHead) force = floatHead;
                particle.addForce(new Vector3(0, force, 0));
            }

        }
    }

    public class BlobDemo 
    {
        Particle[] blobs;

        Platform[] platforms;

        ParticleWorld world;

        BlobForceGenerator blobForceGenerator;

        /* The control for the x-axis. */
        float xAxis;

        /* The control for the y-axis. */
        float yAxis;

        void reset()
        {
            //cyclone::Random r;
            Platform p = platforms[BConst.PLATFORM_COUNT - 2];
            float fraction = 1.0f / BConst.BLOB_COUNT;
            Vector3 delta = p.end - p.start;
            for (uint i = 0; i < BConst.BLOB_COUNT; i++)
            {
                uint me = (i + BConst.BLOB_COUNT / 2) % BConst.BLOB_COUNT;
                blobs[i].setPosition(
                    p.start + delta * ((float)(me) * 0.8f * fraction + 0.1f) +
                    new Vector3(0, 1.0f + (float)Random.GetI().randomReal(), 0));
                blobs[i].setVelocity(0, 0, 0);
                blobs[i].clearAccumulator();
            }
        }


        /** Creates a new demo object. */
        public BlobDemo()
        {
            xAxis = 0;
            yAxis = 0;
            world = new ParticleWorld(BConst.PLATFORM_COUNT + BConst.BLOB_COUNT, BConst.PLATFORM_COUNT);

            // Create the blob storage
            blobs = new Particle[BConst.BLOB_COUNT];
            for(int i=0;i< BConst.BLOB_COUNT;i++ )
            {
                blobs[i] = new Particle();
            }
            Random r = Random.GetI();

            blobForceGenerator = new BlobForceGenerator();
            // Create the force generator
            blobForceGenerator.particles = blobs;
            blobForceGenerator.maxAttraction = 20.0f;
            blobForceGenerator.maxReplusion = 10.0f;
            blobForceGenerator.minNaturalDistance = BConst.BLOB_RADIUS * 0.75f;
            blobForceGenerator.maxNaturalDistance = BConst.BLOB_RADIUS * 1.5f;
            blobForceGenerator.maxDistance = BConst.BLOB_RADIUS * 2.5f;
            blobForceGenerator.maxFloat = 2;
            blobForceGenerator.floatHead = 8.0f;

            // Create the platforms
            platforms = new Platform[BConst.PLATFORM_COUNT];
            for (int i = 0; i < BConst.PLATFORM_COUNT; i++)
            {
                platforms[i] = new Platform();
                platforms[i].init();
            }

            for (uint i = 0; i < BConst.PLATFORM_COUNT; i++)
            {
                platforms[i].start = new Vector3(
                    (float)(i % 2) * 10.0f - 5.0f,
                    (float)(i) * 4.0f + ((i % 2) == 1 ? 0.0f : 2.0f),
                    0);
                platforms[i].start.x += (float)r.randomBinomial(2.0f);
                platforms[i].start.y += (float)r.randomBinomial(2.0f);

                platforms[i].end = new Vector3(
                    (float)(i % 2) * 10.0f + 5.0f,
                    (float)(i) * 4.0f + ((i % 2) == 1 ? 2.0f : 0.0f),
                    0);
                platforms[i].end.x += (float)r.randomBinomial(2.0f);
                platforms[i].end.y += (float)r.randomBinomial(2.0f);

                // Make sure the platform knows which particles it
                // should collide with.
                platforms[i].particles = blobs;
                world.getContactGenerators().Add(platforms[i]);
            }

            // Create the blobs.
            Platform p = platforms[BConst.PLATFORM_COUNT - 2];
            float fraction = 1.0f / BConst.BLOB_COUNT;
            Vector3 delta = p.end - p.start;
            for (uint i = 0; i < BConst.BLOB_COUNT; i++)
            {
                uint me = (i + BConst.BLOB_COUNT / 2) % BConst.BLOB_COUNT;
                blobs[i].setPosition(
                    p.start + delta * ((float)(me) * 0.8f * fraction + 0.1f) +
                    new Vector3(0, 1.0f + (float)r.randomReal(), 0));

                blobs[i].setVelocity(0, 0, 0);
                blobs[i].setDamping(0.2f);
                blobs[i].setAcceleration(Vector3.GRAVITY * 0.4f);
                blobs[i].setMass(1.0f);
                blobs[i].clearAccumulator();

                world.getParticles().Add(blobs[i]);
                world.getForceRegistry().add(blobs[i], blobForceGenerator);
            }
        }

        /** Display the particles. */
        public void display()
        {
            Vector3 pos = blobs[0].getPosition();

            // Clear the view port and set the camera direction
            //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            //glLoadIdentity();
            //gluLookAt(pos.x, pos.y, 6.0, pos.x, pos.y, 0.0, 0.0, 1.0, 0.0);

            //glColor3f(0, 0, 0);

            //glBegin(GL_LINES);
            Color cc = new Color(0, 0, 1);
            for (uint i = 0; i < BConst.PLATFORM_COUNT; i++)
            {
                Vector3 p0 = platforms[i].start;
                Vector3 p1 = platforms[i].end;
                //glVertex3f(p0.x, p0.y, p0.z);
                //glVertex3f(p1.x, p1.y, p1.z);
                DebugWide.DrawLine(p0.ToUnity(), p1.ToUnity(), cc);
            }
            //glEnd();

            cc = new Color(1, 0, 0);
            for (uint i = 0; i < BConst.BLOB_COUNT; i++)
            {
                Vector3 p_ = blobs[i].getPosition();
                //glPushMatrix();
                //glTranslatef(p.x, p.y, p.z);
                //glutSolidSphere(BLOB_RADIUS, 12, 12);
                //glPopMatrix();
                DebugWide.DrawCircle(p_.ToUnity(), BConst.BLOB_RADIUS, cc);
            }

            Vector3 p = blobs[0].getPosition();
            Vector3 v = blobs[0].getVelocity() * 0.05f;
            Vector3 pp;
            v.trim(BConst.BLOB_RADIUS * 0.5f);
            p = p + v;
            //glPushMatrix();
            //glTranslatef(p.x - BLOB_RADIUS * 0.2f, p.y, BLOB_RADIUS);
            pp = new Vector3(p.x - BConst.BLOB_RADIUS * 0.2f, p.y, BConst.BLOB_RADIUS);
            cc = new Color(1, 1, 1);
            //glutSolidSphere(BLOB_RADIUS * 0.2f, 8, 8);
            DebugWide.DrawCircle(pp.ToUnity(), BConst.BLOB_RADIUS * 0.2f, cc);

            //glTranslatef(0, 0, BLOB_RADIUS * 0.2f);
            pp = pp + new Vector3(0, 0, BConst.BLOB_RADIUS * 0.2f);
            cc = new Color(0, 0, 0);
            //glutSolidSphere(BLOB_RADIUS * 0.1f, 8, 8);
            DebugWide.DrawCircle(pp.ToUnity(), BConst.BLOB_RADIUS * 0.1f, cc);

            //glTranslatef(BLOB_RADIUS * 0.4f, 0, -BLOB_RADIUS * 0.2f);
            pp = pp + new Vector3(BConst.BLOB_RADIUS * 0.4f, 0, -BConst.BLOB_RADIUS * 0.2f);
            cc = new Color(1, 1, 1);
            //glutSolidSphere(BLOB_RADIUS * 0.2f, 8, 8);
            DebugWide.DrawCircle(pp.ToUnity(), BConst.BLOB_RADIUS * 0.2f, cc);

            //glTranslatef(0, 0, BLOB_RADIUS * 0.2f);
            pp = pp + new Vector3(0, 0, BConst.BLOB_RADIUS * 0.2f);
            cc = new Color(0, 0, 0);
            //glutSolidSphere(BLOB_RADIUS * 0.1f, 8, 8);
            DebugWide.DrawCircle(pp.ToUnity(), BConst.BLOB_RADIUS * 0.1f, cc);
            //glPopMatrix();
        }

        Stopwatch stopWatch = Stopwatch.StartNew();
        long __prevMs = 0;
        long __timeStepMs = 0;
        /** Update the particle positions. */
        public void update()
        {
            // Clear accumulators
            world.startFrame();

            __timeStepMs = (stopWatch.ElapsedMilliseconds - __prevMs);
            __prevMs = stopWatch.ElapsedMilliseconds;


            float duration = (float)__timeStepMs * 0.001f;
            if (duration <= 0.0f) return;

            // Recenter the axes
            xAxis *= (float)Math.Pow(0.1f, duration);
            yAxis *= (float)Math.Pow(0.1f, duration);

            // Move the controlled blob
            blobs[0].addForce(new Vector3(xAxis, yAxis, 0) * 10.0f);

            // Run the simulation
            world.runPhysics(duration);

            // Bring all the particles back to 2d
            Vector3 position;
            for (uint i = 0; i < BConst.BLOB_COUNT; i++)
            {
                blobs[i].getPosition(out position);
                position.z = 0.0f;
                blobs[i].setPosition(position);
            }

        }

        /** Handle a key press. */
        public void key()
        {
            if(Input.GetKey(KeyCode.W))
            {
                yAxis = 1.0f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                yAxis = -1.0f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                xAxis = -1.0f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                xAxis = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                reset();
            }


        }

    }
}//end namespace
