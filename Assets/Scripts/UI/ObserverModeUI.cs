using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObserverModeUI : MonoBehaviour
{
    public new bool enabled;
    public UIManager manager;
    public GameObject panelObserverMode;
    public WatcherUI watcherUI;
    public DebugPanelUI debugPanelUI;
    public BrushesUI brushesUI;
    public GenomeViewerUI genomeViewerUI;
    public GameObject panelPendingClickPrompt;
    
    GameManager gameManager => GameManager.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Text textTooltip => theCursorCzar.textTooltip;
    GameObject panelTooltip => theCursorCzar.panelTooltip;
    ZooplanktonManager zooplanktonManager => simulationManager.zooplanktonManager;
    VegetationManager vegetationManager => simulationManager.vegetationManager;
    ToolType curActiveTool => manager.curActiveTool;
    
    public int timerAnnouncementTextCounter = 0;
    public bool isAnnouncementTextOn = false;
    public bool isBrushAddingAgents = false;
    public bool updateTerrainAltitude;
    public float terrainUpdateMagnitude;
        
    public PanelFocus panelFocus = PanelFocus.WorldHub;
    public enum PanelFocus {
        None,
        WorldHub,
        Brushes,
        Watcher
    }
    
    bool isKeyboardInput;

    public void StepCamera(Vector2 input)
    {    
        panelObserverMode.SetActive(enabled);
        if (!enabled) return;

        isKeyboardInput = input != Vector2.zero;
        input = input.normalized;
        
        // WPP: extracted input system
        /*
        Vector2 moveDir = Vector2.zero;
        bool isKeyboardInput = false;
        float controllerHorizontal = Input.GetAxis("Horizontal");
        float controllerVertical = Input.GetAxis("Vertical");
        moveDir.x = controllerHorizontal;
        moveDir.y = controllerVertical;

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
        */

        // WPP: Conditions are the same???
        if (isKeyboardInput) {
            //moveDir = moveDir.normalized;
            watcherUI.StopFollowingAgent();
            watcherUI.StopFollowingPlantParticle();
            watcherUI.StopFollowingAnimalParticle();
        }
        if (input.sqrMagnitude > 0.001f) {
            watcherUI.StopFollowingAgent();
            watcherUI.StopFollowingPlantParticle();
            watcherUI.StopFollowingAnimalParticle();
        }

        cameraManager.MoveCamera(input);
        
        // WPP: commented out because only logging to console
        // and should be replaced with keyboard input calls when ready to implement
        /*
        //if (Input.GetKey(KeyCode.R)) {
        //    cameraManager.TiltCamera(-1f);
        //}
        //if (Input.GetKey(KeyCode.F)) {
        //    cameraManager.TiltCamera(1f);
        //}
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
        */
    }

    // WPP: break into separate functions, call from here
    public void Tick()
    {                    
        if (isAnnouncementTextOn) {
            panelPendingClickPrompt.SetActive(true);
            timerAnnouncementTextCounter++;

            if (timerAnnouncementTextCounter > 640) {
                isAnnouncementTextOn = false;
                timerAnnouncementTextCounter = 0;
                //inspectToolUnlockedAnnounce = false;
            }
        }
        else {
            panelPendingClickPrompt.SetActive(false);
        }
                                
        // If mouse is over ANY unity canvas UI object (with raycast enabled)
        if (EventSystem.current.IsPointerOverGameObject()) {  
            //Debug.Log("MouseOverUI!!!");
            if(genomeViewerUI.isTooltipHover) {
                panelTooltip.SetActive(true);
                string tipString = genomeViewerUI.tooltipString;
                    //if()
                textTooltip.text = tipString;
                textTooltip.color = Color.cyan;
            }
            else {
                panelTooltip.SetActive(false);
            }   
        }
        else {
            //int selectedPlantID = vegetationManager.selectedPlantParticleIndex;
            //int closestPlantID = vegetationManager.closestPlantParticleData.index;
            float plantDist = (vegetationManager.closestPlantParticleData.worldPos - new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y)).magnitude;

            //int selectedZoopID = zooplanktonManager.selectedAnimalParticleIndex;
            //int closestZoopID = zooplanktonManager.closestAnimalParticleData.index;
            float zoopDist = (new Vector2(zooplanktonManager.closestAnimalParticleData.worldPos.x, zooplanktonManager.closestAnimalParticleData.worldPos.y) - new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y)).magnitude;
            
            
            //if(plantDist < zoopDist) {                       
            //    if(panelFocus == PanelFocus.Watcher && !cameraManager.isMouseHoverAgent && theCursorCzar.leftClickThisFrame) { 
            //        if(selectedPlantID != closestPlantID && plantDist < 3.3f) {
            //            vegetationManager.selectedPlantParticleIndex = closestPlantID;
            //            vegetationManager.isPlantParticleSelected = true;
            //            Debug.Log("FOLLOWING PLANT " + vegetationManager.selectedPlantParticleIndex.ToString());
                        //isSpiritBrushSelected = true;
            //            watcherUI.StartFollowingPlantParticle();
            //        }
            //    }
                
            //}
            //else {                   
            //    if (panelFocus == PanelFocus.Watcher && !cameraManager.isMouseHoverAgent && theCursorCzar.leftClickThisFrame) {
            //        if (selectedZoopID != closestZoopID && zoopDist < 3.3f) {
            //            zooplanktonManager.selectedAnimalParticleIndex = closestZoopID;
            //            zooplanktonManager.isAnimalParticleSelected = true;
            //            Debug.Log("FOLLOWING ZOOP " + zooplanktonManager.selectedAnimalParticleIndex.ToString());
                        //isSpiritBrushSelected = true;
            //            watcherUI.StartFollowingAnimalParticle();
            //        }
            //    }
            //}
            
            watcherUI.isPlantParticleHighlight = 0f;
            watcherUI.isZooplanktonHighlight = 0f;
            watcherUI.isVertebrateHighlight = 0f;
            float hitboxRadius = 1f;
            if(cameraManager.isMouseHoverAgent) {  // move this to cursorCzar?
                watcherUI.isVertebrateHighlight = 1f;

                textTooltip.text = "Critter #" + cameraManager.mouseHoverAgentRef.candidateRef.candidateID.ToString();
                textTooltip.color = Color.white;
            }
            else {
                if(plantDist < zoopDist && plantDist < hitboxRadius) {
                    watcherUI.isPlantParticleHighlight = 1f;

                    textTooltip.text = "Algae #" + vegetationManager.closestPlantParticleData.index.ToString();
                    textTooltip.color = Color.green;
                }
                if(plantDist > zoopDist && zoopDist < hitboxRadius) {
                    watcherUI.isZooplanktonHighlight = 1f;

                    textTooltip.text = "Microbe #" + zooplanktonManager.closestAnimalParticleData.index.ToString();
                    textTooltip.color = Color.yellow;
                }
            }

            if (watcherUI.isPlantParticleHighlight == 0f && watcherUI.isZooplanktonHighlight == 0f && watcherUI.isVertebrateHighlight == 0f) {
                panelTooltip.SetActive(false);
            }
            else {
                panelTooltip.SetActive(true);
            }
            
            //Debug.Log("plantDist: " + plantDist.ToString() + ", zoopDist: " + zoopDist.ToString() + ",  agent: " + cameraManager.isMouseHoverAgent.ToString());
            
            if (curActiveTool == ToolType.Stir) {  
                theCursorCzar.stirGizmoVisible = true;
                
                float isActing = 0f;
                
                if (theCursorCzar.isDraggingMouseLeft) { 
                    isActing = 1f;
                    
                    float mag = theCursorCzar.smoothedMouseVel.magnitude;
                    float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                    if(mag > 0f) {
                        simulationManager.PlayerToolStirOn(theCursorCzar.curMousePositionOnWaterPlane, theCursorCzar.smoothedMouseVel * (0.25f + theRenderKing.baronVonWater.camDistNormalized * 1.2f), radiusMult);  
                    }
                    else {
                        simulationManager.PlayerToolStirOff();
                    }
                }
                else {
                    simulationManager.PlayerToolStirOff();                        
                }

                if(isActing > 0.5f) {
                    theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, 1f, 0.2f);
                }
                else {
                    theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, -4f, 0.2f);
                }
                theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft;
                theRenderKing.gizmoStirToolMat.SetFloat("_IsStirring", isActing);
                theRenderKing.gizmoStirStickAMat.SetFloat("_IsStirring", isActing);                    
                theRenderKing.gizmoStirStickAMat.SetFloat("_Radius", 6.2f);
            }

            theRenderKing.isBrushing = false;
            theRenderKing.isSpiritBrushOn = false;
            theRenderKing.nutrientToolOn = false;
            isBrushAddingAgents = false;
            vegetationManager.isBrushActive = false;
            
            if (curActiveTool == ToolType.Add && panelFocus == PanelFocus.Brushes) {
                // What Palette Trophic Layer is selected?
                theCursorCzar.stirGizmoVisible = true;

                simulationManager.PlayerToolStirOff();

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
            }
        }
        
        if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight) {
            theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);
        }
        else {
            theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);                   
        }
        
        if(theCursorCzar.stirGizmoVisible) {
            theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 1f);
            theRenderKing.gizmoStirStickAMat.SetFloat("_IsVisible", 1f);
            //Cursor.visible = false;
        }         
        else {
            theRenderKing.gizmoStirToolMat.SetFloat("_IsVisible", 0f);
            theRenderKing.gizmoStirStickAMat.SetFloat("_IsVisible", 0f);
            //Cursor.visible = true;
        }

        // WPP: call externally
        if (Input.GetMouseButtonDown(1)) {
            Debug.Log("RIGHT CLICKETY-CLICK!");
        }

        // WPP: removed 3/20/21, extracted to GetMouseScroll
        /*
        if (Input.GetAxis("Mouse ScrollWheel") > 0f ) //  Forwards
        {
            cameraManager.ZoomCamera(-1f);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) //  Backwards
        {  
            cameraManager.ZoomCamera(1f);                 
        }
        */

        // WPP: extract to Joystick input system
        float zoomSpeed = 0.2f; // * magic number, expose ("sensitivity")
        float zoomVal = 0f;
        if (Input.GetKey("joystick button 4")) //  Forwards
        {
            zoomVal += 1f;
            //Debug.Log("RightShoulder");     
        }
        if (Input.GetKey("joystick button 5")) //  Backwards
        {
            zoomVal -= 1f; 
            //Debug.Log("LeftShoulder");    
        }
        cameraManager.ZoomCamera(zoomVal * zoomSpeed);  
    }     
}

