using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

    public void ProcessNewSpeciesExtraMutation() {
        float magnitude = 0.75f;
        Vector3 primaryHueShift = UnityEngine.Random.onUnitSphere;
        Vector3 secondaryHueShift = UnityEngine.Random.onUnitSphere;
        bodyGenome.appearanceGenome.huePrimary = Vector3.Lerp(bodyGenome.appearanceGenome.huePrimary, primaryHueShift, magnitude);
        bodyGenome.appearanceGenome.hueSecondary = Vector3.Lerp(bodyGenome.appearanceGenome.hueSecondary, secondaryHueShift, magnitude);

        //bodyGenome.coreGenome.
    }

    public void InitializeRandomBrainFromCurrentBody(float initialWeightsMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) {
        brainGenome.InitializeRandomBrainGenome(bodyGenome, initialWeightsMultiplier, initialConnectionDensity, numInitHiddenNeurons);
    }

    public void PrintBrainGenome() {
        string neuronText = "";
        neuronText += (brainGenome.bodyNeuronList.Count + brainGenome.hiddenNeuronList.Count).ToString() + " Neurons (" + brainGenome.hiddenNeuronList.Count.ToString() + ")\n";
        for (int i = 0; i < brainGenome.bodyNeuronList.Count; i++) {
            neuronText += brainGenome.bodyNeuronList[i].neuronType.ToString() + " (" + brainGenome.bodyNeuronList[i].nid.moduleID.ToString() + "," + brainGenome.bodyNeuronList[i].nid.neuronID.ToString() + ")\n";
        }
        for (int i = 0; i < brainGenome.hiddenNeuronList.Count; i++) {
            neuronText += brainGenome.hiddenNeuronList[i].neuronType.ToString() + " (" + brainGenome.hiddenNeuronList[i].nid.moduleID.ToString() + "," + brainGenome.hiddenNeuronList[i].nid.neuronID.ToString() + ")\n";
        }
        Debug.Log(neuronText);
        string linkText = brainGenome.linkList.Count.ToString() + "Axons\n";
        linkText += "";
        for (int i = 0; i < brainGenome.linkList.Count; i++) {
            linkText += "(" + brainGenome.linkList[i].fromModuleID.ToString() + "," + brainGenome.linkList[i].fromNeuronID.ToString() + ") ==> (" +
                        brainGenome.linkList[i].toModuleID.ToString() + "," + brainGenome.linkList[i].toNeuronID.ToString() + ") " + brainGenome.linkList[i].weight.ToString() + "\n";
        }
        Debug.Log(linkText);
    }
}
