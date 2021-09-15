using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
        float randChance = Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            usePos = false;
        }
        else {
            usePos = true;
        }

        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            useVel = false;
        }
        else {
            useVel = true;
        }

        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            useDir = false;
        }
        else {
            useDir = true;
        }
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
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFriendSensorsGenome parentGenome, MutationSettings settings) {
        this.usePos = parentGenome.usePos;
        float randChance = Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.usePos = !this.usePos;
        }

        this.useVel = parentGenome.useVel;
        randChance = Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useVel = !this.useVel;
        }

        this.useDir = parentGenome.useDir;
        randChance = Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useDir = !this.useDir;
        }
    }
}
