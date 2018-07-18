// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Water/WaterCurveStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FluidColorTex ("_FluidColorTex", 2D) = "black" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex("_SkyTex", 2D) = "white" {}
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
			#pragma target 5.0
			
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD0;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD1;
				float2 skyUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 screenUV : TEXCOORD4;
				float2 altitudeUV : TEXCOORD5;
				float4 vignetteLerp : TEXCOORD6;
			};

			struct WaterCurveData {   // 2 ints, 17 floats
				int index;    
				float2 p0;
				float2 p1;
				float2 p2;
				float2 p3;
				float4 widths;
				float3 hue;
				float strength;   // extra data to use however
				float age;  // to allow for fade-in and fade-out
				int brushType;  // brush texture mask
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _FluidColorTex; 
			sampler2D _AltitudeTex; 
			sampler2D _SkyTex;

			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;

			StructuredBuffer<WaterCurveData> waterCurveStrokesCBuffer;
			StructuredBuffer<float3> verticesCBuffer;

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			// CUBIC:
			float2 GetPoint2D (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
			}
			float2 GetPoint1D (float p0, float p1, float p2, float p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
			}
			// CUBIC
			float2 GetFirstDerivative (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return 3.0 * oneMinusT * oneMinusT * (p1 - p0) + 6.0 * oneMinusT * t * (p2 - p1) + 3.0 * t * t * (p3 - p2);
			}
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				WaterCurveData waterCurveData = waterCurveStrokesCBuffer[inst];

				float3 worldPosition = float3(waterCurveData.p0, 0);
				
				float3 vertexPos = verticesCBuffer[id];
				float2 uv = verticesCBuffer[id];
				uv.x += 0.5;
								
				o.skyUV = worldPosition.xy / _MapSize + float2(-0.25, 0.08) * 0.14 * _Time.y;

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomWidth = lerp(0.75, 1.3, random1) * 2.65;				

				// Figure out worldPosition by constructing a bezierCurve between the 4 points
				// and adjusting vertPosition based on curve output.
				// curve t = vertex UV.x
				// find tangent and bitangent of curve and use to project vertex UV.y
				float t = uv.y;
				uv.y = 1.0 - uv.y;

				float2 curvePos = GetPoint2D(waterCurveData.p0, waterCurveData.p1, waterCurveData.p2, waterCurveData.p3, t);
				float2 curveTangent = normalize(GetFirstDerivative(waterCurveData.p0, waterCurveData.p1, waterCurveData.p2, waterCurveData.p3, t));
				float2 curveBitangent = float2(curveTangent.y, -curveTangent.x);

				float randThreshold = rand(float2(inst, randomWidth));	
				float4 screenPos = ComputeScreenPos(mul(UNITY_MATRIX_VP, float4(curvePos, 0, 1)));
				float2 sampleUV = screenPos.xy / screenPos.w;
				float vignetteRadius = length((sampleUV - 0.5) * 2);
				float testNewVignetteMask = saturate((randThreshold + 1.15 - vignetteRadius) * 1.3);
				o.vignetteLerp = float4(testNewVignetteMask,sampleUV,vignetteRadius);

				float width = GetPoint1D(waterCurveData.widths.x, waterCurveData.widths.y, waterCurveData.widths.z, waterCurveData.widths.w, t) * 0.75 * (1 - saturate(testNewVignetteMask));
				float2 offset = curveBitangent * -verticesCBuffer[id].x * width * randomWidth; // *** support full vec4 widths!!!

				float fadeDuration = 0.1;
				float fadeIn = saturate(waterCurveData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - waterCurveData.age) / fadeDuration);
							
				float alpha = fadeIn * fadeOut;
				//float testThresholdMask = saturate((randThreshold + 1 - alpha) * 10000);
				//alpha *= testThresholdMask;
				

				float4 waveNoise = Value3D(float3(waterCurveData.p0, _Time.y), 1);

				o.normal = waveNoise;
				o.pos = UnityObjectToClipPos(float4(curvePos, 0, 1.0) + float4(offset, 0.0, 0.0));

				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				//float4 test = UnityObjectToClipPos (worldPosition);  
				
				
				o.screenUV = ComputeScreenPos(pos); // ** Divide In frag shader!!!
				
				o.worldPos = float3(curvePos,0) + float4(offset, 0.0, 0.0);
				o.color = float4(waterCurveData.hue, alpha);				
				o.uv = uv;
				o.altitudeUV = worldPosition.xy / _MapSize * 0.5 + 0.25;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				//float2 screenUV = i.screenUV.xy * 0.01;
				float2 screenUV = i.screenUV.xy / i.screenUV.w;

				float distFromScreenCenter = length(screenUV * 2 - 1);
				float vignetteMask = smoothstep(0.6, 1.2, saturate(distFromScreenCenter)) * 1;
				

				// sample the texture
				//float4 fluidDensityColor = tex2D(_FluidColorTex, i.fluidTexUV);  //  Color of brushtroke source					
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				// get view space position of vertex
                //float3 viewPos = UnityObjectToViewPos(i.worldPos);
				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);

				//float3 waterWorldNormal = normalize(float3(i.normal.xy * 1,-1));

				float4 bgColor = tex2D(_RenderedSceneRT, screenUV);	
				float altitude = tex2D(_AltitudeTex, i.altitudeUV * 0.5 + 0.25); //i.worldPos.z / 10; // [-1,1] range
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				float strataColorMultiplier = (sin(altitude * (1.0 + i.worldPos.x * 0.01 - i.worldPos.y * -0.01) + i.worldPos.x * 0.01 - i.worldPos.y * 0.01) * 0.5 + 0.5) * 0.5 + 0.5;
				bgColor.rgb *= strataColorMultiplier;
				bgColor.rgb = lerp(bgColor.rgb, waterFogColor, 1 * (saturate(altitude * 0.8)) + 0.25 * isUnderwater);

				float snowAmount = saturate((-altitude - 0.6) * 2 +
								   ((sin(i.worldPos.x * 0.0785 + i.worldPos.y * 0.02843) * 0.5 + 0.5) * 1 - 
								   (cos(i.worldPos.x * 0.012685 + i.worldPos.y * -0.01843) * 0.5 + 0.5) * 0.9 +
								   (sin(i.worldPos.x * 0.2685 + i.worldPos.y * -0.1843) * 0.5 + 0.5) * 0.45 - 
								   (cos(i.worldPos.x * -0.2843 + i.worldPos.y * 0.01143) * 0.5 + 0.5) * 0.45 +
								   (sin(i.worldPos.x * 0.1685 + i.worldPos.y * -0.03843) * 0.5 + 0.5) * 0.3 - 
								   (cos(i.worldPos.x * -0.1843 + i.worldPos.y * 0.243) * 0.5 + 0.5) * 0.3) * 0.5);
				
				bgColor.rgb = lerp(bgColor.rgb, float3(0.56, 1, 0.34) * 0.6, snowAmount * 1);
				bgColor.a = col.a;

				//brushColor = float4(screenUV.xy,0,1);
				//return brushColor;
				//float3 reflectedViewDir = cameraToVertexDir + 2 * waterWorldNormal;
				//float4 finalCol = lerp(col, fluidDensityColor, 0);				
				//finalCol.a = col.a;
				//finalCol.rgb *= 1.125;
				//float4 frameBufferColor = float4(tex2Dlod(_SkyTex, float4((i.fluidTexUV + i.normal.xy * 0.01) * 0.75, 0, 1)).rgb, col.a); //col;
				//float4 frameBufferColor = float4(tex2Dlod(_SkyTex, float4(screenUV,0,0)).rgb, col.a); //col;
				//finalColor.a *= vignetteMask;
				float4 reflectedColor = float4(tex2Dlod(_SkyTex, float4((i.skyUV + i.normal.xy * 0.01) * 0.75, 0, 1)).rgb, col.a); //col;
				
				
				
				float4 finalColor = lerp(reflectedColor, bgColor, i.vignetteLerp.x); //float4(1,1,1,1);
				finalColor.a *= (1 - saturate(i.vignetteLerp.x) * 0.4) * 0.5;

				//return float4(i.vignetteLerp.w,0,0,1);
				return finalColor;

				//return finalCol;
			}
			ENDCG
		}
	}
}
