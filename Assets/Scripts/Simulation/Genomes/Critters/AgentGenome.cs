using System;
using UnityEngine;

[Serializable]
public class AgentGenome 
{
    public int generationCount = 0;
    public BodyGenome bodyGenome = new BodyGenome();
    public BrainGenome brainGenome = new BrainGenome();

    public void GenerateInitialRandomBodyGenome() {
        generationCount = 0;
        bodyGenome.FirstTimeInitializeCritterModuleGenomes();
        bodyGenome.GenerateInitialRandomBodyGenome();        
    }

    public void InitializeRandomBrainFromCurrentBody(float initialWeightsMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) {
        brainGenome.InitializeRandomBrainGenome(bodyGenome, initialWeightsMultiplier, initialConnectionDensity, numInitHiddenNeurons);
    }

    public void PrintBrainGenome() {
        string neuronText = "";
        neuronText += (brainGenome.bodyNeuronList.Count + brainGenome.hiddenNeuronList.Count) + " Neurons (" + brainGenome.hiddenNeuronList.Count + ")\n";
        foreach (var neuronGenome in brainGenome.bodyNeuronList) {
            neuronText += neuronGenome.neuronType + " (" + neuronGenome.nid.moduleID + "," + neuronGenome.nid.neuronID + ")\n";
        }
        foreach (var neuronGenome in brainGenome.hiddenNeuronList) {
            neuronText += neuronGenome.neuronType + " (" + neuronGenome.nid.moduleID + "," + neuronGenome.nid.neuronID + ")\n";
        }
        Debug.Log(neuronText);
        
        string linkText = brainGenome.linkList.Count + "Axons\n";
        foreach (var linkGenome in brainGenome.linkList) {
            linkText += "(" + linkGenome.fromModuleID + "," + linkGenome.fromNeuronID + ") ==> (" +
                        linkGenome.toModuleID + "," + linkGenome.toNeuronID + ") " + linkGenome.weight + "\n";
        }
        Debug.Log(linkText);
    }
}
