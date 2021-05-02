using System;
using UnityEngine;

[Serializable]
public class AgentGenome {

    //public int index = -1;  // *** THIS IS NOW MEANINGLESS??
    
    public int generationCount = 0;
    public BodyGenome bodyGenome;
    public BrainGenome brainGenome;

    // Constructor
    public AgentGenome() {
        //this.index = index;
        bodyGenome = new BodyGenome();  // empty constructors:
        brainGenome = new BrainGenome();
    }

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
        for (int i = 0; i < brainGenome.bodyNeuronList.Count; i++) {
            neuronText += brainGenome.bodyNeuronList[i].neuronType + " (" + brainGenome.bodyNeuronList[i].nid.moduleID + "," + brainGenome.bodyNeuronList[i].nid.neuronID + ")\n";
        }
        for (int i = 0; i < brainGenome.hiddenNeuronList.Count; i++) {
            neuronText += brainGenome.hiddenNeuronList[i].neuronType + " (" + brainGenome.hiddenNeuronList[i].nid.moduleID + "," + brainGenome.hiddenNeuronList[i].nid.neuronID + ")\n";
        }
        Debug.Log(neuronText);
        string linkText = brainGenome.linkList.Count + "Axons\n";
        linkText += "";
        for (int i = 0; i < brainGenome.linkList.Count; i++) {
            linkText += "(" + brainGenome.linkList[i].fromModuleID + "," + brainGenome.linkList[i].fromNeuronID + ") ==> (" +
                        brainGenome.linkList[i].toModuleID + "," + brainGenome.linkList[i].toNeuronID + ") " + brainGenome.linkList[i].weight + "\n";
        }
        Debug.Log(linkText);
    }
}
