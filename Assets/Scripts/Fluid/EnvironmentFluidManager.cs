using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentFluidManager : MonoBehaviour {

    public ComputeShader computeShaderFluidSim;
    public Texture2D initialDensityTex;
    public Texture2D initialObstaclesTex;

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

    private RenderTexture divergence;
    private RenderTexture obstacles;

    private Material displayMat;

    public bool tick = false;

    private float prevMousePosX;
    private float prevMousePosY;

    public Agent[] agentsArray;
    public Agent playerAgent;
    public PredatorModule[] predatorsArray;
    public FoodModule[] foodArray;

    private int numForcePoints = 77;
    public ForcePoint[] forcePointsArray;
    public ComputeBuffer forcePointsCBuffer;
    public struct ForcePoint {
        public float posX;
        public float posY;
        public float velX;
        public float velY;
        public float size;
    }
    private int numColorPoints = 77;
    public ColorPoint[] colorPointsArray;
    public ComputeBuffer colorPointsCBuffer;
    public struct ColorPoint {
        public float posX;
        public float posY;
        public Vector3 color;
    }

    public DisplayTexture displayTex = DisplayTexture.densityA;
    public enum DisplayTexture {
        velocityA,
        velocityB,
        pressureA,
        pressureB,
        densityA,
        densityB,
        divergence,
        obstacles
    }
    

	// Use this for initialization
	void Start () {
        CreateTextures();
        
        displayMat = GetComponent<MeshRenderer>().material; //.SetTexture("_MainTex", velocityA);
        Graphics.Blit(initialDensityTex, densityA);
        Graphics.Blit(initialObstaclesTex, obstacles);

        InitializeVelocity();
        //Advection();
        //Graphics.Blit(velocityB, velocityA); // TEMP! slow...

        //BoundaryConditions();
        forcePointsCBuffer = new ComputeBuffer(numForcePoints, sizeof(float) * 5);
        forcePointsArray = new ForcePoint[numForcePoints];
        //Tick();
        CreateForcePoints();
        
    }

    private void CreateForcePoints() {
        if(agentsArray == null || playerAgent == null || predatorsArray == null) {
            return;
        }
        
        //Debug.Log("agentsArray[i].testModule.ownRigidBody2D.velocity: " + agentsArray[0].testModule.ownRigidBody2D.velocity.ToString());
        for(int i = 0; i < agentsArray.Length; i++) {
            ForcePoint agentPoint = new ForcePoint();
            //if(i == 0)
            //    
            // convert world coords to UVs:
            Vector3 agentPos = agentsArray[i].testModule.ownRigidBody2D.transform.position;
            float u = (agentPos.x + 70f) / 140f;
            float v = (agentPos.y + 70f) / 140f;
            agentPoint.posX = u; // UnityEngine.Random.Range(0f, 1f);
            agentPoint.posY = v;
            agentPoint.velX = agentsArray[i].testModule.ownRigidBody2D.velocity.x * 0.01f;
            agentPoint.velY = agentsArray[i].testModule.ownRigidBody2D.velocity.y * 0.01f;
            agentPoint.size = 450f;

            //point.posX = UnityEngine.Random.Range(0f, 1f);
            //point.posY = UnityEngine.Random.Range(0f, 1f);
            //point.velX = UnityEngine.Random.Range(-1f, 1f);
            //point.velY = UnityEngine.Random.Range(-1f, 1f);
            //point.size = UnityEngine.Random.Range(256f, 600f);
            forcePointsArray[i] = agentPoint;

            //Debug.Log("point[" + i.ToString() + ", pos: (" + point.posX.ToString() + ", " + point.posY.ToString() + ") vel: (" + point.velX.ToString() + ", " + point.velY.ToString() + ") size: " + point.size.ToString());
        }

        // Player:
        ForcePoint point = new ForcePoint();
        Vector3 playerPos = playerAgent.testModule.ownRigidBody2D.transform.position;
        float pu = (playerPos.x + 70f) / 140f;
        float pv = (playerPos.y + 70f) / 140f;
        point.posX = pu; 
        point.posY = pv;
        point.velX = playerAgent.testModule.ownRigidBody2D.velocity.x * 0.01f;
        point.velY = playerAgent.testModule.ownRigidBody2D.velocity.y * 0.01f;
        point.size = 400f;        
        forcePointsArray[agentsArray.Length] = point;

        // Predators:
        for (int i = 0; i < predatorsArray.Length; i++) {
            ForcePoint predatorPoint = new ForcePoint();
            Vector3 predatorPos = predatorsArray[i].rigidBody.transform.position;
            float predU = (predatorPos.x + 70f) / 140f;
            float predV = (predatorPos.y + 70f) / 140f;
            predatorPoint.posX = predU; // UnityEngine.Random.Range(0f, 1f);
            predatorPoint.posY = predV;
            predatorPoint.velX = predatorsArray[i].rigidBody.velocity.x * 0.01f;
            predatorPoint.velY = predatorsArray[i].rigidBody.velocity.y * 0.01f;
            predatorPoint.size = 160f;
            forcePointsArray[agentsArray.Length + 1 + i] = predatorPoint;
            //Debug.Log("predatorPoint[" + i.ToString() + ", pos: (" + predatorPoint.posX.ToString() + ", " + predatorPoint.posY.ToString() + ") vel: (" + predatorPoint.velX.ToString() + ", " + predatorPoint.velY.ToString() + ") size: " + predatorPoint.size.ToString());
        }

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

        obstacles = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        obstacles.wrapMode = TextureWrapMode.Repeat;
        obstacles.enableRandomWrite = true;
        obstacles.Create();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        SetDisplayTexture();
        CreateForcePoints();
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
        RefreshColor();
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
        //CreateForcePoints();
        VelocityInjectionPoints();

        //Graphics.Blit(velocityB, velocityA); // TEMP! slow...
        // EnforceBoundariesVelocity:
        //InitializeVelocity();
        EnforceBoundariesVelocity(velocityB, velocityA);  // REplaces BLIT for now!!!
        
        // DIVERGENCE:::::
        VelocityDivergence();  // calculate velocity divergence

        // PRESSURE JACOBI:::::
        //InitializePressure();  // zeroes out initial pressure guess
        int numPressureJacobiIter = 60;
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
        
        computeShaderFluidSim.Dispatch(kernelVelocityInjectionPoints, resolution / 16, resolution / 16, 1);
    }
    private void VelocityInjectionTexture() {
        
    }
    private void DensityInjectionPoints() {
        
        int kernelDensityInjectionPoints = computeShaderFluidSim.FindKernel("DensityInjectionPoints");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetBuffer(kernelDensityInjectionPoints, "ColorPointsCBuffer", colorPointsCBuffer);
        computeShaderFluidSim.SetTexture(kernelDensityInjectionPoints, "DensityRead", densityA);
        computeShaderFluidSim.SetTexture(kernelDensityInjectionPoints, "DensityWrite", densityB);

        computeShaderFluidSim.Dispatch(kernelDensityInjectionPoints, resolution / 16, resolution / 16, 1);
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

    private void EnforceBoundariesVelocity(RenderTexture readTex, RenderTexture writeTex) {
        int kernelEnforceBoundariesVelocity = computeShaderFluidSim.FindKernel("EnforceBoundariesVelocity");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelEnforceBoundariesVelocity, "ObstaclesRead", obstacles);
        computeShaderFluidSim.SetTexture(kernelEnforceBoundariesVelocity, "VelocityRead", readTex);
        computeShaderFluidSim.SetTexture(kernelEnforceBoundariesVelocity, "VelocityWrite", writeTex);
        computeShaderFluidSim.Dispatch(kernelEnforceBoundariesVelocity, resolution / 16, resolution / 16, 1);
    }

    private void VelocityDivergence() {
        int kernelViscosityDivergence = computeShaderFluidSim.FindKernel("VelocityDivergence");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "DivergenceWrite", divergence);
        computeShaderFluidSim.Dispatch(kernelViscosityDivergence, resolution / 16, resolution / 16, 1);
    }

    private void InitializePressure() {
        int kernelInitializePressure = computeShaderFluidSim.FindKernel("InitializePressure");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelInitializePressure, "PressureWrite", pressureA);
        computeShaderFluidSim.Dispatch(kernelInitializePressure, resolution / 16, resolution / 16, 1);
    }

    private void PressureJacobi(RenderTexture readRT, RenderTexture writeRT) {
        int kernelPressureJacobi = computeShaderFluidSim.FindKernel("PressureJacobi");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "ObstaclesRead", obstacles);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "DivergenceRead", divergence);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "PressureRead", readRT);
        computeShaderFluidSim.SetTexture(kernelPressureJacobi, "PressureWrite", writeRT);
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
