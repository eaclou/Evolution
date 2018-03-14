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
			};

			struct TrailStrokeData {
				float2 worldPos;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;

			StructuredBuffer<TrailStrokeData> agentTrailStrokesReadCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				TrailStrokeData trailStrokeData = agentTrailStrokesReadCBuffer[inst];

				uint trailPointIndex = inst;
				uint localTrailIndex = trailPointIndex % 32;

				float3 worldPosition = float3(trailStrokeData.worldPos, -0.1);
				
				float2 vertexPos = quadVerticesCBuffer[id].xy;
				float2 uv = vertexPos + 0.5;
				uv.y = 1 - uv.y;
				vertexPos.y += 0.5;

				float localTrailPercentage = (float)localTrailIndex / 32.0;
				uv.y = uv.y / 32.0 + localTrailPercentage;
				
				float2 scale = float2(0.1, 0.1);
				scale.x += (1 - localTrailPercentage) * 0.15;

				if(localTrailIndex != 0) {
					float2 parentPos = agentTrailStrokesReadCBuffer[inst - 1].worldPos;
					scale.y = clamp(length(parentPos - trailStrokeData.worldPos), 0.0001, 0.5);
					vertexPos *= scale;

					float2 forward = normalize(parentPos - trailStrokeData.worldPos);
					float2 right = float2(forward.y, -forward.x);
					// With final facing Vectors, find rotation of QuadPoints:
					
					vertexPos = float2(vertexPos.x * right + vertexPos.y * forward);

				}
				else {
					vertexPos *= scale;
				}								
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(vertexPos, 0.0, 0.0f));
				//o.pos = UnityObjectToClipPos(float4(worldPosition, 1.0) + float4(vertexPos, 0.0, 0.0));
				o.color = float4(1,1,1,1);			
				o.uv = uv;

				return o;				
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				return col;
			}
			ENDCG
		}
	}
}
