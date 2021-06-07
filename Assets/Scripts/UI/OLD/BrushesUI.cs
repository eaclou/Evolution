using UnityEngine;
using UnityEngine.UI;

public class BrushesUI : MonoBehaviour {
    UIManager uiManagerRef => UIManager.instance;
    WorldSpiritHubUI worldSpiritHubUI => uiManagerRef.worldSpiritHubUI;
    
    public bool isUnlocked;
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

    //public Animator animatorBrushesUI;
        
    public float toolbarInfluencePoints = 1f;
    public Text textInfluencePointsValue;
    //public Material infoMeterInfluencePointsMat;
    //private int influencePointsCooldownCounter = 0;     // WPP: assigned but not used
    //private int influencePointsCooldownDuration = 90;   // WPP: assigned but not used
    public bool isInfluencePointsCooldown = false;
    public CreationBrush[] creationBrushesArray;

    public Button buttonSendToWorldHub;
    public Image imageInfluencePoints;

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
    public int selectedBrushLinkedSpiritTerrainLayer = 0;

    // * WPP: references not used
    public Button buttonBrushLinkedSpiritOther0;  // Minerals
    public Button buttonBrushLinkedSpiritOther1;  // Water
    public Button buttonBrushLinkedSpiritOther2;  // Air
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
    
    SimulationManager simulationManager => SimulationManager.instance;
    TrophicLayersManager trophicLayersManager => simulationManager.trophicLayersManager;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Lookup lookup => Lookup.instance;
    Sprite spiritBrushStirIcon;
    Sprite spiritBrushCreationIcon;
    Color buttonActiveColor => lookup.buttonActiveColor;
    Color buttonDisabledColor => lookup.buttonDisabledColor;
    Color colorSpiritBrushLight => lookup.colorSpiritBrushLight;
    Color colorSpiritBrushDark => lookup.colorSpiritBrushDark;
    
    /*Sprite spiritWorldIcon => lookup.spiritWorldIcon;
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
    Color colorDecomposersLayer => lookup.colorDecomposersLayer;
    Color colorAlgaeLayer => lookup.colorAlgaeLayer;
    Color colorPlantsLayer => lookup.colorPlantsLayer;
    Color colorZooplanktonLayer => lookup.colorZooplanktonLayer;*/


