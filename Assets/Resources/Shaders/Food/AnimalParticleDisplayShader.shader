Shader "Brushstrokes/AnimalParticleDisplayShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}  // stem texture sheet		
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
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
			sampler2D _WaterSurfaceTex;
			sampler2D _AltitudeTex;
			sampler2D _TerrainColorTex;
			
			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			uniform int _SelectedParticleIndex;
			uniform float _IsHighlight;

			uniform float _MapSize;
			uniform float _MaxAltitude;
			uniform float _GlobalWaterLevel;
			//uniform float _MouseCoordX;
			//uniform float _MouseCoordY;

			uniform int _SelectedParticleID;
			uniform int _ClosestParticleID;
			uniform float _IsSelected;
			uniform float _IsHover;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				float2 highlight : TEXCOORD1;
				float2 status : TEXCOORD2;
				float2 altitudeUV : TEXCOORD3;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			// CUBIC:
			float2 GetPoint2D (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
			}
			float2 GetPoint1D (float p0, float p1, float p2, float p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
			}
			// CUBIC
			float2 GetFirstDerivative (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return 3.0 * oneMinusT * oneMinusT * (p1 - p0) + 6.0 * oneMinusT * t * (p2 - p1) + 3.0 * t * t * (p3 - p2);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy;
				uv.x += 0.5;
				
				AnimalParticleData particleData = animalParticleDataCBuffer[inst];
				//_SelectedParticleIndex = round(_Time.y * 3) % 1024;
				float highlightMask = _IsHighlight;

				float t = uv.y; // + 0.5;
				uv.y = 1.0 - uv.y;

				float selectedMask = (1.0 - saturate(abs(_SelectedParticleIndex - (int)inst))) * _IsSelected;
				float hoverMask = (1.0 - saturate(abs(_ClosestParticleID - (int)inst))) * _IsHover;

				//float width = 0.25 + hoverMask + selectedMask * 2.5;
				float width = 0.25 + sqrt(particleData.biomass) * 0.025 * (1 - 2 * abs(0.75 - uv.y)) + 0.0115 + 0.01 * hoverMask + 0.02 * selectedMask; //GetPoint1D(waterCurveData.widths.x, waterCurveData.widths.y, waterCurveData.widths.z, waterCurveData.widths.w, t) * 0.75 * (1 - saturate(testNewVignetteMask));
				
				float freq = 20;
				float swimAnimOffset = sin(_Time.y * freq - t * 7 + (float)inst * 0.1237) * 4;
				float swimAnimMask = t * saturate(1.0 - particleData.isDecaying); //saturate(1.0 - uv.y); //saturate(1.0 - t);
				
				float3 worldPosition = particleData.worldPos; // float3(curvePos,0) + float3(offset, 0.0);
				worldPosition.x += (swimAnimOffset * swimAnimMask) * width;

				//float decayProgress = 0;				
				//decayProgress = 1.0 - (particleData.biomass / (particleData.extra0 + 0.0000001));
					
				
				// REFRACTION:				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0)).yzw;				
				float refractionStrength = 0.15;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				
				float2 altUV = worldPosition.xy / _MapSize;				
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0));
				float seaFloorAltitude = -altitudeRaw * _MaxAltitude;
				worldPosition.z = -max(_GlobalWaterLevel, altitudeRaw) * _MaxAltitude;
				worldPosition.z = lerp(worldPosition.z, seaFloorAltitude, particleData.isDecaying);
				
				float2 vertexOffset = quadPoint.xy * 1; // * width;
				//vertexOffset.xy *= 4;

				//*** TEMP::::: ****
				float spriteScale = (sqrt(particleData.biomass) * 0.025 + 0.0115 + (0.06 * hoverMask + 0.02 * selectedMask)) * 1;
				spriteScale = 0.1;
				vertexOffset.xy = quadPoint.xy * spriteScale * 10;


				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition.xy + vertexOffset, worldPosition.z, 1.0)));			
				o.uv = uv;
				o.altitudeUV = worldPosition.xy / _MapSize;


				o.color = particleData.color;
				float oldAgeMask = saturate((particleData.age - 1.0) * 1000);
				o.color.a = saturate(particleData.age * 0.5); //  1.0 - oldAgeMask; // particleData.isDecaying;
				o.highlight = float2(hoverMask, selectedMask);
				float blah = (1.0 - (particleData.biomass / particleData.extra0)) * particleData.isDecaying;
				o.status = float2(particleData.isActive, blah);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1) * (0.5 + i.color.y);

				float4 texColor = tex2D(_MainTex, i.uv);
				float4 terrainColorTex = tex2D(_TerrainColorTex, i.altitudeUV);

				float val = i.color.a;
				
				float4 finalColor = float4(lerp(i.color.rgb, float3(0.75, 0.75, 1.0), 0.5) * 1.25, i.uv.y); // float4(float3(i.color.z * 1.2, 0.85, (1.0 - i.color.w) * 0.2) + i.color.y, texColor.a * i.color.x * 0.33 * (1 - i.color.z));
				finalColor.rgb = lerp(finalColor.rgb, float3(0.4, 0.4, 0.4), val);
				finalColor.rgb *= 3.14;
				finalColor.rgb = lerp(finalColor.rgb, float3(0.75, 0.2, 0.92), 0.372);
				finalColor.rgb = lerp(finalColor.rgb, i.color.rgb, 0.33);
				float uvDist = length(i.uv - 0.5) * 2;
				float circleFade = saturate(uvDist - 0.9);
				finalColor.rgb *= saturate(1.0 - uvDist);
				float circleMask = saturate(circleFade * 12);
				// ****************************************************************************
				finalColor.rgb = lerp(finalColor.rgb, float3(0.1, 0.05, 0.02) * 2, saturate(i.status.y));
				finalColor *= 1.0 + i.highlight.x * 2 + i.highlight.y;

				finalColor.rgb = lerp(finalColor.rgb, terrainColorTex.rgb, 0.4 + saturate(i.status.y) * 0.7);
				finalColor = float4(finalColor.rgb, (1.0 - circleMask) * i.status.x);
				
				
				//finalColor.rgb = float3(1,0.25,0.521) * 1.73;
				return finalColor;//  // * (1.0 + i.color.y));  // age

			}
		ENDCG
		}
	}
}
