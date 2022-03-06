using System;
using System.Collections.Generic;
using UnityEngine;
using Playcraft;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Initial Ability Set")]
public class InitialUnlockedTechInfo : ScriptableObject
{ 
    public InitialTechElement[] potentialValues;
    
    public UnlockedTech GetInitialUnlocks() 
    {
        List<TechElement> values = new List<TechElement>();
        
        foreach (var value in potentialValues)
            if (RandomStatics.CoinToss(value.probability))
                values.Add(value.value);
     
        return new UnlockedTech(values); 
    }
}

[Serializable]
public class InitialTechElement
{
    public TechElement value;
    [Range(0, 1)] public float probability;
}

/// Per agent list management of TechElements
[Serializable]
public class UnlockedTech
{
    public List<TechElement> values;
    
    public UnlockedTech(List<TechElement> values) { this.values = values; }
    
    /// Deep copy
    public UnlockedTech(UnlockedTech original)
    {
        values = new List<TechElement>();
        
        for (int i = 0; i < original.values.Count; i++)
            values.Add(original.values[i]);
    }
    
    public bool Contains(TechElementId id) 
    { 
        foreach (var value in values)
            if (value.id == id)
                return true;
                
        return false;
    }
    
    public UnlockedTech GetMutatedCopy()
    {
        var copy = new UnlockedTech(this);
        var removed = new List<TechElement>();
        var added = new List<TechElement>();
        
        // Random chance of removing a tech if it is not the prerequisite of some other tech the agent has.
        foreach (var value in values)
            if (!value.IsPrerequisite(values) && RandomStatics.CoinToss(value.mutationLockChance))
                removed.Add(value);
                
        foreach (var remove in removed)
            values.Remove(remove);

        // Random chance of adding a tech if its prerequisite is met and the agent doesn't already have it.
        foreach (var value in values)
            foreach (var tech in value.nextTech)
                if (RandomStatics.CoinToss(tech.mutationUnlockChance) && !values.Contains(tech))
                    added.Add(tech);
        
        foreach (var add in added)
            copy.values.Add(add);
            
        return copy;
    }
}