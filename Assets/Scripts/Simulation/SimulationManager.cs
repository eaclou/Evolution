using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Playcraft;
using Playcraft.Pooling;
using Random = UnityEngine.Random;

// The meat of the Game, controls the primary simulation/core logic gameplay Loop
public class SimulationManager : Singleton<SimulationManager> 
{
    Lookup lookup => Lookup.instance;
    ObjectPoolMaster spawner => ObjectPoolMaster.instance;
    UIManager uiManager => UIManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    CameraManager cameraManager => CameraManager.instance;
    AudioManager audioManager => AudioManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SettingsManager settingsManager => SettingsManager.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    public QualitySettingData qualitySettings;
    public LoadingPanelUI loadingPanel;
    public SimulationStateData simStateData;
    public StartPositionsPresetLists startPositionsPresets;
    public ComputeShader computeShaderResourceGrid;    // algae grid
    public ComputeShader computeShaderPlantParticles;  // algae particles
    public ComputeShader computeShaderAnimalParticles; // animal particles
    public MasterGenomePool masterGenomePool;           // agents
    public FogSettings fogSettings;

    public TrophicLayersManager trophicLayersManager;
    public VegetationManager vegetationManager;
    public ZooplanktonManager zooplanktonManager;
    //public AgentsManager agentsManager;

    public SimEventsManager simEventsManager;
    public SimResourceManager simResourceManager;

    public List<Feat> featsList;

    public float fogAmount = 0.25f;
    public Vector4 fogColor; 

    public bool isQuickStart = true;

    //public float curPlayerMutationRate = 0.75f;  // UI-based value, giving player control over mutation frequency with one parameter

    public bool loadingComplete = false;
    
    public bool _BigBangOn => uiManager.bigBangPanelUI.isRunning;

    private static float mapSize = 256f;  // This determines scale of environment, size of FluidSim plane!!! Important!
    public static float _MapSize => mapSize;

    public static float _MaxAltitude = 5f;
    public static float _GlobalWaterLevel = 0.42f;

    private int agentGridCellResolution = 1;  // How much to subdivide the map in order to detect nearest-neighbors more efficiently --> to not be O(n^2)
    public MapGridCell[][] mapGridCellArray;

    // * WPP Rename to maxAgents (use array size to calculate numAgents if helpful)
    // * or remove the hard cap entirely and scale down the pond to create a soft cap
    [NonSerialized]
    public int numAgents = 64;

    public Agent[] agents;
    public EggSackGenome[] eggSackGenomes;
    
    public EggSack[] eggSacks;
    
    // * See comment above re numAgents
    public int numEggSacks = 48;

    public int numAgentsBorn = 0;
    public int numAgentsDied = 0;

    public StatsHistory statsHistory;

    public int curApproxGen = 1;

    public int numInitialHiddenNeurons = 2;
            
    private int numAgentsProcessed = 0;

    private int eggSackRespawnCounter;    
    private int agentRespawnCounter = 0;

    private int numAgentEvaluationsPerGenome = 1;

    public int simAgeTimeSteps = 0;
    private int numStepsInSimYear = 4096;
    private int simAgeYearCounter = 0;
    public int curSimYear = 0;
    
    MutationSettingsInstance _cachedVertebrateMutationSettings;
    MutationSettingsInstance cachedVertebrateMutationSettings => _cachedVertebrateMutationSettings ?? GetSetVertebrateMutationSettings();
    MutationSettingsInstance GetSetVertebrateMutationSettings() { return _cachedVertebrateMutationSettings = lookup.GetMutationSettingsCopy(MutationSettingsId.Vertebrate); }

    
    public int GetNumTimeStepsPerYear() {
        return numStepsInSimYear;
    }
    public SpeciesGenomePool GetSelectedGenomePool() {
        return GetGenomePoolBySpeciesID(selectionManager.currentSelection.historySelectedSpeciesID);
    }
    
    public SpeciesGenomePool GetGenomePoolBySpeciesID(int id) {
        return masterGenomePool.completeSpeciesPoolsList[id];
    }
    
    public Agent GetAgent(CandidateAgentData candidate) { return GetAgent(candidate.candidateID); }
    
    public Agent GetAgent(int candidateID)
    {
        foreach (var agent in agents)
        {
            if(agent.candidateRef == null)
                continue;

            if(candidateID == agent.candidateRef.candidateID)
                return agent;
        }
        
        return null;
    }

    public void AddHistoryEvent(SimEventData value) { simEventsManager.completeEventHistoryList.Add(value); }

    GlobalGraphData globalGraphData = new GlobalGraphData();
        
    #region Loading
    
    public void LoadingWarmupComplete()
    {
        // Turn off menu music
        audioManager.TurnOffMenuAudioGroup();
        // otherwise it's null and a giant mess
        selectionManager.SetSelected(agents[0].candidateRef);
    }
    
    public void BeginLoadingNewSimulation() { StartCoroutine(LoadingNewSimulation()); }

