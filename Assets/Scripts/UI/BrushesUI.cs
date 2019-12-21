using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushesUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isOpen;
    //public bool isBrushSelected;
    public bool isPaletteOpen;
    public TrophicSlot selectedEssenceSlot;        
    public int curCreationBrushIndex = 0;  // 0 == original, 1-3 extra experimental

    public GameObject panelBrushes;
    public GameObject panelBrushPaletteSelect;

    public Button buttonBrushStir;
    public Button buttonBrushAdd;
    public Button buttonBrushExtra1;
    public Button buttonBrushExtra2;
    public Button buttonBrushExtra3;
        
    public float toolbarInfluencePoints = 1f;
    public Text textInfluencePointsValue;
    public Material infoMeterInfluencePointsMat;
    private int influencePointsCooldownCounter = 0;
    private int influencePointsCooldownDuration = 90;
    public bool isInfluencePointsCooldown = false;
    public CreationBrush[] creationBrushesArray;

    public Image imageColorBar;
    public Image imageIsBrushing;

    public Text textSelectedBrushName;
    public Text textSelectedBrushDescription;
    public Text textSelectedBrushEffects;
    public Image imageSelectedBrushThumbnail;
    public Image imageSelectedBrushThumbnailBorder;

    public Text textBrushLinkedSpiritName;
    public Image imageBrushLinkedSpiritThumbnail;
    public Image imageBrushLinkedSpiritThumbnailBorder;
    

    public int selectedBrushLinkedSpiritOtherLayer = 0;
    public Button buttonBrushLinkedSpiritOther0;  // Minerals
    public Button buttonBrushLinkedSpiritOther1;  // Water
    public Button buttonBrushLinkedSpiritOther2;  // Air
    public int selectedBrushLinkedSpiritTerrainLayer = 0;
    public Button buttonBrushLinkedSpiritTerrain0;
    public Button buttonBrushLinkedSpiritTerrain1;
    public Button buttonBrushLinkedSpiritTerrain2;
    public Button buttonBrushLinkedSpiritTerrain3;
    public Button buttonBrushLinkedSpiritDecomposers;
    public Button buttonBrushLinkedSpiritAlgae;
    public Button buttonBrushLinkedSpiritPlant1;
    public Button buttonBrushLinkedSpiritPlant2;
    public Button buttonBrushLinkedSpiritZooplankton;
    public Button buttonBrushLinkedSpiritAnimal1;
    public Button buttonBrushLinkedSpiritAnimal2;
    public Button buttonBrushLinkedSpiritAnimal3;
    public Button buttonBrushLinkedSpiritAnimal4;
    public int selectedBrushLinkedSpiritVertebrateLayer = 0;
    public int selectedBrushVertebrateSpeciesID = 0;

    public Color curIconColor = Color.white;
    public Sprite curIconSprite = null;

    //public Text textLinkedSpiritDescription; -- more of KnowledgeSpirit's thing

    //public Image imageToolbarButtonBarBackground;
    //public Image imageToolbarWingLine;
    //public Button buttonToolbarWingCreateSpecies;
    //public Text textToolbarWingSpeciesSummary;
    //public Text textToolbarWingPanelName;
    //public Text textSelectedSpeciesTitle;
    //public Text textSelectedSpeciesIndex;
    
    //public Text textSelectedSpeciesDescription;
    //public int selectedSpeciesStatsIndex;
    


    private string GetSpiritBrushSummary(TrophicLayersManager layerManager) {
        string str = "";

        switch(uiManagerRef.curActiveTool) {
            case UIManager.ToolType.None:
                //buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;

                break;
                /*case ToolType.Watcher:

                str = "This spirit reveals\nhidden information.";
                str += "\n\n";                
                break;
            case ToolType.Mutate:
                //buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor;
                break;*/
            case UIManager.ToolType.Add:
                str = "This spirit creates stuff.\n\nAlt Effect: Removes stuff.";
                break;
            //case ToolType.Remove:
                //buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;
                //buttonToolbarRemove.gameObject.transform.localScale = Vector3.one * 1.25f;
                //break;
            case UIManager.ToolType.Stir:
                str = "This spirit pushes material around.\n\nAlt Effect: None";
                break;
            default:
                break;

        }

        return str;
    }
	// Use this for initialization
	void Start () {
        // ********************************************* MOVE THIS!!!! somewhere into initial loading code
        creationBrushesArray = new CreationBrush[4];
        CreationBrush createBrush0 = new CreationBrush();
        createBrush0.name = "Create Brush 0";
        createBrush0.type = CreationBrush.BrushType.Drag;
        createBrush0.baseScale = 3f;
        createBrush0.baseAmplitude = 1f;
        createBrush0.patternColumn = 0f;
        createBrush0.patternRow = 2f;  

        CreationBrush createBrush1 = new CreationBrush();
        createBrush1.name = "Create Brush 1";
        createBrush1.type = CreationBrush.BrushType.Drag;
        createBrush1.baseScale = 3f;
        createBrush1.baseAmplitude = 1f;
        createBrush1.patternColumn = 3f;
        createBrush1.patternRow = 3f;

        CreationBrush createBrush2 = new CreationBrush();
        createBrush2.name = "Create Brush 2";
        createBrush2.type = CreationBrush.BrushType.Burst;
        createBrush2.baseScale = 1f;
        createBrush2.baseAmplitude = 1f;
        createBrush2.patternColumn = 0f;
        createBrush2.patternRow = 2f;
        createBrush2.burstTotalDuration = 40;

        CreationBrush createBrush3 = new CreationBrush();
        createBrush3.name = "Create Brush 3";
        createBrush3.type = CreationBrush.BrushType.Burst;
        createBrush3.baseScale = 2f;
        createBrush3.baseAmplitude = 0.2f;
        createBrush3.patternColumn = 3f;
        createBrush3.patternRow = 3f;
        createBrush3.burstTotalDuration = 40;

        creationBrushesArray[0] = createBrush0;
        creationBrushesArray[1] = createBrush1;
        creationBrushesArray[2] = createBrush2;
        creationBrushesArray[3] = createBrush3;
        
	}

    public void UpdateBrushesUI() {
        panelBrushes.SetActive(isOpen);
        panelBrushPaletteSelect.SetActive(isPaletteOpen);
        if (isOpen) {
            UpdateUI();
            if(isPaletteOpen) {
                UpdateBrushPaletteUI();
            }
        }        
    }
    private void UpdateUI() {
        buttonBrushStir.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        buttonBrushStir.gameObject.transform.localScale = Vector3.one;
        buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
        buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
        buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
        buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
        
        TrophicLayersManager layerManager = uiManagerRef.gameManager.simulationManager.trophicLayersManager;

        UpdateCurSelectedColor();
        imageColorBar.color = curIconColor;

        imageIsBrushing.gameObject.SetActive(uiManagerRef.isBrushModeON_snoopingOFF);
        imageIsBrushing.sprite = curIconSprite;
        imageIsBrushing.color = curIconColor;


        switch(uiManagerRef.curActiveTool) {
            case UIManager.ToolType.None:
                //
                break;            
            case UIManager.ToolType.Add:
                if(curCreationBrushIndex == 0) {
                    buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else if(curCreationBrushIndex == 1) {
                    buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else if(curCreationBrushIndex == 2) {
                    buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else {
                    buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
                }
                break;            
            case UIManager.ToolType.Stir:
                buttonBrushStir.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
                buttonBrushStir.gameObject.transform.localScale = Vector3.one * 1.5f;
                break;            
            default:
                break;

        }

    
        string spiritBrushName = "Minor Creation Spirit " + curCreationBrushIndex.ToString();
        imageSelectedBrushThumbnail.sprite = uiManagerRef.spriteSpiritBrushCreationIcon;
        //strSpiritBrushDescription = "This spirit has some powers of life and death";
        //strSpiritBrushEffects = "Left-Click:\n" + strLeftClickEffect[leftClickDescriptionIndex] + "\n\nRight-Click:\n" + strRightClickEffect[rightClickDescriptionIndex];
        if(uiManagerRef.theCursorCzar.isDraggingMouseRight) {
            spiritBrushName = "Minor Decay Spirit" + curCreationBrushIndex.ToString();

            
        }
        if(uiManagerRef.curActiveTool == UIManager.ToolType.Stir) {
            spiritBrushName = "Lesser Stir Spirit";      
            imageSelectedBrushThumbnail.sprite = uiManagerRef.spriteSpiritBrushStirIcon;

            //strSpiritBrushDescription = "This spirit reveals hidden information about aspects of the world";
            //strSpiritBrushEffects = "Left-Click:\nFollows the nearest Vertebrate\n\nRight-Click:\nStops following";
        }

        UpdateInfluencePointsUI();

        textBrushLinkedSpiritName.resizeTextMaxSize = 20;

        textSelectedBrushName.text = spiritBrushName;
        textSelectedBrushName.resizeTextMaxSize = 24;
        textSelectedBrushName.color = uiManagerRef.colorSpiritBrushLight;

        imageSelectedBrushThumbnail.color = uiManagerRef.colorSpiritBrushLight;
        imageSelectedBrushThumbnailBorder.color = uiManagerRef.colorSpiritBrushDark;
        
        textBrushLinkedSpiritName.color = curIconColor; // speciesColorLight;
        imageBrushLinkedSpiritThumbnailBorder.color = curIconColor; // speciesColorDark;
          
        textBrushLinkedSpiritName.text = "Brushing: " + selectedEssenceSlot.speciesName;
        
        imageSelectedBrushThumbnail.color = curIconColor;                
        imageBrushLinkedSpiritThumbnail.color = curIconColor;
        imageBrushLinkedSpiritThumbnail.sprite = curIconSprite;

    }
    private void UpdateCurSelectedColor() {
        
        string str = "";
        //selected layer ui identifying color:
        if (selectedEssenceSlot.kingdomID == 0) {
            curIconColor = uiManagerRef.colorDecomposersLayer;
            curIconSprite = uiManagerRef.spriteSpiritDecomposerIcon;
            str = "Creates Decomposers";
        }
        else if (selectedEssenceSlot.kingdomID == 1) {
            if (selectedEssenceSlot.tierID == 0) {
                curIconColor = uiManagerRef.colorAlgaeLayer;
                curIconSprite = uiManagerRef.spriteSpiritAlgaeIcon;
                str = "Creates a bloom of Algae";
            }
            else {
                curIconColor = uiManagerRef.colorPlantsLayer;
                curIconSprite = uiManagerRef.spriteSpiritPlantIcon;
                str = "Creates floating plant seedlings";
            }
        }
        else if (selectedEssenceSlot.kingdomID == 2) {
            if (selectedEssenceSlot.tierID == 0) {
                curIconColor = uiManagerRef.colorZooplanktonLayer;
                curIconSprite = uiManagerRef.spriteSpiritZooplanktonIcon;
                str = "Creates simple tiny creatures";
            }
            else {
                curIconColor = uiManagerRef.colorVertebratesLayer;
                curIconSprite = uiManagerRef.spriteSpiritVertebrateIcon;
                str = "Hatches Vertebrates";
                if (selectedEssenceSlot.slotID == 0) {
                    //isSelectedVertebrate0 = true;
                }
                else if (selectedEssenceSlot.slotID == 1) {
                    //isSelectedVertebrate1 = true;
                }
                else if (selectedEssenceSlot.slotID == 2) {
                    //isSelectedVertebrate2 = true;
                }
                else {
                    //isSelectedVertebrate3 = true;
                }
            }
        }
        else if (selectedEssenceSlot.kingdomID == 3) {                
            if (selectedEssenceSlot.slotID == 0) {
                curIconSprite = uiManagerRef.spriteSpiritWorldIcon;                    
                curIconColor = uiManagerRef.colorWorldLayer;
                str = "Creates World";
            }
            else if (selectedEssenceSlot.slotID == 1) {
                curIconSprite = uiManagerRef.spriteSpiritStoneIcon;
                curIconColor = uiManagerRef.colorTerrainLayer;
                str = "Raises stone from deep below";
            }
            else if (selectedEssenceSlot.slotID == 2) {
                curIconSprite = uiManagerRef.spriteSpiritPebblesIcon;
                curIconColor = uiManagerRef.colorTerrainLayer;
                str = "Creates mounds of pebbles";
            }
            else if (selectedEssenceSlot.slotID == 3) {
                curIconSprite = uiManagerRef.spriteSpiritSandIcon;
                curIconColor = uiManagerRef.colorTerrainLayer;
                str = "Blankets the terrain with sand";
            }
        }
        else if (selectedEssenceSlot.kingdomID == 4) {
            if (selectedEssenceSlot.slotID == 0) {
                curIconColor = uiManagerRef.colorMineralLayer;
                curIconSprite = uiManagerRef.spriteSpiritMineralsIcon;
                str = "Creates nutrient-rich minerals in the ground";
            }
            else if (selectedEssenceSlot.slotID == 1) {
                curIconColor = uiManagerRef.colorWaterLayer;
                curIconSprite = uiManagerRef.spriteSpiritWaterIcon;
                str = "Raises the water level";
            }
            else if (selectedEssenceSlot.slotID == 2) {
                curIconColor = uiManagerRef.colorAirLayer;
                curIconSprite = uiManagerRef.spriteSpiritAirIcon;
                str = "Increases wind strength";
            }
        }

        textSelectedBrushDescription.text = str;
    }
	private void UpdateInfluencePointsUI() {
        // Influence points meter:     
        if(isInfluencePointsCooldown) {
            influencePointsCooldownCounter++;
            if(influencePointsCooldownCounter > influencePointsCooldownDuration) {
                influencePointsCooldownCounter = 0;
                isInfluencePointsCooldown = false;
            }
        }
        if(toolbarInfluencePoints < 0.1f) {
            toolbarInfluencePoints += 0.00015f; // x10 while debugging
        }
        else {
            toolbarInfluencePoints += 0.001f; // x10 while debugging
        }
        toolbarInfluencePoints = Mathf.Clamp01(toolbarInfluencePoints);
        infoMeterInfluencePointsMat.SetFloat("_FillPercentage", toolbarInfluencePoints);
        Color influenceBarTint = new Color(0.2f, 0.8f, 1f, 1f);
        string influenceText = (toolbarInfluencePoints * 100f).ToString("F0") + "%";
        if(isInfluencePointsCooldown) {
            influenceBarTint = new Color(0.8f, 0.2f, 0.2f, 1f);
            influenceText = "cooldown";
        }
        infoMeterInfluencePointsMat.SetColor("_Tint", influenceBarTint);
        textInfluencePointsValue.text = "Influence: \n" + influenceText;
        
    }
    private void UpdateBrushPaletteUI() {
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
        if (true) { //layerManager.isSelectedTrophicSlot) {
            if (selectedEssenceSlot.kingdomID == 0) {
                isSelectedDecomposers = true;
                iconColor = uiManagerRef.colorDecomposersLayer;
                //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritDecomposerIcon;
            }
            else if (selectedEssenceSlot.kingdomID == 1) {
                if (selectedEssenceSlot.tierID == 0) {
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
            else if (selectedEssenceSlot.kingdomID == 2) {
                if (selectedEssenceSlot.tierID == 0) {
                    isSelectedZooplankton = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritZooplanktonIcon;
                }
                else {
                    iconColor = uiManagerRef.colorVertebratesLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritVertebrateIcon;

                    if (selectedEssenceSlot.slotID == 0) {
                        isSelectedVertebrate0 = true;
                    }
                    else if (selectedEssenceSlot.slotID == 1) {
                        isSelectedVertebrate1 = true;
                    }
                    else if (selectedEssenceSlot.slotID == 2) {
                        isSelectedVertebrate2 = true;
                    }
                    else {
                        isSelectedVertebrate3 = true;
                    }

                }

            }
            else if (selectedEssenceSlot.kingdomID == 3) {
                iconColor = uiManagerRef.colorTerrainLayer;
                if (selectedEssenceSlot.slotID == 0) {
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritWorldIcon;
                    isSelectedTerrain0 = true;
                }
                else if (selectedEssenceSlot.slotID == 1) {
                    isSelectedTerrain1 = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritStoneIcon;
                }
                else if (selectedEssenceSlot.slotID == 2) {
                    isSelectedTerrain2 = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritPebblesIcon;
                }
                else if (selectedEssenceSlot.slotID == 3) {
                    isSelectedTerrain3 = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritSandIcon;
                }
            }
            else if (selectedEssenceSlot.kingdomID == 4) {
                if (selectedEssenceSlot.slotID == 0) {
                    isSelectedMinerals = true;
                    iconColor = uiManagerRef.colorMineralLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritMineralsIcon;
                }
                else if (selectedEssenceSlot.slotID == 1) {
                    isSelectedWater = true;
                    iconColor = uiManagerRef.colorWaterLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritWaterIcon;
                }
                else if (selectedEssenceSlot.slotID == 2) {
                    isSelectedAir = true;
                    iconColor = uiManagerRef.colorAirLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritAirIcon;
                }
            }
        }
        else {

        }
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelectedDecomposers);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelectedAlgae);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status, isSelectedPlants);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritPlant2, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[1].status, isSelectedPlants);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelectedZooplankton);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelectedVertebrate0);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelectedVertebrate1);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelectedVertebrate2);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelectedVertebrate3);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritTerrain0, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0].status, isSelectedTerrain0);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritTerrain1, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[1].status, isSelectedTerrain1);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritTerrain2, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2].status, isSelectedTerrain2);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritTerrain3, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3].status, isSelectedTerrain3);

        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritOther0, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status, isSelectedMinerals);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritOther1, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status, isSelectedWater);
        uiManagerRef.SetToolbarButtonStateUI(ref buttonBrushLinkedSpiritOther2, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status, isSelectedAir);

    }
	
    /*public void UpdateSpiritBrushDescriptionsUI() {
        TrophicLayersManager layerManager = uiManagerRef.gameManager.simulationManager.trophicLayersManager;  

        int linkedSpiritIndex = 0;

        if (selectedEssenceSlot.kingdomID == 0) {
            linkedSpiritIndex = 7;            
        }
        else if(selectedEssenceSlot.kingdomID == 1) {
            if(selectedEssenceSlot.tierID == 0) {
                linkedSpiritIndex = 8;
            }
            else {
                linkedSpiritIndex = 9;
            }            
        }
        else if(selectedEssenceSlot.kingdomID == 2) {
            if(selectedEssenceSlot.tierID == 0) {
                linkedSpiritIndex = 10;
            }
            else {
                linkedSpiritIndex = 11;
            }
        }
        else if(selectedEssenceSlot.kingdomID == 3) {
            if(selectedEssenceSlot.slotID == 0) {  // world/bedrock
                linkedSpiritIndex = 0;
            }
            else if(selectedEssenceSlot.slotID == 1) {
                linkedSpiritIndex = 1;
            }
            else if(selectedEssenceSlot.slotID == 2) {
                linkedSpiritIndex = 2;
            }
            else {
                linkedSpiritIndex = 3;
            }
        }
        else {  // 4 == OTHER
            if(selectedEssenceSlot.slotID == 0) {  // minerals
                linkedSpiritIndex = 4;
            }
            else if(selectedEssenceSlot.slotID == 1) {  // water
                linkedSpiritIndex = 5;
            }
            else {  // air
                linkedSpiritIndex = 6;
            }
        }

        int spiritBrushIndex = 0;
        if(uiManagerRef.curActiveTool == UIManager.ToolType.Add) {
            spiritBrushIndex = 1;
        }
        else if (uiManagerRef.curActiveTool == UIManager.ToolType.Stir) {
            spiritBrushIndex = 2; 
        }

        string strSpiritBrushDescription = "";
        string[] linkedSpiritNamesArray = new string[12]; 
        linkedSpiritNamesArray[0] = "World";
        linkedSpiritNamesArray[1] = "Stone";
        linkedSpiritNamesArray[2] = "Pebbles";
        linkedSpiritNamesArray[3] = "Sand";
        linkedSpiritNamesArray[4] = "Minerals";
        linkedSpiritNamesArray[5] = "Water";
        linkedSpiritNamesArray[6] = "Air";
        linkedSpiritNamesArray[7] = "Decomposers";
        linkedSpiritNamesArray[8] = "Algae";
        linkedSpiritNamesArray[9] = "Plants";
        linkedSpiritNamesArray[10] = "Zooplankton";
        linkedSpiritNamesArray[11] = "Vertebrates";
        
        string startTxt = "Left-Click:\n";
        string midTxt = "\n\nRight-Click:\n";
        string[][] strBrushEffectsArray = new string[5][];
        for(int s = 0; s < 5; s++) {
            strBrushEffectsArray[s] = new string[12];            
        }
        // CREATION BRUSH:
        strBrushEffectsArray[1][0] = startTxt + "Creates World" + midTxt + "None";
        strBrushEffectsArray[1][1] = startTxt + "Raises stone from deep below" + midTxt + "Destroys stone, deeping the Pond";
        strBrushEffectsArray[1][2] = startTxt + "Creates mounds of pebbles" + midTxt + "Removes pebbles from the area";
        strBrushEffectsArray[1][3] = startTxt + "Blankets the terrain with sand" + midTxt + "Removes sand from the area";
        strBrushEffectsArray[1][4] = startTxt + "Creates nutrient-rich minerals in the ground" + midTxt + "Saps nutrients out of the environment";
        strBrushEffectsArray[1][5] = startTxt + "Raises the water level" + midTxt + "Lowers water level";
        strBrushEffectsArray[1][6] = startTxt + "Increases wind strength" + midTxt + "Decreases wind strength";
        strBrushEffectsArray[1][7] = startTxt + "Creates Decomposers" + midTxt + "Kills decomposers in the area";
        strBrushEffectsArray[1][8] = startTxt + "Creates a bloom of Algae" + midTxt + "Kills algae in the area";
        strBrushEffectsArray[1][9] = startTxt + "Creates floating plant seedlings" + midTxt + "Kills plants in the area";
        strBrushEffectsArray[1][10] = startTxt + "Creates simple tiny creatures" + midTxt + "Kills nearby zooplankton";
        strBrushEffectsArray[1][11] = startTxt + "Hatches Vertebrates" + midTxt + "Kills Animals";

        // STIR BRUSH:
        strBrushEffectsArray[2][0] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][1] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][2] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][3] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][4] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][5] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][6] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][7] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][8] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][9] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][10] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][11] = startTxt + "Drags water along with itself while moving" + midTxt + "None";

    }*/
    
    public void ApplyCreationBrush() {
        toolbarInfluencePoints -= 0.002f;

        float randRipple = UnityEngine.Random.Range(0f, 1f);
        if(randRipple < 0.33) {
            uiManagerRef.gameManager.theRenderKing.baronVonWater.RequestNewWaterRipple(new Vector2(uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane.x / SimulationManager._MapSize, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane.y / SimulationManager._MapSize));

        }
        uiManagerRef.updateTerrainAltitude = false;                
        // IF TERRAIN SELECTED::::
        if (true) { //uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot) {
            // DECOMPOSERS::::
            if(selectedEssenceSlot.kingdomID == 0) {
                uiManagerRef.gameManager.simulationManager.vegetationManager.isBrushActive = true;
            }// PLANTS:
            else if(selectedEssenceSlot.kingdomID == 1) {
                if(selectedEssenceSlot.tierID == 0) {
                    uiManagerRef.gameManager.simulationManager.vegetationManager.isBrushActive = true;
                }
                else {
                    uiManagerRef.gameManager.simulationManager.vegetationManager.isBrushActive = true;
                                    
                }                                
            }  // ANIMALS::::                            
            else if(selectedEssenceSlot.kingdomID == 2) {
                if (selectedEssenceSlot.tierID == 0) {  
                    // zooplankton?
                }
                else {// AGENTS                                
                    int speciesIndex = selectedEssenceSlot.linkedSpeciesID;
                    if (uiManagerRef.theCursorCzar.isDraggingMouseLeft) {
                        //gameManager.simulationManager.recentlyAddedSpeciesOn = true; // ** needed?
                        uiManagerRef.isBrushAddingAgents = true;
                                        
                        //Debug.Log("isBrushAddingAgents = true; speciesID = " + speciesIndex.ToString());

                        uiManagerRef.brushAddAgentCounter++;

                        if(uiManagerRef.brushAddAgentCounter >= uiManagerRef.framesPerAgentSpawn) {
                            uiManagerRef.brushAddAgentCounter = 0;

                            uiManagerRef.gameManager.simulationManager.AttemptToBrushSpawnAgent(speciesIndex);
                        }
                    }
                    if (uiManagerRef.theCursorCzar.isDraggingMouseRight) {
                        uiManagerRef.gameManager.simulationManager.AttemptToKillAgent(speciesIndex, new Vector2(uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane.x, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane.y), 15f);
                    }                                   
                }
            }
            else if (selectedEssenceSlot.kingdomID == 3) {
                uiManagerRef.updateTerrainAltitude = true;
                uiManagerRef.terrainUpdateMagnitude = 0.05f;
                if(selectedEssenceSlot.slotID == 0) { // WORLD
                    uiManagerRef.terrainUpdateMagnitude = 1f;
                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.4f);
                }
                else if(selectedEssenceSlot.slotID == 1) {  // STONE

                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                }
                else if(selectedEssenceSlot.slotID == 2) {  // PEBBLES
                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                }
                else if(selectedEssenceSlot.slotID == 3) {  // SAND
                    //gameManager.simulationManager.theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                }
                                
            }
            else {
                if (selectedEssenceSlot.slotID == 0) {  // MINERALS
                    uiManagerRef.gameManager.simulationManager.vegetationManager.isBrushActive = true;
                    Debug.Log("SDFADSFAS");
                }
                else if (selectedEssenceSlot.slotID == 1) {   // WATER
                    if (uiManagerRef.theCursorCzar.isDraggingMouseLeft) {
                        uiManagerRef.gameManager.theRenderKing.baronVonWater._GlobalWaterLevel = Mathf.Clamp01(uiManagerRef.gameManager.theRenderKing.baronVonWater._GlobalWaterLevel + 0.002f);
                    }
                    if (uiManagerRef.theCursorCzar.isDraggingMouseRight) {
                        uiManagerRef.gameManager.theRenderKing.baronVonWater._GlobalWaterLevel = Mathf.Clamp01(uiManagerRef.gameManager.theRenderKing.baronVonWater._GlobalWaterLevel - 0.002f);
                    }
                }
                else if (selectedEssenceSlot.slotID == 2) {   // AIR
                    if (uiManagerRef.theCursorCzar.isDraggingMouseLeft) {
                        if (UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                            uiManagerRef.gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp((uiManagerRef.gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents + 1), 0, 10);
                        }                                    
                    }
                    if (uiManagerRef.theCursorCzar.isDraggingMouseRight) {
                        if(UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                            uiManagerRef.gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp((uiManagerRef.gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents - 1), 0, 10);
                                            
                        }
                    }  
                }
            }              
        }
                        
        uiManagerRef.gameManager.theRenderKing.isBrushing = true;
        uiManagerRef.gameManager.theRenderKing.isSpiritBrushOn = true;
        uiManagerRef.gameManager.theRenderKing.spiritBrushPosNeg = 1f;
        if(uiManagerRef.theCursorCzar.isDraggingMouseRight) {  // *** Re-Factor!!!
            uiManagerRef.gameManager.theRenderKing.spiritBrushPosNeg = -1f;
        }

        // Also Stir Water -- secondary effect
        float mag = uiManagerRef.theCursorCzar.smoothedMouseVel.magnitude * 0.35f;
        float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(uiManagerRef.gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

        if (mag > 0f) {
            uiManagerRef.gameManager.simulationManager.PlayerToolStirOn(uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane, uiManagerRef.theCursorCzar.smoothedMouseVel * (0.25f + uiManagerRef.gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.2f) * 0.33f, radiusMult);

        }
        else {
            uiManagerRef.gameManager.simulationManager.PlayerToolStirOff();
        }
        uiManagerRef.gameManager.theRenderKing.isStirring = uiManagerRef.theCursorCzar.isDraggingMouseLeft || uiManagerRef.theCursorCzar.isDraggingMouseRight; 
        
    }

    public void ClickToolButton() {
        isOpen = !isOpen;

        if(isOpen) {
            EnterCreationBrushMode();
            //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
        }
    }
    public void ClickBrushPaletteOpen() {
        isPaletteOpen = !isPaletteOpen;
    }
    public void ClickToolButtonStir() {
        
        uiManagerRef.curActiveTool = UIManager.ToolType.Stir;
              
        uiManagerRef.watcherUI.StopFollowingAgent();
        uiManagerRef.watcherUI.StopFollowingPlantParticle();
        uiManagerRef.watcherUI.StopFollowingAnimalParticle();
        //uiManagerRef.watcherUI.isHighlight = false;
        buttonBrushStir.GetComponent<Image>().color = uiManagerRef.buttonActiveColor; 
 
        //TurnOffAddTool();

        //isBrushSelected = true; // ??????

        uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
              
    }
    public void ClickToolButtonAdd() {  
        Debug.Log("ClickToolButtonAdd(0)");
        curCreationBrushIndex = 0;
        EnterCreationBrushMode();
        
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        
        uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    public void ClickToolButtonExtra1() {
        Debug.Log("ClickToolButtonExtra1()");
        curCreationBrushIndex = 1;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;

        uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    public void ClickToolButtonExtra2() {
        Debug.Log("ClickToolButtonExtra2()");
        curCreationBrushIndex = 2;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;

        uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    public void ClickToolButtonExtra3() {
        Debug.Log("ClickToolButtonExtra3()");
        curCreationBrushIndex = 3;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;

        uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    private void EnterCreationBrushMode() {
        uiManagerRef.curActiveTool = UIManager.ToolType.Add;
        uiManagerRef.watcherUI.StopFollowingAgent();
        uiManagerRef.watcherUI.StopFollowingPlantParticle();
        uiManagerRef.watcherUI.StopFollowingAnimalParticle();
        //uiManagerRef.watcherUI.isHighlight = false;
        //TurnOffStirTool();        
        //isBrushSelected = true;

        uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    //*********************************************
    public void ClickButtonBrushPaletteOther(int index) {
        Debug.Log("ClickButtonPaletteOther: " + index.ToString());

        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];
        selectedEssenceSlot = slot;
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        //
        selectedBrushVertebrateSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedBrushLinkedSpiritOtherLayer = index;

        //isBrushSelected = false;
        isPaletteOpen = false;
    }
    public void ClickButtonBrushPaletteTerrain(int index) {
        Debug.Log("ClickButtonPaletteTerrain: " + index.ToString());

        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        //old: //uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        selectedEssenceSlot = slot;


        selectedBrushVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedBrushLinkedSpiritTerrainLayer = index;

        //isBrushSelected = false;
        isPaletteOpen = false;
    }
    public void ClickButtonBrushPaletteDecomposers() {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
        selectedEssenceSlot = slot; 
        isPaletteOpen = false;
    }
    public void ClickButtonBrushPaletteAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        selectedEssenceSlot = slot;    
        isPaletteOpen = false;
    }
    public void ClickButtonBrushPalettePlants(int slotID) {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
        selectedEssenceSlot = slot;      
        isPaletteOpen = false;
    }
    public void ClickButtonBrushPaletteZooplankton() {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
        selectedEssenceSlot = slot;  
        isPaletteOpen = false;
    }
    public void ClickButtonBrushPaletteAgent(int index) {
        TrophicSlot slot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        selectedEssenceSlot = slot;
       
        selectedBrushVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedBrushLinkedSpiritVertebrateLayer = index;
        isPaletteOpen = false;
    }
}