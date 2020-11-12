﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpeciesGenomePool {

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life

    public int depthLevel;

    [System.NonSerialized]
    public MutationSettings mutationSettingsRef;  // Or remove this later to keep everything saveable?

    public string identifier;
    public CandidateAgentData representativeCandidate;
    public CandidateAgentData foundingCandidate;
    public CandidateAgentData longestLivedCandidate;
    public CandidateAgentData mostEatenCandidate;
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

    // stats:

    public float avgLifespan = 0f;
    public List<float> avgLifespanPerYearList;    
    public float avgFoodEatenPlant = 0f;
    public List<float> avgFoodEatenPlantPerYearList;
    public float avgFoodEatenZoop = 0f;
    public List<float> avgFoodEatenZoopPerYearList;
    public float avgFoodEatenCreature = 0f;
    public List<float> avgFoodEatenCreaturePerYearList;    
    public float avgFoodEatenEgg = 0f;
    public List<float> avgFoodEatenEggPerYearList;
    public float avgFoodEatenCorpse = 0f;
    public List<float> avgFoodEatenCorpsePerYearList;

    public float avgBodySize = 0f;
    public List<float> avgBodySizePerYearList;
    public float avgSpecAttack = 0f;
    public List<float> avgSpecAttackPerYearList;
    public float avgSpecDefend = 0f;
    public List<float> avgSpecDefendPerYearList;
    public float avgSpecSpeed = 0f;
    public List<float> avgSpecSpeedPerYearList;
    public float avgSpecUtility = 0f;
    public List<float> avgSpecUtilityPerYearList;
    public float avgFoodSpecDecay = 0f;
    public List<float> avgFoodSpecDecayPerYearList;
    public float avgFoodSpecPlant = 0f;
    public List<float> avgFoodSpecPlantPerYearList;
    public float avgFoodSpecMeat = 0f;
    public List<float> avgFoodSpecMeatPerYearList;
    public float avgNumNeurons = 0f;
    public List<float> avgNumNeuronsPerYearList;
    public float avgNumAxons = 0f;
    public List<float> avgNumAxonsPerYearList;
    public float avgExperience = 0f;
    public List<float> avgExperiencePerYearList;
    public float avgFitnessScore = 0f;
    public List<float> avgFitnessScorePerYearList;
    public float avgDamageDealt = 0f;
    public List<float> avgDamageDealtPerYearList;
    public float avgDamageTaken = 0f;
    public List<float> avgDamageTakenPerYearList;

    public float avgTimeRested = 0f;
    public List<float> avgTimeRestedPerYearList;
    public float avgTimesDefended = 0f;
    public List<float> avgTimesDefendedPerYearList;
    public float avgTimesDashed = 0f;
    public List<float> avgTimesDashedPerYearList;
    public float avgTimesAttacked = 0f;
    public List<float> avgTimesAttackedPerYearList;
    public float avgTimesPregnant = 0f;
    public List<float> avgTimesPregenantPerYearList;
    

    public bool isFlaggedForExtinction = false;
    public bool isExtinct = false;
	
    public SpeciesGenomePool(int ID, int parentID, int year, int timeStep, MutationSettings settings) {
        yearCreated = year;
        speciesID = ID;
        parentSpeciesID = parentID;
        mutationSettingsRef = settings;
        timeStepCreated = timeStep;
        timeStepExtinct = 2000000000;

        
    }

    private void InitShared() {
        isFlaggedForExtinction = false;
        isExtinct = false;
        // *** Turn these into Array of Lists
        avgLifespanPerYearList = new List<float>();   
        avgLifespanPerYearList.Add(0f);
        avgFoodEatenPlantPerYearList = new List<float>();
        avgFoodEatenPlantPerYearList.Add(0f);
        avgFoodEatenZoopPerYearList = new List<float>();
        avgFoodEatenZoopPerYearList.Add(0f);
        avgFoodEatenCreaturePerYearList = new List<float>();
        avgFoodEatenCreaturePerYearList.Add(0f);
        avgFoodEatenEggPerYearList = new List<float>();
        avgFoodEatenEggPerYearList.Add(0f);
        avgFoodEatenCorpsePerYearList = new List<float>();
        avgFoodEatenCorpsePerYearList.Add(0f);
        avgBodySizePerYearList = new List<float>();
        avgBodySizePerYearList.Add(0f);
        avgSpecAttackPerYearList = new List<float>();
        avgSpecAttackPerYearList.Add(0f);
        avgSpecDefendPerYearList = new List<float>();
        avgSpecDefendPerYearList.Add(0f);
        avgSpecSpeedPerYearList = new List<float>();
        avgSpecSpeedPerYearList.Add(0f);
        avgSpecUtilityPerYearList = new List<float>();
        avgSpecUtilityPerYearList.Add(0f);
        avgFoodSpecDecayPerYearList = new List<float>();
        avgFoodSpecDecayPerYearList.Add(0f);
        avgFoodSpecPlantPerYearList = new List<float>();
        avgFoodSpecPlantPerYearList.Add(0f);
        avgFoodSpecMeatPerYearList = new List<float>();
        avgFoodSpecMeatPerYearList.Add(0f);
        avgNumNeuronsPerYearList = new List<float>();
        avgNumNeuronsPerYearList.Add(0f);
        avgNumAxonsPerYearList = new List<float>();
        avgNumAxonsPerYearList.Add(0f);
        avgExperiencePerYearList = new List<float>();
        avgExperiencePerYearList.Add(0f);
        avgFitnessScorePerYearList = new List<float>();
        avgFitnessScorePerYearList.Add(0f);
        avgDamageDealtPerYearList = new List<float>();
        avgDamageDealtPerYearList.Add(0f);
        avgDamageTakenPerYearList = new List<float>();
        avgDamageTakenPerYearList.Add(0f);

        avgTimeRestedPerYearList = new List<float>();
        avgTimeRestedPerYearList.Add(0f);
        
        avgTimesDefendedPerYearList = new List<float>();
        avgTimesDefendedPerYearList.Add(0f);
        avgTimesDashedPerYearList = new List<float>();
        avgTimesDashedPerYearList.Add(0);
        avgTimesAttackedPerYearList = new List<float>();
        avgTimesAttackedPerYearList.Add(0);
        avgTimesPregenantPerYearList = new List<float>();
        avgTimesPregenantPerYearList.Add(0);

        candidateGenomesList = new List<CandidateAgentData>();
        leaderboardGenomesList = new List<CandidateAgentData>();
        
        hallOfFameGenomesList = new List<CandidateAgentData>();
    }

    // **** Change this for special-case of First-Time startup?
    // **** Create a bunch of random genomes and then organize them into Species first?
    // **** THEN create species and place genomes in?
    public void FirstTimeInitializeROOT(int numGenomes, int depth) {
        
        InitShared();
        depthLevel = depth;
        int tempNumHiddenNeurons = 0;

        int numInitialGenomes = 1;
        AgentGenome[] seedGenomeArray = new AgentGenome[numInitialGenomes];
        for(int i = 0; i < seedGenomeArray.Length; i++) {
            AgentGenome seedGenome = new AgentGenome();
            seedGenome.GenerateInitialRandomBodyGenome();
            seedGenome.InitializeRandomBrainFromCurrentBody(1.0f, mutationSettingsRef.brainInitialConnectionChance, tempNumHiddenNeurons);

            seedGenomeArray[i] = seedGenome;
        }

        foundingCandidate = new CandidateAgentData(seedGenomeArray[0], speciesID);
        longestLivedCandidate = foundingCandidate;
        mostEatenCandidate = foundingCandidate;

        for (int i = 0; i < numGenomes; i++) {

            int seedGenomeIndex = i % numInitialGenomes;
            
            mutationSettingsRef.bodyCoreSizeMutationChance = 0.5f;
            mutationSettingsRef.bodyCoreMutationStepSize = 0.075f;
                        
            AgentGenome newGenome = Mutate(seedGenomeArray[seedGenomeIndex], true, true);

            CandidateAgentData candidate = new CandidateAgentData(newGenome, speciesID);


            if(i < maxLeaderboardGenomePoolSize) {
                leaderboardGenomesList.Add(candidate);
            }
            candidateGenomesList.Add(candidate);

            //yield return null;
        }

        representativeCandidate = candidateGenomesList[0];
    }
    public void FirstTimeInitialize(CandidateAgentData foundingGenome, int depth) {
        this.foundingCandidate = foundingGenome;        
        longestLivedCandidate = foundingGenome;
        mostEatenCandidate = foundingGenome;

        InitShared();
        depthLevel = depth;
        Vector3 newHue = UnityEngine.Random.insideUnitSphere;

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
        for (int i = 0; i < 24; i++) {
            
            mutationSettingsRef.bodyCoreSizeMutationChance = 0.5f;
            mutationSettingsRef.bodyCoreMutationStepSize = 0.1f;
            //mutationSettingsRef.mutationStrengthSlot = 0.15f;

            AgentGenome agentGenome = Mutate(foundingGenome.candidateGenome, true, true);            
            //int tempNumHiddenNeurons = 0;
            //agentGenome.InitializeRandomBrainFromCurrentBody(0.5f, mutationSettingsRef.initialConnectionChance, tempNumHiddenNeurons);

            CandidateAgentData candidate = new CandidateAgentData(agentGenome, speciesID);
            candidateGenomesList.Add(candidate);
            leaderboardGenomesList.Add(candidate);

            debugTxt += "" + candidate.candidateGenome.brainGenome.linkList[0].weight.ToString("F2") + "  ";
        }

        Debug.Log("SPECIES CREATED! " + debugTxt);

        representativeCandidate = foundingGenome;
    }

    public void ProcessExtinction(int curTimeStep) {
        isExtinct = true;
        /*avgLifespan = 0f;
        avgConsumptionDecay = 0f;
        avgConsumptionPlant = 0f;
        avgConsumptionMeat = 0f;
        avgBodySize = 0f;
        avgSpecAttack = 0f;
        avgSpecDefend = 0f;
        avgSpecSpeed = 0f;
        avgSpecUtility = 0f;
        avgFoodSpecDecay = 0f;
        avgFoodSpecPlant = 0f;
        avgFoodSpecMeat = 0f;
        avgNumNeurons = 0f;
        avgNumAxons = 0f;
        avgExperience = 0f;
        avgFitnessScore = 0f;
        avgDamageDealt = 0f;
        avgDamageTaken = 0f;
        */
        timeStepExtinct = curTimeStep;
    }

    public void AddNewYearlyStats(int year) {
        avgLifespanPerYearList.Add(avgLifespan);
        avgFoodEatenPlantPerYearList.Add(avgFoodEatenPlant);
        avgFoodEatenZoopPerYearList.Add(avgFoodEatenZoop);
        avgFoodEatenCreaturePerYearList.Add(avgFoodEatenCreature);
        avgFoodEatenCorpsePerYearList.Add(avgFoodEatenCorpse);
        avgFoodEatenEggPerYearList.Add(avgFoodEatenEgg);
        
        avgBodySizePerYearList.Add(avgBodySize);
        avgSpecAttackPerYearList.Add(avgSpecAttack);
        avgSpecDefendPerYearList.Add(avgSpecDefend);
        avgSpecSpeedPerYearList.Add(avgSpecSpeed);
        avgSpecUtilityPerYearList.Add(avgSpecUtility);
        avgFoodSpecDecayPerYearList.Add(avgFoodSpecDecay);
        avgFoodSpecPlantPerYearList.Add(avgFoodSpecPlant);
        avgFoodSpecMeatPerYearList.Add(avgFoodSpecMeat);
        avgNumNeuronsPerYearList.Add(avgNumNeurons);
        avgNumAxonsPerYearList.Add(avgNumAxons);
        avgExperiencePerYearList.Add(avgExperience);
        avgFitnessScorePerYearList.Add(avgFitnessScore);
        avgDamageDealtPerYearList.Add(avgDamageDealt);
        avgDamageTakenPerYearList.Add(avgDamageTaken);

        avgTimeRestedPerYearList.Add(avgTimeRested);
        avgTimesDashedPerYearList.Add(avgTimesDashed);
        avgTimesDefendedPerYearList.Add(avgTimesDefended);
        avgTimesAttackedPerYearList.Add(avgTimesAttacked);
        avgTimesPregenantPerYearList.Add(avgTimesPregnant);
    }
    /*public void UpdateSpeciesStats() {
        avgLifespanPerYearList[avgLifespanPerYearList.Count - 1] = avgLifespan;
        avgConsumptionDecayPerYearList[avgConsumptionDecayPerYearList.Count - 1] = avgConsumptionDecay;
        avgConsumptionPlantPerYearList[avgConsumptionPlantPerYearList.Count - 1] = avgConsumptionPlant;
        avgConsumptionMeatPerYearList[avgConsumptionMeatPerYearList.Count - 1] = avgConsumptionMeat;
        avgBodySizePerYearList[avgBodySizePerYearList.Count - 1] = avgBodySize;
        avgSpecAttackPerYearList[avgSpecAttackPerYearList.Count - 1] = avgSpecAttack;
        avgSpecDefendPerYearList[avgSpecDefendPerYearList.Count - 1] = avgSpecDefend;
        avgSpecSpeedPerYearList[avgSpecSpeedPerYearList.Count - 1] = avgSpecSpeed;
        avgSpecUtilityPerYearList[avgSpecUtilityPerYearList.Count - 1] = avgSpecUtility;
        avgFoodSpecDecayPerYearList[avgFoodSpecDecayPerYearList.Count - 1] = avgFoodSpecDecay;
        avgFoodSpecPlantPerYearList[avgFoodSpecPlantPerYearList.Count - 1] = avgFoodSpecPlant;
        avgFoodSpecMeatPerYearList[avgFoodSpecMeatPerYearList.Count - 1] = avgFoodSpecMeat;
        avgNumNeuronsPerYearList[avgNumNeuronsPerYearList.Count - 1] = avgNumNeurons;
        avgNumAxonsPerYearList[avgNumAxonsPerYearList.Count - 1] = avgNumAxons;
        avgExperiencePerYearList[avgExperiencePerYearList.Count - 1] = avgExperience;
        avgFitnessScorePerYearList[avgFitnessScorePerYearList.Count - 1] = avgFitnessScore;
        avgDamageDealtPerYearList[avgDamageDealtPerYearList.Count - 1] = avgDamageDealt;
        avgDamageTakenPerYearList[avgDamageTakenPerYearList.Count - 1] = avgDamageTaken;
    }*/

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
            float fitnessScore = 0.01f;
            for(int j = 0; j < leaderboardGenomesList[i].evaluationScoresList.Count; j++) {
                fitnessScore += (float)leaderboardGenomesList[i].evaluationScoresList[j];
            }
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

        return parentGenome;
    }


    public AgentGenome Mutate(AgentGenome parentGenome, bool bodySettings, bool brainSettings) {
        //AgentGenome parentGenome = leaderboardGenomesList[selectedIndex].candidateGenome;
        AgentGenome childGenome = new AgentGenome();
        
        BodyGenome newBodyGenome = new BodyGenome();
        BrainGenome newBrainGenome = new BrainGenome();

        BodyGenome parentBodyGenome = parentGenome.bodyGenome;
        BrainGenome parentBrainGenome = parentGenome.brainGenome;

        MutationSettings mutationSettingsNoneCopy = new MutationSettings();
        if(bodySettings) {
            newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, mutationSettingsRef);
        }
        else {
            newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, mutationSettingsNoneCopy);
        }
        if(brainSettings) {
            newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, newBodyGenome, mutationSettingsRef);
        }
        else {
            newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, newBodyGenome, mutationSettingsNoneCopy);
        }
        //Debug.Log("Mutate() " + mutationSettingsRef.mutationStrengthSlot.ToString("F2"));
        
        childGenome.bodyGenome = newBodyGenome; 
        childGenome.brainGenome = newBrainGenome;
        //childGenome.generationCount = parentGenome.generationCount + 1;

        return childGenome;
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
