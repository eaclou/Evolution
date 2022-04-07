using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Brain 
{
    public List<Neuron> allNeurons => genome.neurons.all;
    public List<Neuron> inputNeurons => genome.neurons.input;
    public List<Axon> allAxons => genome.axons.all;
    
    public BrainGenome genome;

    public Brain(BrainGenome genome, Agent agent) {
       // Debug.Log("Brain constructed with " + genome.inOutNeurons.Count + " neurons.");
        this.genome = genome;
        //RebuildBrain(genome, agent);
    }

    /// Create Neurons and Axons
   /*public void RebuildBrain(BrainGenome genome, Agent agent) 
    {
        //Debug.Log($"Rebuilding brain with {genome.inOutNeurons.Count} neurons in genome, " +
        //          $"and {genome.linkCount} links in genome");
        
        this.genome = genome;
        //neurons = new List<Neuron>();
        
        //RebuildNeurons(genome.inOutNeurons, agent);
        //RebuildNeurons(genome.hiddenNeurons, agent);

        //RebuildAxons(genome);
        //PrintBrain();
    }*/
    
    // WPP: get data from BrainGenome, ensuring lists are in sync
    /*void RebuildAxons(BrainGenome genome)
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

    Neuron GetMatchingNeuron(List<Neuron> list, Neuron genome)
    {
        foreach (var item in list)
            if (item.IsMe(genome))
                return item;
                
        return null;        
    }
    
    void RebuildNeurons(List<Neuron> neuronGenomes, Agent agent)
    {
        foreach (var genome in neuronGenomes)
        {
            Neuron neuron = new Neuron(genome);
            agent.MapNeuronToModule(genome.template, neuron);
            neurons.Add(neuron);
        }
    }*/
    
    public void ResetBrainState() {
        foreach (var neuron in allNeurons) {
            neuron.currentValue = 0f;
        }
    }

    /// Ticks state of brain forward 1 time-step
    /// NAIVE APPROACH
    public void TickAxons() 
    {
        ValidateAxons();
    
        // Clear all neurons' input signals
        foreach (var neuron in allNeurons)
            neuron.inputTotal = 0f;        
    
        // Find input neuron, multiply its value by the axon's weight, and add that to output neuron's total
        foreach (var axon in allAxons)
            axon.to.inputTotal += axon.weight * axon.from.currentValue;

        // Process the output & hidden neurons' input signals
        foreach (var neuron in allNeurons)
            if (neuron.io != NeuronType.In) 
                neuron.currentValue = TransferFunctions.Evaluate(TransferFunctions.TransferFunction.RationalSigmoid, neuron.inputTotal);
    }
    
    #region Hackfix for mismatch between axon connection targets and actual neurons
    void ValidateAxons()
    {
        foreach (var axon in allAxons)
            if (!allNeurons.Contains(axon.from))
                axon.from = FindNeuron(axon.from);
                
        foreach (var axon in allAxons)
            if (!allNeurons.Contains(axon.to))
                axon.to = FindNeuron(axon.to);
    }
    
    Neuron FindNeuron(Neuron invalidNeuron)
    {
        foreach (var neuron in allNeurons)
            if (neuron.index == invalidNeuron.index)
                return neuron;
                
        return invalidNeuron;
    }
    #endregion
    
    public List<Neuron> GetNeuronsByTechElement(TechElement tech)
    {
        var result = new List<Neuron>();

        foreach (var neuron in allNeurons)
            if (tech.ContainsNeuron(neuron.data.template))
                result.Add(neuron);
                
        return result;
    }
    
    #region Debug
    
    public void PrintBrain() {
        Debug.Log("neuronCount: " + allNeurons.Count);
        string neuronText = "";
        for (int i = 0; i < allNeurons.Count; i++) {
            neuronText += "Neuron " + i + ": " + allNeurons[i].currentValues.Length + "\n";
        }
        string axonText = "";
        for (int j = 0; j < allAxons.Count; j++) {
            axonText += "Axon " + j + ": (" + allAxons[j].from.index + "," + allAxons[j].to.index + ") " + allAxons[j].weight + "\n";
        }
        Debug.Log(neuronText + "\n" + axonText);
    }
    
    #endregion
}
