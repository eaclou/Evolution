using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {

    #region attributes
    public GameManager gameManager;
    public WorldSpiritHubUI worldSpiritHubUI;
    public DebugPanelUI debugPanelUI;
    public WatcherUI watcherUI;
    public BrushesUI brushesUI;
    public KnowledgeUI knowledgeUI;
    public MutationUI mutationUI;
    public GlobalResourcesUI globalResourcesUI;
    public TheCursorCzar theCursorCzar;
    public ClockUI clockUI;

    public CameraManager cameraManager;
    public GameOptionsManager gameOptionsManager;

    private bool firstTimeStartup = true;

    //public bool isBrushModeON_snoopingOFF = true;

    public Animator animatorSpiritUnlock;

    public Text textFXPH_loading_start;
    public Text textFXPH_loading_end;
    public Text textFXPH_playing_start;

    public GameObject protoSpiritClickColliderGO;
    
    public PanelFocus panelFocus = PanelFocus.WorldHub;
    public enum PanelFocus {
        None,
        WorldHub,
        Brushes,
        Watcher
    }

    // Main Menu:
    public Button buttonQuickStartResume;
    public Button buttonNewSimulation;
    public Text textMouseOverInfo;  // use!
    //public Texture2D healthDisplayTex;
    public GameObject panelTitleMenu;
    public GameObject panelGameOptions;

    //public GameObject panelTint;
    public bool controlsMenuOn = false; // main menu
    public bool optionsMenuOn = false;  // Game options main menu

    public Image imageLoadingStartBG;
    public Image imageLoadingStrokes01;
    public Image imageLoadingStrokes02;
    public Image imageLoadingStrokes03;
    public Image imageLoadingStrokesFull;
    public Image imageLoadingGemGrowing;
    public Button buttonLoadingGemStart;
    public Text textLoadingTooltips;

    public GameObject panelBigBang;
    public Image imageBigBangStrokes01;
    public Image imageBigBangStrokes02;
    public Image imageBigBangStrokes03;

    public bool updateTerrainAltitude;
    public float terrainUpdateMagnitude;
    
    public ToolType curActiveTool;  // ******** move to phase out this approach
    public enum ToolType {
        None,
        Add,
        Stir
    }

    
    // *********************************************    NEW UI PASS *******************
    public GameObject panelClock;
    public Text textCurYear;
    public Button buttonClockOpenClose;
    public Button buttonOpenGlobalResourcesPanel;
    public Button buttonOpenMutationPanel;
    public Button buttonOpenWatcherPanel;
    public Button buttonOpenBrushesPanel;
    public Button buttonOpenKnowledgePanel;

    // *******************************************

    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    public Color colorSpiritBrushLight;
    public Color colorSpiritBrushDark;

    public Color colorWorldLayer;
    public Color colorTerrainLayer;
    public Color colorMineralLayer;
    public Color colorWaterLayer;
    public Color colorAirLayer;
    public Color colorDecomposersLayer;
    public Color colorAlgaeLayer;
    public Color colorPlantsLayer;
    public Color colorZooplanktonLayer;
    public Color colorVertebratesLayer;
        
    
    // announcements:
    public GameObject panelPendingClickPrompt;
    private bool announceAlgaeCollapsePossible = false;
    private bool announceAlgaeCollapseOccurred = false;
    private bool announceAgentCollapsePossible = false;
    private bool announceAgentCollapseOccurred = false;
    public int timerAnnouncementTextCounter = 0;
    public bool isAnnouncementTextOn = false;
    public bool isUnlockCooldown = false;
    public int unlockCooldownCounter = 0;
    private bool inspectToolUnlockedAnnounce = false;
    public TrophicSlot unlockedAnnouncementSlotRef;
  
    // ***** figure out where to put this
    public Sprite spriteSpiritBrushKnowledgeIcon;
    public Sprite spriteSpiritBrushMutationIcon;
    public Sprite spriteSpiritBrushStirIcon;
    public Sprite spriteSpiritBrushWatcherIcon;
    public Sprite spriteSpiritBrushWatcherOffIcon;
    public Sprite spriteSpiritBrushCreationIcon;

    public Sprite spriteSpiritWorldIcon;
    public Sprite spriteSpiritStoneIcon;
    public Sprite spriteSpiritPebblesIcon;
    public Sprite spriteSpiritSandIcon;
    public Sprite spriteSpiritMineralsIcon;
    public Sprite spriteSpiritWaterIcon;
    public Sprite spriteSpiritAirIcon;
    public Sprite spriteSpiritDecomposerIcon;
    public Sprite spriteSpiritAlgaeIcon;
    public Sprite spriteSpiritPlantIcon;
    public Sprite spriteSpiritZooplanktonIcon;
    public Sprite spriteSpiritVertebrateIcon;
        
    public GameObject panelObserverMode;
    public GameObject panelPaused;
    
    //public bool isActiveDebug = true;

    public bool isObserverMode = false;
    public bool isPaused = false;

    public GameObject panelMainMenu;
    public GameObject panelLoading;
    public GameObject panelPlaying;
    
    public float loadingProgress = 0f;

    private int[] displaySpeciesIndicesArray;
   
    public int selectedSpeciesID;
    public int hoverAgentID;

    private const int maxDisplaySpecies = 32;

    private float curSpeciesStatValue;

    public bool isBrushAddingAgents = false;
    public int brushAddAgentCounter = 0;
    public int framesPerAgentSpawn = 3;

    public int bigBangFramesCounter = 0;

    public Vector3 creationSpiritClickableStartPos = new Vector3(120f, 124f, -5f);
    public Vector3 roamingSpiritPosition;
    public float roamingSpiritScale = 4.0f;
    public bool isClickableSpiritRoaming;
    public Color roamingSpiritColor;
    public int framesSinceLastClickableSpirit;
    public ClickableSpiritType curClickableSpiritType = ClickableSpiritType.CreationBrush;
    public enum ClickableSpiritType {
        CreationBrush,
        KnowledgeSpirit,
        MutationSpirit,
        WatcherSpirit,
        Pebbles,
        Sand,
        Air,
        Water,
        Minerals,
        Decomposers,
        Algae,
        Zooplankton,
        Plants,
        VertA,
        VertB,
        VertC,
        VertD
    }

    //public bool brainDisplayOn = false;
    
    #endregion


    #region Initialization Functions:::
    // Use this for initialization
    void Start() {
        //animatorInspectPanel.enabled = false;

        
        //ClickToolButtonAdd();
        
    }
    public void EnterObserverMode() {
        isObserverMode = true;
    }
    public void TransitionToNewGameState(GameManager.GameState gameState) {
        switch (gameState) {
            case GameManager.GameState.MainMenu:
                //canvasMain.renderMode = RenderMode.ScreenSpaceOverlay;
                EnterMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                EnterLoadingUI();
                break;
            case GameManager.GameState.Playing:
                //canvasMain.renderMode = RenderMode.ScreenSpaceCamera;
                firstTimeStartup = false;
                EnterPlayingUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameState.ToString() + ")");
                break;
        }
    }
    private void EnterMainMenuUI() {
        panelMainMenu.SetActive(true);

        UpdateMainMenuUI();

        /*
        // Best place for these???
        if (statsSpeciesColorKey == null) {
            statsSpeciesColorKey = new Texture2D(maxDisplaySpecies, 1, TextureFormat.ARGB32, false);
            statsSpeciesColorKey.filterMode = FilterMode.Point;
            statsSpeciesColorKey.wrapMode = TextureWrapMode.Clamp;
        }

        if (statsTreeOfLifeSpeciesTexArray.Length == 0) {
            statsTreeOfLifeSpeciesTexArray = new Texture2D[16]; // start with 16 choosable stats

            for (int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
                Texture2D statsTexture = new Texture2D(maxDisplaySpecies, 1, TextureFormat.RGBAFloat, false);
                statsSpeciesColorKey.filterMode = FilterMode.Point;
                statsSpeciesColorKey.wrapMode = TextureWrapMode.Clamp;

                statsTreeOfLifeSpeciesTexArray[i] = statsTexture;
            }
        }

        if (maxValuesStatArray.Length == 0) {
            maxValuesStatArray = new float[16];
            for (int i = 0; i < maxValuesStatArray.Length; i++) {
                maxValuesStatArray[i] = 0.01f;
            }
        }

        // &&&& INFO PANEL GRAPHS::::
        if (infoOxygenDataTexture == null) {
            infoOxygenDataTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            infoOxygenDataTexture.filterMode = FilterMode.Bilinear;
            infoOxygenDataTexture.wrapMode = TextureWrapMode.Clamp;
        }
        if (infoNutrientsDataTexture == null) {
            infoNutrientsDataTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            infoNutrientsDataTexture.filterMode = FilterMode.Bilinear;
            infoNutrientsDataTexture.wrapMode = TextureWrapMode.Clamp;
        }
        if (infoDetritusDataTexture == null) {
            infoDetritusDataTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            infoDetritusDataTexture.filterMode = FilterMode.Bilinear;
            infoDetritusDataTexture.wrapMode = TextureWrapMode.Clamp;
        }
        if (infoDecomposersDataTexture == null) {
            infoDecomposersDataTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            infoDecomposersDataTexture.filterMode = FilterMode.Bilinear;
            infoDecomposersDataTexture.wrapMode = TextureWrapMode.Clamp;
        }
        if (infoPlantsDataTexture == null) {
            infoPlantsDataTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            infoPlantsDataTexture.filterMode = FilterMode.Bilinear;
            infoPlantsDataTexture.wrapMode = TextureWrapMode.Clamp;
        }
        if (infoAnimalsDataTexture == null) {
            infoAnimalsDataTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            infoAnimalsDataTexture.filterMode = FilterMode.Bilinear;
            infoAnimalsDataTexture.wrapMode = TextureWrapMode.Clamp;
        }
        infoGraphOxygenMat.SetTexture("_DataTex", infoOxygenDataTexture);
        infoGraphOxygenMat.SetFloat("_MaxValue", 1000f);
        infoGraphNutrientsMat.SetTexture("_DataTex", infoNutrientsDataTexture);
        infoGraphNutrientsMat.SetFloat("_MaxValue", 4000f);
        //infoGraphDetritusMat.SetTexture("_DataTex", infoDetritusDataTexture);
        //infoGraphDetritusMat.SetFloat("_MaxValue", 4000f);
        infoGraphDecomposersMat.SetTexture("_DataTex", infoDecomposersDataTexture);
        infoGraphDecomposersMat.SetFloat("_MaxValue", 4000f);
        infoGraphPlantsMat.SetTexture("_DataTex", infoPlantsDataTexture);
        infoGraphPlantsMat.SetFloat("_MaxValue", 4000f);
        infoGraphAnimalsMat.SetTexture("_DataTex", infoAnimalsDataTexture);
        infoGraphAnimalsMat.SetFloat("_MaxValue", 100f);
        */
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

        //Animation Big Bang here
        gameManager.simulationManager._BigBangOn = true;
        //worldSpiritHubUI.OpenWorldTreeSelect();

        SimEventData newEventData = new SimEventData();
        newEventData.name = "New Simulation Start!";
        newEventData.category = SimEventData.SimEventCategories.NPE;
        newEventData.timeStepActivated = 0;
        gameManager.simulationManager.simEventsManager.completeEventHistoryList.Add(newEventData);

        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "... And Then There Was Not Nothing ...";// "Welcome! This Pond is devoid of life...\nIt's up to you to change that!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.75f, 0.75f, 0.75f);
        panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        isAnnouncementTextOn = true;
        timerAnnouncementTextCounter = 0;
    }
    #endregion

    // Update is called once per frame    
    // =================================================================================================================================================
    #region UPDATE UI PANELS FUNCTIONS!!! :::

    void Update() {
        switch (gameManager.CurrentGameState) {
            case GameManager.GameState.MainMenu:
                UpdateMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                UpdateLoadingUI();
                break;
            case GameManager.GameState.Playing:
                UpdateSimulationUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameManager.CurrentGameState.ToString() + ")");
                break;
        }
    }
    private void UpdateMainMenuUI() {
        Cursor.visible = true;   /////// ********************** Move to CursorCzar!!!
        if (firstTimeStartup) {
            buttonQuickStartResume.GetComponentInChildren<Text>().text = "QUICK START";
        }
        else {
            buttonQuickStartResume.GetComponentInChildren<Text>().text = "RESUME";
            buttonNewSimulation.gameObject.SetActive(false); // *** For now, 1 sim at a time ***
            textMouseOverInfo.gameObject.SetActive(false);
        }

        if (optionsMenuOn) {
            panelGameOptions.SetActive(true);
            textMouseOverInfo.gameObject.SetActive(false);
            //panelTitleMenu.SetActive(false);
        }
        else {
            panelGameOptions.SetActive(false);
            //panelTitleMenu.SetActive(true);
        }

        panelLoading.SetActive(false);
        panelPlaying.SetActive(false);

        //panelHUD.SetActive(isActiveHUD);
        //panelDebug.SetActive(isActiveDebug);
    }
    private void UpdateLoadingUI() {
        //Cursor.visible = true;
        //imageLoadingGemGrowing.gameObject.SetActive(true);
        //buttonLoadingGemStart.gameObject.SetActive(false);
        if (loadingProgress > 0.5f) {
            textLoadingTooltips.text = "";
            
            imageLoadingStartBG.gameObject.SetActive(false);
            imageLoadingStrokes01.gameObject.SetActive(false);
            imageLoadingStrokes02.gameObject.SetActive(false);
            imageLoadingStrokes03.gameObject.SetActive(false);

            imageLoadingStrokesFull.gameObject.SetActive(true);
        }
        
        /*if (loadingProgress < 0.4f) {
            //textLoadingTooltips.text = "( Feeding Hamsters )";
        }
        if (loadingProgress < 0.1f) {
            textLoadingTooltips.text = "( Reticulating Splines )";
        }*/
    }
    private void UpdateSimulationUI() {

        if(gameManager.simulationManager._BigBangOn) {
            panelBigBang.SetActive(true);
            bigBangFramesCounter += 1;

            if(bigBangFramesCounter == 70) {
                worldSpiritHubUI.isUnlocked = true;
                worldSpiritHubUI.OpenWorldTreeSelect();
                
            }
            
            if(bigBangFramesCounter > 70) {
                bigBangFramesCounter = 0;
                gameManager.simulationManager._BigBangOn = false;
                panelBigBang.SetActive(false);

                
            }
            else if(bigBangFramesCounter > 40) {
                imageBigBangStrokes01.gameObject.SetActive(true);
                imageBigBangStrokes02.gameObject.SetActive(false);
                imageBigBangStrokes03.gameObject.SetActive(false);

                 worldSpiritHubUI.PlayBigBangSpawnAnim();               

            }
            else if(bigBangFramesCounter > 20) {
                imageBigBangStrokes01.gameObject.SetActive(true);
                imageBigBangStrokes02.gameObject.SetActive(true);
                imageBigBangStrokes03.gameObject.SetActive(false);

                
            }
            else if(bigBangFramesCounter > 0) {
                imageBigBangStrokes01.gameObject.SetActive(true);
                imageBigBangStrokes02.gameObject.SetActive(true);
                imageBigBangStrokes03.gameObject.SetActive(true);

                
            }
        }


        UpdateObserverModeUI();  // <== this is the big one *******  
        // ^^^  Need to Clean this up and replace with better approach ***********************
        
        theCursorCzar.UpdateCursorCzar();  // this will assume a larger role
        
        brushesUI.UpdateBrushesUI();
        watcherUI.UpdateWatcherPanelUI(gameManager.simulationManager.trophicLayersManager);
        knowledgeUI.UpdateKnowledgePanelUI(gameManager.simulationManager.trophicLayersManager);
        mutationUI.UpdateMutationPanelUI(gameManager.simulationManager.trophicLayersManager);
        worldSpiritHubUI.UpdateWorldSpiritHubUI();
        globalResourcesUI.UpdateGlobalResourcesPanelUpdate();
        UpdateClockPanelUI();
        
        debugPanelUI.UpdateDebugUI();
              
        UpdatePausedUI();

        if(gameManager.simulationManager.simAgeTimeSteps < 240f && gameManager.simulationManager.simAgeTimeSteps > 160f) {
            textFXPH_playing_start.gameObject.SetActive(true);
        }
        else {
            textFXPH_playing_start.gameObject.SetActive(false);
        }

        if(isClickableSpiritRoaming) {
            protoSpiritClickColliderGO.SetActive(true);
            float orbitSpeed = 0.5f;
            float orbitRadius = 20f;
            float spinAngle = Time.realtimeSinceStartup * orbitSpeed;
            float zPhase = spinAngle * 9.5f;
            float zBounceMag = 1f;
            roamingSpiritScale = 3f + 0.5f * Mathf.Cos(Time.realtimeSinceStartup * 7.673f);
            roamingSpiritPosition = creationSpiritClickableStartPos + new Vector3(Mathf.Cos(spinAngle) * orbitRadius, Mathf.Sin(spinAngle) * orbitRadius, Mathf.Sin(zPhase) * zBounceMag);

            protoSpiritClickColliderGO.transform.position = roamingSpiritPosition;

            framesSinceLastClickableSpirit = 0;
        }
        else {
            framesSinceLastClickableSpirit++;
            protoSpiritClickColliderGO.SetActive(false);
        }

        if(animatorSpiritUnlock.GetBool("_AnimFinished")) {
            SpiritUnlockComplete();
            animatorSpiritUnlock.SetBool("_AnimFinished", false);
        }
        
        /*if(worldSpiritHubUI.isUnlocked) {
            worldSpiritHubUI.panelWorldHubExpand.SetActive(true);
        }
        else {
            worldSpiritHubUI.panelWorldHubExpand.SetActive(false);
        }*/

        if(brushesUI.isUnlocked) {
            buttonOpenBrushesPanel.gameObject.SetActive(true);
        }
        else {
            buttonOpenBrushesPanel.gameObject.SetActive(false);
        }

        if(watcherUI.isUnlocked) {
            buttonOpenWatcherPanel.gameObject.SetActive(true);
        }
        else {
            buttonOpenWatcherPanel.gameObject.SetActive(false);
        }

        if(knowledgeUI.isUnlocked) {
            buttonOpenKnowledgePanel.gameObject.SetActive(true);
            buttonOpenKnowledgePanel.GetComponent<Image>().sprite = worldSpiritHubUI.curIconSprite;
        }
        else {
            buttonOpenKnowledgePanel.gameObject.SetActive(false);
        }

        if(mutationUI.isUnlocked) {
            buttonOpenMutationPanel.gameObject.SetActive(true);
        }
        else {
            buttonOpenMutationPanel.gameObject.SetActive(false);
        }

        if(globalResourcesUI.isUnlocked) {
            buttonOpenGlobalResourcesPanel.gameObject.SetActive(true);
        }
        else {
            buttonOpenGlobalResourcesPanel.gameObject.SetActive(false);
        }
        


        /*
        if(isWatcherPanelOn) {
            buttonToolbarWatcher.GetComponent<Image>().color = buttonActiveColor;
            buttonToolbarWatcher.gameObject.transform.localScale = Vector3.one * 1.25f;
            //imageToolbarInspectLinkedIcon.color = buttonActiveColor;

            UpdateWatcherPanelUI(layerManager);

            panelWatcherSpiritMain.SetActive(true);
        }
        else {
            buttonToolbarWatcher.GetComponent<Image>().color = buttonDisabledColor;
            buttonToolbarWatcher.gameObject.transform.localScale = Vector3.one;
            //imageToolbarInspectLinkedIcon.color = buttonDisabledColor;

            panelWatcherSpiritMain.SetActive(false);
        }
        
        if(isKnowledgePanelOn) {
            buttonToolbarKnowledge.GetComponent<Image>().color = buttonActiveColor;
            buttonToolbarKnowledge.gameObject.transform.localScale = Vector3.one * 1.25f;
            //imageToolbarKnowledgeLinkedIcon.color = buttonActiveColor;            
                        
            //spiritBrushName = "Knowledge Spirit";
            //imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushKnowledgeIcon;
            panelKnowledgeSpiritBase.SetActive(true);
            //panelKnowledgeSpirit.SetActive(true); 
        
            UpdateKnowledgePanelUI(layerManager);
        }
        else {
            buttonToolbarKnowledge.GetComponent<Image>().color = buttonDisabledColor;
            buttonToolbarKnowledge.gameObject.transform.localScale = Vector3.one;
            //imageToolbarKnowledgeLinkedIcon.color = buttonDisabledColor;

            panelKnowledgeSpiritBase.SetActive(false);
            //panelKnowledgeSpirit.SetActive(false); 
        }

        if(isMutationPanelOn) {
            buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor;
            buttonToolbarMutate.gameObject.transform.localScale = Vector3.one * 1.25f;            
            panelMutationSpirit.SetActive(true);
        
            UpdateToolbarMutationPanel(layerManager);
        }
        else {
            buttonToolbarMutate.GetComponent<Image>().color = buttonDisabledColor;
            buttonToolbarMutate.gameObject.transform.localScale = Vector3.one;
            //imageToolbarKnowledgeLinkedIcon.color = buttonDisabledColor;

            panelMutationSpirit.SetActive(false);
        }
           
        UpdateDebugUI(); // IF ACTIVE!!!!!!
        UpdateObserverModeUI();  // <== this is the big one *******        
        UpdatePausedUI();
        UpdateToolbarPanelUI();
        //Update other panels
        UpdateKnowledgePanelUI(gameManager.simulationManager.trophicLayersManager);
        */
        // ***/////// ********************** Move to CursorCzar!!!
        
             

        
    }
    
    public void UpdateObserverModeUI() {
        if (isObserverMode) {
            panelObserverMode.SetActive(true);

            Vector2 moveDir = Vector2.zero;
            bool isKeyboardInput = false;
            // CONTROLLER:
            float controllerHorizontal = Input.GetAxis("Horizontal");
            float controllerVertical = Input.GetAxis("Vertical");
            moveDir.x = controllerHorizontal;
            moveDir.y = controllerVertical;

            Vector2 rightStickInput = new Vector2(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"));

            float leftTrigger = Input.GetAxis("LeftTrigger");
            float rightTrigger = Input.GetAxis("RightTrigger");
            //float buttonA = Input.GetAxis("ButtonA");
            //float buttonB = Input.GetAxis("ButtonB");
            //float buttonX = Input.GetAxis("ButtonX");
            //float buttonY = Input.GetAxis("ButtonY");
            /*if(rightStickInput.sqrMagnitude > 0.01f) {
                Debug.Log("Controller Right Stick: (" + rightStickInput.x.ToString() + ", " + rightStickInput.y.ToString() + 
                ")\nTriggers (" + leftTrigger.ToString() + ", " + rightTrigger.ToString() + ") A: " + 
                buttonA.ToString() + ", B: " + buttonB.ToString() + ", X: " + buttonX.ToString() + ", Y: " + buttonY.ToString());

            }*/

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {  // UP !!!!
                moveDir.y = 1f;
                isKeyboardInput = true;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {  // DOWN !!!!                
                moveDir.y = -1f;
                isKeyboardInput = true;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {  // RIGHT !!!!
                moveDir.x = 1f;
                isKeyboardInput = true;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {  // LEFT !!!!!                
                moveDir.x = -1f;
                isKeyboardInput = true;
            }

            if (isKeyboardInput) {
                moveDir = moveDir.normalized;
                watcherUI.StopFollowingAgent();
                watcherUI.StopFollowingPlantParticle();
                watcherUI.StopFollowingAnimalParticle();
            }
            if (moveDir.sqrMagnitude > 0.001f) {
                watcherUI.StopFollowingAgent();
                watcherUI.StopFollowingPlantParticle();
                watcherUI.StopFollowingAnimalParticle();
            }

            cameraManager.MoveCamera(moveDir); // ********************

            if (Input.GetKey(KeyCode.R)) {
                cameraManager.TiltCamera(-1f);
            }
            if (Input.GetKey(KeyCode.F)) {
                cameraManager.TiltCamera(1f);
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Debug.Log("Pressed Escape!");
                // Pause & Main Menu? :::
                ClickButtonPause();
                gameManager.EscapeToMainMenu();
            }
            if (Input.GetKeyDown(KeyCode.Tab)) {
                Debug.Log("Pressed Tab!");
                debugPanelUI.ClickButtonToggleDebug();
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log("Pressed Spacebar!");
                isPaused = !isPaused;
                if (isPaused) {
                    ClickButtonPause();
                }
                else {
                    ClickButtonPlayNormal();
                }
            }

            if (Input.GetKeyDown("joystick button 7")) {
                Debug.Log("Pressed Start!");
                //ClickButtonQuit();
            }
            if (Input.GetKeyDown("joystick button 0")) {
                Debug.Log("Pressed ButtonA!");
                //ClickStatsButton();
            }
            if (Input.GetKeyDown("joystick button 1")) {
                Debug.Log("Pressed ButtonB!");
                //ClickStatsButton();
            }
            if (Input.GetKeyDown("joystick button 2")) {
                Debug.Log("Pressed ButtonX!");
                //ClickStatsButton();
            }
            if (Input.GetKeyDown("joystick button 3")) {
                Debug.Log("Pressed ButtonY!");
                //ClickStatsButton();
            }
            if (Input.GetAxis("RightTrigger") > 0.01f) {
                Debug.Log("Pressed RightTrigger: " + Input.GetAxis("RightTrigger").ToString());
                //ClickStatsButton();
            }
                        
            if (isAnnouncementTextOn) {
                panelPendingClickPrompt.SetActive(true);
                timerAnnouncementTextCounter++;

                if (timerAnnouncementTextCounter > 640) {
                    isAnnouncementTextOn = false;
                    timerAnnouncementTextCounter = 0;

                    inspectToolUnlockedAnnounce = false;
                }
            }
            else {
                panelPendingClickPrompt.SetActive(false);
            }
                                    
            if (EventSystem.current.IsPointerOverGameObject()) {  // if mouse is over ANY unity canvas UI object (with raycast enabled)
                //Debug.Log("MouseOverUI!!!");
            }
            else {
                int selectedPlantID = gameManager.simulationManager.vegetationManager.selectedPlantParticleIndex;
                int closestPlantID = gameManager.simulationManager.vegetationManager.closestPlantParticleData.index;
                float plantDist = (gameManager.simulationManager.vegetationManager.closestPlantParticleData.worldPos - new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y)).magnitude;

                int selectedZoopID = gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex;
                int closestZoopID = gameManager.simulationManager.zooplanktonManager.closestAnimalParticleData.index;
                float zoopDist = (new Vector2(gameManager.simulationManager.zooplanktonManager.closestAnimalParticleData.worldPos.x, gameManager.simulationManager.zooplanktonManager.closestAnimalParticleData.worldPos.y) - new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y)).magnitude;
                
                if(plantDist < zoopDist) {                    
                    
                    if(panelFocus == PanelFocus.Watcher && !cameraManager.isMouseHoverAgent && theCursorCzar.leftClickThisFrame) { 
                        if(selectedPlantID != closestPlantID && plantDist < 3.3f) {
                            gameManager.simulationManager.vegetationManager.selectedPlantParticleIndex = closestPlantID;
                            gameManager.simulationManager.vegetationManager.isPlantParticleSelected = true;
                            Debug.Log("FOLLOWING PLANT " + gameManager.simulationManager.vegetationManager.selectedPlantParticleIndex.ToString());
                            //isSpiritBrushSelected = true;
                            watcherUI.StartFollowingPlantParticle();
                        }
                    }
                    
                }
                else {                   

                    if (panelFocus == PanelFocus.Watcher && !cameraManager.isMouseHoverAgent && theCursorCzar.leftClickThisFrame) {
                        if (selectedZoopID != closestZoopID && zoopDist < 3.3f) {
                            gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex = closestZoopID;
                            gameManager.simulationManager.zooplanktonManager.isAnimalParticleSelected = true;
                            Debug.Log("FOLLOWING ZOOP " + gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString());
                            //isSpiritBrushSelected = true;
                            watcherUI.StartFollowingAnimalParticle();
                        }
                    }
                }

                watcherUI.isPlantParticleHighlight = 0f;
                watcherUI.isZooplanktonHighlight = 0f;
                watcherUI.isVertebrateHighlight = 0f;
                if(cameraManager.isMouseHoverAgent) {  // move this to cursorCzar?
                    watcherUI.isVertebrateHighlight = 1f;
                }
                else {
                    if(plantDist < zoopDist && plantDist < 2f) {
                        watcherUI.isPlantParticleHighlight = 1f;
                    }
                    if(plantDist > zoopDist && zoopDist < 2f) {
                        watcherUI.isZooplanktonHighlight = 1f;
                    }
                }
                

                //Debug.Log("plantDist: " + plantDist.ToString() + ", zoopDist: " + zoopDist.ToString() + ",  agent: " + cameraManager.isMouseHoverAgent.ToString());
                
                if (curActiveTool == ToolType.Stir) {  
                    theCursorCzar.stirGizmoVisible = true;
                    
                    float isActing = 0f;
                    
                    if (theCursorCzar.isDraggingMouseLeft) {
                        
                        isActing = 1f;
                        
                        float mag = theCursorCzar.smoothedMouseVel.magnitude;
                        float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                        if(mag > 0f) {
                            gameManager.simulationManager.PlayerToolStirOn(theCursorCzar.curMousePositionOnWaterPlane, theCursorCzar.smoothedMouseVel * (0.25f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.2f), radiusMult);
                            
                        }
                        else {
                            gameManager.simulationManager.PlayerToolStirOff();
                        }
                    }
                    else {
                        gameManager.simulationManager.PlayerToolStirOff();                        
                    }

                    if(isActing > 0.5f) {
                        theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, 1f, 0.2f);
                    }
                    else {
                        theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, -4f, 0.2f);
                    }
                    gameManager.theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft;
                    gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsStirring", isActing);
                    gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_IsStirring", isActing);                    
                    gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_Radius", 6.2f);
                }

                gameManager.theRenderKing.isBrushing = false;
                gameManager.theRenderKing.isSpiritBrushOn = false;
                gameManager.theRenderKing.nutrientToolOn = false;
                isBrushAddingAgents = false;
                gameManager.simulationManager.vegetationManager.isBrushActive = false;
                
                if (curActiveTool == ToolType.Add && panelFocus == PanelFocus.Brushes) {
                    // What Palette Trophic Layer is selected?
                    theCursorCzar.stirGizmoVisible = true;

                    gameManager.simulationManager.PlayerToolStirOff();

                    if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight) {

                        if(!brushesUI.isInfluencePointsCooldown) {
                            if(brushesUI.toolbarInfluencePoints >= 0.05f) {
                                brushesUI.ApplyCreationBrush();
                            }
                            else {
                                Debug.Log("NOT ENOUGH MANA!");
                                // Enter Cooldown!
                                brushesUI.isInfluencePointsCooldown = true;
                            } 
                        }                                               
                    }
                    else {
                        
                    }
                }
                else {

                }
            }
            
            if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight) {
                gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);
            }
            else {
                gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);                   
            }
            
            if(theCursorCzar.stirGizmoVisible) {
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
                gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_IsVisible", 1f);
                //Cursor.visible = false;
            }         
            else {
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
                gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_IsVisible", 0f);
                //Cursor.visible = true;
            }

            //bool isCursorOn = true;
            if(gameManager.uiManager.curActiveTool == ToolType.Add) {
                //isCursorOn = false;
            }
            if(gameManager.uiManager.curActiveTool == ToolType.Stir) {
                //isCursorOn = false;
            }
            //Cursor.visible = isCursorOn;

            if (Input.GetMouseButtonDown(1)) {
                Debug.Log("RIGHT CLICKETY-CLICK!");
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0f ) //  Forwards
            {
                cameraManager.ZoomCamera(-1f);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) //  Backwarfds
            {
                cameraManager.ZoomCamera(1f);            
            }

            float zoomSpeed = 0.167f;
            float zoomVal = 0f;
            if (Input.GetKey("joystick button 4")) //  Forwards
            {
                zoomVal += 1f;
                //Debug.Log("RightShoulder");     
            }
            if (Input.GetKey("joystick button 5")) //  Backwarfds
            {
                zoomVal -= 1f; 
                //Debug.Log("LeftShoulder");    
            }
            cameraManager.ZoomCamera(zoomVal * zoomSpeed);  
        }
        else {
            panelObserverMode.SetActive(false);
        }
    }        
    public void UpdateClockPanelUI() {
        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();

        clockUI.UpdateClockUI(gameManager.simulationManager.simAgeTimeSteps);
        //Update Clock
    }
    public void UpdatePausedUI() {
        if(isPaused) {
            panelPaused.SetActive(true);
        }
        else {
            panelPaused.SetActive(false);
        }
    }    
            
    public void CheckForAnnouncements() {
        //announceAlgaeCollapsePossible = false;
    //private bool announceAlgaeCollapseOccurred = false;
    //private bool announceAgentCollapsePossible = false; 
    //private bool announceAgentCollapseOccurred = false;

        // 
        if(!announceAlgaeCollapseOccurred) {
            if(announceAlgaeCollapsePossible) {
                if(gameManager.simulationManager.simResourceManager.curGlobalPlantParticles < 10f) {
                    announceAlgaeCollapsePossible = false;
                    announceAlgaeCollapseOccurred = true;

                    panelPendingClickPrompt.GetComponentInChildren<Text>().text = "<color=#DDDDDDFF>Algae Died from lack of Nutrients!</color>\nAdd Decomposers to recycle waste";
                    panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorDecomposersLayer;
                    panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
                    isAnnouncementTextOn = true;
                }
            }
            else {

            }
        }
        // Enable checking for this announcement happening -- algae collapse requires it to have reached full population at some point
        /*if(gameManager.simulationManager.trophicLayersManager.GetAlgaeOnOff()) {
            if(gameManager.simulationManager.simResourceManager.curGlobalAlgaeParticles > 100f) {
                announceAlgaeCollapsePossible = true;
            }
        }*/

        // AGENTS:
        /*if(!announceAgentCollapseOccurred) {
            if(announceAgentCollapsePossible) {
                if(gameManager.simulationManager.simResourceManager.curGlobalAgentParticles < 10f) {
                    announceAgentCollapsePossible = false;
                    announceAgentCollapseOccurred = true;

                    panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Vertebrate Population Collapsed! (Lack of Oxygen)";
                    panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorPlantsLight;
                    isAnnouncementTextOn = true;
                }
            }
            else {

            }
        }
        // Enable checking for this announcement happening -- algae collapse requires it to have reached full population at some point
        if(gameManager.simulationManager.trophicLayersManager.GetAlgaeOnOff()) {
            if(gameManager.simulationManager.simResourceManager.curGlobalAlgaeParticles > 100f) {
                announceAlgaeCollapsePossible = true;
            }
        }*/
    }

    public Vector4 SampleTexture(RenderTexture tex, Vector2 uv) {
        Vector4[] sample = new Vector4[1];


        ComputeBuffer outputBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        outputBuffer.SetData(sample);
        int kernelCSSampleTexture = gameManager.simulationManager.computeShaderResourceGrid.FindKernel("CSSampleTexture");
        gameManager.simulationManager.computeShaderResourceGrid.SetTexture(kernelCSSampleTexture, "_CommonSampleTex", tex);
        gameManager.simulationManager.computeShaderResourceGrid.SetBuffer(kernelCSSampleTexture, "outputValuesCBuffer", outputBuffer);
        gameManager.simulationManager.computeShaderResourceGrid.SetFloat("_CoordX", uv.x);
        gameManager.simulationManager.computeShaderResourceGrid.SetFloat("_CoordY", uv.y); 
        // DISPATCH !!!
        gameManager.simulationManager.computeShaderResourceGrid.Dispatch(kernelCSSampleTexture, 1, 1, 1);
        
        outputBuffer.GetData(sample);

        outputBuffer.Release();

        return sample[0];
    }
    
    private void InitToolbarPortraitCritterData(TrophicSlot slot) { // ** should be called AFTER new species actually created?? something is fucked here
        Debug.Log("InitToolbarPortraitCritterData.. selectedSpeciesID: " + selectedSpeciesID.ToString() + " , slotLinkedID: " + slot.linkedSpeciesID.ToString());
        
        SpeciesGenomePool speciesPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        //gameManager.theRenderKing.InitializeNewCritterPortraitGenome(speciesPool.representativeGenome);
        gameManager.theRenderKing.InitializeNewCritterPortraitGenome(gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome); // speciesPool.leaderboardGenomesList[0].candidateGenome);
        gameManager.theRenderKing.isToolbarCritterPortraitEnabled = true;
         
    }

    public void SetToolbarButtonStateUI(bool isDim, ref Button button, TrophicSlot.SlotStatus slotStatus, bool isSelected) {

        button.gameObject.SetActive(true);
        //Animation anim = button.GetComponent<Animation>();
        Vector3 scale = Vector3.one;
        ColorBlock colBlock = button.colors; //.colorMultiplier = 1f;
        colBlock.colorMultiplier = 1f;
        switch(slotStatus) {
            case TrophicSlot.SlotStatus.Off:
                button.gameObject.SetActive(false); 
                //button.gameObject.transform.localScale = Vector3.one;
                break;
            case TrophicSlot.SlotStatus.Locked:
                button.interactable = false; 
                colBlock.colorMultiplier = 0.5f;
                //button.GetComponentInChildren<Text>().text = "";
                //button.gameObject.transform.localScale = Vector3.one;
                
                break;
            case TrophicSlot.SlotStatus.Unlocked:
                button.interactable = true;  
                colBlock.colorMultiplier = 0.8f;
                //button.GetComponentInChildren<Text>().text = "";
                //button.GetComponent<Image>().sprite = spriteSpeciesSlotRing;
                //anim.Play();
                //scale = Vector3.one * 1f;
                break;
            //case TrophicSlot.SlotStatus.Pending:
            //    button.interactable = false;
            //    button.GetComponentInChildren<Text>().text = "%";
            //    break;
            case TrophicSlot.SlotStatus.On:
                button.interactable = true;
                colBlock.colorMultiplier = 0.8f;                
                
                //button.GetComponentInChildren<Text>().text = "";
                //button.GetComponent<Image>().sprite = spriteSpeciesSlotFull;
                //anim.Rewind();
                //anim.Stop();
                
                //
                break;
            //case TrophicSlot.SlotStatus.Selected:
            //    button.interactable = false;
            //    button.GetComponentInChildren<Text>().text = "**";
            //    break;
            default:
                break;
        }
        if(isSelected) {
            scale = Vector3.one * 1.5f;
            colBlock.colorMultiplier = 2f;
        }

        if(isDim) {
            button.colors = colBlock;
            button.gameObject.transform.localScale = scale;

            button.gameObject.SetActive(false);
        }
        else {
            button.colors = colBlock;

            button.gameObject.transform.localScale = scale;
            button.gameObject.SetActive(true);
        }

        
    }
    
    #endregion

    #region UTILITY & EVENT FUNCTIONS

    /*private void RemoveSpeciesSlot() {
                
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
        //Debug.Log("RemoveSpeciesSlot( " + slot.slotID.ToString() + " ), k= " + slot.slotID.ToString());

        if(slot.kingdomID == 0) {
            Debug.Log("Remove Decomposers");
            gameManager.simulationManager.trophicLayersManager.TurnOffDecomposers();
        }
        else if(slot.kingdomID == 1) {  // plant
            Debug.Log("Remove Algae");
            gameManager.simulationManager.trophicLayersManager.TurnOffAlgae();
            //gameManager.simulationManager.TurnOffAlgae();  // what to do with existing algae???
        }
        else {  // animals
            if(slot.tierID == 0) {
                Debug.Log("Remove Zooplankton");
                gameManager.simulationManager.trophicLayersManager.TurnOffZooplankton();

            }
            else {  // tier 1
                Debug.Log("Remove AGENT");
                //slot.status
                gameManager.simulationManager.RemoveSelectedAgentSpecies(slot.slotID);
            }
        }

        slot.status = TrophicSlot.SlotStatus.Unlocked;
        
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0]; // Bedrock
        
    }*/

    #endregion

    #region UI ELEMENT CLICK FUNCTIONS!!!!

    // 
    // MAIN MENU STUFFS::
    public void ClickControlsMenu() {
        controlsMenuOn = true;
        optionsMenuOn = false;
        UpdateMainMenuUI();
    }
    public void ClickOptionsMenu() {
        optionsMenuOn = true;
        controlsMenuOn = false;
        UpdateMainMenuUI();
    }
    
    public void ClickQuickStart() {
        Debug.Log("ClickQuickStart()!");
        if(firstTimeStartup) {
            gameManager.StartNewGameQuick();
        }
        else {
            //animatorStatsPanel.enabled = false;
            //animatorInspectPanel.enabled = false;
            gameManager.ResumePlaying();
            ClickButtonPlayNormal();

        } 
    }
    public void ClickNewSimulation() {
        //Debug.Log("ClickNewSimulation()!");
        if(firstTimeStartup) {
            gameManager.StartNewGameBlank();
        }
        else {
            //animatorStatsPanel.enabled = false;
            //animatorInspectPanel.enabled = false;
            gameManager.ResumePlaying();
            ClickButtonPlayNormal();
        }        
    }
    public void MouseEnterQuickStart() {
        //textMouseOverInfo.gameObject.SetActive(true);
        //textMouseOverInfo.text = "Start with an existing ecosystem full of various living organisms.";
    }
    public void MouseExitQuickStart() {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterNewSimulation() {
        //textMouseOverInfo.gameObject.SetActive(true);
        //textMouseOverInfo.text = "Create a brand new ecosystem from scratch. It might take a significant amount of time for intelligent creatures to evolve.\n*Not recommended for first-time players.";
    }
    public void MouseExitNewSimulation() {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterControlsButton() {
        textMouseOverInfo.gameObject.SetActive(true);
        textMouseOverInfo.text = "Arrows or WASD for movement, scrollwheel for zoom. 'R' and 'F' tilt Camera.\nKeyboard & Mouse only - Controller support coming soon.";
    }
    public void MouseExitControlsButton() {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void ClickLoadingGemStart() {
        Debug.Log("Let there be not nothing!");

        gameManager.simulationManager._BigBangOn = true;
    }

    public void ClickToolButtonBrushes() {
        brushesUI.ClickToolButton();
    }
    public void ClickToolButtonWatcher() {
        watcherUI.ClickToolButton();
    }
    public void ClickToolButtonKnowledge() {
        knowledgeUI.ClickToolButton();
    }
    public void ClickToolButtonMutate() {
        mutationUI.ClickToolButton();        
    }
    public void ClickToolButtonGlobalResources() {
        globalResourcesUI.ClickToolButton();        
    }
    
    public void SpiritUnlockComplete() {
        switch(curClickableSpiritType) {
            case UIManager.ClickableSpiritType.CreationBrush:
                UnlockBrushes();                         
                break;
            case UIManager.ClickableSpiritType.Water:
                gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status = TrophicSlot.SlotStatus.On;
                Debug.Log("WATER UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //brushesUI.Unlock();
                //brushesUI.SetTargetFromWorldTree();
                AnnounceUnlockWater();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
                            
                curClickableSpiritType = UIManager.ClickableSpiritType.Decomposers;
                break;
            case UIManager.ClickableSpiritType.Decomposers:
                gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                Debug.Log("DECOMPOSERS UNLOCKED!!! " + unlockCooldownCounter.ToString());

                AnnounceUnlockDecomposers();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
                worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);

                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                            
                //isClickableSpiritRoaming = false;
                curClickableSpiritType = UIManager.ClickableSpiritType.KnowledgeSpirit;
                break;
            case UIManager.ClickableSpiritType.KnowledgeSpirit:
                
                knowledgeUI.isUnlocked = true;
                AnnounceUnlockKnowledgeSpirit();
                knowledgeUI.OpenKnowledgePanel();
                worldSpiritHubUI.OpenWorldTreeSelect();
                //isClickableSpiritRoaming = false;
                curClickableSpiritType = UIManager.ClickableSpiritType.Algae;
                break;
            case UIManager.ClickableSpiritType.WatcherSpirit:
                watcherUI.isUnlocked = true;
                watcherUI.ClickToolButton();
                panelFocus = PanelFocus.Watcher;
                watcherUI.animatorWatcherUI.SetBool("_IsOpen", true);
                AnnounceUnlockWatcherSpirit();
                curClickableSpiritType = UIManager.ClickableSpiritType.Plants;
                break;
            case UIManager.ClickableSpiritType.MutationSpirit:
                mutationUI.isUnlocked = true;
                AnnounceUnlockMutationSpirit();
                //curClickableSpiritType = UIManager.ClickableSpiritType.Zooplankton; // *** Last unlock?
                curClickableSpiritType = UIManager.ClickableSpiritType.Air;
                mutationUI.ClickToolButton();
                worldSpiritHubUI.OpenWorldTreeSelect();
                break;
            case UIManager.ClickableSpiritType.Minerals:
                TrophicSlot mineralSlot = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[0];
                mineralSlot.status = TrophicSlot.SlotStatus.On;
                Debug.Log("MINERALS UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockMinerals();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = mineralSlot;
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
                //worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);
                worldSpiritHubUI.selectedWorldSpiritSlot = mineralSlot;
                brushesUI.selectedEssenceSlot = mineralSlot;
                            
                curClickableSpiritType = UIManager.ClickableSpiritType.Zooplankton;
                break;
            case UIManager.ClickableSpiritType.Air:
                TrophicSlot airSlot = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[2];
                airSlot.status = TrophicSlot.SlotStatus.On;
                Debug.Log("AIR SPIRIT UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockAir();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = airSlot;
                worldSpiritHubUI.selectedWorldSpiritSlot = airSlot;
                brushesUI.selectedEssenceSlot = airSlot;
                            
                curClickableSpiritType = UIManager.ClickableSpiritType.MutationSpirit;
                break;
            case UIManager.ClickableSpiritType.Pebbles:
                TrophicSlot pebblesSlot = gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2];
                pebblesSlot.status = TrophicSlot.SlotStatus.On;
                Debug.Log("PEBBLES SPIRIT UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockPebbles();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = pebblesSlot;
                worldSpiritHubUI.selectedWorldSpiritSlot = pebblesSlot;
                brushesUI.selectedEssenceSlot = pebblesSlot;
                brushesUI.selectedBrushLinkedSpiritTerrainLayer = 2; // worldSpiritHubUI.selectedToolbarTerrainLayer; 
                brushesUI.ClickButtonBrushPaletteTerrain(2);

                curClickableSpiritType = UIManager.ClickableSpiritType.WatcherSpirit;
                break;
            case UIManager.ClickableSpiritType.Sand:
                TrophicSlot sandSlot = gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3];
                sandSlot.status = TrophicSlot.SlotStatus.On;
                Debug.Log("SAND SPIRIT UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockSand();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = sandSlot;
                worldSpiritHubUI.selectedWorldSpiritSlot = sandSlot;
                brushesUI.selectedEssenceSlot = sandSlot;
                brushesUI.selectedBrushLinkedSpiritTerrainLayer = 3; 
                brushesUI.ClickButtonBrushPaletteTerrain(3);
                            
                curClickableSpiritType = UIManager.ClickableSpiritType.VertA;
                break;
            case UIManager.ClickableSpiritType.Algae:
                gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                Debug.Log("ALGAE UNLOCKED!!! " + unlockCooldownCounter.ToString());

                AnnounceUnlockAlgae();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];   
                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0]);
                            
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                //isClickableSpiritRoaming = false;
                curClickableSpiritType = UIManager.ClickableSpiritType.Minerals;
                break;
            case UIManager.ClickableSpiritType.Plants:
                gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                Debug.Log("PLANTS UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockPlants();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0]);

                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];

                curClickableSpiritType = UIManager.ClickableSpiritType.Sand;
                break;
            case UIManager.ClickableSpiritType.Zooplankton:
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                Debug.Log("ZOOPLANKTON UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockZooplankton();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0]);
                            
                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                            
                curClickableSpiritType = UIManager.ClickableSpiritType.Pebbles;
                break;
            case UIManager.ClickableSpiritType.VertA:                            
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.On;                
                Debug.Log("CREATURE A UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockVertebrates();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0]);

                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];

                            
                curClickableSpiritType = UIManager.ClickableSpiritType.MutationSpirit;
                break;
            case UIManager.ClickableSpiritType.VertB:                            
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.On;                
                Debug.Log("CREATURE B UNLOCKED!!! " + unlockCooldownCounter.ToString());
                AnnounceUnlockVertebrates();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1]);

                worldSpiritHubUI.selectedWorldSpiritSlot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];
                brushesUI.selectedEssenceSlot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];

                break;
            default:
                Debug.LogError("No Enum Type Found! (");
                break;

        }
    }
    public void CapturedClickableSpirit() {
        animatorSpiritUnlock.SetTrigger("_TriggerClicked");
        isClickableSpiritRoaming = false;

        
    }
    public void UnlockBrushes() {

        brushesUI.Unlock();
        brushesUI.SetTargetFromWorldTree();
        curClickableSpiritType = UIManager.ClickableSpiritType.Water;

        AnnounceUnlockBrushes();
    }

    public void AnnounceBrushAppear() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A Minor Creation Spirit Appeared!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f);        
        isAnnouncementTextOn = true;
    }

    public void AnnounceUnlockBrushes() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Creation Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockWater() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Water Spirit Found!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.4f, 0.4f, 0.9f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockKnowledgeSpirit() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Knowledge Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.9f, 0.77f, 0.76f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockWatcherSpirit() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Watcher Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.6f, 0.71277f, 1f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockMutationSpirit() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Mutation Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.8f, 0.1277f, 0.1276f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockMinerals() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Minerals Essence Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockAir() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Air Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.5f, 0.5f, 1f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockPebbles() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Pebble Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockSand() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Sand Spirit Captured!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f);        
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockAlgae() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Algae Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorAlgaeLayer;
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = true;
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockDecomposers() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Decomposer Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorDecomposersLayer;
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = true;

        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockZooplankton() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Zooplankton Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorZooplanktonLayer;
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = true;
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockVertebrates() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Vertebrate Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorVertebratesLayer;
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = true;
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockPlants() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Plant Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorPlantsLayer;
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = true;
        isAnnouncementTextOn = true;
    }
   
    public void CheatUnlockAll() {
        Debug.Log("Cheat!!! Unlocked all species!!!");

        gameManager.simulationManager.trophicLayersManager.CheatUnlockAll();
    }
    public void ClickButtonQuit() {
        Debug.Log("Quit!");
        Application.Quit();
    }
    public void ClickButtonMainMenu() {
        gameManager.EscapeToMainMenu();
    }

    public void ClickButtonPause() {
        Time.timeScale = 0f;
    }
    public void ClickButtonPlayNormal() {
        Time.timeScale = 1f;
    }    
        
    

   
    
    

    #endregion
}



