using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour {

    public Text textDebugTrainingInfo;
    public Button buttonToggleRecording;
    public Button buttonToggleTraining;
    public Button buttonResetGenomes;
    public Button buttonClearTrainingData;

    public Camera mainCam;
    private int populationSize = 64;
    public Material playerMat;
    public Agent playerAgent;
    public Agent[] agentsArray;

    public BodyGenome bodyGenomeTemplate;
    public AgentGenome[] genomePoolArray;

    public bool recording = false;
    public List<DataSample> dataSamplesList;  // master pool
    public List<DataSample> currentDataBatch;
    private int maxSavedDataSamples = 256;  // dataSamples above this number replace existing samples randomly
    private int dataBatchSize = 96;
    private int minDataSamplesForTraining = 4;  // won't start training brains until DataList has at least this many samples
    private int periodicSamplingRate = 64;  // saves a sample of player's data every (this #) frames
    public MutationSettings mutationSettings;
    public float[] rawFitnessScoresArray;
    public bool trainingRequirementsMet = false;  // minimum reqs met
    public bool isTraining = false;  // player control
    private float lastHorizontalInput = 0f;
    private float lastVerticalInput = 0f;
    private int timeStepCounter = 1;
    public int curGen;
    public float avgFitnessLastGen = 0f;
    public float bestFitnessScore = 0f;
    public int curTestingAgent = 0;
    public int curTestingSample = 0;
    private Agent dummyAgent;
    private StartPositionGenome dummyStartGenome;

    public float rawFitnessScoreBlankAgent = 0f;
    public float lastGenBlankAgentFitness = 0f;

    private int agentGridCellResolution = 4;  // How much to subdivide the map in order to detect nearest-neighbors more efficiently --> to not be O(n^2)
    public MapGridCell[][] mapGridCellArray;

    // need to be able to update Agent's Brain on the fly?  --- but needs to access the Module to set up inputs/outputs???
    // Ability to run a Brain Headless (without instantiating an Agent?)
    
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
    public void PopulateGridCells() {
        // Inefficient!!!
        for (int x = 0; x < agentGridCellResolution; x++) {
            for (int y = 0; y < agentGridCellResolution; y++) {                
                mapGridCellArray[x][y].agentIndicesList.Clear();
            }
        }

        float mapSize = 80f;
        float cellSize = mapSize / agentGridCellResolution;

        for (int a = 0; a < agentsArray.Length; a++) {
            float xPos = agentsArray[a].transform.localPosition.x;
            float yPos = agentsArray[a].transform.localPosition.y;
            int xCoord = Mathf.FloorToInt((xPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((yPos + mapSize / 2f) / mapSize * (float)agentGridCellResolution);

            //Debug.Log("PopulateGrid(" + a.ToString() + ") [" + xCoord.ToString() + "," + yCoord.ToString() + "]");
            mapGridCellArray[xCoord][yCoord].agentIndicesList.Add(a);
        }
    }
    public void ToggleRecording() {
        recording = !recording;
    }
    public void ToggleTraining() {
        isTraining = !isTraining;
    }
    public void ResetGenomes() {
        InitializePopulationGenomes();
        curGen = 0;
        ResetTrainingForNewGen();
        UpdateAgentBrains();
    }
    public void ClearTrainingData() {
        trainingRequirementsMet = false;
        dataSamplesList.Clear();
    }
    public void UpdateDebugUI() {
        string debugTxt = "Training: False";
        if(trainingRequirementsMet) {
            debugTxt = "Training: ACTIVE   numSamples: " + dataSamplesList.Count.ToString() + "\n";
            debugTxt += "Gen: " + curGen.ToString() + ", Agent: " + curTestingAgent.ToString() + ", Sample: " + curTestingSample.ToString() + "\n";
            debugTxt += "Fitness Best: " + bestFitnessScore.ToString() + " ( Avg: " + avgFitnessLastGen.ToString() + " ) Blank: " + lastGenBlankAgentFitness.ToString() + "\n";
            debugTxt += "Agent[0] # Neurons: " + agentsArray[0].brain.neuronList.Count.ToString() + ", # Axons: " + agentsArray[0].brain.axonList.Count.ToString() + "\n";

        }
        textDebugTrainingInfo.text = debugTxt;

        if(recording) {
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

        if(isTraining) {
            buttonToggleTraining.GetComponentInChildren<Text>().text = "Training: ON";
        }
        else {
            buttonToggleTraining.GetComponentInChildren<Text>().text = "Training: OFF";
        }
    }
    public void InitializeTrainingApparatus() {
        mutationSettings = new MutationSettings(0.04f, 0.75f, 0.025f, 0.002f);
        GameObject dummyAgentGO = new GameObject("DummyAgent");
        dummyAgent = dummyAgentGO.AddComponent<Agent>();
        dummyStartGenome = new StartPositionGenome(Vector3.zero, Quaternion.identity);

        dataSamplesList = new List<DataSample>();

        //CreateFakeData();

        ResetTrainingForNewGen();
    }
    private void RecordPlayerData() {        
        DataSample sample = playerAgent.RecordData();// new DataSample();
        if(dataSamplesList.Count < maxSavedDataSamples) {
            dataSamplesList.Add(sample);  // Add to master List
        }
        else {
            int randIndex = UnityEngine.Random.Range(0, dataSamplesList.Count - 1);
            dataSamplesList.RemoveAt(randIndex);
            dataSamplesList.Add(sample);  // Add to master List
        }        

        if(dataSamplesList.Count >= minDataSamplesForTraining) {
            if(!trainingRequirementsMet) {
                SetUpTrainingDataBatch();
                trainingRequirementsMet = true;
            }            
        }
        // Make this respect maxDataSamples size and act accordingly
        //sample.Print();
    }
    private void SetUpTrainingDataBatch() {
        if(currentDataBatch == null) {
            currentDataBatch = new List<DataSample>();
        }
        else {
            currentDataBatch.Clear();
        }
        int numSamplesInBatch = (int)Mathf.Min(dataSamplesList.Count, dataBatchSize);

        // Populate dataBatch:
        for(int i = 0; i < numSamplesInBatch; i++) {
            // OLD: RANDOM SAMPLING:
            //int randIndex = UnityEngine.Random.Range(0, dataSamplesList.Count - 1);
            //DataSample sample = dataSamplesList[randIndex].GetCopy();
            //currentDataBatch.Add(sample);

        // TEST: Same As dataPool:
            //int randIndex = UnityEngine.Random.Range(0, dataSamplesList.Count - 1);
            DataSample sample = dataSamplesList[i].GetCopy();
            currentDataBatch.Add(sample);
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
    private void InitializePopulationGenomes() {
        for (int i = 0; i < genomePoolArray.Length; i++) {   // Create initial Population
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
            agentGenome.InitializeRandomBrainFromCurrentBody(0.0f);
            genomePoolArray[i] = agentGenome;
        }
    }
    public void InitializeNewSimulation() {

        // Create Environment (Or have it pre-built)

        // Create initial population of Genomes:
        // Re-Factor:
        bodyGenomeTemplate = new BodyGenome();
        bodyGenomeTemplate.InitializeGenomeAsDefault();
        genomePoolArray = new AgentGenome[populationSize];
        // Player's dummy Genome (required to initialize Agent Class):
        AgentGenome playerGenome = new AgentGenome(-1);
        playerGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
        playerGenome.InitializeRandomBrainFromCurrentBody(0.0f);

        InitializePopulationGenomes();

        // Instantiate Player Agent
        string assetURL = "AgentPrefab";
        GameObject playerAgentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
        playerAgentGO.name = "PlayerAgent";
        playerAgentGO.GetComponent<MeshRenderer>().material = playerMat;
        playerAgentGO.GetComponent<Rigidbody2D>().mass = 10f;
        playerAgentGO.transform.localScale = new Vector3(1.5f, 1.5f, 0.33f);
        //How to handle initial Placement?
        //agentGO.transform.localPosition = currentEvalTicket.environmentGenome.agentStartPositionsList[i].agentStartPosition;
        //.transform.localRotation = currentEvalTicket.environmentGenome.agentStartPositionsList[i].agentStartRotation;
        //agentGO.GetComponent<CircleCollider2D>().enabled = false;
        playerAgent = playerAgentGO.AddComponent<Agent>();
        playerAgent.humanControlled = true;
        playerAgent.speed *= 10f;
        //agentScript.isVisible = visible;
        StartPositionGenome playerStartPosGenome = new StartPositionGenome(Vector3.zero, Quaternion.identity);
        playerAgent.InitializeAgentFromGenome(playerGenome, playerStartPosGenome);
        //currentAgentsArray[i] = agentScript;


        // Instantiate AI Agents
        agentsArray = new Agent[populationSize];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
            agentGO.name = "Agent" + i.ToString();
            Agent newAgent = agentGO.AddComponent<Agent>();
            int numColumns = Mathf.RoundToInt(Mathf.Sqrt(populationSize));
            int row = Mathf.FloorToInt(i / numColumns);
            int col = i % numColumns;
            StartPositionGenome agentStartPosGenome = new StartPositionGenome(new Vector3(1.25f * (col - numColumns / 2), -2.0f - (1.25f * row), 0f), Quaternion.identity);
            agentsArray[i] = newAgent; // Add to stored list of current Agents
            newAgent.InitializeAgentFromGenome(genomePoolArray[i], agentStartPosGenome);
        }

        //Hookup Agent Modules to their proper Objects/Transforms/Info
        //playerAgent.testModule.enemyTestModule = agentsArray[0].testModule;
        for (int p = 0; p < agentsArray.Length; p++) {
            //agentsArray[p].testModule.enemyTestModule = playerAgent.testModule;
            //for (int e = 0; e < currentAgentsArray.Length; e++) {
            //if (e != p) {  // not vs self:
                    //currentAgentsArray[p].testModule.ownRigidBody2D = currentAgentsArray[p].GetComponent<Rigidbody2D>();
                    //currentAgentsArray[p].testModule.enemyTestModule = currentAgentsArray[e].testModule;
                    //currentAgentsArray[p].testModule.enemyPosX[0] = currentAgentsArray[e].testModule.posX[0] - currentAgentsArray[p].testModule.posX[0];
                    //currentAgentsArray[p].testModule.enemyPosY[0] = currentAgentsArray[e].testModule.posY[0] - currentAgentsArray[p].testModule.posY[0];
                //}
            //}
        }

        InitializeGridCells();
        HookUpModules();
        InitializeTrainingApparatus();
    }

    private void HookUpModules() {
        PopulateGridCells();

        // Find NearestNeighbors:
        float mapSize = 80f;
        float cellSize = mapSize / agentGridCellResolution;
        Vector2 playerPos = new Vector2(playerAgent.transform.localPosition.x, playerAgent.transform.localPosition.y);

        for (int a = 0; a < agentsArray.Length; a++) {
            // Find which gridCell this Agent is in:    
            Vector2 agentPos = new Vector2(agentsArray[a].transform.localPosition.x, agentsArray[a].transform.localPosition.y);
            int xCoord = Mathf.FloorToInt((agentPos.x + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
            int yCoord = Mathf.FloorToInt((agentPos.y + mapSize / 2f) / mapSize * (float)agentGridCellResolution);

            int closestNeighborIndex = a;  // default to self
            float nearestSquaredDistance = float.PositiveInfinity;
            // Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
            for(int i = 0; i < mapGridCellArray[xCoord][yCoord].agentIndicesList.Count; i++) {
                Vector2 neighborPos = new Vector2(agentsArray[mapGridCellArray[xCoord][yCoord].agentIndicesList[i]].transform.localPosition.x, agentsArray[mapGridCellArray[xCoord][yCoord].agentIndicesList[i]].transform.localPosition.y);
                float squaredDist = (neighborPos - agentPos).sqrMagnitude;

                if(squaredDist <= nearestSquaredDistance) { // if now the closest so far, update index and dist:
                    if(a != mapGridCellArray[xCoord][yCoord].agentIndicesList[i]) {  // make sure it doesn't consider itself:
                        closestNeighborIndex = mapGridCellArray[xCoord][yCoord].agentIndicesList[i];
                        nearestSquaredDistance = squaredDist;
                    }                    
                }
            }            

            //Debug.Log("closestNeighborIndex: " + closestNeighborIndex.ToString());
            agentsArray[a].testModule.enemyTestModule = agentsArray[closestNeighborIndex].testModule;

            // Compare to Player also:
            
            float squaredPlayerDist = (playerPos - agentPos).sqrMagnitude;
            if (squaredPlayerDist <= nearestSquaredDistance) {
                agentsArray[a].testModule.enemyTestModule = playerAgent.testModule;
            }
        }

        // Update PlayerAgent's module:
        // Find which gridCell this Agent is in:
        int xIndex = Mathf.FloorToInt((playerPos.x + mapSize / 2f) / mapSize * (float)agentGridCellResolution);
        int yIndex = Mathf.FloorToInt((playerPos.y + mapSize / 2f) / mapSize * (float)agentGridCellResolution);

        int closestIndex = 0;  // default to 0?
        float nearestDist = float.PositiveInfinity;
        // Only checking its own grid cell!!! Will need to expand to adjacent cells as well!
        for (int i = 0; i < mapGridCellArray[xIndex][yIndex].agentIndicesList.Count; i++) {
            Vector2 neighborPos = new Vector2(agentsArray[mapGridCellArray[xIndex][yIndex].agentIndicesList[i]].transform.localPosition.x, agentsArray[mapGridCellArray[xIndex][yIndex].agentIndicesList[i]].transform.localPosition.y);
            float squaredDist = (neighborPos - playerPos).sqrMagnitude;
            if (squaredDist <= nearestDist) { // if now the closest so far, update index and dist:if (a != mapGridCellArray[xCoord][yCoord].agentIndicesList[i]) {  // make sure it doesn't consider itself:
                closestIndex = mapGridCellArray[xIndex][yIndex].agentIndicesList[i];
                nearestDist = squaredDist;                
            }
        }
        //Debug.Log("closestNeighborIndex: " + closestNeighborIndex.ToString());
        playerAgent.testModule.enemyTestModule = agentsArray[closestIndex].testModule;

    }
    public void TickSimulation() {
        HookUpModules(); // Sets nearest-neighbors etc.
        playerAgent.Tick();
        for (int i = 0; i < agentsArray.Length; i++) {
            agentsArray[i].Tick();
        }

        //Vector3 camPos = new Vector3(playerAgent.transform.position.x, playerAgent.transform.position.y, -10f);
        //mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, camPos, 0.045f);

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
    }

    public void TickTrainingMode() {
        // Can do one genome & 1 dataSample per frame
        if (curTestingAgent < genomePoolArray.Length) {
            if (curTestingSample < currentDataBatch.Count) {
                CopyDataSampleToModule(currentDataBatch[curTestingSample], dummyAgent.testModule);  // load training data into module (and by extension, brain)
                // Run Brain ( few ticks? )
                for (int t = 0; t < 3; t++) {
                    dummyAgent.TickBrain();
                }
                //Debug.Log(CompareDataSampleToBrainOutput(dataSamplesList[curTestingSample], dummyAgent.testModule).ToString());
                rawFitnessScoresArray[curTestingAgent] += CompareDataSampleToBrainOutput(currentDataBatch[curTestingSample], dummyAgent.testModule);

                // Hacky way to check if blank brains are scoring too well:
                if(curTestingAgent == 0) {  // do Blank brain parallel to agent[0]
                    rawFitnessScoreBlankAgent += CompareDataSampleToBrainOutput(currentDataBatch[curTestingSample], playerAgent.testModule);
                }

                curTestingSample++;
            }
            else {
                curTestingSample = 0;

                curTestingAgent++;
                if (curTestingAgent < genomePoolArray.Length) { // this can clearly be done better
                    dummyAgent.InitializeAgentFromGenome(genomePoolArray[curTestingAgent], dummyStartGenome);
                }
            }
        }
        else {
            // New Generation!!!
            //
            NextGeneration();

        }

        // Loop through all Genomes in pop
        // Build Brain from genome
        // Test Brain vs all dataSamples
        // keep track of fitness score
        // Sort by fitness
        // Crossover
        // Set new population


    }
    private void NextGeneration() {
        string fitTxt = "Fitness Scores:\n";
        float totalFitness = 0f;
        float minScore = float.PositiveInfinity;
        float maxScore = float.NegativeInfinity;
        for (int f = 0; f < populationSize; f++) {
            fitTxt += f.ToString() + ": " + rawFitnessScoresArray[f].ToString() + "\n";
            totalFitness += rawFitnessScoresArray[f];
            minScore = Mathf.Min(minScore, rawFitnessScoresArray[f]);
            maxScore = Mathf.Max(maxScore, rawFitnessScoresArray[f]);
        }
        Debug.Log(fitTxt);
        avgFitnessLastGen = totalFitness / (float)populationSize;
        lastGenBlankAgentFitness = rawFitnessScoreBlankAgent;
        bestFitnessScore = minScore;

        // Process Fitness (bigger is better so lottery works)
        float scoreRange = maxScore - minScore;
        if (scoreRange == 0f) {
            scoreRange = 1f;  // avoid divide by zero
        }
        for (int f = 0; f < populationSize; f++) {
            rawFitnessScoresArray[f] = 1f - ((rawFitnessScoresArray[f] - minScore) / scoreRange); // normalize and invert scores for fitness lottery
        }

        // Sort Fitness Scores
        int[] rankedIndicesList = new int[rawFitnessScoresArray.Length];
        float[] rankedFitnessList = new float[rawFitnessScoresArray.Length];

        // populate arrays:
        for (int i = 0; i < rawFitnessScoresArray.Length; i++) {
            rankedIndicesList[i] = i;
            rankedFitnessList[i] = rawFitnessScoresArray[i];
        }
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
        string fitnessRankText = "";
        for (int i = 0; i < rawFitnessScoresArray.Length; i++) {
            fitnessRankText += "[" + rankedIndicesList[i].ToString() + "]: " + rankedFitnessList[i].ToString() + "\n";
        }

        // CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER CROSSOVER 
        List<BrainGenome> newGenBrainGenomeList = new List<BrainGenome>(); // new population!        

        //FitnessManager fitnessManager = teamsConfig.playersList[playerIndex].fitnessManager;
        //TrainingSettingsManager trainingSettingsManager = teamsConfig.playersList[playerIndex].trainingSettingsManager;

        // Keep top-half peformers + mutations:
        for (int x = 0; x < genomePoolArray.Length; x++) {
            if (x == 0) {
                BrainGenome parentGenome = genomePoolArray[rankedIndicesList[x]].brainGenome;  // keep top performer as-is
                newGenBrainGenomeList.Add(parentGenome);
            }
            else {
                BrainGenome newBrainGenome = new BrainGenome();
                int parentIndex = GetAgentIndexByLottery(rankedFitnessList, rankedIndicesList);
                BrainGenome parentGenome = genomePoolArray[parentIndex].brainGenome;
                newBrainGenome.SetToMutatedCopyOfParentGenome(parentGenome, mutationSettings);
                newGenBrainGenomeList.Add(newBrainGenome);
            }
        }

        for (int i = 0; i < genomePoolArray.Length; i++) {
            genomePoolArray[i].brainGenome = newGenBrainGenomeList[i];
        }

        // Reset
        ResetTrainingForNewGen();

        //mutationSettings.mutationChance *= 0.995f;
        //mutationSettings.mutationStepSize *= 0.995f;

        UpdateAgentBrains();

        curGen++;
    }

    private void UpdateAgentBrains() {
        // UPDATE BRAINS TEST!!!!!
        for (int a = 0; a < agentsArray.Length; a++) {
            agentsArray[a].ReplaceBrain(genomePoolArray[a % 4]);
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

    private void ResetTrainingForNewGen() {
        rawFitnessScoresArray = new float[populationSize];
        rawFitnessScoreBlankAgent = 0f;

        curTestingAgent = 0;
        curTestingSample = 0;

        dummyAgent.InitializeAgentFromGenome(genomePoolArray[0], dummyStartGenome);
        SetUpTrainingDataBatch();
    }
}
