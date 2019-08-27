Shader "Water/WaterNutrientsBitsDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_NutrientTex ("_NutrientTex", 2D) = "black" {}
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

			sampler2D _MainTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _SkyTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _NutrientTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;
			uniform float _CamDistNormalized;

			uniform float _AlgaeReservoir;
			uniform float _NutrientDensity;

			struct FrameBufferStrokeData {
				float3 worldPos;
				float2 scale;
				float2 heading;
				int brushType;
			};

			struct WaterQuadData {
				int index;
				float3 worldPos;
				float2 heading;
				float2 localScale;
				float age;
				float speed;
				int brushType;
			};

			StructuredBuffer<WaterQuadData> waterQuadStrokesCBuffer;
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
								
				WaterQuadData waterQuadData = waterQuadStrokesCBuffer[inst];

				float3 worldPosition = waterQuadData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				float rand0 = rand(float2(inst, 0));	
				float densityMask = saturate((_NutrientDensity - rand0) * 100);
				

				o.quadUV = quadPoint + 0.5;
				o.worldPos = worldPosition;
				float2 uv = worldPosition.xy / 256;
				o.altitudeUV = uv;
								
				float2 scale = waterQuadData.localScale * 12;
				//scale.x *= 1;
				//scale.y = scale.y * (1 + saturate(waterQuadData.speed * 64));
				
				//float4 nutrientGridSample = tex2Dlod(_NutrientTex, float4((o.altitudeUV - 0.25) * 2.0, 0, 0));
				//scale *= (nutrientGridSample.x * 0.4 + 0.6) * 1;				
				//scale = float2(1,1) * 0.033;
				_NutrientDensity = 1.0;
				quadPoint *= float3(scale, 1.0); // * (0.2 + _NutrientDensity * 0.175) * (_CamDistNormalized * 0.85 + 0.15);
				
				float4 fluidVelocity = tex2Dlod(_VelocityTex, float4(worldPosition.xy / 256, 0, 2));
				float2 fluidDir = float2(0,1); //normalize(fluidVelocity.xy);
				if(length(fluidVelocity) > 0.0000001) {
					fluidDir = normalize(fluidVelocity.xy);
				}

				// Wave Surface Height:
				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0));
				float dotLight = dot(waterSurfaceData.yzw, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				float waveHeight = waterSurfaceData.x;
				
							
				//worldPosition.z -= waveHeight * 1 - rand0 * 2; // - 1;

				// REFRACTION:
				//float3 offset = worldPosition;				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				float refractionStrength = 1.5 * (rand0 * 0.5 + 0.5);
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;


				// Figure out final facing Vectors!!!
				float2 forward = fluidDir; //waterQuadData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				//worldPosition.z = 1.0;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(quadPoint.xy, 0, 0));


				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //altitudeUV.xy / altitudeUV.w;

				o.skyUV = worldPosition.xy / _MapSize;

				//remnants from water surface bits?
				//float2 rand = Value2D(float2((float)inst, (float)inst + 30), 100);
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

				float fadeDuration = 0.1;
				float fadeIn = saturate(waterQuadData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - waterQuadData.age) / fadeDuration);
							
				float alpha = fadeIn * fadeOut;

				alpha *= densityMask;

				o.color = float4(rand(float2(-0.347 * inst, inst)),(saturate(_NutrientDensity * 1)),1,alpha);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(1,1,1,1);


				float4 finalColor = tex2D(_MainTex, i.quadUV);
				//i.color.r = random 0-1
				finalColor.rgb = lerp(float3(0.05,0.04,0.015), float3(0.9,1,0.7) * 0.5, i.color.r); //rand()); //saturate(nutrientGridSample.x * 10 + 0.033));
				finalColor.rgb = float3(1, 0.85, 0.2) * 1.25;
				//finalColor.rgb *= i.color.y;
				finalColor.a *= i.color.a;				
				return finalColor;

				//return float4(1,1,1,1);
				/*
				float2 screenUV = i.screenUV.xy / i.screenUV.w;

				float distFromScreenCenter = length(screenUV * 2 - 1);
				float vignetteMask = smoothstep(0.5, 2.0, distFromScreenCenter) * 0.8;

				// get view space position of vertex
				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);

				float time = _Time.y;
				float3 waterWorldNormal = normalize(float3(sin(i.worldPos.x * 0.5 + time * 6) * 0.005, 
														   cos(i.worldPos.y * 0.9 + time * 4.2 + 12) * 0.005,
														   -1));  // -Z is UP

				float3 reflectedViewDir = cameraToVertexDir + 2 * waterWorldNormal;

				float3 coords = reflectedViewDir; //normalize(float3(i.altitudeUV, 1));
				float4 cubeMapColor = texCUBE(_Cube, coords);
                
				float4 brushColor = tex2D(_MainTex, i.quadUV);	
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	
				frameBufferColor.a = 1;
				frameBufferColor.rgb += 0.2;
				//return frameBufferColor;
				// use separate camera?
				return frameBufferColor;
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); // [-1,1] range
				float isUnderwater = saturate((-altitude + 0.5) * 10000);

				float val = 1;
				float3 valColor = float3(val, val, val);

				float4 backgroundColor = float4(cubeMapColor.rgb, isUnderwater * brushColor.a);
				backgroundColor.a *= vignetteMask;
				*/

				//return float4(i.color.x * 1000, 0, 0, 1);
				//==================================================================================================================
				/*float4 brushColor = tex2D(_MainTex, i.quadUV);	
				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	
				// use separate camera?


				
				float4 backgroundColor = frameBufferColor;
				backgroundColor.a = brushColor.a;
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); // i.worldPos.z / 10; // [-1,1] range
				// 0-1 range --> -1 to 1
				altitude = (altitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				float strataColorMultiplier = (sin(altitude * (1.0 + i.worldPos.x * 0.01 - i.worldPos.y * -0.01) + i.worldPos.x * 0.01 - i.worldPos.y * 0.01) * 0.5 + 0.5) * 0.5 + 0.5;
				backgroundColor.rgb *= strataColorMultiplier;				
				backgroundColor.rgb = lerp(backgroundColor.rgb, waterFogColor, 1 * (saturate(altitude * 0.8)) + 0.25 * isUnderwater);

				float snowAmount = saturate((-altitude - 0.6) * 2 +
								   ((sin(i.worldPos.x * 0.0785 + i.worldPos.y * 0.02843) * 0.5 + 0.5) * 1 - 
								   (cos(i.worldPos.x * 0.012685 + i.worldPos.y * -0.01843) * 0.5 + 0.5) * 0.9 +
								   (sin(i.worldPos.x * 0.2685 + i.worldPos.y * -0.1843) * 0.5 + 0.5) * 0.45 - 
								   (cos(i.worldPos.x * -0.2843 + i.worldPos.y * 0.01143) * 0.5 + 0.5) * 0.45 +
								   (sin(i.worldPos.x * 0.1685 + i.worldPos.y * -0.03843) * 0.5 + 0.5) * 0.3 - 
								   (cos(i.worldPos.x * -0.1843 + i.worldPos.y * 0.243) * 0.5 + 0.5) * 0.3) * 0.5);
				
				backgroundColor.rgb = lerp(backgroundColor.rgb, float3(0.56, 1, 0.34) * 0.6, snowAmount * 1);
				//==================================================================================================================
				backgroundColor.a *= isUnderwater;

				
				

				float4 waterSurfaceSample = tex2Dlod(_WaterSurfaceTex, float4((i.altitudeUV - 0.25) * 2, 0, 0));
				float3 surfaceNormal = waterSurfaceSample.yzw;
				float diffuse = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				//float refractionStrength = 0.5;
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				//float diffuse = 

				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);
				float3 reflectedViewDir = cameraToVertexDir + 2 * surfaceNormal * 0.05;

				float viewDot = dot(-cameraToVertexDir, surfaceNormal);
				float rangeStart = 0.25;
				float rangeEnd = 1;
				float rangeSize = rangeEnd - rangeStart;

				float viewDotRemapped = saturate((viewDot / rangeSize) - (0.25 / rangeSize));

				float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;

				float4 reflectedColor = float4(tex2Dlod(_SkyTex, float4((skyCoords) - _Time.y * 0.015, 0, 1)).rgb, backgroundColor.a); //col;
				
				float4 finalColor = lerp(reflectedColor, backgroundColor, viewDotRemapped); //saturate(1 - (1 - i.vignetteLerp.x) * 0.5)); //float4(1,1,1,1);
				//finalColor.a *= saturate(i.vignetteLerp.w * 0.55 - 0.25); //(1 - saturate(i.vignetteLerp.x) * 0.4) * 0.5;
				//finalColor.a *= i.color.a;
				//finalColor.rgb *= diffuse;
				//finalColor.rgb *= 0.75;
				//return float4(1,1,1,1);
				//finalColor = float4(reflectedColor.rgb, 1);
				//finalColor.rgb = float3(viewDotRemapped, viewDotRemapped, viewDotRemapped);
				*/
				
				//finalColor.a = (1 - viewDotRemapped) * backgroundColor.a; //viewDotRemapped;
				//finalColor.a *= finalColor.a;
				//finalColor.a *= 0.33;

				//float4 nutrientGridSample = tex2D(_NutrientTex, (i.altitudeUV - 0.25) * 2.0); 
				//finalColor.a = 1;
				//float foodAmt = nutrientGridSample.x * 2;
				//finalColor.rgb = float3(foodAmt, foodAmt, foodAmt);
				
				
			}
		ENDCG
		}
	}
}
