﻿Shader "UI/TreeOfLifePortraitShader"
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
			};

			sampler2D _MainTex;
			sampler2D _PatternTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _SkyTex;
			sampler2D _WaterSurfaceTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;

			uniform float4 _TopLeftCornerWorldPos;
			uniform float4 _CamRightDir;
			uniform float4 _CamUpDir;

			uniform float _CamScale;

			uniform float _AnimatedScale1;  // signal from main CPU program of whether the panel is active, hidden, or interpolating
			uniform float _AnimatedScale2; // signal from main CPU program of whether the panel is active, hidden, or interpolating

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
				
				float3 uiPivot = _TopLeftCornerWorldPos.xyz + (_CamRightDir.xyz * 0.6 - _CamUpDir.xyz * 0.6) * _CamScale;
				float3 critterWorldPos = uiPivot; //critterSimData.worldPos;
				float3 critterCurScale = critterInitData.boundingBoxSize * 0.25; // * critterSimData.growthPercentage * 0.75;

				float dotGrowth = saturate(skinStrokeData.strength * 2.0);
				float dotDecay = saturate((skinStrokeData.strength - 0.5) * 2);
				float dotHealthValue = dotGrowth * (1.0 - dotDecay);

				float3 spriteLocalPos = skinStrokeData.localPos * critterCurScale;
				float3 vertexWorldOffset = quadPoint;
				//vertexWorldOffset.x *= 0.25;
				//vertexWorldOffset.y *= 2.5;

				// EGG EMBRYO MASK:
				//float eggMask = saturate(saturate(critterSimData.embryoPercentage - 0.99) * 100);
				//float softEggMask = critterSimData.embryoPercentage * (1.0 - eggMask) * 0.25 + critterSimData.embryoPercentage * eggMask;
								
				//spriteLocalPos *= softEggMask;
				//vertexWorldOffset *= eggMask;

				float2 brushAspectRatio = float2(lerp((skinStrokeData.localScale.x + skinStrokeData.localScale.y) / 2.0, skinStrokeData.localScale.x, 0.5),
												lerp((skinStrokeData.localScale.x + skinStrokeData.localScale.y) / 2.0, skinStrokeData.localScale.y, 0.5));
				vertexWorldOffset.xy = vertexWorldOffset.xy * brushAspectRatio * critterCurScale;
								
				// ANIMATIONS:
				spriteLocalPos = GetAnimatedPosOld(spriteLocalPos, float3(0,0,0), critterInitData, critterSimData, skinStrokeData.localPos);
				float magnitude = 0.5;
				//float swimAngle = GetSwimAngle(strokeLocalPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, magnitude, critterSimData.turnAmount);
				float swimAngle = GetSwimAngleOld(skinStrokeData.localPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, magnitude, critterSimData.turnAmount);
				vertexWorldOffset = RotatePointAroundZAngle(float3(0,0,0), swimAngle, vertexWorldOffset);
				//vertexWorldOffset // Rotate with Critter:
				//float2 forward1 = critterSimData.heading;
				//float2 right1 = float2(forward1.y, -forward1.x);
				//vertexWorldOffset = RotatePointAroundZAngle(float3(0,0,0), swimAngle, vertexWorldOffset);
				float2 forward2 = critterSimData.heading;
				float2 right2 = float2(forward2.y, -forward2.x); // perpendicular to forward vector
				vertexWorldOffset = float3(vertexWorldOffset.x * right2 + vertexWorldOffset.y * forward2, 0);  
				//spriteLocalPos = float3(spriteLocalPos.x * right2 + spriteLocalPos.y * forward2, spriteLocalPos.z); 
				// REFRACTION:
				float3 offset = skinStrokeData.worldPos;				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(offset.xy /  _MapSize, 0, 0)).yzw;
				float refractionStrength = 2.45;
				offset.xy += -surfaceNormal.xy * refractionStrength;

				float3 worldPosition;// = offset + vertexWorldOffset; //critterWorldPos + vertexWorldOffset; //
				
				worldPosition = uiPivot + (vertexWorldOffset + spriteLocalPos) * 0.8;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.worldPos = worldPosition;

				float3 localNormal = normalize(skinStrokeData.localPos);
				localNormal = normalize(float3(localNormal.x * critterInitData.boundingBoxSize.y, localNormal.y * critterInitData.boundingBoxSize.x, localNormal.z * critterInitData.boundingBoxSize.y));
				
				float3 worldNormal = GetAnimatedPosOld(localNormal, float3(0,0,0), critterInitData, critterSimData, skinStrokeData.localPos); //skinStrokeData.localPos;
				o.worldNormal = normalize(worldNormal);

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

				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0));
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV;

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
				
				o.color = float4(1, 1, 1, 0);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				CritterInitData critterInitData = critterInitDataCBuffer[i.bufferIndices.y];

				float2 screenUV = i.screenUV.xy / i.screenUV.w;
				float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	


				float4 texColor = tex2D(_MainTex, i.spriteUV);
				float4 patternSample = tex2Dlod(_PatternTex, float4(i.patternUV, 0, 1));	

				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float diffuseLight = saturate(dot(lightDir, normalize(i.worldNormal)));
				
				float altitude = tex2D(_AltitudeTex, i.altitudeUV); // i.worldPos.z / 10; // [-1,1] range

				float4 finalColor = float4(lerp(critterInitData.primaryHue, critterInitData.secondaryHue, patternSample.x), texColor.a);

				float3 surfaceNormal = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
				float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;
								
				float4 backgroundColor = frameBufferColor;
				backgroundColor.a = texColor.a;
				
				
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

				float fogAmount = 0.05; //saturate((i.worldPos.z + 1) * 0.5);
				
				// Health & Energy:::: **
				float health = i.color.x;
				float energy = i.color.y;
				finalColor.rgb = lerp(float3(1,0,0), finalColor.rgb, saturate(health * 2.0) * 0.5 + 0.5);
				finalColor.rgb = lerp(float3(0.4,0.4,0.4), finalColor.rgb, saturate(energy * 2.0));
				
				// FAKE CAUSTICS:::				
				//dotLight *= saturate(diffuseLight * 2.5);
				//finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (dotLight * 0.6 + 0.4) + dotLight * 1.1, isUnderwater); //dotLight * 1.0;

				// Egg EmbryoFade
				finalColor.a *= i.color.b;

				finalColor.rgb *= diffuseLight * 0.67 + 0.33;

				// Decay
				finalColor.rgb = lerp(finalColor.rgb, backgroundColor, i.color.a * 0.67);

				return finalColor;

			}
			ENDCG
		}
	}
}
