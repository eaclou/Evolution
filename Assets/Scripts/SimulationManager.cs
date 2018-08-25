using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

// The meat of the Game, controls the primary simulation/core logic gameplay Loop
public class SimulationManager : MonoBehaviour {

    public UIManager uiManager;
    public EnvironmentFluidManager environmentFluidManager;
    public TheRenderKing theRenderKing;
    public CameraManager cameraManager;
    public SettingsManager settingsManager;
    public SimulationStateData simStateData;
    public AudioManager audioManager;
    public StartPositionsPresetLists startPositionsPresets;
    public ComputeShader computeShaderNutrientMap;
    public ComputeShader computeShaderFoodParticles;

    public bool isQuickStart = true;

    public float curPlayerMutationRate = 0.25f;  // UI-based value, giving player control over mutation frequency with one parameter

    private bool isLoading = false;
    private bool loadingComplete = false;
    public bool _LoadingComplete
    {
        get
        {
            return loadingComplete;
        }
        set
        {

        }
    }
    private bool simulationWarmUpComplete = false;
    public bool _SimulationWarmUpComplete
    {
        get
        {
            return simulationWarmUpComplete;
        }
        set
        {

        }
    }
    private int numWarmUpTimeSteps = 10;
    private int currentWarmUpTimeStep = 0;

    private static float mapSize = 256f;  // This determines scale of environment, size of FluidSim plane!!! Important!
    public static float _MapSize
    {
        get
        {
            return mapSize;
        }
        set
        {

        }
    }
    private int agentGridCellResolution = 1;  // How much to subdivide the map in order to detect nearest-neighbors more efficiently --> to not be O(n^2)
    public MapGridCell[][] mapGridCellArray;

    private int numAgents = 64;
    public int _NumAgents {
        get
        {
            return numAgents;
        }
        set
        {
            numAgents = value;
        }
    }
    //private int agentGenomePoolSize = 64;  // spawned as Agents that live until they are killed naturally, tested on Fitness Function
    //public Agent playerAgent; // Should just be a reference to whichever #Agent that the player is currently controlling
    // playerAgent is always agentsArray[0]
    public Agent[] agentsArray;
    //public BodyGenome bodyGenomeTemplate; // .... refactor?
    public AgentGenome[] agentGenomePoolArray;
    private AgentGenome[] savedGenomePoolArray1;
    private AgentGenome[] savedGenomePoolArray2;
    private AgentGenome[] savedGenomePoolArray3;
    public FoodGenome[] foodGenomePoolArray;
    private FoodGenome foodGenomeAnimalCorpse;
    public FoodChunk[] foodArray;
    private int numFood = 48;
    public int _NumFood {
        get
        {
            return numFood;
        }
        set
        {
            numFood = value;
        }
    }    
    //public FoodChunk[] foodDeadAnimalArray;
    public PredatorModule[] predatorArray;
    private int numPredators = 64;
    public int _NumPredators {
        get
        {
            return numPredators;
        }
        set
        {
            numPredators = value;
        }
    }
   
    public float[] rawFitnessScoresArray;
    private int[] rankedIndicesList;
    private float[] rankedFitnessList;
    public int numAgentsBorn = 0;
    public int currentOldestAgent = 0;
    public int recordPlayerAge = 0;
    public int lastPlayerScore = 0;
    //public bool playerIsDead = false;

        // Species
    private int numSpecies = 4;
    public float[] speciesAvgFoodEaten;
    public Vector2[] speciesAvgSizes;
    public float[] speciesAvgMouthTypes;

    public Vector4 statsAvgGlobalNutrients;
    public float statsAvgMutationRate;

    private float curGlobalNutrients = 0f;
    private float curGlobalFoodParticles = 0f;
    
    public int recordBotAge = 0;
    public float[] rollingAverageAgentScoresArray;
    public List<Vector4> statsLifespanEachGenerationList;
    public List<Vector4> statsBodySizesEachGenerationList;
    public List<Vector4> statsFoodEatenEachGenerationList;
    public List<Vector4> statsPredationEachGenerationList;
    public List<Vector4> statsNutrientsEachGenerationList;
    public List<float> statsMutationEachGenerationList;
    public float agentAvgRecordScore = 1f;
    public int curApproxGen = 1;

    public int numInitialHiddenNeurons = 16;


    private int foodGridResolution = 32;
    public FoodGridCell[][] foodGrid;
    public float[][] foodGridSwapArray;
    public Texture2D debugFoodTexture;

    private int nutrientMapResolution = 32;
    public RenderTexture nutrientMapRT1;
    public RenderTexture nutrientMapRT2;
    private Vector4[] nutrientSamplesArray;
    private Vector4[] nutrientEatAmountsArray;

    private RenderTexture tempTex16;
    private RenderTexture tempTex8;
    private RenderTexture tempTex4;
    private RenderTexture tempTex2;
    private RenderTexture tempTex1;

    private ComputeBuffer nutrientSamplesCBuffer;

    private int numFoodParticles = 1024;
    public ComputeBuffer foodParticlesCBuffer;
    private ComputeBuffer foodParticlesCBufferSwap;
    //private ComputeBuffer[] foodParticlesMipsCBufferArray;  // smaller sizes used as temporary data while sorting/computing totals
    private RenderTexture foodParticlesNearestCritters1024;
    private RenderTexture foodParticlesNearestCritters32;
    private RenderTexture foodParticlesNearestCritters1;
    private ComputeBuffer closestFoodParticlesDataCBuffer;
    public FoodParticleData[] closestFoodParticlesDataArray;
    private ComputeBuffer foodParticlesEatAmountsCBuffer;
    public float[] foodParticlesEatAmountsArray;
    private ComputeBuffer foodParticlesMeasure32;
    private ComputeBuffer foodParticlesMeasure1;
    private FoodParticleData[] foodParticleMeasurementTotalsData;

    private int numAgentsProcessed = 0;

    public struct FoodParticleData {
        public int index;
        public int critterIndex;
        public float isSwallowed;   // 0 = normal, 1 = in critter's belly
        public float digestedAmount;  // 0 = freshly eaten, 1 = fully dissolved/shrunk
        public Vector2 worldPos;
        public float radius;
        public float foodAmount;
        public float active;  // not disabled
        public float refactoryAge;
    }



    //public bool isTrainingPersistent = false; // RENAME ONCE FUNCTIONAL
    //private float lastHorizontalInput = 0f;
    //private float lastVerticalInput = 0f;
    //private int timeStepCounter = 1; // needed???
    //public int curGen;
    //public float avgFitnessLastGen = 0f;
    //public float bestFitnessScore = 0f;    
    //private bool firstTimeTraining = true; // might not be needed    
    //private int idleFramesToBotControl = 10; // probably no longer needed....
    //private int idleFramesCounter = 0;
    //public int botToHumanControlTransitionFrameCount = 8;
    //public int humanToBotControlTransitionFrameCount = 30;
     // needed?
    // GRID SEARCH!!!
    // Temporarily out of use:::
    //public GridSearchManager gridSearchManager;
    //public bool isGridSearching = false;
    // need to be able to update Agent's Brain on the fly?  --- but needs to access the Module to set up inputs/outputs???
    // Ability to run a Brain Headless (without instantiating an Agent?)

    #region loading   // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& LOADING LOADING LOADING &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    public void TickLoading() {
        // "Hey, I'm Loadin' Here!!!"
        
        

