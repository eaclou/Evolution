using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// * WPP: Refactor -> remove dead code, expose magic values, simplify conditionals
public class BrushesUI : MonoBehaviour {
    UIManager uiManagerRef => UIManager.instance;
    WorldSpiritHubUI worldSpiritHubUI => uiManagerRef.worldSpiritHubUI;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    
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
    
    public ButtonBrush stirBrush;
    public ButtonBrush addBrush;
    public ButtonBrush extraBrush1;
    public ButtonBrush extraBrush2;
    public ButtonBrush extraBrush3;

    //public Animator animatorBrushesUI;
        
    public float toolbarInfluencePoints = 1f;
    public Text textInfluencePointsValue;
    //public Material infoMeterInfluencePointsMat;
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
    [SerializeField] Vector3 inactiveBrushScale = Vector3.one;
    [SerializeField] Vector3 activeBrushScale = new Vector3(1.3f, 1.3f, 1.3f);


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
        // * WPP: isDim is always false
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
            //UpdateBrushPaletteUI();
            if(isPaletteOpen) {
                //UpdateBrushPaletteUI();
            }
        }        
    }
    
    private void UpdateUI() {
        SetAllBrushesInactive();

        UpdateCurSelectedColor();
        
        curIconColor = new Color(curIconColor.r * 0.35f, curIconColor.g * 0.35f, curIconColor.b * 0.35f);
        imageColorBar.color = curIconColor;

        switch(uiManagerRef.curActiveTool) {
            case ToolType.None:
                break;            
            case ToolType.Add:
                if(curCreationBrushIndex == 0) {
                    SetBrushActive(addBrush);
                }
                else if(curCreationBrushIndex == 1) {
                    SetBrushActive(extraBrush1);
                }
                else if(curCreationBrushIndex == 2) {
                    SetBrushActive(extraBrush2);
                }
                else {
                    SetBrushActive(extraBrush3);
                }
                break;            
            case ToolType.Stir:
                stirBrush.image.color = buttonActiveColor;
                stirBrush.transform.localScale = Vector3.one * 1.5f;
                break;            
            default:
                break;
        }

        addBrush.image.color = buttonDisabledColor * 0.5f;

        string spiritBrushName = "Minor Creation Spirit " + curCreationBrushIndex;
        imageSelectedBrushThumbnail.sprite = spiritBrushCreationIcon;

        if (theCursorCzar.isDraggingMouseRight) {
            spiritBrushName = "Minor Decay Spirit" + curCreationBrushIndex;  
        }
        
        if(uiManagerRef.curActiveTool == ToolType.Stir) {
            spiritBrushName = "Lesser Stir Spirit";      
            imageSelectedBrushThumbnail.sprite = spiritBrushStirIcon;

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
    
    void SetBrushActive(ButtonBrush brush)
    {
        SetAllBrushesInactive();
        SetBrush(brush, true);
    }
    
    void SetAllBrushesInactive()
    {
        SetBrush(stirBrush, false);
        SetBrush(addBrush, false);
        SetBrush(extraBrush1, false);
        SetBrush(extraBrush2, false);
        SetBrush(extraBrush3, false);
    }
    
    void SetBrush(ButtonBrush brush, bool active)
    {
        brush.image.color = active ? buttonActiveColor : buttonDisabledColor;
        brush.transform.localScale = active ? activeBrushScale : inactiveBrushScale;
    }
    
    private void UpdateCurSelectedColor() {
        curIconColor = selectedEssenceSlot.color;
        curIconSprite = selectedEssenceSlot.icon;
        textSelectedBrushDescription.text = selectedEssenceSlot.data.brushDescription;
    }
    
    
    public void TickCreationBrush()
    {
        if(toolbarInfluencePoints >= 0.05f)
            ApplyCreationBrush();
        else
            isInfluencePointsCooldown = true;
    }
	
    // * WPP: reduce conditional nesting
    public void ApplyCreationBrush() {
        toolbarInfluencePoints -= 0.002f;

        float randRipple = Random.Range(0f, 1f);
        if(randRipple < 0.33) {
            theRenderKing.baronVonWater.RequestNewWaterRipple(new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x / SimulationManager._MapSize, theCursorCzar.curMousePositionOnWaterPlane.y / SimulationManager._MapSize));
        }
        uiManagerRef.updateTerrainAltitude = false;                
               
        if (selectedEssenceSlot.kingdomID == KingdomId.Decomposers) 
        {
            simulationManager.vegetationManager.isBrushActive = true;
        }
        else if(selectedEssenceSlot.kingdomID == KingdomId.Plants) 
        {
            simulationManager.vegetationManager.isBrushActive = true;
        }
        else if(selectedEssenceSlot.id == KnowledgeMapId.Animals) 
        {
            int speciesIndex = selectedEssenceSlot.linkedSpeciesID;
            if (theCursorCzar.isDraggingMouseLeft) 
            {
                //gameManager.simulationManager.recentlyAddedSpeciesOn = true; // ** needed?
                uiManagerRef.isBrushAddingAgents = true;
                //Debug.Log("isBrushAddingAgents = true; speciesID = " + speciesIndex.ToString());
                uiManagerRef.brushAddAgentCounter++;

                if(uiManagerRef.brushAddAgentCounter >= uiManagerRef.framesPerAgentSpawn) 
                {
                    uiManagerRef.brushAddAgentCounter = 0;
                    simulationManager.AttemptToBrushSpawnAgent(speciesIndex);
                }
            }
            if (theCursorCzar.isDraggingMouseRight) 
            {
                simulationManager.AttemptToKillAgent(speciesIndex, new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y), 15f);
            }
        }
        else if (selectedEssenceSlot.terrainSelected) 
        {
            uiManagerRef.updateTerrainAltitude = true;
            uiManagerRef.terrainUpdateMagnitude = 0.05f;
            
            // * WPP: is this all possible conditions?
            if (selectedEssenceSlot.worldSelected || 
                selectedEssenceSlot.stoneSelected ||   
                selectedEssenceSlot.pebblesSelected ||  
                selectedEssenceSlot.sandSelected)  
            { 
                uiManagerRef.terrainUpdateMagnitude = 1f;
                //theRenderKing.ClickTestTerrainUpdateMaps(true, 0.4f);
            }
        }
        else 
        {
            if (selectedEssenceSlot.mineralsSelected) 
            {  
                simulationManager.vegetationManager.isBrushActive = true;
            }
            else if (selectedEssenceSlot.waterSelected && theCursorCzar.isDraggingMouse)
            {
                SimulationManager._GlobalWaterLevel = Mathf.Clamp01(SimulationManager._GlobalWaterLevel + 0.002f);
            }
            else if (selectedEssenceSlot.airSelected &&
                    theCursorCzar.isDraggingMouse &&
                    Random.Range(0f, 1f) < 0.1f) 
            {
                fluidManager.curTierWaterCurrents = Mathf.Clamp(fluidManager.curTierWaterCurrents + 1, 0, 10);
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
        //theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight; 
    }

    public void Unlock() {
        isUnlocked = true;
    }

    public void ClickToolButton() {
        isOpen = !isOpen;

        if(isOpen) {
            EnterCreationBrushMode();
        }
        
    }
    
    public void SetTargetFromWorldTree() {
        selectedEssenceSlot = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;

        EnterCreationBrushMode();
        isOpen = true;
        isPaletteOpen = false;
    }
    
    
    public void ClickToolButtonStir() {
        uiManagerRef.curActiveTool = ToolType.Stir;
              
        //uiManagerRef.watcherUI.StopFollowingAgent();
        //uiManagerRef.watcherUI.StopFollowingPlantParticle();
        //uiManagerRef.watcherUI.StopFollowingAnimalParticle();
        
        stirBrush.image.color = buttonActiveColor;

        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonAdd() {  
        //Debug.Log("ClickToolButtonAdd(0)");
        curCreationBrushIndex = 0;
        EnterCreationBrushMode();
        
        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonExtra1() {
        //Debug.Log("ClickToolButtonExtra1()");
        curCreationBrushIndex = 1;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;

        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonExtra2() {
        //Debug.Log("ClickToolButtonExtra2()");
        curCreationBrushIndex = 2;
        EnterCreationBrushMode();
        //buttonBrushAdd.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra1.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;
        //buttonBrushExtra2.GetComponent<Image>().color = uiManagerRef.buttonActiveColor;
        //buttonBrushExtra3.GetComponent<Image>().color = uiManagerRef.buttonDisabledColor;

        //uiManagerRef.isBrushModeON_snoopingOFF = true; // ***** Switching to brushingMode!!! ***
    }
    
    public void ClickToolButtonExtra3() {
        //Debug.Log("ClickToolButtonExtra3()");
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
    
    [Serializable] public struct ButtonBrush
    {
        public Button button;
        
        private Transform _transform;
        public Transform transform
        {
            get
            {
                if (!_transform)
                    _transform = button.transform;
                
                return _transform;
            }
        }
        
        private Image _image;
        public Image image
        {
            get
            {
                if (!_image)
                    _image = transform.GetComponent<Image>();
                    
                return _image;
            }
        }
    }
}