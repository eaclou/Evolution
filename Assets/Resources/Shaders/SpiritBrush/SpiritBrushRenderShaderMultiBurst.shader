Shader "SpiritBrush/SpiritBrushRenderShaderMultiBurst"
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
		Blend SrcAlpha OneMinusSrcAlpha

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

			struct SpiritBrushQuadData {
				int index;
				float3 worldPos;
				float2 heading;
				float2 localScale;
				float lifespan;
				float age01;
				float4 extraVec4;
				float2 vel;
				float drag;
				float noiseStart;
				float noiseEnd;
				float noiseFreq;
				int brushType;
			};

			StructuredBuffer<SpiritBrushQuadData> _SpiritBrushQuadsRead;

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

			StructuredBuffer<float3> quadVerticesCBuffer;
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				SpiritBrushQuadData data = _SpiritBrushQuadsRead[inst];

				float3 worldPosition = data.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy + 0.5;
								
				float2 scale = float2(5, 12) * data.age01 * 2;// * 0.05 + data.age01 * 4; //strokeData.scale;				
				quadPoint *= float3(scale, 1.0);

				float fadeIn = saturate(data.age01 * 3);
				float fadeOut = saturate((1.0 - data.age01) * 3);
				float alpha = fadeIn * fadeOut;

				// Figure out final facing Vectors!!!
				float2 forward0 = data.heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float3 rotatedPoint0 = float3(quadPoint.x * right0 + quadPoint.y * forward0,
											 quadPoint.z);
				//worldPosition = float3(0,0,0);
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition + rotatedPoint0, 1.0)));				
				o.color = float4(1,1,1,alpha);				
				o.uv = uv;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = 1 * _Strength;
				col.rgb = saturate(col.rgb);

				//float timeScale = 0.2789;
				//float4 noiseTex = tex2D(_NoiseTex, frac(i.uv * 0.12 + _Time.y * 0.935 * timeScale)) * tex2D(_NoiseTex, frac(i.uv * 0.256 - _Time.y * 0.835 * timeScale));
				
				float uvDist = length(i.uv - 0.5) * 2;
				
				float mask = saturate(1.0 - uvDist);

				//float mult = lerp(mask, saturate(((noiseTex.x * noiseTex.x * 1 + 0.05) - uvDist) * 15), 0.5);
				//col.rgb *= mult;

				float2 patternUV = i.uv * (1.0 / 8.0) + float2(0.125 * _PatternColumn, 0.125 * _PatternRow);

				float4 patternSample = tex2D(_BrushPatternTex, patternUV);

				//return float4(1,1,1,);
				return float4(1,1,1, i.color.a * patternSample.x);
				
				return col;
			}
			ENDCG
		}
	}
}
