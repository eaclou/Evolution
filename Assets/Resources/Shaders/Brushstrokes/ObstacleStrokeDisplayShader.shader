Shader "Unlit/ObstacleStrokeDisplayShader"
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			struct ObstacleStrokeData {				
				float2 worldPos;
				float2 scale;
				float2 velocity;
			};
			
			StructuredBuffer<ObstacleStrokeData> obstacleStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				ObstacleStrokeData strokeData = obstacleStrokesCBuffer[inst];

				float3 worldPosition = float3(strokeData.worldPos, 0.0);
				float3 quadPoint = quadVerticesCBuffer[id];
				
				float2 scale = strokeData.scale;				
				quadPoint *= float3(scale, 1.0);
								
				//o.pos = mul(UNITY_MATRIX_VP, float4(rotatedPoint, 0.0f));
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));
				
				o.color = float4(strokeData.velocity, 1, 1);
				
				float2 uv = quadVerticesCBuffer[id] + 0.5f;

				o.uv = uv;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 brushColor = tex2D(_MainTex, i.uv);

				float4 finalColor = float4(i.color) * brushColor;
				//finalColor.a = brushColor.a;
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
