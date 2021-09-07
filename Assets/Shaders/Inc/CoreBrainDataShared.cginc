#include "Assets/Shaders/Inc/NoiseShared.cginc"

struct NeuronInitData {
    float radius;
    float type;  // in/out/hidden
    float age;
};
struct NeuronFeedData {
    float curValue;  // [-1,1]  // set by CPU continually
};
struct NeuronSimData {
    float3 pos;
};
struct AxonInitData {  // set once at start
    float weight;
    int fromID;
    int toID;
};
struct AxonSimData {
    float3 p0;
    float3 p1;
    float3 p2;
    float3 p3;
    float pulsePos;
};
struct CableInitData {
    int socketID;
    int neuronID;
};
struct CableSimData {
    float3 p0;
    float3 p1;
    float3 p2;
    float3 p3;
};
struct SocketInitData {
    float3 pos;
};

struct CurveSample {
	float3 origin;
	float3 normal;
	float3 right;
	float3 up;	
	float3 forward;
};

StructuredBuffer<NeuronInitData> neuronInitDataCBuffer;
StructuredBuffer<NeuronFeedData> neuronFeedDataCBuffer;
RWStructuredBuffer<NeuronSimData> neuronSimDataCBuffer;
StructuredBuffer<AxonInitData> axonInitDataCBuffer;
RWStructuredBuffer<AxonSimData> axonSimDataCBuffer;
StructuredBuffer<SocketInitData> socketInitDataCBuffer;
StructuredBuffer<CableInitData> cableInitDataCBuffer;
RWStructuredBuffer<CableSimData> cableSimDataCBuffer;

// Core Sizes:
float minNeuronRadius = 0.05;
float maxNeuronRadius = 0.5;
float minAxonRadius = 0.05;
float maxAxonRadius = 0.5;
float minSubNeuronScale = 0.25;
float maxSubNeuronScale = 0.75;  // max size relative to parent Neuron
float minAxonFlareScale = 0.2;
float maxAxonFlareScale = 0.9;  // max axon flare size relative to SubNeuron
float axonFlarePos = 0.92;
float axonFlareWidth = 0.08;
float axonMaxPulseMultiplier = 2.0;
float cableRadius = 0.05;

// Noise Parameters:
float neuronExtrudeNoiseFreq = 1.5;
float neuronExtrudeNoiseAmp = 0.0;
float neuronExtrudeNoiseScrollSpeed = 0.6;
float axonExtrudeNoiseFreq = 0.33;
float axonExtrudeNoiseAmp = 0.33;
float axonExtrudeNoiseScrollSpeed = 1.0;
float axonPosNoiseFreq = 0.14;
float axonPosNoiseAmp = 0;
float axonPosNoiseScrollSpeed = 10;
float axonPosSpiralFreq = 20.0;
float axonPosSpiralAmp = 0;

// Forces:
float neuronAttractForce = 0.004;
float neuronRepelForce = 2.0;
float axonPerpendicularityForce = 0.01;
float axonAttachStraightenForce = 0.01;
float axonAttachSpreadForce = 0.025;
float axonRepelForce = 0.2;
float cableAttractForce = 0.01;

float time = 0.0;

// CUBIC:
float3 GetPoint (float3 p0, float3 p1, float3 p2, float3 p3, float t) {
	t = saturate(t);
	float oneMinusT = 1.0 - t;
	return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
}
// CUBIC
float3 GetFirstDerivative (float3 p0, float3 p1, float3 p2, float3 p3, float t) {
	t = saturate(t);
	float oneMinusT = 1.0 - t;
	return 3.0 * oneMinusT * oneMinusT * (p1 - p0) + 6.0 * oneMinusT * t * (p2 - p1) + 3.0 * t * t * (p3 - p2);
}

float GetNeuronRadius(int neuronID, float3 dir) {

	float interp = smoothstep(0, 1, abs(neuronFeedDataCBuffer[neuronID].curValue));
	float radius = lerp(minNeuronRadius, maxNeuronRadius, interp);
	
	float3 extrudeNoiseOffset = float3(0.14, 0.73, 0.52) * time * neuronExtrudeNoiseScrollSpeed;

	float noiseExtrude = (Value3D(dir + neuronID + extrudeNoiseOffset, neuronExtrudeNoiseFreq).x + 0.25) * radius;  // add neuronID so noise varies from neuron to neuron
	radius += noiseExtrude * neuronExtrudeNoiseAmp;

	return radius;
}

