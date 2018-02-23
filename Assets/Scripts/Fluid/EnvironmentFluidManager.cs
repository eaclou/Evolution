using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentFluidManager : MonoBehaviour {

    public Camera mainCam;
    public ComputeShader computeShaderFluidSim;
    public Texture2D initialDensityTex;
    //public Texture2D initialObstaclesTex;
    public Camera obstacleRenderCamera;
    public Camera fluidColorRenderCamera;
    public GameObject debugGO;

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

    public Vector2[] agentFluidVelocitiesArray;
    public Vector3[] agentPositionsArray;  // z coord holds radius of object
    public Vector2[] foodFluidVelocitiesArray;
    public Vector3[] foodPositionsArray;
    public Vector2[] predatorFluidVelocitiesArray;
    public Vector3[] predatorPositionsArray;

    private int numFloatyBits = 1024 * 32;
    private ComputeBuffer floatyBitsCBuffer;
    private ComputeBuffer quadVerticesCBuffer;
    public Material floatyBitsDisplayMat;

    private int numForcePoints = 12;
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

    public struct AgentSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
    }
    public struct FoodSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public float scale;
        public Vector3 foodAmount;
    }
    public struct PredatorSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public float scale;
    }
    public AgentSimData[] agentSimDataArray;
    public FoodSimData[] foodSimDataArray;
    public PredatorSimData[] predatorSimDataArray;
    public ComputeBuffer agentSimDataCBuffer;
    public ComputeBuffer foodSimDataCBuffer;
    public ComputeBuffer predatorSimDataCBuffer;

    public Material agentProceduralDisplayMat;
    public Material foodProceduralDisplayMat;
    public Material predatorProceduralDisplayMat;

    // Use this for initialization
    void Start () {
        obstacleRenderCamera.enabled = false;
        fluidColorRenderCamera.enabled = false;
        agentFluidVelocitiesArray = new Vector2[64];
        agentPositionsArray = new Vector3[64];
        foodFluidVelocitiesArray = new Vector2[36];
        foodPositionsArray = new Vector3[36];
        predatorFluidVelocitiesArray = new Vector2[12];
        predatorPositionsArray = new Vector3[12];

        agentSimDataArray = new AgentSimData[64];
        for(int i = 0; i < agentSimDataArray.Length; i++) {
            agentSimDataArray[i] = new AgentSimData();
        }
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 6);

        foodSimDataArray = new FoodSimData[36];
        for (int i = 0; i < foodSimDataArray.Length; i++) {
            foodSimDataArray[i] = new FoodSimData();
        }
        foodSimDataCBuffer = new ComputeBuffer(foodSimDataArray.Length, sizeof(float) * 8);

        predatorSimDataArray = new PredatorSimData[12];
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            predatorSimDataArray[i] = new PredatorSimData();
        }
        predatorSimDataCBuffer = new ComputeBuffer(predatorSimDataArray.Length, sizeof(float) * 5);

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


        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });
        Vector2[] floatyBitsInitPos = new Vector2[numFloatyBits];
        floatyBitsCBuffer = new ComputeBuffer(numFloatyBits, sizeof(float) * 4);
        for(int i = 0; i < numFloatyBits; i++) {
            floatyBitsInitPos[i] = new Vector4(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 1f, 0f);
        }
        floatyBitsCBuffer.SetData(floatyBitsInitPos);
        int kernelSimFloatyBits = computeShaderFluidSim.FindKernel("SimFloatyBits");
        computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);

        floatyBitsDisplayMat.SetPass(0);
        floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
        floatyBitsDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        agentProceduralDisplayMat.SetPass(0);
        agentProceduralDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        agentProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        foodProceduralDisplayMat.SetPass(0);
        foodProceduralDisplayMat.SetBuffer("foodSimDataCBuffer", foodSimDataCBuffer);
        foodProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        predatorProceduralDisplayMat.SetPass(0);
        predatorProceduralDisplayMat.SetBuffer("predatorSimDataCBuffer", predatorSimDataCBuffer);
        predatorProceduralDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

    }

    private void OnRenderObject() {
        if(Camera.current == mainCam) {
            floatyBitsDisplayMat.SetPass(0);
            floatyBitsDisplayMat.SetBuffer("floatyBitsCBuffer", floatyBitsCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, floatyBitsCBuffer.count);

            agentProceduralDisplayMat.SetPass(0);
            agentProceduralDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentSimDataCBuffer.count);

            foodProceduralDisplayMat.SetPass(0);
            foodProceduralDisplayMat.SetBuffer("foodSimDataCBuffer", foodSimDataCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, foodSimDataCBuffer.count);

            predatorProceduralDisplayMat.SetPass(0);
            predatorProceduralDisplayMat.SetBuffer("predatorSimDataCBuffer", predatorSimDataCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, predatorSimDataCBuffer.count);
        }        
    }

    private void CreateForcePoints() {
        
        for(int i = 0; i < numForcePoints; i++) {
            ForcePoint agentPoint = new ForcePoint();
            
            float forceStrength = 0.2f;
            agentPoint.posX = UnityEngine.Random.Range(0f, 1f);
            agentPoint.posY = UnityEngine.Random.Range(0f, 1f);
            agentPoint.velX = UnityEngine.Random.Range(-1f, 1f) * forceStrength;
            agentPoint.velY = UnityEngine.Random.Range(-1f, 1f) * forceStrength;
            agentPoint.size = UnityEngine.Random.Range(64f, 128f);
            forcePointsArray[i] = agentPoint;
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

    void Update() {
        
    }

    public Vector2 GetFluidVelocityAtPosition(Vector2 uv) {
        ComputeBuffer velocityValuesCBuffer = new ComputeBuffer(1, sizeof(float) * 2);

        Vector2[] fluidVelocityValues = new Vector2[1];
        //fluidVelocityValues [0] = Vector2.zero;
        computeShaderFluidSim.SetFloat("_Time", Time.time);
        computeShaderFluidSim.SetFloat("_ForceMagnitude", forceMagnitude);
        computeShaderFluidSim.SetFloat("_Viscosity", viscosity);
        computeShaderFluidSim.SetFloat("_Damping", damping);
        computeShaderFluidSim.SetFloat("_ForceSize", invBrushSize);
        computeShaderFluidSim.SetFloat("_ColorRefreshAmount", colorRefreshAmount);
        
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
    }

    public Vector2[] GetFluidVelocityAtObjectPositions(Vector3[] positionsArray) {

        ComputeBuffer objectPositionsCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 3);
        ComputeBuffer velocityValuesCBuffer = new ComputeBuffer(positionsArray.Length, sizeof(float) * 2);

        Vector2[] objectVelocitiesArray = new Vector2[positionsArray.Length];

        //for(int i = 0; i < 64; i++) {
        //    Vector3 agentPos = agentsArray[i].testModule.ownRigidBody2D.position;
        //    agentPositionsArray[i] = new Vector2((agentPos.x + 70f) / 140f, (agentPos.y + 70f) / 140f);
        //}

        objectPositionsCBuffer.SetData(positionsArray);
        
        //computeShaderFluidSim.SetFloat("_Time", Time.time);
        //computeShaderFluidSim.SetFloat("_ForceMagnitude", forceMagnitude);
        //computeShaderFluidSim.SetFloat("_Viscosity", viscosity);
        //computeShaderFluidSim.SetFloat("_Damping", damping);
        //computeShaderFluidSim.SetFloat("_ForceSize", invBrushSize);
        //computeShaderFluidSim.SetFloat("_ColorRefreshAmount", colorRefreshAmount);

        int kernelGetObjectVelocities = computeShaderFluidSim.FindKernel("GetObjectVelocities");
        //computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        //computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);        
        computeShaderFluidSim.SetBuffer(kernelGetObjectVelocities, "ObjectPositionsCBuffer", objectPositionsCBuffer);
        computeShaderFluidSim.SetBuffer(kernelGetObjectVelocities, "VelocityValuesCBuffer", velocityValuesCBuffer);
        computeShaderFluidSim.SetTexture(kernelGetObjectVelocities, "VelocityRead", velocityA);
        computeShaderFluidSim.Dispatch(kernelGetObjectVelocities, positionsArray.Length, 1, 1);

        velocityValuesCBuffer.GetData(objectVelocitiesArray);

        velocityValuesCBuffer.Release();
        objectPositionsCBuffer.Release();

        return objectVelocitiesArray;
        
    }

    public void SimFloatyBits() {
        int kernelSimFloatyBits = computeShaderFluidSim.FindKernel("SimFloatyBits");

        computeShaderFluidSim.SetFloat("_TextureResolution", (float)resolution);
        computeShaderFluidSim.SetFloat("_DeltaTime", deltaTime);
        computeShaderFluidSim.SetFloat("_InvGridScale", invGridScale);
        computeShaderFluidSim.SetBuffer(kernelSimFloatyBits, "FloatyBitsCBuffer", floatyBitsCBuffer);
        computeShaderFluidSim.SetTexture(kernelSimFloatyBits, "VelocityRead", velocityA);        
        computeShaderFluidSim.Dispatch(kernelSimFloatyBits, floatyBitsCBuffer.count / 1024, 1, 1);
    }

    public void SetSimDataArrays() {
        
        for(int i = 0; i < agentSimDataArray.Length; i++) {            
            agentSimDataArray[i].worldPos = new Vector2(agentsArray[i].transform.position.x, agentsArray[i].transform.position.y);
            agentSimDataArray[i].velocity = new Vector2(agentsArray[i].testModule.ownRigidBody2D.velocity.x, agentsArray[i].testModule.ownRigidBody2D.velocity.y);
            agentSimDataArray[i].heading = new Vector2(0f, 1f); // Update later -- store inside Agent class?            
        }
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
    }

    public void Run() {
        Vector3 debugPos = playerAgent.testModule.ownRigidBody2D.position;
        debugGO.transform.position = debugPos;
        obstacleRenderCamera.targetTexture = obstacles;
        fluidColorRenderCamera.targetTexture = sourceColor;
        obstacleRenderCamera.Render();
        fluidColorRenderCamera.Render();

        // Update AGENT positions & velocities:
        for(int i = 0; i < agentPositionsArray.Length; i++) {
            Vector3 agentPos = agentsArray[i].testModule.ownRigidBody2D.position;
            // z coord holds radius of object   
            agentPositionsArray[i] = new Vector3((agentPos.x + 70f) / 140f, (agentPos.y + 70f) / 140f, 0.005357f); //... 0.6/140 ...
        }
        agentFluidVelocitiesArray = GetFluidVelocityAtObjectPositions(agentPositionsArray);
        // Update FOOD positions & velocities:
        //Debug.Log("foodPositionsArray.Length " + foodPositionsArray.Length.ToString() + ", foodArray: " + foodArray.Length.ToString());
        for (int i = 0; i < foodPositionsArray.Length; i++) {
            Vector3 foodPos = foodArray[i].transform.position;
            float sampleRadius = (foodArray[i].curScale + 0.1f) / 140f;
            foodPositionsArray[i] = new Vector3((foodPos.x + 70f) / 140f, (foodPos.y + 70f) / 140f, sampleRadius); // z coord holds radius of object   
        }
        foodFluidVelocitiesArray = GetFluidVelocityAtObjectPositions(foodPositionsArray);
        // Update PREDATOR positions & velocities:
        for (int i = 0; i < predatorPositionsArray.Length; i++) {
            Vector3 predatorPos = predatorsArray[i].transform.position;
            float sampleRadius = (predatorsArray[i].curScale + 0.1f) / 140f;
            predatorPositionsArray[i] = new Vector3((predatorPos.x + 70f) / 140f, (predatorPos.y + 70f) / 140f, sampleRadius); // z coord holds radius of object   
        }
        predatorFluidVelocitiesArray = GetFluidVelocityAtObjectPositions(predatorPositionsArray);

        for (int i = 0; i < agentsArray.Length; i++) {
            agentsArray[i].testModule.ownRigidBody2D.AddForce(agentFluidVelocitiesArray[i] * 42f, ForceMode2D.Impulse);
            //agentsArray[i].testModule.ownRigidBody2D.velocity = Vector2.Lerp(agentsArray[i].testModule.ownRigidBody2D.velocity, agentFluidVelocitiesArray[i] * 160f, 0.85f);
        }
        for (int i = 0; i < foodArray.Length; i++) {
            foodArray[i].GetComponent<Rigidbody2D>().AddForce(foodFluidVelocitiesArray[i] * 15f * foodArray[i].GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse);
        }
        for (int i = 0; i < predatorsArray.Length; i++) {
            predatorsArray[i].rigidBody.AddForce(predatorFluidVelocitiesArray[i] * 32f * predatorsArray[i].rigidBody.mass, ForceMode2D.Impulse);
        }

        SetSimDataArrays(); // Send data about gameState to GPU for display

        SetDisplayTexture();
        //CreateForcePoints();
        if (tick) {   // So I can step through the program slowly at first            
            Tick();            
        }

        

        //Vector2 playerUV = new Vector2((debugPos.x + 70f) / 140f, (debugPos.y + 70f) / 140f);
        //GetFluidVelocityAtPosition(playerUV);

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

        SimFloatyBits();
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
        if (floatyBitsCBuffer != null) {
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
        //agentSimDataCBuffer
    }
}
