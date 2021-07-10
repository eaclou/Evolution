using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObserverModeUI : MonoBehaviour
{
    UIManager manager => UIManager.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Text textTooltip => theCursorCzar.textTooltip;
    GameObject panelTooltip => theCursorCzar.panelTooltip;
    ZooplanktonManager zooplanktonManager => simulationManager.zooplanktonManager;
    VegetationManager vegetationManager => simulationManager.vegetationManager;
    ToolType curActiveTool => manager.curActiveTool;
    float cursorX => theCursorCzar.GetCursorPixelCoords().x;
    
    [SerializeField] int announcementDuration = 640;
        
    public new bool enabled;
    public GameObject panelObserverMode;
    public WatcherUI watcherUI;
    public DebugPanelUI debugPanelUI;
    public BrushesUI brushesUI;
    public GenomeViewerUI genomeViewerUI;
    public GameObject panelPendingClickPrompt;

    const string VISIBLE = "_IsVisible";
    const string STIRRING = "_IsStirring";
    const string RADIUS = "_Radius";
    
    int timerAnnouncementTextCounter;
    public bool isAnnouncementTextOn = false;
    public bool isBrushAddingAgents = false;
    public bool updateTerrainAltitude;
    public float terrainUpdateMagnitude;
        
    public PanelFocus panelFocus = PanelFocus.WorldHub;
    public enum PanelFocus 
    {
        None,
        WorldHub,
        Brushes,
        Watcher
    }
    
    bool isKeyboardInput;

    public void StepCamera(Vector2 input)
    {    
        panelObserverMode.SetActive(enabled);
        
        if (!enabled || input == Vector2.zero) 
            return;

        watcherUI.StopFollowingAgent();
        watcherUI.StopFollowingPlantParticle();
        watcherUI.StopFollowingAnimalParticle();

        cameraManager.MoveCamera(input.normalized);
    }

    // WPP: broken into helper functions
    public void Tick()
    {
        if(vegetationManager == null) return;

        TickAnnouncement();
                                
        // If mouse is over ANY unity canvas UI object (with raycast enabled)
        if (EventSystem.current.IsPointerOverGameObject()) 
        {  
            TickUITooltip();
        }
        else 
        {
            TickObjectTooltip();
            TickBrushes();
        }
        
        // WPP: branches have identical result
        //if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight) {
        //    theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);
        //}
        //else {
        theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);                   
        //}
        
        SetStirVisible();
    }
    
    void TickAnnouncement()
    {
        panelPendingClickPrompt.SetActive(isAnnouncementTextOn);
        if (!isAnnouncementTextOn) return;
        
        timerAnnouncementTextCounter++;

        if (timerAnnouncementTextCounter > announcementDuration) 
        {
            isAnnouncementTextOn = false;
            timerAnnouncementTextCounter = 0;
            //inspectToolUnlockedAnnounce = false;
        }
    }
    
    void TickUITooltip()
    {
        if(genomeViewerUI.isTooltipHover) 
        {
            panelTooltip.SetActive(true);
            string tipString = genomeViewerUI.tooltipString;
            textTooltip.text = tipString;
            textTooltip.color = Color.cyan;
        }
        else 
        {
            panelTooltip.SetActive(theCursorCzar.cursorInBounds);
            if (!panelTooltip.activeSelf) return;

            textTooltip.text = "Year " + ((simulationManager.simAgeTimeSteps / 2048f) * cursorX / 360f).ToString("F0");
            textTooltip.color = Color.yellow;
            panelTooltip.SetActive(true);
        } 
    }
    
    void TickObjectTooltip()
    {
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
        
        manager.isPlantHighlight = false;
        manager.isZooplanktonHighlight = false;
        manager.isVertebrateHighlight = false;
        
        float hitboxRadius = 1f;
        
        if(cameraManager.isMouseHoverAgent) 
        {
            manager.isVertebrateHighlight = true;
            textTooltip.text = "Critter #" + cameraManager.mouseHoverAgentRef.candidateRef.candidateID;
            textTooltip.color = Color.white;
        }
        else 
        {
            if(plantDist < zoopDist && plantDist < hitboxRadius) 
            {
                manager.isPlantHighlight = true;
                textTooltip.text = "Algae #" + vegetationManager.closestPlantParticleData.index;
                textTooltip.color = Color.green;
            }
            if(plantDist > zoopDist && zoopDist < hitboxRadius) 
            {
                manager.isZooplanktonHighlight = true;
                textTooltip.text = "Microbe #" + zooplanktonManager.closestAnimalParticleData.index;
                textTooltip.color = Color.yellow;
            }
        }
        
        panelTooltip.SetActive(manager.isLifeHighlight);

        //float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        //float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);  
        if (theCursorCzar.cursorInBounds) 
        {
            textTooltip.text = "TimeStep #" + (simulationManager.simAgeTimeSteps * cursorX / 360f).ToString("F0");
            textTooltip.color = Color.yellow;
            panelTooltip.SetActive(true);
        }
    }
    
    void TickBrushes()
    {
        if (curActiveTool == ToolType.Stir) 
        {  
            theCursorCzar.stirGizmoVisible = true;
            
            float isActing = 0f;
            
            if (theCursorCzar.isDraggingMouseLeft) 
            { 
                isActing = 1f;
                float mag = theCursorCzar.smoothedMouseVel.magnitude;
                float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                // * WPP: move to SimulationManager
                if(mag > 0f) 
                {
                    simulationManager.PlayerToolStirOn(theCursorCzar.curMousePositionOnWaterPlane, theCursorCzar.smoothedMouseVel * (0.25f + theRenderKing.baronVonWater.camDistNormalized * 1.2f), radiusMult);  
                }
                else 
                {
                    simulationManager.PlayerToolStirOff();
                }
            }
            else 
            {
                simulationManager.PlayerToolStirOff();                        
            }

            // var stirMin = isActing > 0.5f ? 1f : -4f;
            // theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, stirMin, 0.2f);
            
            theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft;
            theRenderKing.gizmoStirToolMat.SetFloat(STIRRING, isActing);
            theRenderKing.gizmoStirStickAMat.SetFloat(STIRRING, isActing);                    
            theRenderKing.gizmoStirStickAMat.SetFloat(RADIUS, 6.2f);
        }

        theRenderKing.isBrushing = false;
        theRenderKing.isSpiritBrushOn = false;
        theRenderKing.nutrientToolOn = false;
        isBrushAddingAgents = false;
        vegetationManager.isBrushActive = false;
        
        if (curActiveTool == ToolType.Add && panelFocus == PanelFocus.Brushes) 
        {
            // What Palette Trophic Layer is selected?
            theCursorCzar.stirGizmoVisible = true;
            simulationManager.PlayerToolStirOff();

            if (theCursorCzar.isDraggingMouse && !brushesUI.isInfluencePointsCooldown) 
            {
                if(brushesUI.toolbarInfluencePoints >= 0.05f) 
                {
                    brushesUI.ApplyCreationBrush();
                }
                else 
                {
                    Debug.Log("NOT ENOUGH MANA!");
                    // Enter Cooldown!
                    brushesUI.isInfluencePointsCooldown = true;
                }
            }
        }
    }  
    
    void SetStirVisible()
    {
        var visibility = theCursorCzar.stirGizmoVisible ? 1f : 0f;
        theRenderKing.gizmoStirToolMat.SetFloat(VISIBLE, visibility);
        theRenderKing.gizmoStirStickAMat.SetFloat(VISIBLE, visibility);
    }   
}

