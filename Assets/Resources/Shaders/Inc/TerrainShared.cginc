uniform float _Turbidity;
uniform float _CausticsStrength;
uniform float _MinFog;
uniform float4 _FogColor;

float4 GetGroundColor(float3 worldPos, float4 frameBufferColor, float4 altitudeTex, float4 waterSurfaceTex, float4 resourceTex) {
	float turbidity = _Turbidity;// saturate((resourceTex.x + resourceTex.y) * 0.95); //1; //0.33;
	float causticsStrength = lerp(0.025, 0.275, _Turbidity);
	float minFog = 0.06125;
	
	float4 finalColor = frameBufferColor;

	// Final fog color?  algae+nutrients for fog color, decomposers+detritus for ground color -- separate them!! ******
	//algae = green
	//nutrients = yellow/orange
	//decomposers = pink?
	//detritus = black/grey/brown
					
	float altitude = altitudeTex.x; // tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
	// 0-1 range --> -1 to 1
	altitude = (altitude * 2 - 1) * -1;
	float isUnderwater = saturate(altitude * 10000);
	//float3 waterFogColor = float3(0.3, 0.45, 0.86); // float3(0.95, 0.33, 0.025) * resourceTex.x + float3(0.33, 1, 0.1) * resourceTex.y;
	//waterFogColor = lerp(waterFogColor, float3(0.875, 0.33, 0.045) * 0.9, resourceTex.x);
	//waterFogColor = lerp(waterFogColor, float3(0.4, 0.9, 0.27) * (altitudeTex.x + 0.5), resourceTex.y);
	float3 waterFogColor = _FogColor.rgb;
	//float3 waterFogColor = lerp(_FogColor.rgb, float3(0.95,0.65,0.02), resourceTex.g * 0.75); // * saturate(resourceTex.r * 3); // float3(0.03,0.4,0.24) * 0.4;
	float strataColorMultiplier = (sin(altitude * (1.0 + worldPos.x * 0.01 - worldPos.y * -0.01) + worldPos.x * 0.01 - worldPos.y * 0.01) * 0.5 + 0.5) * 0.5 + 0.5;
				
	finalColor.rgb *= strataColorMultiplier;				
				
	float snowAmount = saturate((-altitude - 0.6) * 2 +
						((sin(worldPos.x * 0.0785 + worldPos.y * 0.02843) * 0.5 + 0.5) * 1 - 
						(cos(worldPos.x * 0.012685 + worldPos.y * -0.01843) * 0.5 + 0.5) * 0.9 +
						(sin(worldPos.x * 0.2685 + worldPos.y * -0.1843) * 0.5 + 0.5) * 0.45 - 
						(cos(worldPos.x * -0.2843 + worldPos.y * 0.01143) * 0.5 + 0.5) * 0.45 +
						(sin(worldPos.x * 0.1685 + worldPos.y * -0.03843) * 0.5 + 0.5) * 0.3 - 
						(cos(worldPos.x * -0.1843 + worldPos.y * 0.243) * 0.5 + 0.5) * 0.3) * 0.5);
				
	finalColor.rgb = lerp(finalColor.rgb, float3(0.56, 1, 0.34) * 0.6, snowAmount * 1);
	
	finalColor.rgb = lerp(finalColor.rgb, float3(1,1,0), resourceTex.w);  // Detritus Discoloration!
	
	// FAKE CAUSTICS:::
	float3 surfaceNormal = waterSurfaceTex.yzw; // // pre-calculated // //tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
	float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
	dotLight = dotLight * dotLight;
	//finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (dotLight * 0.33 + 0.67) * causticsStrength + dotLight * 0.75 * causticsStrength, isUnderwater * (1.0 - altitude));

	float depthNormalized = saturate((1.0 - altitudeTex.x) - 0.5) * 2;
	depthNormalized *= turbidity;
	depthNormalized = saturate(depthNormalized);

	// Wetness darkening:
	float wetnessMask = saturate(((altitudeTex.x + waterSurfaceTex.x * 0.05) - 0.5) * 8.25);
	finalColor.rgb *= (0.6 + wetnessMask * 0.4);
	
	//finalColor.rgb *= saturate((1.0 - depthNormalized) + dotLight * isUnderwater * (1.0 - depthNormalized) * causticsStrength) - isUnderwater * 0.05; // darken
	//finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (dotLight * 0.33 + 0.67), isUnderwater * depthNormalized);

	finalColor.rgb += dotLight * isUnderwater * (1.0 - depthNormalized) * causticsStrength;		
				
	// FOG:	
	finalColor.rgb = lerp(finalColor.rgb, waterFogColor, (saturate(depthNormalized + minFog) * isUnderwater));
	//finalColor.rgb = lerp(finalColor.rgb, waterFogColor, 1 * saturate(2 * (saturate(altitude * (1.0 - turbidity) )) + 0.5 * isUnderwater) * (turbidity));
	//finalColor.rgb *= 0.9;
	
	//return float4(1,1,1,1);
	return finalColor;

	
	
}
