Shader "Water/WaterSurfaceBitsDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_NutrientTex ("_NutrientTex", 2D) = "black" {}
		_MainColor ("_MainColor", Color) = (0,1,0,1)
		_DecayColor ("_DecayColor", Color) = (1,1,0,1)
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

			uniform float4 _MainColor;
			uniform float4 _DecayColor;
			
			uniform float _MapSize;

			uniform float _CamDistNormalized;

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

				o.quadUV = quadPoint + 0.5;
				o.worldPos = worldPosition;
				float2 uv = (worldPosition.xy + 128) / 512;
				o.altitudeUV = uv;

				float fadeDuration = 0.22;
				float fadeIn = saturate(waterQuadData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - waterQuadData.age) / fadeDuration);							
				float alpha = fadeIn * fadeOut;
				
				float2 scale = waterQuadData.localScale * alpha * (_CamDistNormalized * 0.75 + 0.25);
				//scale.x *= 0.66;
				//scale.y = scale.y * (1 + saturate(waterQuadData.speed * 64));
				quadPoint *= float3(scale * 3.5, 1.0);
				
				float4 fluidVelocity = tex2Dlod(_VelocityTex, float4(worldPosition.xy / 256, 0, 2));
				float2 fluidDir = waterQuadData.heading; //float2(0,1); //normalize(fluidVelocity.xy);
				//if(length(fluidVelocity) > 0.0000001) {
				//	fluidVelocity.y *= 0.33;
				//	fluidDir = normalize(fluidVelocity.xy);
				//}

				// Wave Surface Height:
				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0));
				float dotLight = dot(waterSurfaceData.yzw, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				float waveHeight = waterSurfaceData.x;

				worldPosition.z -= waveHeight * 2.5 + 1;


				// Figure out final facing Vectors!!!
				float2 forward = fluidDir; //waterQuadData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation
				rotatedPoint.x *= 2.0;
				rotatedPoint.y *= 0.74;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));
				
				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
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
				o.color = float4(rand,waterQuadData.age,1,alpha);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 brushColor = tex2D(_MainTex, i.quadUV);	
				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	
				// use separate camera?

				float4 backgroundColor = float4(0.37, 0.53, 0.26, 1); //frameBufferColor;
				backgroundColor.a = brushColor.a;
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); // i.worldPos.z / 10; // [-1,1] range
				// 0-1 range --> -1 to 1
				altitude = (altitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
								
				backgroundColor.a *= isUnderwater;
				
				float4 waterSurfaceSample = tex2Dlod(_WaterSurfaceTex, float4((i.altitudeUV - 0.25) * 2, 0, 0));
				float3 surfaceNormal = waterSurfaceSample.yzw;
				float diffuse = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				
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
				
				float4 finalColor = backgroundColor; //lerp(reflectedColor, backgroundColor, viewDotRemapped); //saturate(1 - (1 - i.vignetteLerp.x) * 0.5)); //float4(1,1,1,1);
				
				finalColor.rgb = (finalColor.rgb + reflectedColor.rgb * 0.25) * (saturate(diffuse) * 0.5 + 0.5);
				
				finalColor.rgb *= i.color.r * 0.2 + 0.8;

				finalColor.rgb = lerp(finalColor.rgb, _DecayColor.rgb, saturate((i.color.g - 0.6) * 2.5));
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
