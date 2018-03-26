Shader "Unlit/TrailStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD1;
				float2 segmentUV : TEXCOORD2;
			};

			struct TrailStrokeData {
				float2 worldPos;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
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
			StructuredBuffer<TrailStrokeData> agentTrailStrokesReadCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				TrailStrokeData trailStrokeData = agentTrailStrokesReadCBuffer[inst];

				uint trailPointIndex = inst;
				uint localTrailIndex = trailPointIndex % 16;
				uint agentIndex = floor(inst / 16);

				AgentSimData agentData = agentSimDataCBuffer[agentIndex];

				float3 worldPosition = float3(trailStrokeData.worldPos, -0.1);

				o.segmentUV = quadVerticesCBuffer[id].xy + 0.5;
				
				float2 vertexPos = quadVerticesCBuffer[id].xy;
				float2 uv = vertexPos + 0.5;
				uv.y = 1 - uv.y;
				vertexPos.y += 0.5;

				float localTrailPercentage = (float)localTrailIndex / 16.0;
				uv.y = uv.y / 32.0 + localTrailPercentage;
				
				float2 scale = float2(0.24 * (1 + (agentData.size.x - 1) * 0.55), 0.25);
				scale.x += (1 - localTrailPercentage) * 0.2;  // Taper
				scale.x *= (1 + (agentData.secondaryHue.g - 0.5) * 2);
				//scale *= 1.4;

				if(localTrailIndex != 0) {
					float2 parentPos = agentTrailStrokesReadCBuffer[inst - 1].worldPos;
					scale.y = clamp(length(parentPos - trailStrokeData.worldPos) * 1.65, 0.0001, 0.75);
					vertexPos *= scale;

					float2 forward = normalize(parentPos - trailStrokeData.worldPos);
					float2 right = float2(forward.y, -forward.x);
					// With final facing Vectors, find rotation of QuadPoints:
					
					vertexPos = float2(vertexPos.x * right + vertexPos.y * forward);

				}
				else {
					vertexPos *= scale;
				}	
				
				float alpha = 1;
				alpha = alpha * agentData.maturity;
				alpha = alpha * (1.0 - agentData.decay);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(vertexPos, 0.0, 0.0f));
				//o.pos = UnityObjectToClipPos(float4(worldPosition, 1.0) + float4(vertexPos, 0.0, 0.0));
				float3 tailHue = lerp(agentData.primaryHue, agentData.secondaryHue, localTrailPercentage);
				tailHue = lerp(float3(0.5, 0.5, 0.5), tailHue, saturate(agentData.maturity * 4 - 3));
				tailHue = lerp(float3(0.5, 0.5, 0.5), tailHue, saturate(agentData.foodAmount * 4));
				o.color = float4(tailHue,alpha);			
				o.uv = uv;

				return o;				
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				float4 quadCol = tex2D(_MainTex, i.segmentUV);
				col.a *= quadCol.a;
				
				return col;
			}
			ENDCG
		}
	}
}
