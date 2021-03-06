﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleThreatSensorsGenome {

	public int parentID;
    public int inno;

    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;

    public CritterModuleThreatSensorsGenome(int parentID, int inno) {
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
        
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(usePos) {
            NeuronGenome enemyPosX = new NeuronGenome("enemyPosX", NeuronGenome.NeuronType.In, inno, 14);
            NeuronGenome enemyPosY = new NeuronGenome("enemyPosY", NeuronGenome.NeuronType.In, inno, 15);
            neuronList.Add(enemyPosX); // 14
            neuronList.Add(enemyPosY); // 15
        }
        if(useVel) {
            NeuronGenome enemyVelX = new NeuronGenome("enemyVelX", NeuronGenome.NeuronType.In, inno, 16);
            NeuronGenome enemyVelY = new NeuronGenome("enemyVelY", NeuronGenome.NeuronType.In, inno, 17);
            neuronList.Add(enemyVelX); // 16
            neuronList.Add(enemyVelY); // 17
        }
        if(useDir) {
            NeuronGenome enemyDirX = new NeuronGenome("enemyDirX", NeuronGenome.NeuronType.In, inno, 18);
            NeuronGenome enemyDirY = new NeuronGenome("enemyDirY", NeuronGenome.NeuronType.In, inno, 19);
            neuronList.Add(enemyDirX); // 18
            neuronList.Add(enemyDirY); // 19
        }
        if(useStats) {
            NeuronGenome enemyRelSize = new NeuronGenome("enemyRelSize", NeuronGenome.NeuronType.In, inno, 200);
            NeuronGenome enemyHealth = new NeuronGenome("enemyHealth", NeuronGenome.NeuronType.In, inno, 201);
            NeuronGenome enemyGrowthStage = new NeuronGenome("enemyGrowthStage", NeuronGenome.NeuronType.In, inno, 202);
            NeuronGenome enemyThreatRating = new NeuronGenome("enemyThreatRating", NeuronGenome.NeuronType.In, inno, 203);
            neuronList.Add(enemyRelSize); // 200
            neuronList.Add(enemyHealth); // 201
            neuronList.Add(enemyGrowthStage); // 202
            neuronList.Add(enemyThreatRating); // 203
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleThreatSensorsGenome parentGenome, MutationSettings settings) {
        this.usePos = parentGenome.usePos;
        float randChance = UnityEngine.Random.Range(0f, 1f);
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
    }
}
