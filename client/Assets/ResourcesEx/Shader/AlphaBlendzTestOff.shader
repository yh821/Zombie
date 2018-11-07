Shader "Unlit/AlphaBlendzTestOff"
{
	Properties
	{
		_TintColor("Tint Color",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		 _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"  }
		

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			 Cull Off ZTest off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			

			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				 fixed4 color : COLOR;
				  UNITY_VERTEX_INPUT_INSTANCE_ID

			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				
				float4 vertex : SV_POSITION;
				fixed4 color:COLOR;

				 #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;
			v2f vert (appdata v)
			{
				v2f o;
				 UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                #ifdef SOFTPARTICLES_ON
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
                #endif



				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _TintColor;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				#ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
                #endif
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv)*i.color;
				
				return col;
			}
			ENDCG
		}
	}
}
