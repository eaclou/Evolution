uniform float _Turbidity;
uniform float _CausticsStrength;
uniform float _MinFog;
uniform float4 _FogColor;
uniform float4 _DecomposersColor;
uniform float4 _DetritusColor;

struct ShadingData {
    float4 baseAlbedo;
	float4 altitudeTex;
    float4 waterSurfaceTex;
	float4 groundNormalsTex;
    float4 resourceGridTex;
	float4 spiritBrushTex;
	float4 skyTex;
	float3 worldPos;
	float maxAltitude;
	float4 waterFogColor;
	float4 sunDir;
	float4 worldSpaceCameraPosition;
	float globalWaterLevel;
	float causticsStrength;
};

float GetDepthNormalized(float rawAltitude) {
	float depthNormalized = saturate((1.0 - rawAltitude) - 0.5) * 2;
	depthNormalized *= _Turbidity;
	return saturate(depthNormalized);
}

float3 ApplyWaterFogColor(float3 sourceColor, float rawAltitude) {
	float depthNormalized = GetDepthNormalized(rawAltitude);
	float altitude = (rawAltitude * 2 - 1) * -1;
	float isUnderwater = saturate(altitude * 10000);
	float3 waterFogColor = _FogColor.rgb;
	
	return lerp(sourceColor, waterFogColor, (saturate(depthNormalized + _MinFog) * isUnderwater));
}

float4 MasterLightingModel(ShadingData shadingData) {

	//return float4(1,1,1,1);
	// What information is needed and when in order to properly render??? *******
	// Ground Terrain baseAlbedo color? -- either precompute or blend btw stone/pebble/sand colors
	// Height -- Z-Pos of particle
	// Resource distribution tinting -- what colors?  :: algae, decomposers, waste, nutrients
	// Caustics lighting
	// Water Fog amount
	// depth
	// Water fog color
	// global water level
	// terrain normals
	// skyTexture
	// Camera worldPos
	// Sun Pos/Dir
	// SpiritBrush lighting


	/// ponderings::
	//  Inputs:  BaseAlbedo --> spits out lit value
	//			 worldPos xyz
	//			 Textures! - altitude, water, (resources?), spiritBrushLight, SkyTex
	//			 material properties?

			
	float4 altitudeTex = shadingData.altitudeTex;	
	float4 waterSurfaceTex = shadingData.waterSurfaceTex;
	float4 resourceTex = shadingData.resourceGridTex;
	float4 spiritBrushTex = shadingData.spiritBrushTex;
		
	float4 finalColor = shadingData.baseAlbedo;   // INPUT COLOR:	

	float causticsStrength = shadingData.causticsStrength;
	float minFog = shadingData.waterFogColor.a;
/*
	float3 decomposerHue = float3(0.8,0.3,0);
	float decomposerMask = saturate(resourceTex.z * 1) * 0.8;
	float3 detritusHue = float3(0.2,0.1,0.02);
	float detritusMask = saturate(resourceTex.y * 1) * 0.8;
	float3 algaeColor = float3(0.5,0.8,0.5) * 0.5;
	float algaeMask = saturate(resourceTex.w * 2.70);
*/
				
	float altitudeRaw = altitudeTex.x;
	
	float3 waterFogColor = shadingData.waterFogColor.rgb;
	
	//waterFogColor = lerp(waterFogColor, algaeColor, algaeMask);

	float3 sunDir = shadingData.sunDir.xyz;

	// FAKE CAUSTICS:::
	float3 waterSurfaceNormal = shadingData.waterSurfaceTex.yzw; // pre-calculated
	float dotLight = dot(waterSurfaceNormal, sunDir);
	dotLight = dotLight * dotLight;
	
	float altitude = altitudeRaw;								
	float depth = saturate(-altitude + shadingData.globalWaterLevel);  // 0-1 values
	float isUnderwater = saturate(depth * 50);
					
	//=============================//Diffuse				
	float3 groundSurfaceNormal = shadingData.groundNormalsTex.yzw;
	float3 diffuseSurfaceNormal = lerp(groundSurfaceNormal, waterSurfaceNormal, depth);
	float dotDiffuse = dot(diffuseSurfaceNormal, sunDir);
	float diffuseWrap = dotDiffuse * 0.5 + 0.5;
	finalColor.rgb *= (0.7 + dotDiffuse * 0.33 + 0.081 * diffuseWrap);

// ********* Are these two specific to ground strokes only?? ***
	// Wetness darkening:
	float wetnessMask = 1.0 - saturate((-altitude + shadingData.globalWaterLevel + 0.05) * 17.5); 
	finalColor.rgb *= (0.3 + wetnessMask * 0.7);
	// shoreline foam:
	float foamMask = 1.0 - saturate((abs(-altitude + shadingData.globalWaterLevel) * 57));
	finalColor.rgb += foamMask * 0.3675;
// ********* Are these two specific to ground strokes only?? ***

	// Caustics
	finalColor.rgb += dotLight * isUnderwater * (1.0 - depth) * shadingData.causticsStrength;		
	
	// FOG:	
	float fogAmount = lerp(0, 1, depth * 2);
	finalColor.rgb = lerp(finalColor.rgb, waterFogColor, fogAmount * isUnderwater);
		
	// Reflection!!!
	float3 worldPos = shadingData.worldPos; ////////////////// float3(i.altitudeUV * _MapSize, -altitude * _MaxAltitude);
	float3 cameraToVertex = worldPos - shadingData.worldSpaceCameraPosition.xyz;
	float3 cameraToVertexDir = normalize(cameraToVertex);
	float3 reflectedViewDir = cameraToVertexDir + 2 * waterSurfaceNormal * 0.5;
	float viewDot = 1.0 - saturate(dot(-cameraToVertexDir, waterSurfaceNormal));
					
	//float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;
	float4 skyTex = shadingData.skyTex; ////////////////////////tex2D(_SkyTex, skyCoords);
	float4 reflectedColor = float4(skyTex.rgb, finalColor.a);
	
	viewDot * 0.3 + 0.2;		
	float reflectLerp = saturate(viewDot * isUnderwater);
	finalColor.rgb += lerp(float3(0,0,0), reflectedColor.xyz, reflectLerp);

	finalColor.rgb += shadingData.spiritBrushTex.y;
				
	finalColor.a = 1; 

	return finalColor;
}

