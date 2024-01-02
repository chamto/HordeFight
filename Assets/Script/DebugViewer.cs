using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//using UnityEngine.Assertions;

using HordeFight;
using UtilGS9;



//========================================================
//==================     디버그      ==================
//========================================================
//namespace HordeFight
//{
    public struct DrawInfo
    {
        public enum eKind
        {
            Circle,
            Line,
        }

        public eKind kind;
        public Vector3 origin;
        public Vector3 last;
        public float radius;
        public Color color;

        public void Draw()
        {
            switch(kind)
            {
                case eKind.Circle:
                    DebugWide.DrawCircle(origin, radius, color);
                    break;
                case eKind.Line:
                    DebugWide.DrawLine(origin, last, color);
                    break;
                    
            }

        }

    }
    public class DebugViewer : MonoBehaviour
    {
        public float _length_interval = 0.1f;
        public Transform _origin = null;
        public Transform _target = null;

        private Tilemap _tilemap = null;
        private Dictionary<Vector3Int, Color> _colorMap = new Dictionary<Vector3Int, Color>();

        private Queue<DrawInfo> _drawQ = new Queue<DrawInfo>();

        private void Start()
        {
            GameObject game = GameObject.Find("Tilemap_Debug");
            if (null != game)
            {
                _tilemap = game.GetComponent<Tilemap>();
            }

        }

        public void AddDrawInfo(DrawInfo info)
        {
            _drawQ.Enqueue(info);
        }

         public void AddDraw_Circle(Vector3 origin, float radius, Color color)
        {

            DrawInfo drawInfo = new DrawInfo();
            drawInfo.kind = DrawInfo.eKind.Circle;
            drawInfo.origin = origin;
            drawInfo.radius = radius;
            drawInfo.color = color;

            AddDrawInfo(drawInfo);
        }

         public void AddDraw_Line(Vector3 origin, Vector3 last, Color color)
        {
            DrawInfo drawInfo = new DrawInfo();
            drawInfo.kind = DrawInfo.eKind.Line;
            drawInfo.origin = origin;
            drawInfo.last = last;
            drawInfo.color = color;

            AddDrawInfo(drawInfo);
        }

        public void ResetColor()
        {
            foreach (Vector3Int i3 in _colorMap.Keys)
            {
                _tilemap.SetColor(i3, Color.white);
            }
        }
        public void SetColor(Vector3 pos, Color c)
        {
            Vector3Int i3 = _tilemap.WorldToCell(pos);
            _tilemap.SetTileFlags(i3, UnityEngine.Tilemaps.TileFlags.None);
            _tilemap.SetColor(i3, c);

            if (!_colorMap.Keys.Contains(i3))
                _colorMap.Add(i3, c);
        }

        public void SetPos(Vector3 origin, Vector3 target)
        {
            _origin.position = origin;
            _target.position = target;
        }


        //private Vector3 _start = ConstV.v3_zero;
        //private Vector3 _end = ConstV.v3_right;
        //public void DrawLine(Vector3 start, Vector3 end)
        //{
        //    _start = start;
        //    _end = end;
        //}

        public void UpdateDraw_IndexesNxN()
        {
#if UNITY_EDITOR
            Index2 prev = ConstV.id2_zero;
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(7, ConstV.v3_up))
            foreach (Index2 cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter_Tornado(11, 11))
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter(7, ConstV.v3_up))
            {
                //DebugWide.LogBlue(v);
                Vector3 v3_cur = SingleO.cellPartition.ToPosition3D_Center(cur);
                Vector3 v3_prev = SingleO.cellPartition.ToPosition3D_Center(prev);
                Debug.DrawLine(v3_cur - Vector3.one * 0.3f, v3_cur + Vector3.one * 0.3f, Color.red);
                Debug.DrawLine(v3_prev, v3_cur);

                UnityEditor.Handles.Label(v3_cur, "( " + cur.x + " , " + cur.y + " )");

                prev = cur;

            }
