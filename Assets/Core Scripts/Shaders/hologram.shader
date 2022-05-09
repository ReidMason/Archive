// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.36 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.36;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:1,bsrc:3,bdst:7,culm:2,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32661,y:32678|emission-366-OUT,alpha-319-OUT,olwid-416-OUT,olcol-56-RGB,voffset-112-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33456,y:32757,ptlb:MovingTex1,ptin:_MovingTex1,tex:f99becf018bd18e4e8252bb6108eadb6,ntxv:2,isnm:False|UVIN-3-UVOUT;n:type:ShaderForge.SFN_Panner,id:3,x:33662,y:32815,spu:0,spv:1|DIST-4-TSL;n:type:ShaderForge.SFN_Time,id:4,x:33854,y:32871;n:type:ShaderForge.SFN_Tex2d,id:10,x:33456,y:32940,ptlb:MovingTex2,ptin:_MovingTex2,tex:46fc31b92fe8ca8459c2af380aae5f83,ntxv:0,isnm:False|UVIN-14-UVOUT;n:type:ShaderForge.SFN_Time,id:12,x:33854,y:33010;n:type:ShaderForge.SFN_Panner,id:14,x:33662,y:32975,spu:0,spv:-1|DIST-12-TSL;n:type:ShaderForge.SFN_Color,id:19,x:33456,y:32558,ptlb:MovingTex1Color,ptin:_MovingTex1Color,glob:False,c1:0.9862069,c2:1,c3:0,c4:1;n:type:ShaderForge.SFN_Blend,id:22,x:33264,y:32705,blmd:10,clmp:True|SRC-19-RGB,DST-2-G;n:type:ShaderForge.SFN_Blend,id:29,x:33233,y:32940,blmd:10,clmp:True|SRC-31-RGB,DST-10-G;n:type:ShaderForge.SFN_Color,id:31,x:33467,y:33150,ptlb:MovingTex2Color,ptin:_MovingTex2Color,glob:False,c1:0.8639855,c2:0.9264706,c3:0.2793036,c4:1;n:type:ShaderForge.SFN_Add,id:32,x:33036,y:32784|A-22-OUT,B-29-OUT;n:type:ShaderForge.SFN_Add,id:46,x:32984,y:32909|A-2-G,B-10-G;n:type:ShaderForge.SFN_Vector1,id:54,x:33155,y:33101,v1:0.3;n:type:ShaderForge.SFN_Color,id:56,x:33296,y:33131,ptlb:OutlineColor,ptin:_OutlineColor,glob:False,c1:0.5039672,c2:0.4894031,c3:0.9117647,c4:1;n:type:ShaderForge.SFN_Panner,id:99,x:33576,y:33321,spu:0,spv:-1|DIST-101-TSL;n:type:ShaderForge.SFN_Time,id:101,x:33794,y:33269;n:type:ShaderForge.SFN_Tex2d,id:103,x:33341,y:33314,ptlb:VertexFlickerNM,ptin:_VertexFlickerNM,tex:bbab0a6f7bae9cf42bf057d8ee2755f6,ntxv:3,isnm:True|UVIN-99-UVOUT;n:type:ShaderForge.SFN_Multiply,id:112,x:32851,y:33277|A-103-RGB,B-113-OUT;n:type:ShaderForge.SFN_Vector1,id:113,x:33109,y:33458,v1:0.3;n:type:ShaderForge.SFN_Add,id:319,x:32958,y:33101|A-46-OUT,B-320-B;n:type:ShaderForge.SFN_Color,id:320,x:33087,y:33199,ptlb:Transparency,ptin:_Transparency,glob:False,c1:0.4632353,c2:0.4632353,c3:0.4632353,c4:1;n:type:ShaderForge.SFN_Add,id:366,x:32929,y:32632|A-401-OUT,B-32-OUT;n:type:ShaderForge.SFN_Tex2d,id:368,x:33264,y:32551,ptlb:Wireframe,ptin:_Wireframe,tex:ef53c231aa27f0c4cb4ff6337e3fed66,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:401,x:32976,y:32462|A-368-G,B-403-OUT;n:type:ShaderForge.SFN_Slider,id:403,x:33254,y:32439,ptlb:WireframeStrength,ptin:_WireframeStrength,min:0,cur:0,max:20;n:type:ShaderForge.SFN_Slider,id:416,x:33393,y:33625,ptlb:OutlineThickness,ptin:_OutlineThickness,min:0,cur:0.3,max:1;proporder:2-19-10-31-56-103-320-368-403-416;pass:END;sub:END;*/

