using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

using UtilGS9;

namespace HordeFight
{
    public class HordeFight_Main : MonoBehaviour
    {
        
        // Use this for initialization
        void Start()
        {
            Misc.Init();

            SingleO.Init(gameObject); //싱글톤 객체 생성 , 초기화 


            SingleO.debugViewer._origin = SingleO.hierarchy.GetTransformA("z_debug/origin");
            SingleO.debugViewer._target = SingleO.hierarchy.GetTransformA("z_debug/target");
            //===================

            //SingleO.objectManager.Create_Characters(); //여러 캐릭터들 테스트용
            //SingleO.objectManager.Create_ChampCamp();

        }


        // Update is called once per frame
        //void Update()
        //{
        //}


        //void OnGUI()
        //{
        //    //if (GUI.Button(new Rect(10, 10, 200, 100), new GUIContent("Refresh Timemap Fog of War")))
        //    //{
        //    //    //RuleExtraTile ruleTile =  SingleO.gridManager.GetTileMap_Struct().GetTile<RuleExtraTile>(new Vector3Int(0, 0, 0));
        //    //    SingleO.gridManager.GetTileMap_FogOfWar().RefreshAllTiles();
        //    //    //DebugWide.LogBlue("TileMap_Struct RefreshAllTiles");
        //    //}
        //}


    }

}



//========================================================
//==================      라인 관리기      ==================
//========================================================
namespace HordeFight
{
    public class LineControl : MonoBehaviour
    {

        private int _sequenceId = 0;
        private Dictionary<int, Info> _list = new Dictionary<int, Info>();


        public enum eKind
        {
            None,
            Line,   //hp 표현
            Circle, //캐릭터 선택 표현
            Square, //캐릭터 선택 표현
            Polygon,//여러 캐릭터 선택 표현
            Graph,  //경로 표현 
        }

        public struct Info
        {
            public LineRenderer render;
            public GameObject gameObject;
            public Transform transform;
            public eKind kind;
            public int id;

            public Vector3 hpPos_0;

            public void Init()
            {
                render = null;
                gameObject = null;
                transform = null;
                kind = eKind.None;
                id = -1;

                hpPos_0 = ConstV.v3_zero;
            }

            public void SetScale(float scale)
            {
                //Vector3 s = _list[id].render.transform.localScale;
                transform.localScale = Vector3.one * scale;
            }

            public void SetLineHP(float rate)
            {
                if (false == gameObject.activeSelf) return; //비활성시에는 처리하지 않는다 
                if (eKind.Line != kind) return;
                
                if (0 > rate) rate = 0;
                if (1f < rate) rate = 1f;

                Vector3 pos = hpPos_0;
                pos.x += HP_BAR_LENGTH * rate;
                render.SetPosition(1, pos);

            }

            //public void Update_Circle()
            //{
            //    if (null == renderer) return;

            //    float deg = 360f / renderer.positionCount;
            //    float radius = renderer.transform.parent.GetComponent<CircleCollider2D>().radius;
            //    Vector3 pos = ConstV.v3_right;
            //    for (int i = 0; i < renderer.positionCount; i++)
            //    {
            //        pos.x = Mathf.Cos(deg * i * Mathf.Deg2Rad) * radius;
            //        pos.y = Mathf.Sin(deg * i * Mathf.Deg2Rad) * radius;
            //        renderer.SetPosition(i, pos + renderer.transform.parent.position);
            //        //DebugWide.LogBlue(Mathf.Cos(deg * i * Mathf.Deg2Rad) + " _ " + deg*i);
            //    }
            //}
        }

		private void Start()
		{
			
		}

		

        const float HP_BAR_LENGTH = 0.8f;
        public Info Create_LineHP_AxisY(Transform dst)
        {
            GameObject obj = new GameObject();
            LineRenderer render = obj.AddComponent<LineRenderer>();
            Info info = new Info();
            info.Init();

            _sequenceId++;

            info.id = _sequenceId;
            info.render = render;
            info.gameObject = render.gameObject;
            info.transform = render.transform;
            info.kind = eKind.Line;

            render.name = info.kind.ToString() + "_" + _sequenceId.ToString("000");
            render.material = new Material(Shader.Find("Sprites/Default"));
            render.useWorldSpace = false; //로컬좌표로 설정하면 부모객체 이동시 영향을 받는다. (변경정보에 따른 재갱싱 필요없음)
            render.transform.parent = dst;
            //render.sortingOrder = -10; //나중에 그려지게 한다.
            render.sortingLayerName = "Effect";
            render.positionCount = 2;
            render.transform.localPosition = ConstV.v3_zero;


            render.startWidth = 0.12f;
            render.endWidth = 0.12f;
            render.startColor = Color.red;
            render.endColor = Color.red;

            _list.Add(_sequenceId, info); //추가

            Vector3 pos = ConstV.v3_zero;
            pos.x = -0.5f; pos.z = -0.8f;
            render.SetPosition(0, pos);
            info.hpPos_0 = pos; //초기위치 저장해 놓음
            pos.x += HP_BAR_LENGTH;
            render.SetPosition(1, pos);

            //return _sequenceId;
            return info;
        }

