Shader "Custom/GPU/TestPointShaderA"
{

	SubShader
	{

		Pass
		{
			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct data {
				float3 pos;
			};

			struct constantTime {
				float time;
			};

			StructuredBuffer<data> buf_Points;
			StructuredBuffer<constantTime> buf_Time;
			//float3 _worldPos;

			struct ps_input {
				float4 pos : SV_POSITION;
				float3 color : COLOR0;
			};

			ps_input vert(uint id : SV_VertexID) {
				ps_input o;
				float3 worldPos = buf_Points[id].pos;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
				o.color = float4(10, 1, 1, 1);
				return o;
			}

			// Pixel Shader:
			float4 frag(ps_input i) : COLOR{
				float4 col = float4(i.color * (sin(buf_Time[0].time * 0.25) * 0.5 + 0.5), 1);
				
				return col;
			}


		ENDCG
		}
	}
}
