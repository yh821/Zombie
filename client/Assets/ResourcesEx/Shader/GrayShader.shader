Shader "Unlit/GrayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Saturation("Saturation",Range(0,1))=1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass
		{
		Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Saturation;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed gray = 0.2125 * col.r + 0.7154 * col.g + 0.0721 * col.b;
				fixed3 grayColor = fixed3(gray,gray,gray);
				 col.xyz = lerp(grayColor,col.xyz,_Saturation);
				return col;
			}
			ENDCG
		}
	}
}
