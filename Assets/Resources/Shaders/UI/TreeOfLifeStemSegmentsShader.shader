Shader "UI/TreeOfLifeStemSegmentsShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"

			StructuredBuffer<float3> treeOfLifeStemSegmentVerticesCBuffer; //quadVerticesCBuffer;
			StructuredBuffer<TreeOfLifeNodeColliderData> treeOfLifeNodeColliderDataCBuffer;
			StructuredBuffer<TreeOfLifeLeafNodeData> treeOfLifeLeafNodeDataCBuffer;
			StructuredBuffer<TreeOfLifeStemSegmentData> treeOfLifeStemSegmentDataCBuffer;

			uniform float4 _TopLeftCornerWorldPos;
			uniform float4 _CamRightDir;
			uniform float4 _CamUpDir;

			uniform float _CamScale;

			uniform int _HoverID;
			uniform int _SelectedID;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 colorPri : COLOR;
				float4 colorSec : TEXCOORD1;
			};

			sampler2D _MainTex;

			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				TreeOfLifeStemSegmentData segmentData = treeOfLifeStemSegmentDataCBuffer[inst];

				TreeOfLifeNodeColliderData fromNodeData = treeOfLifeNodeColliderDataCBuffer[segmentData.fromID];
				TreeOfLifeNodeColliderData toNodeData = treeOfLifeNodeColliderDataCBuffer[segmentData.toID];

				TreeOfLifeLeafNodeData speciesData = treeOfLifeLeafNodeDataCBuffer[segmentData.speciesID];
				TreeOfLifeLeafNodeData fromSpeciesData = treeOfLifeLeafNodeDataCBuffer[segmentData.fromID];
				TreeOfLifeLeafNodeData toSpeciesData = treeOfLifeLeafNodeDataCBuffer[segmentData.toID];

				float extinctMask = 1.0 - speciesData.isExtinct;

				float3 pivot = _TopLeftCornerWorldPos.xyz + (_CamRightDir.xyz - _CamUpDir.xyz * 0.5) * _CamScale;

				float3 debugOffset = float3(-0.05,(float)segmentData.speciesID * -0.01,0) * (float)inst;
				float3 startPos = fromNodeData.localPos;
				float3 endPos = toNodeData.localPos;

				float2 forwardDir = normalize(endPos.xy - startPos.xy);
				float3 rightDir = float3(forwardDir.y, -forwardDir.x, 0.0);

				float v = (treeOfLifeStemSegmentVerticesCBuffer[id].y + 0.5);
				float wobbleMask = saturate(1.0 - (abs(0.5 - v) * 2.0));

				float hoverMask = saturate(1.0 - abs((float)_HoverID - (float)speciesData.speciesID));  // speciesData.isHover;
				float selectedMask = saturate(1.0 - saturate(abs((float)_SelectedID - (float)speciesData.speciesID)));  // speciesData.isSelected;
				

				float3 worldPosition = lerp(startPos, endPos, v);
				worldPosition += pivot + (rightDir * (1.0 + selectedMask * 0.5) * speciesData.age * (treeOfLifeStemSegmentVerticesCBuffer[id].x * 0.025 * (float)(speciesData.graphDepth - fromSpeciesData.graphDepth) + sin(-_Time.y * 2.01 - (1.0 - (treeOfLifeStemSegmentVerticesCBuffer[id].y + 0.5)) * 6.28 + (float)speciesData.speciesID * 3.33) * wobbleMask * 0.0325 * (1.2 - speciesData.decayPercentage) + (float)speciesData.speciesID * 0.0001)) * _CamScale;
				
				//float3 worldPosition = float3(rightDir * quadVerticesCBuffer[id].x * 0.1 + forwardDir * (quadVerticesCBuffer[id].y + 0.5), 0.0);
				//float activeMask = leafData.isAlive;
				//float3 worldPosition = nodeData.localPos + quadVerticesCBuffer[id] * activeMask * 0.25;

				
				
				o.uv = treeOfLifeStemSegmentVerticesCBuffer[id].xy + 0.5;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0))); //float4(quadVerticesCBuffer[id], 1);
				o.colorPri = float4(lerp(fromSpeciesData.primaryHue, toSpeciesData.primaryHue, v) * (1.05 - speciesData.decayPercentage) + float3(1,1,1) * (hoverMask + selectedMask) * 0.5, 1);
				o.colorSec = float4(lerp(fromSpeciesData.secondaryHue, toSpeciesData.secondaryHue, v) * (1.05 - speciesData.decayPercentage) + float3(1,1,1) * (hoverMask + selectedMask) * 0.5, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.colorPri;
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