#region OLD OLD OLD!!!!



 
      
    /*public void UpdateSpeciesTreeDataTextures(int year) {  // refactor using year?

        int numActiveSpecies = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int numTotalSpecies = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count;
        
        displaySpeciesIndicesArray = new int[maxDisplaySpecies];

        TheRenderKing.TreeOfLifeSpeciesKeyData[] speciesKeyDataArray = new TheRenderKing.TreeOfLifeSpeciesKeyData[32];

         // Get Active ones first:
        for(int i = 0; i < gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
            SpeciesGenomePool pool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]];
            SpeciesGenomePool parentPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];

            Vector3 huePrimary = pool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
            Vector3 parentHue = parentPool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            statsSpeciesColorKey.SetPixel(i, 1, new Color(huePrimary.x, huePrimary.y, huePrimary.z));            
            //Debug.Log("(" + i.ToString() + ", " + gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i].ToString());
            displaySpeciesIndicesArray[i] = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i];

            TheRenderKing.TreeOfLifeSpeciesKeyData keyData = new TheRenderKing.TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.huePrimary = huePrimary;
            keyData.hueSecondary = hueSecondary;
            keyData.parentHue = parentHue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = 1f;
            //int selectedID = treeOfLifeManager.selectedID;
            
            keyData.isSelected = (selectedSpeciesID == gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]) ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        
        // Then fill with most recently extinct:
        for(int i = (numTotalSpecies - 1); i > Mathf.Clamp((numTotalSpecies - maxDisplaySpecies), 0, numTotalSpecies); i--) {
            SpeciesGenomePool pool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i];
            SpeciesGenomePool parentPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];

            Vector3 huePrimary = pool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
            Vector3 parentHue = parentPool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            if(gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                huePrimary = Vector3.Lerp(huePrimary, Vector3.one * 0.2f, 0.75f);
            }
            statsSpeciesColorKey.SetPixel(i, 1, new Color(huePrimary.x, huePrimary.y, huePrimary.z));
            
            displaySpeciesIndicesArray[i] = i;

            TheRenderKing.TreeOfLifeSpeciesKeyData keyData = new TheRenderKing.TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.huePrimary = huePrimary;
            keyData.hueSecondary = hueSecondary;
            keyData.parentHue = parentHue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = 1f;
            if(i >= gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {
                keyData.isOn = 0f;
            }
            if(pool.yearCreated == -1) {
                keyData.isOn = 0f;
            }
            if(i == 0) {
                keyData.isOn = 0f;
            }
            
            keyData.isSelected = selectedSpeciesID == i ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        statsSpeciesColorKey.Apply();

        gameManager.simulationManager.theRenderKing.treeOfLifeSpeciesDataKeyCBuffer.SetData(speciesKeyDataArray);

        // ========== data: =========== //
        int years = Mathf.Min(2048, year);  // cap textures at 2k for now?
        years = Mathf.Max(1, years);
        // check for resize before doing it?
        for(int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
            statsTreeOfLifeSpeciesTexArray[i].Resize(years, maxDisplaySpecies);
        }
        
        for(int i = 0; i < maxValuesStatArray.Length; i++) {
            maxValuesStatArray[i] = 0.01f;
        }
        // for each year & each species, create 2D texture with fitness scores:
        for(int s = 0; s < maxDisplaySpecies; s++) {            
            
            if(displaySpeciesIndicesArray[s] < gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {

                SpeciesGenomePool speciesPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[displaySpeciesIndicesArray[s]];
                if(speciesPool == null) {
                    Debug.LogError("well, shit");
                }
                for(int t = 0; t < years; t++) {
                
                    //int index = t - speciesPool.yearCreated;
                    
                    for(int a = 0; a < statsTreeOfLifeSpeciesTexArray.Length; a++) {
                        float valStat = 0f;
                        //= speciesPool
                        // I know there's a better way to do this:
                        if(a == 0) {
                            valStat = speciesPool.avgLifespanPerYearList[t];
                        }
                        else if(a == 1) {
                            valStat = speciesPool.avgConsumptionDecayPerYearList[t];
                        }
                        else if(a == 2) {
                            valStat = speciesPool.avgConsumptionPlantPerYearList[t];
                        }
                        else if(a == 3) {
                            valStat = speciesPool.avgConsumptionMeatPerYearList[t];
                        }
                        else if(a == 4) {
                            valStat = speciesPool.avgBodySizePerYearList[t];
                        }
                        else if(a == 5) {
                            valStat = speciesPool.avgSpecAttackPerYearList[t];
                        }
                        else if(a == 6) {
                            valStat = speciesPool.avgSpecDefendPerYearList[t];
                        }
                        else if(a == 7) {
                            valStat = speciesPool.avgSpecSpeedPerYearList[t];
                        }
                        else if(a == 8) {
                            valStat = speciesPool.avgSpecUtilityPerYearList[t];
                        }
                        else if(a == 9) {
                            valStat = speciesPool.avgFoodSpecDecayPerYearList[t];
                        }
                        else if(a == 10) {
                            valStat = speciesPool.avgFoodSpecPlantPerYearList[t];
                        }
                        else if(a == 11) {
                            valStat = speciesPool.avgFoodSpecMeatPerYearList[t];
                        }
                        else if(a == 12) {
                            valStat = speciesPool.avgNumNeuronsPerYearList[t];
                        }
                        else if(a == 13) {
                            valStat = speciesPool.avgNumAxonsPerYearList[t];
                        }
                        else if(a == 14) {
                            valStat = speciesPool.avgExperiencePerYearList[t];
                        }
                        else if(a == 15) {
                            valStat = speciesPool.avgFitnessScorePerYearList[t];
                        }
                        else if(a == 16) {
                            valStat = speciesPool.avgDamageDealtPerYearList[t];
                        }
                        else if(a == 17) {
                            valStat = speciesPool.avgDamageTakenPerYearList[t];
                        }
                        else if(a == 18) {
                            valStat = speciesPool.avgLifespanPerYearList[t];
                        }

                        maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);

                        //statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                        statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                    }                    
                }
                for (int b = 0; b < statsTreeOfLifeSpeciesTexArray.Length; b++) {
                    statsTreeOfLifeSpeciesTexArray[b].Apply();
                }
            }
            
        }
        
        //int selectedSpeciesID = treeOfLifeManager.selectedID;
        
        SpeciesGenomePool selectedPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];


        curSpeciesStatValue = 0f;
        
        if(selectedSpeciesStatsIndex == 0) {
            curSpeciesStatValue = selectedPool.avgLifespan;
        }
        else if(selectedSpeciesStatsIndex == 1) {
            curSpeciesStatValue = selectedPool.avgConsumptionDecay;
        }
        else if(selectedSpeciesStatsIndex == 2) {
            curSpeciesStatValue = selectedPool.avgConsumptionPlant;
        }
        else if(selectedSpeciesStatsIndex == 3) {
            curSpeciesStatValue = selectedPool.avgConsumptionMeat;
        }
        else if(selectedSpeciesStatsIndex == 4) {
            curSpeciesStatValue = selectedPool.avgBodySize;
        }
        else if(selectedSpeciesStatsIndex == 5) {
            curSpeciesStatValue = selectedPool.avgSpecAttack;
        }
        else if(selectedSpeciesStatsIndex == 6) {
            curSpeciesStatValue = selectedPool.avgSpecDefend;
        }
        else if(selectedSpeciesStatsIndex == 7) {
            curSpeciesStatValue = selectedPool.avgSpecSpeed;
        }
        else if(selectedSpeciesStatsIndex == 8) {
            curSpeciesStatValue = selectedPool.avgSpecUtility;
        }
        else if(selectedSpeciesStatsIndex == 9) {
            curSpeciesStatValue = selectedPool.avgFoodSpecDecay;
        }
        else if(selectedSpeciesStatsIndex == 10) {
            curSpeciesStatValue = selectedPool.avgFoodSpecPlant;
        }
        else if(selectedSpeciesStatsIndex == 11) {
            curSpeciesStatValue = selectedPool.avgFoodSpecMeat;
        }
        else if(selectedSpeciesStatsIndex == 12) {
            curSpeciesStatValue = selectedPool.avgNumNeurons;
        }
        else if(selectedSpeciesStatsIndex == 13) {
            curSpeciesStatValue = selectedPool.avgNumAxons;
        }
        else if(selectedSpeciesStatsIndex == 14) {
            curSpeciesStatValue = selectedPool.avgExperience;
        }
        else if(selectedSpeciesStatsIndex == 15) {
            curSpeciesStatValue = selectedPool.avgFitnessScore;
        }
        else if(selectedSpeciesStatsIndex == 16) {
            curSpeciesStatValue = selectedPool.avgDamageDealt;
        }
        else if(selectedSpeciesStatsIndex == 17) {
            curSpeciesStatValue = selectedPool.avgDamageTaken;
        }
        else if(selectedSpeciesStatsIndex == 18) {
            curSpeciesStatValue = selectedPool.avgLifespan;
        }
        
    }
    */
    
    
    /*
    public void UpdateInfoPanelUI() {
        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();


        textGlobalMass.text = "Global Biomass: " + gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles).ToString("F0");

        float percentageOxygen = resourcesRef.curGlobalOxygen / 3000f;
        infoMeterOxygenMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageOxygen));
        float percentageNutrients = resourcesRef.curGlobalNutrients / 3000f;
        infoMeterNutrientsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageNutrients));
        float percentageDetritus = resourcesRef.curGlobalDetritus / 3000f;
        infoMeterDetritusMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDetritus));
        float percentageDecomposers = resourcesRef.curGlobalDecomposers / 3000f;
        infoMeterDecomposersMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDecomposers));
        float percentagePlants = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir) / 3000f;
        infoMeterPlantsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentagePlants));
        float percentageAnimals = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles) / 100f;
        infoMeterAnimalsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageAnimals));

        if(isActiveInfoPanel) {
            panelInfoExpanded.SetActive(true);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = ">";

            if(isActiveInfoResourcesTab) {
                panelInfoResourcesOverview.SetActive(true);
                //panelInfoSpeciesOverview.SetActive(false);
            }
            else {  // &&& SPECIES TAB::::
                
            }
        }
        else {
            panelInfoExpanded.SetActive(false);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = "<";
        }
    }    
    */
     
    
    
    /*
    public void UpdateInfoPanelUI() {
        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();


        textGlobalMass.text = "Global Biomass: " + gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles).ToString("F0");

        float percentageOxygen = resourcesRef.curGlobalOxygen / 3000f;
        infoMeterOxygenMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageOxygen));
        float percentageNutrients = resourcesRef.curGlobalNutrients / 3000f;
        infoMeterNutrientsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageNutrients));
        float percentageDetritus = resourcesRef.curGlobalDetritus / 3000f;
        infoMeterDetritusMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDetritus));
        float percentageDecomposers = resourcesRef.curGlobalDecomposers / 3000f;
        infoMeterDecomposersMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDecomposers));
        float percentagePlants = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir) / 3000f;
        infoMeterPlantsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentagePlants));
        float percentageAnimals = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles) / 100f;
        infoMeterAnimalsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageAnimals));

        if(isActiveInfoPanel) {
            panelInfoExpanded.SetActive(true);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = ">";

            if(isActiveInfoResourcesTab) {
                panelInfoResourcesOverview.SetActive(true);
                //panelInfoSpeciesOverview.SetActive(false);
            }
            else {  // &&& SPECIES TAB::::
                
            }
        }
        else {
            panelInfoExpanded.SetActive(false);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = "<";
        }
    }    
    */
      
    
    //public void UpdateToolbarPanelUI() {

        //if (true) { }
        /*simManager.uiManager.AnnounceUnlockAlgae();
            simManager.uiManager.isUnlockCooldown = true;
            simManager.uiManager.unlockedAnnouncementSlotRef = kingdomPlants.trophicTiersList[0].trophicSlots[0];
            simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            simManager.uiManager.buttonToolbarExpandOn.interactable = true;)
            */
        /*if(!inspectToolUnlocked) {
            //if(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass > 0f) {
                // Unlock!!!!
                AnnounceUnlockInspect();
                inspectToolUnlocked = true;
                
            //}
        }*/

        // Check for Announcements:
        //CheckForAnnouncements();
        
    //}
    


 /*public void ClickAnnouncementText() {
        Debug.Log("ClickAnnouncementText");

        //if blah -- which species unlocked?
        //is toolbar on?
                
        if(inspectToolUnlockedAnnounce) { // *** GROSS HACK!!!

            ClickToolButtonInspect();
            inspectToolUnlockedAnnounce = false;
        }
        else {
            if(!isToolbarPaletteExpandOn) {
                ClickToolbarExpandOn();
            }

            if(unlockedAnnouncementSlotRef.kingdomID == 0) {
                ClickButtonToolbarDecomposers();
            }
            else if(unlockedAnnouncementSlotRef.kingdomID == 1) {
                ClickButtonToolbarAlgae();
            }
            else {
                if(unlockedAnnouncementSlotRef.tierID == 0) {
                    ClickButtonToolbarZooplankton();
                }
                else {
                    ClickButtonToolbarAgent(unlockedAnnouncementSlotRef.slotID);
                }
            } 
        }
    }
    */

