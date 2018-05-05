Shader "FX/RippleDistortionBlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DistortTex ("_DistortTex", 2D) = "gray" {}
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
				o.uv = v.uv; //float2(v.uv.x, 1 - v.uv.y);
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _DistortTex;

			fixed4 frag (v2f i) : SV_Target
			{
				

				float time = _Time.y;
				float2 distortSampleUV = (i.uv) * 0.095 + time * 0.01;
				
				float4 distortOffset = tex2D(_DistortTex, distortSampleUV);
				float distortStrength = 0.0035;
				
				float2 screenSampleUV = float2(i.uv.x, 1 - i.uv.y);
				screenSampleUV += (distortOffset - 0.5) * distortStrength;
				//float2 distortSampleUV = i.uv + (distortOffset - 0.5) * distortStrength;

				fixed4 col = tex2D(_MainTex, screenSampleUV);
				// just invert the colors
				//col.xyz = 1 - col.xyz;

				float4 debugCol = float4(screenSampleUV, 0, 1);
				return col;
			}
			ENDCG
		}
	}
}
