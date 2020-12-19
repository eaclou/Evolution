using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CandidateAgentData {

    public int candidateID;
    public int speciesID;
    public AgentGenome candidateGenome;
    public int numCompletedEvaluations = 0;
    //public List<float> evaluationScoresList;  // fitness scores of agents with this genome
    public bool allEvaluationsComplete = false;
    public bool isBeingEvaluated = false;

    public SpeciesGenomePool.PerformanceData performanceData;
    
    // move stored performance stats here? *****
    //
    //

    public CandidateAgentData(AgentGenome genome, int speciesID) {
        //Debug.Log("NewCandidateData: " + MasterGenomePool.nextCandidateIndex.ToString());
        int ID = MasterGenomePool.nextCandidateIndex;
        MasterGenomePool.nextCandidateIndex++;

        this.candidateID = ID;
        this.speciesID = speciesID;
        candidateGenome = genome;
        numCompletedEvaluations = 0;
        //evaluationScoresList = new List<float>();
        allEvaluationsComplete = false;
        isBeingEvaluated = false;

        performanceData = new SpeciesGenomePool.PerformanceData();
    }

    public void ProcessCompletedEvaluation(Agent agentRef) {
        //evaluationScoresList.Add(agentRef.masterFitnessScore);
        //this.performanceData = agentRef.perform  // 

        numCompletedEvaluations++;
        isBeingEvaluated = false;
    }

    
}