Shader "Shader Forge/hologram" {
    Properties {
        _MovingTex1 ("MovingTex1", 2D) = "black" {}
        _MovingTex1Color ("MovingTex1Color", Color) = (0.9862069,1,0,1)
        _MovingTex2 ("MovingTex2", 2D) = "white" {}
        _MovingTex2Color ("MovingTex2Color", Color) = (0.8639855,0.9264706,0.2793036,1)
        _OutlineColor ("OutlineColor", Color) = (0.5039672,0.4894031,0.9117647,1)
        _VertexFlickerNM ("VertexFlickerNM", 2D) = "bump" {}
        _Transparency ("Transparency", Color) = (0.4632353,0.4632353,0.4632353,1)
        _Wireframe ("Wireframe", 2D) = "white" {}
        _WireframeStrength ("WireframeStrength", Range(0, 20)) = 0
        _OutlineThickness ("OutlineThickness", Range(0, 1)) = 0.3
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            uniform float4 _TimeEditor;
            uniform float4 _OutlineColor;
            uniform sampler2D _VertexFlickerNM; uniform float4 _VertexFlickerNM_ST;
            uniform float _OutlineThickness;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                float4 node_101 = _Time + _TimeEditor;
                float2 node_99 = (o.uv0.rg+node_101.r*float2(0,-1));
                v.vertex.xyz += (UnpackNormal(tex2Dlod(_VertexFlickerNM,float4(TRANSFORM_TEX(node_99, _VertexFlickerNM),0.0,0))).rgb*0.3);
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal*_OutlineThickness,1));
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                return fixed4(_OutlineColor.rgb,0);
            }
            ENDCG
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            uniform float4 _TimeEditor;
            uniform sampler2D _MovingTex1; uniform float4 _MovingTex1_ST;
            uniform sampler2D _MovingTex2; uniform float4 _MovingTex2_ST;
            uniform float4 _MovingTex1Color;
            uniform float4 _MovingTex2Color;
            uniform sampler2D _VertexFlickerNM; uniform float4 _VertexFlickerNM_ST;
            uniform float4 _Transparency;
            uniform sampler2D _Wireframe; uniform float4 _Wireframe_ST;
            uniform float _WireframeStrength;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                float4 node_101 = _Time + _TimeEditor;
                float2 node_427 = o.uv0;
                float2 node_99 = (node_427.rg+node_101.r*float2(0,-1));
                v.vertex.xyz += (UnpackNormal(tex2Dlod(_VertexFlickerNM,float4(TRANSFORM_TEX(node_99, _VertexFlickerNM),0.0,0))).rgb*0.3);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float2 node_427 = i.uv0;
                float4 node_4 = _Time + _TimeEditor;
                float2 node_3 = (node_427.rg+node_4.r*float2(0,1));
                float4 node_2 = tex2D(_MovingTex1,TRANSFORM_TEX(node_3, _MovingTex1));
                float4 node_12 = _Time + _TimeEditor;
                float2 node_14 = (node_427.rg+node_12.r*float2(0,-1));
                float4 node_10 = tex2D(_MovingTex2,TRANSFORM_TEX(node_14, _MovingTex2));
                float3 emissive = ((tex2D(_Wireframe,TRANSFORM_TEX(node_427.rg, _Wireframe)).g*_WireframeStrength)+(saturate(( node_2.g > 0.5 ? (1.0-(1.0-2.0*(node_2.g-0.5))*(1.0-_MovingTex1Color.rgb)) : (2.0*node_2.g*_MovingTex1Color.rgb) ))+saturate(( node_10.g > 0.5 ? (1.0-(1.0-2.0*(node_10.g-0.5))*(1.0-_MovingTex2Color.rgb)) : (2.0*node_10.g*_MovingTex2Color.rgb) ))));
                float3 finalColor = emissive;
/// Final Color:
                return fixed4(finalColor,((node_2.g+node_10.g)+_Transparency.b));
            }
            ENDCG
        }
        Pass {
            Name "ShadowCollector"
            Tags {
                "LightMode"="ShadowCollector"
            }
            Cull Off
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCOLLECTOR
            #define SHADOW_COLLECTOR_PASS
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcollector
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            uniform float4 _TimeEditor;
            uniform sampler2D _VertexFlickerNM; uniform float4 _VertexFlickerNM_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_COLLECTOR;
                float2 uv0 : TEXCOORD5;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                float4 node_101 = _Time + _TimeEditor;
                float2 node_99 = (o.uv0.rg+node_101.r*float2(0,-1));
                v.vertex.xyz += (UnpackNormal(tex2Dlod(_VertexFlickerNM,float4(TRANSFORM_TEX(node_99, _VertexFlickerNM),0.0,0))).rgb*0.3);
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_COLLECTOR(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                SHADOW_COLLECTOR_FRAGMENT(i)
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Cull Off
            Offset 1, 1
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            uniform float4 _TimeEditor;
            uniform sampler2D _VertexFlickerNM; uniform float4 _VertexFlickerNM_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                float4 node_101 = _Time + _TimeEditor;
                float2 node_99 = (o.uv0.rg+node_101.r*float2(0,-1));
                v.vertex.xyz += (UnpackNormal(tex2Dlod(_VertexFlickerNM,float4(TRANSFORM_TEX(node_99, _VertexFlickerNM),0.0,0))).rgb*0.3);
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
