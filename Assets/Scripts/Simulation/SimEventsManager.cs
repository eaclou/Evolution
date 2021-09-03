using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// * WPP: Refactor -> repetition, overly-nested conditionals, expose values, remove dead code, shorten reference chains
public class SimEventsManager 
{
    SimulationManager simManager => SimulationManager.instance;

    private const int numAvailableEventsPerMenu = 4;

    public int curEventBucks = 250;
    public int cooldownDurationTicks = 100;  // ~one year to start?
    public int curCooldownCounter = 0;
    public bool isCooldown;

    public string mostRecentEventString = "";

    public List<SimEventData> availableMinorEventsList;
    public List<SimEventData> availableMajorEventsList;
    public List<SimEventData> availableExtremeEventsList;

    public List<SimEventData> completeEventHistoryList;

	public SimEventsManager() 
	{
        availableMinorEventsList = new List<SimEventData>();
        availableMajorEventsList = new List<SimEventData>();
        availableExtremeEventsList = new List<SimEventData>();

        completeEventHistoryList = new List<SimEventData>();

        RegenerateAvailableMinorEvents();
        RegenerateAvailableMajorEvents();
        RegenerateAvailableExtremeEvents();
    }

    public void Tick() 
    {
        if (!isCooldown) return;

        curCooldownCounter++;

        if (curCooldownCounter > cooldownDurationTicks) 
        {
            curCooldownCounter = 0;
            isCooldown = false;
        }
    }
    
