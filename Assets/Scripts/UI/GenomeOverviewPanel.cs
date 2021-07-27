using UnityEngine;
using UnityEngine.UI;

public class GenomeOverviewPanel : MonoBehaviour
{
    SimulationManager simulation => SimulationManager.instance;
    CandidateAgentData agent => UIManager.instance.focusedCandidate;

    // WPP: use descriptive names instead of A,B,C    
    public Text textGeneration;
    public Text textBodySize;
    public Text textBrainSize;
       
    
    AgentGenome genome;
    BrainGenome brain;
    CritterModuleCoreGenome core;

    public void Refresh()
    {
        genome = agent.candidateGenome;
        brain = genome.brainGenome;
        core = genome.bodyGenome.coreGenome;
    
        float lifespan = agent.performanceData.totalTicksAlive;
        
        textGeneration.text = "Gen: " + genome.generationCount;
        textBodySize.text = "Size: " + (100f * core.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / core.creatureAspectRatio).ToString("F0");
        textBrainSize.text = "Brain Size: " + brain.bodyNeuronList.Count + "--" + brain.linkList.Count;
    }
}
