Shader "Unlit/CritterHighlightTrailShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
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
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;

			StructuredBuffer<float2> highlightTrailDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;				
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;

				//int particleIndex = floor((float)inst / 128.0);
				
				//float rand0 = rand(float2(inst, inst) * 10);
				//float rand1 = rand(float2(rand0, rand0) * 10);
				//float rand2 = rand(float2(rand1, rand1) * 10);
				
							
				//AlgaeParticleData particleData = foodParticleDataCBuffer[particleIndex];

				float3 worldPosition = float3(highlightTrailDataCBuffer[inst], 1.0);    //float3(rawData.worldPos, -random2);
				
				/*float2 offsetRaw = (float2(rand0, rand1) * 2 - 1);				
				float2 offset = offsetRaw * (16 * particleData.biomass + 0.2) * 0.66;
				
				float masterFreq = 1;
				float spatialFreq = 0.55285;
				float timeMult = 0.013;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.520;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;
				*/
				//float2 forward = normalize((float2(rand1, rand2) * 2 - 1));
				//float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				//float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward, quadPoint.z);  // Rotate localRotation by AgentRotation
				
				
				worldPosition = worldPosition + quadPoint * 0.2; // rotatedPoint * particleData.isActive;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));				
					
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				float4 texColor = tex2D(_MainTex, i.uv);	
				texColor.a *= 0.25;
				return texColor;
			}

			
			ENDCG
		}
	}
}