/*public void UpdateTreeOfLifeWidget() {
        if(tolWorldStatsOn) {
            float displayVal = 0f;
            displayVal = 0f; // tolWorldStatsValueRangesKeyArray[tolSelectedWorldStatsIndex].y;

            //textTolWorldStatsValue.text = displayVal.ToString();
            if(tolSelectedWorldStatsIndex == 0) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalOxygen;
            }
            else if(tolSelectedWorldStatsIndex == 1) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalNutrients;
            }
            else if(tolSelectedWorldStatsIndex == 2) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalDetritus;
            }
            else if(tolSelectedWorldStatsIndex == 3) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalDecomposers;
            }
            else if(tolSelectedWorldStatsIndex == 4) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAlgaeReservoir;
            }
            else if(tolSelectedWorldStatsIndex == 5) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAlgaeParticles;
            }
            else if(tolSelectedWorldStatsIndex == 6) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles;
            }
            else if(tolSelectedWorldStatsIndex == 7) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass; // gameManager.simulationManager.settingsManager.curTierBodyMutationFrequency;
            }
            else if(tolSelectedWorldStatsIndex == 8) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalCarrionVolume;
            }
            else if(tolSelectedWorldStatsIndex == 9) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalEggSackVolume;
            }
            else if(tolSelectedWorldStatsIndex == 10) {
                displayVal = gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents;
            }
            //textTolWorldStatsValue.text = displayVal.ToString();
            curWorldStatValue = displayVal;

            UpdateTolWorldStatsTextReadout();
            
        }
        if(tolEventsTimelineOn) {
            if(gameManager.simulationManager.simEventsManager.completeEventHistoryList.Count > 0) {
                textTolEventsTimelineName.text = "Year: " + Mathf.FloorToInt((float)gameManager.simulationManager.simEventsManager.completeEventHistoryList[curClosestEventToCursor].timeStepActivated / 2000f).ToString() +
                ":   " + gameManager.simulationManager.simEventsManager.completeEventHistoryList[curClosestEventToCursor].name; // + " (" +  gameManager.simulationManager.simAgeTimeSteps.ToString();
                
            }
            else {

            }
            
            
        }
        else {

        }

        textTolYearValue.text = gameManager.simulationManager.curSimYear.ToString();

        imageTolBackdropGraphs.gameObject.SetActive(tolEventsTimelineOn);
        imageTolBackdropDescription.gameObject.SetActive(tolSpeciesDescriptionOn);
    }*/

