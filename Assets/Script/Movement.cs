using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace HordeFight
{
    //========================================================
    //==================   캐릭터/구조물 정보   ==================
    //========================================================

    public class Movement : MonoBehaviour
    {
        //public const float ONE_METER = 0.16f; //타일 한개의 가로길이 
        //public const float SQR_ONE_METER = ONE_METER * ONE_METER;
        //public const float WorldToMeter = 1f / ONE_METER;
        //public const float MeterToWorld = ONE_METER;

        public Being _being = null;
        public GameObject _gameObject = null;
        public Transform _transform = null;

        public eDirection8 _eDir8 = eDirection8.down;
        //public float _dir_angleY = 0f;
        public Vector3 _direction = Vector3.back; //_eDir8과 같은 방향으로 설정한다  
        //private Vector3 _direction_prev = Vector3.back;
        public float _anglePerSecond = 180f; //초당 회전각 

        private float _speed_meterPerSecond = 1f;

        private bool _isNextMoving = false;
        private float _elapsedTime = 0f;
        private float _interTime_prev = 0;

        private Vector3 _startPos = ConstV.v3_zero;
        private Vector3 _lastTargetPos = ConstV.v3_zero;
        private Vector3 _nextTargetPos = ConstV.v3_zero;
        private Queue<Vector3> _targetPath = null;

        private float _oneMeter_movingTime = 0.8f; //임시처리
        private float _elapsed_movingTime = 0f;

        private void Start()
        {
            _gameObject = gameObject;
            _transform = transform;
            _direction = Misc.GetDir8_Normal3D_AxisY(_eDir8);
        }

        //private void Update()
        //{
        //    //UpdateNextPath();
        //}


        public void UpdateNextPath()
        {
            if (false == _isNextMoving) return;

            if (null == (object)_targetPath || 0 == _targetPath.Count)
            {
                //이동종료 
                _elapsed_movingTime = 0;
                _isNextMoving = false;
                return;
            }

            //1meter 가는데 걸리는 시간을 넘었다면 다시 경로를 찾는다
            if (_oneMeter_movingTime < _elapsed_movingTime)
            {
                _elapsed_movingTime = 0;

                MoveToTarget(_lastTargetPos, _speed_meterPerSecond); //test
            }
            //1meter 의 20% 길이에 해당하는 거리가 남았다면 도착 
            else if ((_nextTargetPos - _being.GetPos3D()).sqrMagnitude <= GridManager.SQR_ONE_METER * 0.2f)
            {
                _elapsed_movingTime = 0;

                _nextTargetPos = _targetPath.Dequeue();
                _nextTargetPos.y = 0f; //목표위치의 y축 값이 있으면, 위치값이 잘못계산됨 

                //회전쿼터니언을 추출해 방향값에 곱한다 => 새로운 방향 
                _direction = Quaternion.FromToRotation(_direction, VOp.Minus(_nextTargetPos, _transform.position)) * _direction;
                _eDir8 = Misc.GetDir8_AxisY(_direction);
                //_dir_angleY = Geo.AngleSigned_AxisY(ConstV.v3_forward, _direction);
            }
            _elapsed_movingTime += Time.deltaTime;



            //1초후 초기화 
            if (_speed_meterPerSecond < _elapsedTime)
            {
                _elapsedTime = 0;
                _interTime_prev = 0;
            }
            _elapsedTime += Time.deltaTime;


            Move_Interpolation(_direction, 2f, _speed_meterPerSecond); //2 미터를 1초에 간다

            //this.transform.position = Vector3.Lerp(this.transform.position, _targetPos, _elapsedTime / (_onePath_movingTime * _speed));

            DebugVeiw_DrawPath_MoveToTarget(); //chamto test
        }

        //public void UpdateMove()
        //{
        //    _elapsedTime += Time.deltaTime;
        //}

        public void SetNextMoving(bool isNext)
        {
            _isNextMoving = isNext;
        }

        public bool IsMoving()
        {
            return _isNextMoving;
        }

        public void MoveToTarget(Vector3 targetPos, float speed_meterPerSecond)
        {
            _targetPath = null; //기존 설정 경로를 비운다
            targetPos.y = 0;

            _isNextMoving = true;
            _startPos = _being.GetPos3D();//_transform.position;
            _lastTargetPos = targetPos;
            _nextTargetPos = _being.GetPos3D();//_transform.position; //현재위치를 넣어 바로 다음 경로를 꺼내게 한다 
            _speed_meterPerSecond = 1f / speed_meterPerSecond; //역수를 넣어준다. 숫자가 커질수록 빠르게 설정하기 위함이다 

            //연속이동요청시에 이동처리를 할수 있게 주석처리함
            _elapsedTime = 0;
            _interTime_prev = 0;

            _targetPath = SingleO.pathFinder.Search(_being.GetPos3D(), targetPos);

            //초기방향을 구한다
            _eDir8 = Misc.GetDir8_AxisY(_targetPath.First() - _startPos);
            _direction = Misc.GetDir8_Normal3D_AxisY(_eDir8);
        }


        private void Move_Interpolation(Vector3 dir, float meter, float perSecond)
        {
            //float interpolationTime = Interpolation.easeInBack(0f, 1f, (_elapsedTime) / perSecond); //슬라임
            //float interpolationTime = Interpolation.easeInOutSine(0f, 1f, (_elapsedTime) / perSecond); //말탄애들 
            //float interpolationTime = Interpolation.punch( 1f, (_elapsedTime) / perSecond);

            float interpolationTime = Interpolation.linear(0f, 1f, (_elapsedTime) / perSecond);


            //보간이 들어갔을때 : Tile.deltaTime 와 같은 간격을 구하기 위해, 현재보간시간에서 전보간시간을 빼준다  
            //_transform.Translate(dir * (GridManager.ONE_METER * meter) * (interpolationTime - _prevInterpolationTime));
            Vector3 newPos = _being.GetPos3D() + dir * (GridManager.ONE_METER * meter) * (interpolationTime - _interTime_prev);
            _being.SetPos(newPos);

            //보간 없는 기본형
            //this.transform.Translate(dir * (ONE_METER * meter) * (Time.deltaTime * perSecond));

            _interTime_prev = interpolationTime;
        }


        //초당회전각 값에 따라 회전량을 구한다 
        private void RotateDirection(Vector3 dir)
        {
            Vector3 dir_prev = _direction;
            dir_prev.y = 0; dir.y = 0;
            Vector3 up = Vector3.Cross(dir_prev, dir);
            float test_0 = Vector3.Dot(dir_prev, dir);
            bool upZero = Misc.IsZero(up.y);
            float angle = Time.deltaTime * _anglePerSecond;

            if (180f > angle)
            {

                if (true == upZero && 0 > test_0)
                {
                    //각도가 정확히 180도 차이가 나는 경우 외적값을 구하지 못하므로 기본up값을 넣어준다 
                    up = ConstV.v3_up;
                    _direction = Quaternion.AngleAxis(angle, up) * _direction;
                    //DebugWide.LogBlue(VOp.ToString(up) + "  " + angle + "  " + _direction + "    : " + dir + "    : " + _being.name); //chamto test
                }
                else if (false == upZero)
                {
                    _direction = Quaternion.AngleAxis(angle, up) * _direction;
                    //DebugWide.LogBlue(VOp.ToString(up) + "  " + angle + "  " + _direction + "    : " + dir + "    : " + _being.name); //chamto test
                }
                //else
                //{
                //    //현재방향과 목표방향이 같을때는 방향을 구하지 않는다 
                //}
            }

            //요청방향을 넘어서 회전한 경우 요청방향으로 맞춘다 
            //회전할 각도가 180도 보다 크다면 요청방향으로 바로 맞춘다
            Vector3 test_1 = Vector3.Cross(dir, _direction);
            if (Vector3.Dot(up, test_1) > 0 || 180f <= angle)
            {
                _direction = Quaternion.FromToRotation(dir_prev, dir) * dir_prev;
                //DebugWide.LogRed(VOp.ToString(test_1) + "  " + angle + "  " + _direction + "    : " + dir + "    : " + _being.name); //chamto test
            }

        }

        public Interpolation.eKind __interKind = Interpolation.eKind.linear;
        public void Move_Forward(Vector3 dir, float meter, float perSecond)
        {

            //dir.Normalize();
            _isNextMoving = true;


            RotateDirection(dir);
            _eDir8 = Misc.GetDir8_AxisY(_direction);

            //===========================================
            _elapsedTime += Time.deltaTime;
            if (perSecond < _elapsedTime)
            {
                _elapsedTime = 0;

                _interTime_prev = 0;
            }

            float interTime = Interpolation.Calc(__interKind, 0, 1f, _elapsedTime / perSecond);
            if (0 > interTime) interTime = 0;
            else if (1 < interTime) interTime = 1;
            //보간이 들어갔을때 : Tile.deltaTime 와 같은 간격을 구하기 위해, 현재보간시간에서 전보간시간을 빼준다  
            float tt_delta = interTime - _interTime_prev;

            _interTime_prev = interTime;
            //DebugWide.LogBlue(interTime + "  " + _interTime_prev + "  "  + tt_delta + "  el: " +  _elapsedTime + "  ps: "  + perSecond );
            //===========================================


            //perSecond = 1f / perSecond;
            //보간 없는 기본형
            ////this.transform.Translate(_direction * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond));
            //Vector3 newPos = _being.GetPos3D() + _direction * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond);
            //Vector3 newPosBounds = _being.GetPos3D() + _direction * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond) * _being._collider_radius;
            Vector3 newPos = _being.GetPos3D() + _direction * (GridManager.ONE_METER * meter) * tt_delta;
            Vector3 newPosBounds = _being.GetPos3D() + _direction * (GridManager.ONE_METER * meter) * (tt_delta) * _being._collider_radius;

            //================================

            if (null != SingleO.cellPartition)
            {
                //!!!! 셀에 원의 중점 기준으로만 등록되어 있어서 가져온 셀정보 외에서도 충돌가능원이 있을 가능성이 매우 높다. 즉 제대로 처리 할 수가 없다  
                //새로운 위치로 이동이 가능한지 검사 
                //CellSpace cell = SingleO.cellPartition.GetCellSpace(newPos); //원의 중점인 자기위치의 타일정보만 가져오는 결과가 됨
                CellSpace cell = SingleO.cellPartition.GetCellSpace(newPosBounds); //객체 반지름의 경계면의 타일정보를 가져온다  
                if (null != cell)
                {
                    //DebugWide.LogBlue(cell._childCount);
                    bool moveable = true;
                    Being cur = cell._children;
                    for (int i = 0; i < cell._childCount; i++)
                    {
                        if ((object)_being == (object)cur) continue;
                        Vector3 between = cur.GetPos3D() - newPos;
                        float sumRadius = cur._collider_radius + _being._collider_radius;
                        if (between.sqrMagnitude <= sumRadius * sumRadius)
                        {
                            moveable = false;
                            break;
                        }


                        cur = cell._children._next_sibling;
                    }

                    if (true == moveable)//&& 3 > cell._childCount)
                    {
                        //DebugWide.LogBlue("sdfsdf"); //두 객체가 같은 셀에 있을경우 여기에 도달 못하는 문제 발견 
                        _being.SetPos(newPos);
                    }

                }
            }
            else
            {
                _being.SetPos(newPos);
            }


            //================================

        }

        public void SetDirection(Vector3 dir)
        {
            _direction = Quaternion.FromToRotation(_direction, dir) * _direction;
            _eDir8 = Misc.GetDir8_AxisY(dir);
        }

        public void Move_LookAt(Vector3 moveDir, Vector3 lookAt, float meter, float perSecond)
        {
            SetDirection(lookAt);

            _isNextMoving = true;
            perSecond = 1f / perSecond;
            //보간 없는 기본형
            //this.transform.Translate(dir * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond));
            Vector3 newPos = _being.GetPos3D() + moveDir * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond);
            _being.SetPos(newPos);
        }

        //dir 이 정규화 되어 있다 가정함 
        public void Move_Push(Vector3 dir, float meter, float perSecond)
        {
            _isNextMoving = true;
            perSecond = 1f / perSecond;
            //보간 없는 기본형
            //this.transform.Translate(dir * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond));
            Vector3 newPos = _being.GetPos3D() + dir * (GridManager.ONE_METER * meter) * (Time.deltaTime * perSecond);
            _being.SetPos(newPos);
        }

        public void DebugVeiw_DrawPath_MoveToTarget()
        {
            Vector3 prev = _targetPath.FirstOrDefault();

            foreach (Vector3 pos3d in _targetPath)
            {
                DebugWide.DrawLine(prev, pos3d, Color.green);
                prev = pos3d;
            }
        }

    }

}


