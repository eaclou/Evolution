using System.Collections;
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
    private int callTickCounter = 90;
    //private int callTickCooldownCounter = 0;

    public AgentBehaviorOneHot agentBehaviorOneHot;
    public WidgetAgentStatus widgetAgentStatus;

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

    public float isPlantParticleHighlight;
    public float isZooplanktonHighlight;
    public float isVertebrateHighlight;

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
    public Text textVertebrateLifestage;
    public Text textVertebrateStatus;
    //public GameObject panelWatcherVertebrateWidgets01;
    public GameObject panelWatcherSpiritVertebratesText;  // 1
    public Text textWatcherVertebrateText;
    //public GameObject panelWatcherSpiritVertebratesGenome; // 2
    //public Text textWatcherVertebrateGenome;
    //public GameObject panelWatcherSpiritVertebratesBrain; // 3
   // public Text textWatcherVertebrateBrain;

    /*public GameObject panelWatcherSpiritZooplankton;
    public Text textZoopGen;
    public Text textZoopLifestage;
    public Text textZoopSize;
    public Text textZoopHealth;
    public Text textZoopStatus;
    public Text textZoopLog;

    public GameObject panelWatcherSpiritPlants;
    public Text textPlantGen;
    public Text textPlantLifestage;
    public Text textPlantSize;
    public Text textPlantHealth;
    public Text textPlantStatus;
    public Text textPlantLog;
    */
    //public GameObject panelWatcherSpiritAlgae;
    //public GameObject panelWatcherSpiritDecomposers;
    //public GameObject panelWatcherSpiritTerrain;
    //public Text textWatcherVertebratePageNum;
    public Text textWatcherTargetIndex;
    
    //Inspect!!!
    //public bool isWatcherPanelOn = false;
    
    //public Button buttonInspectCyclePrevSpecies;
    //public Button buttonInspectCycleNextSpecies;
    public Button buttonInspectCyclePrevAgent;
    public Button buttonInspectCycleNextAgent;
    public Text textStomachContents;
    public Text textEnergy;
    public Text textHealth;
    public Text textWaste;
    //public Text textDimensionsWidth;
    //public Text textDimensionsLength;
    //public Text textSpeciesID;
    //public Text textAgentID;
    //public Text textLifeCycle;
    //public GameObject panelInspectHUD;
    //public Animator animatorInspectPanel;
    //public Text textInspectData;

    //public GameObject panelNewInspect;
    public Text textNewInspectAgentName;
    public Material newInspectAgentEnergyMat;
    public Material newInspectAgentStaminaMat;
    public Material newInspectAgentStomachMat;
    public Material newInspectAgentAgeMat;
    public Material newInspectAgentHealthMat;
    public Material newInspectAgentThrottleMat;
    public Material newInspectAgentCommsMat;
    public Material newInspectAgentStateMat;
    public Material newInspectAgentCurActivityMat;
    public Material newInspectAgentWasteMat;
    public Material newInspectAgentBrainMat;
    public Text textNewInspectLog;


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

        if(uiManagerRef.panelFocus != PanelFocus.Watcher) {           
            panelWatcherExpand.SetActive(false);            
        }
        else {
            panelWatcherExpand.SetActive(true);
        }
        
        if(cameraManager.isFollowingPlantParticle) {
            cameraManager.targetPlantWorldPos = simulationManager.vegetationManager.selectedPlantParticleData.worldPos;
        }
        if(cameraManager.isFollowingAnimalParticle) {
            cameraManager.targetZooplanktonWorldPos = simulationManager.zooplanktonManager.selectedAnimalParticleData.worldPos;
        }

        //panelWatcherSpiritVertebratesText.SetActive(false);
      
        TextCommonStatsA.gameObject.SetActive(false);
        if(watcherSelectedTrophicSlotRef != null && uiManagerRef.panelFocus == PanelFocus.Watcher) {
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

            textNewInspectAgentName.text = "Critter #" + agent.candidateRef.candidateID.ToString(); //.candidateGenome.bodyGenome.coreGenome.name;
            textNewInspectAgentName.color = textColorSecondary; // uiManagerRef.colorVertebratesLayer;

            if(agent.coreModule != null) {
                
                followCreaturePanel.SetActive(true);

                panelFollowStatus.SetActive(isFollowStatusPanelOn);
                panelFollowBehavior.SetActive(isFollowBehaviorPanelOn);
                //panelFollowGenome.SetActive(isFollowGenomePanelOn);
                panelFollowHistory.SetActive(isFollowHistoryPanelOn);
                
                textStomachContents.text = "STOMACH " + Mathf.Clamp01(agent.coreModule.stomachContentsNorm * 1f).ToString("F5");
                textEnergy.text = "ENERGY " + (agent.coreModule.energy / agent.currentBiomass).ToString("F5");
                textHealth.text = "HEALTH " + agent.coreModule.healthBody.ToString("F5");
                textWaste.text = "WASTE " + agent.wasteProducedLastFrame.ToString("F5");

                textNewInspectLog.text = agent.stringCauseOfDeath.ToString() + ", " + agent.cooldownFrameCounter.ToString() + " / " + agent.cooldownDuration.ToString(); // agent.lastEvent;
                newInspectAgentEnergyMat.SetFloat("_Value", Mathf.Clamp01((agent.coreModule.energy * Mathf.Sqrt(agent.currentBiomass)) * 0.33f));
                newInspectAgentStaminaMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stamina[0] * 1f));
                newInspectAgentStomachMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stomachContentsNorm * 1f));

                newInspectAgentHealthMat.SetFloat("_HealthHead", Mathf.Clamp01(agent.coreModule.healthHead));
                newInspectAgentHealthMat.SetFloat("_HealthBody", Mathf.Clamp01(agent.coreModule.healthBody));
                newInspectAgentHealthMat.SetFloat("_HealthExternal", Mathf.Clamp01(agent.coreModule.healthExternal));
                newInspectAgentAgeMat.SetFloat("_Value", Mathf.Clamp01((float)agent.ageCounter * 0.0005f));
                newInspectAgentAgeMat.SetFloat("_Age", Mathf.RoundToInt(agent.ageCounter * 0.1f));
                int developmentStateID = 0;
                int curActivityID = 0;
                if(agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                    developmentStateID = 7;
                    //curActivityID = 7;
                }
                if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {

                    // state
                    if (agent.ageCounter < 1000) {
                        developmentStateID = 1;
                    }
                    else {
                        if (agent.ageCounter < 10000) {
                            developmentStateID = 2;
                        }
                    }

                    if (agent.sizePercentage > 0.5f) {
                        developmentStateID = 3;
                    }
                            
                    // curActivity
                    if(agent.isPregnantAndCarryingEggs) {
                        curActivityID = 6;
                    }
                    if(agent.isFeeding) {
                        curActivityID = 1;
                    }
                    if(agent.isAttacking) {
                        curActivityID = 2;
                    }
                    if(agent.isDashing) {
                        curActivityID = 3;
                    }
                    if(agent.isDefending) {
                        curActivityID = 4;
                    }
                    if(agent.isResting) {
                        curActivityID = 5;
                    }
                            
                    if(agent.isCooldown) {
                        curActivityID = 7;
                    }
                }
                newInspectAgentStateMat.SetInt("_StateID", developmentStateID);
                newInspectAgentCurActivityMat.SetInt("_CurActivityID", curActivityID);
                newInspectAgentBrainMat.SetFloat("_Value", Mathf.Clamp01((float)agent.brain.axonList.Count * 0.05f + (float)agent.brain.neuronList.Count * 0.05f));
                newInspectAgentWasteMat.SetFloat("_Value", Mathf.Clamp01(agent.wasteProducedLastFrame * 1000f));
                newInspectAgentThrottleMat.SetFloat("_ThrottleX", Mathf.Clamp01(agent.smoothedThrottle.x));
                newInspectAgentThrottleMat.SetFloat("_ThrottleY", Mathf.Clamp01(agent.smoothedThrottle.y));
                newInspectAgentThrottleMat.SetTexture("_VelocityTex", simulationManager.environmentFluidManager._VelocityPressureDivergenceMain);
                newInspectAgentThrottleMat.SetFloat("_AgentCoordX", agent.ownPos.x / SimulationManager._MapSize);
                newInspectAgentThrottleMat.SetFloat("_AgentCoordY", agent.ownPos.y / SimulationManager._MapSize);
                
                TextCommonStatsA.gameObject.SetActive(false);  // non-vertebrates just share one textbox for now

                //textWatcherVertebratePageNum.text = "PAGE " + (curWatcherPanelVertebratePageNum + 1).ToString() + " of 4";
                textWatcherTargetIndex.text = "#" + agent.index.ToString();
                                
                textNewInspectLog.text = "";
                textWatcherVertebrateHUD.text = "";

                string statusStr = "Alive!";
                Color statusColor = Color.white;
                Color healthColor = Color.green;
                            
                if(agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                    statusStr = "Dead! (Decaying)";
                    statusColor = Color.red;
                    healthColor = Color.red;
                }
                if(agent.curLifeStage == Agent.AgentLifeStage.Egg) {
                    statusStr = "Egg!";
                    healthColor = Color.yellow;
                }
                if(agent.coreModule.healthBody <= 0f) {
                    statusStr = "Died of Injury";
                }                

                //textVertebrateGen.text = "Gen #" + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                textVertebrateLifestage.text = "Age: " + (0.1f * agent.ageCounter).ToString("F0");// + ", stateID: " + developmentStateID;
                textVertebrateLifestage.color = healthColor;
                textVertebrateStatus.text = statusStr; // "activity: " + curActivityID;
                
                                   
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
                
                agentBehaviorOneHot.UpdateBars( agent.coreModule.healEffector[0],
                                                agent.coreModule.dashEffector[0],
                                                agent.coreModule.defendEffector[0],
                                                agent.coreModule.mouthFeedEffector[0],
                                                agent.coreModule.mouthAttackEffector[0],
                                                agent.communicationModule.outComm0[0], agent.isCooldown);

                if(agent.communicationModule.outComm3[0] > 0.25f) {
                    callTickCounter = Mathf.Min(200, callTickCounter++);
                            
                }
                else {
                    callTickCounter = Mathf.Max(0, callTickCounter--);
                }
               
                agentBehaviorOneHot.UpdateExtras(agent);

                widgetAgentStatus.UpdateBars((agent.coreModule.healthBody + agent.coreModule.healthHead + agent.coreModule.healthExternal) / 3f,
                                                agent.coreModule.energy * agent.currentBiomass,
                                                agent.coreModule.stomachContentsNorm,
                                                agent.currentBiomass,
                                                agent.coreModule.stamina[0]);   
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
            uiManagerRef.panelFocus = PanelFocus.Watcher;
            //uiManagerRef.curActiveTool = UIManager.ToolType.None;

            //animatorWatcherUI.SetBool("_IsOpen", true);
        }
        else {
            uiManagerRef.panelFocus = PanelFocus.WorldHub;
            //animatorWatcherUI.SetBool("_IsOpen", false);
        }
        //watcherLockedTrophicSlotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;//
    }
    public void ClickSnoopModeButton() {        
        //uiManagerRef.isBrushModeON_snoopingOFF = false;
        uiManagerRef.panelFocus = PanelFocus.Watcher;
        //uiManagerRef.curActiveTool = UIManager.ToolType.None;
    }
    public void ActivateWatcherPanel() {
        isOpen = true;
        uiManagerRef.panelFocus = PanelFocus.Watcher;
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
