using UnityEngine;
using UnityEngine.Rendering;

public class BaronVonTerrain : RenderBaron 
{
    SimulationManager simulation => SimulationManager.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    UIManager ui => UIManager.instance;
    
    TheRenderKing renderKing => TheRenderKing.instance;
    RenderTexture spiritBrushRT => renderKing.spiritBrushRT;
    CommandBuffer cmdBufferMain => renderKing.cmdBufferMain;
    Vector3 sunDirection => renderKing.sunDirection;
    Vector3 mainRenderCamPosition => renderKing.mainRenderCam.transform.position;
    Vector4 mainRenderCamPosition4 => new Vector4(mainRenderCamPosition.x, mainRenderCamPosition.y, mainRenderCamPosition.z, 0f);
    float minimumFogDensity => renderKing.minimumFogDensity;
    Texture2D skyTexture => renderKing.skyTexture;
    BaronVonWater water => renderKing.baronVonWater;
    RenderTexture waterSurfaceDataRT0 => water.waterSurfaceDataRT0;
    RenderTexture waterSurfaceDataRT1 => water.waterSurfaceDataRT1;
    float camDistanceNormalized => water.camDistNormalized;
    VegetationManager vegetation => simulation.vegetationManager;
    WorldLayerDecomposerGenome decomposerSlotGenomeCurrent => vegetation.decomposerSlotGenomeCurrent;

    public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderTerrainGeneration;
    
    //public Material frameBufferStrokeDisplayMat;
    public Material groundStrokesLrgDisplayMat;
    public Material groundStrokesMedDisplayMat;
    public Material groundStrokesSmlDisplayMat;
    public Material decomposerBitsDisplayMat;
    public Material decomposerBitsShadowDisplayMat;
    public Material wasteBitsDisplayMat;
    
    public Color terrainColor0;
    public Color terrainColor1;
    public Color terrainColor2;
    public Color terrainColor3;
    public WorldLayerTerrainGenome bedrockSlotGenomeCurrent;
    public WorldLayerTerrainGenome[] bedrockSlotGenomeMutations;
    public WorldLayerTerrainGenome stoneSlotGenomeCurrent;
    public WorldLayerTerrainGenome[] stoneSlotGenomeMutations;
    public WorldLayerTerrainGenome pebblesSlotGenomeCurrent;
    public WorldLayerTerrainGenome[] pebblesSlotGenomeMutations;
    public WorldLayerTerrainGenome sandSlotGenomeCurrent;
    public WorldLayerTerrainGenome[] sandSlotGenomeMutations;

    //public Material frameBufferStrokeDisplayMat;
    private int terrainHeightMapResolution = 256;
    private int terrainColorResolution = 256;

    public float _WorldRadius = 2.0f;

    //public GameObject terrainGO;
    public Material terrainObstaclesHeightMaskMat;
    public Texture2D terrainInitHeightMap;
    public RenderTexture terrainHeightRT0;
    public RenderTexture terrainHeightRT1;
    public RenderTexture terrainHeightDataRT;
    public RenderTexture terrainColorRT0;
    public RenderTexture terrainColorRT1; // *** needed?

    private int simTextureBetaResolution = 128;
    //  x = dust, y = _, z = _, w = map active mask
    public RenderTexture simTextureBetaRT0;
    public RenderTexture simTextureBetaRT1;
    

    public struct TriangleIndexData {
        public int v1;
        public int v2;
        public int v3;
    }
    
    public Mesh quadMesh;
    public Mesh terrainMesh;

    private ComputeBuffer terrainVertexCBuffer;
    private ComputeBuffer terrainUVCBuffer;
    private ComputeBuffer terrainNormalCBuffer;
    private ComputeBuffer terrainColorCBuffer;
    private ComputeBuffer terrainTriangleCBuffer; 

    private ComputeBuffer quadVerticesCBuffer;  // quad mesh

    //private int numFrameBufferStrokesPerDimension = 512;
    private ComputeBuffer frameBufferStrokesCBuffer; // combine!!! ***
    private int numGroundStrokesStone = 32;
    private int numGroundStrokesPebbles = 64;
    private int numGroundStrokesSand = 128;
    public ComputeBuffer terrainStoneStrokesCBuffer;
    public ComputeBuffer terrainPebbleStrokesCBuffer;
    public ComputeBuffer terrainSandStrokesCBuffer;

    private int numDecomposerBits = 1024 * 4;
    public ComputeBuffer decomposerBitsCBuffer;
    private int numWasteBits = 1024 * 4;
    public ComputeBuffer wasteBitsCBuffer;

    public Vector4 spawnBoundsCameraDetails;

    //public float maxAltitude = 24f;

    /*public struct TerrainSlotGenome {
        public Color color;
        public float elevationChange;
    }*/

    // background terrain
    public struct EnvironmentStrokeData 
    { 
        public Vector3 worldPos;
		public Vector2 scale;
		public Vector2 heading;
		public int brushType;
        public float isActive;        
    }

    public struct GroundBitsData
    {
        public int index;
        public Vector3 worldPos;
        public Vector2 heading;
        public Vector2 localScale;
        public float age;
        public float speed;
        public float noiseVal;
        public float isActive;
        public int brushType;
    }

