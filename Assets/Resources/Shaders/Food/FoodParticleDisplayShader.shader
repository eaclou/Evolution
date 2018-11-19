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

			sampler2D _MainTex;
			sampler2D _WaterSurfaceTex;
			
			//float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			
			struct FoodParticleData {
				int index;				
				int critterIndex; // index of creature which swallowed this foodParticle
				int nearestCritterIndex;
				float isSwallowed;   // 0 = normal, 1 = in critter's belly
				float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk      
				float2 worldPos;
				float radius;
				float foodAmount;
				float active;
				float refactoryAge;
			};

			StructuredBuffer<FoodParticleData> foodParticleDataCBuffer;			
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
							
				FoodParticleData particleData = foodParticleDataCBuffer[inst];

				float3 worldPosition = float3(particleData.worldPos, 1.0);    //float3(rawData.worldPos, -random2);
				
				quadPoint = quadPoint * particleData.radius * (1.0 - particleData.digestedAmount) * 0.7; // * particleData.active; // *** remove * 3 after!!!
				worldPosition = worldPosition + quadPoint;

				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;				
				float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;


				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;	

				o.color = float4((float)particleData.nearestCritterIndex / 64.0, particleData.active, particleData.refactoryAge, particleData.digestedAmount);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);
				
				float val = i.color.a;
				
				float4 finalColor = float4(float3(i.color.y, i.color.z, i.color.w), 1); //texColor.a);
				finalColor = float4(0.25, 1, 0.36, texColor.a);
				return finalColor;
			}
		ENDCG
		}
	}
}
