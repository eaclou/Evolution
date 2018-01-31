using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    public int populationSize = 120;
    public Material playerMat;
    public Agent playerAgent;
    public Agent[] agentsArray;

    public AgentGenome[] genomePoolArray;
    
    public void InitializeNewSimulation() {

        // Create Environment (Or have it pre-built)

        // Create initial population of Genomes:
        // Re-Factor:
        BodyGenome bodyGenomeTemplate = new BodyGenome();
        bodyGenomeTemplate.InitializeGenomeAsDefault();
        genomePoolArray = new AgentGenome[populationSize];
        // Player's dummy Genome (required to initialize Agent Class):
        AgentGenome playerGenome = new AgentGenome(-1);
        playerGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
        playerGenome.InitializeRandomBrainFromCurrentBody(0.0f);
        for(int i = 0; i < genomePoolArray.Length; i++) {   // Create initial Population
            AgentGenome agentGenome = new AgentGenome(i);
            agentGenome.InitializeBodyGenomeFromTemplate(bodyGenomeTemplate);
            agentGenome.InitializeRandomBrainFromCurrentBody(1f);
            genomePoolArray[i] = agentGenome;
        }

        // Instantiate Player Agent
        string assetURL = "AgentPrefab";
        GameObject playerAgentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
        playerAgentGO.name = "PlayerAgent";
        playerAgentGO.GetComponent<MeshRenderer>().material = playerMat;
        playerAgentGO.GetComponent<Rigidbody2D>().mass = 10f;
        playerAgentGO.transform.localScale = new Vector3(1.5f, 1.5f, 0.33f);
        //How to handle initial Placement?
        //agentGO.transform.localPosition = currentEvalTicket.environmentGenome.agentStartPositionsList[i].agentStartPosition;
        //.transform.localRotation = currentEvalTicket.environmentGenome.agentStartPositionsList[i].agentStartRotation;
        //agentGO.GetComponent<CircleCollider2D>().enabled = false;
        playerAgent = playerAgentGO.AddComponent<Agent>();
        playerAgent.humanControlled = true;
        playerAgent.speed *= 10f;
        //agentScript.isVisible = visible;
        StartPositionGenome playerStartPosGenome = new StartPositionGenome(Vector3.zero, Quaternion.identity);
        playerAgent.InitializeAgentFromGenome(playerGenome, playerStartPosGenome);
        //currentAgentsArray[i] = agentScript;


        // Instantiate AI Agents
        agentsArray = new Agent[populationSize];
        for (int i = 0; i < agentsArray.Length; i++) {
            GameObject agentGO = Instantiate(Resources.Load(assetURL)) as GameObject;
            agentGO.name = "Agent" + i.ToString();
            Agent newAgent = agentGO.AddComponent<Agent>();
            StartPositionGenome agentStartPosGenome = new StartPositionGenome(new Vector3(1.1f * (float)(i/8), -1.1f - 1.1f * (i / 8), 0f), Quaternion.identity);
            agentsArray[i] = newAgent; // Add to stored list of current Agents
            newAgent.InitializeAgentFromGenome(genomePoolArray[i], agentStartPosGenome);
        }

        //Hookup Agent Modules to their proper Objects/Transforms/Info
        playerAgent.testModule.enemyTestModule = agentsArray[0].testModule;
        for (int p = 0; p < agentsArray.Length; p++) {
            agentsArray[p].testModule.enemyTestModule = playerAgent.testModule;
            //for (int e = 0; e < currentAgentsArray.Length; e++) {
            //if (e != p) {  // not vs self:
                    //currentAgentsArray[p].testModule.ownRigidBody2D = currentAgentsArray[p].GetComponent<Rigidbody2D>();
                    //currentAgentsArray[p].testModule.enemyTestModule = currentAgentsArray[e].testModule;
                    //currentAgentsArray[p].testModule.enemyPosX[0] = currentAgentsArray[e].testModule.posX[0] - currentAgentsArray[p].testModule.posX[0];
                    //currentAgentsArray[p].testModule.enemyPosY[0] = currentAgentsArray[e].testModule.posY[0] - currentAgentsArray[p].testModule.posY[0];
                //}
            //}
        }
    }

    public void TickSimulation() {
        playerAgent.Tick();
        for (int i = 0; i < agentsArray.Length; i++) {
            agentsArray[i].Tick();
        }
    }
}
