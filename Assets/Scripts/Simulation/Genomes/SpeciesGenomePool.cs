using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Playcraft;

[Serializable]
public class SpeciesGenomePool 
{
    Lookup lookup => Lookup.instance;

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life
    public int depthLevel;

    [NonSerialized]
    public MutationSettingsInstance mutationSettings;  // Or remove this later to keep everything saveable?

    public string speciesName;
    //public CandidateAgentData representativeCandidate;
    public CandidateAgentData foundingCandidate;
    public CandidateAgentData longestLivedCandidate;
    public CandidateAgentData mostEatenCandidate;
    //public Lineage Map --> avg genome over time
    public List<CandidateAgentData> hallOfFameGenomesList;
    public List<CandidateAgentData> leaderboardGenomesList; // potential parents ranked
    public List<CandidateAgentData> candidateGenomesList; // *To be born

    public int maxLeaderboardGenomePoolSize = 32;    
    public int numAgentsEvaluated = 0;
     
    public int yearCreated = -1;
    public int timeStepCreated = 1;
    public int timeStepExtinct = 2000000000;

    // Records:
    public int recordLongestLife = 0;
    public CandidateAgentData recordHolderLongestLife;
    public float recordMostEaten = 0f;
    public CandidateAgentData recordHolderMostEaten;
    public int recordMostPregnancies = 0;
    public CandidateAgentData recordHolderMostPregnancies;
    public int recordMostKills = 0;
    public CandidateAgentData recordHolderMostKills;
    public int recordBiggestBrain = 0;
    public CandidateAgentData recordHolderBiggestBrain;
    public int recordDefender = 0;
    public CandidateAgentData recordHolderDefender;
    public int recordDasher = 0;
    public CandidateAgentData recordHolderDasher;
        
    public CandidateAgentData avgCandidateData;
        
    [SerializeField]
    public List<SpeciesDataPoint> speciesDataPointsList;

    public Material coatOfArmsMat;

    public bool isFlaggedForExtinction = false;
    public bool isFlaggedForEndangered = false;
    public int timestepsEndangeredCounter = 0;
    public bool isExtinct = false;
    public bool isStillEvaluating = true;

    public float avgLifespan;
    public float curActiveGraphBoundsMinX;
    public float curActiveGraphBoundsMinY;
    public float curActiveGraphBoundsMaxX;
    public float curActiveGraphBoundsMaxY;

    private int maxNumDataPointEntries = 64;
    public float speciesAllTimeMinScore;
    public float speciesAllTimeMaxScore;
    public float speciesCurAliveMinScore;
    public float speciesCurAliveMaxScore;
       
    MutationSettingsInstance _cachedNoneMutationSettings;
    MutationSettingsInstance cachedNoneMutationSettings => _cachedNoneMutationSettings ?? GetSetNoneMutationSettings();
    MutationSettingsInstance GetSetNoneMutationSettings() { return _cachedNoneMutationSettings = lookup.GetMutationSettingsCopy(MutationSettingsId.None); }

    public SpeciesGenomePool(int ID, int parentID, int year, int timeStep, MutationSettingsInstance settings) {
        yearCreated = year;
        speciesID = ID;
        parentSpeciesID = parentID;
        mutationSettings = settings;
        timeStepCreated = timeStep;
        timeStepExtinct = 2000000000;        
        speciesName = ID.ToString();
    }

