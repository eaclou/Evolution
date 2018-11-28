using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
///  Move Food-related stuff from SimulationManager into here to de-clutter simManager:
/// </summary>
public class FoodManager {

    public SettingsManager settingsRef;

    private ComputeShader computeShaderNutrientMap;
    private ComputeShader computeShaderFoodParticles;

    public float curGlobalNutrients = 0f;
    public float curGlobalFoodParticles = 0f;

    public int nutrientMapResolution = 32;
    public RenderTexture nutrientMapRT1;
    public RenderTexture nutrientMapRT2;
    public Vector4[] nutrientSamplesArray;
    public Vector4[] nutrientEatAmountsArray;

    private RenderTexture tempTex16;
    private RenderTexture tempTex8;
    private RenderTexture tempTex4;
    private RenderTexture tempTex2;
    private RenderTexture tempTex1;
    
    private ComputeBuffer nutrientSamplesCBuffer;

    private const int numFoodParticles = 1024;  // *** 
    public ComputeBuffer foodParticlesCBuffer;
    private ComputeBuffer foodParticlesCBufferSwap;    
    private RenderTexture foodParticlesNearestCritters1024;
    private RenderTexture foodParticlesNearestCritters32;
    private RenderTexture foodParticlesNearestCritters1;
    private ComputeBuffer closestFoodParticlesDataCBuffer;
    public FoodParticleData[] closestFoodParticlesDataArray;
    private ComputeBuffer foodParticlesEatAmountsCBuffer;
    public float[] foodParticlesEatAmountsArray;
    private ComputeBuffer foodParticlesMeasure32;
    private ComputeBuffer foodParticlesMeasure1;
    private FoodParticleData[] foodParticleMeasurementTotalsData;

    public Vector2[] nutrientPatchesArray;
    
    public struct FoodParticleData {
        public int index;
        public int critterIndex;
        public int nearestCritterIndex;
        public float isSwallowed;   // 0 = normal, 1 = in critter's belly
        public float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk
        public Vector2 worldPos;
        public float radius;
        public float foodAmount;
        public float active;  // not disabled
        public float refactoryAge;
    }
    
    private int GetFoodParticleDataSize() {
        int bitSize = sizeof(float) * 8 + sizeof(int) * 3;
        return bitSize;
    }

    public void MoveRandomNutrientPatches(int index) {
        nutrientPatchesArray[index] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
        Debug.Log("Moved Nutrient Patch! [" + index.ToString() + "], " + nutrientPatchesArray[index].ToString());
    }
	
    public FoodManager(SettingsManager settings) {
        settingsRef = settings;

        nutrientPatchesArray = new Vector2[4];
        for(int i = 0; i < nutrientPatchesArray.Length; i++) {
            nutrientPatchesArray[i] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
        }
    }

