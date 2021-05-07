using System.Collections.Generic;

[System.Serializable]
public class CandidateAgentData {

    public int candidateID;
    public int speciesID;
    public AgentGenome candidateGenome;
    public int numCompletedEvaluations = 0;
    //public List<float> evaluationScoresList;  // fitness scores of agents with this genome
    public bool allEvaluationsComplete = false;
    public bool isBeingEvaluated = false;


    // stats:
    public struct PerformanceData {
	    public float totalFoodEatenPlant;
        public float totalFoodEatenZoop;
        public float totalFoodEatenEgg;
        public float totalFoodEatenCorpse;
        public float totalFoodEatenCreature;
        public float totalDamageDealt;
        public float totalDamageTaken;
        public float totalTicksRested;
        public float totalTicksAlive;
        public float totalTimesPregnant;
        
        public float totalTimesDashed;
        public float totalTimesDefended;
        public float totalTimesAttacked;
        public float totalTimesActed => totalTimesAttacked + totalTimesDashed + totalTimesDefended;
        public float attackActionPercent => totalTimesAttacked / (totalTimesActed + .01f);
        public float defendActionPercent => totalTimesDefended / (totalTimesActed + .01f);
        public float dashActionPercent => totalTimesDashed / (totalTimesActed + .01f);
        
        // WPP 5/6/21: corpse included twice -> replaced with creature
        public float totalEaten => totalFoodEatenCreature + totalFoodEatenCorpse + totalFoodEatenEgg + totalFoodEatenPlant + totalFoodEatenZoop;
        public float percentPlantEaten => totalFoodEatenPlant / (totalEaten + .01f);
        public float percentEggEaten => totalFoodEatenEgg / (totalEaten + .01f);
        public float percentCorpseEaten => totalFoodEatenCorpse / (totalEaten + .01f);
        public float percentZooplantonEaten => totalFoodEatenZoop / (totalEaten + .01f);
        public float percentCreatureEaten => totalFoodEatenCreature / (totalEaten + .01f);
    }
    
    public PerformanceData performanceData;

    public struct CandidateEventData {
        public int eventFrame;
        public string eventText;
        public float goodness;
        
        public CandidateEventData(int eventFrame, string eventText, float goodness)
        {
            this.eventFrame = eventFrame;
            this.eventText = eventText;
            this.goodness = goodness;
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

        candidateEventDataList = new List<CandidateEventData>();
        performanceData = new PerformanceData();
    }

    public void ProcessCompletedEvaluation(Agent agentRef) {
        //evaluationScoresList.Add(agentRef.masterFitnessScore);
        //this.performanceData = agentRef.perform  // 

        numCompletedEvaluations++;
        isBeingEvaluated = false;
    }

    public void RegisterCandidateEvent(int frame, string textString, float goodness) {        
        // WPP: use object initializer
        CandidateEventData newEvent = new CandidateEventData(frame, textString, goodness);
        candidateEventDataList.Add(newEvent);
    }

    
}