/*public void UpdateHUDUI() {
        if(isObserverMode) {
            panelHUD.SetActive(false);
        }
        else {
            panelHUD.SetActive(true);
        }        
    }*/

/*private void UpdateStatsPanelUI() {
        buttonGraphLifespan.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphBodySizes.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphConsumption.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphPredation.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphNutrients.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphMutation.GetComponent<Image>().color = buttonDisabledColor;

        textStatsGraphLegend.text = "<color=#ff0000ff>Species A -----</color>\n\n<color=#00ff00ff>Species B -----</color>\n\n<color=#0000ffff>Species C -----</color>\n\n<color=#ffffffff>Species D -----</color>";
        
        if(curStatsGraphMode == GraphMode.Lifespan) {
            //statsGraphMatLifespan.SetFloat("_MaxValue", maxLifespanValue);            
            statsPanelTextMaxValue.text = maxLifespanValue.ToString("F0");
            textStatsGraphTitle.text = "Average Lifespan Over Time:";
            buttonGraphLifespan.GetComponent<Image>().color = buttonActiveColor;            
            imageStatsGraphDisplay.material = statsGraphMatLifespan;
        }

        if(curStatsGraphMode == GraphMode.BodySizes) {
            //statsGraphMatBodySizes.SetFloat("_MaxValue", maxBodySizeValue);            
            statsPanelTextMaxValue.text = maxBodySizeValue.ToString("F2");
            textStatsGraphTitle.text = "Average Creature Body Sizes Over Time:";
            buttonGraphBodySizes.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatBodySizes;
        }

        if(curStatsGraphMode == GraphMode.Consumption) {
            //statsGraphMatFoodEaten.SetFloat("_MaxValue", maxFoodEatenValue);
            statsPanelTextMaxValue.text = maxFoodEatenValue.ToString("F4");
            textStatsGraphTitle.text = "Average Food Eaten Over Time:";
            buttonGraphConsumption.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatFoodEaten;
        }

        if(curStatsGraphMode == GraphMode.Predation) {
            //statsGraphMatPredation.SetFloat("_MaxValue", maxPredationValue);
            statsPanelTextMaxValue.text = maxPredationValue.ToString("F2");
            textStatsGraphTitle.text = "Average Predatory Behavior Over Time:";
            buttonGraphPredation.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatPredation;
        }
        
        if(curStatsGraphMode == GraphMode.Nutrients) {
            statsGraphMatNutrients.SetFloat("_MaxValue", maxNutrientsValue);            
            statsPanelTextMaxValue.text = maxNutrientsValue.ToString("F2");
            textStatsGraphTitle.text = "Global Nutrient Levels Over Time:";
            buttonGraphNutrients.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatNutrients;

            // ** CHANGE GRAPH LEGEND!!! ***
            textStatsGraphLegend.text = "<color=#ff8800ff>Small -----</color>\n\n<color=#88ff00ff>Medium -----</color>";
        }

        if(curStatsGraphMode == GraphMode.Mutation) {
            statsGraphMatMutation.SetFloat("_MaxValue", maxMutationValue);            
            statsPanelTextMaxValue.text = maxMutationValue.ToString("F2");
            textStatsGraphTitle.text = "Average Mutation Rates Over Time:";
            buttonGraphMutation.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatMutation;

            textStatsGraphLegend.text = "<color=#ffffffff>Mutation -----</color>";
        }

        int numBorn = gameManager.simulationManager.numAgentsBorn + gameManager.simulationManager._NumAgents;
        textStatsCurGen.text = "Generation:\n" + gameManager.simulationManager.curApproxGen.ToString() + "\n\n# Born:\n" + numBorn.ToString();
    }
    */

