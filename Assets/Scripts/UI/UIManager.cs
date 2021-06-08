using System.Collections.Generic;
using Playcraft;
using UnityEngine;
using UnityEngine.UI;

public enum ToolType {
    None,
    Add,
    Stir
}

public class UIManager : Singleton<UIManager> {

    #region attributes
    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool genomePool => simulationManager.masterGenomePool;
    TrophicLayersManager trophicLayersManager => simulationManager.trophicLayersManager;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;

    public SpeciesOverviewUI speciesOverviewUI;  //***EAC to be phased-out --> incorporated into WorldTreePanel
    public SpeciesGraphPanelUI speciesGraphPanelUI;  //***EAC to be phased-out --> incorporated into WorldTreePanel
    public GenomeViewerUI genomeViewerUI;
    public BrainGenomeImage brainGenomeImage;
    public PanelTopLeftUI panelTopLeftUI;
    public WorldTreePanelUI worldTreePanelUI;
    public BigBangPanelUI bigBangPanelUI;
    public ClockPanelUI clockPanelUI;
    public ObserverModeUI observerModeUI;
    public DebugPanelUI debugPanelUI;
    public WorldSpiritHubUI worldSpiritHubUI; // ***EAC remove after cannibalizing this! (tied into initialization atm)
    public BrushesUI brushesUI;
    public GlobalResourcesUI globalResourcesUI;
    public CreatureBrainActivityUI creatureBrainActivityUI;
    public CreaturePaperDollUI creaturePaperDollUI;
    public CreatureLifeEventsLogUI creatureLifeEventsLogUI;
    public MinimapPanel minimapUI;      
        
    public GameOptionsManager gameOptionsManager;    
    
    Lookup lookup => Lookup.instance;

    public float isPlantParticleHighlight;
    public float isZooplanktonHighlight;
    public float isVertebrateHighlight;
       
    public bool updateTerrainAltitude;  // WPP: assigned but not used
    public float terrainUpdateMagnitude;// WPP: assigned but not used
    
    public ToolType curActiveTool;  // ***EAC move to phase out this approach
    
    // announcements:
    public PanelNotificationsUI panelNotificationsUI;
    private bool announceAlgaeCollapsePossible = false;
    private bool announceAlgaeCollapseOccurred = false;
    public int timerAnnouncementTextCounter = 0;// WPP: assigned but not used
    public bool isAnnouncementTextOn = false;   // WPP: assigned but not used
    public bool isUnlockCooldown = false;       // WPP: assigned but not used
    public int unlockCooldownCounter = 0;
    //private bool inspectToolUnlockedAnnounce = false;
    //TrophicSlot unlockedAnnouncementSlotRef;    // WPP: assigned but not used

    public GameObject panelLoading;
    public GameObject panelPlaying;

    //public GameObject prefabGenomeIcon;
    public GameObject panelHallOfFameGenomes;

    //public float loadingProgress = 0f;

    private int[] displaySpeciesIndicesArray;
      
    public CandidateAgentData focusedCandidate; // *** TRANSITION TO THIS?
    public int selectedSpeciesID;
    //public int hoverAgentID;

    private const int maxDisplaySpecies = 32;

    private float curSpeciesStatValue;

    public bool isBrushAddingAgents = false; // WPP: assigned but not used
    public int brushAddAgentCounter = 0;
    public int framesPerAgentSpawn = 3;

    [SerializeField] MainMenuUI mainMenu;
    [SerializeField] int timeStepsToRebuildGenomeButtons = 111;
    SpeciesGenomePool pool;
    const string ANIM_FINISHED = "_AnimFinished";
    List<SpeciesGenomePool> speciesPools => genomePool.completeSpeciesPoolsList;
    bool isRebuildTimeStep => simulationManager.simAgeTimeSteps % timeStepsToRebuildGenomeButtons == 1;


    // ***EAC -- Reorganize these to their appropriate locations once you have it working:
    //private bool isTopLeftPanelOpen;
    //private bool isSpeciesListOn;
    //private bool isGraphModeOn;
    //private bool isSpeciesOverview;
    
    #endregion
    
    public void CycleFocusedCandidateGenome()
    {
        int curSpeciesID = selectedSpeciesID + 1;

        if(curSpeciesID >= simulationManager.masterGenomePool.speciesPoolCount) {
            curSpeciesID = 0;
        }
    
        var candidate = simulationManager.masterGenomePool.completeSpeciesPoolsList[curSpeciesID].representativeCandidate;
        SetFocusedCandidateGenome(candidate);
    }
    
    public void ResetCurrentFocusedCandidateGenome() { SetFocusedCandidateGenome(focusedCandidate); }
    
    public void SetFocusedCandidateGenome(SpeciesGenomePool selectedPool, SelectionGroup group, int index) {
        var candidate = selectedPool.GetFocusedCandidate(group, index);
        SetFocusedCandidateGenome(candidate);
    }

    public void SetFocusedCandidateGenome(CandidateAgentData candidate) {
        focusedCandidate = candidate;
        
        theRenderKing.InitializeCreaturePortrait(focusedCandidate.candidateGenome);
        Debug.Log("SetFocusedCandidateGenome --> theRenderKing.InitializeCreaturePortrait(focusedCandidate.candidateGenome);");
        
        speciesOverviewUI.RebuildGenomeButtons();
        brainGenomeImage.SetTexture(focusedCandidate.candidateGenome.brainGenome);

        SetSelectedSpeciesUI(focusedCandidate.speciesID);
    }
    
    public bool IsFocus(CandidateAgentData candidate) { return candidate.candidateID == focusedCandidate.candidateID; }
    
    public void SetSelectedSpeciesUI(int id) {
        if(id == selectedSpeciesID) {
            // already selected -->
            worldTreePanelUI.ToggleFocusLevel(); // species overview vs world overview
        }
        else {

        }
        selectedSpeciesID = id;
        worldTreePanelUI.RefreshPanelUI();
        speciesOverviewUI.RebuildGenomeButtons();
    }

    public void BeginAnnouncement()
    {
        isAnnouncementTextOn = true;
        timerAnnouncementTextCounter = 0;        
    }