float GetAxonRadius(int axonID, float t, float angle) {
	
	float weight = axonInitDataCBuffer[axonID].weight;
	float baseRadius = abs(weight) * (maxAxonRadius - minAxonRadius) + minAxonRadius;

	float3 extrudeNoiseOffset = float3(0.14, 0.73, 0.52) * time * axonExtrudeNoiseScrollSpeed;
	float noiseExtrude = (Value3D(float3(axonID, t, angle) + extrudeNoiseOffset, axonExtrudeNoiseFreq).x + 0.25) * baseRadius;

	baseRadius += clamp(noiseExtrude * axonExtrudeNoiseAmp, 0, 10);

	float neuronRadius0 = GetNeuronRadius(axonInitDataCBuffer[axonID].fromID, float3(0,1,0));
	float neuronRadius1 = GetNeuronRadius(axonInitDataCBuffer[axonID].toID, float3(0,1,0));
	float closestNeuronRadius = lerp(neuronRadius0, neuronRadius1, round(t));

	float distToSideScreenEdge = (0.5 - min((1.0 - t), t)) * 2.0;           // 1 at edge, 0.0 at middle of spline
	//float flareLength = 0.1;
	//float flareMask = saturate(distToSideScreenEdge - (1.0 - flareLength)) * (1.0 / flareLength);      // 0.0 --> 0.1 * 10 == 0->1,   1 at edge
	
	float distToInflectionPoint = abs(distToSideScreenEdge - axonFlarePos);
	float flareMask = smoothstep((axonFlarePos - axonFlareWidth), (axonFlarePos + axonFlareWidth), distToSideScreenEdge);
	
	float pulseDistance = abs(t - axonSimDataCBuffer[axonID].pulsePos);
	float pulseMultiplier = 1.0 - smoothstep(0, 0.2, pulseDistance);  // 0->1
	pulseMultiplier = pulseMultiplier * axonMaxPulseMultiplier + 1.0;  // [1,1+axonMaxPulseMultiplier]

	baseRadius *= pulseMultiplier;

	float edgeRadius = lerp(max(minAxonRadius, closestNeuronRadius * (maxSubNeuronScale * minAxonFlareScale)), closestNeuronRadius * (maxSubNeuronScale * maxAxonFlareScale), abs(axonInitDataCBuffer[axonID].weight));
	edgeRadius += clamp(noiseExtrude * axonExtrudeNoiseAmp, 0, 10);
	
	baseRadius = lerp(baseRadius, edgeRadius, flareMask);
	
	return baseRadius;
}

CurveSample GetAxonSample(int axonID, float t, float angle) {

	CurveSample curveSample;

	float3 ringOrigin = GetPoint(axonSimDataCBuffer[axonID].p0, axonSimDataCBuffer[axonID].p1, axonSimDataCBuffer[axonID].p2, axonSimDataCBuffer[axonID].p3, t);

	float3 noiseOffset = float3(0.5, 0.25, 0.9) * time * axonPosNoiseScrollSpeed;
	float distToSideScreenEdge = (0.5 - min((1.0 - t), t)) * 2.0;           // 0.5 at edge, 0.0 at middle of spline
	ringOrigin += Value3D(ringOrigin + noiseOffset, axonPosNoiseFreq).yzw * axonPosNoiseAmp * (1.0 - distToSideScreenEdge); // noise masked at ends

	float3 forward = normalize(GetFirstDerivative(axonSimDataCBuffer[axonID].p0, axonSimDataCBuffer[axonID].p1, axonSimDataCBuffer[axonID].p2, axonSimDataCBuffer[axonID].p3, t));
	float3 right = normalize(cross(forward, float3(0.0, 1.0, 0.0)));
	float3 up = normalize(cross(right, forward));

	// Spiral Offset!!!:::::
	float spiralRight = cos(t * axonPosSpiralFreq) * axonPosSpiralAmp;
	float spiralUp = sin(t * axonPosSpiralFreq) * axonPosSpiralAmp;
	ringOrigin += (right * spiralRight + up * spiralUp) * (1.0 - distToSideScreenEdge); // noise masked at ends;

	float x = cos((angle) * 2.0 * 3.14159);
	float y = sin((angle) * 2.0 * 3.14159);
		
	curveSample.normal = right * x + up * y;
	curveSample.origin = ringOrigin;
	curveSample.right = right;
	curveSample.up = up;
	curveSample.forward = forward;
	
	return curveSample;
}

float GetCableRadius(int ID, float t, float angle) {
	float radius = cableRadius;
	float edgeRadius = cableRadius * 4;

	float flarePos = 0.9;
	float flareWidth = 0.03;
	float distToSideScreenEdge = (0.5 - min((1.0 - t), t)) * 2.0;           // 1 at edge, 0.0 at middle of spline	
	float distToInflectionPoint = abs(distToSideScreenEdge - flarePos);
	float flareMask = smoothstep((flarePos - flareWidth), (flarePos + flareWidth), distToSideScreenEdge);
	
	//float pulseDistance = abs(t - axonSimDataCBuffer[axonID].pulsePos);
	//float pulseMultiplier = 1.0 - smoothstep(0, 0.2, pulseDistance);  // 0->1
	//pulseMultiplier = pulseMultiplier * axonMaxPulseMultiplier + 1.0;  // [1,1+axonMaxPulseMultiplier]
	//baseRadius *= pulseMultiplier;

	//float edgeRadius = lerp(max(minAxonRadius, closestNeuronRadius * (maxSubNeuronScale * minAxonFlareScale)), closestNeuronRadius * (maxSubNeuronScale * maxAxonFlareScale), abs(axonInitDataCBuffer[axonID].weight));
	//edgeRadius += clamp(noiseExtrude * axonExtrudeNoiseAmp, 0, 10);
	
	radius = lerp(radius, edgeRadius, flareMask);



	//float radius = cableRadius;
	return radius;
	
}

CurveSample GetCableCurveSample(int ID, float t, float angle) {

	CurveSample curveSample;

	float3 ringOrigin = GetPoint(cableSimDataCBuffer[ID].p0, cableSimDataCBuffer[ID].p1, cableSimDataCBuffer[ID].p2, cableSimDataCBuffer[ID].p3, t);	

	float3 forward = normalize(GetFirstDerivative(cableSimDataCBuffer[ID].p0, cableSimDataCBuffer[ID].p1, cableSimDataCBuffer[ID].p2, cableSimDataCBuffer[ID].p3, t));
	float3 right = normalize(cross(forward, float3(0.0, 1.0, 0.0)));
	float3 up = normalize(cross(right, forward));
	
	float x = cos((angle) * 2.0 * 3.14159);
	float y = sin((angle) * 2.0 * 3.14159);
		
	curveSample.normal = right * x + up * y;
	curveSample.origin = ringOrigin;
	curveSample.right = right;
	curveSample.up = up;
	curveSample.forward = forward;
	
	return curveSample;
}