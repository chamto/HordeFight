using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;


namespace UnityEngine
{

    /// <summary>
    /// Generic visual tile for creating different tilesets like terrain, pipeline, random or animated tiles.
    /// This is templated to accept a Neighbor Rule Class for Custom Rules.
    /// </summary>
    /// <typeparam name="T">Neighbor Rule Class for Custom Rules</typeparam>
    public class RuleTile_Custom<T> : RuleTile_Custom
    {
        /// <summary>
        /// Returns the Neighbor Rule Class type for this Rule Tile.
        /// </summary>
        public sealed override Type m_NeighborType => typeof(T);
    }

    /// <summary>
    /// Generic visual tile for creating different tilesets like terrain, pipeline, random or animated tiles.
    /// </summary>
    [CreateAssetMenu]
    [Serializable]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleTile.html")]
    public class RuleTile_Custom : TileBase
    {
        /// <summary>
        /// Returns the default Neighbor Rule Class type.
        /// </summary>
        public virtual Type m_NeighborType => typeof(TilingRuleOutput.Neighbor);

        /// <summary>
        /// The Default Sprite set when creating a new Rule.
        /// </summary>
        public Sprite m_DefaultSprite;
        /// <summary>
        /// The Default GameObject set when creating a new Rule.
        /// </summary>
        public GameObject m_DefaultGameObject;
        /// <summary>
        /// The Default Collider Type set when creating a new Rule.
        /// </summary>
        public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

        /// <summary>
        /// Angle in which the RuleTile is rotated by for matching in Degrees.
        /// </summary>
        public virtual int m_RotationAngle => 90;

        /// <summary>
        /// Number of rotations the RuleTile can be rotated by for matching.
        /// </summary>
        public int m_RotationCount => 360 / m_RotationAngle;


        public TileBase[] AdjacentTiles; //그리드 상의 타일종류를 등록한다. 등록된 정보로 인접한 타일이 맞는지 검사에 사용된다 

        /// <summary>
        /// The data structure holding the Rule information for matching Rule Tiles with
        /// its neighbors.
        /// </summary>
        [Serializable]
        public class TilingRuleOutput
        {
            /// <summary>
            /// Id for this Rule.
            /// </summary>
            public int m_Id;
            /// <summary>
            /// The output Sprites for this Rule.
            /// </summary>
            public Sprite[] m_Sprites = new Sprite[1];
            /// <summary>
            /// The output GameObject for this Rule.
            /// </summary>
            public GameObject m_GameObject;
            /// <summary>
            /// The output minimum Animation Speed for this Rule.
            /// </summary>
            [FormerlySerializedAs("m_AnimationSpeed")]
            public float m_MinAnimationSpeed = 1f;
            /// <summary>
            /// The output maximum Animation Speed for this Rule.
            /// </summary>
            [FormerlySerializedAs("m_AnimationSpeed")]
            public float m_MaxAnimationSpeed = 1f;
            /// <summary>
            /// The perlin scale factor for this Rule.
            /// </summary>
            public float m_PerlinScale = 0.5f;
            /// <summary>
            /// The output type for this Rule.
            /// </summary>
            public OutputSprite m_Output = OutputSprite.Single;
            /// <summary>
            /// The output Collider Type for this Rule.
            /// </summary>
            public Tile.ColliderType m_ColliderType = Tile.ColliderType.Sprite;
            /// <summary>
            /// The randomized transform output for this Rule.
            /// </summary>
            public Transform m_RandomTransform;

            /// <summary>
            /// The enumeration for matching Neighbors when matching Rule Tiles
            /// </summary>
            public class Neighbor
            {
                //public const int DontCare = 0;

                /// <summary>
                /// The Rule Tile will check if the contents of the cell in that direction is an instance of this Rule Tile.
                /// If not, the rule will fail.
                /// </summary>
                public const int This = 1;
                /// <summary>
                /// The Rule Tile will check if the contents of the cell in that direction is not an instance of this Rule Tile.
                /// If it is, the rule will fail.
                /// </summary>
                public const int NotThis = 2;

                //타일링룰을 구별하는데 사용하는 문자 (최대2글자)
                public const int Specifier_This = 3; //chamto add

                public const int Specifier_NotThis = 4; //chamto add

                //그리드에 인접타일 지정에 사용
                public const int Adjacent = 5; //chamto add
            }

            /// <summary>
            /// The enumeration for the transform rule used when matching Rule Tiles.
            /// </summary>
            public enum Transform
            {
                /// <summary>
                /// The Rule Tile will match Tiles exactly as laid out in its neighbors.
                /// </summary>
                Fixed,
                /// <summary>
                /// The Rule Tile will rotate and match its neighbors.
                /// </summary>
                Rotated,
                /// <summary>
                /// The Rule Tile will mirror in the X axis and match its neighbors.
                /// </summary>
                MirrorX,
                /// <summary>
                /// The Rule Tile will mirror in the Y axis and match its neighbors.
                /// </summary>
                MirrorY,
                /// <summary>
                /// The Rule Tile will mirror in the X or Y axis and match its neighbors.
                /// </summary>
                MirrorXY,
                /// <summary>
                /// The Rule Tile will rotate and mirror in the X and match its neighbors.
                /// </summary>
                RotatedMirror
            }

            /// <summary>
            /// The Output for the Tile which fits this Rule.
            /// </summary>
            public enum OutputSprite
            {
                /// <summary>
                /// A Single Sprite will be output.
                /// </summary>
                Single,
                /// <summary>
                /// A Random Sprite will be output.
                /// </summary>
                Random,
                /// <summary>
                /// A Sprite Animation will be output.
                /// </summary>
                Animation
            }
        }

