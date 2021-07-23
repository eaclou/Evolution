using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Handles tooltips, announcements, moving the camera, and setting brushes
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

    public bool isTooltipHover = false;
    public string tooltipString;
    
    float cursorX => theCursorCzar.GetCursorPixelCoords().x;
    Vector2 mousePositionOnWater => theCursorCzar.curMousePositionOnWaterPlane2D;
    
    [SerializeField] int announcementDuration = 640;
    [SerializeField] float hitboxRadius = 1f;

    public new bool enabled;
    public GameObject panelObserverMode;
    public DebugPanelUI debugPanelUI;       // * Not used
    public BrushesUI brushesUI;
    public GenomeViewerUI genomeViewerUI;   // * Not used
    public GameObject panelPendingClickPrompt;

    const string VISIBLE = "_IsVisible";
    const string STIRRING = "_IsStirring";
    const string RADIUS = "_Radius";
    
    int timerAnnouncementTextCounter;
    public bool isAnnouncementTextOn = false;
    public bool isBrushAddingAgents = false;
    public bool updateTerrainAltitude;
    public float terrainUpdateMagnitude;
    
    TooltipId tooltipId;
    public bool isVertebrateHighlight => tooltipId == TooltipId.Agent;
    public bool isPlantHighlight => tooltipId == TooltipId.Algae;
    public bool isMicrobeHighlight => tooltipId == TooltipId.Microbe;
        
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

        // WPP: cut middle-man reference, condensed to single function
        //watcherUI.StopFollowingAgent();
        //watcherUI.StopFollowingPlantParticle();
        //watcherUI.StopFollowingAnimalParticle();
        
        // Don't auto-follow anything if player is manually controlling camera
        cameraManager.SetFollowing(KnowledgeMapId.Undefined);
        
        cameraManager.MoveCamera(input.normalized);
    }
    
    bool tooltipActive;

    // WPP: broken into helper functions
    public void Tick()
    {
        if(vegetationManager == null) return;

        TickAnnouncement();
        TickTooltip();
        TickBrushes();
        
        theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);

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

    #region dead code - please delete
    void TickObjectTooltip()
    {
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

        //float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        //float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f); 
    }
    #endregion
    
    float plantDistance;
    float microbeDistance;
    
    void TickTooltip()
    {
        plantDistance = (vegetationManager.closestPlantParticleData.worldPos - mousePositionOnWater).magnitude;
        microbeDistance = (zooplanktonManager.closestAnimalParticlePosition2D - mousePositionOnWater).magnitude;
                                
        tooltipActive = false;
        foreach (var tooltip in tooltips)
        {
            if (!GetTooltipCondition(tooltip.id))
                continue;
            
            ActivateTooltip(tooltip);
            tooltipActive = true;
            break;
        }
        panelTooltip.SetActive(tooltipActive);
    }
    
    void ActivateTooltip(TooltipData data)
    {
        tooltipId = data.id;
        ActivateTooltip(GetTooltipString(data.id), data.color);
    }

    void ActivateTooltip(string text, Color color)
    {
        textTooltip.text = text;
        textTooltip.color = color;
        panelTooltip.SetActive(true);        
    }
    
    void TickBrushes()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
    
        if (curActiveTool == ToolType.Stir) 
        {  
            theCursorCzar.stirGizmoVisible = true;
            simulationManager.TickPlayerStirTool();

            // theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, stirMin, 0.2f);
            float brushStrength = theCursorCzar.isDraggingMouseLeft ? 1f : 0f;
            theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft;
            theRenderKing.gizmoStirToolMat.SetFloat(STIRRING, brushStrength);
            theRenderKing.gizmoStirStickAMat.SetFloat(STIRRING, brushStrength);                    
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
    
    public void EnterTooltipObject(TooltipUI tip) {
        isTooltipHover = true;
        tooltipString = tip.tooltipString;
    }
    
    public void ExitTooltipObject() {
        isTooltipHover = false;
    }
    string GetTooltipString(TooltipId id)
    {
        switch (id)
        {
            case TooltipId.CanvasElement: return tooltipString;
            case TooltipId.Year: return "Year " + ((simulationManager.simAgeTimeSteps / 2048f) * cursorX / 360f).ToString("F0");
            case TooltipId.Agent: return "Critter #" + cameraManager.mouseHoverAgentRef.candidateRef.candidateID;
            case TooltipId.Algae: return "Algae #" + vegetationManager.closestPlantParticleData.index;
            case TooltipId.Microbe: return "Microbe #" + zooplanktonManager.closestAnimalParticleData.index;
            case TooltipId.Timestep: return "TimeStep #" + (simulationManager.simAgeTimeSteps * cursorX / 360f).ToString("F0");
            default: return "";
        }
    }
    
    bool GetTooltipCondition(TooltipId id)
    {
        switch (id)
        {
            case TooltipId.CanvasElement: return isTooltipHover;
            case TooltipId.Year: return cursorInSpeciesHistoryPanel;
            case TooltipId.Agent: return cameraManager.isMouseHoverAgent;
            case TooltipId.Algae: return plantDistance < hitboxRadius && plantDistance < microbeDistance;
            case TooltipId.Microbe: return microbeDistance < hitboxRadius && microbeDistance < plantDistance;
            case TooltipId.Timestep: return cursorInSpeciesHistoryPanel;    // ERROR: same as Year
            default: return false;
        }
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
        CanvasElement,
        Year,
        Agent,
        Algae,
        Microbe,
        Timestep,
    }
    
    #endregion
}

