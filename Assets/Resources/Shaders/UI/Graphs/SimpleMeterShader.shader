﻿Shader "UI/SimpleMeterShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("_Tint", Color) = (1,1,1,1)
		_FillPercentage ("_FillPercentage", float) = 0
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
				o.uv = (v.uv - 0.2) * 1.5; // TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float mask = saturate(saturate(_FillPercentage - i.uv.y) * 10000);

				fixed4 col = lerp(float4(0,0,0,0.33), _Tint, mask); //tex2D(_MainTex, i.uv);
				
				//return float4(i.uv.x, i.uv.x, i.uv.x, 1);
				//col.a = mask;
				//return _Tint;
				return col;
			}
			ENDCG
		}
	}
}
