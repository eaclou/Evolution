using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimEventsManager {

    private const int numAvailableEventsPerMenu = 4;

    public int curEventBucks = 250;
    public int cooldownDurationTicks = 100;  // ~one year to start?
    public int curCooldownCounter = 0;
    public bool isCooldown;

    public string mostRecentEventString = "";

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
                simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[randomSpeciesID].leaderboardGenomesList[0].candidateGenome, randomSpeciesID);
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
                    simManager.masterGenomePool.completeSpeciesPoolsList[randomSpeciesID].isFlaggedForExtinction = true;
                    Debug.Log("FLAG EXTINCT: " + randomSpeciesID.ToString());
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
                simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].leaderboardGenomesList[0].candidateGenome, speciesIndex);
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
                    simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].isFlaggedForExtinction = true;
                    Debug.Log("FLAG EXTINCT: " + speciesIndex.ToString());
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
                    int candID = UnityEngine.Random.Range(0, simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList.Count);
                    simManager.AddNewSpecies(simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList[candID].candidateGenome, speciesID);
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
                            simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].isFlaggedForExtinction = true;
                            Debug.Log("FLAG EXTINCT: " + speciesIndex.ToString());
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

    private int GetEventSpeciesID(SimulationManager simManager, SimEventData data) {
        int speciesID = 0; // -1;
        float recordLow = 9999999f;
        float recordHigh = -9999999f;
        for (int i = 0; i < simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
            if (data.speciesQualifier == SimEventData.SpeciesQualifier.Age) {
                float val = (float)simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].yearCreated;

                if(data.polarity) {
                    if (val < recordLow) {
                        recordLow = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                else {
                    if (val > recordHigh) {
                        recordHigh = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
            }
            else if(data.speciesQualifier == SimEventData.SpeciesQualifier.BodySize) {
                float val = (float)simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].avgBodySize;

                if(data.polarity) {
                    if (val > recordHigh) {
                        recordLow = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                else {
                    if (val < recordLow) {
                        recordHigh = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
            }
            else if(data.speciesQualifier == SimEventData.SpeciesQualifier.Fitness) {
                float val = (float)simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].avgFitnessScore;

                if(data.polarity) {
                    if (val > recordHigh) {
                        recordLow = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                else {
                    if (val < recordLow) {
                        recordHigh = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
            }
            else if(data.speciesQualifier == SimEventData.SpeciesQualifier.Novelty) {  // **** IMPLEMENT ACTUAL NOVELTY SCORE!!!! ****
                float val = UnityEngine.Random.Range(0f, 1f); // (float)simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[i]].av;

                if(data.polarity) {
                    if (val > recordHigh) {
                        recordLow = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
                else {
                    if (val < recordLow) {
                        recordHigh = val;
                        speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[i];
                    }
                }
            }
            else {
                //Debug.LogError("No such enum found!?!?! Now you've done it...");
            }
        }       

        return speciesID;
    } 

    public SimEventData GenerateNewRandomMinorEvent(List<SimEventData> eventList) {
        SimEventData newEventData = new SimEventData();
        newEventData.cost = 3;
        newEventData.category = SimEventData.SimEventCategories.Minor;
        newEventData.quantity = 1;

        newEventData.isPositive = true;
        newEventData.polarity = true;
        float randPolarity = UnityEngine.Random.Range(0f, 1f);
        if(randPolarity < 0.5f) {
            newEventData.polarity = false;
        }
        float randSign = UnityEngine.Random.Range(0f, 1f);
        if(randSign < 0.5f) {
            newEventData.isPositive = false;
        }

        // Avoid Duplicates????
        SimEventData.SimEventTypeMinor randType = (SimEventData.SimEventTypeMinor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventTypeMinor)).Length);
        for(int i = 0; i < 8; i++) {
            
            // Check if impossible:
            
            
            // check for DUPES:
            if (eventList.Count > 0) {  // if not the first selection:
                randType = (SimEventData.SimEventTypeMinor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventTypeMinor)).Length);
                // reroll isPositive?

                bool duplicateDetected = false;
                for(int j = 0; j < eventList.Count; j++) {
                    if(randType == eventList[j].typeMinor) {
                        //if(newEventData.isPositive == eventList[j].isPositive) {                            
                        duplicateDetected = true;                            
                        //}
                        //if(newEventData.polarity == eventList[j].polarity) {
                        //    duplicateDetected = true;
                        //}
                        break;
                    }
                }
                if(duplicateDetected) {
                    Debug.Log("Duplicate detected! iter: " + i.ToString());
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
        
        newEventData.typeMinor = randType;
        
        string qualifierTxt = "DECREASE";
        if(newEventData.isPositive) {
            qualifierTxt = "INCREASE";
        }
        
        switch(randType) {
            case SimEventData.SimEventTypeMinor.BodyModules:
                //
                if (newEventData.isPositive) {
                    newEventData.name = "Differentiate Senses I";
                    newEventData.description = " Slightly " + qualifierTxt + " the chance of altering the creatures' sensory capabilities";       
                }
                else {
                    newEventData.name = "Settle Senses I"; 
                    newEventData.description = " Slightly " + qualifierTxt + " the chance of altering creature sensors";       
                }       
                break;
            case SimEventData.SimEventTypeMinor.BodyMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Tweak Proportions";
                    newEventData.description = " Slightly INCREASE the global mutation rate for creature bodies";         
                }
                else {
                    newEventData.name = "Stabilize Forms"; 
                    newEventData.description = " Slightly DECREASE the global mutation rate for creature bodies";       
                }           
                break;
            case SimEventData.SimEventTypeMinor.BrainMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Neural Plasticity I";
                    newEventData.description = " Slightly " + qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                else {
                    newEventData.name = "Harden Axons I"; 
                    newEventData.description = " Slightly " + qualifierTxt + " the chance of rewiring creature brain"; 
                }
                break;
            case SimEventData.SimEventTypeMinor.BrainSize:
                if (newEventData.isPositive) {
                    newEventData.name = "Inflate Brain I";
                    newEventData.description = "Slightly INCREASE the chance for new Neurons and connections between them within creatures' brains";  
                }
                else {
                    newEventData.name = "Shrink Brain I"; 
                    newEventData.description = "Slightly DECREASE the chance for new Neurons and connections between them within creatures' brains";  
                }                
                break;
            case SimEventData.SimEventTypeMinor.CalmWaters:
                
                newEventData.name = "Calm Waters";                 
                newEventData.description = "Set the speed of water currents to a tranquil state";  
                break;
            case SimEventData.SimEventTypeMinor.CreateSpecies:                
                newEventData.speciesQualifier = SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events
                newEventData.name = "New Random Species";
                newEventData.description = "A new species emerges, originating from a random current creature";
                break;

            case SimEventData.SimEventTypeMinor.FoodCorpse:
                if (newEventData.isPositive) {
                    newEventData.name = "Slow Decomposition I";
                    newEventData.description = "Slow the rate at which dead creatures' bodies break down into nutrients";  
                }
                else {
                    newEventData.name = "Recycled Material I"; 
                    newEventData.description = "Slow the rate at which dead creatures' bodies are broken down into their component parts";  
                }
                //newEventData.name = "Detritus";
                //newEventData.description = qualifierTxt + " the global levels of decayed organic matter";  
                break;
            case SimEventData.SimEventTypeMinor.FoodEgg:
                if (newEventData.isPositive) {
                    newEventData.name = "Fertility I";
                    newEventData.description = "INCREASE the global frequency of egg-laying and increases durability of egg sacks";  
                }
                else {
                    newEventData.name = "Barren I"; 
                    newEventData.description = "DECREASE the global frequency of egg-laying and lower durability of egg sacks";   
                }
                break;
            case SimEventData.SimEventTypeMinor.FoodDecay:
                if (newEventData.isPositive) {
                    newEventData.name = "Algae Bloom I";
                    newEventData.description = "INCREASE the global levels of basic nutrients";  
                }
                else {
                    newEventData.name = "Nutrient Shortage I"; 
                    newEventData.description = "DECREASE the global levels of basic nutrients";   
                }
                break;
            case SimEventData.SimEventTypeMinor.FoodPlant:
                if (newEventData.isPositive) {
                    newEventData.name = "Blossom I";
                    newEventData.description = "INCREASE the global levels of plants";  
                }
                else {
                    newEventData.name = "Wilt I"; 
                    newEventData.description = "DECREASE the global levels of plants";   
                }
                break;
            case SimEventData.SimEventTypeMinor.KillSpecies:
                newEventData.speciesQualifier = SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events
                newEventData.name = "Kill Random Species";
                newEventData.description = "A species chosen at random goes extinct";                                
                break;            
            default:

                break;
        }

        //GetRandomSimEventType(ref newEventData);

        return newEventData;
    }

    public SimEventData GenerateNewRandomMajorEvent(List<SimEventData> eventList) {
        SimEventData newEventData = new SimEventData();
        newEventData.cost = 15;
        newEventData.category = SimEventData.SimEventCategories.Major;
        newEventData.quantity = 1; // needed??
        
        newEventData.isPositive = true;
        newEventData.polarity = true;
        float randPolarity = UnityEngine.Random.Range(0f, 1f);
        if(randPolarity < 0.5f) {
            newEventData.polarity = false;
        }
        float randSign = UnityEngine.Random.Range(0f, 1f);
        if(randSign < 0.5f) {
            newEventData.isPositive = false;
        }

        // Avoid Duplicates????
        SimEventData.SimEventTypeMajor randType = (SimEventData.SimEventTypeMajor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventTypeMajor)).Length);
        for(int i = 0; i < 8; i++) {
            
            // Check if impossible:
            
            
            // check for DUPES:
            if (eventList.Count > 0) {  // if not the first selection:
                randType = (SimEventData.SimEventTypeMajor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventTypeMajor)).Length);
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
                    Debug.Log("Duplicate detected! iter: " + i.ToString());
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

        SimEventData.SpeciesQualifier randQualifier = (SimEventData.SpeciesQualifier)UnityEngine.Random.Range(1, 4);
        
        switch(randType) {
            case SimEventData.SimEventTypeMajor.BodyModules:
                //
                if (newEventData.isPositive) {
                    newEventData.name = "Differentiate Senses II";
                    newEventData.description = " Moderately " + qualifierTxt + " the chance of altering the creatures' sensory capabilities";       
                }
                else {
                    newEventData.name = "Settle Senses II"; 
                    newEventData.description = " Moderately " + qualifierTxt + " the chance of altering creature sensors";       
                }       
                break;
            case SimEventData.SimEventTypeMajor.BodyMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Morph Proportions";
                    newEventData.description = " Moderately INCREASE the global mutation rate for creature bodies";         
                }
                else {
                    newEventData.name = "Stabilize Forms"; 
                    newEventData.description = " Moderately DECREASE the global mutation rate for creature bodies";       
                }           
                break;
            case SimEventData.SimEventTypeMajor.BrainMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Neural Plasticity II";
                    newEventData.description = " Moderately " + qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                else {
                    newEventData.name = "Harden Axons II"; 
                    newEventData.description = " Moderately " + qualifierTxt + " the chance of rewiring creature brain"; 
                }
                break;
            case SimEventData.SimEventTypeMajor.BrainSize:
                if (newEventData.isPositive) {
                    newEventData.name = "Inflate Brain II";
                    newEventData.description = "Moderately INCREASE the chance for new Neurons and connections between them within creatures' brains";  
                }
                else {
                    newEventData.name = "Shrink Brain II"; 
                    newEventData.description = "Moderately DECREASE the chance for new Neurons and connections between them within creatures' brains";  
                }                
                break;
            case SimEventData.SimEventTypeMajor.Gale:
                
                newEventData.name = "Gale";                 
                newEventData.description = "Set the speed of water currents to a steady flow";  
                break;
            case SimEventData.SimEventTypeMajor.CreateSpecies:                
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                string qualityString = "";
                if(randQualifier == SimEventData.SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "New Species\n(" + qualityString.ToString() + ")";
                newEventData.description = "A new lineage emerges, originating from the current " + qualityString + " species";
                break;

            case SimEventData.SimEventTypeMajor.FoodCorpse:
                if (newEventData.isPositive) {
                    newEventData.name = "Slow Decomposition II";
                    newEventData.description = "Slow the rate at which dead creatures' bodies break down into nutrients";  
                }
                else {
                    newEventData.name = "Recycled Material II"; 
                    newEventData.description = "Increase the rate at which dead creatures' bodies are broken down into their component parts";  
                } 
                break;
            case SimEventData.SimEventTypeMajor.FoodEgg:
                if (newEventData.isPositive) {
                    newEventData.name = "Fertility II";
                    newEventData.description = "Moderately INCREASE the global frequency of egg-laying and increases durability of egg sacks";  
                }
                else {
                    newEventData.name = "Barren II"; 
                    newEventData.description = "Moderately DECREASE the global frequency of egg-laying and lower durability of egg sacks";   
                }
                break;
            case SimEventData.SimEventTypeMajor.FoodDecay:
                if (newEventData.isPositive) {
                    newEventData.name = "Algae Bloom II";
                    newEventData.description = "Moderately INCREASE the global levels of basic nutrients";  
                }
                else {
                    newEventData.name = "Nutrient Shortage II"; 
                    newEventData.description = "Moderately DECREASE the global levels of basic nutrients";   
                }
                break;
            case SimEventData.SimEventTypeMajor.FoodPlant:
                if (newEventData.isPositive) {
                    newEventData.name = "Blossom II";
                    newEventData.description = "Moderately INCREASE the global levels of plants";  
                }
                else {
                    newEventData.name = "Wilt II"; 
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
            case SimEventData.SimEventTypeMajor.KillSpecies:
                
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                qualityString = "";
                if(randQualifier == SimEventData.SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "Kill " + qualityString.ToString() + " Species";
                newEventData.description = "The current " + qualityString + " species goes extinct";
                break;            
            default:

                break;
        }

        return newEventData;
    }

    public SimEventData GenerateNewRandomExtremeEvent(List<SimEventData> eventList) {
        SimEventData newEventData = new SimEventData();
        newEventData.cost = 75;
        newEventData.category = SimEventData.SimEventCategories.Extreme;
        newEventData.quantity = 3; // needed??
       
        newEventData.isPositive = true;
        newEventData.polarity = true;
        float randPolarity = UnityEngine.Random.Range(0f, 1f);
        if(randPolarity < 0.5f) {
            newEventData.polarity = false;
        }
        float randSign = UnityEngine.Random.Range(0f, 1f);
        if(randSign < 0.5f) {
            newEventData.isPositive = false;
        }

        // Avoid Duplicates????
        SimEventData.SimEventTypeExtreme randType = (SimEventData.SimEventTypeExtreme)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventTypeExtreme)).Length);
        for(int i = 0; i < 8; i++) {
            
            // Check if impossible:
            
            
            // check for DUPES:
            if (eventList.Count > 0) {  // if not the first selection:
                randType = (SimEventData.SimEventTypeExtreme)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SimEventData.SimEventTypeExtreme)).Length);
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
                    Debug.Log("Duplicate detected! iter: " + i.ToString());
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

        SimEventData.SpeciesQualifier randQualifier = (SimEventData.SpeciesQualifier)UnityEngine.Random.Range(1, 4);
        
        switch(randType) {
            case SimEventData.SimEventTypeExtreme.BodyModules:
                //
                if (newEventData.isPositive) {
                    newEventData.name = "Differentiate Senses III";
                    newEventData.description = " Greatly " + qualifierTxt + " the chance of altering the creatures' sensory capabilities";       
                }
                else {
                    newEventData.name = "Settle Senses III"; 
                    newEventData.description = " Greatly " + qualifierTxt + " the chance of altering creature sensors";       
                }       
                break;
            case SimEventData.SimEventTypeExtreme.BodyMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Exaggerate Proportions";
                    newEventData.description = " Greatly INCREASE the global mutation rate for creature bodies";         
                }
                else {
                    newEventData.name = "Preserve Forms"; 
                    newEventData.description = " Greatly DECREASE the global mutation rate for creature bodies";       
                }           
                break;
            case SimEventData.SimEventTypeExtreme.BrainMutation:
                if (newEventData.isPositive) {
                    newEventData.name = "Neural Plasticity III";
                    newEventData.description = " Greatly " + qualifierTxt + " the mutation rate for creature brain wirings"; 
                }
                else {
                    newEventData.name = "Harden Axons III"; 
                    newEventData.description = " Greatly " + qualifierTxt + " the chance of rewiring creature brain"; 
                }
                break;
            case SimEventData.SimEventTypeExtreme.BrainSize:
                if (newEventData.isPositive) {
                    newEventData.name = "Inflate Brain III";
                    newEventData.description = "Greatly INCREASE the chance for new Neurons and connections between them within creatures' brains";  
                }
                else {
                    newEventData.name = "Shrink Brain III"; 
                    newEventData.description = "Greatly DECREASE the chance for new Neurons and connections between them within creatures' brains";  
                }                
                break;
            case SimEventData.SimEventTypeExtreme.Hurricane:                
                newEventData.name = "Maelstrom";                 
                newEventData.description = "Greatly increase the magnitude and turbulence of the water's currents";  
                break;
            case SimEventData.SimEventTypeExtreme.CreateSpecies:                
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                string qualityString = "";
                if(randQualifier == SimEventData.SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Novelty) {
                    qualityString = "Most Average";
                    if(newEventData.polarity) {
                        qualityString = "Most Unique";
                    }
                }
                newEventData.name = "New Species\n(" + qualityString.ToString() + ")";
                newEventData.description = "Up to " + newEventData.quantity.ToString() + " new lineages emerge, originating from the current " + qualityString + " species";
                break;

            case SimEventData.SimEventTypeExtreme.FoodCorpse:
                if (newEventData.isPositive) {
                    newEventData.name = "Slow Decomposition III";
                    newEventData.description = "Slow the rate at which dead creatures' bodies break down into nutrients";  
                }
                else {
                    newEventData.name = "Recycled Material III"; 
                    newEventData.description = "Increase the rate at which dead creatures' bodies are broken down into their component parts";  
                } 
                break;
            case SimEventData.SimEventTypeExtreme.FoodEgg:
                if (newEventData.isPositive) {
                    newEventData.name = "Fertility III";
                    newEventData.description = "Greatly INCREASE the global frequency of egg-laying and increases durability of egg sacks";  
                }
                else {
                    newEventData.name = "Barren III"; 
                    newEventData.description = "Greatly DECREASE the global frequency of egg-laying and lower durability of egg sacks";   
                }
                break;
            case SimEventData.SimEventTypeExtreme.FoodDecay:
                if (newEventData.isPositive) {
                    newEventData.name = "Algae Bloom III";
                    newEventData.description = "Greatly INCREASE the global levels of basic nutrients";  
                }
                else {
                    newEventData.name = "Nutrient Shortage III"; 
                    newEventData.description = "Greatly DECREASE the global levels of basic nutrients";   
                }
                break;
            case SimEventData.SimEventTypeExtreme.FoodPlant:
                if (newEventData.isPositive) {
                    newEventData.name = "Blossom III";
                    newEventData.description = "Greatly INCREASE the global levels of plants";  
                }
                else {
                    newEventData.name = "Wilt III"; 
                    newEventData.description = "Greatly DECREASE the global levels of plants";   
                }
                break;
            case SimEventData.SimEventTypeExtreme.FoodAll:
                if (newEventData.isPositive) {
                    newEventData.name = "Feast";
                    newEventData.description = "Greatly INCREASE the global levels of all food types";  
                }
                else {
                    newEventData.name = "Famine"; 
                    newEventData.description = "Greatly DECREASE the global levels of all food types";   
                }
                break;
            case SimEventData.SimEventTypeExtreme.KillSpecies:
                
                newEventData.speciesQualifier = randQualifier; // SimEventData.SpeciesQualifier.Random;  // only random allowed for minor events                
                qualityString = "";
                if(randQualifier == SimEventData.SpeciesQualifier.Age) {
                    qualityString = "Youngest";
                    if(newEventData.polarity) {
                        qualityString = "Oldest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.BodySize) {
                    qualityString = "Smallest";
                    if(newEventData.polarity) {
                        qualityString = "Largest";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Fitness) {
                    qualityString = "Least Fit";
                    if(newEventData.polarity) {
                        qualityString = "Most Fit";
                    }
                }
                else if(randQualifier == SimEventData.SpeciesQualifier.Novelty) {
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

    public void RegenerateAvailableMinorEvents(SimulationManager simManager) {
        availableMinorEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomMinorEvent(availableMinorEventsList); // new SimEventData();            
            availableMinorEventsList.Add(newEventData);
        }
    }

    public void RegenerateAvailableMajorEvents(SimulationManager simManager) {
        availableMajorEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomMajorEvent(availableMajorEventsList); // new SimEventData();            
            availableMajorEventsList.Add(newEventData);
        }
    }

    public void RegenerateAvailableExtremeEvents(SimulationManager simManager) {
        availableExtremeEventsList.Clear();

        for(int i = 0; i < numAvailableEventsPerMenu; i++) {
            SimEventData newEventData = GenerateNewRandomExtremeEvent(availableExtremeEventsList); // new SimEventData();            
            availableExtremeEventsList.Add(newEventData);
        }
    }

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
}
