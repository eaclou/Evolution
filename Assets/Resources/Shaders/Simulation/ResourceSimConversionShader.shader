Shader "Simulation/ResourceSimConversionShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsAnimalParticles.cginc"

			sampler2D _MainTex;
			
			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform float _MapSize;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture					
				float2 altitudeUV : TEXCOORD1;
				float4 color : COLOR;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];

				int particleIndex = inst;
							
				AnimalParticleData particleData = animalParticleDataCBuffer[particleIndex];

				float3 worldPosition = float3(particleData.worldPos.xy, 0);   
				
				float radius = 2;
				worldPosition.xy += quadPoint.xy * radius;
				
				//worldPosition
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));			
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
				o.altitudeUV = 	particleData.worldPos.xy / _MapSize;			
				o.color = float4(particleData.age, particleData.algaeConsumed, 1, particleData.isActive);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(saturate(i.color.x * 1),1,1,i.color.a);


			}
			ENDCG
		}
	}
}
