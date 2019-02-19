Shader "Brushstrokes/AnimalParticleDisplayShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}  // stem texture sheet		
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		//_Tint("Color", Color) = (1,1,1,1)
		//_Size("Size", vector) = (1,1,1,1)
	}
	SubShader
	{		
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		//Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsAnimalParticles.cginc"

			sampler2D _MainTex;
			sampler2D _WaterSurfaceTex;
			
			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				
			};

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
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy;
				uv.x += 0.5;
				
				AnimalParticleData particleData = animalParticleDataCBuffer[inst];

				//float3 worldPosition = float3(particleData.worldPos.xy, 1.0);
				//quadPoint.y = (quadPoint.y - 0.35) * 4.20 * 0.365;
				//quadPoint = quadPoint * particleData.radius * 0.568241; // * particleData.active; // *** remove * 3 after!!!
				
				


				// Figure out worldPosition by constructing a bezierCurve between the 4 points
				// and adjusting vertPosition based on curve output.
				// curve t = vertex UV.x
				// find tangent and bitangent of curve and use to project vertex UV.y
				float t = uv.y; // + 0.5;
				uv.y = 1.0 - uv.y;

				float2 curvePos = GetPoint2D(particleData.worldPos.xy, particleData.p1, particleData.p2, particleData.p3, t);
				float2 curveTangent = normalize(GetFirstDerivative(particleData.worldPos.xy, particleData.p1, particleData.p2, particleData.p3, t));
				float2 curveBitangent = float2(curveTangent.y, -curveTangent.x);

				//float randThreshold = rand(float2(inst, randomWidth));	
				//float4 screenPos = ComputeScreenPos(mul(UNITY_MATRIX_VP, float4(curvePos, 0, 1)));
				//float2 sampleUV = screenPos.xy / screenPos.w;
				//float vignetteRadius = length((sampleUV - 0.5) * 2);
				//float testNewVignetteMask = saturate((randThreshold + 1.15 - vignetteRadius) * 1.3);
				//o.vignetteLerp = float4(testNewVignetteMask,sampleUV,vignetteRadius);

				//float width = 
				float width = sqrt(particleData.biomass) * 0.42 * (1 - 2 * abs(0.75 - uv.y)); //GetPoint1D(waterCurveData.widths.x, waterCurveData.widths.y, waterCurveData.widths.z, waterCurveData.widths.w, t) * 0.75 * (1 - saturate(testNewVignetteMask));
				float2 offset = curveBitangent * -quadPoint.x * width; // * randomWidth; // *** support full vec4 widths!!!
				
				//float fadeDuration = 0.1;
				//float fadeIn = saturate(waterCurveData.age / fadeDuration);  // fade time = 0.1
				//float fadeOut = saturate((1 - waterCurveData.age) / fadeDuration);							
				//float alpha = fadeIn * fadeOut;
				
				//o.pos = UnityObjectToClipPos(float4(curvePos, 0, 1.0) + float4(offset, 0.0, 0.0));
				//o.worldPos = float3(curvePos,0) + float4(offset, 0.0, 0.0);
				float3 worldPosition = float3(curvePos,0) + float4(offset, 0.0, 0.0);



				
				// Figure out final facing Vectors!!!
				//float2 forward0 = normalize(particleData.velocity);
				//float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				//float3 rotatedPoint0 = float3(quadPoint.x * right0 + quadPoint.y * forward0,
				//							 quadPoint.z);
				
				//worldPosition = worldPosition + rotatedPoint0; // * particleData.active;

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;				
				float refractionStrength = 0.15;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));			
				o.uv = uv; //quadVerticesCBuffer[id].xy + 0.5f;	

				o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), saturate(particleData.age * 0.5), 1);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);
				
				float val = i.color.a;
				
				float4 finalColor = float4(i.color.rgb, i.uv.y); // float4(float3(i.color.z * 1.2, 0.85, (1.0 - i.color.w) * 0.2) + i.color.y, texColor.a * i.color.x * 0.33 * (1 - i.color.z));
				finalColor.a *= 1.0;
				finalColor.rgb = float3(0.45, 0.55, 1.295) * 1.25;
				return finalColor;
			}
		ENDCG
		}
	}
}
