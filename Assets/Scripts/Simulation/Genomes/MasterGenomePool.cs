using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MasterGenomePool {

    public int maxNumActiveSpecies = 6;
    
    public List<int> currentlyActiveSpeciesIDList;

    public List<SpeciesGenomePool> completeSpeciesPoolsList;


    public MasterGenomePool() {
        // empty constructor
    }

    public void FirstTimeInitialize(int numAgentGenomes, MutationSettings mutationSettingsRef) {

        currentlyActiveSpeciesIDList = new List<int>();
        completeSpeciesPoolsList = new List<SpeciesGenomePool>();

        // Create foundational Species:
        SpeciesGenomePool firstSpecies = new SpeciesGenomePool(0, mutationSettingsRef);
        firstSpecies.FirstTimeInitialize(numAgentGenomes);

        currentlyActiveSpeciesIDList.Add(0);
        completeSpeciesPoolsList.Add(firstSpecies);
    }
        
	public int GetClosestActiveSpeciesToGenome(AgentGenome genome) {

        // temp:
        return currentlyActiveSpeciesIDList[0];
    }
}
