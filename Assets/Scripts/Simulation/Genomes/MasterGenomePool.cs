﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Playcraft;
using Random = UnityEngine.Random;

[Serializable]
public class MasterGenomePool 
{
    UIManager uiManager => UIManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;
    SimulationManager simulation => SimulationManager.instance;

    public static int nextCandidateIndex = 0;

    public int maxNumActiveSpecies = 6;
    private int targetNumSpecies = 4;
    public float speciesSimilarityDistanceThreshold = 16f;
    private int minNumGuaranteedEvalsForNewSpecies = 128;
    int numInitSpecies = 1;

    [SerializeField] float oldestSpeciesRerollChance = 0.33f;

    public int curNumAliveAgents;

    public int currentHighestDepth = 1;
    
    public List<int> currentlyActiveSpeciesIDList;
    public List<SpeciesGenomePool> completeSpeciesPoolsList;

    public bool speciesCreatedOrDestroyedThisFrame = false;

    MutationSettingsInstance mutationSettings;

    public List<int> debugRecentlyDeletedCandidateIDsList;

    PanelNotificationsUI panelPendingClickPrompt => uiManager.panelNotificationsUI;
    public SpeciesGenomePool selectedPool => completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID];
    public int speciesPoolCount => completeSpeciesPoolsList.Count;   
   
    // WPP: empty constructor is redundant, compiler creates this by default
    //public MasterGenomePool() { }

    public void FirstTimeInitialize(MutationSettingsInstance mutationSettings) {
        debugRecentlyDeletedCandidateIDsList = new List<int>();

        nextCandidateIndex = 0;
        this.mutationSettings = mutationSettings;
        currentlyActiveSpeciesIDList = new List<int>();
        completeSpeciesPoolsList = new List<SpeciesGenomePool>();

        //SpeciesGenomePool rootSpecies = new SpeciesGenomePool(0, -1, 0, 0, mutationSettingsRef);
        //rootSpecies.FirstTimeInitializeROOT(numAgentGenomes, 0);
        //currentlyActiveSpeciesIDList.Add(0);
        //completeSpeciesPoolsList.Add(rootSpecies);

        for (int i = 0; i < numInitSpecies; i++) {
            //float lerpV = Mathf.Clamp01((i + 0.1f) / (numInitSpecies + 1) + 0.06f) * 0.8f + 0.1f;
            //float lerpV = 0.5f;
            
            SpeciesGenomePool newSpecies = new SpeciesGenomePool(i, -1, 0, 0, mutationSettings);
            AgentGenome seedGenome = new AgentGenome(mutationSettings.brainInitialConnectionChance, simulation.numInitialHiddenNeurons);

            newSpecies.FirstTimeInitialize(new CandidateAgentData(seedGenome, i), 0);
            currentlyActiveSpeciesIDList.Add(i);
            completeSpeciesPoolsList.Add(newSpecies);
        }

        selectionManager.SetSelected(completeSpeciesPoolsList[0].candidateGenomesList[0]);
    }

    public void AddNewYearlySpeciesStats(int year) {
        foreach (var speciesPool in completeSpeciesPoolsList) {
            speciesPool.AddNewYearlyStats(year);
        }
    }        

    /// Keep track of when species created so can't create multiple per frame?
    public void Tick() {
        speciesCreatedOrDestroyedThisFrame = false;
    }

    private void CheckForExtinction() {
        if (currentlyActiveSpeciesIDList.Count <= targetNumSpecies)
            return; 
        
        int leastFitSpeciesID = -1;
        float worstFitness = Mathf.Infinity;
        bool noCurrentlyFlaggedSpecies = true;
        
        foreach (var idList in currentlyActiveSpeciesIDList) {
            //Debug.Log(idList + ", " + currentlyActiveSpeciesIDList[idList] + ", " + completeSpeciesPoolsList.Count);
            SpeciesGenomePool pool = completeSpeciesPoolsList[idList];
            float fitness = pool.avgCandidateData.performanceData.totalTicksAlive;
            if (fitness < worstFitness) {
                worstFitness = fitness;
                leastFitSpeciesID = idList;
                
            }
            if (pool.isFlaggedForExtinction) {
                noCurrentlyFlaggedSpecies = false;
            }
        }
        
        if (noCurrentlyFlaggedSpecies) {
            if (completeSpeciesPoolsList[leastFitSpeciesID].numAgentsEvaluated > minNumGuaranteedEvalsForNewSpecies) {
                // OK to KILL!!!
                FlagSpeciesExtinct(leastFitSpeciesID);
                string readout = "FLAG\n";
                foreach(int id in currentlyActiveSpeciesIDList) {
                    if(id == leastFitSpeciesID) {
                        readout += "EXTINCT ";
                    }
                    readout += "Species " + id + ", fitness: " + completeSpeciesPoolsList[id].avgCandidateData.performanceData.totalTicksAlive + ", " + completeSpeciesPoolsList[id].candidateGenomesList.Count + " Cands\n";
                }
                Debug.Log(readout);
            }
        }
        else {
            foreach (var idList in currentlyActiveSpeciesIDList) {
                SpeciesGenomePool pool = completeSpeciesPoolsList[idList];
                if(pool.isFlaggedForExtinction) {
                    float fitness = pool.avgCandidateData.performanceData.totalTicksAlive;
                    if (pool.candidateGenomesList.Count < 1) {
                        ExtinctifySpecies(idList);

                        Debug.Log("EXTINCTIFY " + idList + ", fitness: " + fitness + ", curWorst: " + leastFitSpeciesID + " (" + worstFitness);
                        string readout = "";
                        foreach (int id in currentlyActiveSpeciesIDList) {
                            if (id == leastFitSpeciesID) {
                                readout += "[LEAST FIT] ";
                            }
                            readout += "Species " + id + ", fitness: " + completeSpeciesPoolsList[id].avgCandidateData.performanceData.totalTicksAlive + ", " + completeSpeciesPoolsList[id].candidateGenomesList.Count + " Cands\n";
                        }
                        Debug.Log(readout);
                        return;
                    }
                }
            }
        }
    }

    public void FlagSpeciesExtinct(int speciesID) {
        completeSpeciesPoolsList[speciesID].isFlaggedForExtinction = true;
        //Debug.Log("FLAG EXTINCT: " + speciesID);
    }
    
    public void ExtinctifySpecies(int speciesID) {
        //Debug.Log("REMOVE SPECIES " + speciesID.ToString());

        // find and remove from active list:
        int listIndex = -1;
        for (int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(speciesID == completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].speciesID) {
                listIndex = i;
            }
        }

        currentlyActiveSpeciesIDList.RemoveAt(listIndex);
        completeSpeciesPoolsList[speciesID].ProcessExtinction(simulation.simAgeTimeSteps);

    }

    public SpeciesGenomePool SelectNewGenomeSourceSpecies(bool weighted, float weightedAmount) {
        if(weighted) 
        {
            // Filter Out species which are flagged for extinction?!?!?!
            
            // figure out which species has most evals
            int totalNumActiveEvals = 0;
            //int evalLeaderActiveIndex = 0;
            int recordNumEvals = 0;            
            foreach (var idList in currentlyActiveSpeciesIDList) {
                int numEvals = completeSpeciesPoolsList[idList].numAgentsEvaluated + 1;
                totalNumActiveEvals += numEvals;
                if (numEvals > recordNumEvals) {
                    recordNumEvals = numEvals;
                    //evalLeaderActiveIndex = i;
                }
            }

            float[] unsortedEvalScoresArray = new float[currentlyActiveSpeciesIDList.Count];
            float[] rankedEvalScoresArray = new float[currentlyActiveSpeciesIDList.Count];
            int[] rankedEvalIndices = new int[currentlyActiveSpeciesIDList.Count];

            //float weightedAmount = 0.5f;
            float avgFractionVal = 1f / currentlyActiveSpeciesIDList.Count;

            for (int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
                float rawEvalFraction = 1f - (((float)completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].numAgentsEvaluated + 1f) / (float)totalNumActiveEvals);
                unsortedEvalScoresArray[i] = Mathf.Lerp(avgFractionVal, rawEvalFraction, weightedAmount);
                rankedEvalScoresArray[i] = unsortedEvalScoresArray[i];
                rankedEvalIndices[i] = i;
            }

            // SORT ARRAY BY #EVALS:
            for (int i = 0; i < rankedEvalIndices.Length - 1; i++) {
                for (int j = 0; j < rankedEvalIndices.Length - 1; j++) {
                    float swapFitA = rankedEvalScoresArray[j];
                    float swapFitB = rankedEvalScoresArray[j + 1];
                    int swapIdA = rankedEvalIndices[j];
                    int swapIdB = rankedEvalIndices[j + 1];
                    
                    // bigger is better now after inversion
                    if (swapFitA < swapFitB) {  
                        rankedEvalScoresArray[j] = swapFitB;
                        rankedEvalScoresArray[j + 1] = swapFitA;
                        rankedEvalIndices[j] = swapIdB;
                        rankedEvalIndices[j + 1] = swapIdA;
                    }
                }
            }

            int selectedIndex = 0;
            // generate random lottery value between 0f and totalFitness:
            float lotteryValue = Random.Range(0f, 1f);
            float currentValue = 0f;
            for (int i = 0; i < rankedEvalIndices.Length; i++) {
                if (lotteryValue >= currentValue && lotteryValue < (currentValue + rankedEvalScoresArray[i])) {
                    // Jackpot!
                    selectedIndex = rankedEvalIndices[i];
                    //Debug.Log("Selected: " + selectedIndex.ToString() + "! (" + i.ToString() + ") fit= " + currentValue.ToString() + "--" + (currentValue + (1f - rankedFitnessList[i])).ToString() + " / " + totalFitness.ToString() + ", lotto# " + lotteryValue.ToString() + ", fit= " + (1f - rankedFitnessList[i]).ToString());
                }
                currentValue += rankedEvalIndices[i]; // add this agent's fitness to current value for next check
            }

            return completeSpeciesPoolsList[currentlyActiveSpeciesIDList[selectedIndex]];        
        }

        // NAIVE RANDOM AT FIRST:
        // filter flagged extinct species:
        List<int> eligibleSpeciesIDList = new List<int>();
        foreach (var id in currentlyActiveSpeciesIDList)
            if (!completeSpeciesPoolsList[id].isFlaggedForExtinction) 
                eligibleSpeciesIDList.Add(id);

        int randomTableIndex = Random.Range(0, eligibleSpeciesIDList.Count);

        // temp minor penalty to oldest species:
        if (randomTableIndex == 0 && RandomStatics.CoinToss(oldestSpeciesRerollChance)) {
            randomTableIndex = Random.Range(0, eligibleSpeciesIDList.Count);
        }
        int speciesIndex = eligibleSpeciesIDList[randomTableIndex];

        return completeSpeciesPoolsList[speciesIndex];
    }
    
    public SpeciesGenomePool GetSmallestSpecies() 
    {
        int targetSpeciesID = 0;
        // filter flagged extinct species:
        List<int> eligibleSpeciesIDList = new List<int>();
        
        foreach (var id in currentlyActiveSpeciesIDList) {
            if (!completeSpeciesPoolsList[id].isFlaggedForExtinction && !completeSpeciesPoolsList[id].isExtinct) {
                eligibleSpeciesIDList.Add(id);
                
                if(completeSpeciesPoolsList[id].candidateGenomesList.Count < completeSpeciesPoolsList[targetSpeciesID].candidateGenomesList.Count) {
                    targetSpeciesID = id;
                }
            }
        }  
              
        return completeSpeciesPoolsList[targetSpeciesID];
    }
    
    // *** Gross code organization btw this and SimManager ***
    public void AssignNewMutatedGenomeToSpecies(AgentGenome newGenome, int parentSpeciesID) {
        int closestSpeciesID = -1;
        float closestDistance = 99999f;
        
        foreach (var id in currentlyActiveSpeciesIDList) {
            // Dying species not allowed?
            if (completeSpeciesPoolsList[id].isFlaggedForExtinction) 
                continue;
             
            float similarityDistance = GetSimilarityScore(newGenome, completeSpeciesPoolsList[id].representativeCandidate.candidateGenome);
            if (similarityDistance >= closestDistance) 
                continue;
            
            closestDistance = similarityDistance;
            closestSpeciesID = id;
        }
        
        // CHECK IF NEW SPECIES CREATED:
        bool assignedToNewSpecies = false;
            
        if (closestDistance > speciesSimilarityDistanceThreshold && !speciesCreatedOrDestroyedThisFrame && currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) 
        {
            // Create new species!::::     
            assignedToNewSpecies = true;

            int seedSpeciesID = closestSpeciesID; // simManagerRef.masterGenomePool.currentlyActiveSpeciesIDList[ UnityEngine.Random.Range(0, currentlyActiveSpeciesIDList.Count) ];
            //AgentGenome seedGenome =
            simulation.AddNewSpecies(newGenome, seedSpeciesID);

            speciesSimilarityDistanceThreshold += 70f;

            Color color = new Color(newGenome.bodyGenome.appearanceGenome.huePrimary.x, newGenome.bodyGenome.appearanceGenome.huePrimary.y, newGenome.bodyGenome.appearanceGenome.huePrimary.z);
            panelPendingClickPrompt.Narrate("A new species has emerged! agentname: " + newGenome.name, color);
        }

        if (!assignedToNewSpecies) 
        {
            // *** maybe something fishy here??
            // **********************

            // Allowing new creatures to be assigned to species other than its parent :
            //    -- In theory should keep species closer to its Representative Genome?
            //    -- causes issues with 0-candidate species....

            // The alternative allows species to drift further away from the founding Representative genome???
            // So far avg. fitness not improving at same rate as before????
            // ****************
            // could try going for a hybrid approach?


            // NEW:::
            completeSpeciesPoolsList[parentSpeciesID].AddNewCandidateGenome(newGenome);
            // OLD:::
            //completeSpeciesPoolsList[closestSpeciesID].AddNewCandidateGenome(newGenome);
        }
        else 
        {
            // *** ???
            Debug.Log($"assignedToNewSpecies closestDistanceSpeciesID: {closestSpeciesID}, score: {closestDistance}");
            completeSpeciesPoolsList[closestSpeciesID].AddNewCandidateGenome(newGenome);
        }
        
        if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
            speciesSimilarityDistanceThreshold *= 0.97f;  // lower while creating treeOfLifeUI
        }
        else {
            speciesSimilarityDistanceThreshold *= 1.05f;
            speciesSimilarityDistanceThreshold = Mathf.Min(speciesSimilarityDistanceThreshold, 6f); // cap
        }

        CheckForExtinction();
    }

    public void GlobalFindCandidateID(int ID) 
    {
        int foundSpeciesID = -1;
        bool foundCandidate = false;

        foreach (var speciesID in currentlyActiveSpeciesIDList) 
        {
            foreach (var candidate in completeSpeciesPoolsList[speciesID].candidateGenomesList) 
            {
                if (candidate.candidateID != ID) 
                    continue; 
                    
                foundCandidate = true;
                foundSpeciesID = completeSpeciesPoolsList[speciesID].speciesID;
                Debug.Log("FOUND! " + ID + ", species: " + foundSpeciesID + ", CDSID: " + candidate.candidateID);
            }
        } 
        
        if (!foundCandidate) 
        {
            // look through ALL species?
            foreach (var pool in completeSpeciesPoolsList) 
            {
                foreach (var candidate in pool.candidateGenomesList) 
                {
                    if (candidate.candidateID == ID) 
                    {
                        foundCandidate = true;
                        foundSpeciesID = pool.speciesID;
                        Debug.Log($"FOUND COMPLETE! {ID}, species: {foundSpeciesID}, CDSID: {candidate.candidateID}");
                    }
                }                
            }
        }

        if (!foundCandidate) {
            //for (int k = 0; k < debugRecentlyDeletedCandidateIDsList.Count; k++) 
            foreach (var id in debugRecentlyDeletedCandidateIDsList) {
                if (id != ID) continue;
                
                foundCandidate = true;
                //foundSpeciesID = completeSpeciesPoolsList[a].speciesID;
                // It was somehow deleted twice?  Or simply looking in the wrong SpeciesPool?
                Debug.Log("FOUND COMPLETE! " + ID + ", CDSID: " + id);
            }
        }
    }
     
    // UPGRADE / OVERHAUL!!!!  ****** 
    // *** need to normalize all to 0-1 for more fair comparison? ***
    // have centralized min/max values for each attribute?
    public float GetSimilarityScore(AgentGenome newGenome, AgentGenome repGenome) 
    {
        var newBody = newGenome.bodyGenome;
        var newCore = newBody.coreGenome;
        var newAppearance = newBody.appearanceGenome;

        var repBody = repGenome.bodyGenome;
        var repCore = repBody.coreGenome;
        var repAppearance = repBody.appearanceGenome;

        float dBaseSize = Mathf.Abs(newCore.creatureBaseLength - repCore.creatureBaseLength);
        float dBaseAspectRatio = Mathf.Abs(newCore.creatureAspectRatio - repCore.creatureAspectRatio);

        float mouthA = newCore.isPassive ? 0f : 1f;
        float mouthB = repCore.isPassive ? 0f : 1f;

        float dMouthType = Mathf.Abs(mouthA - mouthB);

        float dPriColor = Mathf.Abs((newAppearance.huePrimary - repAppearance.huePrimary).sqrMagnitude);
        float dSecColor = Mathf.Abs((newAppearance.hueSecondary - repAppearance.hueSecondary).sqrMagnitude);

        // proportions:
        float dMouthLength = Mathf.Abs(newCore.mouthLength - repCore.mouthLength);
        float dHeadLength = Mathf.Abs(newCore.headLength - repCore.headLength);
        float dBodyLength = Mathf.Abs(newCore.bodyLength - repCore.bodyLength);
        float dTailLength = Mathf.Abs(newCore.tailLength - repCore.tailLength);
        
        // eyes
        float dEyeSize = Mathf.Abs(newCore.socketRadius - repCore.socketRadius);
        float dEyeHeight = Mathf.Abs(newCore.socketHeight - repCore.socketHeight);

        // tail
        float dTailFinLength = Mathf.Abs(newCore.tailFinBaseLength - repCore.tailFinBaseLength);
        float dTailFinSpread = Mathf.Abs(newCore.tailFinSpreadAngle - repCore.tailFinSpreadAngle);

        // sensors:
        float dFoodSpecDecay = Mathf.Abs(newCore.dietSpecializationDecay - repCore.dietSpecializationDecay);
        float dFoodSpecPlant = Mathf.Abs(newCore.dietSpecializationPlant - repCore.dietSpecializationPlant);
        float dFoodSpecMeat = Mathf.Abs(newCore.dietSpecializationMeat - repCore.dietSpecializationMeat);
                
        float dUseComms = newBody.data.hasComms == repBody.data.hasComms ? 0f : 1f;
        float dUseWaterStats = newBody.data.useWaterStats == repBody.data.useWaterStats ? 0f : 1f;
        float dUseCardinals = newBody.data.useCardinals == repBody.data.useCardinals ? 0f : 1f;
        float dUseDiagonals = newBody.data.useDiagonals == repBody.data.useDiagonals ? 0f : 1f;
        float dUseNutrients = newBody.data.useNutrients == repBody.data.useNutrients ? 0f : 1f;
        float dUseFoodPos = newBody.data.useFoodPosition == repBody.data.useFoodPosition ? 0f : 1f;
        float dUseFoodVel = newBody.data.useFoodVelocity == repBody.data.useFoodVelocity ? 0f : 1f;
        float dUseFoodDir = newBody.data.useFoodDirection == repBody.data.useFoodDirection ? 0f : 1f;
        float dUseFoodStats = newBody.data.useFoodStats == repBody.data.useFoodStats ? 0f : 1f;
        float dUseFoodEgg = newBody.data.useEggs == repBody.data.useEggs ? 0f : 1f;
        float dUseFoodCorpse = newBody.data.useCorpse == repBody.data.useCorpse ? 0f : 1f;
        
        var dHasAnimalSensor = newBody.data.hasAnimalSensor == repBody.data.hasAnimalSensor;
        float dUseFriendPos = dHasAnimalSensor ? 0f : 1f; // newFriend.usePos == repFriend.usePos
        float dUseFriendVel = dHasAnimalSensor ? 0f : 1f; // newFriend.useVel == repFriend.useVel
        float dUseFriendDir = dHasAnimalSensor ? 0f : 1f; // newFriend.useDir == repFriend.useDir
        float dUseThreatPos = dHasAnimalSensor ? 0f : 1f; // newThreat.usePos == repThreat.usePos
        float dUseThreatVel = dHasAnimalSensor ? 0f : 1f; // newThreat.useVel == repThreat.useVel
        float dUseThreatDir = dHasAnimalSensor ? 0f : 1f; // newThreat.useDir == repThreat.useDir
        float dUseThreatStats = dHasAnimalSensor ? 0f : 1f; // newThreat.useStats == repThreat.useStats

        float dSensory = dFoodSpecDecay + dFoodSpecPlant + dFoodSpecMeat + dUseComms + dUseWaterStats + dUseCardinals + dUseDiagonals +
                         dUseNutrients + dUseFoodPos + dUseFoodVel + dUseFoodDir + dUseFoodStats + dUseFoodEgg + dUseFoodCorpse + dUseFriendPos + dUseFriendVel + dUseFriendDir +
                         dUseThreatPos + dUseThreatVel + dUseThreatDir + dUseThreatStats;
        float dNeurons = Mathf.Abs(newGenome.brainGenome.neurons.hiddenCount - repGenome.brainGenome.neurons.hiddenCount);
        float dAxons = Mathf.Abs(newGenome.brainGenome.axonCount - repGenome.brainGenome.axonCount) / (float)repGenome.brainGenome.neurons.inOutCount;

        float dSpecialization = (new Vector4(newCore.talentSpecializationAttack, newCore.talentSpecializationDefense, newCore.talentSpecializationSpeed, newCore.talentSpecializationUtility).normalized - 
                                 new Vector4(repCore.talentSpecializationAttack, repCore.talentSpecializationDefense, repCore.talentSpecializationSpeed, repCore.talentSpecializationUtility).normalized).magnitude;

        dSensory *= 0.2f;
        
        float delta = dBaseSize * 2f + dBaseAspectRatio * 2f +
            dMouthType +
            dPriColor * 5f + dSecColor * 5f +
            dMouthLength + dHeadLength + dBodyLength + dTailLength +
            dEyeSize + dEyeHeight +
            dTailFinLength + dTailFinSpread +
            dSensory +
            dNeurons + dAxons +
            dSpecialization;

        //Debug.Log("Difference Score: " + delta.ToString());

        return delta;
    }
}
