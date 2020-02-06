Shader "UI/Mutation/MutationThumbnailDecomposers"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintPri ("_TintPri", Color) = (1,1,1,1)
		_TintSec ("_TintSec", Color) = (1,1,1,1)
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

			float _DecomposerAspectRatio;
			int _PatternColumn;
			int _PatternRow;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				v.uv = v.uv / 8;
				v.uv.x += (1.0 / 8.0) * (float)_PatternColumn;
				v.uv.y += (1.0 / 8.0) * (float)_PatternRow;
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = v.uv;
				
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 texSample = tex2D(_MainTex, i.uv);
				return lerp(_TintPri, _TintSec, texSample.x);
				/*
				if(i.uv.y >= i.uv.x) {
					return _TintPri * tex2D(_MainTex, i.uv);;
				}
				else {
					return _TintSec * tex2D(_MainTex, i.uv);;
				}
				*/
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
