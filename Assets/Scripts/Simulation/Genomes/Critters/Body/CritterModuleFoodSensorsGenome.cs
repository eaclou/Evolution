using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleFoodSensorsGenome {

	public int parentID;
    public int inno;

    public CritterModuleFoodSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:

    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

        NeuronGenome foodPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1);
        NeuronGenome foodPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2);
        NeuronGenome foodDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);
        NeuronGenome foodDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4);
        NeuronGenome foodRelSize = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
        
        neuronList.Add(foodPosX);
        neuronList.Add(foodPosY);
        neuronList.Add(foodDirX);
        neuronList.Add(foodDirY);
        neuronList.Add(foodRelSize);
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFoodSensorsGenome parentGenome, MutationSettings settings) {

    }
}
