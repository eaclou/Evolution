using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Element")]
public class TechElement : ScriptableObject
{
    // * Not sure if this is needed
    public TechCategory category;
    public TechElementId id;
    
    // Not yet implemented
    [Tooltip("Probability a mutation will unlock this ability if all prerequisites are met.")]
    [Range(0, 1)] public float mutationUnlockChance;
    
    // Not yet implemented
    [Tooltip("Probability a mutation will lock this ability if it is not the prerequisite for another active ability.")]
    [Range(0, 1)] public float mutationLockChance;

    public TechElement[] prerequisites;
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
    Priopriception,
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
}