    private string GetSpiritBrushSummary(TrophicLayersManager layerManager) {
        string str = "";

        switch(uiManagerRef.curActiveTool) {
            case ToolType.None:
                //buttonToolbarInspect.GetComponent<Image>().color = buttonDisabledColor;

                break;
                /*case ToolType.Watcher:

                str = "This spirit reveals\nhidden information.";
                str += "\n\n";                
                break;
            case ToolType.Mutate:
                //buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor;
                break;*/
            case ToolType.Add:
                str = "This spirit creates stuff.\n\nAlt Effect: Removes stuff.";
                break;
            //case ToolType.Remove:
                //buttonToolbarRemove.GetComponent<Image>().color = buttonActiveColor;
                //buttonToolbarRemove.gameObject.transform.localScale = Vector3.one * 1.25f;
                //break;
            case ToolType.Stir:
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
        createBrush1.baseScale = 1.5f;
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
        // TEMPORARY!!!!!::::::
        selectedEssenceSlot = worldSpiritHubUI.selectedWorldSpiritSlot;
        selectedBrushLinkedSpiritOtherLayer = worldSpiritHubUI.selectedToolbarOtherLayer;
        selectedBrushLinkedSpiritTerrainLayer = worldSpiritHubUI.selectedToolbarTerrainLayer;
        selectedBrushVertebrateSpeciesID = worldSpiritHubUI.selectedWorldSpiritVertebrateSpeciesID;
        
        //animatorBrushesUI.SetBool("MinPanel", !isOpen);
        bool isDim = false;
        
        textBrushLinkedSpiritName.gameObject.SetActive(!isDim);
        textSelectedBrushDescription.gameObject.SetActive(!isDim);
        textSelectedBrushEffects.gameObject.SetActive(!isDim);
        textSelectedBrushName.gameObject.SetActive(!isDim);
        textInfluencePointsValue.gameObject.SetActive(!isDim);
        buttonSendToWorldHub.gameObject.SetActive(!isDim);
        imageInfluencePoints.gameObject.SetActive(!isDim);

        //panelBrushes.SetActive(isOpen);
        panelBrushPaletteSelect.SetActive(false); // isPaletteOpen);
        if (isOpen) {
            UpdateUI();
            UpdateBrushPaletteUI();
            if(isPaletteOpen) {
                //UpdateBrushPaletteUI();
            }
        }        
    }
    private void UpdateUI() {
        // * WPP: use nested class pattern to avoid GetComponent in main loop
        buttonBrushStir.GetComponent<Image>().color = buttonDisabledColor;
        buttonBrushStir.gameObject.transform.localScale = Vector3.one;
        buttonBrushAdd.GetComponent<Image>().color = buttonDisabledColor;
        buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
        buttonBrushExtra1.GetComponent<Image>().color = buttonDisabledColor;
        buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
        buttonBrushExtra2.GetComponent<Image>().color = buttonDisabledColor;
        buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
        buttonBrushExtra3.GetComponent<Image>().color = buttonDisabledColor;
        buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
        
        UpdateCurSelectedColor();
        
        curIconColor = new Color(curIconColor.r * 0.35f, curIconColor.g * 0.35f, curIconColor.b * 0.35f);
        imageColorBar.color = curIconColor;

        //imageIsBrushing.gameObject.SetActive(uiManagerRef.isBrushModeON_snoopingOFF);
        //imageIsBrushing.sprite = curIconSprite;
        //imageIsBrushing.color = curIconColor;

        switch(uiManagerRef.curActiveTool) {
            case ToolType.None:
                //
                break;            
            case ToolType.Add:
                if(curCreationBrushIndex == 0) {
                    // * WPP: use nested class pattern to avoid GetComponent in main loop
                    buttonBrushAdd.GetComponent<Image>().color = buttonActiveColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushExtra1.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra2.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra3.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else if(curCreationBrushIndex == 1) {
                    buttonBrushExtra1.GetComponent<Image>().color = buttonActiveColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushAdd.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra2.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra3.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else if(curCreationBrushIndex == 2) {
                    buttonBrushExtra2.GetComponent<Image>().color = buttonActiveColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushExtra1.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonBrushAdd.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra3.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else {
                    buttonBrushExtra3.GetComponent<Image>().color = buttonActiveColor;
                    buttonBrushExtra3.gameObject.transform.localScale = Vector3.one * 1.33f;

                    buttonBrushExtra1.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonBrushExtra2.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonBrushAdd.GetComponent<Image>().color = buttonDisabledColor;
                    buttonBrushAdd.gameObject.transform.localScale = Vector3.one;
                }
                break;            
            case ToolType.Stir:
                buttonBrushStir.GetComponent<Image>().color = buttonActiveColor;
                buttonBrushStir.gameObject.transform.localScale = Vector3.one * 1.5f;
                break;            
            default:
                break;

        }

        //if(uiManagerRef.panelFocus != PanelFocus.Brushes) {
            buttonBrushAdd.GetComponent<Image>().color = buttonDisabledColor * 0.5f;
        //}
    
        string spiritBrushName = "Minor Creation Spirit " + curCreationBrushIndex.ToString();
        imageSelectedBrushThumbnail.sprite = spiritBrushCreationIcon;
        //strSpiritBrushDescription = "This spirit has some powers of life and death";
        //strSpiritBrushEffects = "Left-Click:\n" + strLeftClickEffect[leftClickDescriptionIndex] + "\n\nRight-Click:\n" + strRightClickEffect[rightClickDescriptionIndex];
        if(theCursorCzar.isDraggingMouseRight) {
            spiritBrushName = "Minor Decay Spirit" + curCreationBrushIndex.ToString();  
        }
        
        if(uiManagerRef.curActiveTool == ToolType.Stir) {
            spiritBrushName = "Lesser Stir Spirit";      
            imageSelectedBrushThumbnail.sprite = spiritBrushStirIcon;

            //strSpiritBrushDescription = "This spirit reveals hidden information about aspects of the world";
            //strSpiritBrushEffects = "Left-Click:\nFollows the nearest Vertebrate\n\nRight-Click:\nStops following";
        }

        textBrushLinkedSpiritName.resizeTextMaxSize = 20;

        textSelectedBrushName.text = spiritBrushName;
        textSelectedBrushName.resizeTextMaxSize = 24;
        textSelectedBrushName.color = colorSpiritBrushLight;

        imageSelectedBrushThumbnail.color = colorSpiritBrushLight;
        imageSelectedBrushThumbnailBorder.color = colorSpiritBrushDark;
        
        textBrushLinkedSpiritName.color = curIconColor; // speciesColorLight;
        imageBrushLinkedSpiritThumbnailBorder.color = curIconColor; // speciesColorDark;
          
        textBrushLinkedSpiritName.text = "Brushing: " + selectedEssenceSlot.speciesName;
        
        imageSelectedBrushThumbnail.color = curIconColor;                
        imageBrushLinkedSpiritThumbnail.color = curIconColor;
        imageBrushLinkedSpiritThumbnail.sprite = curIconSprite;
    }
    
    // WPP: pull data from SO
    private void UpdateCurSelectedColor() {  
        /*string str = "";
        //selected layer ui identifying color:
        if (selectedEssenceSlot.kingdomID == KingdomId.Decomposers) {
            curIconColor = selectedEssenceSlot.color; //colorDecomposersLayer;
            curIconSprite = spiritDecomposerIcon;
            str = "Creates Decomposers";
        }
        else if (selectedEssenceSlot.id == KnowledgeMapId.Algae) {
            //if (selectedEssenceSlot.tierID == 0) {
            curIconColor = colorAlgaeLayer;
            curIconSprite = spiritAlgaeIcon;
            str = "Creates a bloom of Algae";
        }
        else if (selectedEssenceSlot.id == KnowledgeMapId.Plants) {
            curIconColor = colorPlantsLayer;
            curIconSprite = spiritPlantIcon;
            str = "Creates floating plant seedlings";
        }
        //}
        else if (selectedEssenceSlot.id == KnowledgeMapId.Microbes) {
        //    if (selectedEssenceSlot.tierID == 0) {
            curIconColor = colorZooplanktonLayer;
            curIconSprite = spiritZooplanktonIcon;
            str = "Creates simple tiny creatures";
        }
        else if (selectedEssenceSlot.id == KnowledgeMapId.Animals) {
                curIconColor = colorVertebratesLayer;
                curIconSprite = spiritVertebrateIcon;
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
            //}
        }
        else if (selectedEssenceSlot.kingdomID == KingdomId.Terrain) {                
            if (selectedEssenceSlot.slotID == 0) {
                curIconSprite = spiritWorldIcon;                    
                curIconColor = colorWorldLayer;
                str = "Creates World";
            }
            else if (selectedEssenceSlot.slotID == 1) {
                curIconSprite = spiritStoneIcon;
                curIconColor = colorTerrainLayer;
                str = "Raises stone from deep below";
            }
            else if (selectedEssenceSlot.slotID == 2) {
                curIconSprite = spiritPebblesIcon;
                curIconColor = colorTerrainLayer;
                str = "Creates mounds of pebbles";
            }
            else if (selectedEssenceSlot.slotID == 3) {
                curIconSprite = spiritSandIcon;
                curIconColor = colorTerrainLayer;
                str = "Blankets the terrain with sand";
            }
        }
        else if (selectedEssenceSlot.kingdomID == KingdomId.Other) {
            if (selectedEssenceSlot.slotID == 0) {
                curIconColor = colorMineralLayer;
                curIconSprite = spiritMineralsIcon;
                str = "Creates nutrient-rich minerals in the ground";
            }
            else if (selectedEssenceSlot.slotID == 1) {
                curIconColor = colorWaterLayer;
                curIconSprite = spiritWaterIcon;
                str = "Raises the water level";
            }
            else if (selectedEssenceSlot.slotID == 2) {
                curIconColor = colorAirLayer;
                curIconSprite = spiritAirIcon;
                str = "Increases wind strength";
            }
        }*/

        curIconColor = selectedEssenceSlot.color;
        curIconSprite = selectedEssenceSlot.icon;
        textSelectedBrushDescription.text = selectedEssenceSlot.data.brushDescription;
    }
    
    private void UpdateBrushPaletteUI() {
        Color iconColor = Color.white;
        /*
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
                iconColor = colorDecomposersLayer;
                //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritDecomposerIcon;
            }
            else if (selectedEssenceSlot.kingdomID == 1) {
                if (selectedEssenceSlot.tierID == 0) {
                    isSelectedAlgae = true;
                    iconColor = colorAlgaeLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritAlgaeIcon;
                }
                else {
                    isSelectedPlants = true;
                    iconColor = colorPlantsLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritPlantIcon;
                }
            }
            else if (selectedEssenceSlot.kingdomID == 2) {
                if (selectedEssenceSlot.tierID == 0) {
                    isSelectedZooplankton = true;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritZooplanktonIcon;
                }
                else {
                    iconColor = colorVertebratesLayer;
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
                iconColor = colorTerrainLayer;
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
                    iconColor = colorMineralLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritMineralsIcon;
                }
                else if (selectedEssenceSlot.slotID == 1) {
                    isSelectedWater = true;
                    iconColor = colorWaterLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritWaterIcon;
                }
                else if (selectedEssenceSlot.slotID == 2) {
                    isSelectedAir = true;
                    iconColor = colorAirLayer;
                    //imageToolbarSpeciesPortraitRender.sprite = uiManagerRef.spriteSpiritAirIcon;
                }
            }
        }
        */
        //bool dimButtons = true;
        /*
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelectedDecomposers);

        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelectedAlgae);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status, isSelectedPlants);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritPlant2, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[1].status, isSelectedPlants);

        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelectedZooplankton);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelectedVertebrate0);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelectedVertebrate1);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelectedVertebrate2);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelectedVertebrate3);

        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritTerrain0, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0].status, isSelectedTerrain0);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritTerrain1, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[1].status, isSelectedTerrain1);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritTerrain2, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2].status, isSelectedTerrain2);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritTerrain3, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3].status, isSelectedTerrain3);

        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritOther0, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status, isSelectedMinerals);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritOther1, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status, isSelectedWater);
        uiManagerRef.SetToolbarButtonStateUI(dimButtons, ref buttonBrushLinkedSpiritOther2, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status, isSelectedAir);
        */
    }
	
    // * WPP: reduce conditional nesting
    public void ApplyCreationBrush() {
        toolbarInfluencePoints -= 0.002f;

        float randRipple = Random.Range(0f, 1f);
        if(randRipple < 0.33) {
            theRenderKing.baronVonWater.RequestNewWaterRipple(new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x / SimulationManager._MapSize, theCursorCzar.curMousePositionOnWaterPlane.y / SimulationManager._MapSize));
        }
        uiManagerRef.updateTerrainAltitude = false;                

        if (true) { //trophicLayersManager.isSelectedTrophicSlot) {
            if(selectedEssenceSlot.kingdomID == KingdomId.Decomposers) {
                simulationManager.vegetationManager.isBrushActive = true;
            }
            else if(selectedEssenceSlot.kingdomID == KingdomId.Plants) {
                simulationManager.vegetationManager.isBrushActive = true;
            }
            else if(selectedEssenceSlot.id == KnowledgeMapId.Animals) {
                int speciesIndex = selectedEssenceSlot.linkedSpeciesID;
                if (theCursorCzar.isDraggingMouseLeft) {
                    //gameManager.simulationManager.recentlyAddedSpeciesOn = true; // ** needed?
                    uiManagerRef.isBrushAddingAgents = true;
                                    
                    //Debug.Log("isBrushAddingAgents = true; speciesID = " + speciesIndex.ToString());

                    uiManagerRef.brushAddAgentCounter++;

                    if(uiManagerRef.brushAddAgentCounter >= uiManagerRef.framesPerAgentSpawn) {
                        uiManagerRef.brushAddAgentCounter = 0;

                        simulationManager.AttemptToBrushSpawnAgent(speciesIndex);
                    }
                }
                if (theCursorCzar.isDraggingMouseRight) {
                    simulationManager.AttemptToKillAgent(speciesIndex, new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y), 15f);
                }
            }
            else if (selectedEssenceSlot.kingdomID == KingdomId.Terrain) {
                uiManagerRef.updateTerrainAltitude = true;
                uiManagerRef.terrainUpdateMagnitude = 0.05f;
                if(selectedEssenceSlot.layerIndex == 0) { // WORLD
                    uiManagerRef.terrainUpdateMagnitude = 1f;
                    //theRenderKing.ClickTestTerrainUpdateMaps(true, 0.4f);
                }
                else if(selectedEssenceSlot.layerIndex == 1) {  // STONE
                    uiManagerRef.terrainUpdateMagnitude = 1f;
                    //theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                }
                else if(selectedEssenceSlot.layerIndex == 2) {  // PEBBLES
                    uiManagerRef.terrainUpdateMagnitude = 1f;
                    //theRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                }
                else if(selectedEssenceSlot.layerIndex == 3) {  // SAND
                    uiManagerRef.terrainUpdateMagnitude = 1f;
                    //heRenderKing.ClickTestTerrainUpdateMaps(true, 0.04f);
                }
            }
            else {
                if (selectedEssenceSlot.layerIndex == 0) {  // MINERALS
                    simulationManager.vegetationManager.isBrushActive = true;
                }
                else if (selectedEssenceSlot.layerIndex == 1) {   // WATER
                    if (theCursorCzar.isDraggingMouseLeft) {
                        SimulationManager._GlobalWaterLevel = Mathf.Clamp01(SimulationManager._GlobalWaterLevel + 0.002f);
                    }
                    if (theCursorCzar.isDraggingMouseRight) {
                        SimulationManager._GlobalWaterLevel = Mathf.Clamp01(SimulationManager._GlobalWaterLevel - 0.002f);
                    }
                }
                else if (selectedEssenceSlot.layerIndex == 2) {   // AIR
                    if (theCursorCzar.isDraggingMouseLeft) {
                        if (Random.Range(0f, 1f) < 0.1f) {
                            simulationManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp((simulationManager.environmentFluidManager.curTierWaterCurrents + 1), 0, 10);
                        }                                    
                    }
                    if (theCursorCzar.isDraggingMouseRight) {
                        if(Random.Range(0f, 1f) < 0.1f) {
                            simulationManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp((simulationManager.environmentFluidManager.curTierWaterCurrents - 1), 0, 10);                       
                        }
                    }  
                }
            }              
        }
                        
        theRenderKing.isBrushing = true;
        theRenderKing.isSpiritBrushOn = true;
        theRenderKing.spiritBrushPosNeg = 1f;
        if(theCursorCzar.isDraggingMouseRight) {  // *** Refactor!!!
            theRenderKing.spiritBrushPosNeg = -1f;
        }

        // Also Stir Water -- secondary effect
        float mag = theCursorCzar.smoothedMouseVel.magnitude * 0.35f;
        float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

        if (mag > 0f) {
            simulationManager.PlayerToolStirOn(theCursorCzar.curMousePositionOnWaterPlane, theCursorCzar.smoothedMouseVel * (0.25f + theRenderKing.baronVonWater.camDistNormalized * 1.2f) * 0.33f, radiusMult);
        }
        else {
            simulationManager.PlayerToolStirOff();
        }
        theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight; 
    }

    public void Unlock() {
        isUnlocked = true;
    }

    public void ClickToolButton() {
        isOpen = !isOpen;

        if(isOpen) {
            EnterCreationBrushMode();
            //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***

            //animatorBrushesUI.SetBool("MinPanel", false);
        }
        else {
            //uiManagerRef.panelFocus = PanelFocus.WorldHub;

            //animatorBrushesUI.SetBool("MinPanel", true);
        }
    }
    
    public void SetTargetFromWorldTree() {
        selectedEssenceSlot = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;

        EnterCreationBrushMode();
        isOpen = true;
        isPaletteOpen = false;
    }
    
    /*public void ClickBrushPaletteOpen() {
        isPaletteOpen = !isPaletteOpen;
        uiManagerRef.curActiveTool = UIManager.ToolType.Add;
        uiManagerRef.isBrushModeON_snoopingOFF = true;
        EnterCreationBrushMode();
    }*/
    
    public void ClickToolButtonStir() {
        uiManagerRef.curActiveTool = ToolType.Stir;
              
        //uiManagerRef.watcherUI.StopFollowingAgent();
        //uiManagerRef.watcherUI.StopFollowingPlantParticle();
        //uiManagerRef.watcherUI.StopFollowingAnimalParticle();
        buttonBrushStir.GetComponent<Image>().color = buttonActiveColor; 
 
        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
              
    }
    
    public void ClickToolButtonAdd() {  
        Debug.Log("ClickToolButtonAdd(0)");
        curCreationBrushIndex = 0;
        EnterCreationBrushMode();
        
        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonExtra1() {
        Debug.Log("ClickToolButtonExtra1()");
        curCreationBrushIndex = 1;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;

        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonExtra2() {
        Debug.Log("ClickToolButtonExtra2()");
        curCreationBrushIndex = 2;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;

        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonExtra3() {
        Debug.Log("ClickToolButtonExtra3()");
        curCreationBrushIndex = 3;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;

        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    private void EnterCreationBrushMode() {
        uiManagerRef.curActiveTool = ToolType.Add;
        //uiManagerRef.watcherUI.StopFollowingAgent();
        //uiManagerRef.watcherUI.StopFollowingPlantParticle();
        //uiManagerRef.watcherUI.StopFollowingAnimalParticle();      
        //uiManagerRef.panelFocus = PanelFocus.Brushes;
    }
    
    //*********************************************
    public void ClickButtonBrushPaletteOther(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedEssenceSlot = slot;
        //trophicLayersManager.isSelectedTrophicSlot = true;
        //trophicLayersManager.selectedTrophicSlotRef = slot;
        
        //
        selectedBrushVertebrateSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedBrushLinkedSpiritOtherLayer = data.layerIndex;

        //isBrushSelected = false;
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
    
    public void ClickButtonBrushPaletteTerrain(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        //trophicLayersManager.isSelectedTrophicSlot = true;
        //old: //trophicLayersManager.selectedTrophicSlotRef = slot;
        selectedEssenceSlot = slot;
        
        //selectedBrushVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedBrushLinkedSpiritTerrainLayer = data.layerIndex;

        //isBrushSelected = false;
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
    
    public void ClickButtonBrushPaletteDecomposers() {
        //TrophicSlot slot = trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
        var slot = trophicLayersManager.GetSlot(KnowledgeMapId.Decomposers);
        selectedEssenceSlot = slot; 
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
    
    // Not interactable if locked
    public void ClickButtonBrushPaletteAlgae() { 
        //TrophicSlot slot = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        var slot = trophicLayersManager.GetSlot(KnowledgeMapId.Algae);
        selectedEssenceSlot = slot;    
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
    
    public void ClickButtonBrushPalettePlants(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedEssenceSlot = slot;      
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
    
    public void ClickButtonBrushPaletteZooplankton() {
        //TrophicSlot slot = trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];   
        var slot = trophicLayersManager.GetSlot(KnowledgeMapId.Microbes);     
        selectedEssenceSlot = slot;  
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
    
    public void ClickButtonBrushPaletteAgent(TrophicLayerSO data) {
        TrophicSlot slot = trophicLayersManager.GetSlot(data);
        selectedEssenceSlot = slot;
       
        selectedBrushVertebrateSpeciesID = slot.linkedSpeciesID; // update this next

        selectedBrushLinkedSpiritVertebrateLayer = data.layerIndex;
        isPaletteOpen = false;
        EnterCreationBrushMode();
    }
}