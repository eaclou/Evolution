Shader "UI/TreeOfLifeStemSegmentsShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"

			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<TreeOfLifeNodeColliderData> treeOfLifeNodeColliderDataCBuffer;
			StructuredBuffer<TreeOfLifeLeafNodeData> treeOfLifeLeafNodeDataCBuffer;
			StructuredBuffer<TreeOfLifeStemSegmentData> treeOfLifeStemSegmentDataCBuffer;

			uniform float3 topLeftCornerWorldPos;
			uniform float3 camRightDir;
			uniform float3 camUpDir;

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

				TreeOfLifeStemSegmentData segmentData = treeOfLifeStemSegmentDataCBuffer[inst];

				TreeOfLifeNodeColliderData fromNodeData = treeOfLifeNodeColliderDataCBuffer[segmentData.fromID];
				TreeOfLifeNodeColliderData toNodeData = treeOfLifeNodeColliderDataCBuffer[segmentData.toID];

				TreeOfLifeLeafNodeData speciesData = treeOfLifeLeafNodeDataCBuffer[segmentData.speciesID];

				float3 debugOffset = float3(-0.05,(float)segmentData.speciesID * -0.01,0) * (float)inst;
				float3 startPos = fromNodeData.localPos;
				float3 endPos = toNodeData.localPos;

				float2 forwardDir = normalize(endPos.xy - startPos.xy);
				float3 rightDir = float3(forwardDir.y, -forwardDir.x, 0.0);

				float3 worldPosition = lerp(startPos, endPos, (quadVerticesCBuffer[id].y + 0.5));
				worldPosition += rightDir * quadVerticesCBuffer[id].x;
				//float3 worldPosition = float3(rightDir * quadVerticesCBuffer[id].x * 0.1 + forwardDir * (quadVerticesCBuffer[id].y + 0.5), 0.0);
				//float activeMask = leafData.isAlive;
				//float3 worldPosition = nodeData.localPos + quadVerticesCBuffer[id] * activeMask * 0.25;
				
				o.uv = quadVerticesCBuffer[id].xy + 0.5;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0))); //float4(quadVerticesCBuffer[id], 1);
				o.color = float4(speciesData.primaryHue, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//return col;
			}
			ENDCG
		}
	}
}
