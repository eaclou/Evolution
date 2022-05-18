using UnityEngine;
using System.Collections.Generic;
using Playcraft;

/// Stats
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
    
    /// Initialize as average
    public PerformanceData(List<CandidateAgentData> leaderboard, float inverseCount)
    {
        PerformanceData self = new PerformanceData();
        PerformanceData leader;
    
        // Sum the average leaderboard values
        foreach (var agent in leaderboard)
        {
            leader = agent.performanceData;
        
            self.totalDamageDealt += leader.totalDamageDealt;
            self.totalDamageTaken += leader.totalDamageTaken;
            self.totalFoodEatenCorpse += leader.totalFoodEatenCorpse;
            self.totalFoodEatenCreature += leader.totalFoodEatenCreature;
            self.totalFoodEatenEgg += leader.totalFoodEatenEgg;
            self.totalFoodEatenPlant += leader.totalFoodEatenPlant;
            self.totalFoodEatenZoop += leader.totalFoodEatenZoop;
            self.totalTicksAlive += leader.totalTicksAlive;
            self.totalTicksRested += leader.totalTicksRested;
            self.totalTimesAttacked += leader.totalTimesAttacked;
            self.totalTimesDashed += leader.totalTimesDashed;
            self.totalTimesDefended += leader.totalTimesDefended;
            self.totalTimesPregnant += leader.totalTimesPregnant;  
        }

        // Multiply the result by the inverse of the leaderboard count for the average values
        self.totalDamageDealt *= inverseCount;
        self.totalDamageTaken *= inverseCount;
        self.totalFoodEatenCorpse *= inverseCount;
        self.totalFoodEatenCreature *= inverseCount;
        self.totalFoodEatenEgg *= inverseCount;
        self.totalFoodEatenPlant *= inverseCount;
        self.totalFoodEatenZoop *= inverseCount;
        self.totalTicksAlive *= inverseCount;
        self.totalTicksRested *= inverseCount;
        self.totalTimesAttacked *= inverseCount;
        self.totalTimesDashed *= inverseCount;
        self.totalTimesDefended *= inverseCount;
        self.totalTimesPregnant *= inverseCount; 
        
        this = self;
    }
}

[System.Serializable]
public class CandidateAgentData 
{
    public int candidateID;
    public int parentID;
    public int speciesID;
    public AgentGenome candidateGenome;
    public int numCompletedEvaluations = 0;
    //public List<float> evaluationScoresList;  // fitness scores of agents with this genome
    public bool allEvaluationsComplete = false; // * WPP: always false
    
    /// True while agent is alive and for a little while after.  Must be false before ready to respawn.
    public bool isBeingEvaluated = false;
    
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
    
    public string causeOfDeath = "";
    public List<CandidateEventData> candidateEventDataList;
    
    public CandidateAgentData(CandidateAgentData candidate, int speciesID) {
        //Debug.Log("NewCandidateData: " + MasterGenomePool.nextCandidateIndex.ToString());
        MasterGenomePool.nextCandidateIndex++;
        causeOfDeath = "Alive!";
        candidateID = MasterGenomePool.nextCandidateIndex;
        this.speciesID = speciesID;
        candidateGenome = candidate.candidateGenome;
        numCompletedEvaluations = 0;
        //evaluationScoresList = new List<float>();
        allEvaluationsComplete = false;  
        isBeingEvaluated = false;

        parentID = candidate.parentID;

        candidateEventDataList = new List<CandidateEventData>();
        performanceData = new PerformanceData();
    }
    
    public CandidateAgentData(AgentGenome genome, int speciesID) {
        //Debug.Log("NewCandidateData: " + MasterGenomePool.nextCandidateIndex.ToString());
        MasterGenomePool.nextCandidateIndex++;
        causeOfDeath = "Alive!";
        candidateID = MasterGenomePool.nextCandidateIndex;
        this.speciesID = speciesID;
        candidateGenome = genome;
        numCompletedEvaluations = 0;
        //evaluationScoresList = new List<float>();
        allEvaluationsComplete = false;  
        isBeingEvaluated = false;

        parentID = 0; //????

        candidateEventDataList = new List<CandidateEventData>();
        performanceData = new PerformanceData();
    }
    
