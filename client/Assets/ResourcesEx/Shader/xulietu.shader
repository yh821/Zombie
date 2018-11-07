// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.35 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.35;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4088,x:32775,y:32643,varname:node_4088,prsc:2|custl-3539-OUT,alpha-9187-OUT;n:type:ShaderForge.SFN_Tex2d,id:3241,x:32387,y:32643,ptovrint:False,ptlb:node_3241,ptin:_node_3241,varname:_node_3241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:babfdd593be6efc45b08bb4560f37253,ntxv:0,isnm:False|UVIN-7822-UVOUT;n:type:ShaderForge.SFN_UVTile,id:7822,x:32210,y:32677,varname:node_7822,prsc:2|UVIN-3440-OUT,WDT-7741-OUT,HGT-3425-OUT,TILE-1711-OUT;n:type:ShaderForge.SFN_Append,id:3440,x:32046,y:32588,varname:node_3440,prsc:2|A-739-U,B-8984-OUT;n:type:ShaderForge.SFN_TexCoord,id:739,x:31697,y:32562,varname:node_739,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:8984,x:31885,y:32647,varname:node_8984,prsc:2,frmn:0,frmx:1,tomn:1,tomx:0|IN-739-V;n:type:ShaderForge.SFN_Negate,id:3425,x:31885,y:32838,varname:node_3425,prsc:2|IN-2412-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7741,x:31673,y:32769,ptovrint:False,ptlb:u,ptin:_u,varname:_u,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_ValueProperty,id:2412,x:31673,y:32898,ptovrint:False,ptlb:v,ptin:_v,varname:_v,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_ValueProperty,id:1711,x:31865,y:33039,ptovrint:False,ptlb:shuzi,ptin:_shuzi,varname:_shuzi,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:9;n:type:ShaderForge.SFN_Color,id:2730,x:32346,y:32901,ptovrint:False,ptlb:node_2730,ptin:_node_2730,varname:_node_2730,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.7058823,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:3539,x:32621,y:32643,varname:node_3539,prsc:2|A-3241-RGB,B-2730-RGB;n:type:ShaderForge.SFN_Multiply,id:9187,x:32572,y:32851,varname:node_9187,prsc:2|A-3241-R,B-2730-A;proporder:3241-7741-2412-1711-2730;pass:END;sub:END;*/

Shader "Custom/xulietu" {
    Properties {
        _node_3241 ("node_3241", 2D) = "white" {}
        _u ("u", Float ) = 3
        _v ("v", Float ) = 3
        _shuzi ("shuzi", Float ) = 9
        _node_2730 ("node_2730", Color) = (0.7058823,0,0,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _node_3241; uniform float4 _node_3241_ST;
            uniform float _u;
            uniform float _v;
            uniform float _shuzi;
            uniform float4 _node_2730;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
                float2 node_7822_tc_rcp = float2(1.0,1.0)/float2( _u, (-1*_v) );
                float node_7822_ty = floor(_shuzi * node_7822_tc_rcp.x);
                float node_7822_tx = _shuzi - _u * node_7822_ty;
                float2 node_7822 = (float2(i.uv0.r,(i.uv0.g*-1.0+1.0)) + float2(node_7822_tx, node_7822_ty)) * node_7822_tc_rcp;
                float4 _node_3241_var = tex2D(_node_3241,TRANSFORM_TEX(node_7822, _node_3241));
                float3 finalColor = (_node_3241_var.rgb*_node_2730.rgb);
                fixed4 finalRGBA = fixed4(finalColor,(_node_3241_var.r*_node_2730.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
