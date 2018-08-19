using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameManager gameManager;
    //public SimulationManager simManager;
    public CameraManager cameraManager;
    public GameOptionsManager gameOptionsManager;

    public Texture2D healthDisplayTex;

    public GameObject panelTint;

    public bool optionsMenuOn = false;

    public Text textLoadingTooltips;

    public GameObject panelHUD;
    public Image imageFood;
    public Image imageHitPoints;
    public Material foodMat;
    public Material hitPointsMat;
    public Text textScore;

    public GameObject panelDeathScreen;
    public Text textRespawnCounter;
    public Text textCauseOfDeath;
    public Text textPlayerScore;

    public GameObject panelObserverMode;
    public Text textCurGen;
    public Text textAvgLifespan;

    public GameObject panelPaused;

    public GameObject panelDebug;
    public Material fitnessDisplayMat;
    public Button buttonPause;
    public Button buttonPlaySlow;
    public Button buttonPlayNormal;
    public Button buttonPlayFast;
    public Button buttonModeA;
    public Button buttonModeB;
    public Button buttonModeC;

    public Text textDebugTrainingInfo1;
    public Text textDebugTrainingInfo2;
    public Text textDebugTrainingInfo3;
    public Text textDebugSimSettings;
    public Button buttonToggleRecording;
    public Button buttonToggleTrainingSupervised;
    public Button buttonResetGenomes;
    public Button buttonClearTrainingData;
    public Button buttonToggleTrainingPersistent;

    public GameObject panelTitleMenu;
    public GameObject panelGameOptions;

    // TOP PANEL::::
    public GameObject panelTop;
    public Button buttonToggleHUD;
    public bool isActiveHUD = true;
    public Button buttonToggleDebug;
    public bool isActiveDebug = true;

    public bool isObserverMode = false;
    public bool deathScreenOn = false;
    public bool isPaused = false;

    public GameObject panelMainMenu;    
    public GameObject panelLoading;
    public GameObject panelPlaying;

    public Texture2D fitnessDisplayTexture;

    public float timeOfLastPlayerDeath = 0f;

    public float loadingProgress = 0f;

    private int obsZoomLevel = 0;  // 0 most zoomed out

    
	// Use this for initialization
	void Start () {
        
        
    }

    public void UpdateObserverModeUI() {
        if(isObserverMode) {
            panelObserverMode.SetActive(true);

            textCurGen.text = "Generation: " + gameManager.simulationManager.curApproxGen.ToString("F0");
            textAvgLifespan.text = "Average Lifespan: " + Mathf.RoundToInt(gameManager.simulationManager.rollingAverageAgentScoresArray[0]).ToString();

            if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {  // UP !!!!
                
                //cameraManager
                
                /*obsZoomLevel = obsZoomLevel + 1;
                if(obsZoomLevel > 2) {
                    obsZoomLevel = 2;
                }

                if(obsZoomLevel == 1) {
                    ClickButtonModeB();
                }
                if(obsZoomLevel == 2) {
                    ClickButtonModeA();
                } */               
            }
            if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {  // DOWN !!!!
                /*obsZoomLevel = obsZoomLevel - 1;
                if(obsZoomLevel < 0) {
                    obsZoomLevel = 0;
                }

                if(obsZoomLevel == 1) {
                    ClickButtonModeB();
                }
                if(obsZoomLevel == 0) {
                    ClickButtonModeC();
                }*/
            }
            
            if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {  // RIGHT !!!!
                /*int newIndex = cameraManager.targetCritterIndex + 24;
                if(newIndex >= gameManager.simulationManager._NumAgents) {
                    newIndex = 0;                    
                }
                cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);
                */
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {  // LEFT !!!!!
                /*
                int newIndex = cameraManager.targetCritterIndex - 1;
                if(newIndex < 0) {
                    newIndex = newIndex + 24;                    
                }
                cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);
                */
            }
            if(Input.GetKeyDown(KeyCode.Escape)) {
                Debug.Log("Pressed Escape!");
                isPaused = !isPaused;

                if(isPaused) {
                    ClickButtonPause();
                }
                else {
                    ClickButtonPlayNormal();
                }
            }
            if(Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log("Pressed Spacebar!");
                    
                // Detach from following creature
            }

            // &&&&&&&&&&&&&&&&& MOUSE: &&&&&&&&&&&&&&&
            if (Input.GetMouseButtonDown(0)) {
                //Debug.Log("LEFT CLICK!");

                // CHECK IF CLICKED ON AN OBJECT OF INTEREST:
                Ray ray = cameraManager.camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

                if (hit.point != null) {
                    //Debug.Log("CLICKED ON: [ " + hit.ToString() + " ]);

                    if(hit.collider != null) {
                        Agent agentRef = hit.collider.gameObject.GetComponentInParent<Agent>();
                        if(agentRef != null) {
                            Debug.Log("AGENT: [ " + agentRef.gameObject.name + " ] #" + agentRef.index.ToString());

                            cameraManager.SetTarget(agentRef, agentRef.index);
                        }


                        Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
                    }
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                Debug.Log("RIGHT CLICKETY-CLICK!");
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0f ) //  Forwards
            {
                Debug.Log("Mouse ScrollWheel Forward");

                // ** TEMPORARY!!!! ****!!!!

                obsZoomLevel = obsZoomLevel + 1;
                if(obsZoomLevel > 2) {
                    obsZoomLevel = 2;
                }

                if(obsZoomLevel == 1) {
                    ClickButtonModeB();
                }
                if(obsZoomLevel == 2) {
                    ClickButtonModeA();
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) //  Backwarfds
            {
                Debug.Log("Mouse ScrollWheel Backward");

                obsZoomLevel = obsZoomLevel - 1;
                if(obsZoomLevel < 0) {
                    obsZoomLevel = 0;
                }

                if(obsZoomLevel == 1) {
                    ClickButtonModeB();
                }
                if(obsZoomLevel == 0) {
                    ClickButtonModeC();
                }
            }
                    
        }
        else {
            panelObserverMode.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
        switch (gameManager.CurrentGameState) {
            case GameManager.GameState.MainMenu:
                UpdateMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                UpdateLoadingUI();
                break;
            case GameManager.GameState.Playing:
                // Check for Key PResses!
                /*if(Input.GetKeyDown(KeyCode.Escape)) {
                    Debug.Log("Pressed Escape Key!");
                    //if(!deathScreenOn) {
                    //    EnterObserverMode();                        
                    //}     
                    if(!isPaused) {

                        EnterObserverMode();                        
                    }  
                }*/
                /*if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) {
                    Debug.Log("Pressed ENTER Key!");

                    if(!isPaused) {
                        if(isObserverMode) {
                            isObserverMode = false;
                            ClickButtonModeA();
                            // Respawn PLAYER!!! ****
                            gameManager.simulationManager.RespawnPlayer();
                            deathScreenOn = false;
                        }
                        if(deathScreenOn) {
                            // Respawn PLAYER!!! ****
                            gameManager.simulationManager.RespawnPlayer();
                            deathScreenOn = false;
                        }
                        
                    }                    
                }*/
                
                UpdateSimulationUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameManager.CurrentGameState.ToString() + ")");
                break;
        }
    }

    public void EnterObserverMode() {
        obsZoomLevel = 0; // C, zoomed out max

        ClickButtonModeC();

        isObserverMode = true;
        deathScreenOn = false;

        gameManager.simulationManager.EnterObserverMode();
    }

    public void TransitionToNewGameState(GameManager.GameState gameState) {
        switch (gameState) {
            case GameManager.GameState.MainMenu:
                EnterMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                EnterLoadingUI();
                break;
            case GameManager.GameState.Playing:
                EnterPlayingUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameState.ToString() + ")");
                break;
        }
    }
    private void EnterMainMenuUI() {
        panelMainMenu.SetActive(true);
        if (optionsMenuOn) {
            panelGameOptions.SetActive(true);
            panelTitleMenu.SetActive(false);
        }
        else {
            panelGameOptions.SetActive(false);
            panelTitleMenu.SetActive(true);
        }
        panelLoading.SetActive(false);
        panelPlaying.SetActive(false);

        panelHUD.SetActive(isActiveHUD);
        panelDebug.SetActive(isActiveDebug);

        if(fitnessDisplayTexture == null) {
            fitnessDisplayTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
            fitnessDisplayTexture.wrapMode = TextureWrapMode.Clamp;
            fitnessDisplayMat.SetTexture("_MainTex", fitnessDisplayTexture);
        }        
        
        ClickButtonModeB();
    }
    private void EnterLoadingUI() {
        panelMainMenu.SetActive(false);
        panelLoading.SetActive(true);
        panelPlaying.SetActive(false);
        panelGameOptions.SetActive(false);
    }
    private void EnterPlayingUI() {
        panelMainMenu.SetActive(false);
        panelLoading.SetActive(false);
        panelPlaying.SetActive(true);
        panelGameOptions.SetActive(false);
    }
    private void UpdateMainMenuUI() {
                
    }
    private void UpdateLoadingUI() {
        if (loadingProgress < 1f) {
            textLoadingTooltips.text = "( Calculating Enjoyment Coefficients )";
        }
        if (loadingProgress < 0.65f) {
            textLoadingTooltips.text = "( Warming Up Simulation Cubes )";
        }
        if (loadingProgress < 0.4f) {
            textLoadingTooltips.text = "( Feeding Hamsters )";
        }
        if (loadingProgress < 0.1f) {
            textLoadingTooltips.text = "( Reticulating Splines )";
        }
    }
    private void UpdateSimulationUI() {
        UpdateScoreText(gameManager.simulationManager.agentsArray[0].scoreCounter);

        SetDisplayTextures();

        UpdateDebugUI();
        UpdateHUDUI();
        UpdateObserverModeUI();
        //UpdateDeathScreenUI();
        UpdatePausedUI();
    }

    public void PlayerDied(bool starved) {
        deathScreenOn = true;
        timeOfLastPlayerDeath = Time.realtimeSinceStartup;

        string causeOfDeath = "STARVED!";
        if(!starved) {
            causeOfDeath = "IMPALED!";
        }
        textCauseOfDeath.text = causeOfDeath;
        textPlayerScore.text = "Lived For " + gameManager.simulationManager.lastPlayerScore.ToString() + " Years!";
    }

    public void UpdateDebugUI() {

        // DISABLED!!!! -- Need to establish good method for grabbing data from SimulationManager!
        SimulationManager simManager = gameManager.simulationManager;

        string debugTxt1 = ""; // Training: False";

        //debugTxt = "Training: ACTIVE   numSamples: " + dataSamplesList.Count.ToString() + "\n";
        //debugTxt += "Gen: " + curGen.ToString() + ", Agent: " + curTestingGenomeSupervised.ToString() + ", Sample: " + curTestingSample.ToString() + "\n";
        //debugTxt += "Fitness Best: " + bestFitnessScoreSupervised.ToString() + " ( Avg: " + avgFitnessLastGenSupervised.ToString() + " ) Blank: " + lastGenBlankAgentFitnessSupervised.ToString() + "\n";
        Agent agentRef = cameraManager.targetAgent;
        int agentIndex = agentRef.index;

        //debugTxt1 += "Agent[" + agentIndex.ToString() + "] # Neurons: " + cameraManager.targetAgent.brain.neuronList.Count.ToString() + ", # Axons: " + cameraManager.targetAgent.brain.axonList.Count.ToString() + "\n";
        debugTxt1 += "CurOldestAge: " + simManager.currentOldestAgent.ToString() + ", numChildrenBorn: " + simManager.numAgentsBorn.ToString() + ", ~Gen: " + ((float)simManager.numAgentsBorn / (float)simManager._NumAgents).ToString();
        debugTxt1 += "\nBotRecordAge: " + simManager.recordBotAge.ToString() + ", PlayerRecordAge: " + simManager.recordPlayerAge.ToString();
        debugTxt1 += "\nAverageAgentScore: " + simManager.rollingAverageAgentScoresArray[0].ToString();
        

        string debugTxt2 = "";
        debugTxt2 += "THE BRAIN !!!\n\n"; // + agentRef.coreModule.coreWidth.ToString() + "\n";
        debugTxt2 += "# Neurons: " + cameraManager.targetAgent.brain.neuronList.Count.ToString() + ", # Axons: " + cameraManager.targetAgent.brain.axonList.Count.ToString() + "\n\n";
        debugTxt2 += "Throttle: [ " + agentRef.movementModule.throttleX[0].ToString("F3") + ", " + agentRef.movementModule.throttleY[0].ToString("F3") + " ]\n\n";
        debugTxt2 += "OutComms: [ " + agentRef.coreModule.outComm0[0].ToString("F2") + ", " + agentRef.coreModule.outComm1[0].ToString("F2") + ", " + agentRef.coreModule.outComm2[0].ToString("F2")  + ", " + agentRef.coreModule.outComm3[0].ToString("F2") + " ]\n";
        debugTxt2 += "Dash: " + agentRef.movementModule.dash[0].ToString("F2") + "\n";

        string debugTxt3 = "";
        debugTxt3 += "CRITTER # " + agentIndex.ToString() + " ( " + agentRef.curLifeStage.ToString() + " )  Age: " + agentRef.scoreCounter.ToString() + " Frames\n\n";
        debugTxt3 += "Energy: " + agentRef.coreModule.energyStored[0].ToString("F4") + "\n";
        debugTxt3 += "Health: " + agentRef.coreModule.healthBody.ToString("F2") + "\n";
        debugTxt3 += "Food: " + agentRef.coreModule.foodStored[0].ToString("F2") + "\n";
        debugTxt3 += "Stamina: " + agentRef.coreModule.stamina[0].ToString("F2") + "\n\n";
        debugTxt3 += "Width: " + agentRef.coreModule.coreWidth.ToString("F2") + ",  Length: " + agentRef.coreModule.coreLength.ToString("F2") + "\n";
        /*
        string debugTxtSimSettings = "\nSIMULATION SETTINGS\n\n";
        debugTxtSimSettings += "Mutation Parameters:\nBODY: Frequency: " + simManager.settingsManager.mutationSettingsPersistent.defaultBodyMutationChance.ToString("F4") + ", Magnitude: " + simManager.settingsManager.mutationSettingsPersistent.defaultBodyMutationStepSize.ToString("F4") + "\n";
        debugTxtSimSettings += "BRAIN: Frequency: " + simManager.settingsManager.mutationSettingsPersistent.mutationChance.ToString("F4") + ", Magnitude: " + simManager.settingsManager.mutationSettingsPersistent.mutationStepSize.ToString("F4") + "\n";
        debugTxtSimSettings += "New Axon Chance: " + simManager.settingsManager.mutationSettingsPersistent.newLinkChance.ToString("F4") + ",  New Neuron Chance: " + simManager.settingsManager.mutationSettingsPersistent.newHiddenNodeChance.ToString("F4") + ",  Weight Decay: " + simManager.settingsManager.mutationSettingsPersistent.weightDecayAmount.ToString("F4") + "\n";
        debugTxtSimSettings += "\nFood:\n";
        debugTxtSimSettings += "Energy Burn Rate Multiplier: " + simManager.settingsManager.energyDrainMultiplier.ToString("F2") + "\n";
        debugTxtSimSettings += "Max Global Food [Tiny]: " + simManager.settingsManager.maxGlobalNutrients.ToString("F2") + "\n";
        debugTxtSimSettings += "Eat Rate Multiplier: " + simManager.settingsManager.eatRateMultiplier.ToString("F3") + "\n";
        debugTxtSimSettings += "Spawn New Food Rate: " + simManager.settingsManager.spawnNewFoodChance.ToString("F3") + "\n";
        debugTxtSimSettings += "Diffusion Rate: " + simManager.settingsManager.foodDiffusionRate.ToString("F2") + "\n";
        debugTxtSimSettings += "Feeding Efficiency Smallest Critter: " + simManager.settingsManager.minSizeFeedingEfficiency.ToString("F2") + "\n";
        debugTxtSimSettings += "Feeding Efficiency Largest Critter: : " + simManager.settingsManager.maxSizeFeedingEfficiency.ToString("F2") + "\n";
        debugTxtSimSettings += "\nMax Global Food [Particle]: " + simManager.settingsManager.maxFoodParticleTotalAmount.ToString("F2") + "\n";
        debugTxtSimSettings += "Avg Radius: " + simManager.settingsManager.avgFoodParticleRadius.ToString("F3") + "\n";
        debugTxtSimSettings += "Radii Variance: " + simManager.settingsManager.foodParticleRadiusVariance.ToString("F2") + "\n";
        debugTxtSimSettings += "Nutrients Bonus Multiplier: x" + simManager.settingsManager.foodParticleNutrientDensity.ToString("F2") + "\n";
        debugTxtSimSettings += "Respawn Rate: " + simManager.settingsManager.foodParticleRegrowthRate.ToString("F5") + "\n";
        */
        textDebugTrainingInfo1.text = debugTxt1;
        textDebugTrainingInfo2.text = debugTxt2;
        textDebugTrainingInfo3.text = debugTxt3;
        //textDebugSimSettings.text = debugTxtSimSettings;

        /*if (recording) {
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

        if (simManager.isTrainingPersistent) {
            buttonToggleTrainingPersistent.GetComponentInChildren<Text>().text = "Persistent\nTraining: ON";
        }
        else {
            buttonToggleTrainingPersistent.GetComponentInChildren<Text>().text = "Persistent\nTraining: OFF";
        }*/
    }
    public void UpdateHUDUI() {
        if(isObserverMode) {
            panelHUD.SetActive(false);
        }
        else {
            panelHUD.SetActive(true);
        }
        if(deathScreenOn) {
            // Do something to Energy Meter???
            panelHUD.SetActive(false);
        }
        else {
            
        }
    }
    public void UpdatePausedUI() {
        if(isPaused) {
            panelPaused.SetActive(true);
        }
        else {
            panelPaused.SetActive(false);
        }
    }
    
    /*public void UpdateDeathScreenUI() {
        if(deathScreenOn) {
            panelDeathScreen.SetActive(true);

            float currentTime = Time.realtimeSinceStartup;

            float elapsedDeathTime = currentTime - timeOfLastPlayerDeath;

            float respawnTimer = 6f - elapsedDeathTime;
            textRespawnCounter.text = "Respawn In " + Mathf.CeilToInt(respawnTimer).ToString() + " Seconds... Or Press 'Enter'";

            if(elapsedDeathTime > 6f) {

                if(isPaused) {

                }
                else {
                    Debug.Log("5 seconds elapsed since player died");

                    deathScreenOn = false;
                    gameManager.simulationManager.RespawnPlayer();
                }                
            }
        }
        else {
            panelDeathScreen.SetActive(false);
        }
    }*/

    public void RefreshFitnessTexture(List<Vector4> generationScores) {
        fitnessDisplayTexture.Resize(Mathf.Max(1, generationScores.Count), 1);

        // Find Score Range:
        float bestScore = 1f;
        for (int i = 0; i < generationScores.Count; i++) {
            bestScore = Mathf.Max(bestScore, generationScores[i].x);
            bestScore = Mathf.Max(bestScore, generationScores[i].y);
            bestScore = Mathf.Max(bestScore, generationScores[i].z);
            bestScore = Mathf.Max(bestScore, generationScores[i].w);
        }
        for(int i = 0; i < generationScores.Count; i++) {
            fitnessDisplayTexture.SetPixel(i, 0, new Color(generationScores[i].x, generationScores[i].y, generationScores[i].z, generationScores[i].w));
        }
        fitnessDisplayTexture.Apply();

        fitnessDisplayMat.SetFloat("_BestScore", bestScore);
        fitnessDisplayMat.SetTexture("_MainTex", fitnessDisplayTexture);
        
    }

    public void UpdateScoreText(int score) {
        textScore.text = "Score: " + score.ToString();
    }
        

    public void SetDisplayTextures() {
        foodMat.SetTexture("_MainTex", healthDisplayTex);
        hitPointsMat.SetTexture("_MainTex", healthDisplayTex);
    }

    public void ClickOptionsMenu() {
        optionsMenuOn = true;
        EnterMainMenuUI();
    }
    public void ClickBackToMainMenu() {
        optionsMenuOn = false;
        EnterMainMenuUI();
    }
    public void ClickStartGame() {
        Debug.Log("ClickStartGame()!");
        gameManager.StartNewGame();
    }

    public void ClickResetWorld() {
        Debug.Log("Reset The World!");
        gameManager.simulationManager.ResetWorld();
    }

    public void ClickButtonQuit() {
        Debug.Log("Quit!");
        Application.Quit();
    }

    public void ClickButtonPause() {
        Time.timeScale = 0f;
    }
    public void ClickButtonPlaySlow() {
        Time.timeScale = 0.4f;
    }
    public void ClickButtonPlayNormal() {
        Time.timeScale = 1f;
    }
    public void ClickButtonPlayFast() {
        Time.timeScale = 2.5f;
    }
    public void ClickButtonModeA() {
        cameraManager.ChangeGameMode(CameraManager.GameMode.ModeA);
        obsZoomLevel = 2; // C, zoomed out max
    }
    public void ClickButtonModeB() {
        cameraManager.ChangeGameMode(CameraManager.GameMode.ModeB);
        obsZoomLevel = 1; // C, zoomed out max
    }
    public void ClickButtonModeC() {
        cameraManager.ChangeGameMode(CameraManager.GameMode.ModeC);
        obsZoomLevel = 0; // C, zoomed out max
    }

    public void ClickButtonToggleHUD() {
        isActiveHUD = !isActiveHUD;
        panelHUD.SetActive(isActiveHUD);
    }
    public void ClickButtonToggleDebug() {
        isActiveDebug = !isActiveDebug;
        panelDebug.SetActive(isActiveDebug);
    }


}