        public Info Create_Circle_AxisY(Transform parent, float radius, Color color)
        {
            GameObject obj = new GameObject();
            LineRenderer render = obj.AddComponent<LineRenderer>();
            Info info = new Info();
            info.Init();

            _sequenceId++;

            info.id = _sequenceId;
            info.render = render;
            info.gameObject = render.gameObject;
            info.transform = render.transform;
            info.kind = eKind.Circle;

            render.name = info.kind.ToString() + "_" + _sequenceId.ToString("000");
            render.material = new Material(Shader.Find("Sprites/Default"));
            render.useWorldSpace = false; //로컬좌표로 설정하면 부모객체 이동시 영향을 받는다. (변경정보에 따른 재갱싱 필요없음)
            render.transform.parent = parent;//부모객체 지정
            //render.sortingOrder = -10; //먼저그려지게 한다.
            render.sortingLayerName = "Effect";
            render.positionCount = 20;
            render.loop = true; //처음과 끝을 연결한다 .
            render.transform.localPosition = ConstV.v3_zero;

            color.a = 0.4f; //흐리게 한다
            render.startWidth = 0.1f;
            render.endWidth = 0.1f;
            render.startColor = color;//Color.green;
            render.endColor = color;//Color.green;

            _list.Add(_sequenceId, info); //추가

            //info.Update_Circle(); //값설정
            float deg = 360f / render.positionCount;
            //float radius = render.transform.parent.GetComponent<SphereCollider>().radius;
            Vector3 pos = ConstV.v3_right;
            for (int i = 0; i < render.positionCount; i++)
            {
                pos.x = Mathf.Cos(deg * i * Mathf.Deg2Rad) * radius;
                pos.z = Mathf.Sin(deg * i * Mathf.Deg2Rad) * radius;

                render.SetPosition(i, pos );
                //DebugWide.LogBlue(Mathf.Cos(deg * i * Mathf.Deg2Rad) + " _ " + deg*i);
            }

            //return _sequenceId;
            return info;

        }

        //public void Create_Square(Transform dst)
        //{ }

        //public void Create_Polygon(Transform dst)
        //{ }

        public bool IsActive(int id)
        {
            return _list[id].gameObject.activeSelf;
        }

        public void SetActive(int id, bool onOff)
        {
            //todo : 예외처리 추가하기 
            _list[id].gameObject.SetActive(onOff);
        }

        public void SetScale(int id, float scale)
        {
            //Vector3 s = _list[id].render.transform.localScale;
            _list[id].render.transform.localScale = Vector3.one * scale; 
        }

        //public void SetCircle_Radius(int id, float radius)
        //{
            
        //}

        //rate : 0~1
        public void SetLineHP(int id, float rate)
        {
            if (0 > rate) rate = 0;
            if (1f < rate) rate = 1f;

            LineRenderer render = _list[id].render;

            if (false == render.gameObject.activeSelf) return; //비활성시에는 처리하지 않는다 

            Vector3 pos = render.GetPosition(0);
            pos.x += HP_BAR_LENGTH * rate;
            render.SetPosition(1, pos);
            //pos.x = -0.05f; pos.z = -0.15f;
            //render.SetPosition(0, pos);
            //pos.x += (0.1f * rate) ;
            //render.SetPosition(1, pos);

        }

	}
}

namespace HordeFight
{
    //작업하다 말았음 - 나중에 작업 다시 시작하기 
    public class EffectAni
    {
        public enum eEffectKind
        {
            Aim,        //조준점
            Dir,        //방향
            Emotion,    //감정표현
            Hand_Left,  //행동 왼손
            Hand_Right, //행동 오른손

            Max,
        }

        public Being _owner = null;
        public Being _target = null;
        public bool _on_theWay = false; //발사되어 날아가는 중

        //전용effect
        private Transform[] _effect = new Transform[(int)eEffectKind.Max];


        public void Init()
        {

            // 전용 effect 설정 
            _effect[(int)eEffectKind.Aim] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/aim");
            _effect[(int)eEffectKind.Dir] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/dir");
            _effect[(int)eEffectKind.Emotion] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/emotion");
            _effect[(int)eEffectKind.Hand_Left] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/hand_left");
            _effect[(int)eEffectKind.Hand_Right] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/hand_right");

        }


        //public void End()
        //{
        //    if (null != (object)_owner)
        //    {
        //        ChampUnit champ = _owner as ChampUnit;
        //        if (null != (object)champ)
        //        {
        //            champ._shot = null;
        //        }
        //    }
        //    _owner = null;
        //    _on_theWay = false;

