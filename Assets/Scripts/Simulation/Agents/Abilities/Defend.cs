public class Defend : IAgentAbility
{    
    Agent agent;
    CandidateAgentData candidate => agent.candidateRef;
    CritterModuleCore coreModule => agent.coreModule;
    bool outOfStamina => coreModule.stamina[0] < 0.1f;
    
    AbilityProcess process;
    
    public Defend(Agent agent, int actionDuration, int cooldownDuration, OnAbilityComplete cooldown)
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
        candidate.performanceData.totalTimesDefended++;
            
        process.Begin();
    }
    
    public void End() { process.End(); }
}