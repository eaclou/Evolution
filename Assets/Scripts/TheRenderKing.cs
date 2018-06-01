using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TheRenderKing : MonoBehaviour {

    // SET IN INSPECTOR!!!::::
    public EnvironmentFluidManager fluidManager;
    public SimulationManager simManager;

    public Camera mainRenderCam;
    public Camera fluidObstaclesRenderCamera;
    public Camera fluidColorRenderCamera;

    private CommandBuffer cmdBufferMainRender;
    private CommandBuffer cmdBufferFluidObstacles;
    private CommandBuffer cmdBufferFluidColor;

    public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderUberChains;
    public ComputeShader computeShaderTerrainGeneration;

    public Material agentEyesDisplayMat;
    public Material curveStrokeDisplayMat;
    public Material trailStrokeDisplayMat;
    public Material frameBufferStrokeDisplayMat;
    public Material basicStrokeDisplayMat;
    public Material fluidBackgroundColorMat;
    public Material floatyBitsDisplayMat;
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
    
    //public Material debugMat;

    private Mesh fluidRenderMesh;

    private bool isInitialized = false;

    private const float velScale = 0.17f; // Conversion for rigidBody Vel --> fluid vel units ----  // approx guess for now

    public GameObject terrainGO;
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
    
    //PointStrokeData[] pointStrokeDataArray;
    CurveStrokeData[] agentSmearStrokesDataArray; // does this need to be global???
    //TrailStrokeData[] trailStrokeDataArray;
    
    private ComputeBuffer quadVerticesCBuffer;  // quad mesh
    private ComputeBuffer agentBodyStrokesCBuffer;

    private int numCurveRibbonQuads = 6;
    private ComputeBuffer curveRibbonVerticesCBuffer;  // short ribbon mesh
    private ComputeBuffer agentSmearStrokesCBuffer;

    private ComputeBuffer agentEyeStrokesCBuffer;

    private ComputeBuffer agentTrailStrokes0CBuffer;
    private ComputeBuffer agentTrailStrokes1CBuffer;
    private int numTrailPointsPerAgent = 16;

    private int numFrameBufferStrokesPerDimension = 512;
    private ComputeBuffer frameBufferStrokesCBuffer;

    private BasicStrokeData[] playerGlowInitPos;
    private ComputeBuffer playerGlowCBuffer;

    private int numPlayerGlowyBits = 1024 * 10;
    private ComputeBuffer playerGlowyBitsCBuffer;

    private int numFloatyBits = 1024 * 6;
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

    public Material debugMaterial;
    public Mesh debugMesh;
    public RenderTexture debugRT; // Used to see texture inside editor (inspector)

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
    public struct FrameBufferStrokeData { // background terrain
        public Vector3 worldPos;
		public Vector2 scale;
		public Vector2 heading;
		public int brushType;
    }

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
        this.simManager = simManager;

        InitializeBuffers();
        InitializeMaterials();
        InitializeUberBrushes();
        InitializeCommandBuffers();

        InitializeTerrain();

        AlignFrameBufferStrokesToTerrain();

        for(int i = 0; i < simManager._NumFood; i++) {
            UpdateDynamicFoodBuffers(i);
        }

        isInitialized = true;  // we did it, guys!
    }

    private void AlignFrameBufferStrokesToTerrain() {
        int kernelCSAlignFrameBufferStrokes = computeShaderBrushStrokes.FindKernel("CSAlignFrameBufferStrokes");
        computeShaderBrushStrokes.SetTexture(kernelCSAlignFrameBufferStrokes, "terrainHeightTex", terrainHeightMap);
        computeShaderBrushStrokes.SetBuffer(kernelCSAlignFrameBufferStrokes, "terrainFrameBufferStrokesCBuffer", frameBufferStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSAlignFrameBufferStrokes, frameBufferStrokesCBuffer.count, 1, 1);
    }
    // Actual mix of rendering passes will change!!! 
    private void InitializeBuffers() {  // primary function -- calls sub-functions for initializing each buffer
            
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering            
        InitializeCurveRibbonMeshBuffer(); // Set up Curve Ribbon Mesh billboard for brushStroke rendering
        InitializeWaterSplineMeshBuffer(); // same for water splines
        InitializeFluidRenderMesh(); 
        
        obstacleStrokesCBuffer = new ComputeBuffer(simManager._NumAgents + simManager._NumFood + simManager._NumPredators, sizeof(float) * 10);
        obstacleStrokeDataArray = new BasicStrokeData[obstacleStrokesCBuffer.count];

        colorInjectionStrokesCBuffer = new ComputeBuffer(simManager._NumAgents + simManager._NumFood + simManager._NumPredators, sizeof(float) * 10);
        colorInjectionStrokeDataArray = new BasicStrokeData[colorInjectionStrokesCBuffer.count];

        InitializeAgentBodyStrokesBuffer();         
        InitializeAgentEyeStrokesBuffer();
        InitializeAgentSmearStrokesBuffer();
        InitializeAgentTailStrokesBuffer();
        InitializeFrameBufferStrokesBuffer();
        InitializePlayerGlowBuffer();
        InitializePlayerGlowyBitsBuffer();
        InitializeFloatyBitsBuffer();
        InitializeRipplesBuffer();
        InitializeWaterSplinesCBuffer();
        InitializeWaterChainsCBuffer();
        
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
        vertices[0] = new Vector3(-simManager._MapSize, -simManager._MapSize, 0f);
        vertices[1] = new Vector3(simManager._MapSize, -simManager._MapSize, 0f);
        vertices[2] = new Vector3(-simManager._MapSize, simManager._MapSize, 0f);
        vertices[3] = new Vector3(simManager._MapSize, simManager._MapSize, 0f);

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
            data.coords = new Vector2(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f)); // (UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
            data.vel = new Vector2(1f, 0f);
            data.heading = new Vector2(1f, 0f);
            data.age = (float)i / (float)numFloatyBits;
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
                data.coords01 = new Vector2((simManager.simStateData.agentSimDataArray[i].worldPos.x + simManager._MapSize) / (simManager._MapSize * 2f), (simManager.simStateData.agentSimDataArray[i].worldPos.y + simManager._MapSize) / (simManager._MapSize * 2f));
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
    private void InitializeFrameBufferStrokesBuffer() {
        frameBufferStrokesCBuffer = new ComputeBuffer(numFrameBufferStrokesPerDimension * numFrameBufferStrokesPerDimension, sizeof(float) * 7 + sizeof(int));
        FrameBufferStrokeData[] frameBufferStrokesArray = new FrameBufferStrokeData[frameBufferStrokesCBuffer.count];
        float frameBufferStrokesBounds = 280f;
        for(int x = 0; x < numFrameBufferStrokesPerDimension; x++) {
            for(int y = 0; y < numFrameBufferStrokesPerDimension; y++) {
                int index = x * numFrameBufferStrokesPerDimension + y;
                float xPos = (float)x / (float)(numFrameBufferStrokesPerDimension - 1) * frameBufferStrokesBounds - frameBufferStrokesBounds / 2f;
                float yPos = (float)y / (float)(numFrameBufferStrokesPerDimension - 1) * frameBufferStrokesBounds - frameBufferStrokesBounds / 2f;
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
    public void InitializeAgentEyeStrokesBuffer() {
        agentEyeStrokesCBuffer = new ComputeBuffer(simManager._NumAgents * 2, sizeof(float) * 13 + sizeof(int) * 2); // pointStrokeData size
        AgentEyeStrokeData[] agentEyesDataArray = new AgentEyeStrokeData[agentEyeStrokesCBuffer.count];        
        for (int i = 0; i < simManager._NumAgents; i++) {
            AgentEyeStrokeData dataLeftEye = new AgentEyeStrokeData();
            dataLeftEye.parentIndex = i;
            dataLeftEye.localPos = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localPos;
            dataLeftEye.localPos.x *= -1f; // LEFT SIDE!
            dataLeftEye.localDir = new Vector2(0f, 1f);
            dataLeftEye.localScale = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localScale;
            dataLeftEye.irisHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.irisHue;
            dataLeftEye.pupilHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
            dataLeftEye.strength = 1f;
            dataLeftEye.brushType = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;

            AgentEyeStrokeData dataRightEye = new AgentEyeStrokeData();
            dataRightEye.parentIndex = i;
            dataRightEye.localPos = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localPos;
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
    public void InitializeAgentSmearStrokesBuffer() {
        // **** Just Curves to start!!!! ********        
        agentSmearStrokesDataArray = new CurveStrokeData[simManager._NumAgents]; // **** Temporarily just for Agents! ******
        agentSmearStrokesCBuffer = new ComputeBuffer(agentSmearStrokesDataArray.Length, sizeof(float) * 14 + sizeof(int) * 2);

        for (int i = 0; i < agentSmearStrokesDataArray.Length; i++) {
            agentSmearStrokesDataArray[i] = new CurveStrokeData();
            agentSmearStrokesDataArray[i].parentIndex = i; // link to Agent
            agentSmearStrokesDataArray[i].hue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary;

            agentSmearStrokesDataArray[i].restLength = simManager.agentsArray[i].fullSize.y * 0.25f; // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio.y * 0.25f;

            agentSmearStrokesDataArray[i].p0 = new Vector2(0f, 0f);
            agentSmearStrokesDataArray[i].p1 = agentSmearStrokesDataArray[i].p0 - new Vector2(0f, agentSmearStrokesDataArray[i].restLength);
            agentSmearStrokesDataArray[i].p2 = agentSmearStrokesDataArray[i].p0 - new Vector2(0f, agentSmearStrokesDataArray[i].restLength * 2f);
            agentSmearStrokesDataArray[i].p3 = agentSmearStrokesDataArray[i].p0 - new Vector2(0f, agentSmearStrokesDataArray[i].restLength * 3f);
            agentSmearStrokesDataArray[i].width = simManager.agentsArray[i].fullSize.x; // simManager.agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio.x;
            
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

    private void InitializeMaterials() {
        agentEyesDisplayMat.SetPass(0); // Eyes
        //pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        agentEyesDisplayMat.SetBuffer("agentEyesStrokesCBuffer", agentEyeStrokesCBuffer);
        agentEyesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                
        curveStrokeDisplayMat.SetPass(0);
        curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
        curveStrokeDisplayMat.SetBuffer("agentCurveStrokesReadCBuffer", agentSmearStrokesCBuffer); 
                
        frameBufferStrokeDisplayMat.SetPass(0);
        frameBufferStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        frameBufferStrokeDisplayMat.SetBuffer("frameBufferStrokesCBuffer", frameBufferStrokesCBuffer);

        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        playerGlowyBitsDisplayMat.SetPass(0);
        playerGlowyBitsDisplayMat.SetBuffer("playerGlowyBitsCBuffer", playerGlowyBitsCBuffer);
        playerGlowyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        
        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        ripplesDisplayMat.SetPass(0);
        //ripplesDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        ripplesDisplayMat.SetBuffer("trailDotsCBuffer", ripplesCBuffer);
        ripplesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                
        trailStrokeDisplayMat.SetPass(0);
        trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);

        waterSplinesMat.SetPass(0);
        waterSplinesMat.SetBuffer("verticesCBuffer", waterSplineVerticesCBuffer);
        waterSplinesMat.SetBuffer("waterSplinesReadCBuffer", waterSplinesCBuffer);

        waterChainsMat.SetPass(0);
        waterChainsMat.SetBuffer("verticesCBuffer", quadVerticesCBuffer);
        waterChainsMat.SetBuffer("waterChainsReadCBuffer", waterChains0CBuffer);

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

        cmdBufferMainRender = new CommandBuffer();
        cmdBufferMainRender.name = "cmdBufferMainRender";
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMainRender);
        
        cmdBufferFluidObstacles = new CommandBuffer();
        cmdBufferFluidObstacles.name = "cmdBufferFluidObstacles";
        //fluidObstaclesRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferFluidObstacles);

        cmdBufferFluidColor = new CommandBuffer();
        cmdBufferFluidColor.name = "cmdBufferFluidColor";
        //fluidColorRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferFluidColor);
        //()
    }
    
    private void InitializeTerrain() {
        Debug.Log("InitializeTerrain!");

        int meshResolution = 192;
        float mapSize = simManager._MapSize;

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
            computeShaderTerrainGeneration.SetVector("_QuadBounds", new Vector4(-mapSize * 2f, mapSize * 2f, -mapSize * 2f, mapSize * 2f));
            computeShaderTerrainGeneration.SetVector("_HeightRange", new Vector4(-32f, 32f, 0f, 0f));

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
            colorInjectionStrokeDataArray[baseIndex + i].scale = simManager.agentsArray[i].fullSize * 1.0f;
            
            float agentAlpha = 0.024f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                agentAlpha = 2.2f / simManager.agentsArray[i].fullSize.magnitude;
            }
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Decaying) {
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
        dataLeftEye.localDir = new Vector2(0f, 1f);
        dataLeftEye.localScale = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.localScale;
        dataLeftEye.irisHue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.irisHue;
        dataLeftEye.pupilHue = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
        dataLeftEye.strength = 1f;
        dataLeftEye.brushType = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;

        AgentEyeStrokeData dataRightEye = new AgentEyeStrokeData();
        dataRightEye.parentIndex = agentIndex;
        dataRightEye.localPos = simManager.agentGenomePoolArray[agentIndex].bodyGenome.appearanceGenome.eyeGenome.localPos;
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

        singleCurveStrokeArray[0].restLength = simManager.agentsArray[agentIndex].fullSize.y * 0.25f;  // simManager.agentGenomePoolArray[agentIndex].bodyGenome.sizeAndAspectRatio.y * 0.25f;

        singleCurveStrokeArray[0].p0 = new Vector2(simManager.agentsArray[agentIndex]._PrevPos.x, simManager.agentsArray[agentIndex]._PrevPos.y);
        singleCurveStrokeArray[0].p1 = singleCurveStrokeArray[0].p0 - new Vector2(0f, singleCurveStrokeArray[0].restLength);
        singleCurveStrokeArray[0].p2 = singleCurveStrokeArray[0].p0 - new Vector2(0f, singleCurveStrokeArray[0].restLength * 2f);
        singleCurveStrokeArray[0].p3 = singleCurveStrokeArray[0].p0 - new Vector2(0f, singleCurveStrokeArray[0].restLength * 3f);
        singleCurveStrokeArray[0].width = simManager.agentsArray[agentIndex].fullSize.x; //simManager.agentGenomePoolArray[agentIndex].bodyGenome.sizeAndAspectRatio.x;
        
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

        // *** Hard-coded 32 Fruits per food object!!!! *** BEWARE!!!
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
        playerGlowyBitsDisplayMat.SetFloat("_PosX", (simManager.agentsArray[0].transform.position.x + 70f) / 140f);
        playerGlowyBitsDisplayMat.SetFloat("_PosY", (simManager.agentsArray[0].transform.position.y + 70f) / 140f);


        int kernelSimPlayerGlowyBits = fluidManager.computeShaderFluidSim.FindKernel("SimPlayerGlowyBits");

        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);
        fluidManager.computeShaderFluidSim.SetVector("_PlayerPos", new Vector4((simManager.agentsArray[0].transform.position.x + 70f) / 140f, (simManager.agentsArray[0].transform.position.y + 70f) / 140f, 0f, 0f));
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

    public void Tick() {  // should be called from SimManager at proper time!
        fullscreenFade = 1f;
        if(simManager.agentsArray[0].curLifeStage == Agent.AgentLifeStage.Egg) {
            fullscreenFade = fullscreenFade * (float)simManager.agentsArray[0].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[0]._GestationDurationTimeSteps;
        }
        if(simManager.agentsArray[0].curLifeStage == Agent.AgentLifeStage.Decaying) {
            fullscreenFade = fullscreenFade * (1f - (float)simManager.agentsArray[0].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[0]._DecayDurationTimeSteps);
        }
        fadeToBlackBlitMat.SetPass(0);
        fadeToBlackBlitMat.SetFloat("_FadeAmount", fullscreenFade);
        //
        // Read current stateData and update all Buffers, send data to GPU
        // Execute computeShaders to update any dynamic particles that are purely cosmetic
        SimPlayerGlow();
        SimAgentSmearStrokes(); // start with this one?
        IterateTrailStrokesData();
        SimPlayerGlowyBits();
        SimFloatyBits();
        SimRipples();
        SimFruit();
        SimWaterSplines();
        SimWaterChains();

        uberFlowChainBrush1.Tick(fluidManager._VelocityA);
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
        cmdBufferFluidObstacles.DrawMesh(terrainMesh, Matrix4x4.identity, terrainObstaclesHeightMaskMat); // Masks out areas above the fluid "Sea Level"
        // Draw dynamic Obstacles:
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
        basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", obstacleStrokesCBuffer);
        cmdBufferFluidObstacles.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, obstacleStrokesCBuffer.count);
                
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

        cmdBufferMainRender.Clear();

        // Create RenderTargets:
        int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        cmdBufferMainRender.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        cmdBufferMainRender.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferMainRender.SetRenderTarget(renderTarget);  // Set render Target
        cmdBufferMainRender.ClearRenderTarget(true, true, Color.gray, 1.0f);  // clear -- needed???
        //cmdBufferMainRender.ClearRenderTarget(true, true, new Color(225f / 255f, 217f / 255f, 200f / 255f), 1.0f);  // clear -- needed???
                
        // FLUID ITSELF:
        fluidRenderMat.SetPass(0);
        fluidRenderMat.SetTexture("_DensityTex", fluidManager._DensityA);
        fluidRenderMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
        fluidRenderMat.SetTexture("_PressureTex", fluidManager._PressureA);
        fluidRenderMat.SetTexture("_DivergenceTex", fluidManager._Divergence);
        fluidRenderMat.SetTexture("_ObstaclesTex", fluidManager._ObstaclesRT);
        fluidRenderMat.SetTexture("_TerrainHeightTex", terrainHeightMap);
        cmdBufferMainRender.DrawMesh(fluidRenderMesh, Matrix4x4.identity, fluidRenderMat);
        
        // BACKGROUND STROKES:::
        frameBufferStrokeDisplayMat.SetPass(0);
        frameBufferStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        frameBufferStrokeDisplayMat.SetBuffer("frameBufferStrokesCBuffer", frameBufferStrokesCBuffer);         
        // Use this technique for Environment Brushstrokes:
        cmdBufferMainRender.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, frameBufferStrokeDisplayMat, 0, MeshTopology.Triangles, 6, frameBufferStrokesCBuffer.count);

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

        // PLAYER GLOWY BITS!
        playerGlowyBitsDisplayMat.SetPass(0);
        //playerGlowyBitsDisplayMat.SetFloat("_PosX", (simManager.agentsArray[0].transform.position.x + 70f) / 140f);
        //playerGlowyBitsDisplayMat.SetFloat("_PosY", (simManager.agentsArray[0].transform.position.y + 70f) / 140f);
        //playerGlowyBitsDisplayMat.SetVector("_PlayerPos", new Vector4((simManager.agentsArray[0].transform.position.x + 70f) / 140f, (simManager.agentsArray[0].transform.position.y + 70f) / 140f, 0f, 0f));
        playerGlowyBitsDisplayMat.SetBuffer("playerGlowyBitsCBuffer", playerGlowyBitsCBuffer);
        playerGlowyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, playerGlowyBitsDisplayMat, 0, MeshTopology.Triangles, 6, playerGlowyBitsCBuffer.count);

        bool displayAgents = true;
        if(displayAgents) {
            
        
            // AGENT TAILS WOO!
            trailStrokeDisplayMat.SetPass(0);
            trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            trailStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, trailStrokeDisplayMat, 0, MeshTopology.Triangles, 6, agentTrailStrokes0CBuffer.count);

            foodFruitDisplayMat.SetPass(0);
            foodFruitDisplayMat.SetBuffer("fruitDataCBuffer", simManager.simStateData.foodFruitDataCBuffer);
            foodFruitDisplayMat.SetBuffer("foodSimDataCBuffer", simManager.simStateData.foodSimDataCBuffer);
            foodFruitDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, foodFruitDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.foodFruitDataCBuffer.count);
                
            // TEMP AGENTS: // CHANGE THIS TO SMEARS!
            curveStrokeDisplayMat.SetPass(0);
            curveStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
            curveStrokeDisplayMat.SetBuffer("agentCurveStrokesReadCBuffer", agentSmearStrokesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, curveStrokeDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, agentSmearStrokesCBuffer.count);
            // AGENT BODY:
            agentBodyDisplayMat.SetPass(0);
            agentBodyDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            agentBodyDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            agentBodyDisplayMat.SetBuffer("bodyStrokesCBuffer", agentBodyStrokesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, agentBodyDisplayMat, 0, MeshTopology.Triangles, 6, agentBodyStrokesCBuffer.count);
            // AGENT EYES:
            agentEyesDisplayMat.SetPass(0);
            agentEyesDisplayMat.SetBuffer("agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
            agentEyesDisplayMat.SetBuffer("agentEyesStrokesCBuffer", agentEyeStrokesCBuffer);
            agentEyesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, agentEyesDisplayMat, 0, MeshTopology.Triangles, 6, agentEyeStrokesCBuffer.count);

            predatorProceduralDisplayMat.SetPass(0);
            predatorProceduralDisplayMat.SetBuffer("predatorSimDataCBuffer", simManager.simStateData.predatorSimDataCBuffer);
            predatorProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, predatorProceduralDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.predatorSimDataCBuffer.count);
        
        
        }
        
        
        
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
        if (frameBufferStrokesCBuffer != null) {
            frameBufferStrokesCBuffer.Release();
        }
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
