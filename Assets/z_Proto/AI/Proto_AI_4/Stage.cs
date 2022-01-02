﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Proto_AI_4
{
    [System.Serializable]
    public class Stage
    {

        public Transform _tr_target = null;

        public Transform _tr_test = null;
        public Transform _tr_line_a = null;
        public Transform _tr_line_b = null;

        public Transform _tr_test2_s = null;
        public Transform _tr_test2_e = null;

        public float _formation_speed = 10;
        public float _radius = 0.5f;
        public float _radius_damage = 0.5f;
        public float _mass = 1f;
        public float _weight = 20;
        public float _maxForce = 40f;
        public float _maxSpeed = 10f;
        public float _withstand = 1f;
        public float _Friction = 0.85f; //마찰력 
        public float _anglePerSecond = 180;

        public bool _isObjNonpenetration = true;
        public bool _isStrNonpenetration = true;

        public float _minRange = 0;
        public float _maxRange = 0.5f;

        public OrderPoint _formationPoint = new OrderPoint();

        public bool _init = false;

        public void Init()
        {
            _init = true;

            _tr_target = GameObject.Find("tr_target").transform;

            _tr_test = GameObject.Find("Test").transform;
            _tr_line_a = GameObject.Find("line_a").transform;
            _tr_line_b = GameObject.Find("line_b").transform;
            _tr_test2_s = GameObject.Find("Test2_s").transform;
            _tr_test2_e = GameObject.Find("Test2_e").transform;


            ObjectManager.Inst.Init();

            _formationPoint._target = _tr_target.position;
            _formationPoint._pos = _tr_target.position;

            //------

            EntityMgr.list.Clear();

            //0
            Character v = new Character();
            int id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._mode = SteeringBehavior.eType.arrive;
            //v._leader = _formationPoint;
            //v._offset = new Vector3(0, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;
            //v._maxSpeed = 14;

            //1
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(1f, 0, -1f);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            //2
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(-1f, 0, -1f);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            //-------------------

            //3
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(1f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            ////4
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(2f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;

            ////5
            v = new Character();
            id = EntityMgr.Add(v);
            v.Init(id, 0.5f, new Vector3(17, 0, 12));
            //v._leader = _formationPoint;
            //v._offset = new Vector3(3f, 0, 0);
            //v._mode = SteeringBehavior.eType.offset_pursuit;
            //v._mode = SteeringBehavior.eType.arrive;


            for (int i = 0; i < 30; i++)
            {
                v = new Character();
                id = EntityMgr.Add(v);
                v.Init(id, 0.5f, new Vector3(17, 0, 12));
                //v._mode = SteeringBehavior.eType.arrive;
                //v._target = new Vector3(17, 0, 12);
            }

            //==============================


        }


        public void Update(float deltaTime)
        {

            _formationPoint._speed = _formation_speed;
            _formationPoint._target = _tr_target.position;
            _formationPoint.Update(deltaTime);
            KeyInput();

            //==============================================

            foreach (Character v in EntityMgr.list)
            {
                if (5 == v._id)
                {
                    v._withstand = _withstand;
                    //v._target = _tr_test.position;
                    v.SetRadius(_radius);
                    v._radius_damage = _radius_damage;
                }
                if (6 <= v._id)
                {
                    //v._target = _tr_test2_s.position;
                }

                //v._radius = _radius;
                //v.SetRadius(_radius);
                v._mass = _mass;
                v._maxSpeed = _maxSpeed;
                v._maxForce = _maxForce;
                v._Friction = _Friction;
                v._anglePerSecond = _anglePerSecond;
                v._weight = _weight;
                v._isNonpenetration = _isObjNonpenetration;
                v.Update(deltaTime);


            }


            //==============================================

            foreach (Character v in EntityMgr.list)
            {
                //==========================================
                //동굴벽과 캐릭터 충돌처리 

                //객체의 반지름이 <0.1 ~ 0.99> 범위에 있어야 한다.
                //float maxR = Mathf.Clamp(v._radius, 0.1, 0.99); //최대값이 타일한개의 길이를 벗어나지 못하게 한다 

                if (_isStrNonpenetration)
                    v.SetPos(GridManager.Inst.Collision_StructLine_Test3(v._oldPos, v._pos, v._radius));


                //==========================================
            }

            ObjectManager.Inst.Update(deltaTime);
        }

        public void KeyInput()
        {
            const float MOVE_LENGTH = 1f;
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = VOp.Normalize(n);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }
            if (Input.GetKey(KeyCode.S))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = -VOp.Normalize(n);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = -VOp.PerpN(n, Vector3.up);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }
            if (Input.GetKey(KeyCode.D))
            {
                Vector3 n = _formationPoint._target - _formationPoint._pos;
                n = VOp.PerpN(n, Vector3.up);
                _formationPoint._target += n * MOVE_LENGTH;
                _formationPoint._pos += n * MOVE_LENGTH;

                _tr_target.position = _formationPoint._target;
            }

        }

        public void CollisionPush(Character src, Character dst)
        {
            if (null == src || null == dst) return;


            Vector3 dir_dstTOsrc = src._pos - dst._pos;
            Vector3 n = ConstV.v3_zero;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;

            float r_sum = (src._radius + dst._radius);
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


            _formationPoint.Draw(Color.white);

            Color color = Color.black;
            foreach (Character v in EntityMgr.list)
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



