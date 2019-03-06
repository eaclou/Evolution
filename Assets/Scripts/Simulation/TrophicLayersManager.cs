using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophicLayersManager {

    private bool decomposersOn = false;  // first pass -- temporary?
    private bool algaeOn = false;
    private bool zooplanktonOn = false;
    private bool agentsOn = false;

    public bool pendingTrophicSlot = false;
    public TrophicSlot pendingTrophicSlotRef;
    public bool selectedTrophicSlot = false;
    public TrophicSlot selectedTrophicSlotRef;

    public TrophicKingdom kingdomDecomposers;
    public TrophicKingdom kingdomPlants;
    public TrophicKingdom kingdomAnimals;

	public TrophicLayersManager() {  // constructor
        decomposersOn = false;  // first pass -- temporary?
        algaeOn = false;
        zooplanktonOn = false;
        agentsOn = false;

        // DECOMPOSERS::::  // hacky manual initialization for now!!!!
        kingdomDecomposers = new TrophicKingdom();
        kingdomDecomposers.name = "Decomposers";
        TrophicTier decomposersTier0 = new TrophicTier();
        decomposersTier0.trophicSlots[0].Initialize(TrophicSlot.SlotStatus.Locked, 0, 0, 0);
        kingdomDecomposers.trophicTiersList.Add(decomposersTier0);

        // PLANTS::::
        kingdomPlants = new TrophicKingdom();
        kingdomPlants.name = "Plants";
        TrophicTier plantsTier0 = new TrophicTier();  // simple algae
        plantsTier0.trophicSlots[0].Initialize(TrophicSlot.SlotStatus.Locked, 1, 0, 0);
        kingdomPlants.trophicTiersList.Add(plantsTier0);
        TrophicTier plantsTier1 = new TrophicTier();  // bigger plants
        plantsTier1.trophicSlots[0].Initialize(TrophicSlot.SlotStatus.Locked, 1, 1, 0);
        plantsTier1.trophicSlots[1].Initialize(TrophicSlot.SlotStatus.Locked, 1, 1, 1);
        kingdomPlants.trophicTiersList.Add(plantsTier1);

        // ANIMALS:::::
        kingdomAnimals = new TrophicKingdom();
        kingdomAnimals.name = "Animals";
        TrophicTier animalsTier0 = new TrophicTier();  // Zooplankton
        animalsTier0.trophicSlots[0].Initialize(TrophicSlot.SlotStatus.Locked, 2, 0, 0);
        kingdomAnimals.trophicTiersList.Add(animalsTier0);
        TrophicTier animalsTier1 = new TrophicTier();  // full Agents
        animalsTier1.trophicSlots[0].Initialize(TrophicSlot.SlotStatus.Locked, 2, 1, 0);
        animalsTier1.trophicSlots[1].Initialize(TrophicSlot.SlotStatus.Locked, 2, 1, 1);
        animalsTier1.trophicSlots[2].Initialize(TrophicSlot.SlotStatus.Locked, 2, 1, 2);
        animalsTier1.trophicSlots[3].Initialize(TrophicSlot.SlotStatus.Locked, 2, 1, 3);
        kingdomAnimals.trophicTiersList.Add(animalsTier1);

    }
    public void ClickedPendingTrophicSlot() {
        // reset things, figure out which slot was created:
        pendingTrophicSlot = false;
        pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.On;

        if (pendingTrophicSlotRef.kingdomID == 0) { // decomposers:
            TurnOnDecomposers();
        }
        if (pendingTrophicSlotRef.kingdomID == 1) { // plants!:
            if (pendingTrophicSlotRef.tierID == 0) { // plants!:
                TurnOnAlgae();
            }
        }
        if (pendingTrophicSlotRef.kingdomID == 2) { // Animals:
            if (pendingTrophicSlotRef.tierID == 0) { // Animals:
                TurnOnZooplankton();
                kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;
                kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Empty;
                kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Empty;
                kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Empty;
            }
            if(pendingTrophicSlotRef.tierID == 1) {
                TurnOnAgents();

                if(pendingTrophicSlotRef.slotID < 3) {
                    //kingdomAnimals.trophicTiersList[1].trophicSlots[pendingTrophicSlotRef.slotID + 1].status = TrophicSlot.SlotStatus.Empty;
                    //SetToolbarButtonStateUI(ref pendingTrophicSlotRef, TrophicSlot.SlotStatus.On);
                }

                //if(pendingTrophicSlotRef.slotID == 0) {
                //    pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.On;
                //    //SetToolbarButtonStateUI(ref pendingTrophicSlotRef, TrophicSlot.SlotStatus.On);
                //}
            }
        }
    }

    public void ClickedSelectedTrophicSlot() {
        // reset things, figure out which slot was created:
        selectedTrophicSlot = false;
        selectedTrophicSlotRef.status = TrophicSlot.SlotStatus.On;  // Deselects
        
        if (pendingTrophicSlotRef.kingdomID == 2) { // Animals:
            
            if(pendingTrophicSlotRef.tierID == 1) {
                
                // Add eggsack of this species!  ************************************
                
            }
        }
    }

    public void ResetSelectedAgentSlots() {
        for(int i = 0; i < 4; i++) {  // if others were selected: revert to on:
            if (kingdomAnimals.trophicTiersList[1].trophicSlots[i].status == TrophicSlot.SlotStatus.Selected) {
                kingdomAnimals.trophicTiersList[1].trophicSlots[i].status = TrophicSlot.SlotStatus.On;
            }
        }
    }

    public void PendingDecomposers() {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
        pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
        // waiting on click!
    }
    public void PendingAlgae() {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomPlants.trophicTiersList[0].trophicSlots[0];
        pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
    }
    public void PendingZooplankton() {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomAnimals.trophicTiersList[0].trophicSlots[0];
        pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
    }
    public void PendingAgents(int index) {
        pendingTrophicSlot = true;
        pendingTrophicSlotRef = kingdomAnimals.trophicTiersList[1].trophicSlots[index];
        pendingTrophicSlotRef.status = TrophicSlot.SlotStatus.Pending;
    }
    public void TurnOnDecomposers() {
        decomposersOn = true;
    }
    public void TurnOnAlgae() {
        algaeOn = true;
    }
    public void TurnOnZooplankton() {
        zooplanktonOn = true;
    }
    public void TurnOnAgents() {
        agentsOn = true;
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
