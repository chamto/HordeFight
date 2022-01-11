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

    //전달모임 , Transmission , 최소한의 배치 , 역할의 단위 
    public class Squard : OrderPoint
    {
        public enum eFormKind
        {
            None = 0,
            Width = 1,
            Height = 2,
            Circle = 3,
            Data = 4,
        }

        public eFormKind _eFormKind = eFormKind.None;
        public Platoon _platoon = null; //속한 소대 
        public int _squard_num = -1;
        public int _unit_count = 0; //전체분대원수 
        public bool _Solo_Activity = false; //단독활동 

        //public Vector3 _dir = Vector3.forward; //방향 
        public Vector3 _form_standard = Vector3.zero; //기준점
        public int _form_row; //행
        public int _form_column; //열
        public float _form_horn; //width 진형의 뿔 , 양수면 앞으로 나오고 음수면 뒤로 나온다 
        public float _form_dis_between; //사이거리 
        public float _form_radius; //원형진형 반지름
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

        private void CalcOffset_Width()
        {
            for (int row = 0; row < _form_row; row++)
            {
                for (int col = 0; col < _form_column; col++)
                {
                    int idx = col +( row * _form_column);
                    if (idx >= _units.Count) return;

                    _units[idx]._formation._initPos = new Vector3(col * _form_dis_between, 0, -row * _form_dis_between);
                    _units[idx]._formation._offset = _units[idx]._formation._initPos - _form_standard;

                }

            }
        }

        public void ApplyFormationOffset()
        {
            //임시작성
            if (_eFormKind == eFormKind.Width)
            {
                _form_row = 3;
                _form_column = 5;
                _form_dis_between = 1.2f;
                int center = _unit_count < _form_column ?  _unit_count : _form_column;
                _form_standard = new Vector3((center - 1) * _form_dis_between * 0.5f, 0, 0); //중앙을 기준점으로 삼는다 
                //for (int i = 0; i < _units.Count; i++)
                //{
                //    _units[i]._formation._initPos = new Vector3(i * 1, 0, 0);
                //    _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
                //}
                CalcOffset_Width();
            }
            //임시작성
            if (_eFormKind == eFormKind.Circle)
            {
                _form_radius = 2;
                _form_standard = new Vector3(0, 0, _form_radius);
                float angle = 360 / _unit_count;
                for (int i = 0; i < _units.Count; i++)
                {
                    Quaternion rot = Quaternion.AngleAxis(angle*i, Vector3.up);
                    _units[i]._formation._initPos = rot * new Vector3(0, 0, _form_radius);
                    _units[i]._formation._offset = _units[i]._formation._initPos - _form_standard;
                }


            }
        }

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

        //십자가형 배치
        public void ApplyFormationOffset_0()
        {
            if (_squard_count != 4) return;

            _squards[0]._eFormKind = Squard.eFormKind.Circle;
            _squards[0]._formation._initPos = new Vector3(0, 0, 3);
            _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
            _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[0].ApplyFormationOffset();

            _squards[1]._eFormKind = Squard.eFormKind.Width;
            _squards[1]._formation._initPos = new Vector3(-3, 0, -3);
            _squards[1]._formation._offset = _squards[1]._formation._initPos - _form_standard;
            _squards[1]._pos = (_rotation * _squards[1]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[1].ApplyFormationOffset();

            _squards[2]._eFormKind = Squard.eFormKind.Width;
            _squards[2]._formation._initPos = new Vector3(3, 0, -3);
            _squards[2]._formation._offset = _squards[2]._formation._initPos - _form_standard;
            _squards[2]._pos = (_rotation * _squards[2]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[2].ApplyFormationOffset();

            _squards[3]._eFormKind = Squard.eFormKind.Width;
            _squards[3]._formation._initPos = new Vector3(0, 0, -6);
            _squards[3]._formation._offset = _squards[3]._formation._initPos - _form_standard;
            _squards[3]._pos = (_rotation * _squards[3]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[3].ApplyFormationOffset();
        }

        //길게 배치
        public void ApplyFormationOffset_1()
        {
            if (_squard_count != 4) return;

            _squards[0]._eFormKind = Squard.eFormKind.Width;
            _squards[0]._formation._initPos = new Vector3(0, 0, 0);
            _squards[0]._formation._offset = _squards[0]._formation._initPos - _form_standard;
            _squards[0]._pos = (_rotation * _squards[0]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[0].ApplyFormationOffset();

            _squards[1]._eFormKind = Squard.eFormKind.Width;
            _squards[1]._formation._initPos = new Vector3(0, 0, -3);
            _squards[1]._formation._offset = _squards[1]._formation._initPos - _form_standard;
            _squards[1]._pos = (_rotation * _squards[1]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[1].ApplyFormationOffset();

            _squards[2]._eFormKind = Squard.eFormKind.Width;
            _squards[2]._formation._initPos = new Vector3(0, 0, -6);
            _squards[2]._formation._offset = _squards[2]._formation._initPos - _form_standard;
            _squards[2]._pos = (_rotation * _squards[2]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[2].ApplyFormationOffset();

            _squards[3]._eFormKind = Squard.eFormKind.Width;
            _squards[3]._formation._initPos = new Vector3(0, 0, -9);
            _squards[3]._formation._offset = _squards[3]._formation._initPos - _form_standard;
            _squards[3]._pos = (_rotation * _squards[3]._formation._offset) + _pos; //PointToWorldSpace 
            _squards[3].ApplyFormationOffset();
        }

        public void Update(float deltaTime)
        {
            base.Update(deltaTime);

            Vector3 beforePos = _pos;
            for (int i=0;i< _squards.Count;i++)
            {
                if(false == _squards[i]._Solo_Activity)
                {
                    //1* 오프셋위치르 고정위치를 계산함 
                    _squards[i]._targetPos = (_rotation * _squards[i]._formation._offset) + _pos; //PointToWorldSpace  

                    //1* 일정거리를 두며 앞에 분대를 따라가게 계산함 
                    //_squards[i]._targetPos = beforePos + (_squards[i]._pos - beforePos).normalized * 3;
                    //beforePos = _squards[i]._pos;
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



