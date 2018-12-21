Shader "UI/TreeOfLifeSpeciesLineShader"
{
	Properties
	{
		_KeyTex ("_KeyTex", 2D) = "white" {}
		_BrushTex ("_BrushTex", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			//#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct TreeOfLifeSpeciesKeyData {
				int timeCreated;        
				int timeExtinct;
				float3 hue;
				float isOn;
				float isExtinct;
				float isSelected;
			};

			StructuredBuffer<float3> quadVerticesCBuffer;			
			StructuredBuffer<float3> treeOfLifeSpeciesSegmentsCBuffer;  // assume 64 segments x 32 species   modular
			StructuredBuffer<TreeOfLifeSpeciesKeyData> treeOfLifeSpeciesDataKeyCBuffer;
				
			uniform int _CurSimStep;
			uniform int _CurSimYear;

			uniform float _IsOn;

			uniform float _MouseCoordX;
			uniform float _MouseCoordY;
			uniform float _MouseOn;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D _KeyTex;
			sampler2D _BrushTex;

			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float speciesID = floor((float)inst / 64.0);
				int localSegmentID = inst % 64;
				int nextIndexCapped = speciesID * 64 + min(localSegmentID + 1, 63);

				float3 quadData = quadVerticesCBuffer[id];
				o.uv = quadData.xy + 0.5;

				// calculate positions in computeShader?				
				float ownScore = treeOfLifeSpeciesSegmentsCBuffer[inst].y;
				float nextScore = treeOfLifeSpeciesSegmentsCBuffer[nextIndexCapped].y;


				// *** REMOVE THESE!!! REPLACE
				//float4 speciesKeyUV = float4((speciesID + 0.5) / 32.0, 0.5, 0, 0);
				//float4 colorKeySample = tex2Dlod(_KeyTex, speciesKeyUV);
				// *** REMOVE THESE!!! REPLACE


				TreeOfLifeSpeciesKeyData keyData = treeOfLifeSpeciesDataKeyCBuffer[(int)speciesID];
				
				float instFloat = (float)(inst % 64);

				float xCoord = instFloat / 64.0;
				float creationTimeNormalized = saturate((float)keyData.timeCreated / (float)_CurSimStep - 0.2); //saturate((float)keyData.timeCreated); // / (float)_CurSimStep;
				float isExtant = saturate((xCoord - creationTimeNormalized) * 1000);
				float extinctionTimeNormalized = saturate((float)keyData.timeExtinct / (float)_CurSimStep);
				float isExtinct = saturate((xCoord - extinctionTimeNormalized) * 1000);
				isExtant *= (1.0 - isExtinct);

				float zOffset = -keyData.isSelected + keyData.isExtinct * 0.5; //(creationTimeNormalized - 0.2) * xCoord * -0.66;

				float2 ownSubCoords = float2(instFloat / 64.0, ownScore);
				float2 nextSubCoords = float2(saturate((instFloat + 1.0) / 64.0), nextScore);

				float2 thisToNextVec = nextSubCoords - ownSubCoords;
				
				float2 forward = normalize(thisToNextVec);
				float2 right = float2(forward.y, -forward.x);

				float baseWidthLerpVal = (1.0 - xCoord / extinctionTimeNormalized);
				float width = 0.94 * lerp(0.0085, 0.027, baseWidthLerpVal * baseWidthLerpVal);
				width = width * max(keyData.isOn * isExtant, keyData.isSelected * 1.33);
				float2 billboardVertexOffset = right * quadData.x * width + forward * quadData.y * 0.015;

				float lerpVal = o.uv.y;
				
				float2 vertexCoord = lerp(ownSubCoords, nextSubCoords, lerpVal);
				vertexCoord += billboardVertexOffset;
								
				float3 worldPosition = float3(vertexCoord, zOffset) * _IsOn;								
				
				o.uv.y = xCoord;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.color = float4(keyData.hue * keyData.isSelected, 0);
				o.color = float4(keyData.hue * (0.35 + 0.45 * (1.0 - keyData.isExtinct) + keyData.isSelected * 1.0), 0);
				float distToMouse = 1.0 - saturate(abs(xCoord - _MouseCoordX) * 15);
				o.color.xyz = lerp(o.color.xyz, float3(1, 1, 1), _MouseOn * distToMouse * 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 brushColor = tex2D(_BrushTex, i.uv);
				float shad = (1.0 - abs(i.uv.x - 0.5));
				shad = 1;//shad * shad;
				float4 finalColor = float4(i.color.rgb * brushColor.a * shad,1);				
				return finalColor;
			}
			ENDCG
		}
	}
}
