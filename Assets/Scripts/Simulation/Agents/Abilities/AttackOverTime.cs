using UnityEngine;

public class AttackOverTime : IAgentAbility
{
    Agent agent;
    Collider2D mouth => agent.mouthRef.triggerCollider;
    CandidateAgentData candidate => agent.candidateRef;
    
    AbilityProcess process;

    public AttackOverTime(Agent agent, int actionDuration, int cooldownDuration, OnAbilityComplete cooldown)
    {
        this.agent = agent;
        process = new AbilityProcess(actionDuration, cooldownDuration, cooldown);
    }
    
    public bool inProcess => process.inProcess;
    public int frameCount => process.frameCount;

    public void Begin()
    {
        if (inProcess) return;
        
        mouth.enabled = true;
        candidate.performanceData.totalTimesAttacked++;
        
        process.Begin();
    }
    
    public void End() { process.End(); }
}