        /// <summary>
        /// The data structure holding the Rule information for matching Rule Tiles with
        /// its neighbors.
        /// </summary>
        [Serializable]
        public class TilingRule : TilingRuleOutput
        {
            //chamto 분석 - 20231110
            //m_Neighbors 와 m_NeighborPositions 는 같은 인덱스로 짝을 이룬다.
            //딕셔너리를 쓰면 되는 것을 List 두개로 복잡하게 사용한다
            //m_NeighborPositions 의 초기값은 나중에 변경된다. 위치에 대한 인덱스값이 고정이 아니다.
            //이 또한 딕셔너리를 쓰면 고정의 인덱스값을 가지게 하며 확장 , 검색 할 수 있을 것이다
            //딕셔너리를 안쓴이유 찾음 : 유니티에서 딕셔너리는 직렬화를 지원하지 않음 , 직렬화 지원하는 딕셔너리를 만들어 사용해야 함 
            /// <summary>
            /// The matching Rule conditions for each of its neighboring Tiles.
            /// </summary>
            //public List<int> m_Neighbors = new List<int>(); //chamto 20231121 제거 - bucket 에 통합 
            /// <summary>
            /// * Preset this list to RuleTile backward compatible, but not support for HexagonalRuleTile backward compatible.
            /// </summary>
            //public List<Vector3Int> m_NeighborPositions = new List<Vector3Int>() //chamto 20231121 제거 - bucket 에 통합 
            //{
            //    new Vector3Int(-1, 1, 0),
            //    new Vector3Int(0, 1, 0),
            //    new Vector3Int(1, 1, 0),
            //    new Vector3Int(-1, 0, 0),
            //    new Vector3Int(1, 0, 0),
            //    new Vector3Int(-1, -1, 0),
            //    new Vector3Int(0, -1, 0),
            //    new Vector3Int(1, -1, 0),
            //};
            /// <summary>
            /// The transform matching Rule for this Rule.
            /// </summary>
            public Transform m_RuleTransform;


            public enum eDirection8 : int
            {
                none = 0,
                center = none,
                right = 1,
                rightUp = 2,
                up = 3,
                leftUp = 4,
                left = 5,
                leftDown = 6,
                down = 7,
                rightDown = 8,
                max,

            }

            //------------------------------------------------------------------------------------------
            // custom 추가 정보 
            //------------------------------------------------------------------------------------------
            public int _border_dir = 0; //arrows 경계 방향이 들어간다 , eDirection8 방향값으로 변환되어 들어간다 
            public string _specifier = "00"; //임의의 지정값 , 사용자가 인스펙터상에서 직접 지정한다

            //딕셔너리는 직렬화를 지원하지 않는다. 직렬화 기능을 추가한 딕셔너리 사용하기 
            //public Dictionary<Vector3Int, string> m_Neighbors_Specifier = new Dictionary<Vector3Int, string>(); //이웃한 객체의 지정값을 나타낸다
            public SerializeDictionary<Vector3Int, Neighbor_Bucket> m_Neighbors_bucket = new SerializeDictionary<Vector3Int, Neighbor_Bucket>(); //이웃한 객체의 지정값을 나타낸다

            [Serializable]
            public class Neighbor_Bucket
            {
                public Vector3Int _posotion = Vector3Int.zero;
                public int _kind = 0;
                public string _specifier = "00";
            }

            //------------------------------------------------------------------------------------------

            /// <summary>
            /// This clones a copy of the TilingRule.
            /// </summary>
            /// <returns>A copy of the TilingRule.</returns>
            public TilingRule Clone()
            {
                TilingRule rule = new TilingRule
                {
                    //m_Neighbors = new List<int>(m_Neighbors),
                    //m_NeighborPositions = new List<Vector3Int>(m_NeighborPositions),
                    m_RuleTransform = m_RuleTransform,
                    m_Sprites = new Sprite[m_Sprites.Length],
                    m_GameObject = m_GameObject,
                    m_MinAnimationSpeed = m_MinAnimationSpeed,
                    m_MaxAnimationSpeed = m_MaxAnimationSpeed,
                    m_PerlinScale = m_PerlinScale,
                    m_Output = m_Output,
                    m_ColliderType = m_ColliderType,
                    m_RandomTransform = m_RandomTransform,

                    _border_dir = _border_dir,
                };
                Array.Copy(m_Sprites, rule.m_Sprites, m_Sprites.Length);
                rule._specifier = string.Copy(_specifier);

                //----------------------
                //rule.m_Neighbors_Specifier = new SerializeDictionary<Vector3Int, string>(m_Neighbors_Specifier);
                rule.m_Neighbors_bucket = new SerializeDictionary<Vector3Int, Neighbor_Bucket>();
                foreach (KeyValuePair<Vector3Int, Neighbor_Bucket> pair in m_Neighbors_bucket)
                {
                    rule.m_Neighbors_bucket.Add(pair.Key, pair.Value);
                }
                //----------------------

                return rule;
            }

            /// <summary>
            /// Returns all neighbors of this Tile as a dictionary
            /// </summary>
            /// <returns>A dictionary of neighbors for this Tile</returns>
            //public Dictionary<Vector3Int, int> GetNeighbors()
            //{
            //    Dictionary<Vector3Int, int> dict = new Dictionary<Vector3Int, int>();

            //    for (int i = 0; i < m_Neighbors.Count && i < m_NeighborPositions.Count; i++)
            //        dict.Add(m_NeighborPositions[i], m_Neighbors[i]);

            //    return dict;
            //}


