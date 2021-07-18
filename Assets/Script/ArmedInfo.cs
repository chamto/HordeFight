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

        public MovingModel.Frame _frame = new MovingModel.Frame();

        public GameObject _go_view = null;
        public Transform _tr_frame = null;
        public Transform _tr_frame_start = null;
        public Transform _tr_frame_end = null;

        public Transform _tr_view = null;
        public Transform _tr_view_shadow = null;
        //public SpriteMesh _view_spr = null;
        public SpriteRenderer _view_spr = null;
        //무기의 시작과 끝위치
        public Vector3 _view_arms_start = ConstV.v3_zero;
        public Vector3 _view_arms_end = ConstV.v3_zero;


        public string _name = "";
        public eIdx _eIdx = eIdx.None;
        public float _length = 1f;
        public float _radius = 0.05f;

        public void Init(Transform arm)
        {
            if (null == (object)arm) return;

            _go_view = arm.gameObject;
            _tr_view = Hierarchy.GetTransform(arm, "view"); ;
            _tr_view_shadow = Hierarchy.GetTransform(_tr_view, "shadow");
            //_view_spr = Hierarchy.GetTransform(_tr_view, "arm_spr").GetComponent<SpriteMesh>();
            _view_spr = Hierarchy.GetTransform(_tr_view, "arm_spr").GetComponent<SpriteRenderer>();

            _tr_frame = Hierarchy.GetTransform(arm, "frame");
            _frame.Init(_tr_frame , "root");
            _tr_frame_start =    _frame._info[0].start;
            _tr_frame_end =      _frame._info[0].end;

            //DebugWide.LogBlue(_arm_spr + " : " + Hierarchy.GetTransform(_tr_arm, "arm_spr").name);

            _length = (_tr_frame_start.position - _tr_frame_end.position).magnitude;
            if(float.Epsilon > _length)
            {
                //DebugWide.LogBlue(_name);
                _length = 0.001f;
            }
        }

        public void Update_Frame(Transform frame)
        {
            
            _tr_frame.position = frame.position;
            _tr_frame.rotation = frame.rotation;

        }

        public void SetActive(bool value)
        {
            _go_view.SetActive(value); //chamto test
        }

        public void Draw(Color color)
        {
            DebugWide.DrawCircle(_tr_frame_end.position, _radius, color);
            DebugWide.DrawLine(_tr_frame_start.position, _tr_frame_end.position, color);
        }
    }
}


