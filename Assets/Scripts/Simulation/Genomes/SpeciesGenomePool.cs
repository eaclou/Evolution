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
    public CandidateAgentData representativeCandidate;
    public CandidateAgentData foundingCandidate;
    public CandidateAgentData longestLivedCandidate;
    public CandidateAgentData mostEatenCandidate;
    //public Lineage Map --> avg genome over time
    public List<CandidateAgentData> hallOfFameGenomesList;
    public List<CandidateAgentData> leaderboardGenomesList;
    public List<CandidateAgentData> candidateGenomesList;

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
        
    //public List<PerformanceData> avgPerformanceDataYearList;
    //public PerformanceData avgPerformanceData;
    public List<CandidateAgentData> avgCandidateDataYearList;
    public CandidateAgentData avgCandidateData;

    //private Texture2D coatOfArmsTex;
    public Material coatOfArmsMat;

    //public float avgLifespan = 0f;
    public bool isFlaggedForExtinction = false;
    public bool isExtinct = false;
    
    public CritterModuleAppearanceGenome appearanceGenome => 
        foundingCandidate.candidateGenome.bodyGenome.appearanceGenome;
        
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

        avgCandidateDataYearList = new List<CandidateAgentData>(); 
        CreateNewAverageCandidate(); // avgCandidateData = new CandidateAgentData();
    }
    
    public void FirstTimeInitialize(CandidateAgentData foundingGenome, int depth) {
        foundingCandidate = foundingGenome;        
        longestLivedCandidate = foundingGenome;
        mostEatenCandidate = foundingGenome;

        InitShared();
        depthLevel = depth;
        Vector3 newHue = Random.insideUnitSphere;
        
        foundingGenome.candidateGenome.name = MutateName(foundingGenome.candidateGenome.name);
        foundingGenome.candidateGenome.bodyGenome.appearanceGenome.BlendHue(newHue, 0.75f);
        
        //string debugTxt = "";
        for (int i = 0; i < 32; i++) {            
            AgentGenome agentGenome = Mutate(foundingGenome.candidateGenome, true, true);
            CandidateAgentData candidate = new CandidateAgentData(agentGenome, speciesID);
            candidateGenomesList.Add(candidate);
            leaderboardGenomesList.Add(candidate);
            //debugTxt += "" + candidate.candidateGenome.brainGenome.linkList[0].weight.ToString("F2") + "  ";
        }
        //Debug.Log("SPECIES CREATED! " + debugTxt);
        representativeCandidate = foundingGenome;

        coatOfArmsMat = new Material(TheRenderKing.instance.coatOfArmsShader);
        //coatOfArmsTex = TheRenderKing.instance.GenerateSpeciesCoatOfArms(foundingGenome.candidateGenome.bodyGenome.appearanceGenome);
        coatOfArmsMat.SetPass(0);
        coatOfArmsMat.SetTexture("_MainTex", TheRenderKing.instance.shapeTex);
        coatOfArmsMat.SetTexture("_PatternTex", TheRenderKing.instance.patternTex);
        coatOfArmsMat.SetFloat("_PatternX", foundingGenome.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeX);
        coatOfArmsMat.SetFloat("_PatternX", foundingGenome.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeY);
        Vector3 huePri = foundingGenome.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        Vector3 hueSec = foundingGenome.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
        coatOfArmsMat.SetColor("_TintPri", new Color(huePri.x, huePri.y, huePri.z));
        coatOfArmsMat.SetColor("_TintSec", new Color(hueSec.x, hueSec.y, hueSec.z));
        coatOfArmsMat.SetFloat("_IsSelected", 0f);
    }
    
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
        
    public void ProcessExtinction(int curTimeStep) {
        isExtinct = true;
        timeStepExtinct = curTimeStep;
    }

    private void CreateNewAverageCandidate() {
        AgentGenome blankGenome = new AgentGenome(.1f, 0);
        avgCandidateData = new CandidateAgentData(blankGenome, speciesID);
        avgCandidateData.SetToAverage(leaderboardGenomesList);
    }

    public void AddNewYearlyStats(int year) 
    {
        CreateNewAverageCandidate(); // ***EC figure this out???        
        avgCandidateDataYearList.Add(avgCandidateData); // = new List<CandidateAgentData>(); // INCLUDES PerformanceData on CandidateData
        //Debug.Log("AddNewYearlyStats " + avgCandidateData.performanceData.totalTicksAlive);
    }

    public CandidateAgentData GetNextAvailableCandidate() {
        CandidateAgentData candidateData = null; 
        
        if (candidateGenomesList.Count > 0) {
            foreach (var candidate in candidateGenomesList) {
                if (candidate.isBeingEvaluated) continue;
                candidateData = candidate;
                break;
            }
        }
        else {
            candidateData = new CandidateAgentData(representativeCandidate.candidateGenome, speciesID);
            Debug.LogError("GetNextAvailableCandidate(): candidateData representativeGenome!!!! " + candidateData);
        }
        
        return candidateData;
    }
       
    public void ProcessCompletedCandidate(CandidateAgentData candidateData, MasterGenomePool masterGenomePool) 
    {
        numAgentsEvaluated++;

        leaderboardGenomesList.Insert(0, candidateData);  // place at front of leaderboard list (genomes eligible for being parents)
        if(leaderboardGenomesList.Count > maxLeaderboardGenomePoolSize) {
            leaderboardGenomesList.RemoveAt(leaderboardGenomesList.Count - 1);
        }

        int beforeCount = candidateGenomesList.Count;
        int listIndex = -1;
        for (int i = 0; i < candidateGenomesList.Count; i++) {
            if(candidateData.candidateID == candidateGenomesList[i].candidateID) {
                listIndex = i;
            }
        }
        //Debug.Log("Removed! " + beforeCount.ToString() + " #: " + listIndex.ToString() + ", candID: " + candidateData.candidateID.ToString());
        if (listIndex > -1) {
            //Debug.Log("RemoveAt(" + listIndex.ToString() + "),[" + candidateGenomesList[listIndex].candidateID.ToString() + "], candID: " + candidateData.candidateID.ToString() + ", SpeciesPool: " + this.speciesID.ToString() + ", CDSID: " + candidateData.speciesID.ToString());
            candidateGenomesList.RemoveAt(listIndex);  // Will this work? never used this before

            masterGenomePool.debugRecentlyDeletedCandidateIDsList.Insert(0, candidateData.candidateID);
            if (masterGenomePool.debugRecentlyDeletedCandidateIDsList.Count > 512) {
                masterGenomePool.debugRecentlyDeletedCandidateIDsList.RemoveAt(masterGenomePool.debugRecentlyDeletedCandidateIDsList.Count - 1);
            }
        }
        else {
            Debug.LogError(candidateGenomesList.Count <= 0 ? "META-ERROR NO INDEX FOUND! " : 
                $"ERROR NO INDEX FOUND! {candidateData.candidateID}, species: {speciesID}, CDSID: {candidateData.speciesID}, [0]: {candidateGenomesList[0].candidateID}");

            masterGenomePool.GlobalFindCandidateID(candidateData.candidateID); // temp debug
        }
        
        // *** NOTE! *** List.Remove() was unreliable - worked sometimes but not others? still unsure about it
    }

    public void AddNewCandidateGenome(AgentGenome newGenome) {
        //Debug.Log("AddedNewCandidate! " + candidateGenomesList.Count.ToString());
        CandidateAgentData newCandidateData = new CandidateAgentData(newGenome, speciesID);
        candidateGenomesList.Add(newCandidateData);
    }

    public AgentGenome GetGenomeFromFitnessLottery() {
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
        AgentGenome parentGenome = leaderboardGenomesList[selectedIndex].candidateGenome;

        return parentGenome;
    }

    // Avoid use of local variables in Mutate() for efficiency
    MutationSettingsInstance tempMutationSettings;

    public AgentGenome Mutate(AgentGenome parentGenome, bool bodySettings, bool brainSettings) 
    {
        string newName = GetMutatedName(parentGenome.name);

        tempMutationSettings = bodySettings ? mutationSettings : cachedNoneMutationSettings;
        BodyGenome newBodyGenome = new BodyGenome(parentGenome.bodyGenome, tempMutationSettings);

        tempMutationSettings = brainSettings ? mutationSettings : cachedNoneMutationSettings;
        BrainGenome newBrainGenome = new BrainGenome(parentGenome.brainGenome, newBodyGenome, tempMutationSettings);
        
        return new AgentGenome(newBodyGenome, newBrainGenome, parentGenome.generationCount + 1, newName);
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
    
    public CandidateAgentData GetFocusedCandidate(SelectionGroup group, int index) {
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
    }
    
    public int GetNumberAgentsEvaluated()
    {
        int result = 0;
        
        foreach (var agent in candidateGenomesList) 
            if(agent.isBeingEvaluated) 
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