    public override void Initialize() 
    {
        // Bedrock data
        int numMutations = 4;
        bedrockSlotGenomeCurrent = new WorldLayerTerrainGenome();
        bedrockSlotGenomeCurrent.color = terrainColor0;
        bedrockSlotGenomeCurrent.name = "Bedrock";
        bedrockSlotGenomeCurrent.textDescriptionMutation = "Elevation Gain: " + " ??";
        bedrockSlotGenomeMutations = new WorldLayerTerrainGenome[numMutations];   
                     
        // stones:
        stoneSlotGenomeCurrent = new WorldLayerTerrainGenome();
        stoneSlotGenomeCurrent.color = terrainColor1;
        stoneSlotGenomeCurrent.name = "Stone";
        stoneSlotGenomeMutations = new WorldLayerTerrainGenome[numMutations];
        
        // pebbles:
        pebblesSlotGenomeCurrent = new WorldLayerTerrainGenome();
        pebblesSlotGenomeCurrent.color = terrainColor2;
        pebblesSlotGenomeCurrent.name = "Pebbles";
        pebblesSlotGenomeMutations = new WorldLayerTerrainGenome[numMutations];
        
        // sand:
        sandSlotGenomeCurrent = new WorldLayerTerrainGenome();
        sandSlotGenomeCurrent.color = terrainColor3;
        sandSlotGenomeCurrent.name = "Sand";
        sandSlotGenomeMutations = new WorldLayerTerrainGenome[numMutations];
        
        for(int i = 0; i < 4; i++) { // not needed?
            GenerateTerrainSlotGenomeMutationOptions(i);
        }

        terrainHeightRT0 = new RenderTexture(terrainHeightMapResolution, terrainHeightMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        terrainHeightRT0.wrapMode = TextureWrapMode.Clamp;
        terrainHeightRT0.enableRandomWrite = true;
        terrainHeightRT0.Create();

        int CSInitTerrainMapsKernelID = computeShaderTerrainGeneration.FindKernel("CSInitTerrainMaps");
        computeShaderTerrainGeneration.SetTexture(CSInitTerrainMapsKernelID, "AltitudeRead", terrainInitHeightMap);   // Read-Only 
        computeShaderTerrainGeneration.SetTexture(CSInitTerrainMapsKernelID, "AltitudeWrite", terrainHeightRT0);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", terrainHeightMapResolution);
        computeShaderTerrainGeneration.Dispatch(CSInitTerrainMapsKernelID, terrainHeightMapResolution / 32, terrainHeightMapResolution / 32, 1);

        /*
        Graphics.Blit(terrainInitHeightMap, terrainHeightRT0); //, testInitTerrainDataBlitMat);
        testInitTerrainDataBlitMat.SetPass(0);
        testInitTerrainDataBlitMat.SetTexture("_MainTex", terrainInitHeightMap);
        Graphics.Blit(terrainInitHeightMap, terrainHeightRT0, testInitTerrainDataBlitMat);
        */

        //computeShaderTerrainGeneration.Set

        terrainHeightRT1 = new RenderTexture(terrainHeightMapResolution, terrainHeightMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        terrainHeightRT1.wrapMode = TextureWrapMode.Clamp;
        terrainHeightRT1.enableRandomWrite = true;
        terrainHeightRT1.Create();

        terrainHeightDataRT = terrainHeightRT0;

        terrainColorRT0 = new RenderTexture(terrainColorResolution, terrainColorResolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        terrainColorRT0.wrapMode = TextureWrapMode.Clamp;
        terrainColorRT0.enableRandomWrite = true;
        terrainColorRT0.Create();

        terrainColorRT1 = new RenderTexture(terrainColorResolution, terrainColorResolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        terrainColorRT1.wrapMode = TextureWrapMode.Clamp;
        terrainColorRT1.enableRandomWrite = true;
        terrainColorRT1.Create();

        // dump extra data in here:::
        simTextureBetaRT0 = new RenderTexture(simTextureBetaResolution, simTextureBetaResolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        simTextureBetaRT0.wrapMode = TextureWrapMode.Clamp;
        simTextureBetaRT0.enableRandomWrite = true;
        simTextureBetaRT0.Create();

        simTextureBetaRT1 = new RenderTexture(simTextureBetaResolution, simTextureBetaResolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        simTextureBetaRT1.wrapMode = TextureWrapMode.Clamp;
        simTextureBetaRT1.enableRandomWrite = true;
        simTextureBetaRT1.Create();

        InitializeBuffers();        
        InitializeMaterials();
        RebuildTerrainMesh();
        //AlignFrameBufferStrokesToTerrain();
        AlignGroundStrokesToTerrain();
    }

    /*public void ApplyMutation(int id) {
        bedrockSlotGenomeCurrent = bedrockSlotGenomeMutations[id];
        GenerateTerrainSlotGenomeMutationOptions(id);
    }*/
    
    public void GenerateTerrainSlotGenomeMutationOptions(int slotID) {
        float mutationSize = 1f;
        // *** Invert code to bring for() loop to the outside? might be more efficient ***
       // *** Just save current slot object first then use that as single ref?
        if(slotID == 0) {
            for(int i = 0; i < bedrockSlotGenomeMutations.Length; i++) {
                float iLerp = Mathf.Clamp01((float)i / 3f + 0.015f); // ((float)i + 0.2f / (bedrockSlotGenomeMutations.Length - 1f));
                iLerp = iLerp * iLerp;
                WorldLayerTerrainGenome mutatedGenome = new WorldLayerTerrainGenome();
                Color randColor = Random.ColorHSV();
                float randAlpha = Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(bedrockSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                
                mutatedGenome.elevationChange = Mathf.Lerp(bedrockSlotGenomeCurrent.elevationChange, Random.Range(0f, 1f), iLerp);

                mutatedGenome.name = bedrockSlotGenomeCurrent.name;
                mutatedGenome.textDescriptionMutation = "Mutation Amt: " + (iLerp * 100f).ToString("F0") + "%";
                bedrockSlotGenomeMutations[i] = mutatedGenome;
            }
        }
        else if(slotID == 1) {
            for(int i = 0; i < stoneSlotGenomeMutations.Length; i++) {
                float iLerp = Mathf.Clamp01((float)i / 3f + 0.015f); 
                iLerp = iLerp * iLerp;
                WorldLayerTerrainGenome mutatedGenome = new WorldLayerTerrainGenome();
                Color randColor = Random.ColorHSV();
                float randAlpha = Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(stoneSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                mutatedGenome.elevationChange = Mathf.Lerp(stoneSlotGenomeCurrent.elevationChange, Random.Range(0f, 1f), iLerp);

                mutatedGenome.name = stoneSlotGenomeCurrent.name;
                mutatedGenome.textDescriptionMutation = "Mutation Amt: " + (iLerp * 100f).ToString("F0") + "%";
                stoneSlotGenomeMutations[i] = mutatedGenome;
            }
        }
        else if(slotID == 2) {
            for(int i = 0; i < pebblesSlotGenomeMutations.Length; i++) {
                float iLerp = Mathf.Clamp01((float)i / 3f + 0.015f); 
                iLerp = iLerp * iLerp;
                WorldLayerTerrainGenome mutatedGenome = new WorldLayerTerrainGenome();
                Color randColor = Random.ColorHSV();
                float randAlpha = Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(pebblesSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                mutatedGenome.elevationChange = Mathf.Lerp(pebblesSlotGenomeCurrent.elevationChange, Random.Range(0f, 1f), iLerp);

                mutatedGenome.name = pebblesSlotGenomeCurrent.name;
                mutatedGenome.textDescriptionMutation = "Mutation Amt: " + (iLerp * 100f).ToString("F0") + "%";
                pebblesSlotGenomeMutations[i] = mutatedGenome;
            }
        }
        else if(slotID == 3) {
            for(int i = 0; i < sandSlotGenomeMutations.Length; i++) {
                float iLerp = Mathf.Clamp01((float)i / 3f + 0.015f); 
                iLerp = iLerp * iLerp;
                WorldLayerTerrainGenome mutatedGenome = new WorldLayerTerrainGenome();
                Color randColor = Random.ColorHSV();
                float randAlpha = Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(sandSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                mutatedGenome.elevationChange = Mathf.Lerp(sandSlotGenomeCurrent.elevationChange, Random.Range(0f, 1f), iLerp);

                mutatedGenome.name = sandSlotGenomeCurrent.name;
                mutatedGenome.textDescriptionMutation = "Mutation Amt: " + (iLerp * 100f).ToString("F0") + "%";
                sandSlotGenomeMutations[i] = mutatedGenome;
            }
        }
    }

    private void InitializeBuffers() {
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering     
        //InitializeFrameBufferStrokesBuffer();
        InitializeGroundStrokeBuffers();
        InitializeDecomposerBits();
        InitializeWasteBits();
    }

    private void InitializeDecomposerBits()
    {
        decomposerBitsCBuffer = new ComputeBuffer(numDecomposerBits, sizeof(float) * 11 + sizeof(int) * 2);
        GroundBitsData[] decomposerBitsArray = new GroundBitsData[decomposerBitsCBuffer.count];
        //float boundsLrg = 256f;
        for (int x = 0; x < numDecomposerBits; x++)
        {
            decomposerBitsArray[x].index = x;
            //int index = y * numGroundStrokesLrg + x;
            float xPos = 0f; // (float)x / (float)(numGroundBits - 1) * boundsLrg;
            float yPos = 0f; // xPos; // (1f - (float)y / (float)(numGroundStrokesLrg - 1)) * boundsLrg;
            Vector2 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f) * 0.0f) * 16f;
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            decomposerBitsArray[x].worldPos = pos;
            decomposerBitsArray[x].heading = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            decomposerBitsArray[x].localScale = new Vector2(Random.Range(0.4f, 1.5f), Random.Range(1.0f, 2.5f)) * Random.Range(1.5f, 4.2f); // Y is forward, along stroke
            decomposerBitsArray[x].age = Random.Range(1f, 2f);
            decomposerBitsArray[x].speed = 0f;
            decomposerBitsArray[x].noiseVal = 1f;
            decomposerBitsArray[x].brushType = Random.Range(0, 16);
        }
        decomposerBitsCBuffer.SetData(decomposerBitsArray);
    }
    
    private void InitializeWasteBits()
    {
        wasteBitsCBuffer = new ComputeBuffer(numWasteBits, sizeof(float) * 11 + sizeof(int) * 2);
        GroundBitsData[] wasteBitsArray = new GroundBitsData[wasteBitsCBuffer.count];
        //float boundsLrg = 256f;
        for (int x = 0; x < numWasteBits; x++)
        {
            wasteBitsArray[x].index = x;
            //int index = y * numcarpetStrokesLrg + x;
            //float xPos = (float)x / (float)(numWasteBits - 1) * boundsLrg;
            //float yPos = xPos; // (1f - (float)y / (float)(numcarpetStrokesLrg - 1)) * boundsLrg;
            ////Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 0.0f) * 16f;
            //Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            wasteBitsArray[x].worldPos = Vector3.zero;
            wasteBitsArray[x].heading = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            wasteBitsArray[x].localScale = new Vector2(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f)) * Random.Range(0.5f, 1f); // Y is forward, along stroke
            wasteBitsArray[x].age = Random.Range(0f, 1f);
            wasteBitsArray[x].speed = 0f;
            wasteBitsArray[x].noiseVal = 1f;
            wasteBitsArray[x].brushType = Random.Range(0, 32);
            wasteBitsArray[x].isActive = 0f;
        }
        wasteBitsCBuffer.SetData(wasteBitsArray);
    }
    
    private void InitializeGroundStrokeBuffers() 
    {
        // LARGE ::::::
        int totalNumStoneStrokes = numGroundStrokesStone * numGroundStrokesStone;
        int totalNumPebbleStrokes = numGroundStrokesPebbles * numGroundStrokesPebbles;
        int totalNumSandStrokes = numGroundStrokesSand * numGroundStrokesSand;
        //int totalNumTerrainStrokes = totalNumStoneStrokes + totalNumPebbleStrokes + totalNumSandStrokes;
        //int baseIndex = 0;

        terrainStoneStrokesCBuffer = new ComputeBuffer(totalNumStoneStrokes, sizeof(float) * 8 + sizeof(int));
        terrainPebbleStrokesCBuffer = new ComputeBuffer(totalNumPebbleStrokes, sizeof(float) * 8 + sizeof(int));
        terrainSandStrokesCBuffer = new ComputeBuffer(totalNumSandStrokes, sizeof(float) * 8 + sizeof(int));
        //groundStrokesLrgCBuffer = new ComputeBuffer(numGroundStrokesLrg * numGroundStrokesLrg, sizeof(float) * 7 + sizeof(int));
        EnvironmentStrokeData[] terrainStoneStrokesArray = new EnvironmentStrokeData[totalNumStoneStrokes];
        EnvironmentStrokeData[] terrainPebbleStrokesArray = new EnvironmentStrokeData[totalNumPebbleStrokes];
        EnvironmentStrokeData[] terrainSandStrokesArray = new EnvironmentStrokeData[totalNumSandStrokes];
        //float boundsLrg = 256f;
        
        for(int x = 0; x < numGroundStrokesStone; x++) {
            for(int y = 0; y < numGroundStrokesStone; y++) {
                int index = y * numGroundStrokesStone + x;
                float xPos = (float)x / (float)(numGroundStrokesStone - 1) * SimulationManager._MapSize;
                float yPos = (1f - (float)y / (float)(numGroundStrokesStone - 1)) * SimulationManager._MapSize;
                Vector2 offset = Random.insideUnitCircle * 31.29f; // new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 1.0f) * 70.6f;
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                terrainStoneStrokesArray[index].worldPos = pos;
                terrainStoneStrokesArray[index].scale = new Vector2(Random.Range(1f, 1f), Random.Range(1f, 1f)) * 7.737f; // Y is forward, along stroke
                terrainStoneStrokesArray[index].heading = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                terrainStoneStrokesArray[index].brushType = Random.Range(0,16);
                terrainStoneStrokesArray[index].isActive = 0f;
            }
        }
        
        //baseIndex = totalNumStoneStrokes;
        for(int x = 0; x < numGroundStrokesPebbles; x++) {
            for(int y = 0; y < numGroundStrokesPebbles; y++) {
                //int index = baseIndex + y * numGroundStrokesPebbles + x;
                int index = y * numGroundStrokesPebbles + x;
                float xPos = (float)x / (float)(numGroundStrokesPebbles - 1) * SimulationManager._MapSize;
                float yPos = (1f - (float)y / (float)(numGroundStrokesPebbles - 1)) * SimulationManager._MapSize;
                Vector2 offset = Random.insideUnitCircle * 3.41729f; // new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 1.0f) * 5.6f;
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                terrainPebbleStrokesArray[index].worldPos = pos;
                terrainPebbleStrokesArray[index].scale = new Vector2(Random.Range(1f, 1f), Random.Range(1f, 1.7f)) * 0.085f; // Y is forward, along stroke
                terrainPebbleStrokesArray[index].heading = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                terrainPebbleStrokesArray[index].brushType = Random.Range(0,16);
                terrainPebbleStrokesArray[index].isActive = 0f;
            }
        }
        
        //baseIndex = totalNumStoneStrokes + totalNumPebbleStrokes;
        for(int x = 0; x < numGroundStrokesSand; x++) {
            for(int y = 0; y < numGroundStrokesSand; y++) {
                //int index = baseIndex + y * numGroundStrokesSand + x;
                int index = y * numGroundStrokesSand + x;
                float xPos = (float)x / (float)(numGroundStrokesSand - 1) * SimulationManager._MapSize;
                float yPos = (1f - (float)y / (float)(numGroundStrokesSand - 1)) * SimulationManager._MapSize;
                Vector2 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f) * 1.0f) * 0.76f;
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                terrainSandStrokesArray[index].worldPos = pos;
                terrainSandStrokesArray[index].scale = new Vector2(Random.Range(0.6f, 1.4f), Random.Range(1.4f, 2.1f)) * Random.Range(0.4f, 1.80f) * 0.59531875f; // Y is forward, along stroke
                terrainSandStrokesArray[index].heading = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                terrainSandStrokesArray[index].brushType = Random.Range(0,16);
                terrainSandStrokesArray[index].isActive = 0f;
            }
        }

        terrainStoneStrokesCBuffer.SetData(terrainStoneStrokesArray);
        terrainPebbleStrokesCBuffer.SetData(terrainPebbleStrokesArray);
        terrainSandStrokesCBuffer.SetData(terrainSandStrokesArray);

        // MEDIUM :::::
        //groundStrokesMedCBuffer = new ComputeBuffer(numGroundStrokesMed * numGroundStrokesMed, sizeof(float) * 7 + sizeof(int));
        //TerrainSimpleBrushData[] groundStrokesMedArray = new TerrainSimpleBrushData[groundStrokesMedCBuffer.count];
        //float boundsMed = 256f;
        
        //groundStrokesMedCBuffer.SetData(groundStrokesMedArray);


        // SMALL :::::
        //groundStrokesSmlCBuffer = new ComputeBuffer(numGroundStrokesSml * numGroundStrokesSml, sizeof(float) * 7 + sizeof(int));
        //TerrainSimpleBrushData[] groundStrokesSmlArray = new TerrainSimpleBrushData[groundStrokesSmlCBuffer.count];
        
        //groundStrokesSmlCBuffer.SetData(groundStrokesSmlArray);
    }

    private void InitializeQuadMeshBuffer() {
        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });
    }

    /*private void InitializeFrameBufferStrokesBuffer() {
        frameBufferStrokesCBuffer = new ComputeBuffer(numFrameBufferStrokesPerDimension * numFrameBufferStrokesPerDimension, sizeof(float) * 7 + sizeof(int));
        TerrainSimpleBrushData[] frameBufferStrokesArray = new TerrainSimpleBrushData[frameBufferStrokesCBuffer.count];
        float frameBufferStrokesBounds = 256f;
        for(int x = 0; x < numFrameBufferStrokesPerDimension; x++) {
            for(int y = 0; y < numFrameBufferStrokesPerDimension; y++) {
                int index = x * numFrameBufferStrokesPerDimension + y;
                float xPos = (float)x / (float)(numFrameBufferStrokesPerDimension - 1) * frameBufferStrokesBounds;
                float yPos = (float)y / (float)(numFrameBufferStrokesPerDimension - 1) * frameBufferStrokesBounds;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                frameBufferStrokesArray[index].worldPos = pos;
                frameBufferStrokesArray[index].scale = new Vector2(1.15f, 2.20f); // Y is forward, along stroke
                frameBufferStrokesArray[index].heading = new Vector2(1f, 0f);
                frameBufferStrokesArray[index].brushType = UnityEngine.Random.Range(0,4);
            }
        }
        frameBufferStrokesCBuffer.SetData(frameBufferStrokesArray);
    }*/

    private void InitializeMaterials() {
        //groundStrokesLrgDisplayMat.SetPass(0);
        //groundStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        //groundStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //groundStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesLrgCBuffer);   
        
        groundStrokesMedDisplayMat.SetPass(0);
        groundStrokesMedDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesMedDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesMedDisplayMat.SetBuffer("environmentStrokesCBuffer", terrainPebbleStrokesCBuffer);    

        //groundStrokesSmlDisplayMat.SetPass(0);
        //groundStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        //groundStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //groundStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesSmlCBuffer);

        decomposerBitsDisplayMat.SetPass(0);
        decomposerBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        decomposerBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        decomposerBitsDisplayMat.SetBuffer("groundBitsCBuffer", decomposerBitsCBuffer);

        decomposerBitsShadowDisplayMat.SetPass(0);
        decomposerBitsShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        decomposerBitsShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        decomposerBitsShadowDisplayMat.SetBuffer("groundBitsCBuffer", decomposerBitsCBuffer);
        
        wasteBitsDisplayMat.SetPass(0);
        wasteBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        wasteBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        wasteBitsDisplayMat.SetBuffer("groundBitsCBuffer", wasteBitsCBuffer);
        
        //groundDryLandDisplayMat.SetPass(0);
        //groundDryLandDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        //groundDryLandDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //groundDryLandDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesSmlCBuffer);  
    }

    public void RebuildTerrainMesh() 
    {
        //Debug.Log("InitializeTerrain!");
        int meshResolution = 128;
        float mapSize = SimulationManager._MapSize;

        if(!terrainInitHeightMap) {
            Debug.LogError("ERROR! No terrainGO or heightmap referenced! Plug them into inspector!!!", gameObject);
            return;
        }
        
        if (!computeShaderTerrainGeneration) {
            Debug.LogError("NO COMPUTE SHADER SET!!!!");
        }

        terrainVertexCBuffer?.Release();
        terrainVertexCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 3);
        terrainUVCBuffer?.Release();
        terrainUVCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 2);
        terrainNormalCBuffer?.Release();
        terrainNormalCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 3);
        terrainColorCBuffer?.Release();
        terrainColorCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 4);
        terrainTriangleCBuffer?.Release();
        terrainTriangleCBuffer = new ComputeBuffer((meshResolution - 1) * (meshResolution - 1) * 2, sizeof(int) * 3);
                        
