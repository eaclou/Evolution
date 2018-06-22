Shader "Critter/CritterBodyStrokesShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
		//_BumpMap("Normal Map", 2D) = "bump" {}		
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
			sampler2D _PatternTex;
			//sampler2D _BumpMap;
			
			struct BodyStrokeData {
				int parentIndex;  // what agent/object is this attached to?				
				float2 localPos;
				float2 localDir;
				float2 localScale;
				float strength;  // abstraction for pressure of brushstroke + amount of paint 
				int brushTypeX;  // what texture/mask/brush pattern to use
				int brushTypeY;
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

			StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			StructuredBuffer<BodyStrokeData> bodyStrokesCBuffer;
			StructuredBuffer<AgentMovementAnimData> agentMovementAnimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float3 worldPos : TEXCOORD6;
				float2 uvPattern : TEXCOORD8;
				int2 bufferIndices : TEXCOORD9;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				BodyStrokeData bodyStrokeData = bodyStrokesCBuffer[inst];
				uint agentIndex = bodyStrokeData.parentIndex;
				AgentSimData agentSimData = agentSimDataCBuffer[agentIndex];
				AgentMovementAnimData animData = agentMovementAnimDataCBuffer[agentIndex];
				o.bufferIndices = int2(inst, agentIndex);
								
				float3 worldPosition = float3(agentSimData.worldPos, -0.5);

				float3 quadPoint = quadVerticesCBuffer[id];				
				float2 scale = bodyStrokeData.localScale;  // assume bodyStroke is 1:1 agentSize
				
				quadPoint *= float3(scale, 1.0);

				
				//float clock = _Time.y;

				float2 localPosition = bodyStrokeData.localPos;
				localPosition = getWarpedPoint(localPosition, 
												bodyStrokeData.localPos, 
												quadPoint.xy,
												animData.turnAmount, 
												animData.animCycle, 
												animData.accel, 
												animData.smoothedThrottle,
												agentSimData.foodAmount,
												agentSimData.size,
												agentSimData.eatingStatus);				


				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = agentSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);

				localPosition = localPosition.x * rightAgent + localPosition.y * forwardAgent;

				//worldPosition.xy = getWarpedPoint(worldPosition.xy, agentSimData.worldPos.xy, animData.turnAmount, panningYawStrength, 0, animData.animCycle, animData.accel, animData.smoothedThrottle);
				worldPosition.xy += localPosition;
				// NoiseWobble offset:
				//float clock = _Time.y;

				// Figure out final facing Vectors!!!
				float2 forward0 = agentSimData.heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float2 rotatedPoint0 = float2(bodyStrokeData.localDir.x * right0 + bodyStrokeData.localDir.y * forward0);  // Rotate localRotation by AgentRotation

				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);

				float alpha = 1;
				alpha = alpha * (1.0 - agentSimData.decay);
				float activeColorLerp = 1;
				activeColorLerp = activeColorLerp * floor(agentSimData.maturity + 0.01);
				activeColorLerp = activeColorLerp * floor((1.0 - agentSimData.decay) + 0.01);
				
				//o.pos = mul(UNITY_MATRIX_VP, float4(rotatedPoint, 0.0f));
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)));
				o.worldPos = worldPosition + rotatedPoint1;
				o.color = lerp(float4(0.5, 0.5, 0.5, alpha), float4(0,0,1,alpha), activeColorLerp);	

				const float tilePercentage = (1.0 / 8.0);

				float2 baseUV = quadVerticesCBuffer[id] + 0.5f;				

				// PATTERNS UV:
				float2 patternUV = bodyStrokeData.localPos;
				float randPatternIDX = bodyStrokeData.brushTypeX;
				float randPatternIDY = fmod(bodyStrokeData.brushTypeY, 4); // ********** UPDATE!!! **************
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;
				o.uvPattern = patternUV;
								
				o.uv = baseUV;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				BodyStrokeData bodyStrokeData = bodyStrokesCBuffer[i.bufferIndices.x];
				AgentSimData agentSimData = agentSimDataCBuffer[i.bufferIndices.y];

				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture start Row
				//float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row

				float4 patternSample = tex2Dlod(_PatternTex, float4(i.uvPattern, 0, 5));
				
				//float4 brushColor = lerp(texColor0, texColor1, i.frameBlendLerp);

				float4 finalColor = float4(lerp(agentSimData.primaryHue, agentSimData.secondaryHue, patternSample.x), texColor.a);
				

				return finalColor;
				
			}
		ENDCG
		}
	}
}
