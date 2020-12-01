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
