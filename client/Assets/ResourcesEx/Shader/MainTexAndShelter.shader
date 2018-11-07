Shader "Unlit/MainTexAndShelter"
{
	Properties
	{
		_MainColor("MainColor",Color)= (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_RimColor("RimColor",Color)= (1,1,1,1)
		_RimStrength("RimStrength",Range(0.1,3)) = 3
		_DisAlpha("DisAlpha",Range(0,1)) = 0.5
		_DamageFX("Damage FX", Float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass
			{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Greater
			ZWrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal:NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 viewDir :TEXCOORD1;
				float3 normal:NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainColor;
			float4 _MainTex_ST;
			float _RimStrength;
			float _DisAlpha;
			float4 _RimColor;
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal= UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

				


				float rim =max(0.5,( 1 - abs(dot(i.normal, normalize(i.viewDir)))* _RimStrength));
				fixed4 col = fixed4(1,1,1,1);
				col.rgb = _RimColor.rgb*rim*1.2;
				col.a *= _DisAlpha*_MainColor.a;

				//col = fixed4(1, 1, 0, 1)*rim*1.2;
				//col = fixed4(1, 1, 0, 1);


			return col;
			}
				ENDCG
			}
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
			float4 _MainColor;
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
		 
				col.a = _MainColor.a;
				return col;
			}
			ENDCG
		}
	}
}
