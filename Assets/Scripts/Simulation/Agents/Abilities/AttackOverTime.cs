using System.Collections;
using UnityEngine;
using Playcraft;

public class AttackOverTime : IAgentAbility
{
    MonoSim mono => MonoSim.instance;

    public bool inProcess { get; private set; }
    public int frameCount { get; private set; }

    Agent agent;
    Collider2D mouth => agent.mouthRef.triggerCollider;
    CandidateAgentData candidate => agent.candidateRef;
    
    int actionDuration;
    int cooldownDuration;
    
    public delegate void OnComplete(int duration);
    OnComplete cooldown;

    public AttackOverTime(Agent agent, int actionDuration, int cooldownDuration, OnComplete cooldown)
    {
        this.agent = agent;
        this.actionDuration = actionDuration;
        this.cooldownDuration = cooldownDuration;
        this.cooldown = cooldown;        
    }

    public void Begin()
    {
        if (inProcess) return;
        mono.SimRoutine(Process);
    }
    
    IEnumerator Process() 
    {            
        inProcess = true;
        mouth.enabled = true;
        frameCount = 0;
        
        candidate.performanceData.totalTimesAttacked++;
                
        for (int i = 0; i < actionDuration; i++) 
        {
            frameCount++;            
            yield return null;
        }
        
        End();
    }
    
    public void End()
    {
        inProcess = false;
        frameCount = 0;
        cooldown(cooldownDuration);          
    }
}