    #region Initialization Functions:::
    
    
    public void InitialUnlocks() {
        Debug.Log("InitialUnlocks WATER UNLOCKED!!! " + unlockCooldownCounter); // + ", " + BigBangPanelUI.bigBangFramesCounter.ToString());

        focusedCandidate = simulationManager.masterGenomePool.completeSpeciesPoolsList[0].candidateGenomesList[0];
        
        theRenderKing.baronVonTerrain.IncrementWorldRadius(5.7f);

        worldSpiritHubUI.isUnlocked = true;
        worldSpiritHubUI.OpenWorldTreeSelect();

        UnlockBrushes();   
        
        // WPP: unlockedAnnouncementSlotRef assigned but never used, 
        // get slot from manager, set status directly
        var algae = trophicLayersManager.GetSlot(KnowledgeMapId.Algae);
        worldSpiritHubUI.selectedWorldSpiritSlot = algae;//trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        brushesUI.selectedEssenceSlot = algae; //trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        //trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status = TrophicSlotStatus.On;
        algae.status = TrophicSlotStatus.On;
        worldSpiritHubUI.ClickWorldCreateNewSpecies(algae);

        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
        //trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlotStatus.On;
        var decomposers = trophicLayersManager.GetSlot(KnowledgeMapId.Decomposers);
        decomposers.status = TrophicSlotStatus.On;
        worldSpiritHubUI.ClickWorldCreateNewSpecies(decomposers);
        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
        //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);
        
        //trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlotStatus.On;
        var plants = trophicLayersManager.GetSlot(KnowledgeMapId.Plants);
        plants.status = TrophicSlotStatus.On;
        worldSpiritHubUI.ClickWorldCreateNewSpecies(plants);
                
        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
        //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0]);
        //trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlotStatus.On;
                
        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
        //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0]);
        var microbes = trophicLayersManager.GetSlot(KnowledgeMapId.Microbes);
        microbes.status = TrophicSlotStatus.On;
        worldSpiritHubUI.ClickWorldCreateNewSpecies(microbes);
        
        //trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlotStatus.On;
        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];                
        //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0]);
        //trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
        //trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlotStatus.On;                
        //Debug.Log("CREATURE A UNLOCKED!!! " + unlockCooldownCounter);
        var animals = trophicLayersManager.GetSlot(KnowledgeMapId.Animals);
        animals.status = TrophicSlotStatus.On;
        worldSpiritHubUI.ClickWorldCreateNewSpecies(animals);
                
        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];                
        //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0]);
        theRenderKing.InitializeCreaturePortrait(simulationManager.masterGenomePool.completeSpeciesPoolsList[0].foundingCandidate.candidateGenome); //, gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][mutationUI.selectedToolbarMutationID].representativeGenome);
        
        //TrophicSlot mineralSlot = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[0];
        //mineralsSlot.status = TrophicSlotStatus.On;
        trophicLayersManager.SetSlotStatus(KnowledgeMapId.Nutrients, TrophicSlotStatus.On);

        //mutationUI.isUnlocked = true;
        //mutationUI.ClickToolButton();
        //worldSpiritHubUI.OpenWorldTreeSelect();
                
        //unlockedAnnouncementSlotRef = mineralSlot;
        //TrophicSlot pebblesSlot = trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2];
        //pebblesSlot.status = TrophicSlotStatus.On;
        //Debug.Log("PEBBLES SPIRIT UNLOCKED!!! " + unlockCooldownCounter);
        trophicLayersManager.SetSlotStatus(KnowledgeMapId.Pebbles, TrophicSlotStatus.On);

        //unlockedAnnouncementSlotRef = pebblesSlot;
        //TrophicSlot sandSlot = trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3];
        //sandSlot.status = TrophicSlotStatus.On;
        trophicLayersManager.SetSlotStatus(KnowledgeMapId.Sand, TrophicSlotStatus.On);

        //unlockedAnnouncementSlotRef = sandSlot;
        //TrophicSlot airSlot = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[2];
        //airSlot.status = TrophicSlotStatus.On;
        trophicLayersManager.SetSlotStatus(KnowledgeMapId.Wind, TrophicSlotStatus.On);
        //Debug.Log("AIR SPIRIT UNLOCKED!!! " + unlockCooldownCounter);
        //AnnounceUnlockAir();
        
        isUnlockCooldown = true;
        //unlockedAnnouncementSlotRef = airSlot;
        //trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
        //trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlotStatus.On;                
                
        //AnnounceUnlockVertebrates();
                
        //unlockedAnnouncementSlotRef = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];                
        //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1]);

        //worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];
        //brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];

        // SPAWNS!!!! 
        simulationManager.AttemptToBrushSpawnAgent(brushesUI.selectedEssenceSlot.linkedSpeciesID);

        worldSpiritHubUI.ClickButtonWorldSpiritHubAgent(lookup.GetTrophicSlotData(KnowledgeMapId.Animals));

        worldTreePanelUI.InitializeSpeciesIcons();
    }
    
    public void TransitionToNewGameState(GameState gameState) {
        mainMenu.gameObject.SetActive(gameState == GameState.MainMenu);
    
        // * Remove: replace with delegation
        switch (gameState) {
            case GameState.MainMenu: break;
            case GameState.Loading:
                EnterLoadingUI();
                break;
            case GameState.Playing:
                //canvasMain.renderMode = RenderMode.ScreenSpaceCamera;
                //firstTimeStartup = false;
                EnterPlayingUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameState + ")");
                break;
        }
    }
        
    private void EnterLoadingUI() {
        panelLoading.SetActive(true);
        panelPlaying.SetActive(false);
    }
    
    private void EnterPlayingUI() {   //// ******* this happens everytime quit to menu and resume.... *** needs to change!!! ***
        panelLoading.SetActive(false);
        panelPlaying.SetActive(true);

        Debug.Log("EnterPlayingUI() " + Time.timeScale);
        //Animation Big Bang here
        simulationManager._BigBangOn = true;
        //worldSpiritHubUI.OpenWorldTreeSelect();

        SimEventData newEventData = new SimEventData();
        newEventData.name = "New Simulation Start!";
        newEventData.category = SimEventData.SimEventCategories.NPE;
        newEventData.timeStepActivated = 0;
        simulationManager.simEventsManager.completeEventHistoryList.Add(newEventData);

        panelNotificationsUI.Narrate("... And Then There Was Not Nothing ...", new Color(0.75f, 0.75f, 0.75f));
        //panelPendingClickPrompt.GetComponentInChildren<Text>().text = "... And Then There Was Not Nothing ...";// "Welcome! This Pond is devoid of life...\nIt's up to you to change that!";
        //panelPendingClickPrompt.GetComponentInChildren<Text>().color = new Color(0.75f, 0.75f, 0.75f);
        //panelPendingClickPrompt.GetComponent<Image>().raycastTarget = false;
        //isAnnouncementTextOn = true;
        //timerAnnouncementTextCounter = 0;

    }
    #endregion

    #region UPDATE UI PANELS FUNCTIONS!!! :::
    void Update() {                                        // ***EC CLEAN THIS CRAP UP
        if(!simulationManager.loadingComplete) return;
        bigBangPanelUI.Tick();
        if(simulationManager._BigBangOn) return;
        
        observerModeUI.Tick();  // <== this is the big one *******  
        // ^^^  Need to Clean this up and replace with better approach ***********************        
        theCursorCzar.UpdateCursorCzar();  // this will assume a larger role
        brushesUI.UpdateBrushesUI();        
        
        if(focusedCandidate != null && focusedCandidate.candidateGenome != null) {
            genomeViewerUI.UpdateUI();
            creatureBrainActivityUI.Tick();
            creaturePaperDollUI.Tick();
            creatureLifeEventsLogUI.Tick(focusedCandidate);

            if(simulationManager.simAgeTimeSteps % 67 == 1) { //***EAC still needed?
                speciesOverviewUI.RebuildGenomeButtons();  
            }
        }

        worldTreePanelUI.UpdateUI();
        clockPanelUI.Tick();
        minimapUI.Tick();
        debugPanelUI.UpdateDebugUI();
    }

    public void SetFocus()
    {
        pool = speciesPools[selectedSpeciesID];
        
        if(focusedCandidate != null && focusedCandidate.candidateGenome != null) {
            genomeViewerUI.UpdateUI();

            if(isRebuildTimeStep) {
                speciesOverviewUI.RebuildGenomeButtons();  
            }
        } 
    }
    
    /*
    private void UpdateSimulationUI() {

        UpdateBigBangPanel();
        
        UpdateObserverModeUI();  // <== this is the big one *******  
        // ^^^  Need to Clean this up and replace with better approach ***********************
        
        theCursorCzar.UpdateCursorCzar();  // this will assume a larger role

        brushesUI.UpdateBrushesUI();
        watcherUI.UpdateWatcherPanelUI(trophicLayersManager);
        knowledgeUI.UpdateKnowledgePanelUI(trophicLayersManager);
        mutationUI.UpdateMutationPanelUI(trophicLayersManager);
        //worldSpiritHubUI.UpdateWorldSpiritHubUI();
        globalResourcesUI.UpdateGlobalResourcesPanelUpdate();
        featsUI.UpdateFeatsPanelUI(gameManager.simulationManager.featsList);
        UpdateClockPanelUI();

        SpeciesGenomePool pool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        if(focusedCandidate != null) {
            genomeViewerUI.UpdateUI(pool, focusedCandidate);

            if(gameManager.simulationManager.simAgeTimeSteps % 111 == 1) {
                speciesOverviewUI.RebuildGenomeButtons();  
            }
        }
        

        // **** Temp panel display stuff:
 
        if(wildSpirit.isClickableSpiritRoaming) {
            wildSpirit.UpdateWildSpiritProto();
        }
        else {
            wildSpirit.framesSinceLastClickableSpirit++;
            wildSpirit.protoSpiritClickColliderGO.SetActive(false);
        }

        if(animatorSpiritUnlock.GetBool("_AnimFinished")) {
            SpiritUnlockComplete();
            animatorSpiritUnlock.SetBool("_AnimFinished", false);
        }
        
        debugPanelUI.UpdateDebugUI();
              
        UpdatePausedUI();        
    }
    */

    // * REFACTOR: delegate to something else
    /*public void SpiritUnlockComplete() {
        Debug.LogError("SPIRIT UNLOCK!!!  world size increase!");
        theRenderKing.baronVonTerrain.IncrementWorldRadius(0.7f);

        switch (wildSpirit.curClickableSpiritType) {
            case WildSpirit.ClickableSpiritType.CreationBrush:
                UnlockBrushes();   
                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                break;
            case WildSpirit.ClickableSpiritType.Water:
                trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status = TrophicSlot.SlotStatus.On;
                Debug.Log("WATER UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //brushesUI.Unlock();
                //brushesUI.SetTargetFromWorldTree();
                //AnnounceUnlockWater();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[1];
                            
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Decomposers;
                break;
            case WildSpirit.ClickableSpiritType.Decomposers:
                trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                Debug.Log("DECOMPOSERS UNLOCKED!!! " + unlockCooldownCounter.ToString());

                //AnnounceUnlockDecomposers();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
                worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);

                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                            
                //isClickableSpiritRoaming = false;
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.KnowledgeSpirit;
                break;
            case WildSpirit.ClickableSpiritType.KnowledgeSpirit:
                knowledgeUI.isUnlocked = true;
                //AnnounceUnlockKnowledgeSpirit();
                knowledgeUI.OpenKnowledgePanel();
                worldSpiritHubUI.OpenWorldTreeSelect();
                //isClickableSpiritRoaming = false;
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Algae;
                break;
            case WildSpirit.ClickableSpiritType.Algae:
                trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                //Debug.Log("ALGAE UNLOCKED!!! " + unlockCooldownCounter.ToString());

                //AnnounceUnlockAlgae();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];   
                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0]);
                            
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                //isClickableSpiritRoaming = false;
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Zooplankton;
                break;
            case WildSpirit.ClickableSpiritType.Zooplankton:
                trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                //Debug.Log("ZOOPLANKTON UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockZooplankton();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0]);
                            
                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                            
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.WatcherSpirit;
                break;
            case WildSpirit.ClickableSpiritType.WatcherSpirit:
                watcherUI.isUnlocked = true;
                watcherUI.ClickToolButton();
                panelFocus = PanelFocus.Watcher;
                //watcherUI.animatorWatcherUI.SetBool("_IsOpen", true);
                //AnnounceUnlockWatcherSpirit();
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Plants;
                break;
            case WildSpirit.ClickableSpiritType.Plants:
                trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                //Debug.Log("PLANTS UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockPlants();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0]);

                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];

                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.VertA;
                break;
            case WildSpirit.ClickableSpiritType.VertA:                            
                trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
                trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.On;                
                //Debug.Log("CREATURE A UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockVertebrates();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0]);

                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];

                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.MutationSpirit;
                break;
            case WildSpirit.ClickableSpiritType.MutationSpirit:
                mutationUI.isUnlocked = true;
                //AnnounceUnlockMutationSpirit();
                //curClickableSpiritType = UIManager.ClickableSpiritType.Zooplankton; // *** Last unlock?
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Minerals;
                mutationUI.ClickToolButton();
                worldSpiritHubUI.OpenWorldTreeSelect();

                //gameManager.theRenderKing.InitializeCreaturePortraitGenomes();// gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1].representativeGenome, gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][mutationUI.selectedToolbarMutationID].representativeGenome);
                //Debug.LogError("InitializeNewCritterPortraitGenome");
                break;
            case WildSpirit.ClickableSpiritType.Minerals:
                TrophicSlot mineralSlot = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[0];
                mineralSlot.status = TrophicSlot.SlotStatus.On;
                Debug.Log("MINERALS UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockMinerals();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = mineralSlot;
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
                //worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);
                worldSpiritHubUI.selectedWorldSpiritSlot = mineralSlot;
                brushesUI.selectedEssenceSlot = mineralSlot;
                            
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Pebbles;
                break;
            case WildSpirit.ClickableSpiritType.Pebbles:
                TrophicSlot pebblesSlot = trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2];
                pebblesSlot.status = TrophicSlot.SlotStatus.On;
                //Debug.Log("PEBBLES SPIRIT UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockPebbles();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = pebblesSlot;
                worldSpiritHubUI.selectedWorldSpiritSlot = pebblesSlot;
                worldSpiritHubUI.selectedToolbarTerrainLayer = 2;

                //brushesUI.selectedEssenceSlot = pebblesSlot;
                //brushesUI.selectedBrushLinkedSpiritTerrainLayer = 2; // worldSpiritHubUI.selectedToolbarTerrainLayer; 
                //brushesUI.ClickButtonBrushPaletteTerrain(2);

                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Sand;
                break;
            case WildSpirit.ClickableSpiritType.Sand:
                TrophicSlot sandSlot = trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3];
                sandSlot.status = TrophicSlot.SlotStatus.On;
                //Debug.Log("SAND SPIRIT UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockSand();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = sandSlot;
                worldSpiritHubUI.selectedWorldSpiritSlot = sandSlot;
                worldSpiritHubUI.selectedToolbarTerrainLayer = 3;

                //brushesUI.selectedEssenceSlot = sandSlot;
                //brushesUI.selectedBrushLinkedSpiritTerrainLayer = 3; 
                //brushesUI.ClickButtonBrushPaletteTerrain(3);
                            
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Air;
                break;
            case WildSpirit.ClickableSpiritType.Air:
                TrophicSlot airSlot = trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[2];
                airSlot.status = TrophicSlot.SlotStatus.On;
                //Debug.Log("AIR SPIRIT UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockAir();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = airSlot;
                worldSpiritHubUI.selectedWorldSpiritSlot = airSlot;
                brushesUI.selectedEssenceSlot = airSlot;
                            
                wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.VertB;
                break;            
            case WildSpirit.ClickableSpiritType.VertB:                            
                trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
                trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.On;                
                //Debug.Log("CREATURE B UNLOCKED!!! " + unlockCooldownCounter.ToString());
                //AnnounceUnlockVertebrates();
                isUnlockCooldown = true;
                unlockedAnnouncementSlotRef = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];                
                worldSpiritHubUI.ClickWorldCreateNewSpecies(trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1]);

                worldSpiritHubUI.selectedWorldSpiritSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];
                brushesUI.selectedEssenceSlot = trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1];
                break;
            default:
                Debug.LogError("No Enum Type Found! (");
                break;
        }

        if(panelFocus != PanelFocus.Brushes) {
            brushesUI.ClickToolButtonAdd();
        }
    }*/
            
    public void CheckForAnnouncements() {
    
        // ***WPP: replace nested conditions with early exit 
        if(!announceAlgaeCollapseOccurred) {
            if(announceAlgaeCollapsePossible) {
                if(simulationManager.simResourceManager.curGlobalPlantParticles < 10f) {
                    announceAlgaeCollapsePossible = false;
                    announceAlgaeCollapseOccurred = true;

                    var decomposersColor = lookup.GetTrophicSlotData(KnowledgeMapId.Decomposers).color;
                    panelNotificationsUI.Narrate("<color=#DDDDDDFF>Algae Died from lack of Nutrients!</color>\nAdd Decomposers to recycle waste", decomposersColor);
                    isAnnouncementTextOn = true;
                }
            }
        }
    }

    public Vector4 SampleTexture(RenderTexture tex, Vector2 uv) {
        Vector4[] sample = new Vector4[1];

        ComputeBuffer outputBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        outputBuffer.SetData(sample);
        int kernelCSSampleTexture = simulationManager.computeShaderResourceGrid.FindKernel("CSSampleTexture");
        simulationManager.computeShaderResourceGrid.SetTexture(kernelCSSampleTexture, "_CommonSampleTex", tex);
        simulationManager.computeShaderResourceGrid.SetBuffer(kernelCSSampleTexture, "outputValuesCBuffer", outputBuffer);
        simulationManager.computeShaderResourceGrid.SetFloat("_CoordX", uv.x);
        simulationManager.computeShaderResourceGrid.SetFloat("_CoordY", uv.y); 
        // DISPATCH !!!
        simulationManager.computeShaderResourceGrid.Dispatch(kernelCSSampleTexture, 1, 1, 1);
        
        outputBuffer.GetData(sample);
        outputBuffer.Release();

        return sample[0];
    }
    
    /*
    public void InitToolbarPortraitCritterData(TrophicSlot slot) { // ** should be called AFTER new species actually created?? something is fucked here
        Debug.Log("InitToolbarPortraitCritterData.. selectedSpeciesID: " + selectedSpeciesID.ToString() + " , slotLinkedID: " + slot.linkedSpeciesID.ToString());

        //SpeciesGenomePool speciesPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        //gameManager.theRenderKing.InitializeNewCritterPortraitGenome(speciesPool.representativeGenome);
        gameManager.theRenderKing.InitializeCreaturePortraitGenomes();// gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slot.slotID].representativeGenome, gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[0][mutationUI.selectedToolbarMutationID].representativeGenome); // speciesPool.leaderboardGenomesList[0].candidateGenome);
        gameManager.theRenderKing.isToolbarCritterPortraitEnabled = true;
         
    }
    */
    
        /*
    public void SetToolbarButtonStateUI(bool isDim, ref Button button, TrophicSlot.SlotStatus slotStatus, bool isSelected) {

        button.gameObject.SetActive(true);
        //Animation anim = button.GetComponent<Animation>();
        Vector3 scale = Vector3.one;
        ColorBlock colBlock = button.colors; //.colorMultiplier = 1f;
        colBlock.colorMultiplier = 1f;
        switch(slotStatus) {
            case TrophicSlot.SlotStatus.Off:
                button.gameObject.SetActive(false); 
                //button.gameObject.transform.localScale = Vector3.one;
                break;
            case TrophicSlot.SlotStatus.Locked:
                button.gameObject.SetActive(false); 
                button.interactable = false; 
                colBlock.colorMultiplier = 0.5f;
                //button.GetComponentInChildren<Text>().text = "";
                //button.gameObject.transform.localScale = Vector3.one;
                break;
            case TrophicSlot.SlotStatus.Unlocked:
                button.gameObject.SetActive(false); 
                button.interactable = false;  
                colBlock.colorMultiplier = 0.8f;
                //button.GetComponentInChildren<Text>().text = "";
                //button.GetComponent<Image>().sprite = spriteSpeciesSlotRing;
                //anim.Play();
                //scale = Vector3.one * 1f;
                break;
            //case TrophicSlot.SlotStatus.Pending:
            //    button.interactable = false;
            //    button.GetComponentInChildren<Text>().text = "%";
            //    break;
            case TrophicSlot.SlotStatus.On:
                button.gameObject.SetActive(true); 
                button.interactable = true;
                colBlock.colorMultiplier = 0.65f;                
                scale = Vector3.one * 0.85f;
                //button.GetComponentInChildren<Text>().text = "";
                //button.GetComponent<Image>().sprite = spriteSpeciesSlotFull;
                //anim.Rewind();
                //anim.Stop();
                
                //
                break;
            //case TrophicSlot.SlotStatus.Selected:
            //    button.interactable = false;
            //    button.GetComponentInChildren<Text>().text = "**";
            //    break;
            default:
                break;
        }
        if(isSelected) {
            scale = Vector3.one * 1.65f;
            colBlock.colorMultiplier = 1.287f;
        }

        if(isDim) {
            button.colors = colBlock;
            button.gameObject.transform.localScale = scale;
            //button.gameObject.SetActive(false);
        }
        else {
            button.colors = colBlock;
            button.gameObject.transform.localScale = scale;
            //button.gameObject.SetActive(true);
        }  
    }
    */
    
    #endregion

    #region UTILITY & EVENT FUNCTIONS

    /*private void RemoveSpeciesSlot() {
                
        TrophicSlot slot = trophicLayersManager.selectedTrophicSlotRef;
        //Debug.Log("RemoveSpeciesSlot( " + slot.slotID.ToString() + " ), k= " + slot.slotID.ToString());

        if(slot.kingdomID == 0) {
            Debug.Log("Remove Decomposers");
            trophicLayersManager.TurnOffDecomposers();
        }
        else if(slot.kingdomID == 1) {  // plant
            Debug.Log("Remove Algae");
            trophicLayersManager.TurnOffAlgae();
            //gameManager.simulationManager.TurnOffAlgae();  // what to do with existing algae???
        }
        else {  // animals
            if(slot.tierID == 0) {
                Debug.Log("Remove Zooplankton");
                trophicLayersManager.TurnOffZooplankton();
            }
            else {  // tier 1
                Debug.Log("Remove AGENT");
                //slot.status
                gameManager.simulationManager.RemoveSelectedAgentSpecies(slot.slotID);
            }
        }

        slot.status = TrophicSlot.SlotStatus.Unlocked;
        
        trophicLayersManager.selectedTrophicSlotRef = trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0]; // Bedrock
    }*/

    #endregion

    #region UI ELEMENT CLICK FUNCTIONS!!!!

    public void ClickLoadingGemStart() {
        Debug.Log("Let there be not nothing!");
        simulationManager._BigBangOn = true;
    }
    
    public void UnlockBrushes() {
        brushesUI.Unlock();
        brushesUI.SetTargetFromWorldTree();
        //wildSpirit.curClickableSpiritType = WildSpirit.ClickableSpiritType.Zooplankton;
        AnnounceUnlockBrushes();
    }

    // *** WPP: convert to NarrationSO
    public void AnnounceBrushAppear() {
        panelNotificationsUI.Narrate("A Minor Creation Spirit Appeared!", new Color(1f, 1f, 1f));
        // map opens!
    }

    public void AnnounceUnlockBrushes() {
        panelNotificationsUI.Narrate("Creation Spirit Captured!", new Color(1f, 1f, 1f));  
        
        Feat feat = new Feat("Brush", FeatType.WorldExpand, Time.frameCount, Color.white, "blah blah blah blah!");
        simulationManager.LogFeat(feat);
        //featsUI.isOpen = true;
    }
    
    public void Narrate(NarrationSO value) 
    { 
        panelNotificationsUI.Narrate(value); 
        
        foreach (var feat in value.feats)
            simulationManager.LogFeat(feat);
    }
   
    public void CheatUnlockAll() {
        Debug.Log("Cheat!!! Unlocked all species!!!");

        trophicLayersManager.CheatUnlockAll();

        //mutationUI.isUnlocked = true;
        //knowledgeUI.isUnlocked = true;
        //watcherUI.isUnlocked = true;
        brushesUI.isUnlocked = true;
    }
    
    #endregion
}



