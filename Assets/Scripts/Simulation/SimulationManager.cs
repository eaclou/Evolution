using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Playcraft;
using Random = UnityEngine.Random;

// The meat of the Game, controls the primary simulation/core logic gameplay Loop
public class SimulationManager : Singleton<SimulationManager> 
{
    public UIManager uiManager;
    public QualitySettingData qualitySettings;

    public LoadingPanelUI loadingPanel;
    public EnvironmentFluidManager environmentFluidManager;
    public TheRenderKing theRenderKing;
    public CameraManager cameraManager;
    public SettingsManager settingsManager;
    public SimulationStateData simStateData;
    public AudioManager audioManager;
    public StartPositionsPresetLists startPositionsPresets;
    public ComputeShader computeShaderResourceGrid;    // algae grid
    public ComputeShader computeShaderPlantParticles;  // algae particles
    public ComputeShader computeShaderAnimalParticles; // animal particles
    public MasterGenomePool masterGenomePool;           // agents

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
    
    public bool _BigBangOn = false;

    private static float mapSize = 256f;  // This determines scale of environment, size of FluidSim plane!!! Important!
    public static float _MapSize => mapSize;

    public static float _MaxAltitude = 5f;
    public static float _GlobalWaterLevel = 0.42f;

    private int agentGridCellResolution = 1;  // How much to subdivide the map in order to detect nearest-neighbors more efficiently --> to not be O(n^2)
    public MapGridCell[][] mapGridCellArray;

    private int numAgents = 64;
    public int _NumAgents {
        get => numAgents;
        set => numAgents = value;
    }
    
    public Agent[] agentsArray;    
    
    public EggSackGenome[] eggSackGenomePoolArray;
    
    public EggSack[] eggSackArray;
    private int numEggSacks = 48;
    public int _NumEggSacks {
        get =>  numEggSacks;
        set => numEggSacks = value;
    }   
    
    public int numAgentsBorn = 0;
    public int numAgentsDied = 0;

    public StatsHistory statsHistory;
// WPP 4/27: delegated to StatsHistory    
/*    
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
*/

    public int curApproxGen = 1;

    public int numInitialHiddenNeurons = 16;
            
    private int numAgentsProcessed = 0;

    private int eggSackRespawnCounter;    
    private int agentRespawnCounter = 0;

    private int numAgentEvaluationsPerGenome = 1;

    public int simAgeTimeSteps = 0;
    private int numStepsInSimYear = 2048;
    private int simAgeYearCounter = 0;
    public int curSimYear = 0;

    //public bool recentlyAddedSpeciesOn = false;// = true;
    //private Vector2 recentlyAddedSpeciesWorldPos; // = new Vector2(spawnPos.x, spawnPos.y);
    //private int recentlyAddedSpeciesID; // = masterGenomePool.completeSpeciesPoolsList.Count - 1;
    //private int recentlyAddedSpeciesTimeCounter = 0;

    GlobalGraphData globalGraphData = new GlobalGraphData();
    // WPP 4/27/21: delegated to GlobalGraphData
    /*
    public GraphData graphDataGlobalNutrients;
    public GraphData graphDataGlobalWaste;
    public GraphData graphDataGlobalDecomposers;
    public GraphData graphDataGlobalAlgae;
    public GraphData graphDataGlobalPlants;
    public GraphData graphDataGlobalZooplankton;
    public GraphData graphDataGlobalVertebrates;

    public GraphData graphDataVertebrateLifespan0;
    public GraphData graphDataVertebratePopulation0;
    public GraphData graphDataVertebrateFoodEaten0;
    public GraphData graphDataVertebrateGenome0;

    public GraphData graphDataVertebrateLifespan1;
    public GraphData graphDataVertebratePopulation1;
    public GraphData graphDataVertebrateFoodEaten1;
    public GraphData graphDataVertebrateGenome1;

    public GraphData graphDataVertebrateLifespan2;
    public GraphData graphDataVertebratePopulation2;
    public GraphData graphDataVertebrateFoodEaten2;
    public GraphData graphDataVertebrateGenome2;

    public GraphData graphDataVertebrateLifespan3;
    public GraphData graphDataVertebratePopulation3;
    public GraphData graphDataVertebrateFoodEaten3;
    public GraphData graphDataVertebrateGenome3;
    */
        
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;

    #region loading   // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& LOADING LOADING LOADING &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    
    public void LoadingWarmupComplete()
    {
        //simulationWarmUpComplete = true;
        // Turn off menu music:
        audioManager.TurnOffMenuAudioGroup();
        // otherwise it's null and a giant mess
        cameraManager.SetTargetAgent(agentsArray[0], 0);  
    }
    
    public void BeginLoadingNewSimulation() { StartCoroutine(LoadingNewSimulation()); }