/*private void UpdateFeedToolPanelUI() {
        if(foodToolSprinkleOn) {
            buttonFeedToolSprinkle.GetComponent<Image>().color = buttonActiveColor;
        }
        else {
            buttonFeedToolSprinkle.GetComponent<Image>().color = buttonDisabledColor;
        }

        if(foodToolPourOn) {
            buttonFeedToolPour.GetComponent<Image>().color = buttonActiveColor;
        }
        else {
            buttonFeedToolPour.GetComponent<Image>().color = buttonDisabledColor;
        }
    }
    private void UpdateMutateToolPanelUI() {


    }*/

/*public void UpdateStatsTextureNutrients(List<Vector4> data) {
        statsTextureNutrients.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i].x);
            maxValue = Mathf.Max(maxValue, data[i].y);
            maxValue = Mathf.Max(maxValue, data[i].z);
            maxValue = Mathf.Max(maxValue, data[i].w);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureNutrients.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTextureNutrients.Apply();

        maxNutrientsValue = maxValue;    
    }
    public void UpdateStatsTextureMutation(List<float> data) {
        statsTextureMutation.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i]);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureMutation.SetPixel(i, 0, new Color(data[i], data[i], data[i], data[i]));
        }
        statsTextureMutation.Apply();

        maxMutationValue = maxValue;  
    }
    */
    /*public void UpdateScoreText(int score) {
        textScore.text = "Score: " + score.ToString();
    }*/

