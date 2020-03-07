Shader "Terrain/TerrainDecomposerBitsDisplayShader"
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
		_NumRows ("_NumRows", Float) = 1
		_NumColumns ("_NumColumns", Float) = 1
		
	}
	SubShader
	{		
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
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

			uniform float _NumRows;
			uniform float _NumColumns;
			
			uniform float3 _SunDir;

			uniform float _MapSize;
			uniform float _GlobalWaterLevel;
			uniform float _CamDistNormalized;
			uniform float4 _WorldSpaceCameraPosition;
			uniform float _MaxAltitude;

			uniform float _Density;

			uniform float4 _TintPri;
			uniform float4 _TintSec;
			uniform float _PatternThreshold;
			uniform int _PatternColumn;
			uniform int _PatternRow;


			struct GroundBitsData
			{
				int index;
				float3 worldPos;
				float2 heading;
				float2 localScale;
				float age;
				float speed;
				float noiseVal;
				float isActive;
				int brushType;
			};

			StructuredBuffer<GroundBitsData> groundBitsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 quadUV : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float2 altitudeUV : TEXCOORD1;
				float4 screenUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 vignetteLerp : TEXCOORD4;
				float2 skyUV : TEXCOORD5;
				float2 patternUV : TEXCOORD6;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				GroundBitsData groundBitData = groundBitsCBuffer[inst];

				float3 worldPosition = groundBitData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				float2 vel = tex2Dlod(_VelocityTex, float4(worldPosition.xy / 256, 0, 1)).xy;
				float fluidSpeedMult = 3;
				worldPosition.xy += vel * fluidSpeedMult;  // carried by water

				groundBitData.brushType = (groundBitData.brushType + floor(_Time.y * 0)) % (_NumColumns * _NumRows);
				float2 quadUV = quadPoint + 0.5f; // 0-1,0-1
				quadUV.x = quadUV.x / _NumColumns;
				quadUV.y = quadUV.y / _NumRows;				
				float column = (float)(groundBitData.brushType % _NumColumns);
				float row = (float)floor((groundBitData.brushType) / _NumColumns);
				quadUV.x += column * (1.0 / _NumColumns);
				quadUV.y += row * (1.0 / _NumRows);
				o.quadUV = quadUV; // full texture

				o.worldPos = worldPosition;
				float2 uv = worldPosition.xy / _MapSize;
				o.altitudeUV = uv;

				float4 altitudeTexSample = tex2Dlod(_AltitudeTex, float4(o.altitudeUV, 0, 0)); //i.worldPos.z / 10; // [-1,1] range
				float altitudeRaw = altitudeTexSample.x;
				float worldActiveMask = saturate(altitudeTexSample.w * 10);

				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(o.altitudeUV, 0, 0)).yzw;
				float depth = saturate(-altitudeRaw + _GlobalWaterLevel);
				float refractionStrength = depth * 4.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				float fadeDuration = 0.33;
				float fadeIn = saturate(groundBitData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - groundBitData.age) / fadeDuration);							
				float alpha = fadeIn * fadeOut;

				worldPosition.z = -altitudeRaw * _MaxAltitude;
				
				float2 scale = float2(1,1) * 1.53641 * alpha; //groundBitData.localScale * alpha * (_CamDistNormalized * 0.75 + 0.25) * 2.0;
			
				float4 resourceGridSample = tex2Dlod(_ResourceGridTex, float4(uv, 0, 0));
				float decomposerAmount = saturate(resourceGridSample.z);
				float decomposerMinMask = saturate(decomposerAmount * 100);
				float wasteAmount = saturate(resourceGridSample.y) * decomposerMinMask;

				float sizeFadeMask = 1;// (decomposerAmount + wasteAmount * 0.5) * 0.9 + 0.1; // saturate((1.0 - altitude) * 4 - 2);
				quadPoint *= float3(scale, 1.0) * sizeFadeMask;
								
				float4 fluidVelocity = tex2Dlod(_VelocityTex, float4(worldPosition.xy / 256, 0, 2));
				float fluidSpeed = length(fluidVelocity.xy);
				float2 fluidDir = float2(0,1); //normalize(fluidVelocity.xy);
				if(fluidSpeed > 0.0000001) {
					fluidDir = normalize(fluidVelocity.xy);
				}

				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0));
				float dotLight = dot(waterSurfaceData.yzw, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				float waveHeight = waterSurfaceData.x;

				// Figure out final facing Vectors!!!
				float2 forward = groundBitData.heading; //float2(0,1); // lerp(groundBitData.heading, fluidDir, saturate(fluidSpeed * 5)); //groundBitData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				float4 pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0)); //mul(UNITY_MATRIX_VP, float4(worldPosition + quadVerticesCBuffer[id] * 1, 1.0)); // *** Revisit to better understand!!!! ***
				o.pos = pos;
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //altitudeUV.xy / altitudeUV.w;

				o.skyUV = worldPosition.xy / _MapSize;
								
				float randNoise1 = Value3D(float3(worldPosition.x - _Time.y * 5.34, worldPosition.y + _Time.y * 7.1, _Time.y * 15), 0.1).x * 0.5 + 0.5; //				
				float randNoise2 = Value3D(float3(worldPosition.x + _Time.y * 7.34, worldPosition.y - _Time.y * 6.1, _Time.y * -10), 0.25).x * 0.5 + 0.5;
				float randNoise3 = Value3D(float3(worldPosition.x + _Time.y * 3.34, worldPosition.y - _Time.y * 5.1, _Time.y * 7.5), 0.36).x * 0.5 + 0.5;
				float randNoise4 = Value3D(float3(worldPosition.x - _Time.y * 8.34, worldPosition.y + _Time.y * 4.1, _Time.y * -3.5),0.55).x * 0.5 + 0.5;
				randNoise1 *= 1;
				randNoise2 *= 1;
				randNoise3 *= 1;
				randNoise4 *= 1;
				float randThreshold = (randNoise1 + randNoise2 + randNoise3 + randNoise4) / 4;	
				float2 sampleUV = screenUV.xy / screenUV.w;
				float vignetteRadius = length((sampleUV - 0.5) * 2);
				float testNewVignetteMask = saturate(((randThreshold + 0.6 - (saturate(vignetteRadius) * 0.4 + 0.3)) * 2));
				o.vignetteLerp = float4(testNewVignetteMask,sampleUV,saturate(vignetteRadius));

				o.patternUV = float2(o.quadUV.x * (1.0 / 8.0) + (1.0 / 8.0) * (float)_PatternColumn, o.quadUV.y * (1.0 / 8.0) + (1.0 / 8.0) * (float)_PatternRow);
				float rand = Value2D(float2((float)inst, (float)inst + 30), 100).x;
				o.color = float4(rand,groundBitData.age,1,alpha);
				
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

				float4 patternTex = tex2D(_PatternTex, i.patternUV);

				ShadingData data;
				data.baseAlbedo = saturate(1.0962 * float4(0.8,0.3,0,1));
				data.altitudeTex = tex2D(_AltitudeTex, i.altitudeUV);
    			data.waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				data.groundNormalsTex = float4(groundSurfaceNormal, 0);
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

				float4 finalColor = data.baseAlbedo;
				float caustics = GetCausticsLight(finalColor, data);
				float diffuse = GetDiffuseLight(finalColor, data);
				float wetnessMod = GetWetnessModifier(data.altitudeTex.x, _GlobalWaterLevel);
				float shoreFoam = GetFoamBrightness(data.altitudeTex.x, _GlobalWaterLevel);
				float fogAmount = GetWaterFogAmount(data.depth * 2);
				float isUnderwater = saturate(data.depth * 57);
				float4 reflectionColor = GetReflectionAmount(data.worldPos, data.worldSpaceCameraPosition.xyz, data.waterSurfaceTex.yzw, data.skyTex, isUnderwater);
				
				finalColor.a = 1;
				finalColor.rgb *= diffuse;
				finalColor.rgb += caustics;
				finalColor.rgb *= wetnessMod;
				finalColor.rgb += shoreFoam;
				finalColor.rgb = lerp(finalColor.rgb, data.waterFogColor.rgb, fogAmount);
				finalColor.rgb += lerp(float3(0,0,0), reflectionColor.xyz, reflectionColor.w);
								
				finalColor.rgb += data.spiritBrushTex.y;
				
				finalColor.a *= tex2D(_MainTex, i.quadUV).a * i.color.a;
				return finalColor;
				// What information is needed and when in order to properly render??? *******
				// Ground Terrain baseAlbedo color? -- either precompute or blend btw stone/pebble/sand colors
				// Height -- Z-Pos of particle
				// Resource distribution tinting -- what colors?  :: algae, decomposers, waste, nutrients
				// Caustics lighting
				// Water Fog amount
				// depth
				// Water fog color
				// global water level
				// terrain normals
				// skyTexture
				// Camera worldPos
				// Sun Pos/Dir
				// SpiritBrush lighting

			}
		ENDCG
		}
	}
}
