using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;
using System.Xml;

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
        public IEnumerator CreateImage_AddPadding(string imgFileName, Vector2Int cellSize, Vector2Int padding)
        {

            float startTime = Time.time;
            Vector2Int pos = Vector2Int.zero;
            Vector2Int TILE_COUNT = Vector2Int.zero;
            Vector2Int newImgSize = Vector2Int.zero;
            //===================================================
            if(false == _spriteMap.Keys.Contains(imgFileName))
            {
                DebugWide.LogRed("File not found!! " + imgFileName);
                yield break;
            }
            Sprite ori_image = _spriteMap[imgFileName];

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


            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/" + imgFileName + "_addPadding.png", new_raw);

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
        public IEnumerator CreatePNG_ImageHashMap(string imgFileName, eTileMap_Kind createKind, Vector2Int padding)
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
            Texture2D new_tilemap = new Texture2D(IMG_SIZE.x, IMG_SIZE.y, TextureFormat.ARGB32, false);

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



            byte[] new_raw = new_tilemap.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/Warcraft/Textures/TileMap/" + imgFileName + ".png", new_raw);

            DebugWide.LogBlue("done. file create complete +" + ((Time.time - startTime) / 60f).ToString() + "분 걸림");

            yield break;
        }

        /// <summary>
        /// 이미지해쉬맵의 객체컬러값을 갱신한다
        /// 파일에서 불러온 이미지해쉬맵의 아이템은 컬러값이 없기 떄문에, 따로 채워주어야 한다  
        /// </summary>
        public void UpdateAllColors()
        {
            foreach(ImageHashValue hv in _uniqueImages.Values)
            {
                Sprite tileMap_spr = _spriteMap[hv.fileName];
                hv.colors = tileMap_spr.texture.GetPixels(hv.rect.x, hv.rect.y, hv.rect.width, hv.rect.height);
            }

            DebugWide.LogBlue("=============UpdateAllColors=============");
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

                StartCoroutine(MakeUp_ImageHashMap("dungeon_01_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("dungeon_02_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("dungeon_03_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("dungeon_04_addPadding", eTileMap_Kind.Dungeon, CELL_SIZE, PADDING));

                //StartCoroutine(MakeUp_ImageHashMap("forest_01_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("forest_02_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("forest_03_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("forest_04_addPadding", eTileMap_Kind.Forest, CELL_SIZE, PADDING));

                //StartCoroutine(MakeUp_ImageHashMap("swamp_01_addPadding", eTileMap_Kind.Swamp, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("swamp_02_addPadding", eTileMap_Kind.Swamp, CELL_SIZE, PADDING));
                //StartCoroutine(MakeUp_ImageHashMap("swamp_03_addPadding", eTileMap_Kind.Swamp, CELL_SIZE, PADDING));

                //코루틴 때문에 수행되는 시기가 처음부분이다 <= 정상수행 안됨 
                //_parser.SaveXML();
                //DebugWide.LogBlue("Saved!!");
            }

            if (GUI.Button(new Rect(10, 230, 200, 100), new GUIContent("CreatePNG_ImageHashMap", icon)))
            {
                Vector2Int PADDING = new Vector2Int(2, 2);

                StartCoroutine(CreatePNG_ImageHashMap("dungeon", eTileMap_Kind.Dungeon, PADDING));
                StartCoroutine(CreatePNG_ImageHashMap("forest", eTileMap_Kind.Forest, PADDING));
                StartCoroutine(CreatePNG_ImageHashMap("swamp", eTileMap_Kind.Swamp, PADDING));


            }

            if (GUI.Button(new Rect(10, 340, 200, 100), new GUIContent("Save", icon)))
            {
                //StartCoroutine(_parser.SaveXML());
                _parser.SaveXML();

            }

            if (GUI.Button(new Rect(220, 340, 200, 100), new GUIContent("Load", icon)))
            {
                HordeFight.Single.coroutine.Start_Sync(_parser.LoadXML(), null, "ImageHashMap");

                UpdateAllColors();
            }
        }

    }



}

namespace Tool
{
    public class XML_Parser
    {
        private string m_strFileName = "ImageHashMap.xml";
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
                                    Vector2Int v2Int =  Utility.Misc.StringToVector2Int(n.Value);
                                    item.rect.x = v2Int.x;
                                    item.rect.y = v2Int.y;
                                }    
                                break;
                            case "cellSize":
                                {
                                    Vector2Int v2Int = Utility.Misc.StringToVector2Int(n.Value);
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

        public IEnumerator LoadXML()
        {
            //내부 코루틴 부분
            //------------------------------------------------------------------------
            //DebugWide.LogBlue(GlobalConstants.ASSET_PATH + m_strFileName); //chamto test
            MemoryStream stream = null;
            //IEnumerator irator = this.FileLoading(GlobalConstants.ASSET_PATH + m_strFileName, value => stream = value);
            IEnumerator irator = this.FileLoading(Utility.Misc.ASSET_PATH + m_strFileName, null);
            yield return irator;

            stream = irator.Current as MemoryStream; //이뮬레이터의 양보반환값을 가져온다
            if (null == stream)
            {
                DebugWide.Log("error : failed LoadFromFile : " + Utility.Misc.ASSET_PATH + m_strFileName);
                yield break;
            }
            this.Parse_MemoryStream(stream);

        }

        //public IEnumerator SaveXML()
        public void SaveXML()
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
            Xmldoc.Save(Application.dataPath +"/StreamingAssets/" +m_strFileName);

            DebugWide.LogBlue("=====================Saved!!=====================");
            //yield break;
        }

    }
}


namespace Tool
{
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
            UnityEngine.Object.Destroy(tex);

            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

            DebugWide.LogBlue(Application.dataPath + "/../SavedScreen.png");


        }

    }
}