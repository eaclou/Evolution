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
    public MasterGenomePool masterGenomePool;
    public FoodManager foodManager;

    public bool isQuickStart = true;

    public float curPlayerMutationRate = 0.75f;  // UI-based value, giving player control over mutation frequency with one parameter

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
    private int numWarmUpTimeSteps = 60;
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
    
    public Agent[] agentsArray;    
    //public AgentGenome[] agentGenomePoolArray;  *** OLD

    //private AgentGenome[] savedGenomePoolArray1;
    //private AgentGenome[] savedGenomePoolArray2;
    //private AgentGenome[] savedGenomePoolArray3;
    public EggSackGenome[] eggSackGenomePoolArray;
    //private EggSackGenome foodGenomeAnimalCorpse;
    public EggSack[] eggSackArray;
    private int numEggSacks = 48;
    public int _NumEggSacks {
        get
        {
            return numEggSacks;
        }
        set
        {
            numEggSacks = value;
        }
    }    
    
    public float[] rawFitnessScoresArray;
    private int[] rankedIndicesList;
    private float[] rankedFitnessList;
    public int numAgentsBorn = 0;
    public int numAgentsDied = 0;
    public int currentOldestAgent = 0;
    
        // Species
    private int numSpecies = 4;
    public float[] speciesAvgFoodEaten;
    public Vector2[] speciesAvgSizes;
    public float[] speciesAvgMouthTypes;
    
    public int recordBotAge = 0;
    public Vector4 statsAvgGlobalNutrients;
    public float statsAvgMutationRate;
    public float[] rollingAverageAgentScoresArray;
    //public List<Vector4> statsLifespanEachGenerationList;
    //public List<Vector4> statsBodySizesEachGenerationList;
    //public List<Vector4> statsFoodEatenEachGenerationList;
    //public List<Vector4> statsPredationEachGenerationList;
    public List<Vector4> statsNutrientsEachGenerationList;    
    //public List<float> statsMutationEachGenerationList;
    //public List<Color> statsSpeciesPrimaryColorsList;
    //public List<Color> statsSpeciesSecondaryColorsList;
    public float agentAvgRecordScore = 1f;
    public int curApproxGen = 1;

    public int numInitialHiddenNeurons = 16;
            
    private int numAgentsProcessed = 0;

    private int eggSackRespawnCounter;    
    private int agentRespawnCounter = 0;

    private int numAgentEvaluationsPerGenome = 1;

    public int simAgeTimeSteps = 0;
    private int numStepsInSimYear = 2000;
    private int simAgeYearCounter = 0;
    public int curSimYear = 0;

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

                cameraManager.SetTarget(agentsArray[0], 0);  // otherwise it's null and a giant mess

                // Populate renderKingBuffers:
                //for(int i = 0; i < numAgents; i++) {
                //    theRenderKing.UpdateAgentBodyStrokesBuffer(i); // hacky fix but seems to work...
                //}

                //RespawnPlayer(); // Needed???? *****                
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

        // create first EggSacks:
        LoadingInstantiateEggSacks();
        yield return null;
        LoadingInitializeEggSacksFirstTime();

        yield return null;

        // Can I create Egg Sacks and then immediately sapwn agents on them
        // Initialize Agents:
        LoadingInstantiateAgents();  // Fills the AgentsArray, Instantiates Agent Objects (MonoBehaviors + GameObjects)
        // Do *** !!!! *** v v v *** This is being replaced by a different mechanism for spawning Agents:
        //LoadingInitializeAgentsFromGenomes(); // This was "RespawnAgents()" --  Used to actually place the Agent in the game in a random spot and set the Agent's atributes ffrom its genome

        yield return null;

        // Initialize Food:
        LoadingInitializeFoodParticles();

        yield return null;

        LoadingInitializeFoodGrid();
        
        
        yield return null;

        // Load pre-saved genomes:
        //LoadingLoadGenepoolFiles();       
        //yield return null;

        // **** How to handle sharing simulation data between different Managers???
        // Once Agents, Food, etc. are established, Initialize the Fluid:
        LoadingInitializeFluidSim();


        yield return null;
                
        // Wake up the Render King and prepare him for the day ahead, proudly ruling over Renderland.
        GentlyRouseTheRenderMonarchHisHighnessLordOfPixels();

        // TreeOfLife Render buffers here ???
        // Tree Of LIFE UI collider & RenderKing updates:
        //uiManager.treeOfLifeManager.AddNewSpecies(masterGenomePool, 0);
        theRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, 0);
        theRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, 1);
        masterGenomePool.completeSpeciesPoolsList[0].isFlaggedForExtinction = true;
        masterGenomePool.ExtinctifySpecies(this, masterGenomePool.currentlyActiveSpeciesIDList[0]);

        // TEMP!!! ****
        /*for(int i = 0; i < numAgents; i++) {
            theRenderKing.UpdateAgentWidthsTexture(agentsArray[i]);
        }*/
        
        yield return null;
                
        LoadingHookUpFluidAndRenderKing();  // fluid needs refs to RK's obstacle/color cameras' RenderTextures!
        // ***** ^^^^^ Might need to call this every frame???

        // Hook up Camera to data -- fill out CameraManager class
        //cameraManager.SetTarget(cameraManager.targetAgent, cameraManager.targetCritterIndex);
        //cameraManager.targetTransform = agentsArray[cameraManager.targetCritterIndex].bodyGO.transform;
        // ***** Hook up UI to proper data or find a way to handle that ****
        // possibly just top-down let cameraManager read simulation data
        //LoadingHookUpUIManager();
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
        foodManager = new FoodManager(settingsManager);

        //debugScores:
        rollingAverageAgentScoresArray = new float[numSpecies];

        speciesAvgFoodEaten = new float[numSpecies];
        speciesAvgSizes = new Vector2[numSpecies];
        speciesAvgMouthTypes = new float[numSpecies];
                        
        LoadingInitializePopulationGenomes();

        if(isQuickStart) {
            Debug.Log("QUICK START! Loading Pre-Trained Genomes!");
            LoadTrainingData();
        }

        simStateData = new SimulationStateData(this);

        //uiManager.treeOfLifeManager = new TreeOfLifeManager(uiManager.treeOfLifeAnchorGO, uiManager);  // Moved inside MasterGenome Init!!! ***
        //uiManager.treeOfLifeManager.FirstTimeInitialize(masterGenomePool);
    }
    private void LoadingInitializePopulationGenomes() {
        masterGenomePool = new MasterGenomePool();
        masterGenomePool.FirstTimeInitialize(numAgents, settingsManager.mutationSettingsPersistent, uiManager);
               

        /*agentGenomePoolArray = new AgentGenome[numAgents];
        for (int i = 0; i < agentGenomePoolArray.Length; i++) {   // Create initial Population Supervised Learners
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.GenerateInitialRandomBodyGenome();
            //agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);  // OLD
            agentGenome.InitializeRandomBrainFromCurrentBody(settingsManager.mutationSettingsPersistent.initialConnectionChance, numInitialHiddenNeurons);
            agentGenomePoolArray[i] = agentGenome;
        }*/

        // THIS WILL BE FORCED TO BE REFACTORED!!! :::::::::
        // Sort Fitness Scores Persistent:
        rankedIndicesList = new int[numAgents];
        rankedFitnessList = new float[numAgents];

        for (int i = 0; i < rankedIndicesList.Length; i++) {
            rankedIndicesList[i] = i;
            rankedFitnessList[i] = 1f;
        }

        // EGGSACKS:
        eggSackGenomePoolArray = new EggSackGenome[numEggSacks];

        for(int i = 0; i < eggSackGenomePoolArray.Length; i++) {
            EggSackGenome eggSackGenome = new EggSackGenome(i);

            eggSackGenome.InitializeAsRandomGenome();

            eggSackGenomePoolArray[i] = eggSackGenome;
        }        
    }
    private void LoadingInitializeFluidSim() {
        environmentFluidManager.InitializeFluidSystem();
    }
    private void GentlyRouseTheRenderMonarchHisHighnessLordOfPixels() {
        theRenderKing.InitializeRiseAndShine(this);
    }
    private void LoadingInstantiateAgents() {
        
        // Instantiate AI Agents
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = new GameObject("Agent" + i.ToString());
            Agent newAgent = agentGO.AddComponent<Agent>();
            //newAgent.speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
            newAgent.FirstTimeInitialize(); // agentGenomePoolArray[i]);
            agentsArray[i] = newAgent; // Add to stored list of current Agents            
        }
    }
    /*private void LoadingInitializeAgentsFromGenomes() {        
        for (int i = 0; i < numAgents; i++) {
            
            int speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
            agentsArray[i].InitializeSpawnAgentFromGenome(i, agentGenomePoolArray[i]); //, GetInitialAgentSpawnPosition(speciesIndex));            
        }
    }*/
    private void LoadingInitializeFoodParticles() {

        foodManager.InitializeFoodParticles(numAgents, computeShaderFoodParticles);

        /*foodParticlesCBuffer = new ComputeBuffer(numFoodParticles, sizeof(float) * 8 + sizeof(int) * 3);
        foodParticlesCBufferSwap = new ComputeBuffer(numFoodParticles, sizeof(float) * 8 + sizeof(int) * 3);
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
        closestFoodParticlesDataCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 8 + sizeof(int) * 3);

        foodParticlesEatAmountsCBuffer = new ComputeBuffer(numAgents, sizeof(float) * 1);
        foodParticlesEatAmountsArray = new float[numAgents];

        foodParticleMeasurementTotalsData = new FoodParticleData[1];
        foodParticlesMeasure32 = new ComputeBuffer(32, sizeof(float) * 8 + sizeof(int) * 3);
        foodParticlesMeasure1 = new ComputeBuffer(1, sizeof(float) * 8 + sizeof(int) * 3);
        */
    }
    private void LoadingInitializeFoodGrid() {

        foodManager.InitializeNutrientsMap(numAgents, computeShaderNutrientMap);

        /*        
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
        */
    }
    private void LoadingInstantiateEggSacks() {
        // FOOODDDD!!!!
        eggSackArray = new EggSack[numEggSacks]; // create array
        
        //Debug.Log("SpawnFood!");
        for (int i = 0; i < eggSackArray.Length; i++) {
            GameObject eggSackGO = new GameObject("EggSack" + i.ToString()); // Instantiate(Resources.Load("Prefabs/FoodPrefab")) as GameObject;
            //eggSackGO.name = "EggSack" + i.ToString();
            EggSack newEggSack = eggSackGO.AddComponent<EggSack>();
            newEggSack.speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[0]; // Mathf.FloorToInt((float)i / (float)numEggSacks * (float)numSpecies);
            newEggSack.FirstTimeInitialize();
            eggSackArray[i] = newEggSack; // Add to stored list of current Food objects                     
        }
    }
    private void LoadingInitializeEggSacksFirstTime() {  // Skip pregnancy, Instantiate EggSacks that begin as 'GrowingIndependent' ?
        for (int i = 0; i < eggSackArray.Length; i++) {
            eggSackArray[i].isDepleted = true;
            eggSackArray[i].curLifeStage = EggSack.EggLifeStage.Null;
            
        }
    }
    
    private void LoadingHookUpFluidAndRenderKing() {
        // **** NEED TO ADDRESS THIS!!!!!! ************************
        theRenderKing.fluidObstaclesRenderCamera.targetTexture = environmentFluidManager._ObstaclesRT; // *** See if this works **
        theRenderKing.fluidColorRenderCamera.targetTexture = environmentFluidManager._SourceColorRT;

        //temp:
        theRenderKing.debugRT = environmentFluidManager._SourceColorRT;
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
        statsNutrientsEachGenerationList = new List<Vector4>();
        statsNutrientsEachGenerationList.Add(Vector4.one * 0.0001f);
        /*
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
        */
    }
    /*private void LoadingLoadGenepoolFiles() {
        
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
        
    }*/

    #endregion

    #region Every Frame  //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& EVERY FRAME &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

    public void TickSimulation() {
        simAgeTimeSteps++;
        simAgeYearCounter++;
        if(simAgeYearCounter >= numStepsInSimYear) {
            curSimYear++;
            simAgeYearCounter = 0;

            // update graphs each "year"
            RefreshGraphData();
        }

        
        eggSackRespawnCounter++;
        agentRespawnCounter++;

        float totalNutrients = foodManager.MeasureTotalNutrients();
        foodManager.GetNutrientValuesAtMouthPositions(simStateData);
        //Find closest critters to foodParticles:
        foodManager.FindClosestFoodParticleToCritters(simStateData);
        foodManager.MeasureTotalFoodParticlesAmount();

        masterGenomePool.Tick(); // keep track of when species created so can't create multiple per frame?

                
        // ******** REVISIT CODE ORDERING!!!!  -- Should check for death Before or After agent Tick/PhysX ???
        CheckForDevouredEggSacks();
        CheckForNullAgents();  // Result of this will affect: "simStateData.PopulateSimDataArrays(this)" !!!!!
        CheckForReadyToSpawnAgents();
        

        simStateData.PopulateSimDataArrays(this);  // reads from GameObject Transforms & RigidBodies!!! ++ from FluidSimulationData!!!        
        theRenderKing.RenderSimulationCameras(); // will pass current info to FluidSim before it Ticks()
        // Reads from CameraRenders, GameObjects, and query GPU for fluidState
        // eventually, if agents get fluid sensors, will want to download FluidSim data from GPU into simStateData!*        
              
        // **** Figure out proper Execution Order / in which Class to Run RenderOps while being synced w/ results of physX sim!!!        
        theRenderKing.Tick(); // updates all renderData, buffers, brushStrokes etc.

        HookUpModules(); // Sets nearest-neighbors etc. feed current data into agent Brains
        
        // &&&&& STEP SIMULATION FORWARD:::: &&&&&&&&&&
        // &&&&& STEP SIMULATION FORWARD:::: &&&&&&&&&&
        // Load gameState into Agent Brain, process brain function, read out brainResults,
        // Execute Agent Actions -- apply propulsive force to each Agent:
        for (int i = 0; i < agentsArray.Length; i++) {
            // *** FIND FOOD GRID CELL!!!!  ************
            Vector2 agentPos = agentsArray[i].bodyRigidbody.transform.position;
            /*
            int foodGridIndexX = Mathf.FloorToInt(agentPos.x / mapSize * (float)foodGridResolution);
            foodGridIndexX = Mathf.Clamp(foodGridIndexX, 0, foodGridResolution - 1);
            int foodGridIndexY = Mathf.FloorToInt(agentPos.y / mapSize * (float)foodGridResolution);
            foodGridIndexY = Mathf.Clamp(foodGridIndexY, 0, foodGridResolution - 1);
            */
            //agentsArray[i].Tick(new Vector4(0f,0f,0f,0f), ref nutrientEatAmountsArray);  
            agentsArray[i].Tick(this, foodManager.nutrientSamplesArray[i], ref foodManager.nutrientEatAmountsArray, settingsManager);  
        }
        for (int i = 0; i < eggSackArray.Length; i++) {
            eggSackArray[i].Tick();
        }
        
        // Apply External Forces to dynamic objects: (internal PhysX Updates):        
        ApplyFluidForcesToDynamicObjects();

        foodManager.EatSelectedFoodParticles(simStateData); //         
        foodManager.RemoveEatenNutrients(numAgents, simStateData);
        float spawnNewFoodChance = settingsManager.spawnNewFoodChance;
        float spawnFoodPercentage = UnityEngine.Random.Range(0f, 0.25f);
        float maxGlobalFood = settingsManager.maxGlobalNutrients;

        if(totalNutrients < maxGlobalFood) {   // **** MOVE THIS INTO FOOD MANAGER?
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < spawnNewFoodChance) {
                // pick random cell:
                int randX = UnityEngine.Random.Range(0, foodManager.nutrientMapResolution - 1);
                int randY = UnityEngine.Random.Range(0, foodManager.nutrientMapResolution - 1);

                float foodAvailable = maxGlobalFood - totalNutrients;

                float newFoodAmount = Mathf.Min(1f, foodAvailable * spawnFoodPercentage);

                // ADD FOOD HERE::
                foodManager.AddNutrientsAtCoords(newFoodAmount, randX, randY);                
            }
        }
        foodManager.ApplyDiffusionOnNutrientMap(environmentFluidManager);
        foodManager.RespawnFoodParticles(environmentFluidManager, theRenderKing, simStateData);


        // TEMP AUDIO EFFECTS!!!!        
        float volume = agentsArray[0].smoothedThrottle.magnitude * 0.24f;
        audioManager.SetPlayerSwimLoopVolume(volume);              
                
        // Simulate timestep of fluid Sim - update density/velocity maps:
        // Or should this be right at beginning of frame????? ***************** revisit...
        environmentFluidManager.Tick(); // ** Clean this up, but generally OK
    }

    private void ApplyFluidForcesToDynamicObjects() {
        // ********** REVISIT CONVERSION btw fluid/scene coords and Force Amounts !!!! *************
        for (int i = 0; i < agentsArray.Length; i++) {

            Vector3 depthSample = simStateData.depthAtAgentPositionsArray[i];
            float agentSize = agentsArray[i].fullSizeBoundingBox.x * 1.6f + 0.15f;
            float floorDepth = depthSample.x * 10f;
            if (floorDepth < agentSize)
            {
                float wallForce = Mathf.Clamp01(agentSize - floorDepth) / agentSize;
                agentsArray[i].bodyRigidbody.AddForce(new Vector2(depthSample.y, depthSample.z).normalized * 50.20f * agentsArray[i].bodyRigidbody.mass * wallForce, ForceMode2D.Impulse);
            }
            
            agentsArray[i].bodyRigidbody.AddForce(simStateData.fluidVelocitiesAtAgentPositionsArray[i] * 30f * agentsArray[i].bodyRigidbody.mass, ForceMode2D.Impulse);

            agentsArray[i].avgFluidVel = Mathf.Lerp(agentsArray[i].avgFluidVel, simStateData.fluidVelocitiesAtAgentPositionsArray[i].magnitude, 0.25f);

            agentsArray[i].depth = depthSample.x;
        }
        for (int i = 0; i < eggSackArray.Length; i++) { // *** cache rigidBody reference
            
            eggSackArray[i].GetComponent<Rigidbody2D>().AddForce(simStateData.fluidVelocitiesAtEggSackPositionsArray[i] * 16f * eggSackArray[i].GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse); //
            
        }
    }
    
    public void PlayerToolStirOn(Vector3 origin, Vector2 forceVector, float radiusMult) {
        float magnitude = forceVector.magnitude;
        if(magnitude == 0f) {
            Debug.Log("ERROR null vector!");
        }
        magnitude *= 0.04f;
        float maxMag = 0.9f;
        if(magnitude > maxMag) {
            magnitude = maxMag;            
        }
        forceVector = forceVector.normalized * magnitude;

        //Debug.Log("PlayerToolStir pos: " + origin.ToString() + ", forceVec: [" + forceVector.x.ToString() + "," + forceVector.y.ToString() + "]  mag: " + magnitude.ToString());

        environmentFluidManager.StirWaterOn(origin, forceVector, radiusMult);
    }
    public void PlayerToolStirOff() {        
        environmentFluidManager.StirWaterOff();
    }
    public void PlayerFeedToolSprinkle(Vector3 pos) {
        Debug.Log("PlayerFeedToolSprinkle pos: " + pos.ToString());
        int[] respawnIndices = new int[4];
        for(int i = 0; i < respawnIndices.Length; i++) {
            respawnIndices[i] = UnityEngine.Random.Range(0, 1024);
        }
        foodManager.ReviveSelectFoodParticles(respawnIndices, 1.25f, new Vector4(pos.x / _MapSize, pos.y / _MapSize, 0f, 0f), simStateData);
    }
    public void PlayerFeedToolPour(Vector3 pos) {        
        int xCoord = Mathf.RoundToInt(pos.x / 256f * foodManager.nutrientMapResolution);
        int yCoord = Mathf.RoundToInt(pos.y / 256f * foodManager.nutrientMapResolution);

        Debug.Log("PlayerFeedToolPour pos: " + xCoord.ToString() + ", " + yCoord.ToString());

        foodManager.AddNutrientsAtCoords(5f, xCoord, yCoord);

        int[] respawnIndices = new int[32];
        for(int i = 0; i < respawnIndices.Length; i++) {
            respawnIndices[i] = UnityEngine.Random.Range(0, 1024);
        }
        foodManager.ReviveSelectFoodParticles(respawnIndices, 6f, new Vector4(pos.x / _MapSize, pos.y / _MapSize, 0f, 0f), simStateData);
    }
    public void ChangeGlobalMutationRate(float normalizedVal) {
        settingsManager.SetGlobalMutationRate(normalizedVal);
    }
    
    private void PopulateGridCells() {

        // Inefficient!!!
        for (int x = 0; x < agentGridCellResolution; x++) {
            for (int y = 0; y < agentGridCellResolution; y++) {
                mapGridCellArray[x][y].agentIndicesList.Clear();
                mapGridCellArray[x][y].eggSackIndicesList.Clear();
                //mapGridCellArray[x][y].predatorIndicesList.Clear();
            }
        }

        // !! ******* MAP CHANGE!! ***** Now starts at 0,0 (bottomLeft) and goes up to mapSize,mapSize (topRight) just like UV coords *****
        // This should make conversions between CPU and GPU coords a bit simpler in the long run
        
        // FOOD!!! :::::::
        for (int f = 0; f < eggSackArray.Length; f++) {
            
            float xPos = eggSackArray[f].transform.position.x;
            float yPos = eggSackArray[f].transform.position.y;
            int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            mapGridCellArray[xCoord][yCoord].eggSackIndicesList.Add(f);
                       
        }
        
        // FRIENDS::::::
        for (int a = 0; a < agentsArray.Length; a++) {
            float xPos = agentsArray[a].bodyGO.transform.position.x;
            float yPos = agentsArray[a].bodyGO.transform.position.y;
            int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
            xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
            yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);

            mapGridCellArray[xCoord][yCoord].agentIndicesList.Add(a);
        }
        
    }
    private void HookUpModules() {
                
        // mapSize is global now, get with the times, jeez-louise!
        //float cellSize = mapSize / agentGridCellResolution;
        //Vector2 playerPos = new Vector2(playerAgent.transform.localPosition.x, playerAgent.transform.localPosition.y);


        // ***** Check for inactive/Null agents and cull them from consideration:
        // ******  REFACTOR!!! BROKEN BY SPECIATION UPDATE! ***

        // Find NearestNeighbors:
        for (int a = 0; a < agentsArray.Length; a++) {
            if(agentsArray[a].curLifeStage != Agent.AgentLifeStage.Null && agentsArray[a].curLifeStage != Agent.AgentLifeStage.AwaitingRespawn) { // *****
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
                int closestEggSackIndex = -1; // default to -1??? ***            
                float nearestFoodDistance = float.PositiveInfinity;
            
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
                for (int i = 0; i < mapGridCellArray[xCoord][yCoord].agentIndicesList.Count; i++) {
                    int neighborIndex = mapGridCellArray[xCoord][yCoord].agentIndicesList[i];

                    if(agentsArray[neighborIndex].curLifeStage != Agent.AgentLifeStage.Null && agentsArray[neighborIndex].curLifeStage != Agent.AgentLifeStage.AwaitingRespawn) {
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
                }
            
                for (int i = 0; i < mapGridCellArray[xCoord][yCoord].eggSackIndicesList.Count; i++) {
                    // EggSacks:
                    int eggSackIndex = mapGridCellArray[xCoord][yCoord].eggSackIndicesList[i];

                    if(eggSackArray[eggSackIndex].curLifeStage != EggSack.EggLifeStage.Null) { // if enabled:
                        if(!eggSackArray[eggSackIndex].isProtectedByParent) {
                            //Debug.Log("Found valid Food!");
                            Vector2 eggSackPos = new Vector2(eggSackArray[eggSackIndex].rigidbodyRef.transform.position.x, eggSackArray[eggSackIndex].rigidbodyRef.transform.position.y);
                            float distEggSack = (eggSackPos - agentPos).magnitude - (eggSackArray[eggSackIndex].curSize.magnitude + 1f) * 0.5f;  // subtract food & agent radii

                            if (distEggSack <= nearestFoodDistance) { // if now the closest so far, update index and dist:

                                //int neighborSpeciesIndex = Mathf.FloorToInt((float)eggSackIndex / (float)numEggSacks * (float)numSpecies);

                                //if (ownSpeciesIndex != neighborSpeciesIndex) {  // if eggSack and Agent diff species
                                    //if (a != eggSackIndex) {  // make sure it doesn't consider itself:
                                closestEggSackIndex = eggSackIndex;
                                nearestFoodDistance = distEggSack;
                                    //}
                                //}                            
                            }
                        } 
                    }                             
                }
            
                // Set proper references between AgentBrains and Environment/Game Objects:::
                // ***** DISABLED!!!! *** NEED TO RE_IMPLEMENT THIS LATER!!!! ********************************************
                agentsArray[a].coreModule.nearestFriendAgent = agentsArray[closestFriendIndex];
                agentsArray[a].coreModule.nearestEnemyAgent = agentsArray[closestEnemyAgentIndex];
                if(closestEggSackIndex != -1) {
                    agentsArray[a].coreModule.nearestEggSackModule = eggSackArray[closestEggSackIndex];                
                }  
            }
                      
            //agentsArray[a].coreModule.nearestPredatorModule = predatorArray[closestPredIndex];            
        }
    }
    
    private void CheckForDevouredEggSacks() { // *** revisit
        // CHECK FOR DEAD FOOD!!! :::::::
        for (int f = 0; f < eggSackArray.Length; f++) {
            if (eggSackArray[f].isDepleted) {
                ProcessDeadEggSack(f);
            }
        }
    }
    private void CheckForNullAgents() { 
        for (int a = 0; a < agentsArray.Length; a++) {
            if (agentsArray[a].curLifeStage == Agent.AgentLifeStage.Null) {                
                ProcessNullAgent(agentsArray[a]);                                
            }
        }
    }  // *** revisit
    private void CheckForReadyToSpawnAgents() {        
        for (int a = 0; a < agentsArray.Length; a++) {
            if(agentRespawnCounter > 5) {
                if (agentsArray[a].curLifeStage == Agent.AgentLifeStage.AwaitingRespawn) {
                    //Debug.Log("AttemptToSpawnAgent(" + a.ToString() + ")");
                    AttemptToSpawnAgent(a);
                    agentRespawnCounter = 0;
                }
            }
        }                
    }
    private void AttemptToSpawnAgent(int agentIndex) { //, int speciesIndex) {

        // Which Species will the new agent belong to?
        // Random selection? Lottery-Selection among Species? Use this Agent's previous-life's Species?  Global Ranked Selection (across all species w/ modifiers) ?

        // Using Random Selection as first naive implementation:
        int randomTableIndex = UnityEngine.Random.Range(0, masterGenomePool.currentlyActiveSpeciesIDList.Count);
        int speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[randomTableIndex];
        
        // Find next-in-line genome waiting to be evaluated:
        CandidateAgentData candidateData = masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNextAvailableCandidate();

        //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString() + ") speciesIndex: " + speciesIndex.ToString() + " candidates: " + masterGenomePool.completeSpeciesPoolsList[speciesIndex].candidateGenomesList.Count.ToString());

        if(candidateData == null) {
            // all candidates are currently being tested, or candidate list is empty?
            //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString() + ") candidateData == null");
        }
        else {
            // Good to go?
            // Look for avaialbe EggSacks first:
            bool foundValidEggSack = false;
            EggSack parentEggSack = null;
            List<int> validEggSackIndicesList = new List<int>();
            for(int i = 0; i < numEggSacks; i++) {  // if EggSack belongs to the right species
                if(eggSackArray[i].curLifeStage == EggSack.EggLifeStage.Growing) {
                    if(eggSackArray[i].lifeStageTransitionTimeStepCounter < eggSackArray[i]._MatureDurationTimeSteps) {  // egg sack is at proper stage of development
                        if(eggSackArray[i].speciesIndex == speciesIndex) {
                            validEggSackIndicesList.Add(i);
                        }                        
                    }
                }
            }
            if(validEggSackIndicesList.Count > 0) {  // move this inside the for loop ??   // **** BROKEN BY SPECIATION UPDATE!!! *****
                int randIndex = UnityEngine.Random.Range(0, validEggSackIndicesList.Count);
                //Debug.Log("listLength:" + validEggSackIndicesList.Count.ToString() + ", randIndex = " + randIndex.ToString() + ", p: " + validEggSackIndicesList[randIndex].ToString());
                parentEggSack = eggSackArray[validEggSackIndicesList[randIndex]];
                SpawnAgentFromEggSack(candidateData, agentIndex, speciesIndex, parentEggSack);
                candidateData.isBeingEvaluated = true;
            }
            else { // No eggSack found:
                if(agentIndex == 0) {  // temp hack to avoid null reference exceptions:
                    SpawnAgentImmaculate(candidateData, agentIndex, speciesIndex);
                    candidateData.isBeingEvaluated = true;
                }                
            }
        }       
    }
       
    #endregion

    #region Process Events // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& PROCESS EVENTS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    
    // *** confirm these are set up alright       
    public void ProcessNullAgent(Agent agentRef) {   // (Upon Agent Death:)
        numAgentsDied++;
        // Now, this function should:
        // -- look up the connected CandidateGenome & its speciesID
        CandidateAgentData candidateData = agentRef.candidateRef;
        int agentSpeciesIndex = agentRef.speciesIndex;
        int candidateSpeciesIndex = candidateData.speciesID;
        if(agentSpeciesIndex != candidateSpeciesIndex) {
            Debug.LogError("agentSpeciesIndex (" + agentSpeciesIndex.ToString() + " != candidateSpeciesIndex (" + candidateSpeciesIndex.ToString());
        }
        //Debug.Log("masterGenomePool.completeSpeciesPoolsList: " + masterGenomePool.completeSpeciesPoolsList.Count.ToString());
        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex];

        // -- save its fitness score
        candidateData.ProcessCompletedEvaluation(agentRef);
        
        // -- check if it has finished all of its evaluations
        if(candidateData.numCompletedEvaluations >= numAgentEvaluationsPerGenome) {
            // -- If it has:
            // -- then push the candidate to Leaderboard List so it is eligible for reproduction
            // -- at the same time, remove it from the ToBeEvaluated pool
            speciesPool.ProcessCompletedCandidate(candidateData, masterGenomePool);
            float lerpAmount = Mathf.Max(0.01f, 1f / (float)speciesPool.numAgentsEvaluated);

            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgLifespan = Mathf.Lerp(speciesPool.avgLifespan, (float)agentRef.scoreCounter, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgConsumption = Mathf.Lerp(speciesPool.avgConsumption, (float)agentRef.totalFoodEaten, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgBodySize = Mathf.Lerp(speciesPool.avgBodySize, (float)agentRef.fullSizeBodyVolume, lerpAmount);
            float mouthType = 1f;
            if(agentRef.mouthRef.isPassive) {
                mouthType = 0f;
            }
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgDietType = Mathf.Lerp(speciesPool.avgDietType, mouthType, lerpAmount);
        }
        else {
            // -- Else:
            // -- (move to end of pool queue OR evaluate all Trials of one genome before moving onto the next)

        }

        // &&&&& *****  HERE!!!! **** &&&&&&   --- Select a species first to serve as parentGenome !! ***** &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        // Can be random selection (unbiased), or proportional to species avg Fitnesses?
        SpeciesGenomePool sourceSpeciesPool = masterGenomePool.SelectNewGenomeSourceSpecies();
        // -- Select a ParentGenome from the leaderboardList and create a mutated copy (childGenome):
        AgentGenome newGenome = sourceSpeciesPool.GetNewMutatedGenome();
        // -- Check which species this new childGenome should belong to (most likely its parent, but maybe it creates a new species or better fits in with a diff existing species)        
        masterGenomePool.AssignNewMutatedGenomeToSpecies(newGenome, sourceSpeciesPool.speciesID, this); // Checks which species this new genome should belong to and adds it to queue / does necessary processing   
                
        // -- Clear Agent object so that it's ready to be reused
            // i.e. Set curLifecycle to .AwaitingRespawn ^
            // then new Agents should use the next available genome from the updated ToBeEvaluated pool      
        
        agentRef.SetToAwaitingRespawn();

        ProcessAgentScores(agentRef);  // *** CLEAN THIS UP!!! ***

        // &&&&&& OLD OLD OLD &&&&&&&&&&&& OLD OLD &&&&&&&&&&&&&&&&& OLD &&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        /*
        CheckForRecordAgentScore(agentIndex);
        ProcessAgentScores(agentIndex);
        // Updates rankedIndicesArray[] so agents are ordered by score:
        int speciesIndex = Mathf.FloorToInt((float)agentIndex / (float)numAgents * (float)numSpecies);
        ProcessAndRankAgentFitness(speciesIndex);
        int parentGenomeIndex = GetAgentIndexByLottery(rankedFitnessList, rankedIndicesList, speciesIndex);
        SetAgentGenomeToMutatedCopyOfParentGenome(agentIndex, agentGenomePoolArray[parentGenomeIndex]);
        
        agentsArray[agentIndex].SetToAwaitingRespawn();
        */
    }
    // ********** RE-IMPLEMENT THIS LATER!!!! ******************************************************************************
    private void SpawnAgentFromEggSack(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex, EggSack parentEggSack) {

        // Refactor this function to work with new GenomePool architecture!!!

        //Debug.Log("SpawnAgentFromEggSack! " + agentIndex.ToString());
        numAgentsBorn++;
        currentOldestAgent = agentsArray[rankedIndicesList[0]].scoreCounter;
        agentsArray[agentIndex].InitializeSpawnAgentFromEggSack(agentIndex, sourceCandidate, parentEggSack); // Spawn that genome in dead Agent's body and revive it!
        theRenderKing.UpdateCritterGenericStrokesData(agentsArray[agentIndex]); // agentIndex, sourceCandidate.candidateGenome);
        //theRenderKing.UpdateAgentWidthsTexture(agentsArray[agentIndex]);
                
        //agentRespawnCounterArrayOld[speciesIndex] = 0;
        //agentRespawnCounter = 0;
    }
    private void SpawnAgentImmaculate(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex) {

        // Refactor this function to work with new GenomePool architecture!!!
        

        numAgentsBorn++;
        currentOldestAgent = agentsArray[rankedIndicesList[0]].scoreCounter;
        agentsArray[agentIndex].InitializeSpawnAgentImmaculate(agentIndex, sourceCandidate, GetRandomFoodSpawnPosition()); // Spawn that genome in dead Agent's body and revive it!
        theRenderKing.UpdateCritterGenericStrokesData(agentsArray[agentIndex]); //agentIndex, sourceCandidate.candidateGenome);
        //theRenderKing.UpdateAgentWidthsTexture(agentsArray[agentIndex]);
                
        //agentRespawnCounterArrayOld[speciesIndex] = 0;
        
    }
    public void ProcessDeadEggSack(int eggSackIndex) {
        //Debug.Log("ProcessDeadEggSack(" + eggSackIndex.ToString() + ") eggSackRespawnCounter " + eggSackRespawnCounter.ToString());

        int numActiveSpecies = masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int randEggSpeciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[UnityEngine.Random.Range(0, numActiveSpecies)];
        eggSackArray[eggSackIndex].speciesIndex = randEggSpeciesIndex;

        // Check for timer?
        if(eggSackRespawnCounter > 3) {
            
            // How many active EggSacks are there in play?
            int totalSuitableParentAgents = 0;
            List<int> suitableParentAgentsList = new List<int>();
            for(int i = 0; i < _NumAgents; i++) {
                if(agentsArray[i].speciesIndex == eggSackArray[eggSackIndex].speciesIndex) {
                    if(agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                        if(agentsArray[i].isPregnantAndCarryingEggs) {
                            // already carrying, not available
                        }
                        else {
                            if(agentsArray[i].pregnancyRefactoryTimeStepCounter > agentsArray[i].pregnancyRefactoryDuration) {
                                // Able to grow eggs
                                totalSuitableParentAgents++;
                                suitableParentAgentsList.Add(i);
                            }
                        }
                    }
                    else {

                    }
                }                
            }
        
            if(totalSuitableParentAgents > 0) {
                // At Least ONE fertile Agent available:
                int randParentAgentIndex = suitableParentAgentsList[UnityEngine.Random.Range(0, totalSuitableParentAgents)];

                // RespawnFood  // *** REFACTOR -- Need to sync egg and agent genomes to match each other
                EggSackGenome newEggSackGenome = new EggSackGenome(eggSackIndex);
                newEggSackGenome.SetToMutatedCopyOfParentGenome(eggSackGenomePoolArray[eggSackIndex], settingsManager.mutationSettingsPersistent);
                eggSackGenomePoolArray[eggSackIndex] = newEggSackGenome;

                //Debug.Log("BeginPregnancy! Egg[" + eggSackIndex.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                if(agentsArray[randParentAgentIndex].childEggSackRef != null && agentsArray[randParentAgentIndex].isPregnantAndCarryingEggs) {
                    Debug.Log("DOUBLE PREGNANT!! egg[" + agentsArray[randParentAgentIndex].childEggSackRef.index.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                }

                eggSackArray[eggSackIndex].InitializeEggSackFromGenome(eggSackIndex, agentsArray[randParentAgentIndex].candidateRef.candidateGenome, agentsArray[randParentAgentIndex], GetRandomFoodSpawnPosition().startPosition);
            
                agentsArray[randParentAgentIndex].BeginPregnancy(eggSackArray[eggSackIndex]);

                eggSackRespawnCounter = 0;
            }
            else {
                // Wait? SpawnImmaculate?
                //if(curApproxGen < 2) {
                
                int respawnCooldown = 37;
                
                if(eggSackRespawnCounter > respawnCooldown) {  // try to encourage more pregnancies?
                   
                    List<int> eligibleAgentIndicesList = new List<int>();
                    for(int a = 0; a < numAgents; a++) {
                        if(agentsArray[a].isInert) {

                        }
                        else {
                            eligibleAgentIndicesList.Add(a);
                        }
                    }
                    if(eligibleAgentIndicesList.Count > 0) {
                        int randListIndex = UnityEngine.Random.Range(0, eligibleAgentIndicesList.Count);
                        int agentIndex = eligibleAgentIndicesList[randListIndex];
                    
                        eggSackArray[eggSackIndex].parentAgentIndex = agentIndex;
                        eggSackArray[eggSackIndex].InitializeEggSackFromGenome(eggSackIndex, agentsArray[agentIndex].candidateRef.candidateGenome, null, GetRandomFoodSpawnPosition().startPosition);

                        eggSackRespawnCounter = 0;       
                    }         
                }             
            }
        }
        else {

        }
        
    }
    private void CheckForRecordAgentScore(int agentIndex) {
        if (agentsArray[agentIndex].scoreCounter > recordBotAge && agentIndex != 0) {
            recordBotAge = agentsArray[agentIndex].scoreCounter;
        }
    }
    private void ProcessAgentScores(Agent agentRef) {

        // REFACTOR!! This will need to be per-species, updated when leaderboard candidate list is updated
        
        numAgentsProcessed++;
        //get species index:
        int speciesIndex = agentRef.speciesIndex; // Mathf.FloorToInt((float)agentIndex / (float)_NumAgents * (float)numSpecies);

        float weightedAvgLerpVal = 1f / 128f;
        weightedAvgLerpVal = Mathf.Max(weightedAvgLerpVal, 1f / (float)(numAgentsProcessed + 1));

        //speciesAvgFoodEaten[speciesIndex] = Mathf.Lerp(speciesAvgFoodEaten[speciesIndex], agentsArray[agentIndex].totalFoodEaten, weightedAvgLerpVal);
        //speciesAvgSizes[speciesIndex] = Vector2.Lerp(speciesAvgSizes[speciesIndex], new Vector2(agentsArray[agentIndex].coreModule.coreWidth, agentsArray[agentIndex].coreModule.coreLength), weightedAvgLerpVal);
        float mouthType = 0f;
        //if(agentsArray[agentIndex].mouthRef.isPassive == false) {
        //    mouthType = 1f;
        //}
        //speciesAvgMouthTypes[speciesIndex] = Mathf.Lerp(speciesAvgMouthTypes[speciesIndex], mouthType, weightedAvgLerpVal);
        //rollingAverageAgentScoresArray[speciesIndex] = Mathf.Lerp(rollingAverageAgentScoresArray[speciesIndex], (float)agentsArray[agentIndex].scoreCounter, weightedAvgLerpVal);
        
        float approxGen = (float)numAgentsBorn / (float)(numAgents - 1);
        if (approxGen > curApproxGen) {
            statsNutrientsEachGenerationList.Add(new Vector4(foodManager.curGlobalNutrients, foodManager.curGlobalFoodParticles, 0f, 0f));
            //RefreshGraphTextureNutrients();
            
            curApproxGen++;
            
            
            
            /*
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
            statsNutrientsEachGenerationList.Add(new Vector4(foodManager.curGlobalNutrients, foodManager.curGlobalFoodParticles, 0f, 0f));

            RefreshGraphTextureLifespan();
            RefreshGraphTextureFoodEaten();
            RefreshGraphTexturePredation();
            RefreshGraphTextureBodySizes();
            RefreshGraphTextureNutrients();
            RefreshGraphTextureMutation();
            
            */

            UpdateSimulationClimate();
        }
        else {
            if(numAgentsProcessed < 130) {
                if(numAgentsProcessed % 8 == 0) {
                    statsNutrientsEachGenerationList[curApproxGen - 1] = new Vector4(foodManager.curGlobalNutrients, foodManager.curGlobalFoodParticles, 0f, 0f);
                    //RefreshGraphTextureNutrients();
                    //Debug.Log("process Stats!! + " + curApproxGen.ToString() + ", numProcessed: " + numAgentsProcessed.ToString());

                    /*
                    Vector4 scores = new Vector4(rollingAverageAgentScoresArray[0], rollingAverageAgentScoresArray[1], rollingAverageAgentScoresArray[2], rollingAverageAgentScoresArray[3]); ;
                    statsLifespanEachGenerationList[curApproxGen - 1] = scores;
                    
                    Vector4 foodEaten = new Vector4(speciesAvgFoodEaten[0], speciesAvgFoodEaten[1], speciesAvgFoodEaten[2], speciesAvgFoodEaten[3]); 
                    statsFoodEatenEachGenerationList[curApproxGen - 1] = foodEaten;

                    Vector4 predation = new Vector4(speciesAvgMouthTypes[0], speciesAvgMouthTypes[1], speciesAvgMouthTypes[2], speciesAvgMouthTypes[3]); ;
                    statsPredationEachGenerationList[curApproxGen - 1] = predation;

                    //bodySizes:
                    Vector4 bodySizes = new Vector4(speciesAvgSizes[0].x * speciesAvgSizes[0].y, speciesAvgSizes[1].x * speciesAvgSizes[1].y, speciesAvgSizes[2].x * speciesAvgSizes[2].y, speciesAvgSizes[3].x * speciesAvgSizes[3].y);
                    statsBodySizesEachGenerationList[curApproxGen - 1] = bodySizes;

                    statsMutationEachGenerationList[curApproxGen - 1] = curPlayerMutationRate;
                    statsNutrientsEachGenerationList[curApproxGen - 1] = new Vector4(foodManager.curGlobalNutrients, foodManager.curGlobalFoodParticles, 0f, 0f);

                    RefreshGraphTextureLifespan();
                    RefreshGraphTextureFoodEaten();
                    RefreshGraphTexturePredation();
                    RefreshGraphTextureBodySizes();
                    */
                }
            }
        }
    }

    private void UpdateSimulationClimate() {
        // Change force of turbulence, damping, other fluidSim parameters,
        // Inject pre-trained critters
        environmentFluidManager.UpdateSimulationClimate((float)curApproxGen);
    }
    private void RefreshGraphData() {
        uiManager.UpdateGraphDataTextures(curSimYear);
        uiManager.UpdateStatsTextureNutrients(statsNutrientsEachGenerationList);
    }
    /*private void RefreshGraphTextureLifespan() {
        Debug.Log("Happy New Years! (refreshing graph) " + curSimYear.ToString());
        uiManager.UpdateStatsTextureLifespan(curSimYear);
    }
    private void RefreshGraphTextureBodySizes() {
        //uiManager.UpdateStatsTextureBodySizes(statsBodySizesEachGenerationList);
    }
    private void RefreshGraphTextureFoodEaten() {
        //uiManager.UpdateStatsTextureFoodEaten(statsFoodEatenEachGenerationList);
    }
    private void RefreshGraphTexturePredation() {
        //uiManager.UpdateStatsTexturePredation(statsPredationEachGenerationList);
    }
    private void RefreshGraphTextureNutrients() {
        uiManager.UpdateStatsTextureNutrients(statsNutrientsEachGenerationList);
    }
    private void RefreshGraphTextureMutation() {
        //uiManager.UpdateStatsTextureMutation(statsMutationEachGenerationList);
    }
    */
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

    public void AddNewSpecies(AgentGenome newGenome, int parentSpeciesID) {
        int newSpeciesID = masterGenomePool.completeSpeciesPoolsList.Count;
        
        SpeciesGenomePool newSpecies = new SpeciesGenomePool(newSpeciesID, parentSpeciesID, curSimYear, settingsManager.mutationSettingsPersistent);
        newSpecies.FirstTimeInitialize(newGenome, masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].depthLevel + 1);
        masterGenomePool.currentlyActiveSpeciesIDList.Add(newSpeciesID);
        masterGenomePool.completeSpeciesPoolsList.Add(newSpecies);
        masterGenomePool.speciesCreatedOrDestroyedThisFrame = true;

        // Tree Of LIFE UI collider & RenderKing updates:
        uiManager.treeOfLifeManager.AddNewSpecies(masterGenomePool, newSpeciesID);
        theRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, newSpeciesID);
        // WRAP THIS IN A FUNCTION!!! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

        if(newSpecies.depthLevel > masterGenomePool.currentHighestDepth) {
            masterGenomePool.currentHighestDepth = newSpecies.depthLevel;
        }
        //Debug.Log("New Species Created!!! (" + newSpeciesID.ToString() + "] score: " + closestDistance.ToString());
    }
    
    /*private void SetAgentGenomeToMutatedCopyOfParentGenome(int agentIndex, AgentGenome parentGenome) {

        BodyGenome newBodyGenome = new BodyGenome();
        BrainGenome newBrainGenome = new BrainGenome();

        BodyGenome parentBodyGenome = parentGenome.bodyGenome;
        BrainGenome parentBrainGenome = parentGenome.brainGenome;

        newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, settingsManager.mutationSettingsPersistent);
        newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, settingsManager.mutationSettingsPersistent);
        
        agentGenomePoolArray[agentIndex].bodyGenome = newBodyGenome; 
        agentGenomePoolArray[agentIndex].brainGenome = newBrainGenome;         
    }*/
    
    private StartPositionGenome GetRandomAgentSpawnPosition(int speciesIndex) {
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;

        int randZone = UnityEngine.Random.Range(0, numSpawnZones);

        int numPotentialParents = numEggSacks / numSpecies;
        int lowIndex = numPotentialParents * speciesIndex;
        int highIndex = (numPotentialParents + 1) * speciesIndex;

        int randParentIndex = UnityEngine.Random.Range(lowIndex, highIndex);


        float randRadius = startPositionsPresets.spawnZonesList[randZone].radius;

        Vector2 randOffset = UnityEngine.Random.insideUnitCircle * randRadius;

        if(randParentIndex >= eggSackArray.Length)
        {
            Debug.Log("ERROR!: " + randParentIndex.ToString() + ", " + numEggSacks.ToString());
        }
        Vector3 parentEggPos = eggSackArray[randParentIndex].transform.position;

        Vector3 agentStartPos = new Vector3(parentEggPos.x + randOffset.x,
                               parentEggPos.y + randOffset.y,
                               0f);

        Vector3 randStartPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);

        Vector3 startPos = Vector3.Lerp(agentStartPos, randStartPos, Mathf.Round(UnityEngine.Random.Range(0f, 1f)));
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        return startPosGenome;
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
                if(startPositionsPresets.spawnZonesList[randZone].refactoryCounter > 100) {
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
            /*if (simStateData.agentSimDataCBuffer != null) {
                simStateData.agentSimDataCBuffer.Release();
            }*/
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
            if (simStateData.eggSackSimDataCBuffer != null) {
                simStateData.eggSackSimDataCBuffer.Release();
            }
            /*if (simStateData.predatorSimDataCBuffer != null) {
                simStateData.predatorSimDataCBuffer.Release();
            }*/
            if (simStateData.foodStemDataCBuffer != null) {
                simStateData.foodStemDataCBuffer.Release();
            }
            if (simStateData.foodLeafDataCBuffer != null) {
                simStateData.foodLeafDataCBuffer.Release();
            }
            if (simStateData.eggDataCBuffer != null) {
                simStateData.eggDataCBuffer.Release();
            }
        }

        foodManager.ClearBuffers();
        
    }

    public void SaveTrainingData() {  // ********* BROKEN BY SPECIATION UPDATE!!!!!!!!!!!!! ****************
        /*
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
        */
    }
    public void LoadTrainingData() {  // ********* BROKEN BY SPECIATION UPDATE!!!!!!!!!!!!! ****************
        /*
        //Debug.Log("LOAD Population!");
        //"E:\Unity Projects\GitHub\Evolution\Assets\GridSearchSaves\2018_2_13_12_35\GS_RawScores.json"
        string filePath = Path.Combine(Application.streamingAssetsPath, "testSave.json");
        //string filePath = Application.dataPath + "/Resources/SavedGenePools/testSave.json";
        // Read the json from the file into a string
        string dataAsJson = File.ReadAllText(filePath);
        // Pass the json to JsonUtility, and tell it to create a GameData object from it
        GenePool loadedData = JsonUtility.FromJson<GenePool>(dataAsJson);

        // Try to respect current size:
        int numAgents = agentGenomePoolArray.Length;
        int smallerArrayLength = Mathf.Min(numAgents, loadedData.genomeArray.Length);
        Debug.Log("LOAD Population! numAgents: " + numAgents.ToString());
        for(int i = 0; i < numAgents; i++) {

            int loadedIndex = i % smallerArrayLength;

            agentGenomePoolArray[i] = loadedData.genomeArray[loadedIndex];
        }
        //agentGenomePoolArray = loadedData.genomeArray;
        */
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
