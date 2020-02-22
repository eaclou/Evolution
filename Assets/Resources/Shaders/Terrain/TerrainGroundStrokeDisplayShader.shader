Shader "Terrain/TerrainGroundStrokeDisplayShader"
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
			
			//sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			uniform float _MapSize;	
			
			uniform float _GlobalWaterLevel;
			
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
				float4 brushColor = tex2D(_MainTex, i.uv);	
								
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				float4 resourceTex = tex2D(_ResourceGridTex, i.altitudeUV);	
				float4 terrainColorTex = tex2D(_TerrainColorTex, i.altitudeUV);	
				float4 spiritBrushTex = tex2D(_SpiritBrushTex, i.altitudeUV);	
				
				float4 finalCol = float4(terrainColorTex.rgb, brushColor.a);
				
				return finalCol;
								
			}
		ENDCG
		}
	}
}
