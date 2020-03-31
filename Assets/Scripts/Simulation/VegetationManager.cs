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

    private const int numPlantParticles = 1024;  // *** 
    public ComputeBuffer plantParticlesCBuffer;
    private ComputeBuffer plantParticlesRepresentativeGenomeCBuffer;
    private ComputeBuffer plantParticlesCBufferSwap;    
    //private RenderTexture plantParticlesNearestCritters1024;
    //private RenderTexture plantParticlesNearestCritters32;
    //private RenderTexture plantParticlesNearestCritters1;
    private ComputeBuffer closestPlantParticlesDataCBuffer;
    public PlantParticleData[] closestPlantParticlesDataArray;
    //private ComputeBuffer closestParticlesToCursorDataCBuffer;    
    //public PlantParticleData[] closestParticlesToCursorDataArray;
    private ComputeBuffer cursorClosestParticleDataCBuffer;
    public PlantParticleData[] cursorParticleDataArray;
    private ComputeBuffer cursorDistances1024;
    public PlantParticleData selectedPlantParticleData;
    public PlantParticleData closestPlantParticleData;
    public bool isPlantParticleSelected = false;
    public int selectedPlantParticleIndex = 0;
    //private ComputeBuffer cursorDistances32;
    //private ComputeBuffer cursorDistances1;

    private ComputeBuffer plantParticlesEatAmountsCBuffer;
    public float[] plantParticlesEatAmountsArray;
    private ComputeBuffer plantParticlesMeasure32;
    private ComputeBuffer plantParticlesMeasure1;
    private PlantParticleData[] plantParticleMeasurementTotalsData;

    public RenderTexture critterNearestPlants32;
    private ComputeBuffer closestPlantIndexCBuffer;
    public Vector4[] closestPlantIndexArray;
    
    public Vector2[] resourceGridSpawnPatchesArray;

    public bool isBrushActive = false;

    public Vector3 tempClosestPlantParticleIndexAndPos;

    private float tempSharedIntakeRate = 0.0065f;
    
    public struct PlantParticleData {  // 4 ints, 28 floats
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
	    public Vector3 colorA;	
	    public Vector3 colorB;
	    public float health;
	    public float typeID;
	    public float rootedness;
	    public float radiusAxisOne;
	    public float radiusAxisTwo;
	    public float leafDensity;
	    public float angleInc;
	    public float leafLength;
	    public float leafWidth;
	    public float leafRoundness;
	    public int brushTypeX;
    }
    
    private int GetPlantParticleDataSize() {
        int bitSize = sizeof(float) * 28 + sizeof(int) * 4;
        return bitSize;
    }

    public int tempMeasureClosestParticlesCounter = 0;
    
    public VegetationManager(SettingsManager settings, SimResourceManager resourcesRef) {
        settingsRef = settings;
        resourceManagerRef = resourcesRef;
        
    }

    public void ProcessPlantSlotMutation() {  // was unused, still is but renamed
        PlantParticleData[] plantParticlesRepresentativeGenomeArray = new PlantParticleData[1];
        plantParticlesRepresentativeGenomeArray[0] = plantSlotGenomeCurrent.plantRepData;
        plantParticlesRepresentativeGenomeCBuffer.SetData(plantParticlesRepresentativeGenomeArray);
        Debug.Log("ASDF ProcessPlantSlotMutation " + plantSlotGenomeCurrent.growthRate.ToString());
        Vector3 hueA = plantSlotGenomeCurrent.plantRepData.colorA;
        plantSlotGenomeCurrent.displayColorPri = new Color(hueA.x, hueA.y, hueA.z);
        Vector3 hueB = plantSlotGenomeCurrent.plantRepData.colorB;
        plantSlotGenomeCurrent.displayColorSec = new Color(hueB.x, hueB.y, hueB.z);
    }

    // PLANT PARTICLES:::::
    public void InitializeAlgaeGrid() {
        // Plants:
        algaeSlotGenomeCurrent = new WorldLayerAlgaeGenome();
        
        algaeSlotGenomeMutations = new WorldLayerAlgaeGenome[4];

        GenerateWorldLayerAlgaeGridGenomeMutationOptions();
        
    }
    public void InitializePlantParticles(int numAgents, ComputeShader computeShader) {
        //float startTime = Time.realtimeSinceStartup;
        //Debug.Log((Time.realtimeSinceStartup - startTime).ToString());
        computeShaderPlantParticles = computeShader;
        
        plantParticlesCBuffer = new ComputeBuffer(numPlantParticles, GetPlantParticleDataSize());
        plantParticlesCBufferSwap = new ComputeBuffer(numPlantParticles, GetPlantParticleDataSize());
        PlantParticleData[] plantParticlesArray = new PlantParticleData[numPlantParticles];

        float minParticleSize = settingsRef.avgAlgaeParticleRadius / settingsRef.algaeParticleRadiusVariance;
        float maxParticleSize = settingsRef.avgAlgaeParticleRadius * settingsRef.algaeParticleRadiusVariance;

        for(int i = 0; i < plantParticlesCBuffer.count; i++) {
            PlantParticleData data = new PlantParticleData();
            data.index = i;            
            data.worldPos = new Vector2(UnityEngine.Random.Range(0f, SimulationManager._MapSize), UnityEngine.Random.Range(0f, SimulationManager._MapSize));

            data.radius = UnityEngine.Random.Range(minParticleSize, maxParticleSize);
            data.biomass = 0f; // data.radius * data.radius * Mathf.PI * settingsRef.algaeParticleNutrientDensity;
            data.isActive = 0f;
            data.isDecaying = 0f;
            data.age = UnityEngine.Random.Range(1f, 2f);
            data.colorA = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            data.colorB = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            data.health = 0f;
            data.typeID = 0f;
	        data.rootedness = UnityEngine.Random.Range(0f, 1f);
	        data.radiusAxisOne = UnityEngine.Random.Range(0f, 1f);
	        data.radiusAxisTwo = UnityEngine.Random.Range(0f, 1f);
	        data.leafDensity = UnityEngine.Random.Range(0f, 1f);
	        data.angleInc = UnityEngine.Random.Range(0f, 1f);
	        data.leafLength = UnityEngine.Random.Range(0f, 1f);
	        data.leafWidth = UnityEngine.Random.Range(0f, 1f);
	        data.leafRoundness = UnityEngine.Random.Range(0f, 1f);
	        data.brushTypeX = 0;

            plantParticlesArray[i] = data;
        }
        //Debug.Log("Fill Initial Particle Array Data CPU: " + (Time.realtimeSinceStartup - startTime).ToString());

        plantParticlesCBuffer.SetData(plantParticlesArray);
        plantParticlesCBufferSwap.SetData(plantParticlesArray);
        //Debug.Log("Set Data GPU: " + (Time.realtimeSinceStartup - startTime).ToString());
        /*
        plantParticlesNearestCritters1024 = new RenderTexture(numPlantParticles, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        plantParticlesNearestCritters1024.wrapMode = TextureWrapMode.Clamp;
        plantParticlesNearestCritters1024.filterMode = FilterMode.Point;
        plantParticlesNearestCritters1024.enableRandomWrite = true;        
        plantParticlesNearestCritters1024.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    
        //Debug.Log("Create RT 1024: " + (Time.realtimeSinceStartup - startTime).ToString());
        plantParticlesNearestCritters32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        plantParticlesNearestCritters32.wrapMode = TextureWrapMode.Clamp;
        plantParticlesNearestCritters32.filterMode = FilterMode.Point;
        plantParticlesNearestCritters32.enableRandomWrite = true;        
        plantParticlesNearestCritters32.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***   
        //Debug.Log("Create RT 32: " + (Time.realtimeSinceStartup - startTime).ToString());
        plantParticlesNearestCritters1 = new RenderTexture(1, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        plantParticlesNearestCritters1.wrapMode = TextureWrapMode.Clamp;
        plantParticlesNearestCritters1.filterMode = FilterMode.Point;
        plantParticlesNearestCritters1.enableRandomWrite = true;        
        plantParticlesNearestCritters1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***
        */
        //Debug.Log("Pre Buffer Creation: " + (Time.realtimeSinceStartup - startTime).ToString());
        closestPlantParticlesDataArray = new PlantParticleData[numAgents];
        closestPlantParticlesDataCBuffer = new ComputeBuffer(numAgents, GetPlantParticleDataSize());
        
        //closestParticlesToCursorDataArray = new PlantParticleData[32];
        //closestParticlesToCursorDataCBuffer = new ComputeBuffer(32, GetPlantParticleDataSize());
        cursorClosestParticleDataCBuffer = new ComputeBuffer(2, GetPlantParticleDataSize());  // 0 = selected, 1 = closest to cursor
        cursorParticleDataArray = new PlantParticleData[2];
        cursorDistances1024 = new ComputeBuffer(1024, sizeof(float) * 4);
        //cursorDistances32 = new ComputeBuffer(32, sizeof(float) * 1);
        //cursorDistances1 = new ComputeBuffer(1, sizeof(float) * 1);

        plantParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        plantParticlesEatAmountsArray = new float[numAgents];

        plantParticleMeasurementTotalsData = new PlantParticleData[1];
        plantParticlesMeasure32 = new ComputeBuffer(32, GetPlantParticleDataSize());
        plantParticlesMeasure1 = new ComputeBuffer(1, GetPlantParticleDataSize());
        //Debug.Log("End: " + (Time.realtimeSinceStartup - startTime).ToString());
       
        critterNearestPlants32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        critterNearestPlants32.wrapMode = TextureWrapMode.Clamp;
        critterNearestPlants32.filterMode = FilterMode.Point;
        critterNearestPlants32.enableRandomWrite = true;        
        critterNearestPlants32.Create(); 

        closestPlantIndexCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        closestPlantIndexArray = new Vector4[numAgents];
                
        plantSlotGenomeCurrent = new WorldLayerPlantGenome();        
        //algaeSlotGenomeCurrent.algaeRepData = algaeParticlesArray[0];
        plantSlotGenomeCurrent.displayColorPri = UnityEngine.Random.ColorHSV();
        plantSlotGenomeCurrent.displayColorPri.a = 1f;
        plantSlotGenomeCurrent.name = "Plant Particles!";
        float minRate = 0.5f;
        float maxRate = 1.75f;
        plantSlotGenomeCurrent.growthRate = Mathf.Lerp(minRate, maxRate, UnityEngine.Random.Range(0f, 1f));
        plantSlotGenomeCurrent.textDescriptionMutation = "Growth Rate: " + plantSlotGenomeCurrent.growthRate.ToString("F2"); // + ", GrowthEfficiency: " + plantSlotGenomeCurrent.plantGrowthEfficiency.ToString("F2") + ", IntakeRate: " + plantSlotGenomeCurrent.plantIntakeRate.ToString("F4");
        
        plantSlotGenomeMutations = new WorldLayerPlantGenome[4];

        plantSlotGenomeCurrent.plantRepData = plantParticlesArray[0];
        
        plantParticlesRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetPlantParticleDataSize());
        PlantParticleData[] plantParticlesRepresentativeGenomeArray = new PlantParticleData[1];
        Debug.Log(" BADFHADFHADF" + plantSlotGenomeCurrent.name);
        plantParticlesRepresentativeGenomeArray[0] = plantParticlesArray[0]; // plantSlotGenomeCurrent.plantRepData;
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

        /*
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
        */
        //theRenderKing.fluidRenderMat.SetTexture("_DebugTex", nutrientMapRT1);
        
    }
    public void InitializeDecomposersGrid() {
        
        decomposerSlotGenomeCurrent = new WorldLayerDecomposerGenome();
        
        decomposerSlotGenomeCurrent.name = "Decomposers";
        decomposerSlotGenomeMutations = new WorldLayerDecomposerGenome[4];

        float minIntakeRate = tempSharedIntakeRate * 0.95f;
        float maxIntakeRate = tempSharedIntakeRate * 1.042f;
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        decomposerSlotGenomeCurrent.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        decomposerSlotGenomeCurrent.growthEfficiency = UnityEngine.Random.Range(0.9f, 1.1f);
        decomposerSlotGenomeCurrent.textDescriptionMutation = "Metabolic Rate: " + (decomposerSlotGenomeCurrent.metabolicRate * 100f).ToString("F2"); 

        GenerateWorldLayerDecomposersGenomeMutationOptions();

        


        int kernelCSInitResourceGrid = computeShaderResourceGrid.FindKernel("CSInitResourceGrid"); 
        //computeShaderResourceGrid.SetTexture(kernelCSUpdateAlgaeGrid, "rdRead", rdRT1);
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)resourceGridTexResolution);
        //computeShaderResourceGrid.SetTexture(kernelCSInitRD, "rdWrite", rdRT1);
        computeShaderResourceGrid.SetTexture(kernelCSInitResourceGrid, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.Dispatch(kernelCSInitResourceGrid, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);

    }

    private void WorldLayerDecomposerGenomeStuff(ref WorldLayerDecomposerGenome genome, float mutationSizeLerp) {
                
        float minIntakeRate = tempSharedIntakeRate * 0.1f;
        float maxIntakeRate = tempSharedIntakeRate * 4f;
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        genome.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        genome.metabolicRate = Mathf.Lerp(decomposerSlotGenomeCurrent.metabolicRate, genome.metabolicRate, mutationSizeLerp);

        genome.name = decomposerSlotGenomeCurrent.name;
        //genome.textDescriptionMutation = "Metabolic Rate: " + (genome.metabolicRate * 100f).ToString("F2"); // + ", GrowthEfficiency: " + mutatedGenome.algaeGrowthEfficiency.ToString("F2") + ", IntakeRate: " + mutatedGenome.algaeIntakeRate.ToString("F4");
            
        genome.growthEfficiency = UnityEngine.Random.Range(0.1f, 2f);
        
    }

    public void GenerateWorldLayerAlgaeGridGenomeMutationOptions() {
        for(int j = 0; j < algaeSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            jLerp = jLerp * jLerp;
            jLerp = 0.3f;
            WorldLayerAlgaeGenome mutatedGenome = new WorldLayerAlgaeGenome();

            Color randColorPri = UnityEngine.Random.ColorHSV();
            Color randColorSec = UnityEngine.Random.ColorHSV();
            
            Color mutatedColorPri = Color.Lerp(algaeSlotGenomeCurrent.displayColorPri, randColorPri, jLerp);
            Color mutatedColorSec = Color.Lerp(algaeSlotGenomeCurrent.displayColorSec, randColorSec, jLerp);
            mutatedGenome.displayColorPri = mutatedColorPri;
            mutatedGenome.displayColorSec = mutatedColorSec;
            if(UnityEngine.Random.Range(0f, 1f) < jLerp * 1f) {
                mutatedGenome.patternRowID = UnityEngine.Random.Range(0, 8);
                mutatedGenome.patternColumnID = UnityEngine.Random.Range(0, 8);
            }
            else {
                mutatedGenome.patternRowID = algaeSlotGenomeCurrent.patternRowID;
                mutatedGenome.patternColumnID = algaeSlotGenomeCurrent.patternColumnID;
            }
            
            mutatedGenome.patternThreshold = Mathf.Lerp(algaeSlotGenomeCurrent.patternThreshold, UnityEngine.Random.Range(0f, 1f), jLerp);
            
            float minIntakeRate = tempSharedIntakeRate * 0.1f;
            float maxIntakeRate = tempSharedIntakeRate * 10f; // init around 1?
            float lnLerp = UnityEngine.Random.Range(0f, 1f);
            lnLerp *= lnLerp;
            mutatedGenome.metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
            mutatedGenome.metabolicRate = Mathf.Lerp(algaeSlotGenomeCurrent.metabolicRate, mutatedGenome.metabolicRate, jLerp);
            mutatedGenome.growthEfficiency = UnityEngine.Random.Range(0.1f, 2f);
            
            algaeSlotGenomeMutations[j] = mutatedGenome;
        }
    }
    public void GenerateWorldLayerPlantParticleGenomeMutationOptions() {
        for(int j = 0; j < plantSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            jLerp = jLerp * jLerp;
            WorldLayerPlantGenome mutatedGenome = new WorldLayerPlantGenome();
            //Color randColor = UnityEngine.Random.ColorHSV();
            //Color col = plantSlotGenomeCurrent.displayColor;
            //col = Color.Lerp(col, randColor, jLerp);
            //mutatedGenome.displayColor = col;

            float minRate = 0.05f;
            float maxRate = 5f;
            mutatedGenome.growthRate = Mathf.Lerp(minRate, maxRate, UnityEngine.Random.Range(0f, 1f));
            mutatedGenome.growthRate = Mathf.Lerp(plantSlotGenomeCurrent.growthRate, mutatedGenome.growthRate, jLerp);

            mutatedGenome.name = plantSlotGenomeCurrent.name;
            mutatedGenome.textDescriptionMutation = "Growth Rate: " + mutatedGenome.growthRate.ToString("F2");

            mutatedGenome.plantRepData = plantSlotGenomeCurrent.plantRepData;
            mutatedGenome.plantRepData.colorA = Vector3.Lerp(mutatedGenome.plantRepData.colorA, UnityEngine.Random.insideUnitSphere, 0.1f);
            
            plantSlotGenomeMutations[j] = mutatedGenome;
            Vector3 hue = mutatedGenome.plantRepData.colorA;
            plantSlotGenomeMutations[j].displayColorPri = new Color(hue.x, hue.y, hue.z);
            hue = mutatedGenome.plantRepData.colorB;
            plantSlotGenomeMutations[j].displayColorSec = new Color(hue.x, hue.y, hue.z);
            
        }
    }

    public void GenerateWorldLayerDecomposersGenomeMutationOptions() {
        for(int j = 0; j < decomposerSlotGenomeMutations.Length; j++) {
            float jLerp = Mathf.Clamp01((float)j / 3f + 0.015f); 
            
            jLerp = jLerp * jLerp;
            jLerp = 0.3f;
            WorldLayerDecomposerGenome mutatedGenome = new WorldLayerDecomposerGenome();
            Color randColorPri = UnityEngine.Random.ColorHSV();
            Color randColorSec = UnityEngine.Random.ColorHSV();
            
            Color mutatedColorPri = Color.Lerp(decomposerSlotGenomeCurrent.displayColorPri, randColorPri, jLerp);
            Color mutatedColorSec = Color.Lerp(decomposerSlotGenomeCurrent.displayColorSec, randColorSec, jLerp);
            mutatedGenome.displayColorPri = mutatedColorPri;
            mutatedGenome.displayColorSec = mutatedColorSec;
            if(UnityEngine.Random.Range(0f, 1f) < jLerp * 1f) {
                mutatedGenome.patternRowID = UnityEngine.Random.Range(0, 8);
                mutatedGenome.patternColumnID = UnityEngine.Random.Range(0, 8);
            }
            else {
                mutatedGenome.patternRowID = decomposerSlotGenomeCurrent.patternRowID;
                mutatedGenome.patternColumnID = decomposerSlotGenomeCurrent.patternColumnID;
            }
            
            mutatedGenome.patternThreshold = Mathf.Lerp(decomposerSlotGenomeCurrent.patternThreshold, UnityEngine.Random.Range(0f, 1f), jLerp);
            WorldLayerDecomposerGenomeStuff(ref mutatedGenome, jLerp);

            mutatedGenome.name = decomposerSlotGenomeCurrent.name;
            
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
        computeShaderPlantParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "foodParticlesRead", plantParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSSimulateAlgaeParticles, "foodParticlesWrite", plantParticlesCBufferSwap);
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "velocityRead", fluidManagerRef._VelocityPressureDivergenceMain);        
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "altitudeRead", renderKingRef.baronVonTerrain.terrainHeightDataRT);
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_SpawnDensityMap", renderKingRef.spiritBrushRT);
        computeShaderPlantParticles.SetTexture(kernelCSSimulateAlgaeParticles, "_ResourceGridRead", resourceGridRT1);
        computeShaderPlantParticles.SetFloat("_MapSize", SimulationManager._MapSize);   
        computeShaderPlantParticles.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);   
        computeShaderPlantParticles.SetFloat("_SpiritBrushPosNeg", renderKingRef.spiritBrushPosNeg);            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderPlantParticles.SetFloat("_Time", Time.realtimeSinceStartup);
        //float randRoll = UnityEngine.Random.Range(0f, 1f);
        float brushF = 0f;
        if(renderKingRef.isSpiritBrushOn) {
            if(renderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.kingdomID == 1) {  // Plants kingdom selected
                //Debug.Log("brushF: " + brushF.ToString());
                if(renderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.tierID == 0) {  // Algae selected
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
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", plantParticlesCBufferSwap);
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", plantParticlesCBuffer);        
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
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesRead", plantParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesWrite", plantParticlesCBufferSwap);
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesEatAmountsCBuffer", plantParticlesEatAmountsCBuffer);        
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "closestParticlesDataCBuffer", closestPlantParticlesDataCBuffer);  
        computeShaderPlantParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "_ClosestPlantIndexCBuffer", closestPlantIndexCBuffer);
        //"_ClosestPlantIndexCBuffer", closestPlantIndexCBuffer); 
        computeShaderPlantParticles.Dispatch(kernelCSEatSelectedFoodParticles, simStateDataRef.critterSimDataCBuffer.count, 1, 1);

        plantParticlesEatAmountsCBuffer.GetData(plantParticlesEatAmountsArray);

        float totalFoodEaten = 0f;
        for(int i = 0; i < plantParticlesEatAmountsCBuffer.count; i++) {
            totalFoodEaten += plantParticlesEatAmountsArray[i];
        }
        // Copy/Swap Food PArticle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderPlantParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", plantParticlesCBufferSwap);
        computeShaderPlantParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", plantParticlesCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
    }
    public void FindClosestPlantParticleToCritters(SimulationStateData simStateDataRef) {  // need to send info on closest particle pos/dir/amt back to CPU also
        
        int kernelCSNewMeasureDistancesInit = computeShaderPlantParticles.FindKernel("CSNewMeasureDistancesInit");
        int kernelCSNewMeasureDistancesMainA = computeShaderPlantParticles.FindKernel("CSNewMeasureDistancesMainA");

        computeShaderPlantParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterSimDataCBuffer", simStateDataRef.critterSimDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "critterInitDataCBuffer", simStateDataRef.critterInitDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSNewMeasureDistancesInit, "foodParticlesRead", plantParticlesCBuffer);        
        computeShaderPlantParticles.SetTexture(kernelCSNewMeasureDistancesInit, "_CritterToPlantDistancesWrite", critterNearestPlants32);

        // ************************************************* Set RenderTexture BEFORE DISPATCH?
        computeShaderPlantParticles.SetTexture(kernelCSNewMeasureDistancesMainA, "_CritterToPlantDistancesRead", critterNearestPlants32); 

        computeShaderPlantParticles.Dispatch(kernelCSNewMeasureDistancesInit, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        //PlantParticleData[] debugArray = new PlantParticleData[plantParticlesMeasure32.count];
        //plantParticlesMeasure32.GetData(debugArray);
        //Debug.Log("Ugh: " + debugArray[0].index.ToString() + ", " + debugArray[0].worldPos.ToString());
        
        
        computeShaderPlantParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "foodParticlesRead", plantParticlesCBuffer);      
        computeShaderPlantParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "_ClosestPlantIndexCBuffer", closestPlantIndexCBuffer);  // Float4 buffer
        computeShaderPlantParticles.SetBuffer(kernelCSNewMeasureDistancesMainA, "closestParticlesDataCBuffer", closestPlantParticlesDataCBuffer);  // PlantParticleData buffer
        computeShaderPlantParticles.Dispatch(kernelCSNewMeasureDistancesMainA, 1, simStateDataRef.critterSimDataCBuffer.count, 1);
        
        closestPlantParticlesDataCBuffer.GetData(closestPlantParticlesDataArray);
        closestPlantIndexCBuffer.GetData(closestPlantIndexArray);

        /*Debug.Log("FindClosestPlantParticleToCritters[1] " + simStateDataRef.critterSimDataArray[1].worldPos.ToString() +  ", " +
                    closestPlantParticlesDataArray[1].worldPos.ToString() + ", id: " +
                    closestPlantParticlesDataArray[1].index.ToString());*/
    }
    // Keep these two pipelines separate at first while try to debug::::
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
    public void FindClosestPlantParticleToCursor(float xCoord, float yCoord) {
        
        int kernelCSMeasureInitCursorDistances = computeShaderPlantParticles.FindKernel("CSMeasureInitCursorDistances");        
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCursorDistances, "foodParticlesRead", plantParticlesCBuffer);  
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCursorDistances, "cursorDistancesWrite", cursorDistances1024);
        computeShaderPlantParticles.SetFloat("_MouseCoordX", xCoord);
        computeShaderPlantParticles.SetFloat("_MouseCoordY", yCoord);
        computeShaderPlantParticles.Dispatch(kernelCSMeasureInitCursorDistances, plantParticlesCBuffer.count / 32, 1, 1);

        Vector4[] cursorDistanceArray1024 = new Vector4[1024];
        cursorDistances1024.GetData(cursorDistanceArray1024);


        //Vector4[] dstBuffer = new Vector4[25];

        // Manual Sort!
        Vector4[] swapBuffer = cursorDistanceArray1024; // new Vector4[cursorDistanceArray1024.Length / 2];
        swapBuffer = ReduceDistancesArray(cursorDistanceArray1024);        
        for(int tierID = 0; tierID < 9; tierID++) {            
            Vector4[] writeBuffer = ReduceDistancesArray(swapBuffer);            
            swapBuffer = new Vector4[writeBuffer.Length];
            for(int x = 0; x < writeBuffer.Length; x++) {
                swapBuffer[x] = writeBuffer[x];
            }
        }
        // assuming size of 4 because this is a gross hack
        /*float nearestDist = swapBuffer[0].y;
        int nearestIndex = 0;
        if(swapBuffer[0].y < swapBuffer[1].y) {

        }
        else {
            nearestDist = swapBuffer[1].y;
            nearestIndex = 1;
        }
        if(swapBuffer[2].y < nearestDist) {
            nearestDist = swapBuffer[2].y;
            nearestIndex = 2;
        }
        if(swapBuffer[3].y < nearestDist) {
            nearestDist = swapBuffer[3].y;
            nearestIndex = 3;
        }*/
        tempClosestPlantParticleIndexAndPos.x = swapBuffer[0].x;
        
        //int bufferLength = swapBuffer.Length;

        // Now Fetch the actual particleData:::::
        int kernelCSFetchParticleByID = computeShaderPlantParticles.FindKernel("CSFetchParticleByID");
        computeShaderPlantParticles.SetBuffer(kernelCSFetchParticleByID, "selectedPlantParticleDataCBuffer", cursorClosestParticleDataCBuffer);
        
        computeShaderPlantParticles.SetInt("_SelectedParticleID", selectedPlantParticleIndex);   
        computeShaderPlantParticles.SetInt("_ClosestParticleID", Mathf.RoundToInt(tempClosestPlantParticleIndexAndPos.x)); 
        computeShaderPlantParticles.SetBuffer(kernelCSFetchParticleByID, "foodParticlesRead", plantParticlesCBuffer);        
        computeShaderPlantParticles.Dispatch(kernelCSFetchParticleByID, 1, 1, 1);

        cursorClosestParticleDataCBuffer.GetData(cursorParticleDataArray);

        //string txt = "nearestIndex = " + cursorClosestParticleDataArray[0].index + " (" + cursorClosestParticleDataArray[0].age +  ")   " + (cursorClosestParticleDataArray[0].biomass * 1000f).ToString("F0"); // "lngth: " + swapBuffer.Length.ToString() + ",  " + swapBuffer[0].ToString() + "   " + swapBuffer[1].ToString() + "   " + swapBuffer[swapBuffer.Length - 2].ToString() + "   " + swapBuffer[swapBuffer.Length - 1].ToString();
                                                                                                                                                                                                                //"nearestIndex = " + nearestIndex.ToString() + " (" + nearestDist.ToString() + ")  size: " + swapBuffer.Length.ToString() + ", " + swapBuffer[0].ToString();

        // TEMPP!!!!!
        //closestPlantParticlesDataArray[0] = cursorClosestParticleDataArray[0];
        closestPlantParticleData = cursorParticleDataArray[1];
        selectedPlantParticleData = cursorParticleDataArray[0];

        //Debug.Log(xCoord.ToString() + ", " + yCoord.ToString() + " *** " + swapBuffer[0].x + ", c: " + closestPlantParticleData.index.ToString() + ", s: " + selectedPlantParticleData.index.ToString());


        /*
        // Reduce from 1024 --> 32 particles per cursor:  
        // need to set buffers early?
        int kernelCSReduceCursorDistances32 = computeShaderPlantParticles.FindKernel("CSReduceCursorDistances32");
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances32, "cursorDistancesRead", cursorDistances1024);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances32, "cursorDistancesWrite", cursorDistances32);
         
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances32, "foodParticlesRead", plantParticlesCBuffer);        
        computeShaderPlantParticles.Dispatch(kernelCSReduceCursorDistances32, 32, 1, 1);

        Vector4[] cursorDistanceArray32 = new Vector4[32];
        cursorDistances32.GetData(cursorDistanceArray32);
        txt += "   32[0] = " + cursorDistanceArray32[0].ToString();
        */
        /*
        // Reduce from 32 --> 1 particles per cursor:
        int kernelCSReduceCursorDistances1 = computeShaderPlantParticles.FindKernel("CSReduceCursorDistances32");
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances1, "cursorDistancesRead", cursorDistances32);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances1, "cursorDistancesWrite", cursorDistances1); // Needed???
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances1, "foodParticlesRead", plantParticlesCBuffer);        
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCursorDistances1, "closestParticlesDataCBuffer", cursorClosestParticleDataCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSReduceCursorDistances1, 1, 1, 1);

        cursorClosestParticleDataCBuffer.GetData(cursorClosestParticleDataArray);
        */
        //Debug.Log(txt);
        
        /*
        float closestSqrDist = 50000f;
        int closestIndex = -1;
        Vector2 cursorPos = new Vector2(simStateDataRef.critterSimDataArray[0].worldPos.x, simStateDataRef.critterSimDataArray[0].worldPos.y);
        for(int i = 0; i < 32; i++) {
            float dist = (closestParticlesToCursorDataArray[i].worldPos - cursorPos).sqrMagnitude;

            if(dist < closestSqrDist) {
                closestSqrDist = dist;
                closestIndex = i;
            }
        }
        tempClosestPlantParticleIndexAndPos = new Vector3((float)closestParticlesToCursorDataArray[closestIndex].index, closestParticlesToCursorDataArray[closestIndex].worldPos.x, closestParticlesToCursorDataArray[closestIndex].worldPos.y);
        // *************************************************************************************

        // Reduce from 32 --> 1 particles per critter:
        int kernelCSReduceCritterDistances1 = computeShaderPlantParticles.FindKernel("CSReduceCritterDistances1");
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances1, "critterDistancesRead", plantParticlesNearestCritters32);
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances1, "critterDistancesWrite", plantParticlesNearestCritters1);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances1, "foodParticlesRead", plantParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances1, "closestParticlesDataCBuffer", closestPlantParticlesDataCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSReduceCritterDistances1, 1, simStateDataRef.critterSimDataCBuffer.count, 1);

        closestPlantParticlesDataCBuffer.GetData(closestPlantParticlesDataArray);

        ComputeBuffer cursorCritterSimDataCBuffer = new ComputeBuffer(1, SimulationStateData.GetCritterSimDataSize());

        int kernelCSMeasureInitCritterDistances = computeShaderPlantParticles.FindKernel("CSMeasureInitCritterDistances");
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterSimDataCBuffer", cursorCritterSimDataCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "foodParticlesRead", plantParticlesCBuffer);        
        computeShaderPlantParticles.SetTexture(kernelCSMeasureInitCritterDistances, "foodParticlesNearestCrittersRT", plantParticlesNearestCritters1024);        
        computeShaderPlantParticles.Dispatch(kernelCSMeasureInitCritterDistances, plantParticlesCBuffer.count / 1024, cursorCritterSimDataCBuffer.count, 1);
        
        // Reduce from 1024 --> 32 particles per critter:
        int kernelCSReduceCritterDistances32 = computeShaderPlantParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", plantParticlesNearestCritters1024);
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", plantParticlesNearestCritters32);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", plantParticlesCBuffer);        
        computeShaderPlantParticles.Dispatch(kernelCSReduceCritterDistances32, 32, cursorCritterSimDataCBuffer.count, 1);
        // Stop at 32? download all to CPU?

        // Reduce from 32 --> 1 particles per critter:
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", plantParticlesNearestCritters32);
        computeShaderPlantParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", plantParticlesNearestCritters1);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", plantParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestPlantParticlesDataCBuffer);
        computeShaderPlantParticles.Dispatch(kernelCSReduceCritterDistances32, 1, cursorCritterSimDataCBuffer.count, 1);

        closestPlantParticlesDataCBuffer.GetData(closestPlantParticlesDataArray);
        */
    }
    public void MeasureTotalPlantParticlesAmount() {  // 

        
        int kernelCSMeasureTotalFoodParticlesAmount = computeShaderPlantParticles.FindKernel("CSMeasureTotalFoodParticlesAmount");
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", plantParticlesCBuffer);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", plantParticlesMeasure32);         
        // DISPATCH !!!
        computeShaderPlantParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 32, 1, 1);

        PlantParticleData[] debugArray = new PlantParticleData[plantParticlesMeasure32.count];
        plantParticlesMeasure32.GetData(debugArray);
        //Debug.Log("Ugh: " + debugArray[0].index.ToString() + ", " + debugArray[0].worldPos.ToString());

        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", plantParticlesMeasure32);
        computeShaderPlantParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", plantParticlesMeasure1);
        computeShaderPlantParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 1, 1, 1);
        
        plantParticlesMeasure1.GetData(plantParticleMeasurementTotalsData);
        resourceManagerRef.curGlobalPlantParticles = plantParticleMeasurementTotalsData[0].biomass;
        resourceManagerRef.oxygenProducedByPlantParticlesLastFrame = plantParticleMeasurementTotalsData[0].oxygenProduced;
        resourceManagerRef.wasteProducedByPlantParticlesLastFrame = plantParticleMeasurementTotalsData[0].wasteProduced;
        resourceManagerRef.nutrientsUsedByPlantParticlesLastFrame = plantParticleMeasurementTotalsData[0].nutrientsUsed;

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
    /*public void GetResourceGridValuesAtMouthPositions(SimulationStateData simStateDataRef) {
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
    }*/
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
        
        curGlobalNutrientGridValues = outputValuesArray[0];

        outputValuesCBuffer.Release();

        /*Debug.Log("Resource Totals:\nNutrients: " + outputValuesArray[0].x.ToString() + 
                                  "\nWaste: " + outputValuesArray[0].y.ToString() + 
                                  "\nDecomposers: " + outputValuesArray[0].z.ToString() + 
                                  "\nAlgae: " + outputValuesArray[0].w.ToString());
*/
        //return outputValuesArray[0].x;
    }    
    /*public void AddResourcesAtCoords(Vector4 amount, float x, float y) {  // 0-1 normalized map coords
        
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
        
    }*/
    /*public void RemoveEatenResourceGrid(int numAgents, SimulationStateData simStateDataRef) { // **** WILL BE MODIFIED!!! *****
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
    }*/
       
    
    public void SimResourceGrid(ref EnvironmentFluidManager fluidManagerRef, ref BaronVonTerrain baronTerrainRef, ref TheRenderKing theRenderKingRef) {
        int kernelCSSimRD = computeShaderResourceGrid.FindKernel("CSSimResourceGrid"); 
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "_AltitudeTex", baronTerrainRef.terrainHeightRT0);        
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)resourceGridTexResolution);
        computeShaderResourceGrid.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "_ResourceGridRead", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSSimRD, "_ResourceGridWrite", resourceGridRT2);
        computeShaderResourceGrid.Dispatch(kernelCSSimRD, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);
        // write into 2
        int kernelCSAdvectRD = computeShaderResourceGrid.FindKernel("CSAdvectResourceGrid");
        computeShaderResourceGrid.SetFloat("_TextureResolution", (float)resourceGridTexResolution);        
        computeShaderResourceGrid.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderResourceGrid.SetFloat("_DeltaTime", fluidManagerRef.deltaTime);
        computeShaderResourceGrid.SetFloat("_InvGridScale", fluidManagerRef.invGridScale);
        computeShaderResourceGrid.SetFloat("_MapSize", SimulationManager._MapSize);
        float brushDecomposersOn = 0f;  // eventually make this more elegant during next refactor ***
        float brushAlgaeOn = 0f;
        float brushMineralsOn = 0f;
        float brushIntensityMult = 1f;
        if(isBrushActive) {  // Set from uiManager

            if (theRenderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.kingdomID == 0) {
                brushDecomposersOn = 1f;
                brushIntensityMult = 0.2f;
            }
            else if (theRenderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.kingdomID == 1) {
                if (theRenderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.tierID == 0) {
                    brushAlgaeOn = 1f;
                    
                    brushIntensityMult = 0.2f;
                }
                else {
                    //brushPlantsOn = 1f;
                }
            }
            else if (theRenderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.kingdomID == 4) {
                if (theRenderKingRef.simManager.uiManager.brushesUI.selectedEssenceSlot.slotID == 0) {  // MINERALS
                    brushMineralsOn = 1f;  
                    brushIntensityMult = 0.1f;
                    Debug.Log("// minerals brush on!");
                }
            }
        }
        computeShaderResourceGrid.SetFloat("_SpiritBrushIntensity", brushIntensityMult); // *** INVESTIGATE THIS -- not used/needed?
        computeShaderResourceGrid.SetFloat("_IsSpiritBrushDecomposersOn", brushDecomposersOn);
        computeShaderResourceGrid.SetFloat("_IsSpiritBrushAlgaeOn", brushAlgaeOn);
        computeShaderResourceGrid.SetFloat("_IsSpiritBrushMineralsOn", brushMineralsOn);
        computeShaderResourceGrid.SetFloat("_SpiritBrushPosNeg", theRenderKingRef.spiritBrushPosNeg);
        //computeShaderResourceGrid.SetFloat("_RD_FeedRate", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.feedRate);
        //computeShaderResourceGrid.SetFloat("_RD_KillRate", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.killRate);            
        //computeShaderResourceGrid.SetFloat("_RD_Scale", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.scale);
        //computeShaderResourceGrid.SetFloat("_RD_Rate", theRenderKingRef.simManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate);

        //float algaeUpkeep = algaeSlotGenomeCurrent.
        computeShaderResourceGrid.SetFloat("_AlgaeUpkeep", algaeSlotGenomeCurrent.metabolicRate * 0.5f); // decomposerSlotGenomeCurrent.metabolicRate);  // *********** SHARING WITH DECOMPOSERS!!!! *****
        computeShaderResourceGrid.SetFloat("_AlgaeMaxIntakeRate", algaeSlotGenomeCurrent.metabolicRate * 0.1f);
        computeShaderResourceGrid.SetFloat("_AlgaeGrowthEfficiency", 6.55f); // empirical

        computeShaderResourceGrid.SetFloat("_DecomposerUpkeep", decomposerSlotGenomeCurrent.metabolicRate * 0.6f); // value from empirical tinkering // decomposerSlotGenomeCurrent.metabolicRate);
        computeShaderResourceGrid.SetFloat("_DecomposerMaxIntakeRate", decomposerSlotGenomeCurrent.metabolicRate);
        computeShaderResourceGrid.SetFloat("_DecomposerEnergyGenerationEfficiency", 1f);
        
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceGridRead", resourceGridRT2);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceGridWrite", resourceGridRT1);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_SpiritBrushTex", theRenderKingRef.spiritBrushRT);
        computeShaderResourceGrid.SetTexture(kernelCSAdvectRD, "_ResourceSimTransferRead", resourceSimTransferRT);
        computeShaderResourceGrid.Dispatch(kernelCSAdvectRD, resourceGridTexResolution / 32, resourceGridTexResolution / 32, 1);
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
        /*
        if (plantParticlesNearestCritters1 != null) {
            plantParticlesNearestCritters1.Release();
            plantParticlesNearestCritters32.Release();
            plantParticlesNearestCritters1024.Release();
        }*/        
        if(plantParticlesCBuffer != null) {
            plantParticlesCBuffer.Release();
        }  
        if(plantParticlesCBufferSwap != null) {
            plantParticlesCBufferSwap.Release();
        } 
        if(closestPlantParticlesDataCBuffer != null) {
            closestPlantParticlesDataCBuffer.Release();
        }
        /*if(closestParticlesToCursorDataCBuffer != null) {
            closestParticlesToCursorDataCBuffer.Release();
        }*/
        if(cursorDistances1024 != null) {
            cursorDistances1024.Release();
        }
        //if(cursorDistances32 != null) {
        //    cursorDistances32.Release();
        //} 
        //if(cursorDistances1 != null) {
        //    cursorDistances1.Release();
        //}

        if(critterNearestPlants32 != null) {
            critterNearestPlants32.Release();
        }
        if(closestPlantIndexCBuffer != null) {
            closestPlantIndexCBuffer.Release();
        }
        if(cursorClosestParticleDataCBuffer != null) {
            cursorClosestParticleDataCBuffer.Release();
        } 

        if(plantParticlesEatAmountsCBuffer != null) {
            plantParticlesEatAmountsCBuffer.Release();
        }
        if(plantParticlesMeasure32 != null) {
            plantParticlesMeasure32.Release();
        }
        if(plantParticlesMeasure1 != null) {
            plantParticlesMeasure1.Release();
        }    
        
        if(plantParticlesRepresentativeGenomeCBuffer != null) {
            plantParticlesRepresentativeGenomeCBuffer.Release();
        }
    }
}
