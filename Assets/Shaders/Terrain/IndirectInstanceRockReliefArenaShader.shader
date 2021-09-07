Shader "Instanced/IndirectInstanceRockReliefArenaShader" {
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
		sampler2D _BumpMap_ST;
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
            //TransformData data = instancedRocksCBuffer[unity_InstanceID];

			//unity_ObjectToWorld._11_21_31_41 = float4(1, 0, 0, 0);
			//unity_ObjectToWorld._12_22_32_42 = float4(0, 1, 0, 0);
			//unity_ObjectToWorld._13_23_33_43 = float4(0, 0, 1, 0);
			//unity_ObjectToWorld._14_24_34_44 = float4(matricesCBuffer[unity_InstanceID]._14_24_34_44.xyz, 1); //float4(matricesCBuffer[unity_InstanceID]._11_21_31_41.x, matricesCBuffer[unity_InstanceID]._11_21_31_41.y, matricesCBuffer[unity_InstanceID]._11_21_31_41.w, 1);		
			
			/*float rx = data.rotation.x;
			float ry = data.rotation.y;
			float rz = data.rotation.z;
			float4x4 rotationMatrix = float4x4(  // ZXY:
				cos(rz)*cos(ry)-sin(rz)*cos(rx)*cos(ry)-sin(rz)*sin(rx)*sin(ry), -sin(rz)*cos(rx), cos(rz)*sin(ry)-sin(rz)*cos(rx)*sin(ry)+sin(rz)*sin(rx)*cos(ry), 0,
				sin(rz)*cos(ry)-cos(rx)*sin(ry), cos(rz)*cos(rx), sin(rz)*sin(ry)-cos(rz)*sin(rx)*cos(ry), 0,
				-cos(rx)*sin(ry), sin(rx), cos(rx)*cos(ry), 0,
				0, 0, 0, 1);*/
			//unity_ObjectToWorld = mul(unity_ObjectToWorld, rotationMatrix);  // apply rotation

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
			col = matricesCBuffer[unity_InstanceID]._11_21_31_41;	
			randColorLerp = rand(col.xz);
			//col = col.xxx1;
#else
			//col.gb = float4(0, 0, 1, 1);
			col = float4(0, 0, 1, 1);
#endif

			float2 terrainUV = (IN.worldPos.xz / 680) * 0.5;

			float4 terrainHeights = tex2D(_HeightTex, terrainUV);
			
			float4 rockDetailSample = tex2D (_RockHeightDetailTex, terrainUV * 32 + 0.5);
			float4 sediDetailSample = tex2D (_SediHeightDetailTex, terrainUV * 64 + 0.5);
			float4 snowDetailSample = tex2D (_SnowHeightDetailTex, terrainUV * 32 + 0.5);

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
			float3 nmlMapNormals = UnpackNormal (tex2D (_BumpMap, terrainUV * 256));
			o.Normal = nmlMapNormals;
			float3 rockNml = lerp(o.Normal, normalize(o.Normal + detailNormalRock), 0.2);
			//o.Normal = rockNml;
			
			//float3 rockHue = GetRockHue(_PriHueRock.rgb, _SecHueRock.rgb, IN.worldPos.y, rockDetailSample.xyz);
			float3 rockHue = lerp(_SecHueRock.rgb, _PriHueRock.rgb, saturate(sin(IN.worldPos.y * 0.07431) * 0.5 + 0.5));
			rockHue = lerp(rockHue, rockDetailSample.xyz, 0.015);
			o.Albedo = rockHue; // float3(1, 1, 1); // Rock

			float3 sediNml = lerp(o.Normal, normalize(o.Normal + detailNormalSedi), 0.16);
			float3 sediHue = lerp(_SecHueSedi.rgb, _PriHueSedi.rgb, saturate(cos(IN.worldPos.y * 0.1489) * 0.5 + 0.5));
			sediHue = lerp(sediHue, sediDetailSample.xyz, 0.015);
			if(terrainHeights.y > 0) {
				o.Albedo = lerp(o.Albedo, sediHue * saturate(terrainHeights.rgb + 0.5), smoothstep(0.25, 4, terrainHeights.y + 0.1));
				//o.Normal = lerp(o.Normal, sediNml, smoothstep(0.25, 4, terrainHeights.y));
			}

			float3 snowNml = lerp(o.Normal, normalize(o.Normal + detailNormalSnow), 0.2);
			float3 snowHue = lerp(_SecHueSnow.rgb, _PriHueSnow.rgb, saturate(sin(IN.worldPos.y * 0.237) * 0.5 + 0.5));
			snowHue = lerp(snowHue, snowDetailSample.xyz, 0.00025);
			if(terrainHeights.z > 0) {
				o.Albedo = lerp(o.Albedo, snowHue, smoothstep(0.1,0.25, terrainHeights.z));
				//o.Normal = lerp(o.Normal, snowNml, smoothstep(0.25, 4, terrainHeights.z));
			}

			float grayVal = o.Albedo.x * 0.299 + o.Albedo.y * 0.587 + o.Albedo.z * 0.114;
			o.Albedo = float3(grayVal, grayVal, grayVal);

            //o.Albedo *= c.rgb + 0.25;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}