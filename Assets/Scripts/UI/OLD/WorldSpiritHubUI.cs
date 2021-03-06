﻿using UnityEngine;
using UnityEngine.UI;

public class WorldSpiritHubUI : MonoBehaviour {
    UIManager uiManagerRef => UIManager.instance;
    //public PanelPendingClickPromptUI panelPendingClickPrompt;

    public bool isUnlocked;
    public bool isOpen;
    public bool isDim;
    
    //public int selectedSlotID;
    public TrophicSlot selectedWorldSpiritSlot;

    public GameObject panelWorldHubMain;
    public GameObject panelWorldHubExpand;
    public Image imageSelectedTargetLayer;

    public Animator animatorTest01;
    //private int animTestTriggerHash = Animator.StringToHash("PlayAnim");
    public Animator animatorWorldHubUI;

    public Button buttonMutationLink;
    public Button buttonBrushesLink;
    public Button buttonKnowledgeLink;
    public Button buttonWatcherLink;

    public Text textSelectedEssenceName;
    public Text textSelectedEssenceDescription;
    public Image imageSelectedEssence;
    public Image imageColorBar;
    public Image imageBitBG;

    public Image imageBitHub;
    public Image imageBitTerrain;
    public Image imageBitWater;
    public Image imageBitAir;
    public Image imageBitMinerals;
    public Image imageBitDecomposers;
    public Image imageBitPlants;
    public Image imageBitAnimals;
    public Image imageBitInfo;
    public Image imageBitMutation;
    public Image imageBitKnowledge;
    public Image imageBitWatcher;
    public Image imageBitBrushes;

    public Text textTotalMass;
        
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

    public Color curIconColor = Color.white;
    public Sprite curIconSprite = null;

    public Text textCurrencies;
    public int tierA = 1;
    public int currencyB = 0;
    
    SimulationManager simulationManager => SimulationManager.instance;
    TrophicLayersManager trophicLayersManager => simulationManager.trophicLayersManager;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    CameraManager cameraManager => CameraManager.instance;


    // Use this for initialization
	void Start () {
        isUnlocked = true; 
        //isFocused = true;
	}

    public void PlayBigBangSpawnAnim() {
        Debug.Log("Play Anim!!!!!!");
        //animatorTest01.SetTrigger(animTestTriggerHash);
    }