#region OLD OLD OLD!!!!
    
    /*
    public void UpdateInfoPanelUI() {
        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();


        textGlobalMass.text = "Global Biomass: " + gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles).ToString("F0");

        float percentageOxygen = resourcesRef.curGlobalOxygen / 3000f;
        infoMeterOxygenMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageOxygen));
        float percentageNutrients = resourcesRef.curGlobalNutrients / 3000f;
        infoMeterNutrientsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageNutrients));
        float percentageDetritus = resourcesRef.curGlobalDetritus / 3000f;
        infoMeterDetritusMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDetritus));
        float percentageDecomposers = resourcesRef.curGlobalDecomposers / 3000f;
        infoMeterDecomposersMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDecomposers));
        float percentagePlants = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir) / 3000f;
        infoMeterPlantsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentagePlants));
        float percentageAnimals = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles) / 100f;
        infoMeterAnimalsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageAnimals));

        if(isActiveInfoPanel) {
            panelInfoExpanded.SetActive(true);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = ">";

            if(isActiveInfoResourcesTab) {
                panelInfoResourcesOverview.SetActive(true);
                //panelInfoSpeciesOverview.SetActive(false);
            }
            else {  // &&& SPECIES TAB::::
                
            }
        }
        else {
            panelInfoExpanded.SetActive(false);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = "<";
        }
    }    
    */
      
    
    /*
    public void UpdateInfoPanelUI() {
        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();


        textGlobalMass.text = "Global Biomass: " + gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles).ToString("F0");

        float percentageOxygen = resourcesRef.curGlobalOxygen / 3000f;
        infoMeterOxygenMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageOxygen));
        float percentageNutrients = resourcesRef.curGlobalNutrients / 3000f;
        infoMeterNutrientsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageNutrients));
        float percentageDetritus = resourcesRef.curGlobalDetritus / 3000f;
        infoMeterDetritusMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDetritus));
        float percentageDecomposers = resourcesRef.curGlobalDecomposers / 3000f;
        infoMeterDecomposersMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageDecomposers));
        float percentagePlants = (resourcesRef.curGlobalPlantParticles + resourcesRef.curGlobalAlgaeReservoir) / 3000f;
        infoMeterPlantsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentagePlants));
        float percentageAnimals = (resourcesRef.curGlobalAgentBiomass + resourcesRef.curGlobalAnimalParticles) / 100f;
        infoMeterAnimalsMat.SetFloat("_FillPercentage", Mathf.Sqrt(percentageAnimals));

        if(isActiveInfoPanel) {
            panelInfoExpanded.SetActive(true);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = ">";

            if(isActiveInfoResourcesTab) {
                panelInfoResourcesOverview.SetActive(true);
                //panelInfoSpeciesOverview.SetActive(false);
            }
            else {  // &&& SPECIES TAB::::
                
            }
        }
        else {
            panelInfoExpanded.SetActive(false);
            buttonInfoOpenClose.GetComponentInChildren<Text>().text = "<";
        }
    }    
    */
      
    
    //public void UpdateToolbarPanelUI() {

        //if (true) { }
        /*simManager.uiManager.AnnounceUnlockAlgae();
            simManager.uiManager.isUnlockCooldown = true;
            simManager.uiManager.unlockedAnnouncementSlotRef = kingdomPlants.trophicTiersList[0].trophicSlots[0];
            simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            simManager.uiManager.buttonToolbarExpandOn.interactable = true;)
            */
        /*if(!inspectToolUnlocked) {
            //if(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass > 0f) {
                // Unlock!!!!
                AnnounceUnlockInspect();
                inspectToolUnlocked = true;
                
            //}
        }*/

        // Check for Announcements:
        //CheckForAnnouncements();
        
    //}

 /*public void ClickAnnouncementText() {
        Debug.Log("ClickAnnouncementText");

        //if blah -- which species unlocked?
        //is toolbar on?
                
        if(inspectToolUnlockedAnnounce) { // *** GROSS HACK!!!

            ClickToolButtonInspect();
            inspectToolUnlockedAnnounce = false;
        }
        else {
            if(!isToolbarPaletteExpandOn) {
                ClickToolbarExpandOn();
            }

            if(unlockedAnnouncementSlotRef.kingdomID == 0) {
                ClickButtonToolbarDecomposers();
            }
            else if(unlockedAnnouncementSlotRef.kingdomID == 1) {
                ClickButtonToolbarAlgae();
            }
            else {
                if(unlockedAnnouncementSlotRef.tierID == 0) {
                    ClickButtonToolbarZooplankton();
                }
                else {
                    ClickButtonToolbarAgent(unlockedAnnouncementSlotRef.slotID);
                }
            } 
        }
    }
    */

