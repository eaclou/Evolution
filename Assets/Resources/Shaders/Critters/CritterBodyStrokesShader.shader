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
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			sampler2D _PatternTex;
			//sampler2D _BumpMap;
						

			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<CritterBodyStrokeData> bodyStrokesCBuffer;

			//StructuredBuffer<AgentSimData> agentSimDataCBuffer;			
			//StructuredBuffer<AgentMovementAnimData> agentMovementAnimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float3 worldPos : TEXCOORD6;
				float3 worldNormal : TEXCOORD7;
				float2 uvPattern : TEXCOORD8;
				int2 bufferIndices : TEXCOORD9;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				CritterBodyStrokeData bodyStrokeData = bodyStrokesCBuffer[inst];
				uint agentIndex = bodyStrokeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				o.bufferIndices = int2(inst, agentIndex);
				
				float2 critterPosition = critterSimData.worldPos.xy;
				float2 centerToVertexOffset = quadVerticesCBuffer[id];
				
				float2 curAgentSize = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);
				
				// ******
				//curAgentSize *= 5;
				//*****

				float dotGrowth = saturate(bodyStrokeData.strength * 2.0);
				float dotDecay = saturate((bodyStrokeData.strength - 0.5) * 2);
				float dotHealthValue = dotGrowth * (1.0 - dotDecay);
				centerToVertexOffset *= bodyStrokeData.localScale * curAgentSize * dotHealthValue * 1.7;
				centerToVertexOffset.x *= 1.33;

				float bodyAspectRatio = critterInitData.boundingBoxSize.y / critterInitData.boundingBoxSize.x;
				float bendStrength = 0.5; // * saturate(bodyAspectRatio * 0.5 - 0.4);
				float swimAngle = getSwimAngle(bodyStrokeData.localPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, bendStrength, critterSimData.turnAmount);
				centerToVertexOffset = rotate_point(float2(0,0), swimAngle, centerToVertexOffset);
				centerToVertexOffset = rotatePointVector(centerToVertexOffset, float2(0,0), critterSimData.heading);

				float3 worldPosition = float3(bodyStrokeData.worldPos + centerToVertexOffset, curAgentSize.x);
				
				float3 localNormal = normalize(bodyStrokeData.localPos);
				float3 worldNormal = localNormal;
				worldNormal.xy = float2(localNormal.x * cos(swimAngle) - localNormal.y * sin(swimAngle), localNormal.y * cos(swimAngle) + localNormal.x * sin(swimAngle));
				
				o.worldNormal = worldNormal;
				
				float alpha = saturate(1.0 - critterSimData.decayPercentage * 1.2);

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)));
				o.worldPos = worldPosition; // + rotatedPoint1;

				float2 fakeNml = rotatePointVector(normalize(bodyStrokeData.localPos), float2(0,0), critterSimData.heading);
				float diffuse = saturate(dot(fakeNml, float2(0,1)));
				
				o.color = float4(saturate(dotHealthValue * 10),bodyStrokeData.lifeStatus,diffuse,alpha);

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
				//CritterBodyStrokeData bodyStrokeData = bodyStrokesCBuffer[i.bufferIndices.x];
				//AgentSimData agentSimData = agentSimDataCBuffer[i.bufferIndices.y];
				CritterInitData critterInitData = critterInitDataCBuffer[i.bufferIndices.y];
				CritterSimData critterSimData = critterSimDataCBuffer[i.bufferIndices.y];

				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture start Row
				//float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row

				float4 patternSample = tex2Dlod(_PatternTex, float4(i.uvPattern, 0, 3));
				
				//float4 brushColor = lerp(texColor0, texColor1, i.frameBlendLerp);

				// *** Better way to handle this???
				float4 finalColor = float4(lerp(critterInitData.primaryHue, critterInitData.secondaryHue, patternSample.x), texColor.a);
				finalColor.a *= i.color.a;
				finalColor.rgb = lerp(float3(1, 0.5, 0.5), finalColor.rgb, i.color.g) * 0.75;
				//finalColor.rgb *= lerp(1, i.color.b * 1.5, 0.25);
				//finalColor.rgb = lerp(float3(0.65, 0.65, 0.65), finalColor.rgb, saturate(critterSimData.growthPercentage * 10));
				//finalColor.rgb = lerp(float3(0.45, 0.45, 0.45), finalColor.rgb, saturate(critterSimData.energy * 5));
				
				//finalColor.rgb += 0.35;
				//finalColor.rgb = float3(0.15 ,0.4 ,0.8);

				// how to keep track of proper surface normal??? do it int he compute shader and store it?
				// Rotate in direction of agent for now:
				//float3 normal = bodyStrokeData.localPos;
				//float angle = 2.0;
				//normal.xy = float2(bodyStrokeData.localPos.x * cos(angle) - bodyStrokeData.localPos.y * sin(angle), bodyStrokeData.localPos.y * cos(angle) + bodyStrokeData.localPos.x * sin(angle));

				float3 lightDir = normalize(float3(1, 0, 0));
				float diffuseLight = dot(lightDir, normalize(i.worldNormal));
				return float4(diffuseLight,diffuseLight,diffuseLight,1);

				return finalColor;
				
			}
		ENDCG
		}
	}
}
