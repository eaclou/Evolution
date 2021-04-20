using UnityEngine;

public class Feed : IAgentAbility
{    
    Agent agent;
    CritterMouthComponent mouth => agent.mouthRef;
    Collider2D mouthCollider => mouth.triggerCollider;
    
    AbilityProcess process;
    
    public Feed(Agent agent, int actionDuration, int cooldownDuration, OnAbilityComplete cooldown)
    {
        this.agent = agent;
        process = new AbilityProcess(actionDuration, cooldownDuration, cooldown);
    }
    
    public bool inProcess => process.inProcess;
    public int frameCount => process.frameCount;
    
    public void Begin()
    {
        if (inProcess || agent.isAttacking || agent.isDefending) 
            return;
            
        mouthCollider.enabled = true;
        mouth.lastBiteFoodAmount = 0f;
            
        process.Begin();
    }
    
    public void End() { process.End(); }
    //if(mouthRef.lastBiteFoodAmount > 0f) {
    //RegisterAgentEvent(UnityEngine.Time.frameCount, "Ate Plant/Meat! (" + mouthRef.lastBiteFoodAmount.ToString("F3") + ")");
    //}
}