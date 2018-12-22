Shader "UI/TreeOfLifeEventsLineShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
		Tags{ "RenderType" = "Opaque" }
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
			
			struct TreeOfLifeEventLineData {
				int timeStepActivated;
				float eventCategory;  // minor major extreme 0, 0.5, 1.0
				float isActive;
			};

			StructuredBuffer<float3> quadVerticesCBuffer;
			//StructuredBuffer<float3> treeOfLifeEventLineDataCBuffer;
			StructuredBuffer<TreeOfLifeEventLineData> treeOfLifeEventLineDataCBuffer;

			uniform float _CurSimStep;

			//uniform float _GraphCoordStatsStart;
			//uniform float _GraphCoordStatsRange;
			//uniform float _GraphCoordSpeciesStart;
			//uniform float _GraphCoordSpeciesRange;
			uniform float _GraphCoordEventsStart;  // needed?
			uniform float _GraphCoordEventsRange;

			//uniform float _StatsPanelOn;
			//uniform float _SpeciesPanelOn;
			//uniform float _EventsPanelOn;

			uniform int _ClosestEventIndex;

			uniform float _IsOn;

			uniform float _MouseCoordX;
			uniform float _MouseCoordY;
			uniform float _MouseOn;

			//StructuredBuffer<float3> treeOfLifeStemSegmentVerticesCBuffer; //quadVerticesCBuffer;
			//StructuredBuffer<TreeOfLifeNodeColliderData> treeOfLifeNodeColliderDataCBuffer;
			//StructuredBuffer<TreeOfLifeLeafNodeData> treeOfLifeLeafNodeDataCBuffer;
			//StructuredBuffer<TreeOfLifeStemSegmentData> treeOfLifeStemSegmentDataCBuffer;

			//uniform float4 _TopLeftCornerWorldPos;
			//uniform float4 _CamRightDir;
			//uniform float4 _CamUpDir;

			//uniform float _CamScale;
			
			//uniform int _HoverID;
			//uniform int _SelectedID;
			//uniform int _DraggingID;
			//uniform int _CurDeepestGraphDepth;

			
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
				TreeOfLifeEventLineData eventData = treeOfLifeEventLineDataCBuffer[inst];	
				float highlightedMask = _MouseOn * (1.0 - saturate(abs((float)(inst - _ClosestEventIndex))));
				quadData.x *= (1.0 + highlightedMask);
				quadData.y *= (1.0 + highlightedMask * 0.67);

				quadData.x *= 0.0125; // bar width
				quadData.y += 0.5;
				
				float barHeight = (eventData.eventCategory * 0.6 + 0.6);
				//quadData.y += 1.0 - barHeight;
				quadData.y *= barHeight; // * _GraphCoordEventsRange;
				quadData.y += (1.0 - barHeight);
				//quadData.y = 1.0 - quadData.y;		
				quadData.y = quadData.y * _GraphCoordEventsRange + (1.0 - _GraphCoordEventsRange);	 // INVERT

				
				
				
				o.uv = quadVerticesCBuffer[id].xy + 0.5;
				
				float3 worldPosition = float3((float)eventData.timeStepActivated / (float)_CurSimStep, 0, 0) + quadData * eventData.isActive;
				//worldPosition.y += _GraphCoordEventsStart;				
				worldPosition *= _IsOn;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));

				float3 hueMinor = float3(0.55, 1, 0.6) * 0.8;
				float3 hueMajor = float3(0.25, 0.55, 1) * 0.8;
				float3 hueExtreme = float3(1, 0.55, 0.65) * 0.675;

				float mag = eventData.eventCategory; //saturate(eventData.eventCategory - 0.5) * 2;
				float3 hue = lerp(hueMinor, hueMajor, saturate(mag * 2));
				hue = lerp(hue, hueExtreme, saturate(mag - 0.5) * 2);
				hue = lerp(hue, float3(1,1,1), highlightedMask * 0.75 * (1.0 - (quadVerticesCBuffer[id].y + 0.5)));
				o.color = float4(hue, 0);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 finalColor = float4(i.color.rgb,1);
				//finalColor.rgb *= i.color.x;
				return finalColor;
			}
			ENDCG
		}
	}
}