    IEnumerator LoadingNewSimulation() 
    {
        loadingComplete = false;

        float startTime = Time.realtimeSinceStartup;
        float masterStartTime = Time.realtimeSinceStartup;

        loadingPanel.SetCursorActive(false);
        loadingPanel.Refresh("", 0);

        LoadingInitializeCoreSimulationState();  // creates arrays and stuff for the (hopefully)only time
        Debug.Log("LoadingInitializeCoreSimulationState: " + (Time.realtimeSinceStartup - startTime).ToString());
                
        // Fitness Stuffs:::
        startTime = Time.realtimeSinceStartup;
        
        statsHistory = new StatsHistory(simResourceManager, settingsManager, environmentFluidManager);
        statsHistory.Initialize();
        
        Debug.Log("LoadingSetUpFitnessStorage: " + (Time.realtimeSinceStartup - startTime).ToString());
        yield return null;
        startTime = Time.realtimeSinceStartup;
        //uiManager.textLoadingTooltips.text = "LoadingInstantiateEggSacks()";
        // create first EggSacks:
        LoadingInstantiateEggSacks();

        loadingPanel.Refresh("( Reticulating Splines )", 1);
        
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
        // *** This is being replaced by a different mechanism for spawning Agents:
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
        LoadingInitializeAlgaeGrid();
        LoadingInitializePlantParticles();
        Debug.Log("LoadingInitializeFoodParticles: " + (Time.realtimeSinceStartup - startTime).ToString());
        Debug.Log("End Total up to LoadingInitializeFoodParticles: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
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
        PopulateGridCells();
        HookUpModules();

        loadingPanel.Refresh("", 3);
        
        //yield return new WaitForSeconds(5f); // TEMP!!!
        Debug.Log("End Total: " + (Time.realtimeSinceStartup - masterStartTime).ToString());
        environmentFluidManager.UpdateSimulationClimate();
        
        loadingComplete = true;
        loadingPanel.SetCursorActive(true);
        loadingPanel.BeginWarmUp();
                
        Debug.Log("LOADING COMPLETE - Starting WarmUp!");
    }

    /*
    private void InitializeGraphData() {
        graphDataGlobalNutrients = new GraphData(uiManager.globalResourcesUI.knowledgeGraphNutrientsMat);  // testing!!!!
        graphDataGlobalWaste = new GraphData(uiManager.globalResourcesUI.knowledgeGraphDetritusMat);  // testing!!!!
        graphDataGlobalDecomposers = new GraphData(uiManager.globalResourcesUI.knowledgeGraphDecomposersMat);  // testing!!!!
        graphDataGlobalAlgae = new GraphData(uiManager.globalResourcesUI.knowledgeGraphAlgaeMat);  // testing!!!!
        graphDataGlobalPlants = new GraphData(uiManager.globalResourcesUI.knowledgeGraphPlantsMat);  // testing!!!!
        graphDataGlobalZooplankton = new GraphData(uiManager.globalResourcesUI.knowledgeGraphZooplanktonMat);  // testing!!!!
        graphDataGlobalVertebrates = new GraphData(uiManager.globalResourcesUI.knowledgeGraphVertebratesMat);  // testing!!!!
        
        //graphDataVertebrateLifespan0 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateLifespanMat0);
        //graphDataVertebratePopulation0 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebratePopulationMat0);
        //graphDataVertebrateFoodEaten0 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat0);
        //graphDataVertebrateGenome0 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateGenomeMat0);

        //graphDataVertebrateLifespan1 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateLifespanMat1);
        //graphDataVertebratePopulation1 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebratePopulationMat1);
        //graphDataVertebrateFoodEaten1 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat1);
        //graphDataVertebrateGenome1 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateGenomeMat1);

        //graphDataVertebrateLifespan2 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateLifespanMat2);
        //graphDataVertebratePopulation2 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebratePopulationMat2);
        //graphDataVertebrateFoodEaten2 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat2);
        //graphDataVertebrateGenome2 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateGenomeMat2);

        //graphDataVertebrateLifespan3 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateLifespanMat3);
        //graphDataVertebratePopulation3 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebratePopulationMat3);
        //graphDataVertebrateFoodEaten3 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat3);
        //graphDataVertebrateGenome3 = new GraphData(uiManager.knowledgeUI.knowledgeGraphVertebrateGenomeMat3);
    }
    */
    
    public void LogFeat(FeatSO value)
    {
        var frame = value.useEventFrame ? Time.frameCount : 0;
        var feat = new Feat(value.message, value.type, frame, value.color, value.description);
        LogFeat(feat);
    }

    public void LogFeat(Feat feat) {
        featsList.Insert(0, feat);
    }

    private void LoadingInitializeCoreSimulationState() {

        featsList = new List<Feat>();
        Feat feat = new Feat("Power of Creation", FeatType.WorldExpand, 0, Color.white, "A new world is created!");
        LogFeat(feat);

        // allocate memory and initialize data structures, classes, arrays, etc.
        globalGraphData.Initialize(uiManager.globalResourcesUI);

        settingsManager.Initialize();
        trophicLayersManager = new TrophicLayersManager(uiManager);
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
        masterGenomePool.FirstTimeInitialize(24, settingsManager.mutationSettingsVertebrates, uiManager);   

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
        theRenderKing.InitializeRiseAndShine();
    }
    
    // Instantiate AI Agents
    private void LoadingInstantiateAgents() {
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = new GameObject("Agent" + i.ToString());
            Agent newAgent = agentGO.AddComponent<Agent>();
            //newAgent.speciesIndex = Mathf.FloorToInt((float)i / (float)numAgents * (float)numSpecies);
            newAgent.FirstTimeInitialize(settingsManager); // agentGenomePoolArray[i]);
            agentsArray[i] = newAgent; // Add to stored list of current Agents            
        }
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
    
    // Skip pregnancy, Instantiate EggSacks that begin as 'GrowingIndependent' ?
    private void LoadingInitializeEggSacksFirstTime() {  
        foreach (var eggSack in eggSackArray)
        {
            eggSack.isDepleted = true;
            eggSack.curLifeStage = EggSack.EggLifeStage.Null;
        }
    }
    
    private void LoadingHookUpFluidAndRenderKing() {
        // **** NEED TO ADDRESS THIS!!!!!! ************************
        theRenderKing.fluidObstaclesRenderCamera.targetTexture = environmentFluidManager._ObstaclesRT; // *** See if this works **

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
    
    // WPP 4/27: delegated
    /*
    private void LoadingSetUpFitnessStorage() {
        statsNutrientsEachGenerationList = new List<Vector4>{Vector4.one * 0.0001f};
        statsHistoryBrainMutationFreqList = new List<float>{0f};
        statsHistoryBrainMutationAmpList = new List<float>{0f};
        statsHistoryBrainSizeBiasList = new List<float>{0f};
        statsHistoryBodyMutationFreqList = new List<float>{0f};
        statsHistoryBodyMutationAmpList = new List<float>{0f};
        statsHistoryBodySensorVarianceList = new List<float>{0f};
        statsHistoryWaterCurrentsList = new List<float>{0f};

        statsHistoryOxygenList = new List<float>{0f};
        statsHistoryNutrientsList = new List<float>{0f};
        statsHistoryDetritusList = new List<float>{0f};
        statsHistoryDecomposersList = new List<float>{0f};
        statsHistoryAlgaeSingleList = new List<float>{0f};
        statsHistoryAlgaeParticleList = new List<float>{0f};
        statsHistoryZooplanktonList = new List<float>{0f};
        statsHistoryLivingAgentsList = new List<float>{0f};
        statsHistoryDeadAgentsList = new List<float>{0f};
        statsHistoryEggSacksList = new List<float>{0f};
        
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
    //}
    
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

    Agent targetAgent => cameraManager.targetAgent;

    // ***WPP: break into sections -> comments (minimum) or functions (better)
    public void TickSimulation() {
        simAgeTimeSteps++;
        simAgeYearCounter++;
        TickSparseEvents();  // Updates that don't happen every frame        
        simEventsManager.Tick();  // Cooldown increment                
        eggSackRespawnCounter++;
        agentRespawnCounter++;
        masterGenomePool.Tick(); // keep track of when species created so can't create multiple per frame?

        //trophicLayersManager.Tick(this);

        /*private bool recentlyAddedSpeciesOn = false;// = true;
        private Vector2 recentlyAddedSpeciesWorldPos; // = new Vector2(spawnPos.x, spawnPos.y);
        private int recentlyAddedSpeciesID; // = masterGenomePool.completeSpeciesPoolsList.Count - 1;
        private int recentlyAddedSpeciesTimeCounter = 0;*/

        /*
        if(recentlyAddedSpeciesOn) {
            recentlyAddedSpeciesTimeCounter++;

            if(recentlyAddedSpeciesTimeCounter > 300) {
                recentlyAddedSpeciesOn = false;
                recentlyAddedSpeciesTimeCounter = 0;

                Debug.Log("recentlyAddedSpeciesOn  TIMED OUT!!!");
            }
        }
        */

        // Go through each Trophic Layer:
            // Measure resources used/produced
            // send current data to GPU

        // simulation ticks per layer


        // MEASURE GLOBAL RESOURCES:
        //if(trophicLayersManager.GetAlgaeOnOff()) {
            //vegetationManager.FindClosestAlgaeParticleToCritters(simStateData);
            //vegetationManager.MeasureTotalAlgaeParticlesAmount();

        vegetationManager.MeasureTotalResourceGridAmount();
        //}
        
        vegetationManager.MeasureTotalPlantParticlesAmount();

        if(trophicLayersManager.GetZooplanktonOnOff()) {
            zooplanktonManager.MeasureTotalAnimalParticlesAmount();
        }
        
        // Actually measuring results of last frame's execution?
        float totalOxygenUsedByAgents = 0f;
        float totalWasteProducedByAgents = 0f;
        
        if(trophicLayersManager.GetAgentsOnOff()) {
            vegetationManager.FindClosestPlantParticleToCritters(simStateData);
            
            foreach (var agent in agentsArray) {
                totalOxygenUsedByAgents += agent.oxygenUsedLastFrame;
                totalWasteProducedByAgents += agent.wasteProducedLastFrame;          
            }

            zooplanktonManager.FindClosestAnimalParticleToCritters(simStateData);            
        } 
        
        vegetationManager.FindClosestPlantParticleToCursor(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y);
        zooplanktonManager.FindClosestAnimalParticleToCursor(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y);
        // Find best way to insert Agent Waste into ResourceGridTex.waste

        simResourceManager.oxygenUsedByAgentsLastFrame = totalOxygenUsedByAgents;
        simResourceManager.wasteProducedByAgentsLastFrame = totalWasteProducedByAgents;

        // Global Resources Here????
        // Try to make sure AlgaeReservoir and AlgaeParticles share same mechanics!!! *********************************************
        simResourceManager.Tick(settingsManager, trophicLayersManager, vegetationManager);  // Resource Flows Here
        
        if(targetAgent && uiManager.focusedCandidate != null &&
           targetAgent.candidateRef != null &&
           targetAgent.curLifeStage == Agent.AgentLifeStage.AwaitingRespawn && 
           targetAgent.candidateRef.candidateID == uiManager.focusedCandidate.candidateID) 
        {
           cameraManager.isFollowingAgent = false;        
        }
                
        // CHECK FOR NULL Objects:        
        // ******** REVISIT CODE ORDERING!!!!  -- Should check for death Before or After agent Tick/PhysX ???
        if(trophicLayersManager.GetAgentsOnOff()) {
            CheckForDevouredEggSacks();
            CheckForNullAgents();  // Result of this will affect: "simStateData.PopulateSimDataArrays(this)" !!!!!
            if(simResourceManager.curGlobalOxygen > 10f) {
                CheckForReadyToSpawnAgents();
            }            
        }

        fogColor = Color.Lerp(new Color(0.15f, 0.25f, 0.52f), new Color(0.07f, 0.27f, 0.157f), Mathf.Clamp01(simResourceManager.curGlobalPlantParticles * 0.035f));
        fogAmount = Mathf.Lerp(0.3f, 0.55f, Mathf.Clamp01(simResourceManager.curGlobalPlantParticles * 0.0036f));

        simStateData.PopulateSimDataArrays(this);  // reads from GameObject Transforms & RigidBodies!!! ++ from FluidSimulationData!!!
        theRenderKing.RenderSimulationCameras(); // will pass current info to FluidSim before it Ticks()
        // Reads from CameraRenders, GameObjects, and query GPU for fluidState
        // eventually, if agents get fluid sensors, will want to download FluidSim data from GPU into simStateData!*
        // **** Figure out proper Execution Order / in which Class to Run RenderOps while being synced w/ results of physX sim!!!        
        theRenderKing.Tick(); // updates all renderData, buffers, brushStrokes etc.
        // Simulate timestep of fluid Sim - update density/velocity maps:
        // Or should this be right at beginning of frame????? ***************** revisit...
        environmentFluidManager.Tick(vegetationManager); // ** Clean this up, but generally OK

        //vegetationManager.ApplyDiffusionOnResourceGrid(environmentFluidManager);
        //vegetationManager.AdvectResourceGrid(environmentFluidManager);
        //if(curSimYear < 4) {  // stop simulating after certain point !! TEMPORARY!!!
        vegetationManager.SimResourceGrid(ref environmentFluidManager, ref theRenderKing.baronVonTerrain, ref theRenderKing);
        //}
        
        if(trophicLayersManager.GetPlantsOnOff()) {
            vegetationManager.EatSelectedFoodParticles(simStateData); // 
            // How much light/nutrients available?
            vegetationManager.SimulatePlantParticles(environmentFluidManager, theRenderKing, simStateData, simResourceManager);
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
            foreach (var agent in agentsArray) {
                agent.Tick(this, settingsManager);            
            }
            foreach (var eggSack in eggSackArray) {
                eggSack.Tick();
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

            statsHistory.AddNewHistoricalDataEntry();            
            AddNewSpeciesDataEntry(curSimYear);
            
            CheckForYearEvent();
        }

        if(simAgeTimeSteps % 80 == 10) {
            uiManager.speciesGraphPanelUI.UpdateSpeciesTreeDataTextures(curSimYear);

            // WPP 4/27: delegated
            /*
            graphDataGlobalNutrients.AddNewEntry(simResourceManager.curGlobalNutrients);
            graphDataGlobalWaste.AddNewEntry(simResourceManager.curGlobalDetritus);
            graphDataGlobalDecomposers.AddNewEntry(simResourceManager.curGlobalDecomposers);
            graphDataGlobalAlgae.AddNewEntry(simResourceManager.curGlobalAlgaeReservoir);
            graphDataGlobalPlants.AddNewEntry(simResourceManager.curGlobalPlantParticles);
            graphDataGlobalZooplankton.AddNewEntry(simResourceManager.curGlobalAnimalParticles);
            */
            
            int speciesID0 = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].linkedSpeciesID;
            int speciesID1 = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].linkedSpeciesID;
            int speciesID2 = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].linkedSpeciesID;
            int speciesID3 = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].linkedSpeciesID;
            
