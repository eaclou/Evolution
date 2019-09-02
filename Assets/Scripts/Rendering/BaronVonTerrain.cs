using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BaronVonTerrain : RenderBaron {

    public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderTerrainGeneration;

    public Material frameBufferStrokeDisplayMat;
    public Material groundStrokesLrgDisplayMat;
    public Material groundStrokesMedDisplayMat;
    public Material groundStrokesSmlDisplayMat;
    public Material decomposerBitsDisplayMat;
    public Material decomposerBitsShadowDisplayMat;
    public Material wasteBitsDisplayMat;
    public Material groundDryLandDisplayMat;

    public Material terrainBlitMat;
    public Material terrainSimulationBlitMat;
    public Material testInitTerrainDataBlitMat;
    public Material terrainGenerateColorBlitMat;

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

    public GameObject terrainGO;
    public Material terrainObstaclesHeightMaskMat;
    public Texture2D terrainInitHeightMap;
    public RenderTexture terrainHeightRT0;
    public RenderTexture terrainHeightRT1;
    public RenderTexture terrainHeightDataRT;
    public RenderTexture terrainColorRT0;
    public RenderTexture terrainColorRT1; // *** needed?

    public struct TriangleIndexData {
        public int v1;
        public int v2;
        public int v3;
    }
    public Mesh terrainMesh;

    private ComputeBuffer terrainVertexCBuffer;
    private ComputeBuffer terrainUVCBuffer;
    private ComputeBuffer terrainNormalCBuffer;
    private ComputeBuffer terrainColorCBuffer;
    private ComputeBuffer terrainTriangleCBuffer; 

    private ComputeBuffer quadVerticesCBuffer;  // quad mesh

    private int numFrameBufferStrokesPerDimension = 512;
    private ComputeBuffer frameBufferStrokesCBuffer;
    private int numGroundStrokesLrg = 64;
    private int numGroundStrokesMed = 128;
    private int numGroundStrokesSml = 256;
    public ComputeBuffer groundStrokesLrgCBuffer;
    public ComputeBuffer groundStrokesMedCBuffer;
    public ComputeBuffer groundStrokesSmlCBuffer;

    private int numDecomposerBits = 1024 * 4;
    public ComputeBuffer decomposerBitsCBuffer;
    private int numWasteBits = 1024 * 4;
    public ComputeBuffer wasteBitsCBuffer;

    public Vector4 spawnBoundsCameraDetails;

    /*public struct TerrainSlotGenome {
        public Color color;
        public float elevationChange;
    }*/

    public struct TerrainSimpleBrushData { // background terrain
        public Vector3 worldPos;
		public Vector2 scale;
		public Vector2 heading;
		public int brushType;
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
    };

    public override void Initialize() {

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

        
        Graphics.Blit(terrainInitHeightMap, terrainHeightRT0); //, testInitTerrainDataBlitMat);
        testInitTerrainDataBlitMat.SetPass(0);
        testInitTerrainDataBlitMat.SetTexture("_MainTex", terrainInitHeightMap);
        Graphics.Blit(terrainInitHeightMap, terrainHeightRT0, testInitTerrainDataBlitMat);

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

        //Graphics.Blit(terrainHeightRT0, terrainColorRT0, testInitTerrainDataBlitMat);
        //Graphics.Blit(terrainColorRT0, terrainColorRT1);  // maybe not needed?
        //Graphics.Blit(terrainHeightRT0, terrainHeightDataRT);  // maybe not needed?

        InitializeBuffers();        
        InitializeMaterials();
        InitializeTerrain();
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
                Color randColor = UnityEngine.Random.ColorHSV();
                float randAlpha = UnityEngine.Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(bedrockSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                
                mutatedGenome.elevationChange = Mathf.Lerp(bedrockSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

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
                Color randColor = UnityEngine.Random.ColorHSV();
                float randAlpha = UnityEngine.Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(stoneSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                mutatedGenome.elevationChange = Mathf.Lerp(stoneSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

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
                Color randColor = UnityEngine.Random.ColorHSV();
                float randAlpha = UnityEngine.Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(pebblesSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                mutatedGenome.elevationChange = Mathf.Lerp(pebblesSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

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
                Color randColor = UnityEngine.Random.ColorHSV();
                float randAlpha = UnityEngine.Random.Range(0f, 1f);  // shininess
                randColor.a = randAlpha;
                Color mutatedColor = Color.Lerp(sandSlotGenomeCurrent.color, randColor, mutationSize * iLerp);
                mutatedGenome.color = mutatedColor;
                mutatedGenome.elevationChange = Mathf.Lerp(sandSlotGenomeCurrent.elevationChange, UnityEngine.Random.Range(0f, 1f), iLerp);

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
        float boundsLrg = 256f;
        for (int x = 0; x < numDecomposerBits; x++)
        {
            decomposerBitsArray[x].index = x;
            //int index = y * numGroundStrokesLrg + x;
            float xPos = 0f; // (float)x / (float)(numGroundBits - 1) * boundsLrg;
            float yPos = 0f; // xPos; // (1f - (float)y / (float)(numGroundStrokesLrg - 1)) * boundsLrg;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 0.0f) * 16f;
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            decomposerBitsArray[x].worldPos = pos;
            decomposerBitsArray[x].heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            decomposerBitsArray[x].localScale = new Vector2(UnityEngine.Random.Range(0.4f, 1.5f), UnityEngine.Random.Range(1.0f, 2.5f)) * UnityEngine.Random.Range(1.5f, 4.2f); // Y is forward, along stroke
            decomposerBitsArray[x].age = UnityEngine.Random.Range(1f, 2f);
            decomposerBitsArray[x].speed = 0f;
            decomposerBitsArray[x].noiseVal = 1f;
            decomposerBitsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            
        }
        decomposerBitsCBuffer.SetData(decomposerBitsArray);
}
    private void InitializeWasteBits()
    {
        wasteBitsCBuffer = new ComputeBuffer(numWasteBits, sizeof(float) * 11 + sizeof(int) * 2);
        GroundBitsData[] wasteBitsArray = new GroundBitsData[wasteBitsCBuffer.count];
        float boundsLrg = 256f;
        for (int x = 0; x < numWasteBits; x++)
        {
            wasteBitsArray[x].index = x;
            //int index = y * numcarpetStrokesLrg + x;
            float xPos = (float)x / (float)(numWasteBits - 1) * boundsLrg;
            float yPos = xPos; // (1f - (float)y / (float)(numcarpetStrokesLrg - 1)) * boundsLrg;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 0.0f) * 16f;
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            wasteBitsArray[x].worldPos = Vector3.zero;
            wasteBitsArray[x].heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            wasteBitsArray[x].localScale = new Vector2(UnityEngine.Random.Range(0.5f, 1.5f), UnityEngine.Random.Range(0.5f, 1.5f)) * UnityEngine.Random.Range(0.5f, 1f); // Y is forward, along stroke
            wasteBitsArray[x].age = UnityEngine.Random.Range(1f, 2f);
            wasteBitsArray[x].speed = 0f;
            wasteBitsArray[x].noiseVal = 1f;
            wasteBitsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            wasteBitsArray[x].isActive = 0f;
        }
        wasteBitsCBuffer.SetData(wasteBitsArray);
    }

    private void InitializeGroundStrokeBuffers() {

        // LARGE ::::::
        groundStrokesLrgCBuffer = new ComputeBuffer(numGroundStrokesLrg * numGroundStrokesLrg, sizeof(float) * 7 + sizeof(int));
        TerrainSimpleBrushData[] groundStrokesLrgArray = new TerrainSimpleBrushData[groundStrokesLrgCBuffer.count];
        float boundsLrg = 256f;
        for(int x = 0; x < numGroundStrokesLrg; x++) {
            for(int y = 0; y < numGroundStrokesLrg; y++) {
                int index = y * numGroundStrokesLrg + x;
                float xPos = (float)x / (float)(numGroundStrokesLrg - 1) * boundsLrg;
                float yPos = (1f - (float)y / (float)(numGroundStrokesLrg - 1)) * boundsLrg;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 0.0f) * 16f;
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                groundStrokesLrgArray[index].worldPos = pos;
                groundStrokesLrgArray[index].scale = new Vector2(UnityEngine.Random.Range(0.4f, 0.8f), UnityEngine.Random.Range(1.5f, 2f)) * 12f; // Y is forward, along stroke
                groundStrokesLrgArray[index].heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                groundStrokesLrgArray[index].brushType = UnityEngine.Random.Range(0,4);
            }
        }
        groundStrokesLrgCBuffer.SetData(groundStrokesLrgArray);


        // MEDIUM :::::
        groundStrokesMedCBuffer = new ComputeBuffer(numGroundStrokesMed * numGroundStrokesMed, sizeof(float) * 7 + sizeof(int));
        TerrainSimpleBrushData[] groundStrokesMedArray = new TerrainSimpleBrushData[groundStrokesMedCBuffer.count];
        float boundsMed = 256f;
        for(int x = 0; x < numGroundStrokesMed; x++) {
            for(int y = 0; y < numGroundStrokesMed; y++) {
                int index = y * numGroundStrokesMed + x;
                float xPos = (float)x / (float)(numGroundStrokesMed - 1) * boundsMed;
                float yPos = (1f - (float)y / (float)(numGroundStrokesMed - 1)) * boundsMed;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 0.0f) * 16f;
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                groundStrokesMedArray[index].worldPos = pos;
                groundStrokesMedArray[index].scale = new Vector2(UnityEngine.Random.Range(0.4f, 0.8f), UnityEngine.Random.Range(1.55f, 2.3f)) * 4.20f; // Y is forward, along stroke
                groundStrokesMedArray[index].heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                groundStrokesMedArray[index].brushType = UnityEngine.Random.Range(0,4);
            }
        }
        groundStrokesMedCBuffer.SetData(groundStrokesMedArray);


        // SMALL :::::
        groundStrokesSmlCBuffer = new ComputeBuffer(numGroundStrokesSml * numGroundStrokesSml, sizeof(float) * 7 + sizeof(int));
        TerrainSimpleBrushData[] groundStrokesSmlArray = new TerrainSimpleBrushData[groundStrokesSmlCBuffer.count];
        float boundsSml = 256f;
        for(int x = 0; x < numGroundStrokesSml; x++) {
            for(int y = 0; y < numGroundStrokesSml; y++) {
                int index = y * numGroundStrokesSml + x;
                float xPos = (float)x / (float)(numGroundStrokesSml - 1) * boundsSml;
                float yPos = (1f - (float)y / (float)(numGroundStrokesSml - 1)) * boundsSml;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f) * 0.0f) * 16f;
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                groundStrokesSmlArray[index].worldPos = pos;
                groundStrokesSmlArray[index].scale = new Vector2(UnityEngine.Random.Range(0.4f, 0.8f), UnityEngine.Random.Range(1.75f, 3f) * 0.5f) * 1.55f; // Y is forward, along stroke
                groundStrokesSmlArray[index].heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                groundStrokesSmlArray[index].brushType = UnityEngine.Random.Range(0,4);
            }
        }
        groundStrokesSmlCBuffer.SetData(groundStrokesSmlArray);
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

    private void InitializeFrameBufferStrokesBuffer() {
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
    }

    private void InitializeMaterials() {
        frameBufferStrokeDisplayMat.SetPass(0);
        frameBufferStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        frameBufferStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        frameBufferStrokeDisplayMat.SetBuffer("frameBufferStrokesCBuffer", frameBufferStrokesCBuffer);     
        
        
        groundStrokesLrgDisplayMat.SetPass(0);
        groundStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesLrgCBuffer);   
        
        groundStrokesMedDisplayMat.SetPass(0);
        groundStrokesMedDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesMedDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesMedDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesMedCBuffer);    

        groundStrokesSmlDisplayMat.SetPass(0);
        groundStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesSmlCBuffer);

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

        
        groundDryLandDisplayMat.SetPass(0);
        groundDryLandDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        groundDryLandDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        groundDryLandDisplayMat.SetBuffer("frameBufferStrokesCBuffer", groundStrokesSmlCBuffer);  
    }

    public void InitializeTerrain() {
        //Debug.Log("InitializeTerrain!");

        int meshResolution = 128;
        float mapSize = SimulationManager._MapSize;

        if(terrainGO != null && terrainInitHeightMap != null) {
            if (computeShaderTerrainGeneration == null) {
                Debug.LogError("NO COMPUTE SHADER SET!!!!");
            }

            if (terrainVertexCBuffer != null)
                terrainVertexCBuffer.Release();
            terrainVertexCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 3);
            if (terrainUVCBuffer != null)
                terrainUVCBuffer.Release();
            terrainUVCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 2);
            if (terrainNormalCBuffer != null)
                terrainNormalCBuffer.Release();
            terrainNormalCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 3);
            if (terrainColorCBuffer != null)
                terrainColorCBuffer.Release();
            terrainColorCBuffer = new ComputeBuffer(meshResolution * meshResolution, sizeof(float) * 4);
            if (terrainTriangleCBuffer != null)
                terrainTriangleCBuffer.Release();
            terrainTriangleCBuffer = new ComputeBuffer((meshResolution - 1) * (meshResolution - 1) * 2, sizeof(int) * 3);
                            
            // Set Shader properties so it knows where and what to build::::
            computeShaderTerrainGeneration.SetInt("_MeshResolution", meshResolution);
            computeShaderTerrainGeneration.SetInt("_MeshResolution", meshResolution);
            computeShaderTerrainGeneration.SetVector("_QuadBounds", new Vector4(0f, mapSize, 0f, mapSize)); //new Vector4(-mapSize * 0.5f, mapSize * 1.5f, -mapSize * 0.5f, mapSize * 1.5f));
            computeShaderTerrainGeneration.SetVector("_HeightRange", new Vector4(-10f, 10f, 0f, 0f));


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

            terrainGO.GetComponent<MeshFilter>().sharedMesh = mesh;
            terrainMesh = mesh;
        }
        else {
            Debug.LogError("ERROR! No terrainGO or heightmap referenced! Plug them into inspector!!!");
        }
    }
    private Mesh GenerateTerrainMesh() {
        if(terrainMesh == null) {
            terrainMesh = new Mesh();
        }
        else {

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
    public void AlignGroundStrokesToTerrain() {
        int kernelCSAlignGroundStrokesLrg = computeShaderTerrainGeneration.FindKernel("CSUpdateGroundStrokes");
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        //computeShaderTerrainGeneration.SetFloat("_GroundStrokeTerrainAlign", 1f);
        computeShaderTerrainGeneration.SetFloat("_BrushAlignment", 1f);
        computeShaderTerrainGeneration.SetTexture(kernelCSAlignGroundStrokesLrg, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetBuffer(kernelCSAlignGroundStrokesLrg, "terrainFrameBufferStrokesCBuffer", groundStrokesLrgCBuffer);        
        computeShaderTerrainGeneration.Dispatch(kernelCSAlignGroundStrokesLrg, groundStrokesLrgCBuffer.count, 1, 1);

        int kernelCSAlignGroundStrokesMed = computeShaderTerrainGeneration.FindKernel("CSUpdateGroundStrokes");
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        //computeShaderTerrainGeneration.SetFloat("_GroundStrokeTerrainAlign", 1f);
        computeShaderTerrainGeneration.SetTexture(kernelCSAlignGroundStrokesMed, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_BrushAlignment", 1f);
        computeShaderTerrainGeneration.SetBuffer(kernelCSAlignGroundStrokesMed, "terrainFrameBufferStrokesCBuffer", groundStrokesMedCBuffer);        
        computeShaderTerrainGeneration.Dispatch(kernelCSAlignGroundStrokesMed, groundStrokesMedCBuffer.count, 1, 1);

        int kernelCSAlignGroundStrokesSml = computeShaderTerrainGeneration.FindKernel("CSUpdateGroundStrokes");
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        //computeShaderTerrainGeneration.SetFloat("_GroundStrokeTerrainAlign", 1f);
        computeShaderTerrainGeneration.SetTexture(kernelCSAlignGroundStrokesSml, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_BrushAlignment", 0f);
        computeShaderTerrainGeneration.SetBuffer(kernelCSAlignGroundStrokesSml, "terrainFrameBufferStrokesCBuffer", groundStrokesSmlCBuffer);        
        computeShaderTerrainGeneration.Dispatch(kernelCSAlignGroundStrokesSml, groundStrokesSmlCBuffer.count, 1, 1);

    }

    public override void Tick(RenderTexture maskTex) {
        SimTerrainBits(maskTex);
    }

    private void SimTerrainBits(RenderTexture maskTex)
    {
        
        int kernelSimGroundBits = computeShaderTerrainGeneration.FindKernel("CSSimDecomposerBitsData");
        computeShaderTerrainGeneration.SetBuffer(kernelSimGroundBits, "groundBitsCBuffer", decomposerBitsCBuffer);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "decomposersRead", maskTex);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        
        computeShaderTerrainGeneration.Dispatch(kernelSimGroundBits, decomposerBitsCBuffer.count / 1024, 1, 1);

        int kernelSimWasteBits = computeShaderTerrainGeneration.FindKernel("CSSimCarpetBitsData");
        computeShaderTerrainGeneration.SetBuffer(kernelSimWasteBits, "groundBitsCBuffer", wasteBitsCBuffer);
        computeShaderTerrainGeneration.SetTexture(kernelSimWasteBits, "AltitudeRead", terrainHeightDataRT);
        computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        computeShaderTerrainGeneration.Dispatch(kernelSimWasteBits, wasteBitsCBuffer.count / 1024, 1, 1);

    }

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

    public override void Cleanup() {
        if (frameBufferStrokesCBuffer != null) {
            frameBufferStrokesCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        if (groundStrokesLrgCBuffer != null) {
            groundStrokesLrgCBuffer.Release();
        }
        if (groundStrokesMedCBuffer != null) {
            groundStrokesMedCBuffer.Release();
        }
        if (groundStrokesSmlCBuffer != null) {
            groundStrokesSmlCBuffer.Release();
        }
        if (decomposerBitsCBuffer != null)
        {
            decomposerBitsCBuffer.Release();
        }
        if (wasteBitsCBuffer != null)
        {
            wasteBitsCBuffer.Release();
        }
        
    }
}
