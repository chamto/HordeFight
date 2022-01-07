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

        //변경가능정보
        public Vector3 _initPos; //초기위치 
        public Vector3 _offset;

        public void Init()
        {
            _platoon_num = -1;
            _squard_num = -1;
            _squard_pos = -1;

            _initPos = Vector3.zero;
            _offset = Vector3.zero;
        }
    }

    //진형
    public class FormationInfo
    {

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
        }

        public eFormKind _eFormKind = eFormKind.None;
        public Platoon _platoon = null; //속한 소대 
        public int _squard_num = -1;
        public int _unit_count = 0; //전체분대원수 


        public int _form_row; //행
        public int _form_column; //열
        public float _form_horn; //width 진형의 뿔 , 양수면 앞으로 나오고 음수면 뒤로 나온다 
        public float _form_dis_between; //사이거리 
        public float _form_radius; //원형진형 반지름


        //public Vector3 _dir = Vector3.forward; //방향 
        public Vector3 _standard = Vector3.zero; //기준점
        public List<Character> _units = new List<Character>(); //전체배치정보

        //==================================================

        static public Squard Create_Squard(Platoon platoon, int squard_num)
        {
            Squard squard = new Squard();
            squard._platoon = platoon;
            squard._squard_num = squard_num;
            squard._eFormKind = eFormKind.None;

            //list 순서와 squard_pos를 일치시키기 위해 미리 만들어 놓는다 
            Character unit = null;
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

                //dpos._initPos = new Vector3(i * 1, 0, 0);
                //dpos._offset = dpos._initPos - squard._standard;
                squard._units[unit._disposition._squard_pos] = unit;
                unit._platoon = platoon;
                unit._squard = squard;
            }


            //---------------------------

            return squard;
        }

        //유닛을 분대에서 뺀다 
        public bool UnitDismiss(Character unit)
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
        public bool UnitInclude(Character unit)
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
        public List<Character> _units = new List<Character>();
        public List<Squard> _squards = new List<Squard>();

        static public Platoon Create_Platoon (List<Character> units)
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
            for (int i = 0; i < max_squard; i++)
            {
                squard = Squard.Create_Squard(platoon, i);
                platoon._squards.Add(squard);
            }


            return platoon;
        }


    }

}//end namespace



