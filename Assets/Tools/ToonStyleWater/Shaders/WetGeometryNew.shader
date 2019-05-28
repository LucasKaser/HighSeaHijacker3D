Shader "ToonWater/WetGeometryNew" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Tex (RGB)", 2D) = "white" {}
	}

	Category{
		Tags{ 
			"RenderType" = "Opaque"
			"LightMode" = "ForwardBase"
			"Queue" = "Geometry"
		}
		LOD 200
		ColorMask RGBA

		SubShader{
			Pass{

				CGPROGRAM

				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc" 
				#include "Lighting.cginc"
				#include "AutoLight.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog

				#pragma multi_compile_fwdbase

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _Foam;
				float4 _foamParams;
				float4 _foamDynamics;
				float4 _Color;
				float4 _ShadowColor;

				struct v2f {
					float4 pos : POSITION;
					UNITY_FOG_COORDS(1)
					float3 viewDir : TEXCOORD2;
					float4 wpos : TEXCOORD3;
					SHADOW_COORDS(4)
					float3 normal : TEXCOORD5;
					float2 texcoord : TEXCOORD6;
				};

				v2f vert(appdata_full v) {
					v2f o;

					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

					o.pos = UnityObjectToClipPos(v.vertex);
					o.wpos = worldPos;
					o.viewDir.xyz = (WorldSpaceViewDir(v.vertex));

					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o, o.pos);
					TRANSFER_SHADOW(o);

					float3 worldNormal = UnityObjectToWorldNormal(v.normal);

					o.normal = normalize(worldNormal);

					o.wpos = worldPos;
					o.wpos.y -= _foamParams.z;
					o.wpos.y *= _foamDynamics.x;
					o.wpos.y += 128 / _foamDynamics.x;
					o.wpos.xz *= _foamDynamics.y;
					o.wpos /= _foamDynamics.z;
					o.wpos.y += 1;

					return o;
				}


				float4 frag(v2f i) : COLOR{

					i.viewDir = normalize(i.viewDir.xyz);
				
					float4 col = tex2D(_MainTex, i.texcoord);

				// FOAM

					float4 foamMap = tex2D(_Foam, i.wpos.xz*0.1);

					float l = cos(_foamParams.x + i.wpos.x + foamMap.a * 4) - i.wpos.y;
					float dl = max(0, 0.2 - abs(l));

					float l1 = sin(_foamParams.y + i.wpos.z - foamMap.a * 2) - i.wpos.y;
					float dl1 = max(0, (0.3 - abs(l1))*max(0, 1 - l));

					float foamAlpha = (dl + dl1);
					float2 foamWhite;
					foamWhite.x = saturate(max(l, l1) * 8);
					foamWhite.y = (1 - col.a)*0.25;

	
				// LIGHT
	
					float direct = saturate(dot(_WorldSpaceLightPos0, i.normal) ) * SHADOW_ATTENUATION(i);

					float wet = 128 * col.a * foamWhite.x;

					_LightColor0.rgb *= direct;

					col.rgb = col.rgb*((ShadeSH9(float4(i.normal, 1)).rgb * direct + _ShadowColor.rgb*(1-direct)) + _LightColor0.rgb);

					col.rgb = saturate(foamWhite.x + (1 - foamWhite.x)*col.rgb);

					UNITY_APPLY_FOG(i.fogCoord, col);

					col.a = saturate(i.wpos.y + 2 - (max(0, l) + max(0, l1) - max(0, foamAlpha)*(_foamDynamics.w)) * 2);

				return col;
				}
				ENDCG
			}
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		}
	}
}
