using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophicLayersManager {

    private bool decomposersOn = false;  // first pass -- temporary?
    private bool algaeOn = false;
    private bool zooplanktonOn = false;
    private bool agentsOn = false;

    //public bool pendingTrophicSlot = false;
    //public TrophicSlot pendingTrophicSlotRef;
    public bool isSelectedTrophicSlot = false;
    public TrophicSlot selectedTrophicSlotRef;

    public TrophicKingdom kingdomDecomposers;
    public TrophicKingdom kingdomPlants;
    public TrophicKingdom kingdomAnimals;

    public Vector2 decomposerOriginPos;
    public Vector2 algaeOriginPos;
    public Vector2 zooplanktonOriginPos;
    public int timeStepAlgaeOn = 0;
    public int timeStepDecomposersOn = 0;
    public int timeStepZooplanktonOn = 0;
    private int timeStepsLayerGrowthDuration = 1200;

	public TrophicLayersManager() {  // constructor
        decomposersOn = false;  // first pass -- temporary?
        algaeOn = false;
        zooplanktonOn = false;
        agentsOn = false;

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
        plantsTier0.trophicSlots[0].Initialize("Algae", TrophicSlot.SlotStatus.Empty, 1, 0, 0);        
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

    }
    public void CreateTrophicSlotSpecies(SimulationManager simManagerRef, Vector2 spawnPos, int timeStep) {
        
        // reset things, figure out which slot was created:
        //isSelectedTrophicSlot = false;
        selectedTrophicSlotRef.status = TrophicSlot.SlotStatus.On;

        if (selectedTrophicSlotRef.kingdomID == 0) { // decomposers:
            TurnOnDecomposers(spawnPos, timeStep);
        }
        if (selectedTrophicSlotRef.kingdomID == 1) { // plants!:
            if (selectedTrophicSlotRef.tierID == 0) { // plants!:
                TurnOnAlgae(spawnPos, timeStep);
                simManagerRef.vegetationManager.SpawnInitialAlgaeParticles(5f, new Vector4(spawnPos.x, spawnPos.y, 0f, 0f));
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
        //check for unlocks:
        if(kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAlgaeParticles > 200f || simManager.simResourceManager.curGlobalDetritus > 150f) {
                kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;
                Debug.Log("DECOMPOSERS UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());

                simManager.uiManager.AnnounceUnlockDecomposers();
                simManager.uiManager.isUnlockCooldown = true;
            }
        }
        
        if(kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalOxygen > 250f && simManager.simResourceManager.curGlobalDecomposers > 25f && !simManager.uiManager.isUnlockCooldown) {
                kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;
                Debug.Log("ZOOPLANKTON UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                simManager.uiManager.AnnounceUnlockZooplankton();
                simManager.uiManager.isUnlockCooldown = true;
            }
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[0].status == TrophicSlot.SlotStatus.Locked) {
            if(simManager.simResourceManager.curGlobalAnimalParticles > 10f && !simManager.uiManager.isUnlockCooldown) {
                kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;

                kingdomAnimals.trophicTiersList[1].unlocked = true;
                kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;
                kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Empty;
                kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Empty;
                kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Empty;

                Debug.Log("CREATURES UNLOCKED!!! " + simManager.uiManager.unlockCooldownCounter.ToString());
                simManager.uiManager.AnnounceUnlockVertebrates();
                simManager.uiManager.isUnlockCooldown = true;
            }
        }
    }

    public string GetSpeciesPreviewDescriptionString() {
        string str = "";

        if(selectedTrophicSlotRef.kingdomID == 0) {
            str = "Bacterial and Fungal organisms that recycle vital nutrients.\n\nUses: <b><color=#A97860FF>Waste</color></b>, <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#FBC653FF>Nutrients</color></b>";
        }
        else if(selectedTrophicSlotRef.kingdomID == 1) {
            str = "Tiny Plants that form the foundation of the ecosystem.\n\nUses: <b><color=#FBC653FF>Nutrients</color></b>\n\nProduces: <b><color=#8EDEEEFF>Oxygen</color></b>";
        }
        else {
            if(selectedTrophicSlotRef.tierID == 0) {
                str = "Tiny Animals that feed on Algae.\n\nUses: <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#A97860FF>Waste</color></b>";
            }
            else {
                str = "Simple Animal that feeds on Algae and Zooplankton.\n\nUses: <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#A97860FF>Waste</color></b>";
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
    public float GetZooplanktonOnLerp(int curTimeStep) {
        float lerp = 0f;
        if(algaeOn) {
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

    public bool GetZooplanktonOnOff() {
        return zooplanktonOn;
    }

    public bool GetAgentsOnOff() {
        return agentsOn;
    }
}
