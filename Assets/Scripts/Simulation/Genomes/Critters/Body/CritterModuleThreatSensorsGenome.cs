using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleThreatSensorsGenome 
{
    public int parentID;
    public int inno;    // * WPP: what is this?  (NEVER use abbreviations!)

    public bool usePos;
    public bool useVel;
    public bool useDir;
    public bool useStats;

    public CritterModuleThreatSensorsGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
        useStats = RandomStatics.CoinToss();
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(usePos) {
            NeuronGenome enemyPosX = new NeuronGenome("enemyPosX", NeuronType.In, inno, 14);
            NeuronGenome enemyPosY = new NeuronGenome("enemyPosY", NeuronType.In, inno, 15);
            neuronList.Add(enemyPosX); // 14
            neuronList.Add(enemyPosY); // 15
        }
        if(useVel) {
            NeuronGenome enemyVelX = new NeuronGenome("enemyVelX", NeuronType.In, inno, 16);
            NeuronGenome enemyVelY = new NeuronGenome("enemyVelY", NeuronType.In, inno, 17);
            neuronList.Add(enemyVelX); // 16
            neuronList.Add(enemyVelY); // 17
        }
        if(useDir) {
            NeuronGenome enemyDirX = new NeuronGenome("enemyDirX", NeuronType.In, inno, 18);
            NeuronGenome enemyDirY = new NeuronGenome("enemyDirY",NeuronType.In, inno, 19);
            neuronList.Add(enemyDirX); // 18
            neuronList.Add(enemyDirY); // 19
        }
        if(useStats) {
            NeuronGenome enemyRelSize = new NeuronGenome("enemyRelSize", NeuronType.In, inno, 200);
            NeuronGenome enemyHealth = new NeuronGenome("enemyHealth", NeuronType.In, inno, 201);
            NeuronGenome enemyGrowthStage = new NeuronGenome("enemyGrowthStage", NeuronType.In, inno, 202);
            NeuronGenome enemyThreatRating = new NeuronGenome("enemyThreatRating", NeuronType.In, inno, 203);
            neuronList.Add(enemyRelSize); // 200
            neuronList.Add(enemyHealth); // 201
            neuronList.Add(enemyGrowthStage); // 202
            neuronList.Add(enemyThreatRating); // 203
        }
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleThreatSensorsGenome parentGenome, MutationSettings settings) {
        usePos = RequestMutation(settings, parentGenome.usePos);
        useVel = RequestMutation(settings, parentGenome.useVel);
        useDir = RequestMutation(settings, parentGenome.useDir);
        useStats = RequestMutation(settings, parentGenome.useStats);
    }
    
    bool RequestMutation(MutationSettings settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