    /*
    public void ExecuteEvent(SimulationManager simManager, SimEventData data) {
        curEventBucks -= data.cost;
        
        if(data.category == SimEventData.SimEventCategories.Minor) {
            ExecuteEventMinor(simManager, data);
        }
        else if(data.category == SimEventData.SimEventCategories.Major) {
            ExecuteEventMajor(simManager, data);
        }
        else {
            ExecuteEventExtreme(simManager, data);
        }

        isCooldown = true;
        mostRecentEventString = data.name + " Activated! ( -$" + data.cost + ")";
        RegenerateAvailableMinorEvents(simManager);
        RegenerateAvailableMajorEvents(simManager);
        RegenerateAvailableExtremeEvents(simManager);

        data.timeStepActivated = simManager.simAgeTimeSteps;
        completeEventHistoryList.Add(data);
    }
    public void ExecuteEventMinor(SimulationManager simManager, SimEventData data) {
        
        int posNeg = -1;
        if(data.isPositive) {
            posNeg = 1;
        }
        // Do stuff here:
        int deltaTiers = 1 * posNeg;

        int randomSpeciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[UnityEngine.Random.Range(0, simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count)];

        switch(data.typeMinor) {
            case SimEventData.SimEventTypeMinor.BodyModules:
                simManager.settingsManager.ChangeTierBodyMutationModules(deltaTiers);     
                break;
            case SimEventData.SimEventTypeMinor.BodyMutation:
                float randRollBodyMutation = UnityEngine.Random.Range(0f, 1f);
                if(randRollBodyMutation < 0.67f) {
                    simManager.settingsManager.ChangeTierBodyMutationAmplitude(deltaTiers);
                }
                if(randRollBodyMutation > 0.33f) {
                    simManager.settingsManager.ChangeTierBodyMutationFrequency(deltaTiers);
                }         
                break;
            case SimEventData.SimEventTypeMinor.BrainMutation:
                float randRollBrainMutation = UnityEngine.Random.Range(0f, 1f);
                if(randRollBrainMutation < 0.67f) {
                    simManager.settingsManager.ChangeTierBrainMutationAmplitude(deltaTiers);
                }
                if(randRollBrainMutation > 0.33f) {
                    simManager.settingsManager.ChangeTierBrainMutationFrequency(deltaTiers);
                }
                break;
            case SimEventData.SimEventTypeMinor.BrainSize:
                float randRollGrowth = UnityEngine.Random.Range(0f, 1f);                
                simManager.settingsManager.ChangeTierBrainMutationNewLink(deltaTiers);                
                simManager.settingsManager.ChangeTierBrainMutationNewHiddenNeuron(deltaTiers);                
                simManager.settingsManager.ChangeTierBrainMutationWeightDecay(deltaTiers);                             
                break;
            case SimEventData.SimEventTypeMinor.CalmWaters:
                simManager.environmentFluidManager.curTierWaterCurrents = 1; // Mathf.Clamp(simManager.environmentFluidManager.curTierWaterCurrents + deltaTiers, 0, 10);
                //simManager.environmentFluidManager.RerollForcePoints();
                break;
            case SimEventData.SimEventTypeMinor.CreateSpecies:   
                //AgentGenome newGenome = simManager.masterGenomePool.completeSpeciesPoolsList[randomSpeciesID].GetNewMutatedGenome();
                //simManager.AddNewSpecies(newGenome, randomSpeciesID);
                //simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[randomSpeciesID].leaderboardGenomesList[0].candidateGenome, randomSpeciesID);
                break;
            case SimEventData.SimEventTypeMinor.FoodCorpse:
                simManager.settingsManager.ChangeTierFoodCorpse(deltaTiers); 
                break;
            case SimEventData.SimEventTypeMinor.FoodEgg:
                simManager.settingsManager.ChangeTierFoodEgg(deltaTiers); 
                break;
            case SimEventData.SimEventTypeMinor.FoodDecay:
                simManager.settingsManager.ChangeTierFoodDecay(deltaTiers);
                break;
            case SimEventData.SimEventTypeMinor.FoodPlant:
                simManager.settingsManager.ChangeTierFoodPlant(deltaTiers); 
                break;
            case SimEventData.SimEventTypeMinor.KillSpecies:
                if(simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {
                    simManager.masterGenomePool.FlagSpeciesExtinct(randomSpeciesID);
                    //simManager.masterGenomePool.completeSpeciesPoolsList[randomSpeciesID].isFlaggedForExtinction = true;
                    //Debug.Log("FLAG EXTINCT: " + randomSpeciesID.ToString());
                }
                else {
                    Debug.LogError("ERROR: Couldn't Kill last remaining species");
                }                            
                break;            
            default:
                break;            
        }

        // ^ Did stuff there
    }
    public void ExecuteEventMajor(SimulationManager simManager, SimEventData data) {
        int posNeg = -1;
        if(data.isPositive) {
            posNeg = 1;
        }
        // Do stuff here:
        int deltaTiers = 3 * posNeg;

        int speciesIndex = GetEventSpeciesID(simManager, data);
        //int randomSpeciesID = UnityEngine.Random.Range(0, simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count);

        switch(data.typeMajor) {
            case SimEventData.SimEventTypeMajor.BodyModules:
                simManager.settingsManager.ChangeTierBodyMutationModules(deltaTiers);     
                break;
            case SimEventData.SimEventTypeMajor.BodyMutation:
                float randRollBodyMutation = UnityEngine.Random.Range(0f, 1f);
                if(randRollBodyMutation < 0.67f) {
                    simManager.settingsManager.ChangeTierBodyMutationAmplitude(deltaTiers);
                }
                if(randRollBodyMutation > 0.33f) {
                    simManager.settingsManager.ChangeTierBodyMutationFrequency(deltaTiers);
                }         
                break;
            case SimEventData.SimEventTypeMajor.BrainMutation:
                float randRollBrainMutation = UnityEngine.Random.Range(0f, 1f);
                if(randRollBrainMutation < 0.67f) {
                    simManager.settingsManager.ChangeTierBrainMutationAmplitude(deltaTiers);
                }
                if(randRollBrainMutation > 0.33f) {
                    simManager.settingsManager.ChangeTierBrainMutationFrequency(deltaTiers);
                }
                break;
            case SimEventData.SimEventTypeMajor.BrainSize:
                float randRollGrowth = UnityEngine.Random.Range(0f, 1f);                
                simManager.settingsManager.ChangeTierBrainMutationNewLink(deltaTiers);                
                simManager.settingsManager.ChangeTierBrainMutationNewHiddenNeuron(deltaTiers);                
                simManager.settingsManager.ChangeTierBrainMutationWeightDecay(deltaTiers);                             
                break;
            case SimEventData.SimEventTypeMajor.Gale:
                simManager.environmentFluidManager.curTierWaterCurrents = 4; // Mathf.Clamp(simManager.environmentFluidManager.curTierWaterCurrents + deltaTiers, 0, 10);
                simManager.environmentFluidManager.RerollForcePoints();
                break;
            case SimEventData.SimEventTypeMajor.CreateSpecies:                
                //int speciesIndex = GetEventSpeciesID(simManager, data);    
                //AgentGenome newGenome = simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].GetNewMutatedGenome();
                //simManager.AddNewSpecies(newGenome, speciesIndex);
                //simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].leaderboardGenomesList[0].candidateGenome, speciesIndex);
                break;
            case SimEventData.SimEventTypeMajor.FoodCorpse:
                simManager.settingsManager.ChangeTierFoodCorpse(deltaTiers); 
                break;
            case SimEventData.SimEventTypeMajor.FoodEgg:
                simManager.settingsManager.ChangeTierFoodEgg(deltaTiers); 
                break;
            case SimEventData.SimEventTypeMajor.FoodDecay:
                simManager.settingsManager.ChangeTierFoodDecay(deltaTiers);
                break;
            case SimEventData.SimEventTypeMajor.FoodPlant:
                simManager.settingsManager.ChangeTierFoodPlant(deltaTiers); 
                break;
            case SimEventData.SimEventTypeMajor.KillSpecies:                
                if(simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {    
                    simManager.masterGenomePool.FlagSpeciesExtinct(speciesIndex);
                    //simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].isFlaggedForExtinction = true;
                    //Debug.Log("FLAG EXTINCT: " + speciesIndex.ToString());
                }
                else {
                    Debug.LogError("ERROR: Couldn't Kill last remaining species");
                }                            
                break;            
            default:
                break;            
        }
    }
    public void ExecuteEventExtreme(SimulationManager simManager, SimEventData data) {
        int posNeg = -1;
        if(data.isPositive) {
            posNeg = 1;
        }
        // Do stuff here:
        int deltaTiers = 9 * posNeg;

        int speciesIndex = GetEventSpeciesID(simManager, data);
        //int randomSpeciesID = UnityEngine.Random.Range(0, simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count);

        switch(data.typeExtreme) {
            case SimEventData.SimEventTypeExtreme.BodyModules:
                simManager.settingsManager.ChangeTierBodyMutationModules(deltaTiers);     
                break;
            case SimEventData.SimEventTypeExtreme.BodyMutation:
                float randRollBodyMutation = UnityEngine.Random.Range(0f, 1f);
                if(randRollBodyMutation < 0.67f) {
                    simManager.settingsManager.ChangeTierBodyMutationAmplitude(deltaTiers);
                }
                if(randRollBodyMutation > 0.33f) {
                    simManager.settingsManager.ChangeTierBodyMutationFrequency(deltaTiers);
                }         
                break;
            case SimEventData.SimEventTypeExtreme.BrainMutation:
                float randRollBrainMutation = UnityEngine.Random.Range(0f, 1f);
                if(randRollBrainMutation < 0.67f) {
                    simManager.settingsManager.ChangeTierBrainMutationAmplitude(deltaTiers);
                }
                if(randRollBrainMutation > 0.33f) {
                    simManager.settingsManager.ChangeTierBrainMutationFrequency(deltaTiers);
                }
                break;
            case SimEventData.SimEventTypeExtreme.BrainSize:
                float randRollGrowth = UnityEngine.Random.Range(0f, 1f);                
                simManager.settingsManager.ChangeTierBrainMutationNewLink(deltaTiers);                
                simManager.settingsManager.ChangeTierBrainMutationNewHiddenNeuron(deltaTiers);                
                simManager.settingsManager.ChangeTierBrainMutationWeightDecay(deltaTiers);                             
                break;
            case SimEventData.SimEventTypeExtreme.Hurricane:
                simManager.environmentFluidManager.curTierWaterCurrents = 10; // Mathf.Clamp(simManager.environmentFluidManager.curTierWaterCurrents + deltaTiers, 0, 10);
                simManager.environmentFluidManager.RerollForcePoints();
                break;
            case SimEventData.SimEventTypeExtreme.CreateSpecies:                
                //int speciesIndex = GetEventSpeciesID(simManager, data);     
                for(int i = 0; i < data.quantity; i++) {
                    int speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[UnityEngine.Random.Range(0, simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count)];
                    //int candID = UnityEngine.Random.Range(0, simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList.Count);
                    // Get mutated version first?
                    //simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList[candID].candidateGenome
                    //AgentGenome newGenome = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].GetNewMutatedGenome();
                    //simManager.AddNewSpecies(newGenome, speciesID);
                }                
                break;
            case SimEventData.SimEventTypeExtreme.FoodCorpse:
                simManager.settingsManager.ChangeTierFoodCorpse(deltaTiers); 
                break;
            case SimEventData.SimEventTypeExtreme.FoodEgg:
                simManager.settingsManager.ChangeTierFoodEgg(deltaTiers); 
                break;
            case SimEventData.SimEventTypeExtreme.FoodDecay:
                simManager.settingsManager.ChangeTierFoodDecay(deltaTiers);
                break;
            case SimEventData.SimEventTypeExtreme.FoodPlant:
                simManager.settingsManager.ChangeTierFoodPlant(deltaTiers); 
                break;
            case SimEventData.SimEventTypeExtreme.KillSpecies:
                int randSpeciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[UnityEngine.Random.Range(0, simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count)];
                if(simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count > 1) {    
                    for(int i = 0; i < simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
                        if(simManager.masterGenomePool.currentlyActiveSpeciesIDList[i] != randSpeciesID) {
                            simManager.masterGenomePool.FlagSpeciesExtinct(simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]);
                            //simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].isFlaggedForExtinction = true;
                            //Debug.Log("FLAG EXTINCT: " + speciesIndex.ToString());
                        }
                    }
                }
                else {
                    Debug.LogError("ERROR: Couldn't Kill last remaining species");
                }                            
                break;            
            default:
                break;            
        }
    }
    */
    