    private string GenerateTempCritterName() 
    {
        if (candidateID < 0)
            return "-1";

        string result = RandomStatics.alphabet[speciesID % 26] + "'";
        
        for (int i = 0; i < 4; i++)
            if (!RequestAddLetterToName(ref result, i))
                break;
        
        return result;
    }
    
    bool RequestAddLetterToName(ref string value, int column)
    {
        if (candidateID <= Mathf.Pow(26, column))
            return false;
        
        int character = Mathf.FloorToInt(candidateID / Mathf.Pow(26f, column)) % 26;
        value += RandomStatics.alphabet[character];
        return true;
    }
    
    public void ProcessCompletedEvaluation(Agent agent) {
        //evaluationScoresList.Add(agent.masterFitnessScore);
        //this.performanceData = agent.perform  // 

        numCompletedEvaluations++;
        isBeingEvaluated = false;
    }

    public void RegisterCandidateEvent(int frame, string textString, float goodness, int type) {        
        CandidateEventData newEvent = new CandidateEventData(frame, textString, goodness, type);
        candidateEventDataList.Add(newEvent);
    }
    
    public void SetToAverage(List<CandidateAgentData> leaderboard)
    {
        var inverseCount = 1f / (leaderboard.Count - 1);
        candidateGenome.bodyGenome.coreGenome.SetToAverage(leaderboard, inverseCount);
        candidateGenome.bodyGenome.appearanceGenome.SetToAverage(leaderboard, inverseCount);
        performanceData = new PerformanceData(leaderboard, inverseCount);
        //SetPerformanceDataToAverage(leaderboard, inverseCount);
    }
    
    public Vector3 primaryHue => candidateGenome.primaryHue;
    public Vector3 secondaryHue => candidateGenome.secondaryHue;
    public Color primaryColor => StaticHelpers.VectorToColor(primaryHue);
    public Color secondaryColor => StaticHelpers.VectorToColor(secondaryHue);
    public int bodyStrokeBrushTypeX => candidateGenome.bodyStrokeBrushTypeX;
    public int bodyStrokeBrushTypeY => candidateGenome.bodyStrokeBrushTypeY;
}

#region Dead Code - consider deletion

// PerformanceData is a struct, so needs to be performed here
    /*void SetPerformanceDataToAverage(List<CandidateAgentData> leaderboard, float inverseCount)
    {
        // Clear out existing values
        performanceData = new PerformanceData(); 
        
        PerformanceData leader;
        
        // Sum the average leaderboard values
        foreach (var agent in leaderboard)
        {
            leader = agent.performanceData;
        
            performanceData.totalDamageDealt += leader.totalDamageDealt;
            performanceData.totalDamageTaken += leader.totalDamageTaken;
            performanceData.totalFoodEatenCorpse += leader.totalFoodEatenCorpse;
            performanceData.totalFoodEatenCreature += leader.totalFoodEatenCreature;
            performanceData.totalFoodEatenEgg += leader.totalFoodEatenEgg;
            performanceData.totalFoodEatenPlant += leader.totalFoodEatenPlant;
            performanceData.totalFoodEatenZoop += leader.totalFoodEatenZoop;
            performanceData.totalTicksAlive += leader.totalTicksAlive;
            performanceData.totalTicksRested += leader.totalTicksRested;
            performanceData.totalTimesAttacked += leader.totalTimesAttacked;
            performanceData.totalTimesDashed += leader.totalTimesDashed;
            performanceData.totalTimesDefended += leader.totalTimesDefended;
            performanceData.totalTimesPregnant += leader.totalTimesPregnant;  
        }

        // Multiply the result by the inverse of the leaderboard count for the average values
        performanceData.totalDamageDealt *= inverseCount;
        performanceData.totalDamageTaken *= inverseCount;
        performanceData.totalFoodEatenCorpse *= inverseCount;
        performanceData.totalFoodEatenCreature *= inverseCount;
        performanceData.totalFoodEatenEgg *= inverseCount;
        performanceData.totalFoodEatenPlant *= inverseCount;
        performanceData.totalFoodEatenZoop *= inverseCount;
        performanceData.totalTicksAlive *= inverseCount;
        performanceData.totalTicksRested *= inverseCount;
        performanceData.totalTimesAttacked *= inverseCount;
        performanceData.totalTimesDashed *= inverseCount;
        performanceData.totalTimesDefended *= inverseCount;
        performanceData.totalTimesPregnant *= inverseCount; 
    }*/

#endregion
