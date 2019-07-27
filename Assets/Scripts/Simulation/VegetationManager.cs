using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationManager {

    public SettingsManager settingsRef;
    public SimResourceManager resourceManagerRef;
    
    private ComputeShader computeShaderResourceGrid;
    private ComputeShader computeShaderAlgaeParticles;

    //public float curGlobalAlgaeGrid = 0f;  // not using this currently
    
    public int resourceGridTexResolution = 128; 
    public RenderTexture resourceGridRT1;
    public RenderTexture resourceGridRT2;
    public Vector4[] resourceGridSamplesArray;
    public Vector4[] resourceGridEatAmountsArray;

    public RenderTexture rdRT1;
    public RenderTexture rdRT2;
    private int rdTextureResolution = 128;  // decomposers and algae Tex2D's
    // decomposer genomes:
    public WorldLayerDecomposerGenome decomposerSlotGenomeCurrent;
    public WorldLayerDecomposerGenome[] decomposerSlotGenomeMutations;

    public WorldLayerAlgaeGenome algaeSlotGenomeCurrent;  // algae particles!  -- likely to be converted into plants eventually ***
    public WorldLayerAlgaeGenome[] algaeSlotGenomeMutations;
    

    private RenderTexture tempTex16;
    private RenderTexture tempTex8;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex4;
    private RenderTexture tempTex2;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex1;
    
    private ComputeBuffer resourceGridAgentSamplesCBuffer;

    private const int numAlgaeParticles = 1024;  // *** 
    public ComputeBuffer algaeParticlesCBuffer;
    private ComputeBuffer algaeParticlesRepresentativeGenomeCBuffer;
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

    public bool isBrushActive = false;

    //public Vector2 algaeOriginPos;
    //public float algaeOnLerp01;

    //private AlgaeParticleData representativeAlgaeLayerGenome;
        
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
        public Vector3 hue;
    }
    
    private int GetAlgaeParticleDataSize() {
        int bitSize = sizeof(float) * 15 + sizeof(int) * 3;
        return bitSize;
    }

    /*public void MoveRandomResourceGridPatches(int index) {
        resourceGridSpawnPatchesArray[index] = new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f)); // (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
        Debug.Log("Moved Resource Patch! [" + index.ToString() + "], " + resourceGridSpawnPatchesArray[index].ToString());
    }*/
    //algaeSlotGenomeCurrent
    
    public VegetationManager(SettingsManager settings, SimResourceManager resourcesRef) {
        settingsRef = settings;
        resourceManagerRef = resourcesRef;
        
    }

    /*public void ProcessSlotMutation() {
        AlgaeParticleData[] algaeParticlesRepresentativeGenomeArray = new AlgaeParticleData[1];
        algaeParticlesRepresentativeGenomeArray[0] = algaeSlotGenomeCurrent.algaeRepData;
        algaeParticlesRepresentativeGenomeCBuffer.SetData(algaeParticlesRepresentativeGenomeArray);
    }*/

    // PLANT PARTICLES:::::
    public void InitializeAlgaeGrid() {
        // Plants:
        algaeSlotGenomeCurrent = new WorldLayerAlgaeGenome();
        
        //algaeSlotGenomeCurrent.algaeRepData = algaeParticlesArray[0];
        algaeSlotGenomeCurrent.displayColor = UnityEngine.Random.ColorHSV();
        algaeSlotGenomeCurrent.displayColor.a = 1f;
        algaeSlotGenomeCurrent.name = "Algae Particles!";        
            
        float minAlgaeMaxIntakeRate = 0.0005f;
        float maxAlgaeMaxIntakeRate = 0.005f;
        algaeSlotGenomeCurrent.algaeIntakeRate = Mathf.Lerp(minAlgaeMaxIntakeRate, maxAlgaeMaxIntakeRate, UnityEngine.Random.Range(0f, 1f));
            
        algaeSlotGenomeCurrent.algaeUpkeep = algaeSlotGenomeCurrent.algaeIntakeRate * (UnityEngine.Random.Range(0f, 1f) * 0.5f + 0.5f); // Mathf.Lerp(minAlgaeUpkeep, maxAlgaeUpkeep, UnityEngine.Random.Range(0f, 1f));
            
        float minAlgaeGrowthEfficiency = 0.5f;
        float maxAlgaeGrowthEfficiency = 2.5f;
        algaeSlotGenomeCurrent.algaeGrowthEfficiency = Mathf.Lerp(minAlgaeGrowthEfficiency, maxAlgaeGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));
         
        algaeSlotGenomeCurrent.textDescriptionMutation = "Upkeep: " + algaeSlotGenomeCurrent.algaeUpkeep.ToString("F4") + ", GrowthEfficiency: " + algaeSlotGenomeCurrent.algaeGrowthEfficiency.ToString("F2") + ", IntakeRate: " + algaeSlotGenomeCurrent.algaeIntakeRate.ToString("F4");
        
        // initialized in InitializeAlgaePArticles() method *** missing here
        algaeSlotGenomeMutations = new WorldLayerAlgaeGenome[4];

        GenerateWorldLayerAlgaeGridGenomeMutationOptions();
        /*
        algaeParticlesRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetAlgaeParticleDataSize());
        AlgaeParticleData[] algaeParticlesRepresentativeGenomeArray = new AlgaeParticleData[1];
        algaeParticlesRepresentativeGenomeArray[0] = algaeSlotGenomeCurrent.algaeRepData;
        algaeParticlesRepresentativeGenomeCBuffer.SetData(algaeParticlesRepresentativeGenomeArray);*/
    }
    public void InitializePlantParticles(int numAgents, ComputeShader computeShader) {
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
            data.hue = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
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
        

        
        
        //representativeAlgaeLayerGenome = 

        

        

    }    
    public void InitializeResourceGrid(int numAgents, ComputeShader computeShader) {

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

        //int kernelCSInitializeResourceGrid = computeShaderResourceGrid.FindKernel("CSInitializeResourceGrid");
        //computeShaderResourceGrid.SetTexture(kernelCSInitializeResourceGrid, "_ResourceGridWrite", resourceGridRT1);
        //computeShaderResourceGrid.Dispatch(kernelCSInitializeResourceGrid, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);
        //Graphics.Blit(resourceGridRT1, resourceGridRT2);

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

        
        // Decomposers and algae grid:
        rdRT1 = new RenderTexture(rdTextureResolution, rdTextureResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rdRT1.wrapMode = TextureWrapMode.Clamp;
        rdRT1.filterMode = FilterMode.Bilinear;
        rdRT1.enableRandomWrite = true;
        rdRT1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        rdRT2 = new RenderTexture(rdTextureResolution, rdTextureResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rdRT2.wrapMode = TextureWrapMode.Clamp;
        rdRT2.filterMode = FilterMode.Bilinear;
        rdRT2.enableRandomWrite = true;
        rdRT2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        //theRenderKing.fluidRenderMat.SetTexture("_DebugTex", nutrientMapRT1);
        
    }
    public void InitializeDecomposersGrid() {
        //int numMutations = 4;  // don't change this
        decomposerSlotGenomeCurrent = new WorldLayerDecomposerGenome();
        decomposerSlotGenomeCurrent.displayColor = UnityEngine.Random.ColorHSV();
        decomposerSlotGenomeCurrent.displayColor.a = 1f;
        decomposerSlotGenomeCurrent.name = "Decomposers";
        decomposerSlotGenomeMutations = new WorldLayerDecomposerGenome[4];
        

        float minIntakeRate = 0.0001f;
        float maxIntakeRate = 0.05f;
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        decomposerSlotGenomeCurrent.decomposerIntakeRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        decomposerSlotGenomeCurrent.decomposerUpkeep = decomposerSlotGenomeCurrent.decomposerIntakeRate * (UnityEngine.Random.Range(0f, 1f) * 0.5f + 0.5f); // Mathf.Lerp(minAlgaeUpkeep, maxAlgaeUpkeep, UnityEngine.Random.Range(0f, 1f));
        float minGrowthEfficiency = 0.5f;
        float maxGrowthEfficiency = 2.5f;
        decomposerSlotGenomeCurrent.decomposerGrowthEfficiency = Mathf.Lerp(minGrowthEfficiency, maxGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));
         
        decomposerSlotGenomeCurrent.textDescriptionMutation = "Upkeep: " + decomposerSlotGenomeCurrent.decomposerUpkeep.ToString("F4") + ", GrowthEfficiency: " + decomposerSlotGenomeCurrent.decomposerGrowthEfficiency.ToString("F2") + ", IntakeRate: " + decomposerSlotGenomeCurrent.decomposerIntakeRate.ToString("F4");
        
        // initialized in InitializeAlgaePArticles() method *** missing here
        

        GenerateWorldLayerDecomposersGenomeMutationOptions();

        


        int kernelCSInitRD = computeShaderResourceGrid.FindKernel("CSInitRD"); 
        //computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "rdRead", rdRT1);
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)rdTextureResolution);
        computeShaderResourceGrid.SetTexture(kernelCSInitRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSInitRD, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.Dispatch(kernelCSInitRD, rdTextureResolution / 32, rdTextureResolution / 32, 1);

    }

    public void GenerateWorldLayerAlgaeGridGenomeMutationOptions() {
        for(int j = 0; j < algaeSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            jLerp = jLerp * jLerp;
            WorldLayerAlgaeGenome mutatedGenome = new WorldLayerAlgaeGenome();
            Color randColor = UnityEngine.Random.ColorHSV();
            Color col = algaeSlotGenomeCurrent.displayColor;
            col = Color.Lerp(col, randColor, jLerp);
            mutatedGenome.displayColor = col;
            float minAlgaeMaxIntakeRate = 0.0001f;
            float maxAlgaeMaxIntakeRate = 0.005f;
            float logLerp = UnityEngine.Random.Range(0f, 1f);
            logLerp *= logLerp;
            mutatedGenome.algaeIntakeRate = Mathf.Lerp(minAlgaeMaxIntakeRate, maxAlgaeMaxIntakeRate, logLerp);
            mutatedGenome.algaeIntakeRate = Mathf.Lerp(algaeSlotGenomeCurrent.algaeIntakeRate, mutatedGenome.algaeIntakeRate, jLerp);
            //float minAlgaeUpkeep = 0.0001f;
            //float maxAlgaeUpkeep = 0.05f;
            mutatedGenome.algaeUpkeep = mutatedGenome.algaeIntakeRate * (UnityEngine.Random.Range(0f, 1f) * 0.5f + 0.5f); // Mathf.Lerp(minAlgaeUpkeep, maxAlgaeUpkeep, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.algaeUpkeep = Mathf.Lerp(algaeSlotGenomeCurrent.algaeUpkeep, mutatedGenome.algaeUpkeep, jLerp);
            float minAlgaeGrowthEfficiency = 0.5f;
            float maxAlgaeGrowthEfficiency = 2.5f;
            mutatedGenome.algaeGrowthEfficiency = Mathf.Lerp(minAlgaeGrowthEfficiency, maxAlgaeGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.algaeGrowthEfficiency = Mathf.Lerp(algaeSlotGenomeCurrent.algaeGrowthEfficiency, mutatedGenome.algaeGrowthEfficiency, jLerp);
            //mutatedGenome.feedRate = Mathf.Lerp(decomposerSlotGenomeCurrent.feedRate, UnityEngine.Random.Range(0f, 1f), jLerp);
            //mutatedGenome.killRate = Mathf.Lerp(decomposerSlotGenomeCurrent.killRate, UnityEngine.Random.Range(0f, 1f), jLerp);
            //mutatedGenome.scale = Mathf.Lerp(decomposerSlotGenomeCurrent.scale, UnityEngine.Random.Range(0f, 1f), jLerp);
            //mutatedGenome.reactionRate = Mathf.Lerp(decomposerSlotGenomeCurrent.reactionRate, UnityEngine.Random.Range(0f, 1f), jLerp);

            mutatedGenome.name = algaeSlotGenomeCurrent.name;
            mutatedGenome.textDescriptionMutation = "Upkeep: " + mutatedGenome.algaeUpkeep.ToString("F4") + ", GrowthEfficiency: " + mutatedGenome.algaeGrowthEfficiency.ToString("F2") + ", IntakeRate: " + mutatedGenome.algaeIntakeRate.ToString("F4");
            // other attributes here
            //mutatedGenome.elevationChange = Mathf.Lerp(bedrockSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

            algaeSlotGenomeMutations[j] = mutatedGenome;
        }
    }
    public void GenerateWorldLayerDecomposersGenomeMutationOptions() {
        for(int j = 0; j < decomposerSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            
            //int magnitudeIndex = Mathf.FloorToInt(jLerp * 3.99f);

            jLerp = jLerp * jLerp;
            WorldLayerDecomposerGenome mutatedGenome = new WorldLayerDecomposerGenome();
            Color randColor = UnityEngine.Random.ColorHSV();
            float randAlpha = UnityEngine.Random.Range(0f, 1f);  // shininess
            randColor.a = randAlpha;
            Color mutatedColor = Color.Lerp(decomposerSlotGenomeCurrent.displayColor, randColor, jLerp);
            mutatedGenome.displayColor = mutatedColor;


            
            float minDecomposerMaxIntakeRate = 0.0001f;
            float maxDecomposerMaxIntakeRate = 0.05f;
            float logLerp = UnityEngine.Random.Range(0f, 1f);
            logLerp *= logLerp;
            mutatedGenome.decomposerIntakeRate = Mathf.Lerp(minDecomposerMaxIntakeRate, maxDecomposerMaxIntakeRate, logLerp);
            mutatedGenome.decomposerIntakeRate = Mathf.Lerp(decomposerSlotGenomeCurrent.decomposerIntakeRate, mutatedGenome.decomposerIntakeRate, jLerp);
            
            mutatedGenome.decomposerUpkeep = mutatedGenome.decomposerIntakeRate * (UnityEngine.Random.Range(0f, 1f) * 0.5f + 0.5f); // Mathf.Lerp(minAlgaeUpkeep, maxAlgaeUpkeep, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.decomposerUpkeep = Mathf.Lerp(decomposerSlotGenomeCurrent.decomposerUpkeep, mutatedGenome.decomposerUpkeep, jLerp);
            float minAlgaeGrowthEfficiency = 0.5f;
            float maxAlgaeGrowthEfficiency = 2.5f;
            mutatedGenome.decomposerGrowthEfficiency = Mathf.Lerp(minAlgaeGrowthEfficiency, maxAlgaeGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.decomposerGrowthEfficiency = Mathf.Lerp(decomposerSlotGenomeCurrent.decomposerGrowthEfficiency, mutatedGenome.decomposerGrowthEfficiency, jLerp);
            
            
            string[] magnitudeWordsArray = new string[4];
            magnitudeWordsArray[0] = "Tiny";
            magnitudeWordsArray[1] = "Small";
            magnitudeWordsArray[2] = "Large";
            magnitudeWordsArray[3] = "Huge";

            mutatedGenome.name = decomposerSlotGenomeCurrent.name;
            mutatedGenome.textDescriptionMutation = "Upkeep: " + mutatedGenome.decomposerUpkeep.ToString("F4") + ", GrowthEfficiency: " + mutatedGenome.decomposerGrowthEfficiency.ToString("F2") + ", IntakeRate: " + mutatedGenome.decomposerIntakeRate.ToString("F4");
                //magnitudeWordsArray[j]; // "Mutation Amt: " + (jLerp * 100f).ToString("F0") + "% - " + mutatedGenome.reactionRate.ToString();
            // other attributes here
            //mutatedGenome.elevationChange = Mathf.Lerp(bedrockSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

            decomposerSlotGenomeMutations[j] = mutatedGenome;
        }
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
        Debug.Log("SpawnInitialAlgaeParticles(float radius, Vector4 spawnCoords) DISABLED!~");
        /*
        int kernelCSSpawnInitialAlgaeParticles = computeShaderAlgaeParticles.FindKernel("CSSpawnInitialAlgaeParticles");        
        computeShaderAlgaeParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderAlgaeParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderAlgaeParticles.SetVector("_FoodSprinklePos", spawnCoords);
        computeShaderAlgaeParticles.SetFloat("_FoodSprinkleRadius", radius);
        computeShaderAlgaeParticles.SetBuffer(kernelCSSpawnInitialAlgaeParticles, "foodParticlesWrite", algaeParticlesCBuffer);
        computeShaderAlgaeParticles.Dispatch(kernelCSSpawnInitialAlgaeParticles, 1, 1, 1);
        */
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
        computeShaderAlgaeParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_SpawnDensityMap", renderKingRef.spiritBrushRT);
        computeShaderAlgaeParticles.SetFloat("_MapSize", SimulationManager._MapSize);    
        computeShaderAlgaeParticles.SetFloat("_SpiritBrushPosNeg", renderKingRef.spiritBrushPosNeg);            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderAlgaeParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        //float randRoll = UnityEngine.Random.Range(0f, 1f);
        float brushF = 0f;
        if(renderKingRef.isSpiritBrushOn) {
            if(renderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {  // Plants kingdom selected
                //Debug.Log("brushF: " + brushF.ToString());
                if(renderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {  // Algae selected
                    brushF = 1f;
                }
            }            
        }
        computeShaderAlgaeParticles.SetFloat("_IsBrushing", brushF);

        

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
        // Representative AlgaePArticle struct data -- new particles spawn as mutations of this genome
        computeShaderAlgaeParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "_RepresentativeAlgaeParticleGenomeCBuffer", algaeParticlesRepresentativeGenomeCBuffer);

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
           
    // RESOURCE GRID:::::
    
    /*public void AdvectResourceGrid(EnvironmentFluidManager fluidManagerRef) {
        int kernelAdvectResourceGrid = computeShaderResourceGrid.FindKernel("CSAdvectResourceGrid");
        computeShaderResourceGrid.SetBuffer(kernelAdvectResourceGrid, "algaeParticlesRead", algaeParticlesCBuffer);
        computeShaderResourceGrid.SetFloat("_TextureResolution", 32f); // (float)resolution);
        computeShaderResourceGrid.SetFloat("_DeltaTime", fluidManagerRef.deltaTime);
        computeShaderResourceGrid.SetFloat("_InvGridScale", fluidManagerRef.invGridScale);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "ObstaclesRead", fluidManagerRef._ObstaclesRT);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "_ResourceGridRead", resourceGridRT2);
        computeShaderResourceGrid.SetTexture(kernelAdvectResourceGrid, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.Dispatch(kernelAdvectResourceGrid, 1, 1, 1);
    }
    */
    public void GetResourceGridValuesAtMouthPositions(SimulationStateData simStateDataRef) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        //ComputeBuffer nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        
        int kernelCSGetResourceGridSamples = computeShaderResourceGrid.FindKernel("CSGetResourceGridSamples");        
        computeShaderResourceGrid.SetBuffer(kernelCSGetResourceGridSamples, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderResourceGrid.SetBuffer(kernelCSGetResourceGridSamples, "_ResourceGridSamplesCBuffer", resourceGridAgentSamplesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSGetResourceGridSamples, "_ResourceGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.Dispatch(kernelCSGetResourceGridSamples, resourceGridAgentSamplesCBuffer.count, 1, 1);

        //Vector4[] outArray = new Vector4[_NumAgents];
        resourceGridAgentSamplesCBuffer.GetData(resourceGridSamplesArray); // Disappearing body strokes due to this !?!?!?!?!?

        //Debug.Log("Food: " + nutrientSamplesArray[0].x.ToString());
        //nutrientSamplesCBuffer.Release();
        
        // Read out sample values::::
    }
    /*public void ApplyDiffusionOnResourceGrid(EnvironmentFluidManager fluidManagerRef) {
        int kernelCSUpdateAlgaeGrid = computeShaderResourceGrid.FindKernel("CSUpdateAlgaeGrid");
        computeShaderResourceGrid.SetFloat("_AlgaeGridDiffusion", settingsRef.nutrientDiffusionRate);

        computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "ObstaclesRead", fluidManagerRef._ObstaclesRT);
        computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "_ResourceGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "_ResourceGridWrite", resourceGridRT2);
        computeShaderResourceGrid.Dispatch(kernelCSUpdateAlgaeGrid, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);

        //Graphics.Blit(resourceGridRT2, resourceGridRT1);
        
    } */   
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
        
        ComputeBuffer addResourceCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        Vector4[] addResourceArray = new Vector4[1];

        addResourceArray[0] = amount;
        addResourceCBuffer.SetData(addResourceArray);

        int kernelCSAddResourcesAtCoords = computeShaderResourceGrid.FindKernel("CSAddResourcesAtCoords");
        computeShaderResourceGrid.SetBuffer(kernelCSAddResourcesAtCoords, "_AddResourceCBuffer", addResourceCBuffer);        
        computeShaderResourceGrid.SetTexture(kernelCSAddResourcesAtCoords, "_ResourceGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAddResourcesAtCoords, "_ResourceGridWrite", resourceGridRT2);
        computeShaderResourceGrid.SetFloat("_CoordX", x);
        computeShaderResourceGrid.SetFloat("_CoordY", y);
        computeShaderResourceGrid.Dispatch(kernelCSAddResourcesAtCoords, 1, 1, 1);  // one-at-a-time for now, until re-factor (separate location buffers to resourceAmounts) ****
        
        Graphics.Blit(resourceGridRT2, resourceGridRT1);

        addResourceCBuffer.Release();
        
    }
    public void RemoveEatenResourceGrid(int numAgents, SimulationStateData simStateDataRef) { // **** WILL BE MODIFIED!!! *****
        ComputeBuffer eatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
                
        eatAmountsCBuffer.SetData(resourceGridEatAmountsArray);

        int kernelCSRemoveAlgaeAtLocations = computeShaderResourceGrid.FindKernel("CSRemoveResourceGridAtLocations");
        computeShaderResourceGrid.SetBuffer(kernelCSRemoveAlgaeAtLocations, "nutrientEatAmountsCBuffer", eatAmountsCBuffer);
        computeShaderResourceGrid.SetBuffer(kernelCSRemoveAlgaeAtLocations, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSRemoveAlgaeAtLocations, "_ResourceGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSRemoveAlgaeAtLocations, "_ResourceGridWrite", resourceGridRT2);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderResourceGrid.Dispatch(kernelCSRemoveAlgaeAtLocations, eatAmountsCBuffer.count, 1, 1);

        Graphics.Blit(resourceGridRT2, resourceGridRT1);
        
        eatAmountsCBuffer.Release();
    }

    // REACTION DIFFUSION (i.e DECOMPOSERS AND ALGAE GRID) ::::
    
    public void SimReactionDiffusionGrid(ref EnvironmentFluidManager fluidManagerRef, ref BaronVonTerrain baronTerrainRef, ref TheRenderKing theRenderKingRef) {
        int kernelCSSimRD = computeShaderResourceGrid.FindKernel("CSSimRD"); 
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "_AltitudeTex", baronTerrainRef.terrainHeightRT0);        
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)rdTextureResolution);
        computeShaderResourceGrid.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "_ResourceGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "_ResourceGridWrite", resourceGridRT2);
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "rdRead", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "rdWrite", rdRT2);
        computeShaderResourceGrid.Dispatch(kernelCSSimRD, rdTextureResolution / 32, rdTextureResolution / 32, 1);
        // write into 2
        int kernelCSAdvectRD = computeShaderResourceGrid.FindKernel("CSAdvectRD");
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)rdTextureResolution);        
        computeShaderResourceGrid.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderResourceGrid.SetFloat("_DeltaTime", fluidManagerRef.deltaTime);
        computeShaderResourceGrid.SetFloat("_InvGridScale", fluidManagerRef.invGridScale);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        float brushDecomposersOn = 0f;  // eventually make this more elegant during next refactor ***
        float brushAlgaeOn = 0f; 
        if(isBrushActive) {  // Set from uiManager

            if (theRenderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
                brushDecomposersOn = 1f;
            }
            else if (theRenderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
                if (theRenderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) { 
                    brushAlgaeOn = 1f;
                    Debug.Log("// Algae brush on!");
                }
            }
        }
        computeShaderResourceGrid.SetFloat("_SpiritBrushIntensity", 0.1f); // *** INVESTIGATE THIS -- not used/needed?
        computeShaderResourceGrid.SetFloat("_IsSpiritBrushDecomposersOn", brushDecomposersOn);
        computeShaderResourceGrid.SetFloat("_IsSpiritBrushAlgaeOn", brushAlgaeOn);
        computeShaderResourceGrid.SetFloat("_SpiritBrushPosNeg", theRenderKingRef.spiritBrushPosNeg);
        //computeShaderResourceGrid.SetFloat("_RD_FeedRate", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.feedRate);
        //computeShaderResourceGrid.SetFloat("_RD_KillRate", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.killRate);            
        //computeShaderResourceGrid.SetFloat("_RD_Scale", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.scale);
        //computeShaderResourceGrid.SetFloat("_RD_Rate", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate);

        //float algaeUpkeep = algaeSlotGenomeCurrent.
        computeShaderResourceGrid.SetFloat("_AlgaeUpkeep", algaeSlotGenomeCurrent.algaeUpkeep);
        computeShaderResourceGrid.SetFloat("_AlgaeMaxIntakeRate", algaeSlotGenomeCurrent.algaeIntakeRate);
        computeShaderResourceGrid.SetFloat("_AlgaeGrowthEfficiency", algaeSlotGenomeCurrent.algaeGrowthEfficiency);

        computeShaderResourceGrid.SetFloat("_DecomposerUpkeep", decomposerSlotGenomeCurrent.decomposerUpkeep);
        computeShaderResourceGrid.SetFloat("_DecomposerMaxIntakeRate", decomposerSlotGenomeCurrent.decomposerIntakeRate);
        computeShaderResourceGrid.SetFloat("_DecomposerEnergyGenerationEfficiency", decomposerSlotGenomeCurrent.decomposerGrowthEfficiency);
        
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "rdRead", rdRT2);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceGridRead", resourceGridRT2);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_SpiritBrushTex", theRenderKingRef.spiritBrushRT);
        computeShaderResourceGrid.Dispatch(kernelCSAdvectRD, rdTextureResolution / 32, rdTextureResolution / 32, 1);
        //back into 1
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
        
        if(algaeParticlesRepresentativeGenomeCBuffer != null) {
            algaeParticlesRepresentativeGenomeCBuffer.Release();
        }
    }
}
