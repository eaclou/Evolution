using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleFriendSensorsGenome
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;

    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.FriendSensors;

    public bool usePos;
    public bool useVel;
    public bool useDir;

    public float sensorRange;
    

    public void InitializeRandom() {
        usePos = RandomStatics.CoinToss();
        useVel = RandomStatics.CoinToss();
        useDir = RandomStatics.CoinToss();
    }
    
    public void Initialize(UnlockedTech unlockedTech) {
        // usePos = unlockedTech.Contains(TechElementId.???);
        // useVel = unlockedTech.Contains(TechElementId.???);
        // useDir = unlockedTech.Contains(TechElementId.???);
    }
    
    List<NeuronGenome> masterList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
        
        if (usePos) 
        {
            AddNeuron("friendPosX");
            AddNeuron("friendPosY");
        }
        if (useVel) 
        {
            AddNeuron("friendVelX");
            AddNeuron("friendVelY");
        }
        if (useDir) 
        {
            AddNeuron("friendDirX");
            AddNeuron("friendDirY");
        }
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }

    /*List<NeuronGenome> neuronList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> neuronList) {
        this.neuronList = neuronList;

        if (usePos) {
            AddNeuron("friendPosX");
            AddNeuron("friendPosY");
            //neuronList.Add(new NeuronGenome("friendPosX", NeuronType.In, moduleID, 8));
            //neuronList.Add(new NeuronGenome("friendPosY", NeuronType.In, moduleID, 9));
        }
        if (useVel) {
            AddNeuron("friendVelX");
            AddNeuron("friendVelY");
            //neuronList.Add(new NeuronGenome("friendVelX", NeuronType.In, moduleID, 10));
            //neuronList.Add(new NeuronGenome("friendVelY", NeuronType.In, moduleID, 11));
        }
        if (useDir) {
            AddNeuron("friendDirX");
            AddNeuron("friendDirY");
            //neuronList.Add(new NeuronGenome("friendDirX", NeuronType.In, moduleID, 12));
            //neuronList.Add(new NeuronGenome("friendDirY", NeuronType.In, moduleID, 13));
        }
    }
    
    void AddNeuron(string name) { neuronList.Add(map.GetGenome(name)); }*/

    public void SetToMutatedCopyOfParentGenome(CritterModuleFriendSensorsGenome parentGenome, MutationSettingsInstance settings) {
        usePos = RequestMutation(settings, parentGenome.usePos);
        useVel = RequestMutation(settings, parentGenome.useVel);
        useDir = RequestMutation(settings, parentGenome.useDir);
    }
    
    bool RequestMutation(MutationSettingsInstance settings, bool defaultValue) {
        return RandomStatics.RandomFlip(settings.bodyModuleInternalMutationChance, defaultValue);
    }
}
