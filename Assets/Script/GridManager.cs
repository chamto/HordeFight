using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//using UnityEngine.Assertions;

using UtilGS9;


//========================================================
//==================     그리드 관리기     ==================
//========================================================
namespace HordeFight
{
    public class GridManager : MonoBehaviour
    {
        public const int NxN_MIN = 3;   //그리드 범위 최소크기
        public const int NxN_MAX = 11;  //그리드 범위 최대크기
        //public const float GridCell_Size = 0.16f;
        //public const float GridCell_HalfSize = GridCell_Size * 0.5f;

        public const float ONE_METER = 1f; //타일 한개의 가로길이 
        public const float SQR_ONE_METER = ONE_METER * ONE_METER;
        public const float WorldToMeter = 1f / ONE_METER;
        public const float MeterToWorld = ONE_METER;


        private Grid _grid = null;
        private Tilemap _tilemap_struct = null;
        private Tilemap _tilemap_structUp = null;
        private Tilemap _tilemap_fogOfWar = null;
        public Dictionary<Vector3Int, CellInfo> _cellInfoList = new Dictionary<Vector3Int, CellInfo>(new Vector3IntComparer());
        public Dictionary<Vector3Int, CellSpace> _structTileList = new Dictionary<Vector3Int, CellSpace>(new Vector3IntComparer());

        //public CellSpacePartition _cellSpacePartition = new CellSpacePartition();

        //중심이 (0,0)인 nxn 그리드 인덱스 값을 미리 구해놓는다
        public Dictionary<uint, Index2[]> _indexesNxN = new Dictionary<uint, Index2[]>();


        public Grid grid
        {
            get { return _grid; }
        }
        public float _cellSize_x = 1f;
        //{
        //    get
        //    {
        //        return (_grid.cellSize.x * _grid.transform.localScale.x);
        //    }
        //}
        public float _cellSize_z = 1f;
        //{
        //    get
        //    {
        //        return (_grid.cellSize.y * _grid.transform.localScale.z);
        //    }
        //}

        public Tilemap GetTileMap_Struct()
        {
            return _tilemap_struct;
        }

        public Tilemap GetTileMap_StructUp()
        {
            return _tilemap_structUp;
        }

        public Tilemap GetTileMap_FogOfWar()
        {
            return _tilemap_fogOfWar;
        }

        //private void Start()
        public void Init()
        {

            _grid = GameObject.Find("0_grid").GetComponent<Grid>();
            GameObject o = GameObject.Find("Tilemap_Structure");
            if (null != o)
            {
                _tilemap_struct = o.GetComponent<Tilemap>();
            }
            o = GameObject.Find("Tilemap_StructureUp");
            if (null != o)
            {
                _tilemap_structUp = o.GetComponent<Tilemap>();
            }
            o = GameObject.Find("Tilemap_FogOfWar");
            if (null != o)
            {
                _tilemap_fogOfWar = o.GetComponent<Tilemap>();
            }

            _cellSize_x = _grid.cellSize.x * _grid.transform.localScale.x;
            _cellSize_z = _grid.cellSize.y * _grid.transform.localScale.z;

            //타일맵 좌표계 x-y에 해당하는 축값 back로 구한다 
            _indexesNxN[3] = CreateIndexesNxN_SquareCenter_Tornado(3, 3);
            _indexesNxN[5] = CreateIndexesNxN_SquareCenter_Tornado(5, 5);
            _indexesNxN[7] = CreateIndexesNxN_SquareCenter_Tornado(7, 7);
            _indexesNxN[9] = CreateIndexesNxN_SquareCenter_Tornado(9, 9);
            _indexesNxN[11] = CreateIndexesNxN_SquareCenter_Tornado(11, 11); //화면 세로길이를 벗어나지 않는 그리드 최소값
            _indexesNxN[29] = CreateIndexesNxN_SquareCenter_Tornado(29, 19);

            this.LoadTilemap_Struct();

            SingleO.cellPartition.Init();
            //SingleO.cellPartition.Init(new Index2(64, 64));
            //_cellSpacePartition.Init(new Vector2Int(64, 64)); //chamto test
        }


        //ref : https://gamedev.stackexchange.com/questions/150917/how-to-get-all-tiles-from-a-tilemap
        public void LoadTilemap_Struct()
        {
            if (null == _tilemap_struct) return;

            SingleO.gridManager.GetTileMap_Struct().RefreshAllTiles();
            CellSpace structTile = null;
            RuleExtraTile.TilingRule ruleInfo = null;
            int specifier = 0;
            foreach (Vector3Int XY_2d in _tilemap_struct.cellBounds.allPositionsWithin)
            {
                RuleExtraTile ruleTile = _tilemap_struct.GetTile(XY_2d) as RuleExtraTile; //룰타일 종류와 상관없이 다 가져온다. 
                if (null == ruleTile) continue;

                ruleInfo = ruleTile._tileDataMap.GetTilingRule(XY_2d);
                if (null == ruleInfo || false == int.TryParse(ruleInfo.m_specifier, out specifier))
                    specifier = 0;


                structTile = new CellSpace();
                structTile._specifier = specifier;
                structTile._pos3d_center = this.ToPosition3D_Center(XY_2d);
                structTile._pos2d = new Index2(XY_2d.x, XY_2d.y);
                structTile._pos1d = this.ToPosition1D(XY_2d, 64); //임시코드
                structTile._eDir = ruleTile._tileDataMap.GetDirection8(XY_2d);
                //structTile._v3Dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir); //방향값을 미리 구해 놓는다 
                //structTile._i2Dir = Misc.GetDir8_Normal2D(structTile._eDir);
                structTile._isUpTile = ruleTile._tileDataMap.Get_IsUpTile(XY_2d);
                structTile._isStructTile = true;
                _structTileList.Add(XY_2d, structTile);

            }

            DebugWide.LogBlue("LoadTile : " + _structTileList.Count + "  -  TileMap_Struct RefreshAllTiles");

            //덮개 타일 생성
            TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            //TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            foreach (KeyValuePair<Vector3Int, CellSpace> t in _structTileList)
            {
                if (true == t.Value._isUpTile)
                {
                    _tilemap_structUp.SetTile(t.Key, tileScript);
                }
            }

            DebugWide.LogBlue("덮개타일 생성 완료 : " + tileScript.name);

            //확장영역에 구조물 경계선 추가 
            Load_StructLine();
        }


