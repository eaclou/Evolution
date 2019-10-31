Shader "Terrain/TerrainGroundStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_ResourceGridTex ("_ResourceGridTex", 2D) = "black" {}
		_TerrainColorTex ("_TerrainColorTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
	}
	SubShader
	{		
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		//ZTest Always
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _ResourceGridTex;
			sampler2D _TerrainColorTex;
			sampler2D _SkyTex;
			
			//sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			uniform float _MapSize;			
			
			struct FrameBufferStrokeData {
				float3 worldPos;
				float2 scale;
				float2 heading;
				int brushType;
			};

			StructuredBuffer<FrameBufferStrokeData> frameBufferStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				//float4 screenUV : TEXCOORD1;
				float2 altitudeUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				// New code:
				// Brushstrokes here vary in:  (size, position, color)
				// Simulated separately (within ComputeTerrain) -- rendered here				
				FrameBufferStrokeData strokeData = frameBufferStrokesCBuffer[inst];

				float3 worldPosition = strokeData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				o.worldPos = worldPosition;

				float2 altUV = worldPosition.xy / _MapSize;
				o.altitudeUV = altUV;

				float altitude = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceSample = tex2Dlod(_WaterSurfaceTex, float4(altUV, 0, 0));
				float3 surfaceNormal = waterSurfaceSample.yzw;
				float depth = saturate(-altitude + 0.5);
				float refractionStrength = depth * 7.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomAspect = lerp(0.75, 1.33, random1);
				
				float2 scale = strokeData.scale * randomAspect * 1.0;
				quadPoint *= float3(scale, 1.0);

				// &&&& Screen-space UV of center of brushstroke:
				// Magic to get proper UV's for sampling from GBuffers:
				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				//float4 screenUV = ComputeScreenPos(pos);
				//o.screenUV = screenUV; //centerUV.xy / centerUV.w;

				// Figure out final facing Vectors!!!
				float2 forward = strokeData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				worldPosition.z = min(worldPosition.z, waterSurfaceSample.x * 1);

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));				
				o.uv = quadVerticesCBuffer[id] + 0.5f; // full texture
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				float4 brushColor = tex2D(_MainTex, i.uv);	
				
				//float2 screenUV = i.screenUV.xy / i.screenUV.w;
				//float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source					
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				float4 resourceTex = tex2D(_ResourceGridTex, i.altitudeUV);	
				float4 terrainColorTex = tex2D(_TerrainColorTex, i.altitudeUV);	
				
				
				//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


				float4 outColor = float4(0,0,0,1);

				float turbidity = _Turbidity;  
				float causticsStrength = lerp(0.025, 0.275, 0.6); //_Turbidity);
				float minFog = 1;
	
				float3 decomposerHue = float3(0.8,0.3,0);
				float decomposerMask = saturate(resourceTex.z * 1) * 0.8;
				float3 detritusHue = float3(0.2,0.1,0.02);
				float detritusMask = saturate(resourceTex.y * 1) * 0.8;
	
				outColor.rgb = lerp(terrainColorTex.rgb, decomposerHue, decomposerMask);
				outColor.rgb = lerp(outColor.rgb, detritusHue, detritusMask);

				float algaeMask = saturate(resourceTex.w * 1.0);
	
				float altitudeRaw = altitudeTex.x;
	
				float3 waterFogColor = float3(0.36, 0.4, 0.44) * 0.42; // _FogColor.rgb;
	
				// FAKE CAUSTICS:::
				float3 surfaceNormal = waterSurfaceTex.yzw; // pre-calculated
				float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
	
				float altitude = altitudeRaw + waterSurfaceTex.x * 0.05;
				float depthNormalized = saturate((1.0 - altitude) - 0.5) * 2;	
				depthNormalized = saturate(depthNormalized); //  ????
				float isUnderwater = saturate((altitude * 2 - 1) * -11);

				// Wetness darkening:
				float wetnessMask = saturate(((altitudeRaw + waterSurfaceTex.x * 0.34) - 0.6) * 5.25);
				outColor.rgb *= (0.6 + wetnessMask * 0.4);
	
				// Caustics
				outColor.rgb += dotLight * isUnderwater * (1.0 - depthNormalized) * causticsStrength;		
	
				//Diffuse 
				float3 sunDir = normalize(float3(1,1,-1));
				float3 waterSurfaceNormal = waterSurfaceTex.yzw;
	
				float dotDiffuse = saturate(dot(waterSurfaceNormal, sunDir));
				outColor.rgb *= dotDiffuse;

				// FOG:	
				float fogAmount = lerp(0, 1, depthNormalized);
				outColor.rgb = lerp(outColor.rgb, waterFogColor, fogAmount * isUnderwater);
		
				// Reflection!!!
	
				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
				float3 cameraToVertexDir = normalize(cameraToVertex);
				float3 reflectedViewDir = cameraToVertexDir + 2 * waterSurfaceNormal * 0.5;
				float viewDot = 1.0 - saturate(dot(-cameraToVertexDir, waterSurfaceNormal));

				//float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;
				// Have to sample SkyTexture din displayShader????

	
				float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;
				float4 skyTex = tex2D(_SkyTex, skyCoords);
				float4 reflectedColor = float4(skyTex.rgb, outColor.a); //col;
	
				
				float reflectLerp = saturate(viewDot * isUnderwater);
				outColor.rgb += lerp(float3(0,0,0), reflectedColor, reflectLerp);

				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

				//float4 finalColor = GetEnvironmentColor(i.worldPos, terrainColorTex, altitudeTex, waterSurfaceTex, resourceTex, skySample);
				
				float4 finalColor = outColor;
				finalColor.a = brushColor.a;
				
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
