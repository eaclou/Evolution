using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleEnvironmentSensorsGenome {

	public int parentID;
    public int inno;

    public bool useCardinals;
    public bool useDiagonals;

    public float maxRange;

    public CritterModuleEnvironmentSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:
        useCardinals = false;
        useDiagonals = false;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useCardinals) {
            NeuronGenome distUp = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 32); // 32 // start up and go clockwise!        
            NeuronGenome distRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 34);  // 34        
            NeuronGenome distDown = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 36);  // 36        
            NeuronGenome distLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 38);  // 38       
        
            neuronList.Add(distUp); // 32 // start up and go clockwise!        
            neuronList.Add(distRight); // 34        
            neuronList.Add(distDown); // 36        
            neuronList.Add(distLeft); // 38
        }   
        if(useDiagonals) {
            NeuronGenome distTopRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 33); // 33
            NeuronGenome distBottomRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 35); // 35
            NeuronGenome distBottomLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 37);  // 37
            NeuronGenome distTopLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 39);  // 39        
        
            neuronList.Add(distTopRight); // 33
            neuronList.Add(distBottomRight); // 35
            neuronList.Add(distBottomLeft); // 37
            neuronList.Add(distTopLeft); // 39
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleEnvironmentSensorsGenome parentGenome, MutationSettings settings) {
        this.useCardinals = parentGenome.useCardinals;
        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useCardinals = !this.useCardinals;
        }

        this.useDiagonals = parentGenome.useDiagonals;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useDiagonals = !this.useDiagonals;
        }
    }
}
