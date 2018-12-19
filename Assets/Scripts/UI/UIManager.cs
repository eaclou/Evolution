using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameManager gameManager;
    //public SimulationManager simManager;
    public CameraManager cameraManager;
    public GameOptionsManager gameOptionsManager;
    public TreeOfLifeManager treeOfLifeManager;

    //public Canvas canvasMain;

    private bool firstTimeStartup = true;
        
    // Main Menu:
    public Button buttonQuickStartResume;
    public Button buttonNewSimulation;
    public Text textMouseOverInfo;

    public Texture2D healthDisplayTex;

    public GameObject panelTint;

    public bool controlsMenuOn = false;
    public bool optionsMenuOn = false;

    public Text textLoadingTooltips;

    public GameObject panelStatsHUD;
    public Animator animatorStatsPanel;
    public Button buttonStats;
    public bool isActiveStatsPanel = false;

    public ToolType curActiveTool;
    public enum ToolType {
        None,
        Stir,
        Inspect,
        Feed,
        Mutate
    }

    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public GameObject mouseRaycastWaterPlane;
    private Vector3 prevMousePositionOnWaterPlane;
    public Vector3 curMousePositionOnWaterPlane;

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
    
    public bool isActiveFeedToolPanel = false;
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

    public bool isActiveToolsPanel = false;
    public Button buttonToolOpenClose;
    public Button buttonToolStir;
    public Button buttonToolInspect;
    public Button buttonToolFeed;
    public Button buttonToolMutate;
    public GameObject panelTools;
    public Animator animatorToolsPanel;
        

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

    // EVENTS TOOL SECTION:
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
    public Text textTolDescription;
    public Image imageTolPortraitRender;
    public Image imageTolWorldStatsRender;
    public Image imageTolSpeciesTreeRender;
    public Image imageTolEventsTimelineRender;
    public Button buttonTolCycleWorldStatsPrev;
    public Button buttonTolCycleWorldStatsNext;
    public Text textTolWorldStatsName;
    public Button buttonTolCycleSpeciesStatsPrev;
    public Button buttonTolCycleSpeciesStatsNext;
    public Text textTolSpeciesStatsName;
    public Button buttonTolEventsMinorToggle;
    public Button buttonTolEventsMajorToggle;
    public Button buttonTolEventsExtremeToggle;
    public Text textTolEventsTimelineName;

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

    // VVV OLD BELOW:::
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
    /// <summary>
    /// //
    /// </summary>
    /// 
    //public Texture2D fitnessDisplayTexture;

    public float timeOfLastPlayerDeath = 0f;

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

    public Text statsPanelTextMaxValue;
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

    public enum WorldStatsMode {
        Decay,
        Plant,
        Eggs,
        Corpse,
        BrainMutationFreq,
        BrainMutationAmp,
        BrainSizeBias,
        BodyMutationFreq,
        BodyMutationAmp,
        BodySensorVariance,
        WaterCurrents
    }

    public enum SpeciesStatsMode {
        Lifespan,
        ConsumptionDecay,
        ConsumptionPlant,
        ConsumptionMeat,
        BodySize,
        SpecAttack,
        SpecDefend,
        SpecSpeed,
        SpecUtility,
        DietSpecDecay,
        DietSpecPlant,
        DietSpecMeat,
        NumNeurons,
        NumAxons,
        Experience,
        Fitness,
        DamageDealt,
        DamageTaken
    }

    public Button buttonGraphLifespan;
    public Button buttonGraphBodySizes;
    public Button buttonGraphConsumption;
    public Button buttonGraphPredation;
    public Button buttonGraphNutrients;
    public Button buttonGraphMutation;

    public float[] maxValuesStatArray;

    public float maxLifespanValue = 0.00001f;
    public float maxBodySizeValue = 0.00001f;
    public float maxFoodEatenValue = 0.00001f;
    public float maxPredationValue = 0.00001f;
    public float maxNutrientsValue = 0.00001f;
    public float maxMutationValue = 0.00001f;

    public Vector2 smoothedMouseVel;
    private Vector2 prevMousePos;

    public Vector2 smoothedCtrlCursorVel;
    private Vector2 prevCtrlCursorPos;
    private Vector3 prevCtrlCursorPositionOnWaterPlane;
    public Vector3 curCtrlCursorPositionOnWaterPlane;
    private bool rightTriggerOn = false;

    public bool isDraggingMouse = false;
    public bool isDraggingSpeciesNode = false;

    private int selectedAgentID;

    private const int maxDisplaySpecies = 32;

    public Color buttonEventMinorColor = new Color(53f / 255f, 114f / 255f, 97f / 255f);
    public Color buttonEventMajorColor = new Color(53f / 255f, 67f / 255f, 107f / 255f);
    public Color buttonEventExtremeColor = new Color(144f / 255f, 54f / 255f, 82f / 255f);
    // Tree of Life:
    //public Image imageTreeOfLifeDisplay;
    
	// Use this for initialization
	void Start () {

        animatorStatsPanel.enabled = false;
        animatorInspectPanel.enabled = false;
        animatorToolsPanel.enabled = false;
        animatorFeedToolPanel.enabled = false;
        animatorMutateToolPanel.enabled = false;
        animatorStirToolPanel.enabled = false;

        buttonToolStir.GetComponent<Image>().color = buttonDisabledColor;   
        buttonToolInspect.GetComponent<Image>().color = buttonDisabledColor;        
        buttonToolFeed.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolMutate.GetComponent<Image>().color = buttonDisabledColor;

        simEventComponentsArray = new SimEventComponent[4];
        simEventComponentsArray[0] = simEvent0;
        simEventComponentsArray[1] = simEvent1;
        simEventComponentsArray[2] = simEvent2;
        simEventComponentsArray[3] = simEvent3;
    }

    
    public void UpdateObserverModeUI() {
        if(isObserverMode) {
            panelObserverMode.SetActive(true);

            //textCurGen.text = "Generation: " + gameManager.simulationManager.curApproxGen.ToString("F0");
            //textAvgLifespan.text = "Average Lifespan: " + Mathf.RoundToInt(gameManager.simulationManager.rollingAverageAgentScoresArray[0]).ToString();
            
            Vector2 moveDir = Vector2.zero;
            bool isKeyboardInput = false;
            // CONTROLLER:
            float controllerHorizontal = Input.GetAxis ("Horizontal"); 
            float controllerVertical = Input.GetAxis ("Vertical");
            moveDir.x = controllerHorizontal;
            moveDir.y = controllerVertical;
            
            Vector2 rightStickInput = new Vector2(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"));
            
            float leftTrigger = Input.GetAxis ("LeftTrigger");
            float rightTrigger = Input.GetAxis ("RightTrigger");
            //float buttonA = Input.GetAxis("ButtonA");
            //float buttonB = Input.GetAxis("ButtonB");
            //float buttonX = Input.GetAxis("ButtonX");
            //float buttonY = Input.GetAxis("ButtonY");

            /*if(rightStickInput.sqrMagnitude > 0.01f) {
                Debug.Log("Controller Right Stick: (" + rightStickInput.x.ToString() + ", " + rightStickInput.y.ToString() + 
                ")\nTriggers (" + leftTrigger.ToString() + ", " + rightTrigger.ToString() + ") A: " + 
                buttonA.ToString() + ", B: " + buttonB.ToString() + ", X: " + buttonX.ToString() + ", Y: " + buttonY.ToString());

            }*/
            
            if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {  // UP !!!!
                moveDir.y = 1f;
                isKeyboardInput = true;
                //cameraManager.targetCamPos.y += camPanSpeed;
                //StopFollowing();                
            }
            if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {  // DOWN !!!!                
                moveDir.y = -1f;
                isKeyboardInput = true;
            }            
            if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {  // RIGHT !!!!
                moveDir.x = 1f;
                isKeyboardInput = true;
            }
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {  // LEFT !!!!!                
                moveDir.x = -1f;
                isKeyboardInput = true;
            }
            
            if(isKeyboardInput) {   
                moveDir = moveDir.normalized;
                StopFollowing();
            }
            if(moveDir.sqrMagnitude > 0.001f) {                
                StopFollowing();
            }
            cameraManager.MoveCamera(moveDir);

            if (Input.GetKey(KeyCode.R)) 
            {
                cameraManager.TiltCamera(-1f);
                //cameraManager.masterTargetTiltAngle -= cameraManager.masterTiltSpeed * tiltSpeedMult * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.F)) 
            {
                cameraManager.TiltCamera(1f);
                //cameraManager.masterTargetTiltAngle += cameraManager.masterTiltSpeed * tiltSpeedMult * Time.deltaTime;
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
            if(Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log("Pressed Spacebar!");
                isPaused = !isPaused;
                if (isPaused)
                {
                    ClickButtonPause();
                }
                else
                {
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
                ClickStatsButton();
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
            
            //bool isDragging = Input.GetMouseButton(0);
            
            //if(curActiveTool == ToolType.Inspect) {
            //    MouseRaycastInspect(leftClickThisFrame);
            //}
            MouseRaycastCheck(leftClickThisFrame);

            bool rightTriggerDownThisFrame = false;
            float rightTriggerVal = Input.GetAxis("RightTrigger");
            bool ctrlCursorDragging = false;
            if(rightTriggerVal > 0.01f) {
                if(rightTriggerOn) {    
                    ctrlCursorDragging = true;
                }
                else {
                    rightTriggerDownThisFrame = true;
                }
                rightTriggerOn = true;
                
            }
            else {
                rightTriggerOn = false;
            }

            Vector2 curRightStickPos = rightStickInput;
            curRightStickPos.y *= -1f;
            Vector2 instantCtrlCursorVel = curRightStickPos - prevCtrlCursorPos;           
            smoothedCtrlCursorVel = Vector2.Lerp(smoothedCtrlCursorVel, instantCtrlCursorVel, 0.167f);
            prevCtrlCursorPos = curRightStickPos;

            Vector4[] dataArray = new Vector4[1];
            Vector4 gizmoPos = new Vector4(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f, 0f);

            if(rightTriggerOn) {                    
                mouseRaycastWaterPlane.SetActive(true);
                MouseRaycastWaterPlane(false, ctrlCursorDragging, true, new Vector3((rightStickInput.x * 0.5f + 0.5f) * 1920f, (-rightStickInput.y * 0.5f + 0.5f) * 1080f, 0f), smoothedCtrlCursorVel * 512f); // smoothedCtrlCursorVel); 

                gizmoPos = new Vector4(curCtrlCursorPositionOnWaterPlane.x, curCtrlCursorPositionOnWaterPlane.y, 0f, 0f);
                
                //gameManager.theRenderKing.gizmoStirToolPosCBuffer.SetData(dataArray);
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
                float isActing = 0f;
                if (ctrlCursorDragging)
                    isActing = 1f;
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsStirring", isActing);
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_Radius", 6f);
            }
            else {
                gameManager.simulationManager.PlayerToolStirOff();
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
            }

            dataArray[0] = gizmoPos;
            gameManager.theRenderKing.gizmoStirToolPosCBuffer.SetData(dataArray);

            if(curActiveTool == ToolType.Stir) {                    
                mouseRaycastWaterPlane.SetActive(true);
                MouseRaycastWaterPlane(false, isDraggingMouse, true, Input.mousePosition, smoothedMouseVel); 
                
                //gameManager.theRenderKing.gizmoStirToolPosCBuffer.SetData(dataArray);
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
                float isActing = 0f;
                if (isDraggingMouse)
                    isActing = 1f;
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsStirring", isActing);
                gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_Radius", 4f);
            }
            else {
                if (!rightTriggerOn) {
                    gameManager.theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
                }                
            }
            if(curActiveTool == ToolType.Feed) {                    
                mouseRaycastWaterPlane.SetActive(true);
                MouseRaycastWaterPlane(leftClickThisFrame, false, false, Input.mousePosition, smoothedMouseVel);
                
                float brushRadius = 5f;
                if(foodToolPourOn) {
                    brushRadius = 15f;
                }
                gameManager.theRenderKing.gizmoFeedToolMat.SetFloat("_IsVisible", 1f);
                float isActing = 0f;
                if (leftClickThisFrame)
                    isActing = 1f;
                gameManager.theRenderKing.gizmoFeedToolMat.SetFloat("_IsStirring", isActing);
                gameManager.theRenderKing.gizmoFeedToolMat.SetFloat("_Radius", brushRadius);
            }
            else {
                gameManager.theRenderKing.gizmoFeedToolMat.SetFloat("_IsVisible", 0f);
            }

            if(curActiveTool != ToolType.Feed && curActiveTool != ToolType.Stir) {
                mouseRaycastWaterPlane.SetActive(false);
            }
            
            if (Input.GetMouseButtonDown(1)) {
                Debug.Log("RIGHT CLICKETY-CLICK!");
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0f ) //  Forwards
            {
                cameraManager.ZoomCamera(-1f);

                //Debug.Log("Mouse ScrollWheel Forward");                
                //cameraManager.masterTargetDistance -= zoomSpeed;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) //  Backwarfds
            {
                cameraManager.ZoomCamera(1f);

                //Debug.Log("Mouse ScrollWheel Backward");
                //cameraManager.masterTargetDistance += zoomSpeed;               
            }

            float zoomSpeed = 0.167f;
            float zoomVal = 0f;
            //cameraManager.ZoomCamera(Input.GetAxis("RightShoulder") * zoomSpeed);
            //Debug.Log("RightShoulder " + Input.GetAxis("RightShoulder").ToString() );
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
    
    private void MouseRaycastWaterPlane(bool clicked, bool heldDown, bool stirOn, Vector3 screenPos, Vector2 smoothedVel) {
        
        Vector3 camPos = cameraManager.gameObject.transform.position;                
        Ray ray = cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(screenPos);
        RaycastHit hit = new RaycastHit();
        int layerMask = 1 << 12;
        Physics.Raycast(ray, out hit, layerMask);

        if(hit.collider != null) {
            curMousePositionOnWaterPlane = hit.point;
            curCtrlCursorPositionOnWaterPlane = hit.point;
            
            if (clicked) {
                //Debug.Log("CLICK Hit Water Plane! Coords: " + hit.point.ToString());                
                if(curActiveTool == ToolType.Feed) {
                    if(foodToolSprinkleOn) {
                        gameManager.simulationManager.PlayerFeedToolSprinkle(hit.point);
                    }
                    if(foodToolPourOn) {
                        gameManager.simulationManager.PlayerFeedToolPour(hit.point);
                    }
                }
            }

            if(heldDown) {
                if(stirOn) {
                    //Vector2 forceVector = new Vector2(curMousePositionOnWaterPlane.x - prevMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y - prevMousePositionOnWaterPlane.y);
                    float mag = smoothedVel.magnitude;
                    float radiusMult = (0.75f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                    if(mag > 0f) {
                        gameManager.simulationManager.PlayerToolStirOn(hit.point, smoothedVel * (0.75f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.25f), radiusMult);
                    }
                }
            }
            else {
                gameManager.simulationManager.PlayerToolStirOff();
            }
            

            prevMousePositionOnWaterPlane = curMousePositionOnWaterPlane;
            prevCtrlCursorPositionOnWaterPlane = curCtrlCursorPositionOnWaterPlane;
        }
    }
    private void MouseRaycastCheck(bool clicked) {
        
        Vector3 camPos = cameraManager.gameObject.transform.position;
        
        float mouseRatioX = Input.mousePosition.x / Screen.width;
        float mouseRatioY = Input.mousePosition.y / Screen.height;
        Vector3 mousePos = new Vector3(mouseRatioX, mouseRatioY, -1f);
        Ray ray = cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //int layerMask = 0;
        Physics.Raycast(ray, out hit);

        //if(clicked) {
        //    Debug.Log((mousePos * 0.5f).x.ToString() + ", " + (mousePos * 0.5f).y.ToString());
        //}
        

        cameraManager.isMouseHoverAgent = false;
        cameraManager.mouseHoverAgentIndex = 0;
        cameraManager.mouseHoverAgentRef = null;

        if(hit.collider != null) {
            // How to handle multiple hits? UI Trumps environmental?
            TreeOfLifeNodeRaycastTarget speciesNodeRayTarget = hit.collider.gameObject.GetComponent<TreeOfLifeNodeRaycastTarget>();

            if(speciesNodeRayTarget != null) {
                selectedAgentID = speciesNodeRayTarget.speciesRef.speciesID;
                if(clicked) {
                    ClickOnSpeciesNode(selectedAgentID);                    
                }
                else {
                    treeOfLifeManager.HoverOverSpeciesNode(selectedAgentID);
                }
            }
            else {
                treeOfLifeManager.HoverAllOff();
            }
            // CHECK FOR AGENT COLLISION:
            Agent agentRef = hit.collider.gameObject.GetComponentInParent<Agent>();
            if(agentRef != null) {
                //Debug.Log("AGENT: [ " + agentRef.gameObject.name + " ] #" + agentRef.index.ToString());
                    
                if(clicked) {
                    cameraManager.SetTarget(agentRef, agentRef.index);
                    cameraManager.isFollowing = true;
                                        
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
            treeOfLifeManager.HoverAllOff();
        }
    }

    private void ClickOnSpeciesNode(int ID) {
        Debug.Log("Clicked Species[" + ID.ToString() + "]");                    
        treeOfLifeManager.ClickedOnSpeciesNode(ID);
        if(displaySpeciesIndicesArray != null) {
            int id = 0;
            for(int i = 0; i < displaySpeciesIndicesArray.Length; i++) {
                if(displaySpeciesIndicesArray[i] == ID) {
                    id = i;
                    break;
                }
            }
            statsGraphMatLifespan.SetInt("_SelectedSpeciesID", id);  // 
            statsGraphMatFoodEaten.SetInt("_SelectedSpeciesID", id);
            statsGraphMatBodySizes.SetInt("_SelectedSpeciesID", id);
            statsGraphMatPredation.SetInt("_SelectedSpeciesID", id);
        }
        
        textTreeOfLifeSpeciesID.text = "Species <size=24> " + ID.ToString() + "</size>";

        string speciesInfoTxt = "";
        speciesInfoTxt += "Parent Species: " + gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[ID].parentSpeciesID.ToString() + "\n";
        speciesInfoTxt += "Dimensions: { " + gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[ID].representativeGenome.bodyGenome.fullsizeBoundingBox.x.ToString("F2") + ", " +
            gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[ID].representativeGenome.bodyGenome.fullsizeBoundingBox.z.ToString("F2") + " }\n";
        speciesInfoTxt += "Avg Fitness: " + gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[ID].avgLifespan.ToString("F2");
        textTreeOfLifeInfoA.text = speciesInfoTxt;

        isDraggingSpeciesNode = true;
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
        //obsZoomLevel = 0; // C, zoomed out max

        ClickButtonModeC();

        isObserverMode = true;
        deathScreenOn = false;

        //gameManager.simulationManager.EnterObserverMode();
    }

    public void TransitionToNewGameState(GameManager.GameState gameState) {
        switch (gameState) {
            case GameManager.GameState.MainMenu:
                //canvasMain.renderMode = RenderMode.ScreenSpaceOverlay;
                EnterMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                EnterLoadingUI();
                
                //treeOfLifeManager = new TreeOfLifeManager(treeOfLifeAnchorGO, this);
                //treeOfLifeManager.FirstTimeInitialize(gameManager.simulationManager.masterGenomePool);
                break;
            case GameManager.GameState.Playing:
                //canvasMain.renderMode = RenderMode.ScreenSpaceCamera;
                // After self Initialized:                
                //ClickOnSpeciesNode(1);
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
        if(statsSpeciesColorKey == null) {
            statsSpeciesColorKey = new Texture2D(maxDisplaySpecies, 1, TextureFormat.ARGB32, false);
            statsSpeciesColorKey.filterMode = FilterMode.Point;
            statsSpeciesColorKey.wrapMode = TextureWrapMode.Clamp;
        }

        if(statsTreeOfLifeSpeciesTexArray.Length == 0) {
            statsTreeOfLifeSpeciesTexArray = new Texture2D[16]; // start with 16 choosable stats

            for(int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
                Texture2D statsTexture = new Texture2D(maxDisplaySpecies, 1, TextureFormat.RGBAFloat, false);
                statsSpeciesColorKey.filterMode = FilterMode.Bilinear;
                statsSpeciesColorKey.wrapMode = TextureWrapMode.Clamp;

                statsTreeOfLifeSpeciesTexArray[i] = statsTexture;
            }
        }

        if(maxValuesStatArray.Length == 0) {
            maxValuesStatArray = new float[16];
            for(int i = 0; i < maxValuesStatArray.Length; i++) {
                maxValuesStatArray[i] = 1f;
            }
        }

        if(statsTextureLifespan == null) {
            statsTextureLifespan = new Texture2D(1, 1, TextureFormat.RFloat, true);
            statsTextureLifespan.filterMode = FilterMode.Bilinear;
            statsTextureLifespan.wrapMode = TextureWrapMode.Clamp;            
        }         
        statsGraphMatLifespan.SetTexture("_MainTex", statsTextureLifespan);
        statsGraphMatLifespan.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
        


        if(statsTextureBodySizes == null) {
            statsTextureBodySizes = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
            statsTextureBodySizes.filterMode = FilterMode.Bilinear;
            statsTextureBodySizes.wrapMode = TextureWrapMode.Clamp;            
        }         
        statsGraphMatBodySizes.SetTexture("_MainTex", statsTextureBodySizes);
        statsGraphMatBodySizes.SetTexture("_ColorKeyTex", statsSpeciesColorKey);

        if (statsTextureFoodEaten == null) {
            statsTextureFoodEaten = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
            statsTextureFoodEaten.filterMode = FilterMode.Bilinear;
            statsTextureFoodEaten.wrapMode = TextureWrapMode.Clamp;
        }
        statsGraphMatFoodEaten.SetTexture("_MainTex", statsTextureFoodEaten);
        statsGraphMatFoodEaten.SetTexture("_ColorKeyTex", statsSpeciesColorKey);

        if (statsTexturePredation == null) {
            statsTexturePredation = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
            statsTexturePredation.filterMode = FilterMode.Bilinear;
            statsTexturePredation.wrapMode = TextureWrapMode.Clamp;
        }
        statsGraphMatPredation.SetTexture("_MainTex", statsTexturePredation);
        statsGraphMatPredation.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
        
        if (statsTextureNutrients == null) {
            statsTextureNutrients = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
            statsTextureNutrients.filterMode = FilterMode.Bilinear;
            statsTextureNutrients.wrapMode = TextureWrapMode.Clamp;
        }
        statsGraphMatNutrients.SetTexture("_MainTex", statsTextureNutrients);

        if(tolTextureWorldStats == null) {
            tolTextureWorldStats = new Texture2D(1, 32, TextureFormat.RGBAFloat, true);
            tolTextureWorldStats.filterMode = FilterMode.Bilinear;
            tolTextureWorldStats.wrapMode = TextureWrapMode.Clamp;
        }
        if(tolTextureWorldStatsKey == null) {
            tolTextureWorldStatsKey = new Texture2D(2, 32, TextureFormat.RGBAFloat, true);  // 0 = color, 1 = stats
            tolTextureWorldStatsKey.filterMode = FilterMode.Point;
            tolTextureWorldStatsKey.wrapMode = TextureWrapMode.Clamp;

            tolTextureWorldStatsKey.SetPixel(0, 0, new Color(1f, 1f, 0f));
            tolTextureWorldStatsKey.SetPixel(0, 1, new Color(0f, 1f, 0f));
            tolTextureWorldStatsKey.SetPixel(0, 2, new Color(0f, 1f, 1f));
            tolTextureWorldStatsKey.SetPixel(0, 3, new Color(1f, 0f, 0f));
            tolTextureWorldStatsKey.SetPixel(0, 4, new Color(0.5f, 1f, 0f));
            tolTextureWorldStatsKey.SetPixel(0, 5, new Color(0.5f, 1f, 0.5f));
            tolTextureWorldStatsKey.SetPixel(0, 6, new Color(0.5f, 1f, 1f));
            tolTextureWorldStatsKey.SetPixel(0, 7, new Color(1f, 0.5f, 0f));
            tolTextureWorldStatsKey.SetPixel(0, 8, new Color(1f, 0.5f, 0.5f));
            tolTextureWorldStatsKey.SetPixel(0, 9, new Color(1f, 0.5f, 1f));
            tolTextureWorldStatsKey.SetPixel(0, 10, new Color(0f, 0f, 1f));

            tolTextureWorldStatsKey.Apply();
        }        
        
        tolWorldStatsValueRangesKeyArray = new Vector2[32]; // starts not null due to being public variable in inspector???
        for(int i = 0; i < tolWorldStatsValueRangesKeyArray.Length; i++) {
            tolWorldStatsValueRangesKeyArray[i] = new Vector2(0f, 1f);
        }
        
        //Debug.Log("created key array! " + tolWorldStatsValueRangesKeyArray.Length.ToString());

        if (statsTextureMutation == null) {
            statsTextureMutation = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
            statsTextureMutation.filterMode = FilterMode.Bilinear;
            statsTextureMutation.wrapMode = TextureWrapMode.Clamp;
        }
        statsGraphMatMutation.SetTexture("_MainTex", statsTextureMutation);
        
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
        treeOfLifeManager.UpdateVisualUI(treeOfLifePanelOn);
    }
    private void UpdateMainMenuUI() {
        
        if(firstTimeStartup) {
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

        panelHUD.SetActive(isActiveHUD);
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
        UpdateScoreText(Mathf.RoundToInt(gameManager.simulationManager.agentsArray[0].masterFitnessScore));

        SetDisplayTextures();

        UpdateDebugUI();
        UpdateHUDUI();
        UpdateObserverModeUI();
        //UpdateDeathScreenUI();
        UpdatePausedUI();

        UpdateStatsPanelUI();
        UpdateInspectPanelUI();
        UpdateFeedToolPanelUI();
        UpdateMutateToolPanelUI();
        UpdateStirToolPanelUI();

        UpdateSimEventsUI();

        Vector2 curMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        Vector2 instantMouseVel = curMousePos - prevMousePos;

        smoothedMouseVel = Vector2.Lerp(smoothedMouseVel, instantMouseVel, 0.2f);
        //smoothedCtrlCursorVel = Vector2.Lerp(smoothedCtrlCursorVel, )

        prevMousePos = curMousePos;
    }


    public void UpdateSimEventsUI() {
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
    
    public void UpdateDebugUI() {

        // DISABLED!!!! -- Need to establish good method for grabbing data from SimulationManager!
        SimulationManager simManager = gameManager.simulationManager;

        //string debugTxt1 = "avgFitnessScore: " + simManager.masterGenomePool.completeSpeciesPoolsList[0].avgFitnessScore.ToString() + "\n"; // Training: False";

        //debugTxt = "Training: ACTIVE   numSamples: " + dataSamplesList.Count.ToString() + "\n";
        //debugTxt += "Gen: " + curGen.ToString() + ", Agent: " + curTestingGenomeSupervised.ToString() + ", Sample: " + curTestingSample.ToString() + "\n";
        //debugTxt += "Fitness Best: " + bestFitnessScoreSupervised.ToString() + " ( Avg: " + avgFitnessLastGenSupervised.ToString() + " ) Blank: " + lastGenBlankAgentFitnessSupervised.ToString() + "\n";
        
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
            debugTxtAgent += "Total Food Eaten -- Decay: " + agentRef.totalFoodEatenDecay.ToString("F3") + ", Plant: " + agentRef.totalFoodEatenPlant.ToString("F2") + ", Meat: " + agentRef.totalFoodEatenMeat.ToString("F2") + "\nFood Stored: " + agentRef.coreModule.foodStored[0].ToString() + ", Corpse Food Amount: " + agentRef.currentCorpseFoodAmount.ToString("F3") + "\n";

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
            
            

            string debugTxtSpecies = "";
            int selectedSpeciesIndex = agentRef.speciesIndex;
            debugTxtSpecies += "SPECIES# " + selectedSpeciesIndex.ToString();
            //debugTxtSpecies += "THE BRAIN !!!\n\n"; // + agentRef.coreModule.coreWidth.ToString() + "\n";
            //debugTxt2 += "# Neurons: " + cameraManager.targetAgent.brain.neuronList.Count.ToString() + ", # Axons: " + cameraManager.targetAgent.brain.axonList.Count.ToString() + "\n\n";
           
            //debugTxtSpecies += "OutComms: [ " + agentRef.communicationModule.outComm0[0].ToString("F2") + ", " + agentRef.communicationModule.outComm1[0].ToString("F2") + ", " + agentRef.communicationModule.outComm2[0].ToString("F2") + ", " + agentRef.communicationModule.outComm3[0].ToString("F2") + " ]\n";
            //debugTxtSpecies += "Dash: " + agentRef.movementModule.dash[0].ToString("F2") + "\n";

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
            textDebugTrainingInfo2.text = debugTxtSpecies;
            textDebugTrainingInfo3.text = debugTxtGlobalSim;
            
        }        
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
    private void UpdateStatsPanelUI() {
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
        textStomachContents.text = "Food\n" + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].foodAmount * 100f).ToString("F1") + "%\nCapacity: " + gameManager.simulationManager.agentsArray[critterIndex].coreModule.stomachCapacity.ToString("F3");

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
        textEnergy.text = "Energy\n" + (gameManager.simulationManager.simStateData.critterSimDataArray[critterIndex].energy * 100f).ToString("F1") + "%";

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
    private void UpdateFeedToolPanelUI() {
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


    }
    private void UpdateStirToolPanelUI() {

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

            Vector3 hue = pool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            statsSpeciesColorKey.SetPixel(i, 1, new Color(hue.x, hue.y, hue.z));            
            //Debug.Log("(" + i.ToString() + ", " + gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i].ToString());
            displaySpeciesIndicesArray[i] = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i];

            TheRenderKing.TreeOfLifeSpeciesKeyData keyData = new TheRenderKing.TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.hue = hue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = 1f;
            int selectedID = 1;
            if(cameraManager.targetAgent != null) {
                selectedID = cameraManager.targetAgent.speciesIndex;
            }
            keyData.isSelected = (selectedID == gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]) ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        // Then fill with most recently extinct:
        for(int i = (numTotalSpecies - 1); i > Mathf.Clamp((numTotalSpecies - maxDisplaySpecies), 0, numTotalSpecies); i--) {
            SpeciesGenomePool pool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i];

            Vector3 hue = pool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            if(gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                hue = Vector3.Lerp(hue, Vector3.one * 0.2f, 0.75f);
            }
            statsSpeciesColorKey.SetPixel(i, 1, new Color(hue.x, hue.y, hue.z));
            //Debug.Log("(" + i.ToString() + ", ");
            displaySpeciesIndicesArray[i] = i;

            TheRenderKing.TreeOfLifeSpeciesKeyData keyData = new TheRenderKing.TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.hue = hue;
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
            int selectedID = 1;
            if(cameraManager.targetAgent != null) {
                selectedID = cameraManager.targetAgent.speciesIndex;
            }
            keyData.isSelected = selectedID == i ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        statsSpeciesColorKey.Apply();

        gameManager.simulationManager.theRenderKing.treeOfLifeSpeciesDataKeyCBuffer.SetData(speciesKeyDataArray);

        // ========== data: =========== //
        int years = Mathf.Min(2048, year);  // cap textures at 2k for now?
        // check for resize before doing it?
        for(int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
            statsTreeOfLifeSpeciesTexArray[i].Resize(Mathf.Max(1, years), maxDisplaySpecies);
        }
        //statsTextureLifespan.Resize(Mathf.Max(1, years), maxDisplaySpecies);
        //statsTextureFoodEaten.Resize(Mathf.Max(1, years), maxDisplaySpecies);
        //statsTextureBodySizes.Resize(Mathf.Max(1, years), maxDisplaySpecies);
        //statsTexturePredation.Resize(Mathf.Max(1, years), maxDisplaySpecies);

        //Debug.Log("tex: " + statsTextureLifespan.width.ToString() + ", " + statsTextureLifespan.height.ToString());

        //float maxValueLifespan = 0.01f;
        //float maxValueConsumptionDecay = 0.01f;
        //float maxValueBodySize = 0.01f;
        //float maxValueDietTypeDecay = 0.01f;
        
        for(int i = 0; i < maxValuesStatArray.Length; i++) {
            maxValuesStatArray[i] = 1f;
        }
        // for each year & each species, create 2D texture with fitness scores:
        for(int s = 0; s < maxDisplaySpecies; s++) {            
            
            if(displaySpeciesIndicesArray[s] < gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {

                SpeciesGenomePool speciesPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[displaySpeciesIndicesArray[s]];
                if(speciesPool == null) {
                    Debug.LogError("well shit");
                }
                for(int t = 0; t < years; t++) {
                
                    int index = t - speciesPool.yearCreated;
                    
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
                            valStat = speciesPool.avgNumNeuronsPerYearList[t];
                        }
                        else if(a == 10) {
                            valStat = speciesPool.avgNumAxonsPerYearList[t];
                        }
                        else if(a == 11) {
                            valStat = speciesPool.avgExperiencePerYearList[t];
                        }
                        else if(a == 12) {
                            valStat = speciesPool.avgFitnessScorePerYearList[t];
                        }
                        else if(a == 13) {
                            valStat = speciesPool.avgDamageDealtPerYearList[t];
                        }
                        else if(a == 14) {
                            valStat = speciesPool.avgDamageTakenPerYearList[t];
                        }
                        else if(a == 15) {
                            valStat = speciesPool.avgLifespanPerYearList[t];
                        }

                        maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);

                        //statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                        statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                    }

                    
                    
                    
                    /*float valLifespan = speciesPool.avgLifespanPerYearList[t];
                    float valConsumptionDecay = speciesPool.avgConsumptionDecayPerYearList[t];
                    float valBodySize = speciesPool.avgBodySizePerYearList[t];
                    float valFoodSpecDecay = speciesPool.avgFoodSpecDecayPerYearList[t];                    

                    maxValueLifespan = Mathf.Max(maxValueLifespan, valLifespan);
                    maxValueConsumptionDecay = Mathf.Max(maxValueConsumptionDecay, valConsumptionDecay);
                    maxValueBodySize = Mathf.Max(maxValueBodySize, valBodySize);
                    maxValueDietTypeDecay = Mathf.Max(maxValueDietTypeDecay, valFoodSpecDecay);

                    statsTextureLifespan.SetPixel(t, s, new Color(valLifespan, valLifespan, valLifespan, valLifespan));  // *** look into ways to use the other 3 channels ***
                    statsTextureFoodEaten.SetPixel(t, s, new Color(valConsumptionDecay, valConsumptionDecay, valConsumptionDecay, valConsumptionDecay));
                    statsTextureBodySizes.SetPixel(t, s, new Color(valBodySize, valBodySize, valBodySize, valBodySize));
                    statsTexturePredation.SetPixel(t, s, new Color(valFoodSpecDecay, valFoodSpecDecay, valFoodSpecDecay, valFoodSpecDecay));
                    */
                }
                for (int b = 0; b < statsTreeOfLifeSpeciesTexArray.Length; b++) {
                    statsTreeOfLifeSpeciesTexArray[b].Apply();
                }
            }
            
        }
        //statsTextureLifespan.Apply();
        //statsTextureFoodEaten.Apply();
        //statsTextureBodySizes.Apply();
        //statsTexturePredation.Apply();

        // move shader parameter setting here?
        /*statsGraphMatLifespan.SetFloat("_MaxValue", maxValueLifespan); 
        statsGraphMatLifespan.SetFloat("_MinValue", 500f);
        statsGraphMatFoodEaten.SetFloat("_MaxValue", maxValueConsumptionDecay);
        statsGraphMatFoodEaten.SetFloat("_MinValue", 0f);
        statsGraphMatBodySizes.SetFloat("_MaxValue", maxValueBodySize);      
        statsGraphMatBodySizes.SetFloat("_MinValue", 0f);
        statsGraphMatPredation.SetFloat("_MaxValue", maxValueDietTypeDecay);
        statsGraphMatPredation.SetFloat("_MinValue", 0f);
        
        maxLifespanValue = maxValueLifespan;
        maxFoodEatenValue = maxValueConsumptionDecay;
        maxBodySizeValue = maxValueBodySize;
        maxPredationValue = maxValueDietTypeDecay;
        */
        // *** TEMP!!!!! **********
        int selectedSpeciesID = 1;
        if(cameraManager.targetAgent != null) {
            selectedSpeciesID = cameraManager.targetAgent.speciesIndex;
        }
        textTolSpeciesTitle.text = "<size=24>Species</size> <size=36> " + selectedSpeciesID.ToString() + "</size>";

        textTolDescription.text = "Fun Facts Here";
    }
    /*public void UpdaGraphDataTextures(int year) {
        int numActiveSpecies = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int numTotalSpecies = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count;

        //gameManager.simulationManager.masterGenomePool.UpdateYearlySpeciesStats(year);
                
        statsGraphMatLifespan.SetInt("_NumDisplayed", maxDisplaySpecies); 
        statsGraphMatFoodEaten.SetInt("_NumDisplayed", maxDisplaySpecies); 
        statsGraphMatBodySizes.SetInt("_NumDisplayed", maxDisplaySpecies); 
        statsGraphMatPredation.SetInt("_NumDisplayed", maxDisplaySpecies); 
        // need way to sort & pick the current 8 species to display
        displaySpeciesIndicesArray = new int[maxDisplaySpecies];
        
         // Get Active ones first:
        for(int i = 0; i < gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) { 
            Vector3 hue = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            statsSpeciesColorKey.SetPixel(i, 1, new Color(hue.x, hue.y, hue.z));            
            //Debug.Log("(" + i.ToString() + ", " + gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i].ToString());
            displaySpeciesIndicesArray[i] = gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
        }
        // Then fill with most recently extinct:
        for(int i = (numTotalSpecies - 1); i > Mathf.Clamp((numTotalSpecies - maxDisplaySpecies), 0, numTotalSpecies); i--) {
            Vector3 hue = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            if(gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                hue = Vector3.Lerp(hue, Vector3.one * 0.2f, 0.75f);
            }
            statsSpeciesColorKey.SetPixel(i, 1, new Color(hue.x, hue.y, hue.z));
            //Debug.Log("(" + i.ToString() + ", ");
            displaySpeciesIndicesArray[i] = i;
        }
        statsSpeciesColorKey.Apply();

        // ========== data: =========== //
        int years = Mathf.Min(2048, year);  // cap textures at 2k for now?
        // check for resize before doing it?
        statsTextureLifespan.Resize(Mathf.Max(1, years), maxDisplaySpecies);
        statsTextureFoodEaten.Resize(Mathf.Max(1, years), maxDisplaySpecies);
        statsTextureBodySizes.Resize(Mathf.Max(1, years), maxDisplaySpecies);
        statsTexturePredation.Resize(Mathf.Max(1, years), maxDisplaySpecies);

        //Debug.Log("tex: " + statsTextureLifespan.width.ToString() + ", " + statsTextureLifespan.height.ToString());

        float maxValueLifespan = 0.01f;
        float maxValueConsumption = 0.01f;
        float maxValueBodySize = 0.01f;
        float maxValueDietType = 0.01f;
        // for each year & each species, create 2D texture with fitness scores:
        for(int s = 0; s < maxDisplaySpecies; s++) {            
            
            if(displaySpeciesIndicesArray[s] < gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {

                SpeciesGenomePool speciesPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[displaySpeciesIndicesArray[s]];
                if(speciesPool == null) {
                    Debug.LogError("well shit");
                }
                for(int t = 0; t < years; t++) {
                
                    int index = t - speciesPool.yearCreated;
                    float valLifespan = 0f;
                    float valConsumption = 0f;
                    float valBodySize = 0f;
                    float valFoodSpec = 0f;
                    if(index >= 0) { // species didn't exist in this year - set fitness to 0
                        valLifespan = speciesPool.avgLifespanPerYearList[index];
                        valConsumption = speciesPool.avgConsumptionDecayPerYearList[index];
                        valBodySize = speciesPool.avgBodySizePerYearList[index];
                        valFoodSpec = speciesPool.avgFoodSpecDecayPerYearList[index]; // ** make these Vec3's
                    }
                    maxValueLifespan = Mathf.Max(maxValueLifespan, valLifespan);
                    maxValueConsumption = Mathf.Max(maxValueConsumption, valConsumption);
                    maxValueBodySize = Mathf.Max(maxValueBodySize, valBodySize);
                    maxValueDietType = Mathf.Max(maxValueDietType, valFoodSpec);

                    statsTextureLifespan.SetPixel(t, s, new Color(valLifespan, valLifespan, valLifespan, valLifespan));  // *** look into ways to use the other 3 channels ***
                    statsTextureFoodEaten.SetPixel(t, s, new Color(valConsumption, valConsumption, valConsumption, valConsumption));
                    statsTextureBodySizes.SetPixel(t, s, new Color(valBodySize, valBodySize, valBodySize, valBodySize));
                    statsTexturePredation.SetPixel(t, s, new Color(valFoodSpec, valFoodSpec, valFoodSpec, valFoodSpec));
                }
            }
            
        }
        statsTextureLifespan.Apply();
        statsTextureFoodEaten.Apply();
        statsTextureBodySizes.Apply();
        statsTexturePredation.Apply();

        // move shader parameter setting here?
        statsGraphMatLifespan.SetFloat("_MaxValue", maxValueLifespan); 
        statsGraphMatLifespan.SetFloat("_MinValue", 500f);
        statsGraphMatFoodEaten.SetFloat("_MaxValue", maxValueConsumption);
        statsGraphMatFoodEaten.SetFloat("_MinValue", 0f);
        statsGraphMatBodySizes.SetFloat("_MaxValue", maxValueBodySize);      
        statsGraphMatBodySizes.SetFloat("_MinValue", 0f);
        statsGraphMatPredation.SetFloat("_MaxValue", maxValueDietType);
        statsGraphMatPredation.SetFloat("_MinValue", 0f);

        maxLifespanValue = maxValueLifespan;
        maxFoodEatenValue = maxValueConsumption;
        maxBodySizeValue = maxValueBodySize;
        maxPredationValue = maxValueDietType;        
    }*/
    public void UpdateStatsTextureLifespan(int year) {
        
        

        // *** OLD BELOW:::: ****
        /*
        statsTextureLifespan.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i].x);
            maxValue = Mathf.Max(maxValue, data[i].y);
            maxValue = Mathf.Max(maxValue, data[i].z);
            maxValue = Mathf.Max(maxValue, data[i].w);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureLifespan.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTextureLifespan.Apply();

        maxLifespanValue = maxValue;
          */  
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

        maxBodySizeValue = maxValue;    
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

        maxFoodEatenValue = maxValue;     
    }
    public void UpdateStatsTexturePredation(List<Vector4> data) {
        statsTexturePredation.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 1f;
        /*for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i].x);
            maxValue = Mathf.Max(maxValue, data[i].y);
            maxValue = Mathf.Max(maxValue, data[i].z);
            maxValue = Mathf.Max(maxValue, data[i].w);
        }*/
        for(int i = 0; i < data.Count; i++) {
            statsTexturePredation.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTexturePredation.Apply();

        maxPredationValue = maxValue;     
    }
    public void UpdateTolWorldStatsTexture(List<Vector4> nutrientData) {
        //Debug.Log("UpdateTolWorldStatsTexture");
        int numDataPoints = Mathf.Max(1, nutrientData.Count);

        if(numDataPoints != tolTextureWorldStats.width) {  // resize needed?
            tolTextureWorldStats.Resize(numDataPoints, 32);  // 32 max values? should be more than enough
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
            // valRangeKey, x = min, y = max
            // nutrientData, x = decay, y = plant, z = egg, w = corpse
            //Debug.LogError(i.ToString() + ", count: " + nutrientData.Count.ToString());
            //Debug.LogError("data: " + nutrientData[i].x.ToString());
            //Debug.LogError("key: " + tolWorldStatsValueRangesKeyArray[0].x.ToString());
            // Decay:
            tolWorldStatsValueRangesKeyArray[0].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[0].x, nutrientData[i].x);
            tolWorldStatsValueRangesKeyArray[0].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[0].y, nutrientData[i].x);
            tolTextureWorldStats.SetPixel(i, 0, new Color(nutrientData[i].x, 0f, 0f));            
            // Plants:
            tolWorldStatsValueRangesKeyArray[1].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[1].x, nutrientData[i].y);
            tolWorldStatsValueRangesKeyArray[1].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[1].y, nutrientData[i].y);
            tolTextureWorldStats.SetPixel(i, 1, new Color(nutrientData[i].y, 0f, 0f));    
            // egg food:
            tolWorldStatsValueRangesKeyArray[2].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[2].x, nutrientData[i].z);
            tolWorldStatsValueRangesKeyArray[2].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[2].y, nutrientData[i].z);
            tolTextureWorldStats.SetPixel(i, 2, new Color(nutrientData[i].z, 0f, 0f));    
            // corpse food:
            tolWorldStatsValueRangesKeyArray[3].x = Mathf.Min(tolWorldStatsValueRangesKeyArray[3].x, nutrientData[i].w);
            tolWorldStatsValueRangesKeyArray[3].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[3].y, nutrientData[i].w);
            tolTextureWorldStats.SetPixel(i, 3, new Color(nutrientData[i].w, 0f, 0f)); 
            
            // MUTATION RATES:
            //
            float value = gameManager.simulationManager.statsHistoryBrainMutationFreqList[i];
            tolWorldStatsValueRangesKeyArray[4].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[4].x, value);
            tolWorldStatsValueRangesKeyArray[4].y = 10f; // Mathf.Max(tolWorldStatsValueRangesKeyArray[4].y, value);
            tolTextureWorldStats.SetPixel(i, 4, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryBrainMutationAmpList[i];
            tolWorldStatsValueRangesKeyArray[5].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[5].x, value);
            tolWorldStatsValueRangesKeyArray[5].y = 10f; // Mathf.Max(tolWorldStatsValueRangesKeyArray[5].y, value);
            tolTextureWorldStats.SetPixel(i, 5, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryBrainSizeBiasList[i];
            tolWorldStatsValueRangesKeyArray[6].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[6].x, value);
            tolWorldStatsValueRangesKeyArray[6].y = 10f; // Mathf.Max(tolWorldStatsValueRangesKeyArray[6].y, value);
            tolTextureWorldStats.SetPixel(i, 6, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryBodyMutationFreqList[i];
            tolWorldStatsValueRangesKeyArray[7].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[7].x, value);
            tolWorldStatsValueRangesKeyArray[7].y = 10f; // Mathf.Max(tolWorldStatsValueRangesKeyArray[7].y, value);
            tolTextureWorldStats.SetPixel(i, 7, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryBodyMutationAmpList[i];
            tolWorldStatsValueRangesKeyArray[8].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[8].x, value);
            tolWorldStatsValueRangesKeyArray[8].y = 10f; // Mathf.Max(tolWorldStatsValueRangesKeyArray[8].y, value);
            tolTextureWorldStats.SetPixel(i, 8, new Color(value, 0f, 0f));

            value = gameManager.simulationManager.statsHistoryBodySensorVarianceList[i];
            tolWorldStatsValueRangesKeyArray[9].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[9].x, value);
            tolWorldStatsValueRangesKeyArray[9].y = 10f; // Mathf.Max(tolWorldStatsValueRangesKeyArray[9].y, value);
            tolTextureWorldStats.SetPixel(i, 9, new Color(value, 0f, 0f));

            // WATER / WEATHER STATS:
            //
            value = gameManager.simulationManager.statsHistoryWaterCurrentsList[i];
            tolWorldStatsValueRangesKeyArray[10].x = 0f; // Mathf.Min(tolWorldStatsValueRangesKeyArray[10].x, value);
            tolWorldStatsValueRangesKeyArray[10].y = Mathf.Max(tolWorldStatsValueRangesKeyArray[10].y, value);
            tolTextureWorldStats.SetPixel(i, 10, new Color(value, 0f, 0f));
        }        
        
        // Min/Max values for each stat stored in second pixel --> set from valueRangeKeyArray, x = min, y = max
        for(int i = 0; i < tolWorldStatsValueRangesKeyArray.Length; i++) {
            tolTextureWorldStatsKey.SetPixel(1, i, new Color(tolWorldStatsValueRangesKeyArray[i].x, tolWorldStatsValueRangesKeyArray[i].y, 0f));
        }

        tolTextureWorldStats.Apply();
        tolTextureWorldStatsKey.Apply();        
    }
    public void UpdateStatsTextureNutrients(List<Vector4> data) {
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

    public void UpdateScoreText(int score) {
        textScore.text = "Score: " + score.ToString();
    }
        

    public void SetDisplayTextures() {
        foodMat.SetTexture("_MainTex", healthDisplayTex);
        hitPointsMat.SetTexture("_MainTex", healthDisplayTex);
    }

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
    
    public void ClickToolButtonStir() {

        if(curActiveTool != ToolType.Stir) {
            curActiveTool = ToolType.Stir;
             
            isActiveStirToolPanel = true;    
            animatorStirToolPanel.enabled = true;
            animatorStirToolPanel.Play("SlideOnPanelStirTool"); 
            buttonToolStir.GetComponent<Image>().color = buttonActiveColor; 

            TurnOffInspectTool();  
            TurnOffFeedTool();
            TurnOffMutateTool();
        }
        else {
            curActiveTool = ToolType.None;
            TurnOffStirTool();
        }        
    }
    public void ClickToolButtonInspect() {

        if(curActiveTool != ToolType.Inspect) {
            curActiveTool = ToolType.Inspect;

            TurnOnInspectTool();
            TurnOffStirTool();  
            TurnOffFeedTool();
            TurnOffMutateTool();
        } 
        else {
            curActiveTool = ToolType.None;
            TurnOffInspectTool();
        } 
    }
    public void ClickToolButtonFeed() {

        if(curActiveTool != ToolType.Feed) {
            curActiveTool = ToolType.Feed;

            isActiveFeedToolPanel = true;    
            animatorFeedToolPanel.enabled = true;
            animatorFeedToolPanel.Play("SlideOnPanelFeedTool"); 
            buttonToolFeed.GetComponent<Image>().color = buttonActiveColor;

            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffMutateTool();
        }  
        else {
            curActiveTool = ToolType.None;
            TurnOffFeedTool();
        } 
    }
    public void ClickFeedToolSprinkle() {
        
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

        if(curActiveTool != ToolType.Mutate) {
            curActiveTool = ToolType.Mutate;

            isActiveMutateToolPanel = true;    
            animatorMutateToolPanel.enabled = true;
            animatorMutateToolPanel.Play("SlideOnPanelMutateTool"); 
            buttonToolMutate.GetComponent<Image>().color = buttonActiveColor; 
                        
            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffFeedTool();
            // *** make these their own private functions for reuse: OLD:::
            //buttonToolStir.GetComponent<Image>().color = buttonDisabledColor;  
            //buttonToolFeed.GetComponent<Image>().color = buttonDisabledColor;
        }
        else {
            curActiveTool = ToolType.None;
            TurnOffMutateTool();
        }        
    }

    private void TurnOnInspectTool() {
        buttonToolInspect.GetComponent<Image>().color = buttonActiveColor;            
    }    
    private void TurnOffInspectTool() {
                
        buttonToolInspect.GetComponent<Image>().color = buttonDisabledColor;  
        if(isActiveInspectPanel) {
            animatorInspectPanel.enabled = true;
            animatorInspectPanel.Play("SlideOffPanelInspect");
        }        
        isActiveInspectPanel = false;
        StopFollowing();
    }
    private void TurnOffFeedTool() {
        buttonToolFeed.GetComponent<Image>().color = buttonDisabledColor;

        if(isActiveFeedToolPanel) {
            animatorFeedToolPanel.enabled = true;
            animatorFeedToolPanel.Play("SlideOffPanelFeedTool");
        }        
        isActiveFeedToolPanel = false;
    }
    private void TurnOffMutateTool() {
        buttonToolMutate.GetComponent<Image>().color = buttonDisabledColor;

        if(isActiveMutateToolPanel) {
            animatorMutateToolPanel.enabled = true;
            animatorMutateToolPanel.Play("SlideOffPanelMutateTool");
        }        
        isActiveMutateToolPanel = false;
    }
    private void TurnOffStirTool() {
        buttonToolStir.GetComponent<Image>().color = buttonDisabledColor;

        if(isActiveStirToolPanel) {
            animatorStirToolPanel.enabled = true;
            animatorStirToolPanel.Play("SlideOffPanelStirTool");
        }        
        isActiveStirToolPanel = false;
    }

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
    /*public void ClickBackToMainMenu() {
        optionsMenuOn = false;
        UpdateMainMenuUI();
    }*/
    public void ClickQuickStart() {
        Debug.Log("ClickQuickStart()!");
        if(firstTimeStartup) {
            gameManager.StartNewGameQuick();
        }
        else {
            animatorStatsPanel.enabled = false;
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
            animatorStatsPanel.enabled = false;
            animatorInspectPanel.enabled = false;
            gameManager.ResumePlaying();
            ClickButtonPlayNormal();

        }        
    }

    public void MouseEnterQuickStart() {
        textMouseOverInfo.gameObject.SetActive(true);
        textMouseOverInfo.text = "Start with an existing ecosystem full of various living organisms.";
    }
    public void MouseExitQuickStart() {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterNewSimulation() {
        textMouseOverInfo.gameObject.SetActive(true);
        textMouseOverInfo.text = "Create a brand new ecosystem from scratch. It might take a significant amount of time for intelligent creatures to evolve.\n*Not recommended for first-time players.";
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



    /*public void ClickResetWorld() {
        Debug.Log("Reset The World!");
        gameManager.simulationManager.ResetWorld();
    }*/

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
        //cameraManager.ChangeGameMode(CameraManager.GameMode.ModeA);
        //obsZoomLevel = 2; // C, zoomed out max
    }
    public void ClickButtonModeB() {
        //cameraManager.ChangeGameMode(CameraManager.GameMode.ModeB);
        //obsZoomLevel = 1; // C, zoomed out max
    }
    public void ClickButtonModeC() {
        //cameraManager.ChangeGameMode(CameraManager.GameMode.ModeC);
        //obsZoomLevel = 0; // C, zoomed out max
    }

    public void ClickButtonToggleHUD() {
        isActiveHUD = !isActiveHUD;
        panelHUD.SetActive(isActiveHUD);
    }
    public void ClickButtonToggleDebug() {
        isActiveDebug = !isActiveDebug;
        panelDebug.SetActive(isActiveDebug);
    }

    public void ClickPrevSpecies() {
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
    }
    public void ClickPrevAgent() {
        Debug.Log("ClickPrevAgent");
        int newIndex = cameraManager.targetCritterIndex - 1;
        if(newIndex < 0) {
            newIndex = gameManager.simulationManager._NumAgents - 1;                    
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);                
    }
    public void ClickNextAgent() {
        Debug.Log("ClickNextAgent");
        int newIndex = cameraManager.targetCritterIndex + 1;
        if(newIndex >= gameManager.simulationManager._NumAgents) {
            newIndex = 0;                      
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);                
    }

    public void ClickTreeOfLifeGroupOnOff() {
        treeOfLifePanelOn = !treeOfLifePanelOn;

        // Update panel:
        if(treeOfLifePanelOn) {            
            // Update treeOfLife colliders & display            
        }
        else {

        }
        //panelTreeOfLifeScaleHideGroup.SetActive(treeOfLifePanelOn);
        panelTolMaster.SetActive(treeOfLifePanelOn);

        treeOfLifeManager.UpdateVisualUI(treeOfLifePanelOn);
    }

    public void ClickTolWorldStatsOnOff() {
        tolWorldStatsOn = !tolWorldStatsOn;
        panelTolWorldStats.SetActive(tolWorldStatsOn);
    }
    public void ClickTolSpeciesTreeOnOff() {
        tolSpeciesTreeOn = !tolSpeciesTreeOn;
        panelTolSpeciesTree.SetActive(tolSpeciesTreeOn);
    }
    public void ClickTolEventsTimelineOnOff() {
        tolEventsTimelineOn = !tolEventsTimelineOn;
        panelTolEventsTimeline.SetActive(tolEventsTimelineOn);
    }

    public void ClickTolSpeciesTreeExtinctToggle() {
        tolSpeciesTreeExtinctOn = !tolSpeciesTreeExtinctOn;
    }
    public void ClickTolBrainDisplayToggle() {
        tolBrainDisplayOn = !tolBrainDisplayOn;
    }
    public void ClickTolInspectOnOff() {
        tolInspectOn = !tolInspectOn;
    }

    public void ClickTolWorldStatsCyclePrev() {
        tolSelectedWorldStatsIndex--;
        if(tolSelectedWorldStatsIndex < 0) {
            tolSelectedWorldStatsIndex = 10;
        }
        textTolWorldStatsName.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString();
    }
    public void ClickTolWorldStatsCycleNext() {
        tolSelectedWorldStatsIndex++;
        if(tolSelectedWorldStatsIndex > 10) {
            tolSelectedWorldStatsIndex = 0;
        }
        textTolWorldStatsName.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString();
    }
    public void ClickTolSpeciesTreeCyclePrev() {
        tolSelectedSpeciesStatsIndex--;
        if(tolSelectedSpeciesStatsIndex < 0) {
            tolSelectedSpeciesStatsIndex = 14;
        }
        textTolSpeciesStatsName.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString();
    }
    public void ClickTolSpeciesTreeCycleNext() {
        tolSelectedSpeciesStatsIndex++;
        if(tolSelectedSpeciesStatsIndex > 14) {
            tolSelectedSpeciesStatsIndex = 0;
        }
        textTolSpeciesStatsName.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString();
    }
    public void ClickTolEventsTimelineMinor() {

    }
    public void ClickTolEventsTimelineMajor() {

    }
    public void ClickTolEventsTimelineExtreme() {

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

}