    IEnumerator LoadingNewSimulation() 
    {
        loadingComplete = false;

        float startTime = Time.realtimeSinceStartup;
        float masterStartTime = Time.realtimeSinceStartup;

        loadingPanel.SetCursorActive(false);
        loadingPanel.Refresh("", 0);

        LoadingInitializeCoreSimulationState();  // creates arrays and stuff for the (hopefully) only time
        Logger.Log("LoadingInitializeCoreSimulationState: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
                
        // Fitness Stuffs:::
        startTime = Time.realtimeSinceStartup;
        
        statsHistory = new StatsHistory(simResourceManager);
        statsHistory.Initialize();
        
        Logger.Log("LoadingSetUpFitnessStorage: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
        yield return null;
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInstantiateEggSacks()";
        // create first EggSacks:
        LoadingInstantiateEggSacks();

        loadingPanel.Refresh("( Reticulating Splines )", 1);
        
        Logger.Log("LoadingInstantiateEggSacks: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
        yield return null;
        // ******  Combine this with ^ ^ ^ function ??? **************
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeEggSacksFirstTime()";
        LoadingInitializeEggSacksFirstTime();
        Logger.Log("LoadingInitializeEggSacksFirstTime: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
        yield return null;
        startTime = Time.realtimeSinceStartup;
        // Can I create Egg Sacks and then immediately sapwn agents on them
        // Initialize Agents:
        //uiManager.textLoadingTooltips.text = "LoadingInstantiateAgents()";
        //LoadingInstantiateAgents();  // Fills the AgentsArray, Instantiates Agent Objects (MonoBehaviors + GameObjects)
        // TEMP BREAKOUT!!!
        
        yield return InstantiateAgentArrayOverTime();

        // *** This is being replaced by a different mechanism for spawning Agents:
        //LoadingInitializeAgentsFromGenomes(); // This was "RespawnAgents()" --  Used to actually place the Agent in the game in a random spot and set the Agent's atributes ffrom its genome
        Logger.Log("LoadingInstantiateAgents: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
        Logger.Log("End Total up to LoadingInstantiateAgents: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);
        yield return null;
        
        // Load pre-saved genomes:
        //LoadingLoadGenepoolFiles();       
        //yield return null;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeFluidSim()";
        // **** How to handle sharing simulation data between different Managers???
        // Once Agents, Food, etc. are established, Initialize the Fluid:
        LoadingInitializeFluidSim();
        Logger.Log("End Total up to LoadingInitializeFluidSim: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);

        yield return null;
        startTime = Time.realtimeSinceStartup;
        // Initialize Food:
        //uiManager.textLoadingTooltips.text = "LoadingInitializeFoodParticles()";
        LoadingInitializeAlgaeGrid();
        LoadingInitializePlantParticles();
        Logger.Log("LoadingInitializeFoodParticles: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
        Logger.Log("End Total up to LoadingInitializeFoodParticles: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);
        yield return null;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeAnimalParticles()";
        LoadingInitializeAnimalParticles();

        yield return null;

        loadingPanel.Refresh("( Calculating Enjoyment Coefficients )", 2);

        //uiManager.textLoadingTooltips.text = "GentlyRouseTheRenderMonarchHisHighnessLordOfPixels()";   
        // Wake up the Render King and prepare him for the day ahead, proudly ruling over Renderland.
        GentlyRouseTheRenderMonarchHisHighnessLordOfPixels();

        // TreeOfLife Render buffers here ???              ///////// **********************   THIS NEEDS REFACTOR!!!!! **********
        // Tree Of LIFE UI collider & RenderKing updates:
        //wtheRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, 0);
        //theRenderKing.TreeOfLifeAddNewSpecies(masterGenomePool, 1);
        //masterGenomePool.completeSpeciesPoolsList[0].isFlaggedForExtinction = true;
        //masterGenomePool.ExtinctifySpecies(this, masterGenomePool.currentlyActiveSpeciesIDList[0]);
                
        Logger.Log("End Total up to GentlyRouseTheRenderMonarchHisHighnessLordOfPixels: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);

        yield return null;
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInitializeFoodGrid()";
        LoadingInitializeResourceGrid();
        Logger.Log("LoadingInitializeFoodGrid: " + (Time.realtimeSinceStartup - startTime), debugLogStartup);
        Logger.Log("End Total up to LoadingInitializeFoodGrid: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);
        yield return null;

        //uiManager.textLoadingTooltips.text = "LoadingHookUpFluidAndRenderKing()";        
        LoadingHookUpFluidAndRenderKing();  // fluid needs refs to RK's obstacle/color cameras' RenderTextures!
        
        Logger.Log("End Total up to LoadingHookUpFluidAndRenderKing: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);
        yield return null;
        
        //uiManager.textLoadingTooltips.text = "LoadingInitializeGridCells()";
        LoadingInitializeGridCells();
        // Populates GridCells with their contents (agents/food/preds)
        PopulateGridCells();

        loadingPanel.Refresh("", 3);
        
        //yield return new WaitForSeconds(5f); // TEMP!!!
        Logger.Log("End Total: " + (Time.realtimeSinceStartup - masterStartTime), debugLogStartup);
        fluidManager.UpdateSimulationClimate();
        
        loadingComplete = true;
        loadingPanel.SetCursorActive(true);
        loadingPanel.BeginWarmUp();
                
        Logger.Log("LOADING COMPLETE - Starting WarmUp!", debugLogStartup);
    }
    
    public void LogFeat(FeatSO value)
    {
        var frame = value.useEventFrame ? Time.frameCount : 0;
        var feat = new Feat(value.message, value.type, frame, value.color, value.description);
        LogFeat(feat);
    }

    public void LogFeat(Feat feat) {
        featsList.Insert(0, feat);
    }

    private void LoadingInitializeCoreSimulationState() 
    {
        featsList = new List<Feat>();
        Feat feat = new Feat("Power of Creation", FeatType.WorldExpand, 0, Color.white, "A new world is created!");
        LogFeat(feat);

        // allocate memory and initialize data structures, classes, arrays, etc.
        globalGraphData.Initialize();

        settingsManager.Initialize();
        trophicLayersManager = new TrophicLayersManager();
        simEventsManager = new SimEventsManager();
        simResourceManager = new SimResourceManager();
        //agentsManager = new AgentsManager();
        vegetationManager = new VegetationManager(simResourceManager);
        zooplanktonManager = new ZooplanktonManager(simResourceManager);
         
        LoadingInitializePopulationGenomes();  // **** Maybe change this up ? ** depending on time of first Agent Creation?

        if(isQuickStart) {
            Logger.Log("QUICK START! Loading Pre-Trained Genomes!", debugLogStartup);
            LoadTrainingData();
        }

        simStateData = new SimulationStateData();
    }
    
    private void LoadingInitializePopulationGenomes() {
        masterGenomePool = new MasterGenomePool();
        masterGenomePool.FirstTimeInitialize(cachedVertebrateMutationSettings);  //(settingsManager.mutationSettingsVertebrates);   

        // EGGSACKS:
        eggSackGenomes = new EggSackGenome[numEggSacks];

        for(int i = 0; i < eggSackGenomes.Length; i++) {
            EggSackGenome eggSackGenome = new EggSackGenome(i);
            eggSackGenome.InitializeAsRandomGenome();
            eggSackGenomes[i] = eggSackGenome;
        }
    }
    
    private void LoadingInitializeFluidSim() {
        fluidManager.InitializeFluidSystem();
    }
    
    private void GentlyRouseTheRenderMonarchHisHighnessLordOfPixels() {
        theRenderKing.InitializeRiseAndShine();
    }
    
    private void InstantiateAgentArrayImmediate() {
        agents = new Agent[numAgents];
        for (int i = 0; i < agents.Length; i++)
            InstantiateAgent(i);          
    }
    
    private IEnumerator InstantiateAgentArrayOverTime() {
        agents = new Agent[numAgents];
        for (int i = 0; i < agents.Length; i++) {
            InstantiateAgent(i);
            yield return null;     
        }
    }    
    
    void InstantiateAgent(int index)
    {
        var agentGO = spawner.Spawn(lookup.agent, Vector3.zero);
        agentGO.name = "Agent " + index;
        var newAgent = agentGO.GetComponent<Agent>();
        
        //newAgent.speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
        newAgent.FirstTimeInitialize(); // agentGenomePoolArray[i]);
        agents[index] = newAgent;        
    }
    
    private void LoadingInitializeAlgaeGrid() {
        vegetationManager.InitializeAlgaeGrid();
    }
    
    private void LoadingInitializePlantParticles() {
        vegetationManager.InitializePlantParticles(numAgents, computeShaderPlantParticles);
    }   
     
    private void LoadingInitializeResourceGrid() {        
        vegetationManager.InitializeResourceGrid(numAgents, computeShaderResourceGrid); 
        vegetationManager.InitializeDecomposersGrid();
    }
    
    private void LoadingInitializeAnimalParticles() {
        zooplanktonManager.InitializeAnimalParticles(numAgents, computeShaderAnimalParticles);
    }
    
    private void LoadingInstantiateEggSacks() {
        eggSacks = new EggSack[numEggSacks];
        
        for (int i = 0; i < eggSacks.Length; i++) {
            var eggSackGO = spawner.Spawn(lookup.eggSack, Vector3.zero);
            eggSackGO.name = "EggSack " + i;
            var newEggSack = eggSackGO.GetComponent<EggSack>();
            
            newEggSack.speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[0]; // Mathf.FloorToInt((float)i / (float)numEggSacks * (float)numSpecies);
            newEggSack.FirstTimeInitialize();
            eggSacks[i] = newEggSack; // Add to stored list of current Food objects                     
        }
    }
    
    // Skip pregnancy, Instantiate EggSacks that begin as 'GrowingIndependent' ?
    private void LoadingInitializeEggSacksFirstTime() {  
        foreach (var eggSack in eggSacks) {
            eggSack.Nullify();
        }
    }
    
    private void LoadingHookUpFluidAndRenderKing() {
        // **** NEED TO ADDRESS THIS!!!!!! ************************
        theRenderKing.fluidObstaclesRenderCamera.targetTexture = fluidManager._ObstaclesRT; // *** See if this works **

        //theRenderKing.fluidColorRenderCamera.targetTexture = environmentFluidManager._SourceColorRT;
        
        //treeOfLifeSpeciesTreeRenderCamera targetTexture set in scene
        //temp:
        //theRenderKing.debugRT = environmentFluidManager._SourceColorRT;
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

    public Agent targetAgent => SelectionManager.instance.currentSelection.agent;
    
    public bool targetAgentIsDead => targetAgent.isDead;
    public int targetAgentAge => targetAgent.ageCounter;
    
    public int GetIndexOfFocusedAgent()
    {
        for (int i = 0; i < agents.Length; i++)
            if (IsAgentUIFocus(i))
                return i;
        
        return -1;
    }
    
    public bool IsAgentUIFocus(int index) { return GetAgentID(index) == selectionManager.currentSelection.candidate.candidateID; }
    public int GetAgentID(int agentIndex) { return agents[agentIndex].candidateRef.candidateID; }

    // * WPP: break into sections -> comments (minimum) or functions (better)
    public void TickSimulation() {
        simAgeTimeSteps++;
        simAgeYearCounter++;
        TickSparseEvents();       
        //simEventsManager.Tick();  // WPP: replaced with timer              
        eggSackRespawnCounter++;
        agentRespawnCounter++;
        masterGenomePool.Tick(); 
        audioManager.Tick();

        vegetationManager.MeasureTotalResourceGridAmount();
        vegetationManager.MeasureTotalPlantParticlesAmount();

        if (trophicLayersManager.IsLayerOn(KnowledgeMapId.Microbes)) {
            zooplanktonManager.MeasureTotalAnimalParticlesAmount();
        }
        
        // Actually measuring results of last frame's execution?
        float totalOxygenUsedByAgents = 0f;
        float totalWasteProducedByAgents = 0f;
        
        if (trophicLayersManager.IsLayerOn(KnowledgeMapId.Animals)) {
            vegetationManager.FindClosestPlantParticleToCritters(simStateData);
            
            foreach (var agent in agents) {
                totalOxygenUsedByAgents += agent.oxygenUsedLastFrame;
                totalWasteProducedByAgents += agent.wasteProducedLastFrame;          
            }

            zooplanktonManager.FindClosestAnimalParticleToCritters(simStateData);            
        } 
        
        vegetationManager.FindClosestPlantParticleToCursor(theCursorCzar.curMousePositionOnWaterPlane);
        zooplanktonManager.FindClosestAnimalParticleToCursor(theCursorCzar.curMousePositionOnWaterPlane);
        // Find best way to insert Agent Waste into ResourceGridTex.waste

        simResourceManager.oxygenUsedByAgentsLastFrame = totalOxygenUsedByAgents;
        simResourceManager.wasteProducedByAgentsLastFrame = totalWasteProducedByAgents;

        // Global Resources Here????
        // Try to make sure AlgaeReservoir and AlgaeParticles share same mechanics!!! *********************************************
        simResourceManager.Tick(trophicLayersManager, vegetationManager);  // Resource Flows Here
        
        if (targetAgent && selectionManager.currentSelection.candidate != null &&
           targetAgent.candidateRef != null &&
           targetAgent.isAwaitingRespawn && 
           targetAgent.candidateRef.candidateID == selectionManager.currentSelection.candidate.candidateID) 
        {
           cameraManager.isFollowingAgent = false;        
        }
                
        // CHECK FOR NULL Objects:        
        // ******** REVISIT CODE ORDERING!!!!  -- Should check for death Before or After agent Tick/PhysX ???
        if (trophicLayersManager.IsLayerOn(KnowledgeMapId.Animals)) {
            CheckForDevouredEggSacks();
            CheckForNullAgents();  // Result of this will affect: "simStateData.PopulateSimDataArrays(this)" !!!!!
            if (simResourceManager.curGlobalOxygen > 10f) {
                CheckForReadyToSpawnAgents();
            }            
        }

        // WPP: exposed and calculated in ScriptableObject
        fogColor = fogSettings.fogColor; 
        fogAmount = fogSettings.fogIntensity; 
        //Color.Lerp(new Color(0.15f, 0.25f, 0.52f), new Color(0.07f, 0.27f, 0.157f), Mathf.Clamp01(simResourceManager.curGlobalPlantParticles * 0.035f));
        //Mathf.Lerp(0.3f, 0.55f, Mathf.Clamp01(simResourceManager.curGlobalPlantParticles * 0.0036f));

        simStateData.PopulateSimDataArrays();  // reads from GameObject Transforms & RigidBodies!!! ++ from FluidSimulationData!!!
        theRenderKing.RenderSimulationCameras(); // will pass current info to FluidSim before it Ticks()
        // Reads from CameraRenders, GameObjects, and query GPU for fluidState
        // eventually, if agents get fluid sensors, will want to download FluidSim data from GPU into simStateData!*
        // **** Figure out proper Execution Order / in which Class to Run RenderOps while being synced w/ results of physX sim!!!        
        theRenderKing.Tick(); // updates all renderData, buffers, brushStrokes etc.
        // Simulate timestep of fluid Sim - update density/velocity maps:
        // Or should this be right at beginning of frame????? ***************** revisit...
        fluidManager.Tick(vegetationManager); // ** Clean this up, but generally OK

        //vegetationManager.ApplyDiffusionOnResourceGrid(environmentFluidManager);
        //vegetationManager.AdvectResourceGrid(environmentFluidManager);
        //if(curSimYear < 4) {  // stop simulating after certain point !! TEMPORARY!!!
        vegetationManager.SimResourceGrid(ref theRenderKing.baronVonTerrain);
        //}
        
        if (trophicLayersManager.IsLayerOn(KnowledgeMapId.Plants)) {
            vegetationManager.EatSelectedFoodParticles(simStateData); 
            // How much light/nutrients available?
            vegetationManager.SimulatePlantParticles(simStateData, simResourceManager);
        }
        
        if (trophicLayersManager.IsLayerOn(KnowledgeMapId.Microbes)) {
            zooplanktonManager.EatSelectedAnimalParticles(simStateData);        
            // Send back information about how much growth/photosynthesis there was?
            zooplanktonManager.SimulateAnimalParticles(simStateData, simResourceManager);
            // how much oxygen used? How much eaten? How much growth? How much waste/detritus?
        }
              
        if (trophicLayersManager.IsLayerOn(KnowledgeMapId.Animals)) {
            // Load gameState into Agent Brain, process brain function, read out brainResults,
            // Execute Agent Actions -- apply propulsive force to each Agent:       
            foreach (var agent in agents) {
                agent.Tick();            
            }
            foreach (var eggSack in eggSacks) {
                eggSack.Tick();
            }                
            // Apply External Forces to dynamic objects: (internal PhysX Updates):        
            ApplyFluidForcesToDynamicObjects();
        }        

        // TEMP AUDIO EFFECTS!!!!        
        //float volume = agents[0].smoothedThrottle.magnitude * 0.24f;
        //audioManager.SetPlayerSwimLoopVolume(volume);      

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
    
    /// Updates that don't happen every frame 
    private void TickSparseEvents() {
        if (simAgeYearCounter >= numStepsInSimYear) {
            curSimYear++;
            simEventsManager.curEventBucks += 5; // temporarily high!
            simAgeYearCounter = 0;

            statsHistory.AddNewHistoricalDataEntry();            
            AddNewSpeciesDataEntry(curSimYear);
            CheckForYearEvent();
        }

        if (simAgeTimeSteps % 80 == 70) {
            uiManager.speciesGraphPanelUI.UpdateSpeciesTreeDataTextures(curSimYear);
            globalGraphData.AddNewEntry(simResourceManager, GetTotalAgentBiomass());
        }

        if (simAgeTimeSteps % 79 == 3) {
            UpdateSimulationClimate();
            //RefreshLatestHistoricalDataEntry();
            RefreshLatestSpeciesDataEntry();
            //uiManager.UpdateSpeciesTreeDataTextures(curSimYear); // shouldn't lengthen!
            //uiManager.UpdateTolWorldStatsTexture(statsNutrientsEachGenerationList);
            //theRenderKing.UpdateTreeOfLifeEventLineData(simEventsManager.completeEventHistoryList);
        }
    }
    
    float GetTotalSpeciesBiomassBySpeciesIndex(int index) {
        return GetTotalAgentBiomass(trophicLayersManager.animalSlots[index].linkedSpeciesID);
    }
    
    /// Pass in linkedSpeciesID to get mass for one species only, otherwise gets entire population mass
    float GetTotalAgentBiomass(int linkedSpeciesID = -1) {
        float result = 0f;
        int aliveCreatures = 0;
        
        for (int a = 0; a < numAgents; a++) {
            if(agents[a].isAwaitingRespawn ||
               linkedSpeciesID != -1 && linkedSpeciesID != agents[a].speciesIndex) 
                continue;
            
            result += agents[a].currentBiomass;
            aliveCreatures++;
        }

        masterGenomePool.curNumAliveAgents = aliveCreatures;
        return result;
    }

    [SerializeField] YearEventData[] yearEvents;
    
    private void CheckForYearEvent() {
        foreach (var yearEvent in yearEvents) {
            if (curSimYear != yearEvent.year)
                continue;

            SimEventData newEventData = new SimEventData(yearEvent.message, simAgeTimeSteps);
            simEventsManager.completeEventHistoryList.Add(newEventData);                
        }
    }
    
    [Serializable]
    public struct YearEventData {
        public int year;
        public string message;
    }
    
    // WPP: delegated to agents
    private void ApplyFluidForcesToDynamicObjects() {
        for (int i = 0; i < agents.Length; i++) {
            agents[i].ApplyFluidForces(i);
            /*Vector4 depthSample = simStateData.depthAtAgentPositionsArray[i]; 
            agents[i].waterDepth = _GlobalWaterLevel - depthSample.x;
            
            bool depthSampleInitialized = depthSample.y != 0f && depthSample.z != 0f;
            agents[i].depthGradient = depthSampleInitialized ? 
                new Vector2(depthSample.y, depthSample.z).normalized :
                Vector2.zero;
                                    //***** world boundary *****
            if (depthSample.x > _GlobalWaterLevel || depthSample.w < 0.1f) //(floorDepth < agentSize)
            {
                float wallForce = 12.0f; // Mathf.Clamp01(agentSize - floorDepth) / agentSize;
                Vector2 gradient = agents[i].depthGradient; // new Vector2(depthSample.y, depthSample.z); //.normalized;
                agents[i].bodyRigidbody.AddForce(agents[i].bodyRigidbody.mass * wallForce * -gradient, ForceMode2D.Impulse);

                float damage = wallForce * 0.015f;  
                
                if(depthSample.w < 0.51f) {
                    damage *= 0.33f;
                }
                
                float defendBonus = 1f;
                
                if(agents[i].coreModule != null && agents[i].isMature) 
                {
                    defendBonus = agents[i].isDefending ? 0f : 1.5f;
                    damage *= defendBonus;
                    
                    agents[i].candidateRef.performanceData.totalDamageTaken += damage;
                    agents[i].coreModule.isContact[0] = 1f;
                    agents[i].coreModule.contactForceX[0] = gradient.x;
                    agents[i].coreModule.contactForceY[0] = gradient.y;
                    agents[i].TakeDamage(damage);
                }
            }
            
            agents[i].bodyRigidbody.AddForce(simStateData.fluidVelocitiesAtAgentPositionsArray[i] * 64f * agents[i].bodyRigidbody.mass, ForceMode2D.Impulse);
            agents[i].avgFluidVel = Vector2.Lerp(agents[i].avgFluidVel, simStateData.fluidVelocitiesAtAgentPositionsArray[i], 0.25f);*/
        }
        //for (int i = 0; i < eggSackArray.Length; i++) { // *** cache rigidBody reference 
        //    eggSackArray[i].GetComponent<Rigidbody2D>().AddForce(simStateData.fluidVelocitiesAtEggSackPositionsArray[i] * 16f * eggSackArray[i].GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse); //
        //}
    }
    
    public void TickPlayerStirTool()
    {
        if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.smoothedMouseVel.magnitude <= 0f)
            PlayerToolStirOff();
        else
        {
            float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);
            PlayerToolStirOn(theCursorCzar.curMousePositionOnWaterPlane, theCursorCzar.smoothedMouseVel * (0.25f + theRenderKing.baronVonWater.camDistNormalized * 1.2f), radiusMult);  
        }
    }
    
    public void PlayerToolStirOn(Vector3 origin, Vector2 forceVector, float radiusMult) {
        float magnitude = forceVector.magnitude;
        if(magnitude == 0f) {
            Debug.Log("ERROR null vector!");
        }
        magnitude *= 0.05f;
        float maxMagnitude = 0.9f;
        magnitude = Mathf.Min(magnitude, maxMagnitude);
        forceVector = forceVector.normalized * magnitude;

        //Debug.Log("PlayerToolStir pos: " + origin.ToString() + ", forceVec: [" + forceVector.x.ToString() + "," + forceVector.y.ToString() + "]  mag: " + magnitude.ToString());
        fluidManager.StirWaterOn(origin, forceVector, radiusMult);
    }
    
    public void PlayerToolStirOff() {        
        fluidManager.StirWaterOff();
    }

    // WPP: delegated sections to functions
    void PopulateGridCells() {
        ClearAllMapGridCells();

        // !! ******* MAP CHANGE!! ***** Now starts at 0,0 (bottomLeft) and goes up to mapSize,mapSize (topRight) just like UV coords *****
        // This should make conversions between CPU and GPU coords a bit simpler in the long run
        
        PopulateFoodCells();
        PopulateFriendCells();
    }
    
    // Inefficient
    void ClearAllMapGridCells()
    {
        for (int x = 0; x < agentGridCellResolution; x++) 
        {
            for (int y = 0; y < agentGridCellResolution; y++) 
            {
                mapGridCellArray[x][y].agentIndicesList.Clear();
                mapGridCellArray[x][y].eggSackIndicesList.Clear();
                //mapGridCellArray[x][y].predatorIndicesList.Clear();
            }
        }
    }
    
    void PopulateFoodCells()
    {
        for (int f = 0; f < eggSacks.Length; f++) 
        {
            (int xCoord, int yCoord) = GetCellCoordinates(eggSacks[f].transform.position);
            mapGridCellArray[xCoord][yCoord].eggSackIndicesList.Add(f);
        }        
    }
    
    void PopulateFriendCells()
    {
        for (int a = 0; a < agents.Length; a++) 
        {
            (int xCoord, int yCoord) = GetCellCoordinates(agents[a].position);
            mapGridCellArray[xCoord][yCoord].agentIndicesList.Add(a);
        }        
    }
    
    // WPP: common functionality from PopulateFood/FriendCells
    (int, int) GetCellCoordinates(Vector3 position)
    {
        float xPos = position.x;
        float yPos = position.y;
        int xCoord = Mathf.FloorToInt(xPos / mapSize * (float)agentGridCellResolution);
        int yCoord = Mathf.FloorToInt(yPos / mapSize * (float)agentGridCellResolution);
        xCoord = Mathf.Clamp(xCoord, 0, agentGridCellResolution - 1);
        yCoord = Mathf.Clamp(yCoord, 0, agentGridCellResolution - 1);
        return (xCoord, yCoord);      
    }

    // *** revisit
    private void CheckForDevouredEggSacks() {
        for (int f = 0; f < eggSacks.Length; f++) {
            if (eggSacks[f].isDepleted) {
                ProcessDeadEggSack(f);                              
            }
        }
    }
    
    private void CheckForNullAgents() { 
        foreach (var agent in agents) {
            if (agent.isNull) {
                ProcessNullAgent(agent);
            }
        } 
    }  
    
    // AUTO-SPAWN  *** revisit 
    private void CheckForReadyToSpawnAgents() {      
        int respawnThreshold = 1;
        
        for (int a = 0; a < agents.Length; a++) {
            if(agentRespawnCounter <= respawnThreshold || !agents[a].isAwaitingRespawn)
                continue; 
                
            //Debug.Log("AttemptToSpawnAgent(" + a.ToString() + ")");
            int randomTableIndex = Random.Range(0, masterGenomePool.currentlyActiveSpeciesIDList.Count);
            int speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[randomTableIndex];
            SpeciesGenomePool species = masterGenomePool.completeSpeciesPoolsList[speciesIndex];
            CandidateAgentData candidateData = species.GetNextAvailableCandidate();

            AttemptToSpawnAgent(a, speciesIndex, candidateData);
            agentRespawnCounter = 0;
        }                
    }
    
    public void AttemptToKillAgent(int speciesID, Vector2 clickPos, float brushRadius) 
    {
        Debug.Log("AttemptToKillAgent loc: " + clickPos + " ,,, species: " + speciesID + ", brushRadius: " + brushRadius);  

        foreach (var agent in agents) 
        {            
            if (!agent.isMature || speciesID != agent.speciesIndex)
                continue; 

            float distToBrushCenter = (clickPos - agent.ownPos).magnitude;

            if(distToBrushCenter >= brushRadius) 
                continue;
                                
            Debug.Log("KILL AGENT " + agent.index + " ,,, species: " + speciesID + ", distToBrushCenter: " + distToBrushCenter); 
            agent.isMarkedForDeathByUser = true;
        }
    }
    
    public void AttemptToBrushSpawnAgent(int speciesIndex) {
        if (!TryGetIndexOfAgentAwaitingRespawn(false, out int agentIndex))
            return;

        CandidateAgentData candidateData = masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNextAvailableCandidate();
        candidateData.candidateGenome = masterGenomePool.completeSpeciesPoolsList[speciesIndex].representativeCandidate.candidateGenome;
        
        //Debug.Log("AttemptToBrushSpawnAgent(" + a.ToString() + ") species: " + speciesIndex.ToString() + ", " + candidateData.ToString());
        //float isBrushingLerp = 0f;
        //if(uiManager.isDraggingMouseLeft && trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
        //    isBrushingLerp = 1f;
        //}

        Vector3 cursorWorldPos = theCursorCzar.curMousePositionOnWaterPlane;
        cursorWorldPos.x += Random.Range(-10f, 10f);
        cursorWorldPos.y += Random.Range(-10f, 10f);
        
        Vector2 spawnWorldPos = new Vector2(cursorWorldPos.x, cursorWorldPos.y); 
        
        Vector4 altitudeSample = SampleTexture(theRenderKing.baronVonTerrain.terrainHeightDataRT, spawnWorldPos / _MapSize);

        if(IsValidSpawnLoc(altitudeSample)) {
            SpawnAgentImmaculate(candidateData, agentIndex, speciesIndex, spawnWorldPos);
            candidateData.isBeingEvaluated = true;
        }
        else {
            Debug.Log("AttemptToBrushSpawnAgent(" + ") pos: " + spawnWorldPos + ", alt: " + altitudeSample);
        }
    }
    
    // WPP: search logic moved from AttemptToBrushSpawnAgent
    /// Returns whether there is an agent that: 
    /// is awaiting respawn if condition true, or is not awaiting respawn if condition false.
    /// If successful, sets index to the first found agent.
    bool TryGetIndexOfAgentAwaitingRespawn(bool condition, out int index)
    {
        for (index = 0; index < agents.Length; index++)
            if (agents[index].isAwaitingRespawn == condition)
                return true;
                
        index = -1;
        return false;
    }
    
    private bool IsValidSpawnLoc(Vector4 altitudeSample) {
        return altitudeSample.x <= _GlobalWaterLevel && altitudeSample.w >= 0.1f;
    }
    
    private void AttemptToSpawnAgent(int agentIndex, int speciesIndex, CandidateAgentData candidateData) { 
        //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString());
        // Which Species will the new agent belong to?
        // Random selection? Lottery-Selection among Species? Use this Agent's previous-life's Species?  Global Ranked Selection (across all species w/ modifiers) ?

        // Using Random Selection as first naive implementation:
        //int randomTableIndex = UnityEngine.Random.Range(0, masterGenomePool.currentlyActiveSpeciesIDList.Count);
        //int speciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[randomTableIndex];
        
        // Find next-in-line genome waiting to be evaluated:
        //CandidateAgentData candidateData = masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNextAvailableCandidate();

        // All candidates are currently being tested, or candidate list is empty?
        if(candidateData == null) {
            //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString() + ") candidateData == null\n +" +  "   ");
            return;
        }

        // Good to go?
        // Look for available EggSacks first:
        EggSack parentEggSack = null;
        
        List<int> validEggSackIndicesList = GetValidEggSackIndices(speciesIndex);
        
        // **** BROKEN BY SPECIATION UPDATE!!! *****
        if (validEggSackIndicesList.Count > 0) {  
            int randIndex = Random.Range(0, validEggSackIndicesList.Count);
            //Debug.Log("listLength:" + validEggSackIndicesList.Count.ToString() + ", randIndex = " + randIndex.ToString() + ", p: " + validEggSackIndicesList[randIndex].ToString());
            parentEggSack = eggSacks[validEggSackIndicesList[randIndex]];
            
            SpawnAgentFromEggSack(candidateData, agentIndex, speciesIndex, parentEggSack);
            candidateData.isBeingEvaluated = true;
        }
        // No eggSack found:
        else { 
            Vector3 randWorldPos = new Vector3(Random.Range(_MapSize * 0.4f, _MapSize * 0.6f), Random.Range(_MapSize * 0.4f, _MapSize * 0.6f), 0f);// GetRandomFoodSpawnPosition().startPosition;
            
            Vector2 spawnWorldPos = new Vector2(randWorldPos.x, randWorldPos.y);
            Vector4 altitudeSample = SampleTexture(theRenderKing.baronVonTerrain.terrainHeightDataRT, spawnWorldPos / _MapSize);

            if (IsValidSpawnLoc(altitudeSample)) {
                SpawnAgentImmaculate(candidateData, agentIndex, speciesIndex, spawnWorldPos);
                candidateData.isBeingEvaluated = true;
                //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString() + "x= " + altitudeSample.x.ToString() + ", w= " + altitudeSample.w.ToString() + ", pos: " + spawnWorldPos.ToString());
            }               
            //Debug.Log("AttemptToSpawnAgent Immaculate (" + agentIndex.ToString() + ") speciesIndex: " + speciesIndex.ToString() + " candidates: " + masterGenomePool.completeSpeciesPoolsList[speciesIndex].candidateGenomesList.Count.ToString());
        }       
    }
    
    /// Conditions: EggSack belongs to the right species, is at proper stage of development, and matches the species index
    List<int> GetValidEggSackIndices(int speciesIndex) {
        List<int> validEggSackIndicesList = new List<int>();
            
        for (int i = 0; i < numEggSacks; i++) {  
            if (eggSacks[i].isMature && 
                eggSacks[i].lifeStageTransitionTimeStepCounter < eggSacks[i]._MatureDurationTimeSteps &&
                eggSacks[i].speciesIndex == speciesIndex) {
                validEggSackIndicesList.Add(i);                        
            }
        }
        
        return validEggSackIndicesList;
    }
    
    public Vector4 SampleTexture(RenderTexture tex, Vector2 uv) 
    {
        Vector4[] sample = new Vector4[1];

        ComputeBuffer outputBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        outputBuffer.SetData(sample);
        int kernelCSSampleTexture = computeShaderResourceGrid.FindKernel("CSSampleTexture");
        computeShaderResourceGrid.SetTexture(kernelCSSampleTexture, "_CommonSampleTex", tex);
        computeShaderResourceGrid.SetBuffer(kernelCSSampleTexture, "outputValuesCBuffer", outputBuffer);
        computeShaderResourceGrid.SetFloat("_CoordX", uv.x);
        computeShaderResourceGrid.SetFloat("_CoordY", uv.y); 
        computeShaderResourceGrid.Dispatch(kernelCSSampleTexture, 1, 1, 1);
        
        outputBuffer.GetData(sample);
        outputBuffer.Release();

        return sample[0];
    }
       
    #endregion

    #region Process Events // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& PROCESS EVENTS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    
    public void RemoveSelectedAgentSpecies(int slotIndex) {
        //Debug.Log("pressedRemoveSpecies! " + trophicLayersManager.selectedTrophicSlotRef.slotID.ToString());

        // Need to connect UI slotID to speciesID
        if(masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
            masterGenomePool.ExtinctifySpecies(masterGenomePool.currentlyActiveSpeciesIDList[0]);
        }
    }
    
    /*public void CreateAgentSpecies(Vector3 spawnPos) {
        //eggSackArray[0].parentAgentIndex = 0;
        //eggSackArray[0].InitializeEggSackFromGenome(0, masterGenomePool.completeSpeciesPoolsList[0].representativeGenome, null, spawnPos);
        //eggSackArray[0].currentBiomass = settingsManager.agentSettings._BaseInitMass; // *** TEMP!!! ***                        
        //eggSackRespawnCounter = 0;
                
        AddNewSpecies(masterGenomePool.completeSpeciesPoolsList[masterGenomePool.completeSpeciesPoolsList.Count - 1].leaderboardGenomesList[0].candidateGenome, 0);

        //recentlyAddedSpeciesOn = true;
        //recentlyAddedSpeciesWorldPos = new Vector2(spawnPos.x, spawnPos.y);
        //recentlyAddedSpeciesID = masterGenomePool.completeSpeciesPoolsList.Count - 1;
        //recentlyAddedSpeciesTimeCounter = 0;

        Debug.Log("CREATE CreateAgentSpecies pos: " + spawnPos.ToString());
    }*/
    
    public void ExecuteSimEvent(SimEventData eventData) {
        Debug.LogError("ExecuteSimEvent(SimEventData eventData) DISABLED");
        //simEventsManager.ExecuteEvent(this, eventData);
    }
    
    public void UpdateRecords(Agent agent) {
        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[agent.speciesIndex];
        speciesPool.UpdateLongestLife(agent);
        speciesPool.UpdateMostEaten(agent);
    }
    
    // *** Confirm these are set up alright       
    public void ProcessNullAgent(Agent agentRef) {   // (Upon Agent Death:)
        numAgentsDied++;
        // Look up the connected CandidateGenome & its speciesID
        CandidateAgentData candidateData = agentRef.candidateRef;
        
        int agentSpeciesIndex = agentRef.speciesIndex;
        if(candidateData == null) {
            Debug.LogError("candidateData NULL (" + agentRef.index + ") species " + agentSpeciesIndex);
        }
        
        int candidateSpeciesIndex = candidateData.speciesID;
        if(agentSpeciesIndex != candidateSpeciesIndex) {
            Debug.LogError("agentSpeciesIndex (" + agentSpeciesIndex + " != candidateSpeciesIndex (" + candidateSpeciesIndex);
        }
        
        //Debug.Log("masterGenomePool.completeSpeciesPoolsList: " + masterGenomePool.completeSpeciesPoolsList.Count.ToString());
        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex];
        var avgPerformanceData = speciesPool.avgCandidateData.performanceData;
        var agentPerformanceData = agentRef.candidateRef.performanceData;  //***EC  - something fishy!!!!!!*****  

        UpdateRecords(agentRef);

        // save fitness score
        candidateData.ProcessCompletedEvaluation(agentRef);
        
        // check if it has finished all of its evaluations
        if(candidateData.numCompletedEvaluations >= numAgentEvaluationsPerGenome) {
            // If it has, then push the candidate to Leaderboard List so it is eligible for reproduction
            // and remove it from the ToBeEvaluated pool
            speciesPool.ProcessCompletedCandidate(candidateData, masterGenomePool);
        }

        // &&&&& *****  HERE!!!! **** &&&&&&   --- Select a species first to serve as parentGenome !! ***** &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        // Can be random selection (unbiased), or proportional to species avg Fitnesses?
        SpeciesGenomePool sourceSpeciesPool = masterGenomePool.GetSmallestSpecies(); // masterGenomePool.SelectNewGenomeSourceSpecies(false, 0.33f); // select at random
                
        // -- Select a ParentGenome from the leaderboardList and create a mutated copy (childGenome):  
        AgentGenome newGenome = sourceSpeciesPool.GetGenomeFromFitnessLottery();
        newGenome = sourceSpeciesPool.Mutate(newGenome, true, true);
        newGenome.bodyGenome.coreGenome.generation++;

        // -- Check which species this new childGenome should belong to (most likely its parent, but maybe it creates a new species or better fits in with a diff existing species)        
        //sourceSpeciesPool.AddNewCandidateGenome(newGenome);
        masterGenomePool.AssignNewMutatedGenomeToSpecies(newGenome, sourceSpeciesPool.speciesID); // Checks which species this new genome should belong to and adds it to queue / does necessary processing   
                
        // -- Clear Agent object so that it's ready to be reused
            // i.e. Set curLifecycle to .AwaitingRespawn ^
            // then new Agents should use the next available genome from the updated ToBeEvaluated pool      
        
        agentRef.SetToAwaitingRespawn();
        cameraManager.DidFollowedCreatureDie(agentRef);

        ProcessAgentScores(agentRef);
    }
    
    // ********** RE-IMPLEMENT THIS LATER!!!! ******************************************************************************
    private void SpawnAgentFromEggSack(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex, EggSack parentEggSack) {
        //Debug.Log("Spawn Creature #" + agentIndex.ToString() + " (" + numAgentsBorn.ToString() + ") FromEggSack " + parentEggSack.index.ToString() + "  " + parentEggSack._PrevPos.ToString());

        numAgentsBorn++;
        //currentOldestAgent = agentsArray[rankedIndicesList[0]].ageCounter;
        agents[agentIndex].InitializeSpawnAgentFromEggSack(agentIndex, sourceCandidate, parentEggSack, _GlobalWaterLevel); // Spawn that genome in dead Agent's body and revive it!
        theRenderKing.UpdateCritterGenericStrokesData(agents[agentIndex]); // agentIndex, sourceCandidate.candidateGenome);
        audioManager.PlayCritterSpawn(new Vector2(parentEggSack.gameObject.transform.position.x, parentEggSack.gameObject.transform.position.y)); 
    }
    
    private void SpawnAgentImmaculate(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex, Vector2 spawnPos2D) {
        //Debug.Log("SpawnAgentImmaculate! i= " + agentIndex.ToString() + ", spawnWorldPos: " + spawnPos2D.ToString());    
        
        // Spawn that genome in dead Agent's body and revive it!
        agents[agentIndex].InitializeSpawnAgentImmaculate(agentIndex, sourceCandidate, new Vector3(spawnPos2D.x, spawnPos2D.y, 0f), _GlobalWaterLevel); 
        theRenderKing.UpdateCritterGenericStrokesData(agents[agentIndex]); //agentIndex, sourceCandidate.candidateGenome);
        numAgentsBorn++;

        audioManager.PlayCritterSpawn(spawnPos2D); 
    }
    
    public void ProcessDeadEggSack(int eggSackIndex) {
        //Debug.Log("ProcessDeadEggSack(" + eggSackIndex.ToString() + ") eggSackRespawnCounter " + eggSackRespawnCounter.ToString());
        EggSack eggSack = eggSacks[eggSackIndex];

        int numActiveSpecies = masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int randEggSpeciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[Random.Range(0, numActiveSpecies)];
        eggSack.speciesIndex = randEggSpeciesIndex;

        // Check for timer?
        if (eggSackRespawnCounter <= 3)
            return;
            
        // How many active EggSacks are there in play?
        int totalSuitableParentAgents = 0;
        List<int> suitableParentAgentsList = new List<int>();
        
        for (int i = 0; i < numAgents; i++) 
        {
            if (agents[i].speciesIndex == eggSacks[eggSackIndex].speciesIndex &&
               agents[i].isMature &&
               !agents[i].isPregnantAndCarryingEggs &&
               agents[i].pregnancyRefactoryComplete &&
               agents[i].childEggSackRef) 
            {
                //if(agentsArray[i].pregnancyRefactoryTimeStepCounter > agentsArray[i].pregnancyRefactoryDuration) 

                // Able to grow eggs
                // figure out if agent has enough biomass;
                float reqMass = settingsManager.agentSettings._BaseInitMass * settingsManager.agentSettings._MinPregnancyFactor;
                float agentMass = agents[i].currentBiomass * settingsManager.agentSettings._MaxPregnancyProportion;
                
                if(reqMass < agentMass) 
                {
                    //Debug.Log("RequiredMass met! " + reqMass.ToString() + " biomass: " + agentsArray[i].currentBiomass.ToString() + ", _BaseInitMass: " + settingsManager.agentSettings._BaseInitMass.ToString());
                    totalSuitableParentAgents++;
                    suitableParentAgentsList.Add(i);
                }
            }                
        
            if (totalSuitableParentAgents > 0) 
            {
                // At Least ONE fertile Agent available:
                int parentAgentIndex = suitableParentAgentsList[Random.Range(0, totalSuitableParentAgents)];
                Agent parentAgent = agents[parentAgentIndex];

                //Debug.Log("BeginPregnancy! Egg[" + eggSackIndex.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                if(parentAgent.childEggSackRef && parentAgent.isPregnantAndCarryingEggs) 
                {
                    Debug.Log("DOUBLE PREGNANT!! egg[" + parentAgent.childEggSackRef.index + "]  Agent[" + parentAgentIndex + "]");
                }

                // RespawnFood
                // *** REFACTOR -- Need to sync egg and agent genomes to match each other
                EggSackGenome newEggSackGenome = new EggSackGenome(eggSackIndex);
                newEggSackGenome.SetToMutatedCopyOfParentGenome(eggSackGenomes[eggSackIndex], cachedVertebrateMutationSettings);
                eggSackGenomes[eggSackIndex] = newEggSackGenome;
                
                // Transfer Energy from ParentAgent to childEggSack!!! ******
                eggSack.InitializeEggSackFromGenome(eggSackIndex, 
                    parentAgent.candidateRef.candidateGenome, 
                    parentAgent, 
                    GetRandomFoodSpawnPosition().startPosition);
                    
                parentAgent.BeginPregnancy(eggSacks[eggSackIndex]);
                
                eggSackRespawnCounter = 0;
            }
            else 
            {
                // Wait? SpawnImmaculate?
                int respawnCooldown = 1000;
                
                if (eggSackRespawnCounter <= respawnCooldown)
                    continue;
                //{  // try to encourage more pregnancies?
                //Debug.Log("eggSackRespawnCounter > respawnCooldown");
                List<int> eligibleAgentIndicesList = new List<int>();
                
                for (int a = 0; a < numAgents; a++) 
                {
                    if (agents[a].isInert) continue;
                    eligibleAgentIndicesList.Add(a);
                }
                
                if (eligibleAgentIndicesList.Count <= 0)
                    continue;
                    
                int randListIndex = Random.Range(0, eligibleAgentIndicesList.Count);
                int agentIndex = eligibleAgentIndicesList[randListIndex];
            
                eggSack.parentAgentIndex = agentIndex;
                eggSack.InitializeEggSackFromGenome(eggSackIndex, agents[agentIndex].candidateRef.candidateGenome, null, GetRandomFoodSpawnPosition().startPosition);

                // TEMP::: TESTING!!!
                eggSack.currentBiomass = settingsManager.agentSettings._BaseInitMass;
                eggSackRespawnCounter = 0;
            }
        }        
    }
        
    private void ProcessAgentScores(Agent agent) {
        numAgentsProcessed++;      
        float weightedAvgLerpVal = 1f / 64f;
        weightedAvgLerpVal = Mathf.Max(weightedAvgLerpVal, 1f / (float)(numAgentsProcessed + 1));
        // Expand this to handle more complex Fitness Functions with more components:
        
        float totalEggSackVolume = GetTotalEggSackVolume();
        Vector2 livingAndDeadMass = GetTotalAgentBiomassAndCarrionVolume();
        float totalAgentBiomass = livingAndDeadMass.x;
        float totalCarrionVolume = livingAndDeadMass.y;

        simResourceManager.SetGlobalBiomassVolumes(totalEggSackVolume, totalCarrionVolume, totalAgentBiomass, weightedAvgLerpVal);
                
        float approxGen = (float)numAgentsBorn / (float)(numAgents - 1);
        if (approxGen > curApproxGen) {
            curApproxGen++;            
        }
    }
    
    float GetTotalEggSackVolume() {
        float total = 0f;
    
        foreach (var eggsack in eggSacks) 
        {
            if(eggsack.isNull) continue;
            total += eggsack.currentBiomass;
        }
        
        return total;
    }
    
    Vector2 GetTotalAgentBiomassAndCarrionVolume() {
        float living = 0f;
        float dead = 0f;
    
        foreach (var agent in agents) {
            if(agent.isDead) {
                dead += agent.currentBiomass;   
            }
            if(agent.isEgg || agent.isMature) {
                living += agent.currentBiomass;
            }
        }
        
        return new Vector2(living, dead);
    }
    
    /// Change force of turbulence, damping, other fluidSim parameters
    private void UpdateSimulationClimate() {
        fluidManager.UpdateSimulationClimate();
    }
    
    private void AddNewSpeciesDataEntry(int year) {
        masterGenomePool.AddNewYearlySpeciesStats(year);
    }
    
    private void RefreshLatestSpeciesDataEntry() { 
        foreach (var list in masterGenomePool.completeSpeciesPoolsList) {
            if (list.avgCandidateDataYearList.Count <= 0) continue; 
            list.avgCandidateDataYearList[list.avgCandidateDataYearList.Count - 1] = list.avgCandidateData;
        } 
    }
    
    // **********EC Move this to MasterGenomePool class?
    public void AddNewSpecies(AgentGenome newGenome, int parentSpeciesID) {  
        int newSpeciesID = masterGenomePool.completeSpeciesPoolsList.Count;
               
        SpeciesGenomePool newSpecies = new SpeciesGenomePool(newSpeciesID, parentSpeciesID, curSimYear, simAgeTimeSteps, cachedVertebrateMutationSettings);
        AgentGenome foundingGenome = newGenome; // newSpecies.Mutate(newGenome, true, true); //
        SpeciesGenomePool parentSpeciesPool = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID];
        
        newSpecies.FirstTimeInitialize(new CandidateAgentData(foundingGenome, newSpeciesID), parentSpeciesPool.depthLevel + 1);
        masterGenomePool.currentlyActiveSpeciesIDList.Add(newSpeciesID);
        masterGenomePool.completeSpeciesPoolsList.Add(newSpecies);
        masterGenomePool.speciesCreatedOrDestroyedThisFrame = true;
        
        List<CandidateAgentData> avgParentYearData = parentSpeciesPool.avgCandidateDataYearList;

        int lastIndex = Mathf.Max(0, avgParentYearData.Count - 1);
        if(lastIndex > 0) {
            newSpecies.avgCandidateData = avgParentYearData[lastIndex];    
        }
        //newSpecies.avgPerformanceDataYearList.Clear(); // handled inside FirstTimeInitialize()
        // Inherit Parent Data Stats:  // *** Vestigial but harmless ***
        
        foreach (var yearData in avgParentYearData) {
            newSpecies.avgCandidateDataYearList.Add(yearData);                
        }
        
        if(newSpecies.depthLevel > masterGenomePool.currentHighestDepth) {
            masterGenomePool.currentHighestDepth = newSpecies.depthLevel;
        }

        uiManager.historyPanelUI.AddNewSpeciesToPanel(newSpecies);
    }
    
    private StartPositionGenome GetInitialAgentSpawnPosition() {
        SpawnZone zone = GetRandomSpawnZone();
        float randRadius = zone.radius;
        Vector2 randOffset = Random.insideUnitCircle * randRadius;
        Vector3 startCenter = zone.transform.position;
        Vector3 startPosition = new Vector3(startCenter.x + randOffset.x, startCenter.y + randOffset.y, 0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPosition, Quaternion.identity);
        return startPosGenome;
    }
    
    private StartPositionGenome GetRandomFoodSpawnPosition() {
        StartPositionGenome startPosGenome;
        SpawnZone zone = GetValidSpawnZone();
        
        //Debug.Log("Rand Zone: " + randZone.ToString());
        float randRadius = zone.radius;
        Vector2 randOffset = Random.insideUnitCircle * randRadius;
        Vector3 startCenter = zone.transform.position;
        Vector3 startPosition = new Vector3(startCenter.x + randOffset.x, startCenter.y + randOffset.y, 0f);
        startPosGenome = new StartPositionGenome(startPosition, Quaternion.identity);
        
        return startPosGenome;
    }
    
    const int MAX_SPAWN_ZONE_SEARCH_ATTEMPTS = 10;
    
    SpawnZone GetValidSpawnZone()
    {
        SpawnZone zone = GetRandomSpawnZone();

        for (int i = 0; i < MAX_SPAWN_ZONE_SEARCH_ATTEMPTS; i++) {
            if(zone.active) {
                return zone;
            }
            
            if(zone.refactoryCounter > 100) {
                zone.refactoryCounter = 0;
                zone.active = true;
                return zone;
            }

            zone = GetRandomSpawnZone();
        }
        
        // Made it to end of loop - food will never spawn again? or just plop it wherever, refactory be damned
        Debug.Log("No active spawnZones found!");
        return zone;        
    }
    
    SpawnZone GetRandomSpawnZone()
    {
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;
        int zoneIndex = Random.Range(0, numSpawnZones);
        return startPositionsPresets.spawnZonesList[zoneIndex];        
    }
    
    private Vector3 GetRandomPredatorSpawnPosition() {
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;
        int randZone = Random.Range(0, numSpawnZones);
        float randRadius = 10f;

        Vector2 randOffset = Random.insideUnitCircle.normalized * randRadius;
        Vector3 startCenter = startPositionsPresets.spawnZonesList[randZone].transform.position;
        Vector3 startPosition = new Vector3(startCenter.x + randOffset.x, startCenter.y + randOffset.y, 0f);
        return startPosition;
    }    
    #endregion

    GameOptions gameOptions => uiManager.gameOptionsManager.gameOptions;
    QualitySettingId simulationComplexity => gameOptions.simulationComplexity;
    QualitySettingId fluidPhysicsQuality => gameOptions.fluidPhysicsQuality;
    
    public void ApplyQualitySettings() 
    {
        numAgents = qualitySettings.GetAgentCount(simulationComplexity);
        numEggSacks = qualitySettings.GetEggSackCount(simulationComplexity);
        numInitialHiddenNeurons = qualitySettings.GetHiddenNeuronCount(simulationComplexity);
        fluidManager.SetResolution(fluidPhysicsQuality);
        //Debug.Log("ApplyQualitySettings() numAgents: " + numAgents);
    }
    
    private void OnDisable() {
        simStateData?.Release();
        vegetationManager?.ClearBuffers();
        zooplanktonManager?.ClearBuffers();
    }
    
    [Header("Debug")]
    [SerializeField] bool debugLogStartup;

    #region Broken by Speciation update
    /// BROKEN BY SPECIATION UPDATE
    public void SaveTrainingData() {  
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
    
    /// BROKEN BY SPECIATION UPDATE
    public void LoadTrainingData() {  
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
    #endregion
}

#region Utility Functions (not used) 
/*public int GetAgentIndexByLottery(float[] rankedFitnessList, int[] rankedIndicesList, int speciesIndex) {
    int selectedIndex = 0;

    int popSize = numAgents / numSpecies;
    int startIndex = popSize * speciesIndex;
    int endIndex = popSize * (speciesIndex + 1);
    
    // calculate total fitness of all EVEN and ODD agents separately!
    float totalFitness = 0f;
    for (int i = startIndex; i < endIndex; i++) {            
        totalFitness += rankedFitnessList[i];
    }
    // generate random lottery value between 0f and totalFitness:
    float lotteryValue = Random.Range(0f, totalFitness);
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

#region OLD CODE: 
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
