using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public enum SimEventCategories {
        Minor,
        Major,
        Extreme,
        NPE
    }

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
    

	public SimEventData() {

    }
}
