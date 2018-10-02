Shader "UI/TreeOfLifeLeafNodesShader"
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
			//StructuredBuffer<TreeOfLifeStemSegmentData> treeOfLifeStemSegmentDataCBuffer;

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
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				TreeOfLifeNodeColliderData nodeData = treeOfLifeNodeColliderDataCBuffer[inst];
				TreeOfLifeLeafNodeData leafData = treeOfLifeLeafNodeDataCBuffer[inst];

				float activeMask = leafData.isAlive;

				float3 worldPosition = nodeData.localPos + quadVerticesCBuffer[id] * activeMask * 0.25;
				
				//o.pos = float4(quadVerticesCBuffer[id].xy * 2, 0.0, 1.0);
				//o.pos = float4((quadVerticesCBuffer[5 - id].xy) * 2.0, 0.0, 1.0);  // Winding order opposite ?????
				o.uv = quadVerticesCBuffer[id].xy + 0.5;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0))); //float4(quadVerticesCBuffer[id], 1);
				o.color = float4(activeMask, activeMask, activeMask, 1.0);
				//o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(i.uv, i.color.b, 1); // * i.color;

				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				//return col;
			}
			ENDCG
		}
	}
}
