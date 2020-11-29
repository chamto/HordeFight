using System;
using UnityEngine;
//using UnityEngine.Assertions;
using UtilGS9;

namespace HordeFight
{
    public class Shot : Being
    {
        public Being _owner = null; //발사체를 소유한 객체
        public bool _on_theWay = false; //발사되어 날아가는 중
        private Transform _sprParent = null;
        private Transform _shader = null;



        //private void Start()
        //{
        //    //base.Init();

        //}

        public override void Init()
        {
            base.Init();

            //_sprParent = SingleO.hierarchy.GetTransformA(SingleO.hierarchy.GetFullPath(transform) + "/pos");
            //_shader = SingleO.hierarchy.GetTransformA(SingleO.hierarchy.GetFullPath(transform) + "/shader");

            //_sprParent = SingleO.hierarchy.GetTransformA(transform, "/pos");
            //_shader = SingleO.hierarchy.GetTransformA(transform, "/shader");
            //this.gameObject.SetActive(false); //start 함수 호출하는 메시지가 비활성객체에는 전달이 안된다


            _sprParent = Hierarchy.GetTransform(transform, "ani_spr");
            _shader = Hierarchy.GetTransform(transform, "shader");


        }

        //private void Update()
        //{

        //    Update_Shot();
        //    //Update_CellInfo(); //매프레임 마다 충돌검사의 대상이 된다

        //    //덮개 타일과 충돌하면 동작을 종료한다 
        //    int result = SingleO.gridManager.IsIncluded_InDiagonalArea(this.transform.position);
        //    if(GridManager._ReturnMessage_Included_InStructTile == result ||
        //       GridManager._ReturnMessage_Included_InDiagonalArea == result)
        //    {

        //        End();
        //    }

        //}

        //샷의 움직임을 종료한다 
        public void End()
        {
            if (null != (object)_owner)
            {
                ChampUnit champ = _owner as ChampUnit;
                if (null != (object)champ)
                {
                    champ._shot = null;
                }
            }
            _owner = null;
            _on_theWay = false;

            //spr 위치를 현재위치로 적용
            _transform.position = _sprParent.position;
            _sprParent.localPosition = ConstV.v3_zero;

            //캐릭터 아래에 깔리도록 설정
            base.Update_SortingOrder(-500);
            //_sortingGroup.sortingOrder = base.GetSortingOrder(0);


        }

        //____________________________________________
        //                  충돌반응
        //____________________________________________

        public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        {
            //무언가와 충돌했으면 투사체를 해제한다 (챔프 , 숥통 등)

            if ((object)dst != (object)_owner)
                End();
        }


        //____________________________________________
        //                  갱신 처리 
        //____________________________________________

        Vector3 _launchPos = ConstV.v3_zero; //시작위치
        Vector3 _targetPos = ConstV.v3_zero;
        Vector3 _maxHeight_pos = ConstV.v3_zero; //곡선의 최고점이 되는 위치 
        Vector3 _perpNormal = ConstV.v3_zero; //target - launch 벡터의 수직노멀 
        Vector3 _prev_pos = ConstV.v3_zero;
        float _shotMoveTime = 1f; //샷 이동 시간 