float3 ApplyCausticsLight(float3 sourceColor, float4 waterSurfaceTex, float rawAltitude) {
	
	float causticsStrength = lerp(0.02, 0.275, _Turbidity);
	
	// FAKE CAUSTICS:::
	float3 surfaceNormal = waterSurfaceTex.yzw;
	float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
	dotLight = dotLight * dotLight;

	float depthNormalized = saturate((1.0 - rawAltitude) - 0.5) * 2;
	depthNormalized *= _Turbidity;
	depthNormalized = saturate(depthNormalized);

	float altitude = (rawAltitude * 2 - 1) * -1;
	float isUnderwater = saturate(altitude * 10000);

	sourceColor += dotLight * isUnderwater * (1.0 - depthNormalized) * causticsStrength;		

	return sourceColor;
}

// can this be broken up into modules?

float4 GetEnvironmentColor(float3 worldPos, float4 terrainColorTex, float4 altitudeTex, float4 waterSurfaceTex, float4 resourceTex, float4 skyTex, float globalWaterLevel) {
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
	
	float3 waterFogColor = float3(0.36, 0.4, 0.44) * 0.42; // _FogColor.rgb;
	
	// FAKE CAUSTICS:::
	float3 surfaceNormal = waterSurfaceTex.yzw; // pre-calculated
	float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
	dotLight = dotLight * dotLight;
	
	
	float altitude = altitudeTex.x + waterSurfaceTex.x * 0.05;						
	float depth = saturate(-altitude + globalWaterLevel);
	float isUnderwater = saturate(depth * 50);

	// Wetness darkening:
	float wetnessMask = saturate(depth * 45.5); 
	outColor.rgb *= wetnessMask; //(0.4 + wetnessMask * 0.6);
	
	// Caustics
	outColor.rgb += dotLight * isUnderwater * (1.0 - depth) * causticsStrength;		
	
	//Diffuse 
	float3 sunDir = normalize(float3(1,1,-1));
	float3 waterSurfaceNormal = waterSurfaceTex.yzw;
	
	float dotDiffuse = saturate(dot(waterSurfaceNormal, sunDir));
	outColor.rgb *= dotDiffuse;

	// FOG:	
	float fogAmount = lerp(0, 1, depth);
	outColor.rgb = lerp(outColor.rgb, waterFogColor, fogAmount * isUnderwater); 
		
	// Reflection!!!
	
	float3 cameraToVertex = worldPos - _WorldSpaceCameraPos;
    float3 cameraToVertexDir = normalize(cameraToVertex);
	float3 reflectedViewDir = cameraToVertexDir + 2 * waterSurfaceNormal * 0.05;
	float viewDot = dot(-cameraToVertexDir, waterSurfaceNormal);

	//float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;
	// Have to sample SkyTexture in displayShader????

	
	//float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;
	float4 reflectedColor = float4(skyTex.rgb, outColor.a); //col;
	
				
	float reflectLerp = saturate(viewDot * viewDot * 0.35 * isUnderwater);
	outColor.rgb += lerp(float3(0,0,0), reflectedColor, reflectLerp);

	//outColor.rgb = terrainColorTex;
	return outColor;
}

