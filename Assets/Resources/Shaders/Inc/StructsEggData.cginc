struct EggSackSimData {
	int parentAgentIndex;
	float2 worldPos;
	float2 velocity;
	float2 heading;
	float2 fullSize;
	float foodAmount;
	float growth;
	float decay;
	float health;
	int brushType;
};

struct EggData {
	int eggSackIndex;
	float2 worldPos;
	float2 localCoords;
	float2 localScale;
	float lifeStage;
	float attached;  // if attached, sticks to parent food, else, floats in water
};