using UnityEngine;

public class Cooldown : IAgentAbility
{    
    AbilityProcess process;
    public Cooldown() { process = new AbilityProcess(); }
    
    public int duration { set => process.actionDuration = value; }
    
    public bool inProcess => process.inProcess;
    public int frameCount => process.frameCount;
        
    public void Begin() { process.Begin(); }
    public void End() { process.End(); }
}