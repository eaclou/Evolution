using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpiritHubUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isOpen;

    //public int selectedSlotID;
    public TrophicSlot selectedWorldSpiritSlot;
        
    public int selectedToolbarOtherLayer = 0;
    public Button buttonWorldSpiritOther0;  // Minerals
    public Button buttonWorldSpiritOther1;  // Water
    public Button buttonWorldSpiritOther2;  // Air

    public int selectedToolbarTerrainLayer = 0;
    public Button buttonWorldSpiritTerrain0;
    public Button buttonWorldSpiritTerrain1;
    public Button buttonWorldSpiritTerrain2;
    public Button buttonWorldSpiritTerrain3;
    //
    //public Button buttonWorldSpiritRemoveDecomposer;
    public Button buttonWorldSpiritDecomposers;
    //
    //public Button buttonToolbarRemovePlant;
    public Button buttonWorldSpiritAlgae;
    public Button buttonWorldSpiritPlant1;
    public Button buttonWorldSpiritPlant2;
    //
    //public Button buttonToolbarRemoveAnimal;
    public Button buttonWorldSpiritZooplankton;
    public Button buttonWorldSpiritAnimal1;
    public Button buttonWorldSpiritAnimal2;
    public Button buttonWorldSpiritAnimal3;
    public Button buttonWorldSpiritAnimal4;
    //public int selectedWorldSpiritVertebrateLayer = 0;
    public int selectedWorldSpiritVertebrateSpeciesID = 0;

	// Use this for initialization
	void Start () {
        
	}

    public void UpdateWorldSpiritHubUI() {

        TrophicLayersManager layerManager = uiManagerRef.gameManager.simulationManager.trophicLayersManager;  


        Color iconColor = Color.white;

        bool isSelectedDecomposers = false;
        bool isSelectedAlgae = false;
        bool isSelectedPlants = false;
        bool isSelectedZooplankton = false;
        bool isSelectedVertebrate0 = false;
        bool isSelectedVertebrate1 = false;
        bool isSelectedVertebrate2 = false;
        bool isSelectedVertebrate3 = false;
        bool isSelectedMinerals = false;
        bool isSelectedWater = false;
        bool isSelectedAir = false;
        bool isSelectedTerrain0 = false;
        bool isSelectedTerrain1 = false;
        bool isSelectedTerrain2 = false;
        bool isSelectedTerrain3 = false;
        if (layerManager.isSelectedTrophicSlot) {
            if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
                isSelectedDecomposers = true;
                iconColor = uiManagerRef.colorDecomposersLayer;
                //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritDecomposerIcon;
            }
            else if (layerManager.selectedTrophicSlotRef.kingdomID == 1) {
                if (layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelectedAlgae = true;
                    iconColor = uiManagerRef.colorAlgaeLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritAlgaeIcon;
                }
                else {
                    isSelectedPlants = true;
                    iconColor = uiManagerRef.colorPlantsLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritPlantIcon;
                }
            }
            else if (layerManager.selectedTrophicSlotRef.kingdomID == 2) {
                if (layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelectedZooplankton = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritZooplanktonIcon;
                }
                else {
                    iconColor = uiManagerRef.colorVertebratesLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritVertebrateIcon;

                    if (layerManager.selectedTrophicSlotRef.slotID == 0) {
                        isSelectedVertebrate0 = true;
                    }
                    else if (layerManager.selectedTrophicSlotRef.slotID == 1) {
                        isSelectedVertebrate1 = true;
                    }
                    else if (layerManager.selectedTrophicSlotRef.slotID == 2) {
                        isSelectedVertebrate2 = true;
                    }
                    else {
                        isSelectedVertebrate3 = true;
                    }

                }

            }
            else if (layerManager.selectedTrophicSlotRef.kingdomID == 3) {
                iconColor = uiManagerRef.colorTerrainLayer;
                if (layerManager.selectedTrophicSlotRef.slotID == 0) {
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritWorldIcon;
                    isSelectedTerrain0 = true;
                }
                else if (layerManager.selectedTrophicSlotRef.slotID == 1) {
                    isSelectedTerrain1 = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritStoneIcon;
                }
                else if (layerManager.selectedTrophicSlotRef.slotID == 2) {
                    isSelectedTerrain2 = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritPebblesIcon;
                }
                else if (layerManager.selectedTrophicSlotRef.slotID == 3) {
                    isSelectedTerrain3 = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritSandIcon;
                }
            }
            else if (layerManager.selectedTrophicSlotRef.kingdomID == 4) {
                if (layerManager.selectedTrophicSlotRef.slotID == 0) {
                    isSelectedMinerals = true;
                    iconColor = uiManagerRef.colorMineralLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritMineralsIcon;
                }
                else if (layerManager.selectedTrophicSlotRef.slotID == 1) {
                    isSelectedWater = true;
                    iconColor = uiManagerRef.colorWaterLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritWaterIcon;
                }
                else if (layerManager.selectedTrophicSlotRef.slotID == 2) {
                    isSelectedAir = true;
                    iconColor = uiManagerRef.colorAirLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritAirIcon;
                }
            }
        }
        else {

        }
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelectedDecomposers);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelectedAlgae);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status, isSelectedPlants);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelectedZooplankton);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelectedVertebrate0);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelectedVertebrate1);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelectedVertebrate2);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelectedVertebrate3);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritTerrain0, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0].status, isSelectedTerrain0);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritTerrain1, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[1].status, isSelectedTerrain1);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritTerrain2, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2].status, isSelectedTerrain2);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritTerrain3, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3].status, isSelectedTerrain3);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritOther0, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status, isSelectedMinerals);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritOther1, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status, isSelectedWater);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonWorldSpiritOther2, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status, isSelectedAir);

    }


    


    
    //*********************************************
    public void ClickButtonWorldSpiritHubOther(int index) {
        Debug.Log("ClickButtonPaletteOther: " + index.ToString());

        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedToolbarOtherLayer = index;

        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubTerrain(int index) {
        Debug.Log("ClickButtonPaletteTerrain: " + index.ToString());

        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedToolbarTerrainLayer = index;

        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubDecomposers() {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
    }
    public void ClickButtonWorldSpiritHubPlants(int slotID) {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
    }
    public void ClickButtonWorldSpiritHubZooplankton() {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            //ClickToolbarCreateNewSpecies();
            ClickWorldCreateNewSpecies(slot);
        }
        //curActiveTool = ToolType.None;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubAgent(int index) {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;

        selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            //ClickToolbarCreateNewSpecies();  // UNLOCKING!!!! ********************************************* need to address at some point!!!! ***************
            ClickWorldCreateNewSpecies(slot);

        }
        
        if(uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.status != TrophicSlot.SlotStatus.Unlocked) {
            //InitToolbarPortraitCritterData(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef); // ***
        }

    }

    
    public void ClickWorldCreateNewSpecies(TrophicSlot slot) {
        // questionable code, possibly un-needed:
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.CreateTrophicSlotSpecies(uiManagerRef.gameManager.simulationManager, uiManagerRef.cameraManager.curCameraFocusPivotPos, uiManagerRef.gameManager.simulationManager.simAgeTimeSteps);
                
        uiManagerRef.gameManager.theRenderKing.baronVonWater.StartCursorClick(uiManagerRef.cameraManager.curCameraFocusPivotPos);
        
        //isAnnouncementTextOn = true;

        if(uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 0) {
            uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Decomposer added!";
            uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorDecomposersLayer;
            //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        }
        else if(uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 0) {
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Algae added!";
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorAlgaeLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
            }
            else {   /// BIG PLANTS:
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of PLAN%T added!";
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorPlantsLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
            }
            
        }
        else if(uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.kingdomID == 2) { // ANIMALS
            if(uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.tierID == 1) {
                //if(createSpecies) {
                // v v v Actually creates new speciesPool here:::
                //TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
                slot.speciesName = "Vertebrate " + (slot.slotID + 1).ToString();
                uiManagerRef.gameManager.simulationManager.CreateAgentSpecies(uiManagerRef.cameraManager.curCameraFocusPivotPos);
                
                uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.slotID].linkedSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1].speciesID;

                // *** IMPORTANT::::
                int speciesIndex = slot.linkedSpeciesID;
                uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].representativeGenome;
                uiManagerRef.gameManager.simulationManager.masterGenomePool.GenerateWorldLayerVertebrateGenomeMutationOptions(slot.slotID, slot.linkedSpeciesID);

                Debug.Log("ADSF: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary.ToString());

                // duplicated code shared with clickAgentButton :(   bad 
                //
                //gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
                //gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
                //isToolbarWingOn = true;
                //selectedSpeciesID = slot.linkedSpeciesID; // update this next
                                
                selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // ???
                //InitToolbarPortraitCritterData(slot);                
                
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Vertebrate added!";
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorVertebratesLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
                
                if(slot.slotID == 0) {
                    //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "These creatures start with randomly-generated brains\n and must evolve successful behavior\nthrough survival of the fittest";
                    uiManagerRef.isAnnouncementTextOn = false;  // *** hacky...
                }
                //ClickToolButtonInspect();
                
            }
            else {
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of Zooplankton added!";
                uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorVertebratesLayer;
                //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
            }
        }

        //curToolbarWingPanelSelectID = 1;

        
        //timerAnnouncementTextCounter = 0;
        //recentlyCreatedSpecies = true;
        //recentlyCreatedSpeciesTimeStepCounter = 0;
        //UpdateSelectedSpeciesColorUI();
    }
    

}
