struct CritterInitData {
	float3 boundingBoxSize;
	float spawnSizePercentage;
	float maxEnergy;
	float3 primaryHue;
	float3 secondaryHue;
	float mouthIsActive;	
	int bodyPatternX;  // what grid cell of texture sheet to use
	int bodyPatternY;  // what grid cell of texture sheet to use
	int speciesID;
};
struct CritterSimData {
	float3 worldPos;
	float2 velocity;
	float2 heading;
	float embryoPercentage;
	float growthPercentage;
	float decayPercentage;
	float foodAmount;
	float energy;
	float health;
	float stamina;
	float isBiting;
	float biteAnimCycle;
	float moveAnimCycle;
	float turnAmount;
	float accel;
	float smoothedThrottle;
};
struct CritterSkinStrokeData {
	int parentIndex;  // what agent/object is this attached to?	
	int brushType;
	float3 worldPos;
	float3 localPos;
	float3 localDir;
	float2 localScale;
	float strength;  // abstraction for pressure of brushstroke + amount of paint 
	float lifeStatus;
	float age;
	float randomSeed;
	float followLerp;				
};
struct CritterGenericStrokeData {
	int parentIndex;  // which Critter is this attached to?	
	int brushType;
	float3 bindPos;  // object-coordinates (z=forward, y=up)
	float3 worldPos;
    float3 bindNormal;
	float3 worldNormal;
    float3 bindTangent;
	float3 worldTangent;
    float2 uv;
    float2 scale;
};
