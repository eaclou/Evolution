Shader "HUD/InspectLifeCycleShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("_Tint", Color) = (1,1,1,1)
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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Tint;
			uniform float _FillPercentage;
			uniform int _CurLifeStage;
			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.color = float4(1,1,1,1);
				//o.color.rgb *= (float)_CurLifeStage / 3.0;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 centeredPixelCoords = (i.uv - 0.5) * 2;
				float distToOrigin = length(centeredPixelCoords);
				float alpha = 1.0 - saturate((distToOrigin - 0.95) * 25);
				float radialMask1 = 1.0 - saturate((distToOrigin - 0.6) * 25);

				float3 barHue = float3(1, 1, 1);
				float3 eggHue = float3(0, 0, 0);
				float3 youngHue = float3(0.4, 0.8, 0.6);
				float3 matureHue = float3(0.7, 0.9, 1);
				float3 decayHue = float3(0.4, 0.1, 0.05);

				float rawLifeStageFloat = (float)_CurLifeStage;
				float eggArcSize = 0.1;
				float youngArcSize = 0.2;
				float matureArcSize = 0.6;
				float decayArcSize = 0.1;
				float remainingArcSize = saturate(1.0 - eggArcSize - youngArcSize - decayArcSize);
				
				float curRevolutionPercent = eggArcSize * _FillPercentage; // egg
				barHue = lerp(eggHue, youngHue, _FillPercentage);
				if(rawLifeStageFloat > 0.5) { // young
					curRevolutionPercent = eggArcSize + youngArcSize * _FillPercentage;
					barHue = lerp(youngHue, matureHue, _FillPercentage);
				}	
				if(rawLifeStageFloat > 1.5) {  // mature
					curRevolutionPercent = eggArcSize + youngArcSize + matureArcSize * _FillPercentage;
					barHue = matureHue;
				}
				if(rawLifeStageFloat > 2.5) {  // dead
					curRevolutionPercent = eggArcSize + youngArcSize + matureArcSize + decayArcSize * _FillPercentage;
					barHue = lerp(matureHue, decayHue, (_FillPercentage * 0.25) + 0.75);
				}
				
				float pixelRotationPercent = 0;
				float2 dir = normalize(centeredPixelCoords);

				if(dir.x < 0) {
					if(dir.y < 0) {
						pixelRotationPercent = dot(dir, float2(-1,0)) * 0.25;
					}
					else {
						pixelRotationPercent = dot(dir, float2(0,1)) * 0.25 + 0.25;
					}
				}
				else {
					if(dir.y < 0) {
						pixelRotationPercent = dot(dir, float2(0,-1)) * 0.25 + 0.75;
					}
					else {
						pixelRotationPercent = dot(dir, float2(1,0)) * 0.25 + 0.5;
					}
				}

				float fillMask = saturate(curRevolutionPercent - pixelRotationPercent) * 50;

				float3 centerHue = float3(0.5, 0.5, 0.5);
				
				// sample the texture
				fixed4 col = float4(0,0,0,alpha); //tex2D(_MainTex, i.uv);
				col.rgb = lerp(col.rgb, barHue, saturate(fillMask) * (1.0 - radialMask1));
				col.rgb = lerp(col.rgb, centerHue, radialMask1);
				col.rgb *= ((i.uv.y * 0.6) + 0.4) * (1.0 - saturate(distToOrigin) * 0.36);
				return col;
			}
			ENDCG
		}
	}
}
