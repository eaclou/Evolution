﻿Shader "Terrain/TerrainGroundStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_ResourceGridTex ("_ResourceGridTex", 2D) = "black" {}
		_TerrainColorTex ("_TerrainColorTex", 2D) = "black" {}
		_SpiritBrushTex ("_SpiritBrushTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
	}
	SubShader
	{	
		//Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }

		//Tags { "RenderType"="Opaque" }
		Tags { "Queue"="Transparent" }
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		//ZTest Off
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
			sampler2D _SpiritBrushTex;
			sampler2D _SkyTex;
			
			uniform float4 _WorldSpaceCameraPosition;
			uniform float _MapSize;	
			uniform float _MaxAltitude;
			uniform float _GlobalWaterLevel;
			uniform float3 _SunDir;

			uniform float4 _Color0;
			uniform float4 _Color1;
			uniform float4 _Color2;
			uniform float4 _Color3;
			
			struct EnvironmentStrokeData {
				float3 worldPos;
				float2 scale;
				float2 heading;
				float isActive;
				int brushType;
				// extra stuff:
				// mass, type,
				// velocity, accel
			};

			//StructuredBuffer<FrameBufferStrokeData> frameBufferStrokesCBuffer;
			StructuredBuffer<EnvironmentStrokeData> environmentStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture				
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
				EnvironmentStrokeData strokeData = environmentStrokesCBuffer[inst];

				float3 worldPosition = strokeData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				o.worldPos = worldPosition;


				float2 altUV = worldPosition.xy / _MapSize;
				o.altitudeUV = altUV;

				float4 altitudeSample = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0));
				float altitude = altitudeSample.x; //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceSample = tex2Dlod(_WaterSurfaceTex, float4(altUV, 0, 0));
				float3 surfaceNormal = waterSurfaceSample.yzw;
				float depth = saturate(-altitude + _GlobalWaterLevel);

				float refractionStrength = depth * 4.5;
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				
				
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomAspect = lerp(0.75, 1.33, random1);
				
				float2 scale = strokeData.scale * randomAspect * 0.99514 * strokeData.isActive;
				quadPoint *= float3(scale, 1.0);

				// &&&& Screen-space UV of center of brushstroke:
				// Magic to get proper UV's for sampling from GBuffers:
				//float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				//float4 screenUV = ComputeScreenPos(pos);
				//o.screenUV = screenUV; //centerUV.xy / centerUV.w;

				// Figure out final facing Vectors!!!
				float2 forward = strokeData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				//worldPosition.z = min(worldPosition.z, waterSurfaceSample.x * 1);

				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));				
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0) + float4(rotatedPoint, 0, 0)));	
				o.pos = lerp( mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0) + float4(rotatedPoint, 0, 0))), mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0)), 0.42);	
				o.uv = quadVerticesCBuffer[id] + 0.5f; // full texture
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{	
				
				float4 resourceTex = tex2D(_ResourceGridTex, i.altitudeUV);
				
				float3 decomposerHue = float3(0.8,0.3,0);
				float decomposerMask = saturate(resourceTex.z * 1) * 0.8;
				float3 detritusHue = float3(0.2,0.1,0.02);
				float detritusMask = saturate(resourceTex.y * 1) * 0.8;
				float3 algaeColor = float3(0.5,0.8,0.5) * 0.5;
				float algaeMask = saturate(resourceTex.w * 2.70);
				
				
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV);

				float4 finalColor = _Color0;   // BASE STONE COLOR:	
				finalColor = lerp(finalColor, _Color1, saturate(altitudeTex.y));
				finalColor = lerp(finalColor, _Color2, saturate(altitudeTex.z));

				finalColor.rgb = lerp(finalColor.rgb, decomposerHue, decomposerMask);
				finalColor.rgb = lerp(finalColor.rgb, detritusHue, detritusMask);
				finalColor.rgb = lerp(finalColor.rgb, algaeColor, algaeMask);
	
				//Diffuse
				float pixelOffset = 1.0 / 256;  // resolution  // **** THIS CAN"T BE HARDCODED AS FINAL ****"
				// ************  PRE COMPUTE THIS IN A TEXTURE!!!!!! ************************
				float altitudeNorth = tex2D(_AltitudeTex, i.altitudeUV + float2(0, pixelOffset)).x;
				float altitudeEast = tex2D(_AltitudeTex, i.altitudeUV + float2(pixelOffset, 0)).x;
				float altitudeSouth = tex2D(_AltitudeTex, i.altitudeUV + float2(0, -pixelOffset)).x;
				float altitudeWest = tex2D(_AltitudeTex, i.altitudeUV + float2(-pixelOffset, 0)).x;

				float dX = altitudeEast - altitudeWest;
				float dY = altitudeNorth - altitudeSouth;

				float2 grad = float2(0,1);
				if(dX != 0 && dY != 0) {
					grad = normalize(float2(dX, dY));
				}
				//store normals in brushstrokeData?? // *************

				float3 groundSurfaceNormal = normalize(float3(-grad.x, -grad.y, -length(float2(dX,dY)))); ////normalize(altitudeTex.yzw);
				groundSurfaceNormal.z *= -1;

				
				ShadingData data;
				data.baseAlbedo = finalColor; //float4(0.145,0.0972,0.015,1);
				data.altitudeTex = altitudeTex;
    			data.waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				data.groundNormalsTex = float4(0, groundSurfaceNormal);
    			data.resourceGridTex = resourceTex;
				data.spiritBrushTex = tex2D(_SpiritBrushTex, i.altitudeUV);
				data.skyTex = tex2D(_SkyTex, i.altitudeUV);
				data.worldPos = i.worldPos;
				data.maxAltitude = _MaxAltitude;
				data.waterFogColor = float4(algaeColor, 1);
				data.sunDir = float4(_SunDir, 0);
				data.worldSpaceCameraPosition = _WorldSpaceCameraPosition;
				data.globalWaterLevel = _GlobalWaterLevel;
				data.causticsStrength = 0.5;
				data.depth = saturate(-data.altitudeTex.x + data.globalWaterLevel);

				float4 outColor = MasterLightingModel(data);
				outColor.a *= tex2D(_MainTex, i.uv).a;
				return outColor;
			}
		ENDCG
		}
	}
}
