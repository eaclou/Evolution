Shader "Brushstrokes/AlgaeParticleDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet	
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
			#include "Assets/Resources/Shaders/Inc/StructsAlgaeParticles.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			
			StructuredBuffer<AlgaeParticleData> foodParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];

				int particleIndex = floor((float)inst / 128.0);

				

				float rand0 = rand(float2(inst, inst) * 10);
				float rand1 = rand(float2(rand0, rand0) * 10);
				float rand2 = rand(float2(rand1, rand1) * 10);
				
							
				AlgaeParticleData particleData = foodParticleDataCBuffer[particleIndex];

				float3 worldPosition = float3(particleData.worldPos, 1.0);    //float3(rawData.worldPos, -random2);
				float2 offset = (float2(rand0, rand1) * 2 - 1) * (1 * particleData.biomass + 1);
				worldPosition.xy += offset;
				worldPosition.z = rand2 * 6;

				float masterFreq = 1;
				float spatialFreq = 0.55285;
				float timeMult = 0.013;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.520;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;

				float2 altUV = (worldPosition.xy + 128) / 512;
				//o.altitudeUV = altUV;
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range

				float threshold = particleData.biomass * 10;
				float isOn = saturate((threshold - ((float)(inst % 128) / 128)) * 100);

				worldPosition.z = min(worldPosition.z, (1.0 - altitudeRaw) * 1);
				float radius = (0.2 + saturate(particleData.biomass * 0.42) + rand2 * 0.4) * isOn * 0.56; //sqrt(particleData.biomass) * 2 + 0.5;
				quadPoint.x *= 0.52;
				quadPoint = quadPoint * radius; // * particleData.active; // *** remove * 3 after!!!

				float2 forward = normalize((float2(rand1, rand2) * 2 - 1));
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward, quadPoint.z);  // Rotate localRotation by AgentRotation
				//float3 rotatedPoint = 

				
				worldPosition = worldPosition + rotatedPoint * particleData.isActive;

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;				
				float refractionStrength = 0.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;


				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
								
				o.color = float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), rand2, 1 - saturate(particleData.isDecaying));
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);
				
				float val = i.color.a;
				//float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				float4 finalColor = i.color; // float4(float3(i.color.z * 1.2, 0.85, (1.0 - i.color.w) * 0.2) + i.color.y, texColor.a * i.color.x * 0.33 * (1 - i.color.z));
				finalColor.rgb = lerp(float3(0.25, 1, 0.36), float3(0.03,0.4,0.3) * 0.4, i.color.z);
				finalColor.r += i.color.r;
				finalColor.a = texColor.a * 0.45;
				return finalColor;
			}
		ENDCG
		}
	}
}
