using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;


//md5를 이용한 텍스쳐 해쉬값 생성
//http://www.vcskicks.com/image-hash2.php

//참고용 
//https://blog.bloodcat.com/243
//https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NE


//바이트 배열을 정수로 변환
//https://docs.microsoft.com/ko-kr/dotnet/csharp/programming-guide/types/how-to-convert-a-byte-array-to-an-int

namespace Tool
{
    public class ImageHashValue
    {
        public enum eKind
        {
            None,
            Dungeon,
            Forest,
            Swamp,
        }
        public eKind    kind = eKind.None;      //타일의 종류를 지정
        public string   hash = string.Empty;    //지정한 영역의 이미지를 md5암호화 알고리즘으로 해쉬로 변환한 값 
        public string   fileName = string.Empty;//전체이미지의 파일이름 
        public RectInt  rect = new RectInt();   //이미지 영역 지정

        public Color[]  colors = null;          //이미지색값

    }

    public class ImageHash_Compare : MonoBehaviour
    {
        


        public Dictionary<string, ImageHashValue> _uniqueImages = new Dictionary<string, ImageHashValue>();

        //<문자열 해쉬키 , 타일Colors>
        //public Dictionary<string, Color[]> _uniqueImages = new Dictionary<string, Color[]>();


        void Start()
        {
            
        }

        /// <summary>
        /// 지정한 이미지 파일에 셀과 셀사이의 간격을 준다 
        /// 타일 분석을 위해 셀과 셀사이를 벌린다
        /// </summary>
        public IEnumerator CreateImage_AddPadding(string imgFileName , Vector2Int cellSize , Vector2Int padding )
        {
            
            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int TILE_COUNT = Vector2Int.zero;
            Vector2Int newImgSize = Vector2Int.zero;
            //===================================================
            Sprite ori_image = Resources.Load<Sprite>("Warcraft/Textures/TileMap/" + imgFileName);
            if (null == ori_image)
            {
                DebugWide.LogRed("File not found!! " + imgFileName);
                yield break;
            }
            TILE_COUNT.x = ori_image.texture.width / cellSize.x;
            TILE_COUNT.y = ori_image.texture.height / cellSize.y;
            newImgSize.x = ori_image.texture.width + (TILE_COUNT.x * padding.x);
            newImgSize.y = ori_image.texture.height + (TILE_COUNT.y * padding.y);

            Texture2D ori_tilemap = ori_image.texture;
            Texture2D new_tilemap = new Texture2D(newImgSize.x, newImgSize.y, TextureFormat.ARGB32, false);

            DebugWide.LogBlue(new_tilemap.format + "  w:" + newImgSize.x + "   h:" + newImgSize.y); //print

            int count = 0;
            float progress = 0f;
            for (int h = 0; h < TILE_COUNT.y; h++)
            {
                for (int w = 0; w < TILE_COUNT.x; w++)
                {
                    pos.x = cellSize.x * w;
                    pos.y = cellSize.y * h;

                    //텍스쳐의 좌하단이 0,0 좌표이다 
                    Color[] colors = ori_tilemap.GetPixels(pos.x,pos.y, cellSize.x, cellSize.y);

                    //띨 간격을 넣어준다
                    pos.x += padding.x * w;
                    pos.y += padding.y * h;
                    new_tilemap.SetPixels(pos.x, pos.y, cellSize.x, cellSize.y, colors);

                    count++;
                    progress = (float)count / (TILE_COUNT.y * TILE_COUNT.x);
                    DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + (TILE_COUNT.y * TILE_COUNT.x) + " : " + count  + "  :" + pos); //print
                    yield return new WaitForEndOfFrame();
                }
            }


            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/" + imgFileName + "_addPadding.png", new_raw);


            DebugWide.LogBlue("done. file create complete +" + ((Time.time - startTime) / 60f).ToString() + "분 걸림");

            yield break;
        }

