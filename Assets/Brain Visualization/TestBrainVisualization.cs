using System.Collections.Generic;
using UnityEngine;

// Assumes full connectivity
public class TestBrainVisualization : MonoBehaviour
{
    [SerializeField] int neuronCount = 33;
    [SerializeField] int minInputs = 10;
    [SerializeField] int maxInputs = 20;
    [SerializeField] GenerateBrainVisualization brainVisualization;
    [SerializeField] float inputZ = -.9f;
    [SerializeField] float outputZ = .9f;
    
    const int MAX_AXON_COUNT = 270;
    
    List<Neuron> inputNeurons = new List<Neuron>();
    List<Neuron> outputNeurons = new List<Neuron>();

    void Start()
    {
        CreateBrain(Random.Range(minInputs, maxInputs));
    }

    public void CreateBrain(int inputCount) 
    {
        var neurons = CreateNeurons(inputCount);
        var axons = CreateAxons(inputCount);

        if (axons.Count <= 0)
        {
            Debug.LogError($"Invalid input count {inputCount}, cannot create brain." +
                           $"Input count must be greater than 0 and less than the number of neurons ({neuronCount})");
        }

        var sockets = CreateSockets(neurons);
        
        brainVisualization.Initialize(neurons, axons, ref sockets, inputNeurons.Count, outputNeurons.Count);
    }
    
    List<Neuron> CreateNeurons(int inputCount)
    {
        var neurons = new List<Neuron>();
        
        for (int i = 0; i < neuronCount; i++) 
        {
            Neuron neuron = new Neuron(i, inputCount);
            neurons.Add(neuron);
        }
        
        return neurons;
    }
    
    List<Axon> CreateAxons(int inputCount)
    {
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
        
        return axons;
    }

    SocketInitData[] CreateSockets(List<Neuron> neurons)
    {
        SocketInitData[] sockets = new SocketInitData[neuronCount];
        
        for (int i = 0; i < neuronCount; i++) {
            var list = neurons[i].neuronType == NeuronType.In ? inputNeurons : outputNeurons;
            list.Add(neurons[i]);
        }
        
        AssignNeuralPositions(ref sockets, inputNeurons.Count, 0, inputZ);
        AssignNeuralPositions(ref sockets, outputNeurons.Count, inputNeurons.Count, outputZ);
        
        return sockets;
    }
    
    // * WPP: expose magic numbers in inspector
    void AssignNeuralPositions(ref SocketInitData[] data, int count, int offsetIndex, float zPosition)
    {
        for(int i = 0; i < count; i++) {
            float x = 0.6f * (float)i / (float)count - 0.3f;
            int tier = Random.Range(0, 5);
            data[i + offsetIndex].pos = new Vector3(x, (float)(tier - 2) * 0.12f, zPosition);
        }
    }
}
