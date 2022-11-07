using UnityEngine;
using System.Collections.Generic;
using Playcraft;

/// Stats
public struct PerformanceData {
    //public List<CreatureDataPoint> creatureDataPointsList; // won't even need this eventually with bezier curve approach
    public int maxNumDataPointEntries;
    //old:
    public float minScoreValue;
    public float maxScoreValue;
    //NEW:
    public BezierCurve bezierCurve;
    public float p0y;
    public float p0x;
    public float p1y;
    public float p1x;

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

    //public float speciesAvgLifespanAtTimeOfBirth;// use curve.p0y
        
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
            //
            //creatureDataPointsList = new List<SpeciesDataPoint>();
            //maxNumDataPointEntries = 32; //***EAC
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
    public CandidateAgentData parentCandidate;
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
        //parentCandidate = 
        candidateEventDataList = new List<CandidateEventData>();
        performanceData = new PerformanceData();
        performanceData.minScoreValue = float.PositiveInfinity;
        performanceData.maxScoreValue = -1f;
    }
    
    public CandidateAgentData(AgentGenome genome, int speciesID, CandidateAgentData parentCand) {
        //Debug.Log("NewCandidateData: " + MasterGenomePool.nextCandidateIndex.ToString());
        MasterGenomePool.nextCandidateIndex++;
        causeOfDeath = "Alive!";
        candidateID = MasterGenomePool.nextCandidateIndex;
        this.speciesID = speciesID;
        candidateGenome = genome;
        numCompletedEvaluations = 0;
        allEvaluationsComplete = false;  
        isBeingEvaluated = false;
        if (parentCand != null) {
            parentCandidate = parentCand;
            parentID = parentCand.candidateID;
        }
        else {
            parentID = -1;
        }
        candidateEventDataList = new List<CandidateEventData>();
        performanceData = new PerformanceData();
        performanceData.minScoreValue = float.PositiveInfinity;
        performanceData.maxScoreValue = -1f;

        //SetCurvePointStart(UIManager.instance.historyPanelUI.graphBoundsMinX, UIManager.instance.historyPanelUI.graphBoundsMinY);
        //SetCurvePointEnd(UIManager.instance.historyPanelUI.graphBoundsMaxX, UIManager.instance.historyPanelUI.graphBoundsMaxY);
        //performanceData.scoreStart = 0f;
        
        /*if(SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList.Count < 1) {
            performanceData.scoreStart = 0f;
        }
        else {
            if (speciesID < 0) {

            }
            else {
                performanceData.scoreStart = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[speciesID].avgLifespan;
            }
        }*/
        //performanceData.timeStart = SimulationManager.instance.simAgeTimeSteps; // ??? ***EAC  set this at hatching time?
    }
    
    

    public void UpdateDisplayCurve() {
        if(performanceData.bezierCurve == null) {
            performanceData.bezierCurve = new BezierCurve();
            //performanceData.timeStart = SimulationManager.instance.simAgeTimeSteps;
            //performanceData.scoreStart = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[speciesID].avgLifespan;
        }
        //performanceData.timeStart = performanceData.timeStepHatched;
        performanceData.bezierCurve.SetPoints(new Vector3(performanceData.p0x, performanceData.p0y, 0f), 
                                            new Vector3(performanceData.p0x, performanceData.p1y, 0f), 
                                            new Vector3(performanceData.p1x, performanceData.p1y, 0f));
        
    }
    /*
    public void AddNewDataPoint(int timestep, float score) {

        CreatureDataPoint dataPoint = new CreatureDataPoint();
        dataPoint.timestep = timestep;
        dataPoint.lifespan = score;
        if(performanceData.creatureDataPointsList == null) {
            performanceData.creatureDataPointsList = new List<CreatureDataPoint>();
            performanceData.maxNumDataPointEntries = 32; // ***EAC BAD! move to simManager or smth
            for(int i = 0; i < performanceData.maxNumDataPointEntries; i++) {
                performanceData.creatureDataPointsList.Add(new CreatureDataPoint()); // not needed?
            }
             
        }
        if(score < performanceData.minScoreValue) {
            performanceData.minScoreValue = score;
        }
        if(score > performanceData.maxScoreValue) {
            performanceData.maxScoreValue = score;
        }
        performanceData.creatureDataPointsList.Add(dataPoint);
        SlideDataPoints(score);
        if (performanceData.creatureDataPointsList.Count > performanceData.maxNumDataPointEntries) {
            MergeDataPoints();            
        }
        
        //for(int dP = 0; dP < performanceData.creatureDataPointsList.Count; dP++) {
            //int count = Mathf.Max(1, performanceData.creatureDataPointsList.Count - 1);
            //float frac = (float)dP / (float)count;
           // performanceData.creatureDataPointsList[dP] = score;
            //performanceData.creatureDataPointsList[dP].lifespan = Mathf.Lerp(performanceData.creatureDataPointsList[dP].lifespan, performanceData.creatureDataPointsList[performanceData.creatureDataPointsList.Count - 1].lifespan, frac);
       // }
        //float bufferWidth = 75f;
        //maxScoreValue = dataPoint.lifespan + bufferWidth;
        //minScoreValue = dataPoint.lifespan - bufferWidth;
        //if (this.speciesID == 0 && SimulationManager.instance.simAgeTimeSteps < 10000) {
        //minScoreValue = Mathf.Max(0f, performanceData.creatureDataPointsList[0].lifespan - bufferWidth);
        //}
    }
    private void MergeDataPoints() {
        float closestPairDistance = float.PositiveInfinity;
        int closestPairStartIndex = 1;
        for(int i = 1; i < performanceData.creatureDataPointsList.Count - 2; i++) { // don't include first or last point
            float distFront = performanceData.creatureDataPointsList[i + 1].timestep - performanceData.creatureDataPointsList[i].timestep;
            float distBack = performanceData.creatureDataPointsList[i].timestep - performanceData.creatureDataPointsList[i - 1].timestep;

            float multiplier = 2f;
            float bonusDist = (multiplier - (float)i * multiplier / (float)(performanceData.creatureDataPointsList.Count - 1)) * 1f;
            float dist = (distFront + distBack) / bonusDist;
            if(dist < closestPairDistance) {
                closestPairDistance = dist;
                closestPairStartIndex = i;
            }
        }
        CreatureDataPoint avgData = new CreatureDataPoint();
        avgData.timestep = (performanceData.creatureDataPointsList[closestPairStartIndex].timestep + performanceData.creatureDataPointsList[closestPairStartIndex + 1].timestep) / 2f;
        avgData.lifespan = (performanceData.creatureDataPointsList[closestPairStartIndex].lifespan + performanceData.creatureDataPointsList[closestPairStartIndex + 1].lifespan) / 2f;
                
        performanceData.creatureDataPointsList[closestPairStartIndex + 1] = avgData;
        performanceData.creatureDataPointsList.RemoveAt(closestPairStartIndex);
        
        
    }

    private void SlideDataPoints(float targetScore) {
        if (performanceData.creatureDataPointsList.Count < 2)
            return;

        for (int i = 1; i < performanceData.creatureDataPointsList.Count-1; i++) {           
            performanceData.creatureDataPointsList[i].lifespan = Mathf.Lerp(performanceData.creatureDataPointsList[i].lifespan, targetScore, 0.4f * (float)i / (float)(performanceData.creatureDataPointsList.Count-1)); ;
        }
    }
    public void SmoothDataPoints(int numIter) {
        
        for(int j = 0; j < numIter; j++) {
            List<CreatureDataPoint> swapDataPointsList = new List<CreatureDataPoint>();
            for (int i = 0; i < performanceData.creatureDataPointsList.Count; i++) {
                int indexPrev = Mathf.Max(0, i - 1);
                int indexNext = Mathf.Min(i, performanceData.creatureDataPointsList.Count - 1);
                CreatureDataPoint pointPrev = performanceData.creatureDataPointsList[indexPrev];
                CreatureDataPoint pointCur = performanceData.creatureDataPointsList[i];
                CreatureDataPoint pointNext = performanceData.creatureDataPointsList[indexNext];
                if (i == 0) {
                    
                }
                else {                
                    pointCur.lifespan = (pointPrev.lifespan + pointCur.lifespan + pointNext.lifespan) / 3f;
                }
                swapDataPointsList.Add(pointCur);
            }
            
            performanceData.creatureDataPointsList.Clear();
            for (int i = 0; i < swapDataPointsList.Count; i++) {
                performanceData.creatureDataPointsList.Add(swapDataPointsList[i]);
            }
        }
        
        
        
    }

    public void SmoothDataPoints(float proportion) {
        if (proportion > 1f) return;
        if (proportion < 0f) return;
        
        List<CreatureDataPoint> swapDataPointsList = new List<CreatureDataPoint>();
        for (int i = 0; i < performanceData.creatureDataPointsList.Count; i++) {
            int indexPrev = Mathf.Max(1, i - 1);
            int indexNext = Mathf.Min(i, performanceData.creatureDataPointsList.Count - 1);
            CreatureDataPoint pointPrev = performanceData.creatureDataPointsList[indexPrev];
            CreatureDataPoint pointCur = performanceData.creatureDataPointsList[i];
            CreatureDataPoint pointNext = performanceData.creatureDataPointsList[indexNext];
                
            float gamble = UnityEngine.Random.Range(0f, 1f);            
            if(gamble < proportion && i>1) {
                pointCur.lifespan = (pointPrev.lifespan + pointCur.lifespan + pointNext.lifespan) / 3f;
            }                
            swapDataPointsList.Add(pointCur);
        }
            
        performanceData.creatureDataPointsList.Clear();
        for (int i = 0; i < swapDataPointsList.Count; i++) {
            performanceData.creatureDataPointsList.Add(swapDataPointsList[i]);
        }
        
    }
    */
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
