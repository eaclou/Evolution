using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidManager : MonoBehaviour {

    public ComputeShader computeShaderFluidSim;
    public Texture2D initialDensityTex;

    public int resolution = 64;
    public float deltaTime = 1f;
    public float invGridScale = 1f;

    public float forceMagnitude = 0.25f;
    public float viscosity = 1f;
    public float damping = 0.01f;
    public float invBrushSize = 128f;
    public float colorRefreshAmount = 0.01f;

    private RenderTexture velocityA;
    private RenderTexture velocityB;
    private RenderTexture pressureA;
    private RenderTexture pressureB;
    private RenderTexture densityA;
    private RenderTexture densityB;

    private RenderTexture obstaclesRT;
    public RenderTexture _ObstaclesRT => obstaclesRT;

    private RenderTexture divergence;

    private Material displayMat;

    public bool tick = false;

    private float prevMousePosX;
    private float prevMousePosY;

    public ComputeBuffer forcePointsCBuffer;
    public struct ForcePoint {
        public float posX;
        public float posY;
        public float velX;
        public float velY;
        public float size;
    }

    public DisplayTexture displayTex = DisplayTexture.densityA;
    public enum DisplayTexture {
        velocityA,
        velocityB,
        pressureA,
        pressureB,
        densityA,
        densityB,
        divergence
    }
    

	// Use this for initialization
	void Start () {
        CreateTextures();
        CreateForcePoints();
        displayMat = GetComponent<MeshRenderer>().material; //.SetTexture("_MainTex", velocityA);
        Graphics.Blit(initialDensityTex, densityA);
        //InitializeVelocity();
        //Advection();
        //Graphics.Blit(velocityB, velocityA); // TEMP! slow...

        //BoundaryConditions();

        //Tick();
        
    }

    private void CreateForcePoints() {
        int numForcePoints = 64;

        ForcePoint[] forcePointsArray = new ForcePoint[numForcePoints];
        for(int i = 0; i < numForcePoints; i++) {
            ForcePoint point = new ForcePoint();
            point.posX = UnityEngine.Random.Range(0f, 1f);
            point.posY = UnityEngine.Random.Range(0f, 1f);
            point.velX = UnityEngine.Random.Range(-1f, 1f);
            point.velY = UnityEngine.Random.Range(-1f, 1f);
            point.size = UnityEngine.Random.Range(256f, 600f);
            forcePointsArray[i] = point;
        }

        forcePointsCBuffer = new ComputeBuffer(numForcePoints, sizeof(float) * 5);

        forcePointsCBuffer.SetData(forcePointsArray);
    }

    private void CreateTextures() {
        Debug.Log("CreateTextures()!");

        velocityA = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        velocityA.wrapMode = TextureWrapMode.Repeat;
        velocityA.enableRandomWrite = true;
        velocityA.Create();

        velocityB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        velocityB.wrapMode = TextureWrapMode.Repeat;
        velocityB.enableRandomWrite = true;
        velocityB.Create();

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
        densityA.Create();

        densityB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        densityB.wrapMode = TextureWrapMode.Repeat;
        densityB.enableRandomWrite = true;
        densityB.Create();

        divergence = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        divergence.wrapMode = TextureWrapMode.Repeat;
        divergence.enableRandomWrite = true;
        divergence.Create();

        obstaclesRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        obstaclesRT.wrapMode = TextureWrapMode.Clamp;
        obstaclesRT.enableRandomWrite = true;
        obstaclesRT.Create();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        SetDisplayTexture();
        
        if (tick) {   // So I can step through the program slowly at first
            //tick = false;
            Tick();

            //Debug.Log(new Vector2(Input.mousePosition.x - prevMousePosX, Input.mousePosition.y - prevMousePosY).ToString());
        }

        prevMousePosX = Input.mousePosition.x;
        prevMousePosY = Input.mousePosition.y;
    }

    private void SetDisplayTexture() {
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
    }

    private void Tick() {
        //Debug.Log("Tick!");
        computeShaderFluidSim.SetFloat("_Time", Time.time);
        computeShaderFluidSim.SetFloat("_ForceMagnitude", forceMagnitude);
        computeShaderFluidSim.SetFloat("_Viscosity", viscosity);
        computeShaderFluidSim.SetFloat("_Damping", damping);
        computeShaderFluidSim.SetFloat("_ForceSize", invBrushSize);
        computeShaderFluidSim.SetFloat("_ColorRefreshAmount", colorRefreshAmount);
        
        // ADVECTION:::::
        Advection();
        Graphics.Blit(velocityB, velocityA); // TEMP! slow...
        //RefreshColor();
        //Graphics.Blit(densityB, densityA); // TEMP! slow...

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

        // EXTERNAL FORCE:::::
        //ExternalForce();
        VelocityInjectionPoints();
        Graphics.Blit(velocityB, velocityA); // TEMP! slow...

        // DIVERGENCE:::::
        VelocityDivergence();

        // PRESSURE JACOBI:::::
        //InitializePressure();  // zeroes out initial pressure guess
        int numPressureJacobiIter = 48;
        for (int i = 0; i < numPressureJacobiIter; i++) {
            if (i % 2 == 0) {
                PressureJacobi(pressureA, pressureB);
            }
            else {
                PressureJacobi(pressureB, pressureA);
            }
        }

        // SUBTRACT GRADIENT:::::
        SubtractGradient();
        Graphics.Blit(velocityB, velocityA); // TEMP! slow...

        // BOUNDARY CONDITIONS:
        //BoundaryConditions();  // Have to interweave this into each Texture "Slab Operation"
    }

    private void RefreshColor() {
        int kernelRefreshColor = computeShaderFluidSim.FindKernel("RefreshColor");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "SourceColorTex", initialDensityTex);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "DensityRead", densityB);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "DensityWrite", densityA);
        computeShaderFluidSim.Dispatch(kernelRefreshColor, resolution / 16, resolution / 16, 1);
    }

    private void InitializeVelocity() {
        int kernelInitializeVelocity = computeShaderFluidSim.FindKernel("InitializeVelocity");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelInitializeVelocity, "VelocityWrite", velocityA);
        computeShaderFluidSim.Dispatch(kernelInitializeVelocity, resolution / 16, resolution / 16, 1);
    }

    private void Advection() {

        int kernelAdvection = computeShaderFluidSim.FindKernel("Advection");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelAdvection, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelAdvection, "VelocityWrite", velocityB);
        computeShaderFluidSim.SetTexture(kernelAdvection, "DensityRead", densityA);
        computeShaderFluidSim.SetTexture(kernelAdvection, "DensityWrite", densityB);
        computeShaderFluidSim.SetTexture(kernelAdvection, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.Dispatch(kernelAdvection, resolution / 16, resolution / 16, 1);
    }

    private void ViscousDiffusion(RenderTexture readRT, RenderTexture writeRT) {

        int kernelViscousDiffusion = computeShaderFluidSim.FindKernel("ViscousDiffusion");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelViscousDiffusion, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelViscousDiffusion, "VelocityWrite", writeRT);
        computeShaderFluidSim.Dispatch(kernelViscousDiffusion, resolution / 16, resolution / 16, 1);
    }

    private void VelocityInjectionPoints() {
        int kernelVelocityInjectionPoints = computeShaderFluidSim.FindKernel("VelocityInjectionPoints");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetBuffer(kernelVelocityInjectionPoints, "ForcePointsCBuffer", forcePointsCBuffer);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "VelocityWrite", velocityB);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.Dispatch(kernelVelocityInjectionPoints, resolution / 16, resolution / 16, 1);
    }
    private void ExternalForce() {
        int kernelExternalForce = computeShaderFluidSim.FindKernel("ExternalForce");
        computeShaderFluidSim.SetFloat("_ForceOn", 0.0f);

        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {                

                computeShaderFluidSim.SetFloat("_ForcePosX", hit.textureCoord.x);
                computeShaderFluidSim.SetFloat("_ForcePosY", hit.textureCoord.y);
                Vector2 mouseDir = new Vector2(Input.mousePosition.x - prevMousePosX, Input.mousePosition.y - prevMousePosY);
                computeShaderFluidSim.SetFloat("_ForceDirX", mouseDir.x);
                computeShaderFluidSim.SetFloat("_ForceDirY", mouseDir.y);
                computeShaderFluidSim.SetFloat("_ForceOn", 1.0f);

                //Debug.Log("RayCast HIT!!! " + hit.textureCoord.ToString() + ", dir: " + mouseDir.ToString());
            }
        }

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelExternalForce, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelExternalForce, "VelocityWrite", velocityB);
        computeShaderFluidSim.Dispatch(kernelExternalForce, resolution / 16, resolution / 16, 1);
    }

    private void VelocityDivergence() {
        int kernelVelocityDivergence = computeShaderFluidSim.FindKernel("VelocityDivergence");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelVelocityDivergence, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelVelocityDivergence, "VelocityWrite", velocityB);
        computeShaderFluidSim.SetTexture(kernelVelocityDivergence, "DivergenceWrite", divergence);
        computeShaderFluidSim.SetTexture(kernelVelocityDivergence, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.Dispatch(kernelVelocityDivergence, resolution / 16, resolution / 16, 1);
    }

    private void InitializePressure() {
        int kernelInitializePressure = computeShaderFluidSim.FindKernel("InitializePressure");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelInitializePressure, "PressureWrite", pressureA);
        computeShaderFluidSim.SetTexture(kernelInitializePressure, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.Dispatch(kernelInitializePressure, resolution / 16, resolution / 16, 1);
    }

    private void PressureJacobi(RenderTexture readRT, RenderTexture writeRT) {
        int kernelPressureJacobi = computeShaderFluidSim.FindKernel("PressureJacobi");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "DivergenceRead", divergence);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "VelocityWrite", velocityB);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "PressureRead", readRT);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "PressureWrite", writeRT);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.Dispatch(kernelPressureJacobi, resolution / 16, resolution / 16, 1);
    }

    private void SubtractGradient() {
        int kernelSubtractGradient = computeShaderFluidSim.FindKernel("SubtractGradient");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "PressureRead", pressureA);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "VelocityWrite", velocityB);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "ObstaclesRead", obstaclesRT);
        computeShaderFluidSim.Dispatch(kernelSubtractGradient, resolution / 16, resolution / 16, 1);
    }

    private void BoundaryConditions() {
        int kernelBoundaryConditions = computeShaderFluidSim.FindKernel("BoundaryConditions");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelBoundaryConditions, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelBoundaryConditions, "VelocityWrite", velocityB);
        computeShaderFluidSim.SetTexture(kernelBoundaryConditions, "PressureRead", pressureA);
        computeShaderFluidSim.SetTexture(kernelBoundaryConditions, "PressureWrite", pressureB);
        computeShaderFluidSim.Dispatch(kernelBoundaryConditions, resolution / 16, resolution / 16, 1);
    }

    private void OnDisable() {
        if(forcePointsCBuffer != null) {
            forcePointsCBuffer.Release();
        }
    }
}
