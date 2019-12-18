using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushesUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isOpen;

    public Button buttonToolbarStir;
    public Button buttonToolbarMutate;
    public Button buttonToolbarExtra1;
    public Button buttonToolbarExtra2;
    public Button buttonToolbarExtra3;
    public Button buttonToolbarAdd;
    
    public float toolbarInfluencePoints = 1f;
    public Text textInfluencePointsValue;
    public Material infoMeterInfluencePointsMat;
    private int influencePointsCooldownCounter = 0;
    private int influencePointsCooldownDuration = 90;
    public bool isInfluencePointsCooldown = false;
    //private float addSpeciesInfluenceCost = 0.33f;
    public CreationBrush[] creationBrushesArray;

    public Text textSpiritBrushDescription;
    public Text textSpiritBrushEffects;
    public Text textLinkedSpiritDescription;

    public Image imageToolbarButtonBarBackground;
    public Image imageToolbarWingLine;
    public Button buttonToolbarWingCreateSpecies;
    public Text textToolbarWingSpeciesSummary;
    //public Text textToolbarWingPanelName;
    public Text textSelectedSpeciesTitle;
    public Text textSelectedSpeciesIndex;
    public Image imageToolbarSpeciesPortraitRender;
    public Image imageToolbarSpeciesPortraitBorder;
    public Text textSelectedSpeciesDescription;
    public int selectedSpeciesStatsIndex;
    public Text textSelectedSpiritBrushName;
    public Image imageToolbarSpiritBrushThumbnail;
    public Image imageToolbarSpiritBrushThumbnailBorder;


    private string GetSpiritBrushSummary(TrophicLayersManager layerManager) {
        string str = "";

        switch(curActiveTool) {
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
	
	//imageToolbarInspectLinkedIcon.color = buttonDisabledColor;
        buttonToolbarStir.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarStir.gameObject.transform.localScale = Vector3.one;
        buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarAdd.gameObject.transform.localScale = Vector3.one;
        imageToolbarAddLinkedIcon.color = buttonDisabledColor;

    TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;  

        //panelKnowledgeSpiritBase.SetActive(false);
        //textToolbarWingSpeciesSummary.gameObject.SetActive(true);

        switch(curActiveTool) {
            case ToolType.None:
                //
                break;            
            case ToolType.Add:
                if(curCreationBrushIndex == 0) {
                    buttonToolbarAdd.GetComponent<Image>().color = buttonActiveColor;
                    buttonToolbarAdd.gameObject.transform.localScale = Vector3.one * 1.25f;

                    buttonToolbarExtra1.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarExtra2.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarExtra3.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else if(curCreationBrushIndex == 1) {
                    buttonToolbarExtra1.GetComponent<Image>().color = buttonActiveColor;
                    buttonToolbarExtra1.gameObject.transform.localScale = Vector3.one * 1.25f;

                    buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarAdd.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarExtra2.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarExtra3.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else if(curCreationBrushIndex == 2) {
                    buttonToolbarExtra2.GetComponent<Image>().color = buttonActiveColor;
                    buttonToolbarExtra2.gameObject.transform.localScale = Vector3.one * 1.25f;

                    buttonToolbarExtra1.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarAdd.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarExtra3.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra3.gameObject.transform.localScale = Vector3.one;
                }
                else {
                    buttonToolbarExtra3.GetComponent<Image>().color = buttonActiveColor;
                    buttonToolbarExtra3.gameObject.transform.localScale = Vector3.one * 1.25f;

                    buttonToolbarExtra1.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra1.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarExtra2.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarExtra2.gameObject.transform.localScale = Vector3.one;
                    buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
                    buttonToolbarAdd.gameObject.transform.localScale = Vector3.one;
                }
                break;            
            case ToolType.Stir:
                buttonToolbarStir.GetComponent<Image>().color = buttonActiveColor;
                buttonToolbarStir.gameObject.transform.localScale = Vector3.one * 1.25f;
                break;            
            default:
                break;

        }
}

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




    
        
        textSelectedSpeciesTitle.resizeTextMaxSize = 20;
        textSelectedSpiritBrushName.resizeTextMaxSize = 24;

        textSelectedSpiritBrushName.color = colorSpiritBrushLight;
                
        string spiritBrushName = "Minor Creation Spirit";
        imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushCreationIcon;
        //strSpiritBrushDescription = "This spirit has some powers of life and death";
        //strSpiritBrushEffects = "Left-Click:\n" + strLeftClickEffect[leftClickDescriptionIndex] + "\n\nRight-Click:\n" + strRightClickEffect[rightClickDescriptionIndex];
        if(isDraggingMouseRight) {
            spiritBrushName = "Minor Decay Spirit";

            
        }
        if(curActiveTool == ToolType.Stir) {
            spiritBrushName = "Lesser Stir Spirit";      
            imageToolbarSpiritBrushThumbnail.sprite = spriteSpiritBrushStirIcon;

            //strSpiritBrushDescription = "This spirit reveals hidden information about aspects of the world";
            //strSpiritBrushEffects = "Left-Click:\nFollows the nearest Vertebrate\n\nRight-Click:\nStops following";
        }
        
        textSelectedSpiritBrushName.text = spiritBrushName;

        imageToolbarSpiritBrushThumbnail.color = colorSpiritBrushLight;
        imageToolbarSpiritBrushThumbnailBorder.color = colorSpiritBrushDark;




    UpdateSpiritBrushDescriptionsUI(); // ****************************************************************


textSelectedSpeciesTitle.color = iconColor; // speciesColorLight;
        imageToolbarSpeciesPortraitBorder.color = iconColor; // speciesColorDark;
          
        textSelectedSpeciesTitle.text = layerManager.selectedTrophicSlotRef.speciesName;

        // Test:
        if(!gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot) {
            textSelectedSpeciesTitle.text = "NONE";
        }

        //panelToolbarWingDeletePrompt.SetActive(false);
        textToolbarWingSpeciesSummary.gameObject.SetActive(true);
        
        imageToolbarSpeciesPortraitRender.color = iconColor;                
        imageToolbarSpeciesPortraitBorder.color = iconColor;








    
    public void ClickToolButtonStir() {
        
        curActiveTool = ToolType.Stir;
              
        StopFollowingAgent();
        StopFollowingPlantParticle();
        StopFollowingAnimalParticle();
        watcherUI.isHighlight = false;
        buttonToolbarStir.GetComponent<Image>().color = buttonActiveColor; 
 
        TurnOffAddTool();

        isSpiritBrushSelected = true;
              
    }
    
    public void ClickToolButtonExtra1() {
        Debug.Log("ClickToolButtonExtra1()");
        curCreationBrushIndex = 1;
        EnterCreationBrushMode();
        buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra1.GetComponent<Image>().color = buttonActiveColor;
        buttonToolbarExtra2.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra3.GetComponent<Image>().color = buttonDisabledColor;
    }
    public void ClickToolButtonExtra2() {
        Debug.Log("ClickToolButtonExtra2()");
        curCreationBrushIndex = 2;
        EnterCreationBrushMode();
        buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra1.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra2.GetComponent<Image>().color = buttonActiveColor;
        buttonToolbarExtra3.GetComponent<Image>().color = buttonDisabledColor;
    }
    public void ClickToolButtonExtra3() {
        Debug.Log("ClickToolButtonExtra3()");
        curCreationBrushIndex = 3;
        EnterCreationBrushMode();
        buttonToolbarAdd.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra1.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra2.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra3.GetComponent<Image>().color = buttonActiveColor;
    }
    private void EnterCreationBrushMode() {
        curActiveTool = ToolType.Add;
        StopFollowingAgent();
        StopFollowingPlantParticle();
        StopFollowingAnimalParticle();
        watcherUI.isHighlight = false;
        TurnOffStirTool();        
        isSpiritBrushSelected = true;
    }
    public void ClickToolButtonAdd() {  
        Debug.Log("ClickToolButtonAdd(0)");
        curCreationBrushIndex = 0;
        EnterCreationBrushMode();
        
        buttonToolbarAdd.GetComponent<Image>().color = buttonActiveColor;
        buttonToolbarExtra1.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra2.GetComponent<Image>().color = buttonDisabledColor;
        buttonToolbarExtra3.GetComponent<Image>().color = buttonDisabledColor;
        
    }



//*********************************************
public void ClickButtonToolbarOther(int index) {
        Debug.Log("ClickButtonToolbarOther: " + index.ToString());

        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;

        selectedSpeciesID = slot.linkedSpeciesID; // update this next ***
        
        selectedToolbarOtherLayer = index;

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarTerrain(int index) {
        Debug.Log("ClickButtonToolbarTerrain: " + index.ToString());

        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;

        selectedSpeciesID = slot.linkedSpeciesID; // update this next

        //curActiveTool = ToolType.None;


        /*if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.status != TrophicSlot.SlotStatus.Empty) {
            InitToolbarPortraitCritterData(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef); // ***
        }*/

        selectedToolbarTerrainLayer = index;

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarDecomposers() {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];

        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;

        if(gameManager.simulationManager.trophicLayersManager.GetDecomposersOnOff()) {
            /*if (slot.status == TrophicSlot.SlotStatus.On) {            
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlot = true;
                gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;  
                buttonSelectedTrophicSlot = buttonToolbarDecomposers;
            }*/
        }
        else {
            
            //gameManager.simulationManager.trophicLayersManager.PendingDecomposers();
            //buttonPendingTrophicSlot = buttonToolbarDecomposers;
            
        }

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }
        //curActiveTool = ToolType.None;
        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarAlgae() {  // shouldn't be able to click if LOCKED (interactive = false)
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarPlants(int slotID) {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[slotID];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }

        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarZooplankton() {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];        
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        
        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();
        }
        //curActiveTool = ToolType.None;
        isSpiritBrushSelected = false;
    }
    public void ClickButtonToolbarAgent(int index) {
        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        gameManager.simulationManager.trophicLayersManager.isSelectedTrophicSlot = true;
        gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = slot;
        //isToolbarDetailPanelOn = true;

        selectedSpeciesID = slot.linkedSpeciesID; // update this next

        if(slot.status == TrophicSlot.SlotStatus.Unlocked) {
            ClickToolbarCreateNewSpecies();


        }
        //curActiveTool = ToolType.None;


        if(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef.status != TrophicSlot.SlotStatus.Unlocked) {
            //InitToolbarPortraitCritterData(gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef); // ***
        }

        isSpiritBrushSelected = false;
        // Why do I have to click this twice before portrait shows up properly??????
    }