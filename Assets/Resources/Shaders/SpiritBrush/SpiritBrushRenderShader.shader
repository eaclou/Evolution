Shader "SpiritBrush/SpiritBrushRenderShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("_NoiseTex", 2D) = "white" {}
		_BrushPatternTex ("_BrushPatternTex", 2D) = "white" {}
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
			sampler2D _NoiseTex;
			sampler2D _BrushPatternTex;

			uniform float _PatternColumn;
			uniform float _PatternRow;

			uniform float _FacingDirX;
			uniform float _FacingDirY;

			uniform float _Scale = 1.0;
			uniform float _Strength = 1.0;
			uniform float4 _Position;

			uniform float _IsActive;
			uniform float _IsBrushing;
			uniform float _IsWildSpirit;

			StructuredBuffer<float3> quadVerticesCBuffer;
			//StructuredBuffer<particledata> dataCBuffer;

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				float3 worldPosition = float3(_Position.xy, 0.0);
				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy + 0.5f;

				float instMask = saturate((float)inst); // 0 or 1
				worldPosition = lerp(worldPosition, float3(_Position.zw, 0), instMask);

				float2 scale = float2(_Scale, _Scale); //strokeData.scale;		
				scale = lerp(scale, scale * 0.33, instMask);

				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				float2 forward0 = float2(_FacingDirX, _FacingDirY);
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float3 rotatedPoint0 = float3(quadPoint.x * right0 + quadPoint.y * forward0,
											 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				
				float4 color0 = float4(1,1,0,1);
				color0.x *= _IsActive;
				color0.y *= _IsActive * 0.5 + 0.5;
				color0.y *= _IsBrushing;
				color0.z = 0;

				float4 color1 = float4(0,1,1,1);
				color1.x *= 0;
				color1.y *= 1;
				color1.z = 0;
				o.color = lerp(color0, color1 * _IsWildSpirit, instMask);	
				
				o.uv = uv;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = 1 * _Strength;
				col.rgb = saturate(col.rgb * i.color.rgb);

				float timeScale = 0.3789;
				float4 noiseTex = tex2D(_NoiseTex, frac(i.uv * 0.12 + _Time.y * 0.935 * timeScale)) * tex2D(_NoiseTex, frac(i.uv * 0.256 - _Time.y * 0.835 * timeScale));
				
				float uvDist = length(i.uv - 0.5) * 2;
				
				float mask = saturate(1.0 - uvDist);

				float mult = lerp(mask, saturate(((noiseTex.x * noiseTex.x * 1 + 0.05) - uvDist) * 15), 0.5);
				col.rgb *= mult;

				//float4 wildSpiritCol = col;

				//col.x *= _IsActive;
				//col.y *= _IsActive * 0.5 + 0.5;
				//col.z = 0;


				//col.rgb *= (_IsActive * 0.5 + 0.5);
				
				//float2 patternUV = i.uv * (1.0 / 8.0) + float2(0.125 * _PatternColumn, 0.125 * _PatternRow);
				//float4 patternSample = tex2D(_BrushPatternTex, patternUV);

				//return float4(patternSample.rgb * mask, 1);
				//return float4(1,1,1,1);
				return col;
			}
			ENDCG
		}
	}
}
