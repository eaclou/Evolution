Shader "Brushstrokes/FoodStemStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			struct FoodSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float2 scale;
				float3 foodAmount;
				float growth;
				float decay;
				float health;
				int stemBrushType;
				int leafBrushType;
				int fruitBrushType;
				float3 stemHue;
				float3 leafHue;
				float3 fruitHue;
			};

			struct StemData {  // Only the main trunk for now!!! worry about other ones later!!! SIMPLIFY!!!
				int foodIndex;
				float2 localBaseCoords;  // main trunk is always (0, -1f) --> (0f, 1f), secondary stems need to start with x=0 (to be on main trunk)
				float2 localTipCoords;  // scaled with main Food scale
				float width; // thickness of branch
				float childGrowth; // for future use:  // 0-1, 1 means fully mature childFood attached to this, 0 means empty end  
				float attached;
			};

			StructuredBuffer<StemData> stemDataCBuffer;
			StructuredBuffer<FoodSimData> foodSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				FoodSimData rawData = foodSimDataCBuffer[inst];
				float3 worldPosition = float3(rawData.worldPos, -0.25);  // -25 arbitrary to be visible above floaty bits & BG
				float3 quadPoint = quadVerticesCBuffer[id];

				float2 velocity = rawData.velocity;

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				//float randomAspect = lerp(_Size.z, _Size.w, random1);
				//float randomAspect = lerp(0.67, 1.33, random1);
				//float randomScale = lerp(_Size.x, _Size.y, random2);
				//float randomValue = rand(float2(inst, randomAspect * 10));
				//float randomScale = lerp(0.033, 0.09, random2);
				//float2 scale = float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 50 + 1));
				float2 scale = float2(1, 1) * rawData.scale;
				quadPoint *= float3(scale, 1.0);

				// ROTATION:										 quadPoint.z);
				float2 forward = normalize(rawData.heading);
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
											 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				o.color = float4(rawData.leafHue, saturate(1.0 - rawData.decay));
				o.uv = quadVerticesCBuffer[id] + 0.5f;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				float4 finalColor = float4(i.color) * texColor; // * float4(0.5, 1, 0.5, 1); //texColor * _Tint * float4(i.color, 1);
				return finalColor;
				
			}
		ENDCG
		}
	}
}