/*public void UpdateTreeOfLifeWidget() {
        if(tolWorldStatsOn) {
            float displayVal = 0f;
            displayVal = 0f; // tolWorldStatsValueRangesKeyArray[tolSelectedWorldStatsIndex].y;

            //textTolWorldStatsValue.text = displayVal.ToString();
            if(tolSelectedWorldStatsIndex == 0) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalOxygen;
            }
            else if(tolSelectedWorldStatsIndex == 1) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalNutrients;
            }
            else if(tolSelectedWorldStatsIndex == 2) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalDetritus;
            }
            else if(tolSelectedWorldStatsIndex == 3) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalDecomposers;
            }
            else if(tolSelectedWorldStatsIndex == 4) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAlgaeReservoir;
            }
            else if(tolSelectedWorldStatsIndex == 5) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAlgaeParticles;
            }
            else if(tolSelectedWorldStatsIndex == 6) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles;
            }
            else if(tolSelectedWorldStatsIndex == 7) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass; // gameManager.simulationManager.settingsManager.curTierBodyMutationFrequency;
            }
            else if(tolSelectedWorldStatsIndex == 8) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalCarrionVolume;
            }
            else if(tolSelectedWorldStatsIndex == 9) {
                displayVal = gameManager.simulationManager.simResourceManager.curGlobalEggSackVolume;
            }
            else if(tolSelectedWorldStatsIndex == 10) {
                displayVal = gameManager.simulationManager.environmentFluidManager.curTierWaterCurrents;
            }
            //textTolWorldStatsValue.text = displayVal.ToString();
            curWorldStatValue = displayVal;

            UpdateTolWorldStatsTextReadout();
            
        }
        if(tolEventsTimelineOn) {
            if(gameManager.simulationManager.simEventsManager.completeEventHistoryList.Count > 0) {
                textTolEventsTimelineName.text = "Year: " + Mathf.FloorToInt((float)gameManager.simulationManager.simEventsManager.completeEventHistoryList[curClosestEventToCursor].timeStepActivated / 2000f).ToString() +
                ":   " + gameManager.simulationManager.simEventsManager.completeEventHistoryList[curClosestEventToCursor].name; // + " (" +  gameManager.simulationManager.simAgeTimeSteps.ToString();
                
            }
            else {

            }
            
            
        }
        else {

        }

        textTolYearValue.text = gameManager.simulationManager.curSimYear.ToString();

        imageTolBackdropGraphs.gameObject.SetActive(tolEventsTimelineOn);
        imageTolBackdropDescription.gameObject.SetActive(tolSpeciesDescriptionOn);
    }*/

/*public void UpdateHUDUI() {
        if(isObserverMode) {
            panelHUD.SetActive(false);
        }
        else {
            panelHUD.SetActive(true);
        }        
    }*/

