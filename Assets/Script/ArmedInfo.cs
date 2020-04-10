using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace HordeFight
{
    public class ArmedInfo : MonoBehaviour
    {
        public enum eIdx
        {
            None = 0,
            Federschwert,
            LongSword,
            Spear,
            Staff,

            Sword_0,
            Sword_1,
            Shield,

        }

        public Transform _tr_arm = null;
        public GameObject _go_arm = null;
        public Transform _arm_start = null;
        public Transform _arm_end = null;
        public Transform _arm_shadow = null;
        public SpriteMesh _arm_spr = null;
        public LineSegment3 _line;

        //public int _idx = -1;
        public string _name = "";
        public eIdx _eIdx = eIdx.None;
        public float _length = 1f;
        //public bool _equipment = false;

        public void Init(Transform arm)
        {
            if (null == (object)arm) return;

            _tr_arm = arm;
            _go_arm = arm.gameObject;
            _arm_start =    Hierarchy.GetTransform(_tr_arm, "start");
            _arm_end =      Hierarchy.GetTransform(_tr_arm, "end");
            _arm_shadow =   Hierarchy.GetTransform(_tr_arm, "shadow");
            _arm_spr =      Hierarchy.GetTransform(_tr_arm, "arm_spr").GetComponent<SpriteMesh>();
            //DebugWide.LogBlue(_arm_spr + " : " + Hierarchy.GetTransform(_tr_arm, "arm_spr").name);

            //_idx = idx;
            _length = (_arm_start.position - _arm_end.position).magnitude;
            _line = new LineSegment3(_arm_start.position, _arm_end.position);
        }

        //실제 갱신이 일어나지 않는 처리임 
        //public void SetArmPos(LineSegment3 arm)
        //{
        //    _arm_start.position = arm.origin;
        //    _arm_end.position = arm.last;
        //}

        public void Update_Info(Transform parent)
        {
            _tr_arm.position = parent.position;
            _tr_arm.rotation = parent.rotation;

            _line.origin = _arm_start.position;
            //_line.direction = _arm_end.position - _line.origin;
            _line.last = _arm_end.position;
        }

        public void SetActive(bool value)
        {
            //_equipment = value;
            _go_arm.SetActive(value);
        }

        public void DrawInfo(Color color)
        {
            DebugWide.DrawLine(_arm_start.position, _arm_end.position, color);
        }
    }
}


