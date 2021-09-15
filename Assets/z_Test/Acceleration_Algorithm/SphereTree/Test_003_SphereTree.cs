using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace ST_Test_003
{
    public class Test_003_SphereTree : MonoBehaviour
    {
        public enum TreeMode
        {
            None,
            Root,
            Leaf,
            All,
        }

        public enum TestMode
        {
            None,
            RayTrace,
            RangeTest,
            FrustumTest,
            //SphereInterstion, //원과 반직선 교차검사 
        }

        private PT_SphereTree _sphereTree = null;

        //public Transform _sphere = null;
        public Transform _lineStart = null;
        public Transform _lineEnd = null;

        public TreeMode _treeMode = TreeMode.All;
        public bool _isText = false;
        public TestMode _testMode = TestMode.None;
        public int _count = 100;
        public int _radius_root = 25;
        public int _radius_leaf = 4;
        public int _radius_gravy = 1;

        [Space]
        [Space]
        public int _out_MaxRecompute = 0;
        public int _out_MaxIntegrate = 0;
        public float _A_radius = 3;
        public float _A_speed = 10f;
        public Vector3 _A_pos = Vector3.zero;

        private PT_SphereModel _A_model = null;

        // Use this for initialization
        void Start()
        {

            _sphereTree = new PT_SphereTree(_count, _radius_root, _radius_leaf, _radius_gravy);

            for (int i = 0; i < _count; i++)
            {
                Vector3 pos = new Vector3(Misc.rand.Next() % 100, Misc.rand.Next() % 60, 0);
                float radius = (Misc.rand.Next() % 4) + 1;
                PT_SphereModel model = _sphereTree.AddSphere(pos, radius, PT_SphereModel.Flag.TREE_LEVEL_2);
                _sphereTree.AddIntegrateQ(model);

                _A_model = model;
            }


        }


        void Update()
        {
            if (null == _sphereTree) return;

            _sphereTree._maxRadius_supersphere_root = _radius_root;
            _sphereTree._maxRadius_supersphere_leaf = _radius_leaf;
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
            _sphereTree.Process(out _out_MaxRecompute, out _out_MaxIntegrate);

        }


        private Frustum __f = new Frustum();
        private void OnDrawGizmos()
        {
            if (null == _sphereTree) return;

            _sphereTree.Render_Debug((int)_treeMode, _isText);

            //==================================================
            //광선추적 테스트
            if (TestMode.RayTrace == _testMode)
            {
                _sphereTree.Render_RayTrace(_lineStart.position, _lineEnd.position);
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
            //원과 반직선 교차 테스트
            Color cc = Color.black;
            //if (TestMode.SphereInterstion == _testMode)
            //{
            //    Vector3 pointIts = Vector3.zero;
            //    Vector3 dir = _lineEnd.position - _lineStart.position;
            //    dir.Normalize();
            //    DebugWide.DrawCircle(_sphere.position, 10, Color.red);
            //    //DefineI.IntersectLineSegment(_sphere.position, 10, _lineStart.position, dir, out pointIts);
            //    //DefineI.DrawCircle(pointIts, 1, Color.white);


            //    //if (true == DefineI.IntersectRay(_sphere.position, 10, _lineStart.position, dir))
            //    if(true == Geo.IntersectLineSegment(_sphere.position, 10, _lineStart.position, _lineEnd.position))
            //    {
            //        cc = Color.red;
            //    }
            //}

            DebugWide.DrawLine(_lineStart.position, _lineEnd.position, cc);


            //==================================================
        }
    }

    //=======================================================

}

