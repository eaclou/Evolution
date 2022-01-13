using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Playcraft;
using Random = UnityEngine.Random;

/// Initializes and updates all materials and compute buffers in game, including individual agents
// * WPP: break into regions to focus optimization efforts
    // General initialization: low priority
    // Agent initialization: low/moderate priority
    // Update loop: high priority
public class TheRenderKing : Singleton<TheRenderKing> 
{
    // Singleton references
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simManager => SimulationManager.instance;
    SimulationStateData simStateData => simManager.simStateData;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManager => UIManager.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    ZooplanktonManager zooplanktonManager => simManager.zooplanktonManager;
    VegetationManager vegetationManager => simManager.vegetationManager;
    SelectionManager selectionManager => SelectionManager.instance;
    CandidateAgentData focusedCandidate => selectionManager.focusedCandidate;
    
    WorldSpiritHubUI worldSpiritHubUI => uiManager.worldSpiritHubUI;
    TrophicSlot selectedWorldSpiritSlot => worldSpiritHubUI.selectedWorldSpiritSlot;
    Agent[] agents => simManager.agents;
    
    CreaturePanelUI creaturePanelUI => uiManager.creaturePanelUI;
    ClockPanelUI clockPanelUI => uiManager.clockPanelUI;
    
    /// Max number of threads that can be executed by a shader group
    const int maxShaderThreads = 1024;

    // Set in inspector
    public BaronVonTerrain baronVonTerrain;
    public BaronVonWater baronVonWater;

    public GameObject sunGO;
    
    [NonSerialized] public Vector3 sunDirection = new Vector3(-1f, 0.7f, -1f).normalized;
    public float minimumFogDensity = 0.3f;

    public Camera mainRenderCam;
    public Camera fluidObstaclesRenderCamera;
    public Camera fluidColorRenderCamera;
    public Camera spiritBrushRenderCamera;
    public Camera slotPortraitRenderCamera;
    public Camera resourceSimRenderCamera;
    public Camera worldTreeRenderCamera;

    public bool isDebugRender = false;

    [NonSerialized]
    public CommandBuffer cmdBufferMain;
    private CommandBuffer cmdBufferDebugVis;
    private CommandBuffer cmdBufferFluidObstacles;
    private CommandBuffer cmdBufferFluidColor;
    private CommandBuffer cmdBufferSpiritBrush;
    private CommandBuffer cmdBufferSlotPortraitDisplay;
    private CommandBuffer cmdBufferResourceSim;
    [NonSerialized]
    public CommandBuffer cmdBufferWorldTree;

    //public ComputeShader computeShaderBrushStrokes;  // Not used
    //public ComputeShader computeShaderUberChains;    // Not used
    public ComputeShader computeShaderCritters;
    public ComputeShader computeShaderEggSacks;
    public ComputeShader computeShaderTreeOfLife; //***EAC revisit --> convert to WorldTree
    public ComputeShader computeShaderSpiritBrush;

    public Mesh meshStirStickA;     // Not used
    public Mesh meshStirStickSml;   // Not used
    public Mesh meshStirStickMed;   // Not used
    public Mesh meshStirStickLrg;   // Not used
    
    public Material gizmoStirStickAMat;
    public Material gizmoStirStickShadowMat;    // Not used
    public Material gizmoProtoSpiritClickableMat; // Not used

    public Material cursorParticlesDisplayMat;  // Not used

    // ORGANIZE AND REMOVE UNUSED!!!!!! *********
    public Material rockMat;
    public Material backgroundMat;
    public Material terrainMeshOpaqueMat;
    public Material debugVisModeMat;
    public Material debugVisAlgaeParticlesMat;
    public Material debugVisAnimalParticlesMat;
    public Material basicStrokeDisplayMat;
    public Material fluidBackgroundColorMat;    // Not used
    public Material floatyBitsDisplayMat;
    public Material floatyBitsShadowDisplayMat;
    public Material fluidRenderMat;             // Not used

    public Material eggSackStrokeDisplayMat;
    public Material eggSackShadowDisplayMat;

    public Material debugAgentResourcesMat;
    public Material algaeParticleDisplayMat;
    public Material plantParticleDisplayMat;
    public Material plantParticleShadowDisplayMat;
    public Material animalParticleDisplayMat;
    public Material animalParticleShadowDisplayMat;

    public Material eggCoverDisplayMat;
    public Material critterDebugGenericStrokeMat;
    public Material critterUberStrokeShadowMat;     // Not used

    //public Material critterInspectHighlightMat;
    //public Material critterHighlightTrailMat;

    public Material algaeParticleColorInjectMat;
    public Material zooplanktonParticleColorInjectMat;
    public Material playerBrushColorInjectMat;      // Not used
    public Material resourceSimTransferMat;
    public Material resourceSimAgentDataMat;
    public Material plantParticleDataMat;

    public Material gizmoStirToolMat;
    public Material gizmoFeedToolMat;

    public Material treeOfLifeLeafNodesMat;
    public Material treeOfLifeStemSegmentsMat;
    public Material treeOfLifeBackdropMat;
    public Material treeOfLifeBackdropPortraitBorderMat;
    public Material treeOfLifePortraitBorderMat;    // Not used
    public Material treeOfLifePortraitMat;
    public Material treeOfLifePortraitEyeMat;       // Not used
    public Material treeOfLifeDecorationMat;        // Not used

    public Material treeOfLifeWorldStatsMat;     // Not used
    public Material treeOfLifeSpeciesLineMat;    // Not used
    public Material treeOfLifeEventsLineMat;     // Not used
    public Material treeOfLifeSpeciesHeadTipMat; // Not used
    public Material treeOfLifeCursorLineMat;     // Not used
    public Material toolbarSpeciesPortraitStrokesMat;
    public Material spiritBrushRenderMultiBurstMat;
    public Material spiritBrushRenderMat;
    //public Material mutationUIVertebratesRenderTexMat;
    public Material worldTreeDisplayRTMat;      // Not used
    public Material worldTreeLineDataMat;
    public Material clockOrbitLineDataMat;      // Not used

    public ComputeBuffer gizmoCursorPosCBuffer;
    public ComputeBuffer gizmoFeedToolPosCBuffer;
    
    private Mesh fluidRenderMesh;
    public Texture2D skyTexture;

    private bool isInitialized = false;

    private const float velScale = 0.390f; // Conversion for rigidBody Vel --> fluid vel units ----  // approx guess for now ** HACK

    public bool isBrushing = false;

    [NonSerialized]
    public ComputeBuffer quadVerticesCBuffer;  // quad mesh

    private int numCurveRibbonQuads = 4;
    private ComputeBuffer curveRibbonVerticesCBuffer;  // short ribbon mesh

    //private int numStrokesPerCritterSkin = 128;        
    private int numStrokesPerCritterLength = 32;   // This is the official order of brush indexing!!!
    private int numStrokesPerCritterCross = 32;
    private int numStrokesPerCritterEyes = 256;
    private int numStrokesPerCritterMouth = 64;
    private int numStrokesPerCritterTeeth = 64;
    private int numStrokesPerCritterPectoralFins = 64;
    private int numStrokesPerCritterDorsalFin = 128;
    private int numStrokesPerCritterTailFin = 128;
    private int numStrokesPerCritterSkinDetail = 128;
    private ComputeBuffer mainCritterStrokesCBuffer;    

    private int numFloatyBits = maxShaderThreads;
    private ComputeBuffer floatyBitsCBuffer;

    private BasicStrokeData[] obstacleStrokeDataArray;
    private ComputeBuffer obstacleStrokesCBuffer;

    private BasicStrokeData[] colorInjectionStrokeDataArray;
    private ComputeBuffer colorInjectionStrokesCBuffer;

    // Future use
    private struct CursorParticleData 
    {
        public int index;
        public Vector3 worldPos;
        public Vector2 heading;
        public Vector2 localScale;
        public float lifespan;
        public float age01;
        public Vector4 extraVec4;
        public Vector2 vel;
        public float drag;
        public float noiseStart;
        public float noiseEnd;
        public float noiseFreq;
        public int brushType;
        
        public CursorParticleData(Vector3[] spawnPoints, int spawnPointIndex)
        {
            worldPos = new Vector3(Random.Range(0f, 256f), Random.Range(0f, 256f), 0f);
            vel = Random.insideUnitCircle;
            heading = new Vector2(spawnPoints[spawnPointIndex].x, spawnPoints[spawnPointIndex].z);
            lifespan = Random.Range(20f, 40f);
            age01 = Random.Range(0f, 1f);
            
            brushType = default;
            drag = default;
            extraVec4 = default;
            index = default;
            localScale = default;
            noiseEnd = default;
            noiseFreq = default;
            noiseStart = default;
        }
    }
    
    private struct SpiritBrushQuadData 
    {
        public int index;
        public Vector3 worldPos;
        public Vector2 heading;
        public Vector2 localScale;
        public float lifespan;
        public float age01;
        public Vector4 extraVec4;
        public Vector2 vel;
        public float drag;
        public float noiseStart;
        public float noiseEnd;
        public float noiseFreq;
        public int brushType;
        
        public SpiritBrushQuadData(float xMin, float xMax, float yMin, float yMax, float maxVelocity)
        {
            worldPos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0f);
            vel = new Vector2(0f, maxVelocity);
            heading = new Vector2(0f, 1f);
            lifespan = 1f;
            age01 = 0f;
            
