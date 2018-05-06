Shader "Unlit/UberChainsRenderShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SourceColorTex ("_SourceColorTex", 2D) = "black" {}
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
				float2 fluidTexUV : TEXCOORD3;
			};

			struct LinkData {
				float2 worldPos;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SourceColorTex; 			

			//StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			StructuredBuffer<LinkData> chainsReadCBuffer;
			StructuredBuffer<float3> verticesCBuffer;

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				LinkData linkData = chainsReadCBuffer[inst];

				uint trailPointIndex = inst;
				uint localTrailIndex = trailPointIndex % 32;
				uint headIndex = floor(inst / 32) * 32;

				float3 worldPosition = float3(linkData.worldPos, -0.5);

				o.fluidTexUV = (chainsReadCBuffer[headIndex].worldPos + 70) / 140;

				o.segmentUV = verticesCBuffer[id].xy + 0.5;
				
				float2 vertexPos = verticesCBuffer[id].xy;
				float2 uv = vertexPos + 0.5;
				//uv.y = 1 - uv.y;
				vertexPos.y += 0.5;

				float localTrailPercentage = (float)localTrailIndex / 32.0;
				uv.y = uv.y / 64.0 + localTrailPercentage; // *** ??????

				float random1 = rand(float2(inst, inst));
				float randomWidth = lerp(0.5, 1.6, random1) * 1;	
				
				float2 scale = float2(0.22 * randomWidth, 1);
				//scale.x += (1 - localTrailPercentage) * 0.2;  // Taper
				//scale.x *= (1 + (agentData.secondaryHue.g - 0.5) * 2);
				//scale *= 1.4;

				if(localTrailIndex != 0) {
					float2 parentPos = chainsReadCBuffer[inst - 1].worldPos;
					scale.y = clamp(length(parentPos - linkData.worldPos) * 1, 0.0001, 1);
					vertexPos *= scale;

					float2 forward = normalize(parentPos - linkData.worldPos);
					float2 right = float2(forward.y, -forward.x);
					// With final facing Vectors, find rotation of QuadPoints:
					
					vertexPos = float2(vertexPos.x * right + vertexPos.y * forward);

				}
				else {
					vertexPos *= scale * 0;
				}	
				
				float alpha = 1;
				alpha *= (1 - localTrailPercentage) * 0.75 + 0.25;
				//alpha = alpha * agentData.maturity;
				//alpha = alpha * (1.0 - agentData.decay);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(vertexPos, 0.0, 0.0f));
				//float3 tailHue = lerp(agentData.secondaryHue, agentData.primaryHue, localTrailPercentage);
				//tailHue = lerp(float3(0.5, 0.5, 0.5), tailHue, saturate(agentData.maturity * 4 - 3));
				//tailHue = lerp(float3(0.5, 0.5, 0.5), tailHue, saturate(agentData.foodAmount * 4));
				o.color = float4(1,1,1,alpha);			
				o.uv = uv;

				return o;				
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float4 fluidDensityColor = tex2D(_SourceColorTex, i.fluidTexUV);  //  Color of brushtroke source		
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				//float4 quadCol = tex2D(_MainTex, i.segmentUV);
				//col.a *= quadCol.a;
				
				float4 finalCol = lerp(col, fluidDensityColor, 1);				
				finalCol.a = col.a * 1;
				finalCol.rgb *= 1.21;
				
				//finalCol = float4(1,1,1,1);
				return finalCol;
			}
			ENDCG
		}
	}
}
