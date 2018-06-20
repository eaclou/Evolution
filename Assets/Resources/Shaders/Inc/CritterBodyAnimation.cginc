struct AgentMovementAnimData {
	float animCycle;
	float turnAmount;
	float accel;
	float smoothedThrottle;
};

float2 rotate_point(float2 pivot,float angle, float2 p)
{
	float2 rotatedPoint = p;
	float s = sin(angle);
	float c = cos(angle);

	// translate point back to origin:
	rotatedPoint -= pivot;
			  
	// rotate point
	float xnew = rotatedPoint.x * c - rotatedPoint.y * s;
	float ynew = rotatedPoint.x * s + rotatedPoint.y * c;

	rotatedPoint = float2(xnew, ynew);

	// translate point back:
	rotatedPoint += pivot;

	return rotatedPoint;
}

float2 getWarpedPoint(float2 originalPoint, float v, float turnAmount, float warpStrength, float pivot, float animCycle, float accel, float throttle) {
	// --------------Swim Anim:-----------------------
	float animSpeed = 30;
	float accelAnimSpeed = 55;
	float offsetMask = saturate(1 - v * 0.75);
				
	// panning yaw:
	float turningAngle = turnAmount;
	float2 warpedPoint = rotate_point(float2(0,pivot), clamp(turningAngle * -1, -1, 1) * offsetMask + warpStrength * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle, originalPoint);
				
	return warpedPoint;
}