Shader "Unlit/PointStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		//_Tint("Color", Color) = (1,1,1,1)
		//_Size("Size", vector) = (1,1,1,1)
	}
	SubShader
	{		
		Tags{ "RenderType" = "Transparant" }
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			struct PointStrokeData {
				int parentIndex;  // what agent/object is this attached to?
				float2 localScale;
				float2 localPos;
				float2 localDir;
				float3 hue;   // RGB color tint
				float strength;  // abstraction for pressure of brushstroke + amount of paint 
				int brushType;  // what texture/mask/brush pattern to use
			};

			struct AgentSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float2 size;
			};


			StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			StructuredBuffer<PointStrokeData> pointStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				PointStrokeData pointStrokeData = pointStrokesCBuffer[inst];
				AgentSimData agentSimData = agentSimDataCBuffer[pointStrokeData.parentIndex];

				float3 worldPosition = float3(agentSimData.worldPos, -0.5);
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = agentSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				float2 positionOffset = float2(pointStrokeData.localPos.x * agentSimData.size.x * rightAgent + pointStrokeData.localPos.y * agentSimData.size.y * forwardAgent);
				worldPosition.xy += positionOffset; // Place properly

				float3 quadPoint = quadVerticesCBuffer[id];
				
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomAspect = lerp(0.75, 1.33, random1);
				float randomValue = 1; //rand(float2(inst, randomAspect * 10));
				
				float2 scale = pointStrokeData.localScale * agentSimData.size;
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				float2 forward0 = agentSimData.heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float2 rotatedPoint0 = float2(pointStrokeData.localDir.x * right0 + pointStrokeData.localDir.y * forward0);  // Rotate localRotation by AgentRotation

				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);
				
				//o.pos = mul(UNITY_MATRIX_VP, float4(rotatedPoint, 0.0f));
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint1, 0.0f));
				o.color = float4(pointStrokeData.hue,1);
				
				float2 uvs = quadVerticesCBuffer[id] + 0.5f; // full texture
				float randBrush = pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				uvs.x *= 0.25;  // 4 brushes on texture
				uvs.x += 0.25 * randBrush;
				o.uv = uvs;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				float4 finalColor = float4(i.color) * texColor;
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
