using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatcherUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;

    //public Image imageWatcherButtonMIP;  // superscript button over watcher toolbar Button
    public Image imageWatcherCurTargetLayer; // in watcher panel
    public Text textTargetLayer;
    //public Button buttonWatcherLock;
    public Button buttonHighlightingToggle;
    //public Button buttonFollowingToggle;
    
    public bool isFollow;
    //public TrophicSlot targetSlotRef;
    //public bool isSelected;

    public Animator animatorWatcherUI;

    public Image imageColorBar;
    public Image imageIsSnooping;
    public Image imageWatcherInactiveOverlay;
    public Text textIsSnooping;

    public float isPlantParticleHighlight;
    public float isZooplanktonHighlight;
    public float isVertebrateHighlight;
        
    public TrophicSlot watcherSelectedTrophicSlotRef; // plant, zooplankton, or vertebrate
    //public bool isWatcherTargetLayerLocked;
    public Text textWatcherPanelTargetLayer;
    public GameObject panelWatcherSpiritMain;
    public GameObject panelWatcherExpand;
    public Text TextCommonStatsA;
    private int curWatcherPanelPlantsPageNum;
    private int curWatcherPanelZooplanktonPageNum;
    private int curWatcherPanelVertebratePageNum;
    public Button buttonWatcherVertebrateCyclePage; // maybe not needed?
    public GameObject panelWatcherSpiritVertebratesHUD; // 0
    public Text textWatcherVertebrateHUD;
    public Text textVertebrateGen;
    public Text textVertebrateLifestage;
    public Text textVertebrateStatus;
    public GameObject panelWatcherSpiritVertebratesText;  // 1
    public Text textWatcherVertebrateText;
    public GameObject panelWatcherSpiritVertebratesGenome; // 2
    public Text textWatcherVertebrateGenome;
    public GameObject panelWatcherSpiritVertebratesBrain; // 3
    public Text textWatcherVertebrateBrain;

    public GameObject panelWatcherSpiritZooplankton;
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

    //public GameObject panelWatcherSpiritAlgae;
    //public GameObject panelWatcherSpiritDecomposers;
    //public GameObject panelWatcherSpiritTerrain;
    public Text textWatcherVertebratePageNum;
    public Text textWatcherTargetIndex;
    
    //Inspect!!!
    //public bool isWatcherPanelOn = false;
    
    //public Button buttonInspectCyclePrevSpecies;
    //public Button buttonInspectCycleNextSpecies;
    public Button buttonInspectCyclePrevAgent;
    public Button buttonInspectCycleNextAgent;
    //public Text textStomachContents;
    //public Text textEnergy;
    //public Text textHealth;
    //public Text textDiet;
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
    	
    private void UpdateUI(TrophicLayersManager layerManager) {
        //TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot; // *************   CHANGE THIS!!!!! ************
         
        if(watcherSelectedTrophicSlotRef == null) {
            //imageIsSnooping.color = Color.green;
            //textIsSnooping.text = "SNOOPING";
            imageColorBar.color = Color.black;
        }
        else {
            imageColorBar.color = watcherSelectedTrophicSlotRef.color;
            imageWatcherCurTargetLayer.color = watcherSelectedTrophicSlotRef.color;
            textWatcherPanelTargetLayer.color = watcherSelectedTrophicSlotRef.color;
        }
        
        if(uiManagerRef.panelFocus != UIManager.PanelFocus.Watcher) {
            imageIsSnooping.color = Color.gray;
            imageIsSnooping.sprite = uiManagerRef.spriteSpiritBrushWatcherOffIcon;
            textIsSnooping.text = "";
            panelWatcherExpand.SetActive(false);
            imageWatcherInactiveOverlay.gameObject.SetActive(true);
        }
        else {
            
            imageIsSnooping.color = Color.white;
            imageIsSnooping.sprite = uiManagerRef.spriteSpiritBrushWatcherIcon;
            textIsSnooping.text = "";
            panelWatcherExpand.SetActive(true);
            imageWatcherInactiveOverlay.gameObject.SetActive(false);
        }
        

        //buttonHighlightingToggle.GetComponentInChildren<Text>().text = "huh?"; // isHighlight.ToString();
        //buttonFollowingToggle.GetComponentInChildren<Text>().text = isFollow.ToString();
        
        if(uiManagerRef.cameraManager.isFollowingPlantParticle) {
            uiManagerRef.cameraManager.targetPlantWorldPos = uiManagerRef.gameManager.simulationManager.vegetationManager.selectedPlantParticleData.worldPos;
        }
        if(uiManagerRef.cameraManager.isFollowingAnimalParticle) {
            uiManagerRef.cameraManager.targetZooplanktonWorldPos = uiManagerRef.gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleData.worldPos;
        }

        //textNewInspectAgentName.text = "asda";

        panelWatcherSpiritVertebratesHUD.SetActive(false);
        panelWatcherSpiritVertebratesText.SetActive(false);
        panelWatcherSpiritVertebratesGenome.SetActive(false);
        panelWatcherSpiritVertebratesBrain.SetActive(false);
        panelWatcherSpiritZooplankton.SetActive(false);
        panelWatcherSpiritPlants.SetActive(false);
        //panelWatcherSpiritAlgae.SetActive(false);
        //panelWatcherSpiritDecomposers.SetActive(false);
        //panelWatcherSpiritTerrain.SetActive(false);
        //
        
        TextCommonStatsA.gameObject.SetActive(false);
        if(watcherSelectedTrophicSlotRef != null && uiManagerRef.panelFocus == UIManager.PanelFocus.Watcher) {
            TextCommonStatsA.gameObject.SetActive(true);
            textTargetLayer.text = watcherSelectedTrophicSlotRef.speciesName;
            //textTargetLayer.color = 

            string str = "";
            if (watcherSelectedTrophicSlotRef.kingdomID == 0) {
                /*

                Vector4 resourceGridSample = uiManagerRef.SampleTexture(uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nWaste    : " + (resourceGridSample.y * 1000f).ToString("F0");                    
                str += "\nDecomposers  : " + (resourceGridSample.z * 1000f).ToString("F0");

                Vector4 simTansferSample = uiManagerRef.SampleTexture(uiManagerRef.gameManager.simulationManager.vegetationManager.resourceSimTransferRT, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");
                //str += "\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + ")";

                panelWatcherSpiritDecomposers.SetActive(true);
                //UpdateWatcherDecomposersPanelUI();
                */
            }
            else if(watcherSelectedTrophicSlotRef.kingdomID == 1) {
                if(watcherSelectedTrophicSlotRef.tierID == 0) {
                    
                }
                else {
                    VegetationManager.PlantParticleData particleData = uiManagerRef.gameManager.simulationManager.vegetationManager.selectedPlantParticleData;

                    textNewInspectAgentName.text = "Name# " + particleData.index.ToString();
                    textNewInspectAgentName.color = uiManagerRef.colorPlantsLayer;
                    imageWatcherCurTargetLayer.sprite = uiManagerRef.spriteSpiritPlantIcon;

                    if(curWatcherPanelPlantsPageNum == 0) {
                        string strPlantStatus = "Alive!";
                        Color statusColor = Color.white;
                        Color healthColor = Color.green;
                        if(particleData.isDecaying > 0.5) {
                            strPlantStatus = "Dead! (Decaying)";
                            statusColor = Color.gray;
                            healthColor = Color.red;

                        }
                        if(particleData.isSwallowed > 0.5) {
                            strPlantStatus = "Dead! (Swallowed)";
                            healthColor = Color.red;
                        }
                        if(particleData.isActive > 0.5f) {

                        }
                        else {
                            strPlantStatus = "INACTIVE";
                            statusColor = Color.black;
                            healthColor = Color.black;
                        }

                        textPlantGen.text = "Gen: " + "coming soon"; // (particleData.genomeVector.w * 1f).ToString("F0");
                        textPlantGen.color = statusColor;
                        textPlantLifestage.text = "Age: " + (particleData.age * 100f).ToString("F0");
                        textPlantLifestage.color = statusColor;
                        textPlantSize.text = "Biomass: " + (particleData.biomass * 1000f).ToString("F0") + 
                                            "\nSize: " + "...";
                        textPlantSize.color = statusColor;
                        textPlantHealth.text = "Health: " + (particleData.health * 100f).ToString("F0") 
                                              + "\nNutrients Used: " + (particleData.nutrientsUsed * 1000000f).ToString("F0");
                        textPlantHealth.color = healthColor;

                        textPlantStatus.text = strPlantStatus;
                        textPlantStatus.color = healthColor;
                        textPlantLog.text = "ColorA: " + particleData.colorA.ToString() + "\nColorB: " + particleData.colorB.ToString();
                            //+ (particleData.genomeVector.x * 1f).ToString("F2") + ", " + (particleData.genomeVector.y * 1f).ToString("F2") + ", " + (particleData.genomeVector.x * 1f).ToString("F2");
                        textPlantLog.color = statusColor;
                    }
                    else if(curWatcherPanelPlantsPageNum == 1) {
                        

                        str += "\nPlant Particle # " + particleData.index.ToString() + "  [" + particleData.nearestCritterIndex.ToString() + "]";
                        //str += "\nCPU: " + gameManager.simulationManager.vegetationManager.tempClosestPlantParticleIndexAndPos.ToString();
                        str += "\nCoords [ " + particleData.worldPos.x.ToString("F0") + " , " + particleData.worldPos.y.ToString("F0");
                        str += "\nColorA (" + particleData.colorA.ToString() + ".";                        
                        str += "\n\nAge: " + (particleData.age * 1000f).ToString("F0");
                        str += "\nBiomass: " + (particleData.biomass * 1000f).ToString("F0");
                        str += "\nNutrients Used: " + (particleData.nutrientsUsed * 100000000f).ToString("F0");
                        str += "\nOxygen Produced: " + (particleData.oxygenProduced * 10000000f).ToString("F0");
                        str += "\nIsDecaying: " + (particleData.isDecaying).ToString("F0");
                        str += "\nIsSwallowed: " + (particleData.isSwallowed).ToString("F0");
                        str += "\n\nDistance: " + (new Vector2(uiManagerRef.gameManager.simulationManager.uiManager.theCursorCzar.curMousePositionOnWaterPlane.x, uiManagerRef.gameManager.simulationManager.uiManager.theCursorCzar.curMousePositionOnWaterPlane.y) - particleData.worldPos).magnitude;

                        Vector4 resourceGridSample = uiManagerRef.SampleTexture(uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                        str += "\n\nNutrients    : " + (resourceGridSample.x * 1000000f).ToString("F0");                    
                        //str += "\nAlgae        : " + (resourceGridSample.w * 1000f).ToString("F0");
                        Vector4 simTansferSample = uiManagerRef.SampleTexture(uiManagerRef.gameManager.simulationManager.vegetationManager.resourceSimTransferRT, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                        str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");
                
                        textWatcherTargetIndex.text = "#" + particleData.index.ToString();
                    }
                    
                    TextCommonStatsA.text = str;
                    panelWatcherSpiritPlants.SetActive(true);
                }
                    
            }
            else if(watcherSelectedTrophicSlotRef.kingdomID == 2) {
                if(watcherSelectedTrophicSlotRef.tierID == 0) {
                    imageWatcherCurTargetLayer.color = uiManagerRef.colorZooplanktonLayer;
                    imageWatcherCurTargetLayer.sprite = uiManagerRef.spriteSpiritZooplanktonIcon;

                    textNewInspectAgentName.text = "Name#" + uiManagerRef.gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString();
                    textNewInspectAgentName.color = uiManagerRef.colorZooplanktonLayer;

                    ZooplanktonManager.AnimalParticleData particleData = uiManagerRef.gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleData;
                    //textNewInspectAgentName.text = "Mittens";

                    if (curWatcherPanelZooplanktonPageNum == 0) {
                        
                        string strZoopStatus = "Alive!";
                        Color statusColor = Color.white;
                        Color healthColor = Color.green;
                        if(particleData.isDecaying > 0.5) {
                            strZoopStatus = "Dead! (Decaying)";
                            statusColor = Color.gray;
                            healthColor = Color.red;

                        }
                        if(particleData.isSwallowed > 0.5) {
                            strZoopStatus = "Dead! (Swallowed)";
                            healthColor = Color.red;
                        }
                        if(particleData.isActive > 0.5f) {

                        }
                        else {
                            strZoopStatus = "INACTIVE";
                            statusColor = Color.black;
                            healthColor = Color.black;
                        }

                        textZoopGen.text = "Gen: " + (particleData.genomeVector.w * 1f).ToString("F0");
                        textZoopGen.color = statusColor;
                        textZoopLifestage.text = "Age: " + (particleData.age * 100f).ToString("F0");
                        textZoopLifestage.color = statusColor;
                        textZoopSize.text = "Biomass: " + (particleData.biomass * 1000f).ToString("F0") + 
                                            "\nSize: " + "...";
                        textZoopSize.color = statusColor;
                        textZoopHealth.text = "Energy: " + (particleData.energy * 100f).ToString("F0") 
                                              + "\nAlgae Eaten: " + (particleData.algaeConsumed * 100000000f).ToString("F0");
                        textZoopHealth.color = healthColor;

                        textZoopStatus.text = strZoopStatus;
                        textZoopStatus.color = healthColor;
                        textZoopLog.text = "Genome: " + (particleData.genomeVector.x * 1f).ToString("F2") + ", " + (particleData.genomeVector.y * 1f).ToString("F2") + ", " + (particleData.genomeVector.x * 1f).ToString("F2");
                        textZoopLog.color = statusColor;
                        str = "";
                    }
                    else if (curWatcherPanelZooplanktonPageNum == 1) {
                        
                        
                        str += "\nZooplankton # " + uiManagerRef.gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString();
                        str += "\nCoords [ " + particleData.worldPos.x.ToString("F0") + " , " + particleData.worldPos.y.ToString("F0") + " ]"; //  Critter (" + gameManager.simulationManager.agentsArray[0].ownPos.ToString() + ")";
                        str += "\nAge: " + (particleData.age * 1000f).ToString("F0");
                        str += "\nBiomass: " + (particleData.biomass * 1000f).ToString("F0");
                        str += "\nEnergy: " + (particleData.energy * 100f).ToString();
                        str += "\nAlgae Eaten: " + (particleData.algaeConsumed * 1000000000f).ToString();
                        str += "\nOxygen Used: " + (particleData.oxygenUsed * 1000000f).ToString("F0");
                        str += "\nWaste Produced: " + (particleData.wasteProduced * 1000000000f).ToString();                        
                        str += "\nVelocity (" + (particleData.velocity.x * 1000f).ToString("F0") + ", " + (particleData.velocity.y * 1000f).ToString("F0") + ")";
                        str += "\nGenome: " + (particleData.genomeVector * 1f).ToString("F2");
                        str += "\nIsDecaying: " + (particleData.isDecaying).ToString("F0");
                        str += "\nIsSwallowed: " + (particleData.isSwallowed).ToString("F0");
                        //str += "\n\nDistance: " + gameManager.simulationManager.zooplanktonManager.closestZooplanktonArray[0].y.ToString(); // (gameManager.simulationManager.agentsArray[0].ownPos - new Vector2(particleData.worldPos.x, particleData.worldPos.y)).magnitude;
                  
                        textWatcherTargetIndex.text = "#" + uiManagerRef.gameManager.simulationManager.zooplanktonManager.selectedAnimalParticleIndex.ToString();
                    
                    }

                    panelWatcherSpiritZooplankton.SetActive(true);
                    
                    TextCommonStatsA.text = str;
                }
                else {
                    imageWatcherCurTargetLayer.color = uiManagerRef.colorVertebratesLayer;
                    imageWatcherCurTargetLayer.sprite = uiManagerRef.spriteSpiritVertebrateIcon;

                    int critterIndex = uiManagerRef.cameraManager.targetAgentIndex;
                    Agent agent = uiManagerRef.gameManager.simulationManager.agentsArray[critterIndex];

                    textNewInspectAgentName.text = agent.candidateRef.candidateGenome.bodyGenome.coreGenome.name;
                    textNewInspectAgentName.color = uiManagerRef.colorVertebratesLayer;

                    if(agent.coreModule != null) {
                        //textNewInspectAgentName.text = agent.candidateRef.candidateGenome.bodyGenome.coreGenome.name;

                        textNewInspectLog.text = agent.stringCauseOfDeath.ToString() + ", " + agent.cooldownFrameCounter.ToString() + " / " + agent.cooldownDuration.ToString(); // agent.lastEvent;
                        newInspectAgentEnergyMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.energy * 0.01f));
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
                            if(agent.sizePercentage > 0.5f) {
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

                        panelWatcherSpiritVertebratesHUD.SetActive(false);
                        panelWatcherSpiritVertebratesText.SetActive(false);
                        panelWatcherSpiritVertebratesGenome.SetActive(false);
                        panelWatcherSpiritVertebratesBrain.SetActive(false);

                        textWatcherVertebratePageNum.text = "PAGE " + (curWatcherPanelVertebratePageNum + 1).ToString() + " of 4";
                        textWatcherTargetIndex.text = "#" + agent.index.ToString();
                
                        // pages:
                        if(curWatcherPanelVertebratePageNum == 0) {
                            textNewInspectLog.text = "";
                            textWatcherVertebrateHUD.text = "";

                            panelWatcherSpiritVertebratesHUD.SetActive(true);
                            
                            textVertebrateGen.text = "Gen #" + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                            textVertebrateLifestage.text = "Age: " + agent.ageCounter.ToString() + ", stateID: " + developmentStateID;
                            string statusStr = "Alive!";
                            Color statusColor = Color.white;
                            Color healthColor = Color.green;
                            if(agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                                statusStr = "Dead! (Decaying)";
                                statusColor = Color.gray;
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

                            textVertebrateStatus.text = statusStr; // "activity: " + curActivityID;

                            //where do hud elements get updated?? ***
                            string hudString = "";
                            hudString += "Species " + agent.speciesIndex.ToString();
                            hudString += "\nGeneration " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();

                            // Life Stage
                            hudString += "\n\nAge: " + agent.ageCounter.ToString();
                            hudString += "\nCur Biomass: " + agent.currentBiomass.ToString("F3");
                            hudString += "\nGrowth % " + (agent.currentBiomass / 2.5f * 100f).ToString("F0");
                            // STATE
                            hudString += "\n";// + curActivityID.ToString();
                            hudString += "\n";// + developmentStateID.ToString();
                            // BIOMETRICS:
                            hudString += "\n\nNRG: " + agent.coreModule.energy.ToString("F1");
                            hudString += "\nHP: " + ((agent.coreModule.healthBody + agent.coreModule.healthHead + agent.coreModule.healthExternal) / 3f * 100f).ToString("F0");
                            hudString += "\nFOOD: " + (agent.coreModule.stomachContentsNorm * 100f).ToString("F0");
                            hudString += "\nSTAM: " + agent.coreModule.stamina[0].ToString("F2");
                            // BASE STATS FROM GENOME:
                            hudString += "\n\nBase Size: " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2"); 
                            hudString += "\nFullsize: ( " + agent.fullSizeBoundingBox.x.ToString("F2") + ", " + agent.fullSizeBoundingBox.y.ToString("F2") + ", " + agent.fullSizeBoundingBox.z.ToString("F2") + " )\n";
                            hudString += "\n# Neurons: " + agent.brain.neuronList.Count.ToString() + ", # Axons: " + agent.brain.axonList.Count.ToString();
                            hudString += "\n# In/Out: " + agent.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count.ToString() + ", Hid: " + agent.candidateRef.candidateGenome.brainGenome.hiddenNeuronList.Count.ToString();
                    
                            // History:
                            hudString += "\nMeatEaten: " + agent.totalFoodEatenMeat.ToString("F3");
                            hudString += "\nPlantsEaten: " + agent.totalFoodEatenPlant.ToString("F3");
                    
                            string textString = "Event Log! [" + agent.index.ToString() + "]";
                            int maxEventsToDisplay = 8;
                            int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
                            int startIndex = Mathf.Max(0, agent.agentEventDataList.Count - maxEventsToDisplay);                   
                            string eventLogString = "";
                            for(int q = agent.agentEventDataList.Count - 1; q >= startIndex; q--) {
                                eventLogString += "\n[" + agent.agentEventDataList[q].eventFrame.ToString() + "] " + agent.agentEventDataList[q].eventText;
                            }
                            //textNewInspectLog.text = eventLogString;
                            //textWatcherVertebrateHUD.text = hudString;

                        }
                        else if(curWatcherPanelVertebratePageNum == 1) {
                            
                            panelWatcherSpiritVertebratesText.SetActive(true);

                            string textString = "Event Log! [" + agent.index.ToString() + "]";
                    
                            // Agent Event Log:
                            int maxEventsToDisplay = 12;
                            int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
                            int startIndex = Mathf.Max(0, agent.agentEventDataList.Count - maxEventsToDisplay);                   
                            string eventLogString = "";
                            for(int q = agent.agentEventDataList.Count - 1; q >= startIndex; q--) {
                                eventLogString += "\n[" + agent.agentEventDataList[q].eventFrame.ToString() + "] " + agent.agentEventDataList[q].eventText;
                            } 
                    
                            textString += "\n\nNumChildrenBorn: " + uiManagerRef.gameManager.simulationManager.numAgentsBorn.ToString() + ", numDied: " + uiManagerRef.gameManager.simulationManager.numAgentsDied.ToString() + ", ~Gen: " + ((float)uiManagerRef.gameManager.simulationManager.numAgentsBorn / (float)uiManagerRef.gameManager.simulationManager._NumAgents).ToString();
                            textString += "\nSimulation Age: " + uiManagerRef.gameManager.simulationManager.simAgeTimeSteps.ToString();
                            textString += "\nYear " + uiManagerRef.gameManager.simulationManager.curSimYear.ToString() + "\n\n";
                            int numActiveSpecies = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
                            textString += numActiveSpecies.ToString() + " Active Species:\n";
                            textString += eventLogString;

                            textWatcherVertebrateText.text = textString;
                            
                        }
                        else if(curWatcherPanelVertebratePageNum == 2) {
                            panelWatcherSpiritVertebratesGenome.SetActive(true);
                    
                            string genomeString = "GENOME PAGE!"; ;
                    
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
                            genomeString += "\nCID: " + agent.candidateRef.candidateID.ToString() + ", gen# " + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
                    
                            genomeString += "\nBONUSES:\nDamage: " + agent.coreModule.damageBonus.ToString("F2") + "\nSpeed: " + agent.coreModule.speedBonus.ToString("F2") + "\nHealth: " + agent.coreModule.healthBonus.ToString("F2") + "\nEnergy: " + agent.coreModule.energyBonus.ToString("F2") + "\n";
                            genomeString += "\nDIET:\nDecay: " + agent.coreModule.foodEfficiencyDecay.ToString("F2") + "\nPlant: " + agent.coreModule.foodEfficiencyPlant.ToString("F2") + "\nMeat: " + agent.coreModule.foodEfficiencyMeat.ToString("F2") + "\n";
                  
                            genomeString += "\nSENSORS:\n";
                            genomeString += "Comms= " + agent.candidateRef.candidateGenome.bodyGenome.communicationGenome.useComms.ToString() + "\n";
                            genomeString += "Enviro: " + agent.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useWaterStats.ToString() + "\n";
                            CritterModuleFoodSensorsGenome foodGenome = agent.candidateRef.candidateGenome.bodyGenome.foodGenome;
                            genomeString += "Food: " + foodGenome.useNutrients.ToString() + ", " + foodGenome.usePos.ToString() + ", " + foodGenome.useDir.ToString() + ", " + foodGenome.useStats.ToString() + ", " + foodGenome.useEggs.ToString() + ", " + foodGenome.useCorpse.ToString() + "\n";
                            genomeString += "Friend: " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.usePos.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.useDir.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.friendGenome.useVel.ToString() + "\n";
                            genomeString += "Threat: " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.usePos.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useDir.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useVel.ToString() + ", " + agent.candidateRef.candidateGenome.bodyGenome.threatGenome.useStats.ToString() + "\n";
            
                            textWatcherVertebrateGenome.text = genomeString;
                        }
                        else if(curWatcherPanelVertebratePageNum == 3) {
                            panelWatcherSpiritVertebratesBrain.SetActive(true);
                            // update brain panel
                                        
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
                        }
                    }

                    TextCommonStatsA.text = str;
                }
            }
            else if(watcherSelectedTrophicSlotRef.kingdomID == 3) {
                /*Vector4 resourceGridSample = uiManagerRef.SampleTexture(uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nNutrients    : " + (resourceGridSample.x * 1000f).ToString("F0");
                str += "\nWaste        : " + (resourceGridSample.y * 1000f).ToString("F0");
                str += "\nDecomposers  : " + (resourceGridSample.z * 1000f).ToString("F0");
                str += "\nAlgae        : " + (resourceGridSample.w * 1000f).ToString("F0");
                Vector4 simTansferSample = uiManagerRef.SampleTexture(uiManagerRef.gameManager.simulationManager.vegetationManager.resourceSimTransferRT, uiManagerRef.theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 1f;
                str += "\n\nProduced This Frame:\nWaste: " + (simTansferSample.z * 1000000f).ToString("F0") + "\n\nConsumed This Frame:\nNutrients: " + (simTansferSample.x * 1000000f).ToString("F0");

                panelWatcherSpiritTerrain.SetActive(true);*/
            }
            else {
                // minerals? water? air?
            }

            
            // string?
        }
        else {
            // No target
        }  
        
        if(watcherSelectedTrophicSlotRef == null) {

            //panelWatcherExpand.SetActive(false);
            panelWatcherExpand.SetActive(false);
            imageWatcherInactiveOverlay.gameObject.SetActive(true);
        }
    }

	public void UpdateWatcherPanelUI(TrophicLayersManager layerManager) {
        animatorWatcherUI.SetBool("_IsOpen", isOpen);

        if(uiManagerRef.panelFocus == UIManager.PanelFocus.Watcher) {
            //animatorWatcherUI.SetBool("_IsOpen", true);
            animatorWatcherUI.SetBool("_IsDim", false);
        }
        else {
            animatorWatcherUI.SetBool("_IsDim", true);

            StopFollowingAgent();
            StopFollowingPlantParticle();
            StopFollowingAnimalParticle();

        }

        panelWatcherSpiritMain.SetActive(true); // isOpen);
        if (isOpen) {
            UpdateUI(layerManager);
        }
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
            uiManagerRef.curActiveTool = UIManager.ToolType.None;

            animatorWatcherUI.SetBool("_IsOpen", true);
        }
        else {
            uiManagerRef.panelFocus = UIManager.PanelFocus.WorldHub;
            animatorWatcherUI.SetBool("_IsOpen", false);
        }
        //watcherLockedTrophicSlotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;//
    }
    public void ClickSnoopModeButton() {        
        //uiManagerRef.isBrushModeON_snoopingOFF = false;
        uiManagerRef.panelFocus = UIManager.PanelFocus.Watcher;
        uiManagerRef.curActiveTool = UIManager.ToolType.None;
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
