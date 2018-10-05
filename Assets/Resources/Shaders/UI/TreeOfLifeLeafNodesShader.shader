Shader "UI/TreeOfLifeLeafNodesShader"
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

			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<TreeOfLifeNodeColliderData> treeOfLifeNodeColliderDataCBuffer;
			StructuredBuffer<TreeOfLifeLeafNodeData> treeOfLifeLeafNodeDataCBuffer;
			//StructuredBuffer<TreeOfLifeStemSegmentData> treeOfLifeStemSegmentDataCBuffer;

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
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				TreeOfLifeNodeColliderData nodeData = treeOfLifeNodeColliderDataCBuffer[inst];
				TreeOfLifeLeafNodeData leafData = treeOfLifeLeafNodeDataCBuffer[inst];

				float activeMask = leafData.isActive;
				float extinctMask = (1.0 - leafData.isExtinct) * activeMask;
				float hoverMask = saturate((float)_HoverID - (float)leafData.speciesID);  // speciesData.isHover;
				float selectedMask = saturate(1.0 - saturate(abs((float)_SelectedID - (float)leafData.speciesID)));

				//float3 worldPosition = nodeData.localPos + quadVerticesCBuffer[id] * activeMask * 0.8 * leafData.age;
				//worldPosition += _TopLeftCornerWorldPos.xyz + float3(5, -5, 0);
				float3 pivot = _TopLeftCornerWorldPos.xyz + (_CamRightDir.xyz - _CamUpDir.xyz * 0.5) * _CamScale;
				float3 worldPosition = pivot + (nodeData.localPos + quadVerticesCBuffer[id] * 0.1 * (activeMask + extinctMask + selectedMask * 0.5) * leafData.age * (1.02 - leafData.decayPercentage)) * _CamScale; //_TopLeftCornerWorldPos.xyz + _CamRightDir.xyz + _CamUpDir.xyz + quadVerticesCBuffer[id];

				//float3 camCenterPos = _TopLeftCornerWorldPos.xyz;

				//worldPosition = lerp(worldPosition, camCenterPos + quadVerticesCBuffer[id] * 5, saturate(sin(_Time.y) * 0.5 + 0.5));
				//float hoverMask = leafData.isHover;
				//float selectedMask = leafData.isSelected;
				
				//o.pos = float4(quadVerticesCBuffer[id].xy * 2, 0.0, 1.0);
				//o.pos = float4((quadVerticesCBuffer[5 - id].xy) * 2.0, 0.0, 1.0);  // Winding order opposite ?????
				o.uv = quadVerticesCBuffer[id].xy + 0.5;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0))); //float4(quadVerticesCBuffer[id], 1);
				o.colorPri = float4(leafData.primaryHue * (1.05 - leafData.decayPercentage) + float3(1,1,1) * (hoverMask + selectedMask) * 0.5, 1.0);
				o.colorSec = float4(leafData.secondaryHue * (1.05 - leafData.decayPercentage) + float3(1,1,1) * (hoverMask + selectedMask) * 0.5, 1.0);

				//o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float uvDist = length((i.uv - 0.5) * 2);
				float radiusOneMask = saturate((1.0 - saturate(uvDist)) * 256);
				float radiusHalfMask = saturate((1.0 - saturate(uvDist * 2)) * 256);
				
				float4 finalColor = float4(i.colorPri.rgb, radiusOneMask);
				finalColor.rgb = lerp(finalColor.rgb, i.colorSec.rgb, radiusHalfMask);
				
				return finalColor;

				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				//return col;
			}
			ENDCG
		}
	}
}
