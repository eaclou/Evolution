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
        
    ZooplanktonManager zooplanktonManager => simulationManager.zooplanktonManager;
    VegetationManager vegetationManager => simulationManager.vegetationManager;
    ToolType curActiveTool => manager.curActiveTool;
        
    float cursorX => theCursorCzar.GetCursorPixelCoords().x;
    Vector2 mousePositionOnWater => theCursorCzar.curMousePositionOnWaterPlane2D;
    
    [SerializeField] int announcementDuration = 640;    

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

        // Don't auto-follow anything if player is manually controlling camera
        cameraManager.SetFollowing(KnowledgeMapId.Undefined);
        
        cameraManager.MoveCamera(input.normalized);
    }
    
    

    public void Tick()
    {
        if (vegetationManager == null) return;

        TickAnnouncement();
        
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
    
    
    
}

