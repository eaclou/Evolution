Shader "Brushstrokes/AnimalParticleShadowDisplayShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}  // stem texture sheet	
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
			#include "Assets/Resources/Shaders/Inc/StructsAnimalParticles.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;	
			sampler2D _WaterSurfaceTex;
			sampler2D _TerrainColorTex;
			uniform float _MapSize;
			uniform float _GlobalWaterLevel;
			uniform float _MaxAltitude;	

			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform int _SelectedParticleIndex;
			uniform int _SelectedParticleID;
			uniform int _ClosestParticleID;
			uniform float _IsSelected;
			uniform float _IsHover;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				float2 altitudeUV : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
							

				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy;
				uv.x += 0.5;
				
				AnimalParticleData particleData = animalParticleDataCBuffer[inst];

				float t = uv.y; // + 0.5;
				uv.y = 1.0 - uv.y;

				float selectedMask = (1.0 - saturate(abs(_SelectedParticleIndex - (int)inst))) * _IsSelected;
				float hoverMask = (1.0 - saturate(abs(_ClosestParticleID - (int)inst))) * _IsHover;

				float width = sqrt(particleData.biomass) * 0.04 * (1 - 2 * abs(0.75 - uv.y)) + 0.015 + 0.015 * hoverMask + 0.025 * selectedMask;
				//width *= 0.5;
				float freq = 20;
				float swimAnimOffset = sin(_Time.y * freq - t * 7 + (float)inst * 0.1237) * 4;
				float swimAnimMask = t * saturate(1.0 - particleData.isDecaying); //saturate(1.0 - uv.y); //saturate(1.0 - t);
				
				float3 worldPosition = particleData.worldPos; // float3(curvePos,0) + float3(offset, 0.0);
				//worldPosition.x += (swimAnimOffset * swimAnimMask) * width;
				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0)).yzw;				
				float refractionStrength = 0.15;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				//float2 altUV = particleData.worldPos.xy / _MapSize;
				//worldPosition.z = -(tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x) * 10;
				
				float2 altUV = particleData.worldPos.xy / _MapSize;
				o.altitudeUV = altUV;
				
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x;
				float zPos = -altitudeRaw * _MaxAltitude;
				
				//*** TEMP::::: ****
				float spriteScale = (sqrt(particleData.biomass) * 0.1 + 0.04 + (0.06 * hoverMask + 0.02 * selectedMask)) * 1;
				//spriteScale = 0.1;
				float2 vertexOffset = quadPoint.xy * spriteScale;
				float2 forward = normalize(particleData.velocity);
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(vertexOffset.x * right + vertexOffset.y * forward);
				//vertexOffset.xy *= 4;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition.xy + rotatedPoint, zPos, 1.0)));			
				o.uv = uv; //quadVerticesCBuffer[id].xy + 0.5f;	
				/////////////////////////////////////////////////////
				
				o.color = float4(0,0,0,0);
				float oldAgeMask = saturate((particleData.age - 1.0) * 1000);
				o.color.a = 1.0 - oldAgeMask; 
				//o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), saturate(particleData.age * 0.5), 1);
								
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 brushColor = tex2D(_MainTex, i.uv);									
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float depth = saturate((_GlobalWaterLevel - altitudeTex.x) * 1);
				//float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				float4 terrainColorTex = tex2D(_TerrainColorTex, i.altitudeUV);
				//float4 finalColor = float4(0,0,0,1);
				//finalColor.rgb = lerp(finalColor.rgb, terrainColorTex.rgb, (0.5 + 0.5 * depth)); //GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(0,0,0,0));
				//finalColor.a = brushColor.a * 1;
				float4 finalColor = float4(0,0,0,0.7);
				finalColor.rgb = lerp(finalColor.rgb, terrainColorTex.rgb * 0.75, depth); //GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(0,0,0,0));
				

				return finalColor;
				

			}
		ENDCG
		}
	}
}
