using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentFluidManager : MonoBehaviour {

    //public Camera mainCam;
    public ComputeShader computeShaderFluidSim;
    public Texture2D initialDensityTex;
    public Texture2D firstTimeRelaxedColorTex;

    public int resolution = 512;
    public float deltaTime = 1f;
    public float invGridScale = 1f;
    
    public float viscosity = 1f;
    public float damping = 0.01f;
    public float colorRefreshBackgroundMultiplier = 0.01f;
    public float colorRefreshDynamicMultiplier = 0.01f;

    public float isStirring = 0f;

    public int curTierWaterCurrents = 2;

    private RenderTexture velocityPressureDivergenceMain;
    public RenderTexture _VelocityPressureDivergenceMain
    {
        get
        {
            return velocityPressureDivergenceMain;
        }
        set
        {

        }
    }
    private RenderTexture velocityPressureDivergenceSwap;
    /*
    private RenderTexture pressureA;
    public RenderTexture _PressureA
    {
        get
        {
            return pressureA;
        }
        set
        {

        }
    }
    private RenderTexture pressureB;
    private RenderTexture densityA;
    public RenderTexture _DensityA
    {
        get
        {
            return densityA;
        }
        set
        {

        }
    }
    private RenderTexture densityB;
    private RenderTexture divergence;
    public RenderTexture _Divergence
    {
        get
        {
            return divergence;
        }
        set
        {

        }
    }
    */
    private RenderTexture obstaclesRT;
    public RenderTexture _ObstaclesRT
    {
        get
        {
            return obstaclesRT;
        }
        set
        {

        }
    }
    private RenderTexture sourceColorRT; // Rendered by renderKing.fluidColorRenderCamera;
    public RenderTexture _SourceColorRT
    {
        get
        {
            return sourceColorRT;
        }
        set
        {

        }
    }
    
    //private Material displayMat; // shader for display Mesh
    //public Material debugMat;

    //public bool tick = false;
    
    private int maxNumForcePoints = 64;
    public ForcePoint[] forcePointsArray;
    public ComputeBuffer forcePointsCBuffer;
    public struct ForcePoint {
        public float posX;
        public float posY;
        public float velX;
        public float velY;
        public float size;
    }    

    private float forceMultiplier;
    /*private int numColorPoints = 77;
    public ColorPoint[] colorPointsArray;
    public ComputeBuffer colorPointsCBuffer;
    public struct ColorPoint {
        public float posX;
        public float posY;
        public Vector3 color;
    }*/
    
    /*public DisplayTexture displayTex = DisplayTexture.densityA;
    public enum DisplayTexture {
        velocityA,
        velocityB,
        pressureA,
        pressureB,
        densityA,
        densityB,
        divergence,
        obstacles
    }*/

    public void StirWaterOff() {
        computeShaderFluidSim.SetFloat("_ForcePosX", 0.5f);
        computeShaderFluidSim.SetFloat("_ForcePosY", 0.5f);
        computeShaderFluidSim.SetFloat("_ForceDirX", 0f);
        computeShaderFluidSim.SetFloat("_ForceDirY", 0f);
        computeShaderFluidSim.SetFloat("_ForceSize", 100f);
        computeShaderFluidSim.SetFloat("_ForceOn", 0f);
    }
    public void StirWaterOn(Vector3 pos, Vector2 forceVector, float radiusMult) {
        /*computeShaderFluidSim.SetFloat("_ForcePosX", pos.x / 256f);
        computeShaderFluidSim.SetFloat("_ForcePosY", pos.y / 256f);
        computeShaderFluidSim.SetFloat("_ForceDirX", 1f);
        computeShaderFluidSim.SetFloat("_ForceDirY", 0f);
        computeShaderFluidSim.SetFloat("_ForceSize", 100f);
        computeShaderFluidSim.SetFloat("_ForceOn", 1f);*/

        computeShaderFluidSim.SetFloat("_ForcePosX", pos.x / 256f);
        computeShaderFluidSim.SetFloat("_ForcePosY", pos.y / 256f);
        computeShaderFluidSim.SetFloat("_ForceDirX", forceVector.x);
        computeShaderFluidSim.SetFloat("_ForceDirY", forceVector.y);
        computeShaderFluidSim.SetFloat("_ForceSize", 100f / radiusMult);
        computeShaderFluidSim.SetFloat("_ForceOn", 1f);
          
        /*
        //forcePointsArray[0]
        ForcePoint stirPoint = new ForcePoint();
            
        float forceStrength = magnitude * 0.1f;
        stirPoint.posX = pos.x / 256f;
        stirPoint.posY = pos.y / 256f;
        stirPoint.velX = forceVector.x * forceStrength;
        stirPoint.velY = forceVector.y * forceStrength;
        stirPoint.size = UnityEngine.Random.Range(100f, 100f);  // 60f, 300f originally
        forcePointsArray[0] = stirPoint;
               
        forcePointsCBuffer.SetData(forcePointsArray);
        */
    }
    
    public void InitializeFluidSystem() {
        CreateTextures(); // create RT's        

        //displayMat = GetComponent<MeshRenderer>().material;                
        
        //Graphics.Blit(firstTimeRelaxedColorTex, densityA);
        //Graphics.Blit(firstTimeRelaxedColorTex, densityB);
        //Graphics.Blit(firstTimeRelaxedColorTex, sourceColorRT);
        //InitializeVelocity();
        
        forcePointsCBuffer = new ComputeBuffer(maxNumForcePoints, sizeof(float) * 5);
        forcePointsArray = new ForcePoint[maxNumForcePoints];
        RerollForcePoints();
        //debugMat.SetTexture("_MainTex", sourceColorRT);

        computeShaderFluidSim.SetFloat("_ForceOn", 1f);
    }
    public void RerollForcePoints() {
        CreateForcePoints(5f, 64f, 128f);
    }
    public void Tick(VegetationManager vegetationManagerRef) {
        //Debug.Log("Tick!");
        computeShaderFluidSim.SetFloat("_Time", Time.time);
        computeShaderFluidSim.SetFloat("_ForceMagnitude", forceMultiplier);
        computeShaderFluidSim.SetFloat("_Viscosity", viscosity);
        computeShaderFluidSim.SetFloat("_Damping", damping);
        computeShaderFluidSim.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderFluidSim.SetFloat("_ColorRefreshAmount", colorRefreshBackgroundMultiplier);

        // Lerp towards sourceTexture color:
        //RefreshColor(vegetationManagerRef);

        // ADVECTION:::::
        Advection(velocityPressureDivergenceMain, velocityPressureDivergenceSwap);
        //Graphics.Blit(velocityB, velocityA); // TEMP! slow...

        // ******************** TEMPORARY!!! BRING THIS BACK OR REPLACE WITH Texture-based approach!!!! *******
        VelocityInjectionPoints(velocityPressureDivergenceSwap, velocityPressureDivergenceMain);

        // VISCOUS DIFFUSION:::::
        /*int numViscousDiffusionIter = 8;
        for(int i = 0; i < numViscousDiffusionIter; i++) {
            if(i % 2 == 0) {
                ViscousDiffusion(velocityA, velocityB);
            }
            else {
                ViscousDiffusion(velocityB, velocityA);
            }
        }*/
        // if #iter is even, then velA will hold latest values, so no need to Blit():
        
        // DIVERGENCE:::::
        VelocityDivergence(velocityPressureDivergenceMain, velocityPressureDivergenceSwap);  // calculate velocity divergence

        // PRESSURE JACOBI:::::
        //InitializePressure();  // zeroes out initial pressure guess // doesn't seem to produce great results
        int numPressureJacobiIter = 64;
        for (int i = 0; i < numPressureJacobiIter; i++) {
            if (i % 2 == 0) {
                PressureJacobi(velocityPressureDivergenceSwap, velocityPressureDivergenceMain);
            }
            else {                
                PressureJacobi(velocityPressureDivergenceMain, velocityPressureDivergenceSwap);
            }
        }

        // SUBTRACT GRADIENT:::::
        SubtractGradient(velocityPressureDivergenceSwap, velocityPressureDivergenceMain);
        //Graphics.Blit(velocityPressureDivergenceSwap, velocityPressureDivergenceMain); // TEMP! slow...  

        
        //SimFloatyBits();

        //SimRipples();
        //SimTrailDots();
    }

    public void UpdateSimulationClimate() {
        //SetClimateStormy();
        SetCurrentsByTier();
        //SetClimateInitial();
        /*
        float cycleValue = generation % 21f;

        if(cycleValue <= 12) {
            SetClimateInitial();
        }
        if(cycleValue > 12f && cycleValue <= 17) {            
            SetClimateStormy();
        }
        if(cycleValue > 17f) {
            SetClimateInitial();
            //SetClimateThick();
        }
        */
    }
    private void SetCurrentsByTier() {
        float lerpAmount = 0.06f;
        viscosity = Mathf.Lerp(viscosity, 0.003f, lerpAmount);
        damping = Mathf.Lerp(damping, 0.00152f, lerpAmount);
        //colorRefreshBackgroundMultiplier = Mathf.Lerp(colorRefreshBackgroundMultiplier, 0.001f, lerpAmount);
        //colorRefreshDynamicMultiplier = Mathf.Lerp(colorRefreshDynamicMultiplier, 0.0075f, lerpAmount);

        float baseTierLerp = Mathf.Clamp01((float)curTierWaterCurrents / 10f);
        float targetSpeed = 2.24f * baseTierLerp; // Mathf.Lerp(0f, 15f, baseTierLerp * baseTierLerp * baseTierLerp);

        forceMultiplier = targetSpeed; // Mathf.Lerp(forceMultiplier, targetSpeed, lerpAmount);
    }
    private void SetClimateInitial() {
        //Debug.Log("UpdateSimulationClimate Initial!");
        //CreateForcePoints(0.08f, 60f, 300f);
        float lerpAmount = 1f;

        viscosity = Mathf.Lerp(viscosity, 0.0002f, lerpAmount);
        damping = Mathf.Lerp(damping, 0.004f, lerpAmount);
        //colorRefreshBackgroundMultiplier = Mathf.Lerp(colorRefreshBackgroundMultiplier, 0.001f, lerpAmount);
        //colorRefreshDynamicMultiplier = Mathf.Lerp(colorRefreshDynamicMultiplier, 0.0075f, lerpAmount);

        forceMultiplier = Mathf.Lerp(forceMultiplier, 4.20f, lerpAmount);
    }
    private void SetClimateStormy() {
        //Debug.Log("UpdateSimulationClimate Stormy!");
        //CreateForcePoints(2f, 60f, 300f);
        float lerpAmount = 0.3f;

        viscosity = Mathf.Lerp(viscosity, 0.0002f, lerpAmount);
        damping = Mathf.Lerp(damping, 0.003f, lerpAmount);
        //colorRefreshBackgroundMultiplier = Mathf.Lerp(colorRefreshBackgroundMultiplier, 0.0025f, lerpAmount);
        //colorRefreshDynamicMultiplier = Mathf.Lerp(colorRefreshDynamicMultiplier, 0.01f, lerpAmount);

        //forceMultiplier = Mathf.Lerp(forceMultiplier, 2.5f, lerpAmount);
        forceMultiplier = Mathf.Lerp(forceMultiplier, 14f, lerpAmount);
    }
    private void SetClimateThick() {
        //Debug.Log("UpdateSimulationClimate Thick!");
        float lerpAmount = 0.3f;

        viscosity = Mathf.Lerp(viscosity, 0.02f, lerpAmount);
        damping = Mathf.Lerp(damping, 0.06f, lerpAmount);
        //colorRefreshBackgroundMultiplier = Mathf.Lerp(colorRefreshBackgroundMultiplier, 0.00005f, lerpAmount);
        //colorRefreshDynamicMultiplier = Mathf.Lerp(colorRefreshDynamicMultiplier, 0.001f, lerpAmount);

        //forceMultiplier = Mathf.Lerp(forceMultiplier, 2.5f, lerpAmount);
        forceMultiplier = Mathf.Lerp(forceMultiplier, 4.5f, lerpAmount);
    }
    
    private void CreateForcePoints(float magnitude, float minRadius, float maxRadius) {
        
        for(int i = 0; i < maxNumForcePoints; i++) {
            ForcePoint agentPoint = new ForcePoint();
            
            float forceStrength = magnitude * 0.1f;
            agentPoint.posX = UnityEngine.Random.Range(0f, 1f);
            agentPoint.posY = UnityEngine.Random.Range(0f, 1f);
            agentPoint.velX = UnityEngine.Random.Range(-1f, 1f) * forceStrength;
            agentPoint.velY = UnityEngine.Random.Range(-1f, 1f) * forceStrength;
            agentPoint.size = UnityEngine.Random.Range(minRadius, maxRadius);  // 60f, 300f originally
            forcePointsArray[i] = agentPoint;
        }        
        forcePointsCBuffer.SetData(forcePointsArray);
    }
    
    private void CreateTextures() {
        //Debug.Log("CreateTextures()!");

        velocityPressureDivergenceMain = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        velocityPressureDivergenceMain.wrapMode = TextureWrapMode.Clamp;
        velocityPressureDivergenceMain.enableRandomWrite = true;
        velocityPressureDivergenceMain.Create();

        velocityPressureDivergenceSwap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        velocityPressureDivergenceSwap.wrapMode = TextureWrapMode.Clamp;
        velocityPressureDivergenceSwap.enableRandomWrite = true;
        velocityPressureDivergenceSwap.Create();

        /*
        pressureA = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        pressureA.wrapMode = TextureWrapMode.Repeat;
        pressureA.enableRandomWrite = true;
        pressureA.Create();

        pressureB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        pressureB.wrapMode = TextureWrapMode.Repeat;
        pressureB.enableRandomWrite = true;
        pressureB.Create();

        densityA = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        densityA.wrapMode = TextureWrapMode.Repeat;
        densityA.enableRandomWrite = true;
        densityA.useMipMap = true;
        densityA.Create();

        densityB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        densityB.wrapMode = TextureWrapMode.Repeat;
        densityB.enableRandomWrite = true;
        densityB.Create();

        divergence = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        divergence.wrapMode = TextureWrapMode.Repeat;
        divergence.enableRandomWrite = true;
        divergence.Create();
        */
        // ** Needed?

        obstaclesRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        obstaclesRT.wrapMode = TextureWrapMode.Clamp;
        obstaclesRT.enableRandomWrite = true;
        obstaclesRT.Create();
        
        sourceColorRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        sourceColorRT.wrapMode = TextureWrapMode.Repeat;
        sourceColorRT.enableRandomWrite = true;
        sourceColorRT.Create();
    }
    public Vector2[] GetFluidVelocityAtObjectPositions(Vector4[] positionsArray) {

        ComputeBuffer objectDataInFluidCoordsCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 4);
        ComputeBuffer velocityValuesCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 2);

        Vector2[] objectVelocitiesArray = new Vector2[positionsArray.Length];

        objectDataInFluidCoordsCBuffer.SetData(positionsArray);
        
        int kernelGetObjectVelocities = computeShaderFluidSim.FindKernel("GetObjectVelocities");  
        computeShaderFluidSim.SetBuffer(kernelGetObjectVelocities, "ObjectPositionsCBuffer", objectDataInFluidCoordsCBuffer);
        computeShaderFluidSim.SetBuffer(kernelGetObjectVelocities, "VelocityValuesCBuffer", velocityValuesCBuffer);
        computeShaderFluidSim.SetTexture(kernelGetObjectVelocities, "VelocityRead", velocityPressureDivergenceMain);
        computeShaderFluidSim.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderFluidSim.Dispatch(kernelGetObjectVelocities, positionsArray.Length, 1, 1);

        velocityValuesCBuffer.GetData(objectVelocitiesArray);

        velocityValuesCBuffer.Release();
        objectDataInFluidCoordsCBuffer.Release();

        return objectVelocitiesArray;
        
    }
    // *** TEMP DISABLED!!!!! ****
    /*
    private void RefreshColor(VegetationManager vegetationManagerRef) { 
        int kernelRefreshColor = computeShaderFluidSim.FindKernel("RefreshColor");
        //RenderTexture reactionDiffusionRT
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetFloat("_MapSize", SimulationManager._MapSize);

        computeShaderFluidSim.SetFloat("_ColorRefreshDynamicMultiplier", colorRefreshDynamicMultiplier);
        computeShaderFluidSim.SetFloat("_ColorRefreshAmount", colorRefreshBackgroundMultiplier);
        computeShaderFluidSim.SetVector("_NutrientsColor", new Color(1f, 1f, 0.4f)); // vegetationManagerRef.decomposerSlotGenomeCurrent.displayColor);
        computeShaderFluidSim.SetVector("_AlgaeColor", vegetationManagerRef.algaeSlotGenomeCurrent.displayColor);
        // break this out into Background texture and Dynamic Render pass (agents/food/preds/FX only) ??? ******
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "fluidBackgroundColorTex", initialDensityTex);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "colorInjectionRenderTex", sourceColorRT);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "DensityRead", densityB);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "DensityWrite", densityA);
        //computeShaderFluidSim.SetTexture(kernelRefreshColor, "_AlgaeDecomposerMapRead", vegetationManagerRef.rdRT1); 
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "_NutrientGridRead", vegetationManagerRef.resourceGridRT1); 
        computeShaderFluidSim.Dispatch(kernelRefreshColor, resolution / 16, resolution / 16, 1);
        
    }
    */

    private void Advection(RenderTexture readRT, RenderTexture writeRT) {

        int kernelAdvection = computeShaderFluidSim.FindKernel("Advection");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetFloat("_MapSize", SimulationManager._MapSize);
        computeShaderFluidSim.SetTexture(kernelAdvection, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.SetTexture(kernelAdvection, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelAdvection, "VelocityWrite", writeRT);
        //computeShaderFluidSim.SetTexture(kernelAdvection, "DensityRead", densityA);
        //computeShaderFluidSim.SetTexture(kernelAdvection, "DensityWrite", densityB);
        computeShaderFluidSim.Dispatch(kernelAdvection, resolution / 32, resolution / 32, 1);
    }
    private void ViscousDiffusion(RenderTexture readRT, RenderTexture writeRT) {

        int kernelViscousDiffusion = computeShaderFluidSim.FindKernel("ViscousDiffusion");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelViscousDiffusion, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelViscousDiffusion, "VelocityWrite", writeRT);
        computeShaderFluidSim.Dispatch(kernelViscousDiffusion, resolution / 32, resolution / 32, 1);
    }

    // *** Replace with textural?
    private void VelocityInjectionPoints(RenderTexture readRT, RenderTexture writeRT) {
        
        //int kernelVelocityInjectionPoints = computeShaderFluidSim.FindKernel("VelocityInjectionPoints");
        int kernelVelocityInjectionPoints = computeShaderFluidSim.FindKernel("VelocityInjectionPointsVortex");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.SetBuffer(kernelVelocityInjectionPoints, "ForcePointsCBuffer", forcePointsCBuffer);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "VelocityWrite", writeRT);
        
        computeShaderFluidSim.Dispatch(kernelVelocityInjectionPoints, resolution / 32, resolution / 32, 1);
    }
    /*private void DensityInjectionPoints() {
        
        int kernelDensityInjectionPoints = computeShaderFluidSim.FindKernel("DensityInjectionPoints");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        //computeShaderFluidSim.SetBuffer(kernelDensityInjectionPoints, "ColorPointsCBuffer", colorPointsCBuffer);
        computeShaderFluidSim.SetTexture(kernelDensityInjectionPoints, "DensityRead", densityA);
        computeShaderFluidSim.SetTexture(kernelDensityInjectionPoints, "DensityWrite", densityB);

        computeShaderFluidSim.Dispatch(kernelDensityInjectionPoints, resolution / 16, resolution / 16, 1);
    }  */  
    private void VelocityDivergence(RenderTexture readTex, RenderTexture writeTex) {
        int kernelViscosityDivergence = computeShaderFluidSim.FindKernel("VelocityDivergence");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "VelocityRead", readTex);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "VelocityWrite", writeTex);
        computeShaderFluidSim.Dispatch(kernelViscosityDivergence, resolution / 32, resolution / 32, 1);
    }
    private void PressureJacobi(RenderTexture readRT, RenderTexture writeRT) {
        int kernelPressureJacobi = computeShaderFluidSim.FindKernel("PressureJacobi");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "ObstaclesRead", obstaclesRT);
        //computeShaderFluidSim.SetTexture(kernelPressureJacobi, "DivergenceRead", divergence);
        //computeShaderFluidSim.SetTexture(kernelPressureJacobi, "PressureRead", readRT);
        //computeShaderFluidSim.SetTexture(kernelPressureJacobi, "PressureWrite", writeRT);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "VelocityWrite", writeRT);
        computeShaderFluidSim.Dispatch(kernelPressureJacobi, resolution / 32, resolution / 32, 1);
    }
    private void SubtractGradient(RenderTexture readRT, RenderTexture writeRT) {
        int kernelSubtractGradient = computeShaderFluidSim.FindKernel("SubtractGradient");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "VelocityWrite", writeRT);
        computeShaderFluidSim.Dispatch(kernelSubtractGradient, resolution / 32, resolution / 32, 1);
    }

    private void OnDisable() {
        if(forcePointsCBuffer != null) {
            forcePointsCBuffer.Release();
        }
        /*if (floatyBitsCBuffer != null) {
            floatyBitsCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        if (agentSimDataCBuffer != null) {
            agentSimDataCBuffer.Release();
        }
        if (foodSimDataCBuffer != null) {
            foodSimDataCBuffer.Release();
        }
        if (predatorSimDataCBuffer != null) {
            predatorSimDataCBuffer.Release();
        }
        if (ripplesCBuffer != null) {
            ripplesCBuffer.Release();
        }
        if (trailDotsCBuffer != null) {
            trailDotsCBuffer.Release();
        }*/
        //trailDotsCBuffer
    }

    // OLD:::::
    /*public Vector2 GetFluidVelocityAtPosition(Vector2 uv) {
        ComputeBuffer velocityValuesCBuffer = new ComputeBuffer(1, sizeof(float) * 2);

        Vector2[] fluidVelocityValues = new Vector2[1];
        //fluidVelocityValues [0] = Vector2.zero;
        computeShaderFluidSim.SetFloat("_Time", Time.time);
        //computeShaderFluidSim.SetFloat("_ForceMagnitude", forceMagnitude);
        computeShaderFluidSim.SetFloat("_Viscosity", viscosity);
        computeShaderFluidSim.SetFloat("_Damping", damping);
        //computeShaderFluidSim.SetFloat("_ForceSize", invBrushSize);
        computeShaderFluidSim.SetFloat("_ColorRefreshAmount", colorRefreshGlobalMultiplier);
        
        int kernelGetVelAtCoords = computeShaderFluidSim.FindKernel("GetVelAtCoords");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetFloat("_SampleCoordX", uv.x);
        computeShaderFluidSim.SetFloat("_SampleCoordY", uv.y);
        computeShaderFluidSim.SetBuffer(kernelGetVelAtCoords, "VelocityValuesCBuffer", velocityValuesCBuffer);
        computeShaderFluidSim.SetTexture(kernelGetVelAtCoords, "VelocityRead", velocityA);
        computeShaderFluidSim.Dispatch(kernelGetVelAtCoords, 1, 1, 1);

        velocityValuesCBuffer.GetData(fluidVelocityValues);

        velocityValuesCBuffer.Release();
        //Debug.Log("Fluid Velocity at: (" + uv.x.ToString() + ", " + uv.y.ToString() + "): [" + fluidVelocityValues[0].x.ToString() + ", " + fluidVelocityValues[0].y.ToString() + "]");
        return fluidVelocityValues[0];
    }*/
     /*private void InitializeVelocity() {
        int kernelInitializeVelocity = computeShaderFluidSim.FindKernel("InitializeVelocity");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelInitializeVelocity, "VelocityWrite", velocityA);
        computeShaderFluidSim.Dispatch(kernelInitializeVelocity, resolution / 16, resolution / 16, 1);
    }*/
    /*public void SetSimDataArrays() {
        
        for(int i = 0; i < agentSimDataArray.Length - 1; i++) {            
            agentSimDataArray[i].worldPos = new Vector2(agentsArray[i].transform.position.x, agentsArray[i].transform.position.y);
            agentSimDataArray[i].velocity = agentsArray[i].smoothedThrottle; // new Vector2(agentsArray[i].testModule.ownRigidBody2D.velocity.x, agentsArray[i].testModule.ownRigidBody2D.velocity.y);
            agentSimDataArray[i].heading = agentsArray[i].facingDirection; // new Vector2(0f, 1f); // Update later -- store inside Agent class?            
        } // Player:
        agentSimDataArray[agentSimDataArray.Length - 1].worldPos = new Vector2(playerAgent.transform.position.x, playerAgent.transform.position.y);
        agentSimDataArray[agentSimDataArray.Length - 1].velocity = playerAgent.smoothedThrottle; 
        agentSimDataArray[agentSimDataArray.Length - 1].heading = playerAgent.facingDirection;
        agentSimDataCBuffer.SetData(agentSimDataArray);

        for (int i = 0; i < foodSimDataArray.Length; i++) {
            foodSimDataArray[i].worldPos = new Vector2(foodArray[i].transform.position.x, foodArray[i].transform.position.y);
            foodSimDataArray[i].velocity = new Vector2(foodArray[i].GetComponent<Rigidbody2D>().velocity.x, agentsArray[i].GetComponent<Rigidbody2D>().velocity.y);
            foodSimDataArray[i].scale = foodArray[i].curScale;
            foodSimDataArray[i].foodAmount = new Vector3(foodArray[i].amountR, foodArray[i].amountG, foodArray[i].amountB);
        }
        foodSimDataCBuffer.SetData(foodSimDataArray);
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            predatorSimDataArray[i].worldPos = new Vector2(predatorsArray[i].transform.position.x, predatorsArray[i].transform.position.y);
            predatorSimDataArray[i].velocity = new Vector2(predatorsArray[i].rigidBody.velocity.x, predatorsArray[i].rigidBody.velocity.y);
            predatorSimDataArray[i].scale = predatorsArray[i].curScale; 
        }
        predatorSimDataCBuffer.SetData(predatorSimDataArray);
    }*/
    /*public void Run() {
        
        
                
        //SetSimDataArrays(); // Send data about gameState to GPU for display

        //SetDisplayTexture(); // ***deprecated
        if (tick) {   // So I can step through the program slowly at first            
            Tick();            
        }
        
    }*/
    //
    /*private void SetDisplayTexture() {
        switch(displayTex) {
            case DisplayTexture.densityA:
                displayMat.SetTexture("_MainTex", densityA);
                break;
            case DisplayTexture.densityB:
                displayMat.SetTexture("_MainTex", densityB);
                break;
            case DisplayTexture.divergence:
                displayMat.SetTexture("_MainTex", divergence);
                break;
            case DisplayTexture.pressureA:
                displayMat.SetTexture("_MainTex", pressureA);
                break;
            case DisplayTexture.pressureB:
                displayMat.SetTexture("_MainTex", pressureB);
                break;
            case DisplayTexture.obstacles:
                displayMat.SetTexture("_MainTex", obstacles);
                break;
            case DisplayTexture.velocityA:
                displayMat.SetTexture("_MainTex", velocityA);
                break;
            case DisplayTexture.velocityB:
                displayMat.SetTexture("_MainTex", velocityB);
                break;
            default:
                //
                break;
        }
    }*/
}
