using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimEventsManager {

    private const int numAvailableEventsPerMenu = 4;

    public int curEventBucks = 25;
    public int cooldownDurationTicks = 60;  // ~one year to start?
    public int curCooldownCounter = 0;
    public bool isCooldown;

    public List<SimEventData> availableMinorEventsList;
    public List<SimEventData> availableMajorEventsList;
    public List<SimEventData> availableExtremeEventsList;

	public SimEventsManager(SimulationManager simManager) {

        availableMinorEventsList = new List<SimEventData>();
        availableMajorEventsList = new List<SimEventData>();
        availableExtremeEventsList = new List<SimEventData>();

        RegenerateAvailableMinorEvents(simManager);
        RegenerateAvailableMajorEvents(simManager);
        RegenerateAvailableExtremeEvents(simManager);
    }

    public void Tick() {
        if(isCooldown) {
            curCooldownCounter++;

            if(curCooldownCounter > cooldownDurationTicks) {
                curCooldownCounter = 0;
                isCooldown = false;
            }
        }
    }

    public void ExecuteEvent(SimulationManager simManager, SimEventData data) {
        curEventBucks -= data.cost;

        int posNeg = -1;
        if(data.isPositive) {
            posNeg = 1;
        }
        // Do stuff here:
        int delta = (int)Mathf.Pow(3, data.simEventTier) * posNeg;

        switch(data.type) {
            case SimEventData.SimEventType.BodyMutation:
                //
                simManager.settingsManager.ChangeTierBodyMutation(delta);         
                break;
            case SimEventData.SimEventType.BrainMutation:
                simManager.settingsManager.ChangeTierBrainMutation(delta);          
                break;
            case SimEventData.SimEventType.FoodAll:
                simManager.settingsManager.ChangeTierFoodDecay(delta);
                simManager.settingsManager.ChangeTierFoodPlant(delta); 
                break;
            case SimEventData.SimEventType.FoodCorpse:
                
                break;
            case SimEventData.SimEventType.FoodDecay:
                simManager.settingsManager.ChangeTierFoodDecay(delta);
                break;
            case SimEventData.SimEventType.FoodEgg:
                
                break;
            case SimEventData.SimEventType.FoodPlant:
                simManager.settingsManager.ChangeTierFoodPlant(delta);  
                break;
            case SimEventData.SimEventType.SpeciesLeastFit:
                int leastFitSpeciesID = 0; // -1;
                float worstFitness = 99999f;
                for(int i = 0; i < simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
                    float fitness = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].avgFitnessScore;
                    if(fitness < worstFitness) {
                        worstFitness = fitness;
                        leastFitSpeciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                if(data.isPositive) {
                    simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[leastFitSpeciesID].leaderboardGenomesList[0].candidateGenome, leastFitSpeciesID);        
                }
                else {
                    if(simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
                        simManager.masterGenomePool.completeSpeciesPoolsList[leastFitSpeciesID].isFlaggedForExtinction = true;
                        Debug.Log("FLAG EXTINCT: " + leastFitSpeciesID.ToString());
                    }                    
                }
                
                break;
            case SimEventData.SimEventType.SpeciesLeastNovel:
                int leastNovelSpeciesID = 0; // -1;
                float worstNovelty = 99999f;
                for(int i = 0; i < simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
                    float fitness = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].avgFitnessScore;
                    if(fitness < worstNovelty) {
                        worstNovelty = fitness;
                        leastNovelSpeciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                if(data.isPositive) {
                    simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[leastNovelSpeciesID].leaderboardGenomesList[0].candidateGenome, leastNovelSpeciesID);        
                }
                else {                    
                    if(simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
                        simManager.masterGenomePool.completeSpeciesPoolsList[leastNovelSpeciesID].isFlaggedForExtinction = true;
                        Debug.Log("FLAG EXTINCT: " + leastNovelSpeciesID.ToString());
                    } 
                }
                break;
            case SimEventData.SimEventType.SpeciesMostFit:

                int mostFitSpeciesID = 0; // -1;
                float bestFitness = 0f;
                for(int i = 0; i < simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
                    float fitness = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].avgFitnessScore;
                    
                    if(fitness > bestFitness) {
                        bestFitness = fitness;
                        mostFitSpeciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                if(data.isPositive) {
                    simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[mostFitSpeciesID].leaderboardGenomesList[0].candidateGenome, mostFitSpeciesID);        
                }
                else {
                    if (simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
                        simManager.masterGenomePool.completeSpeciesPoolsList[mostFitSpeciesID].isFlaggedForExtinction = true;
                        Debug.Log("FLAG EXTINCT: " + mostFitSpeciesID.ToString());
                    }
                }
                break;
            case SimEventData.SimEventType.SpeciesMostNovel:
                int mostNovelSpeciesID = 0;// -1;
                float bestNovelty = 0f;
                for(int i = 0; i < simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
                    float fitness = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].avgFitnessScore;
                    
                    if(fitness > bestNovelty) {
                        bestNovelty = fitness;
                        mostNovelSpeciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                if(data.isPositive) {
                    simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[mostNovelSpeciesID].leaderboardGenomesList[0].candidateGenome, mostNovelSpeciesID);        
                }
                else {
                    if (simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
                        simManager.masterGenomePool.completeSpeciesPoolsList[mostNovelSpeciesID].isFlaggedForExtinction = true;
                        Debug.Log("FLAG EXTINCT: " + mostNovelSpeciesID.ToString());
                    }
                }
                break;
            case SimEventData.SimEventType.WaterCurrents:
                simManager.environmentFluidManager.curTierWaterCurrents = Mathf.Clamp(simManager.environmentFluidManager.curTierWaterCurrents + delta, 0, 10);
                break;
            default:

                break;
        }

        // ^ Did stuff there

        isCooldown = true;

        RegenerateAvailableMinorEvents(simManager);
        RegenerateAvailableMajorEvents(simManager);
        RegenerateAvailableExtremeEvents(simManager);
    }

    public void GetRandomSimEventType(ref SimEventData newEventData) {
        SimEventData.SimEventType randType = (SimEventData.SimEventType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventType)).Length);
        newEventData.type = randType;

        newEventData.isPositive = true;
        float randPolarity = UnityEngine.Random.Range(0f, 1f);
        if(randPolarity < 0.5f) {
            newEventData.isPositive = false;
        }

        string qualifierTxt = "Decrease";
        if(newEventData.isPositive) {
            qualifierTxt = "Increase";
        }
        
        switch(randType) {
            case SimEventData.SimEventType.BodyMutation:
                //
                newEventData.name = "Body Mutation";
                newEventData.description = qualifierTxt + " the global mutation rate for creature bodies";                
                break;
            case SimEventData.SimEventType.BrainMutation:
                newEventData.name = "Brain Mutation";
                newEventData.description = qualifierTxt + " the global mutation rate for creature brain wirings";  
                break;
            case SimEventData.SimEventType.FoodAll:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast!";
                }
                else {
                    newEventData.name = "Famine!"; 
                }
                newEventData.description = qualifierTxt + " the global levels of ALL food types";  
                break;
            case SimEventData.SimEventType.FoodCorpse:
                newEventData.name = "Recycle";
                newEventData.description = qualifierTxt + " the global levels of carrion";  
                break;
            case SimEventData.SimEventType.FoodDecay:
                newEventData.name = "Detritus";
                newEventData.description = qualifierTxt + " the global levels of decayed organic matter";  
                break;
            case SimEventData.SimEventType.FoodEgg:
                newEventData.name = "Fertility";
                newEventData.description = qualifierTxt + " the global levels of eggs";  
                break;
            case SimEventData.SimEventType.FoodPlant:
                newEventData.name = "Foliage";
                newEventData.description = qualifierTxt + " the global levels of plants";  
                break;
            case SimEventData.SimEventType.SpeciesLeastFit:
                if (newEventData.isPositive) {
                    newEventData.name = "Double Down";
                    newEventData.description = "Create a new species branching off from the current least fit species";  
                }
                else {
                    newEventData.name = "Cull The Weak!";
                    newEventData.description = "Kill off the least fit species";  
                }                
                break;
            case SimEventData.SimEventType.SpeciesLeastNovel:
                if (newEventData.isPositive) {
                    newEventData.name = "Reward Mediocrity";
                    newEventData.description = "Create a new species branching off from the most average species";  
                }
                else {
                    newEventData.name = "Axe The Average";
                    newEventData.description = "Kill off the most average species";  
                } 
                break;
            case SimEventData.SimEventType.SpeciesMostFit:
                if (newEventData.isPositive) {
                    newEventData.name = "Rich Get Richer";
                    newEventData.description = "Create a new species branching off from the current most fit species";  
                }
                else {
                    newEventData.name = "Skim The Top";
                    newEventData.description = "Kill off the most fit species";  
                } 
                break;
            case SimEventData.SimEventType.SpeciesMostNovel:
                if (newEventData.isPositive) {
                    newEventData.name = "Doubly Different";
                    newEventData.description = "Create a new species branching off from the most unique species";  
                }
                else {
                    newEventData.name = "Conformity";
                    newEventData.description = "Kill off the most unique species";  
                } 
                break;
            case SimEventData.SimEventType.WaterCurrents:
                newEventData.name = "Weather";
                newEventData.description = qualifierTxt + " the speed of global water currents";
                break;
            default:

                break;
        }
    }

    public SimEventData GenerateNewRandomMinorEvent() {
        SimEventData newEventData = new SimEventData();
        newEventData.cost = 3;
        newEventData.simEventTier = 0;

        GetRandomSimEventType(ref newEventData);

        return newEventData;
    }

    public SimEventData GenerateNewRandomMajorEvent() {
        SimEventData newEventData = new SimEventData();
        newEventData.cost = 15;
        newEventData.simEventTier = 1;

        GetRandomSimEventType(ref newEventData);

        return newEventData;
    }

    public SimEventData GenerateNewRandomExtremeEvent() {
        SimEventData newEventData = new SimEventData();
        newEventData.cost = 75;
        newEventData.simEventTier = 2;

        GetRandomSimEventType(ref newEventData);

        return newEventData;
    }

    public void RegenerateAvailableMinorEvents(SimulationManager simManager) {
        availableMinorEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomMinorEvent(); // new SimEventData();
            //newEventData.description = "This is a minor event, number " + i.ToString();
            //newEventData.name = "Minor" + i.ToString();
            //newEventData.cost = 3;
            availableMinorEventsList.Add(newEventData);
        }
    }

    public void RegenerateAvailableMajorEvents(SimulationManager simManager) {
        availableMajorEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomMajorEvent(); // new SimEventData();
            //newEventData.description = "This is a Major event, number " + i.ToString();
            //newEventData.name = "Major" + i.ToString();
            //newEventData.cost = 15;
            availableMajorEventsList.Add(newEventData);
        }
    }

    public void RegenerateAvailableExtremeEvents(SimulationManager simManager) {
        availableExtremeEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomExtremeEvent(); // new SimEventData();
            //newEventData.description = "This is an EXTREME event, number " + i.ToString();
            //newEventData.name = "Xtreme" + i.ToString();
            //newEventData.cost = 75;
            availableExtremeEventsList.Add(newEventData);
        }
    }
}
