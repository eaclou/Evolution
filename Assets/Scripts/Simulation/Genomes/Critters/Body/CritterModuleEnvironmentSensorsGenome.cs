using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleEnvironmentSensorsGenome {

	public int parentID;
    public int inno;

    public CritterModuleEnvironmentSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:

    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
                
        NeuronGenome distUp = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 32); // 32 // start up and go clockwise!
        NeuronGenome distTopRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 33); // 33
        NeuronGenome distRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 34);  // 34
        NeuronGenome distBottomRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 35); // 35
        NeuronGenome distDown = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 36);  // 36
        NeuronGenome distBottomLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 37);  // 37
        NeuronGenome distLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 38);  // 38
        NeuronGenome distTopLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 39);  // 39


        neuronList.Add(distUp); // 32 // start up and go clockwise!
        neuronList.Add(distTopRight); // 33
        neuronList.Add(distRight); // 34
        neuronList.Add(distBottomRight); // 35
        neuronList.Add(distDown); // 36
        neuronList.Add(distBottomLeft); // 37
        neuronList.Add(distLeft); // 38
        neuronList.Add(distTopLeft); // 39
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleEnvironmentSensorsGenome parentGenome, MutationSettings settings) {

    }
}