/*
    public void ClickToolsButtonOpenClose() {
        animatorToolsPanel.enabled = true;
        if(isActiveToolsPanel) {
            Debug.Log("ClickToolsButton - deactivate"); 
            isActiveToolsPanel = false;
            animatorToolsPanel.Play("SlideOffPanelTools");
            buttonToolOpenClose.GetComponent<Image>().color = buttonDisabledColor;
        }
        else {
            Debug.Log("ClickToolsButton - activate");
            isActiveToolsPanel = true;
            animatorToolsPanel.Play("SlideOnPanelTools");

            buttonToolOpenClose.GetComponent<Image>().color = buttonActiveColor;
        }
    }
    public void ClickStatsButton() {
        //Debug.Log("ClickStatsButton");
        animatorStatsPanel.enabled = true;
        if(isActiveStatsPanel) {
            Debug.Log("ClickStatsButton - deactivate");
            isActiveStatsPanel = false;
            animatorStatsPanel.Play("SlideOffPanelStats");
            buttonStats.GetComponent<Image>().color = buttonDisabledColor;
        }
        else {
            Debug.Log("ClickStatsButton - activate");
            isActiveStatsPanel = true;
            animatorStatsPanel.Play("SlideOnPanelStats");
            buttonStats.GetComponent<Image>().color = buttonActiveColor;
        }
    }
    public void ClickGraphButtonLifespan() {
        if (curStatsGraphMode != GraphMode.Lifespan) {
            curStatsGraphMode = GraphMode.Lifespan;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonBodySizes() {
        if (curStatsGraphMode != GraphMode.BodySizes) {
            curStatsGraphMode = GraphMode.BodySizes;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonFoodEaten() {
        if (curStatsGraphMode != GraphMode.Consumption) {
            curStatsGraphMode = GraphMode.Consumption;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonPredation() {
        if (curStatsGraphMode != GraphMode.Predation) {
            curStatsGraphMode = GraphMode.Predation;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonNutrients() {
        if (curStatsGraphMode != GraphMode.Nutrients) {
            curStatsGraphMode = GraphMode.Nutrients;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonMutation() {
        if (curStatsGraphMode != GraphMode.Mutation) {
            curStatsGraphMode = GraphMode.Mutation;
            UpdateStatsPanelUI();
        }
    }
    */

    /*public void ClickFeedToolSprinkle() {
        
        if(foodToolSprinkleOn) {
            // already on
        }
        else {
            foodToolSprinkleOn = true;
            foodToolPourOn = false;
            Debug.Log("ClickFeedToolSprinkle()! foodToolSprinkleOn = true");
        }
        UpdateFeedToolPanelUI();
    }
    public void ClickFeedToolPour() {
        
        if(foodToolPourOn) {
            // already on
        }
        else {
            foodToolPourOn = true;
            foodToolSprinkleOn = false;
            Debug.Log("ClickFeedToolPour()! foodToolPourOn = true");
        }
        UpdateFeedToolPanelUI();
    }
    public void ClickFeedToolRegrowthSlider(float val) {
        Debug.Log("ClickFeedToolRegrowthSlider()!");
    }
    public void ClickMutateToolGlobalRateSlider(float val) {
        Debug.Log("ClickMutateToolGlobalRateSlider(" + val.ToString() + ")!");
        //gameManager.simulationManager.curPlayerMutationRate = val;
        //gameManager.simulationManager.ChangeGlobalMutationRate(val); // normalizedVal);
    }
    public void ClickToolButtonMutate() {
            //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Mutate) {
            curActiveTool = ToolType.Mutate;

            isActiveMutateToolPanel = true;    
            //animatorMutateToolPanel.enabled = true;
            //animatorMutateToolPanel.Play("SlideOnPanelMutateTool"); 
            buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor; 
                        
            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffNutrientsTool();
            TurnOffRemoveTool();
            // *** make these their own private functions for reuse: OLD:::
            //buttonToolStir.GetComponent<Image>().color = buttonDisabledColor;  
            //buttonToolFeed.GetComponent<Image>().color = buttonDisabledColor;
        //}            
    }
    */

/*public void ClickBackToMainMenu() {
        optionsMenuOn = false;
        UpdateMainMenuUI();
    }*/

/*public void ClickResetWorld() {
        Debug.Log("Reset The World!");
        gameManager.simulationManager.ResetWorld();
    }*/

/*public void ClickButtonToggleHUD() {
        isActiveHUD = !isActiveHUD;
        panelHUD.SetActive(isActiveHUD);
    }*/

/*public void ClickPrevSpecies() {
        Debug.Log("ClickPrevSpecies");
        int newIndex = cameraManager.targetCritterIndex - gameManager.simulationManager._NumAgents / 4;
        if(newIndex < 0) {
            newIndex = newIndex + gameManager.simulationManager._NumAgents;                    
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);      
    }
    public void ClickNextSpecies() {
        Debug.Log("ClickNextSpecies");
        int newIndex = cameraManager.targetCritterIndex + gameManager.simulationManager._NumAgents / 4;
        if(newIndex >= gameManager.simulationManager._NumAgents) {
            newIndex = newIndex - gameManager.simulationManager._NumAgents;                      
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);    
    }*/

