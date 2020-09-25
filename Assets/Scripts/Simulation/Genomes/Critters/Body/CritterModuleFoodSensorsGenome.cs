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
        float randChance = UnityEngine.Random.Range(0f, 1f);
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

        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            useStats = false;
        }
        else {
            useStats = true;
        }

        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            useNutrients = false;
        }
        else {
            useNutrients = true;
        }
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            useEggs = false;
        }
        else {
            useEggs = true;
        }
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < 0.5f) {
            useCorpse = false;
        }
        else {
            useCorpse = true;
        }
        
        preferredSize = 0.5f;

        sensorRangeMult = 1f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useNutrients) {
            NeuronGenome nutrientDensity = new NeuronGenome("nutrientDensity", NeuronGenome.NeuronType.In, inno, 1);
            NeuronGenome nutrientGradX = new NeuronGenome("nutrientGradX", NeuronGenome.NeuronType.In, inno, 2);
            NeuronGenome nutrientGradY = new NeuronGenome("nutrientGradY", NeuronGenome.NeuronType.In, inno, 3);
            neuronList.Add(nutrientDensity);
            neuronList.Add(nutrientGradX);
            neuronList.Add(nutrientGradY);
        }
        if(usePos) {
            NeuronGenome foodPosX = new NeuronGenome("foodPosX", NeuronGenome.NeuronType.In, inno, 4);
            NeuronGenome foodPosY = new NeuronGenome("foodPosY", NeuronGenome.NeuronType.In, inno, 5);
            NeuronGenome distance = new NeuronGenome("distance", NeuronGenome.NeuronType.In, inno, 6);
            neuronList.Add(foodPosX);
            neuronList.Add(foodPosY);
            neuronList.Add(distance);

            NeuronGenome animalPosX = new NeuronGenome("animalPosX", NeuronGenome.NeuronType.In, inno, 24);
            NeuronGenome animalPosY = new NeuronGenome("animalPosY", NeuronGenome.NeuronType.In, inno, 25);
            NeuronGenome animalDistance = new NeuronGenome("animalDistance", NeuronGenome.NeuronType.In, inno, 26);
            neuronList.Add(animalPosX);
            neuronList.Add(animalPosY);
            neuronList.Add(animalDistance);
        }
        if(useVel) {
            NeuronGenome foodVelX = new NeuronGenome("foodVelX", NeuronGenome.NeuronType.In, inno, 7);
            NeuronGenome foodVelY = new NeuronGenome("foodVelY", NeuronGenome.NeuronType.In, inno, 8);
            neuronList.Add(foodVelX);
            neuronList.Add(foodVelY);

            NeuronGenome animalVelX = new NeuronGenome("animalVelX", NeuronGenome.NeuronType.In, inno, 27);
            NeuronGenome animalVelY = new NeuronGenome("animalVelY", NeuronGenome.NeuronType.In, inno, 28);
            neuronList.Add(animalVelX);
            neuronList.Add(animalVelY);
        }
        if(useDir) {
            NeuronGenome foodDirX = new NeuronGenome("foodDirX", NeuronGenome.NeuronType.In, inno, 9);
            NeuronGenome foodDirY = new NeuronGenome("foodDirY", NeuronGenome.NeuronType.In, inno, 10);
            neuronList.Add(foodDirX);
            neuronList.Add(foodDirY);

            NeuronGenome animalDirX = new NeuronGenome("animalDirX", NeuronGenome.NeuronType.In, inno, 29);
            NeuronGenome animalDirY = new NeuronGenome("animalDirY", NeuronGenome.NeuronType.In, inno, 30);
            neuronList.Add(animalDirX);
            neuronList.Add(animalDirY);
        }
        if(useStats) {
            NeuronGenome foodQuality = new NeuronGenome("foodQuality", NeuronGenome.NeuronType.In, inno, 11);
            NeuronGenome foodRelSize = new NeuronGenome("foodRelSize", NeuronGenome.NeuronType.In, inno, 12);
            neuronList.Add(foodQuality);
            neuronList.Add(foodRelSize);

            NeuronGenome animalQuality = new NeuronGenome("animalQuality", NeuronGenome.NeuronType.In, inno, 31);
            NeuronGenome animalRelSize = new NeuronGenome("animalRelSize", NeuronGenome.NeuronType.In, inno, 32);
            neuronList.Add(animalQuality);
            neuronList.Add(animalRelSize);
        }
        if(useEggs) {
            NeuronGenome distance = new NeuronGenome("distance", NeuronGenome.NeuronType.In, inno, 13);
            NeuronGenome eggDirX = new NeuronGenome("eggDirX", NeuronGenome.NeuronType.In, inno, 14);
            NeuronGenome eggDirY = new NeuronGenome("eggDirY", NeuronGenome.NeuronType.In, inno, 15);
            neuronList.Add(distance);
            neuronList.Add(eggDirX);
            neuronList.Add(eggDirY);
        }
        if(useCorpse) {
            NeuronGenome distance = new NeuronGenome("distance", NeuronGenome.NeuronType.In, inno, 16);
            NeuronGenome corpseDirX = new NeuronGenome("corpseDirX", NeuronGenome.NeuronType.In, inno, 17);
            NeuronGenome corpseDirY = new NeuronGenome("corpseDirY", NeuronGenome.NeuronType.In, inno, 18);
            neuronList.Add(distance);
            neuronList.Add(corpseDirX);
            neuronList.Add(corpseDirY);
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleFoodSensorsGenome parentGenome, MutationSettings settings) {
        this.useNutrients = parentGenome.useNutrients;
        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useNutrients = !this.useNutrients;
        }

        this.usePos = parentGenome.usePos;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.usePos = !this.usePos;
        }

        this.useVel = parentGenome.useVel;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useVel = !this.useVel;
        }

        this.useDir = parentGenome.useDir;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useDir = !this.useDir;
        }

        this.useStats = parentGenome.useStats;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useStats = !this.useStats;
        }

        this.useEggs = parentGenome.useEggs;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useEggs = !this.useEggs;
        }

        this.useCorpse = parentGenome.useCorpse;
        randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleInternalMutationChance) {
            this.useCorpse = !this.useCorpse;
        }

        //preferenceParticles = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceParticles, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        //preferenceEggs = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceEggs, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        //preferenceCreatures = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferenceCreatures, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        preferredSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.preferredSize, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 0f, 1f);

        sensorRangeMult = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.sensorRangeMult, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 0f, 1f);
    }
}
