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

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
							
				AnimalParticleData particleData = animalParticleDataCBuffer[inst];

				float3 worldPosition = float3(particleData.worldPos);
				
				quadPoint = quadPoint * particleData.radius * (1.0 - particleData.digestedAmount) * 4 * particleData.isActive;

				worldPosition = worldPosition + quadPoint;

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				float2 altUV = (worldPosition.xy + 128) / 512;				
				
				worldPosition.z = -(tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 2)).x * 2 - 1) * 10;
								
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));			
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				float4 texColor = tex2D(_MainTex, i.uv);	
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				texColor.rgb = waterFogColor;  // shadow

				return texColor;
				//return float4(0.7,1,0.1,texColor.a * 0.75);
			}
		ENDCG
		}
	}
}
