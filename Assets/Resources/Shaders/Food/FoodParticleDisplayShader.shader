Shader "Brushstrokes/FoodParticleDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet		
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
			#include "Assets/Resources/Shaders/Inc/StructsAlgaeParticles.cginc"

			sampler2D _MainTex;
			sampler2D _WaterSurfaceTex;
			
			StructuredBuffer<AlgaeParticleData> foodParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				float4 hue : TEXCOORD1;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				
				int particleIndex = floor((float)inst / 32.0);

				AlgaeParticleData particleData = foodParticleDataCBuffer[particleIndex];

				

				float3 worldPosition = float3(particleData.worldPos, 1.0);    //float3(rawData.worldPos, -random2);

				float rand0 = rand(float2(inst, inst) * 10);
				float rand1 = rand(float2(rand0, rand0) * 10);
				float rand2 = rand(float2(rand1, rand1) * 10);	
				float rand3 = rand(float2(rand2, rand2) * 10);		
				
				float3 offsetRaw = (float3(rand0, rand1, rand2) * 2 - 1);				
				//float2 offset = offsetRaw * (16 * particleData.biomass + 0.2);
				worldPosition.xyz += offsetRaw * 0.5;
				
				float threshold = particleData.biomass * 4.5 + 0.033;
				float isOn = saturate((threshold - length(offsetRaw)) * 100);

				float masterFreq = 3;
				float spatialFreq = 0.55285;
				float timeMult = 0.06;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.015;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;


				float radius = particleData.radius * 0.10915 * isOn; // 1; //sqrt(particleData.biomass) * 2 + 0.5;
				quadPoint = quadPoint * radius; // * particleData.active; // *** remove * 3 after!!!
				
				
				worldPosition = worldPosition + quadPoint * particleData.isActive;

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;				
				float refractionStrength = 0.5;
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;


				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
								
				o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), 0, 1 - saturate(particleData.isDecaying));
				o.hue = float4(particleData.color, 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);
				
				float val = i.color.a;
				
				float4 finalColor = i.color; // float4(float3(i.color.z * 1.2, 0.85, (1.0 - i.color.w) * 0.2) + i.color.y, texColor.a * i.color.x * 0.33 * (1 - i.color.z));
				finalColor.rgb = lerp(finalColor.rgb, float3(0.25, 1, 0.36), 1);
				finalColor.rgb += 0.25;
				finalColor.a = texColor.a * 0.9; // * 0.25;
				
				finalColor.rgb = lerp(finalColor, i.hue, 0.5);
				//finalColor.rgb = 1;
				return finalColor;
			}
		ENDCG
		}
	}
}
