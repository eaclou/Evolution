Shader "Unlit/FluidRenderShader"
{
	Properties
	{
		_DensityTex ("_DensityTex", 2D) = "white" {}
		_VelocityTex ("_VelocityTex", 2D) = "white" {}
		_PressureTex ("_PressureTex", 2D) = "white" {}
		_DivergenceTex ("_DivergenceTex", 2D) = "white" {}
		_ObstaclesTex ("_ObstaclesTex", 2D) = "white" {}
		_TerrainHeightTex ("_TerrainHeightTex", 2D) = "grey" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
		_SpiritBrushTex ("_SpiritBrushTex", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
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
			#include "Assets/Resources/Shaders/Inc/WaterUtilityFunctions.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				//float2 skyUV : TEXCOORD5;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD6;
				//float2 skyUV : TEXCOORD5;
			};

			sampler2D _DensityTex;
			sampler2D _VelocityTex;
			sampler2D _PressureTex;
			sampler2D _DivergenceTex;
			sampler2D _ObstaclesTex;
			sampler2D _TerrainHeightTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _SpiritBrushTex;
			sampler2D _SkyTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				//v.vertex.z -= tex2Dlod(_WaterSurfaceTex, float4(v.uv,0,0)).x * 5;
				o.worldPos = v.vertex.xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				//float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);				
				//float3 surfaceNormal = waterSurfaceTex.yzw;
				
				//float3 worldPosition = o.vertex.xyz;
				
				//float3 cameraToVertex = worldPosition - _WorldSpaceCameraPos;
                //float3 cameraToVertexDir = normalize(cameraToVertex);				
				//float viewDot = dot(-cameraToVertexDir, surfaceNormal);
				//float reflectLerp = GetReflectionLerpSml(worldPosition, surfaceNormal, viewDot, _CamDistNormalized * 0.25 + 0.2, _CamFocusPosition, vignetteRadius);

				//o.skyUV = worldPosition.xy / _MapSize;
				
				//o.vignetteLerp = float4(reflectLerp,0,0,0);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				
				
				// sample the texture
				fixed4 density = tex2D(_DensityTex, i.uv);
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.uv);


				float3 worldPosition = i.worldPos;
				float waveHeight = 10;
				//worldPosition.z = 0;
				worldPosition.z += waterSurfaceTex.x * waveHeight;

				float algaeCoverAmount = saturate(density.a * 1.0);

				float brightnessMask = (algaeCoverAmount - 0.35) * 3;

				float timeMult = 0.0420;
				float noiseMag01 = (Value3D(float3(-_Time.y * 0.25, -i.uv), 53).x * 0.5 + 0.5);
				float noiseMag02 = (Value3D(float3(_Time.y * 0.0042 * timeMult, i.uv), 178).x * 0.5 + 0.5);				
				float noiseMag03 = (Value3D(float3(_Time.y * 0.0195 * timeMult, -i.uv), 304).x * 0.5 + 0.5);
				float noiseMag04 = (Value3D(float3(-_Time.y * 0.012 * timeMult, -i.uv), 532.331).x * 0.5 + 0.5);
				
				float noiseMag05 = (Value3D(float3(-_Time.y * 0.0315, i.uv), 256).x * 0.5 + 0.5);
				float noiseMag06 = (Value3D(float3(_Time.y * 0.00762 * timeMult, i.uv), 328).x * 0.5 + 0.5);				
				float noiseMag07 = (Value3D(float3(_Time.y * 0.01395 * timeMult, -i.uv), 288.7).x * 0.5 + 0.5);
				float noiseMag08 = (Value3D(float3(-_Time.y * 0.0142 * timeMult, -i.uv), 492.331).x * 0.5 + 0.5);

				float noiseMag = saturate((noiseMag04 * 0.4 + noiseMag02 * 0.2 + noiseMag03 * 0.4) * 0.5 + 0.5 * (noiseMag05 * 0.25 + noiseMag06 * 0.25 + noiseMag07 * 0.25 + noiseMag08 * 0.25)); // * noiseMag01;
				noiseMag = saturate((noiseMag - 0.5) * 2 + 0.5);

				algaeCoverAmount -= noiseMag * 0.15;
				algaeCoverAmount = saturate(algaeCoverAmount);


				float jagged0 = sin(algaeCoverAmount * 20.9) * 1.94; // * (1 + algaeCoverAmount);
				float jagged1 = frac(algaeCoverAmount * 3.753);
				float jagged2 = frac(algaeCoverAmount * 17.753);
				float jagged3 = 1.0 - frac(algaeCoverAmount * 37.753);
				//algaeCoverAmount 
				float newAlgae0 = max(algaeCoverAmount * (jagged0 * 0.5 + 0.5), algaeCoverAmount * 0.95);
				float newAlgae1 = max(algaeCoverAmount * (jagged1), algaeCoverAmount * 0.95);
				//float 
				algaeCoverAmount = saturate((lerp(newAlgae0, newAlgae1, 0.5) + jagged2 * 0.1) - 0.02);
				algaeCoverAmount += jagged3 * 0.05;

				algaeCoverAmount = saturate((sqrt(algaeCoverAmount) - 0.2) * 1.25);
				//float algaeSquared = algaeCoverAmount * algaeCoverAmount;
				//float invSqr = 

				

				float3 surfaceNormal = waterSurfaceTex.yzw;
				float3 cameraToVertex = worldPosition - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);
				//float waveDistortMag = 0.05;
				float3 reflectedViewDir = cameraToVertexDir + surfaceNormal * waveHeight;
				float viewDot = dot(-cameraToVertexDir, surfaceNormal);
				viewDot = min(max(viewDot, 0.5), 0.985);

				float2 skyCoords = reflectedViewDir.xy * 0.25 + 0.75;
				
				float skyScrollingSpeed = 0.0075;
				const int skySampleLOD = 0;
				float4 skySampleUV = float4((skyCoords) - _Time.y * skyScrollingSpeed, 0, skySampleLOD);
				
				float diffuse = saturate(dot(normalize(float3(-0.5,0.4,-0.6)), surfaceNormal));
				
				float reflectLerp = (1.0 - saturate(viewDot));
				//reflectLerp = reflectLerp * reflectLerp;

				float4 reflectedColor = float4(tex2Dlod(_SkyTex, skySampleUV).rgb, 1); //col;
				
				
				fixed4 finalColor = density * 0.5;
				//finalColor.rgb = float3(1,1,1);
				float4 heightTex = tex2D(_TerrainHeightTex, i.uv);
				float altitude = heightTex.x;  // [-1,1] range
				float onLandMask = 1.0 - saturate((altitude - 0.5) * 16);
				float shorelineGlow = saturate((altitude - 0.38) * 6.2) * (1 - algaeCoverAmount);

				finalColor *= 1.0 + shorelineGlow;
				finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * 0.82, saturate(brightnessMask));
								
				
				finalColor = lerp(finalColor, reflectedColor, reflectLerp * onLandMask);
				finalColor.a *= reflectLerp;
				finalColor.a = saturate(finalColor.a + algaeCoverAmount * 0.55);// * 0.35;
				finalColor.a *= onLandMask;
				
				finalColor += saturate((altitude - 0.5) * 80) * 0.01;
				//finalColor += reflectLerp * onLandMask * 1;
				float shoreMask = saturate((altitude - 0.34) * 3.20) * (onLandMask);
				//finalColor.a = saturate(max(finalColor.a, shoreMask));
				finalColor.a += shoreMask * 0.22;
				finalColor.rgb += shoreMask * 0.42;
				return saturate(finalColor);
				
				

			}
			ENDCG
		}
	}
}
