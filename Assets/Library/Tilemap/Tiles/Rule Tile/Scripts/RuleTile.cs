using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEngine
{
	public class RuleTile<T> : RuleTile
	{
		public sealed override Type m_NeighborType { get { return typeof(T); } }
	}
	[Serializable]
	[CreateAssetMenu(fileName = "New Rule Tile", menuName = "Tiles/Rule Tile")]
	public class RuleTile : TileBase
	{
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
				case RuleTile.TilingRule.Neighbor.DontCare:
					break;
				case RuleTile.TilingRule.Neighbor.This:
					GUI.DrawTexture(rect, arrows[pos.y * 3 + pos.x]);
					break;
				case RuleTile.TilingRule.Neighbor.NotThis:
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
		private static readonly int NeighborCount = 8;

		public Sprite m_DefaultSprite;
		public GameObject m_DefaultGameObject;
        public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;
		public TileBase m_Self
		{
			get { return m_OverrideSelf ? m_OverrideSelf : this; }
			set { m_OverrideSelf = value; }
		}

		private TileBase[] m_CachedNeighboringTiles = new TileBase[NeighborCount];
		private TileBase m_OverrideSelf;
        private Quaternion m_GameObjectQuaternion;

		[Serializable]
		public class TilingRule
		{
			public int[] m_Neighbors;
            public int[] m_Neighbors_Length; //기본거리 1 , 이웃한 타일 검사시 중앙에서 n칸 거리까지 검사한다
			public Sprite[] m_Sprites;
			public GameObject m_GameObject;
            public float m_AnimationSpeed;
			public float m_PerlinScale;
			public Transform m_RuleTransform;
			public OutputSprite m_Output;
			public Tile.ColliderType m_ColliderType;
			public Transform m_RandomTransform;

			public TilingRule()
			{
				m_Output = OutputSprite.Single;
				m_Neighbors = new int[NeighborCount];
                m_Neighbors_Length = new int[NeighborCount];
				m_Sprites = new Sprite[1];
                m_GameObject = null;
                m_AnimationSpeed = 1f;
				m_PerlinScale = 0.5f;
				m_ColliderType = Tile.ColliderType.Sprite;

				for (int i = 0; i < m_Neighbors.Length; i++)
                {
                    m_Neighbors[i] = Neighbor.DontCare;
                    m_Neighbors_Length[i] = 1; //기본거리 1
                }
					
			}

			public class Neighbor
			{
				public const int DontCare = 0;
				public const int This = 1;
				public const int NotThis = 2;
			}
			public enum Transform { Fixed, Rotated, MirrorX, MirrorY }
			public enum OutputSprite { Single, Multi, Random, Animation }
		}

		[HideInInspector] public List<TilingRule> m_TilingRules;


        public class AppointData
        {
            public bool activeMultiTile = false;
            public Vector3Int root_pos = Vector3Int.zero;
            public Sprite sprite = null;
            public int max_size = 0; //예약한 전체 스프라이트배열 크기 
            public Matrix4x4 transform = Matrix4x4.identity;

            public void Init()
            {
                activeMultiTile = false;
                root_pos = Vector3Int.zero;
                sprite = null;
                max_size = 0;
                transform = Matrix4x4.identity;
            }
        }

        /// <summary>
        /// Tile에 적용할 예약 정보를 관리한다 
        /// </summary>
        public class AppointDataMap : Dictionary<Vector3Int, AppointData>
        {

            public Tilemap _DST_TileMap = null;
            public RuleTile _ruleTileObject = null;

            private void AddOrUpdate(Vector3Int position, AppointData data)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    this.Add(position, data);
                }
                this[position] = data;
            }


            public void SetMultiData(Vector3Int rootPos, ITilemap tilemap, Sprite[] multi_sprites , Matrix4x4 transform)
            {
                RuleTile ruleTile = null;
                AppointData newData = null;
                Vector3Int newPos = rootPos;

                //** 루트 위치에 타일이 없으면 설정을 중단한다 
                ruleTile = tilemap.GetTile<RuleTile>(rootPos);
                if (null == ruleTile)
                {
                    return;
                }

                ////** 
                //AppointData getData = null;
                //if (true == this.TryGetValue(rootPos, out getData))
                //{
                //    return;
                //}

                for (int i = 0; i < multi_sprites.Length; i++)
                {

                    newPos.y = rootPos.y + 1 * i;

                    newData = new AppointData();
                    newData.root_pos = rootPos;
                    newData.sprite = multi_sprites[i];
                    newData.activeMultiTile = true;
                    newData.max_size = multi_sprites.Length;
                    newData.transform = transform;

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

                RuleTile ruleTile = null;
                Vector3Int newPos = getData.root_pos;
                Vector3Int rootPos = getData.root_pos;
                int max_size = getData.max_size;
                for (int i = 0; i < max_size; i++)
                {
                    newPos.y = rootPos.y + 1 * i;

                    //ruleTile = tilemap.GetTile<RuleTile>(newPos);
                    //if (null == ruleTile)
                    //{
                    //    //처리할 데이터 없음 
                    //    continue;
                    //}


                    if (false == this.TryGetValue(newPos, out getData))
                    {
                        //처리할 데이터 없음 
                        continue;
                    }

                    //if (false == getData.activeMultiTile)
                        //return;

                    getData.Init();

                }

            }

            /// <summary>
            /// 루트위치가 활정중인지 알려준다 
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
                RuleTile ruleTile = tilemap.GetTile<RuleTile>(rootData.root_pos);
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

        public AppointDataMap _appointDataMap = new AppointDataMap();

        //========================================================================
        //========================================================================

        public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject instantiateedGameObject)
        {
            if (instantiateedGameObject != null)
            {
                instantiateedGameObject.transform.position = location + new Vector3(0.5f,0.5f,0);
                instantiateedGameObject.transform.rotation = m_GameObjectQuaternion;
            }
            _appointDataMap._ruleTileObject = this;
            if(null == _appointDataMap._DST_TileMap)
            {
                //todo : fixe me : 임시처리 
                _appointDataMap._DST_TileMap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
            }

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

            if(_appointDataMap.IsRoot(position))
            {
                //현재 위치에 멀티타일 루트설정이 있으면 해제한다
                _appointDataMap.ResetAll(position, tilemap);
            }

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
                        case TilingRule.OutputSprite.Multi:
                            {
                                //현재 위치를 포함하는 활성화된 루트설정이 있는 경우 
                                if (true == _appointDataMap.IsActive_Root(position, tilemap))
                                {
                                    //설정을 안한다 
                                    break;
                                }

                                //루트 멀티타일 설정을 한다 
                                _appointDataMap.SetMultiData(position, tilemap, rule.m_Sprites , transform);
                                //DebugWide.LogBlue(_appoint_sprite); //chamto test

                            }
                            break;
						case TilingRule.OutputSprite.Animation:
							tileData.sprite = rule.m_Sprites[0];
							break;
						case TilingRule.OutputSprite.Random:
							int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
							tileData.sprite = rule.m_Sprites[index];
							if (rule.m_RandomTransform != TilingRule.Transform.Fixed)
								transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
							break;
					}
                    tileData.transform = transform;
					tileData.gameObject = rule.m_GameObject;
                    tileData.colliderType = rule.m_ColliderType;

                    // Converts the tile's rotation matrix to a quaternion to be used by the instantiated Game Object
                    m_GameObjectQuaternion = Quaternion.LookRotation(new Vector3(transform.m02, transform.m12, transform.m22), new Vector3(transform.m01, transform.m11, transform.m21));
                    break;
				}

			}//end for

            //==================================================================================
            //DebugWide.LogBlue(_appointDataMap.IsActive_MultiTile_Child(position) + "   " + position);

            //멀티설정이 있는 경우, 설정된 값으로 덮는다 
            if (_appointDataMap.IsActive_Root(position, tilemap))
            {
                //멀티설정 적용 
                tileData.sprite = _appointDataMap[position].sprite;
                tileData.transform = _appointDataMap[position].transform;

                //DebugWide.LogBlue(_appointDataMap[position].sprite.name + "   " + position);
            }else
            {
                if(true == _appointDataMap.IsActive_Child(position))
                {
                    //루트설정이 해제되어 있는데, 자식설정이 활성상태인 경우 
                    //자식설정값을 제거해 준다
                    _appointDataMap.Reset(position);
                }
            }
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

                //세로로 길게 배치되는 타일 호출
                //base.RefreshTile(location + new Vector3Int(0, 2, 0), tileMap);
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
			switch (neighbor)
			{
				case TilingRule.Neighbor.This: return tile == m_Self;
				case TilingRule.Neighbor.NotThis: return tile != m_Self;
			}
			return true;
		}

		public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, int angle)
		{
			for (int i = 0; i < NeighborCount; ++i)
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
			for (int i = 0; i < NeighborCount; ++i)
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

        public bool RuleMatchesFull_ToNeighborLength(ITilemap tilemap, Vector3Int position, TilingRule rule, ref Matrix4x4 transform)
        {
            // Check rule against rotations of 0, 90, 180, 270
            for (int angle = 0; angle <= (rule.m_RuleTransform == TilingRule.Transform.Rotated ? 270 : 0); angle += 90)
            {
                if (RuleMatches_ToNeighborLength(tilemap, position, rule, angle, false, false))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                    return true;
                }
            }

            // Check rule against x-axis mirror
            if ((rule.m_RuleTransform == TilingRule.Transform.MirrorX) && RuleMatches_ToNeighborLength(tilemap, position, rule, 0, true, false))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
                return true;
            }

            // Check rule against y-axis mirror
            if ((rule.m_RuleTransform == TilingRule.Transform.MirrorY) && RuleMatches_ToNeighborLength(tilemap, position, rule, 0, false, true))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
                return true;
            }

            return false;
        }

        public bool RuleMatches_ToNeighborLength(ITilemap tilemap, Vector3Int position, TilingRule rule, int angle, bool mirrorX, bool mirrorY)
		{

            //각도모드와 미러모드로 각각 동작한다
            //둘 동시에 작동못함 
            if(0 < angle )
            {
                mirrorX = false;
                mirrorY = false;
            }


            //0 1 2  :  (-1, 1)  ( 0, 1)  ( 1, 1)
            //3 x 4  :  (-1, 0)  ( 0, 0)  ( 1, 0)
            //5 6 7  :  (-1,-1)  ( 0,-1)  ( 1,-1)
            int seq = 0;
            TileBase tile = null;
            TileBase[] neighboringTiles = new TileBase[NeighborCount];
            for (int y = 1; y >= -1; y--)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x != 0 || y != 0)
                    {
                        int add_len = rule.m_Neighbors_Length[seq];
                        Vector3Int tilePosition = new Vector3Int(position.x + x * add_len, position.y + y * add_len, position.z);
                        tile = tilemap.GetTile(tilePosition);
                        neighboringTiles[seq] = tile;
                        seq++;
                    }
                }
            }

            int index = -1;
            for (int i = 0; i < NeighborCount; ++i)
            {
                if (0 < angle)
                {
                    index = GetRotatedIndex(i, angle);    
                }else
                {
                    index = GetMirroredIndex(i, mirrorX, mirrorY);    
                }


                tile = neighboringTiles[index];
                if (!RuleMatch(rule.m_Neighbors[i], tile))
                {
                    return false;
                }
            }

            return true;
		}

		private void GetMatchingNeighboringTiles(ITilemap tilemap, Vector3Int position, ref TileBase[] neighboringTiles)
		{
			if (neighboringTiles != null)
				return;

			if (m_CachedNeighboringTiles == null || m_CachedNeighboringTiles.Length < NeighborCount)
				m_CachedNeighboringTiles = new TileBase[NeighborCount];

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
