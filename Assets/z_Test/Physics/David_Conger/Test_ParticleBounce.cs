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
            allParticles[1].location = new Vector3(0.0f, 0, 0.5f);
            //allParticles[1].forces = new Vector3(-10.0f, 0, 5.0f);
            //allParticles[1].forces = new Vector3(-50.0f, 0.0f, 0.0f);

        }

        //private bool forceApplied = false;
        void Update()
        {
            float m0_v = allParticles[0].linearVelocity.magnitude;
            float m1_v = allParticles[1].linearVelocity.magnitude;
            float m01_v = m0_v + m1_v; //일차원 운동량값만 보존되어서 나옴 , 정상임 
            DebugWide.LogBlue(m0_v + "  " + m1_v + "  = " + m01_v);

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

            float timeInterval = 0.05f; //물리시뮬시 Time.delta 넣으면 안됨 , 디버그 코드에 의한 프레임드롭시 결과가 이상해짐 
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
            DebugWide.DrawQ_All();

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
            float velocity0 = Vector3.Dot(pm0.linearVelocity, unitNormal);
            float velocity1 = Vector3.Dot(pm1.linearVelocity, unitNormal);


            // Find the average coefficent of restitution.
            float averageE = (pm0.elasticity + pm1.elasticity) / 2f;

            // Calculate the final velocities.
            float finalVelocity0 =
                (((pm0.mass -
                   (averageE * pm1.mass)) * velocity0) +
                 ((1 + averageE) * pm1.mass * velocity1)) /
                (pm0.mass + pm1.mass);
            float finalVelocity1 =
                (((pm1.mass -
                   (averageE * pm0.mass)) * velocity1) +
                 ((1 + averageE) * pm0.mass * velocity0)) /
                (pm0.mass + pm1.mass);

            Vector3 prev_pm0_Vel = pm0.linearVelocity;

            pm0.linearVelocity = (
                (finalVelocity0 - velocity0) * unitNormal +
                pm0.linearVelocity);
            pm1.linearVelocity = (
                (finalVelocity1 - velocity1) * unitNormal +
                pm1.linearVelocity);

            DebugWide.LogBlue(velocity0 + "  --  " + velocity1 + "  " + averageE + "  " + pm0.elasticity + "  " + pm1.elasticity);
            DebugWide.LogBlue(finalVelocity0 + " == " + finalVelocity1);
            Vector3 tp = pm1.location + unitNormal * pm1.radius; //접점 
            Vector3 cr = Vector3.Cross(unitNormal,Vector3.up);
            DebugWide.AddDrawQ_Circle(pm0.location, pm0.radius, Color.gray);
            DebugWide.AddDrawQ_Circle(pm1.location, pm1.radius, Color.gray);
            DebugWide.AddDrawQ_Line(tp, tp + cr * 10, Color.gray);
            DebugWide.AddDrawQ_Line(tp, tp - cr * 10, Color.gray);
            DebugWide.AddDrawQ_Line(pm0.location, pm1.location, Color.blue);
            DebugWide.AddDrawQ_Line(pm0.location, pm0.location + pm0.linearVelocity, Color.green);
            DebugWide.AddDrawQ_Line(pm0.location, pm0.location + prev_pm0_Vel, Color.green);
            Vector3 pm0_velpos = pm0.location + prev_pm0_Vel;
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + velocity0 * unitNormal, Color.red);
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + finalVelocity0 * unitNormal, Color.blue);
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + (finalVelocity0- velocity0) * unitNormal, Color.white);

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


    //[System.Serializable]
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

