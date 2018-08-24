Shader "HUD/InspectEnergyShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("_Tint", Color) = (1,1,1,1)
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Tint;
			uniform float _FillPercentage;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float distToOrigin = length((i.uv - 0.5) * 2);
				float alpha = 1.0 - saturate((distToOrigin - 0.95) * 20);

				float fillMask = saturate(_FillPercentage - i.uv.y) * 20;

				float4 emptyColor = float4(0.5, 0.5, 0.5, alpha * (saturate(distToOrigin)));
				float4 liquidColor = float4(_Tint.rgb, alpha);

				alpha *= (saturate(distToOrigin));

				fixed4 col = lerp(emptyColor, liquidColor, saturate(fillMask));
				col.rgb *= ((i.uv.y * 0.6) + 0.4) * (1.0 - saturate(distToOrigin) * 0.36);
				
				return col;



				/*float distToOrigin = length((i.uv - 0.5) * 2);
				float alpha = 1.0 - saturate((distToOrigin - 0.95) * 25);

				float fillMask = saturate(_FillPercentage - i.uv.y) * 10;

				float3 emptyColor = float3(0.4,0.4,0.4); 
				// sample the texture
				fixed4 col = float4(alpha,fillMask,0,alpha); //tex2D(_MainTex, i.uv);
				col.rgb = lerp(emptyColor, _Tint.rgb, fillMask);
				
				return col;*/
			}
			ENDCG
		}
	}
}
