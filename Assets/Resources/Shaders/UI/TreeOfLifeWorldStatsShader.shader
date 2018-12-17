Shader "UI/TreeOfLifeWorldStatsShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		//LOD 100
		//Tags{ "RenderType" = "Transparent" }
		//ZWrite Off
		//Cull Off
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			//#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			StructuredBuffer<float3> quadVerticesCBuffer;
			// Change to 2x texture2D's ((1)time series data + (2)key) for data input?
			StructuredBuffer<float> treeOfLifeWorldStatsValuesCBuffer;
						
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D _MainTex;

			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadData = quadVerticesCBuffer[id];
				o.uv = quadData.xy + 0.5;
				
				// calculate positions in computeShader?				
				float ownScore = treeOfLifeWorldStatsValuesCBuffer[inst];
				float nextScore = treeOfLifeWorldStatsValuesCBuffer[min(inst + 1, 63)];

				float instFloat = (float)inst;

				float2 ownSubCoords = float2(instFloat / 64.0, ownScore);
				float2 nextSubCoords = float2(saturate((instFloat + 1.0) / 64.0), nextScore);

				float2 thisToNextVec = nextSubCoords - ownSubCoords;
				
				float2 forward = normalize(thisToNextVec);
				float2 right = float2(forward.y, -forward.x);

				float2 billboardVertexOffset = right * quadData.x * 0.01; // + // + forward * quadData.y

				float lerpVal = o.uv.y;
				
				float2 vertexCoord = lerp(ownSubCoords, nextSubCoords, lerpVal);
				//vertexCoord.y = treeOfLifeWorldStatsValuesCBuffer[0];
				vertexCoord += billboardVertexOffset;
				//vertexCoord.y += (sin(_Time.y * 4.31 + ownSubCoords.x * 10.67) * 0.10725) * instFloat / 64.0;
				
				float3 worldPosition = float3(vertexCoord, 0);								
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.color = float4(1, 0, 0, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 finalColor = float4(0.5,1,0.5,1);				
				return finalColor;
			}
			ENDCG
		}
	}
}
