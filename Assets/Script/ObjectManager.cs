using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Assertions;
using UnityEngine.Rendering;

using UtilGS9;


//========================================================
//==================      객체 관리기      ==================
//========================================================
namespace HordeFight
{
    public class ObjectManager : MonoBehaviour
    {

        private uint _id_sequence = 0;
        private uint _id_shot_sequence = 0; //발사체 아이디 생성기

        public Dictionary<uint, Being> _beings = new Dictionary<uint, Being>();
        public List<Being> _linearSearch_list = new List<Being>(); //충돌처리시 선형검색 속도를 높이기 위해 _beings 사전과 동일한 객체를 리스트에 넣음 

        //public Dictionary<uint, Shot> _shots = new Dictionary<uint, Shot>(); //발사체는 따로 관리한다
        public List<Shot> _shots = new List<Shot>();

        private AABBCulling _aabbCulling = new AABBCulling();
        private SphereTree _sphereTree_being = new SphereTree(2000, new float[] { 16, 10, 5 , 3 }, 0.5f);
        private SphereTree _sphereTree_struct = new SphereTree(2000, new float[] { 16, 10, 4 }, 1f);


        //private void Start()
        public void Init()
        {
            DateTime _startDateTime;
            string _timeTemp = "";

            //===============
            //해쉬와 문자열 설정
            //===============

            //==============================================
            _startDateTime = DateTime.Now;
            SingleO.hashMap.Add(Animator.StringToHash("idle"), "idle");
            SingleO.hashMap.Add(Animator.StringToHash("move"), "move");
            SingleO.hashMap.Add(Animator.StringToHash("block"), "block");
            SingleO.hashMap.Add(Animator.StringToHash("attack"), "attack");
            SingleO.hashMap.Add(Animator.StringToHash("fallDown"), "fallDown");
            SingleO.hashMap.Add(Animator.StringToHash("idle -> attack"), "idle -> attack");
            SingleO.hashMap.Add(Animator.StringToHash("attack -> idle"), "attack -> idle");
            _timeTemp += "  ObjectManager.hashMap.Add  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================
            _startDateTime = DateTime.Now;
            this.Create_ChampCamp(); //임시로 여기서 호출한다. 추후 스테이지 생성기로 옮겨야 한다 
            _timeTemp += "  ObjectManager.Create_ChampCamp  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================
            _startDateTime = DateTime.Now;
            _aabbCulling.Initialize(_linearSearch_list); //aabb 컬링 초기화 
            DebugWide.LogBlue("Start_ObjectManager !! ");
            _timeTemp += "  ObjectManager.aabbCulling.init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //==============================================
            _startDateTime = DateTime.Now;
            //임시로 여기서 처리
            //구조타일 정보로 구트리정보를 만든다 
            foreach (KeyValuePair<Vector3Int, CellSpace> t in SingleO.gridManager._structTileList)
            {
                //if (true == t.Value._isUpTile)
                {
                    SphereModel model = _sphereTree_struct.AddSphere(t.Value._pos3d_center, 0.6f, SphereModel.Flag.CREATE_LEVEL_LAST);
                    _sphereTree_struct.AddIntegrateQ(model);
                }
            }
            _timeTemp += "  ObjectManager.sphereTree.init  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            DebugWide.LogBlue(_timeTemp);
            //==============================================

        }

        public SphereTree GetSphereTree_Being() { return _sphereTree_being; }
        public SphereTree GetSphereTree_Struct() { return _sphereTree_struct; }

        public Bounds GetBounds_CameraView()
        {
            float dis = 0f;
            float height_half = 0f;
            float width_half = 0f;
            Vector3 landPos = SingleO.mainCamera.transform.position;
            landPos.y = 0;
            Bounds bounds = new Bounds();
            bounds.center = landPos;
            if (true == SingleO.mainCamera.orthographic)
            {
                //직교투영
                height_half = SingleO.mainCamera.orthographicSize;
                width_half = SingleO.mainCamera.aspect * height_half;
                bounds.size = new Vector3(width_half * 2f, 0, height_half * 2f);

            }
            else
            {
                //원근투영
                //땅바닥이 항상 y축 0이라 가정한다
                dis = SingleO.mainCamera.transform.position.y - 0f;
                height_half = dis * Mathf.Tan(SingleO.mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                width_half = SingleO.mainCamera.aspect * height_half;
                bounds.size = new Vector3(width_half * 2f, 0, height_half * 2f);
            }

            return bounds;
        }


        //객체간의 충돌검사 최적화를 위한 충돌가능객체군 미리 조직하기 
        private void Update()
        //private void LateUpdate()
        {

            //UpdateCollision();

            //UpdateCollision_UseDictElementAt(); //obj100 : fps10
            //UpdateCollision_UseDictForeach(); //obj100 : fps60

            //UpdateCollision_UseList(); //obj100 : fps80 , obj200 : fps40 , obj400 : fps15
            //UpdateCollision_UseGrid3x3(); //obj100 : fps65 , obj200 : fps40
            //UpdateCollision_UseDirectGrid3x3(); //obj100 : fps100 , obj200 : fps75 , obj400 : fps55

            UpdateCollision_AABBCulling(); //obj110 : fps100 , obj200 : fps77 , obj400 : fps58


            _sphereTree_being.Process();
            _sphereTree_struct.Process();
        }


        //중복되는 영역을 제외한 검사영역 : 기존 9개의 영역에서 5개의 영역으로 줄였다
        Vector3Int[] __cellIndexes_5 = new Vector3Int[] {
            new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 0),
        };


