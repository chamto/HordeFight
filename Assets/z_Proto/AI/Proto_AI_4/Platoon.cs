﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Proto_AI_4
{
    //배치
    public struct Disposition
    {
        //소대 , 분대 정보가 없을 수 있다  
        //public int _platoon_id; //소대 고유번호
        //public int _squad_id;  //분대 고유번호 
        //public int _unit_pos;  //분대 개별위치 - 분대 유닛리스트와 일치되어야 함

        public Vector3 _initPos; //[초기위치] 원점에서 부터의 떨어진 위치 , 부모 기준점이 0 이면 <initPos == offset> 이다 
        public Vector3 _offset; //부모의 기준점으로 부터 초기위치가 떨어진 거리량

        //public Squad _belong_platoon; //소속소대정보 
        //public Squad _belong_squad; //소속분대정보  
        public Squad _belong_formation; //속한진형 

        //public bool _solo_activity; //단독활동 ??


        //public void Init()
        //{
        //    //_platoon_id = -1;
        //    //_squad_id = -1;
        //    //_unit_pos = -1;

        //    //_solo_activity = false;
        //}
    }


    public enum eFormKind
    {
        None = 0,
        Rectangular = 1,
        Circle = 2,
        Spike = 3,
        Data,
    }

    public enum eFormDepth
    {
        Platoon = 20,
        Squad = 10,
        None = 0,
    }

    public struct FormInfo
    {
        public eFormKind eKind;

        //Rectangular
        public int column; //열
        public int row; //행 
        public float horn; //width 진형의 뿔 , 양수면 앞으로 나오고 음수면 뒤로 나온다 
        public float between_x; //사이거리 
        public float between_z; //사이거리 

        //Circle
        public float radius; //원형진형 반지름

        //Spike
        public int spike_num;
        public float spike_between;

        public void Init()
        {
            eKind = eFormKind.None;
            column = 0;
            row = 0;
            horn = 0;
            between_x = 0;
            between_z = 0;

            radius = 0;

            spike_num = 4; //기본 십자가형태
            spike_between = 0;
        }
    }

    //분대 , 역할의 단위 
    public class Squad : OrderPoint
    {
        public int _depth; //진형의 깊이 : 예) ...상위21 - 소대20 - 분대10 - 하위9 ...

        public Vector3 _standard; //[기준점] 기본으로 원점의 위치에 있다. 하위 자식들을 한꺼번에 위치변경하기 위해 사용된다 

        public Vector3 _initPos; //[초기위치] 원점에서 부터의 떨어진 위치 , 부모 기준점이 0 이면 <initPos == offset> 이다 
        public Vector3 _offset; //부모의 기준점으로 부터 초기위치가 떨어진 거리량

        public Squad _squad_parent = null; //부모 분대 
        public List<Unit> _units = new List<Unit>(); //포함유닛
        public List<Squad> _squad_children = new List<Squad>(); //포함진형

        public FormInfo _info;

        //----------------------------------------------------

        public int _squard_id = -1;

        public bool _solo_activity = false; //단독활동 
        public bool _isFollow = false;
        public float _follow_gap = 3;
        public bool _isDirMatch = true; //분대방향을 소대방향과 일치

        public void Init()
        {
            //OrderPoint 멤버변수 임시로 초기화 
            _pos = Vector3.zero;
            _targetPos = Vector3.zero;
            _before_targetPos = Vector3.zero;

            //--------------------------

            _depth = 0;

            _standard = Vector3.zero;
            _initPos = Vector3.zero;
            _offset = Vector3.zero;

            _squad_parent = null; //todo : 소대에서도 연결 끊는 처리 필요

            _units.Clear();
            _squad_children.Clear(); //todo : 나중에 메모리풀로 변경하여 재할당 문제 피하기 

            _info.Init();
            _squard_id = -1;
            _solo_activity = false;
            _isFollow = false;
            _follow_gap = 3;
            _isDirMatch = false;

        }

        //현재기준에서 상위만 검색
        public Squad FindUpDepth(eFormDepth findDepth)
        {
            if ((int)findDepth == _depth)
                return this;

            const int FIND_COUNT = 20;

            Squad parent = _squad_parent;
            for (int i = 0; i < FIND_COUNT; i++)
            {
                if (null == parent) break;

                if ((int)findDepth == parent._depth)
                    return parent;
                parent = parent._squad_parent;
            }

            return null;
        }



        //처음부터 스쿼드 목록을 제공 한다 
        public void UnitList_Update(int squard_id, List<Unit> lists)
        {

            if (0 == lists.Count) return;

            _squard_id = squard_id;

            //지정 분대id를 가지는 수만큼 공간을 늘린다 
            _units.Clear();
            for (int i = 0; i < lists.Count; i++)
            {
                //if (_squard_id == units[i]._disposition._squad_id)
                {
                    _units.Add(null);
                }
            }

            //_units 의 unit_pos 위치에 unit 을 넣는다 
            for (int i = 0; i < lists.Count; i++)
            {
                //if (_squard_id == lists[i]._disposition._squad_id)
                //{
                //    int upos = lists[i]._disposition._unit_pos;
                //    _units[upos] = lists[i];
                //    lists[i]._disposition._belong_squad = this;
                //}

                _units[i] = lists[i];
                lists[i]._disposition._belong_formation = this;
            }

        }

        //fixme : 메모리풀로 변경 필요 
        static public Squad Create(int squard_id, List<Unit> units)
        {
            Squad squard = new Squad(); 
            squard.Init();
            squard.UnitList_Update(squard_id, units);

            return squard; 
        }

        static public Squad Create()
        {
            Squad squard = new Squad();
            squard.Init();

            return squard;
        }

        static public void Release()
        {
            //todo : 메모리풀로 되돌리는 해제처리 넣기  
        }

        //_formation 값을 채운다 
        private void CalcOffset_Rect()
        {
            int max_row = (_units.Count / _info.column) + 1;
            int len_x = _units.Count < _info.column ? _units.Count : _info.column;
            float center_x = (len_x - 1) * _info.between_x * 0.5f; //중앙 길이를 구한다


            float tan = 0 != center_x ? (_info.horn / center_x) : 0;
            for (int row = 0; row < max_row; row++)
            {
                for (int col = 0; col < _info.column; col++)
                {
                    int idx = col + (row * _info.column);
                    if (idx >= _units.Count) return;

                    float x = col * _info.between_x;
                    float z = -row * _info.between_z;

                    // z/x = tan , z = x * tan
                    float height = x * tan; //왼쪽 빗면
                    if (center_x < x) //오른쪽 빗면
                    {
                        height = (center_x * 2 - x) * tan;
                    }

                    x -= center_x; //중앙에 배치되도록 한다

                    _units[idx]._disposition._initPos = new Vector3(x, 0, z + height);
                    _units[idx]._disposition._offset = _units[idx]._disposition._initPos - _standard;

                }

            }
        }

        private void CalcOffset_Circle()
        {
            //_standard = new Vector3(0, 0, _set.radius);
            float angle = 360 / _units.Count;
            for (int i = 0; i < _units.Count; i++)
            {
                Quaternion rot = Quaternion.AngleAxis(angle * i, Vector3.up);
                _units[i]._disposition._initPos = rot * new Vector3(0, 0, _info.radius);
                _units[i]._disposition._offset = _units[i]._disposition._initPos - _standard;
            }
        }

        private void CalcOffset_Spike()
        {

            _units[0]._disposition._initPos = Vector3.zero;
            _units[0]._disposition._offset = _units[0]._disposition._initPos - _standard;

            float angle = 360f / _info.spike_num;
            for (int i = 1; i < _units.Count; i++)
            {
                int len = ((i-1) / _info.spike_num)+1;
                //DebugWide.LogBlue(i + "  " + len);
                Quaternion rot = Quaternion.AngleAxis(angle * (i-1), Vector3.up);
                _units[i]._disposition._initPos = rot * new Vector3(0, 0, _info.spike_between * len);
                _units[i]._disposition._offset = _units[i]._disposition._initPos - _standard;
            }
        }

        public void CalcOffset()
        {

            if (_info.eKind == eFormKind.Rectangular)
            {
                CalcOffset_Rect();
            }
            else if (_info.eKind == eFormKind.Circle)
            {
                CalcOffset_Circle();
            }
            else if (_info.eKind == eFormKind.Spike)
            {
                CalcOffset_Spike();
            }
        }

        public void ApplyFormation_SQD1_Width()
        {
            _squad_children.Clear();
            Squad form = Squad.Create();
            _squad_children.Add(form);

            _depth = (int)eFormDepth.Squad;
            form._depth = (int)eFormDepth.Squad - 1;
            //--------------------------------------------
            //진형에 유닛등록
            _squad_children[0]._units.Clear();
            _squad_children[0]._units.AddRange(_units);
            //_forms[0]._units = _units; //이렇게 주소를 넣어주면 안된다. 복사본을 만들어야 한다 

            //유닛이 속한 진형 갱신 
            for(int i=0;i<_units.Count;i++)
            {
                _units[i]._disposition._belong_formation = form; 
            }
            //--------------------------------------------

            _isDirMatch = true;
            //_standard = new Vector3(0, 0, 2);

            _squad_children[0]._squad_parent = this;
            _squad_children[0]._info.column = 10;
            _squad_children[0]._info.between_x = 1.2f;
            _squad_children[0]._info.between_z = 1.2f;
            _squad_children[0]._info.horn = 0f;
            _squad_children[0]._info.eKind = eFormKind.Rectangular;
            //_squad_children[0]._info.eKind = eFormKind.Circle;
            //_squad_children[0]._info.radius = 4f;
            //_squad_children[0]._info.eKind = eFormKind.Spike;
            //_squad_children[0]._info.spike_num = 9;
            //_squad_children[0]._info.spike_between = 2;
            _squad_children[0]._initPos = new Vector3(0, 0, 4);
            _squad_children[0]._offset = _squad_children[0]._initPos - _standard;
            _squad_children[0]._pos = (_rotation * _squad_children[0]._offset) + _pos; //PointToWorldSpace 
            _squad_children[0].CalcOffset();
        }

        public void ApplyFormation_SQD1_Height()
        {
            _squad_children.Clear();
            Squad form = Squad.Create();
            _squad_children.Add(form);

            _depth = (int)eFormDepth.Squad;
            form._depth = (int)eFormDepth.Squad - 1;
            //--------------------------------------------
            //진형에 유닛등록
            _squad_children[0]._units.Clear();
            _squad_children[0]._units.AddRange(_units);
            //_forms[0]._units = _units; //이렇게 주소를 넣어주면 안된다. 복사본을 만들어야 한다 

            //유닛이 속한 진형 갱신 
            for (int i = 0; i < _units.Count; i++)
            {
                _units[i]._disposition._belong_formation = form;
            }
            //--------------------------------------------

            _isDirMatch = true;

            _squad_children[0]._squad_parent = this;
            _squad_children[0]._info.column = 3;
            _squad_children[0]._info.between_x = 1.2f;
            _squad_children[0]._info.between_z = 2.5f;
            _squad_children[0]._info.horn = 0;
            _squad_children[0]._info.eKind = eFormKind.Rectangular;
            //_forms[0]._info.eKind = eFormKind.Circle;
            //_forms[0]._info.radius = 4f;
            _squad_children[0]._initPos = new Vector3(0, 0, 4);
            _squad_children[0]._offset = _squad_children[0]._initPos - _standard;
            _squad_children[0]._pos = (_rotation * _squad_children[0]._offset) + _pos; //PointToWorldSpace 
            _squad_children[0].CalcOffset();
        }

        public void ApplyFormation_SQD1_Spike()
        {
            _squad_children.Clear();
            Squad form = Squad.Create();
            _squad_children.Add(form);

            _depth = (int)eFormDepth.Squad;
            form._depth = (int)eFormDepth.Squad - 1;
            //--------------------------------------------
            //진형에 유닛등록
            _squad_children[0]._units.Clear();
            _squad_children[0]._units.AddRange(_units);

            //유닛이 속한 진형 갱신 
            for (int i = 0; i < _units.Count; i++)
            {
                _units[i]._disposition._belong_formation = form;
            }
            //--------------------------------------------

            //_isDirMatch = true;
            _squad_children[0]._squad_parent = this;
            _squad_children[0]._standard = new Vector3(0, 0, 8); //머리기준으로 회전하게 해준다 
            _squad_children[0]._info.eKind = eFormKind.Spike;
            _squad_children[0]._info.spike_num = 9;
            _squad_children[0]._info.spike_between = 2;
            _squad_children[0]._initPos = new Vector3(0, 0, 0);
            _squad_children[0]._offset = _squad_children[0]._initPos - _standard;
            _squad_children[0]._pos = (_rotation * _squad_children[0]._offset) + _pos; //PointToWorldSpace 
            _squad_children[0].CalcOffset();
        }

        public void ApplyFormation_String()
        {

            //등록된 form 해제 처리 필요 
            _squad_children.Clear();

            _depth = (int)eFormDepth.Squad;


            FormInfo info = new FormInfo();
            info.eKind = eFormKind.Rectangular;
            info.column = 3;
            info.row = 2;
            info.between_x = 1.2f;
            info.between_z = 1.5f;
            info.horn = 0;

            Squad form = null;
            int rowNum = (_units.Count / (info.column * info.row))+1; //나머지가 나올수 있으므로 1을 더한다 
            int unitCount = 0;
            float rowLen = info.between_z * info.row;
            for (int i=0;i< rowNum;i++)
            {
                if (_units.Count <= unitCount) break;

                form = Squad.Create();
                form._units.Clear();
                for(int j=0;j< info.column*info.row ;j++)
                {
                    if (_units.Count <= unitCount) break;

                    form._units.Add(_units[unitCount]);
                    _units[unitCount]._disposition._belong_formation = form;
                    unitCount++;
                }

                form._squad_parent = this;
                form._depth = (int)eFormDepth.Squad - 1;
                form._info = info;
                form._initPos = new Vector3(0, 0, -rowLen*i);
                form._offset = form._initPos - _standard;
                form._pos = (_rotation * form._offset) + _pos; //PointToWorldSpace , 위치적용
                form._isFollow = true;
                form._follow_gap = rowLen;
                //form._isDirMatch = true;
                form.CalcOffset();
                _squad_children.Add(form);
            }
        }

        public void ApplyFormation_String2()
        {

            //등록된 form 해제 처리 필요 
            _squad_children.Clear();

            _depth = (int)eFormDepth.Squad;

            const int SQUAD_NUM = 5;

            FormInfo info = new FormInfo();
            info.eKind = eFormKind.Spike;
            info.spike_num = 4;
            info.spike_between = 2;


            Squad form = null;
            int squad_count = (_units.Count / SQUAD_NUM) + 1; //나머지가 나올수 있으므로 1을 더한다 
            int unitCount = 0;

            int spike_skin_num = (int)((SQUAD_NUM-1) / info.spike_num); //중앙 유닛은 분대개수에서 제외한다 
            spike_skin_num += ((SQUAD_NUM - 1) % info.spike_num) > 0 ? 1 : 0; //나머지가 있으면 껍질 +1 한다 
            float rowLen = info.spike_between * (spike_skin_num * 2 + 1); //하위 모임과의 거리 구하기
            for (int i = 0; i < squad_count; i++)
            {
                if (_units.Count <= unitCount) break;

                form = Squad.Create();
                form._units.Clear();
                for (int j = 0; j < SQUAD_NUM; j++)
                {
                    if (_units.Count <= unitCount) break;

                    form._units.Add(_units[unitCount]);
                    _units[unitCount]._disposition._belong_formation = form;
                    unitCount++;
                }

                //DebugWide.LogBlue(i + "  " + spike_skin_num + "  " + rowLen); //test

                form._squad_parent = this;
                form._depth = (int)eFormDepth.Squad - 1;
                form._standard = new Vector3(0, 0, (info.spike_between * spike_skin_num)); //머리기준으로 회전하게 해준다 
                form._info = info;
                form._initPos = new Vector3(0, 0, -rowLen * i);
                form._offset = form._initPos - _standard;
                form._pos = (_rotation * form._offset) + _pos; //PointToWorldSpace , 위치적용
                form._isFollow = true;
                form._follow_gap = rowLen ;
                //form._isDirMatch = true;
                form.CalcOffset();
                _squad_children.Add(form);
            }
        }

        public void AddSquad(Squad squad)
        {
            _squad_children.Add(squad);
        }

        public void ApplyPlatoon_Cross(Squad squad_0, Squad squad_1, Squad squad_2 , Squad squad_3 )
        {
            _depth = (int)eFormDepth.Platoon;

            //등록된 form 해제 처리 필요 
            _squad_children.Clear();

            AddSquad(squad_0);
            AddSquad(squad_1);
            AddSquad(squad_2);
            AddSquad(squad_3);

            _isFollow = false;
            _isDirMatch = true;


            squad_0._depth = (int)eFormDepth.Squad;
            //squad_0._info.row = 3;
            squad_0._info.column = 5;
            squad_0._info.between_x = 1.2f;
            squad_0._info.between_z = 1.2f;
            squad_0._info.horn = 0;
            squad_0._info.eKind = eFormKind.Rectangular;
            squad_0._initPos = new Vector3(0, 0, 3);
            squad_0._offset = squad_0._initPos - _standard;
            squad_0._pos = (_rotation * squad_0._offset) + _pos; //PointToWorldSpace 
            squad_0.CalcOffset();

            squad_1._depth = (int)eFormDepth.Squad;
            squad_1._info = squad_0._info;
            squad_1._info.eKind = eFormKind.Rectangular;
            squad_1._initPos = new Vector3(-5, 0, -3);
            squad_1._offset = squad_1._initPos - _standard;
            squad_1._pos = (_rotation * squad_1._offset) + _pos; //PointToWorldSpace 
            squad_1.CalcOffset();

            squad_2._depth = (int)eFormDepth.Squad;
            squad_2._info = squad_0._info;
            squad_2._info.eKind = eFormKind.Rectangular;
            squad_2._initPos = new Vector3(5, 0, -3);
            squad_2._offset = squad_2._initPos - _standard;
            squad_2._pos = (_rotation * squad_2._offset) + _pos; //PointToWorldSpace 
            squad_2.CalcOffset();

            squad_3._depth = (int)eFormDepth.Squad;
            squad_3._info.radius = 3;
            squad_3._info.eKind = eFormKind.Circle;
            squad_3._initPos = new Vector3(0, 0, -6);
            squad_3._offset = squad_3._initPos - _standard;
            squad_3._pos = (_rotation * squad_3._offset) + _pos; //PointToWorldSpace 
            squad_3.CalcOffset();
            //squad_3.ApplyFormation_String();

        }


        override public void Update(float deltaTime)
        {
            base.Update(deltaTime);

            Vector3 toDir = _pos - _targetPos;
            Vector3 beforePos = _targetPos;
            for (int i = 0; i < _squad_children.Count; i++)
            {
                if (false == _squad_children[i]._solo_activity)
                {
                    if (false == _squad_children[i]._isFollow)
                    {
                        //1* 오프셋위치로 고정위치를 계산함 
                        _squad_children[i]._targetPos = (_rotation * _squad_children[i]._offset) + _pos; //PointToWorldSpace  
                    }
                    else
                    {
                        //1* 일정거리를 두며 앞에 분대를 따라가게 계산함 
                        if(0 == i)
                        {
                            //첫번째 진형은 오프셋에 따른 설정된 위치로 목표가 설정되어야 한다 
                            _squad_children[i]._targetPos = (_rotation * _squad_children[i]._offset) + _pos;
                        }
                        else
                        {
                            //_forms[i]._targetPos = beforePos + (_forms[i]._targetPos - beforePos).normalized * _forms[i]._follow_gap;
                            _squad_children[i]._targetPos = beforePos + (_squad_children[i]._pos - beforePos).normalized * _squad_children[i]._follow_gap; //초기위치가 설정된 pos를 사용해야 한다 
                        }
                        beforePos = _squad_children[i]._targetPos;
                        //beforePos = _forms[i]._pos;
                    }

                    if (_squad_children[i]._isDirMatch)
                    {
                        //분대방향을 소대방향과 일치시킨다 
                        _squad_children[i]._pos = _squad_children[i]._targetPos + toDir;
                    }

                }
                _squad_children[i].Update(deltaTime); //임시로 여기서 갱신 , 스쿼드관리객체에서 따로 모아 갱신시키는 것으로 변경하기 
            }
        }

        public void Draw(Color color)
        {
            base.Draw(Color.red);

            for (int i = 0; i < _squad_children.Count; i++)
            {
                _squad_children[i].Draw(color);
            }
        }
    }

    //public class Platoon : OrderPoint
    //{ 
    //}

    //전달모임 , Transmission , 최소한의 배치 , 역할의 단위 
    //public class Squard_old : OrderPoint
    //{

    //    public Platoon _platoon = null; //속한 소대 
    //    public int _squard_num = -1;
    //    public int _unit_count = 0; //전체분대원수 
    //    public bool _Solo_Activity = false; //단독활동 

    //    //public Vector3 _dir = Vector3.forward; //방향 
    //    public Vector3 _form_standard = Vector3.zero; //기준점
    //    //public int _form_row; //행
    //    //public int _form_column; //열
    //    //public float _form_horn; //width 진형의 뿔 , 양수면 앞으로 나오고 음수면 뒤로 나온다 
    //    //public float _form_dis_between; //사이거리 
    //    //public float _form_radius; //원형진형 반지름
    //    public FormRectangular _form_rect;
    //    public FormCircle  _form_circle;
    //    public Formation _formation;

    //    public List<Unit> _units = new List<Unit>(); //전체배치정보

    //    //==================================================

    //    static public Squard Create_Squard(Platoon platoon, int squard_num)
    //    {
    //        Squard squard = new Squard();
    //        squard._platoon = platoon;
    //        squard._squard_num = squard_num;
    //        squard._eFormKind = eFormKind.None;

    //        //list 순서와 squard_pos를 일치시키기 위해 미리 만들어 놓는다 
    //        Unit unit = null;
    //        for (int i = 0; i < platoon._units.Count; i++)
    //        {
    //            unit = platoon._units[i];
    //            if (squard._squard_num != unit._disposition._squard_num) continue;
    //            squard._unit_count++;
    //            squard._units.Add(null); 
    //        }

    //        //소대전체 유닛에서 분대에 속하는 유닛만 찾아 추가한다 
    //        for (int i = 0; i < platoon._units.Count; i++)
    //        {
    //            unit = platoon._units[i];
    //            if (squard._squard_num != unit._disposition._squard_num) continue;

    //            squard._units[unit._disposition._squard_pos] = unit;
    //            unit._platoon = platoon;
    //            unit._squard = squard;
    //        }


    //        //---------------------------

    //        return squard;
    //    }

    //    //_formation 값을 채운다 
    //    private void CalcOffset_Rect()
    //    {
    //        int max_row = (_units.Count / _form_rect.column) + 1;
    //        int len_x = _unit_count < _form_rect.column ? _unit_count : _form_rect.column;
    //        float center_x = (len_x - 1) * _form_rect.between_x * 0.5f; //중앙 길이를 구한다
    //        //_form_standard = new Vector3(center_x, 0, 0); 

    //        float tan =  0 != center_x ? (_form_rect.horn / center_x) : 0;
    //        for (int row = 0; row < max_row; row++)
    //        {
    //            for (int col = 0; col < _form_rect.column; col++)
    //            {
    //                int idx = col +( row * _form_rect.column);
    //                if (idx >= _units.Count) return;

    //                float x = col * _form_rect.between_x; 
    //                float z = -row * _form_rect.between_z;

    //                // z/x = tan , z = x * tan
    //                float height = x * tan; //왼쪽 빗면
    //                if(center_x < x ) //오른쪽 빗면
    //                {
    //                    height = (center_x * 2 - x) * tan; 
    //                }

    //                x -= center_x; //중앙에 배치되도록 한다

    //                _units[idx]._formation._initPos = new Vector3(x, 0, z + height);
    //                _units[idx]._formation._offset = _units[idx]._formation._initPos - _form_standard;

    //            }

    //        }
    //    }

    //    private void CalcOffset_Circle()
    //    {
    //        _form_standard = new Vector3(0, 0, _form_circle.radius);
    //        float angle = 360 / _unit_count;
    //        for (int i = 0; i < _units.Count; i++)
    //        {
    //            Quaternion rot = Quaternion.AngleAxis(angle * i, Vector3.up);
    //            _units[i]._formation._initPos = rot * new Vector3(0, 0, _form_circle.radius);
    //            _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
    //        }
    //    }

    //    public void ApplyFormation()
    //    {

    //        if (_eFormKind == eFormKind.Rectangular)
    //        {
    //            //_form_rect.row = 3;
    //            //_form_rect.column = 5;
    //            //_form_rect.dis_between = 1.2f;
    //            //_form_rect.horn = 0; 
    //            //int len = _unit_count < _form_rect.column ?  _unit_count : _form_rect.column;
    //            //_form_standard = new Vector3((len - 1) * _form_rect.dis_between * 0.5f, 0, 0); //중앙을 기준점으로 삼는다 
    //            //for (int i = 0; i < _units.Count; i++)
    //            //{
    //            //    _units[i]._formation._initPos = new Vector3(i * 1, 0, 0);
    //            //    _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
    //            //}
    //            CalcOffset_Rect();
    //        }

    //        if (_eFormKind == eFormKind.Circle)
    //        {
    //            //_form_circle.radius = 2;
    //            //_form_standard = new Vector3(0, 0, _form_circle.radius);
    //            //float angle = 360 / _unit_count;
    //            //for (int i = 0; i < _units.Count; i++)
    //            //{
    //            //    Quaternion rot = Quaternion.AngleAxis(angle*i, Vector3.up);
    //            //    _units[i]._formation._initPos = rot * new Vector3(0, 0, _form_circle.radius);
    //            //    _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
    //            //}
    //            CalcOffset_Circle();


    //        }
    //    }

    //    //bool _isFollow = true;
    //    //float _follow_gap = 2;
    //    //override public void Update(float deltaTime)
    //    //{
    //    //    base.Update(deltaTime);

    //    //    //Vector3 toDir = _pos - _targetPos;
    //    //    Vector3 beforePos = _pos;
    //    //    for (int i = 0; i < _units.Count; i++)
    //    //    {
    //    //        if (false == _isFollow)
    //    //        {
    //    //            //1* 오프셋위치로 고정위치를 계산함 
    //    //            _units[i]._steeringBehavior._targetPos = (_rotation * _units[i]._formation._offset) + _pos; //PointToWorldSpace  
    //    //        }
    //    //        else
    //    //        {
    //    //            //1* 일정거리를 두며 앞에 분대를 따라가게 계산함 
    //    //            _units[i]._steeringBehavior._targetPos = beforePos + (_units[i]._pos - beforePos).normalized * _follow_gap;
    //    //            beforePos = _units[i]._pos;

    //    //        }

    //    //    }
    //    //}

    //    //유닛을 분대에서 뺀다 
    //    public bool UnitDismiss(Unit unit)
    //    {
    //        if (_platoon._platoon_num != unit._disposition._platoon_num) return false;
    //        if (_squard_num != unit._disposition._squard_num) return false;
    //        if (_units.Count <= unit._disposition._squard_pos) return false;

    //        _units[unit._disposition._squard_pos] = null;
    //        unit._platoon = null;
    //        unit._squard = null;
    //        unit._disposition.Init();

    //        return true; 
    //    }

    //    //유닛을 분대에 추가한다 
    //    public bool UnitInclude(Unit unit)
    //    {
    //        if (_platoon._platoon_num != unit._disposition._platoon_num) return false;
    //        if (_squard_num != unit._disposition._squard_num) return false;

    //        //분대유닛 목록을 미리 늘려준다
    //        if(_units.Count <= unit._disposition._squard_pos)
    //        {
    //            int addCount = unit._disposition._squard_pos - _units.Count + 1;
    //            for(int i=0;i<addCount;i++)
    //            {
    //                _units.Add(null);
    //            }
    //        }

    //        _units[unit._disposition._squard_pos] = unit;
    //        unit._platoon = _platoon;
    //        unit._squard = this;

    //        return true;
    //    }
    //}



    //public class Platoon_old : OrderPoint
    //{
    //    public int _platoon_num = -1;
    //    public int _squard_count = 0;
    //    public Vector3 _form_standard = Vector3.zero; //기준점

    //    public bool _isFollow = false;
    //    public float _follow_gap = 3;

    //    public bool _isDirMatch = true; //분대방향을 소대방향과 일치

    //    public List<Unit> _units = new List<Unit>();
    //    public List<Squard> _squards = new List<Squard>();

    //    static public Platoon Create_Platoon (List<Unit> units)
    //    {
    //        if (0 == units.Count) return null;

    //        Platoon platoon = new Platoon();
    //        platoon._platoon_num = units[0]._disposition._platoon_num; //첫번째 유닛의 소대번호를 사용 
    //        platoon._units = units;

    //        //분대가 몇개인지 검색한다 
    //        int max_squard = 0;
    //        for (int i = 0; i < units.Count; i++)
    //        {
    //            if (max_squard < units[i]._disposition._squard_num)
    //                max_squard = units[i]._disposition._squard_num;
    //        }
    //        platoon._squard_count = max_squard + 1;

    //        Squard squard = null;
    //        for (int i = 0; i < platoon._squard_count; i++)
    //        {
    //            squard = Squard.Create_Squard(platoon, i);
    //            platoon._squards.Add(squard);
    //        }

    //        //DebugWide.LogBlue(platoon._squards.Count);
    //        return platoon;
    //    }

    //    public void ApplyFormation_SQD1_Width()
    //    {
    //        //_squards[0]._form_rect.row = 2;
    //        _squards[0]._form_rect.column = 10;
    //        _squards[0]._form_rect.between_x = 1.2f;
    //        _squards[0]._form_rect.between_z = 1.2f;
    //        _squards[0]._form_rect.horn = 0;
    //        _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[0]._formation._initPos = new Vector3(0, 0, 3);
    //        _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
    //        _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[0].ApplyFormation();
    //    }

    //    public void ApplyFormation_SQD1_Height()
    //    {
    //        //_squards[0]._form_rect.row = 7;
    //        _squards[0]._form_rect.column = 3;
    //        _squards[0]._form_rect.between_x = 1.2f;
    //        _squards[0]._form_rect.between_z = 2.5f;
    //        _squards[0]._form_rect.horn = 0;
    //        _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[0]._formation._initPos = new Vector3(0, 0, 3);
    //        _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
    //        _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[0].ApplyFormation();
    //    }

    //    //십자가형 배치
    //    public void ApplyFormation_SQD4_Cross()
    //    {
    //        if (_squard_count != 4) return;

    //        _isFollow = false;
    //        _isDirMatch = true;


    //        //_squards[0]._form_rect.row = 3;
    //        _squards[0]._form_rect.column = 5;
    //        _squards[0]._form_rect.between_x = 1.2f;
    //        _squards[0]._form_rect.between_z = 1.2f;
    //        _squards[0]._form_rect.horn = 0;
    //        _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[0]._formation._initPos = new Vector3(0, 0, 3);
    //        _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
    //        _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[0].ApplyFormation();

    //        _squards[1]._form_rect = _squards[0]._form_rect;
    //        _squards[1]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[1]._formation._initPos = new Vector3(-5, 0, -3);
    //        _squards[1]._formation._offset = _squards[1]._formation._initPos - _form_standard;
    //        _squards[1]._pos = (_rotation * _squards[1]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[1].ApplyFormation();

    //        _squards[2]._form_rect = _squards[0]._form_rect;
    //        _squards[2]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[2]._formation._initPos = new Vector3(5, 0, -3);
    //        _squards[2]._formation._offset = _squards[2]._formation._initPos - _form_standard;
    //        _squards[2]._pos = (_rotation * _squards[2]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[2].ApplyFormation();

    //        _squards[3]._form_circle.radius = 3;
    //        _squards[3]._eFormKind = Squard.eFormKind.Circle;
    //        _squards[3]._formation._initPos = new Vector3(0, 0, -6);
    //        _squards[3]._formation._offset = _squards[3]._formation._initPos - _form_standard;
    //        _squards[3]._pos = (_rotation * _squards[3]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[3].ApplyFormation();
    //    }

    //    //길게 배치
    //    public void ApplyFormation_SQD4_String()
    //    {
    //        if (_squard_count != 4) return;

    //        _isFollow = true;
    //        _isDirMatch = false;
    //        _follow_gap = 5;


    //        //_squards[0]._form_rect.row = 3;
    //        _squards[0]._form_rect.column = 5;
    //        _squards[0]._form_rect.between_x = 1.2f;
    //        _squards[0]._form_rect.between_z = 1.2f;
    //        _squards[0]._form_rect.horn = 0;
    //        _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[0]._formation._initPos = new Vector3(0, 0, 0);
    //        _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
    //        _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[0].ApplyFormation();

    //        _squards[1]._form_rect = _squards[0]._form_rect;
    //        _squards[1]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[1]._formation._initPos = new Vector3(0, 0, -3);
    //        _squards[1]._formation._offset = _squards[1]._formation._initPos - _form_standard;
    //        _squards[1]._pos = (_rotation * _squards[1]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[1].ApplyFormation();

    //        _squards[2]._form_rect = _squards[0]._form_rect;
    //        _squards[2]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[2]._formation._initPos = new Vector3(0, 0, -6);
    //        _squards[2]._formation._offset = _squards[2]._formation._initPos - _form_standard;
    //        _squards[2]._pos = (_rotation * _squards[2]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[2].ApplyFormation();

    //        _squards[3]._form_rect = _squards[0]._form_rect;
    //        _squards[3]._eFormKind = Squard.eFormKind.Rectangular;
    //        _squards[3]._formation._initPos = new Vector3(0, 0, -9);
    //        _squards[3]._formation._offset = _squards[3]._formation._initPos - _form_standard;
    //        _squards[3]._pos = (_rotation * _squards[3]._formation._offset) + _pos; //PointToWorldSpace 
    //        _squards[3].ApplyFormation();
    //    }

    //    override public void Update(float deltaTime)
    //    {
    //        base.Update(deltaTime);

    //        Vector3 toDir = _pos - _targetPos;
    //        Vector3 beforePos = _pos;
    //        for (int i=0;i< _squards.Count;i++)
    //        {
    //            if(false == _squards[i]._Solo_Activity)
    //            {
    //                if(false == _isFollow)
    //                {
    //                    //1* 오프셋위치로 고정위치를 계산함 
    //                    _squards[i]._targetPos = (_rotation * _squards[i]._formation._offset) + _pos; //PointToWorldSpace  
    //                }
    //                else
    //                {
    //                    //1* 일정거리를 두며 앞에 분대를 따라가게 계산함 
    //                    _squards[i]._targetPos = beforePos + (_squards[i]._pos - beforePos).normalized * _follow_gap;
    //                    beforePos = _squards[i]._pos; 
    //                }

    //                if(_isDirMatch)
    //                {
    //                    //분대방향을 소대방향과 일치시킨다 
    //                    _squards[i]._pos = _squards[i]._targetPos + toDir;
    //                }

    //            }
    //            _squards[i].Update(deltaTime);
    //        }
    //    }

    //    public void Draw(Color color)
    //    {
    //        base.Draw(Color.red);

    //        for (int i = 0; i < _squards.Count; i++)
    //        {
    //            _squards[i].Draw(color);
    //        }
    //    }
    //}

    //======================================================




    //======================================================
}//end namespace



