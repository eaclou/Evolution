using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

// The meat of the Game, controls the primary simulation/core logic gameplay Loop
public class SimulationManager : MonoBehaviour {
    
    public UIManager uiManager;
    public EnvironmentFluidManager environmentFluidManager;
    public TheRenderKing theRenderKing;    
    public CameraManager cameraManager;
    public SettingsManager settingsManager;

    private bool isLoading = false;
    private bool loadingCompleteAllSystemsGo;
    public bool LoadingCompleteAllSystemsGo
    {
        get
        {
            return loadingCompleteAllSystemsGo;
        }
        set
        {

        }
    }
    
    private float mapSize = 70f;  // This determines scale of environment, size of FluidSim plane!!! Important!
    private int agentGridCellResolution = 1;  // How much to subdivide the map in order to detect nearest-neighbors more efficiently --> to not be O(n^2)
    public MapGridCell[][] mapGridCellArray;

    private int numAgents = 64;
    private int agentGenomePoolSize = 64;  // spawned as Agents that live until they are killed naturally, tested on Fitness Function
    public Agent playerAgent; // Should just be a reference to whichever #Agent that the player is currently controlling
    public Agent[] agentsArray;
    public BodyGenome bodyGenomeTemplate; // .... refactor?
    public AgentGenome[] agentGenomePoolArray;
    public FoodModule[] foodArray;
    private int numFood = 36;
    public PredatorModule[] predatorArray;
    private int numPredators = 12;
   
    public float[] rawFitnessScoresArray;
    private int[] rankedIndicesList;
    private float[] rankedFitnessList;
    public int numAgentsBorn = 0;
    public int currentOldestAgent = 0;
    public int recordPlayerAge = 0;
    public int recordBotAge = 0;
    public float rollingAverageAgentScore = 0f;
    public List<float> fitnessScoresEachGenerationList;
    public float agentAvgRecordScore = 1f;
    public int curApproxGen = 1;

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
        // "Hey, I'm Loading Here!!!"

        // Check if already loading:
        if(isLoading) {
            // loading coroutine already underway.. chill out and relax
        }
        else {
            // Start Loading coroutine!!!!:
            isLoading = true;
            StartCoroutine(LoadingNewSimulation());
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

        // **** How to handle sharing simulation data between different Managers???
        // Once Agents, Food, etc. are established, Initialize the Fluid:
        LoadingInitializeFluidSim();

        yield return null;

        // Wake up the Render King and prepare him for the day ahead, proudly ruling over Renderland.
        LoadingGentlyRouseTheRenderMonarchHisHighnessLordOfPixels();

        yield return null;

        // Hook up Camera to data -- fill out CameraManager class

        // ***** Hook up UI to proper data or find a way to handle that ****
        // possibly just top-down let cameraManager read simulation data

        // Separate class to hold all simulation State Data?

        yield return null;

        // Initialize Agents:
        LoadingInstantiateAgents();  // Fills the AgentsArray, Instantiates Agent Objects (MonoBehaviors + GameObjects)
        LoadingInitializeAgentsFromGenomes(); // This used to be "RespawnAgents()" --  Used to actually place the Agent in the game in a random spot and set the Agent's atributes ffrom its genome
        // Initialize Food:
        LoadingInstantiateFood();
        LoadingInitializeFoodFromGenome();
        // Initialize Predators:
        LoadingInstantiatePredators();
        LoadingInitializePredatorsFromGenome();

        yield return null;

        LoadingInitializeGridCells();
        // Populates GridCells with their contents (agents/food/preds)
        LoadingFillGridCells();
        LoadingHookUpModules();
        

        //elapsedTime = Time.realtimeSinceStartup - startTime;
        //if(elapsedTime > maxComputeTimePerFrame) {
        //    yield return null;
        //}

        //yield return new WaitForSeconds(5f); // TEMP!!!

        // Done - will be detected by GameManager next frame
        loadingCompleteAllSystemsGo = true;

        Debug.Log("LOADING COMPLETE");
    }

