Shader "Brushstrokes/FoodParticleShadowDisplayShader"
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
			uniform float _MapSize;


			StructuredBuffer<AlgaeParticleData> foodParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				
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

				//float threshold = 

				float3 worldPosition = float3(particleData.worldPos, 5);    //float3(rawData.worldPos, -random2);
				float rand0 = rand(float2(inst, inst) * 10);
				float rand1 = rand(float2(rand0, rand0) * 10);
				float rand2 = rand(float2(rand1, rand1) * 10);
				float2 offsetRaw = (float2(rand0, rand1) * 2 - 1);				
				worldPosition.xy += offsetRaw * 0.5;

				float threshold = particleData.biomass * 4.5 + 0.033;
				float isOn = saturate((threshold - length(offsetRaw)) * 100);

				float masterFreq = 3;
				float spatialFreq = 0.55285;
				float timeMult = 0.06;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.15;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;


				//quadPoint = quadPoint * particleData.radius * particleData.active;
				quadPoint = quadPoint * particleData.radius * 0.2 * isOn; // * (1.0 - particleData.digestedAmount) * 4 * particleData.isActive;

				worldPosition = worldPosition + quadPoint;
				// REFRACTION:
				float2 altUV = (worldPosition.xy + 128) / 512;

				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 2)).x;

				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				float depth = saturate(-altitudeRaw + 0.5);
				float refractionStrength = depth * 5.5;

				//float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				
				//o.altitudeUV = altUV;
				
				worldPosition.z = -(altitudeRaw * 2 - 1) * 10;
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.worldPos = worldPosition;

				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				float4 texColor = tex2D(_MainTex, i.uv);	
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				texColor.rgb = waterFogColor;  // shadow
				texColor.a *= 0.3;
				//texColor = float4(1,1,1,1);
				return texColor;
				//return float4(0.7,1,0.1,texColor.a * 0.75);
			}
		ENDCG
		}
	}
}
