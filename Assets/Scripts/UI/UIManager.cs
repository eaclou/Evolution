using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {

    #region attributes
    public GameManager gameManager;
    //public SimulationManager simManager; - go through gameManager
    public CameraManager cameraManager;
    public GameOptionsManager gameOptionsManager;
    //public TreeOfLifeManager treeOfLifeManager;

    private bool firstTimeStartup = true;

    // Main Menu:
    public Button buttonQuickStartResume;
    public Button buttonNewSimulation;
    public Text textMouseOverInfo;
    //public Texture2D healthDisplayTex;
    public GameObject panelTitleMenu;
    public GameObject panelGameOptions;

    //public GameObject panelTint;
    public bool controlsMenuOn = false; // main menu
    public bool optionsMenuOn = false;  // Game options main menu

    public Text textLoadingTooltips;

    /*public GameObject panelStatsHUD;
    public Animator animatorStatsPanel;
    public Button buttonStats;
    public bool isActiveStatsPanel = false;
    */
    public ToolType curActiveTool;
    public enum ToolType {
        None,
        Watcher,
        Add,
        Stir,
        Mutate,
        Remove,
        Resources,
        Sage
    }

    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.75f, 0.75f, 0.75f, 1f);

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
    

    public GameObject mouseRaycastWaterPlane;
    private Vector3 prevMousePositionOnWaterPlane;
    public Vector3 curMousePositionOnWaterPlane;

    //public Image imageHighlightNewUI;

    public Text textInspectReadout;

    // &&& INFO PANEL &&& !!!! ==============================================
    public bool isActiveInfoPanel = false;
    public bool isActiveInfoResourcesTab = true;  // toggle between resources tab and species tab
    public GameObject panelInfoExpanded;
    public Text textGlobalMass;
    public Button buttonInfoTime;
    public Button buttonInfoOpenClose;
    public Button buttonTabResourcesOverview;
    public Button buttonTabSpeciesOverview;
    public Text textCurYear;
    public Material infoMeterOxygenMat;
    public Material infoMeterNutrientsMat;
    public Material infoMeterDetritusMat;
    public Material infoMeterDecomposersMat;
    public Material infoMeterPlantsMat;
    public Material infoMeterAnimalsMat;
    //public Image imageMeterDecomposersIcon;
    //public Image imageMeterPlantsIcon;
    //public Image imageMeterAnimalsIcon;
    public Text textMeterOxygen;
    public Text textMeterNutrients;
    public Text textMeterDetritus;
    public Text textMeterDecomposers;
    public Text textMeterAlgae;
    public Text textMeterPlants;
    public Text textMeterZooplankton;
    public Text textMeterAnimals;

    // Info Expanded: Resources Overview:
    public GameObject panelInfoResourcesOverview;
    private Texture2D infoOxygenDataTexture;
    private Texture2D infoNutrientsDataTexture;
    private Texture2D infoDetritusDataTexture;
    private Texture2D infoDecomposersDataTexture;
    private Texture2D infoPlantsDataTexture;
    private Texture2D infoAnimalsDataTexture;
    public Material knowledgeGraphOxygenMat;
    public Material knowledgeGraphNutrientsMat;
    public Material knowledgeGraphDetritusMat;
    public Material knowledgeGraphDecomposersMat;
    public Material knowledgeGraphAlgaeMat;
    public Material knowledgeGraphPlantsMat;
    public Material knowledgeGraphZooplanktonMat;
    public Material knowledgeGraphVertebratesMat;

    public Image knowledgeGraphVertebrateLifespan;
    public Image knowledgeGraphVertebratePopulation;
    public Image knowledgeGraphVertebrateFoodEaten;
    public Image knowledgeGraphVertebrateGenome;
    public Text textKnowledgeGraphLifespan;
    public Text textKnowledgeGraphPopulation;
    public Text textKnowledgeGraphFoodEaten;
    public Text textKnowledgeGraphGenome;
    public Material knowledgeGraphVertebrateLifespanMat0;
    public Material knowledgeGraphVertebratePopulationMat0;
    public Material knowledgeGraphVertebrateFoodEatenMat0;
    public Material knowledgeGraphVertebrateGenomeMat0;
    public Material knowledgeGraphVertebrateLifespanMat1;
    public Material knowledgeGraphVertebratePopulationMat1;
    public Material knowledgeGraphVertebrateFoodEatenMat1;
    public Material knowledgeGraphVertebrateGenomeMat1;
    public Material knowledgeGraphVertebrateLifespanMat2;
    public Material knowledgeGraphVertebratePopulationMat2;
    public Material knowledgeGraphVertebrateFoodEatenMat2;
    public Material knowledgeGraphVertebrateGenomeMat2;
    public Material knowledgeGraphVertebrateLifespanMat3;
    public Material knowledgeGraphVertebratePopulationMat3;
    public Material knowledgeGraphVertebrateFoodEatenMat3;
    public Material knowledgeGraphVertebrateGenomeMat3;

    public Material debugTextureViewerMat;
    
    // announcements:
    private bool announceAlgaeCollapsePossible = false;
    private bool announceAlgaeCollapseOccurred = false;
    private bool announceAgentCollapsePossible = false;
    private bool announceAgentCollapseOccurred = false;

    public TrophicSlot unlockedAnnouncementSlotRef;

    public bool isSpiritBrushSelected = true;
    // &&& PLAYER TOOLBAR &&& !!!! ==============================================
    public bool isActiveStirToolPanel = false;
    public bool isToolbarPaletteExpandOn = true;
    public bool isToolbarDetailPanelOn = true;
    public int curToolbarWingPanelSelectID = 0;  // 0 == description, 1 == stats, 2 == mutate/upgrade

    public bool isKnowledgePanelOn = false;
    
    public bool isToolbarDeletePromptOn = false;
    public int timerAnnouncementTextCounter = 0;
    public bool isAnnouncementTextOn = false;
    public bool isUnlockCooldown = false;
    public int unlockCooldownCounter = 0;
    public bool recentlyCreatedSpecies = false;
    public int recentlyCreatedSpeciesTimeStepCounter = 0;
    private bool inspectToolUnlockedAnnounce = false;
    // portrait
    //public int curToolUnlockLevel = 0;
    public float toolbarInfluencePoints = 0.5f;
    public Text textInfluencePointsValue;
    public Material infoMeterInfluencePointsMat;
    private float addSpeciesInfluenceCost = 0.33f;

    private Button buttonPendingTrophicSlot;  // 
    private Button buttonSelectedTrophicSlot;

    public GameObject panelKnowledgeSpiritBase;
    public GameObject panelKnowledgeInfoWorld;
    public GameObject panelKnowledgeInfoDecomposers;
    public GameObject panelKnowledgeInfoAlgae;
    public GameObject panelKnowledgeInfoPlants;
    public GameObject panelKnowledgeInfoZooplankton;
    public GameObject panelKnowledgeInfoVertebrates;

    public Text textSpiritBrushDescription;
    public Text textSpiritBrushEffects;
    public Text textLinkedSpiritDescription;

    public Sprite spriteSpiritBrushKnowledgeIcon;
    public Sprite spriteSpiritBrushMutationIcon;
    public Sprite spriteSpiritBrushStirIcon;
    public Sprite spriteSpiritBrushWatcherIcon;
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

    public GameObject panelToolbarPaletteExpand;
    public Button buttonToolbarPaletteExpandOn;
    
    private bool inspectToolUnlocked = true;

    public Button buttonToolbarWatcher;
    public Image imageToolbarInspectLinkedIcon;
    //public Sprite spriteToolbarInspectButton;
    //public Button buttonToolbarNutrients;
    //public Sprite spriteToolbarStirButton;
    public Button buttonToolbarStir;
    //public Button buttonToolbarMutate;
    //public Sprite spriteToolbarAddButton;
    public Button buttonToolbarAdd;
    public Image imageToolbarAddLinkedIcon;
    //public Button buttonToolbarRemove;
    public Button buttonToolbarKnowledge;
    public Image imageToolbarKnowledgeLinkedIcon;

    /*public GameObject wireSpiritBrush;
    public GameObject wireTerrain;
    public GameObject wireAnimals;
    public GameObject wirePlants;
    public GameObject wireDecomposers;
*/
    public GameObject panelPendingClickPrompt;

    public int selectedToolbarOtherLayer = 0;
    public Button buttonToolbarOther0;  // Minerals
    public Button buttonToolbarOther1;  // Water
    public Button buttonToolbarOther2;  // Air

    public int selectedToolbarTerrainLayer = 0;
    public Button buttonToolbarTerrain0;
    public Button buttonToolbarTerrain1;
    public Button buttonToolbarTerrain2;
    public Button buttonToolbarTerrain3;
    //
    public Button buttonToolbarRemoveDecomposer;
    public Button buttonToolbarDecomposers;
    //
    public Button buttonToolbarRemovePlant;
    public Button buttonToolbarAlgae;
    public Button buttonToolbarPlant1;
    public Button buttonToolbarPlant2;
    //
    public Button buttonToolbarRemoveAnimal;
    public Button buttonToolbarZooplankton;
    public Button buttonToolbarAnimal1;
    public Button buttonToolbarAnimal2;
    public Button buttonToolbarAnimal3;
    public Button buttonToolbarAnimal4;
    //
    public Sprite spriteSpeciesSlotRing;
    public Sprite spriteSpeciesSlotFull;
    public Sprite spriteSpeciesSlotSelected;
    // // WING:::::
    //public GameObject panelToolbarWing;
    //public GameObject panelToolbarWingDescription;
    //public GameObject panelToolbarWingStats;
    //public GameObject panelToolbarWingMutation;
    //public GameObject panelToolbarWingDeletePrompt;
    //public Button buttonToolbarWingDeleteSpecies;
    //public Button buttonToolbarWingDescription;
    //public Button buttonToolbarWingStats;
    //public Button buttonToolbarWingMutation;
    public Material toolbarSpeciesStatsGraphMat;
    public Image imageToolbarSpeciesStatsGraph;
    public Text textToolbarWingStatsUnlockStatus;
    public Text textToolbarWingStatsUnlockPercentage;
    public Image imageUnlockMeter;
    public Material matUnlockMeter;
    public Image imageToolbarButtonBarBackground;
    public Image imageToolbarWingLine;
    public Button buttonToolbarWingCreateSpecies;
    public Text textToolbarWingSpeciesSummary;
    public Text textToolbarWingPanelName;
    public Text textSelectedSpeciesTitle;
    public Text textSelectedSpeciesIndex;
    public Image imageToolbarSpeciesPortraitRender;
    public Image imageToolbarSpeciesPortraitBorder;
    public Text textSelectedSpeciesDescription;
    public int selectedSpeciesStatsIndex;
    public Text textSelectedSpiritBrushName;
    public Image imageToolbarSpiritBrushThumbnail;
    public Image imageToolbarSpiritBrushThumbnailBorder;
    // Mutation Panel elements:
    public Image imageMutationPanelThumbnailA;
    public Image imageMutationPanelThumbnailB;
    public Image imageMutationPanelThumbnailC;
    public Image imageMutationPanelThumbnailD;
    public Text textMutationPanelOptionA;
    public Text textMutationPanelOptionB;
    public Text textMutationPanelOptionC;
    public Text textMutationPanelOptionD;
    public Image imageMutationPanelHighlightA;  // changes upon selection
    public Image imageMutationPanelHighlightB;
    public Image imageMutationPanelHighlightC;
    public Image imageMutationPanelHighlightD;
    public Button buttonMutationReroll;
    public Image imageMutationPanelCurPortrait;
    public Image imageMutationPanelNewPortrait;
    public Text textMutationPanelCur;
    public Text textMutationPanelNew;
    public Text textMutationPanelTitleCur;
    public Text textMutationPanelTitleNew;
    public GameObject panelNewMutationPreview;
    private int selectedToolbarMutationID = 0;
    public Button buttonToolbarMutateConfirm;

    public GameObject panelWatcherSpiritMain;
    public Text TextCommonStatsA;
    private int curWatcherPanelVertebratePageNum;
    public Button buttonWatcherVertebrateCyclePage; // maybe not needed?
    public GameObject panelWatcherSpiritVertebratesHUD; // 0
    public Text textWatcherVertebrateHUD;
    public GameObject panelWatcherSpiritVertebratesText;  // 1
    public Text textWatcherVertebrateText;
    public GameObject panelWatcherSpiritVertebratesGenome; // 2
    public Text textWatcherVertebrateGenome;
    public GameObject panelWatcherSpiritVertebratesBrain; // 3
    public Text textWatcherVertebrateBrain;
    public GameObject panelWatcherSpiritZooplankton;    
    public GameObject panelWatcherSpiritPlants;
    public GameObject panelWatcherSpiritAlgae;
    public GameObject panelWatcherSpiritDecomposers;
    public GameObject panelWatcherSpiritTerrain;
    public Text textWatcherVertebratePageNum;
    public Text textWatcherTargetIndex;
    //public Texture2D textureWorldStats;
    //public Texture2D textureWorldStatsKey;
    //public Vector2[] tolWorldStatsValueRangesKeyArray;
    //public int tolSelectedWorldStatsIndex = 0;
    //public int tolSelectedSpeciesStatsIndex = 0;
    //public Sprite spriteAlgaePortrait;
    //public Sprite spriteDecomposerPortrait;
    //public Sprite spriteZooplanktonPortrait;
    //public Sprite spriteBedrockPortrait;
    //public Sprite spriteStonesPortrait;
    //public Sprite spritePebblesPortrait;
    //public Sprite spriteSandPortrait;

    //Inspect!!!
    public bool isActiveInspectPanel = false;
    public Material inspectWidgetDietMat;
    public Material inspectWidgetStomachFoodMat;
    public Material inspectWidgetEnergyMat;
    public Material inspectWidgetLifeCycleMat;
    public Material inspectWidgetDimensionsMat;
    public Material inspectWidgetHealthMat;
    public Material inspectWidgetSpeciesIconMat;
    public Material inspectWidgetAgentIconMat;
    public Button buttonInspectCyclePrevSpecies;
    public Button buttonInspectCycleNextSpecies;
    public Button buttonInspectCyclePrevAgent;
    public Button buttonInspectCycleNextAgent;
    public Text textStomachContents;
    public Text textEnergy;
    public Text textHealth;
    public Text textDiet;
    public Text textDimensionsWidth;
    public Text textDimensionsLength;
    public Text textSpeciesID;
    public Text textAgentID;
    public Text textLifeCycle;
    public GameObject panelInspectHUD;
    public Animator animatorInspectPanel;
    public Text textInspectData;

    public GameObject panelNewInspect;
    public Text textNewInspectAgentName;
    public Material newInspectAgentEnergyMat;
    public Material newInspectAgentStaminaMat;
    public Material newInspectAgentStomachMat;
    public Material newInspectAgentAgeMat;
    public Material newInspectAgentHealthMat;
    public Material newInspectAgentThrottleMat;
    public Material newInspectAgentCommsMat;
    public Material newInspectAgentStateMat;
    public Material newInspectAgentCurActivityMat;
    public Material newInspectAgentWasteMat;
    public Material newInspectAgentBrainMat;
    public Text textNewInspectLog;

    private Vector4 _Zoom = new Vector4(1f, 1f, 1f, 1f);
    private float _Amplitude = 1f;
    private Vector4 _ChannelMask = new Vector4(1f, 1f, 1f, 1f);
    private int _ChannelSoloIndex = 0;
    private float _IsChannelSolo = 0f;
    private float _Gamma = 1f;
    private int _DebugTextureIndex = 0;
    private string _DebugTextureString = "-";
        
    public Button buttonDebugTexturePrev;
    public Button buttonDebugTextureNext;
    public Slider sliderDebugTextureZoomX;
    public Slider sliderDebugTextureZoomY;
    public Slider sliderDebugTextureAmplitude;
    public Slider sliderDebugTextureSoloChannelIndex;
    public Toggle toggleDebugTextureIsSolo;
    public Slider sliderDebugTextureGamma;
    public Text textDebugTextureName;
    public Text textDebugTextureZoomX;
    public Text textDebugTextureZoomY;
    public Text textDebugTextureAmplitude;
    public Text textDebugTextureSoloChannelIndex;
    public Text textDebugTextureGamma;
    public Image imageDebugTexture;

    private RenderTexture[] debugTextureViewerArray;

    public GameObject panelObserverMode;
    
    public GameObject panelPaused;

    public GameObject panelDebug;
    
    public Text textDebugTrainingInfo1;
    public Text textDebugTrainingInfo2;
    public Text textDebugTrainingInfo3;
    public Text textDebugSimSettings;
    

    public Button buttonToggleDebug;
    public bool isActiveDebug = true;

    public bool isObserverMode = false;
    public bool isPaused = false;

    public GameObject panelMainMenu;
    public GameObject panelLoading;
    public GameObject panelPlaying;
    
    public float loadingProgress = 0f;

    private int[] displaySpeciesIndicesArray;

    public Image imageStatsGraphDisplay;
    public Material statsGraphMatLifespan;
    public Material statsGraphMatBodySizes;
    public Material statsGraphMatFoodEaten;
    public Material statsGraphMatPredation;
    public Material statsGraphMatNutrients;
    public Material statsGraphMatMutation;
    //public Texture2D statsGraphDataTex;
    public Texture2D[] statsTreeOfLifeSpeciesTexArray;
    public Texture2D statsSpeciesColorKey;
    public Texture2D statsTextureLifespan;
    public Texture2D statsTextureBodySizes;
    public Texture2D statsTextureFoodEaten;
    public Texture2D statsTexturePredation;
    public Texture2D statsTextureNutrients;
    public Texture2D statsTextureMutation;
    
    public float[] maxValuesStatArray;

    public float stirStickDepth = 0f;

    public Vector2 smoothedMouseVel;
    private Vector2 prevMousePos;

    public Vector2 smoothedCtrlCursorVel;
    private Vector2 prevCtrlCursorPos;
    private Vector3 prevCtrlCursorPositionOnWaterPlane;
    public Vector3 curCtrlCursorPositionOnWaterPlane;
    private bool rightTriggerOn = false;

    public bool isDraggingMouseLeft = false;
    public bool isDraggingMouseRight = false;
    public bool isDraggingSpeciesNode = false;

    public int selectedSpeciesID;
    public int hoverAgentID;

    private const int maxDisplaySpecies = 32;

    private float curSpeciesStatValue;

    private bool isBrushAddingAgents = false;
    private int brushAddAgentCounter = 0;
    private int framesPerAgentSpawn = 3;

    public bool brainDisplayOn = false;
    
    #endregion


    #region Initialization Functions:::
    // Use this for initialization
    void Start() {

        //animatorStatsPanel.enabled = false;
        animatorInspectPanel.enabled = false;
        //animatorToolsPanel.enabled = false;
        //animatorFeedToolPanel.enabled = false;
        //animatorMutateToolPanel.enabled = false;
        //animatorStirToolPanel.enabled = false;

        //buttonToolStir.GetComponent<Image>().color = buttonDisabledColor;   
        //buttonToolInspect.GetComponent<Image>().color = buttonDisabledColor;        
        //buttonToolFeed.GetComponent<Image>().color = buttonDisabledColor;
        //buttonToolMutate.GetComponent<Image>().color = buttonDisabledColor;

        /*simEventComponentsArray = new SimEventComponent[4];
        simEventComponentsArray[0] = simEvent0;
        simEventComponentsArray[1] = simEvent1;
        simEventComponentsArray[2] = simEvent2;
        simEventComponentsArray[3] = simEvent3;
        */
        ClickToolButtonAdd();
        //buttonToolbarExpandOn.GetComponent<Animator>().StopPlayback();
        //buttonToolbarExpandOn.GetComponent<Animator>().enabled = false;
        //buttonToolbarWatcher.GetComponent<Animator>().enabled = false;

        //buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
        buttonToolbarPaletteExpandOn.interactable = true;

        //ClickToolButtonInspect();  // **** Clean this up! don't mix UI click function with underlying code for initialization
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

        SimEventData newEventData = new SimEventData();
        newEventData.name = "New Simulation Start!";
        newEventData.category = SimEventData.SimEventCategories.NPE;
        newEventData.timeStepActivated = 0;
        gameManager.simulationManager.simEventsManager.completeEventHistoryList.Add(newEventData);

        //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Welcome! This Pond is devoid of life...\nIt's up to you to change that!";
        //panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.75f, 0.75f, 0.75f);
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        //isAnnouncementTextOn = true;
        //timerAnnouncementTextCounter = 0;
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
        Cursor.visible = true;
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
        panelDebug.SetActive(isActiveDebug);
    }
    private void UpdateLoadingUI() {
        Cursor.visible = true;
        if (loadingProgress < 1f) {
            //textLoadingTooltips.text = "( Calculating Enjoyment Coefficients )";
            textLoadingTooltips.text = "( Reticulating Splines )";
        }
        if (loadingProgress < 0.4f) {
            textLoadingTooltips.text = "( Warming Up Simulation Cubes )";
        }
        /*if (loadingProgress < 0.4f) {
            //textLoadingTooltips.text = "( Feeding Hamsters )";
        }
        if (loadingProgress < 0.1f) {
            textLoadingTooltips.text = "( Reticulating Splines )";
        }*/
    }
    private void UpdateSimulationUI() {
        //UpdateScoreText(Mathf.RoundToInt(gameManager.simulationManager.agentsArray[0].masterFitnessScore));

        //SetDisplayTextures();

        UpdateDebugUI();
        //UpdateHUDUI();
        
        UpdateObserverModeUI();  // <== this is the big one        
        UpdatePausedUI();
        //UpdateInspectPanelUI();
        isActiveInspectPanel = false;
        panelInspectHUD.SetActive(false);

        //UpdateInfoPanelUI();
        UpdateToolbarPanelUI();
        UpdateKnowledgePanelUI(gameManager.simulationManager.trophicLayersManager);

        //UpdateStatsPanelUI(); // needed?
        //UpdateInspectPanelUI();  // needed?        

        Vector2 curMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        Vector2 instantMouseVel = curMousePos - prevMousePos;

        smoothedMouseVel = Vector2.Lerp(smoothedMouseVel, instantMouseVel, 0.16f);

        prevMousePos = curMousePos;
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
                StopFollowingAgent();
                StopFollowingPlantParticle();
                StopFollowingAnimalParticle();
            }
            if (moveDir.sqrMagnitude > 0.001f) {
                StopFollowingAgent();
                StopFollowingPlantParticle();
                StopFollowingAnimalParticle();
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
                ClickButtonToggleDebug();
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

            // &&&&&&&&&&&&&&&&& MOUSE: &&&&&&&&&&&&&&&
            bool leftClickThisFrame = Input.GetMouseButtonDown(0);
            bool rightClickThisFrame = Input.GetMouseButtonDown(1);
            bool letGoThisFrameLeft = Input.GetMouseButtonUp(0);
            bool letGoThisFrameRight = Input.GetMouseButtonUp(1);
            isDraggingMouseLeft = Input.GetMouseButton(0);
            isDraggingMouseRight = Input.GetMouseButton(1);
            if (letGoThisFrameLeft) {
                isDraggingMouseLeft = false;
                isDraggingSpeciesNode = false;
                //gameManager.simulationManager.isBrushingAgents = false;
            }
            if (letGoThisFrameRight) {
                isDraggingMouseRight = false;
                //gameManager.simulationManager.isBrushingAgents = false;
            }


            

            if(cameraManager.isFollowingPlantParticle) {
                cameraManager.targetPlantWorldPos = gameManager.simulationManager.vegetationManager.selectedPlantParticleData.worldPos;
            }
            if(cameraManager.isFollowingAnimalParticle) {
                cameraManager.targetZooplanktonWorldPos = gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleData.worldPos;
            }
            

            /*if (leftClickThisFrame) {
                
                gameManager.simulationManager.isBrushingAgents = true; // ** TEMP DEBUGGING
                // ANIMALS::::
                if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                    if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {  // AGENTS
                        gameManager.simulationManager.isBrushingAgents = true;
                    }
                }
                
                Debug.Log("Left Click! " + gameManager.simulationManager.isBrushingAgents.ToString());
            }*/

            // check for player clicking on an animal in the world
            MouseRaycastCheckAgents(leftClickThisFrame);

            // Get position of mouse on water plane:
            MouseRaycastWaterPlane(Input.mousePosition);
            Vector4[] dataArray = new Vector4[1];
            Vector4 gizmoPos = new Vector4(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f, 0f);
            dataArray[0] = gizmoPos;
            gameManager.theRenderKing.gizmoCursorPosCBuffer.SetData(dataArray);

            bool stirGizmoVisible = false;

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

            if (recentlyCreatedSpecies) {
                recentlyCreatedSpeciesTimeStepCounter++;
                if(recentlyCreatedSpeciesTimeStepCounter > 360) {
                    recentlyCreatedSpecies = false;
                    recentlyCreatedSpeciesTimeStepCounter = 0;
                }
            }

            bool updateTerrainAltitude = false;
            float terrainUpdateMagnitude = 0f;

            
            if (EventSystem.current.IsPointerOverGameObject()) {  // if mouse is over ANY unity canvas UI object (with raycast enabled)
                //Debug.Log("MouseOverUI!!!");
            }
            else {
                // Plant Particle Following/Selection
                if(leftClickThisFrame) {
                    if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
                        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {
                            // if plant particles
                            if (curActiveTool == ToolType.Watcher) {
                                int selectedID = gameManager.simulationManager.vegetationManager.selectedPlantParticleIndex;
                                int closestID = gameManager.simulationManager.vegetationManager.closestPlantParticleData.index;

                                if(selectedID != closestID) {
                                    gameManager.simulationManager.vegetationManager.selectedPlantParticleIndex = closestID;
                                    gameManager.simulationManager.vegetationManager.isPlantParticleSelected = true;
                                    Debug.Log("FOLLOWING " + gameManager.simulationManager.vegetationManager.selectedPlantParticleIndex.ToString());
                                    isSpiritBrushSelected = true;
                                    StartFollowingPlantParticle();
                                }
                            }
                        }
                    }
                    if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {
                            // if plant particles
                            if (curActiveTool == ToolType.Watcher) {
                                int selectedID = gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex;
                                int closestID = gameManager.simulationManager.zooplanktonManager.closestAnimalParticleData.index;

                                if(selectedID != closestID) {
                                    gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex = closestID;
                                    gameManager.simulationManager.zooplanktonManager.isAnimalParticleSelected = true;
                                    Debug.Log("FOLLOWING " + gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString());
                                    isSpiritBrushSelected = true;
                                    StartFollowingAnimalParticle();
                                }
                            }
                        }
                    }
                }
            
                if (curActiveTool == ToolType.Watcher || curActiveTool == ToolType.None) {                    
                    //gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);

                    
                }
                else {
                    
                    //gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
                }

                if (curActiveTool == ToolType.Add) {                    
                    stirGizmoVisible = true;
                }
                                
            
                if (curActiveTool == ToolType.Stir) {  
                    stirGizmoVisible = true;
                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(false, 0.05f);
                    float isActing = 0f;
                    
                    if (isDraggingMouseLeft) {
                        
                        isActing = 1f;
                        
                        float mag = smoothedMouseVel.magnitude;
                        float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                        if(mag > 0f) {
                            gameManager.simulationManager.PlayerToolStirOn(curMousePositionOnWaterPlane, smoothedMouseVel * (0.25f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.2f), radiusMult);
                            
                        }
                        else {
                            gameManager.simulationManager.PlayerToolStirOff();
                        }
                    }
                    else {
                        gameManager.simulationManager.PlayerToolStirOff();                        
                    }

                    if(isActing > 0.5f) {
                        stirStickDepth = Mathf.Lerp(stirStickDepth, 1f, 0.2f);
                    }
                    else {
                        stirStickDepth = Mathf.Lerp(stirStickDepth, -4f, 0.2f);
                    }
                    gameManager.theRenderKing.isStirring = isDraggingMouseLeft;
                    gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsStirring", isActing);
                    gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_IsStirring", isActing);
                    //gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_Radius", Mathf.Lerp(0.05f, 2.5f, gameManager.theRenderKing.baronVonWater.camDistNormalized));  // **** Make radius variable! (possibly texture based?)
                    gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_Radius", 6.2f);
                }

                gameManager.theRenderKing.isBrushing = false;
                gameManager.theRenderKing.isSpiritBrushOn = false;
                gameManager.theRenderKing.nutrientToolOn = false;
                isBrushAddingAgents = false;
                gameManager.simulationManager.vegetationManager.isBrushActive = false;
                //isBrushingAgents = false;
                //gameManager.simulationManager.theRenderKing.ClickTestTerrain(true); // *********************** always updates!
                if (curActiveTool == ToolType.Add || curActiveTool == ToolType.Remove) {
                    // What Palette Trophic Layer is selected?


                    gameManager.simulationManager.PlayerToolStirOff();

                    if (isDraggingMouseLeft || isDraggingMouseRight) {

                        float randRipple = UnityEngine.Random.Range(0f, 1f);
                        if(randRipple < 0.33) {
                            gameManager.theRenderKing.baronVonWater.RequestNewWaterRipple(new Vector2(curMousePositionOnWaterPlane.x / SimulationManager._MapSize, curMousePositionOnWaterPlane.y / SimulationManager._MapSize));

                        }
                        
                        // IF TERRAIN SELECTED::::
                        if (gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot) {
                            // DECOMPOSERS::::
                            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
                                gameManager.simulationManager.vegetationManager.isBrushActive = true;
                            }// PLANTS:
                            else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
                                if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {
                                    gameManager.simulationManager.vegetationManager.isBrushActive = true;
                                }
                                else {
                                    gameManager.simulationManager.vegetationManager.isBrushActive = true;
                                    
                                }                                
                            }  // ANIMALS::::                            
                            else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                                if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {  
                                    // zooplankton?
                                }
                                else {// AGENTS                                
                                    int speciesIndex = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.linkedSpeciesID;
                                    if (isDraggingMouseLeft) {
                                        gameManager.simulationManager.recentlyAddedSpeciesOn = true; // ** needed?
                                        isBrushAddingAgents = true;
                                        //gameManager.simulationManager.isBrushingAgents = true;
                                        Debug.Log("isBrushAddingAgents = true; speciesID = " + speciesIndex.ToString());

                                        brushAddAgentCounter++;

                                        if(brushAddAgentCounter >= framesPerAgentSpawn) {
                                            brushAddAgentCounter = 0;

                                            gameManager.simulationManager.AttemptToBrushSpawnAgent(speciesIndex);
                                        }
                                    }
                                    if (isDraggingMouseRight) {
                                        gameManager.simulationManager.AttemptToKillAgent(speciesIndex, new Vector2(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y), 15f);
                                    }                                   
                                }
                            }
                            else if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 3) {
                                updateTerrainAltitude = true;
                                terrainUpdateMagnitude = 0.05f;
                                if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 0) { // WORLD
                                    terrainUpdateMagnitude = 1f;
                                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.4f);
                                }
                                else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 1) {  // STONE

                                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                                }
                                else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 2) {  // PEBBLES
                                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                                }
                                else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 3) {  // SAND
                                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                                }
                                
                            }
                            else {
                                if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 0) {  // MINERALS
                                    gameManager.simulationManager.vegetationManager.isBrushActive = true;
                                    Debug.Log("SDFADSFAS");
                                }
                                else if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 1) {   // WATER
                                    if (isDraggingMouseLeft) {
                                        gameManager.theRenderKing.baronVonWater._GlobalWaterLevel = Mathf.Clamp01(gameManager.theRenderKing.baronVonWater._GlobalWaterLevel + 0.002f);
                                    }
                                    if (isDraggingMouseRight) {
                                       gameManager.theRenderKing.baronVonWater._GlobalWaterLevel = Mathf.Clamp01(gameManager.theRenderKing.baronVonWater._GlobalWaterLevel - 0.002f);
                                    }
                                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                                }
                                else if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID == 2) {   // AIR
                                    if (isDraggingMouseLeft) {
                                        if (UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                                            gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp((gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents + 1), 0, 10);
                                        }                                    
                                    }
                                    if (isDraggingMouseRight) {
                                       if(UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                                            gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp((gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents - 1), 0, 10);
                                            
                                        }
                                    }  
                                }
                            }
                            
                            
                            
                        }

                        
                        gameManager.theRenderKing.isBrushing = true;
                        gameManager.theRenderKing.isSpiritBrushOn = true;
                        gameManager.theRenderKing.spiritBrushPosNeg = 1f;
                        if(isDraggingMouseRight) {  // *** Re-Factor!!!
                            gameManager.theRenderKing.spiritBrushPosNeg = -1f;
                        }

                        // Also Stir Water -- secondary effect
                        float mag = smoothedMouseVel.magnitude;
                        float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                        if (mag > 0f) {
                            gameManager.simulationManager.PlayerToolStirOn(curMousePositionOnWaterPlane, smoothedMouseVel * (0.25f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.2f) * 0.33f, radiusMult);

                        }
                        else {
                            gameManager.simulationManager.PlayerToolStirOff();
                        }
                        gameManager.theRenderKing.isStirring = isDraggingMouseLeft || isDraggingMouseRight; 

                    }
                    else {
                        //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(false, 0.05f);
                        gameManager.simulationManager.recentlyAddedSpeciesOn = false;
                    }
                }
                else {

                }

                
            }

            if (isDraggingMouseLeft || isDraggingMouseRight) {
                gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);
            }
            else {
                gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);                   
            }
            
            if(stirGizmoVisible) {
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
                gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_IsVisible", 1f);
                Cursor.visible = false;
            }         
            else {
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
                gameManager.theRenderKing.gizmoStirStickAMat.SetFloat("_IsVisible", 0f);
                Cursor.visible = true;
            }

            bool isCursorOn = true;
            if(gameManager.uiManager.curActiveTool == ToolType.Add) {
                isCursorOn = false;
            }
            if(gameManager.uiManager.curActiveTool == ToolType.Stir) {
                isCursorOn = false;
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
    public void UpdateKnowledgePanelUI(TrophicLayersManager layerManager) {
        panelKnowledgeInfoWorld.SetActive(false);
        panelKnowledgeInfoDecomposers.SetActive(false);
        panelKnowledgeInfoAlgae.SetActive(false);
        panelKnowledgeInfoPlants.SetActive(false);
        panelKnowledgeInfoZooplankton.SetActive(false);
        panelKnowledgeInfoVertebrates.SetActive(false);

        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();
        
        textGlobalMass.text = "Global Biomass: " + gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterAlgae.text = (resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles).ToString("F0");
        textMeterZooplankton.text = (resourcesRef.curGlobalAnimalParticles).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass).ToString("F2");    
                
        textToolbarWingSpeciesSummary.gameObject.SetActive(true);
        string summaryText = layerManager.GetSpeciesPreviewDescriptionString(gameManager.simulationManager);
        textToolbarWingSpeciesSummary.text = summaryText;

        if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
            // DECOMPOSERS
            panelKnowledgeInfoDecomposers.SetActive(true);
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {  // ALGAE
                panelKnowledgeInfoAlgae.SetActive(true);
            }
            else {  // PLANT PARTICLES
                panelKnowledgeInfoPlants.SetActive(true);     
            }
                    
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) { // ZOOPLANKTON
                panelKnowledgeInfoZooplankton.SetActive(true);
            }
            else {  // VERTEBRATES
                panelKnowledgeInfoVertebrates.SetActive(true);
                

                float lifespan = 0f;
                float population = 0f;
                float foodEaten = 0f;
                float genome = 0f;

                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan0.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation0.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten0.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome0.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat0;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat0;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat0;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat0;
                } 
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan1.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation1.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten1.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome1.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat1;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat1;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat1;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat1;
                } 
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan2.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation2.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten2.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome2.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat2;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat2;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat2;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat2;
                } 
                else {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan3.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation3.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten3.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome3.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat3;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat3;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat3;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat3;
                }

                textKnowledgeGraphLifespan.text = lifespan.ToString("F0");
                textKnowledgeGraphPopulation.text = population.ToString("F0");
                textKnowledgeGraphFoodEaten.text = foodEaten.ToString("F3");
                textKnowledgeGraphGenome.text = genome.ToString("F1");  // avg Body size
            }
                    
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) {
            panelKnowledgeInfoWorld.SetActive(true);
            if (layerManager.selectedTrophicSlotRef.slotID == 0) {
                // WORLD    
            } 
            else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                // STONE
            } 
            else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                // PEBBLES
            } 
            else {
                // SAND
            }                    
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 4) {
            panelKnowledgeInfoWorld.SetActive(true);
            if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                // Minerals
            } 
            else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                // Water
            } 
            else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                // AIR
            }      
        }
    }
    private void UpdateWatcherPanelUI(TrophicLayersManager layerManager) {

        panelWatcherSpiritVertebratesHUD.SetActive(false);
        panelWatcherSpiritVertebratesText.SetActive(false);
        panelWatcherSpiritVertebratesGenome.SetActive(false);
        panelWatcherSpiritVertebratesBrain.SetActive(false);
        panelWatcherSpiritZooplankton.SetActive(false);
        panelWatcherSpiritPlants.SetActive(false);
        panelWatcherSpiritAlgae.SetActive(false);
        panelWatcherSpiritDecomposers.SetActive(false);
        panelWatcherSpiritTerrain.SetActive(false);

        TextCommonStatsA.gameObject.SetActive(true);

        string str = "";
        if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
            

            Vector4 resourceGridSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceGridRT1, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
            str += "\n\nWaste    : " + (resourceGridSample.y * 1000f).ToString("F0");                    
            str += "\nDecomposers  : " + (resourceGridSample.z * 1000f).ToString("F0");

            Vector4 simTansferSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceSimTransferRT, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
            str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");
            //str += "\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + ")";

            panelWatcherSpiritDecomposers.SetActive(true);
            //UpdateWatcherDecomposersPanelUI();
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                Vector4 resourceGridSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceGridRT1, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nNutrients    : " + (resourceGridSample.x * 1000f).ToString("F0");                    
                str += "\nAlgae        : " + (resourceGridSample.w * 1000f).ToString("F0");
                Vector4 simTansferSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceSimTransferRT, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");
                 
                
                panelWatcherSpiritAlgae.SetActive(true);
            }
            else {
                     
                VegetationManager.PlantParticleData particleData = gameManager.simulationManager.vegetationManager.selectedPlantParticleData;

                str += "\nPlant Particle # " + particleData.index.ToString() + "  [" + particleData.nearestCritterIndex.ToString() + "]";
                //str += "\nCPU: " + gameManager.simulationManager.vegetationManager.tempClosestPlantParticleIndexAndPos.ToString();
                str += "\nCoords [ " + particleData.worldPos.x.ToString("F0") + " , " + particleData.worldPos.y.ToString("F0");
                str += "\nColorA (" + particleData.colorA.ToString() + ".";                        
                str += "\n\nAge: " + (particleData.age * 1000f).ToString("F0");
                str += "\nBiomass: " + (particleData.biomass * 1000f).ToString("F0");
                str += "\nNutrients Used: " + (particleData.nutrientsUsed * 100000000f).ToString("F0");
                str += "\nOxygen Produced: " + (particleData.oxygenProduced * 10000000f).ToString("F0");
                str += "\nIsDecaying: " + (particleData.isDecaying).ToString("F0");
                str += "\nIsSwallowed: " + (particleData.isSwallowed).ToString("F0");
                str += "\n\nDistance: " + (new Vector2(gameManager.simulationManager.uiManager.curMousePositionOnWaterPlane.x, gameManager.simulationManager.uiManager.curMousePositionOnWaterPlane.y) - particleData.worldPos).magnitude;

                Vector4 resourceGridSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceGridRT1, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nNutrients    : " + (resourceGridSample.x * 1000000f).ToString("F0");                    
                //str += "\nAlgae        : " + (resourceGridSample.w * 1000f).ToString("F0");
                Vector4 simTansferSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceSimTransferRT, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");
                
                textWatcherTargetIndex.text = "#" + particleData.index.ToString();
                panelWatcherSpiritPlants.SetActive(true);
            }
                    
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                ZooplanktonManager.AnimalParticleData particleData = gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleData;
                        
                str += "\nZooplankton # " + gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString();
                str += "\nCoords [ " + particleData.worldPos.x.ToString("F0") + " , " + particleData.worldPos.y.ToString("F0") + " ]"; //  Critter (" + gameManager.simulationManager.agentsArray[0].ownPos.ToString() + ")";
                str += "\nAge: " + (particleData.age * 1000f).ToString("F0");
                str += "\nBiomass: " + (particleData.biomass * 1000f).ToString("F0");
                str += "\nEnergy: " + (particleData.energy * 100f).ToString();
                str += "\nAlgae Eaten: " + (particleData.algaeConsumed * 1000000000f).ToString();
                str += "\nOxygen Used: " + (particleData.oxygenUsed * 1000000f).ToString("F0");
                str += "\nWaste Produced: " + (particleData.wasteProduced * 1000000000f).ToString();                        
                str += "\nVelocity (" + (particleData.velocity.x * 1000f).ToString("F0") + ", " + (particleData.velocity.y * 1000f).ToString("F0") + ")";
                str += "\nGenome: " + (particleData.genomeVector * 1f).ToString("F2");
                str += "\nIsDecaying: " + (particleData.isDecaying).ToString("F0");
                str += "\nIsSwallowed: " + (particleData.isSwallowed).ToString("F0");
                //str += "\n\nDistance: " + gameManager.simulationManager.zooplanktonManager.closestZooplanktonArray[0].y.ToString(); // (gameManager.simulationManager.agentsArray[0].ownPos - new Vector2(particleData.worldPos.x, particleData.worldPos.y)).magnitude;
                  
                textWatcherTargetIndex.text = "#" + gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString();
                panelWatcherSpiritZooplankton.SetActive(true);
                        
            }
            else {

                int critterIndex = cameraManager.targetAgentIndex;
                Agent agent = gameManager.simulationManager.agentsArray[critterIndex];

                textNewInspectAgentName.text = agent.candidateRef.candidateGenome.bodyGenome.coreGenome.name;

                textNewInspectLog.text = agent.stringCauseOfDeath.ToString() + ", " + agent.cooldownFrameCounter.ToString() + " / " + agent.cooldownDuration.ToString(); // agent.lastEvent;
                newInspectAgentEnergyMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.energy * 0.25f));
                newInspectAgentStaminaMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stamina[0] * 1f));
                newInspectAgentStomachMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stomachContentsNorm * 1f));

                newInspectAgentHealthMat.SetFloat("_HealthHead", Mathf.Clamp01(agent.coreModule.healthHead));
                newInspectAgentHealthMat.SetFloat("_HealthBody", Mathf.Clamp01(agent.coreModule.healthBody));
                newInspectAgentHealthMat.SetFloat("_HealthExternal", Mathf.Clamp01(agent.coreModule.healthExternal));
                newInspectAgentAgeMat.SetFloat("_Value", Mathf.Clamp01((float)agent.ageCounter * 0.0005f));
                newInspectAgentAgeMat.SetFloat("_Age", agent.ageCounter);
                int developmentStateID = 0;
                int curActivityID = 0;
                if(agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                    developmentStateID = 7;
                    //curActivityID = 7;
                }
                if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {

                    // state
                    if (agent.ageCounter < 1000) {
                        developmentStateID = 1;
                    }
                    else {
                        if (agent.ageCounter < 10000) {
                            developmentStateID = 2;
                        }
                    }
                    if(agent.sizePercentage > 0.5f) {
                        developmentStateID = 3;
                    }
                            

                    // curActivity
                    if(agent.isPregnantAndCarryingEggs) {
                        curActivityID = 6;
                    }
                    if(agent.isFeeding) {
                        curActivityID = 1;
                    }
                    if(agent.isAttacking) {
                        curActivityID = 2;
                    }
                    if(agent.isDashing) {
                        curActivityID = 3;
                    }
                    if(agent.isDefending) {
                        curActivityID = 4;
                    }
                    if(agent.isResting) {
                        curActivityID = 5;
                    }
                            
                    if(agent.isCooldown) {
                        curActivityID = 7;
                    }
                }
                newInspectAgentStateMat.SetInt("_StateID", developmentStateID);
                newInspectAgentCurActivityMat.SetInt("_CurActivityID", curActivityID);
                newInspectAgentBrainMat.SetFloat("_Value", Mathf.Clamp01((float)agent.brain.axonList.Count * 0.05f + (float)agent.brain.neuronList.Count * 0.05f));
                newInspectAgentWasteMat.SetFloat("_Value", Mathf.Clamp01(agent.wasteProducedLastFrame * 1000f));
                newInspectAgentThrottleMat.SetFloat("_ThrottleX", Mathf.Clamp01(agent.smoothedThrottle.x));
                newInspectAgentThrottleMat.SetFloat("_ThrottleY", Mathf.Clamp01(agent.smoothedThrottle.y));
                newInspectAgentThrottleMat.SetTexture("_VelocityTex", gameManager.simulationManager.environmentFluidManager._VelocityPressureDivergenceMain);
                newInspectAgentThrottleMat.SetFloat("_AgentCoordX", agent.ownPos.x / SimulationManager._MapSize);
                newInspectAgentThrottleMat.SetFloat("_AgentCoordY", agent.ownPos.y / SimulationManager._MapSize);
                
                TextCommonStatsA.gameObject.SetActive(false);  // non-vertebrates just share one textbox for now

                panelWatcherSpiritVertebratesHUD.SetActive(false);
                panelWatcherSpiritVertebratesText.SetActive(false);
                panelWatcherSpiritVertebratesGenome.SetActive(false);
                panelWatcherSpiritVertebratesBrain.SetActive(false);

                textWatcherVertebratePageNum.text = "PAGE " + (curWatcherPanelVertebratePageNum + 1).ToString() + " of 4";
                textWatcherTargetIndex.text = "#" + agent.index.ToString();
                
                // pages:
                if(curWatcherPanelVertebratePageNum == 0) {
                    panelWatcherSpiritVertebratesHUD.SetActive(true);
                    //where do hud elements get updated?? ***
                    string hudString = "";
                    
                    hudString += "\nCur Biomass: " + agent.currentBiomass.ToString("F3") + "";
                    hudString += "\nAge: " + agent.ageCounter.ToString() + "\n";
                    //hudString += "\nisMouthTrigger" + agent.coreModule.isMouthTrigger[0].ToString() + "";
                    //hudString += "\nWaterDepth " + agent.environmentModule.waterDepth[0].ToString("F3") + "  " + agent.depthGradient.ToString();
                    hudString += "\n#" + agent.index.ToString() + " (" + agent.speciesIndex.ToString() + ") Coords: (" + gameManager.simulationManager.simStateData.agentFluidPositionsArray[agent.index].x.ToString("F2") + ", " + gameManager.simulationManager.simStateData.agentFluidPositionsArray[agent.index].x.ToString("F2") + ")";
                    //hudString += "\nN=" + agent.environmentModule.depthNorth[0].ToString("F3") + ", E=" + agent.environmentModule.depthEast[0].ToString("F3") + ", S=" + agent.environmentModule.depthSouth[0].ToString("F3") + ", W=" + agent.environmentModule.depthWest[0].ToString("F3") + ",";
                    //hudString += "\nWaterVel " + agent.environmentModule.waterVelX[0].ToString("F3") + ", " + agent.environmentModule.waterVelY[0].ToString("F3");
                    //hudString += "\nPlant[" + agent.foodModule.nearestFoodParticleIndex.ToString() + "] " + agent.foodModule.nearestFoodParticlePos.ToString() + "";
                    //hudString += "\nZoo[" + agent.foodModule.nearestAnimalParticleIndex.ToString() + "] " + agent.foodModule.nearestAnimalParticlePos.ToString() + "";
                    //hudString += "\nOwnVel" + agent.movementModule.ownVelX[0].ToString("F3") + ", " + agent.movementModule.ownVelY[0].ToString("F3");
                    hudString += "\nCID: " + agent.candidateRef.candidateID.ToString() + ", gen# " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();

                    //hudString += "\n\nHit " + agent.coreModule.isContact[0].ToString("F2") + " (" + agent.coreModule.contactForceX[0].ToString("F4") + ", " + agent.coreModule.contactForceY[0].ToString("F4");
                    hudString += "\nInCOMM: (" + agent.communicationModule.inComm0[0].ToString("F2") + ", " + agent.communicationModule.inComm1[0].ToString("F2") + ", " + agent.communicationModule.inComm2[0].ToString("F2") + ", " + agent.communicationModule.inComm3[0].ToString("F2") + ")";
                    hudString += "\nOutCOMM: (" + agent.communicationModule.outComm0[0].ToString("F2") + ", " + agent.communicationModule.outComm1[0].ToString("F2") + ", " + agent.communicationModule.outComm2[0].ToString("F2") + ", " + agent.communicationModule.outComm3[0].ToString("F2") + ")";
                    
                    textWatcherVertebrateHUD.text = hudString;

                }
                else if(curWatcherPanelVertebratePageNum == 1) {
                    panelWatcherSpiritVertebratesText.SetActive(true);

                    string textString = "Event Log! [" + agent.index.ToString() + "]";
                    textString += "PlantsEaten: " + agent.totalFoodEatenPlant.ToString();
                    //textString += "Depth Gradient: " + agent.depthGradient.ToString();
                    // Agent Event Log:
                    int maxEventsToDisplay = 12;
                    int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
                    int startIndex = Mathf.Max(0, agent.agentEventDataList.Count - maxEventsToDisplay);                   
                    string eventLogString = "";
                    //if(agent.agentEventDataList.Count > 0) {
                    for(int q = agent.agentEventDataList.Count - 1; q >= startIndex; q--) {
                        eventLogString += "\n[" + agent.agentEventDataList[q].eventFrame.ToString() + "] " + agent.agentEventDataList[q].eventText;
                    } 
                    
                    //Debug.Log("eventLogString" + eventLogString);
                    //}                    

                    //textString += "\nNearestPlant[" + agent.foodModule.nearestFoodParticleIndex.ToString() + "] " + agent.foodModule.nearestFoodParticlePos.ToString() + " d: " + (agent.foodModule.nearestFoodParticlePos.magnitude).ToString();
                    //textString += "\nNearestZooplankton[" + agent.foodModule.nearestAnimalParticleIndex.ToString() + "] " + agent.foodModule.nearestAnimalParticlePos.ToString() + " d: " + (agent.foodModule.nearestAnimalParticlePos.magnitude).ToString();
                    
                    textString += "\n\nNumChildrenBorn: " + gameManager.simulationManager.numAgentsBorn.ToString() + ", numDied: " + gameManager.simulationManager.numAgentsDied.ToString() + ", ~Gen: " + ((float)gameManager.simulationManager.numAgentsBorn / (float)gameManager.simulationManager._NumAgents).ToString();
                    textString += "\nSimulation Age: " + gameManager.simulationManager.simAgeTimeSteps.ToString();
                    textString += "\nYear " + gameManager.simulationManager.curSimYear.ToString() + "\n\n";
                    int numActiveSpecies = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
                    textString += numActiveSpecies.ToString() + " Active Species:\n";
                    textString += eventLogString;

                    textWatcherVertebrateText.text = textString;
                }
                else if(curWatcherPanelVertebratePageNum == 2) {
                    panelWatcherSpiritVertebratesGenome.SetActive(true);
                    
                    string genomeString = "GENOME PAGE!"; ;
                    
                    int curCount = 0;
                    int maxCount = 1;
                    if (agent.curLifeStage == Agent.AgentLifeStage.Egg) {
                        curCount = agent.lifeStageTransitionTimeStepCounter;
                        maxCount = agent._GestationDurationTimeSteps;
                    }            
                    if (agent.curLifeStage == Agent.AgentLifeStage.Mature) {
                        curCount = agent.ageCounter;
                        maxCount = agent.maxAgeTimeSteps;
                    }
                    if (agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                        curCount = agent.lifeStageTransitionTimeStepCounter;
                        maxCount = curCount; // agentRef._DecayDurationTimeSteps;
                    }
                    int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
                    string lifeStageProgressTxt = " " + agent.curLifeStage.ToString() + " " + curCount.ToString() + "/" + maxCount.ToString() + "  " + progressPercent.ToString() + "% ";
                    genomeString += "\nCID: " + agent.candidateRef.candidateID.ToString() + ", gen# " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                    // &&&& INDIVIDUAL AGENT: &&&&
                    //string debugTxtAgent = "";            
                    //genomeString += "CRITTER# [" + agent.index.ToString() + "]     SPECIES# [" + agent.speciesIndex.ToString() + "]\n\n";
                    // Init Attributes:
                    // Body:
                    genomeString += "\nBase Size: " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2") + "\n"; 
                    genomeString += "Fullsize: ( " + agent.fullSizeBoundingBox.x.ToString("F2") + ", " + agent.fullSizeBoundingBox.y.ToString("F2") + ", " + agent.fullSizeBoundingBox.z.ToString("F2") + " )\n";
                    genomeString += "\nBONUSES:\nDamage: " + agent.coreModule.damageBonus.ToString("F2") + "\nSpeed: " + agent.coreModule.speedBonus.ToString("F2") + "\nHealth: " + agent.coreModule.healthBonus.ToString("F2") + "\nEnergy: " + agent.coreModule.energyBonus.ToString("F2") + "\n";
                    genomeString += "\nDIET:\nDecay: " + agent.coreModule.foodEfficiencyDecay.ToString("F2") + "\nPlant: " + agent.coreModule.foodEfficiencyPlant.ToString("F2") + "\nMeat: " + agent.coreModule.foodEfficiencyMeat.ToString("F2") + "\n";
                    //string mouthType = "Active";
                    //if (agentRef.mouthRef.isPassive) { mouthType = "Passive"; }
                    //debugTxtAgent += "Mouth: [" + mouthType + "]\n";
                    genomeString += "# Neurons: " + agent.brain.neuronList.Count.ToString() + ", # Axons: " + agent.brain.axonList.Count.ToString() + "\n";
                    genomeString += "# In/Out Nodes: " + agent.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count.ToString() + ", # Hidden Nodes: " + agent.candidateRef.candidateGenome.brainGenome.hiddenNeuronList.Count.ToString() + ", # Links: " + agent.candidateRef.candidateGenome.brainGenome.linkList.Count.ToString() + "\n";

                    genomeString += "\nSENSORS:\n";
                    genomeString += "Comms= " + agent.candidateRef.candidateGenome.bodyGenome.communicationGenome.useComms.ToString() + "\n";
                    genomeString += "Enviro: WaterStats: " + agent.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useWaterStats.ToString() + "\n";
                    CritterModuleFoodSensorsGenome foodGenome = agent.candidateRef.candidateGenome.bodyGenome.foodGenome;
                    genomeString += "Food: Nutrients= " + foodGenome.useNutrients.ToString() + ", Pos= " + foodGenome.usePos.ToString() + ",  Dir= " + foodGenome.useDir.ToString() + ",  Stats= " + foodGenome.useStats.ToString() + ", useEggs: " + foodGenome.useEggs.ToString() + ", useCorpse: " + foodGenome.useCorpse.ToString() + "\n";
                    genomeString += "Friend: Pos= " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.usePos.ToString() + ",  Dir= " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.useDir.ToString() + ",  Vel= " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.useVel.ToString() + "\n";
                    genomeString += "Threat: Pos= " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.usePos.ToString() + ",  Dir= " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useDir.ToString() + ",  Vel= " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useVel.ToString() + ",  Stats= " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useStats.ToString() + "\n";
            
                    textWatcherVertebrateGenome.text = genomeString;
                }
                else if(curWatcherPanelVertebratePageNum == 3) {
                    panelWatcherSpiritVertebratesBrain.SetActive(true);
                    // update brain panel
                                        
                    string brainString = "BRAIN PAGE! 3";
                    brainString += "\n" + agent.brain.neuronList.Count.ToString() + " Neurons    " + agent.brain.axonList.Count.ToString() + " Axons";
                  
                    string brainInputsTxt = "\nINPUTS:";
                    for(int n = 0; n < agent.brain.neuronList.Count; n++) {
                        if(agent.brain.neuronList[n].neuronType == NeuronGenome.NeuronType.In) {
                                    
                            if (n % 3 == 0) {
                                brainInputsTxt += "\n";
                            }
                            float neuronValue = agent.brain.neuronList[n].currentValue[0];
                            if(neuronValue < -0.2f) {
                                brainInputsTxt += "<color=#FF6644FF>";
                            }
                            else if(neuronValue > 0.2f) {
                                brainInputsTxt += "<color=#44FF66FF>";
                            }
                            else {
                                brainInputsTxt += "<color=#A998B5FF>";
                            }
                            brainInputsTxt += "[" + n.ToString() + "] " + agent.brain.neuronList[n].currentValue[0].ToString("F2") + "</color>  ";
                                    
                        }
                    }
                    string brainOutputsTxt = "\n\nOUTPUTS:\n";
                    for(int o = 0; o < agent.brain.neuronList.Count; o++) {
                        if(agent.brain.neuronList[o].neuronType == NeuronGenome.NeuronType.Out) {
                                    
                            if (o % 3 == 0) {
                                brainOutputsTxt += "\n";
                            }
                            float neuronValue = agent.brain.neuronList[o].currentValue[0];
                            if(neuronValue < -0.2f) {
                                brainOutputsTxt += "<color=#FF6644FF>";
                            }
                            else if(neuronValue > 0.2f) {
                                brainOutputsTxt += "<color=#44FF66FF>";
                            }
                            else {
                                brainOutputsTxt += "<color=#A998B5FF>";
                            }
                            brainOutputsTxt += "[" + o.ToString() + "] " + agent.brain.neuronList[o].currentValue[0].ToString("F2") + "</color>  ";
                                    
                            brainOutputsTxt += "";
                                    
                        }
                    }
                    brainString += brainInputsTxt + brainOutputsTxt;

                    textWatcherVertebrateBrain.text = brainString;
                }
                
            }

            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) {
            Vector4 resourceGridSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceGridRT1, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
            str += "\n\nNutrients    : " + (resourceGridSample.x * 1000f).ToString("F0");
            str += "\nWaste        : " + (resourceGridSample.y * 1000f).ToString("F0");
            str += "\nDecomposers  : " + (resourceGridSample.z * 1000f).ToString("F0");
            str += "\nAlgae        : " + (resourceGridSample.w * 1000f).ToString("F0");
            Vector4 simTansferSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceSimTransferRT, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
            str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");

            panelWatcherSpiritTerrain.SetActive(true);
        }
        else {
            // minerals? water? air?
        }

        TextCommonStatsA.text = str;
        // string?
    }
    public void UpdateToolbarPanelUI() {

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
        
        // SpiritBrush Icons
        buttonToolbarWatcher.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarWatcher.gameObject.transform.localScale = Vector3.one;
        imageToolbarInspectLinkedIcon.color = buttonDisabledColor;
        buttonToolbarStir.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarStir.gameObject.transform.localScale = Vector3.one;
        buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarAdd.gameObject.transform.localScale = Vector3.one;
        imageToolbarAddLinkedIcon.color = buttonDisabledColor;
        //buttonTool.gameObject.transform.localScale = Vector3.one;
        buttonToolbarKnowledge.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarKnowledge.gameObject.transform.localScale = Vector3.one;
        imageToolbarKnowledgeLinkedIcon.color = buttonDisabledColor;
        
        //buttonToolbarRemove.GetComponent<Image>().color = buttonDisabledColor;
        //buttonToolbarRemove.gameObject.transform.localScale = Vector3.one;

        TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;  

        //panelKnowledgeSpiritBase.SetActive(false);
        textToolbarWingSpeciesSummary.gameObject.SetActive(true);
        switch(curActiveTool) {
            case ToolType.None:
                //buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;

                break;
            case ToolType.Watcher:
                buttonToolbarWatcher.GetComponent<Image>().color = buttonActiveColor;
                buttonToolbarWatcher.gameObject.transform.localScale = Vector3.one * 1.25f;
                imageToolbarInspectLinkedIcon.color = buttonActiveColor;

                UpdateWatcherPanelUI(layerManager);
                break;
            case ToolType.Mutate:
                //buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor;
                break;
            case ToolType.Add:
                buttonToolbarAdd.GetComponent<Image>().color = buttonActiveColor;
                buttonToolbarAdd.gameObject.transform.localScale = Vector3.one * 1.25f;
                imageToolbarAddLinkedIcon.color = buttonActiveColor;
                break;
            case ToolType.Remove:
                //buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;
                //buttonToolbarRemove.gameObject.transform.localScale = Vector3.one * 1.25f;
                break;
            case ToolType.Stir:
                buttonToolbarStir.GetComponent<Image>().color = buttonActiveColor;
                buttonToolbarStir.gameObject.transform.localScale = Vector3.one * 1.25f;
                break;
            case ToolType.Sage:
                buttonToolbarKnowledge.GetComponent<Image>().color = buttonActiveColor;
                buttonToolbarKnowledge.gameObject.transform.localScale = Vector3.one * 1.25f;
                imageToolbarKnowledgeLinkedIcon.color = buttonActiveColor;

                panelKnowledgeSpiritBase.SetActive(true);

                //UpdateKnowledgePanelUI();

                break;
            default:
                break;

        }


        // Influence points meter:     
        //toolbarInfluencePoints += 0.0005225f; // x10 while debugging
        //toolbarInfluencePoints = Mathf.Clamp01(toolbarInfluencePoints);
        //infoMeterInfluencePointsMat.SetFloat("_FillPercentage", toolbarInfluencePoints);
        //textInfluencePointsValue.text = "Influence: \n" + (toolbarInfluencePoints * 100f).ToString("F0") + "%";

        textSelectedSpeciesTitle.resizeTextMaxSize = 20;
        textSelectedSpiritBrushName.resizeTextMaxSize = 24;

        textSelectedSpiritBrushName.color = colorSpiritBrushLight;

         
        //panelToolbarPaletteExpand.SetActive(isToolbarPaletteExpandOn);
        //panelToolbarWing.SetActive(isToolbarDetailPanelOn);
        //buttonToolbarPaletteExpandOn.gameObject.SetActive(true);
        

        

        
        string spiritBrushName = "Minor Creation Spirit";
        imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushCreationIcon;
        //strSpiritBrushDescription = "This spirit has some powers of life and death";
        //strSpiritBrushEffects = "Left-Click:\n" + strLeftClickEffect[leftClickDescriptionIndex] + "\n\nRight-Click:\n" + strRightClickEffect[rightClickDescriptionIndex];
        if(isDraggingMouseRight) {
            spiritBrushName = "Minor Decay Spirit";

            
        }
        if(curActiveTool == ToolType.Stir) {
            spiritBrushName = "Lesser Stir Spirit";      
            imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushStirIcon;

            //strSpiritBrushDescription = "This spirit reveals hidden information about aspects of the world";
            //strSpiritBrushEffects = "Left-Click:\nFollows the nearest Vertebrate\n\nRight-Click:\nStops following";
        }
        if (curActiveTool == ToolType.Watcher) {
            spiritBrushName = "Minor Watcher Spirit";
            imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushWatcherIcon;

            panelWatcherSpiritMain.SetActive(true);
            
            //panelToolbarWing.SetActive(true);
            //imageToolbarInspectLinkedIcon.sprite = sprit
            //strSpiritBrushDescription = "This spirit reveals hidden information about aspects of the world";
            //strSpiritBrushEffects = "Left-Click:\nFollows the nearest Vertebrate\n\nRight-Click:\nStops following";
        }
        else {
            panelWatcherSpiritMain.SetActive(false);
            //panelToolbarWing.SetActive(false);
        }
        //panelInfoResourcesOverview.SetActive(false); 
        if (curActiveTool == ToolType.Sage) {
            spiritBrushName = "Knowledge Spirit";
            imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushKnowledgeIcon;
            panelInfoResourcesOverview.SetActive(true); 
        }
        //textSpiritBrushDescription.text = strSpiritBrushDescription;
        //textSpiritBrushEffects.text = strSpiritBrushEffects;
        //textLinkedSpiritDescription.text = strLinkedSpiritDescriptionArray[linkedSpiritIndex];

        textSelectedSpiritBrushName.text = spiritBrushName;

        imageToolbarSpiritBrushThumbnail.color = colorSpiritBrushLight;
        imageToolbarSpiritBrushThumbnailBorder.color = colorSpiritBrushDark;

        UpdateSpiritBrushDescriptionsUI(); // ****************************************************************

        


        Color iconColor = Color.white;
        
        bool isSelectedDecomposers = false;     
        bool isSelectedAlgae = false;  
        bool isSelectedPlants = false;  
        bool isSelectedZooplankton = false;  
        bool isSelectedVertebrate0 = false;  
        bool isSelectedVertebrate1 = false;  
        bool isSelectedVertebrate2 = false;  
        bool isSelectedVertebrate3 = false;  
        bool isSelectedMinerals = false;  
        bool isSelectedWater = false;  
        bool isSelectedAir = false;  
        bool isSelectedTerrain0 = false;  
        bool isSelectedTerrain1 = false;  
        bool isSelectedTerrain2 = false;  
        bool isSelectedTerrain3 = false;  
        if(layerManager.isSelectedTrophicSlot) {
            if(layerManager.selectedTrophicSlotRef.kingdomID == 0) {
                isSelectedDecomposers = true; 
                iconColor = colorDecomposersLayer;
                imageToolbarSpeciesPortraitRender.sprite = spriteSpiritDecomposerIcon;
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) {
                if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelectedAlgae = true;
                    iconColor = colorAlgaeLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritAlgaeIcon;
                }
                else {
                    isSelectedPlants = true;  
                    iconColor = colorPlantsLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritPlantIcon;
                }                                   
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) {
                if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelectedZooplankton = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritZooplanktonIcon;
                }
                else {
                    iconColor = colorVertebratesLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritVertebrateIcon;

                    if(layerManager.selectedTrophicSlotRef.slotID == 0) {                        
                        isSelectedVertebrate0 = true;  
                    }
                    else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                        isSelectedVertebrate1 = true; 
                    }
                    else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                        isSelectedVertebrate2 = true; 
                    }
                    else {
                        isSelectedVertebrate3 = true; 
                    }
                    
                }
                                   
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) {
                iconColor = colorTerrainLayer;
                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritWorldIcon;
                    isSelectedTerrain0 = true;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    isSelectedTerrain1 = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritStoneIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    isSelectedTerrain2 = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritPebblesIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 3) {
                    isSelectedTerrain3 = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritSandIcon;
                }
                                   
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 4) {
                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    isSelectedMinerals = true;
                    iconColor = colorMineralLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritMineralsIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    isSelectedWater = true;
                    iconColor = colorWaterLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritWaterIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    isSelectedAir = true;
                    iconColor = colorAirLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritAirIcon;
                }
            }
        }
        else {

        }
        SetToolbarButtonStateUI(ref buttonToolbarDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelectedDecomposers);  
        
        SetToolbarButtonStateUI(ref buttonToolbarAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelectedAlgae);
        SetToolbarButtonStateUI(ref buttonToolbarPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status, isSelectedPlants);
         
        SetToolbarButtonStateUI(ref buttonToolbarZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelectedZooplankton);        
        SetToolbarButtonStateUI(ref buttonToolbarAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelectedVertebrate0);
        SetToolbarButtonStateUI(ref buttonToolbarAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelectedVertebrate1);
        SetToolbarButtonStateUI(ref buttonToolbarAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelectedVertebrate2);
        SetToolbarButtonStateUI(ref buttonToolbarAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelectedVertebrate3);
           
        SetToolbarButtonStateUI(ref buttonToolbarTerrain0, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0].status, isSelectedTerrain0);
        SetToolbarButtonStateUI(ref buttonToolbarTerrain1, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[1].status, isSelectedTerrain1);
        SetToolbarButtonStateUI(ref buttonToolbarTerrain2, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2].status, isSelectedTerrain2);
        SetToolbarButtonStateUI(ref buttonToolbarTerrain3, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3].status, isSelectedTerrain3);

        SetToolbarButtonStateUI(ref buttonToolbarOther0, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status, isSelectedMinerals);
        SetToolbarButtonStateUI(ref buttonToolbarOther1, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status, isSelectedWater);
        SetToolbarButtonStateUI(ref buttonToolbarOther2, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status, isSelectedAir);
    
        textSelectedSpeciesTitle.color = iconColor; // speciesColorLight;
        imageToolbarSpeciesPortraitBorder.color = iconColor; // speciesColorDark;
          
        textSelectedSpeciesTitle.text = layerManager.selectedTrophicSlotRef.speciesName;

        // Test:
        if(!gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot) {
            textSelectedSpeciesTitle.text = "NONE";
        }

        //panelToolbarWingDeletePrompt.SetActive(false);
        textToolbarWingSpeciesSummary.gameObject.SetActive(true);
                                
        // Which panels are available?
        //int panelTier = -1;
                 
        
        imageToolbarSpeciesPortraitRender.color = iconColor;                
        imageToolbarSpeciesPortraitBorder.color = iconColor;
    }
    public void UpdateDebugUI() {

        // DISABLED!!!! -- Need to establish good method for grabbing data from SimulationManager!
        SimulationManager simManager = gameManager.simulationManager;

        Agent agentRef = cameraManager.targetAgent;        
        int agentIndex = agentRef.index;

        if (!agentRef.isInert) {
            // DebugTxt1 : use this for selected creature stats:
            int curCount = 0;
            int maxCount = 1;
            if (agentRef.curLifeStage == Agent.AgentLifeStage.Egg) {
                curCount = agentRef.lifeStageTransitionTimeStepCounter;
                maxCount = agentRef._GestationDurationTimeSteps;
            }
            /*if (agentRef.curLifeStage == Agent.AgentLifeStage.Young) {
                curCount = agentRef.lifeStageTransitionTimeStepCounter;
                maxCount = agentRef._YoungDurationTimeSteps;
            }*/
            if (agentRef.curLifeStage == Agent.AgentLifeStage.Mature) {
                curCount = agentRef.ageCounter;
                maxCount = agentRef.maxAgeTimeSteps;
            }
            if (agentRef.curLifeStage == Agent.AgentLifeStage.Dead) {
                curCount = agentRef.lifeStageTransitionTimeStepCounter;
                maxCount = curCount; // agentRef._DecayDurationTimeSteps;
            }
            int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
            string lifeStageProgressTxt = " " + agentRef.curLifeStage.ToString() + " " + curCount.ToString() + "/" + maxCount.ToString() + "  " + progressPercent.ToString() + "% ";

            // &&&& INDIVIDUAL AGENT: &&&&
            string debugTxtAgent = "";            
            debugTxtAgent += "CRITTER# [" + agentIndex.ToString() + "]     SPECIES# [" + agentRef.speciesIndex.ToString() + "]\n\n";
            // Init Attributes:
            // Body:
            debugTxtAgent += "Base Size: " + agentRef.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + agentRef.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2") + "\n"; 
            debugTxtAgent += "Fullsize Dimensions: ( " + agentRef.fullSizeBoundingBox.x.ToString("F2") + ", " + agentRef.fullSizeBoundingBox.y.ToString("F2") + ", " + agentRef.fullSizeBoundingBox.z.ToString("F2") + " )\n";
            debugTxtAgent += "BONUS - Damage: " + agentRef.coreModule.damageBonus.ToString("F2") + ", Speed: " + agentRef.coreModule.speedBonus.ToString("F2") + ", Health: " + agentRef.coreModule.healthBonus.ToString("F2") + ", Energy: " + agentRef.coreModule.energyBonus.ToString("F2") + "\n";
            debugTxtAgent += "DIET - Decay: " + agentRef.coreModule.foodEfficiencyDecay.ToString("F2") + ", Plant: " + agentRef.coreModule.foodEfficiencyPlant.ToString("F2") + ", Meat: " + agentRef.coreModule.foodEfficiencyMeat.ToString("F2") + "\n";
            //string mouthType = "Active";
            //if (agentRef.mouthRef.isPassive) { mouthType = "Passive"; }
            //debugTxtAgent += "Mouth: [" + mouthType + "]\n";
            debugTxtAgent += "# Neurons: " + agentRef.brain.neuronList.Count.ToString() + ", # Axons: " + agentRef.brain.axonList.Count.ToString() + "\n";
            debugTxtAgent += "# In/Out Nodes: " + agentRef.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count.ToString() + ", # Hidden Nodes: " + agentRef.candidateRef.candidateGenome.brainGenome.hiddenNeuronList.Count.ToString() + ", # Links: " + agentRef.candidateRef.candidateGenome.brainGenome.linkList.Count.ToString() + "\n";

            debugTxtAgent += "\nSENSORS:\n";
            debugTxtAgent += "Comms= " + agentRef.candidateRef.candidateGenome.bodyGenome.communicationGenome.useComms.ToString() + "\n";
            debugTxtAgent += "Enviro: WaterStats: " + agentRef.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useWaterStats.ToString() + ", Cardinals= " + agentRef.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useCardinals.ToString() + ", Diagonals= " + agentRef.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useDiagonals.ToString() + "\n";
            CritterModuleFoodSensorsGenome foodGenome = agentRef.candidateRef.candidateGenome.bodyGenome.foodGenome;
            debugTxtAgent += "Food: Nutrients= " + foodGenome.useNutrients.ToString() + ", Pos= " + foodGenome.usePos.ToString() + ",  Dir= " + foodGenome.useDir.ToString() + ",  Stats= " + foodGenome.useStats.ToString() + ", useEggs: " + foodGenome.useEggs.ToString() + ", useCorpse: " + foodGenome.useCorpse.ToString() + "\n";
            debugTxtAgent += "Friend: Pos= " + agentRef.candidateRef.candidateGenome.bodyGenome.friendGenome.usePos.ToString() + ",  Dir= " + agentRef.candidateRef.candidateGenome.bodyGenome.friendGenome.useDir.ToString() + ",  Vel= " + agentRef.candidateRef.candidateGenome.bodyGenome.friendGenome.useVel.ToString() + "\n";
            debugTxtAgent += "Threat: Pos= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.usePos.ToString() + ",  Dir= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.useDir.ToString() + ",  Vel= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.useVel.ToString() + ",  Stats= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.useStats.ToString() + "\n";
            // Realtime Values:
            debugTxtAgent += "\nREALTIME DATA:";
            debugTxtAgent += "\nExp: " + agentRef.totalExperience.ToString("F2") + ",  fitnessScore: " + agentRef.masterFitnessScore.ToString("F2") + ", LVL: " + agentRef.curLevel.ToString();
            debugTxtAgent += "\n(" + lifeStageProgressTxt + ") Growth: " + (agentRef.sizePercentage * 100f).ToString("F0") + "%, Age: " + agentRef.ageCounter.ToString() + " Frames\n\n";
                        
            debugTxtAgent += "Nearest Food: [" + agentRef.foodModule.nearestFoodParticleIndex.ToString() +
                        "] Amount: " + agentRef.foodModule.nearestFoodParticleAmount.ToString("F4") +
                        "\nPos: ( " + agentRef.foodModule.nearestFoodParticlePos.x.ToString("F2") +
                        ", " + agentRef.foodModule.nearestFoodParticlePos.y.ToString("F2") +
                        " ), Dir: ( " + agentRef.foodModule.foodPlantDirX[0].ToString("F2") +
                        ", " + agentRef.foodModule.foodPlantDirY[0].ToString("F2") + " )" +
                        "\n";
            debugTxtAgent += "\nNutrients: " + agentRef.foodModule.nutrientDensity[0].ToString("F4") + ", Stamina: " + agentRef.coreModule.stamina[0].ToString("F3") + "\n";
            debugTxtAgent += "Gradient Dir: (" + agentRef.foodModule.nutrientGradX[0].ToString("F2") + ", " + agentRef.foodModule.nutrientGradY[0].ToString("F2") + ")\n";
            debugTxtAgent += "Total Food Eaten -- Decay: n/a, Plant: " + agentRef.totalFoodEatenPlant.ToString("F2") + ", Meat: " + agentRef.totalFoodEatenMeat.ToString("F2") + "\nFood Stored: " + agentRef.coreModule.foodStored[0].ToString() + ", Corpse Food Amount: " + agentRef.currentBiomass.ToString("F3") + "\n";

            //debugTxtAgent += "\nFullSize: " + agentRef.fullSizeBoundingBox.ToString() + ", Volume: " + agentRef.fullSizeBodyVolume.ToString() + "\n";
            //debugTxtAgent += "( " + (agentRef.sizePercentage * 100f).ToString("F0") + "% )\n";

            debugTxtAgent += "\nCurVel: " + agentRef.curVel.ToString("F3") + ", CurAccel: " + agentRef.curAccel.ToString("F3") + ", AvgVel: " + agentRef.avgVel.ToString("F3") + "\n";

            debugTxtAgent += "\nWater Depth: " + agentRef.worldAltitude.ToString("F3") + ", Vel: " + (agentRef.avgFluidVel * 10f).ToString("F3") + "\n";
            debugTxtAgent += "Throttle: [ " + agentRef.movementModule.throttleX[0].ToString("F3") + ", " + agentRef.movementModule.throttleY[0].ToString("F3") + " ]\n";
            debugTxtAgent += "FeedEffector: " + agentRef.coreModule.mouthFeedEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "AttackEffector: " + agentRef.coreModule.mouthAttackEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "DefendEffector: " + agentRef.coreModule.defendEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "DashEffector: " + agentRef.coreModule.dashEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "HealEffector: " + agentRef.coreModule.healEffector[0].ToString("F2") + "\n";
            
            //+++++++++++++++++++++++++++++++++++++ CRITTER: ++++++++++++++++++++++++++++++++++++++++++++
            string debugTxtGlobalSim = "";
            debugTxtGlobalSim += "\n\nNumChildrenBorn: " + simManager.numAgentsBorn.ToString() + ", numDied: " + simManager.numAgentsDied.ToString() + ", ~Gen: " + ((float)simManager.numAgentsBorn / (float)simManager._NumAgents).ToString();
            debugTxtGlobalSim += "\nSimulation Age: " + simManager.simAgeTimeSteps.ToString();
            debugTxtGlobalSim += "\nYear " + simManager.curSimYear.ToString() + "\n\n";
            int numActiveSpecies = simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
            debugTxtGlobalSim += numActiveSpecies.ToString() + " Active Species:\n";
            for (int s = 0; s < numActiveSpecies; s++) {
                int speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[s];
                int parentSpeciesID = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;
                int numCandidates = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].candidateGenomesList.Count;
                int numLeaders = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList.Count;
                int numBorn = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].numAgentsEvaluated;
                int speciesPopSize = 0;
                float avgFitness = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgLifespan;
                for (int a = 0; a < simManager._NumAgents; a++) {
                    if (simManager.agentsArray[a].speciesIndex == speciesID) {
                        speciesPopSize++;
                    }
                }
                if(simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].isFlaggedForExtinction) {
                    debugTxtGlobalSim += "xxx ";
                }
                debugTxtGlobalSim += "Species[" + speciesID.ToString() + "] p(" + parentSpeciesID.ToString() + "), size: " + speciesPopSize.ToString() + ", #cands: " + numCandidates.ToString() + ", numEvals: " + numBorn.ToString() + 
                             ",   avgFitness: " + avgFitness.ToString("F2") + 
                             ",   avgConsumption: (" + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgConsumptionDecay.ToString("F4") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgConsumptionPlant.ToString("F4") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgConsumptionMeat.ToString("F4") +
                             "),   avgBodySize: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgBodySize.ToString("F3") +
                             ",   avgTalentSpec: (" + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecAttack.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecDefend.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecSpeed.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecUtility.ToString("F2") +
                             "),   avgDiet: (" + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecDecay.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecPlant.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecMeat.ToString("F2") +
                             "),   avgNumNeurons: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumNeurons.ToString("F1") +
                             ",   avgNumAxons: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumAxons.ToString("F1") +
                             ", avgFitness: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFitnessScore.ToString("F2") +
                             ", avgExp: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgExperience.ToString() + "\n\n";
            }

            
            /*debugTxtGlobalSim += "\n\nAll-Time Species List:\n";
            for (int p = 0; p < simManager.masterGenomePool.completeSpeciesPoolsList.Count; p++) {
                string extString = "Active!";
                if (simManager.masterGenomePool.completeSpeciesPoolsList[p].isExtinct) {
                    extString = "Extinct!";
                }
                debugTxtGlobalSim += "Species[" + p.ToString() + "] p(" + simManager.masterGenomePool.completeSpeciesPoolsList[p].parentSpeciesID.ToString() + ") " + extString + "\n";
            }*/
            
            textDebugTrainingInfo1.text = debugTxtAgent;
            
            textDebugTrainingInfo3.text = debugTxtGlobalSim;
            
        } 
        
        string debugTxtResources = "";
        debugTxtResources += "GLOBAL RESOURCES:\n";
        debugTxtResources += "\nSunlight: " + simManager.settingsManager.environmentSettings._BaseSolarEnergy.ToString();
        debugTxtResources += "\nOxygen: " + simManager.simResourceManager.curGlobalOxygen.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.oxygenProducedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString() + " ( zooplankton )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString() + " ( agents )";
        debugTxtResources += "\nNutrients: " + simManager.simResourceManager.curGlobalNutrients.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.nutrientsUsedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\nDetritus: " + simManager.simResourceManager.curGlobalDetritus.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString() + " ( zooplankton )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString() + " ( agents )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\nDecomposers: " + simManager.simResourceManager.curGlobalDecomposers.ToString();
        debugTxtResources += "\nAlgae (Reservoir): " + simManager.simResourceManager.curGlobalAlgaeReservoir.ToString();
        debugTxtResources += "\nAlgae (Particles): " + simManager.simResourceManager.curGlobalPlantParticles.ToString();
        debugTxtResources += "\nZooplankton: " + simManager.simResourceManager.curGlobalAnimalParticles.ToString();
        debugTxtResources += "\nLive Agents: " + simManager.simResourceManager.curGlobalAgentBiomass.ToString();
        debugTxtResources += "\nDead Agents: " + simManager.simResourceManager.curGlobalCarrionVolume.ToString();
        debugTxtResources += "\nEggSacks: " + simManager.simResourceManager.curGlobalEggSackVolume.ToString();
        debugTxtResources += "\nGlobal Mass: " + simManager.simResourceManager.curTotalMass.ToString();
        Vector4 resourceGridSample = SampleTexture(simManager.vegetationManager.resourceGridRT1, curMousePositionOnWaterPlane / SimulationManager._MapSize);
        Vector4 simTansferSample = SampleTexture(simManager.vegetationManager.resourceSimTransferRT, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 100f;
        //Debug.Log("curMousePositionOnWaterPlane: " + curMousePositionOnWaterPlane.ToString());
        debugTxtResources += "\nresourceGridSample: (" + resourceGridSample.x.ToString("F4") + ", " + resourceGridSample.y.ToString("F4") + ", " + resourceGridSample.z.ToString("F4") + ", " + resourceGridSample.w.ToString("F4") + ")";
        debugTxtResources += "\nsimTansferSample: (" + simTansferSample.x.ToString("F4") + ", " + simTansferSample.y.ToString("F4") + ", " + simTansferSample.z.ToString("F4") + ", " + simTansferSample.w.ToString("F4") + ")";

        textDebugTrainingInfo2.text = debugTxtResources;

        if(debugTextureViewerArray == null) {
            CreateDebugRenderViewerArray();
        }
        debugTextureViewerMat.SetPass(0);
        debugTextureViewerMat.SetVector("_Zoom", _Zoom);
        debugTextureViewerMat.SetFloat("_Amplitude", _Amplitude);
        debugTextureViewerMat.SetVector("_ChannelMask", _ChannelMask);
        debugTextureViewerMat.SetInt("_ChannelSoloIndex", _ChannelSoloIndex);
        debugTextureViewerMat.SetFloat("_IsChannelSolo", (float)_IsChannelSolo);
        debugTextureViewerMat.SetFloat("_Gamma", _Gamma);        
        //debugTextureViewerMat.
        if(debugTextureViewerArray[_DebugTextureIndex] != null) {
            debugTextureViewerMat.SetTexture("_MainTex", debugTextureViewerArray[_DebugTextureIndex]);
            int channelID = 4;
            string[] channelLabelTxt = new string[5];            
            channelLabelTxt[0] = " (X Solo)";
            channelLabelTxt[1] = " (Y Solo)";
            channelLabelTxt[2] = " (Z Solo)";
            channelLabelTxt[3] = " (W Solo)";
            channelLabelTxt[4] = " (Color)";
            if(_IsChannelSolo > 0.5f) {
                channelID = _ChannelSoloIndex;
            }
            textDebugTextureName.text = debugTextureViewerArray[_DebugTextureIndex].name + channelLabelTxt[channelID];
        }
        textDebugTextureZoomX.text = _Zoom.x.ToString();
        textDebugTextureZoomY.text = _Zoom.y.ToString();
        textDebugTextureAmplitude.text = _Amplitude.ToString();
        textDebugTextureSoloChannelIndex.text = _ChannelSoloIndex.ToString();
        textDebugTextureGamma.text = _Gamma.ToString();
    }    
    public void UpdatePausedUI() {
        if(isPaused) {
            panelPaused.SetActive(true);
        }
        else {
            panelPaused.SetActive(false);
        }
    }    
    private void UpdateInspectPanelUI() {
        int critterIndex = cameraManager.targetAgentIndex;
        Agent agent = gameManager.simulationManager.agentsArray[critterIndex];

        
        // DIET:
        string txt = "Diet:\nHerbivore";
        Color dietColor = new Color(0.5f, 1f, 0.3f);
        //if(gameManager.simulationManager.simStateData.critterInitDataArray[critterIndex].mouthIsActive > 0.5) {
        //    txt = "Diet:\nCarnivore";
        //    dietColor = new Color(1f, 0.5f, 0.3f);
        //}
        textDiet.text = txt;
        inspectWidgetDietMat.SetPass(0);
        inspectWidgetDietMat.SetColor("_Tint", dietColor);

        // STOMACH FOOD:
        inspectWidgetStomachFoodMat.SetPass(0);
        inspectWidgetStomachFoodMat.SetColor("_Tint", Color.yellow);        
        float percentage = gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount * 9f;
        inspectWidgetStomachFoodMat.SetFloat("_FillPercentage", Mathf.Clamp01(percentage));
        textStomachContents.text = "Food\n" + Mathf.RoundToInt(gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount * 22f).ToString(); // + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount * 100f).ToString("F1") + "%\nCapacity: " + gameManager.simulationManager.agentsArray[critterIndex].coreModule.stomachCapacity.ToString("F3");

        // ENERGY:
        Color energyHue = new Color(0.3f, 0.5f, 1f);
        inspectWidgetEnergyMat.SetPass(0);
        inspectWidgetEnergyMat.SetColor("_Tint", energyHue);        
        //percentage = gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy *
        //            gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].embryoPercentage;
        percentage = 1.0f - (1.0f / (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy + 1f));
        percentage *= gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].embryoPercentage;
        if (gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
            percentage = 0.01f;
        }
        
        inspectWidgetEnergyMat.SetFloat("_FillPercentage", percentage);
        textEnergy.text = "Energy\n" + (10 * gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy).ToString("F0"); // + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy * 100f).ToString("F1") + "%";

        // Life Cycle
        inspectWidgetLifeCycleMat.SetPass(0);
        inspectWidgetLifeCycleMat.SetColor("_Tint", Color.blue);

        string lifeStageString = "Embryo";
        bool isAlive = false;
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Egg) {
            //percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 
            //            (float)gameManager.simulationManager.agentsArray[critterIndex]._GestationDurationTimeSteps;
            //percentage = 0.25f;
            isAlive = true;
             textLifeCycle.text = "Embryo";
        }
        /*if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Young) {
            percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 
                        (float)gameManager.simulationManager.agentsArray[critterIndex]._YoungDurationTimeSteps;
        }*/
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Mature) {
            //percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].ageCounter / 
            //            (float)gameManager.simulationManager.agentsArray[critterIndex].maxAgeTimeSteps;
            //percentage = 0.6f;
            lifeStageString = "Mature";
            isAlive = true;

            int daysOld = Mathf.RoundToInt((float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 1500f * 365f);
            int yearsOld = Mathf.RoundToInt((float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 1500f);
            string ageText = "Age: \n" + daysOld.ToString() + " Days";
            if(yearsOld > 0) {
                ageText = "Age: \n" + yearsOld.ToString() + " Year";
                if(yearsOld > 1) {
                    ageText = "Age: \n" + yearsOld.ToString() + " Years";
                }
            }
            textLifeCycle.text = ageText; // lifeStageString + "\n" + ((float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 1500f * 365f).ToString("F0") + "Days";

        }
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
            //percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 
            //            (float)gameManager.simulationManager.agentsArray[critterIndex]._DecayDurationTimeSteps;
            //percentage = 0.0f;
            lifeStageString = "Decaying";
            textLifeCycle.text = gameManager.simulationManager.agentsArray[critterIndex].stringCauseOfDeath; // "Dead";
            //isAlive = false;
        }
        percentage = 1f;
        inspectWidgetLifeCycleMat.SetFloat("_FillPercentage", percentage);
        inspectWidgetLifeCycleMat.SetInt("_CurLifeStage", (int)gameManager.simulationManager.agentsArray[critterIndex].curLifeStage);
        //textLifeCycle.text = "Life Stage:\n" + gameManager.simulationManager.agentsArray[critterIndex].curLifeStage.ToString() + "\n" + gameManager.simulationManager.agentsArray[critterIndex].ageCounter.ToString();


        textInspectData.text = "Species: " + agent.speciesIndex.ToString() +
                                "\nSize: " + agent.fullSizeBodyVolume.ToString() + " (" + agent.sizePercentage.ToString() + ")" +
                                "\nBrain: "; 

        // Dimensions
        inspectWidgetDimensionsMat.SetPass(0);
        inspectWidgetDimensionsMat.SetColor("_Tint", Color.gray);
        float width = gameManager.simulationManager.simStateData.critterInitDataArray[critterIndex].boundingBoxSize.x;
        float length = gameManager.simulationManager.simStateData.critterInitDataArray[critterIndex].boundingBoxSize.y;
        inspectWidgetDimensionsMat.SetFloat("_Width", width);
        inspectWidgetDimensionsMat.SetFloat("_Length", length);
        textDimensionsWidth.text = width.ToString("F2") + " Wide";
        textDimensionsLength.text = length.ToString("F2") + " Long";

        // HEALTH:
        Color liveHue = new Color(0.5f, 1f, 0.3f);
        Color deadHue = new Color(0.5f, 0.25f, 0.15f);
        percentage = gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].health *
                    gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].embryoPercentage;
        Color healthHue = Color.Lerp(deadHue, liveHue, percentage); 
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
            percentage = 0.05f;
        }  
        inspectWidgetHealthMat.SetPass(0);
        inspectWidgetHealthMat.SetColor("_Tint", healthHue);
        inspectWidgetHealthMat.SetFloat("_FillPercentage", percentage);
        textHealth.text = "Health\n" + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].health * 100f).ToString("F1") + "%\nStam: " + gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].stamina.ToString("F2");
        
        // Species Icon:
        textSpeciesID.text = gameManager.simulationManager.agentsArray[critterIndex].speciesIndex.ToString();
        Vector3 primaryHue = gameManager.simulationManager.simStateData.critterInitDataArray[critterIndex].primaryHue;
        Color speciesHue = new Color(primaryHue.x, primaryHue.y, primaryHue.z);
        
        inspectWidgetSpeciesIconMat.SetPass(0);
        inspectWidgetSpeciesIconMat.SetColor("_Tint", speciesHue);

        // AGENT ICON
        inspectWidgetAgentIconMat.SetPass(0);
        inspectWidgetAgentIconMat.SetColor("_Tint", Color.gray);
        textAgentID.text = cameraManager.targetAgent.index.ToString();

        //****************
        
        string readoutText = "";
        readoutText += "ID " + critterIndex.ToString() + "    SpeciesID " + agent.speciesIndex.ToString();
        float health = 100f;
        if(agent.coreModule != null) {
            health = agent.coreModule.healthBody * 100f;
        }
        readoutText += "\nBiomass  " + (agent.currentBiomass * 100f).ToString("F0") + "   ( " + (agent.sizePercentage * 100f).ToString("F0") + "%)   Health: " + health.ToString("F0") + "%";
        readoutText += "\nThrottle  " + (agent.smoothedThrottle * 100f).ToString("F0");
        readoutText += "\nWaste   " + (agent.wasteProducedLastFrame * 1000f).ToString("F0") + ", Oxygen   " + (agent.oxygenUsedLastFrame * 1000f).ToString("F0");        
        
        float bodWidth = (agent.fullSizeBoundingBox.x + agent.fullSizeBoundingBox.z) * 0.5f;
        float bodLength = agent.fullSizeBoundingBox.y;
        readoutText += "\nBody Size   " + (bodLength * 10f).ToString("F0") + " x " + (bodWidth * 10f).ToString("F0");
        readoutText += "\nTotal Eaten -- Meat: " + (agent.totalFoodEatenMeat * 1000f).ToString("F0") + ", Plant: " + (agent.totalFoodEatenPlant * 1000f).ToString("F0");
        readoutText += "\nBiteAnimCounter   " + agent.feedingFrameCounter.ToString();
        Vector4 resourceGridSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceGridRT1, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
        Vector4 simTansferSample = SampleTexture(gameManager.simulationManager.vegetationManager.resourceSimTransferRT, curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
        readoutText += "\n\nNutrients    " + (resourceGridSample.x * 1000f).ToString("F0");
        readoutText += "\nWaste        " + (resourceGridSample.y * 1000f).ToString("F0");
        readoutText += "\nDecomposers  " + (resourceGridSample.z * 1000f).ToString("F0");
        readoutText += "\nAlgae        " + (resourceGridSample.w * 1000f).ToString("F0");
        //readoutText += "\nresourceGridSample: (" + resourceGridSample.x.ToString("F4") + ", " + resourceGridSample.y.ToString("F4") + ", " + resourceGridSample.z.ToString("F4") + ", " + resourceGridSample.w.ToString("F4") + ")";
        //readoutText += "\nsimTansferSample: (" + simTansferSample.x.ToString("F4") + ", " + simTansferSample.y.ToString("F4") + ", " + simTansferSample.z.ToString("F4") + ", " + simTansferSample.w.ToString("F4") + ")";

        textInspectReadout.text = readoutText;
    }    
    private void UpdateSpiritBrushDescriptionsUI() {
        TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;  

        int linkedSpiritIndex = 0;

        if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
            linkedSpiritIndex = 7;            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                linkedSpiritIndex = 8;
            }
            else {
                linkedSpiritIndex = 9;
            }            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                linkedSpiritIndex = 10;
            }
            else {
                linkedSpiritIndex = 11;
            }
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) {
            if(layerManager.selectedTrophicSlotRef.slotID == 0) {  // world/bedrock
                linkedSpiritIndex = 0;
            }
            else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                linkedSpiritIndex = 1;
            }
            else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                linkedSpiritIndex = 2;
            }
            else {
                linkedSpiritIndex = 3;
            }
        }
        else {  // 4 == OTHER
            if(layerManager.selectedTrophicSlotRef.slotID == 0) {  // minerals
                linkedSpiritIndex = 4;
            }
            else if(layerManager.selectedTrophicSlotRef.slotID == 1) {  // water
                linkedSpiritIndex = 5;
            }
            else {  // air
                linkedSpiritIndex = 6;
            }
        }

        int spiritBrushIndex = 0;

        if (curActiveTool == ToolType.Sage) {
            //spiritBrushIndex = 0; 
            //strSpiritBrushDescription = "This spirit reveals hidden information about aspects of the world";
            //strSpiritBrushEffects = "Left-Click:\nFollows the nearest Vertebrate\n\nRight-Click:\nStops following";
        }
        else if(curActiveTool == ToolType.Add) {
            spiritBrushIndex = 1;
        }
        else if (curActiveTool == ToolType.Stir) {
            spiritBrushIndex = 2; 
        }
        else if (curActiveTool == ToolType.Watcher) {
            spiritBrushIndex = 3; 
        }
        else if (curActiveTool == ToolType.Mutate) {
            spiritBrushIndex = 4; 
        }
        else if (curActiveTool == ToolType.Resources) {
            spiritBrushIndex = 5; 
        }

        string strSpiritBrushDescription = "";
        string[] linkedSpiritNamesArray = new string[12]; 
        linkedSpiritNamesArray[0] = "World";
        linkedSpiritNamesArray[1] = "Stone";
        linkedSpiritNamesArray[2] = "Pebbles";
        linkedSpiritNamesArray[3] = "Sand";
        linkedSpiritNamesArray[4] = "Minerals";
        linkedSpiritNamesArray[5] = "Water";
        linkedSpiritNamesArray[6] = "Air";
        linkedSpiritNamesArray[7] = "Decomposers";
        linkedSpiritNamesArray[8] = "Algae";
        linkedSpiritNamesArray[9] = "Plants";
        linkedSpiritNamesArray[10] = "Zooplankton";
        linkedSpiritNamesArray[11] = "Vertebrates";
        
        string[] strSpiritBrushDescriptionArray = new string[6]; // = "Decomposers break down the old so that new life can grow.";
        strSpiritBrushDescriptionArray[0] = "Provides information about the world and its contents, and chronicles events through time";
        strSpiritBrushDescriptionArray[1] = "This spirit possesses limited control of life & existence itself";
        strSpiritBrushDescriptionArray[2] = "A mysterioues Kelpie able to control the flow of water";
        strSpiritBrushDescriptionArray[3] = "A Watcher Spirit can track an organism's journey through space and time";
        strSpiritBrushDescriptionArray[4] = "Mutate...       blah blah";
        strSpiritBrushDescriptionArray[5] = "Extra.";

        string[] strLinkedSpiritDescriptionArray = new string[12]; // = "Decomposers break down the old so that new life can grow.";
        strLinkedSpiritDescriptionArray[0] = "The World Spirit provides the spark for a new universe";
        strLinkedSpiritDescriptionArray[1] = "Stone Spirits are some of the oldest beings in the world, they value Stability and Strength";
        strLinkedSpiritDescriptionArray[2] = "Pebble Spirits are usually found in rivers and streams, but can find their way to a lake or pond. They value Balance and Patience";
        strLinkedSpiritDescriptionArray[3] = "Sand Spirits are slippery and hard to pin down. They prize Cooperation and Speed";
        strLinkedSpiritDescriptionArray[4] = "Mineral Spirits infuse nutrients into the earth. They value Empathy and Nurturing";
        strLinkedSpiritDescriptionArray[5] = "Water Spirits are sometimes called Kelpies, They are known for Adaptability and Resilience";
        strLinkedSpiritDescriptionArray[6] = "Air Spirits come in many forms, and value Swiftness and Persistence";
        strLinkedSpiritDescriptionArray[7] = "Decomposers break down the old so that new life can grow.";
        strLinkedSpiritDescriptionArray[8] = "Algae needs light and nutrients to grow.";
        strLinkedSpiritDescriptionArray[9] = "Floating Plants that are a foodsource for Vertebrates";
        strLinkedSpiritDescriptionArray[10] = "Tiny Organisms that feed on Algae";
        strLinkedSpiritDescriptionArray[11] = "Animals that can feed on Plants, Zooplankton, or even other Vertebrates.";

        string startTxt = "Left-Click:\n";
        string midTxt = "\n\nRight-Click:\n";
        string[][] strBrushEffectsArray = new string[5][];
        for(int s = 0; s < 5; s++) {
            strBrushEffectsArray[s] = new string[12];            
        }
        // KNOWLEDGE BRUSH:
        strBrushEffectsArray[0][0] = startTxt + "Knowledge Spirit --> World Spirit" + midTxt + "Knowledge Spirit --> World Spirit";
        strBrushEffectsArray[0][1] = startTxt + "Knowledge Spirit --> Stone Spirit" + midTxt + "Knowledge Spirit --> Stone Spirit";
        strBrushEffectsArray[0][2] = startTxt + "Knowledge Spirit --> Pebbles Spirit" + midTxt + "Knowledge Spirit --> Pebbles Spirit";
        strBrushEffectsArray[0][3] = startTxt + "Knowledge Spirit --> Sand Spirit" + midTxt + "Knowledge Spirit --> Sand Spirit";
        strBrushEffectsArray[0][4] = startTxt + "Knowledge Spirit --> Minerals Spirit" + midTxt + "Knowledge Spirit --> Minerals Spirit";
        strBrushEffectsArray[0][5] = startTxt + "Knowledge Spirit --> Water Spirit" + midTxt + "Knowledge Spirit --> Water Spirit";
        strBrushEffectsArray[0][6] = startTxt + "Knowledge Spirit --> Air Spirit" + midTxt + "Knowledge Spirit --> Air Spirit";
        strBrushEffectsArray[0][7] = startTxt + "Knowledge Spirit --> Decomposer Spirit" + midTxt + "Knowledge Spirit --> Decomposer Spirit";
        strBrushEffectsArray[0][8] = startTxt + "Knowledge Spirit --> Algae Spirit" + midTxt + "Knowledge Spirit --> Algae Spirit";
        strBrushEffectsArray[0][9] = startTxt + "Knowledge Spirit --> Plant Spirit" + midTxt + "Knowledge Spirit --> Plant Spirit";
        strBrushEffectsArray[0][10] = startTxt + "Knowledge Spirit --> Zooplankton Spirit" + midTxt + "Knowledge Spirit --> Zooplankton Spirit";
        strBrushEffectsArray[0][11] = startTxt + "Knowledge Spirit --> Vertebrate Spirit" + midTxt + "Knowledge Spirit --> Vertebrate Spirit";
        // CREATION BRUSH:
        strBrushEffectsArray[1][0] = startTxt + "Creates World" + midTxt + "None";
        strBrushEffectsArray[1][1] = startTxt + "Raises stone from deep below" + midTxt + "Destroys stone, deeping the Pond";
        strBrushEffectsArray[1][2] = startTxt + "Creates mounds of pebbles" + midTxt + "Removes pebbles from the area";
        strBrushEffectsArray[1][3] = startTxt + "Blankets the terrain with sand" + midTxt + "Removes sand from the area";
        strBrushEffectsArray[1][4] = startTxt + "Creates nutrient-rich minerals in the ground" + midTxt + "Saps nutrients out of the environment";
        strBrushEffectsArray[1][5] = startTxt + "Raises the water level" + midTxt + "Lowers water level";
        strBrushEffectsArray[1][6] = startTxt + "Increases wind strength" + midTxt + "Decreases wind strength";
        strBrushEffectsArray[1][7] = startTxt + "Creates Decomposers" + midTxt + "Kills decomposers in the area";
        strBrushEffectsArray[1][8] = startTxt + "Creates a bloom of Algae" + midTxt + "Kills algae in the area";
        strBrushEffectsArray[1][9] = startTxt + "Creates floating plant seedlings" + midTxt + "Kills plants in the area";
        strBrushEffectsArray[1][10] = startTxt + "Creates simple tiny creatures" + midTxt + "Kills nearby zooplankton";
        strBrushEffectsArray[1][11] = startTxt + "Hatches Vertebrates" + midTxt + "Kills Animals";

        // STIR BRUSH:
        strBrushEffectsArray[2][0] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][1] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][2] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][3] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][4] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][5] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][6] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][7] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][8] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][9] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][10] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][11] = startTxt + "Drags water along with itself while moving" + midTxt + "None";

        // WATCHER BRUSH: // RESOURCES
        strBrushEffectsArray[3][0] = startTxt + "Senses nearby spirits" + midTxt + "None";
        strBrushEffectsArray[3][1] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][2] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][3] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][4] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][5] = startTxt + "Info" + midTxt + "None";
        strBrushEffectsArray[3][6] = startTxt + "Info" + midTxt + "None";
        strBrushEffectsArray[3][7] = startTxt + "Info" + midTxt + "None";
        strBrushEffectsArray[3][8] = startTxt + "Info" + midTxt + "None";
        // WATCHER BRUSH: // separate brush???
        strBrushEffectsArray[3][9] = startTxt + "Follows the nearest Plant" + midTxt + "Stops following";
        strBrushEffectsArray[3][10] = startTxt + "Follows the nearest Zooplankton" + midTxt + "Stops following";
        strBrushEffectsArray[3][11] = startTxt + "Follows the nearest Vertebrate" + midTxt + "Stops following";

        // MUTATION BRUSH:
        strBrushEffectsArray[4][0] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][1] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][2] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][3] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][4] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][5] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][6] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][7] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][8] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][9] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][10] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][11] = startTxt + "Mutates more" + midTxt + "Mutates less";

        //strBrushEffectsArray[1][0] = startTxt + "Creates " + linkedSpiritNamesArray[linkedSpiritIndex] + midTxt + "Kills " + linkedSpiritNamesArray[linkedSpiritIndex];
        //strBrushEffectsArray[2][0] = startTxt + "Spirit movement drags water along with it." + midTxt + "None";
        //strBrushEffectsArray[3][0] = startTxt + "Follows the nearest " + linkedSpiritNamesArray[linkedSpiritIndex] + midTxt + "Stops following";
        //strBrushEffectsArray[4][0] = startTxt + "Mutates nearby " + linkedSpiritNamesArray[linkedSpiritIndex] + midTxt + "Lowers mutation rates";
        // STONE :
        //strBrushEffectsArray[5] = "Extra.";
        //strSpiritBrushEffects = "Left-Click:\n" + strLeftClickEffect[leftClickDescriptionIndex] + "\n\nRight-Click:\n" + strRightClickEffect[rightClickDescriptionIndex];
        
        textSpiritBrushDescription.text = strSpiritBrushDescriptionArray[spiritBrushIndex];
        textSpiritBrushEffects.text = strBrushEffectsArray[spiritBrushIndex][linkedSpiritIndex];
        textLinkedSpiritDescription.text = strLinkedSpiritDescriptionArray[linkedSpiritIndex];
    }
    public void ClickMutationConfirm() {
        TrophicSlot slotRef = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
        if(slotRef.kingdomID == 0) {  // Decomposers
            gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID];
            //gameManager.simulationManager.vegetationManager.WorldLayerDecomposerGenomeStuff(ref decomposerSlotGenomeCurrent, 0f);
            gameManager.simulationManager.vegetationManager.GenerateWorldLayerDecomposersGenomeMutationOptions();
        }
        else if(slotRef.kingdomID == 1) {  // Plants
            if (slotRef.tierID == 0) {
                gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID];
                gameManager.simulationManager.vegetationManager.GenerateWorldLayerAlgaeGridGenomeMutationOptions();
            }
            else {
                // OLD //gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID];
                gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.plantRepData = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].plantRepData;
                gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation;
                gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.growthRate = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate;
                gameManager.simulationManager.vegetationManager.ProcessPlantSlotMutation();
                gameManager.simulationManager.vegetationManager.GenerateWorldLayerPlantParticleGenomeMutationOptions();
                 
            }
            
            //gameManager.simulationManager.vegetationManager.ProcessSlotMutation();
            //algaeRepData = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.algaeRepData;
        }
        else if(slotRef.kingdomID == 2) {  // Animals
            if(slotRef.tierID == 0) { // zooplankton
                gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID];
                gameManager.simulationManager.zooplanktonManager.GenerateWorldLayerZooplanktonGenomeMutationOptions();
                gameManager.simulationManager.zooplanktonManager.ProcessSlotMutation();
            }
            else { // vertebrates
                // *** REFERENCE ISSUE!!!!!
                AgentGenome parentGenome = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].representativeGenome;
                //generate "mutated" genome copy with 0 mutationSize ??? workaround:::::  ***********
                gameManager.simulationManager.settingsManager.mutationSettingsVertebrates.mutationStrengthSlot = 0f;

                //vertebrateSlotsGenomesCurrentArray[slotID].representativeGenome  // **** Use this genome as basis?
                AgentGenome mutatedGenome = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[0].Mutate(parentGenome, true, true);  // does speciesPoolIndex matter?

                gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome = mutatedGenome;
                gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].name = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].name;
                //gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID];
                //gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;

                Debug.Log("CONFIR<M  " + gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary.ToString());
                //gameManager.simulationManager.masterGenomePool.
                //gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[slotRef.linkedSpeciesID].representativeGenome = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome;

                gameManager.simulationManager.masterGenomePool.GenerateWorldLayerVertebrateGenomeMutationOptions(slotRef.slotID, slotRef.linkedSpeciesID);
                //gameManager.simulationManager.masterGenomePool.ProcessSlotMutation(slotRef.slotID, selectedToolbarMutationID, slotRef.linkedSpeciesID);
                InitToolbarPortraitCritterData(slotRef);
            }
        }
        else if(slotRef.kingdomID == 3) { // Terrain
            if(slotRef.slotID == 0) {
                //gameManager.theRenderKing.baronVonTerrain.ApplyMutation(id);
                gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 1) {
                //gameManager.theRenderKing.baronVonTerrain.ApplyMutation(id);
                gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 2) {
                gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 3) {
                gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedToolbarMutationID];
            }

            // if terrain:
            gameManager.theRenderKing.baronVonTerrain.GenerateTerrainSlotGenomeMutationOptions(slotRef.slotID);
            //gameManager.theRenderKing.ClickTestTerrainUpdateMaps(false, 0.05f); // refresh color
        }
        Debug.Log("MUTATION!!! kingdom(" + slotRef.kingdomID.ToString() + ")");
        //selectedToolbarMutationID = 0; // Reset?? figure out what you want to do here
    }
    public void ClickMutationOption(int id) { // **** Need better smarter way to detect selected slot and point to corresponding data
        //Debug.Log("ClickMutationOption(" + id.ToString() + ")");

        selectedToolbarMutationID = id;
                
    }
    private void UpdateToolbarMutationPanel() {
        textToolbarWingPanelName.text = "Mutations:";
        //panelToolbarWingMutation.SetActive(true);
        
        
        
        
        
        // Update mutation/upgrade panel

        //imageMutationPanelThumbnailB.color = Color.red;
        //imageMutationPanelThumbnailB.sprite = null;
        //imageMutationPanelThumbnailC.color = Color.white;
        //imageMutationPanelThumbnailC.sprite = null;

        textMutationPanelOptionA.text = "Tiny";
        textMutationPanelOptionB.text = "Small";
        textMutationPanelOptionC.text = "Large";
        textMutationPanelOptionD.text = "Huge";

        textMutationPanelTitleCur.text = "CURRENT";
        string[] titlesTxt = new string[4];
        titlesTxt[0] = "TINY MUTATION";
        titlesTxt[1] = "SMALL MUTATION";
        titlesTxt[2] = "LARGE MUTATION";
        titlesTxt[3] = "HUGE MUTATION";
        textMutationPanelTitleNew.text = titlesTxt[selectedToolbarMutationID];

        buttonToolbarMutateConfirm.gameObject.SetActive(false);
        if(selectedToolbarMutationID >= 0) {
            buttonToolbarMutateConfirm.gameObject.SetActive(true);

            if(selectedToolbarMutationID == 0) {
                imageMutationPanelHighlightA.color = Color.gray;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 1) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.gray;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 2) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.gray;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 3) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.gray;
            }
        }



        TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;   
        if(layerManager.selectedTrophicSlotRef.kingdomID == 0) { // DECOMPOSERS
            // Look up decomposer variants and populate UI elements from them:
            //textMutationPanelOptionA.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionB.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionC.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionD.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
            Color uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailB.color = uiColor;
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailC.color = uiColor;
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailD.color = uiColor;

            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColor;
            uiColor.a = 1f;
            imageMutationPanelNewPortrait.color = uiColor;
            textMutationPanelCur.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
            textMutationPanelNew.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.displayColor;
            uiColor.a = 1f;
            imageMutationPanelCurPortrait.color = uiColor; 
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) { // PLANTS

            // Algae Particles:
            //gameManager.simulationManager.vegetationManager.

            //textMutationPanelOptionA.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionB.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionC.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionD.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                Color uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].displayColor; // new Color(hue.x, hue.y, hue.z);
                //uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].displayColor;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].displayColor;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].displayColor;
                imageMutationPanelThumbnailD.color = uiColor;
            
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColor;
                //uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.displayColor;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            else {  // PLANTS:
                Color uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[0].displayColor; // new Color(hue.x, hue.y, hue.z);
                //uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[1].displayColor;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[2].displayColor;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[3].displayColor;
                imageMutationPanelThumbnailD.color = uiColor;
            
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].displayColor;
                //uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.displayColor;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) { // ANIMALS
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {  // Zooplankton
                //textMutationPanelOptionA.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionB.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionC.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionD.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
                Color uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[0].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[1].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[2].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[3].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailD.color = uiColor;

                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            else { // vertebrates
                int slotID = layerManager.selectedTrophicSlotRef.slotID;
                //textMutationPanelOptionA.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].textDescriptionMutation; 
                //textMutationPanelOptionB.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].textDescriptionMutation; 
                //textMutationPanelOptionC.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].textDescriptionMutation; 
                //textMutationPanelOptionD.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].textDescriptionMutation;

                int speciesID = layerManager.selectedTrophicSlotRef.linkedSpeciesID;
                Vector3 hue0 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailA.color = new Color(hue0.x, hue0.y, hue0.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].displayColor; // new Color(hue0.x, hue0.y, hue0.z); 
                Vector3 hue1 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailB.color = new Color(hue1.x, hue1.y, hue1.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].displayColor; //
                Vector3 hue2 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailC.color = new Color(hue2.x, hue2.y, hue2.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].displayColor; //
                Vector3 hue3 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailD.color = new Color(hue3.x, hue3.y, hue3.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].displayColor; //
                //Vector3 hue0 = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;

                Vector3 hue = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[layerManager.selectedTrophicSlotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                Debug.Log("ADSF: " + hue.ToString());
                //Vector3 hueCur = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                Color thumbCol = new Color(hue.x, hue.y, hue.z); 
                imageMutationPanelCurPortrait.color = thumbCol;
                imageMutationPanelCurPortrait.sprite = null;

                //imageMutationPanelCurPortrait.color = Color.white;
                Vector3 hueNew = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][selectedToolbarMutationID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageToolbarSpeciesPortraitBorder.color = thumbCol; 
                imageMutationPanelNewPortrait.color = new Color(hueNew.x, hueNew.y, hueNew.z); // uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotID].name; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            }
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) { // Terrain
            if (layerManager.selectedTrophicSlotRef.tierID == 0) {
                Color colorOptionA = Color.white;
                Color colorOptionB = Color.white;
                Color colorOptionC = Color.white;
                Color colorOptionD = Color.white;
                Color colorCur = Color.white;
                Color colorNew = Color.white;
                int selectedIndex = selectedToolbarMutationID;
                if(selectedIndex < 0) {
                    selectedIndex = 0;
                    panelNewMutationPreview.SetActive(false);
                }
                else {
                    panelNewMutationPreview.SetActive(true);
                }
                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color;
                    colorOptionD = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[3].color;
                    // *** make these Text objects into an array:
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].textDescriptionMutation; // "Minor Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].textDescriptionMutation; // "Minor Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].textDescriptionMutation; // "Major Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[3].textDescriptionMutation; // "Major Mutation!";
                    colorCur = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color;
                    colorOptionD = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].textDescriptionMutation; // "Minor Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].textDescriptionMutation; // "Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].textDescriptionMutation; // "Major Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].textDescriptionMutation; // "Major Stones Mutation!";
                    colorCur = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].color;
                    colorOptionD = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].textDescriptionMutation; // "Minor Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].textDescriptionMutation; // "Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].textDescriptionMutation; // "Major Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].textDescriptionMutation; // "Major Pebbles Mutation!"; 
                    colorCur = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].textDescriptionMutation; // "Minor Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].textDescriptionMutation; // "Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].textDescriptionMutation; // "Major Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].textDescriptionMutation; // "Major Sand Mutation!";
                    colorCur = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.color;    
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                // **** v v v Make these into arrays during cleanup
                colorOptionA.a = 1f;
                colorOptionB.a = 1f;
                colorOptionC.a = 1f;
                colorOptionD.a = 1f;
                colorCur.a = 1f;
                imageMutationPanelThumbnailA.color = colorOptionA;
                imageMutationPanelThumbnailA.sprite = null;
                imageMutationPanelThumbnailB.color = colorOptionB;
                imageMutationPanelThumbnailB.sprite = null;
                imageMutationPanelThumbnailC.color = colorOptionC;
                imageMutationPanelThumbnailC.sprite = null;
                imageMutationPanelThumbnailD.color = colorOptionD;
                imageMutationPanelThumbnailD.sprite = null;

                imageMutationPanelCurPortrait.color = colorCur;

            }
        }
         
    }
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
        gameManager.simulationManager.computeShaderResourceGrid.SetTexture(kernelCSSampleTexture, "_ResourceGridRead", tex);
        gameManager.simulationManager.computeShaderResourceGrid.SetBuffer(kernelCSSampleTexture, "outputValuesCBuffer", outputBuffer);
        gameManager.simulationManager.computeShaderResourceGrid.SetFloat("_CoordX", uv.x);
        gameManager.simulationManager.computeShaderResourceGrid.SetFloat("_CoordY", uv.y); 
        // DISPATCH !!!
        gameManager.simulationManager.computeShaderResourceGrid.Dispatch(kernelCSSampleTexture, 1, 1, 1);
        
        outputBuffer.GetData(sample);

        outputBuffer.Release();

        return sample[0];
    }
    private string GetSpiritBrushSummary(TrophicLayersManager layerManager) {
        string str = "";

        switch(curActiveTool) {
            case ToolType.None:
                //buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;

                break;
            case ToolType.Watcher:

                


                str = "This spirit reveals\nhidden information.";
                str += "\n\n";
                

                
                break;
            case ToolType.Mutate:
                //buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor;
                break;
            case ToolType.Add:
                str = "This spirit creates stuff.\n\nAlt Effect: Removes stuff.";
                break;
            case ToolType.Remove:
                //buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;
                //buttonToolbarRemove.gameObject.transform.localScale = Vector3.one * 1.25f;
                break;
            case ToolType.Stir:
                str = "This spirit pushes material around.\n\nAlt Effect: None";
                break;
            default:
                break;

        }

        return str;
    }
        
    public void UpdateSpeciesTreeDataTextures(int year) {  // refactor using year?

        int numActiveSpecies = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int numTotalSpecies = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count;

        //gameManager.simulationManager.masterGenomePool.UpdateYearlySpeciesStats(year);
                
        //statsGraphMatLifespan.SetInt("_NumDisplayed", maxDisplaySpecies); 
        //statsGraphMatFoodEaten.SetInt("_NumDisplayed", maxDisplaySpecies); 
        //statsGraphMatBodySizes.SetInt("_NumDisplayed", maxDisplaySpecies); 
        //statsGraphMatPredation.SetInt("_NumDisplayed", maxDisplaySpecies); 
        // need way to sort & pick the current 8 species to display
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

    private void UpdateToolbarWingStatsPanel() {
        imageToolbarSpeciesStatsGraph.gameObject.SetActive(false);

        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;

        string descriptionText = "";
        
        if(slot.kingdomID == 0) {
            descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalDecomposers.ToString("F1") + "</b></size>\n\n";

            descriptionText += "<color=#FBC653FF>Nutrient Production: <b>" + gameManager.simulationManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Processed: <b>" + gameManager.simulationManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";

            if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
                textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>10</i></b> Total Biomass";
                float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalDecomposers / 10f);
                textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                //imageUnlockMeter;
                matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                imageUnlockMeter.gameObject.SetActive(true);
            }
            else {
                textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                imageUnlockMeter.gameObject.SetActive(false);
            }
        }
        else if(slot.kingdomID == 1) {
            descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalPlantParticles.ToString("F1") + "</b></size>\n\n";

            descriptionText += "<color=#8EDEEEFF>Oxygen Production: <b>" + gameManager.simulationManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";

            // *************** GROSS CODE ALERT!!!!   temp hack!!!! *****************
            if(gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
                textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>150</i></b> Total Biomass";
                float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalPlantParticles / 150f);
                textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                //imageUnlockMeter;
                matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                imageUnlockMeter.gameObject.SetActive(true);
            }
            else {
                textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                imageUnlockMeter.gameObject.SetActive(false);
            }
        }
        else {
            if(slot.tierID == 0) {  // ZOOPLANKTON
                descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles.ToString("F1") + "</b></size>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";
                //descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";

                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>6</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles / 6f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    imageUnlockMeter.gameObject.SetActive(false);
                }
            }
            else {  // AGENTS
                imageToolbarSpeciesStatsGraph.gameObject.SetActive(true);

                SpeciesGenomePool selectedPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
                
                descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b></size>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString("F3") + "</b></color>\n";

                descriptionText += "<color=#8BD06AFF>Avg Food Consumed: <b>" + (selectedPool.avgConsumptionPlant + selectedPool.avgConsumptionMeat).ToString("F3") + "</b></color>\n";
                //descriptionText += "Avg Meat Consumed: <b>" + selectedPool.avgConsumptionMeat.ToString("F3") + "</b>\n\n";  
                //descriptionText += "Descended From Species: <b>" + selectedPool.parentSpeciesID.ToString() + "</b>\n";
                //descriptionText += "Year Evolved: <b>" + selectedPool.yearCreated.ToString() + "</b>\n\n";
                descriptionText += "\n\n\nAvg Lifespan: <b>" + (selectedPool.avgLifespan / 1500f).ToString("F1") + " Years</b>\n\n";

                selectedPool.representativeGenome.bodyGenome.CalculateFullsizeBoundingBox();
                descriptionText += "Avg Body Size: <b>" + ((selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.x + selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.y) * 0.5f * selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.z).ToString("F2") + "</b>\n";
                                                        //+ selectedPool.avgBodySize.ToString("F2") + "</b>\n";
                descriptionText += "Avg Brain Size: <b>" + ((selectedPool.avgNumNeurons + selectedPool.avgNumAxons) * 0.1f).ToString("F1") + "</b>\n";
                //descriptionText += "Avg Axon Count: <b>" + selectedPool.avgNumAxons.ToString("F0") + "</b>\n\n";
        
                toolbarSpeciesStatsGraphMat.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[0]); // statsTreeOfLifeSpeciesTexArray[0]);
                toolbarSpeciesStatsGraphMat.SetTexture("_ColorKeyTex", statsSpeciesColorKey); // statsTreeOfLifeSpeciesTexArray[0]);
                toolbarSpeciesStatsGraphMat.SetFloat("_MinValue", 0f);
                toolbarSpeciesStatsGraphMat.SetFloat("_MaxValue", maxValuesStatArray[0]);
                toolbarSpeciesStatsGraphMat.SetFloat("_NumEntries", (float)statsTreeOfLifeSpeciesTexArray[0].width);
                toolbarSpeciesStatsGraphMat.SetFloat("_SelectedSpeciesID", slot.slotID);

                // TEMP UNLOCK TEXT:
                // Slot 4/4:
                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>8</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass / 8f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    imageUnlockMeter.gameObject.SetActive(false);
                }
                // Slot 3/4:
                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>4</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass / 4f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    //textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    //textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    //imageUnlockMeter.gameObject.SetActive(false);
                }
                // Slot 2/4:
                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>1</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass / 1f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    //textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    //textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    //imageUnlockMeter.gameObject.SetActive(false);
                }
            }
        }             
        
        textSelectedSpeciesDescription.text = descriptionText;
    }

    private void InitToolbarPortraitCritterData(TrophicSlot slot) { // ** should be called AFTER new species actually created?? something is fucked here
        Debug.Log("InitToolbarPortraitCritterData.. selectedSpeciesID: " + selectedSpeciesID.ToString() + " , slotLinkedID: " + slot.linkedSpeciesID.ToString());

        //int selectedSpeciesID = 0; // simManager.uiManager.selectedSpeciesID;
        /*
        if (selectedSpeciesID < 0) {
            selectedSpeciesID = 0;  // Temporary catch
        }
        */

        SpeciesGenomePool speciesPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        //gameManager.theRenderKing.InitializeNewCritterPortraitGenome(speciesPool.representativeGenome);
        gameManager.theRenderKing.InitializeNewCritterPortraitGenome(gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome); // speciesPool.leaderboardGenomesList[0].candidateGenome);
        gameManager.theRenderKing.isToolbarCritterPortraitEnabled = true;
                
        //Vector3 speciesHuePrimary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        //Vector3 speciesHueSecondary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID].representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
        //imageToolbarSpeciesPortraitRender.color = new Color(speciesHueSecondary.x, speciesHueSecondary.y, speciesHueSecondary.z);
        //imageTolSpeciesIndex.color = new Color(speciesHueSecondary.x, speciesHueSecondary.y, speciesHueSecondary.z);
        //imageTolSpeciesNameBackdrop.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
        //imageTolSpeciesReadoutBackdrop.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);

        //gameManager.simulationManager.theRenderKing.UpdateCritterPortraitStrokesData(gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID].representativeGenome);
    }

    private void SetToolbarButtonStateUI(ref Button button, TrophicSlot.SlotStatus slotStatus, bool isSelected) {

        button.gameObject.SetActive(true);
        Animation anim = button.GetComponent<Animation>();
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
            scale = Vector3.one * 1.2f;
            colBlock.colorMultiplier = 1.2f;
        }

        button.colors = colBlock;

        /*if(isSelected) {
            scale = Vector3.one * 1.33f;
            ColorBlock colorBlock = button.colors;
            colorBlock.colorMultiplier = 1.4f;
            button.colors = colorBlock;
            //button.GetComponent<Image>().sprite = spriteSpeciesSlotSelected;
        }
        else {            
            ColorBlock colorBlock = button.colors;
            colorBlock.colorMultiplier = 0.9f;
            button.colors = colorBlock;
        }*/

        button.gameObject.transform.localScale = scale;
    }
    /*private void SetInfoSpeciesButtonStateUI(ref Button button, TrophicSlot.SlotStatus slotStatus) {

        button.gameObject.SetActive(true);
        switch(slotStatus) {
            case TrophicSlot.SlotStatus.Off:
                button.gameObject.SetActive(false);                
                break;
            case TrophicSlot.SlotStatus.Locked:
                button.interactable = false;                
                button.GetComponentInChildren<Text>().text = "-";
                break;
            case TrophicSlot.SlotStatus.Empty:
                button.interactable = false;                
                button.GetComponentInChildren<Text>().text = "-";
                break;
            case TrophicSlot.SlotStatus.On:
                button.interactable = true;
                button.GetComponentInChildren<Text>().text = "0";
                break;
            default:
                break;
        }
    }
    */

    private void UpdateTolWorldStatsTextReadout() {        
        //textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");
    }
    private void UpdateTolSpeciesStatsTextReadout() {
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)selectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");
    }
        
    public void UpdateStatsTextureBodySizes(List<Vector4> data) {
        statsTextureBodySizes.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i].x);
            maxValue = Mathf.Max(maxValue, data[i].y);
            maxValue = Mathf.Max(maxValue, data[i].z);
            maxValue = Mathf.Max(maxValue, data[i].w);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureBodySizes.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTextureBodySizes.Apply();

        //maxBodySizeValue = maxValue;    
    }
    public void UpdateStatsTextureFoodEaten(List<Vector4> data) {
        statsTextureFoodEaten.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i].x);
            maxValue = Mathf.Max(maxValue, data[i].y);
            maxValue = Mathf.Max(maxValue, data[i].z);
            maxValue = Mathf.Max(maxValue, data[i].w);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureFoodEaten.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTextureFoodEaten.Apply();

        //maxFoodEatenValue = maxValue;     
    }
    public void UpdateStatsTexturePredation(List<Vector4> data) {
        statsTexturePredation.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 1f;
        
        for(int i = 0; i < data.Count; i++) {
            statsTexturePredation.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTexturePredation.Apply();

        //maxPredationValue = maxValue;     
    }
    public void UpdateTolWorldStatsTexture(List<Vector4> nutrientData) {
        
        
        //Debug.Log("UpdateTolWorldStatsTexture");
        int numDataPoints = Mathf.Max(1, nutrientData.Count);

        /*if(numDataPoints != textureWorldStats.width) {  // resize needed?
            textureWorldStats.Resize(numDataPoints, 32);  // 32 max values? should be more than enough
        }*/

        if(numDataPoints != infoOxygenDataTexture.width) {
            infoOxygenDataTexture.Resize(numDataPoints, 1);
            infoNutrientsDataTexture.Resize(numDataPoints, 1);
            infoDetritusDataTexture.Resize(numDataPoints, 1);
            infoDecomposersDataTexture.Resize(numDataPoints, 1);
            infoPlantsDataTexture.Resize(numDataPoints, 1);
            infoAnimalsDataTexture.Resize(numDataPoints, 1);
        }

        // INDEX KEY:
        // 0 == decay nutrients
        // 1 == plant food
        // 2 == eggs food
        // 3 = corpse food
        // 4 = brain mutation freq
        // 5 = brain mutation amp
        // 6 = brain size bias
        // 7 = body proportion freq
        // 8 = body proportion amp
        // 9 = body sensor mutation rate
        // 10 = water current / storminess
        
        // NUTRIENTS / FOOD TYPES:          
        for(int i = 0; i < numDataPoints; i++) {   
            
            infoOxygenDataTexture.SetPixel(i, 0, new Color(gameManager.simulationManager.statsHistoryOxygenList[i], 0f, 0f));
            infoNutrientsDataTexture.SetPixel(i, 0, new Color(gameManager.simulationManager.statsHistoryNutrientsList[i], 0f, 0f));
            infoDetritusDataTexture.SetPixel(i, 0, new Color(gameManager.simulationManager.statsHistoryDetritusList[i], 0f, 0f));
            infoDecomposersDataTexture.SetPixel(i, 0, new Color(gameManager.simulationManager.statsHistoryDecomposersList[i], 0f, 0f));
            infoPlantsDataTexture.SetPixel(i, 0, new Color(gameManager.simulationManager.statsHistoryAlgaeParticleList[i] + gameManager.simulationManager.statsHistoryAlgaeSingleList[i], 0f, 0f));
            infoAnimalsDataTexture.SetPixel(i, 0, new Color(gameManager.simulationManager.statsHistoryZooplanktonList[i] + gameManager.simulationManager.statsHistoryLivingAgentsList[i], 0f, 0f));
            
            // NEW:
            /*float value = gameManager.simulationManager.statsHistoryOxygenList[i];
            tolWorldStatsValueRangesKeyArray[0].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[0].x, value);
            tolWorldStatsValueRangesKeyArray[0].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[0].y, value);
            textureWorldStats.SetPixel(i, 0, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryNutrientsList[i];
            tolWorldStatsValueRangesKeyArray[1].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[1].x, value);
            tolWorldStatsValueRangesKeyArray[1].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[1].y, value);
            textureWorldStats.SetPixel(i, 1, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryDetritusList[i];
            tolWorldStatsValueRangesKeyArray[2].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[2].x, value);
            tolWorldStatsValueRangesKeyArray[2].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[2].y, value);
            textureWorldStats.SetPixel(i, 2, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryDecomposersList[i];
            tolWorldStatsValueRangesKeyArray[3].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[3].x, value);
            tolWorldStatsValueRangesKeyArray[3].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[3].y, value);
            textureWorldStats.SetPixel(i, 3, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryAlgaeSingleList[i];
            tolWorldStatsValueRangesKeyArray[4].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[4].x, value);
            tolWorldStatsValueRangesKeyArray[4].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[4].y, value);
            textureWorldStats.SetPixel(i, 4, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryAlgaeParticleList[i];
            tolWorldStatsValueRangesKeyArray[5].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[5].x, value);
            tolWorldStatsValueRangesKeyArray[5].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[5].y, value);
            textureWorldStats.SetPixel(i, 5, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryZooplanktonList[i];
            tolWorldStatsValueRangesKeyArray[6].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[6].x, value);
            tolWorldStatsValueRangesKeyArray[6].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[6].y, value);
            textureWorldStats.SetPixel(i, 6, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryLivingAgentsList[i];
            tolWorldStatsValueRangesKeyArray[7].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[7].x, value);
            tolWorldStatsValueRangesKeyArray[7].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[7].y, value);
            textureWorldStats.SetPixel(i, 7, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryDeadAgentsList[i];
            tolWorldStatsValueRangesKeyArray[8].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[8].x, value);
            tolWorldStatsValueRangesKeyArray[8].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[8].y, value);
            textureWorldStats.SetPixel(i, 8, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryEggSacksList[i];
            tolWorldStatsValueRangesKeyArray[9].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[9].x, value);
            tolWorldStatsValueRangesKeyArray[9].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[9].y, value);
            textureWorldStats.SetPixel(i, 9, new Color(value, 0f, 0f));

            // WATER / WEATHER STATS:
            //

            value = gameManager.simulationManager.statsHistoryWaterCurrentsList[i];
            tolWorldStatsValueRangesKeyArray[10].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[10].x, value);
            tolWorldStatsValueRangesKeyArray[10].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[10].y, value);
            textureWorldStats.SetPixel(i, 10, new Color(value, 0f, 0f));
            */
        }        
        
        // Min/Max values for each stat stored in second pixel --> set from valueRangeKeyArray, x = min, y = max
        //for(int i = 0; i < tolWorldStatsValueRangesKeyArray.Length; i++) {
            //textureWorldStatsKey.SetPixel(1, i, new Color(tolWorldStatsValueRangesKeyArray[i].x, tolWorldStatsValueRangesKeyArray[i].y, 0f));
        //}

        //textureWorldStats.Apply();
        //textureWorldStatsKey.Apply();


        infoOxygenDataTexture.Apply();
        infoNutrientsDataTexture.Apply();
        infoDetritusDataTexture.Apply();
        infoDecomposersDataTexture.Apply();
        infoPlantsDataTexture.Apply();
        infoAnimalsDataTexture.Apply();
    }
    #endregion

    #region UTILITY & EVENT FUNCTIONS

    private void RemoveSpeciesSlot() {
                
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
        //gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = false;

        isToolbarDetailPanelOn = false;
        //pressedRemoveSpecies = false;
        //buttonToolbarRemoveDecomposer.gameObject.SetActive(false);
        //buttonToolbarRemovePlant.gameObject.SetActive(false);
        //buttonToolbarRemoveAnimal.gameObject.SetActive(false);
        
        // REMOVE!!!
        // which slot was deleted? selectedID
        //layerManager.selectedTrophicSlotRef.kingdomID;
        //remove from simulation -- need function for this!
        // update trophic layers?
        //deselect
        //SetToolbarButtonStateUI()
        //buttonToolbarRemoveAnimal.gameObject.SetActive(false);
                
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
          
    }

    public void StopFollowingAgent() {
        cameraManager.isFollowingAgent = false;
        
        if(isActiveInspectPanel) {
            isActiveInspectPanel = false;
            //buttonToolInspect.GetComponent<Image>().color = buttonDisabledColor;        
            animatorInspectPanel.enabled = true;
            animatorInspectPanel.Play("SlideOffPanelInspect");
        } 
    }
    public void StartFollowingAgent() {
        cameraManager.isFollowingAgent = true;
        isActiveInspectPanel = true;    
        //animatorInspectPanel.enabled = true;
        //animatorInspectPanel.Play("SlideOnPanelInspect");        
    }

    public void StopFollowingPlantParticle() {
        cameraManager.isFollowingPlantParticle = false;
        
    }
    public void StartFollowingPlantParticle() {
        cameraManager.isFollowingPlantParticle = true;
        cameraManager.isFollowingAnimalParticle = false; 
    }
    public void StopFollowingAnimalParticle() {
        cameraManager.isFollowingAnimalParticle = false;        
    }
    public void StartFollowingAnimalParticle() {
        cameraManager.isFollowingAnimalParticle = true;  
        cameraManager.isFollowingPlantParticle = false;
    }
    
    private void MouseRaycastWaterPlane(Vector3 screenPos) {
        mouseRaycastWaterPlane.SetActive(true);
        //Vector3 camPos = cameraManager.gameObject.transform.position;                
        Ray ray = cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(screenPos);
        RaycastHit hit = new RaycastHit();
        int layerMask = 1 << 12;
        Physics.Raycast(ray, out hit, layerMask);

        if (hit.collider != null) {
            curMousePositionOnWaterPlane = hit.point;  
            // Z = -0.5f ???? Look into that if it's causing a problem:
            prevMousePositionOnWaterPlane = curMousePositionOnWaterPlane;
            //Debug.Log("curMousePositionOnWaterPlane:" + curMousePositionOnWaterPlane.ToString());
        }
    }    
    private void MouseRaycastCheckAgents(bool clicked) {
        
        Vector3 camPos = cameraManager.gameObject.transform.position;
        
        Ray ray = cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        int layerMask = ~(1 << LayerMask.NameToLayer("UtilityRaycast")); 

        Physics.Raycast(ray, out hit, 1000f, layerMask);  // *** USE DEDICATED LAYER FOR THIS CHECK!!!! *********

        cameraManager.isMouseHoverAgent = false;
        cameraManager.mouseHoverAgentIndex = 0;
        cameraManager.mouseHoverAgentRef = null;

        if(hit.collider != null) {
            
            // CHECK FOR AGENT COLLISION:
            Agent agentRef = hit.collider.gameObject.GetComponentInParent<Agent>();
            if(agentRef != null) {
                //Debug.Log("AGENT: [ " + agentRef.gameObject.name + " ] #" + agentRef.index.ToString());
                    
                if(clicked && curActiveTool == ToolType.Watcher) {
                    if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                        if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {
                            cameraManager.SetTargetAgent(agentRef, agentRef.index);
                            cameraManager.isFollowingAgent = true;
                            StartFollowingAgent();
                        }
                    }
                    
                }
                else {
                    // HOVER:
                    //hoverAgentID = agentRef.index;
                    //Debug.Log("HOVER AGENT: " + agentRef.index.ToString() + ", species: " + agentRef.speciesIndex.ToString());
                }

                cameraManager.isMouseHoverAgent = true;
                cameraManager.mouseHoverAgentIndex = agentRef.index;
                cameraManager.mouseHoverAgentRef = agentRef;                    
            }
            else {
                
            }
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            //treeOfLifeManager.HoverAllOff();
        }
    }
    
    /*private void ClickOnSpeciesNode(int ID) {
        Debug.Log("Clicked Species[" + ID.ToString() + "]");

        //treeOfLifeManager.ClickedOnSpeciesNode(ID);
        selectedSpeciesID = ID;

        if (displaySpeciesIndicesArray != null) {
            int id = 0;
            for(int i = 0; i < displaySpeciesIndicesArray.Length; i++) {
                if(displaySpeciesIndicesArray[i] == ID) {
                    id = i;
                    break;
                }
            }
        }

        textSelectedSpeciesIndex.text = selectedSpeciesID.ToString();

        UpdateSelectedSpeciesColorUI();
        
        UpdateSpeciesTreeDataTextures(gameManager.simulationManager.curSimYear); // shouldn't lengthen!
        
        isDraggingSpeciesNode = true; // needed?
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
            animatorInspectPanel.enabled = false;
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
            animatorInspectPanel.enabled = false;
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

    public void ClickToolButtonStir() {
        
        curActiveTool = ToolType.Stir;
             
        isActiveStirToolPanel = true;  
        StopFollowingAgent();
        StopFollowingPlantParticle();
        //animatorStirToolPanel.enabled = true;
        //animatorStirToolPanel.Play("SlideOnPanelStirTool"); 
        buttonToolbarStir.GetComponent<Image>().color = buttonActiveColor; 

        TurnOffInspectTool();  
        TurnOffAddTool();
        TurnOffMutateTool();
        TurnOffRemoveTool();
        TurnOffKnowledgeTool();

        isSpiritBrushSelected = true;
              
    }
    public void ClickToolButtonInspect() {
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Inspect) {
        curActiveTool = ToolType.Watcher;
        
        TurnOffStirTool();  
        TurnOffAddTool();
        TurnOffMutateTool();
        TurnOffRemoveTool();
        TurnOffKnowledgeTool();

        buttonToolbarWatcher.GetComponent<Image>().color = buttonActiveColor; 
        isSpiritBrushSelected = true;

    }
    public void ClickToolButtonAdd() {
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Nutrients) {
        curActiveTool = ToolType.Add;

        StopFollowingAgent();
        StopFollowingPlantParticle();
        StopFollowingAnimalParticle();
        //isActiveFeedToolPanel = true;    
        //animatorFeedToolPanel.enabled = true;
        //animatorFeedToolPanel.Play("SlideOnPanelFeedTool"); 
        buttonToolbarAdd.GetComponent<Image>().color = buttonActiveColor;

        TurnOffInspectTool();
        TurnOffStirTool();
        TurnOffMutateTool();
        TurnOffRemoveTool();
        TurnOffKnowledgeTool();
            
        isSpiritBrushSelected = true;
        //}  
        /*else {
            curActiveTool = ToolType.None;
            TurnOffNutrientsTool();
        } */
    }
    public void ClickToolButtonKnowledge() {
        
        curActiveTool = ToolType.Sage;

        StopFollowingAgent();
        StopFollowingPlantParticle();
        StopFollowingAnimalParticle();

        buttonToolbarKnowledge.GetComponent<Image>().color = buttonActiveColor;

        TurnOffAddTool();
        TurnOffInspectTool();
        TurnOffStirTool();
        TurnOffMutateTool();
        TurnOffRemoveTool();
            
        isSpiritBrushSelected = true;        
    }
    public void ClickToolButtonRemove() {
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Remove) {
            curActiveTool = ToolType.Remove;

            //isActiveFeedToolPanel = true;    
            //animatorFeedToolPanel.enabled = true;
            //animatorFeedToolPanel.Play("SlideOnPanelFeedTool"); 
            //buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;

            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffMutateTool();
            TurnOffAddTool();
        //}  
        /*else {
            curActiveTool = ToolType.None;
            TurnOffRemoveTool();
        } */
    }
    
    private void TurnOffKnowledgeTool() {
                
        buttonToolbarKnowledge.GetComponent<Image>().color = buttonDisabledColor;  
        //buttonToolbarInspect.GetComponent<Animator>().enabled = false;       
        isActiveInspectPanel = false;
        //StopFollowing();
    }
    private void TurnOffInspectTool() {
                
        buttonToolbarWatcher.GetComponent<Image>().color = buttonDisabledColor;  
        //buttonToolbarInspect.GetComponent<Animator>().enabled = false;       
        isActiveInspectPanel = false;
        //StopFollowing();
    }
    private void TurnOffAddTool() {
        buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;

        //if(isActiveFeedToolPanel) {
        //    animatorFeedToolPanel.enabled = true;
        //    animatorFeedToolPanel.Play("SlideOffPanelFeedTool");
        //}        
        //isActiveFeedToolPanel = false;
    }
    private void TurnOffMutateTool() {
        //buttonToolbarMutate.GetComponent<Image>().color = buttonDisabledColor;

        //if(isActiveMutateToolPanel) {
        //    animatorMutateToolPanel.enabled = true;
        //    animatorMutateToolPanel.Play("SlideOffPanelMutateTool");
        //}        
        //isActiveMutateToolPanel = false;
    }
    private void TurnOffStirTool() {
        //buttonToolbarStir.GetComponent<Image>().color = buttonDisabledColor;
        //if(isActiveStirToolPanel) {
        //    animatorStirToolPanel.enabled = true;
        //    animatorStirToolPanel.Play("SlideOffPanelStirTool");
        //}        
        isActiveStirToolPanel = false;
        gameManager.simulationManager.PlayerToolStirOff();
    }
    private void TurnOffRemoveTool() {
        //buttonToolbarRemove.GetComponent<Image>().color = buttonDisabledColor;
        //if(isActiveStirToolPanel) {
        //    animatorStirToolPanel.enabled = true;
        //    animatorStirToolPanel.Play("SlideOffPanelStirTool");
        //}        
        //isActiveStirToolPanel = false;
    }
    public void ClickButtonToolbarOther(int index) {
        Debug.Log("ClickButtonToolbarOther: " + index.ToString());

        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;

        selectedSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedToolbarOtherLayer = index;

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarTerrain(int index) {
        Debug.Log("ClickButtonToolbarTerrain: " + index.ToString());

        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;

        selectedSpeciesID = slot.linkedSpeciesID; // update this next

        //curActiveTool = ToolType.None;


        /*if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.status != TrophicSlot.SlotStatus.Empty) {
            InitToolbarPortraitCritterData(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef); // ***
        }*/

        selectedToolbarTerrainLayer = index;

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarDecomposers() {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;

        if(gameManager.simulationManager.trophicLayersManager.GetDecomposersOnOff()) {
            /*if (slot.status == TrophicSlot.SlotStatus.On) {            
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlot = true;
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;  
                buttonSelectedTrophicSlot = buttonToolbarDecomposers;
            }*/
        }
        else {
            
            //gameManager.simulationManager.trophicLayersManager.PendingDecomposers();
            //buttonPendingTrophicSlot = buttonToolbarDecomposers;
            
        }

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }
        //curActiveTool = ToolType.None;
        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];

        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarPlants(int slotID) {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];

        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarZooplankton() {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
        
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;
        
        if(gameManager.simulationManager.trophicLayersManager.GetZooplanktonOnOff()) {
            /*if (slot.status == TrophicSlot.SlotStatus.On) {            
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlot = true;
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot; 
                buttonSelectedTrophicSlot = buttonToolbarZooplankton;
            } */
        }
        else {
            
            //gameManager.simulationManager.trophicLayersManager.PendingZooplankton();
            //buttonPendingTrophicSlot = buttonToolbarZooplankton;
            
        }

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }
        //curActiveTool = ToolType.None;
        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarAgent(int index) {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarDetailPanelOn = true;

        selectedSpeciesID = slot.linkedSpeciesID; // update this next

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();


        }
        //curActiveTool = ToolType.None;


        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.status != TrophicSlot.SlotStatus.Unlocked) {
            InitToolbarPortraitCritterData(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef); // ***
        }

        isSpiritBrushSelected = false;
        // Why do I have to click this twice before portrait shows up properly??????
    }

    public void ClickToolbarRemoveSpeciesSlot() {
        //pressedRemoveSpecies = true;  // Sets remove flag for update loop:       
    }
    public void ClickToolbarExpandOn() {
        isToolbarPaletteExpandOn = true;
        //imageHighlightNewUI.gameObject.SetActive(false);
        //buttonToolbarExpandOn.GetComponent<Animation>().Stop();
        //buttonToolbarExpandOn.GetComponent<Animator>().StopPlayback();
        buttonToolbarPaletteExpandOn.GetComponent<Animator>().enabled = false;
    }
    public void ClickToolbarExpandOff() {
        isToolbarPaletteExpandOn = false;
        buttonToolbarPaletteExpandOn.GetComponent<Animator>().enabled = false;
    }

    public void ClickSpiritBrushThumbnail() {
        isToolbarPaletteExpandOn = true;
        isSpiritBrushSelected = true;
    }

    public void ClickPaletteSlotThumbnail() {
        isToolbarPaletteExpandOn = true;
        isSpiritBrushSelected = false;
    }

    public void ClickToolbarWingClose() {
        isToolbarDetailPanelOn = false;
        //gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = false;
        
    }
    /*public void AnnounceUnlockInspect() {
        
        buttonToolbarInspect.GetComponent<Animator>().enabled = true;

        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Watcher Spirit Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.75f, 0.75f, 0.75f);
        panelPendingClickPrompt.GetComponent<Image>().raycastTarget = true;
        isAnnouncementTextOn = true;

        buttonToolbarInspect.interactable = true;
        buttonToolbarInspect.image.sprite = spriteSpiritBrushWatcherIcon;

        inspectToolUnlockedAnnounce = true;
        //ClickToolButtonInspect();

        //if(isToolbarExpandOn) {
        //    ClickToolbarExpandOff();
        //}
    }*/
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
    public void ClickToolbarCreateNewSpecies() {
        // questionable code, possibly un-needed:
        gameManager.simulationManager.trophicLayersManager.CreateTrophicSlotSpecies(gameManager.simulationManager, cameraManager.curCameraFocusPivotPos, gameManager.simulationManager.simAgeTimeSteps);
                

        gameManager.theRenderKing.baronVonWater.StartCursorClick(cameraManager.curCameraFocusPivotPos);
        
        //isAnnouncementTextOn = true;

        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
            panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Decomposer added!";
            panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorDecomposersLayer;
            //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        }
        else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {
                panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Algae added!";
                panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorAlgaeLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
            }
            else {   /// BIG PLANTS:
                panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of PLAN%T added!";
                panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorPlantsLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
            }
            
        }
        else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) { // ANIMALS
            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {
                //if(createSpecies) {
                // v v v Actually creates new speciesPool here:::
                TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
                slot.speciesName = "Vertebrate " + (slot.slotID + 1).ToString();
                gameManager.simulationManager.CreateAgentSpecies(cameraManager.curCameraFocusPivotPos);
                
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID].linkedSpeciesID = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1].speciesID;

                // *** IMPORTANT::::
                int speciesIndex = slot.linkedSpeciesID;
                gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].representativeGenome;
                gameManager.simulationManager.masterGenomePool.GenerateWorldLayerVertebrateGenomeMutationOptions(slot.slotID, slot.linkedSpeciesID);

                Debug.Log("ADSF: " + gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary.ToString());

                // duplicated code shared with clickAgentButton :(   bad 
                //
                //gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
                //gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
                //isToolbarWingOn = true;
                //selectedSpeciesID = slot.linkedSpeciesID; // update this next
                                
                selectedSpeciesID = slot.linkedSpeciesID; // ???
                InitToolbarPortraitCritterData(slot);                
                
                panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Vertebrate added!";
                panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorVertebratesLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
                
                if(slot.slotID == 0) {
                    //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "These creatures start with randomly-generated brains\n and must evolve successful behavior\nthrough survival of the fittest";
                    isAnnouncementTextOn = false;  // *** hacky...
                }
                //ClickToolButtonInspect();
                
            }
            else {
                panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Zooplankton added!";
                panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorVertebratesLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
            }
        }

        //curToolbarWingPanelSelectID = 1;

        
        timerAnnouncementTextCounter = 0;
        recentlyCreatedSpecies = true;
        recentlyCreatedSpeciesTimeStepCounter = 0;
        //UpdateSelectedSpeciesColorUI();
    }
    public void CheatUnlockAll() {
        Debug.Log("Cheat!!! Unlocked all species!!!");

        gameManager.simulationManager.trophicLayersManager.CheatUnlockAll();
    }
    public void ClickToolbarWingDescription() {
        curToolbarWingPanelSelectID = 0;
        //isToolbarWingDescriptionOn = true;
        //isToolbarWingStatsOn = false;
        //isToolbarWingMutationOn = false;
        isToolbarDeletePromptOn = false;
    } 
    public void ClickToolbarWingStats() {
        curToolbarWingPanelSelectID = 1;
        //isToolbarWingStatsOn = true;
        //isToolbarWingMutationOn = false;
        isToolbarDeletePromptOn = false;
        //isToolbarWingDescriptionOn = false;
    }   
    public void ClickToolbarWingMutation() {
        curToolbarWingPanelSelectID = 2;
        //isToolbarWingMutationOn = true;
        //isToolbarWingStatsOn = false;
        isToolbarDeletePromptOn = false;
        //isToolbarWingDescriptionOn = false;
    }
    public void ClickToolbarWingDelete() {
        isToolbarDeletePromptOn = true;
        //isToolbarWingMutationOn = false;
        //isToolbarWingStatsOn = false; 
        //isToolbarWingDescriptionOn = false;
    }
    public void ClickToolbarWingDeleteCancel() {
        isToolbarDeletePromptOn = false;
        // or summary?
    }
    public void ClickToolbarWingDeleteConfirm() {
        isToolbarDeletePromptOn = false;

        RemoveSpeciesSlot();
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
    public void ClickButtonPlaySlow() {
        Time.timeScale = 0.4f;
    }
    public void ClickButtonPlayNormal() {
        Time.timeScale = 1f;
    }
    public void ClickButtonPlayFast() {
        Time.timeScale = 2.5f;
    }
    
    
    public void ClickButtonToggleDebug() {
        isActiveDebug = !isActiveDebug;
        //gameManager.theRenderKing.isDebugRender = isActiveDebug;
        panelDebug.SetActive(isActiveDebug);
    }
    
    public void ClickPrevAgent() {
        Debug.Log("ClickPrevAgent");
        /*
        int newIndex = cameraManager.targetAgentIndex;
        for(int i = 1; i < gameManager.simulationManager._NumAgents; i++) {
            int index = (gameManager.simulationManager._NumAgents + cameraManager.targetAgentIndex - i) % gameManager.simulationManager._NumAgents;
            
            if (gameManager.simulationManager.agentsArray[index].speciesIndex == selectedSpeciesID) {
                newIndex = index;
                break;
            }
        }
        */
        int newIndex = (gameManager.simulationManager._NumAgents + cameraManager.targetAgentIndex - 1) % gameManager.simulationManager._NumAgents;
        cameraManager.SetTargetAgent(gameManager.simulationManager.agentsArray[newIndex], newIndex);          
                           
    }
    public void ClickNextAgent() {
        Debug.Log("ClickNextAgent");
        /*
        int newIndex = cameraManager.targetAgentIndex;
        for(int i = 1; i < gameManager.simulationManager._NumAgents; i++) {
            int index = (cameraManager.targetAgentIndex + i) % gameManager.simulationManager._NumAgents;

            if (gameManager.simulationManager.agentsArray[index].speciesIndex == selectedSpeciesID) {
                newIndex = index;
                break;
            }
        }
        */
        int newIndex = (cameraManager.targetAgentIndex + 1) % gameManager.simulationManager._NumAgents;
        cameraManager.SetTargetAgent(gameManager.simulationManager.agentsArray[newIndex], newIndex);                
    }
    
    public void ClickInfoPanelExpand() {
        isActiveInfoPanel = !isActiveInfoPanel;

    }

    public void ClickToolbarInspectInfoPanelPageCycle() {
        brainDisplayOn = !brainDisplayOn;
    }

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
    private void CreateDebugRenderViewerArray() {
        debugTextureViewerArray = new RenderTexture[13];
        debugTextureViewerArray[0] = gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT;
        debugTextureViewerArray[0].name = "Terrain Height Data";
        //if (gameManager.theRenderKing.baronVonTerrain.terrainColorRT0 != null) {
        debugTextureViewerArray[1] = gameManager.theRenderKing.baronVonTerrain.terrainColorRT0;
        debugTextureViewerArray[1].name = "Terrain Color";

        debugTextureViewerArray[2] = gameManager.theRenderKing.baronVonWater.waterSurfaceDataRT0;
        debugTextureViewerArray[2].name = "Water Surface Data";

        debugTextureViewerArray[3] = gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[3].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[4] = gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[4].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[5] = gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[5].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[6] = gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[6].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[7] = gameManager.theRenderKing.fluidManager._ObstaclesRT;
        debugTextureViewerArray[7].name = "Solid Obstacles Render";

        debugTextureViewerArray[8] = gameManager.simulationManager.vegetationManager.critterNearestPlants32;
        debugTextureViewerArray[8].name = "critterNearestPlants32";        
        
        debugTextureViewerArray[9] = gameManager.simulationManager.zooplanktonManager.critterNearestZooplankton32;
        debugTextureViewerArray[9].name = "critterNearestZooplankton32";

        debugTextureViewerArray[10] = gameManager.simulationManager.vegetationManager.resourceGridRT1;
        debugTextureViewerArray[10].name = "Resources Grid";

        
        //}
        //if(gameManager.theRenderKing.spiritBrushRT != null) {
        debugTextureViewerArray[11] = gameManager.theRenderKing.spiritBrushRT;
        debugTextureViewerArray[11].name = "Spirit Brush";
        //}        
        //if(gameManager.simulationManager.environmentFluidManager._DensityA != null) {
        //debugTextureViewerArray[3] = gameManager.simulationManager.environmentFluidManager._DensityA;
        //debugTextureViewerArray[3].name = "Water DensityA";
        //}        
        
        debugTextureViewerArray[12] = gameManager.simulationManager.vegetationManager.resourceSimTransferRT;
        debugTextureViewerArray[12].name = "Resource Sim Transfer";
    }
    public void ClickDebugTexturePrev() {
        if(debugTextureViewerArray == null) {
            CreateDebugRenderViewerArray();
        }

        _DebugTextureIndex -= 1;
        if(_DebugTextureIndex < 0) {
            _DebugTextureIndex = debugTextureViewerArray.Length - 1;
        }
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
        debugTextureViewerMat.SetTexture("_MainTex", debugTextureViewerArray[_DebugTextureIndex]);
    }
    public void ClickDebugTextureNext() {
        if(debugTextureViewerArray == null) {
            CreateDebugRenderViewerArray();
        }

        _DebugTextureIndex += 1;
        if(_DebugTextureIndex > debugTextureViewerArray.Length - 1) {
            _DebugTextureIndex = 0;
        }
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
        debugTextureViewerMat.SetTexture("_MainTex", debugTextureViewerArray[_DebugTextureIndex]);
    }
    public void SliderDebugTextureZoomX(float val) {
        _Zoom.x = val;
    }
    public void SliderDebugTextureZoomY(float val) {
        _Zoom.y = val;
    }
    public void SliderDebugTextureAmplitude(float val) {
        _Amplitude = val;
    }
    public void SliderDebugTextureChannelSoloIndex(float val) {
        _ChannelSoloIndex = Mathf.RoundToInt(val);
        debugTextureViewerMat.SetInt("_ChannelSoloIndex", _ChannelSoloIndex);
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
    }
    public void ToggleDebugTextureIsSolo(bool val) {
        if(_IsChannelSolo > 0.5f) {
            _IsChannelSolo = 0f;
        }
        else {
            _IsChannelSolo = 1f;
        }
        debugTextureViewerMat.SetFloat("_IsChannelSolo", (float)_IsChannelSolo);
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;

    }
    public void SliderDebugTextureGamma(float val) {
        _Gamma = val;
    }

    public void ClickWatcherVertebratePageCyclePrev() {
        curWatcherPanelVertebratePageNum--;

        if(curWatcherPanelVertebratePageNum < 0) {
            curWatcherPanelVertebratePageNum = 3;
        }
    }
    public void ClickWatcherVertebratePageCycleNext() {
        curWatcherPanelVertebratePageNum++;

        if(curWatcherPanelVertebratePageNum > 3) {
            curWatcherPanelVertebratePageNum = 0;
        }
    }

    public void ClickWatcherCycleTargetPrev() {
        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {

            }
            else {
                VegetationManager veggieRef = gameManager.simulationManager.vegetationManager;
                veggieRef.selectedPlantParticleIndex--;
                if(veggieRef.selectedPlantParticleIndex < 0) {
                    veggieRef.selectedPlantParticleIndex = veggieRef.plantParticlesCBuffer.count - 1;
                    veggieRef.isPlantParticleSelected = true; // ???
                }
            }
        }
        else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
            if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {
                ZooplanktonManager zoopRef = gameManager.simulationManager.zooplanktonManager;
                zoopRef.selectedAnimalParticleIndex--;
                if (zoopRef.selectedAnimalParticleIndex < 0) {
                    zoopRef.selectedAnimalParticleIndex = zoopRef.animalParticlesCBuffer.count - 1;
                    zoopRef.isAnimalParticleSelected = true; // ???
                }
            }
            else {
                ClickPrevAgent();
            }
        }        
    }
    public void ClickWatcherCycleTargetNext() {
        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {

            }
            else {
                VegetationManager veggieRef = gameManager.simulationManager.vegetationManager;
                veggieRef.selectedPlantParticleIndex++;
                if(veggieRef.selectedPlantParticleIndex > veggieRef.plantParticlesCBuffer.count - 1) {
                    veggieRef.selectedPlantParticleIndex = 0;
                    veggieRef.isPlantParticleSelected = true; // ???
                }
            }
        }
        else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
            if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {
                ZooplanktonManager zoopRef = gameManager.simulationManager.zooplanktonManager;
                zoopRef.selectedAnimalParticleIndex++;
                if (zoopRef.selectedAnimalParticleIndex > zoopRef.animalParticlesCBuffer.count - 1) {
                    zoopRef.selectedAnimalParticleIndex = 0;
                    zoopRef.isAnimalParticleSelected = true; // ???
                }
            }
            else {
                ClickNextAgent();
            }
        } 
    }


    #endregion
}



#region OLD OLD OLD!!!!


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

