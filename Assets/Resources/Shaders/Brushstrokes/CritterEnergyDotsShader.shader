Shader "Critter/CritterEnergyDotsShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}		
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
			
			struct CritterStrokeData {
				int parentIndex;  // what agent/object is this attached to?				
				float2 worldPos;
				float2 localPos;
				float2 localDir;
				float2 localScale;
				float strength;  // abstraction for pressure of brushstroke + amount of paint 
				float lifeStatus;
				int brushType;
			};
			struct CritterInitData {
				float2 boundingBoxSize;
				float spawnSizePercentage;
				float maxEnergy;
				float3 primaryHue;
				float3 secondaryHue;
				int bodyPatternX;  // what grid cell of texture sheet to use
				int bodyPatternY;  // what grid cell of texture sheet to use
			};
			struct CritterSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float growthPercentage;
				float decayPercentage;
				float foodAmount;
				float energy;
				float health;
				float stamina;
				float isBiting;
				float biteAnimCycle;
				float moveAnimCycle;
				float turnAmount;
				float accel;
				float smoothedThrottle;
			};

			struct AgentSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float2 size;
				float3 primaryHue;  // can eventually pull these static variables out of here to avoid per-frame updates on non-dynamic attributes
				float3 secondaryHue;
				float maturity;
				float decay;
				float eatingStatus;
				float foodAmount;
			};

			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<CritterStrokeData> bodyStrokesCBuffer;

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
								
				CritterStrokeData bodyStrokeData = bodyStrokesCBuffer[inst];
				uint agentIndex = bodyStrokeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				float2 critterPosition = critterSimData.worldPos.xy;
				float2 centerToVertexOffset = quadVerticesCBuffer[id];
				
				float2 curAgentSize = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);

				// spriteCenterPos!!! ::::  ===========================================================================
				float2 centerPosition = bodyStrokeData.localPos * 0.9;
				float v = centerPosition.y;
				// foodBloat (normalized coords -1,1)
				centerPosition = foodBloatAnimPos(centerPosition, bodyStrokeData.localPos.y, critterSimData.foodAmount);
				// biteAnim (normalized coords -1, 1)
				centerPosition = biteAnimPos(centerPosition, bodyStrokeData.localPos.y, critterSimData.biteAnimCycle);
				// scale coords by agent size? does order of ops matter?
				centerPosition = centerPosition * curAgentSize * 0.5;
				// swimAnim:
				float bodyAspectRatio = critterInitData.boundingBoxSize.y / critterInitData.boundingBoxSize.x;
				float bendStrength = 0.5; // * saturate(bodyAspectRatio * 0.5 - 0.4);
				float swimAngle = getSwimAngle(bodyStrokeData.localPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, bendStrength, critterSimData.turnAmount);

				centerPosition = rotate_point(float2(0,0), swimAngle, centerPosition);
				// rotate with agent:
				centerPosition = rotatePointVector(centerPosition, float2(0,0), critterSimData.heading);

				
				//centerToVertexOffset *= bodyStrokeData.localScale * length(curAgentSize) * saturate(critterSimData.energy / critterInitData.maxEnergy) * 0.2 * (1.0 - critterSimData.decayPercentage);
				//centerToVertexOffset = rotatePointVector(centerToVertexOffset, float2(0,0), critterSimData.heading);


				//======================================================================================================================&&&&&&&&&&
				
				//float dotGrowth = saturate(bodyStrokeData.strength * 2.0);
				//float dotDecay = saturate((bodyStrokeData.strength - 0.5) * 2);
				//float dotHealthValue = dotGrowth * (1.0 - dotDecay);
				
				centerToVertexOffset *= bodyStrokeData.localScale * curAgentSize * 0.1;
				centerToVertexOffset.x *= 1.33;
				
				centerToVertexOffset = rotate_point(float2(0,0), swimAngle, centerToVertexOffset);
				centerToVertexOffset = rotatePointVector(centerToVertexOffset, float2(0,0), critterSimData.heading);
				//========================================================================================================================&&&&&&&&&
				
				float3 worldPosition = float3(critterPosition + centerPosition + centerToVertexOffset, -0.5);

				//======================================================================================================

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
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture start Row
				
				return float4(0.25,0.9,2.4,texColor.a);

				
			}
		ENDCG
		}
	}
}