    private void InitShared() {
        isFlaggedForExtinction = false;
        isExtinct = false;
               
        candidateGenomesList = new List<CandidateAgentData>();
        leaderboardGenomesList = new List<CandidateAgentData>();        
        hallOfFameGenomesList = new List<CandidateAgentData>();

        CreateNewAverageCandidate();
        speciesDataPointsList = new List<SpeciesDataPoint>();
        isStillEvaluating = true;
        avgLifespan = 1f;
    }

    
    public void FirstTimeInitialize(CandidateAgentData foundingCandidate, int depth) {
                
        longestLivedCandidate = foundingCandidate;
        mostEatenCandidate = foundingCandidate;

        InitShared();
        depthLevel = depth;
        //Vector3 newHue = Random.insideUnitSphere;
              
        this.foundingCandidate = foundingCandidate;
        this.foundingCandidate.candidateGenome.name = MutateName(foundingCandidate.candidateGenome.name);
        this.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary = UtilityMutationFunctions.GetMutatedVector3Additive(foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary, 1f, 0.3725f, 0f, 1f);
        this.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary = UtilityMutationFunctions.GetMutatedVector3Additive(foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary, 1f, 0.45f, 0f, 1f);
         
        //foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.BlendHue(newHue, 0.35f);
        Vector3 foundingHuePrimary = this.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        Vector3 foundingHueSecondary = this.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
        
        //string debugTxt = "";
        for (int i = 0; i < 32; i++) {            
            CandidateAgentData childCandidate = Mutate(this.foundingCandidate, true, true);
            childCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary = foundingHuePrimary; // needed??
            childCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary = foundingHueSecondary;
            
            candidateGenomesList.Add(childCandidate);
            leaderboardGenomesList.Add(childCandidate);
            //debugTxt += "" + candidate.candidateGenome.brainGenome.linkList[0].weight.ToString("F2") + "  ";
        }
        
        if(this.speciesID != 0) {                
            avgLifespan = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[parentSpeciesID].avgLifespan; // starting value// avgCandidateData.performanceData.totalTicksAlive;
                
        }
        else {
            avgLifespan = 1500f;
        }
        // create species CoatOfArms:
        coatOfArmsMat = new Material(TheRenderKing.instance.coatOfArmsShader);
        //coatOfArmsTex = TheRenderKing.instance.GenerateSpeciesCoatOfArms(foundingGenome.candidateGenome.bodyGenome.appearanceGenome);
        coatOfArmsMat.SetPass(0);
        coatOfArmsMat.SetTexture("_MainTex", TheRenderKing.instance.shapeTex);
        coatOfArmsMat.SetTexture("_PatternTex", TheRenderKing.instance.patternTex);
        coatOfArmsMat.SetFloat("_PatternX", this.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeX);
        coatOfArmsMat.SetFloat("_PatternX", this.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeY);
        coatOfArmsMat.SetColor("_TintPri", new Color(foundingHuePrimary.x, foundingHuePrimary.y, foundingHuePrimary.z));
        coatOfArmsMat.SetColor("_TintSec", new Color(foundingHueSecondary.x, foundingHueSecondary.y, foundingHueSecondary.z));
        coatOfArmsMat.SetFloat("_IsSelected", 0f);
        //coatOfArmsMat.SetFloat("_IsEndangered", isFlaggedForExtinction ? 1f : 0f);
    }
       
    public void ProcessExtinction(int curTimeStep) {
        isExtinct = true;
        timeStepExtinct = curTimeStep;
    }

    private void CreateNewAverageCandidate() {
        if(avgCandidateData == null) {
            AgentGenome blankGenome = new AgentGenome(0f, 0);
            avgCandidateData = new CandidateAgentData(blankGenome, speciesID, null);
        }
        
        avgCandidateData.SetToAverage(leaderboardGenomesList);
    }

