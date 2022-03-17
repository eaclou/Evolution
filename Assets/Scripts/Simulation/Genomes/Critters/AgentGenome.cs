using System;
using UnityEngine;

[Serializable]
public class AgentGenome 
{
    [ReadOnly] public int generationCount = 0;
    [ReadOnly] public BodyGenome bodyGenome;
    [ReadOnly] public BrainGenome brainGenome;
    public string name;
    Lookup lookup => Lookup.instance;
    NameList nameList => lookup.nameList;
    
    //public AgentGenome() { Debug.LogError("Default AgentGenomeConstructor"); ConstructRandom(0.1f, 0); }

    public void IncrementGenerationCount() {
        generationCount++;
    }
    public AgentGenome(float initialConnectionDensity, int hiddenNeurons)
    {        
        ConstructRandom(initialConnectionDensity, hiddenNeurons);
        name = nameList.GetRandomName();
        Debug.Log("Constructing random AgentGenome...");
    }
   
    void ConstructRandom(float initialConnectionDensity, int hiddenNeuronCount)
    {
        bodyGenome = new BodyGenome();
        brainGenome = new BrainGenome(bodyGenome, initialConnectionDensity, hiddenNeuronCount);
    }

    public AgentGenome(BodyGenome bodyGenome, BrainGenome brainGenome, int generationCount, string name)
    {
        this.bodyGenome = bodyGenome; 
        this.brainGenome = brainGenome;
        this.generationCount = generationCount;
        this.name = name;
        Debug.Log("Constructing AgentGenome via mutation");
    }
    
    public void GenerateInitialRandomBodyGenome() {
        generationCount = 0;
        bodyGenome.FirstTimeInitializeCritterModuleGenomes();
        bodyGenome.GenerateInitialRandomBodyGenome();        
    }

    public void PrintBrainGenome() {
        string neuronText = "";
        neuronText += (brainGenome.inOutNeurons.Count + brainGenome.hiddenNeurons.Count) + " Neurons (" + brainGenome.hiddenNeurons.Count + ")\n";
        foreach (var neuronGenome in brainGenome.inOutNeurons) {
            neuronText += neuronGenome.io + " (" + neuronGenome.moduleID + "," + neuronGenome.index + ")\n";
        }
        foreach (var neuronGenome in brainGenome.hiddenNeurons) {
            neuronText += neuronGenome.io + " (" + neuronGenome.moduleID + "," + neuronGenome.index + ")\n";
        }
        Debug.Log(neuronText);
        
        string linkText = brainGenome.linkCount + "Axons\n";
        foreach (var linkGenome in brainGenome.links) {
            linkText += "(" + linkGenome.from.moduleID + "," + linkGenome.from.index + ") ==> (" +
                        linkGenome.to.moduleID + "," + linkGenome.to.index + ") " + linkGenome.weight + "\n";
        }
        Debug.Log(linkText);
    }
}
