
float3 RotatePoint(float3 position, float4 rotation) {
	float3 t = 2 * cross(rotation.xyz, position);
	return (position + rotation.w * t + cross(rotation.xyz, t));
}

float DistancePointToBox(float3 pos, float3 boxPos, float3 boxScale, float4 boxRotation, float extrudeAmount) {

	float3 boxCenterToPoint = pos - boxPos + float3(0.00001, 0.00001, 0.00001);  // prevent divide by 0
	float3 right = RotatePoint(float3(1.0, 0, 0), boxRotation);
	float3 up = RotatePoint(float3(0, 1.0, 0), boxRotation);
	float3 forward = RotatePoint(float3(0, 0, 1.0), boxRotation);

	float dotRight = dot(right, boxCenterToPoint);
	float dotUp = dot(up, boxCenterToPoint);
	float dotForward = dot(forward, boxCenterToPoint);

	// Cache distance amounts per box-local-dimensions:
	float dotRightAbs = abs(dotRight);
	float dotRightSign = dotRight / dotRightAbs;
	float dotUpAbs = abs(dotUp);
	float dotUpSign = dotUp / dotUpAbs;
	float dotForwardAbs = abs(dotForward);
	float dotForwardSign = dotForward / dotForwardAbs;

	float minDistance = 0.001;  // prevent divide by 0 and negative distances
								//float roundness = 1; // multiplier on boxScale
								//float extrudeAmount = 0.25;

								// find point on box 'edge' to measure against samplepoint:
	float distRight = min(dotRightAbs, max(boxScale.x - extrudeAmount, 0.0)) * dotRightSign; // amount to move in box's local X direction
	float distUp = min(dotUpAbs, max(boxScale.y - extrudeAmount, 0.0)) * dotUpSign;
	float distForward = min(dotForwardAbs, max(boxScale.z - extrudeAmount, 0.0)) * dotForwardSign;

	float3 offsetVector = pos - (right * distRight + up * distUp + forward * distForward + boxPos);

	float distance = max(minDistance, length(offsetVector));

	return distance;
}

float DistancePointToPoint(float3 pos, float3 boxPos) {
	float3 boxCenterToPoint = pos - boxPos + float3(0.0001, 0.0001, 0.0001);  // prevent divide by 0

	float distance = length(boxCenterToPoint);

	return distance;
}