using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleThreatSensorsGenome {

	public int parentID;
    public int inno;

    public CritterModuleThreatSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:

    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

        NeuronGenome enemyPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 14);
        NeuronGenome enemyPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 15);
        NeuronGenome enemyVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 16);
        NeuronGenome enemyVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 17);
        NeuronGenome enemyDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 18);
        NeuronGenome enemyDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 19);

        NeuronGenome enemyRelSize = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 200);
        NeuronGenome enemyHealth = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 201);
        NeuronGenome enemyGrowthStage = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 202);
        NeuronGenome enemyThreatRating = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 203);

        neuronList.Add(enemyPosX); // 14
        neuronList.Add(enemyPosY); // 15
        neuronList.Add(enemyVelX); // 16
        neuronList.Add(enemyVelY); // 17
        neuronList.Add(enemyDirX); // 18
        neuronList.Add(enemyDirY); // 19

        neuronList.Add(enemyRelSize); // 200
        neuronList.Add(enemyHealth); // 201
        neuronList.Add(enemyGrowthStage); // 202
        neuronList.Add(enemyThreatRating); // 203
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleThreatSensorsGenome parentGenome, MutationSettings settings) {

    }
}
