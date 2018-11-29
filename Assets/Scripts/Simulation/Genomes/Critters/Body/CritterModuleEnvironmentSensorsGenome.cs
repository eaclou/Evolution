using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleEnvironmentSensorsGenome {

	public int parentID;
    public int inno;

    public bool useWaterStats;
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
        useWaterStats = false;

        maxRange = 20f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useWaterStats) {
            NeuronGenome depth = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1); // 33
            NeuronGenome velX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2); // 35
            NeuronGenome velY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);  // 37
            neuronList.Add(depth);      
            neuronList.Add(velX);      
            neuronList.Add(velY);   
        }
        if (useCardinals) {
            NeuronGenome distUp = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4); // 32 // start up and go clockwise!        
            NeuronGenome distRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);  // 34        
            NeuronGenome distDown = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);  // 36        
            NeuronGenome distLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);  // 38       
        
            neuronList.Add(distUp);   
            neuronList.Add(distRight);     
            neuronList.Add(distDown);       
            neuronList.Add(distLeft); 
        }   
        if(useDiagonals) {
            NeuronGenome distTopRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8); // 33
            NeuronGenome distBottomRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9); // 35
            NeuronGenome distBottomLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);  // 37
            NeuronGenome distTopLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);  // 39        
        
            neuronList.Add(distTopRight); 
            neuronList.Add(distBottomRight); 
            neuronList.Add(distBottomLeft);
            neuronList.Add(distTopLeft);
        }        
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleEnvironmentSensorsGenome parentGenome, MutationSettings settings) {
        this.useWaterStats = parentGenome.useWaterStats;
        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useWaterStats = !this.useWaterStats;
        }

        this.useCardinals = parentGenome.useCardinals;
        randChance = UnityEngine.Random.Range(0f, 1f);
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
