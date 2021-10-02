using System.Collections.Generic;
using UnityEngine;

public class VisualizeSelectedBrain : MonoBehaviour
{
    UIManager ui => UIManager.instance;
    
    [SerializeField] GenerateBrainVisualization visualization;

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
        neurons = agent.brain.neuronList;
        axons = agent.brain.axonList;
    
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

        //Debug.Log($"Of {neurons.Count} neurons, {inputNeurons.Count} are input and {outputNeurons.Count} are output");
        AssignNeuralPositions(ref sockets, inputNeurons.Count, 0, 0f);
        AssignNeuralPositions(ref sockets, outputNeurons.Count, inputNeurons.Count, 0f);
        
        /*for (int i = 0; i < sockets.Length; i++)
        {
            var neuron = neurons[i];
            var IO = neuron.neuronType;
        }*/
        
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
    
    // * Modify so arranged in concentric circles
    void AssignNeuralPositions(ref SocketInitData[] data, int count, int offsetIndex, float zPosition)
    {
        for(int i = 0; i < count; i++) {
            float x = 0.6f * i / count - 0.3f;
            int tier = Random.Range(0, 5);
            data[i + offsetIndex].pos = new Vector3(x, (tier - 2) * 0.12f, zPosition);
        }
    }
}
