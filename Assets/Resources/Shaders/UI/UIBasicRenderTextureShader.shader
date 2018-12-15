Shader "UI/UIBasicRenderTextureShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintPri ("_TintPri", Color) = (1,1,1,1)
		_TintSec ("_TintSec", Color) = (1,1,1,1)
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
			float4 _TintPri;
			float4 _TintSec;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.a = 1;
				//col.rgb = lerp(_TintPri.rgb, _TintSec.rgb, i.uv.y);
				
				return col;
			}
			ENDCG
		}
	}
}
