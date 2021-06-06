using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace David_Conger
{

    [System.Serializable]
    public class Test_ParticleBounce : MonoBehaviour
    {
        public const int COUNT = 2;
        public Point_mass[] allParticles = new Point_mass[COUNT];
        // Use this for initialization
        void Start()
        {

            for (int i = 0; i < COUNT;i++)
            {
                allParticles[i] = new Point_mass();    
            }


            allParticles[0].mass = 1;
            allParticles[0].elasticity = 1f;
            allParticles[0].radius = 1;
            allParticles[0].location = new Vector3(-5.0f, 0.0f, 0.0f);
            allParticles[0].forces = new Vector3(50.0f, 0.0f, 0.0f);

            allParticles[1].mass = 1;
            allParticles[1].elasticity = 1f;
            allParticles[1].radius = 1f;
            allParticles[1].location = new Vector3(0.0f, 0, 0.0f);
            allParticles[1].forces = new Vector3(-10.0f, 0, 5.0f);


        }

        //private bool forceApplied = false;
        void Update()
        {
            // If the force has not yet been applied...
            //if (false == forceApplied)
            //{
            //    forceApplied = true;
            //}
            //// Else the force was already applied...
            //else
            //{
            //    // Set the forces to zero.
            //    //allParticles[0].forces = ConstV.v3_zero;
            //    //allParticles[1].forces = ConstV.v3_zero;
            //}

            float timeInterval = Time.deltaTime;
            for (int i = 0; i < COUNT; i++)
            {
                allParticles[i].Update(timeInterval);
                allParticles[i].forces = ConstV.v3_zero; //힘 적용후 바로 초기화 
            }

            // 
            // Test for a collision.
            //
            // Find the distance vector between the balls.
            Vector3 distance =
                allParticles[0].location - allParticles[1].location;
            float distanceSquared = distance.sqrMagnitude;

            // Find the square of the sum of the radii of the balls.
            float minDistanceSquared =
                allParticles[0].radius + allParticles[1].radius;
            minDistanceSquared = minDistanceSquared * minDistanceSquared;


            // If there is a collision...
            if (distanceSquared < minDistanceSquared)
            {
                //DebugWide.LogBlue("sfsdf");
                // Handle the collision.
                HandleCollision(ref allParticles[0], ref allParticles[1], distance, timeInterval);
            }

        }

		public void OnDrawGizmos()
		{

            allParticles[0].Draw(Color.white);
            allParticles[1].Draw(Color.black);
		}

		public void HandleCollision(ref Point_mass pm0, ref Point_mass pm1, 
                                    Vector3 separationDistance, float changeInTime)
        {
            //
            // Find the outgoing velicities.
            //
            /* First, normalize the displacement vector because it's 
            perpendicular to the collision. */
            Vector3 unitNormal = VOp.Normalize(separationDistance);


            /* Compute the projection of the velocities in the direction
            perpendicular to the collision. */
            float velocity1 = Vector3.Dot(pm0.linearVelocity, unitNormal);
            float velocity2 = Vector3.Dot(pm1.linearVelocity, unitNormal);


            // Find the average coefficent of restitution.
            float averageE = (pm0.elasticity * pm1.elasticity) / 2f;
                

            // Calculate the final velocities.
            float finalVelocity1 =
                (((pm0.mass -
                   (averageE * pm1.mass)) * velocity1) +
                 ((1 + averageE) * pm1.mass * velocity2)) /
                (pm0.mass + pm1.mass);
            float finalVelocity2 =
                (((pm1.mass -
                   (averageE * pm0.mass)) * velocity2) +
                 ((1 + averageE) * pm0.mass * velocity1)) /
                (pm0.mass + pm1.mass);


            pm0.linearVelocity = (
                (finalVelocity1 - velocity1) * unitNormal +
                pm0.linearVelocity);
            pm1.linearVelocity = (
                (finalVelocity2 - velocity2) * unitNormal +
                pm1.linearVelocity);

            //DebugWide.DrawLine(pm0.location, pm0.location + pm0.linearVelocity, Color.red);
            //DebugWide.DrawLine(pm1.location, pm1.location + pm1.linearVelocity, Color.red);
            //
            // Convert the velocities to accelerations.
            //
            Vector3 acceleration1 =
                pm0.linearVelocity / changeInTime;
            Vector3 acceleration2 =
                pm1.linearVelocity / changeInTime;

            // Find the force on each ball.
            pm0.forces = (
                acceleration1 * pm0.mass);
            pm1.forces = (
                acceleration2 * pm1.mass);
        }
    }


    [System.Serializable]
    public struct Point_mass
    {
        public float mass;
        public Vector3 location; //centerOfMassLocation;
        public Vector3 linearVelocity;
        public Vector3 linearAcceleration;
        public Vector3 forces;

        public float radius;
        public float elasticity; //coefficientOfRestitution


        public void Update(float changeInTime)
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
            location += linearVelocity * changeInTime;

            //
            // End calculating linear dynamics.
            //

        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(location, radius, color);
            //DebugWide.DrawLine(location, location + forces, color);
            DebugWide.DrawLine(location, location + linearVelocity, Color.green);
        }
    }
}

