using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Proto_AI_4
{
    public class ObjectManager
    {
        public static readonly ObjectManager Inst = new ObjectManager();
        //public SphereTree _sphereTree = new SphereTree(500, new float[] { 16, 10 }, 0.5f);
        public SphereTree _sphereTree = new SphereTree(500, new float[] { 32, 16, 8, 4 }, 0.5f);
        public SphereTree _sphereTree_struct = new SphereTree(2000, new float[] { 32, 16, 8, 2 }, 0.5f);

        //--------------------------------------------------
        //메모리풀
        public MemoryPool<Squad> _squadPool = new MemoryPool<Squad>();
            
        //--------------------------------------------------

        private ObjectManager()
        { }

        public void Init()
        {

            _squadPool.Init(200, 50, "SQUAD_POOL");

            //----------------------------------------------

            foreach (KeyValuePair<Vector3Int, CellSpace> t in GridManager.Inst._structTileList)
            {
                //if (true == t.Value._isUpTile)
                {
                    SphereModel model = _sphereTree_struct.AddSphere(t.Value._pos3d_center, 0.6f, SphereTree.CREATE_LEVEL_LAST);
                    _sphereTree_struct.AddIntegrateQ(model);
                }
            }

            _sphereTree_struct.Process();
        }

        public void AddSphereTree(BaseEntity entity)
        {
            SphereModel model = _sphereTree.AddSphere(entity._pos, entity._radius_body, SphereTree.CREATE_LEVEL_LAST);
            _sphereTree.AddIntegrateQ(model);
            model.SetLink_UserData<BaseEntity>(entity);

            entity._sphereModel = model;
        }

        public void Update(float deltaTime)
        {
            _sphereTree.Process();

        }

        public void Draw(bool none_recursive, bool level_0, bool level_1, bool level_2, bool level_3 , Vector3 pos1, Vector3 pos2)
        {
            if (level_3)
            {
                _sphereTree.Render_Debug(3, true);
            }
            if (level_2)
            {
                _sphereTree.Render_Debug(2, true);
            }

            if (level_1)
            {
                _sphereTree.Render_Debug(1, true);
            }
            if (level_0)
            {
                _sphereTree.Render_Debug(0, true);
            }
            if (none_recursive)
            {
                _sphereTree.Debug_NoneRecursive(pos1, pos2); //chamto test
            }
        }

        public void Draw_Struct(bool level_0, bool level_1, bool level_2, bool level_3)
        {
            if (level_3)
            {
                _sphereTree_struct.Render_Debug(3, true);
            }
            if (level_2)
            {
                _sphereTree_struct.Render_Debug(2, true);
            }

            if (level_1)
            {
                _sphereTree_struct.Render_Debug(1, true);
            }

            if (level_0)
            {
                _sphereTree_struct.Render_Debug(0, true);
            }
        }

        //public struct Param_RangeTest<ENTITY> where ENTITY : class, new()
        public struct Param_RangeTest
        {
            //==============================================
            public SphereModel find; //결과값 

            //public ENTITY unit;
            public BaseEntity unit;
            //public Camp.eRelation vsRelation;
            //public Camp.eKind unit_campKind;

            public Vector3 src_pos;
            public float minRadius;
            public float maxRadius;
            //public float maxRadiusSqr;

            public delegate bool Proto_ConditionCheck(ref Param_RangeTest param, SphereModel dstModel);
            public Proto_ConditionCheck callback;
            //==============================================


            public Param_RangeTest(BaseEntity in_srcUnit, Vector3 pos, float meter_minRadius, float meter_maxRadius)
            {
                find = null;

                unit = in_srcUnit;
                //vsRelation = in_vsRelation;
                //unit_campKind = in_srcUnit._campKind;
                src_pos = pos;
                minRadius = meter_minRadius;
                maxRadius = meter_maxRadius;
                //maxRadiusSqr = maxRadius * maxRadius;

                callback = Param_RangeTest.Func_ConditionCheck;
            }

            //==============================================

            static public bool Func_ConditionCheck(ref Param_RangeTest param, SphereModel dstModel)
            {
                //return true;

                //기준객체는 검사대상에서 제외한다
                if (null != param.unit && param.unit._sphereModel == dstModel) return false;

                BaseEntity dstBeing = dstModel.GetLink_UserData() as BaseEntity;
                //BaseEntity dstUnit = dstModel.GetLink_UserData() as BaseEntity;

                if (null != dstBeing)
                {
                    //가시거리 검사 
                    return true;
                    //return GridManager.Inst.IsVisibleTile(param.src_pos, dstModel.GetPos(), 10);
                }

                return false;
            }
        }

        public BaseEntity RangeTest(BaseEntity src, Vector3 pos, float meter_minRadius, float meter_maxRadius)
        {
            Param_RangeTest param = new Param_RangeTest(src, pos, meter_minRadius, meter_maxRadius);
            _sphereTree.RangeTest_MinDisReturn(ref param);


            if (null != param.find)
            {

                return param.find.GetLink_UserData() as BaseEntity;
            }

            return null;
        }
    }



}//end namespace



