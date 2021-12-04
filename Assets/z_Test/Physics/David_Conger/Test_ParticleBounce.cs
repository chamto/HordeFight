using System;
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
        public float mass_0 = 1;
        public float mass_1 = 1;
        public float elasticity_0 = 1;
        public float elasticity_1 = 1;
        public float force_0 = 100;
        public float force_1 = 100;
        public float Friction = 1;
        public bool OneHit = true;

        bool _forceApplied = false;

        // Use this for initialization
        void Start()
        {

            for (int i = 0; i < COUNT;i++)
            {
                allParticles[i] = new Point_mass();    
            }

            Init();


        }

        public void Init()
        {
            DebugWide.LogRed("init ***********************************************************");

            DebugWide.ClearDrawQ();

            allParticles[0].mass = mass_0;
            allParticles[0].elasticity = elasticity_0;
            allParticles[0].radius = 1;
            allParticles[0].linearVelocity = Vector3.zero;
            allParticles[0].linearAcceleration = Vector3.zero;
            allParticles[0].location = new Vector3(-10.0f, 0.0f, 0.0f);
            allParticles[0].forces = new Vector3(force_0, 0.0f, 0.0f);

            allParticles[1].mass = mass_1;
            allParticles[1].elasticity = elasticity_1;
            allParticles[1].radius = 1f;
            allParticles[1].linearVelocity = Vector3.zero;
            allParticles[1].linearAcceleration = Vector3.zero;
            allParticles[1].location = new Vector3(0.0f, 0, 0);
            //allParticles[1].location = new Vector3(0.0f, 0, 1f);
            allParticles[1].forces = Vector3.zero;
            //allParticles[1].forces = new Vector3(-10.0f, 0, 5.0f);
            allParticles[1].forces = new Vector3(-force_1, 0.0f, 0.0f);

            _forceApplied = false;
        }

        //private bool forceApplied = false;
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.R))
            {
                Init();
            }

            //1초 / 30프레임 = 0.033
            float timeInterval = 0.05f; //물리시뮬시 Time.delta 넣으면 안됨 , 디버그 코드에 의한 프레임드롭시 결과가 이상해짐 
            for (int i = 0; i < COUNT; i++)
            {
                //퍼센트단위 감소 마찰력 적용1이 움직임이 더 좋음 
                allParticles[i].linearVelocity *= Friction; //마찰력 간단적용 1
                //allParticles[i].linearVelocity += -allParticles[i].linearVelocity.normalized * Friction; //마찰력 간단적용 2
                allParticles[i].Update(timeInterval);
                allParticles[i].forces = ConstV.v3_zero; //힘 적용후 바로 초기화 
            }

            float m0_v = allParticles[0].linearVelocity.magnitude;
            float m1_v = allParticles[1].linearVelocity.magnitude;
            float m01_v = m0_v + m1_v; //맞부딪힐 경우 일차원 운동량값만 보존되어서 나옴
            //DebugWide.LogBlue("------------- "+m0_v + " + " + m1_v + "  = " + m01_v);


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
                if (false == _forceApplied)
                {
                    if(OneHit) _forceApplied = true;
                    //HandleCollision(ref allParticles[0], ref allParticles[1], timeInterval);
                    CollisionPush(ref allParticles[0], ref allParticles[1]);
                }

            }

        }


		public void OnDrawGizmos()
		{
            DebugWide.DrawQ_All();
            //DebugWide.DrawQ_All_AfterClear();

            allParticles[0].Draw(Color.white);
            allParticles[1].Draw(Color.black);
		}


        public void CollisionPush(ref Point_mass src, ref Point_mass dst)
        {

            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다

            Vector3 dir_dstTOsrc = src.location - dst.location;
            Vector3 contactNormal = ConstV.v3_zero;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;

            float r_sum = (src.radius + dst.radius);
            float sqr_r_sum = r_sum * r_sum;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dstTOsrc < sqr_r_sum)
            {

                float len_dstTOsrc = (float)Math.Sqrt(sqr_dstTOsrc);
                float len_bitween = (r_sum - len_dstTOsrc);
                contactNormal = (dir_dstTOsrc) / len_dstTOsrc;

                CalcCollisionVelocity(ref src, ref dst, contactNormal);

                //==========================================
                float rate_src, rate_dst;
                float f_sum = src.linearVelocity.sqrMagnitude + dst.linearVelocity.sqrMagnitude;
                if (Misc.IsZero(f_sum)) rate_src = rate_dst = 0.5f;
                else
                {
                    rate_src = 1f - (dst.linearVelocity.sqrMagnitude / f_sum);
                    rate_dst = 1f - rate_src;
                }

                float len_bt_src = len_bitween * rate_src;
                float len_bt_dst = len_bitween * rate_dst;



                //2.완전겹친상태 
                if (float.Epsilon >= len_dstTOsrc)
                {
                    contactNormal = Misc.GetDir8_Random_AxisY();
                    len_dstTOsrc = 1f;
                    len_bt_src = r_sum * 0.5f;
                    len_bt_dst = r_sum * 0.5f;
                }


                src.location += contactNormal * len_bt_src;
                dst.location += -contactNormal * len_bt_dst;
            }
        }

        public void CalcCollisionVelocity(ref Point_mass pm0, ref Point_mass pm1 , Vector3 contactNormal)
        {
            float velocity0 = Vector3.Dot(pm0.linearVelocity, contactNormal);
            float velocity1 = Vector3.Dot(pm1.linearVelocity, contactNormal);


            // Find the average coefficent of restitution.
            float averageE = (pm0.elasticity + pm1.elasticity) / 2f;

            //탄성력이 1 , 질량이 1 이라고 가정할시,
            //((1 - (1 * 1)) * v0 + ((1 + 1) * 1 * v1) / 1 + 1
            //(2 * v1) / 2 = v1
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

            //탄성력이 1 , 질량이 1 , 서로 정면으로 부딪친다고 가정할시, 
            //대상의 속도 + 튕겨지는 내 속도 + 현재 속도 = 최종 속도 
            //--> 정면 충돌시 대상의 속도만 남는다 
            //[->A의 속도 --- B의 속도<-] => [<-B의 속도 --- A의 속도->] 로 속도와 방향이 바뀐다 
            //쉽게 보면 대상의 속도(힘) 만이 적용된다고 볼 수 있다   
            pm0.linearVelocity = (
                (finalVelocity0 - velocity0) * contactNormal +
                pm0.linearVelocity);
            pm1.linearVelocity = (
                (finalVelocity1 - velocity1) * contactNormal +
                pm1.linearVelocity);


            //---------------------------------------------------------------
            //d1 : 1차원 , 디멘션1 
            DebugWide.LogRed("v0_d1 : " + velocity0 + "  --  v1_d1 : " + velocity1 + "   - averE: " + averageE + "  elastic0: " + pm0.elasticity + "  elastic1: " + pm1.elasticity);
            DebugWide.LogRed("fv0_d1 : " + finalVelocity0 + " -- fv1_d1 : " + finalVelocity1);
            DebugWide.LogBlue("fv0_d1 - v0_d1 = " + (finalVelocity0 - velocity0) + " -- fv1_d1 - v1_d1 = " + (finalVelocity1 - velocity1));
            DebugWide.LogRed("(fv0_d1 - v0_d1) + v0_d1 = " + pm0.linearVelocity.magnitude + "  --  (fv1_d1 - v1_d1) + v1_d1 = " + pm1.linearVelocity.magnitude);
            Vector3 tp = pm1.location + contactNormal * pm1.radius; //접점 
            Vector3 cr = Vector3.Cross(contactNormal, Vector3.up);
            DebugWide.AddDrawQ_Circle(pm0.location, pm0.radius, Color.gray);
            DebugWide.AddDrawQ_Circle(pm1.location, pm1.radius, Color.gray);
            DebugWide.AddDrawQ_Line(tp, tp + cr * 10, Color.gray);
            DebugWide.AddDrawQ_Line(tp, tp - cr * 10, Color.gray);
            DebugWide.AddDrawQ_Line(pm0.location, pm1.location, Color.blue);
            DebugWide.AddDrawQ_Line(pm0.location, pm0.location + pm0.linearVelocity, Color.green);
            DebugWide.AddDrawQ_Line(pm0.location, pm0.location + prev_pm0_Vel, Color.green);
            Vector3 pm0_velpos = pm0.location + prev_pm0_Vel;
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + velocity0 * contactNormal, Color.red);
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + finalVelocity0 * contactNormal, Color.blue);
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + (finalVelocity0 - velocity0) * contactNormal, Color.white);
            //---------------------------------------------------------------
        }

        public void HandleCollision(ref Point_mass pm0, ref Point_mass pm1, float changeInTime)
        {

            Vector3 separationDistance = pm0.location - pm1.location;

            // Find the outgoing velicities.
            //
            /* First, normalize the displacement vector because it's 
            perpendicular to the collision. */
            Vector3 contactNormal = VOp.Normalize(separationDistance);


            /* Compute the projection of the velocities in the direction
            perpendicular to the collision. */
            float velocity0 = Vector3.Dot(pm0.linearVelocity, contactNormal);
            float velocity1 = Vector3.Dot(pm1.linearVelocity, contactNormal);


            // Find the average coefficent of restitution.
            float averageE = (pm0.elasticity + pm1.elasticity) / 2f;

            //탄성력이 1 , 질량이 1 이라고 가정할시,
            //((1 - (1 * 1)) * v0 + ((1 + 1) * 1 * v1) / 1 + 1
            //(2 * v1) / 2 = v1
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

            //탄성력이 1 , 질량이 1 , 서로 정면으로 부딪친다고 가정할시, 
            //대상의 속도 + 튕겨지는 내 속도 + 현재 속도 = 최종 속도 
            //--> 정면 충돌시 대상의 속도만 남는다 
            //[->A의 속도 --- B의 속도<-] => [<-B의 속도 --- A의 속도->] 로 속도와 방향이 바뀐다 
            //쉽게 보면 대상의 속도(힘) 만이 적용된다고 볼 수 있다   
            pm0.linearVelocity = (
                (finalVelocity0 - velocity0) * contactNormal +
                pm0.linearVelocity);
            pm1.linearVelocity = (
                (finalVelocity1 - velocity1) * contactNormal +
                pm1.linearVelocity);

            //---------------------------------------------------------------
            //d1 : 1차원 , 디멘션1 
            DebugWide.LogRed("v0_d1 : "+velocity0 + "  --  v1_d1 : " + velocity1 + "   - averE: " + averageE + "  elastic0: " + pm0.elasticity + "  elastic1: " + pm1.elasticity);
            DebugWide.LogRed("fv0_d1 : "+finalVelocity0 + " -- fv1_d1 : " + finalVelocity1);
            DebugWide.LogBlue("fv0_d1 - v0_d1 = " + (finalVelocity0- velocity0) + " -- fv1_d1 - v1_d1 = " + (finalVelocity1- velocity1));
            DebugWide.LogRed("(fv0_d1 - v0_d1) + v0_d1 = " + pm0.linearVelocity.magnitude + "  --  (fv1_d1 - v1_d1) + v1_d1 = " + pm1.linearVelocity.magnitude);
            Vector3 tp = pm1.location + contactNormal * pm1.radius; //접점 
            Vector3 cr = Vector3.Cross(contactNormal,Vector3.up);
            DebugWide.AddDrawQ_Circle(pm0.location, pm0.radius, Color.gray);
            DebugWide.AddDrawQ_Circle(pm1.location, pm1.radius, Color.gray);
            DebugWide.AddDrawQ_Line(tp, tp + cr * 10, Color.gray);
            DebugWide.AddDrawQ_Line(tp, tp - cr * 10, Color.gray);
            DebugWide.AddDrawQ_Line(pm0.location, pm1.location, Color.blue);
            DebugWide.AddDrawQ_Line(pm0.location, pm0.location + pm0.linearVelocity, Color.green);
            DebugWide.AddDrawQ_Line(pm0.location, pm0.location + prev_pm0_Vel, Color.green);
            Vector3 pm0_velpos = pm0.location + prev_pm0_Vel;
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + velocity0 * contactNormal, Color.red);
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + finalVelocity0 * contactNormal, Color.blue);
            DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + (finalVelocity0- velocity0) * contactNormal, Color.white);
            //---------------------------------------------------------------

            //
            // Convert the velocities to accelerations.
            //
            Vector3 acceleration1 =
                pm0.linearVelocity / changeInTime;
            Vector3 acceleration2 =
                pm1.linearVelocity / changeInTime;

            //부딪힐때마다 속도가 올라가는 것은 정상적이지 않은것 같음? , 마찰력이 있다면 가능한가? 
            // Find the force on each ball.
            //pm0.forces = (acceleration1 * pm0.mass);
            //pm1.forces = (acceleration2 * pm1.mass);

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