    public CandidateAgentData UpdateOldestActiveCandidateAndGraphBounds() { // ***EAC use time alive directly, deprecate p0y
        float earliestTimestepBorn = SimulationManager.instance.simAgeTimeSteps;
        CandidateAgentData oldestLivingCandidate = null;
        float minX = SimulationManager.instance.simAgeTimeSteps;
        float minY = float.PositiveInfinity;
        float maxX = 0f;
        float maxY = 0f;
        bool nonZeroX = false;
        bool nonZeroY = false;
       // float oldestTimestep = earliestTimestepBorn;
        for(int i = 0; i < candidateGenomesList.Count; i++) {
            if(candidateGenomesList[i].isBeingEvaluated) {
                if(candidateGenomesList[i].performanceData.timeStepHatched < earliestTimestepBorn) {
                    earliestTimestepBorn = candidateGenomesList[i].performanceData.timeStepHatched;
                    oldestLivingCandidate = candidateGenomesList[i];
                }
                //MINIMUMS
                if(candidateGenomesList[i].performanceData.timeStepHatched < minX) {
                    if(candidateGenomesList[i].performanceData.timeStepHatched <= 0.001f) {

                    }
                    else {
                        nonZeroX = true;
                        minX = candidateGenomesList[i].performanceData.timeStepHatched;
                    }
                    
                }
                
                if(candidateGenomesList[i].performanceData.timeStepDied > maxX) {
                    maxX = candidateGenomesList[i].performanceData.timeStepDied;
                }
                
                if(!nonZeroX) {
                    minX = maxX;
                }
                if(!nonZeroY) {
                    minY = maxY;
                }
            }
        }
        if(oldestLivingCandidate == null) {
            //curGraphBoundsMinX = 11;
            Debug.LogError("no candidates found!");
        }
        else {
            curActiveGraphBoundsMinX = minX;
            curActiveGraphBoundsMinY = minY;
            curActiveGraphBoundsMaxX = maxX;
            curActiveGraphBoundsMaxY = maxY;
        }
        //Debug.Log("minX " + minX + "\nMinY " + minY + "\nMaxX " + maxX + "\nMaxY " + maxY);
        
        return oldestLivingCandidate;
    }
    public void AddNewDataPoint(int timestep) 
    {
        //ADD NEW SPECIES DATA POINT::::::
        CreateNewAverageCandidate(); // ***EC figure this out???        
        SpeciesDataPoint dataPoint = new SpeciesDataPoint();
        dataPoint.timestep = timestep;
        dataPoint.lifespan = avgLifespan; // avgCandidateData.performanceData.totalTicksAlive;
              
        speciesDataPointsList.Add(dataPoint);
        if(speciesDataPointsList.Count > maxNumDataPointEntries) {
            MergeDataPoints();            
        }
        

        // BELOW HERE REPLACE:::
        if(dataPoint.lifespan < speciesAllTimeMinScore) {
            speciesAllTimeMinScore = dataPoint.lifespan;
        }
        if(dataPoint.lifespan > speciesAllTimeMaxScore) {
            speciesAllTimeMaxScore = dataPoint.lifespan;
        }


        //ADD NEW CREATURE DATAPOINTS:::::
        if(candidateGenomesList.Count != 0) {
            speciesCurAliveMinScore = float.PositiveInfinity;
            speciesCurAliveMaxScore = 1;
        }
        /*
        for(int i = 0; i < candidateGenomesList.Count; i++) {
            
            float frac = (float)i / (float)Mathf.Max(1, candidateGenomesList.Count - 1);
            PerformanceData perf = candidateGenomesList[i].performanceData;
            perf.p1x = perf.timeStepDied;
            
            //perf.p1y = Mathf.Lerp(UIManager.instance.historyPanelUI.graphBoundsMinY, UIManager.instance.historyPanelUI.graphBoundsMaxY, lineFrac);
            
            candidateGenomesList[i].UpdateDisplayCurve();
            
            if(avgLifespan < speciesCurAliveMinScore) {
                speciesCurAliveMinScore = avgLifespan;
            }
            if(avgLifespan > speciesCurAliveMaxScore) {
                speciesCurAliveMaxScore = avgLifespan;
            }                      
        }*/
    }
    private void MergeDataPoints() {
        float closestPairDistance = float.PositiveInfinity;
        int closestPairStartIndex = 1;
        for(int i = 1; i < speciesDataPointsList.Count - 2; i++) { // don't include first or last point
            //float distFront = (speciesDataPointsList[i + 1].timestep + speciesDataPointsList[i + 1].lifespan) - (speciesDataPointsList[i].timestep + speciesDataPointsList[i].lifespan);
            //float distBack = (speciesDataPointsList[i].timestep + speciesDataPointsList[i].lifespan) - (speciesDataPointsList[i - 1].timestep + speciesDataPointsList[i - 1].lifespan);
            float distFront = speciesDataPointsList[i + 1].timestep - speciesDataPointsList[i].timestep;
            float distBack = speciesDataPointsList[i].timestep - speciesDataPointsList[i - 1].timestep;

            float multiplier = 25f;
            float bonusDist = (multiplier - (float)i * multiplier / (float)(speciesDataPointsList.Count - 1)) * 1f;
            float dist = (distFront + distBack) / bonusDist;
            if(dist < closestPairDistance) {
                closestPairDistance = dist;
                closestPairStartIndex = i;
            }
        }
        SpeciesDataPoint avgData = new SpeciesDataPoint();
        avgData.timestep = (speciesDataPointsList[closestPairStartIndex].timestep + speciesDataPointsList[closestPairStartIndex + 1].timestep) / 2f;
        avgData.lifespan = (speciesDataPointsList[closestPairStartIndex].lifespan + speciesDataPointsList[closestPairStartIndex + 1].lifespan) / 2f;
                
        speciesDataPointsList[closestPairStartIndex + 1] = avgData;
        speciesDataPointsList.RemoveAt(closestPairStartIndex);
        
    }
    