    private void UpdateUI() {
        /*
        bool isSelectedDecomposers = false;
        bool isSelectedAlgae = false;
        bool isSelectedPlants0 = false;
        bool isSelectedPlants1 = false;
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
        
        if (selectedWorldSpiritSlot.kingdomID == 0) {
            isSelectedDecomposers = true;
            curIconColor = colorDecomposersLayer;
            curIconSprite = spiritDecomposerIcon;
            //essenceDescriptionStr = "Decomposers break down the old so that new life can grow.";
            textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalDecomposers.ToString("F1");
        }
        else if (selectedWorldSpiritSlot.kingdomID == 1) {
            if (selectedWorldSpiritSlot.tierID == 0) {
                isSelectedAlgae = true;
                curIconColor = colorAlgaeLayer;
                curIconSprite = spiritAlgaeIcon;
                //essenceDescriptionStr = "Algae needs light and nutrients to grow.";
                textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalAlgaeReservoir.ToString("F1");
            }
            else {
                if(selectedWorldSpiritSlot.slotID == 0) {
                    isSelectedPlants0 = true;
                    curIconColor = colorPlantsLayer;
                    curIconSprite = spiritPlantIcon;
                    //essenceDescriptionStr = "Floating Plants that are a foodsource for Vertebrates";
                    textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalPlantParticles.ToString("F1");
                }
                else {
                    isSelectedPlants1 = true;
                    curIconColor = colorPlantsLayer;
                    curIconSprite = spiritPlantIcon;
                    //essenceDescriptionStr = "Big Plants?";
                    //textTotalMass.text = "Biomass: " + uiManagerRef.gameManager.simulationManager
                }
                
            }
        }
        else if (selectedWorldSpiritSlot.kingdomID == 2) {
            if (selectedWorldSpiritSlot.tierID == 0) {
                isSelectedZooplankton = true;
                curIconColor = colorZooplanktonLayer;
                curIconSprite = spiritZooplanktonIcon;
                //essenceDescriptionStr = "Tiny Organisms that feed on Algae";
                textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalAnimalParticles.ToString("F1");
            }
            else {
                curIconColor = colorVertebratesLayer;
                curIconSprite = spiritVertebrateIcon;
                //essenceDescriptionStr = "Animals that can feed on Plants, Zooplankton, or even other Vertebrates.";
                textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalAgentBiomass.ToString("F1");

                if (selectedWorldSpiritSlot.slotID == 0) {
                    isSelectedVertebrate0 = true;
                }
                else if (selectedWorldSpiritSlot.slotID == 1) {
                    isSelectedVertebrate1 = true;
                }
                else if (selectedWorldSpiritSlot.slotID == 2) {
                    isSelectedVertebrate2 = true;
                }
                else {
                    isSelectedVertebrate3 = true;
                }

            }

        }
        else if (selectedWorldSpiritSlot.kingdomID == 3) {
                
            if (selectedWorldSpiritSlot.slotID == 0) {
                curIconSprite = spiritWorldIcon;
                isSelectedTerrain0 = true;
                curIconColor = colorWorldLayer;
                //essenceDescriptionStr = "The World Spirit provides the spark for a new universe";
                textTotalMass.text = "Area: ";
            }
            else if (selectedWorldSpiritSlot.slotID == 1) {
                isSelectedTerrain1 = true;
                curIconSprite = spiritStoneIcon;
                curIconColor = colorTerrainLayer;
                //essenceDescriptionStr = "Stone Spirits are some of the oldest known";
                textTotalMass.text = "Mass: ";
            }
            else if (selectedWorldSpiritSlot.slotID == 2) {
                isSelectedTerrain2 = true;
                curIconSprite = spiritPebblesIcon;
                curIconColor = colorTerrainLayer;
               // essenceDescriptionStr = "Pebble Spirits are usually found in rivers and streams";
                textTotalMass.text = "Mass: ";
            }
            else if (selectedWorldSpiritSlot.slotID == 3) {
                isSelectedTerrain3 = true;
                curIconSprite = spiritSandIcon;
                curIconColor = colorTerrainLayer;
                //essenceDescriptionStr = "Sand Spirits";
                textTotalMass.text = "Mass: ";
            }
        }
        else if (selectedWorldSpiritSlot.kingdomID == 4) {
            if (selectedWorldSpiritSlot.slotID == 0) {
                isSelectedMinerals = true;
                curIconColor = colorMineralLayer;
                curIconSprite = spiritMineralsIcon;
                //essenceDescriptionStr = "Mineral Spirits infuse nutrients into the earth.";
                textTotalMass.text = "Mass: ";
            }
            else if (selectedWorldSpiritSlot.slotID == 1) {
                isSelectedWater = true;
                curIconColor = colorWaterLayer;
                curIconSprite = spiritWaterIcon;
                //essenceDescriptionStr = "Water Spirits";
                textTotalMass.text = "Level: " + SimulationManager._GlobalWaterLevel;
            }
            else if (selectedWorldSpiritSlot.slotID == 2) {
                isSelectedAir = true;
                curIconColor = colorAirLayer;
                curIconSprite = spiritAirIcon;
                //essenceDescriptionStr = "Air Spirits";
                textTotalMass.text = "Speed: " + simulationManager.environmentFluidManager.curTierWaterCurrents.ToString();
            }
        }
        */
        isDim = false;
        //imageBitInfo.gameObject.SetActive(isDim);
        textSelectedEssenceName.gameObject.SetActive(!isDim);
        textSelectedEssenceDescription.gameObject.SetActive(!isDim);
        //buttonBrushesLink.gameObject.SetActive(!isDim);
        //buttonKnowledgeLink.gameObject.SetActive(!isDim);
        //buttonMutationLink.gameObject.SetActive(!isDim);
        /*
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelectedDecomposers);

        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelectedAlgae);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status, isSelectedPlants0);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritPlant2, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[1].status, isSelectedPlants1);

        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelectedZooplankton);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelectedVertebrate0);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelectedVertebrate1);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelectedVertebrate2);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelectedVertebrate3);

        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritTerrain0, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0].status, isSelectedTerrain0);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritTerrain1, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[1].status, isSelectedTerrain1);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritTerrain2, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2].status, isSelectedTerrain2);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritTerrain3, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3].status, isSelectedTerrain3);

        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritOther0, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status, isSelectedMinerals);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritOther1, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status, isSelectedWater);
        uiManagerRef.SetToolbarButtonStateUI(isDim, ref buttonWorldSpiritOther2, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status, isSelectedAir);
        */
        //imageSelectedTargetLayer.sprite = curIconSprite;
        //imageSelectedTargetLayer.color = curIconColor;

        textSelectedEssenceName.text = selectedWorldSpiritSlot.speciesName;
        textSelectedEssenceName.color = curIconColor;
        textSelectedEssenceDescription.text = "text here?";// essenceDescriptionStr;
        imageSelectedEssence.color = curIconColor * 1.35f;
        imageSelectedEssence.sprite = curIconSprite;
        imageColorBar.color = curIconColor;
        //imageBitBG.color = curIconColor;
    }
    
    public void UpdateWorldSpiritHubUI() {
        animatorWorldHubUI.SetBool("_IsOpen", isOpen);

        bool animFinished = animatorWorldHubUI.GetBool("_AnimFinished");
        if(animFinished) {
            //Debug.Log("UpdateWorldSpiritHubUI: animFinished " + animFinished.ToString());
            //animatorWorldHubUI.SetBool("_AnimFinished", false);
        }
        imageBitInfo.gameObject.SetActive(animFinished); // Testing anim-driven approach
        panelWorldHubExpand.SetActive(true); // isOpen);
        //if(isOpen) {
            UpdateUI();
        //}
        //else {
            //panelWorldHubExpand.SetActive(false);
        //}
    }

