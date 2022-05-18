using UnityEngine;
using UnityEngine.UI;

public class WatcherUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManagerRef => UIManager.instance;
    TrophicLayersManager trophicLayersManager => simulationManager.trophicLayersManager;

    public bool isUnlocked;
    public bool isOpen;
    public bool isDim;

    public AudioSource audioSource00;
    public AudioSource audioSource01;
    public AudioSource audioSource02;
    public AudioSource audioSource03;
    
    //private int callTickCooldownCounter = 0;

    //public Image imageWatcherButtonMIP;  // superscript button over watcher toolbar Button
    //public Image imageWatcherCurTargetLayer; // in watcher panel
    public Text textTargetLayer;
    //public Button buttonWatcherLock;
    public Button buttonHighlightingToggle;
    //public Button buttonFollowingToggle;
    
    //public bool isFollow;
    //public TrophicSlot targetSlotRef;
    //public bool isSelected;

    //public Animator animatorWatcherUI;

    public Image imageDimmingSheet;
    //public Image imageIsSnooping;
    //public Image imageWatcherInactiveOverlay;
    //public Text textIsSnooping;

    public GameObject followCreaturePanel;
    public GameObject panelFollowStatus;
    private bool isFollowStatusPanelOn = true;
    public GameObject panelFollowBehavior;
    private bool isFollowBehaviorPanelOn = true;
    public GameObject panelFollowGenome;
    public Text textFollowGenome;
    //private bool isFollowGenomePanelOn = false;
    public GameObject panelFollowHistory;
    public Text textFollowHistory;
    private bool isFollowHistoryPanelOn = false;


    public TrophicSlot watcherSelectedTrophicSlotRef; // plant, zooplankton, or vertebrate
    //public bool isWatcherTargetLayerLocked;
    //public Text textWatcherPanelTargetLayer;
    public GameObject panelWatcherSpiritMain;
    public GameObject panelWatcherExpand;
    
    public Text TextCommonStatsA;
    //private int curWatcherPanelPlantsPageNum;
    //private int curWatcherPanelZooplanktonPageNum;
    private int curWatcherPanelVertebratePageNum;
    public Button buttonWatcherVertebrateCyclePage; // maybe not needed?
    //public GameObject panelWatcherSpiritVertebratesHUD; // 0
    public Text textWatcherVertebrateHUD;
    public Text textVertebrateGen;
    
    //public GameObject panelWatcherVertebrateWidgets01;
    public GameObject panelWatcherSpiritVertebratesText;  // 1
    public Text textWatcherVertebrateText;

    public Text textWatcherTargetIndex;
    
    

	// Use this for initialization
	void Start () {
        //isSnoopingModeON = false;
	}

    public void ClickButtonFollowStatus() {
        isFollowStatusPanelOn = !isFollowStatusPanelOn;
    }
    public void ClickButtonFollowBehavior() {
        isFollowBehaviorPanelOn = !isFollowBehaviorPanelOn;
    }
    //public void ClickButtonFollowGenome() {
    //    isFollowGenomePanelOn = !isFollowGenomePanelOn;
    //}
    public void ClickButtonFollowHistory() {
        isFollowHistoryPanelOn = !isFollowHistoryPanelOn;
    }
    	
    private void UpdateUI() {
        panelWatcherExpand.SetActive(false);

        if (cameraManager.isFollowingPlantParticle) {
            cameraManager.targetPlantWorldPos = simulationManager.vegetationManager.selectedPlantParticleData.worldPos;
        }
        if (cameraManager.isFollowingAnimalParticle) {
            cameraManager.targetZooplanktonWorldPos = simulationManager.zooplanktonManager.selectedAnimalParticleData.worldPos;
        }

        //panelWatcherSpiritVertebratesText.SetActive(false);
      
        TextCommonStatsA.gameObject.SetActive(false);
        if (watcherSelectedTrophicSlotRef != null) 
        {
            TextCommonStatsA.gameObject.SetActive(true);

            //int critterIndex = cameraManager.targetAgentIndex;
            Agent agent = SelectionManager.instance.currentSelection.agent;

            imageDimmingSheet.gameObject.SetActive(agent.isDecaying);  // ** dimming sheet! to show not following???? just an experiment
            //button

            Vector3 textHuePrimary = agent.candidateRef.primaryHue;
            Color textColorPrimary = new Color(textHuePrimary.x, textHuePrimary.y, textHuePrimary.z);
            Vector3 textHueSecondary = agent.candidateRef.secondaryHue;
            Color textColorSecondary = new Color(textHueSecondary.x, textHueSecondary.y, textHueSecondary.z);
            textTargetLayer.color = textColorPrimary;
            
            textTargetLayer.text = "Species " + agent.speciesIndex + ":  " + agent.candidateRef.candidateGenome.name;

            //textNewInspectAgentName.text = "Critter #" + agent.candidateRef.candidateID.ToString(); //.candidateGenome.bodyGenome.coreGenome.name;
            //textNewInspectAgentName.color = textColorSecondary; // uiManagerRef.colorVertebratesLayer;

            if (agent.coreModule == null) 
            {
                followCreaturePanel.SetActive(false);
                return;
            }
                
            followCreaturePanel.SetActive(true);
            panelFollowStatus.SetActive(isFollowStatusPanelOn);
            panelFollowBehavior.SetActive(isFollowBehaviorPanelOn);
            //panelFollowGenome.SetActive(isFollowGenomePanelOn);
            panelFollowHistory.SetActive(isFollowHistoryPanelOn);
            
            TextCommonStatsA.gameObject.SetActive(false);  // non-vertebrates just share one textbox for now

            //textWatcherVertebratePageNum.text = "PAGE " + (curWatcherPanelVertebratePageNum + 1).ToString() + " of 4";
            textWatcherTargetIndex.text = "#" + agent.index;
                            
            //textNewInspectLog.text = "";
            textWatcherVertebrateHUD.text = "";
                               
            int maxEventsToDisplay = 8;
            //int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
            int startIndex = Mathf.Max(0, agent.candidateRef.candidateEventDataList.Count - maxEventsToDisplay);                   
            string eventString = "";
            for(int q = agent.candidateRef.candidateEventDataList.Count - 1; q >= startIndex; q--) {
                eventString += "\n[" + agent.candidateRef.candidateEventDataList[q].eventFrame + "] " + agent.candidateRef.candidateEventDataList[q].eventText;
            }                

            string textStringLog = "Event Log! Agent[" + agent.index + "]";                    
            // Agent Event Log:
            int maxEventsToDisplayLog = 12;
            //int numEventsLog = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplayLog);
            int startIndexLog = Mathf.Max(0, agent.candidateRef.candidateEventDataList.Count - maxEventsToDisplayLog);                   
            string eventLogString = "";
            
            for (int q = agent.candidateRef.candidateEventDataList.Count - 1; q >= startIndexLog; q--) 
            {
                float dimAmount = Mathf.Clamp01((float)(agent.candidateRef.candidateEventDataList.Count - q - 1) * 0.55f);
                //Color displayColor = Color.Lerp(Color.red, Color.green, agent.agentEventDataList[q].goodness);
                string goodColorStr = dimAmount > 0.5f ? "#007700FF" : "#00FF00FF";
                string badColorStr = dimAmount > 0.5f ? "#770000FF" : "#FF0000FF";
                string colorStr = agent.candidateRef.candidateEventDataList[q].goodness > 0.5f ? goodColorStr : badColorStr;
                eventLogString += "<color=" + colorStr + ">";
                eventLogString += "\n[" + agent.candidateRef.candidateEventDataList[q].eventFrame + "] " + agent.candidateRef.candidateEventDataList[q].eventText;
                eventLogString += "</color>";
            }
            textStringLog += eventLogString;
            textWatcherVertebrateText.text = textStringLog;
                
            // WPP: delegated to function (commented out because results not used for anything)
            //GetLifeStageProgressText(agent);
            
            TextCommonStatsA.text = "";
        }
    }
    
    string GetLifeStageProgressText(Agent agent)
    {
        int curCount = 0;
        int maxCount = 1;
        if (agent.isEgg) {
            curCount = agent.lifeStageTransitionTimeStepCounter;
            maxCount = agent.gestationDurationTimeSteps;
        }            
        if (agent.isMature) {
            curCount = agent.ageCounter;
            maxCount = agent.maxAgeTimeSteps;
        }
        if (agent.isDead) {
            curCount = agent.lifeStageTransitionTimeStepCounter;
            maxCount = curCount; // agentRef._DecayDurationTimeSteps;
        }
        int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
        return " " + agent.curLifeStage + " " + curCount + "/" + maxCount + "  " + progressPercent + "% ";
    }

	// * WPP: clean out dead code.  If it is needed again, write it again, it is not a big loss.
	public void UpdateWatcherPanelUI() {
        //animatorWatcherUI.SetBool("_IsOpen", isOpen);
        UpdateUI();

        if(cameraManager.isFollowingAgent) {  // only active when following a selected critter -- make sure it only contains:
            // FollowTitle, FollowStatus, FollowBrain
            // NOT HISTORY (shared with world event timeline?)

            //animatorWatcherUI.SetBool("_IsOpen", true);
            //animatorWatcherUI.SetBool("_IsDim", false);
        }
        else {
            //animatorWatcherUI.SetBool("_IsDim", true);

            // WPP: condensed
            //StopFollowingAgent();
            //StopFollowingPlantParticle();
            //StopFollowingAnimalParticle();
            cameraManager.SetFollowing(KnowledgeMapId.Undefined);

            followCreaturePanel.SetActive(false);
        }

        panelWatcherSpiritMain.SetActive(true); // isOpen);
    }
    
    /*
    TrophicSlot slotRefWatcher = trophicLayersManager.selectedTrophicSlotRef;
        if(slotRefWatcher != null) {
            if(isWatcherTargetLayerLocked) {
                slotRefWatcher = watcherLockedTrophicSlotRef;
            }
            else {
                
                //textWatcherPanelTargetLayer.text = isWatcherTargetLayerLocked.ToString() + ", " + watcherLockedTrophicSlotRef.kingdomID.ToString();
            }
            
        }
        if(slotRefWatcher != null) {
            imageWatcherButtonMIP.sprite = slotRefWatcher.icon; // isWatcherTargetLayerLocked;
            imageWatcherButtonMIP.color = slotRefWatcher.color;
            imageWatcherCurTarget.sprite = slotRefWatcher.icon;
            imageWatcherCurTarget.color = slotRefWatcher.color;
            textTargetLayer.text = slotRefWatcher.speciesName;
            textTargetLayer.color = slotRefWatcher.color;

        }
        else {
            
        }
*/
    /*public void ClickButtonHighlightingToggle() {
        isHighlight = !isHighlight;
        if(isHighlight) {
            uiManagerRef.curActiveTool = UIManager.ToolType.None;
            //Set ToolType to .None
        }
    }*/
    /*
    public void ClickButtonFollowingToggle() {
        isFollow = !isFollow;

        if(isFollow) {
            uiManagerRef.curActiveTool = UIManager.ToolType.None;
            //Set ToolType to .None
            //uiManagerRef.StartFollowingAgent();
            //uiManagerRef.StartFollowingPlantParticle();
            //uiManagerRef.StartFollowingAnimalParticle();
        }
        else {
            StopFollowingAgent();
            StopFollowingPlantParticle();
            StopFollowingAnimalParticle();
        }
    }
    */
    
    public void ClickToolButton() {
        isOpen = !isOpen;
        //isHighlight = true;
        if(isOpen) {  // if opening the panel automatically engage snooping mode
            //isSnoopingModeON = true;
            //uiManagerRef.isBrushModeON_snoopingOFF = false;
            //uiManagerRef.panelFocus = PanelFocus.Watcher;
            //uiManagerRef.curActiveTool = UIManager.ToolType.None;

            //animatorWatcherUI.SetBool("_IsOpen", true);
        }
        else {
            //uiManagerRef.panelFocus = PanelFocus.WorldHub;
            //animatorWatcherUI.SetBool("_IsOpen", false);
        }
        //watcherLockedTrophicSlotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;//
    }
    
    public void ClickSnoopModeButton() {        
        //uiManagerRef.isBrushModeON_snoopingOFF = false;
        //uiManagerRef.panelFocus = PanelFocus.Watcher;
        //uiManagerRef.curActiveTool = UIManager.ToolType.None;
    }
    
    public void ActivateWatcherPanel() {
        isOpen = true;
        //uiManagerRef.panelFocus = PanelFocus.Watcher;
        //animatorWatcherUI.SetBool("_IsOpen", true);
    }

    public void ClickWatcherVertebratePageCyclePrev() {
        curWatcherPanelVertebratePageNum--;

        if(curWatcherPanelVertebratePageNum < 0) {
            curWatcherPanelVertebratePageNum = 3;
        }
    }
    
    public void ClickWatcherVertebratePageCycleNext() {
        curWatcherPanelVertebratePageNum++;

        if(curWatcherPanelVertebratePageNum > 3) {
            curWatcherPanelVertebratePageNum = 0;
        }
    }

    public void ClickWatcherCycleTargetPrev() {
        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot; // ************* TEMP!!!!!!!!!

        if(slotRef.id == KnowledgeMapId.Plants) {
            VegetationManager veggieRef = simulationManager.vegetationManager;
            veggieRef.selectedPlantParticleIndex--;
            
            if(veggieRef.selectedPlantParticleIndex < 0) {
                veggieRef.selectedPlantParticleIndex = veggieRef.plantParticlesCBuffer.count - 1;
                veggieRef.isPlantParticleSelected = true; // ???
            }
        }
        else if (slotRef.id == KnowledgeMapId.Microbes) {
            ZooplanktonManager zoopRef = simulationManager.zooplanktonManager;
            zoopRef.selectedAnimalParticleIndex--;
            
            if (zoopRef.selectedAnimalParticleIndex < 0) {
                zoopRef.selectedAnimalParticleIndex = zoopRef.animalParticlesCBuffer.count - 1;
                zoopRef.isAnimalParticleSelected = true; // ???
            }
        }
        else if (slotRef.id == KnowledgeMapId.Animals) {
            //ClickPrevAgent();
        }
    }
    
    public void ClickWatcherCycleTargetNext() {
        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;  // ************* TEMP!!!!!!!!!

        if(slotRef.id == KnowledgeMapId.Plants) {
            VegetationManager veggieRef = simulationManager.vegetationManager;
            veggieRef.selectedPlantParticleIndex++;
            
            if(veggieRef.selectedPlantParticleIndex > veggieRef.plantParticlesCBuffer.count - 1) {
                veggieRef.selectedPlantParticleIndex = 0;
                veggieRef.isPlantParticleSelected = true; // ???
            }
        }
        else if (slotRef.id == KnowledgeMapId.Microbes) {
            ZooplanktonManager zoopRef = simulationManager.zooplanktonManager;
            zoopRef.selectedAnimalParticleIndex++;
            if (zoopRef.selectedAnimalParticleIndex > zoopRef.animalParticlesCBuffer.count - 1) {
                zoopRef.selectedAnimalParticleIndex = 0;
                zoopRef.isAnimalParticleSelected = true; // ???
            }
        }
        else if (slotRef.id == KnowledgeMapId.Animals) {
            //ClickNextAgent();
        }
    }
    /*
    public void ClickPrevAgent() {
        //Debug.Log("ClickPrevAgent");
        int newIndex = (simulationManager.numAgents + cameraManager.targetAgentIndex - 1) % simulationManager.numAgents;
        cameraManager.SetTargetAgent(simulationManager.agents[newIndex], newIndex);                              
    }
    
    public void ClickNextAgent() {
        //Debug.Log("ClickNextAgent");
        int newIndex = (cameraManager.targetAgentIndex + 1) % simulationManager.numAgents;
        cameraManager.SetTargetAgent(simulationManager.agents[newIndex], newIndex);                
    }
    */
    public void StartFollowingAgent() {
        cameraManager.isFollowingAgent = true;
        //uiManagerRef.globalResourcesUI.CreateBrainGenomeTexture(uiManagerRef.cameraManager.targetAgent.candidateRef.candidateGenome);
    }
    
    public void StartFollowingPlantParticle() {
        cameraManager.SetFollowing(KnowledgeMapId.Plants);
        watcherSelectedTrophicSlotRef = trophicLayersManager.GetSlot(KnowledgeMapId.Plants); 
    }
    
    public void StartFollowingAnimalParticle() {
        cameraManager.SetFollowing(KnowledgeMapId.Microbes);
        watcherSelectedTrophicSlotRef = trophicLayersManager.GetSlot(KnowledgeMapId.Microbes);
    }
}