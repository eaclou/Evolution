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
            TickUITooltip();
        else 
        {
            TickObjectTooltip();
            TickBrushes();
        }
        
        // WPP: branches have identical result - error?
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
        if (genomeViewerUI.isTooltipHover)
            ActivateTooltip(genomeViewerUI.tooltipString, Color.cyan);
        else if (theCursorCzar.cursorInTopLeftWindow)
            ActivateTooltip("Year " + ((simulationManager.simAgeTimeSteps / 2048f) * cursorX / 360f).ToString("F0"), Color.yellow);
        else
            panelTooltip.SetActive(false);
    }
    
    void TickObjectTooltip()
    {
        #region dead code - please delete
        //int selectedPlantID = vegetationManager.selectedPlantParticleIndex;
        //int closestPlantID = vegetationManager.closestPlantParticleData.index;

        //int selectedZoopID = zooplanktonManager.selectedAnimalParticleIndex;
        //int closestZoopID = zooplanktonManager.closestAnimalParticleData.index;
        
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
        #endregion
        
        manager.isPlantHighlight = false;
        manager.isZooplanktonHighlight = false;
        manager.isVertebrateHighlight = false;
        
        float hitboxRadius = 1f;
        float plantDistance = (vegetationManager.closestPlantParticleData.worldPos - theCursorCzar.curMousePositionOnWaterPlane2D).magnitude;
        float microbeDistance = (zooplanktonManager.closestAnimalParticlePosition2D - theCursorCzar.curMousePositionOnWaterPlane2D).magnitude;
        
        if (cameraManager.isMouseHoverAgent) 
        {
            manager.isVertebrateHighlight = true;
            ActivateTooltip("Critter #" + cameraManager.mouseHoverAgentRef.candidateRef.candidateID, Color.white);
        }
        else if (plantDistance < hitboxRadius || microbeDistance < hitboxRadius)
        {
            if (plantDistance < microbeDistance) 
            {
                manager.isPlantHighlight = true;
                ActivateTooltip("Algae #" + vegetationManager.closestPlantParticleData.index, Color.green);
            }
            else
            {
                manager.isZooplanktonHighlight = true;
                ActivateTooltip("Microbe #" + zooplanktonManager.closestAnimalParticleData.index, Color.yellow);
            }
        }
        else
            panelTooltip.SetActive(false);

        //float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        //float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);  
        if (theCursorCzar.cursorInTopLeftWindow)
            ActivateTooltip("TimeStep #" + (simulationManager.simAgeTimeSteps * cursorX / 360f).ToString("F0"), Color.yellow);
    }
    
    // WPP - TBD:
    // string GetTooltipText(TooltipTypeEnum) { }
    // TooltipData GetTooltipData(TooltipTypeEnum) { }
    // struct TooltipData { TooltipTypeEnum, Color }
    // void ActivateTooltip(TooltipTypeEnum) { }
    
    void ActivateTooltip(string text, Color color)
    {
        textTooltip.text = text;
        textTooltip.color = color;
        panelTooltip.SetActive(true);        
    }
    
    void TickBrushes()
    {
        if (curActiveTool == ToolType.Stir) 
        {  
            theCursorCzar.stirGizmoVisible = true;
            simulationManager.TickPlayerStirTool();

            // var stirMin = isActing > 0.5f ? 1f : -4f;
            // theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, stirMin, 0.2f);
            float isActing = theCursorCzar.isDraggingMouseLeft ? 1f : 0f;
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
                brushesUI.TickCreationBrush();
        }
    }  
    
    void SetStirVisible()
    {
        var visibility = theCursorCzar.stirGizmoVisible ? 1f : 0f;
        theRenderKing.gizmoStirToolMat.SetFloat(VISIBLE, visibility);
        theRenderKing.gizmoStirStickAMat.SetFloat(VISIBLE, visibility);
    }   
}

