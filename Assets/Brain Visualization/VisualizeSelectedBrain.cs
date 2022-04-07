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
        this.agent = agent;
        //neurons = agent.brain.allNeurons;
        //axons = agent.brain.allAxons;
        
        //Debug.Log($"Selected agent brain has {neurons.Count} neurons and {axons.Count} axons");
        var sockets = CreateSockets();
        /*
        string neuronString = "RefreshAgentneuronData: [" + neurons.Count + "] ";
        foreach(var socket in sockets) {
            neuronString += socket.position + ", ";
        }
        Debug.Log(neuronString);
        */
        visualization.Initialize(neurons, axons, ref sockets, inputNeuronCount, outputNeuronCount);
    }
    
    Agent agent;
    List<Neuron> neurons => agent.brain.allNeurons;
    int inputNeuronCount => agent.brain.inputNeurons.Count;
    int outputNeuronCount => agent.brain.genome.neurons.output.Count;
    List<Axon> axons => agent.brain.allAxons;
    
    SocketInitData[] CreateSockets()
    {
        //SortNeuronsByIO(neurons); // WPP: removed
        SocketInitData[] sockets = new SocketInitData[neurons.Count];

        for (int i = 0; i < sockets.Length; i++)
            sockets[i].position = neurons[i].data.iconPosition;

        return sockets;
    }
    
    // WPP: redundant because sorted in agent brain
    /*void SortNeuronsByIO(List<Neuron> neurons)
    {
        inputNeurons.Clear();
        outputNeurons.Clear();
        hiddenNeurons.Clear();
    
        foreach (var neuron in neurons) 
        {
            switch (neuron.data.io)
            {
                case NeuronType.In: inputNeurons.Add(neuron); break;
                case NeuronType.Out: outputNeurons.Add(neuron); break;
                case NeuronType.Hidden: hiddenNeurons.Add(neuron); break;
            }
        }
    }*/
}