/*public void ClickTreeOfLifeGroupOnOff() {
        treeOfLifePanelOn = !treeOfLifePanelOn;

        // Update panel:
        if(treeOfLifePanelOn) {
            // Update treeOfLife colliders & display   
            UpdateSpeciesTreeDataTextures(gameManager.simulationManager.curSimYear);
            RecalculateTreeOfLifeGraphPanelSizes();
            UpdateTolSpeciesColorUI();
            //ClickToolButtonInspect();
            //gfhdf
        }
        else {
            //ClickToolButtonStir();
        }
        
        //panelTreeOfLifeScaleHideGroup.SetActive(treeOfLifePanelOn);
        panelTolMaster.SetActive(treeOfLifePanelOn);

        treeOfLifeManager.UpdateVisualUI(treeOfLifePanelOn);
    }*/

    /*public void ClickTolWorldStatsOnOff() {
        tolWorldStatsOn = !tolWorldStatsOn;
        panelTolWorldStats.SetActive(tolWorldStatsOn && tolEventsTimelineOn);
        RecalculateTreeOfLifeGraphPanelSizes();
    }
    public void ClickTolSpeciesTreeOnOff() {
        //tolSpeciesTreeOn = !tolSpeciesTreeOn;
        //panelTolSpeciesTree.SetActive(tolSpeciesTreeOn);
        //RecalculateTreeOfLifeGraphPanelSizes();
    }
    public void ClickTolEventsTimelineOnOff() {
        tolEventsTimelineOn = !tolEventsTimelineOn;

        panelTolWorldStats.SetActive(tolWorldStatsOn && tolEventsTimelineOn);
        panelTolSpeciesTree.SetActive(tolEventsTimelineOn);
        panelTolEventsTimeline.SetActive(tolEventsTimelineOn);
        // backdrop on/off

        RecalculateTreeOfLifeGraphPanelSizes();
    }
    */
    /*
    private void RecalculateTreeOfLifeGraphPanelSizes() {
        
        
        if(tolSpeciesTreeOn) {
            

            if (tolEventsTimelineOn) {
                

                if (tolWorldStatsOn) {  // ALL ON
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 0.1f;

                    tolGraphCoordsStatsStart = 0.575f;
                    tolGraphCoordsStatsRange = 0.175f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.46f;
                }
                else {  // Species + Events:
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 0.1f;

                    tolGraphCoordsStatsStart = 0.75f;
                    tolGraphCoordsStatsRange = 0.025f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.675f;
                }
            }
            else {
                if (tolWorldStatsOn) {  // species + world Stats
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 0.1f;

                    tolGraphCoordsStatsStart = 0.5f;
                    tolGraphCoordsStatsRange = 0.25f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.75f;

                }
                else {  // species only
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 1f;

                    tolGraphCoordsStatsStart = 1f;
                    tolGraphCoordsStatsRange = 0.025f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.75f;
                }
            }
        }
        else {
            if (tolEventsTimelineOn) {
                

                if (tolWorldStatsOn) {  // World Stats + TEvents Timeline!
                    tolGraphCoordsEventsStart = 0.8f;
                    tolGraphCoordsEventsRange = 0.2f;

                    tolGraphCoordsStatsStart = 0.33f;
                    tolGraphCoordsStatsRange = 0.333f;

                    tolGraphCoordsSpeciesStart = 0f;
                    tolGraphCoordsSpeciesRange = 0.75f;

                }
                else {  // Events Timeline only:
                    tolGraphCoordsEventsStart = 0.8f;
                    tolGraphCoordsEventsRange = 0.2f;

                    tolGraphCoordsStatsStart = 0.5f;
                    tolGraphCoordsStatsRange = 0.025f;

                    tolGraphCoordsSpeciesStart = 0f;
                    tolGraphCoordsSpeciesRange = 0.75f;
                }
            }
            else {
                if (tolWorldStatsOn) {  // world Stats only
                    tolGraphCoordsEventsStart = 0.8f;
                    tolGraphCoordsEventsRange = 0.2f;

                    tolGraphCoordsStatsStart = 0.7f;
                    tolGraphCoordsStatsRange = 0.3f;

                    tolGraphCoordsSpeciesStart = 0f;
                    tolGraphCoordsSpeciesRange = 0.75f;

                }
            }
        }
    }*/

    /*public void ClickTolSpeciesTreeExtinctToggle() {
        tolSpeciesTreeExtinctOn = !tolSpeciesTreeExtinctOn;
    }
    public void ClickTolBrainDisplayToggle() {
        tolBrainDisplayOn = !tolBrainDisplayOn;
    }
    public void ClickTolInspectOnOff() {
        tolInspectOn = !tolInspectOn;

        if(tolInspectOn) {
            ClickToolButtonStir();
        }
        else {
            ClickToolButtonInspect();
        }
    }

    public void ClickTolWorldStatsCyclePrev() {
        tolSelectedWorldStatsIndex--;
        if(tolSelectedWorldStatsIndex < 0) {
            tolSelectedWorldStatsIndex = 10;
        }
        UpdateTolWorldStatsTextReadout();
        //textTolWorldStatsName.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString();
        //textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");
    }
    public void ClickTolWorldStatsCycleNext() {
        tolSelectedWorldStatsIndex++;
        if(tolSelectedWorldStatsIndex > 10) {
            tolSelectedWorldStatsIndex = 0;
        }
        UpdateTolWorldStatsTextReadout();
        //textTolWorldStatsName.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString();
        //textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");
    }
    public void ClickTolSpeciesTreeCyclePrev() {
        tolSelectedSpeciesStatsIndex--;
        if(tolSelectedSpeciesStatsIndex < 0) {
            tolSelectedSpeciesStatsIndex = 15;
        }
        //textTolSpeciesStatsName.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString();
        //UpdateTolSpeciesStatsTextReadout()
        UpdateTolSpeciesStatsTextReadout();
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");
    }
    public void ClickTolSpeciesTreeCycleNext() {
        tolSelectedSpeciesStatsIndex++;
        if(tolSelectedSpeciesStatsIndex > 15) {
            tolSelectedSpeciesStatsIndex = 0;
        }
        //textTolSpeciesStatsName.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString();
        UpdateTolSpeciesStatsTextReadout();
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");
    }
    public void ClickTolEventsTimelineMinor() {
        Debug.Log("ClickTolEventsTimelineMinor()");
    }
    public void ClickTolEventsTimelineMajor() {
        Debug.Log("ClickTolEventsTimelineMajor()");
    }
    public void ClickTolEventsTimelineExtreme() {
        Debug.Log("ClickTolEventsTimelineExtreme()");
    }

    public void ClickEventMinor() {
        if(isActiveEventsMinor) {
            isActiveEventsMinor = false;
            isActiveEventSelectionScreen = false;
        }
        else {
            isActiveEventsMinor = true;
            isActiveEventSelectionScreen = true;
            isActiveEventsMajor = false;
            isActiveEventsExtreme = false;
        }
        UpdateSimEventsUI();
    }
    public void ClickEventMajor() {
        if(isActiveEventsMajor) {
            isActiveEventsMajor = false;
            isActiveEventSelectionScreen = false;
        }
        else {
            isActiveEventsMajor = true;
            isActiveEventSelectionScreen = true;
            isActiveEventsMinor = false;
            isActiveEventsExtreme = false;
        }
        UpdateSimEventsUI();
    }
    public void ClickEventExtreme() {
        if(isActiveEventsExtreme) {
            isActiveEventsExtreme = false;
            isActiveEventSelectionScreen = false;
        }
        else {
            isActiveEventsExtreme = true;
            isActiveEventSelectionScreen = true;
            isActiveEventsMinor = false;
            isActiveEventsMajor = false;            
        }
        UpdateSimEventsUI();
    }
    public void ClickEventReroll() {
        if(isActiveEventsMinor) {
            gameManager.simulationManager.simEventsManager.RegenerateAvailableMinorEvents(gameManager.simulationManager);
        }
        if(isActiveEventsMajor) {
            gameManager.simulationManager.simEventsManager.RegenerateAvailableMajorEvents(gameManager.simulationManager);
        }
        if(isActiveEventsExtreme) {
            gameManager.simulationManager.simEventsManager.RegenerateAvailableExtremeEvents(gameManager.simulationManager);
        }
    }
    */
    /*public void ClickShowHideTolSpeciesDescription() {
        tolSpeciesDescriptionOn = !tolSpeciesDescriptionOn;
        textTolDescription.gameObject.SetActive(tolSpeciesDescriptionOn);
    }*/

    /*public void ClickTolPrevSpecies() {
        Debug.Log("ClickTolPrevSpecies()");
        int curSpeciesIndex = treeOfLifeManager.selectedID;

        int prevSpeciesIndex = curSpeciesIndex - 1;
        if(prevSpeciesIndex < 1) {
            prevSpeciesIndex = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1;
        }

        //treeOfLifeManager.selectedID = prevSpeciesIndex;
        ClickOnSpeciesNode(prevSpeciesIndex);
        //textTolSpeciesIndex.text = treeOfLifeManager.selectedID.ToString();
        
        // Update COLORS!!!
        //Vector3 speciesHuePrimary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[treeOfLifeManager.selectedID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        //imageTolPortraitBorder.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
        //imageTolSpeciesIndex.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
    }
    public void ClickTolNextSpecies() {
        Debug.Log("ClickTolNextSpecies()");
        int curSpeciesIndex = treeOfLifeManager.selectedID;
        
        int nextSpeciesIndex = curSpeciesIndex + 1;
        if(nextSpeciesIndex > (gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1)) {
            nextSpeciesIndex = 1;
        }

        //treeOfLifeManager.selectedID = nextSpeciesIndex;
        ClickOnSpeciesNode(nextSpeciesIndex);
        //textTolSpeciesIndex.text = treeOfLifeManager.selectedID.ToString();

        // Update COLORS!!!
        //Vector3 speciesHuePrimary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[treeOfLifeManager.selectedID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        //imageTolPortraitBorder.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
        //imageTolSpeciesIndex.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
    }
    */
    /*
    public void HoverOverTolGraphRenderPanel() {
        //Debug.Log("HoverOverTolGraphRenderPanel " + tolMouseCoords.ToString());

        Ray ray = new Ray(new Vector3(tolMouseCoords.x, tolMouseCoords.y, -1f), new Vector3(0f, 0f, 1f)); // cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //int layerMask = 0;
        Physics.Raycast(ray, out hit);
                
        
        if(hit.collider != null) {
            // How to handle multiple hits? UI Trumps environmental?
            TreeOfLifeNodeRaycastTarget speciesNodeRayTarget = hit.collider.gameObject.GetComponent<TreeOfLifeNodeRaycastTarget>();

            if(speciesNodeRayTarget != null) {
                selectedSpeciesID = speciesNodeRayTarget.speciesRef.speciesID;                
                treeOfLifeManager.HoverOverSpeciesNode(selectedSpeciesID);                
            }
            else {
                treeOfLifeManager.HoverAllOff();
            }            
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            treeOfLifeManager.HoverAllOff();
        }
    }
    public void ClickOnTolGraphRenderPanel(BaseEventData eventData) {
        PointerEventData pointerData = (PointerEventData)eventData;
        //Debug.Log("ClickOnTolGraphRenderPanel " + pointerData.pointerPressRaycast.ToString());
        Vector2 localPoint = Vector2.zero;
        RectTransform rectTransform = pointerData.pointerPressRaycast.gameObject.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);

        Vector2 uvCoord = new Vector2((localPoint.x + 230f) / 460f, (localPoint.y + 160f) / 320f);
        tolMouseCoords = uvCoord;
        tolMouseOver = 1f;
        Debug.Log("ClickOnTolGraphRenderPanel " + uvCoord.ToString());

        // Raycast Attempt:
        //Vector3 camPos = cameraManager.gameObject.transform.position;        
        //float mouseRatioX = Input.mousePosition.x / Screen.width;
        //float mouseRatioY = Input.mousePosition.y / Screen.height;
        //Vector3 mousePos = new Vector3(mouseRatioX, mouseRatioY, -1f);
        Ray ray = new Ray(new Vector3(uvCoord.x, uvCoord.y, -1f), new Vector3(0f, 0f, 1f)); // cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //int layerMask = 0;
        Physics.Raycast(ray, out hit);

        bool clicked = true;
        
        if(hit.collider != null) {
            // How to handle multiple hits? UI Trumps environmental?
            TreeOfLifeNodeRaycastTarget speciesNodeRayTarget = hit.collider.gameObject.GetComponent<TreeOfLifeNodeRaycastTarget>();

            if(speciesNodeRayTarget != null) {
                selectedSpeciesID = speciesNodeRayTarget.speciesRef.speciesID;
                if(clicked) {
                    ClickOnSpeciesNode(selectedSpeciesID);                    
                }
                else {
                    //treeOfLifeManager.HoverOverSpeciesNode(selectedSpeciesID);
                }
            }
            else {
                //treeOfLifeManager.HoverAllOff();
            }            
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            //treeOfLifeManager.HoverAllOff();
        }
    }
    */
    /*
    public void MouseOverTolGraphRenderPanel(BaseEventData eventData) {
        PointerEventData pointerData = (PointerEventData)eventData;
        //Debug.Log("ClickOnTolGraphRenderPanel " + pointerData.pointerPressRaycast.ToString());
        Vector2 localPoint = Vector2.zero;
        RectTransform rectTransform = pointerData.pointerPressRaycast.gameObject.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);

        Vector2 uvCoord = new Vector2((localPoint.x + 230f) / 460f, (localPoint.y + 160f) / 320f);
        tolMouseCoords = uvCoord;
        tolMouseOver = 1f;
        Debug.Log("ClickOnTolGraphRenderPanel " + uvCoord.ToString());
    }*/
    /*
    public void MouseEnterTolGraphRenderPanel(BaseEventData eventData) {        
        Debug.Log("MouseEnterTolGraphRenderPanel");
        tolMouseOver = 1f;
    }
    public void MouseExitTolGraphRenderPanel(BaseEventData eventData) {        
        Debug.Log("MouseExitTolGraphRenderPanel");
        tolMouseOver = 0f;
    }
    */
    /*public void UpdateTolGraphCursorTimeSelectUI(Vector2 coords) {
        // find closest event:
        if(gameManager.simulationManager.simEventsManager.completeEventHistoryList.Count > 0) {
            float closestEventDistance = 1f;
            int closestEventIndex = 0;

            for(int i = 0; i < gameManager.simulationManager.simEventsManager.completeEventHistoryList.Count; i++) {
                float eventCoord = (float)gameManager.simulationManager.simEventsManager.completeEventHistoryList[i].timeStepActivated / (float)gameManager.simulationManager.simAgeTimeSteps;

                float eventDistance = Mathf.Abs(coords.x - eventCoord);

                if(eventDistance < closestEventDistance) {
                    closestEventIndex = i;
                    closestEventDistance = eventDistance;
                }
            }
            //Debug.Log("Closest Event: " + closestEventIndex.ToString() + ",  " + gameManager.simulationManager.simEventsManager.completeEventHistoryList[closestEventIndex].name);

            curClosestEventToCursor = closestEventIndex;
        }

        


        int selectedSpeciesID = treeOfLifeManager.selectedID;
        SpeciesGenomePool selectedPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        int sampleYear = Mathf.FloorToInt(coords.x * (float)gameManager.simulationManager.curSimYear);
        sampleYear = Mathf.Clamp(sampleYear, 0, selectedPool.avgLifespanPerYearList.Count - 1);
        
        //int index = t - speciesPool.yearCreated;
        //int index = sampleYear;
        float valStat = 0f;
        float a = tolSelectedSpeciesStatsIndex;
        // I know there's a better way to do this:
        if(a == 0) {
            valStat = selectedPool.avgLifespanPerYearList[sampleYear];
        }
        else if(a == 1) {
            valStat = selectedPool.avgConsumptionDecayPerYearList[sampleYear];
        }
        else if(a == 2) {
            valStat = selectedPool.avgConsumptionPlantPerYearList[sampleYear];
        }
        else if(a == 3) {
            valStat = selectedPool.avgConsumptionMeatPerYearList[sampleYear];
        }
        else if(a == 4) {
            valStat = selectedPool.avgBodySizePerYearList[sampleYear];
        }
        else if(a == 5) {
            valStat = selectedPool.avgSpecAttackPerYearList[sampleYear];
        }
        else if(a == 6) {
            valStat = selectedPool.avgSpecDefendPerYearList[sampleYear];
        }
        else if(a == 7) {
            valStat = selectedPool.avgSpecSpeedPerYearList[sampleYear];
        }
        else if(a == 8) {
            valStat = selectedPool.avgSpecUtilityPerYearList[sampleYear];
        }
        else if(a == 9) {
            valStat = selectedPool.avgFoodSpecDecayPerYearList[sampleYear];
        }
        else if(a == 10) {
            valStat = selectedPool.avgFoodSpecPlantPerYearList[sampleYear];
        }
        else if(a == 11) {
            valStat = selectedPool.avgFoodSpecMeatPerYearList[sampleYear];
        }
        else if(a == 12) {
            valStat = selectedPool.avgNumNeuronsPerYearList[sampleYear];
        }
        else if(a == 13) {
            valStat = selectedPool.avgNumAxonsPerYearList[sampleYear];
        }
        else if(a == 14) {
            valStat = selectedPool.avgExperiencePerYearList[sampleYear];
        }
        else if(a == 15) {
            valStat = selectedPool.avgFitnessScorePerYearList[sampleYear];
        }
        else if(a == 16) {
            valStat = selectedPool.avgDamageDealtPerYearList[sampleYear];
        }
        else if(a == 17) {
            valStat = selectedPool.avgDamageTakenPerYearList[sampleYear];
        }
        else if(a == 18) {
            valStat = selectedPool.avgLifespanPerYearList[sampleYear];
        }

        curSpeciesStatValue = valStat;

        //maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);
                        
        //statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                    //}                    
                //}
            //}            
        //}
        
        
        
        
        // Move Text-boxes to position of vertical mouse line
        // update text values
        // Find graph values at uv coord???  // query GPU or save info on CPU??
        //turn on/off panel in enter/exit functions
        
        //RectTransform rectSpeciesStats = textTolSpeciesStatsValue.GetComponent<RectTransform>();
        //rectSpeciesStats.localPosition = new Vector3(coords.x + 220f, coords.y + 10f, 0f);
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");

        RectTransform rectWorldStats = textTolWorldStatsValue.GetComponent<RectTransform>();
        rectWorldStats.localPosition = new Vector3(coords.x + 220f, coords.y + 20f, 0f);
        textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");

        RectTransform rectEventStats = textTolEventsTimelineName.GetComponent<RectTransform>();
        rectEventStats.localPosition = new Vector3(coords.x + 220f, coords.y + 30f, 0f);
        textTolEventsTimelineName.text = "EVENT";
        
    }*/


