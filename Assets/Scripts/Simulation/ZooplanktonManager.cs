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
    //private RenderTexture animalParticlesNearestCritters1024;
    //private RenderTexture animalParticlesNearestCritters32;
    //private RenderTexture animalParticlesNearestCritters1;
    private ComputeBuffer closestMicrobesToCrittersCBuffer;
    public AnimalParticleData[] closestMicrobesToCrittersArray; // Vessel for Getting CBufffer data, may need processing
    public AnimalParticleData[] finalCritterClosestMicrobesArray; // processed information, length = numAgents
    //may have to do similar trick with eatBuffers v v v
    private ComputeBuffer animalParticlesEatAmountsCBuffer;
    public float[] animalParticlesEatAmountsArray;
    private ComputeBuffer animalParticlesMeasure32;
    private ComputeBuffer animalParticlesMeasure1;
    private AnimalParticleData[] animalParticleMeasurementTotalsData;

    private ComputeBuffer cursorClosestParticleDataCBuffer;
    public AnimalParticleData[] cursorParticleDataArray;
    private ComputeBuffer cursorDistances;
    public AnimalParticleData selectedAnimalParticleData;
    public AnimalParticleData closestAnimalParticleData;
    public bool isAnimalParticleSelected = false;
    public int selectedAnimalParticleIndex = 0;
    public int closestZooplanktonToCursorIndex = 0;

    public RenderTexture critterNearestZooplanktonRT;
    private ComputeBuffer closestMicrobeDistanceCBuffer;
    public Vector4[] closestMicrobeDistanceArray;  // counterpart to CBuffer
    public Vector4[] finalMicrobeDistancesArray; // final data to use in simulation

    private ComputeBuffer zooplanktonRepresentativeGenomeCBuffer;

    public WorldLayerZooplanktonGenome zooplanktonSlotGenomeCurrent;  // algae particles!  -- likely to be converted into plants eventually ***
    public WorldLayerZooplanktonGenome[] zooplanktonSlotGenomeMutations;
    
    public Vector2 closestAnimalParticlePosition2D => closestAnimalParticleData.worldPos2D;
    
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
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        animalParticlesCBuffer.SetData(animalParticlesArray);
        animalParticlesCBufferSwap.SetData(animalParticlesArray);
       
        animalParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        animalParticlesEatAmountsArray = new float[numAgents];

        animalParticleMeasurementTotalsData = new AnimalParticleData[maxNumMicrobes / 1024];
        animalParticlesMeasure32 = new ComputeBuffer(maxNumMicrobes / 32, GetAnimalParticleDataSize());
        animalParticlesMeasure1 = new ComputeBuffer(maxNumMicrobes / 1024, GetAnimalParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
        
        critterNearestZooplanktonRT = new RenderTexture(maxNumMicrobes / 32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        critterNearestZooplanktonRT.wrapMode = TextureWrapMode.Clamp;
        critterNearestZooplanktonRT.filterMode = FilterMode.Point;
        critterNearestZooplanktonRT.enableRandomWrite = true;        
        critterNearestZooplanktonRT.Create(); 
 ///***EAC  TRY:::: Formally splitting buffers into extended and condensed form
 ///    OR:: LEAVE OVERFILL in and just ignore it, use only first Entries per critter
 ///    avoids need for double buffers and extra copying
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        finalCritterClosestMicrobesArray = new AnimalParticleData[numAgents];
        closestMicrobesToCrittersCBuffer = new ComputeBuffer(numAgents*(maxNumMicrobes/1024), GetAnimalParticleDataSize());
        closestMicrobesToCrittersArray = new AnimalParticleData[closestMicrobesToCrittersCBuffer.count];
        
        finalMicrobeDistancesArray = new Vector4[numAgents];
        closestMicrobeDistanceCBuffer = new ComputeBuffer(numAgents*(maxNumMicrobes/1024), sizeof(float) * 4);
        closestMicrobeDistanceArray = new Vector4[closestMicrobeDistanceCBuffer.count];
        cursorClosestParticleDataCBuffer = new ComputeBuffer(2, GetAnimalParticleDataSize());  // 0 = selected, 1 = closest to cursor
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
        int kernelCSEatSelectedAnimalParticles = computeShaderAnimalParticles.FindKernel("CSEatSelectedAnimalParticles");
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "animalParticlesWrite", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "animalParticlesEatAmountsCBuffer", animalParticlesEatAmountsCBuffer);        
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "closestParticlesDataCBuffer", closestMicrobesToCrittersCBuffer);  
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "_ClosestZooplanktonCBuffer", closestMicrobeDistanceCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSEatSelectedAnimalParticles, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        animalParticlesEatAmountsCBuffer.GetData(animalParticlesEatAmountsArray);

        float totalAnimalEaten = 0f;
        for(int i = 0; i < animalParticlesEatAmountsCBuffer.count; i++) {
            totalAnimalEaten += animalParticlesEatAmountsArray[i];
        }
        // Copy/Swap Animal PArticle Buffer:
        int kernelCSCopyAnimalParticlesBuffer = computeShaderAnimalParticles.FindKernel("CSCopyAnimalParticlesBuffer");
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesRead", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesWrite", animalParticlesCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSCopyAnimalParticlesBuffer, Mathf.CeilToInt(maxNumMicrobes / 1024), 1, 1);
    }

    // need to send info on closest particle pos/dir/amt back to CPU also
    public void FindClosestAnimalParticleToCritters(SimulationStateData simStateDataRef) {
        int kernelCSNewMeasureDistancesInit = computeShaderAnimalParticles.FindKernel("CSNewMeasureDistancesInit");
        int kernelCSNewMeasureDistancesMainA = computeShaderAnimalParticles.FindKernel("CSNewMeasureDistancesMainA");

        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "animalParticlesRead", animalParticlesCBuffer);
        //computeShaderAnimalParticles.SetFloat("_NumAgents", SimulationManager.instance.maxAgents);
        computeShaderAnimalParticles.SetTexture(kernelCSNewMeasureDistancesInit, "_CritterToZooplanktonDistancesTexWrite", critterNearestZooplanktonRT); // INIT
        computeShaderAnimalParticles.SetTexture(kernelCSNewMeasureDistancesMainA, "_CritterToZooplanktonDistancesTexRead", critterNearestZooplanktonRT); /// MAINA
        computeShaderAnimalParticles.Dispatch(kernelCSNewMeasureDistancesInit, maxNumMicrobes / 32, simStateDataRef.critterSimDataCBuffer.count, 1);

        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "_ClosestZooplanktonCBuffer", closestMicrobeDistanceCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "closestParticlesDataCBuffer", closestMicrobesToCrittersCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSNewMeasureDistancesMainA, maxNumMicrobes / 1024, simStateDataRef.critterSimDataCBuffer.count, 1);

        AnimalParticleData[] microbeDataArray = new AnimalParticleData[closestMicrobesToCrittersCBuffer.count];
        closestMicrobesToCrittersCBuffer.GetData(microbeDataArray);// finalCritterClosestMicrobesArray); //ParticleData // condense to 1D buffer and interpret/process that output here?

        Vector4[] unpackedDistances = new Vector4[closestMicrobeDistanceCBuffer.count];
        closestMicrobeDistanceCBuffer.GetData(unpackedDistances); // DISTANCES VECTOR4
        
        //if (simManager.simAgeTimeSteps % 500 == 288) {
        //unpack if needed here:
        //string txt = "";
        for(int j=0; j< unpackedDistances.Length;j++) {

            //txt += unpackedDistances[j] + "\n";
        }

        for(int a = 0; a < simManager.maxAgents; a++) { //64?
            int closestMicrobeIndex = 0;
            float closestDistanceSqr = 100000f;
            AnimalParticleData data = new AnimalParticleData();
            for(int p = 0; p < (maxNumMicrobes / 1024); p++) { // 8
                int particleIndex = p * simManager.maxAgents + a;
                if(unpackedDistances[particleIndex].y < closestDistanceSqr) {
                    closestMicrobeIndex = (int)unpackedDistances[particleIndex].x;
                    closestDistanceSqr = unpackedDistances[particleIndex].y;
                    data = microbeDataArray[particleIndex];
                    //txt += particleIndex + "\n";
                }
            }
            finalCritterClosestMicrobesArray[a] = data;
            finalMicrobeDistancesArray[a].x = closestMicrobeIndex;
            finalMicrobeDistancesArray[a].y = closestDistanceSqr;
            //txt += a + ", ---- " + closestMicrobeIndex + ":  " + closestDistanceSqr + "\n";
                
        }
            //Debug.Log(txt + finalCritterClosestMicrobesArray[0].worldPos);
        //}
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
        computeShaderAnimalParticles.SetBuffer(kernelCSFetchParticleByID, "selectedAnimalParticleDataCBuffer", cursorClosestParticleDataCBuffer);      
        
        computeShaderAnimalParticles.SetInt("_SelectedParticleID", selectedAnimalParticleIndex);   
        computeShaderAnimalParticles.SetInt("_ClosestParticleID", Mathf.RoundToInt(swapBuffer[0].x)); 
        computeShaderAnimalParticles.SetBuffer(kernelCSFetchParticleByID, "animalParticlesRead", animalParticlesCBuffer);        
        computeShaderAnimalParticles.Dispatch(kernelCSFetchParticleByID, 1, 1, 1);

        cursorClosestParticleDataCBuffer.GetData(cursorParticleDataArray);

        //string txt = "nearestIndex = " + cursorParticleDataArray[0].index + " (" + cursorParticleDataArray[0].age +  ")   " + (cursorParticleDataArray[0].biomass * 1000f).ToString("F0"); // "lngth: " + swapBuffer.Length.ToString() + ",  " + swapBuffer[0].ToString() + "   " + swapBuffer[1].ToString() + "   " + swapBuffer[swapBuffer.Length - 2].ToString() + "   " + swapBuffer[swapBuffer.Length - 1].ToString();
        //Debug.Log(cursorParticleDataArray[1].index);
        closestAnimalParticleData = cursorParticleDataArray[1];
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
        int curAliveCount = 0;
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
        closestMicrobesToCrittersCBuffer?.Release();
        animalParticlesEatAmountsCBuffer?.Release();
        animalParticlesMeasure32?.Release();
        animalParticlesMeasure1?.Release();
        zooplanktonRepresentativeGenomeCBuffer?.Release();

        if(critterNearestZooplanktonRT != null) {
            critterNearestZooplanktonRT.Release();
        }
        
        closestMicrobeDistanceCBuffer?.Release();
        closestMicrobesToCrittersCBuffer?.Release();
        cursorDistances?.Release();
        cursorClosestParticleDataCBuffer?.Release();
    }
}
