Shader "Critter/EggSackShadowDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet
		_MatureToDecayTex ("_MatureToDecayTex", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
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
			#include "Assets/Resources/Shaders/Inc/StructsEggData.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			sampler2D _MatureToDecayTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _AltitudeTex;	

			sampler2D _RenderedSceneRT;
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<EggData> eggDataCBuffer;
			StructuredBuffer<EggSackSimData> eggSackSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform float _MapSize;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				int foodIndex : TEXCOORD2;
				float frameLerp : TEXCOORD3;
				float2 quadCoords : TEXCOORD4;
				float decayPercentage : TEXCOORD5;
				float2 altitudeUV : TEXCOORD6;
				float4 screenUV : TEXCOORD7;
				float3 worldPos : TEXCOORD8;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			
			float length_squared(float2 v, float2 w) {
				float2 r = w - v;
				float distSqr = (r.x * r.x + r.y * r.y);
				return distSqr;
			}

			float minimum_distance(float2 v, float2 w, float2 p) {
			  // Return minimum distance between line segment vw and point p
			  const float l2 = length_squared(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
			  if (l2 == 0.0) {
				return length(v - p);   // v == w case
			  }
			  // Consider the line extending the segment, parameterized as v + t (w - v).
			  // We find projection of point p onto the line. 
			  // It falls where t = [(p-v) . (w-v)] / |w-v|^2
			  // We clamp t from [0,1] to handle points outside the segment vw.
			  const float t = max(0, min(1, dot(p - v, w - v) / l2));
			  const float2 projection = v + t * (w - v);  // Projection falls on the segment
			  return distance(p, projection);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
							
				EggData eggData = eggDataCBuffer[inst];
				EggSackSimData rawData = eggSackSimDataCBuffer[eggData.eggSackIndex];

				float orderVal = fmod(inst, 64) / 64;
				float numEggsNormalized = rawData.health;
				float numEggsMask = saturate((numEggsNormalized - (1.0 - orderVal)) * 1000);
				numEggsMask *= numEggsMask; // Purely Cosmetic -- tends too look sparser

				float distToCore = minimum_distance(float2(0,-0.5), float2(0,0.5), eggData.localCoords);
												
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				float randomScale = lerp(1, 1.25, random2);				
				float scale = length(rawData.fullSize) * rawData.growth * randomScale * 0.35;
						
				quadPoint *= scale * 0.4 * ((1.0 - rawData.decay));

				// Figure out final facing Vectors!!!
				float rotationAngle = random1 * 10.0 * 3.141592;  // radians
				float2 forward0 = rawData.heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float2 rotatedPoint0 = float2(cos(rotationAngle) * right0 + sin(rotationAngle) * forward0);  // Rotate localRotation by AgentRotation

				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);
				
				rotatedPoint1 *= numEggsMask * 1.15;
								
				// *** $^@$%^#$% TESTING @#^*******
				float3 worldPosition = float3(eggData.worldPos, orderVal * scale + (1.0 + scale * 0.15));

				// REFRACTION:
				//float2 offset = worldPosition.xy; //skinStrokeData.worldPos;				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0)).yzw;
				float refractionStrength = 2.45;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV;

				float2 altUV = worldPosition.xy / _MapSize;	
				o.altitudeUV = altUV;
				worldPosition.z = -(tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x * 2 - 1) * 10;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint1, 0.0));
				o.worldPos = worldPosition;
				
				float2 uv0 = quadVerticesCBuffer[id] + 0.5; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float randBrush = 1; //fmod((float)rawData.brushType, 2); //rawData.fruitBrushType; //pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				const float tilePercentage = (1.0 / 8.0);
				uv0.x *= tilePercentage;  // 8 brushes on texture
				uv0.x += tilePercentage * randBrush;
				uv1.x *= tilePercentage;  // 5 brushes on texture
				uv1.x += tilePercentage * randBrush;

				// Figure out how much to blur:
				//float eatenLerp = saturate(((distToCore + 0.065) * 1.58 - rawData.foodAmount) * 9);
				//eatenLerp = max(eatenLerp, rawData.growth * 0.18);				
				float frameLerp = rawData.growth * 4 + rawData.decay * 3 + random2 * 0.1; // (rawData.growth * 0.5 + (1.0 - rawData.decay) * 0.5) * 7; //saturate(growthLerp) * 3;
			
				// Can use this for random variations?
				float row0 = 0; //floor(frameLerp);  // 0-6
				float row1 = 1; //clamp(row0 + 1, 1, 7); // 1-7
				
				uv0.y = uv0.y * tilePercentage + tilePercentage * row0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * row1;

				float3 primaryHue = critterInitDataCBuffer[rawData.parentAgentIndex].primaryHue;
				float3 secondaryHue = critterInitDataCBuffer[rawData.parentAgentIndex].secondaryHue;

				o.color = float4(lerp(primaryHue, secondaryHue, random1), rawData.health);
				o.frameLerp = rawData.growth; //frameLerp - row0;
				o.uv = float4(uv0, uv1);
				o.foodIndex = eggData.eggSackIndex;
				o.quadCoords = quadVerticesCBuffer[id].xy;	
				o.decayPercentage = rawData.decay;
				
				//o.color = float4(rawData.foodAmount, rawData.growth, rawData.health, rawData.decay);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				EggSackSimData rawData = eggSackSimDataCBuffer[i.foodIndex];

				float4 growTexColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 growTexColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row				
				float4 growBrushColor = lerp(growTexColor0, growTexColor1, i.frameLerp);
				float4 finalGrowColor = growBrushColor;

				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	

				float4 backgroundColor = frameBufferColor;
				//backgroundColor.a = texColor.a;
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); // i.worldPos.z / 10; // [-1,1] range
				// 0-1 range --> -1 to 1
				altitude = (altitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				float strataColorMultiplier = (sin(altitude * (1.0 + i.worldPos.x * 0.01 - i.worldPos.y * -0.01) + i.worldPos.x * 0.01 - i.worldPos.y * 0.01) * 0.5 + 0.5) * 0.5 + 0.5;
				backgroundColor.rgb *= strataColorMultiplier;				
				backgroundColor.rgb = lerp(backgroundColor.rgb, waterFogColor, 1 * (saturate(altitude * 0.8)) + 0.25 * isUnderwater);

				float snowAmount = saturate((-altitude - 0.6) * 2 +
								   ((sin(i.worldPos.x * 0.0785 + i.worldPos.y * 0.02843) * 0.5 + 0.5) * 1 - 
								   (cos(i.worldPos.x * 0.012685 + i.worldPos.y * -0.01843) * 0.5 + 0.5) * 0.9 +
								   (sin(i.worldPos.x * 0.2685 + i.worldPos.y * -0.1843) * 0.5 + 0.5) * 0.45 - 
								   (cos(i.worldPos.x * -0.2843 + i.worldPos.y * 0.01143) * 0.5 + 0.5) * 0.45 +
								   (sin(i.worldPos.x * 0.1685 + i.worldPos.y * -0.03843) * 0.5 + 0.5) * 0.3 - 
								   (cos(i.worldPos.x * -0.1843 + i.worldPos.y * 0.243) * 0.5 + 0.5) * 0.3) * 0.5);
				
				backgroundColor.rgb = lerp(backgroundColor.rgb, float3(0.56, 1, 0.34) * 0.6, snowAmount * 1);
				//==================================================================================================================
				backgroundColor.a *= isUnderwater;

				float fogAmount = saturate(i.worldPos.z * 0.5);
				
				//finalGrowColor.rgb = lerp(finalGrowColor.rgb, i.color.rgb, growBrushColor.r); //); // temp flower color use stem color
				//finalGrowColor.rgb = lerp(finalGrowColor.rgb, float3(0.1,0.1,0.1), growBrushColor.g * 0.5);								
				//finalGrowColor.rgb = lerp(finalGrowColor.rgb, float3(0.4,0.4,0.4), i.decayPercentage);

				finalGrowColor.rgb = float3(0,0,0);
				finalGrowColor.rgb = lerp(finalGrowColor.rgb, waterFogColor, fogAmount);
				finalGrowColor.a *= 0.65;
				finalGrowColor.a *= growBrushColor.r;

				return finalGrowColor;
			}
		ENDCG
		}
	}
}
