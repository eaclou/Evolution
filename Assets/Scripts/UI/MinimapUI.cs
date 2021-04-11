﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;

    public bool isOpen = true;  
    public Text textTitle;
    public Image imageKnowledgeMapTextureViewer;
    public Material uiKnowledgeMapViewerMat;
    public GameObject groupMinimap;

    private TrophicSlot selectedTrophicSlot = new TrophicSlot();
    

    public void Tick() {
        if (!isOpen) return;

        
        uiKnowledgeMapViewerMat.SetTexture("_AltitudeTex", theRenderKing.baronVonTerrain.terrainHeightDataRT);
        uiKnowledgeMapViewerMat.SetTexture("_ResourceGridTex", simulationManager.vegetationManager.resourceGridRT1);
        uiKnowledgeMapViewerMat.SetFloat("_WaterLevel", SimulationManager._GlobalWaterLevel);

        if (selectedTrophicSlot.kingdomID == 0) {
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
        }
    }
    
    public void ClickButtonOpenPanel() {
        isOpen = true;
    }
    public void ClickButtonClosePanel() {
        isOpen = false;
    }
    public void ClickButtonLayerOther(int index) {
        Debug.Log("ClickButtonPaletteOther: " + index.ToString());
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];        
        selectedTrophicSlot = slot;
        
        //selectedToolbarOtherLayer = index;        
    }
    public void ClickButtonLayerTerrain(int index) {        
        Debug.Log("ClickButtonPaletteTerrain: " + index.ToString());

        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        selectedTrophicSlot = slot;
        
    }
    public void ClickButtonLayerDecomposers() {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        selectedTrophicSlot = slot;
        
    }
    public void ClickButtonLayerAlgae() {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        selectedTrophicSlot = slot;
        
    }
    public void ClickButtonLayerPlants(int slotID) {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
        selectedTrophicSlot = slot;
        
    }
    public void ClickButtonLayerZooplankton() {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
        selectedTrophicSlot = slot;
        
    }
    public void ClickButtonLaterAgents(int index) {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        selectedTrophicSlot = slot;
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next               
        
    }
}