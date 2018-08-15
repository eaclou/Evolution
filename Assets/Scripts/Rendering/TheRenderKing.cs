using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TheRenderKing : MonoBehaviour {

    // SET IN INSPECTOR!!!::::
    public EnvironmentFluidManager fluidManager;
    public SimulationManager simManager;

    public BaronVonTerrain baronVonTerrain;
    public BaronVonWater baronVonWater;

    public Camera mainRenderCam;
    public Camera fluidObstaclesRenderCamera;
    public Camera fluidColorRenderCamera;

    private CommandBuffer cmdBufferTest;
    private CommandBuffer cmdBufferPrimary;
    private CommandBuffer cmdBufferMainRender;
    private CommandBuffer cmdBufferFluidObstacles;
    private CommandBuffer cmdBufferFluidColor;

    public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderUberChains;
    public ComputeShader computeShaderCritters;
    //public ComputeShader computeShaderTerrainGeneration;

    public Material agentEyesDisplayMat;
    public Material curveStrokeDisplayMat;
    public Material trailStrokeDisplayMat;
    //public Material frameBufferStrokeDisplayMat;
    public Material basicStrokeDisplayMat;
    public Material fluidBackgroundColorMat;
    public Material floatyBitsDisplayMat;
    public Material floatyBitsShadowDisplayMat;
    public Material ripplesDisplayMat;
    public Material fluidRenderMat;
    public Material foodProceduralDisplayMat;
    public Material predatorProceduralDisplayMat;
    public Material foodStemDisplayMat;
    public Material foodLeafDisplayMat;
    public Material foodFruitDisplayMat;
    public Material agentBodyDisplayMat;
    public Material playerGlowyBitsDisplayMat;
    public Material playerGlowMat; // soft glow to indicate it is the one player is controlling!
    public Material fadeToBlackBlitMat;
    public Material waterSplinesMat;
    public Material waterChainsMat;
    public Material uberFlowChainBrushMat1;
    public Material debugAgentResourcesMat;
    public Material testSwimmingBodyMat;
    
    public Material critterEnergyDotsMat;
    public Material critterFoodDotsMat;
    public Material foodParticleDisplayMat;
    public Material foodParticleShadowDisplayMat;
    public Material critterSkinStrokesDisplayMat;
    public Material critterShadowStrokesDisplayMat;
    //public Material critterEyeStrokesDisplayMat;

    public bool isDebugRenderOn = true;
    
    //public Material debugMat;

    private Mesh fluidRenderMesh;

    //private RenderTexture primaryRT;

    private bool isInitialized = false;

    private const float velScale = 0.17f; // Conversion for rigidBody Vel --> fluid vel units ----  // approx guess for now

    /*public GameObject terrainGO;
    public Material terrainObstaclesHeightMaskMat;
    public Texture2D terrainHeightMap;         
    public struct TriangleIndexData {
        public int v1;
        public int v2;
        public int v3;
    }
    private Mesh terrainMesh;
    
    private ComputeBuffer terrainVertexCBuffer;
    private ComputeBuffer terrainUVCBuffer;
    private ComputeBuffer terrainNormalCBuffer;
    private ComputeBuffer terrainColorCBuffer;
    private ComputeBuffer terrainTriangleCBuffer; 
    */
    //PointStrokeData[] pointStrokeDataArray;
    CurveStrokeData[] agentSmearStrokesDataArray; // does this need to be global???
    //TrailStrokeData[] trailStrokeDataArray;
    
    private ComputeBuffer quadVerticesCBuffer;  // quad mesh
    private ComputeBuffer agentBodyStrokesCBuffer;

    private int numCurveRibbonQuads = 6;
    private ComputeBuffer curveRibbonVerticesCBuffer;  // short ribbon mesh
    private ComputeBuffer agentSmearStrokesCBuffer;

    private int numBodyQuads = 16;
    private ComputeBuffer bodySwimAnimVerticesCBuffer;

    private ComputeBuffer agentEyeStrokesCBuffer;

    private ComputeBuffer agentTrailStrokes0CBuffer;
    private ComputeBuffer agentTrailStrokes1CBuffer;
    private int numTrailPointsPerAgent = 16;

    //private int numFrameBufferStrokesPerDimension = 512;
    //private ComputeBuffer frameBufferStrokesCBuffer;

    private int numStrokesPerCritterSkin = 128;
    private ComputeBuffer critterSkinStrokesCBuffer;

    //private int numStrokesPerCritterShadow = 4;
    //private ComputeBuffer critterShadowStrokesCBuffer;

    private int numEnergyDotsPerCritter = 32;
    private ComputeBuffer critterEnergyDotsCBuffer;
    private int numFoodDotsPerCritter = 32;
    private ComputeBuffer critterFoodDotsCBuffer;

    private BasicStrokeData[] playerGlowInitPos;
    private ComputeBuffer playerGlowCBuffer;

    private int numPlayerGlowyBits = 1024 * 10;
    private ComputeBuffer playerGlowyBitsCBuffer;

    private int numFloatyBits = 1024 * 512;
    private ComputeBuffer floatyBitsCBuffer;
        
    private int numRipplesPerAgent = 8;
    private ComputeBuffer ripplesCBuffer;

    private int numWaterSplineMeshQuads = 4;
    private ComputeBuffer waterSplineVerticesCBuffer;  // short ribbon mesh
    private int numWaterSplines = 1024 * 3;
    private ComputeBuffer waterSplinesCBuffer;

    private int numWaterChains = 1024 * 3;
    private int numPointsPerWaterChain = 16;
    private ComputeBuffer waterChains0CBuffer;
    private ComputeBuffer waterChains1CBuffer;

    private UberFlowChainBrush uberFlowChainBrush1;

    private BasicStrokeData[] obstacleStrokeDataArray;
    private ComputeBuffer obstacleStrokesCBuffer;

    private BasicStrokeData[] colorInjectionStrokeDataArray;
    private ComputeBuffer colorInjectionStrokesCBuffer;

    //private ComputeBuffer debugAgentResourcesCBuffer;

    public Material debugMaterial;
    public Mesh debugMesh;
    public RenderTexture debugRT; // Used to see texture inside editor (inspector)
    
    public Texture2D critterBodyWidthsTex;

    public float fullscreenFade = 1f;
    
    public struct PlayerGlowyBitData {
		public Vector2 coords;
		public Vector2 vel;
        public Vector2 heading;
		public float age;
	}

    public struct FloatyBitData {
		public Vector2 coords;
		public Vector2 vel;
        public Vector2 heading;
		public float age;
	}

    public struct AgentEyeStrokeData {
        public int parentIndex;  // what agent/object is this attached to?
        public Vector2 localPos;
        public Vector2 localDir; // rotation direction
        public Vector2 localScale;
        public Vector3 irisHue;   // RGB color tint
        public Vector3 pupilHue;   // RGB color tint
        public float strength;  // abstraction for pressure of brushstroke + amount of paint 
        public int brushType;  // what texture/mask/brush pattern to use
    }
    public struct AgentBodyStrokeData {
        public int parentIndex;  // what agent/object is this attached to?
        public Vector2 localPos;
        public Vector2 localDir;
        public Vector2 localScale;
        public float strength;  // abstraction for pressure of brushstroke + amount of paint 
        public int brushTypeX;  // what texture/mask/brush pattern to use
        public int brushTypeY;  // what texture/mask/brush pattern to use
    }
    public struct CritterSkinStrokeData {
        public int parentIndex;  // what agent/object is this attached to?
        public int brushType;  // what brush alpha mask?  // just make it random???
        public Vector3 worldPos;
        public Vector3 localPos;
        public Vector3 localDir;
        public Vector2 localScale;
        public float strength;  // abstraction for pressure of brushstroke + amount of paint
        public float lifeStatus;
        public float age;
		public float randomSeed;
		public float followLerp;        
    }
    public struct CurveStrokeData {
        public int parentIndex;
        public Vector3 hue;
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;
        public float width;
        public float restLength;
        public float strength;
        public int brushType;
    }   
    public struct WaterSplineData {   // 2 ints, 17 floats
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
    public struct TrailStrokeData {
        public Vector2 worldPos;
    }
    public struct TrailDotData {  // for Ripples (temp)
        public int parentIndex;
        public Vector2 coords01;
        public float age;
        public float initAlpha;
    }
    public struct BasicStrokeData {  // fluidSim Render -- Obstacles + ColorInjection
        public Vector2 worldPos;
        public Vector2 localDir;
        public Vector2 scale;
        public Vector4 color;
    }
    /*public struct FrameBufferStrokeData { // background terrain
        public Vector3 worldPos;
		public Vector2 scale;
		public Vector2 heading;
		public int brushType;
    }  */  

    private int debugFrameCounter = 0;
    
    /* $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$  RENDER PIPELINE PSEUDOCODE!!!  $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
    1) Standard main camera beauty pass finishes -- Renders Environment & Background objects -- store result in RT to be sampled later by brushstroke shaders
    2) Background pass Brushstrokes
    3) Fluid Display + extra brushstrokes
    4+) Dynamic Objects Brushstrokes -- Agents, Food, Predators, floatyBits etc.
    *///$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$  RENDER PIPELINE PSEUDOCODE!!!  $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

    private void Awake() {
        fluidObstaclesRenderCamera.enabled = false;
        fluidColorRenderCamera.enabled = false;
    }
    // Use this for initialization:
    public void InitializeRiseAndShine(SimulationManager simManager) {
        //primaryRT = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);        
        //primaryRT.wrapMode = TextureWrapMode.Clamp;
        //primaryRT.enableRandomWrite = true;
        //primaryRT.Create();

        //Debug.Log("Quaternion in z: " + Quaternion.AngleAxis(45f, new Vector3(0f, 0f, 1f)));

        //simManager.cameraManager.camera.targetTexture = primaryRT;

        this.simManager = simManager;

        // temp bodyWidthsTexture:
        critterBodyWidthsTex = new Texture2D(16, simManager._NumAgents, TextureFormat.RGBAFloat, false, true);
        
        InitializeBuffers();
        InitializeMaterials();
        InitializeUberBrushes();
        InitializeCommandBuffers();
        
        baronVonTerrain.Initialize();
        baronVonWater.Initialize();

        //InitializeTerrain();

        //AlignFrameBufferStrokesToTerrain();

        for(int i = 0; i < simManager._NumFood; i++) {
            UpdateDynamicFoodBuffers(i);
        }
        /*for(int i = 0; i < 32; i++) {
            UpdateDynamicFoodBuffers(false, i);
        }*/

        isInitialized = true;  // we did it, guys!
    }
    /*
    private void AlignFrameBufferStrokesToTerrain() {
        int kernelCSAlignFrameBufferStrokes = computeShaderBrushStrokes.FindKernel("CSAlignFrameBufferStrokes");
        computeShaderBrushStrokes.SetTexture(kernelCSAlignFrameBufferStrokes, "terrainHeightTex", terrainHeightMap);
        computeShaderBrushStrokes.SetBuffer(kernelCSAlignFrameBufferStrokes, "terrainFrameBufferStrokesCBuffer", frameBufferStrokesCBuffer);
        computeShaderBrushStrokes.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderBrushStrokes.Dispatch(kernelCSAlignFrameBufferStrokes, frameBufferStrokesCBuffer.count, 1, 1);
    }*/
    // Actual mix of rendering passes will change!!! 
    private void InitializeBuffers() {  // primary function -- calls sub-functions for initializing each buffer
            
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering            
        InitializeCurveRibbonMeshBuffer(); // Set up Curve Ribbon Mesh billboard for brushStroke rendering
        InitializeWaterSplineMeshBuffer(); // same for water splines
        InitializeFluidRenderMesh();
        InitializeBodySwimAnimMeshBuffer(); // test movementAnimation
        
        obstacleStrokesCBuffer = new ComputeBuffer(simManager._NumAgents + simManager._NumFood + simManager._NumPredators, sizeof(float) * 10);
        obstacleStrokeDataArray = new BasicStrokeData[obstacleStrokesCBuffer.count];

        colorInjectionStrokesCBuffer = new ComputeBuffer(simManager._NumAgents + simManager._NumFood + simManager._NumPredators, sizeof(float) * 10);
        colorInjectionStrokeDataArray = new BasicStrokeData[colorInjectionStrokesCBuffer.count];

        InitializeAgentBodyStrokesBuffer();
        InitializeCritterSkinStrokesCBuffer();
        InitializeAgentEyeStrokesBuffer();
        InitializeAgentSmearStrokesBuffer();
        InitializeAgentTailStrokesBuffer();
        //InitializeFrameBufferStrokesBuffer();
        InitializePlayerGlowBuffer();
        InitializePlayerGlowyBitsBuffer();
        InitializeFloatyBitsBuffer();
        InitializeRipplesBuffer();
        InitializeWaterSplinesCBuffer();
        InitializeWaterChainsCBuffer();

        //InitializeDebugBuffers(); 


        /*        
        // TRAIL DOTS:
        TrailDotData[] trailDotsDataArray = new TrailDotData[numTrailDotsPerAgent * agentSimDataCBuffer.count];
        for (int i = 0; i < agentSimDataCBuffer.count; i++) {
            for (int t = 0; t < numTrailDotsPerAgent; t++) {
                TrailDotData data = new TrailDotData();
                data.parentIndex = i;
                data.coords01 = new Vector2((agentSimDataArray[i].worldPos.x + 70f) / 140f, (agentSimDataArray[i].worldPos.y + 70f) / 140f);
                data.age = (float)t / (float)numTrailDotsPerAgent;
                data.initAlpha = 1f;
                trailDotsDataArray[i * numTrailDotsPerAgent + t] = data;
            }
        }
        trailDotsCBuffer = new ComputeBuffer(numTrailDotsPerAgent * agentSimDataCBuffer.count, sizeof(int) + sizeof(float) * 4);
        int kernelSimTrailDots = computeShaderFluidSim.FindKernel("SimTrailDots");
        computeShaderFluidSim.SetBuffer(kernelSimTrailDots, "AgentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderFluidSim.SetBuffer(kernelSimTrailDots, "TrailDotsCBuffer", trailDotsCBuffer);
        trailDotsCBuffer.SetData(trailDotsDataArray);
        */
    }
    private void InitializeCurveRibbonMeshBuffer() {
        
        float rowSize = 1f / (float)numCurveRibbonQuads;

        curveRibbonVerticesCBuffer = new ComputeBuffer(6 * numCurveRibbonQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[curveRibbonVerticesCBuffer.count];
        for(int i = 0; i < numCurveRibbonQuads; i++) {
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

        curveRibbonVerticesCBuffer.SetData(verticesArray);
    }
    private void InitializeWaterSplineMeshBuffer() {
        
        float rowSize = 1f / (float)numWaterSplineMeshQuads;

        waterSplineVerticesCBuffer = new ComputeBuffer(6 * numWaterSplineMeshQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[waterSplineVerticesCBuffer.count];
        for(int i = 0; i < numWaterSplineMeshQuads; i++) {
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

        waterSplineVerticesCBuffer.SetData(verticesArray);
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
    private void InitializeFluidRenderMesh() {
        fluidRenderMesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0f, 0f, 0f);
        vertices[1] = new Vector3(SimulationManager._MapSize, 0f, 0f);
        vertices[2] = new Vector3(0f, SimulationManager._MapSize, 0f);
        vertices[3] = new Vector3(SimulationManager._MapSize, SimulationManager._MapSize, 0f);

        Vector2[] uvs = new Vector2[4] {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        int[] triangles = new int[6] {
            0, 3, 1, 0, 2, 3
        };

        fluidRenderMesh.vertices = vertices;
        fluidRenderMesh.uv = uvs;
        fluidRenderMesh.triangles = triangles;
    }
    private void InitializeBodySwimAnimMeshBuffer() {
        
        float rowSize = 1f / (float)numBodyQuads;

        bodySwimAnimVerticesCBuffer = new ComputeBuffer(6 * numBodyQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[bodySwimAnimVerticesCBuffer.count];
        for(int i = 0; i < numBodyQuads; i++) {
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

        bodySwimAnimVerticesCBuffer.SetData(verticesArray);
    }

    private void InitializeUberBrushes() {
        uberFlowChainBrush1 = new UberFlowChainBrush();
        uberFlowChainBrush1.Initialize(computeShaderUberChains, uberFlowChainBrushMat1);
    }
    //playerGlowCBuffer
    private void InitializePlayerGlowBuffer() {
        playerGlowInitPos = new BasicStrokeData[1];
        playerGlowCBuffer = new ComputeBuffer(1, sizeof(float) * 10);
        Vector3 agentPos = simManager.agentsArray[0].transform.position;
        playerGlowInitPos[0].worldPos = new Vector2(agentPos.x, agentPos.y);
        playerGlowInitPos[0].localDir = simManager.agentsArray[0].facingDirection;
        playerGlowInitPos[0].scale = new Vector2(simManager.agentsArray[0].transform.localScale.x, simManager.agentsArray[0].transform.localScale.y); // ** revisit this later // should leave room for velSampling around Agent
        
        playerGlowInitPos[0].color = new Vector4(simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.x, 
                                                 simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.y,
                                                 simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.z, 1f);

        playerGlowCBuffer.SetData(playerGlowInitPos);
        //int kernelSimPlayerGlow = fluidManager.computeShaderFluidSim.FindKernel("SimPlayerGlowyBits");
        //fluidManager.computeShaderFluidSim.SetBuffer(kernelSimPlayerGlowyBits, "PlayerGlowyBitsCBuffer", playerGlowyBitsCBuffer);

    }
    private void InitializePlayerGlowyBitsBuffer() {
        PlayerGlowyBitData[] playerGlowyBitsInitPos = new PlayerGlowyBitData[numPlayerGlowyBits];
        playerGlowyBitsCBuffer = new ComputeBuffer(numPlayerGlowyBits, sizeof(float) * 7);
        for(int i = 0; i < numPlayerGlowyBits; i++) {
            PlayerGlowyBitData data = new PlayerGlowyBitData();
            data.coords = new Vector2(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f)); // (UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
            data.vel = new Vector2(1f, 0f);
            data.heading = new Vector2(1f, 0f);
            data.age = (float)i / (float)numPlayerGlowyBits * 256f;
            playerGlowyBitsInitPos[i] = data;
        }
        playerGlowyBitsCBuffer.SetData(playerGlowyBitsInitPos);
        int kernelSimPlayerGlowyBits = fluidManager.computeShaderFluidSim.FindKernel("SimPlayerGlowyBits");
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimPlayerGlowyBits, "PlayerGlowyBitsCBuffer", playerGlowyBitsCBuffer);

    }
    private void InitializeFloatyBitsBuffer() {
        FloatyBitData[] floatyBitsInitPos = new FloatyBitData[numFloatyBits];
        floatyBitsCBuffer = new ComputeBuffer(numFloatyBits, sizeof(float) * 7);
        for(int i = 0; i < numFloatyBits; i++) {
            //floatyBitsInitPos[i] = new Vector4(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
            FloatyBitData data = new FloatyBitData();
            data.coords = new Vector2(UnityEngine.Random.Range(0.25f, 0.35f), UnityEngine.Random.Range(0.65f, 0.75f)); // (UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
            data.vel = new Vector2(1f, 0f);
            data.heading = new Vector2(1f, 0f);
            int numGroups = 4;
            int randGroup = UnityEngine.Random.Range(0, numGroups);
            float startGroupAge = (float)randGroup / (float)numGroups;
            data.age = startGroupAge; // (float)i / (float)numFloatyBits;
            floatyBitsInitPos[i] = data;
        }
        floatyBitsCBuffer.SetData(floatyBitsInitPos);
        int kernelSimFloatyBits = fluidManager.computeShaderFluidSim.FindKernel("SimFloatyBits");
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);

    }
    private void InitializeRipplesBuffer() {
        // RIPPLES:
        TrailDotData[] ripplesDataArray = new TrailDotData[numRipplesPerAgent * simManager.simStateData.agentSimDataCBuffer.count];
        for (int i = 0; i < simManager.simStateData.agentSimDataCBuffer.count; i++) {
            for(int t = 0; t < numRipplesPerAgent; t++) {
                TrailDotData data = new TrailDotData();
                data.parentIndex = i;
                data.coords01 = new Vector2(simManager.simStateData.agentSimDataArray[i].worldPos.x / SimulationManager._MapSize, simManager.simStateData.agentSimDataArray[i].worldPos.y / SimulationManager._MapSize);
                data.age = (float)t / (float)numRipplesPerAgent;
                data.initAlpha = 0f;
                ripplesDataArray[i * numRipplesPerAgent + t] = data;
            }
        }
        ripplesCBuffer = new ComputeBuffer(numRipplesPerAgent * simManager.simStateData.agentSimDataCBuffer.count, sizeof(int) + sizeof(float) * 4);
        int kernelSimRipples = fluidManager.computeShaderFluidSim.FindKernel("SimRipples");        
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimRipples, "AgentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimRipples, "RipplesCBuffer", ripplesCBuffer);
        ripplesCBuffer.SetData(ripplesDataArray);
    }
    /*private void InitializeFrameBufferStrokesBuffer() {
        frameBufferStrokesCBuffer = new ComputeBuffer(numFrameBufferStrokesPerDimension * numFrameBufferStrokesPerDimension, sizeof(float) * 7 + sizeof(int));
        FrameBufferStrokeData[] frameBufferStrokesArray = new FrameBufferStrokeData[frameBufferStrokesCBuffer.count];
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
    public void InitializeAgentEyeStrokesBuffer() {
        agentEyeStrokesCBuffer = new ComputeBuffer(simManager._NumAgents * 2, sizeof(float) * 13 + sizeof(int) * 2); // pointStrokeData size
        AgentEyeStrokeData[] agentEyesDataArray = new AgentEyeStrokeData[agentEyeStrokesCBuffer.count];        
        for (int i = 0; i < simManager._NumAgents; i++) {
            AgentEyeStrokeData dataLeftEye = new AgentEyeStrokeData();
            dataLeftEye.parentIndex = i;
            dataLeftEye.localPos = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localPos;
            dataLeftEye.localPos.x *= -1f; // LEFT SIDE!
            float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((dataLeftEye.localPos.y * 0.5f + 0.5f) * 15f)];
            dataLeftEye.localPos.x *= width * 0.5f;
            dataLeftEye.localDir = new Vector2(0f, 1f);
            dataLeftEye.localScale = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localScale;
            dataLeftEye.irisHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.irisHue;
            dataLeftEye.pupilHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
            dataLeftEye.strength = 1f;
            dataLeftEye.brushType = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;

            AgentEyeStrokeData dataRightEye = new AgentEyeStrokeData();
            dataRightEye.parentIndex = i;
            dataRightEye.localPos = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localPos;
            width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((dataRightEye.localPos.y * 0.5f + 0.5f) * 15f)];
            dataRightEye.localPos.x *= width * 0.5f;
            dataRightEye.localDir = new Vector2(0f, 1f);
            dataRightEye.localScale = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localScale;
            dataRightEye.irisHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.irisHue;
            dataRightEye.pupilHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
            dataRightEye.strength = 1f;
            dataRightEye.brushType = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;
            
            agentEyesDataArray[i * 2 + 0] = dataLeftEye;
            agentEyesDataArray[i * 2 + 1] = dataRightEye;
        }
        agentEyeStrokesCBuffer.SetData(agentEyesDataArray);
    }
    public void InitializeAgentBodyStrokesBuffer() {
        agentBodyStrokesCBuffer = new ComputeBuffer(simManager._NumAgents, sizeof(float) * 7 + sizeof(int) * 3);
        AgentBodyStrokeData[] agentBodyStrokesArray = new AgentBodyStrokeData[agentBodyStrokesCBuffer.count];
        for (int i = 0; i < agentBodyStrokesArray.Length; i++) {
            AgentBodyStrokeData bodyStroke = new AgentBodyStrokeData();
            bodyStroke.parentIndex = i;
            bodyStroke.localPos = Vector2.zero;
            bodyStroke.localDir = new Vector2(0f, 1f); // start up? shouldn't matter
            bodyStroke.localScale = Vector2.one; // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio;
            bodyStroke.strength = 1f;
            bodyStroke.brushTypeX = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.bodyStrokeBrushTypeX; // ** Revisit
            bodyStroke.brushTypeY = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;
        }        
        agentBodyStrokesCBuffer.SetData(agentBodyStrokesArray);
    }
    public void InitializeCritterSkinStrokesCBuffer() {
        critterSkinStrokesCBuffer = new ComputeBuffer(simManager._NumAgents * numStrokesPerCritterSkin, sizeof(float) * 16 + sizeof(int) * 2);
        CritterSkinStrokeData[] critterSkinStrokesArray = new CritterSkinStrokeData[critterSkinStrokesCBuffer.count];
        for (int i = 0; i < simManager._NumAgents; i++) {
            for(int j = 0; j < numStrokesPerCritterSkin; j++) {
                CritterSkinStrokeData skinStroke = new CritterSkinStrokeData();
                skinStroke.parentIndex = i;
                skinStroke.brushType = 0; // ** Revisit

                skinStroke.worldPos = new Vector3(SimulationManager._MapSize / 2f, SimulationManager._MapSize / 2f, 0f);

                float zCoord = (1f - ((float)j / (float)(numStrokesPerCritterSkin - 1))) * 2f - 1f;
                float radiusAtZ = Mathf.Sqrt(1f - zCoord * zCoord);
                Vector2 xyCoords = UnityEngine.Random.insideUnitCircle.normalized * radiusAtZ; // possibility for (0,0) ??? ***** undefined/null divide by zero hazard!
                skinStroke.localPos = new Vector3(xyCoords.x, xyCoords.y, zCoord);
                float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((skinStroke.localPos.y * 0.5f + 0.5f) * 15f)];
                skinStroke.localPos.x *= width * 0.5f;
                skinStroke.localPos.z *= width * 0.5f;                
                skinStroke.localDir = new Vector3(0f, 1f, 0f); // start up? shouldn't matter
                skinStroke.localScale = new Vector2(0.25f, 0.420f) * 1.25f; // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio;
                skinStroke.strength = UnityEngine.Random.Range(0f, 1f);
                skinStroke.lifeStatus = 0f;
                skinStroke.age = UnityEngine.Random.Range(1f, 2f);
                skinStroke.randomSeed = UnityEngine.Random.Range(0f, 1f);
                skinStroke.followLerp = 1f;

                critterSkinStrokesArray[i * numStrokesPerCritterSkin + j] = skinStroke;
            }
        }        
        critterSkinStrokesCBuffer.SetData(critterSkinStrokesArray);



        // ENERGY DOTS:::::
        critterEnergyDotsCBuffer = new ComputeBuffer(simManager._NumAgents * numEnergyDotsPerCritter, sizeof(float) * 16 + sizeof(int) * 2);
        CritterSkinStrokeData[] energyDotsArray = new CritterSkinStrokeData[critterEnergyDotsCBuffer.count];
        for (int i = 0; i < simManager._NumAgents; i++) {
            for(int j = 0; j < numEnergyDotsPerCritter; j++) {
                CritterSkinStrokeData energyDot = new CritterSkinStrokeData();
                energyDot.parentIndex = i;
                energyDot.brushType = 0; // ** Revisit
                energyDot.worldPos = new Vector3(0f, 0f, 0f);
                energyDot.localPos = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((energyDot.localPos.y * 0.5f + 0.5f) * 15f)];
                energyDot.localPos.x *= width * 0.5f;
                energyDot.localPos.z *= width * 0.5f;
                energyDot.localPos *= 0.67f;
                energyDot.localDir = new Vector3(0f, 1f, 0f); // start up? shouldn't matter
                energyDot.localScale = new Vector2(0.1f, 0.1f);
                energyDot.strength = UnityEngine.Random.Range(0f, 1f);
                energyDot.lifeStatus = 0f;

                energyDot.age = UnityEngine.Random.Range(1f, 2f);
                energyDot.randomSeed = UnityEngine.Random.Range(0f, 1f);
                energyDot.followLerp = 1f;

                energyDotsArray[i * numEnergyDotsPerCritter + j] = energyDot;
            }
        }        
        critterEnergyDotsCBuffer.SetData(energyDotsArray);


        // FOOD DOTS::::
        critterFoodDotsCBuffer = new ComputeBuffer(simManager._NumAgents * numFoodDotsPerCritter, sizeof(float) * 16 + sizeof(int) * 2);
        CritterSkinStrokeData[] foodDotsArray = new CritterSkinStrokeData[critterFoodDotsCBuffer.count];
        for (int i = 0; i < simManager._NumAgents; i++) {
            for(int j = 0; j < numFoodDotsPerCritter; j++) {
                CritterSkinStrokeData foodDot = new CritterSkinStrokeData();
                foodDot.parentIndex = i;
                foodDot.brushType = 0; // ** Revisit
                foodDot.worldPos = new Vector3(0f, 0f, 0f);
                foodDot.localPos = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)); 
                float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((foodDot.localPos.y * 0.5f + 0.5f) * 15f)];
                foodDot.localPos.x *= width * 0.5f;
                foodDot.localPos.z *= width * 0.5f;
                foodDot.localDir = new Vector3(0f, 1f, 0f); // start up? shouldn't matter
                foodDot.localScale = new Vector2(0.1f, 0.1f); // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio;
                foodDot.strength = UnityEngine.Random.Range(0f, 1f);
                foodDot.lifeStatus = 0f;                
                foodDot.age = UnityEngine.Random.Range(1f, 2f);
                foodDot.randomSeed = UnityEngine.Random.Range(0f, 1f);
                foodDot.followLerp = 1f;

                foodDotsArray[i * numFoodDotsPerCritter + j] = foodDot;
            }
        }        
        critterFoodDotsCBuffer.SetData(foodDotsArray);
    }
    public void InitializeAgentSmearStrokesBuffer() {
        // **** Just Curves to start!!!! ********        
        agentSmearStrokesDataArray = new CurveStrokeData[simManager._NumAgents]; // **** Temporarily just for Agents! ******
        agentSmearStrokesCBuffer = new ComputeBuffer(agentSmearStrokesDataArray.Length, sizeof(float) * 14 + sizeof(int) * 2);

        for (int i = 0; i < agentSmearStrokesDataArray.Length; i++) {
            agentSmearStrokesDataArray[i] = new CurveStrokeData();
            agentSmearStrokesDataArray[i].parentIndex = i; // link to Agent
            agentSmearStrokesDataArray[i].hue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary;

            agentSmearStrokesDataArray[i].restLength = simManager.agentsArray[i].fullSizeBoundingBox.y * 0.25f; // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio.y * 0.25f;

            agentSmearStrokesDataArray[i].p0 = new Vector2(0f, 0f);
            agentSmearStrokesDataArray[i].p1 = agentSmearStrokesDataArray[i].p0 - new Vector2(0f, agentSmearStrokesDataArray[i].restLength);
            agentSmearStrokesDataArray[i].p2 = agentSmearStrokesDataArray[i].p0 - new Vector2(0f, agentSmearStrokesDataArray[i].restLength * 2f);
            agentSmearStrokesDataArray[i].p3 = agentSmearStrokesDataArray[i].p0 - new Vector2(0f, agentSmearStrokesDataArray[i].restLength * 3f);
            agentSmearStrokesDataArray[i].width = simManager.agentsArray[i].fullSizeBoundingBox.x; // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio.x;
            
            agentSmearStrokesDataArray[i].strength = 1f;
            agentSmearStrokesDataArray[i].brushType = 0;
        }        
        agentSmearStrokesCBuffer.SetData(agentSmearStrokesDataArray);
    }
    public void InitializeAgentTailStrokesBuffer() {
        TrailStrokeData[] trailStrokeDataArray = new TrailStrokeData[simManager._NumAgents * numTrailPointsPerAgent];
        for (int i = 0; i < trailStrokeDataArray.Length; i++) {
            int agentIndex = (int)Mathf.Floor((float)i / numTrailPointsPerAgent);
            float trailPos = (float)i % (float)numTrailPointsPerAgent;
            trailStrokeDataArray[i] = new TrailStrokeData();
            trailStrokeDataArray[i].worldPos = new Vector2(0f, trailPos * -1f);
        }
        agentTrailStrokes0CBuffer = new ComputeBuffer(trailStrokeDataArray.Length, sizeof(float) * 2);
        agentTrailStrokes0CBuffer.SetData(trailStrokeDataArray);
        agentTrailStrokes1CBuffer = new ComputeBuffer(trailStrokeDataArray.Length, sizeof(float) * 2);
    }
    public void InitializeWaterSplinesCBuffer() {
        WaterSplineData[] waterSplinesDataArray = new WaterSplineData[numWaterSplines];
        waterSplinesCBuffer = new ComputeBuffer(waterSplinesDataArray.Length, sizeof(float) * 17 + sizeof(int) * 2);

        for(int i = 0; i < waterSplinesDataArray.Length; i++) {
            waterSplinesDataArray[i] = new WaterSplineData();
            waterSplinesDataArray[i].index = i;
            
            waterSplinesDataArray[i].p0 = new Vector2(UnityEngine.Random.Range(-60f, 60f), UnityEngine.Random.Range(-60f, 60f));
            waterSplinesDataArray[i].p1 = waterSplinesDataArray[i].p0 + new Vector2(0f, -1f);
            waterSplinesDataArray[i].p2 = waterSplinesDataArray[i].p0 + new Vector2(0f, -2f);
            waterSplinesDataArray[i].p3 = waterSplinesDataArray[i].p0 + new Vector2(0f, -3f);
            waterSplinesDataArray[i].widths = new Vector4(UnityEngine.Random.Range(0.75f, 1.25f), UnityEngine.Random.Range(0.75f, 1.25f), UnityEngine.Random.Range(0.75f, 1.25f), UnityEngine.Random.Range(0.75f, 1.25f));
            waterSplinesDataArray[i].strength = 1f;  
		    waterSplinesDataArray[i].age = UnityEngine.Random.Range(1f, 2f);
            waterSplinesDataArray[i].brushType = 0; 
        }

        waterSplinesCBuffer.SetData(waterSplinesDataArray);
    }
    public void InitializeWaterChainsCBuffer() {
        TrailStrokeData[] waterChainDataArray = new TrailStrokeData[numWaterChains * numPointsPerWaterChain];
        for (int i = 0; i < waterChainDataArray.Length; i++) {
            int agentIndex = (int)Mathf.Floor((float)i / numPointsPerWaterChain);
            float trailPos = (float)i % (float)numPointsPerWaterChain;
            Vector2 randPos = new Vector2(UnityEngine.Random.Range(-60f, 60f), UnityEngine.Random.Range(-60f, 60f));
            waterChainDataArray[i] = new TrailStrokeData();
            waterChainDataArray[i].worldPos = randPos + new Vector2(0f, trailPos * -1f);
        }
        waterChains0CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
        waterChains0CBuffer.SetData(waterChainDataArray);
        waterChains1CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
    }
    /*private void InitializeDebugBuffers() {
        debugAgentResourcesCBuffer = new ComputeBuffer(simManager._NumAgents, sizeof(float) * 10);
        SimulationStateData.DebugBodyResourcesData[] debugAgentResourcesArray = new SimulationStateData.DebugBodyResourcesData[debugAgentResourcesCBuffer.count];
        for (int i = 0; i < debugAgentResourcesArray.Length; i++) {
            SimulationStateData.DebugBodyResourcesData data = new SimulationStateData.DebugBodyResourcesData();
            data.developmentPercentage = 0f;
            data.energy = 0f;
            data.health = 0f;
            data.isBiting = 0f;
            data.isDamageFrame = 0f;
            data.mouthDimensions = new Vector2(1f, 1f);
            data.mouthOffset = 0f;
            data.stamina = 0f;
            data.stomachContents = 0f;
            debugAgentResourcesArray[i] = data;
        }        
        debugAgentResourcesCBuffer.SetData(debugAgentResourcesArray);
    }*/

    private void InitializeMaterials() {
        agentEyesDisplayMat.SetPass(0); // Eyes
        //pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        agentEyesDisplayMat.SetBuffer("agentEyesStrokesCBuffer", agentEyeStrokesCBuffer);
        agentEyesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        //critterSkinStrokesMat.SetPass(0);
        //critterSkinStrokesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        critterSkinStrokesDisplayMat.SetPass(0);
        critterSkinStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        critterShadowStrokesDisplayMat.SetPass(0);
        critterShadowStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                
        curveStrokeDisplayMat.SetPass(0);
        curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
        curveStrokeDisplayMat.SetBuffer("agentCurveStrokesReadCBuffer", agentSmearStrokesCBuffer); 
                
        /*frameBufferStrokeDisplayMat.SetPass(0);
        frameBufferStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        frameBufferStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        frameBufferStrokeDisplayMat.SetBuffer("frameBufferStrokesCBuffer", frameBufferStrokesCBuffer);
        */
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        playerGlowyBitsDisplayMat.SetPass(0);
        playerGlowyBitsDisplayMat.SetBuffer("playerGlowyBitsCBuffer", playerGlowyBitsCBuffer);
        playerGlowyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        playerGlowyBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        
        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatyBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        floatyBitsShadowDisplayMat.SetPass(0);
        floatyBitsShadowDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatyBitsShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        ripplesDisplayMat.SetPass(0);
        //ripplesDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        ripplesDisplayMat.SetBuffer("trailDotsCBuffer", ripplesCBuffer);
        ripplesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        ripplesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                
        trailStrokeDisplayMat.SetPass(0);
        trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);

        waterSplinesMat.SetPass(0);
        waterSplinesMat.SetBuffer("verticesCBuffer", waterSplineVerticesCBuffer);
        waterSplinesMat.SetBuffer("waterSplinesReadCBuffer", waterSplinesCBuffer);
        waterSplinesMat.SetFloat("_MapSize", SimulationManager._MapSize);

        waterChainsMat.SetPass(0);
        waterChainsMat.SetBuffer("verticesCBuffer", quadVerticesCBuffer);
        waterChainsMat.SetBuffer("waterChainsReadCBuffer", waterChains0CBuffer);
        waterChainsMat.SetFloat("_MapSize", SimulationManager._MapSize);

        debugAgentResourcesMat.SetPass(0);
        debugAgentResourcesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //debugAgentResourcesMat.SetBuffer("indexCBuffer", debugAgentResourcesCBuffer);

        testSwimmingBodyMat.SetPass(0);
        testSwimmingBodyMat.SetTexture("widthsTex", critterBodyWidthsTex);
        testSwimmingBodyMat.SetBuffer("meshVerticesCBuffer", bodySwimAnimVerticesCBuffer);

        critterEnergyDotsMat.SetPass(0);
        critterEnergyDotsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        critterFoodDotsMat.SetPass(0);
        critterFoodDotsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        foodParticleDisplayMat.SetPass(0);
        foodParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        foodParticleShadowDisplayMat.SetPass(0);
        foodParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        /*
        trailDotsDisplayMat.SetPass(0);
        trailDotsDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        trailDotsDisplayMat.SetBuffer("trailDotsCBuffer", trailDotsCBuffer);
        trailDotsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        agentProceduralDisplayMat.SetPass(0);
        agentProceduralDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        agentProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        foodProceduralDisplayMat.SetPass(0);
        foodProceduralDisplayMat.SetBuffer("foodSimDataCBuffer", foodSimDataCBuffer);
        foodProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        predatorProceduralDisplayMat.SetPass(0);
        predatorProceduralDisplayMat.SetBuffer("predatorSimDataCBuffer", predatorSimDataCBuffer);
        predatorProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        */
    }
    private void InitializeCommandBuffers() {

        cmdBufferTest = new CommandBuffer();
        cmdBufferTest.name = "cmdBufferTest";
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferTest);
        /*
        cmdBufferPrimary = new CommandBuffer();
        cmdBufferPrimary.name = "cmdBufferPrimary";
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferPrimary);

        cmdBufferMainRender = new CommandBuffer();
        cmdBufferMainRender.name = "cmdBufferMainRender";
        //mainRenderCam.AddCommandBuffer(CameraEvent.AfterReflections, cmdBufferMainRender);
        */
        cmdBufferFluidObstacles = new CommandBuffer();
        cmdBufferFluidObstacles.name = "cmdBufferFluidObstacles";
        fluidObstaclesRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferFluidObstacles);

        cmdBufferFluidColor = new CommandBuffer();
        cmdBufferFluidColor.name = "cmdBufferFluidColor";
        fluidColorRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferFluidColor);
        //()
        
    }

    /*private void InitializeTerrain() {
        Debug.Log("InitializeTerrain!");

        int meshResolution = 192;
        float mapSize = SimulationManager._MapSize;

        if(terrainGO != null && terrainHeightMap != null) {
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
            computeShaderTerrainGeneration.SetInt("resolutionX", meshResolution);
            computeShaderTerrainGeneration.SetInt("resolutionZ", meshResolution);
            computeShaderTerrainGeneration.SetVector("_QuadBounds", new Vector4(-mapSize * 0.5f, mapSize * 1.5f, -mapSize * 0.5f, mapSize * 1.5f));
            computeShaderTerrainGeneration.SetVector("_HeightRange", new Vector4(-10f, 10f, 0f, 0f));


            // Creates Actual Mesh data by reading from existing main Height Texture!!!!::::::
            int generateMeshDataKernelID = computeShaderTerrainGeneration.FindKernel("CSGenerateMeshData");
            computeShaderTerrainGeneration.SetTexture(generateMeshDataKernelID, "heightTexture", terrainHeightMap);   // Read-Only 

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
        Mesh terrainMesh = new Mesh();

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
    }*/

    public Vector3[] GetDepthAtObjectPositions(Vector4[] positionsArray)
    {

        ComputeBuffer objectDataInFluidCoordsCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 4);
        ComputeBuffer depthValuesCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 3);

        Vector3[] objectDepthsArray = new Vector3[positionsArray.Length];

        objectDataInFluidCoordsCBuffer.SetData(positionsArray);

        int kernelGetObjectDepths = baronVonTerrain.computeShaderTerrainGeneration.FindKernel("CSGetObjectDepths");
        baronVonTerrain.computeShaderTerrainGeneration.SetBuffer(kernelGetObjectDepths, "ObjectPositionsCBuffer", objectDataInFluidCoordsCBuffer);
        baronVonTerrain.computeShaderTerrainGeneration.SetBuffer(kernelGetObjectDepths, "DepthValuesCBuffer", depthValuesCBuffer);
        baronVonTerrain.computeShaderTerrainGeneration.SetTexture(kernelGetObjectDepths, "AltitudeRead", baronVonTerrain.terrainHeightMap);
        //computeShaderFluidSim.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.computeShaderTerrainGeneration.Dispatch(kernelGetObjectDepths, positionsArray.Length, 1, 1);

        depthValuesCBuffer.GetData(objectDepthsArray);

        depthValuesCBuffer.Release();
        objectDataInFluidCoordsCBuffer.Release();

        //Debug.Log("Depth at 0: " + objectDepthsArray[0].ToString());

        return objectDepthsArray;

    }

    private void PopulateObstaclesBuffer() {
        
        int baseIndex = 0;
        // AGENTS:
        for(int i = 0; i < simManager.agentsArray.Length; i++) {
            Vector3 agentPos = simManager.agentsArray[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(agentPos.x, agentPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = simManager.agentsArray[i].facingDirection;
            obstacleStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.agentsArray[i].transform.localScale.x, simManager.agentsArray[i].transform.localScale.y) * 0.9f; // ** revisit this later // should leave room for velSampling around Agent *** weird popping when * 0.9f

            float velX = (agentPos.x - simManager.agentsArray[i]._PrevPos.x) * velScale;
            float velY = (agentPos.y - simManager.agentsArray[i]._PrevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }
        // FOOD:
        baseIndex = simManager.agentsArray.Length;
        for(int i = 0; i < simManager.foodArray.Length; i++) {
            Vector3 foodPos = simManager.foodArray[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(foodPos.x, foodPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = simManager.foodArray[i].facingDirection;
            obstacleStrokeDataArray[baseIndex + i].scale = simManager.foodArray[i].curSize * 0.95f;

            float velX = (foodPos.x - simManager.foodArray[i]._PrevPos.x) * velScale;
            float velY = (foodPos.y - simManager.foodArray[i]._PrevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }
        // PREDATORS:
        baseIndex = simManager.agentsArray.Length + simManager.foodArray.Length;
        for(int i = 0; i < simManager.predatorArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = new Vector2(Mathf.Cos(simManager.predatorArray[i].transform.rotation.z), Mathf.Sin(simManager.predatorArray[i].transform.rotation.z));
            obstacleStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.predatorArray[i].curScale, simManager.predatorArray[i].curScale) * 0.95f;

            float velX = (predatorPos.x - simManager.predatorArray[i]._PrevPos.x) * velScale;
            float velY = (predatorPos.y - simManager.predatorArray[i]._PrevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }

        obstacleStrokesCBuffer.SetData(obstacleStrokeDataArray);
    }
    private void PopulateColorInjectionBuffer() {
        
        int baseIndex = 0;
        // AGENTS:
        for(int i = 0; i < simManager.agentsArray.Length; i++) {
            Vector3 agentPos = simManager.agentsArray[i].transform.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(agentPos.x, agentPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = simManager.agentsArray[i].facingDirection;
            colorInjectionStrokeDataArray[baseIndex + i].scale = simManager.agentsArray[i].fullSizeBoundingBox * 1.0f;
            
            float agentAlpha = 0.024f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                agentAlpha = 2.2f / simManager.agentsArray[i].fullSizeBoundingBox.magnitude;
            }
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                agentAlpha = 3f * Mathf.Clamp01(1f - (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter * 2f / (float)simManager.agentsArray[i]._DecayDurationTimeSteps);
            }
            Color drawColor = new Color(simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary.x, simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary.y, simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary.z, agentAlpha);
            if(simManager.agentsArray[i].wasImpaled) {
                drawColor.r = 0.8f;
                drawColor.g = 0.1f;
                drawColor.b = 0f;
                drawColor.a = 1.4f;
                colorInjectionStrokeDataArray[baseIndex + i].scale *= 1.25f;
            }
            colorInjectionStrokeDataArray[baseIndex + i].color = drawColor;
            
        }
        // FOOD:
        baseIndex = simManager.agentsArray.Length;
        for(int i = 0; i < simManager.foodArray.Length; i++) {
            Vector3 foodPos = simManager.foodArray[i].transform.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(foodPos.x, foodPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = simManager.foodArray[i].facingDirection;
            colorInjectionStrokeDataArray[baseIndex + i].scale = simManager.foodArray[i].curSize * 1.0f;
            
            float foodAlpha = 0.06f;
            if(simManager.foodArray[i].isBeingEaten > 0.5) {
                foodAlpha = 1.2f;
            }

            colorInjectionStrokeDataArray[baseIndex + i].color = new Vector4(Mathf.Lerp(simManager.foodGenomePoolArray[i].fruitHue.x, 0.1f, 0.7f), Mathf.Lerp(simManager.foodGenomePoolArray[i].fruitHue.y, 0.9f, 0.7f), Mathf.Lerp(simManager.foodGenomePoolArray[i].fruitHue.z, 0.2f, 0.7f), foodAlpha);
        }
        // PREDATORS:
        baseIndex = simManager.agentsArray.Length + simManager.foodArray.Length;
        for(int i = 0; i < simManager.predatorArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = new Vector2(Mathf.Cos(simManager.predatorArray[i].transform.rotation.z), Mathf.Sin(simManager.predatorArray[i].transform.rotation.z));
            colorInjectionStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.predatorArray[i].curScale, simManager.predatorArray[i].curScale) * 0.9f;
            
            colorInjectionStrokeDataArray[baseIndex + i].color = new Vector4(1f, 0.25f, 0f, 0.2f);
        }

        colorInjectionStrokesCBuffer.SetData(colorInjectionStrokeDataArray);
    }
    
    public void UpdateAgentWidthsTexture(Agent agent) {
        for(int i = 0; i < agent.agentWidthsArray.Length; i++) {
            critterBodyWidthsTex.SetPixel(i, agent.index, new Color(agent.agentWidthsArray[i], agent.agentWidthsArray[i], agent.agentWidthsArray[i]));
        }
        critterBodyWidthsTex.Apply();
    }

    public void UpdateAgentBodyStrokesBuffer(int agentIndex) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        ComputeBuffer singleAgentBodyStrokeCBuffer = new ComputeBuffer(1, sizeof(float) * 7 + sizeof(int) * 3);
        AgentBodyStrokeData[] singleBodyStrokeArray = new AgentBodyStrokeData[1];
        
        singleBodyStrokeArray[0] = new AgentBodyStrokeData();
        singleBodyStrokeArray[0].parentIndex = agentIndex; // link to Agent
        singleBodyStrokeArray[0].localPos = Vector2.zero;
        singleBodyStrokeArray[0].localDir = new Vector2(0f, 1f); // start up? shouldn't matter
        singleBodyStrokeArray[0].localScale = Vector2.one; // simManager.agentGenomePoolArray[agentIndex].bodyGenome.sizeAndAspectRatio;
        singleBodyStrokeArray[0].strength = 1f;
        singleBodyStrokeArray[0].brushTypeX = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.bodyStrokeBrushTypeX;
        singleBodyStrokeArray[0].brushTypeY = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;
        
        singleAgentBodyStrokeCBuffer.SetData(singleBodyStrokeArray);

        int kernelCSUpdateBodyStrokeDataAgentIndex = computeShaderBrushStrokes.FindKernel("CSUpdateBodyStrokeDataAgentIndex");        
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateBodyStrokeDataAgentIndex, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateBodyStrokeDataAgentIndex, "agentBodyStrokesReadCBuffer", singleAgentBodyStrokeCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateBodyStrokeDataAgentIndex, "agentBodyStrokesWriteCBuffer", agentBodyStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateBodyStrokeDataAgentIndex, 1, 1, 1);
        
        singleAgentBodyStrokeCBuffer.Release();        
    }    
    public void UpdateAgentEyeStrokesBuffer(int agentIndex) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        ComputeBuffer singleAgentEyeStrokeCBuffer = new ComputeBuffer(2, sizeof(float) * 13 + sizeof(int) * 2);
        AgentEyeStrokeData[] singleAgentEyeStrokeArray = new AgentEyeStrokeData[singleAgentEyeStrokeCBuffer.count];        
        
        AgentEyeStrokeData dataLeftEye = new AgentEyeStrokeData();
        dataLeftEye.parentIndex = agentIndex;
        dataLeftEye.localPos = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.localPos;
        dataLeftEye.localPos.x *= -1f; // LEFT SIDE!
        float width = simManager.agentsArray[agentIndex].agentWidthsArray[Mathf.RoundToInt((dataLeftEye.localPos.y * 0.5f + 0.5f) * 15f)];
        dataLeftEye.localPos.x *= width * 0.5f;
        dataLeftEye.localDir = new Vector2(0f, 1f);
        dataLeftEye.localScale = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.localScale;
        dataLeftEye.irisHue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.irisHue;
        dataLeftEye.pupilHue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
        dataLeftEye.strength = 1f;
        dataLeftEye.brushType = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;

        AgentEyeStrokeData dataRightEye = new AgentEyeStrokeData();
        dataRightEye.parentIndex = agentIndex;
        dataRightEye.localPos = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.localPos;
        width = simManager.agentsArray[agentIndex].agentWidthsArray[Mathf.RoundToInt((dataRightEye.localPos.y * 0.5f + 0.5f) * 15f)];
        dataRightEye.localPos.x *= width * 0.5f;
        dataRightEye.localDir = new Vector2(0f, 1f);
        dataRightEye.localScale = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.localScale;
        dataRightEye.irisHue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.irisHue;
        dataRightEye.pupilHue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
        dataRightEye.strength = 1f;
        dataRightEye.brushType = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;
            
        singleAgentEyeStrokeArray[0] = dataLeftEye;
        singleAgentEyeStrokeArray[1] = dataRightEye;
        
        singleAgentEyeStrokeCBuffer.SetData(singleAgentEyeStrokeArray);

        int kernelCSUpdateEyeStrokeDataAgentIndex = computeShaderBrushStrokes.FindKernel("CSUpdateEyeStrokeDataAgentIndex");        
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateEyeStrokeDataAgentIndex, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateEyeStrokeDataAgentIndex, "agentEyeStrokesReadCBuffer", singleAgentEyeStrokeCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateEyeStrokeDataAgentIndex, "agentEyeStrokesWriteCBuffer", agentEyeStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateEyeStrokeDataAgentIndex, 1, 1, 1);
        
        singleAgentEyeStrokeCBuffer.Release();  
        
        //AgentEyeStrokeData[] agentEyesDataArray = new AgentEyeStrokeData[agentEyeStrokesCBuffer.count];
        //agentEyeStrokesCBuffer.GetData(agentEyesDataArray);
        //Debug.Log(" " + agentEyesDataArray[agentIndex].parentIndex.ToString() + ", pos: " + agentEyesDataArray[agentIndex].localPos.ToString());
    }
    public void UpdateAgentSmearStrokesBuffer(int agentIndex) {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!

        ComputeBuffer singleAgentSmearStrokeCBuffer = new ComputeBuffer(1, sizeof(float) * 14 + sizeof(int) * 2);
        CurveStrokeData[] singleCurveStrokeArray = new CurveStrokeData[1];
        
        singleCurveStrokeArray[0] = new CurveStrokeData();
        singleCurveStrokeArray[0].parentIndex = agentIndex; // link to Agent
        singleCurveStrokeArray[0].hue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.huePrimary;

        singleCurveStrokeArray[0].restLength = simManager.agentsArray[agentIndex].fullSizeBoundingBox.y * 0.25f;  // simManager.agentGenomePoolArray[agentIndex].bodyGenome.sizeAndAspectRatio.y * 0.25f;

        singleCurveStrokeArray[0].p0 = new Vector2(simManager.agentsArray[agentIndex]._PrevPos.x, simManager.agentsArray[agentIndex]._PrevPos.y);
        singleCurveStrokeArray[0].p1 = singleCurveStrokeArray[0].p0 - new Vector2(0f, singleCurveStrokeArray[0].restLength);
        singleCurveStrokeArray[0].p2 = singleCurveStrokeArray[0].p0 - new Vector2(0f, singleCurveStrokeArray[0].restLength * 2f);
        singleCurveStrokeArray[0].p3 = singleCurveStrokeArray[0].p0 - new Vector2(0f, singleCurveStrokeArray[0].restLength * 3f);
        singleCurveStrokeArray[0].width = simManager.agentsArray[agentIndex].fullSizeBoundingBox.x; //simManager.agentGenomePoolArray[agentIndex].bodyGenome.sizeAndAspectRatio.x;
        
        singleCurveStrokeArray[0].strength = 1f;
        singleCurveStrokeArray[0].brushType = 0;
               
        singleAgentSmearStrokeCBuffer.SetData(singleCurveStrokeArray);

        int kernelCSUpdateCurveBrushDataAgentIndex = computeShaderBrushStrokes.FindKernel("CSUpdateCurveBrushDataAgentIndex");

        computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateCurveBrushDataAgentIndex, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateCurveBrushDataAgentIndex, "agentCurveStrokesReadCBuffer", singleAgentSmearStrokeCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateCurveBrushDataAgentIndex, "agentCurveStrokesWriteCBuffer", agentSmearStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateCurveBrushDataAgentIndex, 1, 1, 1);
        
        singleAgentSmearStrokeCBuffer.Release();

        //Debug.Log("Update curve Strokes! [" + singleCurveStrokeArray[0].p0.ToString() + ", " + singleCurveStrokeArray[0].p1.ToString() + ", " + singleCurveStrokeArray[0].p2.ToString() + ", " + singleCurveStrokeArray[0].p3.ToString() + "]");
    }    
    public void UpdateAgentTailStrokesBuffer(int agentIndex) {

    }
    public void UpdateDynamicFoodBuffers(int foodIndex) {
        //if(isPlant) {
        ComputeBuffer singleStemCBuffer = new ComputeBuffer(1, sizeof(float) * 7 + sizeof(int) * 1);
        SimulationStateData.StemData[] singleStemDataArray = new SimulationStateData.StemData[1];        
        singleStemDataArray[0] = new SimulationStateData.StemData();
        singleStemDataArray[0].foodIndex = foodIndex;
        singleStemDataArray[0].localBaseCoords = new Vector2(0f, -1f);
        singleStemDataArray[0].localTipCoords = new Vector2(0f, 1f);
        singleStemDataArray[0].childGrowth = 0f;
        singleStemDataArray[0].width = simManager.foodGenomePoolArray[foodIndex].stemWidth;
        singleStemDataArray[0].attached = 1f;
        singleStemCBuffer.SetData(singleStemDataArray);
        int kernelCSUpdateDynamicStemBuffers = computeShaderBrushStrokes.FindKernel("CSUpdateDynamicStemBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicStemBuffers, "foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicStemBuffers, "foodStemDataUpdateCBuffer", singleStemCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicStemBuffers, "foodStemDataWriteCBuffer", simManager.simStateData.foodStemDataCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateDynamicStemBuffers, 1, 1, 1);        
        singleStemCBuffer.Release();

        // *** Hard-coded 16 leaves per food object!!!! *** BEWARE!!!
        ComputeBuffer foodLeafUpdateCBuffer = new ComputeBuffer(16, sizeof(float) * 7 + sizeof(int) * 1);
        SimulationStateData.LeafData[] foodLeafDataArray = new SimulationStateData.LeafData[16];
        for (int i = 0; i < 16; i++) {
            foodLeafDataArray[i] = new SimulationStateData.LeafData();
            foodLeafDataArray[i].foodIndex = foodIndex;
            foodLeafDataArray[i].worldPos = new Vector3(0f, 0f, 0f);
            foodLeafDataArray[i].localCoords = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
            foodLeafDataArray[i].localScale =  simManager.foodGenomePoolArray[foodIndex].leafScale;          
            foodLeafDataArray[i].attached = 1f;
        }
        foodLeafUpdateCBuffer.SetData(foodLeafDataArray);
        int kernelCSUpdateDynamicLeafBuffers = computeShaderBrushStrokes.FindKernel("CSUpdateDynamicLeafBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicLeafBuffers, "foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicLeafBuffers, "foodLeafDataUpdateCBuffer", foodLeafUpdateCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicLeafBuffers, "foodLeafDataWriteCBuffer", simManager.simStateData.foodLeafDataCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateDynamicLeafBuffers, 1, 1, 1);        
        foodLeafUpdateCBuffer.Release();

        // DEBUG ***** RACE CONDITIONS -- NEVER FORGET!!! ********
        // DEBUG ***** RACE CONDITIONS -- NEVER FORGET!!! ********
        //SimulationStateData.LeafData[] testDataArray = new SimulationStateData.LeafData[simManager.simStateData.foodLeafDataCBuffer.count];
        //simManager.simStateData.foodLeafDataCBuffer.GetData(testDataArray);
        //string txt = "";
        //for(int i = 0; i < 32; i++) {
        //    int index = i * 15;
        //    txt += "\n" + (index).ToString() + ", foodIndex: " + testDataArray[index].foodIndex.ToString();
        //}
        //Debug.Log(txt);
        //Debug.Log("foodLeafDataArray length " + foodLeafDataArray.Length.ToString() + " foodLeafDataCBuffer: " + simManager.simStateData.foodLeafDataCBuffer.count.ToString() + ", index: " + foodLeafDataArray[9].foodIndex.ToString());
        // DEBUG ***** RACE CONDITIONS -- NEVER FORGET!!! ********
        // DEBUG ***** RACE CONDITIONS -- NEVER FORGET!!! ********

        // *** Hard-coded 64 Fruits per food object!!!! *** BEWARE!!!
        ComputeBuffer foodFruitUpdateCBuffer = new ComputeBuffer(64, sizeof(float) * 7 + sizeof(int) * 1);

        SimulationStateData.FruitData[] foodFruitDataArray = new SimulationStateData.FruitData[64];
        for(int i = 0; i < 64; i++) {
            foodFruitDataArray[i] = new SimulationStateData.FruitData();
            foodFruitDataArray[i].foodIndex = foodIndex;
            foodFruitDataArray[i].localCoords = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 0.5f + UnityEngine.Random.insideUnitCircle * 0.4f;
            foodFruitDataArray[i].localScale = simManager.foodGenomePoolArray[foodIndex].fruitScale;  
            foodFruitDataArray[i].worldPos = simManager.foodArray[foodIndex].transform.position;
            foodFruitDataArray[i].attached = 1f;
        }        
        foodFruitUpdateCBuffer.SetData(foodFruitDataArray);
        int kernelCSUpdateDynamicFruitBuffers = computeShaderBrushStrokes.FindKernel("CSUpdateDynamicFruitBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicFruitBuffers, "foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicFruitBuffers, "foodFruitDataUpdateCBuffer", foodFruitUpdateCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicFruitBuffers, "foodFruitDataWriteCBuffer", simManager.simStateData.foodFruitDataCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateDynamicFruitBuffers, 1, 1, 1);        
        foodFruitUpdateCBuffer.Release();
        //}
        /*else {
            ComputeBuffer foodFruitUpdateCBuffer = new ComputeBuffer(64, sizeof(float) * 7 + sizeof(int) * 1);

            SimulationStateData.FruitData[] foodFruitDataArray = new SimulationStateData.FruitData[64];
            for(int i = 0; i < 64; i++) {
                foodFruitDataArray[i] = new SimulationStateData.FruitData();
                foodFruitDataArray[i].foodIndex = simManager._NumFood + foodIndex;
                foodFruitDataArray[i].localCoords = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 0.5f + UnityEngine.Random.insideUnitCircle * 0.4f;
                foodFruitDataArray[i].localScale = simManager.foodGenomePoolArray[0].fruitScale;  
                foodFruitDataArray[i].worldPos = simManager.foodDeadAnimalArray[foodIndex].transform.position;
                foodFruitDataArray[i].attached = 1f;
            }        
            foodFruitUpdateCBuffer.SetData(foodFruitDataArray);
            int kernelCSUpdateDynamicFruitBuffers = computeShaderBrushStrokes.FindKernel("CSUpdateDynamicFruitBuffers");
            //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
            computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicFruitBuffers, "foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
            computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicFruitBuffers, "foodFruitDataUpdateCBuffer", foodFruitUpdateCBuffer);
            computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicFruitBuffers, "foodFruitDataWriteCBuffer", simManager.simStateData.foodFruitDataCBuffer);
            computeShaderBrushStrokes.Dispatch(kernelCSUpdateDynamicFruitBuffers, 1, 1, 1);        
            foodFruitUpdateCBuffer.Release();
        }*/
        


    }

    private void SimAgentSmearStrokes() {
        int kernelCSSinglePassCurveBrushData = computeShaderBrushStrokes.FindKernel("CSSinglePassCurveBrushData");
        
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentCurveStrokesWriteCBuffer", agentSmearStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSSinglePassCurveBrushData, agentSmearStrokesCBuffer.count, 1, 1);        
    }    
    public void SimPlayerGlow() {
        //Vector3 agentPos = simManager.agentsArray[0].transform.position;
        //playerGlowInitPos[0].worldPos = new Vector2(agentPos.x, agentPos.y);
        //playerGlowInitPos[0].localDir = simManager.agentsArray[0].facingDirection;
        playerGlowInitPos[0].scale = new Vector2(simManager.agentsArray[0].transform.localScale.x, simManager.agentsArray[0].transform.localScale.y); // ** revisit this later // should leave room for velSampling around Agent
        
        playerGlowInitPos[0].color = new Vector4(simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.x, 
                                                 simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.y,
                                                 simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.z, 1f);

        playerGlowCBuffer.SetData(playerGlowInitPos);
    }
    public void SimPlayerGlowyBits() {
        playerGlowyBitsDisplayMat.SetVector("_PrimaryHue", new Vector4(simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.x,
                                                                       simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.y,
                                                                       simManager.agentGenomePoolArray[0].bodyGenome.appearanceGenome.huePrimary.z,
                                                                       0f));
        playerGlowyBitsDisplayMat.SetFloat("_PosX", simManager.agentsArray[0].bodyRigidbody.transform.position.x / SimulationManager._MapSize);
        playerGlowyBitsDisplayMat.SetFloat("_PosY", simManager.agentsArray[0].bodyRigidbody.transform.position.y / SimulationManager._MapSize);


        int kernelSimPlayerGlowyBits = fluidManager.computeShaderFluidSim.FindKernel("SimPlayerGlowyBits");

        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);
        fluidManager.computeShaderFluidSim.SetVector("_PlayerPos", new Vector4(simManager.agentsArray[0].bodyRigidbody.transform.position.x / SimulationManager._MapSize, simManager.agentsArray[0].bodyRigidbody.transform.position.y / SimulationManager._MapSize, 0f, 0f));
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimPlayerGlowyBits, "PlayerGlowyBitsCBuffer", playerGlowyBitsCBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimPlayerGlowyBits, "VelocityRead", fluidManager._VelocityA);        
        fluidManager.computeShaderFluidSim.Dispatch(kernelSimPlayerGlowyBits, playerGlowyBitsCBuffer.count / 1024, 1, 1);
    }
    public void SimFloatyBits() {
        int kernelSimFloatyBits = fluidManager.computeShaderFluidSim.FindKernel("SimFloatyBits");

        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimFloatyBits, "VelocityRead", fluidManager._VelocityA);        
        fluidManager.computeShaderFluidSim.Dispatch(kernelSimFloatyBits, floatyBitsCBuffer.count / 1024, 1, 1);
    }
    private void SimRipples() {
        int kernelSimRipples = fluidManager.computeShaderFluidSim.FindKernel("SimRipples");
        
        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);

        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimRipples, "AgentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimRipples, "RipplesCBuffer", ripplesCBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimRipples, "VelocityRead", fluidManager._VelocityA);
        fluidManager.computeShaderFluidSim.Dispatch(kernelSimRipples, ripplesCBuffer.count / 8, 1, 1);
    }
    private void SimFruit() {
        int kernelCSSimulateFruit = computeShaderBrushStrokes.FindKernel("CSSimulateFruit");
        
        computeShaderBrushStrokes.SetTexture(kernelCSSimulateFruit, "velocityRead", fluidManager._VelocityA);
        computeShaderBrushStrokes.SetBuffer(kernelCSSimulateFruit, "foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSSimulateFruit, "foodFruitDataWriteCBuffer", simManager.simStateData.foodFruitDataCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSSimulateFruit, simManager.simStateData.foodFruitDataCBuffer.count / 64, 1, 1);        
    }
    private void IterateTrailStrokesData() {
        // Set position of trail Roots:
        int kernelCSPinRootTrailStrokesData = computeShaderBrushStrokes.FindKernel("CSPinRootTrailStrokesData");        
        computeShaderBrushStrokes.SetBuffer(kernelCSPinRootTrailStrokesData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSPinRootTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSPinRootTrailStrokesData, simManager.simStateData.agentSimDataCBuffer.count, 1, 1);
        computeShaderBrushStrokes.SetBuffer(kernelCSPinRootTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSPinRootTrailStrokesData, simManager.simStateData.agentSimDataCBuffer.count, 1, 1);

        //if(velocityTex != null) {
            // update all trailPoint positions:
        int kernelCSIterateTrailStrokesData = computeShaderBrushStrokes.FindKernel("CSIterateTrailStrokesData");
        // PING:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.SetTexture(kernelCSIterateTrailStrokesData, "velocityRead", fluidManager._VelocityA);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateTrailStrokesData, agentTrailStrokes0CBuffer.count, 1, 1);
        // PONG:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesReadCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.SetTexture(kernelCSIterateTrailStrokesData, "velocityRead", fluidManager._VelocityA);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateTrailStrokesData, agentTrailStrokes0CBuffer.count, 1, 1);

        // PING:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.SetTexture(kernelCSIterateTrailStrokesData, "velocityRead", fluidManager._VelocityA);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateTrailStrokesData, agentTrailStrokes0CBuffer.count, 1, 1);
        // PONG:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesReadCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.SetTexture(kernelCSIterateTrailStrokesData, "velocityRead", fluidManager._VelocityA);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateTrailStrokesData, agentTrailStrokes0CBuffer.count, 1, 1);
        //}
    }
    private void SimWaterSplines() {
        int kernelSimWaterSplines = fluidManager.computeShaderFluidSim.FindKernel("SimWaterSplines");

        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimWaterSplines, "WaterSplinesCBuffer", waterSplinesCBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimWaterSplines, "VelocityRead", fluidManager._VelocityA);     
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimWaterSplines, "DensityRead", fluidManager._DensityA);  
        fluidManager.computeShaderFluidSim.Dispatch(kernelSimWaterSplines, waterSplinesCBuffer.count / 1024, 1, 1);
    }
    private void SimWaterChains() {
        // Set position of trail Roots:
        int kernelCSPinWaterChainsData = fluidManager.computeShaderFluidSim.FindKernel("CSPinWaterChainsData");        
        fluidManager.computeShaderFluidSim.SetBuffer(kernelCSPinWaterChainsData, "waterChainsReadCBuffer", waterChains0CBuffer);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelCSPinWaterChainsData, "waterChainsWriteCBuffer", waterChains1CBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelCSPinWaterChainsData, "VelocityRead", fluidManager._VelocityA);
        fluidManager.computeShaderFluidSim.Dispatch(kernelCSPinWaterChainsData, waterChains0CBuffer.count / numPointsPerWaterChain / 1024, 1, 1);
        
        if(debugFrameCounter % 1 == 0) {
            // Shift positions:::
            int kernelCSShiftWaterChainsData = fluidManager.computeShaderFluidSim.FindKernel("CSShiftWaterChainsData");
            fluidManager.computeShaderFluidSim.SetBuffer(kernelCSShiftWaterChainsData, "waterChainsReadCBuffer", waterChains0CBuffer);
            fluidManager.computeShaderFluidSim.SetBuffer(kernelCSShiftWaterChainsData, "waterChainsWriteCBuffer", waterChains1CBuffer);
            fluidManager.computeShaderFluidSim.SetTexture(kernelCSShiftWaterChainsData, "VelocityRead", fluidManager._VelocityA);
            fluidManager.computeShaderFluidSim.Dispatch(kernelCSShiftWaterChainsData, waterChains0CBuffer.count / 1024, 1, 1);
        }      
        
        // Copy back to buffer1:::        
        int kernelCSSwapWaterChainsData = fluidManager.computeShaderFluidSim.FindKernel("CSSwapWaterChainsData");
        fluidManager.computeShaderFluidSim.SetBuffer(kernelCSSwapWaterChainsData, "waterChainsReadCBuffer", waterChains1CBuffer);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelCSSwapWaterChainsData, "waterChainsWriteCBuffer", waterChains0CBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelCSSwapWaterChainsData, "VelocityRead", fluidManager._VelocityA);
        fluidManager.computeShaderFluidSim.Dispatch(kernelCSSwapWaterChainsData, waterChains0CBuffer.count / 1024, 1, 1);

        debugFrameCounter++;
    }
    private void SimCritterSkinStrokes() {
        int kernelCSCSSimulateCritterSkinStrokes = computeShaderCritters.FindKernel("CSSimulateSkinCritterStrokes");
        
        computeShaderCritters.SetTexture(kernelCSCSSimulateCritterSkinStrokes, "velocityRead", fluidManager._VelocityA);
        computeShaderCritters.SetBuffer(kernelCSCSSimulateCritterSkinStrokes, "critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSCSSimulateCritterSkinStrokes, "critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSCSSimulateCritterSkinStrokes, "critterSkinStrokesWriteCBuffer", critterSkinStrokesCBuffer);
        computeShaderCritters.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderCritters.Dispatch(kernelCSCSSimulateCritterSkinStrokes, critterSkinStrokesCBuffer.count / 16, 1, 1);
    }

    public void Tick() {  // should be called from SimManager at proper time!
        fullscreenFade = 1f;
        if(simManager.agentsArray[0].curLifeStage == Agent.AgentLifeStage.Egg) {
            fullscreenFade = fullscreenFade * (float)simManager.agentsArray[0].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[0]._GestationDurationTimeSteps;
        }
        if(simManager.agentsArray[0].curLifeStage == Agent.AgentLifeStage.Dead) {
            fullscreenFade = fullscreenFade * (1f - (float)simManager.agentsArray[0].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[0]._DecayDurationTimeSteps);
        }
        fadeToBlackBlitMat.SetPass(0);
        fadeToBlackBlitMat.SetFloat("_FadeAmount", fullscreenFade);
        //
        // Read current stateData and update all Buffers, send data to GPU
        // Execute computeShaders to update any dynamic particles that are purely cosmetic
        //SimPlayerGlow();
        //SimAgentSmearStrokes(); // start with this one?
        //IterateTrailStrokesData();
        //SimPlayerGlowyBits();
        //SimFloatyBits();
        //SimRipples();
        SimFruit();
        //SimWaterSplines();
        //SimWaterChains();

        SimCritterSkinStrokes();

        baronVonWater.altitudeMapRef = baronVonTerrain.terrainHeightMap;
        float camDist = Mathf.Clamp01(-1f * simManager.cameraManager.gameObject.transform.position.z / (65f - 5f));
        baronVonWater.camDistNormalized = camDist;
        Vector2 boxSizeHalf = 0.5f * Vector2.Lerp(new Vector2(16f, 9f) * 2, new Vector2(256f, 180f), Mathf.Clamp01(-(simManager.cameraManager.gameObject.transform.position.z) / 150f));
        if(simManager.cameraManager.targetAgent != null)
        {
            baronVonWater.spawnBoundsCameraDetails = new Vector4(simManager.cameraManager.targetAgent.bodyRigidbody.position.x - boxSizeHalf.x,
                                                            simManager.cameraManager.targetAgent.bodyRigidbody.position.y - boxSizeHalf.y,
                                                            simManager.cameraManager.targetAgent.bodyRigidbody.position.x + boxSizeHalf.x,
                                                            simManager.cameraManager.targetAgent.bodyRigidbody.position.y + boxSizeHalf.y);

            
        }
        else
        {
            baronVonWater.spawnBoundsCameraDetails = new Vector4(0f, 0f, SimulationManager._MapSize, SimulationManager._MapSize);
        }
        baronVonTerrain.spawnBoundsCameraDetails = baronVonWater.spawnBoundsCameraDetails;

        baronVonTerrain.Tick();
        baronVonWater.Tick();  // <-- SimWaterCurves/Chains/Water surface

        //uberFlowChainBrush1.Tick(fluidManager._VelocityA);
    }

    public void RenderSimulationCameras() { // **** revisit
        debugRT = fluidManager._SourceColorRT;

        // SOLID OBJECTS OBSTACLES:::
        PopulateObstaclesBuffer();  // update data for obstacles before rendering

        cmdBufferFluidObstacles.Clear(); // needed since camera clear flag is set to none
        cmdBufferFluidObstacles.SetRenderTarget(fluidManager._ObstaclesRT);
        cmdBufferFluidObstacles.ClearRenderTarget(true, true, Color.black, 1.0f);  // clear -- needed???
        cmdBufferFluidObstacles.SetViewProjectionMatrices(fluidObstaclesRenderCamera.worldToCameraMatrix, fluidObstaclesRenderCamera.projectionMatrix);

        // Draw Solid Land boundaries:
        cmdBufferFluidObstacles.DrawMesh(baronVonTerrain.terrainMesh, Matrix4x4.identity, baronVonTerrain.terrainObstaclesHeightMaskMat); // Masks out areas above the fluid "Sea Level"
        // Draw dynamic Obstacles:
        /*
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
        basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", obstacleStrokesCBuffer);        
        cmdBufferFluidObstacles.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, obstacleStrokesCBuffer.count);
            */  // Disabling for now -- starting with one-way interaction between fluid & objects (fluid pushes objects, they don't push back)
            
        Graphics.ExecuteCommandBuffer(cmdBufferFluidObstacles);
        // Still not sure if this will work correctly... ****
        fluidObstaclesRenderCamera.Render(); // is this even needed? all drawcalls taken care of within commandBuffer?


        // COLOR INJECTION:::
        PopulateColorInjectionBuffer(); // update data for colorInjection objects before rendering

        cmdBufferFluidColor.Clear(); // needed since camera clear flag is set to none
        cmdBufferFluidColor.SetRenderTarget(fluidManager._SourceColorRT);
        cmdBufferFluidColor.ClearRenderTarget(true, true, new Color(0f,0f,0f,0f), 1.0f);  // clear -- needed???
        cmdBufferFluidColor.SetViewProjectionMatrices(fluidColorRenderCamera.worldToCameraMatrix, fluidColorRenderCamera.projectionMatrix);
        //cmdBufferFluidColor.Blit(fluidManager.initialDensityTex, fluidManager._SourceColorRT);
        //cmdBufferFluidColor.DrawMesh(fluidRenderMesh, Matrix4x4.identity, fluidBackgroundColorMat); // Simple unlit Texture shader -- wysiwyg

        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", colorInjectionStrokesCBuffer);
        cmdBufferFluidColor.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, colorInjectionStrokesCBuffer.count);
        // Render Agent/Food/Pred colors here!!!
        // just use their display renders?

        Graphics.ExecuteCommandBuffer(cmdBufferFluidColor);

        fluidColorRenderCamera.Render();
        // Update this ^^ to use Graphics.ExecuteCommandBuffer()  ****
    }
    /*private void Render() {
        cmdBufferPrimary.Clear();

        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferPrimary.SetRenderTarget(renderTarget);  // Set render Target
        cmdBufferPrimary.ClearRenderTarget(true, true, Color.yellow, 1.0f);  // clear -- needed???
    }*/
    private void Render() {
        //Debug.Log("TestRenderCommandBuffer()");
        
        
        // To DO:
        // 1) Wall/Rocks standard shader LIT w/ fog
        // 2) Background Brushstrokes
        // 3) Fluid Shader 
        // 4) Either distortion of Fluid to mimic brushstroke or Dedicated strokes that sample from fluid Color
        // 5) Floaty bits in Fluid
        // 6) Ripples/Wakes from movement in fluid
        // 7) Agent Bodies
        // 8) Agent Decorations
        // 9) Agent Trails/Tentacles
        // 10) Bushes/Trees (scaffolding for food)
        // 11) Food objects
        // 12) Predators

        //cmdBufferMainRender.Clear();

        cmdBufferTest.Clear();
        // control render target capture Here?
        // Create RenderTargets:
        int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        cmdBufferTest.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferTest.Blit(renderTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline
        cmdBufferTest.SetRenderTarget(renderTarget);  // Set render Target
        cmdBufferTest.ClearRenderTarget(true, true, Color.black, 1.0f);  // clear -- needed???
                                                                         //cmdBufferMainRender.ClearRenderTarget(true, true, new Color(225f / 255f, 217f / 255f, 200f / 255f), 1.0f);  // clear -- needed???


        //baronVonTerrain.RenderCommands(ref cmdBufferTest, renderedSceneID);
        // GROUND:
        // LARGE STROKES!!!!
        baronVonTerrain.groundStrokesLrgDisplayMat.SetPass(0);
        baronVonTerrain.groundStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonTerrain.groundStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesLrgCBuffer);
        baronVonTerrain.groundStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.groundStrokesLrgDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonTerrain.groundStrokesLrgDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesLrgDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesLrgCBuffer.count);

        // MEDIUM STROKES!!!!
        baronVonTerrain.groundStrokesMedDisplayMat.SetPass(0);
        baronVonTerrain.groundStrokesMedDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonTerrain.groundStrokesMedDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesMedCBuffer);
        baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesMedDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesMedCBuffer.count);

        // SMALL STROKES!!!!
        baronVonTerrain.groundStrokesSmlDisplayMat.SetPass(0);
        baronVonTerrain.groundStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonTerrain.groundStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesSmlCBuffer);
        baronVonTerrain.groundStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.groundStrokesSmlDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonTerrain.groundStrokesSmlDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesSmlDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesSmlCBuffer.count);

        // GROUND BITS:::
        baronVonTerrain.groundBitsDisplayMat.SetPass(0);
        baronVonTerrain.groundBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonTerrain.groundBitsDisplayMat.SetBuffer("groundBitsCBuffer", baronVonTerrain.groundBitsCBuffer);
        baronVonTerrain.groundBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.groundBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonTerrain.groundBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundBitsCBuffer.count);

        // CARPET BITS:: (microbial mats, algae?)
        baronVonTerrain.carpetBitsDisplayMat.SetPass(0);
        baronVonTerrain.carpetBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonTerrain.carpetBitsDisplayMat.SetBuffer("groundBitsCBuffer", baronVonTerrain.carpetBitsCBuffer);
        baronVonTerrain.carpetBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.carpetBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonTerrain.carpetBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonTerrain.carpetBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.carpetBitsCBuffer.count);


        //renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        //cmdBufferTest.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        //cmdBufferTest.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

        /*
        // FOOD PARTICLE SHADOWS::::
        foodParticleShadowDisplayMat.SetPass(0);
        foodParticleShadowDisplayMat.SetBuffer("foodParticleDataCBuffer", simManager.foodParticlesCBuffer);
        foodParticleShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        foodParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        foodParticleShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, foodParticleShadowDisplayMat, 0, MeshTopology.Triangles, 6, simManager.foodParticlesCBuffer.count);
        */
        // Surface Bits Shadows:
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetPass(0);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterSurfaceBitsCBuffer);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonWater.waterSurfaceBitsDisplayMat.SetFloat("_CamDistNormalized", Mathf.Lerp(0f, 1f, Mathf.Clamp01((simManager.cameraManager.gameObject.transform.position.z * -1f) / 100f)));
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterSurfaceBitsShadowsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterSurfaceBitsCBuffer.count);


        // SHADOWS TEST:
        critterShadowStrokesDisplayMat.SetPass(0);
        critterShadowStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        critterShadowStrokesDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        critterShadowStrokesDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        critterShadowStrokesDisplayMat.SetBuffer("critterSkinStrokesCBuffer", critterSkinStrokesCBuffer);
        critterShadowStrokesDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        critterShadowStrokesDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        critterShadowStrokesDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        critterShadowStrokesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterShadowStrokesDisplayMat, 0, MeshTopology.Triangles, 6, critterSkinStrokesCBuffer.count);






        foodParticleDisplayMat.SetPass(0);
        foodParticleDisplayMat.SetBuffer("foodParticleDataCBuffer", simManager.foodParticlesCBuffer);
        foodParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        foodParticleDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, foodParticleDisplayMat, 0, MeshTopology.Triangles, 6, simManager.foodParticlesCBuffer.count);
        
        foodFruitDisplayMat.SetPass(0);
        foodFruitDisplayMat.SetBuffer("fruitDataCBuffer", simManager.simStateData.foodFruitDataCBuffer);
        foodFruitDisplayMat.SetBuffer("foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
        foodFruitDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, foodFruitDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.foodFruitDataCBuffer.count);
        
        // CRITTER BODY:


        // WATER :::::
        //baronVonWater.RenderCommands(ref cmdBufferTest, renderedSceneID);
        // Re-capture FrameBuffer to match backgroundColor
        //int renderedSceneID2 = Shader.PropertyToID("_RenderedSceneID2");
        //cmdBufferTest.SetRenderTarget();
        //cmdBufferTest.GetTemporaryRT(renderedSceneID2, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        //renderTarget = new RenderTargetIdentifier(primaryRT);
        //cmdBufferTest.SetRenderTarget(renderTarget);  // Set render Target
        //cmdBufferTest.Blit(renderTarget, renderedSceneID2);  // save contents of Standard Rendering Pipeline

        //RenderTargetIdentifier waterTargetID = new RenderTargetIdentifier(primaryRT);
        //cmdBufferTest.Blit(waterTargetID, primaryRT);

        
        /*
        // WATER DEBRIS BITS:
        baronVonWater.waterDebrisBitsDisplayMat.SetPass(0);
        baronVonWater.waterDebrisBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterDebrisBitsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterDebrisBitsCBuffer);
        baronVonWater.waterDebrisBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterDebrisBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterDebrisBitsDisplayMat.SetTexture("_WaterDebrisTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterDebrisBitsDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        baronVonWater.waterDebrisBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterDebrisBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterDebrisBitsCBuffer.count);
        
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetPass(0);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterDebrisBitsShadowsCBuffer);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetTexture("_WaterDebrisTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        baronVonWater.waterDebrisBitsShadowsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterDebrisBitsShadowsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterDebrisBitsShadowsCBuffer.count);
        */

        // WATER BITS TEMP::::::::::::::^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        baronVonWater.waterNutrientsBitsDisplayMat.SetPass(0);
        baronVonWater.waterNutrientsBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterNutrientsBitsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterNutrientsBitsCBuffer);
        baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        baronVonWater.waterNutrientsBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterNutrientsBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterNutrientsBitsCBuffer.count);


        // Critter Stomach Bits
        critterFoodDotsMat.SetPass(0);
        critterFoodDotsMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        critterFoodDotsMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        critterFoodDotsMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        critterFoodDotsMat.SetBuffer("bodyStrokesCBuffer", critterFoodDotsCBuffer);
        critterFoodDotsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterFoodDotsMat, 0, MeshTopology.Triangles, 6, critterFoodDotsCBuffer.count);


        // CRITTER SKIN:
        critterSkinStrokesDisplayMat.SetPass(0);
        critterSkinStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        critterSkinStrokesDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        critterSkinStrokesDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        critterSkinStrokesDisplayMat.SetBuffer("critterSkinStrokesCBuffer", critterSkinStrokesCBuffer);     
        critterSkinStrokesDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        critterSkinStrokesDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        critterSkinStrokesDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        critterSkinStrokesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterSkinStrokesDisplayMat, 0, MeshTopology.Triangles, 6, critterSkinStrokesCBuffer.count);

        
        // Critter Energy blops!
        critterEnergyDotsMat.SetPass(0);
        critterEnergyDotsMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        critterEnergyDotsMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        critterEnergyDotsMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        critterEnergyDotsMat.SetBuffer("bodyStrokesCBuffer", critterEnergyDotsCBuffer);
        critterEnergyDotsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterEnergyDotsMat, 0, MeshTopology.Triangles, 6, critterEnergyDotsCBuffer.count);
        

        // AGENT EYES:
        agentEyesDisplayMat.SetPass(0);
        agentEyesDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        agentEyesDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        agentEyesDisplayMat.SetBuffer("agentEyesStrokesCBuffer", agentEyeStrokesCBuffer);
        agentEyesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        agentEyesDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        agentEyesDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        agentEyesDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        agentEyesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, agentEyesDisplayMat, 0, MeshTopology.Triangles, 6, agentEyeStrokesCBuffer.count);
        /*
        // Water surface reflective
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetPass(0);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterQuadStrokesCBufferSml);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonWater.waterQuadStrokesSmlDisplayMat.SetFloat("_CamDistNormalized", Mathf.Lerp(0f, 1f, Mathf.Clamp01((simManager.cameraManager.gameObject.transform.position.z * -1f) / 100f)));
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterQuadStrokesSmlDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterQuadStrokesCBufferSml.count);
        */



        // SURFACE BITS FLOATY:::::
        baronVonWater.waterSurfaceBitsDisplayMat.SetPass(0);
        baronVonWater.waterSurfaceBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterSurfaceBitsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterSurfaceBitsCBuffer);
        baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        baronVonWater.waterSurfaceBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonWater.waterSurfaceBitsDisplayMat.SetFloat("_CamDistNormalized", Mathf.Lerp(0f, 1f, Mathf.Clamp01((simManager.cameraManager.gameObject.transform.position.z * -1f) / 100f)));
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterSurfaceBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterSurfaceBitsCBuffer.count);



        if(isDebugRenderOn) {
            
            
            debugAgentResourcesMat.SetPass(0);
            debugAgentResourcesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            debugAgentResourcesMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
            debugAgentResourcesMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
            //debugAgentResourcesMat.SetBuffer("debugAgentResourcesCBuffer", simManager.simStateData.debugBodyResourcesCBuffer);
            //debugAgentResourcesMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            cmdBufferTest.DrawProcedural(Matrix4x4.identity, debugAgentResourcesMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.critterInitDataCBuffer.count);
            

        }


        /*
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetPass(0);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterQuadStrokesCBufferLrg);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_NutrientTex", simManager.nutrientMapRT1);
        //cmdBufferTest.SetGlobalTexture(("_SceneRT", primaryRT);
        baronVonWater.waterQuadStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        // Use this technique for Environment Brushstrokes:
        //RenderTargetIdentifier customTarget = new RenderTargetIdentifier(primaryRT);
        //cmdBufferTest.Blit(customTarget, renderedSceneID);
        cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        //cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterQuadStrokesLrgDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterQuadStrokesCBufferLrg.count);
        */
        // Detail brushes:


        /*
        // FLOATY BITS!
        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetTexture("_FluidColorTex", fluidManager._DensityA);
        floatyBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferTest.DrawProcedural(Matrix4x4.identity, floatyBitsDisplayMat, 0, MeshTopology.Triangles, 6, floatyBitsCBuffer.count);
        */

        /*
        // WATER SPLINES:::
        baronVonWater.waterCurveStrokeDisplayMat.SetPass(0);
        baronVonWater.waterCurveStrokeDisplayMat.SetBuffer("verticesCBuffer", baronVonWater.waterCurveVerticesCBuffer);
        baronVonWater.waterCurveStrokeDisplayMat.SetBuffer("waterCurveStrokesCBuffer", baronVonWater.waterCurveStrokesCBuffer);
        baronVonWater.waterCurveStrokeDisplayMat.SetTexture("_FluidColorTex", fluidManager._DensityA);
        baronVonWater.waterCurveStrokeDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
        //cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterCurveStrokeDisplayMat, 0, MeshTopology.Triangles, 6 * baronVonWater.numWaterCurveMeshQuads, baronVonWater.waterCurveStrokesCBuffer.count);
        
        // WATER CHAINS:::
        baronVonWater.waterChainStrokeDisplayMat.SetPass(0);
        baronVonWater.waterChainStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", baronVonWater.quadVerticesCBuffer);
        baronVonWater.waterChainStrokeDisplayMat.SetBuffer("waterChainsReadCBuffer", baronVonWater.waterChains0CBuffer);
        baronVonWater.waterChainStrokeDisplayMat.SetTexture("_FluidColorTex", fluidManager._DensityA);
        //cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.waterChainStrokeDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterChains0CBuffer.count);
        */
        //critterSkinStrokesMat.SetPass(0);
        //critterSkinStrokesMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        //critterSkinStrokesMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        //critterSkinStrokesMat.SetBuffer("critterSkinStrokesCBuffer", critterSkinStrokesCBuffer);
        //critterSkinStrokesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);   
        //cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterSkinStrokesMat, 0, MeshTopology.Triangles, 6, 1);
        //CritterSkinStrokeData[] critterBodyStrokesArray = new CritterSkinStrokeData[critterSkinStrokesCBuffer.count];
        //critterSkinStrokesCBuffer.GetData(critterBodyStrokesArray);
        //Debug.Log(critterBodyStrokesArray[0].worldPos.ToString());
        //renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        //cmdBufferTest.Blit(primaryRT, renderTarget);  // save contents of Standard Rendering Pipeline

        // TEMP!
        //baronVonWater.computeShaderWaterRender.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);

        //baronVonWater.testStrokesDisplayMat.SetPass(0);
        //baronVonWater.testStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //baronVonWater.testStrokesDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.testStrokesCBuffer);    
        //baronVonWater.testStrokesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        // Use this technique for Environment Brushstrokes:
        //cmdBufferTest.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        //cmdBufferTest.DrawProcedural(Matrix4x4.identity, baronVonWater.testStrokesDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.testStrokesCBuffer.count);

        /*
        // Create RenderTargets:
        int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        cmdBufferMainRender.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        cmdBufferMainRender.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferMainRender.SetRenderTarget(renderTarget);  // Set render Target
        cmdBufferMainRender.ClearRenderTarget(true, true, Color.black, 1.0f);  // clear -- needed???
        //cmdBufferMainRender.ClearRenderTarget(true, true, new Color(225f / 255f, 217f / 255f, 200f / 255f), 1.0f);  // clear -- needed???
                
        // FLUID ITSELF:
        fluidRenderMat.SetPass(0);
        fluidRenderMat.SetTexture("_DensityTex", fluidManager._DensityA);
        fluidRenderMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        fluidRenderMat.SetTexture("_PressureTex", fluidManager._PressureA);
        fluidRenderMat.SetTexture("_DivergenceTex", fluidManager._Divergence);
        fluidRenderMat.SetTexture("_ObstaclesTex", fluidManager._ObstaclesRT);
        fluidRenderMat.SetTexture("_TerrainHeightTex", terrainHeightMap);
        //cmdBufferMainRender.DrawMesh(fluidRenderMesh, Matrix4x4.identity, fluidRenderMat);

        // Fluid Render Article:
        // http://blog.camposanto.com/post/171934927979/hi-im-matt-wilde-an-old-man-from-the-north-of/amp?__twitter_impression=true
        
        // BACKGROUND STROKES:::
        frameBufferStrokeDisplayMat.SetPass(0);
        frameBufferStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        frameBufferStrokeDisplayMat.SetBuffer("frameBufferStrokesCBuffer", frameBufferStrokesCBuffer);    
        frameBufferStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        // Use this technique for Environment Brushstrokes:
        cmdBufferMainRender.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, frameBufferStrokeDisplayMat, 0, MeshTopology.Triangles, 6, frameBufferStrokesCBuffer.count);
        */

        /*
        // WATER SPLINES:::
        waterSplinesMat.SetPass(0);
        waterSplinesMat.SetBuffer("verticesCBuffer", waterSplineVerticesCBuffer);
        waterSplinesMat.SetBuffer("waterSplinesReadCBuffer", waterSplinesCBuffer);
        waterSplinesMat.SetTexture("_FluidColorTex", fluidManager._DensityA);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, waterSplinesMat, 0, MeshTopology.Triangles, 6 * numWaterSplineMeshQuads, waterSplinesCBuffer.count);
        
        // WATER CHAINS:::
        waterChainsMat.SetPass(0);
        waterChainsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterChainsMat.SetBuffer("waterChainsReadCBuffer", waterChains0CBuffer);
        waterChainsMat.SetTexture("_FluidColorTex", fluidManager._DensityA);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, waterChainsMat, 0, MeshTopology.Triangles, 6, waterChains0CBuffer.count);

        // FLOATY BITS!
        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetTexture("_FluidColorTex", fluidManager._DensityA);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, floatyBitsDisplayMat, 0, MeshTopology.Triangles, 6, floatyBitsCBuffer.count);

        // Uber Chains !!!
        uberFlowChainBrush1.RenderSetup(fluidManager._DensityA);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, uberFlowChainBrush1.renderMat, 0, MeshTopology.Triangles, 6, uberFlowChainBrush1.chains0CBuffer.count);
        */

        /*        
        // RIPPLES:
        ripplesDisplayMat.SetPass(0);
        ripplesDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        ripplesDisplayMat.SetBuffer("trailDotsCBuffer", ripplesCBuffer);
        ripplesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, ripplesDisplayMat, 0, MeshTopology.Triangles, 6, ripplesCBuffer.count);

        // PLAYER GLOW:::
        //if (!simManager.uiManager.isObserverMode) {
        playerGlowMat.SetPass(0);
        playerGlowMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        playerGlowMat.SetBuffer("basicStrokesCBuffer", playerGlowCBuffer);
        playerGlowMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, playerGlowMat, 0, MeshTopology.Triangles, 6, playerGlowCBuffer.count);
        //}
        */

        /*
        // PLAYER GLOWY BITS!
        playerGlowyBitsDisplayMat.SetPass(0);
        //playerGlowyBitsDisplayMat.SetFloat("_PosX", (simManager.agentsArray[0].transform.position.x + 70f) / 140f);
        //playerGlowyBitsDisplayMat.SetFloat("_PosY", (simManager.agentsArray[0].transform.position.y + 70f) / 140f);
        //playerGlowyBitsDisplayMat.SetVector("_PlayerPos", new Vector4((simManager.agentsArray[0].transform.position.x + 70f) / 140f, (simManager.agentsArray[0].transform.position.y + 70f) / 140f, 0f, 0f));
        playerGlowyBitsDisplayMat.SetBuffer("playerGlowyBitsCBuffer", playerGlowyBitsCBuffer);
        playerGlowyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, playerGlowyBitsDisplayMat, 0, MeshTopology.Triangles, 6, playerGlowyBitsCBuffer.count);
        */

        bool displayAgents = true;
        if(displayAgents) {
            
            /*
            // AGENT TAILS WOO!
            trailStrokeDisplayMat.SetPass(0);
            trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            trailStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, trailStrokeDisplayMat, 0, MeshTopology.Triangles, 6, agentTrailStrokes0CBuffer.count);
            */
            /*
            foodFruitDisplayMat.SetPass(0);
            foodFruitDisplayMat.SetBuffer("fruitDataCBuffer", simManager.simStateData.foodFruitDataCBuffer);
            foodFruitDisplayMat.SetBuffer("foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
            foodFruitDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, foodFruitDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.foodFruitDataCBuffer.count);
                */
            /*
            // TEMP AGENTS: // CHANGE THIS TO SMEARS!
            curveStrokeDisplayMat.SetPass(0);
            curveStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
            curveStrokeDisplayMat.SetBuffer("agentCurveStrokesReadCBuffer", agentSmearStrokesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, curveStrokeDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, agentSmearStrokesCBuffer.count);
            */
            /*
            // AGENT BODY:
            agentBodyDisplayMat.SetPass(0);
            agentBodyDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            agentBodyDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            agentBodyDisplayMat.SetBuffer("bodyStrokesCBuffer", agentBodyStrokesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, agentBodyDisplayMat, 0, MeshTopology.Triangles, 6, agentBodyStrokesCBuffer.count);
            */
            // AGENT EYES:
            //agentEyesDisplayMat.SetPass(0);
            //agentEyesDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            //agentEyesDisplayMat.SetBuffer("agentEyesStrokesCBuffer", agentEyeStrokesCBuffer);
            //agentEyesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            //cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, agentEyesDisplayMat, 0, MeshTopology.Triangles, 6, agentEyeStrokesCBuffer.count);
            /*
            predatorProceduralDisplayMat.SetPass(0);
            predatorProceduralDisplayMat.SetBuffer("predatorSimDataCBuffer", simManager.simStateData.predatorSimDataCBuffer);
            predatorProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, predatorProceduralDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.predatorSimDataCBuffer.count);
        */
        
        }

        /*if(isDebugRenderOn) {
            
            
            debugAgentResourcesMat.SetPass(0);
            debugAgentResourcesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            debugAgentResourcesMat.SetBuffer("debugAgentResourcesCBuffer", simManager.simStateData.debugBodyResourcesCBuffer);
            debugAgentResourcesMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, debugAgentResourcesMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.debugBodyResourcesCBuffer.count);
            

        }*/
        /*
        testSwimmingBodyMat.SetPass(0);
        testSwimmingBodyMat.SetBuffer("meshVerticesCBuffer", bodySwimAnimVerticesCBuffer);
        testSwimmingBodyMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        testSwimmingBodyMat.SetBuffer("agentMovementAnimDataCBuffer", simManager.simStateData.agentMovementAnimDataCBuffer);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, testSwimmingBodyMat, 0, MeshTopology.Triangles, 6 * numBodyQuads, simManager.simStateData.agentMovementAnimDataCBuffer.count);
        */

        //foodProceduralDisplayMat.SetPass(0);
        //foodProceduralDisplayMat.SetBuffer("foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
        //foodProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, foodProceduralDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.foodSimDataCBuffer.count);
        //Graphics.DrawProcedural(MeshTopology.Triangles, 6, simManager.simStateData.foodSimDataCBuffer.count);
                
        // DEBUG *****
        //SimulationStateData.LeafData[] testDataArray = new SimulationStateData.LeafData[simManager.simStateData.foodLeafDataCBuffer.count];
        //simManager.simStateData.foodLeafDataCBuffer.GetData(testDataArray);
        //Debug.Log("testDataArray[0] " + testDataArray[0].foodIndex.ToString() + " testDataArray[15] " + testDataArray[15].foodIndex.ToString() + ", testDataArray[570]: " + testDataArray[570].foodIndex.ToString());
           
        //cmdBufferMainRender.Blit()
        /*int mainTexID = Shader.PropertyToID("_MainTex");        
        fadeToBlackBlitMat.SetPass(0);
        fadeToBlackBlitMat.SetFloat("_FadeAmount", fullscreenFade);
        cmdBufferMainRender.GetTemporaryRT(mainTexID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        cmdBufferMainRender.Blit(BuiltinRenderTextureType.CameraTarget, mainTexID);  // save contents of Standard Rendering Pipeline
        cmdBufferMainRender.Blit(mainTexID, BuiltinRenderTextureType.CameraTarget, fadeToBlackBlitMat);
        */
        //Graphics.DrawProcedural(MeshTopology.Triangles, 6, simManager.simStateData.predatorSimDataCBuffer.count);
    }

    private void OnWillRenderObject() {  // requires MeshRenderer Component to be called
        //Debug.Log("OnWillRenderObject()");
        if (isInitialized) {
            Render();
        }
    }
    private void OnRenderObject() { // for direct rendering
        
        if (Camera.current == mainRenderCam && isInitialized) {
            //Render();
        }


            //trailStrokeDisplayMat.SetPass(0);
            //trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
            //trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            //Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentTrailStrokes0CBuffer.count);

            //curveStrokeDisplayMat.SetPass(0);
            //curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
            //curveStrokeDisplayMat.SetBuffer("agentCurveStrokes0CBuffer", agentCurveStrokes0CBuffer);
            //Graphics.DrawProcedural(MeshTopology.Triangles, numCurveRibbonQuads * 6, agentCurveStrokes0CBuffer.count);

            //pointStrokeDisplayMat.SetPass(0);
            //pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            //pointStrokeDisplayMat.SetBuffer("pointStrokesCBuffer", agentPointStrokesCBuffer);
            //pointStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            //Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentPointStrokesCBuffer.count);
        

        // From OLD FluidManager:::
        /*if(Camera.current == mainCam) {
            floatyBitsDisplayMat.SetPass(0);
            floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, floatyBitsCBuffer.count);
            
            ripplesDisplayMat.SetPass(0);
            ripplesDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            ripplesDisplayMat.SetBuffer("trailDotsCBuffer", ripplesCBuffer);
            ripplesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, ripplesCBuffer.count);
                        
            trailDotsDisplayMat.SetPass(0);
            trailDotsDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            trailDotsDisplayMat.SetBuffer("trailDotsCBuffer", trailDotsCBuffer);
            trailDotsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, trailDotsCBuffer.count);

            agentProceduralDisplayMat.SetPass(0);
            agentProceduralDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentSimDataCBuffer.count);
            
            foodProceduralDisplayMat.SetPass(0);
            foodProceduralDisplayMat.SetBuffer("foodSimDataCBuffer", foodSimDataCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, foodSimDataCBuffer.count);

            predatorProceduralDisplayMat.SetPass(0);
            predatorProceduralDisplayMat.SetBuffer("predatorSimDataCBuffer", predatorSimDataCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, predatorSimDataCBuffer.count);
        }  */
    }
    private void OnDisable() {
        if(mainRenderCam != null) {
            mainRenderCam.RemoveAllCommandBuffers();
        }
        if(fluidColorRenderCamera != null) {
            fluidColorRenderCamera.RemoveAllCommandBuffers();
        }
        if(fluidObstaclesRenderCamera != null) {
            fluidObstaclesRenderCamera.RemoveAllCommandBuffers();
        }

        if(baronVonTerrain != null) {
            baronVonTerrain.Cleanup();
        }
        if(baronVonWater != null) {
            baronVonWater.Cleanup();
        }

        if(cmdBufferTest != null) {
            cmdBufferTest.Release();
        }
        if(cmdBufferPrimary != null) {
            cmdBufferPrimary.Release();
        }
        if(cmdBufferMainRender != null) {
            cmdBufferMainRender.Release();
        }
        if(cmdBufferFluidObstacles != null) {
            cmdBufferFluidObstacles.Release();
        }
        if(cmdBufferFluidColor != null) {
            cmdBufferFluidColor.Release();
        }
        
        if (agentBodyStrokesCBuffer != null) {
            agentBodyStrokesCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        if (agentSmearStrokesCBuffer != null) {
            agentSmearStrokesCBuffer.Release();
        }
        if (curveRibbonVerticesCBuffer != null) {
            curveRibbonVerticesCBuffer.Release();
        }
        if (agentTrailStrokes0CBuffer != null) {
            agentTrailStrokes0CBuffer.Release();
        }
        if (agentTrailStrokes1CBuffer != null) {
            agentTrailStrokes1CBuffer.Release();
        }
        //if (frameBufferStrokesCBuffer != null) {
        //    frameBufferStrokesCBuffer.Release();
        //}
        if (obstacleStrokesCBuffer != null) {
            obstacleStrokesCBuffer.Release();
        } 
        if (colorInjectionStrokesCBuffer != null) {
            colorInjectionStrokesCBuffer.Release();
        }
        if (playerGlowCBuffer != null) {
            playerGlowCBuffer.Release();
        }
        if (playerGlowyBitsCBuffer != null) {
            playerGlowyBitsCBuffer.Release();
        } 
        if (floatyBitsCBuffer != null) {
            floatyBitsCBuffer.Release();
        } 
        if (ripplesCBuffer != null) {
            ripplesCBuffer.Release();
        }
        if (agentEyeStrokesCBuffer != null) {
            agentEyeStrokesCBuffer.Release();
        }
        if (waterSplinesCBuffer != null) {
            waterSplinesCBuffer.Release();
        }
        if (waterSplineVerticesCBuffer != null) {
            waterSplineVerticesCBuffer.Release();
        }
        if (waterChains0CBuffer != null) {
            waterChains0CBuffer.Release();
        }
        if (waterChains1CBuffer != null) {
            waterChains1CBuffer.Release();
        }

        if(uberFlowChainBrush1 != null) {
            uberFlowChainBrush1.CleanUp();
        }  
        
        if(bodySwimAnimVerticesCBuffer != null) {
            bodySwimAnimVerticesCBuffer.Release();
        }
        if(critterSkinStrokesCBuffer != null) {
            critterSkinStrokesCBuffer.Release();
        }
        
        if(critterEnergyDotsCBuffer != null) {
            critterEnergyDotsCBuffer.Release();
        }
         if(critterFoodDotsCBuffer != null) {
            critterFoodDotsCBuffer.Release();
        }
    }

    /*public PointStrokeData GeneratePointStrokeData(int index, Vector2 size, Vector2 pos, Vector2 dir, Vector3 hue, float str, int brushType) {
        PointStrokeData pointStroke = new PointStrokeData();
        pointStroke.parentIndex = index;
        pointStroke.localScale = size;
        pointStroke.localPos = pos;
        pointStroke.localDir = dir;
        pointStroke.hue = hue;
        pointStroke.strength = str; // temporarily used to lerp btw primary & secondary Agent Hues
        pointStroke.brushType = brushType;

        return pointStroke;
    }*/
    
    /*public void InitializeAllAgentCurveData() {
        ComputeBuffer agentInitializeCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(int));
        int[] agentsToInitArray = new int[agentInitializeCBuffer.count];
        for(int i = 0; i < agentsToInitArray.Length; i++) {
            agentsToInitArray[i] = i;
        }
        agentInitializeCBuffer.SetData(agentsToInitArray);

        int kernelCSInitializeCurveBrushData = computeShaderBrushStrokes.FindKernel("CSInitializeCurveBrushData");        
        computeShaderBrushStrokes.SetBuffer(kernelCSInitializeCurveBrushData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSInitializeCurveBrushData, "agentInitializeCBuffer", agentInitializeCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSInitializeCurveBrushData, "agentCurveStrokesWriteCBuffer", agentCurveStrokes0CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSInitializeCurveBrushData, agentInitializeCBuffer.count, 1, 1);

        agentInitializeCBuffer.Release();
    }
    public void InitializeAgentCurveData(int index) {
        ComputeBuffer agentInitializeCBuffer = new ComputeBuffer(1, sizeof(int));
        int[] agentsToInitArray = new int[1];
        agentsToInitArray[0] = index;
        
        agentInitializeCBuffer.SetData(agentsToInitArray);

        int kernelCSInitializeCurveBrushData = computeShaderBrushStrokes.FindKernel("CSInitializeCurveBrushData");
        computeShaderBrushStrokes.SetBuffer(kernelCSInitializeCurveBrushData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSInitializeCurveBrushData, "agentInitializeCBuffer", agentInitializeCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSInitializeCurveBrushData, "agentCurveStrokesWriteCBuffer", agentCurveStrokes0CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSInitializeCurveBrushData, agentInitializeCBuffer.count, 1, 1);

        agentInitializeCBuffer.Release();
    }

    private void IterateCurveBrushData() {
        int kernelCSIterateCurveBrushData = computeShaderBrushStrokes.FindKernel("CSIterateCurveBrushData");
        // PING:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateCurveBrushData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateCurveBrushData, "agentCurveStrokesReadCBuffer", agentCurveStrokes0CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateCurveBrushData, "agentCurveStrokesWriteCBuffer", agentCurveStrokes1CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateCurveBrushData, agentCurveStrokes0CBuffer.count, 1, 1);
        // PONG:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateCurveBrushData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateCurveBrushData, "agentCurveStrokesReadCBuffer", agentCurveStrokes1CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateCurveBrushData, "agentCurveStrokesWriteCBuffer", agentCurveStrokes0CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateCurveBrushData, agentCurveStrokes0CBuffer.count, 1, 1);
    }*/
    /*

    private void SimTrailDots() {
        int kernelSimTrailDots = computeShaderFluidSim.FindKernel("SimTrailDots");
        
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);

        computeShaderFluidSim.SetBuffer(kernelSimTrailDots, "AgentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderFluidSim.SetBuffer(kernelSimTrailDots, "TrailDotsCBuffer", trailDotsCBuffer);
        computeShaderFluidSim.SetTexture(kernelSimTrailDots, "VelocityRead", velocityA);
        computeShaderFluidSim.Dispatch(kernelSimTrailDots, trailDotsCBuffer.count / 8, 1, 1);
    }
    */
    /*public void SetSimDataArrays() {

        for (int i = 0; i < agentSimDataArray.Length - 1; i++) {
            agentSimDataArray[i].worldPos = new Vector2(agentsArray[i].transform.position.x, agentsArray[i].transform.position.y);
            agentSimDataArray[i].velocity = agentsArray[i].smoothedThrottle; // new Vector2(agentsArray[i].testModule.ownRigidBody2D.velocity.x, agentsArray[i].testModule.ownRigidBody2D.velocity.y);
            agentSimDataArray[i].heading = agentsArray[i].facingDirection; // new Vector2(0f, 1f); // Update later -- store inside Agent class? 
            agentSimDataArray[i].size = agentsArray[i].size;
        } // Player:
        agentSimDataArray[agentSimDataArray.Length - 1].worldPos = new Vector2(playerAgent.transform.position.x, playerAgent.transform.position.y);
        agentSimDataArray[agentSimDataArray.Length - 1].velocity = playerAgent.smoothedThrottle;
        agentSimDataArray[agentSimDataArray.Length - 1].heading = playerAgent.facingDirection;
        agentSimDataArray[agentSimDataArray.Length - 1].size = playerAgent.size;
        agentSimDataCBuffer.SetData(agentSimDataArray);        
    }*/
    /*public void SetPointStrokesBuffer() {
        
        int numDecorationStrokes = 8;
        int numPointStrokes = (65) * (1 + numDecorationStrokes + 2);

        if (agentPointStrokesCBuffer == null) {
            agentPointStrokesCBuffer = new ComputeBuffer(numPointStrokes, sizeof(int) * 2 + sizeof(float) * 10);
        }
        if (pointStrokeDataArray == null) {
            pointStrokeDataArray = new PointStrokeData[agentPointStrokesCBuffer.count];
        }

        for (int i = 0; i < agentsArray.Length; i++) {
            // BODY BRUSH STROKE:

            int baseIndex = i * 11;
                        
            pointStrokeDataArray[baseIndex].strength = agentsArray[i].bodyPointStroke.strength;
            pointStrokeDataArray[baseIndex].parentIndex = agentsArray[i].bodyPointStroke.parentIndex;
            pointStrokeDataArray[baseIndex].localScale = agentsArray[i].bodyPointStroke.localScale;
            pointStrokeDataArray[baseIndex].localPos = agentsArray[i].bodyPointStroke.localPos;
            pointStrokeDataArray[baseIndex].localDir = agentsArray[i].bodyPointStroke.localDir;
            pointStrokeDataArray[baseIndex].hue = agentsArray[i].bodyPointStroke.hue;
            pointStrokeDataArray[baseIndex].brushType = agentsArray[i].bodyPointStroke.brushType;

            // EYES BRUSH STROKES:::
            pointStrokeDataArray[baseIndex + 1].strength = agentsArray[i].decorationPointStrokesArray[0].strength;
            pointStrokeDataArray[baseIndex + 1].parentIndex = agentsArray[i].decorationPointStrokesArray[0].parentIndex;
            pointStrokeDataArray[baseIndex + 1].localScale = agentsArray[i].decorationPointStrokesArray[0].localScale;
            pointStrokeDataArray[baseIndex + 1].localPos = agentsArray[i].decorationPointStrokesArray[0].localPos;
            pointStrokeDataArray[baseIndex + 1].localDir = agentsArray[i].decorationPointStrokesArray[0].localDir;
            pointStrokeDataArray[baseIndex + 1].hue = agentsArray[i].decorationPointStrokesArray[0].hue;
            pointStrokeDataArray[baseIndex + 1].brushType = agentsArray[i].decorationPointStrokesArray[0].brushType;

            pointStrokeDataArray[baseIndex + 2].strength = agentsArray[i].decorationPointStrokesArray[1].strength;
            pointStrokeDataArray[baseIndex + 2].parentIndex = agentsArray[i].decorationPointStrokesArray[1].parentIndex;
            pointStrokeDataArray[baseIndex + 2].localScale = agentsArray[i].decorationPointStrokesArray[1].localScale;
            pointStrokeDataArray[baseIndex + 2].localPos = agentsArray[i].decorationPointStrokesArray[1].localPos;
            pointStrokeDataArray[baseIndex + 2].localDir = agentsArray[i].decorationPointStrokesArray[1].localDir;
            pointStrokeDataArray[baseIndex + 2].hue = agentsArray[i].decorationPointStrokesArray[1].hue;
            pointStrokeDataArray[baseIndex + 2].brushType = agentsArray[i].decorationPointStrokesArray[1].brushType;

            for (int j = 0; j < 8; j++) {
                // DECORATIONS BRUSH STROKES:

                int index = baseIndex + j + 3;

                pointStrokeDataArray[index].strength = agentsArray[i].decorationPointStrokesArray[j + 2].strength;
                pointStrokeDataArray[index].parentIndex = agentsArray[i].decorationPointStrokesArray[j + 2].parentIndex;
                pointStrokeDataArray[index].localScale = agentsArray[i].decorationPointStrokesArray[j + 2].localScale;
                pointStrokeDataArray[index].localPos = agentsArray[i].decorationPointStrokesArray[j + 2].localPos;
                pointStrokeDataArray[index].localDir = agentsArray[i].decorationPointStrokesArray[j + 2].localDir;
                pointStrokeDataArray[index].hue = agentsArray[i].decorationPointStrokesArray[j + 2].hue;
                pointStrokeDataArray[index].brushType = agentsArray[i].decorationPointStrokesArray[j + 2].brushType;
            }           
        }
        // Player:
        int playerBaseIndex = agentsArray.Length * 11;
        //pointStrokeDataArray[playerBaseIndex] = playerAgent.bodyPointStroke;
        pointStrokeDataArray[playerBaseIndex].strength = playerAgent.bodyPointStroke.strength;
        pointStrokeDataArray[playerBaseIndex].parentIndex = playerAgent.bodyPointStroke.parentIndex;
        pointStrokeDataArray[playerBaseIndex].localScale = playerAgent.bodyPointStroke.localScale;
        pointStrokeDataArray[playerBaseIndex].localPos = playerAgent.bodyPointStroke.localPos;
        pointStrokeDataArray[playerBaseIndex].localDir = playerAgent.bodyPointStroke.localDir;
        pointStrokeDataArray[playerBaseIndex].hue = playerAgent.bodyPointStroke.hue;
        pointStrokeDataArray[playerBaseIndex].brushType = playerAgent.bodyPointStroke.brushType;

        // EYES:
        pointStrokeDataArray[playerBaseIndex + 1].strength = playerAgent.decorationPointStrokesArray[0].strength;
        pointStrokeDataArray[playerBaseIndex + 1].parentIndex = playerAgent.decorationPointStrokesArray[0].parentIndex;
        pointStrokeDataArray[playerBaseIndex + 1].localScale = playerAgent.decorationPointStrokesArray[0].localScale;
        pointStrokeDataArray[playerBaseIndex + 1].localPos = playerAgent.decorationPointStrokesArray[0].localPos;
        pointStrokeDataArray[playerBaseIndex + 1].localDir = playerAgent.decorationPointStrokesArray[0].localDir;
        pointStrokeDataArray[playerBaseIndex + 1].hue = playerAgent.decorationPointStrokesArray[0].hue;
        pointStrokeDataArray[playerBaseIndex + 1].brushType = playerAgent.decorationPointStrokesArray[0].brushType;

        pointStrokeDataArray[playerBaseIndex + 2].strength = playerAgent.decorationPointStrokesArray[1].strength;
        pointStrokeDataArray[playerBaseIndex + 2].parentIndex = playerAgent.decorationPointStrokesArray[1].parentIndex;
        pointStrokeDataArray[playerBaseIndex + 2].localScale = playerAgent.decorationPointStrokesArray[1].localScale;
        pointStrokeDataArray[playerBaseIndex + 2].localPos = playerAgent.decorationPointStrokesArray[1].localPos;
        pointStrokeDataArray[playerBaseIndex + 2].localDir = playerAgent.decorationPointStrokesArray[1].localDir;
        pointStrokeDataArray[playerBaseIndex + 2].hue = playerAgent.decorationPointStrokesArray[1].hue;
        pointStrokeDataArray[playerBaseIndex + 2].brushType = playerAgent.decorationPointStrokesArray[1].brushType;

        for (int k = 0; k < 8; k++) {
            int playerIndex = playerBaseIndex + k + 3;

            //pointStrokeDataArray[playerIndex] = playerAgent.decorationPointStrokesArray[k];

            pointStrokeDataArray[playerIndex].strength = playerAgent.decorationPointStrokesArray[k + 2].strength;
            pointStrokeDataArray[playerIndex].parentIndex = playerAgent.decorationPointStrokesArray[k + 2].parentIndex;
            pointStrokeDataArray[playerIndex].localScale = playerAgent.decorationPointStrokesArray[k + 2].localScale;
            pointStrokeDataArray[playerIndex].localPos = playerAgent.decorationPointStrokesArray[k + 2].localPos;
            pointStrokeDataArray[playerIndex].localDir = playerAgent.decorationPointStrokesArray[k + 2].localDir;
            pointStrokeDataArray[playerIndex].hue = playerAgent.decorationPointStrokesArray[k + 2].hue;
            pointStrokeDataArray[playerIndex].brushType = playerAgent.decorationPointStrokesArray[k + 2].brushType;
        }

        agentPointStrokesCBuffer.SetData(pointStrokeDataArray);
    }*/

}
