using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Proto_AI_4
{
    //배치
    public struct Disposition
    {
        //고정정보
        public int _platoon_num;
        public int _squard_num;
        public int _squard_pos;  //분대 개별위치 - 분대 유닛리스트와 일치되어야 함

        public void Init()
        {
            _platoon_num = -1;
            _squard_num = -1;
            _squard_pos = -1;
        }
    }

    //진형
    public struct Formation
    {
        public Vector3 _initPos; //초기위치 
        public Vector3 _offset;

        public void Init()
        {
            _initPos = Vector3.zero;
            _offset = Vector3.zero;
        }
    }

    public struct FormRectangular
    {
        //public int row; //행
        public int column; //열
        public float horn; //width 진형의 뿔 , 양수면 앞으로 나오고 음수면 뒤로 나온다 
        public float between_x; //사이거리 
        public float between_z; //사이거리 
    }

    public struct FormCircle
    {
        public float radius; //원형진형 반지름
    }

    //전달모임 , Transmission , 최소한의 배치 , 역할의 단위 
    public class Squard : OrderPoint
    {
        public enum eFormKind
        {
            None = 0,
            Rectangular = 1,
            Circle = 2,
            Data = 3,
        }

        public eFormKind _eFormKind = eFormKind.None;
        public Platoon _platoon = null; //속한 소대 
        public int _squard_num = -1;
        public int _unit_count = 0; //전체분대원수 
        public bool _Solo_Activity = false; //단독활동 

        //public Vector3 _dir = Vector3.forward; //방향 
        public Vector3 _form_standard = Vector3.zero; //기준점
        //public int _form_row; //행
        //public int _form_column; //열
        //public float _form_horn; //width 진형의 뿔 , 양수면 앞으로 나오고 음수면 뒤로 나온다 
        //public float _form_dis_between; //사이거리 
        //public float _form_radius; //원형진형 반지름
        public FormRectangular _form_rect;
        public FormCircle  _form_circle;
        public Formation _formation;

        public List<Unit> _units = new List<Unit>(); //전체배치정보

        //==================================================

        static public Squard Create_Squard(Platoon platoon, int squard_num)
        {
            Squard squard = new Squard();
            squard._platoon = platoon;
            squard._squard_num = squard_num;
            squard._eFormKind = eFormKind.None;

            //list 순서와 squard_pos를 일치시키기 위해 미리 만들어 놓는다 
            Unit unit = null;
            for (int i = 0; i < platoon._units.Count; i++)
            {
                unit = platoon._units[i];
                if (squard._squard_num != unit._disposition._squard_num) continue;
                squard._unit_count++;
                squard._units.Add(null); 
            }

            //소대전체 유닛에서 분대에 속하는 유닛만 찾아 추가한다 
            for (int i = 0; i < platoon._units.Count; i++)
            {
                unit = platoon._units[i];
                if (squard._squard_num != unit._disposition._squard_num) continue;

                squard._units[unit._disposition._squard_pos] = unit;
                unit._platoon = platoon;
                unit._squard = squard;
            }


            //---------------------------

            return squard;
        }

        //_formation 값을 채운다 
        private void CalcOffset_Rect()
        {
            int max_row = (_units.Count / _form_rect.column) + 1;
            int len_x = _unit_count < _form_rect.column ? _unit_count : _form_rect.column;
            float center_x = (len_x - 1) * _form_rect.between_x * 0.5f; //중앙 길이를 구한다
            //_form_standard = new Vector3(center_x, 0, 0); 

            float tan =  0 != center_x ? (_form_rect.horn / center_x) : 0;
            for (int row = 0; row < max_row; row++)
            {
                for (int col = 0; col < _form_rect.column; col++)
                {
                    int idx = col +( row * _form_rect.column);
                    if (idx >= _units.Count) return;

                    float x = col * _form_rect.between_x; 
                    float z = -row * _form_rect.between_z;

                    // z/x = tan , z = x * tan
                    float height = x * tan; //왼쪽 빗면
                    if(center_x < x ) //오른쪽 빗면
                    {
                        height = (center_x * 2 - x) * tan; 
                    }

                    x -= center_x; //중앙에 배치되도록 한다

                    _units[idx]._formation._initPos = new Vector3(x, 0, z + height);
                    _units[idx]._formation._offset = _units[idx]._formation._initPos - _form_standard;

                }

            }
        }

        private void CalcOffset_Circle()
        {
            _form_standard = new Vector3(0, 0, _form_circle.radius);
            float angle = 360 / _unit_count;
            for (int i = 0; i < _units.Count; i++)
            {
                Quaternion rot = Quaternion.AngleAxis(angle * i, Vector3.up);
                _units[i]._formation._initPos = rot * new Vector3(0, 0, _form_circle.radius);
                _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
            }
        }

        public void ApplyFormation()
        {

            if (_eFormKind == eFormKind.Rectangular)
            {
                //_form_rect.row = 3;
                //_form_rect.column = 5;
                //_form_rect.dis_between = 1.2f;
                //_form_rect.horn = 0; 
                //int len = _unit_count < _form_rect.column ?  _unit_count : _form_rect.column;
                //_form_standard = new Vector3((len - 1) * _form_rect.dis_between * 0.5f, 0, 0); //중앙을 기준점으로 삼는다 
                //for (int i = 0; i < _units.Count; i++)
                //{
                //    _units[i]._formation._initPos = new Vector3(i * 1, 0, 0);
                //    _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
                //}
                CalcOffset_Rect();
            }

            if (_eFormKind == eFormKind.Circle)
            {
                //_form_circle.radius = 2;
                //_form_standard = new Vector3(0, 0, _form_circle.radius);
                //float angle = 360 / _unit_count;
                //for (int i = 0; i < _units.Count; i++)
                //{
                //    Quaternion rot = Quaternion.AngleAxis(angle*i, Vector3.up);
                //    _units[i]._formation._initPos = rot * new Vector3(0, 0, _form_circle.radius);
                //    _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
                //}
                CalcOffset_Circle();


            }
        }

        //bool _isFollow = true;
        //float _follow_gap = 2;
        //override public void Update(float deltaTime)
        //{
        //    base.Update(deltaTime);

        //    //Vector3 toDir = _pos - _targetPos;
        //    Vector3 beforePos = _pos;
        //    for (int i = 0; i < _units.Count; i++)
        //    {
        //        if (false == _isFollow)
        //        {
        //            //1* 오프셋위치로 고정위치를 계산함 
        //            _units[i]._steeringBehavior._targetPos = (_rotation * _units[i]._formation._offset) + _pos; //PointToWorldSpace  
        //        }
        //        else
        //        {
        //            //1* 일정거리를 두며 앞에 분대를 따라가게 계산함 
        //            _units[i]._steeringBehavior._targetPos = beforePos + (_units[i]._pos - beforePos).normalized * _follow_gap;
        //            beforePos = _units[i]._pos;

        //        }

        //    }
        //}

        //유닛을 분대에서 뺀다 
        public bool UnitDismiss(Unit unit)
        {
            if (_platoon._platoon_num != unit._disposition._platoon_num) return false;
            if (_squard_num != unit._disposition._squard_num) return false;
            if (_units.Count <= unit._disposition._squard_pos) return false;

            _units[unit._disposition._squard_pos] = null;
            unit._platoon = null;
            unit._squard = null;
            unit._disposition.Init();

            return true; 
        }

        //유닛을 분대에 추가한다 
        public bool UnitInclude(Unit unit)
        {
            if (_platoon._platoon_num != unit._disposition._platoon_num) return false;
            if (_squard_num != unit._disposition._squard_num) return false;

            //분대유닛 목록을 미리 늘려준다
            if(_units.Count <= unit._disposition._squard_pos)
            {
                int addCount = unit._disposition._squard_pos - _units.Count + 1;
                for(int i=0;i<addCount;i++)
                {
                    _units.Add(null);
                }
            }

            _units[unit._disposition._squard_pos] = unit;
            unit._platoon = _platoon;
            unit._squard = this;

            return true;
        }
    }



    public class Platoon : OrderPoint
    {
        public int _platoon_num = -1;
        public int _squard_count = 0;
        public Vector3 _form_standard = Vector3.zero; //기준점

        public bool _isFollow = false;
        public float _follow_gap = 3;

        public bool _isDirMatch = true; //분대방향을 소대방향과 일치

        public List<Unit> _units = new List<Unit>();
        public List<Squard> _squards = new List<Squard>();

        static public Platoon Create_Platoon (List<Unit> units)
        {
            if (0 == units.Count) return null;

            Platoon platoon = new Platoon();
            platoon._platoon_num = units[0]._disposition._platoon_num; //첫번째 유닛의 소대번호를 사용 
            platoon._units = units;

            //분대가 몇개인지 검색한다 
            int max_squard = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (max_squard < units[i]._disposition._squard_num)
                    max_squard = units[i]._disposition._squard_num;
            }
            platoon._squard_count = max_squard + 1;

            Squard squard = null;
            for (int i = 0; i < platoon._squard_count; i++)
            {
                squard = Squard.Create_Squard(platoon, i);
                platoon._squards.Add(squard);
            }

            //DebugWide.LogBlue(platoon._squards.Count);
            return platoon;
        }

        public void ApplyFormation_SQD1_Width()
        {
            //_squards[0]._form_rect.row = 2;
            _squards[0]._form_rect.column = 10;
            _squards[0]._form_rect.between_x = 1.2f;
            _squards[0]._form_rect.between_z = 1.2f;
            _squards[0]._form_rect.horn = 0;
            _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[0]._formation._initPos = new Vector3(0, 0, 3);
            _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
            _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[0].ApplyFormation();
        }

        public void ApplyFormation_SQD1_Height()
        {
            //_squards[0]._form_rect.row = 7;
            _squards[0]._form_rect.column = 3;
            _squards[0]._form_rect.between_x = 1.2f;
            _squards[0]._form_rect.between_z = 2.5f;
            _squards[0]._form_rect.horn = 0;
            _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[0]._formation._initPos = new Vector3(0, 0, 3);
            _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
            _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[0].ApplyFormation();
        }

        //십자가형 배치
        public void ApplyFormation_SQD4_Cross()
        {
            if (_squard_count != 4) return;

            _isFollow = false;
            _isDirMatch = true;


            //_squards[0]._form_rect.row = 3;
            _squards[0]._form_rect.column = 5;
            _squards[0]._form_rect.between_x = 1.2f;
            _squards[0]._form_rect.between_z = 1.2f;
            _squards[0]._form_rect.horn = 0;
            _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[0]._formation._initPos = new Vector3(0, 0, 3);
            _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
            _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[0].ApplyFormation();

            _squards[1]._form_rect = _squards[0]._form_rect;
            _squards[1]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[1]._formation._initPos = new Vector3(-5, 0, -3);
            _squards[1]._formation._offset = _squards[1]._formation._initPos - _form_standard;
            _squards[1]._pos = (_rotation * _squards[1]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[1].ApplyFormation();

            _squards[2]._form_rect = _squards[0]._form_rect;
            _squards[2]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[2]._formation._initPos = new Vector3(5, 0, -3);
            _squards[2]._formation._offset = _squards[2]._formation._initPos - _form_standard;
            _squards[2]._pos = (_rotation * _squards[2]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[2].ApplyFormation();

            _squards[3]._form_circle.radius = 3;
            _squards[3]._eFormKind = Squard.eFormKind.Circle;
            _squards[3]._formation._initPos = new Vector3(0, 0, -6);
            _squards[3]._formation._offset = _squards[3]._formation._initPos - _form_standard;
            _squards[3]._pos = (_rotation * _squards[3]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[3].ApplyFormation();
        }

        //길게 배치
        public void ApplyFormation_SQD4_String()
        {
            if (_squard_count != 4) return;

            _isFollow = true;
            _isDirMatch = false;
            _follow_gap = 5;


            //_squards[0]._form_rect.row = 3;
            _squards[0]._form_rect.column = 5;
            _squards[0]._form_rect.between_x = 1.2f;
            _squards[0]._form_rect.between_z = 1.2f;
            _squards[0]._form_rect.horn = 0;
            _squards[0]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[0]._formation._initPos = new Vector3(0, 0, 0);
            _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
            _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[0].ApplyFormation();

            _squards[1]._form_rect = _squards[0]._form_rect;
            _squards[1]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[1]._formation._initPos = new Vector3(0, 0, -3);
            _squards[1]._formation._offset = _squards[1]._formation._initPos - _form_standard;
            _squards[1]._pos = (_rotation * _squards[1]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[1].ApplyFormation();

            _squards[2]._form_rect = _squards[0]._form_rect;
            _squards[2]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[2]._formation._initPos = new Vector3(0, 0, -6);
            _squards[2]._formation._offset = _squards[2]._formation._initPos - _form_standard;
            _squards[2]._pos = (_rotation * _squards[2]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[2].ApplyFormation();

            _squards[3]._form_rect = _squards[0]._form_rect;
            _squards[3]._eFormKind = Squard.eFormKind.Rectangular;
            _squards[3]._formation._initPos = new Vector3(0, 0, -9);
            _squards[3]._formation._offset = _squards[3]._formation._initPos - _form_standard;
            _squards[3]._pos = (_rotation * _squards[3]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[3].ApplyFormation();
        }

        override public void Update(float deltaTime)
        {
            base.Update(deltaTime);

            Vector3 toDir = _pos - _targetPos;
            Vector3 beforePos = _pos;
            for (int i=0;i< _squards.Count;i++)
            {
                if(false == _squards[i]._Solo_Activity)
                {
                    if(false == _isFollow)
                    {
                        //1* 오프셋위치로 고정위치를 계산함 
                        _squards[i]._targetPos = (_rotation * _squards[i]._formation._offset) + _pos; //PointToWorldSpace  
                    }
                    else
                    {
                        //1* 일정거리를 두며 앞에 분대를 따라가게 계산함 
                        _squards[i]._targetPos = beforePos + (_squards[i]._pos - beforePos).normalized * _follow_gap;
                        beforePos = _squards[i]._pos; 
                    }

                    if(_isDirMatch)
                    {
                        //분대방향을 소대방향과 일치시킨다 
                        _squards[i]._pos = _squards[i]._targetPos + toDir;
                    }

                }
                _squards[i].Update(deltaTime);
            }
        }

        public void Draw(Color color)
        {
            base.Draw(Color.red);

            for (int i = 0; i < _squards.Count; i++)
            {
                _squards[i].Draw(color);
            }
        }
    }

}//end namespace



