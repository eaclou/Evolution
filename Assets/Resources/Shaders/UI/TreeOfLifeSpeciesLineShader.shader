Shader "UI/TreeOfLifeSpeciesLineShader"
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
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

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
			uniform int _DraggingID;
			uniform int _CurDeepestGraphDepth;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 colorPri : COLOR;
				float4 colorSec : TEXCOORD1;
				float4 extraData : TEXCOORD2;
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

				float3 rootPivotWorldPos = _TopLeftCornerWorldPos.xyz + (_CamRightDir.xyz - _CamUpDir.xyz * 0.5) * _CamScale;

				float v = (treeOfLifeStemSegmentVerticesCBuffer[id].y + 0.5);
				float startDepth = (float)fromSpeciesData.graphDepth;
				float endDepth = (float)toSpeciesData.graphDepth;
				float normalizedDepth = (lerp(startDepth, endDepth, v) / (float)_CurDeepestGraphDepth);

				// LOCAL SPACE CALCULATIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
				float3 startPos = fromNodeData.localPos;
				float3 endPos = toNodeData.localPos;

				float3 forwardStemDir = float3(normalize(endPos.xy - startPos.xy), 0.0);
				float3 rightStemDir = float3(forwardStemDir.y, -forwardStemDir.x, 0.0);
								
				float wobbleMask = saturate(1.0 - (abs(0.5 - v) * 2.0));
				float hoverMask = saturate(1.0 - abs((float)_HoverID - (float)speciesData.speciesID));  // speciesData.isHover;
				float selectedMask = saturate(1.0 - saturate(abs((float)_SelectedID - (float)speciesData.speciesID)));  // speciesData.isSelected;	
				float3 localSpaceAnchorPos = lerp(startPos, endPos, v * 1.0);

				float4 noiseSampleStaticWrinkle = Value3D(localSpaceAnchorPos + (float)speciesData.speciesID * 10, 2.23);
				float4 noiseSampleWobble = Value3D(float3(1,1,1) * _Time.y * 0.057 + (float)speciesData.speciesID * -11, 5.723794);
				float noiseMask = 1.0 - saturate((abs(0.5 - saturate((lerp(startDepth, endDepth, v) / (float)speciesData.graphDepth))) * 2.0));
				float wrinkleMask = speciesData.decayPercentage * saturate((lerp(startDepth, endDepth, v) / (float)speciesData.graphDepth));
				localSpaceAnchorPos.yz += noiseSampleStaticWrinkle.zw * wrinkleMask * 0.33;
				
				float decayMask = (1.0 - speciesData.decayPercentage);
				float3 anchorPosNoiseOffset = (_CamRightDir.xyz * noiseSampleWobble.y + _CamUpDir.xyz * noiseSampleWobble.z) * 0.05 * noiseMask * (decayMask);
								
				float bendRadius = 5;
				float bendAngle = localSpaceAnchorPos.x; // normalizedDepth;
				float3 localPosBendCircleOrigin = float3(0, -bendRadius, 0);
				float3 bendOffset = float3((1.0 - cos(bendAngle)), sin(bendAngle) * -0.5, 0);
				//bendOffset - localPosBendCircleOrigin
				
				localSpaceAnchorPos *= (decayMask * 0.85 + 0.15);
				//localSpaceAnchorPos += bendOffset * 0.25 * speciesData.decayPercentage * saturate(lerp(startDepth, endDepth, v));
				
				localSpaceAnchorPos += anchorPosNoiseOffset;
				// LOCAL SPACE CALCULATIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
				
				
				float3 billboardVertexOffset = treeOfLifeStemSegmentVerticesCBuffer[id];				
				float stemWidth = lerp(0.175, 0.033, lerp(startDepth, endDepth, v) / (float)speciesData.graphDepth);
				stemWidth *= speciesData.age * 0.8 + 0.2;
				stemWidth *= decayMask * 0.95 + 0.05;
				stemWidth += 0.01;
				billboardVertexOffset.x *= stemWidth;
				
				float wobbleOffset = sin(-_Time.y * 2.01 - (1.0 - (treeOfLifeStemSegmentVerticesCBuffer[id].y + 0.5)) * 6.28 + (float)speciesData.speciesID * 3.33) * wobbleMask * 0.0625;
				billboardVertexOffset.x += wobbleOffset * decayMask;
				billboardVertexOffset = _CamUpDir * billboardVertexOffset.x;
				//billboardVertexOffset = rightStemDir * billboardVertexOffset.x + _CamUpDir * billboardVertexOffset.x;
				
				//billboardVertexOffset.x *= 0.1;
				
				float3 worldPosition = rootPivotWorldPos + localSpaceAnchorPos * _CamScale + billboardVertexOffset * _CamScale;
				//worldPosition += pivot + (rightDir * (1.0 + selectedMask * 0.5) * speciesData.age * (treeOfLifeStemSegmentVerticesCBuffer[id].x * 0.025 * (float)(speciesData.graphDepth - fromSpeciesData.graphDepth) + sin(-_Time.y * 2.01 - (1.0 - (treeOfLifeStemSegmentVerticesCBuffer[id].y + 0.5)) * 6.28 + (float)speciesData.speciesID * 3.33) * wobbleMask * 0.0325 * (1.2 - speciesData.decayPercentage) + (float)speciesData.speciesID * 0.0001)) * _CamScale;
				//float3 worldPosition = float3(rightDir * quadVerticesCBuffer[id].x * 0.1 + forwardDir * (quadVerticesCBuffer[id].y + 0.5), 0.0);
				//float activeMask = leafData.isAlive;
				//float3 worldPosition = nodeData.localPos + quadVerticesCBuffer[id] * activeMask * 0.25;

				
				
				o.uv = treeOfLifeStemSegmentVerticesCBuffer[id].xy + 0.5;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0))); //float4(quadVerticesCBuffer[id], 1);
				o.colorPri = float4(lerp(fromSpeciesData.primaryHue, toSpeciesData.primaryHue, v), 1); // * (1.0 - speciesData.decayPercentage) + float3(1,1,1) * (hoverMask + selectedMask - 0.5) * 0.5
				o.colorSec = float4(lerp(fromSpeciesData.secondaryHue, toSpeciesData.secondaryHue, v), 1);
				o.extraData = float4(speciesData.decayPercentage, hoverMask, selectedMask, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 finalColor = i.colorPri;
				finalColor.rgb = lerp(finalColor.rgb, float3(0.44, 0.42, 0.39) * 0.62, (i.extraData.x * 0.9 + 0.1));
				finalColor.rgb += (i.extraData.y) * 0.35 + 0.0;
				finalColor.rgb += (i.extraData.z) * 0.75 + 0.0;
				//float uDist = 1.0 - saturate(abs(i.uv.x - 0.5) * 2.0);
				//finalColor.rgb *= uDist * 0.35 + 0.7;
				return finalColor;
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
