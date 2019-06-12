Shader "Terrain/TestInitTerrainColorBlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DeltaTex ("_DeltaTex", 2D) = "black" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _DeltaTex;


			fixed4 frag (v2f i) : SV_Target
			{
				//return float4(1,1,1,1);
				fixed4 col = tex2D(_MainTex, i.uv);
				float2 noiseCoords = i.uv * 128;
				float noise0 = saturate(Value2D(noiseCoords + 572, 0.0255).x);
				float noise1 = saturate(Value2D(noiseCoords + 5, 0.055).x);
				float noise2 = saturate(Value2D(noiseCoords - 33, 0.15).x);
				float noise3 = saturate(Value2D(noiseCoords + 349.7, 0.4579).x);

				float noise10 = saturate(Value2D(noiseCoords + 5.72, 0.0155).x);
				float noise11 = saturate(Value2D(noiseCoords - 5, 0.035).x);
				float noise12 = saturate(Value2D(noiseCoords + 22, 0.0915).x);
				float noise13 = saturate(Value2D(noiseCoords - 349.7, 0.6579).x);

				float initY = saturate(noise0 * 0.6 + noise1 * 0.25 + noise2 * 0.15 + noise3 * 0.15 - 0.4);
				float initZ = saturate((noise10 * 0.4 + noise11 * 0.45 + noise12 * 0.15 + noise13 * 0.15) - 0.45);
				float initW = saturate(((1.0 - noise10) * 0.3 + (1.0 - noise1) * 0.5 + (1.0 - noise12) * 0.25 + (1.0 - noise3) * 0.15) - 0.5);

				return float4(col.x, 
								initY, 
								initZ, 
								initW);


			}
			ENDCG
		}
	}
}
