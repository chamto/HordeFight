using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_4
{
    [System.Serializable]
    public class Stage
    {
        public Transform _tr_test = null;
        public Transform _tr_line_a = null;
        public Transform _tr_line_b = null;

        public Transform _tr_platoon_0 = null;
        public Transform _tr_squard_0 = null;
        public Transform _tr_squard_1 = null;
        public Transform _tr_squard_2 = null;
        public Transform _tr_squard_3 = null;

        public SweepPrune _sweepPrune = new SweepPrune();
        //private List<Contact> _contacts = new List<Contact>();

        public float _formation_platoon_speed = 10;
        public float _formation_squard_speed = 10;
        public float _radius_geo = 0.5f;
        public float _radius_body = 0.5f;
        public float _mass = 1f;
        public float _maxForce = 40f;
        public float _maxSpeed = 10f;
        public float _withstand = 1f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;

        public float _weightArrive = 20;
        public float _weightOffsetPursuit = 20;
        public float _weightObstacleAvoidance = 20;
        public float _weightSeparation = 20;
        public float _weightAlignment = 20;
        public float _weightCohesion = 20;
        public float _viewDistance = 10; //시야거리 

        //public bool _Nonpenetration = true;
        public bool _ObjNonpenetration = true;
        public bool _StrNonpenetration = true;

        public float _minRange = 0;
        public float _maxRange = 0.5f;

        public int _Iterations = 20;
        public bool _Squard_0_Solo_Activity = false;
        public bool _Squard_1_Solo_Activity = false;
        public bool _Squard_2_Solo_Activity = false;
        public bool _Squard_3_Solo_Activity = false;

        public Platoon _Platoon_0 = null;
        public Squard _Squard_0 = null;
        public Squard _Squard_1 = null;
        public Squard _Squard_2 = null;
        public Squard _Squard_3 = null;

        public bool _init = false;

        public void Init()
        {
            _init = true;

            _tr_test = GameObject.Find("Test").transform;
            _tr_line_a = GameObject.Find("line_a").transform;
            _tr_line_b = GameObject.Find("line_b").transform;

            _tr_platoon_0 = GameObject.Find("tr_platoon_0").transform;
            _tr_squard_0 = GameObject.Find("tr_squard_0").transform;
            _tr_squard_1 = GameObject.Find("tr_squard_1").transform;
            _tr_squard_2 = GameObject.Find("tr_squard_2").transform;
            _tr_squard_3 = GameObject.Find("tr_squard_3").transform;


            ObjectManager.Inst.Init();

            //------

            EntityMgr.list.Clear();

            Unit unit = null;
            for (int i = 0; i < 10; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
                unit._collision._list_idx = id; //임시처리 
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_num = 0;
                unit._disposition._squard_num = 0;
                unit._disposition._squard_pos = i;
                //unit._steeringBehavior.ArriveOn();
            }

            for (int i = 0; i < 4; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
                unit._collision._list_idx = id; //임시처리
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_num = 0;
                unit._disposition._squard_num = 1;
                unit._disposition._squard_pos = i;
                //unit._steeringBehavior.ArriveOn();
            }
            for (int i = 0; i < 4; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
                unit._collision._list_idx = id; //임시처리
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_num = 0;
                unit._disposition._squard_num = 2;
                unit._disposition._squard_pos = i;
                //unit._steeringBehavior.ArriveOn();
            }
            for (int i = 0; i < 15; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
                unit._collision._list_idx = id; //임시처리
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_num = 0;
                unit._disposition._squard_num = 3;
                unit._disposition._squard_pos = i;
                //unit._steeringBehavior.ArriveOn();
            }

            _Platoon_0 = Platoon.Create_Platoon(EntityMgr.list);
            _Platoon_0._targetPos = _tr_platoon_0.position;
            _Platoon_0._pos = _tr_platoon_0.position;
            //_Platoon_0.ApplyFormationOffset_Fixed();
            //_Platoon_0.ApplyFormationOffset_Follow();

            _Squard_0 = _Platoon_0._squards[0];
            _Squard_1 = _Platoon_0._squards[1];
            _Squard_2 = _Platoon_0._squards[2];
            _Squard_3 = _Platoon_0._squards[3];

            for(int i=0;i< EntityMgr.list.Count;i++)
            {
                Unit u = EntityMgr.list[i];
                u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
                //u._steeringBehavior.ObstacleAvoidanceOn();
                //u._steeringBehavior.FlockingOn();
                //u._steeringBehavior.SeparationOn(); //비침투 알고리즘 문제점을 어느정도 해결해 준다 
            }

            //==============================

            //충돌검출기 초기화 
            List<SweepPrune.CollisionObject> collObj = new List<SweepPrune.CollisionObject>();
            for (int i = 0; i < EntityMgr.list.Count; i++)
            {
                collObj.Add(EntityMgr.list[i]._collision);
            }
            _sweepPrune.Initialize(collObj);

            //==============================

        }


        public void Update(float deltaTime)
        {

            _Platoon_0._speed = _formation_platoon_speed;
            _Platoon_0._targetPos = _tr_platoon_0.position;
            _Platoon_0.Update(deltaTime);

            _Squard_0._speed = _formation_squard_speed;
            _Squard_1._speed = _formation_squard_speed;
            _Squard_2._speed = _formation_squard_speed;
            _Squard_3._speed = _formation_squard_speed;

            _Squard_0._Solo_Activity = _Squard_0_Solo_Activity;
            _Squard_1._Solo_Activity = _Squard_1_Solo_Activity;
            _Squard_2._Solo_Activity = _Squard_2_Solo_Activity;
            _Squard_3._Solo_Activity = _Squard_3_Solo_Activity;


            if (true == _Squard_0._Solo_Activity)
            {
                _Squard_0._targetPos = _tr_squard_0.position;
            }
            else
            {
                _tr_squard_0.position = _Squard_0._targetPos;
            }
            if (true == _Squard_1._Solo_Activity)
            {
                _Squard_1._targetPos = _tr_squard_1.position;
            }
            else
            {
                _tr_squard_1.position = _Squard_1._targetPos;
            }
            if (true == _Squard_2._Solo_Activity)
            {
                _Squard_2._targetPos = _tr_squard_2.position;
            }
            else
            {
                _tr_squard_2.position = _Squard_2._targetPos;
            }
            if (true == _Squard_3._Solo_Activity)
            {
                _Squard_3._targetPos = _tr_squard_3.position;
            }
            else
            {
                _tr_squard_3.position = _Squard_3._targetPos;
            }


            KeyInput();

            //==============================================

            foreach (Unit v in EntityMgr.list)
            {
                v._steeringBehavior._targetPos = v._squard._pos;

                v._radius_geo = _radius_geo;
                v.SetRadius(_radius_body);
                v._mass = _mass;
                v._maxSpeed = _maxSpeed;
                v._maxForce = _maxForce;
                v._damping = _Friction;
                v._anglePerSecond = _anglePerSecond;

                v._steeringBehavior._weightArrive = _weightArrive;
                v._steeringBehavior._weightOffsetPursuit = _weightOffsetPursuit;
                v._steeringBehavior._weightObstacleAvoidance = _weightObstacleAvoidance;
                v._steeringBehavior._weightSeparation = _weightSeparation;
                v._steeringBehavior._weightAlignment = _weightAlignment;
                v._steeringBehavior._weightCohesion = _weightCohesion;
                v._steeringBehavior._viewDistance = _viewDistance;

                //v._isNonpenetration = _Nonpenetration;
                v.Update(deltaTime);

            }

            ResolveContacts(deltaTime);

            ObjectManager.Inst.Update(deltaTime);
        }

        //public void GenerateContacts()
        //{
        //    _contacts.Clear(); //접촉정보를 모두 비운다 

        //    //==============================================
        //    //sweepPrune 삽입정렬 및 접촉정보 수집
        //    //==============================================
        //    for (int i = 0; i < EntityMgr.list.Count; i++)
        //    {
        //        _sweepPrune.SetEndPoint(EntityMgr.list[i]._collision); //경계상자 위치 갱신
        //    }

        //    _sweepPrune.UpdateXZ();

        //    foreach (SweepPrune.UnOrderedEdgeKey key in _sweepPrune.GetOverlap())
        //    {
        //        Unit src = EntityMgr.list[key._V0];
        //        Unit dst = EntityMgr.list[key._V1];

        //        if (src == dst) continue;

        //        if (_ObjNonpenetration)
        //        {
        //            //CollisionPush(src, dst);
        //            Contact contact;
        //            if(true == CollisionDetector.SphereAndSphere(src, dst, out contact))
        //                _contacts.Add(contact);
        //        }
        //    }
        //}

        public void ResolveContacts(float timeInterval)
        {

            int Iterations = _Iterations;
            int iterationsUsed = 0;
            while(iterationsUsed < Iterations )
            {
                int collCount = 0;

                //---------------------------------------------------
                //접촉정보를 따로 모아서 계산하는 방식은 떠는 현상때문에 사용안함 
                //GenerateContacts();
                ////DebugWide.LogGreen(calcCount + "  ------ " +_contacts.Count);
                //if (0 == _contacts.Count) break;
                //for (int i = 0; i < _contacts.Count; i++)
                //{
                //    _contacts[i].Resolve(timeInterval);
                //}
                //---------------------------------------------------

                //==============================================
                //sweepPrune 삽입정렬 및 충돌처리 
                //==============================================
                for (int i = 0; i < EntityMgr.list.Count; i++)
                {
                    _sweepPrune.SetEndPoint(EntityMgr.list[i]._collision); //경계상자 위치 갱신
                }

                _sweepPrune.UpdateXZ();

                foreach (SweepPrune.UnOrderedEdgeKey key in _sweepPrune.GetOverlap())
                {
                    Unit src = EntityMgr.list[key._V0];
                    Unit dst = EntityMgr.list[key._V1];

                    if (src == dst) continue;

                    if (_ObjNonpenetration)
                    {
                        Contact contact;
                        if(true == CollisionDetector.SphereAndSphere(src, dst, out contact)) //객체가 원이므로 원과원 검사를 해야함 
                        {
                            contact.Resolve(timeInterval);
                            collCount++;
                        }

                        //if (true == CollisionPush(src, dst))
                            //collCount++;
                    }

                }

                //==============================================

                foreach (Unit v in EntityMgr.list)
                {
                    //==========================================
                    //동굴벽과 캐릭터 충돌처리 

                    //객체의 반지름이 <0.1 ~ 0.99> 범위에 있어야 한다.
                    //float maxR = Mathf.Clamp(v._radius, 0.1, 0.99); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 

                    if (_StrNonpenetration)
                    {
                        Vector3 calcPos;
                        bool isColl = GridManager.Inst.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius_geo, out calcPos);
                        if(isColl)
                        {
                            v.SetPos(calcPos);
                            collCount++;
                        }
                    }

                    //==========================================
                }

                //DebugWide.LogBlue(" spCC : " + _sweepPrune._calcCount);

                iterationsUsed++;

                //DebugWide.LogGreen(calcCount + "  ------ " + collCount);
                if (0 == collCount) break; //충돌횟수가 0이라면 더이상 계산 할 것이 없음 
            }
            //DebugWide.LogBlue(calcCount  + "  " );

        }

        public void KeyInput()
        {
            const float MOVE_LENGTH = 1f;
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 n = _Platoon_0._targetPos - _Platoon_0._pos;
                n = VOp.Normalize(n);
                _Platoon_0._targetPos += n * MOVE_LENGTH;
                _Platoon_0._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoon_0._targetPos;
            }
            if (Input.GetKey(KeyCode.S))
            {
                Vector3 n = _Platoon_0._targetPos - _Platoon_0._pos;
                n = -VOp.Normalize(n);
                _Platoon_0._targetPos += n * MOVE_LENGTH;
                _Platoon_0._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoon_0._targetPos;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Vector3 n = _Platoon_0._targetPos - _Platoon_0._pos;
                n = -VOp.PerpN(n, Vector3.up);
                _Platoon_0._targetPos += n * MOVE_LENGTH;
                _Platoon_0._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoon_0._targetPos;
            }
            if (Input.GetKey(KeyCode.D))
            {
                Vector3 n = _Platoon_0._targetPos - _Platoon_0._pos;
                n = VOp.PerpN(n, Vector3.up);
                _Platoon_0._targetPos += n * MOVE_LENGTH;
                _Platoon_0._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoon_0._targetPos;
            }

        }

        public bool CollisionPush(Unit src, Unit dst)
        {
            if (null == src || null == dst) return false;


            Vector3 dir_dstTOsrc = src._pos - dst._pos;
            Vector3 n = ConstV.v3_zero;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;

            float r_sum = (src._radius_body + dst._radius_body);
            float sqr_r_sum = r_sum * r_sum;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dstTOsrc < sqr_r_sum)
            {

                //==========================================
                float rate_src, rate_dst;
                float f_sum = src._withstand + dst._withstand;
                if (Misc.IsZero(f_sum)) rate_src = rate_dst = 0.5f;
                else
                {
                    rate_src = 1f - (src._withstand / f_sum);
                    rate_dst = 1f - rate_src;
                }


                //n = Misc.GetDir8_Normal3D(dir_dstTOsrc); //8방향으로만 밀리게 한다 
                n = VOp.Normalize(dir_dstTOsrc);

                float len_dstTOsrc = (float)Math.Sqrt(sqr_dstTOsrc);
                float len_bitween = (r_sum - len_dstTOsrc);
                float len_bt_src = len_bitween * rate_src;
                float len_bt_dst = len_bitween * rate_dst;

                //2.완전겹친상태 
                if (float.Epsilon >= len_dstTOsrc)
                {
                    n = Misc.GetDir8_Random_AxisY();
                    len_dstTOsrc = 1f;
                    len_bt_src = r_sum * 0.5f;
                    len_bt_dst = r_sum * 0.5f;
                }

                src.SetPos(src._pos + n * len_bt_src);
                dst.SetPos(dst._pos - n * len_bt_dst);

                return true;
            }

            return false;
        }

        public void OnDrawGizmos()
        {
            if (false == _init) return;

            DebugWide.DrawLine(_tr_test.position, _tr_line_a.position, Color.white);
            DebugWide.DrawLine(_tr_test.position, _tr_line_b.position, Color.white);


            _Platoon_0.Draw(Color.green); //소대 출력

            Color color = Color.black;
            foreach (Unit v in EntityMgr.list)
            {
                color = Color.black;

                if (null != v._squard && v == v._squard._units[0])
                    color = Color.yellow;
                if (0 == v._id)
                    color = Color.red;

                v.Draw(color);
            }


        }
    }

    //======================================================

    public class CollisionDetector
    {

        public static bool SphereAndSphere(BaseEntity pm_0, BaseEntity pm_1, out Contact contact)
        {
            contact = new Contact();

            //충돌비활성일때 처리하지 않는다 
            if (false == pm_0._isCollision || false == pm_1._isCollision)
                return false;

            Vector3 dir_dstTOsrc = pm_0._pos - pm_1._pos;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;
            float r_sum = (pm_0._radius_body + pm_1._radius_body);
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
                contact.restitution = (pm_0._elasticity + pm_1._elasticity) * 0.5f; //평균값

                contact.contactNormal = contactNormal;
                contact.penetration = penetration;

                return true;
            }

            //2.안겹침 
            return false;
        }
    }

    public struct Contact
    {
        public BaseEntity pm_0;
        public BaseEntity pm_1;

        //public Vector3 contactPoint;

        public Vector3 contactNormal;

        public float penetration;

        public float restitution; //반발계수


        public void Init()
        {
            pm_0 = null;
            pm_1 = null;
            contactNormal = Vector3.zero;
            penetration = 0;
            restitution = 1; //완전탄성으로 설정  

        }

        public void Resolve(float duration)
        {
            ResolveVelocity(duration);
            ResolveInterpenetration(duration);
        }


        public void ResolveVelocity(float timeInterval)
        {

            Vector3 initVelocity0 = pm_0._velocity;
            Vector3 initVelocity1 = pm_1._velocity;

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
                (((pm_0._mass -
                   (averageE * pm_1._mass)) * dotVelocity0) +
                 ((1 + averageE) * pm_1._mass * dotVelocity1)) /
                (pm_0._mass + pm_1._mass);
            float finalVelocity1 =
                (((pm_1._mass -
                   (averageE * pm_0._mass)) * dotVelocity1) +
                 ((1 + averageE) * pm_0._mass * dotVelocity0)) /
                (pm_0._mass + pm_1._mass);

            //Vector3 prev_pm0_Vel = initVelocity0;

            //탄성력이 1 , 질량이 1 , 서로 정면으로 부딪친다고 가정할시, 
            //대상의 속도 + 튕겨지는 내 속도 + 현재 속도 = 최종 속도 
            //--> 정면 충돌시 대상의 속도만 남는다 
            //[->A의 속도 --- B의 속도<-] => [<-B의 속도 --- A의 속도->] 로 속도와 방향이 바뀐다 
            //쉽게 보면 대상의 속도(힘) 만이 적용된다고 볼 수 있다 
            if (false == pm_0._isStatic)
            {
                pm_0._velocity = ((finalVelocity0 - dotVelocity0) * contactNormal + initVelocity0);
            }
            if (false == pm_1._isStatic)
            {
                pm_1._velocity = ((finalVelocity1 - dotVelocity1) * contactNormal + initVelocity1);
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
            float sqrVel_src = pm_0._velocity.sqrMagnitude;
            float sqrVel_dst = pm_1._velocity.sqrMagnitude;
            float sqr_sum = sqrVel_src + sqrVel_dst;
            if (Misc.IsZero(sqr_sum)) rate_src = rate_dst = 0.5f;
            else
            {
                rate_src = 1f - (sqrVel_src / sqr_sum);
                rate_dst = 1f - rate_src;
            }

            //------------------------------
            //한쪽만 정적일때 안밀리게 하는 예외처리 
            if (true == pm_0._isStatic && false == pm_1._isStatic)
            {
                rate_src = 0;
                rate_dst = 1f;
            }
            if (false == pm_0._isStatic && true == pm_1._isStatic)
            {
                rate_src = 1f;
                rate_dst = 0;
            }
            //------------------------------
            //rate_src = rate_dst = 0.5f; //test
            //rate_src = 0;
            //rate_dst = 1;

            float len_bt_src = penetration * rate_src;
            float len_bt_dst = penetration * rate_dst;

            //pm_0._pos += contactNormal * len_bt_src;
            //pm_1._pos += -contactNormal * len_bt_dst;

            pm_0.SetPos(pm_0._pos + contactNormal * len_bt_src);
            pm_1.SetPos(pm_1._pos - contactNormal * len_bt_dst);

        }

    }

}//end namespace



