using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnowledgeUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isOpen;
    public Image imageKnowledgeButtonMIP;  // superscript button over watcher toolbar Button
    public Image imageKnowledgeCurTarget; // in watcher panel
    public Text textTargetLayer;
    public Button buttonKnowledgeLock;

    
    public Image knowledgeGraphVertebrateLifespan;
    public Image knowledgeGraphVertebratePopulation;
    public Image knowledgeGraphVertebrateFoodEaten;
    public Image knowledgeGraphVertebrateGenome;
    public Text textKnowledgeGraphLifespan;
    public Text textKnowledgeGraphPopulation;
    public Text textKnowledgeGraphFoodEaten;
    public Text textKnowledgeGraphGenome;
    public Material knowledgeGraphVertebrateLifespanMat0;
    public Material knowledgeGraphVertebratePopulationMat0;
    public Material knowledgeGraphVertebrateFoodEatenMat0;
    public Material knowledgeGraphVertebrateGenomeMat0;
    public Material knowledgeGraphVertebrateLifespanMat1;
    public Material knowledgeGraphVertebratePopulationMat1;
    public Material knowledgeGraphVertebrateFoodEatenMat1;
    public Material knowledgeGraphVertebrateGenomeMat1;
    public Material knowledgeGraphVertebrateLifespanMat2;
    public Material knowledgeGraphVertebratePopulationMat2;
    public Material knowledgeGraphVertebrateFoodEatenMat2;
    public Material knowledgeGraphVertebrateGenomeMat2;
    public Material knowledgeGraphVertebrateLifespanMat3;
    public Material knowledgeGraphVertebratePopulationMat3;
    public Material knowledgeGraphVertebrateFoodEatenMat3;
    public Material knowledgeGraphVertebrateGenomeMat3;

    public GameObject panelKnowledgeSpiritBase;
    public GameObject panelKnowledgeInfoWorld;
    public GameObject panelKnowledgeInfoDecomposers;
    public GameObject panelKnowledgeInfoAlgae;
    public GameObject panelKnowledgeInfoPlants;
    public GameObject panelKnowledgeInfoZooplankton;
    public GameObject panelKnowledgeInfoVertebrates;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateKnowledgePanelUI(TrophicLayersManager layerManager) {
        TrophicSlot slotRef = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
        if (slotRef != null) {
            if (isKnowledgeTargetLayerLocked) {
                knowledgeUI.buttonKnowledgeLock.GetComponent<Image>().color = knowledgeLockedTrophicSlotRef.color;
            }
            else {
                knowledgeLockedTrophicSlotRef = slotRef;
                textKnowledgePanelTargetLayer.text = isKnowledgeTargetLayerLocked.ToString() + ", " + knowledgeLockedTrophicSlotRef.kingdomID.ToString();
            }
        }

        if(isKnowledgeTargetLayerLocked) {
            knowledgeUI.buttonKnowledgeLock.GetComponentInChildren<Text>().text = "Locked!";
        }
        else {
            knowledgeUI.buttonKnowledgeLock.GetComponentInChildren<Text>().text = "Unlocked!";
            knowledgeUI.buttonKnowledgeLock.GetComponent<Image>().color = Color.white;
        }

        panelKnowledgeInfoWorld.SetActive(false);
        panelKnowledgeInfoDecomposers.SetActive(false);
        panelKnowledgeInfoAlgae.SetActive(false);
        panelKnowledgeInfoPlants.SetActive(false);
        panelKnowledgeInfoZooplankton.SetActive(false);
        panelKnowledgeInfoVertebrates.SetActive(false);

        textCurYear.text = (gameManager.simulationManager.curSimYear + 1).ToString();
        
        textGlobalMass.text = "Global Biomass: " + gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = gameManager.simulationManager.simResourceManager;
        textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterAlgae.text = (resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles).ToString("F0");
        textMeterZooplankton.text = (resourcesRef.curGlobalAnimalParticles).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass).ToString("F2");    
                
        textToolbarWingSpeciesSummary.gameObject.SetActive(true);
        string summaryText = layerManager.GetSpeciesPreviewDescriptionString(gameManager.simulationManager);
        textToolbarWingSpeciesSummary.text = summaryText;

        if (knowledgeLockedTrophicSlotRef.kingdomID == 0) {
            // DECOMPOSERS
            panelKnowledgeInfoDecomposers.SetActive(true);
        }
        else if(knowledgeLockedTrophicSlotRef.kingdomID == 1) {
            if(knowledgeLockedTrophicSlotRef.tierID == 0) {  // ALGAE
                panelKnowledgeInfoAlgae.SetActive(true);
            }
            else {  // PLANT PARTICLES
                panelKnowledgeInfoPlants.SetActive(true);     
            }
                    
        }
        else if(knowledgeLockedTrophicSlotRef.kingdomID == 2) {
            if(knowledgeLockedTrophicSlotRef.tierID == 0) { // ZOOPLANKTON
                panelKnowledgeInfoZooplankton.SetActive(true);
            }
            else {  // VERTEBRATES
                panelKnowledgeInfoVertebrates.SetActive(true);
                

                float lifespan = 0f;
                float population = 0f;
                float foodEaten = 0f;
                float genome = 0f;

                if(knowledgeLockedTrophicSlotRef.slotID == 0) {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan0.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation0.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten0.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome0.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat0;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat0;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat0;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat0;
                } 
                else if(knowledgeLockedTrophicSlotRef.slotID == 1) {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan1.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation1.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten1.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome1.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat1;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat1;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat1;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat1;
                } 
                else if(knowledgeLockedTrophicSlotRef.slotID == 2) {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan2.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation2.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten2.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome2.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat2;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat2;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat2;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat2;
                } 
                else {
                    lifespan = gameManager.simulationManager.graphDataVertebrateLifespan3.curVal;
                    population = gameManager.simulationManager.graphDataVertebratePopulation3.curVal;
                    foodEaten = gameManager.simulationManager.graphDataVertebrateFoodEaten3.curVal;
                    genome = gameManager.simulationManager.graphDataVertebrateGenome3.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat3;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat3;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat3;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat3;
                }

                textKnowledgeGraphLifespan.text = lifespan.ToString("F0");
                textKnowledgeGraphPopulation.text = population.ToString("F0");
                textKnowledgeGraphFoodEaten.text = foodEaten.ToString("F3");
                textKnowledgeGraphGenome.text = genome.ToString("F1");  // avg Body size
            }
                    
        }
        else if(knowledgeLockedTrophicSlotRef.kingdomID == 3) {
            panelKnowledgeInfoWorld.SetActive(true);
            if (knowledgeLockedTrophicSlotRef.slotID == 0) {
                // WORLD    
            } 
            else if(knowledgeLockedTrophicSlotRef.slotID == 1) {
                // STONE
            } 
            else if(knowledgeLockedTrophicSlotRef.slotID == 2) {
                // PEBBLES
            } 
            else {
                // SAND
            }                    
        }
        else if(knowledgeLockedTrophicSlotRef.kingdomID == 4) {
            panelKnowledgeInfoWorld.SetActive(true);
            if(knowledgeLockedTrophicSlotRef.slotID == 0) {
                // Minerals
            } 
            else if(knowledgeLockedTrophicSlotRef.slotID == 1) {
                // Water
            } 
            else if(knowledgeLockedTrophicSlotRef.slotID == 2) {
                // AIR
            }      
        }
    }

    

        TrophicSlot slotRefKnowledge = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
        if(slotRefKnowledge != null) {
            if(isKnowledgeTargetLayerLocked) {
                slotRefKnowledge = knowledgeLockedTrophicSlotRef;
            }            
        }
        if(slotRefKnowledge != null) {
            knowledgeUI.imageKnowledgeButtonMIP.sprite = slotRefKnowledge.icon; // isWatcherTargetLayerLocked;
            knowledgeUI.imageKnowledgeButtonMIP.color = slotRefKnowledge.color;
            knowledgeUI.imageKnowledgeCurTarget.sprite = slotRefKnowledge.icon;
            knowledgeUI.imageKnowledgeCurTarget.color = slotRefKnowledge.color;
            knowledgeUI.textTargetLayer.text = slotRefKnowledge.speciesName;
            knowledgeUI.textTargetLayer.color = slotRefKnowledge.color;
        }
        else {
            
        }
        
}


    private void UpdateToolbarWingStatsPanel() {
        imageToolbarSpeciesStatsGraph.gameObject.SetActive(false);

        TrophicSlot slot = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;

        string descriptionText = "";
        
        if(slot.kingdomID == 0) {
            descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalDecomposers.ToString("F1") + "</b></size>\n\n";

            descriptionText += "<color=#FBC653FF>Nutrient Production: <b>" + gameManager.simulationManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Processed: <b>" + gameManager.simulationManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";

            if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
                textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>10</i></b> Total Biomass";
                float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalDecomposers / 10f);
                textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                //imageUnlockMeter;
                matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                imageUnlockMeter.gameObject.SetActive(true);
            }
            else {
                textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                imageUnlockMeter.gameObject.SetActive(false);
            }
        }
        else if(slot.kingdomID == 1) {
            descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalPlantParticles.ToString("F1") + "</b></size>\n\n";

            descriptionText += "<color=#8EDEEEFF>Oxygen Production: <b>" + gameManager.simulationManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";

            // *************** GROSS CODE ALERT!!!!   temp hack!!!! *****************
            if(gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
                textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>150</i></b> Total Biomass";
                float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalPlantParticles / 150f);
                textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                //imageUnlockMeter;
                matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                imageUnlockMeter.gameObject.SetActive(true);
            }
            else {
                textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                imageUnlockMeter.gameObject.SetActive(false);
            }
        }
        else {
            if(slot.tierID == 0) {  // ZOOPLANKTON
                descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles.ToString("F1") + "</b></size>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";
                //descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";

                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>6</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles / 6f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    imageUnlockMeter.gameObject.SetActive(false);
                }
            }
            else {  // AGENTS
                imageToolbarSpeciesStatsGraph.gameObject.SetActive(true);

                SpeciesGenomePool selectedPool = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesID];
                
                descriptionText += "<size=13><b>Total Biomass: " + gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b></size>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + gameManager.simulationManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + gameManager.simulationManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString("F3") + "</b></color>\n";

                descriptionText += "<color=#8BD06AFF>Avg Food Consumed: <b>" + (selectedPool.avgConsumptionPlant + selectedPool.avgConsumptionMeat).ToString("F3") + "</b></color>\n";
                //descriptionText += "Avg Meat Consumed: <b>" + selectedPool.avgConsumptionMeat.ToString("F3") + "</b>\n\n";  
                //descriptionText += "Descended From Species: <b>" + selectedPool.parentSpeciesID.ToString() + "</b>\n";
                //descriptionText += "Year Evolved: <b>" + selectedPool.yearCreated.ToString() + "</b>\n\n";
                descriptionText += "\n\n\nAvg Lifespan: <b>" + (selectedPool.avgLifespan / 1500f).ToString("F1") + " Years</b>\n\n";

                selectedPool.representativeGenome.bodyGenome.CalculateFullsizeBoundingBox();
                descriptionText += "Avg Body Size: <b>" + ((selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.x + selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.y) * 0.5f * selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.z).ToString("F2") + "</b>\n";
                                                        //+ selectedPool.avgBodySize.ToString("F2") + "</b>\n";
                descriptionText += "Avg Brain Size: <b>" + ((selectedPool.avgNumNeurons + selectedPool.avgNumAxons) * 0.1f).ToString("F1") + "</b>\n";
                //descriptionText += "Avg Axon Count: <b>" + selectedPool.avgNumAxons.ToString("F0") + "</b>\n\n";
        
                toolbarSpeciesStatsGraphMat.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[0]); // statsTreeOfLifeSpeciesTexArray[0]);
                toolbarSpeciesStatsGraphMat.SetTexture("_ColorKeyTex", statsSpeciesColorKey); // statsTreeOfLifeSpeciesTexArray[0]);
                toolbarSpeciesStatsGraphMat.SetFloat("_MinValue", 0f);
                toolbarSpeciesStatsGraphMat.SetFloat("_MaxValue", maxValuesStatArray[0]);
                toolbarSpeciesStatsGraphMat.SetFloat("_NumEntries", (float)statsTreeOfLifeSpeciesTexArray[0].width);
                toolbarSpeciesStatsGraphMat.SetFloat("_SelectedSpeciesID", slot.slotID);

                // TEMP UNLOCK TEXT:
                // Slot 4/4:
                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>8</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass / 8f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    imageUnlockMeter.gameObject.SetActive(false);
                }
                // Slot 3/4:
                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>4</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass / 4f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    //textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    //textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    //imageUnlockMeter.gameObject.SetActive(false);
                }
                // Slot 2/4:
                if(gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status == TrophicSlot.SlotStatus.Locked) {
                    textToolbarWingStatsUnlockStatus.text = "<b>Next Unlock:</b>\nReach <b><i>1</i></b> Total Biomass";
                    float unlockProgressLerp = Mathf.Clamp01(gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass / 1f);
                    textToolbarWingStatsUnlockPercentage.text = (unlockProgressLerp * 100f).ToString("F0") + "%";
                    //imageUnlockMeter;
                    matUnlockMeter.SetFloat("_FillPercentage", unlockProgressLerp);

                    textToolbarWingStatsUnlockStatus.gameObject.SetActive(true);
                    textToolbarWingStatsUnlockPercentage.gameObject.SetActive(true);
                    imageUnlockMeter.gameObject.SetActive(true);
                }
                else {
                    //textToolbarWingStatsUnlockStatus.gameObject.SetActive(false);
                    //textToolbarWingStatsUnlockPercentage.gameObject.SetActive(false);
                    //imageUnlockMeter.gameObject.SetActive(false);
                }
            }
        }             
        
        textSelectedSpeciesDescription.text = descriptionText;
    }

