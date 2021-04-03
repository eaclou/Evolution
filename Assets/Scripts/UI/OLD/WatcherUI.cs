﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatcherUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public UIManager uiManagerRef;
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
    	
    private void UpdateUI(TrophicLayersManager layerManager) {

        
        panelWatcherExpand.SetActive(false);            
        
        
        if(cameraManager.isFollowingPlantParticle) {
            cameraManager.targetPlantWorldPos = simulationManager.vegetationManager.selectedPlantParticleData.worldPos;
        }
        if(cameraManager.isFollowingAnimalParticle) {
            cameraManager.targetZooplanktonWorldPos = simulationManager.zooplanktonManager.selectedAnimalParticleData.worldPos;
        }

        //panelWatcherSpiritVertebratesText.SetActive(false);
      
        TextCommonStatsA.gameObject.SetActive(false);
        if(watcherSelectedTrophicSlotRef != null) {
            TextCommonStatsA.gameObject.SetActive(true);

            int critterIndex = cameraManager.targetAgentIndex;
            Agent agent = simulationManager.agentsArray[critterIndex];

            imageDimmingSheet.gameObject.SetActive(agent.isDecaying);  // ** dimming sheet! to show not following???? just an experiment
            //button

            Vector3 textHuePrimary = agent.candidateRef.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            Color textColorPrimary = new Color(textHuePrimary.x, textHuePrimary.y, textHuePrimary.z);
            Vector3 textHueSecondary = agent.candidateRef.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            Color textColorSecondary = new Color(textHueSecondary.x, textHueSecondary.y, textHueSecondary.z);
            textTargetLayer.color = textColorPrimary;
            
            string str = "";
            
            textTargetLayer.text = "Species " + agent.speciesIndex.ToString() + ":  " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.name;

            //textNewInspectAgentName.text = "Critter #" + agent.candidateRef.candidateID.ToString(); //.candidateGenome.bodyGenome.coreGenome.name;
            //textNewInspectAgentName.color = textColorSecondary; // uiManagerRef.colorVertebratesLayer;

            if(agent.coreModule != null) {
                
                followCreaturePanel.SetActive(true);

                panelFollowStatus.SetActive(isFollowStatusPanelOn);
                panelFollowBehavior.SetActive(isFollowBehaviorPanelOn);
                //panelFollowGenome.SetActive(isFollowGenomePanelOn);
                panelFollowHistory.SetActive(isFollowHistoryPanelOn);
                
                TextCommonStatsA.gameObject.SetActive(false);  // non-vertebrates just share one textbox for now

                //textWatcherVertebratePageNum.text = "PAGE " + (curWatcherPanelVertebratePageNum + 1).ToString() + " of 4";
                textWatcherTargetIndex.text = "#" + agent.index.ToString();
                                
                //textNewInspectLog.text = "";
                textWatcherVertebrateHUD.text = "";
                                   
                int maxEventsToDisplay = 8;
                //int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
                int startIndex = Mathf.Max(0, agent.agentEventDataList.Count - maxEventsToDisplay);                   
                string eventString = "";
                for(int q = agent.agentEventDataList.Count - 1; q >= startIndex; q--) {
                    eventString += "\n[" + agent.agentEventDataList[q].eventFrame.ToString() + "] " + agent.agentEventDataList[q].eventText;
                }                

                string textStringLog = "Event Log! Agent[" + agent.index.ToString() + "]";                    
                // Agent Event Log:
                int maxEventsToDisplayLog = 12;
                //int numEventsLog = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplayLog);
                int startIndexLog = Mathf.Max(0, agent.agentEventDataList.Count - maxEventsToDisplayLog);                   
                string eventLogString = "";
                for(int q = agent.agentEventDataList.Count - 1; q >= startIndexLog; q--) {
                    float dimAmount = Mathf.Clamp01((float)(agent.agentEventDataList.Count - q - 1) * 0.55f);
                    //Color displayColor = Color.Lerp(Color.red, Color.green, agent.agentEventDataList[q].goodness);
                    string goodColorStr = "#00FF00FF";
                    if(dimAmount > 0.5f) {
                        goodColorStr = "#007700FF";
                    }
                    string badColorStr = "#FF0000FF";
                    if(dimAmount > 0.5f) {
                        badColorStr = "#770000FF";
                    }
                    if(agent.agentEventDataList[q].goodness > 0.5f) {
                        eventLogString += "<color=" + goodColorStr + ">";
                    }
                    else {
                        eventLogString += "<color=" + badColorStr + ">";
                    }
                            
                    eventLogString += "\n[" + agent.agentEventDataList[q].eventFrame.ToString() + "] " + agent.agentEventDataList[q].eventText;
                    eventLogString += "</color>";
                }
                textStringLog += eventLogString;
                textWatcherVertebrateText.text = textStringLog;
                    
                    
                int curCount = 0;
                int maxCount = 1;
                if (agent.curLifeStage == Agent.AgentLifeStage.Egg) {
                    curCount = agent.lifeStageTransitionTimeStepCounter;
                    maxCount = agent._GestationDurationTimeSteps;
                }            
                if (agent.curLifeStage == Agent.AgentLifeStage.Mature) {
                    curCount = agent.ageCounter;
                    maxCount = agent.maxAgeTimeSteps;
                }
                if (agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                    curCount = agent.lifeStageTransitionTimeStepCounter;
                    maxCount = curCount; // agentRef._DecayDurationTimeSteps;
                }
                //int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
                //string lifeStageProgressTxt = " " + agent.curLifeStage.ToString() + " " + curCount.ToString() + "/" + maxCount.ToString() + "  " + progressPercent.ToString() + "% ";
                
                
            }
            else {
                followCreaturePanel.SetActive(false);
            }
            TextCommonStatsA.text = str;
            
            // string?
        }
        else {
            // No target
        }  
        
    }

	public void UpdateWatcherPanelUI(TrophicLayersManager layerManager) {
        //animatorWatcherUI.SetBool("_IsOpen", isOpen);

        UpdateUI(layerManager);

        if(cameraManager.isFollowingAgent) {  // only active when following a selected critter -- make sure it only contains:
            // FollowTitle, FollowStatus, FollowBrain
            // NOT HISTORY (shared with world event timeline?)

            //animatorWatcherUI.SetBool("_IsOpen", true);
            //animatorWatcherUI.SetBool("_IsDim", false);
        }
        else {
            //animatorWatcherUI.SetBool("_IsDim", true);

            StopFollowingAgent();
            StopFollowingPlantParticle();
            StopFollowingAnimalParticle();

            followCreaturePanel.SetActive(false);
        }

        

        panelWatcherSpiritMain.SetActive(true); // isOpen);
        
    }
    /*
    TrophicSlot slotRefWatcher = uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
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

        if(slotRef.kingdomID == 1) {
            if(slotRef.tierID == 0) {

            }
            else {
                VegetationManager veggieRef = simulationManager.vegetationManager;
                veggieRef.selectedPlantParticleIndex--;
                if(veggieRef.selectedPlantParticleIndex < 0) {
                    veggieRef.selectedPlantParticleIndex = veggieRef.plantParticlesCBuffer.count - 1;
                    veggieRef.isPlantParticleSelected = true; // ???
                }
            }
        }
        else if(slotRef.kingdomID == 2) {
            if (slotRef.tierID == 0) {
                ZooplanktonManager zoopRef = simulationManager.zooplanktonManager;
                zoopRef.selectedAnimalParticleIndex--;
                if (zoopRef.selectedAnimalParticleIndex < 0) {
                    zoopRef.selectedAnimalParticleIndex = zoopRef.animalParticlesCBuffer.count - 1;
                    zoopRef.isAnimalParticleSelected = true; // ???
                }
            }
            else {
                ClickPrevAgent();
            }
        }        
    }
    
    public void ClickWatcherCycleTargetNext() {
        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;  // ************* TEMP!!!!!!!!!

        if(slotRef.kingdomID == 1) {
            if(slotRef.tierID == 0) {
            }
            else {
                VegetationManager veggieRef = simulationManager.vegetationManager;
                veggieRef.selectedPlantParticleIndex++;
                if(veggieRef.selectedPlantParticleIndex > veggieRef.plantParticlesCBuffer.count - 1) {
                    veggieRef.selectedPlantParticleIndex = 0;
                    veggieRef.isPlantParticleSelected = true; // ???
                }
            }
        }
        else if(slotRef.kingdomID == 2) {
            if (slotRef.tierID == 0) {
                ZooplanktonManager zoopRef = simulationManager.zooplanktonManager;
                zoopRef.selectedAnimalParticleIndex++;
                if (zoopRef.selectedAnimalParticleIndex > zoopRef.animalParticlesCBuffer.count - 1) {
                    zoopRef.selectedAnimalParticleIndex = 0;
                    zoopRef.isAnimalParticleSelected = true; // ???
                }
            }
            else {
                ClickNextAgent();
            }
        } 
    }
    
    public void ClickPrevAgent() {
        Debug.Log("ClickPrevAgent");
        int newIndex = (simulationManager._NumAgents + cameraManager.targetAgentIndex - 1) % simulationManager._NumAgents;
        cameraManager.SetTargetAgent(simulationManager.agentsArray[newIndex], newIndex);                              
    }
    
    public void ClickNextAgent() {
        Debug.Log("ClickNextAgent");
        int newIndex = (cameraManager.targetAgentIndex + 1) % simulationManager._NumAgents;
        cameraManager.SetTargetAgent(simulationManager.agentsArray[newIndex], newIndex);                
    }

    public void StopFollowingAgent() {
        cameraManager.isFollowingAgent = false;
    }
    
    public void StartFollowingAgent() {
        cameraManager.isFollowingAgent = true;
        //uiManagerRef.globalResourcesUI.CreateBrainGenomeTexture(uiManagerRef.cameraManager.targetAgent.candidateRef.candidateGenome);
    }

    public void StopFollowingPlantParticle() {
        cameraManager.isFollowingPlantParticle = false;
        
    }
    public void StartFollowingPlantParticle() {
        cameraManager.isFollowingPlantParticle = true;
        cameraManager.isFollowingAnimalParticle = false; 
        watcherSelectedTrophicSlotRef = simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef =   
    }
    
    public void StopFollowingAnimalParticle() {
        cameraManager.isFollowingAnimalParticle = false;        
    }
    
    public void StartFollowingAnimalParticle() {
        cameraManager.isFollowingAnimalParticle = true;  
        cameraManager.isFollowingPlantParticle = false;
        watcherSelectedTrophicSlotRef = simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
    }
}