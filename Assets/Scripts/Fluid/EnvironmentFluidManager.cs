using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentFluidManager : MonoBehaviour {

    public ComputeShader computeShaderFluidSim;
    public Texture2D initialDensityTex;
    //public Texture2D initialObstaclesTex;
    public Camera obstacleRenderCamera;
    public Camera fluidColorRenderCamera;

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
    private RenderTexture sourceColor;

    private Material displayMat;

    public bool tick = false;

    private float prevMousePosX;
    private float prevMousePosY;

    public Agent[] agentsArray;
    public Agent playerAgent;
    public PredatorModule[] predatorsArray;
    public FoodModule[] foodArray;

    private int numForcePoints = 32;
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

        //Graphics.Blit(initialObstaclesTex, obstacles);
        //obstacleRenderCamera.activeTexture = obstacles;
        obstacleRenderCamera.targetTexture = obstacles;
        Graphics.Blit(initialDensityTex, densityA);
        InitializeVelocity();
        
        forcePointsCBuffer = new ComputeBuffer(numForcePoints, sizeof(float) * 5);
        forcePointsArray = new ForcePoint[numForcePoints];
        
        CreateForcePoints();
        
    }

    private void CreateForcePoints() {
        //if(agentsArray == null || playerAgent == null || predatorsArray == null) {
        //    return;
        //}

        //Debug.Log("agentsArray[i].testModule.ownRigidBody2D.velocity: " + agentsArray[0].testModule.ownRigidBody2D.velocity.ToString());
        int numForcePoints = 32;
        for(int i = 0; i < numForcePoints; i++) {
            ForcePoint agentPoint = new ForcePoint();
            //if(i == 0)
            //    
            // convert world coords to UVs:
            /*float rawVelX = agentsArray[i].testModule.ownRigidBody2D.velocity.x;
            float rawVelY = agentsArray[i].testModule.ownRigidBody2D.velocity.y;
            agentPoint.velX = rawVelX * 0.01f;
            agentPoint.velY = rawVelY * 0.01f;
            Vector3 agentPos = agentsArray[i].testModule.ownRigidBody2D.transform.position;
            float u = (agentPos.x + rawVelX * Time.fixedDeltaTime * 2f + 70f) / 140f;
            float v = (agentPos.y + rawVelX * Time.fixedDeltaTime * 2f + 70f) / 140f;
            agentPoint.posX = u; // UnityEngine.Random.Range(0f, 1f);
            agentPoint.posY = v;
            agentPoint.size = 450f;*/

            float forceStrength = 0.15f;
            agentPoint.posX = UnityEngine.Random.Range(0f, 1f);
            agentPoint.posY = UnityEngine.Random.Range(0f, 1f);
            agentPoint.velX = UnityEngine.Random.Range(-1f, 1f) * forceStrength;
            agentPoint.velY = UnityEngine.Random.Range(-1f, 1f) * forceStrength;
            agentPoint.size = UnityEngine.Random.Range(64f, 128f);
            forcePointsArray[i] = agentPoint;

            //Debug.Log("point[" + i.ToString() + ", pos: (" + point.posX.ToString() + ", " + point.posY.ToString() + ") vel: (" + point.velX.ToString() + ", " + point.velY.ToString() + ") size: " + point.size.ToString());
        }

        // Player:
        /*
        ForcePoint point = new ForcePoint();
        Vector2 pVel = playerAgent.testModule.ownRigidBody2D.velocity;
        Vector3 playerPos = playerAgent.testModule.ownRigidBody2D.transform.position;
        float pu = (playerPos.x + pVel.x * Time.fixedDeltaTime * 2f + 70f) / 140f;
        float pv = (playerPos.y + pVel.y * Time.fixedDeltaTime * 2f + 70f) / 140f;
        point.posX = pu; 
        point.posY = pv;
        point.velX = pVel.x * 0.01f;
        point.velY = pVel.y * 0.01f;
        point.size = 400f;        
        forcePointsArray[agentsArray.Length] = point;*/

        // Predators:
        /*
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
        }*/

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

        sourceColor = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        sourceColor.wrapMode = TextureWrapMode.Repeat;
        sourceColor.enableRandomWrite = true;
        sourceColor.Create();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        /*SetDisplayTexture();
        CreateForcePoints();
        if (tick) {   // So I can step through the program slowly at first
            //tick = false;
            Tick();

            //Debug.Log(new Vector2(Input.mousePosition.x - prevMousePosX, Input.mousePosition.y - prevMousePosY).ToString());
        }

        prevMousePosX = Input.mousePosition.x;
        prevMousePosY = Input.mousePosition.y;*/
    }

    public void Run() {
        obstacleRenderCamera.targetTexture = obstacles;
        fluidColorRenderCamera.targetTexture = sourceColor;
        SetDisplayTexture();
        //CreateForcePoints();
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

        // Lerp towards sourceTexture color:
        RefreshColor();

        // ADVECTION:::::
        Advection();
        //Graphics.Blit(velocityB, velocityA); // TEMP! slow...
        VelocityInjectionPoints(velocityB, velocityA);

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
        VelocityDivergence(velocityA);  // calculate velocity divergence

        // PRESSURE JACOBI:::::
        //InitializePressure();  // zeroes out initial pressure guess // doesn't seem to produce great results
        int numPressureJacobiIter = 40;
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
    }

    private void RefreshColor() { 
        int kernelRefreshColor = computeShaderFluidSim.FindKernel("RefreshColor");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelRefreshColor, "SourceColorTex", sourceColor);
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
        computeShaderFluidSim.SetTexture(kernelAdvection, "ObstaclesRead", obstacles);
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

    private void VelocityInjectionPoints(RenderTexture readRT, RenderTexture writeRT) {
        
        int kernelVelocityInjectionPoints = computeShaderFluidSim.FindKernel("VelocityInjectionPoints");
        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "ObstaclesRead", obstacles);
        computeShaderFluidSim.SetBuffer(kernelVelocityInjectionPoints, "ForcePointsCBuffer", forcePointsCBuffer);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "VelocityRead", readRT);
        computeShaderFluidSim.SetTexture(kernelVelocityInjectionPoints, "VelocityWrite", writeRT);
        
        computeShaderFluidSim.Dispatch(kernelVelocityInjectionPoints, resolution / 16, resolution / 16, 1);
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
    
    private void VelocityDivergence(RenderTexture readTex) {
        int kernelViscosityDivergence = computeShaderFluidSim.FindKernel("VelocityDivergence");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "ObstaclesRead", obstacles);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "VelocityRead", readTex);
        computeShaderFluidSim.SetTexture(kernelViscosityDivergence, "DivergenceWrite", divergence);
        computeShaderFluidSim.Dispatch(kernelViscosityDivergence, resolution / 16, resolution / 16, 1);
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
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "ObstaclesRead", obstacles);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "VelocityRead", velocityA);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "PressureRead", pressureA);
        computeShaderFluidSim.SetTexture(kernelSubtractGradient, "VelocityWrite", velocityB);
        computeShaderFluidSim.Dispatch(kernelSubtractGradient, resolution / 16, resolution / 16, 1);
    }

    private void OnDisable() {
        if(forcePointsCBuffer != null) {
            forcePointsCBuffer.Release();
        }
    }
}