    private int GetEventSpeciesID(SimEventData data) 
    {
        int speciesID = 0; // -1;
        float recordLow = 9999999f;
        float recordHigh = -9999999f;
        bool positivePolaritySetsHighRecord = true;
        
        foreach (var id in simManager.masterGenomePool.currentlyActiveSpeciesIDList)
        {
            var value = 0f;
        
            switch (data.speciesQualifier)
            {
                case SpeciesQualifier.Age:
                    positivePolaritySetsHighRecord = false;
                    break;
                case SpeciesQualifier.BodySize:
                    break;
                case SpeciesQualifier.Fitness:
                    value = simManager.masterGenomePool.completeSpeciesPoolsList[id].avgCandidateData.performanceData.totalTicksAlive;
                    break;
                 // **** IMPLEMENT ACTUAL NOVELTY SCORE!!!! ****   
                 case SpeciesQualifier.Novelty:
                    value = Random.Range(0f, 1f);
                    break;
            }
            
            bool checkHighRecord = positivePolaritySetsHighRecord == data.polarity;
            
            if (checkHighRecord && value > recordHigh)
            {
                recordHigh = value;
                speciesID = id;
            }
            else if (!checkHighRecord && value < recordLow)
            {
                recordLow = value;
                speciesID = id;
            }
        }

        return speciesID;
    }

