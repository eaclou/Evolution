Shader "Unlit/AgentShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			#pragma multi_compile_fog
			
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
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 texSample = tex2D(_MainTex, i.uv);

				float4 bgColor = float4(0.0, 0.6, 0.88, 1.0);
				float4 damageColor = float4(1,-0.4,-0.4,1);
				float damageRadius = 0.8;
				
				
				float2 coords = float2((i.uv - 0.5) * 2.0);
				float circle = ceil(1.0 - saturate(length(coords)));
				float distToOrigin = length(coords);

				float4 outCol = float4(bgColor.rgb, circle);

				

				float foodRadius = 0.8;

				float foodEnergy = (tex2D(_MainTex, float2(0.375, 0.333)).r + tex2D(_MainTex, float2(0.625, 0.333)).r + tex2D(_MainTex, float2(0.875, 0.333)).r) / 3.0;
				
				//float2 foodOrigin = float2(0, 0);
				//float foodDist = length(coords - foodOrigin);
				//if(foodDist < foodRadius) {
				//	outCol.rgb = lerp(damageColor, bgColor, foodEnergy);
				//}
				float hitPoints = tex2D(_MainTex, float2(0.125, 0.333)).r;
				//if(distToOrigin >= damageRadius) {
					//float4 sampleCol = tex2D(_MainTex, float2(0.125, 0.333));
					//outCol.rgb = lerp(damageColor, bgColor, sampleCol.r);
				//}

				float healthValue = min(foodEnergy, hitPoints);
				outCol.rgb = lerp(damageColor, bgColor, healthValue);

				float outCommRadius = 0.225;
				float3 negColor = float3(1, 2, 1);
				float3 posColor = float3(1, 1, 2);
				
				float2 outCommOrigin0 = float2(0.2, 0.2);
				float outCommDist0 = length(coords - outCommOrigin0);
				if(outCommDist0 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.125, 0.667));
					outCol.rgb = lerp(negColor, posColor, sampleCol.r) * (sampleCol.r * 2.0 - 1.0);
				}
				float2 outCommOrigin1 = float2(0.2, -0.2);
				float outCommDist1 = length(coords - outCommOrigin1);
				if(outCommDist1 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.375, 0.667));
					outCol.rgb = lerp(negColor, posColor, sampleCol.r) * (sampleCol.r * 2.0 - 1.0);
				}
				float2 outCommOrigin2 = float2(-0.2, 0.2);
				float outCommDist2 = length(coords - outCommOrigin2);
				if(outCommDist2 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.625, 0.667));
					outCol.rgb = lerp(negColor, posColor, sampleCol.r) * (sampleCol.r * 2.0 - 1.0);
				}
				float2 outCommOrigin3 = float2(-0.2, -0.2);
				float outCommDist3 = length(coords - outCommOrigin3);
				if(outCommDist3 < outCommRadius) {
					float4 sampleCol = tex2D(_MainTex, float2(0.875, 0.667));
					outCol.rgb = lerp(negColor, posColor, sampleCol.r) * (sampleCol.r * 2.0 - 1.0);
				}
				
				outCol.rgb = lerp(outCol.rgb, float3(0.5, 0.75, 0.9), 0.94);

				// inside CGPROGRAM in the fragment Shader:
				float alphaCutoffValue = 0.1;
				clip(outCol.a - alphaCutoffValue);
				return outCol;
			}
			ENDCG
		}
	}
}
