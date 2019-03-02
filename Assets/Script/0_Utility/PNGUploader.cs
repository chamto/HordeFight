using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;



//ref : https://docs.unity3d.com/kr/530/ScriptReference/Texture2D.EncodeToPNG.html
//인코딩된 PNG 데이터는 /ARGB32/텍스쳐에서는 알파 채널을 포함하고, /RGB24/에서는 알파채널을 포함하지 않습니다. PNG 데이터는 감마 보정(gamma correction)을 포함하지 않거나 색상 프로파일 정보가 없습니다.
//현재 랜더링 화면을 파일로 출력한다
//public class PNGUploader : MonoBehaviour
//{
//    // Take a shot immediately
//    IEnumerator Start()
//    {
//        yield return UploadPNG();
//    }

//    IEnumerator UploadPNG()
//    {
//        // We should only read the screen buffer after rendering is complete
//        yield return new WaitForEndOfFrame();

//        // Create a texture the size of the screen, RGB24 format
//        int width = Screen.width;
//        int height = Screen.height;
//        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

//        // Read screen contents into the texture
//        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//        tex.Apply();

//        // Encode texture into PNG
//        byte[] bytes = tex.EncodeToPNG();
//        UnityEngine.Object.Destroy(tex);

//        // For testing purposes, also write to a file in the project folder
//        File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

//        DebugWide.LogBlue(Application.dataPath + "/../SavedScreen.png");

//    }

//}
