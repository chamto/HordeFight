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

        public float _formation_speed = 10;
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

        public bool _isObjNonpenetration = true;
        public bool _isStrNonpenetration = true;

        public float _minRange = 0;
        public float _maxRange = 0.5f;

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
            for (int i = 0; i < 4; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
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
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_num = 0;
                unit._disposition._squard_num = 2;
                unit._disposition._squard_pos = i;
                //unit._steeringBehavior.ArriveOn();
            }
            for (int i = 0; i < 4; i++)
            {
                unit = new Unit();
                int id = EntityMgr.Add(unit);
                unit.Init(id, 0.5f, new Vector3(17, 0, 12));
                unit._disposition._platoon_num = 0;
                unit._disposition._squard_num = 3;
                unit._disposition._squard_pos = i;
                //unit._steeringBehavior.ArriveOn();
            }

            _Platoon_0 = Platoon.Create_Platoon(EntityMgr.list);
            _Platoon_0._targetPos = _tr_platoon_0.position;
            _Platoon_0._pos = _tr_platoon_0.position;
            _Platoon_0.ApplyFormationOffset_0();

            _Squard_0 = _Platoon_0._squards[0];
            _Squard_1 = _Platoon_0._squards[1];
            _Squard_2 = _Platoon_0._squards[2];
            _Squard_3 = _Platoon_0._squards[3];

            for(int i=0;i< EntityMgr.list.Count;i++)
            {
                Unit u = EntityMgr.list[i];
                u._steeringBehavior.OffsetPursuitOn(u._squard, u._formation._offset);
            }

            //==============================


        }


        public void Update(float deltaTime)
        {

            _Platoon_0._speed = _formation_speed;
            _Platoon_0._targetPos = _tr_platoon_0.position;
            _Platoon_0.Update(deltaTime);

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
                v._Friction = _Friction;
                v._anglePerSecond = _anglePerSecond;

                v._steeringBehavior._weightArrive = _weightArrive;
                v._steeringBehavior._weightOffsetPursuit = _weightOffsetPursuit;

                v._isNonpenetration = _isObjNonpenetration;
                v.Update(deltaTime);


            }


            //==============================================

            foreach (Unit v in EntityMgr.list)
            {
                //==========================================
                //동굴벽과 캐릭터 충돌처리 

                //객체의 반지름이 <0.1 ~ 0.99> 범위에 있어야 한다.
                //float maxR = Mathf.Clamp(v._radius, 0.1, 0.99); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 

                if (_isStrNonpenetration)
                    v.SetPos(GridManager.Inst.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius_geo));


                //==========================================
            }

            ObjectManager.Inst.Update(deltaTime);
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

        public void CollisionPush(Unit src, Unit dst)
        {
            if (null == src || null == dst) return;


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

            }
        }

        public void OnDrawGizmos()
        {
            if (false == _init) return;

            DebugWide.DrawLine(_tr_test.position, _tr_line_a.position, Color.white);
            DebugWide.DrawLine(_tr_test.position, _tr_line_b.position, Color.white);


            _Platoon_0.Draw(Color.white);

            Color color = Color.black;
            foreach (Unit v in EntityMgr.list)
            {
                color = Color.black;
                if (0 == v._id)
                    color = Color.red;
                if (5 == v._id)
                    color = Color.yellow;

                v.Draw(color);
            }


        }
    }



}//end namespace



