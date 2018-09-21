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
    public enum eTileMap_Kind
    {
        None,
        Dungeon,
        Forest,
        Swamp,
    }

    public class ImageHashValue
    {

        public string           hash = string.Empty;    //지정한 영역의 이미지를 md5암호화 알고리즘으로 해쉬로 변환한 값 

        public eTileMap_Kind    kind = eTileMap_Kind.None;      //타일맵의 종류를 지정
        public string           fileName = string.Empty;//전체이미지의 파일이름 
        public RectInt          rect = new RectInt();   //이미지 영역 지정

        public Color[]          colors = null;          //이미지색값

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

        /// <summary>
        /// 이미지 해쉬맵을 구성한다
        /// </summary>
        public IEnumerator MakeUp_ImageHashMap(string imgFileName , eTileMap_Kind kind, Vector2Int cellSize , Vector2Int padding )
        {
            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int TILE_COUNT = Vector2Int.zero;
            string str_hash = string.Empty;
            //===================================================
            Sprite ori_image = Resources.Load<Sprite>("Warcraft/Textures/TileMap/" + imgFileName);
            if (null == ori_image)
            {
                DebugWide.LogRed("File not found!! " + imgFileName);
                yield break;
            }
            TILE_COUNT.x = ori_image.texture.width / cellSize.x;
            TILE_COUNT.y = ori_image.texture.height / cellSize.y;


            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            Texture2D ori_tilemap = ori_image.texture;
            Texture2D get_tile = new Texture2D(cellSize.x, cellSize.y, TextureFormat.ARGB32, false);

            DebugWide.LogBlue(ori_tilemap.format + "  w:" + ori_tilemap.width + "   h:" + ori_tilemap.height); //print

            int count = 0;
            int uniCount = 0;
            float progress = 0f;
            for (int h = 0; h < TILE_COUNT.y; h++)
            {
                for (int w = 0; w < TILE_COUNT.x; w++)
                {
                    pos.x = cellSize.x * w;
                    pos.y = cellSize.y * h;

                    pos.x += padding.x * w;
                    pos.y += padding.y * h;

                    //텍스쳐의 좌하단이 0,0 좌표이다 
                    Color[] colors = ori_tilemap.GetPixels(pos.x, pos.y, cellSize.x, cellSize.y);

                    //타일 중앙의 알파값이 투명일때 이미지가 없는 것으로 간주한다 
                    int center = cellSize.x * cellSize.y / 2;
                    if (true == colors[center].a.Equals(0f))
                    {
                        continue;
                    }

                    //이미지에서 해쉬값 구하기
                    get_tile.SetPixels(colors);
                    byte[] raw = get_tile.EncodeToPNG();
                    byte[] byte_hash = md5.ComputeHash(raw);

                    //해쉬값을 문자열로 변환 
                    str_hash = string.Empty;
                    foreach (byte one in byte_hash)
                    {
                        str_hash += one;
                    }
                    //DebugWide.LogBlue(str_hash + "   " + byte_hash.Length + "   " + pos); //print

                    ImageHashValue out_value = null;
                    if (false == _uniqueImages.TryGetValue(str_hash, out out_value))
                    {
                        out_value = new ImageHashValue();
                        out_value.colors = colors;
                        out_value.fileName = imgFileName;
                        out_value.hash = str_hash;
                        out_value.rect = new RectInt(pos.x, pos.y, cellSize.x, cellSize.y);
                        out_value.kind = kind;
                        _uniqueImages.Add(str_hash, out_value);

                        uniCount++;
                    }

                    count++;
                    progress = (float)count / (TILE_COUNT.y * TILE_COUNT.x);
                    DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + (TILE_COUNT.y * TILE_COUNT.x) + " : " + count + "  :" + pos); //print
                    yield return new WaitForEndOfFrame();
                }
            }


            DebugWide.LogBlue("done. unique tile find complete! : " + imgFileName + " add result: " + uniCount);

            yield break;
        }

        /// <summary>
        /// 저장된 이미지해쉬맵으로 타일맵 텍스쳐를 만든다
        /// </summary>
        public IEnumerator CreatePNG_ImageHashMap(string imgFileName , eTileMap_Kind createKind , Vector2Int padding)
        {
            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int CELL_SIZE = new Vector2Int(16, 16); //16x16 셀이미지
            Vector2Int TILE_COUNT = Vector2Int.zero;
            Vector2Int IMG_SIZE = new Vector2Int(512, 0); //생성할 이미지 가로길이를 512로 고정한다 

            TILE_COUNT.x = IMG_SIZE.x / (CELL_SIZE.x + padding.x);
            TILE_COUNT.y = IMG_SIZE.x / (CELL_SIZE.y + padding.y);


            int MAX_COUNT = 0;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                if (createKind != byby.kind) continue;
                MAX_COUNT++;
            }

            IMG_SIZE.y = (MAX_COUNT / TILE_COUNT.x);
            IMG_SIZE.y *= (CELL_SIZE.y + padding.y);
            DebugWide.LogBlue(imgFileName +"   :  "+ IMG_SIZE +"    :  "+ createKind.ToString()); //print
            Texture2D new_tilemap = new Texture2D(IMG_SIZE.x, IMG_SIZE.y, TextureFormat.ARGB32, false);

            int count = 0;
            float progress = 0f;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                if (createKind != byby.kind) continue;

                pos.x = (count % TILE_COUNT.x) * (CELL_SIZE.x + padding.x);
                pos.y = (count / TILE_COUNT.y) * (CELL_SIZE.y + padding.y);

                new_tilemap.SetPixels(pos.x, pos.y, CELL_SIZE.x, CELL_SIZE.y, byby.colors);

                count++;
                progress = (float)count / (TILE_COUNT.y * TILE_COUNT.x);
                DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + MAX_COUNT + " : " + count + "  :" + pos); //print
                yield return new WaitForEndOfFrame();
            }



            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/" + imgFileName , new_raw);

            DebugWide.LogBlue("done. file create complete +" + ((Time.time - startTime) / 60f).ToString() + "분 걸림");

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

                //StartCoroutine(CreateImage_AddPadding("dungeon_01", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("dungeon_02", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("dungeon_03", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("dungeon_04", CELL_SIZE, PADDING));   

                //StartCoroutine(CreateImage_AddPadding("forest_01", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("forest_02", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("forest_03", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("forest_04", CELL_SIZE, PADDING));   

                //StartCoroutine(CreateImage_AddPadding("swamp_01", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("swamp_02", CELL_SIZE, PADDING));   
                //StartCoroutine(CreateImage_AddPadding("swamp_03", CELL_SIZE, PADDING));   
            }

            if (GUI.Button(new Rect(10, 120, 200, 100), new GUIContent("MakeUp_ImageHashMap", icon)))
            {
                Vector2Int CELL_SIZE = new Vector2Int(16, 16);
                Vector2Int PADDING = new Vector2Int(2, 2);

                StartCoroutine(MakeUp_ImageHashMap("dungeon_01_addPadding",eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("dungeon_02_addPadding",eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("dungeon_03_addPadding",eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("dungeon_04_addPadding",eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));   
							   
                StartCoroutine(MakeUp_ImageHashMap("forest_01_addPadding",eTileMap_Kind.Forest, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("forest_02_addPadding",eTileMap_Kind.Forest, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("forest_03_addPadding",eTileMap_Kind.Forest, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("forest_04_addPadding",eTileMap_Kind.Forest, CELL_SIZE, PADDING));   
							   
                StartCoroutine(MakeUp_ImageHashMap("swamp_01_addPadding",eTileMap_Kind.Swamp, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("swamp_02_addPadding",eTileMap_Kind.Swamp, CELL_SIZE, PADDING));   
                StartCoroutine(MakeUp_ImageHashMap("swamp_03_addPadding",eTileMap_Kind.Swamp, CELL_SIZE, PADDING)); 
            }

            if (GUI.Button(new Rect(10, 230, 200, 100), new GUIContent("CreatePNG_ImageHashMap", icon)))
            {
                Vector2Int PADDING = new Vector2Int(2, 2);

                StartCoroutine(CreatePNG_ImageHashMap("dungeon",eTileMap_Kind.Dungeon, PADDING));   
                StartCoroutine(CreatePNG_ImageHashMap("forest",eTileMap_Kind.Forest, PADDING));   
                StartCoroutine(CreatePNG_ImageHashMap("swamp",eTileMap_Kind.Swamp, PADDING));   
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
