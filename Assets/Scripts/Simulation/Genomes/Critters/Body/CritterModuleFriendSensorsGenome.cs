using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleFriendSensorsGenome
{
    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.FriendSensors;

    public bool usePos;
    public bool useVel;
    public bool useDir;

    public float sensorRange;

    public CritterModuleFriendSensorsGenome(int parentID) {
        this.parentID = parentID;
    }

    public void GenerateRandomInitialGenome() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(usePos) {
            NeuronGenome friendPosX = new NeuronGenome("friendPosX", NeuronType.In, moduleID, 8);
            NeuronGenome friendPosY = new NeuronGenome("friendPosY", NeuronType.In, moduleID, 9);
            neuronList.Add(friendPosX); // 8
            neuronList.Add(friendPosY); // 9
        }
        if(useVel) {
            NeuronGenome friendVelX = new NeuronGenome("friendVelX", NeuronType.In, moduleID, 10);
            NeuronGenome friendVelY = new NeuronGenome("friendVelY", NeuronType.In, moduleID, 11);
            neuronList.Add(friendVelX); // 10
            neuronList.Add(friendVelY); // 11
        }
        if(useDir) {
            NeuronGenome friendDirX = new NeuronGenome("friendDirX", NeuronType.In, moduleID, 12);
            NeuronGenome friendDirY = new NeuronGenome("friendDirY", NeuronType.In, moduleID, 13);
            neuronList.Add(friendDirX); // 12
            neuronList.Add(friendDirY); // 13
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFriendSensorsGenome parentGenome, MutationSettingsInstance settings) {
        usePos = RequestMutation(settings, parentGenome.usePos);
        useVel = RequestMutation(settings, parentGenome.useVel);
        useDir = RequestMutation(settings, parentGenome.useDir);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
