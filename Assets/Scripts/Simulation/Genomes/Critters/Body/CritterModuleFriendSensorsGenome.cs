using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleFriendSensorsGenome {

	public int parentID;
    public int inno;

    public bool useFriendPos;
    public bool useFriendVel;
    public bool useFriendDir;

    public float sensorRange;

    public CritterModuleFriendSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:

        useFriendPos = false;
        useFriendVel = false;
        useFriendDir = false;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useFriendPos) {
            NeuronGenome friendPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8);
            NeuronGenome friendPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9);
            neuronList.Add(friendPosX); // 8
            neuronList.Add(friendPosY); // 9
        }
        if(useFriendVel) {
            NeuronGenome friendVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);
            NeuronGenome friendVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);
            neuronList.Add(friendVelX); // 10
            neuronList.Add(friendVelY); // 11
        }
        if(useFriendDir) {
            NeuronGenome friendDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 12);
            NeuronGenome friendDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 13);
            neuronList.Add(friendDirX); // 12
            neuronList.Add(friendDirY); // 13
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFriendSensorsGenome parentGenome, MutationSettings settings) {
        this.useFriendPos = parentGenome.useFriendPos;
        this.useFriendVel = parentGenome.useFriendVel;
        this.useFriendDir = parentGenome.useFriendDir;
    }
}
