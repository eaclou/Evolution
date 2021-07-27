using System.Collections.Generic;
using UnityEngine;

public class TrophicLayersManager {
    UIManager uiManager => UIManager.instance;

    private bool decomposersOn = false;  // first pass -- temporary?
    private bool algaeOn = false;
    private bool plantsOn = false;
    private bool zooplanktonOn = false;
    private bool agentsOn = false;
    //private bool terrainOn = true;

    //public bool pendingTrophicSlot = false;
    //public TrophicSlot pendingTrophicSlotRef;
    //public bool isSelectedTrophicSlot = false;
    //public TrophicSlot selectedTrophicSlotRef;

    //private bool _IsUnlocked


    public Vector2 decomposerOriginPos;
    public Vector2 algaeOriginPos;
    public Vector2 plantOriginPos;
    public Vector2 zooplanktonOriginPos;
    public int timeStepAlgaeOn = 0;
    public int timeStepPlantsOn = 0;
    public int timeStepDecomposersOn = 0;
    public int timeStepZooplanktonOn = 0;
    private int timeStepsLayerGrowthDuration = 1200;
    
    Lookup lookup => Lookup.instance;

    List<TrophicSlot> allTrophicSlots = new List<TrophicSlot>();
    public List<TrophicSlot> animalSlots = new List<TrophicSlot>();
    
    public TrophicSlot selectedSlot;
    

	public TrophicLayersManager() {  // constructor
        decomposersOn = true;  // first pass -- temporary?
        algaeOn = true;
        plantsOn = true;
        zooplanktonOn = true;
        agentsOn = true;
        //terrainOn = false;
        
        foreach (var element in lookup.knowledgeMaps)
        {
            for (int i = 0; i < element.startingSlotCount; i++)
            {
                var slot = new TrophicSlot(element);
                allTrophicSlots.Add(slot);
                
                if (slot.id == KnowledgeMapId.Animals)
                    animalSlots.Add(slot);
            }
        }
        
        //selectedTrophicSlotRef = terrainTier0.trophicSlots[0];        
        //isSelectedTrophicSlot = true;
        
        SetSlot(KnowledgeMapId.Animals);
        uiManager.worldSpiritHubUI.selectedWorldSpiritSlot = selectedSlot; //kingdomTerrain.trophicTiersList[0].trophicSlots[1];
        uiManager.brushesUI.selectedEssenceSlot = selectedSlot; //kingdomTerrain.trophicTiersList[0].trophicSlots[1];
    }

    public void SetSlot(KnowledgeMapId id) { SetSlot(lookup.GetTrophicSlotData(id)); }
    
    public void SetSlot(TrophicLayerSO data)
    {
        foreach (var slot in allTrophicSlots)
            if (slot.data.id == data.id)
                selectedSlot = slot;
    }
    
    public TrophicSlot GetSlot(TrophicLayerSO data) { return GetSlot(data.id); }
    
    public TrophicSlot GetSlot(KnowledgeMapId id)
    {
        foreach (var slot in allTrophicSlots)
            if (slot.data.id == id)
                return slot;
        
        Debug.LogError("Invalid slot " + id);
        return null;
    }
    
    /*public TrophicSlot GetSlot(TrophicLayerSO mapData)
    {
        return GetKingdom(mapData.kingdom).trophicTiersList[mapData.listIndex].trophicSlots[mapData.slotIndex];
    }
    
    TrophicKingdom GetKingdom(KingdomId id)
    {
        switch (id)
        {
            case KingdomId.Decomposers: return kingdomDecomposers;
            case KingdomId.Plants: return kingdomPlants;
            case KingdomId.Animals: return kingdomAnimals;
            case KingdomId.Terrain: return kingdomTerrain;
            case KingdomId.Other: return kingdomOther;
            default: Debug.LogError("Invalid kingdom " + id); return null;
        }
    }*/

    public void CreateTrophicSlotSpecies(TrophicSlot addedSlot, Vector2 spawnPos, int timeStep) {
        
        // reset things, figure out which slot was created:
        //isSelectedTrophicSlot = false;
        addedSlot.status = TrophicSlotStatus.On;
        
        switch (addedSlot.data.id)
        {
            case KnowledgeMapId.Decomposers: TurnOnDecomposers(spawnPos, timeStep); break;
            case KnowledgeMapId.Algae: TurnOnAlgae(spawnPos, timeStep); break;
            case KnowledgeMapId.Plants: TurnOnPlants(spawnPos, timeStep); break;
            case KnowledgeMapId.Microbes: TurnOnZooplankton(spawnPos, timeStep); break;
            case KnowledgeMapId.Animals: TurnOnAgents(); break;
        }
    }
    
    public void SetSlotStatus(KnowledgeMapId id, TrophicSlotStatus value)
    {
        GetSlot(id).status = value;
    }

    public void CheatUnlockAll() {
        foreach (var slot in allTrophicSlots)
            if (slot.status == TrophicSlotStatus.Locked)
                slot.status = TrophicSlotStatus.On;
        
        // ALGAE
        /*if(kingdomPlants.trophicTiersList[0].trophicSlots[0].status == TrophicSlotStatus.Locked) {            
            kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlotStatus.On;
        }
        if(kingdomPlants.trophicTiersList[1].trophicSlots[0].status == TrophicSlotStatus.Locked) {            
            kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlotStatus.On;
        }
        
        //check for unlocks:
        if(kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status == TrophicSlotStatus.Locked) {            
                kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlotStatus.On;  
        }
        
        if(kingdomAnimals.trophicTiersList[0].trophicSlots[0].status == TrophicSlotStatus.Locked) {            
            kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlotStatus.On;                
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[0].status == TrophicSlotStatus.Locked) { 
            kingdomAnimals.trophicTiersList[1].unlocked = true;
            kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlotStatus.On; 
        }

        if(kingdomAnimals.trophicTiersList[1].trophicSlots[1].status == TrophicSlotStatus.Locked) {                         
            kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlotStatus.On;                
        }
        if(kingdomAnimals.trophicTiersList[1].trophicSlots[2].status == TrophicSlotStatus.Locked) {                       
            kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlotStatus.On;                
        }
        if(kingdomAnimals.trophicTiersList[1].trophicSlots[3].status == TrophicSlotStatus.Locked) {                     
            kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlotStatus.On;                
        }*/
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
        //Debug.Log("PLANTS ON!");
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

    public float GetLayerLerp(KnowledgeMapId layer, int timeStep) {
        return IsLayerOn(layer) ?
            Mathf.Clamp01((float)(timeStep - TimeStepLayerOn(layer)) / (float)timeStepsLayerGrowthDuration) :
            0f;
    }
    
    public bool IsLayerOn(KnowledgeMapId layer)
    {
        switch (layer)
        {
            case KnowledgeMapId.Decomposers: return decomposersOn;
            case KnowledgeMapId.Algae: return algaeOn;
            case KnowledgeMapId.Plants: return plantsOn;
            case KnowledgeMapId.Microbes: return zooplanktonOn;
            case KnowledgeMapId.Animals: return agentsOn;
            default: return false;
        }
    }
    
    int TimeStepLayerOn(KnowledgeMapId layer)
    {
        switch (layer)
        {
            case KnowledgeMapId.Microbes: return timeStepZooplanktonOn; 
            case KnowledgeMapId.Plants: return timeStepPlantsOn; 
            case KnowledgeMapId.Algae: return timeStepAlgaeOn; 
            case KnowledgeMapId.Decomposers: return timeStepDecomposersOn; 
            default: return 0;
        }
    }
}
