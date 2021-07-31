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
            if (CellSpace.Specifier_DiagonalFixing == cell._specifier) return false;

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
            foreach (KeyValuePair<Vector3Int, BoundaryTileList> info1 in _boundaryList)
            {

                Vector3 pos = ToPosition3D_Center(new Vector3Int(info1.Key.x, info1.Key.y,0));
                //DebugWide.PrintText(pos, Color.black, "" + info1.Value.Count);

                foreach (BoundaryTile info2 in info1.Value)
                {
                    if (true == info2.isBoundary)
                    {
                        DebugWide.DrawLine(info2.cell._pos3d_center, pos, Color.white);
                    }
                    else
                    {
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

        public void Draw_line_equation3(float x1, float y1, float x2, float y2)
        {
            float m;
            float x, y;
            float temp;

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

        //public void Draw_line_equation2(float x1, float y1, float x2, float y2)
        //{
        //    float m;
        //    float x, y;
        //    float temp;

        //    //int sign = VOp.Sign_ZX(Vector3.right, new Vector3((x2 - x1), 0, (y2 - y1)));

        //    //if (x1 == x2) //수직선인 경우 
        //    //{
        //    //    if (y1 > y2)
        //    //    {
        //    //        //swap
        //    //        temp = y1;
        //    //        y1 = y2;
        //    //        y2 = temp;
        //    //    }
        //    //    for (y = y1; y <= y2; y++)
        //    //    {
        //    //        DebugWide.DrawCircle(new Vector3(x1, 0, y), 0.1f, Color.green);
        //    //    }
        //    //    return;
        //    //}
        //    m = (float)(y2 - y1) / (float)(x2 - x1); //기울기 계산
        //    if (-1 < m && m < 1) //x축이 독립축 
        //    {
        //        if (x1 > x2)
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
        //        //x = (int)x1;
        //        //y = (int)y1;
        //        x = (int)x1+1;
        //        y = y1;
        //        Vector3 nextTile;

        //        for (; x <= x2; x++)
        //        {
        //            //y = (int)(m * (x - x1) + y1 + 0.5f);
        //            y = (m * (x - x1) + y1 );
        //            DebugWide.DrawCircle(new Vector3(x, 0, y), 0.1f, Color.green);
        //            //DebugWide.DrawCube(new Vector3(x + 0.5f, 0, y + 0.5f), new Vector3(1f, 0, 1f), Color.green);
        //            Vector3 origin_center = ToPosition3D_Center(new Vector3(x, 0, y));
        //            //DebugWide.DrawCube(origin_center, new Vector3(1f, 0, 1f), Color.green);
        //            DebugWide.DrawCircle(origin_center, 0.2f, Color.green);

        //            //------------------
        //            Vector3 centerToTarget = new Vector3(x,0,y) - origin_center;
        //            //Vector3 centerToTarget = new Vector3(x, 0, y) - prev_center;

        //            Vector3 dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);
        //            nextTile = origin_center + dir4n;

        //            DebugWide.DrawCube(nextTile, new Vector3(1f, 0, 1f), Color.magenta);

        //        }
        //    }
        //    else //y축이 독립축 
        //    { }
        //}

        //public void Draw_line_equation(int x1, int y1, int x2, int y2)
        //{
        //    float m;
        //    int x, y;
        //    int temp;
        //    if (x1 == x2) //수직선인 경우 
        //    {
        //        if (y1 > y2)
        //        {
        //            //swap
        //            temp = y1;
        //            y1 = y2;
        //            y2 = temp;
        //        }
        //        for (y = y1; y <= y2; y++)
        //        {
        //            DebugWide.DrawCircle(new Vector3(x1, 0, y), 0.1f, Color.green);
        //        }
        //        return;
        //    }
        //    m = (float)(y2 - y1) / (float)(x2 - x1); //기울기 계산
        //    if (-1 < m && m < 1) //x축이 독립축 
        //    {
        //        if (x1 > x2)
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
        //        y = y1;
        //        for (x = x1; x <= x2; x++)
        //        {
        //            y = (int)(m * (x - x1) + y1 + 0.5f);
        //            DebugWide.DrawCircle(new Vector3(x, 0, y ), 0.1f, Color.green);
        //            DebugWide.DrawCube(new Vector3(x+0.5f, 0, y+0.5f), new Vector3(1f, 0, 1f), Color.green);

        //        }
        //    }
        //    else //y축이 독립축 
        //    { }
        //}

        //public void Draw_line_incremental(int x1, int y1, int x2, int y2)
        //{
        //    float m, x, y;
        //    int temp;
        //    if(x1 == x2) //수직선인 경우 
        //    {
        //        if(y1 > y2)
        //        {
        //            //swap
        //            temp = y1;
        //            y1 = y2;
        //            y2 = temp;
        //        }
        //        for(y = y1;y<=y2;y++)
        //        {
        //            DebugWide.DrawCircle(new Vector3(x1, 0, y), 0.1f, Color.green);
        //        }
        //        return;
        //    }
        //    m = (float)(y2 - y1) / (float)(x2 - x1); //기울기 계산
        //    if(-1 < m && m < 1) //x축이 독립축 
        //    {
        //        if(x1 > x2)
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
        //        y = y1;
        //        for(x = x1; x <= x2; x++)
        //        {
        //            DebugWide.DrawCircle(new Vector3(x, 0, (int)(y + 0.5f)), 0.1f, Color.green); //실수값인 y를 반올림
        //            //DebugWide.DrawCircle(new Vector3(x, 0, y ), 0.1f, Color.green);
        //            y += m;
        //        }
        //    }
        //    else //y축이 독립축 
        //    { }
        //}

        //public void Draw_line_midpoint2(float x1, float y1, float x2, float y2)
        //{

        //    float x=0, y=0;
        //    float delta_x, delta_y, d;
        //    int inc;
        //    float Einc, NEinc;
        //    float temp;

        //    if (Math.Abs(x2-x1) > Math.Abs(y2-y1)) // |기울기| < 1
        //    {
        //        if(x > x2)
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
        //        inc = (y2 > y1) ? 1 : -1;
        //        delta_x = x2 - x1;
        //        delta_y = Math.Abs(y2 - y1);
        //        d = 2 * delta_y - delta_x;
        //        Einc = 2 * delta_y;
        //        NEinc = 2 * (delta_y - delta_x);
        //        x = (int)x1; 
        //        y = y1;

        //        Vector3 origin_center;
        //        Vector3 prev_pos = new Vector3(x,0,y);
        //        while (x<x2)
        //        {
        //            x++;

        //            origin_center = ToPosition3D_Center(new Vector3(x, 0, y));
        //            DebugWide.DrawCube(origin_center, new Vector3(1f, 0, 1f), Color.green);

        //            if (d > 0)
        //            {
        //                y += inc;
        //                d += NEinc;
        //                origin_center = ToPosition3D_Center(new Vector3(x, 0, y));
        //                DebugWide.DrawCube(origin_center, new Vector3(1f, 0, 1f), Color.green);
        //            }
        //            else
        //            {
        //                d += Einc;
        //            }
        //            DebugWide.DrawCircle(new Vector3(x, 0, y), 0.1f, Color.green);

        //            DebugWide.DrawLine(prev_pos, new Vector3(x, 0, y), Color.black);
        //            prev_pos = new Vector3(x, 0, y);
        //        }
        //    }
        //    else
        //    { }
        //}

        public CellSpace Find_FirstStructTile3(Vector3 origin_3d, Vector3 target_3d)
        {

            Vector3 nextTile = origin_3d;
            int count = 0;
            while ((nextTile - target_3d).sqrMagnitude > 1)
            {
                if (count > 10) break;
                count++;

                //구조타일을 발견하면 바로 반환 
                CellSpace structTile = GetStructTile(nextTile);
                if (null != structTile)
                    return structTile;

            }

            return null;
        }

        public CellSpace Find_FirstStructTile2(Vector3 origin_3d, Vector3 target_3d)
        {

            Vector3 toTarget = target_3d - origin_3d;
            Vector3 origin_center;
            Vector3 centerToTarget;

            Vector3 dir4n = Vector3.zero;
            Vector3 nextTile = origin_3d;

            Line3 line = new Line3(origin_3d, toTarget);

            int count = 0;
            while((nextTile- target_3d).sqrMagnitude > 1)
            {
                if (count > 10) break;

                origin_center = ToPosition3D_Center(nextTile);
                centerToTarget = target_3d - origin_center;

                dir4n = Misc.GetDir4_Normal3D_Y(centerToTarget);
                nextTile = origin_center + dir4n;

                //DebugWide.DrawLine(origin_center, target_3d, Color.green);
                DebugWide.DrawCube(nextTile, new Vector3(1f, 0, 1f), Color.green);

                count++;
            }

            return null;
        }

        public CellSpace Find_FirstStructTile(Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {

            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.001f >= length_interval)
            {
                return null;
            }

            //Index2 origin_2d;
            Vector3 origin_3d_center = origin_3d;
            //origin_2d = ToPosition2D(origin_3d);
            //origin_3d_center = origin._cur_cell._pos3d_center;

            CellSpace structTile = null;

            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            //CellSpace structTile = GetCellSpace(origin_3d);
            //if (null != structTile && structTile._isStructTile)
            //{
            //    switch (structTile._eDir)
            //    {
            //        case eDirection8.leftUp:
            //        case eDirection8.leftDown:
            //        case eDirection8.rightUp:
            //        case eDirection8.rightDown:
            //            {
            //                //모서리 값으로 설정 
            //                Vector3Int dir = Misc.GetDir8_Normal2D(structTile._eDir);
            //                origin_3d_center.x += dir.x * _cellSize_x * 0.5f;
            //                origin_3d_center.z += dir.y * _cellSize_z * 0.5f;

            //                //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
            //            }
            //            break;
            //        default:
            //            {
            //                Vector3Int vd = Misc.GetDir8_Normal2D(structTile._eDir);
            //                origin_2d.x += vd.x;
            //                origin_2d.y += vd.y;
            //                origin_3d_center = ToPosition3D_Center(origin_2d);
            //            }
            //            break;
            //    }

            //}

            Vector3 line = target_3d - origin_3d_center;
            Vector3 n = VOp.Normalize(line);
            //Vector3 n = Misc.GetDir360_Normal3D(line); //근사치 노멀값을 사용하면 목표에 도달이 안되는 무한루프에 
            //Vector3 n = line.normalized;

            //n *= length_interval; //미리 곱해 놓는다 


            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 0;
            Vector3 next = n * count;
            float lineSqr = line.sqrMagnitude;
            while (lineSqr > next.sqrMagnitude)
            {
                //최대 50회까지만 탐색한다 
                if (50 <= count)
                {
                    //DebugWide.LogBlue(n); //chamto test
                    return null;
                }
                next = origin_3d_center + next;
                DebugWide.DrawCircle(next, 0.1f, Color.green);
                DebugWide.DrawCube(ToPosition3D_Center(next), new Vector3(1f, 0, 1f), Color.green);

                structTile = GetStructTile(next);
                if (null != structTile)
                {
                    structTile.line.Draw(Color.red);
                    return structTile;
                }

                count++;
                next = n * count;

            }

            return null;
        }

    }//end class
}