        //    //spr 위치를 현재위치로 적용
        //    _transform.position = _sprParent.position;
        //    _sprParent.localPosition = ConstV.v3_zero;

        //    //캐릭터 아래에 깔리도록 설정
        //    base.Update_SortingOrder(-500);
        //    //_sortingGroup.sortingOrder = base.GetSortingOrder(0);

        //}

        //____________________________________________
        //                  충돌반응
        //____________________________________________

        //public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        //{
        //    //무언가와 충돌했으면 투사체를 해제한다 (챔프 , 숥통 등)

        //    if ((object)dst != (object)_owner)
        //        End();
        //}


        //____________________________________________
        //                  갱신 처리 
        //____________________________________________

        //Vector3 _launchPos = ConstV.v3_zero; //시작위치
        //Vector3 _targetPos = ConstV.v3_zero;
        //Vector3 _maxHeight_pos = ConstV.v3_zero; //곡선의 최고점이 되는 위치 
        //Vector3 _perpNormal = ConstV.v3_zero; //target - launch 벡터의 수직노멀 
        //Vector3 _prev_pos = ConstV.v3_zero;
        //float _shotMoveTime = 1f; //샷 이동 시간 

        //float _elapsedTime = 0f;
        //public float _maxHeight_length = 2f; //곡선의 최고점 높이 길이
        //public float _maxHeight_posRate = 0.5f; //곡선의 최고점이 되는 위치 비율
        //public void ThrowThings(Being owner, Vector3 launchPos, Vector3 targetPos)
        //{

        //    if (null == (object)_sprRender)
        //        return; //Start 함수 수행전에 호출된 경우 


        //    if (false == _on_theWay && null != owner)
        //    {
        //        base.Update_SortingOrder(1000); //지형위로 날라다니게 설정 

        //        _transform.position = launchPos;
        //        _on_theWay = true;
        //        float shake = (float)Misc.RandInRange(0f, 0.2f); //흔들림 정도
        //        _launchPos = launchPos;
        //        _targetPos = targetPos + Misc.GetDir8_Random_AxisY() * shake;
        //        Vector3 toTarget = VOp.Minus(_targetPos, _launchPos);

        //        //기준값 : 7미터 당 1초
        //        _shotMoveTime = toTarget.magnitude / (GridManager.MeterToWorld * 7f);
        //        //DebugWide.LogBlue((_targetPos - _launchPos).magnitude + "  " + _shotMoveTime);

        //        //_perpNormal = Vector3.Cross(_targetPos - _launchPos, Vector3.up).normalized;
        //        _perpNormal = Misc.GetDir8_Normal3D(Vector3.Cross(toTarget, ConstV.v3_up));
        //        _elapsedTime = 0f;
        //        _owner = owner;
        //        _prev_pos = _launchPos; //!
        //        _gameObject.SetActive(true);


        //        //초기 방향 설정 
        //        Vector3 angle = ConstV.v3_zero;
        //        //angle.y = Vector3.SignedAngle(ConstV.v3_forward, toTarget, ConstV.v3_up);
        //        angle.y = Geo.AngleSigned_AxisY(ConstV.v3_forward, toTarget);
        //        _transform.localEulerAngles = angle;


        //        float posRateVert = 0f;
        //        Vector3 posHori = ConstV.v3_zero;
        //        if (eDirection8.up == owner._move._eDir8)
        //        {
        //            //그림자를 참 뒤에 보이게 함
        //            posRateVert = 0.8f;
        //        }
        //        else if (eDirection8.down == owner._move._eDir8)
        //        {
        //            //그림자를 창 앞에 보이게 함 
        //            posRateVert = 0.3f;
        //        }
        //        else
        //        {
        //            //* 캐릭터 방향이 위,아래가 아니면 포물선을 나타내기 위해 중앙노멀값을 적용한다
        //            if (owner._move._direction.x < 0)
        //            {
        //                //왼쪽방향을 보고 있을 때, 중앙노멀값의 방향을 바꿔준다 
        //                _perpNormal *= -1f;
        //            }
        //            posRateVert = _maxHeight_posRate + shake;
        //            posHori = VOp.Multiply(_perpNormal, _maxHeight_length);
        //        }
        //        _maxHeight_pos = VOp.Multiply(toTarget, posRateVert);
        //        _maxHeight_pos = VOp.Plus(_maxHeight_pos, _launchPos);
        //        _maxHeight_pos = VOp.Plus(_maxHeight_pos, posHori);


        //        //TweenStart();

        //    }
        //}

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

        //Vector3 _ori_scale = Vector3.one;
        //public void Update_Shot()
        //{

