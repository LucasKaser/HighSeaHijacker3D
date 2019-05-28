Shader "ToonWater/WaterShaderNew" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
	}

		Category{
		Tags{ "RenderType" = "Transparent"
		"LightMode" = "ForwardBase"
		"Queue" = "Geometry+50"
	}
		LOD 200
		Blend OneMinusDstAlpha DstAlpha // This makes water invisible
		ColorMask RGBA


		SubShader{
		Pass{



		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "UnityLightingCommon.cginc" 
#include "Lighting.cginc"
#include "UnityCG.cginc"
#include "AutoLight.cginc"

#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

	float4 _Color;

	struct v2f {
		float4 pos : POSITION;
		float4 viewDir : TEXCOORD1; 
		float3 wpos : TEXCOORD2;
		UNITY_FOG_COORDS(3)
		SHADOW_COORDS(4)
	};

	v2f vert(appdata_full v) {
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
		o.viewDir.xyz = WorldSpaceViewDir(v.vertex);

		o.viewDir.w = 0;
		UNITY_TRANSFER_FOG(o, o.pos);
		TRANSFER_SHADOW(o);

		float3 worldNormal = UnityObjectToWorldNormal(v.normal);

		return o;
	}


	float4 frag(v2f i) : COLOR{

	float dist = length(i.wpos.xyz - _WorldSpaceCameraPos.xyz);

	i.viewDir.xyz = normalize(i.viewDir.xyz);

	float3 normal = normalize(float3(0,1,0));

	float3 preDot = normal * i.viewDir.xyz;

	float dotprod = (preDot.x + preDot.y + preDot.z);

	float3 reflected = normalize(i.viewDir.xyz - 2 * (dotprod)*normal);

	float shadow = SHADOW_ATTENUATION(i);

	float dott = max(0.1,dot(_WorldSpaceLightPos0, -reflected));

	//dotprod = (1 - dotprod);

	//float3 halfDirection = normalize(i.viewDir.xyz + _WorldSpaceLightPos0.xyz);
//	float NdotH = max(0.01, (dot(float3(0,1,0), halfDirection)));
	//float normTerm =  saturate(( dott - 0.99) * 256);


	float4 cont = 0;

	cont.rgb = _Color.rgb*(ShadeSH9(float4(0, 1, 0, 1)) + _LightColor0.rgb * shadow);// +_LightColor0.rgb * normTerm;



	UNITY_APPLY_FOG(i.fogCoord, cont);

	return cont;

	}


		ENDCG
	}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
	}
}

