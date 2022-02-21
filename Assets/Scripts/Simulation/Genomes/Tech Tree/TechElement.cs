using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Element")]
public class TechElement : ScriptableObject
{
    [Tooltip("Potential future use")]
    public TechCategory category;
    
    [Tooltip("Used internally for code-based lookups")]
    public TechElementId id;
    
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
}

public enum TechElementId
{
    Heterotrophic,
    Jaw,
    SimpleMouth,
    Teeth,
    VocalCords,
    Dash,
    DorsalFin,
    Faster,
    LimbDifferentiation,
    Maneuver,
    Spin,
    Spines,
    TailFin,
    AnimalDiet,
    DigestiveTract,
    EggDiet,
    Eggs,
    FatCells,
    Intestines,
    Lysosomes,
    MicrobeDiet,
    PlantDiet,
    Stomach,
    FoodSensor,
    MicrobeSensor,
    PlantSensor,
    AnimalSensor,
    CorpseSensor,
    EggSensor,
    WaterSensor,
    Ears,
    Eyes,
    Proximity,
    Targeting,
    Biometrics,
    Clock,
    Dream,
    Hormones,
    Proprioception,
    SensoryGanglia,
    Sleep,
    BiteResistant,
    BodySize,
    Brain,
    Catalysts,
    Gills,
    Mitochondria,
    Organs,
    Plates,
    Poisonous,
    Scales,
    SpinalCord,
    VenomGland,
    NerveCells,
    Unknown,
}