    public SimEventData GenerateNewRandomMinorEvent(List<SimEventData> eventList) {
        SimEventData newEventData = new SimEventData(3, 1, SimEventCategories.Minor);
        var randType = SimEventData.GetRandomMinorEventType();
        
        // Check if impossible or duplicates present
        for (int i = 0; i < 8; i++) 
        {
            if (eventList.Count <= 0 || eventList.Any(e => e.typeMinor == randType))
                break;

            randType = SimEventData.GetRandomMinorEventType();
        }
        
        newEventData.typeMinor = randType;
        newEventData.Refresh();
        return newEventData;
    }

    // * WPP: apply changes from Minor events to Major and Extreme events
    public SimEventData GenerateNewRandomMajorEvent(List<SimEventData> eventList) {
        SimEventData newEventData = new SimEventData(15, 1, SimEventCategories.Major);

        // Avoid Duplicates????
        SimEventTypeMajor randType = (SimEventTypeMajor)Random.Range(0, System.Enum.GetValues(typeof(SimEventTypeMajor)).Length);
        for(int i = 0; i < 8; i++) {
            // Check if impossible:
            
            // check for DUPES:
            if (eventList.Count > 0) {  // if not the first selection:
                randType = (SimEventTypeMajor)Random.Range(0, System.Enum.GetValues(typeof(SimEventTypeMajor)).Length);
                // reroll isPositive?

                bool duplicateDetected = false;
                for(int j = 0; j < eventList.Count; j++) {
                    if(randType == eventList[j].typeMajor) {
                        //if(newEventData.isPositive == eventList[j].isPositive) {                            
                        duplicateDetected = true;                            
                        //}
                        break;
                    }
                }
                if(duplicateDetected) {
                    //Debug.Log("Duplicate detected! iter: " + i.ToString());
                    // try again (up to 8 times)
                }
                else {
                    break;
                }
            }
            else {  // first One
                break;
            }
        }
        
        newEventData.typeMajor = randType;

        string qualifierTxt = "DECREASE";
        if(newEventData.isPositive) {
            qualifierTxt = "INCREASE";
        }

        SpeciesQualifier randQualifier = (SpeciesQualifier)Random.Range(1, 5);
        
        switch(randType) {
            case SimEventTypeMajor.BodyModules:
                if (newEventData.isPositive) {
                    newEventData.name = "Differentiate Senses II";
                    newEventData.description = " Moderately " + qualifierTxt + " the chance of altering the creatures' sensory capabilities";       
                }
                else {
                    newEventData.name = "Settle Senses II"; 
                    newEventData.description = " Moderately " + qualifierTxt + " the chance of altering creature sensors";       
                }       
                break;
            case SimEventTypeMajor.BodyMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Morph Proportions";
                    newEventData.description = " Moderately INCREASE the global mutation rate for creature bodies";         
                }
                else {
                    newEventData.name = "Stabilize Forms"; 
                    newEventData.description = " Moderately DECREASE the global mutation rate for creature bodies";       
                }           
                break;
            case SimEventTypeMajor.BrainMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Neural Plasticity II";
                    newEventData.description = " Moderately " + qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                else {
                    newEventData.name = "Harden Axons II"; 
                    newEventData.description = " Moderately " + qualifierTxt + " the chance of rewiring creature brain"; 
                }
                break;
            case SimEventTypeMajor.BrainSize:
                if (newEventData.isPositive) {
                    newEventData.name = "Inflate Brain II";
                    newEventData.description = "Moderately INCREASE the chance for new Neurons and connections between them within creatures' brains";  
                }
                else {
                    newEventData.name = "Shrink Brain II"; 
                    newEventData.description = "Moderately DECREASE the chance for new Neurons and connections between them within creatures' brains";  
                }                
                break;
            case SimEventTypeMajor.Gale:
                
                newEventData.name = "Gale";                 
                newEventData.description = "Set the speed of water currents to a steady flow";  
                break;
            case SimEventTypeMajor.CreateSpecies:                
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                string qualityString = "";
                if(randQualifier == SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "New Species\n(" + qualityString + ")";
                newEventData.description = "A new lineage emerges, originating from the current " + qualityString + " species";
                break;

            case SimEventTypeMajor.FoodCorpse:
                if (newEventData.isPositive) {
                    newEventData.name = "Slow Decomposition II";
                    newEventData.description = "Slow the rate at which dead creatures' bodies break down into nutrients";  
                }
                else {
                    newEventData.name = "Recycled Material II"; 
                    newEventData.description = "Increase the rate at which dead creatures' bodies are broken down into their component parts";  
                } 
                break;
            case SimEventTypeMajor.FoodEgg:
                if (newEventData.isPositive) {
                    newEventData.name = "Fertility II";
                    newEventData.description = "Moderately INCREASE the global frequency of egg-laying and increases durability of egg sacks";  
                }
                else {
                    newEventData.name = "Barren II"; 
                    newEventData.description = "Moderately DECREASE the global frequency of egg-laying and lower durability of egg sacks";   
                }
                break;
            case SimEventTypeMajor.FoodDecay:
                if (newEventData.isPositive) {
                    newEventData.name = "Nutrient Increase II";
                    newEventData.description = "Moderately INCREASE the global levels of basic nutrients";  
                }
                else {
                    newEventData.name = "Nutrient Shortage II"; 
                    newEventData.description = "Moderately DECREASE the global levels of basic nutrients";   
                }
                break;
            case SimEventTypeMajor.FoodPlant:
                if (newEventData.isPositive) {
                    newEventData.name = "Algae Bloom II";
                    newEventData.description = "Moderately INCREASE the global levels of plants";  
                }
                else {
                    newEventData.name = "Algae Decrease II"; 
                    newEventData.description = "Moderately DECREASE the global levels of plants";   
                }
                break;
            /*case SimEventData.SimEventTypeMajor.FoodAll:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                    newEventData.description = "Greatly INCREASE the global levels of all food types";  
                }
                else {
                    newEventData.name = "Famine"; 
                    newEventData.description = "Greatly DECREASE the global levels of all food types";   
                }
                break;*/
            case SimEventTypeMajor.KillSpecies:
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                qualityString = "";
                if(randQualifier == SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "Kill " + qualityString + " Species";
                newEventData.description = "The current " + qualityString + " species goes extinct";
                break;            
            default:
                break;
        }

        return newEventData;
    }

    public SimEventData GenerateNewRandomExtremeEvent(List<SimEventData> eventList) {
        SimEventData newEventData = new SimEventData(75, 3, SimEventCategories.Extreme);

        // Avoid Duplicates????
        SimEventTypeExtreme randType = (SimEventTypeExtreme)Random.Range(0, System.Enum.GetValues(typeof(SimEventTypeExtreme)).Length);
        for(int i = 0; i < 8; i++) {
            // Check if impossible:
            
            // check for duplicates:
            if (eventList.Count > 0) {  // if not the first selection:
                randType = (SimEventTypeExtreme)Random.Range(0, System.Enum.GetValues(typeof(SimEventTypeExtreme)).Length);
                // reroll isPositive?

                bool duplicateDetected = false;
                for(int j = 0; j < eventList.Count; j++) {
                    if(randType == eventList[j].typeExtreme) {
                        //if(newEventData.isPositive == eventList[j].isPositive) {                            
                        duplicateDetected = true;                            
                        //}
                        break;
                    }
                }
                if(duplicateDetected) {
                    //Debug.Log("Duplicate detected! iter: " + i.ToString());
                    // try again (up to 8 times)
                }
                else {
                    break;
                }
            }
            else {  // first One
                break;
            }
        }
        
        newEventData.typeExtreme = randType;

        string qualifierTxt = "DECREASE";
        if(newEventData.isPositive) {
            qualifierTxt = "INCREASE";
        }

        SpeciesQualifier randQualifier = (SpeciesQualifier)Random.Range(1, 5);
        
        switch(randType) {
            case SimEventTypeExtreme.BodyModules:
                if (newEventData.isPositive) {
                    newEventData.name = "Differentiate Senses III";
                    newEventData.description = " Greatly " + qualifierTxt + " the chance of altering the creatures' sensory capabilities";       
                }
                else {
                    newEventData.name = "Settle Senses III"; 
                    newEventData.description = " Greatly " + qualifierTxt + " the chance of altering creature sensors";       
                }       
                break;
            case SimEventTypeExtreme.BodyMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Exaggerate Proportions";
                    newEventData.description = " Greatly INCREASE the global mutation rate for creature bodies";         
                }
                else {
                    newEventData.name = "Preserve Forms"; 
                    newEventData.description = " Greatly DECREASE the global mutation rate for creature bodies";       
                }           
                break;
            case SimEventTypeExtreme.BrainMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Neural Plasticity III";
                    newEventData.description = " Greatly " + qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                else {
                    newEventData.name = "Harden Axons III"; 
                    newEventData.description = " Greatly " + qualifierTxt + " the chance of rewiring creature brain"; 
                }
                break;
            case SimEventTypeExtreme.BrainSize:
                if (newEventData.isPositive) {
                    newEventData.name = "Inflate Brain III";
                    newEventData.description = "Greatly INCREASE the chance for new Neurons and connections between them within creatures' brains";  
                }
                else {
                    newEventData.name = "Shrink Brain III"; 
                    newEventData.description = "Greatly DECREASE the chance for new Neurons and connections between them within creatures' brains";  
                }                
                break;
            case SimEventTypeExtreme.Hurricane:                
                newEventData.name = "Maelstrom";                 
                newEventData.description = "Greatly increase the magnitude and turbulence of the water's currents";  
                break;
            case SimEventTypeExtreme.CreateSpecies:                
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                string qualityString = "";
                if(randQualifier == SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "New Species\n(" + qualityString + ")";
                newEventData.description = "Up to " + newEventData.quantity + " new lineages emerge, originating from the current " + qualityString + " species";
                break;
            case SimEventTypeExtreme.FoodCorpse:
                if (newEventData.isPositive) {
                    newEventData.name = "Slow Decomposition III";
                    newEventData.description = "Slow the rate at which dead creatures' bodies break down into nutrients";  
                }
                else {
                    newEventData.name = "Recycled Material III"; 
                    newEventData.description = "Increase the rate at which dead creatures' bodies are broken down into their component parts";  
                } 
                break;
            case SimEventTypeExtreme.FoodEgg:
                if (newEventData.isPositive) {
                    newEventData.name = "Fertility III";
                    newEventData.description = "Greatly INCREASE the global frequency of egg-laying and increases durability of egg sacks";  
                }
                else {
                    newEventData.name = "Barren III"; 
                    newEventData.description = "Greatly DECREASE the global frequency of egg-laying and lower durability of egg sacks";   
                }
                break;
            case SimEventTypeExtreme.FoodDecay:
                if (newEventData.isPositive) {
                    newEventData.name = "Nutrient Increase III";
                    newEventData.description = "Greatly INCREASE the global levels of basic nutrients";  
                }
                else {
                    newEventData.name = "Nutrient Shortage III"; 
                    newEventData.description = "Greatly DECREASE the global levels of basic nutrients";   
                }
                break;
            case SimEventTypeExtreme.FoodPlant:
                if (newEventData.isPositive) {
                    newEventData.name = "Algae Bloom III";
                    newEventData.description = "Greatly INCREASE the global levels of plants";  
                }
                else {
                    newEventData.name = "Algae Decrease III"; 
                    newEventData.description = "Greatly DECREASE the global levels of plants";   
                }
                break;
            case SimEventTypeExtreme.FoodAll:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                    newEventData.description = "Greatly INCREASE the global levels of all food types";  
                }
                else {
                    newEventData.name = "Famine"; 
                    newEventData.description = "Greatly DECREASE the global levels of all food types";   
                }
                break;
            case SimEventTypeExtreme.KillSpecies:
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                qualityString = "";
                if(randQualifier == SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "Mass Extinction";
                newEventData.description = "All but one species suffer extinction";
                break;            
            default:
                newEventData.name = "DEFAULT ERROR";
                break;
        }

        return newEventData;
    }

    // * WPP: condense into RegenerateAvailableEvents(List<SimEventData>, delegate{SimEventData GenerateEvent(List<SimEventData)})
    public void RegenerateAvailableMinorEvents() {
        availableMinorEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomMinorEvent(availableMinorEventsList);           
            availableMinorEventsList.Add(newEventData);
        }
    }

