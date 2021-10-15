using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VisualizeSelectedBrain : MonoBehaviour
{
    UIManager ui => UIManager.instance;
    
    [SerializeField] GenerateBrainVisualization visualization;
    [SerializeField] PlaceNeuronsAtUI placement;

    void Start()
    {
        ui.OnAgentSelected += RefreshAgent;
    }

    void OnDestroy()
    {
        if (UIManager.exists) ui.OnAgentSelected -= RefreshAgent;
    }
    
    void RefreshAgent(Agent agent)
    {
        neurons = agent.brain.neurons;
        axons = agent.brain.axons;
        
        //Debug.Log($"Selected agent brain has {neurons.Count} neurons and {axons.Count} axons");
        var sockets = CreateSockets();
        visualization.Initialize(neurons, axons, ref sockets, inputNeurons.Count, outputNeurons.Count);
    }
    
    List<Neuron> neurons = new List<Neuron>();
    List<Neuron> inputNeurons = new List<Neuron>();
    List<Neuron> outputNeurons = new List<Neuron>();
    List<Axon> axons = new List<Axon>();
    
    SocketInitData[] CreateSockets()
    {
        SortNeuronsByIO(neurons);
        
        SocketInitData[] sockets = new SocketInitData[neurons.Count];
        
        for (int i = 0; i < neurons.Count; i++)
            sockets[i].pos = placement.GetNeuronPosition(neurons[i]);
        
        return sockets;
    }
    
    void SortNeuronsByIO(List<Neuron> neurons)
    {
        inputNeurons.Clear();
        outputNeurons.Clear();
    
        foreach (var neuron in neurons) 
        {
            var list = neuron.neuronType == NeuronType.In ? inputNeurons : outputNeurons;
            list.Add(neuron);
        }
    }
}
