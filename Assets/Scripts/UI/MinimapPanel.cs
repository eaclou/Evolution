using UnityEngine;
using UnityEngine.UI;

public class MinimapPanel : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManagerRef => UIManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TrophicLayersManager trophicLayers => simulationManager.trophicLayersManager;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;

    public Text textTitle;
    public Image imageKnowledgeMapTextureViewer;
    public Material uiKnowledgeMapViewerMat;
    public GameObject groupMinimap;
    public Image imageCameraViewArea;
    [SerializeField]
    public OpenCloseButton openCloseButton;
    [SerializeField]
    public Button buttonToggleFollow;
    [SerializeField]
    TooltipUI tooltipOpenCloseButton;

    [ReadOnly]
    public bool mouseWithinPanelBounds;
    [ReadOnly]
    public Vector2 mousePosPanelCoords;

    public static int panelSizePixels => 360; // better way to do this, use build-in UI stuff to set a flag(s)

    TrophicSlot selectedTrophicSlot => trophicLayers.selectedSlot;
    public bool isOpen => openCloseButton.isOpen;
    public bool isOverOpenCloseButton = false;
    
    public void Start() {
        openCloseButton.SetHighlight(true);
        //curOverlayMode = MapOverlayModes.Microbes;
    }
    public void Tick() {
        
        var mouseInOpenCloseArea = Screen.height - Input.mousePosition.y < 64 && Screen.width - Input.mousePosition.x < 64;
        isOverOpenCloseButton = mouseInOpenCloseArea;
        openCloseButton.SetMouseEnter(mouseInOpenCloseArea);
        tooltipOpenCloseButton.tooltipString = isOpen ? "Hide Minimap Panel" : "Open Minimap Panel";
        
        mouseWithinPanelBounds = Screen.height - Input.mousePosition.y < panelSizePixels && Screen.width - Input.mousePosition.x < panelSizePixels;
        //mousePosPanelCoords = Vector2.zero;
        uiManagerRef.SetCursorInMinimapPanel(mouseWithinPanelBounds);
        
        if(mouseWithinPanelBounds) {
            mousePosPanelCoords.x = Input.mousePosition.x - (Screen.width - panelSizePixels);
            mousePosPanelCoords.y = Input.mousePosition.y - (Screen.height - panelSizePixels);
        }

        uiKnowledgeMapViewerMat.SetTexture("_AltitudeTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
        uiKnowledgeMapViewerMat.SetTexture("_ResourceGridTex", simulationManager.vegetationManager.resourceGridRT1);
        uiKnowledgeMapViewerMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        uiKnowledgeMapViewerMat.SetTexture("_FluidColorTex", fluidManager.initialDensityTex);
        uiKnowledgeMapViewerMat.SetFloat("_WaterLevel", SimulationManager._GlobalWaterLevel);
        
        SetKnowledgeMapViewer(selectedTrophicSlot.data);

        imageCameraViewArea.gameObject.SetActive(isOpen);

        if (!isOpen) {            
            return;
        }
        
        float unitConversion = 360f / 256f;
        imageCameraViewArea.transform.localPosition = new Vector3(cameraManager.curCameraFocusPivotPos.x * unitConversion, cameraManager.curCameraFocusPivotPos.y * unitConversion, 0f);
        float camAltitude = -cameraManager.cameraRef.transform.position.z;
        float startAlt = 1f;
        float endAlt = 350f;
        float zoomLevel01 = Mathf.Clamp01((camAltitude - startAlt) / (endAlt - startAlt));
        imageCameraViewArea.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, 2.5f, zoomLevel01);

        Color toggleButtonColor = new Color(0.75f, 0.35f, 0.2f);
        TooltipUI tooltip = buttonToggleFollow.GetComponent<TooltipUI>();
        tooltip.tooltipString = "Turn Autofollow ON";
        if(cameraManager.GetIsAutoFollowModeON()) {
            toggleButtonColor = new Color(0.2f, 0.75f, 0.5f);
            tooltip.tooltipString = "Turn Autofollow OFF";
        }
        buttonToggleFollow.GetComponent<Image>().color = toggleButtonColor;



    }
    

    public void ClickToggleFollow() {
        cameraManager.ToggleAutoFollow();
        Debug.Log("Autofollow is ON = " + cameraManager.GetIsAutoFollowModeON());
    }
            
    void SetKnowledgeMapViewer(TrophicLayerSO data) { SetKnowledgeMapViewer(data, GetRenderTexture(data.id)); }
    
    // * WPP: if possible, store RenderTextures in KnowledgeMapData fields
    RenderTexture GetRenderTexture(KnowledgeMapId id)
    {
        switch (id)
        {
            case KnowledgeMapId.Decomposers: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Algae: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Plants: return fluidManager._PlantsMicrobesAnimalsColorRT;
            case KnowledgeMapId.Microbes: return fluidManager._PlantsMicrobesAnimalsColorRT;
            case KnowledgeMapId.Animals: return fluidManager._PlantsMicrobesAnimalsColorRT;
            case KnowledgeMapId.World: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Stone: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Pebbles: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Sand: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Nutrients: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Water: return fluidManager._VelocityPressureDivergenceMain;
            case KnowledgeMapId.Wind: return theRenderKing.baronVonWater.waterSurfaceDataRT0;
            case KnowledgeMapId.Detritus: return simulationManager.vegetationManager.resourceGridRT1;
            default: return null;
        }        
    }
    
    void SetKnowledgeMapViewer(TrophicLayerSO data, RenderTexture renderTexture)
    {
        textTitle.text = data.title;// "WORLD MAP"; // data.title;      
        imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
        uiKnowledgeMapViewerMat.SetTexture("_MainTex", renderTexture);
        uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
        uiKnowledgeMapViewerMat.SetFloat("_Amplitude", data.amplitude);
        uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); 
        uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", data.channelSoloIndex);
        uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", data.isChannelSolo);
        uiKnowledgeMapViewerMat.SetFloat("_Gamma", data.gamma);
        uiKnowledgeMapViewerMat.SetFloat("_Offset", data.offset);
        uiKnowledgeMapViewerMat.SetPass(0);
    }
    
    public void SelectTrophicSlot(TrophicLayerSO data) {
        imageKnowledgeMapTextureViewer.gameObject.SetActive(false);
        trophicLayers.SetSlot(data);
        SetKnowledgeMapViewer(data);
        imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
    }
}
