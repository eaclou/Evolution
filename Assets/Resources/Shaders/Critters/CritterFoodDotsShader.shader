Shader "Critter/CritterFoodDotsShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}		
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
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
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"

			sampler2D _MainTex;
			sampler2D _WaterSurfaceTex;
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<CritterSkinStrokeData> bodyStrokesCBuffer;

			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float3 worldPos : TEXCOORD6;
				float2 uvPattern : TEXCOORD8;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				CritterSkinStrokeData bodyStrokeData = bodyStrokesCBuffer[inst];
				uint agentIndex = bodyStrokeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];
				/*
				float2 critterPosition = critterSimData.worldPos.xy;
				float2 centerToVertexOffset = quadVerticesCBuffer[id];
				
				float growthScale = lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);
				float2 curAgentSize = critterInitData.boundingBoxSize * growthScale;

				float2 centerPosition = bodyStrokeData.localPos * 0.7 * (critterSimData.foodAmount * 0.5 + 0.5);
				centerPosition.y *= 0.6;
				float v = centerPosition.y;
				
				float bodyAspectRatio = critterInitData.boundingBoxSize.y / critterInitData.boundingBoxSize.x;
				float bendStrength = 0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);
				
				centerToVertexOffset *= bodyStrokeData.localScale * (curAgentSize.x + curAgentSize.y) / 2.5 * critterSimData.foodAmount * (1.0 - critterSimData.decayPercentage);
				
				float3 worldPosition = float3(critterPosition + centerPosition + centerToVertexOffset, -0.5);
				*/

				
				float3 quadPoint = quadVerticesCBuffer[id];		

				float3 critterWorldPos = critterSimData.worldPos;
				float3 critterCurScale = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 0.5;
		
				float3 spriteLocalPos = bodyStrokeData.localPos * critterCurScale;
				
				float3 spriteWorldOffset = spriteLocalPos * critterSimData.foodAmount * 0.88 * (1.0 - critterSimData.decayPercentage); // **** Vector from critter origin to sprite origin		
				// SWIM ANIMS:
				spriteWorldOffset = GetAnimatedPosOld(spriteWorldOffset, float3(0,0,0), critterInitData, critterSimData, bodyStrokeData.localPos);		

				float3 worldPosition = critterWorldPos + spriteWorldOffset + quadPoint * 0.35 * critterSimData.foodAmount * critterCurScale * (1.0 - critterSimData.decayPercentage);

				// REFRACTION:
				//float3 offset = spriteWorldOffset;				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				float refractionStrength = 2.45;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
			
				

				float alpha = saturate(1.0 - critterSimData.decayPercentage * 1.2);

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)));
				o.worldPos = worldPosition; // + rotatedPoint1;
				o.color = float4(1,1,1,alpha);	

				const float tilePercentage = (1.0 / 8.0);

				float2 baseUV = quadVerticesCBuffer[id] + 0.5f;				

				// PATTERNS UV:
				float2 patternUV = bodyStrokeData.localPos * 0.5 + 0.5;
				float randPatternIDX = critterInitData.bodyPatternX; // ** This is now inside CritterInitData!!!!!
				float randPatternIDY = critterInitData.bodyPatternY; //  fmod(bodyStrokeData.brushTypeY, 4); // ********** UPDATE!!! **************
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;
				o.uvPattern = patternUV;
								
				o.uv = baseUV;

				o.worldPos = float3(128,128,-1) + quadVerticesCBuffer[id] * 20;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				//return float4(1,1,1,1);

				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture start Row
				
				return float4(0.2,1,0.2,texColor.a);
							
				
			}
		ENDCG
		}
	}
}
