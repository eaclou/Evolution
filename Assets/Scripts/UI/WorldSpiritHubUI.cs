using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpiritHubUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;
    public bool isDim;
    
    //public int selectedSlotID;
    public TrophicSlot selectedWorldSpiritSlot;

    public GameObject panelWorldHubMain;
    public GameObject panelWorldHubExpand;
    public Image imageSelectedTargetLayer;

    public Animator animatorTest01;
    private int animTestTriggerHash = Animator.StringToHash("PlayAnim");
    public Animator animatorWorldHubUI;


    //public Button buttonMutationLink;
    public Button buttonBrushesLink;
    //public Button buttonKnowledgeLink;

    public Text textSelectedEssenceName;
    public Text textSelectedEssenceDescription;
    public Image imageSelectedEssence;
    public Image imageColorBar;

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
    public Image imageBitBrushes;
        
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



	// Use this for initialization
	void Start () {
        isUnlocked = true; // false;
        //isFocused = true;
	}

    public void PlayBigBangSpawnAnim() {
        Debug.Log("Play Anim!!!!!!");

        //animatorTest01.SetTrigger(animTestTriggerHash);
    }

    private void UpdateUI() {
        TrophicLayersManager layerManager = uiManagerRef.gameManager.simulationManager.trophicLayersManager;

        bool showMinerals = layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.On;
        imageBitMinerals.gameObject.SetActive(true);
        
        bool showAir = layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status == TrophicSlot.SlotStatus.On;
        imageBitAir.gameObject.SetActive(true);
        
        bool showDecomposers = layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.On;
        imageBitDecomposers.gameObject.SetActive(true);

        bool showPlants = layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.On;
        imageBitPlants.gameObject.SetActive(true);

        bool showAnimals = layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.On;
        imageBitAnimals.gameObject.SetActive(true);
        
        //buttonKnowledgeLink.interactable = true;
        //buttonMutationLink.interactable = false;3
        
        if(uiManagerRef.brushesUI.isUnlocked) {
            buttonBrushesLink.interactable = true;
            //buttonBrushesLink.gameObject.SetActive(true);
        }
        else {
            buttonBrushesLink.interactable = false;
            //buttonBrushesLink.gameObject.SetActive(false);
        }

        string essenceDescriptionStr = "";

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
            curIconColor = uiManagerRef.colorDecomposersLayer;
            curIconSprite = uiManagerRef.spriteSpiritDecomposerIcon;
            essenceDescriptionStr = "Decomposers break down the old so that new life can grow.";
        }
        else if (selectedWorldSpiritSlot.kingdomID == 1) {
            if (selectedWorldSpiritSlot.tierID == 0) {
                isSelectedAlgae = true;
                curIconColor = uiManagerRef.colorAlgaeLayer;
                curIconSprite = uiManagerRef.spriteSpiritAlgaeIcon;
                essenceDescriptionStr = "Algae needs light and nutrients to grow.";
            }
            else {
                if(selectedWorldSpiritSlot.slotID == 0) {
                    isSelectedPlants0 = true;
                    curIconColor = uiManagerRef.colorPlantsLayer;
                    curIconSprite = uiManagerRef.spriteSpiritPlantIcon;
                    essenceDescriptionStr = "Floating Plants that are a foodsource for Vertebrates";
                }
                else {
                    isSelectedPlants1 = true;
                    curIconColor = uiManagerRef.colorPlantsLayer;
                    curIconSprite = uiManagerRef.spriteSpiritPlantIcon;
                    essenceDescriptionStr = "Big Plants?";
                }
                
            }
        }
        else if (selectedWorldSpiritSlot.kingdomID == 2) {
            if (selectedWorldSpiritSlot.tierID == 0) {
                isSelectedZooplankton = true;
                curIconColor = uiManagerRef.colorZooplanktonLayer;
                curIconSprite = uiManagerRef.spriteSpiritZooplanktonIcon;
                essenceDescriptionStr = "Tiny Organisms that feed on Algae";
            }
            else {
                curIconColor = uiManagerRef.colorVertebratesLayer;
                curIconSprite = uiManagerRef.spriteSpiritVertebrateIcon;
                essenceDescriptionStr = "Animals that can feed on Plants, Zooplankton, or even other Vertebrates.";

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
                curIconSprite = uiManagerRef.spriteSpiritWorldIcon;
                isSelectedTerrain0 = true;
                curIconColor = uiManagerRef.colorWorldLayer;
                essenceDescriptionStr = "The World Spirit provides the spark for a new universe";
            }
            else if (selectedWorldSpiritSlot.slotID == 1) {
                isSelectedTerrain1 = true;
                curIconSprite = uiManagerRef.spriteSpiritStoneIcon;
                curIconColor = uiManagerRef.colorTerrainLayer;
                essenceDescriptionStr = "Stone Spirits are some of the oldest known";
            }
            else if (selectedWorldSpiritSlot.slotID == 2) {
                isSelectedTerrain2 = true;
                curIconSprite = uiManagerRef.spriteSpiritPebblesIcon;
                curIconColor = uiManagerRef.colorTerrainLayer;
                essenceDescriptionStr = "Pebble Spirits are usually found in rivers and streams";
            }
            else if (selectedWorldSpiritSlot.slotID == 3) {
                isSelectedTerrain3 = true;
                curIconSprite = uiManagerRef.spriteSpiritSandIcon;
                curIconColor = uiManagerRef.colorTerrainLayer;
                essenceDescriptionStr = "Sand Spirits";
            }
        }
        else if (selectedWorldSpiritSlot.kingdomID == 4) {
            if (selectedWorldSpiritSlot.slotID == 0) {
                isSelectedMinerals = true;
                curIconColor = uiManagerRef.colorMineralLayer;
                curIconSprite = uiManagerRef.spriteSpiritMineralsIcon;
                essenceDescriptionStr = "Mineral Spirits infuse nutrients into the earth.";
            }
            else if (selectedWorldSpiritSlot.slotID == 1) {
                isSelectedWater = true;
                curIconColor = uiManagerRef.colorWaterLayer;
                curIconSprite = uiManagerRef.spriteSpiritWaterIcon;
                essenceDescriptionStr = "Water Spirits";
            }
            else if (selectedWorldSpiritSlot.slotID == 2) {
                isSelectedAir = true;
                curIconColor = uiManagerRef.colorAirLayer;
                curIconSprite = uiManagerRef.spriteSpiritAirIcon;
                essenceDescriptionStr = "Air Spirits";
            }
        }

        isDim = true;
        if(uiManagerRef.panelFocus == UIManager.PanelFocus.WorldHub) {
            isDim = false;
        }
        isDim = false;
        //imageBitInfo.gameObject.SetActive(isDim);
        textSelectedEssenceName.gameObject.SetActive(!isDim);
        textSelectedEssenceDescription.gameObject.SetActive(!isDim);
        //buttonBrushesLink.gameObject.SetActive(!isDim);
        //buttonKnowledgeLink.gameObject.SetActive(!isDim);
        //buttonMutationLink.gameObject.SetActive(!isDim);

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

        //imageSelectedTargetLayer.sprite = curIconSprite;
        //imageSelectedTargetLayer.color = curIconColor;

        textSelectedEssenceName.text = selectedWorldSpiritSlot.speciesName;
        textSelectedEssenceName.color = curIconColor;
        textSelectedEssenceDescription.text = essenceDescriptionStr;
        imageSelectedEssence.color = curIconColor;
        imageSelectedEssence.sprite = curIconSprite;
        imageColorBar.color = curIconColor;

    }
    public void UpdateWorldSpiritHubUI() {
        if(uiManagerRef.panelFocus == UIManager.PanelFocus.WorldHub) {
            animatorWorldHubUI.SetBool("_IsDimmed", false);
        }
        else {
            animatorWorldHubUI.SetBool("_IsDimmed", true);            
        }

        animatorWorldHubUI.SetBool("_IsOpen", isOpen);

        bool animFinished = animatorWorldHubUI.GetBool("_AnimFinished");
        if(animFinished) {
            //Debug.Log("UpdateWorldSpiritHubUI: animFinished " + animFinished.ToString());
            //animatorWorldHubUI.SetBool("_AnimFinished", false);
        }
        imageBitInfo.gameObject.SetActive(animFinished); // Testing anim-driven approach
        panelWorldHubExpand.SetActive(true); // isOpen);
        if(isOpen) {
            UpdateUI();
        }
        else {
            //panelWorldHubExpand.SetActive(false);
        }
    }

    public void ClickMinimizePanel() {
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        isOpen = false;
        animatorWorldHubUI.SetBool("_IsOpen", false);
        //panelWorldHubExpand.SetActive(false);
        Debug.Log("ClickToolButton) " + isOpen.ToString());
    }
    public void SetTargetFromBrushesUI() {
        selectedWorldSpiritSlot = uiManagerRef.brushesUI.selectedEssenceSlot;
        uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        this.isOpen = true;
        animatorWorldHubUI.SetBool("_IsOpen", true);
        //isPaletteOpen = true;
    }
    public void SetTargetFromWatcherUI() {
        selectedWorldSpiritSlot = uiManagerRef.watcherUI.watcherSelectedTrophicSlotRef;
        uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        this.isOpen = true;
        animatorWorldHubUI.SetBool("_IsOpen", true);
    }
    public void OpenWorldTreeSelect() {
        isOpen = true;
        uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        animatorWorldHubUI.SetBool("_IsOpen", true);
        //Animation animTerrainShake = imageBitTerrain.GetComponent<Animation>();
        //animTerrainShake.StartPlayback();
        //animTerrainShake.Play();

        Debug.Log("OpenWorldTreeSelect " + isOpen.ToString());
    }
    
    //*********************************************
    public void ClickButtonWorldSpiritHubOther(int index) {
        Debug.Log("ClickButtonPaletteOther: " + index.ToString());

        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        selectedWorldSpiritSlot = slot;
        
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedToolbarOtherLayer = index;
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubTerrain(int index) {
        

        Debug.Log("ClickButtonPaletteTerrain: " + index.ToString());

        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        selectedWorldSpiritSlot = slot;
        
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedToolbarTerrainLayer = index;
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubDecomposers() {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }
    public void ClickButtonWorldSpiritHubPlants(int slotID) {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }
    public void ClickButtonWorldSpiritHubZooplankton() {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            //ClickToolbarCreateNewSpecies();
            ClickWorldCreateNewSpecies(slot);
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //curActiveTool = ToolType.None;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubAgent(int index) {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        selectedWorldSpiritSlot = slot;
        //isToolbarDetailPanelOn = true;

        selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            //ClickToolbarCreateNewSpecies();  // UNLOCKING!!!! ********************************************* need to address at some point!!!! ***************
            ClickWorldCreateNewSpecies(slot);

        }
        
        if(selectedWorldSpiritSlot.status != TrophicSlot.SlotStatus.Unlocked) {
            //InitToolbarPortraitCritterData(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef); // ***
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }

    
    public void ClickWorldCreateNewSpecies(TrophicSlot slot) {
        // questionable code, possibly un-needed:
        uiManagerRef.gameManager.simulationManager.trophicLayersManager.CreateTrophicSlotSpecies(uiManagerRef.gameManager.simulationManager, slot, uiManagerRef.cameraManager.curCameraFocusPivotPos, uiManagerRef.gameManager.simulationManager.simAgeTimeSteps);
                
        uiManagerRef.gameManager.theRenderKing.baronVonWater.StartCursorClick(uiManagerRef.cameraManager.curCameraFocusPivotPos);
        
        //isAnnouncementTextOn = true;

        if(slot.kingdomID == 0) {
            uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of DEcomposer added!";
            uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorDecomposersLayer;
            //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        }
        else if(slot.kingdomID == 1) {
            if(slot.tierID == 0) {
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
        else if(slot.kingdomID == 2) { // ANIMALS
            if(slot.tierID == 1) {
                //if(createSpecies) {
                // v v v Actually creates new speciesPool here:::
                //TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
                slot.speciesName = "Vertebrate " + (slot.slotID + 1).ToString();
                uiManagerRef.gameManager.simulationManager.CreateAgentSpecies(uiManagerRef.cameraManager.curCameraFocusPivotPos);
                
                uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[slot.slotID].linkedSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1].speciesID;

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
