using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpeciesGenomePool {

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life

    public int depthLevel;

    [System.NonSerialized]
    public MutationSettings mutationSettingsRef;  // Or remove this later to keep everything saveable?

    public AgentGenome representativeGenome;
    public List<CandidateAgentData> leaderboardGenomesList;
    public List<CandidateAgentData> candidateGenomesList;

    public int maxLeaderboardGenomePoolSize = 32;    
    public int numAgentsEvaluated = 0;
     
    public int yearCreated = -1;
    public int timeStepCreated = 1;
    public int timeStepExtinct = 2000000000;

    public float avgLifespan = 0f;
    public List<float> avgLifespanPerYearList;
    public float avgConsumptionDecay = 0f;
    public List<float> avgConsumptionDecayPerYearList;
    public float avgConsumptionPlant = 0f;
    public List<float> avgConsumptionPlantPerYearList;
    public float avgConsumptionMeat = 0f;
    public List<float> avgConsumptionMeatPerYearList;
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

        avgLifespanPerYearList = new List<float>();
        avgLifespanPerYearList.Add(0f);
        avgConsumptionDecayPerYearList = new List<float>();
        avgConsumptionDecayPerYearList.Add(0f);
        avgConsumptionPlantPerYearList = new List<float>();
        avgConsumptionPlantPerYearList.Add(0f);
        avgConsumptionMeatPerYearList = new List<float>();
        avgConsumptionMeatPerYearList.Add(0f);
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

        candidateGenomesList = new List<CandidateAgentData>();
        leaderboardGenomesList = new List<CandidateAgentData>();
    }

    // **** Change this for special-case of First-Time startup?
    // **** Create a bunch of random genomes and then organize them into Species first?
    // **** THEN create species and place genomes in?
    public void FirstTimeInitialize(int numGenomes, int depth) {
        InitShared();
        depthLevel = depth;
    
        for (int i = 0; i < numGenomes; i++) {
            AgentGenome agentGenome = new AgentGenome();
            agentGenome.GenerateInitialRandomBodyGenome();
            
            int tempNumHiddenNeurons = 0;
            agentGenome.InitializeRandomBrainFromCurrentBody(1.25f, mutationSettingsRef.initialConnectionChance, tempNumHiddenNeurons);

            CandidateAgentData candidate = new CandidateAgentData(agentGenome, speciesID);

            if(i < maxLeaderboardGenomePoolSize) {
                leaderboardGenomesList.Add(candidate);
            }
            candidateGenomesList.Add(candidate);

            //yield return null;
        }

        representativeGenome = candidateGenomesList[0].candidateGenome;
    }
    public void FirstTimeInitialize(AgentGenome foundingGenome, int depth) {
        InitShared();
        depthLevel = depth;

        string debugTxt = "";
        for (int i = 0; i < 36; i++) {
            
            mutationSettingsRef.defaultBodyMutationChance = 0.3f;
            mutationSettingsRef.defaultBodyMutationStepSize = 0.05f;
            mutationSettingsRef.mutationStrengthSlot = 0.1f;

            AgentGenome agentGenome = Mutate(foundingGenome, true, true);
            int tempNumHiddenNeurons = 0;
            agentGenome.InitializeRandomBrainFromCurrentBody(0.5f, mutationSettingsRef.initialConnectionChance, tempNumHiddenNeurons);

            CandidateAgentData candidate = new CandidateAgentData(agentGenome, speciesID);
            candidateGenomesList.Add(candidate);
            leaderboardGenomesList.Add(candidate);

            debugTxt += "" + candidate.candidateGenome.brainGenome.linkList[0].weight.ToString("F2") + "  ";
        }

        Debug.Log("SPECIES CREATED! " + debugTxt);
        
        representativeGenome = foundingGenome;
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
        avgConsumptionDecayPerYearList.Add(avgConsumptionDecay);
        avgConsumptionPlantPerYearList.Add(avgConsumptionPlant);
        avgConsumptionMeatPerYearList.Add(avgConsumptionMeat);
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
                }
                else {
                    candidateData = candidateGenomesList[i];                
                }
            }
        }
        else {
            candidateData = new CandidateAgentData(representativeGenome, speciesID);
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

        return parentGenome;
    }


    public AgentGenome Mutate(AgentGenome parentGenome, bool bodySettings, bool brainSettings) {
        //AgentGenome parentGenome = leaderboardGenomesList[selectedIndex].candidateGenome;
        AgentGenome childGenome = new AgentGenome();
        
        BodyGenome newBodyGenome = new BodyGenome();
        BrainGenome newBrainGenome = new BrainGenome();

        BodyGenome parentBodyGenome = parentGenome.bodyGenome;
        BrainGenome parentBrainGenome = parentGenome.brainGenome;

        //Debug.Log("Mutate() " + mutationSettingsRef.mutationStrengthSlot.ToString("F2"));
        newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, mutationSettingsRef);
        newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, newBodyGenome, mutationSettingsRef);
        
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
