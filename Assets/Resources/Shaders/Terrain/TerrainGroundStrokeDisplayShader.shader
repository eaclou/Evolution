﻿Shader "Terrain/TerrainGroundStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_DecomposerTex ("_DecomposerTex", 2D) = "black" {}
		//_NutrientTex ("_NutrientTex", 2D) = "black" {}
		
	}
	SubShader
	{		
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		ZTest Always
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
			sampler2D _DecomposerTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
						
			
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
				float4 screenUV : TEXCOORD1;
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
								
				FrameBufferStrokeData strokeData = frameBufferStrokesCBuffer[inst];

				float3 worldPosition = strokeData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				o.worldPos = worldPosition;

				float2 altUV = worldPosition.xy / 256;
				o.altitudeUV = altUV;

				float altitude = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(altUV, 0, 0)).yzw;
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
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //centerUV.xy / centerUV.w;

				// Figure out final facing Vectors!!!
				float2 forward = strokeData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));				
				o.uv = quadVerticesCBuffer[id] + 0.5f; // full texture
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				//float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				//float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = tex2D(_MainTex, i.uv);	
				
				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source					
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				float4 resourceTex = tex2D(_DecomposerTex, i.altitudeUV);	
				
				float4 finalColor = GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, resourceTex);
				finalColor.a = brushColor.a;
				
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}
