﻿Shader "Unlit/AgentShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_IsPlayer ("_IsPlayer", Float) = 0.0
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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float _IsPlayer;

			float rand(float2 co){
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + float4(0,0,-0.1,0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 texSample = tex2D(_MainTex, i.uv);

				float4 bgColor = float4(0.2, 0.85, 0.75, 1.0);				
				float4 damageColor = float4(0.6, 0.4, 0.3,1);
				float damageRadius = 0.8;
				float outCommRadius = 0.23;
				float3 negColor = float3(1, 0.6, 0.0) * 1.05;
				float3 posColor = float3(0.25, 0.6, 1) * 1.25;
				if(_IsPlayer > 0.5) {
					bgColor = float4(0.9, 1.25, 0.2, 1.0);
					damageColor = float4(0.8, 0.3, 0.2,1);
					outCommRadius *= 0.75;
				}
				
				
				float2 coords = float2((i.uv - 0.5) * 2.0);
				float circle = ceil(1.0 - saturate(length(coords)));
				float distToOrigin = length(coords);

				float4 outCol = float4(bgColor.rgb, circle);				

				float foodRadius = 0.8;
				float foodEnergy = (tex2D(_MainTex, float2(0.375, 0.333)).r + tex2D(_MainTex, float2(0.625, 0.333)).r + tex2D(_MainTex, float2(0.875, 0.333)).r) / 3.0;
								
				float hitPoints = tex2D(_MainTex, float2(0.125, 0.333)).r;
				
				float healthValue = saturate(min(foodEnergy, hitPoints));
				if(healthValue < 0.50) {
					outCol.rgb = lerp(damageColor, bgColor, healthValue * 2);
					//outCommRadius *= healthValue * 3;
					//negColor *= 0.5;
					//posColor *= 0.5;
				}
				//outCol.rgb = lerp(outCol.rgb, float3(0.8, 0.75, 0.9), 0.45);
							
				float2 outCommOrigin0 = float2(0.25, 0.25);
				float outCommDist0 = length(coords - outCommOrigin0);
				if(outCommDist0 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.125, 0.667));  // 0-1
					float val = floor(sampleCol.r * 3) - 1; // ??? -1,0,1 ???
					//outCol.rgb = lerp(negColor, posColor, val * 0.5 + 0.5) * abs(val);
				}
				float2 outCommOrigin1 = float2(0.25, -0.25);
				float outCommDist1 = length(coords - outCommOrigin1);
				if(outCommDist1 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.375, 0.667));
					float val = floor(sampleCol.r * 3) - 1; // ??? -1,0,1 ???
					//outCol.rgb = lerp(negColor, posColor, val * 0.5 + 0.5) * abs(val);
					//outCol.rgb = lerp(negColor, posColor, sampleCol.r) * abs(sampleCol.r * 2.0 - 1.0);
				}
				float2 outCommOrigin2 = float2(-0.25, 0.25);
				float outCommDist2 = length(coords - outCommOrigin2);
				if(outCommDist2 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.625, 0.667));
					float val = floor(sampleCol.r * 3) - 1; // ??? -1,0,1 ???
					//outCol.rgb = lerp(negColor, posColor, val * 0.5 + 0.5) * abs(val);
					//outCol.rgb = lerp(negColor, posColor, sampleCol.r) * abs(sampleCol.r * 2.0 - 1.0);
				}
				float2 outCommOrigin3 = float2(-0.25, -0.25);
				float outCommDist3 = length(coords - outCommOrigin3);
				if(outCommDist3 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.875, 0.667));
					float val = floor(sampleCol.r * 3) - 1; // ??? -1,0,1 ???
					//outCol.rgb = lerp(negColor, posColor, val * 0.5 + 0.5) * abs(val);
					//outCol.rgb = lerp(negColor, posColor, sampleCol.r) * abs(sampleCol.r * 2.0 - 1.0);
				}

				float cellSize = 4;
				coords = floor(coords * cellSize) / cellSize;

				float randVal = lerp(0, rand(coords), (distToOrigin));//rand(coords) - (saturate((1.0 - distToCenter))) * 0.25;
				if(randVal > hitPoints * 1.5) {
					//outCol.a = min(outCol.a, hitPoints * 0.5);					
				}
				if(hitPoints < 0.60) {
						outCol.rgb = lerp(damageColor, outCol.rgb, hitPoints * 1.5);
					}
				
				outCol.rgb = float3(outCol.r, outCol.r, outCol.r) * 0.05;
				// inside CGPROGRAM in the fragment Shader:
				float alphaCutoffValue = 0.1;
				clip(outCol.a - alphaCutoffValue);
				return outCol;
			}
			ENDCG
		}
	}
}
