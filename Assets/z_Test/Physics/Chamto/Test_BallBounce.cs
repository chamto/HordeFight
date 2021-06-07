using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_BallBounce
{
    public class Test_BallBounce : MonoBehaviour
    {
        Point_mass ball = new Point_mass();
        //public bool forceApplied = false;

        public Transform _tr_force = null;

        public float Mass = 1;
        public float Friction = 0.9f; //마찰력
        public float Restitution = 0.7f; //반반력 , 탄성계수 
        public float Damping = 1;

        // Use this for initialization
        void Start()
        {
            _tr_force = GameObject.Find("tr_force").transform;

            Init();
        }

        public void Init()
        {
            ball.position = new Vector3(0, 0, 2);
            ball.mass = 1;
            ball.radius = 1;
            ball.forces = _tr_force.position - ball.position;
            ball.linearAcceleration = new Vector3(0, 0, -9.8f);
            ball.linearVelocity = new Vector3(10, 0, 10);
            //ball.damping = 1f;
        }

        // Update is called once per frame
        void Update()
        {
            //if (true == forceApplied)
            //{
            //    forceApplied = false;
            //    ball.forces = _tr_force.position - ball.location;
            //    ball.forces *= 10;
            //}
            //else
            //{
            //    ball.forces = Vector3.zero;
            //}

            if (Input.GetKeyDown(KeyCode.R))
            {
                Init();
            }

            ball.mass = Mass;
            ball.damping = Damping;

            //ball.Update(Time.deltaTime);
            ball.integrate(Time.deltaTime);
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

            CollisionPlane planeX = new CollisionPlane();
            planeX.direction = new Vector3(-1, 0, 0); //정규화된 값이어야 함 
            planeX.offset = -30;
            planeX.mass = 1;
            planeX.elasticity = 1;

            Contact contact;
            if (CollisionDetector.sphereAndHalfSpace(ball, planeZ, out contact))
            {
                //충돌처리  
                DebugWide.LogBlue(contact.contactPoint + "  " + contact.penetration);


                ball.position += contact.contactNormal * contact.penetration; //겹침 계산 



                //하드코딩
                if (false)
                {
                    Vector3 refl = VOp.Reflect(ball.linearVelocity, contact.contactNormal);

                    //축을 분리해서 적용해야 하지만 간단히 하드코딩으로 탄성력과 마찰력을 적용한다 
                    refl.z *= Restitution;
                    refl.x *= Friction;
                    ball.linearVelocity = refl;

                }

                if (true)
                {
                    HandleCollision(ref ball, ref planeZ, Time.deltaTime);
                }

            }
            if (CollisionDetector.sphereAndHalfSpace(ball, planeX, out contact))
            {
                ball.position += contact.contactNormal * contact.penetration; //겹침 계산 

                HandleCollision(ref ball, ref planeX, Time.deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            if (null == _tr_force) return;

            //DebugWide.DrawLine(ball.location, _tr_force.position, Color.white);
            ball.Draw(Color.white);
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



    //public struct CollisionSphere
    //{

    //    /**
    //     * The radius of the sphere.
    //     */
    //    public float radius;

    //    public Vector3 position;
    //}

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


    public struct Contact
    {

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
                Vector3.Dot(plane.direction , position) - sphere.radius - plane.offset;

            if (ballDistance >= 0) return false;


            contact.contactNormal = plane.direction;
            contact.penetration = -ballDistance;
            contact.contactPoint = position - plane.direction * (ballDistance + sphere.radius);

            return true;
        }
    }

    public struct Point_mass
    {
        public float mass;
        public Vector3 position; //centerOfMassLocation;
        public Vector3 linearVelocity;
        public Vector3 linearAcceleration;
        public Vector3 forces;

        public float radius;
        public float elasticity; //coefficientOfRestitution
        public float damping;

        public void Update2(float changeInTime)
        {
            //
            // Begin calculating linear dynamics.
            //

            // Find the linear acceleration.
            // a = F/m
            //assert(mass != 0);
            linearAcceleration = forces / mass;

            // Find the linear velocity.
            linearVelocity += linearAcceleration * changeInTime;

            // Find the new location of the center of mass.
            position += linearVelocity * changeInTime;

            //
            // End calculating linear dynamics.
            //

        }

        public void integrate(float changeInTime)
        {

            // Update linear position.
            position += linearVelocity * changeInTime;

            // Work out the acceleration from the force
            Vector3 resultingAcc = linearAcceleration;
            resultingAcc += (forces / mass);

            // Update linear velocity from the acceleration.
            linearVelocity += resultingAcc * changeInTime;

            // Impose drag.
            linearVelocity *= (float)Math.Pow(damping, changeInTime);

            // Clear the forces.
            forces = Vector3.zero;
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(position, radius, color);
            //DebugWide.DrawLine(location, location + forces, color);
            DebugWide.DrawLine(position, position + linearVelocity, Color.green);
        }
    }
}

