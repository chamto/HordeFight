using System;
using System.Collections.Generic;
using System.Linq;
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
        public Vector3 _nDir = ConstV.v3_zero;

        public bool _isUpTile = false; //챔프보다 위에 있는 타일 (TileMap_StructUp)
        public bool _isStructTile = false; //구조물 타일인지 나타냄

        public Vector3 _pos3d_center = ConstV.v3_zero;    //타일의 중앙 위치

        public Vector3Int _pos2d = Vector3Int.zero;
        public int _pos1d = -1; //타일의 1차원 위치값 

        public LineSegment3 _line;
        public Vector3 _line_center = ConstV.v3_zero; //선분의 중앙 위치 

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
    {
    }

    public struct Vector3Int_TwoKey
    {
        public Vector3Int a;
        public Vector3Int b;

        public Vector3Int_TwoKey(Vector3Int k0 , Vector3Int k1)
        {
            //(k0 , k1) 과 (k1 , k0) 의 데이터를 같게 한다 
            if(k0.GetHashCode() < k1.GetHashCode())
            {
                a = k0;
                b = k1;
            }else
            {
                a = k1;
                b = k0;
            }

        }

        //ref : https://devlog-wjdrbs96.tistory.com/243
        override public int GetHashCode()
        {

            int hash = 7;
            hash = 31 * hash + a.GetHashCode();
            hash = 31 * hash + b.GetHashCode();


            return hash;
        }
    }

    public class ArcTile
    {
        public bool is_arc = false;

        public float angle = 0;
        public float r_factor = 0;
        public Vector3 inter_pt = Vector3.zero;
        public Vector3 dir = Vector3.zero;

        public CellSpace cell_0 = null;
        public CellSpace cell_1 = null;
        public LineSegment3 line_0;
        public LineSegment3 line_1;

        public void Init(CellSpace c0, CellSpace c1)
        {

            //같은 방향 , 정반대 방향이면 처리하지 못한다 
            if (Vector3.Cross(c0._line.direction, c1._line.direction).sqrMagnitude <= float.Epsilon)
            {
                is_arc = false;
                return;
            }

            cell_0 = c0;
            cell_1 = c1;

            Vector3 inter_pt0, inter_pt1;
            Line3.ClosestPoints(out inter_pt0, out inter_pt1, new Line3(c0._line.origin, c0._line.direction), new Line3(c1._line.origin, c1._line.direction));
            inter_pt = inter_pt0;

            line_0 = c0._line;
            line_1 = c1._line;
            if ((line_0.origin - inter_pt0).sqrMagnitude > (line_0.last - inter_pt0).sqrMagnitude)
            {
                line_0.origin = c0._line.last;
                line_0.last = c0._line.origin;
            }
            if ((line_1.origin - inter_pt0).sqrMagnitude > (line_1.last - inter_pt0).sqrMagnitude)
            {
                line_1.origin = c1._line.last;
                line_1.last = c1._line.origin;
            }



            angle = Geo.AngleSigned(line_0.direction, line_1.direction, Vector3.up);
            if (0 > angle) angle *= -1;

            //호에 완전포함 시키기 위한 factor 값을 구한다 
            r_factor = 1 / (float)Math.Sin(Mathf.Deg2Rad * angle * 0.5f);
            dir = VOp.Normalize(line_0.direction.normalized + line_1.direction.normalized);


            is_arc = true;

        }

        public Vector3 Pos(float radius)
        {
            return inter_pt + dir * (radius * r_factor);
        }

        public void AddDrawQ(float RADIUS, Color color)
        {
            if (false == is_arc) return;
            Vector3 newPos = Pos(RADIUS);

            DebugWide.AddDrawQ_Circle(newPos, RADIUS, color);
            DebugWide.AddDrawQ_Circle(inter_pt, 0.1f, color);
            DebugWide.AddDrawQ_Line(inter_pt, inter_pt + dir * 3, color);

            //-------------------------------
            //계산영역을 벗어나는지 검사 

            Vector3 cp_0 = Line3.ClosestPoint(line_0.origin, line_0.direction, newPos);
            Vector3 perp_0 = inter_pt + cell_0._nDir * RADIUS;
            Vector3 cp_1 = Line3.ClosestPoint(line_1.origin, line_1.direction, newPos);
            Vector3 perp_1 = inter_pt + cell_1._nDir * RADIUS;

            DebugWide.AddDrawQ_Line(cp_0, newPos, Color.black);
            DebugWide.AddDrawQ_Line(inter_pt, perp_0, Color.black);
            DebugWide.AddDrawQ_Line(perp_0, newPos, Color.black);

            DebugWide.AddDrawQ_Line(cp_1, newPos, Color.black);
            DebugWide.AddDrawQ_Line(inter_pt, perp_1, Color.black);
            DebugWide.AddDrawQ_Line(perp_1, newPos, Color.black);

            DebugWide.AddDrawQ_Line(line_0.origin, line_1.origin, Color.green);
        }
    }

    public class GridManager
    {
        public Grid _grid = null;
        public Tilemap _tilemap_struct = null;
        public Dictionary<Vector3Int, CellSpace> _structTileList = new Dictionary<Vector3Int, CellSpace>(new Vector3IntComparer());
        public Dictionary<Vector3Int, BoundaryTileList> _boundaryList = new Dictionary<Vector3Int, BoundaryTileList>(new Vector3IntComparer());
        public Dictionary<Vector3Int_TwoKey, ArcTile> _arcTileList = new Dictionary<Vector3Int_TwoKey, ArcTile>();

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
                structTile._pos2d = new Vector3Int(XY_2d.x, XY_2d.y , 0);
                structTile._pos1d = this.ToPosition1D(XY_2d, _tileBlock_width_size); 
                structTile._eDir = ruleTile._tileDataMap.GetDirection8(XY_2d);
                structTile._nDir = Misc.GetDir8_Normal3D_AxisY(structTile._eDir);

                //방향이 없는 덮개타일은 걸러낸다 
                if (eDirection8.none == structTile._eDir) continue;

                structTile._isUpTile = ruleTile._tileDataMap.Get_IsUpTile(XY_2d);
                structTile._isStructTile = true;

                LineSegment3 line;
                CalcBoundaryLine(structTile, out line);
                structTile._line = line;
                structTile._line_center = line.origin + line.direction * 0.5f;

                _structTileList.Add(XY_2d, structTile);

            }

            DebugWide.LogBlue("LoadTile : " + _structTileList.Count + "  -  TileMap_Struct RefreshAllTiles");


            //확장영역에 구조물 경계선 추가 
            Load_StructLine();

            //확장영역 정보중 각을 이루고 있는 쌍을 찾아 계산 
            Load_ArcTileInfo();
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

                    if (false == diag)
                    {
                        //대각이 아닌 경우 한칸더 이동한 방향에 추가 
                        //key = t.Key + dir1 * 2;
                        //_boundaryList.TryGetValue(key, out list);
                        //if (null == list)
                        //{
                        //    list = new BoundaryTileList();
                        //    _boundaryList.Add(key, list);
                        //}
                        //info.isBoundary = true;
                        //list.AddLast(info);
                    }
                    else
                    {
                        //대각인 경우 방향을 수평,수직으로 분리해서 추가 

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

            //대각타일이 첫번째로 가게 정렬시킨다 
            foreach (KeyValuePair<Vector3Int, BoundaryTileList> t in _boundaryList)
            {
                if(1 < t.Value.Count)
                {

                     
                    BoundaryTile[] link = t.Value.OrderByDescending(x => x.cell._specifier).ToArray();
                    _boundaryList[t.Key].Clear();
                    //_boundaryList[t.Key].CopyTo(link, 0); //안됨 , 수동으로 넣어주기 

                    foreach (BoundaryTile bt in link)
                    {
                        _boundaryList[t.Key].AddLast(bt);
                        //DebugWide.LogBlue(ct+"  "+ bt.cell._specifier + "  " + bt.cell._eDir);
                        //ct++;
                    }

                    //int ct = 0;
                    //foreach (BoundaryTile bt in _boundaryList[t.Key])
                    //{
                    //    DebugWide.LogBlue(ct + "  " + bt.cell._specifier + "  " + bt.cell._eDir);
                    //    ct++;
                    //}
                }

            }
        }

        public void Load_ArcTileInfo()
        {
            CellSpace c0, c1;
            foreach (KeyValuePair<Vector3Int, BoundaryTileList> list in _boundaryList)
            {
                if (1 >= list.Value.Count) continue;

                //list의 갯수가 최대 3개 까지만 처리한다
                if(2 == list.Value.Count)
                {
                    c0 = list.Value.ElementAt(0).cell;
                    c1 = list.Value.ElementAt(1).cell;

                    ArcTile arcTile = new ArcTile();
                    arcTile.Init(c0, c1);
                    if(arcTile.is_arc)
                    {
                        ArcTile outInfo;    
                        if(false == _arcTileList.TryGetValue(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), out outInfo))
                        {
                            _arcTileList.Add(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), arcTile);
                        }

                    }
                }

                if (3 == list.Value.Count)
                {
                    c0 = list.Value.ElementAt(0).cell;
                    c1 = list.Value.ElementAt(1).cell;

                    ArcTile arcTile = new ArcTile();
                    arcTile.Init(c0, c1);
                    if (arcTile.is_arc)
                    {
                        ArcTile outInfo;
                        if (false == _arcTileList.TryGetValue(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), out outInfo))
                        {
                            _arcTileList.Add(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), arcTile);
                        }
                    }
                    //----

                    c0 = list.Value.ElementAt(1).cell;
                    c1 = list.Value.ElementAt(2).cell;
                    arcTile = new ArcTile();
                    arcTile.Init(c0, c1);
                    if (arcTile.is_arc)
                    {
                        ArcTile outInfo;
                        if (false == _arcTileList.TryGetValue(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), out outInfo))
                        {
                            _arcTileList.Add(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), arcTile);
                        }
                    }
                    //---

                    c0 = list.Value.ElementAt(2).cell;
                    c1 = list.Value.ElementAt(0).cell;
                    arcTile = new ArcTile();
                    arcTile.Init(c0, c1);
                    if (arcTile.is_arc)
                    {
                        ArcTile outInfo;
                        if (false == _arcTileList.TryGetValue(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), out outInfo))
                        {
                            _arcTileList.Add(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), arcTile);
                        }
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
            //foreach(ArcTile arc in _arcTileList.Values)
            //{
            //    if(true == arc.is_arc)
            //    {
            //        arc.AddDrawQ(1, Color.yellow); 
            //    }
            //}

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
                if (1 == info1.Value.Count) continue;

                Vector3 pos = ToPosition3D_Center(new Vector3Int(info1.Key.x, info1.Key.y,0));
                DebugWide.PrintText(pos, Color.black, "" + info1.Value.Count);

                foreach (BoundaryTile info2 in info1.Value)
                {
                    if (true == info2.isBoundary)
                    {
                        //DebugWide.DrawLine(info2.cell._pos3d_center, pos, Color.white);
                        DebugWide.DrawLine(info2.cell._line.origin + info2.cell._line.direction * 0.5f, pos, Color.gray);
                    }
                    //else
                    {
                        if (CellSpace.Specifier_DiagonalFixing == info2.cell._specifier)
                            DebugWide.DrawCircle(info2.cell._line.origin, 0.1f, Color.red);

                        info2.cell._line.Draw(Color.white);
                    }

                }

            }
        }


        public Vector3 GetBorder_StructTile(Vector3 srcPos, CellSpace structTile)
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
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z -= size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z += size;
                        line3.last = temp;

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

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z += size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z -= size;
                        line3.last = temp;

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

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z += size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z -= size;
                        line3.last = temp;

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

                        //중심점 방향으로 부터 반대방향이면 충돌영역에 도달한것이 아니다 
                        if (0 < Vector3.Dot(centerToSrc_dir, push_dir)) return srcPos;
                        temp = structTile._pos3d_center;
                        temp.x -= size;
                        temp.z -= size;
                        line3.origin = temp;

                        temp = structTile._pos3d_center;
                        temp.x += size;
                        temp.z += size;
                        line3.last = temp;

                        srcPos = line3.ClosestPoint(srcPos);
                    }
                    break;

            }

            return srcPos;

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

                    if (true == Geo.IntersectLineSegment(srcPos, RADIUS, info.cell._line.origin, info.cell._line.last))
                    {

                        Vector3 cp = info.cell._line.ClosestPoint(srcPos);
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
            const int MAX_COUNT = 2; //2개 이상의 타일을 검사시 순간이동 현상발생  
            //도달할 수 없는 위치인데도 경로상의 타일영역에 들어오는 첫 번째 구조타일을 무조건 가져오기 때문에 생기는 문제임 

            Vector3 dir = srcPos - oldPos;
            //structTile = Find_FirstStructTile(oldPos, srcPos, MAX_COUNT); //이렇게 사용하면 안됨 , 한개타일을 못 벗어나는 길이의 경우 경로상의 구조타일을 못 구한다 
            CellSpace structTile = Find_FirstStructTile(oldPos, oldPos + dir * 100, MAX_COUNT); 
            if (null != structTile)
            {

                bool calc = false;
                Vector3 cp = structTile._line.ClosestPoint(srcPos);
                //Vector3 cp = LineInterPos(oldPos, srcPos, structTile._line.origin, structTile._line.last);
                Vector3 cpToSrc = srcPos - cp;
                //Vector3 push_dir = Misc.GetDir8_Normal3D(srcPos - cp); //srcPos 가 잘못계산되는 문제 발생 
                Vector3 push_dir = VOp.Normalize(cpToSrc);


                //DebugWide.LogBlue("  Collision_FirstStructTile 1 : " + "  old : " + oldPos + " src : " + srcPos + "  len : " + dir.magnitude);

                //지형타일을 넘어간 경우 
                //if (Vector3.Dot(dir, cpToSrc) > 0) //잘못된 계산 
                //if (Vector3.Dot((oldPos - cp), cpToSrc) < 0) //삐죽하게 튀어나온 부분에서 처리 못해줌
                if (Vector3.Dot(structTile._nDir, cpToSrc) < 0)
                {
                    //지형과 같은 방향일 때는 처리하지 않는다. 적당히 작은값과 비교하여 걸러낸다 
                    //지형과 같은 방향일 때 방향이 바뀌어 튀는 현상 발생함 
                    //DebugWide.LogRed("calc - ================================================== " + Vector3.Cross(structTile._line.direction, dir).sqrMagnitude);
                    if (Vector3.Cross(structTile._line.direction, dir).sqrMagnitude > 0.1f)
                    {
                        calc = true;
                        push_dir *= -1;
                        //DebugWide.LogRed("calc true ================================================== ");
                    }


                }


                //지형과 원이 겹친경우 
                if (cpToSrc.sqrMagnitude <= RADIUS*RADIUS)
                {
                    calc = true;
                }

                if(calc)
                {
                    srcPos = cp + push_dir * RADIUS;

                    Vector3 newPos;
                    if(true == CalcArcFullyPos2(structTile, srcPos, cp ,RADIUS, out newPos))
                    {
                        srcPos = newPos;
                    }
                }

                //DebugWide.LogBlue("  Collision_FirstStructTile 2 : cent: " + structTile._pos3d_center + " cp: " + cp + " dir: " + push_dir + " calc: " + srcPos);

                //DebugWide.AddDrawQ_Circle(srcPos, 0.1f, Color.green);
                //DebugWide.AddDrawQ_Line(structTile._line.origin, structTile._line.last, Color.green);
                //DebugWide.AddDrawQ_Line(cp, srcPos, Color.green);
                ////DebugWide.AddDrawQ_Circle(cp, 0.3f, Color.white);
                //DebugWide.AddDrawQ_Circle(structTile._pos3d_center, 0.5f, Color.green);
            }

            return srcPos;
        }

        public Vector3 LineInterPos(Vector3 oldPos1, Vector3 srcPos1, Vector3 oldPos2, Vector3 srcPos2)
        {
            float x, z, m1 , m2;
            if(Misc.IsZero(srcPos2.x - oldPos2.x))
            {

                m1 = (srcPos1.z - oldPos1.z) / (srcPos1.x - oldPos1.x);
                m1 = 1 / m1;
                m2 = 0;
                //x = -oldPos1.z + oldPos2.z;
                //z = srcPos1.z;

            }else
            {
                m1 = (srcPos1.z - oldPos1.z) / (srcPos1.x - oldPos1.x); //기울기 계산
                m2 = (srcPos2.z - oldPos2.z) / (srcPos2.x - oldPos2.x); //기울기 계산 
                m1 = 1 / m1;
                m2 = 1 / m2;
            }



            //x = (m1 * oldPos1.x - m2 * oldPos2.x - oldPos1.z + oldPos2.z) / (m1 - m2);
            //z = m1 * (x - srcPos1.x) + srcPos1.z;
            //x = ((y - y1) / m + x1);

            z = (m1 * oldPos1.z - m2 * oldPos2.z - oldPos1.x + oldPos2.x) / (m1 - m2);
            x = m1 * (z - srcPos1.z) + srcPos1.x;


            //---------
            Vector3 a, b;
            Line3.ClosestPoints(out a, out b, new Line3(oldPos1, srcPos1-oldPos1), new Line3(oldPos2, srcPos2-oldPos2));
            //DebugWide.DrawCircle(a, 0.1f, Color.red);
            //DebugWide.LogBlue(a + "  " + b); ;
            return a;
            //---------

            DebugWide.DrawCircle(new Vector3(x, 0, z), 0.1f, Color.red);
            DebugWide.LogBlue(x + "  " + z);

            return new Vector3(x,0,z); 
        }

        public bool CalcArcFullyPos(CellSpace firstTile, Vector3 srcPos, Vector3 closestPt, float RADIUS , out Vector3 newPos)
        {
            newPos = srcPos;
            Vector3Int pos_2d = ToPosition2D(srcPos);

            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list))
            {
                DebugWide.LogRed("0-----------");
                return false;
            }


            float MAX_SQRLEN = 5*5;
            CellSpace c0 = null, c1 = null;
            if (2 == list.Count)
            {
                c0 = list.ElementAt(0).cell;
                c1 = list.ElementAt(1).cell;
            }
            else if (3 == list.Count)
            {

                foreach(BoundaryTile cpa in list)
                {
                    if (cpa.cell == firstTile)
                    {
                        c0 = firstTile;
                        continue; 
                    }
                    //float sqrlen = (cpa.cell._pos3d_center - c0._pos3d_center).sqrMagnitude;
                    //float sqrlen = (cpa.cell._pos3d_center - srcPos).sqrMagnitude;
                    //Vector3 center = cpa.cell._line.origin + cpa.cell._line.direction * 0.5f;
                    float sqrlen = (cpa.cell._line_center - srcPos).sqrMagnitude;
                    DebugWide.LogRed(cpa.cell._eDir +"   "+ sqrlen);
                    //DebugWide.AddDrawQ_Circle(center, 0.1f, Color.red);
                    if (MAX_SQRLEN > sqrlen)
                    {
                        c1 = cpa.cell;
                        MAX_SQRLEN = sqrlen;
                    }

                }
            }
            else
            {
                DebugWide.LogRed("1-----------");
                return false;
            }

            if(null == c0)
            {
                DebugWide.LogRed("2-----------");
                return false;
            }

            //if (c0._eDir == c1._eDir) return srcPos;

            Vector3 inter_pt0, inter_pt1;
            Line3.ClosestPoints(out inter_pt0, out inter_pt1, new Line3(c0._line.origin, c0._line.direction), new Line3(c1._line.origin, c1._line.direction));


            LineSegment3 line_0 = c0._line;
            LineSegment3 line_1 = c1._line;
            if((line_0.origin - inter_pt0).sqrMagnitude > (line_0.last - inter_pt0).sqrMagnitude)
            {
                line_0.origin = c0._line.last;
                line_0.last = c0._line.origin;
            }
            if ((line_1.origin - inter_pt0).sqrMagnitude > (line_1.last - inter_pt0).sqrMagnitude)
            {
                line_1.origin = c1._line.last;
                line_1.last = c1._line.origin;
            }

            //-------------------------------
            //지형의 최소길이 보다 반지름이 작은 경우 처리안함 
            if ((line_0.origin - line_1.origin).sqrMagnitude >= (RADIUS * 2 * RADIUS * 2)) //지름의 제곱길이 비교 
            {
                DebugWide.LogRed("3-----------");
                return false;
            }

            //같은 방향 , 정반대 방향이면 처리하지 못한다 
            if (Vector3.Cross(line_0.direction, line_1.direction).sqrMagnitude <= float.Epsilon)
            {
                DebugWide.LogRed("4-----------");
                return false; 
            }

            float angle = Geo.AngleSigned(line_0.direction, line_1.direction, Vector3.up);
            if (0 > angle) angle *= -1;

            //호에 완전포함 시키기 위한 factor 값을 구한다 
            float factor = RADIUS / (float)Math.Sin(Mathf.Deg2Rad * angle * 0.5f);
            Vector3 ndir = VOp.Normalize(line_0.direction.normalized + line_1.direction.normalized);
            newPos = inter_pt0 + ndir * factor ;


            DebugWide.AddDrawQ_Circle(newPos, RADIUS, Color.yellow);
            DebugWide.AddDrawQ_Circle(inter_pt0, 0.1f, Color.yellow);
            DebugWide.AddDrawQ_Line(inter_pt0, inter_pt0 + ndir * 3, Color.yellow);

            //-------------------------------
            //계산영역을 벗어나는지 검사 

            Vector3 cp_0 = Line3.ClosestPoint(line_0.origin, line_0.direction, newPos);
            Vector3 cpp_0 = Line3.ClosestPoint(line_0.origin, line_0.direction, srcPos);
            Vector3 perp_0 = inter_pt0 + c0._nDir * RADIUS;
            Vector3 cp_1 = Line3.ClosestPoint(line_1.origin, line_1.direction, newPos);
            Vector3 cpp_1 = Line3.ClosestPoint(line_1.origin, line_1.direction, srcPos);
            Vector3 perp_1 = inter_pt0 + c1._nDir * RADIUS;

            DebugWide.AddDrawQ_Line(cp_0, newPos, Color.black);
            DebugWide.AddDrawQ_Line(inter_pt0, perp_0, Color.black);
            DebugWide.AddDrawQ_Line(perp_0, newPos, Color.black);

            DebugWide.AddDrawQ_Line(cp_1, newPos, Color.black);
            DebugWide.AddDrawQ_Line(inter_pt0, perp_1, Color.black);
            DebugWide.AddDrawQ_Line(perp_1, newPos, Color.black);

            DebugWide.AddDrawQ_Line(line_0.origin, line_1.origin, Color.green);



            DebugWide.LogBlue("a - "+(inter_pt0 - cp_0).magnitude + "    " + (inter_pt0 - cpp_0).magnitude + "  " + c0._eDir);
            DebugWide.LogBlue("b - " + (inter_pt0 - cp_1).magnitude + "    " + (inter_pt0 - cpp_1).magnitude + "  " + c1._eDir);

            //계산영역 검사 
            Vector3 clpt_newpos = firstTile._line.ClosestPoint(newPos);
            if ((inter_pt0 - closestPt).sqrMagnitude < (inter_pt0 - clpt_newpos).sqrMagnitude)
            {

                DebugWide.LogRed((inter_pt0 - closestPt).magnitude + "   " + (inter_pt0 - clpt_newpos).magnitude + "  " + srcPos + "   " + newPos + "  " + inter_pt0 + "  " + closestPt + "  " + clpt_newpos);
                return true;
            }


            DebugWide.LogRed("5-----------");

            return false;
        }

        public bool CalcArcFullyPos2(CellSpace firstTile, Vector3 srcPos, Vector3 closestPt, float RADIUS, out Vector3 newPos)
        {
            newPos = srcPos;
            Vector3Int pos_2d = ToPosition2D(srcPos);

            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list))
            {
                //DebugWide.LogRed("0-----------");
                return false;
            }


            float MAX_SQRLEN = 5 * 5;
            CellSpace c0 = null, c1 = null;
            if (2 == list.Count)
            {
                c0 = list.ElementAt(0).cell;
                c1 = list.ElementAt(1).cell;
            }
            else if (3 == list.Count)
            {

                foreach (BoundaryTile cpa in list)
                {
                    if (cpa.cell == firstTile)
                    {
                        c0 = firstTile;
                        continue;
                    }

                    float sqrlen = (cpa.cell._line_center - srcPos).sqrMagnitude;
                    //DebugWide.LogRed(cpa.cell._eDir + "   " + sqrlen);
                    //DebugWide.AddDrawQ_Circle(center, 0.1f, Color.red);
                    if (MAX_SQRLEN > sqrlen)
                    {
                        c1 = cpa.cell;
                        MAX_SQRLEN = sqrlen;
                    }

                }
            }
            else
            {
                //DebugWide.LogRed("1-----------");
                return false;
            }

            if (null == c0)
            {
                //DebugWide.LogRed("2-----------");
                return false;
            }

            ArcTile arcTile = null;
            if(false == _arcTileList.TryGetValue(new Vector3Int_TwoKey(c0._pos2d, c1._pos2d), out arcTile))
            {
                //DebugWide.LogRed("3-----------");
                return false; 
            }
            newPos = arcTile.Pos(RADIUS);

            //-------------------------------
            //지형의 최소길이 보다 반지름이 작은 경우 처리안함 
            if ((arcTile.line_0.origin - arcTile.line_1.origin).sqrMagnitude >= (RADIUS * 2 * RADIUS * 2)) //지름의 제곱길이 비교 
            {
                //DebugWide.LogRed("4-----------");
                return false;
            }

            //계산영역 검사 
            Vector3 clpt_newpos = firstTile._line.ClosestPoint(newPos);
            if ((arcTile.inter_pt - closestPt).sqrMagnitude < (arcTile.inter_pt - clpt_newpos).sqrMagnitude)
            {

                //DebugWide.LogRed((arcTile.inter_pt - closestPt).magnitude + "   " + (arcTile.inter_pt - clpt_newpos).magnitude + "  " + srcPos + "   " + newPos + "  " + "  " + closestPt + "  " + clpt_newpos);
                //arcTile.AddDrawQ(RADIUS, Color.yellow);

                return true;
            }

            return false;
        }


        public Vector3 Collision_StructLine_Test3(Vector3 oldPos, Vector3 srcPos, float RADIUS , Proto_AI_2.Vehicle vh)
        {
            bool isCalcArc;
            srcPos = Collision_FirstStructTile(oldPos, srcPos, RADIUS); //벽통과 되는 경우 통과되기 전위치를 반환한다 

            Vector3Int pos_2d = ToPosition2D(srcPos);

            vh._stop = false;

            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list)) return srcPos;

            //DebugWide.LogBlue(" === 2d : " + pos_2d + "  count : " + list.Count);


            int count = 0;
            int interCount = 0;
            foreach (BoundaryTile info in list)
            {
                //if (firstTile == info.cell) continue; //Collision_FirstStructTile 에서 계산된 값이 구조타일과 반지름이 겹쳐있는 경우가 있기 때문에 넘기는 처리를 하면 안된다 

                //DebugWide.AddDrawQ_Line(info.cell._line.origin, info.cell._line.last, Color.white);
                //Vector3Int cellpos = ToPosition2D(info.cell._pos3d_center);
                //DebugWide.AddDrawQ_Text(new Vector3(cellpos.x,0,cellpos.y), "" + new Vector2Int(cellpos.x,cellpos.y), Color.white);

                //주변경계타일인 경우
                if (true == info.isBoundary)
                {

                    if (Geo.IntersectLineSegment(srcPos, RADIUS, info.cell._line.origin, info.cell._line.last))
                    {
                        interCount++;


                        Vector3 cp = info.cell._line.ClosestPoint(srcPos);

                        //DebugWide.AddDrawQ_Circle(cp, 0.3f, Color.black);

                        Vector3 n = VOp.Normalize(srcPos - cp);
                        //Vector3 n = Misc.GetDir8_Normal3D_AxisY(info.cell._eDir); //경계에서 튀는 버그 발생 
                        srcPos = cp + n * RADIUS;

                        //DebugWide.LogBlue(count + "  boundary  " + cellpos + "  cp: " + cp + "  " + n + "  " + srcPos);
                        //DebugWide.AddDrawQ_Line(cp, srcPos, Color.black);

                    }

                }
                //경계타일인 경우 
                else
                {
                    srcPos = GetBorder_StructTile(srcPos, RADIUS, info.cell);

                    //DebugWide.LogBlue(count);
                }

                count++;
            }

            if (2 <= count && 2 <= interCount)
            {
                //반지름이 0.99일때 터널통과 현상을 막기위해 최저속도로 설정한다 
                vh._stop = true;
            }



            return srcPos;


        }



        public Vector3 Collision_StructLine_Test4(Vector3 oldPos, Vector3 srcPos, float RADIUS, Proto_AI_2.Vehicle vh)
        {
            CellSpace firstTile;
            //srcPos = Collision_FirstStructTile(oldPos, srcPos, RADIUS , out firstTile); //벽통과 되는 경우 통과되기 전위치를 반환한다 

            Vector3Int pos_2d = ToPosition2D(srcPos);


            BoundaryTileList list = null;
            if (false == _boundaryList.TryGetValue(pos_2d, out list)) return srcPos;

            DebugWide.LogBlue(" === 2d : " + pos_2d + "  count : " + list.Count);


            int count = 0;
            Vector3 prevPos = srcPos;
            foreach (BoundaryTile info in list)
            {
                //if (firstTile == info.cell) continue;

                DebugWide.AddDrawQ_Line(info.cell._line.origin, info.cell._line.last, Color.white);
                Vector3Int cellpos = ToPosition2D(info.cell._pos3d_center);
                DebugWide.AddDrawQ_Text(new Vector3(cellpos.x, 0, cellpos.y), "" + new Vector2Int(cellpos.x, cellpos.y), Color.white);


                //if (true == info.isBoundary)
                if (false == info.isBoundary)
                {

                    srcPos = GetBorder_StructTile(srcPos, info.cell);

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

                //DebugWide.AddDrawQ_Circle(nextPos, 0.5f, Color.gray);
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



