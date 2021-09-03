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
    public TooltipUI tooltip;
    //public string tooltipString;
    
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

        // Don't auto-follow anything if player is manually controlling camera
        cameraManager.SetFollowing(KnowledgeMapId.Undefined);
        
        cameraManager.MoveCamera(input.normalized);
    }
    
    bool tooltipActive;

    public void Tick()
    {
        if(vegetationManager == null) return;

        TickAnnouncement();
        TickTooltip();
        TickBrushes();
        
        theRenderKing.baronVonTerrain.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);

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
            //theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft;
            theRenderKing.gizmoStirToolMat.SetFloat(STIRRING, brushStrength);
            theRenderKing.gizmoStirStickAMat.SetFloat(STIRRING, brushStrength);                    
            theRenderKing.gizmoStirStickAMat.SetFloat(RADIUS, 6.2f);
        }

        theRenderKing.isBrushing = false;
        theRenderKing.isSpiritBrushOn = false;
        //theRenderKing.nutrientToolOn = false;
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
        tooltip = tip;
    }
    
    public void ExitTooltipObject() {
        isTooltipHover = false;
    }
    
    string GetTooltipString(TooltipId id)
    {
        switch (id)
        {
            case TooltipId.CanvasElement: { //*** EAC Moving the logic to CreaturePanelUI to update Tooltip text for matching neurons
                //if (cameraManager.targetAgent) {
                //    return "OutComm[" + tooltip.elementID.ToString() + "] " + cameraManager.targetAgent.communicationModule.outComm0[0];
                //}
                //else {
                    return tooltip.tooltipString;
                //}
            }
            case TooltipId.Time: return "TIME: " + ((float)simulationManager.simAgeTimeSteps * cursorX / 360f).ToString("F0");
            case TooltipId.Agent: return "Critter " + cameraManager.mouseHoverAgentRef.candidateRef.name + "\nBiomass: " + cameraManager.mouseHoverAgentRef.currentBiomass.ToString("F2");
            case TooltipId.Algae: return "Algae #" + vegetationManager.closestPlantParticleData.index;
            case TooltipId.Microbe: return "Microbe #" + zooplanktonManager.closestAnimalParticleData.index;
            case TooltipId.Sensor: return "Sensor #" + (simulationManager.simAgeTimeSteps * cursorX / 360f).ToString("F0");
            case TooltipId.Specialization: return "Specializations"; //***EAC EGGSACK???
            case TooltipId.Status: return "STATUS";
            default: return "";
        }
    }
    
    bool GetTooltipCondition(TooltipId id)
    {
        switch (id)
        {
            case TooltipId.CanvasElement: return isTooltipHover;
            case TooltipId.Time: return cursorInSpeciesHistoryPanel;
            case TooltipId.Agent: return cameraManager.isMouseHoverAgent;
            case TooltipId.Algae: return plantDistance < hitboxRadius && plantDistance < microbeDistance;
            case TooltipId.Microbe: return microbeDistance < hitboxRadius && microbeDistance < plantDistance;
            case TooltipId.Sensor: return cursorInSpeciesHistoryPanel;    // ERROR: same as Year
            case TooltipId.Specialization: return isTooltipHover;
            case TooltipId.Status: return isTooltipHover;
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
        Time,
        Agent,
        Algae,
        Microbe,
        Sensor,
        Specialization,
        Status
    }
    
    #endregion
}