    public void UpdateAvgData(CandidateAgentData candidateData) {
        float numPoints = 32f; // rolling avg over this many creatures
        avgLifespan = avgLifespan * ((numPoints - 1f) / numPoints) + candidateData.performanceData.totalTicksAlive * (1f / numPoints);
        if (numAgentsEvaluated >= numPoints && isStillEvaluating) {
            isStillEvaluating = false;
                      
            Debug.Log("Species " + speciesID + " is done with initial evaluation. avgLife: " + avgLifespan);
        }
        else {
            if(speciesID == 0 && isStillEvaluating) {
                foreach(var dP in speciesDataPointsList) {
                    //dP.lifespan = Mathf.Lerp(dP.lifespan, avgLifespan, 0.05f);
                }
            }
        }
        //SmoothDataPoints(0.05f);
        
    }

    public void SmoothDataPoints(float proportion) {
        if (proportion > 1f) return;
        if (proportion < 0f) return;
        
        List<SpeciesDataPoint> swapDataPointsList = new List<SpeciesDataPoint>();
        //int startIndex = 1; // Mathf.Min(1, speciesDataPointsList.Count);
        //if (speciesDataPointsList.Count < 2) return;
        for (int i = 0; i < speciesDataPointsList.Count; i++) {
            int indexPrev = Mathf.Max(1, i - 1);
            int indexNext = Mathf.Min(i, speciesDataPointsList.Count - 1);
            SpeciesDataPoint pointPrev = speciesDataPointsList[indexPrev];
            SpeciesDataPoint pointCur = speciesDataPointsList[i];
            SpeciesDataPoint pointNext = speciesDataPointsList[indexNext];
            float gamble = UnityEngine.Random.Range(0f, 1f);
            
            if(gamble < proportion) {
                pointCur.lifespan = (pointPrev.lifespan + pointCur.lifespan + pointNext.lifespan) / 3f;
            }                
            swapDataPointsList.Add(pointCur);
        }
        speciesDataPointsList.Clear();

        for (int i = 0; i < swapDataPointsList.Count; i++) {
            speciesDataPointsList.Add(swapDataPointsList[i]);
        }
        
        
    }
    /// Finds an unborn agent ready to be respawned
    public CandidateAgentData GetNextAvailableCandidate() 
    {
        if (candidateGenomesList.Count <= 0)
        {
            var candidateData = new CandidateAgentData(foundingCandidate, speciesID);
            Debug.LogError($"GetNextAvailableCandidate(): representativeGenome! {candidateData}");
            return candidateData;            
        }
        
        foreach (var candidate in candidateGenomesList)
            if (!candidate.isBeingEvaluated)
                return candidate;
        
        return null;
    }
    
    public void ProcessCompletedCandidate(CandidateAgentData candidateData, MasterGenomePool masterGenomePool) 
    {
        numAgentsEvaluated++;
        UpdateAvgData(candidateData);

        leaderboardGenomesList.Insert(0, candidateData);  // place at front of leaderboard list (genomes eligible for being parents)
        if(leaderboardGenomesList.Count > maxLeaderboardGenomePoolSize) {
            leaderboardGenomesList.RemoveAt(leaderboardGenomesList.Count - 1);
        }

        //int beforeCount = candidateGenomesList.Count;
        int listIndex = -1;
        for (int i = 0; i < candidateGenomesList.Count; i++) {
            if(candidateData.candidateID == candidateGenomesList[i].candidateID) {
                listIndex = i;
            }
        }
        //Debug.Log("Removed! " + beforeCount.ToString() + " #: " + listIndex.ToString() + ", candID: " + candidateData.candidateID.ToString());
        if (listIndex > -1) 
        {
            //Debug.Log("RemoveAt(" + listIndex.ToString() + "),[" + candidateGenomesList[listIndex].candidateID.ToString() + "], candID: " + candidateData.candidateID.ToString() + ", SpeciesPool: " + this.speciesID.ToString() + ", CDSID: " + candidateData.speciesID.ToString());
            candidateGenomesList.RemoveAt(listIndex);  // Will this work? never used this before

            masterGenomePool.debugRecentlyDeletedCandidateIDsList.Insert(0, candidateData.candidateID);
            if (masterGenomePool.debugRecentlyDeletedCandidateIDsList.Count > 512) {
                masterGenomePool.debugRecentlyDeletedCandidateIDsList.RemoveAt(masterGenomePool.debugRecentlyDeletedCandidateIDsList.Count - 1);
            }
        }
        else 
        {
            Debug.LogError(candidateGenomesList.Count <= 0 ? "META-ERROR NO INDEX FOUND! " : 
                $"ERROR NO INDEX FOUND! {candidateData.candidateID}, species: {speciesID}, CDSID: {candidateData.speciesID}, [0]: {candidateGenomesList[0].candidateID}");

            masterGenomePool.GlobalFindCandidateID(candidateData.candidateID); // temp debug
        }
        
        // *** NOTE! *** List.Remove() was unreliable - worked sometimes but not others? still unsure about it
    }

