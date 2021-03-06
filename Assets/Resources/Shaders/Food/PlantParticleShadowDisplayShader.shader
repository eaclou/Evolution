﻿Shader "Brushstrokes/PlantParticleShadowDisplayShader"
{
	Properties
	{
		
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_ResourceGridTex ("_ResourceGridTex", 2D) = "black" {}
		_TerrainColorTex ("_TerrainColorTex", 2D) = "black" {}
		_SpiritBrushTex ("_SpiritBrushTex", 2D) = "black" {}
		_PatternTex ("_PatternTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
	}
	SubShader
	{		
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		//Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsPlantParticles.cginc"
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"
			
			sampler2D _MainTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _ResourceGridTex;
			sampler2D _TerrainColorTex;
			sampler2D _SpiritBrushTex;
			sampler2D _PatternTex;
			sampler2D _SkyTex;
			
			uniform float3 _SunDir;
			uniform float _MapSize;
			uniform float _GlobalWaterLevel;
			uniform float _CamDistNormalized;
			uniform float4 _WorldSpaceCameraPosition;
			uniform float _MaxAltitude;	

			StructuredBuffer<PlantParticleData> plantParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform int _SelectedParticleIndex;
			uniform int _HoverParticleIndex;
			uniform float _IsSelected;
			uniform float _IsHover;

			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				float4 hue : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float2 altitudeUV : TEXCOORD3;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy;
				uv.x += 0.5;
				float t = uv.y; // + 0.5;
				uv.y = 1.0 - uv.y;

				
				int particleIndex = floor((float)inst / 32.0);

				PlantParticleData particleData = plantParticleDataCBuffer[particleIndex];

				float selectedMask = (1.0 - saturate(abs(_SelectedParticleIndex - particleIndex))) * _IsSelected;
				float hoverMask = (1.0 - saturate(abs(_HoverParticleIndex - particleIndex))) * _IsHover;

				float2 altUV = particleData.worldPos.xy / _MapSize;	
				o.altitudeUV = altUV;
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x;
				float zPos = -altitudeRaw * _MaxAltitude;

				float3 worldPosition = float3(particleData.worldPos, zPos);    //float3(rawData.worldPos, -random2);
				
				//float3 localQuadPos = quadPoint;

				float rand0 = rand(float2(inst, inst) * 10);
				float rand1 = rand(float2(rand0, rand0) * 10);
				float rand2 = rand(float2(rand1, rand1) * 10);	
				float rand3 = rand(float2(rand2, rand2) * 10);	

				float leafIndex = (float)(inst % 32);
				float leafIndexNormalized = leafIndex / 32.0;
				//Type:
				float type = particleData.typeID;
				if(type < 0.5) {
					// Rooted fully, grows separately on ground in circle? Grassy

					float radius = saturate(particleData.biomass * 1.8 + 0.004) * leafIndexNormalized * 0.5;
					float2 spawnOffset = float2(cos(particleData.angleInc * leafIndex * 2.5) * radius, sin(particleData.angleInc * leafIndex * 2.5) * radius);
					
					worldPosition.xy += spawnOffset;
														
					// REFRACTION:
					float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0)).yzw;				
					float refractionStrength = 0.5;
					
					worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				}
				else {
				
				}

				float masterFreq = 5;
				float spatialFreq = 0.06125285;
				float timeMult = 0.08;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time.y * timeMult + inst * 11.11, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.025;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;

				float2 forward = float2(cos(rand3 * 10), sin(rand3 * 10));
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward, 0);  // Rotate localRotation by AgentRotation

				float leafScale = 1.05 * (saturate(particleData.biomass * 3 + 0.1) * 0.35 * particleData.isActive + hoverMask * 0.3);
				o.worldPos = float4(worldPosition, 0);
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition + rotatedPoint * leafScale, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = uv;	
				
				o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), particleData.rootedness, max(hoverMask * 0.5, selectedMask));
				o.hue = float4(particleData.colorA * 0.12, 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//Diffuse
				float pixelOffset = 1.0 / 256;  // resolution  // **** THIS CAN"T BE HARDCODED AS FINAL ****"
				// ************  PRE COMPUTE THIS IN A TEXTURE!!!!!! ************************
				float altitudeNorth = tex2D(_AltitudeTex, i.altitudeUV + float2(0, pixelOffset)).x;
				float altitudeEast = tex2D(_AltitudeTex, i.altitudeUV + float2(pixelOffset, 0)).x;
				float altitudeSouth = tex2D(_AltitudeTex, i.altitudeUV + float2(0, -pixelOffset)).x;
				float altitudeWest = tex2D(_AltitudeTex, i.altitudeUV + float2(-pixelOffset, 0)).x;

				float dX = altitudeEast - altitudeWest;
				float dY = altitudeNorth - altitudeSouth;

				float2 grad = float2(0,1);
				if(dX != 0 && dY != 0) {
					grad = normalize(float2(dX, dY));
				}
				float3 groundSurfaceNormal = normalize(float3(-grad.x, -grad.y, length(float2(dX,dY)))); ////normalize(altitudeTex.yzw);
				
				float3 algaeColor = float3(0.5,0.8,0.5) * 0.5;

				ShadingData data;
				data.baseAlbedo = float4(0.01,0.01,0.01,1);
				data.altitudeTex = tex2D(_AltitudeTex, i.altitudeUV);
    			data.waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				data.groundNormalsTex = float4(0, groundSurfaceNormal);
    			data.resourceGridTex = tex2D(_ResourceGridTex, i.altitudeUV);
				data.spiritBrushTex = tex2D(_SpiritBrushTex, i.altitudeUV);
				data.skyTex = tex2D(_SkyTex, i.altitudeUV);
				data.worldPos = i.worldPos;
				data.maxAltitude = _MaxAltitude;
				data.waterFogColor = float4(algaeColor, 1);
				data.sunDir = float4(_SunDir, 0);
				data.worldSpaceCameraPosition = _WorldSpaceCameraPosition;
				data.globalWaterLevel = _GlobalWaterLevel;
				data.causticsStrength = 0.5;
				data.depth = saturate(-data.altitudeTex.x + data.globalWaterLevel);
				float4 outColor = MasterLightingModel(data);
				outColor.a *= tex2D(_MainTex, i.uv).a;

				//return float4(1,1,1,1);
				return outColor;
				
				//float4 texColor = tex2D(_MainTex, i.uv);
				//float4 terrainColor = tex2D(_TerrainColorTex, i.worldPos.xy / _MapSize);
				
				//float4 finalCol = terrainColor * 0.7;
				//finalCol.a = 1;
				//return finalCol;
				/*
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				col.rgb = lerp(col, float3(0.81, 0.79, 0.65) * 0.1, i.color.x * 0.6);
				col.rgb = lerp(col, float3(0.6, 1, 0.4) * 1, 0.25);
				col.rgb = lerp(col.rgb, terrainColor.rgb, 0.37);
				//col.rgb += i.color.w * 2.5;
				col.a = texColor.a;

				return col;
				*/
			}
		ENDCG
		}
	}
}
