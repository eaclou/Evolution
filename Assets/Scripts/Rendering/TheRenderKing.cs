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

    public float tempSwimMag = 1f;
    public float tempSwimFreq = 1f;
    public float tempSwimSpeed = 1f;
    public float tempAccelMult = 1f;

    public Camera mainRenderCam;
    public Camera fluidObstaclesRenderCamera;
    public Camera fluidColorRenderCamera;
    public Camera spiritBrushRenderCamera;
    public Camera slotPortraitRenderCamera;
    public Camera resourceSimRenderCamera;

    public bool isDebugRender = false;

    private CommandBuffer cmdBufferMain;
    private CommandBuffer cmdBufferDebugVis;
    //private CommandBuffer cmdBufferMainRender;
    private CommandBuffer cmdBufferFluidObstacles;
    private CommandBuffer cmdBufferFluidColor;
    private CommandBuffer cmdBufferSpiritBrush;
    //private CommandBuffer cmdBufferTreeOfLifeDisplay;
    private CommandBuffer cmdBufferSlotPortraitDisplay;
    //private CommandBuffer cmdBufferTreeOfLifeSpeciesTree;
    private CommandBuffer cmdBufferResourceSim;

    public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderUberChains;
    public ComputeShader computeShaderCritters;
    public ComputeShader computeShaderEggSacks;
    public ComputeShader computeShaderTreeOfLife;

    public Mesh meshStirStickA;
    public Mesh meshStirStickSml;
    public Mesh meshStirStickMed;
    public Mesh meshStirStickLrg;
    public Material gizmoStirStickAMat;
    public Material gizmoStirStickShadowMat;

    // ORGANIZE AND REMOVE UNUSED!!!!!! *********
    public Material rockMat;
    public Material debugVisModeMat;
    public Material debugVisAlgaeParticlesMat;
    public Material debugVisAnimalParticlesMat;
    public Material agentEyesDisplayMat;
    public Material curveStrokeDisplayMat;
    public Material trailStrokeDisplayMat;
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
    public Material eggSackStrokeDisplayMat;
    public Material eggSackShadowDisplayMat;
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
    public Material algaeParticleDisplayMat;
    public Material foodParticleDisplayMat;
    public Material foodParticleShadowDisplayMat;
    public Material animalParticleDisplayMat;
    public Material animalParticleShadowDisplayMat;
    public Material critterSkinStrokesDisplayMat;
    public Material critterShadowStrokesDisplayMat;
    public Material eggCoverDisplayMat;
    public Material critterDebugGenericStrokeMat;
    public Material critterUberStrokeShadowMat;
    
    public Material critterInspectHighlightMat;
    public Material critterHighlightTrailMat;

    public Material algaeParticleColorInjectMat;
    public Material playerBrushColorInjectMat;
    public Material resourceSimTransferMat;
    public Material resourceSimAgentDataMat;
    public Material plantParticleDataMat;

    public Material gizmoStirToolMat;
    public Material gizmoFeedToolMat;

    public Material treeOfLifeLeafNodesMat;
    public Material treeOfLifeStemSegmentsMat;
    public Material treeOfLifeBackdropMat;
    public Material treeOfLifeBackdropPortraitBorderMat;
    public Material treeOfLifePortraitBorderMat;
    public Material treeOfLifePortraitMat;
    public Material treeOfLifePortraitEyeMat;
    public Material treeOfLifeDecorationMat;

    public Material treeOfLifeWorldStatsMat;  // start with these simple ones first
    public Material treeOfLifeSpeciesLineMat;
    public Material treeOfLifeEventsLineMat;
    public Material treeOfLifeSpeciesHeadTipMat;
    public Material treeOfLifeCursorLineMat;
    public Material toolbarSpeciesPortraitStrokesMat;

    public Material spiritBrushRenderMat;

    public ComputeBuffer gizmoCursorPosCBuffer;
    public ComputeBuffer gizmoFeedToolPosCBuffer;
    //public Material critterEyeStrokesDisplayMat;

    //public bool isDebugRenderOn = true;
    
    //public Material debugMat;

    private Mesh fluidRenderMesh;

    //private RenderTexture primaryRT;

    private bool isInitialized = false;

    private const float velScale = 0.390f; // Conversion for rigidBody Vel --> fluid vel units ----  // approx guess for now

    public bool nutrientToolOn = false;
    public bool mutateToolOn = false;
    public bool removeToolOn = false;
    public bool isStirring = false;
    public bool isBrushing = false;
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

    private int numCurveRibbonQuads = 18;
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

    //private int numStrokesPerCritterBody = 1024;       
    private int numStrokesPerCritterLength = 32;   // This is the official order of brush indexing!!!
    private int numStrokesPerCritterCross = 32;
    private int numStrokesPerCritterEyes = 256;
    private int numStrokesPerCritterMouth = 64;
    private int numStrokesPerCritterTeeth = 64;
    private int numStrokesPerCritterPectoralFins = 64;
    private int numStrokesPerCritterDorsalFin = 128;
    private int numStrokesPerCritterTailFin = 128;
    private int numStrokesPerCritterSkinDetail = 128;
    private ComputeBuffer critterGenericStrokesCBuffer;
    // PORTRAIT!!!!
    public ComputeBuffer toolbarPortraitCritterInitDataCBuffer;
    public ComputeBuffer toolbarPortraitCritterSimDataCBuffer;
    private ComputeBuffer toolbarCritterPortraitStrokesCBuffer;

    //private int numStrokesPerCritterShadow = 4;
    //private ComputeBuffer critterShadowStrokesCBuffer;

    private int numEnergyDotsPerCritter = 32;
    private ComputeBuffer critterEnergyDotsCBuffer;
    private int numFoodDotsPerCritter = 32;
    private ComputeBuffer critterFoodDotsCBuffer;

    private ComputeBuffer critterHighlightTrailCBuffer;

    

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

    //private ComputeBuffer agentHoverHighlightCBuffer;

    private BasicStrokeData[] obstacleStrokeDataArray;
    private ComputeBuffer obstacleStrokesCBuffer;

    private BasicStrokeData[] colorInjectionStrokeDataArray;
    private ComputeBuffer colorInjectionStrokesCBuffer;

    //private ComputeBuffer debugAgentResourcesCBuffer;

    public Material debugMaterial;
    public Mesh debugMesh;
    public RenderTexture debugRT; // Used to see texture inside editor (inspector)

    public bool isSpiritBrushOn = false;
    public float spiritBrushPosNeg = 1f;
    public RenderTexture spiritBrushRT;
    private int spiritBrushResolution = 128;
    //public RenderTexture terrainBaseColorRT;
    //private int terrainBaseColorResolution = 128;
        
    public Texture2D critterBodyWidthsTex;

    public float fullscreenFade = 1f;

    private Vector3[] testTreeOfLifePositionArray;
    private ComputeBuffer testTreeOfLifePositionCBuffer;
    private TreeOfLifeEventLineData[] treeOfLifeEventLineDataArray;
    private ComputeBuffer treeOfLifeEventLineDataCBuffer;
    private ComputeBuffer treeOfLifeWorldStatsValuesCBuffer;
    private ComputeBuffer treeOfLifeSpeciesSegmentsCBuffer;

    public ComputeBuffer treeOfLifeSpeciesDataKeyCBuffer;
    public Vector3[] treeOfLifeSpeciesDataHeadPosArray;
    public ComputeBuffer treeOfLifeSpeciesDataHeadPosCBuffer;

    public struct TreeOfLifeSpeciesKeyData {
        public int timeCreated;        
        public int timeExtinct;
        public Vector3 huePrimary;
        public Vector3 hueSecondary;
        public Vector3 parentHue;
        public float isOn;
        public float isExtinct;
        public float isSelected;
    }

    private int numTreeOfLifeStemSegmentQuads = 16;
    private ComputeBuffer treeOfLifeStemSegmentVerticesCBuffer;  // short ribbon mesh
    private int maxNumTreeOfLifeNodes = 512; // max numSpecies
    private int maxNumTreeOfLifeSegments = 512;  
    private TreeOfLifeNodeColliderData[] treeOfLifeNodeColliderDataArray;
    private ComputeBuffer treeOfLifeNodeColliderDataCBufferA;
    private ComputeBuffer treeOfLifeNodeColliderDataCBufferB;
    private TreeOfLifeLeafNodeData[] treeOfLifeLeafNodeDataArray;
    private ComputeBuffer treeOfLifeLeafNodeDataCBuffer;
    //private TreeOfLifeStemSegmentStruct[] treeOfLifeStemSegmentDataArray;
    private ComputeBuffer treeOfLifeStemSegmentDataCBuffer;
    private int curNumTreeOfLifeStemSegments = 0;

    public ComputeBuffer treeOfLifeBasicStrokeDataCBuffer;
    public ComputeBuffer treeOfLifePortraitBorderDataCBuffer;
    public ComputeBuffer treeOflifePortraitDataCBuffer;
    public ComputeBuffer treeOfLifePortraitEyeDataCBuffer;
    
    public struct TreeOfLifeEventLineData {
        public int timeStepActivated;
        public float eventCategory;  // minor major extreme 0, 0.5, 1.0
        public float isActive;
    }
    // Is this still needed??
    public struct TreeOfLifeNodeColliderData {  // only the data that needs to be transferred between CPU & GPU  - minimize!!
        public Vector3 localPos;
        public Vector3 scale;        
    }
    public struct TreeOfLifeLeafNodeData {
        public int speciesID;
        public int parentSpeciesID;
        public int graphDepth;
        public Vector3 primaryHue;
        public Vector3 secondaryHue;
        public float growthPercentage;
        public float age;
        public float decayPercentage;
        public float isActive;
        public float isExtinct;
        public float isHover;
	    public float isSelected;
        public float relFitnessScore;
    }
    public struct TreeOfLifeStemSegmentStruct {
        public int speciesID;
        public int fromID;
        public int toID;
        public float attachPosLerp;
    }
    
    public struct PlayerGlowyBitData {
		public Vector2 coords;
		public Vector2 vel;
        public Vector2 heading;
		public float age;
	}

    public struct HighlightTrailData {
		public Vector2 worldPos;
		public Vector2 vel;
        public Vector2 heading;
		public float age;
        public float strength;
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
    public struct CritterUberStrokeData {
	    public int parentIndex;  // which Critter is this attached to?	
        public int neighborIndex;
	    public int brushType;
        public float t;  // normalized position along spine, 0=tailtip, 1=headtip
        public Vector3 bindPos;  // object-coordinates (z=forward, y=up)
        public Vector3 worldPos;
        public Vector3 bindNormal;
        public Vector3 worldNormal;
        public Vector3 bindTangent;
        public Vector3 worldTangent;
        public Vector2 uv;
        public Vector2 scale;
        public Vector4 color;
        public float jawMask;  // +1 top, -1 bottom.  Eventually look into better method, support different mouth/jaw types
	    public float restDistance;
        public float neighborAlign;  // how much to adjust tangent towards neighbor point
        public float passiveFollow;  // only look at neighborPos/restDistance for positioning
	    public float thresholdValue;  // evenly distributed random 0-1 for decay
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

    public bool isToolbarCritterPortraitEnabled = false;
    
    /* $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$  RENDER PIPELINE PSEUDOCODE!!!  $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
    1) Standard main camera beauty pass finishes -- Renders Environment & Background objects -- store result in RT to be sampled later by brushstroke shaders
    2) Background pass Brushstrokes
    3) Fluid Display + extra brushstrokes
    4+) Dynamic Objects Brushstrokes -- Agents, Food, Predators, floatyBits etc.
    *///$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$  RENDER PIPELINE PSEUDOCODE!!!  $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

    private void Awake() {
        fluidObstaclesRenderCamera.enabled = false;
        fluidColorRenderCamera.enabled = false;
        spiritBrushRenderCamera.enabled = false;
        //treeOfLifeSpeciesTreeRenderCamera.enabled = false;  // only render when the King commands it!!!
        //treeOfLifeRenderCamera.enabled = false;
        slotPortraitRenderCamera.enabled = false;
        resourceSimRenderCamera.enabled = false;
    }
    // Use this for initialization:
    public void InitializeRiseAndShine(SimulationManager simManager) {
        
        this.simManager = simManager;

        // temp bodyWidthsTexture:
        critterBodyWidthsTex = new Texture2D(16, simManager._NumAgents, TextureFormat.RGBAFloat, false, true);
        
        InitializeBuffers();
        InitializeMaterials();
        //InitializeUberBrushes(); // old uber
        InitializeCommandBuffers();
        
        baronVonTerrain.Initialize();
        baronVonWater.Initialize();

        for(int i = 0; i < simManager._NumEggSacks; i++) {
            UpdateDynamicFoodBuffers(i);
        }

        //TreeOfLifeAddNewSpecies(0); // ** HACKY!!!! ***

        rockMat.SetTexture("_MainTex", simManager.theRenderKing.baronVonTerrain.terrainColorRT0); // terrainBaseColorRT);

        isInitialized = true;  // we did it, guys!
    }
    
    // Actual mix of rendering passes will change!!! 
    private void InitializeBuffers() {  // primary function -- calls sub-functions for initializing each buffer
            
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering            
        InitializeCurveRibbonMeshBuffer(); // Set up Curve Ribbon Mesh billboard for brushStroke rendering
        InitializeWaterSplineMeshBuffer(); // same for water splines
        InitializeFluidRenderMesh();
        InitializeBodySwimAnimMeshBuffer(); // test movementAnimation
        
        obstacleStrokesCBuffer = new ComputeBuffer(simManager._NumAgents + simManager._NumEggSacks, sizeof(float) * 10);
        obstacleStrokeDataArray = new BasicStrokeData[obstacleStrokesCBuffer.count];

        colorInjectionStrokesCBuffer = new ComputeBuffer(simManager._NumAgents + simManager._NumEggSacks, sizeof(float) * 10);
        colorInjectionStrokeDataArray = new BasicStrokeData[colorInjectionStrokesCBuffer.count];

        InitializeCritterUberStrokesBuffer();
        //InitializeAgentBodyStrokesBuffer();
        InitializeCritterSkinStrokesCBuffer();
        InitializeAgentEyeStrokesBuffer();
        //InitializeAgentSmearStrokesBuffer();
        InitializeAgentTailStrokesBuffer();
        //InitializeFrameBufferStrokesBuffer();
        //InitializePlayerGlowBuffer();
        InitializePlayerGlowyBitsBuffer();
        InitializeFloatyBitsBuffer();
        //InitializeRipplesBuffer();
        InitializeWaterSplinesCBuffer();
        InitializeWaterChainsCBuffer();
        InitializeGizmos();
        //InitializeAgentHoverHighlightCBuffer();
        InitializeTreeOfLifeBuffers();
        //InitializeDebugBuffers(); 


        // INIT:: ugly :(
        if(toolbarPortraitCritterInitDataCBuffer != null) {
            toolbarPortraitCritterInitDataCBuffer.Release();
        }
        toolbarPortraitCritterInitDataCBuffer = new ComputeBuffer(1, sizeof(float) * 25 + sizeof(int) * 3);

    }

    private int GetNumUberStrokesPerCritter() {
        int numStrokes = numStrokesPerCritterLength * numStrokesPerCritterCross +
                            numStrokesPerCritterEyes +
                            numStrokesPerCritterMouth +
                            numStrokesPerCritterTeeth +
                            numStrokesPerCritterPectoralFins +
                            numStrokesPerCritterDorsalFin +                            
                            numStrokesPerCritterTailFin +
                            numStrokesPerCritterSkinDetail;
                            
        return numStrokes;
    }
    private void InitializeCritterUberStrokesBuffer() {
        // Most of this will be populated piece-meal later as critters are generated:
        int bufferLength = simManager._NumAgents * GetNumUberStrokesPerCritter();
        critterGenericStrokesCBuffer = new ComputeBuffer(bufferLength, GetMemorySizeCritterUberStrokeData());
        toolbarCritterPortraitStrokesCBuffer = new ComputeBuffer(GetNumUberStrokesPerCritter(), GetMemorySizeCritterUberStrokeData());
    }
    private int GetMemorySizeCritterUberStrokeData() {
        int numBytes = sizeof(int) * 3 + sizeof(float) * 32;
        return numBytes;
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
        //GameObject plane  = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //fluidRenderMesh = plane.GetComponent<MeshFilter>().sharedMesh; // 
        fluidRenderMesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0f, 0f, 1f);
        vertices[1] = new Vector3(SimulationManager._MapSize, 0f, 1f);
        vertices[2] = new Vector3(0f, SimulationManager._MapSize, 1f);
        vertices[3] = new Vector3(SimulationManager._MapSize, SimulationManager._MapSize, 1f);

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
    /*private void InitializeUberBrushes() {
        uberFlowChainBrush1 = new UberFlowChainBrush();
        uberFlowChainBrush1.Initialize(computeShaderUberChains, uberFlowChainBrushMat1);
    }*/
    //playerGlowCBuffer
    /*private void InitializePlayerGlowBuffer() {
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

    }*/
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
    /*private void InitializeRipplesBuffer() {
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
    }*/
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
    public void InitializeAgentEyeStrokesBuffer() {  // ************* BROKEN BY SPECIATION UPDATE!!!!! **************************************************************************** !!!!!!
        agentEyeStrokesCBuffer = new ComputeBuffer(simManager._NumAgents * 2, sizeof(float) * 13 + sizeof(int) * 2); // pointStrokeData size
        AgentEyeStrokeData[] agentEyesDataArray = new AgentEyeStrokeData[agentEyeStrokesCBuffer.count];        
        for (int i = 0; i < simManager._NumAgents; i++) {
            AgentEyeStrokeData dataLeftEye = new AgentEyeStrokeData();
            dataLeftEye.parentIndex = i;
            dataLeftEye.localPos = new Vector2(-0.5f, 0.5f); // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localPos;
            dataLeftEye.localPos.x *= -1f; // LEFT SIDE!
            //float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((dataLeftEye.localPos.y * 0.5f + 0.5f) * 15f)];
            dataLeftEye.localPos.x *= 0.5f;
            dataLeftEye.localDir = new Vector2(0f, 1f);
            dataLeftEye.localScale = Vector2.one * 0.1f; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localScale;
            dataLeftEye.irisHue = Vector3.one; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.irisHue;
            dataLeftEye.pupilHue = Vector3.zero; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
            dataLeftEye.strength = 1f;
            dataLeftEye.brushType = 0; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;

            AgentEyeStrokeData dataRightEye = new AgentEyeStrokeData();
            dataRightEye.parentIndex = i;
            dataRightEye.localPos = new Vector2(0.5f, 0.5f); // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localPos;
            //width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((dataRightEye.localPos.y * 0.5f + 0.5f) * 15f)];
            dataRightEye.localPos.x *= 0.5f;
            dataRightEye.localDir = new Vector2(0f, 1f);
            dataRightEye.localScale = Vector2.one * 0.1f; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.localScale;
            dataRightEye.irisHue = Vector3.one; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.irisHue;
            dataRightEye.pupilHue = Vector3.zero; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.pupilHue;
            dataRightEye.strength = 1f;
            dataRightEye.brushType = 0; // simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;
            
            agentEyesDataArray[i * 2 + 0] = dataLeftEye;
            agentEyesDataArray[i * 2 + 1] = dataRightEye;
        }
        agentEyeStrokesCBuffer.SetData(agentEyesDataArray);
    }
    /*public void InitializeAgentBodyStrokesBuffer() {
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
    }*/
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
                float radiusAtZ = Mathf.Sqrt(1f - zCoord * zCoord); // pythagorean theorem
                Vector2 xyCoords = UnityEngine.Random.insideUnitCircle.normalized * radiusAtZ; // possibility for (0,0) ??? ***** undefined/null divide by zero hazard!
                skinStroke.localPos = new Vector3(xyCoords.x, xyCoords.y, zCoord);
                //float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((skinStroke.localPos.y * 0.5f + 0.5f) * 15f)];
                skinStroke.localPos.x *= 0.5f;
                skinStroke.localPos.z *= 0.5f;               // * width  
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
                //float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((energyDot.localPos.y * 0.5f + 0.5f) * 15f)];
                energyDot.localPos.x *= 0.5f;
                energyDot.localPos.z *= 0.5f;
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
                //float width = simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((foodDot.localPos.y * 0.5f + 0.5f) * 15f)];
                foodDot.localPos.x *= 0.5f;
                foodDot.localPos.z *= 0.5f;
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

        // Highlight trail:
        critterHighlightTrailCBuffer = new ComputeBuffer(simManager._NumAgents * 1024, sizeof(float) * 8);
        HighlightTrailData[] highlightTrailDataArray = new HighlightTrailData[critterHighlightTrailCBuffer.count];
        for (int i = 0; i < highlightTrailDataArray.Length; i++) {
            
            HighlightTrailData data = new HighlightTrailData();
            data.age = UnityEngine.Random.Range(0f, 1f);                

            highlightTrailDataArray[i] = data;            
        }        
        critterHighlightTrailCBuffer.SetData(highlightTrailDataArray);

    }
    /*public void InitializeAgentSmearStrokesBuffer() {
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
    }*/
    public void InitializeAgentTailStrokesBuffer() {
        TrailStrokeData[] trailStrokeDataArray = new TrailStrokeData[simManager._NumAgents * numTrailPointsPerAgent];
        for (int i = 0; i < trailStrokeDataArray.Length; i++) {
            //int agentIndex = (int)Mathf.Floor((float)i / numTrailPointsPerAgent);
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
            //int agentIndex = (int)Mathf.Floor((float)i / numPointsPerWaterChain);
            float trailPos = (float)i % (float)numPointsPerWaterChain;
            Vector2 randPos = new Vector2(UnityEngine.Random.Range(-60f, 60f), UnityEngine.Random.Range(-60f, 60f));
            waterChainDataArray[i] = new TrailStrokeData();
            waterChainDataArray[i].worldPos = randPos + new Vector2(0f, trailPos * -1f);
        }
        waterChains0CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
        waterChains0CBuffer.SetData(waterChainDataArray);
        waterChains1CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
    }
    public void InitializeGizmos() {
        Vector4[] dataArray = new Vector4[1];
        Vector4 gizmoPos = new Vector4(128f, 128f, 0f, 0f);
        dataArray[0] = gizmoPos;
        gizmoCursorPosCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        gizmoCursorPosCBuffer.SetData(dataArray);
    }
    private void InitializeTreeOfLifeBuffers() {

        testTreeOfLifePositionArray = new Vector3[64];
        testTreeOfLifePositionCBuffer = new ComputeBuffer(testTreeOfLifePositionArray.Length, sizeof(float) * 3);
        for(int i = 0; i < testTreeOfLifePositionArray.Length; i++) {
            Vector3 pos = new Vector3((float)i / 64f, 0f, 0f);
            testTreeOfLifePositionArray[i] = pos;
        }
        testTreeOfLifePositionCBuffer.SetData(testTreeOfLifePositionArray);

        treeOfLifeEventLineDataArray = new TreeOfLifeEventLineData[64];
        treeOfLifeEventLineDataCBuffer = new ComputeBuffer(treeOfLifeEventLineDataArray.Length, sizeof(float) * 2 + sizeof(int));
        for(int i = 0; i < testTreeOfLifePositionArray.Length; i++) {
            TreeOfLifeEventLineData data = new TreeOfLifeEventLineData();
            data.timeStepActivated = 0;
            data.eventCategory = 0f;
            data.isActive = 0f;
            treeOfLifeEventLineDataArray[i] = data;
        }
        treeOfLifeEventLineDataCBuffer.SetData(treeOfLifeEventLineDataArray);

        treeOfLifeWorldStatsValuesCBuffer = new ComputeBuffer(64, sizeof(float));

        treeOfLifeSpeciesSegmentsCBuffer = new ComputeBuffer(64 * 32, sizeof(float) * 3);

        treeOfLifeSpeciesDataKeyCBuffer = new ComputeBuffer(32, sizeof(int) * 2 + sizeof(float) * 12);
        treeOfLifeSpeciesDataHeadPosArray = new Vector3[32];
        treeOfLifeSpeciesDataHeadPosCBuffer = new ComputeBuffer(32, sizeof(float) * 3);

        // ========================================================================================================================================
        // **** OLD BELOW:::: *********
        // ========================================================================================================================================
        treeOfLifeNodeColliderDataArray = new TreeOfLifeNodeColliderData[maxNumTreeOfLifeNodes];
        treeOfLifeNodeColliderDataCBufferA = new ComputeBuffer(treeOfLifeNodeColliderDataArray.Length, sizeof(float) * 6);
        treeOfLifeNodeColliderDataCBufferB = new ComputeBuffer(treeOfLifeNodeColliderDataArray.Length, sizeof(float) * 6);
        for(int i = 0; i < treeOfLifeNodeColliderDataArray.Length; i++) {
            TreeOfLifeNodeColliderData data = new TreeOfLifeNodeColliderData();
            data.localPos = Vector3.zero;
            data.scale = Vector3.one;
            treeOfLifeNodeColliderDataArray[i] = data;
        }
        treeOfLifeNodeColliderDataCBufferA.SetData(treeOfLifeNodeColliderDataArray);
        treeOfLifeNodeColliderDataCBufferB.SetData(treeOfLifeNodeColliderDataArray);

        treeOfLifeLeafNodeDataArray = new TreeOfLifeLeafNodeData[maxNumTreeOfLifeNodes];
        treeOfLifeLeafNodeDataCBuffer = new ComputeBuffer(treeOfLifeLeafNodeDataArray.Length, sizeof(int) * 3 + sizeof(float) * 14);
        for(int i = 0; i < treeOfLifeLeafNodeDataArray.Length; i++) {
            TreeOfLifeLeafNodeData data = new TreeOfLifeLeafNodeData();
            data.speciesID = i;
            data.parentSpeciesID = 0;
            data.graphDepth = 0;
            data.primaryHue = Vector3.one;
            data.secondaryHue = Vector3.zero;
            data.growthPercentage = 1f;
            data.age = 0f;
            data.decayPercentage = 0f;       
            data.isActive = 0f;
            data.isExtinct = 0f;
            treeOfLifeLeafNodeDataArray[i] = data;
        }
        treeOfLifeLeafNodeDataCBuffer.SetData(treeOfLifeLeafNodeDataArray);        

        // Stem Segments:
        // QuadVertices:        
        float rowSize = 1f / (float)numTreeOfLifeStemSegmentQuads;
        treeOfLifeStemSegmentVerticesCBuffer = new ComputeBuffer(6 * numTreeOfLifeStemSegmentQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[treeOfLifeStemSegmentVerticesCBuffer.count];
        for(int i = 0; i < numTreeOfLifeStemSegmentQuads; i++) {
            int baseIndex = i * 6;

            float startCoord = (float)i;
            float endCoord = (float)(i + 1);   // **** WHY WAS THERE A WINDING ORDER PROBLEM???? *****
            verticesArray[baseIndex + 0] = new Vector3(-0.5f, endCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 1] = new Vector3(0.5f, endCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 2] = new Vector3(0.5f, startCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 3] = new Vector3(0.5f, startCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 4] = new Vector3(-0.5f, startCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 5] = new Vector3(-0.5f, endCoord * rowSize - 0.5f); 

            /*quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });*/

        }
        treeOfLifeStemSegmentVerticesCBuffer.SetData(verticesArray);
    

        // actual segments buffer:
        TreeOfLifeStemSegmentStruct[] treeOfLifeStemSegmentDataArray = new TreeOfLifeStemSegmentStruct[maxNumTreeOfLifeSegments];
        treeOfLifeStemSegmentDataCBuffer = new ComputeBuffer(maxNumTreeOfLifeSegments, sizeof(int) * 3 + sizeof(float) * 1);
        for(int i = 0; i < treeOfLifeStemSegmentDataArray.Length; i++) {
            TreeOfLifeStemSegmentStruct newStruct = new TreeOfLifeStemSegmentStruct();
            newStruct.speciesID = 0;
            newStruct.fromID = 0;
            newStruct.toID = 0;
            treeOfLifeStemSegmentDataArray[i] = newStruct;
        }
        treeOfLifeStemSegmentDataCBuffer.SetData(treeOfLifeStemSegmentDataArray);
        // might not have to do anything here -- update piece-meal with ComputeShader Dispatches?
        //treeOfLifeStemSegmentDataCBuffer.GetData(treeOfLifeStemSegmentDataArray);
        //Debug.Log("WTF M8 treeOfLifeStemSegmentDataArray " + treeOfLifeStemSegmentDataArray[32].speciesID.ToString());
        //TreeOfLifeAddNewSpecies(0, 0);


        // Backdrop:
        // float2 forward = data.localDir;
        // float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
        BasicStrokeData[] treeOfLifeBasicStrokeDataArray = new BasicStrokeData[1];
        treeOfLifeBasicStrokeDataCBuffer = new ComputeBuffer(treeOfLifeBasicStrokeDataArray.Length, sizeof(float) * 10);        
        BasicStrokeData strokeData = new BasicStrokeData();
        strokeData.worldPos = new Vector2(0f, 0f);
        strokeData.localDir = new Vector2(1f, 0f);
        strokeData.scale = new Vector2(6f, 3f);
        strokeData.color = new Vector4(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
        treeOfLifeBasicStrokeDataArray[0] = strokeData;
        treeOfLifeBasicStrokeDataCBuffer.SetData(treeOfLifeBasicStrokeDataArray);

        // Portrait Border Backdrop:
        BasicStrokeData[] treeOfLifePortraitBorderDataArray = new BasicStrokeData[1];
        treeOfLifePortraitBorderDataCBuffer = new ComputeBuffer(treeOfLifePortraitBorderDataArray.Length, sizeof(float) * 10);        
        BasicStrokeData borderData = new BasicStrokeData();
        borderData.worldPos = new Vector2(-0.26f, -0.24f);
        borderData.localDir = new Vector2(1f, 0f);
        borderData.scale = new Vector2(1.22f, 1.22f);
        borderData.color = new Vector4(1f, 1f, 1f, 1f);
        treeOfLifePortraitBorderDataArray[0] = borderData;
        treeOfLifePortraitBorderDataCBuffer.SetData(treeOfLifePortraitBorderDataArray);

        /*int numPortraitBackdropStrokes = 128;
        int numTreeBackdropStrokes = 128;
        BasicStrokeData[] treeOfLifeBasicStrokeDataArray = new BasicStrokeData[numPortraitBackdropStrokes + numTreeBackdropStrokes];
        treeOfLifeBasicStrokeDataCBuffer = new ComputeBuffer(treeOfLifeBasicStrokeDataArray.Length, sizeof(float) * 10);
        for(int i = 0; i < numTreeBackdropStrokes; i++) {
            BasicStrokeData strokeData = new BasicStrokeData();
            strokeData.worldPos = new Vector2(UnityEngine.Random.Range(1f, 4.5f), UnityEngine.Random.Range(-1.33f, 0.2f));
            strokeData.localDir = new Vector2(1f, UnityEngine.Random.Range(-0.36f, 0.36f)).normalized;
            strokeData.scale = new Vector2(UnityEngine.Random.Range(0.7f, 1.5f), UnityEngine.Random.Range(3.9f, 6f)) * UnityEngine.Random.Range(0.24f, 0.4f);
            strokeData.color = new Vector4(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
            treeOfLifeBasicStrokeDataArray[i] = strokeData;
        } 
        for(int i = 0; i < numPortraitBackdropStrokes; i++) {
            BasicStrokeData strokeData = new BasicStrokeData();
            Vector2 circleCoords = UnityEngine.Random.insideUnitCircle;
            strokeData.worldPos = circleCoords * 0.5f + new Vector2(0.5f, -0.5f);
            strokeData.localDir = circleCoords.normalized;
            strokeData.localDir = new Vector2(strokeData.localDir.y, -strokeData.localDir.x);
            strokeData.scale = new Vector2(1.25f, 3.5f) * 0.1f; // * (strokeData.worldPos.magnitude * 0.5f + 0.5f);
            strokeData.color = new Vector4(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
            treeOfLifeBasicStrokeDataArray[i + numTreeBackdropStrokes] = strokeData;
        }             
        treeOfLifeBasicStrokeDataCBuffer.SetData(treeOfLifeBasicStrokeDataArray);
        */

        // Portrait:
        // Update single-element buffer on-demand through UI -- repopulate from given genome
        CritterSkinStrokeData[] treeOfLifePortraitDataArray = new CritterSkinStrokeData[1 * numStrokesPerCritterSkin];
        treeOflifePortraitDataCBuffer = new ComputeBuffer(treeOfLifePortraitDataArray.Length, sizeof(float) * 16 + sizeof(int) * 2);
        for(int j = 0; j < treeOfLifePortraitDataArray.Length; j++) {
            CritterSkinStrokeData skinStroke = new CritterSkinStrokeData();
            skinStroke.parentIndex = 0;
            skinStroke.brushType = 0; // ** Revisit

            skinStroke.worldPos = new Vector3(SimulationManager._MapSize / 2f, SimulationManager._MapSize / 2f, 0f);

            float zCoord = (1f - ((float)j / (float)(numStrokesPerCritterSkin - 1))) * 2f - 1f;
            float radiusAtZ = Mathf.Sqrt(1f - zCoord * zCoord); // pythagorean theorem
            Vector2 xyCoords = UnityEngine.Random.insideUnitCircle.normalized * radiusAtZ; // possibility for (0,0) ??? ***** undefined/null divide by zero hazard!
            skinStroke.localPos = new Vector3(xyCoords.x, xyCoords.y, zCoord);
            //float width = 1f; // simManager.agentsArray[i].agentWidthsArray[Mathf.RoundToInt((skinStroke.localPos.y * 0.5f + 0.5f) * 15f)];
            skinStroke.localPos.x *= 0.5f;
            skinStroke.localPos.z *= 0.5f;               
            skinStroke.localDir = new Vector3(0f, 1f, 0f); // start up? shouldn't matter
            skinStroke.localScale = new Vector2(0.25f, 0.420f) * 1.25f;
            skinStroke.strength = UnityEngine.Random.Range(0f, 1f);
            skinStroke.lifeStatus = 0f;
            skinStroke.age = UnityEngine.Random.Range(1f, 2f);
            skinStroke.randomSeed = UnityEngine.Random.Range(0f, 1f);
            skinStroke.followLerp = 1f;

            treeOfLifePortraitDataArray[j] = skinStroke;
        }
        treeOflifePortraitDataCBuffer.SetData(treeOfLifePortraitDataArray);

        // Decorations:
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
    /*public void InitializeAgentHoverHighlightCBuffer() {
        agentHoverHighlightCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        Vector4[] agentHoverHighlightArray = new Vector4[1];
        agentHoverHighlightArray[0] = new Vector4(0f, 0f, 0f, 0f);
        agentHoverHighlightCBuffer.SetData(agentHoverHighlightArray);
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

        critterHighlightTrailMat.SetPass(0);
        critterHighlightTrailMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                
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
                
        algaeParticleDisplayMat.SetPass(0);
        algaeParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        foodParticleDisplayMat.SetPass(0);
        foodParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        foodParticleShadowDisplayMat.SetPass(0);
        foodParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        animalParticleDisplayMat.SetPass(0);
        animalParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        animalParticleShadowDisplayMat.SetPass(0);
        animalParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        eggCoverDisplayMat.SetPass(0);
        eggCoverDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                
        eggSackShadowDisplayMat.SetPass(0);
        eggSackShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        critterInspectHighlightMat.SetPass(0);
        critterInspectHighlightMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        algaeParticleColorInjectMat.SetPass(0);
        algaeParticleColorInjectMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        gizmoStirToolMat.SetPass(0);
        gizmoStirToolMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        gizmoFeedToolMat.SetPass(0);
        gizmoFeedToolMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        treeOfLifeLeafNodesMat.SetPass(0);
        treeOfLifeLeafNodesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        treeOfLifeStemSegmentsMat.SetPass(0);
        treeOfLifeStemSegmentsMat.SetBuffer("treeOfLifeStemSegmentVerticesCBuffer", treeOfLifeStemSegmentVerticesCBuffer);

        treeOfLifeBackdropMat.SetPass(0);
        treeOfLifeBackdropMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeBackdropPortraitBorderMat.SetPass(0);
        treeOfLifeBackdropPortraitBorderMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifePortraitMat.SetPass(0);
        treeOfLifePortraitMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
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

        cmdBufferMain = new CommandBuffer();
        cmdBufferMain.name = "cmdBufferMain";
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMain);

        cmdBufferDebugVis = new CommandBuffer();
        cmdBufferDebugVis.name = "cmdBufferDebugVis";

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

        spiritBrushRT = new RenderTexture(spiritBrushResolution, spiritBrushResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        spiritBrushRT.wrapMode = TextureWrapMode.Clamp;
        spiritBrushRT.enableRandomWrite = true;
        spiritBrushRT.Create();

        //terrainBaseColorRT = new RenderTexture(terrainBaseColorResolution, terrainBaseColorResolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        //terrainBaseColorRT.wrapMode = TextureWrapMode.Clamp;
        //terrainBaseColorRT.enableRandomWrite = true;
        //terrainBaseColorRT.Create();

        cmdBufferSpiritBrush = new CommandBuffer();
        cmdBufferSpiritBrush.name = "cmdBufferSpiritBrush";
        spiritBrushRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferSpiritBrush);

        /*cmdBufferTreeOfLifeSpeciesTree = new CommandBuffer();
        cmdBufferTreeOfLifeSpeciesTree.name = "cmdBufferTreeOfLifeSpeciesTree";
        treeOfLifeSpeciesTreeRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferTreeOfLifeSpeciesTree);
        */
        //cmdBufferTreeOfLifeDisplay = new CommandBuffer();
        //cmdBufferTreeOfLifeDisplay.name = "cmdBufferTreeOfLifeDisplay";
        //treeOfLifeRenderCamera.AddCommandBuffer(CameraEvent.AfterEverything, cmdBufferTreeOfLifeDisplay);
        
        cmdBufferSlotPortraitDisplay = new CommandBuffer();
        cmdBufferSlotPortraitDisplay.name = "cmdBufferSpeciesPortraitDisplay";
        slotPortraitRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferSlotPortraitDisplay);
        
        cmdBufferResourceSim = new CommandBuffer();
        cmdBufferResourceSim.name = "cmdBufferResourceSim";
        resourceSimRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferResourceSim);
        //simManager.vegetationManager.resourceSimTransferRT = resourceSimRenderCamera.targetTexture;
        //resourceSimRenderCamera.targetTexture = simManager.vegetationManager.resourceSimTransferRT;
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
        baronVonTerrain.computeShaderTerrainGeneration.SetTexture(kernelGetObjectDepths, "AltitudeRead", baronVonTerrain.terrainHeightDataRT);
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
            Vector3 agentPos = simManager.agentsArray[i].bodyRigidbody.transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(agentPos.x, agentPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = simManager.agentsArray[i].facingDirection;
            float deadMult = 1f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                deadMult = 0f;
            }
            obstacleStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.agentsArray[i].currentBoundingBoxSize.x, simManager.agentsArray[i].currentBoundingBoxSize.y) * deadMult; // Vector2.one * 5.5f * simManager.agentsArray[i].sizePercentage; // new Vector2(simManager.agentsArray[i].transform.localScale.x, simManager.agentsArray[i].transform.localScale.y) * 2.9f; // ** revisit this later // should leave room for velSampling around Agent *** weird popping when * 0.9f

            float velX = Mathf.Clamp(simManager.agentsArray[i].ownVel.x, -100f, 100f) * velScale * 0.01f; // agentPos.x - simManager.agentsArray[i]._PrevPos.x * velScale;
            float velY = Mathf.Clamp(simManager.agentsArray[i].ownVel.y, -100f, 100f) * velScale * 0.01f;
            

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }
        // FOOD:
        /*baseIndex = simManager.agentsArray.Length;
        for(int i = 0; i < simManager.eggSackArray.Length; i++) {
            Vector3 foodPos = simManager.eggSackArray[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(foodPos.x, foodPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = simManager.eggSackArray[i].facingDirection;
            obstacleStrokeDataArray[baseIndex + i].scale = simManager.eggSackArray[i].curSize * 0.95f;

            float velX = (foodPos.x - simManager.eggSackArray[i]._PrevPos.x) * velScale;
            float velY = (foodPos.y - simManager.eggSackArray[i]._PrevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }*/
        // PREDATORS:
        /*baseIndex = simManager.agentsArray.Length + simManager.eggSackArray.Length;
        for(int i = 0; i < simManager.predatorArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = new Vector2(Mathf.Cos(simManager.predatorArray[i].transform.rotation.z), Mathf.Sin(simManager.predatorArray[i].transform.rotation.z));
            obstacleStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.predatorArray[i].curScale, simManager.predatorArray[i].curScale) * 0.95f;

            float velX = (predatorPos.x - simManager.predatorArray[i]._PrevPos.x) * velScale;
            float velY = (predatorPos.y - simManager.predatorArray[i]._PrevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }*/

        obstacleStrokesCBuffer.SetData(obstacleStrokeDataArray);
    }
    private void PopulateColorInjectionBuffer() {
        
        int baseIndex = 0;
        // AGENTS:
        for(int i = 0; i < simManager.agentsArray.Length; i++) {
            Vector3 agentPos = simManager.agentsArray[i].bodyRigidbody.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(agentPos.x, agentPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = simManager.agentsArray[i].facingDirection;
            colorInjectionStrokeDataArray[baseIndex + i].scale = simManager.agentsArray[i].fullSizeBoundingBox * 1.55f; // * simManager.agentsArray[i].sizePercentage;
            
            float agentAlpha = 0.024f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                agentAlpha = 2.2f / simManager.agentsArray[i].fullSizeBoundingBox.magnitude;
            }
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                agentAlpha = 3f * Mathf.Clamp01(1f - (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter * 2f / (float)simManager.agentsArray[i]._DecayDurationTimeSteps);
            }
            // ********** BROKEN BY SPECIATION UPDATE!!!! *****************************
            Color drawColor = new Color(1f, 1f, 1f, 3f);
            if(simManager.agentsArray[i].candidateRef != null) {
                Vector3 rgb = simManager.agentsArray[i].candidateRef.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
                if(i % 2 == 0) {
                    rgb = simManager.agentsArray[i].candidateRef.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
                }
                drawColor = new Color(rgb.x, rgb.y, rgb.z, 1.1f); // agentAlpha);
            }
            
            
            /*if(simManager.agentsArray[i].wasImpaled) {
                drawColor.r = 0.8f;
                drawColor.g = 0.1f;
                drawColor.b = 0f;
                drawColor.a = 1.4f;
                colorInjectionStrokeDataArray[baseIndex + i].scale *= 1.25f;
            }*/
            colorInjectionStrokeDataArray[baseIndex + i].color = drawColor;
            
        }
        // FOOD:
        baseIndex = simManager.agentsArray.Length;
        for(int i = 0; i < simManager.eggSackArray.Length; i++) {
            Vector3 foodPos = simManager.eggSackArray[i].transform.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(foodPos.x, foodPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = simManager.eggSackArray[i].facingDirection;
            colorInjectionStrokeDataArray[baseIndex + i].scale = simManager.eggSackArray[i].curSize * 1.0f;
            
            float foodAlpha = 0.06f;
            if(simManager.eggSackArray[i].isBeingEaten > 0.5) {
                foodAlpha = 1.2f;
            }

            colorInjectionStrokeDataArray[baseIndex + i].color = new Vector4(Mathf.Lerp(simManager.eggSackGenomePoolArray[i].fruitHue.x, 0.1f, 0.7f), Mathf.Lerp(simManager.eggSackGenomePoolArray[i].fruitHue.y, 0.9f, 0.7f), Mathf.Lerp(simManager.eggSackGenomePoolArray[i].fruitHue.z, 0.2f, 0.7f), foodAlpha);
        }
        // PREDATORS:
        /*baseIndex = simManager.agentsArray.Length + simManager.eggSackArray.Length;
        for(int i = 0; i < simManager.predatorArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = new Vector2(Mathf.Cos(simManager.predatorArray[i].transform.rotation.z), Mathf.Sin(simManager.predatorArray[i].transform.rotation.z));
            colorInjectionStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.predatorArray[i].curScale, simManager.predatorArray[i].curScale) * 0.9f;
            
            colorInjectionStrokeDataArray[baseIndex + i].color = new Vector4(1f, 0.25f, 0f, 0.2f);
        }*/

        colorInjectionStrokesCBuffer.SetData(colorInjectionStrokeDataArray);
    }
    
    /*public void UpdateAgentWidthsTexture(Agent agent) {
        for(int i = 0; i < agent.agentWidthsArray.Length; i++) {
            critterBodyWidthsTex.SetPixel(i, agent.index, new Color(agent.agentWidthsArray[i], agent.agentWidthsArray[i], agent.agentWidthsArray[i]));
        }
        critterBodyWidthsTex.Apply();
    }*/

    /*public void UpdateAgentBodyStrokesBuffer(int agentIndex) {
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
    }  */  
    /*public void UpdateAgentEyeStrokesBuffer(int agentIndex) {
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
    }*/
    /*public void UpdateAgentSmearStrokesBuffer(int agentIndex) {
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
    }  */  
    
    public void UpdateDynamicFoodBuffers(int eggSackIndex) {
        // *** Hard-coded 64 Fruits per food object!!!! *** BEWARE!!!
        ComputeBuffer eggsUpdateCBuffer = new ComputeBuffer(simManager._NumEggSacks, sizeof(float) * 8 + sizeof(int) * 1);

        SimulationStateData.EggData[] eggDataArray = new SimulationStateData.EggData[simManager._NumEggSacks];
        for(int i = 0; i < eggDataArray.Length; i++) {
            eggDataArray[i] = new SimulationStateData.EggData();
            eggDataArray[i].eggSackIndex = eggSackIndex;
            Vector3 randSphere = UnityEngine.Random.insideUnitSphere;
            eggDataArray[i].localCoords = UnityEngine.Random.insideUnitCircle; // new Vector2(randSphere.x, randSphere.y); // * 0.5f + UnityEngine.Random.insideUnitCircle * 0.4f;
            eggDataArray[i].localScale = Vector2.one * 0.25f; // simManager.eggSackGenomePoolArray[eggSackIndex].fruitScale;  
            eggDataArray[i].worldPos = simManager.eggSackArray[eggSackIndex].transform.position;
            eggDataArray[i].attached = 1f;
        }        
        eggsUpdateCBuffer.SetData(eggDataArray);
        int kernelCSUpdateDynamicEggBuffers = computeShaderEggSacks.FindKernel("CSUpdateDynamicEggBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderEggSacks.SetBuffer(kernelCSUpdateDynamicEggBuffers, "eggSackSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
        computeShaderEggSacks.SetBuffer(kernelCSUpdateDynamicEggBuffers, "eggDataUpdateCBuffer", eggsUpdateCBuffer);
        computeShaderEggSacks.SetBuffer(kernelCSUpdateDynamicEggBuffers, "eggDataWriteCBuffer", simManager.simStateData.eggDataCBuffer);
        computeShaderEggSacks.Dispatch(kernelCSUpdateDynamicEggBuffers, 1, 1, 1);        
        eggsUpdateCBuffer.Release();
        
        /*
        //if(isPlant) {
        ComputeBuffer singleStemCBuffer = new ComputeBuffer(1, sizeof(float) * 7 + sizeof(int) * 1);
        SimulationStateData.StemData[] singleStemDataArray = new SimulationStateData.StemData[1];        
        singleStemDataArray[0] = new SimulationStateData.StemData();
        singleStemDataArray[0].foodIndex = foodIndex;
        singleStemDataArray[0].localBaseCoords = new Vector2(0f, -1f);
        singleStemDataArray[0].localTipCoords = new Vector2(0f, 1f);
        singleStemDataArray[0].childGrowth = 0f;
        singleStemDataArray[0].width = simManager.eggSackGenomePoolArray[foodIndex].stemWidth;
        singleStemDataArray[0].attached = 1f;
        singleStemCBuffer.SetData(singleStemDataArray);
        int kernelCSUpdateDynamicStemBuffers = computeShaderBrushStrokes.FindKernel("CSUpdateDynamicStemBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicStemBuffers, "foodSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
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
            foodLeafDataArray[i].localScale =  simManager.eggSackGenomePoolArray[foodIndex].leafScale;          
            foodLeafDataArray[i].attached = 1f;
        }
        foodLeafUpdateCBuffer.SetData(foodLeafDataArray);
        int kernelCSUpdateDynamicLeafBuffers = computeShaderBrushStrokes.FindKernel("CSUpdateDynamicLeafBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicLeafBuffers, "foodSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicLeafBuffers, "foodLeafDataUpdateCBuffer", foodLeafUpdateCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSUpdateDynamicLeafBuffers, "foodLeafDataWriteCBuffer", simManager.simStateData.foodLeafDataCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSUpdateDynamicLeafBuffers, 1, 1, 1);        
        foodLeafUpdateCBuffer.Release();
        */
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

    public void UpdateCritterGenericStrokesData(Agent agent) { //int critterIndex, AgentGenome genome) {
        ComputeBuffer singleCritterGenericStrokesCBuffer = new ComputeBuffer(GetNumUberStrokesPerCritter(), GetMemorySizeCritterUberStrokeData());
        CritterUberStrokeData[] singleCritterGenericStrokesArray = new CritterUberStrokeData[singleCritterGenericStrokesCBuffer.count];  // optimize this later?? ***
        CritterGenomeInterpretor.BrushPoint[] brushPointArray = new CritterGenomeInterpretor.BrushPoint[GetNumUberStrokesPerCritter()];
        
        // Generate main body strokes:
        GenerateCritterBodyBrushstrokes(ref singleCritterGenericStrokesArray, brushPointArray, agent.candidateRef.candidateGenome, agent.index); 
        
        // Loop through all points again and calculate normals/tangents/other things:
        CalculateCritterBodyBrushstrokesNormals(ref singleCritterGenericStrokesArray, brushPointArray);        
        
        // Create Eye points here:
        GenerateCritterEyeBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);

        // Mouth
        GenerateCritterMouthTeethBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);
        // Teeth

        // Pectoral Fins
        GenerateCritterPectoralFinsBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);
        // Dorsal Fin
        GenerateCritterDorsalFinBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);
        // Tail Fin
        GenerateCritterTailFinBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);
        // Skin Detail
        GenerateCritterSkinDetailBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);
        // SORT BRUSHSTROKES:::
        SortCritterBrushstrokes(ref singleCritterGenericStrokesArray, agent.index);
                
        // SET DATA:::        
        singleCritterGenericStrokesCBuffer.SetData(singleCritterGenericStrokesArray); // send data to gPU
        int kernelCSUpdateCritterGenericStrokes = computeShaderCritters.FindKernel("CSUpdateCritterGenericStrokes");        
        computeShaderCritters.SetBuffer(kernelCSUpdateCritterGenericStrokes, "critterGenericStrokesWriteCBuffer", critterGenericStrokesCBuffer);
        computeShaderCritters.SetBuffer(kernelCSUpdateCritterGenericStrokes, "critterGenericStrokesUpdateCBuffer", singleCritterGenericStrokesCBuffer);
        computeShaderCritters.SetInt("_UpdateBufferStartIndex", agent.index * singleCritterGenericStrokesCBuffer.count);
        computeShaderCritters.Dispatch(kernelCSUpdateCritterGenericStrokes, singleCritterGenericStrokesCBuffer.count, 1, 1);        
        singleCritterGenericStrokesCBuffer.Release(); 
    }
    public void GenerateCritterPortraitStrokesData(AgentGenome genome) {
        //ComputeBuffer singleCritterGenericStrokesCBuffer = new ComputeBuffer(GetNumUberStrokesPerCritter(), GetMemorySizeCritterUberStrokeData());
        CritterUberStrokeData[] singleCritterGenericStrokesArray = new CritterUberStrokeData[toolbarCritterPortraitStrokesCBuffer.count];  // optimize this later?? ***

        CritterGenomeInterpretor.BrushPoint[] brushPointArray = new CritterGenomeInterpretor.BrushPoint[GetNumUberStrokesPerCritter()];        
        // Generate main body strokes:
        GenerateCritterBodyBrushstrokes(ref singleCritterGenericStrokesArray, brushPointArray, genome, 0);         
        // Loop through all points again and calculate normals/tangents/other things:
        CalculateCritterBodyBrushstrokesNormals(ref singleCritterGenericStrokesArray, brushPointArray);
        // Create Eye points here:
        GenerateCritterEyeBrushstrokes(ref singleCritterGenericStrokesArray, genome, 0);
        // Mouth
        GenerateCritterMouthTeethBrushstrokes(ref singleCritterGenericStrokesArray, genome, 0);
        // Teeth
        // Pectoral Fins
        GenerateCritterPectoralFinsBrushstrokes(ref singleCritterGenericStrokesArray, genome, 0);
        // Dorsal Fin
        GenerateCritterDorsalFinBrushstrokes(ref singleCritterGenericStrokesArray, genome, 0);
        // Tail Fin
        GenerateCritterTailFinBrushstrokes(ref singleCritterGenericStrokesArray, genome, 0);
        // Skin Detail
        GenerateCritterSkinDetailBrushstrokes(ref singleCritterGenericStrokesArray, genome, 0);
        // SORT BRUSHSTROKES:::
        SortCritterBrushstrokes(ref singleCritterGenericStrokesArray, 0);
        
        toolbarCritterPortraitStrokesCBuffer.SetData(singleCritterGenericStrokesArray);


        //float size = (genome.bodyGenome.fullsizeBoundingBox.x + genome.bodyGenome.fullsizeBoundingBox.y) * 5f;        
        //float sizeNormalized = Mathf.Clamp01((size - 0.1f) / 1f);
        genome.bodyGenome.CalculateFullsizeBoundingBox();
        float minLength = 0.5f;
        float maxLength = 40f;
        float sizeNormalized = Mathf.Clamp01((genome.bodyGenome.fullsizeBoundingBox.y - minLength) / (maxLength - minLength));
        //sizeNormalized = 1f;
        slotPortraitRenderCamera.GetComponent<CritterPortraitCameraManager>().UpdateCameraTargetValues(sizeNormalized);
        
        Debug.Log("GenerateCritterPortraitStrokesData: " + genome.bodyGenome.appearanceGenome.huePrimary.ToString());
    }

    private void GenerateCritterBodyBrushstrokes(ref CritterUberStrokeData[] strokesArray, CritterGenomeInterpretor.BrushPoint[] brushPointArray, AgentGenome agentGenome, int agentIndex) {
        // Loop through all brush points, starting at the tip of the tail (underside), (x=0, y=0, z=1)
        // ... Then working its way to tip of head by doing series of cross-section rings
        for(int y = 0; y < numStrokesPerCritterLength; y++) {

            float yLerp = Mathf.Clamp01((float)y / (float)(numStrokesPerCritterLength - 1)); // start at tail (Y = 0)
            
            for(int a = 0; a < numStrokesPerCritterCross; a++) {
                
                int brushIndex = y * numStrokesPerCritterCross + a;         
                float angleRad = ((float)a / (float)numStrokesPerCritterCross) * Mathf.PI * 2f; // verticalLerpPos * Mathf.PI;   
                float crossSectionCoordX = Mathf.Sin(angleRad);
                float crossSectionCoordZ = Mathf.Cos(angleRad);
                Vector2 crossSectionNormalizedCoords = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * 1f;
                
                CritterGenomeInterpretor.BrushPoint newBrushPoint = new CritterGenomeInterpretor.BrushPoint();                
                newBrushPoint.initCoordsNormalized = new Vector3(crossSectionCoordX, yLerp, crossSectionCoordZ);
                newBrushPoint.uv = new Vector2((float)a / (float)numStrokesPerCritterCross, (float)y / (float)numStrokesPerCritterLength);
                newBrushPoint.ix = a;
                newBrushPoint.iy = y;
                newBrushPoint = CritterGenomeInterpretor.ProcessBrushPoint(newBrushPoint, agentGenome);                
                brushPointArray[brushIndex] = newBrushPoint;

                // Create Data:
                CritterUberStrokeData newData = new CritterUberStrokeData();
                newData.parentIndex = agentIndex;                
                newData.brushType = 0;
                newData.t = yLerp;
                newData.bindPos = newBrushPoint.bindPos;
                newData.scale = Vector2.one; // new Vector2(UnityEngine.Random.Range(0.75f, 1.33f), UnityEngine.Random.Range(0.75f, 1.33f));
                newData.uv = newBrushPoint.uv;
                newData.thresholdValue = UnityEngine.Random.Range(0f, 1f);
                if(crossSectionCoordZ >= 0f) {
                    newData.jawMask = -1f;
                }
                else {
                    newData.jawMask = 1f;
                }
                strokesArray[brushIndex] = newData;
            }
        }
    }
    private void CalculateCritterBodyBrushstrokesNormals(ref CritterUberStrokeData[] strokesArray, CritterGenomeInterpretor.BrushPoint[] brushPointArray) {

        for (int y = 0; y < numStrokesPerCritterLength; y++) {                        
                        
            for(int a = 0; a < numStrokesPerCritterCross; a++) {
                // do a line from head to tail at same altitude:
                int indexCenter = y * numStrokesPerCritterCross + a;

                // find neighbor positions: (all in bindPos object coordinates)
                int indexNegX = y * numStrokesPerCritterCross + Mathf.Clamp((a - 1), 0, numStrokesPerCritterCross - 1); // switch to modulo arithmetic for wrapping!
                int indexPosX = y * numStrokesPerCritterCross + Mathf.Clamp((a + 1), 0, numStrokesPerCritterCross - 1);
                int indexNegY = Mathf.Clamp((y - 1), 0, numStrokesPerCritterLength - 1) * numStrokesPerCritterCross + a;
                int indexPosY = Mathf.Clamp((y + 1), 0, numStrokesPerCritterLength - 1) * numStrokesPerCritterCross + a;
                
                Vector3 uTangentAvg = (brushPointArray[indexPosX].bindPos - brushPointArray[indexNegX].bindPos);
                Vector3 vTangentAvg = (brushPointArray[indexPosY].bindPos - brushPointArray[indexNegY].bindPos);

                Vector2 scale;
                Vector3 normal;
                Vector3 tangent;

                if (y == 0) {  // tailtip
                    scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude) * 0.5f;
                    normal = new Vector3(0f, -1f, 0f);
                    tangent = new Vector3(0f, 0f, 1f);
                }
                else if (y == numStrokesPerCritterLength - 1) {  // headTip
                    scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude) * 0.5f;
                    normal = new Vector3(0f, 1f, 0f);
                    tangent = new Vector3(0f, 0f, 1f);
                }
                else {  // body
                    scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude);
                    normal = Vector3.Cross(uTangentAvg, vTangentAvg).normalized;
                    tangent = vTangentAvg.normalized;
                }

                //Vector3 bitangent = Vector3.Cross(normal, tangent);

                float offsetShiftX = (float)(y % 2) * 0.25f;
                float randShift = UnityEngine.Random.Range(-1f, 1f) * 0.05f;
                strokesArray[indexCenter].bindPos += uTangentAvg * offsetShiftX - vTangentAvg * randShift;
                //brushPointArray[indexCenter].normal = normal; // not needed?>
                Vector2 randScale = new Vector2(UnityEngine.Random.Range(0.75f, 1.33f), UnityEngine.Random.Range(0.75f, 1.33f));
                strokesArray[indexCenter].scale = new Vector2(scale.x * randScale.x, scale.y * randScale.y);
                strokesArray[indexCenter].bindNormal = (normal + UnityEngine.Random.insideUnitSphere * 0.165f).normalized;
                strokesArray[indexCenter].bindTangent = tangent;

                strokesArray[indexCenter].neighborIndex = indexNegY;
                strokesArray[indexCenter].neighborAlign = (1f - strokesArray[indexCenter].t);
            }
        }
    }
    private void GenerateCritterEyeBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {

        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross;

        CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;
        //float fullCritterLength = segmentsSummedCritterLength * gene.creatureBaseLength;
        //float fullCritterWidth = gene.creatureBaseLength / gene.creatureBaseAspectRatio;
        float critterSizeScore = gene.creatureBaseLength * gene.creatureAspectRatio; ; // 1f;// fullCritterWidth;

        int numEyes = gene.numEyes;
        float eyePosSpread = gene.eyePosSpread;  // 1f == full hemisphere coverage, 0 == top
        //if(numEyes < 2) { eyePosSpread = 0f; }
        float eyeLocAmplitude = gene.eyeLocAmplitude;
        float eyeLocFrequency = gene.eyeLocFrequency;
        float eyeLocOffset = gene.eyeLocOffset;
        
        float socketRadius = gene.socketRadius * critterSizeScore;  // relative to body size?
        float socketHeight = gene.socketHeight * critterSizeScore * 2f; 
        float socketBulge = gene.socketBulge;
        float eyeballRadius = gene.socketRadius * gene.eyeballRadius * critterSizeScore;
        float eyeBulge = gene.eyeBulge;
        float irisWidthFraction = gene.irisWidthFraction;        
        float pupilWidthFraction = gene.pupilWidthFraction;  // percentage of iris size
        float pupilHeightFraction = gene.pupilHeightFraction;
        

        int numPointsPerEye = (int)Mathf.Floor((float)numStrokesPerCritterEyes / (float)numEyes);

        int totalLengthResolution = (int)Mathf.Floor(Mathf.Sqrt(numPointsPerEye));
        int baseCrossResolution = totalLengthResolution;

        int socketLengthResolution = totalLengthResolution / 2;
        int eyeballLengthResolution = totalLengthResolution - socketLengthResolution;

        //Debug.Log("socketLengthResolution: " + socketLengthResolution.ToString() + ", eyeballLengthResolution: " + eyeballLengthResolution.ToString());
        
        for(int eyeIndex = 0; eyeIndex < numEyes; eyeIndex++) {

            // find eye anchor pos and normalDir:            
            
            float headStartCoordV = (gene.bodyLength * 0.9f + gene.tailLength) / segmentsSummedCritterLength;
            float headEndCoordV = (gene.mouthLength * 0.33f + gene.headLength + gene.bodyLength + gene.tailLength) / segmentsSummedCritterLength;

            float uRange = 0.5f * eyePosSpread;            
            
            float eyeLerp = (float)eyeIndex / (float)Mathf.Max(1, numEyes - 1);
            int coordU = Mathf.RoundToInt((eyeLerp * uRange + (1f - uRange) * 0.5f) * (float)numStrokesPerCritterCross);

            float distToCenter = Mathf.Abs(eyeLerp * 2f - 1f);
            //float foreAft = Mathf.Sin(distToCenter * Mathf.PI * eyeLocFrequency + eyeLocOffset) * eyeLocAmplitude;
            //int coordU = Mathf.RoundToInt(0.5f * (float)numStrokesPerCritterCross);
            //int coordV = Mathf.RoundToInt(Mathf.Lerp(headStartCoordV, headEndCoordV, 0f) * (float)numStrokesPerCritterLength);
            int coordV = Mathf.RoundToInt(Mathf.Lerp(headStartCoordV, headEndCoordV, Mathf.Sin(distToCenter * eyeLocFrequency + eyeLocOffset) * eyeLocAmplitude * 0.5f + 0.5f) * (float)numStrokesPerCritterLength);

            int anchorIndex = coordV * numStrokesPerCritterCross + coordU;

            Vector3 eyeAnchorPos = strokesArray[anchorIndex].bindPos;
            Vector3 eyeNormal = -strokesArray[anchorIndex].bindNormal;
            Vector3 eyeTangent = strokesArray[anchorIndex].bindTangent;
            Vector3 eyeBitangent = Vector3.Cross(eyeNormal, eyeTangent);

            //float prevRingRadius = socketRadius; // hack for getting slope of normal?

            for(int z = 0; z < totalLengthResolution; z++) {
                float zFract = (float)z / (float)totalLengthResolution;
                float socketFractZ = Mathf.Clamp01((float)z / (float)(socketLengthResolution - 1));
                float eyeballFractZ = Mathf.Clamp01((float)(z - socketLengthResolution) / (float)(eyeballLengthResolution));

                float socketBulgeMultiplier = Mathf.Sin(socketFractZ * Mathf.PI) * socketBulge + 1f;
                //float eyeballBulgeMultiplier
                float radius = Mathf.Lerp(socketRadius, eyeballRadius, socketFractZ);
                //radius *= Mathf.Cos(eyeballFractZ * Mathf.PI * 0.5f);

                if(z < socketLengthResolution) {
                    radius = Mathf.Lerp(socketRadius, eyeballRadius, socketFractZ) * socketBulgeMultiplier;
                }
                else {
                    radius = Mathf.Cos(eyeballFractZ * Mathf.PI * 0.5f) * eyeballRadius;
                }

                //float deltaRadius = radius - prevRingRadius;
                //float deltaHeight = 1f / (float)totalLengthResolution;
                //float slope = deltaRadius / deltaHeight;
                
                Vector3 ringCenterPos = eyeAnchorPos - eyeNormal * ((socketFractZ - 0.05f) * socketHeight + eyeballFractZ * eyeballRadius * eyeBulge);

                for (int a = 0; a < baseCrossResolution; a++) {

                    int eyeBrushPointIndex = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + a;

                    float ringX = Mathf.Cos((float)a / (float)baseCrossResolution * Mathf.PI * 2f);
                    float ringY = Mathf.Sin((float)a / (float)baseCrossResolution * Mathf.PI * 2f);

                    Vector3 ringNormal = (eyeBitangent * ringX + eyeTangent * ringY);                    
                    Vector3 offset = ringNormal * radius;
                                        
                    //Vector3 tempNormal = Mathf.Cos(zFract * Mathf.PI * 0.5f) * ringNormal + Mathf.Sin(zFract * Mathf.PI * 0.5f) * eyeNormal;
                    //Vector3 ringTangent = Vector3.Cross(ringNormal, eyeNormal);
                    Vector4 color = Vector4.zero;

                    if(z > socketLengthResolution) {  // Is Part of Eyeball!
                        color = new Vector4(gene.eyeballHue.x, gene.eyeballHue.y, gene.eyeballHue.z, 1f);

                        if (radius / eyeballRadius < irisWidthFraction) {  // Is part of IRIS                        
                            color = new Vector4(gene.irisHue.x, gene.irisHue.y, gene.irisHue.z, 1f);

                            if (radius / eyeballRadius < pupilWidthFraction && radius / eyeballRadius < pupilHeightFraction) {  // PUPIL                        
                                color = new Vector4(0f, 0f, 0f, 1f);
                            }
                        }                        
                    }
                    
                    // create stroke:
                    CritterUberStrokeData newData = new CritterUberStrokeData();
                    newData.parentIndex = agentIndex;
                    newData.brushType = 0;
                    newData.t = strokesArray[anchorIndex].t;
                    newData.bindPos = ringCenterPos + offset; // new Vector3(0f, 0f, 0f); // ringCenterPos
                    //newData.scale = Vector2.one * 0.15f; // new Vector2(UnityEngine.Random.Range(0.75f, 1.33f), UnityEngine.Random.Range(0.75f, 1.33f));
                    newData.uv = strokesArray[anchorIndex].uv + new Vector2(ringX, ringY) * socketFractZ * gene.socketHeight * 0.1f;
                    newData.color = color;
                    newData.jawMask = 1f;
                    newData.thresholdValue = UnityEngine.Random.Range(0f, 1f);
                    //newData.bindNormal = tempNormal; //.normalized; // -offset.normalized;
                    //newData.bindTangent = ringTangent; // eyeNormal; // strokesArray[anchorIndex].bindTangent;
                    strokesArray[eyeBrushPointIndex] = newData;
                }
            }

            // CALCULATE NORMALS EMPIRICALLY:
            for(int z = 0; z < totalLengthResolution; z++) {                
                
                for (int a = 0; a < baseCrossResolution; a++) {

                    int indexCenter = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + a;

                    // find neighbor positions: (all in bindPos object coordinates)+
                    int indexNegX = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + Mathf.Clamp((a - 1), 0, baseCrossResolution - 1);
                    int indexPosX = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + Mathf.Clamp((a + 1), 0, baseCrossResolution - 1);
                    int indexNegY = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + Mathf.Clamp((z - 1), 0, totalLengthResolution - 1) * baseCrossResolution + a;
                    int indexPosY = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + Mathf.Clamp((z + 1), 0, totalLengthResolution - 1) * baseCrossResolution + a;
                                    
                    Vector3 uTangentAvg = (strokesArray[indexPosX].bindPos - strokesArray[indexNegX].bindPos);
                    Vector3 vTangentAvg = (strokesArray[indexPosY].bindPos - strokesArray[indexNegY].bindPos);

                    Vector2 scale;
                    Vector3 normal;
                    Vector3 tangent;                    
                    
                    if (z == totalLengthResolution - 1) {  // PUPIL
                        scale = Vector2.one * eyeballRadius * irisWidthFraction * pupilHeightFraction * 0.5f; // new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude);
                        normal = -eyeNormal;
                        tangent = eyeTangent;
                        //color = new Vector4(0f, 0f, 0f, 1f);
                    }
                    else {  // body
                        scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude);
                        normal = Vector3.Cross(uTangentAvg, vTangentAvg).normalized;
                        tangent = vTangentAvg.normalized;
                    }
                    
                    int eyeBrushPointIndex = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + a;
                    //scale.y *= 1.15f;
                    strokesArray[eyeBrushPointIndex].scale = scale;
                    strokesArray[eyeBrushPointIndex].bindNormal = normal; //.normalized; // -offset.normalized;
                    strokesArray[eyeBrushPointIndex].bindTangent = tangent; // eyeNormal; // strokesArray[anchorIndex].bindTangent;
                    //strokesArray[eyeBrushPointIndex].color = color;
                    /*if(z >= socketLengthResolution) {  // Is Part of Eyeball!
                        
                    }
                    else {
                        strokesArray[eyeBrushPointIndex].bindPos -= vTangentAvg * 0.5f;
                    }*/
                }
            }
        }
    }

    private void GenerateCritterMouthTeethBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {
        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross + numStrokesPerCritterEyes;

        CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;

        float startCoordY = (gene.headLength + gene.bodyLength + gene.tailLength) / segmentsSummedCritterLength;
        // how many rows? 1 at first

        int numStrokesPerRowSide = numStrokesPerCritterMouth / 4;
        float lipThickness = 0.05f;  // how to scale with creature size?
        // maybe better to interpolate between existing body strokes? measure size by single row?
        
        // JUST TEETH FOR NOW::::

        // RIGHT side:
        for(int y = 0; y < numStrokesPerRowSide; y++) {
            float yLerp = Mathf.Lerp(startCoordY, 1f, Mathf.Clamp01((float)y / (float)(numStrokesPerRowSide - 1))); // start at tail (Y = 0)            
            int brushIndexTop = arrayIndexStart + y;
            int brushIndexBottom = arrayIndexStart + numStrokesPerRowSide * 2 + y;

            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(1f, yLerp, 0f), new Vector2(0.25f, yLerp), brushIndexTop);
            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(1f, yLerp, 0f), new Vector2(0.25f, yLerp), brushIndexBottom);
                        
            if(y == 0) {

            }
            else {
                Vector3 uTangentAvg = new Vector3(0f, 0f, 1f);
                Vector3 vTangentAvg = (strokesArray[brushIndexTop].bindPos - strokesArray[brushIndexTop - 1].bindPos);
                strokesArray[brushIndexTop].scale = new Vector2(0.1f, vTangentAvg.magnitude);
                strokesArray[brushIndexTop].bindNormal = Vector3.Cross(vTangentAvg, uTangentAvg).normalized;
                strokesArray[brushIndexTop].bindTangent = vTangentAvg.normalized;
                strokesArray[brushIndexTop].color = Vector4.one;
                strokesArray[brushIndexTop].jawMask = 1f;

                strokesArray[brushIndexBottom].scale = new Vector2(0.1f, vTangentAvg.magnitude);
                strokesArray[brushIndexBottom].bindNormal = Vector3.Cross(vTangentAvg, uTangentAvg).normalized;
                strokesArray[brushIndexBottom].bindTangent = vTangentAvg.normalized;
                strokesArray[brushIndexBottom].color = Vector4.one;
                strokesArray[brushIndexBottom].jawMask = -1f;
            }
            
        }
        // LEFT side:
        for(int y = 0; y < numStrokesPerRowSide; y++) {
            float yLerp = Mathf.Lerp(startCoordY, 1f, Mathf.Clamp01((float)y / (float)(numStrokesPerRowSide - 1))); // start at tail (Y = 0)            
            int brushIndexTop = arrayIndexStart + numStrokesPerRowSide + y;
            int brushIndexBottom = arrayIndexStart + numStrokesPerRowSide * 3 + y;

            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(-1f, yLerp, 0f), new Vector2(0.75f, yLerp), brushIndexTop); 
            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(-1f, yLerp, 0f), new Vector2(0.75f, yLerp), brushIndexBottom); 
            
            if(y == 0) {

            }
            else {
                Vector3 uTangentAvg = new Vector3(0f, 0f, 1f);
                Vector3 vTangentAvg = (strokesArray[brushIndexTop].bindPos - strokesArray[brushIndexTop - 1].bindPos);
                strokesArray[brushIndexTop].scale = new Vector2(0.1f, vTangentAvg.magnitude);
                strokesArray[brushIndexTop].bindNormal = Vector3.Cross(uTangentAvg, vTangentAvg).normalized;
                strokesArray[brushIndexTop].bindTangent = vTangentAvg.normalized;
                strokesArray[brushIndexTop].color = Vector4.one;
                strokesArray[brushIndexTop].jawMask = 1f;

                strokesArray[brushIndexBottom].scale = new Vector2(0.1f, vTangentAvg.magnitude);
                strokesArray[brushIndexBottom].bindNormal = Vector3.Cross(uTangentAvg, vTangentAvg).normalized;
                strokesArray[brushIndexBottom].bindTangent = vTangentAvg.normalized;
                strokesArray[brushIndexBottom].color = Vector4.one;
                strokesArray[brushIndexBottom].jawMask = -1f;
            }
        }

        /*
        int indexCenter = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + a;

        // find neighbor positions: (all in bindPos object coordinates)+
        int indexNegX = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + Mathf.Clamp((a - 1), 0, baseCrossResolution - 1);
        int indexPosX = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + Mathf.Clamp((a + 1), 0, baseCrossResolution - 1);
        int indexNegY = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + Mathf.Clamp((z - 1), 0, totalLengthResolution - 1) * baseCrossResolution + a;
        int indexPosY = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + Mathf.Clamp((z + 1), 0, totalLengthResolution - 1) * baseCrossResolution + a;
                                    
        Vector3 uTangentAvg = (strokesArray[indexPosX].bindPos - strokesArray[indexNegX].bindPos);
        Vector3 vTangentAvg = (strokesArray[indexPosY].bindPos - strokesArray[indexNegY].bindPos);

        Vector2 scale;
        Vector3 normal;
        Vector3 tangent;                    
                    
        if (z == totalLengthResolution - 1) {  // PUPIL
            scale = Vector2.one * eyeballRadius * irisWidthFraction * pupilHeightFraction * 0.2f; // new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude);
            normal = eyeNormal;
            tangent = eyeTangent;
            //color = new Vector4(0f, 0f, 0f, 1f);
        }
        else {  // body
            scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude);
            normal = Vector3.Cross(vTangentAvg, uTangentAvg).normalized;
            tangent = vTangentAvg.normalized;
        }
                    
        int eyeBrushPointIndex = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + a;
        strokesArray[eyeBrushPointIndex].scale = scale * 1.15f;
        strokesArray[eyeBrushPointIndex].bindNormal = normal; //.normalized; // -offset.normalized;
        strokesArray[eyeBrushPointIndex].bindTangent = tangent;
        */
    }
    private void AddUberBrushPoint(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex, Vector3 initCoords, Vector2 uv, int brushIndex) {
        CritterGenomeInterpretor.BrushPoint newBrushPoint = new CritterGenomeInterpretor.BrushPoint();                
        newBrushPoint.initCoordsNormalized = initCoords;
        newBrushPoint.uv = uv;                
        newBrushPoint = CritterGenomeInterpretor.ProcessBrushPoint(newBrushPoint, agentGenome);                
            
        // Create Data:
        CritterUberStrokeData newData = new CritterUberStrokeData();
        newData.parentIndex = agentIndex;
        newData.brushType = 0;
        newData.t = initCoords.y;
        newData.bindPos = newBrushPoint.bindPos;
        newData.scale = Vector2.one * 1f;
        newData.uv = newBrushPoint.uv;
        newData.thresholdValue = UnityEngine.Random.Range(0f, 1f);

        strokesArray[brushIndex] = newData;
    }
    private void GenerateCritterPectoralFinsBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {

    }
    private void GenerateCritterDorsalFinBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {
        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross + numStrokesPerCritterEyes + numStrokesPerCritterMouth + numStrokesPerCritterTeeth + numStrokesPerCritterPectoralFins;

        CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;

        // genome parameters:
        float startCoordY = gene.dorsalFinStartCoordY; // (gene.tailLength * 0.5f) / segmentsSummedCritterLength;
        float endCoordY = gene.dorsalFinEndCoordY;  // (gene.headLength * 0.5f + gene.bodyLength + gene.tailLength) / segmentsSummedCritterLength;
        float slantAmount = gene.dorsalFinSlantAmount;
        float baseHeight = gene.dorsalFinBaseHeight;
        


        int numRows = 4;
        int pointsPerRow = numStrokesPerCritterDorsalFin / numRows / 2;  // 2 one facing each side

        
        // Initial position:
        for (int i = 0; i < numRows; i++) {
            
            for(int y = 0; y < pointsPerRow; y++) {
                float yLerp = Mathf.Lerp(startCoordY, endCoordY, Mathf.Clamp01((float)y / (float)(pointsPerRow - 1))); // start at tail (Y = 0)            
                int brushIndexLeft = arrayIndexStart + pointsPerRow * i + y;
                int brushIndexRight = arrayIndexStart + pointsPerRow * i + y + (numStrokesPerCritterDorsalFin / 2);

                AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(0f, yLerp, -1f), new Vector2(0.5f, yLerp), brushIndexLeft);
                AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(0f, yLerp, -1f), new Vector2(0.5f, yLerp), brushIndexRight);
                                
                if(y == 0) {

                }
                else { 
                    Vector3 uTangentAvg = (strokesArray[brushIndexLeft].bindPos - strokesArray[brushIndexLeft - 1].bindPos);
                    Vector3 vTangentAvg = new Vector3(0f, 0f, -1f);
                    strokesArray[brushIndexLeft].scale = new Vector2(uTangentAvg.magnitude, 0.125f) * baseHeight; // vTangentAvg.magnitude);
                    strokesArray[brushIndexLeft].bindNormal = (new Vector3(1f, 0f, -0.5f) + UnityEngine.Random.insideUnitSphere * 0.3f).normalized;
                    strokesArray[brushIndexLeft].bindTangent = new Vector3(0f, -slantAmount, -1f);
                    //strokesArray[brushIndexLeft].color = Vector4.one;
                    strokesArray[brushIndexLeft].jawMask = 1f;
                    strokesArray[brushIndexLeft].neighborIndex = arrayIndexStart + pointsPerRow * i + Mathf.Max(0, (y - 1));
                    strokesArray[brushIndexLeft].neighborAlign = 1f;

                    strokesArray[brushIndexRight].scale = strokesArray[brushIndexLeft].scale; // vTangentAvg.magnitude);
                    strokesArray[brushIndexRight].bindNormal = (new Vector3(-1f, 0f, -0.5f) + UnityEngine.Random.insideUnitSphere * 0.3f).normalized;
                    strokesArray[brushIndexRight].bindTangent = new Vector3(0f, -slantAmount, -1f);
                    //strokesArray[brushIndexRight].color = Vector4.one;
                    strokesArray[brushIndexRight].jawMask = 1f;
                    strokesArray[brushIndexRight].neighborIndex = arrayIndexStart + pointsPerRow * i + Mathf.Max(0, (y - 1)) + (numStrokesPerCritterDorsalFin / 2);
                    strokesArray[brushIndexRight].neighborAlign = 1f;
                    
                } 
            }
        }
        
        // Offset placement from anchor pos:
        for(int i = 0; i < numRows; i++) {
            // top:
            
            for(int y = 0; y < pointsPerRow; y++) {
                float yLerp = Mathf.Lerp(startCoordY, endCoordY, Mathf.Clamp01((float)y / (float)(pointsPerRow - 1))); // start at tail (Y = 0)            
                int brushIndexLeft = arrayIndexStart + pointsPerRow * i + y;
                int brushIndexRight = arrayIndexStart + pointsPerRow * i + (numStrokesPerCritterDorsalFin / 2) + y;
       
                if(y == 0) {

                }
                else {
                    
                    // figure out height at this point
                    float finHeight = Mathf.Sin(Mathf.PI * Mathf.Clamp01((float)y / (float)(pointsPerRow - 1)));
                    strokesArray[brushIndexLeft].scale.y *= finHeight;
                    strokesArray[brushIndexRight].scale.y *= finHeight;
                    strokesArray[brushIndexLeft].bindPos += strokesArray[brushIndexLeft].bindTangent * strokesArray[brushIndexLeft].scale.y * ((float)i - 0f);
                    strokesArray[brushIndexRight].bindPos += strokesArray[brushIndexRight].bindTangent * strokesArray[brushIndexRight].scale.y * ((float)i - 0f);
                    //strokesArray[brushIndexLeft].bindPos += strokesArray[brushIndexLeft].bindNormal * strokesArray[brushIndexLeft].scale.y / ((float)i - 0f) * 0.55f * UnityEngine.Random.Range(0.9f, 1.125f);
                    //strokesArray[brushIndexRight].bindPos += strokesArray[brushIndexRight].bindNormal * strokesArray[brushIndexRight].scale.y / ((float)i - 0f) * 0.55f * UnityEngine.Random.Range(0.9f, 1.125f);
                    strokesArray[brushIndexLeft].scale *= 1.5f;
                    strokesArray[brushIndexRight].scale *= 1.5f;
                    //strokesArray[brushIndexLeft].bindTangent = (strokesArray[brushIndexLeft].bindTangent + UnityEngine.Random.insideUnitSphere * 0.2f).normalized;
                    //strokesArray[brushIndexRight].bindTangent = (strokesArray[brushIndexRight].bindTangent + UnityEngine.Random.insideUnitSphere * 0.2f).normalized;
                    
                }            
            }
        }
    }
    private void GenerateCritterTailFinBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {
        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross + numStrokesPerCritterEyes + numStrokesPerCritterMouth + numStrokesPerCritterTeeth + numStrokesPerCritterPectoralFins + numStrokesPerCritterDorsalFin;

        CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;

        // Genome parameters:
        float spreadAngle = gene.tailFinSpreadAngle;
        float baseLength = gene.tailFinBaseLength * 3f;
        Vector3 frequencies = gene.tailFinFrequencies;
        Vector3 amplitudes = gene.tailFinAmplitudes;
        Vector3 offsets = gene.tailFinOffsets;

        int numRows = 8;
        int numColumns = numStrokesPerCritterTailFin / numRows / 2;

        float lengthMult = baseLength * gene.creatureBaseLength / (float)numRows;

        float tMult = segmentsSummedCritterLength * gene.creatureBaseLength;

        float angleInc = spreadAngle / (float)(numColumns - 1);
        
        for(int y = 0; y < numColumns; y++) {
            // radial fan lines:
            float angleRad = (-spreadAngle / 2f + angleInc * y) * Mathf.PI;

            Vector2 tangent = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            for(int x = 0; x < numRows; x++) {
                //arcs:

                // Find Tail Anchor Point:               
                int brushIndexLeft = arrayIndexStart + numRows * y + x;        
                int brushIndexRight = arrayIndexStart + numRows * y + x + (numStrokesPerCritterTailFin / 2); 

                AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(0f, 0.05f, -1f), new Vector2(0.5f, 0.05f), brushIndexLeft);
                AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(0f, 0.05f, -1f), new Vector2(0.5f, 0.05f), brushIndexRight);
                // Build off that:
                float frac = Mathf.Clamp01((float)y / (float)(numColumns - 1));
                float functionMult = (Mathf.Sin(Mathf.PI * frac * frequencies.x + offsets.x) * amplitudes.x * 0.5f + 1.5f +
                                    Mathf.Cos(Mathf.PI * frac * frequencies.y + offsets.y) * amplitudes.y * 0.5f +
                                    Mathf.Sin(Mathf.PI * frac * frequencies.z + offsets.z) * amplitudes.z * 0.5f) / 3f;

                strokesArray[brushIndexLeft].scale = new Vector2(0.05f * (float)x, lengthMult * functionMult);
                strokesArray[brushIndexRight].scale = strokesArray[brushIndexLeft].scale;

                strokesArray[brushIndexLeft].bindNormal = (new Vector3(-1f, 0f, 0f) + UnityEngine.Random.insideUnitSphere * 0.3f).normalized;               
                strokesArray[brushIndexLeft].bindTangent = new Vector3(0f, -tangent.x, tangent.y);
                strokesArray[brushIndexLeft].bindPos += strokesArray[brushIndexLeft].bindTangent * strokesArray[brushIndexLeft].scale.y * (float)x;
                strokesArray[brushIndexLeft].jawMask = 1f;
                strokesArray[brushIndexLeft].scale.y *= 1.75f;
                strokesArray[brushIndexLeft].t = 0.05f - tangent.x * strokesArray[brushIndexLeft].scale.y * (float)x / tMult;
                strokesArray[brushIndexLeft].neighborIndex = arrayIndexStart + numRows * y + Mathf.Max(0, (x - 1));
                strokesArray[brushIndexLeft].neighborAlign = 1f;
                strokesArray[brushIndexLeft].passiveFollow = Mathf.Lerp(0.3f, 0.7f, (float)x / (float)(numRows - 1));
                                
                strokesArray[brushIndexRight].bindNormal = (new Vector3(1f, 0f, 0f) + UnityEngine.Random.insideUnitSphere * 0.3f).normalized;               
                strokesArray[brushIndexRight].bindTangent = new Vector3(0f, -tangent.x, tangent.y);
                strokesArray[brushIndexRight].bindPos += strokesArray[brushIndexRight].bindTangent * strokesArray[brushIndexRight].scale.y * (float)x;
                strokesArray[brushIndexRight].jawMask = 1f;
                strokesArray[brushIndexRight].scale.y *= 1.75f;
                strokesArray[brushIndexRight].t = 0.05f - tangent.x * strokesArray[brushIndexLeft].scale.y * (float)x / tMult;
                strokesArray[brushIndexRight].neighborIndex = arrayIndexStart + numRows * y + Mathf.Max(0, (x - 1)) + (numStrokesPerCritterTailFin / 2); 
                strokesArray[brushIndexRight].neighborAlign = 1f;
                strokesArray[brushIndexRight].passiveFollow = Mathf.Lerp(0.3f, 0.7f, (float)x / (float)(numRows - 1));
            }
        }
    }
    private void GenerateCritterSkinDetailBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {
        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross + numStrokesPerCritterEyes + numStrokesPerCritterMouth + numStrokesPerCritterTeeth + numStrokesPerCritterPectoralFins + numStrokesPerCritterDorsalFin + numStrokesPerCritterTailFin;

        CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;

        for(int i = 0; i < numStrokesPerCritterSkinDetail; i++) {
            Vector2 randUV = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

            int brushIndex = arrayIndexStart + i;         
            float angleRad = randUV.x * Mathf.PI * 2f; // verticalLerpPos * Mathf.PI;   
            float crossSectionCoordX = Mathf.Sin(angleRad);
            float crossSectionCoordZ = Mathf.Cos(angleRad);
            Vector2 crossSectionNormalizedCoords = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * 1f;

            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(crossSectionCoordX, randUV.y, crossSectionCoordZ), randUV, brushIndex);
            
            int coordU = Mathf.RoundToInt(randUV.x * (float)numStrokesPerCritterCross);
            int coordV = Mathf.RoundToInt(randUV.y * (float)numStrokesPerCritterLength);
            int anchorIndex = coordV * numStrokesPerCritterCross + coordU;

            Vector3 anchorNormal = strokesArray[anchorIndex].bindNormal;
            Vector3 anchorTangent = strokesArray[anchorIndex].bindTangent;

            strokesArray[brushIndex].scale = strokesArray[anchorIndex].scale * 0.4f;
            strokesArray[brushIndex].bindNormal = (anchorNormal + UnityEngine.Random.insideUnitSphere * 0.4f).normalized;               
            strokesArray[brushIndex].bindTangent = (anchorTangent + UnityEngine.Random.insideUnitSphere * 0.3f).normalized;      
            strokesArray[brushIndex].bindPos += anchorNormal * strokesArray[brushIndex].scale.x * 0.33f;
            strokesArray[brushIndex].jawMask = 1f;
            if(strokesArray[brushIndex].bindPos.z > 0f) {
                strokesArray[brushIndex].jawMask = -1f;
            }
            Vector3 hue = Vector3.Lerp(agentGenome.bodyGenome.appearanceGenome.huePrimary, agentGenome.bodyGenome.appearanceGenome.hueSecondary, UnityEngine.Random.Range(0f, 1f));
            strokesArray[brushIndex].color = new Vector4(hue.x, hue.y, hue.z, 1f);
        }
    }

    private void SortCritterBrushstrokes(ref CritterUberStrokeData[] strokesArray, int agentIndex) {
        List<CritterUberStrokeData> sortedBrushStrokesList = new List<CritterUberStrokeData>(); // temporary naive approach:
        // Add first brushstroke first:
        sortedBrushStrokesList.Add(strokesArray[0]);

        int[] sortedIndexMap = new int[strokesArray.Length];  // temporarilly use parent agent index to store own brush index, to avoid adding extra int *********************************************
        for(int p = 0; p < strokesArray.Length; p++) {
            strokesArray[p].parentIndex = p;
            //sortedIndexMap[p] = p;
        }
        for(int b = 1; b < strokesArray.Length; b++) {
            
            // For each brushstroke of this creature:w
            float brushDepth = strokesArray[b].bindPos.z;
            int listSize = sortedBrushStrokesList.Count;

            int numSamples = 6;
            float sampleCoord = 0.5f;
            int sampleIndex = Mathf.FloorToInt((float)listSize * sampleCoord);  // Floor?
            
            for(int s = 0; s < numSamples; s++) {
                // progressively bisect temporary sorted brushPointList to check if it is bigger/smaller
                // 
                if(listSize < Mathf.Pow(2f, s)) { // Add early break if list size is still tiny:
                    break;
                }
                else {   
                    if(sampleIndex >= sortedBrushStrokesList.Count) {  // *** GROSS HACK!!! **** 
                        sampleIndex = sortedBrushStrokesList.Count - 1;
                        //Debug.LogError("error: sampleIndex= " + sampleIndex.ToString() + ", listCount: " + sortedBrushStrokesList.Count.ToString());
                        //sortedBrushStrokesList.Add(singleCritterGenericStrokesArray[b]);
                    }
                    float sampleDepth = sortedBrushStrokesList[sampleIndex].bindPos.z;
                    // Which half of current range to sample next
                    if(brushDepth < sampleDepth) { 
                        sampleCoord += 1f / Mathf.Pow(2f, s + 2f);
                    }
                    else { 
                        sampleCoord -= 1f / Mathf.Pow(2f, s + 2f);
                    }
                    sampleIndex = Mathf.RoundToInt((float)listSize * sampleCoord);  // *** Might have to use FLoorToInt !!! ****
                }                
            }
                        
            sortedBrushStrokesList.Insert(sampleIndex, strokesArray[b]);
            
            
        }
        // Copy sorted list into actual buffer:
        if(sortedBrushStrokesList.Count == strokesArray.Length) {
            for(int i = 0; i < strokesArray.Length; i++) {

                int sortedIndex = sortedBrushStrokesList[i].parentIndex; // stored own brushpoint index in critterParentIndex slot
                sortedIndexMap[sortedIndex] = i; // store original index

                strokesArray[i] = sortedBrushStrokesList[i];  // copy values
                
            }
            for(int p = 0; p < strokesArray.Length; p++) {
                //int originalBrushIndex = sortedIndexMap[p];
                int oldNeighborIndex = strokesArray[p].neighborIndex;
                int newNeighborIndex = sortedIndexMap[oldNeighborIndex];
                strokesArray[p].neighborIndex = newNeighborIndex;
                //strokesArray[p].neighborIndex = strokesArray[sortedIndexMap[p]].neighborIndex;
                strokesArray[p].parentIndex = agentIndex; // remember to set back to correct meaning/value
            }
        }        
        else {
            Debug.Log("Arrays don't match length!!! sorted: " + sortedBrushStrokesList.Count.ToString() + ", master: " + strokesArray.Length.ToString());
        }
    }

    /*private void SimAgentSmearStrokes() {
        int kernelCSSinglePassCurveBrushData = computeShaderBrushStrokes.FindKernel("CSSinglePassCurveBrushData");
        
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentCurveStrokesWriteCBuffer", agentSmearStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSSinglePassCurveBrushData, agentSmearStrokesCBuffer.count, 1, 1);        
    }  */  
    /*public void SimPlayerGlow() {
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
    }*/
    private void SimHighlightTrails() {
        int kernelCSSimulateHighlightTrail = computeShaderCritters.FindKernel("CSSimulateHighlightTrail");

        //computeShaderCritters.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        //computeShaderCritters.SetFloat("_DeltaTime", fluidManager.deltaTime);
        //computeShaderCritters.SetFloat("_InvGridScale", fluidManager.invGridScale);
        //computeShaderCritters.SetVector("_PlayerPos", new Vector4(simManager.agentsArray[0].bodyRigidbody.transform.position.x / SimulationManager._MapSize, simManager.agentsArray[0].bodyRigidbody.transform.position.y / SimulationManager._MapSize, 0f, 0f));
        computeShaderCritters.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderCritters.SetVector("_CursorCoords", new Vector4(simManager.uiManager.curMousePositionOnWaterPlane.x, simManager.uiManager.curMousePositionOnWaterPlane.y, 0f, 0f));
        computeShaderCritters.SetTexture(kernelCSSimulateHighlightTrail, "velocityRead", fluidManager._VelocityA); 
        computeShaderCritters.SetBuffer(kernelCSSimulateHighlightTrail, "highlightTrailDataCBuffer", critterHighlightTrailCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateHighlightTrail, "critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer); 
        computeShaderCritters.SetBuffer(kernelCSSimulateHighlightTrail, "critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);        
        computeShaderCritters.Dispatch(kernelCSSimulateHighlightTrail, critterHighlightTrailCBuffer.count / 1024, 1, 1);
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
    /*private void SimRipples() {
        int kernelSimRipples = fluidManager.computeShaderFluidSim.FindKernel("SimRipples");
        
        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);

        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimRipples, "AgentSimDataCBuffer", simManager.simStateData.agentSimDataCBuffer);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimRipples, "RipplesCBuffer", ripplesCBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimRipples, "VelocityRead", fluidManager._VelocityA);
        fluidManager.computeShaderFluidSim.Dispatch(kernelSimRipples, ripplesCBuffer.count / 8, 1, 1);
    }*/
    private void SimEggSacks() {
        int kernelCSSimulateEggs = computeShaderEggSacks.FindKernel("CSSimulateEggs");
        
        computeShaderEggSacks.SetTexture(kernelCSSimulateEggs, "velocityRead", fluidManager._VelocityA);
        computeShaderEggSacks.SetBuffer(kernelCSSimulateEggs, "eggSackSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
        computeShaderEggSacks.SetBuffer(kernelCSSimulateEggs, "eggDataWriteCBuffer", simManager.simStateData.eggDataCBuffer);
        computeShaderEggSacks.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderEggSacks.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderEggSacks.Dispatch(kernelCSSimulateEggs, simManager.simStateData.eggDataCBuffer.count / 64, 1, 1);        
    }
    /*private void IterateTrailStrokesData() {
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
    }*/
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
    private void SimCritterGenericStrokes() {
        int kernelCSSimulateCritterGenericStrokes = computeShaderCritters.FindKernel("CSSimulateCritterGenericStrokes");        
        computeShaderCritters.SetTexture(kernelCSSimulateCritterGenericStrokes, "velocityRead", fluidManager._VelocityA);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterGenericStrokes, "critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterGenericStrokes, "critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterGenericStrokes, "critterGenericStrokesWriteCBuffer", critterGenericStrokesCBuffer);
        computeShaderCritters.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderCritters.Dispatch(kernelCSSimulateCritterGenericStrokes, critterGenericStrokesCBuffer.count / 16, 1, 1);
                
    }
    private void SimUIToolbarCritterPortraitStrokes() {
        int kernelCSSimulateCritterPortraitStrokes = computeShaderCritters.FindKernel("CSSimulateCritterPortraitStrokes");        
        computeShaderCritters.SetTexture(kernelCSSimulateCritterPortraitStrokes, "velocityRead", fluidManager._VelocityA);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterPortraitStrokes, "critterInitDataCBuffer", toolbarPortraitCritterInitDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterPortraitStrokes, "critterSimDataCBuffer", toolbarPortraitCritterSimDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterPortraitStrokes, "critterGenericStrokesWriteCBuffer", toolbarCritterPortraitStrokesCBuffer);
        computeShaderCritters.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderCritters.Dispatch(kernelCSSimulateCritterPortraitStrokes, toolbarCritterPortraitStrokesCBuffer.count / 16, 1, 1);
    }

    /*public void UpdateTreeOfLifeEventLineData(List<SimEventData> eventDataList) {

        // Move some of this to ComputeShader kernel?

        treeOfLifeEventLineDataArray = new TreeOfLifeEventLineData[64]; // hardcoded!! 64 max!!!
        if(treeOfLifeEventLineDataCBuffer != null) {
            treeOfLifeEventLineDataCBuffer.Release();
        }
        treeOfLifeEventLineDataCBuffer = new ComputeBuffer(treeOfLifeEventLineDataArray.Length, sizeof(float) * 3);
        for(int i = 0; i < testTreeOfLifePositionArray.Length; i++) {
            TreeOfLifeEventLineData data = new TreeOfLifeEventLineData();

            if(i < eventDataList.Count) {
                data.timeStepActivated = eventDataList[i].timeStepActivated;
                //data.xCoord = (float)eventDataList[i].timeStepActivated / (float)simManager.simAgeTimeSteps;
                data.eventCategory = (float)eventDataList[i].category * 0.5f; // 0-->1  
                //data.eventMagnitude = (float)(eventDataList[i].category + 1) * 0.111f;   
                data.isActive = 1f;
            }
            
            treeOfLifeEventLineDataArray[i] = data;
        }
        treeOfLifeEventLineDataCBuffer.SetData(treeOfLifeEventLineDataArray);
    }
    */
    /*public void SimTreeOfLifeWorldStatsData(Texture2D dataTex, Texture2D keyTex) {
        int kernelCSUpdateWorldStatsValues = computeShaderTreeOfLife.FindKernel("CSUpdateWorldStatsValues");
        computeShaderTreeOfLife.SetTexture(kernelCSUpdateWorldStatsValues, "treeOfLifeWorldStatsTex", dataTex); // simManager.uiManager.statsTextureLifespan);
        computeShaderTreeOfLife.SetTexture(kernelCSUpdateWorldStatsValues, "treeOfLifeWorldStatsKeyTex", keyTex);  // used for line color, max/min values reference, and other extra info per graph line (32 max)
        computeShaderTreeOfLife.SetBuffer(kernelCSUpdateWorldStatsValues, "treeOfLifeWorldStatsValuesCBuffer", treeOfLifeWorldStatsValuesCBuffer);
        computeShaderTreeOfLife.SetFloat("_Time", Time.realtimeSinceStartup); // for animation & shit        
        computeShaderTreeOfLife.SetFloat("_GraphCoordStatsStart", simManager.uiManager.tolGraphCoordsStatsStart);
        computeShaderTreeOfLife.SetFloat("_GraphCoordStatsRange", simManager.uiManager.tolGraphCoordsStatsRange);
        computeShaderTreeOfLife.SetInt("_CurSimStep", simManager.simAgeTimeSteps);
        computeShaderTreeOfLife.SetInt("_CurSimYear", simManager.curSimYear);
        computeShaderTreeOfLife.SetInt("_SelectedWorldStatsID", simManager.uiManager.tolSelectedWorldStatsIndex); // UI control
        computeShaderTreeOfLife.SetInt("_NumTimeSeriesEntries", dataTex.width);
        computeShaderTreeOfLife.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        computeShaderTreeOfLife.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        computeShaderTreeOfLife.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        computeShaderTreeOfLife.Dispatch(kernelCSUpdateWorldStatsValues, 1, 1, 1);  // need 32 * 64 segments? -- not yet - as long as one-at-a-time

    }*/
    /*
    public void SimTreeOfLifeSpeciesTreeData(Texture2D dataTex, float maxVal) {
        int kernelCSUpdateSpeciesTreeData = computeShaderTreeOfLife.FindKernel("CSUpdateSpeciesTreeData");
        computeShaderTreeOfLife.SetTexture(kernelCSUpdateSpeciesTreeData, "treeOfLifeSpeciesTreeTex", dataTex); // simManager.uiManager.statsTextureLifespan);
        computeShaderTreeOfLife.SetBuffer(kernelCSUpdateSpeciesTreeData, "treeOfLifeSpeciesDataKeyCBuffer", treeOfLifeSpeciesDataKeyCBuffer);  // 32 indices
        computeShaderTreeOfLife.SetBuffer(kernelCSUpdateSpeciesTreeData, "treeOfLifeSpeciesDataHeadPosCBuffer", treeOfLifeSpeciesDataHeadPosCBuffer);
        computeShaderTreeOfLife.SetBuffer(kernelCSUpdateSpeciesTreeData, "treeOfLifeSpeciesSegmentsCBuffer", treeOfLifeSpeciesSegmentsCBuffer);
        computeShaderTreeOfLife.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderTreeOfLife.SetFloat("_SpeciesStatsMin", 0f);
        computeShaderTreeOfLife.SetFloat("_SpeciesStatsMax", maxVal);
        computeShaderTreeOfLife.SetFloat("_GraphCoordSpeciesStart", simManager.uiManager.tolGraphCoordsSpeciesStart); // try it this way first
        computeShaderTreeOfLife.SetFloat("_GraphCoordSpeciesRange", simManager.uiManager.tolGraphCoordsSpeciesRange);
        computeShaderTreeOfLife.SetInt("_CurSimStep", simManager.simAgeTimeSteps);
        computeShaderTreeOfLife.SetInt("_CurSimYear", simManager.curSimYear);
        computeShaderTreeOfLife.SetInt("_NumTimeSeriesEntries", dataTex.width);
        computeShaderTreeOfLife.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        computeShaderTreeOfLife.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        computeShaderTreeOfLife.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        computeShaderTreeOfLife.Dispatch(kernelCSUpdateSpeciesTreeData, 32, 1, 1);  // 32 = num of species displayed * 64 inside shader

        treeOfLifeSpeciesDataHeadPosCBuffer.GetData(treeOfLifeSpeciesDataHeadPosArray);
    }*/


    
    public void InitializeNewCritterPortraitGenome(AgentGenome genome) {
        SetToolbarPortraitCritterInitData(genome);
        // ^^ genome data for critter

        GenerateCritterPortraitStrokesData(genome);
        // ^^ skin stroke data
    }
    private void SetToolbarPortraitCritterInitData(AgentGenome genome) {

        // NOT the best place for this:::: ***        
        treeOfLifeBackdropPortraitBorderMat.SetColor("_TintPri", new Color(genome.bodyGenome.appearanceGenome.huePrimary.x, genome.bodyGenome.appearanceGenome.huePrimary.y, genome.bodyGenome.appearanceGenome.huePrimary.z));
        treeOfLifeBackdropPortraitBorderMat.SetColor("_TintSec", new Color(genome.bodyGenome.appearanceGenome.hueSecondary.x, genome.bodyGenome.appearanceGenome.hueSecondary.y, genome.bodyGenome.appearanceGenome.hueSecondary.z));
        

        SimulationStateData.CritterInitData[] toolbarPortraitCritterInitDataArray = new SimulationStateData.CritterInitData[1];
        SimulationStateData.CritterInitData initData  = new SimulationStateData.CritterInitData();

        // set values
        initData.boundingBoxSize = genome.bodyGenome.fullsizeBoundingBox; // new Vector3(genome.bodyGenome.coreGenome.fullBodyWidth, genome.bodyGenome.coreGenome.fullBodyLength, genome.bodyGenome.coreGenome.fullBodyWidth);
        initData.spawnSizePercentage = 0.1f;
        initData.maxEnergy = Mathf.Min(initData.boundingBoxSize.x * initData.boundingBoxSize.y, 0.5f);
        initData.maxStomachCapacity = 1f;
        initData.primaryHue = genome.bodyGenome.appearanceGenome.huePrimary;
        initData.secondaryHue = genome.bodyGenome.appearanceGenome.hueSecondary;
        initData.biteConsumeRadius = 1f; 
        initData.biteTriggerRadius = 1f;
        initData.biteTriggerLength = 1f;
        initData.eatEfficiencyPlant = 1f;
        initData.eatEfficiencyDecay = 1f;
        initData.eatEfficiencyMeat = 1f;
        
        float critterFullsizeLength = genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.mouthLength;
        float flexibilityScore = 1f; // Mathf.Min((1f / genome.bodyGenome.coreGenome.creatureAspectRatio - 1f) * 0.6f, 6f);
        //float mouthLengthNormalized = genome.bodyGenome.coreGenome.mouthLength / critterFullsizeLength;
        float approxRadius = genome.bodyGenome.coreGenome.creatureBaseLength * genome.bodyGenome.coreGenome.creatureAspectRatio;
        float approxSize = 1f; // approxRadius * genome.bodyGenome.coreGenome.creatureBaseLength;

        //tempSwimMag = 1f;
        //tempSwimFreq = 1f;
        //tempSwimSpeed = 1f;
        //tempAccelMult = 1f;

        float swimLerp = Mathf.Clamp01((genome.bodyGenome.coreGenome.creatureAspectRatio - 0.175f) / 0.35f);  // 0 = longest, 1 = shortest
                
                // Mag range: 2 --> 0.5
                //freq range: 1 --> 2
        initData.swimMagnitude = Mathf.Lerp(0.225f, 1.1f, swimLerp); // 1f * (1f - flexibilityScore * 0.2f);
        initData.swimFrequency = Mathf.Lerp(2f, 0.8f, swimLerp);   //flexibilityScore * 1.05f;
        initData.swimAnimSpeed = 12f;    // 12f * (1f - approxSize * 0.25f);

        //initData.swimMagnitude = tempSwimMag; // 0.75f * (1f - flexibilityScore * 0.2f);
        //initData.swimFrequency = tempSwimFreq; // flexibilityScore * 2f;
        //initData.swimAnimSpeed = tempSwimSpeed; // 12f * (1f - approxSize * 0.25f);
        initData.bodyCoord = genome.bodyGenome.coreGenome.tailLength / critterFullsizeLength;
	    initData.headCoord = (genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength) / critterFullsizeLength;
        initData.mouthCoord = (genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.headLength) / critterFullsizeLength;
        initData.bendiness = 1f; // tempAccelMult;
        initData.speciesID = 0; // selectedSpeciesID;

        //initData.mouthIsActive = 0.25f;
        //if(simManager.agentsArray[i].mouthRef.isPassive) {
        //    initData.mouthIsActive = 0f;
        //}
        initData.bodyPatternX = genome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeX;
        initData.bodyPatternY = genome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;  // what grid cell of texture sheet to use
        
        toolbarPortraitCritterInitDataArray[0] = initData;
        if(toolbarPortraitCritterInitDataCBuffer != null) {
            toolbarPortraitCritterInitDataCBuffer.Release();
        }
        toolbarPortraitCritterInitDataCBuffer = new ComputeBuffer(1, sizeof(float) * 25 + sizeof(int) * 3);
        toolbarPortraitCritterInitDataCBuffer.SetData(toolbarPortraitCritterInitDataArray);
        
    }
    private void SetToolbarPortraitCritterSimData() {
        
        SimulationStateData.CritterSimData[] toolbarPortraitCritterSimDataArray = new SimulationStateData.CritterSimData[1];
        SimulationStateData.CritterSimData simData = new SimulationStateData.CritterSimData();
        // SIMDATA ::===========================================================================================================================================================================
        Vector3 agentPos = new Vector3(0f, 0f, 0f); // simManager.agentsArray[i].bodyRigidbody.position;
        simData.worldPos = new Vector3(agentPos.x, agentPos.y, 0f);
        float angle = Mathf.Cos(Time.realtimeSinceStartup * 0.67f) * 2f;
        float angle2 = angle; // + (float)selectedSpeciesID; // + Time.realtimeSinceStartup * 0.1f; // + (Mathf.PI * 0.5f);
        Vector2 facingDir = new Vector2(Mathf.Cos(angle2 + (Mathf.PI * 0.75f)), Mathf.Sin(angle2 + (Mathf.PI * 0.75f)));
        //new Vector2(0f, 1f); // facingDir.normalized;
        simData.heading = facingDir.normalized; //new Vector2(0f, 1f); //     facingDir.normalized;        //new Vector2(0f, 1f); //     
        float embryo = 1f;        
        simData.embryoPercentage = embryo;
        simData.growthPercentage = 1f;
        float decay = 0f;        
        simData.decayPercentage = decay;
        simData.foodAmount = 0f; // Mathf.Lerp(simData.foodAmount, simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity, 0.16f);
        simData.energy = 1; // simManager.agentsArray[i].coreModule.energyRaw / simManager.agentsArray[i].coreModule.maxEnergyStorage;
        simData.health = 1; // simManager.agentsArray[i].coreModule.healthHead;
        simData.stamina = 1; // simManager.agentsArray[i].coreModule.stamina[0];
        simData.consumeOn = Mathf.Sin(angle2 * 3.19f) * 0.5f + 0.5f;
        simData.biteAnimCycle = 0f; // (Time.realtimeSinceStartup * 1f) % 1f;
        simData.moveAnimCycle = Time.realtimeSinceStartup * 0.6f % 1f;
        simData.turnAmount = Mathf.Sin(Time.realtimeSinceStartup * 1.654321f) * 0.65f + 0.25f;
        simData.accel = (Mathf.Sin(Time.realtimeSinceStartup * 0.79f) * 0.5f + 0.5f) * 0.081f; // Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f; // ** RE-FACTOR!!!!
		simData.smoothedThrottle = (Mathf.Sin(Time.realtimeSinceStartup * 3.97f + 0.4f) * 0.5f + 0.5f) * 0.85f;
        simData.velocity = facingDir.normalized * (simData.accel + simData.smoothedThrottle); 
        toolbarPortraitCritterSimDataArray[0] = simData;     
        if(toolbarPortraitCritterSimDataCBuffer != null) {
            toolbarPortraitCritterSimDataCBuffer.Release();
        }
        toolbarPortraitCritterSimDataCBuffer = new ComputeBuffer(1, SimulationStateData.GetCritterSimDataSize());
        toolbarPortraitCritterSimDataCBuffer.SetData(toolbarPortraitCritterSimDataArray);
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }

    /*private void SimTreeOfLife() {
                        
        SimulationStateData.CritterInitData[] treeOfLifePortraitCritterInitDataArray = new SimulationStateData.CritterInitData[1];
        SimulationStateData.CritterInitData initData  = new SimulationStateData.CritterInitData();

        int selectedSpeciesID = simManager.uiManager.selectedSpeciesID;
        if(selectedSpeciesID < 0) {
            selectedSpeciesID = 0;  // Temporary catch
        }
        SpeciesGenomePool speciesPool = simManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        AgentGenome genome = speciesPool.representativeGenome;

        // NOT the best place for this:::: ***
        //treeOfLifePortraitBorderMat.SetColor("_TintPri", new Color(genome.bodyGenome.appearanceGenome.huePrimary.x, genome.bodyGenome.appearanceGenome.huePrimary.y, genome.bodyGenome.appearanceGenome.huePrimary.z));
        //treeOfLifePortraitBorderMat.SetColor("_TintSec", new Color(genome.bodyGenome.appearanceGenome.hueSecondary.x, genome.bodyGenome.appearanceGenome.hueSecondary.y, genome.bodyGenome.appearanceGenome.hueSecondary.z));
        treeOfLifeBackdropPortraitBorderMat.SetColor("_TintPri", new Color(genome.bodyGenome.appearanceGenome.huePrimary.x, genome.bodyGenome.appearanceGenome.huePrimary.y, genome.bodyGenome.appearanceGenome.huePrimary.z));
        treeOfLifeBackdropPortraitBorderMat.SetColor("_TintSec", new Color(genome.bodyGenome.appearanceGenome.hueSecondary.x, genome.bodyGenome.appearanceGenome.hueSecondary.y, genome.bodyGenome.appearanceGenome.hueSecondary.z));

        // set values
        initData.boundingBoxSize = genome.bodyGenome.fullsizeBoundingBox; // new Vector3(genome.bodyGenome.coreGenome.fullBodyWidth, genome.bodyGenome.coreGenome.fullBodyLength, genome.bodyGenome.coreGenome.fullBodyWidth);
        initData.spawnSizePercentage = 0.1f;
        initData.maxEnergy = Mathf.Min(initData.boundingBoxSize.x * initData.boundingBoxSize.y, 0.5f);
        initData.maxStomachCapacity = 1f;
        initData.primaryHue = genome.bodyGenome.appearanceGenome.huePrimary;
        initData.secondaryHue = genome.bodyGenome.appearanceGenome.hueSecondary;
        initData.biteConsumeRadius = 1f; 
        initData.biteTriggerRadius = 1f;
        initData.biteTriggerLength = 1f;
        initData.eatEfficiencyPlant = 1f;
        initData.eatEfficiencyDecay = 1f;
        initData.eatEfficiencyMeat = 1f;
        
        float critterFullsizeLength = genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.mouthLength;
        float flexibilityScore = Mathf.Min((1f / genome.bodyGenome.coreGenome.creatureAspectRatio - 1f) * 0.6f, 6f);
        //float mouthLengthNormalized = genome.bodyGenome.coreGenome.mouthLength / critterFullsizeLength;
        float approxRadius = genome.bodyGenome.coreGenome.creatureBaseLength * genome.bodyGenome.coreGenome.creatureAspectRatio;
        float approxSize = approxRadius * genome.bodyGenome.coreGenome.creatureBaseLength;
        initData.swimMagnitude = 0.75f * (1f - flexibilityScore * 0.2f);
        initData.swimFrequency = flexibilityScore * 2f;
	    initData.swimAnimSpeed = 12f * (1f - approxSize * 0.25f);
        initData.bodyCoord = genome.bodyGenome.coreGenome.tailLength / critterFullsizeLength;
	    initData.headCoord = (genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength) / critterFullsizeLength;
        initData.mouthCoord = (genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.headLength) / critterFullsizeLength;
        initData.bendiness = flexibilityScore;
        initData.speciesID = selectedSpeciesID;

        //initData.mouthIsActive = 0.25f;
        //if(simManager.agentsArray[i].mouthRef.isPassive) {
        //    initData.mouthIsActive = 0f;
        //}
        initData.bodyPatternX = genome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeX;
        initData.bodyPatternY = genome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;  // what grid cell of texture sheet to use
        //initData.speciesID = 0;

        treeOfLifePortraitCritterInitDataArray[0] = initData;
        if(treeOfLifePortraitCritterInitDataCBuffer != null) {
            treeOfLifePortraitCritterInitDataCBuffer.Release();
        }
        treeOfLifePortraitCritterInitDataCBuffer = new ComputeBuffer(1, sizeof(float) * 25 + sizeof(int) * 3);
        treeOfLifePortraitCritterInitDataCBuffer.SetData(treeOfLifePortraitCritterInitDataArray);
        
        //upload data to GPU
        SimulationStateData.CritterSimData[] treeOfLifePortraitCritterSimDataArray = new SimulationStateData.CritterSimData[1];
        SimulationStateData.CritterSimData simData = new SimulationStateData.CritterSimData();
        // SIMDATA ::===========================================================================================================================================================================
        Vector3 agentPos = new Vector3(0f, 0f, 0f); // simManager.agentsArray[i].bodyRigidbody.position;
        simData.worldPos = new Vector3(agentPos.x, agentPos.y, 0f);
        float angle = Mathf.Cos(Time.realtimeSinceStartup * 0.67f);
        float angle2 = angle + (float)selectedSpeciesID; // + Time.realtimeSinceStartup * 0.1f; // + (Mathf.PI * 0.5f);
        Vector2 facingDir = new Vector2(Mathf.Cos(angle2 + (Mathf.PI * 0.75f)), Mathf.Sin(angle2 + (Mathf.PI * 0.75f)));
        //new Vector2(0f, 1f); // facingDir.normalized;
        simData.heading = facingDir.normalized; //new Vector2(0f, 1f); //     facingDir.normalized;        //new Vector2(0f, 1f); //     
        float embryo = 1f;        
        simData.embryoPercentage = embryo;
        simData.growthPercentage = 1f;
        float decay = 0f;        
        simData.decayPercentage = decay;
        simData.foodAmount = 0f; // Mathf.Lerp(simData.foodAmount, simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity, 0.16f);
        simData.energy = 1; // simManager.agentsArray[i].coreModule.energyRaw / simManager.agentsArray[i].coreModule.maxEnergyStorage;
        simData.health = 1; // simManager.agentsArray[i].coreModule.healthHead;
        simData.stamina = 1; // simManager.agentsArray[i].coreModule.stamina[0];
        simData.consumeOn = Mathf.Sin(angle2 * 3.19f) * 0.5f + 0.5f;
        simData.biteAnimCycle = 0f; // (Time.realtimeSinceStartup * 1f) % 1f;
        simData.moveAnimCycle = Time.realtimeSinceStartup * 0.6f % 1f;
        simData.turnAmount = Mathf.Sin(Time.realtimeSinceStartup * 1.654321f) * 0.65f + 0.25f;
        simData.accel = (Mathf.Sin(Time.realtimeSinceStartup * 0.79f) * 0.5f + 0.5f) * 0.081f; // Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f; // ** RE-FACTOR!!!!
		simData.smoothedThrottle = (Mathf.Sin(Time.realtimeSinceStartup * 3.97f + 0.4f) * 0.5f + 0.5f) * 0.85f;
        simData.velocity = facingDir.normalized * (simData.accel + simData.smoothedThrottle); 
        treeOfLifePortraitCritterSimDataArray[0] = simData;     
        if(treeOfLifePortraitCritterSimDataCBuffer != null) {
            treeOfLifePortraitCritterSimDataCBuffer.Release();
        }
        treeOfLifePortraitCritterSimDataCBuffer = new ComputeBuffer(1, sizeof(float) * 21);
        treeOfLifePortraitCritterSimDataCBuffer.SetData(treeOfLifePortraitCritterSimDataArray);
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Portrait Eyes:
        if(treeOfLifePortraitEyeDataCBuffer != null) {
            treeOfLifePortraitEyeDataCBuffer.Release();
        }
        treeOfLifePortraitEyeDataCBuffer = new ComputeBuffer(2, sizeof(float) * 13 + sizeof(int) * 2);
        AgentEyeStrokeData[] singleAgentEyeStrokeArray = new AgentEyeStrokeData[treeOfLifePortraitEyeDataCBuffer.count];        
        
        AgentEyeStrokeData dataLeftEye = new AgentEyeStrokeData();
        dataLeftEye.parentIndex = 0;
        dataLeftEye.localPos = genome.bodyGenome.appearanceGenome.eyeGenome.localPos;
        dataLeftEye.localPos.x *= -1f; // LEFT SIDE!
        float width = 1f; // genome.bodyGenome.appearanceGenome.eyeGenome.localScale.x; // simManager.agentsArray[agentIndex].agentWidthsArray[Mathf.RoundToInt((dataLeftEye.localPos.y * 0.5f + 0.5f) * 15f)];
        dataLeftEye.localPos.x *= width * 0.5f;
        dataLeftEye.localDir = new Vector2(0f, 1f);
        dataLeftEye.localScale = genome.bodyGenome.appearanceGenome.eyeGenome.localScale;
        dataLeftEye.irisHue = genome.bodyGenome.appearanceGenome.eyeGenome.irisHue;
        dataLeftEye.pupilHue = genome.bodyGenome.appearanceGenome.eyeGenome.pupilHue;
        dataLeftEye.strength = 1f;
        dataLeftEye.brushType = genome.bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;

        AgentEyeStrokeData dataRightEye = new AgentEyeStrokeData();
        dataRightEye.parentIndex = 0;
        dataRightEye.localPos = genome.bodyGenome.appearanceGenome.eyeGenome.localPos;
        width = 1f; //genome.bodyGenome.appearanceGenome.eyeGenome.localScale.x;
        dataRightEye.localPos.x *= width * 0.5f;
        dataRightEye.localDir = new Vector2(0f, 1f);
        dataRightEye.localScale = genome.bodyGenome.appearanceGenome.eyeGenome.localScale;
        dataRightEye.irisHue = genome.bodyGenome.appearanceGenome.eyeGenome.irisHue;
        dataRightEye.pupilHue = genome.bodyGenome.appearanceGenome.eyeGenome.pupilHue;
        dataRightEye.strength = 1f;
        dataRightEye.brushType = genome.bodyGenome.appearanceGenome.eyeGenome.eyeBrushType;
            
        singleAgentEyeStrokeArray[0] = dataLeftEye;
        singleAgentEyeStrokeArray[1] = dataRightEye;
        
        treeOfLifePortraitEyeDataCBuffer.SetData(singleAgentEyeStrokeArray);
        
    }
    */
    /*private void UpdateAgentHighlightData() {
        //agentHoverHighlightCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        
        float isHighlightOn = 0f;
        if(simManager.cameraManager.isMouseHoverAgent) {
            isHighlightOn = 1f;
        }

        Vector4[] agentHoverHighlightArray = new Vector4[1];
        agentHoverHighlightArray[0] = new Vector4(isHighlightOn, 0f, 0f, 0f);
        agentHoverHighlightCBuffer.SetData(agentHoverHighlightArray);
    }*/
    

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
        SimHighlightTrails();
        //SimFloatyBits();
        //SimRipples();
        SimEggSacks();
        //SimWaterSplines();
        //SimWaterChains();


        //SimTreeOfLife(); // issues with this being on FixedUpdate() cycle vs Update() ?? ***

        //SimTreeOfLifeWorldStatsData(simManager.uiManager.textureWorldStats, simManager.uiManager.textureWorldStatsKey);
        //Debug.Log("size: " + simManager.uiManager.tolSelectedSpeciesStatsIndex.ToString());
        //Texture2D graphTex = simManager.uiManager.statsTreeOfLifeSpeciesTexArray[simManager.uiManager.selectedSpeciesStatsIndex];
        //float maxVal = simManager.uiManager.maxValuesStatArray[simManager.uiManager.selectedSpeciesStatsIndex];

        //SimTreeOfLifeSpeciesTreeData(graphTex, maxVal); // update this?                
        //UpdateAgentHighlightData();

        //SimCritterSkinStrokes();

        // PORTRAIT
        if(isToolbarCritterPortraitEnabled) {
            //SimCritterPortrait();
            SetToolbarPortraitCritterSimData();
            SimUIToolbarCritterPortraitStrokes();
        }
        

        // PRIMARY CRITTERS
        SimCritterGenericStrokes();

        


        baronVonWater.altitudeMapRef = baronVonTerrain.terrainHeightDataRT;
        float camDist = Mathf.Clamp01(-1f * simManager.cameraManager.gameObject.transform.position.z / (400f - 10f));
        baronVonWater.camDistNormalized = camDist;
        Vector2 boxSizeHalf = 0.8f * Vector2.Lerp(new Vector2(16f, 12f) * 2, new Vector2(256f, 204f), Mathf.Clamp01(-(simManager.cameraManager.gameObject.transform.position.z) / 150f));
        
        baronVonWater.spawnBoundsCameraDetails = new Vector4(simManager.cameraManager.curCameraFocusPivotPos.x - boxSizeHalf.x,
                                                            simManager.cameraManager.curCameraFocusPivotPos.y - boxSizeHalf.y,
                                                            simManager.cameraManager.curCameraFocusPivotPos.x + boxSizeHalf.x,
                                                            simManager.cameraManager.curCameraFocusPivotPos.y + boxSizeHalf.y);

        baronVonTerrain.spawnBoundsCameraDetails = baronVonWater.spawnBoundsCameraDetails;


        //baronVonTerrain.Tick(simManager.vegetationManager.rdRT1);
        int kernelSimGroundBits = baronVonTerrain.computeShaderTerrainGeneration.FindKernel("CSSimGroundBitsData");
        baronVonTerrain.computeShaderTerrainGeneration.SetBuffer(kernelSimGroundBits, "groundBitsCBuffer", baronVonTerrain.groundBitsCBuffer);
        baronVonTerrain.computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "AltitudeRead", baronVonTerrain.terrainHeightDataRT);
        baronVonTerrain.computeShaderTerrainGeneration.SetTexture(kernelSimGroundBits, "decomposersRead", simManager.vegetationManager.resourceGridRT1);
        baronVonTerrain.computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.computeShaderTerrainGeneration.SetFloat("_Time", Time.realtimeSinceStartup);        
        baronVonTerrain.computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", baronVonTerrain.spawnBoundsCameraDetails);

        float spawnLerp = simManager.trophicLayersManager.GetDecomposersOnLerp(simManager.simAgeTimeSteps);
        float spawnRadius = Mathf.Lerp(1f, SimulationManager._MapSize, spawnLerp);
        Vector4 spawnPos = new Vector4(simManager.trophicLayersManager.decomposerOriginPos.x, simManager.trophicLayersManager.decomposerOriginPos.y, 0f, 0f);
        baronVonTerrain.computeShaderTerrainGeneration.SetFloat("_SpawnRadius", spawnRadius);
        baronVonTerrain.computeShaderTerrainGeneration.SetVector("_SpawnPos", spawnPos);
        baronVonTerrain.computeShaderTerrainGeneration.SetFloat("_DecomposerDensityLerp", Mathf.Clamp01(simManager.simResourceManager.curGlobalDecomposers / 100f));
        baronVonTerrain.computeShaderTerrainGeneration.Dispatch(kernelSimGroundBits, baronVonTerrain.groundBitsCBuffer.count / 1024, 1, 1);

        int kernelSimCarpetBits = baronVonTerrain.computeShaderTerrainGeneration.FindKernel("CSSimCarpetBitsData");
        baronVonTerrain.computeShaderTerrainGeneration.SetBuffer(kernelSimCarpetBits, "groundBitsCBuffer", baronVonTerrain.carpetBitsCBuffer);
        baronVonTerrain.computeShaderTerrainGeneration.SetTexture(kernelSimCarpetBits, "AltitudeRead", baronVonTerrain.terrainHeightDataRT);
        baronVonTerrain.computeShaderTerrainGeneration.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.computeShaderTerrainGeneration.SetVector("_SpawnBoundsCameraDetails", baronVonTerrain.spawnBoundsCameraDetails);
        baronVonTerrain.computeShaderTerrainGeneration.Dispatch(kernelSimCarpetBits, baronVonTerrain.carpetBitsCBuffer.count / 1024, 1, 1);


        baronVonWater.Tick(null);  // <-- SimWaterCurves/Chains/Water surface
        
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
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
        basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", obstacleStrokesCBuffer);        
        cmdBufferFluidObstacles.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, obstacleStrokesCBuffer.count);
        // Disabling for now -- starting with one-way interaction between fluid & objects (fluid pushes objects, they don't push back)
        

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

        algaeParticleColorInjectMat.SetPass(0);
        algaeParticleColorInjectMat.SetBuffer("foodParticleDataCBuffer", simManager.vegetationManager.plantParticlesCBuffer);
        algaeParticleColorInjectMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        algaeParticleColorInjectMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        algaeParticleColorInjectMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferFluidColor.DrawProcedural(Matrix4x4.identity, algaeParticleColorInjectMat, 0, MeshTopology.Triangles, 6, simManager.vegetationManager.plantParticlesCBuffer.count);

        /*Vector4 cursorPos = new Vector4(simManager.uiManager.curMousePositionOnWaterPlane.x, simManager.uiManager.curMousePositionOnWaterPlane.y, 0f, 0f);
        if(isStirring) {            // Particle-based instead? // hijack and use for stir tool
            playerBrushColorInjectMat.SetPass(0);
            playerBrushColorInjectMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            playerBrushColorInjectMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
            //playerBrushColorInjectMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            playerBrushColorInjectMat.SetVector("_CursorPos", cursorPos);
            playerBrushColorInjectMat.SetFloat("_CursorRadius", Mathf.Lerp(0.5f, 1.5f, baronVonWater.camDistNormalized));
            playerBrushColorInjectMat.SetVector("_BrushColor", new Vector4(0.7f, 0.8f, 1f, Mathf.Clamp(simManager.uiManager.smoothedMouseVel.magnitude * 0.5f, 0f, 10f)));
            cmdBufferFluidColor.DrawProcedural(Matrix4x4.identity, playerBrushColorInjectMat, 0, MeshTopology.Triangles, 6, 1);        
        }*/
        // Creatures + EggSacks:
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", colorInjectionStrokesCBuffer);
        cmdBufferFluidColor.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, colorInjectionStrokesCBuffer.count);
        // Render Agent/Food/Pred colors here!!!
        // just use their display renders?
        Graphics.ExecuteCommandBuffer(cmdBufferFluidColor);
        fluidColorRenderCamera.Render();
        //simManager.environmentFluidManager.densityA.GenerateMips();
        // Update this ^^ to use Graphics.ExecuteCommandBuffer()  ****



        // SPIRIT BRUSH TEST!
        cmdBufferSpiritBrush.Clear(); // needed since camera clear flag is set to none
        cmdBufferSpiritBrush.SetRenderTarget(spiritBrushRT);
        cmdBufferSpiritBrush.ClearRenderTarget(true, true, Color.black, 1.0f);
        
            // clear -- needed???
        cmdBufferSpiritBrush.SetViewProjectionMatrices(spiritBrushRenderCamera.worldToCameraMatrix, spiritBrushRenderCamera.projectionMatrix);
        // Draw Solid Land boundaries:
        float scale = Mathf.Lerp(8f, 12f, baronVonWater.camDistNormalized) * 6.283f;
        if(simManager.trophicLayersManager.isSelectedTrophicSlot) {
            if(simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
                scale *= 0.5f;
            }
            else if(simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {

            }
            else if(simManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                scale *= 0.37f;
            }
            else {
                // terrain
            
            }
        }
        
        Matrix4x4 stirStickTransformMatrix = Matrix4x4.TRS(new Vector3(simManager.uiManager.curMousePositionOnWaterPlane.x, simManager.uiManager.curMousePositionOnWaterPlane.y, 0f), Quaternion.identity, Vector3.one * scale);
        //Debug.Log("mouseCursorPos: " + simManager.uiManager.curMousePositionOnWaterPlane.ToString());
        if(isBrushing) {
            
            //cmdBufferSpiritBrush.DrawMesh(meshStirStickLrg, stirStickTransformMatrix, spiritBrushRenderMat); // Masks out areas above the fluid "Sea Level"
            spiritBrushRenderMat.SetPass(0);
            spiritBrushRenderMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
            spiritBrushRenderMat.SetVector("_Position", new Vector4(simManager.uiManager.curMousePositionOnWaterPlane.x, simManager.uiManager.curMousePositionOnWaterPlane.y, 0f, 0f));
            spiritBrushRenderMat.SetFloat("_Scale", scale);
            float brushIntensity = 1f;
            /*if(simManager.uiManager.selectedToolbarTerrainLayer > 0) {
                brushIntensity *= 0.42f;
            }
            else {
                brushIntensity = (baronVonWater.camDistNormalized * 0.25f + 0.75f) * 0.05f;
            }*/
            spiritBrushRenderMat.SetFloat("_Strength", brushIntensity);
            //spiritBrushRenderMat.SetBuffer("basicStrokesCBuffer", obstacleStrokesCBuffer); 
            cmdBufferSpiritBrush.DrawProcedural(Matrix4x4.identity, spiritBrushRenderMat, 0, MeshTopology.Triangles, 6, 1);
            // Draw dynamic Obstacles:        
            //basicStrokeDisplayMat.SetPass(0);
            //basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
            //basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", obstacleStrokesCBuffer);        
            //cmdBufferSpiritBrush.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, .count);
            // Disabling for now -- starting with one-way interaction between fluid & objects (fluid pushes objects, they don't push back)
            
        }
        else {

        }
        Graphics.ExecuteCommandBuffer(cmdBufferSpiritBrush);
        // Still not sure if this will work correctly... ****
        spiritBrushRenderCamera.Render();

        // Species PORTRAIT:
        cmdBufferSlotPortraitDisplay.Clear();
        cmdBufferSlotPortraitDisplay.SetRenderTarget(slotPortraitRenderCamera.targetTexture); // needed???
        cmdBufferSlotPortraitDisplay.ClearRenderTarget(true, true, new Color(0f,0f,0f,0f), 1.0f);  // clear -- needed???
        cmdBufferSlotPortraitDisplay.SetViewProjectionMatrices(slotPortraitRenderCamera.worldToCameraMatrix, slotPortraitRenderCamera.projectionMatrix);

        toolbarSpeciesPortraitStrokesMat.SetPass(0);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("critterInitDataCBuffer", toolbarPortraitCritterInitDataCBuffer);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("critterSimDataCBuffer", toolbarPortraitCritterSimDataCBuffer);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("critterGenericStrokesCBuffer", toolbarCritterPortraitStrokesCBuffer);    
        toolbarSpeciesPortraitStrokesMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        toolbarSpeciesPortraitStrokesMat.SetFloat("_MapSize", SimulationManager._MapSize);          
        cmdBufferSlotPortraitDisplay.DrawProcedural(Matrix4x4.identity, toolbarSpeciesPortraitStrokesMat, 0, MeshTopology.Triangles, 6, toolbarCritterPortraitStrokesCBuffer.count);
        
        Graphics.ExecuteCommandBuffer(cmdBufferSlotPortraitDisplay);
        slotPortraitRenderCamera.Render();



        //===================   RESOURCE SIMULATION   ==========================================================
        cmdBufferResourceSim.Clear();
        cmdBufferResourceSim.SetRenderTarget(simManager.vegetationManager.resourceSimTransferRT);
        cmdBufferResourceSim.ClearRenderTarget(true, true, Color.black, 1.0f);
        cmdBufferResourceSim.SetViewProjectionMatrices(resourceSimRenderCamera.worldToCameraMatrix, resourceSimRenderCamera.projectionMatrix);
        
        // render StructuredBuffers:
        // ZOOPLANKTON:
        resourceSimTransferMat.SetPass(0);
        resourceSimTransferMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        resourceSimTransferMat.SetBuffer("animalParticleDataCBuffer", simManager.zooplanktonManager.animalParticlesCBuffer); // simManager.vegetationManager.algaeParticlesCBuffer);    
        resourceSimTransferMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferResourceSim.DrawProcedural(Matrix4x4.identity, resourceSimTransferMat, 0, MeshTopology.Triangles, 6, simManager.zooplanktonManager.animalParticlesCBuffer.count); // simManager.vegetationManager.algaeParticlesCBuffer.count);

        // PLANT PARTICLES:
        plantParticleDataMat.SetPass(0);
        plantParticleDataMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        plantParticleDataMat.SetBuffer("plantParticleDataCBuffer", simManager.vegetationManager.plantParticlesCBuffer); // simManager.vegetationManager.algaeParticlesCBuffer);    
        plantParticleDataMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferResourceSim.DrawProcedural(Matrix4x4.identity, plantParticleDataMat, 0, MeshTopology.Triangles, 6, simManager.vegetationManager.plantParticlesCBuffer.count); // simManager.vegetationManager.algaeParticlesCBuffer.count);


        // CRITTERS:
        //critterSimDataCBuffer
        resourceSimAgentDataMat.SetPass(0);
        resourceSimAgentDataMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        resourceSimAgentDataMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer); // simManager.vegetationManager.algaeParticlesCBuffer);    
        resourceSimAgentDataMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferResourceSim.DrawProcedural(Matrix4x4.identity, resourceSimAgentDataMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.critterSimDataCBuffer.count); // simManager.vegetationManager.algaeParticlesCBuffer.count);


        Graphics.ExecuteCommandBuffer(cmdBufferResourceSim);
        resourceSimRenderCamera.Render();
        //======================================================================================================




        // TREE OF LIFE:
        // TREE OF LIFE:

        //graphs mouse coords:
        /*if(simManager.uiManager.tolMouseOver > 0.5f) {
            Vector2 localPoint = Vector2.zero;
            RectTransform rectTransform = simManager.uiManager.imageTolSpeciesTreeRender.gameObject.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);

            Vector2 uvCoord = new Vector2((localPoint.x + 230f) / 460f, (localPoint.y + 160f) / 320f);
            simManager.uiManager.tolMouseCoords = uvCoord;

            simManager.uiManager.UpdateTolGraphCursorTimeSelectUI(uvCoord);

            simManager.uiManager.HoverOverTolGraphRenderPanel();
            
        }
        else {
            
        }*/
        
        // TREE OF LIFE STUFFS:::
        /*
        cmdBufferTreeOfLifeSpeciesTree.Clear();
        cmdBufferTreeOfLifeSpeciesTree.SetRenderTarget(treeOfLifeSpeciesTreeRenderCamera.targetTexture); // needed???
        cmdBufferTreeOfLifeSpeciesTree.ClearRenderTarget(true, true, new Color(0f,0f,0f,0f), 1.0f);  // clear -- needed???
        cmdBufferTreeOfLifeSpeciesTree.SetViewProjectionMatrices(treeOfLifeSpeciesTreeRenderCamera.worldToCameraMatrix, treeOfLifeSpeciesTreeRenderCamera.projectionMatrix);

        float panelIsOn = 0f;
        if(simManager.uiManager.tolEventsTimelineOn) {   // room for code improvement here - only update on UI events rather than every frame conditional
            panelIsOn = 1f;   
        }

        // Cursor line first
        treeOfLifeCursorLineMat.SetPass(0);
        treeOfLifeCursorLineMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeCursorLineMat.SetInt("_CurSimStep", simManager.simAgeTimeSteps);
        treeOfLifeCursorLineMat.SetFloat("_IsOn", panelIsOn);
        treeOfLifeCursorLineMat.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        treeOfLifeCursorLineMat.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        treeOfLifeCursorLineMat.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        treeOfLifeCursorLineMat.SetBuffer("treeOfLifeEventLineDataCBuffer", treeOfLifeEventLineDataCBuffer);
        cmdBufferTreeOfLifeSpeciesTree.DrawProcedural(Matrix4x4.identity, treeOfLifeCursorLineMat, 0, MeshTopology.Triangles, 6, 1);
        
        // draw event lines next:
        
        treeOfLifeEventsLineMat.SetPass(0);
        treeOfLifeEventsLineMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeEventsLineMat.SetInt("_CurSimStep", simManager.simAgeTimeSteps);
        treeOfLifeEventsLineMat.SetFloat("_GraphCoordEventsStart", simManager.uiManager.tolGraphCoordsEventsStart);
        treeOfLifeEventsLineMat.SetFloat("_GraphCoordEventsRange", simManager.uiManager.tolGraphCoordsEventsRange);        
        treeOfLifeEventsLineMat.SetFloat("_IsOn", panelIsOn);
        treeOfLifeEventsLineMat.SetInt("_ClosestEventIndex", simManager.uiManager.curClosestEventToCursor);
        treeOfLifeEventsLineMat.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        treeOfLifeEventsLineMat.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        treeOfLifeEventsLineMat.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        treeOfLifeEventsLineMat.SetBuffer("treeOfLifeEventLineDataCBuffer", treeOfLifeEventLineDataCBuffer);
        cmdBufferTreeOfLifeSpeciesTree.DrawProcedural(Matrix4x4.identity, treeOfLifeEventsLineMat, 0, MeshTopology.Triangles, 6, treeOfLifeEventLineDataCBuffer.count);
        
        // World Stats graph lines:
        treeOfLifeWorldStatsMat.SetPass(0);
        treeOfLifeWorldStatsMat.SetTexture("_KeyTex", simManager.uiManager.textureWorldStatsKey);
        treeOfLifeWorldStatsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeWorldStatsMat.SetFloat("_GraphCoordStatsStart", simManager.uiManager.tolGraphCoordsStatsStart);
        treeOfLifeWorldStatsMat.SetBuffer("treeOfLifeWorldStatsValuesCBuffer", treeOfLifeWorldStatsValuesCBuffer);
        treeOfLifeWorldStatsMat.SetInt("_SelectedWorldStatsID", simManager.uiManager.tolSelectedWorldStatsIndex);
        panelIsOn = 0f;
        if(simManager.uiManager.tolWorldStatsOn) {   // room for code improvement here - only update on UI events rather than every frame conditional
            panelIsOn = 1f;   
        }
        treeOfLifeWorldStatsMat.SetFloat("_IsOn", panelIsOn);
        treeOfLifeWorldStatsMat.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        treeOfLifeWorldStatsMat.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        treeOfLifeWorldStatsMat.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        cmdBufferTreeOfLifeSpeciesTree.DrawProcedural(Matrix4x4.identity, treeOfLifeWorldStatsMat, 0, MeshTopology.Triangles, 6, 64);

        treeOfLifeSpeciesLineMat.SetPass(0);
        //treeOfLifeSpeciesLineMat.SetTexture("_KeyTex", simManager.uiManager.statSpeciesColorKey);
        treeOfLifeSpeciesLineMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeSpeciesLineMat.SetBuffer("treeOfLifeSpeciesDataKeyCBuffer", treeOfLifeSpeciesDataKeyCBuffer);        
        treeOfLifeSpeciesLineMat.SetBuffer("treeOfLifeSpeciesSegmentsCBuffer", treeOfLifeSpeciesSegmentsCBuffer);
        treeOfLifeSpeciesLineMat.SetInt("_CurSimStep", simManager.simAgeTimeSteps);
        treeOfLifeSpeciesLineMat.SetInt("_CurSimYear", simManager.curSimYear);
        panelIsOn = 0f;
        if(simManager.uiManager.tolSpeciesTreeOn) {   // room for code improvement here - only update on UI events rather than every frame conditional
            panelIsOn = 1f;   
        }
        treeOfLifeSpeciesLineMat.SetFloat("_IsOn", panelIsOn);
        treeOfLifeSpeciesLineMat.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        treeOfLifeSpeciesLineMat.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        treeOfLifeSpeciesLineMat.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        cmdBufferTreeOfLifeSpeciesTree.DrawProcedural(Matrix4x4.identity, treeOfLifeSpeciesLineMat, 0, MeshTopology.Triangles, 6, 64 * 32);

        // Head Tips!
        treeOfLifeSpeciesHeadTipMat.SetPass(0);
        treeOfLifeSpeciesHeadTipMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeSpeciesHeadTipMat.SetBuffer("treeOfLifeSpeciesDataKeyCBuffer", treeOfLifeSpeciesDataKeyCBuffer);        
        treeOfLifeSpeciesHeadTipMat.SetBuffer("treeOfLifeSpeciesDataHeadPosCBuffer", treeOfLifeSpeciesDataHeadPosCBuffer);
        treeOfLifeSpeciesHeadTipMat.SetInt("_CurSimStep", simManager.simAgeTimeSteps);
        treeOfLifeSpeciesHeadTipMat.SetInt("_CurSimYear", simManager.curSimYear);
        treeOfLifeSpeciesHeadTipMat.SetInt("_HoverIndex", simManager.uiManager.treeOfLifeManager.hoverID);
        treeOfLifeSpeciesHeadTipMat.SetFloat("_IsOn", panelIsOn);
        treeOfLifeSpeciesHeadTipMat.SetFloat("_MouseCoordX", simManager.uiManager.tolMouseCoords.x);
        treeOfLifeSpeciesHeadTipMat.SetFloat("_MouseCoordY", simManager.uiManager.tolMouseCoords.y);
        treeOfLifeSpeciesHeadTipMat.SetFloat("_MouseOn", simManager.uiManager.tolMouseOver);
        cmdBufferTreeOfLifeSpeciesTree.DrawProcedural(Matrix4x4.identity, treeOfLifeSpeciesHeadTipMat, 0, MeshTopology.Triangles, 6, 32);


        Graphics.ExecuteCommandBuffer(cmdBufferTreeOfLifeSpeciesTree);
        treeOfLifeSpeciesTreeRenderCamera.Render();
        */



        
        // OLD BELOW:::::
        /*UpdateTreeOfLifeData();
        cmdBufferTreeOfLifeDisplay.Clear();
        cmdBufferTreeOfLifeDisplay.SetRenderTarget(simManager.masterGenomePool.treeOfLifeManager.treeOfLifeDisplayRT);
        cmdBufferTreeOfLifeDisplay.ClearRenderTarget(true, true, new Color(0f,0f,0f,0f), 1.0f);  // clear -- needed???
        cmdBufferTreeOfLifeDisplay.SetViewProjectionMatrices(treeOfLifeRenderCamera.worldToCameraMatrix, treeOfLifeRenderCamera.projectionMatrix);
        // UI TREE OF LIFE TESTING:
        treeOfLifeLeafNodesMat.SetPass(0);
        treeOfLifeLeafNodesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeLeafNodesMat.SetBuffer("treeOfLifeLeafNodeDataCBuffer", treeOfLifeLeafNodeDataCBuffer);
        treeOfLifeLeafNodesMat.SetBuffer("treeOfLifeNodeColliderDataCBuffer", treeOfLifeNodeColliderDataCBuffer);
        cmdBufferTreeOfLifeDisplay.DrawProcedural(Matrix4x4.identity, treeOfLifeLeafNodesMat, 0, MeshTopology.Triangles, 6, treeOfLifeLeafNodeDataCBuffer.count);
        Graphics.ExecuteCommandBuffer(cmdBufferTreeOfLifeDisplay);
        treeOfLifeRenderCamera.Render();
        */
    }
    
    public void TreeOfLifeAddNewSpecies(MasterGenomePool masterGenomePool, int newSpeciesID) { //int speciesID, int parentSpeciesID) {

        SpeciesGenomePool newSpecies = masterGenomePool.completeSpeciesPoolsList[newSpeciesID];

        //computeShaderTreeOfLife.SetTexture(kernelCSAddNewSpecies, "velocityRead", fluidManager._VelocityA);
        int[] speciesIDArray = new int[1];
        speciesIDArray[0] = newSpecies.speciesID;
        ComputeBuffer speciesIDCBuffer = new ComputeBuffer(1, sizeof(int));
        speciesIDCBuffer.SetData(speciesIDArray);

        TreeOfLifeLeafNodeData[] updateLeafNodeDataArray = new TreeOfLifeLeafNodeData[1];        
        
        TreeOfLifeLeafNodeData data = new TreeOfLifeLeafNodeData();
        data.speciesID = newSpecies.speciesID;
        data.parentSpeciesID = newSpecies.parentSpeciesID;
        data.graphDepth = newSpecies.depthLevel;
        data.primaryHue = newSpecies.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        data.secondaryHue = newSpecies.representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
        data.growthPercentage = 1f;
        data.age = 0f;
        data.decayPercentage = 0f;       
        data.isActive = 1f;
        data.isExtinct = 0f;
        updateLeafNodeDataArray[0] = data;   

        ComputeBuffer updateLeafNodeDataCBuffer = new ComputeBuffer(updateLeafNodeDataArray.Length, sizeof(int) * 3 + sizeof(float) * 14);
        updateLeafNodeDataCBuffer.SetData(updateLeafNodeDataArray);    

        int kernelCSAddNewSpeciesNode = computeShaderTreeOfLife.FindKernel("CSAddNewSpeciesNode");
        computeShaderTreeOfLife.SetBuffer(kernelCSAddNewSpeciesNode, "treeOfLifeNodeColliderDataCBufferWrite", treeOfLifeNodeColliderDataCBufferA);
        computeShaderTreeOfLife.SetBuffer(kernelCSAddNewSpeciesNode, "treeOfLifeLeafNodeDataCBuffer", treeOfLifeLeafNodeDataCBuffer);
        computeShaderTreeOfLife.SetBuffer(kernelCSAddNewSpeciesNode, "updateSpeciesNodeDataCBuffer", updateLeafNodeDataCBuffer);
        computeShaderTreeOfLife.SetBuffer(kernelCSAddNewSpeciesNode, "speciesIndexCBuffer", speciesIDCBuffer);
        computeShaderTreeOfLife.Dispatch(kernelCSAddNewSpeciesNode, speciesIDCBuffer.count, 1, 1);

        speciesIDCBuffer.Release();
        updateLeafNodeDataCBuffer.Release();

        // STEM SEGMENTS:
        
        if(newSpeciesID > 0) {  // if not root node
            TreeOfLifeStemSegmentStruct[] segmentStructUpdateArray = new TreeOfLifeStemSegmentStruct[newSpecies.depthLevel]; // *** +1?
            ComputeBuffer updateStemSegmentDataCBuffer = new ComputeBuffer(segmentStructUpdateArray.Length, sizeof(int) * 3 + sizeof(float) * 1);

            int curSpeciesID = newSpeciesID;

            for(int i = 0; i < newSpecies.depthLevel; i++) {

                int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[curSpeciesID].parentSpeciesID;

                // Create StemSegment!    
                TreeOfLifeStemSegmentStruct newStemSegment = new TreeOfLifeStemSegmentStruct();
                newStemSegment.speciesID = newSpeciesID;
                newStemSegment.fromID = parentSpeciesID;
                newStemSegment.toID = curSpeciesID;

                segmentStructUpdateArray[i] = newStemSegment;
                
                curSpeciesID = parentSpeciesID;  // set curSpecies to ParentSpecies (traverse up tree)                
            }

            updateStemSegmentDataCBuffer.SetData(segmentStructUpdateArray);

            // DISPATCH::
            int kernelCSAddNewSpeciesStemSegments = computeShaderTreeOfLife.FindKernel("CSAddNewSpeciesStemSegments");
            computeShaderTreeOfLife.SetBuffer(kernelCSAddNewSpeciesStemSegments, "treeOfLifeStemSegmentDataCBuffer", treeOfLifeStemSegmentDataCBuffer);
            computeShaderTreeOfLife.SetBuffer(kernelCSAddNewSpeciesStemSegments, "updateStemSegmentDataCBuffer", updateStemSegmentDataCBuffer);
            computeShaderTreeOfLife.SetInt("_UpdateBufferStartIndex", curNumTreeOfLifeStemSegments);
            computeShaderTreeOfLife.Dispatch(kernelCSAddNewSpeciesStemSegments, updateStemSegmentDataCBuffer.count, 1, 1);

            //Debug.Log("UPDATE STEM SEGMENTS: " + newSpeciesID.ToString() + ", depth: " + newSpecies.depthLevel.ToString());

            updateStemSegmentDataCBuffer.Release();

            curNumTreeOfLifeStemSegments += newSpecies.depthLevel;  // keep track of start index
        }       
        
    }
    public void TreeOfLifeExtinctSpecies(int speciesID) {
        //computeShaderTreeOfLife.SetTexture(kernelCSAddNewSpecies, "velocityRead", fluidManager._VelocityA);
        int[] speciesIDArray = new int[1];
        speciesIDArray[0] = speciesID;
        ComputeBuffer speciesIDCBuffer = new ComputeBuffer(1, sizeof(int));
        speciesIDCBuffer.SetData(speciesIDArray);

        TreeOfLifeLeafNodeData[] updateLeafNodeDataArray = new TreeOfLifeLeafNodeData[1];        
        
        TreeOfLifeLeafNodeData data = new TreeOfLifeLeafNodeData();
        data.speciesID = speciesID;
        data.parentSpeciesID = 0;
        data.graphDepth = 0;
        data.primaryHue = Vector3.zero;
        data.secondaryHue = Vector3.zero;
        data.growthPercentage = 0f;
        data.age = 0f;
        data.decayPercentage = 0f;       
        data.isActive = 1f;
        data.isExtinct = 1f;
        updateLeafNodeDataArray[0] = data;   

        ComputeBuffer updateLeafNodeDataCBuffer = new ComputeBuffer(updateLeafNodeDataArray.Length, sizeof(int) * 3 + sizeof(float) * 14);
        updateLeafNodeDataCBuffer.SetData(updateLeafNodeDataArray);    

        // DISPATCH::
        int kernelCSCSExctinctSpecies = computeShaderTreeOfLife.FindKernel("CSExctinctSpecies");
        computeShaderTreeOfLife.SetBuffer(kernelCSCSExctinctSpecies, "treeOfLifeLeafNodeDataCBuffer", treeOfLifeLeafNodeDataCBuffer);
        computeShaderTreeOfLife.SetBuffer(kernelCSCSExctinctSpecies, "updateSpeciesNodeDataCBuffer", updateLeafNodeDataCBuffer);
        computeShaderTreeOfLife.SetBuffer(kernelCSCSExctinctSpecies, "speciesIndexCBuffer", speciesIDCBuffer);
        computeShaderTreeOfLife.Dispatch(kernelCSCSExctinctSpecies, updateLeafNodeDataCBuffer.count, 1, 1);

        speciesIDCBuffer.Release();
        updateLeafNodeDataCBuffer.Release();
        //Debug.Log("UPDATE STEM SEGMENTS: " + newSpeciesID.ToString() + ", depth: " + newSpecies.depthLevel.ToString());
    }
    public void TreeOfLifeGetColliderNodePositionData() {
        
        //simManager.uiManager.treeOfLifeManager.UpdateNodePositionsFromGPU(simManager.uiManager.cameraManager, treeOfLifeSpeciesDataHeadPosArray);     // **** Need to cap this at 32 or it breaks!!!  
        
    }
    /*private void Render() {
        cmdBufferPrimary.Clear();
        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferPrimary.SetRenderTarget(renderTarget);  // Set render Target
        cmdBufferPrimary.ClearRenderTarget(true, true, Color.yellow, 1.0f);  // clear -- needed???
    }*/
    
    private void Render() {
        //Debug.Log("TestRenderCommandBuffer()");
        

        if(isDebugRender) {
            mainRenderCam.RemoveAllCommandBuffers();
            mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferDebugVis);

            cmdBufferDebugVis.Clear();
            RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);            
            cmdBufferDebugVis.SetRenderTarget(renderTarget);  // Set render Target
            cmdBufferDebugVis.ClearRenderTarget(true, true, Color.blue, 1.0f);  // clear -- needed???

            debugVisModeMat.SetPass(0);
            debugVisModeMat.SetTexture("_MainTex", simManager.vegetationManager.resourceGridRT1);
            debugVisModeMat.SetFloat("_ColorMagnitude", 1f);
            cmdBufferDebugVis.DrawMesh(fluidRenderMesh, Matrix4x4.identity, debugVisModeMat);  //baronVonTerrain.terrainMesh

            debugAgentResourcesMat.SetPass(0);
            debugAgentResourcesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            debugAgentResourcesMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
            debugAgentResourcesMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
            cmdBufferDebugVis.DrawProcedural(Matrix4x4.identity, debugAgentResourcesMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.critterInitDataCBuffer.count);
            
            debugVisAlgaeParticlesMat.SetPass(0);
            debugVisAlgaeParticlesMat.SetBuffer("foodParticleDataCBuffer", simManager.vegetationManager.plantParticlesCBuffer);
            debugVisAlgaeParticlesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            //debugVisAlgaeParticlesMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            cmdBufferDebugVis.DrawProcedural(Matrix4x4.identity, debugVisAlgaeParticlesMat, 0, MeshTopology.Triangles, 6, simManager.vegetationManager.plantParticlesCBuffer.count);
        
            // add shadow pass eventually
            debugVisAnimalParticlesMat.SetPass(0);
            debugVisAnimalParticlesMat.SetBuffer("animalParticleDataCBuffer", simManager.zooplanktonManager.animalParticlesCBuffer);
            debugVisAnimalParticlesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            //debugVisAnimalParticlesMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            cmdBufferDebugVis.DrawProcedural(Matrix4x4.identity, debugVisAnimalParticlesMat, 0, MeshTopology.Triangles, 6, simManager.zooplanktonManager.animalParticlesCBuffer.count);
        

            //cmdBufferDebugVis
            //mainRenderCam.RemoveAllCommandBuffers();
        }
        else {
            mainRenderCam.RemoveAllCommandBuffers();
            mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMain);
            //if(mainRenderCam.commandBufferCount < 1) {
                //mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMain);
            //}
            
            //cmdBufferMainRender.Clear();

            cmdBufferMain.Clear();
            // control render target capture Here?
            // Create RenderTargets:
            int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
            cmdBufferMain.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
            RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
            cmdBufferMain.Blit(renderTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline
            cmdBufferMain.SetRenderTarget(renderTarget);  // Set render Target
            cmdBufferMain.ClearRenderTarget(true, true, new Color(1f, 0.9f, 0.75f) * 0.8f, 1.0f);  // clear -- needed???
            
            
            
            //baronVonTerrain.RenderCommands(ref cmdBufferTest, renderedSceneID);
            // GROUND:
            // LARGE STROKES!!!!
            baronVonTerrain.groundStrokesLrgDisplayMat.SetPass(0);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesLrgCBuffer);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetTexture("_DecomposerTex", simManager.vegetationManager.resourceGridRT1);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);  
            baronVonTerrain.groundStrokesLrgDisplayMat.SetFloat("_MinFog", 0.0625f);
            baronVonTerrain.groundStrokesLrgDisplayMat.SetVector("_FogColor", simManager.fogColor);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesLrgDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesLrgCBuffer.count);

            // MEDIUM STROKES!!!!
            baronVonTerrain.groundStrokesMedDisplayMat.SetPass(0);
            baronVonTerrain.groundStrokesMedDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonTerrain.groundStrokesMedDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesMedCBuffer);
            baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_DecomposerTex", simManager.vegetationManager.resourceGridRT1);
            baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);       
            baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_MinFog", 0.0625f);             
            baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_FogColor", simManager.fogColor);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesMedDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesMedCBuffer.count);

            // SMALL STROKES!!!!
            baronVonTerrain.groundStrokesSmlDisplayMat.SetPass(0);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesSmlCBuffer);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetTexture("_DecomposerTex", simManager.vegetationManager.resourceGridRT1);
            baronVonTerrain.groundStrokesSmlDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);     
            baronVonTerrain.groundStrokesSmlDisplayMat.SetFloat("_MinFog", 0.0625f);             
            baronVonTerrain.groundStrokesSmlDisplayMat.SetVector("_FogColor", simManager.fogColor);
            
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesSmlDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesSmlCBuffer.count);


            // CARPET BITS:: (microbial mats, algae?) -- DETRITUS / WASTE
            baronVonTerrain.carpetBitsDisplayMat.SetPass(0);
            baronVonTerrain.carpetBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonTerrain.carpetBitsDisplayMat.SetBuffer("groundBitsCBuffer", baronVonTerrain.carpetBitsCBuffer);
            baronVonTerrain.carpetBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonTerrain.carpetBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonTerrain.carpetBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonTerrain.carpetBitsDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);     
            baronVonTerrain.carpetBitsDisplayMat.SetFloat("_MinFog", 0.0625f);  
            baronVonTerrain.carpetBitsDisplayMat.SetFloat("_DetritusDensityLerp", Mathf.Clamp01(simManager.simResourceManager.curGlobalDetritus / 200f));  
            baronVonTerrain.carpetBitsDisplayMat.SetVector("_FogColor", simManager.fogColor); 
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.carpetBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.carpetBitsCBuffer.count);
            
            /*
            // GROUND BITS:::   DECOMPOSERS
            if(simManager.trophicLayersManager.GetDecomposersOnOff()) {
                baronVonTerrain.groundBitsShadowDisplayMat.SetPass(0);
                baronVonTerrain.groundBitsShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                baronVonTerrain.groundBitsShadowDisplayMat.SetBuffer("groundBitsCBuffer", baronVonTerrain.groundBitsCBuffer);                
                baronVonTerrain.groundBitsShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                baronVonTerrain.groundBitsShadowDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
                baronVonTerrain.groundBitsShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                baronVonTerrain.groundBitsShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                baronVonTerrain.groundBitsShadowDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);     
                baronVonTerrain.groundBitsShadowDisplayMat.SetFloat("_MinFog", 0.0625f);  
                baronVonTerrain.groundBitsShadowDisplayMat.SetFloat("_Density", Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(simManager.simResourceManager.curGlobalDecomposers / 100f)));  
                baronVonTerrain.groundBitsShadowDisplayMat.SetVector("_FogColor", simManager.fogColor);                
                cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundBitsShadowDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundBitsCBuffer.count);
            
                

                baronVonTerrain.groundBitsDisplayMat.SetPass(0);
                baronVonTerrain.groundBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                baronVonTerrain.groundBitsDisplayMat.SetBuffer("groundBitsCBuffer", baronVonTerrain.groundBitsCBuffer);                
                baronVonTerrain.groundBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                baronVonTerrain.groundBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
                baronVonTerrain.groundBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                baronVonTerrain.groundBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                baronVonTerrain.groundBitsDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);     
                baronVonTerrain.groundBitsDisplayMat.SetFloat("_MinFog", 0.0625f);                
                baronVonTerrain.groundBitsDisplayMat.SetFloat("_Density", Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(simManager.simResourceManager.curGlobalDecomposers / 100f)));  
                baronVonTerrain.groundBitsDisplayMat.SetVector("_FogColor", simManager.fogColor);                
                cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundBitsCBuffer.count);
            
            }
            */
            /*if(simManager.trophicLayersManager.GetAlgaeOnOff()) {
                // Algae Carpet!
                algaeParticleDisplayMat.SetPass(0);
                algaeParticleDisplayMat.SetBuffer("foodParticleDataCBuffer", simManager.vegetationManager.algaeParticlesCBuffer);
                algaeParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                algaeParticleDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
                algaeParticleDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, algaeParticleDisplayMat, 0, MeshTopology.Triangles, 6, simManager.vegetationManager.algaeParticlesCBuffer.count * 128);
        
            }*/
            
            
            //renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
            //cmdBufferTest.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
            //cmdBufferTest.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

            // SHADOWS:
        
            /*// Surface Bits Shadows:
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetPass(0);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterSurfaceBitsCBuffer);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetTexture("_NutrientTex", simManager.vegetationManager.algaeGridRT1);
            baronVonWater.waterSurfaceBitsShadowsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonWater.waterSurfaceBitsDisplayMat.SetFloat("_CamDistNormalized", Mathf.Lerp(0f, 1f, Mathf.Clamp01((simManager.cameraManager.gameObject.transform.position.z * -1f) / 100f)));
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonWater.waterSurfaceBitsShadowsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterSurfaceBitsCBuffer.count);
        */

            
            
            critterUberStrokeShadowMat.SetPass(0);
            critterUberStrokeShadowMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            critterUberStrokeShadowMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
            critterUberStrokeShadowMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
            critterUberStrokeShadowMat.SetBuffer("critterGenericStrokesCBuffer", critterGenericStrokesCBuffer); 
            critterUberStrokeShadowMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            //critterUberStrokeShadowMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            critterUberStrokeShadowMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            critterUberStrokeShadowMat.SetFloat("_MapSize", SimulationManager._MapSize);
            critterUberStrokeShadowMat.SetFloat("_Turbidity", simManager.fogAmount);     
            critterUberStrokeShadowMat.SetFloat("_MinFog", 0.0625f);  
            critterUberStrokeShadowMat.SetFloat("_Density", Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(simManager.simResourceManager.curGlobalDecomposers / 100f)));  
            critterUberStrokeShadowMat.SetVector("_FogColor", simManager.fogColor);     
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID);
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, critterUberStrokeShadowMat, 0, MeshTopology.Triangles, 6, critterGenericStrokesCBuffer.count);
        
            eggSackShadowDisplayMat.SetPass(0);
            eggSackShadowDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
            eggSackShadowDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
            eggSackShadowDisplayMat.SetBuffer("eggDataCBuffer", simManager.simStateData.eggDataCBuffer);
            eggSackShadowDisplayMat.SetBuffer("eggSackSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
            eggSackShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            eggSackShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            eggSackShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            eggSackShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID);
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, eggSackShadowDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.eggDataCBuffer.count);
        

            /*
            // FOOD PARTICLE SHADOWS::::
            foodParticleShadowDisplayMat.SetPass(0);
            foodParticleShadowDisplayMat.SetBuffer("foodParticleDataCBuffer", simManager.foodParticlesCBuffer);
            foodParticleShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
            foodParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            foodParticleShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            cmdBufferTest.DrawProcedural(Matrix4x4.identity, foodParticleShadowDisplayMat, 0, MeshTopology.Triangles, 6, simManager.foodParticlesCBuffer.count);
            */

            /*if(!simManager.uiManager.tolInspectOn) {
                critterInspectHighlightMat.SetPass(0);
                critterInspectHighlightMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                critterInspectHighlightMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
                critterInspectHighlightMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
                critterInspectHighlightMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
                critterInspectHighlightMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                critterInspectHighlightMat.SetInt("_HoverAgentIndex", simManager.cameraManager.mouseHoverAgentIndex);
                critterInspectHighlightMat.SetInt("_LockedOnAgentIndex", simManager.cameraManager.targetCritterIndex);
        
                float isHoverOn = 0f;
                if (simManager.cameraManager.isMouseHoverAgent) {
                    isHoverOn = 1f;
                }
                float isHighlightOn = 0f;
                if (simManager.uiManager.curActiveTool == UIManager.ToolType.Inspect) {
                    isHighlightOn = 1f;
                }
                float isLockedOn = 0f;
                if (simManager.cameraManager.isFollowing) {
                    isLockedOn = 1f;
                }
                critterInspectHighlightMat.SetFloat("_IsHover", isHoverOn);
                critterInspectHighlightMat.SetFloat("_IsHighlighted", isHighlightOn);
                critterInspectHighlightMat.SetFloat("_IsLockedOn", isLockedOn);
                critterInspectHighlightMat.SetInt("_SelectedSpecies", simManager.uiManager.treeOfLifeManager.selectedID);
                cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterInspectHighlightMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.critterInitDataCBuffer.count);
            }*/

            if(simManager.trophicLayersManager.GetZooplanktonOnOff()) {
                animalParticleShadowDisplayMat.SetPass(0);
                animalParticleShadowDisplayMat.SetBuffer("animalParticleDataCBuffer", simManager.zooplanktonManager.animalParticlesCBuffer);
                animalParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", curveRibbonVerticesCBuffer);
                animalParticleShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                animalParticleShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, animalParticleShadowDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, simManager.zooplanktonManager.animalParticlesCBuffer.count);
        
            }
            
            if(simManager.trophicLayersManager.GetAlgaeOnOff()) {
                // algae shadows:
                foodParticleShadowDisplayMat.SetPass(0);
                foodParticleShadowDisplayMat.SetBuffer("foodParticleDataCBuffer", simManager.vegetationManager.plantParticlesCBuffer);
                foodParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                foodParticleShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                foodParticleShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                foodParticleShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                foodParticleShadowDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);     
                foodParticleShadowDisplayMat.SetFloat("_MinFog", 0.0625f);  
                foodParticleShadowDisplayMat.SetVector("_FogColor", simManager.fogColor);      
                cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); 
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, foodParticleShadowDisplayMat, 0, MeshTopology.Triangles, 6, simManager.vegetationManager.plantParticlesCBuffer.count * 32);
        
            }

            // STIR STICK!!!!
            if(simManager.uiManager.curActiveTool == UIManager.ToolType.Stir) {
                //gizmoStirStickAMat.SetPass(0);
                //simManager.uiManager.smoothedCtrlCursorVel
                Quaternion rot = Quaternion.Euler(new Vector3(Mathf.Clamp(simManager.uiManager.smoothedMouseVel.y * 2.5f + 10f, -45f, 45f), Mathf.Clamp(simManager.uiManager.smoothedMouseVel.x * -1.5f, -45f, 45f), 0f));
                float scale = Mathf.Lerp(0.35f, 1.75f, baronVonWater.camDistNormalized);
                Matrix4x4 stirStickTransformMatrix = Matrix4x4.TRS(new Vector3(simManager.uiManager.curMousePositionOnWaterPlane.x, simManager.uiManager.curMousePositionOnWaterPlane.y, simManager.uiManager.stirStickDepth), rot, Vector3.one * scale);
                Mesh stickMesh = meshStirStickMed;
                if(baronVonWater.camDistNormalized > 0.67f) {
                    stickMesh = meshStirStickLrg;
                }
                if(baronVonWater.camDistNormalized < 0.24f) {
                    stickMesh = meshStirStickSml;
                }

                gizmoStirStickShadowMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                gizmoStirStickShadowMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                gizmoStirStickShadowMat.SetFloat("_MapSize", SimulationManager._MapSize);
                gizmoStirStickShadowMat.SetFloat("_MinFog", 0.0625f);  
                gizmoStirStickShadowMat.SetVector("_FogColor", simManager.fogColor);
                gizmoStirStickShadowMat.SetFloat("_Turbidity", simManager.fogAmount); 
                cmdBufferMain.DrawMesh(stickMesh, stirStickTransformMatrix, gizmoStirStickShadowMat);

                gizmoStirStickAMat.SetFloat("_MinFog", 0.0625f);  
                gizmoStirStickAMat.SetVector("_FogColor", simManager.fogColor);
                gizmoStirStickAMat.SetFloat("_Turbidity", simManager.fogAmount);
                gizmoStirStickAMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                cmdBufferMain.DrawMesh(stickMesh, stirStickTransformMatrix, gizmoStirStickAMat);
            }

            if(simManager.trophicLayersManager.GetZooplanktonOnOff()) {
                // add shadow pass eventually
                animalParticleDisplayMat.SetPass(0);
                animalParticleDisplayMat.SetBuffer("animalParticleDataCBuffer", simManager.zooplanktonManager.animalParticlesCBuffer);
                animalParticleDisplayMat.SetBuffer("quadVerticesCBuffer", curveRibbonVerticesCBuffer);
                animalParticleDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, animalParticleDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, simManager.zooplanktonManager.animalParticlesCBuffer.count);
        
            }

            if(simManager.trophicLayersManager.GetAgentsOnOff()) {
                // Highlight trail:
                critterHighlightTrailMat.SetPass(0);
                critterHighlightTrailMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
                critterHighlightTrailMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
                critterHighlightTrailMat.SetBuffer("highlightTrailDataCBuffer", critterHighlightTrailCBuffer);
                critterHighlightTrailMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                critterHighlightTrailMat.SetFloat("_MapSize", SimulationManager._MapSize);
                float highlightOn = 0f;
                if(simManager.uiManager.curActiveTool == UIManager.ToolType.Inspect) {
                    highlightOn = 1f;
                }
                critterHighlightTrailMat.SetFloat("_HighlightOn", highlightOn);
                //public int mouseHoverAgentIndex = 0;
                //public bool isMouseHoverAgent = false;
                //uniform float _HoverID;
			    //uniform float _SelectedID;
                critterHighlightTrailMat.SetInt("_HoverID", simManager.uiManager.cameraManager.mouseHoverAgentIndex);
                critterHighlightTrailMat.SetInt("_SelectedID", simManager.uiManager.cameraManager.targetCritterIndex);
                critterHighlightTrailMat.SetFloat("_IsHover", simManager.uiManager.cameraManager.isMouseHoverAgent ? 1f : 0f);
                critterHighlightTrailMat.SetFloat("_IsSelected", simManager.uiManager.cameraManager.isFollowing ? 1f : 0f);
                critterHighlightTrailMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, critterHighlightTrailMat, 0, MeshTopology.Triangles, 6, critterHighlightTrailCBuffer.count);
            
        
                eggSackStrokeDisplayMat.SetPass(0);
                eggSackStrokeDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
                eggSackStrokeDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
                eggSackStrokeDisplayMat.SetBuffer("eggDataCBuffer", simManager.simStateData.eggDataCBuffer);
                eggSackStrokeDisplayMat.SetBuffer("eggSackSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
                eggSackStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                eggSackStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                eggSackStrokeDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, eggSackStrokeDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.eggDataCBuffer.count);
        
                // What is this????
                critterDebugGenericStrokeMat.SetPass(0);
                critterDebugGenericStrokeMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                critterDebugGenericStrokeMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
                critterDebugGenericStrokeMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
                critterDebugGenericStrokeMat.SetBuffer("critterGenericStrokesCBuffer", critterGenericStrokesCBuffer);    
                critterDebugGenericStrokeMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                //critterDebugGenericStrokeMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
                //critterDebugGenericStrokeMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
                highlightOn = 0f;
                if(simManager.uiManager.curActiveTool == UIManager.ToolType.Inspect) {
                    highlightOn = 1f;
                }
                critterDebugGenericStrokeMat.SetFloat("_HighlightOn", highlightOn);                
                critterDebugGenericStrokeMat.SetInt("_HoverID", simManager.uiManager.cameraManager.mouseHoverAgentIndex);
                critterDebugGenericStrokeMat.SetInt("_SelectedID", simManager.uiManager.cameraManager.targetCritterIndex);
                critterDebugGenericStrokeMat.SetFloat("_IsHover", simManager.uiManager.cameraManager.isMouseHoverAgent ? 1f : 0f);
                critterDebugGenericStrokeMat.SetFloat("_IsSelected", simManager.uiManager.cameraManager.isFollowing ? 1f : 0f);

                critterDebugGenericStrokeMat.SetFloat("_MapSize", SimulationManager._MapSize);            
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, critterDebugGenericStrokeMat, 0, MeshTopology.Triangles, 6, critterGenericStrokesCBuffer.count);

                // *** Revisit this in future - probably can get away without it, just use one pass for all eggSacks
                eggCoverDisplayMat.SetPass(0);
                eggCoverDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
                eggCoverDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
                eggCoverDisplayMat.SetBuffer("eggSackSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
                eggCoverDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                eggCoverDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                eggCoverDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, eggCoverDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.critterInitDataCBuffer.count);
            

            }
            
            
            /*
            // suspended particle bits:
            baronVonWater.waterNutrientsBitsDisplayMat.SetPass(0);
            baronVonWater.waterNutrientsBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonWater.waterNutrientsBitsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterNutrientsBitsCBuffer);
            baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonWater.waterNutrientsBitsDisplayMat.SetTexture("_NutrientTex", simManager.vegetationManager.resourceGridRT1);
            baronVonWater.waterNutrientsBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonWater.waterNutrientsBitsDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
            baronVonWater.waterNutrientsBitsDisplayMat.SetFloat("_NutrientDensity", Mathf.Clamp01(simManager.simResourceManager.curGlobalNutrients / 300f));
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonWater.waterNutrientsBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterNutrientsBitsCBuffer.count);
            */
            

            if(simManager.trophicLayersManager.GetAlgaeOnOff()) {
                foodParticleDisplayMat.SetPass(0);
                foodParticleDisplayMat.SetBuffer("foodParticleDataCBuffer", simManager.vegetationManager.plantParticlesCBuffer);
                foodParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                foodParticleDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                foodParticleDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, foodParticleDisplayMat, 0, MeshTopology.Triangles, 6, simManager.vegetationManager.plantParticlesCBuffer.count * 32);
        
            }
                                    
            
            // FLUID ITSELF:
            fluidRenderMat.SetPass(0);
            fluidRenderMat.SetTexture("_DensityTex", fluidManager._DensityA);
            fluidRenderMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            fluidRenderMat.SetTexture("_PressureTex", fluidManager._PressureA);
            fluidRenderMat.SetTexture("_DivergenceTex", fluidManager._Divergence);
            fluidRenderMat.SetTexture("_ObstaclesTex", fluidManager._ObstaclesRT);
            fluidRenderMat.SetTexture("_TerrainHeightTex", baronVonTerrain.terrainHeightDataRT);
            fluidRenderMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            fluidRenderMat.SetTexture("_SpiritBrushTex", spiritBrushRT);
            cmdBufferMain.DrawMesh(fluidRenderMesh, Matrix4x4.identity, fluidRenderMat);

          
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
                
            // WATER DEBRIS BITS:
            /*baronVonWater.waterDebrisBitsDisplayMat.SetPass(0);
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
                
            /*
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
            */
        
            // Critter Energy blops!
            /*critterEnergyDotsMat.SetPass(0);
            critterEnergyDotsMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            critterEnergyDotsMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
            critterEnergyDotsMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
            critterEnergyDotsMat.SetBuffer("bodyStrokesCBuffer", critterEnergyDotsCBuffer);
            critterEnergyDotsMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            cmdBufferTest.DrawProcedural(Matrix4x4.identity, critterEnergyDotsMat, 0, MeshTopology.Triangles, 6, critterEnergyDotsCBuffer.count);
            */
            /*
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
            */
            // DEBUG BODY START:
            // Test Debug Critter Body:
            //critterDebugGenericStrokeMat

            

            // WATER BITS TEMP::::::::::::::^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // use this as algae grid???
            
            float yOffset = Mathf.Sin(simManager.cameraManager.curTiltAngleDegrees * Mathf.Deg2Rad) * simManager.cameraManager.curCameraPos.z;
            Vector4 camFocusPos = new Vector4(simManager.cameraManager.curCameraPos.x, simManager.cameraManager.curCameraPos.y + yOffset, 0f, 0f);
        /*
            // Water surface reflective
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetPass(0);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterQuadStrokesCBufferLrg);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_ResourceTex", simManager.vegetationManager.resourceGridRT1);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetTexture("_WaterColorTex", simManager.environmentFluidManager._DensityA);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized); // Mathf.Lerp(0f, 1f, Mathf.Clamp01((simManager.cameraManager.gameObject.transform.position.z * -1f) / 100f)));
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetVector("_CamFocusPosition", camFocusPos);
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);            
            baronVonWater.waterQuadStrokesLrgDisplayMat.SetVector("_FogColor", simManager.fogColor);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonWater.waterQuadStrokesLrgDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterQuadStrokesCBufferLrg.count);

            // Water surface reflective
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetPass(0);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterQuadStrokesCBufferSml);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_ResourceTex", simManager.vegetationManager.resourceGridRT1);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetTexture("_WaterColorTex", simManager.environmentFluidManager._DensityA);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized); // Mathf.Lerp(0f, 1f, Mathf.Clamp01((simManager.cameraManager.gameObject.transform.position.z * -1f) / 100f)));
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetVector("_CamFocusPosition", camFocusPos);
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);            
            baronVonWater.waterQuadStrokesSmlDisplayMat.SetVector("_FogColor", simManager.fogColor);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonWater.waterQuadStrokesSmlDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterQuadStrokesCBufferSml.count);
        */
                
            /*// SURFACE BITS FLOATY:::::  // LILY PADS
            baronVonWater.waterSurfaceBitsDisplayMat.SetPass(0);
            baronVonWater.waterSurfaceBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonWater.waterSurfaceBitsDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonWater.waterSurfaceBitsCBuffer);
            baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightMap);
            baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            baronVonWater.waterSurfaceBitsDisplayMat.SetTexture("_NutrientTex", simManager.vegetationManager.algaeGridRT1);
            baronVonWater.waterSurfaceBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonWater.waterSurfaceBitsDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonWater.waterSurfaceBitsDisplayMat, 0, MeshTopology.Triangles, 6, baronVonWater.waterSurfaceBitsCBuffer.count);
        */
            // DRY LAND!!!!!!
            baronVonTerrain.groundDryLandDisplayMat.SetPass(0);
            baronVonTerrain.groundDryLandDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            baronVonTerrain.groundDryLandDisplayMat.SetBuffer("frameBufferStrokesCBuffer", baronVonTerrain.groundStrokesSmlCBuffer);
            baronVonTerrain.groundDryLandDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            baronVonTerrain.groundDryLandDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            baronVonTerrain.groundDryLandDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            //baronVonTerrain.groundDryLandDisplayMat.SetTexture("_ResourceTex", simManager.vegetationManager.resourceGridRT1);
            baronVonTerrain.groundDryLandDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);            
            baronVonTerrain.groundDryLandDisplayMat.SetVector("_FogColor", simManager.fogColor);
            cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
            //cmdBufferMain.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundDryLandDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.groundStrokesSmlCBuffer.count);
            
        

            //if(simManager.uiManager.tolInspectOn) {
            //}
            gizmoStirToolMat.SetPass(0);
            gizmoStirToolMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            gizmoStirToolMat.SetBuffer("gizmoStirToolPosCBuffer", gizmoCursorPosCBuffer);
            gizmoStirToolMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            gizmoStirToolMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            gizmoStirToolMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
            gizmoStirToolMat.SetFloat("_Radius", Mathf.Lerp(0.067f, 5f, baronVonWater.camDistNormalized));  // **** Make radius variable! (possibly texture based?)
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, gizmoStirToolMat, 0, MeshTopology.Triangles, 6, 1);
        
        }
        
        
        /*if(isDebugRenderOn) {
            
            
            

        }*/
        

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

    public void ClickTestTerrain(bool on) {  // *** RENAME!!! ***********

        //Debug.Log("CLICKED TEST TERRAIN BUTTON! " + on.ToString());

        // TEMP!!! ********* DECOMPOSERS!!!!!

        
        baronVonTerrain.terrainBlitMat.SetTexture("_DeltaTex", spiritBrushRT);
        baronVonTerrain.terrainBlitMat.SetInt("_ChannelID", simManager.uiManager.selectedToolbarTerrainLayer);

        float intensity = 0.0420f; // Mathf.Lerp(0.02f, 0.06f, baronVonWater.camDistNormalized) * 1.05f;
        baronVonTerrain.terrainBlitMat.SetFloat("_Intensity", intensity);

        float addSubtract = spiritBrushPosNeg; // 1f;
        //if(simManager.uiManager.curActiveTool == UIManager.ToolType.Remove) {
        //    addSubtract = -1f;
        //}
        if(on) {  // Actively brushing this frame
            baronVonTerrain.terrainBlitMat.SetFloat("_AddSubtractSign", addSubtract);
            Graphics.Blit(baronVonTerrain.terrainHeightRT0, baronVonTerrain.terrainHeightRT1, baronVonTerrain.terrainBlitMat);
            Graphics.Blit(baronVonTerrain.terrainHeightRT1, baronVonTerrain.terrainHeightRT0, baronVonTerrain.terrainSimulationBlitMat); 

            
        }
        else {
            addSubtract = 0f;
            baronVonTerrain.terrainBlitMat.SetFloat("_AddSubtractSign", addSubtract);
            Graphics.Blit(baronVonTerrain.terrainHeightRT0, baronVonTerrain.terrainHeightRT1);
            Graphics.Blit(baronVonTerrain.terrainHeightRT1, baronVonTerrain.terrainHeightRT0, baronVonTerrain.terrainSimulationBlitMat); 
        }
        
        baronVonTerrain.terrainGenerateColorBlitMat.SetTexture("_MainTex", baronVonTerrain.terrainHeightRT0);
        baronVonTerrain.terrainGenerateColorBlitMat.SetTexture("_DeltaTex", spiritBrushRT);
        baronVonTerrain.terrainGenerateColorBlitMat.SetVector("_Color0", baronVonTerrain.bedrockSlotGenomeCurrent.color); // new Vector4(0.54f, 0.43f, 0.37f, 1f));
        baronVonTerrain.terrainGenerateColorBlitMat.SetVector("_Color1", baronVonTerrain.stoneSlotGenomeCurrent.color); // new Vector4(0.9f, 0.9f, 0.8f, 1f));
        baronVonTerrain.terrainGenerateColorBlitMat.SetVector("_Color2", baronVonTerrain.pebblesSlotGenomeCurrent.color); // new Vector4(0.7f, 0.8f, 0.9f, 1f));
        baronVonTerrain.terrainGenerateColorBlitMat.SetVector("_Color3", baronVonTerrain.sandSlotGenomeCurrent.color); // new Vector4(0.7f, 0.6f, 0.3f, 1f));
        Graphics.Blit(baronVonTerrain.terrainHeightRT0, baronVonTerrain.terrainColorRT0, baronVonTerrain.terrainGenerateColorBlitMat);

        baronVonTerrain.InitializeTerrain();
        baronVonTerrain.AlignGroundStrokesToTerrain();
        
        
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
        if(spiritBrushRenderCamera != null) {
            spiritBrushRenderCamera.RemoveAllCommandBuffers();
        }
        /*if(treeOfLifeSpeciesTreeRenderCamera != null) {
            treeOfLifeSpeciesTreeRenderCamera.RemoveAllCommandBuffers();
        }*/
        /*if(treeOfLifeRenderCamera != null) {
            treeOfLifeRenderCamera.RemoveAllCommandBuffers();
        }*/
        if(slotPortraitRenderCamera != null) {
            slotPortraitRenderCamera.RemoveAllCommandBuffers();
        }
        if(resourceSimRenderCamera != null) {
            resourceSimRenderCamera.RemoveAllCommandBuffers();
        }

        if(baronVonTerrain != null) {
            baronVonTerrain.Cleanup();
        }
        if(baronVonWater != null) {
            baronVonWater.Cleanup();
        }

        if(cmdBufferMain != null) {
            cmdBufferMain.Release();
        }
        if(cmdBufferDebugVis != null) {
            cmdBufferDebugVis.Release();
        }
        //if(cmdBufferMainRender != null) {
        //    cmdBufferMainRender.Release();
        //}
        if(cmdBufferFluidObstacles != null) {
            cmdBufferFluidObstacles.Release();
        }
        if(cmdBufferFluidColor != null) {
            cmdBufferFluidColor.Release();
        }
        if(cmdBufferResourceSim != null) {
            cmdBufferResourceSim.Release();
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
        if(critterGenericStrokesCBuffer != null) {
            critterGenericStrokesCBuffer.Release();
        }
        if(critterEnergyDotsCBuffer != null) {
            critterEnergyDotsCBuffer.Release();
        }
        if(critterFoodDotsCBuffer != null) {
            critterFoodDotsCBuffer.Release();
        }
        if(critterHighlightTrailCBuffer != null) {
            critterHighlightTrailCBuffer.Release();
        }
        if(gizmoCursorPosCBuffer != null) {
            gizmoCursorPosCBuffer.Release();
        }
        if(gizmoFeedToolPosCBuffer != null) {
            gizmoFeedToolPosCBuffer.Release();
        }
        // TREE OF LIFE:
        if(testTreeOfLifePositionCBuffer != null) {
            testTreeOfLifePositionCBuffer.Release();
        }
        if(treeOfLifeEventLineDataCBuffer != null) {
            treeOfLifeEventLineDataCBuffer.Release();
        }
        if(treeOfLifeWorldStatsValuesCBuffer != null) {
            treeOfLifeWorldStatsValuesCBuffer.Release();
        }
        if(treeOfLifeSpeciesSegmentsCBuffer != null) {
            treeOfLifeSpeciesSegmentsCBuffer.Release();
        }
        if(treeOfLifeSpeciesDataKeyCBuffer != null) {
            treeOfLifeSpeciesDataKeyCBuffer.Release();
        }
        if(treeOfLifeSpeciesDataHeadPosCBuffer != null) {
            treeOfLifeSpeciesDataHeadPosCBuffer.Release();
        }
        // OLD TOL:
        if(treeOfLifeLeafNodeDataCBuffer != null) {
            treeOfLifeLeafNodeDataCBuffer.Release();
        }
        if(treeOfLifeNodeColliderDataCBufferA != null) {
            treeOfLifeNodeColliderDataCBufferA.Release();
        }
        if(treeOfLifeNodeColliderDataCBufferB != null) {
            treeOfLifeNodeColliderDataCBufferB.Release();
        }
        if(treeOfLifeStemSegmentDataCBuffer != null) {
            treeOfLifeStemSegmentDataCBuffer.Release();
        }

        if(toolbarCritterPortraitStrokesCBuffer != null) {
            toolbarCritterPortraitStrokesCBuffer.Release();
        }
        
        if(treeOfLifeStemSegmentVerticesCBuffer != null) {
            treeOfLifeStemSegmentVerticesCBuffer.Release();
        }
        if(treeOfLifeBasicStrokeDataCBuffer != null) {
            treeOfLifeBasicStrokeDataCBuffer.Release();
        }
        if(treeOfLifePortraitBorderDataCBuffer != null) {
            treeOfLifePortraitBorderDataCBuffer.Release();
        }
        if(treeOflifePortraitDataCBuffer != null) {
            treeOflifePortraitDataCBuffer.Release();
        }
        if(treeOfLifePortraitEyeDataCBuffer != null) {
            treeOfLifePortraitEyeDataCBuffer.Release();
        }
        if(toolbarPortraitCritterInitDataCBuffer != null) {
            toolbarPortraitCritterInitDataCBuffer.Release();
        }
        if(toolbarPortraitCritterSimDataCBuffer != null) {
            toolbarPortraitCritterSimDataCBuffer.Release();
        }
        
        /*if(agentHoverHighlightCBuffer != null) {
            agentHoverHighlightCBuffer.Release();
        }*/
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
