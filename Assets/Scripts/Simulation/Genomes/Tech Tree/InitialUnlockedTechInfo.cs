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
    
    public void GetMutatedCopy()
    {
        var copy = new UnlockedTech(this);
        
        
    }
}