namespace HordeFight
{

    //public class Movable : MonoBehaviour
    //{

    //public Vector3 GetDirect(Vector3 dstPos)
    //{
    //    Vector3 dir = dstPos - this.transform.position;
    //    dir.Normalize();
    //    return dir;
    //}

    //객체의 전진 방향을 반환한다.
    //public Vector3 GetForwardDirect()
    //{
    //    return Quaternion.Euler(this.transform.localEulerAngles) * Vector3.forward;
    //}


    //내방향을 기준으로 목표위치가 어디쪽에 있는지 반환한다.  
    //public eDirection DirectionalInspection(Vector3 targetPos)
    //{

    //    Vector3 mainDir = GetForwardDirect();

    //    Vector3 targetTo = targetPos - this.transform.localPosition;

    //    //mainDir.Normalize();
    //    //targetTo.Normalize();

    //    Vector3 dir = Vector3.Cross(mainDir, targetTo);
    //    //dir.Normalize();
    //    //DebugWide.LogBlue("mainDir:" + mainDir + "  targetTo:" + targetTo + "   cross:" + dir.y);

    //    float angle = Vector3.Angle(mainDir, targetTo);
    //    angle = Mathf.Abs(angle);

    //    if (angle < 3f) return eDirection.CENTER; //사이각이 3도 보다 작다면 중앙으로 여긴다 

    //    //외적의 y축값이 음수는 왼쪽방향 , 양수는 오른쪽방향 
    //    if (dir.y < 0)
    //        return eDirection.LEFT;
    //    else if (dir.y > 0)
    //        return eDirection.RIGHT;

    //    return eDirection.CENTER;
    //}


    //회전할 각도 구하기
    //public float CalcRotationAngle(Vector3 targetDir)
    //{
    //    //atan2로 각도 구하는 것과 같음. -180 ~ 180 사이의 값을 반환
    //    return Vector3.SignedAngle(GetForwardDirect(), targetDir, ConstV.v3_up);

    //}
    //}

}