    public void ClickMinimizePanel() {
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        isOpen = false;
        animatorWorldHubUI.SetBool("_IsOpen", false);
        //panelWorldHubExpand.SetActive(false);
        Debug.Log("ClickToolButton) " + isOpen);
    }
    
    public void SetTargetFromBrushesUI() {
        selectedWorldSpiritSlot = uiManagerRef.brushesUI.selectedEssenceSlot;
        //uiManagerRef.panelFocus = PanelFocus.WorldHub;
        isOpen = true;
        //animatorWorldHubUI.SetBool("_IsOpen", true);
        //isPaletteOpen = true;
    }
    
    public void SetTargetFromWatcherUI() {
        //selectedWorldSpiritSlot = uiManagerRef.watcherUI.watcherSelectedTrophicSlotRef;
        //uiManagerRef.panelFocus = PanelFocus.WorldHub;
        isOpen = true;
        //animatorWorldHubUI.SetBool("_IsOpen", true);
    }
    
    public void OpenWorldTreeSelect() {
        isOpen = true;
        //uiManagerRef.panelFocus = PanelFocus.WorldHub;
        //animatorWorldHubUI.SetBool("_IsOpen", true);
        //Animation animTerrainShake = imageBitTerrain.GetComponent<Animation>();
        //animTerrainShake.StartPlayback();
        //animTerrainShake.Play();

        Debug.Log("OpenWorldTreeSelect " + isOpen);
    }
    
    //*********************************************
    public void ClickButtonWorldSpiritHubOther(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        //trophicLayersManager.isSelectedTrophicSlot = true;
        selectedWorldSpiritSlot = slot;
        
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedToolbarOtherLayer = data.layerIndex;
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    
    public void ClickButtonWorldSpiritHubTerrain(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedWorldSpiritSlot = slot;
        
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedToolbarTerrainLayer = data.layerIndex;
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    
    public void ClickButtonWorldSpiritHubDecomposers(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);

        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    
    // Not interactable if locked
    public void ClickButtonWorldSpiritHubAlgae(TrophicLayerSO data) {  
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }
    
    public void ClickButtonWorldSpiritHubPlants(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }
    
    public void ClickButtonWorldSpiritHubZooplankton(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlotStatus.Unlocked) {
            //ClickToolbarCreateNewSpecies();
            ClickWorldCreateNewSpecies(slot);
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //curActiveTool = ToolType.None;
        //isBrushSelected = false;
    }
    
    public void ClickButtonWorldSpiritHubAgent(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedWorldSpiritSlot = slot;
        //isToolbarDetailPanelOn = true;

        selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        if(slot.status == TrophicSlotStatus.Unlocked) {
            //ClickToolbarCreateNewSpecies();  // UNLOCKING!!!! *** need to address at some point!!!! ***
            ClickWorldCreateNewSpecies(slot);
        }
        
        if(selectedWorldSpiritSlot.status != TrophicSlotStatus.Unlocked) {
            //InitToolbarPortraitCritterData(trophicLayersManager.selectedTrophicSlotRef); // ***
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }

    public void ClickWorldCreateNewSpecies(TrophicSlot slot) {
        // questionable code, possibly un-needed:
        trophicLayersManager.CreateTrophicSlotSpecies(slot, cameraManager.curCameraFocusPivotPos, simulationManager.simAgeTimeSteps);
                
        theRenderKing.baronVonWater.StartCursorClick(cameraManager.curCameraFocusPivotPos);
        
        //isAnnouncementTextOn = true;

        if (slot.kingdomID == KingdomId.Decomposers) {
            //panelPendingClickPrompt.Narrate("A new species of D3composer added!", colorDecomposersLayer);
            //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of DEcomposer added!";
            //uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorDecomposersLayer;
            //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        }
        /*else if (slot.kingdomID == KingdomId.Plants) {
            if (slot.tierID == 0) {
                //panelPendingClickPrompt.Narrate("A new species of Algae added!", colorAlgaeLayer);                
            }
            else {   /// BIG PLANTS:
                //panelPendingClickPrompt.Narrate("A new species of PLANT added!", colorPlantsLayer);               
            }
            
        }*/
        else if(slot.id == KnowledgeMapId.Animals) { 
            //if(createSpecies) {
            // v v v Actually creates new speciesPool here:::
            //TrophicSlot slot = trophicLayersManager.selectedTrophicSlotRef;
            slot.speciesName = "Vertebrate " + (slot.slotID + 1);
                        
            selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // ???
            //InitToolbarPortraitCritterData(slot);                
            
            if(slot.slotID == 0) {
                //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "These creatures start with randomly-generated brains\n and must evolve successful behavior\nthrough survival of the fittest";
                uiManagerRef.isAnnouncementTextOn = false;  // *** hacky...
            }
        }
        else if (slot.id == KnowledgeMapId.Microbes) {
            //panelPendingClickPrompt.Narrate("A new species of Zooplankton added!", colorVertebratesLayer);
        }

        //curToolbarWingPanelSelectID = 1;
        //timerAnnouncementTextCounter = 0;
        //recentlyCreatedSpecies = true;
        //recentlyCreatedSpeciesTimeStepCounter = 0;
        //UpdateSelectedSpeciesColorUI();
    }
}
