Shader "Unlit/AlphaSheetSingle"
{
	Properties
	{
		_MainColor("MainColor",Color)= (1,0,0,1)
		_MainTex ("Texture", 2D) = "white" {}
		_horizontalCout("horizontalCout",Float)= 1
		_verticalCout("vertical",Float) = 1
		_N("The N Spite",Float) = 1
		
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"= "Transparent"  }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite off
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
			float4 _MainColor;
			float4 _MainTex_ST;
			float _horizontalCout;
			float _verticalCout;
			float _N;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				float2 MTOffset = float2( _MainTex_ST.x/_horizontalCout ,_MainTex_ST.y/_verticalCout);

				o.uv = v.uv.xy*_MainTex_ST.xy + _MainTex_ST.zw;
				o.uv.x *= 1/_horizontalCout;
				o.uv.y *= 1/_verticalCout;
				o.uv.x+=(_N%_horizontalCout)*(1/_horizontalCout);
				o.uv.y+=floor(_N/_verticalCout)*(1/_verticalCout);
					
		
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col*= _MainColor;
				//col.a = col.r*col.a;
				
				return col;
			}
			ENDCG
		}
	}
}
