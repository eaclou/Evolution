/*
using System;
using System.Collections.Generic;
using Playcraft;

/// DEPRECATE
[Serializable]
public class CritterModuleThreatSensorsGenome 
{
    public readonly BrainModuleID moduleID = BrainModuleID.ThreatSensors;

    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;

    
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;
    
    public void InitializeRandom() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
        useStats = RandomStatics.CoinToss();
    }

    List<NeuronGenome> masterList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
        
        if (usePos) 
        {
            AddNeuron("enemyPosX");
            AddNeuron("enemyPosY");
        }
        if (useVel) 
        {
            AddNeuron("enemyVelX");
            AddNeuron("enemyVelY");
        }
        if (useDir) 
        {
            AddNeuron("enemyDirX");
            AddNeuron("enemyDirY");
        }
        if (useStats) 
        {
            AddNeuron("enemyRelSize");
            AddNeuron("enemyHealth");
            AddNeuron("enemyGrowthStage");
            AddNeuron("enemyThreatRating");
        }
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }
    

    public void SetToMutatedCopyOfParentGenome(CritterModuleThreatSensorsGenome parentGenome, MutationSettingsInstance settings) {
        usePos = RequestMutation(settings, parentGenome.usePos);
        useVel = RequestMutation(settings, parentGenome.useVel);
        useDir = RequestMutation(settings, parentGenome.useDir);
        useStats = RequestMutation(settings, parentGenome.useStats);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
*/
