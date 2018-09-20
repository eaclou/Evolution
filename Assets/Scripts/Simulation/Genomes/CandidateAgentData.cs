using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateAgentData {

    //public int candidateID;
    public AgentGenome candidateGenome;
    public int numCompletedEvaluations = 0;
    public List<float> evaluationScoresList;  // fitness scores of agents with this genome
    public bool allEvaluationsComplete = false;
    public bool isBeingEvaluated = false;

    public CandidateAgentData(AgentGenome genome) {

        candidateGenome = genome;
        numCompletedEvaluations = 0;
        evaluationScoresList = new List<float>();
        allEvaluationsComplete = false;
        isBeingEvaluated = false;
    }
}
