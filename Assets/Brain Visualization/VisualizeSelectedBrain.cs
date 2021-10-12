﻿using System.Collections.Generic;
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
        
        Debug.Log($"Selected agent brain has {neurons.Count} neurons and {axons.Count} axons");
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
        
        // *** Success!!! Now use this to assign position (instead of above method based on IO type)
        //foreach (var neuron in neurons)
        //    Debug.Log(neuron.moduleID);

        return sockets;
        //return placement.AssignNeuralPositions(neurons);
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
