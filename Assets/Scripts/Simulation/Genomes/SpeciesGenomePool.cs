using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpeciesGenomePool {

    public int speciesID;
    public int parentSpeciesID;  // keep track of tree of life

    [System.NonSerialized]
    public MutationSettings mutationSettingsRef;  // Or remove this later to keep everything saveable?

    public AgentGenome representativeGenome;
    public List<CandidateAgentData> leaderboardGenomesList;
    public List<CandidateAgentData> candidateGenomesList;

    public int maxLeaderboardGenomePoolSize = 32;
    public float avgFitnessScore = 0f;
    public int numAgentsEvaluated = 0;

    public bool isFlaggedForExtinction = false;
    public bool isExtinct = false;
	
    public SpeciesGenomePool(int ID, int parentID, MutationSettings settings) {

        speciesID = ID;
        parentSpeciesID = parentID;
        mutationSettingsRef = settings;
    }

    // **** Change this for special-case of First-Time startup?
    // **** Create a bunch of random genomes and then organize them into Species first?
    // **** THEN create species and place genomes in?
    public void FirstTimeInitialize(int numGenomes) {  
        isFlaggedForExtinction = false;
        isExtinct = false;

        candidateGenomesList = new List<CandidateAgentData>();
        leaderboardGenomesList = new List<CandidateAgentData>();
                
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
    public void FirstTimeInitialize(AgentGenome foundingGenome) {  
        isFlaggedForExtinction = false;
        isExtinct = false;

        candidateGenomesList = new List<CandidateAgentData>();
        leaderboardGenomesList = new List<CandidateAgentData>();
        
        CandidateAgentData candidate = new CandidateAgentData(foundingGenome, speciesID);

        candidateGenomesList.Add(candidate);
        leaderboardGenomesList.Add(candidate);
        
        representativeGenome = foundingGenome;
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

        newBodyGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome, mutationSettingsRef);
        newBrainGenome.SetToMutatedCopyOfParentGenome(parentBrainGenome, mutationSettingsRef);
        
        childGenome.bodyGenome = newBodyGenome; 
        childGenome.brainGenome = newBrainGenome; 

        return childGenome;
    }
}
