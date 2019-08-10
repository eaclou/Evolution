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
				
				float3 offsetRaw = (float3(rand0, rand1, rand2) * 2 - 1) * rand3;				
				//float2 offset = offsetRaw * (16 * particleData.biomass + 0.2);
				float maxSpread = 6.28;
				float spread = (saturate(16 * particleData.biomass * particleData.biomass) * 0.975 + 0.025) * maxSpread;
				worldPosition.xyz += offsetRaw * spread;
				
				float threshold = particleData.biomass * 1.5 + 0.06;
				float isOn = saturate((threshold - length(offsetRaw)) * 10);

				float masterFreq = 5;
				float spatialFreq = 0.06125285;
				float timeMult = 0.08;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + offsetRaw + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.2;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;


				float radius = saturate(8 * particleData.biomass * particleData.biomass) * 4 + 0.042; // particleData.radius * 0.3 * isOn; // 1; //sqrt(particleData.biomass) * 2 + 0.5;
				quadPoint = quadPoint * radius; // * particleData.active; // *** remove * 3 after!!!
				quadPoint.y *= 1.6;
				float randAngle = (rand2 + rand3 * rand0 - rand1) * 13.92;
				
				float2 forward = float2(cos(randAngle), sin(randAngle));
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward, 0);  // Rotate localRotation by AgentRotation

				
				worldPosition.z = 0.0;
				worldPosition = worldPosition + rotatedPoint * particleData.isActive;

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;				
				float refractionStrength = 0.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;


				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
								
				o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), 1 - rand2, 1 - saturate(particleData.isDecaying));
				o.hue = float4(particleData.color, 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);
				float4 texColor = tex2D(_MainTex, i.uv);
				
				float val = i.color.a;
				
				float4 finalColor = i.hue; // float4(float3(i.color.z * 1.2, 0.85, (1.0 - i.color.w) * 0.2) + i.color.y, texColor.a * i.color.x * 0.33 * (1 - i.color.z));
				finalColor.rgb = lerp(finalColor.rgb, float3(0.35, 0.95, 0.45), 0.07);
				//finalColor.rgb += 0.25;
				finalColor.a = texColor.a * 0.8175;
				
				finalColor.rgb = lerp(finalColor, float3(0.81, 0.79, 0.65) * 0.4, i.color.x);
				finalColor.rgb *= i.color.z * 0.3 + 0.7;
				//finalColor.rgb = i.hue;
				return finalColor;
			}
		ENDCG
		}
	}
}