/*private void UpdateStatsPanelUI() {
        buttonGraphLifespan.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphBodySizes.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphConsumption.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphPredation.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphNutrients.GetComponent<Image>().color = buttonDisabledColor;
        buttonGraphMutation.GetComponent<Image>().color = buttonDisabledColor;

        textStatsGraphLegend.text = "<color=#ff0000ff>Species A -----</color>\n\n<color=#00ff00ff>Species B -----</color>\n\n<color=#0000ffff>Species C -----</color>\n\n<color=#ffffffff>Species D -----</color>";
        
        if(curStatsGraphMode == GraphMode.Lifespan) {
            //statsGraphMatLifespan.SetFloat("_MaxValue", maxLifespanValue);            
            statsPanelTextMaxValue.text = maxLifespanValue.ToString("F0");
            textStatsGraphTitle.text = "Average Lifespan Over Time:";
            buttonGraphLifespan.GetComponent<Image>().color = buttonActiveColor;            
            imageStatsGraphDisplay.material = statsGraphMatLifespan;
        }

        if(curStatsGraphMode == GraphMode.BodySizes) {
            //statsGraphMatBodySizes.SetFloat("_MaxValue", maxBodySizeValue);            
            statsPanelTextMaxValue.text = maxBodySizeValue.ToString("F2");
            textStatsGraphTitle.text = "Average Creature Body Sizes Over Time:";
            buttonGraphBodySizes.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatBodySizes;
        }

        if(curStatsGraphMode == GraphMode.Consumption) {
            //statsGraphMatFoodEaten.SetFloat("_MaxValue", maxFoodEatenValue);
            statsPanelTextMaxValue.text = maxFoodEatenValue.ToString("F4");
            textStatsGraphTitle.text = "Average Food Eaten Over Time:";
            buttonGraphConsumption.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatFoodEaten;
        }

        if(curStatsGraphMode == GraphMode.Predation) {
            //statsGraphMatPredation.SetFloat("_MaxValue", maxPredationValue);
            statsPanelTextMaxValue.text = maxPredationValue.ToString("F2");
            textStatsGraphTitle.text = "Average Predatory Behavior Over Time:";
            buttonGraphPredation.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatPredation;
        }
        
        if(curStatsGraphMode == GraphMode.Nutrients) {
            statsGraphMatNutrients.SetFloat("_MaxValue", maxNutrientsValue);            
            statsPanelTextMaxValue.text = maxNutrientsValue.ToString("F2");
            textStatsGraphTitle.text = "Global Nutrient Levels Over Time:";
            buttonGraphNutrients.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatNutrients;

            // ** CHANGE GRAPH LEGEND!!! ***
            textStatsGraphLegend.text = "<color=#ff8800ff>Small -----</color>\n\n<color=#88ff00ff>Medium -----</color>";
        }

        if(curStatsGraphMode == GraphMode.Mutation) {
            statsGraphMatMutation.SetFloat("_MaxValue", maxMutationValue);            
            statsPanelTextMaxValue.text = maxMutationValue.ToString("F2");
            textStatsGraphTitle.text = "Average Mutation Rates Over Time:";
            buttonGraphMutation.GetComponent<Image>().color = buttonActiveColor;
            imageStatsGraphDisplay.material = statsGraphMatMutation;

            textStatsGraphLegend.text = "<color=#ffffffff>Mutation -----</color>";
        }

        int numBorn = gameManager.simulationManager.numAgentsBorn + gameManager.simulationManager._NumAgents;
        textStatsCurGen.text = "Generation:\n" + gameManager.simulationManager.curApproxGen.ToString() + "\n\n# Born:\n" + numBorn.ToString();
    }
    */

/*private void UpdateFeedToolPanelUI() {
        if(foodToolSprinkleOn) {
            buttonFeedToolSprinkle.GetComponent<Image>().color = buttonActiveColor;
        }
        else {
            buttonFeedToolSprinkle.GetComponent<Image>().color = buttonDisabledColor;
        }

        if(foodToolPourOn) {
            buttonFeedToolPour.GetComponent<Image>().color = buttonActiveColor;
        }
        else {
            buttonFeedToolPour.GetComponent<Image>().color = buttonDisabledColor;
        }
    }
    private void UpdateMutateToolPanelUI() {


    }*/

/*public void UpdateStatsTextureNutrients(List<Vector4> data) {
        statsTextureNutrients.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i].x);
            maxValue = Mathf.Max(maxValue, data[i].y);
            maxValue = Mathf.Max(maxValue, data[i].z);
            maxValue = Mathf.Max(maxValue, data[i].w);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureNutrients.SetPixel(i, 0, new Color(data[i].x, data[i].y, data[i].z, data[i].w));
        }
        statsTextureNutrients.Apply();

        maxNutrientsValue = maxValue;    
    }
    public void UpdateStatsTextureMutation(List<float> data) {
        statsTextureMutation.Resize(Mathf.Max(1, data.Count), 1);

        // Find Score Range:
        float maxValue = 0.00001f;
        for (int i = 0; i < data.Count; i++) {
            maxValue = Mathf.Max(maxValue, data[i]);
        }
        for(int i = 0; i < data.Count; i++) {
            statsTextureMutation.SetPixel(i, 0, new Color(data[i], data[i], data[i], data[i]));
        }
        statsTextureMutation.Apply();

        maxMutationValue = maxValue;  
    }
    */
    /*public void UpdateScoreText(int score) {
        textScore.text = "Score: " + score.ToString();
    }*/

/*
    public void ClickToolsButtonOpenClose() {
        animatorToolsPanel.enabled = true;
        if(isActiveToolsPanel) {
            Debug.Log("ClickToolsButton - deactivate"); 
            isActiveToolsPanel = false;
            animatorToolsPanel.Play("SlideOffPanelTools");
            buttonToolOpenClose.GetComponent<Image>().color = buttonDisabledColor;
        }
        else {
            Debug.Log("ClickToolsButton - activate");
            isActiveToolsPanel = true;
            animatorToolsPanel.Play("SlideOnPanelTools");

            buttonToolOpenClose.GetComponent<Image>().color = buttonActiveColor;
        }
    }
    public void ClickStatsButton() {
        //Debug.Log("ClickStatsButton");
        animatorStatsPanel.enabled = true;
        if(isActiveStatsPanel) {
            Debug.Log("ClickStatsButton - deactivate");
            isActiveStatsPanel = false;
            animatorStatsPanel.Play("SlideOffPanelStats");
            buttonStats.GetComponent<Image>().color = buttonDisabledColor;
        }
        else {
            Debug.Log("ClickStatsButton - activate");
            isActiveStatsPanel = true;
            animatorStatsPanel.Play("SlideOnPanelStats");
            buttonStats.GetComponent<Image>().color = buttonActiveColor;
        }
    }
    public void ClickGraphButtonLifespan() {
        if (curStatsGraphMode != GraphMode.Lifespan) {
            curStatsGraphMode = GraphMode.Lifespan;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonBodySizes() {
        if (curStatsGraphMode != GraphMode.BodySizes) {
            curStatsGraphMode = GraphMode.BodySizes;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonFoodEaten() {
        if (curStatsGraphMode != GraphMode.Consumption) {
            curStatsGraphMode = GraphMode.Consumption;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonPredation() {
        if (curStatsGraphMode != GraphMode.Predation) {
            curStatsGraphMode = GraphMode.Predation;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonNutrients() {
        if (curStatsGraphMode != GraphMode.Nutrients) {
            curStatsGraphMode = GraphMode.Nutrients;
            UpdateStatsPanelUI();
        }
    }
    public void ClickGraphButtonMutation() {
        if (curStatsGraphMode != GraphMode.Mutation) {
            curStatsGraphMode = GraphMode.Mutation;
            UpdateStatsPanelUI();
        }
    }
    */

    /*public void ClickFeedToolSprinkle() {
        
        if(foodToolSprinkleOn) {
            // already on
        }
        else {
            foodToolSprinkleOn = true;
            foodToolPourOn = false;
            Debug.Log("ClickFeedToolSprinkle()! foodToolSprinkleOn = true");
        }
        UpdateFeedToolPanelUI();
    }
    public void ClickFeedToolPour() {
        
        if(foodToolPourOn) {
            // already on
        }
        else {
            foodToolPourOn = true;
            foodToolSprinkleOn = false;
            Debug.Log("ClickFeedToolPour()! foodToolPourOn = true");
        }
        UpdateFeedToolPanelUI();
    }
    public void ClickFeedToolRegrowthSlider(float val) {
        Debug.Log("ClickFeedToolRegrowthSlider()!");
    }
    public void ClickMutateToolGlobalRateSlider(float val) {
        Debug.Log("ClickMutateToolGlobalRateSlider(" + val.ToString() + ")!");
        //gameManager.simulationManager.curPlayerMutationRate = val;
        //gameManager.simulationManager.ChangeGlobalMutationRate(val); // normalizedVal);
    }
    public void ClickToolButtonMutate() {
            //trophicLayersManager.ResetSelectedAgentSlots();
        //if(curActiveTool != ToolType.Mutate) {
            curActiveTool = ToolType.Mutate;

            isActiveMutateToolPanel = true;    
            //animatorMutateToolPanel.enabled = true;
            //animatorMutateToolPanel.Play("SlideOnPanelMutateTool"); 
            buttonToolbarMutate.GetComponent<Image>().color = buttonActiveColor; 
                        
            TurnOffInspectTool();
            TurnOffStirTool();
            TurnOffNutrientsTool();
            TurnOffRemoveTool();
            // *** make these their own private functions for reuse: OLD:::
            //buttonToolStir.GetComponent<Image>().color = buttonDisabledColor;  
            //buttonToolFeed.GetComponent<Image>().color = buttonDisabledColor;
        //}            
    }
    */