            /// <summary>
            /// Applies the values from the given dictionary as this Tile's neighbors
            /// </summary>
            /// <param name="dict">Dictionary to apply values from</param>
            //public void ApplyNeighbors(Dictionary<Vector3Int, int> dict)
            //{
            //    m_NeighborPositions = dict.Keys.ToList();
            //    m_Neighbors = dict.Values.ToList();
            //}

            /// <summary>
            /// Gets the cell bounds of the TilingRule.
            /// </summary>
            /// <returns>Returns the cell bounds of the TilingRule.</returns>
            public BoundsInt GetBounds()
            {
                BoundsInt bounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);
                //foreach (var neighbor in GetNeighbors())
                foreach (var neighbor in m_Neighbors_bucket)
                {
                    bounds.xMin = Mathf.Min(bounds.xMin, neighbor.Key.x);
                    bounds.yMin = Mathf.Min(bounds.yMin, neighbor.Key.y);
                    bounds.xMax = Mathf.Max(bounds.xMax, neighbor.Key.x + 1);
                    bounds.yMax = Mathf.Max(bounds.yMax, neighbor.Key.y + 1);
                }
                return bounds;
            }
        }

        /// <summary>
        /// Attribute which marks a property which cannot be overridden by a RuleOverrideTile
        /// </summary>
        public class DontOverride : Attribute { }

        /// <summary>
        /// A list of Tiling Rules for the Rule Tile.
        /// </summary>
        [HideInInspector] public List<TilingRule> m_TilingRules = new List<RuleTile_Custom.TilingRule>();

        /// <summary>
        /// Returns a set of neighboring positions for this RuleTile
        /// </summary>
        public HashSet<Vector3Int> neighborPositions
        {
            get
            {
                if (m_NeighborPositions.Count == 0)
                    UpdateNeighborPositions();

                return m_NeighborPositions;
            }
        }

        private HashSet<Vector3Int> m_NeighborPositions = new HashSet<Vector3Int>();

