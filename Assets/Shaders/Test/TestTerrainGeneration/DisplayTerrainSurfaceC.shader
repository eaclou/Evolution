// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/DisplayTerrainSurfaceC" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_RockHeightDetailTex ("Rock Height Detail", 2D) = "white" {}
		_SediHeightDetailTex ("Sedi Height Detail", 2D) = "white" {}
		_SnowHeightDetailTex ("Snow Height Detail", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		//_RemapTex ("Remap Tex", 2D) = "gray" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_PriHueRock ("PriHueRock", Color) = (1,1,1,1)
		_SecHueRock ("SecHueRock", Color) = (1,1,1,1)
		_PriHueSedi ("PriHueSedi", Color) = (1,1,1,1)
		_SecHueSedi ("SecHueSedi", Color) = (1,1,1,1)
		_PriHueSnow ("PriHueSnow", Color) = (1,1,1,1)
		_SecHueSnow ("SecHueSnow", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#include "UnityCG.cginc"
		#include "Assets/Shaders/Inc/NoiseShared.cginc"
		#include "Assets/Shaders/Inc/StrataRemap.cginc"

		#pragma only_renderers d3d11
		#pragma surface surf Standard fullforwardshadows
		#pragma vertex vert
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0


		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _RockHeightDetailTex;
		sampler2D _SediHeightDetailTex;
		sampler2D _SnowHeightDetailTex;
		//sampler2D _RemapTex;

		struct v2f {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
			float4 color : COLOR;

			uint id : SV_VERTEXID;
			uint inst : SV_INSTANCEID;
		};

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
			float4 color : COLOR;
		};

		struct RockStrataData {
			float3 color;
			float hardness;
		};

		float _MinAltitude;
		float _MaxAltitude;
		float _RemapOffset;
		float _NumStrata;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float4 _PriHueRock;
		float4 _SecHueRock;
		float4 _PriHueSedi;
		float4 _SecHueSedi;
		float4 _PriHueSnow;
		float4 _SecHueSnow;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		float rand(float2 co){
			return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
		}

		#ifdef SHADER_API_D3D11
        StructuredBuffer<RockStrataData> rockStrataDataCBuffer;
        #endif
		
		float3 GetRockHue(float3 primaryHue, float3 secondaryHue, float altitude, float3 rockDetailSample) {
			float3 hue = lerp(secondaryHue, primaryHue, saturate(sin(altitude * 0.07431) * 0.5 + 0.5));
			hue = lerp(hue, rockDetailSample, 0.015);
			return hue;
			//float3 rockHue = lerp(_SecHueRock.rgb, _PriHueRock.rgb, saturate(sin(IN.worldPos.y * 0.07431) * 0.5 + 0.5));
			//rockHue = lerp(rockHue, rockDetailSample.xyz, 0.015);
		}

		void vert (inout v2f v, out Input o) {				
			UNITY_INITIALIZE_OUTPUT(Input,o);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			
			float2 customUV = float2(IN.worldPos.x / 680 * 20 + 0.5, IN.worldPos.z / 680 * 20 + 0.5);
			float4 customHeightSample = tex2D (_MainTex, customUV);
			float slopeMultiplier = -3;
			float gradCustomHeightX = ddx(customHeightSample.x) * slopeMultiplier;
			float gradCustomHeightY = ddy(customHeightSample.x) * slopeMultiplier;
			float gradCustomHeight = length(float2(gradCustomHeightX, gradCustomHeightY));
			float3 customHeightNormal = cross(normalize(float3(1, 0, gradCustomHeightX)), normalize(float3(0, 1, gradCustomHeightY)));

			//o.Albedo = c.rgb;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			
			//o.Albedo = IN.worldPos;
			float3 strataColorNext;
			float3 strataColorPrev;
			float3 strataColor;
			float strataHardness; 
			
			float normalizedAltitude = (GetRemappedAltitude(IN.worldPos * float3(1/680 ,1 , 1/680), _MinAltitude, _MaxAltitude) - _MinAltitude) / (_MaxAltitude - _MinAltitude);
			//float3 normalizedPosition = float3(IN.worldPos.x / 680, normalizedAltitude, IN.worldPos.z / 680);
			//float strataValue = GetRemappedValue01(normalizedPosition, _RemapOffset, _NumStrata);
			//float strataValue = tex2D(_RemapTex, float2(normalizedAltitude, 0.5)).x;
			//float pixelAltitude = IN.worldPos.y;
			
			float strataIndex = floor(normalizedAltitude * (_NumStrata-1));  // 7 == SizeOfBuffer - 1
			// REALLY WEIRD stuff going on with this, need to set it up exactly like this otherwise it seems to choose 1 or the other compiler path
			#ifdef SHADER_API_D3D11	
			normalizedAltitude = 1;
            strataColor = rockStrataDataCBuffer[(int)clamp(strataIndex, 0, _NumStrata - 1)].color;
			strataColorNext = rockStrataDataCBuffer[(int)clamp(strataIndex + 1, 0, _NumStrata - 1)].color;
			strataColorPrev = rockStrataDataCBuffer[(int)clamp(strataIndex - 1, 0, _NumStrata - 1)].color;
			strataHardness = rockStrataDataCBuffer[(int)clamp(strataIndex, 0, _NumStrata - 1)].hardness;
			#endif

			

			//o.Albedo = float3(1,1,1) * normalizedAltitude * strataColor * strataColor * strataColorNext; // REALLY WEIRD need to multiply by altitude to get strata Colors??????
			//o.Albedo = lerp(o.Albedo, float3(0.9, 0.8, 0.7), 0.8);
			//o.Albedo *= length(strataColor);// * strataHardness;

			float4 rockDetailSample = tex2D (_RockHeightDetailTex, IN.uv_MainTex * 32 + 0.5);
			float4 sediDetailSample = tex2D (_SediHeightDetailTex, IN.uv_MainTex * 64 + 0.5);
			float4 snowDetailSample = tex2D (_SnowHeightDetailTex, IN.uv_MainTex * 32 + 0.5);

			float gradDetailHeightRockX = ddx(rockDetailSample.x);
			float gradDetailHeightRockY = ddy(rockDetailSample.x);
			float gradMagRock = length(float2(gradDetailHeightRockX, gradDetailHeightRockY));
			float3 detailNormalRock = cross(normalize(float3(0, 1, gradDetailHeightRockY)), normalize(float3(1, 0, gradDetailHeightRockX)));
			
			float gradDetailHeightSediX = ddx(sediDetailSample.x);
			float gradDetailHeightSediY = ddy(sediDetailSample.x);
			float gradMagSedi = length(float2(gradDetailHeightSediX, gradDetailHeightSediY));
			float3 detailNormalSedi = cross(normalize(float3(0, 1, gradDetailHeightSediY)), normalize(float3(1, 0, gradDetailHeightSediX)));

			float gradDetailHeightSnowX = ddx(snowDetailSample.x);
			float gradDetailHeightSnowY = ddy(snowDetailSample.x);
			float gradMagSnow = length(float2(gradDetailHeightSnowX, gradDetailHeightSnowY));
			float3 detailNormalSnow = cross(normalize(float3(0, 1, gradDetailHeightSnowY)), normalize(float3(1, 0, gradDetailHeightSnowX)));
			
			//o.Normal = normalize(o.Normal + detailNormalRock * 0.25);
			
			float3 rockNml = lerp(o.Normal, normalize(o.Normal + detailNormalRock), 0.2);
			o.Normal = rockNml;
			
			//float3 rockHue = GetRockHue(_PriHueRock.rgb, _SecHueRock.rgb, IN.worldPos.y, rockDetailSample.xyz);
			float3 rockHue = lerp(_SecHueRock.rgb, _PriHueRock.rgb, saturate(sin(IN.worldPos.y * 0.07431) * 0.5 + 0.5));
			rockHue = lerp(rockHue, rockDetailSample.xyz, 0.015);
			o.Albedo = rockHue; // float3(1, 1, 1); // Rock

			float3 sediNml = lerp(o.Normal, normalize(o.Normal + detailNormalSedi), 0.16);
			float3 sediHue = lerp(_SecHueSedi.rgb, _PriHueSedi.rgb, saturate(cos(IN.worldPos.y * 0.1489) * 0.5 + 0.5));
			sediHue = lerp(sediHue, sediDetailSample.xyz, 0.015);
			if(IN.color.y > 0) {
				o.Albedo = lerp(o.Albedo, sediHue * saturate(1 + 0.5), smoothstep(0.25, 4, IN.color.y + 0.1));
				o.Normal = lerp(o.Normal, sediNml, smoothstep(0.25, 4, IN.color.y));
			}

			float3 snowNml = lerp(o.Normal, normalize(o.Normal + detailNormalSnow), 0.2);
			float3 snowHue = lerp(_SecHueSnow.rgb, _PriHueSnow.rgb, saturate(sin(IN.worldPos.y * 0.237) * 0.5 + 0.5));
			snowHue = lerp(snowHue, snowDetailSample.xyz, 0.00025);
			if(IN.color.z > 0) {
				o.Albedo = lerp(o.Albedo, snowHue, smoothstep(0.1,0.25, IN.color.z));
				o.Normal = lerp(o.Normal, snowNml, smoothstep(0.25, 4, IN.color.z));
			}

			//o.Normal = detailNormal;
			
			//o.Albedo = lerp(o.Albedo, rockDetailSample.xyz, 0.02);
			//o.Albedo = float3(IN.color.y,0,0);
			//o.Albedo.xy = IN.uv_MainTex;

			//o.Normal = customHeightNormal;

			float grayVal = o.Albedo.x * 0.299 + o.Albedo.y * 0.587 + o.Albedo.z * 0.114;
			o.Albedo = lerp(float3(grayVal, grayVal, grayVal), 0.75, 0.75);
			o.Albedo -= (customHeightSample.x - 0.5) * 0.3;
			
			o.Normal = lerp(o.Normal, customHeightNormal, saturate(customHeightSample.x * 1));

			//float2 customHeightUV = float2(IN.uv_MainTex.x - 0.5, IN.uv_MainTex.y - 0.5);
			//customHeightUV += 0.5;
			//customHeightUV *= 0.6;
			//o.Albedo = tex2D(_CustomHeightRT, customHeightUV);
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
			
		}
		
		ENDCG
	}
	FallBack "Diffuse"
}
