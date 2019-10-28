﻿Shader "Unlit/TerrainObstaclesHeightMaskShader"
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
				float4 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.worldPos = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = float4(0,0,0,1);
				//tex2D(_MainTex, i.uv);

				float4 altitudeSample = tex2D(_MainTex, i.uv);
				float isAboveWater = saturate(10000 * (altitudeSample - 0.485));
				//float aboveSeaLevel = saturate(-sign(altitude * 10) * 0.5 + 0.5);

				float4 finalColor = float4(0, 0, isAboveWater, 1);

				return finalColor;
			}
			ENDCG
		}
	}
}
