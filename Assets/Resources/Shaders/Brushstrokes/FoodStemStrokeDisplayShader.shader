﻿Shader "Brushstrokes/FoodStemStrokeDisplayShader"
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
				float2 fullSize;
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
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
							
				StemData stemData = stemDataCBuffer[inst];
				FoodSimData rawData = foodSimDataCBuffer[stemData.foodIndex];
				
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
				
				float2 scale = float2(stemData.width, 1) * rawData.fullSize * saturate(rawData.growth * 2) * (1.0 - rawData.decay);
				quadPoint *= float3(scale, 1.0);

				// ROTATION:										 quadPoint.z);
				float2 forward = normalize(rawData.heading);
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
											 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				o.color = float4(lerp(rawData.stemHue, float3(0.45,0.44,0.35)*1 + random2*0.25, 0.72), saturate(1.0 - rawData.decay));

				float2 uv0 = quadVerticesCBuffer[id] + 0.5f; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float randBrush = rawData.stemBrushType; //pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				const float tilePercentage = (1.0 / 8.0);
				uv0.x *= tilePercentage;  // 8 brushes on texture
				uv0.x += tilePercentage * randBrush;
				uv1.x *= tilePercentage;  // 5 brushes on texture
				uv1.x += tilePercentage * randBrush;

				// Figure out how much to blur:
				//float blurMag = saturate(length(agentSimData.velocity)) * 2.99;	
				// Can use this for random variations?
				float blurRow0 = 0; //floor(blurMag);  // 0-2
				float blurRow1 = 1; //ceil(blurMag); // 1-3
				//float blurLerp = blurMag - blurRow0;
				// calculate UV's to sample from correct rows:

				uv0.y = uv0.y * 0.25 + 0.25 * blurRow0;
				uv1.y = uv1.y * 0.25 + 0.25 * blurRow1;
				// motion blur sampling:
				//o.motionBlurLerp = 0; //blurLerp;

				o.uv = float4(uv0, uv1);
				//o.uv = quadVerticesCBuffer[id] + 0.5f;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = lerp(texColor0, texColor1, 0.0);

				float4 finalColor = float4(i.color) * brushColor;
				finalColor.rgb *= 0.25;
				return finalColor;
			}
		ENDCG
		}
	}
}
