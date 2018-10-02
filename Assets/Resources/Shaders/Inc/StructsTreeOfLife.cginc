struct TreeOfLifeNodeColliderData {  // only the data that needs to be transferred between CPU & GPU  - minimize!!
    float3 localPos;
    float3 scale;        
};
struct TreeOfLifeLeafNodeData {
    int speciesID;
	int parentSpeciesID;
    int graphDepth;
    float3 primaryHue;
    float3 secondaryHue;
    float growthPercentage;
    float age;
    float decayPercentage; 
    float isAlive;
};
struct TreeOfLifeStemSegmentData {
    int speciesID;
    int fromID;
    int toID;        
};
