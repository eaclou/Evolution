Shader "Brushstrokes/AnimalParticleShadowDisplayShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}  // stem texture sheet	
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
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
			sampler2D _AltitudeTex;	
			sampler2D _WaterSurfaceTex;
			uniform float _MapSize;

			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				
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

				float t = uv.y; // + 0.5;
				uv.y = 1.0 - uv.y;

				float2 curvePos = GetPoint2D(particleData.worldPos.xy, particleData.p1, particleData.p2, particleData.p3, t);
				float2 curveTangent = normalize(GetFirstDerivative(particleData.worldPos.xy, particleData.p1, particleData.p2, particleData.p3, t));
				float2 curveBitangent = float2(curveTangent.y, -curveTangent.x);

				float width = sqrt(particleData.biomass) * 0.982 * (1 - 2 * abs(0.75 - uv.y)); //GetPoint1D(waterCurveData.widths.x, waterCurveData.widths.y, waterCurveData.widths.z, waterCurveData.widths.w, t) * 0.75 * (1 - saturate(testNewVignetteMask));
				float2 offset = curveBitangent * -quadPoint.x * width; // * randomWidth; // *** support full vec4 widths!!!
				
				float3 worldPosition = float3(curvePos,0) + float4(offset, 0.0, 0.0);

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;				
				float refractionStrength = 0.15;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				float2 altUV = worldPosition.xy / 256;				
				
				worldPosition.z = -(tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x * 2 - 1) * 10;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));			
				o.uv = uv; //quadVerticesCBuffer[id].xy + 0.5f;	

				//o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), saturate(particleData.age * 0.5), 1);
								
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				float4 texColor = tex2D(_MainTex, i.uv);	
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.33;
				texColor.rgb = waterFogColor;  // shadow
				texColor.a *= 0.25;
				return texColor;
				//return float4(0.7,1,0.1,texColor.a * 0.75);
			}
		ENDCG
		}
	}
}
