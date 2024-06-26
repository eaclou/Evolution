﻿Shader "Unlit/FloatyBitsDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "black" {}
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

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _TerrainColorTex;

			struct FloatyBitData {
				float2 coords;
				float2 vel;
				float2 heading;
				float age;
			};

			StructuredBuffer<FloatyBitData> floatyBitsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform float _MapSize;
			uniform float _MaxAltitude;
			uniform float _GlobalWaterLevel;

			uniform float _CamDistNormalized;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture					
				float4 color : TEXCOORD1;
				float2 fluidCoords : TEXCOORD2;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				FloatyBitData floatyBitData = floatyBitsCBuffer[inst];
				float3 worldPosition = float3(floatyBitData.coords * _MapSize, 0);
				float3 quadPoint = quadVerticesCBuffer[id];

				o.fluidCoords = floatyBitData.coords;

				float2 velocity = floatyBitData.vel;
				float velMag = length(velocity);

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float random3 = rand(float2(random1 - _Time.y * 21.34, random2));

				float randomAspect = lerp(0.85, 1.216, random1);
				float randomValue = rand(float2(inst, randomAspect * random3));
				float randomScale = lerp(0.0785, 0.265, random2) * 0.69;
				
				float2 dir = normalize(velocity);
				float2 scale = float2(randomAspect, (1.0 / randomAspect)) * randomScale; //float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 25 + 1.61));
				//scale.y *= (1.0 + velMag * 10.0);
				quadPoint *= float3(scale, 1.0); // * (_CamDistNormalized * 0.98 + 0.02); 

				float2 forward = dir;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
											 quadPoint.z);
				
				float fadeIn = saturate(floatyBitData.age / 0.0055);
				float fadeOut = saturate((1.0 - floatyBitData.age) / 0.33);
				float alpha = fadeIn * fadeOut;

				float4 altitudeTex = tex2Dlod(_AltitudeTex, float4(floatyBitData.coords, 0, 0));			
				float altitudeRaw = altitudeTex.x;

				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(floatyBitData.coords, 0, 0));
				float dotLight = dot(waterSurfaceData.yzw, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				float isUnderwaterMask = saturate((_GlobalWaterLevel - altitudeRaw) * 34);
				float waveHeight = waterSurfaceData.x * isUnderwaterMask;

				//float seaFloorAltitude = -altitudeRaw * _MaxAltitude;
				//float waterAltitude = -(_GlobalWaterLevel + waveHeight * 0.152) * _MaxAltitude; 
				//worldPosition.z = min(seaFloorAltitude, waterAltitude);
				worldPosition.z = -max(_GlobalWaterLevel, altitudeRaw) * _MaxAltitude;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1)) + float4(rotatedPoint, 0));
				float brightness = (random1);
				dotLight *= brightness;
				o.color = float4(dotLight, floatyBitData.age, dotLight, alpha * 1); // float4(randomValue, randomValue, randomValue, 1 / (length(velocity) * 50 + 1.15));
				o.uv = quadVerticesCBuffer[id] + 0.5;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(0.1,0.1,0.1,0.11);
				return float4(float3(0.32,0.6,0.8) * 0.75,0.7);
				//return float4(0.21,0.71,0.91,0.1);

				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				//return texColor;
				float4 finalColor = texColor; //float4(1,1,1,1); //float4(0.1,0.1,0.05,1) * texColor; //texColor * _Tint * float4(i.color, 1);
				finalColor.rgb = float3(0.49, 0.48, 0.35) * 1.5;
				//finalColor.rgb = float3(0.49, 0.48, 0.95);
				//finalColor.a *= i.color.a * 1;
				//float birthGlowMask = saturate((-i.color.y + 0.2) * 20);
				//finalColor += birthGlowMask;
				finalColor *= 0.5 * texColor.a;
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
