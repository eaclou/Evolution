#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

struct CritterSwimAnimData {
	float animCycle;
	float turnAmount;
	float accel;
	float smoothedThrottle;
};

//struct CritterAnimOutData {
//	float3 spritePivotPos;
//	float3 spriteVertexPos;
//};

float3 RotatePointAroundZAngle(float3 pivot, float angle, float3 p)
{
	float3 rotatedPoint = p;
	float s = sin(angle);
	float c = cos(angle);

	// translate point back to origin:
	rotatedPoint -= pivot;
			  
	// rotate point
	float xnew = rotatedPoint.x * c - rotatedPoint.y * s;
	float ynew = rotatedPoint.x * s + rotatedPoint.y * c;
	
	rotatedPoint.xy = float2(xnew, ynew);

	// translate point back:
	rotatedPoint += pivot;

	rotatedPoint.z = p.z;

	return rotatedPoint;
}

float2 RotatePointVector2D(float2 p, float2 pivot, float2 forward) {
	float2 newPos = p - pivot;
	float2 right = float2(forward.y, -forward.x);
	newPos = newPos.x * right + newPos.y * forward;
	newPos += pivot;
	return newPos;
}

float GetSwimAngle(float t, float animCycle, float accel, float throttle, float magnitude, float turnAmount) {
	float animSpeed = 12;
	float accelAnimSpeed = 200;
	float v = t * 0.5 + 0.5;
	float offsetMask = saturate(1 - v * 0.75);
	float turnMask = saturate((1.0 - v) * 1.0);

	//float angle = clamp(turnAmount * -1.33, -6.282, 6.282) * offsetMask;
	//float angle = magnitude * sin(v * 3.141592 + animCycle * animSpeed + accel * accelAnimSpeed);// * offsetMask;

	float angle = magnitude * sin(v * 3.141592 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle + clamp(turnAmount * -1.0, -6.2, 6.2) * turnMask;

	return angle;
}

float3 TransformPosRotateYaw() {
	
}

float3 TransformPosRotateRoll() {

}

float3 TransformDirRotateYaw() {

}

float3 TransformDirRotateRoll() {

}

float3 GetAnimatedPos(float3 inPos, float3 pivotPos, CritterInitData critterInitData, CritterSimData critterSimData, float3 strokeLocalPos) {
	
	float magnitude = 0.5;

	float swimAngle = GetSwimAngle(strokeLocalPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, magnitude, critterSimData.turnAmount);

	float growthPercentage = critterSimData.growthPercentage;
	
	// FOOD BLOAT:
	float foodAmount = critterSimData.foodAmount;
	float bloatPivotY = 0; //(saturate(foodAmount - 0.5) - 0.5) * 2;
	float bloatMask = smoothstep(0, 1, (1 - saturate(abs((strokeLocalPos.y - bloatPivotY + 0.1) * 1.45))) * 1); // smoothstep makes a more gaussian-looking shape than pointy
	float bloatMagnitude = foodAmount * 2.5;

	float starvartionMask = saturate(critterSimData.energy * 2);

	float starveMultiplier = starvartionMask;

	
	
	inPos *= (1.0 + bloatMask * bloatMagnitude * 1.0);
	inPos.x = lerp(inPos.x, inPos.x * starveMultiplier, bloatMask);
	inPos.z = lerp(inPos.z, inPos.z * starveMultiplier, bloatMask);
	//inPos.x *= saturate(starveMultiplier);
	//inPos.z *= saturate(starveMultiplier);

	//inPos.z *= starveMultiplier;
	//inPos.x = lerp(inPos.x, inPos.x * starveMultiplier, bloatMask);
	//inPos.y = lerp(inPos.y, inPos.y * starveMultiplier, bloatMask);
	// end FOOD BLOAT
	
	// BITE!!!!
	
	float t = strokeLocalPos.y * 0.5 + 0.5;  // [0-1]
	float biteAnimCycle = critterSimData.biteAnimCycle;
	float eatingCycle = sin(biteAnimCycle * 3.141592);
	float biteMask = saturate((t - 0.66667) * 3);
	float lowerJawMask = biteMask * saturate(strokeLocalPos.z * 1000);
	float upperJawMask = biteMask * saturate(-strokeLocalPos.z * 1000);

	float3 critterCurScale = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 0.5;

	float activeMouthMask = critterInitData.mouthIsActive;

	// PASSIVE MOUTH:::
	float3 passiveMouthPos = inPos;
	// Mouth Opening:
	passiveMouthPos.xyz = lerp(passiveMouthPos.xyz, passiveMouthPos.xyz + float3(0,0,critterCurScale.z * 0.65), lowerJawMask * biteAnimCycle * saturate(growthPercentage * 4));
	passiveMouthPos.xyz = lerp(passiveMouthPos.xyz, passiveMouthPos.xyz + float3(0,0,-critterCurScale.z * 0.35), upperJawMask * biteAnimCycle * saturate(growthPercentage * 4));
	// Lunge Forward:
	//passiveMouthPos.y *= (eatingCycle * 0.5 * biteMask + 1.0);
	//passiveMouthPos.y += (eatingCycle * critterInitData.boundingBoxSize.y * 0.175);

	// ACTIVE MOUTH:::
	float3 activeMouthPos = inPos;
	// Mouth Opening:
	activeMouthPos.xyz = lerp(activeMouthPos.xyz, activeMouthPos.xyz + float3(0,0,critterCurScale.z * 0.65), lowerJawMask * eatingCycle * saturate(growthPercentage * 4));
	activeMouthPos.xyz = lerp(activeMouthPos.xyz, activeMouthPos.xyz + float3(0,0,-critterCurScale.z * 0.35), upperJawMask * eatingCycle * saturate(growthPercentage * 4));
	// Lunge Forward:
	activeMouthPos.y *= (eatingCycle * 0.5 * biteMask * saturate(growthPercentage * 4) + 1.0);
	activeMouthPos.y += (eatingCycle * critterInitData.boundingBoxSize.y * 0.175 * saturate(growthPercentage * 4));

	inPos = lerp(passiveMouthPos, activeMouthPos, activeMouthMask);
	
	// end BITE
		
	float3 outPos = RotatePointAroundZAngle(float3(0,0,0), swimAngle, inPos);

	// Rotate with Critter:
	float2 forward = critterSimData.heading;;
	float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
	float2 rotatedPoint = float2(outPos.x * right + outPos.y * forward);  // Rotate localRotation by AgentRotation
	
	outPos.xy = rotatedPoint;

	return outPos;
}
float3 GetAnimatedDir(float3 inDir, float3 pivotPos, CritterInitData critterInitData, CritterSimData critterSimData, CritterSkinStrokeData strokeData) {
	float magnitude = 0.5;

	float swimAngle = GetSwimAngle(strokeData.localPos.y, critterSimData.moveAnimCycle, critterSimData.accel, critterSimData.smoothedThrottle, magnitude, critterSimData.turnAmount);
	float3 outDir = RotatePointAroundZAngle(float3(0,0,0), swimAngle, inDir);

	// Rotate with Critter:
	float2 forward = critterSimData.heading;;
	float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
	float2 rotatedPoint = float2(outDir.x * right + outDir.y * forward);  // Rotate localRotation by AgentRotation
	
	outDir.xy = rotatedPoint;

	return outDir;
}





// *********************************************************************************************************************************************************
// OLD:::::
// *********************************************************************************************************************************************************

/*
struct AgentMovementAnimData {
	float animCycle;
	float turnAmount;
	float accel;
	float smoothedThrottle;
};



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
	float biteMask = saturate(1 - t);
	
	float2 newPos = originalPos * (1.0 + eatingCycle * 0.67f * biteMask);	
	newPos.y *= (eatingCycle * 0.3f * biteMask + 1.0f);
	newPos.x *= (1.0f - eatingCycle * 0.5f * biteMask);
	
	return newPos;
}

float getSwimAngle(float t, float animCycle, float accel, float throttle, float magnitude, float turnAmount) {
	float animSpeed = 15;
	float accelAnimSpeed = 45;
	float v = t * 0.5 + 0.5;
	float offsetMask = saturate(1 - v * 0.75); 
	
	float angle = 1.33 * magnitude * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle + clamp(turnAmount * -1.33, -6.2, 6.2) * offsetMask;

	return angle;
}

float2 swimAnimPos(float2 originalPos, float t, float animCycle, float accel, float throttle, float magnitude, float turnAmount) {
	

	float swimAngle = getSwimAngle(t, animCycle, accel, throttle, magnitude, turnAmount);
	float2 newPos = RotatePointAngle2D(float2(0,0), swimAngle, originalPos);
	
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
	float2 warpedPoint = RotatePointAngle2D(float2(0,size.y * 0.25), clamp(turningAngle * -1, -1, 1) * offsetMask + panningYawStrength * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle, localPosition);
				
	return warpedPoint;
}
*/
