Shader "UI/TreeOfLifeCursorLineShader"
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
		//ZWrite Off
		//Cull Off
		//Blend One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
			
			struct TreeOfLifeEventLineData {
				int timeStepActivated;
				float eventCategory;  // minor major extreme 0, 0.5, 1.0
				float isActive;
			};

			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<TreeOfLifeEventLineData> treeOfLifeEventLineDataCBuffer;

			uniform float _CurSimStep;

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

			sampler2D _MainTex;

			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadData = quadVerticesCBuffer[id];
				TreeOfLifeEventLineData eventData = treeOfLifeEventLineDataCBuffer[inst];				
				quadData.x *= 0.0125; // bar width
				quadData.y += 0.5;
				float barHeight = (eventData.eventCategory * 0.67 + 0.33);
				
				quadData.y *= barHeight; // * _GraphCoordEventsRange;
				quadData.y += (1.0 - barHeight);
				
				//quadData.y = quadData.y * _GraphCoordEventsRange + (1.0 - _GraphCoordEventsRange);	 // INVERT
				
				o.uv = quadVerticesCBuffer[id].xy + 0.5;
				
				//float3 worldPosition = float3((float)eventData.timeStepActivated / (float)_CurSimStep, 0, 0) + quadData * eventData.isActive;
				//worldPosition.y += _GraphCoordEventsStart;				
				//worldPosition *= _IsOn;
				float3 spriteVertexOffset = quadVerticesCBuffer[id];
				spriteVertexOffset.x *= 0.01 * _MouseOn;

				float3 worldPosition = float3(_MouseCoordX, 0.5, 0.0) + spriteVertexOffset;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
								
				o.color = float4(1,1,1,0);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				
				float falloff = 1.0 - (abs(i.uv.x - 0.5) * 2);
				falloff = falloff * falloff * falloff;
				float4 finalColor = float4(0.8, 0.9, 1.25, falloff);
				//finalColor.rgb *= i.color.x;
				return finalColor;
			}
			ENDCG
		}
	}
}
