using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleThreatSensorsGenome 
{
    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.ThreatSensors;

    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;

    public CritterModuleThreatSensorsGenome(int parentID) {
        this.parentID = parentID;
    }

    public void GenerateRandomInitialGenome() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
        useStats = RandomStatics.CoinToss();
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if (usePos) {
            neuronList.Add(new NeuronGenome("enemyPosX", NeuronType.In, moduleID, 14));
            neuronList.Add(new NeuronGenome("enemyPosY", NeuronType.In, moduleID, 15));
        }
        if (useVel) {
            neuronList.Add(new NeuronGenome("enemyVelX", NeuronType.In, moduleID, 16));
            neuronList.Add(new NeuronGenome("enemyVelY", NeuronType.In, moduleID, 17));
        }
        if (useDir) {
            neuronList.Add(new NeuronGenome("enemyDirX", NeuronType.In, moduleID, 18));
            neuronList.Add(new NeuronGenome("enemyDirY",NeuronType.In, moduleID, 19));
        }
        if (useStats) {
            neuronList.Add(new NeuronGenome("enemyRelSize", NeuronType.In, moduleID, 200));
            neuronList.Add(new NeuronGenome("enemyHealth", NeuronType.In, moduleID, 201));
            neuronList.Add(new NeuronGenome("enemyGrowthStage", NeuronType.In, moduleID, 202));
            neuronList.Add(new NeuronGenome("enemyThreatRating", NeuronType.In, moduleID, 203));
        }
    }
	
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
