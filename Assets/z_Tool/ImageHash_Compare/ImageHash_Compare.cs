using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;
using System.Xml;
using UtilGS9;

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
        
        public string hash = string.Empty;    //지정한 영역의 이미지를 md5암호화 알고리즘으로 해쉬로 변환한 값 

        public eTileMap_Kind kind = eTileMap_Kind.None;      //타일맵의 종류를 지정
        public string fileName = string.Empty;//전체이미지의 파일이름 
        public RectInt rect = new RectInt();   //이미지 영역 지정

        public Color[] colors = null;          //이미지색값

		public override string ToString()
		{
            return hash + "  " + kind + "   " + fileName + "   " + rect;
		}
	}

    public class ImageHash_Compare : MonoBehaviour
    {

        private XML_Parser _parser = new XML_Parser();

        public Dictionary<string, ImageHashValue> _uniqueImages = new Dictionary<string, ImageHashValue>();
        Dictionary<string, Sprite> _spriteMap = new Dictionary<string, Sprite>();

        void Start()
        {
            _parser.SetREF_UniqueImage(ref _uniqueImages);

            //타일맵 스프라이트 목록을 저장한다 
            Sprite[] sprs = Resources.LoadAll<Sprite>("Warcraft/Textures/TileMap/");
            foreach(Sprite s in sprs)
            {
                _spriteMap.Add(s.name, s);
            }

        }

        /// <summary>
        /// 지정한 이미지 파일에 셀과 셀사이의 간격을 준다 
        /// 타일 분석을 위해 셀과 셀사이를 벌린다
        /// </summary>
        public IEnumerator CreatePNG_AddPadding(string imgFileName, Vector2Int cellSize, Vector2Int padding)
        {

            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int TILE_COUNT = Vector2Int.zero;
            Vector2Int IMG_SIZE = Vector2Int.zero;
            //===================================================
            if(false == _spriteMap.Keys.Contains(imgFileName))
            {
                DebugWide.LogRed("File not found!! " + imgFileName);
                yield break;
            }
            Sprite ori_image = _spriteMap[imgFileName];

            TILE_COUNT.x = ori_image.texture.width / cellSize.x;
            TILE_COUNT.y = ori_image.texture.height / cellSize.y;
            IMG_SIZE.x = ori_image.texture.width + (TILE_COUNT.x * padding.x);
            IMG_SIZE.y = ori_image.texture.height + (TILE_COUNT.y * padding.y);

            Texture2D ori_tilemap = ori_image.texture;
            Texture2D new_tilemap = new Texture2D(IMG_SIZE.x, IMG_SIZE.y, TextureFormat.RGBA32, false);

            //투명(0,0,0,0)색으로 초기화 한다 
            Color[] black_colors = new Color[IMG_SIZE.x * IMG_SIZE.y];
            new_tilemap.SetPixels(black_colors);

            DebugWide.LogBlue(new_tilemap.format + "  w:" + IMG_SIZE.x + "   h:" + IMG_SIZE.y); //print

            int count = 0;
            float progress = 0f;
            for (int h = 0; h < TILE_COUNT.y; h++)
            {
                for (int w = 0; w < TILE_COUNT.x; w++)
                {
                    pos.x = cellSize.x * w;
                    pos.y = cellSize.y * h;

                    //텍스쳐의 좌하단이 0,0 좌표이다 
                    Color[] colors = ori_tilemap.GetPixels(pos.x, pos.y, cellSize.x, cellSize.y);

                    //띨 간격을 넣어준다
                    pos.x += padding.x * w;
                    pos.y += padding.y * h;
                    new_tilemap.SetPixels(pos.x, pos.y, cellSize.x, cellSize.y, colors);

                    count++;
                    progress = (float)count / (TILE_COUNT.y * TILE_COUNT.x);
                    DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + (TILE_COUNT.y * TILE_COUNT.x) + " : " + count + "  :" + pos); //print
                    yield return new WaitForEndOfFrame();
                }
            }

            new_tilemap.Apply();
            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/" + imgFileName + "_Padding.png", new_raw);

            DebugWide.LogBlue("done. file create complete +" + ((Time.time - startTime) / 60f).ToString() + "분 걸림");

            yield break;
        }

        /// <summary>
        /// 이미지 해쉬맵을 구성한다
        /// </summary>
        public IEnumerator MakeUp_ImageHashMap(string imgFileName, eTileMap_Kind kind, Vector2Int cellSize, Vector2Int padding)
        {
            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int TILE_COUNT = Vector2Int.zero;
            string str_hash = string.Empty;
            //===================================================
            if (false == _spriteMap.Keys.Contains(imgFileName))
            {
                DebugWide.LogRed("File not found!! " + imgFileName);
                yield break;
            }
            Sprite ori_image = _spriteMap[imgFileName];

            TILE_COUNT.x = ori_image.texture.width / (cellSize.x + padding.x);
            TILE_COUNT.y = ori_image.texture.height / (cellSize.y + padding.y);


            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            Texture2D ori_tilemap = ori_image.texture;
            Texture2D get_tile = new Texture2D(cellSize.x, cellSize.y, TextureFormat.RGBA32, false);

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
                    get_tile.Apply();

                    //해쉬값을 문자열로 변환 
                    str_hash = string.Empty;
                    foreach (byte one in byte_hash)
                    {
                        str_hash += one;
                    }
                    //DebugWide.LogBlue(str_hash + "   " + byte_hash.Length + "   " + pos); //print

                    //str_hash = get_tile.imageContentsHash.ToString(); //chamto test - unity 에서 지원하는 이미지해쉬 테스트 : 조각낸 타일에 대한 해쉬값이 모두 동일함. 못씀 

                    ImageHashValue updateValue = null;
                    if (false == _uniqueImages.TryGetValue(str_hash, out updateValue))
                    {
                        //키가 없다면 값추가
                        updateValue = new ImageHashValue();
                        updateValue.hash = str_hash;
                        updateValue.fileName = imgFileName;
                        updateValue.kind = kind;
                        updateValue.rect = new RectInt(pos.x, pos.y, cellSize.x, cellSize.y);
                        updateValue.colors = colors;
                        _uniqueImages.Add(str_hash, updateValue);

                        uniCount++;
                    }
                    else
                    {
                        //키가 있다면 값갱신
                        updateValue.hash = str_hash;
                        updateValue.fileName = imgFileName;
                        updateValue.kind = kind;
                        updateValue.rect = new RectInt(pos.x, pos.y, cellSize.x, cellSize.y);
                        updateValue.colors = colors;
                    }

                    count++;
                    progress = (float)count / (TILE_COUNT.y * TILE_COUNT.x);
                    DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + (TILE_COUNT.y * TILE_COUNT.x) + " : " + count + "  :" + str_hash); //print
                    yield return new WaitForEndOfFrame();
                }
            }


            DebugWide.LogBlue("done. unique tile find complete! : " + imgFileName + " add result: " + uniCount);

            yield break;
        }

        /// <summary>
        /// 저장된 이미지해쉬맵으로 타일맵 텍스쳐를 만든다
        /// </summary>
        public IEnumerator CreatePNG_Atlas(string imgFileName, eTileMap_Kind createKind, Vector2Int padding)
        {
            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int CELL_SIZE = new Vector2Int(16, 16); //16x16 셀이미지
            Vector2Int TILE_COUNT = Vector2Int.zero;
            Vector2Int IMG_SIZE = new Vector2Int(512, 0); //생성할 이미지 가로길이를 512로 고정한다 




            int MAX_KIND_COUNT = 0;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                if (createKind != byby.kind) continue;
                MAX_KIND_COUNT++;
            }

            //처리할 데이터가 없는 경우 
            if (0 == MAX_KIND_COUNT) yield break;

            TILE_COUNT.x = IMG_SIZE.x / (CELL_SIZE.x + padding.x);
            TILE_COUNT.y = (MAX_KIND_COUNT / TILE_COUNT.x);
            IMG_SIZE.y = (TILE_COUNT.y+1) * (CELL_SIZE.y + padding.y);
            DebugWide.LogBlue(imgFileName + "   :  " + IMG_SIZE + "    :  " + createKind.ToString()); //print
            Texture2D new_tilemap = new Texture2D(IMG_SIZE.x, IMG_SIZE.y, TextureFormat.RGBA32, false);

            //투명(0,0,0,0)색으로 초기화 한다 
            Color[] black_colors = new Color[IMG_SIZE.x * IMG_SIZE.y];
            new_tilemap.SetPixels(black_colors);


            int count = 0;
            float progress = 0f;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                if (createKind != byby.kind) continue;

                pos.x = (count % TILE_COUNT.x) * (CELL_SIZE.x + padding.x);
                pos.y = (count / TILE_COUNT.x) * (CELL_SIZE.y + padding.y);

                new_tilemap.SetPixels(pos.x, pos.y, CELL_SIZE.x, CELL_SIZE.y, byby.colors);

                count++;
                progress = (float)count / (MAX_KIND_COUNT);
                DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + MAX_KIND_COUNT + " : " + count + "  :" + pos); //print
                yield return new WaitForEndOfFrame();
            }


            new_tilemap.Apply();
            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/" + imgFileName + ".png", new_raw);

            DebugWide.LogBlue("done. file create complete +" + ((Time.time - startTime) / 60f).ToString() + "분 걸림");

            yield break;
        }

        public IEnumerator CreatePNG_Tiles(string imgFileName, eTileMap_Kind createKind)
        {
            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;

            int MAX_KIND_COUNT = 0;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                if (createKind != byby.kind) continue;
                MAX_KIND_COUNT++;
            }

            //처리할 데이터가 없는 경우 
            if (0 == MAX_KIND_COUNT) yield break;

           
            Texture2D tile = null;
            int count = 0;
            float progress = 0f;
            foreach (ImageHashValue byby in _uniqueImages.Values)
            {
                if (createKind != byby.kind) continue;

                tile = new Texture2D(byby.rect.width, byby.rect.height, TextureFormat.RGBA32, false);
                tile.SetPixels(byby.colors);
                byte[] new_raw = tile.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/"  + byby.kind + "/" + imgFileName + "_" + count.ToString() + ".png", new_raw);

                count++;
                progress = (float)count / (MAX_KIND_COUNT);
                DebugWide.LogBlue("진행률 : " + progress * 100f + "%  - count:" + MAX_KIND_COUNT + " : " + count + "  :" + pos); //print
                yield return new WaitForEndOfFrame();
            }

            DebugWide.LogBlue("done. file create complete +" + ((Time.time - startTime) / 60f).ToString() + "분 걸림");

            yield break;
        }

        /// <summary>
        /// 이미지해쉬맵의 객체컬러값을 갱신한다
        /// 파일에서 불러온 이미지해쉬맵의 아이템은 컬러값이 없기 떄문에, 따로 채워주어야 한다  
        /// </summary>
        public void UpdateColors_Remove()
        {
            int MAX_COUNT = _uniqueImages.Count;
            List<string> removeKeyList = new List<string>();
            foreach(ImageHashValue hv in _uniqueImages.Values)
            {
                Sprite tileMap_spr = _spriteMap[hv.fileName];
                hv.colors = tileMap_spr.texture.GetPixels(hv.rect.x, hv.rect.y, hv.rect.width, hv.rect.height);

                //가져온 색정보가 0인 경우 이미지목록에서 해당 해쉬값을 제거한다 
                int center = hv.rect.width * hv.rect.height / 2;
                if (true == hv.colors[center].a.Equals(0f))
                {
                    //지울대상 지정
                    removeKeyList.Add(hv.hash);
                }

            }

            foreach(string key in removeKeyList)
            {
                _uniqueImages.Remove(key);
            }

            DebugWide.LogBlue("=============UpdateColors_Remove=============  전체: " + MAX_COUNT + " 제거한 타일: " + removeKeyList.Count);
        }

        public void RemoveFromPath(string path)
        {
            Texture2D[] tx = Resources.LoadAll<Texture2D>(path);
            DebugWide.LogBlue(tx.Length);

            int count = 0;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string str_hash = string.Empty;
            string temp = string.Empty;
            foreach(Object t in tx)
            {

                //if (t2 is Sprite) DebugWide.LogBlue("Sprite : " + t2.name);
                //if (t2 is Texture2D) DebugWide.LogBlue("Texture2D : " + t2.name);

                if (t is Texture2D)
                {
                    Texture2D t2d = t as Texture2D;
                    if(TextureFormat.RGBA32 != t2d.format)
                    {
                        DebugWide.LogYellow("[waring] 텍스쳐 포맷이 RGBA32 이어야 합니다!!! " + t2d.name );
                        continue;
                    }
                    //같은 텍스쳐라도 텍스쳐 포맷에 따라 해쉬값이 다르게 나온다. 저장된 해쉬는 RGB32 기반으로 구한 값이기 때문에 해당 포맷을 맞춰주어야 한다 
                    byte[] raw = (t2d).EncodeToPNG();
                    byte[] byte_hash = md5.ComputeHash(raw);
                    //해쉬값을 문자열로 변환 
                    str_hash = string.Empty;
                    foreach (byte one in byte_hash)
                    {
                        str_hash += one;
                    }

                    if(true == _uniqueImages.Remove(str_hash))
                    {
                        count++;

                        temp = Application.dataPath + "/Resources/" + path + t2d.name + ".png";
                        DebugWide.LogBlue(temp + "  :   " + str_hash); //chamto test
                        System.IO.File.Delete(temp); //실제 파일도 제거한다 

                    }    
                }

            }

            DebugWide.LogBlue("=============RemoveImageHash=============  전체: " + tx.Length + " 제거한 타일: " + count);
        }

        //==================================================
        //   레가시 GUI
        //==================================================

        public Texture2D icon;
        void OnGUI()
        {

            if (GUI.Button(new Rect(10, 10, 200, 100), new GUIContent("CreatePNG_AddPadding", icon)))
            {
                Vector2Int CELL_SIZE = new Vector2Int(16, 16);
                Vector2Int PADDING = new Vector2Int(2, 2);

                //StartCoroutine(CreatePNG_AddPadding("dungeon_01", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("dungeon_02", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("dungeon_03", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("dungeon_04", CELL_SIZE, PADDING));   
								 
                //StartCoroutine(CreatePNG_AddPadding("forest_01", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("forest_02", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("forest_03", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("forest_04", CELL_SIZE, PADDING));   
								 
                //StartCoroutine(CreatePNG_AddPadding("swamp_01", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("swamp_02", CELL_SIZE, PADDING));   
                //StartCoroutine(CreatePNG_AddPadding("swamp_03", CELL_SIZE, PADDING));   
            }

            if (GUI.Button(new Rect(10, 120, 200, 100), new GUIContent("MakeUp_ImageHashMap", icon)))
            {
                Vector2Int CELL_SIZE = new Vector2Int(16, 16);
                Vector2Int PADDING = new Vector2Int(2, 2);

                StartCoroutine(MakeUp_ImageHashMap("dungeon_01_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("dungeon_02_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("dungeon_03_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("dungeon_04_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));

                StartCoroutine(MakeUp_ImageHashMap("forest_01_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("forest_02_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("forest_03_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("forest_04_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));

                StartCoroutine(MakeUp_ImageHashMap("swamp_01_addPadding", eTileMap_Kind.Swamp, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("swamp_02_addPadding", eTileMap_Kind.Swamp, CELL_SIZE, PADDING));
                StartCoroutine(MakeUp_ImageHashMap("swamp_03_addPadding", eTileMap_Kind.Swamp, CELL_SIZE, PADDING));

                //코루틴 때문에 수행되는 시기가 처음부분이다 <= 정상수행 안됨 
                //_parser.SaveXML();
                //DebugWide.LogBlue("Saved!!");
            }

            if (GUI.Button(new Rect(10, 230, 200, 100), new GUIContent("Save", icon)))
            {
                _parser.SaveXML("ImageHashMap.xml");
                //_parser.SaveXML("UnityImage_Hash128_Map.xml");

            }

            if (GUI.Button(new Rect(220, 230, 200, 100), new GUIContent("RemoveFromPath", icon)))
            {
                RemoveFromPath("Warcraft/Textures/TileSlice/0_Remove/");

            }

            if (GUI.Button(new Rect(10, 340, 200, 100), new GUIContent("Load", icon)))
            {
                HordeFight.SingleO.coroutine.Start_Sync(_parser.LoadXML("ImageHashMap.xml"), null, "ImageHashMap");
                //HordeFight.Single.coroutine.Start_Sync(_parser.LoadXML("UnityImage_Hash128_Map.xml"), null, "UnityImage_Hash128_Map");

                UpdateColors_Remove();
            }

            if (GUI.Button(new Rect(220, 340, 200, 100), new GUIContent("CreatePNG_Atlas", icon)))
            {
                Vector2Int PADDING = new Vector2Int(2, 2);

                StartCoroutine(CreatePNG_Atlas("dungeon", eTileMap_Kind.Dungeon, PADDING));
                //StartCoroutine(CreatePNG_Atlas("forest", eTileMap_Kind.Forest, PADDING));
                //StartCoroutine(CreatePNG_Atlas("swamp", eTileMap_Kind.Swamp, PADDING));
            }

            if (GUI.Button(new Rect(430, 340, 200, 100), new GUIContent("CreatePNG_Tiles", icon)))
            {
                
                StartCoroutine(CreatePNG_Tiles("dungeon", eTileMap_Kind.Dungeon));
                //StartCoroutine(CreatePNG_Tiles("forest", eTileMap_Kind.Forest));
                //StartCoroutine(CreatePNG_Tiles("swamp", eTileMap_Kind.Swamp));
            }
        }

    }



}

namespace Tool
{
    public class XML_Parser
    {
        //private string m_strFileName = "ImageHashMap.xml";
        private Dictionary<string, ImageHashValue> _uniqueImages = null;

        private bool _bCompleteLoad = false;

        public void SetREF_UniqueImage(ref Dictionary<string, ImageHashValue> uniqueImages)
        {
            _uniqueImages = uniqueImages;
        }

        public bool bCompleteLoad
        {
            get { return _bCompleteLoad; }
        }



        public void ClearAll()
        {
            _bCompleteLoad = false;

        }


        private void Parse_MemoryStream(MemoryStream stream)
        {
            _bCompleteLoad = false;


            //< ImageHash filename = "dungeon_0" kind = "dungeon" >
            //      < info num = "0" cellpos = "(6,7)" cellsize = "(16,0)" hash = "dddsdf" />
            //</ ImageHash >
            //< ImageHash filename = "forest_0" kind = "forest" >
            //      < info num = "0" cellpos = "(0,1)" cellsize = "(4,4)" hash = "ddgffgdsdf" />
            //</ ImageHash >
            //------------------------------------------------------------------------


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(stream);

            XmlElement root_element = xmldoc.DocumentElement;   //<root>        
            XmlNodeList firstList = root_element.ChildNodes;   //<ImageHash>
            XmlNodeList secondList = null;   //<info>
            XmlAttributeCollection attrs = null;
            XmlNode xmlNode = null;
            ImageHashValue item = null;
            for (int i = 0; i<firstList.Count; ++i)
            {
                
                if (false == (firstList[i] is System.Xml.XmlElement)) //System.Xml.XmlComment 주석일 때는 처리 하지 않는다
                    continue;

                item = new ImageHashValue();

                //==================== <ImageHash> ====================
                xmlNode = firstList[i].Attributes.GetNamedItem("fileName");
                item.fileName = xmlNode.Value;
                xmlNode = firstList[i].Attributes.GetNamedItem("kind");
                item.kind = (eTileMap_Kind)System.Enum.Parse(typeof(eTileMap_Kind), xmlNode.Value);


                //==================== <info> ====================
                secondList = firstList[i].ChildNodes; 
                for (int j = 0; j<secondList.Count; ++j)
                {
                    
                    if (false == (secondList[j] is System.Xml.XmlElement)) //System.Xml.XmlComment 주석일 때는 처리 하지 않는다
                        continue;

                    attrs = secondList[j].Attributes;
                    foreach (XmlNode n in attrs)
                    {
                        switch (n.Name)
                        {
                            case "cellPos":
                                {
                                    Vector2Int v2Int =  Misc.StringToVector2Int(n.Value);
                                    item.rect.x = v2Int.x;
                                    item.rect.y = v2Int.y;
                                }    
                                break;
                            case "cellSize":
                                {
                                    Vector2Int v2Int = Misc.StringToVector2Int(n.Value);
                                    item.rect.width = v2Int.x;
                                    item.rect.height = v2Int.y;
                                }
                                break;
                            case "hash":
                                {
                                    item.hash = n.Value;
                                }
                                break;
                            
                        }//switch
                    }//attrs
                }//second

                ImageHashValue updateValue = null;
                if(false == _uniqueImages.TryGetValue(item.hash, out updateValue))
                {
                    //키가 없다면 값추가
                    _uniqueImages.Add(item.hash, item);

                }else
                {
                    //키가 있다면 값갱신
                    updateValue.fileName = item.fileName;
                    updateValue.kind = item.kind;
                    updateValue.rect = item.rect;    
                }

                    
            }//first
            _bCompleteLoad = true;

            DebugWide.LogBlue("=====================Loaded!!=====================");
        }//func end

        public IEnumerator FileLoading(string strFilePath, System.Action<MemoryStream> result = null)
        {
            MemoryStream memStream = null;
#if SERVER || TOOL
            {
                //CDefine.CommonLog("1__" + strFilePath); //chamto test
                memStream = new MemoryStream(File.ReadAllBytes(strFilePath));
            }

#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
            {

                UnityEngine.WWW wwwUrl = new UnityEngine.WWW(strFilePath);

                while (!wwwUrl.isDone)
                {
                    if (wwwUrl.error != null)
                    {
                        DebugWide.LogRed("error : " + wwwUrl.error.ToString());
                        yield break;
                    }
                    //DebugWide.LogGreen("wwwUrl.progress---" + wwwUrl.progress);
                    yield return null;
                }

                if (wwwUrl.isDone)
                {
                    //DebugWide.LogGreen("wwwUrl.isDone---size : "+wwwUrl.size);
                    DebugWide.LogGreen("wwwUrl.isDone---bytesLength : " + wwwUrl.bytes.Length);
                    memStream = new MemoryStream(wwwUrl.bytes);

                }
            }
#endif

            if (null != result)
            {
                result(memStream);
            }
            DebugWide.LogGreen("*** " + strFilePath + " ::: WWW Loading complete");
            yield return memStream;
        }

        public IEnumerator LoadXML(string fileName)
        {
            //내부 코루틴 부분
            //------------------------------------------------------------------------
            //DebugWide.LogBlue(GlobalConstants.ASSET_PATH + m_strFileName); //chamto test
            MemoryStream stream = null;
            //IEnumerator irator = this.FileLoading(GlobalConstants.ASSET_PATH + m_strFileName, value => stream = value);
            IEnumerator irator = this.FileLoading(Misc.ASSET_PATH + fileName, null);
            yield return irator;

            stream = irator.Current as MemoryStream; //이뮬레이터의 양보반환값을 가져온다
            if (null == stream)
            {
                DebugWide.Log("error : failed LoadFromFile : " + Misc.ASSET_PATH + fileName);
                yield break;
            }
            this.Parse_MemoryStream(stream);

        }

        //public IEnumerator SaveXML()
        public void SaveXML(string fileName)
        {
            XmlDocument Xmldoc = new XmlDocument();
            XmlDeclaration decl = Xmldoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            Xmldoc.AppendChild(decl);
            XmlElement root_element = Xmldoc.CreateElement("root");
            XmlElement first_element = null;
            XmlElement second_element = null;

            int count = 0;
            Vector2Int cell = Vector2Int.zero;
            //-----------------------------------
            foreach (ImageHashValue n in _uniqueImages.Values)
            {
                first_element = Xmldoc.CreateElement("ImageHash");
                first_element.SetAttribute("fileName", n.fileName);
                first_element.SetAttribute("kind", n.kind.ToString());


                second_element = Xmldoc.CreateElement("Info");
                cell.x = n.rect.x;
                cell.y = n.rect.y;
                second_element.SetAttribute("cellPos", cell.ToString());
                cell.x = n.rect.width;
                cell.y = n.rect.height;
                second_element.SetAttribute("cellSize", cell.ToString());
                second_element.SetAttribute("hash", n.hash);

                first_element.AppendChild(second_element);
                root_element.AppendChild(first_element);

                //count++;
                //DebugWide.LogBlue(n.fileName + "  : " + count); //print
                //yield return new WaitForEndOfFrame();
            }

            Xmldoc.AppendChild(root_element);
            Xmldoc.Save(Application.dataPath +"/StreamingAssets/" +fileName);

            DebugWide.LogBlue("=====================Saved!!=====================  전체추가: " + _uniqueImages.Count);
            //yield break;
        }

    }
}

