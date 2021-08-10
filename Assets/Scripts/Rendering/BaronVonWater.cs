using UnityEngine;
using UnityEngine.Rendering;

public class BaronVonWater : RenderBaron 
{
    SimulationManager simulation => SimulationManager.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    TheRenderKing renderKing => TheRenderKing.instance;
    BaronVonTerrain terrain => renderKing.baronVonTerrain;
    CommandBuffer cmdBufferMain => renderKing.cmdBufferMain;
    VegetationManager vegetation => renderKing.vegetationManager;

    //public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderWaterRender;

    public EnvironmentFluidManager fluidManagerRef;
    

    //public float _GlobalWaterLevel = 0f;

    public Material waterQuadStrokesLrgDisplayMat;
    public Material waterQuadStrokesSmlDisplayMat;
    public Material waterCurveStrokeDisplayMat;
    public Material waterChainStrokeDisplayMat;

    public Material waterNutrientsBitsDisplayMat;
    public Material waterSurfaceBitsDisplayMat;
    public Material waterSurfaceBitsShadowsDisplayMat;
    public Material waterDebrisBitsDisplayMat;
    public Material waterDebrisBitsShadowsDisplayMat;

    public ComputeBuffer quadVerticesCBuffer;  // quad mesh

    private int numWaterQuadStrokesPerDimensionLrg = 128;
    public ComputeBuffer waterQuadStrokesCBufferLrg;
    private int numWaterQuadStrokesPerDimensionSml = 128;
    public ComputeBuffer waterQuadStrokesCBufferSml;

    public RenderTexture altitudeMapRef;

    public int waterSurfaceMapResolution = 128;
    public RenderTexture waterSurfaceDataRT0;  // 2 for swapping enabled
    public RenderTexture waterSurfaceDataRT1;

    public int numWaterCurveMeshQuads = 6;
    public ComputeBuffer waterCurveVerticesCBuffer;  // short ribbon mesh
    public int numWaterCurves = 1024 * 8;
    public ComputeBuffer waterCurveStrokesCBuffer;

    public int numWaterChains = 1024 * 2;
    public int numPointsPerWaterChain = 16;
    public ComputeBuffer waterChains0CBuffer;
    public ComputeBuffer waterChains1CBuffer;

    private int numNutrientsBits = 1024 * 4;
    private int numSurfaceBits = 1024 * 8;
    private int numDebrisBits = 1024 * 4;
    public ComputeBuffer waterNutrientsBitsCBuffer;
    public ComputeBuffer waterSurfaceBitsCBuffer;
    //public ComputeBuffer waterSurfaceBitsShadowsCBuffer;
    public ComputeBuffer waterDebrisBitsCBuffer;
    public ComputeBuffer waterDebrisBitsShadowsCBuffer;

    private float cursorClickWaveOn = 0f;
    private int cursorClickWaveDuration = 240;
    private int cursorClickWaveTimeStepCounter = 0;
    private Vector4 cursorClickWorldPos;

    public Vector4 spawnBoundsCameraDetails;

    private int debugFrameCounter = 0;

    public float camDistNormalized = 1f;

    public ComputeBuffer waterRipplesCBuffer;
    public WaterRippleData[] waterRipplesDataArray;
    public int numWaterRipples = 32;
    private int nextWaterRippleIndex = 0;

    public struct WaterRippleData {
        public Vector2 coords;
        public float startTime;
        public float amplitude;
        public float frequency;
        public float falloff;
    }
    
    public struct WaterCurveStrokeData {   // 2 ints, 17 floats
        public int index;        
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;
        public Vector4 widths;
        public Vector3 hue;
        //public float restLength;
        public float strength;   // extra data to use however     
		//public Vector2 vel; // was used before for strentching quad -- still needed?
		public float age;  // to allow for fade-in and fade-out
        public int brushType;  // brush texture mask
    }
    
    /*public struct TrailDotData { 
        public int parentIndex;
        public Vector2 coords01;
        public float age;
        public float initAlpha;
    }*/
    
    public struct TestStrokeData { // background terrain
        public Vector3 worldPos;
		public Vector2 scale;
		public Vector2 heading;
		public int brushType;
    }
    
    public struct WaterQuadData {
	    public int index;
	    public Vector3 worldPos;
	    public Vector2 heading;
	    public Vector2 localScale;
	    public float age;
	    public float speed;
        public int brushType;
    };

	public override void Initialize() {
        //altitudeMapRef = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        //altitudeMapRef.wrapMode = TextureWrapMode.Repeat;
        //altitudeMapRef.enableRandomWrite = true;
        //altitudeMapRef.Create();

        InitializeBuffers();        
        InitializeMaterials();
    }