    public void InitializeFoodParticles(int numAgents, ComputeShader computeShader) {
        //float startTime = Time.realtimeSinceStartup;
        //Debug.Log((Time.realtimeSinceStartup - startTime).ToString());
        computeShaderFoodParticles = computeShader;
        
        foodParticlesCBuffer = new ComputeBuffer(numFoodParticles, GetFoodParticleDataSize());
        foodParticlesCBufferSwap = new ComputeBuffer(numFoodParticles, GetFoodParticleDataSize());
        FoodParticleData[] foodParticlesArray = new FoodParticleData[numFoodParticles];

        float minParticleSize = settingsRef.avgFoodParticleRadius / settingsRef.foodParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgFoodParticleRadius * settingsRef.foodParticleRadiusVariance;

        for(int i = 0; i < foodParticlesCBuffer.count; i++) {
            FoodParticleData data = new FoodParticleData();
            data.index = i;            
            data.worldPos = new Vector2(UnityEngine.Random.Range(0f, SimulationManager._MapSize), UnityEngine.Random.Range(0f, SimulationManager._MapSize));

            data.radius = UnityEngine.Random.Range(minParticleSize, maxParticleSize);
            data.foodAmount = data.radius * data.radius * Mathf.PI * settingsRef.foodParticleNutrientDensity;
            data.active = 1f;
            data.refactoryAge = 0f;
            foodParticlesArray[i] = data;
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        foodParticlesCBuffer.SetData(foodParticlesArray);
        foodParticlesCBufferSwap.SetData(foodParticlesArray);
        //Debug.Log("Set Data GPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        foodParticlesNearestCritters1024 = new RenderTexture(numFoodParticles, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        foodParticlesNearestCritters1024.wrapMode = TextureWrapMode.Clamp;
        foodParticlesNearestCritters1024.filterMode = FilterMode.Point;
        foodParticlesNearestCritters1024.enableRandomWrite = true;        
        foodParticlesNearestCritters1024.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    
        //Debug.Log("Create RT 1024: " + (Time.realtimeSinceStartup - startTime).ToString());
        foodParticlesNearestCritters32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        foodParticlesNearestCritters32.wrapMode = TextureWrapMode.Clamp;
        foodParticlesNearestCritters32.filterMode = FilterMode.Point;
        foodParticlesNearestCritters32.enableRandomWrite = true;        
        foodParticlesNearestCritters32.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***   
        //Debug.Log("Create RT 32: " + (Time.realtimeSinceStartup - startTime).ToString());
        foodParticlesNearestCritters1 = new RenderTexture(1, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        foodParticlesNearestCritters1.wrapMode = TextureWrapMode.Clamp;
        foodParticlesNearestCritters1.filterMode = FilterMode.Point;
        foodParticlesNearestCritters1.enableRandomWrite = true;        
        foodParticlesNearestCritters1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        closestFoodParticlesDataArray = new FoodParticleData[numAgents];
        closestFoodParticlesDataCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 8 + sizeof(int) * 3);

        foodParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        foodParticlesEatAmountsArray = new float[numAgents];

        foodParticleMeasurementTotalsData = new FoodParticleData[1];
        foodParticlesMeasure32 = new ComputeBuffer(32, sizeof(float) * 8 + sizeof(int) * 3);
        foodParticlesMeasure1 = new ComputeBuffer(1, sizeof(float) * 8 + sizeof(int) * 3);
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
    }
    public void InitializeNutrientsMap(int numAgents, ComputeShader computeShader) {

        computeShaderNutrientMap = computeShader;
              
        nutrientMapRT1 = new RenderTexture(nutrientMapResolution, nutrientMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        nutrientMapRT1.wrapMode = TextureWrapMode.Clamp;
        nutrientMapRT1.filterMode = FilterMode.Bilinear;
        nutrientMapRT1.enableRandomWrite = true;
        //nutrientMapRT1.useMipMap = true;
        nutrientMapRT1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    

        nutrientMapRT2 = new RenderTexture(nutrientMapResolution, nutrientMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        nutrientMapRT2.wrapMode = TextureWrapMode.Clamp;
        nutrientMapRT2.enableRandomWrite = true;
        //nutrientMapRT2.useMipMap = true;
        nutrientMapRT2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***  
        
        nutrientSamplesArray = new Vector4[numAgents];
        nutrientEatAmountsArray = new Vector4[numAgents];

        int kernelCSInitializeNutrientMap = computeShaderNutrientMap.FindKernel("CSInitializeNutrientMap");
        computeShaderNutrientMap.SetTexture(kernelCSInitializeNutrientMap, "nutrientMapWrite", nutrientMapRT1);
        computeShaderNutrientMap.Dispatch(kernelCSInitializeNutrientMap, nutrientMapResolution / 32, nutrientMapResolution / 32, 1);
        Graphics.Blit(nutrientMapRT1, nutrientMapRT2);

        tempTex16 = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex16.wrapMode = TextureWrapMode.Clamp;
        tempTex16.filterMode = FilterMode.Point;
        tempTex16.enableRandomWrite = true;
        tempTex16.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex8 = new RenderTexture(8, 8, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex8.wrapMode = TextureWrapMode.Clamp;
        tempTex8.filterMode = FilterMode.Point;
        tempTex8.enableRandomWrite = true;
        tempTex8.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex4 = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex4.wrapMode = TextureWrapMode.Clamp;
        tempTex4.filterMode = FilterMode.Point;
        tempTex4.enableRandomWrite = true;
        tempTex4.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex2 = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex2.wrapMode = TextureWrapMode.Clamp;
        tempTex2.filterMode = FilterMode.Point;
        tempTex2.enableRandomWrite = true;
        tempTex2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex1 = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex1.wrapMode = TextureWrapMode.Clamp;
        tempTex1.filterMode = FilterMode.Point;
        tempTex1.enableRandomWrite = true;
        tempTex1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);

        //theRenderKing.fluidRenderMat.SetTexture("_DebugTex", nutrientMapRT1);
        
    }

    public void ApplyDiffusionOnNutrientMap(EnvironmentFluidManager fluidManagerRef) {
        int kernelCSUpdateNutrientMap = computeShaderNutrientMap.FindKernel("CSUpdateNutrientMap");
        computeShaderNutrientMap.SetFloat("_NutrientDiffusion", settingsRef.foodDiffusionRate);
        computeShaderNutrientMap.SetTexture(kernelCSUpdateNutrientMap, "ObstaclesRead", fluidManagerRef._ObstaclesRT);
        computeShaderNutrientMap.SetTexture(kernelCSUpdateNutrientMap, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSUpdateNutrientMap, "nutrientMapWrite", nutrientMapRT2);
        computeShaderNutrientMap.Dispatch(kernelCSUpdateNutrientMap, nutrientMapResolution / 32, nutrientMapResolution / 32, 1);

        Graphics.Blit(nutrientMapRT2, nutrientMapRT1);
        
    }
    public void GetNutrientValuesAtMouthPositions(SimulationStateData simStateDataRef) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        //ComputeBuffer nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        
        int kernelCSGetNutrientSamples = computeShaderNutrientMap.FindKernel("CSGetNutrientSamples");   
        //computeShaderNutrientMap.SetBuffer(kernelCSGetNutrientSamples, "critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        computeShaderNutrientMap.SetBuffer(kernelCSGetNutrientSamples, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderNutrientMap.SetBuffer(kernelCSGetNutrientSamples, "nutrientSamplesCBuffer", nutrientSamplesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSGetNutrientSamples, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderNutrientMap.Dispatch(kernelCSGetNutrientSamples, nutrientSamplesCBuffer.count, 1, 1);

        //Vector4[] outArray = new Vector4[_NumAgents];
        nutrientSamplesCBuffer.GetData(nutrientSamplesArray); // Disappearing body strokes due to this !?!?!?!?!?

        //Debug.Log("Food: " + nutrientSamplesArray[0].x.ToString());
        //nutrientSamplesCBuffer.Release();
        
        // Read out sample values::::
    }
    public float MeasureTotalNutrients() {

        ComputeBuffer outputValuesCBuffer = new ComputeBuffer(1, sizeof(float) * 4);  // holds the result of measurement: total sum of pix colors in texture
        Vector4[] outputValuesArray = new Vector4[1];

        // 32 --> 16:
        int kernelCSMeasureTotalNutrients = computeShaderNutrientMap.FindKernel("CSMeasureTotalNutrients");   
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex16);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 16, 16, 1);
        // 16 --> 8:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex16);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex8);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 8, 8, 1);
        // 8 --> 4:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex8);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex4);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 4, 4, 1);        
        // 4 --> 2:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex4);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex2);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 2, 2, 1);
        // 2 --> 1:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex2);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex1);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 1, 1, 1);
        
        outputValuesCBuffer.GetData(outputValuesArray);

        curGlobalNutrients = outputValuesArray[0].x;

        outputValuesCBuffer.Release();

        //Debug.Log("TotalNutrients: " + outputValuesArray[0].x.ToString() + ", " + outputValuesArray[0].y.ToString());

        return outputValuesArray[0].x;
    }     
    public void AddNutrientsAtCoords(float amount, int x, int y) {
        if(curGlobalNutrients < 128f) {
            ComputeBuffer addNutrientsCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
            Vector4[] addNutrientsArray = new Vector4[1];
            addNutrientsArray[0] = new Vector4(amount, (float)x / 32f, (float)y / 32f, 1f);
            addNutrientsCBuffer.SetData(addNutrientsArray);

            int kernelCSAddNutrientsAtCoords = computeShaderNutrientMap.FindKernel("CSAddNutrientsAtCoords");
            computeShaderNutrientMap.SetBuffer(kernelCSAddNutrientsAtCoords, "addNutrientsCBuffer", addNutrientsCBuffer);        
            computeShaderNutrientMap.SetTexture(kernelCSAddNutrientsAtCoords, "nutrientMapRead", nutrientMapRT1);
            computeShaderNutrientMap.SetTexture(kernelCSAddNutrientsAtCoords, "nutrientMapWrite", nutrientMapRT2);
            computeShaderNutrientMap.Dispatch(kernelCSAddNutrientsAtCoords, addNutrientsCBuffer.count, 1, 1);
        
            Graphics.Blit(nutrientMapRT2, nutrientMapRT1);

            addNutrientsCBuffer.Release();
        }
        else {
            Debug.Log("Can't add nutrients, exceeds max level");
        }

        
    }
    public void RemoveEatenNutrients(int numAgents, SimulationStateData simStateDataRef) {
        ComputeBuffer eatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
                
        eatAmountsCBuffer.SetData(nutrientEatAmountsArray);

        int kernelCSRemoveNutrientsAtLocations = computeShaderNutrientMap.FindKernel("CSRemoveNutrientsAtLocations");
        computeShaderNutrientMap.SetBuffer(kernelCSRemoveNutrientsAtLocations, "nutrientEatAmountsCBuffer", eatAmountsCBuffer);
        computeShaderNutrientMap.SetBuffer(kernelCSRemoveNutrientsAtLocations, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSRemoveNutrientsAtLocations, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSRemoveNutrientsAtLocations, "nutrientMapWrite", nutrientMapRT2);
        computeShaderNutrientMap.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderNutrientMap.Dispatch(kernelCSRemoveNutrientsAtLocations, eatAmountsCBuffer.count, 1, 1);

        Graphics.Blit(nutrientMapRT2, nutrientMapRT1);
        
        eatAmountsCBuffer.Release();
    }
    public void ReviveSelectFoodParticles(int[] indicesArray, float radius, Vector4 spawnCoords, SimulationStateData simStateDataRef) {

        ComputeBuffer selectRespawnFoodParticleIndicesCBuffer = new ComputeBuffer(indicesArray.Length, sizeof(int));
        selectRespawnFoodParticleIndicesCBuffer.SetData(indicesArray);
        //selectRespawnFoodParticleIndicesCBuffer

        int kernelCSReviveSelectFoodParticles = computeShaderFoodParticles.FindKernel("CSReviveSelectFoodParticles");
        computeShaderFoodParticles.SetBuffer(kernelCSReviveSelectFoodParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSReviveSelectFoodParticles, "selectRespawnFoodParticleIndicesCBuffer", selectRespawnFoodParticleIndicesCBuffer);
        
        computeShaderFoodParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderFoodParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderFoodParticles.SetVector("_FoodSprinklePos", spawnCoords);
        computeShaderFoodParticles.SetFloat("_FoodSprinkleRadius", radius);
        computeShaderFoodParticles.SetBuffer(kernelCSReviveSelectFoodParticles, "foodParticlesWrite", foodParticlesCBufferSwap);
        computeShaderFoodParticles.Dispatch(kernelCSReviveSelectFoodParticles, indicesArray.Length, 1, 1);
       

        selectRespawnFoodParticleIndicesCBuffer.Release();
    }
    public void RespawnFoodParticles(EnvironmentFluidManager fluidManagerRef, TheRenderKing renderKingRef, SimulationStateData simStateDataRef) {
        // Go through foodParticleData and check for inactive
        // determined by current total food -- done!
        // if flag on shader for Respawn is on, set to active and initialize

        float maxFoodParticleTotal = settingsRef.maxFoodParticleTotalAmount;

        int kernelCSRespawnFoodParticles = computeShaderFoodParticles.FindKernel("CSRespawnFoodParticles");
        computeShaderFoodParticles.SetBuffer(kernelCSRespawnFoodParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSRespawnFoodParticles, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSRespawnFoodParticles, "foodParticlesWrite", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetTexture(kernelCSRespawnFoodParticles, "velocityRead", fluidManagerRef._VelocityA);        
        computeShaderFoodParticles.SetTexture(kernelCSRespawnFoodParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightMap);
        computeShaderFoodParticles.SetTexture(kernelCSRespawnFoodParticles, "_SpawnDensityMap", nutrientMapRT1);
        computeShaderFoodParticles.SetFloat("_MapSize", SimulationManager._MapSize);
            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderFoodParticles.SetFloat("_Time", Time.realtimeSinceStartup);

        if(foodParticleMeasurementTotalsData[0].foodAmount < maxFoodParticleTotal) {
            computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);                       
        }
        else {
            computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 0f);      
        }