        //충돌원이 셀을 벗어나는 상황별 검사영역 
        //가정 : 셀보다 충돌원이 작아야 한다. 
        //List<Vector3Int[]> __cellIndexes_Max4 = new List<Vector3Int[]> {
        Vector3Int[][] __cellIndexes_Max4 = new Vector3Int[][] {
            new Vector3Int[] { new Vector3Int(0, 0, 0), }, //center = 0,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), }, //right = 1,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0),}, //rightUp = 2
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0),}, //up = 3,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, 0),}, //leftUp = 4,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0),}, //left = 5,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0),}, //leftDown = 6,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, -1, 0),}, //down = 7,
            new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, -1, 0), new Vector3Int(0, -1, 0),}, //rightDown = 8
        };

        //충돌원이 셀에 겹친 영역을 구함
        public int GetOverlapCellSpace(Being being)
        {

            //Vector3 center = SingleO.gridManager.ToPosition3D_Center(being._cellInfo._index);
            Vector3 center = being._cur_cell._pos3d_center;
            Vector3 rate = being.GetPos3D() - center;
            float cellHalfSize = SingleO.gridManager._cellSize_x * 0.5f;

            //return 8;

            if (Mathf.Abs(rate.x) + being._collider_radius <= cellHalfSize && Mathf.Abs(rate.z) + being._collider_radius <= cellHalfSize)
            {
                return 0;
            }

            return (int)Misc.GetDir8_AxisY(rate);
        }

        public void Draw_AABBCulling()
        {
            foreach (AABBCulling.UnOrderedEdgeKey key in _aabbCulling.GetOverlap()) //fixme : 조회속도가 빠른 자료구조로 바꾸기 
            {
                Being src = _linearSearch_list[key._V0];
                Being dst = _linearSearch_list[key._V1];
                if ((object)src == (object)dst) continue;

                DebugWide.DrawCube(src.GetPos3D(), new Vector3(src._collider_radius * 2, 0, src._collider_radius * 2), Color.white);
                DebugWide.DrawCube(dst.GetPos3D(), new Vector3(dst._collider_radius * 2, 0, dst._collider_radius * 2), Color.white);
                DebugWide.DrawLine(src.GetPos3D(), dst.GetPos3D(), Color.green);
                //DebugWide.DrawLine(src._getBounds_min, src._getBounds_max, Color.white);
                //DebugWide.DrawLine(dst._getBounds_min, dst._getBounds_max, Color.white);
            }
        }

        public void Draw_CollisionSphere()
        {
            int src_count = _linearSearch_list.Count;
            for (int sc = 0; sc < src_count; sc++)
            {
                DebugWide.DrawCircle(_linearSearch_list[sc].GetPos3D(), _linearSearch_list[sc]._collider_radius, Color.green);
            }

        }

        //private void OnDrawGizmos()
        //{
        //          Bounds bounds = GetBounds_CameraView();
        //          Gizmos.DrawWireCube(bounds.center, bounds.size);
        //}

        //테스트용으로 임시 사용 
        //public void OnDrawGizmos()
        //{
        //          //          int src_count = _linearSearch_list.Count;
        //          //          //int cell_count = 0;
        //          //          for (int sc = 0; sc < src_count; sc++)
        //          //          {
        //          //              Bounds bb = _linearSearch_list[sc].GetBounds();
        //          //              Gizmos.DrawWireCube(bb.center, bb.size); 
        //          //          }

        //}

        public void UpdateCollision_AABBCulling()
        {

            //====================================
            // 발사체 갱신 
            //====================================
            int shot_count = _shots.Count;
            for (int si = 0; si < shot_count; si++)
            {
                _shots[si].Update_Shot();
            }
            //====================================

            Bounds cameraViewBounds = GetBounds_CameraView();
            Being selected = SingleO.touchControl._selected;
            if (null != (object)selected)
            {
                //SingleO.gridManager.Update_FogOfWar(selected.GetPos3D(), selected._move._direction);
                selected.SetVisible(true);
            }


            //Vector3 collisionCellPos_center = ConstV.v3_zero;
            CellSpace structTile = null;
            Being src = null, dst = null;
            //Vector3Int ix = ConstV.v3Int_zero;
            int src_count = _linearSearch_list.Count;
            for (int key = 0; key < src_count; key++)
            {
                src = _linearSearch_list[key];

                //============================
                src.UpdateAll(); //객체 갱신
                //============================

                if (true == src.isDeath()) continue;
                //if (false == src.gameObject.activeInHierarchy) continue; //<- 코드제거 : 죽지 않았으면 안보이는 상태라 해도 처리해야 한다 


                //===============================================================================


                //객체 컬링 처리
                if (null != (object)selected)
                {
                    //Culling_SightOfView(selected, src);
                }
                else
                {
                    //Culling_ViewFrustum(cameraViewBounds, src); //테스트를 위해 주석 
                }

                //============================
                //src.Apply_Bounds(); //경계상자 위치갱신
                _aabbCulling.SetEndPoint(key, src); //aabb 갱신 
                //============================
            }


            //==============================================
            //AABB 삽입정렬 및 충돌처리
            //==============================================
            _aabbCulling.UpdateXZ();

            //int overlapCount = _aabbCulling.GetOverlap().Count;
            //AABBCulling.UnOrderedEdgeKey[] toArray = _aabbCulling.GetOverlap().ToArray();
            //for (int i = 0; i < overlapCount;i++)
            //{
            //    src = _linearSearch_list[ toArray[i]._V0 ];
            //    dst = _linearSearch_list[ toArray[i]._V1 ];

            //    if (null == (object)src || true == src.isDeath()) continue;
            //    if (null == (object)dst || true == dst.isDeath()) continue;
            //    if ((object)src == (object)dst) continue;

            //    CollisionPush(src, dst);
            //}

            foreach (AABBCulling.UnOrderedEdgeKey key in _aabbCulling.GetOverlap()) //fixme : 조회속도가 빠른 자료구조로 바꾸기 
            {
                src = _linearSearch_list[key._V0];
                dst = _linearSearch_list[key._V1];

                //DebugWide.LogBlue(_aabbCulling.GetOverlap().Count + "   " + key._V0 + "  " + key._V1 + "   " + src + "  " + dst); //chamto test

                if (null == (object)src || true == src.isDeath()) continue;
                if (null == (object)dst || true == dst.isDeath()) continue;
                //if (false == src.gameObject.activeInHierarchy) continue; //<- 코드제거 : 죽지 않았으면 안보이는 상태라 해도 처리해야 한다 
                //if (false == dst.gameObject.activeInHierarchy) continue;
                if ((object)src == (object)dst) continue;

                CollisionPush(src, dst);
            }

            //==============================================

            for (int key = 0; key < src_count; key++)
            {
                src = _linearSearch_list[key];

                //==========================================
                //동굴벽과 캐릭터 충돌처리 
                //if (SingleO.gridManager.HasStructTile(src.transform.position, out structTile))
                if (SingleO.cellPartition.HasStructTile(src.GetPos3D(), out structTile))
                {
                    //CollisionPush_StructTile(src, structTile);
                    Vector3 getPos = SingleO.gridManager.GetBorder_StructTile(src.GetPos3D(), structTile);
                    src.SetPos(getPos);
                }
                //==========================================

                src.Apply_UnityPosition();
            }
        }

        public void UpdateCollision_UseDirectGrid3x3()
        {
            //return; //chamto test

            //====================================
            // 발사체 갱신 
            //====================================
            int shot_count = _shots.Count;
            for (int si = 0; si < shot_count; si++)
            {
                _shots[si].Update_Shot();
            }
            //====================================


            Bounds cameraViewBounds = GetBounds_CameraView();
            Being selected = SingleO.touchControl._selected;
            if (null != selected)
            {
                SingleO.gridManager.Update_FogOfWar(selected.transform.position, selected._move._direction);
                selected.SetVisible(true);
            }


            Vector3 collisionCellPos_center = ConstV.v3_zero;
            //CellInfo cellInfo = null;
            CellSpace cellSpace = null;

            //foreach (Being src in _linearSearch_list)
            Being src = null, dst = null;
            Vector3Int[] cellIndexes = null;
            Vector3Int ix = ConstV.v3Int_zero;
            int src_count = _linearSearch_list.Count;
            int cell_count = 0;
            for (int sc = 0; sc < src_count; sc++)
            {
                src = _linearSearch_list[sc];

                //============================
                src.UpdateAll(); //객체 갱신
                //============================

                //if (false == src.gameObject.activeInHierarchy) continue;
                //if (null == src._cellInfo) continue;
                if (true == src.isDeath()) continue;

                //src._move._direction = Misc.GetDir64_Normal3D(src._move._direction);
                //src._move._direction = src._move._direction.normalized; //성능상의 차이가 없음 

                //1. 3x3그리드 정보를 가져온다
                //foreach (Vector3Int ix in SingleO.gridManager._indexesNxN[3]) //9개의 영역 : 객체200 fps60
                //foreach (Vector3Int ix in __cellIndexes_5) //5개의 영역 (중복되는 4개의 영역 제거) : 객체200 fps80
                //foreach(Vector3Int ix in __cellIndexes_Max4[this.GetOverlapCellSpace(src)]) //충돌원이 셀에 겹친 영역만 가져옴 (최대 4개 영역) : 객체 200 fps90
                cellIndexes = __cellIndexes_Max4[this.GetOverlapCellSpace(src)];
                cell_count = cellIndexes.Length;
                for (int cc = 0; cc < cell_count; cc++)
                {
                    ix = cellIndexes[cc];

                    cellSpace = SingleO.cellPartition.GetCellSpace(ix + src._cellInfo._index);
                    //cellInfo = SingleO.gridManager.GetCellInfo(src._cellInfo._index);
                    //if (null == cellInfo) continue;

                    //foreach (Being dst in cellInfo)
                    //for (LinkedListNode<Being> curNode = cellInfo.First; null != curNode; curNode = curNode.Next)
                    Being next = cellSpace._children;
                    while (null != (object)next)
                    {
                        dst = next;
                        next = next._next_sibling;
                        //dst = curNode.Value;

                        //if (false == dst.gameObject.activeInHierarchy) continue;
                        if ((object)src == (object)dst) continue;
                        if (null == (object)dst || true == dst.isDeath()) continue;

                        CollisionPush(src, dst);
                    }
                }


                //동굴벽과 캐릭터 충돌처리 
                if (SingleO.cellPartition.HasStructTile(src.GetPos3D(), out cellSpace))
                {
                    CollisionPush_StructTile(src, cellSpace);
                    //CollisionPush_Rigid(src, structTile);
                }

                //객체 컬링 처리
                if (null != selected)
                {
                    //Culling_SightOfView(selected, src);
                }
                else
                {
                    //Culling_ViewFrustum(cameraViewBounds, src);
                }

            }

            //DebugWide.LogRed(_listTest.Count + "  총회전:" + count); //114 , 1988
        }



        public void Culling_SightOfView(Being selected, Being target)
        {

            bool onOff = false;
            //챔프 시야에 없으면 안보이게 처리함
            if (true == IsVisibleArea(selected, target.GetPos3D()))
            {
                onOff = true;
            }

            //술통은 항상 보이게 한다 -  임시 처리
            if ((object)target == (object)selected || Being.eKind.barrel == target._kind)
            {
                onOff = true;
            }
            target.SetVisible(onOff);

        }


        public void Culling_ViewFrustum(Bounds viewBounds, Being target)
        {

            if (true == viewBounds.Contains(target.GetPos3D()))
            {
                target.SetVisible(true);
            }
            else
            {
                target.SetVisible(false);
            }

        }

        //챔프를 중심으로 3x3그리드 영역의 정보를 가지고 충돌검사한다
        public void UpdateCollision_UseGrid3x3() //3x3 => 5x5 => 7x7 ... 홀수로 그리드 범위를 늘려 테스트 해볼 수 있다
        {
            CellInfo cellInfo = null;
            foreach (Being src in _linearSearch_list)
            {

                //1. 3x3그리드 정보를 가져온다
                cellInfo = SingleO.gridManager.GetCellInfo_NxN(src._cellInfo._index, 3);

                foreach (Being dst in cellInfo)
                {
                    if (src == dst) continue;
                    CollisionPush(src, dst);
                }
            }
        }

        //딕셔너리 보다 인덱싱 속도가 빠르다. 안정적 객체수 : 500
        public void UpdateCollision_UseList()
        {

            Being src, dst;
            //한집합의 원소로 중복되지 않는 한쌍 만들기  
            for (int i = 0; i < _linearSearch_list.Count - 1; i++)
            {
                for (int j = i + 1; j < _linearSearch_list.Count; j++)
                {
                    //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test
                    src = _linearSearch_list[i];
                    dst = _linearSearch_list[j];
                    CollisionPush(src, dst);
                    //CollisionPush(src.transform, dst.transform);
                }
            }

        }



        //가시영역 검사 
        public bool IsVisibleArea(Being src, Vector3 dstPos)
        {
            //return true; //chamto test

            //Vector3 dirToDst = dstPos - src.transform.position;
            Vector3 dirToDst = VOp.Minus(dstPos, src.transform.position);
            float dirToDstsq = dirToDst.sqrMagnitude;
            float DIS = GridManager.MeterToWorld * 7f;
            if (dirToDstsq < DIS * DIS) //목표와의 거리가 7미터 안
            {

                //대상과 정반대 방향이 아닐때 처리 
                //dirToDst.Normalize();
                //dirToDst = Misc.GetDir360_Normal3D(dirToDst); 
                dirToDst = VOp.Normalize(dirToDst);
                DIS = GridManager.MeterToWorld * 2f;
                //if (Math.Cos(Mathf.Deg2Rad * 45f) < Vector3.Dot(src._move._direction, dirToDst) || dirToDstsq < DIS*DIS)
                if (45f > Geo.Angle_AxisY(src._move._direction, dirToDst) || dirToDstsq < DIS * DIS)
                {

                    //여기까지 오면 캐릭터 검사는 통과된것이다

                    //구조물 검사 
                    //SphereModel model = _sphereTree_struct.RayTrace_FirstReturn(src.transform.position, dstPos, null); 
                    //if (null == model) return true;

                    //보이는 위치의 타일인지 검사한다 
                    //if(true == SingleO.gridManager.IsVisibleTile(src._getPos3D, dstPos, 0.1f))
                    if (true == SingleO.cellPartition.IsVisibleTile(src, src.GetPos3D(), dstPos, 0.1f))
                        return true;
                }

            }

            return false;
        }

        //____________________________________________
        //              선분을 이용한 CCD   
        //____________________________________________
        //public Vector3[] LineSegmentTest(Vector3 origin, Vector3 last)
        //{
        //    LineSegment3 lineSeg = LineSegment3.zero;
        //    lineSeg.origin = origin;
        //    //lineSeg.direction = dir;
        //    lineSeg.last = last;

        //    LinkedList<Vector3> cellList = new LinkedList<Vector3>();
        //    float CELL_HARF_SIZE = SingleO.gridManager._cellSize_x * 0.5f;
        //    float CELL_SQUARED_RADIUS = Mathf.Pow(CELL_HARF_SIZE, 2f);
        //    float sqrDis = 0f;
        //    float t_c = 0;

        //    //기준셀값을 더해준다. 기준셀은 그리드값 변환된 값이이어야 한다 
        //    Vector3Int originToGridInt = SingleO.gridManager.ToPosition2D(origin);
        //    Vector3 originToPos = SingleO.gridManager.ToPosition3D(originToGridInt);
        //    Vector3 worldCellCenterPos = ConstV.v3_zero;
        //    foreach (Vector3Int cellLBPos in SingleO.gridManager._indexesNxN[7])
        //    {
        //        //셀의 중심좌표로 변환 
        //        worldCellCenterPos = SingleO.gridManager.ToPosition3D_Center(cellLBPos);
        //        worldCellCenterPos += originToPos;


        //        //시작위치셀을 포함하거나 뺄때가 있다. 사용하지 않느다 
        //        //선분방향과 반대방향인 셀들을 걸러낸다 , (0,0)원점 즉 출발점의 셀은 제외한다 
        //        if(0 == cellLBPos.sqrMagnitude || 0 >= Vector3.Dot(lineSeg.direction, worldCellCenterPos - origin))
        //        {
        //            continue;
        //        }

        //        sqrDis = lineSeg.MinimumDistanceSquared(worldCellCenterPos, out t_c);

        //        //선분에 멀리있는 셀들을 걸러낸다
        //        if(CELL_SQUARED_RADIUS < sqrDis)
        //        {
        //            continue;
        //        }

        //        cellList.AddLast(worldCellCenterPos);
        //    }


        //    Vector3[] result = (from v3 in cellList
        //                        orderby (v3-origin).sqrMagnitude ascending
        //                        select v3).ToArray();

        //    return result;
        //}

        //____________________________________________
        //                  충돌 검사   
        //____________________________________________

        public void CollisionPush(Being src, Being dst)
        {
            if (null == (object)src || null == (object)dst) return;

            //float max_sqrRadius = Mathf.Max(src._collider_sqrRadius, dst._collider_sqrRadius);
            float max_sqrRadius = dst._collider_sqrRadius;
            if (src._collider_sqrRadius > dst._collider_sqrRadius)
                max_sqrRadius = src._collider_sqrRadius;


            //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
            Vector3 dir_dstTOsrc = VOp.Minus(src.GetPos3D(), dst.GetPos3D());
            Vector3 n = ConstV.v3_zero;
            float sqr_dstTOsrc = dir_dstTOsrc.sqrMagnitude;
            float r_sum = (src._collider_radius + dst._collider_radius);
            float sqr_r_sum = r_sum * r_sum;

            //1.두 캐릭터가 겹친상태 
            if (sqr_dstTOsrc < sqr_r_sum)
            {
                //==========================================
                float f_sum = src._force + dst._force;

                float rate_src = 1f - (src._force / f_sum);
                float rate_dst = 1f - rate_src;


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

                src.SetPos(src.GetPos3D() + n * len_bt_src);
                dst.SetPos(dst.GetPos3D() - n * len_bt_dst);

                //src.OnCollision_MovePush(dst, n, meterPerSecond);
                //dst.OnCollision_MovePush(src, -n, meterPerSecond);
            }
        }


        //public void CollisionPush_Rigid(Being src, CellSpace structTile)
        //{
        //    //이상진동 : 방향의 평균내기 방식
        //    //Vector3 smoothDir = Misc.GetDir8Normal_AxisY(structTile._dir);
        //    //smoothDir += src._move._direction.normalized;
        //    //smoothDir /= 2f;
        //    //src._move.Move_Forward(smoothDir, 2f, 0.5f);
        //    //return;

        //    const float Tile_Radius = 0.08f;
        //    //2. 그리드 안에 포함된 다른 객체와 충돌검사를 한다
        //    Vector3 sqr_dis = src.transform.localPosition - structTile._pos3d_center;
        //    float r_sum = src._collider_radius + Tile_Radius;

        //    //1.두 캐릭터가 겹친상태 
        //    if (sqr_dis.sqrMagnitude < Mathf.Pow(r_sum, 2))
        //    {
        //        //DebugWide.LogBlue(i + "_" + j + "_count:"+_characters.Count); //chamto test

        //        //todo : 최적화 필요 

        //        Vector3 n = sqr_dis.normalized;
        //        //n = Vector3.back;
        //        //Vector3 n = sqr_dis;
        //        float div_dis = 0.5f;

        //        //2.반지름 이상으로 겹쳐있는 경우
        //        if (sqr_dis.sqrMagnitude * 2 < Mathf.Pow(r_sum, 2))
        //        {
        //            //3.완전 겹쳐있는 경우
        //            if (n == ConstV.v3_zero)
        //            {
        //                //방향값이 없기 때문에 임의로 지정해 준다. 
        //                n = Misc.GetDir8_Random_AxisY();
        //            }

        //            div_dis = 0.2f;
        //        }

        //        //src.transform.position = collisionCellPos_center + n * 0.16f;
        //        //src._move.Move_Forward(n, 2f, div_dis);
        //        src.Move_Forward(n, div_dis, true);
        //        //DebugWide.LogBlue(SingleO.gridManager.ToCellIndex(src.transform.position, ConstV.v3_up) + "   " + src.transform.position);
        //    }
        //}

        //고정된 물체와 충돌 검사 : 동굴벽 등 
        public void CollisionPush_StructTile(Being src, CellSpace structTile)
        {
            if (null == structTile) return;

            //Vector3 srcPos = src._transform.position;
            Vector3 srcPos = src.GetPos3D();
            //Vector3 centerToSrc_dir = srcPos - structTile._pos3d_center;
            Vector3 centerToSrc_dir = VOp.Minus(srcPos, structTile._pos3d_center);
            Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);

            float size = SingleO.gridManager._cellSize_x * 0.5f;
            Vector3 center = ConstV.v3_zero;
            LineSegment3 line3 = new LineSegment3();
            //8방향별 축값 고정  
            switch (structTile._eDir)
            {
                case eDirection8.up:
                    {
                        srcPos.z = structTile._pos3d_center.z + size;
                    }
                    break;
                case eDirection8.down:
                    {
                        srcPos.z = structTile._pos3d_center.z - size;
                    }
                    break;
                case eDirection8.left:
                    {
                        srcPos.x = structTile._pos3d_center.x - size;
                    }
                    break;
                case eDirection8.right:
                    {
                        srcPos.x = structTile._pos3d_center.x + size;
                    }
                    break;
                case eDirection8.leftUp:
                    {
                        //down , right
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x - size;
                            srcPos.z = structTile._pos3d_center.z + size;
                            break;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        //down , left
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x + size;
                            srcPos.z = structTile._pos3d_center.z + size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        //up , right
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x - size;
                            srcPos.z = structTile._pos3d_center.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;

                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z += size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z -= size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;
                case eDirection8.rightDown:
                    {
                        //up , left
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x + size;
                            srcPos.z = structTile._pos3d_center.z - size;
                            break;
                        }


                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return;
                        center = structTile._pos3d_center;
                        center.x -= size;
                        center.z -= size;
                        line3.origin = center;

                        center = structTile._pos3d_center;
                        center.x += size;
                        center.z += size;
                        line3.last = center;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;

            }
            //src._transform.position = srcPos;
            src.SetPos(srcPos);

        }


        public void ClearAll()
        {

            foreach (Being b in _beings.Values)
            {
                GameObject.Destroy(b.gameObject);
            }

            _beings.Clear();

        }

        public Being GetCharacter(uint id)
        {
            Being being = null;
            if (true == _beings.TryGetValue(id, out being))
            {
                return being;
            }


            return null;
        }






        public struct Param_RangeTest
        {
            //==============================================
            public Camp.eRelation vsRelation;
            public ChampUnit srcUnit;
            public Camp.eKind src_campKind;
            public Vector3 src_pos;

            public float minRadius;
            public float maxRadius;
            public float maxRadiusSqr;

            public delegate bool Proto_ConditionCheck(ref Param_RangeTest param, SphereModel dstModel);
            public Proto_ConditionCheck callback;
            //==============================================

            public Param_RangeTest(ChampUnit in_srcUnit, Camp.eRelation in_vsRelation, float meter_minRadius, float meter_maxRadius)
            {
                vsRelation = in_vsRelation;
                srcUnit = in_srcUnit;
                src_campKind = in_srcUnit._campKind;
                src_pos = in_srcUnit.transform.position;
                minRadius = meter_minRadius * GridManager.ONE_METER;
                maxRadius = meter_maxRadius * GridManager.ONE_METER;
                maxRadiusSqr = maxRadius * maxRadius;

                callback = Param_RangeTest.Func_ConditionCheck;
            }

            //==============================================

            static public bool Func_ConditionCheck(ref Param_RangeTest param, SphereModel dstModel)
            {
                //return true;

                //기준객체는 검사대상에서 제외한다 
                if (param.srcUnit._sphereModel == dstModel) return false;

                ChampUnit dstUnit = dstModel.GetLink_UserData<ChampUnit>();

                if (null != (object)dstUnit && param.vsRelation != Camp.eRelation.Unknown)
                {
                    if (dstUnit.isDeath()) return false; //죽어있는 대상은 탐지하지 않는다 

                    Camp.eRelation getRelation = SingleO.campManager.GetRelation(param.src_campKind, dstUnit._belongCamp._eCampKind);

                    //요청 관계인지 검사한다 
                    if (param.vsRelation == getRelation)
                    {
                        //가시거리 검사 
                        return SingleO.cellPartition.IsVisibleTile(param.srcUnit, param.src_pos, dstModel.GetPos(), 0.1f);
                    }
                }

                return false;
            }
        }
        public ChampUnit tree_GetNearCharacter(ChampUnit src, Camp.eRelation vsRelation, float meter_minRadius, float meter_maxRadius)
        {
            //Old_GetNearCharacter(src, vsRelation, meter_minRadius, meter_maxRadius); //chamto test

            Param_RangeTest param = new Param_RangeTest(src, vsRelation, meter_minRadius, meter_maxRadius);
            SphereModel sphereModel = _sphereTree_being.RangeTest_MinDisReturn(ref param);


            if (null != sphereModel)
            {
                //DebugWide.LogBlue(sphereModel.GetLink_UserData<ChampUnit>()); //chamto test
                return sphereModel.GetLink_UserData<ChampUnit>();
            }


            return null;
        }

        /// <summary>
        /// 가까운 대상 객체의 충돌원이 지정된 최소/최대 원에 포함되는지 검사한다 
        /// 조건에 포함하는 가장 가까운 객체를 반환한다
        /// 대상 객체의 충돌원 크기와 상관없이, 최대 원 크기의 그리드를 가져와 그리드 안에있는 객체들로만 검사한다   
        /// </summary>
        public ChampUnit GetNearCharacter(ChampUnit src, Camp.eRelation vsRelation, float meter_minRadius, float meter_maxRadius)
        {
            if (null == (object)src) return null;

            float wrd_minRad = meter_minRadius * GridManager.ONE_METER;
            float wrd_maxRad = meter_maxRadius * GridManager.ONE_METER;
            float sqr_minRadius = 0;
            float sqr_maxRadius = 0;
            float min_value = wrd_maxRad * wrd_maxRad * 1000f; //최대 반경보다 큰 최대값 지정
            float sqr_dis = 0f;


            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint NxN = SingleO.gridManager.GetNxNIncluded_CircleRadius(wrd_maxRad);
            int TILE_MAP_SIZE = CellSpacePartition.MAP_WIDTH;

            //int count = 0;
            Index2 tempv2;
            CellSpace cell = null;
            ChampUnit target = null;
            Index2[] array = SingleO.gridManager._indexesNxN[NxN];
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                //int pos1d = pos2d.x + pos2d.y * _tileMapSize_x; 1차원 값으로 변환 
                tempv2 = array[i];
                tempv2.x += src._cur_cell._pos2d.x;
                tempv2.y += src._cur_cell._pos2d.y;
                cell = SingleO.cellPartition.GetCellSpace(tempv2.y * TILE_MAP_SIZE + tempv2.x);

                if (null == cell) continue;

                //foreach (Being dst in cell)
                Being dst = null;
                Being nextBeing = cell._children;
                while (null != (object)nextBeing)
                {
                    dst = nextBeing;
                    nextBeing = nextBeing._next_sibling;

                    ChampUnit champDst = dst as ChampUnit;
                    if (null == (object)champDst) continue;
                    if ((object)src == (object)dst) continue;
                    if (true == dst.isDeath()) continue; //쓰러진 객체는 처리하지 않는다 


                    if (vsRelation != Camp.eRelation.Unknown)//&& null != src._belongCamp && null != champDst._belongCamp)
                    {
                        Camp.eRelation getRelation = SingleO.campManager.GetRelation(src._belongCamp._eCampKind, champDst._belongCamp._eCampKind);

                        //요청 관계가 아니면 처리하지 않는다 
                        if (vsRelation != getRelation)
                            continue;
                    }

                    //count++;
                    //==========================================================
                    sqr_minRadius = (wrd_minRad + dst._collider_radius) * (wrd_minRad + dst._collider_radius);
                    sqr_maxRadius = (wrd_maxRad + dst._collider_radius) * (wrd_maxRad + dst._collider_radius);
                    sqr_dis = VOp.Minus(src.GetPos3D(), dst.GetPos3D()).sqrMagnitude;

                    //최대 반경 이내일 경우
                    if (sqr_minRadius <= sqr_dis && sqr_dis <= sqr_maxRadius)
                    {

                        //DebugWide.LogBlue(min_value + "__" + sqr_dis +"__"+  dst.name); //chamto test

                        //기존 객체보다 더 가까운 경우
                        if (min_value > sqr_dis)
                        {
                            min_value = sqr_dis;
                            target = champDst;

                            //첫번째 발견하면 바로 탈출한다 - 임시 
                            return target;
                        }
                    }
                    //==========================================================v
                }
            }

            //DebugWide.LogRed(count);
            return target;
        }



        //임시주석 
        /// <summary>
        /// 지정된 범위의 캐릭터가 특정캐릭터를 쳐다보게 설정한다
        /// </summary>
        /// <param name="target">Target.</param>
        //public void LookAtTarget(Being src, uint gridRange_NxN)
        //{

        //    Vector3 dir = ConstV.v3_zero;
        //    CellSpace cell = null;

        //    Index2[] array = SingleO.gridManager._indexesNxN[gridRange_NxN];
        //    int count = array.Length;
        //    for (int i = 0; i < count; i++)
        //    {

        //        cell = SingleO.cellPartition.GetCellSpace(array[i] + src._cur_cell._pos2d); //SingleO.gridManager.GetCellInfo(ix + src._cellInfo._index);
        //        if (null == cell) continue;

        //        Being dst = null;
        //        Being getBeing = cell._children;
        //        while (null != (object)getBeing)
        //        {
        //            dst = getBeing;
        //            getBeing = getBeing._next_sibling;

        //            if ((object)src == (object)dst) continue;

        //            if ((int)Behavior.eKind.Idle <= (int)dst._behaviorKind && (int)dst._behaviorKind <= (int)Behavior.eKind.Idle_Max)
        //            {
        //                dir = VOp.Minus(src.GetPos3D(), dst.GetPos3D());

        //                //그리드범위에 딱들어가는 원을 설정, 그 원 밖에 있으면 처리하지 않는다 
        //                //==============================
        //                float sqr_radius = (float)(gridRange_NxN) * 0.5f; //반지름으로 변환
        //                sqr_radius *= SingleO.gridManager._cellSize_x; //셀크기로 변환
        //                sqr_radius *= sqr_radius; //제곱으로 변환
        //                if (sqr_radius < dir.sqrMagnitude)
        //                    continue;
        //                //==============================

        //                //dst.Idle_View(dir, true); //todo 나중에 수정된 함수 호출하기 
        //                dst._behaviorKind = Behavior.eKind.Idle_LookAt;
        //            }
        //        }
        //    }
        //}

        //임시주석 
        //public void SetAll_Behavior(Behavior.eKind kind)
        //{
        //    foreach (Being t in _beings.Values)
        //    {

        //        t._behaviorKind = kind;

        //    }
        //}

        int __shotNextCount = -1;
        public Shot GetNextShot()
        {

            //최대 샷 개수만큼만 사용안하는 샷을 찾는다
            for (int i = 0; i < _shots.Count; i++)
            {
                __shotNextCount++;
                //__shotNextCount = 0; //chamto test
                __shotNextCount %= _shots.Count;

                if (false == _shots[__shotNextCount]._on_theWay)
                {
                    //사용안하면 바로 반환한다 
                    return _shots[__shotNextCount];
                }
            }

            return null;



            //foreach(Shot shot in _shots.Values)
            //{
            //    if(false == shot._on_theWay)    
            //    {
            //        return shot;
            //    }
            //}
            //return null;
        }


        //____________________________________________
        //                  객체 생성 
        //____________________________________________

        public GameObject CreatePrefab(string prefabPath, Transform parent, string name)
        {
            const string root = "Warcraft/Prefab/";
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(root + prefabPath)) as GameObject;
            obj.transform.SetParent(parent, false);
            obj.transform.name = name;


            return obj;
        }



        public ChampUnit Create_Character(Transform parent, Being.eKind eKind, Camp belongCamp, Vector3 pos)
        {
            _id_sequence++;

            GameObject obj = CreatePrefab("0_champ/" + eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            ChampUnit cha = obj.AddComponent<ChampUnit>();
            ////obj.AddComponent<SortingGroup>(); //drawcall(batches) 증가 문제로 주석  
            cha._move = obj.AddComponent<Movement>();
            cha._move._being = cha;
            cha._ai = obj.AddComponent<AI>();
            cha._ai.Init();
            cha._id = _id_sequence;
            cha._kind = eKind;
            cha._belongCamp = belongCamp;
            cha.transform.position = pos;
            cha._collider = obj.GetComponent<SphereCollider>();
            cha._collider_radius = cha._collider.radius;
            cha._collider_sqrRadius = cha._collider_radius * cha._collider_radius;

            ////==============================================
            ////가지(촉수) 등록
            GameObject bone = CreatePrefab("3_part/bone", obj.transform, "bone");
            cha._bone.Load(obj.transform);
            cha._limbs = Limbs.CreateLimbs(obj.transform);
            cha._limbs.Init(cha, cha._move, cha._bone);

            ////==============================================
            //effectIcon 등록 
            CreatePrefab("3_part/effectIcon", obj.transform, "effectIcon");

            //==============================================
            ////구트리 등록 
            SphereModel model = _sphereTree_being.AddSphere(pos, cha._collider_radius, SphereModel.Flag.CREATE_LEVEL_LAST);
            _sphereTree_being.AddIntegrateQ(model);
            model.SetLink_UserData<ChampUnit>(cha);
            //DebugWide.LogRed(cha._collider_radius + "  radius ");
            ////==============================================

            cha._sphereModel = model;
            cha.Init();

            ////==============================================

            _beings.Add(_id_sequence, cha);
            _linearSearch_list.Add(cha); //속도향상을 위해 중복된 데이터 추가

            return cha;
        }

        public Shot Create_Shot(Transform parent, Being.eKind eKind, Vector3 pos)
        {
            _id_shot_sequence++;

            GameObject obj = CreatePrefab("1_effect/" + eKind.ToString(), parent, _id_shot_sequence.ToString("000") + "_" + eKind.ToString());
            Shot shot = obj.AddComponent<Shot>();
            obj.AddComponent<SortingGroup>();
            Movement mov = obj.AddComponent<Movement>();
            mov._being = shot;
            shot._id = _id_shot_sequence;
            shot._kind = eKind;
            shot.transform.position = pos;
            shot._collider = obj.GetComponent<SphereCollider>();
            shot._collider_radius = shot._collider.radius;
            shot._collider_sqrRadius = shot._collider_radius * shot._collider_radius;

            //==============================================
            //구트리 등록 
            SphereModel model = _sphereTree_being.AddSphere(pos, shot._collider_radius, SphereModel.Flag.CREATE_LEVEL_LAST);
            _sphereTree_being.AddIntegrateQ(model);
            //==============================================

            shot._sphereModel = model;
            shot.Init();

            //==============================================

            ///////_shots.Add(_id_shot_sequence, shot);

            _shots.Add(shot);

            return shot;
        }

        public Obstacle Create_Obstacle(Transform parent, Being.eKind eKind, Vector3 pos)
        {
            _id_sequence++;

            GameObject obj = CreatePrefab("2_misc/" + eKind.ToString(), parent, _id_sequence.ToString("000") + "_" + eKind.ToString());
            Obstacle obst = obj.AddComponent<Obstacle>();
            obj.AddComponent<SortingGroup>();
            Movement mov = obj.AddComponent<Movement>();
            mov._being = obst;
            obst._id = _id_sequence;
            obst._kind = eKind;
            obst.transform.position = pos;
            obst._collider = obj.GetComponent<SphereCollider>();
            obst._collider_radius = obst._collider.radius;
            obst._collider_sqrRadius = obst._collider_radius * obst._collider_radius;

            //==============================================
            //구트리 등록 
            SphereModel model = _sphereTree_being.AddSphere(pos, obst._collider_radius, SphereModel.Flag.CREATE_LEVEL_LAST);
            _sphereTree_being.AddIntegrateQ(model);
            //==============================================

            obst._sphereModel = model;
            obst.Init();

            //==============================================

            _beings.Add(_id_sequence, obst);
            _linearSearch_list.Add(obst);

            return obst;
        }



        public void Create_ChampCamp()
        {
            DateTime _startDateTime;
            string _timeTemp = "";

            if (null == SingleO.unitRoot) return;

            //==============================================
            _startDateTime = DateTime.Now;
            string Blue_CampName = "Champ_Sky";
            string White_CampName = "Skel_Gray";
            string Obstacle_CampName = "ExitGoGe";
            //int camp_position = 0;

            SingleO.campManager.Load_CampPlacement(Camp.eKind.Blue);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.White);
            SingleO.campManager.Load_CampPlacement(Camp.eKind.Obstacle);

            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Blue, Camp.eKind.White);
            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Hero, Camp.eKind.Blue);
            SingleO.campManager.SetRelation(Camp.eRelation.Enemy, Camp.eKind.Hero, Camp.eKind.White);

            Camp camp_HERO = SingleO.campManager.GetDefaultCamp(Camp.eKind.Hero);
            Camp camp_BLUE = SingleO.campManager.GetCamp(Camp.eKind.Blue, Blue_CampName.GetHashCode());
            Camp camp_WHITE = SingleO.campManager.GetCamp(Camp.eKind.White, White_CampName.GetHashCode());
            Camp camp_Obstacle = SingleO.campManager.GetCamp(Camp.eKind.Obstacle, Obstacle_CampName.GetHashCode());
            _timeTemp += "  ObjectManager.Create_ChampCamp.CampInfo  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            //==============================================
            _startDateTime = DateTime.Now;

            Being being = null;
            ChampUnit champ = null;
            int numMax_create = 0;
            // -- 블루 진형 --
            champ = Create_Character(SingleO.unitRoot, Being.eKind.lothar, camp_HERO, camp_Obstacle.GetPosition(0));
            champ._hp_max = 10000;
            champ._hp_cur = 10000;
            champ._force = 1;
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.footman, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.spearman, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ._mt_range_min = 1f;
            //champ._mt_range_max = 8f;
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.conjurer, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.knight, camp_BLUE, camp_BLUE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;

            _timeTemp += "  ObjectManager.Create_ChampCamp.Create_Character  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";

            numMax_create = 0;
            for (int i = 0; i < numMax_create; i++)
            {
                champ = Create_Character(SingleO.unitRoot, Being.eKind.peasant, camp_BLUE, camp_BLUE.RandPosition());
                champ._hp_max = 30;
                champ._hp_cur = 30;
                //champ._mt_range_min = 0.3f;
                //champ._mt_range_max = 0.5f;
                champ.GetComponent<AI>()._ai_running = true;
                //camp_position++;
            }

            //===================================================

            // -- 휜색 진형 --
            //camp_position = 0;
            //champ = Create_Character(SingleO.unitRoot, Being.eKind.raider, camp_WHITE, camp_WHITE.GetPosition(camp_position));
            //champ.GetComponent<AI>()._ai_running = true;
            //camp_position++;
            numMax_create = 0;
            for (int i = 0; i < numMax_create; i++)
            {
                champ = Create_Character(SingleO.unitRoot, Being.eKind.cleric, camp_WHITE, camp_WHITE.RandPosition());
                champ._mt_range_min = 1f;
                champ._mt_range_max = 4f;
                champ.GetComponent<AI>()._ai_running = true;
                //camp_position++;
                //champ.SetColor(Color.black);
            }

            numMax_create = 20;
            for (int i = 0; i < numMax_create; i++)
            {
                champ = Create_Character(SingleO.unitRoot, Being.eKind.footman, camp_WHITE, camp_WHITE.RandPosition());
                //champ.GetComponent<AI>()._ai_running = true;
                //camp_position++;
            }

            //===================================================

            // -- 장애물 진형 --
            numMax_create = 5;
            for (int i = 0; i < numMax_create; i++)
            {
                Obstacle ob = Create_Obstacle(SingleO.unitRoot, Being.eKind.barrel, camp_Obstacle.RandPosition());
                ob._force = 1000f;
            }

            //===================================================

            _startDateTime = DateTime.Now;

            // -- 발사체 미리 생성 --
            numMax_create = 300;
            for (int i = 0; i < numMax_create; i++)
            {
                //being = Create_Shot(SingleO.shotRoot, Being.eKind.spear, ConstV.v3_zero);
                being = Create_Shot(SingleO.shotRoot, Being.eKind.waterBolt, ConstV.v3_zero);
            }
            _timeTemp += "  ObjectManager.Create_ChampCamp.Create_Shot  : " + numMax_create + " :  " + (DateTime.Now.Ticks - _startDateTime.Ticks) / 10000f + "ms";
            //===================================================

            DebugWide.LogBlue(_timeTemp);
        }

    }
}
