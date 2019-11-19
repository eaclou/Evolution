using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophicLayersManager {

    private bool decomposersOn = false;  // first pass -- temporary?
    private bool algaeOn = false;
    private bool plantsOn = false;
    private bool zooplanktonOn = false;
    private bool agentsOn = false;
    private bool terrainOn = true;

    //public bool pendingTrophicSlot = false;
    //public TrophicSlot pendingTrophicSlotRef;
    public bool isSelectedTrophicSlot = false;
    public TrophicSlot selectedTrophicSlotRef;

    public TrophicKingdom kingdomDecomposers;
    public TrophicKingdom kingdomPlants;
    public TrophicKingdom kingdomAnimals;
    public TrophicKingdom kingdomTerrain;
    public TrophicKingdom kingdomOther;

    public Vector2 decomposerOriginPos;
    public Vector2 algaeOriginPos;
    public Vector2 plantOriginPos;
    public Vector2 zooplanktonOriginPos;
    public int timeStepAlgaeOn = 0;
    public int timeStepPlantsOn = 0;
    public int timeStepDecomposersOn = 0;
    public int timeStepZooplanktonOn = 0;
    private int timeStepsLayerGrowthDuration = 1200;

	public TrophicLayersManager() {  // constructor
        decomposersOn = false;  // first pass -- temporary?
        algaeOn = false;
        plantsOn = false;
        zooplanktonOn = false;
        agentsOn = false;
        terrainOn = false;

        // DECOMPOSERS::::  // hacky manual initialization for now!!!!
        kingdomDecomposers = new TrophicKingdom();
        kingdomDecomposers.name = "Decomposers";
        TrophicTier decomposersTier0 = new TrophicTier();
        decomposersTier0.trophicSlots[0].Initialize("Decomposers", TrophicSlot.SlotStatus.Locked, 0, 0, 0);
        kingdomDecomposers.trophicTiersList.Add(decomposersTier0);

        // PLANTS::::
        kingdomPlants = new TrophicKingdom();
        kingdomPlants.name = "Plants";
        TrophicTier plantsTier0 = new TrophicTier();  // simple algae
        plantsTier0.trophicSlots[0].Initialize("Algae", TrophicSlot.SlotStatus.Locked, 1, 0, 0);        
        kingdomPlants.trophicTiersList.Add(plantsTier0);
        TrophicTier plantsTier1 = new TrophicTier();  // bigger plants
        plantsTier1.trophicSlots[0].Initialize("Floating Plants", TrophicSlot.SlotStatus.Locked, 1, 1, 0);
        plantsTier1.trophicSlots[1].Initialize("Submerged Plants", TrophicSlot.SlotStatus.Locked, 1, 1, 1);
        kingdomPlants.trophicTiersList.Add(plantsTier1);

        // ANIMALS:::::
        kingdomAnimals = new TrophicKingdom();
        kingdomAnimals.name = "Animals";
        TrophicTier animalsTier0 = new TrophicTier();  // Zooplankton
        animalsTier0.trophicSlots[0].Initialize("Zooplankton", TrophicSlot.SlotStatus.Locked, 2, 0, 0);
        kingdomAnimals.trophicTiersList.Add(animalsTier0);
        TrophicTier animalsTier1 = new TrophicTier();  // full Agents
        animalsTier1.trophicSlots[0].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 0);
        animalsTier1.trophicSlots[1].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 1);
        animalsTier1.trophicSlots[2].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 2);
        animalsTier1.trophicSlots[3].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 3);
        kingdomAnimals.trophicTiersList.Add(animalsTier1);

        // TERRAIN !!!!::::::
        kingdomTerrain = new TrophicKingdom();
        kingdomTerrain.name = "Terrain";
        TrophicTier terrainTier0 = new TrophicTier();
        terrainTier0.trophicSlots[0].Initialize("World", TrophicSlot.SlotStatus.On, 3, 0, 0);
        terrainTier0.trophicSlots[1].Initialize("Stones", TrophicSlot.SlotStatus.On, 3, 0, 1);
        terrainTier0.trophicSlots[2].Initialize("Pebbles", TrophicSlot.SlotStatus.On, 3, 0, 2);
        terrainTier0.trophicSlots[3].Initialize("Sand", TrophicSlot.SlotStatus.On, 3, 0, 3);
        kingdomTerrain.trophicTiersList.Add(terrainTier0);

        // OTHER!!!!!%%%
        kingdomOther = new TrophicKingdom();
        kingdomOther.name = "Other";
        TrophicTier otherTier0 = new TrophicTier();
        otherTier0.trophicSlots[0].Initialize("Minerals", TrophicSlot.SlotStatus.On, 4, 0, 0);
        otherTier0.trophicSlots[1].Initialize("Water", TrophicSlot.SlotStatus.On, 4, 0, 1);
        otherTier0.trophicSlots[2].Initialize("Air", TrophicSlot.SlotStatus.On, 4, 0, 2);
        kingdomOther.trophicTiersList.Add(otherTier0);
                
        selectedTrophicSlotRef = terrainTier0.trophicSlots[0];
        //selectedTrophicSlotRef.status = TrophicSlot.SlotStatus.On;
        isSelectedTrophicSlot = true;
    }
    public void CreateTrophicSlotSpecies(SimulationManager simManagerRef, Vector2 spawnPos, int timeStep) {
        
        // reset things, figure out which slot was created:
        //isSelectedTrophicSlot = false;
        selectedTrophicSlotRef.status = TrophicSlot.SlotStatus.On;

        if (selectedTrophicSlotRef.kingdomID == 0) { // decomposers:
            TurnOnDecomposers(spawnPos, timeStep);
        }
        if (selectedTrophicSlotRef.kingdomID == 1) { // plants!:
            if (selectedTrophicSlotRef.tierID == 0) { // ALGAE!:
                TurnOnAlgae(spawnPos, timeStep);
                //simManagerRef.vegetationManager.SpawnInitialAlgaeParticles(5f, new Vector4(spawnPos.x, spawnPos.y, 0f, 0f));
            }
            else {
                TurnOnPlants(spawnPos, timeStep);
            }
        }
        if (selectedTrophicSlotRef.kingdomID == 2) { // Animals:
            if (selectedTrophicSlotRef.tierID == 0) { // Animals:
                TurnOnZooplankton(spawnPos, timeStep);
                // Unlock Slots:
                //kingdomAnimals.trophicTiersList[1].unlocked = true;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Empty;
            }
            if(selectedTrophicSlotRef.tierID == 1) {
                TurnOnAgents();

                
                
            }
        }
    }

    public void Tick(SimulationManager simManager) {
        // ALGAE
        if(kingdomPlants.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simAgeTimeSteps > 20) {
                kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("ALGAE UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());

                //simManager.uiManager.AnnounceUnlockAlgae();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomPlants.trophicTiersList[0].trophicSlots[0];                
            }
        }

        if(kingdomPlants.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAlgaeReservoir > 15f) { // || simManager.simResourceManager.curGlobalDetritus > 150f) {
                kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("PLANTS UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());

                //simManager.uiManager.AnnounceUnlockDecomposers();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomPlants.trophicTiersList[1].trophicSlots[0];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }
        
        //check for unlocks:
        if(kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAlgaeReservoir > 15f) { // || simManager.simResourceManager.curGlobalDetritus > 150f) {
                kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("DECOMPOSERS UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());

                //simManager.uiManager.AnnounceUnlockDecomposers();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }
        
        if(kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalDecomposers > 10f) { // && !simManager.uiManager.isUnlockCooldown) {
                kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("ZOOPLANKTON UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                //simManager.uiManager.AnnounceUnlockZooplankton();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAnimalParticles > 1f) {     // && !simManager.uiManager.isUnlockCooldown) {
                
                kingdomAnimals.trophicTiersList[1].unlocked = true;
                kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Empty;

                Debug.Log("CREATURE 1 UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                //simManager.uiManager.AnnounceUnlockVertebrates();
                simManager.uiManager.isUnlockCooldown = true;

                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomAnimals.trophicTiersList[1].trophicSlots[0];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[1].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAgentBiomass > 0.1f) {     // && !simManager.uiManager.isUnlockCooldown) {                
                kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("CREATURE 2 UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                //simManager.uiManager.AnnounceUnlockVertebrates();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomAnimals.trophicTiersList[1].trophicSlots[1];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }
        if(kingdomAnimals.trophicTiersList[1].trophicSlots[2].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAgentBiomass > 0.4f) {     // && !simManager.uiManager.isUnlockCooldown) {                
                kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("CREATURE 3 UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                //simManager.uiManager.AnnounceUnlockVertebrates();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomAnimals.trophicTiersList[1].trophicSlots[2];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }
        if(kingdomAnimals.trophicTiersList[1].trophicSlots[3].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAgentBiomass > 8f) {     // && !simManager.uiManager.isUnlockCooldown) {                
                kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Unlocked;
                Debug.Log("CREATURE 4 UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                //simManager.uiManager.AnnounceUnlockVertebrates();
                simManager.uiManager.isUnlockCooldown = true;
                simManager.uiManager.unlockedAnnouncementSlotRef = kingdomAnimals.trophicTiersList[1].trophicSlots[3];
                //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
            }
        }
    }

    public void CheatUnlockAll() {
        // ALGAE
        if(kingdomPlants.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {            
            kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
        }
        if(kingdomPlants.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {            
            kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;
        }
        
        //check for unlocks:
        if(kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {            
                kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;  
        }
        
        if(kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {            
            kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked;                
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) { 
            kingdomAnimals.trophicTiersList[1].unlocked = true;
            kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Unlocked; 
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[1].status == TrophicSlot.SlotStatus.Locked) {                         
            kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Unlocked;                
        }
        if(kingdomAnimals.trophicTiersList[1].trophicSlots[2].status == TrophicSlot.SlotStatus.Locked) {                       
            kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Unlocked;                
        }
        if(kingdomAnimals.trophicTiersList[1].trophicSlots[3].status == TrophicSlot.SlotStatus.Locked) {                     
            kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Unlocked;                
        }
    }

    public string GetSpeciesPreviewDescriptionString(SimulationManager simManager) {
        string str = "";

        if(selectedTrophicSlotRef.kingdomID == 0) {
            str = "Bacterial and Fungal organisms that recycle vital nutrients."; //   \n\nUses: <b><color=#A97860FF>Waste</color></b>, <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#FBC653FF>Nutrients</color></b>";

            str += "\n\n";
            str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalDecomposers.ToString("F1") + "</b></size>\n\n";

            str += "<color=#FBC653FF>Nutrient Production: <b>" + simManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            str += "<color=#8EDEEEFF>Oxygen Usage: <b>" + simManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            str += "<color=#A97860FF>Waste Processed: <b>" + simManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";

        }
        else if(selectedTrophicSlotRef.kingdomID == 1) {
            if (selectedTrophicSlotRef.tierID == 0) {  // ALGAE GRID
                str = "Microscopic Plants that form the foundation of the ecosystem along with the Decomposers."; //   \n\nUses: <b><color=#FBC653FF>Nutrients</color></b>\n\nProduces: <b><color=#8EDEEEFF>Oxygen</color></b>";

                str += "\n\n";
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalAlgaeReservoir.ToString("F1") + "</b></size>\n\n";

                str += "<color=#8EDEEEFF>Oxygen Production: <b>" + simManager.simResourceManager.oxygenProducedByAlgaeReservoirLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#FBC653FF>Nutrient Usage: <b>" + simManager.simResourceManager.nutrientsUsedByAlgaeReservoirLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByAlgaeReservoirLastFrame.ToString("F3") + "</b></color>\n";

            }
            else { //BIG PLANTS
                str = "Larger Plants."; //   \n\nUses: <b><color=#FBC653FF>Nutrients</color></b>\n\nProduces: <b><color=#8EDEEEFF>Oxygen</color></b>";
                str += "\n\nWelcome to the Big Leagues, chloroplasts";
                str += "\n\n";
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalPlantParticles.ToString("F1") + "</b></size>\n\n";

                str += "<color=#8EDEEEFF>Oxygen Production: <b>" + simManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#FBC653FF>Nutrient Usage: <b>" + simManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";

            }
            
        }
        else if(selectedTrophicSlotRef.kingdomID == 2) {
            if(selectedTrophicSlotRef.tierID == 0) {
                str = "Tiny Animals that feed on Algae."; //   \n\nUses: <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#A97860FF>Waste</color></b>";
                str += "\n\n";
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalAnimalParticles.ToString("F1") + "</b></size>\n\n";
                str += "<color=#8EDEEEFF>Oxygen Usage: <b>" + simManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";                
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";

            }
            else {
                str = "Simple Animal that feeds on Plants and Zooplankton.";  //    \n\nUses: <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#A97860FF>Waste</color></b>";
                str += "\n\n";
                float speciesMass = 0f;
                if (selectedTrophicSlotRef.slotID == 0) {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass0;
                }
                else if(selectedTrophicSlotRef.slotID == 1) {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass1;
                }
                else if(selectedTrophicSlotRef.slotID == 2) {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass2;
                }
                else {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass3;
                }
                str += "<size=13><b>Total Biomass: " + speciesMass.ToString("F1") + "</b></size>\n\n";// simManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b></size>\n\n";

                SpeciesGenomePool GenomePool = simManager.masterGenomePool.completeSpeciesPoolsList[selectedTrophicSlotRef.linkedSpeciesID];
                str += "<color=#8EDEEEFF>Oxygen Usage: <b>" + simManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                //str += "<color=#8BD06AFF>Avg Food Eaten: <b>" + (GenomePool.avgConsumptionPlant + GenomePool.avgConsumptionMeat).ToString("F3") + "</b></color>\n";
                //str += "\n\n\nAvg Lifespan: <b>" + (GenomePool.avgLifespan / 1500f).ToString("F1") + " Years</b>\n\n";

                GenomePool.representativeGenome.bodyGenome.CalculateFullsizeBoundingBox();
                str += "Avg Body Size: <b>" + ((GenomePool.representativeGenome.bodyGenome.fullsizeBoundingBox.x + GenomePool.representativeGenome.bodyGenome.fullsizeBoundingBox.y) * 0.5f * GenomePool.representativeGenome.bodyGenome.fullsizeBoundingBox.z).ToString("F2") + "</b>\n";
                str += "Avg Brain Size: <b>" + ((GenomePool.avgNumNeurons + GenomePool.avgNumAxons) * 1f).ToString("F0") + "</b>\n";
                
                str += "\nFOOD EATEN:\nPlants: <b>" + ((GenomePool.avgConsumptionPlant) * 1f).ToString("F3") + "</b> [" + (GenomePool.avgFoodSpecPlant).ToString() + "]\n";
                str += "Meat: <b>" + ((GenomePool.avgConsumptionMeat) * 1f).ToString("F3") + "</b> [" + (GenomePool.avgFoodSpecMeat).ToString() + "]\n";

                str += "\nSPECIALIZATIONS:\nAttack: <b>" + ((GenomePool.avgSpecAttack) * 1f).ToString("F2") + "</b>\n";
                str += "Defend: <b>" + ((GenomePool.avgSpecDefend) * 1f).ToString("F2") + "</b>\n";
                str += "Speed: <b>" + ((GenomePool.avgSpecSpeed) * 1f).ToString("F2") + "</b>\n";
                str += "Utility: <b>" + ((GenomePool.avgSpecUtility) * 1f).ToString("F2") + "</b>\n";
            }            
        }
        else {
            if(selectedTrophicSlotRef.slotID == 0) {
                str = "World";
            }
            else if(selectedTrophicSlotRef.slotID == 1) {
                str = "Stone";
            }
            else if(selectedTrophicSlotRef.slotID == 2) {
                str = "Pebbles";
            }
            else {
                str = "Fine Sand";
            }
        }


        return str;
    }
    
    /*public void ResetSelectedAgentSlots() {
        for(int i = 0; i < 4; i++) {  // if others were selected: revert to on:
            if (kingdomAnimals.trophicTiersList[1].trophicSlots[i].status == TrophicSlot.SlotStatus.Selected) {
                kingdomAnimals.trophicTiersList[1].trophicSlots[i].status = TrophicSlot.SlotStatus.On;
            }
        }
    }*/

    /*public void PendingDecomposers() {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
        
        //pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
        // waiting on click!
    }
    public void PendingAlgae() {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomPlants.trophicTiersList[0].trophicSlots[0];
        //pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
    }
    public void PendingZooplankton() {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomAnimals.trophicTiersList[0].trophicSlots[0];
        //pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
    }
    public void PendingAgents(int index) {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        //pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
    }*/
    public void TurnOnDecomposers(Vector2 spawnPos, int timeStep) {
        decomposerOriginPos = spawnPos;
        timeStepDecomposersOn = timeStep;
        decomposersOn = true;
    }
    public void TurnOnAlgae(Vector2 spawnPos, int timeStep) {
        algaeOriginPos = spawnPos;
        timeStepAlgaeOn = timeStep;
        algaeOn = true;
    }
    public void TurnOnPlants(Vector2 spawnPos, int timeStep) {
        plantOriginPos = spawnPos;
        timeStepPlantsOn = timeStep;
        plantsOn = true;
        Debug.Log("PLANTS ON!");
    }
    public void TurnOnZooplankton(Vector2 spawnPos, int timeStep) {
        zooplanktonOriginPos = spawnPos;
        timeStepZooplanktonOn = timeStep;
        zooplanktonOn = true;
    }
    public void TurnOnAgents() {
        agentsOn = true;
    }

    public void TurnOffDecomposers() {
        decomposersOn = false;
    }
    public void TurnOffAlgae() {
        algaeOn = false;
    }
    public void TurnOffPlants() {
        plantsOn = false;
    }
    public void TurnOffZooplankton() {
        zooplanktonOn = false;        
    }
    public void TurnOffAgents() {
        agentsOn = false;
    }

    public float GetDecomposersOnLerp(int curTimeStep) {
        float lerp = 0f;
        if(decomposersOn) {
            lerp = Mathf.Clamp01((float)(curTimeStep - timeStepDecomposersOn) / (float)timeStepsLayerGrowthDuration);
        }
        return lerp;
    }
    public float GetAlgaeOnLerp(int curTimeStep) {
        float lerp = 0f;
        if(algaeOn) {
            lerp = Mathf.Clamp01((float)(curTimeStep - timeStepAlgaeOn) / (float)timeStepsLayerGrowthDuration);
        }
        return lerp;
    }
    public float GetPlantsOnLerp(int curTimeStep) {
        float lerp = 0f;
        if(plantsOn) {
            lerp = Mathf.Clamp01((float)(curTimeStep - timeStepPlantsOn) / (float)timeStepsLayerGrowthDuration);
        }
        return lerp;
    }
    public float GetZooplanktonOnLerp(int curTimeStep) {
        float lerp = 0f;
        if(zooplanktonOn) {
            lerp = Mathf.Clamp01((float)(curTimeStep - timeStepZooplanktonOn) / (float)timeStepsLayerGrowthDuration);
        }
        return lerp;
    }

    public bool GetDecomposersOnOff() {
        return decomposersOn;
    }

    public bool GetAlgaeOnOff() {
        return algaeOn;
    }

    public bool GetPlantsOnOff() {
        return plantsOn;
    }

    public bool GetZooplanktonOnOff() {
        return zooplanktonOn;
    }

    public bool GetAgentsOnOff() {
        return agentsOn;
    }
}
