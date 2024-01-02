using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace UnityEditor
{
    /// <summary>
    /// The Editor for a RuleTile_Custom.
    /// </summary>
    [CustomEditor(typeof(RuleTile_Custom), true)]
    [CanEditMultipleObjects]
    //[CustomEditor(typeof(RuleTile_Custom))]
    public class RuleTile_Custom_Editor : Editor
    {
        private const string s_XIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";
        private const string s_Arrow0 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPzZExDoQwDATzE4oU4QXXcgUFj+YxtETwgpMwXuFcwMFSRMVKKwzZcWzhiMg91jtg34XIntkre5EaT7yjjhI9pOD5Mw5k2X/DdUwFr3cQ7Pu23E/BiwXyWSOxrNqx+ewnsayam5OLBtbOGPUM/r93YZL4/dhpR/amwByGFBz170gNChA6w5bQQMqramBTgJ+Z3A58WuWejPCaHQAAAABJRU5ErkJggg==";
        private const string s_Arrow1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPxYzBDYAgEATpxYcd+PVr0fZ2siZrjmMhFz6STIiDs8XMlpEyi5RkO/d66TcgJUB43JfNBqRkSEYDnYjhbKD5GIUkDqRDwoH3+NgTAw+bL/aoOP4DOgH+iwECEt+IlFmkzGHlAYKAWF9R8zUnAAAAAElFTkSuQmCC";
        private const string s_Arrow2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAC0SURBVDhPjVE5EsIwDMxPKFKYF9CagoJH8xhaMskLmEGsjOSRkBzYmU2s9a58TUQUmCH1BWEHweuKP+D8tphrWcAHuIGrjPnPNY8X2+DzEWE+FzrdrkNyg2YGNNfRGlyOaZDJOxBrDhgOowaYW8UW0Vau5ZkFmXbbDr+CzOHKmLinAXMEePyZ9dZkZR+s5QX2O8DY3zZ/sgYcdDqeEVp8516o0QQV1qeMwg6C91toYoLoo+kNt/tpKQEVvFQAAAAASUVORK5CYII=";
        private const string s_Arrow3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAB2SURBVDhPzY1LCoAwEEPnLi48gW5d6p31bH5SMhp0Cq0g+CCLxrzRPqMZ2pRqKG4IqzJc7JepTlbRZXYpWTg4RZE1XAso8VHFKNhQuTjKtZvHUNCEMogO4K3BhvMn9wP4EzoPZ3n0AGTW5fiBVzLAAYTP32C2Ay3agtu9V/9PAAAAAElFTkSuQmCC";
        private const string s_Arrow5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPnY3BCYBADASvFx924NevRdvbyoLBmNuDJQMDGjNxAFhK1DyUQ9fvobCdO+j7+sOKj/uSB+xYHZAxl7IR1wNTXJeVcaAVU+614uWfCT9mVUhknMlxDokd15BYsQrJFHeUQ0+MB5ErsPi/6hO1AAAAAElFTkSuQmCC";
        private const string s_Arrow6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACaSURBVDhPxZExEkAwEEVzE4UiTqClUDi0w2hlOIEZsV82xCZmQuPPfFn8t1mirLWf7S5flQOXjd64vCuEKWTKVt+6AayH3tIa7yLg6Qh2FcKFB72jBgJeziA1CMHzeaNHjkfwnAK86f3KUafU2ClHIJSzs/8HHLv09M3SaMCxS7ljw/IYJWzQABOQZ66x4h614ahTCL/WT7BSO51b5Z5hSx88AAAAAElFTkSuQmCC";
        private const string s_Arrow7 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABQSURBVDhPYxh8QNle/T8U/4MKEQdAmsz2eICx6W530gygr2aQBmSMphkZYxqErAEXxusKfAYQ7XyyNMIAsgEkaYQBkAFkaYQBsjXSGDAwAAD193z4luKPrAAAAABJRU5ErkJggg==";
        private const string s_Arrow8 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPxZE9DoAwCIW9iUOHegJXHRw8tIdx1egJTMSHAeMPaHSR5KVQ+KCkCRF91mdz4VDEWVzXTBgg5U1N5wahjHzXS3iFFVRxAygNVaZxJ6VHGIl2D6oUXP0ijlJuTp724FnID1Lq7uw2QM5+thoKth0N+GGyA7IA3+yM77Ag1e2zkey5gCdAg/h8csy+/89v7E+YkgUntOWeVt2SfAAAAABJRU5ErkJggg==";
        private const string s_MirrorX = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG1JREFUOE+lj9ENwCAIRB2IFdyRfRiuDSaXAF4MrR9P5eRhHGb2Gxp2oaEjIovTXSrAnPNx6hlgyCZ7o6omOdYOldGIZhAziEmOTSfigLV0RYAB9y9f/7kO8L3WUaQyhCgz0dmCL9CwCw172HgBeyG6oloC8fAAAAAASUVORK5CYII=";
        private const string s_MirrorY = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG9JREFUOE+djckNACEMAykoLdAjHbPyw1IOJ0L7mAejjFlm9hspyd77Kk+kBAjPOXcakJIh6QaKyOE0EB5dSPJAiUmOiL8PMVGxugsP/0OOib8vsY8yYwy6gRyC8CB5QIWgCMKBLgRSkikEUr5h6wOPWfMoCYILdgAAAABJRU5ErkJggg==";
        private const string s_MirrorXY = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAHkSURBVDhPrVJLSwJRFJ4cdXwjPlrVJly1kB62cpEguElXKgYKIpaC+EIEEfGxLqI/UES1KaJlEdGmRY9ltCsIWrUJatGm0eZO3xkHIsJdH3zce+ec75z5zr3cf2MMmLdYLA/BYFA2mUyPOPvwnR+GR4PXaDQLLpfrKpVKSb1eT6bV6XTeocAS4sIw7S804BzEZ4IgsGq1ykhcr9dlj8czwPdbxJdBMyX/As/zLiz74Ar2J9lsVulcKpUYut5DnEbsHFwEx8AhtFqtGViD6BOc1ul0B5lMRhGXy2Wm1+ufkBOE/2fsL1FsQpXCiCAcQiAlk0kJRZjf7+9TRxI3Gg0WCoW+IpGISHHERBS5UKUch8n2K5WK3O125VqtpqydTkdZie12W261WjIVo73b7RZVKccZDIZ1q9XaT6fTLB6PD9BFKhQKjITFYpGFw+FBNBpVOgcCARH516pUGZYZXk5R4B3efLBxDM9f1CkWi/WR3ICtGVh6Rd4NPE+p0iEgmkSRLRoMEjYhHpA4kUiIOO8iZRU8AmnadK2/QOOfhnjPZrO95fN5Zdq5XE5yOBwvuKoNxGfBkQ8FzXkPprnj9Xrfm82mDI8fsLON3x5H/Od+RwHdLfDds9vtn0aj8QoF6QH9JzjuG3acpxmu1RgPAAAAAElFTkSuQmCC";
        private const string s_Rotated = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAHdJREFUOE+djssNwCAMQxmIFdgx+2S4Vj4YxWlQgcOT8nuG5u5C732Sd3lfLlmPMR4QhXgrTQaimUlA3EtD+CJlBuQ7aUAUMjEAv9gWCQNEPhHJUkYfZ1kEpcxDzioRzGIlr0Qwi0r+Q5rTgM+AAVcygHgt7+HtBZs/2QVWP8ahAAAAAElFTkSuQmCC";
        private const string s_RotatedMirror = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAApklEQVQoFY2SMRaAIAxDwefknVg8uIt3ckVSKdYSnjBAi/lprcaUUphZ+3WGY3u1yJcJMBdNtqAyM3BAFRgohBNmUzDEzIDCVQgGK2rL1gAxhatY3vXh+U7hIs2uOqUZ7EGfN6O1RU/wEf5VX4zgAzpTSessIhL5VDrJkrepitJtFtRHvm0YtA6MMfRSUUGcbGC+A0AdOIJx7w1w1y1WWX/FYUV1uQFvVjvOTYh+rAAAAABJRU5ErkJggg==";
        private const string s_Fixed = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMjHxIGmVAAAA50lEQVQ4T51Ruw6CQBCkwBYKWkIgQAs9gfgCvgb4BML/qWBM9Bdo9QPIuVOQ3JIzosVkc7Mzty9NCPE3lORaKMm1YA/LsnTXdbdhGJ6iKHoVRTEi+r4/OI6zN01Tl/XM7HneLsuyW13XU9u2ous6gYh3kiR327YPsp6ZgyDom6aZYFqiqqqJ8mdZz8xoca64BHjkZT0zY0aVcQbysp6Z4zj+Vvkp65mZttxjOSozdkEzD7KemekcxzRNHxDOHSDiQ/DIy3pmpjtuSJBThStGKMtyRKSOLnSm3DCMz3f+FUpyLZTkOgjtDSWORSDbpbmNAAAAAElFTkSuQmCC";

        private static readonly string k_UndoName = L10n.Tr("Change RuleTile");

        private static Texture2D[] s_Arrows;

        /// <summary>
        /// Array of arrow textures used for marking positions for Rule matches
        /// </summary>
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

        private static Texture2D[] s_AutoTransforms;
        /// <summary>
        /// Arrays of textures used for marking transform Rule matches
        /// </summary>
        public static Texture2D[] autoTransforms
        {
            get
            {
                if (s_AutoTransforms == null)
                {
                    s_AutoTransforms = new Texture2D[6];
                    s_AutoTransforms[0] = Base64ToTexture(s_Rotated);
                    s_AutoTransforms[1] = Base64ToTexture(s_MirrorX);
                    s_AutoTransforms[2] = Base64ToTexture(s_MirrorY);
                    s_AutoTransforms[3] = Base64ToTexture(s_Fixed);
                    s_AutoTransforms[4] = Base64ToTexture(s_MirrorXY);
                    s_AutoTransforms[5] = Base64ToTexture(s_RotatedMirror);
                }
                return s_AutoTransforms;
            }
        }

        private static class Styles
        {
            public static readonly GUIContent defaultSprite = EditorGUIUtility.TrTextContent("Default Sprite"
                , "The default Sprite set when creating a new Rule.");
            public static readonly GUIContent defaultGameObject = EditorGUIUtility.TrTextContent("Default GameObject"
                , "The default GameObject set when creating a new Rule.");
            public static readonly GUIContent defaultCollider = EditorGUIUtility.TrTextContent("Default Collider"
                , "The default Collider Type set when creating a new Rule.");

            public static readonly GUIContent emptyRuleTileInfo =
                EditorGUIUtility.TrTextContent(
                    "Drag Sprite or Sprite Texture assets \n" +
                    " to start creating a Rule Tile.");

            public static readonly GUIContent extendNeighbor = EditorGUIUtility.TrTextContent("Extend Neighbor"
                , "Enabling this allows you to increase the range of neighbors beyond the 3x3 box.");

            public static readonly GUIContent numberOfTilingRules = EditorGUIUtility.TrTextContent(
                "Number of Tiling Rules"
                , "Change this to adjust of the number of tiling rules.");

            public static readonly GUIContent tilingRules = EditorGUIUtility.TrTextContent("Tiling Rules");
            public static readonly GUIContent tilingRulesGameObject = EditorGUIUtility.TrTextContent("GameObject"
                , "The GameObject for the Tile which fits this Rule.");
            public static readonly GUIContent tilingRulesCollider = EditorGUIUtility.TrTextContent("Collider"
                , "The Collider Type for the Tile which fits this Rule");
            public static readonly GUIContent tilingRulesOutput = EditorGUIUtility.TrTextContent("Output"
                , "The Output for the Tile which fits this Rule. Each Output type has its own properties.");

            public static readonly GUIContent tilingRulesNoise = EditorGUIUtility.TrTextContent("Noise"
                , "The Perlin noise factor when placing the Tile.");
            public static readonly GUIContent tilingRulesShuffle = EditorGUIUtility.TrTextContent("Shuffle"
                , "The randomized transform given to the Tile when placing it.");
            public static readonly GUIContent tilingRulesRandomSize = EditorGUIUtility.TrTextContent("Size"
                , "The number of Sprites to randomize from.");

            public static readonly GUIContent tilingRulesMinSpeed = EditorGUIUtility.TrTextContent("Min Speed"
                , "The minimum speed at which the animation is played.");
            public static readonly GUIContent tilingRulesMaxSpeed = EditorGUIUtility.TrTextContent("Max Speed"
                , "The maximum speed at which the animation is played.");
            public static readonly GUIContent tilingRulesAnimationSize = EditorGUIUtility.TrTextContent("Size"
                , "The number of Sprites in the animation.");

            public static readonly GUIStyle extendNeighborsLightStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                normal = new GUIStyleState()
                {
                    textColor = Color.black
                }
            };

            public static readonly GUIStyle extendNeighborsDarkStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };
        }

        /// <summary>
        /// The RuleTile being edited
        /// </summary>
        public RuleTile_Custom tile => target as RuleTile_Custom;

        /// <summary>
        /// List of Sprites for Drag and Drop
        /// </summary>
        private List<Sprite> dragAndDropSprites;

        /// <summary>
        /// Reorderable list for Rules
        /// </summary>
        private ReorderableList m_ReorderableList;
        /// <summary>
        /// Whether the RuleTile can extend its neighbors beyond directly adjacent ones
        /// </summary>
        public bool extendNeighbor;

        /// <summary>
        /// Preview Utility for rendering previews
        /// </summary>
        public PreviewRenderUtility m_PreviewUtility;
        /// <summary>
        /// Grid for rendering previews
        /// </summary>
        public Grid m_PreviewGrid;
        /// <summary>
        /// List of Tilemaps for rendering previews
        /// </summary>
        public List<Tilemap> m_PreviewTilemaps;
        /// <summary>
        /// List of TilemapRenderers for rendering previews
        /// </summary>
        public List<TilemapRenderer> m_PreviewTilemapRenderers;

        /// <summary>
        /// Default height for a Rule Element
        /// </summary>
        public const float k_DefaultElementHeight = 48f;
        /// <summary>
        /// Padding between Rule Elements
        /// </summary>
        public const float k_PaddingBetweenRules = 8f;
        /// <summary>
        /// Single line height
        /// </summary>
        public const float k_SingleLineHeight = 18f;
        /// <summary>
        /// Width for labels
        /// </summary>
        public const float k_LabelWidth = 80f;

        private SerializedProperty m_TilingRules;

        private MethodInfo m_ClearCacheMethod;

        /// <summary>
        /// OnEnable for the RuleTileEditor
        /// </summary>
        public virtual void OnEnable()
        {
            m_ReorderableList = new ReorderableList(tile != null ? tile.m_TilingRules : null, typeof(RuleTile_Custom.TilingRule), true, true, true, true);
            m_ReorderableList.drawHeaderCallback = OnDrawHeader;
            m_ReorderableList.drawElementCallback = OnDrawElement;
            m_ReorderableList.elementHeightCallback = GetElementHeight;
            m_ReorderableList.onChangedCallback = ListUpdated;
            m_ReorderableList.onAddDropdownCallback = OnAddDropdownElement;

            // Required to adjust element height changes
            var rolType = GetType("UnityEditorInternal.ReorderableList");
            if (rolType != null)
            {
                // ClearCache was changed to InvalidateCache in newer versions of Unity.
                // To maintain backwards compatibility, we will attempt to retrieve each method in order
                m_ClearCacheMethod = rolType.GetMethod("InvalidateCache", BindingFlags.Instance | BindingFlags.NonPublic);
                if (m_ClearCacheMethod == null)
                    m_ClearCacheMethod = rolType.GetMethod("ClearCache", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            m_TilingRules = serializedObject.FindProperty("m_TilingRules");
        }

        /// <summary>
        /// OnDisable for the RuleTileEditor
        /// </summary>
        public virtual void OnDisable()
        {
            DestroyPreview();
        }

        private void UpdateTilingRuleIds()
        {
            var existingIdSet = new HashSet<int>();
            var usedIdSet = new HashSet<int>();
            foreach (var rule in tile.m_TilingRules)
            {
                existingIdSet.Add(rule.m_Id);
            }
            foreach (var rule in tile.m_TilingRules)
            {
                if (usedIdSet.Contains(rule.m_Id))
                {
                    while (existingIdSet.Contains(rule.m_Id))
                        rule.m_Id++;
                    existingIdSet.Add(rule.m_Id);
                }
                usedIdSet.Add(rule.m_Id);
            }
        }

        /// <summary>
        /// Get the GUI bounds for a Rule.
        /// </summary>
        /// <param name="bounds">Cell bounds of the Rule.</param>
        /// <param name="rule">Rule to get GUI bounds for.</param>
        /// <returns>The GUI bounds for a rule.</returns>
        public virtual BoundsInt GetRuleGUIBounds(BoundsInt bounds, RuleTile_Custom.TilingRule rule)
        {
            if (extendNeighbor)
            {
                bounds.xMin--;
                bounds.yMin--;
                bounds.xMax++;
                bounds.yMax++;
            }
            bounds.xMin = Mathf.Min(bounds.xMin, -1);
            bounds.yMin = Mathf.Min(bounds.yMin, -1);
            bounds.xMax = Mathf.Max(bounds.xMax, 2);
            bounds.yMax = Mathf.Max(bounds.yMax, 2);
            return bounds;
        }

        /// <summary>
        /// Callback when the Rule list is updated
        /// </summary>
        /// <param name="list">Reorderable list for Rules</param>
        public void ListUpdated(ReorderableList list)
        {
            UpdateTilingRuleIds();
        }

        private float GetElementHeight(int index)
        {
            RuleTile_Custom.TilingRule rule = tile.m_TilingRules[index];
            return GetElementHeight(rule);
        }

        /// <summary>
        /// Gets the GUI element height for a TilingRule 
        /// </summary>
        /// <param name="rule">Rule to get height for</param>
        /// <returns>GUI element height for a TilingRule</returns>
        public float GetElementHeight(RuleTile_Custom.TilingRule rule)
        {
            BoundsInt bounds = GetRuleGUIBounds(rule.GetBounds(), rule);

            float inspectorHeight = GetElementHeight(rule as RuleTile_Custom.TilingRuleOutput);
            float matrixHeight = GetMatrixSize(bounds).y + 10f;

            return Mathf.Max(inspectorHeight, matrixHeight);
        }

        /// <summary>
        /// Gets the GUI element height for a TilingRuleOutput 
        /// </summary>
        /// <param name="rule">Rule to get height for</param>
        /// <returns>GUI element height for a TilingRuleOutput </returns>
        public float GetElementHeight(RuleTile_Custom.TilingRuleOutput rule)
        {
            float inspectorHeight = k_DefaultElementHeight + k_PaddingBetweenRules;

            switch (rule.m_Output)
            {
                case RuleTile_Custom.TilingRuleOutput.OutputSprite.Random:
                case RuleTile_Custom.TilingRuleOutput.OutputSprite.Animation:
                    inspectorHeight = k_DefaultElementHeight + k_SingleLineHeight * (rule.m_Sprites.Length + 3) + k_PaddingBetweenRules;
                    break;
            }

            return inspectorHeight;
        }

        /// <summary>
        /// Gets the GUI matrix size for a Rule of a RuleTile
        /// </summary>
        /// <param name="bounds">Cell bounds of the Rule.</param>
        /// <returns>Returns the GUI matrix size for a Rule of a RuleTile_Custom.</returns>
        public virtual Vector2 GetMatrixSize(BoundsInt bounds)
        {
            return new Vector2(bounds.size.x * k_SingleLineHeight, bounds.size.y * k_SingleLineHeight);
        }

        /// <summary>
        /// Draws the Rule element for the Rule list
        /// </summary>
        /// <param name="rect">Rect to draw the Rule Element in</param>
        /// <param name="index">Index of the Rule Element to draw</param>
        /// <param name="isactive">Whether the Rule Element is active</param>
        /// <param name="isfocused">Whether the Rule Element is focused</param>
        protected virtual void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            RuleTile_Custom.TilingRule rule = tile.m_TilingRules[index];
            BoundsInt bounds = GetRuleGUIBounds(rule.GetBounds(), rule);

            float yPos = rect.yMin + 2f;
            float height = rect.height - k_PaddingBetweenRules;
            Vector2 matrixSize = GetMatrixSize(bounds);

            Rect spriteRect = new Rect(rect.xMax - k_DefaultElementHeight - 5f, yPos, k_DefaultElementHeight, k_DefaultElementHeight);
            Rect matrixRect = new Rect(rect.xMax - matrixSize.x - spriteRect.width - 10f, yPos, matrixSize.x, matrixSize.y);
            Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixSize.x - spriteRect.width - 20f, height);

            RuleInspectorOnGUI(inspectorRect, rule);
            RuleMatrixOnGUI(tile, matrixRect, bounds, rule);
            RuleMatrixOnGUI_Direction(tile, inspectorRect, rule);
            SpriteOnGUI(spriteRect, rule);
        }

        private void OnAddElement(object obj)
        {
            var list = obj as ReorderableList;
            RuleTile_Custom.TilingRule rule = new RuleTile_Custom.TilingRule();
            rule.m_Output = RuleTile_Custom.TilingRuleOutput.OutputSprite.Single;
            rule.m_Sprites[0] = tile.m_DefaultSprite;
            rule.m_GameObject = tile.m_DefaultGameObject;
            rule.m_ColliderType = tile.m_DefaultColliderType;

            var count = m_TilingRules.arraySize;
            ResizeRuleTileList(count + 1);

            if (list.index == -1 || list.index >= list.count)
                tile.m_TilingRules[count] = rule;
            else
            {
                tile.m_TilingRules.Insert(list.index + 1, rule);
                tile.m_TilingRules.RemoveAt(count + 1);
                if (list.IsSelected(list.index))
                    list.index += 1;
            }
            UpdateTilingRuleIds();
        }

        private void OnDuplicateElement(object obj)
        {
            var list = obj as ReorderableList;
            if (list.index < 0 || list.index >= tile.m_TilingRules.Count)
                return;

            var copyRule = tile.m_TilingRules[list.index];
            var rule = copyRule.Clone();

            var count = m_TilingRules.arraySize;
            ResizeRuleTileList(count + 1);

            tile.m_TilingRules.Insert(list.index + 1, rule);
            tile.m_TilingRules.RemoveAt(count + 1);
            if (list.IsSelected(list.index))
                list.index += 1;
            UpdateTilingRuleIds();
        }

        private void OnAddDropdownElement(Rect rect, ReorderableList list)
        {
            if (0 <= list.index && list.index < tile.m_TilingRules.Count && list.IsSelected(list.index))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(EditorGUIUtility.TrTextContent("Add"), false, OnAddElement, list);
                menu.AddItem(EditorGUIUtility.TrTextContent("Duplicate"), false, OnDuplicateElement, list);
                menu.DropDown(rect);
            }
            else
            {
                OnAddElement(list);
            }
        }

        /// <summary>
        /// Saves any changes to the RuleTile
        /// </summary>
        public void SaveTile()
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();

            //UpdateAffectedOverrideTiles(tile); //20231102 chamto - overrideTile 관련 처리 주석함, 추후 필요시 파일 추가해야함 
        }

        /// <summary>
        /// Updates all RuleOverrideTiles which override the given RUleTile
        /// </summary>
        /// <param name="target">RuleTile which has been updated</param>
        //public static void UpdateAffectedOverrideTiles(RuleTile_Custom target)
        //{
        //    List<RuleOverrideTile> overrideTiles = FindAffectedOverrideTiles(target);
        //    if (overrideTiles != null)
        //    {
        //        foreach (var overrideTile in overrideTiles)
        //        {
        //            Undo.RegisterCompleteObjectUndo(overrideTile, k_UndoName);
        //            Undo.RecordObject(overrideTile.m_InstanceTile, k_UndoName);
        //            overrideTile.Override();
        //            UpdateAffectedOverrideTiles(overrideTile.m_InstanceTile);
        //            EditorUtility.SetDirty(overrideTile);
        //        }
        //    }
        //}

        /// <summary>
        /// Gets all RuleOverrideTiles which override the given RuleTile
        /// </summary>
        /// <param name="target">RuleTile which has been updated</param>
        /// <returns>A list of RuleOverrideTiles which override the given RuleTile</returns>
        public static List<RuleOverrideTile> FindAffectedOverrideTiles(RuleTile_Custom target)
        {
            List<RuleOverrideTile> overrideTiles = new List<RuleOverrideTile>();

            string[] overrideTileGuids = AssetDatabase.FindAssets("t:" + typeof(RuleOverrideTile).Name);
            foreach (string overrideTileGuid in overrideTileGuids)
            {
                string overrideTilePath = AssetDatabase.GUIDToAssetPath(overrideTileGuid);
                RuleOverrideTile overrideTile = AssetDatabase.LoadAssetAtPath<RuleOverrideTile>(overrideTilePath);
                if (overrideTile.m_Tile == target)
                {
                    overrideTiles.Add(overrideTile);
                }
            }

            return overrideTiles;
        }

        /// <summary>
        /// Draws the header for the Rule list
        /// </summary>
        /// <param name="rect">GUI Rect to draw the header at</param>
        public void OnDrawHeader(Rect rect)
        {
            GUI.Label(rect, Styles.tilingRules);

            var toggleRect = new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height);

            var style = EditorGUIUtility.isProSkin ? Styles.extendNeighborsDarkStyle : Styles.extendNeighborsLightStyle;
            var extendSize = style.CalcSize(Styles.extendNeighbor);
            var toggleWidth = toggleRect.width + extendSize.x + 5f;
            var toggleLabelRect = new Rect(rect.x + rect.width - toggleWidth, rect.y, toggleWidth, rect.height);

            EditorGUI.BeginChangeCheck();
            extendNeighbor = EditorGUI.Toggle(toggleRect, extendNeighbor);
            EditorGUI.LabelField(toggleLabelRect, Styles.extendNeighbor, style);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_ClearCacheMethod != null)
                    m_ClearCacheMethod.Invoke(m_ReorderableList, null);
            }
        }

        /// <summary>
        /// Draws the Inspector GUI for the RuleTileEditor
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(target, k_UndoName);

            EditorGUI.BeginChangeCheck();

            tile.m_DefaultSprite = EditorGUILayout.ObjectField(Styles.defaultSprite, tile.m_DefaultSprite, typeof(Sprite), false) as Sprite;
            tile.m_DefaultGameObject = EditorGUILayout.ObjectField(Styles.defaultGameObject, tile.m_DefaultGameObject, typeof(GameObject), false) as GameObject;
            tile.m_DefaultColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup(Styles.defaultCollider, tile.m_DefaultColliderType);

            DrawCustomFields(false);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            int count = EditorGUILayout.DelayedIntField(Styles.numberOfTilingRules, tile.m_TilingRules?.Count ?? 0);
            if (count < 0)
                count = 0;
            if (EditorGUI.EndChangeCheck())
                ResizeRuleTileList(count);

            if (count == 0)
            {
                Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 5);
                HandleDragAndDrop(rect);
                EditorGUI.DrawRect(rect, dragAndDropActive && rect.Contains(Event.current.mousePosition) ? Color.white : Color.black);
                var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
                EditorGUI.DrawRect(innerRect, EditorGUIUtility.isProSkin
                    ? (Color)new Color32(56, 56, 56, 255)
                    : (Color)new Color32(194, 194, 194, 255));
                DisplayClipboardText(Styles.emptyRuleTileInfo, rect);
                GUILayout.Space(rect.height);
                EditorGUILayout.Space();
            }

            if (m_ReorderableList != null)
                m_ReorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
                SaveTile();

            GUILayout.Space(k_DefaultElementHeight);
        }

        private void ResizeRuleTileList(int count)
        {
            if (m_TilingRules.arraySize == count)
                return;

            var isEmpty = m_TilingRules.arraySize == 0;
            m_TilingRules.arraySize = count;
            serializedObject.ApplyModifiedProperties();
            if (isEmpty)
            {
                for (int i = 0; i < count; ++i)
                    tile.m_TilingRules[i] = new RuleTile_Custom.TilingRule();
            }
            UpdateTilingRuleIds();
        }

        /// <summary>
        /// Draw editor fields for custom properties for the RuleTile
        /// </summary>
        /// <param name="isOverrideInstance">Whether override fields are drawn</param>
        public void DrawCustomFields(bool isOverrideInstance)
        {
            var customFields = tile.GetCustomFields(isOverrideInstance);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            foreach (var field in customFields)
            {
                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                    EditorGUILayout.PropertyField(property, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                DestroyPreview();
                CreatePreview();
            }

        }

        /// <summary>
        /// Gets the index for a Rule with the RuleTile to display an arrow.
        /// </summary>
        /// <param name="position">The relative position of the arrow from the center.</param>
        /// <returns>Returns the index for a Rule with the RuleTile to display an arrow.</returns>
        public virtual int GetArrowIndex(Vector3Int position)
        {
            if (Mathf.Abs(position.x) == Mathf.Abs(position.y))
            {
                if (position.x < 0 && position.y > 0)
                    return 0;
                else if (position.x > 0 && position.y > 0)
                    return 2;
                else if (position.x < 0 && position.y < 0)
                    return 6;
                else if (position.x > 0 && position.y < 0)
                    return 8;
            }
            else if (Mathf.Abs(position.x) > Mathf.Abs(position.y))
            {
                if (position.x > 0)
                    return 5;
                else
                    return 3;
            }
            else
            {
                if (position.y > 0)
                    return 1;
                else
                    return 7;
            }
            return -1;
        }

        /// <summary>
        /// Draws a neighbor matching rule
        /// </summary>
        /// <param name="rect">Rect to draw on</param>
        /// <param name="position">The relative position of the arrow from the center</param>
        /// <param name="neighbor">The index to the neighbor matching criteria</param>
        public virtual void RuleOnGUI(Rect rect, Vector3Int position, int neighbor)
        {
            switch (neighbor)
            {
                case RuleTile_Custom.TilingRuleOutput.Neighbor.This:
                    GUI.DrawTexture(rect, arrows[GetArrowIndex(position)]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Neighbor.NotThis:
                    GUI.DrawTexture(rect, arrows[9]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Neighbor.Specifier_This:
                    //반으로 줄여서 출력 [반위: 지정자명 , 반아래: 방향GUI]
                    rect.height -= 5;
                    rect.y += 6;
                    GUI.DrawTexture(rect, arrows[GetArrowIndex(position)]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Neighbor.Specifier_NotThis:
                    //반으로 줄여서 출력 [반위: 지정자명 , 반아래: 방향GUI]
                    rect.height -= 5;
                    rect.y += 6;
                    GUI.DrawTexture(rect, arrows[9]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Neighbor.Adjacent:
                    {
                        var style1 = new GUIStyle();
                        style1.normal.textColor = Color.yellow;
                        style1.fontSize = 8;
                        GUI.Label(rect, "adj", style1);

                        rect.height -= 5;
                        rect.y += 6;
                        GUI.DrawTexture(rect, arrows[GetArrowIndex(position)]);
                        break;
                    }
                default:
                    var style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 10;
                    GUI.Label(rect, neighbor.ToString(), style);
                    break;
            }
        }

        /// <summary>
        /// Draws a tooltip for the neighbor matching rule
        /// </summary>
        /// <param name="rect">Rect to draw on</param>
        /// <param name="neighbor">The index to the neighbor matching criteria</param>
        public void RuleTooltipOnGUI(Rect rect, int neighbor)
        {
            var allConsts = tile.m_NeighborType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var c in allConsts)
            {
                if ((int)c.GetValue(null) == neighbor)
                {
                    GUI.Label(rect, new GUIContent("", c.Name));
                    break;
                }
            }
        }

        
        //chamto 추가 - 20231111
        public virtual void RuleTextField_OnGUI(RuleTile_Custom.TilingRule tilingRule, Rect rect, Vector3Int position, int neighbor)
        {

            //RuleNeighborUpdate 함수에서 추가가 발생하므로 여기서 따로 처리할 필요 없음 
            //if (false == tilingRule.m_Neighbors_bucket.ContainsKey(position))
            //{
            //    tilingRule.m_Neighbors_bucket.Add(position, new RuleTile_Custom.TilingRule.Neighbor_Bucket());
            //}


            Rect tfRect = rect;
            GUIStyle style = new GUIStyle();
            if (RuleTile_Custom.TilingRuleOutput.Neighbor.Specifier_This == neighbor ||
                RuleTile_Custom.TilingRuleOutput.Neighbor.Specifier_NotThis == neighbor)
            {   //지정자인 경우 : 격자칸의 상단[반]에 텍스트필드가 배치되도록 한다 
                style.fontSize = 8;
                //tfRect.width -= 5;
                tfRect.height -= 5;
                style.normal.textColor = Color.white;

            }else
            {   //지정자가 아닐 경우 (일반) : 텍스트필드가 안보이게 크기를 줄여 배치한다
                //안보이는 텍스트필드를 배치하는 이유 : 텍스트필드의 커서가 위치중, 텍스트필드가 출력중단될 경우
                //"텍스트필드의 문자열을 가지고 인접 텍스트필드로 입력문자열을 가져가는 문제가 있음
                //이를 해결하기 위해 보이지 않는 텍스트필드를 추가함
                //완벽한 해결책이 아님 : 모든 격자칸에 안보이는 텍스트필드를 만들어야 문제가 발생안함 ,
                //   텍스트필드가 처음 생성될때 여전히 문제 발생함 , 유니티 자체 문제임
                style.fontSize = 1;
                tfRect.width -= 20;
                tfRect.height -= 20;
            }
            


            EditorGUI.BeginChangeCheck();
            var bucket = tilingRule.m_Neighbors_bucket[position];
            string newID = EditorGUI.DelayedTextField(tfRect, bucket._specifier, style);
            //string newID = EditorGUI.TextField(tfRect, tilingRule.m_Neighbors_Specifier[position], style);
            if (EditorGUI.EndChangeCheck())
            {
                //tilingRule.m_Neighbors_bucket[position] = newID.Substring(0, Mathf.Min(2, newID.Length)); //두글자로 제한한다
                tilingRule.m_Neighbors_bucket[position]._specifier = newID.Substring(0, Mathf.Min(2, newID.Length)); //두글자로 제한한다
            }
        }

        /// <summary>
        /// Draws a transform matching rule
        /// </summary>
        /// <param name="rect">Rect to draw on</param>
        /// <param name="ruleTransform">The transform matching criteria</param>
        public virtual void RuleTransformOnGUI(Rect rect, RuleTile_Custom.TilingRuleOutput.Transform ruleTransform)
        {
            switch (ruleTransform)
            {
                case RuleTile_Custom.TilingRuleOutput.Transform.Rotated:
                    GUI.DrawTexture(rect, autoTransforms[0]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Transform.MirrorX:
                    GUI.DrawTexture(rect, autoTransforms[1]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Transform.MirrorY:
                    GUI.DrawTexture(rect, autoTransforms[2]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Transform.Fixed:
                    GUI.DrawTexture(rect, autoTransforms[3]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Transform.MirrorXY:
                    GUI.DrawTexture(rect, autoTransforms[4]);
                    break;
                case RuleTile_Custom.TilingRuleOutput.Transform.RotatedMirror:
                    GUI.DrawTexture(rect, autoTransforms[5]);
                    break;
            }
            GUI.Label(rect, new GUIContent("", ruleTransform.ToString()));
        }

        /// <summary>
        /// Handles a neighbor matching Rule update from user mouse input
        /// </summary>
        /// <param name="rect">Rect containing neighbor matching Rule GUI</param>
        /// <param name="tilingRule">Tiling Rule to update neighbor matching rule</param>
        /// <param name="neighbors">A dictionary of neighbors</param>
        /// <param name="position">The relative position of the neighbor matching Rule</param>
        //public void RuleNeighborUpdate(Rect rect, RuleTile_Custom.TilingRule tilingRule, Dictionary<Vector3Int, int> neighbors, Vector3Int position)
        public void RuleNeighborUpdate(Rect rect, RuleTile_Custom.TilingRule tilingRule, Vector3Int position)
        {
            var neighbors = tilingRule.m_Neighbors_bucket;
            if (Event.current.type == EventType.MouseDown && ContainsMousePosition(rect))
            {
                var allConsts = tile.m_NeighborType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var neighborConsts = allConsts.Select(c => (int)c.GetValue(null)).ToList();
                neighborConsts.Sort();

                if (neighbors.ContainsKey(position))
                {
                    int oldIndex = neighborConsts.IndexOf(neighbors[position]._kind);
                    int newIndex = oldIndex + GetMouseChange();
                    //int newIndex = oldIndex  -1;
                    if (newIndex >= 0 && newIndex < neighborConsts.Count)
                    {
                        newIndex = (int)Mathf.Repeat(newIndex, neighborConsts.Count);
                        neighbors[position]._kind = neighborConsts[newIndex];
                    }
                    else
                    {
                        neighbors.Remove(position);
                    }
                }
                else
                {
                    var bucket = new RuleTile_Custom.TilingRule.Neighbor_Bucket();
                    bucket._posotion = position;
                    bucket._kind = neighborConsts[GetMouseChange() == 1 ? 0 : (neighborConsts.Count - 1)];
                    neighbors.Add(position, bucket);
                    //neighbors.Add(position, neighborConsts[-1 == 1 ? 0 : (neighborConsts.Count - 1)]);
                }
                //tilingRule.ApplyNeighbors(neighbors);

                GUI.changed = true;
                Event.current.Use();
            }
        }

        /// <summary>
        /// Handles a transform matching Rule update from user mouse input
        /// </summary>
        /// <param name="rect">Rect containing transform matching Rule GUI</param>
        /// <param name="tilingRule">Tiling Rule to update transform matching rule</param>
        public void RuleTransformUpdate(Rect rect, RuleTile_Custom.TilingRule tilingRule)
        {
            if (Event.current.type == EventType.MouseDown && ContainsMousePosition(rect))
            {
                tilingRule.m_RuleTransform = (RuleTile_Custom.TilingRuleOutput.Transform)(int)Mathf.Repeat((int)tilingRule.m_RuleTransform + GetMouseChange(), Enum.GetValues(typeof(RuleTile_Custom.TilingRule.Transform)).Length);
                GUI.changed = true;
                Event.current.Use();
            }
        }

        /// <summary>
        /// Determines the current mouse position is within the given Rect.
        /// </summary>
        /// <param name="rect">Rect to test mouse position for.</param>
        /// <returns>True if the current mouse position is within the given Rect. False otherwise.</returns>
        public virtual bool ContainsMousePosition(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        /// <summary>
        /// Gets the offset change for a mouse click input
        /// </summary>
        /// <returns>The offset change for a mouse click input</returns>
        public static int GetMouseChange()
        {
            return Event.current.button == 1 ? -1 : 1;
        }


        //chamto add
        public void RuleMatrixOnGUI_Direction(RuleTile_Custom tile, Rect rect, RuleTile_Custom.TilingRule rule)
        {

            Rect r = rect;
            r.x -= 20f;
            r.y += 16f;
            r.width = 20f;
            r.height = 20f;


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
            //s_Arrows[0] = Base64ToTexture(s_Arrow0); //leftUp ->    4 -> 0
            //s_Arrows[1] = Base64ToTexture(s_Arrow1); //up ->        3 -> 1
            //s_Arrows[2] = Base64ToTexture(s_Arrow2); //rightUp ->   2 -> 2
            //s_Arrows[3] = Base64ToTexture(s_Arrow3); //left ->      5 -> 3
            //s_Arrows[5] = Base64ToTexture(s_Arrow5); //right ->     1 -> 5
            //s_Arrows[6] = Base64ToTexture(s_Arrow6); //leftDown ->  6 -> 6
            //s_Arrows[7] = Base64ToTexture(s_Arrow7); //down ->      7 -> 7
            //s_Arrows[8] = Base64ToTexture(s_Arrow8); //rightDown -> 8 -> 8
            //s_Arrows[9] = Base64ToTexture(s_XIconString); //        0 -> 9

            int[] tr = new int[] { 9, 5, 2, 1, 0, 3, 6, 7, 8 };
            int trans_index = tr[rule._border_dir];

            if ((int)rule._border_dir < 0) rule._border_dir = 0;

            GUI.DrawTexture(r, RuleTileEditor.arrows[trans_index]);

            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                rule._border_dir = (((int)rule._border_dir + 1) % 9);
                GUI.changed = true;
                Event.current.Use();

                //chamto test
                //rule.m_Neighbors[5] = 1; //인덱스값 5 는 초기 m_NeighborPositions 리스트의 순서와 달라질 수 있다. 위치 와 인덱스값이 묶여있지 않다
                //int ct = 0;
                //foreach (string ne in rule.m_Neighbors_Specifier.Values)
                //{
                //    Debug.Log(ct+"  " +ne);
                //    ct++;
                //}
                
            }

            //----------------------------------------------------------------------------------------

            r.y += 20f;
            r.width += 2f;
            //string newID = EditorGUI.DelayedTextField(r, rule._specifier);
            string newID = EditorGUI.TextField(r, rule._specifier);
            rule._specifier = newID.Substring(0, Mathf.Min(2, newID.Length)); //입력한 글자를 2글자로 제한한다 

            //Debug.Log(rule._border_dir);

        }


        /// <summary>
        /// Draws a Rule Matrix for the given Rule for a RuleTile_Custom.
        /// </summary>
        /// <param name="tile">Tile to draw rule for.</param>
        /// <param name="rect">GUI Rect to draw rule at.</param>
        /// <param name="bounds">Cell bounds of the Rule.</param>
        /// <param name="tilingRule">Rule to draw Rule Matrix for.</param>
        public virtual void RuleMatrixOnGUI(RuleTile_Custom tile, Rect rect, BoundsInt bounds, RuleTile_Custom.TilingRule tilingRule)
        {
            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
            float w = rect.width / bounds.size.x;
            float h = rect.height / bounds.size.y;

            for (int y = 0; y <= bounds.size.y; y++)
            {
                float top = rect.yMin + y * h;
                Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
            }
            for (int x = 0; x <= bounds.size.x; x++)
            {
                float left = rect.xMin + x * w;
                Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
            }
            Handles.color = Color.white;

            //var neighbors = tilingRule.GetNeighbors();
            var neighbors = tilingRule.m_Neighbors_bucket;

            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    Rect r = new Rect(rect.xMin + (x - bounds.xMin) * w, rect.yMin + (-y + bounds.yMax - 1) * h, w - 1, h - 1);
                    RuleMatrixIconOnGUI(tilingRule, pos, r);
                }
            }
        }

        /// <summary>
        /// Draws a Rule Matrix Icon for the given matching Rule for a RuleTile with the given position
        /// </summary>
        /// <param name="tilingRule">Tile to draw rule for.</param>
        /// <param name="neighbors">A dictionary of neighbors</param>
        /// <param name="position">The relative position of the neighbor matching Rule</param>
        /// <param name="rect">GUI Rect to draw icon at</param>
        //public void RuleMatrixIconOnGUI(RuleTile_Custom.TilingRule tilingRule, Dictionary<Vector3Int, int> neighbors, Vector3Int position, Rect rect)
        public void RuleMatrixIconOnGUI(RuleTile_Custom.TilingRule tilingRule, Vector3Int position, Rect rect)
        {

            var neighbors = tilingRule.m_Neighbors_bucket;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (position.x != 0 || position.y != 0)
                {
                    bool isContain = neighbors.ContainsKey(position);
                    if (isContain)
                    {
                        RuleOnGUI(rect, position, neighbors[position]._kind);
                        RuleTooltipOnGUI(rect, neighbors[position]._kind);

                        RuleTextField_OnGUI(tilingRule, rect, position, neighbors[position]._kind); //chamto 지정값 입력필드 출력 

                    }
                    RuleNeighborUpdate(rect, tilingRule, position);
                }
                else
                {
                    RuleTransformOnGUI(rect, tilingRule.m_RuleTransform);
                    RuleTransformUpdate(rect, tilingRule);
                }
                if (check.changed)
                {
                    tile.UpdateNeighborPositions();
                }
            }
        }

        /// <summary>
        /// Draws a Sprite field for the Rule
        /// </summary>
        /// <param name="rect">Rect to draw Sprite Inspector in</param>
        /// <param name="tilingRule">Rule to draw Sprite Inspector for</param>
        public virtual void SpriteOnGUI(Rect rect, RuleTile_Custom.TilingRuleOutput tilingRule)
        {
            tilingRule.m_Sprites[0] = EditorGUI.ObjectField(rect, tilingRule.m_Sprites[0], typeof(Sprite), false) as Sprite;
        }

        /// <summary>
        /// Draws an Inspector for the Rule
        /// </summary>
        /// <param name="rect">Rect to draw Inspector in</param>
        /// <param name="tilingRule">Rule to draw Inspector for</param>
        public void RuleInspectorOnGUI(Rect rect, RuleTile_Custom.TilingRuleOutput tilingRule)
        {
            float y = rect.yMin;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesGameObject);
            tilingRule.m_GameObject = (GameObject)EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), "", tilingRule.m_GameObject, typeof(GameObject), false);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesCollider);
            tilingRule.m_ColliderType = (Tile.ColliderType)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_ColliderType);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesOutput);
            tilingRule.m_Output = (RuleTile_Custom.TilingRuleOutput.OutputSprite)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Output);
            y += k_SingleLineHeight;

            if (tilingRule.m_Output == RuleTile_Custom.TilingRuleOutput.OutputSprite.Animation)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesMinSpeed);
                tilingRule.m_MinAnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MinAnimationSpeed);
                y += k_SingleLineHeight;
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesMaxSpeed);
                tilingRule.m_MaxAnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MaxAnimationSpeed);
                y += k_SingleLineHeight;
            }
            if (tilingRule.m_Output == RuleTile_Custom.TilingRuleOutput.OutputSprite.Random)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesNoise);
                tilingRule.m_PerlinScale = EditorGUI.Slider(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_PerlinScale, 0.001f, 0.999f);
                y += k_SingleLineHeight;

                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.tilingRulesShuffle);
                tilingRule.m_RandomTransform = (RuleTile_Custom.TilingRuleOutput.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RandomTransform);
                y += k_SingleLineHeight;
            }

            if (tilingRule.m_Output != RuleTile_Custom.TilingRuleOutput.OutputSprite.Single)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight)
                    , tilingRule.m_Output == RuleTile_Custom.TilingRuleOutput.OutputSprite.Animation ? Styles.tilingRulesAnimationSize : Styles.tilingRulesRandomSize);
                EditorGUI.BeginChangeCheck();
                int newLength = EditorGUI.DelayedIntField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites.Length);
                if (EditorGUI.EndChangeCheck())
                    Array.Resize(ref tilingRule.m_Sprites, Math.Max(newLength, 1));
                y += k_SingleLineHeight;

                for (int i = 0; i < tilingRule.m_Sprites.Length; i++)
                {
                    tilingRule.m_Sprites[i] = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites[i], typeof(Sprite), false) as Sprite;
                    y += k_SingleLineHeight;
                }
            }
        }

        private void DisplayClipboardText(GUIContent clipboardText, Rect position)
        {
            Color old = GUI.color;
            GUI.color = Color.gray;
            var infoSize = GUI.skin.label.CalcSize(clipboardText);
            Rect rect = new Rect(position.center.x - infoSize.x * .5f
                , position.center.y - infoSize.y * .5f
                , infoSize.x
                , infoSize.y);
            GUI.Label(rect, clipboardText);
            GUI.color = old;
        }

        private bool dragAndDropActive
        {
            get
            {
                return dragAndDropSprites != null
                       && dragAndDropSprites.Count > 0;
            }
        }

        private static List<Sprite> GetSpritesFromTexture(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<Sprite> sprites = new List<Sprite>();

            foreach (Object asset in assets)
            {
                if (asset is Sprite)
                {
                    sprites.Add(asset as Sprite);
                }
            }

            return sprites;
        }

        private static List<Sprite> GetValidSingleSprites(Object[] objects)
        {
            List<Sprite> result = new List<Sprite>();
            foreach (Object obj in objects)
            {
                if (obj is Sprite sprite)
                {
                    result.Add(sprite);
                }
                else if (obj is Texture2D texture2D)
                {
                    List<Sprite> sprites = GetSpritesFromTexture(texture2D);
                    if (sprites.Count > 0)
                    {
                        result.AddRange(sprites);
                    }
                }
            }
            return result;
        }

        private void HandleDragAndDrop(Rect guiRect)
        {
            if (DragAndDrop.objectReferences.Length == 0 || !guiRect.Contains(Event.current.mousePosition))
                return;

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    {
                        dragAndDropSprites = GetValidSingleSprites(DragAndDrop.objectReferences);
                        if (dragAndDropActive)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            Event.current.Use();
                            GUI.changed = true;
                        }
                    }
                    break;
                case EventType.DragPerform:
                    {
                        if (!dragAndDropActive)
                            return;

                        Undo.RegisterCompleteObjectUndo(tile, "Drag and Drop to Rule Tile");
                        ResizeRuleTileList(dragAndDropSprites.Count);
                        for (int i = 0; i < dragAndDropSprites.Count; ++i)
                        {
                            tile.m_TilingRules[i].m_Sprites[0] = dragAndDropSprites[i];
                        }
                        DragAndDropClear();
                        GUI.changed = true;
                        EditorUtility.SetDirty(tile);
                        GUIUtility.ExitGUI();
                    }
                    break;
                case EventType.Repaint:
                    // Handled in Render()
                    break;
            }

            if (Event.current.type == EventType.DragExited ||
                Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                DragAndDropClear();
            }
        }

        private void DragAndDropClear()
        {
            dragAndDropSprites = null;
            DragAndDrop.visualMode = DragAndDropVisualMode.None;
            Event.current.Use();
        }

        /// <summary>
        /// Whether the RuleTile has a preview GUI
        /// </summary>
        /// <returns>True</returns>
        public override bool HasPreviewGUI()
        {
            return true;
        }

        /// <summary>
        /// Draws the preview GUI for the RuleTile
        /// </summary>
        /// <param name="rect">Rect to draw the preview GUI</param>
        /// <param name="background">The GUIStyle of the background for the preview</param>
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (m_PreviewUtility == null)
                CreatePreview();

            if (Event.current.type != EventType.Repaint)
                return;

            m_PreviewUtility.BeginPreview(rect, background);
            m_PreviewUtility.camera.orthographicSize = 2;
            if (rect.height > rect.width)
                m_PreviewUtility.camera.orthographicSize *= rect.height / rect.width;
            m_PreviewUtility.camera.Render();
            m_PreviewUtility.EndAndDrawPreview(rect);
        }

        /// <summary>
        /// Creates a Preview for the RuleTile_Custom.
        /// </summary>
        protected virtual void CreatePreview()
        {
            m_PreviewUtility = new PreviewRenderUtility(true);
            m_PreviewUtility.camera.orthographic = true;
            m_PreviewUtility.camera.orthographicSize = 2;
            m_PreviewUtility.camera.transform.position = new Vector3(0, 0, -10);

            var previewInstance = new GameObject();
            m_PreviewGrid = previewInstance.AddComponent<Grid>();
            m_PreviewUtility.AddSingleGO(previewInstance);

            m_PreviewTilemaps = new List<Tilemap>();
            m_PreviewTilemapRenderers = new List<TilemapRenderer>();

            for (int i = 0; i < 4; i++)
            {
                var previewTilemapGo = new GameObject();
                m_PreviewTilemaps.Add(previewTilemapGo.AddComponent<Tilemap>());
                m_PreviewTilemapRenderers.Add(previewTilemapGo.AddComponent<TilemapRenderer>());

                previewTilemapGo.transform.SetParent(previewInstance.transform, false);
            }

            for (int x = -2; x <= 0; x++)
                for (int y = -1; y <= 1; y++)
                    m_PreviewTilemaps[0].SetTile(new Vector3Int(x, y, 0), tile);

            for (int y = -1; y <= 1; y++)
                m_PreviewTilemaps[1].SetTile(new Vector3Int(1, y, 0), tile);

            for (int x = -2; x <= 0; x++)
                m_PreviewTilemaps[2].SetTile(new Vector3Int(x, -2, 0), tile);

            m_PreviewTilemaps[3].SetTile(new Vector3Int(1, -2, 0), tile);
        }

        /// <summary>
        /// Handles cleanup for the Preview GUI
        /// </summary>
        protected virtual void DestroyPreview()
        {
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
                m_PreviewGrid = null;
                m_PreviewTilemaps = null;
                m_PreviewTilemapRenderers = null;
            }
        }

        /// <summary>
        /// Renders a static preview Texture2D for a RuleTile asset
        /// </summary>
        /// <param name="assetPath">Asset path of the RuleTile</param>
        /// <param name="subAssets">Arrays of assets from the given Asset path</param>
        /// <param name="width">Width of the static preview</param>
        /// <param name="height">Height of the static preview </param>
        /// <returns>Texture2D containing static preview for the RuleTile asset</returns>
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (tile.m_DefaultSprite != null)
            {
                Type t = GetType("UnityEditor.SpriteUtility");
                if (t != null)
                {
                    MethodInfo method = t.GetMethod("RenderStaticPreview", new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
                    if (method != null)
                    {
                        object ret = method.Invoke("RenderStaticPreview", new object[] { tile.m_DefaultSprite, Color.white, width, height });
                        if (ret is Texture2D)
                            return ret as Texture2D;
                    }
                }
            }
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts a Base64 string to a Texture2D
        /// </summary>
        /// <param name="base64">Base64 string containing image data</param>
        /// <returns>Texture2D containing an image from the given Base64 string</returns>
        public static Texture2D Base64ToTexture(string base64)
        {
            Texture2D t = new Texture2D(1, 1);
            t.hideFlags = HideFlags.HideAndDontSave;
            t.LoadImage(Convert.FromBase64String(base64));
            return t;
        }

        /// <summary>
        /// Wrapper for serializing a list of Rules
        /// </summary>
        [Serializable]
        class RuleTileRuleWrapper
        {
            /// <summary>
            /// List of Rules to serialize
            /// </summary>
            [SerializeField]
            public List<RuleTile_Custom.TilingRule> rules = new List<RuleTile_Custom.TilingRule>();
        }

        /// <summary>
        /// Copies all Rules from a RuleTile to the clipboard
        /// </summary>
        /// <param name="item">MenuCommand storing the RuleTile to copy from</param>
        [MenuItem("CONTEXT/RuleTile_Custom/Copy All Rules")]
        public static void CopyAllRules(MenuCommand item)
        {
            RuleTile_Custom tile = item.context as RuleTile_Custom;
            if (tile == null)
                return;

            RuleTileRuleWrapper rulesWrapper = new RuleTileRuleWrapper();
            rulesWrapper.rules = tile.m_TilingRules;
            var rulesJson = EditorJsonUtility.ToJson(rulesWrapper);
            EditorGUIUtility.systemCopyBuffer = rulesJson;
        }
        /// <summary>
        /// Pastes all Rules from the clipboard to a RuleTile
        /// </summary>
        /// <param name="item">MenuCommand storing the RuleTile to paste to</param>
        [MenuItem("CONTEXT/RuleTile_Custom/Paste Rules")]
        public static void PasteRules(MenuCommand item)
        {
            RuleTile_Custom tile = item.context as RuleTile_Custom;
            if (tile == null)
                return;

            try
            {
                RuleTileRuleWrapper rulesWrapper = new RuleTileRuleWrapper();
                EditorJsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, rulesWrapper);
                tile.m_TilingRules.AddRange(rulesWrapper.rules);
            }
            catch (Exception)
            {
                Debug.LogError("Unable to paste rules from system copy buffer");
            }
        }
    }
}


