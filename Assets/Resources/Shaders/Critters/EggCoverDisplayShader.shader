Shader "Critter/EggCoverDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}  // stem texture sheet		
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
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
			//#include "Assets/Resources/Shaders/Inc/StructsEggData.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			sampler2D _WaterSurfaceTex;
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;			
			//StructuredBuffer<EggSackSimData> eggSackSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			uniform float _MapSize;
			uniform float _MaxAltitude;
			uniform float _GlobalWaterLevel;
			uniform float _CamDistNormalized;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				//int foodIndex : TEXCOORD2;
				float frameLerp : TEXCOORD3;
				float2 quadCoords : TEXCOORD4;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			
			float length_squared(float2 v, float2 w) {
				float2 r = w - v;
				float distSqr = (r.x * r.x + r.y * r.y);
				return distSqr;
			}

			float minimum_distance(float2 v, float2 w, float2 p) {
			  // Return minimum distance between line segment vw and point p
			  const float l2 = length_squared(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
			  if (l2 == 0.0) {
				return length(v - p);   // v == w case
			  }
			  // Consider the line extending the segment, parameterized as v + t (w - v).
			  // We find projection of point p onto the line. 
			  // It falls where t = [(p-v) . (w-v)] / |w-v|^2
			  // We clamp t from [0,1] to handle points outside the segment vw.
			  const float t = max(0, min(1, dot(p - v, w - v) / l2));
			  const float2 projection = v + t * (w - v);  // Projection falls on the segment
			  return distance(p, projection);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				
				CritterInitData critterInitData = critterInitDataCBuffer[inst];
				CritterSimData critterSimData = critterSimDataCBuffer[inst];

				float3 worldPosition = critterSimData.worldPos;
				worldPosition.y += 0.5;
				worldPosition.z -= 0.5;
														
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				//float randomScale = lerp(0.86, 1, random2);	
				
				float scale = critterSimData.growthPercentage * (critterInitData.boundingBoxSize.x + critterInitData.boundingBoxSize.y) * 4.5 + 0.001; // length(eggData.localScale) * length(rawData.fullSize) * saturate(rawData.growth * 1.5f) * randomScale * (1.0 + rawData.decay);
				scale *= _CamDistNormalized;
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = critterSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
							
				//quadPoint *= scale * 0.5; // * ((1.0 - rawData.decay) * 0.75 + 0.25);
				
				float eggMask = 1.0 - saturate(saturate(critterSimData.embryoPercentage - 0.995) * 1000);
				//quadPoint *= eggMask;
				
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint = float3(quadPoint.x * rightAgent + quadPoint.y * forwardAgent,
											 quadPoint.z);

				// REFRACTION:		
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0)).yzw;
				float refractionStrength = 2.45;
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(quadPoint * lerp(0.1, 2.5, _CamDistNormalized), 0.0));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0.0));
				
				float2 uv0 = quadVerticesCBuffer[id] + 0.5; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float randBrush = 1; //fmod((float)rawData.brushType, 2); //rawData.fruitBrushType; //pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				const float tilePercentage = (1.0 / 8.0);
				uv0.x *= tilePercentage;  // 8 brushes on texture
				uv0.x += tilePercentage * randBrush;
				uv1.x *= tilePercentage;  // 5 brushes on texture
				uv1.x += tilePercentage * randBrush;

				// Figure out how much to blur:				
				float frameLerp = (critterSimData.growthPercentage * 0.5 + (1.0 - critterSimData.decayPercentage) * 0.5) * 7; //saturate(growthLerp) * 3;
			
				// Can use this for random variations?
				float row0 = 1; //floor(frameLerp);  // 0-6
				float row1 = 2; //clamp(ceil(frameLerp), 1, 7); // 1-7
				
				uv0.y = uv0.y * tilePercentage + tilePercentage * row0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * row1;

				float3 primaryHue = critterInitDataCBuffer[inst].primaryHue;
				float3 secondaryHue = critterInitDataCBuffer[inst].secondaryHue;

				// *****
				random2 = 0;
				o.color = float4(lerp(primaryHue, secondaryHue, random2), 1); 
				o.frameLerp = frameLerp - row0;
				o.uv = float4(uv0, uv1);
				o.quadCoords = quadVerticesCBuffer[id].xy;	
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{	
				//return float4(1,1,0.25,1);

				///////////////////////////////////

				float4 growTexColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 growTexColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row				
				float4 growBrushColor = lerp(growTexColor0, growTexColor1, 0); //i.frameLerp.x);
				float4 finalGrowColor = growBrushColor;

				float distToOrigin = saturate(length(i.quadCoords.xy + float2(0.02, 0.03)) * 1.4);
				float2 dir = normalize(i.quadCoords.xy);
				float zDir = 1 - distToOrigin;
				float3 normal = normalize(float3(dir, zDir));

				float3 highlightColor = float3(1,1,1);
				float specular = dot(normal, normalize(float3(0,0,1)));
								
				finalGrowColor.rgb = i.color.rgb;
				
				finalGrowColor.a *= growBrushColor.r * i.color.a;

				
				return finalGrowColor;
			}
		ENDCG
		}
	}
}
