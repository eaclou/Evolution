using System.Collections.Generic;
using Playcraft;

public class CritterModuleFoodSensorsGenome 
{
    public int parentID;
    public int inno;

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

    public CritterModuleFoodSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
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
            NeuronGenome nutrientDensity = new NeuronGenome("nutrientDensity", NeuronType.In, inno, 1);
            NeuronGenome nutrientGradX = new NeuronGenome("nutrientGradX", NeuronType.In, inno, 2);
            NeuronGenome nutrientGradY = new NeuronGenome("nutrientGradY", NeuronType.In, inno, 3);
            neuronList.Add(nutrientDensity);
            neuronList.Add(nutrientGradX);
            neuronList.Add(nutrientGradY);
        }
        if(usePos) {
            NeuronGenome foodPosX = new NeuronGenome("foodPosX", NeuronType.In, inno, 4);
            NeuronGenome foodPosY = new NeuronGenome("foodPosY", NeuronType.In, inno, 5);
            NeuronGenome distance = new NeuronGenome("distance", NeuronType.In, inno, 6);
            neuronList.Add(foodPosX);
            neuronList.Add(foodPosY);
            neuronList.Add(distance);

            NeuronGenome animalPosX = new NeuronGenome("animalPosX", NeuronType.In, inno, 24);
            NeuronGenome animalPosY = new NeuronGenome("animalPosY", NeuronType.In, inno, 25);
            NeuronGenome animalDistance = new NeuronGenome("animalDistance",NeuronType.In, inno, 26);
            neuronList.Add(animalPosX);
            neuronList.Add(animalPosY);
            neuronList.Add(animalDistance);
        }
        if(useVel) {
            NeuronGenome foodVelX = new NeuronGenome("foodVelX", NeuronType.In, inno, 7);
            NeuronGenome foodVelY = new NeuronGenome("foodVelY", NeuronType.In, inno, 8);
            neuronList.Add(foodVelX);
            neuronList.Add(foodVelY);

            NeuronGenome animalVelX = new NeuronGenome("animalVelX", NeuronType.In, inno, 27);
            NeuronGenome animalVelY = new NeuronGenome("animalVelY", NeuronType.In, inno, 28);
            neuronList.Add(animalVelX);
            neuronList.Add(animalVelY);
        }
        if(useDir) {
            NeuronGenome foodDirX = new NeuronGenome("foodDirX", NeuronType.In, inno, 9);
            NeuronGenome foodDirY = new NeuronGenome("foodDirY", NeuronType.In, inno, 10);
            neuronList.Add(foodDirX);
            neuronList.Add(foodDirY);

            NeuronGenome animalDirX = new NeuronGenome("animalDirX", NeuronType.In, inno, 29);
            NeuronGenome animalDirY = new NeuronGenome("animalDirY", NeuronType.In, inno, 30);
            neuronList.Add(animalDirX);
            neuronList.Add(animalDirY);
        }
        if(useStats) {
            NeuronGenome foodQuality = new NeuronGenome("foodQuality", NeuronType.In, inno, 11);
            NeuronGenome foodRelSize = new NeuronGenome("foodRelSize", NeuronType.In, inno, 12);
            neuronList.Add(foodQuality);
            neuronList.Add(foodRelSize);

            NeuronGenome animalQuality = new NeuronGenome("animalQuality", NeuronType.In, inno, 31);
            NeuronGenome animalRelSize = new NeuronGenome("animalRelSize", NeuronType.In, inno, 32);
            neuronList.Add(animalQuality);
            neuronList.Add(animalRelSize);
        }
        if(useEggs) {
            NeuronGenome distance = new NeuronGenome("distance", NeuronType.In, inno, 13);
            NeuronGenome eggDirX = new NeuronGenome("eggDirX", NeuronType.In, inno, 14);
            NeuronGenome eggDirY = new NeuronGenome("eggDirY", NeuronType.In, inno, 15);
            neuronList.Add(distance);
            neuronList.Add(eggDirX);
            neuronList.Add(eggDirY);
        }
        if(useCorpse) {
            NeuronGenome distance = new NeuronGenome("distance", NeuronType.In, inno, 16);
            NeuronGenome corpseDirX = new NeuronGenome("corpseDirX", NeuronType.In, inno, 17);
            NeuronGenome corpseDirY = new NeuronGenome("corpseDirY", NeuronType.In, inno, 18);
            neuronList.Add(distance);
            neuronList.Add(corpseDirX);
            neuronList.Add(corpseDirY);
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
