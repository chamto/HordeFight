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
        private List<Contact> _contacts = new List<Contact>();


        //-----------------------
        public float mass_0 = 1;
        public float elasticity_0 = 1;
        public float force_0 = 100;
        public bool static_0 = false; //충돌에 반응하지 않게 설정
        //public float Friction_0 = 1;
        public float damping_0 = 1;
        public bool Impluse_0 = true; //순간힘 
        [Space]
        //-----------------------
        public string line = "********************";
        //-----------------------
        public float mass_all = 1;
        public float elasticity_all = 1;
        public float force_all = 10;
        public bool static_all = false;
        //public float Friction_all = 1;
        public float damping_all = 1;
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


        public float PowBottom = 0.8f;
        public void Init()
        {
            DebugWide.LogRed("init ***********************************************************");
            //--------------------------------------
            //pow 함수 지수부의 소수값 넣었을때 값의 변환 관찰

            //DebugWide.LogBlue(Math.Pow(damping_0,Time));
            float val0 = 100;
            for (int i=1;i<=30;i++)
            {
                val0 *= (float)Math.Pow(PowBottom, 1f / 30f);
            }
            float val1 = 100;
            for (int i = 1; i <= 60; i++)
            {
                val1 *= (float)Math.Pow(PowBottom, 1f / 60f);
            }
            DebugWide.LogBlue(val0 + "  " + val1);

            //1^x = 항상 1 , 0^x = 항상 0
            //밑값이 0.8 이고 지수부는 1/30 고정값을 100에 30회 곱한 결과 
            //100 => 80 
            //밑값이 0.8 이고 지수부는 1/60 고정값을 100에 60회 곱한 결과 
            //100 => 80
            //**** 결론 : 초당 밑값 비율만큼 감소시키는 효과 , 시간에 따라 일정하게 값을 유지시키는 목적으로 사용됨 

            //--------------------------------------
            DebugWide.ClearDrawQ();

            for (int i = 0; i < COUNT; i++)
            {
                particles[i].mass = mass_all;
                particles[i].elasticity = elasticity_all;
                particles[i].radius = 1;
                particles[i].linearVelocity = Vector3.zero;
                particles[i].linearAcceleration = Vector3.zero;
                //particles[i].location = Misc.GetDir8_Random_AxisY() * 0.5f;
                particles[i].location = Vector3.zero;
                particles[i].forces = new Vector3(0, 0.0f, 0.0f);
                //particles[i].friction = Friction_all;
                particles[i].damping = damping_all;
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
            //particles[0].friction = Friction_0;
            particles[0].damping = damping_0;
            particles[0].isStatic = static_0;
            particles[0].isImpluse = Impluse_0;

        }


        void Update_0()
        {

            if (Input.GetKeyDown(KeyCode.R))
            {
                Init();
            }

            //1초 / 30프레임 = 0.033
            float timeInterval = 0.05f; //물리시뮬시 Time.delta 넣으면 안됨 , 디버그 코드에 의한 프레임드롭시 결과가 이상해짐 
            for (int i = 0; i < COUNT; i++)
            {
                particles[i].Intergrate(timeInterval);
            }

            //접촉정보를 미리 수집하지 않아 충돌처리시 마다 접촉정보가 달라지는 문제가 있음 
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

        //public bool __oneTime = false;
        void Update()
        {


            if (Input.GetKeyDown(KeyCode.R))
            {
                Init();
            }

            //if (false == __oneTime) return;
            //__oneTime = false;

            //----------------------------------------------------

            for (int i = 1; i < COUNT; i++)
            {
                Vector3 dir = particles[0].location - particles[i].location;
                if(dir.sqrMagnitude < 3*3) //3거리안에 들어올때 
                {
                    particles[i].ApplyImpluse(dir.normalized * force_all);
                }
            }

            //----------------------------------------------------

            //1초 / 30프레임 = 0.033
            float timeInterval = 0.033f; //물리시뮬시 Time.delta 넣으면 안됨 , 디버그 코드에 의한 프레임드롭시 결과가 이상해짐 
            for (int i = 0; i < COUNT; i++)
            {
                particles[i].Intergrate(timeInterval);
            }


            GenerateContacts(); //접촉정보를 미리 수집함
            ResolveContacts(timeInterval); //수집한 접촉정보에 따라 충돌처리함 

        }

        public void GenerateContacts()
        {
            //----------------------------------------------------
            _contacts.Clear(); //접촉정보를 모두 비운다 

            Point_mass src, dst;
            Contact contact = null;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < particles.Length - 1; i++)
            {
                for (int j = i + 1; j < particles.Length; j++)
                {

                    src = particles[i];
                    dst = particles[j];
                    CollisionDetector.SphereAndSphere(src, dst, out contact);
                    _contacts.Add(contact);
                }
            }
            //----------------------------------------------------
        }

        public void ResolveContacts(float timeInterval)
        { 
            for(int i=0;i<_contacts.Count;i++)
            {
                _contacts[i].Resolve(timeInterval);
            }

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

    public class CollisionDetector
    {

        public static bool SphereAndSphere(Point_mass pm_0, Point_mass pm_1, out Contact contact)
        {
            contact = new Contact();


            Vector3 dir_dstTOsrc = pm_0.location - pm_1.location;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;
            float r_sum = (pm_0.radius + pm_1.radius);
            float sqr_r_sum = r_sum * r_sum;


            //1.두 캐릭터가 겹친상태 
            if (sqr_dstTOsrc < sqr_r_sum)
            {
                Vector3 contactNormal = ConstV.v3_zero;
                float len_dstTOsrc = (float)Math.Sqrt(sqr_dstTOsrc);
                float penetration = (r_sum - len_dstTOsrc);

                if (float.Epsilon >= len_dstTOsrc)
                {
                    //완전겹친상태
                    contactNormal = Misc.GetDir8_Random_AxisY();
                    penetration = r_sum * 0.5f; //반지름합의 평균을 침투길이로 사용한다. 이 처리를 안하면 비정상적으로 퍼지게 된다 
                }
                else
                {
                    contactNormal = (dir_dstTOsrc) / len_dstTOsrc;
                }

                contact.pm_0 = pm_0;
                contact.pm_1 = pm_1;
                contact.restitution = (pm_0.elasticity + pm_1.elasticity) * 0.5f; //평균값

                contact.contactNormal = contactNormal;
                contact.penetration = penetration;

                return true;
            }

            //2.안겹침 
            return false;
        }
    }

    public class Contact
    {
        public Point_mass pm_0 = new Point_mass();
        public Point_mass pm_1 = new Point_mass();

        //public Vector3 contactPoint;

        public Vector3 contactNormal;


        public float penetration;

        public float restitution; //반발계수

        //public Vector3[] particleMovement = new Vector3[2];

        public bool used; //사용됨을 나타냄 

        public void Init()
        {
            pm_0 = null;
            pm_1 = null;
            contactNormal = Vector3.zero;
            penetration = 0;
            restitution = 1; //완전탄성으로 설정  

            used = false;
        }

        public void Resolve(float duration)
        {
            ResolveVelocity(duration);
            ResolveInterpenetration(duration);
        }


        public void ResolveVelocity(float timeInterval)
        {

            Vector3 initVelocity0 = pm_0.linearVelocity;
            Vector3 initVelocity1 = pm_1.linearVelocity;

            float dotVelocity0 = Vector3.Dot(initVelocity0, contactNormal);
            float dotVelocity1 = Vector3.Dot(initVelocity1, contactNormal);


            // Find the average coefficent of restitution.
            //float averageE = (pm_0.elasticity + pm_1.elasticity) / 2f;
            float averageE = restitution;

            //탄성력이 1 , 질량이 1 이라고 가정할시,
            //((1 - (1 * 1)) * v0 + ((1 + 1) * 1 * v1) / 1 + 1
            //(2 * v1) / 2 = v1
            // Calculate the final velocities.
            float finalVelocity0 =
                (((pm_0.mass -
                   (averageE * pm_1.mass)) * dotVelocity0) +
                 ((1 + averageE) * pm_1.mass * dotVelocity1)) /
                (pm_0.mass + pm_1.mass);
            float finalVelocity1 =
                (((pm_1.mass -
                   (averageE * pm_0.mass)) * dotVelocity1) +
                 ((1 + averageE) * pm_0.mass * dotVelocity0)) /
                (pm_0.mass + pm_1.mass);

            //Vector3 prev_pm0_Vel = initVelocity0;

            //탄성력이 1 , 질량이 1 , 서로 정면으로 부딪친다고 가정할시, 
            //대상의 속도 + 튕겨지는 내 속도 + 현재 속도 = 최종 속도 
            //--> 정면 충돌시 대상의 속도만 남는다 
            //[->A의 속도 --- B의 속도<-] => [<-B의 속도 --- A의 속도->] 로 속도와 방향이 바뀐다 
            //쉽게 보면 대상의 속도(힘) 만이 적용된다고 볼 수 있다 
            if (false == pm_0.isStatic)
            {
                pm_0.linearVelocity = ((finalVelocity0 - dotVelocity0) * contactNormal + initVelocity0);
            }
            if (false == pm_1.isStatic)
            {
                pm_1.linearVelocity = ((finalVelocity1 - dotVelocity1) * contactNormal + initVelocity1);
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

        public void ResolveInterpenetration(float timeInterval)
        {

            float rate_src, rate_dst;
            float sqrVel_src = pm_0.linearVelocity.sqrMagnitude;
            float sqrVel_dst = pm_1.linearVelocity.sqrMagnitude;
            float sqr_sum = sqrVel_src + sqrVel_dst;
            if (Misc.IsZero(sqr_sum)) rate_src = rate_dst = 0.5f;
            else
            {
                rate_src = 1f - (sqrVel_src / sqr_sum);
                rate_dst = 1f - rate_src;
            }

            //------------------------------
            //한쪽만 정적일때 안밀리게 하는 예외처리 
            if (true == pm_0.isStatic && false == pm_1.isStatic)
            {
                rate_src = 0;
                rate_dst = 1f;
            }
            if (false == pm_0.isStatic && true == pm_1.isStatic)
            {
                rate_src = 1f;
                rate_dst = 0;
            }
            //------------------------------


            float len_bt_src = penetration * rate_src;
            float len_bt_dst = penetration * rate_dst;

            pm_0.location += contactNormal * len_bt_src;
            pm_1.location += -contactNormal * len_bt_dst;

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
        //public float friction; //마찰력
        public float damping; //제동

        public float endurance_max; //최대 지구력
        public float endurance; //현재 지구력 
        public float endurance_recovery; //지구력 회복량

        public bool isImpluse = true; //단발성 충격힘 적용
        public bool isStatic = false; //충돌에 반응하지 않게 설정

        public void Intergrate(float changeInTime)
        {

            endurance += endurance_recovery * changeInTime;
            endurance = (endurance_max > endurance) ? endurance : endurance_max;

            //-------------------------------------


            //if (endurance <= 0)
            //{
            //    linearVelocity *= (float)Math.Pow(damping, changeInTime); //초당 damping 비율 만큼 감소시킨다.

            //    location += linearVelocity * changeInTime;
            //}
            //else
            {
                // a = F/m
                linearAcceleration = forces / mass;

                linearVelocity += linearAcceleration * changeInTime;

                linearVelocity *= (float)Math.Pow(damping, changeInTime); //초당 damping 비율 만큼 감소시킨다.
                //linearVelocity *= damping; //!! 이렇게 사용하면 프레임에 따라 값의 변화가 일정하지 않게됨 

                location += linearVelocity * changeInTime;

            }



            //-------------------------------------

            if (isImpluse)
            {
                forces = ConstV.v3_zero; //힘 적용후 바로 초기화 - impluse
            }

        }

        public void ApplyForce(Vector3 force)
        {
            isImpluse = false;
            forces = force;
        }

        public void ApplyImpluse(Vector3 force)
        {
            isImpluse = true;
            forces = force; 
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(location, radius, color);
            //DebugWide.DrawLine(location, location + forces, color);
            DebugWide.DrawLine(location, location + linearVelocity, Color.green);
        }
    }
}

