// REQUIRES ALSO INCLUDING NOISESHARED.cginc !!!!

float GetRemappedValue01(float3 inputValue, float noiseOffset, float numStrata) {
	

	float horFreq = 0.25;
	float horAmplitude = 0;
	float2 horInputValue = float2(inputValue.x * horFreq, inputValue.z * horFreq);
	float altitudeValue = inputValue.y; // + inputValue.x * 0.6 - inputValue.z * 0.4;
	float horNoiseSample = Value2D(horInputValue, numStrata).y;
	
	float verNoiseSample = saturate(Value1D(altitudeValue, numStrata * 0.5).y);

	float horRemappedValue = saturate(horNoiseSample) * (1 / (numStrata-1)) * horAmplitude;
	float verRemappedValue = verNoiseSample * (0.5 / (numStrata-1));
	
	//return saturate(inputValue.y);
	return saturate(altitudeValue + verRemappedValue);
}

float GetRemappedAltitude(float3 pos, float minAltitude, float maxAltitude) {
	float slopeX = 1.1;
	float slopeZ = -1.06;

	//float normalizedAltitude = (pos.y - minAltitude) / (maxAltitude - minAltitude);
	
	return pos.y;
	//return pos.y + slopeX * pos.x + slopeZ * pos.z;
}
