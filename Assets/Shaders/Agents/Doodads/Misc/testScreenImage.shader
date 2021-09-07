Shader "Unlit/testScreenImage"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 screenColor = float3(0,0,0);

				float3 gridLineColor = float3(0.5, 1, 0.66);

				// gridlines:
				float gridLineThickness = 0.1;
				float distToGridLineX = abs(frac(i.uv.x * 6) - gridLineThickness);
				float distToGridLineY = abs(frac(i.uv.y * 6) - gridLineThickness);

				float linesVal = 1.0 - smoothstep(0.0, gridLineThickness, distToGridLineX) + 1.0 - smoothstep(0.0, gridLineThickness, distToGridLineY);
				screenColor += gridLineColor * linesVal * 0.5;


				// SinWave!
				float frequency = lerp(4, 64, cos(_Time.y) * 0.5 + 0.5);
				float edgeReduce = 1.0 - smoothstep(0, 0.4, abs(i.uv.x - 0.5));
				float distToSinY = abs(i.uv.y - 0.5 - sin((i.uv.x + _Time.y) * frequency) * 0.25 * edgeReduce);
				float sinLineIntensity = 1.0 - smoothstep(0, 0.12, distToSinY);

				screenColor += gridLineColor * sinLineIntensity * 1;



				float distToCenter = length(i.uv - float2(0.5, 0.5));
				float radialMask = 1.0 - smoothstep(0.36, 0.5, distToCenter);
				screenColor *= radialMask;

				return float4(screenColor, 1);
			}
			ENDCG
		}
	}
}
