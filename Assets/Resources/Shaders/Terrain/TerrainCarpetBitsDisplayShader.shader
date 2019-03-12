Shader "Terrain/TerrainCarpetBitsDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_NutrientTex ("_NutrientTex", 2D) = "black" {}
		_WaterColorTex ("_WaterColorTex", 2D) = "black" {}
	}
	SubShader
	{		
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _NutrientTex;
			sampler2D _WaterColorTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;

			uniform float _CamDistNormalized;

			struct GroundBitsData
			{
				int index;
				float3 worldPos;
				float2 heading;
				float2 localScale;
				float age;
				float speed;
				float noiseVal;
				int brushType;
			};

			StructuredBuffer<GroundBitsData> groundBitsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 quadUV : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float2 altitudeUV : TEXCOORD1;
				float4 screenUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 vignetteLerp : TEXCOORD4;
				float2 skyUV : TEXCOORD5;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				GroundBitsData groundBitData = groundBitsCBuffer[inst];

				float3 worldPosition = groundBitData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				o.quadUV = quadPoint + 0.5;
				o.worldPos = worldPosition;
				float2 uv = (worldPosition.xy + 128) / 512;
				o.altitudeUV = uv;

				float altitude = tex2Dlod(_AltitudeTex, float4(o.altitudeUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(((o.altitudeUV - 0.25) * 2), 0, 0)).yzw;
				float depth = saturate(-altitude + 0.5);
				float refractionStrength = depth * 8.5;

				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				worldPosition.z = -altitude * 20 + 10;

				float fadeDuration = 0.1;
				float fadeIn = saturate(groundBitData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - groundBitData.age) / fadeDuration);							
				float alpha = fadeIn * fadeOut;
				
				float2 scale = groundBitData.localScale * alpha * (_CamDistNormalized * 0.75 + 0.25);
			
				quadPoint *= float3(scale, 1.0);
				
				float4 fluidVelocity = tex2Dlod(_VelocityTex, float4(worldPosition.xy / 256, 0, 2));
				float2 fluidDir = float2(0,1); //normalize(fluidVelocity.xy);
				if(length(fluidVelocity) > 0.0000001) {
					fluidDir = normalize(fluidVelocity.xy);
				}

				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0));
				float dotLight = dot(waterSurfaceData.yzw, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				//float waveHeight = waterSurfaceData.x;

				//worldPosition.z -= waveHeight * 2.5;


				// Figure out final facing Vectors!!!
				float2 forward = fluidDir; //waterQuadData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));


				float4 pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0)); //mul(UNITY_MATRIX_VP, float4(worldPosition + quadVerticesCBuffer[id] * 1, 1.0)); // *** Revisit to better understand!!!! ***
				o.pos = pos;
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //altitudeUV.xy / altitudeUV.w;

				o.skyUV = worldPosition.xy / _MapSize;

				
				float randNoise1 = Value3D(float3(worldPosition.x - _Time.y * 5.34, worldPosition.y + _Time.y * 7.1, _Time.y * 15), 0.1).x * 0.5 + 0.5; //				
				float randNoise2 = Value3D(float3(worldPosition.x + _Time.y * 7.34, worldPosition.y - _Time.y * 6.1, _Time.y * -10), 0.25).x * 0.5 + 0.5;
				float randNoise3 = Value3D(float3(worldPosition.x + _Time.y * 3.34, worldPosition.y - _Time.y * 5.1, _Time.y * 7.5), 0.36).x * 0.5 + 0.5;
				float randNoise4 = Value3D(float3(worldPosition.x - _Time.y * 8.34, worldPosition.y + _Time.y * 4.1, _Time.y * -3.5),0.55).x * 0.5 + 0.5;
				randNoise1 *= 1;
				randNoise2 *= 1;
				randNoise3 *= 1;
				randNoise4 *= 1;
				float randThreshold = (randNoise1 + randNoise2 + randNoise3 + randNoise4) / 4;	
				float2 sampleUV = screenUV.xy / screenUV.w;
				float vignetteRadius = length((sampleUV - 0.5) * 2);
				float testNewVignetteMask = saturate(((randThreshold + 0.6 - (saturate(vignetteRadius) * 0.4 + 0.3)) * 2));
				o.vignetteLerp = float4(testNewVignetteMask,sampleUV,saturate(vignetteRadius));

				
				float rand = Value2D(float2((float)inst, (float)inst + 30), 100).x;
				o.color = float4(rand,1,1,alpha);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 brushColor = tex2D(_MainTex, i.quadUV);	
				
				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source					
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2);
				float4 waterColorTex = tex2D(_WaterColorTex, (i.altitudeUV - 0.25) * 2);
				
				float4 finalColor = GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, waterColorTex);
				finalColor.a = brushColor.a;
				return finalColor;

				return finalColor;
				
			}
		ENDCG
		}
	}
}
