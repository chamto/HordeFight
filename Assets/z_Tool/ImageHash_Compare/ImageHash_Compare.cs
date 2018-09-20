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

public class ImageHash_Compare : MonoBehaviour 
{

    //<문자열 해쉬키 , 타일Colors>
    public Dictionary<string, Color[]> _uniqueTiles = new Dictionary<string, Color[]>();

	//void Start () 
	IEnumerator Start()
    {
        const int TILEMAP_WITH = 1024;
        const int TILEMAP_HEIGHT = 1024;
        const int TILE_SIZE = 16;
        const int TILE_MAX_COUNT = 1024 / 16; //64 
        Vector2Int pos = Vector2Int.zero;
        string temp = string.Empty;
        float startTime = Time.time;
        //===================================================
        Sprite dungeon = Resources.Load<Sprite>("Warcraft/Textures/Test/dungeon_01");
        if(null == dungeon) 
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
                if(0f ==  colors[ TILE_SIZE * TILE_SIZE / 2 ].a )
                {
                    continue;
                }
                tile.SetPixels(colors);
                byte[] raw = tile.EncodeToPNG();

                byte[] hash = md5.ComputeHash(raw);

                temp = string.Empty;
                foreach (byte one in hash)
                {
                    temp += one;
                }
                DebugWide.LogBlue(temp + "   " + hash.Length + "   " + pos ); //print

                Color[] out_color = null;
                if (false == _uniqueTiles.TryGetValue(temp, out out_color))
                {
                    _uniqueTiles.Add(temp, colors);
                }

                //File.WriteAllBytes(Application.dataPath + "/../test/tile"+pos.x +"_"+pos.y +".png", raw);

                yield return new WaitForEndOfFrame();
            }
        }


        DebugWide.LogBlue("done. unique tile find complete");


        Texture2D new_tilemap = new Texture2D(TILEMAP_WITH/2, TILEMAP_HEIGHT/2, TextureFormat.ARGB32, false);
        int count = 0;
        foreach (Color[] byby in _uniqueTiles.Values)
        {
            pos.x = (count % (TILE_MAX_COUNT/2)) * TILE_SIZE;
            pos.y = (count / (TILE_MAX_COUNT/2)) * TILE_SIZE;
            DebugWide.LogBlue(pos);
            new_tilemap.SetPixels(pos.x, pos.y, TILE_SIZE, TILE_SIZE, byby);

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

        DebugWide.LogBlue("done. file create complete +" + (Time.time - startTime)/1000 + "초 걸림");

        yield break;
	}
	
	// Update is called once per frame
	void Update () 
    {
		
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