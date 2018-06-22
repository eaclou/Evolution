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
			StructuredBuffer<AgentEyeStrokeData> agentEyesStrokesCBuffer;
			StructuredBuffer<AgentMovementAnimData> agentMovementAnimDataCBuffer;
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
				AgentSimData agentSimData = agentSimDataCBuffer[eyeData.parentIndex];
				AgentMovementAnimData animData = agentMovementAnimDataCBuffer[eyeData.parentIndex];
				o.bufferIndices = int2(eyeData.parentIndex, inst);

				float3 quadPoint = quadVerticesCBuffer[id];
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float aspect = eyeData.localScale.y;				
				float2 scale = float2(eyeData.localScale.x * aspect, eyeData.localScale.x * (1.0 / aspect)); // * saturate(agentSimData.maturity * 4 - 3);
				quadPoint *= float3(scale, 1.0);

				float3 worldPosition = float3(agentSimData.worldPos, -0.5);
				
				// OLD:
				//float2 positionOffset = float2(eyeData.localPos.x * agentSimData.size.x * rightAgent + eyeData.localPos.y * agentSimData.size.y * forwardAgent) * 0.5;

				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = agentSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				// Figure out final facing Vectors!!!
				float2 forwardGaze = normalize(agentSimData.velocity);
				if(length(agentSimData.velocity) < 0.0001) {
					forwardGaze = forwardAgent;
				}
				float2 rightGaze = float2(forwardGaze.y, -forwardGaze.x);
				float2 rotatedPoint0 = float2(eyeData.localDir.x * rightGaze + eyeData.localDir.y * forwardGaze);  // Rotate localRotation by AgentRotation
				//float2 rotatedQuad = float2(quadPoint.x / agentSimData.size.x * rightGaze + quadPoint.y / agentSimData.size.y * forwardGaze); 
				//float2 rotatedQuad = quadPoint / agentSimData.size;
				float2 rotatedQuad = float2(quadPoint.x * rightGaze + quadPoint.y * forwardGaze); 
				rotatedQuad = rotatedQuad / agentSimData.size * 2 * agentSimData.maturity;
				float2 positionOffset = eyeData.localPos;
				
				positionOffset = getWarpedPoint(positionOffset, 
												eyeData.localPos, 
												rotatedQuad,
												animData.turnAmount, 
												animData.animCycle, 
												animData.accel, 
												animData.smoothedThrottle,
												agentSimData.foodAmount,
												agentSimData.size,
												agentSimData.eatingStatus);
				
				
				positionOffset = positionOffset.x * rightAgent + positionOffset.y * forwardAgent;
				
				worldPosition.xy += positionOffset; // Place properly

				
				
				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);
				
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(0,0,-1, 1.0)) + float4(quadPoint* 10, 0.0));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint1, 0.0));
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
				//if(agentSimData.decay > 0) {
				//	animFrame = 2;
				//}
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
				AgentSimData agentSimData = agentSimDataCBuffer[i.bufferIndices.x];
				
				float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = lerp(texColor0, texColor1, i.animFrameBlendLerp);

				float3 rgb = agentSimData.primaryHue;
				rgb = lerp(rgb, float3(1,1,1), brushColor.r);  // White of the eye
				rgb = lerp(rgb, eyeData.irisHue, brushColor.g);  // Iris
				rgb = lerp(rgb, eyeData.pupilHue, brushColor.b);  // Pupil
				
				float4 finalColor = float4(rgb, brushColor.a); // float4(rgb, brushColor.a * saturate(2.0 - agentSimData.decay * 2));

				//if(agentSimData.maturity < 0.99) {
				//finalColor.a *= saturate(agentSimData.maturity * 4 - 3);
				//}
				
				//return float4(1,1,1,1);
				return finalColor;
				
			}
		ENDCG
		}
	}
}
