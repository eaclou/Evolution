using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleFriendSensorsGenome
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;

    public readonly BrainModuleID moduleID = BrainModuleID.FriendSensors;

    public bool usePos;
    public bool useVel;
    public bool useDir;

    public float sensorRange;
    

    public void InitializeRandom() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
    }
    
    public void Initialize(UnlockedTech unlockedTech) {
        // usePos = unlockedTech.Contains(TechElementId.???);
        // useVel = unlockedTech.Contains(TechElementId.???);
        // useDir = unlockedTech.Contains(TechElementId.???);
    }
    
    List<NeuronGenome> masterList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
        
        if (usePos) 
        {
            AddNeuron("friendPosX");
            AddNeuron("friendPosY");
        }
        if (useVel) 
        {
            AddNeuron("friendVelX");
            AddNeuron("friendVelY");
        }
        if (useDir) 
        {
            AddNeuron("friendDirX");
            AddNeuron("friendDirY");
        }
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }

    public void SetToMutatedCopyOfParentGenome(CritterModuleFriendSensorsGenome parentGenome, MutationSettingsInstance settings) {
        usePos = RequestMutation(settings, parentGenome.usePos);
        useVel = RequestMutation(settings, parentGenome.useVel);
        useDir = RequestMutation(settings, parentGenome.useDir);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
