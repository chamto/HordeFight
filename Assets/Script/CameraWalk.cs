using UnityEngine;
//using UnityEngine.Assertions;

namespace HordeFight
{
    //========================================================
    //==================   카메라 이동 정보   ==================
    //========================================================

    public class CameraWalk : MonoBehaviour
    {

        public enum eKind
        {
            Basic,
            PixelPerfect,
            Fallow,
            MoveToFallow,
        }

        private Camera _camera = null;
        public Transform _target = null;

        public eKind _ekind = eKind.Fallow;
        //public bool _enable_PixelPerfect = false;
        public float _PIXEL_PER_UNIT = 16f; //ppu
        public float _PIXEL_LENGTH = 0.0625f; //게임에서 사용하는 픽셀하나의 크기 
        public float _FALLOW_RATE = 0.06f;
        public float _MOVE_DIS = 10f;

        private void Start()
        {
            _PIXEL_LENGTH = 1f / _PIXEL_PER_UNIT;
            _camera = SingleO.mainCamera;

        }

        private void Update()
        {
            if (null == _target || null == _camera) return;

            //_camera.cullingMask = 0;

            Vector3 targetPos = _target.position;
            targetPos.y = _camera.transform.position.y;


            switch (_ekind)
            {
                case eKind.Basic:
                    {
                        _camera.transform.position = targetPos;
                        break;
                    }
                case eKind.PixelPerfect:
                    {
                        //픽셀퍼팩트 처리 : PIXEL_LENGTH 단위로 이동하게, 버림 처리한다 
                        _camera.transform.position = ToPixelPerfect(targetPos, _PIXEL_PER_UNIT);
                        break;
                    }
                case eKind.Fallow:
                    {
                        Vector3 dir = targetPos - _camera.transform.position;
                        _camera.transform.position = _camera.transform.position + dir * _FALLOW_RATE;

                        break;
                    }
                case eKind.MoveToFallow:
                    {
                        Vector3 dir = targetPos - _camera.transform.position;
                        //DebugWide.LogBlue(dir.sqrMagnitude);
                        if (dir.sqrMagnitude > _MOVE_DIS)
                        {
                            _camera.transform.position = _camera.transform.position + dir * _FALLOW_RATE;
                        }

                        break;
                    }
            }//end switch


        }


        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public Vector3 ToPixelPerfect(Vector3 pos3d, float pixelPerUnit)
        {
            float pixel_length = 1f / pixelPerUnit;
            Vector3Int posXY_2d = new Vector3Int((int)pos3d.x, (int)pos3d.y, (int)pos3d.z);
            Vector3 decimalPoint = pos3d - posXY_2d;

            //예) 0.2341 => 0.0341
            Vector3 remainder = new Vector3(decimalPoint.x % pixel_length,
                                            decimalPoint.y % pixel_length,
                                            decimalPoint.z % pixel_length);


            return pos3d - remainder;
        }


    }

}

