using System.Collections.Generic;
using UnityEngine;

public class Brain 
{
    public List<Neuron> neurons;
    //public Dictionary<NID, int> IDs;
    public List<Axon> axons;

    public Brain(BrainGenome genome, Agent agent) {
       // Debug.Log("Brain constructed with " + genome.inOutNeurons.Count + " neurons.");
        RebuildBrain(genome, agent);
    }

    public void RebuildBrain(BrainGenome genome, Agent agent) 
    {
        //Debug.Log($"Rebuilding brain with {genome.inOutNeurons.Count} neurons in genome, " +
        //          $"and {genome.linkCount} links in genome");
    
        // Create Neurons:
        neurons = new List<Neuron>();
        //IDs = new Dictionary<NID, int>();
        
        RebuildNeurons(genome.inOutNeurons, agent);
        RebuildNeurons(genome.hiddenNeurons, agent);

        RebuildAxons(genome);
        //PrintBrain();
    }
    
    void RebuildAxons(BrainGenome genome)
    {
        axons = new List<Axon>();
        
        if (genome.linkCount <= 0)
        {
            Debug.LogError("Cannot rebuild axons because genome has zero links");
            return;
        }
        
        foreach (var link in genome.links) 
        {
            // find out neuronIDs:
            //if (!IDs.TryGetValue(new NID(link.fromModuleID, link.fromNeuronID), out int fromID)) {
            //    Debug.LogError("fromNID NOT FOUND " + link.fromModuleID + ", " + link.fromNeuronID);
            //}
            //if (!IDs.TryGetValue(new NID(link.toModuleID, link.toNeuronID), out int toID)) {
            //    Debug.LogError("toNID NOT FOUND " + link.fromModuleID + ", " + link.fromNeuronID);
            //}
            var from = GetMatchingNeuron(neurons, link.from);
            var to = GetMatchingNeuron(neurons, link.to);
            
            if (from == null || to == null)
                continue;

            //Debug.Log(fromID.ToString() + " --> " + toID.ToString() + ", " + genome.linkList[i].weight.ToString());
            //int fromID = IDs < new NID(genome.linkList[i].fromModuleID, genome.linkList[i].fromNeuronID) >;
            Axon axon = new Axon(from, to, link.weight);
            axons.Add(axon);
        }        
    }

    Neuron GetMatchingNeuron(List<Neuron> list, NeuronGenome genome)
    {
        foreach (var item in list)
            if (item.IsMe(genome))
                return item;
                
        return null;        
    }
    
    void RebuildNeurons(List<NeuronGenome> neuronGenomes, Agent agent)
    {
        foreach (var genome in neuronGenomes)
        {
            Neuron neuron = new Neuron(genome);
            agent.MapNeuronToModule(genome.data, neuron);
            neurons.Add(neuron);
        }
    }
    
    // *** WPP: reconsider approach
    /*void RebuildNeurons(List<Neuron> neurons, Agent agent)
    {
        foreach (var neuron in neurons)
        {
            agent.MapNeuronToModule(neuron.data, neuron);
            neurons.Add(neuron);
        }
    }*/

    // * WPP: deprecate
    /*void RebuildNeurons(List<NeuronGenome> neurons, Agent agent)
    {
        for (int i = 0; i < neurons.Count; i++) {
            Neuron neuron = new Neuron(neurons[i].name, neurons[i].nid.moduleID);
            agent.MapNeuronToModule(neurons[i].nid, neuron);
            IDs.Add(neurons[i].nid, i);
            neuronList.Add(neuron);
        }
    }*/

    public void ResetBrainState() {
        foreach (var neuron in neurons) {
            neuron.currentValue[0] = 0f;
            neuron.previousValue = 0f;
        }
    }

    /// Ticks state of brain forward 1 timestep
    public void BrainMasterFunction() 
    {
        // NAIVE APPROACH:
        // run through all links and save sum values in target neurons
        foreach (var axon in axons) {
            // Find input neuron, multiply its value by the axon weight, and add that to output neuron total:
            //neurons[axon.to].inputTotal += axon.weight * neurons[axon.from].currentValue[0];
            axon.to.inputTotal += axon.weight * axon.from.currentValue[0];
        }
        
        // Once all axons are calculated, process the neurons:
        foreach (var neuron in neurons) {
            neuron.previousValue = neuron.currentValue[0]; // Save previous state
            if (neuron.neuronType != NeuronType.In) {
                neuron.currentValue[0] = TransferFunctions.Evaluate(TransferFunctions.TransferFunction.RationalSigmoid, neuron.inputTotal);
            }
            // now zero out inputSum:
            neuron.inputTotal = 0f;
        }
    }


    public void PrintBrain() {
        Debug.Log("neuronCount: " + neurons.Count);
        string neuronText = "";
        for (int i = 0; i < neurons.Count; i++) {
            neuronText += "Neuron " + i + ": " + neurons[i].currentValue.Length + "\n";
        }
        string axonText = "";
        for (int j = 0; j < axons.Count; j++) {
            axonText += "Axon " + j + ": (" + axons[j].from.index + "," + axons[j].to.index + ") " + axons[j].weight + "\n";
        }
        Debug.Log(neuronText + "\n" + axonText);
    }
}
