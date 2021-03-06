struct AnimalParticleData {
    int index;
	int critterIndex; // index of creature which swallowed this foodParticle
	int nearestCritterIndex;  // is this actually used???
    float isSwallowed;   // 0 = normal, 1 = in critter's belly
    float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk        
    float3 worldPos;	
	float2 velocity;  // facingDir?
    float radius; // displaySize?
	float oxygenUsed;
	float wasteProduced;
	float algaeConsumed;
    float biomass; // essentially size?
    float isActive;  // is being simulated?
	float isDecaying;
	float age;  // 
	float speed;
	float4 color;
	float4 genomeVector;
	float extra0;
	float energy;
};
