using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_004
{
    public class Test_004_SphereTree : MonoBehaviour
    {
        public enum TreeLevel
        {
            Level_1 = 0,
            Level_2,
            Level_3,
            Level_4,
            All,
        }

        public enum TestMode
        {
            None,
            RayTrace,
            RangeTest,
            FrustumTest,
        }

        //private PT_SphereTree _sphereTree = null;
        SphereTree _sphereTree = null;

        public Transform _lineStart = null;
        public Transform _lineEnd = null;

        public TreeLevel _tree_level = TreeLevel.All;
        public bool _isText = false;
        public TestMode _testMode = TestMode.None;
        public int _count = 100;
        public float _radius_level_1 = 16;
        public float _radius_level_2 = 10;
        public float _radius_level_3 = 5;
        public float _radius_level_4 = 2;
        public float _radius_gravy = 0.5f;

        [Space]
        [Space]
        public int _out_MaxRecompute = 0;
        public int _out_MaxIntegrate = 0;
        public float _A_radius = 3;
        public float _A_speed = 10f;
        public Vector3 _A_pos = Vector3.zero;

        private SphereModel _A_model = null;

        // Use this for initialization
        void Start()
        {
            _sphereTree = new SphereTree(_count, new float[] { _radius_level_1, _radius_level_2, _radius_level_3, _radius_level_4 }, _radius_gravy);

            for (int i = 0; i < _count; i++)
            {
                Vector3 pos = new Vector3(Misc.rand.Next() % 100, Misc.rand.Next() % 60, 0);
                float radius = (Misc.rand.Next() % 4) + 1;
                SphereModel model = _sphereTree.AddSphere(pos, radius, SphereModel.Flag.TREE_LEVEL_LAST);
                _sphereTree.AddIntegrateQ(model);

                _A_model = model;
            }


        }


        void Update()
        {
            if (null == _sphereTree) return;

            _sphereTree._maxRadius_supersphere[0] = _radius_level_1;
            _sphereTree._maxRadius_supersphere[1] = _radius_level_2;
            _sphereTree._maxRadius_supersphere[2] = _radius_level_3;
            _sphereTree._maxRadius_supersphere[3] = _radius_level_4;
            _sphereTree._gravy_supersphere = _radius_gravy;

            _A_pos = _A_model.GetPos();
            switch (Input.inputString)
            {
                case "a": //left
                    _A_pos.x = _A_pos.x + Time.deltaTime * -1f * _A_speed;
                    //DebugWide.LogBlue("a");
                    break;
                case "s": //down
                    _A_pos.y = _A_pos.y + Time.deltaTime * -1f * _A_speed;
                    break;
                case "d": //right
                    _A_pos.x = _A_pos.x + Time.deltaTime * 1f * _A_speed;
                    break;
                case "w": //up
                    _A_pos.y = _A_pos.y + Time.deltaTime * 1f * _A_speed;
                    break;

            }
            _A_model.SetPosRadius(_A_pos, _A_radius);

            _sphereTree.ResetFlag();
            //_sphereTree.Process(out _out_MaxRecompute, out _out_MaxIntegrate);
            _sphereTree.Process();

        }


        private Frustum __f = new Frustum();
        private void OnDrawGizmos()
        {
            if (null == _sphereTree) return;

            if(TreeLevel.All == _tree_level)
            {
                for (int i = 0; i < _sphereTree._max_level;i++)
                {
                    _sphereTree.Render_Debug(i, _isText);
                }
            }else
            {
                _sphereTree.Render_Debug((int)_tree_level, _isText);
            }


            //==================================================
            //광선추적 테스트
            if (TestMode.RayTrace == _testMode)
            {
                _sphereTree.Render_RayTrace(_lineStart.position, _lineEnd.position);
                DebugWide.DrawLine(_lineStart.position, _lineEnd.position, Color.black);
            }

            //==================================================
            //거리 테스트
            if (TestMode.RangeTest == _testMode)
            {
                float rangeRadius = (_lineEnd.position - _lineStart.position).magnitude;
                _sphereTree.Render_RangeTest(_lineStart.position, rangeRadius);
                DebugWide.DrawCircle(_lineStart.position, rangeRadius, Color.yellow);
            }

            //==================================================
            //플러스텀 테스트
            if (TestMode.FrustumTest == _testMode)
            {
                Vector3Int leftDown = new Vector3Int((int)_lineStart.position.x, (int)_lineStart.position.y, (int)_lineStart.position.z);
                Vector3Int rightUp = new Vector3Int((int)_lineEnd.position.x, (int)_lineEnd.position.y, (int)_lineEnd.position.z);
                Vector3Int leftUp = new Vector3Int(leftDown.x, rightUp.y, 0);
                Vector3Int rightDown = new Vector3Int(rightUp.x, leftDown.y, 0);
                __f.Set(leftDown.x, leftDown.y, rightUp.x, rightUp.y);

                DebugWide.DrawLine(leftDown, leftUp, Color.yellow);
                DebugWide.DrawLine(rightDown, rightUp, Color.yellow);
                DebugWide.DrawLine(leftDown, rightDown, Color.yellow);
                DebugWide.DrawLine(rightUp, leftUp, Color.yellow);
                _sphereTree.Render_FrustumTest(__f, Frustum.ViewState.PARTIAL);
            }

            //==================================================
        }
    }

    //=======================================================

}

