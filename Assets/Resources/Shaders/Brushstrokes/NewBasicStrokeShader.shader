Shader "Unlit/NewBasicStrokeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0		
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
		
			struct BasicStrokeData {				
				float2 worldPos;
				float2 localDir;
				float2 scale;
				float4 color;
			};
			
			StructuredBuffer<BasicStrokeData> basicStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD1;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD2;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				BasicStrokeData strokeData = basicStrokesCBuffer[inst];

				float3 worldPosition = float3(strokeData.worldPos, 0.0);
				float3 quadPoint = quadVerticesCBuffer[id];
				
				float2 scale = strokeData.scale;				
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				float2 forward0 = strokeData.localDir;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float3 rotatedPoint0 = float3(quadPoint.x * right0 + quadPoint.y * forward0,
											 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint0, 0.0f));
				
				o.color = strokeData.color;
			
				float2 uv = quadVerticesCBuffer[id] + 0.5f;

				o.uv = uv;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				float4 finalColor = float4(1,1,1,1); //i.color;				
				
				return finalColor;
				
			}
			ENDCG
		}
	}
}
