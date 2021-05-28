Shader "UI/Clock/ClockPlanetShaderA"
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
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			uniform float _CurFrame;
			uniform float _NumRows;
			uniform float _NumColumns;
			
			float2 GetUV(float2 uvStart, float frame, float rows, float cols) {
				frame = 4;// floor(frame) % (rows * cols);
				
				float2 newUV = uvStart;
				newUV.x = newUV.x / cols;
				newUV.y = newUV.y / rows;				
				float column = (float)(frame % cols);
				float row = (float)floor((frame) / cols);
				newUV.x += column * (1.0 / cols);
				newUV.y += row * (1.0 / rows);

				return newUV;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				float2 uv0 = GetUV(v.uv, _CurFrame, _NumRows, _NumColumns);// + 0.5;				
				float2 uv1 = GetUV(v.uv, _CurFrame + 1, _NumRows, _NumColumns);// + 0.5;		
				
				o.uv0 = uv0; // full texture
				o.uv1 = uv1;
				//o.uv0 = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return float4(i.uv0, 0, 1);
				// sample the texture
				fixed4 col0 = tex2D(_MainTex, i.uv0);
				fixed4 col1 = tex2D(_MainTex, i.uv1);
				//col.rgb = lerp(_TintPri.rgb, _TintSec.rgb, i.uv.y);
				fixed4 finalCol = lerp(col0, col1, frac(_CurFrame));
				finalCol.rgb = lerp(finalCol.rgb, float3(0, 0.61, 0.95), 0.2);
				return finalCol;
			}
			ENDCG
		}
	}
}
