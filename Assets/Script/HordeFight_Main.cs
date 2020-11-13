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
    
}


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
