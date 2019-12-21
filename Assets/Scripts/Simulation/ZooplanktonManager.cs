using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
///  Move Food-related stuff from SimulationManager into here to de-clutter simManager:
/// </summary>

public class ZooplanktonManager {

    public SettingsManager settingsRef;
    public SimResourceManager resourceManagerRef;
    
    private ComputeShader computeShaderAnimalParticles;
    
    private const int numAnimalParticles = 1024;  // *** 
    public ComputeBuffer animalParticlesCBuffer;
    private ComputeBuffer animalParticlesCBufferSwap;    
    //private RenderTexture animalParticlesNearestCritters1024;
    //private RenderTexture animalParticlesNearestCritters32;
    //private RenderTexture animalParticlesNearestCritters1;
    private ComputeBuffer closestAnimalParticlesDataCBuffer;
    public AnimalParticleData[] closestAnimalParticlesDataArray;
    private ComputeBuffer animalParticlesEatAmountsCBuffer;
    public float[] animalParticlesEatAmountsArray;
    private ComputeBuffer animalParticlesMeasure32;
    private ComputeBuffer animalParticlesMeasure1;
    private AnimalParticleData[] animalParticleMeasurementTotalsData;

    private ComputeBuffer cursorClosestParticleDataCBuffer;
    public AnimalParticleData[] cursorParticleDataArray;
    private ComputeBuffer cursorDistances1024;
    public AnimalParticleData selectedAnimalParticleData;
    public AnimalParticleData closestAnimalParticleData;
    public bool isAnimalParticleSelected = false;
    public int selectedAnimalParticleIndex = 0;
    public int closestZooplanktonToCursorIndex = 0;

    public RenderTexture critterNearestZooplankton32;
    private ComputeBuffer closestZooplanktonDistancesCBuffer;
    public Vector4[] closestZooplanktonArray;

    private ComputeBuffer zooplanktonRepresentativeGenomeCBuffer;

