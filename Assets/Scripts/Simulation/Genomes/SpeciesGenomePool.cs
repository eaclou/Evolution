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

    public float avgLifespan = 0f;
    public List<float> avgLifespanPerYearList;
    public float avgConsumption = 0f;
    public List<float> avgConsumptionPerYearList;
    public float avgBodySize = 0f;
    public List<float> avgBodySizePerYearList;
    public float avgDietType = 0f;
    public List<float> avgDietTypePerYearList;
    public float avgNumNeurons = 0f;
    public List<float> avgNumNeuronsPerYearList;
    public float avgNumAxons = 0f;
    public List<float> avgNumAxonsPerYearList;

    public bool isFlaggedForExtinction = false;
    public bool isExtinct = false;
	
    public SpeciesGenomePool(int ID, int parentID, int year, MutationSettings settings) {
        yearCreated = year;
        speciesID = ID;
        parentSpeciesID = parentID;
        mutationSettingsRef = settings;
    }

    private void InitShared() {
        isFlaggedForExtinction = false;
        isExtinct = false;

        avgLifespanPerYearList = new List<float>();
        avgLifespanPerYearList.Add(0f);
        avgConsumptionPerYearList = new List<float>();
        avgConsumptionPerYearList.Add(0f);
        avgBodySizePerYearList = new List<float>();
        avgBodySizePerYearList.Add(0f);
        avgDietTypePerYearList = new List<float>();
        avgDietTypePerYearList.Add(0f);
        avgNumNeuronsPerYearList = new List<float>();
        avgNumNeuronsPerYearList.Add(0f);
        avgNumAxonsPerYearList = new List<float>();
        avgNumAxonsPerYearList.Add(0f);

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
            agentGenome.InitializeRandomBrainFromCurrentBody(mutationSettingsRef.initialConnectionChance, tempNumHiddenNeurons);

            CandidateAgentData candidate = new CandidateAgentData(agentGenome, speciesID);

            if(i < maxLeaderboardGenomePoolSize) {
                leaderboardGenomesList.Add(candidate);
            }
            candidateGenomesList.Add(candidate);            
        }

        representativeGenome = candidateGenomesList[0].candidateGenome;
    }
    public void FirstTimeInitialize(AgentGenome foundingGenome, int depth) {
        InitShared();
        depthLevel = depth;
        
        CandidateAgentData candidate = new CandidateAgentData(foundingGenome, speciesID);

        candidateGenomesList.Add(candidate);
        leaderboardGenomesList.Add(candidate);
        
        representativeGenome = foundingGenome;
    }

    public void ProcessExtinction() {
        isExtinct = true;
        avgLifespan = 0f;
        avgConsumption = 0f;
        avgBodySize = 0f;
        avgDietType = 0f;
        avgNumNeurons = 0f;
        avgNumAxons = 0f;
    }

    public void UpdateYearlyStats(int year) {
        avgLifespanPerYearList.Add(avgLifespan);
        avgConsumptionPerYearList.Add(avgConsumption);
        avgBodySizePerYearList.Add(avgBodySize);
        avgDietTypePerYearList.Add(avgDietType);
        avgNumNeuronsPerYearList.Add(avgNumNeurons);
        avgNumAxonsPerYearList.Add(avgNumAxons);
    }

    public CandidateAgentData GetNextAvailableCandidate() {

        CandidateAgentData candidateData = null; // candidateGenomesList[0].candidateGenome;
        for(int i = 0; i < candidateGenomesList.Count; i++) {
            if(candidateGenomesList[i].isBeingEvaluated) {
                // already being tested
            }
            else {
                candidateData = candidateGenomesList[i];                
            }
        }
        
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
            Debug.LogError("ERROR NO INDEX FOUND! " + candidateData.candidateID.ToString() + ", species: " + this.speciesID.ToString() + ", CDSID: " + candidateData.speciesID.ToString() + ", [0]: " + candidateGenomesList[0].candidateID.ToString());

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
}
