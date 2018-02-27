using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class SimulationManager : MonoBehaviour {
    
    public Text textDebugTrainingInfo;
    public Button buttonToggleRecording;
    public Button buttonToggleTrainingSupervised;
    public Button buttonResetGenomes;
    public Button buttonClearTrainingData;
    public Button buttonToggleTrainingPersistent;

    public UIManager uiManager;
    public EnvironmentFluidManager environmentFluidManager;
    //public Camera mainCam;
    public CameraManager cameraManager;
    private int numAgents = 64;
    private int supervisedGenomePoolSize = 64;  // Brain evaluated tested on DataSamples only
    private int persistentGenomePoolSize = 64;  // spawned as Agents that live until they are killed naturally, tested on Fitness Function
    private int transientGenomePoolSize = 48;  // spawned as Agents (in the background layer) that are short-lived and tested on Fitness Function + DataSamples
    //private int populationSize = 64;
    //public Material playerMat;
    public Material agentBeautyMatTemplate;
    public Material fluidColliderMatTemplate;
    //public Material foodFluidColliderMatTemplate;
    public Agent playerAgent;
    public Agent[] agentsArray;
    public Material[] agentFluidColliderMaterialsArray;
    public Material[] foodMaterialsArray;
    public Texture2D[] agentDebugTexturesArray;
    //public Texture2D[] foodDebugTexturesArray;

    public BodyGenome bodyGenomeTemplate;
    public AgentGenome[] supervisedGenomePoolArray;
    public AgentGenome[] prevGenSupervisedGenomePoolArray;
    public AgentGenome[] persistentGenomePoolArray;
    private int[] prevGenSupervisedRankedIndicesList; //= new int[rawFitnessScoresArraySupervised.Length];
    private float[] prevGenSupervisedRankedFitnessList; //= new float[rawFitnessScoresArraySupervised.Length];
    private int[] rankedIndicesListPersistent;
    private float[] rankedFitnessListPersistent;

    public FoodModule[] foodArray;
    private int numFood = 36;
    public PredatorModule[] predatorArray;
    private int numPredators = 12;

    public bool recording = false;
    public List<DataSample> dataSamplesList;  // master pool
    public List<DataSample> currentDataBatch;
    private int maxSavedDataSamples = 1024;  // dataSamples above this number replace existing samples randomly
    private int dataBatchSize = 48;
    private int minDataSamplesForTraining = 4;  // won't start training brains until DataList has at least this many samples
    private int periodicSamplingRate = 32;  // saves a sample of player's data every (this #) frames
    public SettingsManager settingsManager;
    //public MutationSettings mutationSettingsSupervised;
    //public MutationSettings mutationSettingsPersistent;
    public float[] rawFitnessScoresArraySupervised;
    public float[] rawFitnessScoresArrayPersistent;
    public bool trainingRequirementsMetSupervised = true;  // minimum reqs met
    public bool isTrainingSupervised = false;
    public bool isTrainingPersistent = false;
    private float lastHorizontalInput = 0f;
    private float lastVerticalInput = 0f;
    private int timeStepCounter = 1;
    public int curGen;
    public float avgFitnessLastGenSupervised = 0f;
    public float bestFitnessScoreSupervised = 0f;
    public float avgFitnessLastGenPersistent = 0f;
    public float bestFitnessScorePersistent = 0f;
    public int curTestingGenomeSupervised = 0;
    public int curTestingSample = 0;
    private Agent dummyAgent; // used for supervised training (brain evaluation)
    private StartPositionGenome dummyStartGenome;
    private bool firstTimeTrainingSupervised = true;
    private bool firstTimeTrainingPersistent = true;
    public int numPersistentAgentsBorn = 0;
    public int currentOldestAgent = 0;
    public List<float> supervisedScoresList;
    public float supervisedAvgRecordScore;

    public float rawFitnessScoreBlankAgentSupervised = 0f;
    public float lastGenBlankAgentFitnessSupervised = 0f;
    
    private int agentGridCellResolution = 1;  // How much to subdivide the map in order to detect nearest-neighbors more efficiently --> to not be O(n^2)
    public MapGridCell[][] mapGridCellArray;

    public int recordPlayerAge = 0;
    public int recordBotAge = 0;

    private int idleFramesToBotControl = 10;
    private int idleFramesCounter = 0;
    public int botToHumanControlTransitionFrameCount = 8;
    public int humanToBotControlTransitionFrameCount = 30;

    public float rollingAverageAgentScore = 0f;
    public List<float> persistentScoresList;
    public float persistentAvgRecordScore = 1f;
    public int curPersistentApproxGen = 1;

    // GRID SEARCH!!!
    public GridSearchManager gridSearchManager;
    public bool isGridSearching = false;
        
    // need to be able to update Agent's Brain on the fly?  --- but needs to access the Module to set up inputs/outputs???
    // Ability to run a Brain Headless (without instantiating an Agent?)

    public void InitializeTrainingApparatus() {
        
        GameObject dummyAgentGO = new GameObject("DummyAgent");
        dummyAgent = dummyAgentGO.AddComponent<Agent>();
        dummyStartGenome = new StartPositionGenome(Vector3.zero, Quaternion.identity);

        GenerateDataRepelWalls();
        //CreateFakeData();

        ResetTrainingForNewGenSupervised();
    }
    public void InitializeNewSimulation() {
        //ToggleTrainingPersistent();        
        // Create Environment (Or have it pre-built)
        

        settingsManager.Initialize();

        persistentScoresList = new List<float>();
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
        playerAgentGO.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
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
        }
        
        // SPAWN AGENTS:
        RespawnAgents();

        // FOOODDDD!!!!
        foodArray = new FoodModule[numFood];
        SpawnFood();
        predatorArray = new PredatorModule[numPredators];
        SpawnPredators();

        // Send agent info to FluidBG:
        environmentFluidManager.agentsArray = agentsArray;
        environmentFluidManager.playerAgent = playerAgent;
        environmentFluidManager.foodArray = foodArray;
        environmentFluidManager.predatorsArray = predatorArray;

        InitializeGridCells();
        HookUpModules();
        InitializeTrainingApparatus();


        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        ToggleTrainingPersistent();
        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        // TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!! TEMPORARY!!!!
        // DON"T LEAVE THIS!!!
        //RunAutomatedGridSearch();
    }
    private void RespawnAgents() {  // doesn't create the Agent classes, just resets their data
        for(int i = 0; i < persistentGenomePoolSize; i++) {
            Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
            StartPositionGenome agentStartPosGenome = new StartPositionGenome(startPos, Quaternion.identity);            
            agentsArray[i].InitializeAgentFromGenome(persistentGenomePoolArray[i], agentStartPosGenome);
        }        
    }
    private void InitializePopulationGenomes() {

        // Genome Pools:        
        supervisedGenomePoolArray = new AgentGenome[supervisedGenomePoolSize];
        prevGenSupervisedGenomePoolArray = new AgentGenome[supervisedGenomePoolSize];
        prevGenSupervisedRankedIndicesList = new int[supervisedGenomePoolSize];
        prevGenSupervisedRankedFitnessList = new float[supervisedGenomePoolSize];

        // !@$#!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        // ========= DONT FORGET TO UNDO THIS!!! ============
        persistentGenomePoolArray = supervisedGenomePoolArray; // shared pool!
        //old (correct): persistentGenomePoolArray = new AgentGenome[persistentGenomePoolSize];
        //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        // !@$#!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


        //float initialConnectionWeights = settingsManager.mutationSettingsSupervised.initialConnectionChance;
        for (int i = 0; i < supervisedGenomePoolArray.Length; i++) {   // Create initial Population Supervised Learners
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
            agentGenome.InitializeRandomBrainFromCurrentBody(settingsManager.mutationSettingsSupervised.initialConnectionChance);
            supervisedGenomePoolArray[i] = agentGenome;
            prevGenSupervisedGenomePoolArray[i] = agentGenome;
            prevGenSupervisedRankedIndicesList[i] = i;
            prevGenSupervisedRankedFitnessList[i] = 1f;
        }

        for (int i = 0; i < persistentGenomePoolArray.Length; i++) {   // Create initial Population Supervised Learners
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
            agentGenome.InitializeRandomBrainFromCurrentBody(settingsManager.mutationSettingsPersistent.initialConnectionChance);
            persistentGenomePoolArray[i] = agentGenome;
        }

        // Sort Fitness Scores Persistent:
        rankedIndicesListPersistent = new int[persistentGenomePoolSize];
        rankedFitnessListPersistent = new float[persistentGenomePoolSize];

        for (int i = 0; i < persistentGenomePoolSize; i++) {
            rankedIndicesListPersistent[i] = i;
            rankedFitnessListPersistent[i] = 1f;
        }
    }
    public void InitializeGridCells() {
        mapGridCellArray = new MapGridCell[agentGridCellResolution][];
        for(int i = 0; i < agentGridCellResolution; i++) {
            mapGridCellArray[i] = new MapGridCell[agentGridCellResolution];
        }

        float mapSize = 80f;
        float cellSize = mapSize / agentGridCellResolution;

        for (int x = 0; x < agentGridCellResolution; x++) {
            for(int y = 0; y < agentGridCellResolution; y++) {
                Vector2 cellTopLeft = new Vector2(cellSize * x, cellSize * y);
                Vector2 cellBottomRight = new Vector2(cellSize * (x + 1), cellSize * (y + 1));
                mapGridCellArray[x][y] = new MapGridCell(cellTopLeft, cellBottomRight);
            }
        }
    }
    public void ReviveFood(int index) {        
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-36f, 36f), UnityEngine.Random.Range(-36f, 36f), 0f);
        foodArray[index].transform.localPosition = startPos;
        foodArray[index].Respawn();
    }
    public void RevivePredator(int index) {
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-36f, 36f), UnityEngine.Random.Range(-36f, 36f), 0f);
        predatorArray[index].transform.localPosition = startPos;
    }
    public void PopulateGridCells() {
        
        // Inefficient!!!
        for (int x = 0; x < agentGridCellResolution; x++) {
            for (int y = 0; y < agentGridCellResolution; y++) {                
                mapGridCellArray[x][y].friendIndicesList.Clear();
                mapGridCellArray[x][y].foodIndicesList.Clear();
                mapGridCellArray[x][y].predatorIndicesList.Clear();
            }
        }

        float mapSize = 160f;
        float cellSize = mapSize / agentGridCellResolution;

        // FOOD!!! :::::::
        for (int f = 0; f < foodArray.Length; f++) {
            float xPos = foodArray[f].transform.localPosition.x;
            float yPos = foodArray[f].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);           
            mapGridCellArray[xCoord][yCoord].foodIndicesList.Add(f);
        }

        // FRIENDS::::::
        for (int a = 0; a < agentsArray.Length; a++) {
            float xPos = agentsArray[a].transform.localPosition.x;
            float yPos = agentsArray[a].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);

            //Debug.Log("PopulateGrid(" + a.ToString() + ") [" + xCoord.ToString() + "," + yCoord.ToString() + "]");
            mapGridCellArray[xCoord][yCoord].friendIndicesList.Add(a);
        }

        // PREDATORS !!! :::::::
        for (int p = 0; p < predatorArray.Length; p++) {
            float xPos = predatorArray[p].transform.localPosition.x;
            float yPos = predatorArray[p].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            mapGridCellArray[xCoord][yCoord].predatorIndicesList.Add(p);
        }

        // CHECK FOR DEAD FOOD!!! :::::::
        for (int f = 0; f < foodArray.Length; f++) {
            if (foodArray[f].isDepleted) {
                //Debug.Log("Food " + f.ToString() + " is Depleted!");sddd
                ReviveFood(f);
            }
        }

        // CHECK FOR DEAD AGENTS!!!  // Better place for this????
        for(int a = 0; a < agentsArray.Length; a++) {
            if(agentsArray[a].isDead) {
                ProcessDeadAgent(a);
            }
        }
        if(playerAgent.isDead) {
            ProcessDeadPlayer();
        }
    }
    public void ProcessDeadAgent(int agentIndex) {

        if(agentsArray[agentIndex].ageCounter > recordBotAge) {
            recordBotAge = agentsArray[agentIndex].ageCounter;
        }

        rollingAverageAgentScore = Mathf.Lerp(rollingAverageAgentScore, (float)agentsArray[agentIndex].ageCounter, 1f / 512f);
        float approxGen = (float)numPersistentAgentsBorn / (float)persistentGenomePoolSize;
        if(approxGen > curPersistentApproxGen) {
            persistentScoresList.Add(rollingAverageAgentScore);
            if(rollingAverageAgentScore > persistentAvgRecordScore) {
                persistentAvgRecordScore = rollingAverageAgentScore;
            }
            //Debug.Log("persistentScoresList " + curPersistentApproxGen.ToString() + ": " + rollingAverageAgentScore.ToString());
            // fitnessTex:
            uiManager.fitnessDisplayTexture.Resize(persistentScoresList.Count, 1);
            for(int t = 0; t < persistentScoresList.Count; t++) {
                uiManager.fitnessDisplayTexture.SetPixel(t, 1, new Color(persistentScoresList[t], persistentScoresList[t], persistentScoresList[t]));
            }
            uiManager.fitnessDisplayTexture.Apply();
            uiManager.fitnessDisplayMat.SetFloat("_BestScore", persistentAvgRecordScore);
                        
            
            // Grid Search:
            if (isGridSearching) {
                UpdateGridSearch(curPersistentApproxGen, rollingAverageAgentScore);
            }
            curPersistentApproxGen++;
        }

        // Respawn Agent randomly and replace Brain (Later on handle persistent pool learning -- for now, just controls when Brain updates )
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        //playerAgent.InitializeAgentFromGenome(playerGenome, playerStartPosGenome);

        if(isTrainingPersistent) {
            // Handle Reproduction of persistent Agents Here

            // Measure fitness of all current agents (their genomes, actually)
            for(int i = 0; i < numAgents; i++) {
                rawFitnessScoresArrayPersistent[i] = (float)agentsArray[i].ageCounter;
            }
            //float totalScore = 0f;
            //for(int i = 0; i < rawFitnessScoresArrayPersistent.Length; i++) {
            //    totalScore += rawFitnessScoresArrayPersistent[i];
            //}

            // Sort Fitness Scores Persistent:
            //rankedIndicesListPersistent = new int[rawFitnessScoresArrayPersistent.Length];
            //rankedFitnessListPersistent = new float[rawFitnessScoresArrayPersistent.Length];

            // populate arrays:
            for (int i = 0; i < rawFitnessScoresArrayPersistent.Length; i++) {
                rankedIndicesListPersistent[i] = i;
                rankedFitnessListPersistent[i] = rawFitnessScoresArrayPersistent[i];
            } // Sort By Fitness
            for (int i = 0; i < rawFitnessScoresArrayPersistent.Length - 1; i++) {
                for (int j = 0; j < rawFitnessScoresArrayPersistent.Length - 1; j++) {
                    float swapFitA = rankedFitnessListPersistent[j];
                    float swapFitB = rankedFitnessListPersistent[j + 1];
                    int swapIdA = rankedIndicesListPersistent[j];
                    int swapIdB = rankedIndicesListPersistent[j + 1];

                    if (swapFitA < swapFitB) {  // bigger is better now after inversion
                        rankedFitnessListPersistent[j] = swapFitB;
                        rankedFitnessListPersistent[j + 1] = swapFitA;
                        rankedIndicesListPersistent[j] = swapIdB;
                        rankedIndicesListPersistent[j + 1] = swapIdA;
                    }
                }
            }
            //string txt = "PersistentAges:\n";
            //for (int i = 0; i < rankedFitnessList.Length; i++) {
            //    txt += i.ToString() + ": " + rankedFitnessList[i].ToString() + "\n";
            //}
            //Debug.Log(txt);

            // Randomly select a good one based on fitness Lottery (oldest = best)
            if (rankedIndicesListPersistent[0] == agentIndex) {  // if Top Agent, just respawn identical copy:
                //BrainGenome parentGenome = supervisedGenomePoolArray[rankedIndicesList[x]].brainGenome;  // keep top performer as-is
                //newGenBrainGenomeList.Add(parentGenome);
                // Essentially do nothing and it should just respawn this Agent???

                // Set as Player's dummy Genome??                
            }
            else {
                BrainGenome newBrainGenome = new BrainGenome();                

                int parentIndexPersistent = GetAgentIndexByLottery(rankedFitnessListPersistent, rankedIndicesListPersistent);
                int parentIndex = parentIndexPersistent;
                BrainGenome parentGenome = persistentGenomePoolArray[parentIndexPersistent].brainGenome;
                if (isTrainingSupervised) {  // if both training methods are Active:
                    // find a parent from supervised Pool:
                    int parentIndexSupervised = GetAgentIndexByLottery(prevGenSupervisedRankedFitnessList, prevGenSupervisedRankedIndicesList);
                    
                    if(UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                        parentIndex = parentIndexSupervised;
                        parentGenome = prevGenSupervisedGenomePoolArray[parentIndex].brainGenome;
                    }
                }
                
                // Create duplicate Genome
                newBrainGenome.SetToMutatedCopyOfParentGenome(parentGenome, settingsManager.mutationSettingsPersistent);
                
                persistentGenomePoolArray[agentIndex].brainGenome = newBrainGenome; // update genome to new one

                numPersistentAgentsBorn++;
                currentOldestAgent = agentsArray[rankedIndicesListPersistent[0]].ageCounter;
                //Debug.Log("Agent[" + agentIndex.ToString() + "] replaced by child from Agent: " + parentIndex.ToString());
            }            
            
            // Reset
            // Spawn that genome in dead Agent's body and revive it!
            agentsArray[agentIndex].InitializeAgentFromGenome(persistentGenomePoolArray[agentIndex], startPosGenome);
        }
        else {
            // Needs safeguards for fewer agents than genomes:
            agentsArray[agentIndex].InitializeAgentFromGenome(supervisedGenomePoolArray[agentIndex], startPosGenome);

            int oldestAge = 0;
            for(int a = 0; a < agentsArray.Length; a++) {
                if(agentsArray[a].ageCounter > oldestAge) {
                    oldestAge = agentsArray[a].ageCounter;
                }
            }
            currentOldestAgent = oldestAge;
        }
        //currentOldestAgent = agentsArray[rankedIndicesList[0]].ageCounter;
    }
    public void ProcessDeadPlayer() {
        if (playerAgent.ageCounter > recordPlayerAge) {
            recordPlayerAge = playerAgent.ageCounter;
        }
        // Respawn Agent randomly and replace Brain (Later on handle persistent pool learning -- for now, just controls when Brain updates )
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        playerAgent.InitializeAgentFromGenome(persistentGenomePoolArray[rankedIndicesListPersistent[0]], startPosGenome);
        //playerAgent.ReplaceBrain(persistentGenomePoolArray[rankedIndicesListPersistent[0]]);
        playerAgent.testModule.foodAmountB[0] = 20f;
    }

    private void SpawnPredators() {
        Debug.Log("SpawnPredators!");
        for (int i = 0; i < predatorArray.Length; i++) {
            GameObject predatorGO = Instantiate(Resources.Load("PredatorPrefab")) as GameObject;
            predatorGO.name = "Predator" + i.ToString();
            PredatorModule newPredator = predatorGO.GetComponent<PredatorModule>();
            predatorArray[i] = newPredator; // Add to stored list of current Agents

            RevivePredator(i);
        }
    }
    private void SpawnFood() {
        Debug.Log("SpawnFood!");
        for (int i = 0; i < foodArray.Length; i++) {
            GameObject foodGO = Instantiate(Resources.Load("FoodPrefab")) as GameObject;
            foodGO.name = "Food" + i.ToString();
            FoodModule newFood = foodGO.GetComponent<FoodModule>();
            foodArray[i] = newFood; // Add to stored list of current Agents
            ReviveFood(i);

            Material mat = new Material(fluidColliderMatTemplate);
            foodMaterialsArray[i] = mat;
            newFood.meshRendererFluidCollider.material = mat;
            //newFood.material = mat; // redundant??

            //Texture2D tex = new Texture2D(3, 1);  // foodAmountRGB
            //foodDebugTexturesArray[i] = tex;
            //newFood.texture = tex; // just sets floats parameters now

            //newFood.material.SetTexture("_MainTex", tex);
        }
    }

    private void HookUpModules() {
        PopulateGridCells();

        // Find NearestNeighbors:
        float mapSize = 160f;
        float cellSize = mapSize / agentGridCellResolution;
        Vector2 playerPos = new Vector2(playerAgent.transform.localPosition.x, playerAgent.transform.localPosition.y);

        
        for (int a = 0; a < agentsArray.Length; a++) {
            // Find which gridCell this Agent is in:    
            Vector2 agentPos = new Vector2(agentsArray[a].transform.localPosition.x, agentsArray[a].transform.localPosition.y);
            int xCoord = Mathf.FloorToInt((agentPos.x + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((agentPos.y + mapSize / 2f) / mapSize * (float)agentGridCellResolution);

            int closestFriendIndex = a;  // default to self
            float nearestFriendSquaredDistance = float.PositiveInfinity;
            int closestFoodIndex = 0; // default to 0???
            float nearestFoodDistance = float.PositiveInfinity;
            int closestPredIndex = 0; // default to 0???
            float nearestPredDistance = float.PositiveInfinity;
            // Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].friendIndicesList.Count; i++) {
                // FRIEND:
                Vector2 neighborPos = new Vector2(agentsArray[mapGridCellArray[xCoord][yCoord].friendIndicesList[i]].transform.localPosition.x, agentsArray[mapGridCellArray[xCoord][yCoord].friendIndicesList[i]].transform.localPosition.y);
                float squaredDistFriend = (neighborPos - agentPos).sqrMagnitude;
                               
                if (squaredDistFriend <= nearestFriendSquaredDistance) { // if now the closest so far, update index and dist:
                    if(a != mapGridCellArray[xCoord][yCoord].friendIndicesList[i]) {  // make sure it doesn't consider itself:
                        closestFriendIndex = mapGridCellArray[xCoord][yCoord].friendIndicesList[i];
                        nearestFriendSquaredDistance = squaredDistFriend;
                    }                    
                }
            }
            
            for (int i = 0; i < mapGridCellArray[xCoord][yCoord].foodIndicesList.Count; i++) {
                // FOOD:
                Vector2 foodPos = new Vector2(foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].transform.localPosition.x, foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].transform.localPosition.y);
                float distFood = (foodPos - agentPos).magnitude - (foodArray[mapGridCellArray[xCoord][yCoord].foodIndicesList[i]].curScale + 1f) * 0.5f;  // subtract food & agent radii
                if (distFood <= nearestFoodDistance) { // if now the closest so far, update index and dist:
                    if (a != mapGridCellArray[xCoord][yCoord].foodIndicesList[i]) {  // make sure it doesn't consider itself:
                        closestFoodIndex = mapGridCellArray[xCoord][yCoord].foodIndicesList[i];
                        nearestFoodDistance = distFood;
                    }
                }
            }

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

            //Debug.Log("closestNeighborIndex: " + closestNeighborIndex.ToString());
            agentsArray[a].testModule.friendTestModule = agentsArray[closestFriendIndex].testModule;
            agentsArray[a].testModule.nearestFoodModule = foodArray[closestFoodIndex];
            agentsArray[a].testModule.nearestPredatorModule = predatorArray[closestPredIndex];

            // Compare to Player also:
            float squaredPlayerDistFriend = (playerPos - agentPos).sqrMagnitude;
            // HumanControlLerp:
            float maxControlSqrDist = 400f;
            float lerpVal = (maxControlSqrDist - Mathf.Clamp(squaredPlayerDistFriend, 0f, maxControlSqrDist)) / maxControlSqrDist;
            agentsArray[a].humanControlLerp = 0f; // lerpVal * 0.6f;
            if (squaredPlayerDistFriend <= nearestFriendSquaredDistance) {
                agentsArray[a].testModule.friendTestModule = playerAgent.testModule;
            }
        }

        // Update PlayerAgent's module:
        // Find which gridCell this Agent is in:
        int xIndex = Mathf.FloorToInt((playerPos.x + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
        int yIndex = Mathf.FloorToInt((playerPos.y + mapSize / 2f) / mapSize * (float)agentGridCellResolution);

        int closestIndexFriend = 0;  // default to 0?
        float nearestDistFriend = float.PositiveInfinity;
        int closestIndexFood = 0;  // default to 0?
        float nearestDistFood = float.PositiveInfinity;
        int closestIndexPred = 0;  // default to 0?
        float nearestDistPred = float.PositiveInfinity;
        // Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
        for (int i = 0; i < mapGridCellArray[xIndex][yIndex].friendIndicesList.Count; i++) {
            // FRIENDS::::
            Vector2 neighborPosFriend = new Vector2(agentsArray[mapGridCellArray[xIndex][yIndex].friendIndicesList[i]].transform.localPosition.x, agentsArray[mapGridCellArray[xIndex][yIndex].friendIndicesList[i]].transform.localPosition.y);
            float squaredDistFriend = (neighborPosFriend - playerPos).sqrMagnitude;
            if (squaredDistFriend <= nearestDistFriend) { // if now the closest so far, update index and dist:if (a != mapGridCellArray[xCoord][yCoord].agentIndicesList[i]) {  // make sure it doesn't consider itself:
                closestIndexFriend = mapGridCellArray[xIndex][yIndex].friendIndicesList[i];
                nearestDistFriend = squaredDistFriend;                
            }
        }
        for (int i = 0; i < mapGridCellArray[xIndex][yIndex].foodIndicesList.Count; i++) {
            // FOOD::::
            Vector2 neighborPosFood = new Vector2(foodArray[mapGridCellArray[xIndex][yIndex].foodIndicesList[i]].transform.localPosition.x, foodArray[mapGridCellArray[xIndex][yIndex].foodIndicesList[i]].transform.localPosition.y);
            float squaredDistFood = (neighborPosFood - playerPos).magnitude - (foodArray[mapGridCellArray[xIndex][yIndex].foodIndicesList[i]].curScale + 1f) * 0.5f;  // subtract food & agent radii;
            if (squaredDistFood <= nearestDistFood) { // if now the closest so far, update index and dist:if (a != mapGridCellArray[xCoord][yCoord].agentIndicesList[i]) {  // make sure it doesn't consider itself:
                closestIndexFood = mapGridCellArray[xIndex][yIndex].foodIndicesList[i];
                nearestDistFood = squaredDistFood;
            }
        }
        for (int i = 0; i < mapGridCellArray[xIndex][yIndex].predatorIndicesList.Count; i++) {
            // PREDATORS::::
            Vector2 neighborPosPred = new Vector2(predatorArray[mapGridCellArray[xIndex][yIndex].predatorIndicesList[i]].transform.localPosition.x, predatorArray[mapGridCellArray[xIndex][yIndex].predatorIndicesList[i]].transform.localPosition.y);
            float squaredDistPred = (neighborPosPred - playerPos).magnitude - (predatorArray[mapGridCellArray[xIndex][yIndex].predatorIndicesList[i]].curScale + 1f) * 0.5f;  // subtract pred & agent radii;
            if (squaredDistPred <= nearestDistPred) { // if now the closest so far, update index and dist:if (a != mapGridCellArray[xCoord][yCoord].agentIndicesList[i]) {  // make sure it doesn't consider itself:
                closestIndexPred = mapGridCellArray[xIndex][yIndex].predatorIndicesList[i];
                nearestDistPred = squaredDistPred;
            }
        }
        //Debug.Log("closestNeighborIndex: " + closestNeighborIndex.ToString());
        playerAgent.testModule.friendTestModule = agentsArray[closestIndexFriend].testModule;
        playerAgent.testModule.nearestFoodModule = foodArray[closestIndexFood];
        playerAgent.testModule.nearestPredatorModule = predatorArray[closestIndexPred];

    }
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
    public void Update() {
        cameraManager.targetTransform = playerAgent.transform;
        //Vector3 agentPos = Vector3.zero;
        //if (playerAgent != null) {
        //    agentPos = playerAgent.testModule.ownRigidBody2D.position;
        //}
        //Debug.Log("Update() AgentPos: (" + agentPos.x.ToString() + ", " + agentPos.y.ToString() + ") Time: " + Time.realtimeSinceStartup.ToString());
    }
    public void TickSimulation() {
        //StartCoroutine(TestYieldFixedUpdate());
        //StartCoroutine(TestYieldEndOfFrame());
        //TestExecutionOrder();
        environmentFluidManager.Run();

        HookUpModules(); // Sets nearest-neighbors etc.        

        if (playerAgent.ageCounter > recordPlayerAge) {
            recordPlayerAge = playerAgent.ageCounter;
        }
        uiManager.UpdateScoreText(playerAgent.ageCounter);

        playerAgent.Tick();
        for (int i = 0; i < agentsArray.Length; i++) {
            agentsArray[i].Tick();
        }

        // TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP
        Vector3 debugPos = playerAgent.testModule.ownRigidBody2D.position;
        Vector2 playerUV = new Vector2((debugPos.x + 70f) / 140f, (debugPos.y + 70f) / 140f);
        Vector2 fluidVel = environmentFluidManager.GetFluidVelocityAtPosition(playerUV);
        playerAgent.testModule.ownRigidBody2D.AddForce(fluidVel * 42f, ForceMode2D.Impulse);
        //playerAgent.testModule.foodAmountB[0] += 0.0025f;
        // TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP

        //cameraManager.targetTransform = playerAgent.transform;

        bool recordData = false;
        float curHorizontalInput = 0f;
        float curVerticalInput = 0f;
        if (Input.GetKey("left") || Input.GetKey("a")) {
            curHorizontalInput += -1f;
        }
        if (Input.GetKey("right") || Input.GetKey("d")) {
            curHorizontalInput += 1f;
        }
        if (Input.GetKey("down") || Input.GetKey("s")) {
            curVerticalInput += -1f;
        }
        if (Input.GetKey("up") || Input.GetKey("w")) {
            curVerticalInput += 1f;
        }
        if (curHorizontalInput != lastHorizontalInput || curVerticalInput != lastVerticalInput) {
            recordData = true; // RecordPlayerData();
        }
        //if(timeStepCounter % periodicSamplingRate == 0) {
        //    recordData = true;
        //}
        if(recording == false) {
            recordData = false;
        }
        if(recordData) {
            RecordPlayerData();
        }
        lastHorizontalInput = curHorizontalInput;
        lastVerticalInput = curVerticalInput;

        timeStepCounter++;

        if(curHorizontalInput == 0f && curVerticalInput == 0f) {
            idleFramesCounter++;
        }
        else {
            idleFramesCounter = 0;
            playerAgent.humanControlLerp = 1f;
            playerAgent.meshRendererBeauty.material.SetFloat("_IsPlayer", 1.0f);
        }
        if(idleFramesCounter >= idleFramesToBotControl) {
            // Transition eventually!
            //playerAgent.humanControlLerp = 0f;
            //playerAgent.meshRendererBeauty.material.SetFloat("_IsPlayer", 0.0f);
        }
        //public int idleFramesToBotControl = 100;
        //private int idleFramesCounter = 0;
        //public int botToHumanControlTransitionFrameCount = 8;
        //public int humanToBotControlTransitionFrameCount = 30;
        //TestExecutionOrder();

        
    }
    public void TickTrainingMode() {
        // Can do one genome & 1 dataSample per frame
        if (curTestingGenomeSupervised < supervisedGenomePoolArray.Length) {
            if (curTestingSample < currentDataBatch.Count) {
                // Reset Brain State!!!
                //dummyAgent.ResetBrainState();
                CopyDataSampleToModule(currentDataBatch[curTestingSample], dummyAgent.testModule);  // load training data into module (and by extension, brain)
                // Run Brain ( few ticks? )
                for (int t = 0; t < 3; t++) {
                    dummyAgent.TickBrain();
                }
                //Debug.Log(CompareDataSampleToBrainOutput(dataSamplesList[curTestingSample], dummyAgent.testModule).ToString());
                rawFitnessScoresArraySupervised[curTestingGenomeSupervised] += CompareDataSampleToBrainOutput(currentDataBatch[curTestingSample], dummyAgent.testModule);

                // Hacky way to check if blank brains are scoring too well:
                if(curTestingGenomeSupervised == 0) {  // do Blank brain parallel to agent[0]
                    rawFitnessScoreBlankAgentSupervised += CompareDataSampleToBrainOutput(currentDataBatch[curTestingSample], playerAgent.testModule);
                }

                curTestingSample++;
            }
            else {
                curTestingSample = 0;

                curTestingGenomeSupervised++;
                if (curTestingGenomeSupervised < supervisedGenomePoolArray.Length) { // this can clearly be done better
                    dummyAgent.InitializeAgentFromGenome(supervisedGenomePoolArray[curTestingGenomeSupervised], dummyStartGenome);                    
                }
            }
        }
        else {
            // New Generation!!!
            NextGenerationSupervised();
        }


    }

    private void NextGenerationSupervised() {
        //string fitTxt = "Gen " + curGen.ToString() + " Fitness Scores:\n";
        float totalFitness = 0f;
        float minScore = float.PositiveInfinity;
        float maxScore = float.NegativeInfinity;
        for (int f = 0; f < supervisedGenomePoolSize; f++) {
            //fitTxt += f.ToString() + ": " + rawFitnessScoresArraySupervised[f].ToString() + "\n";
            totalFitness += rawFitnessScoresArraySupervised[f];
            minScore = Mathf.Min(minScore, rawFitnessScoresArraySupervised[f]);
            maxScore = Mathf.Max(maxScore, rawFitnessScoresArraySupervised[f]);
        }
        
        avgFitnessLastGenSupervised = totalFitness / (float)supervisedGenomePoolSize;
        //fitTxt += "\nAvgFitnessLastGen: " + avgFitnessLastGenSupervised.ToString() + ", blank: " + lastGenBlankAgentFitnessSupervised.ToString();
        //Debug.Log(fitTxt);
        lastGenBlankAgentFitnessSupervised = rawFitnessScoreBlankAgentSupervised;
        bestFitnessScoreSupervised = minScore;

        // Process Fitness (bigger is better so lottery works)
        float scoreRange = maxScore - minScore;
        if (scoreRange == 0f) {
            scoreRange = 1f;  // avoid divide by zero
        }
        for (int f = 0; f < supervisedGenomePoolSize; f++) {
            rawFitnessScoresArraySupervised[f] = 1f - ((rawFitnessScoresArraySupervised[f] - minScore) / scoreRange); // normalize and invert scores for fitness lottery
        }

        // Sort Fitness Scores
        int[] rankedIndicesList = new int[rawFitnessScoresArraySupervised.Length];
        float[] rankedFitnessList = new float[rawFitnessScoresArraySupervised.Length];

        // populate arrays:
        for (int i = 0; i < rawFitnessScoresArraySupervised.Length; i++) {
            rankedIndicesList[i] = i;
            rankedFitnessList[i] = rawFitnessScoresArraySupervised[i];
        }
        for (int i = 0; i < rawFitnessScoresArraySupervised.Length - 1; i++) {
            for (int j = 0; j < rawFitnessScoresArraySupervised.Length - 1; j++) {
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

        // Save this ranking and genomes to be used periodically by unsupervised learners:
        for(int i = 0; i < rankedIndicesList.Length; i++) {
            prevGenSupervisedRankedIndicesList[i] = rankedIndicesList[i];
            prevGenSupervisedRankedFitnessList[i] = rankedFitnessList[i];
            prevGenSupervisedGenomePoolArray[i] = supervisedGenomePoolArray[i];
            // Better to just keep it ranked???
        }
        //
        //string fitnessRankText = "";
        //for (int i = 0; i < rawFitnessScoresArraySupervised.Length; i++) {
        //    fitnessRankText += "[" + rankedIndicesList[i].ToString() + "]: " + rankedFitnessList[i].ToString() + "\n";
        //}

        // CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER 
        List<BrainGenome> newGenBrainGenomeList = new List<BrainGenome>(); // new population!        
        
        // Keep top-half peformers + mutations:
        for (int x = 0; x < supervisedGenomePoolArray.Length; x++) {
            if (x == 0) {
                BrainGenome parentGenome = supervisedGenomePoolArray[rankedIndicesList[x]].brainGenome;  // keep top performer as-is
                newGenBrainGenomeList.Add(parentGenome);
            }
            else {
                BrainGenome newBrainGenome = new BrainGenome();
                int parentIndex = GetAgentIndexByLottery(rankedFitnessList, rankedIndicesList);
                BrainGenome parentGenome = supervisedGenomePoolArray[parentIndex].brainGenome;

                if(isTrainingPersistent) {
                    // chance to grab parent from other genome Pool (persistent)
                    if(UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                        int parentIndexPersistent = GetAgentIndexByLottery(rankedFitnessListPersistent, rankedIndicesListPersistent);
                        parentGenome = persistentGenomePoolArray[parentIndexPersistent].brainGenome;
                    }
                }
                newBrainGenome.SetToMutatedCopyOfParentGenome(parentGenome, settingsManager.mutationSettingsSupervised);
                newGenBrainGenomeList.Add(newBrainGenome);
            }
        }

        for (int i = 0; i < supervisedGenomePoolArray.Length; i++) {
            supervisedGenomePoolArray[i].brainGenome = newGenBrainGenomeList[i];
        }

        // Reset
        ResetTrainingForNewGenSupervised();

        //mutationSettings.mutationChance *= 0.995f;
        //mutationSettings.mutationStepSize *= 0.995f;
        
        curGen++;

        
        if(avgFitnessLastGenSupervised > supervisedAvgRecordScore) {  // upper bound on score (0 is best possible)
            supervisedAvgRecordScore = avgFitnessLastGenSupervised;
        }
        supervisedScoresList.Add(avgFitnessLastGenSupervised);

        uiManager.fitnessDisplayTexture.Resize(supervisedScoresList.Count, 1);
        for (int t = 0; t < supervisedScoresList.Count; t++) {
            uiManager.fitnessDisplayTexture.SetPixel(t, 1, new Color(supervisedScoresList[t], supervisedScoresList[t], supervisedScoresList[t]));
        }
        uiManager.fitnessDisplayTexture.Apply();
        uiManager.fitnessDisplayMat.SetFloat("_BestScore", supervisedAvgRecordScore);

        // Grid Search:
        if (isGridSearching) {
            UpdateGridSearch(curGen, avgFitnessLastGenSupervised);
        }
    }

    private void UpdateAgentBrains() {
        // UPDATE BRAINS TEST!!!!!
        for (int a = 0; a < agentsArray.Length; a++) {
            agentsArray[a].ReplaceBrain(supervisedGenomePoolArray[a % 32]);
        }
    }

    public int GetAgentIndexByLottery(float[] rankedFitnessList, int[] rankedIndicesList) {
        int selectedIndex = 0;
        // calculate total fitness of all agents
        float totalFitness = 0f;
        for (int i = 0; i < rankedFitnessList.Length; i++) {
            totalFitness += rankedFitnessList[i];
        }
        // generate random lottery value between 0f and totalFitness:
        float lotteryValue = UnityEngine.Random.Range(0f, totalFitness);
        float currentValue = 0f;
        for (int i = 0; i < rankedFitnessList.Length; i++) {
            if (lotteryValue >= currentValue && lotteryValue < (currentValue + rankedFitnessList[i])) {
                // Jackpot!
                selectedIndex = rankedIndicesList[i];
                //Debug.Log("Selected: " + selectedIndex.ToString() + "! (" + i.ToString() + ") fit= " + currentValue.ToString() + "--" + (currentValue + (1f - rankedFitnessList[i])).ToString() + " / " + totalFitness.ToString() + ", lotto# " + lotteryValue.ToString() + ", fit= " + (1f - rankedFitnessList[i]).ToString());
            }
            currentValue += rankedFitnessList[i]; // add this agent's fitness to current value for next check            
        }        

        return selectedIndex;
    }

    private void ResetTrainingForNewGenSupervised() {
        rawFitnessScoresArraySupervised = new float[supervisedGenomePoolSize];
        rawFitnessScoreBlankAgentSupervised = 0f;

        curTestingGenomeSupervised = 0;
        curTestingSample = 0;
        
        dummyAgent.InitializeAgentFromGenome(supervisedGenomePoolArray[0], dummyStartGenome);
        SetUpTrainingDataBatch();
    }
    private void ResetTrainingForPersistent() {
        rawFitnessScoresArrayPersistent = new float[persistentGenomePoolSize];

        curGen = 0;
        curPersistentApproxGen = 0;
        avgFitnessLastGenPersistent = 0f;
        bestFitnessScorePersistent = 0f;
        numPersistentAgentsBorn = 0;
        currentOldestAgent = 0;

        recordPlayerAge = 0;
        recordBotAge = 0;
    
        rollingAverageAgentScore = 0f;
        if (persistentScoresList == null) {
            persistentScoresList = new List<float>();
        }
        else {
            persistentScoresList.Clear();
        }
        persistentAvgRecordScore = 1f;

        // Respawn Agents:
        RespawnAgents();
        for (int i = 0; i < foodArray.Length; i++) {            
            ReviveFood(i);
        }
        for(int i = 0; i < predatorArray.Length; i++) {
            RevivePredator(i);
        }
    }

    public void ToggleRecording() {
        recording = !recording;
    }
    public void ToggleTrainingSupervised() {
        if (firstTimeTrainingSupervised) {
            ResetTrainingForNewGenSupervised();
            firstTimeTrainingSupervised = false;
        }
        isTrainingSupervised = !isTrainingSupervised;
    }
    public void ToggleTrainingPersistent() {
        if (firstTimeTrainingPersistent) {
            //ResetTrainingForNewGen();
            rawFitnessScoresArrayPersistent = new float[persistentGenomePoolSize];
            firstTimeTrainingPersistent = false;
        }
        isTrainingPersistent = !isTrainingPersistent;
    }
    public void ResetGenomes() {
        InitializePopulationGenomes();
        curGen = 0;
        ResetTrainingForNewGenSupervised();
        ResetTrainingForPersistent();
        //UpdateAgentBrains();
    }
    public void ClearTrainingData() {
        trainingRequirementsMetSupervised = false;
        dataSamplesList.Clear();
    }
    public void SaveTrainingData() {
        Debug.Log("SAVE Population!");
        GenePool pool = new GenePool(persistentGenomePoolArray);
        string json = JsonUtility.ToJson(pool);
        Debug.Log(json);
        //Debug.Log(Application.dataPath);
        //string path = Application.dataPath + "/TrainingSaves/" + savename + ".json";
        string path = Application.dataPath + "/TrainingSaves/testSave.json";
        //Debug.Log(Application.persistentDataPath);
        Debug.Log(path);
        System.IO.File.WriteAllText(path, json);
    }
    public void LoadTrainingData() {
        Debug.Log("LOAD Population!");
        //"E:\Unity Projects\GitHub\Evolution\Assets\GridSearchSaves\2018_2_13_12_35\GS_RawScores.json"
        string filePath = Application.dataPath + "/TrainingSaves/testSave.json";
        // Read the json from the file into a string
        string dataAsJson = File.ReadAllText(filePath);
        // Pass the json to JsonUtility, and tell it to create a GameData object from it
        GenePool loadedData = JsonUtility.FromJson<GenePool>(dataAsJson);
        persistentGenomePoolArray = loadedData.genomeArray;
    }
    public void UpdateDebugUI() {
        string debugTxt = "Training: False";
        if (trainingRequirementsMetSupervised) {
            debugTxt = "Training: ACTIVE   numSamples: " + dataSamplesList.Count.ToString() + "\n";
            debugTxt += "Gen: " + curGen.ToString() + ", Agent: " + curTestingGenomeSupervised.ToString() + ", Sample: " + curTestingSample.ToString() + "\n";
            debugTxt += "Fitness Best: " + bestFitnessScoreSupervised.ToString() + " ( Avg: " + avgFitnessLastGenSupervised.ToString() + " ) Blank: " + lastGenBlankAgentFitnessSupervised.ToString() + "\n";
            debugTxt += "Agent[0] # Neurons: " + agentsArray[0].brain.neuronList.Count.ToString() + ", # Axons: " + agentsArray[0].brain.axonList.Count.ToString() + "\n";
            debugTxt += "CurOldestAge: " + currentOldestAgent.ToString() + ", numChildrenBorn: " + numPersistentAgentsBorn.ToString() + ", ~Gen: " + ((float)numPersistentAgentsBorn / (float)numAgents).ToString();
            debugTxt += "\nBotRecordAge: " + recordBotAge.ToString() + ", PlayerRecordAge: " + recordPlayerAge.ToString();
            debugTxt += "\nAverageAgentScore: " + rollingAverageAgentScore.ToString();
        }
        textDebugTrainingInfo.text = debugTxt;

        if (recording) {
            ColorBlock colorBlock = buttonToggleRecording.colors;
            colorBlock.normalColor = Color.red;
            colorBlock.highlightedColor = Color.red;
            buttonToggleRecording.colors = colorBlock;

            buttonToggleRecording.GetComponentInChildren<Text>().color = Color.white;
            buttonToggleRecording.GetComponentInChildren<Text>().text = "RECORDING";
        }
        else {
            ColorBlock colorBlock = buttonToggleRecording.colors;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;
            buttonToggleRecording.colors = colorBlock;

            buttonToggleRecording.GetComponentInChildren<Text>().color = Color.black;
            buttonToggleRecording.GetComponentInChildren<Text>().text = "OFF";
        }

        if (isTrainingSupervised) {
            buttonToggleTrainingSupervised.GetComponentInChildren<Text>().text = "Supervised\nTraining: ON";
        }
        else {
            buttonToggleTrainingSupervised.GetComponentInChildren<Text>().text = "Supervised\nTraining: OFF";
        }

        if (isTrainingPersistent) {
            buttonToggleTrainingPersistent.GetComponentInChildren<Text>().text = "Persistent\nTraining: ON";
        }
        else {
            buttonToggleTrainingPersistent.GetComponentInChildren<Text>().text = "Persistent\nTraining: OFF";
        }
    }

    public void GenerateDataRepelWalls() {
        Debug.Log("GenerateDataRepelWalls()");
        dataSamplesList = FakeDataGenerator.GenerateDataRepelWalls(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }
    public void GenerateDataRepelPreds() {
        Debug.Log("GenerateDataRepelPreds()");
        dataSamplesList = FakeDataGenerator.GenerateDataRepelPreds(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }
    public void GenerateDataRepelFriends() {
        Debug.Log("GenerateDataRepelFriends()");
        dataSamplesList = FakeDataGenerator.GenerateDataRepelFriends(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }
    public void GenerateDataAttractFriends() {
        Debug.Log("GenerateDataAttractFriends()");
        dataSamplesList = FakeDataGenerator.GenerateDataAttractFriends(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }
    public void GenerateDataAttractFood() {
        Debug.Log("GenerateDataAttractFood()");
        dataSamplesList = FakeDataGenerator.GenerateDataAttractFood(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }
    public void GenerateDataAttractPreds() {
        Debug.Log("GenerateDataAttractPreds()");
        dataSamplesList = FakeDataGenerator.GenerateDataAttractPreds(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }
    public void GenerateDataStandardMix() {
        Debug.Log("GenerateDataStandardMix()");
        dataSamplesList = FakeDataGenerator.GenerateDataStandardMix(64);// new List<DataSample>();
        trainingRequirementsMetSupervised = true;
    }

    private void RecordPlayerData() {
        DataSample sample = playerAgent.RecordData();// new DataSample();
        if (dataSamplesList.Count < maxSavedDataSamples) {
            dataSamplesList.Add(sample);  // Add to master List
        }
        else {
            int randIndex = UnityEngine.Random.Range(0, dataSamplesList.Count - 1);
            dataSamplesList.RemoveAt(randIndex);
            dataSamplesList.Add(sample);  // Add to master List
        }

        if (dataSamplesList.Count >= minDataSamplesForTraining) {
            if (!trainingRequirementsMetSupervised) {
                SetUpTrainingDataBatch();
                trainingRequirementsMetSupervised = true;
            }
        }
        // Make this respect maxDataSamples size and act accordingly
        //sample.Print();
    }
    private void SetUpTrainingDataBatch() {
        if (currentDataBatch == null) {
            currentDataBatch = new List<DataSample>();
        }
        else {
            currentDataBatch.Clear();
        }
        int numSamplesInBatch = (int)Mathf.Min(dataSamplesList.Count, dataBatchSize);

        // Populate dataBatch:
        for (int i = 0; i < numSamplesInBatch; i++) {
            // OLD: RANDOM SAMPLING:
            int randIndex = UnityEngine.Random.Range(0, dataSamplesList.Count - 1);
            DataSample sample = dataSamplesList[randIndex].GetCopy();
            currentDataBatch.Add(sample);

            // TEST: Same As dataPool:
            //DataSample sample = dataSamplesList[i].GetCopy();
            //currentDataBatch.Add(sample);
        }
    }
    private void CopyDataSampleToModule(DataSample sample, TestModule module) {

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
    }
    private float CompareDataSampleToBrainOutput(DataSample sample, TestModule module) {
        float throttleX = Mathf.Round(module.throttleX[0] * 3f / 2f);// + module.throttleX[0];
        float throttleY = Mathf.Round(module.throttleY[0] * 3f / 2f);// + module.throttleY[0];
        float deltaX = sample.outputDataArray[0] - throttleX + (sample.outputDataArray[0] - module.throttleX[0]) * 0.25f;  // Change this later?
        float deltaY = sample.outputDataArray[1] - throttleY + (sample.outputDataArray[1] - module.throttleY[0]) * 0.25f;
        float distSquared = deltaX * deltaX + deltaY * deltaY;
        return distSquared;
    }

    public void RunAutomatedGridSearch() {
        // Runs a bunch of simulations with different settings and saves the results for later analysis
        //gridSearchManager = new GridSearchManager();
        bool newSearch = true;

        if(newSearch) {
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
    }

    private void ResetSupervisedTraining() {  // full restart used for GridSearch Runs:
        if (supervisedScoresList == null) {
            supervisedScoresList = new List<float>();
        }
        else {
            supervisedScoresList.Clear();
        }
        supervisedAvgRecordScore = 1f;
    }

    public void UpdateGridSearch(int curGen, float score) {
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
            if(isTrainingSupervised) {
                ResetSupervisedTraining();
            }
        }
        else {
            // Save data
            //gridSearchManager.DataEntry(curGen, score);
        }
        if(gridSearchManager.isComplete) {
            //Debug.Log("UpdateGridSearch(gridSearchManager.isComplete)");
            isGridSearching = false;
        }
    }
}