        IEnumerator Start_()
        {
            const int TILEMAP_WITH = 1024;
            const int TILEMAP_HEIGHT = 1024;
            const int TILE_SIZE = 16;
            const int TILE_MAX_COUNT = 1024 / 16; //64 
            Vector2Int pos = Vector2Int.zero;
            string str_hash = string.Empty;
            float startTime = Time.time;
            //===================================================
            Sprite dungeon = Resources.Load<Sprite>("Warcraft/Textures/Test/dungeon_01");
            if (null == dungeon)
            {
                DebugWide.LogRed("File not found!!");
                yield break;
            }
            DebugWide.LogBlue(dungeon.texture.format);

            Texture2D ori_tilemap = dungeon.texture;
            //byte[] raw = tex_tilemap.EncodeToPNG();

            Texture2D tile = new Texture2D(TILE_SIZE, TILE_SIZE, TextureFormat.ARGB32, false);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            for (int h = 0; h < TILE_MAX_COUNT; h++)
            {
                for (int w = 0; w < TILE_MAX_COUNT; w++)
                {
                    pos.x = w;
                    pos.y = h;

                    //텍스쳐의 좌하단이 0,0 좌표이다 
                    Color[] colors = ori_tilemap.GetPixels(TILE_SIZE * pos.x, TILE_SIZE * pos.y, TILE_SIZE, TILE_SIZE);
                    //타일 중앙의 알파값이 투명일때 이미지가 없는 것으로 간주한다 
                    if (true == colors[TILE_SIZE * TILE_SIZE / 2].a.Equals(0f))
                    {
                        continue;
                    }
                    tile.SetPixels(colors);
                    byte[] raw = tile.EncodeToPNG();

                    byte[] byte_hash = md5.ComputeHash(raw);

                    str_hash = string.Empty;
                    foreach (byte one in byte_hash)
                    {
                        str_hash += one;
                    }
                    DebugWide.LogBlue(str_hash + "   " + byte_hash.Length + "   " + pos); //print

                    ImageHashValue out_value = null;
                    if (false == _uniqueImages.TryGetValue(str_hash, out out_value))
                    {
                        out_value = new ImageHashValue();
                        out_value.colors = colors;
                        out_value.fileName = "";
                        out_value.hash = str_hash;
                        out_value.rect = new RectInt(TILE_SIZE * pos.x, TILE_SIZE * pos.y, TILE_SIZE, TILE_SIZE);
                        _uniqueImages.Add(str_hash, out_value);
                    }

                    //File.WriteAllBytes(Application.dataPath + "/../test/tile"+pos.x +"_"+pos.y +".png", raw);

                    yield return new WaitForEndOfFrame();
                }
            }


            DebugWide.LogBlue("done. unique tile find complete");


            Texture2D new_tilemap = new Texture2D(TILEMAP_WITH / 2, TILEMAP_HEIGHT / 2, TextureFormat.ARGB32, false);
            int count = 0;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                pos.x = (count % (TILE_MAX_COUNT / 2)) * TILE_SIZE;
                pos.y = (count / (TILE_MAX_COUNT / 2)) * TILE_SIZE;
                DebugWide.LogBlue(pos);
                new_tilemap.SetPixels(pos.x, pos.y, TILE_SIZE, TILE_SIZE, byby.colors);

                count++;
            }

            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/../TileTool/tilemap.png", new_raw);

            //pos = Vector3.zero;
            //foreach(byte[] byby in _uniqueTiles.Values)
            //{

            //    File.WriteAllBytes(Application.dataPath + "/../test/tile" + pos.x + "_" + pos.y + ".png", byby);
            //    DebugWide.LogBlue(pos);
            //    pos.x++;
            //    yield return new WaitForEndOfFrame();
            //}

            DebugWide.LogBlue("done. file create complete +" + (Time.time - startTime) / 1000 + "초 걸림");

            yield break;
        }

        // Update is called once per frame
        void Update()
        {

        }

        //==================================================
        //   레가시 GUI
        //==================================================

        public Texture2D icon;
        void OnGUI()
        {
            
            if (GUI.Button(new Rect(10, 10, 200, 100), new GUIContent("CreateImage_AddPadding", icon)))
            {
                Vector2Int CELL_SIZE = new Vector2Int(16, 16);
                Vector2Int PADDING = new Vector2Int(2, 2);
                StartCoroutine(CreateImage_AddPadding("dungeon_01", CELL_SIZE, PADDING));   
            }
        }

    }


    //ref : https://docs.unity3d.com/kr/530/ScriptReference/Texture2D.EncodeToPNG.html
    //인코딩된 PNG 데이터는 /ARGB32/텍스쳐에서는 알파 채널을 포함하고, /RGB24/에서는 알파채널을 포함하지 않습니다. PNG 데이터는 감마 보정(gamma correction)을 포함하지 않거나 색상 프로파일 정보가 없습니다.
    //현재 랜더링 화면을 파일로 출력한다
    public class PNGUploader : MonoBehaviour
    {
        // Take a shot immediately
        IEnumerator Start()
        {
            yield return UploadPNG();
        }

        IEnumerator UploadPNG()
        {
            // We should only read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();

            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            Object.Destroy(tex);

            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

            DebugWide.LogBlue(Application.dataPath + "/../SavedScreen.png");


        }

    }
}
