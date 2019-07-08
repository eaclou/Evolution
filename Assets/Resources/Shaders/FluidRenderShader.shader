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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _DensityTex;
			sampler2D _VelocityTex;
			sampler2D _PressureTex;
			sampler2D _DivergenceTex;
			sampler2D _ObstaclesTex;
			sampler2D _TerrainHeightTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _SpiritBrushTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				//v.vertex.z -= tex2Dlod(_WaterSurfaceTex, float4(v.uv,0,0)).x * 5;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				
				// sample the texture
				fixed4 density = tex2D(_DensityTex, i.uv);
				//return density;
				//density.a = 1;
				
				
				fixed4 velocity = tex2D(_VelocityTex, i.uv);
				fixed4 pressure = tex2D(_PressureTex, i.uv);
				fixed4 divergence = tex2D(_DivergenceTex, i.uv);
				fixed4 obstacles = tex2D(_ObstaclesTex, i.uv);
				//float val = density.y * 2;
				//float dist = 1.0 - saturate(abs(0.3 - val) * 4.2);
				//return float4(val, val, val, 1);
				//return density; // + density2 * 0.25;
				fixed4 finalColor = float4(0,0,0,1);
				finalColor.xyz = density.xyz;
				finalColor.a = density.y;
				//finalColor.a = smoothstep(0.15, 0.3, density.y) * 0.15;
				
				//float3 Value2D(float2 p, float frequency)
				float timeMult = 0.420;
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
				//density.rgb = lerp(density.rgb, float3(0.64, 1, 0.45) * 0.25, 0.85);
				//density.a = saturate((density.a - 0.000055) * 3.95) * 1;

				//density.a *= (noiseMag * 0.01 + 0.99);
				//float4 testAlpha = density;
				//testAlpha.a *= (noiseMag * 0.25 + 0.75);
				float2 grad = float2(ddx(density.a), ddy(density.a));
				float2 gradNorm = normalize(grad);
				float2 lightDir = float2(0,-1);
				float diffuse = dot(gradNorm, lightDir) * 0.5 + 0.5;
				float shadow = dot(gradNorm, -lightDir) * 0.5 + 0.5;

				density.rgb += float3(1,1,1) * saturate(diffuse) * 0.05;
				density.rgb -= float3(1,1,1) * saturate(shadow) * 0.05;
				//density.a *= (noiseMag * 0.95 + 0.05);

				fixed4 brushTex = tex2D(_SpiritBrushTex, i.uv);
				density.a = saturate(brushTex.x * 0.15 + density.a);
				
				return saturate(density);

				float pressureAmount = saturate((pressure.y - 0.2) * 3);
				//finalColor.a += pressureAmount * 0.4 * noiseMag;
				finalColor.rgb += pressureAmount * 0.2 * noiseMag;

				float velocityGlow = saturate((length(velocity) - 0.025) * 3.3);
				//finalColor.a += velocityGlow * 0.4 * noiseMag;
				//finalColor.rgb += velocityGlow * 0.65 * noiseMag;
				//finalColor.rgb = lerp(finalColor.rgb, float3(1,1,0), velocityGlow);
				
				float4 heightTex = tex2D(_TerrainHeightTex, i.uv);
				float altitude = heightTex.x;  // [-1,1] range
				float onLandMask = saturate((altitude - 0.48) * 8);
				float shallowsMask = 1.0 - saturate(((1-altitude) - 0.485) * 2.5);
				float shorelineGlow = shallowsMask * (1.0 - onLandMask);
				finalColor.rgb += shorelineGlow * 0.75;
				//finalColor.a += shorelineGlow * 0.25;
				
				//finalColor.a *= (0.15 + noiseMag * 0.85);
				//finalColor.rgb = float3(1,1,1) * 0.5;
				return saturate(finalColor);

				

				float posterizeLevels = 64;

				float boost = 1;

				//finalColor.xyz = round(finalColor.xyz * posterizeLevels) / posterizeLevels;

				float pressureAmplitude = length(pressure.xy);
				//finalColor.xyz += (pressureAmplitude * 0.5);
				//boost += pressureAmplitude;

				float velocityAmplitude = length(velocity.xy);
				//finalColor.xyz += velocityAmplitude * 6;
				boost += velocityAmplitude * 5;

				float divergenceAmplitude = abs(divergence.x) * 50;
				//finalColor.xyz += divergenceAmplitude;
				//boost += divergenceAmplitude;

				//float4 heightTex = tex2D(_TerrainHeightTex, i.uv * 0.5 + 0.25);
				//float altitude = heightTex.x * 2 - 1;  // [-1,1] range
				//altitude *= -1; // *** invert needed for some reason!!! REVISIT THIS!!! ****

				//finalColor.a -= (1.0 - saturate(velocityAmplitude * 12)) * 0.2;

				float brightness = (finalColor.x + finalColor.y + finalColor.z) / 3.0;
								
				finalColor.a = saturate(-(altitude - 0.075) * 17);

				//float alphaMod = saturate((brightness - 0.5) * -16);
				//finalColor.a = saturate(finalColor.a - alphaMod * 1);

				//float shorelineBoost = 1.0 - saturate(-altitude);
				//float distToSeaLevel = abs(altitude);
				//float shoreLerp = 1.0 - distToSeaLevel * 10;
				//finalColor.a = shorelineBoost;
				//if(altitude < 0 && altitude > -0.1) {
					//finalColor.a = lerp(finalColor.a, shorelineBoost * 0.5, shoreLerp);
				//}

				//finalColor.a *= 0.55;

				//finalColor = obstacles;
				//finalColor.x *= 0.65;
				//finalColor.y *= 0.85;
				//finalColor.z *= 1.2;

				finalColor.rgb *= 1.1;
				//if(altitude < 0) {
				//	return float4(1,1,1,1);
				//}
				finalColor = lerp(finalColor, float4(0.5, 0.5, 0.5, 1), 0.5);
				//return float4(0.5, 0.5, 0.5, 1);

				//float4 debugTexColor = tex2D(_DebugTex, i.uv);
				//float4 debugColor = float4(debugTexColor.x * 3, debugTexColor.x * 3, 0, 1.0);
				//debugColor.a = 1;
				//debugColor.rgb *= 10;
				//debugColor.rb = 0;
				//return debugColor;

				return finalColor;
			}
			ENDCG
		}
	}
}