    private void LoadingInitializeCoreSimulationState() {
        // allocate memory and initialize data structures, classes, arrays, etc.

        settingsManager.Initialize();
        
        bodyGenomeTemplate = new BodyGenome();
        bodyGenomeTemplate.InitializeGenomeAsDefault(); // ****  Come back to this and make sure using bodyGenome in a good way

        LoadingInitializePopulationGenomes();

    }
    private void LoadingInitializePopulationGenomes() {
        agentGenomePoolArray = new AgentGenome[agentGenomePoolSize];

        for (int i = 0; i < agentGenomePoolArray.Length; i++) {   // Create initial Population Supervised Learners
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
            agentGenome.InitializeRandomBrainFromCurrentBody(settingsManager.mutationSettingsPersistent.initialConnectionChance);
            agentGenomePoolArray[i] = agentGenome;
        }

        // Sort Fitness Scores Persistent:
        rankedIndicesList = new int[agentGenomePoolSize];
        rankedFitnessList = new float[agentGenomePoolSize];

        for (int i = 0; i < agentGenomePoolSize; i++) {
            rankedIndicesList[i] = i;
            rankedFitnessList[i] = 1f;
        }
    }
    private void LoadingInitializeFluidSim() {

    }  // ***** ACTUALLY WRITE THIS!!
    private void LoadingGentlyRouseTheRenderMonarchHisHighnessLordOfPixels() {

    } // ***** ACTUALLY WRITE THIS!!
    private void LoadingInstantiateAgents() {

        string assetURL = "AgentPrefab";

        // Instantiate AI Agents
        agentsArray = new Agent[numAgents];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
            agentGO.name = "Agent" + i.ToString();
            float initScale = UnityEngine.Random.Range(1f, 1f); // **** Replace this with data inside bodyGenome!!!
            agentGO.transform.localScale = new Vector3(initScale, initScale, initScale);
            Agent newAgent = agentGO.GetComponent<Agent>();  // Script placed on Prefab already            
            agentsArray[i] = newAgent; // Add to stored list of current Agents
            
            // Set Material? ... Just pass data to The Render King and let him use his DrawProcedural ability to render in one pass
            
            // Find better way to pass data about Agent's brain/body state to His Majesty.
            
            // ****** :::::::: v v v MOVE INTO BODYGENOME DOMAIN!!!!!!
            //  APPEARANCE:::::
            /*newAgent.huePrimary = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);
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
            }*/
        }
    }
    private void LoadingInitializeAgentsFromGenomes() {
        
        for (int i = 0; i < agentGenomePoolSize; i++) {
            // Revisit how start positions are set up? Probably don't need to be part of the genome....
            Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
            StartPositionGenome agentStartPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
            agentsArray[i].InitializeAgentFromGenome(agentGenomePoolArray[i], agentStartPosGenome);
        }
    }
    private void LoadingInstantiateFood() {
        // FOOODDDD!!!!
        foodArray = new FoodModule[numFood]; // create array

        Debug.Log("SpawnFood!");
        for (int i = 0; i < foodArray.Length; i++) {
            GameObject foodGO = Instantiate(Resources.Load("FoodPrefab")) as GameObject;
            foodGO.name = "Food" + i.ToString();
            FoodModule newFood = foodGO.GetComponent<FoodModule>();
            foodArray[i] = newFood; // Add to stored list of current Food objects                     
        }
    }
    private void LoadingInitializeFoodFromGenome() {
        for (int i = 0; i < foodArray.Length; i++) {
            ReviveFood(i);
        }
    }
    private void LoadingInstantiatePredators() {
        predatorArray = new PredatorModule[numPredators];
        Debug.Log("SpawnPredators!");
        for (int i = 0; i < predatorArray.Length; i++) {
            GameObject predatorGO = Instantiate(Resources.Load("PredatorPrefab")) as GameObject;
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
    private void LoadingInitializeGridCells() {
        mapGridCellArray = new MapGridCell[agentGridCellResolution][];
        for (int i = 0; i < agentGridCellResolution; i++) {
            mapGridCellArray[i] = new MapGridCell[agentGridCellResolution];
        }

        float cellSize = mapSize / agentGridCellResolution;

        for (int x = 0; x < agentGridCellResolution; x++) {
            for (int y = 0; y < agentGridCellResolution; y++) {
                Vector2 cellTopLeft = new Vector2(cellSize * x, cellSize * y);
                Vector2 cellBottomRight = new Vector2(cellSize * (x + 1), cellSize * (y + 1));
                mapGridCellArray[x][y] = new MapGridCell(cellTopLeft, cellBottomRight);
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
        rawFitnessScoresArray = new float[agentGenomePoolSize];

        fitnessScoresEachGenerationList = new List<float>(); // 
    }

    #endregion

    #region Every Frame  //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& EVERY FRAME &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

    public void TickSimulation() {
        
        environmentFluidManager.Run(); // ** Clean this up, but generally OK
        
        // **** Figure out proper Execution Order / in which Class to Run RenderOps while being synced w/ results of physX sim!!!
        //theRenderKing.velocityTex = environmentFluidManager.velocityA; // *** Better way to share DATA!!!
        //theRenderKing.Tick();

        HookUpModules(); // Sets nearest-neighbors etc.        

        // ******** REVISIT CODE ORDERING!!!!  -- Should check for death Before or After agent Tick/PhysX ???
        CheckForDeadFood();
        CheckForDeadAgents();

        for (int i = 0; i < agentsArray.Length; i++) {
            agentsArray[i].Tick();
        }

        CheckForRecordPlayerScore();      
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

        //float mapSize = 160f;
        //float cellSize = mapSize / agentGridCellResolution;

        // FOOD!!! :::::::
        for (int f = 0; f < foodArray.Length; f++) {
            float xPos = foodArray[f].transform.localPosition.x;
            float yPos = foodArray[f].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);

            mapGridCellArray[xCoord][yCoord].foodIndicesList.Add(f);
        }

        // FRIENDS::::::
        for (int a = 0; a < agentsArray.Length; a++) {
            float xPos = agentsArray[a].transform.localPosition.x;
            float yPos = agentsArray[a].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);

            mapGridCellArray[xCoord][yCoord].friendIndicesList.Add(a);
        }

        // PREDATORS !!! :::::::
        for (int p = 0; p < predatorArray.Length; p++) {
            float xPos = predatorArray[p].transform.localPosition.x;
            float yPos = predatorArray[p].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);

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
            Vector2 agentPos = new Vector2(agentsArray[a].transform.localPosition.x, agentsArray[a].transform.localPosition.y);
            int xCoord = Mathf.FloorToInt((agentPos.x + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((agentPos.y + mapSize) / (mapSize * 2f) * (float)agentGridCellResolution);

            int closestFriendIndex = a;  // default to self
            float nearestFriendSquaredDistance = float.PositiveInfinity;
            int closestFoodIndex = 0; // default to 0???
            float nearestFoodDistance = float.PositiveInfinity;
            int closestPredIndex = 0; // default to 0???
            float nearestPredDistance = float.PositiveInfinity;
            
            // **** Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
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
            // Set proper references between AgentBrains and Environment/Game Objects:::
            agentsArray[a].testModule.friendTestModule = agentsArray[closestFriendIndex].testModule;
            agentsArray[a].testModule.nearestFoodModule = foodArray[closestFoodIndex];
            agentsArray[a].testModule.nearestPredatorModule = predatorArray[closestPredIndex];            
        }
    }
    private void CheckForRecordPlayerScore() {
        // Check for Record Agent AGE!
        if (playerAgent.ageCounter > recordPlayerAge) {
            recordPlayerAge = playerAgent.ageCounter;
        }  
    }
    private void CheckForDeadFood() { // *** revisit
        // CHECK FOR DEAD FOOD!!! :::::::
        for (int f = 0; f < foodArray.Length; f++) {
            if (foodArray[f].isDepleted) {
                ReviveFood(f);
            }
        }
    }
    private void CheckForDeadAgents() { 
        for (int a = 0; a < agentsArray.Length; a++) {
            if (agentsArray[a].isDead) {
                ProcessDeadAgent(a);
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
    public void ReviveFood(int index) {
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-36f, 36f), UnityEngine.Random.Range(-36f, 36f), 0f);
        foodArray[index].transform.localPosition = startPos;
        foodArray[index].Respawn();
    } // *** confirm these are set up alright
    public void RevivePredator(int index) {
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-36f, 36f), UnityEngine.Random.Range(-36f, 36f), 0f);
        predatorArray[index].transform.localPosition = startPos;
    } // *** confirm these are set up alright      
    public void ProcessDeadAgent(int agentIndex) {

        CheckForRecordAgentScore(agentIndex);
        ProcessAgentScores(agentIndex);
        
        // Updates rankedIndicesArray[] so agents are ordered by score:
        ProcessAndRankAgentFitness();

        // Reproduction!!!
        CreateMutatedCopyOfAgent(agentIndex);        
    }
    private void CheckForRecordAgentScore(int agentIndex) {
        if (agentsArray[agentIndex].ageCounter > recordBotAge) {
            recordBotAge = agentsArray[agentIndex].ageCounter;
        }
    }
    private void ProcessAgentScores(int agentIndex) {
        rollingAverageAgentScore = Mathf.Lerp(rollingAverageAgentScore, (float)agentsArray[agentIndex].ageCounter, 1f / 512f);
        float approxGen = (float)numAgentsBorn / (float)agentGenomePoolSize;
        if (approxGen > curApproxGen) {
            fitnessScoresEachGenerationList.Add(rollingAverageAgentScore);
            if (rollingAverageAgentScore > agentAvgRecordScore) {
                agentAvgRecordScore = rollingAverageAgentScore;
            }
            curApproxGen++;
        }
    }
    private void ProcessAndRankAgentFitness() {
        // Measure fitness of all current agents (their genomes, actually)
            for (int i = 0; i < numAgents; i++) {
                rawFitnessScoresArray[i] = (float)agentsArray[i].ageCounter;
            }

            // populate arrays:
            for (int i = 0; i < rawFitnessScoresArray.Length; i++) {
                rankedIndicesList[i] = i;
                rankedFitnessList[i] = rawFitnessScoresArray[i];
            } // Sort By Fitness
            for (int i = 0; i < rawFitnessScoresArray.Length - 1; i++) {
                for (int j = 0; j < rawFitnessScoresArray.Length - 1; j++) {
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
    private void CreateMutatedCopyOfAgent(int agentIndex) {
        // Randomly select a good one based on fitness Lottery (oldest = best)
        if (rankedIndicesList[0] == agentIndex) {  // if Top Agent, just respawn identical copy:
                               
        }
        else {
            BrainGenome newBrainGenome = new BrainGenome();
            
            int parentIndex = GetAgentIndexByLottery(rankedFitnessList, rankedIndicesList);
            BrainGenome parentGenome = agentGenomePoolArray[parentIndex].brainGenome;
                
            // MUTATE BODY:
            // Mutate body here

            // Create duplicate Genome
            newBrainGenome.SetToMutatedCopyOfParentGenome(parentGenome, settingsManager.mutationSettingsPersistent);
            agentGenomePoolArray[agentIndex].brainGenome = newBrainGenome; // update genome to new one

            numAgentsBorn++;
            currentOldestAgent = agentsArray[rankedIndicesList[0]].ageCounter;
        }        
        
        // **** !!! REvisit StartPos!!!
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        agentsArray[agentIndex].InitializeAgentFromGenome(agentGenomePoolArray[agentIndex], startPosGenome); // Spawn that genome in dead Agent's body and revive it!

        //theRenderKing.SetSimDataArrays(); // find better way to sync data!! ****
        //theRenderKing.SetPointStrokesBuffer();
        //theRenderKing.InitializeAgentCurveData(agentIndex);
    }
    public void ProcessDeadPlayer() {
        
        /*if (playerAgent.ageCounter > recordPlayerAge) {
            recordPlayerAge = playerAgent.ageCounter;
        }
        // Respawn Agent randomly and replace Brain (Later on handle persistent pool learning -- for now, just controls when Brain updates )
        Vector3 startPos = new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-30f, 30f), 0f);
        StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
        playerAgent.InitializeAgentFromGenome(agentGenomePoolArray[rankedIndicesList[0]], startPosGenome);
        //playerAgent.ReplaceBrain(persistentGenomePoolArray[rankedIndicesListPersistent[0]]);
        playerAgent.testModule.foodAmountB[0] = 20f;

        theRenderKing.SetSimDataArrays();
        theRenderKing.InitializeAgentCurveData(agentsArray.Length);
        */
    }  // **** NEEDS MODERNIZATION!
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

    #endregion
    
    public void SaveTrainingData() {
        /*
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
        */
    }
    public void LoadTrainingData() {
        /*
        Debug.Log("LOAD Population!");
        //"E:\Unity Projects\GitHub\Evolution\Assets\GridSearchSaves\2018_2_13_12_35\GS_RawScores.json"
        string filePath = Application.dataPath + "/TrainingSaves/testSave.json";
        // Read the json from the file into a string
        string dataAsJson = File.ReadAllText(filePath);
        // Pass the json to JsonUtility, and tell it to create a GameData object from it
        GenePool loadedData = JsonUtility.FromJson<GenePool>(dataAsJson);
        persistentGenomePoolArray = loadedData.genomeArray;
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
