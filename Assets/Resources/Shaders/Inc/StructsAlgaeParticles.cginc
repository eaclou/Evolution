struct AlgaeParticleData {
	int index;				
	int critterIndex; // index of creature which swallowed this foodParticle
	int nearestCritterIndex;
	float isSwallowed;   // 0 = normal, 1 = in critter's belly
	float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk      
	float2 worldPos;
	float radius;
	float biomass;
	float isActive;
	float isDecaying;
	float age;
	float oxygenProduced;
    float nutrientsUsed;
    float wasteProduced;
};