#endif
        }

        public void UpdateDraw_FogOfWar_DivisionNum()
        {
#if UNITY_EDITOR
            BoundsInt bounds = SingleO.gridManager.GetTileMap_FogOfWar().cellBounds;
            RuleTile_Custom rule = null;
            byte divisionNum = 0;
            Vector3 pos3d = ConstV.v3_zero;
            //DebugWide.LogBlue(bounds);
            foreach (Vector3Int xy in bounds.allPositionsWithin)
            {
                //rule = SingleO.gridManager.GetTileMap_FogOfWar().GetTile<RuleExtraTile>(xy);
                rule = SingleO.gridManager.GetTileMap_FogOfWar().GetTile(xy) as RuleTile_Custom;
                if (null != rule)
                {
                    DebugWide.LogBlue(xy + " " + rule);   
                    pos3d = SingleO.gridManager.ToPosition3D(xy);
                    pos3d.x += 0.08f;
                    pos3d.z += 0.16f;
                    //divisionNum = rule._tileDataMap.Get_DivisionNum(xy); //chamto : Ver 2018.2.14f1 <RuleTile> => Ver 2022.3.9f1 업그레이드 작업
                    divisionNum = 0; //위 함수가 제거됨에 따라 임시로 0으로 설
                    if (0 < divisionNum)
                        UnityEditor.Handles.color = Color.red;
                    else
                        UnityEditor.Handles.color = Color.white;

                    UnityEditor.Handles.Label(pos3d, "" + divisionNum);
                }


            }
#endif
        }

        //public void UpdateDraw_StructTileDir()
        //{
        //    Color ccc = Color.white;
        //    foreach (CellSpace t in SingleO.gridManager._structTileList.Values)
        //    {
        //        if (eDirection8.none == t._eDir) continue;

        //        //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
        //        //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z

        //        ccc = Color.white;
        //        if (CellSpace.Specifier_DiagonalFixing == t._specifier)
        //        {
        //            ccc = Color.red;
        //        }

        //        Vector3 end = t._pos3d_center + Misc.GetDir8_Normal3D_AxisY(t._eDir) * 0.12f;
        //        Debug.DrawLine(t._pos3d_center, end, ccc);


        //        //UnityEditor.Handles.Label(t._center_3d, "( " + cur.x + " , " + cur.z + " )");

        //        //Vector3 crossL = t._v3Center;
        //        //crossL.x += -0.08f;
        //        //crossL.z += 0.08f;
        //        //Vector3 crossR = t._v3Center;
        //        //crossR.x += 0.08f;
        //        //crossR.z += -0.08f;
        //        //Debug.DrawLine(crossL, crossR, ccc);
        //    }
        //}

        public void Update_DrawEdges(bool printToPos)
        {
            PathFinder finder = SingleO.pathFinder;
            GridManager grid = SingleO.gridManager;
            Vector3Int tileBlock_size = finder.TILE_BLOCK_SIZE;

            int from_1d = 0;
            Vector3Int to_XY_2d;
            Vector3 from_3d, to_3d;
            NavGraphNode from_nav, to_nav;
            BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, tileBlock_size); //타일맵 크기 : (3939, 87, 1) , 타일맵의 음수부분은 사용하지 않는다
            foreach (Vector3Int from_XY_2d in boundsInt.allPositionsWithin)
            {
                from_1d = grid.ToPosition1D(from_XY_2d, boundsInt.size.x);
                from_3d = grid.ToPosition3D(from_XY_2d);

                SparseGraph.EdgeList list = finder._graph.GetEdges(from_1d);
                foreach (NavGraphEdge e in list)
                {
                    to_XY_2d = grid.ToPosition2D(e.To(), boundsInt.size.x);
                    to_3d = grid.ToPosition3D(to_XY_2d);

                    from_nav = finder._graph.GetNode(e.From()) as NavGraphNode;
                    to_nav = finder._graph.GetNode(e.To()) as NavGraphNode;
                    Debug.DrawLine(from_nav.Pos(), to_nav.Pos(), Color.green);

                    if(printToPos)
                        DebugWide.PrintText(to_nav.Pos(), Color.white, "to " + e.To());
                    //chamto test
                    //if (true == debugLog)
                    //{
                    //    DebugWide.LogBlue(e + "  " + from_XY_2d + "   " + to_XY_2d);
                    //}

                }
            }
        }


        public void Draw_Grid()
        {
            float size = SingleO.gridManager._cellSize_x;
            Vector3 start = ConstV.v3_zero;
            Vector3 end = ConstV.v3_zero;
            Vector3 xz_length = new Vector3(6.5f, 0, 4f);

            end.z = xz_length.z;
            for (int x = 0; size * x < xz_length.x; x++)
            {
                start.x = size * x;
                end.x = size * x;

                Debug.DrawLine(start, end, Color.white);
            }

            start = end = ConstV.v3_zero;
            end.x = xz_length.x;
            for (int z = 0; size * z < xz_length.z; z++)
            {
                start.z = size * z;
                end.z = size * z;

                Debug.DrawLine(start, end, Color.white);
            }

        }


        public void Update_IsVisible_SightOfView(Vector3 origin, Vector3 target, float length_interval)
        {
#if UNITY_EDITOR

            //값이 너무 작으면 바로 종료 
            if (0.01f >= length_interval)
            {
                return;
            }

            target = SingleO.gridManager.ToCenter3D_FromPosition3D(target);

            Vector3Int orgin_2d = SingleO.gridManager.ToPosition2D(origin);
            Vector3 origin_center = SingleO.gridManager.ToPosition3D_Center(orgin_2d);
            Vector3 line = target - origin_center;
            Vector3 n = line.normalized;

            CellSpace structTile = null;
            Vector3 next = ConstV.v3_zero;
            string keyword = "";
            for (int i = 0; line.sqrMagnitude > next.sqrMagnitude; i++)
            {
                //========================================================================
                Vector3 cr = Vector3.Cross(n, ConstV.v3_up);
                cr.Normalize();
                Debug.DrawLine(origin_center + next - cr * 0.01f, origin_center + next + cr * 0.01f, Color.red);
                //========================================================================


                next = n * length_interval * i;

                keyword = "";

                if (orgin_2d == SingleO.gridManager.ToPosition2D(origin_center + next))
                {
                    keyword += "Eq";
                    DebugWide.PrintText(origin_center + next, Color.white, keyword + ":" + i);
                    //UnityEditor.Handles.Label(origin_center + next, keyword + ":" + i);
                    continue;
                }


                if (true == SingleO.gridManager.HasStructTile(origin_center + next, out structTile))
                {
                    keyword += "St";

                    if (true == structTile._isUpTile)
                    {
                        //덮개타일인 경우
                        keyword += "Up";
                    }

                    switch (SingleO.gridManager.IsIncluded_InDiagonalArea(origin_center + next))
                    {
                        case GridManager._ReturnMessage_NotIncluded_InDiagonalArea:
                            keyword += "AntiDg";
                            break;
                        case GridManager._ReturnMessage_Included_InDiagonalArea:
                            //대각타일인 경우
                            keyword += "Dg";
                            break;
                    }


                }
                DebugWide.PrintText(origin_center + next, Color.white, keyword + ":" + i);
                //UnityEditor.Handles.Label(origin_center + next, keyword + ":" + i);

            }

            Debug.DrawLine(origin_center, target, Color.green);

#endif
        }


        public void IsVisibleTile(Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {
#if UNITY_EDITOR
            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval)
            {
                return;
            }

            target_3d = SingleO.gridManager.ToCenter3D_FromPosition3D(target_3d);

            //대상타일이 구조덮개타일이면 볼수없다. 바로 끝 
            //if (true == this.HasStructUpTile(target)) return false;

            Vector3Int origin_2d = SingleO.gridManager.ToPosition2D(origin_3d);
            Vector3 origin_3d_center = SingleO.gridManager.ToPosition3D_Center(origin_2d);

            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            CellSpace structTile = null;
            if (SingleO.gridManager.HasStructTile(origin_3d, out structTile))
            {
                switch (structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //모서리 값으로 설정 
                            Vector3Int dir = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_3d_center.x += dir.x * SingleO.gridManager._cellSize_x * 0.5f;
                            origin_3d_center.z += dir.y * SingleO.gridManager._cellSize_z * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            origin_3d_center = SingleO.gridManager.ToPosition3D_Center(origin_2d + Misc.GetDir8_Normal2D(structTile._eDir));
                        }
                        break;
                }

                DebugWide.PrintText(origin_3d_center, Color.white, "__before:" + SingleO.gridManager.ToPosition3D_Center(origin_2d) + " after:" + origin_3d_center);
                //UnityEditor.Handles.Label(origin_3d_center, "__before:" + SingleO.gridManager.ToPosition3D_Center(origin_2d) + " after:" + origin_3d_center);
            }


            Vector3 line = target_3d - origin_3d_center;
            Vector3 n = line.normalized;

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            Vector3 next = n * length_interval * count;
            while (line.sqrMagnitude > next.sqrMagnitude)
            {
                DebugWide.PrintText(origin_3d_center + next, Color.white, "__" + count);
                //UnityEditor.Handles.Label(origin_3d_center + next, "__" + count);

                if (SingleO.gridManager.HasStructTile(origin_3d_center + next, out structTile))
                //if (this.HasStructUpTile(origin_center + next))
                {
                    if (true == structTile._isUpTile)
                    {

                        //return false;

                        if (GridManager._ReturnMessage_NotIncluded_InDiagonalArea != SingleO.gridManager.IsIncluded_InDiagonalArea(origin_3d_center + next))
                        {
                            //return false;
                            break;
                        }
                    }

                }

                count++;
                next = n * length_interval * count;

            }

            DebugWide.PrintText(origin_3d_center + next, Color.white, "_________end :" + count);
            //UnityEditor.Handles.Label(origin_3d_center + next, "_________end :" + count);

            Debug.DrawLine(origin_3d_center, target_3d, Color.green);
            //return true;