        //    if (true == this._on_theWay)
        //    {
        //        base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
        //        base.Update_SortingOrder(1000); //지형위로 날라다니게 한다 

        //        _elapsedTime += Time.deltaTime;

        //        //Rotate_Towards_FrontGap(this.transform);
        //        //float angle = Vector3.SignedAngle(Vector3.forward, __targetPos - __launchPos, Vector3.up);
        //        //Vector3 euler = __shot.transform.localEulerAngles;
        //        //euler.y = angle;
        //        //__shot.transform.localEulerAngles = euler;

        //        float zeroOneRate = _elapsedTime / _shotMoveTime;

        //        //* 화살 중간비율값 위치에 최대크기 비율값 계산
        //        //중간비율값 위치에 최고비율로 값변형 : [ 0 ~ 0.7 ~ 1 ] => [ 0 ~ 1 ~ 0 ]
        //        float scaleMaxRate = 0f;
        //        if (zeroOneRate < _maxHeight_posRate)
        //        {
        //            scaleMaxRate = zeroOneRate / _maxHeight_posRate;
        //        }
        //        else
        //        {
        //            scaleMaxRate = 1f - (zeroOneRate - _maxHeight_posRate) / (1f - _maxHeight_posRate);
        //        }
        //        _sprParent.localScale = _ori_scale + (ConstV.v3_one * scaleMaxRate * 0.5f); //최고점에서 1.5배
        //        _shader.transform.localScale = (_ori_scale * 0.7f) + (ConstV.v3_one * scaleMaxRate * 0.5f); //시작과 끝위치점에서 0.7배. 최고점에서 1.2배

        //        //* 이동 , 회전 
        //        this.transform.position = Vector3.Lerp(_launchPos, _targetPos, zeroOneRate); //그림자 위치 설정 
        //        _sprParent.position = Misc.BezierCurve(_launchPos, _maxHeight_pos, _targetPos, zeroOneRate);
        //        Rotate_Towards_FrontGap(_sprParent);


        //        if (_shotMoveTime < _elapsedTime)
        //        {
        //            //base.Update_PositionAndBounds(); //가장 먼저 실행되어야 한다. transform 의 위치로 갱신 
        //            Update_CellSpace();//마지막 위치 등록 - 등록된 객체만 충돌처리대상이 된다 
        //            this.End();
        //        }


        //        //================================================
        //        //Update_CellInfo(); //매프레임 마다 충돌검사의 대상이 된다

        //        //덮개 타일과 충돌하면 동작을 종료한다 
        //        int result = SingleO.gridManager.IsIncluded_InDiagonalArea(this.transform.position);
        //        if (GridManager._ReturnMessage_Included_InStructTile == result ||
        //           GridManager._ReturnMessage_Included_InDiagonalArea == result)
        //        {

        //            End();
        //        }
        //        //================================================
        //        //debug test
        //        //Debug.DrawLine(_maxHeight_pos, _maxHeight_pos - _perpNormal * _maxHeight_length);
        //        //Debug.DrawLine(_launchPos, _targetPos);
        //    }
        //}
    }
}


//*
namespace HordeFight
{

    //자세 => 준비동작 => 공격 (3단계)

    //========================================================
    //==================     동작  정보     ==================
    //========================================================
    public partial class Behavior
    {
        //제거대상 
        //public enum eKind
        //{
        //    None = 0,

        //    Idle = 10,
        //    Idle_Random = 11,
        //    Idle_LookAt = 12,
        //    Idle_Max = 19,

        //    Move = 20,
        //    Move_Max = 29,

        //    Block = 30,
        //    Block_Max = 39,

        //    Attack = 40,
        //    Attack_Max = 49,

        //    FallDown = 50,
        //    FallDown_Max = 59,

        //}

        //운동 모양
        public enum eMovementShape
        {
            None,
            Straight,   //직선

            Rotation,   //회전 
            Area,       //영역
        }


        //===================================

        public float runningTime;   //동작 전체 시간 


        //아래는 동작의 전체 시간안에 있는 시간이다 

        //상대가 막을 수 있는 시간
        public float cloggedTime_0; //막히는 시간 : 0(시작) , 1(끝)  
        public float cloggedTime_1;

        //동작 이벤트가 성공하는 시간
        public float eventTime_0;   //동작 유효 범위 : 0(시작) , 1(끝)  
        public float eventTime_1;

        //콤보 연결을 위한 입력가능 시간
        public float openTime_0;    //다음 동작 연결시간 : 0(시작) , 1(끝)  
        public float openTime_1;

        //하나의 동작이 완료후 경직되는 시간. 마지막 모션상태로 경직 상태동안 있는다
        public float rigidTime;     //동작 완료후 경직 시간

        //===================================

        //운동 모양 
        public eMovementShape movementShape;

