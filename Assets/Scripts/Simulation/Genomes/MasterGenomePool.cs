using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MasterGenomePool {

    //public TreeOfLifeManager treeOfLifeManager;

    public static int nextCandidateIndex = 0;

    public int maxNumActiveSpecies = 6;
    private int targetNumSpecies = 3;
    public float speciesSimilarityDistanceThreshold = 8f;
    private int minNumGuaranteedEvalsForNewSpecies = 128;

    public int currentHighestDepth = 1;
    
    public List<int> currentlyActiveSpeciesIDList;
    public List<SpeciesGenomePool> completeSpeciesPoolsList;

    public bool speciesCreatedOrDestroyedThisFrame = false;

    private MutationSettings mutationSettingsRef;

    public List<int> debugRecentlyDeletedCandidateIDsList;

    //public int curNumSpecies;
    //private int maxNumSpecies = 6;

    public MasterGenomePool() {
        // empty constructor
        
    }

    public void FirstTimeInitialize(int numAgentGenomes, MutationSettings mutationSettingsRef, UIManager uiManagerRef) {
        debugRecentlyDeletedCandidateIDsList = new List<int>();

        nextCandidateIndex = 0;
        this.mutationSettingsRef = mutationSettingsRef;
        currentlyActiveSpeciesIDList = new List<int>();
        completeSpeciesPoolsList = new List<SpeciesGenomePool>();

        SpeciesGenomePool rootSpecies = new SpeciesGenomePool(0, -1, 0, mutationSettingsRef);
        rootSpecies.FirstTimeInitialize(numAgentGenomes, 0);
        currentlyActiveSpeciesIDList.Add(0);
        completeSpeciesPoolsList.Add(rootSpecies);
        // When do I create nodeCollider & shit?

        // Create foundational Species:
        SpeciesGenomePool firstSpecies = new SpeciesGenomePool(1, 0, 0, mutationSettingsRef);
        firstSpecies.FirstTimeInitialize(numAgentGenomes, 1);

        currentlyActiveSpeciesIDList.Add(1);
        completeSpeciesPoolsList.Add(firstSpecies);
        
        uiManagerRef.treeOfLifeManager = new TreeOfLifeManager(uiManagerRef.treeOfLifeAnchorGO, uiManagerRef);
        uiManagerRef.treeOfLifeManager.FirstTimeInitialize(this);                
    }

    public void UpdateYearlySpeciesStats(int year) {

        for(int i = 0; i < completeSpeciesPoolsList.Count; i++) {
            completeSpeciesPoolsList[i].UpdateYearlyStats(year);
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
                float fitness = completeSpeciesPoolsList[currentlyActiveSpeciesIDList[i]].avgLifespan;
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
                    completeSpeciesPoolsList[leastFitSpeciesID].isFlaggedForExtinction = true;
                    Debug.Log("FLAG EXTINCT: " + leastFitSpeciesID.ToString());
                }
            }
        }
    }
    public void ExtinctifySpecies(SimulationManager simManagerRef, int speciesID) {
        Debug.Log("REMOVE SPECIES " + speciesID.ToString());

        // find and remove from active list:
        int listIndex = -1;
        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(speciesID == completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].speciesID) {
                listIndex = i;
            }
        }

        currentlyActiveSpeciesIDList.RemoveAt(listIndex);
        completeSpeciesPoolsList[speciesID].ProcessExtinction();

        //Signal RenderKing/TreeOfLifeManager to update:
        simManagerRef.uiManager.treeOfLifeManager.RemoveExtinctSpecies(speciesID);
        simManagerRef.theRenderKing.TreeOfLifeExtinctSpecies(speciesID);
    }

    public SpeciesGenomePool SelectNewGenomeSourceSpecies() {
        // NAIVE RANDOM AT FIRST:
        int randomTableIndex = UnityEngine.Random.Range(0, currentlyActiveSpeciesIDList.Count);
        int speciesIndex = currentlyActiveSpeciesIDList[randomTableIndex];

        return completeSpeciesPoolsList[speciesIndex];
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

        // CHECK IF NEW SPECIES CREATED:
        bool assignedToNewSpecies = false;
        if(closestDistance > speciesSimilarityDistanceThreshold) {
            if(!speciesCreatedOrDestroyedThisFrame) {
                if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
                    // Create new species!::::     
                    assignedToNewSpecies = true;
                    // if so, update this 
                    // Create foundational Species:
                    
                    simManagerRef.AddNewSpecies(newGenome, parentSpeciesID);

                    speciesSimilarityDistanceThreshold += 4f;
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
            //completeSpeciesPoolsList[parentSpeciesID].AddNewCandidateGenome(newGenome);
            completeSpeciesPoolsList[closestSpeciesID].AddNewCandidateGenome(newGenome);
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

        CheckForExtinction(simManagerRef);
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
        float dBaseAspectRatio = Mathf.Abs(newGenome.bodyGenome.coreGenome.creatureBaseAspectRatio - repGenome.bodyGenome.coreGenome.creatureBaseAspectRatio);
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

        float delta = dBaseSize + dBaseAspectRatio +
            dMouthType +
            dPriColor * 5f + dSecColor * 5f +
            dMouthLength + dHeadLength + dBodyLength + dTailLength +
            dEyeSize + dEyeHeight +
            dTailFinLength + dTailFinSpread;

        //Debug.Log("Difference Score: " + delta.ToString());

        return delta;
    }
}
