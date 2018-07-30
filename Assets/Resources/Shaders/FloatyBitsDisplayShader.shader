Shader "Unlit/FloatyBitsDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_FluidColorTex ("_FluidColorTex", 2D) = "black" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
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
			sampler2D _FluidColorTex;
			sampler2D _WaterSurfaceTex;

			struct FloatyBitData {
				float2 coords;
				float2 vel;
				float2 heading;
				float age;
			};

			StructuredBuffer<FloatyBitData> floatyBitsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform float _MapSize;

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

				float randomAspect = lerp(0.75, 1.36, random1);
				float randomValue = rand(float2(inst, randomAspect * 10));
				float randomScale = lerp(0.75, 1.4, random2) * 3.6;
				float2 scale = float2(0.05 * randomAspect, 0.05 * (1.0 / randomAspect)) * randomScale; //float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 25 + 1.61));
				//scale.x *= 1.35;
				quadPoint *= float3(scale, 1.0);
				
				float2 forward = floatyBitData.heading; //normalize(velocity);
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
											 quadPoint.z);
				
				float fadeIn = saturate(floatyBitData.age / 0.2);
				float fadeOut = saturate((1.0 - floatyBitData.age) / 0.2);
				float alpha = fadeIn * fadeOut;
				//alpha = alpha * (saturate(velMag * 20 + 0.5));

				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(floatyBitData.coords, 0, 0));
				float dotLight = dot(waterSurfaceData.yzw, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				float waveHeight = waterSurfaceData.x;

				worldPosition.z -= waveHeight * 2.5;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				float brightness = (random1);
				o.color = float4(dotLight,dotLight,dotLight,alpha * 1); // float4(randomValue, randomValue, randomValue, 1 / (length(velocity) * 50 + 1.15));
				o.uv = quadVerticesCBuffer[id] + 0.5f;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				//float4 fluidColor = tex2D(_FluidColorTex, i.fluidCoords * 1); // i.fluidCoords); 

				float4 finalColor = texColor; //float4(1,1,1,1); //float4(0.1,0.1,0.05,1) * texColor; //texColor * _Tint * float4(i.color, 1);
				finalColor.rgb = float3(0.9, 0.8, 0.5) * (0.25 + 0.75 * i.color.x);
				//finalColor.a *= i.color.a * 0.25;
				//finalColor.rgb = lerp(fluidColor.rgb, i.color.rgb, 0.06);
				//finalColor.a *= 0.65;
				//finalColor.rgb *= 1.16;
				//finalColor.rgb = float3(1,1,1);
				return finalColor;
				
			}
		ENDCG
		}
	}
}
