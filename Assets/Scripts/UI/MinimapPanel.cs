using UnityEngine;
using UnityEngine.UI;

// WPP: moved to panel in scene, renamed
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
    
    #region Set Material

    public void Tick() {
        // WPP: set active on state change instead of per tick
        //groupMinimap.SetActive(isOpen);
        if (!isOpen) {            
            return;
        }

        uiKnowledgeMapViewerMat.SetTexture("_AltitudeTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
        uiKnowledgeMapViewerMat.SetTexture("_ResourceGridTex", simulationManager.vegetationManager.resourceGridRT1);
        uiKnowledgeMapViewerMat.SetFloat("_WaterLevel", SimulationManager._GlobalWaterLevel);

        // WPP: eliminated repetition & exposed values via nested class
        /*if (selectedTrophicSlot.kingdomID == 0) {
            // DECOMPOSERS
            textTitle.text = "DECOMPOSERS";               
            imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
            uiKnowledgeMapViewerMat.SetTexture("_MainTex", simulationManager.vegetationManager.resourceGridRT1);
            uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
            uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
            uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
            uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 2);
            uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
            uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
        }
        else if (selectedTrophicSlot.kingdomID == 1) {
            if (selectedTrophicSlot.tierID == 0) {  // ALGAE
                textTitle.text = "ALGAE";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", simulationManager.vegetationManager.resourceGridRT1);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 3);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
            else {  // PLANT PARTICLES
                textTitle.text = "PLANTS";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", simulationManager.environmentFluidManager._SourceColorRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 0f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }
        }
        else if (selectedTrophicSlot.kingdomID == 2) {
            if (selectedTrophicSlot.tierID == 0) { // ZOOPLANKTON
                textTitle.text = "MICROBES";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", simulationManager.environmentFluidManager._SourceColorRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 0f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }
            else {  // VERTEBRATES
                textTitle.text = "ANIMALS";
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", simulationManager.environmentFluidManager._SourceColorRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 3);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }

        }
        else if (selectedTrophicSlot.kingdomID == 3) {
            //panelKnowledgeInfoWorld.SetActive(true);
            if (selectedTrophicSlot.slotID == 0) {
                // WORLD 
                textTitle.text = "WORLD";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 0.7f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);  
            }
            else if (selectedTrophicSlot.slotID == 1) {
                // STONE
                textTitle.text = "STONE";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
            else if (selectedTrophicSlot.slotID == 2) {
                // PEBBLES
                textTitle.text = "PEBBLES";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 1);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
            else {
                // SAND
                textTitle.text = "SAND";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 2);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
        }
        else if (selectedTrophicSlot.kingdomID == 4) {
            //panelKnowledgeInfoWorld.SetActive(true);
            
            if (selectedTrophicSlot.slotID == 0) {
                // Minerals
                textTitle.text = "NUTRIENTS";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", simulationManager.vegetationManager.resourceGridRT1);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }
            else if (selectedTrophicSlot.slotID == 1) {
                // Water
                textTitle.text = "WATER";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", theRenderKing.baronVonWater.waterSurfaceDataRT0);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);  
            }
            else if (selectedTrophicSlot.slotID == 2) {
                // AIR
                textTitle.text = "WIND";      
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", theRenderKing.baronVonWater.waterSurfaceDataRT0);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 0.5f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);  
            }
        }*/
        
        var state = GetStateID(selectedTrophicSlot);
        SetKnowledgeMapViewer(state);
    }
    
    // * WPP: store KnowledgeMapState in the trophic slot directly and/or expose mapping
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
    
    #endregion
    
    #region Button Clicks
    
    // WPP: condensed to SetPanelOpen
    /*public void ClickButtonOpenPanel() {
        SetPanelOpen(true);
    }
    
    public void ClickButtonClosePanel() {
        SetPanelOpen(false);
    }*/
    
    public void SetPanelOpen(bool value)
    {
        isOpen = value;
        groupMinimap.SetActive(value);
    }
    
    public void SelectTrophicSlot(KnowledgeMapSO data)
    {
        selectedTrophicSlot = trophicLayers.GetSlot(data);
    }
    
    // WPP: condensed to SelectTrophicSlot
    /*
    public void ClickButtonLayerOther(int index) {
        selectedTrophicSlot = trophicLayers.kingdomOther.trophicTiersList[0].trophicSlots[index];        
        //selectedToolbarOtherLayer = index;        
    }
    
    public void ClickButtonLayerTerrain(int index) {
        selectedTrophicSlot = trophicLayers.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
    }
    
    public void ClickButtonLayerDecomposers() {
        selectedTrophicSlot = trophicLayers.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
    }
    
    public void ClickButtonLayerAlgae() {
        selectedTrophicSlot = trophicLayers.kingdomPlants.trophicTiersList[0].trophicSlots[0];
    }
    
    public void ClickButtonLayerPlants(int slotID) {
        selectedTrophicSlot = trophicLayers.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
    }
    
    public void ClickButtonLayerZooplankton() {
        selectedTrophicSlot = trophicLayers.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
    }
    
    public void ClickButtonLaterAgents(int index) {
        TrophicSlot slot = trophicLayers.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        selectedTrophicSlot = slot;
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next               
    }
    */
    
    #endregion
}
