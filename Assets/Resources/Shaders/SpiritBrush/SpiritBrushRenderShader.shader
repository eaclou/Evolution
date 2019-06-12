Shader "SpiritBrush/SpiritBrushRenderShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform float _Scale = 1.0;
			uniform float _Strength = 1.0;
			uniform float4 _Position;

			StructuredBuffer<float3> quadVerticesCBuffer;
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				float3 worldPosition = float3(_Position.xy, 0.0);
				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy + 0.5f;

				float2 scale = float2(_Scale, _Scale); //strokeData.scale;				
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				//float2 forward0 = strokeData.localDir;
				//float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				//float3 rotatedPoint0 = float3(quadPoint.x * right0 + quadPoint.y * forward0,
				//							 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.color = float4(1,1,1,1);				
				o.uv = uv;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return float4(1,1,1,1);
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= 0.025 * _Strength;
				col.rgb = saturate(col.rgb);
				// apply fog
				return col;
			}
			ENDCG
		}
	}
}
