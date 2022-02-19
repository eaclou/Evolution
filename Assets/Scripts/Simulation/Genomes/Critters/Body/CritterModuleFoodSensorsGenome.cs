/*
using Playcraft;

/// DEPRECATE
public class CritterModuleFoodSensorsGenome 
{
    public readonly BrainModuleID moduleID = BrainModuleID.FoodSensors;

    public bool useNutrients;
    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;
    public bool useEggs;
    public bool useCorpse;

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
        //preferredSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferredSize, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 0f, 1f);
        //sensorRangeMult = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.sensorRangeMult, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 0f, 1f);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
*/

#region OBSOLETE: use TechElement -> unlocks (delete on verify)
/*
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;

List<NeuronGenome> masterList;
public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
{
    this.masterList = masterList;
    
    if (useNutrients) 
    {
        AddNeuron("nutrientDensity");   
        AddNeuron("nutrientGradX");
        AddNeuron("nutrientGradY");
    }
    if (usePos) 
    {
        AddNeuron("foodPosX");
        AddNeuron("foodPosY");
        AddNeuron("foodDistance");
        AddNeuron("animalPosX");
        AddNeuron("animalPosY");
        AddNeuron("animalDistance");
    }
    if (useVel) 
    {
        AddNeuron("foodVelX");
        AddNeuron("foodVelY");
        AddNeuron("animalVelX");
        AddNeuron("animalVelY");
    }
    if (useDir) 
    {
        AddNeuron("foodDirX");
        AddNeuron("foodDirY");
        AddNeuron("animalDirX");
        AddNeuron("animalDirY");
    }
    if (useStats) 
    {
        AddNeuron("foodQuality");
        AddNeuron("foodRelSize");
        AddNeuron("animalQuality");
        AddNeuron("animalRelSize");
    }
    if (useEggs) 
    {
        AddNeuron("distanceToEgg");
        AddNeuron("eggDirX");
        AddNeuron("eggDirY");
    }
    if (useCorpse) 
    {
        AddNeuron("distanceToCorpse");
        AddNeuron("corpseDirX");
        AddNeuron("corpseDirY");
    }
}

void AddNeuron(string name) { masterList.Add(map.GetData(name)); }
*/
#endregion