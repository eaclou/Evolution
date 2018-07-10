Shader "Critter/CritterEyesStrokesShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
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
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			struct AgentEyeStrokeData {
				int parentIndex;  // what agent/object is this attached to?				
				float2 localPos;
				float2 localDir;
				float2 localScale;
				float3 irisHue;
				float3 pupilHue;
				float strength;  // abstraction for pressure of brushstroke + amount of paint 
				int brushType;  // what texture/mask/brush pattern to use
			};
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<AgentEyeStrokeData> agentEyesStrokesCBuffer;
			
			//StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			//StructuredBuffer<AgentMovementAnimData> agentMovementAnimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float animFrameBlendLerp : TEXCOORD2;
				int2 bufferIndices : TEXCOORD3;  // agent then eye
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				AgentEyeStrokeData eyeData = agentEyesStrokesCBuffer[inst];
				int agentIndex = eyeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				o.bufferIndices = int2(agentIndex, inst);

				float2 critterPosition = critterSimData.worldPos.xy;
				float2 centerToVertexOffset = quadVerticesCBuffer[id];
				
				float growthScale = lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);
				float2 curAgentSize = critterInitData.boundingBoxSize * growthScale;

				// spriteCenterPos!!! ::::  ===========================================================================
				float2 centerPosition = eyeData.localPos;
				// foodBloat (normalized coords -1,1)
				centerPosition = foodBloatAnimPos(centerPosition, eyeData.localPos.y, critterSimData.foodAmount);
				// biteAnim (normalized coords -1, 1)
				centerPosition = biteAnimPos(centerPosition, eyeData.localPos.y, critterSimData.biteAnimCycle);
				// scale coords by agent size? does order of ops matter?
				centerPosition = centerPosition * curAgentSize * 0.5;
				// swimAnim:
				float bodyAspectRatio = critterInitData.boundingBoxSize.y / critterInitData.boundingBoxSize.x;
				float bendStrength = 0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);
				centerPosition = swimAnimPos(centerPosition, eyeData.localPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, bendStrength, critterSimData.turnAmount);
				// rotate with agent:
				centerPosition = rotatePointVector(centerPosition, float2(0,0), critterSimData.heading);

				// vertexOffsetFromSpriteCenter!!! :::: ===============================================================
				//float dotGrowth = saturate(bodyStrokeData.strength * 2.0);
				//float dotDecay = saturate((bodyStrokeData.strength - 0.5) * 2);
				//float dotHealthValue = dotGrowth * (1.0 - dotDecay);
				float aspect = eyeData.localScale.y;				
				float2 eyeLocalScale = float2(eyeData.localScale.x * aspect, eyeData.localScale.x * (1.0 / aspect));
				centerToVertexOffset *= eyeLocalScale * growthScale * (critterInitData.boundingBoxSize.x + critterInitData.boundingBoxSize.y) * 0.5 * (1.0 - critterSimData.decayPercentage);
				float2 forwardGaze = normalize(critterSimData.velocity);
				if(length(critterSimData.velocity) < 0.0001) {
					forwardGaze = critterSimData.heading;
				}
				centerToVertexOffset = rotatePointVector(centerToVertexOffset, float2(0,0), forwardGaze);

				float3 worldPosition = float3(critterPosition + centerPosition + centerToVertexOffset, -0.5);

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.color = float4(1, 1, 1, 1);	// change color of eyes if dead X's?'
				
				const float tilePercentage = (1.0 / 8.0);

				float2 uv0 = quadVerticesCBuffer[id] + 0.5f; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float eyeBrush = 0;  // What eye type variation? horizontal axis <-->
				uv0.x *= tilePercentage; 
				uv0.x += tilePercentage * eyeBrush;
				uv1.x *= tilePercentage; 
				uv1.x += tilePercentage * eyeBrush;

				// Figure out how much to blur:
				// Y coords = animation frame!  // 0 = normal, 1 = closed, 2 = dead
				float animFrame = 0; // sin(agentSimData.eatingStatus * 3.141592) * 0.99;	
				if(critterSimData.decayPercentage > 0) {
					animFrame = 2;
				}
				float frameRow0 = floor(animFrame);  // 0-2
				float frameRow1 = ceil(animFrame); // 1-3
				float animFrameBlendLerp = animFrame - frameRow0;
				// calculate UV's to sample from correct rows:
				uv0.y = uv0.y * tilePercentage + tilePercentage * frameRow0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * frameRow1;
				
				// What percentage of frame interpolation btw current and next frame
				o.animFrameBlendLerp = animFrameBlendLerp;				
				o.uv = float4(uv0, uv1);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				AgentEyeStrokeData eyeData = agentEyesStrokesCBuffer[i.bufferIndices.y];
				CritterInitData critterInitData = critterInitDataCBuffer[i.bufferIndices.x];
				CritterSimData critterSimData = critterSimDataCBuffer[i.bufferIndices.x];
				
				float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = lerp(texColor0, texColor1, i.animFrameBlendLerp);

				float3 rgb = critterInitData.primaryHue;
				rgb = lerp(rgb, float3(1,1,1), brushColor.r);  // White of the eye
				rgb = lerp(rgb, eyeData.irisHue, brushColor.g);  // Iris
				rgb = lerp(rgb, eyeData.pupilHue, brushColor.b);  // Pupil
				
				float4 finalColor = float4(rgb * 1.25, brushColor.a);
				finalColor.rgb = lerp(float3(0.45, 0.45, 0.45), finalColor.rgb, saturate(critterSimData.energy * 5));

				//return float4(1,1,1,1);
				return finalColor;
				
			}
		ENDCG
		}
	}
}
