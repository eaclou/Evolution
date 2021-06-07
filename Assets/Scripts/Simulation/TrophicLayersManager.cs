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

    // WPP
    /*
    public TrophicKingdom kingdomDecomposers;
    public TrophicKingdom kingdomPlants;
    public TrophicKingdom kingdomAnimals;
    public TrophicKingdom kingdomTerrain;
    public TrophicKingdom kingdomOther;
    */

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
    
    // WPP: now stored in SOs
    /*Sprite spiritWorldIcon => lookup.spiritWorldIcon;
    Sprite spiritStoneIcon => lookup.spiritStoneIcon;
    Sprite spiritAlgaeIcon => lookup.spiritAlgaeIcon;
    Sprite spiritPlantIcon => lookup.spiritPlantIcon;
    Sprite spiritZooplanktonIcon => lookup.spiritZooplanktonIcon;
    Sprite spiritVertebrateIcon => lookup.spiritVertebrateIcon;
    Sprite spiritDecomposerIcon => lookup.spiritDecomposerIcon;
    Sprite spiritPebblesIcon => lookup.spiritPebblesIcon;
    Sprite spiritSandIcon => lookup.spiritSandIcon;
    Sprite spiritMineralsIcon => lookup.spiritMineralsIcon;
    Sprite spiritWaterIcon => lookup.spiritWaterIcon;
    Sprite spiritAirIcon => lookup.spiritAirIcon;
    Color colorVertebratesLayer => lookup.colorVertebratesLayer;
    Color colorWorldLayer => lookup.colorWorldLayer;
    Color colorTerrainLayer => lookup.colorTerrainLayer; 
    Color colorMineralLayer => lookup.colorMineralLayer;
    Color colorWaterLayer => lookup.colorWaterLayer;
    Color colorAirLayer => lookup.colorAirLayer;
    Color colorPlantsLayer => lookup.colorPlantsLayer;
    Color colorZooplanktonLayer => lookup.colorZooplanktonLayer;
    Color colorAlgaeLayer => lookup.colorAlgaeLayer;
    Color colorDecomposersLayer => lookup.colorDecomposersLayer;*/
    
    List<TrophicSlot> allTrophicSlots = new List<TrophicSlot>();
    public List<TrophicSlot> animalSlots = new List<TrophicSlot>();
    
    public TrophicSlot selectedSlot;
    

    // * WPP: remove magic numbers
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

        // WPP
        // DECOMPOSERS::::  // hacky manual initialization for now!!!!
        /*kingdomDecomposers = new TrophicKingdom();
        kingdomDecomposers.name = "Decomposers";
        TrophicTier decomposersTier0 = new TrophicTier();
        decomposersTier0.trophicSlots[0].Initialize("Decomposers", TrophicSlotStatus.On, 0, 0, 0, spiritDecomposerIcon, colorDecomposersLayer);
        kingdomDecomposers.trophicTiersList.Add(decomposersTier0);

        // PLANTS::::
        kingdomPlants = new TrophicKingdom();
        kingdomPlants.name = "Plants";
        TrophicTier plantsTier0 = new TrophicTier();  // simple algae
        plantsTier0.trophicSlots[0].Initialize("Algae", TrophicSlotStatus.On, 1, 0, 0, spiritAlgaeIcon, colorAlgaeLayer);        
        kingdomPlants.trophicTiersList.Add(plantsTier0);
        TrophicTier plantsTier1 = new TrophicTier();  // bigger plants
        plantsTier1.trophicSlots[0].Initialize("Floating Plants", TrophicSlotStatus.Locked, 1, 1, 0, spiritPlantIcon, colorPlantsLayer);
        plantsTier1.trophicSlots[1].Initialize("Submerged Plants", TrophicSlotStatus.Locked, 1, 1, 1, spiritPlantIcon, colorPlantsLayer);
        kingdomPlants.trophicTiersList.Add(plantsTier1);
        
        // ANIMALS:::::
        kingdomAnimals = new TrophicKingdom();
        kingdomAnimals.name = "Animals";
        TrophicTier animalsTier0 = new TrophicTier();  // Zooplankton
        animalsTier0.trophicSlots[0].Initialize("Zooplankton", TrophicSlotStatus.Locked, 2, 0, 0, spiritZooplanktonIcon, colorZooplanktonLayer);
        kingdomAnimals.trophicTiersList.Add(animalsTier0);
        TrophicTier animalsTier1 = new TrophicTier();  // full Agents
        animalsTier1.trophicSlots[0].Initialize("Vertebrate", TrophicSlotStatus.Locked, 2, 1, 0, spiritVertebrateIcon, colorVertebratesLayer);
        animalsTier1.trophicSlots[1].Initialize("Vertebrate", TrophicSlotStatus.Locked, 2, 1, 1, spiritVertebrateIcon, colorVertebratesLayer);
        animalsTier1.trophicSlots[2].Initialize("Vertebrate", TrophicSlotStatus.Locked, 2, 1, 2, spiritVertebrateIcon, colorVertebratesLayer);
        animalsTier1.trophicSlots[3].Initialize("Vertebrate", TrophicSlotStatus.Locked, 2, 1, 3, spiritVertebrateIcon, colorVertebratesLayer);
        kingdomAnimals.trophicTiersList.Add(animalsTier1);


        // TERRAIN !!!!::::::
        kingdomTerrain = new TrophicKingdom();
        kingdomTerrain.name = "Terrain";
        TrophicTier terrainTier0 = new TrophicTier();
        terrainTier0.trophicSlots[0].Initialize("World", TrophicSlotStatus.On, 3, 0, 0, spiritWorldIcon, colorWorldLayer);
        terrainTier0.trophicSlots[1].Initialize("*World*", TrophicSlotStatus.On, 3, 0, 1, spiritStoneIcon, colorTerrainLayer);
        terrainTier0.trophicSlots[2].Initialize("Pebbles", TrophicSlotStatus.Locked, 3, 0, 2, spiritPebblesIcon, colorTerrainLayer);
        terrainTier0.trophicSlots[3].Initialize("Sand", TrophicSlotStatus.Locked, 3, 0, 3, spiritSandIcon, colorTerrainLayer);
        kingdomTerrain.trophicTiersList.Add(terrainTier0);

        // OTHER!!!!!%%%
        kingdomOther = new TrophicKingdom();
        kingdomOther.name = "Other";
        TrophicTier otherTier0 = new TrophicTier();
        otherTier0.trophicSlots[0].Initialize("Minerals", TrophicSlotStatus.Locked, 4, 0, 0, spiritMineralsIcon, colorMineralLayer);
        otherTier0.trophicSlots[1].Initialize("Water", TrophicSlotStatus.On, 4, 0, 1, spiritWaterIcon, colorWaterLayer);
        otherTier0.trophicSlots[2].Initialize("Air", TrophicSlotStatus.Locked, 4, 0, 2, spiritAirIcon, colorAirLayer);
        kingdomOther.trophicTiersList.Add(otherTier0);*/
                
        //selectedTrophicSlotRef = terrainTier0.trophicSlots[0];        
        //isSelectedTrophicSlot = true;

        // WPP
        SetSlot(KnowledgeMapId.Water);
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

        // WPP
        /*if (addedSlot.kingdomID == 0) { // decomposers:
            TurnOnDecomposers(spawnPos, timeStep);
        }
        if (addedSlot.kingdomID == 1) { // plants!:
            if (addedSlot.tierID == 0) { // ALGAE!:
                TurnOnAlgae(spawnPos, timeStep);
                //simManagerRef.vegetationManager.SpawnInitialAlgaeParticles(5f, new Vector4(spawnPos.x, spawnPos.y, 0f, 0f));
            }
            else {
                TurnOnPlants(spawnPos, timeStep);
            }
        }
        if (addedSlot.kingdomID == 2) { // Animals:
            if (addedSlot.tierID == 0) { // Animals:
                TurnOnZooplankton(spawnPos, timeStep);
                // Unlock Slots:
                //kingdomAnimals.trophicTiersList[1].unlocked = true;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[1].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[2].status = TrophicSlot.SlotStatus.Empty;
                //kingdomAnimals.trophicTiersList[1].trophicSlots[3].status = TrophicSlot.SlotStatus.Empty;
            }
            if(addedSlot.tierID == 1) {
                TurnOnAgents();
            }
        }*/
    }
    
    public void SetSlotStatus(KnowledgeMapId id, TrophicSlotStatus value)
    {
        GetSlot(id).status = value;
    }

    public void CheatUnlockAll() {
        foreach (var slot in allTrophicSlots)
            if (slot.status == TrophicSlotStatus.Locked)
                slot.status = TrophicSlotStatus.On;
        
        // WPP
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
