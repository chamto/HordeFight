using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace UnityEngine.Tilemaps
{
	[Serializable]
	[CreateAssetMenu(fileName = "New Terrain Tile", menuName = "Tiles/Terrain Tile")]
	public class TerrainTile : TileBase
	{

        public enum Dir8 : byte
        {
            None = 0,
            Up = 1,
            UpRight = 2,
            Right = 4,
            DownRight = 8,
            Down = 16,
            DownLeft = 32,
            Left = 64,
            UpLeft = 128,

            All = 255, //모든방향설정 : 8방향값들의 총합
        }

        public Vector3Int[] _pos_dir8 = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, 1, 0)
        };


		[SerializeField]
		public Sprite[] m_Sprites;


        private Vector3Int __tilePos = Vector3Int.zero;
		public override void RefreshTile(Vector3Int location, ITilemap tileMap)
		{

            //for (int yd = -1; yd <= 1; yd++)
            //for (int xd = -1; xd <= 1; xd++)
            //{
            //	Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
            //	if (TileValue(tileMap, position))
            //		tileMap.RefreshTile(position);
            //}

            base.RefreshTile(location, tileMap);

            foreach(Vector3Int xy in _pos_dir8)
            {
                __tilePos = location + xy; 
                if (TileValue(tileMap, __tilePos))
                    tileMap.RefreshTile(__tilePos);
            }
		}

		public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
		{
			UpdateTile(location, tileMap, ref tileData);
		}



		private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData)
		{
			tileData.transform = Matrix4x4.identity;
			tileData.color = Color.white;

            //int mask = TileValue(tileMap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(1, 1, 0)) ? 2 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(1, 0, 0)) ? 4 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(1, -1, 0)) ? 8 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(0, -1, 0)) ? 16 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0;
            //mask += TileValue(tileMap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0;

            int mask = 0;
            mask += TileValue(tileMap, location + _pos_dir8[0]) ? 1 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[1]) ? 2 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[2]) ? 4 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[3]) ? 8 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[4]) ? 16 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[5]) ? 32 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[6]) ? 64 : 0;
            mask += TileValue(tileMap, location + _pos_dir8[7]) ? 128 : 0;


			byte original = (byte)mask;
            if ((original | 254) < 255) { mask = mask & 125; } //1:up
            if ((original | 251) < 255) { mask = mask & 245; } //4:right
            if ((original | 239) < 255) { mask = mask & 215; } //16:down
            if ((original | 191) < 255) { mask = mask & 95; } //64:left


			int index = GetIndex((byte)mask);
			if (index >= 0 && index < m_Sprites.Length && TileValue(tileMap, location))
			{
				tileData.sprite = m_Sprites[index];
                tileData.transform = GetTransform((byte)mask);

				tileData.color = Color.white;
				tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
				tileData.colliderType = Tile.ColliderType.Sprite;
			}
		}

		private bool TileValue(ITilemap tileMap, Vector3Int position)
		{
			TileBase tile = tileMap.GetTile(position);
			return (tile != null && tile == this);
		}


		private int GetIndex(byte mask)
		{
			switch (mask)
			{
				case 0: 
                    return 0;
                case (byte)Dir8.Up: //1 
                case (byte)Dir8.Right: //4
                case (byte)Dir8.Down: //16
                case (byte)Dir8.Left: //64
                    return 1;
				case 5:
				case 20:
				case 80:
				case 65: return 2;
				case 7:
				case 28:
				case 112:
				case 193: return 3;
				case 17:
				case 68: return 4;
				case 21:
				case 84:
				case 81:
				case 69: return 5;
				case 23:
				case 92:
				case 113:
				case 197: return 6;
				case 29:
				case 116:
				case 209:
				case 71: return 7;
				case 31:
				case 124:
				case 241:
				case 199: return 8;
				case 85: return 9;
				case 87:
				case 93:
				case 117:
				case 213: return 10;
				case 95:
				case 125:
				case 245:
				case 215: return 11;
				case 119:
				case 221: return 12;
				case 127:
				case 253:
				case 247:
				case 223: return 13;
				case 255: return 14;
			}
			return -1;
		}

		private Matrix4x4 GetTransform(byte mask)
		{
			switch (mask)
			{
				case 4:
				case 20:
				case 28:
				case 68:
				case 84:
				case 92:
				case 116:
				case 124:
				case 93:
				case 125:
				case 221:
				case 253:
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f), Vector3.one);
				case 16:
				case 80:
				case 112:
				case 81:
				case 113:
				case 209:
				case 241:
				case 117:
				case 245:
				case 247:
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -180f), Vector3.one);
				case 64:
				case 65:
				case 193:
				case 69:
				case 197:
				case 71:
				case 199:
				case 213:
				case 215:
				case 223:
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -270f), Vector3.one);
			}
			return Matrix4x4.identity;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(TerrainTile))]
	public class TerrainTileEditor : Editor
	{
		private TerrainTile tile { get { return (target as TerrainTile); } }

		public void OnEnable()
		{
			if (tile.m_Sprites == null || tile.m_Sprites.Length != 15)
			{
				tile.m_Sprites = new Sprite[15];
				EditorUtility.SetDirty(tile);
			}
		}


		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Place sprites shown based on the contents of the sprite.");
			EditorGUILayout.Space();

			float oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 210;

			EditorGUI.BeginChangeCheck();
			tile.m_Sprites[0] = (Sprite) EditorGUILayout.ObjectField("0 Filled", tile.m_Sprites[0], typeof(Sprite), false, null);
			tile.m_Sprites[1] = (Sprite) EditorGUILayout.ObjectField("1 Three Sides", tile.m_Sprites[1], typeof(Sprite), false, null);
			tile.m_Sprites[2] = (Sprite) EditorGUILayout.ObjectField("*2 Two Sides and One Corner", tile.m_Sprites[2], typeof(Sprite), false, null);
			tile.m_Sprites[3] = (Sprite) EditorGUILayout.ObjectField("3 Two Adjacent Sides", tile.m_Sprites[3], typeof(Sprite), false, null);
			tile.m_Sprites[4] = (Sprite) EditorGUILayout.ObjectField("4 Two Opposite Sides", tile.m_Sprites[4], typeof(Sprite), false, null);
			tile.m_Sprites[5] = (Sprite) EditorGUILayout.ObjectField("*5 One Side and Two Corners", tile.m_Sprites[5], typeof(Sprite), false, null);
			tile.m_Sprites[6] = (Sprite) EditorGUILayout.ObjectField("*6 One Side and One Lower Corner", tile.m_Sprites[6], typeof(Sprite), false, null);
			tile.m_Sprites[7] = (Sprite) EditorGUILayout.ObjectField("*7 One Side and One Upper Corner", tile.m_Sprites[7], typeof(Sprite), false, null);
			tile.m_Sprites[8] = (Sprite) EditorGUILayout.ObjectField("8 One Side", tile.m_Sprites[8], typeof(Sprite), false, null);
			tile.m_Sprites[9] = (Sprite) EditorGUILayout.ObjectField("*9 Four Corners", tile.m_Sprites[9], typeof(Sprite), false, null);
			tile.m_Sprites[10] = (Sprite) EditorGUILayout.ObjectField("*10 Three Corners", tile.m_Sprites[10], typeof(Sprite), false, null);
			tile.m_Sprites[11] = (Sprite) EditorGUILayout.ObjectField("*11 Two Adjacent Corners", tile.m_Sprites[11], typeof(Sprite), false, null);
			tile.m_Sprites[12] = (Sprite) EditorGUILayout.ObjectField("*12 Two Opposite Corners", tile.m_Sprites[12], typeof(Sprite), false, null);
			tile.m_Sprites[13] = (Sprite) EditorGUILayout.ObjectField("*13 One Corner", tile.m_Sprites[13], typeof(Sprite), false, null);
			tile.m_Sprites[14] = (Sprite) EditorGUILayout.ObjectField("14 Empty", tile.m_Sprites[14], typeof(Sprite), false, null);
			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(tile);

			EditorGUIUtility.labelWidth = oldLabelWidth;
		}
	}
#endif
}
