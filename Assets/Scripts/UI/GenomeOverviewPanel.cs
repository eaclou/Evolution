using UnityEngine;
using UnityEngine.UI;

public class GenomeOverviewPanel : MonoBehaviour
{
    SimulationManager simulation => SimulationManager.instance;

    // WPP: use descriptive names instead of A,B,C
    public Text bodySize;
    public Text species;
    public Text brainSize;
    
    AgentGenome genome;
    BrainGenome brain;
    CritterModuleCoreGenome core;

    public void Refresh(CandidateAgentData agent)
    {
        genome = agent.candidateGenome;
        brain = genome.brainGenome;
        core = genome.bodyGenome.coreGenome;
    
        float lifespan = agent.performanceData.totalTicksAlive;
        
        bodySize.text = "Lifespan: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + genome.generationCount;

        if(agent.isBeingEvaluated) {
            // [WPP: peek reference to target agent for defect note]
            lifespan = simulation.targetAgentAge;
            bodySize.text = "Age: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + genome.generationCount;
        }
        //if (simulationManager.targetAgentIsDead) {   
        //if(simulationManager.agentsArray[cameraManager.targetAgentIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
        //    imageDeadDim.gameObject.SetActive(true);
        //}

        species.text = "Size: " + (100f * core.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / core.creatureAspectRatio).ToString("F0");
        brainSize.text = "Brain Size: " + brain.bodyNeuronList.Count + "--" + brain.linkList.Count;
    }
}
