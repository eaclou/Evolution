using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesGenomePool {

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life

    public MutationSettings mutationSettingsRef;

    public AgentGenome representativeGenome;
    public List<CandidateAgentData> leaderboardGenomesList;
    public List<CandidateAgentData> candidateGenomesList;
	
    public SpeciesGenomePool(int ID, MutationSettings settings) {

        speciesID = ID;
        mutationSettingsRef = settings;
    }

    public void FirstTimeInitialize(int numGenomes) {

        candidateGenomesList = new List<CandidateAgentData>();
        leaderboardGenomesList = new List<CandidateAgentData>();
                
        for (int i = 0; i < numGenomes; i++) {
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.GenerateInitialRandomBodyGenome();
            int tempNumHiddenNeurons = 0;
            agentGenome.InitializeRandomBrainFromCurrentBody(mutationSettingsRef.initialConnectionChance, tempNumHiddenNeurons);

            CandidateAgentData candidate = new CandidateAgentData(agentGenome);

            candidateGenomesList.Add(candidate);
            leaderboardGenomesList.Add(candidate);
        }
    }
}
