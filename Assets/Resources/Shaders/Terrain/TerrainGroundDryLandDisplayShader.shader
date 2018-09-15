Shader "Terrain/TerrainGroundDryLandDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		
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

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			
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
								
				FrameBufferStrokeData strokeData = frameBufferStrokesCBuffer[inst];

				float3 worldPosition = strokeData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				o.worldPos = worldPosition;

				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

				float altitude = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range
				
				float isUnderwater = saturate(((saturate(altitude - 0.1) * 2 - 1) * -1) * 10000);
				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(((altUV - 0.25) * 2), 0, 0)).yzw;
				float depth = saturate(-altitude + 0.5);
				float refractionStrength = depth * 8.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;
								
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomAspect = lerp(0.75, 1.33, random1);
				
				float2 scale = strokeData.scale * randomAspect * 1;
				quadPoint *= float3(scale * (1.0 - isUnderwater), 1.0);

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
				// use separate camera?
				
				float4 finalColor = frameBufferColor;
				finalColor.a = brushColor.a;
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				// 0-1 range --> -1 to 1
				altitude = (altitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				float strataColorMultiplier = (sin(altitude * (1.0 + i.worldPos.x * 0.01 - i.worldPos.y * -0.01) + i.worldPos.x * 0.01 - i.worldPos.y * 0.01) * 0.5 + 0.5) * 0.5 + 0.5;
				finalColor.rgb *= strataColorMultiplier;				
				

				float snowAmount = saturate((-altitude - 0.6) * 2 +
								   ((sin(i.worldPos.x * 0.0785 + i.worldPos.y * 0.02843) * 0.5 + 0.5) * 1 - 
								   (cos(i.worldPos.x * 0.012685 + i.worldPos.y * -0.01843) * 0.5 + 0.5) * 0.9 +
								   (sin(i.worldPos.x * 0.2685 + i.worldPos.y * -0.1843) * 0.5 + 0.5) * 0.45 - 
								   (cos(i.worldPos.x * -0.2843 + i.worldPos.y * 0.01143) * 0.5 + 0.5) * 0.45 +
								   (sin(i.worldPos.x * 0.1685 + i.worldPos.y * -0.03843) * 0.5 + 0.5) * 0.3 - 
								   (cos(i.worldPos.x * -0.1843 + i.worldPos.y * 0.243) * 0.5 + 0.5) * 0.3) * 0.5);
				
				finalColor.rgb = lerp(finalColor.rgb, float3(0.56, 1, 0.34) * 0.6, snowAmount * 1);
				

				// FAKE CAUSTICS:::
				float3 surfaceNormal = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
				float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;

				finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (dotLight * 0.33 + 0.67) + dotLight * 0.75, isUnderwater * (1.0 - altitude)); //dotLight * 1.0;


				// FOG:
				finalColor.rgb = lerp(finalColor.rgb, waterFogColor, 1 * (saturate(altitude * 0.8)) + 0.25 * isUnderwater);

				//return float4(dotLight, dotLight, dotLight, 1);

				//float4 waterSurfaceSample = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2);
				//finalColor = float4(waterSurfaceSample.yzw * 0.5 + 0.5, 1);
				//finalColor.rgb = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
				//finalColor.rg = finalColor.rg * 0.5 + 0.5;
				//finalColor.a = 1;

				//float3 surfaceNormal = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
				//float diffuse = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				//finalColor.rgb = float3(diffuse, diffuse, diffuse);

				return finalColor;
				
			}
		ENDCG
		}
	}
}
