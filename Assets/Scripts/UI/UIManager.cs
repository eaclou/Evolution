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
    public Texture2D healthDisplayTex;
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
        Inspect,
        Nutrients,
        Stir,
        Mutate,
        Remove
    }

    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    public Color colorDecomposersLight;
    public Color colorDecomposersDark;
    public Color colorPlantsLight;
    public Color colorPlantsDark;
    public Color colorAnimalsLight;
    public Color colorAnimalsDark;

    public GameObject mouseRaycastWaterPlane;
    private Vector3 prevMousePositionOnWaterPlane;
    public Vector3 curMousePositionOnWaterPlane;

    // &&& INFO PANEL &&& !!!! ==============================================
    public bool isActiveInfoPanel = false;
    public bool isActiveInfoResourcesTab = true;  // toggle between resources tab and species tab
    public GameObject panelInfoExpanded;
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
    public Text textMeterPlants;
    public Text textMeterAnimals;
    // Info Expanded: Resources Overview:
    public GameObject panelInfoResourcesOverview;
    private Texture2D infoOxygenDataTexture;
    private Texture2D infoNutrientsDataTexture;
    private Texture2D infoDetritusDataTexture;
    private Texture2D infoDecomposersDataTexture;
    private Texture2D infoPlantsDataTexture;
    private Texture2D infoAnimalsDataTexture;
    public Material infoGraphOxygenMat;
    public Material infoGraphNutrientsMat;
    public Material infoGraphDetritusMat;
    public Material infoGraphDecomposersMat;
    public Material infoGraphPlantsMat;
    public Material infoGraphAnimalsMat;
    // Info Expanded: Species Overview:
    //public GameObject panelInfoSpeciesOverview;
    //private Texture2D infoSpeciesDecomposersDataTexture;
    //private Texture2D infoSpeciesPlantsDataTexture;
    //private Texture2D infoSpeciesAnimalsDataTexture;


    /*public bool infoSpeciesSelected = false;
    public int infoSpeciesSelectedKingdom = 0;
    public int infoSpeciesSelectedTier = 0;
    public int infoSpeciesSelectedSlot = 0;
    public Button buttonInfoSpeciesDecomposers;
    public Button buttonInfoSpeciesAlgae;
    public Button buttonInfoSpeciesPlant1;
    public Button buttonInfoSpeciesPlant2;
    public Button buttonInfoSpeciesZooplankton;
    public Button buttonInfoSpeciesAnimal1;
    public Button buttonInfoSpeciesAnimal2;
    public Button buttonInfoSpeciesAnimal3;
    public Button buttonInfoSpeciesAnimal4;
    public Text textInfoSpeciesName;
    public Text textInfoSpeciesStats;
    */

    // announcements:
    private bool announceAlgaeCollapsePossible = false;
    private bool announceAlgaeCollapseOccurred = false;
    private bool announceAgentCollapsePossible = false;
    private bool announceAgentCollapseOccurred = false;

    // &&& PLAYER TOOLBAR &&& !!!! ==============================================
    public bool isActiveStirToolPanel = false;
    public bool isToolbarExpandOn = false;
    public bool isToolbarWingOn = false;
    public int curToolbarWingPanelSelectID = 0;  // 0 == description, 1 == stats, 2 == mutate/upgrade
    //public bool isToolbarWingDescriptionOn = false;
    //public bool isToolbarWingStatsOn = true;
    //public bool isToolbarWingMutationOn = false;  // < < <  there's a better way to handle this, but this is simple and works for now
    public bool isToolbarDeletePromptOn = false;
    public int timerAnnouncementTextCounter = 0;
    public bool isAnnouncementTextOn = false;
    public bool isUnlockCooldown = false;
    public int unlockCooldownCounter = 0;
    // portrait
    //public int curToolUnlockLevel = 0;
    public float toolbarInfluencePoints = 0.5f;
    public Text textInfluencePointsValue;
    public Material infoMeterInfluencePointsMat;
    private float addSpeciesInfluenceCost = 0.33f;

    private Button buttonPendingTrophicSlot;  // 
    private Button buttonSelectedTrophicSlot;

    public GameObject panelToolbarExpand;
    public Button buttonToolbarExpandOn;
    public Button buttonToolbarExpandOff;

    public Button buttonToolbarInspect;
    public Button buttonToolbarNutrients;
    public Button buttonToolbarStir;
    public Button buttonToolbarMutate;
    public Button buttonToolbarRemove;

    public GameObject panelPendingClickPrompt;
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
    public GameObject panelToolbarWing;
    public GameObject panelToolbarWingDescription;
    public GameObject panelToolbarWingStats;
    public GameObject panelToolbarWingMutation;
    public GameObject panelToolbarWingDeletePrompt;
    public Button buttonToolbarWingDeleteSpecies;
    public Button buttonToolbarWingDescription;
    public Button buttonToolbarWingStats;
    public Button buttonToolbarWingMutation;
    public Image imageToolbarButtonBarBackground;
    public Button buttonToolbarWingCreateSpecies;
    public Text textToolbarWingSpeciesSummary;
    public Text textSelectedSpeciesTitle;
    public Text textSelectedSpeciesIndex;
    public Image imageToolbarSpeciesPortraitRender;
    public Image imageToolbarSpeciesPortraitBorder;
    public Text textSelectedSpeciesDescription;
    public int selectedSpeciesStatsIndex;
    //public Texture2D textureWorldStats;
    //public Texture2D textureWorldStatsKey;
    //public Vector2[] tolWorldStatsValueRangesKeyArray;
    //public int tolSelectedWorldStatsIndex = 0;
    //public int tolSelectedSpeciesStatsIndex = 0;
    public Sprite spriteAlgaePortrait;
    public Sprite spriteDecomposerPortrait;
    public Sprite spriteZooplanktonPortrait;


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

    /*public bool isActiveFeedToolPanel = false;
    public GameObject panelFeedToolHUD;
    public Animator animatorFeedToolPanel;
    public Button buttonFeedToolSprinkle;
    public Button buttonFeedToolPour;
    public Slider sliderNutrientRegrowthRate;
    private bool foodToolSprinkleOn = true;
    private bool foodToolPourOn = false;

    public bool isActiveMutateToolPanel = false;
    public GameObject panelMutateToolHUD;
    public Animator animatorMutateToolPanel;
    public Slider sliderMutationRate;

    public bool isActiveStirToolPanel = false;
    public GameObject panelStirToolHUD;
    public Animator animatorStirToolPanel;
    */
    /*
        public bool isActiveToolsPanel = false;
        public Button buttonToolOpenClose;
        public Button buttonToolStir;
        public Button buttonToolInspect;
        public Button buttonToolFeed;
        public Button buttonToolMutate;
        public GameObject panelTools;
        public Animator animatorToolsPanel;
         */

    /*public GameObject panelHUD;
    public Image imageFood;
    public Image imageHitPoints;
    public Material foodMat;
    public Material hitPointsMat;
    public Text textScore;

    public GameObject panelDeathScreen;
    public Text textRespawnCounter;
    public Text textCauseOfDeath;
    public Text textPlayerScore;
    */
    public GameObject panelObserverMode;
    //public Text textCurGen;
    //public Text textAvgLifespan;

    public GameObject panelPaused;

    public GameObject panelDebug;
    //public Material fitnessDisplayMat;
    /*public Button buttonPause;
    public Button buttonPlaySlow;
    public Button buttonPlayNormal;
    public Button buttonPlayFast;
    public Button buttonModeA;
    public Button buttonModeB;
    public Button buttonModeC;
    */
    public Text textDebugTrainingInfo1;
    public Text textDebugTrainingInfo2;
    public Text textDebugTrainingInfo3;
    public Text textDebugSimSettings;
    /*public Button buttonToggleRecording;
    public Button buttonToggleTrainingSupervised;
    public Button buttonResetGenomes;
    public Button buttonClearTrainingData;
    public Button buttonToggleTrainingPersistent;
    */


    // TOP PANEL::::
    //public GameObject panelTop;
    //public Button buttonToggleHUD;
    // public bool isActiveHUD = true;
    public Button buttonToggleDebug;
    public bool isActiveDebug = true;

    public bool isObserverMode = false;
    //public bool deathScreenOn = false;
    public bool isPaused = false;

    public GameObject panelMainMenu;
    public GameObject panelLoading;
    public GameObject panelPlaying;

    // EVENTS TOOL SECTION:
    /*public bool pressedRemoveSpecies = false; // gross hack to get around execution order of button click
    public bool isActiveEventSelectionScreen = false;
    public bool isActiveEventsMinor = false;
    public bool isActiveEventsMajor = false;
    public bool isActiveEventsExtreme = false;
    //public int selectedEventCategoryID = 0;  // 0=minor, 1=major, 2=extreme
    public GameObject panelEventMaster;
    public Text textEventPointsWallet;
    public GameObject panelEventSelectionScreen;
    public Text textEventSelectionTitle;
    public Image imageEventSelectionTitle;
    public Text textEventSelectedDescription;
    public Button buttonConfirmSelectedEvent;
    public SimEventComponent simEvent0;
    public SimEventComponent simEvent1;
    public SimEventComponent simEvent2;
    public SimEventComponent simEvent3;
    public SimEventComponent[] simEventComponentsArray;
    public int selectedMinorEventIndex = 0;
    public int selectedMajorEventIndex = 0;
    public int selectedExtremeEventIndex = 0;
    public Button buttonEventsMinor;
    public Image imageButtonEventsMinor;
    public Button buttonEventsMajor;
    public Image imageButtonEventsMajor;
    public Button buttonEventsExtreme;
    public Image imageButtonEventsExtreme;
    public Text textRecentEvent;
    */
    /*
    // TREE OF LIFE SECTION:
    public Button buttonShowHideTOL;
    // VV NEW HERE:::
    public GameObject panelTolMaster;
    public GameObject panelTolWorldStats;
    public GameObject panelTolSpeciesTree;
    public GameObject panelTolEventsTimeline;
    public Button buttonTolWorldStats;
    public Button buttonTolSpeciesTree;
    public Button buttonTolSpeciesTreeExtinct;
    public Button buttonTolEventsTimeline;
    public Button buttonTolBrainDisplay;
    public Button buttonTolInspect;
    public Text textTolSpeciesTitle;
    public Text textTolSpeciesIndex;
    public Text textTolDescription;
    public Image imageTolPortraitRender;
    public Image imageTolWorldStatsRender;
    public Image imageTolSpeciesTreeRender;
    public Image imageTolEventsTimelineRender;
    public Button buttonTolCycleWorldStatsPrev;
    public Button buttonTolCycleWorldStatsNext;
    public Text textTolWorldStatsName;
    public Text textTolWorldStatsValue;
    public Button buttonTolCycleSpeciesStatsPrev;
    public Button buttonTolCycleSpeciesStatsNext;
    public Text textTolSpeciesStatsName;
    public Text textTolSpeciesStatsValue;
    public Button buttonTolEventsMinorToggle;
    public Button buttonTolEventsMajorToggle;
    public Button buttonTolEventsExtremeToggle;
    public Text textTolEventsTimelineName;

    public Image imageTolBackdropGraphs;
    public Image imageTolBackdropDescription;
    public Text textTolYearValue;

    public Image imageTolPortraitBorder;
    public Image imageTolSpeciesIndex;
    public Image imageTolSpeciesNameBackdrop;

    public Image imageTolEventsReadoutBackdrop;
    public Image imageTolStatsReadoutBackdrop;
    public Image imageTolSpeciesReadoutBackdrop;

    public Vector2 tolMouseCoords;
    public float tolMouseOver = 0f;

    public Texture2D tolTextureWorldStats;
    public Texture2D tolTextureWorldStatsKey;
    public Vector2[] tolWorldStatsValueRangesKeyArray;
    public int tolSelectedWorldStatsIndex = 0;
    public int tolSelectedSpeciesStatsIndex = 0;
    
    public bool treeOfLifePanelOn = true;
    public bool tolWorldStatsOn = true;
    public bool tolSpeciesTreeOn = true;
    public bool tolEventsTimelineOn = true;
    public bool tolSpeciesTreeExtinctOn = false;
    public bool tolBrainDisplayOn = false;
    public bool tolInspectOn = false;
    public bool tolSpeciesDescriptionOn = true;

    public float tolGraphCoordsEventsStart = 0.75f;
    public float tolGraphCoordsEventsRange = 0.25f;
    public float tolGraphCoordsStatsStart = 0.5f;
    public float tolGraphCoordsStatsRange = 0.25f;
    public float tolGraphCoordsSpeciesStart = 0f;
    public float tolGraphCoordsSpeciesRange = 0.5f;
    */

    /*// VVV OLD BELOW:::
public GameObject treeOfLifeAnchorGO;     
public GameObject panelTreeOfLifeScaleHideGroup;
public GameObject panelTreeOfLifeInfoBlock;
public GameObject panelTreeOfLifeInfoA;
public Text textTreeOfLifeInfoA;
public GameObject panelTreeOfLifeInfoB;
public Text textTreeOfLifeInfoB;
public GameObject panelTreeOfLifeInfoC;
public Text textTreeOfLifeInfoC;
public Text textTreeOfLifeSpeciesID;
public Button buttonInfoA;
public Button buttonInfoB;
public Button buttonInfoC;

private bool treeOfLifeInfoOnA = false;
private bool treeOfLifeInfoOnB = false;
private bool treeOfLifeInfoOnC = false;
*/

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

    /*public Text statsPanelTextMaxValue;
    public Text textStatsGraphTitle;
    public Text textStatsGraphLegend;
    public Text textStatsCurGen;
    public GraphMode curStatsGraphMode = GraphMode.Lifespan;
    public enum GraphMode {
        Lifespan,
        BodySizes,
        Consumption,
        Predation,
        Nutrients,
        Mutation
    }
    */
    /*public enum WorldStatsMode {
        Global_Oxygen,
        Global_Nutrients,
        Global_Detritus,
        Global_Decomposers,
        Global_Algae_Grid,
        Global_Algae_Particles,
        Global_Zooplankton,
        Global_Live_Agents,
        Global_Dead_Agents,
        Global_Eggsacks,
        Global_Water_Currents_Velocity
    }

    public enum SpeciesStatsMode {
        Average_Lifespan,
        Avg_Nutrients_Consumed,
        Avg_Algae_Consumed,
        Avg_Meat_Consumed,
        Avg_Body_Size,
        Avg_Specialization_Offense,
        Avg_Specialization_Defense,
        Avg_Specialization_Speed,
        Avg_Specialization_Utility,
        Avg_Diet_Specialization_Nutrients,
        Avg_Diet_Specialization_Algae,
        Avg_Diet_Specialization_Meat,
        Avg_Num_Neurons_In_Brain,
        Avg_Num_Axons_In_Brain,
        Avg_Experience_Earned,
        Avg_Fitness_Score,
        Avg_Amount_Damage_Inflicted,
        Avg_Amount_Damage_Taken
    }

    public Button buttonGraphLifespan;
    public Button buttonGraphBodySizes;
    public Button buttonGraphConsumption;
    public Button buttonGraphPredation;
    public Button buttonGraphNutrients;
    public Button buttonGraphMutation;
    */
    public float[] maxValuesStatArray;

    public Vector2 smoothedMouseVel;
    private Vector2 prevMousePos;

    public Vector2 smoothedCtrlCursorVel;
    private Vector2 prevCtrlCursorPos;
    private Vector3 prevCtrlCursorPositionOnWaterPlane;
    public Vector3 curCtrlCursorPositionOnWaterPlane;
    private bool rightTriggerOn = false;

    public bool isDraggingMouse = false;
    public bool isDraggingSpeciesNode = false;

    public int selectedSpeciesID;

    private const int maxDisplaySpecies = 32;

    //private int unlockCooldownTimer = 0;

    /*public Color buttonEventMinorColor = new Color(53f / 255f, 114f / 255f, 97f / 255f);
    public Color buttonEventMajorColor = new Color(53f / 255f, 67f / 255f, 107f / 255f);
    public Color buttonEventExtremeColor = new Color(144f / 255f, 54f / 255f, 82f / 255f);
    */
    // Tree of Life:
    //public Image imageTreeOfLifeDisplay;

    private float curSpeciesStatValue;
    //private float curWorldStatValue;
    //public int curClosestEventToCursor;
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
        ClickToolButtonInspect();  // **** Clean this up! don't mix UI click function with underlying code for initialization
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
                statsSpeciesColorKey.filterMode = FilterMode.Bilinear;
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
        infoGraphNutrientsMat.SetFloat("_MaxValue", 1000f);
        infoGraphDetritusMat.SetTexture("_DataTex", infoDetritusDataTexture);
        infoGraphDetritusMat.SetFloat("_MaxValue", 1000f);
        infoGraphDecomposersMat.SetTexture("_DataTex", infoDecomposersDataTexture);
        infoGraphDecomposersMat.SetFloat("_MaxValue", 1000f);
        infoGraphPlantsMat.SetTexture("_DataTex", infoPlantsDataTexture);
        infoGraphPlantsMat.SetFloat("_MaxValue", 1000f);
        infoGraphAnimalsMat.SetTexture("_DataTex", infoAnimalsDataTexture);
        infoGraphAnimalsMat.SetFloat("_MaxValue", 100f);

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
        //UpdateScoreText(Mathf.RoundToInt(gameManager.simulationManager.agentsArray[0].masterFitnessScore));

        //SetDisplayTextures();

        UpdateDebugUI();
        //UpdateHUDUI();
        UpdateObserverModeUI();  // <== this is the big one        
        UpdatePausedUI();

        UpdateInfoPanelUI();
        UpdateToolbarPanelUI();

        //UpdateStatsPanelUI(); // needed?
        //UpdateInspectPanelUI();  // needed?        

        Vector2 curMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        Vector2 instantMouseVel = curMousePos - prevMousePos;

        smoothedMouseVel = Vector2.Lerp(smoothedMouseVel, instantMouseVel, 0.2f);

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
                StopFollowing();
            }
            if (moveDir.sqrMagnitude > 0.001f) {
                StopFollowing();
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
            bool letGoThisFrame = Input.GetMouseButtonUp(0);
            isDraggingMouse = Input.GetMouseButton(0);

            if (letGoThisFrame) {
                isDraggingMouse = false;
                isDraggingSpeciesNode = false;
            }

            // check for player clicking on an animal in the world
            MouseRaycastCheckAgents(leftClickThisFrame);

            // Get position of mouse on water plane:
            MouseRaycastWaterPlane(Input.mousePosition);
            Vector4[] dataArray = new Vector4[1];
            Vector4 gizmoPos = new Vector4(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f, 0f);
            dataArray[0] = gizmoPos;
            gameManager.theRenderKing.gizmoCursorPosCBuffer.SetData(dataArray);

            bool mouseCursorVisible = false;

            if (isAnnouncementTextOn) {
                panelPendingClickPrompt.SetActive(true);
                timerAnnouncementTextCounter++;

                if (timerAnnouncementTextCounter > 640) {
                    isAnnouncementTextOn = false;
                    timerAnnouncementTextCounter = 0;
                }
            }
            else {
                panelPendingClickPrompt.SetActive(false);
            }
            //isUnlockCooldown = false;
    //private int unlockCooldownCounter = 0;
            if(isUnlockCooldown) {
                unlockCooldownCounter++;
                if(unlockCooldownCounter > 4200) {
                    isUnlockCooldown = false;
                    unlockCooldownCounter = 0;
                }
            }
            //

            //if (gameManager.simulationManager.trophicLayersManager.pendingTrophicSlot) {
            //    panelPendingClickPrompt.SetActive(true);
            //}

            // This Update() loop all happens before the Event System button click calls,
            // ... so if you disable a button now it will prevent clicking by hiding button UI

            //if(pressedRemoveSpecies) { // flag that the remove button was pressed
                //if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlot) {
                    //Debug.Log("pressedRemoveSpecies!");
                    //pressedRemoveSpecies = false;
                //}
                //CancelSpeciesSlot();
                //gameManager.simulationManager.trophicLayersManager.pendingTrophicSlot = false;
            //}

            if (EventSystem.current.IsPointerOverGameObject()) {  // if mouse is over ANY unity canvas UI object (with raycast enabled)
                //Debug.Log("MouseOverUI!!!");
            }
            else {
                /*if (gameManager.simulationManager.trophicLayersManager.pendingTrophicSlot) {
                    //panelPendingClickPrompt.SetActive(true);
                    mouseCursorVisible = true;

                    if (leftClickThisFrame) {
                        bool createSpecies = false;
                        if(gameManager.simulationManager.trophicLayersManager.GetAgentsOnOff()) {
                            createSpecies = true;
                        }
                        // **** CREATE SPECIES HERE:::::: ****
                        gameManager.simulationManager.trophicLayersManager.CreateTrophicSlotSpecies(gameManager.simulationManager, gizmoPos, gameManager.simulationManager.simAgeTimeSteps);

                        gameManager.theRenderKing.baronVonWater.StartCursorClick(gizmoPos);

                        //toolbarInfluencePoints -= addSpeciesInfluenceCost;
                        
                        if(gameManager.simulationManager.trophicLayersManager.pendingTrophicSlotRef.kingdomID == 2) {
                            if(gameManager.simulationManager.trophicLayersManager.pendingTrophicSlotRef.tierID == 1) {
                                if(createSpecies) {
                                    gameManager.simulationManager.CreateAgentSpecies(new Vector3(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f));
                                    gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[gameManager.simulationManager.trophicLayersManager.pendingTrophicSlotRef.slotID].linkedSpeciesID = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1].speciesID;
                                }
                                
                            }
                        }
                    }
                    else {                    
                    }
                }
                */
                
            
                if (curActiveTool == ToolType.Inspect || curActiveTool == ToolType.None) {                    
                    //gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
                }
                else {
                    mouseCursorVisible = true;
                    //gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
                }
            
                if (curActiveTool == ToolType.Stir) {                    
                
                    float isActing = 0f;
                    if (isDraggingMouse) {
                        isActing = 1f;
                        //toolbarInfluencePoints -= 0.00275f;
                        //toolbarInfluencePoints = Mathf.Clamp01(toolbarInfluencePoints);

                        float mag = smoothedMouseVel.magnitude;
                        float radiusMult = 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                        if(mag > 0f) {
                            gameManager.simulationManager.PlayerToolStirOn(curMousePositionOnWaterPlane, smoothedMouseVel * (0.75f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.25f), radiusMult);
                        }
                        else {
                            gameManager.simulationManager.PlayerToolStirOff();
                        }
                    }
                    else {
                        gameManager.simulationManager.PlayerToolStirOff();
                    }
                    
                    gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsStirring", isActing);
                    gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_Radius", 2f);  // **** Make radius variable! (possibly texture based?)
                }

                gameManager.theRenderKing.nutrientToolOn = false;
                if(curActiveTool == ToolType.Nutrients) {
                    if(isDraggingMouse) {
                        //toolbarInfluencePoints = Mathf.Clamp01(toolbarInfluencePoints - 0.0025f);
                        gameManager.simulationManager.simResourceManager.curGlobalNutrients += 0.25f;
                        gameManager.simulationManager.simResourceManager.curGlobalDetritus += 0.15f;
                        gameManager.simulationManager.vegetationManager.AddResourcesAtCoords(new Vector4(0.1f, 0f, 0f, 0f), curMousePositionOnWaterPlane.x / SimulationManager._MapSize, curMousePositionOnWaterPlane.y / SimulationManager._MapSize);
                        
                        gameManager.theRenderKing.nutrientToolOn = true;
                    }                
                }
                if(curActiveTool == ToolType.Remove) {
                    if(isDraggingMouse) {
                        //toolbarInfluencePoints = Mathf.Clamp01(toolbarInfluencePoints - 0.0025f);
                        gameManager.simulationManager.simResourceManager.curGlobalNutrients -= 0.5f;
                        gameManager.simulationManager.simResourceManager.curGlobalDetritus -= 0.5f;
                    }                
                }
            }

            /*if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlot) {
                    
                panelPendingClickPrompt.SetActive(true); // customized message -- selected vs. pending?
                //mouseCursorVisible = true;
                // display remove button
                if (gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
                    buttonToolbarRemoveDecomposer.gameObject.SetActive(true);
                }
                else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
                    buttonToolbarRemovePlant.gameObject.SetActive(true);
                }
                else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {
                    buttonToolbarRemoveAnimal.gameObject.SetActive(true);
                }
                
                
                

                if (leftClickThisFrame) {
                    if(!pressedRemoveSpecies) {
                        gameManager.simulationManager.trophicLayersManager.ClickedSelectedTrophicSlot();   
                        
                        //toolbarInfluencePoints -= addSpeciesInfluenceCost;
                        treeOfLifeManager.ClickedOnSpeciesNode(1);  // selected speciesID
                        UpdateTolSpeciesColorUI(); // updates creature portrait render to selectedID
                        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) {                            

                            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {
                                //gameManager.simulationManager.CreateAgentSpecies(new Vector3(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f));
                            }
                        }
                    }
                    else {
                        //RemoveAgentSpecies();
                    }                             
                    
                }
                else {

                }
            }
            else {
                
            }
            */
            if(mouseCursorVisible) {
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
            }         
            else {
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
            }

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

    public void CheckForAnnouncements() {
        //announceAlgaeCollapsePossible = false;
    //private bool announceAlgaeCollapseOccurred = false;
    //private bool announceAgentCollapsePossible = false; 
    //private bool announceAgentCollapseOccurred = false;

        // 
        if(!announceAlgaeCollapseOccurred) {
            if(announceAlgaeCollapsePossible) {
                if(gameManager.simulationManager.simResourceManager.curGlobalAlgaeParticles < 10f) {
                    announceAlgaeCollapsePossible = false;
                    announceAlgaeCollapseOccurred = true;

                    panelPendingClickPrompt.GetComponentInChildren<Text>().text = "<color=#DDDDDDFF>Algae Died from lack of Nutrients!</color>\nAdd Decomposers to recycle waste";
                    panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorDecomposersLight;
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
        }

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
        
    public void UpdateToolbarPanelUI() {

        // Check for Announcements:
        CheckForAnnouncements();
        
        buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarStir.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarNutrients.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarMutate.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarRemove.GetComponent<Image>().color = buttonDisabledColor;

        switch(curActiveTool) {
            case ToolType.None:
                //buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;

                break;
            case ToolType.Inspect:
                buttonToolbarInspect.GetComponent<Image>().color = buttonActiveColor;
                break;
            case ToolType.Mutate:
                buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor;
                break;
            case ToolType.Nutrients:
                buttonToolbarNutrients.GetComponent<Image>().color = buttonActiveColor;
                break;
            case ToolType.Remove:
                buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;
                break;
            case ToolType.Stir:
                buttonToolbarStir.GetComponent<Image>().color = buttonActiveColor;
                break;
            default:
                break;

        }
                

        // Influence points meter:     
        //toolbarInfluencePoints += 0.0005225f; // x10 while debugging
        //toolbarInfluencePoints = Mathf.Clamp01(toolbarInfluencePoints);
        //infoMeterInfluencePointsMat.SetFloat("_FillPercentage", toolbarInfluencePoints);
        //textInfluencePointsValue.text = "Influence: \n" + (toolbarInfluencePoints * 100f).ToString("F0") + "%";



        panelToolbarExpand.SetActive(isToolbarExpandOn);
        panelToolbarWing.SetActive(isToolbarWingOn);
        buttonToolbarExpandOn.gameObject.SetActive(!isToolbarExpandOn);

        if(isToolbarExpandOn) {
            
            // Species Slots visuals:  ================================================================================
            TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;  
        
            if(layerManager.GetAlgaeOnOff()) {
                //buttonToolbarStir.interactable = true;
            }
            else {
                //buttonToolbarStir.interactable = false;
            }

            if(layerManager.GetDecomposersOnOff()) {
                //buttonToolbarNutrients.interactable = true;
                //buttonToolbarRemove.interactable = true;
            }
            else {
                //buttonToolbarNutrients.interactable = false;
                //buttonToolbarRemove.interactable = false;
            }

            // KINGDOM DECOMPOSERS:
            bool isSelected = false;
            
            if(layerManager.isSelectedTrophicSlot) {
                if(layerManager.selectedTrophicSlotRef.kingdomID == 0 && layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelected = true; 
                }
            }
            SetToolbarButtonStateUI(ref buttonToolbarDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelected);
            
            // KINGDOM PLANTS:
            isSelected = false;            
            if(layerManager.isSelectedTrophicSlot) {
                if(layerManager.selectedTrophicSlotRef.kingdomID == 1 && layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelected = true; 
                }
            }
            SetToolbarButtonStateUI(ref buttonToolbarAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelected);
            
            SetToolbarButtonStateUI(ref buttonToolbarPlant1, TrophicSlot.SlotStatus.Locked, false);
            SetToolbarButtonStateUI(ref buttonToolbarPlant2, TrophicSlot.SlotStatus.Locked, false);
            // KINGDOM ANIMALS:
            //Zooplankton:
            isSelected = false;            
            if(layerManager.isSelectedTrophicSlot) {
                if(layerManager.selectedTrophicSlotRef.kingdomID == 2 && layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelected = true; 
                }
            }
            SetToolbarButtonStateUI(ref buttonToolbarZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelected);
            
            // AGENTS:
            // *************************************************************************************
            bool isSelected0 = false;
            bool isSelected1 = false;
            bool isSelected2 = false;
            bool isSelected3 = false;
            if(layerManager.isSelectedTrophicSlot) {
                if (layerManager.selectedTrophicSlotRef.kingdomID == 2 && layerManager.selectedTrophicSlotRef.tierID == 1) {
                    if (layerManager.selectedTrophicSlotRef.slotID == 0) {
                        isSelected0 = true;
                    }
                    if (layerManager.selectedTrophicSlotRef.slotID == 1) {
                        isSelected1 = true;
                    }
                    if (layerManager.selectedTrophicSlotRef.slotID == 2) {
                        isSelected2 = true;
                    }
                    if (layerManager.selectedTrophicSlotRef.slotID == 3) {
                        isSelected3 = true;
                    }
                }
            }            

            SetToolbarButtonStateUI(ref buttonToolbarAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelected0);
            SetToolbarButtonStateUI(ref buttonToolbarAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelected1);
            SetToolbarButtonStateUI(ref buttonToolbarAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelected2);
            SetToolbarButtonStateUI(ref buttonToolbarAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelected3);
            

            if (isToolbarWingOn) {  // a species is selected
                //
                //panelToolbarWingStats.SetActive(false);
                //panelToolbarWingMutation.SetActive(false);
                panelToolbarWingDeletePrompt.SetActive(false);
                textToolbarWingSpeciesSummary.gameObject.SetActive(true);


                Color speciesColorLight = Color.white;
                Color speciesColorDark = Color.black;
                if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
                    speciesColorLight = colorDecomposersLight;
                    speciesColorDark = colorDecomposersDark;
                }
                else if (layerManager.selectedTrophicSlotRef.kingdomID == 1) {
                    speciesColorLight = colorPlantsLight;
                    speciesColorDark = colorPlantsDark;
                }
                else {
                    speciesColorLight = colorAnimalsLight;
                    speciesColorDark = colorAnimalsDark;
                }
                buttonToolbarWingCreateSpecies.GetComponent<Image>().color = speciesColorLight;                
                imageToolbarSpeciesPortraitRender.color = speciesColorLight;
                textSelectedSpeciesTitle.color = speciesColorLight;
                imageToolbarSpeciesPortraitBorder.color = speciesColorDark;
                imageToolbarButtonBarBackground.color = speciesColorDark;

                textSelectedSpeciesTitle.text = layerManager.selectedTrophicSlotRef.speciesName;
                
                // Which panels are available?
                int panelTier = -1;
                
                buttonToolbarWingDeleteSpecies.gameObject.SetActive(false); 
                
                if(layerManager.selectedTrophicSlotRef.status == TrophicSlot.SlotStatus.Empty) {  // waiting to be created!
                    panelTier = 0;

                    buttonToolbarWingCreateSpecies.gameObject.SetActive(true);
                    imageToolbarSpeciesPortraitRender.gameObject.SetActive(false);
                                        
                }
                else {
                    buttonToolbarWingCreateSpecies.gameObject.SetActive(false);
                    imageToolbarSpeciesPortraitRender.gameObject.SetActive(true);
                    
                    panelTier = 1;
                    

                    if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
                        imageToolbarSpeciesPortraitRender.sprite = spriteDecomposerPortrait;
                        
                    }
                    else if (layerManager.selectedTrophicSlotRef.kingdomID == 1) {
                        imageToolbarSpeciesPortraitRender.sprite = spriteAlgaePortrait;
                        
                    }
                    else {

                        if (layerManager.selectedTrophicSlotRef.tierID == 1) {
                            panelTier = 2;

                            buttonToolbarWingDeleteSpecies.gameObject.SetActive(true);
                                                        
                            imageToolbarSpeciesPortraitRender.sprite = null;
                            imageToolbarSpeciesPortraitRender.color = Color.white;
                                                        
                        }
                        else {   // ZOOPLANKTON                            
                            imageToolbarSpeciesPortraitRender.sprite = spriteZooplanktonPortrait;
                            
                        }
                    }                    
                } 
                
                if(isToolbarDeletePromptOn) {
                    panelToolbarWingDeletePrompt.SetActive(true);
                    textToolbarWingSpeciesSummary.gameObject.SetActive(false);
                }

                if(panelTier < curToolbarWingPanelSelectID) {  // if
                    curToolbarWingPanelSelectID = 0;
                }

                if(curToolbarWingPanelSelectID == 0) {
                    buttonToolbarWingDescription.transform.localScale = Vector3.one * 1.5f;
                    if(!isToolbarDeletePromptOn) {
                        textToolbarWingSpeciesSummary.gameObject.SetActive(true);
                        textToolbarWingSpeciesSummary.text = layerManager.GetSpeciesPreviewDescriptionString();
                    }                    
                }
                else {
                    buttonToolbarWingDescription.transform.localScale = Vector3.one;
                    textToolbarWingSpeciesSummary.gameObject.SetActive(false);
                }
                if(curToolbarWingPanelSelectID == 1) {
                    buttonToolbarWingStats.transform.localScale = Vector3.one * 1.5f;

                    if (!isToolbarDeletePromptOn) {
                        panelToolbarWingStats.SetActive(true);
                        UpdateToolbarWingStatsPanel();
                    }
                }
                else {
                    buttonToolbarWingStats.transform.localScale = Vector3.one;
                    panelToolbarWingStats.SetActive(false);
                }
                if(curToolbarWingPanelSelectID == 2) {
                    buttonToolbarWingMutation.transform.localScale = Vector3.one * 1.5f;

                    if (!isToolbarDeletePromptOn) {
                        panelToolbarWingMutation.SetActive(true); 
                        // Update mutation/upgrade panel
                    }                
                }
                else {
                    buttonToolbarWingMutation.transform.localScale = Vector3.one;
                    panelToolbarWingMutation.SetActive(false);
                }
                 
                

                // Panel Buttons!!!
                buttonToolbarWingDescription.interactable = false;
                buttonToolbarWingDeleteSpecies.interactable = false;
                buttonToolbarWingStats.interactable = false;   
                buttonToolbarWingMutation.interactable = false;

                if(panelTier >= 0) {
                    buttonToolbarWingDescription.interactable = true;
                }
                if(panelTier >= 1) {
                    buttonToolbarWingStats.interactable = true;   
                }
                if(panelTier >= 2) {
                    buttonToolbarWingMutation.interactable = true;
                    buttonToolbarWingDeleteSpecies.interactable = true;
                }
            }
        }
        else {
            
        }
        
    }
    public void UpdateInfoPanelUI() {
        textCurYear.text = gameManager.simulationManager.curSimYear.ToString();

        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalAlgaeParticles + resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles).ToString("F0");

        float percentageOxygen = resourcesRef.curGlobalOxygen / 1000f;
        infoMeterOxygenMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageOxygen));
        float percentageNutrients = resourcesRef.curGlobalNutrients / 1000f;
        infoMeterNutrientsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageNutrients));
        float percentageDetritus = resourcesRef.curGlobalDetritus / 1000f;
        infoMeterDetritusMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDetritus));
        float percentageDecomposers = resourcesRef.curGlobalDecomposers / 1000f;
        infoMeterDecomposersMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDecomposers));
        float percentagePlants = (resourcesRef.curGlobalAlgaeParticles + resourcesRef.curGlobalAlgaeReservoir) / 1000f;
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
                /*TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;  

                panelInfoResourcesOverview.SetActive(false);
                //panelInfoSpeciesOverview.SetActive(true);

                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status);

                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status);
                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status);
                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesPlant2, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[1].status);

                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status);
                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status);
                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status);
                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status);
                SetInfoSpeciesButtonStateUI(ref buttonInfoSpeciesAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status);

                if(infoSpeciesSelectedKingdom == 0) {
                    textInfoSpeciesName.text = "Decomposers";
                    textInfoSpeciesStats.text = "stats stats stats!";
                }
                else if(infoSpeciesSelectedKingdom == 1) {
                    textInfoSpeciesName.text = "Algae";
                    textInfoSpeciesStats.text = "stats stats stats!";
                }
                else {  // Animals
                    if(infoSpeciesSelectedTier == 0) {
                        //zooplankton
                        textInfoSpeciesName.text = "Zooplankton";
                        textInfoSpeciesStats.text = "stats stats stats!";
                    }
                    else {
                        // Agents:
                        textInfoSpeciesName.text = "Creature " + infoSpeciesSelectedSlot.ToString() + ", " + gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[infoSpeciesSelectedSlot].linkedSpeciesID.ToString();
                        textInfoSpeciesStats.text = "stats stats stats!";

                    }
                }
                //textInfoSpeciesName.text = "SpeciesName";
                //textInfoSpeciesStats.text = "stats stats stats!";
                */
            }
        }
        else {
            panelInfoExpanded.SetActive(false);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = "<";
        }
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
                maxCount = agentRef._DecayDurationTimeSteps;
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

            debugTxtAgent += "\nWater Depth: " + agentRef.depth.ToString("F3") + ", Vel: " + (agentRef.avgFluidVel * 10f).ToString("F3") + "\n";
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
        debugTxtResources += "\n     + " + simManager.simResourceManager.oxygenProducedByAlgaeParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString() + " ( zooplankton )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString() + " ( agents )";
        debugTxtResources += "\nNutrients: " + simManager.simResourceManager.curGlobalNutrients.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.nutrientsUsedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.nutrientsUsedByAlgaeParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\nDetritus: " + simManager.simResourceManager.curGlobalDetritus.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAlgaeParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString() + " ( zooplankton )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString() + " ( agents )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\nDecomposers: " + simManager.simResourceManager.curGlobalDecomposers.ToString();
        debugTxtResources += "\nAlgae (Reservoir): " + simManager.simResourceManager.curGlobalAlgaeReservoir.ToString();
        debugTxtResources += "\nAlgae (Particles): " + simManager.simResourceManager.curGlobalAlgaeParticles.ToString();
        debugTxtResources += "\nZooplankton: " + simManager.simResourceManager.curGlobalAnimalParticles.ToString();
        debugTxtResources += "\nLive Agents: " + simManager.simResourceManager.curGlobalAgentBiomass.ToString();
        debugTxtResources += "\nDead Agents: " + simManager.simResourceManager.curGlobalCarrionVolume.ToString();
        debugTxtResources += "\nEggSacks: " + simManager.simResourceManager.curGlobalEggSackVolume.ToString();

        textDebugTrainingInfo2.text = debugTxtResources;
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
        int critterIndex = cameraManager.targetCritterIndex;

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
        float percentage = gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount;
        inspectWidgetStomachFoodMat.SetFloat("_FillPercentage", percentage);
        textStomachContents.text = "Food\n" + gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount.ToString("F3"); // + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount * 100f).ToString("F1") + "%\nCapacity: " + gameManager.simulationManager.agentsArray[critterIndex].coreModule.stomachCapacity.ToString("F3");

        // ENERGY:
        Color energyHue = new Color(0.3f, 0.5f, 1f);
        inspectWidgetEnergyMat.SetPass(0);
        inspectWidgetEnergyMat.SetColor("_Tint", energyHue);        
        percentage = gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy *
                    gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].embryoPercentage;
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
            percentage = 0.05f;
        } 
        inspectWidgetEnergyMat.SetFloat("_FillPercentage", percentage);
        textEnergy.text = "Energy\n" + gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy.ToString("F3"); // + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy * 100f).ToString("F1") + "%";

        // Life Cycle
        inspectWidgetLifeCycleMat.SetPass(0);
        inspectWidgetLifeCycleMat.SetColor("_Tint", Color.blue);
        
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Egg) {
            percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 
                        (float)gameManager.simulationManager.agentsArray[critterIndex]._GestationDurationTimeSteps;
        }
        /*if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Young) {
            percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 
                        (float)gameManager.simulationManager.agentsArray[critterIndex]._YoungDurationTimeSteps;
        }*/
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Mature) {
            percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].ageCounter / 
                        (float)gameManager.simulationManager.agentsArray[critterIndex].maxAgeTimeSteps;
        }
        if(gameManager.simulationManager.agentsArray[critterIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
            percentage = (float)gameManager.simulationManager.agentsArray[critterIndex].lifeStageTransitionTimeStepCounter / 
                        (float)gameManager.simulationManager.agentsArray[critterIndex]._DecayDurationTimeSteps;
        }        
        inspectWidgetLifeCycleMat.SetFloat("_FillPercentage", percentage);
        inspectWidgetLifeCycleMat.SetInt("_CurLifeStage", (int)gameManager.simulationManager.agentsArray[critterIndex].curLifeStage);
        textLifeCycle.text = "Life Stage:\n" + gameManager.simulationManager.agentsArray[critterIndex].curLifeStage.ToString() + "\n" + gameManager.simulationManager.agentsArray[critterIndex].ageCounter.ToString();


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
        /*if(cameraManager.targetAgent.speciesIndex == 1) {
            speciesHue = new Color(0.33f, 1f, 0.33f);
            textSpeciesID.text = "B";
        }
        else if(cameraManager.targetAgent.speciesIndex == 2) {
            speciesHue = new Color(0.33f, 0.33f, 1f);
            textSpeciesID.text = "C";
        }
        else if(cameraManager.targetAgent.speciesIndex == 3) {
            speciesHue = new Color(1f, 1f, 1f);
            textSpeciesID.text = "D";
        }*/
        inspectWidgetSpeciesIconMat.SetPass(0);
        inspectWidgetSpeciesIconMat.SetColor("_Tint", speciesHue);

        // AGENT ICON
        inspectWidgetAgentIconMat.SetPass(0);
        inspectWidgetAgentIconMat.SetColor("_Tint", Color.gray);
        textAgentID.text = cameraManager.targetAgent.index.ToString();
        

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
        
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;

        string descriptionText = "";
        
        if(slot.kingdomID == 0) {
            descriptionText += "Total Biomass: <b>" + gameManager.simulationManager.simResourceManager.curGlobalDecomposers.ToString("F1") + "</b>\n\n";

            descriptionText += "<color=#FBC653FF>Nutrient Production: <b>" + gameManager.simulationManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Processed: <b>" + gameManager.simulationManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
        }
        else if(slot.kingdomID == 1) {
            descriptionText += "Total Biomass: <b>" + gameManager.simulationManager.simResourceManager.curGlobalAlgaeParticles.ToString("F1") + "</b>\n\n";

            descriptionText += "<color=#8EDEEEFF>Oxygen Production: <b>" + gameManager.simulationManager.simResourceManager.oxygenProducedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
        }
        else {
            if(slot.tierID == 0) {  // ZOOPLANKTON
                descriptionText += "Total Biomass: <b>" + gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles.ToString("F1") + "</b>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";
                //descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";
            }
            else {  // AGENTS
                SpeciesGenomePool selectedPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
                
                descriptionText += "Total Biomass: <b>" + gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString("F3") + "</b></color>\n\n";

                descriptionText += "Avg Algae Consumed: <b>" + selectedPool.avgConsumptionPlant.ToString("F3") + "</b>\n";
                descriptionText += "Avg Meat Consumed: <b>" + selectedPool.avgConsumptionMeat.ToString("F3") + "</b>\n\n";  
                //descriptionText += "Descended From Species: <b>" + selectedPool.parentSpeciesID.ToString() + "</b>\n";
                //descriptionText += "Year Evolved: <b>" + selectedPool.yearCreated.ToString() + "</b>\n\n";
                descriptionText += "Avg Lifespan: <b>" + selectedPool.avgLifespan.ToString("F0") + "</b>\n";

                descriptionText += "Avg Body Size: <b>" + selectedPool.avgBodySize.ToString("F2") + "</b>\n";
                descriptionText += "Avg Brain Size: <b>" + ((selectedPool.avgNumNeurons + selectedPool.avgNumAxons) * 0.1f).ToString("F1") + "</b>\n";
                //descriptionText += "Avg Axon Count: <b>" + selectedPool.avgNumAxons.ToString("F0") + "</b>\n\n";
        
                 
            }
        }             
        
        textSelectedSpeciesDescription.text = descriptionText;
    }

    private void UpdateSelectedSpeciesColorUI() { // ** should be called AFTER new species actually created?? something is fucked here
        Debug.Log("UpdateTolSpeciesColorUI " + selectedSpeciesID.ToString());
        
        //Vector3 speciesHuePrimary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        //Vector3 speciesHueSecondary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID].representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
        //imageToolbarSpeciesPortraitRender.color = new Color(speciesHueSecondary.x, speciesHueSecondary.y, speciesHueSecondary.z);
        //imageTolSpeciesIndex.color = new Color(speciesHueSecondary.x, speciesHueSecondary.y, speciesHueSecondary.z);
        //imageTolSpeciesNameBackdrop.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
        //imageTolSpeciesReadoutBackdrop.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);

        gameManager.simulationManager.theRenderKing.UpdateCritterPortraitStrokesData(gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID].representativeGenome);
    }

    private void SetToolbarButtonStateUI(ref Button button, TrophicSlot.SlotStatus slotStatus, bool isSelected) {

        button.gameObject.SetActive(true);
        Vector3 scale = Vector3.one;
        switch(slotStatus) {
            case TrophicSlot.SlotStatus.Off:
                button.gameObject.SetActive(false); 
                //button.gameObject.transform.localScale = Vector3.one;
                break;
            case TrophicSlot.SlotStatus.Locked:
                button.interactable = false;                
                button.GetComponentInChildren<Text>().text = "";
                //button.gameObject.transform.localScale = Vector3.one;
                break;
            case TrophicSlot.SlotStatus.Empty:
                button.interactable = true;                
                button.GetComponentInChildren<Text>().text = "";
                button.GetComponent<Image>().sprite = spriteSpeciesSlotRing;
                scale = Vector3.one * 0.75f;
                break;
            //case TrophicSlot.SlotStatus.Pending:
            //    button.interactable = false;
            //    button.GetComponentInChildren<Text>().text = "%";
            //    break;
            case TrophicSlot.SlotStatus.On:
                button.interactable = true;
                button.GetComponentInChildren<Text>().text = "";
                button.GetComponent<Image>().sprite = spriteSpeciesSlotFull;
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
        }

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
                gameManager.simulationManager.RemoveSelectedAgentSpecies(slot.slotID);
            }
        }

        slot.status = TrophicSlot.SlotStatus.Empty;
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = false;

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

    public void StopFollowing() {
        cameraManager.isFollowing = false;
        
        if(isActiveInspectPanel) {
            isActiveInspectPanel = false;
            //buttonToolInspect.GetComponent<Image>().color = buttonDisabledColor;        
            animatorInspectPanel.enabled = true;
            animatorInspectPanel.Play("SlideOffPanelInspect");
        } 
    }
    public void StartFollowing() {
        cameraManager.isFollowing = true;
        isActiveInspectPanel = true;    
        animatorInspectPanel.enabled = true;
        animatorInspectPanel.Play("SlideOnPanelInspect");        
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
        //int layerMask = 0;
        Physics.Raycast(ray, out hit);  // *** USE DEDICATED LAYER FOR THIS CHECK!!!! *********

        cameraManager.isMouseHoverAgent = false;
        cameraManager.mouseHoverAgentIndex = 0;
        cameraManager.mouseHoverAgentRef = null;

        if(hit.collider != null) {
            
            // CHECK FOR AGENT COLLISION:
            Agent agentRef = hit.collider.gameObject.GetComponentInParent<Agent>();
            if(agentRef != null) {
                //Debug.Log("AGENT: [ " + agentRef.gameObject.name + " ] #" + agentRef.index.ToString());
                    
                if(clicked) {
                    cameraManager.SetTarget(agentRef, agentRef.index);
                    cameraManager.isFollowing = true;

                    //ClickOnSpeciesNode(agentRef.speciesIndex);
                    
                    StartFollowing();
                }
                else {

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
        //animatorStirToolPanel.enabled = true;
        //animatorStirToolPanel.Play("SlideOnPanelStirTool"); 
        buttonToolbarStir.GetComponent<Image>().color = buttonActiveColor; 

        TurnOffInspectTool();  
        TurnOffNutrientsTool();
        TurnOffMutateTool();
        TurnOffRemoveTool();
              
    }
    public void ClickToolButtonInspect() {
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Inspect) {
            curActiveTool = ToolType.Inspect;

            TurnOnInspectTool();
            TurnOffStirTool();  
            TurnOffNutrientsTool();
            TurnOffMutateTool();
            TurnOffRemoveTool();
        //} 
        /*else {
            curActiveTool = ToolType.None;
            TurnOffInspectTool();
        } */
    }
    public void ClickToolButtonNutrients() {
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Nutrients) {
            curActiveTool = ToolType.Nutrients;

            //isActiveFeedToolPanel = true;    
            //animatorFeedToolPanel.enabled = true;
            //animatorFeedToolPanel.Play("SlideOnPanelFeedTool"); 
            buttonToolbarNutrients.GetComponent<Image>().color = buttonActiveColor;

            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffMutateTool();
            TurnOffRemoveTool();
        //}  
        /*else {
            curActiveTool = ToolType.None;
            TurnOffNutrientsTool();
        } */
    }
    public void ClickToolButtonRemove() {
        //gameManager.simulationManager.trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Remove) {
            curActiveTool = ToolType.Remove;

            //isActiveFeedToolPanel = true;    
            //animatorFeedToolPanel.enabled = true;
            //animatorFeedToolPanel.Play("SlideOnPanelFeedTool"); 
            buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;

            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffMutateTool();
            TurnOffNutrientsTool();
        //}  
        /*else {
            curActiveTool = ToolType.None;
            TurnOffRemoveTool();
        } */
    }

    private void TurnOnInspectTool() {
        buttonToolbarInspect.GetComponent<Image>().color = buttonActiveColor;            
    }    
    private void TurnOffInspectTool() {
                
        buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;  
               
        isActiveInspectPanel = false;
        //StopFollowing();
    }
    private void TurnOffNutrientsTool() {
        buttonToolbarNutrients.GetComponent<Image>().color = buttonDisabledColor;

        //if(isActiveFeedToolPanel) {
        //    animatorFeedToolPanel.enabled = true;
        //    animatorFeedToolPanel.Play("SlideOffPanelFeedTool");
        //}        
        //isActiveFeedToolPanel = false;
    }
    private void TurnOffMutateTool() {
        buttonToolbarMutate.GetComponent<Image>().color = buttonDisabledColor;

        //if(isActiveMutateToolPanel) {
        //    animatorMutateToolPanel.enabled = true;
        //    animatorMutateToolPanel.Play("SlideOffPanelMutateTool");
        //}        
        //isActiveMutateToolPanel = false;
    }
    private void TurnOffStirTool() {
        buttonToolbarStir.GetComponent<Image>().color = buttonDisabledColor;
        //if(isActiveStirToolPanel) {
        //    animatorStirToolPanel.enabled = true;
        //    animatorStirToolPanel.Play("SlideOffPanelStirTool");
        //}        
        isActiveStirToolPanel = false;
    }
    private void TurnOffRemoveTool() {
        buttonToolbarRemove.GetComponent<Image>().color = buttonDisabledColor;
        //if(isActiveStirToolPanel) {
        //    animatorStirToolPanel.enabled = true;
        //    animatorStirToolPanel.Play("SlideOffPanelStirTool");
        //}        
        //isActiveStirToolPanel = false;
    }
    public void ClickButtonToolbarDecomposers() {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarWingOn = true;

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
        curActiveTool = ToolType.None;
    }
    public void ClickButtonToolbarAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];

        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarWingOn = true;
        
        if(gameManager.simulationManager.trophicLayersManager.GetAlgaeOnOff()) {
            // if already on:
            // Selected!!!
            /*if (slot.status == TrophicSlot.SlotStatus.On) {            
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlot = true;
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot; 
                buttonSelectedTrophicSlot = buttonToolbarAlgae;
            }*/
        }
        else {
            
            //gameManager.simulationManager.trophicLayersManager.PendingAlgae();
            //buttonPendingTrophicSlot = buttonToolbarAlgae;
            
        }
        curActiveTool = ToolType.None;
    }
    public void ClickButtonToolbarZooplankton() {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
        
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarWingOn = true;
        
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
        curActiveTool = ToolType.None;
    }

    public void ClickButtonToolbarAgent(int index) {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        isToolbarWingOn = true;

        selectedSpeciesID = slot.linkedSpeciesID;

        curActiveTool = ToolType.None;

        UpdateSelectedSpeciesColorUI(); // ***
        // Why do I have to click this twice before portrait shows up properly??????
    }

    public void ClickToolbarRemoveSpeciesSlot() {
        //pressedRemoveSpecies = true;  // Sets remove flag for update loop:       
    }
    public void ClickToolbarExpandOn() {
        isToolbarExpandOn = true;
    }
    public void ClickToolbarExpandOff() {
        isToolbarExpandOn = false;
    }

    public void ClickToolbarWingClose() {
        isToolbarWingOn = false;
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = false;
        
    }
    public void AnnounceUnlockDecomposers() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Decomposer Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorDecomposersLight;
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockZooplankton() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Zooplankton Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorAnimalsLight;
        isAnnouncementTextOn = true;
    }
    public void AnnounceUnlockVertebrates() {
        panelPendingClickPrompt.GetComponentInChildren<Text>().text = "Vertebrate Species Unlocked!";
        panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorAnimalsLight;
        isAnnouncementTextOn = true;
    }
    public void ClickToolbarCreateNewSpecies() {
        gameManager.simulationManager.trophicLayersManager.CreateTrophicSlotSpecies(gameManager.simulationManager, cameraManager.curCameraFocusPivotPos, gameManager.simulationManager.simAgeTimeSteps);
                
        gameManager.theRenderKing.baronVonWater.StartCursorClick(cameraManager.curCameraFocusPivotPos);
        
        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
            panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Decomposer added!";
            panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorDecomposersLight;
        }
        else if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
            panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Algae added!";
            panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorPlantsLight;
        }
        else {
            if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {
                //if(createSpecies) {
                gameManager.simulationManager.CreateAgentSpecies(new Vector3(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f));
                gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID].linkedSpeciesID = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1].speciesID;
                //}
                
                panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Vertebrate added!";
                panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorAnimalsLight;
            }
            else {
                panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Zooplankton added!";
                panelPendingClickPrompt.GetComponentInChildren<Text>().color = colorAnimalsLight;
            }
        }

        curToolbarWingPanelSelectID = 1;

        isAnnouncementTextOn = true;

        //UpdateSelectedSpeciesColorUI();
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
        
        int newIndex = cameraManager.targetCritterIndex;
        for(int i = 1; i < gameManager.simulationManager._NumAgents; i++) {
            int index = (gameManager.simulationManager._NumAgents + cameraManager.targetCritterIndex - i) % gameManager.simulationManager._NumAgents;

            if (gameManager.simulationManager.agentsArray[index].speciesIndex == selectedSpeciesID) {
                newIndex = index;
                break;
            }
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);          
                           
    }
    public void ClickNextAgent() {
        Debug.Log("ClickNextAgent");
        
        int newIndex = cameraManager.targetCritterIndex;
        for(int i = 1; i < gameManager.simulationManager._NumAgents; i++) {
            int index = (cameraManager.targetCritterIndex + i) % gameManager.simulationManager._NumAgents;

            if (gameManager.simulationManager.agentsArray[index].speciesIndex == selectedSpeciesID) {
                newIndex = index;
                break;
            }
        }
        
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);                
    }
    
    public void ClickInfoPanelExpand() {
        isActiveInfoPanel = !isActiveInfoPanel;

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

