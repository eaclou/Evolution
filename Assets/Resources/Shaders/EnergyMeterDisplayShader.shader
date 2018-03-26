Shader "Unlit/EnergyMeterDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

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
				//return float4(1,1,1,1);
				float4 foodRValue = tex2D(_MainTex, float2(0.375, 0.33));
				float4 foodGValue = tex2D(_MainTex, float2(0.625, 0.33));
				float4 foodBValue = tex2D(_MainTex, float2(0.875, 0.33));
				
				float foodTotal = foodRValue.x + foodGValue.x + foodBValue.x;
				float foodAvgPercentage = foodTotal / 3;

				float divisions = 8;

				float2 newUV = i.uv;
				newUV.x *= divisions;
				float cellID = floor(newUV.x);
				newUV.x = frac(newUV.x);

				
				newUV = newUV - 0.5;  // make center of each section 0

				float xScale = 1.1;
				newUV.x *= xScale;
				

				float clock = _Time.y * 2.5;
				float rotateAmp = 0.09;
				newUV = float2(newUV.x + cos(clock + cellID) * rotateAmp, newUV.y + sin(clock + cellID) * rotateAmp);

				// Danger Wobble:
				float dangerAmount = saturate(1.0 - foodAvgPercentage * 1.44);
				float dangerClockSpeed = clock * dangerAmount * 1.05;
				float freq = 0.8 * (1 + dangerAmount * 0.6);
				float3 noiseOffset = Value2D(dangerClockSpeed + cellID * 10, freq);
				float noiseAmp = 0.075 * dangerAmount;
				newUV += noiseOffset * noiseAmp;

				//newUV = float2(newUV.x + cos(clock + i.uv.x * 10) * wobbleAmp, newUV.y + sin(clock + i.uv.x * 10) * wobbleAmp);

				float displayValue = 0;
				float distToCenter = length(newUV);

				float coreMask = 1.0 - smoothstep(0.16, 0.24, distToCenter);
				float glowMask = 1.0 - smoothstep(0.16, 0.4, distToCenter) - coreMask;
				displayValue = glowMask;
				
				float litMask = ceil((foodAvgPercentage * divisions - cellID) / divisions);

				float coreBrightness = (1.0 - smoothstep(0, 0.3, distToCenter)) * 0.3 + 0.7;

				//foodAvgPercentage = 0.2;

				float3 healthColor = float3(0.25,1,0.36);
				if(foodAvgPercentage < 0.5) {
					healthColor = float3(0.8,0.4,0.15) * 0.84;
				}
				if(foodAvgPercentage < 0.25) {
					healthColor = float3(0.35,0.06,0.05);
				}
				//float3 healthColor = lerp(float3(0.8,0.1,0.05), float3(0.25,1,0.36), colorTier);
				//float3 foodColor = float3(0,0,0);

				float4 coreColor = float4(healthColor * coreBrightness * 3,litMask * coreMask);
				float4 glowColor = float4(healthColor * 0.6, litMask * glowMask);

				fixed4 displayColor = coreColor; //float4(healthColor * coreBrightness * 3,litMask * coreMask); // BG black
				displayColor = lerp(displayColor, glowColor, litMask * glowMask);
				//displayColor.a += litMask * glowMask;
				//displayColor.rgb = lerp(healthColor, healthColor, litMask);

				//if(i.uv.x < foodAvgPercentage) {
				//	float3 healthColor = lerp(float3(0.8,0.1,0.05), float3(0.25,1,0.36), foodAvgPercentage);
				//	displayColor.rgb = lerp(foodRColor * foodRPercentage + foodGColor * foodGPercentage + foodBColor * foodBPercentage, healthColor, 0.5);
				//}
				return displayColor;
			}
			ENDCG
		}
	}
}