    public void AddNewCandidateGenome(CandidateAgentData newCand) {
        //Debug.Log("AddedNewCandidate! " + candidateGenomesList.Count.ToString());
        //CandidateAgentData newCandidateData = new CandidateAgentData(newCand, speciesID); //???
        //candidateGenomesList.Insert(0, newCand);// newCandidateData);
        candidateGenomesList.Add(newCand);
    }

    public CandidateAgentData GetCandidateFromFitnessLottery() {
        int numCandidates = leaderboardGenomesList.Count;
        float[] rankedFitnessScoresArray = new float[numCandidates];
        int[] rankedIndicesList = new int[numCandidates];
        float totalFitness = 0f;

        //string leaderboardGenomesListString = "LEADERBOARD GENOMES (" + speciesID.ToString() + ")";
        // Rank current leaderBoard list based on score
        for (int i = 0; i < numCandidates; i++) {
            float fitnessScore = 0.00001f;
            fitnessScore += leaderboardGenomesList[i].performanceData.totalTicksAlive;
            rankedFitnessScoresArray[i] = fitnessScore;
            rankedIndicesList[i] = i;
            totalFitness += fitnessScore;

            //leaderboardGenomesListString += "\n#" + i.ToString() + ", score= " + fitnessScore.ToString();
        }
        //Debug.Log(leaderboardGenomesListString);
        
        // Sort By Fitness
        for (int i = 0; i < numCandidates - 1; i++) 
        {
            for (int j = 0; j < numCandidates - 1; j++) 
            {
                float swapFitA = rankedFitnessScoresArray[j];
                float swapFitB = rankedFitnessScoresArray[j + 1];
                int swapIdA = rankedIndicesList[j];
                int swapIdB = rankedIndicesList[j + 1];

                // bigger is better now after inversion
                if (swapFitA < swapFitB) 
                { 
                    rankedFitnessScoresArray[j] = swapFitB;
                    rankedFitnessScoresArray[j + 1] = swapFitA;
                    rankedIndicesList[j] = swapIdB;
                    rankedIndicesList[j + 1] = swapIdA;
                }
            }
        }

        int selectedIndex = 0;
        
        // generate random lottery value between 0f and totalFitness:
        float lotteryValue = Random.Range(0f, totalFitness);
        float currentValue = 0f;
        for (int i = 0; i < numCandidates; i++) {
            if (lotteryValue >= currentValue && lotteryValue < (currentValue + rankedFitnessScoresArray[i])) {
                // Jackpot!
                selectedIndex = rankedIndicesList[i];
                //Debug.Log("Selected: " + selectedIndex.ToString() + "! (" + i.ToString() + ") fit= " + currentValue.ToString() + "--" + (currentValue + (1f - rankedFitnessList[i])).ToString() + " / " + totalFitness.ToString() + ", lotto# " + lotteryValue.ToString() + ", fit= " + (1f - rankedFitnessList[i]).ToString());
            }
            currentValue += rankedFitnessScoresArray[i]; // add this agent's fitness to current value for next check
        }
        //return selectedIndex;
        // choose genome by lottery
        CandidateAgentData parentCandidate = leaderboardGenomesList[selectedIndex];

        return parentCandidate;
    }

