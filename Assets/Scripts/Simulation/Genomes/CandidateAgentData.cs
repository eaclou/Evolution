using UnityEngine;
using System.Collections.Generic;

// WPP 5/9/21: De-nested because referenced outside CandidateAgentData
// stats:
public struct PerformanceData {
    public float totalFoodEatenPlant;
    public float totalFoodEatenZoop;
    public float totalFoodEatenEgg;
    public float totalFoodEatenCorpse;
    public float totalFoodEatenCreature;
    public float totalDamageDealt;
    public float totalDamageTaken;
    public float totalTimesDashed;
    public float totalTimesDefended;
    public float totalTimesAttacked;
    public float totalTimesPregnant;
    public float totalTicksRested;
    public float totalTicksAlive;

    public float timeStepHatched;
    public float timeStepDied;
        
    public float totalEaten => totalFoodEatenCorpse + totalFoodEatenEgg + totalFoodEatenCreature + totalFoodEatenPlant + totalFoodEatenZoop;
    public float corpseEatenPercent => totalFoodEatenCorpse / (totalEaten + .01f);
    public float eggEatenPercent => totalFoodEatenEgg / (totalEaten + .01f);
    public float creatureEatenPercent => totalFoodEatenCreature / (totalEaten + .01f);
    public float plantEatenPercent => totalFoodEatenPlant / (totalEaten + .01f);
    public float zooplanktonEatenPercent => totalFoodEatenZoop / (totalEaten + .01f);
        
    public float timesActed => totalTimesAttacked + totalTimesDefended + totalTimesDashed;
    public float attackActionPercent => totalTimesAttacked / (timesActed + .01f);
    public float defendActionPercent => totalTimesDefended / (timesActed + .01f);
    public float dashActionPercent => totalTimesDashed / (timesActed + .01f);

}

[System.Serializable]
public class CandidateAgentData {

    public int candidateID;
    public int speciesID;
    public AgentGenome candidateGenome;
    public int numCompletedEvaluations = 0;
    //public List<float> evaluationScoresList;  // fitness scores of agents with this genome
    public bool allEvaluationsComplete = false; // * WPP: always false
    public bool isBeingEvaluated = false;
    public string name;
    
    public PerformanceData performanceData;

    public struct CandidateEventData {
        public int eventFrame;
        public string eventText;
        public float goodness;
        public int type;
        
        public CandidateEventData(int eventFrame, string eventText, float goodness, int type)
        {
            this.eventFrame = eventFrame;
            this.eventText = eventText;
            this.goodness = goodness;
            this.type = type;
        }
    }
    
    // WPP: renamed -> string = implementation detail
    public string causeOfDeath = "";
    public List<CandidateEventData> candidateEventDataList;
    
    public CandidateAgentData(AgentGenome genome, int speciesID) {
        //Debug.Log("NewCandidateData: " + MasterGenomePool.nextCandidateIndex.ToString());
        int ID = MasterGenomePool.nextCandidateIndex;
        MasterGenomePool.nextCandidateIndex++;
        causeOfDeath = "Alive!";
        this.candidateID = ID;
        this.speciesID = speciesID;
        candidateGenome = genome;
        numCompletedEvaluations = 0;
        //evaluationScoresList = new List<float>();
        allEvaluationsComplete = false;  
        isBeingEvaluated = false;

        this.name = GenerateTempCritterName();

        candidateEventDataList = new List<CandidateEventData>();
        performanceData = new PerformanceData();
    }
    private string GenerateTempCritterName() {
        
        string[] letters = new string[26];
        letters[0] = "A";
        letters[1] = "B";
        letters[2] = "C";
        letters[3] = "D";
        letters[4] = "E";
        letters[5] = "F";
        letters[6] = "G";
        letters[7] = "H";
        letters[8] = "I";
        letters[9] = "J";
        letters[10] = "K";
        letters[11] = "L";
        letters[12] = "M";
        letters[13] = "N";
        letters[14] = "O";
        letters[15] = "P";
        letters[16] = "Q";
        letters[17] = "R";
        letters[18] = "S";
        letters[19] = "T";
        letters[20] = "U";
        letters[21] = "V";
        letters[22] = "W";
        letters[23] = "X";
        letters[24] = "Y";
        letters[25] = "Z";

        
        if (candidateID < 0)
            return "-1";

        string name = "";// letters[cand.speciesID % 26];

        int onesColumn = candidateID % 26;
        name = letters[onesColumn];
        if(candidateID > 26) {
            int tensColummn = Mathf.FloorToInt((float)candidateID / 26f) % 26;
            name = letters[tensColummn] + name;

            if(candidateID > 26 * 26) {
                int hundredsColummn = Mathf.FloorToInt((float)candidateID / (26f * 26f)) % 26;
                name = letters[hundredsColummn] + name;

                if (candidateID > 26 * 26 * 26) {
                    int thousandsColummn = Mathf.FloorToInt((float)candidateID / (26f * 26f * 26f)) % 26;
                    name = letters[thousandsColummn] + name;
                }
            }
        }

        return letters[speciesID % 26] + "'" + name;
    }
    

    public void ProcessCompletedEvaluation(Agent agentRef) {
        //evaluationScoresList.Add(agentRef.masterFitnessScore);
        //this.performanceData = agentRef.perform  // 

        numCompletedEvaluations++;
        isBeingEvaluated = false;
    }

    public void RegisterCandidateEvent(int frame, string textString, float goodness, int type) {        
        // WPP: use object initializer
        CandidateEventData newEvent = new CandidateEventData(frame, textString, goodness, type);
        candidateEventDataList.Add(newEvent);
    }

    
}