        /// <summary>
        /// Updates the neighboring positions of this RuleTile
        /// </summary>
        public void UpdateNeighborPositions()
        {
            m_CacheTilemapsNeighborPositions.Clear();

            HashSet<Vector3Int> positions = m_NeighborPositions;
            positions.Clear();

            foreach (TilingRule rule in m_TilingRules)
            {
                //foreach (var neighbor in rule.GetNeighbors())
                foreach (var neighbor in rule.m_Neighbors_bucket)
                {
                    Vector3Int position = neighbor.Key;
                    positions.Add(position);

                    // Check rule against rotations of 0, 90, 180, 270
                    if (rule.m_RuleTransform == TilingRuleOutput.Transform.Rotated)
                    {
                        for (int angle = m_RotationAngle; angle < 360; angle += m_RotationAngle)
                        {
                            positions.Add(GetRotatedPosition(position, angle));
                        }
                    }
                    // Check rule against x-axis, y-axis mirror
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorXY)
                    {
                        positions.Add(GetMirroredPosition(position, true, true));
                        positions.Add(GetMirroredPosition(position, true, false));
                        positions.Add(GetMirroredPosition(position, false, true));
                    }
                    // Check rule against x-axis mirror
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorX)
                    {
                        positions.Add(GetMirroredPosition(position, true, false));
                    }
                    // Check rule against y-axis mirror
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorY)
                    {
                        positions.Add(GetMirroredPosition(position, false, true));
                    }
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.RotatedMirror)
                    {
                        var mirroredPosition = GetMirroredPosition(position, true, false);
                        for (int angle = m_RotationAngle; angle < 360; angle += m_RotationAngle)
                        {
                            positions.Add(GetRotatedPosition(position, angle));
                            positions.Add(GetRotatedPosition(mirroredPosition, angle));
                        }
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------
        // chamto 추가 - 20231109

        // custom 추가 정보 
        //------------------------------------------------------------------------------------------

        public int GetBorderDirection8(Vector3Int position)
        {
            AppointData getData = null;
            if (false == this._tileDataMap.TryGetValue(position, out getData))
            {
                return 0;
            }

            if (null == getData || null == getData._tilingRule)
                return 0;


            return (int)getData._transForDir8;
        }

        [Serializable]
        public class AppointData
        {
            public int _seq = -1; //chamto test - 추가된 순서
            public Matrix4x4 _transform = Matrix4x4.identity;
            public TilingRule _tilingRule = null;
            public TilingRule.eDirection8 _transForDir8 = 0; //변형값이 적용된 arrows 경계 방향이 들어간다

            public void Init()
            {
                //_idx = -1;
                _transform = Matrix4x4.identity;
                _tilingRule = null;
                _transForDir8 = 0;
            }

            //public enum eDirection8 : int
            //{
            //    none = 0,
            //    center = none,
            //    right = 1,
            //    rightUp = 2,
            //    up = 3,
            //    leftUp = 4,
            //    left = 5,
            //    leftDown = 6,
            //    down = 7,
            //    rightDown = 8,
            //    max,
            //}
            //-z축 기준 
            private Vector3[] _dir8_normal3D_AxisMZ = new Vector3[]
            {   new Vector3(0,0,0) ,                //    zero = 0, 
            new Vector3(1,0,0).normalized ,     //    right = 1, 
            new Vector3(1,1,0).normalized ,     //    rightUp = 2, 
            new Vector3(0,1,0).normalized ,     //    up = 3,
            new Vector3(-1,1,0).normalized ,    //    leftUp = 4,
            new Vector3(-1,0,0).normalized ,    //    left = 5,
            new Vector3(-1,-1,0).normalized ,   //    leftDown = 6,
            new Vector3(0,-1,0).normalized ,    //    down = 7,
            new Vector3(1,-1,0).normalized ,    //    rightDown = 8,
            new Vector3(1,0,0).normalized ,     //    right = 9,
            };

            public Vector3 GetDir8_Normal3D_AxisMZ(int eDirection)
            {
                return _dir8_normal3D_AxisMZ[(int)eDirection];
            }

            public TilingRule.eDirection8 GetDir8_AxisMZ(Vector3 dir)
            {
                float dot = dir.x * dir.x + dir.y * dir.y;
                if (float.Epsilon >= dot) return TilingRule.eDirection8.none;
                //if (Misc.IsZero(dir)) return eDirection8.none;

                float rad = (float)Math.Atan2(dir.y, dir.x);
                float deg = Mathf.Rad2Deg * rad;

                //각도가 음수라면 360을 더한다 
                if (deg < 0) deg += 360f;

                //360 / 45 = 8
                int quad = Mathf.RoundToInt(deg / 45f);
                quad %= 8; //8 => 0 , 8을 0으로 변경  
                quad++; //값의 범위를 0~7 에서 1~8로 변경 

                return (TilingRule.eDirection8)quad;
            }

            /// <summary>
            /// 타일의 경계방향을 특정 좌표축 기준(Axis Minus Z)으로 단위벡터값으로 변화시켜 준다 
            /// </summary>
            /// <returns></returns>
            public Vector3 Trans_BorderDir_AxisMZ()
            {
                if (null == _tilingRule) return Vector3.zero;

                Vector3 n = GetDir8_Normal3D_AxisMZ(_tilingRule._border_dir);
                return _transform * n;
                
            }

            public void ApplyData()
            {
                if (null == _tilingRule) return;

                //if (UtilGS9.eDirection8.none != tilingRule._push_dir8) return;

                Vector3 tn = Trans_BorderDir_AxisMZ();
                _transForDir8 = GetDir8_AxisMZ(tn);

                //DebugWide.LogBlue(n + "   " + tn + "    " + eTransDir);
            }

        }

        public class TileDataMap : Dictionary<Vector3Int, AppointData>
        {
            //private int _count = 0;

            public void InitData(Vector3Int position)
            {
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    return;
                }

                getData.Init();
            }

            //public void AddOrUpdate(Vector3Int position, AppointData data)
            //{
                
            //    AppointData getData = null;
            //    if (false == this.TryGetValue(position, out getData))
            //    {
            //        //_count++;
            //        //data._idx = _count;
            //        this.Add(position, data);
            //    }
            //    this[position] = data;

            //    //if (null != data)
            //    //    data.ApplyData(); 
            //}

            public void AddOrUpdate(Vector3Int position, TilingRule rule, Matrix4x4 transform)
            {
                
                AppointData getData = null;
                if (false == this.TryGetValue(position, out getData))
                {
                    getData = new AppointData();
                    getData.Init();
                    this.Add(position, getData);
                }

                //위치만 있고 알맹이가 없는 경우 
                if (null == getData)
                {
                    getData = new AppointData();
                    getData.Init();
                    this.Add(position, getData);
                }

                //chamto test 
                //_count++;
                //getData._seq = _count;

                getData._transform = transform;
                getData._tilingRule = rule;
                getData.ApplyData();
            }
        }

        public TileDataMap _tileDataMap = new TileDataMap();

        //------------------------------------------------------------------------------------------

        /// <summary>
        /// StartUp is called on the first frame of the running Scene.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="instantiatedGameObject">The GameObject instantiated for the Tile.</param>
        /// <returns>Whether StartUp was successful</returns>
        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
        {
            if (instantiatedGameObject != null)
            {
                Tilemap tmpMap = tilemap.GetComponent<Tilemap>();
                Matrix4x4 orientMatrix = tmpMap.orientationMatrix;

                var iden = Matrix4x4.identity;
                Vector3 gameObjectTranslation = new Vector3();
                Quaternion gameObjectRotation = new Quaternion();
                Vector3 gameObjectScale = new Vector3();

                bool ruleMatched = false;
                Matrix4x4 transform = iden;
                foreach (TilingRule rule in m_TilingRules)
                {
                    if (RuleMatches(rule, position, tilemap, ref transform))
                    {
                        transform = orientMatrix * transform;

                        // Converts the tile's translation, rotation, & scale matrix to values to be used by the instantiated GameObject
                        gameObjectTranslation = new Vector3(transform.m03, transform.m13, transform.m23);
                        gameObjectRotation = Quaternion.LookRotation(new Vector3(transform.m02, transform.m12, transform.m22), new Vector3(transform.m01, transform.m11, transform.m21));
                        gameObjectScale = transform.lossyScale;

                        ruleMatched = true;
                        break;
                    }
                }
                if (!ruleMatched)
                {
                    // Fallback to just using the orientMatrix for the translation, rotation, & scale values.
                    gameObjectTranslation = new Vector3(orientMatrix.m03, orientMatrix.m13, orientMatrix.m23);
                    gameObjectRotation = Quaternion.LookRotation(new Vector3(orientMatrix.m02, orientMatrix.m12, orientMatrix.m22), new Vector3(orientMatrix.m01, orientMatrix.m11, orientMatrix.m21));
                    gameObjectScale = orientMatrix.lossyScale;
                }

                instantiatedGameObject.transform.localPosition = gameObjectTranslation + tmpMap.CellToLocalInterpolated(position + tmpMap.tileAnchor);
                instantiatedGameObject.transform.localRotation = gameObjectRotation;
                instantiatedGameObject.transform.localScale = gameObjectScale;
            }

            return true;
        }

        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            var iden = Matrix4x4.identity;

            tileData.sprite = m_DefaultSprite;
            tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;
            tileData.transform = iden;

            //** 기록 초기화 
            _tileDataMap.InitData(position);

            Matrix4x4 transform = iden;
            foreach (TilingRule rule in m_TilingRules)
            {
                if (RuleMatches(rule, position, tilemap, ref transform))
                {
                    switch (rule.m_Output)
                    {
                        case TilingRuleOutput.OutputSprite.Single:
                        case TilingRuleOutput.OutputSprite.Animation:
                            tileData.sprite = rule.m_Sprites[0];
                            break;
                        case TilingRuleOutput.OutputSprite.Random:
                            int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                            tileData.sprite = rule.m_Sprites[index];
                            if (rule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
                                transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                            break;
                    }
                    tileData.transform = transform;
                    tileData.gameObject = rule.m_GameObject;
                    tileData.colliderType = rule.m_ColliderType;
                    

                    //** 어떤 규칙이 어느 위치로 들어갔는지 기록 
                    _tileDataMap.AddOrUpdate(position, rule, transform);

                    break;
                }
            }

            //Debug.Log(""+position); //chamto test
        }

        /// <summary>
        /// Returns a Perlin Noise value based on the given inputs.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="scale">The Perlin Scale factor of the Tile.</param>
        /// <param name="offset">Offset of the Tile on the Tilemap.</param>
        /// <returns>A Perlin Noise value based on the given inputs.</returns>
        public static float GetPerlinValue(Vector3Int position, float scale, float offset)
        {
            return Mathf.PerlinNoise((position.x + offset) * scale, (position.y + offset) * scale);
        }

        static Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>> m_CacheTilemapsNeighborPositions = new Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>>();
        static TileBase[] m_AllocatedUsedTileArr = Array.Empty<TileBase>();

        static bool IsTilemapUsedTilesChange(Tilemap tilemap, out KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> hashSet)
        {
            if (!m_CacheTilemapsNeighborPositions.TryGetValue(tilemap, out hashSet))
                return true;

            var oldUsedTiles = hashSet.Key;
            int newUsedTilesCount = tilemap.GetUsedTilesCount();
            if (newUsedTilesCount != oldUsedTiles.Count)
                return true;

            if (m_AllocatedUsedTileArr.Length < newUsedTilesCount)
                Array.Resize(ref m_AllocatedUsedTileArr, newUsedTilesCount);

            tilemap.GetUsedTilesNonAlloc(m_AllocatedUsedTileArr);
            for (int i = 0; i < newUsedTilesCount; i++)
            {
                TileBase newUsedTile = m_AllocatedUsedTileArr[i];
                if (!oldUsedTiles.Contains(newUsedTile))
                    return true;
            }

            return false;
        }

        static KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> CachingTilemapNeighborPositions(Tilemap tilemap)
        {
            int usedTileCount = tilemap.GetUsedTilesCount();
            HashSet<TileBase> usedTiles = new HashSet<TileBase>();
            HashSet<Vector3Int> neighborPositions = new HashSet<Vector3Int>();

            if (m_AllocatedUsedTileArr.Length < usedTileCount)
                Array.Resize(ref m_AllocatedUsedTileArr, usedTileCount);

            tilemap.GetUsedTilesNonAlloc(m_AllocatedUsedTileArr);

            for (int i = 0; i < usedTileCount; i++)
            {
                TileBase tile = m_AllocatedUsedTileArr[i];
                usedTiles.Add(tile);
                RuleTile ruleTile = null;

                if (tile is RuleTile rt)
                    ruleTile = rt;
                else if (tile is RuleOverrideTile ot)
                    ruleTile = ot.m_Tile;

                if (ruleTile)
                    foreach (Vector3Int neighborPosition in ruleTile.neighborPositions)
                        neighborPositions.Add(neighborPosition);
            }

            var value = new KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>(usedTiles, neighborPositions);
            m_CacheTilemapsNeighborPositions[tilemap] = value;
            return value;
        }

        static bool NeedRelease()
        {
            foreach (var keypair in m_CacheTilemapsNeighborPositions)
            {
                if (keypair.Key == null)
                {
                    return true;
                }
            }
            return false;
        }

        static void ReleaseDestroyedTilemapCacheData()
        {
            if (!NeedRelease())
                return;

            var hasCleared = false;
            var keys = m_CacheTilemapsNeighborPositions.Keys.ToArray();
            foreach (var key in keys)
            {
                if (key == null && m_CacheTilemapsNeighborPositions.Remove(key))
                    hasCleared = true;
            }
            if (hasCleared)
            {
                // TrimExcess
                m_CacheTilemapsNeighborPositions = new Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>>(m_CacheTilemapsNeighborPositions);
            }
        }

        /// <summary>
        /// Retrieves any tile animation data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileAnimationData">Data to run an animation on the tile.</param>
        /// <returns>Whether the call was successful.</returns>
        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            Matrix4x4 transform = Matrix4x4.identity;
            foreach (TilingRule rule in m_TilingRules)
            {
                if (rule.m_Output == TilingRuleOutput.OutputSprite.Animation)
                {
                    if (RuleMatches(rule, position, tilemap, ref transform))
                    {
                        tileAnimationData.animatedSprites = rule.m_Sprites;
                        tileAnimationData.animationSpeed = Random.Range(rule.m_MinAnimationSpeed, rule.m_MaxAnimationSpeed);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called when the tile is refreshed.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);

            Tilemap baseTilemap = tilemap.GetComponent<Tilemap>();

            ReleaseDestroyedTilemapCacheData(); // Prevent memory leak

            if (IsTilemapUsedTilesChange(baseTilemap, out var neighborPositionsSet))
                neighborPositionsSet = CachingTilemapNeighborPositions(baseTilemap);

            var neighborPositionsRuleTile = neighborPositionsSet.Value;
            foreach (Vector3Int offset in neighborPositionsRuleTile)
            {
                Vector3Int offsetPosition = GetOffsetPositionReverse(position, offset);
                TileBase tile = tilemap.GetTile(offsetPosition);
                RuleTile ruleTile = null;

                if (tile is RuleTile rt)
                    ruleTile = rt;
                else if (tile is RuleOverrideTile ot)
                    ruleTile = ot.m_Tile;

                if (ruleTile != null)
                    if (ruleTile == this || ruleTile.neighborPositions.Contains(offset))
                        base.RefreshTile(offsetPosition, tilemap);
            }
        }

        /// <summary>
        /// Does a Rule Match given a Tiling Rule and neighboring Tiles.
        /// </summary>
        /// <param name="rule">The Tiling Rule to match with.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The tilemap to match with.</param>
        /// <param name="transform">A transform matrix which will match the Rule.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public virtual bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
        {
            if (RuleMatches(rule, position, tilemap, 0))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
                return true;
            }

            // Check rule against rotations of 0, 90, 180, 270
            if (rule.m_RuleTransform == TilingRuleOutput.Transform.Rotated)
            {
                for (int angle = m_RotationAngle; angle < 360; angle += m_RotationAngle)
                {
                    if (RuleMatches(rule, position, tilemap, angle))
                    {
                        transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                        return true;
                    }
                }
            }
            // Check rule against x-axis, y-axis mirror
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorXY)
            {
                if (RuleMatches(rule, position, tilemap, true, true))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, -1f, 1f));
                    return true;
                }
                if (RuleMatches(rule, position, tilemap, true, false))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
                    return true;
                }
                if (RuleMatches(rule, position, tilemap, false, true))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
                    return true;
                }
            }
            // Check rule against x-axis mirror
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorX)
            {
                if (RuleMatches(rule, position, tilemap, true, false))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
                    return true;
                }
            }
            // Check rule against y-axis mirror
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorY)
            {
                if (RuleMatches(rule, position, tilemap, false, true))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
                    return true;
                }
            }
            // Check rule against x-axis mirror with rotations of 0, 90, 180, 270
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.RotatedMirror)
            {
                for (int angle = 0; angle < 360; angle += m_RotationAngle)
                {
                    if (angle != 0 && RuleMatches(rule, position, tilemap, angle))
                    {
                        transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                        return true;
                    }
                    if (RuleMatches(rule, position, tilemap, angle, true))
                    {
                        transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), new Vector3(-1f, 1f, 1f));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a random transform matrix given the random transform rule.
        /// </summary>
        /// <param name="type">Random transform rule.</param>
        /// <param name="original">The original transform matrix.</param>
        /// <param name="perlinScale">The Perlin Scale factor of the Tile.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <returns>A random transform matrix.</returns>
        public virtual Matrix4x4 ApplyRandomTransform(TilingRuleOutput.Transform type, Matrix4x4 original, float perlinScale, Vector3Int position)
        {
            float perlin = GetPerlinValue(position, perlinScale, 200000f);
            switch (type)
            {
                case TilingRuleOutput.Transform.MirrorXY:
                    return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Math.Abs(perlin - 0.5) > 0.25 ? 1f : -1f, perlin < 0.5 ? 1f : -1f, 1f));
                case TilingRuleOutput.Transform.MirrorX:
                    return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(perlin < 0.5 ? 1f : -1f, 1f, 1f));
                case TilingRuleOutput.Transform.MirrorY:
                    return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, perlin < 0.5 ? 1f : -1f, 1f));
                case TilingRuleOutput.Transform.Rotated:
                    {
                        var angle = Mathf.Clamp(Mathf.FloorToInt(perlin * m_RotationCount), 0, m_RotationCount - 1) * m_RotationAngle;
                        return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                    }
                case TilingRuleOutput.Transform.RotatedMirror:
                    {
                        var angle = Mathf.Clamp(Mathf.FloorToInt(perlin * m_RotationCount), 0, m_RotationCount - 1) * m_RotationAngle;
                        return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), new Vector3(perlin < 0.5 ? 1f : -1f, 1f, 1f));
                    }
            }
            return original;
        }

        /// <summary>
        /// Returns custom fields for this RuleTile
        /// </summary>
        /// <param name="isOverrideInstance">Whether override fields are returned</param>
        /// <returns>Custom fields for this RuleTile</returns>
        public FieldInfo[] GetCustomFields(bool isOverrideInstance)
        {
            return this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => typeof(RuleTile).GetField(field.Name) == null)
                .Where(field => field.IsPublic || field.IsDefined(typeof(SerializeField)))
                .Where(field => !field.IsDefined(typeof(HideInInspector)))
                .Where(field => !isOverrideInstance || !field.IsDefined(typeof(DontOverride)))
                .ToArray();
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile.
        /// </summary>
        /// <param name="neighbor">Neighbor matching rule.</param>
        /// <param name="other">Tile to match.</param>
        /// <returns>True if there is a match, False if not.</returns>
        //public virtual bool RuleMatch(int neighbor, TilingRule.Neighbor_Bucket bucket, TileBase other)
        public virtual bool RuleMatch(TilingRule.Neighbor_Bucket bucket, Vector3Int neighbor_worldPos, TileBase other)
        {
            if (other is RuleOverrideTile ot)
                other = ot.m_InstanceTile;

            switch (bucket._kind)
            {
                case TilingRuleOutput.Neighbor.This: return other == this;
                case TilingRuleOutput.Neighbor.NotThis: return other != this;
                case TilingRuleOutput.Neighbor.Specifier_This: 
                    {
                        
                        RuleTile_Custom rule_c_other = other as RuleTile_Custom;
                        if (null != rule_c_other && this == rule_c_other)
                        {
                            //같은 룰타일커스톰 이어야 하며 , 동일 객체여야 한다
                            if (true == rule_c_other._tileDataMap.ContainsKey(neighbor_worldPos))
                            {
                                //위치기록은 되어있는데 데이터가 없는 경우 
                                if (null == rule_c_other._tileDataMap[neighbor_worldPos]._tilingRule)
                                    return false;

                                //지정자를 비교한다
                                if (bucket._specifier == rule_c_other._tileDataMap[neighbor_worldPos]._tilingRule._specifier)
                                    return true;
                            }

                        }
                        return false;
                    }
                case TilingRuleOutput.Neighbor.Specifier_NotThis:
                    {

                        RuleTile_Custom rule_c_other = other as RuleTile_Custom;
                        if (null != rule_c_other && this == rule_c_other)
                        {
                            //같은 룰타일커스톰 이어야 하며 , 동일 객체여야 한다
                            if (true == rule_c_other._tileDataMap.ContainsKey(neighbor_worldPos))
                            {
                                //위치기록은 되어있는데 데이터가 없는 경우 
                                if (null == rule_c_other._tileDataMap[neighbor_worldPos]._tilingRule)
                                    return true;

                                //지정자를 비교한다
                                if (bucket._specifier == rule_c_other._tileDataMap[neighbor_worldPos]._tilingRule._specifier)
                                    return false;
                            }

                        }
                        return true;
                    }
                case TilingRuleOutput.Neighbor.Adjacent:
                    return AdjacentTiles.Contains(other);
            }
            return true;
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile with a rotation angle.
        /// </summary>
        /// <param name="rule">Neighbor matching rule.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">Tilemap to match.</param>
        /// <param name="angle">Rotation angle for matching.</param>
        /// <returns>True if there is a match, False if not.</returns>
        //public bool old_RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle, bool mirrorX = false)
        //{
        //    var minCount = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
            
        //    for (int i = 0; i < minCount; i++)
        //    {
        //        var neighbor = rule.m_Neighbors[i];
        //        var neighborPosition = rule.m_NeighborPositions[i];
        //        var bucket = rule.m_Neighbors_bucket[neighborPosition];

        //        if (mirrorX)
        //            neighborPosition = GetMirroredPosition(neighborPosition, true, false);
        //        var positionOffset = GetRotatedPosition(neighborPosition, angle);
        //        var other = tilemap.GetTile(GetOffsetPosition(position, positionOffset));
        //        if (!RuleMatch(neighbor,bucket, other))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle, bool mirrorX = false)
        {
            foreach (var pair in rule.m_Neighbors_bucket)
            {
                var neighborPosition = pair.Key;
                
                if (mirrorX)
                    neighborPosition = GetMirroredPosition(neighborPosition, true, false);
                var positionOffset = GetRotatedPosition(neighborPosition, angle);
                Vector3Int neighbor_worldPos = GetOffsetPosition(position, positionOffset);
                var other = tilemap.GetTile(neighbor_worldPos);
                if (!RuleMatch(pair.Value, neighbor_worldPos, other))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile with mirrored axii.
        /// </summary>
        /// <param name="rule">Neighbor matching rule.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">Tilemap to match.</param>
        /// <param name="mirrorX">Mirror X Axis for matching.</param>
        /// <param name="mirrorY">Mirror Y Axis for matching.</param>
        /// <returns>True if there is a match, False if not.</returns>
        //public bool old_RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
        //{
        //    var minCount = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
        //    for (int i = 0; i < minCount; i++)
        //    {
        //        int neighbor = rule.m_Neighbors[i];
        //        var neighborPosition = rule.m_NeighborPositions[i];
        //        var bucket = rule.m_Neighbors_bucket[neighborPosition];

        //        Vector3Int positionOffset = GetMirroredPosition(rule.m_NeighborPositions[i], mirrorX, mirrorY);
        //        TileBase other = tilemap.GetTile(GetOffsetPosition(position, positionOffset));
        //        if (!RuleMatch(neighbor,bucket, other))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
        {
            //var minCount = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
            //for (int i = 0; i < minCount; i++)

            foreach (var pair in rule.m_Neighbors_bucket)
            {
                var neighborPosition = pair.Key;
                
                Vector3Int positionOffset = GetMirroredPosition(neighborPosition, mirrorX, mirrorY);
                Vector3Int neighbor_worldPos = GetOffsetPosition(position, positionOffset);
                TileBase other = tilemap.GetTile(neighbor_worldPos);
                if (!RuleMatch(pair.Value, neighbor_worldPos, other))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets a rotated position given its original position and the rotation in degrees. 
        /// </summary>
        /// <param name="position">Original position of Tile.</param>
        /// <param name="rotation">Rotation in degrees.</param>
        /// <returns>Rotated position of Tile.</returns>
        public virtual Vector3Int GetRotatedPosition(Vector3Int position, int rotation)
        {
            switch (rotation)
            {
                case 0:
                    return position;
                case 90:
                    return new Vector3Int(position.y, -position.x, 0);
                case 180:
                    return new Vector3Int(-position.x, -position.y, 0);
                case 270:
                    return new Vector3Int(-position.y, position.x, 0);
            }
            return position;
        }

        /// <summary>
        /// Gets a mirrored position given its original position and the mirroring axii.
        /// </summary>
        /// <param name="position">Original position of Tile.</param>
        /// <param name="mirrorX">Mirror in the X Axis.</param>
        /// <param name="mirrorY">Mirror in the Y Axis.</param>
        /// <returns>Mirrored position of Tile.</returns>
        public virtual Vector3Int GetMirroredPosition(Vector3Int position, bool mirrorX, bool mirrorY)
        {
            if (mirrorX)
                position.x *= -1;
            if (mirrorY)
                position.y *= -1;
            return position;
        }

        /// <summary>
        /// Get the offset for the given position with the given offset.
        /// </summary>
        /// <param name="position">Position to offset.</param>
        /// <param name="offset">Offset for the position.</param>
        /// <returns>The offset position.</returns>
        public virtual Vector3Int GetOffsetPosition(Vector3Int position, Vector3Int offset)
        {
            return position + offset;
        }

        /// <summary>
        /// Get the reversed offset for the given position with the given offset.
        /// </summary>
        /// <param name="position">Position to offset.</param>
        /// <param name="offset">Offset for the position.</param>
        /// <returns>The reversed offset position.</returns>
        public virtual Vector3Int GetOffsetPositionReverse(Vector3Int position, Vector3Int offset)
        {
            return position - offset;
        }

        public void Debug_Print_BoderDir(Tilemap tilemap)
        {
            
            foreach (Vector3Int XY_2d in tilemap.cellBounds.allPositionsWithin)
            {
                RuleTile_Custom ruleTile = tilemap.GetTile(XY_2d) as RuleTile_Custom;
                if (null == ruleTile) continue;

                Debug_Print_BoderDir(ruleTile);

            }

        }

        public void Debug_Print_BoderDir(RuleTile_Custom ruleTile)
        {
            if (null == ruleTile) return;

            foreach (var pair in ruleTile._tileDataMap)
            {
                
                //var t_rule = pair.Value._tilingRule;
                
                //PrintText(pair.Key, Color.white, ""+ t_rule._border_dir);

                Vector3 center = pair.Key;
                center.x += 0.5f;
                center.y += 0.5f;
                DrawLine_BorderDirXY(pair.Value._transForDir8, 1, center);
            }
        }

        public void Debug_Print_TileSeq()
        {
            foreach (var pair in _tileDataMap)
            {
                string temp = "";
                var t_rule = pair.Value._tilingRule;
                if (null != t_rule)
                    temp += t_rule._specifier;
                
                PrintText(pair.Key, Color.white, ""+ pair.Value._seq + " - " + temp);
                
            }
        }

        public void DrawLine_BorderDirXY(TilingRule.eDirection8 eDir , float cellSize , Vector3 centerPos)
        {
            
            if (TilingRule.eDirection8.none == eDir) return;

                
            float size = cellSize * 0.5f;
            Vector3 origin = Vector3.zero , last = Vector3.zero , temp = centerPos;
            switch (eDir)
            {
                case TilingRule.eDirection8.up:
                    {
                        temp.y = temp.y + size;
                        temp.x = centerPos.x - size;
                        origin = temp;

                        temp.x = centerPos.x + size;
                        last = temp;
                    }
                    break;
                case TilingRule.eDirection8.down:
                    {
                        temp.y = temp.y - size;
                        temp.x = centerPos.x - size;
                        origin = temp;

                        temp.x = centerPos.x + size;
                        last = temp;

                    }
                    break;
                case TilingRule.eDirection8.left:
                    {
                        temp.x = temp.x - size;
                        temp.y = centerPos.y + size;
                        origin = temp;

                        temp.y = centerPos.y - size;
                        last = temp;

                    }
                    break;
                case TilingRule.eDirection8.right:
                    {
                        temp.x = temp.x + size;
                        temp.y = centerPos.y + size;
                        origin = temp;

                        temp.y = centerPos.y - size;
                        last = temp;

                    }
                    break;
                case TilingRule.eDirection8.leftUp:
                    {
                           
                        temp = centerPos;
                        temp.x -= size;
                        temp.y -= size;
                        origin = temp;

                        temp = centerPos;
                        temp.x += size;
                        temp.y += size;
                        last = temp;

                    }
                    break;
                case TilingRule.eDirection8.rightUp:
                    {
                            
                        temp = centerPos;
                        temp.x -= size;
                        temp.y += size;
                        origin = temp;

                        temp = centerPos;
                        temp.x += size;
                        temp.y -= size;
                        last = temp;


                    }
                    break;
                case TilingRule.eDirection8.leftDown:
                    {
                           
                        temp = centerPos;
                        temp.x -= size;
                        temp.y += size;
                        origin = temp;

                        temp = centerPos;
                        temp.x += size;
                        temp.y -= size;
                        last = temp;

                    }
                    break;
                case TilingRule.eDirection8.rightDown:
                    {
                            
                        temp = centerPos;
                        temp.x -= size;
                        temp.y -= size;
                        origin = temp;

                        temp = centerPos;
                        temp.x += size;
                        temp.y += size;
                        last = temp;

                    }
                    break;

            }//end switch


            DrawLine(origin, last, Color.white);

            
        }

        public void PrintText(Vector3 pos, Color cc, string text)
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = cc;

            UnityEditor.Handles.BeginGUI();
            UnityEditor.Handles.Label(pos, text, style);
            UnityEditor.Handles.EndGUI();
#endif
        }

        public void DrawLine(Vector3 start, Vector3 end, Color cc)
        {
#if UNITY_EDITOR
            Gizmos.color = cc;
            Gizmos.DrawLine(start, end);
#endif
        }
    }
}

