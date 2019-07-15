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
    public ComputeShader computeShaderResourceGrid; // algae grid
    public ComputeShader computeShaderFoodParticles;  // algae particles
    public ComputeShader computeShaderAnimalParticles;  // animal particles
    public MasterGenomePool masterGenomePool;  // agents

    public TrophicLayersManager trophicLayersManager;
    public VegetationManager vegetationManager;
    public ZooplanktonManager zooplanktonManager;
    //public AgentsManager agentsManager;

    public SimEventsManager simEventsManager;
    public SimResourceManager simResourceManager;

    public float fogAmount = 0.25f;
    public Vector4 fogColor; 

    public bool isQuickStart = true;

    //public float curPlayerMutationRate = 0.75f;  // UI-based value, giving player control over mutation frequency with one parameter

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
    private int numWarmUpTimeSteps = 30;
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
    
    public EggSackGenome[] eggSackGenomePoolArray;
    
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
    
    public int numAgentsBorn = 0;
    public int numAgentsDied = 0;
    
        // 0 == decay nutrients  .x
        // 1 == plant food  .y
        // 2 == eggs food  .z
        // 3 = corpse food  .w
        // 4 = brain mutation freq
        // 5 = brain mutation amp
        // 6 = brain size bias
        // 7 = body proportion freq
        // 8 = body proportion amp
        // 9 = body sensor mutation rate
        // 10 = water current / storminess
    public List<Vector4> statsNutrientsEachGenerationList;
    public List<float> statsHistoryBrainMutationFreqList;
    public List<float> statsHistoryBrainMutationAmpList;
    public List<float> statsHistoryBrainSizeBiasList;
    public List<float> statsHistoryBodyMutationFreqList;
    public List<float> statsHistoryBodyMutationAmpList;
    public List<float> statsHistoryBodySensorVarianceList;
    public List<float> statsHistoryWaterCurrentsList;

    public List<float> statsHistoryOxygenList;
    public List<float> statsHistoryNutrientsList;
    public List<float> statsHistoryDetritusList;
    public List<float> statsHistoryDecomposersList;
    public List<float> statsHistoryAlgaeSingleList;
    public List<float> statsHistoryAlgaeParticleList;
    public List<float> statsHistoryZooplanktonList;
    public List<float> statsHistoryLivingAgentsList;
    public List<float> statsHistoryDeadAgentsList;
    public List<float> statsHistoryEggSacksList;

    public int curApproxGen = 1;

    public int numInitialHiddenNeurons = 16;
            
    private int numAgentsProcessed = 0;

    private int eggSackRespawnCounter;    
    private int agentRespawnCounter = 0;

    private int numAgentEvaluationsPerGenome = 1;

    public int simAgeTimeSteps = 0;
    private int numStepsInSimYear = 1500;
    private int simAgeYearCounter = 0;
    public int curSimYear = 0;

    public bool recentlyAddedSpeciesOn = false;// = true;
    private Vector2 recentlyAddedSpeciesWorldPos; // = new Vector2(spawnPos.x, spawnPos.y);
    private int recentlyAddedSpeciesID; // = masterGenomePool.completeSpeciesPoolsList.Count - 1;
    private int recentlyAddedSpeciesTimeCounter = 0;

    public static float energyDifficultyMultiplier = 1f;

    //public bool isBrushingAgents = false;
    

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
                Debug.Log("StartCoroutine(LoadingNewSimulation()); " + (Time.realtimeSinceStartup).ToString());
                StartCoroutine(LoadingNewSimulation());

                // Turn on Gameplay Audio:
                audioManager.TurnOnGameplayAudioGroup();
            }  
        }  
    }

    IEnumerator LoadingNewSimulation() {
        //Debug.Log("LoadingNewSimulation() ");
        //const float maxComputeTimePerFrame = 0.01f; // 10 milliseconds per frame
        float startTime = Time.realtimeSinceStartup;
        float masterStartTime = Time.realtimeSinceStartup;
        //Debug.Log("Start: " + (Time.realtimeSinceStartup - startTime).ToString());
        //float elapsedTime;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeCoreSimulationState()";
        // Do some stuff:: LOAD!
        LoadingInitializeCoreSimulationState();  // creates arrays and stuff for the (hopefully)only time
        Debug.Log("LoadingInitializeCoreSimulationState: " + (Time.realtimeSinceStartup - startTime).ToString());
        // Fitness Stuffs:::
        startTime = Time.realtimeSinceStartup;
        LoadingSetUpFitnessStorage();
        Debug.Log("LoadingSetUpFitnessStorage: " + (Time.realtimeSinceStartup - startTime).ToString());
        yield return null;
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInstantiateEggSacks()";
        // create first EggSacks:
        LoadingInstantiateEggSacks();
        Debug.Log("LoadingInstantiateEggSacks: " + (Time.realtimeSinceStartup - startTime).ToString());
        yield return null;
        // ******  Combine this with ^ ^ ^ function ??? **************
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeEggSacksFirstTime()";
        LoadingInitializeEggSacksFirstTime();
        Debug.Log("LoadingInitializeEggSacksFirstTime: " + (Time.realtimeSinceStartup - startTime).ToString());
        yield return null;
        startTime = Time.realtimeSinceStartup;
        // Can I create Egg Sacks and then immediately sapwn agents on them
        // Initialize Agents:
        //uiManager.textLoadingTooltips.text = "LoadingInstantiateAgents()";
        //LoadingInstantiateAgents();  // Fills the AgentsArray, Instantiates Agent Objects (MonoBehaviors + GameObjects)
        // TEMP BREAKOUT!!!
        // Instantiate AI Agents
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = new GameObject("Agent" + i.ToString());
            Agent newAgent = agentGO.AddComponent<Agent>();
            //newAgent.speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
            newAgent.FirstTimeInitialize(settingsManager); // agentGenomePoolArray[i]);
            agentsArray[i] = newAgent; // Add to stored list of current Agents 
            yield return null;
        }
        // Do *** !!!! *** v v v *** This is being replaced by a different mechanism for spawning Agents:
        //LoadingInitializeAgentsFromGenomes(); // This was "RespawnAgents()" --  Used to actually place the Agent in the game in a random spot and set the Agent's atributes ffrom its genome
        Debug.Log("LoadingInstantiateAgents: " + (Time.realtimeSinceStartup - startTime).ToString());
        Debug.Log("End Total up to LoadingInstantiateAgents: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
        yield return null;
        
        // Load pre-saved genomes:
        //LoadingLoadGenepoolFiles();       
        //yield return null;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeFluidSim()";
        // **** How to handle sharing simulation data between different Managers???
        // Once Agents, Food, etc. are established, Initialize the Fluid:
        LoadingInitializeFluidSim();
        Debug.Log("End Total up to LoadingInitializeFluidSim: " + (Time.realtimeSinceStartup - masterStartTime).ToString());

        yield return null;
        startTime = Time.realtimeSinceStartup;
        // Initialize Food:
        //uiManager.textLoadingTooltips.text = "LoadingInitializeFoodParticles()";
        LoadingInitializeFoodParticles();
        Debug.Log("LoadingInitializeFoodParticles: " + (Time.realtimeSinceStartup - startTime).ToString());
        Debug.Log("End Total up to LoadingInitializeFoodParticles: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
        yield return null;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeAnimalParticles()";
        LoadingInitializeAnimalParticles();

        yield return null;

        //uiManager.textLoadingTooltips.text = "GentlyRouseTheRenderMonarchHisHighnessLordOfPixels()";   
        // Wake up the Render King and prepare him for the day ahead, proudly ruling over Renderland.
        GentlyRouseTheRenderMonarchHisHighnessLordOfPixels();

        // TreeOfLife Render buffers here ???              ///////// **********************   THIS NEEDS REFACTOR!!!!! **********
        // Tree Of LIFE UI collider & RenderKing updates:
        //wtheRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, 0);
        //theRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, 1);
        masterGenomePool.completeSpeciesPoolsList[0].isFlaggedForExtinction = true;
        masterGenomePool.ExtinctifySpecies(this, masterGenomePool.currentlyActiveSpeciesIDList[0]);
                
        Debug.Log("End Total up to GentlyRouseTheRenderMonarchHisHighnessLordOfPixels: " + (Time.realtimeSinceStartup - masterStartTime).ToString());

        yield return null;
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeFoodGrid()";
        LoadingInitializeResourceGrid();
        Debug.Log("LoadingInitializeFoodGrid: " + (Time.realtimeSinceStartup - startTime).ToString());
        Debug.Log("End Total up to LoadingInitializeFoodGrid: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
        yield return null;

        //uiManager.textLoadingTooltips.text = "LoadingHookUpFluidAndRenderKing()";        
        LoadingHookUpFluidAndRenderKing();  // fluid needs refs to RK's obstacle/color cameras' RenderTextures!
        
        Debug.Log("End Total up to LoadingHookUpFluidAndRenderKing: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
        yield return null;
        

        //uiManager.textLoadingTooltips.text = "LoadingInitializeGridCells()";
        LoadingInitializeGridCells();
        // Populates GridCells with their contents (agents/food/preds)
        LoadingFillGridCells();
        LoadingHookUpModules();
        
        //yield return new WaitForSeconds(5f); // TEMP!!!
        Debug.Log("End Total: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
        environmentFluidManager.UpdateSimulationClimate();
        
        // Done - will be detected by GameManager next frame
        loadingComplete = true;

        Debug.Log("LOADING COMPLETE - Starting WarmUp!");
    }

    private void LoadingInitializeCoreSimulationState() {
        // allocate memory and initialize data structures, classes, arrays, etc.

        settingsManager.Initialize();
        trophicLayersManager = new TrophicLayersManager();
        simEventsManager = new SimEventsManager(this);
        simResourceManager = new SimResourceManager();
        //agentsManager = new AgentsManager();
        vegetationManager = new VegetationManager(settingsManager, simResourceManager);
        zooplanktonManager = new ZooplanktonManager(settingsManager, simResourceManager);
         
        LoadingInitializePopulationGenomes();  // **** Maybe change this up ? ** depending on time of first Agent Creation?

        if(isQuickStart) {
            Debug.Log("QUICK START! Loading Pre-Trained Genomes!");
            LoadTrainingData();
        }

        simStateData = new SimulationStateData(this);
        
    }
    private void LoadingInitializePopulationGenomes() {
        masterGenomePool = new MasterGenomePool();
        masterGenomePool.FirstTimeInitialize(numAgents, settingsManager.mutationSettingsAgents, uiManager);
        

        // EGGSACKS:
        eggSackGenomePoolArray = new EggSackGenome[numEggSacks];

        for(int i = 0; i < eggSackGenomePoolArray.Length; i++) {
            EggSackGenome eggSackGenome = new EggSackGenome(i);

            eggSackGenome.InitializeAsRandomGenome();

            eggSackGenomePoolArray[i] = eggSackGenome;
        }

        //yield return null;
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
            newAgent.FirstTimeInitialize(settingsManager); // agentGenomePoolArray[i]);
            agentsArray[i] = newAgent; // Add to stored list of current Agents            
        }
    }
    
    private void LoadingInitializeFoodParticles() {
        vegetationManager.InitializeAlgaeParticles(numAgents, computeShaderFoodParticles);
    }    
    private void LoadingInitializeResourceGrid() {        
        vegetationManager.InitializeAlgaeGrid(numAgents, computeShaderResourceGrid); 
        vegetationManager.InitializeReactionDiffusionGrid();
    }
    private void LoadingInitializeAnimalParticles() {
        zooplanktonManager.InitializeAnimalParticles(numAgents, computeShaderAnimalParticles);
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
            newEggSack.FirstTimeInitialize(settingsManager);
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
        //treeOfLifeSpeciesTreeRenderCamera targetTexture set in scene
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
        //rawFitnessScoresArray = new float[numAgents];
        statsNutrientsEachGenerationList = new List<Vector4>();
        statsNutrientsEachGenerationList.Add(Vector4.one * 0.0001f);

        statsHistoryBrainMutationFreqList = new List<float>();
        statsHistoryBrainMutationFreqList.Add(0f);
        statsHistoryBrainMutationAmpList = new List<float>();
        statsHistoryBrainMutationAmpList.Add(0f);
        statsHistoryBrainSizeBiasList = new List<float>();
        statsHistoryBrainSizeBiasList.Add(0f);
        statsHistoryBodyMutationFreqList = new List<float>();
        statsHistoryBodyMutationFreqList.Add(0f);
        statsHistoryBodyMutationAmpList = new List<float>();
        statsHistoryBodyMutationAmpList.Add(0f);
        statsHistoryBodySensorVarianceList = new List<float>();
        statsHistoryBodySensorVarianceList.Add(0f);
        statsHistoryWaterCurrentsList = new List<float>();
        statsHistoryWaterCurrentsList.Add(0f);

        statsHistoryOxygenList = new List<float>();
        statsHistoryOxygenList.Add(0f);
        statsHistoryNutrientsList = new List<float>();
        statsHistoryNutrientsList.Add(0f);
        statsHistoryDetritusList = new List<float>();
        statsHistoryDetritusList.Add(0f);
        statsHistoryDecomposersList = new List<float>();
        statsHistoryDecomposersList.Add(0f);
        statsHistoryAlgaeSingleList = new List<float>();
        statsHistoryAlgaeSingleList.Add(0f);
        statsHistoryAlgaeParticleList = new List<float>();
        statsHistoryAlgaeParticleList.Add(0f);
        statsHistoryZooplanktonList = new List<float>();
        statsHistoryZooplanktonList.Add(0f);
        statsHistoryLivingAgentsList = new List<float>();
        statsHistoryLivingAgentsList.Add(0f);
        statsHistoryDeadAgentsList = new List<float>();
        statsHistoryDeadAgentsList.Add(0f);
        statsHistoryEggSacksList = new List<float>();
        statsHistoryEggSacksList.Add(0f);
        
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
        TickSparseEvents();  // Updates that don't happen every frame        
        simEventsManager.Tick();  // Cooldown increment                
        eggSackRespawnCounter++;
        agentRespawnCounter++;
        masterGenomePool.Tick(); // keep track of when species created so can't create multiple per frame?

        trophicLayersManager.Tick(this);

        /*private bool recentlyAddedSpeciesOn = false;// = true;
        private Vector2 recentlyAddedSpeciesWorldPos; // = new Vector2(spawnPos.x, spawnPos.y);
        private int recentlyAddedSpeciesID; // = masterGenomePool.completeSpeciesPoolsList.Count - 1;
        private int recentlyAddedSpeciesTimeCounter = 0;*/

        if(recentlyAddedSpeciesOn) {
            recentlyAddedSpeciesTimeCounter++;

            if(recentlyAddedSpeciesTimeCounter > 300) {
                recentlyAddedSpeciesOn = false;
                recentlyAddedSpeciesTimeCounter = 0;

                Debug.Log("recentlyAddedSpeciesOn  TIMED OUT!!!");
            }
        }

        // Go through each Trophic Layer:
            // Measure resources used/produced
            // send current data to GPU

        // simulation ticks per layer


        // MEASURE GLOBAL RESOURCES:
        if(trophicLayersManager.GetAlgaeOnOff()) {
            vegetationManager.FindClosestAlgaeParticleToCritters(simStateData);
            vegetationManager.MeasureTotalAlgaeParticlesAmount();
        }
        if(trophicLayersManager.GetZooplanktonOnOff()) {
            zooplanktonManager.FindClosestAnimalParticleToCritters(simStateData);
            zooplanktonManager.MeasureTotalAnimalParticlesAmount();
        }
        
        
        // Actually measuring results of last frame's execution?
        float totalOxygenUsedByAgents = 0f;
        float totalWasteProducedByAgents = 0f;
        if(trophicLayersManager.GetAgentsOnOff()) {
            for (int i = 0; i < agentsArray.Length; i++) {
                totalOxygenUsedByAgents += agentsArray[i].oxygenUsedLastFrame;
                totalWasteProducedByAgents += agentsArray[i].wasteProducedLastFrame;          
            }
            for (int i = 0; i < eggSackArray.Length; i++) {
            
            }
        }        
        simResourceManager.oxygenUsedByAgentsLastFrame = totalOxygenUsedByAgents;
        simResourceManager.wasteProducedByAgentsLastFrame = totalWasteProducedByAgents;

        // Global Resources Here????
        // Try to make sure AlgaeReservoir and AlgaeParticles share same mechanics!!! *********************************************
        simResourceManager.Tick(settingsManager, trophicLayersManager);  // Resource Flows Here
        
                
        // CHECK FOR NULL Objects:        
        // ******** REVISIT CODE ORDERING!!!!  -- Should check for death Before or After agent Tick/PhysX ???
        if(trophicLayersManager.GetAgentsOnOff()) {
            CheckForDevouredEggSacks();
            CheckForNullAgents();  // Result of this will affect: "simStateData.PopulateSimDataArrays(this)" !!!!!
            if(simResourceManager.curGlobalOxygen > 10f) {
                CheckForReadyToSpawnAgents();
            }            
        }

        fogColor = Color.Lerp(new Color(0.15f, 0.25f, 0.52f), new Color(0.07f, 0.27f, 0.157f), Mathf.Clamp01(simResourceManager.curGlobalAlgaeParticles * 0.035f));
        fogAmount = Mathf.Lerp(0.3f, 0.55f, Mathf.Clamp01(simResourceManager.curGlobalAlgaeParticles * 0.0036f));

        simStateData.PopulateSimDataArrays(this);  // reads from GameObject Transforms & RigidBodies!!! ++ from FluidSimulationData!!!
        theRenderKing.RenderSimulationCameras(); // will pass current info to FluidSim before it Ticks()
        // Reads from CameraRenders, GameObjects, and query GPU for fluidState
        // eventually, if agents get fluid sensors, will want to download FluidSim data from GPU into simStateData!*
        // **** Figure out proper Execution Order / in which Class to Run RenderOps while being synced w/ results of physX sim!!!        
        theRenderKing.Tick(); // updates all renderData, buffers, brushStrokes etc.
        // Simulate timestep of fluid Sim - update density/velocity maps:
        // Or should this be right at beginning of frame????? ***************** revisit...
        environmentFluidManager.Tick(vegetationManager.rdRT1); // ** Clean this up, but generally OK

        //vegetationManager.ApplyDiffusionOnResourceGrid(environmentFluidManager);
        //vegetationManager.AdvectResourceGrid(environmentFluidManager);
        //if(curSimYear < 4) {  // stop simulating after certain point !! TEMPORARY!!!
        vegetationManager.SimReactionDiffusionGrid(ref environmentFluidManager, ref theRenderKing.baronVonTerrain, ref theRenderKing);
        //}
        

        if(trophicLayersManager.GetAlgaeOnOff()) {
            vegetationManager.EatSelectedFoodParticles(simStateData); // 
            // How much light/nutrients available?
            vegetationManager.SimulateAlgaeParticles(environmentFluidManager, theRenderKing, simStateData, simResourceManager);
        }
        if(trophicLayersManager.GetZooplanktonOnOff()) {
            zooplanktonManager.EatSelectedAnimalParticles(simStateData);        
            // Send back information about how much growth/photosynthesis there was?
            zooplanktonManager.SimulateAnimalParticles(environmentFluidManager, theRenderKing, simStateData, simResourceManager);
            // how much oxygen used? How much eaten? How much growth? How much waste/detritus?
        }
              
        
        if(trophicLayersManager.GetAgentsOnOff()) {
            HookUpModules(); // Sets nearest-neighbors etc. feed current data into agent Brains
            // Load gameState into Agent Brain, process brain function, read out brainResults,
            // Execute Agent Actions -- apply propulsive force to each Agent:        
            for (int i = 0; i < agentsArray.Length; i++) {
                agentsArray[i].Tick(this, settingsManager);            
            }
            for (int i = 0; i < eggSackArray.Length; i++) {
                eggSackArray[i].Tick();
            }                
            // Apply External Forces to dynamic objects: (internal PhysX Updates):        
            ApplyFluidForcesToDynamicObjects();
        }        

        // TEMP AUDIO EFFECTS!!!!        
        float volume = agentsArray[0].smoothedThrottle.magnitude * 0.24f;
        audioManager.SetPlayerSwimLoopVolume(volume);      
        

        // OLD ALGAE GRID:
        /*
        vegetationManager.RemoveEatenAlgaeGrid(numAgents, simStateData);
        float spawnNewFoodChance = settingsManager.spawnNewFoodChance;
        float spawnFoodPercentage = UnityEngine.Random.Range(0f, 0.25f);
        float maxGlobalFood = settingsManager.maxGlobalNutrients;

        if(totalNutrients < maxGlobalFood) {   // **** MOVE THIS INTO FOOD MANAGER?
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < spawnNewFoodChance) {
                // pick random cell:
                int nutrientPatchRandID = UnityEngine.Random.Range(0, vegetationManager.nutrientSpawnPatchesArray.Length);
                int indexX = Mathf.FloorToInt(vegetationManager.algaeGridTexResolution * vegetationManager.nutrientSpawnPatchesArray[nutrientPatchRandID].x);
                int indexY = Mathf.FloorToInt(vegetationManager.algaeGridTexResolution * vegetationManager.nutrientSpawnPatchesArray[nutrientPatchRandID].y);

                float foodAvailable = maxGlobalFood - totalNutrients;

                float newFoodAmount = Mathf.Min(1f, foodAvailable * spawnFoodPercentage);

                // ADD FOOD HERE::
                
                vegetationManager.AddAlgaeAtCoords(newFoodAmount, indexX, indexY);                
            }
        }
        vegetationManager.ApplyDiffusionOnAlgaeGrid(environmentFluidManager);
        */
        
    }
    private void TickSparseEvents() {
        if(simAgeYearCounter >= numStepsInSimYear) {
            curSimYear++;
            simEventsManager.curEventBucks += 5; // temporarily high!
            simAgeYearCounter = 0;

            energyDifficultyMultiplier = Mathf.Lerp(3f, 1f, (float)curSimYear / 100f);
            Debug.Log("energyDifficultyMultiplier: " + energyDifficultyMultiplier.ToString());
            
            AddNewHistoricalDataEntry();
            AddNewSpeciesDataEntry(curSimYear);
            uiManager.UpdateSpeciesTreeDataTextures(curSimYear);
                        
            if(curSimYear == 1) {
                SimEventData newEventData = new SimEventData();
                newEventData.name = "First Full Year";
                newEventData.category = SimEventData.SimEventCategories.NPE;
                newEventData.timeStepActivated = simAgeTimeSteps;
                simEventsManager.completeEventHistoryList.Add(newEventData);
            }
            if(curSimYear == 10) {
                SimEventData newEventData = new SimEventData();
                newEventData.name = "Decade";
                newEventData.category = SimEventData.SimEventCategories.NPE;
                newEventData.timeStepActivated = simAgeTimeSteps;
                simEventsManager.completeEventHistoryList.Add(newEventData);
            }
            if(curSimYear == 100) {
                SimEventData newEventData = new SimEventData();
                newEventData.name = "Century";
                newEventData.category = SimEventData.SimEventCategories.NPE;
                newEventData.timeStepActivated = simAgeTimeSteps;
                simEventsManager.completeEventHistoryList.Add(newEventData);
            }
            if(curSimYear == 1000) {
                SimEventData newEventData = new SimEventData();
                newEventData.name = "Millenium";
                newEventData.category = SimEventData.SimEventCategories.NPE;
                newEventData.timeStepActivated = simAgeTimeSteps;
                simEventsManager.completeEventHistoryList.Add(newEventData);
            }
        }

        if(simAgeTimeSteps % 79 == 3) {
            UpdateSimulationClimate();

            RefreshLatestHistoricalDataEntry();
            RefreshLatestSpeciesDataEntry();
            uiManager.UpdateSpeciesTreeDataTextures(curSimYear); // shouldn't lengthen!
            
            uiManager.UpdateTolWorldStatsTexture(statsNutrientsEachGenerationList);
            
            //theRenderKing.UpdateTreeOfLifeEventLineData(simEventsManager.completeEventHistoryList);
        }
    }
    private void ApplyFluidForcesToDynamicObjects() {
        // ********** REVISIT CONVERSION btw fluid/scene coords and Force Amounts !!!! *************
        for (int i = 0; i < agentsArray.Length; i++) {

            Vector3 depthSample = simStateData.depthAtAgentPositionsArray[i];
            float agentSize = agentsArray[i].fullSizeBoundingBox.z * 1.1f + 0.15f;
            float floorDepth = depthSample.x * 10f;
            if (floorDepth < agentSize)
            {
                float wallForce = Mathf.Clamp01(agentSize - floorDepth) / agentSize;
                agentsArray[i].bodyRigidbody.AddForce(new Vector2(depthSample.y, depthSample.z).normalized * 24f * agentsArray[i].bodyRigidbody.mass * wallForce, ForceMode2D.Impulse);
            }
            
            agentsArray[i].bodyRigidbody.AddForce(simStateData.fluidVelocitiesAtAgentPositionsArray[i] * 48f * agentsArray[i].bodyRigidbody.mass, ForceMode2D.Impulse);

            agentsArray[i].avgFluidVel = Vector2.Lerp(agentsArray[i].avgFluidVel, simStateData.fluidVelocitiesAtAgentPositionsArray[i], 0.25f);

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
        magnitude *= 0.05f;
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
    /*public void PlayerFeedToolSprinkle(Vector3 pos) {
        Debug.Log("PlayerFeedToolSprinkle pos: " + pos.ToString());
        int[] respawnIndices = new int[4];
        for(int i = 0; i < respawnIndices.Length; i++) {
            respawnIndices[i] = UnityEngine.Random.Range(0, 1024);
        }
        //vegetationManager.ReviveSelectFoodParticles(respawnIndices, 1.25f, new Vector4(pos.x / _MapSize, pos.y / _MapSize, 0f, 0f), simStateData);
    }
    public void PlayerFeedToolPour(Vector3 pos) {        
        int xCoord = Mathf.RoundToInt(pos.x / 256f * vegetationManager.resourceGridTexResolution);
        int yCoord = Mathf.RoundToInt(pos.y / 256f * vegetationManager.resourceGridTexResolution);

        Debug.Log("PlayerFeedToolPour pos: " + xCoord.ToString() + ", " + yCoord.ToString());

        //vegetationManager.AddAlgaeAtCoords(5f, xCoord, yCoord);

        int[] respawnIndices = new int[32];
        for(int i = 0; i < respawnIndices.Length; i++) {
            respawnIndices[i] = UnityEngine.Random.Range(0, 1024);
        }
        //vegetationManager.ReviveSelectFoodParticles(respawnIndices, 6f, new Vector4(pos.x / _MapSize, pos.y / _MapSize, 0f, 0f), simStateData);
    }*/
    //public void ChangeGlobalMutationRate(float normalizedVal) {
    //    settingsManager.SetGlobalMutationRate(normalizedVal);
    //}
    
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

        // **** Add priority targeting here? save closest of each time for later determination? ***

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

                int ownSpeciesIndex = agentsArray[a].speciesIndex; // Mathf.FloorToInt((float)a / (float)numAgents * (float)numSpecies);

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
                    int neighborSpeciesIndex = agentsArray[neighborIndex].speciesIndex; 

                    if(agentsArray[neighborIndex].curLifeStage != Agent.AgentLifeStage.Null && agentsArray[neighborIndex].curLifeStage != Agent.AgentLifeStage.AwaitingRespawn) {
                        // FRIEND:
                        Vector2 neighborPos = new Vector2(agentsArray[neighborIndex].bodyRigidbody.transform.localPosition.x, agentsArray[neighborIndex].bodyRigidbody.transform.localPosition.y);
                        float squaredDistNeighbor = (neighborPos - agentPos).sqrMagnitude;
                                
                        if (squaredDistNeighbor <= nearestFriendSquaredDistance) { // if now the closest so far, update index and dist:
                            //int neighborSpeciesIndex = agentsArray[neighborIndex].speciesIndex; // Mathf.FloorToInt((float)neighborIndex / (float)numAgents * (float)numSpecies);
                            if(ownSpeciesIndex == neighborSpeciesIndex) {  // if two agents are of same species - friends
                                if(a != neighborIndex) {  // make sure it doesn't consider itself:
                                    closestFriendIndex = neighborIndex;
                                    nearestFriendSquaredDistance = squaredDistNeighbor;
                                } 
                            }                                       
                        }

                        if (squaredDistNeighbor <= nearestEnemyAgentSqDistance) { // if now the closest so far, update index and dist:
                            // Mathf.FloorToInt((float)neighborIndex / (float)numAgents * (float)numSpecies);
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
                if(recentlyAddedSpeciesOn) {

                }
                else {
                    ProcessDeadEggSack(f);
                }                
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
    private void CheckForReadyToSpawnAgents() {    // AUTO-SPAWN     
        for (int a = 0; a < agentsArray.Length; a++) {
            if(agentRespawnCounter > 35) {
                if (agentsArray[a].curLifeStage == Agent.AgentLifeStage.AwaitingRespawn) {
                    //Debug.Log("AttemptToSpawnAgent(" + a.ToString() + ")");
                    int randomTableIndex = UnityEngine.Random.Range(0, masterGenomePool.currentlyActiveSpeciesIDList.Count);
                    int speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[randomTableIndex];
                    CandidateAgentData candidateData = masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNextAvailableCandidate();
                    AttemptToSpawnAgent(a, speciesIndex, candidateData);
                    agentRespawnCounter = 0;
                }
            }
        }                
    }
    public void AttemptToKillAgent(int speciesID, Vector2 clickPos, float brushRadius) {
        Debug.Log("AttemptToKillAgent loc: " + clickPos.ToString() + " ,,, species: " + speciesID.ToString() + ", brushRadius: " + brushRadius.ToString());  

        for (int a = 0; a < agentsArray.Length; a++) {            
            if (agentsArray[a].curLifeStage == Agent.AgentLifeStage.Mature) {
                if(speciesID == agentsArray[a].speciesIndex) {
                    float distToBrushCenter = (clickPos - agentsArray[a].ownPos).magnitude;

                    if(distToBrushCenter < brushRadius) {
                    
                        Debug.Log("KILL AGENT " + a.ToString() + " ,,, species: " + speciesID.ToString() + ", distToBrushCenter: " + distToBrushCenter.ToString()); 
                    
                        agentsArray[a].isMarkedForDeathByUser = true;
                    }
                }
            }
        }
    }
    public void AttemptToBrushSpawnAgent(int speciesIndex) {
        for (int a = 0; a < agentsArray.Length; a++) {            
            if (agentsArray[a].curLifeStage == Agent.AgentLifeStage.AwaitingRespawn) {
                          
                CandidateAgentData candidateData = masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNextAvailableCandidate();
                if(candidateData == null) {
                    Debug.LogError("GetNextAvailableCandidate(): candidateData NULL!!!!");
                }
                else {
                    Debug.Log("AttemptToBrushSpawnAgent(" + a.ToString() + ") species: " + speciesIndex.ToString() + ", " + candidateData.ToString());      
                    AttemptToSpawnAgent(a, speciesIndex, candidateData);
                    //agentRespawnCounter = 0;
                    break;
                }
                
            }
            
        } 
    }
    private void AttemptToSpawnAgent(int agentIndex, int speciesIndex, CandidateAgentData candidateData) { //, int speciesIndex) {

        // Which Species will the new agent belong to?
        // Random selection? Lottery-Selection among Species? Use this Agent's previous-life's Species?  Global Ranked Selection (across all species w/ modifiers) ?

        // Using Random Selection as first naive implementation:
        //int randomTableIndex = UnityEngine.Random.Range(0, masterGenomePool.currentlyActiveSpeciesIDList.Count);
        //int speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[randomTableIndex];
        
        // Find next-in-line genome waiting to be evaluated:
        //CandidateAgentData candidateData = masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNextAvailableCandidate();

        

        if(candidateData == null) {
            // all candidates are currently being tested, or candidate list is empty?
            //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString() + ") candidateData == null\n +" +  "   ");
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
                Debug.Log("SpawnAgentFromEggSack:");
                SpawnAgentFromEggSack(candidateData, agentIndex, speciesIndex, parentEggSack);
                candidateData.isBeingEvaluated = true;
            }
            else { // No eggSack found:
                //if(agentIndex == 0) {  // temp hack to avoid null reference exceptions:
                SpawnAgentImmaculate(candidateData, agentIndex, speciesIndex);
                candidateData.isBeingEvaluated = true;
                //}    
                //Debug.Log("AttemptToSpawnAgent Immaculate (" + agentIndex.ToString() + ") speciesIndex: " + speciesIndex.ToString() + " candidates: " + masterGenomePool.completeSpeciesPoolsList[speciesIndex].candidateGenomesList.Count.ToString());
            }
        }       
    }
       
    #endregion

    #region Process Events // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& PROCESS EVENTS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    
    public void RemoveSelectedAgentSpecies(int slotIndex) {
        Debug.Log("pressedRemoveSpecies! " + trophicLayersManager.selectedTrophicSlotRef.slotID.ToString());

        // need to connect UI slotID to speciesID
        if(masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
            masterGenomePool.ExtinctifySpecies(this, masterGenomePool.currentlyActiveSpeciesIDList[0]);
        }
        
    }
    public void CreateAgentSpecies(Vector3 spawnPos) {
        //eggSackArray[0].parentAgentIndex = 0;
        //eggSackArray[0].InitializeEggSackFromGenome(0, masterGenomePool.completeSpeciesPoolsList[0].representativeGenome, null, spawnPos);
        //eggSackArray[0].currentBiomass = settingsManager.agentSettings._BaseInitMass; // *** TEMP!!! ***                        
        //eggSackRespawnCounter = 0;
                
        AddNewSpecies(masterGenomePool.completeSpeciesPoolsList[masterGenomePool.completeSpeciesPoolsList.Count - 1].leaderboardGenomesList[0].candidateGenome, 0);

        recentlyAddedSpeciesOn = true;
        recentlyAddedSpeciesWorldPos = new Vector2(spawnPos.x, spawnPos.y);
        recentlyAddedSpeciesID = masterGenomePool.completeSpeciesPoolsList.Count - 1;
        recentlyAddedSpeciesTimeCounter = 0;

        Debug.Log("CREATE CreateAgentSpecies pos: " + spawnPos.ToString());
    }
    public void ExecuteSimEvent(SimEventData eventData) {

        simEventsManager.ExecuteEvent(this, eventData);
    }
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

            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgLifespan = Mathf.Lerp(speciesPool.avgLifespan, (float)agentRef.ageCounter, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgConsumptionDecay = 0f; // Mathf.Lerp(speciesPool.avgConsumptionDecay, agentRef.totalFoodEatenDecay, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgConsumptionPlant = Mathf.Lerp(speciesPool.avgConsumptionPlant, agentRef.totalFoodEatenPlant, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgConsumptionMeat = Mathf.Lerp(speciesPool.avgConsumptionMeat, agentRef.totalFoodEatenMeat, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgBodySize = Mathf.Lerp(speciesPool.avgBodySize, agentRef.fullSizeBodyVolume, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgSpecAttack = Mathf.Lerp(speciesPool.avgSpecAttack, (float)agentRef.coreModule.talentSpecAttackNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgSpecDefend = Mathf.Lerp(speciesPool.avgSpecDefend, (float)agentRef.coreModule.talentSpecDefenseNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgSpecSpeed = Mathf.Lerp(speciesPool.avgSpecSpeed, (float)agentRef.coreModule.talentSpecSpeedNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgSpecUtility = Mathf.Lerp(speciesPool.avgSpecUtility, (float)agentRef.coreModule.talentSpecUtilityNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgFoodSpecDecay = Mathf.Lerp(speciesPool.avgFoodSpecDecay, (float)agentRef.coreModule.dietSpecDecayNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgFoodSpecPlant = Mathf.Lerp(speciesPool.avgFoodSpecPlant, (float)agentRef.coreModule.dietSpecPlantNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgFoodSpecMeat = Mathf.Lerp(speciesPool.avgFoodSpecMeat, (float)agentRef.coreModule.dietSpecMeatNorm, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgNumNeurons = Mathf.Lerp(speciesPool.avgNumNeurons, (float)agentRef.brain.neuronList.Count, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgNumAxons = Mathf.Lerp(speciesPool.avgNumAxons, (float)agentRef.brain.axonList.Count, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgExperience = Mathf.Lerp(speciesPool.avgExperience, (float)agentRef.totalExperience, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgFitnessScore = Mathf.Lerp(speciesPool.avgFitnessScore, (float)agentRef.masterFitnessScore, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgDamageDealt = Mathf.Lerp(speciesPool.avgDamageDealt, agentRef.totalDamageDealt, lerpAmount);
            masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].avgDamageTaken = Mathf.Lerp(speciesPool.avgDamageTaken, agentRef.totalDamageTaken, lerpAmount);
            // More??
            //masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].
        }
        else {
            // -- Else:
            // -- (move to end of pool queue OR evaluate all Trials of one genome before moving onto the next)

        }

        // &&&&& *****  HERE!!!! **** &&&&&&   --- Select a species first to serve as parentGenome !! ***** &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        // Can be random selection (unbiased), or proportional to species avg Fitnesses?


        SpeciesGenomePool sourceSpeciesPool = masterGenomePool.SelectNewGenomeSourceSpecies(false, 0.33f); // select at random
                
        
        // -- Select a ParentGenome from the leaderboardList and create a mutated copy (childGenome):
        //AgentGenome newGenome = sourceSpeciesPool.GetNewMutatedGenome();
        AgentGenome newGenome = sourceSpeciesPool.GetGenomeFromFitnessLottery();
        newGenome = sourceSpeciesPool.Mutate(newGenome, true, true);

        // -- Check which species this new childGenome should belong to (most likely its parent, but maybe it creates a new species or better fits in with a diff existing species)        
        masterGenomePool.AssignNewMutatedGenomeToSpecies(newGenome, sourceSpeciesPool.speciesID, this); // Checks which species this new genome should belong to and adds it to queue / does necessary processing   
                
        // -- Clear Agent object so that it's ready to be reused
            // i.e. Set curLifecycle to .AwaitingRespawn ^
            // then new Agents should use the next available genome from the updated ToBeEvaluated pool      
        
        agentRef.SetToAwaitingRespawn(); 

        ProcessAgentScores(agentRef);  // *** CLEAN THIS UP!!! ***
        
    }
    // ********** RE-IMPLEMENT THIS LATER!!!! ******************************************************************************
    private void SpawnAgentFromEggSack(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex, EggSack parentEggSack) {
        
        numAgentsBorn++;
        //currentOldestAgent = agentsArray[rankedIndicesList[0]].ageCounter;
        agentsArray[agentIndex].InitializeSpawnAgentFromEggSack(settingsManager, agentIndex, sourceCandidate, parentEggSack); // Spawn that genome in dead Agent's body and revive it!
        theRenderKing.UpdateCritterGenericStrokesData(agentsArray[agentIndex]); // agentIndex, sourceCandidate.candidateGenome);
        
    }
    private void SpawnAgentImmaculate(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex) {
        
        bool spawnOn = true;

        float isBrushingLerp = 0f;
        if(uiManager.isDraggingMouseLeft && trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
            isBrushingLerp = 1f;
        }

        Vector3 cursorWorldPos = uiManager.curMousePositionOnWaterPlane;
        cursorWorldPos.x += UnityEngine.Random.Range(-1f, 1f) * 5f;
        cursorWorldPos.y += UnityEngine.Random.Range(-1f, 1f) * 5f;
        Vector3 spawnWorldPos = Vector3.Lerp(GetRandomFoodSpawnPosition().startPosition, cursorWorldPos, isBrushingLerp); // uiManager.curCtrlCursorPositionOnWaterPlane; // GetRandomFoodSpawnPosition().startPosition;
        
        /*
        if (recentlyAddedSpeciesOn) {

            if(speciesIndex == recentlyAddedSpeciesID) {
                spawnOn = true;
                spawnWorldPos = new Vector3(recentlyAddedSpeciesWorldPos.x + UnityEngine.Random.Range(0f, 1f), recentlyAddedSpeciesWorldPos.y + UnityEngine.Random.Range(0f, 1f), 0f);
            }
            else {
                Debug.Log("ERROR! couldn't spqawn correct speciesID!!!");
            }
        }
        else {
            spawnOn = true;
        }
        */
        /*if(isBrushingAgents) {
            spawnOn = true;

            Debug.Log("*** ON **** SIMMANAGER isBrushingAgents = TRUE!");
        }
        else {
            Debug.Log("*** OFF **** SIMMANAGER isBrushingAgents = FALSE!");
        }*/
        if(spawnOn) {
            agentsArray[agentIndex].InitializeSpawnAgentImmaculate(settingsManager, agentIndex, sourceCandidate, spawnWorldPos); // Spawn that genome in dead Agent's body and revive it!
            theRenderKing.UpdateCritterGenericStrokesData(agentsArray[agentIndex]); //agentIndex, sourceCandidate.candidateGenome);
            numAgentsBorn++;
            //Debug.Log("%%%%%%%% SpawnAgentImmaculates pos: " + spawnWorldPos.ToString());
        }
        
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
                                // figure out if agent has enough biomass;
                                float reqMass = settingsManager.agentSettings._BaseInitMass * settingsManager.agentSettings._MinPregnancyFactor;

                                if(reqMass < agentsArray[i].currentBiomass * settingsManager.agentSettings._MaxPregnancyProportion) {
                                    Debug.Log("RequiredMass met! " + reqMass.ToString() + " biomass: " + agentsArray[i].currentBiomass.ToString() + ", spent: ");
                                    totalSuitableParentAgents++;
                                    suitableParentAgentsList.Add(i);
                                }
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
                newEggSackGenome.SetToMutatedCopyOfParentGenome(eggSackGenomePoolArray[eggSackIndex], settingsManager.mutationSettingsAgents);
                eggSackGenomePoolArray[eggSackIndex] = newEggSackGenome;

                Debug.Log("BeginPregnancy! Egg[" + eggSackIndex.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                if(agentsArray[randParentAgentIndex].childEggSackRef != null && agentsArray[randParentAgentIndex].isPregnantAndCarryingEggs) {
                    Debug.Log("DOUBLE PREGNANT!! egg[" + agentsArray[randParentAgentIndex].childEggSackRef.index.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                }

                // Transfer Energy from ParentAgent to childEggSack!!! ******

                eggSackArray[eggSackIndex].InitializeEggSackFromGenome(eggSackIndex, agentsArray[randParentAgentIndex].candidateRef.candidateGenome, agentsArray[randParentAgentIndex], GetRandomFoodSpawnPosition().startPosition);
            
                agentsArray[randParentAgentIndex].BeginPregnancy(eggSackArray[eggSackIndex]);

                eggSackRespawnCounter = 0;
            }
            else {
                // Wait? SpawnImmaculate?
                /*
                int respawnCooldown = 83;
                
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

                        // TEMP::: TESTING!!!
                        eggSackArray[eggSackIndex].currentBiomass = settingsManager.agentSettings._BaseInitMass; // *** TEMP!!! ***
                        
                        eggSackRespawnCounter = 0;       
                    }         
                }  */           
            }
        }
        else {

        }
        
    }
        
    private void ProcessAgentScores(Agent agentRef) {

        numAgentsProcessed++;      
        float weightedAvgLerpVal = 1f / 64f;
        weightedAvgLerpVal = Mathf.Max(weightedAvgLerpVal, 1f / (float)(numAgentsProcessed + 1));
        // Expand this to handle more complex Fitness Functions with more components:
        
        float totalEggSackVolume = 0f;
        float totalCarrionVolume = 0f;
        float totalAgentBiomass = 0f;
        
        for(int i = 0; i < eggSackArray.Length; i++) {
            if(eggSackArray[i].curLifeStage != EggSack.EggLifeStage.Null) {
                totalEggSackVolume += eggSackArray[i].currentBiomass;
            }
        }
        for(int i = 0; i < agentsArray.Length; i++) {
            if(agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                totalCarrionVolume += agentsArray[i].currentBiomass;
            }
            if(agentsArray[i].curLifeStage == Agent.AgentLifeStage.Egg || agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                totalAgentBiomass += agentsArray[i].currentBiomass;
            }
        }

        //Debug.Log("ProcessAgentScores eggVol: " + foodManager.curGlobalEggSackVolume.ToString() + ", carrion: " + foodManager.curGlobalCarrionVolume.ToString());
        simResourceManager.curGlobalEggSackVolume = Mathf.Lerp(simResourceManager.curGlobalEggSackVolume, totalEggSackVolume, weightedAvgLerpVal);
        simResourceManager.curGlobalCarrionVolume = Mathf.Lerp(simResourceManager.curGlobalCarrionVolume, totalCarrionVolume, weightedAvgLerpVal);
        simResourceManager.curGlobalAgentBiomass = Mathf.Lerp(simResourceManager.curGlobalAgentBiomass, totalAgentBiomass, weightedAvgLerpVal);
                
        
        float approxGen = (float)numAgentsBorn / (float)(numAgents - 1);
        if (approxGen > curApproxGen) {
            //statsNutrientsEachGenerationList.Add(new Vector4(foodManager.curGlobalNutrients, foodManager.curGlobalFoodParticles, 0f, 0f));
            
            curApproxGen++;            
        }
        /*else {
            if(numAgentsProcessed < 130) {
                if(numAgentsProcessed % 8 == 0) {
                    //statsNutrientsEachGenerationList[curApproxGen - 1] = new Vector4(foodManager.curGlobalNutrients, foodManager.curGlobalFoodParticles, 0f, 0f);
                    
                }
            }
        }*/
    }

    private void UpdateSimulationClimate() {
        // Change force of turbulence, damping, other fluidSim parameters,
        // Inject pre-trained critters
        environmentFluidManager.UpdateSimulationClimate();
    }
    private void AddNewHistoricalDataEntry() {
        // add new entries to historical data lists: 
        //Debug.Log("eggVol: " + simResourceManager.curGlobalEggSackVolume.ToString() + ", carrion: " + simResourceManager.curGlobalCarrionVolume.ToString());
        statsNutrientsEachGenerationList.Add(new Vector4(simResourceManager.curGlobalAlgaeReservoir, simResourceManager.curGlobalAlgaeParticles, simResourceManager.curGlobalEggSackVolume, simResourceManager.curGlobalCarrionVolume));
        // Still used?
        statsHistoryBrainMutationFreqList.Add((float)settingsManager.curTierBrainMutationFrequency);
        statsHistoryBrainMutationAmpList.Add((float)settingsManager.curTierBrainMutationAmplitude);
        statsHistoryBrainSizeBiasList.Add((float)settingsManager.curTierBrainMutationNewLink);
        statsHistoryBodyMutationFreqList.Add((float)settingsManager.curTierBodyMutationFrequency);
        statsHistoryBodyMutationAmpList.Add((float)settingsManager.curTierBodyMutationAmplitude);
        statsHistoryBodySensorVarianceList.Add((float)settingsManager.curTierBodyMutationModules);
        statsHistoryWaterCurrentsList.Add((float)environmentFluidManager.curTierWaterCurrents);

        statsHistoryOxygenList.Add(simResourceManager.curGlobalOxygen);
        statsHistoryNutrientsList.Add(simResourceManager.curGlobalNutrients);
        statsHistoryDetritusList.Add(simResourceManager.curGlobalDetritus);
        statsHistoryDecomposersList.Add(simResourceManager.curGlobalDecomposers);
        statsHistoryAlgaeSingleList.Add(simResourceManager.curGlobalAlgaeReservoir);
        statsHistoryAlgaeParticleList.Add(simResourceManager.curGlobalAlgaeParticles);
        statsHistoryZooplanktonList.Add(simResourceManager.curGlobalAnimalParticles);
        statsHistoryLivingAgentsList.Add(simResourceManager.curGlobalAgentBiomass);
        statsHistoryDeadAgentsList.Add(simResourceManager.curGlobalCarrionVolume);
        statsHistoryEggSacksList.Add(simResourceManager.curGlobalEggSackVolume);
    }
    private void AddNewSpeciesDataEntry(int year) {
        masterGenomePool.AddNewYearlySpeciesStats(year);
    }
    private void RefreshLatestHistoricalDataEntry() {
        statsNutrientsEachGenerationList[statsNutrientsEachGenerationList.Count - 1] = new Vector4(simResourceManager.curGlobalAlgaeReservoir, simResourceManager.curGlobalAlgaeParticles, simResourceManager.curGlobalEggSackVolume, simResourceManager.curGlobalCarrionVolume);
        statsHistoryBrainMutationFreqList[statsHistoryBrainMutationFreqList.Count - 1] = settingsManager.curTierBrainMutationFrequency;
        statsHistoryBrainMutationAmpList[statsHistoryBrainMutationAmpList.Count - 1] = settingsManager.curTierBrainMutationAmplitude;
        statsHistoryBrainSizeBiasList[statsHistoryBrainSizeBiasList.Count - 1] = settingsManager.curTierBrainMutationNewLink;
        statsHistoryBodyMutationFreqList[statsHistoryBodyMutationFreqList.Count - 1] = settingsManager.curTierBodyMutationFrequency;
        statsHistoryBodyMutationAmpList[statsHistoryBodyMutationAmpList.Count - 1] = settingsManager.curTierBodyMutationAmplitude;
        statsHistoryBodySensorVarianceList[statsHistoryBodySensorVarianceList.Count - 1] = settingsManager.curTierBodyMutationModules;
        statsHistoryWaterCurrentsList[statsHistoryWaterCurrentsList.Count - 1] = environmentFluidManager.curTierWaterCurrents;

        statsHistoryOxygenList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalOxygen;
        statsHistoryNutrientsList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalNutrients;
        statsHistoryDetritusList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalDetritus;
        statsHistoryDecomposersList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalDecomposers;
        statsHistoryAlgaeSingleList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalAlgaeReservoir;
        statsHistoryAlgaeParticleList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalAlgaeParticles;
        statsHistoryZooplanktonList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalAnimalParticles;
        statsHistoryLivingAgentsList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalAgentBiomass;
        statsHistoryDeadAgentsList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalCarrionVolume;
        statsHistoryEggSacksList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalEggSackVolume;
    }
    private void RefreshLatestSpeciesDataEntry() {
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {
            // Combine these into a single stats class/struct?
            masterGenomePool.completeSpeciesPoolsList[i].avgLifespanPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgLifespanPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgLifespan;
            masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionDecayPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionDecayPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionDecay;
            masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionPlantPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionPlantPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionPlant;
            masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionMeatPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionMeatPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgConsumptionMeat;
            masterGenomePool.completeSpeciesPoolsList[i].avgBodySizePerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgBodySizePerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgBodySize;
            masterGenomePool.completeSpeciesPoolsList[i].avgSpecAttackPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgSpecAttackPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgSpecAttack;
            masterGenomePool.completeSpeciesPoolsList[i].avgSpecDefendPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgSpecDefendPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgSpecDefend;
            masterGenomePool.completeSpeciesPoolsList[i].avgSpecSpeedPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgSpecSpeedPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgSpecSpeed;
            masterGenomePool.completeSpeciesPoolsList[i].avgSpecUtilityPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgSpecUtilityPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgSpecUtility;
            masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecDecayPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecDecayPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecDecay;
            masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecPlantPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecPlantPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecPlant;
            masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecMeatPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecMeatPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgFoodSpecMeat;
            masterGenomePool.completeSpeciesPoolsList[i].avgNumNeuronsPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgNumNeuronsPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgNumNeurons;
            masterGenomePool.completeSpeciesPoolsList[i].avgNumAxonsPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgNumAxonsPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgNumAxons;
            masterGenomePool.completeSpeciesPoolsList[i].avgExperiencePerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgExperiencePerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgExperience;
            masterGenomePool.completeSpeciesPoolsList[i].avgFitnessScorePerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgFitnessScorePerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgFitnessScore;
            masterGenomePool.completeSpeciesPoolsList[i].avgDamageDealtPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgDamageDealtPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgDamageDealt;
            masterGenomePool.completeSpeciesPoolsList[i].avgDamageTakenPerYearList[masterGenomePool.completeSpeciesPoolsList[i].avgDamageTakenPerYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgDamageTaken;
        }
        //avgLifespanPerYearList[avgLifespanPerYearList.Count - 1] = avgLifespan;
        
    }
    
    public void AddNewSpecies(AgentGenome newGenome, int parentSpeciesID) {
        
        int newSpeciesID = masterGenomePool.completeSpeciesPoolsList.Count;
               
        
        SpeciesGenomePool newSpecies = new SpeciesGenomePool(newSpeciesID, parentSpeciesID, curSimYear, simAgeTimeSteps, settingsManager.mutationSettingsAgents);

        // Random Body?
        newGenome.ProcessNewSpeciesExtraMutation();
        newGenome.GenerateInitialRandomBodyGenome(); // might break?
        AgentGenome agentGenome = newSpecies.Mutate(newGenome, true, true); //
        
        // **** I want to just change the APPEARANCE of body genome, but keep the brain? ... area to revisit later
        // Maybe just do a fresh restart for now -- fully random init

        newSpecies.FirstTimeInitialize(agentGenome, masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].depthLevel + 1);
        masterGenomePool.currentlyActiveSpeciesIDList.Add(newSpeciesID);
        masterGenomePool.completeSpeciesPoolsList.Add(newSpecies);
        masterGenomePool.speciesCreatedOrDestroyedThisFrame = true;

        // Inherit Parent Data Stats:
        newSpecies.avgLifespanPerYearList.Clear();
        newSpecies.avgConsumptionDecayPerYearList.Clear();
        newSpecies.avgConsumptionPlantPerYearList.Clear();
        newSpecies.avgConsumptionMeatPerYearList.Clear();
        newSpecies.avgBodySizePerYearList.Clear();
        newSpecies.avgSpecAttackPerYearList.Clear();
        newSpecies.avgSpecDefendPerYearList.Clear();
        newSpecies.avgSpecSpeedPerYearList.Clear();
        newSpecies.avgSpecUtilityPerYearList.Clear();
        newSpecies.avgFoodSpecDecayPerYearList.Clear();
        newSpecies.avgFoodSpecPlantPerYearList.Clear();
        newSpecies.avgFoodSpecMeatPerYearList.Clear();
        newSpecies.avgNumNeuronsPerYearList.Clear();
        newSpecies.avgNumAxonsPerYearList.Clear();
        newSpecies.avgExperiencePerYearList.Clear();
        newSpecies.avgFitnessScorePerYearList.Clear();
        newSpecies.avgDamageDealtPerYearList.Clear();
        newSpecies.avgDamageTakenPerYearList.Clear();
        int lastIndex = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgLifespanPerYearList.Count - 1;
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgLifespanPerYearList.Count; i++) {
            newSpecies.avgLifespanPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgLifespanPerYearList[i]);
            newSpecies.avgConsumptionDecayPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgConsumptionDecayPerYearList[i]);
            newSpecies.avgConsumptionPlantPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgConsumptionPlantPerYearList[i]);
            newSpecies.avgConsumptionMeatPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgConsumptionMeatPerYearList[i]);
            newSpecies.avgBodySizePerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgBodySizePerYearList[i]);
            newSpecies.avgSpecAttackPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecAttackPerYearList[i]);
            newSpecies.avgSpecDefendPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecDefendPerYearList[i]);
            newSpecies.avgSpecSpeedPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecSpeedPerYearList[i]);
            newSpecies.avgSpecUtilityPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecUtilityPerYearList[i]);
            newSpecies.avgFoodSpecDecayPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFoodSpecDecayPerYearList[i]);
            newSpecies.avgFoodSpecPlantPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFoodSpecPlantPerYearList[i]);
            newSpecies.avgFoodSpecMeatPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFoodSpecMeatPerYearList[i]);
            newSpecies.avgNumNeuronsPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgNumNeuronsPerYearList[i]);
            newSpecies.avgNumAxonsPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgNumAxonsPerYearList[i]);
            newSpecies.avgExperiencePerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgExperiencePerYearList[i]);
            newSpecies.avgFitnessScorePerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFitnessScorePerYearList[i]);
            newSpecies.avgDamageDealtPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgDamageDealtPerYearList[i]);
            newSpecies.avgDamageTakenPerYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgDamageTakenPerYearList[i]);
        }  // set
        newSpecies.avgLifespan = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgLifespanPerYearList[lastIndex];
        newSpecies.avgConsumptionDecay = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgConsumptionDecayPerYearList[lastIndex];
        newSpecies.avgConsumptionPlant = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgConsumptionPlantPerYearList[lastIndex];
        newSpecies.avgConsumptionMeat = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgConsumptionDecayPerYearList[lastIndex];
        newSpecies.avgBodySize = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgBodySizePerYearList[lastIndex];
        newSpecies.avgSpecAttack = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecAttackPerYearList[lastIndex];
        newSpecies.avgSpecDefend = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecDefendPerYearList[lastIndex];
        newSpecies.avgSpecSpeed = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecSpeedPerYearList[lastIndex];
        newSpecies.avgSpecUtility = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgSpecUtilityPerYearList[lastIndex];
        newSpecies.avgFoodSpecDecay = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFoodSpecDecayPerYearList[lastIndex];
        newSpecies.avgFoodSpecPlant = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFoodSpecPlantPerYearList[lastIndex];
        newSpecies.avgFoodSpecMeat = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFoodSpecMeatPerYearList[lastIndex];
        newSpecies.avgNumNeurons = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgNumNeuronsPerYearList[lastIndex];
        newSpecies.avgNumAxons = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgNumAxonsPerYearList[lastIndex];
        newSpecies.avgExperience = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgExperiencePerYearList[lastIndex];
        newSpecies.avgFitnessScore = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgFitnessScorePerYearList[lastIndex];
        newSpecies.avgDamageDealt = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgDamageDealtPerYearList[lastIndex];
        newSpecies.avgDamageTaken = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgDamageTakenPerYearList[lastIndex];

        
        if(newSpecies.depthLevel > masterGenomePool.currentHighestDepth) {
            masterGenomePool.currentHighestDepth = newSpecies.depthLevel;
        }
        //Debug.Log("New Species Created!!! (" + newSpeciesID.ToString() + "] score: " + closestDistance.ToString());
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
    /*public int GetAgentIndexByLottery(float[] rankedFitnessList, int[] rankedIndicesList, int speciesIndex) {
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
    }*/

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

        if(vegetationManager != null) {
            vegetationManager.ClearBuffers();
        }
        if(zooplanktonManager != null) {
            zooplanktonManager.ClearBuffers();
        }
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
