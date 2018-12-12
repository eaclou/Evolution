using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimEventData {

    public string name = "Null Event";
    public string description = "This is the longer description";
    public int cost = 10;
    public int duration = 1;  // in sim steps, or days?    
    public SimEventType type;
    public int simEventTier;
    public bool isPositive;

    public enum SimEventType {
        FoodDecay,
        FoodPlant,
        FoodEgg,
        FoodCorpse,
        FoodAll,
        SpeciesMostFit,        
        SpeciesLeastFit,
        SpeciesMostNovel,
        SpeciesLeastNovel,
        BrainMutation,
        BodyMutation,
        WaterCurrents
    }

    public enum SimEventAction {
        Extinct,
        NewSpecies
    }

    public enum SimEventQualifier {
        MostFit,
        LeastFit,
        MostNovel,
        LeastNovel,
        Largest,
        Smallest,
        Oldest,
        Youngest
    }
    

	public SimEventData() {

    }


}
