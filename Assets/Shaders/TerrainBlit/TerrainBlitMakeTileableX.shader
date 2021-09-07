Shader "TerrainBlit/TerrainBlitMakeTileableX"
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
					
			fixed4 frag (v2f i) : SV_Target
			{					
				
				float blendFraction = 0.25;  // 0-1
				
				float2 rescaleUV = i.uv;
				rescaleUV.x = i.uv.x * (1.0 - blendFraction) + blendFraction;
				float4 rescaleColor = tex2D(_MainTex, rescaleUV);

				float2 blendUV = float2(frac(i.uv.x - (1.0 - blendFraction)), i.uv.y);
				float blendAmount = saturate(i.uv.x - (1.0 - blendFraction)) * (1.0 / blendFraction);
				float4 blendColor = tex2D(_MainTex, blendUV);
				
				float4 finalColor = lerp(rescaleColor, blendColor, blendAmount);
				return finalColor;
			}

			ENDCG
		}
	}
}
