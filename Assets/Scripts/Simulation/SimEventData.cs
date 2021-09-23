using System;
using Playcraft;
using Random = UnityEngine.Random;

[Serializable]
public class SimEventData 
{
    Lookup lookup => Lookup.instance;
    SimEventLookup events => lookup.simEvents;

    public string name = "Null Event";
    public string description = "This is the longer description";
    public int cost = 10;
    public int duration = 1;  // in sim steps, or days?    
    public SimEventTypeMinor typeMinor;
    public SimEventTypeMajor typeMajor;
    public SimEventTypeExtreme typeExtreme;
    public SimEventCategories category;  // minor major extreme
    public SpeciesQualifier speciesQualifier;
    public int quantity;
    public bool isPositive;
    public bool polarity;
    public int timeStepActivated;

    /*public enum SimEventType {
        FoodDecay,
        FoodPlant,
        FoodEgg,
        FoodCorpse,
        FoodAll,
        SpeciesMostFit,        
        SpeciesLeastFit,
        SpeciesMostNovel,
        SpeciesLeastNovel,
        BrainMutationBasic,
        BrainMutationGrowth,
        BodyMutationBasic,
        BodyMutationModule,
        WaterCurrents
    }*/

    public SimEventData() { }
    
    public SimEventData(int cost, int quantity, SimEventCategories category)
    {
        this.cost = cost;
        this.category = category;
        this.quantity = quantity;
        isPositive = Random.Range(0f, 1f) >= 0.5f;
        polarity = Random.Range(0f, 1f) >= 0.5f;
    }

    public SimEventData(string name, int timeStepActivated, SimEventCategories category = SimEventCategories.NPE) {
        this.name = name;
        this.category = category;
        this.timeStepActivated = timeStepActivated;
    }
    
    public static SimEventTypeMinor GetRandomMinorEventType() { return RandomStatics.RandomEnumValue<SimEventTypeMinor>(); }
    public static SimEventTypeMajor GetRandomMajorEventType() { return RandomStatics.RandomEnumValue<SimEventTypeMajor>(); }
    public static SimEventTypeExtreme GetRandomExtremeEventType() { return RandomStatics.RandomEnumValue<SimEventTypeExtreme>(); }
    
    public void Refresh(SpeciesQualifier qualifier = SpeciesQualifier.Random)
    {
        var data = events.GetEventData(this);
        if (data == null) return;
        
        name = data.setQualifier ? GetQualifiedText(data.GetName(), qualifier, isPositive) : data.GetName(isPositive);
        description = data.setQualifier ? GetQualifiedText(data.GetDescription(), qualifier, isPositive, quantity) : data.GetDescription(isPositive);
        if (data.setQualifier) speciesQualifier = data.speciesQualifier;
    }
    
    public static string GetQualifiedText(string original, SpeciesQualifier qualifierId, bool polarity, int quantity = -1)
    {
        var qualification = GetQualifier(qualifierId, polarity);
        var modified = original.Replace("{*}", qualification);
        modified = modified.Replace("{*1}", quantity.ToString());
        return modified;
    }
    
    public static string GetQualifier(SpeciesQualifier qualifier, bool polarity)
    {
        switch (qualifier)
        {
            case SpeciesQualifier.Age: return polarity ? "Oldest" : "Youngest";
            case SpeciesQualifier.BodySize: return polarity ? "Largest" : "Smallest";
            case SpeciesQualifier.Fitness: return polarity ? "Most Fit" : "Least Fit";
            case SpeciesQualifier.Novelty: return polarity ? "Most Unique" : "Most Average";
            default: return "";
        }
    }
}

public enum SimEventCategories {
    Minor,
    Major,
    Extreme,
    NPE
}

public enum SimEventTypeMinor {
    FoodDecay,
    FoodPlant,
    FoodEgg,
    FoodCorpse,
    CreateSpecies,
    KillSpecies,
    BrainMutation,
    BrainSize,        
    BodyMutation,
    BodyModules,
    CalmWaters        
}
    
public enum SimEventTypeMajor {
    FoodDecay,
    FoodPlant,
    FoodEgg,
    FoodCorpse,
    //FoodAll,
    CreateSpecies,
    KillSpecies,
    BrainMutation,
    BrainSize,        
    BodyMutation,
    BodyModules,
    Gale 
}
    
public enum SimEventTypeExtreme {
    FoodDecay,
    FoodPlant,
    FoodEgg,
    FoodCorpse,
    FoodAll,
    CreateSpecies,
    KillSpecies,
    BrainMutation,
    BrainSize,        
    BodyMutation,
    BodyModules,
    Hurricane 
}

public enum SpeciesQualifier {
    Random,  // Minor cutoff
    Fitness,
    Novelty,
    BodySize, 
    Age,   // Major Cutoff
    SpecAttack,
    SpecDefend,
    SpecSpeed,
    SpecUtility,
    SpecDecay,
    SpecPlant,
    SpecMeat
}