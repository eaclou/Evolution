using System;
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
    Vector2 mousePositionOnWater => theCursorCzar.curMousePositionOnWaterPlane2D;
    
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
    
    public bool cursorInSpeciesHistoryPanel;
    public void SetCursorInSpeciesHistoryPanel(bool value) { cursorInSpeciesHistoryPanel = value; }

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
            ActivateTooltip(TooltipId.Genome); 
        else if (cursorInSpeciesHistoryPanel)
            ActivateTooltip(TooltipId.Year);
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
        float plantDistance = (vegetationManager.closestPlantParticleData.worldPos - mousePositionOnWater).magnitude;
        float microbeDistance = (zooplanktonManager.closestAnimalParticlePosition2D - mousePositionOnWater).magnitude;
        
        if (cameraManager.isMouseHoverAgent) 
        {
            manager.isVertebrateHighlight = true;
            ActivateTooltip(TooltipId.Agent);
        }
        else if (plantDistance < hitboxRadius || microbeDistance < hitboxRadius)
        {
            if (plantDistance < microbeDistance) 
            {
                manager.isPlantHighlight = true;
                ActivateTooltip(TooltipId.Algae);
            }
            else
            {
                manager.isZooplanktonHighlight = true;
                ActivateTooltip(TooltipId.Microbe); 
            }
        }
        else
            panelTooltip.SetActive(false);

        //float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        //float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);  
        if (cursorInSpeciesHistoryPanel)
            ActivateTooltip(TooltipId.Timestep); 
    }

    void ActivateTooltip(TooltipId id)
    {
        var data = GetTooltipData(id);
        ActivateTooltip(GetTooltipString(id), data.color);
    }
    
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
    
    #region Tooltip Data
    
    [SerializeField] TooltipData[] tooltips;
    
    string GetTooltipString(TooltipId id)
    {
        switch (id)
        {
            case TooltipId.Genome: return genomeViewerUI.tooltipString;
            case TooltipId.Year: return "Year " + ((simulationManager.simAgeTimeSteps / 2048f) * cursorX / 360f).ToString("F0");
            case TooltipId.Agent: return "Critter #" + cameraManager.mouseHoverAgentRef.candidateRef.candidateID;
            case TooltipId.Algae: return "Algae #" + vegetationManager.closestPlantParticleData.index;
            case TooltipId.Microbe: return "Microbe #" + zooplanktonManager.closestAnimalParticleData.index;
            case TooltipId.Timestep: return "TimeStep #" + (simulationManager.simAgeTimeSteps * cursorX / 360f).ToString("F0");
            default: return "";
        }
    }
    
    TooltipData GetTooltipData(TooltipId id)
    {
        foreach (var tooltip in tooltips)
            if (tooltip.id == id)
                return tooltip;
                
        Debug.LogError(id + " not found");
        return tooltips[0];
    }
    
    // WPP: colors exposed in editor, opens door to further variation as needed
    [Serializable]
    public struct TooltipData
    {
        public TooltipId id;
        public Color color;
    }
    
    public enum TooltipId
    {
        Genome,
        Year,
        Agent,
        Algae,
        Microbe,
        Timestep,
    }
    
    #endregion
}