/*public void ClickInfoTabResources() {
        isActiveInfoResourcesTab = true;        
    }
    public void ClickInfoTabSpecies() {
        isActiveInfoResourcesTab = false;     
    }

    public void ClickInfoSpeciesDecomposers() {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 0;
        infoSpeciesSelectedTier = 0;
        infoSpeciesSelectedSlot = 0;

        textInfoSpeciesName.text = "Decomposers";
    }
    public void ClickInfoSpeciesAlgae() {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 1;
        infoSpeciesSelectedTier = 0;
        infoSpeciesSelectedSlot = 0;

        textInfoSpeciesName.text = "Algae";
    }
    public void ClickInfoSpeciesZooplankton() {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 2;
        infoSpeciesSelectedTier = 0;
        infoSpeciesSelectedSlot = 0;

        textInfoSpeciesName.text = "Zooplankton";
    }
    public void ClickInfoSpeciesAgent(int slotID) {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 2;
        infoSpeciesSelectedTier = 1;
        infoSpeciesSelectedSlot = slotID;

        textInfoSpeciesName.text = "Animal " + slotID.ToString();

        treeOfLifeManager.ClickedOnSpeciesNode(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[slotID].linkedSpeciesID); //slotID); // find proper ID
        UpdateTolSpeciesColorUI();
    }

    */

// OLD:

/*public void UpdateSimEventsUI() {
    SimEventsManager eventsManager = gameManager.simulationManager.simEventsManager;

    panelEventSelectionScreen.SetActive(isActiveEventSelectionScreen);

    textEventPointsWallet.text = "$" + gameManager.simulationManager.simEventsManager.curEventBucks.ToString();

    // Check for cooldown and currency to determine whether ot unlock menus:
    buttonEventsMinor.interactable = false;
    imageButtonEventsMinor.color = buttonEventMinorColor;
    buttonEventsMajor.interactable = false;
    imageButtonEventsMajor.color = buttonEventMajorColor;
    buttonEventsExtreme.interactable = false;
    imageButtonEventsExtreme.color = buttonEventExtremeColor;
    if(eventsManager.isCooldown) {
        //buttonEventsMinor.interactable = false;
        //buttonEventsMajor.interactable = false;
        //buttonEventsExtreme.interactable = false;
        imageButtonEventsMinor.color = buttonEventMinorColor * 0.25f;
        imageButtonEventsMajor.color = buttonEventMajorColor * 0.25f;
        imageButtonEventsExtreme.color = buttonEventExtremeColor * 0.25f;
        textRecentEvent.gameObject.SetActive(true);
        textRecentEvent.text = eventsManager.mostRecentEventString;
    }
    else {
        textRecentEvent.gameObject.SetActive(false);
        //textRecentEvent.text = eventsManager.mostRecentEventString;

        if(eventsManager.curEventBucks >= 3) {                
            buttonEventsMinor.interactable = true;
        }
        else {
            imageButtonEventsMinor.color = buttonEventMinorColor * 0.25f;    // also multiplying ALPHA!!! ... annoying              
        }
        if(eventsManager.curEventBucks >= 15) {
            buttonEventsMajor.interactable = true;
        }
        else {
            imageButtonEventsMajor.color = buttonEventMajorColor * 0.25f;
        }
        if(eventsManager.curEventBucks >= 75) {
            buttonEventsExtreme.interactable = true;
        }
        else {
            imageButtonEventsExtreme.color = buttonEventExtremeColor * 0.25f;
        }
    }
    //
    //

    if(isActiveEventSelectionScreen) {
        string titleText = "";
        if(isActiveEventsMinor) {
            titleText = "MINOR EVENTS";

            for(int i = 0; i < simEventComponentsArray.Length; i++) {                    
                if(selectedMinorEventIndex == i) {
                    textEventSelectedDescription.text = gameManager.simulationManager.simEventsManager.availableMinorEventsList[i].description;
                    simEventComponentsArray[i].isSelected = true;
                   // simEventComponentsArray[i]  // if selected -- make bigger or glowy or whatever
                }
                else {
                    simEventComponentsArray[i].isSelected = false;
                }
                simEventComponentsArray[i].UpdateSimEventPanel(this, gameManager.simulationManager.simEventsManager.availableMinorEventsList[i], i);
            }

            imageButtonEventsMinor.color = buttonEventMinorColor * 2f;
            imageEventSelectionTitle.color = buttonEventMinorColor;
        }
        if(isActiveEventsMajor) {
            titleText = "MAJOR EVENTS";

            for(int i = 0; i < simEventComponentsArray.Length; i++) {                    
                if(selectedMajorEventIndex == i) {
                    textEventSelectedDescription.text = gameManager.simulationManager.simEventsManager.availableMajorEventsList[i].description;
                    simEventComponentsArray[i].isSelected = true;
                }
                else {
                    simEventComponentsArray[i].isSelected = false;
                }
                simEventComponentsArray[i].UpdateSimEventPanel(this, gameManager.simulationManager.simEventsManager.availableMajorEventsList[i], i);
            }

            imageButtonEventsMajor.color = buttonEventMajorColor * 2f;
            imageEventSelectionTitle.color = buttonEventMajorColor;
        }
            if(isActiveEventsExtreme) {
            titleText = "EXTREME EVENTS";

            for(int i = 0; i < simEventComponentsArray.Length; i++) {                    
                if(selectedExtremeEventIndex == i) {
                    textEventSelectedDescription.text = gameManager.simulationManager.simEventsManager.availableExtremeEventsList[i].description;  // if selected -- make bigger or glowy or whatever
                    simEventComponentsArray[i].isSelected = true;
                }
                else {
                    simEventComponentsArray[i].isSelected = false;
                }
                simEventComponentsArray[i].UpdateSimEventPanel(this, gameManager.simulationManager.simEventsManager.availableExtremeEventsList[i], i);
            }

            imageButtonEventsExtreme.color = buttonEventExtremeColor * 2f;
            imageEventSelectionTitle.color = buttonEventExtremeColor;
        }
        textEventSelectionTitle.text = titleText;
    }
    else {

    }
}
public void ClickedOnEvent(SimEventComponent eventComponent) {
    if(isActiveEventsMinor) {
        selectedMinorEventIndex = eventComponent.index;
    }
    if(isActiveEventsMajor) {
        selectedMajorEventIndex = eventComponent.index;
    }
    if(isActiveEventsExtreme) {
        selectedExtremeEventIndex = eventComponent.index;
    }
}
public void ClickConfirmEvent() {

    if(isActiveEventsMinor) {
        SimEventData eventData = gameManager.simulationManager.simEventsManager.availableMinorEventsList[selectedMinorEventIndex];
        gameManager.simulationManager.ExecuteSimEvent(eventData);            
        Debug.Log("ClickConfirmEvent(" + selectedMinorEventIndex.ToString() + ") minor");
       // selectedMinorEventIndex = eventComponent.index;
    }
    if(isActiveEventsMajor) {
        SimEventData eventData = gameManager.simulationManager.simEventsManager.availableMajorEventsList[selectedMajorEventIndex];
        gameManager.simulationManager.ExecuteSimEvent(eventData);
        Debug.Log("ClickConfirmEvent(" + selectedMajorEventIndex.ToString() + ") major");

    }
    if(isActiveEventsExtreme) {
        SimEventData eventData = gameManager.simulationManager.simEventsManager.availableExtremeEventsList[selectedExtremeEventIndex];
        gameManager.simulationManager.ExecuteSimEvent(eventData);
        Debug.Log("ClickConfirmEvent(" + selectedExtremeEventIndex.ToString() + ") extreme");
    }

    isActiveEventSelectionScreen = false;
    isActiveEventsMinor = false;
    isActiveEventsMajor = false;
    isActiveEventsExtreme = false;
}
*/

#endregion

