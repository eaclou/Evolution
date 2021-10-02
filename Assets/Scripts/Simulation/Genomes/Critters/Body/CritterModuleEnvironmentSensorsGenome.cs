using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Playcraft;

[Serializable]
public class CritterModuleEnvironmentSensorsGenome 
{
    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.EnvironmentSensors;

    public bool useWaterStats;
    public bool useCardinals;
    public bool useDiagonals;    

    public float maxRange;

    public CritterModuleEnvironmentSensorsGenome(int parentID) {
        this.parentID = parentID;
    }

    public void GenerateRandomInitialGenome() {
        useCardinals = RandomStatics.CoinToss();
        useDiagonals = RandomStatics.CoinToss();
        useWaterStats = RandomStatics.CoinToss();
        maxRange = 20f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if (useWaterStats) {
            neuronList.Add(new NeuronGenome("depth", NeuronType.In, moduleID, 1)); 
            neuronList.Add(new NeuronGenome("velX", NeuronType.In, moduleID, 2)); 
            neuronList.Add(new NeuronGenome("velY", NeuronType.In, moduleID, 3));
            neuronList.Add(new NeuronGenome("depthGradX", NeuronType.In, moduleID, 4)); 
            neuronList.Add(new NeuronGenome("depthGradY", NeuronType.In, moduleID, 5)); 

            /*
            NeuronGenome depthSouth = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);  // 36        
            NeuronGenome depthWest = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);  // 38       
        
            neuronList.Add(depthNorth);   
            neuronList.Add(depthEast);     
            neuronList.Add(depthSouth);       
            neuronList.Add(depthWest); */
        }
        if (useCardinals) {
            /*NeuronGenome distUp = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4); // 32 // start up and go clockwise!        
            NeuronGenome distRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);  // 34        
            NeuronGenome distDown = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);  // 36        
            NeuronGenome distLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);  // 38       
        
            neuronList.Add(distUp);   
            neuronList.Add(distRight);     
            neuronList.Add(distDown);       
            neuronList.Add(distLeft); */
        }   
        /*if(useDiagonals) {
            NeuronGenome distTopRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8); // 33
            NeuronGenome distBottomRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9); // 35
            NeuronGenome distBottomLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);  // 37
            NeuronGenome distTopLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);  // 39        
        
            neuronList.Add(distTopRight); 
            neuronList.Add(distBottomRight); 
            neuronList.Add(distBottomLeft);
            neuronList.Add(distTopLeft);
        } */       
    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleEnvironmentSensorsGenome parentGenome, MutationSettingsInstance settings) {
        useWaterStats = RequestMutation(settings, parentGenome.useWaterStats);
        useCardinals = RequestMutation(settings, parentGenome.useCardinals);
        useDiagonals = RequestMutation(settings, parentGenome.useDiagonals);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
