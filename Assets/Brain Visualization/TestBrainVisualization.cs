using System.Collections.Generic;
using UnityEngine;

// Assumes full connectivity
public class TestBrainVisualization : MonoBehaviour
{
    [SerializeField] int minInputs = 10;
    [SerializeField] int maxInputs = 20;
    [SerializeField] BrainSettings settings;
    [SerializeField] GenerateBrainVisualization brainVisualization;
    
    int neuronCount => settings.numNeurons;
    const int MAX_AXON_COUNT = 270;

    void Start()
    {
        CreateBrain(Random.Range(minInputs, maxInputs));
    }

    public void CreateBrain(int inputCount) 
    {
        var neurons = new List<Neuron>();
        
        for (int i = 0; i < neuronCount; i++) 
        {
            Neuron neuron = new Neuron(i, inputCount);
            neurons.Add(neuron);
        }
        
        var axons = new List<Axon>();
        for (int i = 0; i < inputCount; i++) 
        {
            for(int j = 0; j < neuronCount - inputCount; j++) 
            {
                if (j + i * inputCount < MAX_AXON_COUNT) 
                {
                    Axon axon = new Axon(i, inputCount + j, Random.Range(-1f, 1f));
                    axons.Add(axon);
                }
            }
        }
        
        if (axons.Count <= 0)
        {
            Debug.LogError($"Invalid input count {inputCount}, cannot create brain." +
                           $"Input count must be greater than 0 and less than the number of neurons ({neuronCount})");
            return;
        }
        
        brainVisualization.Initialize(neurons, axons);
    }
}
