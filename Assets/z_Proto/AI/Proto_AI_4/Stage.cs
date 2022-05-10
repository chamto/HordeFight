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

        public int _Iterations = 5;
        public bool _Squard_0_Solo_Activity = false;
        public bool _Squard_1_Solo_Activity = false;
        public bool _Squard_2_Solo_Activity = false;
        public bool _Squard_3_Solo_Activity = false;

        public Platoon[] _Platoons = new Platoon[2];
        public Squad[] _Squads = new Squad[2];

        public bool _init = false;

        public void Init()
        {
            _init = true;

            _tr_test = GameObject.Find("Test").transform;
            _tr_line_a = GameObject.Find("line_a").transform;
            _tr_line_b = GameObject.Find("line_b").transform;

            _tr_platoon_0 = GameObject.Find("tr_platoon_0").transform;
            _tr_squard_0 = GameObject.Find("__tr_squard_0_0").transform;
            _tr_squard_1 = GameObject.Find("__tr_squard_0_1").transform;
            _tr_squard_2 = GameObject.Find("__tr_squard_0_2").transform;
            _tr_squard_3 = GameObject.Find("__tr_squard_0_3").transform;


            ObjectManager.Inst.Init();

            //----------------------------------------------

            EntityMgr.list.Clear();

            //InitPlatton_SQD1(0);
            //InitPlatton_SQD4_Cross(0);
            InitSquadTest();
            //----------------------------------------------

            //충돌검출기 초기화 
            List<SweepPrune.CollisionObject> collObj = new List<SweepPrune.CollisionObject>();
            for (int i = 0; i < EntityMgr.list.Count; i++)
            {
                collObj.Add(EntityMgr.list[i]._collision);
            }
            _sweepPrune.Initialize(collObj);

            //----------------------------------------------

        }

        public void InitSquadTest()
        {
            Unit unit = null;
            for (int i = 0; i < 20; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
                unit._collision._list_idx = id; //임시처리 
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_id = -1;
                unit._disposition._squad_id = 0;
                unit._disposition._unit_pos = i;
                //unit._steeringBehavior.ArriveOn(); //도착 활성 
            }

            _Squads[0] = Squad.Create(0,EntityMgr.list);
            _Squads[0]._targetPos = _tr_platoon_0.position;
            _Squads[0]._pos = _tr_platoon_0.position;
            //_Squads[0].ApplyFormation_SQD1_Width();
            //_Squads[0].ApplyFormation_SQD1_Height();
            //_Squads[0].ApplyFormation_SQD1_Spike();
            //_Squads[0].ApplyFormation_String();
            _Squads[0].ApplyFormation_String2();

            for (int i = 0; i < EntityMgr.list.Count; i++)
            {
                Unit u = EntityMgr.list[i];
                //u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
                //u._steeringBehavior.ObstacleAvoidanceOn();
                //u._steeringBehavior.FlockingOn();
                //u._steeringBehavior.SeparationOn(); //비침투 알고리즘 문제점을 어느정도 해결해 준다 

                //if(0 == u._id)
                {
                    u._steeringBehavior.OffsetPursuitOn(u._disposition._belong_formation, u._disposition._offset);
                }
            }
        }

        //public void InitPlatton_SQD1(int platId)
        //{
        //    Unit unit = null;
        //    for (int i = 0; i < 20; i++)
        //    {
        //        unit = new Unit();
        //        int id = EntityMgr.Add(unit);
        //        unit._collision._list_idx = id; //임시처리 
        //        unit.Init(id, 0.5f, new Vector3(17, 0, 12));
        //        unit._disposition._platoon_num = 0;
        //        unit._disposition._squard_num = 0;
        //        unit._disposition._squard_pos = i;
        //        //unit._steeringBehavior.ArriveOn();
        //    }

        //    _Platoon[platId] = Platoon.Create_Platoon(EntityMgr.list);
        //    _Platoon[platId]._targetPos = _tr_platoon_0.position;
        //    _Platoon[platId]._pos = _tr_platoon_0.position;
        //    //_Platoon[platId].ApplyFormation_SQD1_Width();
        //    _Platoon[platId].ApplyFormation_SQD1_Height();


        //    for (int i = 0; i < EntityMgr.list.Count; i++)
        //    {
        //        Unit u = EntityMgr.list[i];
        //        //u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
        //        //u._steeringBehavior.ObstacleAvoidanceOn();
        //        //u._steeringBehavior.FlockingOn();
        //        //u._steeringBehavior.SeparationOn(); //비침투 알고리즘 문제점을 어느정도 해결해 준다 

        //        //if(0 == u._id)
        //        {
        //            u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
        //        }
        //    }
        //}

        //public void InitPlatton_SQD4_Cross(int platId)
        //{
        //    Unit unit = null;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        unit = new Unit();
        //        int id = EntityMgr.Add(unit);
        //        unit._collision._list_idx = id; //임시처리 
        //        unit.Init(id, 0.5f, new Vector3(17, 0, 12));
        //        unit._disposition._platoon_num = 0;
        //        unit._disposition._squard_num = 0;
        //        unit._disposition._squard_pos = i;
        //        //unit._steeringBehavior.ArriveOn();
        //    }

        //    for (int i = 0; i < 4; i++)
        //    {
        //        unit = new Unit();
        //        int id = EntityMgr.Add(unit);
        //        unit._collision._list_idx = id; //임시처리
        //        unit.Init(id, 0.5f, new Vector3(17, 0, 12));
        //        unit._disposition._platoon_num = 0;
        //        unit._disposition._squard_num = 1;
        //        unit._disposition._squard_pos = i;
        //        //unit._steeringBehavior.ArriveOn();
        //    }
        //    for (int i = 0; i < 4; i++)
        //    {
        //        unit = new Unit();
        //        int id = EntityMgr.Add(unit);
        //        unit._collision._list_idx = id; //임시처리
        //        unit.Init(id, 0.5f, new Vector3(17, 0, 12));
        //        unit._disposition._platoon_num = 0;
        //        unit._disposition._squard_num = 2;
        //        unit._disposition._squard_pos = i;
        //        //unit._steeringBehavior.ArriveOn();
        //    }
        //    for (int i = 0; i < 15; i++)
        //    {
        //        unit = new Unit();
        //        int id = EntityMgr.Add(unit);
        //        unit._collision._list_idx = id; //임시처리
        //        unit.Init(id, 0.5f, new Vector3(17, 0, 12));
        //        unit._disposition._platoon_num = 0;
        //        unit._disposition._squard_num = 3;
        //        unit._disposition._squard_pos = i;
        //        //unit._steeringBehavior.ArriveOn();
        //    }

        //    _Platoon[platId] = Platoon.Create_Platoon(EntityMgr.list);
        //    _Platoon[platId]._targetPos = _tr_platoon_0.position;
        //    _Platoon[platId]._pos = _tr_platoon_0.position;
        //    _Platoon[platId].ApplyFormation_SQD4_Cross();
        //    //_Platoon[platId].ApplyFormation_SQD4_String();


        //    for (int i = 0; i < EntityMgr.list.Count; i++)
        //    {
        //        Unit u = EntityMgr.list[i];
        //        //u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
        //        //u._steeringBehavior.ObstacleAvoidanceOn();
        //        //u._steeringBehavior.FlockingOn();
        //        //u._steeringBehavior.SeparationOn(); //비침투 알고리즘 문제점을 어느정도 해결해 준다 

        //        //if(0 == u._id)
        //        {
        //            u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
        //        }
        //    }
        //}


        public void Update(float deltaTime)
        {

            _Squads[0]._speed = _formation_squard_speed;
            _Squads[0]._targetPos = _tr_platoon_0.position;
            _Squads[0].Update(deltaTime);

            //----------------------------------------------

            //_Platoons[0]._speed = _formation_platoon_speed;
            //_Platoons[0]._targetPos = _tr_platoon_0.position;
            //_Platoons[0].Update(deltaTime);

            //for(int i=0;i< _Platoons[0]._squad_count;i++)
            //{
            //    _Platoons[0]._squads[i]._speed = _formation_squard_speed;

            //    Squad squad = _Platoons[0]._squads[i];

            //    //------------------------------
            //    bool activity = false;
            //    Transform tr_squard = null;
            //    if (0 == i)
            //    {
            //        activity = _Squard_0_Solo_Activity;
            //        tr_squard = _tr_squard_0;
            //    }
            //    if (1 == i)
            //    {
            //        activity = _Squard_1_Solo_Activity;
            //        tr_squard = _tr_squard_1;
            //    }
            //    if (2 == i)
            //    {
            //        activity = _Squard_2_Solo_Activity;
            //        tr_squard = _tr_squard_2;
            //    }
            //    if (3 == i)
            //    {
            //        activity = _Squard_3_Solo_Activity;
            //        tr_squard = _tr_squard_3;
            //    }
            //    //------------------------------

            //    if (true == activity)
            //    {
            //        squad._solo_activity = activity;
            //        squad._targetPos = tr_squard.position;
            //    }
            //    else
            //    {
            //        tr_squard.position = squad._targetPos;
            //    }
            //}


            //==============================================
            //KeyInput_Platoon(0);
            //==============================================

            foreach (Unit v in EntityMgr.list)
            {

                //if(0 == v._id)
                //{
                //    v._mass = 10;
                //    //v._forces = (_tr_line_a.position - _tr_test.position) * 5; 
                //}
                //v._steeringBehavior._targetPos = _tr_platoon_0.position;
                v._steeringBehavior._targetPos = v._disposition._belong_squad._pos; //Arrive2 에서 사용 , OffsetPursuit 에서는 사용안함

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

        public void ResolveContacts_test(float timeInterval)
        {
            Unit v = EntityMgr.list[0];
            Vector3 calcPos;
            CellSpace findCell;
            for(int i=0;i<3;i++)
            {

                bool isColl = GridManager.Inst.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius_geo, out calcPos, out findCell);
                if (isColl)
                {
                    DebugWide.AddDrawQ_Circle(v._pos, v._radius_geo, Color.green);
                    v._oldPos = calcPos;
                    v.SetPos(calcPos);


                    DebugWide.LogBlue("  " + i);

                    Contact contact = new Contact();
                    contact.pm_0 = v;
                    contact.pm_1 = null;
                    contact.plane_1 = new CollisionPlane();
                    contact.restitution = (v._elasticity + 1) * 0.5f; //평균값
                    contact.plane_1.mass = 10;
                    //contact.plane_1.direction = findCell._nDir;
                    contact.contactNormal = findCell._nDir;

                    //CollisionPlane plane = new CollisionPlane();
                    //plane.mass = 10;
                    //plane.direction = findCell._nDir;
                    //plane.offset = -1f * Vector3.Dot(findCell._nDir, findCell._line_center); //원점과 평면사이의 최소거리 , fixme: 미리계산 방식으로 변경하기 
                    //Contact contact;
                    //if (true == CollisionDetector.SphereAndHalfSpace(v, plane, out contact))
                    {
                        //contact.ResolveVelocity_SphereAndHalfSpace(timeInterval);
                    }
                }
            }


        }

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

                //사각형 겹침 목록
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
                            contact.Resolve_SphereAndSphere(timeInterval);
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

                    //if (0 != v._id) continue;

                    if (_StrNonpenetration)
                    {
                        Vector3 calcPos;
                        CellSpace findCell;
                        bool isColl = GridManager.Inst.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius_geo, out calcPos, out findCell);
                        if(isColl)
                        {
                            v._oldPos = calcPos;
                            v.SetPos(calcPos);
                            collCount++;


                            {
                                //충돌후 속도계산 
                                Contact contact = new Contact();
                                contact.pm_0 = v;
                                contact.pm_1 = null;
                                contact.plane_1 = new CollisionPlane();
                                //contact.restitution = (v._elasticity + 1) * 0.5f; //평균값
                                contact.restitution = 0.5f; //fixme : 구조타일에 설정 할 수 있게 하기 
                                contact.plane_1.mass = 1f;
                                contact.plane_1.damping = 0.1f;
                                //contact.plane_1.direction = findCell._nDir;
                                contact.contactNormal = findCell._nDir;

                                //CollisionPlane plane = new CollisionPlane();
                                //plane.mass = 10;
                                //plane.direction = findCell._nDir;
                                //plane.offset = -1f * Vector3.Dot(findCell._nDir, findCell._line_center); //원점과 평면사이의 최소거리 , fixme: 미리계산 방식으로 변경하기 
                                //Contact contact;
                                //if (true == CollisionDetector.SphereAndHalfSpace(v, plane, out contact))
                                {
                                    contact.ResolveVelocity_SphereAndPlane(timeInterval);
                                }


                            }

                        }
                    }

                    //==========================================
                }

                //DebugWide.LogBlue(" spCC : " + _sweepPrune._calcCount);

                iterationsUsed++;

                //DebugWide.LogGreen(" loop ------ collCount : " + collCount + " iterationsUsed : " + iterationsUsed);
                if (0 == collCount) break; //충돌횟수가 0이라면 더이상 계산 할 것이 없음 
            }
            //DebugWide.LogBlue(calcCount  + "  " );

        }

        public void KeyInput_Platoon(int platID)
        {
            const float MOVE_LENGTH = 1f;
            Vector3 n = _Platoons[platID]._targetPos - _Platoons[platID]._pos;
            if (Misc.IsZero(n))
                n = Vector3.forward;

            if (Input.GetKey(KeyCode.W))
            {

                n = VOp.Normalize(n);
                _Platoons[platID]._targetPos += n * MOVE_LENGTH;
                _Platoons[platID]._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoons[platID]._targetPos;
            }
            if (Input.GetKey(KeyCode.S))
            {

                n = -VOp.Normalize(n);
                _Platoons[platID]._targetPos += n * MOVE_LENGTH;
                _Platoons[platID]._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoons[platID]._targetPos;

                //------
                //_Squard_0._targetPos += n * MOVE_LENGTH;
                //_Squard_0._pos += n * MOVE_LENGTH;
                //_tr_squard_0.position = _Squard_0._targetPos;

                //_Squard_3._targetPos += n * MOVE_LENGTH;
                //_Squard_3._pos += n * MOVE_LENGTH;
                //_tr_squard_3.position = _Squard_3._targetPos;
                //------
            }
            if (Input.GetKey(KeyCode.A))
            {

                n = -VOp.PerpN(n, Vector3.up);
                _Platoons[platID]._targetPos += n * MOVE_LENGTH;
                _Platoons[platID]._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoons[platID]._targetPos;

                //------
                //_Squard_3._targetPos += n * MOVE_LENGTH;
                //_Squard_3._pos += n * MOVE_LENGTH;
                //_tr_squard_3.position = _Squard_3._targetPos;
                //------
            }
            if (Input.GetKey(KeyCode.D))
            {

                n = VOp.PerpN(n, Vector3.up);
                _Platoons[platID]._targetPos += n * MOVE_LENGTH;
                _Platoons[platID]._pos += n * MOVE_LENGTH;

                _tr_platoon_0.position = _Platoons[platID]._targetPos;
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


            //_Platoons[0].Draw(Color.green); //소대 출력
            _Squads[0].Draw(Color.green);

            Color color = Color.black;
            foreach (Unit v in EntityMgr.list)
            {
                color = Color.black;

                if (null != v._disposition._belong_squad && v == v._disposition._belong_squad._units[0])
                    color = Color.yellow;

                if(3 == v._disposition._squad_id || 2 == v._disposition._squad_id)
                    color = Color.white;

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

        public static bool SphereAndHalfSpace(BaseEntity sphere, CollisionPlane plane, out Contact contact)
        {
            contact = new Contact();

            //충돌비활성일때 처리하지 않는다 
            if (false == sphere._isCollision)
                return false;

            // Cache the sphere position
            Vector3 position = sphere._pos;

            // Find the distance from the plane
            float ballDistance =
                Vector3.Dot(plane.body._normal, position) - sphere._radius_geo + plane.body._offset; //offset이 기본 음수로 설정되어 있으므로 더한다 

            if (ballDistance >= 0) return false;

            contact.pm_0 = sphere;
            contact.pm_1 = null;
            contact.plane_1 = plane;
            contact.restitution = (sphere._elasticity + plane.elasticity) * 0.5f; //평균값

            contact.contactNormal = plane.body._normal;
            contact.penetration = -ballDistance;
            contact.contactPoint = position - plane.body._normal * (ballDistance + sphere._radius_geo);

            return true;
        }
    }

    public struct CollisionPlane
    {
        //public BaseEntity body;
        public UtilGS9.Plane body;

        public float mass;
        public float elasticity;
        public float damping; //마찰력 

    }

    public struct Contact
    {
        public BaseEntity pm_0; //Sphere vs Sphere
        public BaseEntity pm_1;
        public CollisionPlane plane_1; //Sphere vs Plane

        public Vector3 contactPoint;

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

        public void Resolve_SphereAndSphere(float duration)
        {
            ResolveVelocity(duration);
            ResolveInterpenetration(duration);
        }


        public void ResolveVelocity_SphereAndPlane(float duration)
        {
            //----------------------------------------------
            //ResolveVelocity
            //----------------------------------------------

            //if (0 == pm_0._id)
            //{
            //    DebugWide.LogBlue(" ---------- " + pm_0._velocity);
            //}

            float dotVelocity0 = Vector3.Dot(pm_0._velocity, contactNormal);
            float dotVelocity1 = -dotVelocity0; //반작용 속도 설정 


            // Find the average coefficent of restitution.
            float averageE = restitution;


            float finalVelocity0 =
            (((pm_0._mass -
               (averageE * plane_1.mass)) * dotVelocity0) +
             ((1 + averageE) * plane_1.mass * dotVelocity1)) /
            (pm_0._mass + plane_1.mass);


            pm_0._velocity = ((finalVelocity0 - dotVelocity0) * contactNormal + pm_0._velocity);

            //----------------------------------------------
            //ResolveInterpenetration
            //----------------------------------------------

            //float len_bt_src = penetration * 1;
            //pm_0.SetPos(pm_0._pos + contactNormal * len_bt_src);

            pm_0._velocity *= (float)Math.Pow(plane_1.damping, duration); //초당 damping 비율 만큼 감소시킨다.

            //if (0 == pm_0._id)
            //{
            //    DebugWide.LogBlue(contactNormal + "  " + pm_0._velocity); 
            //}
            //DebugWide.AddDrawQ_Line(pm_0._pos, pm_0._pos + contactNormal, Color.blue); //test
            //DebugWide.AddDrawQ_Line(pm_0._pos, pm_0._pos + pm_0._velocity, Color.red); //test

        }

        private void ResolveVelocity(float timeInterval)
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

        private void ResolveInterpenetration(float timeInterval)
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



