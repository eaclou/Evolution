using UnityEngine;

public enum SimEventCategories {
    Minor,
    Major,
    Extreme,
    NPE
}

public class SimEventData {

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
}
