using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatcherUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;
    public bool isDim;

    public AudioSource audioSource00;
    public AudioSource audioSource01;
    public AudioSource audioSource02;
    public AudioSource audioSource03;
    private int callTickCounter = 90;
    private int callTickCooldownCounter = 0;

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
    private bool isFollowBehaviorPanelOn = false;
    public GameObject panelFollowGenome;
    public Text textFollowGenome;
    private bool isFollowGenomePanelOn = false;
    public GameObject panelFollowHistory;
    public Text textFollowHistory;
    private bool isFollowHistoryPanelOn = false;


    public TrophicSlot watcherSelectedTrophicSlotRef; // plant, zooplankton, or vertebrate
    //public bool isWatcherTargetLayerLocked;
    //public Text textWatcherPanelTargetLayer;
    public GameObject panelWatcherSpiritMain;
    public GameObject panelWatcherExpand;
    
    public Text TextCommonStatsA;
    private int curWatcherPanelPlantsPageNum;
    private int curWatcherPanelZooplanktonPageNum;
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

        
        //callTickCounter++;
        //private int callTickCooldownCounter = 0;

        //TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot; // *************   CHANGE THIS!!!!! ************
         
        if(watcherSelectedTrophicSlotRef == null) {
            
            //imageColorBar.color = Color.black;
        }
        else {
            //imageColorBar.color = watcherSelectedTrophicSlotRef.color;
            //imageWatcherCurTargetLayer.color = watcherSelectedTrophicSlotRef.color;
            
        }
        
        if(uiManagerRef.panelFocus != UIManager.PanelFocus.Watcher) {           
            panelWatcherExpand.SetActive(false);
            
        }
        else {            
            
            panelWatcherExpand.SetActive(true);
        }
        
        if(uiManagerRef.cameraManager.isFollowingPlantParticle) {
            uiManagerRef.cameraManager.targetPlantWorldPos = uiManagerRef.gameManager.simulationManager.vegetationManager.selectedPlantParticleData.worldPos;
        }
        if(uiManagerRef.cameraManager.isFollowingAnimalParticle) {
            uiManagerRef.cameraManager.targetZooplanktonWorldPos = uiManagerRef.gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleData.worldPos;
        }

        //panelWatcherSpiritVertebratesText.SetActive(false);
      
        TextCommonStatsA.gameObject.SetActive(false);
        if(watcherSelectedTrophicSlotRef != null && uiManagerRef.panelFocus == UIManager.PanelFocus.Watcher) {
            TextCommonStatsA.gameObject.SetActive(true);

            int critterIndex = uiManagerRef.cameraManager.targetAgentIndex;
            Agent agent = uiManagerRef.gameManager.simulationManager.agentsArray[critterIndex];

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
                newInspectAgentEnergyMat.SetFloat("_Value", Mathf.Clamp01((agent.coreModule.energy * Mathf.Sqrt(agent.currentBiomass)) * 1f));
                newInspectAgentStaminaMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stamina[0] * 1f));
                newInspectAgentStomachMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stomachContentsNorm * 1f));

                newInspectAgentHealthMat.SetFloat("_HealthHead", Mathf.Clamp01(agent.coreModule.healthHead));
                newInspectAgentHealthMat.SetFloat("_HealthBody", Mathf.Clamp01(agent.coreModule.healthBody));
                newInspectAgentHealthMat.SetFloat("_HealthExternal", Mathf.Clamp01(agent.coreModule.healthExternal));
                newInspectAgentAgeMat.SetFloat("_Value", Mathf.Clamp01((float)agent.ageCounter * 0.0005f));
                newInspectAgentAgeMat.SetFloat("_Age", agent.ageCounter);
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
                newInspectAgentThrottleMat.SetTexture("_VelocityTex", uiManagerRef.gameManager.simulationManager.environmentFluidManager._VelocityPressureDivergenceMain);
                newInspectAgentThrottleMat.SetFloat("_AgentCoordX", agent.ownPos.x / SimulationManager._MapSize);
                newInspectAgentThrottleMat.SetFloat("_AgentCoordY", agent.ownPos.y / SimulationManager._MapSize);
                
                TextCommonStatsA.gameObject.SetActive(false);  // non-vertebrates just share one textbox for now

                
                //panelWatcherSpiritVertebratesText.SetActive(false);
                
                //textWatcherVertebratePageNum.text = "PAGE " + (curWatcherPanelVertebratePageNum + 1).ToString() + " of 4";
                textWatcherTargetIndex.text = "#" + agent.index.ToString();
                
                // pages:
                //if(curWatcherPanelVertebratePageNum == 0) {
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
                /*if(!agent.isActing) {
                    statusStr = "INACTIVE";
                    statusColor = Color.black;
                    healthColor = Color.black;
                }*/

                //textVertebrateGen.text = "Gen #" + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                textVertebrateLifestage.text = "Age: " + agent.ageCounter.ToString();// + ", stateID: " + developmentStateID;
                textVertebrateLifestage.color = healthColor;
                textVertebrateStatus.text = statusStr; // "activity: " + curActivityID;
                
                //where do hud elements get updated?? ***
                string genomeString = "ID: " + agent.candidateRef.candidateID.ToString() + "\n";
                genomeString += "Species " + agent.speciesIndex.ToString();
                genomeString += "\nGeneration " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                // BASE STATS FROM GENOME:
                //genomeString += "\n\nBase Size: " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2"); 
                genomeString += "\nFullsize: ( " + agent.fullSizeBoundingBox.x.ToString("F2") + ", " + agent.fullSizeBoundingBox.y.ToString("F2") + ", " + agent.fullSizeBoundingBox.z.ToString("F2") + " )\n";
                genomeString += "\n# Neurons: " + agent.brain.neuronList.Count.ToString() + ", # Axons: " + agent.brain.axonList.Count.ToString();
                //genomeString += "\n# In/Out: " + agent.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count.ToString() + ", Hid: " + agent.candidateRef.candidateGenome.brainGenome.hiddenNeuronList.Count.ToString();
                //genomeString += "\nCID: " + agent.candidateRef.candidateID.ToString() + ", gen# " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                genomeString += "\nBONUSES:\nDamage: " + agent.coreModule.damageBonus.ToString("F2") + "\nSpeed: " + agent.coreModule.speedBonus.ToString("F2") + "\nHealth: " + agent.coreModule.healthBonus.ToString("F2") + "\nEnergy: " + agent.coreModule.energyBonus.ToString("F2") + "\n";
                genomeString += "\nDIET:\nDecay: " + agent.coreModule.foodEfficiencyDecay.ToString("F2") + "\nPlant: " + agent.coreModule.foodEfficiencyPlant.ToString("F2") + "\nMeat: " + agent.coreModule.foodEfficiencyMeat.ToString("F2") + "\n";
                  
                genomeString += "\nSENSORS:\n";
                genomeString += "Comms= " + agent.candidateRef.candidateGenome.bodyGenome.communicationGenome.useComms.ToString() + "\n";
                genomeString += "Enviro: " + agent.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useWaterStats.ToString() + "\n";
                CritterModuleFoodSensorsGenome foodGenome = agent.candidateRef.candidateGenome.bodyGenome.foodGenome;
                genomeString += "Food: " + foodGenome.useNutrients.ToString() + ", " + foodGenome.usePos.ToString() + ", " + foodGenome.useDir.ToString() + ", " + foodGenome.useStats.ToString() + ", " + foodGenome.useEggs.ToString() + ", " + foodGenome.useCorpse.ToString() + "\n";
                genomeString += "Friend: " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.usePos.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.useDir.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.useVel.ToString() + "\n";
                genomeString += "Threat: " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.usePos.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useDir.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useVel.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useStats.ToString() + "\n";
            
                // History:
                string historyString = "";
                /*historyString += "\nMicrobesEaten: " + agent.totalFoodEatenZoop.ToString("F3");
                historyString += "\nAlgaeEaten: " + agent.totalFoodEatenPlant.ToString("F3");
                historyString += "\nMeatEaten: " + agent.totalFoodEatenCreature.ToString("F3");
                historyString += "\nCorpseEaten: " + agent.totalFoodEatenCorpse.ToString("F3");
                historyString += "\nEggEaten: " + agent.totalFoodEatenEgg.ToString("F3");
                historyString += "\nDamageTaken: " + agent.totalDamageTaken.ToString("F3");
                historyString += "\nDamageDealt: " + agent.totalDamageDealt.ToString("F3");
                historyString += "\nTimesAttacked: " + agent.totalTimesAttacked.ToString("F0");
                historyString += "\nTimesDashed: " + agent.totalTimesDashed.ToString("F0");
                historyString += "\nTimesPregnant: " + agent.totalTimesPregnant.ToString("F0");
                historyString += "\nTimeRested: " + agent.totalTicksRested.ToString("F0");
                */
                    
                string textString = "Event Log! [" + agent.index.ToString() + "]";
                int maxEventsToDisplay = 8;
                int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
                int startIndex = Mathf.Max(0, agent.agentEventDataList.Count - maxEventsToDisplay);                   
                string eventString = "";
                for(int q = agent.agentEventDataList.Count - 1; q >= startIndex; q--) {
                    eventString += "\n[" + agent.agentEventDataList[q].eventFrame.ToString() + "] " + agent.agentEventDataList[q].eventText;
                }                

                textFollowGenome.text = "(Genome Stats HERE) " + genomeString;
                textFollowHistory.text = "(Genome Stats HERE) " + historyString;

                string textStringLog = "Event Log! Agent[" + agent.index.ToString() + "]";                    
                // Agent Event Log:
                int maxEventsToDisplayLog = 12;
                int numEventsLog = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplayLog);
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
                int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
                string lifeStageProgressTxt = " " + agent.curLifeStage.ToString() + " " + curCount.ToString() + "/" + maxCount.ToString() + "  " + progressPercent.ToString() + "% ";
                
                //float restEffector = 0f;
                //float dashEffector = 0f;
                //float guardEffector = 0f;
                //float biteEffector = 0f;
                //float attackEffector = 0f;
                //float otherEffector = 0f;

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
                /*
                // AUDIO TESTING:
                bool isCalling = false;
                if(agent.coreModule.defendEffector[0] >= agent.coreModule.healEffector[0] && 
                agent.coreModule.defendEffector[0] >= agent.coreModule.dashEffector[0] &&
                agent.coreModule.defendEffector[0] >= agent.coreModule.mouthFeedEffector[0] &&
                agent.coreModule.defendEffector[0] >= agent.coreModule.mouthAttackEffector[0]) {
                    if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {
                        isCalling = true;
                    }
                }
                if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {
                    isCalling = true;
                }

                        
                //if(isCalling) {
                if(agent.communicationModule.outComm0[0] > 0.15f) {
                    if(audioSource00.isPlaying) {

                    }
                    else {
                        audioSource00.pitch = 2f;
                        audioSource00.volume = agent.communicationModule.outComm0[0] * 1f;
                        audioSource00.Play();
                    }                            
                }
                else {
                    audioSource00.Stop();
                }

                if(agent.communicationModule.outComm1[0] > 0.15f) {
                    if(audioSource01.isPlaying) {

                    }
                    else {
                        audioSource01.pitch = 3f;
                        audioSource01.volume = agent.communicationModule.outComm1[0] * 0.75f;
                        audioSource01.Play();
                    }                            
                }
                else {
                    audioSource01.Stop();
                }

                if(agent.communicationModule.outComm2[0] > 0.15f) {
                    if(audioSource02.isPlaying) {

                    }
                    else {
                        audioSource02.pitch = 6f + agent.communicationModule.outComm3[0] * 3f;
                        audioSource02.volume = agent.communicationModule.outComm2[0] * 0.5f;
                        audioSource02.Play();
                    }                            
                }
                else {
                    audioSource02.Stop();
                }
                            
                    if(agent.communicationModule.outComm3[0] > 0.15f) {
                        if(audioSource03.isPlaying) {

                        }
                        else {
                            audioSource03.pitch = 9f;
                            audioSource03.volume = agent.communicationModule.outComm3[0] * 0.25f;
                            audioSource03.Play();
                        }                            
                    }
                    else {
                        audioSource03.Stop();
                                
                    }
                //}
                //else {
                //    audioSource00.Stop();
                //    audioSource01.Stop();
                //    audioSource02.Stop();
                //    audioSource03.Stop();
                //}
                */
                        

                agentBehaviorOneHot.UpdateExtras(agent);

                widgetAgentStatus.UpdateBars((agent.coreModule.healthBody + agent.coreModule.healthHead + agent.coreModule.healthExternal) / 3f,
                                                agent.coreModule.energy * agent.currentBiomass,
                                                agent.coreModule.stomachContentsNorm,
                                                agent.currentBiomass,
                                                agent.coreModule.stamina[0]);
                        
                        
                /*               
                string brainString = "BRAIN PAGE! 3";
                brainString += "\n" + agent.brain.neuronList.Count.ToString() + " Neurons    " + agent.brain.axonList.Count.ToString() + " Axons";
                  
                string brainInputsTxt = "\nINPUTS:";
                for(int n = 0; n < agent.brain.neuronList.Count; n++) {
                    if(agent.brain.neuronList[n].neuronType == NeuronGenome.NeuronType.In) {
                                    
                        if (n % 2 == 0) {
                            brainInputsTxt += "\n";
                        }
                        float neuronValue = agent.brain.neuronList[n].currentValue[0];
                        brainInputsTxt += agent.brain.neuronList[n].name + " ";
                        if(neuronValue < -0.2f) {
                            brainInputsTxt += "<color=#FF6644FF>";
                        }
                        else if(neuronValue > 0.2f) {
                            brainInputsTxt += "<color=#44FF66FF>";
                        }
                        else {
                            brainInputsTxt += "<color=#A998B5FF>";
                        }
                        brainInputsTxt += "[" + n.ToString() + "] " + agent.brain.neuronList[n].currentValue[0].ToString("F2") + "</color>  ";
                                    
                    }
                }
                string brainOutputsTxt = "\n\nOUTPUTS:\n";
                for(int o = 0; o < agent.brain.neuronList.Count; o++) {
                    if(agent.brain.neuronList[o].neuronType == NeuronGenome.NeuronType.Out) {
                                    
                        if (o % 2 == 0) {
                            brainOutputsTxt += "\n";
                        }
                        float neuronValue = agent.brain.neuronList[o].currentValue[0];
                        brainOutputsTxt += agent.brain.neuronList[o].name + " ";
                        if(neuronValue < -0.2f) {
                            brainOutputsTxt += "<color=#FF6644FF>";
                        }
                        else if(neuronValue > 0.2f) {
                            brainOutputsTxt += "<color=#44FF66FF>";
                        }
                        else {
                            brainOutputsTxt += "<color=#A998B5FF>";
                        }
                        brainOutputsTxt += "[" + o.ToString() + "] " + agent.brain.neuronList[o].currentValue[0].ToString("F2") + "</color>  ";
                                    
                        brainOutputsTxt += "";
                                    
                    }
                }
                brainString += brainInputsTxt + brainOutputsTxt;

                textWatcherVertebrateBrain.text = brainString;
                //}
                */
                        
                        

                //panelWatcherSpiritVertebratesText.SetActive(true);
                        
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
        
        if(watcherSelectedTrophicSlotRef == null) {

            //panelWatcherExpand.SetActive(false);
            //panelWatcherExpand.SetActive(false);
           // imageWatcherInactiveOverlay.gameObject.SetActive(true);
        }
    }

	public void UpdateWatcherPanelUI(TrophicLayersManager layerManager) {
        //animatorWatcherUI.SetBool("_IsOpen", isOpen);

        UpdateUI(layerManager);

        if(uiManagerRef.cameraManager.isFollowingAgent) {  // only active when following a selected critter -- make sure it only contains:
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
            uiManagerRef.panelFocus = UIManager.PanelFocus.Watcher;
            //uiManagerRef.curActiveTool = UIManager.ToolType.None;

            //animatorWatcherUI.SetBool("_IsOpen", true);
        }
        else {
            uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
            //animatorWatcherUI.SetBool("_IsOpen", false);
        }
        //watcherLockedTrophicSlotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;//
    }
    public void ClickSnoopModeButton() {        
        //uiManagerRef.isBrushModeON_snoopingOFF = false;
        uiManagerRef.panelFocus = UIManager.PanelFocus.Watcher;
        //uiManagerRef.curActiveTool = UIManager.ToolType.None;
    }
    public void ActivateWatcherPanel() {
        isOpen = true;
        uiManagerRef.panelFocus = UIManager.PanelFocus.Watcher;
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
                VegetationManager veggieRef = uiManagerRef.gameManager.simulationManager.vegetationManager;
                veggieRef.selectedPlantParticleIndex--;
                if(veggieRef.selectedPlantParticleIndex < 0) {
                    veggieRef.selectedPlantParticleIndex = veggieRef.plantParticlesCBuffer.count - 1;
                    veggieRef.isPlantParticleSelected = true; // ???
                }
            }
        }
        else if(slotRef.kingdomID == 2) {
            if (slotRef.tierID == 0) {
                ZooplanktonManager zoopRef = uiManagerRef.gameManager.simulationManager.zooplanktonManager;
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
                VegetationManager veggieRef = uiManagerRef.gameManager.simulationManager.vegetationManager;
                veggieRef.selectedPlantParticleIndex++;
                if(veggieRef.selectedPlantParticleIndex > veggieRef.plantParticlesCBuffer.count - 1) {
                    veggieRef.selectedPlantParticleIndex = 0;
                    veggieRef.isPlantParticleSelected = true; // ???
                }
            }
        }
        else if(slotRef.kingdomID == 2) {
            if (slotRef.tierID == 0) {
                ZooplanktonManager zoopRef = uiManagerRef.gameManager.simulationManager.zooplanktonManager;
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
        
        int newIndex = (uiManagerRef.gameManager.simulationManager._NumAgents + uiManagerRef.cameraManager.targetAgentIndex - 1) % uiManagerRef.gameManager.simulationManager._NumAgents;
        uiManagerRef.cameraManager.SetTargetAgent(uiManagerRef.gameManager.simulationManager.agentsArray[newIndex], newIndex);          
                           
    }
    public void ClickNextAgent() {
        Debug.Log("ClickNextAgent");
        
        int newIndex = (uiManagerRef.cameraManager.targetAgentIndex + 1) % uiManagerRef.gameManager.simulationManager._NumAgents;
        uiManagerRef.cameraManager.SetTargetAgent(uiManagerRef.gameManager.simulationManager.agentsArray[newIndex], newIndex);                
    }
    

    public void StopFollowingAgent() {
        uiManagerRef.cameraManager.isFollowingAgent = false;
        
    }
    public void StartFollowingAgent() {
        uiManagerRef.cameraManager.isFollowingAgent = true;

        //uiManagerRef.globalResourcesUI.CreateBrainGenomeTexture(uiManagerRef.cameraManager.targetAgent.candidateRef.candidateGenome);
    }

    public void StopFollowingPlantParticle() {
        uiManagerRef.cameraManager.isFollowingPlantParticle = false;
        
    }
    public void StartFollowingPlantParticle() {
        uiManagerRef.cameraManager.isFollowingPlantParticle = true;
        uiManagerRef.cameraManager.isFollowingAnimalParticle = false; 
        watcherSelectedTrophicSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];
        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = 

        
    }
    public void StopFollowingAnimalParticle() {
        uiManagerRef.cameraManager.isFollowingAnimalParticle = false;        
    }
    public void StartFollowingAnimalParticle() {
        uiManagerRef.cameraManager.isFollowingAnimalParticle = true;  
        uiManagerRef.cameraManager.isFollowingPlantParticle = false;
        watcherSelectedTrophicSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
    }

    public void ClickWatcherPage0() {
        curWatcherPanelPlantsPageNum = 0;
        curWatcherPanelZooplanktonPageNum = 0;
        curWatcherPanelVertebratePageNum = 0;
    }
    public void ClickWatcherPage1() {
        curWatcherPanelPlantsPageNum = 1;
        curWatcherPanelZooplanktonPageNum = 1;
        curWatcherPanelVertebratePageNum = 1;
    }
    public void ClickWatcherPage2() {
        curWatcherPanelPlantsPageNum = 2;
        curWatcherPanelZooplanktonPageNum = 2;
        curWatcherPanelVertebratePageNum = 2;
    }
    public void ClickWatcherPage3() {
        curWatcherPanelPlantsPageNum = 3;
        curWatcherPanelZooplanktonPageNum = 3;
        curWatcherPanelVertebratePageNum = 3;
    }
    
}