        public struct BoundaryTile
        {
            //public Vector3 standard_pos;
            //public Vector3 push_dir;
            public LineSegment3 line;
            public bool isBoundary;
            public CellSpace cell;

            public void Init()
            {
                //standard_pos = Vector3.zero;
                //push_dir = Vector3.zero;
                //line = new LineSegment3();
                isBoundary = true;
                cell = null;
            }
        }
        public class BoundaryTileList : LinkedList<BoundaryTile>
        { }
        public Dictionary<Vector3Int, BoundaryTileList> _boundaryList = new Dictionary<Vector3Int, BoundaryTileList>(new Vector3IntComparer());

        public void Load_StructLine()
        {
            foreach (KeyValuePair<Vector3Int, CellSpace> t in _structTileList)
            {
                //덮개타일은 걸러낸다 
                if(eDirection8.none != t.Value._eDir)
                {
                    BoundaryTile info = new BoundaryTile();
                    info.Init();
                    info.cell = t.Value;

                    LineSegment3 line;
                    CalcBoundaryLine(t.Value, out line);
                    info.line = line;


                    Vector3Int dir1 = Misc.GetDir8_Normal2D(t.Value._eDir);
                    Vector3Int dir2 = dir1, dir3 = dir1;
                    Vector3Int key;
                    bool diag = false; //대각 


                    switch (t.Value._eDir)
                    {
                        case eDirection8.leftUp:
                            if (CellSpace.Specifier_DiagonalFixing == t.Value._specifier) break;
                            dir2 = Misc.GetDir8_Normal2D(eDirection8.left);
                            dir3 = Misc.GetDir8_Normal2D(eDirection8.up);
                            diag = true;
                            break;
                        case eDirection8.leftDown:
                            if (CellSpace.Specifier_DiagonalFixing == t.Value._specifier) break;
                            dir2 = Misc.GetDir8_Normal2D(eDirection8.left);
                            dir3 = Misc.GetDir8_Normal2D(eDirection8.down);
                            diag = true;
                            break;
                        case eDirection8.rightUp:
                            if (CellSpace.Specifier_DiagonalFixing == t.Value._specifier) break;
                            dir2 = Misc.GetDir8_Normal2D(eDirection8.right);
                            dir3 = Misc.GetDir8_Normal2D(eDirection8.up);
                            diag = true;
                            break;
                        case eDirection8.rightDown:
                            if (CellSpace.Specifier_DiagonalFixing == t.Value._specifier) break;
                            dir2 = Misc.GetDir8_Normal2D(eDirection8.right);
                            dir3 = Misc.GetDir8_Normal2D(eDirection8.down);
                            diag = true;
                            break;
                        
                    }

                    BoundaryTileList list = null;

                    key = t.Key ;
                    _boundaryList.TryGetValue(key, out list);
                    if (null == list)
                    {
                        list = new BoundaryTileList();
                        _boundaryList.Add(key, list);
                    }
                    info.isBoundary = false;
                    list.AddLast(info);

                    //대각고정은 자기자신만 추가한다 
                    if (CellSpace.Specifier_DiagonalFixing == t.Value._specifier) continue;

                    //한칸 이동한 방향에 추가  
                    key = t.Key + dir1;
                    _boundaryList.TryGetValue(key, out list);
                    if (null == list)
                    {
                        list = new BoundaryTileList();
                        _boundaryList.Add(key, list);
                    }
                    info.isBoundary = true;
                    list.AddLast(info);

                    //대각인 경우 방향을 수평,수직으로 분리해서 추가 
                    if(true == diag)
                    {
                        key = t.Key + dir2;
                        _boundaryList.TryGetValue(key, out list);
                        if (null == list)
                        {
                            list = new BoundaryTileList();
                            _boundaryList.Add(key, list);
                        }
                        info.isBoundary = true;
                        list.AddLast(info);

                        key = t.Key + dir3;
                        _boundaryList.TryGetValue(key, out list);
                        if (null == list)
                        {
                            list = new BoundaryTileList();
                            _boundaryList.Add(key, list);
                        }
                        info.isBoundary = true;
                        list.AddLast(info);
                    }
                }

            }
        }


        public bool CalcBoundaryLine(CellSpace cell , out LineSegment3 line)
        {
            line = new LineSegment3();
            if (null == cell) return false;
            if (eDirection8.none == cell._eDir) return false;
            if (CellSpace.Specifier_DiagonalFixing == cell._specifier) return false;

            //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
            //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z
            //==========================================

            float size = SingleO.gridManager._cellSize_x * 0.5f;

            //Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(cell._eDir);
            //info.standard_pos = cell._pos3d_center - info.push_dir * size;


            Vector3 temp = cell._pos3d_center;
            switch (cell._eDir)
            {
                case eDirection8.up:
                    {
                        temp.z = temp.z + size;
                        temp.x = cell._pos3d_center.x - size;
                        line.origin = temp;

                        temp.x = cell._pos3d_center.x + size;
                        line.last = temp;

                    }
                    break;
                case eDirection8.down:
                    {
                        temp.z = temp.z - size;
                        temp.x = cell._pos3d_center.x - size;
                        line.origin = temp;

                        temp.x = cell._pos3d_center.x + size;
                        line.last = temp;

                    }
                    break;
                case eDirection8.left:
                    {
                        temp.x = temp.x - size;
                        temp.z = cell._pos3d_center.z + size;
                        line.origin = temp;

                        temp.z = cell._pos3d_center.z - size;
                        line.last = temp;

                    }
                    break;
                case eDirection8.right:
                    {
                        temp.x = temp.x + size;
                        temp.z = cell._pos3d_center.z + size;
                        line.origin = temp;

                        temp.z = cell._pos3d_center.z - size;
                        line.last = temp;

                    }
                    break;
                case eDirection8.leftUp:
                    {
                        if (CellSpace.Specifier_DiagonalFixing == cell._specifier)
                        {
                            return false;
                        }

                        temp = cell._pos3d_center;
                        temp.x -= size;
                        temp.z -= size;
                        line.origin = temp;

                        temp = cell._pos3d_center;
                        temp.x += size;
                        temp.z += size;
                        line.last = temp;

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        if (CellSpace.Specifier_DiagonalFixing == cell._specifier)
                        {
                            return false;
                        }

                        temp = cell._pos3d_center;
                        temp.x -= size;
                        temp.z += size;
                        line.origin = temp;

                        temp = cell._pos3d_center;
                        temp.x += size;
                        temp.z -= size;
                        line.last = temp;


                    }
                    break;
                case eDirection8.leftDown:
                    {
                        if (CellSpace.Specifier_DiagonalFixing == cell._specifier)
                        {
                            return false;
                        }

                        temp = cell._pos3d_center;
                        temp.x -= size;
                        temp.z += size;
                        line.origin = temp;

                        temp = cell._pos3d_center;
                        temp.x += size;
                        temp.z -= size;
                        line.last = temp;

                    }
                    break;
                case eDirection8.rightDown:
                    {
                        if (CellSpace.Specifier_DiagonalFixing == cell._specifier)
                        {
                            return false;
                        }

                        temp = cell._pos3d_center;
                        temp.x -= size;
                        temp.z -= size;
                        line.origin = temp;

                        temp = cell._pos3d_center;
                        temp.x += size;
                        temp.z += size;
                        line.last = temp;

                    }
                    break;

            }//end switch

            return true;
        }