/*public void ClickBackToMainMenu() {
        optionsMenuOn = false;
        UpdateMainMenuUI();
    }*/

/*public void ClickResetWorld() {
        Debug.Log("Reset The World!");
        gameManager.simulationManager.ResetWorld();
    }*/

/*public void ClickButtonToggleHUD() {
        isActiveHUD = !isActiveHUD;
        panelHUD.SetActive(isActiveHUD);
    }*/

/*public void ClickPrevSpecies() {
        Debug.Log("ClickPrevSpecies");
        int newIndex = cameraManager.targetCritterIndex - gameManager.simulationManager._NumAgents / 4;
        if(newIndex < 0) {
            newIndex = newIndex + gameManager.simulationManager._NumAgents;                    
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);      
    }
    public void ClickNextSpecies() {
        Debug.Log("ClickNextSpecies");
        int newIndex = cameraManager.targetCritterIndex + gameManager.simulationManager._NumAgents / 4;
        if(newIndex >= gameManager.simulationManager._NumAgents) {
            newIndex = newIndex - gameManager.simulationManager._NumAgents;                      
        }
        cameraManager.SetTarget(gameManager.simulationManager.agentsArray[newIndex], newIndex);    
    }*/

/*public void ClickTreeOfLifeGroupOnOff() {
        treeOfLifePanelOn = !treeOfLifePanelOn;

        // Update panel:
        if(treeOfLifePanelOn) {
            // Update treeOfLife colliders & display   
            UpdateSpeciesTreeDataTextures(gameManager.simulationManager.curSimYear);
            RecalculateTreeOfLifeGraphPanelSizes();
            UpdateTolSpeciesColorUI();
            //ClickToolButtonInspect();
            //gfhdf
        }
        else {
            //ClickToolButtonStir();
        }
        
        //panelTreeOfLifeScaleHideGroup.SetActive(treeOfLifePanelOn);
        panelTolMaster.SetActive(treeOfLifePanelOn);

        treeOfLifeManager.UpdateVisualUI(treeOfLifePanelOn);
    }*/

    /*public void ClickTolWorldStatsOnOff() {
        tolWorldStatsOn = !tolWorldStatsOn;
        panelTolWorldStats.SetActive(tolWorldStatsOn && tolEventsTimelineOn);
        RecalculateTreeOfLifeGraphPanelSizes();
    }
    public void ClickTolSpeciesTreeOnOff() {
        //tolSpeciesTreeOn = !tolSpeciesTreeOn;
        //panelTolSpeciesTree.SetActive(tolSpeciesTreeOn);
        //RecalculateTreeOfLifeGraphPanelSizes();
    }
    public void ClickTolEventsTimelineOnOff() {
        tolEventsTimelineOn = !tolEventsTimelineOn;

        panelTolWorldStats.SetActive(tolWorldStatsOn && tolEventsTimelineOn);
        panelTolSpeciesTree.SetActive(tolEventsTimelineOn);
        panelTolEventsTimeline.SetActive(tolEventsTimelineOn);
        // backdrop on/off

        RecalculateTreeOfLifeGraphPanelSizes();
    }
    */
    /*
    private void RecalculateTreeOfLifeGraphPanelSizes() {
        
        
        if(tolSpeciesTreeOn) {
            

            if (tolEventsTimelineOn) {
                

                if (tolWorldStatsOn) {  // ALL ON
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 0.1f;

                    tolGraphCoordsStatsStart = 0.575f;
                    tolGraphCoordsStatsRange = 0.175f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.46f;
                }
                else {  // Species + Events:
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 0.1f;

                    tolGraphCoordsStatsStart = 0.75f;
                    tolGraphCoordsStatsRange = 0.025f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.675f;
                }
            }
            else {
                if (tolWorldStatsOn) {  // species + world Stats
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 0.1f;

                    tolGraphCoordsStatsStart = 0.5f;
                    tolGraphCoordsStatsRange = 0.25f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.75f;

                }
                else {  // species only
                    tolGraphCoordsEventsStart = 0.9f;
                    tolGraphCoordsEventsRange = 1f;

                    tolGraphCoordsStatsStart = 1f;
                    tolGraphCoordsStatsRange = 0.025f;

                    tolGraphCoordsSpeciesStart = 0.025f;
                    tolGraphCoordsSpeciesRange = 0.75f;
                }
            }
        }
        else {
            if (tolEventsTimelineOn) {
                

                if (tolWorldStatsOn) {  // World Stats + TEvents Timeline!
                    tolGraphCoordsEventsStart = 0.8f;
                    tolGraphCoordsEventsRange = 0.2f;

                    tolGraphCoordsStatsStart = 0.33f;
                    tolGraphCoordsStatsRange = 0.333f;

                    tolGraphCoordsSpeciesStart = 0f;
                    tolGraphCoordsSpeciesRange = 0.75f;

                }
                else {  // Events Timeline only:
                    tolGraphCoordsEventsStart = 0.8f;
                    tolGraphCoordsEventsRange = 0.2f;

                    tolGraphCoordsStatsStart = 0.5f;
                    tolGraphCoordsStatsRange = 0.025f;

                    tolGraphCoordsSpeciesStart = 0f;
                    tolGraphCoordsSpeciesRange = 0.75f;
                }
            }
            else {
                if (tolWorldStatsOn) {  // world Stats only
                    tolGraphCoordsEventsStart = 0.8f;
                    tolGraphCoordsEventsRange = 0.2f;

                    tolGraphCoordsStatsStart = 0.7f;
                    tolGraphCoordsStatsRange = 0.3f;

                    tolGraphCoordsSpeciesStart = 0f;
                    tolGraphCoordsSpeciesRange = 0.75f;

                }
            }
        }
    }*/

    /*public void ClickTolSpeciesTreeExtinctToggle() {
        tolSpeciesTreeExtinctOn = !tolSpeciesTreeExtinctOn;
    }
    public void ClickTolBrainDisplayToggle() {
        tolBrainDisplayOn = !tolBrainDisplayOn;
    }
    public void ClickTolInspectOnOff() {
        tolInspectOn = !tolInspectOn;

        if(tolInspectOn) {
            ClickToolButtonStir();
        }
        else {
            ClickToolButtonInspect();
        }
    }

    public void ClickTolWorldStatsCyclePrev() {
        tolSelectedWorldStatsIndex--;
        if(tolSelectedWorldStatsIndex < 0) {
            tolSelectedWorldStatsIndex = 10;
        }
        UpdateTolWorldStatsTextReadout();
        //textTolWorldStatsName.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString();
        //textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");
    }
    public void ClickTolWorldStatsCycleNext() {
        tolSelectedWorldStatsIndex++;
        if(tolSelectedWorldStatsIndex > 10) {
            tolSelectedWorldStatsIndex = 0;
        }
        UpdateTolWorldStatsTextReadout();
        //textTolWorldStatsName.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString();
        //textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");
    }
    public void ClickTolSpeciesTreeCyclePrev() {
        tolSelectedSpeciesStatsIndex--;
        if(tolSelectedSpeciesStatsIndex < 0) {
            tolSelectedSpeciesStatsIndex = 15;
        }
        //textTolSpeciesStatsName.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString();
        //UpdateTolSpeciesStatsTextReadout()
        UpdateTolSpeciesStatsTextReadout();
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");
    }
    public void ClickTolSpeciesTreeCycleNext() {
        tolSelectedSpeciesStatsIndex++;
        if(tolSelectedSpeciesStatsIndex > 15) {
            tolSelectedSpeciesStatsIndex = 0;
        }
        //textTolSpeciesStatsName.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString();
        UpdateTolSpeciesStatsTextReadout();
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");
    }
    public void ClickTolEventsTimelineMinor() {
        Debug.Log("ClickTolEventsTimelineMinor()");
    }
    public void ClickTolEventsTimelineMajor() {
        Debug.Log("ClickTolEventsTimelineMajor()");
    }
    public void ClickTolEventsTimelineExtreme() {
        Debug.Log("ClickTolEventsTimelineExtreme()");
    }

    public void ClickEventMinor() {
        if(isActiveEventsMinor) {
            isActiveEventsMinor = false;
            isActiveEventSelectionScreen = false;
        }
        else {
            isActiveEventsMinor = true;
            isActiveEventSelectionScreen = true;
            isActiveEventsMajor = false;
            isActiveEventsExtreme = false;
        }
        UpdateSimEventsUI();
    }
    public void ClickEventMajor() {
        if(isActiveEventsMajor) {
            isActiveEventsMajor = false;
            isActiveEventSelectionScreen = false;
        }
        else {
            isActiveEventsMajor = true;
            isActiveEventSelectionScreen = true;
            isActiveEventsMinor = false;
            isActiveEventsExtreme = false;
        }
        UpdateSimEventsUI();
    }
    public void ClickEventExtreme() {
        if(isActiveEventsExtreme) {
            isActiveEventsExtreme = false;
            isActiveEventSelectionScreen = false;
        }
        else {
            isActiveEventsExtreme = true;
            isActiveEventSelectionScreen = true;
            isActiveEventsMinor = false;
            isActiveEventsMajor = false;            
        }
        UpdateSimEventsUI();
    }
    public void ClickEventReroll() {
        if(isActiveEventsMinor) {
            gameManager.simulationManager.simEventsManager.RegenerateAvailableMinorEvents(gameManager.simulationManager);
        }
        if(isActiveEventsMajor) {
            gameManager.simulationManager.simEventsManager.RegenerateAvailableMajorEvents(gameManager.simulationManager);
        }
        if(isActiveEventsExtreme) {
            gameManager.simulationManager.simEventsManager.RegenerateAvailableExtremeEvents(gameManager.simulationManager);
        }
    }
    */
    /*public void ClickShowHideTolSpeciesDescription() {
        tolSpeciesDescriptionOn = !tolSpeciesDescriptionOn;
        textTolDescription.gameObject.SetActive(tolSpeciesDescriptionOn);
    }*/

    /*public void ClickTolPrevSpecies() {
        Debug.Log("ClickTolPrevSpecies()");
        int curSpeciesIndex = treeOfLifeManager.selectedID;

        int prevSpeciesIndex = curSpeciesIndex - 1;
        if(prevSpeciesIndex < 1) {
            prevSpeciesIndex = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1;
        }

        //treeOfLifeManager.selectedID = prevSpeciesIndex;
        ClickOnSpeciesNode(prevSpeciesIndex);
        //textTolSpeciesIndex.text = treeOfLifeManager.selectedID.ToString();
        
        // Update COLORS!!!
        //Vector3 speciesHuePrimary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[treeOfLifeManager.selectedID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        //imageTolPortraitBorder.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
        //imageTolSpeciesIndex.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
    }
    public void ClickTolNextSpecies() {
        Debug.Log("ClickTolNextSpecies()");
        int curSpeciesIndex = treeOfLifeManager.selectedID;
        
        int nextSpeciesIndex = curSpeciesIndex + 1;
        if(nextSpeciesIndex > (gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1)) {
            nextSpeciesIndex = 1;
        }

        //treeOfLifeManager.selectedID = nextSpeciesIndex;
        ClickOnSpeciesNode(nextSpeciesIndex);
        //textTolSpeciesIndex.text = treeOfLifeManager.selectedID.ToString();

        // Update COLORS!!!
        //Vector3 speciesHuePrimary = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[treeOfLifeManager.selectedID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        //imageTolPortraitBorder.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
        //imageTolSpeciesIndex.color = new Color(speciesHuePrimary.x, speciesHuePrimary.y, speciesHuePrimary.z);
    }
    */
    /*
    public void HoverOverTolGraphRenderPanel() {
        //Debug.Log("HoverOverTolGraphRenderPanel " + tolMouseCoords.ToString());

        Ray ray = new Ray(new Vector3(tolMouseCoords.x, tolMouseCoords.y, -1f), new Vector3(0f, 0f, 1f)); // cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //int layerMask = 0;
        Physics.Raycast(ray, out hit);
                
        
        if(hit.collider != null) {
            // How to handle multiple hits? UI Trumps environmental?
            TreeOfLifeNodeRaycastTarget speciesNodeRayTarget = hit.collider.gameObject.GetComponent<TreeOfLifeNodeRaycastTarget>();

            if(speciesNodeRayTarget != null) {
                selectedSpeciesID = speciesNodeRayTarget.speciesRef.speciesID;                
                treeOfLifeManager.HoverOverSpeciesNode(selectedSpeciesID);                
            }
            else {
                treeOfLifeManager.HoverAllOff();
            }            
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            treeOfLifeManager.HoverAllOff();
        }
    }
    public void ClickOnTolGraphRenderPanel(BaseEventData eventData) {
        PointerEventData pointerData = (PointerEventData)eventData;
        //Debug.Log("ClickOnTolGraphRenderPanel " + pointerData.pointerPressRaycast.ToString());
        Vector2 localPoint = Vector2.zero;
        RectTransform rectTransform = pointerData.pointerPressRaycast.gameObject.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);

        Vector2 uvCoord = new Vector2((localPoint.x + 230f) / 460f, (localPoint.y + 160f) / 320f);
        tolMouseCoords = uvCoord;
        tolMouseOver = 1f;
        Debug.Log("ClickOnTolGraphRenderPanel " + uvCoord.ToString());

        // Raycast Attempt:
        //Vector3 camPos = cameraManager.gameObject.transform.position;        
        //float mouseRatioX = Input.mousePosition.x / Screen.width;
        //float mouseRatioY = Input.mousePosition.y / Screen.height;
        //Vector3 mousePos = new Vector3(mouseRatioX, mouseRatioY, -1f);
        Ray ray = new Ray(new Vector3(uvCoord.x, uvCoord.y, -1f), new Vector3(0f, 0f, 1f)); // cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //int layerMask = 0;
        Physics.Raycast(ray, out hit);

        bool clicked = true;
        
        if(hit.collider != null) {
            // How to handle multiple hits? UI Trumps environmental?
            TreeOfLifeNodeRaycastTarget speciesNodeRayTarget = hit.collider.gameObject.GetComponent<TreeOfLifeNodeRaycastTarget>();

            if(speciesNodeRayTarget != null) {
                selectedSpeciesID = speciesNodeRayTarget.speciesRef.speciesID;
                if(clicked) {
                    ClickOnSpeciesNode(selectedSpeciesID);                    
                }
                else {
                    //treeOfLifeManager.HoverOverSpeciesNode(selectedSpeciesID);
                }
            }
            else {
                //treeOfLifeManager.HoverAllOff();
            }            
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            //treeOfLifeManager.HoverAllOff();
        }
    }
    */
    /*
    public void MouseOverTolGraphRenderPanel(BaseEventData eventData) {
        PointerEventData pointerData = (PointerEventData)eventData;
        //Debug.Log("ClickOnTolGraphRenderPanel " + pointerData.pointerPressRaycast.ToString());
        Vector2 localPoint = Vector2.zero;
        RectTransform rectTransform = pointerData.pointerPressRaycast.gameObject.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);

        Vector2 uvCoord = new Vector2((localPoint.x + 230f) / 460f, (localPoint.y + 160f) / 320f);
        tolMouseCoords = uvCoord;
        tolMouseOver = 1f;
        Debug.Log("ClickOnTolGraphRenderPanel " + uvCoord.ToString());
    }*/
    /*
    public void MouseEnterTolGraphRenderPanel(BaseEventData eventData) {        
        Debug.Log("MouseEnterTolGraphRenderPanel");
        tolMouseOver = 1f;
    }
    public void MouseExitTolGraphRenderPanel(BaseEventData eventData) {        
        Debug.Log("MouseExitTolGraphRenderPanel");
        tolMouseOver = 0f;
    }
    */
    
    /*public void UpdateTolGraphCursorTimeSelectUI(Vector2 coords) {
        // find closest event:
        if(gameManager.simulationManager.simEventsManager.completeEventHistoryList.Count > 0) {
            float closestEventDistance = 1f;
            int closestEventIndex = 0;

            for(int i = 0; i < gameManager.simulationManager.simEventsManager.completeEventHistoryList.Count; i++) {
                float eventCoord = (float)gameManager.simulationManager.simEventsManager.completeEventHistoryList[i].timeStepActivated / (float)gameManager.simulationManager.simAgeTimeSteps;

                float eventDistance = Mathf.Abs(coords.x - eventCoord);

                if(eventDistance < closestEventDistance) {
                    closestEventIndex = i;
                    closestEventDistance = eventDistance;
                }
            }
            //Debug.Log("Closest Event: " + closestEventIndex.ToString() + ",  " + gameManager.simulationManager.simEventsManager.completeEventHistoryList[closestEventIndex].name);

            curClosestEventToCursor = closestEventIndex;
        }


        int selectedSpeciesID = treeOfLifeManager.selectedID;
        SpeciesGenomePool selectedPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
        int sampleYear = Mathf.FloorToInt(coords.x * (float)gameManager.simulationManager.curSimYear);
        sampleYear = Mathf.Clamp(sampleYear, 0, selectedPool.avgLifespanPerYearList.Count - 1);
        
        //int index = t - speciesPool.yearCreated;
        //int index = sampleYear;
        float valStat = 0f;
        float a = tolSelectedSpeciesStatsIndex;
        // I know there's a better way to do this:
        if(a == 0) {
            valStat = selectedPool.avgLifespanPerYearList[sampleYear];
        }
        else if(a == 1) {
            valStat = selectedPool.avgConsumptionDecayPerYearList[sampleYear];
        }
        else if(a == 2) {
            valStat = selectedPool.avgConsumptionPlantPerYearList[sampleYear];
        }
        else if(a == 3) {
            valStat = selectedPool.avgConsumptionMeatPerYearList[sampleYear];
        }
        else if(a == 4) {
            valStat = selectedPool.avgBodySizePerYearList[sampleYear];
        }
        else if(a == 5) {
            valStat = selectedPool.avgSpecAttackPerYearList[sampleYear];
        }
        else if(a == 6) {
            valStat = selectedPool.avgSpecDefendPerYearList[sampleYear];
        }
        else if(a == 7) {
            valStat = selectedPool.avgSpecSpeedPerYearList[sampleYear];
        }
        else if(a == 8) {
            valStat = selectedPool.avgSpecUtilityPerYearList[sampleYear];
        }
        else if(a == 9) {
            valStat = selectedPool.avgFoodSpecDecayPerYearList[sampleYear];
        }
        else if(a == 10) {
            valStat = selectedPool.avgFoodSpecPlantPerYearList[sampleYear];
        }
        else if(a == 11) {
            valStat = selectedPool.avgFoodSpecMeatPerYearList[sampleYear];
        }
        else if(a == 12) {
            valStat = selectedPool.avgNumNeuronsPerYearList[sampleYear];
        }
        else if(a == 13) {
            valStat = selectedPool.avgNumAxonsPerYearList[sampleYear];
        }
        else if(a == 14) {
            valStat = selectedPool.avgExperiencePerYearList[sampleYear];
        }
        else if(a == 15) {
            valStat = selectedPool.avgFitnessScorePerYearList[sampleYear];
        }
        else if(a == 16) {
            valStat = selectedPool.avgDamageDealtPerYearList[sampleYear];
        }
        else if(a == 17) {
            valStat = selectedPool.avgDamageTakenPerYearList[sampleYear];
        }
        else if(a == 18) {
            valStat = selectedPool.avgLifespanPerYearList[sampleYear];
        }

        curSpeciesStatValue = valStat;

        //maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);
                        
        //statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                    //}                    
                //}
            //}            
        //}
        
        
        
        
        // Move Text-boxes to position of vertical mouse line
        // update text values
        // Find graph values at uv coord???  // query GPU or save info on CPU??
        //turn on/off panel in enter/exit functions
        
        //RectTransform rectSpeciesStats = textTolSpeciesStatsValue.GetComponent<RectTransform>();
        //rectSpeciesStats.localPosition = new Vector3(coords.x + 220f, coords.y + 10f, 0f);
        //textTolSpeciesStatsValue.text = ((SpeciesStatsMode)tolSelectedSpeciesStatsIndex).ToString() + ": " + curSpeciesStatValue.ToString("F3");

        RectTransform rectWorldStats = textTolWorldStatsValue.GetComponent<RectTransform>();
        rectWorldStats.localPosition = new Vector3(coords.x + 220f, coords.y + 20f, 0f);
        textTolWorldStatsValue.text = ((WorldStatsMode)tolSelectedWorldStatsIndex).ToString() + ": " + curWorldStatValue.ToString("F3");

        RectTransform rectEventStats = textTolEventsTimelineName.GetComponent<RectTransform>();
        rectEventStats.localPosition = new Vector3(coords.x + 220f, coords.y + 30f, 0f);
        textTolEventsTimelineName.text = "EVENT";
        
    }*/