            float totalAgentBiomass = 0f;
            float totalSpeciesPopulation0 = 0f;
            float totalSpeciesPopulation1 = 0f;
            float totalSpeciesPopulation2 = 0f;
            float totalSpeciesPopulation3 = 0f;
            for(int a = 0; a < _NumAgents; a++) {
                if(agentsArray[a].curLifeStage != Agent.AgentLifeStage.AwaitingRespawn) {
                    totalAgentBiomass += agentsArray[a].currentBiomass;
                    if(speciesID0 == agentsArray[a].speciesIndex) {
                    totalSpeciesPopulation0 += 1f;
                    }
                    if(speciesID1 == agentsArray[a].speciesIndex) {
                        totalSpeciesPopulation1 += 1f;
                    }
                    if(speciesID2 == agentsArray[a].speciesIndex) {
                        totalSpeciesPopulation2 += 1f;
                    }
                    if(speciesID3 == agentsArray[a].speciesIndex) {
                        totalSpeciesPopulation3 += 1f;
                    }
                }                
            }
            
            globalGraphData.AddNewEntry(simResourceManager, totalAgentBiomass);
            //graphDataGlobalVertebrates.AddNewEntry(totalAgentBiomass); // simResourceManager.curGlobalAgentBiomass);
                                                                       //uiManager.UpdateTolWorldStatsTexture(statsNutrientsEachGenerationList);

