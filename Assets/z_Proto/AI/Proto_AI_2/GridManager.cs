using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UtilGS9;


namespace Proto_AI
{
    public class Vector3IntComparer : IEqualityComparer<Vector3Int>
    {
        public bool Equals(Vector3Int a, Vector3Int b)
        {
            if (a.x == b.x && a.y == b.y && a.z == b.z)
                return true;
            return false;
        }

        public int GetHashCode(Vector3Int a)
        {
            return a.GetHashCode();
        }
    }


    //장애물 정보 
    public class CellSpace
    {
        public const int Specifier_DiagonalFixing = 7; //대각고정 예약어

        public int _specifier = 0;
        public eDirection8 _eDir = eDirection8.none;

        public bool _isUpTile = false; //챔프보다 위에 있는 타일 (TileMap_StructUp)
        public bool _isStructTile = false; //구조물 타일인지 나타냄

        public Vector3 _pos3d_center = ConstV.v3_zero;    //타일의 중앙 월드위치

        public Index2 _pos2d = ConstV.id2_zero;
        public int _pos1d = -1; //타일의 1차원 위치값 

        public LineSegment3 line;

        //==================================================
        //타일에 속해있는 객체의 링크
        //public Being _head = null;
        //public int _childCount = 0;


        //public Being MatchRelation(Camp.eRelation relation, Being target)
        //{
        //    Being getB = null;
        //    Being next = _head;
        //    while (null != (object)next)
        //    {
        //        getB = next;
        //        next = next._next_sibling;

        //        if ((object)getB == (object)target) continue;

        //        Camp.eRelation getR = SingleO.campManager.GetRelation(target._campKind, getB._campKind);
        //        //DebugWide.LogBlue(getR.ToString()); //chamto test
        //        if (relation == getR)
        //            return getB; //찾았다 !!


        //    }

        //    return null;
        //}


        //새로운 객체가 머리가 된다 
        //public void AttachChild(Being newHead)
        //{
        //    Being cur_child = _head;
        //    _head = newHead; // new head of list

        //    newHead._prev_sibling = null;
        //    newHead._next_sibling = cur_child;
        //    newHead._cur_cell = this;

        //    if (null != cur_child) cur_child._prev_sibling = newHead; // previous now this..

        //    _childCount++;

        //}

        //public void DetachChild(Being dst)
        //{
        //    if (null == dst._cur_cell || null == dst._cur_cell._head || 0 == dst._cur_cell._childCount) return;

        //    Being prev = dst._prev_sibling;
        //    Being next = dst._next_sibling;
        //    if (null != prev)
        //    {
        //        prev._next_sibling = next;
        //        if (null != next) next._prev_sibling = prev;
        //    }
        //    else
        //    {
        //        //dst가 head 인 경우, 새로운 head 설정한다
        //        //_children = next;
        //        dst._cur_cell._head = next;

        //        if (null != next) next._prev_sibling = null;
        //    }

        //    dst._cur_cell._childCount--;
        //    dst._prev_sibling = null;
        //    dst._next_sibling = null;
        //    dst._cur_cell = null;
        //}
    }

    public struct BoundaryTile
    {
        public bool isBoundary;
        public CellSpace cell;

        public void Init()
        {
            isBoundary = true;
            cell = null;
        }
    }
    public class BoundaryTileList : LinkedList<BoundaryTile>
    { }


    public class GridManager
    {
        public Grid _grid = null;
        public Tilemap _tilemap_struct = null;
        public Dictionary<Vector3Int, CellSpace> _structTileList = new Dictionary<Vector3Int, CellSpace>(new Vector3IntComparer());
        public Dictionary<Vector3Int, BoundaryTileList> _boundaryList = new Dictionary<Vector3Int, BoundaryTileList>(new Vector3IntComparer());

        public float _cellSize_x = 1f;
        public float _cellSize_z = 1f;

        public int _tileBlock_width_size = 64;

