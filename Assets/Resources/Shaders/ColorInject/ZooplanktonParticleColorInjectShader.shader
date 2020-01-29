Shader "Zooplankton/ZooplanktonParticleColorInjectShader"
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
			#include "Assets/Resources/Shaders/Inc/StructsAnimalParticles.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			
			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float3 moundUV : TEXCOORD1;
				float2 altitudeUV : TEXCOORD2;
				float4 color : COLOR;
				float4 hue : TEXCOORD3;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];

				int particleIndex = inst;								

				float rand0 = rand(float2(inst, inst) * 10);
				float rand1 = rand(float2(rand0, rand0) * 10);
				float rand2 = rand(float2(rand1, rand1) * 10);				
							
				AnimalParticleData particleData = animalParticleDataCBuffer[particleIndex];

				float3 worldPosition = float3(particleData.worldPos);    //float3(rawData.worldPos, -random2);
				float radius = 2.5; //(0.1 + saturate(particleData.biomass * 2.2) + rand2 * 0.4) * particleData.isActive * 2.8;
				worldPosition.xy += quadPoint.xy * radius;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));			
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;
				o.color = float4(saturate(particleData.isDecaying), 0, rand2, 1 - saturate(particleData.isDecaying));
				o.hue = float4(1,1,1, particleData.isActive);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);
				
				return float4(1,1,1,i.hue.a);
			}
		ENDCG
		}
	}
}
