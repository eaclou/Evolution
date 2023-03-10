Shader "Brushstrokes/AnimalParticleInternalBitsDisplayShader"
{
	Properties
	{
		
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_ResourceGridTex ("_ResourceGridTex", 2D) = "black" {}
		_TerrainColorTex ("_TerrainColorTex", 2D) = "black" {}
		_SpiritBrushTex ("_SpiritBrushTex", 2D) = "black" {}
		_PatternTex ("_PatternTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
	}
	SubShader
	{		
		Tags{ "RenderType" = "Transparent" }
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
			#include "Assets/Resources/Shaders/Inc/StructsAnimalParticles.cginc"
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"

			struct AnimalParticleInternalBitData {
				float4 localPos;
				float4 color;
			};

			sampler2D _MainTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _ResourceGridTex;
			sampler2D _TerrainColorTex;
			sampler2D _SpiritBrushTex;
			sampler2D _PatternTex;
			sampler2D _SkyTex;
			
			StructuredBuffer<AnimalParticleData> animalParticleDataCBuffer;
			StructuredBuffer<AnimalParticleInternalBitData> animalParticleInternalBitDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;


			uniform int _SelectedParticleIndex;
			uniform int _HoverParticleIndex;
			uniform float _IsSelected;
			uniform float _IsHover;

			uniform float3 _SunDir;
			uniform float _MapSize;
			uniform float _GlobalWaterLevel;
			uniform float _CamDistNormalized;
			uniform float4 _WorldSpaceCameraPosition;
			uniform float _MaxAltitude;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : COLOR;
				float4 hue : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float2 altitudeUV : TEXCOORD3;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			// Rotation with angle (in radians) and axis
			float3x3 AngleAxis3x3(float angle, float3 axis)
			{
				float c, s;
				sincos(angle, s, c);

				float t = 1 - c;
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;

				return float3x3(
					t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
					t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
					t * x * z - s * y,  t * y * z + s * x,  t * z * z + c
				);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				float3 quadPoint = quadVerticesCBuffer[id];
				float2 uv = quadPoint.xy;
				//uv.x += 0.5;
				//float t = uv.y; // + 0.5;
				//uv.y = 1.0 - uv.y;
								
				int particleIndex = floor((float)inst / 64.0); // 32 = number of quads, hardcoded
				AnimalParticleData particleData = animalParticleDataCBuffer[particleIndex];

				//float selectedMask = (1.0 - saturate(abs(_SelectedParticleIndex - particleIndex))) * _IsSelected;
				//float hoverMask = (1.0 - saturate(abs(_HoverParticleIndex - particleIndex))) * _IsHover;

				float2 altUV = particleData.worldPos.xy / _MapSize;	
				o.altitudeUV = altUV;
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x;
				float zPos = -max(_GlobalWaterLevel, altitudeRaw) * _MaxAltitude;

				float3 worldPosition = particleData.worldPos;    //float3(rawData.worldPos, -random2);				
				worldPosition.z = zPos;
				//float3 localQuadPos = quadPoint;

				float quadIndex = (float)(inst % 64);
				float quadIndexNormalized = quadIndex / 64.0;
				
				float2 forward = float2(0,1); // add heading? & twist 2.5D
				if(particleData.velocity.x != 0 || particleData.velocity.y != 0) {
					forward = normalize(particleData.velocity);
				}
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector

				float radius = sqrt(particleData.biomass) * 0.056 + 0.04;
				//(saturate(particleData.biomass * 0.25 + 0.05));// * lerp(0.8, 3.6, _CamDistNormalized);
				
				float3 spawnOffset = animalParticleInternalBitDataCBuffer[inst].localPos.xyz * radius;
				spawnOffset = float3(spawnOffset.x * right + spawnOffset.y * forward, spawnOffset.z);	
				float distToCenter = length(spawnOffset);
				// REFRACTION:
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / _MapSize, 0, 0)).yzw;				
				float refractionStrength = 0.5;
				
				spawnOffset = mul(animalParticleInternalBitDataCBuffer[inst].localPos.zyx, AngleAxis3x3(_Time.y*6.421* (1-particleData.isDecaying), normalize(spawnOffset)));
				spawnOffset *= radius;
				
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;
				spawnOffset.xy += -surfaceNormal.xy * refractionStrength;

				float masterFreq = 17;
				//float spatialFreq = 0.06125285;
				float timeMult = 0.03123;
				//float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time.y * timeMult + inst * 11.11, masterFreq); //float3(0, 0, _Time * timeMult) + 
				//float noiseMag = 0.0015;
				//float3 noiseOffset = noiseSample.yzw * noiseMag;
				float3 randomDir = float3(1,0,0);
				float4 noiseSample = Value3D(_Time.y * timeMult + spawnOffset*0.27, masterFreq); 
				float theta = noiseSample.z;
				float phi = noiseSample.y;
				noiseSample *= radius;
				randomDir.x = clamp(noiseSample.y, -1 * radius, radius); //cos(theta)*sin(phi);
				randomDir.y = clamp(noiseSample.z, -1 * radius, radius);//cos(phi);
				randomDir.z = clamp(noiseSample.w, -1 * radius, radius); //sin(theta)*sin(phi);
				//randomDir *= radius;//distToCenter;

				//float3 direction;  // points in the direction of the new Y axis
				//float3 vec;        // This is a randomly generated point that we will
                 // eventually transform using our base-change matrix
				
				worldPosition.xyz += spawnOffset;
				
				//worldPosition.xyz += noiseOffset;

				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward, 0);  // Rotate localRotation by AgentRotation

				float quadScale = (saturate(particleData.biomass * 3 + 0.25) * 0.05247 * particleData.isActive);
				o.worldPos = float4(worldPosition, 0);
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition + rotatedPoint * quadScale, 1.0)));
				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));				
				o.uv = uv;	
				float isAlive = 1-particleData.isDecaying;
				o.color = lerp(particleData.color, animalParticleInternalBitDataCBuffer[inst].color, 0.35);// float4(saturate(particleData.isDecaying), saturate(particleData.biomass * 5), 0, 0);
				o.color = lerp(o.color, particleData.color + float4(1,1,1,0)*(1-particleData.isDecaying)*0.3, (1-particleData.isDecaying));
				o.color *= isAlive * 0.5 + 0.5;
				
				o.hue = particleData.color;
				return o;

			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 texColor = tex2D(_MainTex, i.uv);
				return i.color;
				fixed4 col = texColor;
				col.rgb = lerp(float3(0.4, 0.97, 0.3), i.hue.rgb, 0.325);
				col.a = 1;
				//col.rgb = lerp(col, float3(0.61, 0.79, 0.65) * 0.1, i.color.x * 0.);
				col.rgb = lerp(col.rgb, float3(0.651, 0.8379, 0.55) * 0.4, i.color.x * 0.75);
				if(i.color.a > 0.5) {
					col.rgb = float3(1,1,1);
				}
				return col;
				
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
				float3 groundSurfaceNormal = normalize(float3(-grad.x, -grad.y, length(float2(dX,dY)))); ////normalize(altitudeTex.yzw);
				
				float3 algaeColor = float3(0.5,0.8,0.5) * 0.5;

				ShadingData data;
				data.baseAlbedo = col * 2;
				data.altitudeTex = tex2D(_AltitudeTex, i.altitudeUV);
    			data.waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				data.groundNormalsTex = float4(groundSurfaceNormal, 0);
    			data.resourceGridTex = tex2D(_ResourceGridTex, i.altitudeUV);
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
				
				float4 finalColor = data.baseAlbedo;
				float caustics = GetCausticsLight(finalColor, data);
				float diffuse = GetDiffuseLight(finalColor, data);
				
				float fogAmount = GetWaterFogAmount(data.depth * 0.5);
				float isUnderwater = saturate(data.depth * 57);
				float4 reflectionColor = GetReflectionAmount(data.worldPos, data.worldSpaceCameraPosition.xyz, data.waterSurfaceTex.yzw, data.skyTex, isUnderwater);
								
				finalColor.rgb *= diffuse;
				finalColor.rgb += caustics;
				//finalColor.rgb *= wetnessMod;
				//finalColor.rgb += shoreFoam;
				finalColor.rgb = lerp(finalColor.rgb, data.waterFogColor.rgb, fogAmount);
				finalColor.rgb += lerp(float3(0,0,0), reflectionColor.xyz, reflectionColor.w);
				
				//finalColor.rgb += data.spiritBrushTex.y;
				
				finalColor.a *= tex2D(_MainTex, i.uv).a;
				finalColor.rgb *= (0.5 + 0.5 * i.color.x);
				finalColor.a = 1;
				if(i.color.a > 0.5) {
					finalColor.rgb = float3(1,1,1);
				}
				return finalColor;

				//return float4(1,1,1,1);
				//return finalColor;
			}
		ENDCG
		}
	}
}