    // Avoid use of local variables in Mutate() for efficiency
    MutationSettingsInstance tempMutationSettings;
    
    string MutateName(string original)
    {
        var newName = "";

        foreach (var letter in original) {
            float randChance1 = Random.Range(0f, 1f);
            if (randChance1 < 0.35) {
                newName += RandomStatics.GetRandomLetter();  
                
                if (randChance1 < 0.05) {
                    newName += RandomStatics.GetRandomLetter();          
                }
            }            
            else if (randChance1 <= 0.95f) {
                newName += letter; 
            }
        }
        
        return newName;
    }
     
    public CandidateAgentData Mutate(CandidateAgentData parentCandidate, bool bodySettings, bool brainSettings) 
    {
        AgentGenome parentGenome = parentCandidate.candidateGenome;
        string newName = GetMutatedName(parentGenome.name);

        tempMutationSettings = bodySettings ? mutationSettings : cachedNoneMutationSettings;
        BodyGenome newBodyGenome = new BodyGenome(parentGenome.bodyGenome, tempMutationSettings);

        tempMutationSettings = brainSettings ? mutationSettings : cachedNoneMutationSettings;
        BrainGenome newBrainGenome = new BrainGenome(parentGenome.brainGenome, newBodyGenome, tempMutationSettings);
        
        return new CandidateAgentData(new AgentGenome(newBodyGenome, newBrainGenome, parentGenome.generationCount + 1, newName), parentCandidate.speciesID, parentCandidate);
    }
    
    string GetMutatedName(string parentName)
    {
        int randomIndex = Random.Range(0, parentName.Length - 1);
        
        string frontHalf = parentName.Substring(0, randomIndex);
        string middleChar = RandomStatics.CoinToss(.05f) ?
            RandomStatics.GetRandomLetter() :
            parentName.Substring(randomIndex, 1);
        frontHalf += middleChar;    
        string backHalf = parentName.Substring(randomIndex + 1);
        
        return RandomStatics.CoinToss(.025f) ? backHalf + frontHalf : frontHalf + backHalf;    
    }
    
    public void UpdateLongestLife(Agent agent)
    {
        if(agent.ageCounter <= recordLongestLife) 
            return;

        recordLongestLife = agent.ageCounter;
        recordHolderLongestLife = agent.candidateRef;

        if(numAgentsEvaluated > maxLeaderboardGenomePoolSize) {
            hallOfFameGenomesList.Add(agent.candidateRef);
        }
        //Debug.Log("it works! " + speciesPool.recordLongestLife.ToString() + ", candidate: " + agentRef.candidateRef.candidateID.ToString() + ", species: " + agentRef.candidateRef.speciesID.ToString());
    }
    
    public void UpdateMostEaten(Agent agent)
    {
        float totalEaten = agent.totalEaten;
        
        if (totalEaten <= recordMostEaten) 
            return;

        recordMostEaten = totalEaten;
        recordHolderMostEaten = agent.candidateRef;

        if (numAgentsEvaluated > maxLeaderboardGenomePoolSize) {
            hallOfFameGenomesList.Add(agent.candidateRef);
        }
    }
    
    public Axon GetLeaderboardLinkGenome(int index, int linkedListIndex)
    {
        AgentGenome genome = GetLeaderboardGenome(index);

        if (genome.brainGenome.axonCount > linkedListIndex)
            return null;
            
        return genome.brainGenome.axons.all[linkedListIndex];
    }
    
    public AgentGenome GetLeaderboardGenome(int index)
    {
        if (index >= leaderboardGenomesList.Count)
            return null;
        
        return leaderboardGenomesList[index].candidateGenome;        
    }
    
   /* public CandidateAgentData GetFocusedCandidate(SelectionGroup group, int index) {
        switch (group) {
            case SelectionGroup.Founder: return foundingCandidate;
            case SelectionGroup.Representative: return representativeCandidate;
            case SelectionGroup.LongestLived: return longestLivedCandidate;
            case SelectionGroup.MostEaten: return mostEatenCandidate;
            case SelectionGroup.HallOfFame: return hallOfFameGenomesList[index];
            case SelectionGroup.Leaderboard: return leaderboardGenomesList[index];
            case SelectionGroup.Candidates: return candidateGenomesList[index];
            default: Debug.LogError("Invalid selection group: " + group); return null;
        }
    }*/
    
