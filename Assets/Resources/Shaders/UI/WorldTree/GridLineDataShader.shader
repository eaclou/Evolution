﻿Shader "UI/WorldTree/GridLineDataShader"
{
	Properties
	{
		//_DataTex ("_DataTex", 2D) = "black" {}
		//_KeyTex ("_KeyTex", 2D) = "black" {}
		//_BrushTex ("_BrushTex", 2D) = "white" {}
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
		Tags{ "RenderType" = "Transparent" }
		//ZWrite Off
		//Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			//#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"
			//#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct GridLineData {
				float3 worldPos;
				float4 color;	
				float scale;
			};
			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<GridLineData> gridLineDataCBuffer;
						
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			//sampler2D _DataTex; // needed?
			//sampler2D _KeyTex;
			//sampler2D _BrushTex;

			uniform int _SelectedWorldStatsID;
			uniform int _SelectedSpeciesID;
			uniform int _SelectedCandidateID;
			uniform float _IsOn;

			uniform float _GraphBoundsMinX;
			uniform float _GraphBoundsMaxX;
			uniform float _GraphBoundsMinY;
			uniform float _GraphBoundsMaxY;

			uniform float _GraphBufferLeft;
			uniform float _GraphBufferRight;
			uniform float _GraphBufferBottom;
			uniform float _GraphBufferTop;
			uniform float _GraphClockSize;

			uniform float _MouseCoordX;
			uniform float _MouseCoordY;
			uniform float _MouseOn;

			float3 ScaleData(float3 inPos) {
				float3 outPos = inPos;
				outPos.x = (outPos.x - _GraphBoundsMinX) / (_GraphBoundsMaxX - _GraphBoundsMinX);
				outPos.y = (outPos.y - _GraphBoundsMinY) / (_GraphBoundsMaxY - _GraphBoundsMinY);
				//should be [0-1]range now??
				
				return outPos;
			}

			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadData = quadVerticesCBuffer[id];
				o.uv = quadData.xy + 0.5;
				
				GridLineData dataPrev = gridLineDataCBuffer[max(0, inst - 1)];
				GridLineData data = gridLineDataCBuffer[inst];

				float lineWidth = data.scale;
				float4 col = data.color;
				
				float3 prevToThisVec = ScaleData(data.worldPos) - ScaleData(dataPrev.worldPos);
				float3 right = normalize(float3(prevToThisVec.y, -prevToThisVec.x, 0));

				float3 quadOffset = (quadData.x * right * lineWidth) + (o.uv.y * prevToThisVec * 1.0125);
				
				float3 worldPosition = ScaleData(dataPrev.worldPos) + quadOffset;
				float graphWidth = 1.0 - _GraphBufferLeft - _GraphBufferRight;
				float graphHeight = 1.0 - _GraphClockSize;
				worldPosition.x = worldPosition.x * graphWidth + _GraphBufferLeft; //(outPos.x + _GraphBufferLeft) / (1 - _GraphBufferRight + _GraphBufferLeft);
				//y = y * displayHeight + marginBottom;
				worldPosition.y = worldPosition.y * graphHeight;// +_GraphClockSize; //(outPos.y - _GraphBufferBottom) / (_GraphBufferTop + _GraphClockSize - _GraphBufferBottom);

				//worldPosition = float3(0.5, 0.5, 0) + quadData * 0.1;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.color = col; // tex2Dlod(_KeyTex, float4(0,((float)_SelectedWorldStatsID + 0.5) / 32.0,0,0));
				

				//float distToMouse = 1.0 - saturate(abs(vertexCoord.x - _MouseCoordX) * 15);
				//o.color.xyz = lerp(o.color.xyz, float3(1, 1, 1), _MouseOn * distToMouse * 1);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;

				//old:
				//float fade = (1.0 - i.uv.x);
				//float4 finalColor = float4(i.color.rgb, fade * fade * 1);					
				//return finalColor;
			}
			ENDCG
		}
	}
}
