Shader "UI/NewInspectAgentCommsShader"
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
			uniform float _Value;
			
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
				float fillAreaMask = 1.0 - saturate((distToOrigin - _Value) * 25);
				float alpha = 1.0 - saturate((distToOrigin - 0.975) * 25);
				fixed4 col = fixed4(0,0,0,alpha);	
				col.rgb = lerp(col.rgb, _Tint.rgb, fillAreaMask);
				//col.rgb *= ((i.uv.y * 0.6) + 0.4) * (1.0 - saturate(distToOrigin) * 0.36);
				return col;
			}
			ENDCG
		}
	}
}
