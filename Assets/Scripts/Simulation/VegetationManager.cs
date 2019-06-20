using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationManager {

    public SettingsManager settingsRef;
    public SimResourceManager resourceManagerRef;
    
    private ComputeShader computeShaderResourceGrid;
    private ComputeShader computeShaderAlgaeParticles;

    //public float curGlobalAlgaeGrid = 0f;  // not using this currently
    
    public int resourceGridTexResolution = 32; // Temporarily disabled - replaced by single value (1x1 grid)
    public RenderTexture resourceGridRT1;
    public RenderTexture resourceGridRT2;
    public Vector4[] resourceGridSamplesArray;
    public Vector4[] resourceGridEatAmountsArray;

    public RenderTexture rdRT1;
    public RenderTexture rdRT2;

    private RenderTexture tempTex16;
    private RenderTexture tempTex8;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex4;
    private RenderTexture tempTex2;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex1;
    
    private ComputeBuffer resourceGridAgentSamplesCBuffer;

    private const int numAlgaeParticles = 1024;  // *** 
    public ComputeBuffer algaeParticlesCBuffer;
    private ComputeBuffer algaeParticlesCBufferSwap;    
    private RenderTexture algaeParticlesNearestCritters1024;
    private RenderTexture algaeParticlesNearestCritters32;
    private RenderTexture algaeParticlesNearestCritters1;
    private ComputeBuffer closestAlgaeParticlesDataCBuffer;
    public AlgaeParticleData[] closestAlgaeParticlesDataArray;
    private ComputeBuffer algaeParticlesEatAmountsCBuffer;
    public float[] algaeParticlesEatAmountsArray;
    private ComputeBuffer algaeParticlesMeasure32;
    private ComputeBuffer algaeParticlesMeasure1;
    private AlgaeParticleData[] algaeParticleMeasurementTotalsData;
    
    public Vector2[] resourceGridSpawnPatchesArray;

    //public Vector2 algaeOriginPos;
    //public float algaeOnLerp01;
        
    public struct AlgaeParticleData {
        public int index;
        public int critterIndex;
        public int nearestCritterIndex;
        public float isSwallowed;   // 0 = normal, 1 = in critter's belly
        public float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk
        public Vector2 worldPos;
        public float radius;
        public float biomass;
        public float isActive;  // not disabled
        public float isDecaying;
        public float age;
        public float oxygenProduced;
        public float nutrientsUsed;
        public float wasteProduced;
        public Vector3 color;
    }
    
    private int GetAlgaeParticleDataSize() {
        int bitSize = sizeof(float) * 15 + sizeof(int) * 3;
        return bitSize;
    }

    public void MoveRandomResourceGridPatches(int index) {
        resourceGridSpawnPatchesArray[index] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
        Debug.Log("Moved Resource Patch! [" + index.ToString() + "], " + resourceGridSpawnPatchesArray[index].ToString());
    }
	
    public VegetationManager(SettingsManager settings, SimResourceManager resourcesRef) {
        settingsRef = settings;
        resourceManagerRef = resourcesRef;

        resourceGridSpawnPatchesArray = new Vector2[4]; // *** Refactor this!!! ***
        for(int i = 0; i < resourceGridSpawnPatchesArray.Length; i++) {
            resourceGridSpawnPatchesArray[i] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
        }
    }
    
    public void InitializeAlgaeParticles(int numAgents, ComputeShader computeShader) {
        //float startTime = Time.realtimeSinceStartup;
        //Debug.Log((Time.realtimeSinceStartup - startTime).ToString());
        computeShaderAlgaeParticles = computeShader;
        
        algaeParticlesCBuffer = new ComputeBuffer(numAlgaeParticles, GetAlgaeParticleDataSize());
        algaeParticlesCBufferSwap = new ComputeBuffer(numAlgaeParticles, GetAlgaeParticleDataSize());
        AlgaeParticleData[] algaeParticlesArray = new AlgaeParticleData[numAlgaeParticles];

        float minParticleSize = settingsRef.avgAlgaeParticleRadius / settingsRef.algaeParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgAlgaeParticleRadius * settingsRef.algaeParticleRadiusVariance;

        for(int i = 0; i < algaeParticlesCBuffer.count; i++) {
            AlgaeParticleData data = new AlgaeParticleData();
            data.index = i;            
            data.worldPos = new Vector2(UnityEngine.Random.Range(0f, SimulationManager._MapSize), UnityEngine.Random.Range(0f, SimulationManager._MapSize));

            data.radius = UnityEngine.Random.Range(minParticleSize, maxParticleSize);
            data.biomass = 0f; // data.radius * data.radius * Mathf.PI * settingsRef.algaeParticleNutrientDensity;
            data.isActive = 0f;
            data.isDecaying = 0f;
            data.age = UnityEngine.Random.Range(1f, 2f);
            data.color = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            algaeParticlesArray[i] = data;
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        algaeParticlesCBuffer.SetData(algaeParticlesArray);
        algaeParticlesCBufferSwap.SetData(algaeParticlesArray);
        //Debug.Log("Set Data GPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        algaeParticlesNearestCritters1024 = new RenderTexture(numAlgaeParticles, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        algaeParticlesNearestCritters1024.wrapMode = TextureWrapMode.Clamp;
        algaeParticlesNearestCritters1024.filterMode = FilterMode.Point;
        algaeParticlesNearestCritters1024.enableRandomWrite = true;        
        algaeParticlesNearestCritters1024.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    
        //Debug.Log("Create RT 1024: " + (Time.realtimeSinceStartup - startTime).ToString());
        algaeParticlesNearestCritters32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        algaeParticlesNearestCritters32.wrapMode = TextureWrapMode.Clamp;
        algaeParticlesNearestCritters32.filterMode = FilterMode.Point;
        algaeParticlesNearestCritters32.enableRandomWrite = true;        
        algaeParticlesNearestCritters32.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***   
        //Debug.Log("Create RT 32: " + (Time.realtimeSinceStartup - startTime).ToString());
        algaeParticlesNearestCritters1 = new RenderTexture(1, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        algaeParticlesNearestCritters1.wrapMode = TextureWrapMode.Clamp;
        algaeParticlesNearestCritters1.filterMode = FilterMode.Point;
        algaeParticlesNearestCritters1.enableRandomWrite = true;        
        algaeParticlesNearestCritters1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        closestAlgaeParticlesDataArray = new AlgaeParticleData[numAgents];
        closestAlgaeParticlesDataCBuffer = new ComputeBuffer(numAgents, GetAlgaeParticleDataSize());

        algaeParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        algaeParticlesEatAmountsArray = new float[numAgents];

        algaeParticleMeasurementTotalsData = new AlgaeParticleData[1];
        algaeParticlesMeasure32 = new ComputeBuffer(32, GetAlgaeParticleDataSize());
        algaeParticlesMeasure1 = new ComputeBuffer(1, GetAlgaeParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
    }
        
    public void InitializeAlgaeGrid(int numAgents, ComputeShader computeShader) {

        computeShaderResourceGrid = computeShader;
              
        resourceGridRT1 = new RenderTexture(resourceGridTexResolution, resourceGridTexResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        resourceGridRT1.wrapMode = TextureWrapMode.Clamp;
        resourceGridRT1.filterMode = FilterMode.Bilinear;
        resourceGridRT1.enableRandomWrite = true;
        //nutrientMapRT1.useMipMap = true;
        resourceGridRT1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    

        resourceGridRT2 = new RenderTexture(resourceGridTexResolution, resourceGridTexResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        resourceGridRT2.wrapMode = TextureWrapMode.Clamp;
        resourceGridRT2.enableRandomWrite = true;
        //nutrientMapRT2.useMipMap = true;
        resourceGridRT2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***  
        
        resourceGridSamplesArray = new Vector4[numAgents];
        resourceGridEatAmountsArray = new Vector4[numAgents];

        int kernelCSInitializeAlgaeGrid = computeShaderResourceGrid.FindKernel("CSInitializeAlgaeGrid");
        computeShaderResourceGrid.SetTexture(kernelCSInitializeAlgaeGrid, "algaeGridWrite", resourceGridRT1);
        computeShaderResourceGrid.Dispatch(kernelCSInitializeAlgaeGrid, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);
        Graphics.Blit(resourceGridRT1, resourceGridRT2);

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

        resourceGridAgentSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);

        
        rdRT1 = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rdRT1.wrapMode = TextureWrapMode.Repeat;
        rdRT1.filterMode = FilterMode.Bilinear;
        rdRT1.enableRandomWrite = true;
        rdRT1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        rdRT2 = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rdRT2.wrapMode = TextureWrapMode.Repeat;
        rdRT2.filterMode = FilterMode.Bilinear;
        rdRT2.enableRandomWrite = true;
        rdRT2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        //theRenderKing.fluidRenderMat.SetTexture("_DebugTex", nutrientMapRT1);
        
    }

    public void InitializeReactionDiffusionGrid() {
        int kernelCSInitRD = computeShaderResourceGrid.FindKernel("CSInitRD"); 
        //computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "rdRead", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSInitRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.Dispatch(kernelCSInitRD, 128 / 32, 128 / 32, 1);

    }

    public void SimReactionDiffusionGrid(ref EnvironmentFluidManager fluidManagerRef) {
        int kernelCSInitRD = computeShaderResourceGrid.FindKernel("CSSimRD"); 
        computeShaderResourceGrid.SetTexture(kernelCSInitRD, "rdRead", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSInitRD, "rdWrite", rdRT2);
        computeShaderResourceGrid.Dispatch(kernelCSInitRD, 128 / 32, 128 / 32, 1);
        // write into 2
        int kernelCSAdvectRD = computeShaderResourceGrid.FindKernel("CSAdvectRD");
        computeShaderResourceGrid.SetFloat("_TextureResolution", 128f);
        //computeShaderResourceGrid.SetFloat("_TextureResolution", (float)resolution);
        computeShaderResourceGrid.SetFloat("_DeltaTime", fluidManagerRef.deltaTime);
        computeShaderResourceGrid.SetFloat("_InvGridScale", fluidManagerRef.invGridScale);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "rdRead", rdRT2);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.Dispatch(kernelCSAdvectRD, 128 / 32, 128 / 32, 1);
        //back into 1
    }

    public void ApplyDiffusionOnResourceGrid(EnvironmentFluidManager fluidManagerRef) {
        int kernelCSUpdateAlgaeGrid = computeShaderResourceGrid.FindKernel("CSUpdateAlgaeGrid");
        computeShaderResourceGrid.SetFloat("_AlgaeGridDiffusion", settingsRef.nutrientDiffusionRate);
        computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "ObstaclesRead", fluidManagerRef._ObstaclesRT);
        computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "algaeGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "algaeGridWrite", resourceGridRT2);
        computeShaderResourceGrid.Dispatch(kernelCSUpdateAlgaeGrid, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);

        //Graphics.Blit(resourceGridRT2, resourceGridRT1);
        
    }
    public void AdvectResourceGrid(EnvironmentFluidManager fluidManagerRef) {
        int kernelAdvectResourceGrid = computeShaderResourceGrid.FindKernel("CSAdvectResourceGrid");
        computeShaderResourceGrid.SetBuffer(kernelAdvectResourceGrid, "algaeParticlesRead", algaeParticlesCBuffer);
        computeShaderResourceGrid.SetFloat("_TextureResolution", 32f); // (float)resolution);
        computeShaderResourceGrid.SetFloat("_DeltaTime", fluidManagerRef.deltaTime);
        computeShaderResourceGrid.SetFloat("_InvGridScale", fluidManagerRef.invGridScale);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "ObstaclesRead", fluidManagerRef._ObstaclesRT);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "algaeGridRead", resourceGridRT2);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "algaeGridWrite", resourceGridRT1);
        computeShaderResourceGrid.Dispatch(kernelAdvectResourceGrid, 1, 1, 1);
    }
    public void GetResourceGridValuesAtMouthPositions(SimulationStateData simStateDataRef) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        //ComputeBuffer nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        
        int kernelCSGetAlgaeGridSamples = computeShaderResourceGrid.FindKernel("CSGetAlgaeGridSamples");        
        computeShaderResourceGrid.SetBuffer(kernelCSGetAlgaeGridSamples, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderResourceGrid.SetBuffer(kernelCSGetAlgaeGridSamples, "algaeGridSamplesCBuffer", resourceGridAgentSamplesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSGetAlgaeGridSamples, "algaeGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.Dispatch(kernelCSGetAlgaeGridSamples, resourceGridAgentSamplesCBuffer.count, 1, 1);

        //Vector4[] outArray = new Vector4[_NumAgents];
        resourceGridAgentSamplesCBuffer.GetData(resourceGridSamplesArray); // Disappearing body strokes due to this !?!?!?!?!?

        //Debug.Log("Food: " + nutrientSamplesArray[0].x.ToString());
        //nutrientSamplesCBuffer.Release();
        
        // Read out sample values::::
    }
    /*public float MeasureTotalAlgaeGridAmount() {

        ComputeBuffer outputValuesCBuffer = new ComputeBuffer(1, sizeof(float) * 4);  // holds the result of measurement: total sum of pix colors in texture
        Vector4[] outputValuesArray = new Vector4[1];

        // 32 --> 16:
        int kernelCSMeasureTotalAlgae = computeShaderAlgaeGrid.FindKernel("CSMeasureTotalAlgae");   
        computeShaderAlgaeGrid.SetBuffer(kernelCSMeasureTotalAlgae, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "measureValuesTex", algaeGridRT1);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "pooledResultTex", tempTex16);
        computeShaderAlgaeGrid.Dispatch(kernelCSMeasureTotalAlgae, 16, 16, 1);
        // 16 --> 8:
        computeShaderAlgaeGrid.SetBuffer(kernelCSMeasureTotalAlgae, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "measureValuesTex", tempTex16);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "pooledResultTex", tempTex8);
        computeShaderAlgaeGrid.Dispatch(kernelCSMeasureTotalAlgae, 8, 8, 1);
        // 8 --> 4:
        computeShaderAlgaeGrid.SetBuffer(kernelCSMeasureTotalAlgae, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "measureValuesTex", tempTex8);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "pooledResultTex", tempTex4);
        computeShaderAlgaeGrid.Dispatch(kernelCSMeasureTotalAlgae, 4, 4, 1);        
        // 4 --> 2:
        computeShaderAlgaeGrid.SetBuffer(kernelCSMeasureTotalAlgae, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "measureValuesTex", tempTex4);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "pooledResultTex", tempTex2);
        computeShaderAlgaeGrid.Dispatch(kernelCSMeasureTotalAlgae, 2, 2, 1);
        // 2 --> 1:
        computeShaderAlgaeGrid.SetBuffer(kernelCSMeasureTotalAlgae, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "measureValuesTex", tempTex2);
        computeShaderAlgaeGrid.SetTexture(kernelCSMeasureTotalAlgae, "pooledResultTex", tempTex1);
        computeShaderAlgaeGrid.Dispatch(kernelCSMeasureTotalAlgae, 1, 1, 1);
        
        outputValuesCBuffer.GetData(outputValuesArray);

        curGlobalAlgaeGrid = outputValuesArray[0].x;

        outputValuesCBuffer.Release();

        //Debug.Log("TotalNutrients: " + outputValuesArray[0].x.ToString() + ", " + outputValuesArray[0].y.ToString());

        return outputValuesArray[0].x;
    }*/    
    public void AddResourcesAtCoords(Vector4 amount, float x, float y) {  // 0-1 normalized map coords
        
        ComputeBuffer addAlgaeCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        Vector4[] addAlgaeArray = new Vector4[1];

        addAlgaeArray[0] = amount;
        addAlgaeCBuffer.SetData(addAlgaeArray);

        int kernelCSAddAlgaeAtCoords = computeShaderResourceGrid.FindKernel("CSAddResourcesAtCoords");
        computeShaderResourceGrid.SetBuffer(kernelCSAddAlgaeAtCoords, "addAlgaeCBuffer", addAlgaeCBuffer);        
        computeShaderResourceGrid.SetTexture(kernelCSAddAlgaeAtCoords, "algaeGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAddAlgaeAtCoords, "algaeGridWrite", resourceGridRT2);
        computeShaderResourceGrid.SetFloat("_CoordX", x);
        computeShaderResourceGrid.SetFloat("_CoordY", y);
        computeShaderResourceGrid.Dispatch(kernelCSAddAlgaeAtCoords, 1, 1, 1);  // one-at-a-time for now, until re-factor (separate location buffers to resourceAmounts) ****
        
        Graphics.Blit(resourceGridRT2, resourceGridRT1);

        addAlgaeCBuffer.Release();
        
    }
    public void RemoveEatenAlgaeGrid(int numAgents, SimulationStateData simStateDataRef) {
        ComputeBuffer eatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
                
        eatAmountsCBuffer.SetData(resourceGridEatAmountsArray);

        int kernelCSRemoveAlgaeAtLocations = computeShaderResourceGrid.FindKernel("CSRemoveAlgaeAtLocations");
        computeShaderResourceGrid.SetBuffer(kernelCSRemoveAlgaeAtLocations, "nutrientEatAmountsCBuffer", eatAmountsCBuffer);
        computeShaderResourceGrid.SetBuffer(kernelCSRemoveAlgaeAtLocations, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSRemoveAlgaeAtLocations, "algaeGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSRemoveAlgaeAtLocations, "algaeGridWrite", resourceGridRT2);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.Dispatch(kernelCSRemoveAlgaeAtLocations, eatAmountsCBuffer.count, 1, 1);

        Graphics.Blit(resourceGridRT2, resourceGridRT1);
        
        eatAmountsCBuffer.Release();
    }


    /*public void ReviveSelectFoodParticles(int[] indicesArray, float radius, Vector4 spawnCoords, SimulationStateData simStateDataRef) {

        ComputeBuffer selectRespawnFoodParticleIndicesCBuffer = new ComputeBuffer(indicesArray.Length, sizeof(int));
        selectRespawnFoodParticleIndicesCBuffer.SetData(indicesArray);
        
        int kernelCSReviveSelectFoodParticles = computeShaderAlgaeParticles.FindKernel("CSReviveSelectFoodParticles");
        computeShaderAlgaeParticles.SetBuffer(kernelCSReviveSelectFoodParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSReviveSelectFoodParticles, "selectRespawnFoodParticleIndicesCBuffer", selectRespawnFoodParticleIndicesCBuffer);
        
        computeShaderAlgaeParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderAlgaeParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderAlgaeParticles.SetVector("_FoodSprinklePos", spawnCoords);
        computeShaderAlgaeParticles.SetFloat("_FoodSprinkleRadius", radius);
        computeShaderAlgaeParticles.SetBuffer(kernelCSReviveSelectFoodParticles, "foodParticlesWrite", algaeParticlesCBufferSwap);
        computeShaderAlgaeParticles.Dispatch(kernelCSReviveSelectFoodParticles, indicesArray.Length, 1, 1);

        selectRespawnFoodParticleIndicesCBuffer.Release();
    }*/  // NOT USED ANY MORE????
    public void SpawnInitialAlgaeParticles(float radius, Vector4 spawnCoords) {
        int kernelCSSpawnInitialAlgaeParticles = computeShaderAlgaeParticles.FindKernel("CSSpawnInitialAlgaeParticles");        
        computeShaderAlgaeParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderAlgaeParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderAlgaeParticles.SetVector("_FoodSprinklePos", spawnCoords);
        computeShaderAlgaeParticles.SetFloat("_FoodSprinkleRadius", radius);
        computeShaderAlgaeParticles.SetBuffer(kernelCSSpawnInitialAlgaeParticles, "foodParticlesWrite", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.Dispatch(kernelCSSpawnInitialAlgaeParticles, 1, 1, 1);
    }
    public void SimulateAlgaeParticles(EnvironmentFluidManager fluidManagerRef, TheRenderKing renderKingRef, SimulationStateData simStateDataRef, SimResourceManager resourcesManager) { // Sim
        // Go through foodParticleData and check for inactive
        // determined by current total food -- done!
        // if flag on shader for Respawn is on, set to active and initialize
        float maxFoodParticleTotal = settingsRef.maxFoodParticleTotalAmount;

        

        int kernelCSSimulateAlgaeParticles = computeShaderAlgaeParticles.FindKernel("CSSimulateAlgaeParticles");
        computeShaderAlgaeParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "foodParticlesWrite", algaeParticlesCBufferSwap);
        computeShaderAlgaeParticles.SetTexture(kernelCSSimulateAlgaeParticles, "velocityRead", fluidManagerRef._VelocityA);        
        computeShaderAlgaeParticles.SetTexture(kernelCSSimulateAlgaeParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightDataRT);
        computeShaderAlgaeParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_SpawnDensityMap", resourceGridRT1);
        computeShaderAlgaeParticles.SetFloat("_MapSize", SimulationManager._MapSize);            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderAlgaeParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        float randRoll = UnityEngine.Random.Range(0f, 1f);
        computeShaderAlgaeParticles.SetFloat("_RespawnFoodParticles", randRoll); 

        //_FoodSprinklePos;
//_FoodSprinkleRadius;
        float spawnLerp = renderKingRef.simManager.trophicLayersManager.GetAlgaeOnLerp(renderKingRef.simManager.simAgeTimeSteps);
        float spawnRadius = Mathf.Lerp(1f, SimulationManager._MapSize, spawnLerp);
        Vector4 spawnPos = new Vector4(renderKingRef.simManager.trophicLayersManager.algaeOriginPos.x, renderKingRef.simManager.trophicLayersManager.algaeOriginPos.y, 0f, 0f);
        computeShaderAlgaeParticles.SetFloat("_FoodSprinkleRadius", spawnRadius);
        computeShaderAlgaeParticles.SetVector("_FoodSprinklePos", spawnPos);
        
        /*if(algaeParticleMeasurementTotalsData[0].biomass < maxFoodParticleTotal) {
            computeShaderAlgaeParticles.SetFloat("_RespawnFoodParticles", 1f);                       
        }
        else {
            computeShaderAlgaeParticles.SetFloat("_RespawnFoodParticles", 0f);      
        }*/

        float minParticleSize = settingsRef.avgAlgaeParticleRadius / settingsRef.algaeParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgAlgaeParticleRadius * settingsRef.algaeParticleRadiusVariance;

        computeShaderAlgaeParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderAlgaeParticles.SetFloat("_MaxParticleSize", maxParticleSize);      
        computeShaderAlgaeParticles.SetFloat("_ParticleNutrientDensity", settingsRef.algaeParticleNutrientDensity);
        computeShaderAlgaeParticles.SetFloat("_FoodParticleRegrowthRate", settingsRef.foodParticleRegrowthRate);

        computeShaderAlgaeParticles.SetFloat("_GlobalNutrients", resourcesManager.curGlobalNutrients);
        computeShaderAlgaeParticles.SetFloat("_SolarEnergy", settingsRef.environmentSettings._BaseSolarEnergy);
        computeShaderAlgaeParticles.SetFloat("_AlgaeGrowthNutrientsMask", settingsRef.algaeSettings._AlgaeGrowthNutrientsMask);
        computeShaderAlgaeParticles.SetFloat("_AlgaeBaseGrowthRate", settingsRef.algaeSettings._AlgaeBaseGrowthRate * (2.0f - spawnLerp)); // * (1f + 3 * (1.0f - (float)renderKingRef.simManager.uiManager.recentlyCreatedSpeciesTimeStepCounter / 360f)));

        computeShaderAlgaeParticles.SetFloat("_AlgaeGrowthNutrientUsage", settingsRef.algaeSettings._AlgaeGrowthNutrientUsage);
        computeShaderAlgaeParticles.SetFloat("_AlgaeGrowthOxygenProduction", settingsRef.algaeSettings._AlgaeGrowthOxygenProduction);
        computeShaderAlgaeParticles.SetFloat("_AlgaeAgingRate", settingsRef.algaeSettings._AlgaeAgingRate);
        computeShaderAlgaeParticles.SetFloat("_AlgaeDecayRate", settingsRef.algaeSettings._AlgaeDecayRate);
        computeShaderAlgaeParticles.SetFloat("_AlgaeSpawnMaxAltitude", settingsRef.algaeSettings._AlgaeSpawnMaxAltitude);
        computeShaderAlgaeParticles.SetFloat("_AlgaeParticleInitMass", settingsRef.algaeSettings._AlgaeParticleInitMass);

        computeShaderAlgaeParticles.Dispatch(kernelCSSimulateAlgaeParticles, 1, 1, 1);                

        // Copy/Swap Food Particle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderAlgaeParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderAlgaeParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", algaeParticlesCBufferSwap);
        computeShaderAlgaeParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", algaeParticlesCBuffer);        
        computeShaderAlgaeParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);        
        
    }
    public void EatSelectedFoodParticles(SimulationStateData simStateDataRef) {  // removes gpu particle & sends consumption data back to CPU
        // Use CritterSimData to determine critter mouth locations

        // run through all foodParticles, check against each critter position, then measure min value with recursive reduction:

        // Need to update CritterSim&InitData structs to have more mouth/bite info

        // Record how much food successfully eaten per Critter

        int kernelCSEatSelectedFoodParticles = computeShaderAlgaeParticles.FindKernel("CSEatSelectedFoodParticles");
        computeShaderAlgaeParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesWrite", algaeParticlesCBufferSwap);
        computeShaderAlgaeParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesEatAmountsCBuffer", algaeParticlesEatAmountsCBuffer);        
        computeShaderAlgaeParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "closestParticlesDataCBuffer", closestAlgaeParticlesDataCBuffer);  
        computeShaderAlgaeParticles.SetTexture(kernelCSEatSelectedFoodParticles, "critterDistancesRead", algaeParticlesNearestCritters1);
        computeShaderAlgaeParticles.Dispatch(kernelCSEatSelectedFoodParticles, simStateDataRef.critterSimDataCBuffer.count, 1, 1);

        algaeParticlesEatAmountsCBuffer.GetData(algaeParticlesEatAmountsArray);

        float totalFoodEaten = 0f;
        for(int i = 0; i < algaeParticlesEatAmountsCBuffer.count; i++) {
            totalFoodEaten += algaeParticlesEatAmountsArray[i];
        }
        // Copy/Swap Food PArticle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderAlgaeParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderAlgaeParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", algaeParticlesCBufferSwap);
        computeShaderAlgaeParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
    }
    public void FindClosestAlgaeParticleToCritters(SimulationStateData simStateDataRef) {  // need to send info on closest particle pos/dir/amt back to CPU also
        
        // Populate main RenderTexture with distances for each foodParticle to each Critter:

        int kernelCSMeasureInitCritterDistances = computeShaderAlgaeParticles.FindKernel("CSMeasureInitCritterDistances");
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "foodParticlesRead", algaeParticlesCBuffer);        
        computeShaderAlgaeParticles.SetTexture(kernelCSMeasureInitCritterDistances, "foodParticlesNearestCrittersRT", algaeParticlesNearestCritters1024);        
        computeShaderAlgaeParticles.Dispatch(kernelCSMeasureInitCritterDistances, algaeParticlesCBuffer.count / 1024, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 1024 --> 32 particles per critter:
        int kernelCSReduceCritterDistances32 = computeShaderAlgaeParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderAlgaeParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", algaeParticlesNearestCritters1024);
        computeShaderAlgaeParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", algaeParticlesNearestCritters32);
        computeShaderAlgaeParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", algaeParticlesCBuffer);        
        computeShaderAlgaeParticles.Dispatch(kernelCSReduceCritterDistances32, 32, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 32 --> 1 particles per critter:
        computeShaderAlgaeParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", algaeParticlesNearestCritters32);
        computeShaderAlgaeParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", algaeParticlesNearestCritters1);
        computeShaderAlgaeParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestAlgaeParticlesDataCBuffer);
        computeShaderAlgaeParticles.Dispatch(kernelCSReduceCritterDistances32, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        // Copy/Swap Food PArticle Buffer:
        //int kernelCSCopyFoodParticlesBuffer = computeShaderFoodParticles.FindKernel("CSCopyFoodParticlesBuffer");
        //computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", foodParticlesCBufferSwap);
        //computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", foodParticlesCBuffer);
        //computeShaderFoodParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);

        closestAlgaeParticlesDataCBuffer.GetData(closestAlgaeParticlesDataArray);

        //Debug.Log("ClosestFoodParticle: " + closestFoodParticlesDataArray[0].index.ToString() + ", " + closestFoodParticlesDataArray[0].worldPos.ToString() + ", amt: " + closestFoodParticlesDataArray[0].foodAmount.ToString());
    }
    public void MeasureTotalAlgaeParticlesAmount() {
        
        int kernelCSMeasureTotalFoodParticlesAmount = computeShaderAlgaeParticles.FindKernel("CSMeasureTotalFoodParticlesAmount");
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", algaeParticlesMeasure32);
         
        // DISPATCH !!!
        computeShaderAlgaeParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 32, 1, 1);
        
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", algaeParticlesMeasure32);
        computeShaderAlgaeParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", algaeParticlesMeasure1);
        computeShaderAlgaeParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 1, 1, 1);
        
        algaeParticlesMeasure1.GetData(algaeParticleMeasurementTotalsData);
        resourceManagerRef.curGlobalAlgaeParticles = algaeParticleMeasurementTotalsData[0].biomass;
        resourceManagerRef.oxygenProducedByAlgaeParticlesLastFrame = algaeParticleMeasurementTotalsData[0].oxygenProduced;
        resourceManagerRef.wasteProducedByAlgaeParticlesLastFrame = algaeParticleMeasurementTotalsData[0].wasteProduced;
        resourceManagerRef.nutrientsUsedByAlgaeParticlesLastFrame = algaeParticleMeasurementTotalsData[0].nutrientsUsed;

        /*animalParticlesMeasure1.GetData(animalParticleMeasurementTotalsData);
        curGlobalAnimalParticles = animalParticleMeasurementTotalsData[0].biomass;
        oxygenUsedByAnimalParticlesLastFrame = animalParticleMeasurementTotalsData[0].oxygenUsed;
        wasteProducedByAnimalParticlesLastFrame = animalParticleMeasurementTotalsData[0].wasteProduced;
        algaeConsumedByAnimalParticlesLastFrame = animalParticleMeasurementTotalsData[0].algaeConsumed;
        */
        
    }
    
    public void ClearBuffers() {

        if (tempTex1 != null) {
            tempTex1.Release();
            tempTex2.Release();
            tempTex4.Release();
            tempTex8.Release();
            tempTex16.Release();
        }
        if(resourceGridAgentSamplesCBuffer != null) {
            resourceGridAgentSamplesCBuffer.Release();
        }

        if (algaeParticlesNearestCritters1 != null) {
            algaeParticlesNearestCritters1.Release();
            algaeParticlesNearestCritters32.Release();
            algaeParticlesNearestCritters1024.Release();
        }        
        if(algaeParticlesCBuffer != null) {
            algaeParticlesCBuffer.Release();
        }  
        if(algaeParticlesCBufferSwap != null) {
            algaeParticlesCBufferSwap.Release();
        } 
        if(closestAlgaeParticlesDataCBuffer != null) {
            closestAlgaeParticlesDataCBuffer.Release();
        }
        if(algaeParticlesEatAmountsCBuffer != null) {
            algaeParticlesEatAmountsCBuffer.Release();
        }
        if(algaeParticlesMeasure32 != null) {
            algaeParticlesMeasure32.Release();
        }
        if(algaeParticlesMeasure1 != null) {
            algaeParticlesMeasure1.Release();
        }        
    }
}
