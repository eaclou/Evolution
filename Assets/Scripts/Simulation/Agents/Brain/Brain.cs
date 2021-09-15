using System.Collections.Generic;
using UnityEngine;

public class Brain {
    public List<Neuron> neuronList;
    public Dictionary<NID, int> IDs;
    public List<Axon> axonList;

    public Brain(BrainGenome genome, Agent agent) {
        //Debug.Log("Brain() genome: " + genome.bodyNeuronList.Count.ToString());
        RebuildBrain(genome, agent);
    }

    public void RebuildBrain(BrainGenome genome, Agent agent) 
    {
        // Create Neurons:
        neuronList = new List<Neuron>();
        IDs = new Dictionary<NID, int>();
        
        RebuildNeurons(genome.bodyNeuronList, agent);
        RebuildNeurons(genome.hiddenNeuronList, agent);
        //Debug.Log("RebuildBrain " + genome.bodyNeuronList.Count + ", " + neuronList.Count.ToString());

        // Create Axons:
        axonList = new List<Axon>();
        foreach (var link in genome.linkList) 
        {
            // find out neuronIDs:
            if (!IDs.TryGetValue(new NID(link.fromModuleID, link.fromNeuronID), out int fromID)) {
                Debug.LogError("fromNID NOT FOUND " + link.fromModuleID + ", " + link.fromNeuronID);
            }
            if (!IDs.TryGetValue(new NID(link.toModuleID, link.toNeuronID), out int toID)) {
                Debug.LogError("toNID NOT FOUND " + link.fromModuleID + ", " + link.fromNeuronID);
            }

            //Debug.Log(fromID.ToString() + " --> " + toID.ToString() + ", " + genome.linkList[i].weight.ToString());
            //int fromID = IDs < new NID(genome.linkList[i].fromModuleID, genome.linkList[i].fromNeuronID) >;
            Axon axon = new Axon(fromID, toID, link.weight);
            axonList.Add(axon);
        }

        //PrintBrain();
    }
    
    void RebuildNeurons(List<NeuronGenome> neurons, Agent agent)
    {
        for (int i = 0; i < neurons.Count; i++) {
            Neuron neuron = new Neuron();
            neuron.name = neurons[i].name;
            agent.MapNeuronToModule(neurons[i].nid, neuron);
            IDs.Add(neurons[i].nid, i);
            neuronList.Add(neuron);
        }
    }

    public void ResetBrainState() {
        foreach (var neuron in neuronList) {
            neuron.currentValue[0] = 0f;
            neuron.previousValue = 0f;
        }
    }

    /// Ticks state of brain forward 1 timestep
    public void BrainMasterFunction() 
    {
        // NAIVE APPROACH:
        // run through all links and save sum values in target neurons
        foreach (var axon in axonList) {
            // Find input neuron, multiply its value by the axon weight, and add that to output neuron total:
            neuronList[axon.toID].inputTotal += axon.weight * neuronList[axon.fromID].currentValue[0];
        }
        
        // Once all axons are calculated, process the neurons:
        foreach (var neuron in neuronList) {
            neuron.previousValue = neuron.currentValue[0]; // Save previous state
            if (neuron.neuronType != NeuronType.In) {
                neuron.currentValue[0] = TransferFunctions.Evaluate(TransferFunctions.TransferFunction.RationalSigmoid, neuron.inputTotal);
            }
            // now zero out inputSum:
            neuron.inputTotal = 0f;
        }
    }

    public void PrintBrain() {
        Debug.Log("neuronCount: " + neuronList.Count);
        string neuronText = "";
        for (int i = 0; i < neuronList.Count; i++) {
            neuronText += "Neuron " + i + ": " + neuronList[i].currentValue.Length + "\n";
        }
        string axonText = "";
        for (int j = 0; j < axonList.Count; j++) {
            axonText += "Axon " + j + ": (" + axonList[j].fromID + "," + axonList[j].toID + ") " + axonList[j].weight + "\n";
        }
        Debug.Log(neuronText + "\n" + axonText);
    }
}
