using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Brain 
{
    public List<Neuron> allNeurons => genome.neurons.all;
    public List<Neuron> inputNeurons => genome.neurons.input;
    public List<Neuron> outputNeurons => genome.neurons.output;
    public List<Axon> allAxons => genome.axons.all;
    
    public BrainGenome genome;

    public Brain(BrainGenome genome) {
        this.genome = genome;
        //RebuildBrain(genome, agent);
        //genome.ValidateAxons();
    }

    public void ResetBrainState() {
        foreach (var neuron in allNeurons) {
            neuron.currentValue = 0f;
        }
    }

    /// Ticks state of brain forward 1 time-step
    /// NAIVE APPROACH
    public void TickAxons() 
    {
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