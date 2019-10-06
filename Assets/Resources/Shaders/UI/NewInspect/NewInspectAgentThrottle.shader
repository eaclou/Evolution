Shader "UI/NewInspectAgentThrottleShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_VelocityTex ("_VelocityTex", 2D) = "white" {}
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
				//float2 mapUV : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _VelocityTex;
			uniform float4 _Tint;
			uniform float _ThrottleX;
			uniform float _ThrottleY;
			uniform float _ThrottleMag;
			uniform float2 _ThrottleDirNormalized;

			uniform float _AgentCoordX;
			uniform float _AgentCoordY;
			
			v2f vert (appdata v)
			{
				v2f o;
				//o.mapUV = v.vertex.xy / 256.0;  // _MapSize!!!! ***
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// i.uv.x --> shrink and center on agentCoord
				float waterSize = 0.05;
				
				

				float2 centeredUV = (float2(i.uv.x, i.uv.y) - 0.5) * 2;  // center of quad is (0,0), radius 1
				float2 throttlePosUV = float2(_ThrottleX, _ThrottleY);

				float distToThrottle = length(throttlePosUV - centeredUV);
				float distToOrigin = length(centeredUV);
				
				float radius = lerp(0.025, 0.25, distToOrigin);
				float fillAreaMask = 1.0 - saturate((distToThrottle - radius) * 25);
				
				
				float alpha = 1.0; // - saturate((distToOrigin - 0.975) * 25);

				// GRID of dots?

				float numDots = 3;
				float2 gridSignedUV = (float2(frac(i.uv.x * numDots), frac(i.uv.y * numDots)) - 0.5) * 2;

				float2 waterVelUV = float2(i.uv.x * waterSize - waterSize * 0.5 + _AgentCoordX, i.uv.y * waterSize - waterSize * 0.5 + _AgentCoordY);
				
				float4 waterVelSample = tex2Dlod(_VelocityTex, float4(waterVelUV, 0, 4));
				float2 vel = waterVelSample.xy * 100;
				float2 velNorm = normalize(vel) * 1;
				float velMagNorm = saturate(length(vel));


				float speed = 1;
				float phase0 = frac(_Time.y * speed - 0.5);
				float phase1 = frac(_Time.y * speed + 0.0);
				float phase2 = frac(_Time.y * speed + 0.5);
				float distanceToGridCellDot0 = length(gridSignedUV + velNorm * 0);
				float distanceToGridCellDot1 = length(gridSignedUV + velNorm * phase1);
				float distanceToGridCellDot2 = length(gridSignedUV + velNorm * phase2);
				
				float dotAlpha0 = 1.0; // sin(phase0 * 3.14159);
				float dotMask0 = 1.0 - saturate((distanceToGridCellDot0 - 0.1) * 10);
				float dotAlpha1 = 1.0 - phase1; //sin(phase1 * 3.14159);
				float dotMask1 = 1.0 - saturate((distanceToGridCellDot1 - 0.1) * 10);
				float dotAlpha2 = 1.0 - phase2; //sin(phase2 * 3.14159);
				float dotMask2 = 1.0 - saturate((distanceToGridCellDot2 - 0.1) * 10);
				
				float centeredMask = 1.0 - saturate((distToOrigin - 0.1) * 25);

				fixed4 col = fixed4(0,0,0,alpha);	
				col.xyz = lerp(col.xyz, float3(0.5,0.6,1) * 0.67, max(max(dotMask0 * dotAlpha0, dotMask1 * dotAlpha1), dotMask2 * dotAlpha2));

				col.rgb = lerp(col.rgb, float3(1,1,0.25), centeredMask);
				col.rgb = lerp(col.rgb, _Tint.rgb, fillAreaMask);
				//col.rgb *= ((i.uv.y * 0.6) + 0.4) * (1.0 - saturate(distToOrigin) * 0.36);
				return col;
			}
			ENDCG
		}
	}
}
