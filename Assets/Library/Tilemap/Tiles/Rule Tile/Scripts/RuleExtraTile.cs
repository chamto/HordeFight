using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEngine
{
    public class RuleTileExtra<T> : RuleExtraTile
	{
		public sealed override Type m_NeighborType { get { return typeof(T); } }
	}
	[Serializable]
    [CreateAssetMenu(fileName = "New RuleExtra Tile", menuName = "Tiles/RuleExtra Tile")]
    public class RuleExtraTile : TileBase
	{
        //public void OnEnable()
        //{
        //    DebugWide.LogBlue("RuleTile constrator");
        //}

#if UNITY_EDITOR
		private const string s_XIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";
		private const string s_Arrow0 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPzZExDoQwDATzE4oU4QXXcgUFj+YxtETwgpMwXuFcwMFSRMVKKwzZcWzhiMg91jtg34XIntkre5EaT7yjjhI9pOD5Mw5k2X/DdUwFr3cQ7Pu23E/BiwXyWSOxrNqx+ewnsayam5OLBtbOGPUM/r93YZL4/dhpR/amwByGFBz170gNChA6w5bQQMqramBTgJ+Z3A58WuWejPCaHQAAAABJRU5ErkJggg==";
		private const string s_Arrow1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPxYzBDYAgEATpxYcd+PVr0fZ2siZrjmMhFz6STIiDs8XMlpEyi5RkO/d66TcgJUB43JfNBqRkSEYDnYjhbKD5GIUkDqRDwoH3+NgTAw+bL/aoOP4DOgH+iwECEt+IlFmkzGHlAYKAWF9R8zUnAAAAAElFTkSuQmCC";
		private const string s_Arrow2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAC0SURBVDhPjVE5EsIwDMxPKFKYF9CagoJH8xhaMskLmEGsjOSRkBzYmU2s9a58TUQUmCH1BWEHweuKP+D8tphrWcAHuIGrjPnPNY8X2+DzEWE+FzrdrkNyg2YGNNfRGlyOaZDJOxBrDhgOowaYW8UW0Vau5ZkFmXbbDr+CzOHKmLinAXMEePyZ9dZkZR+s5QX2O8DY3zZ/sgYcdDqeEVp8516o0QQV1qeMwg6C91toYoLoo+kNt/tpKQEVvFQAAAAASUVORK5CYII=";
		private const string s_Arrow3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAB2SURBVDhPzY1LCoAwEEPnLi48gW5d6p31bH5SMhp0Cq0g+CCLxrzRPqMZ2pRqKG4IqzJc7JepTlbRZXYpWTg4RZE1XAso8VHFKNhQuTjKtZvHUNCEMogO4K3BhvMn9wP4EzoPZ3n0AGTW5fiBVzLAAYTP32C2Ay3agtu9V/9PAAAAAElFTkSuQmCC";
		private const string s_Arrow5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPnY3BCYBADASvFx924NevRdvbyoLBmNuDJQMDGjNxAFhK1DyUQ9fvobCdO+j7+sOKj/uSB+xYHZAxl7IR1wNTXJeVcaAVU+614uWfCT9mVUhknMlxDokd15BYsQrJFHeUQ0+MB5ErsPi/6hO1AAAAAElFTkSuQmCC";
		private const string s_Arrow6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACaSURBVDhPxZExEkAwEEVzE4UiTqClUDi0w2hlOIEZsV82xCZmQuPPfFn8t1mirLWf7S5flQOXjd64vCuEKWTKVt+6AayH3tIa7yLg6Qh2FcKFB72jBgJeziA1CMHzeaNHjkfwnAK86f3KUafU2ClHIJSzs/8HHLv09M3SaMCxS7ljw/IYJWzQABOQZ66x4h614ahTCL/WT7BSO51b5Z5hSx88AAAAAElFTkSuQmCC";
		private const string s_Arrow7 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABQSURBVDhPYxh8QNle/T8U/4MKEQdAmsz2eICx6W530gygr2aQBmSMphkZYxqErAEXxusKfAYQ7XyyNMIAsgEkaYQBkAFkaYQBsjXSGDAwAAD193z4luKPrAAAAABJRU5ErkJggg==";
		private const string s_Arrow8 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPxZE9DoAwCIW9iUOHegJXHRw8tIdx1egJTMSHAeMPaHSR5KVQ+KCkCRF91mdz4VDEWVzXTBgg5U1N5wahjHzXS3iFFVRxAygNVaZxJ6VHGIl2D6oUXP0ijlJuTp724FnID1Lq7uw2QM5+thoKth0N+GGyA7IA3+yM77Ag1e2zkey5gCdAg/h8csy+/89v7E+YkgUntOWeVt2SfAAAAABJRU5ErkJggg==";

		private static Texture2D[] s_Arrows;
		public static Texture2D[] arrows
		{
			get
			{
				if (s_Arrows == null)
				{
					s_Arrows = new Texture2D[10];
					s_Arrows[0] = Base64ToTexture(s_Arrow0);
					s_Arrows[1] = Base64ToTexture(s_Arrow1);
					s_Arrows[2] = Base64ToTexture(s_Arrow2);
					s_Arrows[3] = Base64ToTexture(s_Arrow3);
					s_Arrows[5] = Base64ToTexture(s_Arrow5);
					s_Arrows[6] = Base64ToTexture(s_Arrow6);
					s_Arrows[7] = Base64ToTexture(s_Arrow7);
					s_Arrows[8] = Base64ToTexture(s_Arrow8);
                    s_Arrows[9] = Base64ToTexture(s_XIconString);
				}
				return s_Arrows;
			}
		}

		public static Texture2D Base64ToTexture(string base64)
		{
			Texture2D t = new Texture2D(1, 1);
			t.hideFlags = HideFlags.HideAndDontSave;
			t.LoadImage(System.Convert.FromBase64String(base64));
			return t;
		}

		public virtual void RuleOnGUI(Rect rect, Vector2Int pos, int neighbor)
		{
			switch (neighbor)
			{
                case RuleExtraTile.TilingRule.Neighbor.DontCare:
					break;
                case RuleExtraTile.TilingRule.Neighbor.This:
					GUI.DrawTexture(rect, arrows[pos.y * 3 + pos.x]);
					break;
                case RuleExtraTile.TilingRule.Neighbor.NotThis:
					GUI.DrawTexture(rect, arrows[9]);
					break;
				default:
					var style = new GUIStyle();
					style.alignment = TextAnchor.MiddleCenter;
					style.fontSize = 10;
					GUI.Label(rect, neighbor.ToString(), style);
					break;
			}
			var allConsts = m_NeighborType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
			foreach (var c in allConsts)
			{
				if ((int)c.GetValue(null) == neighbor)
				{
					GUI.Label(rect, new GUIContent("", c.Name));
					break;
				}
			}
		}
#endif


        //룰타일 간의 이웃검사시 사용할 분류항목  
        [Serializable]
        public enum eClassification : Int32
        {
            None                = 0,        //0은 비트연산시 항상 참 , 사용하지 말것
            This                = 1 << 0,   //내주소값과 같은지 비교하는 방식 
            TileBase_Type       = 1 << 1,   //객체타입이 TileBase 인지 검사하는 방식 
            //RuleTile_Type       = 1 << 2,   //객체타입이 룰타일인지만 검사하는 방식
            RuleExtraTile_Type  = 1 << 2,

            Boundary            = 1 << 5,   //분류값 경계 : 이 아래 열거형은 분류값으로 처리된다 

            Dungeon             = Boundary,
            Dungeon_Structure   = 1 << 6,
            Dungeon_Floor       = 1 << 7,
            Dungeon_Floor_Ston  = 1 << 8,
            FogOfWar            = 1 << 9,

            Forest              = 1 << 20,

            Swamp               = 1 << 26,
        }
        public eClassification _class_id = eClassification.This;       //자신의 분류값
        public eClassification _permit_rules =  eClassification.This;   //비교할 대상의 분류값들 모임
        public int permit_rules_size
        {
            get
            {
                int count = 0;
                int LENGTH = 8 * sizeof(Int32);
                int mask = 0;
                for (int i = 1; i < LENGTH; i++)
                {
                    mask = 1 << i;
                    if(0 != ((int)_permit_rules & mask))
                        count++;
                       
                }
                return count;
            }
        }

        public eClassification GetPermitRule(int index)
        {
            if (index > permit_rules_size) return (eClassification)0;

            int count = 0;
            int LENGTH = 8 * sizeof(Int32);
            int mask = 0;
            for (int i = 0; i < LENGTH; i++)
            {
                mask = 1 << i;
                if (0 != ((int)_permit_rules & mask))
                {
                    if (count == index)
                        return (eClassification)mask;

                    count++;
                }
            }

            return (eClassification)0;
        }



		public virtual Type m_NeighborType { get { return typeof(TilingRule.Neighbor); } }

        //0 1 2  :  (-1, 1)  ( 0, 1)  ( 1, 1)
        //3 x 4  :  (-1, 0)  ( 0, 0)  ( 1, 0)
        //5 6 7  :  (-1,-1)  ( 0,-1)  ( 1,-1)
		private static readonly int[,] RotatedOrMirroredIndexes =
		{
			{2, 4, 7, 1, 6, 0, 3, 5}, // 90
			{7, 6, 5, 4, 3, 2, 1, 0}, // 180, XY
			{5, 3, 0, 6, 1, 7, 4, 2}, // 270
			{2, 1, 0, 4, 3, 7, 6, 5}, // X
			{5, 6, 7, 3, 4, 0, 1, 2}, // Y
		};
        private static readonly int NEIGHBOR_COUNT = 8;

		public Sprite m_DefaultSprite;
		public GameObject m_DefaultGameObject;
        public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;
		public TileBase m_Self
		{
			get { return m_OverrideSelf ? m_OverrideSelf : this; }
			set { m_OverrideSelf = value; }
		}

		private TileBase[] m_CachedNeighboringTiles = new TileBase[NEIGHBOR_COUNT];
		private TileBase m_OverrideSelf;
        private Quaternion m_GameObjectQuaternion;

		[Serializable]
		public class TilingRule
		{
            public string m_specifier; //임의의 지정값 , 사용자가 인스펙터상에서 직접 지정한다
			public int[] m_Neighbors;
            public int[] m_Neighbors_Length; //기본거리 1 , 이웃한 타일 검사시 중앙에서 n칸 거리까지 검사한다
            public string[] m_Neighbors_Specifier; //이웃한 객체의 지정값을 나타낸다
			public Sprite[] m_Sprites;
            public int m_MultiLength; //멀티모드에서 스프라이트를 몇개 단위로 읽어 들일지 설정
            public bool[] _multi_copy; //지정된 Tilemap에 복사할시 어떤 멀티타일을 복사할지 설정 
            public UtilGS9.eDirection8 _push_dir8; //충돌체크시 밀어내는 방향 
			public GameObject m_GameObject;
            public float m_AnimationSpeed;
			public float m_PerlinScale;
			public Transform m_RuleTransform;
			public OutputSprite m_Output;
            public bool _isTilemapCopy; //다른 타일맵에 복사대상인지 설정
			public Tile.ColliderType m_ColliderType;
			public Transform m_RandomTransform;

			public TilingRule()
			{
                m_specifier = "00";
				m_Output = OutputSprite.Single;
                _isTilemapCopy = false;
				m_Neighbors = new int[NEIGHBOR_COUNT];
                m_Neighbors_Length = new int[NEIGHBOR_COUNT];
                m_Neighbors_Specifier = new string[NEIGHBOR_COUNT];
				m_Sprites = new Sprite[1];
                m_MultiLength = 1;
                _multi_copy = new bool[m_MultiLength];
                _multi_copy[0] = true;
                _push_dir8 = UtilGS9.eDirection8.none;

                m_GameObject = null;
                m_AnimationSpeed = 1f;
				m_PerlinScale = 0.5f;
				m_ColliderType = Tile.ColliderType.Sprite;

				for (int i = 0; i < m_Neighbors.Length; i++)
                {
                    m_Neighbors[i] = Neighbor.DontCare;
                    m_Neighbors_Length[i] = 1; //기본거리 1
                    m_Neighbors_Specifier[i] = string.Empty; //기본값 공백
                }
					
			}

			public class Neighbor
			{
				public const int DontCare = 0;
				public const int This = 1;
				public const int NotThis = 2;
			}
			public enum Transform { Fixed, Rotated, MirrorX, MirrorY }
            public enum OutputSprite { Single, Multi, Random, Random_Multi, Animation }
		}

		[HideInInspector] public List<TilingRule> m_TilingRules;

        private static readonly byte DIVISION_MAX_NUM = byte.MaxValue;

        [Serializable]
        public class AppointData
        {
            public bool activeMultiTile = false;
            public Vector3Int root_pos = Vector3Int.zero;
            public Sprite sprite = null;
            public int max_size = 0; //예약한 전체 스프라이트배열 크기 
            public int multi_sequence = 0; //멀티스프라이트 순서값.

            public Matrix4x4 transform = Matrix4x4.identity;
            public TilingRule tilingRule = null;
            public UtilGS9.eDirection8 eTransDir = UtilGS9.eDirection8.none; //transform 으로 변환된 값을 저장한 방향 
            public bool isUpTile = false; //덮개 타일 (TileMap_StructUp)

            public byte divisionNum = 0; //분리번호가 다르면 다른타일로 인식한다. 초기화 대상값이 아니다

            public void Init()
            {
                activeMultiTile = false;
                root_pos = Vector3Int.zero;
                sprite = null;
                max_size = 0;
                multi_sequence = 0;
                transform = Matrix4x4.identity;
                tilingRule = null;
                eTransDir = UtilGS9.eDirection8.none;
                isUpTile = false;
            }

            public void ApplyData()
            {
                if (null == tilingRule) return;

                //if (UtilGS9.eDirection8.none != tilingRule._push_dir8) return;

                Vector3 n = UtilGS9.Misc.GetDir8_Normal3D_AxisMZ(tilingRule._push_dir8);
                Vector3 tn = transform * n;
                eTransDir = UtilGS9.Misc.GetDir8_AxisMZ(tn);

                //DebugWide.LogBlue(n + "   " + tn + "    " + eTransDir);
            }
        }

        /// <summary>
        /// Tile에 적용할 예약 정보를 관리한다 
        /// </summary>
        [Serializable]
        public class MultiDataMap : Dictionary<Vector3Int, AppointData>
        {
            
            private void AddOrUpdate(Vector3Int position, AppointData data)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    this.Add(position, data);
                }
                this[position] = data;

                if(null != data)
                    data.ApplyData();
            }


            public void SetMultiData(Vector3Int rootPos, ITilemap tilemap, TilingRule rule , Matrix4x4 transform , int multiIndex)
            {
                RuleExtraTile ruleTile = null;
                AppointData newData = null;
                Vector3Int newPos = rootPos;

                //** 루트 위치에 타일이 없으면 설정을 중단한다 
                ruleTile = tilemap.GetTile<RuleExtraTile>(rootPos);
                if (null == ruleTile)
                {
                    return;
                }


                int MULTI_LENGTH = rule.m_MultiLength;


                //멀티길이가 스프라이트 길이보다 작은 경우 : 현재 스프라이트 길이만큼만 멀티설정을 한다 
                if (MULTI_LENGTH > rule.m_Sprites.Length)
                    MULTI_LENGTH = rule.m_Sprites.Length;
                
                for (int i = 0; i < MULTI_LENGTH; i++)
                {

                    newPos.y = rootPos.y + 1 * i; //위로 자라는 방식

                    newData = new AppointData();
                    newData.root_pos = rootPos;
                    newData.sprite = rule.m_Sprites[i + multiIndex * MULTI_LENGTH];
                    newData.activeMultiTile = true;
                    newData.max_size = MULTI_LENGTH;
                    newData.multi_sequence = i;
                    newData.transform = transform;
                    newData.tilingRule = rule;

                    AddOrUpdate(newPos, newData);
                    //DebugWide.LogBlue(newPos + "  " + rootPos +  "  "  + newData.sprite.name); //chamto test
                }
            }

            public void Reset(Vector3Int childPos)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(childPos, out getData))
                {
                    //처리할 데이터 없음 
                    return;
                }

                getData.Init();
            }

            public void ResetAll(Vector3Int childPos, ITilemap tilemap)
            {

                AppointData getData = null;
                if (false == this.TryGetValue(childPos, out getData))
                {
                    //처리할 데이터 없음 
                    return;
                }
                //루트 위치를 찾는다
                if (false == this.TryGetValue(getData.root_pos, out getData))
                {
                    //처리할 데이터 없음 
                    return;
                }

                //RuleTileExtra ruleTile = null;
                Vector3Int newPos = getData.root_pos;
                Vector3Int rootPos = getData.root_pos;
                int max_size = getData.max_size;
                for (int i = 0; i < max_size; i++)
                {
                    newPos.y = rootPos.y + 1 * i;

                    if (false == this.TryGetValue(newPos, out getData))
                    {
                        //처리할 데이터 없음 
                        continue;
                    }

                    getData.Init();

                }

            }


            /// <summary>
            /// 활성중인 루트위치가 있을시, 요청된 자식위치를 복사할수 있는지 알려준다 
            /// </summary>
            public bool IsTileMapCopy_Child(Vector3Int childPos , ITilemap tilemap)
            {
                if (false == this.IsActive_Root(childPos, tilemap))
                    return false;

                int multiSeq = this[childPos].multi_sequence;
                Vector3Int rootPos = this[childPos].root_pos;

                if (this[rootPos].tilingRule._multi_copy.Length <= multiSeq)
                    return false;

                return this[rootPos].tilingRule._multi_copy[multiSeq];
            }


            /// <summary>
            /// 루트위치가 활성중인지 알려준다 
            /// </summary>
            public bool IsActive_Root(Vector3Int childPos , ITilemap tilemap)
            {
                AppointData rootData = null;
                AppointData childData = null;
                if (false == this.TryGetValue(childPos, out childData))
                {
                    return false;
                }
                //** 루트 위치를 찾는다
                if (false == this.TryGetValue(childData.root_pos, out rootData))
                {
                    return false;
                }
                //** 루트 위치의 타일이 비어있는 경우 
                RuleExtraTile ruleTile = tilemap.GetTile<RuleExtraTile>(rootData.root_pos);
                if (null == ruleTile)
                {
                    return false;
                }

                return rootData.activeMultiTile && rootData.max_size == childData.max_size && rootData.root_pos == childData.root_pos;
            }

            /// <summary>
            /// 자식위치(루트도 포함)가 활성중인지 알려준다 
            /// </summary>
            public bool IsActive_Child(Vector3Int childPos)
            {

                AppointData getData = null;
                if (false == this.TryGetValue(childPos, out getData))
                {
                    return false;
                }

                return getData.activeMultiTile;
            }

            /// <summary>
            /// 요청된 위치가 루트위치인지 알려준다 
            /// </summary>
            public bool IsRoot(Vector3Int unknownPos)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(unknownPos, out getData))
                {
                    return false;
                }

                return getData.root_pos == unknownPos;
            }
        }

        public MultiDataMap _multiDataMap = new MultiDataMap();

        //========================================================================
        //========================================================================

        [Serializable]
        public class TileDataMap : Dictionary<Vector3Int, AppointData>
        {
            public void AddOrUpdate(Vector3Int position, AppointData data)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    this.Add(position, data);
                }
                this[position] = data;

                if (null != data)
                    data.ApplyData();
            }

            public void AddOrUpdate(Vector3Int position, TilingRule rule, Matrix4x4 transform)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    getData = new AppointData();
                    getData.Init();
                    this.Add(position,getData);
                }

                //위치만 있고 알맹이가 없는 경우 
                if(null == getData)
                {
                    getData = new AppointData();
                    getData.Init();
                    this.Add(position, getData);
                }

                getData.transform = transform;
                getData.tilingRule = rule;
                getData.ApplyData();
            }

            public void Set_IsUpTile(Vector3Int position, bool isUpTime)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return;    
                }

                getData.isUpTile = isUpTime;

            }

            public bool Get_IsUpTile(Vector3Int position)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return false;
                }

                return getData.isUpTile;

            }

            public void Set_DivisionNum(Vector3Int position, byte num)
            {
                AppointData getData = null;
                if (true == this.TryGetValue(position, out getData))
                {
                    getData.divisionNum = num;
                }
            }

            public byte Get_DivisionNum(Vector3Int position)
            {
                AppointData getData = null;
                if (true == this.TryGetValue(position, out getData))
                {
                    return getData.divisionNum;
                }

                return DIVISION_MAX_NUM;
            }

            public AppointData GetData(Vector3Int position)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return null;
                }

                return getData;
            }

            public void InitPosition(Vector3Int position)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return;
                }

                getData.Init();
            }

            public string GetSpecifierName(Vector3Int position)
            {
                TilingRule getData = GetTilingRule(position);

                if (null == getData)
                    return string.Empty;

                return getData.m_specifier;
            }

            public TilingRule GetTilingRule(Vector3Int position)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return null;
                }

                return getData.tilingRule;
            }

            //멀티모드에서 자식위치에 대해서는 TilingRule 값이 없기 때문에 검사하지 못한다 
            //IsActive_Root() 함수를 사용하여 자식위치로 부모위치를 찾아 검사하여야 한다 
            public bool IsTileMapCopy(Vector3Int position)
            {
                TilingRule getData = GetTilingRule(position);

                if (null == getData)
                    return false;

                return getData._isTilemapCopy;
            }

            public UtilGS9.eDirection8 GetDirection8(Vector3Int position)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return UtilGS9.eDirection8.none;
                }

                if (null == getData || null == getData.tilingRule)
                    return UtilGS9.eDirection8.none;

                //getData.ApplyData(); //적용 

                return getData.eTransDir;
            }

        }

        public TileDataMap _tileDataMap = new TileDataMap();

        public Tilemap _tilemap_this = null; //현재 타일맵
        public Tilemap _tilemap_copy = null; //설정된 타일 정보를 복사할 타일맵
        //========================================================================
        //========================================================================

        public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject instantiateedGameObject)
        {
            //타일맵 정보를 얻는다 
            if(null == _tilemap_this)
            {
                _tilemap_this = tilemap.GetComponent<Tilemap>();    
            }


            //???? 왜 이런처리를 하는 걸까 
            if (instantiateedGameObject != null)
            {
                instantiateedGameObject.transform.position = location + new Vector3(0.5f,0.5f,0);
                instantiateedGameObject.transform.rotation = m_GameObjectQuaternion;
            }

            //return base.StartUp(location, tilemap, instantiateedGameObject);
            return true;

        }


        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
		{
			//TileBase[] neighboringTiles = null;
			//GetMatchingNeighboringTiles(tilemap, position, ref neighboringTiles);
			var iden = Matrix4x4.identity;

			tileData.sprite = m_DefaultSprite;
			tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
			tileData.flags = TileFlags.LockTransform;
			tileData.transform = iden;

            //** 현재 위치에 멀티타일 루트설정이 있으면 해제한다
            if(_multiDataMap.IsRoot(position))
            {
                _multiDataMap.ResetAll(position, tilemap);
            }

            //** 기록 초기화 
            _tileDataMap.InitPosition(position);

			foreach (TilingRule rule in m_TilingRules)
			{
				Matrix4x4 transform = iden;
				//if (RuleMatches(rule, ref neighboringTiles, ref transform))
                if (RuleMatchesFull_ToNeighborLength(tilemap,position,rule,ref transform))
				{
					switch (rule.m_Output)
					{
						case TilingRule.OutputSprite.Single:
                            {
                                tileData.sprite = rule.m_Sprites[0];
                            }
                            break;
                        case TilingRule.OutputSprite.Random_Multi:
                        case TilingRule.OutputSprite.Multi:
                            {
                                //현재 위치를 포함하는 활성화된 루트설정이 있는 경우 
                                if (true == _multiDataMap.IsActive_Root(position, tilemap))
                                {
                                    //설정을 안한다 
                                    break;
                                }


                                //** 랜덤멀티 설정을 한다 
                                int multiIndex = 0; //기본 멀티모드에서는 0으로 동
                                int MAX_MULTI_LENGTH = rule.m_Sprites.Length / rule.m_MultiLength;
                                if(TilingRule.OutputSprite.Random_Multi == rule.m_Output)
                                {
                                    multiIndex = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * MAX_MULTI_LENGTH), 0, MAX_MULTI_LENGTH - 1);
                                    //tileData.sprite = rule.m_Sprites[index]; //todo - 
                                    if (rule.m_RandomTransform != TilingRule.Transform.Fixed)
                                        transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                                }

                                //** 루트 멀티타일 설정을 한다 
                                _multiDataMap.SetMultiData(position, tilemap, rule, transform , multiIndex);
                                //DebugWide.LogBlue(MAX_MULTI_LENGTH + "  " + multiIndex); //chamto test
                            }
                            break;
						case TilingRule.OutputSprite.Animation:
							tileData.sprite = rule.m_Sprites[0];
							break;
						case TilingRule.OutputSprite.Random:
                            {
                                int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                                tileData.sprite = rule.m_Sprites[index];
                                if (rule.m_RandomTransform != TilingRule.Transform.Fixed)
                                    transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);    
                            }
                            break;
                        
					}
                    tileData.transform = transform;
					tileData.gameObject = rule.m_GameObject;
                    tileData.colliderType = rule.m_ColliderType;

                    // Converts the tile's rotation matrix to a quaternion to be used by the instantiated Game Object
                    m_GameObjectQuaternion = Quaternion.LookRotation(new Vector3(transform.m02, transform.m12, transform.m22), new Vector3(transform.m01, transform.m11, transform.m21));

                    //** 어떤 규칙이 어느 위치로 들어갔는지 기록 
                    _tileDataMap.AddOrUpdate(position, rule, transform); 

                    break;
				}

			}//end for



            //==================================================================================
            //DebugWide.LogBlue(_appointDataMap.IsActive_MultiTile_Child(position) + "   " + position);

            //멀티설정이 있는 경우, 설정된 값으로 덮는다 
            if (_multiDataMap.IsActive_Root(position, tilemap))
            {
                //멀티설정 적용 
                tileData.sprite = _multiDataMap[position].sprite;
                tileData.transform = _multiDataMap[position].transform;

                //** 어떤 규칙이 어느 위치로 들어갔는지 기록 
                //멀티설정에 적용되는 타일까지 기록을 하면 ID비교 검사시 겹치는 규칙이 많아지게 된다. 
                //!! 멀티설정 루트 타일 외에는 기록하지 않는다 
                //_rulePositionMap.AddOrUpdate(position, _appointDataMap[position].tilingRule);

            }else
            {
                if(true == _multiDataMap.IsActive_Child(position))
                {
                    //루트설정이 해제되어 있는데, 자식설정이 활성상태인 경우 
                    //자식설정값을 제거해 준다
                    _multiDataMap.Reset(position);
                }
            }

            //==================================================================================


            //멀티모드일 경우 루트데이터를 찾아내 복사옵션이 있는지 검사한다
            //자식위치의 룰데이터는 다른 룰데이터에 스프라이트만 덮은 것이기때문에 쓰면 안된다 
            if (true == _multiDataMap.IsActive_Root(position, tilemap))
            {
                //루트로 변환하여 검사 
                if (_tileDataMap.IsTileMapCopy(_multiDataMap[position].root_pos))
                {

                    //자식위치가 복사가능한지 검사
                    if(true == _multiDataMap.IsTileMapCopy_Child(position,tilemap))
                    {
                        _tileDataMap.Set_IsUpTile(position, true);
                        CopyTile_AnotherTilemap(position, tilemap, tileData);    
                    }
                       
                }
            }
            //싱글모드일 경우 복사옵션이 있는지 검사한다
            else if (_tileDataMap.IsTileMapCopy(position))
            {
                _tileDataMap.Set_IsUpTile(position, true);
                CopyTile_AnotherTilemap(position, tilemap, tileData);    
            }


            //Vector3Int testPos = new Vector3Int(16, 0, 0);
            //RuleTileExtra test = _tilemap_this.GetTile(testPos) as RuleTileExtra;
            //if(null != test)
            //{
            //    DebugWide.LogBlue(test._tileDataMap.GetDirection8(testPos) + "  cur:" + position + "   " + _class_id);

            //}
        }


        public void CopyTile_AnotherTilemap(Vector3Int pos, ITilemap tilemap, TileData data)
        {
            
            if (null == _tilemap_copy) return;
            if (_tilemap_this == _tilemap_copy) return;

            //Tile __tile = new Tile();
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = data.sprite;
            tile.transform = data.transform;


            _tilemap_copy.SetTile(pos, tile);
        }


		private static float GetPerlinValue(Vector3Int position, float scale, float offset)
		{
			return Mathf.PerlinNoise((position.x + offset) * scale, (position.y + offset) * scale);
		}

		public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
		{
			TileBase[] neighboringTiles = null;
			var iden = Matrix4x4.identity;
			foreach (TilingRule rule in m_TilingRules)
			{
				if (rule.m_Output == TilingRule.OutputSprite.Animation)
				{
					Matrix4x4 transform = iden;
					GetMatchingNeighboringTiles(tilemap, position, ref neighboringTiles);
					if (RuleMatches(rule, ref neighboringTiles, ref transform))
					{
						tileAnimationData.animatedSprites = rule.m_Sprites;
						tileAnimationData.animationSpeed = rule.m_AnimationSpeed;
						return true;
					}
				}
			}
			return false;
		}

		public override void RefreshTile(Vector3Int location, ITilemap tileMap)
		{
			if (m_TilingRules != null && m_TilingRules.Count > 0)
			{
				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						base.RefreshTile(location + new Vector3Int(x, y, 0), tileMap);
					}
				}

			}
			else
			{
				base.RefreshTile(location, tileMap);
			}
		}

		public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, ref Matrix4x4 transform)
		{
			// Check rule against rotations of 0, 90, 180, 270
			for (int angle = 0; angle <= (rule.m_RuleTransform == TilingRule.Transform.Rotated ? 270 : 0); angle += 90)
			{
				if (RuleMatches(rule, ref neighboringTiles, angle))
				{
					transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
					return true;
				}
			}

			// Check rule against x-axis mirror
			if ((rule.m_RuleTransform == TilingRule.Transform.MirrorX) && RuleMatches(rule, ref neighboringTiles, true, false))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
				return true;
			}

			// Check rule against y-axis mirror
			if ((rule.m_RuleTransform == TilingRule.Transform.MirrorY) && RuleMatches(rule, ref neighboringTiles, false, true))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
				return true;
			}

			return false;
		}

		private static Matrix4x4 ApplyRandomTransform(TilingRule.Transform type, Matrix4x4 original, float perlinScale, Vector3Int position)
		{
			float perlin = GetPerlinValue(position, perlinScale, 200000f);
			switch (type)
			{
				case TilingRule.Transform.MirrorX:
					return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(perlin < 0.5 ? 1f : -1f, 1f, 1f));
				case TilingRule.Transform.MirrorY:
					return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, perlin < 0.5 ? 1f : -1f, 1f));
				case TilingRule.Transform.Rotated:
					int angle = Mathf.Clamp(Mathf.FloorToInt(perlin * 4), 0, 3) * 90;
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
			}
			return original;
		}

		public virtual bool RuleMatch(int neighbor, TileBase tile)
		{
            //****** 있냐 없냐 를 판단하는 검사 ********

            RuleExtraTile neighborTile = tile as RuleExtraTile;
            bool result = false;


            int test = (int)eClassification.This;

            //!!! 검사순서 중요 : 범위 작은것 부터 큰것까지 순으로 검사한다 
           
            if (null != neighborTile)
            {
                //** 분류항목값들을 검사한다. 분류항목값은 경계값 보다 커야 한다
                if(eClassification.Boundary <= neighborTile._class_id)
                {
                    test = (int)this._permit_rules & (int)neighborTile._class_id;
                    result = test == (int)neighborTile._class_id;    
                }

            }

            test = (int)this._permit_rules & (int)eClassification.RuleExtraTile_Type;
            if(false == result)
            {
                if (test == (int)eClassification.RuleExtraTile_Type)
                {
                    //** RuleTileExtra 객체가 있는지만 검사한다 
                    result = (null != neighborTile);
                }    
            }


            test = (int)this._permit_rules & (int)eClassification.TileBase_Type;
            if (false == result)
            {
                if (test == (int)eClassification.TileBase_Type)
                {
                    //** TileBase 객체가 있는지만 검사한다 
                    result = (null != tile);
                }
            }



            test = (int)this._permit_rules & (int)eClassification.This;
            if (false == result)
            {
                if (test == (int)eClassification.This)
                {
                    //** 주소값이 같은지 검사한다 
                    result = (tile == m_Self);
                }
            }



            //============================================================
            //판단값 변환 
            switch (neighbor)
            {
              case TilingRule.Neighbor.This:
                    return result;
              case TilingRule.Neighbor.NotThis:
                    return !result;
            }

			return true;
		}

		public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, int angle)
		{
			for (int i = 0; i < NEIGHBOR_COUNT; ++i)
			{
				int index = GetRotatedIndex(i, angle);
				TileBase tile = neighboringTiles[index];
				if (!RuleMatch(rule.m_Neighbors[i], tile))
				{
					return false;
				}
			}
			return true;
		}

		public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, bool mirrorX, bool mirrorY)
		{
			for (int i = 0; i < NEIGHBOR_COUNT; ++i)
			{
				int index = GetMirroredIndex(i, mirrorX, mirrorY);
				TileBase tile = neighboringTiles[index];
				if (!RuleMatch(rule.m_Neighbors[i], tile))
				{
					return false;
				}
			}
			return true;
		}


        //============================================================================================================================
        //============================================================================================================================

        public bool RuleMatchesFull_ToNeighborLength(ITilemap tilemap, Vector3Int position, TilingRule rule, ref Matrix4x4 transform)
        {
            Neighbors8_info neighbors8_Info = GetNeighbors8_Info(tilemap, position, rule);

            // Check rule against rotations of 0, 90, 180, 270
            for (int angle = 0; angle <= (rule.m_RuleTransform == TilingRule.Transform.Rotated ? 270 : 0); angle += 90)
            {
                if (RuleMatches_ToNeighborLength(tilemap, position, rule, angle, false, false, neighbors8_Info))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                    return true;
                }
            }

            // Check rule against x-axis mirror
            if ((rule.m_RuleTransform == TilingRule.Transform.MirrorX) && RuleMatches_ToNeighborLength(tilemap, position, rule, 0, true, false, neighbors8_Info))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
                return true;
            }

            // Check rule against y-axis mirror
            if ((rule.m_RuleTransform == TilingRule.Transform.MirrorY) && RuleMatches_ToNeighborLength(tilemap, position, rule, 0, false, true, neighbors8_Info))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
                return true;
            }

            return false;
        }


        public struct Neighbors8_info
        {
            //0 1 2  :  (-1, 1)  ( 0, 1)  ( 1, 1)
            //3 x 4  :  (-1, 0)  ( 0, 0)  ( 1, 0)
            //5 6 7  :  (-1,-1)  ( 0,-1)  ( 1,-1)

            public TileBase[] _tiles;
            public string[] _specifierNames;
            public byte[] _divisionNums;

            public Neighbors8_info(TileBase[] tiles , string[] names , byte[] divideNums)
            {
                _tiles = tiles;
                _specifierNames = names;
                _divisionNums = divideNums;
            }
            
        }

        //todo !!! 최적화 필요 : 주변타일정보를 힙메모리에 저장하지 말고, 바로 조회하게 변경해야 한다 
        //요청 타일의 주변 8개 타일에 대하여 길이설정값에 따른 지정이름을 가져온다
        public Neighbors8_info GetNeighbors8_Info(ITilemap tilemap, Vector3Int position, TilingRule rule)
        {
            //0 1 2  :  (-1, 1)  ( 0, 1)  ( 1, 1)
            //3 x 4  :  (-1, 0)  ( 0, 0)  ( 1, 0)
            //5 6 7  :  (-1,-1)  ( 0,-1)  ( 1,-1)
            int seq = 0;
            TileBase[] neighbors_tiles = new TileBase[NEIGHBOR_COUNT];
            string[] neighbors_specifierNames = new string[NEIGHBOR_COUNT];
            byte[] neighbors_divideNums = new byte[NEIGHBOR_COUNT];
            for (int y = 1; y >= -1; y--)
            {
                for (int x = -1; x <= 1; x++)
                {
                    //(0,0)좌표가 아닐 때만 처리한다 
                    if (x != 0 || y != 0)
                    {
                        //길이값 정보에 따른 주변 타일 가져오기
                        int add_len = rule.m_Neighbors_Length[seq];
                        Vector3Int getPos = new Vector3Int(position.x + x, position.y + y, position.z);
                        Vector3Int getPos_length = new Vector3Int(position.x + x * add_len, position.y + y * add_len, position.z);

                        //주변 타일 가져오기 - (길이값 정보 적용)
                        neighbors_tiles[seq] = tilemap.GetTile(getPos_length);

                        //주변 아이디 가져오기 - (길이값 정보 적용)
                        neighbors_specifierNames[seq] = _tileDataMap.GetSpecifierName(getPos_length);

                        //주변 분리번호 가져오기 - (길이값 정보 적용)
                        neighbors_divideNums[seq] = _tileDataMap.Get_DivisionNum(getPos_length);

                        seq++;
                    }
                }
            }

            return new Neighbors8_info(neighbors_tiles, neighbors_specifierNames, neighbors_divideNums);

        }

        public bool RuleMatches_ToNeighborLength(ITilemap tilemap, Vector3Int position, TilingRule rule, 
                                                 int angle, bool mirrorX, bool mirrorY , Neighbors8_info neighbors8)
		{

            //각도모드와 미러모드로 각각 동작한다
            //둘 동시에 작동못함 
            if(0 < angle )
            {
                //각도값이 있으면 미러모드는 해제한다 
                mirrorX = false; mirrorY = false;
            }

            //0 1 2  :  (-1, 1)  ( 0, 1)  ( 1, 1)
            //3 x 4  :  (-1, 0)  ( 0, 0)  ( 1, 0)
            //5 6 7  :  (-1,-1)  ( 0,-1)  ( 1,-1)
            //주변타일 8개에 대한 규칙이 맞는지 검사한다
            TileBase trsTile = null;
            int trsIndex = -1;
            byte divisionNum = _tileDataMap.Get_DivisionNum(position);
            for (int oriIndex = 0; oriIndex < NEIGHBOR_COUNT; ++oriIndex)
            {
                if (0 < angle)
                {
                    trsIndex = GetRotatedIndex(oriIndex, angle);    
                }else
                {
                    trsIndex = GetMirroredIndex(oriIndex, mirrorX, mirrorY);    
                }

                trsTile = neighbors8._tiles[trsIndex];

                //==================================================

                //* 같은 규칙타일 인스턴스의 구분값(DivideNum)이 다른지 검사한다
                //최대분리값 일때는 타일이 아직 추가되지 않은것이기 때문에 처리하지 않는다 (이미 있는 타일에 대해서만 처리한다)
                //RuleTileExtra 이 아니면 neighbors8._divideNums[n] 값은 최대분리값이 들어가게 된다 
                if (DIVISION_MAX_NUM != divisionNum && DIVISION_MAX_NUM != neighbors8._divisionNums[trsIndex])
                {
                    switch (rule.m_Neighbors[oriIndex])
                    {
                        case TilingRule.Neighbor.This:
                            {
                                if (divisionNum != neighbors8._divisionNums[trsIndex])
                                    return false;
                            }
                            break;
                        case TilingRule.Neighbor.NotThis:
                            {
                                //없는 조건에서의 타일주소값 처리때문에 이렇게 처리하면 안된다
                                //if (divisionNum == neighbors8._divideNums[trsIndex])
                                //return false;

                                //없는 조건이 맞았을때 타일주소값을 null로 만든다
                                //타일주소가 있으면 다음판단에서 있는 조건이 된다
                                if (divisionNum != neighbors8._divisionNums[trsIndex])
                                    trsTile = null;
                            }
                            break;
                    }
                }




                //==================================================
                if (string.Empty == rule.m_Neighbors_Specifier[oriIndex])
                {
                    //** 대분류 값에 따른 검사를 한다
                    if (!RuleMatch(rule.m_Neighbors[oriIndex], trsTile))
                    {
                        return false;
                    }
                }
                else
                {
                    //** 공백 아이디가 아닌 경우 아이디 비교 검사를 한다
                    switch (rule.m_Neighbors[oriIndex])
                    {
                        
                        case TilingRule.Neighbor.DontCare:
                        case TilingRule.Neighbor.This:
                            {
                                //대분류 값에 따른 검사를 한다
                                if (!RuleMatch(rule.m_Neighbors[oriIndex], trsTile))
                                {
                                    return false;
                                }

                                ////** 주변설정ID값과 다른지 검사 
                                if (rule.m_Neighbors_Specifier[oriIndex] != neighbors8._specifierNames[trsIndex])
                                {
                                    return false;
                                }
                            }
                            break;
                        case TilingRule.Neighbor.NotThis:
                            {
                                //대분류 검사를 하지 않는다
                                // - 아이디검사의 전제조건은 비교객체가 존재하는가이다
                                //   대분류 검사에서 비교객체가 존재하면 거짓이 되므로, 아래 처리로 들어오지 못한다  

                                ////** 주변설정ID값과 같은지 검사 
                                if (rule.m_Neighbors_Specifier[oriIndex] != neighbors8._specifierNames[trsIndex])
                                {
                                    return false;
                                }
                            }
                            break;
                    }//end switch
                }//end if


            }//end for

            return true;
		}

        //============================================================================================================================
        //============================================================================================================================


		private void GetMatchingNeighboringTiles(ITilemap tilemap, Vector3Int position, ref TileBase[] neighboringTiles)
		{
			if (neighboringTiles != null)
				return;

			if (m_CachedNeighboringTiles == null || m_CachedNeighboringTiles.Length < NEIGHBOR_COUNT)
				m_CachedNeighboringTiles = new TileBase[NEIGHBOR_COUNT];

            //0 1 2  :  (-1, 1)  ( 0, 1)  ( 1, 1)
            //3 x 4  :  (-1, 0)  ( 0, 0)  ( 1, 0)
            //5 6 7  :  (-1,-1)  ( 0,-1)  ( 1,-1)
			int index = 0;
			for (int y = 1; y >= -1; y--)
			{
				for (int x = -1; x <= 1; x++)
				{
					if (x != 0 || y != 0)
					{
                        Vector3Int tilePosition = new Vector3Int(position.x + x, position.y + y, position.z);
						m_CachedNeighboringTiles[index] = tilemap.GetTile(tilePosition);

                        index++;
					}
				}
			}
			neighboringTiles = m_CachedNeighboringTiles;
		}

		private int GetRotatedIndex(int original, int rotation)
		{
			switch (rotation)
			{
				case 0:
					return original;
				case 90:
					return RotatedOrMirroredIndexes[0, original];
				case 180:
					return RotatedOrMirroredIndexes[1, original];
				case 270:
					return RotatedOrMirroredIndexes[2, original];
			}
			return original;
		}

		private int GetMirroredIndex(int original, bool mirrorX, bool mirrorY)
		{
			if (mirrorX && mirrorY)
			{
				return RotatedOrMirroredIndexes[1, original];
			}
			if (mirrorX)
			{
				return RotatedOrMirroredIndexes[3, original];
			}
			if (mirrorY)
			{
				return RotatedOrMirroredIndexes[4, original];
			}
			return original;
		}

		private int GetIndexOfOffset(Vector3Int offset)
		{
			int result = offset.x + 1 + (-offset.y + 1) * 3;
			if (result >= 4)
				result--;
			return result;
		}

		public Vector3Int GetRotatedPos(Vector3Int original, int rotation)
		{
			switch (rotation)
			{
				case 0:
					return original;
				case 90:
					return new Vector3Int(-original.y, original.x, original.z);
				case 180:
					return new Vector3Int(-original.x, -original.y, original.z);
				case 270:
					return new Vector3Int(original.y, -original.x, original.z);
			}
			return original;
		}

		public Vector3Int GetMirroredPos(Vector3Int original, bool mirrorX, bool mirrorY)
		{
			return new Vector3Int(original.x * (mirrorX ? -1 : 1), original.y * (mirrorY ? -1 : 1), original.z);
		}
    }
}

