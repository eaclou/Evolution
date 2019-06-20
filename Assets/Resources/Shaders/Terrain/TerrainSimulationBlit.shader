Shader "Terrain/TerrainSimulationBlit"
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

			uniform float _AddSubtractSign = 1; 

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 center = tex2D(_MainTex, i.uv);
				float s = 1 / 128.0;
				float deltaTime = 1.0;

				float isUnderwater = saturate(((1.0 - center.x) - 0.5) * 5);
				//float2 uv = float2((float2)id.xy + 0.5) / 128.0;

				// Neighbour cells
				//algaeGridRead.SampleLevel(_LinearRepeat, uvSample, 0);
				float4 top = tex2Dlod(_MainTex, float4(i.uv + float2(0, +s), 0, 0));	// F[x-1, y+1]: Bottom Left
				float4 right = tex2Dlod(_MainTex, float4(i.uv + float2(s, 0), 0, 0));	// F[x-1, y  ]: Centre Left
				float4 bottom = tex2Dlod(_MainTex, float4(i.uv + float2(0, -s), 0, 0)); // F[x-1, y-1]: Top Left
				float4 left = tex2Dlod(_MainTex, float4(i.uv + float2( -s, 0), 0, 0));	// F[x,   y-1]: Top Centre

				float2 altitudeGradient = float2(right.x - left.x, top.x - bottom.x);
				float steepness = length(altitudeGradient);

				float steepnessMask = 1.0; //saturate((steepness - 0.1) * 1);
				
				float hardness = saturate(center.w - 0.2 * 3);

				float4 col = center;

				float newAltitude = lerp(center, ((top.x + right.x + bottom.x + left.x) / 4.0), 0.05 * steepnessMask * hardness);

				col.x = newAltitude; // (isUnderwater * 0.025 + 0.01) * 0.2);

				return col;
			}
			ENDCG
		}
	}
}