        public void Init()
        {

            _grid = GameObject.Find("0_grid").GetComponent<Grid>();
            GameObject o = GameObject.Find("Tilemap_layer_1");
            if (null != o)
            {
                _tilemap_struct = o.GetComponent<Tilemap>();
            }

            this.LoadTilemap_Struct();


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

        public Vector3 ToPosition3D_Center(Vector3 pos3d)
        {

            pos3d.x = Mathf.FloorToInt(pos3d.x / _cellSize_x);
            pos3d.y = 0;
            pos3d.z = Mathf.FloorToInt(pos3d.z / _cellSize_z);

            //셀의 중간에 위치하도록 한다
            pos3d.x += _cellSize_x * 0.5f;
            pos3d.z += _cellSize_z * 0.5f;

            return pos3d;
        }

        //2d 좌표는 x,y만 사용한다. x,z좌표로의 변환은 허용 안한다 
        public Vector3Int ToPosition2D(Vector3 pos3d)
        {

            Vector3Int posXY_2d = Vector3Int.zero;

            //부동소수점 처리 문제로 직접계산하지 않는다 
            posXY_2d.x = Mathf.FloorToInt(pos3d.x / _cellSize_x);
            posXY_2d.y = Mathf.FloorToInt(pos3d.z / _cellSize_z);

            //posXY_2d = _tilemap_struct.WorldToCell(pos3d); //버림함수를 사용하여 계산하는 것과 결과가 달리 나온다 

            return posXY_2d;
        }

        public int ToPosition1D(Vector3Int posXY_2d, int tileBlock_width_size)
        {
            //Assert.IsFalse(0 > posXY_2d.x || 0 > posXY_2d.y, "음수좌표값은 1차원값으로 변환 할 수 없다");
            if (0 > posXY_2d.x || 0 > posXY_2d.y) return -1;

            return (posXY_2d.x + posXY_2d.y * tileBlock_width_size); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public CellSpace GetStructTile(Vector3 pos3d)
        {
            CellSpace cell = null;
            Vector3Int pos = ToPosition2D(pos3d);
            _structTileList.TryGetValue(pos, out cell);

            return cell;
        }

        public BoundaryTileList GetBoundaryTileList(Vector3 pos3d)
        {
            BoundaryTileList list = null;
            Vector3Int pos = ToPosition2D(pos3d);
            _boundaryList.TryGetValue(pos, out list);

            return list;
        }

        public void LoadTilemap_Struct()
        {
            if (null == _tilemap_struct) return;

            _tilemap_struct.RefreshAllTiles();
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
                structTile._pos1d = this.ToPosition1D(XY_2d, _tileBlock_width_size); 
                structTile._eDir = ruleTile._tileDataMap.GetDirection8(XY_2d);

                //방향이 없는 덮개타일은 걸러낸다 
                if (eDirection8.none == structTile._eDir) continue;

                structTile._isUpTile = ruleTile._tileDataMap.Get_IsUpTile(XY_2d);
                structTile._isStructTile = true;

                LineSegment3 line;
                CalcBoundaryLine(structTile, out line);
                structTile.line = line;

                _structTileList.Add(XY_2d, structTile);

            }

            DebugWide.LogBlue("LoadTile : " + _structTileList.Count + "  -  TileMap_Struct RefreshAllTiles");


            //확장영역에 구조물 경계선 추가 
            Load_StructLine();
        }


        public void Load_StructLine()
        {
            foreach (KeyValuePair<Vector3Int, CellSpace> t in _structTileList)
            {
                //덮개타일은 걸러낸다 
                if (eDirection8.none != t.Value._eDir)
                {
                    BoundaryTile info = new BoundaryTile();
                    info.Init();
                    info.cell = t.Value;


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

                    key = t.Key;
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
                    if (true == diag)
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

        public bool CalcBoundaryLine(CellSpace cell, out LineSegment3 line)
        {
            line = new LineSegment3();
            if (null == cell) return false;
            if (eDirection8.none == cell._eDir) return false;
            //if (CellSpace.Specifier_DiagonalFixing == cell._specifier) return false;

            //타일맵 정수 좌표계와 게임 정수 좌표계가 다름
            //타일맵 정수 좌표계 : x-y , 게임 정수 좌표계 : x-z
            //==========================================

            float size = _cellSize_x * 0.5f;

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
                            temp.x = cell._pos3d_center.x - size;
                            temp.z = cell._pos3d_center.z + size;
                            line.origin = temp;
                            line.last = temp;
                            return true;
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
                            temp.x = cell._pos3d_center.x + size;
                            temp.z = cell._pos3d_center.z + size;
                            line.origin = temp;
                            line.last = temp;
                            return true;
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
                            temp.x = cell._pos3d_center.x - size;
                            temp.z = cell._pos3d_center.z - size;
                            line.origin = temp;
                            line.last = temp;
                            return true;
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
                            temp.x = cell._pos3d_center.x + size;
                            temp.z = cell._pos3d_center.z - size;
                            line.origin = temp;
                            line.last = temp;
                            return true;
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

            //foreach (KeyValuePair<Vector3Int, CellSpace> info1 in _structTileList)
            //{
            //    Vector3 pos = ToPosition3D_Center(info1.Key);

            //    if(info1.Value._isUpTile)
            //        DebugWide.DrawCircle(pos, 0.5f, Color.red);


            //    DebugWide.PrintText(pos, Color.black, "" + info1.Value._eDir);

            //    info1.Value.line.Draw(Color.white);
            //}
            //return;

            foreach (KeyValuePair<Vector3Int, BoundaryTileList> info1 in _boundaryList)
            {

                Vector3 pos = ToPosition3D_Center(new Vector3Int(info1.Key.x, info1.Key.y,0));
                //DebugWide.PrintText(pos, Color.black, "" + info1.Value.Count);

                foreach (BoundaryTile info2 in info1.Value)
                {
                    if (true == info2.isBoundary)
                    {
                        //DebugWide.DrawLine(info2.cell._pos3d_center, pos, Color.white);
                        DebugWide.DrawLine(info2.cell.line.origin + info2.cell.line.direction * 0.5f, pos, Color.white);
                    }
                    else
                    {
                        if (CellSpace.Specifier_DiagonalFixing == info2.cell._specifier)
                            DebugWide.DrawCircle(info2.cell.line.origin, 0.1f, Color.red);

                        info2.cell.line.Draw(Color.white);
                    }

                }

            }
        }




        public Vector3 GetBorder_StructTile(Vector3 srcPos, float radius, CellSpace structTile)
        {
            if (null == structTile) return srcPos;

            Vector3 centerToSrc_dir = VOp.Minus(srcPos, structTile._pos3d_center);
            Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);


            float size = _cellSize_x * 0.5f;
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

        public Vector3 Collision_StructLine(Vector3 srcPos, float RADIUS)
        {

            Vector3Int pos_2d = ToPosition2D(srcPos);

            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list)) return srcPos;


            //RADIUS = 1.0f;
            foreach (BoundaryTile info in list)
            {
                //주변경계타일인 경우
                if (true == info.isBoundary)
                {

                    if (true == Geo.IntersectLineSegment(srcPos, RADIUS, info.cell.line.origin, info.cell.line.last))
                    {

                        Vector3 cp = info.cell.line.ClosestPoint(srcPos);
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

        public Vector3 GetBorder_StructFixingTile(Vector3 srcPos, float radius, CellSpace structTile)
        {
            if (null == structTile) return srcPos;
            if (CellSpace.Specifier_DiagonalFixing != structTile._specifier) return srcPos;

            Vector3 push_dir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);
            float size = _cellSize_x * 0.5f;

            //8방향별 축값 고정  
            switch (structTile._eDir)
            {

                //break;
                case eDirection8.leftUp:
                    {
                        //down , right
                        srcPos.x = structTile._pos3d_center.x - size;
                        srcPos.z = structTile._pos3d_center.z + size;
                        return srcPos + push_dir * radius;

                    }
                    break;
                case eDirection8.rightUp:
                    {
                        //down , left
                        srcPos.x = structTile._pos3d_center.x + size;
                        srcPos.z = structTile._pos3d_center.z + size;
                        return srcPos + push_dir * radius;
                    }
                    break;
                case eDirection8.leftDown:
                    {
                        //up , right
                        srcPos.x = structTile._pos3d_center.x - size;
                        srcPos.z = structTile._pos3d_center.z - size;
                        return srcPos + push_dir * radius;

                    }
                    break;
                case eDirection8.rightDown:
                    {
                        //up , left
                        srcPos.x = structTile._pos3d_center.x + size;
                        srcPos.z = structTile._pos3d_center.z - size;
                        return srcPos + push_dir * radius;
                    }
            }
                  
            return srcPos;

        }

        public Vector3 Collision_FirstStructTile(Vector3 oldPos, Vector3 srcPos, float RADIUS)
        {
            const int MAX_COUNT = 5; //5개의 타일만 검사 
            Vector3 dir = srcPos - oldPos;
            CellSpace structTile = Find_FirstStructTile(oldPos, oldPos + dir * 100, MAX_COUNT);
            if(null != structTile)
            {

                //if (CellSpace.Specifier_DiagonalFixing == structTile._specifier)
                //{
                //    return GetBorder_StructFixingTile(srcPos, RADIUS, structTile);
                //}

                bool calc = false;
                Vector3 cp = structTile.line.ClosestPoint(srcPos);
                Vector3 cpToSrc = srcPos - cp;
                //Vector3 push_dir = Misc.GetDir8_Normal3D(srcPos - cp); //srcPos 가 잘못계산되는 문제 발생 
                Vector3 push_dir = VOp.Normalize(cpToSrc);

                //DebugWide.AddDrawQ_Line(srcPos, oldPos, Color.magenta);


                DebugWide.LogBlue("  Collision_FirstStructTile src " + srcPos);

                //지형타일을 넘어간 경우 
                if (Vector3.Dot(dir, cpToSrc) > 0)
                {
                    calc = true;
                    push_dir *= -1;
                }

                //지형과 원이 겹친경우 
                if(cpToSrc.sqrMagnitude < RADIUS*RADIUS)
                {
                    calc = true;
                }

                if(calc)
                {
                    srcPos = cp + push_dir * RADIUS;

                }

                DebugWide.LogBlue("  Collision_FirstStructTile  " + structTile._pos3d_center + "  " + cp + "  " + push_dir + "  " + srcPos);

                //DebugWide.AddDrawQ_Line(structTile.line.origin, structTile.line.last, Color.white);
                //DebugWide.AddDrawQ_Circle(cp, 0.3f, Color.white);
                DebugWide.AddDrawQ_Circle(structTile._pos3d_center, 0.5f, Color.green);
            }

            return srcPos;
        }

        public Vector3 Collision_StructLine_Test3(Vector3 oldPos, Vector3 srcPos, float RADIUS)
        {
            srcPos = Collision_FirstStructTile(oldPos, srcPos, RADIUS); //벽통과 되는 경우 통과되기 전위치를 반환한다 

            Vector3Int pos_2d = ToPosition2D(srcPos);


            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list)) return srcPos;

            DebugWide.LogBlue(pos_2d + "  " + list.Count);

            int count = 0;
            //RADIUS = 1.0f;
            foreach (BoundaryTile info in list)
            {
                //주변경계타일인 경우
                if (true == info.isBoundary)
                {
                    //bool inter = false;
                    //if (CellSpace.Specifier_DiagonalFixing == info.cell._specifier)
                    //{
                    //    //if((info.cell.line.origin - srcPos).sqrMagnitude <= RADIUS*RADIUS)
                    //    {
                    //        //inter = true;
                    //        DebugWide.LogRed("==================================================");
                    //    }
                    //}
                    //else
                    //{
                    //    inter = Geo.IntersectLineSegment(srcPos, RADIUS, info.cell.line.origin, info.cell.line.last);
                    //}

                    if (Geo.IntersectLineSegment(srcPos, RADIUS, info.cell.line.origin, info.cell.line.last))
                    {

                        Vector3 cp = info.cell.line.ClosestPoint(srcPos);

                        //DebugWide.AddDrawQ_Circle(cp, 0.3f, Color.black);

                        Vector3 n = VOp.Normalize(srcPos - cp);
                        //Vector3 n = Misc.GetDir8_Normal3D_AxisY(info.cell._eDir); //연구필요 
                        srcPos = cp + n * RADIUS;

                        DebugWide.LogBlue(count + "  boundary  " + cp + "  " + n + "  " + srcPos);

                    }
                }
                //경계타일인 경우 
                else
                {
                    DebugWide.LogBlue(count);

                    srcPos = GetBorder_StructTile(srcPos, RADIUS, info.cell);
                }

                count++;
            }

            return srcPos;


        }

        public CellSpace Find_FirstStructTile(Vector3 origin_3d, Vector3 target_3d , int MAX_COUNT)
        {

            Vector3 nextPos = origin_3d;
            Vector3 prev_center = ToPosition3D_Center(origin_3d);
            int count = 0;
            bool is_next = true;
            while (is_next)
            {

                is_next = GetTilePosition_Segment(count, origin_3d.x, origin_3d.z, target_3d.x, target_3d.z, prev_center, out nextPos);

                //-------------------

                //DebugWide.DrawCube(nextPos, new Vector3(1f, 0, 1f), Color.magenta);
                //DebugWide.DrawLine(prev_center, nextPos, Color.red);
                //DebugWide.PrintText(nextPos, Color.white, "" + count);

                prev_center = nextPos;


                //-------------------

                //구조타일을 발견하면 바로 반환 
                CellSpace structTile = GetStructTile(nextPos);
                if (null != structTile)
                {
                    //DebugWide.DrawCube(nextPos, new Vector3(1f, 0, 1f), Color.black);
                    //structTile.line.Draw(Color.white);
                    return structTile;
                }


                count++;
                if (count > MAX_COUNT) break; //최대검사 횟수 검사 

            }

            return null;
        }

        public bool GetTilePosition_Segment(int count, float x1, float y1, float x2, float y2, Vector3 prev_center, out Vector3 next_pos)
        {
            next_pos = new Vector3(x2, 0, y2);

            if (0 == count)
            {
                next_pos = prev_center;
                return true;
            }

            //기울기를 계산 할 수 없는 수직선 처리 
            if (Misc.IsZero(x2 - x1))
            {
                //DebugWide.LogBlue("zero  ");

                int sign_y = 1;
                if (y1 > y2)
                {
                    sign_y = -1;
                }

                prev_center.z += sign_y;
                next_pos = prev_center;
                //next_pos = prev_center + new Vector3(0, 0, sign_y);

                if (next_pos.z * sign_y > y2 * sign_y)
                {
                    next_pos = new Vector3(x2, 0, y2);
                    return false;
                }

                return true;
            }

            float x, y;
            float m = (float)(y2 - y1) / (float)(x2 - x1); //기울기 계산
            if (-1 < m && m < 1) //x축이 독립축 
            {
                x = (int)prev_center.x;
                y = y1;

                int sign_x = 1;
                if (x1 > x2)
                {
                    sign_x = -1;
                }
                else
                {
                    x += 1;
                }


                if (x * sign_x <= x2 * sign_x)
                {

                    y = (m * (x - x1) + y1);

                    //------------------
                    Vector3 centerToTarget = new Vector3(x, 0, y) - prev_center;
                    Vector3 dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);
                    next_pos = prev_center + dir4n;


                    return true;

                }
                else
                {
                    return false;
                }
            }
            else //y축이 독립축 
            {
                x = x1;
                y = (int)prev_center.z;

                int sign_y = 1;
                if (y1 > y2)
                {
                    sign_y = -1;
                }
                else
                {
                    y += 1;
                }

                if (y * sign_y <= y2 * sign_y)
                {

                    x = ((y - y1) / m + x1);

                    Vector3 centerToTarget = new Vector3(x, 0, y) - prev_center;
                    //DebugWide.LogBlue(VOp.ToString(centerToTarget));
                    Vector3 dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);
                    next_pos = prev_center + dir4n;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            //return false;
        }



        //public void Draw_line_equation4(float x1, float y1, float x2, float y2)
        //{
        //    float m;
        //    float x, y;
        //    float temp;

        //    m = (float)(y2 - y1) / (float)(x2 - x1); //기울기 계산
        //    if (-1 < m && m < 1) //x축이 독립축 
        //    {
        //        int sign_x = 1;
        //        float plus_y = 0.5f;
        //        if (x1 > x2)
        //        {
        //            sign_x = -1;
        //        }
        //        if(y1 > y2)
        //        {
        //            plus_y = 0; 
        //        }

        //        x = (int)x1;
        //        y = y1;


        //        int count = 0;
        //        //while (x <= x2)
        //        while (true) 
        //        {
        //            count++;
        //            if (count > 5) return;
        //            //DebugWide.LogBlue(count);

        //            y = (m * (x - x1) + y1);


        //            DebugWide.DrawCircle(new Vector3(x, 0, y), 0.1f, Color.green);
        //            DebugWide.PrintText(new Vector3(x, 0, y), Color.green, "" + count);

        //            Vector3 origin_center = ToPosition3D_Center(new Vector3(x, 0, y));
        //            DebugWide.DrawCube(origin_center, new Vector3(1f, 0, 1f), Color.magenta);


        //            float yy = (int)(y + plus_y); //정수 y축에 고정 
        //            float xx = ((yy - y1) / m + x1);
        //            if (x1 <= xx && xx <= x2) //범위를 벗어나는 값 계산 안함
        //            {
        //                DebugWide.DrawCircle(new Vector3(xx, 0, yy), 0.1f, Color.red);
        //                DebugWide.LogBlue(count + "  " + xx + "  " + yy);

        //                origin_center = ToPosition3D_Center(new Vector3(xx, 0, yy));
        //                DebugWide.DrawCube(origin_center, new Vector3(1f, 0, 1f), Color.red);

        //            }

        //            //x++;
        //            x = x + 1 * sign_x;

        //        }
        //    }
        //    else //y축이 독립축 
        //    {
        //        if (y1 > y2)
        //        {
        //            //swap
        //            temp = x1;
        //            x1 = x2;
        //            x2 = temp;

        //            //swap
        //            temp = y1;
        //            y1 = y2;
        //            y2 = temp;
        //        }

        //        x = x1;
        //        y = (int)y1;
        //        Vector3 nextTile;
        //        Vector3 prev_center = ToPosition3D_Center(new Vector3(x, 0, y));
        //        y += 1;
        //        int count = 0;
        //        while (y <= y2)
        //        {
        //            count++;
        //            if (count > 20) return;

        //            x = ((y - y1) / m + x1);


        //            Vector3 origin_center = ToPosition3D_Center(new Vector3(x, 0, y));

        //            Vector3 centerToTarget = new Vector3(x, 0, y) - prev_center;

        //            Vector3 dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);

        //            nextTile = prev_center + dir4n;
        //            DebugWide.DrawCube(nextTile, new Vector3(1f, 0, 1f), Color.magenta);

        //            prev_center = nextTile;

        //            //오른쪽 또는 왼쪽 방향인 경우만 z값 증가  
        //            if (dir4n.z > 0 || dir4n.z < 0)
        //                y++;

        //        }
        //    }
        //}


        //20210801 - c로 배우는 알고리즘 2권 1023p 2d선그리기 참고  
        public void Draw_line_equation3(float x1, float y1, float x2, float y2)
        {
            float m;
            float x, y;

            //if (Misc.IsZero(x2 - x1)) return;

            m = (float)(y2 - y1) / (float)(x2 - x1); //기울기 계산
            if (-1 < m && m < 1) //x축이 독립축 
            {
                x = (int)x1;
                y = y1;
                Vector3 nextTile;
                Vector3 prev_center = ToPosition3D_Center(new Vector3(x, 0, y));

                int sign_x = 1;
                if (x1 > x2)
                {
                    sign_x = -1;

                }else
                {
                    x += 1;
                }


                int count = 0;
                //while (x <= x2)
                while (x*sign_x <= x2*sign_x)
                {
                    count++;
                    if (count > 20) return;
                    //DebugWide.LogBlue(count);

                    y = (m * (x - x1) + y1);


                    Vector3 origin_center = ToPosition3D_Center(new Vector3(x, 0, y));
                    //DebugWide.DrawCube(origin_center, new Vector3(1f, 0, 1f), Color.green);
                    //DebugWide.DrawCircle(new Vector3(x, 0, y), 0.1f, Color.green);
                    DebugWide.DrawCircle(origin_center, 0.2f, Color.green);
                    //DebugWide.DrawCircle(prev_center, 0.4f, Color.blue);
                    DebugWide.PrintText(prev_center, Color.green, "" + count);

                    //------------------
                    Vector3 centerToTarget = new Vector3(x, 0, y) - prev_center;

                    Vector3 dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);

                    nextTile = prev_center + dir4n;
                    DebugWide.DrawCube(nextTile, new Vector3(1f, 0, 1f), Color.magenta);
                    DebugWide.DrawLine(prev_center, nextTile, Color.red);

                    prev_center = nextTile;


                    //오른쪽 또는 왼쪽 방향인 경우만 x값 증가  
                    if (dir4n.x > 0 || dir4n.x < 0)
                    {
                        //x++;
                        x = x + 1 * sign_x; 
                    }



                }
            }
            else //y축이 독립축 
            {
                x = x1;
                y = (int)y1;
                Vector3 nextTile;
                Vector3 prev_center = ToPosition3D_Center(new Vector3(x, 0, y));

                int sign_y = 1;
                if (y1 > y2)
                {
                    sign_y = -1;
                }
                else
                {
                    y += 1;
                }

                int count = 0;
                while (y* sign_y <= y2* sign_y)
                {
                    count++;
                    if (count > 20) return;

                    x = ((y - y1)/m + x1);


                    Vector3 centerToTarget = new Vector3(x, 0, y) - prev_center;

                    Vector3 dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);

                    nextTile = prev_center + dir4n;
                    DebugWide.DrawCube(nextTile, new Vector3(1f, 0, 1f), Color.magenta);

                    prev_center = nextTile;

                    //오른쪽 또는 왼쪽 방향인 경우만 z값 증가  
                    if (dir4n.z > 0 || dir4n.z < 0)
                    {
                        //y++;
                        y = y + 1 * sign_y;
                    }


                }
            }
        }


    }//end class
}



