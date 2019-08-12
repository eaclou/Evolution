using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MasterGenomePool {

    //public TreeOfLifeManager treeOfLifeManager;

    public static int nextCandidateIndex = 0;

    public int maxNumActiveSpecies = 6;
    private int targetNumSpecies = 3;
    public float speciesSimilarityDistanceThreshold = 4f;
    private int minNumGuaranteedEvalsForNewSpecies = 256;

    public int currentHighestDepth = 1;
    
    public List<int> currentlyActiveSpeciesIDList;
    public List<SpeciesGenomePool> completeSpeciesPoolsList;

    public bool speciesCreatedOrDestroyedThisFrame = false;

    public MutationSettings mutationSettingsRef;

    public List<int> debugRecentlyDeletedCandidateIDsList;

    public WorldLayerVertebrateGenome[] vertebrateSlotsGenomesCurrentArray;  // algae particles!  -- likely to be converted into plants eventually ***
    public WorldLayerVertebrateGenome[][] vertebrateSlotsGenomesMutationsArray;  // Layer Slots are on outside

    //public int curNumSpecies;
    //private int maxNumSpecies = 6;

    public MasterGenomePool() {
        
    }

    public void FirstTimeInitialize(int numAgentGenomes, MutationSettings mutationSettingsRef, UIManager uiManagerRef) {
        debugRecentlyDeletedCandidateIDsList = new List<int>();

        nextCandidateIndex = 0;
        this.mutationSettingsRef = mutationSettingsRef;
        currentlyActiveSpeciesIDList = new List<int>();
        completeSpeciesPoolsList = new List<SpeciesGenomePool>();

        SpeciesGenomePool rootSpecies = new SpeciesGenomePool(0, -1, 0, 0, mutationSettingsRef);
        rootSpecies.FirstTimeInitialize(numAgentGenomes, 0);
        currentlyActiveSpeciesIDList.Add(0);
        completeSpeciesPoolsList.Add(rootSpecies);
        

        vertebrateSlotsGenomesCurrentArray = new WorldLayerVertebrateGenome[4]; // 4 slots
        for(int i = 0; i < 4; i++) {
            WorldLayerVertebrateGenome vertebrateSlotGenome = new WorldLayerVertebrateGenome();
            vertebrateSlotGenome.name = "Vertebrate " + i.ToString();
            vertebrateSlotGenome.textDescriptionMutation = "uhhh... " + " ??";
            vertebrateSlotsGenomesCurrentArray[i] = vertebrateSlotGenome;
        }

        vertebrateSlotsGenomesMutationsArray = new WorldLayerVertebrateGenome[4][];
        for(int slot = 0; slot < 4; slot++) {
            vertebrateSlotsGenomesMutationsArray[slot] = new WorldLayerVertebrateGenome[4]; // this 4 is number of mutation variants
            for(int mutation = 0; mutation < 4; mutation++) {
                WorldLayerVertebrateGenome vertebrateSlotMutatedGenome = new WorldLayerVertebrateGenome();
                vertebrateSlotMutatedGenome.name = vertebrateSlotsGenomesCurrentArray[slot].name;
                vertebrateSlotsGenomesMutationsArray[slot][mutation] = vertebrateSlotMutatedGenome;
            }
        }
    }

    public void AddNewYearlySpeciesStats(int year) {
        for(int i = 0; i < completeSpeciesPoolsList.Count; i++) {
            completeSpeciesPoolsList[i].AddNewYearlyStats(year);
        }
    }
    
    public void GenerateWorldLayerVertebrateGenomeMutationOptions(int slotID, int speciesIndex) {
        //int speciesIndex = 
        Debug.Log("GenerateWorldLayerVertebrateGenomeMutationOptions:  slot[ " + slotID.ToString() + " } __ Species:  " + speciesIndex.ToString());
        for(int mutationID = 0; mutationID < 4; mutationID++) {
            float mutationSize = Mathf.Clamp01((float)mutationID / 3f + 0.00015f); 
            mutationSize = mutationSize * mutationSize;
            
            SpeciesGenomePool sourceSpeciesPool = completeSpeciesPoolsList[speciesIndex];
            // update settings:::  not best way to do this.......
            mutationSettingsRef.defaultBodyMutationChance = 1f;
            mutationSettingsRef.defaultBodyMutationStepSize = 1f;
            mutationSettingsRef.mutationStrengthSlot = mutationSize * mutationSize;  // ***** Shallower curve --> smaller mutations on lower end

            AgentGenome mutatedGenome = sourceSpeciesPool.Mutate(vertebrateSlotsGenomesCurrentArray[slotID].representativeGenome, true, true); // sourceSpeciesPool.representativeGenome, true, true);
            vertebrateSlotsGenomesMutationsArray[slotID][mutationID].SetRepresentativeGenome(mutatedGenome);
            mutatedGenome.bodyGenome.CalculateFullsizeBoundingBox();
            string descriptionText = "baseLength: " + mutatedGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") +
                                     "\naspectRatio: " + mutatedGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2") +
                                     "\n" + mutatedGenome.bodyGenome.fullsizeBoundingBox.ToString("F1");
            vertebrateSlotsGenomesMutationsArray[slotID][mutationID].name = descriptionText; // vertebrateSlotsGenomesCurrentArray[slotID].name;
            vertebrateSlotsGenomesMutationsArray[slotID][mutationID].textDescriptionMutation = descriptionText; // "Mutation^^^ Amt: " + (mutationSize * 100f).ToString("F1") + "%";
            
            
        }
    }
    public void ProcessSlotMutation(int slotID, int mutationID, int speciesIndex) {
        SpeciesGenomePool sourceSpeciesPool = completeSpeciesPoolsList[speciesIndex];
        vertebrateSlotsGenomesCurrentArray[slotID].representativeGenome = vertebrateSlotsGenomesMutationsArray[slotID][mutationID].representativeGenome;
        vertebrateSlotsGenomesCurrentArray[slotID].name = vertebrateSlotsGenomesMutationsArray[slotID][mutationID].name;
        Debug.Log("____ProcessSlotMutation: slot: " + slotID.ToString() + ", mut#: " + mutationID.ToString() + ", speciesID: " + speciesIndex.ToString());
        // = mutatedGenome;

        //representativeAlgaeLayerGenome = algaeParticlesArray[0];
        //algaeParticlesRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetAlgaeParticleDataSize());
        //AlgaeParticleData[] algaeParticlesRepresentativeGenomeArray = new AlgaeParticleData[1];
        //algaeParticlesRepresentativeGenomeArray[0] = algaeSlotGenomeCurrent.algaeRepData;
        //algaeParticlesRepresentativeGenomeCBuffer.SetData(algaeParticlesRepresentativeGenomeArray);
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
                float fitness = completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].avgFitnessScore;
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

    public void AssignNewMutatedGenomeToSpecies(AgentGenome newGenome, int parentSpeciesID, SimulationManager simManagerRef) {
        // *** Gross code organization btw this and SimManager ***
        int closestSpeciesID = -1;

        float closestDistance = 99999f;
        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(!completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].isFlaggedForExtinction) {  // Dying species not allowed?
                float similarityDistance = GetSimilarityScore(newGenome, completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].representativeGenome);
                if(similarityDistance < closestDistance) {
                    closestDistance = similarityDistance;
                    closestSpeciesID = currentlyActiveSpeciesIDList[i];
                }
            }            
        }

        // *********** AUTO SPECIES CREATION DISABLED FOR NOW!!! *****************************
        // CHECK IF NEW SPECIES CREATED:

        bool assignedToNewSpecies = false;
            /*
        if(closestDistance > speciesSimilarityDistanceThreshold) {
            if(!speciesCreatedOrDestroyedThisFrame) {
                if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
                    // Create new species!::::     
                    assignedToNewSpecies = true;
                    // if so, update this 
                    // Create foundational Species:
                    
                    simManagerRef.AddNewSpecies(newGenome, parentSpeciesID);

                    speciesSimilarityDistanceThreshold += 10f;
                }               
            }
            else {
                Debug.Log("speciesCreatedOrDestroyedThisFrame closestDistanceSpeciesID: " + closestSpeciesID.ToString() + ", score: " + closestDistance.ToString());
            }
        }
        else {
            //Debug.Log("closestDistanceSpeciesID: " + closestSpeciesID.ToString() + ", score: " + closestDistance.ToString());
        }
        */

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
        }
        
        if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
            speciesSimilarityDistanceThreshold *= 0.9965f;  // lower while creating treeOfLifeUI
        }
        else {
            speciesSimilarityDistanceThreshold *= 1.02f;
            speciesSimilarityDistanceThreshold = Mathf.Min(speciesSimilarityDistanceThreshold, 10f); // cap
        }

        //CheckForExtinction(simManagerRef);  *** TEMPORARILLY DISABLED!!!!! *************
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
