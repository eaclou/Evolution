using System;
using UnityEngine;

[Serializable]
public class AgentGenome 
{
    public int generationCount = 0;
    public BodyGenome bodyGenome;
    public BrainGenome brainGenome;
    
    public AgentGenome(float initialConnectionDensity, int hiddenNeurons)
    {
        ConstructRandom(initialConnectionDensity, hiddenNeurons);
    }
    
    public AgentGenome(MutationSettingsInstance mutationSettings, float lerpV) 
    {
        int numberOfHiddenNeurons = Mathf.RoundToInt(4f * lerpV);  //***EAC CHANGE!
        float brainInitialConnectionDensity = mutationSettings.brainInitialConnectionChance * lerpV;
        ConstructRandom(brainInitialConnectionDensity, numberOfHiddenNeurons);
    }
    
    void ConstructRandom(float initialConnectionDensity, int hiddenNeuronCount)
    {
        bodyGenome = new BodyGenome();
        brainGenome = new BrainGenome(bodyGenome, initialConnectionDensity, hiddenNeuronCount);
    }

    public AgentGenome(BodyGenome bodyGenome, BrainGenome brainGenome, int generationCount)
    {
        this.bodyGenome = bodyGenome; 
        this.brainGenome = brainGenome;
        this.generationCount = generationCount;
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
