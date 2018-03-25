Shader "Unlit/FoodDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 foodRValue = tex2D(_MainTex, float2(0.375, 0.33));
				float4 foodGValue = tex2D(_MainTex, float2(0.625, 0.33));
				float4 foodBValue = tex2D(_MainTex, float2(0.875, 0.33));
				
				float foodTotal = foodRValue.x + foodGValue.x + foodBValue.x;
				float foodAvgPercentage = foodTotal / 2.9;

				float foodRPercentage = (foodRValue.x / foodTotal); //saturate(foodRColor.x);
				float foodGPercentage = (foodGValue.x / foodTotal);
				float foodBPercentage = (foodBValue.x / foodTotal);

				float3 foodRColor = float3(1,0,0);
				float3 foodGColor = float3(0,1,0);
				float3 foodBColor = float3(0,0,1);

				fixed4 displayColor = float4(0,0,0,1); // BG black

				if(i.uv.x < foodAvgPercentage) {
					float3 healthColor = lerp(float3(0.8,0.1,0.05), float3(0.25,1,0.36), foodAvgPercentage);
					displayColor.rgb = lerp(foodRColor * foodRPercentage + foodGColor * foodGPercentage + foodBColor * foodBPercentage, healthColor, 0.5);
				}
				return displayColor;
			}
			ENDCG
		}
	}
}
