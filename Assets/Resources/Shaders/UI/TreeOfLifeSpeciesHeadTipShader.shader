Shader "UI/TreeOfLifeSpeciesHeadTipShader"
{
	Properties
	{
		_KeyTex ("_KeyTex", 2D) = "white" {}
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
			StructuredBuffer<float3> treeOfLifeSpeciesDataHeadPosCBuffer;
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

			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float speciesID = inst;
				
				float3 quadData = quadVerticesCBuffer[id];
				o.uv = quadData.xy + 0.5;

				TreeOfLifeSpeciesKeyData keyData = treeOfLifeSpeciesDataKeyCBuffer[(int)speciesID];
				float3 pos = treeOfLifeSpeciesDataHeadPosCBuffer[inst];

				float width = 0.02 * keyData.isOn * keyData.isExtinct;
				
				float3 worldPosition = float3(pos + quadData * 0.025 * (1.0 + keyData.isSelected * 0.5));								
				worldPosition *= _IsOn;
				worldPosition.z -= 2;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.color = float4(keyData.hue * (0.5 + keyData.isSelected * 1.0), 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 finalColor = float4(i.color.rgb,1);				
				return finalColor;
			}
			ENDCG
		}
	}
}
