using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleFoodSensorsGenome {

	public int parentID;
    public int inno;

    public bool useNutrients;
    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;
    public bool useEggs;
    public bool useCorpse;

    // 0-1, determines what will be chosen by creature as its current food target
    //public float preferenceParticles;
    //public float preferenceEggs;
    //public float preferenceCreatures;
    public float preferredSize;

    public float sensorRangeMult;

    public CritterModuleFoodSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:
        useNutrients = true;
        usePos = false;
        useVel = false;
        useDir = true;
        useStats = false;
        useEggs = true;
        useCorpse = true;

        //preferenceParticles = 0.5f;
        //preferenceEggs = 0.5f;
        //preferenceCreatures = 0.5f;
        preferredSize = 0.5f;

        sensorRangeMult = 1f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useNutrients) {
            NeuronGenome nutrientDensity = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1);
            NeuronGenome nutrientGradX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2);
            NeuronGenome nutrientGradY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);
            neuronList.Add(nutrientDensity);
            neuronList.Add(nutrientGradX);
            neuronList.Add(nutrientGradY);
        }
        if(usePos) {
            NeuronGenome foodPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4);
            NeuronGenome foodPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
            NeuronGenome distance = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);
            neuronList.Add(foodPosX);
            neuronList.Add(foodPosY);
            neuronList.Add(distance);
        }
        if(useVel) {
            NeuronGenome foodVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);
            NeuronGenome foodVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8);
            neuronList.Add(foodVelX);
            neuronList.Add(foodVelY);
        }
        if(useDir) {
            NeuronGenome foodDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9);
            NeuronGenome foodDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);
            neuronList.Add(foodDirX);
            neuronList.Add(foodDirY);
        }
        if(useStats) {
            NeuronGenome foodQuality = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);
            NeuronGenome foodRelSize = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 12);
            neuronList.Add(foodQuality);
            neuronList.Add(foodRelSize);
        }
        if(useEggs) {
            NeuronGenome distance = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 13);
            NeuronGenome eggDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 14);
            NeuronGenome eggDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 15);
            neuronList.Add(distance);
            neuronList.Add(eggDirX);
            neuronList.Add(eggDirY);
        }
        if(useCorpse) {
            NeuronGenome distance = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 16);
            NeuronGenome corpseDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 17);
            NeuronGenome corpseDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 18);
            neuronList.Add(distance);
            neuronList.Add(corpseDirX);
            neuronList.Add(corpseDirY);
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFoodSensorsGenome parentGenome, MutationSettings settings) {
        this.useNutrients = parentGenome.useNutrients;
        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useNutrients = !this.useNutrients;
        }

        this.usePos = parentGenome.usePos;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.usePos = !this.usePos;
        }

        this.useVel = parentGenome.useVel;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useVel = !this.useVel;
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

        this.useEggs = parentGenome.useEggs;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useEggs = !this.useEggs;
        }

        this.useCorpse = parentGenome.useCorpse;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useCorpse = !this.useCorpse;
        }

        //preferenceParticles = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceParticles, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        //preferenceEggs = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceEggs, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        //preferenceCreatures = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceCreatures, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        preferredSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferredSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);

        sensorRangeMult = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.sensorRangeMult, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
    }
}
