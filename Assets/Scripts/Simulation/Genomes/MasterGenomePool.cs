using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MasterGenomePool {

    public static int nextCandidateIndex = 0;

    public int maxNumActiveSpecies = 6;
    private int targetNumSpecies = 4;
    public float speciesSimilarityDistanceThreshold = 5f;
    private int minNumGauranteedEvalsForNewSpecies = 256;
    
    public List<int> currentlyActiveSpeciesIDList;
    public List<SpeciesGenomePool> completeSpeciesPoolsList;

    public bool speciesCreatedOrDestroyedThisFrame = false;

    private MutationSettings mutationSettingsRef;

    //public int curNumSpecies;
    //private int maxNumSpecies = 6;

    public MasterGenomePool() {
        // empty constructor
    }

    public void FirstTimeInitialize(int numAgentGenomes, MutationSettings mutationSettingsRef) {
        nextCandidateIndex = 0;

        this.mutationSettingsRef = mutationSettingsRef;

        currentlyActiveSpeciesIDList = new List<int>();
        completeSpeciesPoolsList = new List<SpeciesGenomePool>();

        // Create foundational Species:
        SpeciesGenomePool firstSpecies = new SpeciesGenomePool(0, -1, mutationSettingsRef);
        firstSpecies.FirstTimeInitialize(numAgentGenomes);

        currentlyActiveSpeciesIDList.Add(0);
        completeSpeciesPoolsList.Add(firstSpecies);
    }

    public void Tick() {
        speciesCreatedOrDestroyedThisFrame = false;


    }

    private void CheckForExtinction() {
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
                        ExtinctifySpecies(currentlyActiveSpeciesIDList[i]);
                    }
                }
            }

            if(noCurrentlyExtinctFlaggedSpecies) {
                if(completeSpeciesPoolsList[leastFitSpeciesID].numAgentsEvaluated > minNumGauranteedEvalsForNewSpecies) {
                    // OK to KILL!!!
                    completeSpeciesPoolsList[leastFitSpeciesID].isFlaggedForExtinction = true;
                    Debug.Log("FLAG EXTINCT: " + leastFitSpeciesID.ToString());
                }
            }
        }
    }
    private void ExtinctifySpecies(int speciesID) {
        Debug.Log("REMOVE SPECIES " + speciesID.ToString());

        // find and remove from active list:
        int listIndex = -1;
        for(int i = 0; i < currentlyActiveSpeciesIDList.Count; i++) {
            if(speciesID == completeSpeciesPoolsList[ currentlyActiveSpeciesIDList[i] ].speciesID) {
                listIndex = i;
            }
        }

        currentlyActiveSpeciesIDList.RemoveAt(listIndex);
        completeSpeciesPoolsList[speciesID].isExtinct = true;
    }

    public SpeciesGenomePool SelectNewGenomeSourceSpecies() {
        // NAIVE RANDOM AT FIRST:
        int randomTableIndex = UnityEngine.Random.Range(0, currentlyActiveSpeciesIDList.Count);
        int speciesIndex = currentlyActiveSpeciesIDList[randomTableIndex];

        return completeSpeciesPoolsList[speciesIndex];
    }

    public void AssignNewMutatedGenomeToSpecies(AgentGenome newGenome, int parentSpeciesID) {
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
                    int newSpeciesID = completeSpeciesPoolsList.Count;
                    closestSpeciesID = newSpeciesID;
                    SpeciesGenomePool newSpecies = new SpeciesGenomePool(newSpeciesID, parentSpeciesID, mutationSettingsRef);
                    newSpecies.FirstTimeInitialize(newGenome);

                    currentlyActiveSpeciesIDList.Add(newSpeciesID);
                    completeSpeciesPoolsList.Add(newSpecies);

                    speciesCreatedOrDestroyedThisFrame = true;

                    Debug.Log("New Species Created!!! (" + newSpeciesID.ToString() + "] score: " + closestDistance.ToString());
                }                
            }
        }
        else {
            //Debug.Log("closestDistanceSpeciesID: " + closestSpeciesID.ToString() + ", score: " + closestDistance.ToString());
        }

        if(!assignedToNewSpecies) {
            completeSpeciesPoolsList[closestSpeciesID].AddNewCandidateGenome(newGenome);
        } 
        
        if(currentlyActiveSpeciesIDList.Count < maxNumActiveSpecies) {
            speciesSimilarityDistanceThreshold *= 0.995f;
        }
        else {
            speciesSimilarityDistanceThreshold *= 1.005f;
            speciesSimilarityDistanceThreshold = Mathf.Min(speciesSimilarityDistanceThreshold, 5f); // cap
        }

        CheckForExtinction();
    }
        
	//public int GetClosestActiveSpeciesToGenome(AgentGenome genome) {

        
        
        //return closestSpeciesID;
    //}

    public float GetSimilarityScore(AgentGenome newGenome, AgentGenome repGenome) {

        float dWidth = Mathf.Abs(newGenome.bodyGenome.coreGenome.fullBodyWidth - repGenome.bodyGenome.coreGenome.fullBodyWidth);
        float dLength = Mathf.Abs(newGenome.bodyGenome.coreGenome.fullBodyLength - repGenome.bodyGenome.coreGenome.fullBodyLength);
        float mouthA = 1f;
        if (newGenome.bodyGenome.coreGenome.isPassive)
            mouthA = 0f;
        float mouthB = 1f;
        if (repGenome.bodyGenome.coreGenome.isPassive)
            mouthB = 0f;
        float dMouth = Mathf.Abs(mouthA - mouthB);
        float dPriColor = Mathf.Abs((newGenome.bodyGenome.appearanceGenome.huePrimary - repGenome.bodyGenome.appearanceGenome.huePrimary).sqrMagnitude);
        float dSecColor = Mathf.Abs((newGenome.bodyGenome.appearanceGenome.hueSecondary - repGenome.bodyGenome.appearanceGenome.hueSecondary).sqrMagnitude);

        float delta = dWidth + dLength + dMouth + dPriColor + dSecColor;

        return delta;
    }
}
