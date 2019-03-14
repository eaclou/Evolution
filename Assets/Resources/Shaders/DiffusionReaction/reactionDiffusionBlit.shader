Shader "Hidden/DiffusionReactionBlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			
			float _Resolution;
			float _DiffusionRateA;
			float _DiffusionRateB;
			float _MinFeedRate;
			float _MaxFeedRate;
			float _MinKillRate;
			float _MaxKillRate;
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{				
				float s = 1 / _Resolution;
				float deltaTime = 1;

				// Neighbour cells
				float4 bl = tex2D(_MainTex, i.uv + fixed2(-s, +s));	// F[x-1, y+1]: Bottom Left
				float4 cl = tex2D(_MainTex, i.uv + fixed2(-s,  0));	// F[x-1, y  ]: Centre Left
				float4 tl = tex2D(_MainTex, i.uv + fixed2(-s, -s)); // F[x-1, y-1]: Top Left
				float4 tc = tex2D(_MainTex, i.uv + fixed2( 0, -s));	// F[x,   y-1]: Top Centre
				float4 tr = tex2D(_MainTex, i.uv + fixed2(+s, -s));	// F[x,   y-1]: Top Right
				float4 cc = tex2D(_MainTex, i.uv + fixed2( 0,  0));	// F[x,   y  ]: Centre Centre
				float4 br = tex2D(_MainTex, i.uv + fixed2(+s, +s));	// F[x+1, y+1]: Bottom Right
				float4 bc = tex2D(_MainTex, i.uv + fixed2( 0, +s));	// F[x,   y+1]: Bottom Centre
				float4 cr = tex2D(_MainTex, i.uv + fixed2(+s,  0));	// F[x+1, y  ]: Centre Right

				//float killRate = lerp(_MinKillRate, _MaxKillRate, i.uv.x);
				//float feedRate = lerp(_MinFeedRate, _MaxFeedRate, i.uv.y);
				float killRate = lerp(_MinKillRate, _MaxKillRate, sin(_Time.y * 0.3437 - 2.5 + i.uv.x * 0.467 + i.uv.y * 5.312) * 0.5 + 0.5);
				float feedRate = lerp(_MinFeedRate, _MaxFeedRate, cos(_Time.y * 0.52 + 1.1983 + i.uv.y * 0.35 - i.uv.x * 1.197) * 0.5 + 0.5);

				float coCenter = -1;
				float coAdjacent = 0.2;
				float coDiagonal = 0.05;

				float laplaceA = cc.x * coCenter +
								 (cl.x + tc.x + cr.x + bc.x) * coAdjacent +
								 (bl.x + tl.x + tr.x + br.x) * coDiagonal;
				float laplaceB = cc.y * coCenter +
								 (cl.y + tc.y + cr.y + bc.y) * coAdjacent +
								 (bl.y + tl.y + tr.y + br.y) * coDiagonal;
				
				float abb = cc.x * cc.y * cc.y;

				float valA = cc.x + (_DiffusionRateA * laplaceA - abb + feedRate * (1 - cc.x)) * deltaTime;
				float valB = cc.y + (_DiffusionRateB * laplaceB + abb - (killRate + feedRate) * cc.y) * deltaTime;

				fixed4 col = float4(saturate(valA), saturate(valB), 0, 1); //tex2D(_MainTex, i.uv);
				// just invert the colors
				//col = 1 - col;
				return col;
			}
			ENDCG
		}
	}
}
