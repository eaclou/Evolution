Shader "UI/WorldTree/CreatureLineDataShader"
{
	Properties
	{
		//_DataTex ("_DataTex", 2D) = "black" {}
		//_KeyTex ("_KeyTex", 2D) = "black" {}
		//_BrushTex ("_BrushTex", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		//LOD 100
		//Tags{ "RenderType" = "Transparent" }
		ZWrite On
		//Cull Off
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			//#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"
			//#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct WorldTreeLineData {
				float3 worldPos;
				float4 color;
				int speciesID;
				int candidateID;
				int isAlive;
				int isSelected;	
				int isVisible;
			};
			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<WorldTreeLineData> worldTreeLineDataCBuffer;
						
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
				
				float lineWidth = 0.0182;
				
				WorldTreeLineData dataPrev = worldTreeLineDataCBuffer[max(0, inst - 1)];
				WorldTreeLineData data = worldTreeLineDataCBuffer[inst];
				
				float4 col = data.color;
				col.a = 1.0;
				if (data.isSelected) {
					lineWidth = 0.02691;
					col.rgb = lerp(col.rgb, 1, 1);
				}
				else {
					//col.rgb *= 0.8;
				}
				if (data.isAlive < 0.5) { // isOFF
					col.rgb = 0;// lerp(col.rgb, float3(1.0, 0.05, 0.05), 0.8255);
					lineWidth *= 0.37;
					//quadData *= 0;
					//worldPosition.z = 1.0; // handled on cpu side for now
				}
				if (data.isVisible < 0.5) {
					quadData *= 0;;
				}

				float3 prevToThisVec = ScaleData(data.worldPos) - ScaleData(dataPrev.worldPos);
				float3 right = normalize(float3(prevToThisVec.y, -prevToThisVec.x, 0));
				float tilt01 = abs(prevToThisVec.y);
				//float tilt01 = prevToThisVec.y * prevToThisVec.y;
				float3 quadOffset = (quadData.x * right * lineWidth * (1.0 - o.uv.y * 0.0067)) + (o.uv.y * prevToThisVec * 1);
				
				float3 worldPosition = ScaleData(dataPrev.worldPos) + quadOffset;
				
				float graphWidth = 1.0 - _GraphBufferLeft - _GraphBufferRight;
				float graphHeight = 1.0 - _GraphClockSize;
				worldPosition.x = worldPosition.x * graphWidth + _GraphBufferLeft; //(outPos.x + _GraphBufferLeft) / (1 - _GraphBufferRight + _GraphBufferLeft);
				worldPosition.y = worldPosition.y * graphHeight;// +_GraphClockSize; //(outPos.y - _GraphBufferBottom) / (_GraphBufferTop + _GraphClockSize - _GraphBufferBottom);
				//worldPosition.z = -1 * data.isSelected;
				float4 fogColor = float4(0, 0, 0, 1);
				col = lerp(col, fogColor, data.color.a);
				if (worldPosition.y > 0.8) {
					col.xyz *= 0.284;
				}
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				
				o.color = col;// float4(0, data.color.a, 0, 1);// col;// *tilt01; // tex2Dlod(_KeyTex, float4(0,((float)_SelectedWorldStatsID + 0.5) / 32.0,0,0));
				

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
