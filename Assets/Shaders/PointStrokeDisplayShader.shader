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

				float3 worldPosition = float3(agentSimData.worldPos + pointStrokeData.localPos, -0.5);
				float3 quadPoint = quadVerticesCBuffer[id];

				//float2 velocity = floatyBitData.zw;

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				float randomAspect = lerp(0.75, 1.33, random1);
				float randomValue = 1; //rand(float2(inst, randomAspect * 10));
				
				float2 scale = pointStrokeData.localScale;
				quadPoint *= float3(scale, 1.0);

				float2 forward0 = agentSimData.heading; //float2(0,1);
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float2 rotatedPoint0 = float2(pointStrokeData.localDir.x * right0 + pointStrokeData.localDir.y * forward0);

				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);
				
				//o.pos = mul(UNITY_MATRIX_VP, float4(rotatedPoint, 0.0f));
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint1, 0.0f));
				o.color = float4(pointStrokeData.hue,1);
				o.uv = quadVerticesCBuffer[id] + 0.5f;
				
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
