Shader "Unlit/Sprite" {
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags 
		{
			"Queue" = "Transparent+1"
		}
		
		Pass
		{
			Lighting Off
			ZWrite Off
			//Cull Back
            Cull Off
            
			Fog
			{
				Mode Off
			}
			
			Blend SrcAlpha OneMinusSrcAlpha
			
			SetTexture[_MainTex]
			{
				Combine texture
			}
		}
	} 
	FallBack "Unlit/Texture"
}
