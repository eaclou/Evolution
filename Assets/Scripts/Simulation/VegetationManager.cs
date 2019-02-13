using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
///  Move Food-related stuff from SimulationManager into here to de-clutter simManager:
/// </summary>

public class VegetationManager {

    public SettingsManager settingsRef;
    public SimResourceManager resourceManagerRef;
    
    private ComputeShader computeShaderAlgaeGrid;
    private ComputeShader computeShaderAlgaeParticles;
    private ComputeShader computeShaderAnimalParticles;

    //public float curGlobalAlgaeGrid = 0f;  // not using this currently
    
    public int algaeGridTexResolution = 32; // Temporarily disabled - replaced by single value (1x1 grid)
    public RenderTexture algaeGridRT1;
    public RenderTexture algaeGridRT2;
    public Vector4[] algaeGridSamplesArray;
    public Vector4[] algaeGridEatAmountsArray;

    private RenderTexture tempTex16;
    private RenderTexture tempTex8;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex4;
    private RenderTexture tempTex2;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex1;
    
    private ComputeBuffer algaeGridSamplesCBuffer;

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

    private const int numAnimalParticles = 1024;  // *** 
    public ComputeBuffer animalParticlesCBuffer;
    private ComputeBuffer animalParticlesCBufferSwap;    
    private RenderTexture animalParticlesNearestCritters1024;
    private RenderTexture animalParticlesNearestCritters32;
    private RenderTexture animalParticlesNearestCritters1;
    private ComputeBuffer closestAnimalParticlesDataCBuffer;
    public AnimalParticleData[] closestAnimalParticlesDataArray;
    private ComputeBuffer animalParticlesEatAmountsCBuffer;
    public float[] animalParticlesEatAmountsArray;
    private ComputeBuffer animalParticlesMeasure32;
    private ComputeBuffer animalParticlesMeasure1;
    private AnimalParticleData[] animalParticleMeasurementTotalsData;


