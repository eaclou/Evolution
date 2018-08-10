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
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			
			/*struct AgentSimData {
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
			*/

			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float4 critterResources : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];				
				float2 quadUV = quadPoint.xy + 0.5;

				CritterInitData critterInitData = critterInitDataCBuffer[inst];
				CritterSimData critterSimData = critterSimDataCBuffer[inst];							
				
				float scale = 2.0 * ((critterInitData.boundingBoxSize.x + critterInitData.boundingBoxSize.y) / 2.0) * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);
				quadPoint *= scale;
				quadPoint.y *= 0.25;

				float3 worldPosition = critterSimData.worldPos;
				worldPosition.z -= 1.0;  // display above creature's head

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(quadPoint, 0.0));
				o.uv = quadUV;

				o.critterResources = float4(critterSimData.health, critterSimData.energy, critterSimData.foodAmount, critterSimData.growthPercentage);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{	
				float4 finalColor = float4(1,1,1,1);
						// Growth?
				//finalColor.rgb = lerp(float3(0,0,0), float3(1,1,1), i.critterResources.w);
				float3 growthBarColor = float3(1,1,1);
				finalColor.rgb = lerp(growthBarColor, float3(0,0,0), saturate((i.uv.x - i.critterResources.w) * 100));

				if(i.uv.y > 0.25) {  // STOMACH
					float3 stomachBarColor = float3(1,1,0.25);
					finalColor.rgb = lerp(stomachBarColor, float3(0,0,0), saturate((i.uv.x - i.critterResources.z) * 100));
				}
				if(i.uv.y > 0.5) {  // ENERGY
					float energyDanger = saturate((1.0 - i.critterResources.y) - 0.8) * 5;
					float3 energyBarColor = lerp(float3(0,1,1), float3(1,0,0), energyDanger);
					finalColor.rgb = lerp(energyBarColor, float3(0,0,0), saturate((i.uv.x - i.critterResources.y) * 100));
				}
				if(i.uv.y > 0.75) {  // HEALTH
					float healthDanger = saturate((1.0 - i.critterResources.x) - 0.5) * 2;
					float3 healthBarColor = lerp(float3(0,1,0.25), float3(1,0.2,0), healthDanger);
					finalColor.rgb = lerp(healthBarColor, float3(0,0,0), saturate((i.uv.x - i.critterResources.x) * 100));
					//finalColor.rgb = lerp(float3(1,0.25,0.25), float3(0.25,1,0.25), i.critterResources.x);
				}

				return finalColor;
				
			}
		ENDCG
		}
	}
}