        float minParticleSize = settingsRef.avgFoodParticleRadius / settingsRef.foodParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgFoodParticleRadius * settingsRef.foodParticleRadiusVariance;

        computeShaderFoodParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderFoodParticles.SetFloat("_MaxParticleSize", maxParticleSize);      
        computeShaderFoodParticles.SetFloat("_ParticleNutrientDensity", settingsRef.foodParticleNutrientDensity);
        computeShaderFoodParticles.SetFloat("_FoodParticleRegrowthRate", settingsRef.foodParticleRegrowthRate);

        computeShaderFoodParticles.Dispatch(kernelCSRespawnFoodParticles, 1, 1, 1);
                

        // Copy/Swap Food Particle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderFoodParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", foodParticlesCBuffer);        
        computeShaderFoodParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
        
        
    }
    public void EatSelectedFoodParticles(SimulationStateData simStateDataRef) {  // removes gpu particle & sends consumption data back to CPU
        // Use CritterSimData to determine critter mouth locations

        // run through all foodParticles, check against each critter position, then measure min value with recursive reduction:

        // Need to update CritterSim&InitData structs to have more mouth/bite info

        // Record how much food successfully eaten per Critter

        int kernelCSEatSelectedFoodParticles = computeShaderFoodParticles.FindKernel("CSEatSelectedFoodParticles");
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesWrite", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesEatAmountsCBuffer", foodParticlesEatAmountsCBuffer);        
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "closestParticlesDataCBuffer", closestFoodParticlesDataCBuffer);  
        computeShaderFoodParticles.SetTexture(kernelCSEatSelectedFoodParticles, "critterDistancesRead", foodParticlesNearestCritters1);
        computeShaderFoodParticles.Dispatch(kernelCSEatSelectedFoodParticles, simStateDataRef.critterSimDataCBuffer.count, 1, 1);

        foodParticlesEatAmountsCBuffer.GetData(foodParticlesEatAmountsArray);

        float totalFoodEaten = 0f;
        for(int i = 0; i < foodParticlesEatAmountsCBuffer.count; i++) {
            totalFoodEaten += foodParticlesEatAmountsArray[i];
        }
        // Copy/Swap Food PArticle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderFoodParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", foodParticlesCBuffer);
        computeShaderFoodParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
    }
    public void FindClosestFoodParticleToCritters(SimulationStateData simStateDataRef) {  // need to send info on closest particle pos/dir/amt back to CPU also
        
        // Populate main RenderTexture with distances for each foodParticle to each Critter:

        int kernelCSMeasureInitCritterDistances = computeShaderFoodParticles.FindKernel("CSMeasureInitCritterDistances");
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetTexture(kernelCSMeasureInitCritterDistances, "foodParticlesNearestCrittersRT", foodParticlesNearestCritters1024);        
        computeShaderFoodParticles.Dispatch(kernelCSMeasureInitCritterDistances, foodParticlesCBuffer.count / 1024, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 1024 --> 32 particles per critter:
        int kernelCSReduceCritterDistances32 = computeShaderFoodParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", foodParticlesNearestCritters1024);
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", foodParticlesNearestCritters32);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.Dispatch(kernelCSReduceCritterDistances32, 32, simStateDataRef.critterSimDataCBuffer.count, 1);

        // Reduce from 32 --> 1 particles per critter:
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", foodParticlesNearestCritters32);
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", foodParticlesNearestCritters1);
        computeShaderFoodParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestFoodParticlesDataCBuffer);
        computeShaderFoodParticles.Dispatch(kernelCSReduceCritterDistances32, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        closestFoodParticlesDataCBuffer.GetData(closestFoodParticlesDataArray);

        //Debug.Log("ClosestFoodParticle: " + closestFoodParticlesDataArray[0].index.ToString() + ", " + closestFoodParticlesDataArray[0].worldPos.ToString() + ", amt: " + closestFoodParticlesDataArray[0].foodAmount.ToString());
    }
    public void MeasureTotalFoodParticlesAmount() {
        
        int kernelCSMeasureTotalFoodParticlesAmount = computeShaderFoodParticles.FindKernel("CSMeasureTotalFoodParticlesAmount");
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", foodParticlesMeasure32);
         
        // DISPATCH !!!
        computeShaderFoodParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 32, 1, 1);
        
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", foodParticlesMeasure32);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", foodParticlesMeasure1);
        computeShaderFoodParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 1, 1, 1);
        
        foodParticlesMeasure1.GetData(foodParticleMeasurementTotalsData);
        curGlobalFoodParticles = foodParticleMeasurementTotalsData[0].foodAmount;
        
    }

    public void ClearBuffers() {

        if (tempTex1 != null) {
            tempTex1.Release();
            tempTex2.Release();
            tempTex4.Release();
            tempTex8.Release();
            tempTex16.Release();
        }
        if(foodParticlesNearestCritters1 != null) {
            foodParticlesNearestCritters1.Release();
            foodParticlesNearestCritters32.Release();
            foodParticlesNearestCritters1024.Release();
        }
        if(nutrientSamplesCBuffer != null) {
            nutrientSamplesCBuffer.Release();
        }
        if(foodParticlesCBuffer != null) {
            foodParticlesCBuffer.Release();
        }  
        if(foodParticlesCBufferSwap != null) {
            foodParticlesCBufferSwap.Release();
        } 
        if(closestFoodParticlesDataCBuffer != null) {
            closestFoodParticlesDataCBuffer.Release();
        }
        if(foodParticlesEatAmountsCBuffer != null) {
            foodParticlesEatAmountsCBuffer.Release();
        }
        if(foodParticlesMeasure32 != null) {
            foodParticlesMeasure32.Release();
        }
        if(foodParticlesMeasure1 != null) {
            foodParticlesMeasure1.Release();
        }
    }
}
