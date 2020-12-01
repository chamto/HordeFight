using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UtilGS9;


//========================================================
//==================     그리드 관리기     ==================
//========================================================
namespace HordeFight
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

    //한셀에 몇명의 캐릭터가 있는지 나타낸다 
    public class CellInfo : LinkedList<Being>
    {

        public Vector3Int _index = default(Vector3Int);
    }


    //장애물 정보 
    public class CellSpace
    {
        public const int Specifier_DiagonalFixing = 7; //대각고정 예약어

        public int _specifier = 0;
        public eDirection8 _eDir = eDirection8.none;
        //public Vector3      _v3Dir = ConstV.v3_zero;
        //public Index2       _i2Dir = ConstV.id2_zero;
        public bool _isUpTile = false; //챔프보다 위에 있는 타일 (TileMap_StructUp)
        public bool _isStructTile = false; //구조물 타일인지 나타냄

        public Vector3 _pos3d_center = ConstV.v3_zero;    //타일의 중앙 월드위치
        //public Vector2Int   _pos2d = ConstV.v2Int_zero;
        public Index2 _pos2d = ConstV.id2_zero;
        public int _pos1d = -1; //타일의 1차원 위치값 

        //==================================================
        //타일에 속해있는 객체의 링크
        public Being _head = null;
        public int _childCount = 0;


        public Being MatchRelation(Camp.eRelation relation, Being target)
        {
            Being getB = null;
            Being next = _head;
            while (null != (object)next)
            {
                getB = next;
                next = next._next_sibling;

                if ((object)getB == (object)target) continue;

                Camp.eRelation getR = SingleO.campManager.GetRelation(target._campKind, getB._campKind);
                //DebugWide.LogBlue(getR.ToString()); //chamto test
                if (relation == getR)
                    return getB; //찾았다 !!


            }

            return null;
        }


        //새로운 객체가 머리가 된다 
        public void AttachChild(Being newHead)
        {
            Being cur_child = _head;
            _head = newHead; // new head of list

            newHead._prev_sibling = null;
            newHead._next_sibling = cur_child;
            newHead._cur_cell = this;

            if (null != cur_child) cur_child._prev_sibling = newHead; // previous now this..

            _childCount++;

        }

        public void DetachChild(Being dst)
        {
            if (null == dst._cur_cell || null == dst._cur_cell._head || 0 == dst._cur_cell._childCount) return;

            Being prev = dst._prev_sibling;
            Being next = dst._next_sibling;
            if (null != prev)
            {
                prev._next_sibling = next;
                if (null != next) next._prev_sibling = prev;
            }
            else
            {
                //dst가 head 인 경우, 새로운 head 설정한다
                //_children = next;
                dst._cur_cell._head = next;

                if (null != next) next._prev_sibling = null;
            }

            dst._cur_cell._childCount--;
            dst._prev_sibling = null;
            dst._next_sibling = null;
            dst._cur_cell = null;
        }
    }


    public class CellSpacePartition
    {
        public const float ONE_METER = 1f; //타일 한개의 가로길이 , 월드 크기 

        private float _tileSize_w = ONE_METER * 1; //월드 크기 
        private float _tileSize_h = ONE_METER * 1; //월드 크기
        public const int MAP_WIDTH = 64;
        public const int MAP_HEIGHT = 64;
        private int _max_tileMapSize = MAP_WIDTH * MAP_HEIGHT;
        //private Vector2Int _tileMapSize = new Vector2Int(64, 64); //64*64 타일 갯수까지 사용한다 
        private CellSpace[] _cellInfo2Map = null;

        string __debugTemp = ConstV.STRING_EMPTY;
        public void Draw_CellInfo()
        {
            foreach (CellSpace cell in _cellInfo2Map)
            {
                if (0 != cell._childCount)
                {
                    __debugTemp = cell._childCount + "  "; //+ DebugPrint_Children(cell._children);
                    DebugWide.PrintText(cell._pos3d_center, Color.red, __debugTemp);
                    DebugWide.DrawCube(cell._pos3d_center, Vector3.one, Color.red);

                    //DebugWide.LogBlue(cell._pos2d + "  " + cell._childCount + "  " + DebugPrint_Children(cell._children));
                }
            }
        }
        public string DrawPrint_Children(Being be)
        {
            if (null == be) return ConstV.STRING_EMPTY;
            return be.name + " : " + DrawPrint_Children(be._next_sibling);

        }


        //public void Init(Vector2Int tileMapSize)
        //public void Init(Index2 tileMapSize)
        public void Init()
        {
            //_tileMapSize_x = tileMapSize.x;
            //_tileMapSize_y = tileMapSize.y;
            //_max_tileMapSize = MAP_WIDTH * MAP_HEIGHT;
            _cellInfo2Map = new CellSpace[MAP_WIDTH * MAP_HEIGHT];

            CreateCellIfoMap_FromStructUpTile();
        }


        private void CreateCellIfoMap_FromStructUpTile()
        {
            int count = 0;
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    CellSpace structTile = null;
                    if (false == SingleO.gridManager.HasStructTile_InPostion2D(new Vector3Int(x, y, 0), out structTile))
                    {
                        //구조타일이 없는 공간설정 
                        structTile = new CellSpace();
                        structTile._pos3d_center = ToPosition3D_Center(count);
                        structTile._pos2d = new Index2(x, y);
                        structTile._pos1d = count;

                    }
                    _cellInfo2Map[count] = structTile;

                    count++;
                }
            }

        }

        //테스트 필요 정상동적 안함 
        public Being RayCast_FirstReturn(Being origin, Vector3 target_3d, Camp.eRelation relation, float length_interval)
        {
            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval || null == (object)origin)
            {
                return null;
            }

            //Index2 origin_2d = origin._getPos2D;
            int origin_1d = -1;
            Index2 origin_2d = ConstV.id2_zero;
            ToPosition1D(origin.GetPos3D(), out origin_2d, out origin_1d);

            Vector3 origin_3d_center = origin._cur_cell._pos3d_center;


            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            //CellSpace structTile = GetCellSpace(origin._getPos1D);
            CellSpace structTile = GetCellSpace(origin_1d);
            if (null != structTile && structTile._isStructTile)
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
                            origin_3d_center.x += dir.x * _tileSize_w * 0.5f;
                            origin_3d_center.z += dir.y * _tileSize_h * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            Vector3Int vd = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_2d.x += vd.x;
                            origin_2d.y += vd.y;
                            origin_3d_center = ToPosition3D_Center(origin_2d);
                        }
                        break;
                }

            }


            Vector3 line = VOp.Minus(target_3d, origin_3d_center);
            Vector3 n = VOp.Normalize(line);
            n = VOp.Multiply(n, length_interval); //미리 곱해 놓는다 

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            //Vector3 next = n * count;
            Vector3 next = VOp.Multiply(n, count);
            float lineSqr = line.sqrMagnitude;
            while (lineSqr > next.sqrMagnitude)
            {
                //최대 50회까지만 탐색한다 
                if (50 <= count)
                {
                    //DebugWide.LogBlue(n); //chamto test
                    return null;
                }

                next = VOp.Plus(origin_3d_center, next);
                structTile = GetCellSpace(next);
                if (null != structTile)
                {
                    if (true == structTile._isUpTile)
                    {

                        if (_ReturnMessage_NotIncluded_InDiagonalArea != this.IsIncluded_InDiagonalArea(next))
                        {
                            return null;
                        }
                    }
                    else if (null != (object)structTile._head)
                    {
                        //==================================
                        if (Camp.eRelation.Unknown != relation)
                        {
                            //요청 관계에 해당하는 객체를 찾는다 
                            Being matchBeing = structTile.MatchRelation(relation, origin);
                            if (null != (object)matchBeing) return matchBeing;
                        }
                        else
                        {
                            return structTile._head; //첫번째로 발견한 객체를 반환한다 (적군 , 아군 구별없다)    
                        }
                        //==================================

                    }

                }

                count++;
                next = VOp.Multiply(n, count);

            }

            return null;
        }

        public bool IsVisibleTile(Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {
            return IsVisibleTile(null, origin_3d, target_3d, length_interval);
        }

        public bool IsVisibleTile(Being origin, Vector3 origin_3d, Vector3 target_3d, float length_interval)
        {

            //interval 값이 너무 작으면 바로 종료 한다 
            if (0.01f >= length_interval)
            {
                return false;
            }

            Index2 origin_2d;
            Vector3 origin_3d_center;
            if (null == (object)origin)
            {
                origin_2d = ToPosition2D(origin_3d);
                origin_3d_center = ToPosition3D_Center(origin_2d);
            }
            else
            {
                //origin_2d = origin._getPos2D;
                origin_2d = ToPosition2D(origin_3d);
                origin_3d_center = origin._cur_cell._pos3d_center;
            }



            //origin 이 구조타일인 경우, 구조타일이 밀어내는 방향값의 타일로 origin_center 의 위치를 변경한다   
            CellSpace structTile = GetCellSpace(origin_3d);
            if (null != structTile && structTile._isStructTile)
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
                            origin_3d_center.x += dir.x * _tileSize_w * 0.5f;
                            origin_3d_center.z += dir.y * _tileSize_h * 0.5f;

                            //DebugWide.LogBlue(origin_2d + "  "+ origin_center.x + "   " + origin_center.z + "  |  " + dir);
                        }
                        break;
                    default:
                        {
                            Vector3Int vd = Misc.GetDir8_Normal2D(structTile._eDir);
                            origin_2d.x += vd.x;
                            origin_2d.y += vd.y;
                            origin_3d_center = ToPosition3D_Center(origin_2d);
                        }
                        break;
                }

            }

            //Vector3 line = target_3d - origin_3d_center;
            Vector3 line = VOp.Minus(target_3d, origin_3d_center);
            Vector3 n = VOp.Normalize(line);
            //Vector3 n = Misc.GetDir360_Normal3D(line); //근사치 노멀값을 사용하면 목표에 도달이 안되는 무한루프에 
            //Vector3 n = line.normalized;
            //n *= length_interval; //미리 곱해 놓는다 
            n = VOp.Multiply(n, length_interval); //미리 곱해 놓는다 

            //인덱스를 1부터 시작시켜 모서리값이 구조타일 검사에 걸리는 것을 피하게 한다 
            int count = 1;
            //Vector3 next = n * count;
            Vector3 next = VOp.Multiply(n, count);
            float lineSqr = line.sqrMagnitude;
            while (lineSqr > next.sqrMagnitude)
            {
                //최대 50회까지만 탐색한다 
                if (50 <= count)
                {
                    //DebugWide.LogBlue(n); //chamto test
                    return false;
                }
                //next = origin_3d_center + next;
                next = VOp.Plus(origin_3d_center, next);
                structTile = GetCellSpace(next);
                if (null != structTile)
                {
                    if (true == structTile._isUpTile)
                    {

                        if (_ReturnMessage_NotIncluded_InDiagonalArea != this.IsIncluded_InDiagonalArea(next))
                        {
                            return false;
                        }
                    }

                }

                count++;
                //next = n * count;
                next = VOp.Multiply(n, count);

            }

            return true;
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

            CellSpace structTile = GetCellSpace(xz_3d);
            if (null != structTile)
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



        public Index2 ToPosition2D(Vector3 pos3d, Index2 result = default(Index2))
        {

            //부동소수점 처리 문제로 직접계산하지 않는다 
            //int pos2d_x = Mathf.FloorToInt(pos3d.x / _tileSize_w);
            //int pos2d_y = Mathf.FloorToInt(pos3d.z / _tileSize_h);


            //직접계산
            //==============================================
            int pos2d_x = 0;
            int pos2d_y = 0;
            if (0 <= pos3d.x)
            {
                //양수일때는 소수점을 버린다. 
                pos2d_x = (int)(pos3d.x / _tileSize_w);
            }
            else
            {
                //음수일때는 올림을 한다. 
                pos2d_x = (int)((pos3d.x / _tileSize_w) - 0.9f);
            }

            if (0 <= pos3d.z)
            {
                //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
                pos2d_y = (int)(pos3d.z / _tileSize_h); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다
            }
            else
            {
                pos2d_y = (int)((pos3d.z / _tileSize_h) - 0.9f);
            }
            //==============================================

            result.x = pos2d_x;
            result.y = pos2d_y;
            return result;
        }

        public Vector3 ToCenter3D_FromPosition3D(Vector3 pos3d)
        {

            return this.ToPosition3D_Center(ToPosition2D(pos3d));
        }


        public Vector3 ToPosition3D_Center(Index2 posXY_2d, Vector3 result = default(Vector3))
        {

            //2d => 3d
            float pos3d_x = (float)posXY_2d.x * _tileSize_w;
            float pos3d_z = (float)posXY_2d.y * _tileSize_h;

            //셀의 중간에 위치하도록 한다
            pos3d_x += _tileSize_w * 0.5f;
            pos3d_z += _tileSize_h * 0.5f;

            result.x = pos3d_x;
            result.y = 0;
            result.z = pos3d_z;
            return result;
        }

        public Vector3 ToPosition3D(Index2 posXY_2d, Vector3 result = default(Vector3))
        {
            result.x = (float)posXY_2d.x * _tileSize_w;
            result.y = 0;
            result.z = (float)posXY_2d.y * _tileSize_h;
            return result;
        }

        public int ToPosition1D(Index2 posXY_2d)
        {
            //Assert.IsFalse(0 > posXY_2d.x || 0 > posXY_2d.y, "음수좌표값은 1차원값으로 변환 할 수 없다");
            if (0 > posXY_2d.x || 0 > posXY_2d.y || MAP_WIDTH <= posXY_2d.x || MAP_HEIGHT <= posXY_2d.y) return -1;

            return (posXY_2d.x + posXY_2d.y * MAP_WIDTH); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Index2 ToPosition2D(int pos1d, Index2 result = default(Index2))
        {
            //Assert.IsFalse(0 > pos1d, "음수좌표값은 2차원값으로 변환 할 수 없다");
            if (0 > pos1d) return ConstV.id2_zero;

            result.x = pos1d % MAP_WIDTH;
            result.y = pos1d / MAP_WIDTH;
            return result;
        }

        public void ToPosition1D(Vector3 pos3d, out Index2 out_pos2d, out int out_pos1d)
        {
            int pos2d_x = 0;
            int pos2d_y = 0;
            if (0 <= pos3d.x)
            {
                //양수일때는 소수점을 버린다. 
                pos2d_x = (int)(pos3d.x / _tileSize_w);
            }
            else
            {
                //음수일때는 올림을 한다. 
                pos2d_x = (int)((pos3d.x / _tileSize_w) - 0.9f);
            }

            if (0 <= pos3d.z)
            {
                //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
                pos2d_y = (int)(pos3d.z / _tileSize_h); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다
            }
            else
            {
                pos2d_y = (int)((pos3d.z / _tileSize_h) - 0.9f);
            }
            //==============================================

            out_pos2d = new Index2(pos2d_x, pos2d_y);
            //out_pos2d.x = pos2d_x; out_pos2d.y = pos2d_y;
            out_pos1d = -1;
            if (0 > pos2d_x || 0 > pos2d_y || MAP_WIDTH <= pos2d_x || MAP_HEIGHT <= pos2d_y) return;

            //2d => 1d
            out_pos1d = (pos2d_x + pos2d_y * MAP_WIDTH); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 
        }

        public int ToPosition1D(Vector3 pos3d)
        {
            //3d => 2d
            //부동소수점 처리 문제로 직접계산하지 않는다 
            //int pos2d_x = Mathf.FloorToInt(pos3d.x / _tileSize_w);
            //int pos2d_y = Mathf.FloorToInt(pos3d.z / _tileSize_h);

            //직접계산
            //==============================================
            int pos2d_x = 0;
            int pos2d_y = 0;
            if (0 <= pos3d.x)
            {
                //양수일때는 소수점을 버린다. 
                pos2d_x = (int)(pos3d.x / _tileSize_w);
            }
            else
            {
                //음수일때는 올림을 한다. 
                pos2d_x = (int)((pos3d.x / _tileSize_w) - 0.9f);
            }

            if (0 <= pos3d.z)
            {
                //???? 부동소수점 때문에 생기는 이상한 계산 결과 : 버림 함수를 사용하여 비트쯔끄러기를 처리한다
                pos2d_y = (int)(pos3d.z / _tileSize_h); //(int)(0.8 / 0.16) => 4 로 잘못계산됨. 5가 나와야 한다
            }
            else
            {
                pos2d_y = (int)((pos3d.z / _tileSize_h) - 0.9f);
            }
            //==============================================

            if (0 > pos2d_x || 0 > pos2d_y || MAP_WIDTH <= pos2d_x || MAP_HEIGHT <= pos2d_y) return -1;

            //2d => 1d
            return (pos2d_x + pos2d_y * MAP_WIDTH); //x축 타일맵 길이 기준으로 왼쪽에서 오른쪽 끝까지 증가후 위쪽방향으로 반복된다 

        }

        public Vector3 ToPosition3D_Center(int pos1d, Vector3 result = default(Vector3))
        {
            if (0 > pos1d || MAP_WIDTH * MAP_HEIGHT <= pos1d) return ConstV.v3_zero;

            //1d => 2d
            int pos2d_x = pos1d % MAP_WIDTH;
            int pos2d_y = pos1d / MAP_WIDTH;

            //2d => 3d
            float pos3d_x = (float)pos2d_x * _tileSize_w;
            float pos3d_z = (float)pos2d_y * _tileSize_h;

            //셀의 중간에 위치하도록 한다
            pos3d_x += _tileSize_w * 0.5f;
            pos3d_z += _tileSize_h * 0.5f;

            result.x = pos3d_x;
            result.y = 0;
            result.z = pos3d_z;
            return result;
        }


        //==================================================

        public bool HasStructUpTile(int p1d)
        {
            if (0 > p1d || p1d >= _max_tileMapSize)
            {
                return false;
            }

            CellSpace structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isUpTile && structTile._isStructTile;
            }

            return false;
        }
        public bool HasStructUpTile(Index2 p2d)
        {
            int p1d = this.ToPosition1D(p2d);
            if (-1 == p1d)
            {
                return false;
            }

            CellSpace structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {

                return structTile._isUpTile && structTile._isStructTile;
            }


            return false;
        }
        public bool HasStructUpTile(Vector3 p3d)
        {
            int p1d = this.ToPosition1D(p3d);
            if (-1 == p1d)
            {
                return false;
            }

            CellSpace structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isUpTile && structTile._isStructTile;
            }

            return false;
        }

        public bool HasStructTile(int p1d, out CellSpace structTile)
        {
            if (0 > p1d || p1d >= _max_tileMapSize)
            {
                structTile = null;
                return false;
            }

            structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isStructTile;
            }

            return false;
        }
        public bool HasStructTile(Index2 p2d, out CellSpace structTile)
        {
            int p1d = this.ToPosition1D(p2d);
            if (-1 == p1d)
            {
                structTile = null;
                return false;
            }

            structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isStructTile;
            }


            return false;
        }
        public bool HasStructTile(Vector3 p3d, out CellSpace structTile)
        {
            int p1d = this.ToPosition1D(p3d);
            if (-1 == p1d)
            {
                structTile = null;
                return false;
            }

            structTile = _cellInfo2Map[p1d];
            if (null != structTile)
            {
                return structTile._isStructTile;
            }

            return false;
        }

        public CellSpace GetCellSpace(int p1d)
        {
            if (0 > p1d || p1d >= _max_tileMapSize)
            {
                return null;
            }

            return _cellInfo2Map[p1d];
        }

        public CellSpace GetCellSpace(Index2 pos2d)
        {
            //int pos1d = pos2d.x + pos2d.y * _tileMapSize_x;

            int pos1d = ToPosition1D(pos2d);
            if (0 > pos1d) return null; //타일맵을 벗어나는 범위 

            return _cellInfo2Map[pos1d];
        }


        public CellSpace GetCellSpace(Vector3 pos3d)
        {
            int pos1d = ToPosition1D(pos3d);
            if (0 > pos1d) return null; //타일맵을 벗어나는 범위 

            return _cellInfo2Map[pos1d];

        }

        public void AttachCellSpace(int pos1d, Being dst)
        {
            CellSpace tile = GetCellSpace(pos1d);
            //if (false == HasStructTile(pos1d, out tile))
            {
                //뗀후 새로운 곳에 붙인다 
                tile.DetachChild(dst);
                tile.AttachChild(dst);
            }
        }

        public void DetachCellSpace(int pos1d, Being dst)
        {
            CellSpace tile = GetCellSpace(pos1d);
            //if (false == HasStructTile(pos1d, out tile))
            {
                tile.DetachChild(dst);
            }
        }

    }
}