        // Has basic loading phase completed?
        if(loadingComplete) {
            // if so, warming up:
            uiManager.loadingProgress = (float)currentWarmUpTimeStep / (float)numWarmUpTimeSteps;
        
            if(currentWarmUpTimeStep >= numWarmUpTimeSteps) {
                Debug.Log("WarmUp Complete!!! ");
                simulationWarmUpComplete = true;

                //LoadTrainingData();

                //turn off menu music:
                audioManager.TurnOffMenuAudioGroup();

                // Populate renderKingBuffers:
                //for(int i = 0; i < numAgents; i++) {
                //    theRenderKing.UpdateAgentBodyStrokesBuffer(i); // hacky fix but seems to work...
                //}

                RespawnPlayer(); // Needed???? *****
                
            }
            else {
                //Debug.Log("WarmUp Step " + currentWarmUpTimeStep.ToString());
                TickSimulation();

                // Fade out menu music:
                audioManager.AdjustMenuVolume(1f - ((float)currentWarmUpTimeStep / (float)numWarmUpTimeSteps));
                // Fade In gameplay audio:
                audioManager.AdjustGameplayVolume((float)currentWarmUpTimeStep / (float)numWarmUpTimeSteps);

                currentWarmUpTimeStep++;
            }

            
        }
        else {
            // Check if already loading or if this is the first time startup:
            if(isLoading) {
                // loading coroutine already underway.. chill out and relax
            }
            else {
                // Start Loading coroutine!!!!:
                isLoading = true;
                StartCoroutine(LoadingNewSimulation());

                // Turn on Gameplay Audio:
                audioManager.TurnOnGameplayAudioGroup();
            }  
        }  
    }

    IEnumerator LoadingNewSimulation() {
        Debug.Log("LoadingNewSimulation() ");
        //const float maxComputeTimePerFrame = 0.01f; // 10 milliseconds per frame
        //float startTime = Time.realtimeSinceStartup;
        //float elapsedTime;

        // Do some stuff:: LOAD!
        LoadingInitializeCoreSimulationState();  // creates arrays and stuff for the (hopefully)only time

        // Fitness Stuffs:::
        LoadingSetUpFitnessStorage();

        yield return null;

        // Initialize Agents:
        LoadingInstantiateAgents();  // Fills the AgentsArray, Instantiates Agent Objects (MonoBehaviors + GameObjects)
        //agentsArray[0].humanControlled = true;
        //agentsArray[0].humanControlLerp = 1f;
        LoadingInitializeAgentsFromGenomes(); // This was "RespawnAgents()" --  Used to actually place the Agent in the game in a random spot and set the Agent's atributes ffrom its genome

        // Initialize Food:
        LoadingInitializeFoodParticles();
        LoadingInitializeFoodGrid();
        LoadingInstantiateFood();
        LoadingInitializeFoodFromGenome();
        // Initialize Predators:
        LoadingInstantiatePredators();
        LoadingInitializePredatorsFromGenome();


        yield return null;

        // Load pre-saved genomes:
        LoadingLoadGenepoolFiles();
       
        yield return null;

        // **** How to handle sharing simulation data between different Managers???
        // Once Agents, Food, etc. are established, Initialize the Fluid:
        LoadingInitializeFluidSim();


        yield return null;

        //simStateData.PopulateSimDataArrays(this); // testing if this works...
        // Wake up the Render King and prepare him for the day ahead, proudly ruling over Renderland.
        LoadingGentlyRouseTheRenderMonarchHisHighnessLordOfPixels();
        
        // TEMP!!! ****
        for(int i = 0; i < numAgents; i++) {
            theRenderKing.UpdateAgentWidthsTexture(agentsArray[i]);
        }
        
        yield return null;
                
        LoadingHookUpFluidAndRenderKing();  // fluid needs refs to RK's obstacle/color cameras' RenderTextures!
        // ***** ^^^^^ Might need to call this every frame???

        // Hook up Camera to data -- fill out CameraManager class
        //cameraManager.SetTarget(cameraManager.targetAgent, cameraManager.targetCritterIndex);
        //cameraManager.targetTransform = agentsArray[cameraManager.targetCritterIndex].bodyGO.transform;
        // ***** Hook up UI to proper data or find a way to handle that ****
        // possibly just top-down let cameraManager read simulation data
        LoadingHookUpUIManager();
        // Separate class to hold all simulation State Data?

        yield return null;

        LoadingInitializeGridCells();
        // Populates GridCells with their contents (agents/food/preds)
        LoadingFillGridCells();
        LoadingHookUpModules();
        //simStateData.PopulateSimDataArrays(this); // might be able to just call everytime in Tick()

        //elapsedTime = Time.realtimeSinceStartup - startTime;
        //if(elapsedTime > maxComputeTimePerFrame) {
        //    yield return null;
        //}

        //yield return new WaitForSeconds(5f); // TEMP!!!

        environmentFluidManager.UpdateSimulationClimate(0);


        // Done - will be detected by GameManager next frame
        loadingComplete = true;

        Debug.Log("LOADING COMPLETE - Starting WarmUp!");
    }

    private void LoadingInitializeCoreSimulationState() {
        // allocate memory and initialize data structures, classes, arrays, etc.

        settingsManager.Initialize();

        //debugScores:
        rollingAverageAgentScoresArray = new float[numSpecies];

        speciesAvgFoodEaten = new float[numSpecies];
        speciesAvgSizes = new Vector2[numSpecies];
        speciesAvgMouthTypes = new float[numSpecies];
        
        //bodyGenomeTemplate = new BodyGenome();
        //bodyGenomeTemplate.InitializeGenomeAsDefault(); // ****  Come back to this and make sure using bodyGenome in a good way

        
        LoadingInitializePopulationGenomes();

        if(isQuickStart) {
            Debug.Log("QUICK START! Loading Pre-Trained Genomes!");
            LoadTrainingData();
        }

        simStateData = new SimulationStateData(this);
    }
    private void LoadingInitializePopulationGenomes() {
        agentGenomePoolArray = new AgentGenome[numAgents];

        for (int i = 0; i < agentGenomePoolArray.Length; i++) {   // Create initial Population Supervised Learners
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.GenerateInitialRandomBodyGenome();
            //agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);  // OLD
            agentGenome.InitializeRandomBrainFromCurrentBody(settingsManager.mutationSettingsPersistent.initialConnectionChance, numInitialHiddenNeurons);
            agentGenomePoolArray[i] = agentGenome;
        }

        // Sort Fitness Scores Persistent:
        rankedIndicesList = new int[numAgents];
        rankedFitnessList = new float[numAgents];

        for (int i = 0; i < rankedIndicesList.Length; i++) {
            rankedIndicesList[i] = i;
            rankedFitnessList[i] = 1f;
        }


        // FOOD:
        foodGenomePoolArray = new FoodGenome[numFood];

        for(int i = 0; i < foodGenomePoolArray.Length; i++) {
            FoodGenome foodGenome = new FoodGenome(i);

            foodGenome.InitializeAsRandomGenome();

            foodGenomePoolArray[i] = foodGenome;
        }

        foodGenomeAnimalCorpse = new FoodGenome(-1);
        foodGenomeAnimalCorpse.InitializeAsRandomGenome();
    }
    private void LoadingInitializeFluidSim() {
        environmentFluidManager.InitializeFluidSystem();
    }
    private void LoadingGentlyRouseTheRenderMonarchHisHighnessLordOfPixels() {
        theRenderKing.InitializeRiseAndShine(this);
    }
    private void LoadingInstantiateAgents() {

        // Re-Factor this to no longer use Prefab?

        // Instantiate AI Agents
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = new GameObject("Agent" + i.ToString());
            Agent newAgent = agentGO.AddComponent<Agent>();  // Script placed on Prefab already   
            newAgent.speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
            agentsArray[i] = newAgent; // Add to stored list of current Agents            
        }


        // OLD:
        /*
        string assetURL = "Prefabs/AgentPrefab";

        // Instantiate AI Agents
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
            agentGO.name = "Agent" + i.ToString();
            //agentGO.transform.localScale = new Vector3(agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio.x, agentGenomePoolArray[i].bodyGenome.sizeAndAspectRatio.y, 1f);
            Agent newAgent = agentGO.GetComponent<Agent>();  // Script placed on Prefab already            
            agentsArray[i] = newAgent; // Add to stored list of current Agents            
        }

        // ************** TEMP!!! *******************
        // Set playerAgent to agent[0]:
        playerAgent = agentsArray[0];   // ***** RE-Factor!!!!! ****** when re-implementing player control! **
        */
    }
    private void LoadingInitializeAgentsFromGenomes() {        
        for (int i = 0; i < numAgents; i++) {
            
            int speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
            agentsArray[i].InitializeAgentFromGenome(i, agentGenomePoolArray[i], GetInitialAgentSpawnPosition(speciesIndex));            
        }
    }
    private void LoadingInitializeFoodParticles() {

        foodParticlesCBuffer = new ComputeBuffer(numFoodParticles, sizeof(float) * 8 + sizeof(int) * 2);
        foodParticlesCBufferSwap = new ComputeBuffer(numFoodParticles, sizeof(float) * 8 + sizeof(int) * 2);
        FoodParticleData[] foodParticlesArray = new FoodParticleData[numFoodParticles];

        float minParticleSize = settingsManager.avgFoodParticleRadius / settingsManager.foodParticleRadiusVariance;
        float maxParticleSize = settingsManager.avgFoodParticleRadius * settingsManager.foodParticleRadiusVariance;

        for(int i = 0; i < foodParticlesCBuffer.count; i++) {
            FoodParticleData data = new FoodParticleData();
            data.index = i;            
            data.worldPos = new Vector2(UnityEngine.Random.Range(0f, mapSize), UnityEngine.Random.Range(0f, mapSize));

            data.radius = UnityEngine.Random.Range(minParticleSize, maxParticleSize);
            data.foodAmount = data.radius * data.radius * Mathf.PI * settingsManager.foodParticleNutrientDensity;
            data.active = 1f;
            data.refactoryAge = 0f;
            foodParticlesArray[i] = data;
        }

        foodParticlesCBuffer.SetData(foodParticlesArray);
        foodParticlesCBufferSwap.SetData(foodParticlesArray);

        foodParticlesNearestCritters1024 = new RenderTexture(numFoodParticles, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        foodParticlesNearestCritters1024.wrapMode = TextureWrapMode.Clamp;
        foodParticlesNearestCritters1024.filterMode = FilterMode.Point;
        foodParticlesNearestCritters1024.enableRandomWrite = true;        
        foodParticlesNearestCritters1024.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    

        foodParticlesNearestCritters32 = new RenderTexture(32, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        foodParticlesNearestCritters32.wrapMode = TextureWrapMode.Clamp;
        foodParticlesNearestCritters32.filterMode = FilterMode.Point;
        foodParticlesNearestCritters32.enableRandomWrite = true;        
        foodParticlesNearestCritters32.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***   

        foodParticlesNearestCritters1 = new RenderTexture(1, numAgents, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        foodParticlesNearestCritters1.wrapMode = TextureWrapMode.Clamp;
        foodParticlesNearestCritters1.filterMode = FilterMode.Point;
        foodParticlesNearestCritters1.enableRandomWrite = true;        
        foodParticlesNearestCritters1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        closestFoodParticlesDataArray = new FoodParticleData[numAgents];
        closestFoodParticlesDataCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 5 + sizeof(int) * 1);

        foodParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        foodParticlesEatAmountsArray = new float[numAgents];

        foodParticleMeasurementTotalsData = new FoodParticleData[1];
        foodParticlesMeasure32 = new ComputeBuffer(32, sizeof(float) * 5 + sizeof(int) * 1);
        foodParticlesMeasure1 = new ComputeBuffer(1, sizeof(float) * 5 + sizeof(int) * 1);
    }
    private void LoadingInitializeFoodGrid() {
        foodGrid = new FoodGridCell[foodGridResolution][];
        foodGridSwapArray = new float[foodGridResolution][];

        debugFoodTexture = new Texture2D(foodGridResolution, foodGridResolution);
        debugFoodTexture.filterMode = FilterMode.Point;
        
                
        nutrientMapRT1 = new RenderTexture(nutrientMapResolution, nutrientMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        nutrientMapRT1.wrapMode = TextureWrapMode.Clamp;
        nutrientMapRT1.filterMode = FilterMode.Bilinear;
        nutrientMapRT1.enableRandomWrite = true;
        //nutrientMapRT1.useMipMap = true;
        nutrientMapRT1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***    

        nutrientMapRT2 = new RenderTexture(nutrientMapResolution, nutrientMapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        nutrientMapRT2.wrapMode = TextureWrapMode.Clamp;
        nutrientMapRT2.enableRandomWrite = true;
        //nutrientMapRT2.useMipMap = true;
        nutrientMapRT2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***  
        
        nutrientSamplesArray = new Vector4[numAgents];
        nutrientEatAmountsArray = new Vector4[numAgents];

        int kernelCSInitializeNutrientMap = computeShaderNutrientMap.FindKernel("CSInitializeNutrientMap");
        computeShaderNutrientMap.SetTexture(kernelCSInitializeNutrientMap, "nutrientMapWrite", nutrientMapRT1);
        computeShaderNutrientMap.Dispatch(kernelCSInitializeNutrientMap, nutrientMapResolution / 32, nutrientMapResolution / 32, 1);
        Graphics.Blit(nutrientMapRT1, nutrientMapRT2);

        tempTex16 = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex16.wrapMode = TextureWrapMode.Clamp;
        tempTex16.filterMode = FilterMode.Point;
        tempTex16.enableRandomWrite = true;
        tempTex16.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex8 = new RenderTexture(8, 8, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex8.wrapMode = TextureWrapMode.Clamp;
        tempTex8.filterMode = FilterMode.Point;
        tempTex8.enableRandomWrite = true;
        tempTex8.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex4 = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex4.wrapMode = TextureWrapMode.Clamp;
        tempTex4.filterMode = FilterMode.Point;
        tempTex4.enableRandomWrite = true;
        tempTex4.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex2 = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex2.wrapMode = TextureWrapMode.Clamp;
        tempTex2.filterMode = FilterMode.Point;
        tempTex2.enableRandomWrite = true;
        tempTex2.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        tempTex1 = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tempTex1.wrapMode = TextureWrapMode.Clamp;
        tempTex1.filterMode = FilterMode.Point;
        tempTex1.enableRandomWrite = true;
        tempTex1.Create();  // actually creates the renderTexture -- don't forget this!!!!! ***

        nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);

        theRenderKing.fluidRenderMat.SetTexture("_DebugTex", nutrientMapRT1);
        /*
        int numFoodSizeLayers = 4;
        for(int x = 0; x < foodGridResolution; x++) {

            foodGrid[x] = new FoodGridCell[foodGridResolution];
            foodGridSwapArray[x] = new float[foodGridResolution];

            for (int y = 0; y < foodGridResolution; y++) {
                FoodGridCell gridCell = new FoodGridCell(numFoodSizeLayers);
                // Set amounts of Food per layer -- based on noise?
                foodGrid[x][y] = gridCell;

            }
        }
        */
    }
    private void LoadingInstantiateFood() {
        // FOOODDDD!!!!
        foodArray = new FoodChunk[numFood]; // create array
        //int numDeadAnimalFood = 32;
        //foodDeadAnimalArray = new FoodChunk[numDeadAnimalFood];

        //Debug.Log("SpawnFood!");
        for (int i = 0; i < foodArray.Length; i++) {
            GameObject foodGO = Instantiate(Resources.Load("Prefabs/FoodPrefab")) as GameObject;
            foodGO.name = "Food" + i.ToString();
            FoodChunk newFood = foodGO.GetComponent<FoodChunk>();
            foodArray[i] = newFood; // Add to stored list of current Food objects                     
        }
        /*
        for(int j = 0; j < numDeadAnimalFood; j++) {
            GameObject deadAnimalGO = Instantiate(Resources.Load("Prefabs/FoodPrefab")) as GameObject;
            deadAnimalGO.name = "DeadAnimal" + j.ToString();
            FoodChunk newDeadAnimal = deadAnimalGO.GetComponent<FoodChunk>();
            newDeadAnimal.gameObject.SetActive(false);
            foodDeadAnimalArray[j] = newDeadAnimal; // Add to stored list of current Food objects        
        }*/
    }
    private void LoadingInitializeFoodFromGenome() {
        for (int i = 0; i < foodArray.Length; i++) {
            foodArray[i].InitializeFoodFromGenome(foodGenomePoolArray[i], GetRandomFoodSpawnPosition(), null);
            //ReviveFood(i);
        }
    }
    private void LoadingInstantiatePredators() {
        predatorArray = new PredatorModule[numPredators];
        Debug.Log("SpawnPredators!");
        for (int i = 0; i < predatorArray.Length; i++) {
            GameObject predatorGO = Instantiate(Resources.Load("Prefabs/PredatorPrefab")) as GameObject;
            predatorGO.name = "Predator" + i.ToString();
            PredatorModule newPredator = predatorGO.GetComponent<PredatorModule>();
            predatorArray[i] = newPredator; // Add to stored list of current Agents
            
        }
    }
    private void LoadingInitializePredatorsFromGenome() {
        for (int i = 0; i < predatorArray.Length; i++) {
            RevivePredator(i);
        }
    }
    private void LoadingHookUpFluidAndRenderKing() {

        // **** NEED TO ADDRESS THIS!!!!!! ************************
        theRenderKing.fluidObstaclesRenderCamera.targetTexture = environmentFluidManager._ObstaclesRT; // *** See if this works **
        theRenderKing.fluidColorRenderCamera.targetTexture = environmentFluidManager._SourceColorRT;

        //temp:
        theRenderKing.debugRT = environmentFluidManager._SourceColorRT;
    }
    private void LoadingHookUpUIManager() {
        Texture2D playerTex = new Texture2D(4, 2);  // Health, foodAmountRGB, 4 outCommChannels
        playerTex.filterMode = FilterMode.Point;
        agentsArray[0].textureHealth = playerTex;

        uiManager.healthDisplayTex = playerTex;
        uiManager.SetDisplayTextures();
    }
    private void LoadingInitializeGridCells() {
        mapGridCellArray = new MapGridCell[agentGridCellResolution][];
        for (int i = 0; i < agentGridCellResolution; i++) {
            mapGridCellArray[i] = new MapGridCell[agentGridCellResolution];
        }

        float cellSize = mapSize / agentGridCellResolution;

        for (int x = 0; x < agentGridCellResolution; x++) {
            for (int y = 0; y < agentGridCellResolution; y++) {
                Vector2 cellBottomLeft = new Vector2(cellSize * x, cellSize * y);
                Vector2 cellTopRight = new Vector2(cellSize * (x + 1), cellSize * (y + 1));
                mapGridCellArray[x][y] = new MapGridCell(cellBottomLeft, cellTopRight);
            }
        }
    }
    private void LoadingFillGridCells() {
        PopulateGridCells();
    }
    private void LoadingHookUpModules() {
        HookUpModules();
    }
    private void LoadingSetUpFitnessStorage() {
        rawFitnessScoresArray = new float[numAgents];

        statsLifespanEachGenerationList = new List<Vector4>(); // 
        statsFoodEatenEachGenerationList = new List<Vector4>();
        statsPredationEachGenerationList = new List<Vector4>();
        statsBodySizesEachGenerationList = new List<Vector4>(); // 
        statsNutrientsEachGenerationList = new List<Vector4>();
        statsMutationEachGenerationList = new List<float>();
        statsLifespanEachGenerationList.Add(Vector4.one * 0.0001f);
        statsFoodEatenEachGenerationList.Add(Vector4.one * 0.0001f);
        statsPredationEachGenerationList.Add(Vector4.one * 0.0001f);
        statsBodySizesEachGenerationList.Add(Vector4.one * 0.0001f);
        statsNutrientsEachGenerationList.Add(Vector4.one * 0.0001f);
        statsMutationEachGenerationList.Add(0.0001f);
    }
    private void LoadingLoadGenepoolFiles() {
        
        Debug.Log("LOAD LoadingLoadGenepoolFiles!");
        string filePath1 = Path.Combine(Application.streamingAssetsPath, "savedGenePool310.json");
        string dataAsJson1 = File.ReadAllText(filePath1);
        GenePool loadedData1 = JsonUtility.FromJson<GenePool>(dataAsJson1);
        savedGenomePoolArray1 = loadedData1.genomeArray;
        
        string filePath2 = Path.Combine(Application.streamingAssetsPath, "testSave01.json");
        string dataAsJson2 = File.ReadAllText(filePath2);
        GenePool loadedData2 = JsonUtility.FromJson<GenePool>(dataAsJson2);
        savedGenomePoolArray2 = loadedData2.genomeArray;

        string filePath3 = Path.Combine(Application.streamingAssetsPath, "saveStillwater01.json");
        string dataAsJson3 = File.ReadAllText(filePath3);
        GenePool loadedData3 = JsonUtility.FromJson<GenePool>(dataAsJson3);
        savedGenomePoolArray3 = loadedData3.genomeArray;
        
    }

    #endregion

    public void ResetWorld() {
        Debug.Log("RESET WORLD!!!$%^&$%^&$%^&$%^&$%^&$%^&");
        // Initialize Agents:
        //LoadingInstantiateAgents();  // Fills the AgentsArray, Instantiates Agent Objects (MonoBehaviors + GameObjects)

        LoadingInitializePopulationGenomes();
        //agentsArray[0].humanControlled = true;
        //agentsArray[0].humanControlLerp = 1f;
        LoadingInitializeAgentsFromGenomes(); // This was "RespawnAgents()" --  Used to actually place the Agent in the game in a random spot and set the Agent's atributes ffrom its genome

        statsLifespanEachGenerationList.Clear();
        statsFoodEatenEachGenerationList.Clear();
        statsPredationEachGenerationList.Clear();
        statsBodySizesEachGenerationList.Clear();
        statsNutrientsEachGenerationList.Clear();
        statsMutationEachGenerationList.Clear();
        statsLifespanEachGenerationList.Add(Vector4.one * 0.0001f);
        statsFoodEatenEachGenerationList.Add(Vector4.one * 0.0001f);
        statsPredationEachGenerationList.Add(Vector4.one * 0.0001f);
        statsBodySizesEachGenerationList.Add(Vector4.one * 0.0001f);
        statsNutrientsEachGenerationList.Add(Vector4.one * 0.0001f);
        statsMutationEachGenerationList.Add(0.0001f);

        numAgentsBorn = 0;
        numAgentsProcessed = 0;
        currentOldestAgent = 0;
        recordPlayerAge = 0;
        lastPlayerScore = 0;
        recordBotAge = 0;
        for(int i = 0; i < rollingAverageAgentScoresArray.Length; i++) {
            rollingAverageAgentScoresArray[i] = 0f;
        }

        for(int i = 0; i < speciesAvgFoodEaten.Length; i++) {
            speciesAvgFoodEaten[i] = 0f;
        }
        for(int i = 0; i < speciesAvgSizes.Length; i++) {
            speciesAvgSizes[i] = Vector2.one;
        }
        for(int i = 0; i < speciesAvgMouthTypes.Length; i++) {
            speciesAvgMouthTypes[i] = 0f;
        }


        curApproxGen = 1;

        //RefreshFitnessGraphTexture();
        RefreshGraphTextureLifespan();
        RefreshGraphTextureFoodEaten();
        RefreshGraphTexturePredation();
    }

    #region Every Frame  //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& EVERY FRAME &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

    public void TickSimulation() {
        //UpdateFoodGrid();

        float totalNutrients = MeasureTotalNutrients();
        GetNutrientValuesAtMouthPositions();

        //Find closest critters to foodParticles:
        FindClosestFoodParticleToCritters();
        MeasureTotalFoodParticlesAmount();
                
        // ******** REVISIT CODE ORDERING!!!!  -- Should check for death Before or After agent Tick/PhysX ???
        CheckForDeadFood();
        CheckForDeadAgents();  // Result of this will affect: "simStateData.PopulateSimDataArrays(this)" !!!!!
        CheckForRecordPlayerScore();  
        // Gather state data information -- current simulation state after previous frame's internal PhysX update:
        //stateData Populate Arrays
        simStateData.PopulateSimDataArrays(this);  // reads from GameObject Transforms & RigidBodies!!! ++ from FluidSimulationData!!!
        // Render fluidSimulationCameras()  -- Will that work to actually render at the proper time???? we'll see..... *****
        theRenderKing.RenderSimulationCameras(); // will pass current info to FluidSim before it Ticks()
        // Reads from CameraRenders, GameObjects, and query GPU for fluidState
        // eventually, if agents get fluid sensors, will want to download FluidSim data from GPU into simStateData!*        
              
        // **** Figure out proper Execution Order / in which Class to Run RenderOps while being synced w/ results of physX sim!!!
        //theRenderKing.velocityTex = environmentFluidManager.velocityA; // *** Better way to share DATA!!!
        theRenderKing.Tick(); // updates all renderData, buffers, brushStrokes etc.

        HookUpModules(); // Sets nearest-neighbors etc. feed current data into agent Brains
        
        // &&&&& STEP SIMULATION FORWARD:::: &&&&&&&&&&
        // &&&&& STEP SIMULATION FORWARD:::: &&&&&&&&&&
        // Load gameState into Agent Brain, process brain function, read out brainResults,
        // Execute Agent Actions -- apply propulsive force to each Agent:
        for (int i = 0; i < agentsArray.Length; i++) {
            // *** FIND FOOD GRID CELL!!!!  ************
            Vector2 agentPos = agentsArray[i].bodyRigidbody.transform.position;

            int foodGridIndexX = Mathf.FloorToInt(agentPos.x / mapSize * (float)foodGridResolution);
            foodGridIndexX = Mathf.Clamp(foodGridIndexX, 0, foodGridResolution - 1);
            int foodGridIndexY = Mathf.FloorToInt(agentPos.y / mapSize * (float)foodGridResolution);
            foodGridIndexY = Mathf.Clamp(foodGridIndexY, 0, foodGridResolution - 1);

            //agentsArray[i].Tick(new Vector4(0f,0f,0f,0f), ref nutrientEatAmountsArray);  
            agentsArray[i].Tick(this, nutrientSamplesArray[i], ref nutrientEatAmountsArray, settingsManager);  
        }
        for (int i = 0; i < foodArray.Length; i++) {
            foodArray[i].Tick();
        }
        /*for (int i = 0; i < foodDeadAnimalArray.Length; i++) {
            if(foodDeadAnimalArray[i].gameObject.activeSelf) {
                foodDeadAnimalArray[i].Tick();
            }            
        }*/
        // Apply External Forces to dynamic objects: (internal PhysX Updates):
        // **** TEMPORARILY DISABLED!
        ApplyFluidForcesToDynamicObjects();

        EatSelectedFoodParticles(); // 

        // *******************************************************************************************************
        // DONE::::        
        // Respawn Food and place initially        
        // measure total amount of food
        // upgrade critterSimData to have more bite/mouth info
        // Render foodParticles
        // Credit food to critters that eat GPU foodParticles
        // Hook GPU particle info up to critter input modules (food sensors)
        // Check if food is not in water
        // pass mouth data into SimStateData
        // check food vs mouth position and biting state, not body center -- also for sensors?  
        // Adjust Amount of food in foodParticles * efficiency multiplier  
        // Restrict being able to eat foodPArticles by size, mouthType
        // only consumable food is passed to brain Data - pa/small are filtered out

        // TO-DO::::-------------------------------------------------------------------------------------------------------------
        // Create multiple mouth types:  
        //      Jaw +Teeth (creatures, eggs, foodParticles)  
        //      Jaw (eggs, foodParticles)  
        //      Mouth (foodParticles, foodGrid)
        //      None (foodGrid)
        // add new inputs for food dir (dedicated one for nutrient gradient map)           
        // food flow in water -- later, requires water sensors probably        
        
        // *******************************************************************************************************

        RemoveEatenNutrients();        

        float spawnNewFoodChance = settingsManager.spawnNewFoodChance;
        float spawnFoodPercentage = UnityEngine.Random.Range(0f, 0.25f);
        float maxGlobalFood = settingsManager.maxGlobalNutrients;

        if(totalNutrients < maxGlobalFood) {
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < spawnNewFoodChance) {
                // pick random cell:
                int randX = UnityEngine.Random.Range(0, nutrientMapResolution - 1);
                int randY = UnityEngine.Random.Range(0, nutrientMapResolution - 1);

                float foodAvailable = maxGlobalFood - totalNutrients;

                float newFoodAmount = foodAvailable * spawnFoodPercentage;

                // ADD FOOD HERE::
                AddNutrientsAtCoords(newFoodAmount, randX, randY);
                
                
                //foodGrid[randX][randY].foodAmountsPerLayerArray[0] += newFoodAmount;
                //Debug.Log("ADDED FOOD! GridCell[" + randX.ToString() + "][" + randY.ToString() + "]: " + newFoodAmount.ToString());
            }
        }


        ApplyDiffusionOnNutrientMap();
        
        RespawnFoodParticles();

        // TEMP AUDIO EFFECTS!!!!
        //float playerSpeed = (agentsArray[0].transform.position - agentsArray[0]._PrevPos).magnitude;
        float volume = agentsArray[0].smoothedThrottle.magnitude * 0.24f;
        audioManager.SetPlayerSwimLoopVolume(volume);
        //agentsArray[0].avgVel       
                
        // Simulate timestep of fluid Sim - update density/velocity maps:
        // Or should this be right at beginning of frame????? ***************** revisit...
        environmentFluidManager.Tick(); // ** Clean this up, but generally OK
    }

    private void ApplyFluidForcesToDynamicObjects() {
        // ********** REVISIT CONVERSION btw fluid/scene coords and Force Amounts !!!! *************
        for (int i = 0; i < agentsArray.Length; i++) {

            Vector3 depthSample = simStateData.depthAtAgentPositionsArray[i];
            float agentSize = agentsArray[i].fullSizeBoundingBox.x * 2.25f + 1.25f;
            float floorDepth = depthSample.x * 10f;
            if (floorDepth < agentSize)
            {
                float wallForce = Mathf.Clamp01(agentSize - floorDepth) / agentSize;
                agentsArray[i].bodyRigidbody.AddForce(new Vector2(depthSample.y, depthSample.z).normalized * 2.5f * agentsArray[i].bodyRigidbody.mass * wallForce, ForceMode2D.Impulse);
            }

            agentsArray[i].bodyRigidbody.AddForce(simStateData.fluidVelocitiesAtAgentPositionsArray[i] * 40f * agentsArray[i].bodyRigidbody.mass, ForceMode2D.Impulse);

            agentsArray[i].avgFluidVel = Mathf.Lerp(agentsArray[i].avgFluidVel, simStateData.fluidVelocitiesAtAgentPositionsArray[i].magnitude, 0.25f);
        }
        for (int i = 0; i < foodArray.Length; i++) { // *** cache rigidBody reference
            float hackyScalingForceMultiplier = 1f;
            if (foodArray[i].curLifeStage == FoodChunk.FoodLifeStage.Growing) {
                hackyScalingForceMultiplier = 2f;
            }
            foodArray[i].GetComponent<Rigidbody2D>().AddForce(simStateData.fluidVelocitiesAtFoodPositionsArray[i] * 20f * hackyScalingForceMultiplier * foodArray[i].GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse); //
            //foodArray[i].GetComponent<Rigidbody2D>().AddForce(new Vector2(1f, 1f), ForceMode2D.Force); //
            // Looks like AddForce has less of an effect on a GO/Rigidbody2D that is being scaled through a script... ??
            // Feels like rigidbody is accumulating velocity which is then released all at once when the scaling stops??
            // Hacking through it by increasign force on growing food:
        }
        for (int i = 0; i < predatorArray.Length; i++) {
            predatorArray[i].rigidBody.AddForce(simStateData.fluidVelocitiesAtPredatorPositionsArray[i] * 25f * predatorArray[i].rigidBody.mass, ForceMode2D.Impulse);
        }
    }

    private void ApplyDiffusionOnNutrientMap() {
        int kernelCSUpdateNutrientMap = computeShaderNutrientMap.FindKernel("CSUpdateNutrientMap");
        computeShaderNutrientMap.SetFloat("_NutrientDiffusion", settingsManager.foodDiffusionRate);
        computeShaderNutrientMap.SetTexture(kernelCSUpdateNutrientMap, "ObstaclesRead", environmentFluidManager._ObstaclesRT);
        computeShaderNutrientMap.SetTexture(kernelCSUpdateNutrientMap, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSUpdateNutrientMap, "nutrientMapWrite", nutrientMapRT2);
        computeShaderNutrientMap.Dispatch(kernelCSUpdateNutrientMap, nutrientMapResolution / 32, nutrientMapResolution / 32, 1);

        Graphics.Blit(nutrientMapRT2, nutrientMapRT1);
        
    }
    private void GetNutrientValuesAtMouthPositions() {
        // Doing it this way to avoid resetting ALL agents whenever ONE is respawned!
        //ComputeBuffer nutrientSamplesCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
        
        int kernelCSGetNutrientSamples = computeShaderNutrientMap.FindKernel("CSGetNutrientSamples");   
        //computeShaderNutrientMap.SetBuffer(kernelCSGetNutrientSamples, "critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        computeShaderNutrientMap.SetBuffer(kernelCSGetNutrientSamples, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        computeShaderNutrientMap.SetBuffer(kernelCSGetNutrientSamples, "nutrientSamplesCBuffer", nutrientSamplesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSGetNutrientSamples, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetFloat("_MapSize", mapSize);
        computeShaderNutrientMap.Dispatch(kernelCSGetNutrientSamples, nutrientSamplesCBuffer.count, 1, 1);

        //Vector4[] outArray = new Vector4[_NumAgents];
        nutrientSamplesCBuffer.GetData(nutrientSamplesArray); // Disappearing body strokes due to this !?!?!?!?!?

        //Debug.Log("Food: " + nutrientSamplesArray[0].x.ToString());
        //nutrientSamplesCBuffer.Release();
        
        // Read out sample values::::
    }
    private float MeasureTotalNutrients() {

        ComputeBuffer outputValuesCBuffer = new ComputeBuffer(1, sizeof(float) * 4);  // holds the result of measurement: total sum of pix colors in texture
        Vector4[] outputValuesArray = new Vector4[1];

        // 32 --> 16:
        int kernelCSMeasureTotalNutrients = computeShaderNutrientMap.FindKernel("CSMeasureTotalNutrients");   
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex16);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 16, 16, 1);
        // 16 --> 8:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex16);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex8);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 8, 8, 1);
        // 8 --> 4:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex8);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex4);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 4, 4, 1);        
        // 4 --> 2:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex4);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex2);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 2, 2, 1);
        // 2 --> 1:
        computeShaderNutrientMap.SetBuffer(kernelCSMeasureTotalNutrients, "outputValuesCBuffer", outputValuesCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "measureValuesTex", tempTex2);
        computeShaderNutrientMap.SetTexture(kernelCSMeasureTotalNutrients, "pooledResultTex", tempTex1);
        computeShaderNutrientMap.Dispatch(kernelCSMeasureTotalNutrients, 1, 1, 1);
                

        outputValuesCBuffer.GetData(outputValuesArray);

        curGlobalNutrients = outputValuesArray[0].x;

        outputValuesCBuffer.Release();

        //Debug.Log("TotalNutrients: " + outputValuesArray[0].x.ToString() + ", " + outputValuesArray[0].y.ToString());

        return outputValuesArray[0].x;
    }     
    private void AddNutrientsAtCoords(float amount, int x, int y) {
        ComputeBuffer addNutrientsCBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        Vector4[] addNutrientsArray = new Vector4[1];
        addNutrientsArray[0] = new Vector4(amount, (float)x / 32f, (float)y / 32f, 1f);
        addNutrientsCBuffer.SetData(addNutrientsArray);

        int kernelCSAddNutrientsAtCoords = computeShaderNutrientMap.FindKernel("CSAddNutrientsAtCoords");
        computeShaderNutrientMap.SetBuffer(kernelCSAddNutrientsAtCoords, "addNutrientsCBuffer", addNutrientsCBuffer);        
        computeShaderNutrientMap.SetTexture(kernelCSAddNutrientsAtCoords, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSAddNutrientsAtCoords, "nutrientMapWrite", nutrientMapRT2);
        computeShaderNutrientMap.Dispatch(kernelCSAddNutrientsAtCoords, addNutrientsCBuffer.count, 1, 1);
        
        Graphics.Blit(nutrientMapRT2, nutrientMapRT1);

        addNutrientsCBuffer.Release();
    }
    private void RemoveEatenNutrients() {
        ComputeBuffer eatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 4);
                
        eatAmountsCBuffer.SetData(nutrientEatAmountsArray);

        int kernelCSRemoveNutrientsAtLocations = computeShaderNutrientMap.FindKernel("CSRemoveNutrientsAtLocations");
        computeShaderNutrientMap.SetBuffer(kernelCSRemoveNutrientsAtLocations, "nutrientEatAmountsCBuffer", eatAmountsCBuffer);
        computeShaderNutrientMap.SetBuffer(kernelCSRemoveNutrientsAtLocations, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        computeShaderNutrientMap.SetTexture(kernelCSRemoveNutrientsAtLocations, "nutrientMapRead", nutrientMapRT1);
        computeShaderNutrientMap.SetTexture(kernelCSRemoveNutrientsAtLocations, "nutrientMapWrite", nutrientMapRT2);
        computeShaderNutrientMap.SetFloat("_MapSize", mapSize);
        computeShaderNutrientMap.Dispatch(kernelCSRemoveNutrientsAtLocations, eatAmountsCBuffer.count, 1, 1);

        Graphics.Blit(nutrientMapRT2, nutrientMapRT1);
        
        eatAmountsCBuffer.Release();
    }
    
    private void RespawnFoodParticles() {
        // Go through foodParticleData and check for inactive
        // determined by current total food -- done!
        // if flag on shader for Respawn is on, set to active and initialize

        float maxFoodParticleTotal = settingsManager.maxFoodParticleTotalAmount;

        int kernelCSRespawnFoodParticles = computeShaderFoodParticles.FindKernel("CSRespawnFoodParticles");
        computeShaderFoodParticles.SetBuffer(kernelCSRespawnFoodParticles, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSRespawnFoodParticles, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSRespawnFoodParticles, "foodParticlesWrite", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetTexture(kernelCSRespawnFoodParticles, "velocityRead", environmentFluidManager._VelocityA);        
        computeShaderFoodParticles.SetTexture(kernelCSRespawnFoodParticles, "obstaclesRead", environmentFluidManager._ObstaclesRT);
        computeShaderFoodParticles.SetTexture(kernelCSRespawnFoodParticles, "_SpawnDensityMap", nutrientMapRT1);
        computeShaderFoodParticles.SetFloat("_MapSize", mapSize);
            
        //computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);
        computeShaderFoodParticles.SetFloat("_Time", Time.realtimeSinceStartup);

        if(foodParticleMeasurementTotalsData[0].foodAmount < maxFoodParticleTotal) {
            computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 1f);                       
        }
        else {
            computeShaderFoodParticles.SetFloat("_RespawnFoodParticles", 0f);      
        }

        float minParticleSize = settingsManager.avgFoodParticleRadius / settingsManager.foodParticleRadiusVariance;
        float maxParticleSize = settingsManager.avgFoodParticleRadius * settingsManager.foodParticleRadiusVariance;

        computeShaderFoodParticles.SetFloat("_MinParticleSize", minParticleSize);   
        computeShaderFoodParticles.SetFloat("_MaxParticleSize", maxParticleSize);      
        computeShaderFoodParticles.SetFloat("_ParticleNutrientDensity", settingsManager.foodParticleNutrientDensity);
        computeShaderFoodParticles.SetFloat("_FoodParticleRegrowthRate", settingsManager.foodParticleRegrowthRate);

        computeShaderFoodParticles.Dispatch(kernelCSRespawnFoodParticles, 1, 1, 1);
                

        // Copy/Swap Food Particle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderFoodParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", foodParticlesCBuffer);        
        computeShaderFoodParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
        
        
    }
    private void EatSelectedFoodParticles() {  // removes gpu particle & sends consumption data back to CPU
        // Use CritterSimData to determine critter mouth locations

        // run through all foodParticles, check against each critter position, then measure min value with recursive reduction:

        // Need to update CritterSim&InitData structs to have more mouth/bite info

        // Record how much food successfully eaten per Critter

        int kernelCSEatSelectedFoodParticles = computeShaderFoodParticles.FindKernel("CSEatSelectedFoodParticles");
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesWrite", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "foodParticlesEatAmountsCBuffer", foodParticlesEatAmountsCBuffer);        
        computeShaderFoodParticles.SetBuffer(kernelCSEatSelectedFoodParticles, "closestParticlesDataCBuffer", closestFoodParticlesDataCBuffer);  
        computeShaderFoodParticles.SetTexture(kernelCSEatSelectedFoodParticles, "critterDistancesRead", foodParticlesNearestCritters1);
        computeShaderFoodParticles.Dispatch(kernelCSEatSelectedFoodParticles, simStateData.critterSimDataCBuffer.count, 1, 1);

        foodParticlesEatAmountsCBuffer.GetData(foodParticlesEatAmountsArray);

        float totalFoodEaten = 0f;
        for(int i = 0; i < foodParticlesEatAmountsCBuffer.count; i++) {
            totalFoodEaten += foodParticlesEatAmountsArray[i];
        }
        //Debug.Log("EatenFoodParticle: " + totalFoodEaten.ToString());

        //int kernelCSRemoveEatenFoodParticles = computeShaderFoodParticles.FindKernel("CSRemoveEatenFoodParticles");
        //computeShaderFoodParticles.SetBuffer(kernelCSRemoveEatenFoodParticles, "foodParticlesWrite", foodParticlesCBuffer);
        //computeShaderFoodParticles.SetBuffer(kernelCSRemoveEatenFoodParticles, "foodParticlesEatAmountsRead", foodParticlesEatAmountsCBuffer);
        //computeShaderFoodParticles.Dispatch(kernelCSRemoveEatenFoodParticles, simStateData.critterSimDataCBuffer.count, 1, 1);

        // Copy/Swap Food PArticle Buffer:
        int kernelCSCopyFoodParticlesBuffer = computeShaderFoodParticles.FindKernel("CSCopyFoodParticlesBuffer");
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesRead", foodParticlesCBufferSwap);
        computeShaderFoodParticles.SetBuffer(kernelCSCopyFoodParticlesBuffer, "foodParticlesWrite", foodParticlesCBuffer);
        computeShaderFoodParticles.Dispatch(kernelCSCopyFoodParticlesBuffer, 1, 1, 1);
    }
    private void FindClosestFoodParticleToCritters() {  // need to send info on closest particle pos/dir/amt back to CPU also
        
        // Populate main RenderTexture with distances for each foodParticle to each Critter:

        int kernelCSMeasureInitCritterDistances = computeShaderFoodParticles.FindKernel("CSMeasureInitCritterDistances");
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterSimDataCBuffer", simStateData.critterSimDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "critterInitDataCBuffer", simStateData.critterInitDataCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureInitCritterDistances, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetTexture(kernelCSMeasureInitCritterDistances, "foodParticlesNearestCrittersRT", foodParticlesNearestCritters1024);
        computeShaderFoodParticles.Dispatch(kernelCSMeasureInitCritterDistances, foodParticlesCBuffer.count / 1024, simStateData.critterSimDataCBuffer.count, 1);
                

        // Reduce from 1024 --> 32 particles per critter:
        int kernelCSReduceCritterDistances32 = computeShaderFoodParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", foodParticlesNearestCritters1024);
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", foodParticlesNearestCritters32);
        //computeShaderFoodParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestFoodParticlesDataCBuffer);
        computeShaderFoodParticles.Dispatch(kernelCSReduceCritterDistances32, 32, simStateData.critterSimDataCBuffer.count, 1);

        // Reduce from 32 --> 1 particles per critter:
        //int kernelCSReduceCritterDistances1 = computeShaderFoodParticles.FindKernel("CSReduceCritterDistances32");
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesRead", foodParticlesNearestCritters32);
        computeShaderFoodParticles.SetTexture(kernelCSReduceCritterDistances32, "critterDistancesWrite", foodParticlesNearestCritters1);
        computeShaderFoodParticles.SetBuffer(kernelCSReduceCritterDistances32, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSReduceCritterDistances32, "closestParticlesDataCBuffer", closestFoodParticlesDataCBuffer);
        computeShaderFoodParticles.Dispatch(kernelCSReduceCritterDistances32, 1, simStateData.critterSimDataCBuffer.count, 1);

        closestFoodParticlesDataCBuffer.GetData(closestFoodParticlesDataArray);

        //Debug.Log("ClosestFoodParticle: " + closestFoodParticlesDataArray[0].index.ToString() + ", " + closestFoodParticlesDataArray[0].worldPos.ToString());
    }
    private void MeasureTotalFoodParticlesAmount() {
        // pool values 32x reduction:
        // 1024 --> 32
        int kernelCSMeasureTotalFoodParticlesAmount = computeShaderFoodParticles.FindKernel("CSMeasureTotalFoodParticlesAmount");
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", foodParticlesCBuffer);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", foodParticlesMeasure32);
        computeShaderFoodParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 32, 1, 1);
        // 32 --> 1 entry - totals
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesRead", foodParticlesMeasure32);
        computeShaderFoodParticles.SetBuffer(kernelCSMeasureTotalFoodParticlesAmount, "foodParticlesWrite", foodParticlesMeasure1);
        computeShaderFoodParticles.Dispatch(kernelCSMeasureTotalFoodParticlesAmount, 1, 1, 1);

        curGlobalFoodParticles = foodParticleMeasurementTotalsData[0].foodAmount;
        //Debug.Log("total food particle nutrients: " + curGlobalFoodParticles.ToString());
        foodParticlesMeasure1.GetData(foodParticleMeasurementTotalsData);

        //Debug.Log("FoodParticlesTotalAmount: " + foodParticleMeasurementTotalsData[0].foodAmount.ToString());
    }
    public void PlayerToolStirOn(Vector3 origin, Vector2 forceVector) {
        float magnitude = forceVector.magnitude;
        if(magnitude == 0f) {
            Debug.Log("ERROR null vector!");
        }
        magnitude *= 0.03f;
        float maxMag = 0.5f;
        if(magnitude > maxMag) {
            magnitude = maxMag;            
        }
        forceVector = forceVector.normalized * magnitude;

        Debug.Log("PlayerToolStir pos: " + origin.ToString() + ", forceVec: [" + forceVector.x.ToString() + "," + forceVector.y.ToString() + "]  mag: " + magnitude.ToString());

        environmentFluidManager.StirWaterOn(origin, forceVector);
    }
    public void PlayerToolStirOff() {        
        environmentFluidManager.StirWaterOff();
    }
    public void PlayerFeedToolSprinkle(Vector3 pos) {
        Debug.Log("PlayerFeedToolSprinkle pos: " + pos.ToString());
    }
    public void PlayerFeedToolPour(Vector3 pos) {        
        int xCoord = Mathf.RoundToInt(pos.x / 256f * nutrientMapResolution);
        int yCoord = Mathf.RoundToInt(pos.y / 256f * nutrientMapResolution);

        Debug.Log("PlayerFeedToolPour pos: " + xCoord.ToString() + ", " + yCoord.ToString());

        AddNutrientsAtCoords(2f, xCoord, yCoord);
    }
    private void UpdateFoodGrid() {
        float totalFoodLayer0 = 0f;
        /*
        for(int x = 0; x < foodGridResolution; x++) {
                        
            // for each cell:
            for (int y = 0; y < foodGridResolution; y++) {

                // cell[x,y]:
                int leftIndex = x - 1;
                if(leftIndex < 0) {
                    leftIndex = 0;
                }
                int rightIndex = x + 1;
                if(rightIndex >= foodGridResolution) {
                    rightIndex = foodGridResolution - 1;
                }
                int downIndex = y - 1;
                if(downIndex < 0) {
                    downIndex = 0;
                }
                int upIndex = y + 1;
                if(upIndex >= foodGridResolution) {
                    upIndex = foodGridResolution - 1;
                }

                float amountRight = foodGrid[rightIndex][y].foodAmountsPerLayerArray[0];
                float amountLeft = foodGrid[leftIndex][y].foodAmountsPerLayerArray[0];
                float amountUp = foodGrid[x][upIndex].foodAmountsPerLayerArray[0];
                float amountDown = foodGrid[x][downIndex].foodAmountsPerLayerArray[0];
                float amountCenter = foodGrid[x][y].foodAmountsPerLayerArray[0];

                float deltaX = amountRight - amountLeft;
                float deltaY = amountUp - amountDown;

                Vector2 grad = new Vector2(deltaX, deltaY).normalized;

                foodGrid[x][y].gradientFoodAmountsPerLayerArray[0] = grad;

                foodGridSwapArray[x][y] = Mathf.Lerp(amountCenter, (amountCenter + amountRight + amountLeft + amountUp + amountDown)  / 5f, 0.01f);

                totalFoodLayer0 += foodGrid[x][y].foodAmountsPerLayerArray[0];
                // ^^^^ HAVE TO DO THIS FOR EACH FOOD SIZE LAYER!!! ^^^^^ **********************
                                
            }
        }
        for(int x = 0; x < foodGridResolution; x++) {                        
            // for each cell:
            for (int y = 0; y < foodGridResolution; y++) {
                foodGrid[x][y].foodAmountsPerLayerArray[0] = foodGridSwapArray[x][y];

                // COLOR: TEMP:::
                Color pixelColor = new Color(foodGridSwapArray[x][y], foodGridSwapArray[x][y], foodGridSwapArray[x][y], 1);
                debugFoodTexture.SetPixel(x, y, pixelColor);
            }
        }
        debugFoodTexture.Apply();        
        */
        float spawnNewFoodChance = 0.125f;
        float spawnFoodPercentage = UnityEngine.Random.Range(0f, 1f);
        float maxGlobalFood = foodGridResolution * foodGridResolution * 0.25f * 0.075f;

        if(totalFoodLayer0 < maxGlobalFood) {
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < spawnNewFoodChance) {
                // pick random cell:
                int randX = UnityEngine.Random.Range(0, foodGridResolution - 1);
                int randY = UnityEngine.Random.Range(0, foodGridResolution - 1);

                float foodAvailable = maxGlobalFood - totalFoodLayer0;

                float newFoodAmount = foodAvailable * spawnFoodPercentage;

                foodGrid[randX][randY].foodAmountsPerLayerArray[0] += newFoodAmount;

                //Debug.Log("ADDED FOOD! GridCell[" + randX.ToString() + "][" + randY.ToString() + "]: " + newFoodAmount.ToString());
            }
        }
        
        /*for(int i = 0; i < foodGrid.Length; i++) {
            int xIndex = i % foodGridResolution;
            int yIndex = Mathf.FloorToInt((float)i / (float)foodGridResolution);

            // Calculate Empirical Gradient:
            if(xIndex - 1 < 0) {
                leftIndex = 0;
            }
            int leftIndex = yIndex * foodGridResolution + (xIndex - 1);
        }*/
    }
    private void PopulateGridCells() {

        // Inefficient!!!
        for (int x = 0; x < agentGridCellResolution; x++) {
            for (int y = 0; y < agentGridCellResolution; y++) {
                mapGridCellArray[x][y].friendIndicesList.Clear();
                mapGridCellArray[x][y].foodIndicesList.Clear();
                mapGridCellArray[x][y].predatorIndicesList.Clear();
            }
        }

        // !! ******* MAP CHANGE!! ***** Now starts at 0,0 (bottomLeft) and goes up to mapSize,mapSize (topRight) just like UV coords *****
        // This should make conversions between CPU and GPU coords a bit simpler in the long run
        
        // FOOD!!! :::::::
        for (int f = 0; f < foodArray.Length; f++) {
            
            float xPos = foodArray[f].transform.position.x;
            float yPos = foodArray[f].transform.position.y;
            int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            mapGridCellArray[xCoord][yCoord].foodIndicesList.Add(f);
                       
        }
        /*for(int a = 0; a < foodDeadAnimalArray.Length; a++) {  // DEAD ANIMALS!!!
            float xPos = foodDeadAnimalArray[a].transform.position.x;
            float yPos = foodDeadAnimalArray[a].transform.position.y;
            int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList.Add(a);
        }*/

        // FRIENDS::::::
        for (int a = 0; a < agentsArray.Length; a++) {
            float xPos = agentsArray[a].bodyGO.transform.position.x;
            float yPos = agentsArray[a].bodyGO.transform.position.y;
            int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            mapGridCellArray[xCoord][yCoord].friendIndicesList.Add(a);
        }

        // PREDATORS !!! :::::::
        for (int p = 0; p < predatorArray.Length; p++) {
            float xPos = predatorArray[p].transform.position.x;
            float yPos = predatorArray[p].transform.position.y;
            int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            mapGridCellArray[xCoord][yCoord].predatorIndicesList.Add(p);
        }
    }
    private void HookUpModules() {
                
        // mapSize is global now, get with the times, jeez-louise!
        //float cellSize = mapSize / agentGridCellResolution;
        //Vector2 playerPos = new Vector2(playerAgent.transform.localPosition.x, playerAgent.transform.localPosition.y);

        // Find NearestNeighbors:
        for (int a = 0; a < agentsArray.Length; a++) {
            // Find which gridCell this Agent is in:    
            Vector2 agentPos = new Vector2(agentsArray[a].bodyRigidbody.transform.position.x, agentsArray[a].bodyRigidbody.transform.position.y);
            int xCoord = Mathf.FloorToInt(agentPos.x / mapSize * (float)agentGridCellResolution); // Mathf.FloorToInt((agentPos.x + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            int yCoord = Mathf.FloorToInt(agentPos.y / mapSize * (float)agentGridCellResolution);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            int closestFriendIndex = a;  // default to self
            float nearestFriendSquaredDistance = float.PositiveInfinity;
            int closestEnemyAgentIndex = 0;
            float nearestEnemyAgentSqDistance = float.PositiveInfinity;
            int closestFoodIndex = -1; // default to -1??? ***            
            float nearestFoodDistance = float.PositiveInfinity;
            int closestPredIndex = 0; // default to 0???
            float nearestPredDistance = float.PositiveInfinity;

           // bool closestFoodIsDeadAnimal = false;

            int ownSpeciesIndex = Mathf.FloorToInt((float)a / (float)numAgents * (float)numSpecies);

            // **** Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
            /*int index = -1;
            try {
                index = mapGridCellArray[xCoord][yCoord].friendIndicesList.Count;
            }
            catch (Exception e) {
                print("error! index = " + index.ToString() + ", xCoord: " + xCoord.ToString() + ", yCoord: " + yCoord.ToString() + ", xPos: " + agentPos.x.ToString() + ", yPos: " + agentPos.y.ToString());
            } */
            // **** Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].friendIndicesList.Count; i++) {
                int neighborIndex = mapGridCellArray[xCoord][yCoord].friendIndicesList[i];
                // FRIEND:
                Vector2 neighborPos = new Vector2(agentsArray[neighborIndex].bodyRigidbody.transform.localPosition.x, agentsArray[neighborIndex].bodyRigidbody.transform.localPosition.y);
                float squaredDistNeighbor = (neighborPos - agentPos).sqrMagnitude;
                                
                if (squaredDistNeighbor <= nearestFriendSquaredDistance) { // if now the closest so far, update index and dist:
                    int neighborSpeciesIndex = Mathf.FloorToInt((float)neighborIndex / (float)numAgents * (float)numSpecies);
                    if(ownSpeciesIndex == neighborSpeciesIndex) {  // if two agents are of same species - friends
                        if(a != neighborIndex) {  // make sure it doesn't consider itself:
                            closestFriendIndex = neighborIndex;
                            nearestFriendSquaredDistance = squaredDistNeighbor;
                        } 
                    }                                       
                }

                if (squaredDistNeighbor <= nearestEnemyAgentSqDistance) { // if now the closest so far, update index and dist:
                    int neighborSpeciesIndex = Mathf.FloorToInt((float)neighborIndex / (float)numAgents * (float)numSpecies);
                    if(ownSpeciesIndex != neighborSpeciesIndex) {  // if two agents are of different species - enemy
                        if(a != neighborIndex) {  // make sure it doesn't consider itself:
                            closestEnemyAgentIndex = neighborIndex;
                            nearestEnemyAgentSqDistance = squaredDistNeighbor;
                        }
                    }
                                        
                }
            }
            
            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].foodIndicesList.Count; i++) {
                // FOOD:
                if(foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].enabled) { // if enabled:
                    if(foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].curLifeStage == FoodChunk.FoodLifeStage.Mature) {
                        //Debug.Log("Found valid Food!");
                        Vector2 foodPos = new Vector2(foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].transform.localPosition.x, foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].transform.localPosition.y);
                        float distFood = (foodPos - agentPos).magnitude - (foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].curSize.magnitude + 1f) * 0.5f;  // subtract food & agent radii
                        if (distFood <= nearestFoodDistance) { // if now the closest so far, update index and dist:
                            if (a != mapGridCellArray[xCoord][yCoord].foodIndicesList[i]) {  // make sure it doesn't consider itself:
                                closestFoodIndex = mapGridCellArray[xCoord][yCoord].foodIndicesList[i];
                                nearestFoodDistance = distFood;
                            }
                        }
                    } 
                }                             
            }
            /*
            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList.Count; i++) {
                // DEAD ANIMALS!!!:
                if(foodDeadAnimalArray[mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i]].enabled) { // if enabled:
                    if(foodDeadAnimalArray[mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i]].curLifeStage == FoodChunk.FoodLifeStage.Mature) {
                        //Debug.Log("Found valid Food!");
                        Vector2 foodPos = new Vector2(foodDeadAnimalArray[mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i]].transform.localPosition.x, foodDeadAnimalArray[mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i]].transform.localPosition.y);
                        float distFood = (foodPos - agentPos).magnitude - (foodDeadAnimalArray[mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i]].curSize.magnitude + 1f) * 0.5f;  // subtract food & agent radii
                        if (distFood <= nearestFoodDistance) { // if now the closest so far, update index and dist:
                            if (a != mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i]) {  // make sure it doesn't consider itself:
                                closestFoodIndex = mapGridCellArray[xCoord][yCoord].deadAnimalIndicesList[i];
                                nearestFoodDistance = distFood;
                                closestFoodIsDeadAnimal = true;
                            }
                        }
                    } 
                }                             
            }
            */

            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].predatorIndicesList.Count; i++) {
                // PREDATORS:::::::
                Vector2 predatorPos = new Vector2(predatorArray[mapGridCellArray[xCoord][yCoord].predatorIndicesList[i]].transform.localPosition.x, predatorArray[mapGridCellArray[xCoord][yCoord].predatorIndicesList[i]].transform.localPosition.y);
                float distPred = (predatorPos - agentPos).magnitude - (predatorArray[mapGridCellArray[xCoord][yCoord].predatorIndicesList[i]].curScale + 1f) * 0.5f;  // subtract pred & agent radii;
                if (distPred <= nearestPredDistance) { // if now the closest so far, update index and dist:
                    if (a != mapGridCellArray[xCoord][yCoord].predatorIndicesList[i]) {  // make sure it doesn't consider itself:
                        closestPredIndex = mapGridCellArray[xCoord][yCoord].predatorIndicesList[i];
                        nearestPredDistance = distPred;
                    }
                }
            }
            // Set proper references between AgentBrains and Environment/Game Objects:::
            // ***** DISABLED!!!! *** NEED TO RE_IMPLEMENT THIS LATER!!!! ********************************************
            agentsArray[a].coreModule.nearestFriendAgent = agentsArray[closestFriendIndex];
            agentsArray[a].coreModule.nearestEnemyAgent = agentsArray[closestEnemyAgentIndex];
            if(closestFoodIndex != -1) {
                agentsArray[a].coreModule.nearestFoodModule = foodArray[closestFoodIndex];
                /*if(closestFoodIsDeadAnimal) {                    
                    agentsArray[a].coreModule.nearestFoodModule = foodDeadAnimalArray[closestFoodIndex];
                }
                else {                    
                    agentsArray[a].coreModule.nearestFoodModule = foodArray[closestFoodIndex];
                }*/
            }            
            agentsArray[a].coreModule.nearestPredatorModule = predatorArray[closestPredIndex];            
        }
    }
    private void CheckForRecordPlayerScore() {
        // Check for Record Agent AGE!
        if (agentsArray[0].scoreCounter > recordPlayerAge) {
            recordPlayerAge = agentsArray[0].scoreCounter;
        }
    }
    private void CheckForDeadFood() { // *** revisit
        // CHECK FOR DEAD FOOD!!! :::::::
        for (int f = 0; f < foodArray.Length; f++) {
            if (foodArray[f].isDepleted) {
                ProcessDeadFood(f);
            }
        }

        /*for (int a = 0; a < foodDeadAnimalArray.Length; a++) {
            if (foodDeadAnimalArray[a].isDepleted) {
                foodDeadAnimalArray[a].gameObject.SetActive(false);
            }
        }*/
    }
    private void CheckForDeadAgents() { 
        for (int a = 0; a < agentsArray.Length; a++) {
            if (agentsArray[a].isNull) {
                if(a == 0) { // if player
                    ProcessDeadAgent(a);
                    /*if(playerIsDead) {
                        // this was already called (or should have been)
                    }
                    else {
                        ProcessDeadAgent(a);
                    }*/
                }
                else {
                    ProcessDeadAgent(a);
                }                
            }
        }
    }  // *** revisit    
        
    // AFTER PHYSX!!!
    private void OnTriggerEnter2D(Collider2D collision) {
        TriggerTestOrder();
    }
    private void OnTriggerStay2D(Collider2D collision) {
        TriggerTestOrder();
    }
    private void TriggerTestOrder() {
        

        //Vector3 agentPos = playerAgent.testModule.ownRigidBody2D.position;
        //Debug.Log("OnTriggerStay2D AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());
    }
    
    #endregion

    #region Process Events // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& PROCESS EVENTS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    /*public void ReviveFood(int index) {
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-36f, 36f), UnityEngine.Random.Range(-36f, 36f), 0f);
        foodArray[index].transform.localPosition = startPos;
        foodArray[index].Respawn();
    }*/ // *** confirm these are set up alright
    public void RevivePredator(int index) {
        Vector3 startPos = GetRandomPredatorSpawnPosition(); // new Vector3(UnityEngine.Random.Range(-36f, 36f), UnityEngine.Random.Range(-36f, 36f), 0f);
        predatorArray[index].transform.localPosition = startPos;
        predatorArray[index].InitializePredator();
        
    } // *** confirm these are set up alright      
    public void ProcessDeadAgent(int agentIndex) {
        /*
        // Convert to DeadAnimal Food:
        int deadAnimalIndex = 0;
        for(int i = 0; i < foodDeadAnimalArray.Length; i++) {
            if(foodDeadAnimalArray[i].gameObject.activeSelf == false) {
                deadAnimalIndex = i;
                break;
            }
        }
        // spawn corpseFood:
        StartPositionGenome startPos = new StartPositionGenome(new Vector3(agentsArray[agentIndex].bodyRigidbody.transform.position.x, 
                                                                           agentsArray[agentIndex].bodyRigidbody.transform.position.y, 
                                                                           0f), 
                                                                           Quaternion.identity);

        // calculate amount of food to leave:        
        float droppedFoodAmount = agentsArray[agentIndex].fullSizeBodyVolume + agentsArray[agentIndex].coreModule.stomachContents;
        float foodSideLength = Mathf.Sqrt(droppedFoodAmount);
        foodGenomeAnimalCorpse.fullSize = new Vector2(foodSideLength, foodSideLength);

        foodDeadAnimalArray[deadAnimalIndex].InitializeFoodFromGenome(foodGenomeAnimalCorpse, startPos, null); // Spawn that genome in dead Agent's body and revive it!
        
        foodDeadAnimalArray[deadAnimalIndex].gameObject.SetActive(true);
        */

        /*bool processAsAI = true;

        if(agentIndex == 0) {
            if(uiManager.isObserverMode) {

            }
            else {
                processAsAI = false;
            }
        }
        else {
            
        } 
        
        if(processAsAI) {*/
        CheckForRecordAgentScore(agentIndex);
        ProcessAgentScores(agentIndex);
        // Updates rankedIndicesArray[] so agents are ordered by score:
        int speciesIndex = Mathf.FloorToInt((float)agentIndex / (float)numAgents * (float)numSpecies);
        ProcessAndRankAgentFitness(speciesIndex);
        // Reproduction!!!
        CreateMutatedCopyOfAgent(agentIndex, speciesIndex); 
        
        theRenderKing.UpdateAgentSmearStrokesBuffer(agentIndex);
        //theRenderKing.UpdateAgentBodyStrokesBuffer(agentIndex);
        theRenderKing.UpdateAgentEyeStrokesBuffer(agentIndex);
        /*}
        else {
            ProcessDeadPlayer();
        }*/

        if(agentIndex == 0) {
            //cameraManager.targetTransform = agentsArray[cameraManager.targetCritterIndex].bodyGO.transform;
            //cameraManager.StartPlayerRespawn();
        }
    }
    public void ProcessDeadFood(int foodIndex) {

        // Check for timer?

        // How much food is already out there?
        float totalEggFoodVolume = 0f;
        for(int i = 0; i < _NumFood; i++)
        {
            totalEggFoodVolume += foodArray[i].foodAmount;
        }

        if (totalEggFoodVolume < 12800f)
        {
            // Reproduction!!!
            //Debug.Log("ProcessDeadFood totalEggFoodVolume < 128f " + totalEggFoodVolume.ToString());
            CreateMutatedCopyOfFood(foodIndex);

            theRenderKing.UpdateDynamicFoodBuffers(foodIndex);
        }

        //CheckForRecordAgentScore(foodIndex);
        //ProcessAgentScores(foodIndex);        
        // Updates rankedIndicesArray[] so agents are ordered by score:
        //ProcessAndRankAgentFitness();
        //theRenderKing.UpdateAgentBodyStrokesBuffer(foodIndex);
        //theRenderKing.InitializeAgentEyeStrokesBuffer();
    }
    private void CheckForRecordAgentScore(int agentIndex) {
        if (agentsArray[agentIndex].scoreCounter > recordBotAge && agentIndex != 0) {
            recordBotAge = agentsArray[agentIndex].scoreCounter;
        }
    }
    private void ProcessAgentScores(int agentIndex) {
        numAgentsProcessed++;
        //get species index:
        int speciesIndex = Mathf.FloorToInt((float)agentIndex / (float)_NumAgents * (float)numSpecies);

        float weightedAvgLerpVal = 1f / 128f;
        weightedAvgLerpVal = Mathf.Max(weightedAvgLerpVal, 1f / (float)(numAgentsProcessed + 1));

        speciesAvgFoodEaten[speciesIndex] = Mathf.Lerp(speciesAvgFoodEaten[speciesIndex], agentsArray[agentIndex].totalFoodEaten, weightedAvgLerpVal);
        speciesAvgSizes[speciesIndex] = Vector2.Lerp(speciesAvgSizes[speciesIndex], new Vector2(agentsArray[agentIndex].coreModule.coreWidth, agentsArray[agentIndex].coreModule.coreLength), weightedAvgLerpVal);
        float mouthType = 0f;
        if(agentsArray[agentIndex].mouthRef.isPassive == false) {
            mouthType = 1f;
        }
        speciesAvgMouthTypes[speciesIndex] = Mathf.Lerp(speciesAvgMouthTypes[speciesIndex], mouthType, weightedAvgLerpVal);
        rollingAverageAgentScoresArray[speciesIndex] = Mathf.Lerp(rollingAverageAgentScoresArray[speciesIndex], (float)agentsArray[agentIndex].scoreCounter, weightedAvgLerpVal);
        
        //statsAvgGlobalNutrients = Vector4.Lerp(statsAvgGlobalNutrients, new Vector4(1f, 1f, 0f, 0f), weightedAvgLerpVal);
        //statsAvgMutationRate = Mathf.Lerp(statsAvgMutationRate, curPlayerMutationRate

        float approxGen = (float)numAgentsBorn / (float)(numAgents - 1);
        if (approxGen > curApproxGen) {
            Vector4 scores = new Vector4(rollingAverageAgentScoresArray[0], rollingAverageAgentScoresArray[1], rollingAverageAgentScoresArray[2], rollingAverageAgentScoresArray[3]); ;
            statsLifespanEachGenerationList.Add(scores); // ** UPDATE THIS TO SAVE aLLL 4 SCORES!!! ***
            if (rollingAverageAgentScoresArray[speciesIndex] > agentAvgRecordScore) {
                agentAvgRecordScore = rollingAverageAgentScoresArray[speciesIndex];
            }
            curApproxGen++;
            
            Vector4 foodEaten = new Vector4(speciesAvgFoodEaten[0], speciesAvgFoodEaten[1], speciesAvgFoodEaten[2], speciesAvgFoodEaten[3]); 
            statsFoodEatenEachGenerationList.Add(foodEaten);

            Vector4 predation = new Vector4(speciesAvgMouthTypes[0], speciesAvgMouthTypes[1], speciesAvgMouthTypes[2], speciesAvgMouthTypes[3]); ;
            statsPredationEachGenerationList.Add(predation);

            //bodySizes:
            Vector4 bodySizes = new Vector4(speciesAvgSizes[0].x * speciesAvgSizes[0].y, speciesAvgSizes[1].x * speciesAvgSizes[1].y, speciesAvgSizes[2].x * speciesAvgSizes[2].y, speciesAvgSizes[3].x * speciesAvgSizes[3].y);
            //Debug.Log("BodySizeAreas: " + bodySizes.ToString());
            statsBodySizesEachGenerationList.Add(bodySizes);

            statsMutationEachGenerationList.Add(curPlayerMutationRate);
            statsNutrientsEachGenerationList.Add(new Vector4(curGlobalNutrients, curGlobalFoodParticles, 0f, 0f));

            RefreshGraphTextureLifespan();
            RefreshGraphTextureFoodEaten();
            RefreshGraphTexturePredation();
            RefreshGraphTextureBodySizes();
            RefreshGraphTextureNutrients();
            RefreshGraphTextureMutation();

            UpdateSimulationClimate();
        }
        else {
            if(numAgentsProcessed < 130) {
                if(numAgentsProcessed % 8 == 0) {
                    //Debug.Log("process Stats!! + " + curApproxGen.ToString() + ", numProcessed: " + numAgentsProcessed.ToString());
                    Vector4 scores = new Vector4(rollingAverageAgentScoresArray[0], rollingAverageAgentScoresArray[1], rollingAverageAgentScoresArray[2], rollingAverageAgentScoresArray[3]); ;
                    statsLifespanEachGenerationList[curApproxGen - 1] = scores;
                    //statsLifespanEachGenerationList.Add(scores); // ** UPDATE THIS TO SAVE aLLL 4 SCORES!!! ***
                    //if (rollingAverageAgentScoresArray[speciesIndex] > agentAvgRecordScore) {
                    //    agentAvgRecordScore = rollingAverageAgentScoresArray[speciesIndex];
                    //}
                    //curApproxGen++;
                    
                    Vector4 foodEaten = new Vector4(speciesAvgFoodEaten[0], speciesAvgFoodEaten[1], speciesAvgFoodEaten[2], speciesAvgFoodEaten[3]); 
                    statsFoodEatenEachGenerationList[curApproxGen - 1] = foodEaten;

                    Vector4 predation = new Vector4(speciesAvgMouthTypes[0], speciesAvgMouthTypes[1], speciesAvgMouthTypes[2], speciesAvgMouthTypes[3]); ;
                    statsPredationEachGenerationList[curApproxGen - 1] = predation;

                    //bodySizes:
                    Vector4 bodySizes = new Vector4(speciesAvgSizes[0].x * speciesAvgSizes[0].y, speciesAvgSizes[1].x * speciesAvgSizes[1].y, speciesAvgSizes[2].x * speciesAvgSizes[2].y, speciesAvgSizes[3].x * speciesAvgSizes[3].y);
                    statsBodySizesEachGenerationList[curApproxGen - 1] = bodySizes;

                    statsMutationEachGenerationList[curApproxGen - 1] = curPlayerMutationRate;
                    statsNutrientsEachGenerationList[curApproxGen - 1] = new Vector4(curGlobalNutrients, curGlobalFoodParticles, 0f, 0f);

                    RefreshGraphTextureLifespan();
                    RefreshGraphTextureFoodEaten();
                    RefreshGraphTexturePredation();
                    RefreshGraphTextureBodySizes();

                    //UpdateSimulationClimate();
                }
            }
        }
    }
    private void UpdateSimulationClimate() {
        // Change force of turbulence, damping, other fluidSim parameters,
        // Inject pre-trained critters
        environmentFluidManager.UpdateSimulationClimate((float)curApproxGen);
    }
    /*private void RefreshFitnessGraphTexture() {
        uiManager.RefreshFitnessTexture(fitnessScoresEachGenerationList);
    }*/
    
    private void RefreshGraphTextureLifespan() {
        uiManager.UpdateStatsTextureLifespan(statsLifespanEachGenerationList);
    }
    private void RefreshGraphTextureBodySizes() {
        uiManager.UpdateStatsTextureBodySizes(statsBodySizesEachGenerationList);
    }
    private void RefreshGraphTextureFoodEaten() {
        uiManager.UpdateStatsTextureFoodEaten(statsFoodEatenEachGenerationList);
    }
    private void RefreshGraphTexturePredation() {
        uiManager.UpdateStatsTexturePredation(statsPredationEachGenerationList);
    }
    private void RefreshGraphTextureNutrients() {
        uiManager.UpdateStatsTextureNutrients(statsNutrientsEachGenerationList);
    }
    private void RefreshGraphTextureMutation() {
        uiManager.UpdateStatsTextureMutation(statsMutationEachGenerationList);
    }
    private void ProcessAndRankAgentFitness(int speciesIndex) {
        // Measure fitness of all current agents (their genomes, actually)  NOT PLAYER!!!!
        for (int i = 0; i < rawFitnessScoresArray.Length; i++) {
            rawFitnessScoresArray[i] = (float)agentsArray[i].scoreCounter;
        }

        int popSize = (numAgents / numSpecies);
        int startIndex = popSize * speciesIndex;
        int endIndex = popSize * (speciesIndex + 1);

        // populate arrays:
        for (int i = startIndex; i < endIndex; i++) {
            try {
                rankedIndicesList[i] = i; // i+1;
            }
            catch {
                Debug.Log("Error! i = " + i.ToString() + ", arrayLength: " + rankedIndicesList.Length.ToString() + ", popSize: " + popSize.ToString() + "si: " + speciesIndex.ToString());
            }
            
            rankedFitnessList[i] = rawFitnessScoresArray[i]; //rawFitnessScoresArray[i+1];
        } // Sort By Fitness
        for (int i = startIndex; i < endIndex - 1; i++) {
            for (int j = startIndex; j < endIndex - 1; j++) {
                float swapFitA = rankedFitnessList[j];
                float swapFitB = rankedFitnessList[j + 1];
                int swapIdA = rankedIndicesList[j];
                int swapIdB = rankedIndicesList[j + 1];

                if (swapFitA < swapFitB) {  // bigger is better now after inversion
                    rankedFitnessList[j] = swapFitB;
                    rankedFitnessList[j + 1] = swapFitA;
                    rankedIndicesList[j] = swapIdB;
                    rankedIndicesList[j + 1] = swapIdA;
                }
            }
        }
    }
    private void CreateMutatedCopyOfAgent(int agentIndex, int speciesIndex) {
        //int parentGenomeIndex = agentIndex;
        // Randomly select a good one based on fitness Lottery (oldest = best)
        if (rankedIndicesList[0] == agentIndex) {  // if Top Agent, just respawn identical copy:
                               
        }
        else {
            BodyGenome newBodyGenome = new BodyGenome();
            BrainGenome newBrainGenome = new BrainGenome();

            int parentGenomeIndex = GetAgentIndexByLottery(rankedFitnessList, rankedIndicesList, speciesIndex);

            //Debug.Log("Agent[" + agentIndex.ToString() + "] species[" + speciesIndex.ToString() + "] parentIndex: " + parentGenomeIndex.ToString());
            
            BodyGenome parentBodyGenome = agentGenomePoolArray[parentGenomeIndex].bodyGenome;
            BrainGenome parentBrainGenome = agentGenomePoolArray[parentGenomeIndex].brainGenome;

            //MutationSettings mutationSettings = settingsManager.mutationSettingsPersistent;

            // Can randomly pull from saved Genepool database:
            bool usePreTrained = false;

            if(usePreTrained) {
                float randRoll = UnityEngine.Random.Range(0f, 1f);
                if(randRoll < 0.006f) {
                    //mutationSettings = settingsManager.mutationSettingsRandomBody;
                    randRoll = UnityEngine.Random.Range(0f, 1f);
                    if(randRoll < 0.55f) {                
                        parentBodyGenome = savedGenomePoolArray1[parentGenomeIndex].bodyGenome;
                        parentBrainGenome = savedGenomePoolArray1[parentGenomeIndex].brainGenome;
                    }
                    else if(randRoll < 0.85f) {                
                        parentBodyGenome = savedGenomePoolArray2[parentGenomeIndex].bodyGenome;
                        parentBrainGenome = savedGenomePoolArray2[parentGenomeIndex].brainGenome;
                    }
                    else {
                        parentBodyGenome = savedGenomePoolArray3[parentGenomeIndex].bodyGenome;
                        parentBrainGenome = savedGenomePoolArray3[parentGenomeIndex].brainGenome;
                    }                
                }
            }
            

            // MUTATE BODY:
            // Mutate body here
            newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, settingsManager.mutationSettingsPersistent);
            // Create duplicate Genome
            newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, settingsManager.mutationSettingsPersistent);

            agentGenomePoolArray[agentIndex].bodyGenome = newBodyGenome; // update genome to new one
            agentGenomePoolArray[agentIndex].brainGenome = newBrainGenome; // update genome to new one

            numAgentsBorn++;
            currentOldestAgent = agentsArray[rankedIndicesList[0]].scoreCounter;
        }

        // **** !!! REvisit StartPos!!!
        //Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
        //StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        agentsArray[agentIndex].InitializeAgentFromGenome(agentIndex, agentGenomePoolArray[agentIndex], GetRandomAgentSpawnPosition(speciesIndex)); // Spawn that genome in dead Agent's body and revive it!

        theRenderKing.UpdateAgentWidthsTexture(agentsArray[agentIndex]);
        //if(agentIndex == 0) {
            //if Player:

            //cameraManager.targetCamPos = agentsArray[agentIndex].transform.position;
            //cameraManager.targetCamPos.z = -50f;
        //}
        //theRenderKing.SetSimDataArrays(); // find better way to sync data!! ****
        //theRenderKing.SetPointStrokesBuffer();
        //theRenderKing.InitializeAgentCurveData(agentIndex);
    }
    private void CreateMutatedCopyOfFood(int foodIndex) {

        int speciesIndex = Mathf.FloorToInt((float)foodIndex / (float)numFood * (float)numSpecies);

        int parentIndex = UnityEngine.Random.Range(0, numFood);
                

        //bool foundParent = false;
        // Try to find a suitable startPos:
        int numParentSearches = 1;
        //int parentIndex = -1;
        for(int i = 0; i < numParentSearches; i++) {
            int foodGroupSize = numFood / numSpecies;
            int randomIndex = UnityEngine.Random.Range(speciesIndex * foodGroupSize, (speciesIndex + 1) * foodGroupSize);
            if(foodArray[randomIndex].curLifeStage == FoodChunk.FoodLifeStage.Mature) {
                
                //return startPosGenome;
                parentIndex = randomIndex;
                //foundParent = true;
                break;
            }            
        }
       
        FoodGenome newFoodGenome = new FoodGenome(foodIndex);

        newFoodGenome.SetToMutatedCopyOfParentGenome(foodGenomePoolArray[parentIndex], settingsManager.mutationSettingsPersistent);

        foodGenomePoolArray[foodIndex] = newFoodGenome;

        foodArray[foodIndex].InitializeFoodFromGenome(foodGenomePoolArray[foodIndex], GetRandomFoodSpawnPosition(), null); // Spawn that genome in dead Agent's body and revive it!
               
    }
    private StartPositionGenome GetRandomAgentSpawnPosition(int speciesIndex) {
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;

        int randZone = UnityEngine.Random.Range(0, numSpawnZones);

        int numPotentialParents = numFood / numSpecies;
        int lowIndex = numPotentialParents * speciesIndex;
        int highIndex = (numPotentialParents + 1) * speciesIndex;

        int randParentIndex = UnityEngine.Random.Range(lowIndex, highIndex);


        float randRadius = startPositionsPresets.spawnZonesList[randZone].radius;

        Vector2 randOffset = UnityEngine.Random.insideUnitCircle * randRadius;

        if(randParentIndex >= foodArray.Length)
        {
            Debug.Log("ERROR!: " + randParentIndex.ToString() + ", " + numFood.ToString());
        }
        Vector3 parentEggPos = foodArray[randParentIndex].transform.position;

        Vector3 agentStartPos = new Vector3(parentEggPos.x + randOffset.x,
                               parentEggPos.y + randOffset.y,
                               0f);

        Vector3 randStartPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);

        Vector3 startPos = Vector3.Lerp(agentStartPos, randStartPos, Mathf.Round(UnityEngine.Random.Range(0f, 1f)));
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        return startPosGenome;

        // OLD:
        /*Vector3 startPos = new Vector3(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-50f, 50f), 0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        return startPosGenome;*/
    }
    private StartPositionGenome GetInitialAgentSpawnPosition(int speciesIndex)
    {
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;

        int randZone = UnityEngine.Random.Range(0, numSpawnZones);
        
        float randRadius = startPositionsPresets.spawnZonesList[randZone].radius;

        Vector2 randOffset = UnityEngine.Random.insideUnitCircle * randRadius;

        Vector3 startPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        return startPosGenome;

    }
    private StartPositionGenome GetRandomFoodSpawnPosition() {

        Vector3 startPos;
        StartPositionGenome startPosGenome;

        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;

        int randZone = UnityEngine.Random.Range(0, numSpawnZones);

        for(int i = 0; i < 10; i++) {

            randZone = UnityEngine.Random.Range(0, numSpawnZones);

            if(startPositionsPresets.spawnZonesList[randZone].active) {
                break;  // use this one
            }
            else {
                if(startPositionsPresets.spawnZonesList[randZone].refactoryCounter > 300) {
                    startPositionsPresets.spawnZonesList[randZone].refactoryCounter = 0;
                    startPositionsPresets.spawnZonesList[randZone].active = true;
                    break;
                }
                else {
                    // do nothing, try again
                }
            }

            if(i == 9) {
                //made it to end of loop - food will never spawn again? or just plop it wherever, refactory be damned..
                Debug.Log("No active spawnZones found!");
            }
        }
        //Debug.Log("Rand Zone: " + randZone.ToString());
        float randRadius = startPositionsPresets.spawnZonesList[randZone].radius;

        Vector2 randOffset = UnityEngine.Random.insideUnitCircle * randRadius;

        startPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);
        startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        // Ttry to find a suitable startPos:
        /*int numParentSearches = 4;
        //int parentIndex = -1;
        for(int i = 0; i < numParentSearches; i++) {
            int parentIndex = UnityEngine.Random.Range(0, numFood);
            if(foodArray[parentIndex].curLifeStage == FoodModule.FoodLifeStage.Mature) {
                startPos = new Vector3(foodArray[parentIndex].transform.position.x, foodArray[parentIndex].transform.position.x, 0f) + new Vector3(0f, 0f, 0f);
                startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);

                return startPosGenome;
            }            
        }*/

        //startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-50f, 50f), 0f);
        //startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        return startPosGenome;
    }
    private StartPositionGenome GetRandomEggClusterSpawnPosition(int speciesIndex)
    {

        Vector3 startPos;
        StartPositionGenome startPosGenome;

        int numPotentialParents = numAgents / numSpecies;

        int lowIndex = numPotentialParents * speciesIndex;
        int highIndex = (numPotentialParents + 1) * speciesIndex;

        int parentIndex = UnityEngine.Random.Range(lowIndex, highIndex);

        for (int i = 0; i < 10; i++)
        {
            int randParentIndex = UnityEngine.Random.Range(lowIndex, highIndex);

            if (agentsArray[randParentIndex].curLifeStage == Agent.AgentLifeStage.Mature && agentsArray[randParentIndex].coreModule.energyRaw / agentsArray[randParentIndex].coreModule.maxEnergyStorage > 0.5f)
            {
                parentIndex = randParentIndex;
                break;
            }
        }
        

        float randRadius = 1f;

        Vector2 randOffset = UnityEngine.Random.insideUnitCircle * randRadius;

        Vector3 parentAgentPos = agentsArray[parentIndex].bodyRigidbody.transform.position;

        startPos = new Vector3(parentAgentPos.x + randOffset.x,
                               parentAgentPos.y + randOffset.y,
                               0f);
        startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        
        return startPosGenome;
    }
    private Vector3 GetRandomPredatorSpawnPosition() {

        Vector3 startPos;
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;
        int randZone = UnityEngine.Random.Range(0, numSpawnZones);
        float randRadius = 10f;

        Vector2 randOffset = UnityEngine.Random.insideUnitCircle.normalized * randRadius;
        startPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);
        return startPos;
    }
    public void ProcessDeadPlayer() {
        
        //playerIsDead = true;  // so this function won't be called continuously
        // Display Death screen, send player's Score, cause of death, etc.
        lastPlayerScore = agentsArray[0].scoreCounter;
        if (agentsArray[0].scoreCounter > recordPlayerAge) {
            recordPlayerAge = agentsArray[0].scoreCounter;
        }
        // Wait certain amount of time OR press enter to immediately respawn.
        // Countdown display showing time to auto-respawn
        bool didStarve = true;
        if(agentsArray[0].wasImpaled) {
            didStarve = false;
        }
        Debug.Log("ProcessDeadPlayer! starved: " + didStarve.ToString());
        uiManager.PlayerDied(didStarve);
        // Fade To Black
        
    }
    public void RespawnPlayer() {
        Debug.Log("Manual New Player RESPAWN!");

        cameraManager.SetTarget(agentsArray[0], 0);

        // respawn the Agent // mutates, respawns, updates RenderKingBuffers

        //agentsArray[0].humanControlled = true;
        //agentsArray[0].humanControlLerp = 1f;
        CreateMutatedCopyOfAgent(0, 0);         
        theRenderKing.UpdateAgentSmearStrokesBuffer(0);
        //theRenderKing.UpdateAgentBodyStrokesBuffer(0);
        theRenderKing.UpdateAgentEyeStrokesBuffer(0);
        theRenderKing.SimPlayerGlow();
        
        // Adjust Camera to position of agent
        //cameraManager.SetTarget(cameraManager.targetAgent, cameraManager.targetTransform, cameraManager.targetCritterIndex);
        //cameraManager.StartPlayerRespawn();
        // Fade-in from black?
        // Reset things that need to be reset i.e score counter?

        // Agent energy display should grow as agent grows through Egg stage
                
        //playerIsDead = false;
    }
    public void EnterObserverMode() {
        agentsArray[0].humanControlled = false;
        agentsArray[0].humanControlLerp = 0f;

        //playerIsDead = false; // maybe rename this? bypasses Agent lifeCycleStage being Null for extended periods...
    }
    
    /*
    private void MutateBody {
        //====================================================================================================================================
        // Mutate Body Colors:
        // Mutate Body Colors:
        // Mutate Body Colors:
        agentsArray[agentIndex].huePrimary = agentsArray[parentIndex].huePrimary;
        agentsArray[agentIndex].hueSecondary = agentsArray[parentIndex].hueSecondary;

        agentsArray[agentIndex].size = agentsArray[parentIndex].size;
        float randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // SIZE
            agentsArray[agentIndex].size.x = Mathf.Clamp(agentsArray[agentIndex].size.x + Gaussian.GetRandomGaussian(0f, 0.1f), 0.5f, 2f);
            agentsArray[agentIndex].size.y = Mathf.Clamp(agentsArray[agentIndex].size.y + Gaussian.GetRandomGaussian(0f, 0.1f), 0.5f, 2f);
        }
        randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // RED
            agentsArray[agentIndex].huePrimary.x = Mathf.Clamp01(agentsArray[agentIndex].huePrimary.x + Gaussian.GetRandomGaussian(0f, 0.1f));
        }
        randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // GREEN
            agentsArray[agentIndex].huePrimary.y = Mathf.Clamp01(agentsArray[agentIndex].huePrimary.y + Gaussian.GetRandomGaussian(0f, 0.1f));
        }
        randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // BLUE
            agentsArray[agentIndex].huePrimary.z = Mathf.Clamp01(agentsArray[agentIndex].huePrimary.z + Gaussian.GetRandomGaussian(0f, 0.1f));
        }
        randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // RED
            agentsArray[agentIndex].hueSecondary.x = Mathf.Clamp01(agentsArray[agentIndex].hueSecondary.x + Gaussian.GetRandomGaussian(0f, 0.1f));
        }
        randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // GREEN
            agentsArray[agentIndex].hueSecondary.y = Mathf.Clamp01(agentsArray[agentIndex].hueSecondary.y + Gaussian.GetRandomGaussian(0f, 0.1f));
        }
        randomMutate = UnityEngine.Random.Range(0f, 1f);
        if (randomMutate < 0.08f) {  // BLUE
            agentsArray[agentIndex].hueSecondary.z = Mathf.Clamp01(agentsArray[agentIndex].hueSecondary.z + Gaussian.GetRandomGaussian(0f, 0.1f));
        }
        agentsArray[agentIndex].bodyPointStroke.parentIndex = agentIndex; // agentsArray[parentIndex].decorationPointStrokesArray[b].parentIndex;
        agentsArray[agentIndex].bodyPointStroke.strength = agentsArray[parentIndex].bodyPointStroke.strength;
        agentsArray[agentIndex].bodyPointStroke.localScale = agentsArray[parentIndex].bodyPointStroke.localScale;
        agentsArray[agentIndex].bodyPointStroke.localPos = agentsArray[parentIndex].bodyPointStroke.localPos;
        agentsArray[agentIndex].bodyPointStroke.localDir = agentsArray[parentIndex].bodyPointStroke.localDir;
        agentsArray[agentIndex].bodyPointStroke.hue = agentsArray[agentIndex].huePrimary;

        for (int b = 0; b < agentsArray[agentIndex].decorationPointStrokesArray.Length; b++) {

            agentsArray[agentIndex].decorationPointStrokesArray[b].parentIndex = agentIndex; // agentsArray[parentIndex].decorationPointStrokesArray[b].parentIndex;
            agentsArray[agentIndex].decorationPointStrokesArray[b].strength = agentsArray[parentIndex].decorationPointStrokesArray[b].strength;
            agentsArray[agentIndex].decorationPointStrokesArray[b].localScale = agentsArray[parentIndex].decorationPointStrokesArray[b].localScale;
            agentsArray[agentIndex].decorationPointStrokesArray[b].localPos = agentsArray[parentIndex].decorationPointStrokesArray[b].localPos;
            agentsArray[agentIndex].decorationPointStrokesArray[b].localDir = agentsArray[parentIndex].decorationPointStrokesArray[b].localDir;
            agentsArray[agentIndex].decorationPointStrokesArray[b].hue = agentsArray[parentIndex].decorationPointStrokesArray[b].hue;

            // Don't mutate Eyes:
            if (b > 1) {
                // mutate decoration stroke:
                randomMutate = UnityEngine.Random.Range(0f, 1f);
                if (randomMutate < 0.1f) {  // 
                    agentsArray[agentIndex].decorationPointStrokesArray[b].strength = Mathf.Clamp01(agentsArray[agentIndex].decorationPointStrokesArray[b].strength + Gaussian.GetRandomGaussian(0f, 0.25f));
                }
                randomMutate = UnityEngine.Random.Range(0f, 1f);
                if (randomMutate < 0.1f) {  // Scale
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localScale.x = Mathf.Clamp(agentsArray[agentIndex].decorationPointStrokesArray[b].localScale.x + Gaussian.GetRandomGaussian(0f, 0.05f), 0.1f, 0.5f);
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localScale.y = Mathf.Clamp(agentsArray[agentIndex].decorationPointStrokesArray[b].localScale.y + Gaussian.GetRandomGaussian(0f, 0.05f), 0.1f, 0.5f);
                }
                randomMutate = UnityEngine.Random.Range(0f, 1f);
                if (randomMutate < 0.1f) {  // Position
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localPos.x = Mathf.Clamp(agentsArray[agentIndex].decorationPointStrokesArray[b].localPos.x + Gaussian.GetRandomGaussian(0f, 0.05f), -0.35f, 0.35f);
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localPos.y = Mathf.Clamp(agentsArray[agentIndex].decorationPointStrokesArray[b].localPos.y + Gaussian.GetRandomGaussian(0f, 0.05f), -0.35f, 0.35f);
                }
                randomMutate = UnityEngine.Random.Range(0f, 1f);
                if (randomMutate < 0.1f) {  // Direction
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localDir.x = agentsArray[agentIndex].decorationPointStrokesArray[b].localDir.x + Gaussian.GetRandomGaussian(0f, 0.1f);
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localDir.y = agentsArray[agentIndex].decorationPointStrokesArray[b].localDir.y + Gaussian.GetRandomGaussian(0f, 0.1f);
                    agentsArray[agentIndex].decorationPointStrokesArray[b].localDir = agentsArray[agentIndex].decorationPointStrokesArray[b].localDir.normalized;
                }
                randomMutate = UnityEngine.Random.Range(0f, 1f);
                if (randomMutate < 0.1f) {  // Brush Type
                    agentsArray[agentIndex].decorationPointStrokesArray[b].brushType = UnityEngine.Random.Range(0, 4);
                }

                agentsArray[agentIndex].decorationPointStrokesArray[b].hue = Vector3.Lerp(agentsArray[agentIndex].huePrimary, agentsArray[agentIndex].hueSecondary, agentsArray[agentIndex].decorationPointStrokesArray[b].strength);
            }
        }
        // Mutate Body Colors:
        // Mutate Body Colors:
        // Mutate Body Colors:
        //====================================================================================================================================
    }
    */ // Mutate Body remnant code
    #endregion

    #region Utility Functions // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& UTILITY FUNCTIONS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    public int GetAgentIndexByLottery(float[] rankedFitnessList, int[] rankedIndicesList, int speciesIndex) {
        int selectedIndex = 0;

        int popSize = (numAgents / numSpecies);
        int startIndex = popSize * speciesIndex;
        int endIndex = popSize * (speciesIndex + 1);
        
        // calculate total fitness of all EVEN and ODD agents separately!
        float totalFitness = 0f;
        for (int i = startIndex; i < endIndex; i++) {            
            totalFitness += rankedFitnessList[i];
        }
        // generate random lottery value between 0f and totalFitness:
        float lotteryValue = UnityEngine.Random.Range(0f, totalFitness);
        float currentValue = 0f;
        for (int i = startIndex; i < endIndex; i++) {
            if (lotteryValue >= currentValue && lotteryValue < (currentValue + rankedFitnessList[i])) {
                // Jackpot!
                selectedIndex = rankedIndicesList[i];
                //Debug.Log("Selected: " + selectedIndex.ToString() + "! (" + i.ToString() + ") fit= " + currentValue.ToString() + "--" + (currentValue + (1f - rankedFitnessList[i])).ToString() + " / " + totalFitness.ToString() + ", lotto# " + lotteryValue.ToString() + ", fit= " + (1f - rankedFitnessList[i]).ToString());
            }
            currentValue += rankedFitnessList[i]; // add this agent's fitness to current value for next check
        }
        return selectedIndex;
    }

    #endregion
    
    private void OnDisable() {
        if(simStateData != null) {
            if (simStateData.agentSimDataCBuffer != null) {
                simStateData.agentSimDataCBuffer.Release();
            }
            if (simStateData.critterInitDataCBuffer != null) {
                simStateData.critterInitDataCBuffer.Release();
            }
            if (simStateData.critterSimDataCBuffer != null) {
                simStateData.critterSimDataCBuffer.Release();
            }
            if (simStateData.debugBodyResourcesCBuffer != null) {
                simStateData.debugBodyResourcesCBuffer.Release();
            }
            if (simStateData.agentMovementAnimDataCBuffer != null) {
                simStateData.agentMovementAnimDataCBuffer.Release();
            }            
            if (simStateData.foodSimDataCBuffer != null) {
                simStateData.foodSimDataCBuffer.Release();
            }
            if (simStateData.predatorSimDataCBuffer != null) {
                simStateData.predatorSimDataCBuffer.Release();
            }
            if (simStateData.foodStemDataCBuffer != null) {
                simStateData.foodStemDataCBuffer.Release();
            }
            if (simStateData.foodLeafDataCBuffer != null) {
                simStateData.foodLeafDataCBuffer.Release();
            }
            if (simStateData.foodFruitDataCBuffer != null) {
                simStateData.foodFruitDataCBuffer.Release();
            }
        }
        
        if(tempTex1 != null) {
            tempTex1.Release();
            tempTex2.Release();
            tempTex4.Release();
            tempTex8.Release();
            tempTex16.Release();
        }
        if(foodParticlesNearestCritters1 != null) {
            foodParticlesNearestCritters1.Release();
            foodParticlesNearestCritters32.Release();
            foodParticlesNearestCritters1024.Release();
        }
        if(nutrientSamplesCBuffer != null) {
            nutrientSamplesCBuffer.Release();
        }
        if(foodParticlesCBuffer != null) {
            foodParticlesCBuffer.Release();
        }  
        if(foodParticlesCBufferSwap != null) {
            foodParticlesCBufferSwap.Release();
        } 
        if(closestFoodParticlesDataCBuffer != null) {
            closestFoodParticlesDataCBuffer.Release();
        }
        if(foodParticlesEatAmountsCBuffer != null) {
            foodParticlesEatAmountsCBuffer.Release();
        }
        if(foodParticlesMeasure32 != null) {
            foodParticlesMeasure32.Release();
        }
        if(foodParticlesMeasure1 != null) {
            foodParticlesMeasure1.Release();
        }
        
    }

    public void SaveTrainingData() {
        
        Debug.Log("SAVE Population!");
        GenePool pool = new GenePool(agentGenomePoolArray);
        string json = JsonUtility.ToJson(pool);
        Debug.Log(json);
        //Debug.Log(Application.dataPath);
        //string path = Application.dataPath + "/TrainingSaves/" + savename + ".json";
        string filePath = Path.Combine(Application.streamingAssetsPath, "testSave.json");
        //string path = Application.dataPath + "/Resources/SavedGenePools/testSave.json";
        //Debug.Log(Application.persistentDataPath);
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, json);
        
    }
    public void LoadTrainingData() {
        
        Debug.Log("LOAD Population!");
        //"E:\Unity Projects\GitHub\Evolution\Assets\GridSearchSaves\2018_2_13_12_35\GS_RawScores.json"
        string filePath = Path.Combine(Application.streamingAssetsPath, "testSave.json");
        //string filePath = Application.dataPath + "/Resources/SavedGenePools/testSave.json";
        // Read the json from the file into a string
        string dataAsJson = File.ReadAllText(filePath);
        // Pass the json to JsonUtility, and tell it to create a GameData object from it
        GenePool loadedData = JsonUtility.FromJson<GenePool>(dataAsJson);
        agentGenomePoolArray = loadedData.genomeArray;
        
    }
    
    #region OLD CODE: // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& OLD CODE! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    /*private void ResetTrainingForPersistent() {
        rawFitnessScoresArray = new float[agentGenomePoolSize];

        curGen = 0;
        curApproxGen = 0;
        avgFitnessLastGen = 0f;
        bestFitnessScore = 0f;
        numAgentsBorn = 0;
        currentOldestAgent = 0;

        recordPlayerAge = 0;
        recordBotAge = 0;
    
        rollingAverageAgentScore = 0f;
        if (fitnessScoresEachGenerationList == null) {
            fitnessScoresEachGenerationList = new List<float>();
        }
        else {
            fitnessScoresEachGenerationList.Clear();
        }
        agentAvgRecordScore = 1f;

        // Respawn Agents:
        RespawnAgents();
        for (int i = 0; i < foodArray.Length; i++) {            
            ReviveFood(i);
        }
        for(int i = 0; i < predatorArray.Length; i++) {
            RevivePredator(i);
        }
    }*/

    /*public void ToggleTrainingPersistent() {
        if (firstTimeTraining) {
            //ResetTrainingForNewGen();
            rawFitnessScoresArray = new float[agentGenomePoolSize];
            firstTimeTraining = false;
        }
        isTrainingPersistent = !isTrainingPersistent;
    }*/
    /*public void ResetGenomes() {
        LoadingInitializePopulationGenomes();
        curGen = 0;
        ResetTrainingForNewGenSupervised();
        ResetTrainingForPersistent();
        //UpdateAgentBrains();
    }*/

    /*private void RespawnAgents() {  // doesn't create the Agent classes, just resets their data
        for (int i = 0; i < agentGenomePoolSize; i++) {
            Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
            StartPositionGenome agentStartPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
            agentsArray[i].InitializeAgentFromGenome(agentGenomePoolArray[i], agentStartPosGenome);
        }
    } */ // *** confirm these are set up alright
  
    

    /*private void UpdateAgentBrains() {
        // UPDATE BRAINS TEST!!!!!
        for (int a = 0; a < agentsArray.Length; a++) {
            agentsArray[a].ReplaceBrain(supervisedGenomePoolArray[a % 32]);
        }
    }*/

    /*public void Update() {
        //cameraManager.targetTransform = playerAgent.transform;
        
        
        
        //Vector3 agentPos = Vector3.zero;
        //if (playerAgent != null) {
        //    agentPos = playerAgent.testModule.ownRigidBody2D.position;
        //}
        //Debug.Log("Update() AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());
    }*/

    /*public IEnumerator TestYieldFixedUpdate() {
        Vector3 agentPos = playerAgent.testModule.ownRigidBody2D.position;
        Debug.Log("Coroutine BEFORE! (pre-physics) AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());

        yield return new WaitForFixedUpdate();

        Debug.Log("Coroutine AFTER! (Yield WaitForFixedUpdate, supposedly after physics) AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());
    }
    public IEnumerator TestYieldEndOfFrame() {
        Vector3 agentPos = playerAgent.testModule.ownRigidBody2D.position;        
        yield return new WaitForEndOfFrame();
        Debug.Log("TestYieldEndOfFrame()) AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());
    }
    public void TestExecutionOrder() {
        Vector3 agentPos = playerAgent.testModule.ownRigidBody2D.position;
        Debug.Log("(During FixedUpdate - pre-physics) AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());        
    }*/

    /*public void InitializeNewSimulation() {
        //ToggleTrainingPersistent();        
        // Create Environment (Or have it pre-built)


        settingsManager.Initialize();

        fitnessScoresEachGenerationList = new List<float>();
        supervisedScoresList = new List<float>();

        agentFluidColliderMaterialsArray = new Material[numAgents];
        agentDebugTexturesArray = new Texture2D[numAgents];
        foodMaterialsArray = new Material[numFood];
        //foodDebugTexturesArray = new Texture2D[numFood];
        // Create initial population of Genomes:
        // Re-Factor:
        bodyGenomeTemplate = new BodyGenome();
        bodyGenomeTemplate.InitializeGenomeAsDefault();

        InitializePopulationGenomes();

        // Player's dummy Genome (required to initialize Agent Class):
        AgentGenome playerGenome = new AgentGenome(-1);
        playerGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
        playerGenome.InitializeRandomBrainFromCurrentBody(0.25f);  // player's dummy genome zeroed

        // Instantiate Player Agent
        string assetURL = "AgentPrefab";
        GameObject playerAgentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
        playerAgentGO.name = "PlayerAgent";
        //playerAgentGO.GetComponentInChildren<MeshRenderer>().material = playerMat;
        playerAgentGO.GetComponent<Rigidbody2D>().mass = 1f;
        //playerAgentGO.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        playerAgent = playerAgentGO.GetComponent<Agent>();
        //playerAgent.humanControlled = true;
        //playerAgent.humanControlLerp = 1f;
        playerAgent.speed *= 1f;
        StartPositionGenome playerStartPosGenome = new StartPositionGenome(Vector3.zero, Quaternion.identity);
        playerAgent.InitializeAgentFromGenome(playerGenome, playerStartPosGenome);
        Material pMat = new Material(fluidColliderMatTemplate);
        //playerAgentGO.GetComponentInChildren<MeshRenderer>().material = pMat;
        playerAgent.meshRendererFluidCollider.material = pMat;
        //playerAgent.material = pMat;
        Texture2D playerTex = new Texture2D(4, 2);  // Health, foodAmountRGB, 4 outCommChannels
        playerTex.filterMode = FilterMode.Point;
        playerAgent.texture = playerTex;
        playerAgent.meshRendererBeauty.material.SetTexture("_MainTex", playerTex);
        //playerAgent.material.SetFloat("_IsPlayer", 1.0f);

        //  APPEARANCE:::::
        playerAgent.huePrimary = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);
        playerAgent.hueSecondary = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);
        playerAgent.bodyPointStroke = theRenderKing.GeneratePointStrokeData(numAgents, Vector2.one, Vector2.zero, new Vector2(0f, 1f), playerAgent.huePrimary, 0f, 0);
        playerAgent.decorationPointStrokesArray = new TheRenderKing.PointStrokeData[10];
        // EYES:
        playerAgent.decorationPointStrokesArray[0] = theRenderKing.GeneratePointStrokeData(numAgents,
                                                                                            new Vector2(0.36f, 0.36f),
                                                                                            new Vector2(-0.25f, 0.45f),
                                                                                            new Vector2(0f, 1f),
                                                                                            Vector3.one,
                                                                                            1,
                                                                                            4);
        playerAgent.decorationPointStrokesArray[1] = theRenderKing.GeneratePointStrokeData(numAgents,
                                                                                            new Vector2(0.36f, 0.36f),
                                                                                            new Vector2(0.25f, 0.45f),
                                                                                            new Vector2(0f, 1f),
                                                                                            Vector3.one,
                                                                                            1,
                                                                                            4);
        float minSize = 0.15f;
        float maxSize = 0.45f;
        for (int j = 0; j < playerAgent.decorationPointStrokesArray.Length - 2; j++) {
            float lerpStrength = UnityEngine.Random.Range(0f, 1f);
            int randBrush = UnityEngine.Random.Range(0, 4);
            playerAgent.decorationPointStrokesArray[j + 2] = theRenderKing.GeneratePointStrokeData(numAgents,
                                                                                            new Vector2(UnityEngine.Random.Range(minSize, maxSize), UnityEngine.Random.Range(minSize, maxSize)),
                                                                                            UnityEngine.Random.insideUnitCircle * 0.35f,
                                                                                            UnityEngine.Random.insideUnitCircle.normalized,
                                                                                            Vector3.Lerp(playerAgent.huePrimary, playerAgent.hueSecondary, lerpStrength),
                                                                                            lerpStrength,
                                                                                            randBrush);
        }

        // $#!%@@@@@@@@@@@@ TEMP TEMP TEMP !%!#$%!#$%!@$%%!@$#%@$%@@@@@@@@@@@@@@@@@@@@@@@
        //playerAgent.meshRendererFluidCollider.enabled = false;
        //playerAgent.meshRendererBeauty.enabled = false;
        playerAgent.humanControlLerp = 1f;
        //playerAgent.meshRendererBeauty.material.SetFloat("_IsPlayer", 1.0f);
        playerAgent.humanControlled = true;
        playerAgent.testModule.foodAmountB[0] = 20f;
        // $#!%@@@@@@@@@@@@ TEMP TEMP TEMP !%!#$%!#$%!@$%%!@$#%@$%@@@@@@@@@@@@@@@@@@@@@@@

        cameraManager.targetTransform = playerAgent.transform;

        uiManager.healthDisplayTex = playerTex;
        uiManager.SetDisplayTextures();

        // Instantiate AI Agents
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
            agentGO.name = "Agent" + i.ToString();
            float randScale = UnityEngine.Random.Range(1f, 1f);
            agentGO.transform.localScale = new Vector3(randScale, randScale, randScale);
            Agent newAgent = agentGO.GetComponent<Agent>();
            //int numColumns = Mathf.RoundToInt(Mathf.Sqrt(numAgents));
            //int row = Mathf.FloorToInt(i / numColumns);
            //int col = i % numColumns;
            //Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
            //StartPositionGenome agentStartPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
            //StartPositionGenome agentStartPosGenome = new StartPositionGenome(new Vector3(1.25f * (col - numColumns / 2), -2.0f - (1.25f * row), 0f), Quaternion.identity);
            agentsArray[i] = newAgent; // Add to stored list of current Agents
            //newAgent.InitializeAgentFromGenome(supervisedGenomePoolArray[i], agentStartPosGenome);

            Material fluidColliderMat = new Material(fluidColliderMatTemplate);
            agentFluidColliderMaterialsArray[i] = fluidColliderMat;
            //agentGO.GetComponentInChildren<MeshRenderer>().material = mat;
            newAgent.meshRendererFluidCollider.material = fluidColliderMat;
            //newAgent.material = fluidColliderMat;

            Texture2D tex = new Texture2D(4, 2);  // Health, foodAmountRGB, 4 outCommChannels
            tex.filterMode = FilterMode.Point;
            agentDebugTexturesArray[i] = tex;
            newAgent.texture = tex;
            newAgent.meshRendererBeauty.material.SetTexture("_MainTex", tex);

            //  APPEARANCE:::::
            newAgent.huePrimary = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);
            newAgent.hueSecondary = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);            
            newAgent.bodyPointStroke = theRenderKing.GeneratePointStrokeData(i, Vector2.one, Vector2.zero, new Vector2(0f, 1f), newAgent.huePrimary, 0f, 0);
            newAgent.decorationPointStrokesArray = new TheRenderKing.PointStrokeData[10];
            // EYES:
            newAgent.decorationPointStrokesArray[0] = theRenderKing.GeneratePointStrokeData(i,
                                                                                            new Vector2(0.36f, 0.36f),
                                                                                            new Vector2(-0.25f, 0.45f),
                                                                                            new Vector2(0f, 1f),
                                                                                            Vector3.one,
                                                                                            1,
                                                                                            4);
            newAgent.decorationPointStrokesArray[1] = theRenderKing.GeneratePointStrokeData(i,
                                                                                            new Vector2(0.36f, 0.36f),
                                                                                            new Vector2(0.25f, 0.45f),
                                                                                            new Vector2(0f, 1f),
                                                                                            Vector3.one,
                                                                                            1,
                                                                                            4);
            for (int j = 0; j < newAgent.decorationPointStrokesArray.Length - 2; j++) {
                float lerpStrength = UnityEngine.Random.Range(0f, 1f);
                int randBrush = UnityEngine.Random.Range(0, 4);
                newAgent.decorationPointStrokesArray[j + 2] = theRenderKing.GeneratePointStrokeData(i, 
                                                                                                new Vector2(UnityEngine.Random.Range(minSize, maxSize), UnityEngine.Random.Range(minSize, maxSize)),
                                                                                                UnityEngine.Random.insideUnitCircle * 0.35f,
                                                                                                UnityEngine.Random.insideUnitCircle.normalized, 
                                                                                                Vector3.Lerp(newAgent.huePrimary, newAgent.hueSecondary, lerpStrength),
                                                                                                lerpStrength,
                                                                                                randBrush);
            }
        }

        // SPAWN AGENTS:
        RespawnAgents();

        // FOOODDDD!!!!
        foodArray = new FoodModule[numFood];
        SpawnFood();
        predatorArray = new PredatorModule[numPredators];
        SpawnPredators();

        // Send agent info to FluidBG & RenderKing:
        environmentFluidManager.agentsArray = agentsArray;
        environmentFluidManager.playerAgent = playerAgent;
        environmentFluidManager.foodArray = foodArray;
        environmentFluidManager.predatorsArray = predatorArray;

        theRenderKing.agentsArray = agentsArray;
        theRenderKing.playerAgent = playerAgent;
        theRenderKing.SetPointStrokesBuffer();
        theRenderKing.SetSimDataArrays();
        theRenderKing.InitializeAllAgentCurveData();

        LoadingInitializeGridCells();
        HookUpModules();
        InitializeTrainingApparatus();


        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        ToggleTrainingPersistent();
        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        // DON"T LEAVE THIS!!!
        //RunAutomatedGridSearch();
    }*/

    /*private void CopyDataSampleToModule(DataSample sample, TestModule module) {

        module.bias[0] = sample.inputDataArray[0];
        module.foodPosX[0] = sample.inputDataArray[1];
        module.foodPosY[0] = sample.inputDataArray[2];
        module.foodDirX[0] = sample.inputDataArray[3];
        module.foodDirY[0] = sample.inputDataArray[4];
        module.foodTypeR[0] = sample.inputDataArray[5];
        module.foodTypeG[0] = sample.inputDataArray[6];
        module.foodTypeB[0] = sample.inputDataArray[7];
        module.friendPosX[0] = sample.inputDataArray[8];
        module.friendPosY[0] = sample.inputDataArray[9];
        module.friendVelX[0] = sample.inputDataArray[10];
        module.friendVelY[0] = sample.inputDataArray[11];
        module.friendDirX[0] = sample.inputDataArray[12];
        module.friendDirY[0] = sample.inputDataArray[13];
        module.enemyPosX[0] = sample.inputDataArray[14];
        module.enemyPosY[0] = sample.inputDataArray[15];
        module.enemyVelX[0] = sample.inputDataArray[16];
        module.enemyVelY[0] = sample.inputDataArray[17];
        module.enemyDirX[0] = sample.inputDataArray[18];
        module.enemyDirY[0] = sample.inputDataArray[19];
        module.ownVelX[0] = sample.inputDataArray[20];
        module.ownVelY[0] = sample.inputDataArray[21];

        module.temperature[0] = sample.inputDataArray[22];
        module.pressure[0] = sample.inputDataArray[23];
        module.isContact[0] = sample.inputDataArray[24];
        module.contactForceX[0] = sample.inputDataArray[25];
        module.contactForceY[0] = sample.inputDataArray[26];
        module.hitPoints[0] = sample.inputDataArray[27];
        module.stamina[0] = sample.inputDataArray[28];
        module.foodAmountR[0] = sample.inputDataArray[29];
        module.foodAmountG[0] = sample.inputDataArray[30];
        module.foodAmountB[0] = sample.inputDataArray[31];
        module.distUp[0] = sample.inputDataArray[32];
        module.distTopRight[0] = sample.inputDataArray[33];
        module.distRight[0] = sample.inputDataArray[34];
        module.distBottomRight[0] = sample.inputDataArray[35];
        module.distDown[0] = sample.inputDataArray[36];
        module.distBottomLeft[0] = sample.inputDataArray[37];
        module.distLeft[0] = sample.inputDataArray[38];
        module.distTopLeft[0] = sample.inputDataArray[39];
        module.inComm0[0] = sample.inputDataArray[40];
        module.inComm1[0] = sample.inputDataArray[41];
        module.inComm2[0] = sample.inputDataArray[42];
        module.inComm3[0] = sample.inputDataArray[43];
    }*/
    /*private float CompareDataSampleToBrainOutput(DataSample sample, TestModule module) {
        float throttleX = Mathf.Round(module.throttleX[0] * 3f / 2f);// + module.throttleX[0];
        float throttleY = Mathf.Round(module.throttleY[0] * 3f / 2f);// + module.throttleY[0];
        float deltaX = sample.outputDataArray[0] - throttleX + (sample.outputDataArray[0] - module.throttleX[0]) * 0.25f;  // Change this later?
        float deltaY = sample.outputDataArray[1] - throttleY + (sample.outputDataArray[1] - module.throttleY[0]) * 0.25f;
        float distSquared = deltaX * deltaX + deltaY * deltaY;
        return distSquared;
    }*/
    /*public void RunAutomatedGridSearch() {
        // Runs a bunch of simulations with different settings and saves the results for later analysis
        //gridSearchManager = new GridSearchManager();
        bool newSearch = true;

        if (newSearch) {
            if (isTrainingPersistent) {
                gridSearchManager.InitializeGridSearch(settingsManager.mutationSettingsPersistent, true);
            }
            if (isTrainingSupervised) {
                gridSearchManager.InitializeGridSearch(settingsManager.mutationSettingsSupervised, false);
            }
        }
        else {
            gridSearchManager.ResumeGridSearch();
        }
        isGridSearching = true;
        //isTrainingPersistent = true;

        ResetGenomes();
    }*/
    /*public void UpdateGridSearch(int curGen, float score) {
        //Debug.Log("UpdateGridSearch(" + curGen.ToString() + ", float score)");
        // run every "Generation"
        //gridSearchManager.UpdateGridSearch();

        // Save data regardless?:::
        gridSearchManager.DataEntry(curGen, score);

        if (curGen >= gridSearchManager.GetNumGens()) {
            // Save this Gen?
            GenePool pool = new GenePool(persistentGenomePoolArray);
            gridSearchManager.storedResults.genePoolList.Add(pool);
            // Next Run!
            gridSearchManager.StartNewRun();
            ResetGenomes();
            if (isTrainingSupervised) {
                ResetSupervisedTraining();
            }
        }
        else {
            // Save data
            //gridSearchManager.DataEntry(curGen, score);
        }
        if (gridSearchManager.isComplete) {
            //Debug.Log("UpdateGridSearch(gridSearchManager.isComplete)");
            isGridSearching = false;
        }
    }*/

    #endregion

}
