using System.Collections;
using Playcraft;

public delegate void OnAbilityComplete(int duration);

public class AbilityProcess
{
    MonoSim mono => MonoSim.instance;

    public bool inProcess;
    public int frameCount;
    
    public int actionDuration;
    readonly int cooldownDuration;
    readonly OnAbilityComplete cooldown;
    
    public AbilityProcess(int actionDuration, int cooldownDuration, OnAbilityComplete cooldown)
    {
        this.actionDuration = actionDuration;
        this.cooldownDuration = cooldownDuration;
        this.cooldown = cooldown;
    }
    
    public AbilityProcess() { }
    
    public void Begin() { mono.SimRoutine(Process); }

    public IEnumerator Process() 
    {            
        inProcess = true;
        frameCount = 0;
                
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
        cooldown?.Invoke(cooldownDuration);
    }
}
