using UnityEngine;
using UnityEngine.UI;

public class MinimapPanel : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    TrophicLayersManager trophicLayers => simulationManager.trophicLayersManager;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    Lookup lookup => Lookup.instance;
    

    public bool isOpen = true;  
    public Text textTitle;
    public Image imageKnowledgeMapTextureViewer;
    public Material uiKnowledgeMapViewerMat;
    public GameObject groupMinimap;

    private TrophicSlot selectedTrophicSlot = new TrophicSlot();
    
    public void Tick() {
        if (!isOpen) {            
            return;
        }

        uiKnowledgeMapViewerMat.SetTexture("_AltitudeTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
        uiKnowledgeMapViewerMat.SetTexture("_ResourceGridTex", simulationManager.vegetationManager.resourceGridRT1);
        uiKnowledgeMapViewerMat.SetFloat("_WaterLevel", SimulationManager._GlobalWaterLevel);
        
        var state = GetStateID(selectedTrophicSlot);
        SetKnowledgeMapViewer(state);
    }
    
    KnowledgeMapId GetStateID(TrophicSlot slot)
    {
        switch (slot.kingdomID)
        {
            case 0: return KnowledgeMapId.Decomposers;
            case 1: return slot.tierID == 0 ? KnowledgeMapId.Algae : KnowledgeMapId.Plants;
            case 2: return slot.tierID == 0 ? KnowledgeMapId.Microbes : KnowledgeMapId.Animals;
            case 3: 
                switch (slot.slotID)
                {
                    case 0: return KnowledgeMapId.World;
                    case 1: return KnowledgeMapId.Stone;
                    case 2: return KnowledgeMapId.Pebbles;
                    default: return KnowledgeMapId.Sand;
                }
            case 4: 
                switch (slot.slotID)
                {
                    case 0: return KnowledgeMapId.Nutrients;
                    case 1: return KnowledgeMapId.Water;
                    case 2: return KnowledgeMapId.Wind;
                    default: return KnowledgeMapId.Undefined;
                }
            default: return KnowledgeMapId.Undefined;
        }
    }
    
    void SetKnowledgeMapViewer(KnowledgeMapId id) { SetKnowledgeMapViewer(id, GetRenderTexture(id)); }
    
    // * WPP: if possible, store RenderTextures in KnowledgeMapData fields
    RenderTexture GetRenderTexture(KnowledgeMapId id)
    {
        switch (id)
        {
            case KnowledgeMapId.Decomposers: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Algae: return simulationManager.vegetationManager.resourceGridRT1;
            case KnowledgeMapId.Plants: return simulationManager.environmentFluidManager._SourceColorRT;
            case KnowledgeMapId.Microbes: return simulationManager.environmentFluidManager._SourceColorRT;
            case KnowledgeMapId.Animals: return simulationManager.environmentFluidManager._SourceColorRT;
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
    
    void SetKnowledgeMapViewer(KnowledgeMapId id, RenderTexture renderTexture)
    {
        var data = lookup.GetKnowledgeMapData(id);
    
        textTitle.text = data.title;      
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
    
    public void SelectTrophicSlot(KnowledgeMapSO data)
    {
        selectedTrophicSlot = trophicLayers.GetSlot(data);
    }
}