        float _elapsedTime = 0f;
        public float _maxHeight_length = 2f; //곡선의 최고점 높이 길이
        public float _maxHeight_posRate = 0.5f; //곡선의 최고점이 되는 위치 비율
        public void ThrowThings(Being owner, Vector3 launchPos, Vector3 targetPos)
        {

            if (null == (object)_ani._sprRender)
                return; //Start 함수 수행전에 호출된 경우 


            if (false == _on_theWay && null != owner)
            {
                base.Update_SortingOrder(1000); //지형위로 날라다니게 설정 

                _transform.position = launchPos;
                _on_theWay = true;
                float shake = (float)Misc.RandInRange(0f, 0.2f); //흔들림 정도
                _launchPos = launchPos;
                _targetPos = targetPos + Misc.GetDir8_Random_AxisY() * shake;
                Vector3 toTarget = VOp.Minus(_targetPos, _launchPos);

                //기준값 : 7미터 당 1초
                _shotMoveTime = toTarget.magnitude / (GridManager.MeterToWorld * 7f);
                //DebugWide.LogBlue((_targetPos - _launchPos).magnitude + "  " + _shotMoveTime);

                //_perpNormal = Vector3.Cross(_targetPos - _launchPos, Vector3.up).normalized;
                _perpNormal = Misc.GetDir8_Normal3D(Vector3.Cross(toTarget, ConstV.v3_up));
                _elapsedTime = 0f;
                _owner = owner;
                _prev_pos = _launchPos; //!
                _gameObject.SetActive(true);


                //초기 방향 설정 
                Vector3 angle = ConstV.v3_zero;
                //angle.y = Vector3.SignedAngle(ConstV.v3_forward, toTarget, ConstV.v3_up);
                angle.y = Geo.AngleSigned_AxisY(ConstV.v3_forward, toTarget);
                _transform.localEulerAngles = angle;


                float posRateVert = 0f;
                Vector3 posHori = ConstV.v3_zero;
                if (eDirection8.up == owner._move._eDir8)
                {
                    //그림자를 참 뒤에 보이게 함
                    posRateVert = 0.8f;
                }
                else if (eDirection8.down == owner._move._eDir8)
                {
                    //그림자를 창 앞에 보이게 함 
                    posRateVert = 0.3f;
                }
                else
                {
                    //* 캐릭터 방향이 위,아래가 아니면 포물선을 나타내기 위해 중앙노멀값을 적용한다
                    if (owner._move._direction.x < 0)
                    {
                        //왼쪽방향을 보고 있을 때, 중앙노멀값의 방향을 바꿔준다 
                        _perpNormal *= -1f;
                    }
                    posRateVert = _maxHeight_posRate + shake;
                    posHori = VOp.Multiply(_perpNormal, _maxHeight_length);
                }
                _maxHeight_pos = VOp.Multiply(toTarget, posRateVert);
                _maxHeight_pos = VOp.Plus(_maxHeight_pos, _launchPos);
                _maxHeight_pos = VOp.Plus(_maxHeight_pos, posHori);


                //TweenStart();

            }
        }

        //public Vector3 ClosestPoint_ToBezPos(Vector3 bezPos)
        //{
        //    Vector3 direction = _targetPos - _launchPos;
        //    Vector3 w = bezPos - _launchPos;
        //    float proj = Vector3.Dot(w, direction);
        //    // endpoint 0 is closest point
        //    if (proj <= 0.0f)
        //        return _launchPos;
        //    else
        //    {
        //        float vsq = Vector3.Dot(direction, direction);
        //        // endpoint 1 is closest point
        //        if (proj >= vsq)
        //            return _launchPos + direction;
        //        // else somewhere else in segment
        //        else
        //            return bezPos - (_launchPos + (proj / vsq) * direction);
        //    }
        //}

