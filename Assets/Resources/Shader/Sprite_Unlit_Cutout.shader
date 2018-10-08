
//20180428 - chamto
//builtin_shaders-2017.3.0f3 버전의 Sprites/Default 쉐이더를 수정하여 컷아웃 쉐이더로 바꾸었음
//깊이버퍼 쓰기 모드 활성 : 스프라이트를 3차원 상에서 표현하기 위함
//알파블렌딩 비활성 : 알파테스트 방식으로 사용하기 위해 끔
//UnitySprites.cginc 와의 호환성을 잃지 않기 위하여,  SpriteFrag 코드를 그대로 가져와 clip 처리만 추가하였다.
//
//유니티에 내장된 "Unlit/Transparent Cutout" 쉐이더를 사용해도 똑같이 표현할 수 있음. 
//스프라이트 쉐이더를 수정하여 사용하는 이유는, 스프라이트 객체의 최적화 처리를 사용하기 위해서다.
//UnitySprites.cginc 내부에서 상수버퍼(HLSL:cbuffer)를 사용하고 있다. 
//c#코드와 연결된 상수버퍼 처리를 그대로 사용하기 위해서 기존쉐이더를 수정한 것이다. 

Shader "Sprites/Unlit_Cutout"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

    	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"

            //"Queue"="Transparent" "RenderType"="Transparent" 
            "Queue"="AlphaTest" "RenderType"="TransparentCutout" 
        }

        Cull Off
        Lighting Off

        //ZWrite Off
        //Blend One OneMinusSrcAlpha


        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            //#pragma fragment SpriteFrag
            #pragma fragment SpriteFrag_CutOut
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"


			fixed _Cutoff;
			fixed4 SpriteFrag_CutOut(v2f IN) : SV_Target
			{
			    fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;

			    c.rgb *= c.a;

			    //추가한 코드
			    clip(c.a - _Cutoff); //지정범위의 프레그먼트를 폐기한다

			    return c;
			}
            
        ENDCG
        }
    }
}