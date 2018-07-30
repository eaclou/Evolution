﻿Shader "Brushstrokes/FoodParticleDisplayShader"
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

			sampler2D _MainTex;
			sampler2D _WaterSurfaceTex;
			
			//float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			
			struct FoodParticleData {
				int index;
				float2 worldPos;
				float radius;
				float foodAmount;
				float active;
			};

			StructuredBuffer<FoodParticleData> foodParticleDataCBuffer;			
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
							
				FoodParticleData particleData = foodParticleDataCBuffer[inst];

				float3 worldPosition = float3(particleData.worldPos, 0);    //float3(rawData.worldPos, -random2);
				
				quadPoint = quadPoint * particleData.radius * particleData.active;
				worldPosition = worldPosition + quadPoint;

				// REFRACTION:
				//float altitude = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				//float depth = saturate(-altitude + 0.5);
				float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				float4 texColor = tex2D(_MainTex, i.uv);
				
				return float4(0.7,1,0.1,texColor.a * 0.75);
			}
		ENDCG
		}
	}
}