        public void Draw_BoundaryTile()
        {
            foreach (KeyValuePair<Vector3Int , BoundaryTileList> info1 in _boundaryList)
            {
                Vector3 pos = SingleO.cellPartition.ToPosition3D_Center(new Index2(info1.Key.x, info1.Key.y));
                DebugWide.DrawCircle(pos, 0.3f, Color.gray);
                DebugWide.PrintText(pos, Color.black, "" + info1.Value.Count);

                foreach(BoundaryTile info2 in info1.Value)
                {
                    DebugWide.DrawLine(info2.cell._pos3d_center, pos, Color.gray);
                }

            }
        }

        public void DrawLine_StructTile()
        {
            //DebugWide.LogBlue(SingleO.gridManager._structTileList.Count);
            foreach (CellSpace t in SingleO.gridManager._structTileList.Values)
            {
                if (eDirection8.none == t._eDir) continue;

                //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
                //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z
                //==========================================

                float size = SingleO.gridManager._cellSize_x * 0.5f;
                Vector3 temp = t._pos3d_center;
                LineSegment3 line3 = new LineSegment3();
                switch (t._eDir)
                {
                    case eDirection8.up:
                        {
                            temp.z = temp.z + size;
                            temp.x = t._pos3d_center.x - size;
                            line3.origin = temp;

                            temp.x = t._pos3d_center.x + size;
                            line3.last = temp;
                        }
                        break;
                    case eDirection8.down:
                        {
                            temp.z = temp.z - size;
                            temp.x = t._pos3d_center.x - size;
                            line3.origin = temp;

                            temp.x = t._pos3d_center.x + size;
                            line3.last = temp;

                        }
                        break;
                    case eDirection8.left:
                        {
                            temp.x = temp.x - size;
                            temp.z = t._pos3d_center.z + size;
                            line3.origin = temp;

                            temp.z = t._pos3d_center.z - size;
                            line3.last = temp;

                        }
                        break;
                    case eDirection8.right:
                        {
                            temp.x = temp.x + size;
                            temp.z = t._pos3d_center.z + size;
                            line3.origin = temp;

                            temp.z = t._pos3d_center.z - size;
                            line3.last = temp;

                        }
                        break;
                    case eDirection8.leftUp:
                        {
                            if (CellSpace.Specifier_DiagonalFixing == t._specifier)
                            {
                                temp.x -= size;
                                temp.z += size;
                                break;
                            }

                            temp = t._pos3d_center;
                            temp.x -= size;
                            temp.z -= size;
                            line3.origin = temp;

                            temp = t._pos3d_center;
                            temp.x += size;
                            temp.z += size;
                            line3.last = temp;

                        }
                        break;
                    case eDirection8.rightUp:
                        {
                            if (CellSpace.Specifier_DiagonalFixing == t._specifier)
                            {
                                temp.x += size;
                                temp.z += size;
                                break;
                            }

                            temp = t._pos3d_center;
                            temp.x -= size;
                            temp.z += size;
                            line3.origin = temp;

                            temp = t._pos3d_center;
                            temp.x += size;
                            temp.z -= size;
                            line3.last = temp;


                        }
                        break;
                    case eDirection8.leftDown:
                        {
                            if (CellSpace.Specifier_DiagonalFixing == t._specifier)
                            {
                                temp.x -= size;
                                temp.z -= size;
                                break;
                            }

                            temp = t._pos3d_center;
                            temp.x -= size;
                            temp.z += size;
                            line3.origin = temp;

                            temp = t._pos3d_center;
                            temp.x += size;
                            temp.z -= size;
                            line3.last = temp;

                        }
                        break;
                    case eDirection8.rightDown:
                        {
                            if (CellSpace.Specifier_DiagonalFixing == t._specifier)
                            {
                                temp.x += size;
                                temp.z -= size;
                                break;
                            }

                            temp = t._pos3d_center;
                            temp.x -= size;
                            temp.z -= size;
                            line3.origin = temp;

                            temp = t._pos3d_center;
                            temp.x += size;
                            temp.z += size;
                            line3.last = temp;

                        }
                        break;

                }//end switch

                if (CellSpace.Specifier_DiagonalFixing != t._specifier)
                {
                    line3.Draw(Color.white);
                }else
                {
                    DebugWide.DrawCircle(temp, 0.2f, Color.red);
                }

            }
        }


        public Vector3 GetBorder_StructTile(Vector3 srcPos, float radius, CellSpace structTile)
        {
            if (null == structTile) return srcPos;

            Vector3 centerToSrc_dir = (srcPos - structTile._pos3d_center);
            Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);


