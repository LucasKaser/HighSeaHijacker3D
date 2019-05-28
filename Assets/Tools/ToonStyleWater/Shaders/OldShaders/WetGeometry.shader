Shader "ToonWater/Legacy/WetGeometry" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Tex (RGB)", 2D) = "white" {}
	}

	SubShader {

		ColorMask RGBA
			
		Tags { 
			"RenderType"="Opaque" 
			"LightMode"="ForwardBase"
		}
		LOD 200
		
		CGPROGRAM
	
		#pragma surface surf SimpleSpecular fullforwardshadows vertex:vert keepalpha nofog nometa interpolateview
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Foam;
		float4 _foamParams;
		float4 _foamDynamics;  
		float4 _Color;
		float4 _ShadowColor;

		struct Input {
			float2 uv_MainTex;
			float4 wpos;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v, out Input b) {
			float4 posWorld = mul( unity_ObjectToWorld, v.vertex );
			b.uv_MainTex = v.texcoord.xy/_foamDynamics.z;
			b.wpos = posWorld;
			b.wpos.y -= _foamParams.z;
			b.wpos.y*=_foamDynamics.x; 
			b.wpos.y+= 128/ _foamDynamics.x;
			b.wpos.xz*=_foamDynamics.y; 
			b.wpos/=_foamDynamics.z;
			b.wpos.y+=1;
		}

		struct MySurfaceOutput {
			float3 Albedo; 
			float3 Normal;  
			float3 Emission;
			float Alpha;  
			float2 foamWhite;
		};


		void surf (Input IN, inout MySurfaceOutput o) {

			float4 c = tex2D (_MainTex, IN.uv_MainTex);
			float4 foamMap = tex2D (_Foam, IN.wpos.xz*0.1);

			float l = cos(_foamParams.x+IN.wpos.x+foamMap.a*4) -IN.wpos.y;
			float dl = max(0,0.2-abs(l));

			float l1 = sin(_foamParams.y+IN.wpos.z-foamMap.a*2) -IN.wpos.y;
			float dl1 = max(0,(0.3-abs(l1))*max(0,1-l));

			float foamAlpha = (dl+dl1);
			o.foamWhite.x = saturate(max(l,l1)*8);
			o.foamWhite.y = (1-c.a)*0.25;
			
			o.Albedo = c.rgb;

			o.Alpha = saturate(IN.wpos.y+2-(max(0,l)+max(0,l1) -max(0,foamAlpha)*(_foamDynamics.w) )*2);
		}


		half4 LightingSimpleSpecular (MySurfaceOutput s, float3 lightDir, float3 viewDir, float atten) {
              float NdotL =  min((dot (s.Normal, lightDir)-s.foamWhite.y)*64 , 1);
              float4 c;
            
			atten = saturate((atten-s.foamWhite.y)*16);

			float lightBlock = max(NdotL*atten, _ShadowColor.a);

			c.rgb = (s.Albedo * _LightColor0.rgb*_Color) * lightBlock
				+s.foamWhite.x*atten
				+(_ShadowColor.rgb)*(1 - lightBlock);

              c.a = s.Alpha;

              return c;
        }

		ENDCG
	}
	FallBack "Diffuse"
}
