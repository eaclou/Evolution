Shader "Unlit/HitPointsDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{ 
			"RenderType"="Opaque" 
		}
		
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
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 hitPointsColor = tex2D(_MainTex, float2(0.125, 0.33));
				float hitPointsPercentage = hitPointsColor.r;

				fixed4 displayColor = float4(0,0,0,1); //tex2D(_MainTex, i.uv);
				if(i.uv.x < hitPointsPercentage) {
					displayColor.rgb = lerp(float3(1,0.35,0.25), float3(0.3,1,0.4), hitPointsPercentage);
				}

				displayColor.rgb *= 0.6;
				return displayColor;
			}
			ENDCG
		}
	}
}
