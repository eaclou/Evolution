using UnityEngine;
using UnityEngine.UI;

public class MinimapPanel : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TrophicLayersManager trophicLayers => simulationManager.trophicLayersManager;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    //Lookup lookup => Lookup.instance;
    

    public bool isOpen = true;  
    public Text textTitle;
    public Image imageKnowledgeMapTextureViewer;
    public Material uiKnowledgeMapViewerMat;
    public GameObject groupMinimap;
    public Image imageCameraViewArea;

    private TrophicSlot selectedTrophicSlot => trophicLayers.selectedSlot;
    
    public void Tick() {
        if (!isOpen) {            
            return;
        }

        uiKnowledgeMapViewerMat.SetTexture("_AltitudeTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
        uiKnowledgeMapViewerMat.SetTexture("_ResourceGridTex", simulationManager.vegetationManager.resourceGridRT1);
        uiKnowledgeMapViewerMat.SetFloat("_WaterLevel", SimulationManager._GlobalWaterLevel);
        
        //var state = GetStateID(selectedTrophicSlot);
        //SetKnowledgeMapViewer(state);
        SetKnowledgeMapViewer(selectedTrophicSlot.data);

        float unitConversion = 360f / 256f;
        imageCameraViewArea.transform.localPosition = new Vector3(cameraManager.curCameraFocusPivotPos.x * unitConversion, cameraManager.curCameraFocusPivotPos.y * unitConversion, 0f);
        float camAltitude = -cameraManager.cameraRef.transform.position.z;
        float startAlt = 1f;
        float endAlt = 350f;
        float zoomLevel01 = Mathf.Clamp01((camAltitude - startAlt) / (endAlt - startAlt));
        imageCameraViewArea.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, 2.5f, zoomLevel01);
    }
        
    void SetKnowledgeMapViewer(TrophicLayerSO data) { SetKnowledgeMapViewer(data, GetRenderTexture(data.id)); }
    
    // * WPP: if possible, store RenderTextures in KnowledgeMapData fields
    RenderTexture GetRenderTexture(KnowledgeMapId id)
    {
        switch (id)
        {
            case KnowledgeMapId.Decomposers: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Algae: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Plants: return fluidManager._SourceColorRT;
            case KnowledgeMapId.Microbes: return fluidManager._SourceColorRT;
            case KnowledgeMapId.Animals: return fluidManager._SourceColorRT;
            case KnowledgeMapId.World: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Stone: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Pebbles: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Sand: return theRenderKing.baronVonTerrain.terrainHeightDataRT;
            case KnowledgeMapId.Nutrients: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Water: return theRenderKing.baronVonWater.waterSurfaceDataRT0;
            case KnowledgeMapId.Wind: return theRenderKing.baronVonWater.waterSurfaceDataRT0;
            case KnowledgeMapId.Detritus: return simulationManager.vegetationManager.resourceGridRT1;
            default: return null;
        }        
    }
    
    void SetKnowledgeMapViewer(TrophicLayerSO data, RenderTexture renderTexture)
    {
        //var data = lookup.GetKnowledgeMapData(id);

        textTitle.text = "WORLD MAP"; // data.title;      
        imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
        uiKnowledgeMapViewerMat.SetTexture("_MainTex", renderTexture);
        uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
        uiKnowledgeMapViewerMat.SetFloat("_Amplitude", data.amplitude);
        uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); 
        uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", data.channelSoloIndex);
        uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", data.isChannelSolo);
        uiKnowledgeMapViewerMat.SetFloat("_Gamma", data.gamma);         
    }
    
    public void SetPanelOpen(bool value)
    {
        isOpen = value;
        groupMinimap.SetActive(value);
    }
    
    public void SelectTrophicSlot(TrophicLayerSO data) { trophicLayers.SetSlot(data); }
}
