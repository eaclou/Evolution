using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Brain 
{
    public List<Neuron> neurons;
    public List<Axon> axons;

    public Brain(BrainGenome genome, Agent agent) {
       // Debug.Log("Brain constructed with " + genome.inOutNeurons.Count + " neurons.");
        RebuildBrain(genome, agent);
    }

    /// Create Neurons and Axons
    public void RebuildBrain(BrainGenome genome, Agent agent) 
    {
        //Debug.Log($"Rebuilding brain with {genome.inOutNeurons.Count} neurons in genome, " +
        //          $"and {genome.linkCount} links in genome");
        
        neurons = new List<Neuron>();
        
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
            var from = GetMatchingNeuron(neurons, link.from);
            var to = GetMatchingNeuron(neurons, link.to);
            
            if (from == null || to == null)
                continue;
            
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
    
    public void ResetBrainState() {
        foreach (var neuron in neurons) {
            neuron.currentValue[0] = 0f;
            neuron.previousValue = 0f;
        }
    }

    /// Ticks state of brain forward 1 time-step
    public void BrainMasterFunction() 
    {
        // NAIVE APPROACH:
        // run through all links and save sum values in target neurons
        foreach (var axon in axons) {
            float curVal = 0f;
            if (axon.from.currentValue != null) {
                curVal = axon.from.currentValue[0];
            }
            // Find input neuron, multiply its value by the axon weight, and add that to output neuron total:
            //neurons[axon.to].inputTotal += axon.weight * neurons[axon.from].currentValue[0];
            axon.to.inputTotal += axon.weight * curVal;
        }
        
        // Once all axons are calculated, process the neurons:
        foreach (var neuron in neurons) {
            if (neuron.currentValue == null) neuron.currentValue = new float[1]; // null reference error fix
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
    
    public List<Neuron> GetNeuronsByTechElement(TechElement tech)
    {
        var result = new List<Neuron>();

        foreach (var neuron in neurons)
            if (tech.ContainsNeuron(neuron.data))
                result.Add(neuron);
                
        return result;
    }
}
