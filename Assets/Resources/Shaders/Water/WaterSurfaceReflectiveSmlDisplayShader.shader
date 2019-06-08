Shader "Water/WaterSurfaceReflectiveSmlDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_ResourceTex ("_ResourceTex", 2D) = "black" {}
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
			sampler2D _SkyTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _ResourceTex;
			sampler2D _WaterColorTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;
			uniform float _CamDistNormalized;
			uniform float4 _CamFocusPosition;

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

			#include "Assets/Resources/Shaders/Inc/WaterUtilityFunctions.cginc"

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				WaterQuadData waterQuadData = waterQuadStrokesCBuffer[inst];

				float3 worldPosition = waterQuadData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				o.quadUV = quadPoint + 0.5;
				o.worldPos = worldPosition;
				float2 uv = worldPosition.xy / 256;
				o.altitudeUV = uv;
				
				float2 scale = waterQuadData.localScale * 32;
				//scale.x *= 1;
				//scale.y = scale.y * (1 + saturate(waterQuadData.speed * 48));
				quadPoint *= float3(scale, 1.0);
				
				float4 fluidVelocity = tex2Dlod(_VelocityTex, float4(worldPosition.xy / _MapSize, 0, 1));
				float2 fluidDir = waterQuadData.heading; //float2(1,0); //normalize(fluidVelocity.xy);
				//if(length(fluidVelocity) > 0.0000001) {
				//	fluidVelocity.y *= 0.35;
				//	fluidDir = normalize(fluidVelocity.xy);
				//}

				// Wave Surface Height:
				// Water Surface:
				float4 waterSurfaceData = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0));
				float3 surfaceNormal = waterSurfaceData.yzw;
				float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
				float waveHeight = waterSurfaceData.x;

				worldPosition.z -= waveHeight * 2.5 + 1;  // STANDARDIZE!

				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //altitudeUV.xy / altitudeUV.w;

				float2 sampleUV = screenUV.xy / screenUV.w;
				float vignetteRadius = length((sampleUV - 0.5) * 2);

				float3 cameraToVertex = worldPosition - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);				
				float viewDot = dot(-cameraToVertexDir, surfaceNormal);
				float reflectLerp = GetReflectionLerpSml(worldPosition, surfaceNormal, viewDot, _CamDistNormalized * 0.25 + 0.2, _CamFocusPosition, vignetteRadius);

				o.skyUV = worldPosition.xy / _MapSize;

				o.vignetteLerp = float4(reflectLerp,0,0,0); //float4(testNewVignetteMask,sampleUV,saturate(vignetteRadius));

				float fadeDuration = 0.25;
				float fadeIn = saturate(waterQuadData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - waterQuadData.age) / fadeDuration);
							
				float alpha = fadeIn * fadeOut;
				
				quadPoint *= alpha * (_CamDistNormalized * 0.4 + 0.5) * saturate(reflectLerp * 6);
				
				// Figure out final facing Vectors!!!
				float2 forward = fluidDir; //waterQuadData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));

				o.color = float4(1,1,1,alpha);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 brushColor = tex2D(_MainTex, i.quadUV);	
				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	
				// use separate camera?

				//float distFromScreenCenter = length(screenUV * 2 - 1);
				
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				float4 resourceTex = tex2D(_ResourceTex, i.altitudeUV);

				float4 finalColor = GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, resourceTex);
				finalColor.a = brushColor.a;

				float3 surfaceNormal = waterSurfaceTex.yzw;
				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);
				float3 reflectedViewDir = cameraToVertexDir + 2 * surfaceNormal * 0.05;
				//float viewDot = dot(-cameraToVertexDir, surfaceNormal);

				float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;

				float4 reflectedColor = float4(tex2Dlod(_SkyTex, float4((skyCoords) - _Time.y * 0.015, 0, 1)).rgb, finalColor.a); //col;
								
				float reflectLerp = saturate(i.vignetteLerp.x * 2);
				finalColor = lerp(finalColor, reflectedColor, reflectLerp);
				finalColor.a *= saturate(reflectLerp * 6) * 0.7;
				
				//return float4(1,1,0,1);
				return finalColor;

			}
		ENDCG
		}
	}
}