/*public void ClickInfoTabResources() {
        isActiveInfoResourcesTab = true;        
    }
    public void ClickInfoTabSpecies() {
        isActiveInfoResourcesTab = false;     
    }

    public void ClickInfoSpeciesDecomposers() {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 0;
        infoSpeciesSelectedTier = 0;
        infoSpeciesSelectedSlot = 0;

        textInfoSpeciesName.text = "Decomposers";
    }
    public void ClickInfoSpeciesAlgae() {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 1;
        infoSpeciesSelectedTier = 0;
        infoSpeciesSelectedSlot = 0;

        textInfoSpeciesName.text = "Algae";
    }
    public void ClickInfoSpeciesZooplankton() {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 2;
        infoSpeciesSelectedTier = 0;
        infoSpeciesSelectedSlot = 0;

        textInfoSpeciesName.text = "Zooplankton";
    }
    public void ClickInfoSpeciesAgent(int slotID) {
        infoSpeciesSelected = true;
        infoSpeciesSelectedKingdom = 2;
        infoSpeciesSelectedTier = 1;
        infoSpeciesSelectedSlot = slotID;

        textInfoSpeciesName.text = "Animal " + slotID.ToString();

        treeOfLifeManager.ClickedOnSpeciesNode(trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[slotID].linkedSpeciesID); //slotID); // find proper ID
        UpdateTolSpeciesColorUI();
    }

    */

