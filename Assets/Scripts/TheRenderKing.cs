using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TheRenderKing : MonoBehaviour {

    // SET IN INSPECTOR!!!::::
    public Camera mainRenderCam;
    public Camera fluidObstaclesRenderCamera;
    public Camera fluidColorRenderCamera;

    private CommandBuffer cmdBufferMainRender;
    private CommandBuffer cmdBufferFluidObstacles;
    private CommandBuffer cmdBufferFluidColor;

    public ComputeShader computeShaderBrushStrokes;
        
    public Material pointStrokeDisplayMat;
    public Material curveStrokeDisplayMat;
    public Material trailStrokeDisplayMat;

    private bool isInitialized = false;

    // Source Data:::
    //public Agent[] agentsArray;
    //public Agent playerAgent;
    //public RenderTexture velocityTex;

    // AGENT LAYERS:
    // Primer:      -- The backdrop for agent, provides minimum silhouette and bg color/shape
    // Body:
    // Decorations:

    PointStrokeData[] pointStrokeDataArray;
    CurveStrokeData[] curveStrokeDataArray;
    TrailStrokeData[] trailStrokeDataArray;

    //public Vector3[] agentPositionsArray;
    //public AgentSimData[] agentSimDataArray;
    private ComputeBuffer quadVerticesCBuffer;
    private ComputeBuffer agentPointStrokesCBuffer;
    //private ComputeBuffer agentSimDataCBuffer;

    private int numCurveRibbonQuads = 6;
    private ComputeBuffer curveRibbonVerticesCBuffer;
    private ComputeBuffer agentCurveStrokesCBuffer;
    //private ComputeBuffer agentCurveStrokes1CBuffer;

    private ComputeBuffer agentTrailStrokes0CBuffer;
    private ComputeBuffer agentTrailStrokes1CBuffer;
    private int numTrailPointsPerAgent = 32;

    public RenderTexture debugRT;
        

    public struct PointStrokeData {
        public int parentIndex;  // what agent/object is this attached to?
        public Vector2 localScale;
        public Vector2 localPos;
        public Vector2 localDir;
        public Vector3 hue;   // RGB color tint
        public float strength;  // abstraction for pressure of brushstroke + amount of paint 
        public int brushType;  // what texture/mask/brush pattern to use
    }

    public struct CurveStrokeData {
        public int parentIndex;
        public Vector3 hue;
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;
    }

    public struct TrailStrokeData {
        public Vector2 worldPos;
    }

    private int numFloatyBits = 1024 * 100;
    private ComputeBuffer floatyBitsCBuffer;
    public Material floatyBitsDisplayMat;

    private int numRipplesPerAgent = 8;
    private ComputeBuffer ripplesCBuffer;
    public Material ripplesDisplayMat;

    private int numTrailDotsPerAgent = 128;
    private ComputeBuffer trailDotsCBuffer;
    public Material trailDotsDisplayMat;

    public struct TrailDotData {
        public int parentIndex;
        public Vector2 coords01;
        public float age;
        public float initAlpha;
    }

    public Material agentProceduralDisplayMat;
    public Material foodProceduralDisplayMat;
    public Material predatorProceduralDisplayMat;

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
    public void InitializeRiseAndShine() {
        InitializeBuffers();
        InitializeMaterials();
        InitializeCommandBuffers();

        isInitialized = true;  // we did it, guys!
    }

    // Actual mix of rendering passes will change!!! 
    private void InitializeBuffers() {
        // Set up Quad Mesh billboard for brushStroke rendering
        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });

        // **** Just Curves to start!!!! ********
        curveStrokeDataArray = new CurveStrokeData[64]; // **** Temporarily just for Agents! ******
        for (int i = 0; i < curveStrokeDataArray.Length; i++) {
            curveStrokeDataArray[i] = new CurveStrokeData();
            curveStrokeDataArray[i].parentIndex = i;
            curveStrokeDataArray[i].hue = Vector3.one;
            curveStrokeDataArray[i].p0 = new Vector2(0f, 0f);
            curveStrokeDataArray[i].p1 = new Vector2(0f, 0.3333f);
            curveStrokeDataArray[i].p2 = new Vector2(0f, 0.6667f);
            curveStrokeDataArray[i].p3 = new Vector2(0f, 1f);
        }
        agentCurveStrokesCBuffer = new ComputeBuffer(curveStrokeDataArray.Length, sizeof(float) * 11 + sizeof(int));
        agentCurveStrokesCBuffer.SetData(curveStrokeDataArray);
        //agentCurveStrokes1CBuffer = new ComputeBuffer(curveStrokeDataArray.Length, sizeof(float) * 11 + sizeof(int)); // not needed?

        // Set up Curve Ribbon Mesh billboard for brushStroke rendering
        InitializeCurveRibbonMeshBuffer();

        /*
        trailStrokeDataArray = new TrailStrokeData[65 * numTrailPointsPerAgent];
        for (int i = 0; i < trailStrokeDataArray.Length; i++) {
            int agentIndex = (int)Mathf.Floor((float)i / numTrailPointsPerAgent); //i % numTrailPointsPerAgent
            float trailPos = (float)i % (float)numTrailPointsPerAgent;
            trailStrokeDataArray[i] = new TrailStrokeData();
            trailStrokeDataArray[i].worldPos = new Vector2(0f, trailPos * 1f);
        }
        agentTrailStrokes0CBuffer = new ComputeBuffer(trailStrokeDataArray.Length, sizeof(float) * 2);
        agentTrailStrokes0CBuffer.SetData(trailStrokeDataArray);
        agentTrailStrokes1CBuffer = new ComputeBuffer(trailStrokeDataArray.Length, sizeof(float) * 2);


        Vector2[] floatyBitsInitPos = new Vector2[numFloatyBits];
        floatyBitsCBuffer = new ComputeBuffer(numFloatyBits, sizeof(float) * 4);
        for(int i = 0; i < numFloatyBits; i++) {
            floatyBitsInitPos[i] = new Vector4(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
        }
        floatyBitsCBuffer.SetData(floatyBitsInitPos);
        int kernelSimFloatyBits = computeShaderFluidSim.FindKernel("SimFloatyBits");
        computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);

        // RIPPLES:
        TrailDotData[] ripplesDataArray = new TrailDotData[numRipplesPerAgent * agentSimDataCBuffer.count];
        for (int i = 0; i < agentSimDataCBuffer.count; i++) {
            for(int t = 0; t < numRipplesPerAgent; t++) {
                TrailDotData data = new TrailDotData();
                data.parentIndex = i;
                data.coords01 = new Vector2((agentSimDataArray[i].worldPos.x + 70f) / 140f, (agentSimDataArray[i].worldPos.y + 70f) / 140f);
                data.age = (float)t / (float)numRipplesPerAgent;
                data.initAlpha = 0f;
                ripplesDataArray[i * numRipplesPerAgent + t] = data;
            }
        }
        ripplesCBuffer = new ComputeBuffer(numRipplesPerAgent * agentSimDataCBuffer.count, sizeof(int) + sizeof(float) * 4);
        int kernelSimRipples = computeShaderFluidSim.FindKernel("SimRipples");        
        computeShaderFluidSim.SetBuffer(kernelSimRipples, "AgentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderFluidSim.SetBuffer(kernelSimRipples, "RipplesCBuffer", ripplesCBuffer);
        ripplesCBuffer.SetData(ripplesDataArray); 
        
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
    private void InitializeMaterials() {
        /*pointStrokeDisplayMat.SetPass(0);
        pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        pointStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        */
        
        curveStrokeDisplayMat.SetPass(0);
        curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
        curveStrokeDisplayMat.SetBuffer("agentCurveStrokesReadCBuffer", agentCurveStrokesCBuffer);                

        /*
        trailStrokeDisplayMat.SetPass(0);
        trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
        
        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);        
              
        ripplesDisplayMat.SetPass(0);
        ripplesDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        ripplesDisplayMat.SetBuffer("trailDotsCBuffer", ripplesCBuffer);
        ripplesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        
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
    private void InitializeCommandBuffers() {

        cmdBufferMainRender = new CommandBuffer();
        cmdBufferMainRender.name = "cmdBufferMainRender";
        mainRenderCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufferMainRender);

        cmdBufferFluidColor = new CommandBuffer();
        cmdBufferFluidColor.name = "cmdBufferFluidColor";

        cmdBufferFluidObstacles = new CommandBuffer();
        cmdBufferFluidObstacles.name = "cmdBufferFluidObstacles";
    }
    
    public void RenderSimulationCameras() { // **** revisit
        // Still not sure if this will work correctly... ****
        fluidObstaclesRenderCamera.Render();
        fluidColorRenderCamera.Render();
        // Update this ^^ to use Graphics.ExecuteCommandBuffer()  ****
    }

    public void Tick(SimulationStateData stateData) {  // should be called from SimManager at proper time!

        // Read current stateData and update all Buffers, send data to GPU
        // Execute computeShaders to update any dynamic particles that are purely cosmetic

        SinglePassCurveBrushData(stateData); // start with this one?
         
    }
    
    // Using this one Primarily for starters!
    private void SinglePassCurveBrushData(SimulationStateData stateData) {
        int kernelCSSinglePassCurveBrushData = computeShaderBrushStrokes.FindKernel("CSSinglePassCurveBrushData");
        
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentSimDataCBuffer", stateData.agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentCurveStrokesWriteCBuffer", agentCurveStrokesCBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSSinglePassCurveBrushData, agentCurveStrokesCBuffer.count, 1, 1);        
    }
    

    private void Render() {
        curveStrokeDisplayMat.SetPass(0);
        curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
        curveStrokeDisplayMat.SetBuffer("agentCurveStrokes0CBuffer", agentCurveStrokesCBuffer);
        Graphics.DrawProcedural(MeshTopology.Triangles, numCurveRibbonQuads * 6, agentCurveStrokesCBuffer.count);
        
    }
    private void TestRenderCommandBuffer() {
        Debug.Log("TestRenderCommandBuffer()");

        cmdBufferMainRender.Clear();

        // Create RenderTargets:
        //int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
        //cmdBuffer.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        //cmdBuffer.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline

        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBufferMainRender.SetRenderTarget(renderTarget);  // Set render Target

        curveStrokeDisplayMat.SetPass(0);
        curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
        curveStrokeDisplayMat.SetBuffer("agentCurveStrokes0CBuffer", agentCurveStrokesCBuffer);
        
        //cmdBuffer.SetGlobalTexture("_BrushColorReadTex", sceneRenderID); // Copy the Contents of FrameBuffer into brushstroke material so it knows what color it should be
        
        cmdBufferMainRender.DrawProcedural(Matrix4x4.identity, curveStrokeDisplayMat, 0, MeshTopology.Triangles, numCurveRibbonQuads * 6, agentCurveStrokesCBuffer.count);
    }
    private void OnWillRenderObject() {  // requires MeshRenderer Component to be called
        //Debug.Log("OnWillRenderObject()");
        if (isInitialized) {
            TestRenderCommandBuffer();
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

    public PointStrokeData GeneratePointStrokeData(int index, Vector2 size, Vector2 pos, Vector2 dir, Vector3 hue, float str, int brushType) {
        PointStrokeData pointStroke = new PointStrokeData();
        pointStroke.parentIndex = index;
        pointStroke.localScale = size;
        pointStroke.localPos = pos;
        pointStroke.localDir = dir;
        pointStroke.hue = hue;
        pointStroke.strength = str; // temporarily used to lerp btw primary & secondary Agent Hues
        pointStroke.brushType = brushType;

        return pointStroke;
    }

    /*private void IterateTrailStrokesData() {
    // Set position of trail Roots:
    int kernelCSPinRootTrailStrokesData = computeShaderBrushStrokes.FindKernel("CSPinRootTrailStrokesData");        
    computeShaderBrushStrokes.SetBuffer(kernelCSPinRootTrailStrokesData, "agentSimDataCBuffer", agentSimDataCBuffer);
    computeShaderBrushStrokes.SetBuffer(kernelCSPinRootTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes0CBuffer);
    computeShaderBrushStrokes.Dispatch(kernelCSPinRootTrailStrokesData, agentSimDataCBuffer.count, 1, 1);
    computeShaderBrushStrokes.SetBuffer(kernelCSPinRootTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes1CBuffer);
    computeShaderBrushStrokes.Dispatch(kernelCSPinRootTrailStrokesData, agentSimDataCBuffer.count, 1, 1);

    if(velocityTex != null) {
        // update all trailPoint positions:
        int kernelCSIterateTrailStrokesData = computeShaderBrushStrokes.FindKernel("CSIterateTrailStrokesData");
        // PING:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.SetTexture(kernelCSIterateTrailStrokesData, "velocityRead", velocityTex);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateTrailStrokesData, agentTrailStrokes0CBuffer.count, 1, 1);
        // PONG:::
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesReadCBuffer", agentTrailStrokes1CBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSIterateTrailStrokesData, "agentTrailStrokesWriteCBuffer", agentTrailStrokes0CBuffer);
        computeShaderBrushStrokes.SetTexture(kernelCSIterateTrailStrokesData, "velocityRead", velocityTex);
        computeShaderBrushStrokes.Dispatch(kernelCSIterateTrailStrokesData, agentTrailStrokes0CBuffer.count, 1, 1);
    }
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
    

    /*public void SimFloatyBits() {
        int kernelSimFloatyBits = computeShaderFluidSim.FindKernel("SimFloatyBits");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);
        computeShaderFluidSim.SetTexture(kernelSimFloatyBits, "VelocityRead", velocityA);        
        computeShaderFluidSim.Dispatch(kernelSimFloatyBits, floatyBitsCBuffer.count / 1024, 1, 1);
    }

    private void SimRipples() {
        int kernelSimRipples = computeShaderFluidSim.FindKernel("SimRipples");

        //TrailDotData[] trailDotsDataArray = new TrailDotData[numTrailDotsPerAgent * agentSimDataCBuffer.count];
        //trailDotsCBuffer.GetData(trailDotsDataArray);
        //Debug.Log("Age0 " + trailDotsDataArray[0].age.ToString() + " Age1 " + trailDotsDataArray[1].age.ToString() + " Age2 " + trailDotsDataArray[2].age.ToString() + " Age3 " + trailDotsDataArray[3].age.ToString());
        
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);

        computeShaderFluidSim.SetBuffer(kernelSimRipples, "AgentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderFluidSim.SetBuffer(kernelSimRipples, "RipplesCBuffer", ripplesCBuffer);
        computeShaderFluidSim.SetTexture(kernelSimRipples, "VelocityRead", velocityA);
        computeShaderFluidSim.Dispatch(kernelSimRipples, ripplesCBuffer.count / 8, 1, 1);
    }

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

    private void OnDisable() {
        mainRenderCam.RemoveAllCommandBuffers();
        fluidColorRenderCamera.RemoveAllCommandBuffers();
        fluidObstaclesRenderCamera.RemoveAllCommandBuffers();
        
        if (agentPointStrokesCBuffer != null) {
            agentPointStrokesCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        if (agentCurveStrokesCBuffer != null) {
            agentCurveStrokesCBuffer.Release();
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
    }



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