            float size = SingleO.gridManager._cellSize_x * 0.5f;
            Vector3 temp = ConstV.v3_zero;
            LineSegment3 line3 = new LineSegment3();
            //8방향별 축값 고정  
            switch (structTile._eDir)
            {
                case eDirection8.up:
                    {
                        srcPos.z = structTile._pos3d_center.z + size;
                        return srcPos + push_dir * radius;
                    }
                    //break;
                case eDirection8.down:
                    {
                        srcPos.z = structTile._pos3d_center.z - size;
                        return srcPos + push_dir * radius;
                    }
                    //break;
                case eDirection8.left:
                    {
                        srcPos.x = structTile._pos3d_center.x - size;
                        return srcPos + push_dir * radius;
                    }
                    //break;
                case eDirection8.right:
                    {
                        srcPos.x = structTile._pos3d_center.x + size;
                        return srcPos + push_dir * radius;
                    }
                    //break;
                case eDirection8.leftUp:
                    {
                        //down , right
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x - size;
                            srcPos.z = structTile._pos3d_center.z + size;
                            return srcPos + push_dir * radius;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        //if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z -= size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z += size;
                        line3.last = temp;

                        if (true == Geo.IntersectLineSegment(srcPos, radius, line3.origin, line3.last))
                        {
                            srcPos = line3.ClosestPoint(srcPos);
                            return srcPos + push_dir * radius;
                        }

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        //down , left
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x + size;
                            srcPos.z = structTile._pos3d_center.z + size;
                            return srcPos + push_dir * radius;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        //if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z += size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z -= size;
                        line3.last = temp;

                        if (true == Geo.IntersectLineSegment(srcPos, radius, line3.origin, line3.last))
                        {
                            srcPos = line3.ClosestPoint(srcPos);
                            return srcPos + push_dir * radius;
                        }
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        //up , right
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x - size;
                            srcPos.z = structTile._pos3d_center.z - size;
                            return srcPos + push_dir * radius;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        //if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z += size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z -= size;
                        line3.last = temp;

                        if (true == Geo.IntersectLineSegment(srcPos, radius, line3.origin, line3.last))
                        {
                            srcPos = line3.ClosestPoint(srcPos);
                            return srcPos + push_dir * radius;
                        }

                    }
                    break;
                case eDirection8.rightDown:
                    {
                        //up , left
                        if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                        {
                            srcPos.x = structTile._pos3d_center.x + size;
                            srcPos.z = structTile._pos3d_center.z - size;
                            return srcPos + push_dir * radius;
                        }

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        //if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z -= size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z += size;
                        line3.last = temp;

                        if (true == Geo.IntersectLineSegment(srcPos, radius, line3.origin, line3.last))
                        {
                            srcPos = line3.ClosestPoint(srcPos);
                            return srcPos + push_dir * radius;
                        }
                    }
                    break;

            }

            return srcPos;

        }

