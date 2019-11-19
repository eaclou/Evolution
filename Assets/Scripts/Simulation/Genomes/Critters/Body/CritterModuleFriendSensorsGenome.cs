using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleFriendSensorsGenome {

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
        // Do stuff:

        usePos = true;
        useVel = false;
        useDir = false;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(usePos) {
            NeuronGenome friendPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8);
            NeuronGenome friendPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9);
            neuronList.Add(friendPosX); // 8
            neuronList.Add(friendPosY); // 9
        }
        if(useVel) {
            NeuronGenome friendVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);
            NeuronGenome friendVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);
            neuronList.Add(friendVelX); // 10
            neuronList.Add(friendVelY); // 11
        }
        if(useDir) {
            NeuronGenome friendDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 12);
            NeuronGenome friendDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 13);
            neuronList.Add(friendDirX); // 12
            neuronList.Add(friendDirY); // 13
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFriendSensorsGenome parentGenome, MutationSettings settings) {
        this.usePos = parentGenome.usePos;
        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            //this.usePos = !this.usePos;
        }

        this.useVel = parentGenome.useVel;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            //this.useVel = !this.useVel;
        }

        this.useDir = parentGenome.useDir;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            //this.useDir = !this.useDir;
        }


    }
}
