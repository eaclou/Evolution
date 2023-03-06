using UnityEngine;


/// <summary>
///  Move Food-related stuff from SimulationManager into here to de-clutter simManager:
/// </summary>
public class ZooplanktonManager {
    SimulationManager simManager => SimulationManager.instance;
    SettingsManager settingsRef => SettingsManager.instance;
    TheRenderKing renderKing => TheRenderKing.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    
    public SimResourceManager resourceManagerRef;
    
    private ComputeShader computeShaderAnimalParticles;
    
    private const int maxNumMicrobes = 1024 * 8;  // *** 
    public ComputeBuffer animalParticlesCBuffer;
    private ComputeBuffer animalParticlesCBufferSwap;

    public ComputeBuffer animalParticleInternalBitsCBuffer;
    //private RenderTexture animalParticlesNearestCritters1024;
    //private RenderTexture animalParticlesNearestCritters32;
    //private RenderTexture animalParticlesNearestCritters1;
    private ComputeBuffer agentsClosestMicrobeDataCBuffer;
    //public AnimalParticleData[] closestMicrobesToCrittersArray; // Vessel for Getting CBufffer data, may need processing
    public AnimalParticleData[] agentsClosestMicrobeDataArray; // processed information, length = numAgents
    //may have to do similar trick with eatBuffers v v v
    private ComputeBuffer animalParticlesEatAmountsCBuffer;
    public float[] animalParticlesEatAmountsArray;
    private ComputeBuffer animalParticlesMeasure32;
    private ComputeBuffer animalParticlesMeasure1;
    private AnimalParticleData[] animalParticleMeasurementTotalsData;

    private ComputeBuffer cursorClosestMicrobeDataCBuffer;
    public AnimalParticleData[] cursorParticleDataArray;
    private ComputeBuffer cursorDistances;
    public AnimalParticleData selectedAnimalParticleData;
    public AnimalParticleData cursorClosestAnimalParticleData;
    public bool isAnimalParticleSelected = false;
    public int selectedAnimalParticleIndex = 0;
    public int closestZooplanktonToCursorIndex = 0;

    public RenderTexture agentsClosestMicrobeDistanceRT;
    public RenderTexture agentsClosestMicrobeDistanceReducedRT;
    private ComputeBuffer agentsClosestMicrobeDistanceCBuffer;
    public Vector4[] agentsClosestMicrobeDistanceArray; // final data to use in simulation

    private ComputeBuffer zooplanktonRepresentativeGenomeCBuffer;

    public WorldLayerZooplanktonGenome zooplanktonSlotGenomeCurrent;  // algae particles!  -- likely to be converted into plants eventually ***
    public WorldLayerZooplanktonGenome[] zooplanktonSlotGenomeMutations;
    
   // public Vector2 closestAnimalParticlePosition2D => closestAnimalParticleData.worldPos2D;
    
    public struct AnimalParticleData {
        public int index;
	    public int critterIndex; // index of creature which swallowed this foodParticle
	    public int nearestCritterIndex;
        public float isSwallowed;   // 0 = normal, 1 = in critter's belly
        public float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk        
        public Vector3 worldPos;
        //public Vector2 p1;  // spline points:
	    //public Vector2 p2;
	    //public Vector2 p3;
        public Vector2 velocity;
        public float radius;
        public float oxygenUsed;
	    public float wasteProduced;
	    public float algaeConsumed;
        public float biomass; // essentially size?
        //public float nutrientContent; // essentially size?
        public float isActive;
	    public float isDecaying;
	    public float age;
	    public float speed;
        public Vector4 color;
        public Vector4 genomeVector;
        public float extra0;
        public float energy;
        //public int count;
        
        public Vector2 worldPos2D => new Vector2(worldPos.x, worldPos.y);
    }

    public struct AnimalParticleInternalBitData {
        public Vector4 localPos;
        public Vector4 color;
    }
   
    private int GetAnimalParticleDataSize() {
        int bitSize = sizeof(float) * 26 + sizeof(int) * 3;
        return bitSize;
    }
    
    public ZooplanktonManager(SimResourceManager resourcesRef) {
        resourceManagerRef = resourcesRef;        
    }

