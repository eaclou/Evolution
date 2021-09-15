using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Playcraft;

[Serializable]
public class CritterModuleEnvironmentSensorsGenome 
{
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
        useCardinals = RandomStatics.CoinToss();
        useDiagonals = RandomStatics.CoinToss();
        useWaterStats = RandomStatics.CoinToss();
        maxRange = 20f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if (useWaterStats) {
            NeuronGenome depth = new NeuronGenome("depth", NeuronType.In, inno, 1); 
            NeuronGenome velX = new NeuronGenome("velX", NeuronType.In, inno, 2); 
            NeuronGenome velY = new NeuronGenome("velY", NeuronType.In, inno, 3); 
            neuronList.Add(depth);      
            neuronList.Add(velX);      
            neuronList.Add(velY);   
            
            NeuronGenome depthGradX = new NeuronGenome("depthGradX", NeuronType.In, inno, 4); 
            NeuronGenome depthGradY = new NeuronGenome("depthGradY", NeuronType.In, inno, 5); 
            neuronList.Add(depthGradX);   
            neuronList.Add(depthGradY); 
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
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleEnvironmentSensorsGenome parentGenome, MutationSettings settings) {
        useWaterStats = RequestMutation(settings, parentGenome.useWaterStats);
        useCardinals = RequestMutation(settings, parentGenome.useCardinals);
        useDiagonals = RequestMutation(settings, parentGenome.useDiagonals);
    }
    
    bool RequestMutation(MutationSettings settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