        public Vector3 Collision_StructLine(Vector3 srcPos , float RADIUS)
        {

            Index2 temp_2d = SingleO.cellPartition.ToPosition2D(srcPos);
            Vector3Int pos_2d = new Vector3Int(temp_2d.x, temp_2d.y, 0); 

            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list)) return srcPos;


            //RADIUS = 1.0f;
            foreach (BoundaryTile info in list)
            {
                //주변경계타일인 경우
                if (true == info.isBoundary)
                {

                    if (true == Geo.IntersectLineSegment(srcPos, RADIUS, info.line.origin, info.line.last))
                    {

                        Vector3 cp = info.line.ClosestPoint(srcPos);
                        Vector3 n = VOp.Normalize(srcPos - cp);
                        srcPos = cp + n * RADIUS;

                    }
                }
                //경계타일인 경우 
                else
                {
                    srcPos = GetBorder_StructTile(srcPos, RADIUS, info.cell);
                }


            }

            return srcPos;


        }

        public bool old_IsVisibleTile(Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {
            return true; //test

            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval)
            {
                return false;
            }

            //if(ToPosition2D(origin_3d) == new Vector3Int(22,13,0) && 
            //   ToPosition2D(target_3d) == new Vector3Int(22,9,0))
            //{
            //    DebugWide.LogBlue("오동작");

            //}
            //Vector3Int test = Vector3Int.zero;
            //test = _tilemap_struct.WorldToCell(origin_3d);
            //test = ToPosition2D(origin_3d);

            //대상타일이 구조덮개타일이면 볼수없다. 바로 끝 
            //if (true == this.HasStructUpTile(target)) return false;

            Vector3Int origin_2d = ToPosition2D(origin_3d);
            Vector3 origin_3d_center = ToPosition3D_Center(origin_2d);


            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            CellSpace structTile = null;
            if (this.HasStructTile(origin_3d, out structTile))
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
                            origin_3d_center.x += dir.x * _cellSize_x * 0.5f;
                            origin_3d_center.z += dir.y * _cellSize_z * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            origin_3d_center = ToPosition3D_Center(origin_2d + Misc.GetDir8_Normal2D(structTile._eDir));
                        }
                        break;
                }

            }

            Vector3 line = target_3d - origin_3d_center;
            Vector3 n = line.normalized;

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            Vector3 next = n * length_interval * count;
            while (line.sqrMagnitude > next.sqrMagnitude)
            {

                if (this.HasStructTile(origin_3d_center + next, out structTile))
                //if (this.HasStructUpTile(origin_center + next))
                {
                    if (true == structTile._isUpTile)
                    {

                        //return false;

                        if (GridManager._ReturnMessage_NotIncluded_InDiagonalArea != this.IsIncluded_InDiagonalArea(origin_3d_center + next))
                        {
                            return false;
                        }
                    }

                }

                count++;
                next = n * length_interval * count;

            }

            return true;
        }

        public void SetTile(Tilemap tilemap, Vector3Int xy_2d, TileBase setTile)
        {
            TileBase getTile = tilemap.GetTile(xy_2d);
            if (getTile != setTile)
            {
                tilemap.SetTile(xy_2d, setTile);
            }
        }


        public void Update_FogOfWar(Vector3 standard_3d, Vector3 lookAt_dir)
        {
            if (null == _tilemap_fogOfWar) return;

            //====================================================
            StartCoroutine(DivideUpdate_FogOfWar(standard_3d, lookAt_dir));
            return;
            //====================================================

            //Color baseColor = Color.white;
            //Color fogColor = new Color(1, 1, 1, 0.7f);
            TileBase tileScript = SingleO.resourceManager.GetTileScript("fow_RuleExtraTile".GetHashCode());
            TileBase tileScript2 = SingleO.resourceManager.GetTileScript("ocean".GetHashCode());
            TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            TileBase tileScript4 = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            Vector3Int posXY_2d = this.ToPosition2D(standard_3d);
            Vector3 tile_3d_center = ConstV.v3_zero;
            //Vector3 standard_3d_center = this.ToCenter3D_FromPosition3D(standard_3d);

            BoundsInt bounds = new BoundsInt(posXY_2d - new Vector3Int(14, 11, 0), new Vector3Int(28, 21, 1));

            foreach (Vector3Int tile_2d in bounds.allPositionsWithin)
            {
                //_tilemap_fogOfWar.SetTileFlags(xy, TileFlags.None);
                tile_3d_center = this.ToPosition3D_Center(tile_2d);

                RuleExtraTile ruleTile = _tilemap_fogOfWar.GetTile(tile_2d) as RuleExtraTile;
                float sqrDis = (tile_3d_center - standard_3d).sqrMagnitude;
                if (sqrDis <= Mathf.Pow(GridManager.MeterToWorld * 6.2f, 2))
                {
                    if (true == old_IsVisibleTile(standard_3d, tile_3d_center, 0.1f))
                    {

                        //대상과 정반대 방향이 아닐때 처리 
                        Vector3 tileDir = tile_3d_center - standard_3d;
                        tileDir.Normalize(); lookAt_dir.Normalize();
                        //tileDir = Misc.GetDir8_Normal3D(tileDir);
                        //target_dir = Misc.GetDir8_Normal3D(target_dir);


                        if (true == this.HasStructUpTile(tile_3d_center))
                        {
                            //구조덮개타일인 경우 
                            SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                        }
                        else
                            if (Mathf.Cos(Mathf.Deg2Rad * 40f) < Vector3.Dot(tileDir, lookAt_dir))
                        {
                            //원거리 시야 표현
                            SetTile(_tilemap_fogOfWar, tile_2d, null);
                        }
                        else if (sqrDis <= Mathf.Pow(GridManager.MeterToWorld * 1.2f, 2))
                        {
                            //주변 시야 표현
                            SetTile(_tilemap_fogOfWar, tile_2d, null);
                        }
                        else
                        {
                            SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                        }

                        //SetTile(_tilemap_fogOfWar, xy, null);

                    }
                    else
                    {
                        SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                    }
                }
                else
                {
                    //안보이는 곳
                    SetTile(_tilemap_fogOfWar, tile_2d, tileScript3);
                }

            }

        }


        private bool __is_dividing = false;
        public IEnumerator DivideUpdate_FogOfWar(Vector3 standard_3d, Vector3 lookAt_dir)
        {

            if (true == __is_dividing) yield break;
            __is_dividing = true;

            //TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_RuleTile".GetHashCode());
            TileBase tileScript3 = SingleO.resourceManager.GetTileScript("fow_TerrainTile".GetHashCode());
            //Color baseColor = Color.white;
            //Color fogColor = new Color(1, 1, 1, 0.7f);
            Vector3Int v2IntTo3 = ConstV.v3Int_zero;
            Index2 posXY_2d = SingleO.cellPartition.ToPosition2D(standard_3d); //this.ToPosition2D(standard_3d);
            Vector3 tile_3d_center = ConstV.v3_zero;
            Index2 tile_2d = ConstV.id2_zero;
            int count = -1;
            foreach (Index2 xy in _indexesNxN[29])
            {
                tile_2d = xy + posXY_2d;

                //=====================================================

                tile_3d_center = SingleO.cellPartition.ToPosition3D_Center(tile_2d);//this.ToPosition3D_Center(tile_2d);

                v2IntTo3.x = tile_2d.x;
                v2IntTo3.y = tile_2d.y;
                v2IntTo3.z = 0;
                RuleExtraTile ruleTile = _tilemap_fogOfWar.GetTile(v2IntTo3) as RuleExtraTile;
                float sqrDis = (tile_3d_center -  standard_3d).sqrMagnitude;
                float sqrStd = GridManager.MeterToWorld * 6.2f; sqrStd *= sqrStd;
                if (sqrDis <= sqrStd)
                {
                    if (true == SingleO.cellPartition.IsVisibleTile(standard_3d, tile_3d_center, 0.1f))
                    //SphereModel model = SingleO.objectManager.GetSphereTree_Struct().RayTrace_FirstReturn(standard_3d, tile_3d_center, null);
                    //if(null == model)
                    {

                        //대상과 정반대 방향이 아닐때 처리 
                        //Vector3 tileDir = tile_3d_center - standard_3d;
                        Vector3 tileDir = (tile_3d_center -  standard_3d);
                        //tileDir.Normalize(); lookAt_dir.Normalize();
                        //tileDir = Misc.GetDir64_Normal3D(tileDir); lookAt_dir = Misc.GetDir64_Normal3D(lookAt_dir); //근사치노멀을 사용하면 값이 이상하게 나옴a
                        sqrStd = GridManager.MeterToWorld * 1.2f; sqrStd *= sqrStd;

                        if (true == SingleO.cellPartition.HasStructUpTile(tile_3d_center))//this.HasStructUpTile(tile_3d_center))
                        {
                            //구조덮개타일인 경우 
                            SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                        }
                        //else if (Mathf.Cos(Mathf.Deg2Rad * 40f) < Vector3.Dot(tileDir, lookAt_dir))
                        else if (40f >= Geo.Angle_AxisY(tileDir, lookAt_dir))
                        {
                            //원거리 시야 표현
                            SetTile(_tilemap_fogOfWar, v2IntTo3, null);
                        }
                        else if (sqrDis <= sqrStd)
                        {
                            //주변 시야 표현
                            SetTile(_tilemap_fogOfWar, v2IntTo3, null);
                        }
                        else
                        {
                            SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                        }

                        //SetTile(_tilemap_fogOfWar, xy, null);

                    }
                    else
                    {
                        SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                    }
                }
                else
                {
                    //안보이는 곳
                    SetTile(_tilemap_fogOfWar, v2IntTo3, tileScript3);
                }

                //_tilemap_fogOfWar.SetTileFlags(dst, TileFlags.None);
                //if(_tilemap_fogOfWar.GetColor(dst).Equals(baseColor))
                //{
                //    _tilemap_fogOfWar.SetColor(dst, fogColor);    
                //}else
                //{
                //    _tilemap_fogOfWar.SetColor(dst, baseColor);    
                //}


                //=====================================================
                count++;

                //타일 300개 처리했으면 0.001초 뒤에 다시 처리한다
                if (0 == count % 300)
                    yield return new WaitForSeconds(0.001f);


            }
            __is_dividing = false;

        }

        //원 반지름 길이를 포함하는 그리드범위 구하기
        public uint GetNxNIncluded_CircleRadius(float maxRadius)
        {
            //최대 반지름 길이를 포함하는  정사각형 그리드 범위 구하기  
            uint nxn = (uint)((maxRadius * 2) / this._cellSize_x); //소수점 버림을 한다 
            if (0 == nxn % 2) nxn -= 1; //짝수라면 홀수로 변환한다
            if (nxn > GridManager.NxN_MAX) nxn = GridManager.NxN_MAX;
            if (nxn < GridManager.NxN_MIN) nxn = GridManager.NxN_MIN;

            return nxn;
        }

        /// <summary>
        /// 정사각형 모양의 중심이 0인 인덱스 배열을 만든다
        /// </summary>
        /// <param name="widthCenter">홀수만 가능 , 가운데 가로길이a</param>
        /// <param name="axis">Y축 , -Z축 만 가능</param>
        public Vector3Int[] CreateIndexesNxN_SquareCenter(ushort widthCenter, Vector3 axis)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == widthCenter % 2) return null;

            Vector3Int[] indexes = new Vector3Int[widthCenter * widthCenter];

            int count = 0;
            Vector3Int index = Vector3Int.zero;
            int startIdx = (widthCenter - 1) / 2; //중심좌표를 (0,0)으로 만들기 위함
            for (int i = -startIdx; i < widthCenter - startIdx; i++)
            {
                //temp1 = "";
                for (int j = -startIdx; j < widthCenter - startIdx; j++)
                {
                    //temp1 += "(" + i + ", "+ j +")  "; 

                    if (ConstV.v3_up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        index.x = j;
                        index.y = 0;
                        index.z = i;
                    }

                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        index.x = j;
                        index.y = i;
                        index.z = 0;
                    }

                    indexes[count++] = index;
                }
                //DebugWide.LogBlue(temp1);
            }
            return indexes;

        }

        //중심좌표로부터 가까운 순으로 배열이 배치된다
        public Index2[] CreateIndexesNxN_SquareCenter_Tornado(ushort width, ushort height)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == width % 2 || 0 == height % 2)
            {
                DebugWide.LogRed("CreateIndexesNxN_SquareCenter_Tornado : width , height : 홀수값을 넣어야 한다 ");
                return null;
            }
            //DebugWide.LogBlue("_____________________" + widthCenter);
            //string debugStr = "";
            //int debugSum = 0;

            int Tornado_Num = 1;
            int Tornado_Count = 0;
            Vector3 dir = ConstV.v3_right;
            Index2 prevMax = ConstV.id2_zero;
            Index2 prediction = ConstV.id2_zero;
            Index2 cur = ConstV.id2_zero;
            Index2[] indexes = new Index2[width * height];
            int cnt = 0;
            int max_cnt = width > height ? width : height; //큰값이 들어가게 한다 
            max_cnt *= max_cnt;
            //for (int cnt = 0; cnt < indexes.Length; cnt++)
            for (int i = 0; i < max_cnt; i++)
            {
                if (Math.Abs(cur.x) < Math.Abs(width * 0.5f) &&
                   Math.Abs(cur.y) < Math.Abs(height * 0.5f))
                {
                    //DebugWide.LogBlue(cnt + "  " + cur);
                    indexes[cnt] = cur;
                    cnt++;
                }


                prediction.x = (int)(dir.x * Tornado_Num); //소수점 버림
                prediction.y = (int)(dir.y * Tornado_Num); //소수점 버림
                //prediction.z = (int)(dir.z * Tornado_Num); //소수점 버림

                //DebugWide.LogBlue(" cnt: " + cnt + "  tnum : " + Tornado_Num + "  " +(prevMax + dir * Tornado_Num) + "  cur : " + cur);
                //해당방향의 최대값에 도달하면 90도 회전 
                if (prevMax + prediction == cur)
                {
                    //DebugWide.LogBlue("___" + cur);
                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Num + Tornado_Count % 2); //1 1 2 2 3 3 4 4 5 5 .... 이렇게 증가하는 수열값임 
                    Tornado_Count++;

                    //debugStr += " " + Tornado_Num; //chamto test
                    //debugSum += Tornado_Num; 

                    //반시계방향으로 회전하도록 한다 
                    //if (ConstV.v3_up == axis)
                    //{
                    //    //y축 중심 : 3d용 좌표
                    //    dir = Quaternion.Euler(0, -90f, 0) * dir;
                    //}
                    //if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, 90f) * dir;
                    }
                    dir.Normalize(); //부동소수점 문제 때문에 정규화 해준다 
                }

                //지정된 방향값으로 한칸씩 증가한다
                cur.x += (int)Mathf.Round(dir.x); //반올림 한다 : 대각선 방향 때문 
                cur.y += (int)Mathf.Round(dir.y);
                //cur.z += (int)Mathf.Round(dir.z);
            }

            //DebugWide.LogBlue(debugStr + "   : sum : " + debugSum);

            return indexes;

        }

        //원형
        public Vector3Int[] CreateIndexesNxN_Circle(ushort radius, Vector3 axis)
        {
            Vector3Int[] indexes = new Vector3Int[5];



            return indexes;
        }

        //마름모꼴
        public Vector3Int[] CreateIndexesNxN_RhombusCenter(ushort widthCenter, Vector3 axis)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == widthCenter % 2) return null;

            //마름모꼴의 전체 셀 개수구하기
            //마름모 반지름의 크기에 따라 1 4 8 12 16 ... 으로 증가한다 
            int Length = 1;
            for (int i = 1; i <= widthCenter / 2; i++)
            {
                Length += i * 4;
            }

            int Tornado_Num = 1;
            int Tornado_Count = 0;
            float angle = 0f;
            Vector3 dir = ConstV.v3_zero;
            Vector3 prevMax = ConstV.v3_zero;
            Vector3Int curMax = Vector3Int.zero;
            Vector3Int cur = Vector3Int.zero;
            Vector3Int[] indexes = new Vector3Int[Length];
            for (int cnt = 0; cnt < indexes.Length; cnt++)
            {
                indexes[cnt] = cur;


                //지정된 방향값으로 한칸씩 증가한다
                cur.x += (int)Mathf.Round(dir.x); //반올림 한다 : 대각선 방향 때문 
                cur.y += (int)Mathf.Round(dir.y);
                cur.z += (int)Mathf.Round(dir.z);

                //DebugWide.LogBlue("cur:  " + cur + "   perv" + prevMax + "   if" + (prevMax + dir * Tornado_Num));
                //해당방향의 최대값에 도달하면 90도 회전 
                curMax.x = (int)(prevMax.x + Mathf.Round(dir.x) * Tornado_Num);
                curMax.y = (int)(prevMax.y + Mathf.Round(dir.y) * Tornado_Num);
                curMax.z = (int)(prevMax.z + Mathf.Round(dir.z) * Tornado_Num);

                if (curMax == cur)
                {
                    //DebugWide.LogBlue(curMax + "  " + Tornado_Count % 4 + "  :" + Tornado_Count);
                    if (0 == Tornado_Count % 4)
                    {
                        //좌상단으로 방향 전환
                        angle = -135f;

                        //오른쪽으로 한칸더 간다
                        dir = ConstV.v3_right;
                        cur.x += (int)Mathf.Round(dir.x);
                        cur.y += (int)Mathf.Round(dir.y);
                        cur.z += (int)Mathf.Round(dir.z);
                    }
                    else
                    {
                        angle = -90f;
                    }

                    if (ConstV.v3_up == axis)
                    {
                        //y축 중심 : 3d용 좌표
                        dir = Quaternion.Euler(0, angle, 0) * dir;
                    }
                    if (Vector3.back == axis)
                    {
                        //-z축 중심 : 2d용 좌표
                        dir = Quaternion.Euler(0, 0, -angle) * dir;
                    }
                    dir.Normalize(); //부동소수점 문제 때문에 정규화 해준다 

                    prevMax = cur; //최대 위치값 갱신
                    Tornado_Num = (Tornado_Count / 4) + 1; //1 1 1 1 2 2 2 2 3 3 3 3 .... 이렇게 증가하는 수열값임 

                    Tornado_Count++;
                }

            }

            return indexes;

        }



        /// <summary>
        /// 구조타일 영역에 미포함
        /// </summary>
        public const int _ReturnMessage_NotIncluded_InStructTile = 0;

        /// <summary>
        /// 구조타일 영역에 포함. 구조대각타일이 아니다
        /// </summary>
        public const int _ReturnMessage_Included_InStructTile = 1;

        /// <summary>
        /// 구조대각타일이며 , 대각타일 영역에 미포함
        /// </summary>
        public const int _ReturnMessage_NotIncluded_InDiagonalArea = 2;

        /// <summary>
        /// 구조대각타일이며 , 대각타일 영역에 포함
        /// </summary>
        public const int _ReturnMessage_Included_InDiagonalArea = 3;


        //구조타일의 대각타일 영역에 포함하는지 검사 
        public int IsIncluded_InDiagonalArea(Vector3 xz_3d)
        {
            Vector3Int xy_2d = _tilemap_struct.WorldToCell(xz_3d);

            CellSpace structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                switch (structTile._eDir)
                {
                    case eDirection8.leftUp:
                    case eDirection8.leftDown:
                    case eDirection8.rightUp:
                    case eDirection8.rightDown:
                        {
                            //특수고정대각 타일은 일반구조 타일처럼 처리한다 
                            if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                                return _ReturnMessage_Included_InStructTile;

                            //타일 중앙점 o , 검사할 점 p 가 있을 때
                            //대각타일 영역에서 점 p를 포함하는지 내적을 이용해서 검사 
                            Vector3 oDir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);
                            //Vector3 pDir = xz_3d - structTile._center_3d; //실수값 부정확성 때문에 같은 위치에서도 다른 결과가 나옴.
                            Vector3 pDir = Misc.GetDir8_Normal3D(xz_3d - structTile._pos3d_center); //정규화된8 방향값으로 변환해서 부정확성을 최소로 줄인다.
                            if (0 > Vector3.Dot(oDir, pDir))
                                return _ReturnMessage_Included_InDiagonalArea; //대각타일 영역에 포함
                            else
                                return _ReturnMessage_NotIncluded_InDiagonalArea; //구조타일 영역에 포함 , 대각타일 영역에 미포함
                        }

                    default:
                        {
                            return _ReturnMessage_Included_InStructTile; //구조타일 영역에 포함
                        }

                }

            }

            return _ReturnMessage_NotIncluded_InStructTile; //구조타일 영역에 미포함
        }

        public bool HasStructUpTile(Vector3 xz_3d)
        {
            CellSpace structTile = null;
            return HasStructUpTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructUpTile(Vector3 xz_3d, out CellSpace structTile)
        {
            //return HasStructUpTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
            return HasStructUpTile_InPostion2D(this.ToPosition2D(xz_3d), out structTile);
        }
        public bool HasStructUpTile_InPostion2D(Vector3Int xy_2d, out CellSpace structTile)
        {
            structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                return structTile._isUpTile;
            }

            return false;
        }

        public bool HasStructTile(Vector3 xz_3d)
        {
            CellSpace structTile = null;
            Vector3Int xy_2d = _tilemap_struct.WorldToCell(xz_3d);
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile(Vector3 xz_3d, out CellSpace structTile)
        {
            //return HasStructTile_InPostion2D(_tilemap_struct.WorldToCell(xz_3d), out structTile);
            Vector3Int xy_2d = ToPosition2D(xz_3d);
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile_InPostion2D(Vector3Int xy_2d)
        {
            CellSpace structTile = null;
            return HasStructTile_InPostion2D(xy_2d, out structTile);
        }
        public bool HasStructTile_InPostion2D(Vector3Int xy_2d, out CellSpace structTile)
        {
            structTile = null;
            if (true == _structTileList.TryGetValue(xy_2d, out structTile))
            {
                return true;
            }

            return false;
        }




        public CellInfo GetCellInfo(Vector3Int cellIndex)
        {
            CellInfo cell = null;
            _cellInfoList.TryGetValue(cellIndex, out cell);

            return cell;
        }



        //todo : 자료구조를 복사하는데 부하가 있기때문에, 자료구조를 직접 순회하면서 처리하는 방향으로 해봐야겠다
        public CellInfo GetCellInfo_NxN(Vector3Int center, ushort NCount_odd)
        {
            //NCount 는 홀수값을 넣어야 한다 
            if (0 == NCount_odd % 2) return null;

            CellInfo cellList = new CellInfo();
            cellList._index = center;

            //DebugWide.LogBlue("================" + NCount_odd + " ================ ");
            //string temp1 = "";

            CellInfo dst = null;
            Vector3Int index = Vector3Int.zero;
            int startIdx = (NCount_odd - 1) / 2; //중심좌표를 (0,0)으로 만들기 위함
            for (int i = -startIdx; i < NCount_odd - startIdx; i++)
            {
                //temp1 = "";
                for (int j = -startIdx; j < NCount_odd - startIdx; j++)
                {
                    //temp1 += "(" + i + ", "+ j +")  "; 
                    index.x = i + center.x;
                    index.y = j + center.y;
                    dst = this.GetCellInfo(index);

                    if (null != dst && 0 != dst.Count)
                    {
                        //DebugWide.LogBlue(dst.Count + "  " + i + "," + j);

                        foreach (var v in dst)
                        {
                            cellList.AddLast(v);
                        }

                    }
                }
                //DebugWide.LogBlue(temp1);
            }

            return cellList;
        }


        public void AddCellInfo_Being(Vector3Int cellIndex, Being being)
        {
            CellInfo cell = null;
            if (false == _cellInfoList.TryGetValue(cellIndex, out cell))
            {
                cell = new CellInfo();
                cell._index = cellIndex;
                _cellInfoList.Add(cellIndex, cell);
            }
            _cellInfoList[cellIndex].AddLast(being);
        }

        public void RemoveCellInfo_Being(Vector3Int cellIndex, Being being)
        {
            if (null == being) return;

            CellInfo cell = null;
            if (true == _cellInfoList.TryGetValue(cellIndex, out cell))
            {
                cell.Remove(being);
            }

        }



        //2d 좌표는 x,y만 사용한다. x,z좌표로의 변환은 허용 안한다 
        public Vector3Int ToPosition2D(Vector3 pos3d)
        {

            Vector3Int posXY_2d = Vector3Int.zero;

            //if (0 <= pos3d.x)
            //{
            //    //양수일때는 소수점을 버린다. 
            //    posXY_2d.x = (int)(pos3d.x / cellSize_x);
            //}
            //else
            //{
            //    //음수일때는 올림을 한다. 
            //    posXY_2d.x = (int)((pos3d.x / cellSize_x) - 0.9f);
            //}


            //if (0 <= pos3d.z)
            //{

            //    //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
            //    //posXY_2d.y = (int)(pos3d.z / cellSize_z); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다

            //    posXY_2d.y = Mathf.FloorToInt(pos3d.z / cellSize_z);

            //}
            //else
            //{
            //    posXY_2d.y = (int)((pos3d.z / cellSize_z) - 0.9f);
            //}

            //부동소수점 처리 문제로 직접계산하지 않는다 
            posXY_2d.x = Mathf.FloorToInt(pos3d.x / _cellSize_x);
            posXY_2d.y = Mathf.FloorToInt(pos3d.z / _cellSize_z);

            //posXY_2d = _tilemap_struct.WorldToCell(pos3d); //버림함수를 사용하여 계산하는 것과 결과가 달리 나온다 

            return posXY_2d;
        }

        public Vector3 ToCenter3D_FromPosition3D(Vector3 pos3d)
        {
            Vector3 pos = ConstV.v3_zero;
            Vector3 cellSize = ConstV.v3_zero;

            Vector3Int cellPos = this.ToPosition2D(pos3d);

            return this.ToPosition3D_Center(cellPos);
        }

        //pos2d 는 항시 x,y공간만 사용한다. 다른 공간은 변환 허용안함.
        //grid 와 호환 안되는 함수 
        public Vector3 ToPosition3D_Center(Vector3Int posXY_2d)
        {
            Vector3 pos3d = ConstV.v3_zero;

            {
                pos3d.x = (float)posXY_2d.x * _cellSize_x;
                pos3d.z = (float)posXY_2d.y * _cellSize_z;

                //셀의 중간에 위치하도록 한다
                pos3d.x += _cellSize_x * 0.5f;
                pos3d.z += _cellSize_z * 0.5f;
            }

            return pos3d;
        }

        public Vector3 ToPosition3D(Vector3Int posXY_2d)
        {
            Vector3 pos3d = ConstV.v3_zero;

            {
                pos3d.x = (float)posXY_2d.x * _cellSize_x;
                pos3d.z = (float)posXY_2d.y * _cellSize_z;
            }

            return pos3d;
        }


        public int ToPosition1D(Vector3Int posXY_2d, int tileBlock_width_size)
        {
            //Assert.IsFalse(0 > posXY_2d.x || 0 > posXY_2d.y, "음수좌표값은 1차원값으로 변환 할 수 없다");
            if (0 > posXY_2d.x || 0 > posXY_2d.y) return -1;

            return (posXY_2d.x + posXY_2d.y * tileBlock_width_size); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Vector3Int ToPosition2D(int pos1d, int tileBlock_width_size)
        {
            //Assert.IsFalse(0 > pos1d, "음수좌표값은 2차원값으로 변환 할 수 없다");
            if (0 > pos1d) return Vector3Int.zero;

            Vector3Int posXY_2d = Vector3Int.zero;

            posXY_2d.x = pos1d % tileBlock_width_size;
            posXY_2d.y = pos1d / tileBlock_width_size;

            return posXY_2d;
        }

    }
}