            brushType = default;
            drag = default;
            extraVec4 = default;
            index = default;
            localScale = default;
            noiseEnd = default;
            noiseFreq = default;
            noiseStart = default;
        }
    }
    
    private ComputeBuffer spiritBrushQuadDataSpawnCBuffer;
    private ComputeBuffer spiritBrushQuadDataCBuffer0;
    private ComputeBuffer spiritBrushQuadDataCBuffer1;
    private SpiritBrushQuadData[] spiritBrushQuadDataArray;
    
    private ComputeBuffer cursorParticlesCBuffer0;
    private ComputeBuffer cursorParticlesCBuffer1;
    private CursorParticleData[] cursorParticlesArray;

    public Material debugMaterial; 
    public Mesh debugMesh;         
    public RenderTexture debugRT;  // Used to see texture inside editor (inspector)

    public bool isSpiritBrushOn = false;
    public float spiritBrushPosNeg = 1f;
    public RenderTexture spiritBrushRT;
    private int spiritBrushResolution = 128;

    //public Texture2D critterBodyWidthsTex;
    public float fullscreenFade = 1f;

    //private Vector3[] testTreeOfLifePositionArray;
    //private ComputeBuffer testTreeOfLifePositionCBuffer;
    //private TreeOfLifeEventLineData[] treeOfLifeEventLineDataArray;
    private ComputeBuffer treeOfLifeEventLineDataCBuffer;
    private ComputeBuffer treeOfLifeWorldStatsValuesCBuffer;
    private ComputeBuffer treeOfLifeSpeciesSegmentsCBuffer;

    public ComputeBuffer treeOfLifeSpeciesDataKeyCBuffer;
    public Vector3[] treeOfLifeSpeciesDataHeadPosArray;     // Values assigned but not used
    public ComputeBuffer treeOfLifeSpeciesDataHeadPosCBuffer;

    #region Dead Code (please delete)
    //private int numTreeOfLifeStemSegmentQuads = 16;
    //private ComputeBuffer treeOfLifeStemSegmentVerticesCBuffer;  // short ribbon mesh
    //private int maxNumTreeOfLifeNodes = 512; // max numSpecies
    //private int maxNumTreeOfLifeSegments = 512;
    //private TreeOfLifeNodeColliderData[] treeOfLifeNodeColliderDataArray;
    //private ComputeBuffer treeOfLifeNodeColliderDataCBufferA;
    //private ComputeBuffer treeOfLifeNodeColliderDataCBufferB;
    //private TreeOfLifeLeafNodeData[] treeOfLifeLeafNodeDataArray;
    //private ComputeBuffer treeOfLifeLeafNodeDataCBuffer;
    //private ComputeBuffer treeOfLifeStemSegmentDataCBuffer;
    //private int curNumTreeOfLifeStemSegments = 0;

    //public ComputeBuffer treeOfLifeBasicStrokeDataCBuffer;
    //public ComputeBuffer treeOfLifePortraitBorderDataCBuffer; //***EAC rename or replace!!!
    //public ComputeBuffer treeOflifePortraitDataCBuffer;
    //public ComputeBuffer treeOfLifePortraitEyeDataCBuffer;

    // only the data that needs to be transferred between CPU & GPU  - minimize!!
    // Is this still needed??
    //public struct TreeOfLifeNodeColliderData {  
    //    public Vector3 localPos;
    //    public Vector3 scale;
    //}
    /*
    public struct TreeOfLifeLeafNodeData 
    {
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
        
        public TreeOfLifeLeafNodeData(SpeciesGenomePool newSpecies)
        {
            speciesID = newSpecies.speciesID;
            parentSpeciesID = newSpecies.parentSpeciesID;
            graphDepth = newSpecies.depthLevel;
            primaryHue = newSpecies.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            secondaryHue = newSpecies.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            growthPercentage = 1f;
            
            age = 0f;
            decayPercentage = 0f;
            isActive = 1f;
            isExtinct = 0f;
            
            isHover = default;
            isSelected = default;
            relFitnessScore = default;
        }
        
        public TreeOfLifeLeafNodeData(int speciesID)
        {
            this.speciesID = speciesID;
            parentSpeciesID = 0;
            graphDepth = 0;
            primaryHue = Vector3.zero;
            secondaryHue = Vector3.zero;
            growthPercentage = 0f;
            
            age = 0f;
            decayPercentage = 0f;
            isActive = 1f;
            isExtinct = 1f;
            
            isHover = default;
            isSelected = default;
            relFitnessScore = default;
        }
    }
    
    public struct TreeOfLifeStemSegmentStruct 
    {
        public int speciesID;
        public int fromID;
        public int toID;
        public float attachPosLerp;
        
        public TreeOfLifeStemSegmentStruct(int curSpeciesID, int newSpeciesID, int parentSpeciesID)
        {
            speciesID = newSpeciesID;
            fromID = parentSpeciesID;
            toID = curSpeciesID;
            
            attachPosLerp = default;
        }
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
    */
    
    /*
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
    */
    
    #endregion
    
    public struct FloatyBitData {
        public Vector2 coords;
        public Vector2 vel;
        public Vector2 heading;
        public float age;
    }

    public struct CritterUberStrokeData 
    {
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
        
        public CritterUberStrokeData(int agentIndex, float t, Vector3 bindPos, Vector2 uv, float jawMask, 
        Vector2 scale = default, Vector4 color = default)
        {
            parentIndex = agentIndex;
            brushType = 0;
            this.t = t;
            this.bindPos = bindPos;
            this.scale = scale;
            this.uv = uv;
            this.color = color;
            this.jawMask = jawMask;
            thresholdValue = Random.Range(0f, 1f);
            
            bindNormal = default;
            bindTangent = default;
            neighborAlign = default;
            neighborIndex = default;
            passiveFollow = default;
            restDistance = default;
            worldNormal = default;
            worldPos = default;
            worldTangent = default;
        }
        
        /*public CritterUberStrokeData(int agentIndex, float t, Vector3 bindPos, Vector2 uv, Vector4 color)
        {
            parentIndex = agentIndex;
            brushType = 0;
            this.t = t; //x strokesArray[anchorIndex].t;
            this.bindPos = bindPos; //x ringCenterPos + offset; 
            //newData.scale = Vector2.one * 0.15f; // new Vector2(UnityEngine.Random.Range(0.75f, 1.33f), UnityEngine.Random.Range(0.75f, 1.33f));
            this.uv = uv; //x strokesArray[anchorIndex].uv + new Vector2(ringX, ringY) * socketFractZ * gene.socketHeight * 0.1f;
            this.color = color;
            jawMask = 1f;
            thresholdValue = Random.Range(0f, 1f);
            
            scale = default;
            
            bindNormal = default;
            bindTangent = default;
            //color = default;
            neighborAlign = default;
            neighborIndex = default;
            passiveFollow = default;
            restDistance = default;
            worldNormal = default;
            worldPos = default;
            worldTangent = default;
        }*/
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

    // 2 ints, 17 floats
    public struct WaterSplineData {   
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

    // for Ripples (temp)
    public struct TrailDotData {  
        public int parentIndex;
        public Vector2 coords01;
        public float age;
        public float initAlpha;
    }
    
    // fluidSim Render -- Obstacles + ColorInjection
    public struct BasicStrokeData {  
        public Vector2 worldPos;
        public Vector2 localDir;
        public Vector2 scale;
        public Vector4 color;
    }
    
    private int debugFrameCounter = 0;

    public bool isToolbarCritterPortraitEnabled = false;
    
    // * WPP: move other startup/initialize methods here, remove any not related to initialization
    #region Initialization 
    
    private void Awake() {
        fluidObstaclesRenderCamera.enabled = false;
        fluidColorRenderCamera.enabled = false;
        spiritBrushRenderCamera.enabled = false;
        slotPortraitRenderCamera.enabled = false;
        resourceSimRenderCamera.enabled = false;
        worldTreeRenderCamera.enabled = false;
    }
    
    // Use this for initialization:
    public void InitializeRiseAndShine() 
    {
        InitializeBuffers();
        InitializeMaterials();
        //InitializeUberBrushes(); // old uber
        InitializeCommandBuffers();

        baronVonTerrain.Initialize();
        baronVonWater.Initialize();

        for (int i = 0; i < simManager.numEggSacks; i++) {
            UpdateDynamicFoodBuffers(i);
        }

        isInitialized = true;  // we did it, guys!
    }

    // Actual mix of rendering passes will change!!! 
    // primary function -- calls sub-functions for initializing each buffer
    private void InitializeBuffers() 
    {
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering            
        InitializeCurveRibbonMeshBuffer(); // Set up Curve Ribbon Mesh billboard for brushStroke rendering
        //InitializeWaterSplineMeshBuffer(); // same for water splines
        InitializeFluidRenderMesh();
        
        obstacleStrokesCBuffer = new ComputeBuffer(simManager.numAgents + simManager.numEggSacks, sizeof(float) * 10);
        obstacleStrokeDataArray = new BasicStrokeData[obstacleStrokesCBuffer.count];

        colorInjectionStrokesCBuffer = new ComputeBuffer(simManager.numAgents + simManager.numEggSacks, sizeof(float) * 10);
        colorInjectionStrokeDataArray = new BasicStrokeData[colorInjectionStrokesCBuffer.count];

        InitializeCritterUberStrokesBuffer();   // In-World        
        //InitializeCritterSkinStrokesCBuffer();  // not used    
        InitializeFloatyBitsBuffer();
        InitializeGizmos();
        InitializeSpiritBrushQuadBuffer();

        //InitializeWorldTreeBuffers(); // move to history panel
        uiManager.historyPanelUI.InitializeRenderBuffers();
        creaturePanelUI.InitializeRenderBuffers();
        clockPanelUI.InitializeClockBuffers();
    }

    public int GetNumStrokesPerCritter() {
        return numStrokesPerCritterLength * numStrokesPerCritterCross +
               numStrokesPerCritterEyes +
               numStrokesPerCritterMouth +
               numStrokesPerCritterTeeth +
               numStrokesPerCritterPectoralFins +
               numStrokesPerCritterDorsalFin +
               numStrokesPerCritterTailFin +
               numStrokesPerCritterSkinDetail; 
    }
   
    // Most of this will be populated piece-meal later as critters are generated:
    private void InitializeCritterUberStrokesBuffer() {
        int bufferLength = simManager.numAgents * GetNumStrokesPerCritter();
        mainCritterStrokesCBuffer = new ComputeBuffer(bufferLength, GetMemorySizeCritterStrokeData());
    }
    
    public int GetMemorySizeCritterStrokeData() {
        int numBytes = sizeof(int) * 3 + sizeof(float) * 32;
        return numBytes;
    }

    private int GetMemorySizeSpiritbrushQuadData() {
        return sizeof(int) * 2 + sizeof(float) * 19;
    }
    
    private int GetMemorySizeCursorParticleData() {
        return sizeof(int) * 2 + sizeof(float) * 19;
    }
    
    private void InitializeSpiritBrushQuadBuffer() {
        cursorParticlesArray = new CursorParticleData[maxShaderThreads];
        //spiritBrushQuadDataSpawnCBuffer = new ComputeBuffer(32, GetMemorySizeSpiritbrushQuadData());
        cursorParticlesCBuffer0 = new ComputeBuffer(maxShaderThreads, GetMemorySizeCursorParticleData());
        cursorParticlesCBuffer1 = new ComputeBuffer(maxShaderThreads, GetMemorySizeCursorParticleData());

        int numSpawnPoints = 3;
        Vector3[] spawnPointsArray = new Vector3[numSpawnPoints];
        
        for (int j = 0; j < numSpawnPoints; j++) {
            spawnPointsArray[j] = Random.onUnitSphere;
        }
        
        for (int i = 0; i < cursorParticlesArray.Length; i++) {
            CursorParticleData data = new CursorParticleData(spawnPointsArray, i % numSpawnPoints);
            cursorParticlesArray[i] = data;
        }
        
        cursorParticlesCBuffer0.SetData(cursorParticlesArray);

        //============================================================
        SpiritBrushQuadData[] spiritBrushQuadDataArray = new SpiritBrushQuadData[maxShaderThreads];
        //spiritBrushQuadDataSpawnCBuffer = new ComputeBuffer(32, GetMemorySizeSpiritbrushQuadData());
        spiritBrushQuadDataCBuffer0 = new ComputeBuffer(maxShaderThreads, GetMemorySizeSpiritbrushQuadData());
        spiritBrushQuadDataCBuffer1 = new ComputeBuffer(maxShaderThreads, GetMemorySizeSpiritbrushQuadData());
        
        for (int i = 0; i < spiritBrushQuadDataArray.Length; i++) {
            SpiritBrushQuadData data = new SpiritBrushQuadData(0, 256f, 200f, 256f, 0.3f);
            spiritBrushQuadDataArray[i] = data;
        }
        
        spiritBrushQuadDataCBuffer0.SetData(spiritBrushQuadDataArray);

        //spiritBrushQuadDataSpawnCBuffer.Release();
    }

    private void InitializeCurveRibbonMeshBuffer() 
    {
        float rowSize = 1f / (float)numCurveRibbonQuads;

        curveRibbonVerticesCBuffer = new ComputeBuffer(6 * numCurveRibbonQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[curveRibbonVerticesCBuffer.count];
        for (int i = 0; i < numCurveRibbonQuads; i++) 
        {
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
    
    /*
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
    }*/

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

        //int resolution = 128;

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

    private void InitializeFloatyBitsBuffer() {
        FloatyBitData[] floatyBitsInitPos = new FloatyBitData[numFloatyBits];
        floatyBitsCBuffer = new ComputeBuffer(numFloatyBits, sizeof(float) * 7);
        for (int i = 0; i < numFloatyBits; i++) {
            //floatyBitsInitPos[i] = new Vector4(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
            FloatyBitData data = new FloatyBitData();
            data.coords = Vector2.zero; // new Vector2(UnityEngine.Random.Range(0.25f, 0.35f), UnityEngine.Random.Range(0.65f, 0.75f)); // (UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
            data.vel = new Vector2(1f, 0f);
            data.heading = new Vector2(1f, 0f);
            //int numGroups = 4;
            //int randGroup = UnityEngine.Random.Range(0, numGroups);
            //float startGroupAge = (float)randGroup / (float)numGroups;
            data.age = 1.0f; // startGroupAge; // (float)i / (float)numFloatyBits;
            floatyBitsInitPos[i] = data;
        }
        
        floatyBitsCBuffer.SetData(floatyBitsInitPos);
        int kernelSimFloatyBits = fluidManager.computeShaderFluidSim.FindKernel("SimFloatyBits");
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);
    }
    
    public void InitializeGizmos() {
        Vector4[] dataArray = new Vector4[1];
        Vector4 gizmoPos = new Vector4(128f, 128f, 0f, 0f);
        dataArray[0] = gizmoPos;
        gizmoCursorPosCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        gizmoCursorPosCBuffer.SetData(dataArray);
    }
    
    #region Dead Code (please delete)
    /*
    private void InitializeTreeOfLifeBuffers() {

        testTreeOfLifePositionArray = new Vector3[64];
        testTreeOfLifePositionCBuffer = new ComputeBuffer(testTreeOfLifePositionArray.Length, sizeof(float) * 3);
        for (int i = 0; i < testTreeOfLifePositionArray.Length; i++) {
            Vector3 pos = new Vector3((float)i / 64f, 0f, 0f);
            testTreeOfLifePositionArray[i] = pos;
        }
        testTreeOfLifePositionCBuffer.SetData(testTreeOfLifePositionArray);

        treeOfLifeEventLineDataArray = new TreeOfLifeEventLineData[64];
        treeOfLifeEventLineDataCBuffer = new ComputeBuffer(treeOfLifeEventLineDataArray.Length, sizeof(float) * 2 + sizeof(int));
        for (int i = 0; i < testTreeOfLifePositionArray.Length; i++) {
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
        for (int i = 0; i < treeOfLifeNodeColliderDataArray.Length; i++) {
            TreeOfLifeNodeColliderData data = new TreeOfLifeNodeColliderData();
            data.localPos = Vector3.zero;
            data.scale = Vector3.one;
            treeOfLifeNodeColliderDataArray[i] = data;
        }
        treeOfLifeNodeColliderDataCBufferA.SetData(treeOfLifeNodeColliderDataArray);
        treeOfLifeNodeColliderDataCBufferB.SetData(treeOfLifeNodeColliderDataArray);

        treeOfLifeLeafNodeDataArray = new TreeOfLifeLeafNodeData[maxNumTreeOfLifeNodes];
        treeOfLifeLeafNodeDataCBuffer = new ComputeBuffer(treeOfLifeLeafNodeDataArray.Length, sizeof(int) * 3 + sizeof(float) * 14);
        for (int i = 0; i < treeOfLifeLeafNodeDataArray.Length; i++) {
            TreeOfLifeLeafNodeData data = new TreeOfLifeLeafNodeData(i);
            
            treeOfLifeLeafNodeDataArray[i] = data;
        }
        treeOfLifeLeafNodeDataCBuffer.SetData(treeOfLifeLeafNodeDataArray);

        // Stem Segments:
        // QuadVertices:        
        float rowSize = 1f / (float)numTreeOfLifeStemSegmentQuads;
        treeOfLifeStemSegmentVerticesCBuffer = new ComputeBuffer(6 * numTreeOfLifeStemSegmentQuads, sizeof(float) * 3);
        Vector3[] verticesArray = new Vector3[treeOfLifeStemSegmentVerticesCBuffer.count];
        for (int i = 0; i < numTreeOfLifeStemSegmentQuads; i++) {
            int baseIndex = i * 6;

            float startCoord = (float)i;
            float endCoord = (float)(i + 1);   // **** WHY WAS THERE A WINDING ORDER PROBLEM???? *****
            verticesArray[baseIndex + 0] = new Vector3(-0.5f, endCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 1] = new Vector3(0.5f, endCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 2] = new Vector3(0.5f, startCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 3] = new Vector3(0.5f, startCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 4] = new Vector3(-0.5f, startCoord * rowSize - 0.5f);
            verticesArray[baseIndex + 5] = new Vector3(-0.5f, endCoord * rowSize - 0.5f);


        }
        treeOfLifeStemSegmentVerticesCBuffer.SetData(verticesArray);
        
        // actual segments buffer:
        TreeOfLifeStemSegmentStruct[] treeOfLifeStemSegmentDataArray = new TreeOfLifeStemSegmentStruct[maxNumTreeOfLifeSegments];
        treeOfLifeStemSegmentDataCBuffer = new ComputeBuffer(maxNumTreeOfLifeSegments, sizeof(int) * 3 + sizeof(float) * 1);
        for (int i = 0; i < treeOfLifeStemSegmentDataArray.Length; i++) {
            TreeOfLifeStemSegmentStruct newStruct = new TreeOfLifeStemSegmentStruct(0, 0, 0);
            //newStruct.speciesID = 0;
            //newStruct.fromID = 0;
            //newStruct.toID = 0;
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
        
        // Portrait:
        // Update single-element buffer on-demand through UI -- repopulate from given genome
        CritterSkinStrokeData[] treeOfLifePortraitDataArray = new CritterSkinStrokeData[1 * numStrokesPerCritterSkin];
        treeOflifePortraitDataCBuffer = new ComputeBuffer(treeOfLifePortraitDataArray.Length, sizeof(float) * 16 + sizeof(int) * 2);
        for (int j = 0; j < treeOfLifePortraitDataArray.Length; j++) {
            CritterSkinStrokeData skinStroke = new CritterSkinStrokeData();
            skinStroke.parentIndex = 0;
            skinStroke.brushType = 0; // ** Revisit

            skinStroke.worldPos = new Vector3(SimulationManager._MapSize / 2f, SimulationManager._MapSize / 2f, 0f);

            float zCoord = (1f - ((float)j / (float)(numStrokesPerCritterSkin - 1))) * 2f - 1f;
            float radiusAtZ = Mathf.Sqrt(1f - zCoord * zCoord); // pythagorean theorem
            Vector2 xyCoords = Random.insideUnitCircle.normalized * radiusAtZ; // possibility for (0,0) ??? ***** undefined/null divide by zero hazard!
            skinStroke.localPos = new Vector3(xyCoords.x, xyCoords.y, zCoord);
            //float width = 1f; // agents[i].agentWidthsArray[Mathf.RoundToInt((skinStroke.localPos.y * 0.5f + 0.5f) * 15f)];
            skinStroke.localPos.x *= 0.5f;
            skinStroke.localPos.z *= 0.5f;
            skinStroke.localDir = new Vector3(0f, 1f, 0f); // start up? shouldn't matter
            skinStroke.localScale = new Vector2(0.25f, 0.420f) * 1.25f;
            skinStroke.strength = Random.Range(0f, 1f);
            skinStroke.lifeStatus = 0f;
            skinStroke.age = Random.Range(1f, 2f);
            skinStroke.randomSeed = Random.Range(0f, 1f);
            skinStroke.followLerp = 1f;

            treeOfLifePortraitDataArray[j] = skinStroke;
        }
        treeOflifePortraitDataCBuffer.SetData(treeOfLifePortraitDataArray);
    }
    */
    #endregion
    
    private void InitializeMaterials() 
    {
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatyBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        floatyBitsShadowDisplayMat.SetPass(0);
        floatyBitsShadowDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatyBitsShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        debugAgentResourcesMat.SetPass(0);
        debugAgentResourcesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        algaeParticleDisplayMat.SetPass(0);
        algaeParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        plantParticleDisplayMat.SetPass(0);
        plantParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        plantParticleShadowDisplayMat.SetPass(0);
        plantParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        animalParticleDisplayMat.SetPass(0);
        animalParticleDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        animalParticleShadowDisplayMat.SetPass(0);
        animalParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        eggCoverDisplayMat.SetPass(0);
        eggCoverDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        eggSackShadowDisplayMat.SetPass(0);
        eggSackShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        algaeParticleColorInjectMat.SetPass(0);
        algaeParticleColorInjectMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        gizmoStirToolMat.SetPass(0);
        gizmoStirToolMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        gizmoFeedToolMat.SetPass(0);
        gizmoFeedToolMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        //treeOfLifeLeafNodesMat.SetPass(0);
        //treeOfLifeLeafNodesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        //treeOfLifeStemSegmentsMat.SetPass(0);
        //treeOfLifeStemSegmentsMat.SetBuffer("treeOfLifeStemSegmentVerticesCBuffer", treeOfLifeStemSegmentVerticesCBuffer);

        treeOfLifeBackdropMat.SetPass(0);
        treeOfLifeBackdropMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifeBackdropPortraitBorderMat.SetPass(0);
        treeOfLifeBackdropPortraitBorderMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        treeOfLifePortraitMat.SetPass(0);
        treeOfLifePortraitMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        worldTreeLineDataMat.SetPass(0);
        worldTreeLineDataMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
    }
    
    private void InitializeCommandBuffers() 
    {
        cmdBufferMain = new CommandBuffer();
        cmdBufferMain.name = "cmdBufferMain";
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMain);

        cmdBufferDebugVis = new CommandBuffer();
        cmdBufferDebugVis.name = "cmdBufferDebugVis";

        cmdBufferFluidObstacles = new CommandBuffer();
        cmdBufferFluidObstacles.name = "cmdBufferFluidObstacles";
        fluidObstaclesRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferFluidObstacles);

        cmdBufferFluidColor = new CommandBuffer();
        cmdBufferFluidColor.name = "cmdBufferFluidColor";
        fluidColorRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferFluidColor);

        spiritBrushRT = new RenderTexture(spiritBrushResolution, spiritBrushResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        spiritBrushRT.wrapMode = TextureWrapMode.Clamp;
        spiritBrushRT.enableRandomWrite = true;
        spiritBrushRT.Create();

        cmdBufferSpiritBrush = new CommandBuffer();
        cmdBufferSpiritBrush.name = "cmdBufferSpiritBrush";
        spiritBrushRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferSpiritBrush);

        cmdBufferSlotPortraitDisplay = new CommandBuffer();
        cmdBufferSlotPortraitDisplay.name = "cmdBufferSpeciesPortraitDisplay";
        slotPortraitRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferSlotPortraitDisplay);

        cmdBufferResourceSim = new CommandBuffer();
        cmdBufferResourceSim.name = "cmdBufferResourceSim";
        resourceSimRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferResourceSim);

        cmdBufferWorldTree = new CommandBuffer();
        cmdBufferWorldTree.name = "cmdBufferWorldTree";
        worldTreeRenderCamera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, cmdBufferWorldTree);
    }
    
    #endregion

    public Vector4[] GetDepthAtObjectPositions(Vector4[] positionsArray) 
    {
        ComputeBuffer objectDataInFluidCoordsCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 4);
        ComputeBuffer depthValuesCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 4);

        Vector4[] objectDepthsArray = new Vector4[positionsArray.Length];

        objectDataInFluidCoordsCBuffer.SetData(positionsArray);

        baronVonTerrain.SetObjectDepths(objectDataInFluidCoordsCBuffer, depthValuesCBuffer);

        // *******
        // only returning x channel data currently!!!! **** Need to move depthMapGeneration to terrainCompute and pre-calculate gradients there
        depthValuesCBuffer.GetData(objectDepthsArray);

        depthValuesCBuffer.Release();
        objectDataInFluidCoordsCBuffer.Release();

        //Debug.Log("Depth at 0: " + objectDepthsArray[0].ToString());
        return objectDepthsArray;
    }

    private void PopulateObstaclesBuffer() 
    {
        PopulateAgentObstaclesBuffer(0);
        //PopulateFoodObstaclesBuffer(agents.Length);
        //PopulatePredatorObstaclesBuffer(agents.Length + simManager.eggSacks.Length);
        obstacleStrokesCBuffer.SetData(obstacleStrokeDataArray);
    }
    
    private void PopulateAgentObstaclesBuffer(int baseIndex)
    {
        for (int i = 0; i < agents.Length; i++) 
        {
            Vector3 agentPos = agents[i].bodyRigidbody.transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(agentPos.x, agentPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = agents[i].facingDirection;
            float deadMult = 1f;
            
            if (agents[i].isDead) {
                deadMult = 0f;
            }
            
            // Vector2.one * 5.5f * agentsArray[i].sizePercentage; // new Vector2(agentsArray[i].transform.localScale.x, agents[i].transform.localScale.y) * 2.9f; // ** revisit this later // should leave room for velSampling around Agent *** weird popping when * 0.9f
            obstacleStrokeDataArray[baseIndex + i].scale = new Vector2(agents[i].currentBoundingBoxSize.x, agents[i].currentBoundingBoxSize.y) * deadMult; 

            float velX = Mathf.Clamp(agents[i].ownVel.x, -100f, 100f) * velScale * 0.01f; // agentPos.x - agents[i]._PrevPos.x * velScale;
            float velY = Mathf.Clamp(agents[i].ownVel.y, -100f, 100f) * velScale * 0.01f;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }
    }
    
    #region Future Use (or delete)
    private void PopulateFoodObstaclesBuffer(int baseIndex)
    {
        for(int i = 0; i < simManager.eggSacks.Length; i++) 
        {
            Vector3 foodPos = simManager.eggSacks[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(foodPos.x, foodPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = simManager.eggSacks[i].facingDirection;
            obstacleStrokeDataArray[baseIndex + i].scale = simManager.eggSacks[i].curSize * 0.95f;

            float velX = (foodPos.x - simManager.eggSacks[i].prevPos.x) * velScale;
            float velY = (foodPos.y - simManager.eggSacks[i].prevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }
    }
    
    private void PopulatePredatorObstaclesBuffer(int baseIndex)
    {
        /*for(int i = 0; i < simManager.predatorArray.Length; i++) 
        {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            obstacleStrokeDataArray[baseIndex + i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            obstacleStrokeDataArray[baseIndex + i].localDir = new Vector2(Mathf.Cos(simManager.predatorArray[i].transform.rotation.z), Mathf.Sin(simManager.predatorArray[i].transform.rotation.z));
            obstacleStrokeDataArray[baseIndex + i].scale = new Vector2(simManager.predatorArray[i].curScale, simManager.predatorArray[i].curScale) * 0.95f;

            float velX = (predatorPos.x - simManager.predatorArray[i]._PrevPos.x) * velScale;
            float velY = (predatorPos.y - simManager.predatorArray[i]._PrevPos.y) * velScale;

            obstacleStrokeDataArray[baseIndex + i].color = new Vector4(velX, velY, 1f, 1f);
        }*/
    }
    #endregion

    private void PopulateColorInjectionBuffer() 
    {
        PopulateAgentColorInjectionBuffer(0);
        PopulateFoodColorInjectionBuffer(0);
        colorInjectionStrokesCBuffer.SetData(colorInjectionStrokeDataArray);
    }
    
    void PopulateAgentColorInjectionBuffer(int baseIndex)
    {
        for (int i = 0; i < agents.Length; i++) 
        {
            Vector3 agentPos = agents[i].bodyRigidbody.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(agentPos.x, agentPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = agents[i].facingDirection;
            colorInjectionStrokeDataArray[baseIndex + i].scale = Vector2.one * 1.5f; 
            // agents[i].fullSizeBoundingBox * 1.55f; // * agents[i].sizePercentage;
            /*
            float agentAlpha = 1f;
            if(agents[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                agentAlpha = 2.2f / agents[i].fullSizeBoundingBox.magnitude;
            }
            if(agents[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                agentAlpha = 3f * agents[i].GetDecayPercentage();
            }*/

            Vector4 hue = Vector4.one * 0.695f;
            if (agents[i].candidateRef != null) {
                if (agents[i].candidateRef.speciesID == focusedCandidate.speciesID) {
                    hue = new Vector4(1f, 1f, 0f, 1f);
                    colorInjectionStrokeDataArray[baseIndex + i].scale = Vector2.one * 5f;
                }
                if (agents[i].candidateRef.candidateID == focusedCandidate.candidateID) {
                    hue = new Vector4(1f, 1f, 1f, 1f);
                    colorInjectionStrokeDataArray[baseIndex + i].scale = Vector2.one * 10f;
                }
            }

            //Color drawColor = new Color(hue.x, hue.y, hue.z);
            colorInjectionStrokeDataArray[baseIndex + i].color = hue;
        }
    }
    
    void PopulateFoodColorInjectionBuffer(int baseIndex)
    {
        baseIndex = agents.Length;
        for (int i = 0; i < simManager.eggSacks.Length; i++) {
            Vector3 foodPos = simManager.eggSacks[i].transform.position;
            colorInjectionStrokeDataArray[baseIndex + i].worldPos = new Vector2(foodPos.x, foodPos.y);
            colorInjectionStrokeDataArray[baseIndex + i].localDir = simManager.eggSacks[i].facingDirection;
            colorInjectionStrokeDataArray[baseIndex + i].scale = simManager.eggSacks[i].curSize * 1.0f;

            float foodAlpha = 0.06f;
            if (simManager.eggSacks[i].isBeingEaten > 0.5) {
                foodAlpha = 1.2f;
            }

            colorInjectionStrokeDataArray[baseIndex + i].color = 
                new Vector4(Mathf.Lerp(simManager.eggSackGenomes[i].fruitHue.x, 0.1f, 0.7f), 
                    Mathf.Lerp(simManager.eggSackGenomes[i].fruitHue.y, 0.9f, 0.7f), 
                    Mathf.Lerp(simManager.eggSackGenomes[i].fruitHue.z, 0.2f, 0.7f), foodAlpha);
        }
    }

    public void UpdateDynamicFoodBuffers(int eggSackIndex) {
        // *** Hard-coded 64 Fruits per food object!!!! *** BEWARE!!!
        ComputeBuffer eggsUpdateCBuffer = new ComputeBuffer(simManager.numEggSacks, sizeof(float) * 8 + sizeof(int) * 1);

        EggData[] eggDataArray = new EggData[simManager.numEggSacks];
        for (int i = 0; i < eggDataArray.Length; i++) {
            eggDataArray[i] = new EggData(eggSackIndex, 0.25f, simManager.eggSacks[eggSackIndex].transform.position);
            /*eggDataArray[i].eggSackIndex = eggSackIndex;
            eggDataArray[i].localCoords = Random.insideUnitCircle; // new Vector2(randSphere.x, randSphere.y); // * 0.5f + UnityEngine.Random.insideUnitCircle * 0.4f;
            eggDataArray[i].localScale = Vector2.one * 0.25f; // simManager.eggSackGenomePoolArray[eggSackIndex].fruitScale;  
            eggDataArray[i].worldPos = simManager.eggSacks[eggSackIndex].transform.position;
            eggDataArray[i].attached = 1f;*/
        }
        
        eggsUpdateCBuffer.SetData(eggDataArray);
        int kernelCSUpdateDynamicEggBuffers = computeShaderEggSacks.FindKernel("CSUpdateDynamicEggBuffers");
        //computeShaderBrushStrokes.SetInt("_CurveStrokesUpdateAgentIndex", agentIndex); // ** can I just use parentIndex instead?
        computeShaderEggSacks.SetBuffer(kernelCSUpdateDynamicEggBuffers, "eggSackSimDataCBuffer", simStateData.eggSackSimDataCBuffer);
        computeShaderEggSacks.SetBuffer(kernelCSUpdateDynamicEggBuffers, "eggDataUpdateCBuffer", eggsUpdateCBuffer);
        computeShaderEggSacks.SetBuffer(kernelCSUpdateDynamicEggBuffers, "eggDataWriteCBuffer", simStateData.eggDataCBuffer);
        computeShaderEggSacks.Dispatch(kernelCSUpdateDynamicEggBuffers, 1, 1, 1);
        eggsUpdateCBuffer.Release();
    }
    
    #region Agent Initialization

    public void UpdateCritterGenericStrokesData(Agent agent) { 
        ComputeBuffer singleCritterGenericStrokesCBuffer = new ComputeBuffer(GetNumStrokesPerCritter(), GetMemorySizeCritterStrokeData());
        CritterUberStrokeData[] singleCritterGenericStrokesArray = new CritterUberStrokeData[singleCritterGenericStrokesCBuffer.count];  // optimize this later?? ***
        CritterGenomeInterpretor.BrushPoint[] brushPointArray = new CritterGenomeInterpretor.BrushPoint[GetNumStrokesPerCritter()];

        // Generate main body strokes:
        GenerateCritterBodyBrushstrokes(ref singleCritterGenericStrokesArray, brushPointArray, agent.candidateRef.candidateGenome, agent.index);

        // Loop through all points again and calculate normals/tangents/other things:
        CalculateCritterBodyBrushstrokesNormals(ref singleCritterGenericStrokesArray, brushPointArray);

        // Create Eye points here:
        GenerateCritterEyeBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);

        // Mouth
        GenerateCritterMouthTeethBrushstrokes(ref singleCritterGenericStrokesArray, agent.candidateRef.candidateGenome, agent.index);
        // Teeth

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
        singleCritterGenericStrokesCBuffer.SetData(singleCritterGenericStrokesArray); // Send data to GPU
        int kernelCSUpdateCritterGenericStrokes = computeShaderCritters.FindKernel("CSUpdateCritterGenericStrokes");
        computeShaderCritters.SetBuffer(kernelCSUpdateCritterGenericStrokes, "critterGenericStrokesWriteCBuffer", mainCritterStrokesCBuffer);
        computeShaderCritters.SetBuffer(kernelCSUpdateCritterGenericStrokes, "critterGenericStrokesUpdateCBuffer", singleCritterGenericStrokesCBuffer);
        computeShaderCritters.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        computeShaderCritters.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        computeShaderCritters.SetInt("_UpdateBufferStartIndex", agent.index * singleCritterGenericStrokesCBuffer.count);
        computeShaderCritters.Dispatch(kernelCSUpdateCritterGenericStrokes, singleCritterGenericStrokesCBuffer.count, 1, 1);
        singleCritterGenericStrokesCBuffer.Release();
    }
    
    public void GenerateCritterPortraitStrokesData(AgentGenome genome) 
    {
        // Get genomes:
        //AgentGenome genome0 = genome; // simManager.masterGenomePool.completeSpeciesPoolsList[0].representativeGenome;

        //ComputeBuffer singleCritterGenericStrokesCBuffer = new ComputeBuffer(GetNumUberStrokesPerCritter(), GetMemorySizeCritterUberStrokeData());
        CritterUberStrokeData[] singleCritterGenericStrokesArray = new CritterUberStrokeData[GetNumStrokesPerCritter()];  // optimize this later?? ***
        //CritterUberStrokeData[] newCritterGenericStrokesArray = new CritterUberStrokeData[toolbarCritterPortraitStrokesCBuffer.count / 2]; 
        CritterUberStrokeData[] completeCritterGenericStrokesArray = new CritterUberStrokeData[creaturePanelUI.critterPortraitStrokesCBuffer.count];
        CritterGenomeInterpretor.BrushPoint[] brushPointArray = new CritterGenomeInterpretor.BrushPoint[GetNumStrokesPerCritter()];
        //CritterGenomeInterpretor.BrushPoint[] newPointArray = new CritterGenomeInterpretor.BrushPoint[toolbarCritterPortraitStrokesCBuffer.count / 2];   
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
        SortCritterBrushstrokes(ref singleCritterGenericStrokesArray, 0);
        // Copy over into master array:
        for (int i = 0; i < singleCritterGenericStrokesArray.Length; i++) {
            completeCritterGenericStrokesArray[i] = singleCritterGenericStrokesArray[i];
        }

        creaturePanelUI.critterPortraitStrokesCBuffer.SetData(completeCritterGenericStrokesArray);
    }

    private void GenerateCritterBodyBrushstrokes(ref CritterUberStrokeData[] strokesArray, CritterGenomeInterpretor.BrushPoint[] brushPointArray, AgentGenome agentGenome, int agentIndex) {
        // Loop through all brush points, starting at the tip of the tail (underside), (x=0, y=0, z=1)
        // ... Then working its way to tip of head by doing series of cross-section rings
        for (int y = 0; y < numStrokesPerCritterLength; y++) {

            float yLerp = Mathf.Clamp01((float)y / (float)(numStrokesPerCritterLength - 1)); // start at tail (Y = 0)

            for (int a = 0; a < numStrokesPerCritterCross; a++) {

                int brushIndex = y * numStrokesPerCritterCross + a;
                float angleRad = ((float)a / (float)numStrokesPerCritterCross) * Mathf.PI * 2f; // verticalLerpPos * Mathf.PI;   
                float crossSectionCoordX = Mathf.Sin(angleRad);
                float crossSectionCoordZ = Mathf.Cos(angleRad);
                //Vector2 crossSectionNormalizedCoords = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * 1f;

                CritterGenomeInterpretor.BrushPoint newBrushPoint = new CritterGenomeInterpretor.BrushPoint();
                newBrushPoint.initCoordsNormalized = new Vector3(crossSectionCoordX, yLerp, crossSectionCoordZ);
                newBrushPoint.uv = new Vector2((float)a / (float)numStrokesPerCritterCross, (float)y / (float)numStrokesPerCritterLength);
                newBrushPoint.ix = a;
                newBrushPoint.iy = y;
                newBrushPoint = CritterGenomeInterpretor.ProcessBrushPoint(newBrushPoint, agentGenome);
                brushPointArray[brushIndex] = newBrushPoint;

                var jawMask = crossSectionCoordZ >= 0 ? -1f : 1f;
                CritterUberStrokeData newData = new CritterUberStrokeData(agentIndex, yLerp, newBrushPoint.bindPos, newBrushPoint.uv, jawMask, Vector2.one);

                strokesArray[brushIndex] = newData;
            }
        }
    }
    
    private void CalculateCritterBodyBrushstrokesNormals(ref CritterUberStrokeData[] strokesArray, CritterGenomeInterpretor.BrushPoint[] brushPointArray) 
    {
        for (int y = 0; y < numStrokesPerCritterLength; y++) 
        {
            for (int a = 0; a < numStrokesPerCritterCross; a++) 
            {
                // line from head to tail at same altitude:
                int indexCenter = y * numStrokesPerCritterCross + a;

                // find neighbor positions: (all in bindPos object coordinates)
                int indexNegX = y * numStrokesPerCritterCross + Mathf.Clamp(a - 1, 0, numStrokesPerCritterCross - 1); // switch to modulo arithmetic for wrapping!
                int indexPosX = y * numStrokesPerCritterCross + Mathf.Clamp(a + 1, 0, numStrokesPerCritterCross - 1);
                int indexNegY = Mathf.Clamp(y - 1, 0, numStrokesPerCritterLength - 1) * numStrokesPerCritterCross + a;
                int indexPosY = Mathf.Clamp(y + 1, 0, numStrokesPerCritterLength - 1) * numStrokesPerCritterCross + a;

                Vector3 uTangentAvg = brushPointArray[indexPosX].bindPos - brushPointArray[indexNegX].bindPos;
                Vector3 vTangentAvg = brushPointArray[indexPosY].bindPos - brushPointArray[indexNegY].bindPos;

                Vector2 scale;
                Vector3 normal;
                Vector3 tangent;

                // tailtip
                if (y == 0) {  
                    scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude) * 0.5f;
                    normal = new Vector3(0f, -1f, 0f);
                    tangent = new Vector3(0f, 0f, 1f);
                }
                // headTip
                else if (y == numStrokesPerCritterLength - 1) {  
                    scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude) * 0.5f;
                    normal = new Vector3(0f, 1f, 0f);
                    tangent = new Vector3(0f, 0f, 1f);
                }
                // body
                else {  
                    scale = new Vector2(uTangentAvg.magnitude, vTangentAvg.magnitude);
                    normal = Vector3.Cross(uTangentAvg, vTangentAvg).normalized;
                    tangent = vTangentAvg.normalized;
                }

                //Vector3 bitangent = Vector3.Cross(normal, tangent);

                float offsetShiftX = (float)(y % 2) * 0.25f;
                float randShift = Random.Range(-1f, 1f) * 0.05f;
                strokesArray[indexCenter].bindPos += uTangentAvg * offsetShiftX - vTangentAvg * randShift;
                //brushPointArray[indexCenter].normal = normal; // not needed?>
                Vector2 randScale = new Vector2(Random.Range(0.75f, 1.33f), Random.Range(0.75f, 1.33f));
                strokesArray[indexCenter].scale = new Vector2(scale.x * randScale.x, scale.y * randScale.y);
                strokesArray[indexCenter].bindNormal = (normal + Random.insideUnitSphere * 0.165f).normalized;
                strokesArray[indexCenter].bindTangent = tangent;

                strokesArray[indexCenter].neighborIndex = indexNegY;
                strokesArray[indexCenter].neighborAlign = (1f - strokesArray[indexCenter].t);
            }
        }
    }
    
    private void GenerateCritterEyeBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) 
    {
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

        for (int eyeIndex = 0; eyeIndex < numEyes; eyeIndex++) 
        {
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

            // * WPP: define purpose of this code block and delegate to method
            for (int z = 0; z < totalLengthResolution; z++) {
                //float zFract = (float)z / (float)totalLengthResolution;
                float socketFractZ = Mathf.Clamp01((float)z / (float)(socketLengthResolution - 1));
                float eyeballFractZ = Mathf.Clamp01((float)(z - socketLengthResolution) / (float)(eyeballLengthResolution));

                float socketBulgeMultiplier = Mathf.Sin(socketFractZ * Mathf.PI) * socketBulge + 1f;
                //float eyeballBulgeMultiplier
                //radius *= Mathf.Cos(eyeballFractZ * Mathf.PI * 0.5f);

                float radius = z < socketLengthResolution ?
                    Mathf.Lerp(socketRadius, eyeballRadius, socketFractZ) * socketBulgeMultiplier :
                    Mathf.Cos(eyeballFractZ * Mathf.PI * 0.5f) * eyeballRadius;

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

                    // Is Part of Eyeball!
                    if (z > socketLengthResolution) 
                    {  
                        color = new Vector4(gene.eyeballHue.x, gene.eyeballHue.y, gene.eyeballHue.z, 1f);
                        
                        // Is part of IRIS  
                        if (radius / eyeballRadius < irisWidthFraction) 
                        {                        
                            color = new Vector4(gene.irisHue.x, gene.irisHue.y, gene.irisHue.z, 1f);
                            
                            // PUPIL
                            if (radius / eyeballRadius < pupilWidthFraction && radius / eyeballRadius < pupilHeightFraction) 
                            {                          
                                color = new Vector4(0f, 0f, 0f, 1f);
                            }
                        }
                    }

                    var uv = strokesArray[anchorIndex].uv + new Vector2(ringX, ringY) * socketFractZ * gene.socketHeight * 0.1f;
                    CritterUberStrokeData newData = new CritterUberStrokeData(agentIndex, strokesArray[anchorIndex].t, 
                        ringCenterPos + offset, uv, 1f, default, color);
                        
                    strokesArray[eyeBrushPointIndex] = newData;
                }
            }

            // CALCULATE NORMALS EMPIRICALLY:
            for (int z = 0; z < totalLengthResolution; z++) 
            {
                for (int a = 0; a < baseCrossResolution; a++) 
                {
                    //int indexCenter = arrayIndexStart + eyeIndex * (totalLengthResolution * baseCrossResolution) + z * baseCrossResolution + a;

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
        //float lipThickness = 0.05f;  // how to scale with creature size?
        // maybe better to interpolate between existing body strokes? measure size by single row?

        // JUST TEETH FOR NOW::::

        // RIGHT side:
        for (int y = 0; y < numStrokesPerRowSide; y++) 
        {
            float yLerp = Mathf.Lerp(startCoordY, 1f, Mathf.Clamp01((float)y / (float)(numStrokesPerRowSide - 1))); // start at tail (Y = 0)            
            int brushIndexTop = arrayIndexStart + y;
            int brushIndexBottom = arrayIndexStart + numStrokesPerRowSide * 2 + y;

            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(1f, yLerp, 0f), new Vector2(0.25f, yLerp), brushIndexTop);
            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(1f, yLerp, 0f), new Vector2(0.25f, yLerp), brushIndexBottom);

            if (y != 0) 
            {
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
        for (int y = 0; y < numStrokesPerRowSide; y++) 
        {
            float yLerp = Mathf.Lerp(startCoordY, 1f, Mathf.Clamp01((float)y / (float)(numStrokesPerRowSide - 1))); // start at tail (Y = 0)            
            int brushIndexTop = arrayIndexStart + numStrokesPerRowSide + y;
            int brushIndexBottom = arrayIndexStart + numStrokesPerRowSide * 3 + y;

            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(-1f, yLerp, 0f), new Vector2(0.75f, yLerp), brushIndexTop);
            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(-1f, yLerp, 0f), new Vector2(0.75f, yLerp), brushIndexBottom);

            if (y != 0) 
            {
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

        CritterUberStrokeData newData = new CritterUberStrokeData(agentIndex, initCoords.y, 
            newBrushPoint.bindPos, newBrushPoint.uv, default, Vector2.one);

        strokesArray[brushIndex] = newData;
    }
    
    // Future use
    private void GenerateCritterPectoralFinsBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {

    }
    
    private void GenerateCritterDorsalFinBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {
        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross + numStrokesPerCritterEyes + numStrokesPerCritterMouth + numStrokesPerCritterTeeth + numStrokesPerCritterPectoralFins;

        CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        //float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;

        // genome parameters:
        float startCoordY = gene.dorsalFinStartCoordY; // (gene.tailLength * 0.5f) / segmentsSummedCritterLength;
        float endCoordY = gene.dorsalFinEndCoordY;  // (gene.headLength * 0.5f + gene.bodyLength + gene.tailLength) / segmentsSummedCritterLength;
        float slantAmount = gene.dorsalFinSlantAmount;
        float baseHeight = gene.dorsalFinBaseHeight;
        
        int numRows = 4;
        int pointsPerRow = numStrokesPerCritterDorsalFin / numRows / 2;  // 2 one facing each side
        
        // Initial position:
        for (int i = 0; i < numRows; i++) 
        {
            for (int y = 1; y < pointsPerRow; y++) 
            {
                float yLerp = Mathf.Lerp(startCoordY, endCoordY, Mathf.Clamp01((float)y / (float)(pointsPerRow - 1))); // start at tail (Y = 0)            
                int brushIndexLeft = arrayIndexStart + pointsPerRow * i + y;
                int brushIndexRight = arrayIndexStart + pointsPerRow * i + y + (numStrokesPerCritterDorsalFin / 2);

                AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(0f, yLerp, -1f), new Vector2(0.5f, yLerp), brushIndexLeft);
                AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(0f, yLerp, -1f), new Vector2(0.5f, yLerp), brushIndexRight);
                
                Vector3 uTangentAvg = (strokesArray[brushIndexLeft].bindPos - strokesArray[brushIndexLeft - 1].bindPos);
                //Vector3 vTangentAvg = new Vector3(0f, 0f, -1f);
                strokesArray[brushIndexLeft].scale = new Vector2(uTangentAvg.magnitude, 0.125f) * baseHeight; // vTangentAvg.magnitude);
                strokesArray[brushIndexLeft].bindNormal = (new Vector3(1f, 0f, -0.5f) + Random.insideUnitSphere * 0.3f).normalized;
                strokesArray[brushIndexLeft].bindTangent = new Vector3(0f, -slantAmount, -1f);
                //strokesArray[brushIndexLeft].color = Vector4.one;
                strokesArray[brushIndexLeft].jawMask = 1f;
                strokesArray[brushIndexLeft].neighborIndex = arrayIndexStart + pointsPerRow * i + Mathf.Max(0, (y - 1));
                strokesArray[brushIndexLeft].neighborAlign = 1f;

                strokesArray[brushIndexRight].scale = strokesArray[brushIndexLeft].scale; // vTangentAvg.magnitude);
                strokesArray[brushIndexRight].bindNormal = (new Vector3(-1f, 0f, -0.5f) + Random.insideUnitSphere * 0.3f).normalized;
                strokesArray[brushIndexRight].bindTangent = new Vector3(0f, -slantAmount, -1f);
                //strokesArray[brushIndexRight].color = Vector4.one;
                strokesArray[brushIndexRight].jawMask = 1f;
                strokesArray[brushIndexRight].neighborIndex = arrayIndexStart + pointsPerRow * i + Mathf.Max(0, (y - 1)) + (numStrokesPerCritterDorsalFin / 2);
                strokesArray[brushIndexRight].neighborAlign = 1f;
            }
        }

        // Offset placement from anchor pos:
        for (int i = 0; i < numRows; i++) 
        {
            // top:
            for (int y = 1; y < pointsPerRow; y++) 
            {
                //float yLerp = Mathf.Lerp(startCoordY, endCoordY, Mathf.Clamp01((float)y / (float)(pointsPerRow - 1))); // start at tail (Y = 0)            
                int brushIndexLeft = arrayIndexStart + pointsPerRow * i + y;
                int brushIndexRight = arrayIndexStart + pointsPerRow * i + (numStrokesPerCritterDorsalFin / 2) + y;
                
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

        for (int y = 0; y < numColumns; y++) 
        {
            // Radial fan lines:
            float angleRad = (-spreadAngle / 2f + angleInc * y) * Mathf.PI;
            Vector2 tangent = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            
            // Arcs:
            for (int x = 0; x < numRows; x++) 
            {
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
                                      
                SetStrokeData(ref strokesArray[brushIndexLeft], x, y, tangent, numRows, arrayIndexStart, lengthMult, functionMult, tMult);
                SetStrokeData(ref strokesArray[brushIndexRight], x, y, tangent, numRows, arrayIndexStart, lengthMult, functionMult, tMult, strokesArray[brushIndexLeft].scale);
                strokesArray[brushIndexRight].neighborIndex += numStrokesPerCritterTailFin / 2;
                // WPP: merged similar code blocks
                /*strokesArray[brushIndexLeft].scale = new Vector2(0.05f * (float)x, lengthMult * functionMult);
                strokesArray[brushIndexLeft].bindNormal = (new Vector3(-1f, 0f, 0f) + Random.insideUnitSphere * 0.3f).normalized;
                strokesArray[brushIndexLeft].bindTangent = new Vector3(0f, -tangent.x, tangent.y);
                strokesArray[brushIndexLeft].bindPos += strokesArray[brushIndexLeft].bindTangent * strokesArray[brushIndexLeft].scale.y * (float)x;
                strokesArray[brushIndexLeft].jawMask = 1f;
                strokesArray[brushIndexLeft].scale.y *= 1.75f;
                strokesArray[brushIndexLeft].t = 0.05f - tangent.x * strokesArray[brushIndexLeft].scale.y * (float)x / tMult;
                strokesArray[brushIndexLeft].neighborIndex = arrayIndexStart + numRows * y + Mathf.Max(0, (x - 1));
                strokesArray[brushIndexLeft].neighborAlign = 1f;
                strokesArray[brushIndexLeft].passiveFollow = Mathf.Lerp(0.3f, 0.7f, (float)x / (float)(numRows - 1));

                strokesArray[brushIndexRight].scale = strokesArray[brushIndexLeft].scale;
                strokesArray[brushIndexRight].bindNormal = (new Vector3(1f, 0f, 0f) + Random.insideUnitSphere * 0.3f).normalized;
                strokesArray[brushIndexRight].bindTangent = new Vector3(0f, -tangent.x, tangent.y);
                strokesArray[brushIndexRight].bindPos += strokesArray[brushIndexRight].bindTangent * strokesArray[brushIndexRight].scale.y * (float)x;
                strokesArray[brushIndexRight].jawMask = 1f;
                strokesArray[brushIndexRight].scale.y *= 1.75f;
                strokesArray[brushIndexRight].t = 0.05f - tangent.x * strokesArray[brushIndexLeft].scale.y * (float)x / tMult;
                strokesArray[brushIndexRight].neighborIndex = arrayIndexStart + numRows * y + Mathf.Max(0, (x - 1)) + (numStrokesPerCritterTailFin / 2);
                strokesArray[brushIndexRight].neighborAlign = 1f;
                strokesArray[brushIndexRight].passiveFollow = Mathf.Lerp(0.3f, 0.7f, (float)x / (float)(numRows - 1));*/
            }
        }
    }

    private void SetStrokeData(ref CritterUberStrokeData strokeData, int x, int y, Vector2 tangent, int rowCount, int startIndex,
    float lengthMultiplier, float functionMultiplier, float tMultiplier, Vector2 overrideScale = default)
    {
        strokeData.scale = overrideScale == Vector2.zero ? new Vector2(0.05f * x, lengthMultiplier * functionMultiplier) : overrideScale;
        strokeData.bindNormal = (new Vector3(-1f, 0f, 0f) + Random.insideUnitSphere * 0.3f).normalized;
        strokeData.bindTangent = new Vector3(0f, -tangent.x, tangent.y);
        strokeData.bindPos += strokeData.bindTangent * strokeData.scale.y * x;
        strokeData.jawMask = 1f;
        strokeData.scale.y *= 1.75f;
        strokeData.t = 0.05f - tangent.x * strokeData.scale.y * x / tMultiplier;
        strokeData.neighborIndex = startIndex + rowCount * y + Mathf.Max(0, x - 1);
        strokeData.neighborAlign = 1f;
        strokeData.passiveFollow = Mathf.Lerp(0.3f, 0.7f, x / (rowCount - 1));
    }

    private void GenerateCritterSkinDetailBrushstrokes(ref CritterUberStrokeData[] strokesArray, AgentGenome agentGenome, int agentIndex) {
        int arrayIndexStart = numStrokesPerCritterLength * numStrokesPerCritterCross + numStrokesPerCritterEyes + numStrokesPerCritterMouth + numStrokesPerCritterTeeth + numStrokesPerCritterPectoralFins + numStrokesPerCritterDorsalFin + numStrokesPerCritterTailFin;

        //CritterModuleCoreGenome gene = agentGenome.bodyGenome.coreGenome; // for readability
        //float segmentsSummedCritterLength = gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength;

        for (int i = 0; i < numStrokesPerCritterSkinDetail; i++) {
            Vector2 randUV = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));

            int brushIndex = arrayIndexStart + i;
            float angleRad = randUV.x * Mathf.PI * 2f; // verticalLerpPos * Mathf.PI;   
            float crossSectionCoordX = Mathf.Sin(angleRad);
            float crossSectionCoordZ = Mathf.Cos(angleRad);
            //Vector2 crossSectionNormalizedCoords = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * 1f;

            AddUberBrushPoint(ref strokesArray, agentGenome, agentIndex, new Vector3(crossSectionCoordX, randUV.y, crossSectionCoordZ), randUV, brushIndex);

            int coordU = Mathf.RoundToInt(randUV.x * (float)numStrokesPerCritterCross);
            int coordV = Mathf.RoundToInt(randUV.y * (float)numStrokesPerCritterLength);
            int anchorIndex = coordV * numStrokesPerCritterCross + coordU;

            Vector3 anchorNormal = strokesArray[anchorIndex].bindNormal;
            Vector3 anchorTangent = strokesArray[anchorIndex].bindTangent;

            strokesArray[brushIndex].scale = strokesArray[anchorIndex].scale * 0.4f;
            strokesArray[brushIndex].bindNormal = (anchorNormal + Random.insideUnitSphere * 0.4f).normalized;
            strokesArray[brushIndex].bindTangent = (anchorTangent + Random.insideUnitSphere * 0.3f).normalized;
            strokesArray[brushIndex].bindPos += anchorNormal * strokesArray[brushIndex].scale.x * 0.33f;
            strokesArray[brushIndex].jawMask = 1f;
            
            if (strokesArray[brushIndex].bindPos.z > 0f) {
                strokesArray[brushIndex].jawMask = -1f;
            }
            
            Vector3 hue = Vector3.Lerp(agentGenome.bodyGenome.appearanceGenome.huePrimary, agentGenome.bodyGenome.appearanceGenome.hueSecondary, Random.Range(0f, 1f));
            strokesArray[brushIndex].color = new Vector4(hue.x, hue.y, hue.z, 1f);
        }
    }

    private void SortCritterBrushstrokes(ref CritterUberStrokeData[] strokesArray, int agentIndex) {
        // temporary naive approach:
        List<CritterUberStrokeData> sortedBrushStrokesList = new List<CritterUberStrokeData>(); 
        
        // Add first brushstroke first:
        sortedBrushStrokesList.Add(strokesArray[0]);
        
        // Temporarilly use parent agent index to store own brush index, to avoid adding extra int
        int[] sortedIndexMap = new int[strokesArray.Length];  
        for (int p = 0; p < strokesArray.Length; p++) {
            strokesArray[p].parentIndex = p;
        }
        
        for (int b = 1; b < strokesArray.Length; b++) 
        {
            // For each brushstroke of this creature:w
            float brushDepth = strokesArray[b].bindPos.z;
            int listSize = sortedBrushStrokesList.Count;

            int numSamples = 6;
            float sampleCoord = 0.5f;
            int sampleIndex = Mathf.FloorToInt((float)listSize * sampleCoord);  // Floor?
            
            // progressively bisect temporary sorted brushPointList to check if it is bigger/smaller
            for (int s = 0; s < numSamples; s++) {
                // Early exit if list size is still tiny:
                if (listSize < Mathf.Pow(2f, s)) { 
                    break;
                }

                // *** GROSS HACK!!! ****
                if (sampleIndex >= sortedBrushStrokesList.Count) {   
                    sampleIndex = sortedBrushStrokesList.Count - 1;
                    //Debug.LogError("error: sampleIndex= " + sampleIndex.ToString() + ", listCount: " + sortedBrushStrokesList.Count.ToString());
                    //sortedBrushStrokesList.Add(singleCritterGenericStrokesArray[b]);
                }
                
                float sampleDepth = sortedBrushStrokesList[sampleIndex].bindPos.z;
                
                var delta = 1f / Mathf.Pow(2f, s + 2f);
                var direction = brushDepth < sampleDepth ? 1f : -1f;
                sampleCoord += delta * direction;
                
                sampleIndex = Mathf.RoundToInt((float)listSize * sampleCoord);  // *** Might have to use FLoorToInt !!! ****
            }

            sortedBrushStrokesList.Insert(sampleIndex, strokesArray[b]);
        }
        
        // Copy sorted list into actual buffer:
        if (sortedBrushStrokesList.Count == strokesArray.Length) 
        {
            for (int i = 0; i < strokesArray.Length; i++) 
            {
                int sortedIndex = sortedBrushStrokesList[i].parentIndex; // stored own brushpoint index in critterParentIndex slot
                sortedIndexMap[sortedIndex] = i; // store original index
                strokesArray[i] = sortedBrushStrokesList[i];  // copy values
            }
            
            for (int p = 0; p < strokesArray.Length; p++) 
            {
                //int originalBrushIndex = sortedIndexMap[p];
                int oldNeighborIndex = strokesArray[p].neighborIndex;
                int newNeighborIndex = sortedIndexMap[oldNeighborIndex];
                strokesArray[p].neighborIndex = newNeighborIndex;
                //strokesArray[p].neighborIndex = strokesArray[sortedIndexMap[p]].neighborIndex;
                strokesArray[p].parentIndex = agentIndex; // remember to set back to correct meaning/value
            }
        }
        else {
            Debug.Log($"Arrays don't match length!!! sorted: {sortedBrushStrokesList.Count}, master: {strokesArray.Length}");
        }
    }
    
    #endregion
    
    public void SpawnSpiritBrushQuads(CreationBrush brushData, int startIndex, int numCells) 
    {
        //Debug.Log("SpawnSpiritBrushQuads(int startIndex, int numCells)");
        SpiritBrushQuadData[] spiritBrushQuadDataArray = new SpiritBrushQuadData[32];
        spiritBrushQuadDataSpawnCBuffer = new ComputeBuffer(32, GetMemorySizeSpiritbrushQuadData());
        for (int i = 0; i < 32; i++) {
            SpiritBrushQuadData data = new SpiritBrushQuadData();
            data.vel = Random.insideUnitCircle * 5;// new Vector2(0f, 1f);
            data.worldPos = new Vector3(theCursorCzar.curMousePositionOnWaterPlane.x + data.vel.x, theCursorCzar.curMousePositionOnWaterPlane.y + data.vel.y, 0f);
            data.heading = data.vel.normalized;
            data.lifespan = Random.Range(10f, 40f);
            data.age01 = 0f;
            spiritBrushQuadDataArray[i] = data;
        }
        spiritBrushQuadDataSpawnCBuffer.SetData(spiritBrushQuadDataArray);

        int kernelCSSpawnBrushQuads = computeShaderSpiritBrush.FindKernel("CSSpawnBrushQuads");
        computeShaderSpiritBrush.SetInt("_StartIndex", 0);
        computeShaderSpiritBrush.SetBuffer(kernelCSSpawnBrushQuads, "_SpiritBrushQuadsRead", spiritBrushQuadDataSpawnCBuffer);
        computeShaderSpiritBrush.SetBuffer(kernelCSSpawnBrushQuads, "_SpiritBrushQuadsWrite", spiritBrushQuadDataCBuffer0);
        computeShaderSpiritBrush.Dispatch(kernelCSSpawnBrushQuads, 1, 1, 1);
        spiritBrushQuadDataSpawnCBuffer.Release();
    }

    private void SimEggSacks() {
        int kernelCSSimulateEggs = computeShaderEggSacks.FindKernel("CSSimulateEggs");
        computeShaderEggSacks.SetTexture(kernelCSSimulateEggs, "velocityRead", fluidManager._VelocityPressureDivergenceMain);
        computeShaderEggSacks.SetBuffer(kernelCSSimulateEggs, "eggSackSimDataCBuffer", simStateData.eggSackSimDataCBuffer);
        computeShaderEggSacks.SetBuffer(kernelCSSimulateEggs, "eggDataWriteCBuffer", simStateData.eggDataCBuffer);
        computeShaderEggSacks.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderEggSacks.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderEggSacks.Dispatch(kernelCSSimulateEggs, simStateData.eggDataCBuffer.count / 64, 1, 1);
    }
    
    // WPP: merged below 2 methods
    private void SimCritterGenericStrokes() {
        /*int kernelCSSimulateCritterGenericStrokes = computeShaderCritters.FindKernel("CSSimulateCritterGenericStrokes");
        computeShaderCritters.SetTexture(kernelCSSimulateCritterGenericStrokes, "velocityRead", fluidManager._VelocityPressureDivergenceMain);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterGenericStrokes, "critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterGenericStrokes, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterGenericStrokes, "critterGenericStrokesWriteCBuffer", mainCritterStrokesCBuffer);
        computeShaderCritters.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderCritters.Dispatch(kernelCSSimulateCritterGenericStrokes, mainCritterStrokesCBuffer.count / 16, 1, 1);*/
        SimCritterStrokes("CSSimulateCritterGenericStrokes", simStateData.critterInitDataCBuffer, 
            simStateData.critterSimDataCBuffer, mainCritterStrokesCBuffer);
    }
    
    private void SimUIToolbarCritterPortraitStrokes() {
        /*int kernelCSSimulateCritterPortraitStrokes = computeShaderCritters.FindKernel("CSSimulateCritterPortraitStrokes");
        computeShaderCritters.SetTexture(kernelCSSimulateCritterPortraitStrokes, "velocityRead", fluidManager._VelocityPressureDivergenceMain);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterPortraitStrokes, "critterInitDataCBuffer", creaturePanelUI.portraitCritterInitDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterPortraitStrokes, "critterSimDataCBuffer", creaturePanelUI.portraitCritterSimDataCBuffer);
        computeShaderCritters.SetBuffer(kernelCSSimulateCritterPortraitStrokes, "critterGenericStrokesWriteCBuffer", creaturePanelUI.critterPortraitStrokesCBuffer);
        computeShaderCritters.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderCritters.Dispatch(kernelCSSimulateCritterPortraitStrokes, creaturePanelUI.critterPortraitStrokesCBuffer.count / 16, 1, 1);*/
        SimCritterStrokes("CSSimulateCritterPortraitStrokes", creaturePanelUI.portraitCritterInitDataCBuffer, 
            creaturePanelUI.portraitCritterSimDataCBuffer, creaturePanelUI.critterPortraitStrokesCBuffer);
    }
    
    private void SimCritterStrokes(string kernelName, ComputeBuffer initBuffer, ComputeBuffer dataBuffer, ComputeBuffer writeBuffer)
    {
        int kernelIndex = computeShaderCritters.FindKernel(kernelName);
        computeShaderCritters.SetTexture(kernelIndex, "velocityRead", fluidManager._VelocityPressureDivergenceMain);
        computeShaderCritters.SetBuffer(kernelIndex, "critterInitDataCBuffer", initBuffer);
        computeShaderCritters.SetBuffer(kernelIndex, "critterSimDataCBuffer", dataBuffer);
        computeShaderCritters.SetBuffer(kernelIndex, "critterGenericStrokesWriteCBuffer", writeBuffer);
        computeShaderCritters.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderCritters.Dispatch(kernelIndex, mainCritterStrokesCBuffer.count / 16, 1, 1);
    }

    //***EAC destined to be replaced by GPU ^ ^ ^
    private void SimWorldTreeCPU() { 
        clockPanelUI.UpdateEarthStampData();
        clockPanelUI.UpdateMoonStampData();
        clockPanelUI.UpdateSunStampData();
        if(simManager.simAgeTimeSteps % 3 == 0)
            uiManager.historyPanelUI.InitializeRenderBuffers();
    }
    
    public void InitializeCreaturePortrait(AgentGenome genome) {
        //InitializeNewCritterPortraitGenome(simManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[0].representativeGenome); // speciesPool.leaderboardGenomesList[0].candidateGenome);
        isToolbarCritterPortraitEnabled = true;

        SetToolbarPortraitCritterInitData(genome);
        // ^^ data for critter        
        GenerateCritterPortraitStrokesData(genome);
        // ^^ skin stroke data
    }
    
    private void SetToolbarPortraitCritterInitData(AgentGenome genome) 
    {
        AgentGenome genome0 = genome; 
        // simManager.masterGenomePool.completeSpeciesPoolsList[simManager.uiManager.globalResourcesUI.selectedSpeciesIndex].representativeGenome;
        //AgentGenome genome1 = simManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][simManager.uiManager.mutationUI.selectedToolbarMutationID].representativeGenome;
        //AgentGenome genome2 = simManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][0].representativeGenome;
        //AgentGenome genome3 = simManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][1].representativeGenome;
        //AgentGenome genome4 = simManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][2].representativeGenome;
        //AgentGenome genome5 = simManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][3].representativeGenome;

        // NOT the best place for this:::: ***        
        treeOfLifeBackdropPortraitBorderMat.SetColor("_TintPri", new Color(genome0.bodyGenome.appearanceGenome.huePrimary.x, genome0.bodyGenome.appearanceGenome.huePrimary.y, genome0.bodyGenome.appearanceGenome.huePrimary.z));
        treeOfLifeBackdropPortraitBorderMat.SetColor("_TintSec", new Color(genome0.bodyGenome.appearanceGenome.hueSecondary.x, genome0.bodyGenome.appearanceGenome.hueSecondary.y, genome0.bodyGenome.appearanceGenome.hueSecondary.z));
        
        CritterInitData[] toolbarPortraitCritterInitDataArray = new CritterInitData[6];
        CritterInitData initData = new CritterInitData(genome0.bodyGenome);

        toolbarPortraitCritterInitDataArray[0] = initData;
        
        // * WPP: move to creaturePanelUI, code repeated at end of SetToolbarPortraitCritterSimData
        creaturePanelUI.portraitCritterInitDataCBuffer?.Release();
        creaturePanelUI.portraitCritterInitDataCBuffer = new ComputeBuffer(6, SimulationStateData.GetCritterInitDataSize());
        creaturePanelUI.portraitCritterInitDataCBuffer.SetData(toolbarPortraitCritterInitDataArray);
    }
    
    private void SetToolbarPortraitCritterSimData() 
    {
        CritterSimData[] toolbarPortraitCritterSimDataArray = new CritterSimData[6];

        bool isDead = true;
        if(simManager.GetAgent(focusedCandidate)) {
            isDead = simManager.GetAgent(focusedCandidate).curLifeStage == AgentLifeStage.Dead;
        }
        
        bool evaluationsComplete = focusedCandidate.allEvaluationsComplete;
        CritterSimData simData = new CritterSimData(isDead, evaluationsComplete);

        toolbarPortraitCritterSimDataArray[0] = simData;
        //simData.worldPos = Vector3.one * 0.034f; // simManager.uiManager.mutationUI.renderSpaceMult;
        //simData.growthPercentage = 1.5f; // simManager.uiManager.mutationUI.critterSizeMult;
        //toolbarPortraitCritterSimDataArray[1] = simData;

        //simData.growthPercentage = 2f; 
        //toolbarPortraitCritterSimDataArray[5] = simData;
        
        creaturePanelUI.portraitCritterSimDataCBuffer?.Release();
        creaturePanelUI.portraitCritterSimDataCBuffer = new ComputeBuffer(6, SimulationStateData.GetCritterSimDataSize());
        creaturePanelUI.portraitCritterSimDataCBuffer.SetData(toolbarPortraitCritterSimDataArray);
    }
    
    #region Per-frame calculation (optimization priority)
    
    // Should be called from SimManager at proper time!
    public void Tick() {  
        sunDirection = -sunGO.transform.forward;

        SimFloatyBits();
        SimEggSacks();
        SimSpiritBrushQuads();

        // PORTRAIT
        if (isToolbarCritterPortraitEnabled) {
            
            SetToolbarPortraitCritterSimData();
            SimUIToolbarCritterPortraitStrokes();
        }

        // PRIMARY CRITTERS
        SimCritterGenericStrokes();

        // WORLDTREE
        SimWorldTreeCPU();

        baronVonWater.altitudeMapRef = baronVonTerrain.terrainHeightDataRT;
        float camDist = Mathf.Clamp01(-1f * cameraManager.gameObject.transform.position.z / (400f - 10f));
        baronVonWater.camDistNormalized = camDist;
        Vector2 boxSizeHalf = 0.8f * Vector2.Lerp(new Vector2(16f, 12f) * 2, new Vector2(256f, 204f), Mathf.Clamp01(-(cameraManager.transform.position.z) / 150f));

        baronVonWater.spawnBoundsCameraDetails = cameraManager.Get4DFocusBox(boxSizeHalf);
        baronVonTerrain.spawnBoundsCameraDetails = baronVonWater.spawnBoundsCameraDetails;
        
        //baronVonTerrain.Tick(simManager.vegetationManager.rdRT1);
        
        baronVonTerrain.SimGroundBits();
        baronVonTerrain.SimWasteBits();

        baronVonWater.Tick(null);  // <-- SimWaterCurves/Chains/Water surface
    }
    
        public void SimFloatyBits() {
        int kernelSimFloatyBits = fluidManager.computeShaderFluidSim.FindKernel("SimFloatyBits");

        fluidManager.computeShaderFluidSim.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        fluidManager.computeShaderFluidSim.SetFloat("_DeltaTime", fluidManager.deltaTime);
        fluidManager.computeShaderFluidSim.SetFloat("_InvGridScale", fluidManager.invGridScale);
        fluidManager.computeShaderFluidSim.SetFloat("_Time", Time.realtimeSinceStartup);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimFloatyBits, "_SpiritBrushTex", spiritBrushRT);
        fluidManager.computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);
        fluidManager.computeShaderFluidSim.SetTexture(kernelSimFloatyBits, "VelocityRead", fluidManager._VelocityPressureDivergenceMain);
        fluidManager.computeShaderFluidSim.Dispatch(kernelSimFloatyBits, floatyBitsCBuffer.count / maxShaderThreads, 1, 1);
    }
    
    public void SimSpiritBrushQuads() {
        bool isSpawn = false;
        //if (simManager.uiManager.panelFocus == PanelFocus.Brushes) {
        //    isSpawn = true;
        //}
        float isBrushing = 0f;
        if (isSpiritBrushOn) {
            isBrushing = 1f;
        }

        // CursorParticles:
        int kernelCSSimulateCursorParticles = computeShaderSpiritBrush.FindKernel("CSSimulateCursorParticles");

        computeShaderSpiritBrush.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        computeShaderSpiritBrush.SetFloat("_DeltaTime", fluidManager.deltaTime);
        computeShaderSpiritBrush.SetFloat("_InvGridScale", fluidManager.invGridScale);
        computeShaderSpiritBrush.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderSpiritBrush.SetVector("_ParticleColor", uiManager.worldSpiritHubUI.curIconColor);
        computeShaderSpiritBrush.SetFloat("_ParticleSpawnRadius", (0.515f + isBrushing * 0.075f) * 5.3065f);
        computeShaderSpiritBrush.SetVector("_CursorWorldPosition", new Vector4(theCursorCzar.cursorParticlesWorldPos.x,
                                                                              theCursorCzar.cursorParticlesWorldPos.y,
                                                                              theCursorCzar.cursorParticlesWorldPos.z,
                                                                              0f));
        computeShaderSpiritBrush.SetBool("_SpawnOn", isSpawn);
        computeShaderSpiritBrush.SetFloat("_IsBrushing", isBrushing);
        //computeShaderSpiritBrush.SetFloat("_CursorWorldPosX", simManager.uiManager.theCursorCzar.curMousePositionOnWaterPlane.x);
        //computeShaderSpiritBrush.SetFloat("_CursorWorldPosY", simManager.uiManager.theCursorCzar.curMousePositionOnWaterPlane.y);
        computeShaderSpiritBrush.SetBuffer(kernelCSSimulateCursorParticles, "_CursorParticlesRead", cursorParticlesCBuffer0);
        computeShaderSpiritBrush.SetBuffer(kernelCSSimulateCursorParticles, "_CursorParticlesWrite", cursorParticlesCBuffer1);
        computeShaderSpiritBrush.Dispatch(kernelCSSimulateCursorParticles, 1, 1, 1);

        int kernelCSCopyBufferCursorParticles = computeShaderSpiritBrush.FindKernel("CSCopyBufferCursorParticles");   // Copy back to original buffer0
        computeShaderSpiritBrush.SetBuffer(kernelCSCopyBufferCursorParticles, "_CursorParticlesRead", cursorParticlesCBuffer1);
        computeShaderSpiritBrush.SetBuffer(kernelCSCopyBufferCursorParticles, "_CursorParticlesWrite", cursorParticlesCBuffer0);
        computeShaderSpiritBrush.Dispatch(kernelCSCopyBufferCursorParticles, 1, 1, 1);
        
        //==========================================================================================================
        int kernelCSSimulateBrushQuads = computeShaderSpiritBrush.FindKernel("CSSimulateBrushQuads");

        computeShaderSpiritBrush.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        computeShaderSpiritBrush.SetFloat("_DeltaTime", fluidManager.deltaTime);
        computeShaderSpiritBrush.SetFloat("_InvGridScale", fluidManager.invGridScale);
        computeShaderSpiritBrush.SetFloat("_Time", Time.realtimeSinceStartup);
        computeShaderSpiritBrush.SetBuffer(kernelCSSimulateBrushQuads, "_SpiritBrushQuadsRead", spiritBrushQuadDataCBuffer0);
        computeShaderSpiritBrush.SetBuffer(kernelCSSimulateBrushQuads, "_SpiritBrushQuadsWrite", spiritBrushQuadDataCBuffer1);
        computeShaderSpiritBrush.Dispatch(kernelCSSimulateBrushQuads, 1, 1, 1);

        int kernelCSCopyBuffer = computeShaderSpiritBrush.FindKernel("CSCopyBuffer");   // Copy back to original buffer0
        computeShaderSpiritBrush.SetBuffer(kernelCSCopyBuffer, "_SpiritBrushQuadsRead", spiritBrushQuadDataCBuffer1);
        computeShaderSpiritBrush.SetBuffer(kernelCSCopyBuffer, "_SpiritBrushQuadsWrite", spiritBrushQuadDataCBuffer0);
        computeShaderSpiritBrush.Dispatch(kernelCSCopyBuffer, 1, 1, 1);
    }

    // **** revisit
    public void RenderSimulationCameras() { 
        //debugRT = fluidManager._SourceColorRT;

        // SOLID OBJECTS OBSTACLES:::
        PopulateObstaclesBuffer();  // update data for obstacles before rendering

        cmdBufferFluidObstacles.Clear(); // needed since camera clear flag is set to none
        cmdBufferFluidObstacles.SetRenderTarget(fluidManager._ObstaclesRT);
        //cmdBufferFluidObstacles.ClearRenderTarget(true, true, Color.black, 1.0f);  // clear -- needed???
        cmdBufferFluidObstacles.SetViewProjectionMatrices(fluidObstaclesRenderCamera.worldToCameraMatrix, fluidObstaclesRenderCamera.projectionMatrix);
        // Draw Solid Land boundaries:
        baronVonTerrain.SetObstacleHeights(cmdBufferFluidObstacles);

        // Draw dynamic Obstacles:        
        //basicStrokeDisplayMat.SetPass(0);
        //basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
        //basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", obstacleStrokesCBuffer);        
        //cmdBufferFluidObstacles.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, obstacleStrokesCBuffer.count);
        // Disabling for now -- starting with one-way interaction between fluid & objects (fluid pushes objects, they don't push back)

        Graphics.ExecuteCommandBuffer(cmdBufferFluidObstacles);
        // Still not sure if this will work correctly... ****
        fluidObstaclesRenderCamera.Render(); // is this even needed? all drawcalls taken care of within commandBuffer?

        //if(simManager.uiManager.knowledgeUI.isOpen) {

        cmdBufferFluidColor.Clear(); // needed since camera clear flag is set to none
        cmdBufferFluidColor.SetRenderTarget(fluidManager._SourceColorRT);
        cmdBufferFluidColor.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f), 1.0f);  // clear -- needed???
        cmdBufferFluidColor.SetViewProjectionMatrices(fluidColorRenderCamera.worldToCameraMatrix, fluidColorRenderCamera.projectionMatrix);

        //cmdBufferFluidColor.Blit(fluidManager.initialDensityTex, fluidManager._SourceColorRT);
        //cmdBufferFluidColor.DrawMesh(fluidRenderMesh, Matrix4x4.identity, fluidBackgroundColorMat); // Simple unlit Texture shader -- wysiwyg
       
        switch (selectedWorldSpiritSlot.id)
        {
            case KnowledgeMapId.Plants: 
                RenderParticleMaterial(algaeParticleColorInjectMat, "foodParticleDataCBuffer", vegetationManager.plantParticlesCBuffer);
                break;
            case KnowledgeMapId.Algae:
                RenderParticleMaterial(zooplanktonParticleColorInjectMat, "animalParticleDataCBuffer", zooplanktonManager.animalParticlesCBuffer);
                break;
        }

        PopulateColorInjectionBuffer(); // update data for colorInjection objects before rendering                    
        // Creatures + EggSacks:
        basicStrokeDisplayMat.SetPass(0);
        basicStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        basicStrokeDisplayMat.SetBuffer("basicStrokesCBuffer", colorInjectionStrokesCBuffer);
        cmdBufferFluidColor.DrawProcedural(Matrix4x4.identity, basicStrokeDisplayMat, 0, MeshTopology.Triangles, 6, colorInjectionStrokesCBuffer.count);
        //why are they always rendering white??
        // Render Agent/Food/Pred colors here!!!
        // just use their display renders?

        Graphics.ExecuteCommandBuffer(cmdBufferFluidColor);
        fluidColorRenderCamera.Render();
        //simManager.environmentFluidManager.densityA.GenerateMips();
        // Update this ^^ to use Graphics.ExecuteCommandBuffer()  ****
        //}
        // COLOR INJECTION:::

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
        
        // SPIRIT BRUSH TEST!
        cmdBufferSpiritBrush.Clear(); // needed since camera clear flag is set to none
        cmdBufferSpiritBrush.SetRenderTarget(spiritBrushRT);
        cmdBufferSpiritBrush.ClearRenderTarget(true, true, Color.black, 1.0f);

        // clear -- needed???
        cmdBufferSpiritBrush.SetViewProjectionMatrices(spiritBrushRenderCamera.worldToCameraMatrix, spiritBrushRenderCamera.projectionMatrix);
        // Draw Solid Land boundaries:

        // Get brush:
        CreationBrush brushData = uiManager.brushesUI.creationBrushesArray[uiManager.brushesUI.curCreationBrushIndex];
        float scale = Mathf.Lerp(1f, 25f, Mathf.Clamp01(baronVonWater.camDistNormalized * 1.35f)) * brushData.baseScale;
        float brushIntensity = 1f * brushData.baseAmplitude;

        if (brushData.type == CreationBrush.BrushType.Burst) {
            if (brushData.isBurstActive) {
                spiritBrushRenderMultiBurstMat.SetPass(0); // *** is this really necessary?
                spiritBrushRenderMultiBurstMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
                spiritBrushRenderMultiBurstMat.SetBuffer("_SpiritBrushQuadsRead", spiritBrushQuadDataCBuffer0);
                spiritBrushRenderMultiBurstMat.SetVector("_Position", new Vector4(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y, 0f, 0f));
                spiritBrushRenderMultiBurstMat.SetFloat("_Scale", scale);
                spiritBrushRenderMultiBurstMat.SetFloat("_Strength", brushIntensity);
                spiritBrushRenderMultiBurstMat.SetFloat("_PatternColumn", brushData.patternColumn);
                spiritBrushRenderMultiBurstMat.SetFloat("_PatternRow", brushData.patternRow);
                cmdBufferSpiritBrush.DrawProcedural(Matrix4x4.identity, spiritBrushRenderMultiBurstMat, 0, MeshTopology.Triangles, 6, spiritBrushQuadDataCBuffer0.count);

                brushData.burstFrameCounter++;
                if (brushData.burstFrameCounter > brushData.burstTotalDuration) {
                    brushData.isBurstActive = false;
                    brushData.burstFrameCounter = 0;
                }
            }
            else if (isBrushing) {
                brushData.isBurstActive = true;  // start!
                brushData.burstFrameCounter = 0;
                SpawnSpiritBrushQuads(brushData, 0, 32);
            }
        }
         // Continuous/Drag type brush 
        else { 
            // *******************************************************
            isBrushing = true;
            
            // ***EC -- Change this when simulation spawn mechanics updated
            if (Time.realtimeSinceStartup % 1f > 0.5f) { 
                isBrushing = false;
            }
            
            spiritBrushRenderMat.SetPass(0);
            spiritBrushRenderMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer); // *** Needed? or just set it once in beginning....
            spiritBrushRenderMat.SetVector("_Position", new Vector4(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y, 0f, 0f));
            spiritBrushRenderMat.SetFloat("_Scale", scale);
            spiritBrushRenderMat.SetFloat("_Strength", brushIntensity);
            spiritBrushRenderMat.SetFloat("_PatternColumn", brushData.patternColumn);
            spiritBrushRenderMat.SetFloat("_PatternRow", brushData.patternRow);
            spiritBrushRenderMat.SetFloat("_IsActive", isBrushing ? 1f : 0f);
            spiritBrushRenderMat.SetFloat("_IsBrushing", 1f);
            
            Vector2 brushDir = theCursorCzar.smoothedMouseVel.magnitude > 0f ?
                theCursorCzar.normalSmoothedMouseVelocity :
                new Vector2(0f, 1f);

            spiritBrushRenderMat.SetFloat("_FacingDirX", brushDir.x);
            spiritBrushRenderMat.SetFloat("_FacingDirY", brushDir.y);
            cmdBufferSpiritBrush.DrawProcedural(Matrix4x4.identity, spiritBrushRenderMat, 0, MeshTopology.Triangles, 6, 2); 

            if (isBrushing) {
                spiritBrushRenderMat.SetFloat("_IsWildSpirit", 0f);
            }
        }

        Graphics.ExecuteCommandBuffer(cmdBufferSpiritBrush);
        // Still not sure if this will work correctly... ****
        spiritBrushRenderCamera.Render();
        
        // Species PORTRAIT:
        cmdBufferSlotPortraitDisplay.Clear();
        cmdBufferSlotPortraitDisplay.SetRenderTarget(slotPortraitRenderCamera.targetTexture); // needed???
        cmdBufferSlotPortraitDisplay.ClearRenderTarget(true, true, new Color(54f / 255f, 73f / 255f, 61f / 255f, 0f), 1.0f);  // clear -- needed???
        cmdBufferSlotPortraitDisplay.SetViewProjectionMatrices(slotPortraitRenderCamera.worldToCameraMatrix, slotPortraitRenderCamera.projectionMatrix);
        /*
        // MEDIUM STROKES!!!!
        baronVonTerrain.groundStrokesMedDisplayMat.SetPass(0);
        baronVonTerrain.groundStrokesMedDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        baronVonTerrain.groundStrokesMedDisplayMat.SetBuffer("environmentStrokesCBuffer", baronVonTerrain.terrainStoneStrokesCBuffer);
        baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_ResourceGridTex", simManager.vegetationManager.resourceGridRT1);
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);    
        baronVonTerrain.groundStrokesMedDisplayMat.SetTexture("_SpiritBrushTex", spiritBrushRT); 
        baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);       
        baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_MinFog", 0.4f);   
        baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);        
        baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_FogColor", simManager.fogColor);
        baronVonTerrain.groundStrokesMedDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);           
        baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_SunDir", sunDirection);
        baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_WorldSpaceCameraPosition", new Vector4(mainRenderCam.transform.position.x, mainRenderCam.transform.position.y, mainRenderCam.transform.position.z, 0f));
        baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_Color0", baronVonTerrain.stoneSlotGenomeCurrent.color); // new Vector4(0.9f, 0.9f, 0.8f, 1f));
        baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_Color1", baronVonTerrain.pebblesSlotGenomeCurrent.color); // new Vector4(0.7f, 0.8f, 0.9f, 1f));
        baronVonTerrain.groundStrokesMedDisplayMat.SetVector("_Color2", baronVonTerrain.sandSlotGenomeCurrent.color);                    
        cmdBufferSlotPortraitDisplay.DrawProcedural(Matrix4x4.identity, baronVonTerrain.groundStrokesMedDisplayMat, 0, MeshTopology.Triangles, 6, baronVonTerrain.terrainStoneStrokesCBuffer.count);
        */
        toolbarSpeciesPortraitStrokesMat.SetPass(0);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        toolbarSpeciesPortraitStrokesMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        toolbarSpeciesPortraitStrokesMat.SetFloat("_MapSize", SimulationManager._MapSize);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("critterInitDataCBuffer", creaturePanelUI.portraitCritterInitDataCBuffer);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("critterSimDataCBuffer", creaturePanelUI.portraitCritterSimDataCBuffer);
        toolbarSpeciesPortraitStrokesMat.SetBuffer("critterGenericStrokesCBuffer", creaturePanelUI.critterPortraitStrokesCBuffer);
        cmdBufferSlotPortraitDisplay.DrawProcedural(Matrix4x4.identity, toolbarSpeciesPortraitStrokesMat, 0, MeshTopology.Triangles, 6, creaturePanelUI.critterPortraitStrokesCBuffer.count); 

        /*
        critterDebugGenericStrokeMat.SetPass(0);
        critterDebugGenericStrokeMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        critterDebugGenericStrokeMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
        critterDebugGenericStrokeMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
        critterDebugGenericStrokeMat.SetBuffer("critterGenericStrokesCBuffer", critterGenericStrokesCBuffer);
        critterDebugGenericStrokeMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        critterDebugGenericStrokeMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        critterDebugGenericStrokeMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        critterDebugGenericStrokeMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);              
        critterDebugGenericStrokeMat.SetInt("_HoverID", cameraManager.mouseHoverAgentIndex);
        critterDebugGenericStrokeMat.SetInt("_SelectedID", cameraManager.targetAgentIndex);                                
        float isHoverCritter = cameraManager.isMouseHoverAgent ? 1f : 0f;
        float isHighlightCritter = isHoverCritter; // simManager.uiManager.panelFocus == PanelFocus.Watcher ? 1f : 0f; // ***EC -- Come back to this later                
        critterDebugGenericStrokeMat.SetFloat("_HighlightOn", isHighlightCritter);
        critterDebugGenericStrokeMat.SetFloat("_IsHover", isHoverCritter);
        critterDebugGenericStrokeMat.SetFloat("_IsSelected", cameraManager.isFollowingAgent ? 1f : 0f);
        //Debug.Log("SetTargetAgent: [ " + cameraManager.targetAgentIndex.ToString());
        critterDebugGenericStrokeMat.SetFloat("_MapSize", SimulationManager._MapSize);
        critterDebugGenericStrokeMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        critterDebugGenericStrokeMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        critterDebugGenericStrokeMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, critterDebugGenericStrokeMat, 0, MeshTopology.Triangles, 6, critterGenericStrokesCBuffer.count);
        */
        
        Graphics.ExecuteCommandBuffer(cmdBufferSlotPortraitDisplay);
        slotPortraitRenderCamera.Render();
        
        //===================   RESOURCE SIMULATION   ==========================================================
        cmdBufferResourceSim.Clear();
        cmdBufferResourceSim.SetRenderTarget(vegetationManager.resourceSimTransferRT);
        cmdBufferResourceSim.ClearRenderTarget(true, true, Color.black, 1.0f);
        cmdBufferResourceSim.SetViewProjectionMatrices(resourceSimRenderCamera.worldToCameraMatrix, resourceSimRenderCamera.projectionMatrix);
        
        RenderStructuredBuffer(resourceSimTransferMat, "animalParticleDataCBuffer", zooplanktonManager.animalParticlesCBuffer);
        RenderStructuredBuffer(plantParticleDataMat, "plantParticleDataCBuffer", vegetationManager.plantParticlesCBuffer);
        RenderStructuredBuffer(resourceSimAgentDataMat, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);

        Graphics.ExecuteCommandBuffer(cmdBufferResourceSim);
        resourceSimRenderCamera.Render();
        //======================================================================================================

        // WORLDTREE:
        cmdBufferWorldTree.Clear(); // needed since camera clear flag is set to none
        cmdBufferWorldTree.SetRenderTarget(worldTreeRenderCamera.targetTexture);
        cmdBufferWorldTree.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f), 1.0f);  // clear -- needed???
        cmdBufferWorldTree.SetViewProjectionMatrices(worldTreeRenderCamera.worldToCameraMatrix, worldTreeRenderCamera.projectionMatrix);
        
        clockPanelUI.RefreshMaterials();

        worldTreeLineDataMat.SetPass(0);
        worldTreeLineDataMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        worldTreeLineDataMat.SetBuffer("worldTreeLineDataCBuffer", uiManager.historyPanelUI.worldTreeLineDataCBuffer);
        cmdBufferWorldTree.DrawProcedural(Matrix4x4.identity, worldTreeLineDataMat, 0, MeshTopology.Triangles, 6, uiManager.historyPanelUI.worldTreeLineDataCBuffer.count);
        
        /*
        clockOrbitLineDataMat.SetPass(0);
        clockOrbitLineDataMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        clockOrbitLineDataMat.SetBuffer("clockOrbitLineDataCBuffer", clockPanelUI.clockMoonStampDataCBuffer);
        cmdBufferWorldTree.DrawProcedural(Matrix4x4.identity, clockOrbitLineDataMat, 0, MeshTopology.Triangles, 6, clockPanelUI.clockMoonStampDataCBuffer.count);

        clockOrbitLineDataMat.SetPass(0);
        clockOrbitLineDataMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        clockOrbitLineDataMat.SetBuffer("clockOrbitLineDataCBuffer", clockPanelUI.clockSunStampDataCBuffer);
        cmdBufferWorldTree.DrawProcedural(Matrix4x4.identity, clockOrbitLineDataMat, 0, MeshTopology.Triangles, 6, clockPanelUI.clockSunStampDataCBuffer.count);
        */
        Graphics.ExecuteCommandBuffer(cmdBufferWorldTree);
        worldTreeRenderCamera.Render();
    }

    void RenderStructuredBuffer(Material material, string bufferId, ComputeBuffer buffer)
    {
        material.SetPass(0);
        material.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        material.SetBuffer(bufferId, buffer);     
        material.SetFloat("_MapSize", SimulationManager._MapSize);
        cmdBufferResourceSim.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 6, buffer.count); 
    }
    
    void RenderParticleMaterial(Material material, string bufferId, ComputeBuffer buffer)
    {
        material.SetPass(0);
        material.SetBuffer(bufferId, buffer);
        material.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        material.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        material.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferFluidColor.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 6, buffer.count);
    }
    
    /*
    public void TreeOfLifeAddNewSpecies(MasterGenomePool masterGenomePool, int newSpeciesID) 
    {
        SpeciesGenomePool newSpecies = masterGenomePool.completeSpeciesPoolsList[newSpeciesID];

        int[] speciesIDArray = new int[1];
        speciesIDArray[0] = newSpecies.speciesID;
        ComputeBuffer speciesIDCBuffer = new ComputeBuffer(1, sizeof(int));
        speciesIDCBuffer.SetData(speciesIDArray);

        TreeOfLifeLeafNodeData[] updateLeafNodeDataArray = new TreeOfLifeLeafNodeData[1];

        TreeOfLifeLeafNodeData data = new TreeOfLifeLeafNodeData(newSpecies);        
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
        // if not root node
        if (newSpeciesID <= 0) return; 
         
        TreeOfLifeStemSegmentStruct[] segmentStructUpdateArray = new TreeOfLifeStemSegmentStruct[newSpecies.depthLevel]; 
        ComputeBuffer updateStemSegmentDataCBuffer = new ComputeBuffer(segmentStructUpdateArray.Length, sizeof(int) * 3 + sizeof(float) * 1);

        int curSpeciesID = newSpeciesID;

        for (int i = 0; i < newSpecies.depthLevel; i++) 
        {
            int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[curSpeciesID].parentSpeciesID;

            TreeOfLifeStemSegmentStruct newStemSegment = new TreeOfLifeStemSegmentStruct(curSpeciesID, newSpeciesID, parentSpeciesID);           

            segmentStructUpdateArray[i] = newStemSegment;
            
            // Traverse up tree
            curSpeciesID = parentSpeciesID;               
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

        // keep track of start index
        curNumTreeOfLifeStemSegments += newSpecies.depthLevel;  
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
    */
    
    private void Render() 
    {
        //Debug.Log("TestRenderCommandBuffer()");

        if (isDebugRender) 
        {
            DebugRender();
            return;
        }

        mainRenderCam.RemoveAllCommandBuffers();
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMain);

        cmdBufferMain.Clear();
        cmdBufferMain.ClearRenderTarget(true, true, new Color(1f, 0.9f, 0.75f) * 0.8f, 1.0f);
        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferMain.SetRenderTarget(renderTarget);  // Set render Target
        
        // TERRAIN MESH:
        //rockMat.SetPass(0);        
        Matrix4x4 canvasQuadTRS = Matrix4x4.TRS(new Vector3(SimulationManager._MapSize * 0.5f, SimulationManager._MapSize * 0.5f, 10f), Quaternion.identity, Vector3.one * 4096f);
        cmdBufferMain.DrawMesh(baronVonTerrain.quadMesh, canvasQuadTRS, backgroundMat);
        terrainMeshOpaqueMat.SetPass(0);
        terrainMeshOpaqueMat.SetTexture("_MainTex", baronVonTerrain.terrainColorRT0);

        baronVonTerrain.SetGroundStrokes(KnowledgeMapId.Stone);
        baronVonTerrain.SetGroundStrokes(KnowledgeMapId.Pebbles);
        baronVonTerrain.SetGroundStrokes(KnowledgeMapId.Sand);

        baronVonTerrain.SetWasteMaterialProperties();
        baronVonTerrain.SetDecomposerMaterialProperties();

        DisplayFloatyBits();

        #region Dead code (please delete)
        //if (simManager.trophicLayersManager.GetAgentsOnOff()) {
            /*critterUberStrokeShadowMat.SetPass(0);
            critterUberStrokeShadowMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            critterUberStrokeShadowMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
            critterUberStrokeShadowMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
            critterUberStrokeShadowMat.SetBuffer("critterGenericStrokesCBuffer", critterGenericStrokesCBuffer); 
            critterUberStrokeShadowMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            critterUberStrokeShadowMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
            //critterUberStrokeShadowMat.SetTexture("_VelocityTex", fluidManager._VelocityA);
            critterUberStrokeShadowMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            critterUberStrokeShadowMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
            critterUberStrokeShadowMat.SetFloat("_MapSize", SimulationManager._MapSize);
            critterUberStrokeShadowMat.SetFloat("_Turbidity", simManager.fogAmount);     
            critterUberStrokeShadowMat.SetFloat("_MinFog", minimumFogDensity);  
            critterUberStrokeShadowMat.SetFloat("_Density", Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(simManager.simResourceManager.curGlobalDecomposers / 100f)));  
            critterUberStrokeShadowMat.SetVector("_FogColor", simManager.fogColor);  
            critterUberStrokeShadowMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel); 
            //cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID);
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, critterUberStrokeShadowMat, 0, MeshTopology.Triangles, 6, critterGenericStrokesCBuffer.count);
            */
            /*
                    eggSackShadowDisplayMat.SetPass(0);
                    eggSackShadowDisplayMat.SetBuffer("critterInitDataCBuffer", simManager.simStateData.critterInitDataCBuffer);
                    eggSackShadowDisplayMat.SetBuffer("critterSimDataCBuffer", simManager.simStateData.critterSimDataCBuffer);
                    eggSackShadowDisplayMat.SetBuffer("eggDataCBuffer", simManager.simStateData.eggDataCBuffer);
                    eggSackShadowDisplayMat.SetBuffer("eggSackSimDataCBuffer", simManager.simStateData.eggSackSimDataCBuffer);
                    eggSackShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                    eggSackShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                    eggSackShadowDisplayMat.SetFloat("_MaxAltitude", baronVonTerrain.maxAltitude);
                    eggSackShadowDisplayMat.SetFloat("_GlobalWaterLevel", baronVonWater._GlobalWaterLevel); 
                    eggSackShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                    eggSackShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                    eggSackShadowDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
                    //cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID);
                    cmdBufferMain.DrawProcedural(Matrix4x4.identity, eggSackShadowDisplayMat, 0, MeshTopology.Triangles, 6, simManager.simStateData.eggDataCBuffer.count);
            */
        //}

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

        /*if(simManager.trophicLayersManager.GetZooplanktonOnOff()) {
            animalParticleShadowDisplayMat.SetPass(0);
            animalParticleShadowDisplayMat.SetBuffer("animalParticleDataCBuffer", simManager.zooplanktonManager.animalParticlesCBuffer);
            animalParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", curveRibbonVerticesCBuffer);
            animalParticleShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
            animalParticleShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            animalParticleShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
            animalParticleShadowDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
            animalParticleShadowDisplayMat.SetFloat("_GlobalWaterLevel", baronVonWater._GlobalWaterLevel);
            cmdBufferMain.DrawProcedural(Matrix4x4.identity, animalParticleShadowDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, simManager.zooplanktonManager.animalParticlesCBuffer.count);
    
        }*/
        #endregion
        
        float isHighlight = 0f;
        
        #region Dead code (please delete)
        /*if (simManager.uiManager.watcherUI.watcherSelectedTrophicSlotRef != null) { // && simManager.uiManager.panelFocus == PanelFocus.Watcher) {

            isHighlight = 1f;
            if (simManager.uiManager.watcherUI.watcherSelectedTrophicSlotRef.kingdomID == 2 && simManager.uiManager.watcherUI.watcherSelectedTrophicSlotRef.tierID == 0) {
                //isSelectedZoop = 1f;
            }
            if (simManager.uiManager.watcherUI.watcherSelectedTrophicSlotRef.kingdomID == 1 && simManager.uiManager.watcherUI.watcherSelectedTrophicSlotRef.tierID == 1) {
                //isSelectedPlant = 1f;
            }
        }*/

            /*
                // floating plants  shadows:
                plantParticleShadowDisplayMat.SetPass(0);
                plantParticleShadowDisplayMat.SetBuffer("plantParticleDataCBuffer", simManager.vegetationManager.plantParticlesCBuffer);
                plantParticleShadowDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                plantParticleShadowDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                plantParticleShadowDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                plantParticleShadowDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
                plantParticleShadowDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                plantParticleShadowDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);     
                plantParticleShadowDisplayMat.SetFloat("_MinFog", minimumFogDensity);  
                plantParticleShadowDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
                plantParticleShadowDisplayMat.SetInt("_SelectedParticleIndex", Mathf.RoundToInt(simManager.vegetationManager.selectedPlantParticleIndex));
                plantParticleShadowDisplayMat.SetInt("_HoverParticleIndex", Mathf.RoundToInt(simManager.vegetationManager.closestPlantParticleData.index));                
                plantParticleShadowDisplayMat.SetFloat("_IsSelected", isSelectedPlant); // isSelected);
                plantParticleShadowDisplayMat.SetFloat("_IsHover", simManager.uiManager.watcherUI.isPlantParticleHighlight * isHighlight);
                plantParticleShadowDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
                plantParticleShadowDisplayMat.SetVector("_FogColor", simManager.fogColor);                  
                plantParticleShadowDisplayMat.SetTexture("_ResourceGridTex", simManager.vegetationManager.resourceGridRT1); 
                plantParticleShadowDisplayMat.SetTexture("_SpiritBrushTex", spiritBrushRT);
                plantParticleShadowDisplayMat.SetVector("_SunDir", sunDirection);
                plantParticleShadowDisplayMat.SetVector("_WorldSpaceCameraPosition", new Vector4(mainRenderCam.transform.position.x, mainRenderCam.transform.position.y, mainRenderCam.transform.position.z, 0f));
                plantParticleShadowDisplayMat.SetVector("_Color0", baronVonTerrain.stoneSlotGenomeCurrent.color); // new Vector4(0.9f, 0.9f, 0.8f, 1f));
                plantParticleShadowDisplayMat.SetVector("_Color1", baronVonTerrain.pebblesSlotGenomeCurrent.color); // new Vector4(0.7f, 0.8f, 0.9f, 1f));
                plantParticleShadowDisplayMat.SetVector("_Color2", baronVonTerrain.sandSlotGenomeCurrent.color);
                //cmdBufferMain.SetGlobalTexture("_RenderedSceneRT", renderedSceneID); 
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, plantParticleShadowDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, simManager.vegetationManager.plantParticlesCBuffer.count * 32);
        */
        #endregion

        DisplayPlantParticles(isHighlight);

        #region Dead code (delegate or delete)
        // STIR STICK!!!!
        /*
        if (simManager.uiManager.curActiveTool == ToolType.Stir) {
            
            Quaternion rot = Quaternion.Euler(new Vector3(Mathf.Clamp(simManager.uiManager.theCursorCzar.smoothedMouseVel.y * 2.5f + 10f, -45f, 45f), Mathf.Clamp(simManager.uiManager.theCursorCzar.smoothedMouseVel.x * -1.5f, -45f, 45f), 0f));
            float scale = Mathf.Lerp(0.35f, 1.75f, baronVonWater.camDistNormalized);
            Matrix4x4 stirStickTransformMatrix = Matrix4x4.TRS(new Vector3(simManager.uiManager.theCursorCzar.curMousePositionOnWaterPlane.x, simManager.uiManager.theCursorCzar.curMousePositionOnWaterPlane.y, simManager.uiManager.theCursorCzar.stirStickDepth), rot, Vector3.one * scale);
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
            gizmoStirStickShadowMat.SetFloat("_MinFog", minimumFogDensity);  
            gizmoStirStickShadowMat.SetVector("_FogColor", simManager.fogColor);
            gizmoStirStickShadowMat.SetFloat("_Turbidity", simManager.fogAmount); 
            cmdBufferMain.DrawMesh(stickMesh, stirStickTransformMatrix, gizmoStirStickShadowMat);

            gizmoStirStickAMat.SetFloat("_MinFog", minimumFogDensity);  
            gizmoStirStickAMat.SetVector("_FogColor", simManager.fogColor);
            gizmoStirStickAMat.SetFloat("_Turbidity", simManager.fogAmount);
            gizmoStirStickAMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
            cmdBufferMain.DrawMesh(stickMesh, stirStickTransformMatrix, gizmoStirStickAMat);
            
        }
        else {
            if (simManager.uiManager.wildSpirit.isClickableSpiritRoaming) {
                //float scale = 4.2f; // Mathf.Lerp(0.35f, 1.75f, baronVonWater.camDistNormalized);
                float radius = simManager.uiManager.wildSpirit.roamingSpiritScale * 1.075f;
                Color tint = simManager.uiManager.wildSpirit.roamingSpiritColor * 0.66f;
                
                if (theCursorCzar._IsHoverClickableSpirit) {
                    radius *= 1.4f;
                    tint *= 1.4f;
                }
                // WILD ROAMING ROGUE SPIRIT:::::

                Matrix4x4 stirStickTransformMatrix = Matrix4x4.TRS(simManager.uiManager.wildSpirit.curRoamingSpiritPosition, Quaternion.identity, Vector3.one * radius);
                Mesh stickMesh = simManager.uiManager.wildSpirit.protoSpiritClickColliderGO.GetComponent<MeshFilter>().mesh; // meshStirStickLrg;

                gizmoProtoSpiritClickableMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                gizmoProtoSpiritClickableMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                gizmoProtoSpiritClickableMat.SetFloat("_MapSize", SimulationManager._MapSize);
                gizmoProtoSpiritClickableMat.SetFloat("_MinFog", minimumFogDensity);
                gizmoProtoSpiritClickableMat.SetVector("_FogColor", tint); //simManager.fogColor);
                gizmoProtoSpiritClickableMat.SetFloat("_CurTime", Time.realtimeSinceStartup); //simManager.fogColor);
                float isFleeingF = 0f;
                if (simManager.uiManager.wildSpirit.isFleeing) {
                    isFleeingF = 1f;
                }
                gizmoProtoSpiritClickableMat.SetFloat("_IsFleeing", isFleeingF);
                cmdBufferMain.DrawMesh(stickMesh, stirStickTransformMatrix, gizmoProtoSpiritClickableMat);
            }
        }
        */
        #endregion
        
        DisplayAnimalParticles(isHighlight);

        if (simManager.trophicLayersManager.IsLayerOn(KnowledgeMapId.Animals)) 
        {
            DisplayEggStrokes();
            DisplayCritterDebugGenericStrokes();
            DisplayEggCover();
        }

        baronVonWater.DisplayNutrients();

        #region Dead code (delegate or delete)
        /*
        if (simManager.uiManager.curActiveTool == ToolType.Add) {
            
            if (simManager.uiManager.panelFocus == PanelFocus.Brushes) {

                gizmoStirToolMat.SetPass(0);
                gizmoStirToolMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
                gizmoStirToolMat.SetBuffer("gizmoStirToolPosCBuffer", gizmoCursorPosCBuffer);
                gizmoStirToolMat.SetTexture("_MainTex", simManager.uiManager.brushesUI.selectedEssenceSlot.icon.texture);
                gizmoStirToolMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
                gizmoStirToolMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
                gizmoStirToolMat.SetColor("_Tint", simManager.uiManager.brushesUI.selectedEssenceSlot.color);
                gizmoStirToolMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
                gizmoStirToolMat.SetFloat("_Radius", 0.4f * Mathf.Lerp(0.067f, 5f, baronVonWater.camDistNormalized));  // **** Make radius variable! (possibly texture based?)
                cmdBufferMain.DrawProcedural(Matrix4x4.identity, gizmoStirToolMat, 0, MeshTopology.Triangles, 6, 1);
            }
        }
        */

        /*
         // CURSOR PARTICLES!
        cursorParticlesDisplayMat.SetPass(0);
        
        cursorParticlesDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cursorParticlesDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        cursorParticlesDisplayMat.SetBuffer("cursorParticlesCBuffer", cursorParticlesCBuffer0);
        cursorParticlesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        cursorParticlesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        cursorParticlesDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, cursorParticlesDisplayMat, 0, MeshTopology.Triangles, 6, cursorParticlesCBuffer0.count);
    */
    #endregion

        // Fluid Render Article:
        // http://blog.camposanto.com/post/171934927979/hi-im-matt-wilde-an-old-man-from-the-north-of/amp?__twitter_impression=true
    }
    
    void DebugRender()
    {
        mainRenderCam.RemoveAllCommandBuffers();
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferDebugVis);

        cmdBufferDebugVis.Clear();
        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferDebugVis.SetRenderTarget(renderTarget);  // Set render Target
        cmdBufferDebugVis.ClearRenderTarget(true, true, Color.blue, 1.0f);  // clear -- needed???

        debugVisModeMat.SetPass(0);
        debugVisModeMat.SetTexture("_MainTex", vegetationManager.resourceGridRT1);
        debugVisModeMat.SetFloat("_ColorMagnitude", 1f);
        cmdBufferDebugVis.DrawMesh(fluidRenderMesh, Matrix4x4.identity, debugVisModeMat);  //baronVonTerrain.terrainMesh

        debugAgentResourcesMat.SetPass(0);
        debugAgentResourcesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        debugAgentResourcesMat.SetBuffer("critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        debugAgentResourcesMat.SetBuffer("critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        cmdBufferDebugVis.DrawProcedural(Matrix4x4.identity, debugAgentResourcesMat, 0, MeshTopology.Triangles, 6, simStateData.critterInitDataCBuffer.count);

        debugVisAlgaeParticlesMat.SetPass(0);
        debugVisAlgaeParticlesMat.SetBuffer("foodParticleDataCBuffer", vegetationManager.plantParticlesCBuffer);
        debugVisAlgaeParticlesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //debugVisAlgaeParticlesMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferDebugVis.DrawProcedural(Matrix4x4.identity, debugVisAlgaeParticlesMat, 0, MeshTopology.Triangles, 6, vegetationManager.plantParticlesCBuffer.count);

        // add shadow pass eventually
        debugVisAnimalParticlesMat.SetPass(0);
        debugVisAnimalParticlesMat.SetBuffer("animalParticleDataCBuffer", zooplanktonManager.animalParticlesCBuffer);
        debugVisAnimalParticlesMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        //debugVisAnimalParticlesMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        cmdBufferDebugVis.DrawProcedural(Matrix4x4.identity, debugVisAnimalParticlesMat, 0, MeshTopology.Triangles, 6, zooplanktonManager.animalParticlesCBuffer.count);

        //cmdBufferDebugVis
        //mainRenderCam.RemoveAllCommandBuffers();
    }
    
    void DisplayFloatyBits()
    {
        floatyBitsDisplayMat.SetPass(0);
        //floatyBitsDisplayMat.SetTexture("_FluidColorTex", fluidManager._VelocityPressureDivergenceMain);
        floatyBitsDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        floatyBitsDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatyBitsDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        floatyBitsDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        floatyBitsDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        floatyBitsDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, floatyBitsDisplayMat, 0, MeshTopology.Triangles, 6, floatyBitsCBuffer.count);
    }
    
    void DisplayPlantParticles(float isHighlight)
    {
        float isSelectedPlant = 0f;     // * WPP: hard coded value

        plantParticleDisplayMat.SetPass(0);
        plantParticleDisplayMat.SetBuffer("plantParticleDataCBuffer", vegetationManager.plantParticlesCBuffer);
        plantParticleDisplayMat.SetBuffer("quadVerticesCBuffer", curveRibbonVerticesCBuffer);
        plantParticleDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        plantParticleDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        plantParticleDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
        plantParticleDisplayMat.SetTexture("_ResourceGridTex", vegetationManager.resourceGridRT1);
        plantParticleDisplayMat.SetTexture("_SpiritBrushTex", spiritBrushRT);
        plantParticleDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        plantParticleDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        plantParticleDisplayMat.SetFloat("_Turbidity", simManager.fogAmount);
        plantParticleDisplayMat.SetFloat("_MinFog", minimumFogDensity);
        plantParticleDisplayMat.SetInt("_SelectedParticleIndex", Mathf.RoundToInt(vegetationManager.selectedPlantParticleIndex));
        plantParticleDisplayMat.SetInt("_HoverParticleIndex", Mathf.RoundToInt(vegetationManager.closestPlantParticleData.index));
        //Debug.Log("_SelectedParticleIndex: " + Mathf.RoundToInt(simManager.vegetationManager.selectedPlantParticleIndex).ToString() + ", _HoverParticleIndex: " + Mathf.RoundToInt(simManager.vegetationManager.closestPlantParticleData.index).ToString());
        plantParticleDisplayMat.SetFloat("_IsSelected", isSelectedPlant); // isSelected);
        plantParticleDisplayMat.SetFloat("_IsHover", uiManager.plantHighlight * isHighlight);
        plantParticleDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        plantParticleDisplayMat.SetVector("_FogColor", simManager.fogColor);
        plantParticleDisplayMat.SetVector("_SunDir", sunDirection);
        plantParticleDisplayMat.SetVector("_WorldSpaceCameraPosition", new Vector4(mainRenderCam.transform.position.x, mainRenderCam.transform.position.y, mainRenderCam.transform.position.z, 0f));
        plantParticleDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, plantParticleDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, vegetationManager.plantParticlesCBuffer.count * 32);
    }
    
    // Add shadow pass eventually
    void DisplayAnimalParticles(float isHighlight)
    {
        float isSelectedMicrobes = 0f; //***EC -- revisit

        animalParticleDisplayMat.SetPass(0);
        animalParticleDisplayMat.SetBuffer("animalParticleDataCBuffer", zooplanktonManager.animalParticlesCBuffer);
        animalParticleDisplayMat.SetBuffer("quadVerticesCBuffer", curveRibbonVerticesCBuffer);
        animalParticleDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        animalParticleDisplayMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        animalParticleDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
        animalParticleDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        animalParticleDisplayMat.SetInt("_SelectedParticleIndex", Mathf.RoundToInt(zooplanktonManager.selectedAnimalParticleIndex));
        animalParticleDisplayMat.SetInt("_ClosestParticleID", Mathf.RoundToInt(zooplanktonManager.closestZooplanktonToCursorIndex));
        animalParticleDisplayMat.SetFloat("_IsSelected", isSelectedMicrobes);
        animalParticleDisplayMat.SetFloat("_IsHover", uiManager.zooplanktonHighlight * isHighlight);
        animalParticleDisplayMat.SetFloat("_IsHighlight", uiManager.zooplanktonHighlight * isHighlight); 
        animalParticleDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        animalParticleDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        animalParticleDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized); 
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, animalParticleDisplayMat, 0, MeshTopology.Triangles, 6 * numCurveRibbonQuads, zooplanktonManager.animalParticlesCBuffer.count);
    }
    
    void DisplayEggStrokes()
    {
        eggSackStrokeDisplayMat.SetPass(0);
        eggSackStrokeDisplayMat.SetBuffer("critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        eggSackStrokeDisplayMat.SetBuffer("critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        eggSackStrokeDisplayMat.SetBuffer("eggDataCBuffer", simStateData.eggDataCBuffer);
        eggSackStrokeDisplayMat.SetBuffer("eggSackSimDataCBuffer", simStateData.eggSackSimDataCBuffer);
        eggSackStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        eggSackStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        eggSackStrokeDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        eggSackStrokeDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        eggSackStrokeDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
        eggSackStrokeDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        eggSackStrokeDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, eggSackStrokeDisplayMat, 0, MeshTopology.Triangles, 6, simStateData.eggDataCBuffer.count);
    }
    
    // What is this????
    void DisplayCritterDebugGenericStrokes()
    {
        critterDebugGenericStrokeMat.SetPass(0);
        critterDebugGenericStrokeMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        critterDebugGenericStrokeMat.SetBuffer("critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        critterDebugGenericStrokeMat.SetBuffer("critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        critterDebugGenericStrokeMat.SetBuffer("critterGenericStrokesCBuffer", mainCritterStrokesCBuffer);
        critterDebugGenericStrokeMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        critterDebugGenericStrokeMat.SetTexture("_AltitudeTex", baronVonTerrain.terrainHeightDataRT);
        critterDebugGenericStrokeMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        critterDebugGenericStrokeMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);              
        critterDebugGenericStrokeMat.SetInt("_HoverID", cameraManager.mouseHoverAgentIndex);
        critterDebugGenericStrokeMat.SetInt("_SelectedID", cameraManager.targetAgentIndex); 
                                       
        float isHoverCritter = cameraManager.isMouseHoverAgent ? 1f : 0f;
        float isHighlightCritter = isHoverCritter;
        
        critterDebugGenericStrokeMat.SetFloat("_HighlightOn", isHighlightCritter);
        critterDebugGenericStrokeMat.SetFloat("_IsHover", isHoverCritter);
        critterDebugGenericStrokeMat.SetFloat("_IsSelected", cameraManager.isFollowingAgent ? 1f : 0f);
        critterDebugGenericStrokeMat.SetFloat("_MapSize", SimulationManager._MapSize);
        critterDebugGenericStrokeMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        critterDebugGenericStrokeMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        critterDebugGenericStrokeMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, critterDebugGenericStrokeMat, 0, MeshTopology.Triangles, 6, mainCritterStrokesCBuffer.count);
    }
    
    // *** Revisit this in future - probably can get away without it, just use one pass for all eggSacks
    void DisplayEggCover()
    {
        eggCoverDisplayMat.SetPass(0);
        eggCoverDisplayMat.SetBuffer("critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        eggCoverDisplayMat.SetBuffer("critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        //eggCoverDisplayMat.SetBuffer("eggSackSimDataCBuffer", simStateData.eggSackSimDataCBuffer);
        eggCoverDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        eggCoverDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        eggCoverDisplayMat.SetFloat("_MaxAltitude", SimulationManager._MaxAltitude);
        eggCoverDisplayMat.SetFloat("_CamDistNormalized", baronVonWater.camDistNormalized);
        eggCoverDisplayMat.SetFloat("_GlobalWaterLevel", SimulationManager._GlobalWaterLevel);
        eggCoverDisplayMat.SetTexture("_WaterSurfaceTex", baronVonWater.waterSurfaceDataRT1);
        //eggCoverDisplayMat.SetTexture("_TerrainColorTex", baronVonTerrain.terrainColorRT0);
        cmdBufferMain.DrawProcedural(Matrix4x4.identity, eggCoverDisplayMat, 0, MeshTopology.Triangles, 6, simStateData.critterInitDataCBuffer.count);
    }

    // requires MeshRenderer Component to be called
    private void OnWillRenderObject() {  
        //Debug.Log("OnWillRenderObject()");
        if (isInitialized) {
            Render();
        }
    }

    #endregion

    private void OnDisable() {
        if (mainRenderCam != null) {
            mainRenderCam.RemoveAllCommandBuffers();
        }
        if (fluidColorRenderCamera != null) {
            fluidColorRenderCamera.RemoveAllCommandBuffers();
        }
        if (fluidObstaclesRenderCamera != null) {
            fluidObstaclesRenderCamera.RemoveAllCommandBuffers();
        }
        if (spiritBrushRenderCamera != null) {
            spiritBrushRenderCamera.RemoveAllCommandBuffers();
        }
        if (worldTreeRenderCamera != null) {
            worldTreeRenderCamera.RemoveAllCommandBuffers();
        }
        /*if(treeOfLifeSpeciesTreeRenderCamera != null) {
            treeOfLifeSpeciesTreeRenderCamera.RemoveAllCommandBuffers();
        }*/
        /*if(treeOfLifeRenderCamera != null) {
            treeOfLifeRenderCamera.RemoveAllCommandBuffers();
        }*/
        if (slotPortraitRenderCamera != null) {
            slotPortraitRenderCamera.RemoveAllCommandBuffers();
        }
        if (resourceSimRenderCamera != null) {
            resourceSimRenderCamera.RemoveAllCommandBuffers();
        }
        if (baronVonTerrain != null) {
            baronVonTerrain.Cleanup();
        }
        if (baronVonWater != null) {
            baronVonWater.Cleanup();
        }
        cmdBufferMain?.Release();
        cmdBufferDebugVis?.Release();
        cmdBufferFluidObstacles?.Release();
        cmdBufferFluidColor?.Release();
        cmdBufferResourceSim?.Release();
        cmdBufferWorldTree?.Release();
        quadVerticesCBuffer?.Release();
        curveRibbonVerticesCBuffer?.Release();
        obstacleStrokesCBuffer?.Release();
        colorInjectionStrokesCBuffer?.Release();
        floatyBitsCBuffer?.Release();
        spiritBrushQuadDataSpawnCBuffer?.Release();
        spiritBrushQuadDataCBuffer0?.Release();
        spiritBrushQuadDataCBuffer1?.Release();
        cursorParticlesCBuffer0?.Release();
        cursorParticlesCBuffer1?.Release();
        //critterSkinStrokesCBuffer?.Release();
        mainCritterStrokesCBuffer?.Release();
        gizmoCursorPosCBuffer?.Release();
        gizmoFeedToolPosCBuffer?.Release();
        
        // TREE OF LIFE:
        //testTreeOfLifePositionCBuffer?.Release();
        treeOfLifeEventLineDataCBuffer?.Release();
        treeOfLifeWorldStatsValuesCBuffer?.Release();
        treeOfLifeSpeciesSegmentsCBuffer?.Release();
        treeOfLifeSpeciesDataKeyCBuffer?.Release();
        treeOfLifeSpeciesDataHeadPosCBuffer?.Release();
        
        // OLD TOL:
        //treeOfLifeLeafNodeDataCBuffer?.Release();
        //treeOfLifeNodeColliderDataCBufferA?.Release();
        //treeOfLifeNodeColliderDataCBufferB?.Release();
        //treeOfLifeStemSegmentDataCBuffer?.Release();
        
        //treeOfLifeStemSegmentVerticesCBuffer?.Release();
        //treeOfLifeBasicStrokeDataCBuffer?.Release();
        //treeOfLifePortraitBorderDataCBuffer?.Release();
        //treeOflifePortraitDataCBuffer?.Release();
        //treeOfLifePortraitEyeDataCBuffer?.Release();
    }
}

public struct TreeOfLifeSpeciesKeyData 
{
    public int timeCreated;
    public int timeExtinct;
    public Vector3 huePrimary;
    public Vector3 hueSecondary;
    public Vector3 parentHue;
    public float isOn;
    public float isExtinct;
    public float isSelected;
    
    public TreeOfLifeSpeciesKeyData(SpeciesGenomePool pool, float isSelected, Vector3 parentHue)
    {
        timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
        timeExtinct = pool.timeStepExtinct;
        
        var appearance = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome;
        huePrimary = appearance.huePrimary;
        hueSecondary = appearance.hueSecondary;
        this.parentHue = parentHue;
        
        isExtinct = pool.isExtinct ? 1f : 0f;
        isOn = 1f;
        this.isSelected = isSelected;
    }
}
