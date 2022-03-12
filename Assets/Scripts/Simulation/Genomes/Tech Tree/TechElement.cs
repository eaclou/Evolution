using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Element")]
public class TechElement : ScriptableObject
{
    [Tooltip("Potential future use")]
    public TechCategory category;
    
    [Tooltip("Used internally for code-based lookups")]
    public TechElementId id;
    
    [Tooltip("Static tooltip text when cursor hovers over icon")]
    public string iconTooltip;
    
    [Tooltip("Probability a mutation will unlock this ability if all prerequisites are met.")]
    [Range(0, 1)] public float mutationUnlockChance;
    
    [Tooltip("Probability a mutation will lock this ability if it is not the prerequisite for another active ability.")]
    [Range(0, 1)] public float mutationLockChance;

    [Tooltip("Other tech that must be unlocked before this tech can be unlocked")]
    public TechElement[] prerequisites;
    
    // * ERROR PRONE! Consider centralizing in Lookup with tree structure (or setting via OnValidate)
    [Tooltip("Other tech that becomes available when this tech is unlocked")]
    public TechElement[] nextTech;
    
    [Tooltip("Neurons activated by this tech")]
    public MetaNeuron[] unlocks;
    
    public bool HasPrerequisites(TechElement[] abilities)
    {
        foreach (var prerequisite in prerequisites)
            if (!HasPrerequisite(abilities, prerequisite))
                return false;
                
        return true;
    }
    
    bool HasPrerequisite(TechElement[] abilities, TechElement prerequisite)
    {
        foreach (var ability in abilities)
            if (ability == prerequisite)
                return true;
                
        return false;
    }
    
    public bool IsPrerequisite(List<TechElement> abilities)
    {
        foreach (var ability in abilities)
            foreach (var prerequisite in ability.prerequisites)
                if (prerequisite == this)
                    return true;
        
        return false;        
    }
    
    public bool Contains(MetaNeuron value)
    {
        foreach (var unlock in unlocks)
            if (unlock == value)
                return true;
        
        return false;
    }
}

public enum TechElementId
{
    Root,
    Social,
    InComms01,
    InComms23,
    OutComms01,
    OutComms23,
    FriendDist,
    FriendVel,
    Protect,
    FoodSensors,
    Nutrients,
    PlantsDir,
    PlantsDist,
    PlantsVel,
    MicrobesDir,
    MicrobesDist,
    MicrobesVel,
    SelfStatus,
    Health,
    Lifestage,
    Energy,
    OwnVel,
    OwnDir,
    Swim, //throttle
    Dash,
    Sleep,
    IsContact,
    ContactXY,
    Water,
    Predation, //bite
    Attack,
    PreyDir,
    PreyDist,
    PreyVel,
    CorpseDir,
    CorpseDist,
    CorpseVel,
    EggDir
}