// Would it make more sense to Pre-compute color within UpdateTerrainStrokes compute process, then just display
//  If pre-computed, then other layers can sample a single texture to "know" the correct background color???
// Cloudiness/sediment! global water level!
// i'm already rebuilding it every frame..... might as well re-use  -- bake in lighting etc.?
// How to mix??? Test appearnce with Critters -- how to get proper water fog contribution
                                            //  frameBufferColor replaced by terrain baseAlbedo??
float4 GetGroundColor(float3 worldPos, float4 frameBufferColor, float4 altitudeTex, float4 waterSurfaceTex, float4 resourceTex) {
	float turbidity = _Turbidity;  
	float causticsStrength = lerp(0.025, 0.275, 0.6); //_Turbidity);
	float minFog = 1;
		
	
	float4 finalColor = frameBufferColor;
	float3 decomposerHue = float3(0.8,0.3,0);
	float decomposerMask = saturate(resourceTex.z * 1) * 0.8;
	float3 detritusHue = float3(0.2,0.1,0.02);
	float detritusMask = saturate(resourceTex.y * 1) * 0.8;
	
	finalColor.rgb = lerp(finalColor.rgb, decomposerHue, decomposerMask);
	finalColor.rgb = lerp(finalColor.rgb, detritusHue, detritusMask);

	float algaeMask = saturate(resourceTex.w * 1.0);
	minFog = max(algaeMask * 1, minFog);
	
	float altitude = altitudeTex.x;
	// 0-1 range --> -1 to 1
	altitude = (altitude * 2 - 1) * -1;
	float isUnderwater = saturate(altitude * 100);
	
	float3 waterFogColor = float3(0.32, 0.5, 0.44) * 0.4; // _FogColor.rgb;
	
	// FAKE CAUSTICS:::
	float3 surfaceNormal = waterSurfaceTex.yzw; // pre-calculated
	float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
	dotLight = dotLight * dotLight;
	
	float depthNormalized = saturate((1.0 - altitudeTex.x) - 0.5) * 2;
	//depthNormalized *= turbidity;
	depthNormalized = saturate(depthNormalized);

	// Wetness darkening:
	float wetnessMask = saturate(((altitudeTex.x + waterSurfaceTex.x * 0.34) - 0.6) * 5.25);
	finalColor.rgb *= (0.6 + wetnessMask * 0.4);
	
	// Caustics
	finalColor.rgb += dotLight * isUnderwater * (1.0 - depthNormalized) * causticsStrength;		
	
	//Diffuse 
	float3 sunDir = normalize(float3(1,1,-1));
	float3 horizontalNormal = float3(0.01, 0, altitudeTex.y);
	float3 verticalNormal = float3(0, 0.01, altitudeTex.z);
	float3 groundNormal = normalize(cross(verticalNormal, horizontalNormal));
	float dotDiffuse = saturate(dot(groundNormal, sunDir));
	//finalColor.rgb += dotDiffuse * 0.1;

	// FOG:	
	float fogAmount = lerp(0, 1, depthNormalized);
	finalColor.rgb = lerp(finalColor.rgb, waterFogColor, fogAmount * isUnderwater); // (max(minFog, saturate(depthNormalized + algaeMask))

	
	finalColor.rgb += decomposerHue * decomposerMask * 0.025;
	
	//return float4(frameBufferColor.rgb,1);
	return finalColor;

	
	
}