    public void InitializeAnimalParticles(int numAgents, ComputeShader computeShader) {
        //float startTime = Time.realtimeSinceStartup;
        //Debug.Log((Time.realtimeSinceStartup - startTime).ToString());
        computeShaderAnimalParticles = computeShader;
        
        animalParticlesCBuffer = new ComputeBuffer(maxNumMicrobes, GetAnimalParticleDataSize());
        animalParticlesCBufferSwap = new ComputeBuffer(maxNumMicrobes, GetAnimalParticleDataSize());
        AnimalParticleData[] animalParticlesArray = new AnimalParticleData[maxNumMicrobes];

        animalParticleInternalBitsCBuffer = new ComputeBuffer(maxNumMicrobes * 32, sizeof(float) * 8);
        AnimalParticleInternalBitData[] animalParticleInternalBitsArray = new AnimalParticleInternalBitData[animalParticleInternalBitsCBuffer.count];
        float minParticleSize = 1f; // settingsRef.avgAnimalParticleRadius / settingsRef.animalParticleRadiusVariance;
        float maxParticleSize = 2f; // settingsRef.avgAnimalParticleRadius * settingsRef.animalParticleRadiusVariance;

        for(int i = 0; i < animalParticlesCBuffer.count; i++) {
            AnimalParticleData data = new AnimalParticleData();
            data.index = i;
            data.worldPos = Vector3.zero; // new Vector3(UnityEngine.Random.Range(0f, SimulationManager._MapSize), UnityEngine.Random.Range(0f, SimulationManager._MapSize), 0f);

            data.radius = Random.Range(minParticleSize, maxParticleSize); // obsolete!
            data.biomass = 0.001f; // data.radius * data.radius * Mathf.PI; // * settingsRef.animalParticleNutrientDensity;
            data.isActive = 0f;
            data.isDecaying = 0f;
            data.age = 0f; // UnityEngine.Random.Range(1f, 2f);
            data.color = Random.ColorHSV();
            data.genomeVector = Vector4.zero;
            data.extra0 = 0f;
            data.energy = 0f;
            animalParticlesArray[i] = data;

            for(int j = 0; j < 32; j++) {
                AnimalParticleInternalBitData bitData = new AnimalParticleInternalBitData();
                Vector3 randPos = UnityEngine.Random.insideUnitSphere;
                bitData.localPos = new Vector4(randPos.x, randPos.y, randPos.z, 0f);
                bitData.color = UnityEngine.Random.ColorHSV();
                animalParticleInternalBitsArray[i * 32 + j] = bitData;
            }
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());
        animalParticleInternalBitsCBuffer.SetData(animalParticleInternalBitsArray);
        animalParticlesCBuffer.SetData(animalParticlesArray);
        animalParticlesCBufferSwap.SetData(animalParticlesArray);
       
        animalParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        animalParticlesEatAmountsArray = new float[numAgents];

        animalParticleMeasurementTotalsData = new AnimalParticleData[maxNumMicrobes / 1024];
        animalParticlesMeasure32 = new ComputeBuffer(maxNumMicrobes / 32, GetAnimalParticleDataSize());
        animalParticlesMeasure1 = new ComputeBuffer(maxNumMicrobes / 1024, GetAnimalParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
        
        agentsClosestMicrobeDistanceRT = new RenderTexture(maxNumMicrobes / 32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        agentsClosestMicrobeDistanceRT.wrapMode = TextureWrapMode.Clamp;
        agentsClosestMicrobeDistanceRT.filterMode = FilterMode.Point;
        agentsClosestMicrobeDistanceRT.enableRandomWrite = true;        
        agentsClosestMicrobeDistanceRT.Create(); 

        agentsClosestMicrobeDistanceReducedRT = new RenderTexture(maxNumMicrobes / 256, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        agentsClosestMicrobeDistanceReducedRT.wrapMode = TextureWrapMode.Clamp;
        agentsClosestMicrobeDistanceReducedRT.filterMode = FilterMode.Point;
        agentsClosestMicrobeDistanceReducedRT.enableRandomWrite = true;        
        agentsClosestMicrobeDistanceReducedRT.Create();
 ///***EAC  TRY:::: Formally splitting buffers into extended and condensed form
 ///    OR:: LEAVE OVERFILL in and just ignore it, use only first Entries per critter
 ///    avoids need for double buffers and extra copying
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        
        agentsClosestMicrobeDataCBuffer = new ComputeBuffer(numAgents, GetAnimalParticleDataSize());
        agentsClosestMicrobeDataArray = new AnimalParticleData[agentsClosestMicrobeDataCBuffer.count];
        
        agentsClosestMicrobeDistanceCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        agentsClosestMicrobeDistanceArray = new Vector4[agentsClosestMicrobeDistanceCBuffer.count];

        cursorClosestMicrobeDataCBuffer = new ComputeBuffer(2, GetAnimalParticleDataSize());  // 0 = selected, 1 = closest to cursor
        cursorParticleDataArray = new AnimalParticleData[2];
        cursorDistances = new ComputeBuffer(maxNumMicrobes, sizeof(float) * 4);
        
        int numMutations = 4;  // don't change this
        zooplanktonSlotGenomeCurrent = new WorldLayerZooplanktonGenome();
        zooplanktonSlotGenomeCurrent.representativeData = animalParticlesArray[0];
        zooplanktonSlotGenomeCurrent.swimSpeed01 = 0.5f;
        zooplanktonSlotGenomeCurrent.agingRate01 = 0.5f;
        zooplanktonSlotGenomeCurrent.attractForce01 = 0.5f;
        zooplanktonSlotGenomeCurrent.name = "Zooplankton, Bebe!";
        zooplanktonSlotGenomeCurrent.textDescriptionMutation = "Swim Speed: " + zooplanktonSlotGenomeCurrent.swimSpeed01.ToString("F2") + "\nAging Rate: " + zooplanktonSlotGenomeCurrent.agingRate01.ToString("F2") + "\nAttraction: " + zooplanktonSlotGenomeCurrent.attractForce01.ToString("F2");
        zooplanktonSlotGenomeMutations = new WorldLayerZooplanktonGenome[numMutations];

        GenerateWorldLayerZooplanktonGenomeMutationOptions();

        zooplanktonRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetAnimalParticleDataSize());
        AnimalParticleData[] zooplanktonRepresentativeGenomeArray = new AnimalParticleData[1];
        zooplanktonRepresentativeGenomeArray[0] = zooplanktonSlotGenomeCurrent.representativeData;
        zooplanktonRepresentativeGenomeCBuffer.SetData(zooplanktonRepresentativeGenomeArray);
    }

    public void GenerateWorldLayerZooplanktonGenomeMutationOptions() {
        for(int j = 0; j < zooplanktonSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            jLerp = jLerp * jLerp;
            WorldLayerZooplanktonGenome mutatedGenome = new WorldLayerZooplanktonGenome();
            Vector4 randColor = Random.ColorHSV();
            
            Vector4 col = zooplanktonSlotGenomeCurrent.representativeData.color;
            col = Vector4.Lerp(col, randColor, jLerp);
            mutatedGenome.representativeData = zooplanktonSlotGenomeCurrent.representativeData;
            mutatedGenome.representativeData.color = col;

            mutatedGenome.swimSpeed01 = Mathf.Lerp(0f, 1f, Random.Range(0f, 1f));
            mutatedGenome.swimSpeed01 = Mathf.Lerp(zooplanktonSlotGenomeCurrent.swimSpeed01, mutatedGenome.swimSpeed01, jLerp);

            mutatedGenome.agingRate01 = Mathf.Lerp(0f, 1f, Random.Range(0f, 1f));
            mutatedGenome.agingRate01 = Mathf.Lerp(zooplanktonSlotGenomeCurrent.agingRate01, mutatedGenome.agingRate01, jLerp);

            mutatedGenome.attractForce01 = Mathf.Lerp(0f, 1f, Random.Range(0f, 1f));
            mutatedGenome.attractForce01 = Mathf.Lerp(zooplanktonSlotGenomeCurrent.attractForce01, mutatedGenome.attractForce01, jLerp);
            
            mutatedGenome.name = zooplanktonSlotGenomeCurrent.name;
            mutatedGenome.textDescriptionMutation = "Swim Speed: " + mutatedGenome.swimSpeed01.ToString("F2") + "\nAging Rate: " + mutatedGenome.agingRate01.ToString("F2") + "\nAttraction: " + mutatedGenome.attractForce01.ToString("F2");
            
            zooplanktonSlotGenomeMutations[j] = mutatedGenome;
        }
    }
    
    /*public void ReviveSelectAnimalParticles(int[] indicesArray, float radius, Vector4 spawnCoords, SimulationStateData simStateDataRef) {  // Not used????

        ComputeBuffer selectRespawnAnimalParticleIndicesCBuffer = new ComputeBuffer(indicesArray.Length, sizeof(int));
        selectRespawnAnimalParticleIndicesCBuffer.SetData(indicesArray);  // manually revive specified indices
        
        int kernelCSReviveSelectAnimalParticles = computeShaderAnimalParticles.FindKernel("CSReviveSelectAnimalParticles");
        computeShaderAnimalParticles.SetBuffer(kernelCSReviveSelectAnimalParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSReviveSelectAnimalParticles, "selectRespawnAnimalParticleIndicesCBuffer", selectRespawnAnimalParticleIndicesCBuffer);
        
        computeShaderAnimalParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderAnimalParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        //computeShaderAnimalParticles.SetVector("_AnimalSprinklePos", spawnCoords);
        //computeShaderAnimalParticles.SetFloat("_AnimalSprinkleRadius", radius);
        computeShaderAnimalParticles.SetBuffer(kernelCSReviveSelectAnimalParticles, "animalParticlesWrite", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.Dispatch(kernelCSReviveSelectAnimalParticles, indicesArray.Length, 1, 1);

        selectRespawnAnimalParticleIndicesCBuffer.Release();
    }*/
    
    // Go through animalParticleData and check for inactive
    // determined by current total animal -- done!
    // if flag on shader for Respawn is on, set to active and initialize
    public void SimulateAnimalParticles(SimulationStateData simStateDataRef, SimResourceManager resourcesManager) 
    {
        int kernelCSSimulateAnimalParticles = computeShaderAnimalParticles.FindKernel("CSSimulateAnimalParticles");
        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "animalParticlesWrite", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "velocityRead", fluidManager._VelocityPressureDivergenceMain);        
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "altitudeRead", renderKing.baronVonTerrain.terrainHeightDataRT);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_SpawnDensityMap", renderKing.spiritBrushRT);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_ResourceGridRead", simManager.vegetationManager.resourceGridRT1);
        //computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_SpawnDensityMap", algaeGridRT1);        
        computeShaderAnimalParticles.SetFloat("_GlobalOxygenLevel", resourcesManager.curGlobalOxygen); // needed?
        computeShaderAnimalParticles.SetFloat("_GlobalAlgaeLevel", resourceManagerRef.curGlobalPlantParticles);
        computeShaderAnimalParticles.SetFloat("_SpiritBrushPosNeg", renderKing.spiritBrushPosNeg);
        computeShaderAnimalParticles.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);   
        // Movement Params:
        computeShaderAnimalParticles.SetFloat("_MasterSwimSpeed", settingsRef.zooplanktonSettings._MasterSwimSpeed * Mathf.Lerp(0.01f, 5f, zooplanktonSlotGenomeCurrent.swimSpeed01)); // = 0.35;
        computeShaderAnimalParticles.SetFloat("_AlignMaskRange", settingsRef.zooplanktonSettings._AlignMaskRange); // = 0.025;
        computeShaderAnimalParticles.SetFloat("_AlignMaskOffset", settingsRef.zooplanktonSettings._AlignMaskOffset); // = 0.0833;
        computeShaderAnimalParticles.SetFloat("_AlignSpeedMult", settingsRef.zooplanktonSettings._AlignSpeedMult); // = 0.00015;
        
        computeShaderAnimalParticles.SetFloat("_AttractMag", settingsRef.zooplanktonSettings._AttractMag * Mathf.Lerp(0.1f, 10f, zooplanktonSlotGenomeCurrent.attractForce01)); // = 0.0000137;
        computeShaderAnimalParticles.SetFloat("_AttractMaskMaxDistance", settingsRef.zooplanktonSettings._AttractMaskMaxDistance * Mathf.Lerp(0.1f, 10f, zooplanktonSlotGenomeCurrent.attractForce01)); // = 0.0036;
        computeShaderAnimalParticles.SetFloat("_AttractMaskOffset", settingsRef.zooplanktonSettings._AttractMaskOffset); // = 0.5;
        computeShaderAnimalParticles.SetFloat("_SwimNoiseMag", settingsRef.zooplanktonSettings._SwimNoiseMag); // = 0.000086;
        computeShaderAnimalParticles.SetFloat("_SwimNoiseFreqMin", settingsRef.zooplanktonSettings._SwimNoiseFreqMin); // = 0.00002
        computeShaderAnimalParticles.SetFloat("_SwimNoiseFreqRange", settingsRef.zooplanktonSettings._SwimNoiseFreqRange); // = 0.0002
        computeShaderAnimalParticles.SetFloat("_SwimNoiseOnOffFreq", settingsRef.zooplanktonSettings._SwimNoiseOnOffFreq); //  = 0.0001
        computeShaderAnimalParticles.SetFloat("_ShoreCollisionMag", settingsRef.zooplanktonSettings._ShoreCollisionMag); // = 0.0065;
        computeShaderAnimalParticles.SetFloat("_ShoreCollisionDistOffset", settingsRef.zooplanktonSettings._ShoreCollisionDistOffset); // = 0.15;
        computeShaderAnimalParticles.SetFloat("_ShoreCollisionDistSlope", settingsRef.zooplanktonSettings._ShoreCollisionDistSlope); // = 3.5;
        computeShaderAnimalParticles.SetFloat("_AgingMult", Mathf.Lerp(0.1f, 10f, zooplanktonSlotGenomeCurrent.agingRate01));
        //computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "animalParticlesNearestCrittersRT", animalParticlesNearestCritters1);
        computeShaderAnimalParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        
        computeShaderAnimalParticles.SetFloat("_Time", Time.realtimeSinceStartup);
                
        //float randRoll = UnityEngine.Random.Range(0f, 1f);
        computeShaderAnimalParticles.SetFloat("_RespawnAnimalParticles", 1f);
        computeShaderAnimalParticles.SetFloat("_IsBrushing", 1f); // brushF);  
        
        // Need to compute when they should be allowed to spawn, how to keep track of resources used/transferred??
        computeShaderAnimalParticles.SetFloat("_SpawnPosX", Random.Range(0.1f, 0.9f)); // UPDATE THIS!!! ****
        computeShaderAnimalParticles.SetFloat("_SpawnPosY", Random.Range(0.1f, 0.9f));

        float spawnLerp = simManager.trophicLayersManager.GetLayerLerp(KnowledgeMapId.Microbes, simManager.simAgeTimeSteps);  // no need to still do this??? ****
        float spawnRadius = Mathf.Lerp(1f, SimulationManager._MapSize, spawnLerp);
        Vector4 spawnPos = new Vector4(simManager.trophicLayersManager.zooplanktonOriginPos.x, simManager.trophicLayersManager.zooplanktonOriginPos.y, 0f, 0f);
        computeShaderAnimalParticles.SetFloat("_SpawnRadius", spawnRadius);
        computeShaderAnimalParticles.SetVector("_SpawnPos", spawnPos);

        float minParticleSize = 0.1f; // settingsRef.avgAnimalParticleRadius / settingsRef.animalParticleRadiusVariance;
        float maxParticleSize = 0.2f; // settingsRef.avgAnimalParticleRadius * settingsRef.animalParticleRadiusVariance;

        computeShaderAnimalParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderAnimalParticles.SetFloat("_MaxParticleSize", maxParticleSize);
        // Revisit::::
        computeShaderAnimalParticles.SetFloat("_ParticleNutrientDensity", 10f); // settingsRef.animalParticleNutrientDensity);
        computeShaderAnimalParticles.SetFloat("_AnimalParticleRegrowthRate", 0.01f); // settingsRef.animalParticleRegrowthRate);  // ************  HARD-CODED!!!!

        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "_RepresentativeGenomeCBuffer", zooplanktonRepresentativeGenomeCBuffer);

        computeShaderAnimalParticles.Dispatch(kernelCSSimulateAnimalParticles, Mathf.CeilToInt(maxNumMicrobes / 1024), 1, 1);                

        // Copy/Swap Animal Particle Buffer:
        int kernelCSCopyAnimalParticlesBuffer = computeShaderAnimalParticles.FindKernel("CSCopyAnimalParticlesBuffer");
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesRead", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesWrite", animalParticlesCBuffer);        
        computeShaderAnimalParticles.Dispatch(kernelCSCopyAnimalParticlesBuffer, Mathf.CeilToInt(maxNumMicrobes / 1024), 1, 1); // 1024x

        //animalParticleInternalBitsCBuffer.SetData(animalParticleInternalBitsArray);
    }
    
    public void ProcessSlotMutation() {
        AnimalParticleData[] zooplanktonRepresentativeGenomeArray = new AnimalParticleData[1];
        zooplanktonRepresentativeGenomeArray[0] = zooplanktonSlotGenomeCurrent.representativeData;
        zooplanktonRepresentativeGenomeCBuffer.SetData(zooplanktonRepresentativeGenomeArray);
    }
    
    public void EatSelectedAnimalParticles(SimulationStateData simStateDataRef) {  // removes gpu particle & sends consumption data back to CPU
        // Use CritterSimData to determine critter mouth locations
        // run through all animalParticles, check against each critter position, then measure min value with recursive reduction:
        // Need to update CritterSim&InitData structs to have more mouth/bite info
        // Record how much animal successfully eaten per Critter
        
        animalParticlesEatAmountsCBuffer.Release();
        animalParticlesEatAmountsCBuffer = new ComputeBuffer(simManager.maxAgents, sizeof(float));
        animalParticlesEatAmountsArray = new float[animalParticlesEatAmountsCBuffer.count];
        //animalParticlesEatAmountsCBuffer.SetData(animalParticlesEatAmountsArray);

        int kernelCSEatSelectedAnimalParticles = computeShaderAnimalParticles.FindKernel("CSEatSelectedAnimalParticles");
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "animalParticlesWrite", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "animalParticlesEatAmountsCBuffer", animalParticlesEatAmountsCBuffer);        
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "closestParticlesDataCBuffer", agentsClosestMicrobeDataCBuffer);  
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "_ClosestZooplanktonCBuffer", agentsClosestMicrobeDistanceCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSEatSelectedAnimalParticles, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        animalParticlesEatAmountsCBuffer.GetData(animalParticlesEatAmountsArray);

        float totalAnimalEaten = 0f;
        /*string detxt = "";
        for(int i = 0; i < animalParticlesEatAmountsArray.Length; i++) {
            totalAnimalEaten += animalParticlesEatAmountsArray[i];
            detxt += i + ": " + animalParticlesEatAmountsArray[i] + "\n";
        }
        if(totalAnimalEaten > 0f) {
            Debug.Log("amount eqaten: " + totalAnimalEaten + "\n" + detxt);
        }*/
        
        // Copy/Swap Animal PArticle Buffer:
        int kernelCSCopyAnimalParticlesBuffer = computeShaderAnimalParticles.FindKernel("CSCopyAnimalParticlesBuffer");
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesRead", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesWrite", animalParticlesCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSCopyAnimalParticlesBuffer, Mathf.CeilToInt(maxNumMicrobes / 1024), 1, 1);
    }

    // need to send info on closest particle pos/dir/amt back to CPU also
    public void FindClosestAnimalParticleToCritters(SimulationStateData simStateDataRef) {
        //LOGIC:
        // Function 3: Finish processing RT into 2 buffers: one vector4 of closest single microbe to each Critter
        
        //int kernelCSNewMeasureDistancesInit = computeShaderAnimalParticles.FindKernel("CSNewMeasureDistancesInit");
        //int kernelCSNewMeasureDistancesMainA = computeShaderAnimalParticles.FindKernel("CSNewMeasureDistancesMainA");

        
        int kernelCSGenerateCritterNearestMicrobesRT = computeShaderAnimalParticles.FindKernel("CSGenerateCritterNearestMicrobesRT");
        int kernelCSCollapseDistancesRTBy32x = computeShaderAnimalParticles.FindKernel("CSCollapseDistancesRTBy32x");
        int kernelCSCollapseDistancesRTBy8x = computeShaderAnimalParticles.FindKernel("CSCollapseDistancesRTBy8x");
        int kernelCSCreateCritterNearestMicrobeBuffers = computeShaderAnimalParticles.FindKernel("CSCreateCritterNearestMicrobeBuffers");
        // Function 1:  Takes list of all microbes + critterData and generates a rendertexture with baked distances from each critter to every 32nd microbe
        
        computeShaderAnimalParticles.SetBuffer(kernelCSGenerateCritterNearestMicrobesRT, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSGenerateCritterNearestMicrobesRT, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSGenerateCritterNearestMicrobesRT, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetTexture(kernelCSGenerateCritterNearestMicrobesRT, "_CritterToZooplanktonDistancesTexWrite", agentsClosestMicrobeDistanceRT); // INIT
        computeShaderAnimalParticles.SetTexture(kernelCSCollapseDistancesRTBy8x, "_CritterToZooplanktonDistancesTexRead", agentsClosestMicrobeDistanceRT); /// MAINA
        computeShaderAnimalParticles.Dispatch(kernelCSGenerateCritterNearestMicrobesRT, maxNumMicrobes / 32, simStateDataRef.critterSimDataCBuffer.count, 1);
        // Function 2: REDUCE RenderTexture size by factor 2^n   Loop this step as needed? or hard code different amounts?
        computeShaderAnimalParticles.SetTexture(kernelCSCollapseDistancesRTBy8x, "_CritterToZooplanktonDistancesTexWrite", agentsClosestMicrobeDistanceReducedRT);
        computeShaderAnimalParticles.SetTexture(kernelCSCreateCritterNearestMicrobeBuffers, "_CritterToZooplanktonDistancesTexRead", agentsClosestMicrobeDistanceReducedRT);
        computeShaderAnimalParticles.Dispatch(kernelCSCollapseDistancesRTBy8x, maxNumMicrobes / 32 / 8, simStateDataRef.critterSimDataCBuffer.count, 1);

        // Function 3: Finish processing length-32 RT into 2 sorted buffers: one vector4 of closest single microbe to each Critter
        
        computeShaderAnimalParticles.SetBuffer(kernelCSCreateCritterNearestMicrobeBuffers, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSCreateCritterNearestMicrobeBuffers, "_ClosestZooplanktonCBuffer", agentsClosestMicrobeDistanceCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSCreateCritterNearestMicrobeBuffers, "closestParticlesDataCBuffer", agentsClosestMicrobeDataCBuffer);
        //
        computeShaderAnimalParticles.Dispatch(kernelCSCreateCritterNearestMicrobeBuffers, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        //Third Stage needed???
        //AnimalParticleData[] microbeDataArray = new AnimalParticleData[agentsClosestMicrobeDataCBuffer.count]; // FULL ~8k length
        agentsClosestMicrobeDataCBuffer.GetData(agentsClosestMicrobeDataArray);
        agentsClosestMicrobeDistanceCBuffer.GetData(agentsClosestMicrobeDistanceArray); // DISTANCES VECTOR4
        
    }
    
    private Vector4[] ReduceDistancesArray(Vector4[] inBuffer) {

        Vector4[] newBuffer = new Vector4[(inBuffer.Length / 2)];
        for (int i = 0; i < newBuffer.Length; i++) {
            Vector4 cellDataA = inBuffer[i * 2];
            Vector4 cellDataB = inBuffer[i * 2 + 1];

            newBuffer[i] = cellDataA.y <= cellDataB.y ? cellDataA : cellDataB;
        }

        return newBuffer;
    }
    
    public void FindClosestAnimalParticleToCursor(Vector3 mousePositionOnWater) {
        FindClosestAnimalParticleToCoords(mousePositionOnWater.x, mousePositionOnWater.y);
    }
    
    public void FindClosestAnimalParticleToCoords(float xCoord, float yCoord) {
        int kernelCSMeasureInitCursorDistances = computeShaderAnimalParticles.FindKernel("CSMeasureInitCursorDistances");        
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCursorDistances, "animalParticlesRead", animalParticlesCBuffer);  
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCursorDistances, "cursorDistancesWrite", cursorDistances);
        computeShaderAnimalParticles.SetFloat("_MouseCoordX", xCoord);
        computeShaderAnimalParticles.SetFloat("_MouseCoordY", yCoord);
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureInitCursorDistances, animalParticlesCBuffer.count / 32, 1, 1);

        Vector4[] cursorDistancesArray = new Vector4[maxNumMicrobes]; //***EAC
        cursorDistances.GetData(cursorDistancesArray);
        
        // Manual Sort!
        Vector4[] swapBuffer = cursorDistancesArray;
        swapBuffer = ReduceDistancesArray(cursorDistancesArray);        
        for(int tierID = 1; tierID < Mathf.Log(maxNumMicrobes,2); tierID++) {            
            Vector4[] writeBuffer = ReduceDistancesArray(swapBuffer);            
            swapBuffer = new Vector4[writeBuffer.Length];
            for(int x = 0; x < writeBuffer.Length; x++) {
                swapBuffer[x] = writeBuffer[x];
            }
        }
        closestZooplanktonToCursorIndex = Mathf.RoundToInt(swapBuffer[0].x);
        //string txt = "Closest = " + swapBuffer[0].x.ToString() + ", D: " + swapBuffer[0].y.ToString(); // + "__ " + swapBuffer[3].x.ToString() + " __ (" + cursorDistanceArray1024[1023].ToString() + ")   " + cursorDistanceArray1024[511].ToString();
        //Debug.Log(txt);
        
        // Now Fetch the actual particleData:::::
        int kernelCSFetchParticleByID = computeShaderAnimalParticles.FindKernel("CSFetchParticleByID");
        computeShaderAnimalParticles.SetBuffer(kernelCSFetchParticleByID, "selectedAnimalParticleDataCBuffer", cursorClosestMicrobeDataCBuffer);      
        
        computeShaderAnimalParticles.SetInt("_SelectedParticleID", selectedAnimalParticleIndex);   
        computeShaderAnimalParticles.SetInt("_ClosestParticleID", Mathf.RoundToInt(swapBuffer[0].x)); 
        computeShaderAnimalParticles.SetBuffer(kernelCSFetchParticleByID, "animalParticlesRead", animalParticlesCBuffer);        
        computeShaderAnimalParticles.Dispatch(kernelCSFetchParticleByID, 1, 1, 1);

        cursorClosestMicrobeDataCBuffer.GetData(cursorParticleDataArray);

        //string txt = "nearestIndex = " + cursorParticleDataArray[0].index + " (" + cursorParticleDataArray[0].age +  ")   " + (cursorParticleDataArray[0].biomass * 1000f).ToString("F0"); // "lngth: " + swapBuffer.Length.ToString() + ",  " + swapBuffer[0].ToString() + "   " + swapBuffer[1].ToString() + "   " + swapBuffer[swapBuffer.Length - 2].ToString() + "   " + swapBuffer[swapBuffer.Length - 1].ToString();
        //Debug.Log(cursorParticleDataArray[1].index);
        cursorClosestAnimalParticleData = cursorParticleDataArray[1];
        selectedAnimalParticleData = cursorParticleDataArray[0];
    }
    
    public void MeasureTotalAnimalParticlesAmount() { // supports >1024 now?
        int kernelCSMeasureTotalAnimalParticlesAmount = computeShaderAnimalParticles.FindKernel("CSMeasureTotalAnimalParticlesAmount");
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesWrite", animalParticlesMeasure32);
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureTotalAnimalParticlesAmount, maxNumMicrobes / 32, 1, 1);
        
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesRead", animalParticlesMeasure32);
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesWrite", animalParticlesMeasure1);
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureTotalAnimalParticlesAmount, maxNumMicrobes / 1024, 1, 1);
        
        animalParticlesMeasure1.GetData(animalParticleMeasurementTotalsData);
        //add up final list:
        float curBiomass = 0f;
        float curOxygenUsed = 0f;
        float curWasteProduced = 0f;
        float curAlgaeConsumed = 0;
        //int curAliveCount = 0;
        for(int i = 0; i < animalParticleMeasurementTotalsData.Length; i++) {
            curBiomass += animalParticleMeasurementTotalsData[i].biomass;
            curOxygenUsed += animalParticleMeasurementTotalsData[i].oxygenUsed;
            curWasteProduced += animalParticleMeasurementTotalsData[i].wasteProduced;
            curAlgaeConsumed += animalParticleMeasurementTotalsData[i].algaeConsumed;
            //curAliveCount++;
        }
        resourceManagerRef.curGlobalAnimalParticles = curBiomass;
        resourceManagerRef.oxygenUsedByAnimalParticlesLastFrame = curOxygenUsed;
        resourceManagerRef.wasteProducedByAnimalParticlesLastFrame = curWasteProduced;
        resourceManagerRef.algaeConsumedByAnimalParticlesLastFrame = curAlgaeConsumed;
        //Debug.Log(animalParticleMeasurementTotalsData.Length + ", " + curBiomass);
        /*if(UnityEngine.Random.Range(0f, 1f) < 0.01f) {
            Debug.Log("curGlobalAnimalParticles: " + curGlobalAnimalParticles.ToString() + "\n" +
            "OxygenUsedByAnimalParticlesLastFrame: " + oxygenUsedByAnimalParticlesLastFrame.ToString() + "\n" +
            "WasteProducedByAnimalParticlesLastFrame: " + wasteProducedByAnimalParticlesLastFrame.ToString() + "\n" +
            "AlgaeConsumedByAnimalParticlesLastFrame: " + algaeConsumedByAnimalParticlesLastFrame.ToString() + "\n");
        }*/
    }
    
    public void ClearBuffers() {
         /*       
        if(animalParticlesNearestCritters1 != null) {
            animalParticlesNearestCritters1.Release();
            animalParticlesNearestCritters32.Release();
            animalParticlesNearestCritters1024.Release();
        } */       
        animalParticlesCBuffer?.Release();
        animalParticlesCBufferSwap?.Release();
        animalParticleInternalBitsCBuffer?.Release();

        agentsClosestMicrobeDataCBuffer?.Release();
        animalParticlesEatAmountsCBuffer?.Release();
        animalParticlesMeasure32?.Release();
        animalParticlesMeasure1?.Release();
        zooplanktonRepresentativeGenomeCBuffer?.Release();

        if(agentsClosestMicrobeDistanceRT != null) {
            agentsClosestMicrobeDistanceRT.Release();
        }
        if(agentsClosestMicrobeDistanceReducedRT != null) {
            agentsClosestMicrobeDistanceReducedRT.Release();
        }
        
        agentsClosestMicrobeDistanceCBuffer?.Release();
        agentsClosestMicrobeDataCBuffer?.Release();
        cursorDistances?.Release();
        cursorClosestMicrobeDataCBuffer?.Release();
    }
}