        // Set Shader properties so it knows where and what to build::::
        //computeShaderTerrainGeneration.SetInt("_MeshResolution", meshResolution);
        computeShaderTerrainGeneration.SetInt("_MeshResolution", meshResolution);
        computeShaderTerrainGeneration.SetVector("_QuadBounds", new Vector4(0f, mapSize, 0f, mapSize)); //new Vector4(-mapSize * 0.5f, mapSize * 1.5f, -mapSize * 0.5f, mapSize * 1.5f));
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetVector("_HeightRange", new Vector4(-SimulationManager._MaxAltitude, 0f, 0f, 0f));
        
        // Creates Actual Mesh data by reading from existing main Height Texture!!!!::::::
        int generateMeshDataKernelID = computeShaderTerrainGeneration.FindKernel("CSGenerateMeshData");
        computeShaderTerrainGeneration.SetTexture(generateMeshDataKernelID, "AltitudeRead", terrainHeightDataRT);   // Read-Only 

        computeShaderTerrainGeneration.SetBuffer(generateMeshDataKernelID, "terrainVertexCBuffer", terrainVertexCBuffer);
        computeShaderTerrainGeneration.SetBuffer(generateMeshDataKernelID, "terrainUVCBuffer", terrainUVCBuffer);
        computeShaderTerrainGeneration.SetBuffer(generateMeshDataKernelID, "terrainNormalCBuffer", terrainNormalCBuffer);
        computeShaderTerrainGeneration.SetBuffer(generateMeshDataKernelID, "terrainColorCBuffer", terrainColorCBuffer);        
    
