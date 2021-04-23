
public class Dash : IAgentAbility
{    
    Agent agent;
    CandidateAgentData candidate => agent.candidateRef;
    CritterModuleCore coreModule => agent.coreModule;
    bool outOfStamina => coreModule.stamina[0] < 0.1f; //***EAC q4will: how does this work under-the-hood?  is it updated everytime it is Get, or only once at Initialization?
    
    AbilityProcess process;
    
    public Dash(Agent agent, int actionDuration, int cooldownDuration, OnAbilityComplete cooldown)
    {
        this.agent = agent;
        process = new AbilityProcess(actionDuration, cooldownDuration, cooldown);
    }
    
    public bool inProcess => process.inProcess;
    public int frameCount => process.frameCount;
    
    public void Begin()
    {
        if (inProcess || outOfStamina) 
            return;
            
        coreModule.stamina[0] -= 0.1f;
        candidate.performanceData.totalTimesDashed++;
            
        process.Begin();
    }
    
    public void End() { process.End(); }
}
