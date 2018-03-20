Shader "Brushstrokes/FoodFruitStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet
		_MatureToDecayTex ("_MatureToDecayTex", 2D) = "white" {}
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
			sampler2D _MatureToDecayTex;
			//float4 _MainTex_ST;
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

			struct FruitData {
				int foodIndex;
				float2 worldPos;
				float2 localCoords;
				float2 localScale;
				float attached;  // if attached, sticks to parent food, else, floats in water
			};

			StructuredBuffer<FruitData> fruitDataCBuffer;
			StructuredBuffer<FoodSimData> foodSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				int foodIndex : TEXCOORD2;
				float2 frameLerp : TEXCOORD3;
				float2 quadCoords : TEXCOORD4;
				float4 uvDecay : TEXCOORD5;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
							
				FruitData fruitData = fruitDataCBuffer[inst];
				FoodSimData rawData = foodSimDataCBuffer[fruitData.foodIndex];
								
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				
				//float3 worldPosition = float3(rawData.worldPos, -0.5);  // -25 arbitrary to be visible above floaty bits & BG
				//float3 offset = float3(fruitData.localCoords * rawData.scale * 0.5, 0);
				//worldPosition += offset;

				float growthLerp = saturate(rawData.growth * 3 - 2 + random2 * 1);

				float3 worldPosition = float3(rawData.worldPos, -random2);
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = rawData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				float2 positionOffset = float2(fruitData.localCoords.x * rightAgent * rawData.fullSize.x + fruitData.localCoords.y * forwardAgent * rawData.fullSize.y) * 0.6 * saturate(rawData.growth * 1.46);
				//float2(fruitData.localCoords.x * rawData.scale.x * rightAgent + fruitData.localCoords.y * rawData.scale.y * forwardAgent) * 0.5;
				worldPosition.xy += positionOffset; // Place properly
				
				float3 quadPoint = quadVerticesCBuffer[id];

				float2 velocity = rawData.velocity;

				

				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				//float randomAspect = lerp(_Size.z, _Size.w, random1);
				//float randomAspect = lerp(0.67, 1.33, random1);
				//float randomScale = lerp(_Size.x, _Size.y, random2);
				//float randomValue = rand(float2(inst, randomAspect * 10));
				float randomScale = lerp(0.75, 1.35, random2);
				//float2 scale = float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 50 + 1));
				
				float2 scale = fruitData.localScale * length(rawData.fullSize) * randomScale * saturate(rawData.growth * 2 - 0.3);
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				//float2 forward0 = rawData.heading;
				//float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				//float2 rotatedPoint0 = float2(pointStrokeData.localDir.x * right0 + pointStrokeData.localDir.y * forward0);  // Rotate localRotation by AgentRotation
				// ROTATION:										 quadPoint.z);
				//float rotationAngle = random1 * 10.0 * 3.141592;  // radians
				//float3 rotatedPoint = float3(quadPoint.x * cos(rotationAngle) - quadPoint.y * sin(rotationAngle),
				//							 quadPoint.x * sin(rotationAngle) + quadPoint.y * cos(rotationAngle),
				//							 quadPoint.z);


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
				//float2 forward = normalize(rawData.heading);
				//float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				//float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
				//							 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint1, 0.0f));

				float eaten = 1.0;
				if(random2 > saturate(length(rawData.foodAmount))) {
					eaten = 0.0;
				}
				//float3 hue = rawData.fruitHue * ;
				o.color.rgb = float3(random1, random2, length(rawData.foodAmount));

				if(random1 > length(rawData.foodAmount)) {
					o.color.a = 0;

				}
				else {
					o.color.a = 1;
				}
				//o.color.a = (1.0 - rawData.decay);

				float2 uv0 = quadVerticesCBuffer[id] + 0.5f; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float randBrush = 0; //rawData.fruitBrushType; //pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				const float tilePercentage = (1.0 / 8.0);
				uv0.x *= tilePercentage;  // 8 brushes on texture
				uv0.x += tilePercentage * randBrush;
				uv1.x *= tilePercentage;  // 5 brushes on texture
				uv1.x += tilePercentage * randBrush;

				float2 uvDecay0 = quadVerticesCBuffer[id] + 0.5f;
				float2 uvDecay1 = uvDecay0;
				uvDecay0.x *= tilePercentage;  // 8 brushes on texture
				uvDecay0.x += tilePercentage * randBrush;
				uvDecay1.x *= tilePercentage;  // 5 brushes on texture
				uvDecay1.x += tilePercentage * randBrush;

				// Figure out how much to blur:
				
				float frameLerp = saturate(growthLerp) * 7;
				float decayLerp = (1 - saturate(length(rawData.foodAmount) - random2 * 0.8)) * 7;
				// Can use this for random variations?
				float growRow0 = floor(frameLerp);  // 0-6
				float growRow1 = clamp(ceil(frameLerp), 1, 7); // 1-7

				float decayRow0 = floor(decayLerp);  // 0-6
				float decayRow1 = clamp(ceil(decayLerp), 1, 7); // 1-7
				//float blurLerp = blurMag - blurRow0;
				// calculate UV's to sample from correct rows:

				uv0.y = uv0.y * tilePercentage + tilePercentage * growRow0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * growRow1;

				uvDecay0.y = uvDecay0.y * tilePercentage + tilePercentage * decayRow0;
				uvDecay1.y = uvDecay1.y * tilePercentage + tilePercentage * decayRow1;
				

				// motion blur sampling:
				o.frameLerp = float2(frameLerp - growRow0, decayLerp - decayRow0); //blurLerp;

				o.uv = float4(uv0, uv1);
				o.uvDecay = float4(uvDecay0, uvDecay1);				

				o.foodIndex = fruitData.foodIndex;

				o.quadCoords = quadVerticesCBuffer[id].xy;				

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				FoodSimData rawData = foodSimDataCBuffer[i.foodIndex];

				float4 growTexColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 growTexColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row				
				float4 growBrushColor = lerp(growTexColor0, growTexColor1, i.frameLerp.x);
				float4 finalGrowColor = growBrushColor;

				float distToOrigin = saturate(length(i.quadCoords.xy) * 1.4);
				float2 dir = normalize(i.quadCoords.xy);
				float zDir = 1 - distToOrigin;
				float3 normal = normalize(float3(dir, zDir));

				float3 highlightColor = float3(1,1,1);
				float specular = dot(normal, normalize(float3(0,0,1)));
				
				float3 hue = lerp(rawData.leafHue * growBrushColor.g, float3(0.15, 0.8, 0.28), 0.67);
				hue = lerp(hue, float3(1,1,1), growBrushColor.b); //); // temp flower color use stem color
				hue = lerp(hue, rawData.fruitHue + zDir * 0.25, growBrushColor.r - i.color.y * 0.1);
				finalGrowColor.rgb = hue * saturate(i.color.x * 0.4 + 0.6);
				
				finalGrowColor.a *= i.color.a;
				
				return finalGrowColor;
			}
		ENDCG
		}
	}
}