    private void InitializeBuffers() {
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering           
        InitializeWaterSurfaceReflectiveBuffers();
        InitializeWaterCurveMeshBuffer();
        //InitializeWaterQuadStrokesBuffer();
        InitializeWaterCurveStrokesCBuffer();
        InitializeWaterChainStrokesCBuffer();
        InitializeWaterSurface();
        InitializeWaterNutrientsBits();
        InitializeWaterSurfaceBits();
        InitializeWaterDebrisBits();
        InitializeWaterRipplesBuffer();
    }

    private void InitializeWaterRipplesBuffer() {
        waterRipplesCBuffer = new ComputeBuffer(numWaterRipples, sizeof(float) * 6);
        waterRipplesDataArray = new WaterRippleData[waterRipplesCBuffer.count];

        for(int i = 0; i < numWaterRipples; i++) {
            WaterRippleData data = new WaterRippleData();
            data.coords = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            data.startTime = UnityEngine.Time.realtimeSinceStartup;

            waterRipplesDataArray[i] = data;
        }
        // Temp init state for debugging:
        waterRipplesCBuffer.SetData(waterRipplesDataArray);
    }
    
    public void RequestNewWaterRipple(Vector2 coords, float time, float amp, float freq) {

    }

    private void InitializeWaterCurveMeshBuffer() {
        float rowSize = 1f / (float)numWaterCurveMeshQuads;

        waterCurveVerticesCBuffer = new ComputeBuffer(6 * numWaterCurveMeshQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[waterCurveVerticesCBuffer.count];
        for(int i = 0; i < numWaterCurveMeshQuads; i++) {
            int baseIndex = i * 6;

            float startCoord = (float)i;
            float endCoord = (float)(i + 1);
            verticesArray[baseIndex + 0] = new Vector3(0.5f, startCoord * rowSize);
            verticesArray[baseIndex + 1] = new Vector3(0.5f, endCoord * rowSize);
            verticesArray[baseIndex + 2] = new Vector3(-0.5f, endCoord * rowSize);
            verticesArray[baseIndex + 3] = new Vector3(-0.5f, endCoord * rowSize);
            verticesArray[baseIndex + 4] = new Vector3(-0.5f, startCoord * rowSize);
            verticesArray[baseIndex + 5] = new Vector3(0.5f, startCoord * rowSize); 
        }

        waterCurveVerticesCBuffer.SetData(verticesArray);
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
    
    public void InitializeWaterCurveStrokesCBuffer() {
        WaterCurveStrokeData[] waterCurveStrokesDataArray = new WaterCurveStrokeData[numWaterCurves];
        waterCurveStrokesCBuffer = new ComputeBuffer(waterCurveStrokesDataArray.Length, sizeof(float) * 17 + sizeof(int) * 2);

        for(int i = 0; i < waterCurveStrokesDataArray.Length; i++) {
            waterCurveStrokesDataArray[i] = new WaterCurveStrokeData();
            waterCurveStrokesDataArray[i].index = i;
            
            waterCurveStrokesDataArray[i].p0 = new Vector2(UnityEngine.Random.Range(0f, 256f), UnityEngine.Random.Range(0f, 256f));
            waterCurveStrokesDataArray[i].p1 = waterCurveStrokesDataArray[i].p0 + new Vector2(0f, -2f);
            waterCurveStrokesDataArray[i].p2 = waterCurveStrokesDataArray[i].p0 + new Vector2(0f, -4f);
            waterCurveStrokesDataArray[i].p3 = waterCurveStrokesDataArray[i].p0 + new Vector2(0f, -6f);
            waterCurveStrokesDataArray[i].widths = new Vector4(UnityEngine.Random.Range(0.75f, 1.25f), UnityEngine.Random.Range(0.75f, 1.25f), UnityEngine.Random.Range(0.75f, 1.25f), UnityEngine.Random.Range(0.75f, 1.25f)) * 0.75f;
            waterCurveStrokesDataArray[i].strength = 1f;  
		    waterCurveStrokesDataArray[i].age = UnityEngine.Random.Range(1f, 2f);
            waterCurveStrokesDataArray[i].brushType = 0; 
        }

        waterCurveStrokesCBuffer.SetData(waterCurveStrokesDataArray);
    }
    
    public void InitializeWaterChainStrokesCBuffer() {
        Vector2[] waterChainDataArray = new Vector2[numWaterChains * numPointsPerWaterChain];
        for (int i = 0; i < waterChainDataArray.Length; i++) {
            //int agentIndex = (int)Mathf.Floor((float)i / numPointsPerWaterChain);
            float trailPos = (float)i % (float)numPointsPerWaterChain;
            Vector2 randPos = new Vector2(UnityEngine.Random.Range(-60f, 60f), UnityEngine.Random.Range(-60f, 60f));
            waterChainDataArray[i] = randPos + new Vector2(0f, trailPos * -1f);
        }
        waterChains0CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
        waterChains0CBuffer.SetData(waterChainDataArray);
        waterChains1CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
    }
    
    private void InitializeWaterSurfaceReflectiveBuffers() {
        waterQuadStrokesCBufferLrg = new ComputeBuffer(numWaterQuadStrokesPerDimensionLrg * numWaterQuadStrokesPerDimensionLrg, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterQuadStrokesArrayLrg = new WaterQuadData[waterQuadStrokesCBufferLrg.count];
        float waterQuadStrokesBounds = SimulationManager._MapSize;
        for(int x = 0; x < numWaterQuadStrokesPerDimensionLrg; x++) {
            for(int y = 0; y < numWaterQuadStrokesPerDimensionLrg; y++) {
                int index = x * numWaterQuadStrokesPerDimensionLrg + y;
                float xPos = (float)x / (float)(numWaterQuadStrokesPerDimensionLrg - 1) * waterQuadStrokesBounds;
                float yPos = (float)y / (float)(numWaterQuadStrokesPerDimensionLrg - 1) * waterQuadStrokesBounds;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                waterQuadStrokesArrayLrg[index].worldPos = pos;
                waterQuadStrokesArrayLrg[index].localScale = 3f * new Vector2(UnityEngine.Random.Range(0.5f, 0.75f), UnityEngine.Random.Range(2.1f, 3.5f)) * UnityEngine.Random.Range(0.75f, 1.33f); // Y is forward, along stroke
                waterQuadStrokesArrayLrg[index].heading = new Vector2(Mathf.Round(UnityEngine.Random.Range(0f, 1f)) * 2f - 1f, UnityEngine.Random.Range(-0.15f, 0.15f)).normalized;
                waterQuadStrokesArrayLrg[index].brushType = UnityEngine.Random.Range(0,4);
                waterQuadStrokesArrayLrg[index].age = UnityEngine.Random.Range(1f, 2f);
            
            }
        }
        waterQuadStrokesCBufferLrg.SetData(waterQuadStrokesArrayLrg);

        waterQuadStrokesCBufferSml = new ComputeBuffer(numWaterQuadStrokesPerDimensionSml * numWaterQuadStrokesPerDimensionSml, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterQuadStrokesArraySml = new WaterQuadData[waterQuadStrokesCBufferSml.count];
        
        for (int x = 0; x < numWaterQuadStrokesPerDimensionSml; x++)
        {
            for (int y = 0; y < numWaterQuadStrokesPerDimensionSml; y++)
            {
                int index = x * numWaterQuadStrokesPerDimensionSml + y;
                float xPos = (float)x / (float)(numWaterQuadStrokesPerDimensionSml - 1) * 64f + 128f;
                float yPos = (float)y / (float)(numWaterQuadStrokesPerDimensionSml - 1) * 64f + 128f;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                waterQuadStrokesArraySml[index].worldPos = pos;
                waterQuadStrokesArraySml[index].localScale = 0.08f * new Vector2(UnityEngine.Random.Range(0.5f, 0.75f), UnityEngine.Random.Range(2.1f, 3.5f)) * UnityEngine.Random.Range(0.75f, 1.33f); // Y is forward, along stroke
                waterQuadStrokesArraySml[index].heading = new Vector2(Mathf.Round(UnityEngine.Random.Range(0f, 1f)) * 2f - 1f, UnityEngine.Random.Range(-0.15f, 0.15f)).normalized;
                waterQuadStrokesArraySml[index].brushType = UnityEngine.Random.Range(0, 4);
                waterQuadStrokesArraySml[index].age = UnityEngine.Random.Range(1f, 2f);

            }
        }
        waterQuadStrokesCBufferSml.SetData(waterQuadStrokesArraySml);
    }
    
    private void InitializeWaterNutrientsBits()
    {
        waterNutrientsBitsCBuffer = new ComputeBuffer(numNutrientsBits, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterNutrientsBitsArray = new WaterQuadData[waterNutrientsBitsCBuffer.count];

        for (int x = 0; x < numNutrientsBits; x++)
        {

            float xPos = 0f; // (float)x / (float)(numNutrientsBits - 1) * waterNutrientsBitsBounds;
            float yPos = 0f; // (float)y / (float)(numwaterNutrientsBitsPerDimension - 1) * waterNutrientsBitsBounds;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            waterNutrientsBitsArray[x].worldPos = pos;
            waterNutrientsBitsArray[x].localScale = 0.035f * new Vector2(UnityEngine.Random.Range(0.75f, 1.5f), UnityEngine.Random.Range(2f, 3.5f)) * UnityEngine.Random.Range(0.4f, 0.7f); // Y is forward, along stroke
            waterNutrientsBitsArray[x].heading = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-0.5f, 0.5f)).normalized;
            waterNutrientsBitsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            waterNutrientsBitsArray[x].age = UnityEngine.Random.Range(1f, 2f);

            
        }
        waterNutrientsBitsCBuffer.SetData(waterNutrientsBitsArray);
    }
    
    private void InitializeWaterSurfaceBits()
    {
        waterSurfaceBitsCBuffer = new ComputeBuffer(numSurfaceBits, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterSurfaceBitsArray = new WaterQuadData[waterSurfaceBitsCBuffer.count];

        for (int x = 0; x < numSurfaceBits; x++)
        {
            float xPos = 128f; // (float)x / (float)(numSurfaceBits - 1) * waterSurfaceBitsBounds;
            float yPos = 128f; // (float)y / (float)(numwaterSurfaceBitsPerDimension - 1) * waterSurfaceBitsBounds;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            waterSurfaceBitsArray[x].worldPos = pos;
            waterSurfaceBitsArray[x].localScale = new Vector2(UnityEngine.Random.Range(0.6f, 1.6f), UnityEngine.Random.Range(0.67f, 1.5f)) * UnityEngine.Random.Range(0.49f, 0.66f); // Y is forward, along stroke
            waterSurfaceBitsArray[x].heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            waterSurfaceBitsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            waterSurfaceBitsArray[x].age = UnityEngine.Random.Range(1f, 2f);
        }
        waterSurfaceBitsCBuffer.SetData(waterSurfaceBitsArray);

        /*waterSurfaceBitsShadowsCBuffer = new ComputeBuffer(numSurfaceBits, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterSurfaceBitsShadowsArray = new WaterQuadData[waterSurfaceBitsShadowsCBuffer.count];

        for (int x = 0; x < numSurfaceBits; x++)
        {

            float xPos = 128f; // (float)x / (float)(numSurfaceShadowsBits - 1) * waterSurfaceShadowsBitsBounds;
            float yPos = 128f; // (float)y / (float)(numwaterSurfaceShadowsBitsPerDimension - 1) * waterSurfaceShadowsBitsBounds;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            waterSurfaceBitsShadowsArray[x].worldPos = pos;
            waterSurfaceBitsShadowsArray[x].localScale = new Vector2(UnityEngine.Random.Range(0.9f, 1.5f), UnityEngine.Random.Range(1.1f, 2.5f)) * UnityEngine.Random.Range(0.36f, 0.65f); // Y is forward, along stroke
            waterSurfaceBitsShadowsArray[x].heading = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-0.5f, 0.5f)).normalized;
            waterSurfaceBitsShadowsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            waterSurfaceBitsShadowsArray[x].age = UnityEngine.Random.Range(1f, 2f);


        }
        waterSurfaceBitsShadowsCBuffer.SetData(waterSurfaceBitsShadowsArray);
        */
    }
    
    private void InitializeWaterDebrisBits()
    {
        waterDebrisBitsCBuffer = new ComputeBuffer(numDebrisBits, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterDebrisBitsArray = new WaterQuadData[waterDebrisBitsCBuffer.count];

        for (int x = 0; x < numDebrisBits; x++)
        {

            float xPos = 128f; // (float)x / (float)(numDebrisBits - 1) * waterDebrisBitsBounds;
            float yPos = 128f; // (float)y / (float)(numwaterDebrisBitsPerDimension - 1) * waterDebrisBitsBounds;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)) * 2f;
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            waterDebrisBitsArray[x].worldPos = pos;
            waterDebrisBitsArray[x].localScale = new Vector2(UnityEngine.Random.Range(0.4f, 2f), UnityEngine.Random.Range(0.4f, 2f)) * UnityEngine.Random.Range(0.26f, 0.85f); // Y is forward, along stroke
            waterDebrisBitsArray[x].heading = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-0.5f, 0.5f)).normalized;
            waterDebrisBitsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            waterDebrisBitsArray[x].age = UnityEngine.Random.Range(1f, 2f);


        }
        waterDebrisBitsCBuffer.SetData(waterDebrisBitsArray);

        waterDebrisBitsShadowsCBuffer = new ComputeBuffer(numDebrisBits, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterDebrisBitsShadowsArray = new WaterQuadData[waterDebrisBitsShadowsCBuffer.count];

        for (int x = 0; x < numDebrisBits; x++)
        {

            float xPos = 128f; // (float)x / (float)(numDebrisShadowsBits - 1) * waterDebrisShadowsBitsBounds;
            float yPos = 128f; // (float)y / (float)(numwaterDebrisShadowsBitsPerDimension - 1) * waterDebrisShadowsBitsBounds;
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
            Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
            waterDebrisBitsShadowsArray[x].worldPos = pos;
            waterDebrisBitsShadowsArray[x].localScale = new Vector2(UnityEngine.Random.Range(0.9f, 1.5f), UnityEngine.Random.Range(1.1f, 2.5f)) * UnityEngine.Random.Range(0.36f, 0.65f); // Y is forward, along stroke
            waterDebrisBitsShadowsArray[x].heading = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-0.5f, 0.5f)).normalized;
            waterDebrisBitsShadowsArray[x].brushType = UnityEngine.Random.Range(0, 4);
            waterDebrisBitsShadowsArray[x].age = UnityEngine.Random.Range(1f, 2f);


        }
        waterDebrisBitsShadowsCBuffer.SetData(waterDebrisBitsShadowsArray);
    }
    
    private void InitializeWaterSurface()
    {
        waterSurfaceDataRT0 = new RenderTexture(waterSurfaceMapResolution, waterSurfaceMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        waterSurfaceDataRT0.wrapMode = TextureWrapMode.Repeat;
        waterSurfaceDataRT0.enableRandomWrite = true;
        waterSurfaceDataRT0.Create();

        waterSurfaceDataRT1 = new RenderTexture(waterSurfaceMapResolution, waterSurfaceMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        waterSurfaceDataRT1.wrapMode = TextureWrapMode.Repeat;
        waterSurfaceDataRT1.enableRandomWrite = true;
        waterSurfaceDataRT1.Create();
    }

    // * WPP: merge repeat functionality
    private void InitializeMaterials() {
        waterQuadStrokesLrgDisplayMat.SetPass(0);
        waterQuadStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterQuadStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterQuadStrokesLrgDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterQuadStrokesCBufferLrg);

        waterQuadStrokesSmlDisplayMat.SetPass(0);
        waterQuadStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterQuadStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterQuadStrokesSmlDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterQuadStrokesCBufferSml);

        waterCurveStrokeDisplayMat.SetPass(0);
        waterCurveStrokeDisplayMat.SetBuffer("verticesCBuffer", waterCurveVerticesCBuffer);
        waterCurveStrokeDisplayMat.SetBuffer("waterCurveStrokesCBuffer", waterCurveStrokesCBuffer);
        waterCurveStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        waterChainStrokeDisplayMat.SetPass(0);
        waterChainStrokeDisplayMat.SetBuffer("verticesCBuffer", quadVerticesCBuffer);
        waterChainStrokeDisplayMat.SetBuffer("waterChainsReadCBuffer", waterChains0CBuffer);
        waterChainStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        // waterBits:
        waterNutrientsBitsDisplayMat.SetPass(0);
        waterNutrientsBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterNutrientsBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterNutrientsBitsDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterNutrientsBitsCBuffer);

        waterSurfaceBitsDisplayMat.SetPass(0);
        waterSurfaceBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterSurfaceBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterSurfaceBitsDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterSurfaceBitsCBuffer);

        waterSurfaceBitsShadowsDisplayMat.SetPass(0);
        waterSurfaceBitsShadowsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterSurfaceBitsShadowsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterSurfaceBitsShadowsDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterSurfaceBitsCBuffer);

        waterDebrisBitsDisplayMat.SetPass(0);
        waterDebrisBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterDebrisBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterDebrisBitsDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterDebrisBitsCBuffer);

        waterDebrisBitsShadowsDisplayMat.SetPass(0);
        waterDebrisBitsShadowsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterDebrisBitsShadowsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterDebrisBitsShadowsDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterDebrisBitsShadowsCBuffer);
    }
    
    private void SimWaterQuads() {
        /*int kernelSimWaterQuads = computeShaderWaterRender.FindKernel("CSSimWaterQuadsData");                
        computeShaderWaterRender.SetBuffer(kernelSimWaterQuads, "waterQuadStrokesCBuffer", waterQuadStrokesCBufferLrg);
        computeShaderWaterRender.SetTexture(kernelSimWaterQuads, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);    
        computeShaderWaterRender.SetTexture(kernelSimWaterQuads, "AltitudeRead", altitudeMapRef);     
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetVector("_SpawnBoundsCameraDetails", new Vector4(0f, 0f, SimulationManager._MapSize, SimulationManager._MapSize));
        computeShaderWaterRender.SetInt("_BufferLength", waterQuadStrokesCBufferLrg.count);
        computeShaderWaterRender.Dispatch(kernelSimWaterQuads, waterQuadStrokesCBufferLrg.count / 1024, 1, 1);

        // SML:
        kernelSimWaterQuads = computeShaderWaterRender.FindKernel("CSSimWaterQuadsData");
        computeShaderWaterRender.SetBuffer(kernelSimWaterQuads, "waterQuadStrokesCBuffer", waterQuadStrokesCBufferSml);
        computeShaderWaterRender.SetTexture(kernelSimWaterQuads, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderWaterRender.SetTexture(kernelSimWaterQuads, "AltitudeRead", altitudeMapRef);
        computeShaderWaterRender.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetInt("_BufferLength", waterQuadStrokesCBufferSml.count);
        computeShaderWaterRender.Dispatch(kernelSimWaterQuads, waterQuadStrokesCBufferSml.count / 1024, 1, 1);
        */
        // WATER BITS HERE::: ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        int kernelSimDetailBitsData = computeShaderWaterRender.FindKernel("CSSimDetailBitsData");
        computeShaderWaterRender.SetBuffer(kernelSimDetailBitsData, "waterQuadStrokesCBuffer", waterNutrientsBitsCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "AltitudeRead", altitudeMapRef);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "_ResourceSimTransferTex", vegetation.resourceSimTransferRT);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "_ResourceGridTex", vegetation.resourceGridRT1);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetVector("_SpawnBoundsCameraDetails", spawnBoundsCameraDetails);
        computeShaderWaterRender.Dispatch(kernelSimDetailBitsData, waterNutrientsBitsCBuffer.count / 1024, 1, 1);
        
        /*
        int kernelSimSurfaceBitsData = computeShaderWaterRender.FindKernel("CSSimSurfaceBitsData");
        computeShaderWaterRender.SetBuffer(kernelSimSurfaceBitsData, "waterQuadStrokesCBuffer", waterSurfaceBitsCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimSurfaceBitsData, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderWaterRender.SetTexture(kernelSimSurfaceBitsData, "AltitudeRead", altitudeMapRef);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderWaterRender.Dispatch(kernelSimSurfaceBitsData, waterSurfaceBitsCBuffer.count / 1024, 1, 1);
        */
        /*
        kernelSimSurfaceBitsData = computeShaderWaterRender.FindKernel("CSSimSurfaceBitsData");
        computeShaderWaterRender.SetBuffer(kernelSimSurfaceBitsData, "waterQuadStrokesCBuffer", waterSurfaceBitsShadowsCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimSurfaceBitsData, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderWaterRender.SetTexture(kernelSimSurfaceBitsData, "AltitudeRead", altitudeMapRef);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderWaterRender.Dispatch(kernelSimSurfaceBitsData, waterSurfaceBitsShadowsCBuffer.count / 1024, 1, 1);
        */
        /*
        // DEBRIS BITS::::::::: %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        kernelSimDetailBitsData = computeShaderWaterRender.FindKernel("CSSimDetailBitsData");
        computeShaderWaterRender.SetBuffer(kernelSimDetailBitsData, "waterQuadStrokesCBuffer", waterDebrisBitsCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "AltitudeRead", altitudeMapRef);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.Dispatch(kernelSimDetailBitsData, waterDebrisBitsCBuffer.count / 1024, 1, 1);

        kernelSimDetailBitsData = computeShaderWaterRender.FindKernel("CSSimDetailBitsData");
        computeShaderWaterRender.SetBuffer(kernelSimDetailBitsData, "waterQuadStrokesCBuffer", waterDebrisBitsShadowsCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderWaterRender.SetTexture(kernelSimDetailBitsData, "AltitudeRead", altitudeMapRef);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.Dispatch(kernelSimDetailBitsData, waterDebrisBitsShadowsCBuffer.count / 1024, 1, 1);
        */
    }
    
    private void SimWaterCurves() {
        int kernelSimWaterCurves = computeShaderWaterRender.FindKernel("CSSimWaterCurvesData");

        //computeShaderWaterRender.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        //computeShaderWaterRender.SetFloat("_DeltaTime", fluidManager.deltaTime);
        //computeShaderWaterRender.SetFloat("_InvGridScale", fluidManager.invGridScale);
        computeShaderWaterRender.SetBuffer(kernelSimWaterCurves, "waterCurveStrokesCBuffer", waterCurveStrokesCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimWaterCurves, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);    
        computeShaderWaterRender.SetTexture(kernelSimWaterCurves, "AltitudeRead", altitudeMapRef);     
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        //computeShaderWaterRender.SetTexture(kernelSimWaterCurves, "DensityRead", fluidManager._DensityA);  
        computeShaderWaterRender.Dispatch(kernelSimWaterCurves, waterCurveStrokesCBuffer.count / 1024, 1, 1);
    }
    
    private void SimWaterChains() {
        // Set position of trail Roots:
        int kernelCSPinWaterChainsData = computeShaderWaterRender.FindKernel("CSPinWaterChainsData");        
        computeShaderWaterRender.SetBuffer(kernelCSPinWaterChainsData, "waterChainsReadCBuffer", waterChains0CBuffer);
        computeShaderWaterRender.SetBuffer(kernelCSPinWaterChainsData, "waterChainsWriteCBuffer", waterChains1CBuffer);
        computeShaderWaterRender.SetTexture(kernelCSPinWaterChainsData, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderWaterRender.Dispatch(kernelCSPinWaterChainsData, waterChains0CBuffer.count / numPointsPerWaterChain / 1024, 1, 1);
        
        if(debugFrameCounter % 1 == 0) {
            // Shift positions:::
            int kernelCSShiftWaterChainsData = computeShaderWaterRender.FindKernel("CSShiftWaterChainsData");
            computeShaderWaterRender.SetBuffer(kernelCSShiftWaterChainsData, "waterChainsReadCBuffer", waterChains0CBuffer);
            computeShaderWaterRender.SetBuffer(kernelCSShiftWaterChainsData, "waterChainsWriteCBuffer", waterChains1CBuffer);
            computeShaderWaterRender.SetTexture(kernelCSShiftWaterChainsData, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
            computeShaderWaterRender.Dispatch(kernelCSShiftWaterChainsData, waterChains0CBuffer.count / 1024, 1, 1);
        }      
        
        // Copy back to buffer1:::        
        int kernelCSSwapWaterChainsData = computeShaderWaterRender.FindKernel("CSSwapWaterChainsData");
        computeShaderWaterRender.SetBuffer(kernelCSSwapWaterChainsData, "waterChainsReadCBuffer", waterChains1CBuffer);
        computeShaderWaterRender.SetBuffer(kernelCSSwapWaterChainsData, "waterChainsWriteCBuffer", waterChains0CBuffer);
        computeShaderWaterRender.SetTexture(kernelCSSwapWaterChainsData, "VelocityRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderWaterRender.Dispatch(kernelCSSwapWaterChainsData, waterChains0CBuffer.count / 1024, 1, 1);

        debugFrameCounter++;
    }
    
    public void StartCursorClick(Vector4 clickPos) {
        cursorClickWorldPos = clickPos;
        cursorClickWaveOn = 1f;
        cursorClickWaveTimeStepCounter = 0; // might be redundant, but just to be safe
        //cursorClickWaveOn = 0f;
    //private int cursorClickWaveDuration;
    //private int cursorClickWaveTimeStepCounter = 0;
    }
    
    public void RequestNewWaterRipple(Vector2 coords) 
    {
        WaterRippleData data = new WaterRippleData();
        data.coords = coords;
        data.startTime = Time.realtimeSinceStartup;

        waterRipplesDataArray[nextWaterRippleIndex] = data;
        nextWaterRippleIndex++;
        if(nextWaterRippleIndex >= numWaterRipples) {  // cycle through the array, reusing each
            nextWaterRippleIndex = 0;
        }

        waterRipplesCBuffer.SetData(waterRipplesDataArray);
    }
    
    private void SimWaterSurface()
    {
        int kernelCSUpdateWaterSurface = computeShaderWaterRender.FindKernel("CSUpdateWaterSurface");
        computeShaderWaterRender.SetBuffer(kernelCSUpdateWaterSurface, "waterRippleDataCBuffer", waterRipplesCBuffer);
        computeShaderWaterRender.SetTexture(kernelCSUpdateWaterSurface, "waterSurfaceDataWriteRT", waterSurfaceDataRT0);
        computeShaderWaterRender.SetTexture(kernelCSUpdateWaterSurface, "PressureRead", fluidManagerRef._VelocityPressureDivergenceMain);
        computeShaderWaterRender.SetFloat("_TextureResolution", waterSurfaceDataRT0.width);
        computeShaderWaterRender.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetFloat("_CamDistNormalized", camDistNormalized);
        computeShaderWaterRender.SetVector("_CursorClickWorldPos", cursorClickWorldPos);
        computeShaderWaterRender.SetFloat("_CursorClickTimeLerp", Mathf.Clamp01((float)cursorClickWaveTimeStepCounter / (float)cursorClickWaveDuration));
        computeShaderWaterRender.SetFloat("_CursorClickWaveOn", cursorClickWaveOn);        
        computeShaderWaterRender.Dispatch(kernelCSUpdateWaterSurface, waterSurfaceMapResolution / 32, waterSurfaceMapResolution / 32, 1);
        
        int kernelCSCalculateWaterSurfaceNormals = computeShaderWaterRender.FindKernel("CSCalculateWaterSurfaceNormals");
        computeShaderWaterRender.SetTexture(kernelCSCalculateWaterSurfaceNormals, "waterSurfaceDataReadRT", waterSurfaceDataRT0);
        computeShaderWaterRender.SetTexture(kernelCSCalculateWaterSurfaceNormals, "waterSurfaceDataWriteRT", waterSurfaceDataRT1);
        computeShaderWaterRender.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderWaterRender.SetFloat("_CamDistNormalized", camDistNormalized);
        computeShaderWaterRender.SetFloat("_TextureResolution", (float)waterSurfaceMapResolution);
        computeShaderWaterRender.Dispatch(kernelCSCalculateWaterSurfaceNormals, waterSurfaceMapResolution / 32, waterSurfaceMapResolution / 32, 1);

        Graphics.Blit(waterSurfaceDataRT1, waterSurfaceDataRT0);
    }

    public override void Tick(RenderTexture maskTex) 
    {
        if(cursorClickWaveOn > 0.5f) {
            cursorClickWaveTimeStepCounter++;

            //Debug.Log("ON! " + cursorClickWorldPos.ToString());

            if(cursorClickWaveTimeStepCounter > cursorClickWaveDuration) {
                cursorClickWaveTimeStepCounter = 0;
                cursorClickWaveOn = 0f;
            }
        }

        SimWaterQuads();
        SimWaterCurves();
        SimWaterSurface();
    }

    public override void RenderCommands(ref CommandBuffer cmdBuffer, int frameBufferID) {
        /*
        testStrokesDisplayMat.SetPass(0);
        testStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        testStrokesDisplayMat.SetBuffer("frameBufferStrokesCBuffer", testStrokesCBuffer);  
        testStrokesDisplayMat.SetTexture("_Altitude", )
        testStrokesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        // Use this technique for Environment Brushstrokes:
        cmdBuffer.SetGlobalTexture("_RenderedSceneRT", frameBufferID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBuffer.DrawProcedural(Matrix4x4.identity, testStrokesDisplayMat, 0, MeshTopology.Triangles, 6, testStrokesCBuffer.count);

        */
        /*// Create RenderTargets:
        int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        cmdBuffer.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        cmdBuffer.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBuffer.SetRenderTarget(renderTarget);  // Set render Target
        //cmdBuffer.ClearRenderTarget(true, true, Color.green, 1.0f);  // clear -- needed???
        //cmdBufferMainRender.ClearRenderTarget(true, true, new Color(225f / 255f, 217f / 255f, 200f / 255f), 1.0f);  // clear -- needed???
        */
        // FLUID ITSELF:
        //fluidRenderMat.SetPass(0);
        //fluidRenderMat.SetTexture("_MainTex", fluidManager._DensityA);
        //fluidRenderMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        //fluidRenderMat.SetTexture("_PressureTex", fluidManager._PressureA);
        //fluidRenderMat.SetTexture("_DivergenceTex", fluidManager._Divergence);
        //fluidRenderMat.SetTexture("_ObstaclesTex", fluidManager._ObstaclesRT);
        //fluidRenderMat.SetTexture("_TerrainHeightTex", terrainHeightMap);
        //cmdBuffer.DrawMesh(fluidRenderMesh, Matrix4x4.identity, fluidRenderMat);
        /*
        // BACKGROUND STROKES:::
        // Can't chare same material
        testStrokesDisplayMat.SetPass(0);
        testStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        testStrokesDisplayMat.SetBuffer("frameBufferStrokesCBuffer", testStrokesCBuffer);    
        testStrokesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        // Use this technique for Environment Brushstrokes:
        cmdBuffer.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBuffer.DrawProcedural(Matrix4x4.identity, testStrokesDisplayMat, 0, MeshTopology.Triangles, 6, testStrokesCBuffer.count);
          */      
    }
    
    public void DisplayNutrients()
    {
        waterNutrientsBitsDisplayMat.SetPass(0);
        waterNutrientsBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterNutrientsBitsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", waterNutrientsBitsCBuffer);
        waterNutrientsBitsDisplayMat.SetTexture("_AltitudeTex", terrain.terrainHeightDataRT);
        waterNutrientsBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        waterNutrientsBitsDisplayMat.SetTexture("_WaterSurfaceTex", waterSurfaceDataRT1);
        waterNutrientsBitsDisplayMat.SetTexture("_ResourceGridTex", simulation.vegetationManager.resourceGridRT1);
        waterNutrientsBitsDisplayMat.SetTexture("_TerrainColorTex", terrain.terrainColorRT0);
        waterNutrientsBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterNutrientsBitsDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        waterNutrientsBitsDisplayMat.SetFloat("_CamDistNormalized", camDistNormalized);
        waterNutrientsBitsDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        //baronVonWater.waterNutrientsBitsDisplayMat.SetFloat("_NutrientDensity", Mathf.Clamp01(simManager.simResourceManager.curGlobalNutrients / 300f));
        //cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, waterNutrientsBitsDisplayMat, 0, MeshTopology.Triangles, 6, waterNutrientsBitsCBuffer.count);
    }

    public override void Cleanup() 
    {
        quadVerticesCBuffer?.Release();
        waterCurveVerticesCBuffer?.Release();
        waterCurveStrokesCBuffer?.Release();
        waterQuadStrokesCBufferLrg?.Release();
        waterQuadStrokesCBufferSml?.Release();
        waterNutrientsBitsCBuffer?.Release();
        waterSurfaceBitsCBuffer?.Release();
        waterRipplesCBuffer?.Release();
        //waterSurfaceBitsShadowsCBuffer?.Release();
        waterDebrisBitsCBuffer?.Release();
        waterDebrisBitsShadowsCBuffer?.Release();
        waterChains0CBuffer?.Release();
        waterChains1CBuffer?.Release();
        
        if(waterSurfaceDataRT0 != null)
        {
            waterSurfaceDataRT0.Release();
        }
        if (waterSurfaceDataRT1 != null)
        {
            waterSurfaceDataRT1.Release();
        }
    }
}
