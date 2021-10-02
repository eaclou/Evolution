using System.Collections.Generic;
using Playcraft;

public class CritterModuleFoodSensorsGenome 
{
    public int parentID;
    //public int inno;    // * WPP: rename or comment (module ID?)
    public readonly BrainModuleID moduleID = BrainModuleID.FoodSensors;

    public bool useNutrients;
    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;
    public bool useEggs;
    public bool useCorpse;

    // 0-1, determines what will be chosen by creature as its current food target
    //public float preferenceParticles;
    //public float preferenceEggs;
    //public float preferenceCreatures;
    public float preferredSize;

    public float sensorRangeMult;

    public CritterModuleFoodSensorsGenome(int parentID) {
        this.parentID = parentID;
    }

    public void GenerateRandomInitialGenome() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
        useStats = RandomStatics.CoinToss();
        useNutrients = RandomStatics.CoinToss();
        useEggs = RandomStatics.CoinToss();
        useCorpse = RandomStatics.CoinToss();

        preferredSize = 0.5f;
        sensorRangeMult = 1f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useNutrients) {
            neuronList.Add(new NeuronGenome("nutrientDensity", NeuronType.In, moduleID, 1));
            neuronList.Add(new NeuronGenome("nutrientGradX", NeuronType.In, moduleID, 2));
            neuronList.Add(new NeuronGenome("nutrientGradY", NeuronType.In, moduleID, 3));
        }
        if(usePos) {
            neuronList.Add(new NeuronGenome("foodPosX", NeuronType.In, moduleID, 4));
            neuronList.Add(new NeuronGenome("foodPosY", NeuronType.In, moduleID, 5));
            neuronList.Add(new NeuronGenome("distance", NeuronType.In, moduleID, 6));
            neuronList.Add(new NeuronGenome("animalPosX", NeuronType.In, moduleID, 24));
            neuronList.Add(new NeuronGenome("animalPosY", NeuronType.In, moduleID, 25));
            neuronList.Add(new NeuronGenome("animalDistance",NeuronType.In, moduleID, 26));
        }
        if(useVel) {
            neuronList.Add(new NeuronGenome("foodVelX", NeuronType.In, moduleID, 7));
            neuronList.Add(new NeuronGenome("foodVelY", NeuronType.In, moduleID, 8));
            neuronList.Add(new NeuronGenome("animalVelX", NeuronType.In, moduleID, 27));
            neuronList.Add(new NeuronGenome("animalVelY", NeuronType.In, moduleID, 28));
        }
        if(useDir) {
            neuronList.Add(new NeuronGenome("foodDirX", NeuronType.In, moduleID, 9));
            neuronList.Add(new NeuronGenome("foodDirY", NeuronType.In, moduleID, 10));

            neuronList.Add(new NeuronGenome("animalDirX", NeuronType.In, moduleID, 29));
            neuronList.Add(new NeuronGenome("animalDirY", NeuronType.In, moduleID, 30));
        }
        if(useStats) {
            neuronList.Add(new NeuronGenome("foodQuality", NeuronType.In, moduleID, 11));
            neuronList.Add(new NeuronGenome("foodRelSize", NeuronType.In, moduleID, 12));
            neuronList.Add(new NeuronGenome("animalQuality", NeuronType.In, moduleID, 31));
            neuronList.Add(new NeuronGenome("animalRelSize", NeuronType.In, moduleID, 32));
        }
        if(useEggs) {
            neuronList.Add(new NeuronGenome("distance", NeuronType.In, moduleID, 13));
            neuronList.Add(new NeuronGenome("eggDirX", NeuronType.In, moduleID, 14));
            neuronList.Add(new NeuronGenome("eggDirY", NeuronType.In, moduleID, 15));
        }
        if(useCorpse) {
            neuronList.Add(new NeuronGenome("distance", NeuronType.In, moduleID, 16));
            neuronList.Add(new NeuronGenome("corpseDirX", NeuronType.In, moduleID, 17));
            neuronList.Add(new NeuronGenome("corpseDirY", NeuronType.In, moduleID, 18));
        }
    }
    
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFoodSensorsGenome parentGenome, MutationSettingsInstance settings) {
        useNutrients = RequestMutation(settings, parentGenome.useNutrients);
        usePos = RequestMutation(settings, parentGenome.usePos);
        useVel = RequestMutation(settings, parentGenome.useVel);
        useDir = RequestMutation(settings, parentGenome.useDir);
        useStats = RequestMutation(settings, parentGenome.useStats);
        useEggs = RequestMutation(settings, parentGenome.useEggs);
        useCorpse = RequestMutation(settings, parentGenome.useCorpse);

        //preferenceParticles = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceParticles, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        //preferenceEggs = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceEggs, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        //preferenceCreatures = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceCreatures, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        preferredSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferredSize, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 0f, 1f);
        sensorRangeMult = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.sensorRangeMult, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 0f, 1f);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