        //공격행동의 중심점 
        public Vector3 point_0; //행동 중심점 (상대위치임) : 0(시작) , 1(끝)
        public Vector3 point_1;


        public Vector3 object_startDir;     //무기 시작 방향
        public Vector3 behaDir;             //행동의 방향
        public float angle;             //범위 각도

        public float plus_range_0;      //더해지는 범위 최소 
        public float plus_range_1;      //더해지는 범위 최대


        //=== 직선 공격 모델 === Straight
        //무기를 휘둘러 무기가 최대공격점까지 이동한후 제자리로 돌아오는 것을 수치모델로 만든것임. 요요라 보면됨
        public float distance_travel;   //공격점까지 이동 거리 : 상대방까지의 직선거리 , 근사치 , 판단을 위한 값임 , 정확한 충돌검사용 값이 아님.
        public float distance_maxTime;  //최대거리가 되는 시간 , 공격점에 도달하는 시간
        public float velocity_before;   //공격점 전 속도 
        public float velocity_after;    //공격점 후 속도  


        public Behavior()
        {
            runningTime = 0f;
            eventTime_0 = 0f;
            eventTime_1 = 0f;
            rigidTime = 0f;
            openTime_0 = 0f;
            openTime_1 = 0f;
            cloggedTime_0 = 0f;
            cloggedTime_1 = 0f;

            movementShape = eMovementShape.None;
            plus_range_0 = 0f;
            plus_range_1 = 0f;
            angle = 45f;
            distance_travel = 0f;
            distance_maxTime = 0f;
            velocity_before = 0f;
            velocity_after = 0f;
        }

        public Behavior Clone()
        {
            return this.MemberwiseClone() as Behavior;
        }



    }

    public partial class Behavior
    {

        public float GetEventTime_Interval()
        {
            return this.eventTime_1 - this.eventTime_0;
        }

        public float GetOpenTime_Interval()
        {
            return this.openTime_1 - this.openTime_0;
        }

