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
        public bool _isText = false;
        public TestMode _testMode = TestMode.None;
        public int _count = 100;
        public int _radius_root = 25;
        public int _radius_leaf = 4;
        public int _radius_gravy = 1;

        public int _MaxRecompute = 0;
        public int _MaxIntegrate = 0;

        private PT_SphereModel _control = null;

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
            _sphereTree.Process(out _MaxRecompute, out _MaxIntegrate);

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
            Color cc = Color.black;
            if (TestMode.SphereInterstion == _testMode)
            {
                Vector3 pointIts = Vector3.zero;
                Vector3 dir = _lineEnd.position - _lineStart.position;
                dir.Normalize();
                DefineI.DrawCircle(_sphere.position, 10, Color.red);
                //DefineI.IntersectLineSegment(_sphere.position, 10, _lineStart.position, dir, out pointIts);
                //DefineI.DrawCircle(pointIts, 1, Color.white);



                //if (true == DefineI.IntersectRay(_sphere.position, 10, _lineStart.position, dir))
                if(true == DefineI.IntersectLineSegment(_sphere.position, 10, _lineStart.position, _lineEnd.position))
                {
                    cc = Color.red;
                }

            }


            DefineI.DrawLine(_lineStart.position, _lineEnd.position, cc);


            //==================================================
        }
    }

    //=======================================================

    public class DefineI
    {
        static public void DrawLine(Vector3 start, Vector3 end, Color cc)
        {
#if UNITY_EDITOR
            Gizmos.color = cc;
            Gizmos.DrawLine(start, end);
#endif
        }

        static public void DrawCircle(Vector3 pos, float radius, Color cc)
        {
#if UNITY_EDITOR
            Gizmos.color = cc;
            Gizmos.DrawWireSphere(pos, radius);
#endif
        }

        static public void PrintText(Vector3 pos, Color cc, string text)
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = cc;

            UnityEditor.Handles.BeginGUI();
            UnityEditor.Handles.Label(pos, text, style);
            UnityEditor.Handles.EndGUI();
#endif
        }

        //ray_dir : 정규화된 값을 넣어야 한다 
        //intersection_firstPoint : 반직선이 원과 충돌한 첫번째 위치를 반환
        static public bool IntersectRay(Vector3 sphere_center, float sphere_radius, Vector3 ray_origin, Vector3 ray_dir, out Vector3 intersection_firstPoint)
        {

            Vector3 w = sphere_center - ray_origin;
            Vector3 v = ray_dir; //rayDirection 이 정규화 되어 있어야 올바르게 계산할 수 있다 
            float rsq = sphere_radius * sphere_radius;
            float wsq = Vector3.Dot(w, w); //w.x * w.x + w.y * w.y + w.z * w.z;

            // Bug Fix For Gem, if origin is *inside* the sphere, invert the
            // direction vector so that we get a valid intersection location.
            if (wsq < rsq) v *= -1; //반직선의 시작점이 원안에 있는 경우 - 방법1 

            float proj = Vector3.Dot(w, v);
            float ssq = (wsq - proj * proj);
            float dsq = rsq - ssq;

            intersection_firstPoint = Vector3.zero;
            if (dsq > 0.0f)
            {
                float d = Mathf.Sqrt(dsq);

                //반직선의 시작점이 원안에 있는 경우 - 방법2
                //float length = proj - d; //선분 시작점이 원 밖에 있는 경우
                //if(wsq < rsq) length = proj + d; //선분 시작점이 원 안에 있는 경우
                //intersect_firstPoint = rayOrigin + v * length;

                intersection_firstPoint = ray_origin + v * (proj - d);

                return true;
            }
            return false;
        }

        //segment_dir : 정규화된 값을 넣어야 한다 
        //intersection_firstPoint : 반직선이 원과 충돌한 첫번째 위치를 반환
        static public bool IntersectLineSegment(Vector3 sphere_center, float sphere_radius, Vector3 segment_origin, Vector3 segment_dir, float segment_distance, out Vector3 intersection_firstPoint)
        {
            Vector3 sect;
            bool hit = IntersectRay(sphere_center, sphere_radius, segment_origin, segment_dir, out sect);

            intersection_firstPoint = Vector3.zero;
            if (hit)
            {
                float d = (segment_origin - sect).sqrMagnitude;
                if (d > (segment_distance * segment_distance)) return false;
                intersection_firstPoint = sect;
                return true;
            }
            return false;
        }

        //ray_dir : 비정규화된 값을 넣어도 된다  
        static public bool IntersectRay(Vector3 sphere_center, float sphere_radius, Vector3 ray_origin , Vector3 ray_dir )
        {
            // compute intermediate values
            Vector3 w = sphere_center - ray_origin;
            float wsq = Vector3.Dot(w, w); //w.sqrMagnitude
            float proj = Vector3.Dot(w, ray_dir);
            float rsq = sphere_radius * sphere_radius;

            // if sphere behind ray, no intersection
            if (proj< 0.0f && wsq > rsq )
                return false;
            float vsq = Vector3.Dot(ray_dir, ray_dir);

            // test length of difference vs. radius
            return (vsq* wsq - proj* proj <= vsq* rsq );
        }



        //ref : http://nic-gamedev.blogspot.com/2011/11/using-vector-mathematics-and-bit-of_09.html
        static public bool IntersectLineSegment(Vector3 sphere_center, float sphere_radius, Vector3 segment_origin, Vector3 segment_last)
        {
            
            Vector3 v = segment_last - segment_origin;
            float vsq = v.sqrMagnitude;

            Vector3 w = sphere_center - segment_origin;
            float proj = Vector3.Dot(w, v);

            //oi = ipt - segment_origin
            //cos||w||||v|| / vsq = cos||w|| / ||v|| => ||oi|| / ||v||
            //전체선분 길이에 대하여, 선분 시작점에서 "반지름과 선분이 직각으로 만나는 점" 까지의 길이의 비율을 구한다 
            float percAlongLine = proj / vsq; //0~1 사이의 비율값으로 변환한다
         
            if (percAlongLine< 0.0f )
            {
               percAlongLine = 0.0f;
            }
            else if (percAlongLine > 1.0f )
            {
               percAlongLine = 1.0f;
            }

            Vector3 ipt = (segment_origin + (percAlongLine * v)); //선분에 비율값을 곱한다 

            Vector3 s = ipt - sphere_center;
            float ssq = s.sqrMagnitude;
            float rsq = sphere_radius * sphere_radius;
            return (ssq <= rsq);
        }

    }
}

