struct AgentMovementAnimData {
	float animCycle;
	float turnAmount;
	float accel;
	float smoothedThrottle;
};

float2 rotate_point(float2 pivot, float angle, float2 p)
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

float2 rotatePointVector(float2 p, float2 pivot, float2 forward) {
	float2 newPos = p - pivot;
	float2 right = float2(forward.y, -forward.x);
	newPos = newPos.x * right + newPos.y * forward;
	newPos += pivot;
	return newPos;
}

// scale to critter size
// keep pivot & billboard vertex offset separate?

float2 foodBloatAnimPos(float2 originalPos, float t, float foodAmount) {
	
	float bloatPivotY = (saturate(foodAmount - 0.5) - 0.5) * 2;
	float bloatMask = smoothstep(0, 1, (1 - saturate(abs((t - bloatPivotY) * 1))) * 1); // smoothstep makes a more gaussain-looking shape than pointy
	float bloatMagnitude = foodAmount * foodAmount;
	
	float2 newPos = originalPos * (1.0 + bloatMask * bloatMagnitude * 1.0);
	
	return newPos;
}

float2 biteAnimPos(float2 originalPos, float t, float biteAnimCycle) {
	
	float eatingCycle = sin(biteAnimCycle * 3.141592);
	float biteMask = saturate(t + 0.25);
	
	float2 newPos = originalPos * (1.0 + eatingCycle * 0.33f * biteMask);	
	newPos.y *= (eatingCycle * 0.15f * biteMask + 1.0f);
	newPos.x *= (1.0f - eatingCycle * 0.25f * biteMask);
	
	return newPos;
}

float2 swimAnimPos(float2 originalPos, float t, float animCycle, float accel, float throttle, float magnitude, float turnAmount) {
	float animSpeed = 15;
	float accelAnimSpeed = 45;
	float v = t * 0.5 + 0.5;
	float offsetMask = saturate(1 - v * 0.75); 

	float2 newPos = rotate_point(float2(0,0), clamp(turnAmount * -1, -6.2, 6.2) * offsetMask + magnitude * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle, originalPos);
	
	return newPos;
}


float2 getWarpedPoint(float2 originalPoint, float2 brushCenter, float2 spriteVertexLocalPos, float turnAmount, float animCycle, float accel, float throttle, float foodAmount, float2 size, float eatingStatus) {
	
	float2 spritePivotPos = originalPoint;				

	// FOOD BLOAT:
	float bloatPivotY = (saturate(foodAmount - 0.5) - 0.5) * 2;
	float bloatMask = smoothstep(0, 1, (1 - saturate(abs((spritePivotPos.y - bloatPivotY) * 1))) * 1); // smoothstep makes a more gaussain-looking shape than pointy
	//float bloatShiftMask = sin(spritePivotPos.y * 4 - clock * 10) * 0.25 + 0.75;
	float bloatMagnitude = foodAmount * foodAmount;
	spritePivotPos *= 1.0 + bloatMask * bloatMagnitude * 1.0;

	// BITING:
	float2 agentSize = size * 0.5;
	float eatingCycle = sin(eatingStatus * 3.141592);
	float biteMask = saturate(brushCenter.y + 0.25);
	spritePivotPos *= 1.0 + eatingCycle * 0.33f * biteMask;
	agentSize.y *= (eatingCycle * 0.15f * biteMask + 1.0f);
	agentSize.x *= (1.0f - eatingCycle * 0.25f * biteMask);
	float2 localPosition = spritePivotPos * agentSize + (spriteVertexLocalPos * size * 1);
	
	
	// --------------SWIM ANIMATION:-----------------------
	float animSpeed = 15;
	float accelAnimSpeed = 45;
	float v = brushCenter.y * 0.5 + 0.5;
	float offsetMask = saturate(1 - v * 0.75); 
				
	// panning yaw:
	// SWIMMING:
	float bodyAspectRatio = size.y / size.x;
	float panningYawStrength = 0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);
	float turningAngle = turnAmount;
	//float2 warpedPoint = originalPoint + spriteVertexLocalPos; //rotate_point(float2(0,size.y * 0.25), clamp(turningAngle * -1, -1, 1) * offsetMask + panningYawStrength * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle, originalPoint);
	float2 warpedPoint = rotate_point(float2(0,size.y * 0.25), clamp(turningAngle * -1, -1, 1) * offsetMask + panningYawStrength * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle, localPosition);
				
	return warpedPoint;
}
