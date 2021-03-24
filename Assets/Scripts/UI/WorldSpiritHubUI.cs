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
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Lookup lookup => Lookup.instance;
    Sprite spiritWorldIcon => lookup.spiritWorldIcon;
    Sprite spiritStoneIcon => lookup.spiritStoneIcon;
    Sprite spiritAlgaeIcon => lookup.spiritAlgaeIcon;
    Sprite spiritPlantIcon => lookup.spiritPlantIcon;
    Sprite spiritZooplanktonIcon => lookup.spiritZooplanktonIcon;
    Sprite spiritVertebrateIcon => lookup.spiritVertebrateIcon;
    Sprite spiritDecomposerIcon => lookup.spiritDecomposerIcon;
    Sprite spiritPebblesIcon => lookup.spiritPebblesIcon;
    Sprite spiritSandIcon => lookup.spiritSandIcon;
    Sprite spiritMineralsIcon => lookup.spiritMineralsIcon;
    Sprite spiritWaterIcon => lookup.spiritWaterIcon;
    Sprite spiritAirIcon => lookup.spiritAirIcon;
    Color colorVertebratesLayer => lookup.colorVertebratesLayer;
    Color colorWorldLayer => lookup.colorWorldLayer;
    Color colorTerrainLayer => lookup.colorTerrainLayer; 
    Color colorMineralLayer => lookup.colorMineralLayer;
    Color colorWaterLayer => lookup.colorWaterLayer;
    Color colorAirLayer => lookup.colorAirLayer;


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
        TrophicLayersManager layerManager = simulationManager.trophicLayersManager;

        //textCurrencies.text = "Tier: " + tierA.ToString() + "\n(" + currencyB.ToString() + " of " + "16" + ") to next Tier"; // More # For Tier Up"; // "CurrencyA: " + currencyA.ToString() + "\nCurrencyB: " + currencyB.ToString();
        //textCurrencies.text = "WORLD SIZE: " + uiManagerRef.gameManager.theRenderKing.baronVonTerrain._WorldRadius.ToString();

        //buttonKnowledgeLink.interactable = true;
        //buttonMutationLink.interactable = false;3
        
        if(uiManagerRef.watcherUI.isUnlocked) {
            buttonWatcherLink.gameObject.SetActive(true);
            if (uiManagerRef.panelFocus == PanelFocus.WorldHub) {
                imageBitWatcher.color = Color.white;
            }
            else {
                imageBitWatcher.color = Color.gray;
            }
        }
        else {
            buttonWatcherLink.gameObject.SetActive(false);
        }

        if(uiManagerRef.brushesUI.isUnlocked) {
            buttonBrushesLink.interactable = true;
            buttonBrushesLink.gameObject.SetActive(true);
        }
        else {
            buttonBrushesLink.interactable = false;
            buttonBrushesLink.gameObject.SetActive(false);
        }

        if(uiManagerRef.panelFocus == PanelFocus.Brushes) {
            imageBitBrushes.color = selectedWorldSpiritSlot.color;// Color.white;
            buttonBrushesLink.GetComponent<Image>().color = Color.white;
        }
        else {
            imageBitBrushes.color = new Color(73f / 255f, 79f / 255f, 88f / 255f); // Color.gray;
            buttonBrushesLink.GetComponent<Image>().color = Color.gray;
        }
        if(uiManagerRef.panelFocus == PanelFocus.Watcher) {
            imageBitWatcher.color = Color.white;
            buttonWatcherLink.GetComponent<Image>().color = Color.white;

        }
        else {
            imageBitWatcher.color = Color.gray;
            buttonWatcherLink.GetComponent<Image>().color = Color.gray;
        }

        if(uiManagerRef.knowledgeUI.isUnlocked) {
            
            if (uiManagerRef.panelFocus == PanelFocus.WorldHub) {
                imageBitKnowledge.color = Color.white;
                buttonKnowledgeLink.GetComponent<Image>().color = Color.white;
            }
            else {
                imageBitKnowledge.color = Color.gray;
                buttonKnowledgeLink.GetComponent<Image>().color = Color.gray;
            }            
        }
        else {
            //buttonOpenKnowledgePanel.gameObject.SetActive(false);
            //worldSpiritHubUI.imageBitKnowledge.gameObject.SetActive(false);
        }
        
        

        //string essenceDescriptionStr = "";

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
            curIconSprite = spiritDecomposerIcon;
            //essenceDescriptionStr = "Decomposers break down the old so that new life can grow.";
            textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalDecomposers.ToString("F1");
        }
        else if (selectedWorldSpiritSlot.kingdomID == 1) {
            if (selectedWorldSpiritSlot.tierID == 0) {
                isSelectedAlgae = true;
                curIconColor = uiManagerRef.colorAlgaeLayer;
                curIconSprite = spiritAlgaeIcon;
                //essenceDescriptionStr = "Algae needs light and nutrients to grow.";
                textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalAlgaeReservoir.ToString("F1");
            }
            else {
                if(selectedWorldSpiritSlot.slotID == 0) {
                    isSelectedPlants0 = true;
                    curIconColor = uiManagerRef.colorPlantsLayer;
                    curIconSprite = spiritPlantIcon;
                    //essenceDescriptionStr = "Floating Plants that are a foodsource for Vertebrates";
                    textTotalMass.text = "Biomass: " + simulationManager.simResourceManager.curGlobalPlantParticles.ToString("F1");
                }
                else {
                    isSelectedPlants1 = true;
                    curIconColor = uiManagerRef.colorPlantsLayer;
                    curIconSprite = spiritPlantIcon;
                    //essenceDescriptionStr = "Big Plants?";
                    //textTotalMass.text = "Biomass: " + uiManagerRef.gameManager.simulationManager
                }
                
            }
        }
        else if (selectedWorldSpiritSlot.kingdomID == 2) {
            if (selectedWorldSpiritSlot.tierID == 0) {
                isSelectedZooplankton = true;
                curIconColor = uiManagerRef.colorZooplanktonLayer;
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

        isDim = true;
        if(uiManagerRef.panelFocus == PanelFocus.WorldHub) {
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
        textSelectedEssenceDescription.text = "text here?";// essenceDescriptionStr;
        imageSelectedEssence.color = curIconColor * 1.35f;
        imageSelectedEssence.sprite = curIconSprite;
        imageColorBar.color = curIconColor;
        //imageBitBG.color = curIconColor;

    }
    public void UpdateWorldSpiritHubUI() {
        if(uiManagerRef.panelFocus == PanelFocus.WorldHub) {
            //animatorWorldHubUI.SetBool("_IsDimmed", false);
        }
        else {
            //animatorWorldHubUI.SetBool("_IsDimmed", true);            
        }

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
        Debug.Log("ClickToolButton) " + isOpen.ToString());
    }
    public void SetTargetFromBrushesUI() {
        selectedWorldSpiritSlot = uiManagerRef.brushesUI.selectedEssenceSlot;
        uiManagerRef.panelFocus = PanelFocus.WorldHub;
        this.isOpen = true;
        //animatorWorldHubUI.SetBool("_IsOpen", true);
        //isPaletteOpen = true;
    }
    public void SetTargetFromWatcherUI() {
        selectedWorldSpiritSlot = uiManagerRef.watcherUI.watcherSelectedTrophicSlotRef;
        uiManagerRef.panelFocus = PanelFocus.WorldHub;
        this.isOpen = true;
        //animatorWorldHubUI.SetBool("_IsOpen", true);
    }
    public void OpenWorldTreeSelect() {
        isOpen = true;
        uiManagerRef.panelFocus = PanelFocus.WorldHub;
        //animatorWorldHubUI.SetBool("_IsOpen", true);
        //Animation animTerrainShake = imageBitTerrain.GetComponent<Animation>();
        //animTerrainShake.StartPlayback();
        //animTerrainShake.Play();

        Debug.Log("OpenWorldTreeSelect " + isOpen.ToString());
    }
    
    //*********************************************
    public void ClickButtonWorldSpiritHubOther(int index) {
        Debug.Log("ClickButtonPaletteOther: " + index.ToString());

        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        selectedWorldSpiritSlot = slot;
        
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedToolbarOtherLayer = index;
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubTerrain(int index) {
        

        Debug.Log("ClickButtonPaletteTerrain: " + index.ToString());

        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        selectedWorldSpiritSlot = slot;
        
        //selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedToolbarTerrainLayer = index;
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubDecomposers() {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
        //isBrushSelected = false;
    }
    public void ClickButtonWorldSpiritHubAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }
    public void ClickButtonWorldSpiritHubPlants(int slotID) {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
        selectedWorldSpiritSlot = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickWorldCreateNewSpecies(slot);
            //ClickToolbarCreateNewSpecies();
        }
        //uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
    }
    public void ClickButtonWorldSpiritHubZooplankton() {
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
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
        TrophicSlot slot = simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
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
        simulationManager.trophicLayersManager.CreateTrophicSlotSpecies(simulationManager, slot, uiManagerRef.cameraManager.curCameraFocusPivotPos, simulationManager.simAgeTimeSteps);
                
        theRenderKing.baronVonWater.StartCursorClick(uiManagerRef.cameraManager.curCameraFocusPivotPos);
        
        //isAnnouncementTextOn = true;

        if(slot.kingdomID == 0) {
            uiManagerRef.NarratorText("A new species of D3composer added!", uiManagerRef.colorDecomposersLayer);
            //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "A new species of DEcomposer added!";
            //uiManagerRef.panelPendingClickPrompt.GetComponentInChildren<Text>().color = uiManagerRef.colorDecomposersLayer;
            //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        }
        else if(slot.kingdomID == 1) {
            if(slot.tierID == 0) {
                uiManagerRef.NarratorText("A new species of Algae added!", uiManagerRef.colorAlgaeLayer);                
            }
            else {   /// BIG PLANTS:
                uiManagerRef.NarratorText("A new species of PLAN%T added!", uiManagerRef.colorPlantsLayer);               
            }
            
        }
        else if(slot.kingdomID == 2) { // ANIMALS
            if(slot.tierID == 1) {
                //if(createSpecies) {
                // v v v Actually creates new speciesPool here:::
                //TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
                slot.speciesName = "Vertebrate " + (slot.slotID + 1).ToString();
                            
                selectedWorldSpiritVertebrateSpeciesID = slot.linkedSpeciesID; // ???
                //InitToolbarPortraitCritterData(slot);                
                
                if(slot.slotID == 0) {
                    //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "These creatures start with randomly-generated brains\n and must evolve successful behavior\nthrough survival of the fittest";
                    uiManagerRef.isAnnouncementTextOn = false;  // *** hacky...
                }
                
            }
            else {

                uiManagerRef.NarratorText("A new species of Zooplankton added!", colorVertebratesLayer);
            }
        }

        //curToolbarWingPanelSelectID = 1;

        
        //timerAnnouncementTextCounter = 0;
        //recentlyCreatedSpecies = true;
        //recentlyCreatedSpeciesTimeStepCounter = 0;
        //UpdateSelectedSpeciesColorUI();
    }
    

}
