using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheRenderKing : MonoBehaviour {

    // SET IN INSPECTOR!!!::::
    public Camera mainCam;
    public ComputeShader computeShaderBrushStrokes;
    public Material pointStrokeDisplayMat;
    public Material curveStrokeDisplayMat;
    public Material trailStrokeDisplayMat;

    // Source Data:::
    public Agent[] agentsArray;
    public Agent playerAgent;

    public RenderTexture velocityTex;

    // AGENT LAYERS:
    // Primer:      -- The backdrop for agent, provides minimum silhouette and bg color/shape
    // Body:
    // Decorations:

    PointStrokeData[] pointStrokeDataArray;
    CurveStrokeData[] curveStrokeDataArray;
    TrailStrokeData[] trailStrokeDataArray;

    //public Vector3[] agentPositionsArray;
    public AgentSimData[] agentSimDataArray;
    private ComputeBuffer quadVerticesCBuffer;
    private ComputeBuffer agentPointStrokesCBuffer;
    private ComputeBuffer agentSimDataCBuffer;

    private ComputeBuffer curveRibbonVerticesCBuffer;
    private ComputeBuffer agentCurveStrokes0CBuffer;
    private ComputeBuffer agentCurveStrokes1CBuffer;

    private ComputeBuffer agentTrailStrokes0CBuffer;
    private ComputeBuffer agentTrailStrokes1CBuffer;
    private int numTrailPointsPerAgent = 32;

    private int numCurveRibbonQuads = 6;

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

    public struct AgentSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
        public Vector2 size;
    }
    
    // Use this for initialization
    void Awake () {

        // Holds info on Agent Positions and current status:
        agentSimDataArray = new AgentSimData[65];
        for (int i = 0; i < agentSimDataArray.Length; i++) {
            agentSimDataArray[i] = new AgentSimData();
        }
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 8);

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

        pointStrokeDisplayMat.SetPass(0);
        pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        pointStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        curveStrokeDataArray = new CurveStrokeData[65];
        for (int i = 0; i < curveStrokeDataArray.Length; i++) {
            curveStrokeDataArray[i] = new CurveStrokeData();
            curveStrokeDataArray[i].parentIndex = i;
            curveStrokeDataArray[i].hue = Vector3.one;
            curveStrokeDataArray[i].p0 = new Vector2(0f, 0f);
            curveStrokeDataArray[i].p1 = new Vector2(0f, 0.3333f);
            curveStrokeDataArray[i].p2 = new Vector2(0f, 0.6667f);
            curveStrokeDataArray[i].p3 = new Vector2(0f, 1f);
        }
        agentCurveStrokes0CBuffer = new ComputeBuffer(curveStrokeDataArray.Length, sizeof(float) * 11 + sizeof(int));
        agentCurveStrokes0CBuffer.SetData(curveStrokeDataArray);

        agentCurveStrokes1CBuffer = new ComputeBuffer(curveStrokeDataArray.Length, sizeof(float) * 11 + sizeof(int));

        // Set up Curve Ribbon Mesh billboard for brushStroke rendering
        InitializeCurveRibbonMeshBuffer();
        
        curveStrokeDisplayMat.SetPass(0);
        curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
        curveStrokeDisplayMat.SetBuffer("agentCurveStrokesReadCBuffer", agentCurveStrokes0CBuffer);

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

        trailStrokeDisplayMat.SetPass(0);
        trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
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

    public void Tick() {
        if (agentsArray != null) {
            SetSimDataArrays();

            // Update curveBrush data
            SinglePassCurveBrushData();
            //IterateCurveBrushData();

            IterateTrailStrokesData();
        }
    }

    public void InitializeAllAgentCurveData() {
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
    }

    private void SinglePassCurveBrushData() {
        int kernelCSSinglePassCurveBrushData = computeShaderBrushStrokes.FindKernel("CSSinglePassCurveBrushData");
        
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentSimDataCBuffer", agentSimDataCBuffer);
        computeShaderBrushStrokes.SetBuffer(kernelCSSinglePassCurveBrushData, "agentCurveStrokesWriteCBuffer", agentCurveStrokes0CBuffer);
        computeShaderBrushStrokes.Dispatch(kernelCSSinglePassCurveBrushData, agentCurveStrokes0CBuffer.count, 1, 1);        
    }

    private void IterateTrailStrokesData() {
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
    }

    private void OnRenderObject() {
        
        if (Camera.current == mainCam) {
            trailStrokeDisplayMat.SetPass(0);
            trailStrokeDisplayMat.SetBuffer("agentTrailStrokesReadCBuffer", agentTrailStrokes0CBuffer);
            trailStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentTrailStrokes0CBuffer.count);

            curveStrokeDisplayMat.SetPass(0);
            curveStrokeDisplayMat.SetBuffer("curveRibbonVerticesCBuffer", curveRibbonVerticesCBuffer);
            curveStrokeDisplayMat.SetBuffer("agentCurveStrokes0CBuffer", agentCurveStrokes0CBuffer);
            //Graphics.DrawProcedural(MeshTopology.Triangles, numCurveRibbonQuads * 6, agentCurveStrokes0CBuffer.count);

            pointStrokeDisplayMat.SetPass(0);
            pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            pointStrokeDisplayMat.SetBuffer("pointStrokesCBuffer", agentPointStrokesCBuffer);
            pointStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentPointStrokesCBuffer.count);
        }
    }

    public void SetPointStrokesBuffer() {

        // Populate Point strokes!!!::::::
        // Main Body Brush:::::
        /*for(int i = 0; i < agentsArray.Length; i++) {        
            pointStrokeDataArray[i] = GeneratePointStrokeData(i, Vector2.one, Vector2.zero, new Vector2(0f, 1f), agentsArray[i].hue);
        }
        pointStrokeDataArray[agentsArray.Length] = GeneratePointStrokeData(agentsArray.Length, Vector2.one, Vector2.zero, new Vector2(0f, 1f), playerAgent.hue);

        // Decorations Brushstrokes::::
        float minScale = 0.2f;
        float maxScale = 0.5f;
        for (int i = 0; i < agentsArray.Length; i++) {
            UnityEngine.Random.InitState(agentsArray[i].randomColorSeed);
            
            Vector2 scale = new Vector2(UnityEngine.Random.Range(minScale, maxScale), UnityEngine.Random.Range(minScale, maxScale));
            Vector2 pos = UnityEngine.Random.insideUnitCircle;
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 col = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

            pointStrokeDataArray[i + agentsArray.Length + 1] = GeneratePointStrokeData(i, scale, pos, dir, col);           
        }
        // PLAYER:
        UnityEngine.Random.InitState(playerAgent.randomColorSeed);
        Vector2 scale0 = new Vector2(UnityEngine.Random.Range(minScale, maxScale), UnityEngine.Random.Range(minScale, maxScale));
        Vector2 pos0 = UnityEngine.Random.insideUnitCircle;
        Vector2 dir0 = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 col0 = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

        pointStrokeDataArray[agentsArray.Length + agentsArray.Length + 1] = GeneratePointStrokeData(agentsArray.Length, scale0, pos0, dir0, col0);
        */

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

    public void SetSimDataArrays() {

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
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnDisable() {
        if(agentPointStrokesCBuffer != null) {
            agentPointStrokesCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        if (agentSimDataCBuffer != null) {
            agentSimDataCBuffer.Release();
        }
        if (agentCurveStrokes0CBuffer != null) {
            agentCurveStrokes0CBuffer.Release();
        }
        if (agentCurveStrokes1CBuffer != null) {
            agentCurveStrokes1CBuffer.Release();
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
}
