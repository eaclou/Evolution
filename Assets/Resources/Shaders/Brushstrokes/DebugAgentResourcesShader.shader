Shader "Brushstrokes/DebugAgentResourcesShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		//_PatternTex ("Pattern Texture", 2D) = "white" {}
		//_BumpMap("Normal Map", 2D) = "bump" {}		
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
			
			struct AgentSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float2 size;
				float3 primaryHue;  // can eventually pull these static variables out of here to avoid per-frame updates on non-dynamic attributes
				float3 secondaryHue;
				float maturity;
				float decay;
				float eatingStatus;
				float foodAmount;
			};

			struct DebugBodyResourcesData {
				float developmentPercentage;
				float health;
				float energy;
				float stamina;
				float stomachContents;
				float2 mouthDimensions;
				float mouthOffset;
				float isBiting;
				float isDamageFrame;        
			};

			StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			//StructuredBuffer<int> indexCBuffer;
			StructuredBuffer<DebugBodyResourcesData> debugAgentResourcesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 healthStaminaEnergyFood : TEXCOORD1;
				float4 bitingDamageGrowthMisc : TEXCOORD2;
				//float3 worldPos : TEXCOORD2;
				//int2 bufferIndices : TEXCOORD3;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				//int agentIndex = indexCBuffer[inst];
				AgentSimData agentSimData = agentSimDataCBuffer[inst];
				DebugBodyResourcesData debugData = debugAgentResourcesCBuffer[inst];
				//o.bufferIndices = int2(inst, agentIndex);
								
				float3 worldPosition = float3(agentSimData.worldPos, -0.5);
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = agentSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				float2 positionOffset = float2(0, 0); // float2(agentSimData.size.x * rightAgent + agentSimData.size.y * forwardAgent) * 0.5;
				worldPosition.xy += positionOffset; // Place properly
								
				float3 quadPoint = quadVerticesCBuffer[id];
				float2 quadUV = quadVerticesCBuffer[id].xy + 0.5;				
				
				float2 scale = agentSimData.size * agentSimData.maturity;  // assume bodyStroke is 1:1 agentSize
				
				quadPoint *= float3(scale, 1.0);

				float3 rotatedPoint1 = float3(quadPoint.x * rightAgent + quadPoint.y * forwardAgent,
											 quadPoint.z);

				float alpha = 1;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint1, 0.0));
				//o.worldPos = worldPosition + rotatedPoint1;
				o.healthStaminaEnergyFood = float4(debugData.health, debugData.stamina, debugData.energy, debugData.stomachContents);
				o.bitingDamageGrowthMisc = float4(debugData.isBiting, debugData.isDamageFrame, debugData.developmentPercentage, 1);
				o.uv = quadUV;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				//AgentSimData agentSimData = agentSimDataCBuffer[i.bufferIndices.y];
								
				float4 finalColor = float4(1,1,1,1); // * i.color;
				
				float health = i.healthStaminaEnergyFood.x;
				float3 healthColor0 = float3(1,0,0);
				float3 healthColor1 = float3(0,1,0);
				finalColor.xyz = lerp(healthColor0, healthColor1, health);

				//if(i.uv.y < 0.75) {
				//	float stamina = i.healthStaminaEnergyFood.y;
				//	finalColor.xyz = float3(stamina,stamina,0);
				//}
				if(i.uv.y < 0.75) {
					float energy = i.healthStaminaEnergyFood.z;
					finalColor.xyz = float3(0,energy,energy);
				}
				if(i.uv.y < 0.25) {
					float food = i.healthStaminaEnergyFood.w;
					finalColor.xyz = float3(0,food,0);
				}

				if(i.uv.x < 0.33) {
					finalColor.xyz = float3(i.bitingDamageGrowthMisc.x * 0.75, i.bitingDamageGrowthMisc.x * 0.75, 0.15);
				}
				if(i.uv.x < 0.25) {
					finalColor.xyz = float3(i.bitingDamageGrowthMisc.y, 0, 0);
				}
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
