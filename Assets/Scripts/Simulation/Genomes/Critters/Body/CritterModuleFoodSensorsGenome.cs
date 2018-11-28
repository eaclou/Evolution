using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleFoodSensorsGenome {

	public int parentID;
    public int inno;

    public bool usePos;
    public bool useDir;
    public bool useStats;

    public CritterModuleFoodSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:
        usePos = true;
        useDir = true;
        useStats = false;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(usePos) {
            NeuronGenome foodPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1);
            NeuronGenome foodPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2);
            neuronList.Add(foodPosX);
            neuronList.Add(foodPosY);
        }
        if(useDir) {
            NeuronGenome foodDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);
            NeuronGenome foodDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4);
            neuronList.Add(foodDirX);
            neuronList.Add(foodDirY);
        }
        if(useStats) {
            NeuronGenome foodRelSize = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
            neuronList.Add(foodRelSize);
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFoodSensorsGenome parentGenome, MutationSettings settings) {
        this.usePos = parentGenome.usePos;
        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.usePos = !this.usePos;
        }

        this.useDir = parentGenome.useDir;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useDir = !this.useDir;
        }

        this.useStats = parentGenome.useStats;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useStats = !this.useStats;
        }
    }
}
