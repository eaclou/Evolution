using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpeciesGenomePool 
{
    Lookup lookup => Lookup.instance;

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life

    public int depthLevel;

    [System.NonSerialized]
    public MutationSettingsInstance mutationSettings;  // Or remove this later to keep everything saveable?

    public string identifier;
    public CandidateAgentData representativeCandidate;
    public CandidateAgentData foundingCandidate;
    public CandidateAgentData longestLivedCandidate;
    public CandidateAgentData mostEatenCandidate;
    //public Lineage Map --> avg genome over time
    public List<CandidateAgentData> hallOfFameGenomesList;
    public List<CandidateAgentData> leaderboardGenomesList;
    public List<CandidateAgentData> candidateGenomesList;

    public int maxLeaderboardGenomePoolSize = 24;    
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
        this.foundingCandidate = foundingGenome;        
        longestLivedCandidate = foundingGenome;
        mostEatenCandidate = foundingGenome;

        InitShared();
        depthLevel = depth;
        Vector3 newHue = Random.insideUnitSphere;

        string newName = "";
        string[] lettersArray = new string[26];
                lettersArray[0] = "A";
                lettersArray[1] = "B";
                lettersArray[2] = "C";
                lettersArray[3] = "D";
                lettersArray[4] = "E";
                lettersArray[5] = "F";
                lettersArray[6] = "G";
                lettersArray[7] = "H";
                lettersArray[8] = "I";
                lettersArray[9] = "J";
                lettersArray[10] = "K";
                lettersArray[11] = "L";
                lettersArray[12] = "M";
                lettersArray[13] = "N";
                lettersArray[14] = "O";
                lettersArray[15] = "P";
                lettersArray[16] = "Q";
                lettersArray[17] = "R";
                lettersArray[18] = "S";
                lettersArray[19] = "T";
                lettersArray[20] = "U";
                lettersArray[21] = "V";
                lettersArray[22] = "W";
                lettersArray[23] = "X";
                lettersArray[24] = "Y";
                lettersArray[25] = "Z";

        for(int i = 0; i < foundingGenome.candidateGenome.bodyGenome.coreGenome.name.Length; i++) {
            float randChance1 = UnityEngine.Random.Range(0f, 1f);
            if(randChance1 < 0.35) {
                int randLetterIndex = UnityEngine.Random.Range(0, 26);
                newName += lettersArray[randLetterIndex];  
                
                if(randChance1 < 0.05) {
                    randLetterIndex = UnityEngine.Random.Range(0, 26);
                    newName += lettersArray[randLetterIndex];            
                }
            }            
            else if(randChance1 > 0.95f) {

            }
            else {
                newName += foundingGenome.candidateGenome.bodyGenome.coreGenome.name[i];
            }
        }

        foundingGenome.candidateGenome.bodyGenome.coreGenome.name = newName;

        foundingGenome.candidateGenome.bodyGenome.appearanceGenome.huePrimary = Vector3.Lerp(foundingGenome.candidateGenome.bodyGenome.appearanceGenome.huePrimary, newHue, 0.75f);
        foundingGenome.candidateGenome.bodyGenome.appearanceGenome.hueSecondary = Vector3.Lerp(foundingGenome.candidateGenome.bodyGenome.appearanceGenome.hueSecondary, Vector3.one - newHue, 0.75f);

        //=========================================================================

        string debugTxt = "";
        for (int i = 0; i < 64; i++) {
            
            mutationSettings.bodyCoreSizeMutationChance = 0.5f;
            mutationSettings.bodyCoreMutationStepSize = 0.1f;
            //mutationSettingsRef.mutationStrengthSlot = 0.15f;

            AgentGenome agentGenome = Mutate(foundingGenome.candidateGenome, true, true);            
            //int tempNumHiddenNeurons = 0;
            //agentGenome.InitializeRandomBrainFromCurrentBody(0.5f, mutationSettingsRef.initialConnectionChance, tempNumHiddenNeurons);

            CandidateAgentData candidate = new CandidateAgentData(agentGenome, speciesID);
            candidateGenomesList.Add(candidate);
            leaderboardGenomesList.Add(candidate);

            debugTxt += "" + candidate.candidateGenome.brainGenome.linkList[0].weight.ToString("F2") + "  ";
        }

        //Debug.Log("SPECIES CREATED! " + debugTxt);

        representativeCandidate = foundingGenome;
    }

    public void ProcessExtinction(int curTimeStep) {
        isExtinct = true;
        
        timeStepExtinct = curTimeStep;
    }

    private void CreateNewAverageCandidate() {
        AgentGenome blankGenome = new AgentGenome();
        blankGenome.GenerateInitialRandomBodyGenome();
        int tempNumHiddenNeurons = 0;
        blankGenome.InitializeRandomBrainFromCurrentBody(1.0f, 0.1f, tempNumHiddenNeurons);   // unneeded?
        avgCandidateData = new CandidateAgentData(blankGenome, speciesID);

        RecalculateAverageCandidate();
    }
    
    private void RecalculateAverageCandidate() {
        
        //calculate avg candidate:
        avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.huePrimary = Vector3.zero;
        avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.hueSecondary = Vector3.zero;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.dietSpecializationDecay = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.dietSpecializationPlant = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.dietSpecializationMeat = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationAttack = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationDefense = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationSpeed = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationUtility = 0f;

        avgCandidateData.candidateGenome.bodyGenome.coreGenome.bodyLength = 0f;
        avgCandidateData.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio = 0f;

        avgCandidateData.performanceData = new PerformanceData();  // clear // ***EC better spot for this??
        //Debug.Log("avgPerformanceData " + avgPerformanceData.totalTicksAlive.ToString());
        //avgCandidateData.performanceData = avgPerformanceData;
        
                
        for(int i = 0; i < leaderboardGenomesList.Count; i++) {
            float norm = 1f / (float)(leaderboardGenomesList.Count - 1);
            
            avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.huePrimary += leaderboardGenomesList[i].candidateGenome.bodyGenome.appearanceGenome.huePrimary * norm;
            avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.hueSecondary += leaderboardGenomesList[i].candidateGenome.bodyGenome.appearanceGenome.hueSecondary * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.dietSpecializationDecay += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.dietSpecializationDecay * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.dietSpecializationPlant += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.dietSpecializationPlant * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.dietSpecializationMeat += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.dietSpecializationMeat * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationAttack += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.talentSpecializationAttack * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationDefense += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.talentSpecializationDefense * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationSpeed += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.talentSpecializationSpeed * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.talentSpecializationUtility += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.talentSpecializationUtility * norm;
        
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.bodyLength += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.bodyLength * norm;
            avgCandidateData.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio += leaderboardGenomesList[i].candidateGenome.bodyGenome.coreGenome.creatureAspectRatio * norm;

            //Performance Data:
            avgCandidateData.performanceData.totalDamageDealt += leaderboardGenomesList[i].performanceData.totalDamageDealt * norm;
            avgCandidateData.performanceData.totalDamageTaken += leaderboardGenomesList[i].performanceData.totalDamageTaken * norm;
            avgCandidateData.performanceData.totalFoodEatenCorpse += leaderboardGenomesList[i].performanceData.totalFoodEatenCorpse * norm;
            avgCandidateData.performanceData.totalFoodEatenCreature += leaderboardGenomesList[i].performanceData.totalFoodEatenCreature * norm;
            avgCandidateData.performanceData.totalFoodEatenEgg += leaderboardGenomesList[i].performanceData.totalFoodEatenEgg * norm;
            avgCandidateData.performanceData.totalFoodEatenPlant += leaderboardGenomesList[i].performanceData.totalFoodEatenPlant * norm;
            avgCandidateData.performanceData.totalFoodEatenZoop += leaderboardGenomesList[i].performanceData.totalFoodEatenZoop * norm;
            avgCandidateData.performanceData.totalTicksAlive += leaderboardGenomesList[i].performanceData.totalTicksAlive * norm;
            avgCandidateData.performanceData.totalTicksRested += leaderboardGenomesList[i].performanceData.totalTicksRested * norm;
            avgCandidateData.performanceData.totalTimesAttacked += leaderboardGenomesList[i].performanceData.totalTimesAttacked * norm;
            avgCandidateData.performanceData.totalTimesDashed += leaderboardGenomesList[i].performanceData.totalTimesDashed * norm;
            avgCandidateData.performanceData.totalTimesDefended += leaderboardGenomesList[i].performanceData.totalTimesDefended * norm;
            avgCandidateData.performanceData.totalTimesPregnant += leaderboardGenomesList[i].performanceData.totalTimesPregnant * norm;   
        }
    }
    
    public void AddNewYearlyStats(int year) {
        
        CreateNewAverageCandidate(); // ***EC figure this out???        
        avgCandidateDataYearList.Add(avgCandidateData); // = new List<CandidateAgentData>(); // INCLUDES PerformanceData on CandidateData

        //Debug.Log("AddNewYearlyStats " + avgCandidateData.performanceData.totalTicksAlive);
    }

    public CandidateAgentData GetNextAvailableCandidate() {

        CandidateAgentData candidateData = null; // candidateGenomesList[0].candidateGenome;
        if(candidateGenomesList.Count > 0) {
            for (int i = 0; i < candidateGenomesList.Count; i++) {
                if(candidateGenomesList[i].isBeingEvaluated) {
                    // already being tested
                    //Debug.LogError("GetNextAvailableCandidate(): candidateGenomesList[i].isBeingEvaluated!");
                }
                else {
                    candidateData = candidateGenomesList[i];
                    break;
                }
            }
        }
        else {
            candidateData = new CandidateAgentData(representativeCandidate.candidateGenome, speciesID);
            Debug.LogError("GetNextAvailableCandidate(): candidateData representativeGenome!!!! " + candidateData.ToString());
        }
        
        /*if(candidateData == null) {
            Debug.LogError("GetNextAvailableCandidate(): candidateData NULL!!!!");
        }*/
        return candidateData;
    }
       
    public void ProcessCompletedCandidate(CandidateAgentData candidateData, MasterGenomePool masterGenomePool) {

        numAgentsEvaluated++;

        leaderboardGenomesList.Insert(0, candidateData);  // place at front of leaderboard list (genomes eligible for being parents)
        if(leaderboardGenomesList.Count > maxLeaderboardGenomePoolSize) {
            leaderboardGenomesList.RemoveAt(leaderboardGenomesList.Count - 1);
        }

        int beforeCount = candidateGenomesList.Count;
        int listIndex = -1;
        for(int i = 0; i < candidateGenomesList.Count; i++) {
            if(candidateData.candidateID == candidateGenomesList[i].candidateID) {
                listIndex = i;
            }
        }
        //Debug.Log("Removed! " + beforeCount.ToString() + " #: " + listIndex.ToString() + ", candID: " + candidateData.candidateID.ToString());
        if(listIndex > -1) {
            //Debug.Log("RemoveAt(" + listIndex.ToString() + "),[" + candidateGenomesList[listIndex].candidateID.ToString() + "], candID: " + candidateData.candidateID.ToString() + ", SpeciesPool: " + this.speciesID.ToString() + ", CDSID: " + candidateData.speciesID.ToString());
            candidateGenomesList.RemoveAt(listIndex);  // Will this work? never used this before

            masterGenomePool.debugRecentlyDeletedCandidateIDsList.Insert(0, candidateData.candidateID);
            if(masterGenomePool.debugRecentlyDeletedCandidateIDsList.Count > 512) {
                masterGenomePool.debugRecentlyDeletedCandidateIDsList.RemoveAt(masterGenomePool.debugRecentlyDeletedCandidateIDsList.Count - 1);
            }
        }
        else {
            if (candidateGenomesList.Count > 0) {
                Debug.LogError("ERROR NO INDEX FOUND! " + candidateData.candidateID.ToString() + ", species: " + this.speciesID.ToString() + ", CDSID: " + candidateData.speciesID.ToString() + ", [0]: " + candidateGenomesList[0].candidateID.ToString());
            }
            else {
                Debug.LogError("META-ERROR NO INDEX FOUND! ");
            }
            
            // Find it:
            masterGenomePool.GlobalFindCandidateID(candidateData.candidateID); // temp debug
        }
        
        // *** NOTE! *** List.Remove() was unreliable - worked sometimes but not others? still unsure about it
        //int afterCount = candidateGenomesList.Count;
        //if(beforeCount - afterCount > 0) {
        //    
        //}
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
            //for(int j = 0; j < leaderboardGenomesList[i].evaluationScoresList.Count; j++) {
                fitnessScore += (float)leaderboardGenomesList[i].performanceData.totalTicksAlive;
            //}
            rankedFitnessScoresArray[i] = fitnessScore;
            rankedIndicesList[i] = i;
            totalFitness += fitnessScore;

            //leaderboardGenomesListString += "\n#" + i.ToString() + ", score= " + fitnessScore.ToString();
        }
        //Debug.Log(leaderboardGenomesListString);
        
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

    public AgentGenome Mutate(AgentGenome parentGenome, bool bodySettings, bool brainSettings) {
        //AgentGenome parentGenome = leaderboardGenomesList[selectedIndex].candidateGenome;
        AgentGenome childGenome = new AgentGenome();
        
        BodyGenome newBodyGenome = new BodyGenome();
        BrainGenome newBrainGenome = new BrainGenome();

        BodyGenome parentBodyGenome = parentGenome.bodyGenome;
        BrainGenome parentBrainGenome = parentGenome.brainGenome;

        tempMutationSettings = bodySettings ? mutationSettings : cachedNoneMutationSettings;
        newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, tempMutationSettings);

        tempMutationSettings = brainSettings ? mutationSettings : cachedNoneMutationSettings;
        newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, newBodyGenome, tempMutationSettings);

        childGenome.bodyGenome = newBodyGenome; 
        childGenome.brainGenome = newBrainGenome;

        childGenome.generationCount = parentGenome.generationCount + 1;

        return childGenome;
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

        if(numAgentsEvaluated > maxLeaderboardGenomePoolSize) {
            hallOfFameGenomesList.Add(agent.candidateRef);
        }
    }
    
    public LinkGenome GetLeaderboardLinkGenome(int index, int linkedListIndex)
    {
        AgentGenome genome = GetLeaderboardGenome(index);

        if (genome.brainGenome.linkList.Count > linkedListIndex)
            return null;
            
        return genome.brainGenome.linkList[linkedListIndex];
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
}