        // Generate list of Triangle Indices (grouped into 3 per triangle):::
        int triangleIndicesKernelID = computeShaderTerrainGeneration.FindKernel("CSGenerateTriangleIndices");        
        computeShaderTerrainGeneration.SetBuffer(triangleIndicesKernelID, "terrainTriangleCBuffer", terrainTriangleCBuffer);
    
        // GENERATE MESH DATA!!!!
        computeShaderTerrainGeneration.Dispatch(generateMeshDataKernelID, meshResolution, 1, meshResolution);
        computeShaderTerrainGeneration.Dispatch(triangleIndicesKernelID, meshResolution - 1, 1, meshResolution - 1);

        Mesh mesh = GenerateTerrainMesh();

        // CLEANUP!!
        terrainVertexCBuffer.Release();
        terrainUVCBuffer.Release();
        terrainNormalCBuffer.Release();
        terrainColorCBuffer.Release();
        terrainTriangleCBuffer.Release();

        //terrainGO.GetComponent<MeshFilter>().sharedMesh = mesh;
        terrainMesh = mesh;
    }
    
    private Mesh GenerateTerrainMesh() {
        if(terrainMesh == null) {
            terrainMesh = new Mesh();
        }

        TriangleIndexData[] triangleIndexDataArray = new TriangleIndexData[terrainTriangleCBuffer.count];
        terrainTriangleCBuffer.GetData(triangleIndexDataArray);
        int[] tris = new int[triangleIndexDataArray.Length * 3];
        for (int i = 0; i < triangleIndexDataArray.Length; i++) {
            tris[i * 3] = triangleIndexDataArray[i].v1;
            tris[i * 3 + 1] = triangleIndexDataArray[i].v2;
            tris[i * 3 + 2] = triangleIndexDataArray[i].v3;
        }

        Vector3[] vertices = new Vector3[terrainVertexCBuffer.count];
        Vector2[] uvs = new Vector2[terrainUVCBuffer.count];
        Vector3[] normals = new Vector3[terrainNormalCBuffer.count];
        Color[] colors = new Color[terrainColorCBuffer.count];
        terrainVertexCBuffer.GetData(vertices);
        terrainUVCBuffer.GetData(uvs);
        terrainNormalCBuffer.GetData(normals);
        terrainColorCBuffer.GetData(colors);

        //for(int i = 0; i < 32; i++) {
        //    Debug.Log("Vertex " + i.ToString() + ": " + vertices[i].ToString());
        //}

        // CONSTRUCT ACTUAL MESH
        terrainMesh.vertices = vertices;
        terrainMesh.uv = uvs;
        terrainMesh.triangles = tris;
        terrainMesh.normals = normals;
        terrainMesh.colors = colors;
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();

        return terrainMesh;
    }
    
    /*private void AlignFrameBufferStrokesToTerrain() {
        int kernelCSAlignFrameBufferStrokes = computeShaderBrushStrokes.FindKernel("CSAlignFrameBufferStrokes");
        computeShaderBrushStrokes.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderBrushStrokes.SetTexture(kernelCSAlignFrameBufferStrokes, "terrainHeightTex", terrainHeightMap);
        computeShaderBrushStrokes.SetBuffer(kernelCSAlignFrameBufferStrokes, "terrainFrameBufferStrokesCBuffer", frameBufferStrokesCBuffer);        
        computeShaderBrushStrokes.Dispatch(kernelCSAlignFrameBufferStrokes, frameBufferStrokesCBuffer.count, 1, 1);
    }*/
    
    // ****  OPTIMIZE!!!! *******
    public void AlignGroundStrokesToTerrain() 
    {  
        int kernelCSAlignGroundStrokesLrg = computeShaderTerrainGeneration.FindKernel("CSUpdateGroundStrokes");
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_BrushAlignment", 1f);        
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", terrainHeightMapResolution);
        computeShaderTerrainGeneration.SetTexture(kernelCSAlignGroundStrokesLrg, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetBuffer(kernelCSAlignGroundStrokesLrg, "terrainFrameBufferStrokesCBuffer", terrainStoneStrokesCBuffer);        
        computeShaderTerrainGeneration.Dispatch(kernelCSAlignGroundStrokesLrg, terrainStoneStrokesCBuffer.count, 1, 1);
        
        int kernelCSAlignGroundStrokesMed = computeShaderTerrainGeneration.FindKernel("CSUpdateGroundStrokes");
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", terrainHeightMapResolution);
        //computeShaderTerrainGeneration.SetFloat("_GroundStrokeTerrainAlign", 1f);
        computeShaderTerrainGeneration.SetTexture(kernelCSAlignGroundStrokesMed, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_BrushAlignment", 1f);
        computeShaderTerrainGeneration.SetBuffer(kernelCSAlignGroundStrokesMed, "terrainFrameBufferStrokesCBuffer", terrainPebbleStrokesCBuffer);        
        computeShaderTerrainGeneration.Dispatch(kernelCSAlignGroundStrokesMed, terrainPebbleStrokesCBuffer.count / 256, 1, 1);
        
        int kernelCSAlignGroundStrokesSml = computeShaderTerrainGeneration.FindKernel("CSUpdateGroundStrokes");
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);        
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", terrainHeightMapResolution);
        computeShaderTerrainGeneration.SetTexture(kernelCSAlignGroundStrokesMed, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_BrushAlignment", 1f);
        computeShaderTerrainGeneration.SetBuffer(kernelCSAlignGroundStrokesSml, "terrainFrameBufferStrokesCBuffer", terrainSandStrokesCBuffer);        
        computeShaderTerrainGeneration.Dispatch(kernelCSAlignGroundStrokesSml, terrainSandStrokesCBuffer.count, 1, 1);   
    }

    public override void Tick(RenderTexture maskTex) {
        SimTerrainBits(maskTex);
    }

    public void IncrementWorldRadius(float radius) {
        //Debug.Log("WorldRadius = " + _WorldRadius);
        _WorldRadius += radius;
    }

    private void SimTerrainBits(RenderTexture mask)
    {
        int kernelSimGroundBits = computeShaderTerrainGeneration.FindKernel("CSSimDecomposerBitsData");
        computeShaderTerrainGeneration.SetBuffer(kernelSimGroundBits, "groundBitsCBuffer", decomposerBitsCBuffer);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "decomposersRead", mask);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", terrainHeightMapResolution);
        computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        
        computeShaderTerrainGeneration.Dispatch(kernelSimGroundBits, decomposerBitsCBuffer.count / 1024, 1, 1);

        int kernelSimWasteBits = computeShaderTerrainGeneration.FindKernel("CSSimCarpetBitsData");
        computeShaderTerrainGeneration.SetBuffer(kernelSimWasteBits, "groundBitsCBuffer", wasteBitsCBuffer);
        computeShaderTerrainGeneration.SetTexture(kernelSimWasteBits, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", terrainHeightMapResolution);
        computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        computeShaderTerrainGeneration.Dispatch(kernelSimWasteBits, wasteBitsCBuffer.count / 1024, 1, 1);
    }
    
    public void SimGroundBits()
    {
        int kernelSimGroundBits = computeShaderTerrainGeneration.FindKernel("CSSimDecomposerBitsData");
        computeShaderTerrainGeneration.SetBuffer(kernelSimGroundBits, "groundBitsCBuffer", decomposerBitsCBuffer);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "VelocityRead", fluidManager._VelocityPressureDivergenceMain);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "_ResourceGridRead", vegetation.resourceGridRT1);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        float spawnLerp = simulation.trophicLayersManager.GetLayerLerp(KnowledgeMapId.Decomposers, simulation.simAgeTimeSteps);
        float spawnRadius = Mathf.Lerp(1f, SimulationManager._MapSize, spawnLerp);
        Vector4 spawnPos = new Vector4(simulation.trophicLayersManager.decomposerOriginPos.x, simulation.trophicLayersManager.decomposerOriginPos.y, 0f, 0f);
        computeShaderTerrainGeneration.SetFloat("_SpawnRadius", spawnRadius);
        computeShaderTerrainGeneration.SetVector("_SpawnPos", spawnPos);
        //baronVonTerrain.computeShaderTerrainGeneration.SetFloat("_DecomposerDensityLerp", Mathf.Clamp01(simManager.simResourceManager.curGlobalDecomposers / 100f));
        computeShaderTerrainGeneration.Dispatch(kernelSimGroundBits, decomposerBitsCBuffer.count / 1024, 1, 1);
    }
    
    public void SimWasteBits()
    {
        int kernelSimWasteBits = computeShaderTerrainGeneration.FindKernel("CSSimWasteBitsData");
        computeShaderTerrainGeneration.SetBuffer(kernelSimWasteBits, "groundBitsCBuffer", wasteBitsCBuffer);
        computeShaderTerrainGeneration.SetTexture(kernelSimWasteBits, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetTexture(kernelSimWasteBits, "VelocityRead", fluidManager._VelocityPressureDivergenceMain);
        computeShaderTerrainGeneration.SetTexture(kernelSimWasteBits, "_ResourceGridRead", vegetation.resourceGridRT1);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        computeShaderTerrainGeneration.Dispatch(kernelSimWasteBits, wasteBitsCBuffer.count / 1024, 1, 1);
    }
    
    public void SetObjectDepths(ComputeBuffer objectDataInFluidCoords, ComputeBuffer depthValues)
    {
        var kernelGetObjectDepths = computeShaderTerrainGeneration.FindKernel("CSGetObjectDepths");
        computeShaderTerrainGeneration.SetBuffer(kernelGetObjectDepths, "ObjectPositionsCBuffer", objectDataInFluidCoords);
        computeShaderTerrainGeneration.SetBuffer(kernelGetObjectDepths, "DepthValuesCBuffer", depthValues);
        computeShaderTerrainGeneration.SetTexture(kernelGetObjectDepths, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", (float)terrainHeightDataRT.width);
        computeShaderTerrainGeneration.Dispatch(kernelGetObjectDepths, 1, 1, 1);        
    }
    
    public void SetObstacleHeights(CommandBuffer fluidObstacles)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.one * 0.5f * SimulationManager._MapSize, Quaternion.identity, Vector3.one * SimulationManager._MapSize);
        terrainObstaclesHeightMaskMat.SetTexture("_MainTex", terrainHeightDataRT);
        terrainObstaclesHeightMaskMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        terrainObstaclesHeightMaskMat.SetFloat("_TexResolution", (float)fluidManager.resolution);
        terrainObstaclesHeightMaskMat.SetFloat("_MapSize", SimulationManager._MapSize);
        fluidObstacles.DrawMesh(quadMesh, matrix, terrainObstaclesHeightMaskMat); // Masks out areas above the fluid "Sea Level"
    }
    
    public void SetGroundStrokes(KnowledgeMapId id)
    {
        var material = GetGroundMaterial(id);
        var buffer = GetComputeBuffer(id);
        
        material.SetPass(0);
        material.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        material.SetBuffer("environmentStrokesCBuffer", buffer);
        material.SetFloat("_MapSize", SimulationManager._MapSize);
        material.SetTexture("_AltitudeTex", terrainHeightDataRT);
        material.SetTexture("_WaterSurfaceTex", waterSurfaceDataRT1);
        material.SetTexture("_ResourceGridTex", vegetation.resourceGridRT1);
        material.SetTexture("_TerrainColorTex", terrainColorRT0);
        material.SetTexture("_SpiritBrushTex", spiritBrushRT);
        material.SetFloat("_Turbidity", simulation.fogAmount);
        material.SetFloat("_MinFog", minimumFogDensity);
        material.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        material.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        material.SetVector("_FogColor", simulation.fogColor);
        material.SetVector("_SunDir", sunDirection);
        material.SetVector("_WorldSpaceCameraPosition", mainRenderCamPosition4);
        material.SetVector("_Color0", stoneSlotGenomeCurrent.color);   // new Vector4(0.9f, 0.9f, 0.8f, 1f));
        material.SetVector("_Color1", pebblesSlotGenomeCurrent.color); // new Vector4(0.7f, 0.8f, 0.9f, 1f));
        material.SetVector("_Color2", sandSlotGenomeCurrent.color);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 6, buffer.count);
    }
    
    public void SetWasteMaterialProperties()
    {
        wasteBitsDisplayMat.SetPass(0);
        wasteBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        wasteBitsDisplayMat.SetBuffer("groundBitsCBuffer", wasteBitsCBuffer);
        wasteBitsDisplayMat.SetTexture("_AltitudeTex", terrainHeightDataRT);
        wasteBitsDisplayMat.SetTexture("_WaterSurfaceTex", waterSurfaceDataRT1);
        wasteBitsDisplayMat.SetTexture("_ResourceGridTex", vegetation.resourceGridRT1);
        wasteBitsDisplayMat.SetTexture("_TerrainColorTex", terrainColorRT0);
        wasteBitsDisplayMat.SetTexture("_SkyTex", skyTexture);
        wasteBitsDisplayMat.SetTexture("_SpiritBrushTex", spiritBrushRT);
        wasteBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        wasteBitsDisplayMat.SetFloat("_Turbidity", simulation.fogAmount);
        wasteBitsDisplayMat.SetFloat("_MinFog", minimumFogDensity);
        wasteBitsDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        wasteBitsDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        wasteBitsDisplayMat.SetVector("_SunDir", sunDirection);
        wasteBitsDisplayMat.SetVector("_WorldSpaceCameraPosition", mainRenderCamPosition4);
        wasteBitsDisplayMat.SetFloat("_CamDistNormalized", camDistanceNormalized);
        wasteBitsDisplayMat.SetVector("_FogColor", simulation.fogColor);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, wasteBitsDisplayMat, 0, MeshTopology.Triangles, 6, wasteBitsCBuffer.count);
    }
    
    public void SetDecomposerMaterialProperties()
    {
        decomposerBitsDisplayMat.SetPass(0);
        decomposerBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        decomposerBitsDisplayMat.SetBuffer("groundBitsCBuffer", decomposerBitsCBuffer);
        decomposerBitsDisplayMat.SetTexture("_AltitudeTex", terrainHeightDataRT);
        decomposerBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        decomposerBitsDisplayMat.SetTexture("_WaterSurfaceTex", waterSurfaceDataRT1);
        decomposerBitsDisplayMat.SetTexture("_ResourceGridTex", vegetation.resourceGridRT1);
        decomposerBitsDisplayMat.SetTexture("_TerrainColorTex", terrainColorRT0);
        decomposerBitsDisplayMat.SetTexture("_SpiritBrushTex", spiritBrushRT);
        decomposerBitsDisplayMat.SetTexture("_SkyTex", skyTexture);
        decomposerBitsDisplayMat.SetVector("_SunDir", sunDirection);
        decomposerBitsDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        decomposerBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        decomposerBitsDisplayMat.SetFloat("_Turbidity", simulation.fogAmount);
        decomposerBitsDisplayMat.SetFloat("_MinFog", minimumFogDensity);
        decomposerBitsDisplayMat.SetFloat("_CamDistNormalized", camDistanceNormalized);
        decomposerBitsDisplayMat.SetVector("_WorldSpaceCameraPosition", mainRenderCamPosition4);
        decomposerBitsDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        decomposerBitsDisplayMat.SetFloat("_Density", Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(simulation.simResourceManager.curGlobalDecomposers / 100f)));
        decomposerBitsDisplayMat.SetVector("_FogColor", simulation.fogColor);

        if (decomposerSlotGenomeCurrent != null) {
            decomposerBitsDisplayMat.SetFloat("_PatternThreshold", decomposerSlotGenomeCurrent.patternThreshold);
            decomposerBitsDisplayMat.SetColor("_TintPri", decomposerSlotGenomeCurrent.displayColorPri);
            decomposerBitsDisplayMat.SetColor("_TintSec", decomposerSlotGenomeCurrent.displayColorSec);
            decomposerBitsDisplayMat.SetInt("_PatternRow", decomposerSlotGenomeCurrent.patternRowID);
            decomposerBitsDisplayMat.SetInt("_PatternColumn", decomposerSlotGenomeCurrent.patternColumnID);
        }
        
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, decomposerBitsDisplayMat, 0, MeshTopology.Triangles, 6, decomposerBitsCBuffer.count);
    }

    // * WPP: Move to Terrain Layer if possible
    ComputeBuffer GetComputeBuffer(KnowledgeMapId id)
    {
        switch (id)
        {
            case KnowledgeMapId.Stone: return terrainStoneStrokesCBuffer;
            case KnowledgeMapId.Pebbles: return terrainPebbleStrokesCBuffer;
            case KnowledgeMapId.Sand: return terrainSandStrokesCBuffer;
            default: Debug.LogError($"Compute buffer not found {id}"); return null;
        }
    }
    
    // * WPP: move to Terrain Layer SO
    Material GetGroundMaterial(KnowledgeMapId id)
    {
        switch (id)
        {
            case KnowledgeMapId.Stone: return groundStrokesLrgDisplayMat;
            case KnowledgeMapId.Pebbles: return groundStrokesMedDisplayMat;
            case KnowledgeMapId.Sand: return groundStrokesSmlDisplayMat;
            default: Debug.LogError($"Material not found {id}"); return null;
        }        
    }

    // * WPP: empty method -> delete
    public override void RenderCommands(ref CommandBuffer cmdBuffer, int frameBufferID) {
        // Create RenderTargets:
        /*int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        cmdBuffer.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        cmdBuffer.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBuffer.SetRenderTarget(renderTarget);  // Set render Target
        cmdBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);  // clear -- needed???
        //cmdBufferMainRender.ClearRenderTarget(true, true, new Color(225f / 255f, 217f / 255f, 200f / 255f), 1.0f);  // clear -- needed???
          
        */
        // BACKGROUND STROKES:::
        //frameBufferStrokeDisplayMat.SetPass(0);
        //frameBufferStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //frameBufferStrokeDisplayMat.SetBuffer("frameBufferStrokesCBuffer", frameBufferStrokesCBuffer);    
       // frameBufferStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        // Use this technique for Environment Brushstrokes:
        //cmdBuffer.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        //cmdBuffer.DrawProcedural(Matrix4x4.identity, frameBufferStrokeDisplayMat, 0, MeshTopology.Triangles, 6, frameBufferStrokesCBuffer.count);
        
        
        /*
        // LARGE STROKES!!!!
        groundStrokesLrgDisplayMat.SetPass(0);
        groundStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesLrgCBuffer);    
        groundStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesLrgDisplayMat.SetTexture("_AltitudeTex", terrainHeightMap);
        cmdBuffer.SetGlobalTexture("_RenderedSceneRT", frameBufferID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBuffer.DrawProcedural(Matrix4x4.identity, groundStrokesLrgDisplayMat, 0, MeshTopology.Triangles, 6, groundStrokesLrgCBuffer.count);
        
        // MEDIUM STROKES!!!!
        groundStrokesMedDisplayMat.SetPass(0);
        groundStrokesMedDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesMedDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesMedCBuffer);    
        groundStrokesMedDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesMedDisplayMat.SetTexture("_AltitudeTex", terrainHeightMap);
        cmdBuffer.SetGlobalTexture("_RenderedSceneRT", frameBufferID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBuffer.DrawProcedural(Matrix4x4.identity, groundStrokesMedDisplayMat, 0, MeshTopology.Triangles, 6, groundStrokesMedCBuffer.count);
        
        // SMALL STROKES!!!!
        groundStrokesSmlDisplayMat.SetPass(0);
        groundStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesSmlCBuffer);    
        groundStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesSmlDisplayMat.SetTexture("_AltitudeTex", terrainHeightMap);
        cmdBuffer.SetGlobalTexture("_RenderedSceneRT", frameBufferID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBuffer.DrawProcedural(Matrix4x4.identity, groundStrokesSmlDisplayMat, 0, MeshTopology.Triangles, 6, groundStrokesSmlCBuffer.count);
        */
    }
    
    public void ClickTestTerrainUpdateMaps(bool on, float _intensity) 
    {
        float intensity = _intensity; // Mathf.Lerp(0.02f, 0.06f, baronVonWater.camDistNormalized) * 1.05f;
        float addSubtract = renderKing.spiritBrushPosNeg; 
        int texRes = terrainHeightRT0.width;
        int CSUpdateTerrainMapsBrushKernelID = computeShaderTerrainGeneration.FindKernel("CSUpdateTerrainMapsBrush");
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "AltitudeRead", terrainHeightRT0);   // Read-Only 
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "AltitudeWrite", terrainHeightRT1);
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "SpiritBrushRead", spiritBrushRT);
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "TerrainColorWrite", terrainColorRT0);
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "_WaterSurfaceTex", waterSurfaceDataRT0);
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "_ResourceGridRead", vegetation.resourceGridRT1);
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsBrushKernelID, "_SkyTex", skyTexture);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_TextureResolution", texRes);
        computeShaderTerrainGeneration.SetFloat("_BrushIntensity", intensity);
        computeShaderTerrainGeneration.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderTerrainGeneration.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderTerrainGeneration.SetFloat("_WorldRadius", _WorldRadius);
        computeShaderTerrainGeneration.SetVector("_WorldSpaceCameraPosition", mainRenderCamPosition4);
        computeShaderTerrainGeneration.SetInt("_ChannelID", ui.brushesUI.selectedBrushLinkedSpiritTerrainLayer); // simManager.trophicLayersManager.selectedTrophicSlotRef.slotID); // 
        
        // Not actively brushing this frame
        if (!on) {  
            addSubtract = 0f;
        }
        
        // LAYERS ARE SWIZZLED!!!!! **********************************************************************************************************************************
        computeShaderTerrainGeneration.SetVector("_Color3", bedrockSlotGenomeCurrent.color); // new Vector4(0.54f, 0.43f, 0.37f, 1f));
        computeShaderTerrainGeneration.SetVector("_Color0", stoneSlotGenomeCurrent.color); // new Vector4(0.9f, 0.9f, 0.8f, 1f));
        computeShaderTerrainGeneration.SetVector("_Color1", pebblesSlotGenomeCurrent.color); // new Vector4(0.7f, 0.8f, 0.9f, 1f));
        computeShaderTerrainGeneration.SetVector("_Color2", sandSlotGenomeCurrent.color); // new Vector4(0.7f, 0.6f, 0.3f, 1f));
        computeShaderTerrainGeneration.SetFloat("_AddSubtractSign", addSubtract);
        computeShaderTerrainGeneration.Dispatch(CSUpdateTerrainMapsBrushKernelID, texRes / 32, texRes / 32, 1);
        //----------------------------------------------------------------------------------------------------------------------------------------

        // Convolve! Erosion etc.
        int CSUpdateTerrainMapsConvolutionKernelID = computeShaderTerrainGeneration.FindKernel("CSUpdateTerrainMapsConvolution");
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsConvolutionKernelID, "AltitudeRead", terrainHeightRT1);   // Read-Only 
        computeShaderTerrainGeneration.SetTexture(CSUpdateTerrainMapsConvolutionKernelID, "AltitudeWrite", terrainHeightRT0);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.Dispatch(CSUpdateTerrainMapsConvolutionKernelID, texRes / 32, texRes / 32, 1);
        AlignGroundStrokesToTerrain();
    }

    public override void Cleanup() 
    {
        frameBufferStrokesCBuffer?.Release();
        quadVerticesCBuffer?.Release();
        terrainPebbleStrokesCBuffer?.Release();
        terrainStoneStrokesCBuffer?.Release();
        terrainSandStrokesCBuffer?.Release();
        decomposerBitsCBuffer?.Release();
        wasteBitsCBuffer?.Release();
    }
}