        Vector3 _ori_scale = Vector3.one;
        public void Update_Shot()
        {

            if (true == this._on_theWay)
            {
                //base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
                base.Update_SortingOrder(1000); //지형위로 날라다니게 한다 

                _elapsedTime += Time.deltaTime;

                //Rotate_Towards_FrontGap(this.transform);
                //float angle = Vector3.SignedAngle(Vector3.forward, __targetPos - __launchPos, Vector3.up);
                //Vector3 euler = __shot.transform.localEulerAngles;
                //euler.y = angle;
                //__shot.transform.localEulerAngles = euler;

                float zeroOneRate = _elapsedTime / _shotMoveTime;

                //* 화살 중간비율값 위치에 최대크기 비율값 계산
                //중간비율값 위치에 최고비율로 값변형 : [ 0 ~ 0.7 ~ 1 ] => [ 0 ~ 1 ~ 0 ]
                float scaleMaxRate = 0f;
                if (zeroOneRate < _maxHeight_posRate)
                {
                    scaleMaxRate = zeroOneRate / _maxHeight_posRate;
                }
                else
                {
                    scaleMaxRate = 1f - (zeroOneRate - _maxHeight_posRate) / (1f - _maxHeight_posRate);
                }
                _sprParent.localScale = _ori_scale + (ConstV.v3_one * scaleMaxRate * 0.5f); //최고점에서 1.5배
                _shader.transform.localScale = (_ori_scale * 0.7f) + (ConstV.v3_one * scaleMaxRate * 0.5f); //시작과 끝위치점에서 0.7배. 최고점에서 1.2배

                //* 이동 , 회전 
                this.transform.position = Vector3.Lerp(_launchPos, _targetPos, zeroOneRate); //그림자 위치 설정 
                _sprParent.position = Misc.BezierCurve(_launchPos, _maxHeight_pos, _targetPos, zeroOneRate);
                Rotate_Towards_FrontGap(_sprParent);


                if (_shotMoveTime < _elapsedTime)
                {
                    //base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
                    Update_CellSpace();//마지막 위치 등록 - 등록된 객체만 충돌처리대상이 된다 
                    this.End();
                }


                //================================================
                //Update_CellInfo(); //매프레임 마다 충돌검사의 대상이 된다

                //덮개 타일과 충돌하면 동작을 종료한다 
                int result = SingleO.gridManager.IsIncluded_InDiagonalArea(this.transform.position);
                if (GridManager._ReturnMessage_Included_InStructTile == result ||
                   GridManager._ReturnMessage_Included_InDiagonalArea == result)
                {

                    End();
                }
                //================================================
                //debug test
                //Debug.DrawLine(_maxHeight_pos, _maxHeight_pos - _perpNormal * _maxHeight_length);
                //Debug.DrawLine(_launchPos, _targetPos);
            }
        }


        public void Rotate_Towards_FrontGap(Transform tr)
        {
            Vector3 dir = VOp.Minus(tr.position, _prev_pos);

            //목표지점의 반대방향일 경우는 처리하지 않는다 
            if (0 > Vector3.Dot(VOp.Minus(_targetPos, _launchPos), dir))
                return;

            const float TILE_LENGTH_SQR = 0.16f * 0.16f;
            if (dir.sqrMagnitude <= TILE_LENGTH_SQR) //타일 하나의 길이만큼 거리가 있어야 함a
                return;


            Vector3 euler = tr.eulerAngles;
            float angle = (float)Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            //아래 3가지 방법도 같은 표현이다
            //float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            //tr.localRotation = Quaternion.LookRotation(dir, Vector3.up);
            //tr.localRotation = Quaternion.FromToRotation(Vector3.forward, dir);

            //euler.x = 90f; euler.z = -30f;//euler.z = -30f;
            euler.y = angle;
            tr.eulerAngles = euler;

            _prev_pos = tr.position;
        }



        public void TweenStart()
        {
            Vector3[] list = { _launchPos, _maxHeight_pos, _targetPos };
            iTween.MoveTo(_gameObject, iTween.Hash(
                "time", 0.8f
                , "easetype", "easeOutBack"
                , "path", list
                //,"orienttopath",true
                //,"axis","z"
                , "islocal", true //로컬위치값을 사용하겠다는 옵션. 대상객체의 로컬위치값이 (0,0,0)이 되는 문제 있음. 직접 대상객체 로컬위치값을 더해주어야 한다.
                , "movetopath", false //현재객체에서 첫번째 노드까지 자동으로 경로를 만들겠냐는 옵션. 경로 생성하는데 문제가 있음. 비활성으로 사용해야함
                                      //"looktarget",new Vector3(5,-5,7)
                , "onupdate", "Rotate_Towards_FrontGap"
                , "onupdatetarget", _gameObject
                , "onupdateparams", _transform
            ));

            //DebugWide.LogBlue("--------------TweenStart"); //chamto test
        }

    }

}

