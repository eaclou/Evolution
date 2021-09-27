using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Playcraft;

[Serializable]
public class CritterModuleFriendSensorsGenome
{
    public int parentID;
    public int inno;

    public bool usePos;
    public bool useVel;
    public bool useDir;

    public float sensorRange;

    public CritterModuleFriendSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(usePos) {
            NeuronGenome friendPosX = new NeuronGenome("friendPosX", NeuronType.In, inno, 8);
            NeuronGenome friendPosY = new NeuronGenome("friendPosY", NeuronType.In, inno, 9);
            neuronList.Add(friendPosX); // 8
            neuronList.Add(friendPosY); // 9
        }
        if(useVel) {
            NeuronGenome friendVelX = new NeuronGenome("friendVelX", NeuronType.In, inno, 10);
            NeuronGenome friendVelY = new NeuronGenome("friendVelY", NeuronType.In, inno, 11);
            neuronList.Add(friendVelX); // 10
            neuronList.Add(friendVelY); // 11
        }
        if(useDir) {
            NeuronGenome friendDirX = new NeuronGenome("friendDirX", NeuronType.In, inno, 12);
            NeuronGenome friendDirY = new NeuronGenome("friendDirY", NeuronType.In, inno, 13);
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
