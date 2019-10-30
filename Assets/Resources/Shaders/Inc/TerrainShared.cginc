uniform float _Turbidity;
uniform float _CausticsStrength;
uniform float _MinFog;
uniform float4 _FogColor;
uniform float4 _DecomposersColor;
uniform float4 _DetritusColor;


float4 GetEnvironmentColor(float3 worldPos) {

	return float4(1,1,1,1);
}

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

float4 GetEnvironmentColor(float3 worldPos, float4 terrainColorTex, float4 altitudeTex, float4 waterSurfaceTex, float4 resourceTex, float4 skyTex) {
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
	//minFog = max(algaeMask * 1, minFog);

	float altitudeRaw = altitudeTex.x;
	
	//float altitude = altitudeTex.x;
	// 0-1 range --> -1 to 1
	float worldSpaceZ = (altitudeRaw * 2 - 1) * -1;
	float isUnderwater = saturate(worldSpaceZ * 100);  // *** UPDATE!!!! ****
	
	float3 waterFogColor = float3(0.36, 0.4, 0.44) * 0.42; // _FogColor.rgb;
	
	// FAKE CAUSTICS:::
	float3 surfaceNormal = waterSurfaceTex.yzw; // pre-calculated
	float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
	dotLight = dotLight * dotLight;
	
	float depthNormalized = saturate((1.0 - altitudeRaw) - 0.5) * 2;	
	depthNormalized = saturate(depthNormalized); //  ????

	// Wetness darkening:
	float wetnessMask = saturate(((altitudeRaw + waterSurfaceTex.x * 0.34) - 0.6) * 5.25);
	outColor.rgb *= (0.6 + wetnessMask * 0.4);
	
	// Caustics
	outColor.rgb += dotLight * isUnderwater * (1.0 - depthNormalized) * causticsStrength;		
	
	//Diffuse 
	float3 sunDir = normalize(float3(1,1,-1));
	float3 waterSurfaceNormal = waterSurfaceTex.yzw;
	
	float dotDiffuse = saturate(dot(waterSurfaceNormal, sunDir));
	outColor.rgb *= dotDiffuse;

	// FOG:	
	float fogAmount = lerp(0, 1, depthNormalized);
	outColor.rgb = lerp(outColor.rgb, waterFogColor, fogAmount * isUnderwater); // (max(minFog, saturate(depthNormalized + algaeMask))
		
	//finalColor.rgb += decomposerHue * decomposerMask * 0.025;
	
	// Reflection!!!
	
	float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
    float3 cameraToVertexDir = normalize(cameraToVertex);
	float3 reflectedViewDir = cameraToVertexDir + 2 * waterSurfaceNormal * 0.05;
	//float viewDot = dot(-cameraToVertexDir, waterSurfaceNormal);

	float2 skyCoords = reflectedViewDir.xy * 0.5 + 0.5;
	// Have to sample SkyTexture in displayShader????

	float4 reflectedColor = float4(tex2Dlod(_SkyTex, float4((skyCoords) - _Time.y * 0.015, 0, 1)).rgb, finalColor.a); //col;
								
	float reflectLerp = saturate(i.vignetteLerp.x * 2);
	finalColor = lerp(finalColor, reflectedColor, reflectLerp);

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
