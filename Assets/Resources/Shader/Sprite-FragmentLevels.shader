Shader "Sprites/FragmentLevels"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

         //Add the Input Levels Values
        _inBlack ("Input Black", Range(0, 255)) = 0
        _inGamma ("Input Gamma", Range(0, 2)) = 1
        _inWhite ("Input White", Range(0, 255)) = 255
        
        //Add the Output Levels
        _outWhite ("Output White", Range(0, 255)) = 255
        _outBlack ("Output Black", Range(0, 255)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha


        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            //#pragma fragment SpriteFrag
            #pragma fragment SpriteFrag_Reversal
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            //Add these variables
            //to the CGPROGRAM
            float _inBlack;
            float _inGamma;
            float _inWhite;
            float _outWhite;
            float _outBlack;

            struct Input 
            {
                float2 uv_MainTex;
            };
            
            float GetPixelLevel(float pixelColor)
            {
                float pixelResult;
                pixelResult = (pixelColor * 255.0);
                pixelResult = max(0, pixelResult - _inBlack);
                pixelResult = saturate(pow(pixelResult / (_inWhite - _inBlack), _inGamma));
                pixelResult = (pixelResult * (_outWhite - _outBlack) + _outBlack)/255.0;    
                return pixelResult;
            }

            fixed4 SpriteFrag_Reversal(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                c.rgb *= c.a;

                //책 셰이더 프로그래밍 입문  -흑백- 258page 
                //c.rgb = dot(c.rgb, float3(0.3,0.59,0.11)); //흑백으로 만들기 : 빨강30% + 녹색59% + 파랑11%

                //책 셰이더 프로그래밍 입문  -세피아- 260page 
                //float4 sepia;
                //sepia.a = c.a;
                //sepia.r = dot(c.rgb, float3(0.393f, 0.769f, 0.189f));
                //sepia.g = dot(c.rgb, float3(0.349f, 0.686f, 0.168f));
                //sepia.b = dot(c.rgb, float3(0.272f, 0.534f, 0.131f));
                //c = sepia;

                //책 유니티 Shader와 Effect 제작 -포토샵 레벨이펙트- 86page
                float outRPixel  = GetPixelLevel(c.r);
                float outGPixel = GetPixelLevel(c.g);
                float outBPixel = GetPixelLevel(c.b);
                c.rgb = float3(outRPixel,outGPixel,outBPixel);


                return c;
            }
        ENDCG
        }
    }
}