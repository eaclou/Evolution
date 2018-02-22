Shader "Unlit/FoodShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FoodAmountR ("_FoodAmountR", Float) = 1.0
		_FoodAmountG ("_FoodAmountG", Float) = 1.0
		_FoodAmountB ("_FoodAmountB", Float) = 1.0
		_Scale ("_Scale", Float) = 1.0
		_IsBeingEaten ("_IsBeingEaten", Float) = 0.0
	}
	SubShader
	{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _FoodAmountR;
			fixed _FoodAmountG;
			fixed _FoodAmountB;
			fixed _Scale;
			fixed _IsBeingEaten;

			float rand(float2 co){
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.worldPos = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 rTypeColor = float3(0.9, 0.8, 0.1);
				float3 gTypeColor = float3(0.35, 1, 0.35);
				float3 bTypeColor = float3(0.1, 0.8, 0.9);

				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				//float2 coords = i.uv * 2.0 - 1.0;

				float distToCenter = length(i.worldPos.xy);

				float2 coords = i.worldPos.xy * _Scale;
				
				float cellSize = 12;
				coords = floor(coords * cellSize) / cellSize;

				float randVal = lerp(0, rand(coords), (distToCenter)) + 0.7;//rand(coords) - (saturate((1.0 - distToCenter))) * 0.25;
				float randCol = floor(rand(coords.yx) * 3);
				

				float4 outColor = float4(0.0, 0.0, 0.0, 1.0);

				if(randCol == 0) {
					if(_FoodAmountR > randVal) {
						outColor.rgb += rTypeColor;
						outColor.a = 1;
					}					
				}
				if(randCol == 1) {
					if(_FoodAmountG > randVal) {
						outColor.rgb += gTypeColor;
						outColor.a = 1;
					}
				}
				if(randCol == 2) {
					if(_FoodAmountB > randVal) {
						outColor.rgb += bTypeColor;
						outColor.a = 1;
					}
				}
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				outColor.rgb = float3(0,0.5,0) * 3;
				outColor.a *= 1;
				float alphaCutoffValue = 0.1;
				clip(outColor.a - alphaCutoffValue);
				//float dimmer = 0.45 + _IsBeingEaten * 0.075;
				//outColor.rgb *= dimmer * 0.1;

				return outColor;
			}
			ENDCG
		}
	}
}