// OLD:

/*public void UpdateSimEventsUI() {
    SimEventsManager eventsManager = gameManager.simulationManager.simEventsManager;

    panelEventSelectionScreen.SetActive(isActiveEventSelectionScreen);

    textEventPointsWallet.text = "$" + gameManager.simulationManager.simEventsManager.curEventBucks.ToString();

    // Check for cooldown and currency to determine whether ot unlock menus:
    buttonEventsMinor.interactable = false;
    imageButtonEventsMinor.color = buttonEventMinorColor;
    buttonEventsMajor.interactable = false;
    imageButtonEventsMajor.color = buttonEventMajorColor;
    buttonEventsExtreme.interactable = false;
    imageButtonEventsExtreme.color = buttonEventExtremeColor;
    if(eventsManager.isCooldown) {
        //buttonEventsMinor.interactable = false;
        //buttonEventsMajor.interactable = false;
        //buttonEventsExtreme.interactable = false;
        imageButtonEventsMinor.color = buttonEventMinorColor * 0.25f;
        imageButtonEventsMajor.color = buttonEventMajorColor * 0.25f;
        imageButtonEventsExtreme.color = buttonEventExtremeColor * 0.25f;
        textRecentEvent.gameObject.SetActive(true);
        textRecentEvent.text = eventsManager.mostRecentEventString;
    }
    else {
        textRecentEvent.gameObject.SetActive(false);
        //textRecentEvent.text = eventsManager.mostRecentEventString;

        if(eventsManager.curEventBucks >= 3) {                
            buttonEventsMinor.interactable = true;
        }
        else {
            imageButtonEventsMinor.color = buttonEventMinorColor * 0.25f;    // also multiplying ALPHA!!! ... annoying              
        }
        if(eventsManager.curEventBucks >= 15) {
            buttonEventsMajor.interactable = true;
        }
        else {
            imageButtonEventsMajor.color = buttonEventMajorColor * 0.25f;
        }
        if(eventsManager.curEventBucks >= 75) {
            buttonEventsExtreme.interactable = true;
        }
        else {
            imageButtonEventsExtreme.color = buttonEventExtremeColor * 0.25f;
        }
    }
    //
    //

    if(isActiveEventSelectionScreen) {
        string titleText = "";
        if(isActiveEventsMinor) {
            titleText = "MINOR EVENTS";

            for(int i = 0; i < simEventComponentsArray.Length; i++) {                    
                if(selectedMinorEventIndex == i) {
                    textEventSelectedDescription.text = gameManager.simulationManager.simEventsManager.availableMinorEventsList[i].description;
                    simEventComponentsArray[i].isSelected = true;
                   // simEventComponentsArray[i]  // if selected -- make bigger or glowy or whatever
                }
                else {
                    simEventComponentsArray[i].isSelected = false;
                }
                simEventComponentsArray[i].UpdateSimEventPanel(this, gameManager.simulationManager.simEventsManager.availableMinorEventsList[i], i);
            }

            imageButtonEventsMinor.color = buttonEventMinorColor * 2f;
            imageEventSelectionTitle.color = buttonEventMinorColor;
        }
        if(isActiveEventsMajor) {
            titleText = "MAJOR EVENTS";

            for(int i = 0; i < simEventComponentsArray.Length; i++) {                    
                if(selectedMajorEventIndex == i) {
                    textEventSelectedDescription.text = gameManager.simulationManager.simEventsManager.availableMajorEventsList[i].description;
                    simEventComponentsArray[i].isSelected = true;
                }
                else {
                    simEventComponentsArray[i].isSelected = false;
                }
                simEventComponentsArray[i].UpdateSimEventPanel(this, gameManager.simulationManager.simEventsManager.availableMajorEventsList[i], i);
            }

            imageButtonEventsMajor.color = buttonEventMajorColor * 2f;
            imageEventSelectionTitle.color = buttonEventMajorColor;
        }
            if(isActiveEventsExtreme) {
            titleText = "EXTREME EVENTS";

            for(int i = 0; i < simEventComponentsArray.Length; i++) {                    
                if(selectedExtremeEventIndex == i) {
                    textEventSelectedDescription.text = gameManager.simulationManager.simEventsManager.availableExtremeEventsList[i].description;  // if selected -- make bigger or glowy or whatever
                    simEventComponentsArray[i].isSelected = true;
                }
                else {
                    simEventComponentsArray[i].isSelected = false;
                }
                simEventComponentsArray[i].UpdateSimEventPanel(this, gameManager.simulationManager.simEventsManager.availableExtremeEventsList[i], i);
            }

            imageButtonEventsExtreme.color = buttonEventExtremeColor * 2f;
            imageEventSelectionTitle.color = buttonEventExtremeColor;
        }
        textEventSelectionTitle.text = titleText;
    }
    else {

    }
}
public void ClickedOnEvent(SimEventComponent eventComponent) {
    if(isActiveEventsMinor) {
        selectedMinorEventIndex = eventComponent.index;
    }
    if(isActiveEventsMajor) {
        selectedMajorEventIndex = eventComponent.index;
    }
    if(isActiveEventsExtreme) {
        selectedExtremeEventIndex = eventComponent.index;
    }
}
public void ClickConfirmEvent() {

    if(isActiveEventsMinor) {
        SimEventData eventData = gameManager.simulationManager.simEventsManager.availableMinorEventsList[selectedMinorEventIndex];
        gameManager.simulationManager.ExecuteSimEvent(eventData);            
        Debug.Log("ClickConfirmEvent(" + selectedMinorEventIndex.ToString() + ") minor");
       // selectedMinorEventIndex = eventComponent.index;
    }
    if(isActiveEventsMajor) {
        SimEventData eventData = gameManager.simulationManager.simEventsManager.availableMajorEventsList[selectedMajorEventIndex];
        gameManager.simulationManager.ExecuteSimEvent(eventData);
        Debug.Log("ClickConfirmEvent(" + selectedMajorEventIndex.ToString() + ") major");

    }
    if(isActiveEventsExtreme) {
        SimEventData eventData = gameManager.simulationManager.simEventsManager.availableExtremeEventsList[selectedExtremeEventIndex];
        gameManager.simulationManager.ExecuteSimEvent(eventData);
        Debug.Log("ClickConfirmEvent(" + selectedExtremeEventIndex.ToString() + ") extreme");
    }

    isActiveEventSelectionScreen = false;
    isActiveEventsMinor = false;
    isActiveEventsMajor = false;
    isActiveEventsExtreme = false;
}
*/

#endregion

