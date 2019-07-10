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
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;	
			sampler2D _WaterSurfaceTex;
			uniform float _MapSize;
			sampler2D _RenderedSceneRT; 

			StructuredBuffer<AlgaeParticleData> foodParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float2 altitudeUV : TEXCOORD1;
				float4 screenUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				//float4 color : COLOR;
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
				float rand3 = rand(float2(rand2, rand2) * 10);	

				float3 offsetRaw = (float3(rand0, rand1, rand2) * 2 - 1) * rand3;
				worldPosition.xyz += offsetRaw * 5;

				//float2 offsetRaw = (float2(rand0, rand1) * 2 - 1) * rand3;					
				//worldPosition.xy += offsetRaw * 5;

				float threshold = particleData.biomass * 4.5 + 0.033;
				float isOn = saturate((threshold - length(offsetRaw)) * 10);

				float masterFreq = 5;
				float spatialFreq = 0.06125285;
				float timeMult = 0.08;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + offsetRaw + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.18;
				float3 noiseOffset = noiseSample.yzw * noiseMag;
				

				worldPosition.xyz += noiseOffset;


				//quadPoint = quadPoint * particleData.radius * particleData.active;
				quadPoint = quadPoint * particleData.radius * 0.2 * isOn; // * (1.0 - particleData.digestedAmount) * 4 * particleData.isActive;

				worldPosition = worldPosition + quadPoint;
				// REFRACTION:
				float2 altUV = worldPosition.xy / 256;
				o.altitudeUV = altUV;
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x;

				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				float depth = saturate(-altitudeRaw + 0.5);
				float refractionStrength = depth * 5.5;

				//float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				
				//o.altitudeUV = altUV;
				
				worldPosition.z = -(altitudeRaw * 2 - 1) * 10;
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.worldPos = worldPosition;

				o.worldPos = worldPosition;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				
				float4 screenUV = ComputeScreenPos(o.pos);
				o.screenUV = screenUV; //altitudeUV.xy / altitudeUV.w;

				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);


				float4 brushColor = tex2D(_MainTex, i.uv);	
				
				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source					
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				//float4 waterColorTex = tex2D(_WaterColorTex, (i.altitudeUV - 0.25) * 2);

				//frameBufferColor = float4(1,1,1,1);
				
				//float3 particleColor = lerp(baseHue * 0.7, baseHue * 1.3, saturate(1.0 - i.color.y * 2));
				frameBufferColor.rgb *= 0.75; // = lerp(frameBufferColor.rgb, particleColor, 0.25);
				float4 finalColor = GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(0,0,0,0));
				finalColor.a = brushColor.a * 0.2;

				return finalColor;
			}
		ENDCG
		}
	}
}