        public bool Valid_EventTime(Being.ePhase phase, float timeDelta)
        {

            if (Being.ePhase.Start == phase || Being.ePhase.Running == phase)
            {
                if (this.eventTime_0 <= timeDelta && timeDelta <= this.eventTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_CloggedTime(Being.ePhase phase, float timeDelta)
        {
            if (Being.ePhase.Start == phase || Being.ePhase.Running == phase)
            {
                if (this.cloggedTime_0 <= timeDelta && timeDelta <= this.cloggedTime_1)
                    return true;
            }

            return false;
        }

        public bool Valid_OpenTime(Being.ePhase phase, float timeDelta)
        {
            if (Being.ePhase.Running == phase)
            {
                if (this.openTime_0 <= timeDelta && timeDelta <= this.openTime_1)
                    return true;
            }

            return false;
        }


        public void Calc_Velocity()
        {
            //t * s = d
            //s = d/t
            if (0f == distance_maxTime)
                this.velocity_before = 0f;
            else
                this.velocity_before = distance_travel / distance_maxTime;

            this.velocity_after = distance_travel / (runningTime - distance_maxTime);

            //DebugWide.LogBlue("velocity_before : " + this.velocity_before + "   <-- 충돌점 -->   velocity_after : " + this.velocity_after + "  [distance_travel:" + distance_travel + "]"); //chamto test
        }

        public float CurrentDistance(float currentTime)
        {
            //* 러닝타임 보다 더 큰 값이 들어오면 사용오류임
            if (runningTime < currentTime)
                return 0f;

            //* 최대거리에 도달하는 시간이 0이면 최대거리를 반환한다.
            if (0f == distance_maxTime)
            {
                return distance_travel;
            }

            //1. 전진
            if (currentTime <= distance_maxTime)
            {
                return this.velocity_before * currentTime;
            }

            //2. 후진
            //if(distance_maxTime < currentTime)
            return this.velocity_after * (runningTime - currentTime);
        }
    }

}
//*/
//========================================================
//==================     스킬  정보     ==================
//========================================================

/*
namespace HordeFight
{
    public class Skill : List<Behavior>
    {

        public enum eKind
        {
            None,
            Move,
            Attack_Strong,
            Attack_Weak,
            Attack_Counter,
            Withstand,
            Block,
            Hit,
            Max
        }

        public enum eName
        {
            None,
            Idle,

            Move_0,

            Hit_Body,
            Hit_Weapon,

            Attack_Strong_1,

            Block_1,

            Max
        }


        //========================================

        private int _index_current = 0;


        //========================================

        public eKind _kind;
        public eName _name;

        //========================================

        public Behavior FirstBehavior()
        {
            _index_current = 0; //index 초기화

            if (this.Count == 0)
                return null;

            return this[_index_current];
        }

        public Behavior NextBehavior()
        {
            if (this.Count > _index_current)
            {
                //마지막 인덱스임
                if (this.Count == _index_current + 1)
                    return null;

                _index_current++;
                return this[_index_current];
            }

            return null;
        }

        //다음 행동이 있나 질의한다
        public bool IsNextBehavior()
        {
            if (this.Count > _index_current)
            {
                //마지막 인덱스임
                if (this.Count == _index_current + 1)
                    return false;


                return true;
            }

            return false;
        }



        //========================================

        //스킬 명세서
        static public Skill Details_Idle()
        {
            Skill skinfo = new Skill();

            skinfo._kind = eKind.None;
            skinfo._name = eName.Idle;

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1f;

            bhvo.eventTime_0 = 0f;
            bhvo.eventTime_1 = 0f;
            bhvo.openTime_0 = 0;
            bhvo.openTime_1 = 10f;
            skinfo.Add(bhvo);

            return skinfo;
        }

        static public Skill Details_Move()
        {
            Skill skinfo = new Skill();

            skinfo._kind = eKind.Move;
            skinfo._name = eName.Move_0;

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 1f;

            bhvo.eventTime_0 = 0f;
            bhvo.eventTime_1 = 0f;
            bhvo.openTime_0 = 0;
            bhvo.openTime_1 = 10f;
            skinfo.Add(bhvo);

            return skinfo;
        }


        static public Skill Details_Attack_Strong()
        {
            Skill skinfo = new Skill();

            skinfo._kind = eKind.Attack_Strong;
            skinfo._name = eName.Attack_Strong_1;

            Behavior bhvo = new Behavior();
            bhvo.runningTime = 2.0f;
            //1
            bhvo.cloggedTime_0 = 0.1f;
            bhvo.cloggedTime_1 = 1.0f;
            //2
            bhvo.eventTime_0 = 1.0f;
            bhvo.eventTime_1 = 1.2f;
            //3
            bhvo.openTime_0 = 1.5f;
            bhvo.openTime_1 = 1.8f;
            //4
            bhvo.rigidTime = 0.5f;

            bhvo.movementShape = Behavior.eMovementShape.Straight;
            //bhvo.attack_shape = eTraceShape.Vertical;
            bhvo.angle = 45f;
            bhvo.plus_range_0 = 2f;
            bhvo.plus_range_1 = 2f;
            bhvo.distance_travel = 1f;
            //bhvo.distance_maxTime = bhvo.eventTime_0; //유효범위 시작시간에 최대 거리가 되게 한다. : 떙겨치기 , [시간증가에 따라 유효거리 감소]
            bhvo.distance_maxTime = bhvo.eventTime_1; //유효범위 끝시간에 최대 거리가 되게 한다. : 일반치기 , [시간증가에 따라 유효거리 증가]

            bhvo.Calc_Velocity();
            skinfo.Add(bhvo);

            return skinfo;
        }



    }

    /// <summary>
    /// Skill book.
    /// </summary>
    public class SkillBook //: Dictionary<Skill.eName, Skill>
    {
        private delegate Skill Details_Skill();
        private Dictionary<Skill.eName, Skill> _referDict = new Dictionary<Skill.eName, Skill>();   //미리 만들어진 정보로 빠르게 사용
        private Dictionary<Skill.eName, Details_Skill> _createDict = new Dictionary<Skill.eName, Details_Skill>(); //새로운 스킬인스턴스를 만들때 사용 

        public SkillBook()
        {
            this.Add(Skill.eName.Idle, Skill.Details_Idle);

            this.Add(Skill.eName.Move_0, Skill.Details_Move);

            this.Add(Skill.eName.Attack_Strong_1, Skill.Details_Attack_Strong);

        }

        private void Add(Skill.eName name, Details_Skill skillPtr)
        {
            _referDict.Add(name, skillPtr());
            _createDict.Add(name, skillPtr);
        }

        //만들어진 객체를 참조한다 
        public Skill Refer(Skill.eName name)
        {
            return _referDict[name];
        }

        //요청객체를 생성한다
        public Skill Create(Skill.eName name)
        {
            return _createDict[name]();
        }
    }
}

namespace HordeFight
{
    public class BodyControl
    {
        public enum eState
        {
            None = 0,

            Start,
            Running,
            Waiting,
            End,

            Max,
        }

        public enum eSubState
        {
            None,

            Start,
            Running,
            End,

            Max
        }


        public enum ePoint
        {
            Start,  //시작
            End,    //끝지

            Cur,    //현재

            Max,
        }

        public class Part
        {
            //신체부위
            public enum eKind
            {
                None = 0,

                //==== 인간형태 ==== 
                Head = 0,
                Hand_Left,  //손
                Hand_Right,
                Foot_Left,  //발
                Foot_Right,

                //Other_1,    //또다른 손발등
                //Other_2,

                Max,

            }


            public Vector3 _pos_standard;    //신체 부위의 기준점 
            public float _range_max;         //부위 기준점으로 부터 최대 범위
            public Vector3[] _pos;           //부위 위치 (로컬값)
            public Vector3[] _dir;           //부위 방향 (로컬값)

            public Vector3 _target;          //목표점 (월드값)

            public void Init()
            {
                _pos_standard = UtilGS9.ConstV.v3Int_zero;
                _range_max = 1f;
                _pos = new Vector3[(int)ePoint.Max];
                _dir = new Vector3[(int)ePoint.Max];

                _target = new Vector3(0, 0, 2f); //z축을 보게 한다 
            }

        }

        public class Weapon
        {
            public float length; //무기 길이 
            //public 
        }


        //====================================
        public Part[] _parts = null;    //부위 정보 


        //동작정보
        public Behavior _behavior = null;
        public Skill _skill_current = null;
        private Skill _skill_next = null;
        public float _timeDelta = 0f;  //시간변화량

        //상태정보
        public eState _state_current = eState.None;
        private eSubState _eventState_current = eSubState.None;     //유효상태

        //판정
        //private Judgment _judgment = new Judgment();



        //====================================

        public BodyControl()
        {
            _parts = new Part[(int)Part.eKind.Max];
            for (int i = 0; i < (int)Part.eKind.Max; i++)
            {
                _parts[i] = new Part();
                _parts[i].Init();
            }

            Setting_Head_2Hand_2Foot();
        }

        public void Setting_2Hand()
        {
            //x-z축 공간에 캐릭터가 놓여 있고, z축을 바라보고 있다 가정 < Forward 방향 >
            Part HL = _parts[(int)Part.eKind.Hand_Left];
            Part HR = _parts[(int)Part.eKind.Hand_Right];

            HL._pos_standard = new Vector3(-0.5f, 0.5f, 0);
            HR._pos_standard = new Vector3(0.5f, 0.5f, 0);
            HR._range_max = 1.1f; //오른쪽 사정거리를 약간 더 늘린다 
        }

        public void Setting_Head_2Hand_2Foot()
        {
            //x-z축 공간에 캐릭터가 놓여 있고, z축을 바라보고 있다 가정 < Forward 방향 >
            Setting_2Hand();
            _parts[(int)Part.eKind.Head]._pos_standard = new Vector3(0, 1f, 0);
            _parts[(int)Part.eKind.Foot_Left]._pos_standard = new Vector3(-0.5f, 0, 0);
            _parts[(int)Part.eKind.Foot_Right]._pos_standard = new Vector3(0.5f, 0, 0);
        }



        public float CurrentDistance()
        {
            return _behavior.CurrentDistance(_timeDelta);
        }

        //public float GetWeaponDistance()
        //{
        //    return _behavior.CurrentDistance(_timeDelta);
        //}

        //public Vector3 GetWeaponPosition(float time)
        //{
        //    //DebugWide.LogBlue (_behavior.CurrentDistance (time) * _direction); //chamto test
        //    return _position + (_behavior.CurrentDistance(time) * _direction);
        //}

        //public Vector3 GetWeaponPosition()
        //{
        //    return this.GetWeaponPosition(_timeDelta);

        //}

        public void SetState(eState setState)
        {
            _state_current = setState;
        }

        public void SetEventState(eSubState setSubState)
        {
            _eventState_current = setSubState;
        }


        public SkillBook ref_skillBook { get { return CSingleton<SkillBook>.Instance; } }

        public void SetSkill_Start(Skill.eName name)
        {
            this.SetSkill_Start(ref_skillBook.Refer(name));
        }

        public void SetSkill_Start(Skill skill)
        {
            _skill_current = skill;
            _behavior = _skill_current.FirstBehavior();
            SetState(eState.Start);
            this._timeDelta = 0f;
        }

        public void SetSkill_AfterInterruption(Skill.eName name)
        {
            this.SetSkill_AfterInterruption(ref_skillBook.Refer(name));
        }

        //현재 스킬 중단후 다음스킬 시작 (현재 스킬을 end 상태로 바로 전환한다)  
        public void SetSkill_AfterInterruption(Skill skill)
        {
            //현재 스킬이 지정되어 있지 않으면 바로 요청 스킬로 지정한다
            //현재 상태가 end라면 스킬을 바로 지정한다
            if (null == _skill_current || eState.End == this._state_current)
            {
                this.SetSkill_Start(skill);
                return;
            }

            _skill_next = skill;

            //SetState(eState.End); //계속 함수를 호출하면 End 상태를 못 벗어나 주석처리함 
        }

        public void Attack_Strong_1()
        {
            SetSkill_AfterInterruption(Skill.eName.Attack_Strong_1);
        }

        public void Move_0()
        {
            SetSkill_AfterInterruption(Skill.eName.Move_0);
        }

        public void Idle()
        {
            SetSkill_AfterInterruption(Skill.eName.Idle);
        }

        public void Update()
        {
            if (null == _behavior) return;

            this._timeDelta += Time.deltaTime;


            switch (this._state_current)
            {
                case eState.None:
                    {
                        //===== 처리철차 ===== 
                        //입력 -> ui갱신 -> (갱신 -> 판정)
                        //공격키입력 -> 행동상태none 에서 start 로 변경 -> start 상태 검출
                        //* 공격키입력으로 시작되는 상태는 None 이 되어야 한다. (바로 Start 상태가 되면 판정에서 Start상태인지 모른다)
                        //* 상태변이에 의해 시작되는 상태는 Start 여야 한다. (None 으로 시작되면 한프레임을 더 수행하는게 되므로 Start로 시작하게 한다)
                        this._timeDelta = 0f;
                        SetState(eState.Start);
                    }
                    break;
                case eState.Start:
                    {
                        this._timeDelta = 0f;
                        SetState(eState.Running);
                        SetEventState(eSubState.None);

                        //DebugWide.LogRed ("[0: "+this._state_current);//chamto test
                    }
                    break;
                case eState.Running:
                    {

                        //====================================================
                        // update sub_state 
                        //====================================================



                        switch (_eventState_current)
                        {
                            case eSubState.None:
                                if (_behavior.eventTime_0 <= _timeDelta && _timeDelta <= _behavior.eventTime_1)
                                {
                                    this.SetEventState(eSubState.Start);
                                }
                                break;
                            case eSubState.Start:
                                this.SetEventState(eSubState.Running);
                                break;
                            case eSubState.Running:
                                if (!(_behavior.eventTime_0 <= _timeDelta && _timeDelta < _behavior.eventTime_1))
                                {
                                    this.SetEventState(eSubState.End);
                                }

                                break;
                            case eSubState.End:
                                this.SetEventState(eSubState.None);
                                break;

                        }


                        if (_behavior.runningTime <= this._timeDelta)
                        {
                            //동작완료
                            this.SetState(eState.Waiting);
                        }
                    }
                    break;
                case eState.Waiting:
                    {
                        //DebugWide.LogBlue (_behavior.rigidTime + "   " + (this._timeDelta - _behavior.allTime));
                        if (_behavior.rigidTime <= (this._timeDelta - _behavior.runningTime))
                        {
                            this.SetState(eState.End);
                        }

                    }
                    break;
                case eState.End:
                    {
                        //* 다음 스킬입력 처리  
                        if (null != _skill_next)
                        {
                            //DebugWide.LogBlue ("next : " + _skill_next.name);
                            SetSkill_Start(_skill_next);
                            _skill_next = null;
                        }
                        else
                        {
                            //** 콤보 스킬 처리
                            _behavior = _skill_current.NextBehavior();
                            if (null == _behavior)
                            //if(false == _skill_current.IsNextBehavior())
                            {
                                //스킬 동작을 모두 꺼냈으면 아이들상태로 들어간다
                                Idle();
                            }
                            else
                            {
                                //_behavior = _skill_current.NextBehavior ().Clone();

                                //다음 스킬 동작으로 넘어간다
                                SetState(eState.Start);
                                _timeDelta = 0f;

                                DebugWide.LogBlue("next combo !!");
                            }
                        }

                    }
                    break;


            }

            //============================================================================

            //this.GetJudgment().Update(); //판정 후처리 갱신

            //if(1 == this.GetID())
            //  DebugWide.LogBlue ("[3:_receiveState_current : " + _receiveState_current);
            //============================================================================

        }//end func

    }
}
*/

//===========================


namespace HordeFight
{
    //========================================================
    //==================   아이템 정보   ==================
    //========================================================

    public class Inventory : Dictionary<uint, Item>
    {
    }

    public class Item
    {
        public enum eCategory_1 : uint
        {
            None,
            Weapone,
            Armor,
            Potion,
            Max,
        }

        public enum eCategory_2 : uint
        {
            None,

            Weapone_Start,
            Sword,  //칼 
            Spear,  //창
            Weapone_End,

            Armor_Start,
            Armor_End,

            Potion_Start,
            Potion_End,

            Max,
        }

        public class BaseInfo { }

        public class WeaponeInfo : BaseInfo
        {
            public ushort _power = 1;
            public float _range_min = 0f;  //최소 사거리
            public float _range_max = 1f;  //최대 사거리 
        }

        public uint _id;
        public eCategory_1 _eCat01 = eCategory_1.None;
        public eCategory_2 _eCat02 = eCategory_2.None;
        public BaseInfo _info = null;
        public ushort _position = 0; //아이템 위치(놓인 위치,장착 위치)


    }

}