    public void RegenerateAvailableMajorEvents() {
        availableMajorEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomMajorEvent(availableMajorEventsList);           
            availableMajorEventsList.Add(newEventData);
        }
    }

    public void RegenerateAvailableExtremeEvents() {
        availableExtremeEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomExtremeEvent(availableExtremeEventsList);           
            availableExtremeEventsList.Add(newEventData);
        }
    }
}

#region Dead code (please delete)
    /*public void GetRandomSimEventType(ref SimEventData newEventData) {
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
            case SimEventData.SimEventType.BodyMutationBasic:
                //
                if (newEventData.isPositive) {
                    newEventData.name = "Myriad Forms";
                    newEventData.description = "Increase the global mutation rate for creature bodies";         
                }
                else {
                    newEventData.name = "Stabilize"; 
                    newEventData.description = "Decrease the global mutation rate for creature bodies";       
                }       
                break;
            case SimEventData.SimEventType.BodyMutationModule:
                //
                if (newEventData.isPositive) {
                    newEventData.name = "Sensory Overload";
                    newEventData.description = qualifierTxt + " the chance of altering creature sensor arrays";       
                }
                else {
                    newEventData.name = "Enough Sense"; 
                    newEventData.description = qualifierTxt + " the chance of altering creature sensor arrays";       
                }               
                break;
            case SimEventData.SimEventType.BrainMutationBasic:
                if (newEventData.isPositive) {
                    newEventData.name = "Neural Plasticity";
                    newEventData.description = qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                else {
                    newEventData.name = "Hardwired"; 
                    newEventData.description = qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                break;
            case SimEventData.SimEventType.BrainMutationGrowth:
                if (newEventData.isPositive) {
                    newEventData.name = "Inflate Brain";
                    newEventData.description = "Encourage creature brain size growth";  
                }
                else {
                    newEventData.name = "Shrink Brain"; 
                    newEventData.description = qualifierTxt + " the chance for creature brain size growth";  
                }                
                break;
            case SimEventData.SimEventType.FoodAll:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                }
                else {
                    newEventData.name = "Famine"; 
                }
                newEventData.description = qualifierTxt + " the global levels of ALL food types";  
                break;
            case SimEventData.SimEventType.FoodCorpse:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                }
                else {
                    newEventData.name = "Famine"; 
                }
                newEventData.name = "Recycle";
                newEventData.description = qualifierTxt + " the global levels of carrion";  
                break;
            case SimEventData.SimEventType.FoodDecay:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                }
                else {
                    newEventData.name = "Famine"; 
                }
                newEventData.name = "Detritus";
                newEventData.description = qualifierTxt + " the global levels of decayed organic matter";  
                break;
            case SimEventData.SimEventType.FoodEgg:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                }
                else {
                    newEventData.name = "Famine"; 
                }
                newEventData.name = "Fertility";
                newEventData.description = qualifierTxt + " the global levels of eggs";  
                break;
            case SimEventData.SimEventType.FoodPlant:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                }
                else {
                    newEventData.name = "Famine"; 
                }
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
                if (newEventData.isPositive) {
                    newEventData.name = "Storm";
                    //newEventData.description = "Create a new species branching off from the most unique species";  
                }
                else {
                    newEventData.name = "Calm Water";
                    //newEventData.description = "Kill off the most unique species";  
                } 
                //newEventData.name = "Weather";
                newEventData.description = qualifierTxt + " the speed of global water currents";
                break;
            default:

                break;
        }
    }
    */
#endregion
