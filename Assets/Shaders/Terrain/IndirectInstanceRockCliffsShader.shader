﻿Shader "Instanced/IndirectInstanceRockCliffsShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_HeightTex ("Height Tex", 2D) = "white" {}
		_RockHeightDetailTex ("Rock Height Detail", 2D) = "white" {}
		_SediHeightDetailTex ("Sedi Height Detail", 2D) = "white" {}
		_SnowHeightDetailTex ("Snow Height Detail", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
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
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;
		sampler2D _HeightTex;
		sampler2D _BumpMap;
		sampler2D _RockHeightDetailTex;
		sampler2D _SediHeightDetailTex;
		sampler2D _SnowHeightDetailTex;
		float4 _PriHueRock;
		float4 _SecHueRock;
		float4 _PriHueSedi;
		float4 _SecHueSedi;
		float4 _PriHueSnow;
		float4 _SecHueSnow;

        struct Input {
            float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
			float3 color;
        };
				
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4x4> matricesCBuffer;
		StructuredBuffer<float4x4> invMatricesCBuffer;
    #endif

        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

		float3 qtransform(float4 q, float3 v){ 
			return v + 2.0*cross(cross(v, q.xyz ) + q.w*v, q.xyz);
		}

		float rand(in float2 uv)
		{
			float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
			return abs(noise.x + noise.y) * 0.5;
		}

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			// Transformation Matrices
			unity_ObjectToWorld = matricesCBuffer[unity_InstanceID];
            unity_WorldToObject = invMatricesCBuffer[unity_InstanceID];
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 col = 1.0f;
			float randColorLerp = 0;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			//col.gb = (float)(unity_InstanceID % 256) / 255.0f;
			//col = matricesCBuffer[unity_InstanceID]._11_21_31_41;	
			//randColorLerp = rand(col.xz);
			//col = col.xxx1;
#else
			//col.gb = float4(0, 0, 1, 1);
			//col = float4(0, 0, 1, 1);
#endif
			float2 terrainUV = (IN.worldPos.xz / 680) * 0.5;

			float3 terrainHeights = tex2D(_HeightTex, terrainUV);
			
			float4 rockDetailSample = tex2D (_RockHeightDetailTex, terrainUV * 32 + 0.5);
			float4 sediDetailSample = tex2D (_SediHeightDetailTex, terrainUV * 128 + 0.5);
			float4 snowDetailSample = tex2D (_SnowHeightDetailTex, terrainUV * 32 + 0.5);

			float3 rockHue = lerp(_SecHueRock.rgb, _PriHueRock.rgb, saturate(sin(IN.worldPos.y * 0.07431) * 0.5 + 0.5));
			rockHue = lerp(rockHue, rockDetailSample.xyz, 0.015);
			o.Albedo = rockHue; // float3(1, 1, 1); // Rock

			float3 sediHue = lerp(_SecHueSedi.rgb, _PriHueSedi.rgb, saturate(cos(IN.worldPos.y * 0.1489) * 0.5 + 0.5));
			sediHue = lerp(sediHue, sediDetailSample.xyz, 0.015);
			if(terrainHeights.y > 0) {
				o.Albedo = lerp(o.Albedo, sediHue * saturate(terrainHeights.rgb + 0.5), smoothstep(0.25, 4, terrainHeights.y + 0.1));
			}
			float3 snowHue = lerp(_SecHueSnow.rgb, _PriHueSnow.rgb, saturate(sin(IN.worldPos.y * 0.237) * 0.5 + 0.5));
			snowHue = lerp(snowHue, snowDetailSample.xyz, 0.00025);
			if(terrainHeights.z > 0) {
				o.Albedo = lerp(o.Albedo, snowHue, smoothstep(0.1,0.25, terrainHeights.z));
			}

			//float3 hue = float3(rand(col.xy), rand(col.yz), rand(col.zw));
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex);			
            //o.Albedo = c.rgb * lerp(_BaseColorPrimary, _BaseColorSecondary, randColorLerp) * 1; //float3(c.r * hue.x, c.y * hue.y, c.z * hue.z);

			float grayVal = o.Albedo.x * 0.299 + o.Albedo.y * 0.587 + o.Albedo.z * 0.114;
			o.Albedo = float3(grayVal, grayVal, grayVal);

			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}