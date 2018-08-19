Shader "Critter/AgentHoverHighlightShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		
	}
	SubShader
	{		
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;			
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;

			StructuredBuffer<float4> agentHoverHighlightData;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			uniform int _HoverAgentIndex;
			uniform float _IsHover;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float4 screenUV : TEXCOORD1;
				float2 altitudeUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 color : COLOR;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				float4 data = agentHoverHighlightData[_HoverAgentIndex];
				uint agentIndex = _HoverAgentIndex; //data.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				float3 worldPosition = critterSimData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id] * 6;

				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition + quadPoint, 1.0)); // *** Revisit to better understand!!!! ***
			
				o.pos = pos;
				o.uv = quadVerticesCBuffer[id] + 0.5; // full texture

				o.color = float4(_IsHover, 1, 1, 1);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 finalCol = float4(i.color.rgb, 1.0);
				
				return finalCol;

			}
		ENDCG
		}
	}
}
