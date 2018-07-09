struct CritterInitData {
	float2 boundingBoxSize;
	float spawnSizePercentage;
	float maxEnergy;
	float3 primaryHue;
	float3 secondaryHue;
	int bodyPatternX;  // what grid cell of texture sheet to use
	int bodyPatternY;  // what grid cell of texture sheet to use
};
struct CritterSimData {
	float2 worldPos;
	float2 velocity;
	float2 heading;
	float growthPercentage;
	float decayPercentage;
	float foodAmount;
	float energy;
	float health;
	float stamina;
	float biteAnimCycle;
	float moveAnimCycle;
	float turnAmount;
	float accel;
	float smoothedThrottle;
};
