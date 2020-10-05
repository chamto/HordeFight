using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//using UnityEngine.Assertions;

using UtilGS9;



//========================================================
//==================     디버그      ==================
//========================================================
namespace HordeFight
{
    public class DebugViewer : MonoBehaviour
    {
        public float _length_interval = 0.1f;
        public Transform _origin = null;
        public Transform _target = null;

        private Tilemap _tilemap = null;
        private Dictionary<Vector3Int, Color> _colorMap = new Dictionary<Vector3Int, Color>();

        private void Start()
        {
            GameObject game = GameObject.Find("Tilemap_Debug");
            if (null != game)
            {
                _tilemap = game.GetComponent<Tilemap>();
            }

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


        private Vector3 _start = ConstV.v3_zero;
        private Vector3 _end = ConstV.v3_right;
        public void DrawLine(Vector3 start, Vector3 end)
        {
            _start = start;
            _end = end;
        }

        public void UpdateDraw_IndexesNxN()
        {
#if UNITY_EDITOR
            Index2 prev = ConstV.id2_zero;
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_RhombusCenter(7, ConstV.v3_up))
            foreach (Index2 cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter_Tornado(11, 11))
            //foreach (Vector3Int cur in SingleO.gridManager.CreateIndexesNxN_SquareCenter(7, ConstV.v3_up))
            {
                //DebugWide.LogBlue(v);
                //Debug.DrawLine(cur - Vector3.one * 0.3f, cur + Vector3.one * 0.3f, Color.red);
                //Debug.DrawLine(prev, cur);

                //UnityEditor.Handles.Label(cur, "( " + cur.x + " , " + cur.z + " )");

                prev = cur;

            }
#endif
        }

        public void UpdateDraw_FogOfWar_DivisionNum()
        {
#if UNITY_EDITOR
            BoundsInt bounds = SingleO.gridManager.GetTileMap_FogOfWar().cellBounds;
            RuleExtraTile rule = null;
            byte divisionNum = 0;
            Vector3 pos3d = ConstV.v3_zero;
            foreach (Vector3Int xy in bounds.allPositionsWithin)
            {
                rule = SingleO.gridManager.GetTileMap_FogOfWar().GetTile<RuleExtraTile>(xy);
                if (null != rule)
                {
                    pos3d = SingleO.gridManager.ToPosition3D(xy);
                    pos3d.x += 0.08f;
                    pos3d.z += 0.16f;
                    divisionNum = rule._tileDataMap.Get_DivisionNum(xy);
                    if (0 < divisionNum)
                        UnityEditor.Handles.color = Color.red;
                    else
                        UnityEditor.Handles.color = Color.white;

                    UnityEditor.Handles.Label(pos3d, "" + divisionNum);
                }


            }
#endif
        }

        public void UpdateDraw_StructTileDir()
        {
            Color ccc = Color.white;
            foreach (CellSpace t in SingleO.gridManager._structTileList.Values)
            {
                if (eDirection8.none == t._eDir) continue;

                //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
                //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z

                ccc = Color.white;
                if (CellSpace.Specifier_DiagonalFixing == t._specifier)
                {
                    ccc = Color.red;
                }

                Vector3 end = t._pos3d_center + Misc.GetDir8_Normal3D_AxisY(t._eDir) * 0.12f;
                Debug.DrawLine(t._pos3d_center, end, ccc);


                //UnityEditor.Handles.Label(t._center_3d, "( " + cur.x + " , " + cur.z + " )");

                //Vector3 crossL = t._v3Center;
                //crossL.x += -0.08f;
                //crossL.z += 0.08f;
                //Vector3 crossR = t._v3Center;
                //crossR.x += 0.08f;
                //crossR.z += -0.08f;
                //Debug.DrawLine(crossL, crossR, ccc);
            }
        }

        public void Update_DrawEdges(bool debugLog)
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
                foreach (GraphEdge e in list)
                {
                    to_XY_2d = grid.ToPosition2D(e.To(), boundsInt.size.x);
                    to_3d = grid.ToPosition3D(to_XY_2d);

                    from_nav = finder._graph.GetNode(e.From()) as NavGraphNode;
                    to_nav = finder._graph.GetNode(e.To()) as NavGraphNode;
                    Debug.DrawLine(from_nav.Pos(), to_nav.Pos(), Color.green);

                    //chamto test
                    if (true == debugLog)
                    {
                        DebugWide.LogBlue(e + "  " + from_XY_2d + "   " + to_XY_2d);
                    }

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
                    UnityEditor.Handles.Label(origin_center + next, keyword + ":" + i);
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
                UnityEditor.Handles.Label(origin_center + next, keyword + ":" + i);

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

                UnityEditor.Handles.Label(origin_3d_center, "__before:" + SingleO.gridManager.ToPosition3D_Center(origin_2d) + " after:" + origin_3d_center);
            }


            Vector3 line = target_3d - origin_3d_center;
            Vector3 n = line.normalized;

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            Vector3 next = n * length_interval * count;
            while (line.sqrMagnitude > next.sqrMagnitude)
            {
                UnityEditor.Handles.Label(origin_3d_center + next, "__" + count);

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

            UnityEditor.Handles.Label(origin_3d_center + next, "_________end :" + count);

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

        void OnDrawGizmos()
        {
            //Misc.DrawDirN();
            //UnityEditor.Handles.Label(ConstV.v3_zero, "0,0");
            //UpdateDraw_IndexesNxN();

            //Debug.DrawLine(_start, _end);

            //UpdateDraw_StructTileDir();

            //UpdateDraw_FogOfWar_DivisionNum();

            //Update_DrawEdges(false);


            //if(null != _origin && null != _target)
            //{
            //    Draw_Grid();
            //    Update_IsVisible_SightOfView(_origin.position, _target.position , _length_interval);
            //    IsVisibleTile(_origin.position, _target.position, _length_interval);
            //}


            //==============================================
            //구트리 테스트 
            //SingleO.objectManager.GetSphereTree_Being().Render_Debug(3, false); 
            //SingleO.objectManager.GetSphereTree_Being().Render_RayTrace(_origin.position, _target.position);
            //SingleO.objectManager.GetSphereTree_Struct().Render_RayTrace(_origin.position, _target.position);
            //float radius = (_origin.position - _target.position).magnitude;
            //SingleO.objectManager.GetSphereTree().Render_RangeTest(_origin.position,radius);
            //==============================================

            //Draw_LookAtChamp();

            // SingleO.cellPartition.DebugPrint();
        }


    }
}
