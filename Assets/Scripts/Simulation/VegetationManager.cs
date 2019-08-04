using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationManager {

    public SettingsManager settingsRef;
    public SimResourceManager resourceManagerRef;
    
    private ComputeShader computeShaderResourceGrid;
    private ComputeShader computeShaderPlantParticles;

    public Vector4 curGlobalNutrientGridValues;  // not using this currently
    
    public int resourceGridTexResolution = 128; 
    public RenderTexture resourceGridRT1;
    public RenderTexture resourceGridRT2;
    public Vector4[] resourceGridSamplesArray;
    public Vector4[] resourceGridEatAmountsArray;

    public RenderTexture resourceSimTransferRT;

    public RenderTexture rdRT1;
    public RenderTexture rdRT2;
    private int rdTextureResolution = 128;  // decomposers and algae Tex2D's
    // decomposer genomes:
    public WorldLayerDecomposerGenome decomposerSlotGenomeCurrent;
    public WorldLayerDecomposerGenome[] decomposerSlotGenomeMutations;

    public WorldLayerAlgaeGenome algaeSlotGenomeCurrent;  // algae particles!  -- likely to be converted into plants eventually ***
    public WorldLayerAlgaeGenome[] algaeSlotGenomeMutations;

    public WorldLayerPlantGenome plantSlotGenomeCurrent;
    public WorldLayerPlantGenome[] plantSlotGenomeMutations;

    private RenderTexture tempTex32;
    private RenderTexture tempTex8;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex4;
    private RenderTexture tempTex2;  // <-- remove these and make function compress 4x
    private RenderTexture tempTex1;
    
    private ComputeBuffer resourceGridAgentSamplesCBuffer;

    private const int numAlgaeParticles = 1024;  // *** 
    public ComputeBuffer algaeParticlesCBuffer;
    private ComputeBuffer plantParticlesRepresentativeGenomeCBuffer;
    private ComputeBuffer algaeParticlesCBufferSwap;    
    private RenderTexture algaeParticlesNearestCritters1024;
    private RenderTexture algaeParticlesNearestCritters32;
    private RenderTexture algaeParticlesNearestCritters1;
    private ComputeBuffer closestAlgaeParticlesDataCBuffer;
    public PlantParticleData[] closestAlgaeParticlesDataArray;
    private ComputeBuffer algaeParticlesEatAmountsCBuffer;
    public float[] algaeParticlesEatAmountsArray;
    private ComputeBuffer algaeParticlesMeasure32;
    private ComputeBuffer algaeParticlesMeasure1;
    private PlantParticleData[] algaeParticleMeasurementTotalsData;
    
    public Vector2[] resourceGridSpawnPatchesArray;

    public bool isBrushActive = false;

    public float tempSharedIntakeRate = 0.0021f;
    //public Vector2 algaeOriginPos;
    //public float algaeOnLerp01;
    //public float cur

    //private AlgaeParticleData representativeAlgaeLayerGenome;
        
    public struct PlantParticleData {
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

    public void ProcessPlantSlotMutation() {  // was unused, still is but renamed
        PlantParticleData[] plantParticlesRepresentativeGenomeArray = new PlantParticleData[1];
        plantParticlesRepresentativeGenomeArray[0] = plantSlotGenomeCurrent.plantRepData;
        plantParticlesRepresentativeGenomeCBuffer.SetData(plantParticlesRepresentativeGenomeArray);
    }

    // PLANT PARTICLES:::::
    public void InitializeAlgaeGrid() {
        // Plants:
        algaeSlotGenomeCurrent = new WorldLayerAlgaeGenome();
        
        //algaeSlotGenomeCurrent.algaeRepData = algaeParticlesArray[0];
        algaeSlotGenomeCurrent.displayColor = UnityEngine.Random.ColorHSV();
        algaeSlotGenomeCurrent.displayColor.a = 1f;
        algaeSlotGenomeCurrent.name = "Algae Particles!";

        float minIntakeRate = tempSharedIntakeRate * 0.96f;
        float maxIntakeRate = tempSharedIntakeRate * 1.042f; // init around 1?
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        algaeSlotGenomeCurrent.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        //algaeSlotGenomeCurrent.algaeIntakeRate = Mathf.Lerp(minAlgaeMaxIntakeRate, maxAlgaeMaxIntakeRate, UnityEngine.Random.Range(0f, 1f));

        //algaeSlotGenomeCurrent.algaeUpkeep = algaeSlotGenomeCurrent.algaeIntakeRate * 0.45f; // * (UnityEngine.Random.Range(0f, 1f) * 0.05f + 0.95f); // Mathf.Lerp(minAlgaeUpkeep, maxAlgaeUpkeep, UnityEngine.Random.Range(0f, 1f));
            
        //float minAlgaeGrowthEfficiency = 1f;
        //float maxAlgaeGrowthEfficiency = 1f;
        algaeSlotGenomeCurrent.growthEfficiency = 1f; // Mathf.Lerp(minAlgaeGrowthEfficiency, maxAlgaeGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));

        algaeSlotGenomeCurrent.textDescriptionMutation = "Metabolic Rate: " + (algaeSlotGenomeCurrent.metabolicRate * 100f).ToString("F2"); // + ", GrowthEfficiency: " + algaeSlotGenomeCurrent.algaeGrowthEfficiency.ToString("F2") + ", IntakeRate: " + algaeSlotGenomeCurrent.algaeIntakeRate.ToString("F4");
        
        // initialized in InitializeAlgaePArticles() method *** missing here
        algaeSlotGenomeMutations = new WorldLayerAlgaeGenome[4];

        GenerateWorldLayerAlgaeGridGenomeMutationOptions();
        
    }
    public void InitializePlantParticles(int numAgents, ComputeShader computeShader) {
        //float startTime = Time.realtimeSinceStartup;
        //Debug.Log((Time.realtimeSinceStartup - startTime).ToString());
        computeShaderPlantParticles = computeShader;
        
        algaeParticlesCBuffer = new ComputeBuffer(numAlgaeParticles, GetAlgaeParticleDataSize());
        algaeParticlesCBufferSwap = new ComputeBuffer(numAlgaeParticles, GetAlgaeParticleDataSize());
        PlantParticleData[] algaeParticlesArray = new PlantParticleData[numAlgaeParticles];

        float minParticleSize = settingsRef.avgAlgaeParticleRadius / settingsRef.algaeParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgAlgaeParticleRadius * settingsRef.algaeParticleRadiusVariance;

        for(int i = 0; i < algaeParticlesCBuffer.count; i++) {
            PlantParticleData data = new PlantParticleData();
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
        closestAlgaeParticlesDataArray = new PlantParticleData[numAgents];
        closestAlgaeParticlesDataCBuffer = new ComputeBuffer(numAgents, GetAlgaeParticleDataSize());

        algaeParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        algaeParticlesEatAmountsArray = new float[numAgents];

        algaeParticleMeasurementTotalsData = new PlantParticleData[1];
        algaeParticlesMeasure32 = new ComputeBuffer(32, GetAlgaeParticleDataSize());
        algaeParticlesMeasure1 = new ComputeBuffer(1, GetAlgaeParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
        
        
        //plantSlotGenomeCurrent
        plantSlotGenomeCurrent = new WorldLayerPlantGenome();
        
        //algaeSlotGenomeCurrent.algaeRepData = algaeParticlesArray[0];
        plantSlotGenomeCurrent.displayColor = UnityEngine.Random.ColorHSV();
        plantSlotGenomeCurrent.displayColor.a = 1f;
        plantSlotGenomeCurrent.name = "Plant Particles!";

        float minRate = 0.5f;
        float maxRate = 1.75f;
        plantSlotGenomeCurrent.growthRate = Mathf.Lerp(minRate, maxRate, UnityEngine.Random.Range(0f, 1f));

        plantSlotGenomeCurrent.textDescriptionMutation = "Growth Rate: " + plantSlotGenomeCurrent.growthRate.ToString("F2"); // + ", GrowthEfficiency: " + plantSlotGenomeCurrent.plantGrowthEfficiency.ToString("F2") + ", IntakeRate: " + plantSlotGenomeCurrent.plantIntakeRate.ToString("F4");
        
        // initialized in InitializeAlgaePArticles() method *** missing here
        plantSlotGenomeMutations = new WorldLayerPlantGenome[4];


        
        plantParticlesRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetAlgaeParticleDataSize());
        PlantParticleData[] plantParticlesRepresentativeGenomeArray = new PlantParticleData[1];
        Debug.Log(" BADFHADFHADF" + plantSlotGenomeCurrent.name);
        plantParticlesRepresentativeGenomeArray[0] = algaeParticlesArray[0]; // plantSlotGenomeCurrent.plantRepData;
        plantParticlesRepresentativeGenomeCBuffer.SetData(plantParticlesRepresentativeGenomeArray);
        

        GenerateWorldLayerPlantParticleGenomeMutationOptions();
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

        resourceSimTransferRT = new RenderTexture(resourceGridTexResolution, resourceGridTexResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        resourceSimTransferRT.wrapMode = TextureWrapMode.Clamp;
        resourceSimTransferRT.enableRandomWrite = true;
        resourceSimTransferRT.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***  
        
        
        tempTex32 = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex32.wrapMode = TextureWrapMode.Clamp;
        tempTex32.filterMode = FilterMode.Point;
        tempTex32.enableRandomWrite = true;
        tempTex32.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

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

        float minIntakeRate = tempSharedIntakeRate * 0.95f;
        float maxIntakeRate = tempSharedIntakeRate * 1.042f;
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        decomposerSlotGenomeCurrent.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        decomposerSlotGenomeCurrent.textDescriptionMutation = "Metabolic Rate: " + (decomposerSlotGenomeCurrent.metabolicRate * 100f).ToString("F2"); 

        GenerateWorldLayerDecomposersGenomeMutationOptions();

        


        int kernelCSInitRD = computeShaderResourceGrid.FindKernel("CSInitRD"); 
        //computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "rdRead", rdRT1);
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)rdTextureResolution);
        //computeShaderResourceGrid.SetTexture(kernelCSInitRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSInitRD, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.Dispatch(kernelCSInitRD, rdTextureResolution / 32, rdTextureResolution / 32, 1);

    }

    private void WorldLayerDecomposerGenomeStuff(ref WorldLayerDecomposerGenome genome, float mutationSizeLerp) {

        
        float minIntakeRate = tempSharedIntakeRate * 0.36f;
        float maxIntakeRate = tempSharedIntakeRate * 2.42f;
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        genome.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        genome.metabolicRate = Mathf.Lerp(decomposerSlotGenomeCurrent.metabolicRate, genome.metabolicRate, mutationSizeLerp);

        genome.name = decomposerSlotGenomeCurrent.name;
        genome.textDescriptionMutation = "Metabolic Rate: " + (genome.metabolicRate * 100f).ToString("F2"); // + ", GrowthEfficiency: " + mutatedGenome.algaeGrowthEfficiency.ToString("F2") + ", IntakeRate: " + mutatedGenome.algaeIntakeRate.ToString("F4");
            
        genome.growthEfficiency = 1f;
        //decomposerSlotGenomeCurrent.decomposerUpkeep = decomposerSlotGenomeCurrent.decomposerIntakeRate * 0.45f; //  * (UnityEngine.Random.Range(0f, 1f) * 0.05f + 0.95f); // Mathf.Lerp(minAlgaeUpkeep, maxAlgaeUpkeep, UnityEngine.Random.Range(0f, 1f));
        //float minGrowthEfficiency = 1f;
        //float maxGrowthEfficiency = 1f;
        //genome.growthEfficiency = 1f; // Mathf.Lerp(minGrowthEfficiency, maxGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));  // ** Remove this? works best at 1

        //genome.textDescriptionMutation = "Metabolic Rate: " + (genome.metabolicRate * 100f + 0.9f).ToString("F2"); // + ", GrowthEfficiency: " + genome.growthEfficiency.ToString("F2");
          
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

            float minIntakeRate = tempSharedIntakeRate * 0.36f;
            float maxIntakeRate = tempSharedIntakeRate * 2.42f; // init around 1?
            float lnLerp = UnityEngine.Random.Range(0f, 1f);
            lnLerp *= lnLerp;
            mutatedGenome.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
            mutatedGenome.metabolicRate = Mathf.Lerp(algaeSlotGenomeCurrent.metabolicRate, mutatedGenome.metabolicRate, jLerp);
            mutatedGenome.growthEfficiency = 1f; // Mathf.Lerp(minAlgaeGrowthEfficiency, maxAlgaeGrowthEfficiency, UnityEngine.Random.Range(0f, 1f));

            mutatedGenome.textDescriptionMutation = "Metabolic Rate: " + (mutatedGenome.metabolicRate * 100f).ToString("F2"); // + ", GrowthEfficiency: " + algaeSlotGenomeCurrent.algaeGrowthEfficiency.ToString("F2") + ", IntakeRate: " + algaeSlotGenomeCurrent.algaeIntakeRate.ToString("F4");
        
            // other attributes here
            //mutatedGenome.elevationChange = Mathf.Lerp(bedrockSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

            algaeSlotGenomeMutations[j] = mutatedGenome;
        }
    }
    public void GenerateWorldLayerPlantParticleGenomeMutationOptions() {
        for(int j = 0; j < plantSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            jLerp = jLerp * jLerp;
            WorldLayerPlantGenome mutatedGenome = new WorldLayerPlantGenome();
            Color randColor = UnityEngine.Random.ColorHSV();
            Color col = plantSlotGenomeCurrent.displayColor;
            col = Color.Lerp(col, randColor, jLerp);
            mutatedGenome.displayColor = col;

            float minRate = 0.5f;
            float maxRate = 1.75f;
            mutatedGenome.growthRate = Mathf.Lerp(minRate, maxRate, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.growthRate = Mathf.Lerp(plantSlotGenomeCurrent.growthRate, mutatedGenome.growthRate, jLerp);

            mutatedGenome.name = plantSlotGenomeCurrent.name;
            mutatedGenome.textDescriptionMutation = "Growth Rate: " + mutatedGenome.growthRate.ToString("F2");
            
            plantSlotGenomeMutations[j] = mutatedGenome;
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

            WorldLayerDecomposerGenomeStuff(ref mutatedGenome, jLerp);

            /*
            string[] magnitudeWordsArray = new string[4];
            magnitudeWordsArray[0] = "Tiny";
            magnitudeWordsArray[1] = "Small";
            magnitudeWordsArray[2] = "Large";
            magnitudeWordsArray[3] = "Huge";
            */
            mutatedGenome.name = decomposerSlotGenomeCurrent.name;
            mutatedGenome.textDescriptionMutation = "Metabolic Rate: " + (mutatedGenome.metabolicRate * 100f).ToString("F2"); 
            //mutatedGenome.textDescriptionMutation = "Upkeep: " + mutatedGenome.decomposerUpkeep.ToString("F4") + ", GrowthEfficiency: " + mutatedGenome.decomposerGrowthEfficiency.ToString("F2") + ", IntakeRate: " + mutatedGenome.decomposerIntakeRate.ToString("F4");
                //magnitudeWordsArray[j]; // "Mutation Amt: " + (jLerp * 100f).ToString("F0") + "% - " + mutatedGenome.reactionRate.ToString();
            // other attributes here
            //mutatedGenome.elevationChange = Mathf.Lerp(bedrockSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

            decomposerSlotGenomeMutations[j] = mutatedGenome;
        }

        //WorldLayerDecomposerGenomeStuff(ref decomposerSlotGenomeCurrent, 0f);
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
    /*public void SpawnInitialPlantParticles(float radius, Vector4 spawnCoords) {
        Debug.Log("SpawnInitialAlgaeParticles(float radius, Vector4 spawnCoords) DISABLED!~");
        
        int kernelCSSpawnInitialAlgaeParticles = computeShaderPlantParticles.FindKernel("CSSpawnInitialAlgaeParticles");        
        computeShaderPlantParticles.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderPlantParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderPlantParticles.SetVector("_FoodSprinklePos", spawnCoords);
        computeShaderPlantParticles.SetFloat("_FoodSprinkleRadius", radius);
        computeShaderPlantParticles.SetBuffer(kernelCSSpawnInitialAlgaeParticles, "foodParticlesWrite", algaeParticlesCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSSpawnInitialAlgaeParticles, 1, 1, 1);
        
    }*/
    public void SimulatePlantParticles(EnvironmentFluidManager fluidManagerRef, TheRenderKing renderKingRef, SimulationStateData simStateDataRef, SimResourceManager resourcesManager) { // Sim
        // Go through foodParticleData and check for inactive
        // determined by current total food -- done!
        // if flag on shader for Respawn is on, set to active and initialize
        float maxFoodParticleTotal = settingsRef.maxFoodParticleTotalAmount;

        //Debug.Log("SimulatePlantParticles");

        int kernelCSSimulateAlgaeParticles = computeShaderPlantParticles.FindKernel("CSSimulateAlgaeParticles");
        computeShaderPlantParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "foodParticlesWrite", algaeParticlesCBufferSwap);
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "velocityRead", fluidManagerRef._VelocityA);        
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightDataRT);
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_SpawnDensityMap", renderKingRef.spiritBrushRT);
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_ResourceGridRead", resourceGridRT1);
        computeShaderPlantParticles.SetFloat("_MapSize", SimulationManager._MapSize);    
        computeShaderPlantParticles.SetFloat("_SpiritBrushPosNeg", renderKingRef.spiritBrushPosNeg);            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderPlantParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        //float randRoll = UnityEngine.Random.Range(0f, 1f);
        float brushF = 0f;
        if(renderKingRef.isSpiritBrushOn) {
            if(renderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {  // Plants kingdom selected
                //Debug.Log("brushF: " + brushF.ToString());
                if(renderKingRef.simManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {  // Algae selected
                    //brushF = 1f;
                }
                else {  // Big Plants
                    //Debug.Log("WOOOOOOOOOOOOOOOOOOOOOOOOO");
                    brushF = 1f;
                }
            }            
        }
        computeShaderPlantParticles.SetFloat("_IsBrushing", brushF);

        float spawnLerp = renderKingRef.simManager.trophicLayersManager.GetAlgaeOnLerp(renderKingRef.simManager.simAgeTimeSteps);
        float spawnRadius = Mathf.Lerp(1f, SimulationManager._MapSize, spawnLerp);
        Vector4 spawnPos = new Vector4(renderKingRef.simManager.trophicLayersManager.algaeOriginPos.x, renderKingRef.simManager.trophicLayersManager.algaeOriginPos.y, 0f, 0f);
        computeShaderPlantParticles.SetFloat("_FoodSprinkleRadius", spawnRadius);
        computeShaderPlantParticles.SetVector("_FoodSprinklePos", spawnPos);
        
        float minParticleSize = settingsRef.avgAlgaeParticleRadius / settingsRef.algaeParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgAlgaeParticleRadius * settingsRef.algaeParticleRadiusVariance;

        computeShaderPlantParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderPlantParticles.SetFloat("_MaxParticleSize", maxParticleSize);      
        computeShaderPlantParticles.SetFloat("_ParticleNutrientDensity", settingsRef.algaeParticleNutrientDensity);
        computeShaderPlantParticles.SetFloat("_FoodParticleRegrowthRate", settingsRef.foodParticleRegrowthRate);

        computeShaderPlantParticles.SetFloat("_GlobalNutrients", resourcesManager.curGlobalNutrients);
        computeShaderPlantParticles.SetFloat("_SolarEnergy", settingsRef.environmentSettings._BaseSolarEnergy);
        computeShaderPlantParticles.SetFloat("_AlgaeGrowthNutrientsMask", settingsRef.algaeSettings._AlgaeGrowthNutrientsMask);
        computeShaderPlantParticles.SetFloat("_AlgaeBaseGrowthRate", settingsRef.algaeSettings._AlgaeBaseGrowthRate * plantSlotGenomeCurrent.growthRate); // * (1f + 3 * (1.0f - (float)renderKingRef.simManager.uiManager.recentlyCreatedSpeciesTimeStepCounter / 360f)));

        computeShaderPlantParticles.SetFloat("_AlgaeGrowthNutrientUsage", settingsRef.algaeSettings._AlgaeGrowthNutrientUsage);
        computeShaderPlantParticles.SetFloat("_AlgaeGrowthOxygenProduction", settingsRef.algaeSettings._AlgaeGrowthOxygenProduction);
        computeShaderPlantParticles.SetFloat("_AlgaeAgingRate", settingsRef.algaeSettings._AlgaeAgingRate);
        computeShaderPlantParticles.SetFloat("_AlgaeDecayRate", settingsRef.algaeSettings._AlgaeDecayRate);
        computeShaderPlantParticles.SetFloat("_AlgaeSpawnMaxAltitude", settingsRef.algaeSettings._AlgaeSpawnMaxAltitude);
        computeShaderPlantParticles.SetFloat("_AlgaeParticleInitMass", settingsRef.algaeSettings._AlgaeParticleInitMass);
        // Representative AlgaePArticle struct data -- new particles spawn as mutations of this genome
        computeShaderPlantParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "_RepresentativeAlgaeParticleGenomeCBuffer", plantParticlesRepresentativeGenomeCBuffer);

        computeShaderPlantParticles.Dispatch(kernelCSSimulateAlgaeParticles, 1, 1, 1);                

        // Copy/Swap Food Particle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderPlantParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", algaeParticlesCBufferSwap);
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", algaeParticlesCBuffer);        
        computeShaderPlantParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);        
        
    }
    public void EatSelectedFoodParticles(SimulationStateData simStateDataRef) {  // removes gpu particle & sends consumption data back to CPU
        // Use CritterSimData to determine critter mouth locations

        // run through all foodParticles, check against each critter position, then measure min value with recursive reduction:

        // Need to update CritterSim&InitData structs to have more mouth/bite info

        // Record how much food successfully eaten per Critter

        int kernelCSEatSelectedFoodParticles = computeShaderPlantParticles.FindKernel("CSEatSelectedFoodParticles");
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesWrite", algaeParticlesCBufferSwap);
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesEatAmountsCBuffer", algaeParticlesEatAmountsCBuffer);        
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "closestParticlesDataCBuffer", closestAlgaeParticlesDataCBuffer);  
        computeShaderPlantParticles.SetTexture(kernelCSEatSelectedFoodParticles, "critterDistancesRead", algaeParticlesNearestCritters1);
        computeShaderPlantParticles.Dispatch(kernelCSEatSelectedFoodParticles, simStateDataRef.critterSimDataCBuffer.count, 1, 1);

        algaeParticlesEatAmountsCBuffer.GetData(algaeParticlesEatAmountsArray);

        float totalFoodEaten = 0f;
        for(int i = 0; i < algaeParticlesEatAmountsCBuffer.count; i++) {
            totalFoodEaten += algaeParticlesEatAmountsArray[i];
        }
        // Copy/Swap Food PArticle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderPlantParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", algaeParticlesCBufferSwap);
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", algaeParticlesCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
    }
    public void FindClosestAlgaeParticleToCritters(SimulationStateData simStateDataRef) {  // need to send info on closest particle pos/dir/amt back to CPU also
        
        // Populate main RenderTexture with distances for each foodParticle to each Critter:

        int kernelCSMeasureInitCritterDistances = computeShaderPlantParticles.FindKernel("CSMeasureInitCritterDistances");
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "foodParticlesRead", algaeParticlesCBuffer);        
        computeShaderPlantParticles.SetTexture(kernelCSMeasureInitCritterDistances, "foodParticlesNearestCrittersRT", algaeParticlesNearestCritters1024);        
        computeShaderPlantParticles.Dispatch(kernelCSMeasureInitCritterDistances, algaeParticlesCBuffer.count / 1024, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 1024 --> 32 particles per critter:
        int kernelCSReduceCritterDistances32 = computeShaderPlantParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", algaeParticlesNearestCritters1024);
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", algaeParticlesNearestCritters32);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", algaeParticlesCBuffer);        
        computeShaderPlantParticles.Dispatch(kernelCSReduceCritterDistances32, 32, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        // Reduce from 32 --> 1 particles per critter:
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", algaeParticlesNearestCritters32);
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", algaeParticlesNearestCritters1);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestAlgaeParticlesDataCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSReduceCritterDistances32, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        // Copy/Swap Food PArticle Buffer:
        //int kernelCSCopyFoodParticlesBuffer = computeShaderFoodParticles.FindKernel("CSCopyFoodParticlesBuffer");
        //computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", foodParticlesCBufferSwap);
        //computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", foodParticlesCBuffer);
        //computeShaderFoodParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);

        closestAlgaeParticlesDataCBuffer.GetData(closestAlgaeParticlesDataArray);

        //Debug.Log("ClosestFoodParticle: " + closestFoodParticlesDataArray[0].index.ToString() + ", " + closestFoodParticlesDataArray[0].worldPos.ToString() + ", amt: " + closestFoodParticlesDataArray[0].foodAmount.ToString());
    }
    public void MeasureTotalPlantParticlesAmount() {
        
        int kernelCSMeasureTotalFoodParticlesAmount = computeShaderPlantParticles.FindKernel("CSMeasureTotalFoodParticlesAmount");
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", algaeParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", algaeParticlesMeasure32);
         
        // DISPATCH !!!
        computeShaderPlantParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 32, 1, 1);
        
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", algaeParticlesMeasure32);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", algaeParticlesMeasure1);
        computeShaderPlantParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 1, 1, 1);
        
        algaeParticlesMeasure1.GetData(algaeParticleMeasurementTotalsData);
        resourceManagerRef.curGlobalPlantParticles = algaeParticleMeasurementTotalsData[0].biomass;
        resourceManagerRef.oxygenProducedByPlantParticlesLastFrame = algaeParticleMeasurementTotalsData[0].oxygenProduced;
        resourceManagerRef.wasteProducedByPlantParticlesLastFrame = algaeParticleMeasurementTotalsData[0].wasteProduced;
        resourceManagerRef.nutrientsUsedByPlantParticlesLastFrame = algaeParticleMeasurementTotalsData[0].nutrientsUsed;

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
    public void MeasureTotalResourceGridAmount() {

        ComputeBuffer outputValuesCBuffer = new ComputeBuffer(1, sizeof(float) * 4);  // holds the result of measurement: total sum of pix colors in texture
        Vector4[] outputValuesArray = new Vector4[1];

        // WAS 32!


        // 128 --> 32:
        int kernelCSMeasureTotalResources2 = computeShaderResourceGrid.FindKernel("CSMeasureTotalResourceGrid2");   
        int kernelCSMeasureTotalResources4 = computeShaderResourceGrid.FindKernel("CSMeasureTotalResourceGrid4");  

        computeShaderResourceGrid.SetBuffer(kernelCSMeasureTotalResources4, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources4, "measureValuesTex", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources4, "pooledResultTex", tempTex32);
        computeShaderResourceGrid.Dispatch(kernelCSMeasureTotalResources4, 32, 32, 1);
        // 32 --> 8:
        computeShaderResourceGrid.SetBuffer(kernelCSMeasureTotalResources4, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources4, "measureValuesTex", tempTex32);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources4, "pooledResultTex", tempTex8);
        computeShaderResourceGrid.Dispatch(kernelCSMeasureTotalResources4, 8, 8, 1);
        // 8 --> 4:
        computeShaderResourceGrid.SetBuffer(kernelCSMeasureTotalResources2, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources2, "measureValuesTex", tempTex8);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources2, "pooledResultTex", tempTex4);
        computeShaderResourceGrid.Dispatch(kernelCSMeasureTotalResources2, 4, 4, 1);        
        // 4 --> 2:
        computeShaderResourceGrid.SetBuffer(kernelCSMeasureTotalResources2, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources2, "measureValuesTex", tempTex4);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources2, "pooledResultTex", tempTex2);
        computeShaderResourceGrid.Dispatch(kernelCSMeasureTotalResources2, 2, 2, 1);
        // 2 --> 1:
        computeShaderResourceGrid.SetBuffer(kernelCSMeasureTotalResources2, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources2, "measureValuesTex", tempTex2);
        computeShaderResourceGrid.SetTexture(kernelCSMeasureTotalResources2, "pooledResultTex", tempTex1);
        computeShaderResourceGrid.Dispatch(kernelCSMeasureTotalResources2, 1, 1, 1);
        
        outputValuesCBuffer.GetData(outputValuesArray);

        /* ********************      ********************  v Re-CONNECT!!!
        curGlobalAlgaeGrid = outputValuesArray[0].x;
        */
        curGlobalNutrientGridValues = outputValuesArray[0];

        outputValuesCBuffer.Release();

        /*Debug.Log("Resource Totals:\nNutrients: " + outputValuesArray[0].x.ToString() + 
                                  "\nWaste: " + outputValuesArray[0].y.ToString() + 
                                  "\nDecomposers: " + outputValuesArray[0].z.ToString() + 
                                  "\nAlgae: " + outputValuesArray[0].w.ToString());
*/
        //return outputValuesArray[0].x;
    }    
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
    
    public void SimResourceGrid(ref EnvironmentFluidManager fluidManagerRef, ref BaronVonTerrain baronTerrainRef, ref TheRenderKing theRenderKingRef) {
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
                else {
                    //brushPlantsOn = 1f;
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
        computeShaderResourceGrid.SetFloat("_AlgaeUpkeep", algaeSlotGenomeCurrent.metabolicRate * 0.42f); // decomposerSlotGenomeCurrent.metabolicRate);  // *********** SHARING WITH DECOMPOSERS!!!! *****
        computeShaderResourceGrid.SetFloat("_AlgaeMaxIntakeRate", algaeSlotGenomeCurrent.metabolicRate);
        computeShaderResourceGrid.SetFloat("_AlgaeGrowthEfficiency", 1f);

        computeShaderResourceGrid.SetFloat("_DecomposerUpkeep", decomposerSlotGenomeCurrent.metabolicRate * 0.42f); // decomposerSlotGenomeCurrent.metabolicRate);
        computeShaderResourceGrid.SetFloat("_DecomposerMaxIntakeRate", decomposerSlotGenomeCurrent.metabolicRate);
        computeShaderResourceGrid.SetFloat("_DecomposerEnergyGenerationEfficiency", 1f);
        
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "rdRead", rdRT2);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceGridRead", resourceGridRT2);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_SpiritBrushTex", theRenderKingRef.spiritBrushRT);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceSimTransferRead", resourceSimTransferRT);
        computeShaderResourceGrid.Dispatch(kernelCSAdvectRD, rdTextureResolution / 32, rdTextureResolution / 32, 1);
        //back into 1
    }

    

    
    public void ClearBuffers() {

        if (tempTex1 != null) {
            tempTex1.Release();
            tempTex2.Release();
            tempTex4.Release();
            tempTex8.Release();
            tempTex32.Release();
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
        
        if(plantParticlesRepresentativeGenomeCBuffer != null) {
            plantParticlesRepresentativeGenomeCBuffer.Release();
        }
    }
}
