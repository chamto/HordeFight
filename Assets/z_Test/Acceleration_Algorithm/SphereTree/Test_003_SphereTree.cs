using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Test_003
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
            SphereInterstion, //원과 반직선 교차검사 
        }

        private PT_SphereTree _sphereTree = null;

        public Transform _sphere = null;
        public Transform _lineStart = null;
        public Transform _lineEnd = null;

        public TreeMode _treeMode = TreeMode.All;
        public TestMode _testMode = TestMode.None;
        public int _count = 100;
        public int _radius_root = 25;
        public int _radius_leaf = 4;
        public int _radius_gravy = 1;

        private SphereModel _control = null;

        // Use this for initialization
        void Start()
        {

            _sphereTree = new PT_SphereTree(_count, _radius_root, _radius_leaf, _radius_gravy);

            for (int i = 0; i < _count; i++)
            {
                Vector3 pos = new Vector3(Misc.rand.Next() % 100, Misc.rand.Next() % 60, 0);
                float radius = (Misc.rand.Next() % 4) + 1;
                SphereModel model = _sphereTree.AddSphere(pos, radius, SphereModel.Flag.TREE_LEVEL_2);
                _sphereTree.AddIntegrateQ(model);

                _control = model;
            }


        }

        public float __radius = 3;
        public float __speed = 10f;
        public Vector3 __p = Vector3.zero;
        void Update()
        {
            if (null == _sphereTree) return;

            __p = _control.GetPos();

            switch (Input.inputString)
            {
                case "a": //left
                    __p.x = __p.x + Time.deltaTime * -1f * __speed;
                    break;
                case "s": //down
                    __p.y = __p.y + Time.deltaTime * -1f * __speed;
                    break;
                case "d": //right
                    __p.x = __p.x + Time.deltaTime * 1f * __speed;
                    break;
                case "w": //up
                    __p.y = __p.y + Time.deltaTime * 1f * __speed;
                    break;

            }
            _control.SetPosRadius(__p, __radius);

            _sphereTree.ResetFlag();
            _sphereTree.Process();

        }


        private Frustum __f = new Frustum();
        private void OnDrawGizmos()
        {
            if (null == _sphereTree) return;

            _sphereTree.Render_Debug((int)_treeMode);

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
                DefineI.DrawCircle(_lineStart.position, rangeRadius, Color.yellow);
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

                DefineI.DrawLine(leftDown, leftUp, Color.yellow);
                DefineI.DrawLine(rightDown, rightUp, Color.yellow);
                DefineI.DrawLine(leftDown, rightDown, Color.yellow);
                DefineI.DrawLine(rightUp, leftUp, Color.yellow);
                _sphereTree.Render_FrustumTest(__f, Frustum.ViewState.PARTIAL);
            }

            //==================================================
            //원과 반직선 교차 테스트
            if (TestMode.SphereInterstion == _testMode)
            {
                Vector3 pointIts = Vector3.zero;
                Vector3 dir = _lineEnd.position - _lineStart.position;
                dir.Normalize();
                DefineI.DrawCircle(_sphere.position, 10, Color.red);
                DefineI.RayIntersection(_sphere.position, 10, _lineStart.position, dir, out pointIts);
                //DefineI.RayIntersectionInFront(_sphere.position, 10, _lineStart.position, dir, out pointIts);

                DefineI.DrawCircle(pointIts, 1, Color.white);
            }

            DefineI.DrawLine(_lineStart.position, _lineEnd.position, Color.red);


            //==================================================
        }
    }

    //=======================================================

    public class DefineI
    {
        static public void DrawLine(Vector3 start, Vector3 end, Color cc)
        {

            Gizmos.color = cc;
            Gizmos.DrawLine(start, end);
        }

        static public void DrawCircle(Vector3 pos, float radius, Color cc)
        {
            Gizmos.color = cc;
            Gizmos.DrawWireSphere(pos, radius);
        }

        static public void PrintText(Vector3 pos, Color cc, string text)
        {

            GUIStyle style = new GUIStyle();
            style.normal.textColor = cc;

            UnityEditor.Handles.BeginGUI();
            UnityEditor.Handles.Label(pos, text, style);
            UnityEditor.Handles.EndGUI();
        }

        //intersect : 반직선이 원과 충돌한 첫번째 위치 
        static public bool RayIntersection(Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersect_firstPoint)
        {

            Vector3 w = sphereCenter - rayOrigin;
            Vector3 v = rayDirection; //rayDirection 이 정규화 되어 있어야 올바르게 계산할 수 있다 
            float rsq = sphereRadius * sphereRadius;
            float wsq = Vector3.Dot(w, w); //w.x * w.x + w.y * w.y + w.z * w.z;

            // Bug Fix For Gem, if origin is *inside* the sphere, invert the
            // direction vector so that we get a valid intersection location.
            if (wsq < rsq) v *= -1; //반직선의 시작점이 원안에 있는 경우 - 방법1 

            float proj = Vector3.Dot(w, v);
            float ssq = (wsq - proj * proj);
            float dsq = rsq - ssq;

            intersect_firstPoint = Vector3.zero;
            if (dsq > 0.0f)
            {
                float d = Mathf.Sqrt(dsq);

                //반직선의 시작점이 원안에 있는 경우 - 방법2
                //float length = proj - d; //선분 시작점이 원 밖에 있는 경우
                //if(wsq < rsq) length = proj + d; //선분 시작점이 원 안에 있는 경우
                //intersect_firstPoint = rayOrigin + v * length;

                intersect_firstPoint = rayOrigin + v * (proj - d);

                return true;
            }
            return false;
        }

        //선분과 원이 교차하는지 검사하는 함수 : !반직선이 아닌 선분이기 때문에 Ray => LineSegment 가 맞는 표현임 
        static public bool LineSegmentIntersection(Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDirection, float distance, out Vector3 intersect)
        {
            Vector3 sect;
            bool hit = RayIntersectionInFront(sphereCenter, sphereRadius, rayOrigin, rayDirection, out sect);

            intersect = Vector3.zero;
            if (hit)
            {
                float d = (rayOrigin - sect).sqrMagnitude;
                if (d > (distance * distance)) return false;
                intersect = sect;
                return true;
            }
            return false;
        }

        static public bool RayIntersectionInFront(Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersect)
        {
            Vector3 intersect_firstPoint;
            bool hit = RayIntersection(sphereCenter, sphereRadius, rayOrigin, rayDirection, out intersect_firstPoint);

            intersect = Vector3.zero;
            if (hit)
            {
                Vector3 dir = intersect_firstPoint - rayOrigin;

                float dot = Vector3.Dot(dir, rayDirection);

                if (dot >= 0) // then it's in front!
                {
                    intersect = intersect_firstPoint;
                    return true;
                }
            }
            return false;
        }
    }
}