    public int GetNumberAgentsEvaluated()
    {
        int result = 0;
        
        foreach (var agent in candidateGenomesList) 
            if (agent.isBeingEvaluated) 
                result++;

        return result;
    }
}

#region Dead code (please remove)
    /*
    public AgentGenome GetNewMutatedGenome() {

        int numCandidates = leaderboardGenomesList.Count;
        float[] rankedFitnessScoresArray = new float[numCandidates];
        int[] rankedIndicesList = new int[numCandidates];
        float totalFitness = 0f;

        // Rank current leaderBoard list based on score
        for (int i = 0; i < numCandidates; i++) {
            float fitnessScore = 0f;
            for(int j = 0; j < leaderboardGenomesList[i].evaluationScoresList.Count; j++) {
                fitnessScore += (float)leaderboardGenomesList[i].evaluationScoresList[j];
            }
            rankedFitnessScoresArray[i] = fitnessScore;
            rankedIndicesList[i] = i;
            totalFitness += fitnessScore;
        }
        
        // Sort By Fitness
        for (int i = 0; i < numCandidates - 1; i++) {
            for (int j = 0; j < numCandidates - 1; j++) {
                float swapFitA = rankedFitnessScoresArray[j];
                float swapFitB = rankedFitnessScoresArray[j + 1];
                int swapIdA = rankedIndicesList[j];
                int swapIdB = rankedIndicesList[j + 1];

                if (swapFitA < swapFitB) {  // bigger is better now after inversion
                    rankedFitnessScoresArray[j] = swapFitB;
                    rankedFitnessScoresArray[j + 1] = swapFitA;
                    rankedIndicesList[j] = swapIdB;
                    rankedIndicesList[j + 1] = swapIdA;
                }
            }
        }

        int selectedIndex = 0;
        
        // generate random lottery value between 0f and totalFitness:
        float lotteryValue = UnityEngine.Random.Range(0f, totalFitness);
        float currentValue = 0f;
        for (int i = 0; i < numCandidates; i++) {
            if (lotteryValue >= currentValue && lotteryValue < (currentValue + rankedFitnessScoresArray[i])) {
                // Jackpot!
                selectedIndex = rankedIndicesList[i];
                //Debug.Log("Selected: " + selectedIndex.ToString() + "! (" + i.ToString() + ") fit= " + currentValue.ToString() + "--" + (currentValue + (1f - rankedFitnessList[i])).ToString() + " / " + totalFitness.ToString() + ", lotto# " + lotteryValue.ToString() + ", fit= " + (1f - rankedFitnessList[i]).ToString());
            }
            currentValue += rankedFitnessScoresArray[i]; // add this agent's fitness to current value for next check
        }
        //return selectedIndex;
        // choose genome by lottery
        AgentGenome parentGenome = leaderboardGenomesList[selectedIndex].candidateGenome;
        AgentGenome childGenome = new AgentGenome();
        // return genome

        BodyGenome newBodyGenome = new BodyGenome();
        BrainGenome newBrainGenome = new BrainGenome();

        BodyGenome parentBodyGenome = parentGenome.bodyGenome;
        BrainGenome parentBrainGenome = parentGenome.brainGenome;

        // A)
        // 1) Copy Brain, mutate weights, add/remove axons & hid neurons
        // 2) Mutate Body, Add/Remove Sensors+Effectors
        // 3) Cleanup brain based on new sensors/effectors (In/Out Neurons)

        // B)   ************* STARTING WITH OPTION B!!! *************
        // 1) Copy & Mutate Body Genome, Edit Attrs, Add/Remove Sensors/Effectors
        // 2) Rebuild Brain's BodyNeurons (assumes unique ID for every module/neuron), Copy Axons & HiddenNeurons
        // 3) Remove Vestigial Links

        // C) Body & Brain in parallel, module by module?        

        newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, mutationSettingsRef);
        newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, newBodyGenome, mutationSettingsRef);
        
        childGenome.bodyGenome = newBodyGenome; 
        childGenome.brainGenome = newBrainGenome; 

        return childGenome;
    }
    */
#endregion