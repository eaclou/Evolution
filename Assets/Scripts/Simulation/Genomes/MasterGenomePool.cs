using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MasterGenomePool {
    //public TreeOfLifeManager treeOfLifeManager;

    public static int nextCandidateIndex = 0;

    public int maxNumActiveSpecies = 8;
    private int targetNumSpecies = 3;
    public float speciesSimilarityDistanceThreshold = 12f;
    private int minNumGuaranteedEvalsForNewSpecies = 128;

    public int curNumAliveAgents;

    public int currentHighestDepth = 1;
    
    public List<int> currentlyActiveSpeciesIDList;
    public List<SpeciesGenomePool> completeSpeciesPoolsList;

    public bool speciesCreatedOrDestroyedThisFrame = false;

    public MutationSettings mutationSettingsRef;

    public List<int> debugRecentlyDeletedCandidateIDsList;

    UIManager uiManager => UIManager.instance;
    PanelNotificationsUI panelPendingClickPrompt => uiManager.panelNotificationsUI;
    public SpeciesGenomePool selectedPool => completeSpeciesPoolsList[uiManager.selectionManager.selectedSpeciesID];
    public int speciesPoolCount => completeSpeciesPoolsList.Count;   
   
    public MasterGenomePool() { }

    public void FirstTimeInitialize(MutationSettings mutationSettingsRef) {
        debugRecentlyDeletedCandidateIDsList = new List<int>();

        nextCandidateIndex = 0;
        this.mutationSettingsRef = mutationSettingsRef;
        currentlyActiveSpeciesIDList = new List<int>();
        completeSpeciesPoolsList = new List<SpeciesGenomePool>();

        //SpeciesGenomePool rootSpecies = new SpeciesGenomePool(0, -1, 0, 0, mutationSettingsRef);
        //rootSpecies.FirstTimeInitializeROOT(numAgentGenomes, 0);
        //currentlyActiveSpeciesIDList.Add(0);
        //completeSpeciesPoolsList.Add(rootSpecies);

        int numInitSpecies = 4;
        for(int i = 0; i < numInitSpecies; i++) {
            float lerpV = Mathf.Clamp01(((float)i + 0.1f) / (float)(numInitSpecies + 1) + 0.06f) * 0.8f + 0.1f;
            lerpV = 0.5f;
            SpeciesGenomePool newSpecies = new SpeciesGenomePool(i, -1, 0, 0, mutationSettingsRef);
            AgentGenome seedGenome = new AgentGenome();
            seedGenome.GenerateInitialRandomBodyGenome();
            int tempNumHiddenNeurons = Mathf.RoundToInt(20f * lerpV);
            seedGenome.InitializeRandomBrainFromCurrentBody(1.0f, mutationSettingsRef.brainInitialConnectionChance * lerpV, tempNumHiddenNeurons);            
            newSpecies.FirstTimeInitialize(new CandidateAgentData(seedGenome, i), 0);
            currentlyActiveSpeciesIDList.Add(i);
            completeSpeciesPoolsList.Add(newSpecies);
        }        
    }

    public void AddNewYearlySpeciesStats(int year) {
        for(int i = 0; i < completeSpeciesPoolsList.Count; i++) {
            completeSpeciesPoolsList[i].AddNewYearlyStats(year);
        }
    }        

    public void Tick() {
        speciesCreatedOrDestroyedThisFrame = false;
    }

    private void CheckForExtinction(SimulationManager simManagerRef) {
        if(currentlyActiveSpeciesIDList.Count > targetNumSpecies) {
            int leastFitSpeciesID = -1;
            float worstFitness = 99999f;
            bool noCurrentlyExtinctFlaggedSpecies = true;
            for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
                float fitness = completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].avgCandidateData.performanceData.totalTicksAlive;
                if(fitness < worstFitness) {
                    worstFitness = fitness;
                    leastFitSpeciesID = currentlyActiveSpeciesIDList[i];
                }
                if(completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].isFlaggedForExtinction) {
                    noCurrentlyExtinctFlaggedSpecies = false;

                    if(completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].candidateGenomesList.Count < 1) {
                        ExtinctifySpecies(simManagerRef, currentlyActiveSpeciesIDList[i]);
                    }
                }
            }

            if(noCurrentlyExtinctFlaggedSpecies) {
                if(completeSpeciesPoolsList[leastFitSpeciesID].numAgentsEvaluated > minNumGuaranteedEvalsForNewSpecies) {
                    // OK to KILL!!!
                    FlagSpeciesExtinct(leastFitSpeciesID);
                    //completeSpeciesPoolsList[leastFitSpeciesID].isFlaggedForExtinction = true;
                    //Debug.Log("FLAG EXTINCT: " + leastFitSpeciesID.ToString());
                }
            }
        }
    }

    public void FlagSpeciesExtinct(int speciesID) {
        completeSpeciesPoolsList[speciesID].isFlaggedForExtinction = true;
        Debug.Log("FLAG EXTINCT: " + speciesID.ToString());
    }
    
    public void ExtinctifySpecies(SimulationManager simManagerRef, int speciesID) {
        //Debug.Log("REMOVE SPECIES " + speciesID.ToString());

        // find and remove from active list:
        int listIndex = -1;
        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(speciesID == completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].speciesID) {
                listIndex = i;
            }
        }

        currentlyActiveSpeciesIDList.RemoveAt(listIndex);
        completeSpeciesPoolsList[speciesID].ProcessExtinction(simManagerRef.simAgeTimeSteps);

        //Signal RenderKing/TreeOfLifeManager to update:
        //simManagerRef.uiManager.treeOfLifeManager.RemoveExtinctSpecies(speciesID);
        //simManagerRef.theRenderKing.TreeOfLifeExtinctSpecies(speciesID);
    }

    public SpeciesGenomePool SelectNewGenomeSourceSpecies(bool weighted, float weightedAmount) {
        if(weighted) {
            // Filter Out species which are flagged for extinction?!?!?!
            
            // figure out which species has most evals
            int totalNumActiveEvals = 0;
            //int evalLeaderActiveIndex = 0;
            int recordNumEvals = 0;            
            for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
                int numEvals = completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].numAgentsEvaluated + 1;
                totalNumActiveEvals += numEvals;
                if(numEvals > recordNumEvals) {
                    recordNumEvals = numEvals;
                    //evalLeaderActiveIndex = i;
                }
            }

            float[] unsortedEvalScoresArray = new float[currentlyActiveSpeciesIDList.Count];
            float[] rankedEvalScoresArray = new float[currentlyActiveSpeciesIDList.Count];
            int[] rankedEvalIndices = new int[currentlyActiveSpeciesIDList.Count];

            //float weightedAmount = 0.5f;
            float avgFractionVal = 1f / currentlyActiveSpeciesIDList.Count;

            for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
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

                    if (swapFitA < swapFitB) {  // bigger is better now after inversion
                        rankedEvalScoresArray[j] = swapFitB;
                        rankedEvalScoresArray[j + 1] = swapFitA;
                        rankedEvalIndices[j] = swapIdB;
                        rankedEvalIndices[j + 1] = swapIdA;
                    }
                }
            }

            int selectedIndex = 0;
            // generate random lottery value between 0f and totalFitness:
            float lotteryValue = UnityEngine.Random.Range(0f, 1f);
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
        else {
            // NAIVE RANDOM AT FIRST:
            // filter flagged extinct species:
            List<int> eligibleSpeciesIDList = new List<int>();
            for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
                if(completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].isFlaggedForExtinction) {

                }
                else {
                    eligibleSpeciesIDList.Add(currentlyActiveSpeciesIDList[i]);
                }
            }
            //int randomTableIndex = UnityEngine.Random.Range(0, currentlyActiveSpeciesIDList.Count);
            int randomTableIndex = UnityEngine.Random.Range(0, eligibleSpeciesIDList.Count);

            // temp minor penalty to oldest species:
            float oldestSpeciesRerollChance = 0.33f;
            if(randomTableIndex == 0) {
                if(UnityEngine.Random.Range(0f, 1f) < oldestSpeciesRerollChance) {
                    randomTableIndex = UnityEngine.Random.Range(0, eligibleSpeciesIDList.Count);
                }
            }
            int speciesIndex = eligibleSpeciesIDList[randomTableIndex];

            return completeSpeciesPoolsList[speciesIndex];
        }
    }
    public SpeciesGenomePool GetSmallestSpecies() {

        int targetSpeciesID = 0;
        // filter flagged extinct species:
        List<int> eligibleSpeciesIDList = new List<int>();
        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].isFlaggedForExtinction || completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].isExtinct) {

            }
            else {
                eligibleSpeciesIDList.Add(currentlyActiveSpeciesIDList[i]);
                if(completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].candidateGenomesList.Count < completeSpeciesPoolsList[targetSpeciesID].candidateGenomesList.Count) {
                    targetSpeciesID = currentlyActiveSpeciesIDList[i];
                }
                
            }
        }        
        return completeSpeciesPoolsList[targetSpeciesID];
        
    }
    public void AssignNewMutatedGenomeToSpecies(AgentGenome newGenome, int parentSpeciesID, SimulationManager simManagerRef) {
        // *** Gross code organization btw this and SimManager ***
        int closestSpeciesID = -1;

        float closestDistance = 99999f;
        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(!completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].isFlaggedForExtinction) {  // Dying species not allowed?
                float similarityDistance = GetSimilarityScore(newGenome, completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].representativeCandidate.candidateGenome);
                if(similarityDistance < closestDistance) {
                    closestDistance = similarityDistance;
                    closestSpeciesID = currentlyActiveSpeciesIDList[i];
                }
            }            
        }

        // *********** AUTO SPECIES CREATION DISABLED FOR NOW!!! ***************************** 
        //*** RE-ENABLED!!!!!!
        // CHECK IF NEW SPECIES CREATED:

        bool assignedToNewSpecies = false;
            
        if(closestDistance > speciesSimilarityDistanceThreshold) {
            if(!speciesCreatedOrDestroyedThisFrame) {
                if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
                    
                    // Create new species!::::     
                    assignedToNewSpecies = true;

                    int seedSpeciesID = closestSpeciesID; // simManagerRef.masterGenomePool.currentlyActiveSpeciesIDList[ UnityEngine.Random.Range(0, currentlyActiveSpeciesIDList.Count) ];
                    //AgentGenome seedGenome =
                    simManagerRef.AddNewSpecies(newGenome, seedSpeciesID);

                    speciesSimilarityDistanceThreshold += 70f;

                    Color colo = new Color(newGenome.bodyGenome.appearanceGenome.huePrimary.x, newGenome.bodyGenome.appearanceGenome.huePrimary.y, newGenome.bodyGenome.appearanceGenome.huePrimary.z);
                    panelPendingClickPrompt.Narrate("A new species has emerged! " + newGenome.bodyGenome.coreGenome.name, colo); 
                }               
            }
            else {
                Debug.Log("speciesCreatedOrDestroyedThisFrame closestDistanceSpeciesID: " + closestSpeciesID.ToString() + ", score: " + closestDistance.ToString());
            }
        }
        else {
            //Debug.Log("closestDistanceSpeciesID: " + closestSpeciesID.ToString() + ", score: " + closestDistance.ToString());
        }

        if(!assignedToNewSpecies) {
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
        else {
            // *** ???
            Debug.Log("assignedToNewSpecies closestDistanceSpeciesID: " + closestSpeciesID.ToString() + ", score: " + closestDistance.ToString());
            completeSpeciesPoolsList[closestSpeciesID].AddNewCandidateGenome(newGenome);
        }
        
        if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
            speciesSimilarityDistanceThreshold *= 0.97f;  // lower while creating treeOfLifeUI
        }
        else {
            speciesSimilarityDistanceThreshold *= 1.05f;
            speciesSimilarityDistanceThreshold = Mathf.Min(speciesSimilarityDistanceThreshold, 6f); // cap
        }

        CheckForExtinction(simManagerRef); // *** TEMPORARILLY (UN)DISABLED!!!!! *************
    }

    public void GlobalFindCandidateID(int ID) {
        
        int foundSpeciesID = -1;
        bool foundCandidate = false;

        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            int completeSpeciesIndex = currentlyActiveSpeciesIDList[i];
            for(int j = 0; j < completeSpeciesPoolsList[completeSpeciesIndex].candidateGenomesList.Count; j++) {
                int refID = completeSpeciesPoolsList[completeSpeciesIndex].candidateGenomesList[j].candidateID;

                if(refID == ID) {
                    foundCandidate = true;
                    foundSpeciesID = completeSpeciesPoolsList[completeSpeciesIndex].speciesID;
                    Debug.Log("FOUND! " + ID.ToString() + ", species: " + foundSpeciesID.ToString() + ", CDSID: " + refID.ToString());
                }
            }
        } 
        
        if(foundCandidate == false) {
            // look through ALL species?

            for(int a = 0; a < completeSpeciesPoolsList.Count; a++) {
                for(int b = 0; b < completeSpeciesPoolsList[a].candidateGenomesList.Count; b++) {
                    int refID = completeSpeciesPoolsList[a].candidateGenomesList[b].candidateID;

                    if(refID == ID) {
                        foundCandidate = true;
                        foundSpeciesID = completeSpeciesPoolsList[a].speciesID;
                        Debug.Log("FOUND COMPLETE! " + ID.ToString() + ", species: " + foundSpeciesID.ToString() + ", CDSID: " + refID.ToString());
                    }
                }                
            }
        }

        if(foundCandidate == false) {
            for(int k = 0; k < debugRecentlyDeletedCandidateIDsList.Count; k++) {
                if(debugRecentlyDeletedCandidateIDsList[k] == ID) {
                    foundCandidate = true;
                    //foundSpeciesID = completeSpeciesPoolsList[a].speciesID;
                    // It was somehow deleted twice?  Or simply looking in the wrong SpeciesPool?
                    Debug.Log("FOUND COMPLETE! " + ID.ToString() + ", CDSID: " + debugRecentlyDeletedCandidateIDsList[k].ToString());
                }
            }
        }
    }
     
    public float GetSimilarityScore(AgentGenome newGenome, AgentGenome repGenome) {

        // DUE FOR UPGRADE / OVERHAUL!!!!  ******

        // *** need to normalize all to 0-1 for more fair comparison? ***
        // have centralized min/max values for each attribute?
        float dBaseSize = Mathf.Abs(newGenome.bodyGenome.coreGenome.creatureBaseLength - repGenome.bodyGenome.coreGenome.creatureBaseLength);
        float dBaseAspectRatio = Mathf.Abs(newGenome.bodyGenome.coreGenome.creatureAspectRatio - repGenome.bodyGenome.coreGenome.creatureAspectRatio);

        float mouthA = 1f;
        if (newGenome.bodyGenome.coreGenome.isPassive)
            mouthA = 0f;
        float mouthB = 1f;
        if (repGenome.bodyGenome.coreGenome.isPassive)
            mouthB = 0f;
        float dMouthType = Mathf.Abs(mouthA - mouthB);

        float dPriColor = Mathf.Abs((newGenome.bodyGenome.appearanceGenome.huePrimary - repGenome.bodyGenome.appearanceGenome.huePrimary).sqrMagnitude);
        float dSecColor = Mathf.Abs((newGenome.bodyGenome.appearanceGenome.hueSecondary - repGenome.bodyGenome.appearanceGenome.hueSecondary).sqrMagnitude);

        // proportions:
        float dMouthLength = Mathf.Abs(newGenome.bodyGenome.coreGenome.mouthLength - repGenome.bodyGenome.coreGenome.mouthLength);
        float dHeadLength = Mathf.Abs(newGenome.bodyGenome.coreGenome.headLength - repGenome.bodyGenome.coreGenome.headLength);
        float dBodyLength = Mathf.Abs(newGenome.bodyGenome.coreGenome.bodyLength - repGenome.bodyGenome.coreGenome.bodyLength);
        float dTailLength = Mathf.Abs(newGenome.bodyGenome.coreGenome.tailLength - repGenome.bodyGenome.coreGenome.tailLength);
        
        // eyes
        float dEyeSize = Mathf.Abs(newGenome.bodyGenome.coreGenome.socketRadius - repGenome.bodyGenome.coreGenome.socketRadius);
        float dEyeHeight = Mathf.Abs(newGenome.bodyGenome.coreGenome.socketHeight - repGenome.bodyGenome.coreGenome.socketHeight);

        // tail
        float dTailFinLength = Mathf.Abs(newGenome.bodyGenome.coreGenome.tailFinBaseLength - repGenome.bodyGenome.coreGenome.tailFinBaseLength);
        float dTailFinSpread = Mathf.Abs(newGenome.bodyGenome.coreGenome.tailFinSpreadAngle - repGenome.bodyGenome.coreGenome.tailFinSpreadAngle);

        // sensors & shit:
        float dFoodSpecDecay = Mathf.Abs(newGenome.bodyGenome.coreGenome.dietSpecializationDecay - repGenome.bodyGenome.coreGenome.dietSpecializationDecay);
        float dFoodSpecPlant = Mathf.Abs(newGenome.bodyGenome.coreGenome.dietSpecializationPlant - repGenome.bodyGenome.coreGenome.dietSpecializationPlant);
        float dFoodSpecMeat = Mathf.Abs(newGenome.bodyGenome.coreGenome.dietSpecializationMeat - repGenome.bodyGenome.coreGenome.dietSpecializationMeat);
                
        float dUseComms = newGenome.bodyGenome.communicationGenome.useComms == repGenome.bodyGenome.communicationGenome.useComms ? 0f : 1f;
        float dUseWaterStats = newGenome.bodyGenome.environmentalGenome.useWaterStats == repGenome.bodyGenome.environmentalGenome.useWaterStats ? 0f : 1f;
        float dUseCardinals = newGenome.bodyGenome.environmentalGenome.useCardinals == repGenome.bodyGenome.environmentalGenome.useCardinals ? 0f : 1f;
        float dUseDiagonals = newGenome.bodyGenome.environmentalGenome.useDiagonals == repGenome.bodyGenome.environmentalGenome.useDiagonals ? 0f : 1f;
        float dUseNutrients = newGenome.bodyGenome.foodGenome.useNutrients == repGenome.bodyGenome.foodGenome.useNutrients ? 0f : 1f;
        float dUseFoodPos = newGenome.bodyGenome.foodGenome.usePos == repGenome.bodyGenome.foodGenome.usePos ? 0f : 1f;
        float dUseFoodVel = newGenome.bodyGenome.foodGenome.useVel == repGenome.bodyGenome.foodGenome.useVel ? 0f : 1f;
        float dUseFoodDir = newGenome.bodyGenome.foodGenome.useDir == repGenome.bodyGenome.foodGenome.useDir ? 0f : 1f;
        float dUseFoodStats = newGenome.bodyGenome.foodGenome.useStats == repGenome.bodyGenome.foodGenome.useStats ? 0f : 1f;
        float dUseFoodEgg = newGenome.bodyGenome.foodGenome.useEggs == repGenome.bodyGenome.foodGenome.useEggs ? 0f : 1f;
        float dUseFoodCorpse = newGenome.bodyGenome.foodGenome.useCorpse == repGenome.bodyGenome.foodGenome.useCorpse ? 0f : 1f;
        float dUseFriendPos = newGenome.bodyGenome.friendGenome.usePos == repGenome.bodyGenome.friendGenome.usePos ? 0f : 1f;
        float dUseFriendVel = newGenome.bodyGenome.friendGenome.useVel == repGenome.bodyGenome.friendGenome.useVel ? 0f : 1f;
        float dUseFriendDir = newGenome.bodyGenome.friendGenome.useDir == repGenome.bodyGenome.friendGenome.useDir ? 0f : 1f;
        float dUseThreatPos = newGenome.bodyGenome.threatGenome.usePos == repGenome.bodyGenome.threatGenome.usePos ? 0f : 1f;
        float dUseThreatVel = newGenome.bodyGenome.threatGenome.useVel == repGenome.bodyGenome.threatGenome.useVel ? 0f : 1f;
        float dUseThreatDir = newGenome.bodyGenome.threatGenome.useDir == repGenome.bodyGenome.threatGenome.useDir ? 0f : 1f;
        float dUseThreatStats = newGenome.bodyGenome.threatGenome.useStats == repGenome.bodyGenome.threatGenome.useStats ? 0f : 1f;

        float dSensory = dFoodSpecDecay + dFoodSpecPlant + dFoodSpecMeat + dUseComms + dUseWaterStats + dUseCardinals + dUseDiagonals +
                         dUseNutrients + dUseFoodPos + dUseFoodVel + dUseFoodDir + dUseFoodStats + dUseFoodEgg + dUseFoodCorpse + dUseFriendPos + dUseFriendVel + dUseFriendDir +
                         dUseThreatPos + dUseThreatVel + dUseThreatDir + dUseThreatStats;
        float dNeurons = Mathf.Abs(newGenome.brainGenome.hiddenNeuronList.Count - repGenome.brainGenome.hiddenNeuronList.Count);
        float dAxons = Mathf.Abs(newGenome.brainGenome.linkList.Count - repGenome.brainGenome.linkList.Count) / (float)repGenome.brainGenome.bodyNeuronList.Count;

        float dSpecialization = (new Vector4(newGenome.bodyGenome.coreGenome.talentSpecializationAttack, newGenome.bodyGenome.coreGenome.talentSpecializationDefense, newGenome.bodyGenome.coreGenome.talentSpecializationSpeed, newGenome.bodyGenome.coreGenome.talentSpecializationUtility).normalized - 
                                 new Vector4(repGenome.bodyGenome.coreGenome.talentSpecializationAttack, repGenome.bodyGenome.coreGenome.talentSpecializationDefense, repGenome.bodyGenome.coreGenome.talentSpecializationSpeed, repGenome.bodyGenome.coreGenome.talentSpecializationUtility).normalized).magnitude;

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
