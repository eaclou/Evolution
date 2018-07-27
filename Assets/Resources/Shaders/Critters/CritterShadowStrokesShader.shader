Shader "Critter/CritterShadowStrokesShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
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
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
			//#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"  // *** This is incliuded in CritterBodyAnimation.cginc

			struct v2f
			{
				float4 pos : SV_POSITION;	
				float3 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float2 spriteUV : TEXCOORD2;  // uv of the brushstroke quad itself, particle texture
				float2 altitudeUV : TEXCOORD3;
				float4 screenUV : TEXCOORD4;
				float2 skyUV : TEXCOORD5;
				float2 patternUV : TEXCOORD6;
				int2 bufferIndices : TEXCOORD7;
				float4 vignetteLerp : TEXCOORD8;
				float4 color : COLOR;
				//float2 altitudeUV : TEXCOORD1;
				//float4 screenUV : TEXCOORD2;
				//float3 worldPos : TEXCOORD3;
				//float4 vignetteLerp : TEXCOORD4;
				//float2 skyUV : TEXCOORD5;
			};

			sampler2D _MainTex;
			sampler2D _PatternTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _SkyTex;
			sampler2D _WaterSurfaceTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;

			StructuredBuffer<float3> quadVerticesCBuffer;
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<CritterSkinStrokeData> critterSkinStrokesCBuffer;
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				CritterSkinStrokeData skinStrokeData = critterSkinStrokesCBuffer[inst];
				uint agentIndex = skinStrokeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];
				o.bufferIndices = int2(inst, agentIndex);
				
				float3 quadPoint = quadVerticesCBuffer[id];				
				
				float3 critterWorldPos = critterSimData.worldPos;
				float3 critterCurScale = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);

				float dotGrowth = saturate(skinStrokeData.strength * 2.0);
				float dotDecay = saturate((skinStrokeData.strength - 0.5) * 2);
				float dotHealthValue = dotGrowth * (1.0 - dotDecay);

				float3 spriteLocalPos = skinStrokeData.localPos * critterCurScale;
				float3 vertexWorldOffset = quadPoint;
				vertexWorldOffset.xy = vertexWorldOffset.xy * skinStrokeData.localScale * critterCurScale * (1.0 - critterSimData.decayPercentage);
				
				float3 spriteWorldOffset = spriteLocalPos; // **** Vector from critter origin to sprite origin
				
				//spriteWorldOffset = GetAnimatedPos(spriteWorldOffset, float3(0,0,0), critterInitData, critterSimData, skinStrokeData);
				vertexWorldOffset = GetAnimatedPos(vertexWorldOffset, float3(0,0,0), critterInitData, critterSimData, skinStrokeData);
				
				float3 worldPosition = skinStrokeData.worldPos + vertexWorldOffset; //critterWorldPos + vertexWorldOffset; //	
				//float3 worldPosition = critterWorldPos + vertexWorldOffset; //
				// REFRACTION:
				//float altitude = tex2Dlod(_AltitudeTex, float4(altUV, 0, 0)).x; //i.worldPos.z / 10; // [-1,1] range
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				//float depth = saturate(-altitude + 0.5);
				float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;


				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

				//vertexWorldOffset *=

				

				
				float3 localNormal = normalize(skinStrokeData.localPos);
				//float3 worldNormal = localNormal;
				localNormal = normalize(float3(localNormal.x * critterInitData.boundingBoxSize.y, localNormal.y * critterInitData.boundingBoxSize.x, localNormal.z * critterInitData.boundingBoxSize.y));
				//worldNormal.xy = float2(localNormal.x * cos(swimAngle) - localNormal.y * sin(swimAngle), localNormal.y * cos(swimAngle) + localNormal.x * sin(swimAngle));
				//worldNormal.xy = rotatePointVector(worldNormal.xy, float2(0,0), critterSimData.heading);

				float3 worldNormal = GetAnimatedPos(localNormal, float3(0,0,0), critterInitData, critterSimData, skinStrokeData); //skinStrokeData.localPos;
				o.worldNormal = worldNormal;

				// UVS:
				//=============================================================================================================
				o.spriteUV = quadPoint.xy + 0.5;

				const float tilePercentage = (1.0 / 8.0);
				float2 patternUV = skinStrokeData.localPos * 0.5 + 0.5;
				float randPatternIDX = critterInitData.bodyPatternX; // ** This is now inside CritterInitData!!!!!
				float randPatternIDY = critterInitData.bodyPatternY; //  fmod(bodyStrokeData.brushTypeY, 4); // ********** UPDATE!!! **************
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;				
				o.patternUV = patternUV;

				

				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV;

				worldPosition.z = -(tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 2)).x * 2 - 1) * 10;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.worldPos = worldPosition;

				o.skyUV = worldPosition.xy / _MapSize + float2(-0.25, 0.08) * 0.14 * _Time.y;
				//=============================================================================================================
				
				float randNoise1 = Value3D(float3(worldPosition.x - _Time.y * 5.34, worldPosition.y + _Time.y * 7.1, _Time.y * 15), 0.1).x * 0.5 + 0.5; //				
				float randNoise2 = Value3D(float3(worldPosition.x + _Time.y * 7.34, worldPosition.y - _Time.y * 6.1, _Time.y * -10), 0.25).x * 0.5 + 0.5;
				float randNoise3 = Value3D(float3(worldPosition.x + _Time.y * 3.34, worldPosition.y - _Time.y * 5.1, _Time.y * 7.5), 0.63).x * 0.5 + 0.5;
				float randNoise4 = Value3D(float3(worldPosition.x - _Time.y * 8.34, worldPosition.y + _Time.y * 4.1, _Time.y * -3.5),1).x * 0.5 + 0.5;
				randNoise1 *= 1;
				randNoise2 *= 1;
				randNoise3 *= 1.5;
				randNoise4 *= 2;
				float randThreshold = (randNoise1 + randNoise2 + randNoise3 + randNoise4) / 4;	
				float2 sampleUV = screenUV.xy / screenUV.w;
				float vignetteRadius = length((sampleUV - 0.5) * 2);
				float testNewVignetteMask = saturate(((randThreshold + 0.6 - (saturate(vignetteRadius) * 0.4 + 0.3)) * 2));
				o.vignetteLerp = float4(testNewVignetteMask,sampleUV,saturate(vignetteRadius));
				
				o.color = float4(1, 1, critterSimData.growthPercentage, critterSimData.decayPercentage);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				CritterInitData critterInitData = critterInitDataCBuffer[i.bufferIndices.y];

				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	


				float4 texColor = tex2D(_MainTex, i.spriteUV);
				float4 patternSample = tex2Dlod(_PatternTex, float4(i.patternUV, 0, 2));	

				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float diffuseLight = saturate(dot(lightDir, normalize(i.worldNormal)));
				//return float4(i.worldNormal,1);

				float4 finalColor = float4(lerp(critterInitData.primaryHue, critterInitData.secondaryHue, patternSample.x), texColor.a);
				
				finalColor.rgb *= diffuseLight * 0.75 + 0.25;
				
				float4 backgroundColor = frameBufferColor;
				backgroundColor.a = texColor.a;
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); // i.worldPos.z / 10; // [-1,1] range
				// 0-1 range --> -1 to 1
				altitude = (altitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				float strataColorMultiplier = (sin(altitude * (1.0 + i.worldPos.x * 0.01 - i.worldPos.y * -0.01) + i.worldPos.x * 0.01 - i.worldPos.y * 0.01) * 0.5 + 0.5) * 0.5 + 0.5;
				backgroundColor.rgb *= strataColorMultiplier;				
				backgroundColor.rgb = lerp(backgroundColor.rgb, waterFogColor, 1 * (saturate(altitude * 0.8)) + 0.25 * isUnderwater);

				float snowAmount = saturate((-altitude - 0.6) * 2 +
								   ((sin(i.worldPos.x * 0.0785 + i.worldPos.y * 0.02843) * 0.5 + 0.5) * 1 - 
								   (cos(i.worldPos.x * 0.012685 + i.worldPos.y * -0.01843) * 0.5 + 0.5) * 0.9 +
								   (sin(i.worldPos.x * 0.2685 + i.worldPos.y * -0.1843) * 0.5 + 0.5) * 0.45 - 
								   (cos(i.worldPos.x * -0.2843 + i.worldPos.y * 0.01143) * 0.5 + 0.5) * 0.45 +
								   (sin(i.worldPos.x * 0.1685 + i.worldPos.y * -0.03843) * 0.5 + 0.5) * 0.3 - 
								   (cos(i.worldPos.x * -0.1843 + i.worldPos.y * 0.243) * 0.5 + 0.5) * 0.3) * 0.5);
				
				backgroundColor.rgb = lerp(backgroundColor.rgb, float3(0.56, 1, 0.34) * 0.6, snowAmount * 1);
				//==================================================================================================================
				backgroundColor.a *= isUnderwater;

				float fogAmount = saturate(i.worldPos.z * 0.5);
				

				
				float4 reflectedColor = float4(tex2Dlod(_SkyTex, float4((i.skyUV), 0, 1)).rgb, backgroundColor.a); //col;
				
				finalColor = lerp(reflectedColor, finalColor, saturate(1 - (1 - i.vignetteLerp.x) * 1)); //float4(1,1,1,1);
				//finalColor.a *= saturate(i.vignetteLerp.w * 1.4 - 0.25); //(1 - saturate(i.vignetteLerp.x) * 0.4) * 0.5;
				//finalColor.a *= i.color.a;
				finalColor.rgb = float3(0,0,0);
				finalColor.rgb = lerp(finalColor.rgb, waterFogColor, fogAmount);
				finalColor.a *= 0.5;
				
				finalColor.a *= (1.0 - i.color.w);
				
				return finalColor;

				//return finalColor;
				//return col;
			}
			ENDCG
		}
	}
}
