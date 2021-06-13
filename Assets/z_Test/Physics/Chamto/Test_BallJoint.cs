using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_BallJoint
{
    public class Test_BallJoint : MonoBehaviour
    {
        const int BALL_COUNT = 4;
        Point_mass[] balls = new Point_mass[BALL_COUNT];

        const int CABLE_COUNT = 3;
        ParticleRod[] cables = new ParticleRod[CABLE_COUNT];
        //ParticleCable[] cables = new ParticleCable[CABLE_COUNT];

        public Transform _tr_center = null;
        public Transform _tr_dir = null;

        public float Mass = 1;
        public float Friction = 0.9f; //마찰력
        public float Restitution = 0.7f; //반반력 , 탄성계수 
        public float Damping = 1;

        // Use this for initialization
        void Start()
        {
            _tr_center = GameObject.Find("tr_center").transform;
            _tr_dir = GameObject.Find("tr_dir_0").transform;


            for (int i=0;i< BALL_COUNT; i++ )
            {
                balls[i] = new Point_mass(); 
            }

            for (int i = 0; i < CABLE_COUNT; i++)
            {
                cables[i] = new ParticleRod();
                //cables[i] = new ParticleCable();
                cables[i].particle[0] = balls[i];
                cables[i].particle[1] = balls[i+1];
                cables[i].length = 3f;
                //cables[i].maxLength = 3f;
                //cables[i].restitution = 0.8f;
            }

            Init();


        }

        public void Init()
        {
            for (int i = 0; i < BALL_COUNT; i++)
            {
                balls[i].position = new Vector3(0, 0, 5+i); //같은 위치 설정시 컨텍트 노멀을 구하지 못해 공이 붙어있는 문제 발생함 
                balls[i].mass = Mass;
                balls[i].radius = 0.5f;
                balls[i].forces = Vector3.zero;
                balls[i].linearAcceleration = new Vector3(0, 0, -9.8f);
                balls[i].linearVelocity = new Vector3(0, 0, 0);
                balls[i].damping = Damping;
            }
            balls[0].linearAcceleration = new Vector3(0, 0, 0); //조인트를 조종하는 공에는 가속도가 설정되면 안됨 , 조금씩 떨어지는 버그 발생 
            balls[3].radius = 1f;
            balls[3].mass = 2;
        }


        float TIME_D = 0.05f;
        Vector3 _dir = Vector3.forward;
        public float _angle = 15;
        public float _force = 30;
        void Update()
        {
            //철퇴 회전시키기 
            if (Input.GetKey(KeyCode.A))
            {
                //자동회전 
                _dir = Quaternion.AngleAxis(_angle, Vector3.up) * _dir;

                //직접조종
                //_dir = _tr_dir.position - _tr_center.position;
                //_dir.Normalize();

                balls[0].position = _tr_center.position + _dir * 5f;

                //충돌효과가 난것처럼 따라한다 
                Vector3 v = _dir * _force;
                Vector3 a = v / TIME_D;
                for (int i = 1; i < BALL_COUNT; i++)
                {
                    balls[i].linearVelocity = v;
                    balls[i].forces = a * balls[i].mass; //힘을 적용하면 철퇴가 자연스럽게 가속하는 모습을 볼 수 있다

                }
            }
            //철퇴회전이 멈추면 중력이 적용되게 한다 
            if (Input.GetKeyUp(KeyCode.A))
            {
                for (int i = 1; i < BALL_COUNT; i++)
                {
                    //조화운동을 하지 않아 부자연스럽게 보인다 
                    balls[i].linearVelocity = new Vector3(0, 0, -15) * balls[i].mass; //임의의 적당한 값을 넣는다 

                }
            }




            if (Input.GetKeyDown(KeyCode.R))
            {
                Init();
            }

            for (int i = 0; i < BALL_COUNT; i++)
            {
                balls[i].integrate(TIME_D);
            }



            generateContacts();
        }

        void generateContacts()
        {
            // Create the ground plane data
            CollisionPlane planeZ = new CollisionPlane();
            planeZ.direction = new Vector3(0, 0, 1); //정규화된 값이어야 함 
            planeZ.offset = 0;
            planeZ.mass = 1;
            planeZ.elasticity = 1;


            Contact contact;

            for (int i = 0; i < CABLE_COUNT; i++)
            {
                if(cables[i].GetContact(out contact) )
                {

                    contact.particle[1].position -= contact.contactNormal * contact.penetration;
                    //DebugWide.LogBlue(i + "  " + contact.particle[1].position + "  " + contact.contactNormal + "  " + contact.penetration);
                }
            }

            for (int i = 0; i < BALL_COUNT; i++)
            {
                if (CollisionDetector.sphereAndHalfSpace(balls[i], planeZ, out contact))
                {
                
                    balls[i].position += contact.contactNormal * contact.penetration; //겹침 계산 


                    HandleCollision(ref balls[i], ref planeZ, TIME_D);

                }
            }

        }

        private void OnDrawGizmos()
        {
            if (null == _tr_dir) return;

            //DebugWide.DrawLine(ball.location, _tr_force.position, Color.white);

            DebugWide.DrawCircle(_tr_center.position, 0.5f, Color.red);

            for (int i = 0; i < BALL_COUNT; i++)
            {
                balls[i].Draw(Color.white);
            }

            for (int i = 0; i < CABLE_COUNT; i++)
            {
                DebugWide.DrawLine(cables[i].particle[0].position, cables[i].particle[1].position, Color.white); 
            }

        }



        public void HandleCollision(ref Point_mass pm0, ref CollisionPlane plane, float changeInTime)
        {
            Vector3 unitNormal = plane.direction;

            float velocity1 = Vector3.Dot(pm0.linearVelocity, unitNormal);
            float velocity2 = -velocity1; //반작용 속도 설정 


            // Find the average coefficent of restitution.
            float averageE = (pm0.elasticity + plane.elasticity) / 2f;
            averageE = Restitution; //임시 

            float finalVelocity1 =
            (((pm0.mass -
               (averageE * plane.mass)) * velocity1) +
             ((1 + averageE) * plane.mass * velocity2)) /
            (pm0.mass + plane.mass);



            pm0.linearVelocity = (
                (finalVelocity1 - velocity1) * unitNormal +
                pm0.linearVelocity);

            pm0.linearVelocity *= Friction;


            //공이 벽에 부딪혔을 때 빨라지는 효과??
            //Vector3 acceleration1 =
            //pm0.linearVelocity / changeInTime;

            //pm0.forces = (
            //acceleration1 * pm0.mass);

        }

    }



    public class ParticleLink
    {

        public Point_mass[] particle = new Point_mass[2];


        /**
         * Returns the current length of the link.
         */
        protected float currentLength()
        {
            Vector3 relativePos = particle[0].position -
                                  particle[1].position;
            return relativePos.magnitude;
        }

    }


    /**
     * Cables link a pair of particles, generating a contact if they
     * stray too far apart.
     */
    public class ParticleCable : ParticleLink
    {

        /**
         * Holds the maximum length of the cable.
         */
        public float maxLength;

        /**
         * Holds the restitution (bounciness) of the cable.
         */
        public float restitution;


        /**
         * Fills the given contact structure with the contact needed
         * to keep the cable from over-extending.
         */
        public bool GetContact(out Contact contact)
        {
            contact = new Contact();

            // Find the length of the cable
            float length = currentLength();

            //DebugWide.LogBlue(length + "  " + maxLength);
            // Check if we're over-extended
            if (length < maxLength)
            {
                return false;
            }

            // Otherwise return the contact
            contact.particle[0] = particle[0];
            contact.particle[1] = particle[1];

            // Calculate the normal
            Vector3 normal = particle[1].position - particle[0].position;
            contact.contactNormal = VOp.Normalize(normal);

            contact.penetration = length - maxLength;
            contact.restitution = restitution;

            return true;
        }
    }

    public class ParticleRod : ParticleLink
    {

        /**
         * Holds the length of the rod.
         */
        public float length;


        /**
         * Fills the given contact structure with the contact needed
         * to keep the rod from extending or compressing.
         */
        public bool GetContact(out Contact contact)
        {
            contact = new Contact();

            // Find the length of the rod
            float currentLen = currentLength();

            // Check if we're over-extended
            if (Misc.IsZero(currentLen - length))
            {
                return false;
            }

            // Otherwise return the contact
            contact.particle[0] = particle[0];
            contact.particle[1] = particle[1];

            // Calculate the normal
            Vector3 normal = particle[1].position - particle[0].position;
            normal = VOp.Normalize(normal);
            contact.contactNormal = normal;


            // The contact normal depends on whether we're extending or compressing
            if (currentLen > length)
            {
                contact.contactNormal = normal;
                contact.penetration = currentLen - length;
            }
            else
            {
                contact.contactNormal = normal * -1;
                contact.penetration = length - currentLen;
            }

            // Always use zero restitution (no bounciness)
            contact.restitution = 0;

            return true;
        }
    }

    public struct CollisionPlane
    {

        public float mass;
        public float elasticity;

        /**
         * The plane normal
         */
        public Vector3 direction;

        /**
         * The distance of the plane from the origin.
         */
        public float offset;
    }


    public class Contact
    {
        public Point_mass[] particle = new Point_mass[2];
        /**
         * Holds the position of the contact in world coordinates.
         */
        public Vector3 contactPoint;

        /**
         * Holds the direction of the contact in world coordinates.
         */
        public Vector3 contactNormal;

        /**
         * Holds the depth of penetration at the contact point. If both
         * bodies are specified then the contact point should be midway
         * between the inter-penetrating points.
         */
        public float penetration;

        public float restitution;
    }

    public class CollisionDetector
    {

        public static bool sphereAndHalfSpace(Point_mass sphere, CollisionPlane plane, out Contact contact)
        {
            contact = new Contact();

            // Cache the sphere position
            Vector3 position = sphere.position;

            // Find the distance from the plane
            float ballDistance =
                Vector3.Dot(plane.direction, position) - sphere.radius - plane.offset;

            if (ballDistance >= 0) return false;


            contact.contactNormal = plane.direction;
            contact.penetration = -ballDistance;
            contact.contactPoint = position - plane.direction * (ballDistance + sphere.radius);

            return true;
        }
    }

    public class Point_mass
    {
        public float mass;
        public Vector3 position; //centerOfMassLocation;
        public Vector3 linearVelocity;
        public Vector3 linearAcceleration;
        public Vector3 forces;

        public float radius;
        public float elasticity; //coefficientOfRestitution
        public float damping;


        public float _MAX_VELO = 100;
        public void integrate(float changeInTime)
        {


            // Work out the acceleration from the force
            Vector3 resultingAcc = linearAcceleration;
            resultingAcc += (forces / mass);

            // Update linear velocity from the acceleration.
            linearVelocity += resultingAcc * changeInTime;

            // Impose drag.
            linearVelocity *= (float)Math.Pow(damping, changeInTime);

            linearVelocity = VOp.Truncate(linearVelocity, _MAX_VELO);

            // Update linear position.
            position += linearVelocity * changeInTime;

            // Clear the forces.
            forces = Vector3.zero;
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(position, radius, color);
            //DebugWide.DrawLine(position, position + forces, color);
            DebugWide.DrawLine(position, position + linearVelocity, Color.green);
        }
    }
}

