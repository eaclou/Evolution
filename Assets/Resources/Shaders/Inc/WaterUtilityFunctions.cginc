
float GetReflectionLerp(float3 worldPos, float3 surfaceNormal, float viewDot, float camDistNormalized, float2 camFocusPositionXY, float vignetteRadius) {
	float waveDot = dot(normalize(float3(0,-0.05,-1)), surfaceNormal);
	
	float2 vertexToFocus = camFocusPositionXY - worldPos.xy;
	vertexToFocus.x *= 1.65;  // 16:9 aspect
	float distToFocusPos = length(vertexToFocus);

	float waveMaskNear = (1.0 - saturate((waveDot - 0.96) * 25)) * 0.6 + 0.3;
	float waveMaskFar = (1.0 - saturate((waveDot - 0.96) * 25)) * 0.2;
	float waveMask = lerp(waveMaskNear, waveMaskFar, camDistNormalized);
	
	float addMaskNear = saturate((vignetteRadius - 0.5) * 1.26) * 1.3;
	float addMaskFar = (1.0 - saturate((viewDot - 0.6) * 2.5)) * 0.85 + saturate((vignetteRadius + 0.5) * 0.25) * 0.5;
	float addMask = saturate(lerp(addMaskNear, addMaskFar, camDistNormalized));
	
	float subtractMaskNear = saturate((vignetteRadius + 0.75) * 0.6) * 1.25 + saturate((vignetteRadius + 0.25) * 4) * 0.33;
	float subtractMaskFar = saturate((vignetteRadius - 0.5) * 0.35) * 0.15;
	float subtractMask = lerp(subtractMaskNear, subtractMaskFar, camDistNormalized);

	/*
	float viewMask = viewDot;	
	float vignetteStart = lerp(1, 0.2, camDistNormalized);
	float vignetteEnd = lerp(2, 1, camDistNormalized);
	float vignetteStrength = camDistNormalized;
	float remappedVignette = saturate(saturate(vignetteRadius - vignetteStart) / (vignetteEnd - vignetteStart));
	float vignetteMask = remappedVignette;
	*/
	float maxReflectionStr = 0.54;

	float finalMask = saturate(waveMask + addMask - subtractMask) * maxReflectionStr;

	// 0 = clear, 1 = REFLECTION FULL
	return finalMask;
	
}

float GetReflectionVignette() {

	return 1.0;
}