    public WorldLayerZooplanktonGenome zooplanktonSlotGenomeCurrent;  // algae particles!  -- likely to be converted into plants eventually ***
    public WorldLayerZooplanktonGenome[] zooplanktonSlotGenomeMutations;
    
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
    }
   
    private int GetAnimalParticleDataSize() {
        int bitSize = sizeof(float) * 26 + sizeof(int) * 3;
        return bitSize;
    }
    
	
    public ZooplanktonManager(SettingsManager settings, SimResourceManager resourcesRef) {
        settingsRef = settings;
        resourceManagerRef = resourcesRef;        
    }
    
    
    public void InitializeAnimalParticles(int numAgents, ComputeShader computeShader) {
        //float startTime = Time.realtimeSinceStartup;
        //Debug.Log((Time.realtimeSinceStartup - startTime).ToString());
        computeShaderAnimalParticles = computeShader;
        
        animalParticlesCBuffer = new ComputeBuffer(numAnimalParticles, GetAnimalParticleDataSize());
        animalParticlesCBufferSwap = new ComputeBuffer(numAnimalParticles, GetAnimalParticleDataSize());
        AnimalParticleData[] animalParticlesArray = new AnimalParticleData[numAnimalParticles];

        float minParticleSize = 0.1f; // settingsRef.avgAnimalParticleRadius / settingsRef.animalParticleRadiusVariance;
        float maxParticleSize = 0.2f; // settingsRef.avgAnimalParticleRadius * settingsRef.animalParticleRadiusVariance;

        for(int i = 0; i < animalParticlesCBuffer.count; i++) {
            AnimalParticleData data = new AnimalParticleData();
            data.index = i;
            data.worldPos = Vector3.zero; // new Vector3(UnityEngine.Random.Range(0f, SimulationManager._MapSize), UnityEngine.Random.Range(0f, SimulationManager._MapSize), 0f);

            data.radius = UnityEngine.Random.Range(minParticleSize, maxParticleSize); // obsolete!
            data.biomass = 0.001f; // data.radius * data.radius * Mathf.PI; // * settingsRef.animalParticleNutrientDensity;
            data.isActive = 0f;
            data.isDecaying = 0f;
            data.age = 0f; // UnityEngine.Random.Range(1f, 2f);
            data.color = UnityEngine.Random.ColorHSV();
            data.genomeVector = Vector4.zero;
            data.extra0 = 0f;
            data.energy = 0f;
            animalParticlesArray[i] = data;
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        animalParticlesCBuffer.SetData(animalParticlesArray);
        animalParticlesCBufferSwap.SetData(animalParticlesArray);
        //Debug.Log("Set Data GPU: " + (Time.realtimeSinceStartup - startTime).ToString());
        /*
        animalParticlesNearestCritters1024 = new RenderTexture(numAnimalParticles, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        animalParticlesNearestCritters1024.wrapMode = TextureWrapMode.Clamp;
        animalParticlesNearestCritters1024.filterMode = FilterMode.Point;
        animalParticlesNearestCritters1024.enableRandomWrite = true;        
        animalParticlesNearestCritters1024.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    
        //Debug.Log("Create RT 1024: " + (Time.realtimeSinceStartup - startTime).ToString());
        animalParticlesNearestCritters32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        animalParticlesNearestCritters32.wrapMode = TextureWrapMode.Clamp;
        animalParticlesNearestCritters32.filterMode = FilterMode.Point;
        animalParticlesNearestCritters32.enableRandomWrite = true;        
        animalParticlesNearestCritters32.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***   
        //Debug.Log("Create RT 32: " + (Time.realtimeSinceStartup - startTime).ToString());
        animalParticlesNearestCritters1 = new RenderTexture(1, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        animalParticlesNearestCritters1.wrapMode = TextureWrapMode.Clamp;
        animalParticlesNearestCritters1.filterMode = FilterMode.Point;
        animalParticlesNearestCritters1.enableRandomWrite = true;        
        animalParticlesNearestCritters1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***
        */
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        closestAnimalParticlesDataArray = new AnimalParticleData[numAgents];
        closestAnimalParticlesDataCBuffer = new ComputeBuffer(numAgents, GetAnimalParticleDataSize());

        animalParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        animalParticlesEatAmountsArray = new float[numAgents];

        animalParticleMeasurementTotalsData = new AnimalParticleData[1];
        animalParticlesMeasure32 = new ComputeBuffer(32, GetAnimalParticleDataSize());
        animalParticlesMeasure1 = new ComputeBuffer(1, GetAnimalParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());


        critterNearestZooplankton32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        critterNearestZooplankton32.wrapMode = TextureWrapMode.Clamp;
        critterNearestZooplankton32.filterMode = FilterMode.Point;
        critterNearestZooplankton32.enableRandomWrite = true;        
        critterNearestZooplankton32.Create(); 

        closestZooplanktonDistancesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        closestZooplanktonArray = new Vector4[numAgents];

        cursorClosestParticleDataCBuffer = new ComputeBuffer(2, GetAnimalParticleDataSize());  // 0 = selected, 1 = closest to cursor
        cursorParticleDataArray = new AnimalParticleData[2];
        cursorDistances1024 = new ComputeBuffer(1024, sizeof(float) * 4);


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


        //////+++++++++
        
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
            Vector4 randColor = UnityEngine.Random.ColorHSV();
            
            Vector4 col = zooplanktonSlotGenomeCurrent.representativeData.color;
            col = Vector4.Lerp(col, randColor, jLerp);
            mutatedGenome.representativeData = zooplanktonSlotGenomeCurrent.representativeData;
            mutatedGenome.representativeData.color = col;

            mutatedGenome.swimSpeed01 = Mathf.Lerp(0f, 1f, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.swimSpeed01 = Mathf.Lerp(zooplanktonSlotGenomeCurrent.swimSpeed01, mutatedGenome.swimSpeed01, jLerp);

            mutatedGenome.agingRate01 = Mathf.Lerp(0f, 1f, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.agingRate01 = Mathf.Lerp(zooplanktonSlotGenomeCurrent.agingRate01, mutatedGenome.agingRate01, jLerp);

            mutatedGenome.attractForce01 = Mathf.Lerp(0f, 1f, UnityEngine.Random.Range(0f, 1f));
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
    public void SimulateAnimalParticles(EnvironmentFluidManager fluidManagerRef, TheRenderKing renderKingRef, SimulationStateData simStateDataRef, SimResourceManager resourcesManager) { // Sim
        // Go through animalParticleData and check for inactive
        // determined by current total animal -- done!
        // if flag on shader for Respawn is on, set to active and initialize

        float maxAnimalParticleTotal = 2048f; // *** Revisit this! Arbitrary! // settingsRef.maxAnimalParticleTotalAmount;

        int kernelCSSimulateAnimalParticles = computeShaderAnimalParticles.FindKernel("CSSimulateAnimalParticles");
        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSSimulateAnimalParticles, "animalParticlesWrite", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "velocityRead", fluidManagerRef._VelocityPressureDivergenceMain);        
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightDataRT);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_SpawnDensityMap", renderKingRef.spiritBrushRT);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_ResourceGridRead", renderKingRef.simManager.vegetationManager.resourceGridRT1);
        //computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_SpawnDensityMap", algaeGridRT1);        
        computeShaderAnimalParticles.SetFloat("_GlobalOxygenLevel", resourcesManager.curGlobalOxygen); // needed?
        computeShaderAnimalParticles.SetFloat("_GlobalAlgaeLevel", resourceManagerRef.curGlobalPlantParticles);
        computeShaderAnimalParticles.SetFloat("_SpiritBrushPosNeg", renderKingRef.spiritBrushPosNeg);
        computeShaderAnimalParticles.SetFloat("_GlobalWaterLevel", renderKingRef.baronVonWater._GlobalWaterLevel);   
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

        // *** SPAWNING ***
        int eggSackIndex = Mathf.FloorToInt(Time.realtimeSinceStartup * 0.1f) % simStateDataRef.eggSackSimDataArray.Length;

        //if(animalParticleMeasurementTotalsData[0].biomass < maxAnimalParticleTotal) {
        float brushF = 0f;
        if(renderKingRef.isSpiritBrushOn) {
            if(renderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.kingdomID == 2) {  // Animals kingdom selected
                if(renderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.tierID == 0) {  // Zooplankton slot selected
                    brushF = 1f;
                }
            }            
        }
        //float randRoll = UnityEngine.Random.Range(0f, 1f);
        computeShaderAnimalParticles.SetFloat("_RespawnAnimalParticles", brushF);      
        computeShaderAnimalParticles.SetFloat("_IsBrushing", brushF);  
        //}
        //else {
        //    computeShaderAnimalParticles.SetFloat("_RespawnAnimalParticles", 0f);      
        //}
        // Need to compute when they should be allowed to spawn, how to keep track of resources used/transferred??
        computeShaderAnimalParticles.SetFloat("_SpawnPosX", UnityEngine.Random.Range(0.1f, 0.9f)); // UPDATE THIS!!! ****
        computeShaderAnimalParticles.SetFloat("_SpawnPosY", UnityEngine.Random.Range(0.1f, 0.9f));

        float spawnLerp = renderKingRef.simManager.trophicLayersManager.GetZooplanktonOnLerp(renderKingRef.simManager.simAgeTimeSteps);
        float spawnRadius = Mathf.Lerp(1f, SimulationManager._MapSize, spawnLerp);
        Vector4 spawnPos = new Vector4(renderKingRef.simManager.trophicLayersManager.zooplanktonOriginPos.x, renderKingRef.simManager.trophicLayersManager.zooplanktonOriginPos.y, 0f, 0f);
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

        computeShaderAnimalParticles.Dispatch(kernelCSSimulateAnimalParticles, 1, 1, 1);                

        // Copy/Swap Animal Particle Buffer:
        int kernelCSCopyAnimalParticlesBuffer = computeShaderAnimalParticles.FindKernel("CSCopyAnimalParticlesBuffer");
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesRead", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesWrite", animalParticlesCBuffer);        
        computeShaderAnimalParticles.Dispatch(kernelCSCopyAnimalParticlesBuffer, 1, 1, 1);        
        
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
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "closestParticlesDataCBuffer", closestAnimalParticlesDataCBuffer);  
        computeShaderAnimalParticles.SetBuffer(kernelCSEatSelectedAnimalParticles, "_ClosestZooplanktonCBuffer", closestZooplanktonDistancesCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSEatSelectedAnimalParticles, simStateDataRef.critterSimDataCBuffer.count, 1, 1);

        animalParticlesEatAmountsCBuffer.GetData(animalParticlesEatAmountsArray);

        float totalAnimalEaten = 0f;
        for(int i = 0; i < animalParticlesEatAmountsCBuffer.count; i++) {
            totalAnimalEaten += animalParticlesEatAmountsArray[i];
        }
        // Copy/Swap Animal PArticle Buffer:
        int kernelCSCopyAnimalParticlesBuffer = computeShaderAnimalParticles.FindKernel("CSCopyAnimalParticlesBuffer");
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesRead", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesWrite", animalParticlesCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSCopyAnimalParticlesBuffer, 1, 1, 1);
    }
    public void FindClosestAnimalParticleToCritters(SimulationStateData simStateDataRef) {  // need to send info on closest particle pos/dir/amt back to CPU also        
        
        int kernelCSNewMeasureDistancesInit = computeShaderAnimalParticles.FindKernel("CSNewMeasureDistancesInit");
        int kernelCSNewMeasureDistancesMainA = computeShaderAnimalParticles.FindKernel("CSNewMeasureDistancesMainA");


        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "animalParticlesRead", animalParticlesCBuffer);        
        computeShaderAnimalParticles.SetTexture(kernelCSNewMeasureDistancesInit, "_CritterToZooplanktonDistancesTexWrite", critterNearestZooplankton32);

        computeShaderAnimalParticles.SetTexture(kernelCSNewMeasureDistancesMainA, "_CritterToZooplanktonDistancesTexRead", critterNearestZooplankton32);

        computeShaderAnimalParticles.Dispatch(kernelCSNewMeasureDistancesInit, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        
        //computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        //computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "animalParticlesRead", animalParticlesCBuffer);      
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "_ClosestZooplanktonCBuffer", closestZooplanktonDistancesCBuffer);    
          
        computeShaderAnimalParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "closestParticlesDataCBuffer", closestAnimalParticlesDataCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSNewMeasureDistancesMainA, 1, simStateDataRef.critterSimDataCBuffer.count, 1);
        

        closestAnimalParticlesDataCBuffer.GetData(closestAnimalParticlesDataArray);
        closestZooplanktonDistancesCBuffer.GetData(closestZooplanktonArray);

        /*string txt = "";
        for(int i = 0; i < simStateDataRef.critterSimDataCBuffer.count; i++) {
            txt += "[" + i.ToString() + ": " + closestZooplanktonArray[i].x.ToString() + ", " + closestZooplanktonArray[i].y.ToString() + "]  ";
        }
        Debug.Log(txt);*/

        // OLD ************************
        /*
        // Populate main RenderTexture with distances for each animalParticle to each Critter:
        int kernelCSMeasureInitCritterDistances = computeShaderAnimalParticles.FindKernel("CSMeasureInitCritterDistances");
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "animalParticlesRead", animalParticlesCBuffer);        
        computeShaderAnimalParticles.SetTexture(kernelCSMeasureInitCritterDistances, "animalParticlesNearestCrittersRT", animalParticlesNearestCritters1024);        
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureInitCritterDistances, animalParticlesCBuffer.count / 1024, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 1024 --> 32 particles per critter:
        int kernelCSReduceCritterDistances32 = computeShaderAnimalParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderAnimalParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", animalParticlesNearestCritters1024);
        computeShaderAnimalParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", animalParticlesNearestCritters32);
        computeShaderAnimalParticles.SetBuffer(kernelCSReduceCritterDistances32, "animalParticlesRead", animalParticlesCBuffer);        
        computeShaderAnimalParticles.Dispatch(kernelCSReduceCritterDistances32, 32, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 32 --> 1 particles per critter:
        computeShaderAnimalParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", animalParticlesNearestCritters32);
        computeShaderAnimalParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", animalParticlesNearestCritters1);
        computeShaderAnimalParticles.SetBuffer(kernelCSReduceCritterDistances32, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestAnimalParticlesDataCBuffer);
        computeShaderAnimalParticles.Dispatch(kernelCSReduceCritterDistances32, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        closestAnimalParticlesDataCBuffer.GetData(closestAnimalParticlesDataArray);
        */
    }
    private Vector4[] ReduceDistancesArray(Vector4[] inBuffer) {

        Vector4[] newBuffer = new Vector4[(inBuffer.Length / 2)];
        for (int i = 0; i < newBuffer.Length; i++) {
            Vector4 cellDataA = inBuffer[i * 2];
            Vector4 cellDataB = inBuffer[i * 2 + 1];

            if(cellDataA.y <= cellDataB.y) {  // A is closer
                newBuffer[i] = cellDataA;
            }
            else {
                newBuffer[i] = cellDataB;
            }
        }

        return newBuffer;
    }
    public void FindClosestAnimalParticleToCursor(float xCoord, float yCoord) {
        
        int kernelCSMeasureInitCursorDistances = computeShaderAnimalParticles.FindKernel("CSMeasureInitCursorDistances");        
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCursorDistances, "animalParticlesRead", animalParticlesCBuffer);  
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureInitCursorDistances, "cursorDistancesWrite", cursorDistances1024);
        computeShaderAnimalParticles.SetFloat("_MouseCoordX", xCoord);
        computeShaderAnimalParticles.SetFloat("_MouseCoordY", yCoord);
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureInitCursorDistances, animalParticlesCBuffer.count / 32, 1, 1);

        Vector4[] cursorDistanceArray1024 = new Vector4[1024];
        cursorDistances1024.GetData(cursorDistanceArray1024);

       
        
        // Manual Sort!
        Vector4[] swapBuffer = cursorDistanceArray1024;
        swapBuffer = ReduceDistancesArray(cursorDistanceArray1024);        
        for(int tierID = 0; tierID < 9; tierID++) {            
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
        //Debug.Log(txt);
        closestAnimalParticleData = cursorParticleDataArray[1];
        selectedAnimalParticleData = cursorParticleDataArray[0];

    }
    public void MeasureTotalAnimalParticlesAmount() {
        
        int kernelCSMeasureTotalAnimalParticlesAmount = computeShaderAnimalParticles.FindKernel("CSMeasureTotalAnimalParticlesAmount");
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesRead", animalParticlesCBuffer);
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesWrite", animalParticlesMeasure32);
         
        // DISPATCH !!!
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureTotalAnimalParticlesAmount, 32, 1, 1);
        
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesRead", animalParticlesMeasure32);
        computeShaderAnimalParticles.SetBuffer(kernelCSMeasureTotalAnimalParticlesAmount, "animalParticlesWrite", animalParticlesMeasure1);
        computeShaderAnimalParticles.Dispatch(kernelCSMeasureTotalAnimalParticlesAmount, 1, 1, 1);
        
        animalParticlesMeasure1.GetData(animalParticleMeasurementTotalsData);
        resourceManagerRef.curGlobalAnimalParticles = animalParticleMeasurementTotalsData[0].biomass;
        resourceManagerRef.oxygenUsedByAnimalParticlesLastFrame = animalParticleMeasurementTotalsData[0].oxygenUsed;
        resourceManagerRef.wasteProducedByAnimalParticlesLastFrame = animalParticleMeasurementTotalsData[0].wasteProduced;
        resourceManagerRef.algaeConsumedByAnimalParticlesLastFrame = animalParticleMeasurementTotalsData[0].algaeConsumed;

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
        if(animalParticlesCBuffer != null) {
            animalParticlesCBuffer.Release();
        }  
        if(animalParticlesCBufferSwap != null) {
            animalParticlesCBufferSwap.Release();
        } 
        if(closestAnimalParticlesDataCBuffer != null) {
            closestAnimalParticlesDataCBuffer.Release();
        }
        if(animalParticlesEatAmountsCBuffer != null) {
            animalParticlesEatAmountsCBuffer.Release();
        }
        if(animalParticlesMeasure32 != null) {
            animalParticlesMeasure32.Release();
        }
        if(animalParticlesMeasure1 != null) {
            animalParticlesMeasure1.Release();
        }
        if(zooplanktonRepresentativeGenomeCBuffer != null) {
            zooplanktonRepresentativeGenomeCBuffer.Release();
        }

        if(critterNearestZooplankton32 != null) {
            critterNearestZooplankton32.Release();
        }
        if(closestZooplanktonDistancesCBuffer != null) {
            closestZooplanktonDistancesCBuffer.Release();
        }

        if(closestAnimalParticlesDataCBuffer != null) {
            closestAnimalParticlesDataCBuffer.Release();
        }
        if(cursorDistances1024 != null) {
            cursorDistances1024.Release();
        }
        if(cursorClosestParticleDataCBuffer != null) {
            cursorClosestParticleDataCBuffer.Release();
        } 
        
    }
}
