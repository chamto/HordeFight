using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Test_ParticleBounce2
{

    [System.Serializable]
    public class Test_ParticleBounce2 : MonoBehaviour
    {
        public const int COUNT = 50;
        public Point_mass[] particles = null;

        //-----------------------
        public float mass_0 = 1;
        public float elasticity_0 = 1;
        public float force_0 = 100;
        public bool static_0 = false; //충돌에 반응하지 않게 설정
        public float Friction_0 = 1;
        public bool Impluse_0 = true; //순간힘 
        //-----------------------
        public string line = "********************";
        //-----------------------
        public float mass_all = 1;
        public float elasticity_all = 1;
        public float force_all = 100;
        public bool static_all = false;
        public float Friction_all = 1;
        public bool Impluse_all = true; //순간힘 
        //-----------------------

        private bool _init = false;
        // Use this for initialization
        void Start()
        {
            particles = new Point_mass[COUNT];
            for (int i = 0; i < COUNT;i++)
            {
                particles[i] = new Point_mass();    
            }

            Init();

            _init = true;
        }

        public void Init()
        {
            DebugWide.LogRed("init ***********************************************************");

            DebugWide.ClearDrawQ();

            for (int i = 0; i < COUNT; i++)
            {
                particles[i].mass = mass_all;
                particles[i].elasticity = elasticity_all;
                particles[i].radius = 1;
                particles[i].linearVelocity = Vector3.zero;
                particles[i].linearAcceleration = Vector3.zero;
                particles[i].location = Misc.GetDir8_Random_AxisY() * 0.5f;
                particles[i].forces = new Vector3(0, 0.0f, 0.0f);
                particles[i].friction = Friction_all;
                particles[i].isStatic = static_all;
                particles[i].isImpluse = Impluse_all;
            }

            particles[0].mass = mass_0;
            particles[0].elasticity = elasticity_0;
            particles[0].radius = 1;
            particles[0].linearVelocity = Vector3.zero;
            particles[0].linearAcceleration = Vector3.zero;
            particles[0].location = new Vector3(-15,0,0);
            particles[0].forces = new Vector3(force_0, 0.0f, 0.0f);
            particles[0].friction = Friction_0;
            particles[0].isStatic = static_0;
            particles[0].isImpluse = Impluse_0;

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
                particles[i].Update(timeInterval);
            }


            //----------------------------------------------------
            Point_mass src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < particles.Length - 1; i++)
            {
                for (int j = i + 1; j < particles.Length; j++)
                {

                    src = particles[i];
                    dst = particles[j];
                    CollisionPush(src, dst, timeInterval);
                }
            }
            //----------------------------------------------------

        }


        public void OnDrawGizmos()
		{
            if (false == _init) return;

            DebugWide.DrawQ_All();
            //DebugWide.DrawQ_All_AfterClear();

            for (int i = 0; i < COUNT; i++)
            {
                particles[i].Draw(Color.white);
            }

            particles[0].Draw(Color.red);
        }


        public void CollisionPush(Point_mass src, Point_mass dst, float timeInterval)
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

                if (float.Epsilon >= len_dstTOsrc)
                {
                    //완전겹친상태
                    contactNormal = Misc.GetDir8_Random_AxisY();
                    //DebugWide.LogBlue(contactNormal);
                }else
                {
                    contactNormal = (dir_dstTOsrc) / len_dstTOsrc;
                }


                CalcCollisionVelocity(src, dst, contactNormal, timeInterval);

                //==========================================
                float rate_src, rate_dst;
                float sqrVel_src = src.linearVelocity.sqrMagnitude;
                float sqrVel_dst = dst.linearVelocity.sqrMagnitude;
                float sqr_sum = sqrVel_src + sqrVel_dst;
                if (Misc.IsZero(sqr_sum)) rate_src = rate_dst = 0.5f;
                else
                {
                    rate_src = 1f - (sqrVel_src / sqr_sum);
                    rate_dst = 1f - rate_src;
                }

                //------------------------------
                //한쪽만 정적일때 안밀리게 하는 예외처리 
                if (true == src.isStatic && false == dst.isStatic)
                {
                    rate_src = 0;
                    rate_dst = 1f;
                }
                if (false == src.isStatic && true == dst.isStatic)
                {
                    rate_src = 1f;
                    rate_dst = 0;
                }
                //------------------------------


                float len_bt_src = len_bitween * rate_src;
                float len_bt_dst = len_bitween * rate_dst;

                //2.완전겹친상태 
                //if (float.Epsilon >= len_dstTOsrc)
                //{
                //    contactNormal = Misc.GetDir8_Random_AxisY();
                //    len_dstTOsrc = 1f;
                //    len_bt_src = r_sum * 0.5f;
                //    len_bt_dst = r_sum * 0.5f;
                //}


                src.location += contactNormal * len_bt_src;
                dst.location += -contactNormal * len_bt_dst;
            }
        }

        public void CalcCollisionVelocity(Point_mass src, Point_mass dst , Vector3 contactNormal, float timeInterval)
        {

            Vector3 initVelocity0 = src.linearVelocity;
            Vector3 initVelocity1 = dst.linearVelocity;

            float dotVelocity0 = Vector3.Dot(initVelocity0, contactNormal);
            float dotVelocity1 = Vector3.Dot(initVelocity1, contactNormal);


            // Find the average coefficent of restitution.
            float averageE = (src.elasticity + dst.elasticity) / 2f;

            //탄성력이 1 , 질량이 1 이라고 가정할시,
            //((1 - (1 * 1)) * v0 + ((1 + 1) * 1 * v1) / 1 + 1
            //(2 * v1) / 2 = v1
            // Calculate the final velocities.
            float finalVelocity0 =
                (((src.mass -
                   (averageE * dst.mass)) * dotVelocity0) +
                 ((1 + averageE) * dst.mass * dotVelocity1)) /
                (src.mass + dst.mass);
            float finalVelocity1 =
                (((dst.mass -
                   (averageE * src.mass)) * dotVelocity1) +
                 ((1 + averageE) * src.mass * dotVelocity0)) /
                (src.mass + dst.mass);

            Vector3 prev_pm0_Vel = initVelocity0;

            //탄성력이 1 , 질량이 1 , 서로 정면으로 부딪친다고 가정할시, 
            //대상의 속도 + 튕겨지는 내 속도 + 현재 속도 = 최종 속도 
            //--> 정면 충돌시 대상의 속도만 남는다 
            //[->A의 속도 --- B의 속도<-] => [<-B의 속도 --- A의 속도->] 로 속도와 방향이 바뀐다 
            //쉽게 보면 대상의 속도(힘) 만이 적용된다고 볼 수 있다 
            if(false == src.isStatic)
            {
                src.linearVelocity = ((finalVelocity0 - dotVelocity0) * contactNormal + initVelocity0);
            }
            if(false == dst.isStatic)
            {
                dst.linearVelocity = ((finalVelocity1 - dotVelocity1) * contactNormal + initVelocity1);
            }




            //---------------------------------------------------------------
            //d1 : 1차원 , 디멘션1 
            //DebugWide.LogBlue("#  init_vel0: " + initVelocity0 + "  init_vel1: " + initVelocity1);
            //DebugWide.LogBlue("v0_d1 : " + dotVelocity0 + "  --  v1_d1 : " + dotVelocity1 + "   - averE: " + averageE );
            //DebugWide.LogRed("fv0_d1 : " + finalVelocity0 + " -- fv1_d1 : " + finalVelocity1);
            //DebugWide.LogRed("fv0_d1 - v0_d1 = " + (finalVelocity0 - dotVelocity0) + " -- fv1_d1 - v1_d1 = " + (finalVelocity1 - dotVelocity1));
            //DebugWide.LogRed("(fv0_d1 - v0_d1) + v0_d1 = " + src.linearVelocity.magnitude + "  --  (fv1_d1 - v1_d1) + v1_d1 = " + dst.linearVelocity.magnitude);
            //Vector3 tp = dst.location + contactNormal * dst.radius; //접점 
            //Vector3 cr = Vector3.Cross(contactNormal, Vector3.up);
            //DebugWide.AddDrawQ_Circle(src.location, src.radius, Color.gray);
            //DebugWide.AddDrawQ_Circle(dst.location, dst.radius, Color.gray);
            //DebugWide.AddDrawQ_Line(tp, tp + cr * 10, Color.gray);
            //DebugWide.AddDrawQ_Line(tp, tp - cr * 10, Color.gray);
            //DebugWide.AddDrawQ_Line(src.location, dst.location, Color.blue);
            //DebugWide.AddDrawQ_Line(src.location, src.location + src.linearVelocity, Color.green);
            //DebugWide.AddDrawQ_Line(src.location, src.location + prev_pm0_Vel, Color.green);
            //Vector3 pm0_velpos = src.location + prev_pm0_Vel;
            //DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + dotVelocity0 * contactNormal, Color.red);
            //DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + finalVelocity0 * contactNormal, Color.blue);
            //DebugWide.AddDrawQ_Line(pm0_velpos, pm0_velpos + (finalVelocity0 - dotVelocity0) * contactNormal, Color.white);
            //---------------------------------------------------------------
        }


    }


    [System.Serializable]
    public class Point_mass
    {
        public float mass;
        public Vector3 location; //centerOfMassLocation;
        public Vector3 linearVelocity;
        public Vector3 linearAcceleration;
        public Vector3 forces;

        public float radius;
        public float elasticity; //coefficientOfRestitution
        public float friction;

        public bool isImpluse = true; //단발성 충격힘 적용
        public bool isStatic = false; //충돌에 반응하지 않게 설정

        public void Update(float changeInTime)
        {
        
            linearVelocity *= friction; //마찰력 간단적용 1

            //-------------------------------------

            // a = F/m
            linearAcceleration = forces / mass;

            // Find the linear velocity.
            linearVelocity += linearAcceleration * changeInTime;

            // Find the new location of the center of mass.
            location += linearVelocity * changeInTime;

            //-------------------------------------

            if (isImpluse)
            {
                forces = ConstV.v3_zero; //힘 적용후 바로 초기화 - impluse
            }

        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(location, radius, color);
            //DebugWide.DrawLine(location, location + forces, color);
            DebugWide.DrawLine(location, location + linearVelocity, Color.green);
        }
    }
}

