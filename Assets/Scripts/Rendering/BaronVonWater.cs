using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BaronVonWater : RenderBaron {

    //public ComputeShader computeShaderBrushStrokes;
    public ComputeShader computeShaderWaterRender;

    public EnvironmentFluidManager fluidManagerRef;

    public Material waterQuadStrokesDisplayMat;
    public Material waterCurveStrokeDisplayMat;
    public Material waterChainStrokeDisplayMat;
    
    public ComputeBuffer quadVerticesCBuffer;  // quad mesh

    private int numWaterQuadStrokesPerDimension = 256;
    public ComputeBuffer waterQuadStrokesCBuffer;

    public Texture2D altitudeMapRef;

    public int numWaterCurveMeshQuads = 6;
    public ComputeBuffer waterCurveVerticesCBuffer;  // short ribbon mesh
    public int numWaterCurves = 1024 * 8;
    public ComputeBuffer waterCurveStrokesCBuffer;

    public int numWaterChains = 1024 * 2;
    public int numPointsPerWaterChain = 16;
    public ComputeBuffer waterChains0CBuffer;
    public ComputeBuffer waterChains1CBuffer;

    private int debugFrameCounter = 0;
    
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
	    public float initAlpha;
        public int brushType;
    };

	public override void Initialize() {
        InitializeBuffers();        
        InitializeMaterials();        
    }

    private void InitializeBuffers() {
        InitializeQuadMeshBuffer(); // Set up Quad Mesh billboard for brushStroke rendering           
        InitializeWaterQuadStrokesBuffer();
        InitializeWaterCurveMeshBuffer();
        InitializeWaterQuadStrokesBuffer();
        InitializeWaterCurveStrokesCBuffer();
        InitializeWaterChainStrokesCBuffer();
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
            int agentIndex = (int)Mathf.Floor((float)i / numPointsPerWaterChain);
            float trailPos = (float)i % (float)numPointsPerWaterChain;
            Vector2 randPos = new Vector2(UnityEngine.Random.Range(-60f, 60f), UnityEngine.Random.Range(-60f, 60f));
            waterChainDataArray[i] = randPos + new Vector2(0f, trailPos * -1f);
        }
        waterChains0CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
        waterChains0CBuffer.SetData(waterChainDataArray);
        waterChains1CBuffer = new ComputeBuffer(waterChainDataArray.Length, sizeof(float) * 2);
    }

    private void InitializeWaterQuadStrokesBuffer() {
        waterQuadStrokesCBuffer = new ComputeBuffer(numWaterQuadStrokesPerDimension * numWaterQuadStrokesPerDimension, sizeof(float) * 9 + sizeof(int) * 2);
        WaterQuadData[] waterQuadStrokesArray = new WaterQuadData[waterQuadStrokesCBuffer.count];
        float waterQuadStrokesBounds = 256f;
        for(int x = 0; x < numWaterQuadStrokesPerDimension; x++) {
            for(int y = 0; y < numWaterQuadStrokesPerDimension; y++) {
                int index = x * numWaterQuadStrokesPerDimension + y;
                float xPos = (float)x / (float)(numWaterQuadStrokesPerDimension - 1) * waterQuadStrokesBounds;
                float yPos = (float)y / (float)(numWaterQuadStrokesPerDimension - 1) * waterQuadStrokesBounds;
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                Vector3 pos = new Vector3(xPos + offset.x, yPos + offset.y, 0f);
                waterQuadStrokesArray[index].worldPos = pos;
                waterQuadStrokesArray[index].localScale = new Vector2(1.15f, 2.20f) * 0.6f; // Y is forward, along stroke
                waterQuadStrokesArray[index].heading = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-0.5f, 0.5f)).normalized;
                waterQuadStrokesArray[index].brushType = UnityEngine.Random.Range(0,4);
                waterQuadStrokesArray[index].age = UnityEngine.Random.Range(1f, 2f);
            
            }
        }
        waterQuadStrokesCBuffer.SetData(waterQuadStrokesArray);
    }

    private void InitializeMaterials() {
        waterQuadStrokesDisplayMat.SetPass(0);
        waterQuadStrokesDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
        waterQuadStrokesDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        waterQuadStrokesDisplayMat.SetBuffer("waterQuadStrokesCBuffer", waterQuadStrokesCBuffer);   
        
        waterCurveStrokeDisplayMat.SetPass(0);
        waterCurveStrokeDisplayMat.SetBuffer("verticesCBuffer", waterCurveVerticesCBuffer);
        waterCurveStrokeDisplayMat.SetBuffer("waterCurveStrokesCBuffer", waterCurveStrokesCBuffer);
        waterCurveStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);

        waterChainStrokeDisplayMat.SetPass(0);
        waterChainStrokeDisplayMat.SetBuffer("verticesCBuffer", quadVerticesCBuffer);
        waterChainStrokeDisplayMat.SetBuffer("waterChainsReadCBuffer", waterChains0CBuffer);
        waterChainStrokeDisplayMat.SetFloat("_MapSize", SimulationManager._MapSize);
                
    }
    private void SimWaterQuads() {
        int kernelSimWaterQuads = computeShaderWaterRender.FindKernel("CSSimWaterQuadsData");
                
        computeShaderWaterRender.SetBuffer(kernelSimWaterQuads, "waterQuadStrokesCBuffer", waterQuadStrokesCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimWaterQuads, "VelocityRead", fluidManagerRef._VelocityA);    
        computeShaderWaterRender.SetTexture(kernelSimWaterQuads, "AltitudeRead", altitudeMapRef);     
        computeShaderWaterRender.SetFloat("_MapSize", SimulationManager._MapSize);
        
        computeShaderWaterRender.Dispatch(kernelSimWaterQuads, waterQuadStrokesCBuffer.count / 1024, 1, 1);
    }
    private void SimWaterCurves() {
        int kernelSimWaterCurves = computeShaderWaterRender.FindKernel("CSSimWaterCurvesData");

        //computeShaderWaterRender.SetFloat("_TextureResolution", (float)fluidManager.resolution);
        //computeShaderWaterRender.SetFloat("_DeltaTime", fluidManager.deltaTime);
        //computeShaderWaterRender.SetFloat("_InvGridScale", fluidManager.invGridScale);
        computeShaderWaterRender.SetBuffer(kernelSimWaterCurves, "waterCurveStrokesCBuffer", waterCurveStrokesCBuffer);
        computeShaderWaterRender.SetTexture(kernelSimWaterCurves, "VelocityRead", fluidManagerRef._VelocityA);    
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
        computeShaderWaterRender.SetTexture(kernelCSPinWaterChainsData, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderWaterRender.Dispatch(kernelCSPinWaterChainsData, waterChains0CBuffer.count / numPointsPerWaterChain / 1024, 1, 1);
        
        if(debugFrameCounter % 1 == 0) {
            // Shift positions:::
            int kernelCSShiftWaterChainsData = computeShaderWaterRender.FindKernel("CSShiftWaterChainsData");
            computeShaderWaterRender.SetBuffer(kernelCSShiftWaterChainsData, "waterChainsReadCBuffer", waterChains0CBuffer);
            computeShaderWaterRender.SetBuffer(kernelCSShiftWaterChainsData, "waterChainsWriteCBuffer", waterChains1CBuffer);
            computeShaderWaterRender.SetTexture(kernelCSShiftWaterChainsData, "VelocityRead", fluidManagerRef._VelocityA);
            computeShaderWaterRender.Dispatch(kernelCSShiftWaterChainsData, waterChains0CBuffer.count / 1024, 1, 1);
        }      
        
        // Copy back to buffer1:::        
        int kernelCSSwapWaterChainsData = computeShaderWaterRender.FindKernel("CSSwapWaterChainsData");
        computeShaderWaterRender.SetBuffer(kernelCSSwapWaterChainsData, "waterChainsReadCBuffer", waterChains1CBuffer);
        computeShaderWaterRender.SetBuffer(kernelCSSwapWaterChainsData, "waterChainsWriteCBuffer", waterChains0CBuffer);
        computeShaderWaterRender.SetTexture(kernelCSSwapWaterChainsData, "VelocityRead", fluidManagerRef._VelocityA);
        computeShaderWaterRender.Dispatch(kernelCSSwapWaterChainsData, waterChains0CBuffer.count / 1024, 1, 1);

        debugFrameCounter++;
    }

    public override void Tick() {

        SimWaterQuads();
        SimWaterCurves();
        //SimWaterChains();
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

    public override void Cleanup() {
        if (waterQuadStrokesCBuffer != null) {
            waterQuadStrokesCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        
        if (waterCurveStrokesCBuffer != null) {
            waterCurveStrokesCBuffer.Release();
        }
        if (waterCurveVerticesCBuffer != null) {
            waterCurveVerticesCBuffer.Release();
        }
        if (waterChains0CBuffer != null) {
            waterChains0CBuffer.Release();
        }
        if (waterChains1CBuffer != null) {
            waterChains1CBuffer.Release();
        }
    }
}
