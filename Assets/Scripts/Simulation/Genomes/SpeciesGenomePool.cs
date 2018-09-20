using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesGenomePool {

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life

    [System.NonSerialized]
    public MutationSettings mutationSettingsRef;  // Or remove this later to keep everything saveable?

    public AgentGenome representativeGenome;
    public List<CandidateAgentData> leaderboardGenomesList;
    public List<CandidateAgentData> candidateGenomesList;

    public int maxLeaderboardGenomePoolSize = 64;
	
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

    public AgentGenome GetNextAvailableCandidateGenome() {

        AgentGenome genome = null; // candidateGenomesList[0].candidateGenome;
        for(int i = 0; i < candidateGenomesList.Count; i++) {
            if(candidateGenomesList[i].isBeingEvaluated) {
                // already being tested
            }
            else {
                genome = candidateGenomesList[i].candidateGenome;
                candidateGenomesList[i].isBeingEvaluated = true;
            }
        }
        
        return genome;
    }  
    
    public void ProcessCompletedCandidate(CandidateAgentData candidateData) {

        leaderboardGenomesList.Insert(0, candidateData);  // place at front of leaderboard list (genomes eligible for being parents)
        if(leaderboardGenomesList.Count > maxLeaderboardGenomePoolSize) {
            leaderboardGenomesList.RemoveAt(leaderboardGenomesList.Count - 1);
        }

        candidateGenomesList.Remove(candidateData);  // Will this work? never used this before
    }

    public AgentGenome GetNewMutatedGenome() {

        // Rank current leaderBoard list based on score
        // choose genome by lottery
        // return genome

        return null;
    }
}