    public Vector2[] nutrientSpawnPatchesArray;

    
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
    }

    public struct AnimalParticleData {
        public int index;
	    public int critterIndex; // index of creature which swallowed this foodParticle
	    public int nearestCritterIndex;
        public float isSwallowed;   // 0 = normal, 1 = in critter's belly
        public float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk        
        public Vector3 worldPos;
        public Vector2 p1;  // spline points:
	    public Vector2 p2;
	    public Vector2 p3;
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
    }
    
    private int GetAlgaeParticleDataSize() {
        int bitSize = sizeof(float) * 12 + sizeof(int) * 3;
        return bitSize;
    }

    private int GetAnimalParticleDataSize() {
        int bitSize = sizeof(float) * 22 + sizeof(int) * 3;
        return bitSize;
    }

    public void MoveRandomNutrientPatches(int index) {
        nutrientSpawnPatchesArray[index] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
        Debug.Log("Moved Nutrient Patch! [" + index.ToString() + "], " + nutrientSpawnPatchesArray[index].ToString());
    }
	
    public VegetationManager(SettingsManager settings, SimResourceManager resourcesRef) {
        settingsRef = settings;
        resourceManagerRef = resourcesRef;

        nutrientSpawnPatchesArray = new Vector2[4]; // *** Refactor this!!! ***
        for(int i = 0; i < nutrientSpawnPatchesArray.Length; i++) {
            nutrientSpawnPatchesArray[i] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
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
            data.worldPos = new Vector3(UnityEngine.Random.Range(0f, SimulationManager._MapSize), UnityEngine.Random.Range(0f, SimulationManager._MapSize), 0f);

            data.radius = UnityEngine.Random.Range(minParticleSize, maxParticleSize); // obsolete!
            data.biomass = 0.01f; // data.radius * data.radius * Mathf.PI; // * settingsRef.animalParticleNutrientDensity;
            data.isActive = 1f;
            data.isDecaying = 0f;
            data.age = UnityEngine.Random.Range(1f, 2f);
            animalParticlesArray[i] = data;
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        animalParticlesCBuffer.SetData(animalParticlesArray);
        animalParticlesCBufferSwap.SetData(animalParticlesArray);
        //Debug.Log("Set Data GPU: " + (Time.realtimeSinceStartup - startTime).ToString());

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
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        closestAnimalParticlesDataArray = new AnimalParticleData[numAgents];
        closestAnimalParticlesDataCBuffer = new ComputeBuffer(numAgents, GetAnimalParticleDataSize());

        animalParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        animalParticlesEatAmountsArray = new float[numAgents];

        animalParticleMeasurementTotalsData = new AnimalParticleData[1];
        animalParticlesMeasure32 = new ComputeBuffer(32, GetAnimalParticleDataSize());
        animalParticlesMeasure1 = new ComputeBuffer(1, GetAnimalParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
    }

    public void InitializeAlgaeGrid(int numAgents, ComputeShader computeShader) {

        computeShaderAlgaeGrid = computeShader;
              
        algaeGridRT1 = new RenderTexture(algaeGridTexResolution, algaeGridTexResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        algaeGridRT1.wrapMode = TextureWrapMode.Clamp;
        algaeGridRT1.filterMode = FilterMode.Bilinear;
        algaeGridRT1.enableRandomWrite = true;
        //nutrientMapRT1.useMipMap = true;
        algaeGridRT1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    

        algaeGridRT2 = new RenderTexture(algaeGridTexResolution, algaeGridTexResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        algaeGridRT2.wrapMode = TextureWrapMode.Clamp;
        algaeGridRT2.enableRandomWrite = true;
        //nutrientMapRT2.useMipMap = true;
        algaeGridRT2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***  
        
        algaeGridSamplesArray = new Vector4[numAgents];
        algaeGridEatAmountsArray = new Vector4[numAgents];

        int kernelCSInitializeAlgaeGrid = computeShaderAlgaeGrid.FindKernel("CSInitializeAlgaeGrid");
        computeShaderAlgaeGrid.SetTexture(kernelCSInitializeAlgaeGrid, "algaeGridWrite", algaeGridRT1);
        computeShaderAlgaeGrid.Dispatch(kernelCSInitializeAlgaeGrid, algaeGridTexResolution / 32, algaeGridTexResolution / 32, 1);
        Graphics.Blit(algaeGridRT1, algaeGridRT2);

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

        algaeGridSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);

        //theRenderKing.fluidRenderMat.SetTexture("_DebugTex", nutrientMapRT1);
        
    }

    public void ApplyDiffusionOnAlgaeGrid(EnvironmentFluidManager fluidManagerRef) {
        int kernelCSUpdateAlgaeGrid = computeShaderAlgaeGrid.FindKernel("CSUpdateAlgaeGrid");
        computeShaderAlgaeGrid.SetFloat("_AlgaeGridDiffusion", settingsRef.foodDiffusionRate);
        computeShaderAlgaeGrid.SetTexture(kernelCSUpdateAlgaeGrid, "ObstaclesRead", fluidManagerRef._ObstaclesRT);
        computeShaderAlgaeGrid.SetTexture(kernelCSUpdateAlgaeGrid, "algaeGridRead", algaeGridRT1);
        computeShaderAlgaeGrid.SetTexture(kernelCSUpdateAlgaeGrid, "algaeGridWrite", algaeGridRT2);
        computeShaderAlgaeGrid.Dispatch(kernelCSUpdateAlgaeGrid, algaeGridTexResolution / 32, algaeGridTexResolution / 32, 1);

        Graphics.Blit(algaeGridRT2, algaeGridRT1);
        
    }
    public void GetAlgaeGridValuesAtMouthPositions(SimulationStateData simStateDataRef) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        //ComputeBuffer nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        
        int kernelCSGetAlgaeGridSamples = computeShaderAlgaeGrid.FindKernel("CSGetAlgaeGridSamples");        
        computeShaderAlgaeGrid.SetBuffer(kernelCSGetAlgaeGridSamples, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAlgaeGrid.SetBuffer(kernelCSGetAlgaeGridSamples, "algaeGridSamplesCBuffer", algaeGridSamplesCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSGetAlgaeGridSamples, "algaeGridRead", algaeGridRT1);
        computeShaderAlgaeGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderAlgaeGrid.Dispatch(kernelCSGetAlgaeGridSamples, algaeGridSamplesCBuffer.count, 1, 1);

        //Vector4[] outArray = new Vector4[_NumAgents];
        algaeGridSamplesCBuffer.GetData(algaeGridSamplesArray); // Disappearing body strokes due to this !?!?!?!?!?

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
    /*public void AddAlgaeAtCoords(float amount, int x, int y) {
        if(curGlobalAlgaeGrid < 128f) {
            ComputeBuffer addAlgaeCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
            Vector4[] addAlgaeArray = new Vector4[1];
            addAlgaeArray[0] = new Vector4(amount, (float)x / 32f, (float)y / 32f, 1f);
            addAlgaeCBuffer.SetData(addAlgaeArray);

            int kernelCSAddAlgaeAtCoords = computeShaderAlgaeGrid.FindKernel("CSAddAlgaeAtCoords");
            computeShaderAlgaeGrid.SetBuffer(kernelCSAddAlgaeAtCoords, "addAlgaeCBuffer", addAlgaeCBuffer);        
            computeShaderAlgaeGrid.SetTexture(kernelCSAddAlgaeAtCoords, "algaeGridRead", algaeGridRT1);
            computeShaderAlgaeGrid.SetTexture(kernelCSAddAlgaeAtCoords, "algaeGridWrite", algaeGridRT2);
            computeShaderAlgaeGrid.Dispatch(kernelCSAddAlgaeAtCoords, addAlgaeCBuffer.count, 1, 1);
        
            Graphics.Blit(algaeGridRT2, algaeGridRT1);

            addAlgaeCBuffer.Release();
        }
        else {
            Debug.Log("Can't add nutrients, exceeds max level");
        }

        
    }*/
    public void RemoveEatenAlgaeGrid(int numAgents, SimulationStateData simStateDataRef) {
        ComputeBuffer eatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
                
        eatAmountsCBuffer.SetData(algaeGridEatAmountsArray);

        int kernelCSRemoveAlgaeAtLocations = computeShaderAlgaeGrid.FindKernel("CSRemoveAlgaeAtLocations");
        computeShaderAlgaeGrid.SetBuffer(kernelCSRemoveAlgaeAtLocations, "nutrientEatAmountsCBuffer", eatAmountsCBuffer);
        computeShaderAlgaeGrid.SetBuffer(kernelCSRemoveAlgaeAtLocations, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderAlgaeGrid.SetTexture(kernelCSRemoveAlgaeAtLocations, "algaeGridRead", algaeGridRT1);
        computeShaderAlgaeGrid.SetTexture(kernelCSRemoveAlgaeAtLocations, "algaeGridWrite", algaeGridRT2);
        computeShaderAlgaeGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderAlgaeGrid.Dispatch(kernelCSRemoveAlgaeAtLocations, eatAmountsCBuffer.count, 1, 1);

        Graphics.Blit(algaeGridRT2, algaeGridRT1);
        
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
        computeShaderAlgaeParticles.SetTexture(kernelCSSimulateAlgaeParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightMap);
        computeShaderAlgaeParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_SpawnDensityMap", algaeGridRT1);
        computeShaderAlgaeParticles.SetFloat("_MapSize", SimulationManager._MapSize);
            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderAlgaeParticles.SetFloat("_Time", Time.realtimeSinceStartup);

        if(algaeParticleMeasurementTotalsData[0].biomass < maxFoodParticleTotal) {
            computeShaderAlgaeParticles.SetFloat("_RespawnFoodParticles", 1f);                       
        }
        else {
            computeShaderAlgaeParticles.SetFloat("_RespawnFoodParticles", 0f);      
        }

        float minParticleSize = settingsRef.avgAlgaeParticleRadius / settingsRef.algaeParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgAlgaeParticleRadius * settingsRef.algaeParticleRadiusVariance;

        computeShaderAlgaeParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderAlgaeParticles.SetFloat("_MaxParticleSize", maxParticleSize);      
        computeShaderAlgaeParticles.SetFloat("_ParticleNutrientDensity", settingsRef.algaeParticleNutrientDensity);
        computeShaderAlgaeParticles.SetFloat("_FoodParticleRegrowthRate", settingsRef.foodParticleRegrowthRate);

        computeShaderAlgaeParticles.SetFloat("_GlobalNutrients", resourcesManager.curGlobalNutrients);
        computeShaderAlgaeParticles.SetFloat("_SolarEnergy", settingsRef.environmentSettings._BaseSolarEnergy);
        computeShaderAlgaeParticles.SetFloat("_AlgaeGrowthNutrientsMask", settingsRef.algaeSettings._AlgaeGrowthNutrientsMask);
        computeShaderAlgaeParticles.SetFloat("_AlgaeBaseGrowthRate", settingsRef.algaeSettings._AlgaeBaseGrowthRate);
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
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "velocityRead", fluidManagerRef._VelocityA);        
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightMap);
        computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "_SpawnDensityMap", algaeGridRT1);        
        computeShaderAnimalParticles.SetFloat("_GlobalOxygenLevel", resourcesManager.curGlobalOxygen); // needed?
        computeShaderAnimalParticles.SetFloat("_GlobalAlgaeLevel", resourceManagerRef.curGlobalAlgaeReservoir);
        
        // Movement Params:
        computeShaderAnimalParticles.SetFloat("_MasterSwimSpeed", settingsRef.zooplanktonSettings._MasterSwimSpeed); // = 0.35;
        computeShaderAnimalParticles.SetFloat("_AlignMaskRange", settingsRef.zooplanktonSettings._AlignMaskRange); // = 0.025;
        computeShaderAnimalParticles.SetFloat("_AlignMaskOffset", settingsRef.zooplanktonSettings._AlignMaskOffset); // = 0.0833;
        computeShaderAnimalParticles.SetFloat("_AlignSpeedMult", settingsRef.zooplanktonSettings._AlignSpeedMult); // = 0.00015;
        computeShaderAnimalParticles.SetFloat("_AttractMag", settingsRef.zooplanktonSettings._AttractMag); // = 0.0000137;
        computeShaderAnimalParticles.SetFloat("_AttractMaskMaxDistance", settingsRef.zooplanktonSettings._AttractMaskMaxDistance); // = 0.0036;
        computeShaderAnimalParticles.SetFloat("_AttractMaskOffset", settingsRef.zooplanktonSettings._AttractMaskOffset); // = 0.5;
        computeShaderAnimalParticles.SetFloat("_SwimNoiseMag", settingsRef.zooplanktonSettings._SwimNoiseMag); // = 0.000086;
        computeShaderAnimalParticles.SetFloat("_SwimNoiseFreqMin", settingsRef.zooplanktonSettings._SwimNoiseFreqMin); // = 0.00002
        computeShaderAnimalParticles.SetFloat("_SwimNoiseFreqRange", settingsRef.zooplanktonSettings._SwimNoiseFreqRange); // = 0.0002
        computeShaderAnimalParticles.SetFloat("_SwimNoiseOnOffFreq", settingsRef.zooplanktonSettings._SwimNoiseOnOffFreq); //  = 0.0001
        computeShaderAnimalParticles.SetFloat("_ShoreCollisionMag", settingsRef.zooplanktonSettings._ShoreCollisionMag); // = 0.0065;
        computeShaderAnimalParticles.SetFloat("_ShoreCollisionDistOffset", settingsRef.zooplanktonSettings._ShoreCollisionDistOffset); // = 0.15;
        computeShaderAnimalParticles.SetFloat("_ShoreCollisionDistSlope", settingsRef.zooplanktonSettings._ShoreCollisionDistSlope); // = 3.5;

        //computeShaderAnimalParticles.SetTexture(kernelCSSimulateAnimalParticles, "animalParticlesNearestCrittersRT", animalParticlesNearestCritters1);
        computeShaderAnimalParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        
        computeShaderAnimalParticles.SetFloat("_Time", Time.realtimeSinceStartup);

        // *** SPAWNING ***
        int eggSackIndex = Mathf.FloorToInt(Time.realtimeSinceStartup * 0.1f) % simStateDataRef.eggSackSimDataArray.Length;

        if(animalParticleMeasurementTotalsData[0].biomass < maxAnimalParticleTotal) {
            computeShaderAnimalParticles.SetFloat("_RespawnAnimalParticles", 1f);                       
        }
        else {
            computeShaderAnimalParticles.SetFloat("_RespawnAnimalParticles", 0f);      
        }
        // Need to compute when they should be allowed to spawn, how to keep track of resources used/transferred??
        computeShaderAnimalParticles.SetFloat("_SpawnPosX", UnityEngine.Random.Range(0.1f, 0.9f)); // UPDATE THIS!!! ****
        computeShaderAnimalParticles.SetFloat("_SpawnPosY", UnityEngine.Random.Range(0.1f, 0.9f));

        float minParticleSize = 0.1f; // settingsRef.avgAnimalParticleRadius / settingsRef.animalParticleRadiusVariance;
        float maxParticleSize = 0.2f; // settingsRef.avgAnimalParticleRadius * settingsRef.animalParticleRadiusVariance;

        computeShaderAnimalParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderAnimalParticles.SetFloat("_MaxParticleSize", maxParticleSize);
        // Revisit::::
        computeShaderAnimalParticles.SetFloat("_ParticleNutrientDensity", 10f); // settingsRef.animalParticleNutrientDensity);
        computeShaderAnimalParticles.SetFloat("_AnimalParticleRegrowthRate", 0.01f); // settingsRef.animalParticleRegrowthRate);  // ************  HARD-CODED!!!!

        computeShaderAnimalParticles.Dispatch(kernelCSSimulateAnimalParticles, 1, 1, 1);                

        // Copy/Swap Animal Particle Buffer:
        int kernelCSCopyAnimalParticlesBuffer = computeShaderAnimalParticles.FindKernel("CSCopyAnimalParticlesBuffer");
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesRead", animalParticlesCBufferSwap);
        computeShaderAnimalParticles.SetBuffer(kernelCSCopyAnimalParticlesBuffer, "animalParticlesWrite", animalParticlesCBuffer);        
        computeShaderAnimalParticles.Dispatch(kernelCSCopyAnimalParticlesBuffer, 1, 1, 1);        
        
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
        computeShaderAnimalParticles.SetTexture(kernelCSEatSelectedAnimalParticles, "critterDistancesRead", animalParticlesNearestCritters1);
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

        if (tempTex1 != null) {
            tempTex1.Release();
            tempTex2.Release();
            tempTex4.Release();
            tempTex8.Release();
            tempTex16.Release();
        }
        if(algaeGridSamplesCBuffer != null) {
            algaeGridSamplesCBuffer.Release();
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

        if(animalParticlesNearestCritters1 != null) {
            animalParticlesNearestCritters1.Release();
            animalParticlesNearestCritters32.Release();
            animalParticlesNearestCritters1024.Release();
        }        
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
    }
}
