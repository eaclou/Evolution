Shader "Brushstrokes/FoodLeafStrokeDisplayShader"
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

			struct LeafData { // fixed number, but some aren't used (zero scale??)
				int foodIndex;
				float2 worldPos;
				float2 localCoords;
				float2 localScale;
				float attached;  // if attached, sticks to parent food, else, floats in water
			};

			StructuredBuffer<LeafData> leafDataCBuffer;
			StructuredBuffer<FoodSimData> foodSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float4 status : TEXCOORD2; 
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
							
				LeafData leafData = leafDataCBuffer[inst];
				FoodSimData rawData = foodSimDataCBuffer[leafData.foodIndex];
				
				float3 worldPosition = float3(rawData.worldPos, -0.5);
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = rawData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				float2 positionOffset = float2(leafData.localCoords.x * rightAgent * rawData.fullSize.x + leafData.localCoords.y * forwardAgent * rawData.fullSize.y) * 0.7 * saturate(rawData.growth * 1.5) * (1.0 - saturate(rawData.decay * 0.5));
				worldPosition.xy += positionOffset; // Place properly

				float3 quadPoint = quadVerticesCBuffer[id];

				float2 velocity = rawData.velocity;

				float random1 = rand(float2(inst, leafData.foodIndex));
				float random2 = rand(float2(random1, random1));

				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				//float randomAspect = lerp(_Size.z, _Size.w, random1);
				//float randomAspect = lerp(0.67, 1.33, random1);
				//float randomScale = lerp(_Size.x, _Size.y, random2);
				//float randomValue = rand(float2(inst, randomAspect * 10));
				float randomScale = lerp(0.75, 1.35, random2);
				//float2 scale = float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 50 + 1));
				
				float2 scale = length(rawData.fullSize) * saturate((rawData.growth) * 2.2) * randomScale * leafData.localScale * (1.0 - rawData.decay);  // 
				//scale = float2(0.2, 0.2);
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				float rotationAngle = random1 * 10.0 * 3.141592;  // radians
				float2 forward0 = rawData.heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float2 rotatedPoint0 = float2(cos(rotationAngle) * right0 + sin(rotationAngle) * forward0);  // Rotate localRotation by AgentRotation
				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint1, 0.0f));
				float3 leafHue = lerp(rawData.leafHue, float3(0.15, 0.8, 0.28), 0.67);
				o.color = lerp(float4(0.57, 0.56, 0.25, 0.2), float4(leafHue * (random1 * 0.4 + 0.8), 1), saturate((1.0 - rawData.decay * 2 + random1*0.5)));
				o.status = float4(rawData.growth, rawData.decay, rawData.health, length(rawData.foodAmount));

				float2 uv0 = quadVerticesCBuffer[id] + 0.5f; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float randBrush = rawData.leafBrushType; //pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
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

				uv0.y = uv0.y * tilePercentage + tilePercentage * blurRow0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * blurRow1;
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
				
				float alpha = brushColor.a;
				if(alpha < i.status.y) {
					alpha = 0;
				}

				float4 finalColor = float4(i.color) * brushColor;
				finalColor.a = alpha;
				finalColor.rgb *= 0.25;
				return finalColor;
			}
		ENDCG
		}
	}
}
