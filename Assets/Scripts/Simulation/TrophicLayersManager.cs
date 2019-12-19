﻿using System.Collections;
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

	public TrophicLayersManager(UIManager uiManagerRef) {  // constructor
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
        decomposersTier0.trophicSlots[0].Initialize("Decomposers", TrophicSlot.SlotStatus.Locked, 0, 0, 0, uiManagerRef.spriteSpiritDecomposerIcon, uiManagerRef.colorDecomposersLayer);
        kingdomDecomposers.trophicTiersList.Add(decomposersTier0);

        // PLANTS::::
        kingdomPlants = new TrophicKingdom();
        kingdomPlants.name = "Plants";
        TrophicTier plantsTier0 = new TrophicTier();  // simple algae
        plantsTier0.trophicSlots[0].Initialize("Algae", TrophicSlot.SlotStatus.Locked, 1, 0, 0, uiManagerRef.spriteSpiritAlgaeIcon, uiManagerRef.colorAlgaeLayer);        
        kingdomPlants.trophicTiersList.Add(plantsTier0);
        TrophicTier plantsTier1 = new TrophicTier();  // bigger plants
        plantsTier1.trophicSlots[0].Initialize("Floating Plants", TrophicSlot.SlotStatus.Locked, 1, 1, 0, uiManagerRef.spriteSpiritPlantIcon, uiManagerRef.colorPlantsLayer);
        plantsTier1.trophicSlots[1].Initialize("Submerged Plants", TrophicSlot.SlotStatus.Locked, 1, 1, 1, uiManagerRef.spriteSpiritPlantIcon, uiManagerRef.colorPlantsLayer);
        kingdomPlants.trophicTiersList.Add(plantsTier1);

        // ANIMALS:::::
        kingdomAnimals = new TrophicKingdom();
        kingdomAnimals.name = "Animals";
        TrophicTier animalsTier0 = new TrophicTier();  // Zooplankton
        animalsTier0.trophicSlots[0].Initialize("Zooplankton", TrophicSlot.SlotStatus.Locked, 2, 0, 0, uiManagerRef.spriteSpiritZooplanktonIcon, uiManagerRef.colorZooplanktonLayer);
        kingdomAnimals.trophicTiersList.Add(animalsTier0);
        TrophicTier animalsTier1 = new TrophicTier();  // full Agents
        animalsTier1.trophicSlots[0].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 0, uiManagerRef.spriteSpiritVertebrateIcon, uiManagerRef.colorVertebratesLayer);
        animalsTier1.trophicSlots[1].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 1, uiManagerRef.spriteSpiritVertebrateIcon, uiManagerRef.colorVertebratesLayer);
        animalsTier1.trophicSlots[2].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 2, uiManagerRef.spriteSpiritVertebrateIcon, uiManagerRef.colorVertebratesLayer);
        animalsTier1.trophicSlots[3].Initialize("Vertebrate", TrophicSlot.SlotStatus.Locked, 2, 1, 3, uiManagerRef.spriteSpiritVertebrateIcon, uiManagerRef.colorVertebratesLayer);
        kingdomAnimals.trophicTiersList.Add(animalsTier1);

        // TERRAIN !!!!::::::
        kingdomTerrain = new TrophicKingdom();
        kingdomTerrain.name = "Terrain";
        TrophicTier terrainTier0 = new TrophicTier();
        terrainTier0.trophicSlots[0].Initialize("World", TrophicSlot.SlotStatus.On, 3, 0, 0, uiManagerRef.spriteSpiritWorldIcon, uiManagerRef.colorWorldLayer);
        terrainTier0.trophicSlots[1].Initialize("Stones", TrophicSlot.SlotStatus.On, 3, 0, 1, uiManagerRef.spriteSpiritStoneIcon, uiManagerRef.colorTerrainLayer);
        terrainTier0.trophicSlots[2].Initialize("Pebbles", TrophicSlot.SlotStatus.On, 3, 0, 2, uiManagerRef.spriteSpiritPebblesIcon, uiManagerRef.colorTerrainLayer);
        terrainTier0.trophicSlots[3].Initialize("Sand", TrophicSlot.SlotStatus.On, 3, 0, 3, uiManagerRef.spriteSpiritSandIcon, uiManagerRef.colorTerrainLayer);
        kingdomTerrain.trophicTiersList.Add(terrainTier0);

        // OTHER!!!!!%%%
        kingdomOther = new TrophicKingdom();
        kingdomOther.name = "Other";
        TrophicTier otherTier0 = new TrophicTier();
        otherTier0.trophicSlots[0].Initialize("Minerals", TrophicSlot.SlotStatus.On, 4, 0, 0, uiManagerRef.spriteSpiritMineralsIcon, uiManagerRef.colorMineralLayer);
        otherTier0.trophicSlots[1].Initialize("Water", TrophicSlot.SlotStatus.On, 4, 0, 1, uiManagerRef.spriteSpiritWaterIcon, uiManagerRef.colorWaterLayer);
        otherTier0.trophicSlots[2].Initialize("Air", TrophicSlot.SlotStatus.On, 4, 0, 2, uiManagerRef.spriteSpiritAirIcon, uiManagerRef.colorAirLayer);
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
