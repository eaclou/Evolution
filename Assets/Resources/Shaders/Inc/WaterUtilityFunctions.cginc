
float GetReflectionLerpSml(float3 worldPos, float3 surfaceNormal, float viewDot, float camDistNormalized, float2 camFocusPositionXY, float vignetteRadius) {
	float waveDot = dot(normalize(float3(0,-0.05,-1)), surfaceNormal);
	
	float waveMaskNear = (1.0 - saturate((waveDot - 0.96) * 25)) * 0.99 + 0.03;
	float waveMaskFar = (1.0 - saturate((waveDot - 0.96) * 25)) * 0.24;
	float waveMask = lerp(waveMaskNear, waveMaskFar, camDistNormalized);
	
	float addMaskNear = saturate((vignetteRadius - 0.5) * 1.26) * 1.2;
	float addMaskFar = (1.0 - saturate((viewDot - 0.56) * 2.4)) * 0.95 + saturate((vignetteRadius + 0.5) * 0.35) * 0.57;
	float addMask = saturate(lerp(addMaskNear, addMaskFar, camDistNormalized));
	
	float subtractMaskNear = saturate((vignetteRadius + 0.75) * 0.6) * 0.5 + saturate((vignetteRadius + 0.25) * 4) * 0.5;
	float subtractMaskFar = saturate((vignetteRadius - 0.5) * 0.95) * 0.35;
	float subtractMask = lerp(subtractMaskNear, subtractMaskFar, camDistNormalized);

	float maxReflectionStr = lerp(0.2, 0.5, camDistNormalized);

	float finalMask = saturate(waveMask + addMask - subtractMask) * maxReflectionStr;

	// 0 = clear, 1 = REFLECTION FULL
	return finalMask;
	
}

float GetReflectionLerpLrg(float3 worldPos, float3 surfaceNormal, float viewDot, float camDistNormalized, float2 camFocusPositionXY, float vignetteRadius) {
	float waveDot = dot(normalize(float3(0,-0.05,-1)), surfaceNormal);
	
	float waveMaskNear = (1.0 - saturate((waveDot - 0.96) * 25)) * 0.3;
	float waveMaskFar = (1.0 - saturate((waveDot - 0.96) * 25)) * 0.4;
	float waveMask = lerp(waveMaskNear, waveMaskFar, camDistNormalized);
	
	float addMaskNear = saturate((vignetteRadius - 0.5) * 1.26) * 1.35;
	float addMaskFar = (1.0 - saturate((viewDot - 0.5) * 2.0)) * 1.1 + saturate((vignetteRadius + 0.5) * 0.45) * 0.5;
	float addMask = saturate(lerp(addMaskNear, addMaskFar, camDistNormalized));
	
	float subtractMaskNear = saturate((vignetteRadius + 0.75) * 0.6) * 0.96 + saturate((vignetteRadius + 0.25) * 4) * 0.6 + 0.07;
	float subtractMaskFar = saturate((vignetteRadius - 0.5) * 0.75) * 0.395;
	float subtractMask = lerp(subtractMaskNear, subtractMaskFar, camDistNormalized);

	float maxReflectionStr = lerp(0.2, 0.5, camDistNormalized);

	float finalMask = saturate(waveMask + addMask - subtractMask) * maxReflectionStr;

	// 0 = clear, 1 = REFLECTION FULL
	return finalMask;
	
}