            /*
            if(speciesID0 >= masterGenomePool.completeSpeciesPoolsList.Count) {
                Debug.LogError("ERROR! speciesID >= masterGenomePool.completeSpeciesPoolsList.Count");
            }
            else {
                graphDataVertebrateLifespan0.AddNewEntry(masterGenomePool.completeSpeciesPoolsList[speciesID0].avgPerformanceData.totalTicksAlive);
                graphDataVertebratePopulation0.AddNewEntry(totalSpeciesPopulation0);
                graphDataVertebrateFoodEaten0.AddNewEntry(masterGenomePool.completeSpeciesPoolsList[speciesID0].avgPerformanceData.totalFoodEatenZoop + masterGenomePool.completeSpeciesPoolsList[speciesID0].avgPerformanceData.totalFoodEatenPlant + masterGenomePool.completeSpeciesPoolsList[speciesID0].avgPerformanceData.totalFoodEatenEgg + masterGenomePool.completeSpeciesPoolsList[speciesID0].avgPerformanceData.totalFoodEatenCorpse + masterGenomePool.completeSpeciesPoolsList[speciesID0].avgPerformanceData.totalFoodEatenCreature);
                //graphDataVertebrateGenome0.AddNewEntry(masterGenomePool.completeSpeciesPoolsList[speciesID0].avgBodySize);
            }*/
            
        }

        if(simAgeTimeSteps % 79 == 3) {
            UpdateSimulationClimate();

            //RefreshLatestHistoricalDataEntry();
            RefreshLatestSpeciesDataEntry();
            //uiManager.UpdateSpeciesTreeDataTextures(curSimYear); // shouldn't lengthen!
            
            //uiManager.UpdateTolWorldStatsTexture(statsNutrientsEachGenerationList);
            
            //theRenderKing.UpdateTreeOfLifeEventLineData(simEventsManager.completeEventHistoryList);
        }
    }
    
    [SerializeField] YearEventData[] yearEvents;
    
    private void CheckForYearEvent() {
        foreach (var yearEvent in yearEvents)
        {
            if (curSimYear != yearEvent.year)
                continue;

            SimEventData newEventData = new SimEventData(yearEvent.message, simAgeTimeSteps);
            simEventsManager.completeEventHistoryList.Add(newEventData);                
        }
    }
    
    [Serializable]
    public struct YearEventData
    {
        public int year;
        public string message;
    }
    
    private void ApplyFluidForcesToDynamicObjects() {
        // ********** REVISIT CONVERSION btw fluid/scene coords and Force Amounts !!!! *************
        for (int i = 0; i < agentsArray.Length; i++) {

            Vector4 depthSample = simStateData.depthAtAgentPositionsArray[i];
            agentsArray[i].waterDepth = _GlobalWaterLevel - depthSample.x;
            if(depthSample.y == 0f && depthSample.z == 0f) {
                agentsArray[i].depthGradient = new Vector2(0f, 0f);
            }
            else {
                agentsArray[i].depthGradient = new Vector2(depthSample.y, depthSample.z).normalized;
            }
            
            //float agentSize = (agentsArray[i].fullSizeBoundingBox.x + agentsArray[i].fullSizeBoundingBox.y) * agentsArray[i].sizePercentage * 0.25f + 0.025f;
            //float floorDepth = depthSample.x * 10f;
            /*Vector3 depthSampleNorth = simStateData.depthAtAgentPositionsArray[i * 5 + 1];
            agentsArray[i].depthNorth = depthSampleNorth.x;
            Vector3 depthSampleEast = simStateData.depthAtAgentPositionsArray[i * 5 + 2];
            agentsArray[i].depthEast = depthSampleEast.x;
            Vector3 depthSampleSouth = simStateData.depthAtAgentPositionsArray[i * 5 + 3];
            agentsArray[i].depthSouth = depthSampleSouth.x;
            Vector3 depthSampleWest = simStateData.depthAtAgentPositionsArray[i * 5 + 4];
            agentsArray[i].depthWest = depthSampleWest.x;
            */
            // precalculate normals?
                                                   //***** world boundary *****
            if (depthSample.x > _GlobalWaterLevel || depthSample.w < 0.1f) //(floorDepth < agentSize)
            {
                float wallForce = 12.0f; // Mathf.Clamp01(agentSize - floorDepth) / agentSize;
                Vector2 grad = agentsArray[i].depthGradient; // new Vector2(depthSample.y, depthSample.z); //.normalized;
                agentsArray[i].bodyRigidbody.AddForce(-grad * agentsArray[i].bodyRigidbody.mass * wallForce, ForceMode2D.Impulse);

                float damage = wallForce * 0.015f;  
                
                if(depthSample.w < 0.51f) {
                    damage *= 0.33f;
                }
                
                float defendBonus = 1f;
                
                if(agentsArray[i].coreModule != null && agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                    if(agentsArray[i].isDefending) {                        
                        defendBonus = 0f;
                    }
                    else {
                        defendBonus = 1.5f; // cooldown penalty
                    }
                    damage *= defendBonus;

                    //agentsArray[i].coreModule.hitPoints[0] -= damage;
                    // currently no distinctionbetween regions:
                    //agentsArray[i].coreModule.healthHead -= damage;  //***EAC Handled inside TakeDamage() func!!!
                    //agentsArray[i].coreModule.healthBody -= damage;
                    //agentsArray[i].coreModule.healthExternal -= damage;

                    agentsArray[i].candidateRef.performanceData.totalDamageTaken += damage;

                    agentsArray[i].coreModule.isContact[0] = 1f;
                    agentsArray[i].coreModule.contactForceX[0] = grad.x;
                    agentsArray[i].coreModule.contactForceY[0] = grad.y;
        
                    agentsArray[i].TakeDamage(damage);
                }
            }
            
            agentsArray[i].bodyRigidbody.AddForce(simStateData.fluidVelocitiesAtAgentPositionsArray[i] * 64f * agentsArray[i].bodyRigidbody.mass, ForceMode2D.Impulse);

            agentsArray[i].avgFluidVel = Vector2.Lerp(agentsArray[i].avgFluidVel, simStateData.fluidVelocitiesAtAgentPositionsArray[i], 0.25f);
        }
        /*for (int i = 0; i < eggSackArray.Length; i++) { // *** cache rigidBody reference
            
            eggSackArray[i].GetComponent<Rigidbody2D>().AddForce(simStateData.fluidVelocitiesAtEggSackPositionsArray[i] * 16f * eggSackArray[i].GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse); //
            
        }*/
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
    
    private void HookUpModules() 
    {            
        // mapSize is global now, get with the times, jeez-louise!
        //float cellSize = mapSize / agentGridCellResolution;
        //Vector2 playerPos = new Vector2(playerAgent.transform.localPosition.x, playerAgent.transform.localPosition.y);

        // ***** Check for inactive/Null agents and cull them from consideration:
        // ******  REFACTOR!!! BROKEN BY SPECIATION UPDATE! ***

        // **** Add priority targeting here? save closest of each time for later determination? ***

        // Find NearestNeighbors:
        for (int a = 0; a < agentsArray.Length; a++) 
        {
            if (agentsArray[a].curLifeStage != Agent.AgentLifeStage.Mature)
                continue;
 
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
            // *** Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
            // *** WPP: Simplify: foreach, invert conditions with continue (or AND)
            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].agentIndicesList.Count; i++) {
                int neighborIndex = mapGridCellArray[xCoord][yCoord].agentIndicesList[i];
                int neighborSpeciesIndex = agentsArray[neighborIndex].speciesIndex; 

                if(agentsArray[neighborIndex].curLifeStage == Agent.AgentLifeStage.Mature) {
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
        
            foreach (var eggSackIndex in mapGridCellArray[xCoord][yCoord].eggSackIndicesList)
            {
                if (eggSackArray[eggSackIndex].curLifeStage == EggSack.EggLifeStage.Null ||
                    eggSackArray[eggSackIndex].isProtectedByParent)
                    continue;

                Vector2 eggSackPos = new Vector2(eggSackArray[eggSackIndex].rigidbodyRef.transform.position.x, eggSackArray[eggSackIndex].rigidbodyRef.transform.position.y);
                float distEggSack = (eggSackPos - agentPos).magnitude - (eggSackArray[eggSackIndex].curSize.magnitude + 1f) * 0.5f;  // subtract food & agent radii

                if (distEggSack > nearestFoodDistance) 
                    continue;

                closestEggSackIndex = eggSackIndex;
                nearestFoodDistance = distEggSack;                           
            }
        
            // Set proper references between AgentBrains and Environment/Game Objects:::
            // ***** DISABLED!!!! *** NEED TO RE_IMPLEMENT THIS LATER!!!! ********************************************
            SetNearestToCritter(agentsArray[a].coreModule, closestFriendIndex, closestEnemyAgentIndex, closestEggSackIndex, a);            
        }
    }
    
    private void SetNearestToCritter(CritterModuleCore critter, int closestFriendIndex, 
    int closestEnemyAgentIndex, int closestEggSackIndex, int agentIndex)
    {
        if (critter == null)
        {
            Debug.LogError("agentsArray[" + agentIndex.ToString() + "].coreModule == null) " + agentsArray[agentIndex].curLifeStage.ToString());                
            return;
        }
    
        critter.nearestFriendAgent = agentsArray[closestFriendIndex];
        critter.nearestEnemyAgent = agentsArray[closestEnemyAgentIndex];
            
        // * If the result is that the module is set to null, set it explcitly with a ternary
        if(closestEggSackIndex == -1) 
            return;

        critter.nearestEggSackModule = eggSackArray[closestEggSackIndex];                 
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
    private void CheckForReadyToSpawnAgents() {    // AUTO-SPAWN   
        int respawnThreshold = 55;
        //lifespan = uiManagerRef.gameManager.simulationManager.graphDataVertebrateLifespan0.curVal;
        //       population = uiManagerRef.gameManager.simulationManager.graphDataVertebratePopulation0.curVal;
        //        foodEaten = uiManagerRef.gameManager.simulationManager.graphDataVertebrateFoodEaten0.curVal;
        //        genome = uiManagerRef.gameManager.simulationManager.graphDataVertebrateGenome0.curVal
        /*if (graphDataVertebratePopulation0.curVal < 32) {
            respawnThreshold = 21;
        }
        else if (graphDataVertebratePopulation0.curVal > 48) {
            respawnThreshold = 700;
        }*/

        for (int a = 0; a < agentsArray.Length; a++) {            

            if(agentRespawnCounter > respawnThreshold) {

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
    
    public void AttemptToKillAgent(int speciesID, Vector2 clickPos, float brushRadius) 
    {
        Debug.Log("AttemptToKillAgent loc: " + clickPos.ToString() + " ,,, species: " + speciesID.ToString() + ", brushRadius: " + brushRadius.ToString());  

        // * WPP: refactor with early exit (continue)
        for (int a = 0; a < agentsArray.Length; a++) 
        {            
            if (agentsArray[a].curLifeStage == Agent.AgentLifeStage.Mature) 
            {
                if(speciesID == agentsArray[a].speciesIndex) 
                {
                    float distToBrushCenter = (clickPos - agentsArray[a].ownPos).magnitude;

                    if(distToBrushCenter < brushRadius) 
                    {
                        Debug.Log("KILL AGENT " + a + " ,,, species: " + speciesID + ", distToBrushCenter: " + distToBrushCenter); 
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
                candidateData.candidateGenome = masterGenomePool.completeSpeciesPoolsList[speciesIndex].representativeCandidate.candidateGenome;
                
                if (candidateData == null) {
                    Debug.LogError("GetNextAvailableCandidate(): candidateData NULL!!!!");
                }
                else {
                    //Debug.Log("AttemptToBrushSpawnAgent(" + a.ToString() + ") species: " + speciesIndex.ToString() + ", " + candidateData.ToString());
                    
                    // Spawn POS:
                    
                    //float isBrushingLerp = 0f;
                    //if(uiManager.isDraggingMouseLeft && trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                    //    isBrushingLerp = 1f;
                    //}

                    Vector3 cursorWorldPos = theCursorCzar.curMousePositionOnWaterPlane;
                    cursorWorldPos.x += UnityEngine.Random.Range(-1f, 1f) * 10f;
                    cursorWorldPos.y += UnityEngine.Random.Range(-1f, 1f) * 10f;
                    
                    Vector2 spawnWorldPos = new Vector2(cursorWorldPos.x, cursorWorldPos.y); 
        
                    Vector4 altitudeSample = uiManager.SampleTexture(theRenderKing.baronVonTerrain.terrainHeightDataRT, spawnWorldPos / _MapSize);

                    bool isValidSpawnLoc = true;
                    if(altitudeSample.x > _GlobalWaterLevel) {
                        isValidSpawnLoc = false;
                    }
                    if(altitudeSample.w < 0.1f) {
                        isValidSpawnLoc = false;
                    }
        
                    if(isValidSpawnLoc) {
                        SpawnAgentImmaculate(candidateData, a, speciesIndex, spawnWorldPos);
                        candidateData.isBeingEvaluated = true;
                    }
                    else {
                        Debug.Log("AttemptToBrushSpawnAgent(" + ") pos: " + spawnWorldPos.ToString() + ", alt: " + altitudeSample.ToString());
                    }
                    
                    break;
                }
                
            }
            
        } 
    }
    private void AttemptToSpawnAgent(int agentIndex, int speciesIndex, CandidateAgentData candidateData) { //, int speciesIndex) {
        //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString());
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
            //bool foundValidEggSack = false;
            EggSack parentEggSack = null;
            List<int> validEggSackIndicesList = new List<int>();
            for(int i = 0; i < numEggSacks; i++) {  // if EggSack belongs to the right species
                if(eggSackArray[i].curLifeStage == EggSack.EggLifeStage.Mature) {
                    if(eggSackArray[i].lifeStageTransitionTimeStepCounter < eggSackArray[i]._MatureDurationTimeSteps) {  // egg sack is at proper stage of development
                        if(eggSackArray[i].speciesIndex == speciesIndex) {
                            validEggSackIndicesList.Add(i);
                        }                        
                    }
                }
            }

            //validEggSackIndicesList.Clear(); // ********************************************* REMOVE THIS AFTER FIX!!!! ************************************
            if(validEggSackIndicesList.Count > 0) {  // move this inside the for loop ??   // **** BROKEN BY SPECIATION UPDATE!!! *****
                int randIndex = UnityEngine.Random.Range(0, validEggSackIndicesList.Count);
                //Debug.Log("listLength:" + validEggSackIndicesList.Count.ToString() + ", randIndex = " + randIndex.ToString() + ", p: " + validEggSackIndicesList[randIndex].ToString());
                parentEggSack = eggSackArray[validEggSackIndicesList[randIndex]];
                
                SpawnAgentFromEggSack(candidateData, agentIndex, speciesIndex, parentEggSack);
                candidateData.isBeingEvaluated = true;
            }
            else { // No eggSack found:
                   //if(agentIndex != 0) {  // temp hack to avoid null reference exceptions:


                Vector3 randWorldPos = new Vector3(UnityEngine.Random.Range(_MapSize * 0.4f, _MapSize * 0.6f), UnityEngine.Random.Range(_MapSize * 0.4f, _MapSize * 0.6f), 0f);// GetRandomFoodSpawnPosition().startPosition;
                // Find parent agent location:
                //Agent parentAgent;
                /*for(int i = 0; i < numAgents; i++) {
                    if(agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature) {
                        float rand = UnityEngine.Random.Range(0f, 1f);
                        if(rand < 0.1f) {
                            float mag = 7.5f;
                            randWorldPos = new Vector3(agentsArray[i].ownPos.x + UnityEngine.Random.Range(-1f, 1f) * mag, agentsArray[i].ownPos.y + UnityEngine.Random.Range(-1f, 1f) * mag, 0f);

                            break;
                        }
                    }
                }*/

                Vector2 spawnWorldPos = new Vector2(randWorldPos.x, randWorldPos.y);
                Vector4 altitudeSample = uiManager.SampleTexture(theRenderKing.baronVonTerrain.terrainHeightDataRT, spawnWorldPos / _MapSize);
                  
                bool isValidSpawnLoc = true;
                if(altitudeSample.x > _GlobalWaterLevel) {
                    isValidSpawnLoc = false;
                }
                if(altitudeSample.w < 0.1f) {
                    isValidSpawnLoc = false;
                }


                if(isValidSpawnLoc) {
                    SpawnAgentImmaculate(candidateData, agentIndex, speciesIndex, spawnWorldPos);
                    candidateData.isBeingEvaluated = true;

                    //Debug.Log("AttemptToSpawnAgent(" + agentIndex.ToString() + "x= " + altitudeSample.x.ToString() + ", w= " + altitudeSample.w.ToString() + ", pos: " + spawnWorldPos.ToString());
                }
                else {
                    //Debug.Log("INVALID SPAWN POS " + spawnWorldPos.ToString() + ", alt: " + altitudeSample.ToString());
                }

                
                //}    
                //Debug.Log("AttemptToSpawnAgent Immaculate (" + agentIndex.ToString() + ") speciesIndex: " + speciesIndex.ToString() + " candidates: " + masterGenomePool.completeSpeciesPoolsList[speciesIndex].candidateGenomesList.Count.ToString());
            }
        }       
    }
       
    #endregion

    #region Process Events // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& PROCESS EVENTS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    
    public void RemoveSelectedAgentSpecies(int slotIndex) {
        //Debug.Log("pressedRemoveSpecies! " + trophicLayersManager.selectedTrophicSlotRef.slotID.ToString());

        // Need to connect UI slotID to speciesID
        if(masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
            masterGenomePool.ExtinctifySpecies(this, masterGenomePool.currentlyActiveSpeciesIDList[0]);
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
    
    public void UpdateRecords(Agent agentRef) {
        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[agentRef.speciesIndex];
        if(agentRef.ageCounter > speciesPool.recordLongestLife) {
            speciesPool.recordLongestLife = agentRef.ageCounter;
            speciesPool.recordHolderLongestLife = agentRef.candidateRef;

            if(speciesPool.numAgentsEvaluated > 24) {
                speciesPool.hallOfFameGenomesList.Add(agentRef.candidateRef);
            }
            //Debug.Log("it works! " + speciesPool.recordLongestLife.ToString() + ", candidate: " + agentRef.candidateRef.candidateID.ToString() + ", species: " + agentRef.candidateRef.speciesID.ToString());
        }
        float totalEaten = (agentRef.candidateRef.performanceData.totalFoodEatenCorpse + agentRef.candidateRef.performanceData.totalFoodEatenEgg + agentRef.candidateRef.performanceData.totalFoodEatenCorpse + agentRef.candidateRef.performanceData.totalFoodEatenPlant + agentRef.candidateRef.performanceData.totalFoodEatenZoop);
        if(totalEaten > speciesPool.recordMostEaten) {
            speciesPool.recordMostEaten = totalEaten;
            speciesPool.recordHolderMostEaten = agentRef.candidateRef;

            if(speciesPool.numAgentsEvaluated > 24) {
                speciesPool.hallOfFameGenomesList.Add(agentRef.candidateRef);
            }
        }
    }
    
    // *** Confirm these are set up alright       
    public void ProcessNullAgent(Agent agentRef) {   // (Upon Agent Death:)
        numAgentsDied++;
        // Look up the connected CandidateGenome & its speciesID
        CandidateAgentData candidateData = agentRef.candidateRef;
        
        int agentSpeciesIndex = agentRef.speciesIndex;
        if(candidateData == null) {
            Debug.LogError("candidateData NULL (" + agentRef.index.ToString() + ") species " + agentSpeciesIndex.ToString());
        }
        
        int candidateSpeciesIndex = candidateData.speciesID;
        if(agentSpeciesIndex != candidateSpeciesIndex) {
            Debug.LogError("agentSpeciesIndex (" + agentSpeciesIndex.ToString() + " != candidateSpeciesIndex (" + candidateSpeciesIndex.ToString());
        }
        
        //Debug.Log("masterGenomePool.completeSpeciesPoolsList: " + masterGenomePool.completeSpeciesPoolsList.Count.ToString());
        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex];
        var avgPerformanceData = speciesPool.avgCandidateData.performanceData;
        var agentPerformanceData = agentRef.candidateRef.performanceData;  //***EC  - something fishy!!!!!!*****  

        UpdateRecords(agentRef);

        // -- save its fitness score
        candidateData.ProcessCompletedEvaluation(agentRef);
        
        // -- check if it has finished all of its evaluations
        if(candidateData.numCompletedEvaluations >= numAgentEvaluationsPerGenome) {
            // -- If it has:
            // -- then push the candidate to Leaderboard List so it is eligible for reproduction
            // -- at the same time, remove it from the ToBeEvaluated pool
            speciesPool.ProcessCompletedCandidate(candidateData, masterGenomePool);

            /*
            float lerpAmount = Mathf.Max(0.01f, 1f / (float)speciesPool.numAgentsEvaluated);  //***EC isn't this handled elsewhere???

            avgPerformanceData.totalTicksAlive = (int)Mathf.Lerp((float)avgPerformanceData.totalTicksAlive, (float)agentRef.ageCounter, lerpAmount);
            avgPerformanceData.totalFoodEatenCorpse = Mathf.Lerp(avgPerformanceData.totalFoodEatenCorpse, agentPerformanceData.totalFoodEatenCorpse, lerpAmount);
            avgPerformanceData.totalFoodEatenPlant = Mathf.Lerp(avgPerformanceData.totalFoodEatenPlant, agentPerformanceData.totalFoodEatenPlant, lerpAmount);
            avgPerformanceData.totalFoodEatenZoop = Mathf.Lerp(avgPerformanceData.totalFoodEatenZoop, agentPerformanceData.totalFoodEatenZoop, lerpAmount);
            avgPerformanceData.totalFoodEatenEgg = Mathf.Lerp(avgPerformanceData.totalFoodEatenEgg, agentPerformanceData.totalFoodEatenEgg, lerpAmount);
            avgPerformanceData.totalFoodEatenCreature = Mathf.Lerp(avgPerformanceData.totalFoodEatenCreature, agentPerformanceData.totalFoodEatenCreature, lerpAmount);
            */
            //Debug.Log("WTF?? " + avgPerformanceData.totalTicksAlive.ToString());

            //speciesPool.avgPerformanceData.avgBodySize = Mathf.Lerp(speciesPool.avgPerformanceData.avgBodySize, agentRef.candidateRef.performanceData.fullSizeBodyVolume, lerpAmount);
            //speciesPool.avgPerformanceData.avgSpecAttack = Mathf.Lerp(speciesPool.avgPerformanceData.avgSpecAttack, (float)agentRef.candidateRef.performanceData.coreModule.talentSpecAttackNorm, lerpAmount);
            ////speciesPool.avgPerformanceData.avgSpecDefend = Mathf.Lerp(speciesPool.avgPerformanceData.avgSpecDefend, (float)agentRef.candidateRef.performanceData.coreModule.talentSpecDefenseNorm, lerpAmount);
            //speciesPool.avgPerformanceData.avgSpecSpeed = Mathf.Lerp(speciesPool.avgPerformanceData.avgSpecSpeed, (float)agentRef.candidateRef.performanceData.coreModule.talentSpecSpeedNorm, lerpAmount);
            //speciesPool.avgPerformanceData.avgSpecUtility = Mathf.Lerp(speciesPool.avgPerformanceData.avgSpecUtility, (float)agentRef.candidateRef.performanceData.coreModule.talentSpecUtilityNorm, lerpAmount);
            //speciesPool.avgPerformanceData.avgFoodSpecDecay = Mathf.Lerp(speciesPool.avgPerformanceData.avgFoodSpecDecay, (float)agentRef.candidateRef.performanceData.coreModule.dietSpecDecayNorm, lerpAmount);
            //speciesPool.avgPerformanceData.avgFoodSpecPlant = Mathf.Lerp(speciesPool.avgPerformanceData.avgFoodSpecPlant, (float)agentRef.candidateRef.performanceData.coreModule.dietSpecPlantNorm, lerpAmount);
            //speciesPool.avgPerformanceData.avgFoodSpecMeat = Mathf.Lerp(speciesPool.avgPerformanceData.avgFoodSpecMeat, (float)agentRef.candidateRef.performanceData.coreModule.dietSpecMeatNorm, lerpAmount);
            //speciesPool.avgPerformanceData.avgNumNeurons = Mathf.Lerp(speciesPool.avgPerformanceData.avgNumNeurons, (float)agentRef.candidateRef.performanceData.brain.neuronList.Count, lerpAmount);
            //speciesPool.avgPerformanceData.avgNumAxons = Mathf.Lerp(speciesPool.avgPerformanceData.avgNumAxons, (float)agentRef.candidateRef.performanceData.brain.axonList.Count, lerpAmount);
            //speciesPool.avgPerformanceData.avgExperience = Mathf.Lerp(speciesPool.avgPerformanceData.avgExperience, (float)agentRef.candidateRef.performanceData.totalExperience, lerpAmount);
            //speciesPool.avgPerformanceData.avgFitnessScore = Mathf.Lerp(speciesPool.avgPerformanceData.avgFitnessScore, (float)agentRef.candidateRef.performanceData.masterFitnessScore, lerpAmount);
            /*
            avgPerformanceData.totalDamageDealt = Mathf.Lerp(avgPerformanceData.totalDamageDealt, agentPerformanceData.totalDamageDealt, lerpAmount);
            avgPerformanceData.totalDamageTaken = Mathf.Lerp(avgPerformanceData.totalDamageTaken, agentPerformanceData.totalDamageTaken, lerpAmount);

            avgPerformanceData.totalTicksRested = (int)Mathf.Lerp((float)avgPerformanceData.totalTicksRested, (float)agentPerformanceData.totalTicksRested, lerpAmount);
            avgPerformanceData.totalTimesAttacked = (int)Mathf.Lerp((float)avgPerformanceData.totalTimesAttacked, (float)agentPerformanceData.totalTimesAttacked, lerpAmount);
            avgPerformanceData.totalTimesDefended = (int)Mathf.Lerp((float)avgPerformanceData.totalTimesDefended, (float)agentPerformanceData.totalTimesDefended, lerpAmount);
            avgPerformanceData.totalTimesDashed = (int)Mathf.Lerp((float)avgPerformanceData.totalTimesDashed, (float)agentPerformanceData.totalTimesDashed, lerpAmount);
            avgPerformanceData.totalTimesPregnant = (int)Mathf.Lerp((float)avgPerformanceData.totalTimesPregnant, (float)agentPerformanceData.totalTimesPregnant, lerpAmount);
            */
            // More??
            //masterGenomePool.completeSpeciesPoolsList[agentSpeciesIndex].
        }
        else {
            // -- Else:
            // -- (move to end of pool queue OR evaluate all Trials of one genome before moving onto the next)
            // only used if single genomes are tested multiple times
        }

        // &&&&& *****  HERE!!!! **** &&&&&&   --- Select a species first to serve as parentGenome !! ***** &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        // Can be random selection (unbiased), or proportional to species avg Fitnesses?
        SpeciesGenomePool sourceSpeciesPool = masterGenomePool.SelectNewGenomeSourceSpecies(false, 0.33f); // select at random
                
        // -- Select a ParentGenome from the leaderboardList and create a mutated copy (childGenome):  
        AgentGenome newGenome = sourceSpeciesPool.GetGenomeFromFitnessLottery();
        newGenome = sourceSpeciesPool.Mutate(newGenome, true, true);
        newGenome.bodyGenome.coreGenome.generation++;

        // -- Check which species this new childGenome should belong to (most likely its parent, but maybe it creates a new species or better fits in with a diff existing species)        
        //sourceSpeciesPool.AddNewCandidateGenome(newGenome);
        masterGenomePool.AssignNewMutatedGenomeToSpecies(newGenome, sourceSpeciesPool.speciesID, this); // Checks which species this new genome should belong to and adds it to queue / does necessary processing   
                
        // -- Clear Agent object so that it's ready to be reused
            // i.e. Set curLifecycle to .AwaitingRespawn ^
            // then new Agents should use the next available genome from the updated ToBeEvaluated pool      
        
        agentRef.SetToAwaitingRespawn(); 

        ProcessAgentScores(agentRef);  // *** CLEAN THIS UP!!! ***
    }
    
    // ********** RE-IMPLEMENT THIS LATER!!!! ******************************************************************************
    private void SpawnAgentFromEggSack(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex, EggSack parentEggSack) {
        //Debug.Log("Spawn Creature #" + agentIndex.ToString() + " (" + numAgentsBorn.ToString() + ") FromEggSack " + parentEggSack.index.ToString() + "  " + parentEggSack._PrevPos.ToString());

        numAgentsBorn++;
        //currentOldestAgent = agentsArray[rankedIndicesList[0]].ageCounter;
        agentsArray[agentIndex].InitializeSpawnAgentFromEggSack(settingsManager, agentIndex, sourceCandidate, parentEggSack, _GlobalWaterLevel); // Spawn that genome in dead Agent's body and revive it!
        theRenderKing.UpdateCritterGenericStrokesData(agentsArray[agentIndex]); // agentIndex, sourceCandidate.candidateGenome);
    }
    
    private void SpawnAgentImmaculate(CandidateAgentData sourceCandidate, int agentIndex, int speciesIndex, Vector2 spawnPos2D) {
        //Debug.Log("SpawnAgentImmaculate!i= " + agentIndex.ToString() + ", spawnWorldPos: " + spawnPos2D.ToString());
            
        agentsArray[agentIndex].InitializeSpawnAgentImmaculate(settingsManager, agentIndex, sourceCandidate, new Vector3(spawnPos2D.x, spawnPos2D.y, 0f), _GlobalWaterLevel); // Spawn that genome in dead Agent's body and revive it!
        theRenderKing.UpdateCritterGenericStrokesData(agentsArray[agentIndex]); //agentIndex, sourceCandidate.candidateGenome);
        numAgentsBorn++;
    }
    
    public void ProcessDeadEggSack(int eggSackIndex) {
        //Debug.Log("ProcessDeadEggSack(" + eggSackIndex.ToString() + ") eggSackRespawnCounter " + eggSackRespawnCounter.ToString());

        int numActiveSpecies = masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int randEggSpeciesIndex = masterGenomePool.currentlyActiveSpeciesIDList[Random.Range(0, numActiveSpecies)];
        eggSackArray[eggSackIndex].speciesIndex = randEggSpeciesIndex;

        // Check for timer?
        if (eggSackRespawnCounter <= 3)
            return;
            
        // How many active EggSacks are there in play?
        int totalSuitableParentAgents = 0;
        List<int> suitableParentAgentsList = new List<int>();
        
        for(int i = 0; i < _NumAgents; i++) 
        {
            if(agentsArray[i].speciesIndex == eggSackArray[eggSackIndex].speciesIndex &&
               agentsArray[i].curLifeStage == Agent.AgentLifeStage.Mature &&
               !agentsArray[i].isPregnantAndCarryingEggs &&
               agentsArray[i].pregnancyRefactoryTimeStepCounter > agentsArray[i].pregnancyRefactoryDuration) 
            {
                //if(agentsArray[i].pregnancyRefactoryTimeStepCounter > agentsArray[i].pregnancyRefactoryDuration) 

                // Able to grow eggs
                // figure out if agent has enough biomass;
                float reqMass = settingsManager.agentSettings._BaseInitMass * settingsManager.agentSettings._MinPregnancyFactor;
                float agentMass = agentsArray[i].currentBiomass * settingsManager.agentSettings._MaxPregnancyProportion;
                
                if(reqMass < agentMass) 
                {
                    //Debug.Log("RequiredMass met! " + reqMass.ToString() + " biomass: " + agentsArray[i].currentBiomass.ToString() + ", _BaseInitMass: " + settingsManager.agentSettings._BaseInitMass.ToString());
                    totalSuitableParentAgents++;
                    suitableParentAgentsList.Add(i);
                }
            }                
        
            if(totalSuitableParentAgents > 0) 
            {
                // At Least ONE fertile Agent available:
                int randParentAgentIndex = suitableParentAgentsList[Random.Range(0, totalSuitableParentAgents)];

                // RespawnFood  // *** REFACTOR -- Need to sync egg and agent genomes to match each other
                EggSackGenome newEggSackGenome = new EggSackGenome(eggSackIndex);
                newEggSackGenome.SetToMutatedCopyOfParentGenome(eggSackGenomePoolArray[eggSackIndex], settingsManager.mutationSettingsVertebrates);
                eggSackGenomePoolArray[eggSackIndex] = newEggSackGenome;

                //Debug.Log("BeginPregnancy! Egg[" + eggSackIndex.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                if(agentsArray[randParentAgentIndex].childEggSackRef && agentsArray[randParentAgentIndex].isPregnantAndCarryingEggs) 
                {
                    Debug.Log("DOUBLE PREGNANT!! egg[" + agentsArray[randParentAgentIndex].childEggSackRef.index.ToString() + "]  Agent[" + randParentAgentIndex.ToString() + "]");
                }

                // Transfer Energy from ParentAgent to childEggSack!!! ******

                eggSackArray[eggSackIndex].InitializeEggSackFromGenome(eggSackIndex, agentsArray[randParentAgentIndex].candidateRef.candidateGenome, agentsArray[randParentAgentIndex], GetRandomFoodSpawnPosition().startPosition);
            
                agentsArray[randParentAgentIndex].BeginPregnancy(eggSackArray[eggSackIndex]);

                eggSackRespawnCounter = 0;
            }
            else 
            {
                // Wait? SpawnImmaculate?
                int respawnCooldown = 1000;
                
                //if(eggSackRespawnCounter > respawnCooldown) 
                if (eggSackRespawnCounter <= respawnCooldown)
                    continue;
                //{  // try to encourage more pregnancies?
                //Debug.Log("eggSackRespawnCounter > respawnCooldown");
                List<int> eligibleAgentIndicesList = new List<int>();
                
                for(int a = 0; a < numAgents; a++) 
                {
                    if(!agentsArray[a].isInert) 
                    {
                        eligibleAgentIndicesList.Add(a);
                    }
                    //if(agentsArray[a].isInert)
                    //{

                    //}
                    //else 
                    //{
                    //eligibleAgentIndicesList.Add(a);
                    //}
                }
                
                //if(eligibleAgentIndicesList.Count > 0) 
                if (eligibleAgentIndicesList.Count <= 0)
                    continue;
                //{
                int randListIndex = Random.Range(0, eligibleAgentIndicesList.Count);
                int agentIndex = eligibleAgentIndicesList[randListIndex];
            
                eggSackArray[eggSackIndex].parentAgentIndex = agentIndex;
                eggSackArray[eggSackIndex].InitializeEggSackFromGenome(eggSackIndex, agentsArray[agentIndex].candidateRef.candidateGenome, null, GetRandomFoodSpawnPosition().startPosition);

                // TEMP::: TESTING!!!
                eggSackArray[eggSackIndex].currentBiomass = settingsManager.agentSettings._BaseInitMass; // *** TEMP!!! ***

                eggSackRespawnCounter = 0;       
                //}         
                //}           
            }
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
    
    // WPP 4/27/21
    /*
    private void AddNewHistoricalDataEntry() {
        // add new entries to historical data lists: 
        //Debug.Log("eggVol: " + simResourceManager.curGlobalEggSackVolume.ToString() + ", carrion: " + simResourceManager.curGlobalCarrionVolume.ToString());
        statsNutrientsEachGenerationList.Add(new Vector4(simResourceManager.curGlobalAlgaeReservoir, simResourceManager.curGlobalPlantParticles, simResourceManager.curGlobalEggSackVolume, simResourceManager.curGlobalCarrionVolume));
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
        statsHistoryAlgaeParticleList.Add(simResourceManager.curGlobalPlantParticles);
        statsHistoryZooplanktonList.Add(simResourceManager.curGlobalAnimalParticles);
        statsHistoryLivingAgentsList.Add(simResourceManager.curGlobalAgentBiomass);
        statsHistoryDeadAgentsList.Add(simResourceManager.curGlobalCarrionVolume);
        statsHistoryEggSacksList.Add(simResourceManager.curGlobalEggSackVolume);
    }
    */
    
    private void AddNewSpeciesDataEntry(int year) {
        masterGenomePool.AddNewYearlySpeciesStats(year);
    }
    
    // WPP 4/27: delegated, never called
    /*
    private void RefreshLatestHistoricalDataEntry() {
        statsNutrientsEachGenerationList[statsNutrientsEachGenerationList.Count - 1] = new Vector4(simResourceManager.curGlobalAlgaeReservoir, simResourceManager.curGlobalPlantParticles, simResourceManager.curGlobalEggSackVolume, simResourceManager.curGlobalCarrionVolume);
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
        statsHistoryAlgaeParticleList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalPlantParticles;
        statsHistoryZooplanktonList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalAnimalParticles;
        statsHistoryLivingAgentsList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalAgentBiomass;
        statsHistoryDeadAgentsList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalCarrionVolume;
        statsHistoryEggSacksList[statsHistoryOxygenList.Count - 1] = simResourceManager.curGlobalEggSackVolume;
    }
    */
    
    private void RefreshLatestSpeciesDataEntry() {   
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {
            if(masterGenomePool.completeSpeciesPoolsList[i].avgCandidateDataYearList.Count > 0) {
                masterGenomePool.completeSpeciesPoolsList[i].avgCandidateDataYearList[masterGenomePool.completeSpeciesPoolsList[i].avgCandidateDataYearList.Count - 1] = masterGenomePool.completeSpeciesPoolsList[i].avgCandidateData;
            }            
        }
    }
    
    public void AddNewSpecies(AgentGenome newGenome, int parentSpeciesID) {  // **********EC Move this to MasterGenomePool class?
        int newSpeciesID = masterGenomePool.completeSpeciesPoolsList.Count;
               
        SpeciesGenomePool newSpecies = new SpeciesGenomePool(newSpeciesID, parentSpeciesID, curSimYear, simAgeTimeSteps, settingsManager.mutationSettingsVertebrates);

        AgentGenome foundingGenome = newGenome; // newSpecies.Mutate(newGenome, true, true); //
        
        newSpecies.FirstTimeInitialize(new CandidateAgentData(foundingGenome, newSpeciesID), masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].depthLevel + 1);
        masterGenomePool.currentlyActiveSpeciesIDList.Add(newSpeciesID);
        masterGenomePool.completeSpeciesPoolsList.Add(newSpecies);
        masterGenomePool.speciesCreatedOrDestroyedThisFrame = true;

        //newSpecies.avgPerformanceDataYearList.Clear(); // handled inside FirstTimeInitialize()
        int lastIndex = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgCandidateDataYearList.Count - 1;
        // Inherit Parent Data Stats:  // *** Vestigial but harmless ***
        
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgCandidateDataYearList.Count; i++) {
            newSpecies.avgCandidateDataYearList.Add(masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgCandidateDataYearList[i]);
        }  
        // set
        newSpecies.avgCandidateData = masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgCandidateDataYearList[lastIndex];
        
        if(newSpecies.depthLevel > masterGenomePool.currentHighestDepth) {
            masterGenomePool.currentHighestDepth = newSpecies.depthLevel;
        }        
    }
    
    private StartPositionGenome GetInitialAgentSpawnPosition(int speciesIndex)
    {
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;
        int randZone = Random.Range(0, numSpawnZones);
        float randRadius = startPositionsPresets.spawnZonesList[randZone].radius;
        Vector2 randOffset = Random.insideUnitCircle * randRadius;
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
        int randZone = Random.Range(0, numSpawnZones);

        // *** WPP: replace magic numbers with variables
        for(int i = 0; i < 10; i++) {

            randZone = Random.Range(0, numSpawnZones);

            if(startPositionsPresets.spawnZonesList[randZone].active) {
                break;  // use this one
            }
            if(startPositionsPresets.spawnZonesList[randZone].refactoryCounter > 100) {
                startPositionsPresets.spawnZonesList[randZone].refactoryCounter = 0;
                startPositionsPresets.spawnZonesList[randZone].active = true;
                break;
            }

            if(i == 9) {
                //made it to end of loop - food will never spawn again? or just plop it wherever, refactory be damned..
                Debug.Log("No active spawnZones found!");
            }
        }
        //Debug.Log("Rand Zone: " + randZone.ToString());
        float randRadius = startPositionsPresets.spawnZonesList[randZone].radius;
        Vector2 randOffset = Random.insideUnitCircle * randRadius;
        startPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);
        startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        
        return startPosGenome;
    }
    
    private Vector3 GetRandomPredatorSpawnPosition() {
        Vector3 startPos;
        int numSpawnZones = startPositionsPresets.spawnZonesList.Count;
        int randZone = Random.Range(0, numSpawnZones);
        float randRadius = 10f;

        Vector2 randOffset = Random.insideUnitCircle.normalized * randRadius;
        startPos = new Vector3(startPositionsPresets.spawnZonesList[randZone].transform.position.x + randOffset.x, 
                               startPositionsPresets.spawnZonesList[randZone].transform.position.y + randOffset.y, 
                               0f);
        return startPos;
    }    
    #endregion

    #region Utility Functions (not used) // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& UTILITY FUNCTIONS! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
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
    
    GameOptions gameOptions => uiManager.gameOptionsManager.gameOptions;
    QualitySettingId simulationComplexity => gameOptions.simulationComplexity;
    QualitySettingId fluidPhysicsQuality => gameOptions.fluidPhysicsQuality;
    
    public void ApplyQualitySettings() {
        _NumAgents = qualitySettings.GetAgentCount(simulationComplexity);
        _NumEggSacks = qualitySettings.GetEggSackCount(simulationComplexity);
        numInitialHiddenNeurons = qualitySettings.GetHiddenNeuronCont(simulationComplexity);
        environmentFluidManager.SetResolution(fluidPhysicsQuality);
    }
    
    private void OnDisable() {
        simStateData?.Release();
        vegetationManager?.ClearBuffers();
        zooplanktonManager?.ClearBuffers();
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
