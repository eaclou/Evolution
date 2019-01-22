struct AnimalParticleData {
    int index;
	int critterIndex; // index of creature which swallowed this foodParticle
	int nearestCritterIndex;
    float isSwallowed;   // 0 = normal, 1 = in critter's belly
    float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk        
    float3 worldPos;
    float radius;
    float nutrientContent; // essentially size?
    float active;
	float refactoryAge;
	float age;
	float speed;
};