#endif
        }


        public void Draw_LookAtChamp()
        {
            foreach (Being bbb in SingleO.objectManager._linearSearch_list)
            {
                ChampUnit unit = bbb as ChampUnit;
                if (null != unit)
                {
                    if (unit.isDeath()) continue;

                    Color cc = Color.red;
                    if (Camp.eKind.Blue == unit._campKind) cc = Color.blue;
                    if (Camp.eKind.White == unit._campKind) cc = Color.white;

                    //chamto test - 
                    if (1 == unit._id)
                    {
                        unit._hp_max = 10000;
                        unit._hp_cur = 10000;
                        DebugWide.DrawCircle(unit.transform.position, 5f, cc); //탐지사정거리 
                    }


                    if (null != unit._looking)
                    {
                        if (unit._looking.isDeath()) continue;
                        Debug.DrawLine(unit.transform.position, unit._looking.transform.position, cc);
                        DebugWide.DrawCircle(unit.transform.position, 0.3f, cc);
                    }

                }
            }
        }

        public bool _draw_Q = false;
        public bool _draw_line = false;
        public bool _draw_line_addInfo = false;
        public bool _draw_DirNormal = false;
        public bool _draw_IndexesNxN = false;
        public bool _draw_StructTile = false;
        public bool _draw_FogOfWar = false;
        public bool _draw_Path_Edges = false;
        public bool _draw_LookAt_Champ = false;
        public bool _draw_CollisionSphere = false;
        public bool _draw_AABBCulling = false;
        public bool _draw_CellPartition = false;
        public bool _draw_SphTree_Being = false;
        public bool _draw_SphTree_Being_RayTrace = false;
        public bool _draw_SphTree_Being_Range = false;
        public bool _draw_SphTree_Struct = false;
        public bool _draw_SphTree_Struct_RayTrace = false;

        void OnDrawGizmos()
        {
#if UNITY_EDITOR

            if(_draw_Q)
            {
                while (0 != _drawQ.Count)
                {
                    DrawInfo info = _drawQ.Dequeue();
                    info.Draw();
                }    
            }



            if(_draw_line)
            {
                DebugWide.DrawLine(_origin.transform.position, _target.transform.position, Color.red);
            }
            if(_draw_line_addInfo)
            {
                Draw_Grid(); //제거대상
                Update_IsVisible_SightOfView(_origin.position, _target.position , _length_interval);
                IsVisibleTile(_origin.position, _target.position, _length_interval);
            }
            if(_draw_DirNormal)
            {
                Misc.DrawDirN();    
            }
            if(_draw_IndexesNxN)
            {
                DebugWide.PrintText(ConstV.v3_zero, Color.green,"0,0");
                UpdateDraw_IndexesNxN();   
            }
            if(_draw_StructTile)
            {
                //UpdateDraw_StructTileDir();    
                SingleO.gridManager.DrawLine_StructTile();
                //SingleO.gridManager.Draw_BoundaryTile();
            }
            if(_draw_FogOfWar)
            {
                UpdateDraw_FogOfWar_DivisionNum(); //??    
            }
            if(_draw_Path_Edges)
            {
                Update_DrawEdges(false);    
            }
            if(_draw_LookAt_Champ)
            {
                Draw_LookAtChamp();    
            }
            if(_draw_CollisionSphere)
            {
                SingleO.objectManager.Draw_CollisionSphere();
            }
            if(_draw_AABBCulling)
            {
                SingleO.objectManager.Draw_AABBCulling();    
            }
            if(_draw_CellPartition)
            {
                SingleO.cellPartition.Draw_CellInfo();    
            }

            //구트리 테스트 
            if(_draw_SphTree_Being)
            {
                //SingleO.objectManager.GetSphereTree_Being().Render_Debug(0, false); 

                SingleO.objectManager.GetSphereTree_Being().Render_Debug(false); 
            }
            if (_draw_SphTree_Being_RayTrace)
            {
                SingleO.objectManager.GetSphereTree_Being().Render_RayTrace(_origin.position, _target.position);
            }
            if (_draw_SphTree_Being_Range)
            {
                float radius = (_origin.position - _target.position).magnitude;
                SingleO.objectManager.GetSphereTree_Being().Render_RangeTest(_origin.position, radius);
            }

            if(_draw_SphTree_Struct)
            {
                //SingleO.objectManager.GetSphereTree_Struct().Render_Debug(0, false); 
                //SingleO.objectManager.GetSphereTree_Struct().Render_Debug(1, false); 
                SingleO.objectManager.GetSphereTree_Struct().Render_Debug(false); 
            }
            if (_draw_SphTree_Struct_RayTrace)
            {
                SingleO.objectManager.GetSphereTree_Struct().Render_RayTrace(_origin.position, _target.position);
            }

            #endif
        }//end func

    }//end class
//}
