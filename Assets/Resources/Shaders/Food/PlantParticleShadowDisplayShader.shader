Shader "Brushstrokes/PlantParticleShadowDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet	
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_TerrainColorTex ("_TerrainColorTex", 2D) = "black" {}
		//_Tint("Color", Color) = (1,1,1,1)
		//_Size("Size", vector) = (1,1,1,1)
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
			sampler2D _WaterSurfaceTex;
			sampler2D _TerrainColorTex;
			uniform float _MapSize;
			//sampler2D _RenderedSceneRT; 

			StructuredBuffer<PlantParticleData> plantParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform int _SelectedParticleIndex;
			uniform int _HoverParticleIndex;
			uniform float _IsSelected;
			uniform float _IsHover;

			uniform float _GlobalWaterLevel;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float2 altitudeUV : TEXCOORD1;
				//float4 screenUV : TEXCOORD2;
				//float3 worldPos : TEXCOORD3;
				//float4 color : COLOR;
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
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0));
				float seaFloorAltitude = -(altitudeRaw * 2 - 1) * 10;

				float3 worldPosition = float3(particleData.worldPos, seaFloorAltitude);    //float3(rawData.worldPos, -random2);
				float3 localQuadPos = quadPoint;

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

					float radius = saturate(particleData.biomass * 5 + 0.06) * leafIndexNormalized;
					float2 spawnOffset = float2(cos(particleData.angleInc * leafIndex * 10) * radius, sin(particleData.angleInc * leafIndex * 10) * radius);
					
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

				float2 altitudeUV = worldPosition.xy / _MapSize;					
				float altRaw = tex2Dlod(_AltitudeTex, float4(altitudeUV.xy, 0, 0)).x;	
				
				worldPosition.z = -(altRaw * 2 - 1) * 10;

				float leafScale = saturate(particleData.biomass * 4 + 0.1) * 0.25 * particleData.isActive;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition + rotatedPoint * leafScale, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = uv;	
				o.altitudeUV = altitudeUV;
				//o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), particleData.rootedness, max(hoverMask * 0.5, selectedMask));
				//o.hue = float4(particleData.colorA, 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 brushColor = tex2D(_MainTex, i.uv);	
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float depth = saturate((_GlobalWaterLevel - altitudeTex.x) * 2);				
				float4 terrainColorTex = tex2D(_TerrainColorTex, i.altitudeUV);
				float4 finalColor = float4(0,0,0,1);
				finalColor.rgb = lerp(finalColor.rgb, terrainColorTex.rgb, (0.5 + 0.5 * depth)); //GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(0,0,0,0));
				finalColor.a = brushColor.a * 0.25;

				return finalColor;
				
			}
		ENDCG
		}
	}
}
